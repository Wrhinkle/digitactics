# Dota Auto Chess (Drodo Studio, Dota 2 custom game) reference data

Compiled 2026-10-15 as design inspiration for Digi-Tactics. "unknown" means no source was found. Numbers come from the game's own data files where possible.

## Sources

- S1. Steam Workshop change notes for workshop item 1613886175, 1,076 entries, 2019-01-04 to 2026-10-12: https://steamcommunity.com/sharedfiles/filedetails/changelog/1613886175
- S2. Drodo Google Docs patch notes linked from S1 (Feb 2021 onward), for example https://docs.google.com/document/d/e/2PACX-1vQ2Yw90G2JdpNMrSGVQOX8y-CC_HK0AoICuxsSxLApu7s3O93N78OXEBpKM0A0kEa3HyL1LGWqjXAGP/pub
- S3. Unpacked game files (Lua rules, npc_units_custom, npc_abilities_custom, npc_items_custom, recipes, localization) at 77 points in time, 2019-02-12 to 2023-04-10: https://github.com/zyi1992/autochess_vpk . Snapshots used: 2019-02-13 (cd554ade29), 2019-06-05 (9fde21df7c), 2019-06-26 (f2f5706156), 2019-12-25 (077c127507), 2020-07-02 (4681f0edf0), 2021-02-07 (f27e499aab), 2022-01-19 (708352019a), 2023-04-10 (28154722e9). File URL pattern: https://raw.githubusercontent.com/zyi1992/autochess_vpk/<sha>/<path>
- S4. Wikipedia: https://en.wikipedia.org/wiki/Dota_Auto_Chess
- S5. Dota Auto Chess fandom wiki (reflects about June 2019, plus three 2022-2023 patch entries): https://dotaautochess.fandom.com/wiki/Chess_pieces , https://dotaautochess.fandom.com/wiki/Items , https://dotaautochess.fandom.com/wiki/Patch_History

Where code and tooltip text disagree, the code is treated as correct; English tooltips in S3 often lag behind the code. Liquipedia covers only tournaments. Reddit and the Wayback Machine returned nothing usable.

Companion file: `dota-auto-chess.json` holds the same data in structured form, including stats and ability numbers for every piece in five code snapshots.

## Units and conventions

- Ranges are Dota units. Melee is 205; ranged pieces use 400 or 600. Tooltips describe roughly 200 units as "1 grid".
- Attack interval is the Dota AttackRate, in seconds per attack before attack-speed bonuses.
- Stars: 3 copies of a piece combine into the next star, up to 3 stars. HP and damage double per star. Unit level = cost + 2 x (star - 1); it drives sell price and player damage.
- Mana: most pieces have 100 max mana and cast at full mana. The ability "cooldown" is a minimum recast time. Some pieces have 0 mana and a passive (e.g. Anti-Mage). Mana gain per attack and per hit is on the wiki Mana page (S5) and was not re-checked against code.

## Balance eras

### Which version the fandom wiki reflects (S5)

The wiki dump matches June 2019.

- The Patch History page has entries up to 2019-06-06, then jumps to Dec 2022 to Feb 2023 (three entries), so mid-2019 to late-2022 is missing from it.
- The Chess pieces page contains Winter Wyvern ($1, added 2019-06-01) and Grimstroke ($4 Demon Wizard, added 2019-06-26). It lacks Rubick ($2 God Wizard, added the same day 2019-06-26) and Visage/Pudge/Lion (2019-07-30). It lists Mirana as $2, and Mirana was removed 2019-06-26 (S1).
- The Items page lacks Summoning Stone (added 2019-07-13) and Pipe of Insight (2019-07-18) (S1).
- Its shop odds, pool sizes (45/30/25/15/10) and drop table match the S3 snapshots 2019-06-05 and 2019-06-26 exactly.
- The closest code snapshot is 2019-06-26. One difference: in code, Alchemist is Goblin/Ogre/Warlock (S3 2019-06-05 and 2019-06-26 `npc_units_custom.txt`, tags `is_goblin is_ogre is_warlock`). The wiki lists Goblin/Warlock.
- The Gold page describes interest capped at 4 and streak tiers 3-5/6-8/9+. Neither matches mid-2019 code (cap 5; tiers 3-4/5-6/7+) or the post-March-2021 rules (cap 4; tiers 2-4/5-9/10+). The page mixes eras (see Economy).

### Timeline

Dates are UTC from S1 unless noted. "Removed" usually means "temporarily removed for rework". Pieces often returned later, sometimes at a new cost.

**Pre-workshop / launch**
- Mod first available November 2018. Relaunched as workshop item 1613886175, first change note "Created DOTA AUTO CHESS" on 2019-01-04 (S1; S4 gives Jan 4, 2019 release).
- Standalone spin-offs are separate games and are not covered by the S3 data: Auto Chess (Dragonest), Android 2019-04-18, iOS 2019-05-22, Epic/Windows 2019-07-19, PS4 2020-12-16. Valve's Dota Underlords is also separate (S4). S1 2019-06-14 mentions TI9 Battle Pass owners being invited to Underlords.

**Era 1: launch balance, Jan to Mar 2019**
- 2019-01-10: Furion ($2), Lone Druid ($4); Druid (4) and Beast (6) added.
- 2019-01-13: odds for levels 6-10 changed; "25% amount of $3 chess added in chess pool".
- 2019-01-25: Morphling (Elemental/Assassin), Terrorblade (Demon/Demon Hunter), Tiny (Elemental/Warrior). Demon combo became 50% extra pure damage. Demon Hunter (2) added.
- 2019-02-02 to 02-03: Candy Store couriers and effects.
- 2019-02-15: Blink Dagger.
- 2019-02-25: Riki ($3 Satyr/Assassin), Mirana ($3 Elf/Hunter), Death Prophet ($5 Undead/Warlock); Satyr species; (9) Elf; (9) Assassin.
- 2019-03-04: Battle Fury.
- 2019-03-29: God species with Zeus and Mars. Riki, Sand King and Slark removed, which removed (9) Assassin, (4) Naga and (6) Beast. Fatigue after round 50 changed to 50% of current HP per round. Mirana $3 to $2, Treant $2 to $3.
- Code snapshot 2019-02-13: 55 pieces in pool (12/13/14/11/5 by cost).

**Era 2: wiki era, Apr to Jun 2019**
- 2019-04-05: half of an eliminated player's items drop to other players.
- 2019-04-29/30: Dazzle ($3 Troll/Priest), Io ($5 Elf, no class); Season S0 rewards; seasons start 2019-05-01.
- 2019-05-09: Ogre Axe, Black King Bar.
- 2019-05-25: Sven ($5 Demon/Warrior), Lich ($2 Undead/Mage); reroll can no longer show the unbought pieces from the previous roll.
- 2019-06-01: Winter Wyvern ($1 Dragon/Mage); mango/tango compensation after 5 empty creep kills.
- 2019-05-31/06-04: DOTA AUTO CHESS Custom Game Pass (the "DAC Pass").
- 2019-06-26: Wizard class with Rubick ($2 God/Wizard) and Grimstroke ($4 Demon/Wizard). New maps: 1x8 Ranked, 1x8 Casual, 2x4 Casual. 2x4 is the duo mode: teammates share courier HP and use a "Post Piece" courier ability. Mirana removed.
- Code snapshot 2019-06-26: 60 pieces (13/14/14/12/7).

**Era 3: season system, loot chests, new races, Jul 2019 to Apr 2020**
- 2019-07-13: Loot Treasure after creep rounds 10/20/30/40/50. 2x4 Ranked mode. Summoning Stone.
- 2019-07-30: Visage ($4 Undead/Dragon/Hunter), Pudge ($3 Undead/Warrior), Lion ($1 Demon/Wizard); Lich removed.
- 2019-08-23: Boots of Speed, Energy Booster, Tranquil Boots, Arcane Boots.
- 2019-08-30: Oracle ($2 God/Priest); Aqir species (Infection); Sand King returned; Slardar and QoP removed.
- 2019-09-06: win-streak terminator bonus of $N for ending an N>=5 streak. Poor Man's Shield.
- 2019-09-26: Ogre Magi ($2) returned; Nyx Assassin ($4 Aqir/Assassin); Ogre trait reworked to Iron Fist.
- 2019-10-20: Bloodseeker ($2 Orc/Shaman), Huskar ($5 Troll/Warrior).
- 2019-10-31: new drop system. Loot Chest after every loot round; higher tiers only from chests after round 25; first 3 creep rounds stop dropping items; creeps from round 10 drop only low/mid tiers. Season S4 "Pumpkin" (01/11 to 31/12).
- 2019-11-19: Candy Store expanded; courier XP levels.
- 2019-11-27: Snapfire ($5 Goblin/Knight).
- 2019-12-11: Rubick $2 to $3, Lion $1 to $2, Crystal Maiden $2 to $1.
- 2019-12-24: Broodmother ($4 Aqir/Hunter); (6) Mech Golden Heart of the Machine (free roll of five $5 pieces).
- 2020-01-15: Invoker ($5 Elf/Mage); Assassin Backstab Leap.
- 2020-02-19: Legion Commander ($3 Human/Knight); Human trait changed from Censorship (silence) to Spark of Mind (Tome of Knowledge XP).
- 2020-03-17: Chen ($4 Orc/Priest); Hand of Midas, Force Staff, Ogre Hood; Slardar $2 to $3, Sniper $3 to $2, Abaddon $3 to $2; Tango removed from non-drop compensation.
- 2020-04-09: Jakiro ($5 Dragon/Mage), Dark Willow ($1 Elf/Wizard); Puck and Lion removed.
- Code snapshot 2019-12-25: 71 pieces (14/16/16/13/12). Pool copies changed to 20/20/15/15/10 (see Economy).

**Era 4: "Land of Mists" economy overhaul, May 2020 to Feb 2021**
- 2020-05-27 (S1, five entries): Pandaren species and Monk class; Brewmaster ($2 Pandaren/Monk); Storm Spirit and Ember Spirit as Pandaren recluse pieces. New courier ability Dismiss (ban a species/class from your own rolls, cost $2 x pieces in category; cut to 1.5x the same day). Deathblow gold. Loot-round gold of floor(creep level/3)+1 per creep (S1 wording: "one-thirds of the Creep's Level plus 1"). Creep rounds no longer damage the courier. Buy XP cost $5 to $4 with a larger XP table. Dazzle $3 to $2. All 3-star pieces +10 base mana regen.
- Code confirms base income changed from min(round,5) to min(round(round/2),5) in the commit dated 2020-05-27 (S3 commit a4986e37c0 vs b9720b4b40).
- 2020-06-29: Monkey King ($5 Beast/Monk), Kunkka ($4) reworked, Earth Spirit ($4 Pandaren/Shaman, recluse only); Vesture of the Tyrant, Manta Style, Linken's Sphere.
- 2020-08-25: Tauren species (Totem of the Clan); Spirit Breaker ($1 Tauren/Assassin), Earthshaker ($4 Tauren/Shaman); Templar Assassin $4 to $5; Death Prophet removed.
- 2020-09-29: Kobold species (Divided We Stand); Meepo ($3 Kobold/Inventor); Luna returned at $1; double-click Buy XP to level instantly.
- 2020-10-27: Elder Titan ($5 Tauren/God/Druid); loot rounds no longer break streaks.
- 2020-11-25: Faceless (N'raqi) species, Whisper of Darkness; Faceless Void ($3 Faceless/Assassin); Phantom Assassin removed.
- 2020-12-27: Pangolier ($4 Beast/Assassin); Crimson Guard.
- 2021-02-05: Vengeful Spirit ($1 Undead/Demon Hunter), Warlock ($5 Orc/Warlock); Winter Wyvern and Sven removed.
- Code snapshots 2020-07-02 (76 pieces, 15/16/16/15/14) and 2021-02-07 (79, 17/17/16/14/15). Max courier level is 16 (`SetCustomHeroMaxLevel(16)`).

**Era 5: relics, Mar 2021 to 2022**
- 2021-03-25 "Dragon's Relic Treasure" (S2): Relic Box replaces the loot box in rounds 10/20/30/40/50 (choose 1 of 3 relics; one relic held at a time; it expires at the next relic round). Max interest 5 to 4. New streak table. New item drop rules.
- 2021-05-31: Riki ($2 Satyr/Assassin), Satyr returned; Necronomicon, Spider Legs, Book of the Dead.
- 2021-06-24: Hoodwink ($1 Beast/Hunter). 2021-08-26: Arc Warden ($5 Kobold/Shaman).
- 2021-10-25: Spectre ($5 Undead/Demon Hunter). 2021-11-30: Marci ($3 Elf/Monk).
- 2021-12-26: class passives (Monk, Knight, Warrior, Mage, Warlock, Priest, Shaman) (S2 `2021-12-26_EoS_Rewards_and_Gameplay_Patch.txt`).
- 2022: Naga Siren ($2 Naga/Wizard, 01-24); Aeon Disk and Mirror Shield (02-24); Primal Beast ($5 Beast/Warrior, 03-23); Void Spirit ($3 Pandaren/Monk recluse, 04-25); ranked maps split by rank (Bishop+/Rook+/King+, 05-05); Dragon Lance, Trickster Cloak (06-26); Undying (07-25; S1 says $2, S2 says $3); Pangolier removed and six new items (08-23); Centaur species with Centaur Warrunner ($2) and Magnus ($4 Centaur/Assassin) (10-24); Lucky Pendant, Repair Tools, Pandaren rework (12-24).
- Code snapshot 2022-01-19: 79 pieces (17/17/16/15/14). Base income cap is now 4 (the change falls between commits 2021-08-11 and 2022-01-19; no patch note found).

**Era 6: 2023, end of the S3 data**
- 2023-01-19: Candy Contest (Beta) mode on map candy_5_1x8.
- 2023-02-26: Leshrac ($5 God/Centaur/Druid); Aghanim's Shard; God (6) and Dragon (5) reworks; relic Malefic Rapture.
- 2023-03-25: Muerta ($3 Undead/Hunter); Necrophos $4 to $5; Pudge $3 to $2.
- 2023-04 to 05: Dota 2 patches 7.33/7.33b/c broke the map; ranked was disabled for a time.
- 2023-08-07: Candy Store shut down.
- Code snapshot 2023-04-10 (last in S3): 88 pieces (19/19/17/15/18).

**Era 7: 2024 to 2026 (patch notes only, no code dump)**
- 2024-01-29: Draenei race (Blessing of the Naaru); Dark Seer ($2), Dawnbreaker ($3).
- 2024-02-28: Anti-Mage $1 to $4.
- 2024-04-27: Shadow Demon ($3 Demon/Draenei/Priest); Lion $1 to $4; Slardar $3 to $2; Slark $2 to $1; Grimstroke $4 to $1.
- 2024-05-26: Wraith King, Clinkz, Visage removed.
- 2024-08-26: Harpy race (Vengeful Spirit, Skywrath Mage).
- 2024-10-24: Bane ($1 Satyr/Warlock).
- 2025: Skywrath $2 to $1, Luna $1 to $2 (01-26); Kez ($3 Harpy/Assassin), PA $3 to $4 (03-27); Lich $5 to $4 (04-27); Lifestealer ($2 Kobold/Undead/Wizard), Abaddon $2 to $1 (09-17); Abyssal Underlord ($5 Demon/Warlock) (10-15); Morphling $2 to $5 and gains Elf, Ogre Magi $1 to $3, Meepo $3 to $2 (11-27); Largo ($2 Naga/Monk), Brewmaster $2 to $1 (12-20).
- 2026: Ancient Apparition ($1 Elemental/Mage), Phoenix ($4 Elemental/Mage) (03-16). Arcade server problems: games hosted locally and ranked closed (09-10); local-host ranked beta (09-16); on 10-11/12 games could not launch inside Dota 2, and players were directed to the Xiaoheihe client.

### Races and classes: which ones exist in DAC

Verified in S1/S3. Species: Orc, Beast, Undead, Goblin, Troll, Elf, Human, Elemental, Demon, Naga, Dwarf, Ogre, Dragon, Satyr, God (2019-03-29), Aqir (2019-08-30), Pandaren (2020-05-27), Tauren (2020-08-25), Kobold (2020-09-29), Faceless/N'raqi (2020-11-25), Centaur (2022-10-24), Draenei (2024-01-29), Harpy (2024-08-26). Classes: Warrior, Mage, Hunter, Assassin, Knight, Druid, Mech (renamed Inventor in later text), Shaman, Warlock, Demon Hunter, Priest (2019-04-29), Wizard (2019-06-26), Monk (2020-05-27).

"Kira", "Insectoid", "Spirit" and "Pandaren as a mobile race" are not in DAC data. No DAC change note or code table uses them; they appear to come from the Dragonest mobile Auto Chess, which these sources do not cover. Aqir is DAC's insect race. Spirit-named heroes (Storm/Ember/Earth/Void Spirit) are Pandaren recluse pieces. "Knight" exists as a class and as a rank tier (Pawn, Knight, Bishop, Rook, King, Queen per S4). No "Knight mode" was found.

## Economy and rules

All formulas are from `addon_game_mode.lua` in S3 unless marked. Gold is stored as courier mana, so gold is capped at 100 (`AddMana` clamps to 100 in every snapshot; courier `StatusMana` is 100). Courier HP is 100 (`npc_heroes_custom.txt`, `StatusHealth 100`).

### Round income

| Component | 2019-02 to 2020-05 | 2020-05-27 to 2021-03 | 2021-03-25 onward | ~late 2021/2022 onward |
|---|---|---|---|---|
| Base | min(round, 5) | min(floor(round/2+0.5), 5) | same | min(floor(round/2+0.5), 4) |
| Interest | min(floor(gold/10), 5) | same | min(floor(gold/10), 4) | same |
| Win streak | 3-4: +1, 5-6: +2, 7+: +3 | same | 2-4: +1, 5-9: +2, 10+: +3 | same |
| Lose streak | same as win | same | same as win | same |
| Win a fight | +1 | +1 | +1 | +1 |

- Streak formula before 2021-03: `floor((streak+3)/2)-2`, clamped 0..3. After: explicit thresholds 2/5/10. Code 2022-01-19 and 2023-04-10. S2 `2021-03-25_Dragon_s_Relic_Treasure_Update.txt` has both tables.
- Interest is computed on gold held at payout time, before adding the round's income.
- Relic "The Great Vault": interest above 40 gold. Changes: max interest 6 (2021-03-25), 7 (2021-03-28), +2 per extra 10 gold above 40 (2021-04-10), 4 per 20 (2023-05-21) (S1, S2; code 2023: `4 + 4*floor((gold-40)/20)`).
- Relic "Chiaroscuro Certainty": doubles the streak bonus (S2).
- Win-streak terminator bonus of $N for ending an N>=5 streak, from 2019-09-06 (S1).
- Deathblow gold and loot-round creep gold from 2020-05-27 (S1).
- An eliminated player's unspent gold is partly redistributed from 2020-03-17 (S1).

### XP and levels

- +1 XP per round, except round 1. Code (all snapshots): `AddExperience(1)` when `battle_round ~= 1`.
- Buy XP: 4 XP for 5 gold until 2020-05-27, then 4 XP for 4 gold. Code: `exp_book` `AbilityManaCost` 5 in 2019-02-13, 2019-06-05 and 2019-12-25; 4 from 2020-07-02; `OnExpBook` cost 4 in 2023. Patch note 2020-05-27 (S1).
- From 2020-09-29, double-clicking Buy XP levels up in one action (S1). Code 2023: cost `ceil((xp_needed)/4)*4 - 4`, after the first 4.

Cumulative XP to reach each level (`SetCustomXPRequiredToReachNextLevel`):

| Level | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 2019-02 to 2019-12 (max 10) | 1 | 2 | 4 | 8 | 16 | 32 | 56 | 88 | 128 | - | - | - | - | - | - |
| 2020-07 to 2023-04 (max 16) | 1 | 2 | 6 | 14 | 30 | 62 | 110 | 166 | 230 | 294 | 358 | 422 | 486 | 550 | 614 |

Commented-out older values in the 2019 code: L8 64, L9 112, L10 176. The 2020-05-27 rebalance note says L8 to L9 needs 56 XP (was 64) and L9 to L10 needs 64 XP (was 80), which matches the deltas 166-110 and 230-166 (S1). Buy XP cannot push past the level-10 total (`if xpadd + xpnow > HeroExpTable[10]`, code 2021/2022). Levels above 10 come from other effects; the exact sources are unknown.

### Shop

- 5 slots per roll (`CHESS_POOL_SIZE`/draw). Reroll costs 2 gold (code `CostMana(hero,2)`, 2019 and 2023).
- From 2019-05-25, a manual reroll excludes the previous unbought pieces (S1; wiki "Blacklisting").
- Lock affects one round (S1 2019-04-13).
- Io: 30/10000 per slot. SSR: both rolls = 1 at courier level >= 7 (code 2019-06-05 `RandomDrawChessNew`; matches wiki).
- Dismiss (2020-05-27): pay to remove a species/class from your own rolls (S1).

Shop odds by courier level (%; roll 1-100, cost bands from `chess_gailv`):

| Lvl | 2019-01 to 2021 ($1/$2/$3/$4/$5) | 2022-01-19 | 2023-04-10 |
|---|---|---|---|
| 1 | 100/0/0/0/0 | same | same |
| 2 | 70/30/0/0/0 | same | same |
| 3 | 60/35/5/0/0 | same | same |
| 4 | 50/35/15/0/0 | same | 45/40/15/0/0 |
| 5 | 40/35/23/2/0 | same | 35/40/23/2/0 |
| 6 | 33/30/30/7/0 | same | 30/33/30/7/0 |
| 7 | 30/30/30/10/0 | same | same |
| 8 | 24/30/30/15/1 | 24/30/28/17/1 | 24/30/28/17/1 |
| 9 | 22/30/25/20/3 | 22/25/25/25/3 | 22/25/25/25/3 |
| 10+ | 19/25/25/25/6 | 20/22/25/28/5 | 20/22/25/28/5 |

Odds for levels 11-16 equal level 10.

Pool copies per piece (`CHESS_INIT_COUNT` x `CHESS_POOL_SIZE`):

| Snapshot | $1 | $2 | $3 | $4 | $5 |
|---|---|---|---|---|---|
| 2019-02-13, 2019-06-05, 2019-06-26 | 45 | 30 | 25 | 15 | 10 |
| 2019-12-25 to 2023-04-10 | 20 | 20 | 15 | 15 | 10 |

The pool counts can be overridden by a server config (`t.chess_pool.chess_init_1..5`, code 2023). Only a random subset of the $5 pieces is active per game (`RandomLegendary()` in code 2023). S1 shows a viewable "legendary pool" on 2021-07-26, and on 2023-02-26 the per-game legendary pool grew from 8 to 10 pieces.The date the subset rule started is unknown. Sold pieces return to the pool: 1/3/9 copies for 1/2/3 stars (wiki).

### Selling

Refund = unit level, where unit level = cost + 2 x (stars - 1). For example, a 2-star $1 piece (level 3) sells for 3. Code 2019 `AddMana(caster, target:GetLevel())`; 2023 `price = target:GetLevel()` unless `buy_price` is set (Family Discount relic). Unit levels confirmed in `npc_units_custom.txt`: chess_axe Level 1/3/5; chess_gyro 5/7/9.

### Rounds and creeps

- PvE rounds 1, 2, 3, 10, 15, 20, 25, 30, 35, 40, 45, 50 (`battle_boss` table, 2019 and 2023).
- 2023 changes: round 20 adds rabbits, round 30 uses "pve_pine_cone" instead of Thunderhides, waves have extra slots.
- Creep rounds stopped dealing courier damage on 2020-05-27 (S1).
- Timers: preparation 35 s, battle 60 s (2019); battle 50 s (from 2019-12); preparation 40 s (2023).
- Fatigue: after round 50, each round the courier loses floor(current HP / 2) (code `pilao_round = 50`, from 2019-03-29 per S1). When 2 players remain, the fatigue round resets to current round + 6 (code 2019-06-05 "grand final").

### Player (courier) damage

- On a loss, each surviving enemy piece deals floor(1 + unit_level/3) (`GetHitDamage`, unchanged 2019-02 to 2023-04).
  - 1-star $1 or $2: 1. 1-star $3 to $5: 2. 2-star $4 (level 6): 3. 3-star $5 (level 9): 4.
- A surviving Zeus instead deals (10 x Thundergod level + 5)% of the courier's current HP (code 2019-06-05).
- Priest buff (2019): total x 0.8, rounded, minimum 1.
- Starting HP 100.

### Item drops

Drop chance by creep level (`drop_item_gailv`; roll 1-100, tier = band containing the roll):

| Creep lvl | 2019 (T1/T2/T3/T4) | 2019-12 to 2021 | 2022-01 / 2023-04 |
|---|---|---|---|
| 1 | 21/0/0/0 | none | none |
| 2 | 41/0/0/0 | none | none |
| 3 | 51/0/0/0 | none | none |
| 4 | 40/21/0/0 | T1 51 | T1 26 |
| 5 | 20/41/0/0 | T1 30, T2 21 | T1 30, T2 21 |
| 6 | 30/30/11/0 | same as lvl 5 | T1 30, T2 31 |
| 7 | 30/30/21/0 | same | T1 30, T2 41 |
| 8 | 19/40/30/11 | same | T1 30, T2 51 |
| 9 | 9/40/30/21 | same | T1 30, T2 71 |

Percentages are derived from the thresholds. For example, `{[80]=1}` gives T1 on rolls 80-100, which is 21%; the wiki rounds this to 20%.

- Since 2019-10-31, higher tiers come from loot chests (S1).
- Pity: 5 empty kills give a Tango or Mango (2019-06-01; Tango removed 2020-03-17).
- Later code adds an accumulating bonus `undrop_item_count_total` to the roll (2023).
- Units of level <= 2 and summoned units drop nothing (2023).

## Items

Items drop from creeps (see Item drops above) and combine automatically when all components sit on the courier or on one piece. From 2021-03-25, relics are a separate system (below).

### Item list, code snapshot 2019-06-26 (51 entries)

| Name | Tier / source | Recipe | Effect (English tooltip) |
|---|---|---|---|
| Chainmail | drop T1 |  | +5 Armor |
| Stout Shield | drop T1 |  | Blocks 10 damage |
| Ring of Health | drop T1 |  | +10 HP regeneration |
| Blades of Attack | drop T1 |  | +10 Attack damage |
| Blight Stone | drop T1 |  | 3 Armor reduction to the target. |
| Quarterstaff | drop T1 |  | +5 Attack damage, +10 Attack speed |
| Morbid Mask | drop T1 |  | +5% Lifesteal |
| Ring of Regeneration | drop T1 |  | +5 HP regeneration |
| Cloak | drop T1 |  | +15% Magic resistance |
| Void Stone | drop T1 |  | +100% mana gained by dealing damage |
| Robe of the Magi | drop T1 |  | -30% magic resistance to target |
| Crown | drop T1 |  | +50% Mana gained by dealing damage |
| Platemail | drop T2 |  | +10 Armor |
| Vitality Booster | drop T2 |  | +250 Max HP |
| Broad Sword | drop T2 |  | +20 Attack damage |
| Mithril Hammer | drop T2 |  | +15 Attack damage |
| Javelin | drop T2 |  | +15 Attack damage |
| Staff of Wizardry | drop T2 |  | -30% magic resistance to target |
| Ogre Axe | drop T2 |  | +15% Max HP |
| Demon Edge | drop T3 |  | +30 Attack damage |
| Hyperstone | drop T3 |  | +30 Attack speed |
| Ultimate Orb | drop T3 |  | +250 Max HP\n+25% mana gained through taking damage |
| Blink Dagger | drop T3 |  | Instantly teleports to the farthest vacant square in this column. |
| Sacred Relic | drop T4 |  | +50 Attack damage |
| Reaver | drop T4 |  | +500 Max HP, +10 HP regeneration |
| Mystic Staff | drop T4 |  | -40% Magic resistance to the target\n+100% mana gained by dealing damage |
| Desolator | combined (tier not listed) | Mithril Hammer + Mithril Hammer + Blight Stone | +30 Attack damage\n15 Armor reduction to the target |
| Black King Bar | combined (tier not listed) | Ogre Axe + Mithril Hammer | +15 Attack damage, +15% Max HP.\nActive: Grants spell immunity and 100% magic resistance for 8 sec. |
| Mjollnir | combined (tier not listed) | Maelstrom + Hyperstone | +50 Attack damage, +30 Attack speed\n25% chance on attack to release a bolt of electricity that leaps between 5 targets, dealing 200 damage to each. |
| Daedalus | combined (tier not listed) | Crystalys + Demon Edge | +40 Attack damage\n10% chance to critical hit for 400% damage. |
| Mask of Madness | combined (tier not listed) | Morbid Mask + Quarterstaff | +10% Lifesteal\n+30 Attack speed\nSilences the bearer. |
| Dagon 1 | combined (tier not listed) | Staff of Wizardry + Crown | -30% Magic resistance to the target, +50% Mana gained by dealing damage.&nbsp;\nAutocast: Deal 400 magical damage to a random enemy. |
| Dagon 2 | combined (tier not listed) | Staff of Wizardry + Dagon 1 | -60% Magic resistance to the target, +50% Mana gained by dealing damage.&nbsp;\nAutocast: Deal 500 magical damage to a random enemy. |
| Dagon 3 | combined (tier not listed) | Staff of Wizardry + Dagon 2 | -90% Magic resistance to the target, +50% Mana gained by dealing damage.&nbsp;\nAutocast: Deal 600 magical damage to a random enemy. |
| Dagon 4 | combined (tier not listed) | Staff of Wizardry + Dagon 3 | -120% Magic resistance to the target, +50% Mana gained by dealing damage.&nbsp;\nAutocast: Deal 700 magical damage to a random enemy. |
| Dagon 5 | combined (tier not listed) | Staff of Wizardry + Dagon 4 | -150% Magic resistance to the target, +50% Mana gained by dealing damage.&nbsp;\nAutocast: Deal 800 magical damage to a random enemy. |
| Kaya | combined (tier not listed) | Staff of Wizardry + Robe of the Magi | -100% Magic resistance to target |
| Perseverance | combined (tier not listed) | Ring of Health + Void Stone | +10 HP regeneration\n+100% mana gained by dealing damage |
| Monkey King Bar | combined (tier not listed) | Demon Edge + Javelin + Quarterstaff | +80 Attack damage, +10 Attack speed\nAttacks pierce through evasion. |
| Battle Fury | combined (tier not listed) | Perseverance + Demon Edge | +30 Attack damage, melee attacks deal 50% cleave damage in a range of 2 grids\n\n+10 HP regeneration\n+100% mana gained by dealing damage |
| Heart of Tarrasque | combined (tier not listed) | Reaver + Vitality Booster + Vitality Booster | +1,000 Max HP\n1% HP regeneration per 2s. |
| Assault Cuirass | combined (tier not listed) | Platemail + Chainmail + Hyperstone | Aura: +10 Armor and +10 attack speed for allies within 1 grid\nAura: -10 Armor and -10 attack speed for enemies within 1 grid |
| Blade Mail | combined (tier not listed) | Chainmail + Broad Sword | +5 Armor, +10 Attack damage\nDuplicates 100% of the damage taken back to the unit that dealt the damage. |
| Divine Rapier | combined (tier not listed) | Sacred Relic + Demon Edge | +150 Attack damage |
| Refresher Orb | combined (tier not listed) | Perseverance + Perseverance | +20 HP regeneration\n+200% mana gained by dealing damage\nRefreshes the abilities after the first cast. 30 seconds cooldown. |
| Crystalys | combined (tier not listed) | Broad Sword + Blades of Attack | +15 Attack damage\n15% chance to critical hit for 150% damage. |
| Hood of Defiance | combined (tier not listed) | Ring of Health + Ring of Regeneration + Cloak | +15 HP regeneration\n+30 Magic resistance |
| Vanguard | combined (tier not listed) | Vitality Booster + Stout Shield + Ring of Health | +250 Max HP, +10 HP regeneration\n50% chance to block 50 damage. |
| Maelstrom | combined (tier not listed) | Mithril Hammer + Javelin | +25 Attack damage\n25% chance on attack to release a bolt of electricity that leaps between 3 targets, dealing 100 to each. |
| Scythe Of Vyse | combined (tier not listed) | Mystic Staff + Ultimate Orb + Void Stone | -20% Magic resistance to the target\n+50% mana gained through dealing and taking damage\n+250 Maximum HP\nAutocast: Transforms an enemy chess into a sheep for 5s. |
| Moon Shard | combined (tier not listed) | Hyperstone + Hyperstone | +70 Attack speed |

### Item list, code snapshot 2023-04-10 (103 entries)

| Name | Tier / source | Recipe | Effect (English tooltip) |
|---|---|---|---|
| Ring of Regeneration | drop T1 |  | +10 health regeneration. |
| Void Stone | drop T1 |  | +50% mana regeneration. |
| Blades of Attack | drop T1 |  | +10 damage. |
| Quarterstaff | drop T1 |  | +10% attack speed. |
| Staff of Wizardry | drop T1 |  | +20% magical damage. |
| Stout Shield | drop T1 |  | +15 physical damage block. |
| Cloak | drop T1 |  | +30% magic resistance. |
| Morbid Mask | drop T1 |  | +10% leech. |
| Blight Stone | drop T1 |  | 3 armor reduction to the target on attacks. |
| Vitality Booster | drop T1 |  | +250 health. |
| Energy Booster | drop T1 |  | +40 mana. |
| Boots of Speed | drop T1 |  | Fast movement. |
| Mithril Hammer | drop T2 |  | +15% damage. |
| Javelin | drop T2 |  | +20% attack speed. |
| Ring of Health | drop T2 |  | +20 health regeneration. |
| Crown | drop T2 |  | +100% mana regeneration. |
| Talisman of Evasion | drop T2 |  | +25% evasion. |
| Ogre Axe | drop T2 |  | +20% health. |
| Broadsword | drop T2 |  | +20 damage. |
| Chainmail | drop T2 |  | +5 armor. |
| item_shengzheyiwu | drop T3 |  | unknown |
| Hyperstone | drop T3 |  | +40% attak speed. |
| item_dafu | drop T3 |  | unknown |
| Mystic Staff | drop T3 |  | +60% magical damage. |
| Demon Edge | drop T3 |  | +40 damage. |
| Platemail | drop T3 |  | +10 armor. |
| Blink Dagger | drop T3 |  | Fast movement.\nActive Ability: InstantInstantly teleports off the farest square that allows it to attack an enemy. Generates 30 on activation.  Range: %max_blink_distance%  15 |
| Poor Man's Shield | combined T2 | Stout Shield + Stout Shield | +30 physical damage block, +30% chance to block extra 30% physical damage. |
| Perseverance | combined T2 | Ring of Health + Void Stone | +20 health regeneration, +50% mana regeneration. |
| Soul Booster | combined T2 | Vitality Booster + Energy Booster | +250 health, +40 mana. |
| Crystalys | combined T2 | Broadsword + Blades of Attack | +30 damage.\nAttacks have 10% chance to cause 200% critical strike damage. |
| Force Staff | combined T2 | Staff of Wizardry + Ring of Regeneration | +20% magical damage, +10 health regeneration.\nActive Ability: Activates when an enemy is nearbyPushes a melee enemy attacking the caster to the fartherest square, and pushes the caster to the opposite farest square. Steals 20 mana from the target enemy after the push.   15 |
| Rod of Atos | combined T2 | Crown + Quarterstaff | +10% attack speed, +100% mana regeneration.\nActive Ability: InstantRoots the highest level enemy chess piece for 5 seconds.   15 |
| DragonLance | combined T2 | Ogre Axe + Quarterstaff | HP + 20%, attack speed +,10%.\nAttack range +200 on ranged unit |
| Mask of Madness | combined T2 | Morbid Mask + Quarterstaff | +10% attack speed, +10% leech.\nActive Ability: InstantGrantss +50% attack speed, but reduces armor by 8 and silences the caster for 5 sec.   20 |
| Summoning Stone | combined T2 | Vitality Booster + Blight Stone | +250 health, 3 armor reduction to the target on attacks.\nUnits summoned by the carrier have +25% Health, +25% damage and +50% duration. |
| Tranquil Boots | combined T2 | Boots of Speed + Ring of Regeneration | Fast movement, +10 health regeneration.\nAura:Allies within the range gain 25 healing every sec.  Range: %max_distance% |
| Arcane Boots | combined T2 | Boots of Speed + Energy Booster | Fast movement, +40 mana.\nActive Ability: InstantRestores 20 mana for all allies within the range.  Range: %max_distance%  30 |
| Hell of Iron Will | combined T2 | Morbid Mask + Stout Shield | +15 physical damage block, 10% leech. |
| Witless Shako | combined T2 | Hell of Iron Will + Cloak | +30% magic resistance, +10% lifesteal, +15 physical damage block.\nThe carrier +1 level. |
| Lucky Pendant | combined T2 | Talisman of Evasion + Void Stone | Evasion +25%, Mana regen +50%.\nThe carrier has +50% additional chance on all probability synergies. |
| Eaglesong | combined T3 | Talisman of Evasion + Javelin | +20% attack speed, +25% evasion. |
| Ultimate Orb | combined T3 | Vitality Booster + Crown | +250 health, +100% mana regeneration. |
| Maelstrom | combined T3 | Mithril Hammer + Javelin | +15% damage, +20% attack speed.\n20% chance on attack to release a bolt of electricity that leaps between 3 targets, dealing 100 magical damage to each. |
| Blade Mail | combined T3 | Broadsword + Chainmail | +5 armor, +20 damage.\nActive Ability: Activates when taking damageDuplicates 100% of the damage taken back to the unit that dealt the damage for 4 sec.   30 |
| Vanguard | combined T3 | Vitality Booster + Ring of Health | +250 health, +20 health regeneration.\n50% chance to block 100 damage on melee pieces, and 50 on ranged. |
| Hood of Defiance | combined T3 | Ring of Health + Cloak | +50% magic resistance. +20 health regeneration. |
| Black King Bar | combined T3 | Ogre Axe + Mithril Hammer | +20% damage, +20% health.\nActive Ability: Activates when taking damageGrants spell immunity and 100% magic resistance for 4 sec. Dispells negative statuses from the caster on activation.   60 |
| Mekansm | combined T3 | Chainmail + Cloak + Ring of Regeneration | +5 armor, +30% magic resistance, +10 health regeneration.\nActive Ability: Activate when most allies are woundedHeals 500 health to all allied units within the range.  Range: %max_distance%  30 |
| Giant's Ring | combined T3 | Poor Man's Shield + Ring of Health | +20 health regeneration, +30 physical damage block.\nActive Ability: InstantTaunts all enemies nearby and increases block bonus granted by Giant's Ring by 200% for 7 sec.   60 |
| Telescope | combined T3 | Soul Booster + Quarterstaff | +250 health, +40 mana, +10% attack speed.\nRanged carrier also counts as Dwarf. |
| Dagon Level 1 | combined T3 | Staff of Wizardry + Crown | +15% magical damage, +100% mana regeneration.\nActive Ability: InstantDeals 400 magical damage to a random enemy chess piece.   15 |
| Dagon Level 2 | combined T3 | Staff of Wizardry + Dagon Level 1 | +30% magical damage, +100% mana regeneration.\nActive Ability: InstantDeals 500 magical damage to a random enemy chess piece.   12 |
| Dagon Level 3 | combined T3 | Staff of Wizardry + Dagon Level 2 | +45% magical damage, +100% mana regeneration.\nActive Ability: InstantDeals 600 magical damage to a random enemy chess piece.   9 |
| Dagon Level 4 | combined T3 | Staff of Wizardry + Dagon Level 3 | +60% magical damage, +100% mana regeneration.\nActive Ability: InstantDeals 700 magical damage to a random enemy chess piece.   6 |
| Dagon Level 5 | combined T3 | Staff of Wizardry + Dagon Level 4 | +75% magical damage, +100% mana regeneration.\nActive Ability: InstantDeals 800 magical damage to a random enemy chess piece.   3 |
| Necronomicon | combined T3 | Morbid Mask + Morbid Mask + Energy Booster | +20% leech, +40 mana.\nActive Ability: InstantSummons a Warrior and an Archer to fight for you that have increased health, duration and ability level based on the caster level.   30 |
| Necronomicon | combined T3 | item_silingshu_1 + Ogre Axe | +20% leech, +40 mana.\nActive Ability: InstantSummons a Warrior and an Archer to fight for you that have increased health, duration and ability level based on the caster level.   30 |
| Necronomicon | combined T3 | Necronomicon + Ogre Axe | +20% leech, +40 mana.\nActive Ability: InstantSummons a Warrior and an Archer to fight for you that have increased health, duration and ability level based on the caster level.   30 |
| Spider Legs | combined T3 | Rod of Atos + Boots of Speed | Fast movement, +10% attack speed, +100% mana restoration.\nThe carrier also counts as Aqir, and is eligible for friendly Infection spawning.\nActive Ability: InstantRoots the highest level enemy chess piece for 5 seconds.   15 |
| Trickster Cloak | combined T3 | Talisman of Evasion + Cloak | Evasion +25%, Magical Resistance +30%, Immune to deformation and mind control.\nHides its owner in the preparation phase. |
| Skull Basher | combined T3 | Mithril Hammer + Platemail | +15% damage, +10 armor.\nGrants melee hero 20% chance on hit to stun the target for 3s. Bash chance for ranged heroes is 10%. |
| Hell of the Dominator | combined T3 | Crown + Hell of Iron Will | +100% %mana regeneration, +15 physical damage block, 10% leech.\nActive Ability: InstantMind Controls a limited-life(illusions/minions/evolved pieces…) enemy whose level equals to or less than 3 for 10s.   20 |
| Repair tools | combined T3 | Hood of Defiance + Mithril Hammer | Magic resistance+50%,Hp+20,Armor+15%.\nCarriers are consideredMech.If the carriers survive a battle on the main field, you will obtain a Dark heart of Machine. |
| Aghanim's Shard | combined T3 | Soul Booster + Energy Booster + Energy Booster | Hp+250,Mana+40.\nUpgrade the chess piece skill level of the carrier. |
| Moon Shard | combined T4 | Hyperstone + Hyperstone | +160% attack speed. |
| Desolator | combined T4 | Mithril Hammer + Mithril Hammer + Blight Stone | +30% damage.\n10 armor reduction to the target on attacks. |
| Battle Fury | combined T4 | Perseverance + Broadsword | +20 damage, +20 health regeneration, +50% mana regeneration.\nMelee attacks deal 30% cleave damage in a range of 1 square. |
| Manta Style | combined T4 | Ultimate Orb + Javelin | +250 health, +20% attack speed, +100 mana regeneration.\nActive Ability: InstantCreates 2 images of the caster that last 10 seconds.   20 |
| Eye of Skadi | combined T4 | Ultimate Orb + Ultimate Orb | +500 Health, +200% mana restoration.\nSlows enemy attack and movement by 50% on attacks, and reduces healing effect received by the victims by 50% for 5 seconds. |
| Linken's Sphere | combined T4 | Perseverance + Ultimate Orb | +250 health, +20 health regeneration, +150% mana regeneration.\nBlocks targeted spell.   3 |
| Refresher Orb | combined T4 | Perseverance + Perseverance | +40 health regeneration, +100% mana regeneration\nActive Ability: Activates when ability or items incur cooldownResets the cooldown of abilities and other items.   20 |
| Bloodstone | combined T4 | Soul Booster + Soul Booster | Hp+500, Mana+40.\nIf an enemy chess piece on the host's side is killed, the holder has a 20% chance of gaining 1 energy point. Each energy point will increase the magic damage the holder inflicts by 10%.\nIf the holder is killed, half of the energy points will be lost and all friendly units within range will absorb (the lost energy points x 500) maximum health and (the lost energy points x 50) mana.  Effect range: %max_distance% |
| Shiva's Guard | combined T4 | Platemail + Mystic Staff | +10 armor, +60% magical damage.\nActive Ability: InstantEmits a freezing wave that slows the attack speed of enemies within 5 cells by 50%, and reduces healing effect received by the victimx by 25% for 4 seconds and deal 100 magical damage.  Range: %max_distance%  20 |
| Pipe of Insight | combined T4 | Hood of Defiance + Cloak | +20 health regeneration, +50% magic resistance.\nActive Ability: Activates when taking damageGrants all allies with 20 health regeneration and 30% magic resistance for 8 sec.  Range: %max_distance%  30 |
| Crimson Guard | combined T4 | Vanguard + Hell of Iron Will | +250 health, +20 health regeneration, +5 armor.\n50% chance to block 100 damage on melee pieces, and 50 on ranged.\nActive Ability: Activates when taking damageGrants all allies within the range 50% chance to block 100 damage for melee pieces and 50 damage for ranged pieces. Lasts 8 sec.  Range: %max_distance%  30 |
| Guardian Greaves | combined T4 | Mekansm + Arcane Boots | Fast movement, +40 mana, +30% magic resistance, +10 health regeneration, +5 armor.\nActive Ability: Activate when most allies are woundedRestores 500 health and 20 mana to all allies within the range, and removes most negative effects.  Range: %max_distance%  20 |
| item_fulingsuo | combined T4 | Maelstrom + Rod of Atos | +15% attack damage, +30% attack speed, +100% mana regeneration.\nActive Ability: Instant20% chance on attack to release a bolt of electricity that leaps between 3 targets, dealing 100 magical damage to each.\nRoots all enemies around the highest level enemy chess piece(a radius of 2 grid) for 5 seconds and deal 100 magical damage.   15 |
| Book of the Dead | combined T4 | Necronomicon + Summoning Stone | +250 health, +40 mana, +20% lifesteal. Attacks reduce the target's armor by 3.\nYour Units have +25% Helath, +25% Damage, +50% duration and 75% chance to summon 1 more minion.\nActive ability: InstantSummons a Warrior and an Archer to fight for you whose health, duration and ability levels are dependent on caster level.   30 |
| Trident | combined T4 | Crystalys + Staff of Wizardry + Staff of Wizardry | +30 damage, +40% magical damage.\nAttacks have 10% chance to cause 200% critical strike damage.\nThe carrier can simultaneously damage at most 2 chess pieces sharing the same name of the target. Eliminating a chess piece inflicts pure damage equal to 20% maximum health to at most 2 chess pieces sharing the same name of the target. |
| Bloodthron | combined T4 | Crystalys + Rod of Atos | +30 damage, +10% attack speed, +100% mana restoration.\nAttacks have a 10% chance to critically stike for 200% damage.\nActive ability: InstantRoots and silences the highest-level enemy chess piece in the attack range for 6 seconds. Any attacks to the victim causes 200% critical damage and all allies with in the attack range focus attacking the victim within the duration.   15 |
| Aeon Disk | combined T4 | Black King Bar + Poor Man's Shield | +15% damage, +20% health, +30 physical damage block, +30% chance to block extra 30% physical damage.\nActive Ability: Activates when taking damageDispels all negative status from the caster and grants 100% physical damage block, 100% magic resistance and spell immunity for 3 seconds. Activates regardless of cooldown when the caster is at low health.   60 |
| Hurricane Pike | combined T4 | DragonLance + Force Staff | Magic damage+20%,Hp+10,Health+20%,Attack speed+10%.\nRanged carrier‘s attack range+200.\nActive skill: Activated when there is an enemy chess piece in close proximityPush nearby enemy to the farthest grid in the opposite direction, and push oneself to the farthest grid while stealing 20 mana from the target. Ranged carriers will also immediately perform 4 distance-ignoring attacks on the target.   15 |
| Hell of the Overlord | combined T4 | Hell of the Dominator + Platemail | +100% mana regeneration, +15 physical damage block, 10% leech, +10 armor.\nActive Ability: InstantMind Controls a limited-life(illusions/minions/evolved pieces…) enemy whose level equals to or less than its owner’s level for 10s.   5 |
| Boots of Bearing | combined T4 | Tranquil Boots + Trickster Cloak | Fast movement, HP regeneration +10, Evasion +25%, Magical Resistance +30%.\nHides its owner in the preparation phase.\nAuraGrants 25 HP per second,  and immune to deformation and mind control to allies nearby.  Max Distance: %max_distance% |
| Divine Rapier | combined T5 | item_shengzheyiwu + Demon Edge | +600 base damage.\nDrops to another random player when losing a home battle. |
| Radiance | combined T5 | item_shengzheyiwu + Talisman of Evasion | +45% damage, +25% evasion.\nAura:Scorches nearby enemies for 50 pure damage per sec and causes them to miss 25% of their attcks.  Range: %max_distance% |
| Monkey King Bar | combined T5 | Demon Edge + Javelin + Quarterstaff | +40 damage, +30% attack speed\nAttacks pierce through evasion. |
| Butterfly | combined T5 | Eaglesong + Talisman of Evasion + Quarterstaff | +50% evasion, +30% attack speed.\nActive Ability: Activates when taking damageDoubles the evasion and attack speed bonus from Butterfly for 5 sec.   30 |
| Daedalus | combined T5 | Crystalys + Demon Edge | +70 damage.\nAttacks have a 10% chance to critically strike for 350% damage. |
| Mjollnir | combined T5 | Maelstrom + Hyperstone | +15% damage, +60% attack speed.\n20% chance on attack or being attacked to release a bolt of electricity that leaps between 5 targets, dealing 100 magical damage to each. |
| Satanic | combined T5 | item_dafu + Mask of Madness | +60% helth, +10% attack speed, +10% lifesteal.\nActive Ability: Activates when taking damageGains +150% Lifesteal for 6 sec.   30 |
| Assault Cuirass | combined T5 | Platemail + Chainmail + Hyperstone | +10 armor, +40% attack speed\nAura: Grants +10 armor and +10% attack speed to allies nearby.  Range: %max_distance%\nAura: Enemies nearby suffer -10 armor and -10% attack speed.  Range: %max_distance% |
| Scythe of Vyse | combined T5 | Mystic Staff + Ultimate Orb | +60% magical damage, +100% mana regeneration, +250 health.\nActive Ability: InstantTransforms an enemy piece of the highest level into a harmless animal for 8 sec.   15 |
| Cloak of Tyrants | combined T5 | item_shengzheyiwu + Trickster Cloak | Evasion+25%, Attack+45%, Magic Resistance+30%.\nThe carrier can hide their location during the preparation phase, and at the start of the battle, gain the full benefit of all activated racial/class skills of friendly units. |
| Ogre Hood | combined T5 | item_dafu + Witless Shako | +60% health, +30% magic resistance, +10% lifesteal, +15 physical damage block.\nThe carrier also counts asOgre and +2 level. |
| Mirror Shield | combined T5 | Blade Mail + Giant's Ring | 5 armor, +20 health regeneration, +20 damage, +30 physical damage block, +30% chance to block extra 30% physical damage. \nActive Ability: InstantTaunts all nearby enemy chess pieces and reflect all incoming damage to the sources as pure damage at 200% effectiveness. Lasts 6 seconds.   30 |
| Abyssal Blade | combined T5 | Skull Basher + Vanguard | +15% damage, +10 armor, +250 HP, +20 HP regeneration.\n50% chance to block 100 physical damage on melee pieces, and 50 on ranged. Grants melee hero 20% chance on hit to stun the target for 3s, and 10% chance for ranged heroes.\nActive Ability: InstantStun a nearby enemy piece for 5s, and deal 100 pure damage.   10 |
| Overwhelming Blink | combined T5 | Blink Dagger + item_dafu | +60% HP, fast movement.\nActive Ability: InstantInstantly teleports off the farest square that allows it to attack an enemy. Generates 30 on activation.+ 30% HP per activation.  Max Blink Distance:%max_blink_distance%  10 |
| Swift Blink | combined T5 | Blink Dagger + Eaglesong | Attack speed+20%, Evasion+25%, Fast movement.\nActive ability:  Instant CastInstantly move to the farthest grid within the range that can attack the enemy, gain 30 mana, and a stackable effect of +25% attack speed.  maximum distance: %max_blink_distance%  10 |
| Arcane Blink | combined T5 | Blink Dagger + Mystic Staff | Magic damage+60%, Fastmovement.\nActive ability:  Instant CastInstantly move to the farthest grid  within the range that can attack the enemy, gain 30 mana, and a stackable effect of +25% magic damage.  maximum distance: %max_blink_distance%  10 |
| Lotus Orb | combined (tier not listed) | Perseverance + Energy Booster | Gains 40 mana when the battle starts.\n+15 Health Regeneration, +50% mana gained by dealing damage.\nReflects unit-targeting spells. Cooldown: 5 sec. |
| item_silingshu_1 | combined (tier not listed) | Ogre Axe + Void Stone | unknown |

### Items and relics after the code dump

Item additions after the code dump (S1/S2):

- Force Staff (T1) and Hand of Midas (T2) re-added 2023-11-29.
- Phase Boots and Giant's Ring 2023-12-26.
- Ascetic's Cap (T2) 2024-01-29.
- Paladin Sword (T3) 2025-05-25.
- Spoil Tokens 2023-07-31.

Relics are a separate system from 2021-03-25. They are listed in S2 docs, for example Captain Greenskin's Treasure, Weaponsmith, The Great Vault, Shroud of Concealment, Chiaroscuro Certainty, Monopoly Contract, Sin Reaper, Family Discount, Elite Recruiter, Grace of Nature and Gaoler's Master Key. Later relics include Malefic Rapture, Unstable Shrink Ray, Serendipity Compass and others. A game uses 10 random relics out of the pool (S1 2021-07-26; 10 of 15 per 2025-05-25).

## Synergies (races and classes)

Breakpoints are the piece counts read from the `combo_ability_type` table in the Lua code. Synergies count different pieces; duplicates do not count.

| Synergy | 2019-02 | 2019-06 | 2019-12 | 2020-07 | 2021-02 | 2023-04 (text) |
|---|---|---|---|---|---|---|
| Warrior | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 |
| Mage | 3/6 | 3/6 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 |
| Warlock | 3/6 | 3/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 (code 2022) |
| Mech/Inventor | 2/4 | 2/4 | 3/6 | 3/6 | 3/6 | 3/6 |
| Assassin | 3/6 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 |
| Hunter | 3/6 | 3/6 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 |
| Knight | 2/4/6 | 3/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 |
| Shaman | 2 | 2 | 2/4 | 2/4 | 2/4 | 2/4/6 |
| Demon Hunter | 1/2 | 1/2 | 1/2 | 1/2 | 1/2 | 2/4 |
| Druid | 2 (4 in text) | 2/4 text | same | same | same | 2/4 |
| Priest | - | special (1) | special | 1/2/3 | 2 | 2/4 |
| Wizard | - | 2 | 2 | 2/3 | 2 | listed |
| Monk | - | - | - | 1/2 | 2 | 2/4 |
| Troll | 2/4 | 2/4 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 |
| Beast | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 |
| Elf | 3/6 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 | 3/6/9 |
| Human | 2/4/6 | 2/4/6 | 2/4/6 | 3/6 | 3/6 | 3/6 |
| Undead | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 3/6/9 |
| Orc | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 | 2/4/6 |
| Naga | 2/4 | 2/4 | 2/4 | 2/4 | 2/4 | 2/4/6 |
| Goblin | 3/6 | 3/6 | 3/6 | 3/6 | 3/6 | 3/6 |
| Elemental | 2/4 | 2/4 | 2/4 | 2/4 | 2/4 | 2/4 |
| Demon | special | special | special | special | special | special |
| Dwarf | 1 | 1 | 1/2 | 1/2 | 2 | 2 |
| Ogre | 1 | 1 | 2 | 2 | 2 | 2 |
| Dragon | 3 | 3 | 3/5 | 3/5 | 3/5 | 3/5 |
| God | - | special | 2/4 | 2/4 | 2/4 | 2/4/6 |
| Satyr | text only | text only | text only | ... | ... | 1 |
| Aqir | - | - | 2/4 | 2/4 | 2/4 | 2/4 |
| Pandaren | - | - | - | 1/2/3 | 1/2/3 | 1/2 (text) |
| Tauren | - | - | - | - | 2/4 | 2/4 |
| Kobold | - | - | - | - | 1 | 1/2 |
| Faceless | defined, unused | defined | 1 | 1 | 1 | 1 |
| Centaur | - | - | - | - | - | 2/4 (2023-02) |

Notes:

- God in 2019: active only when no other species trait is active; (1) -50% CD, (2) a further -50% (S1 2019-03-29). Pre-2019-12 code uses condition 999 (special-case code).
- Wizard (2) reduces the requirement by 1 for synergies needing 4+ pieces (text 2019-12). This is how Wizards "unlocked" (6) Beast, (6) Undead, (6) Mech, (4) Naga and (9) Mage on 2019-06-26 (S1).
- Knight (code 2019-06): 3/6. The tooltip in the same build still says 2/4/6, a tooltip lag. Patch 2019-04-30 set (3)/(6) (S1).
- Warlock (code 2019-06): lifesteal 15%/25% (`AttackHeal per=0.15/0.25`). The tooltip says 20%/30%. Patch 2019-04-13 says 15%/25%.

### Effect text, mid-2019 (wiki era)

These are the English tooltip strings from that build. Several are stale against the code and patch notes: Warrior armor was 5/7/9 by patch (tooltip 7/8/9); Warlock lifesteal is 15%/25% in code (tooltip 20/30); Knight breakpoints are 3/6 in code. The patch notes in the timeline take priority.

**Assassin** (class; breakpoints in code: [3, 6, 9])

Assassins are skilled executioners that leap into enemy backlines. Together, they gain a keen eye for weaknesses in enemy armor, gaining the ability to deliver devastating critical hits.
Class Perk: Coup de Grace
 Active when you have at least (N) different assassin pieces on board.
 (3) Assassins: All friendly assassins have 10% chance to critical hit for 300% damage.
 (6) Assassins: All friendly assassins have 15% chance to critical hit for 400% damage.
 (9) Assassins: All friendly assassins have 20% chance to critical hit for 500% damage.

**Beast** (race; breakpoints in code: [2, 4, 6])

Beasts are ferocious animals. They prey on the weak, drawing power from the size of their ever-growing pack.
Species Trait: Power of the Wild
 Active when you have at least (N) different beast pieces on board.
 (2) Beasts: All allies have +10% attack damage, including summoned units.
 (4) Beasts: All allies have +15% attack damage, including summoned units.
 (6) Beasts: All allies have +20% attack damage, including summoned units.

**Demon** (race; breakpoints in code: [0])

Chaotic beings that emerged from a dimensional fissure.
Species Trait: Fel Power
 Active when you control only one kind of demon on the chessboard.
 Attacks deal 50% extra pure damage to the target.

**Demon Hunter** (class; breakpoints in code: [1, 2])

Exiled warriors who’ve learned to control fel energies.
Class Perk: Shattered soul
 Active when there are at least (N) different demon hunter chess pieces on the chessboard.
 (1) Demon Hunter: Negates enemy demon’s Fel Power.
 (2) Demon Hunters: All friendly demons retain their Fel Power.

**Dragon** (race; breakpoints in code: [3])

Dragons were among the first creatures to exist on this ancient land. They possess unimaginable powers, deep within, which they like to show off when they commune together.
Species Trait: Attunement
 Active when there are at least (N) different dragon chess pieces on the chessboard.
 (3) Dragons: All friendly dragons have 100 mana when battle starts.
Coming Soon: 
(6) Dragons: All allies have 100 mana when battle starts.

**Druid** (class; breakpoints in code: [2])

Shapeshifters and healers that have harnessed the ancient powers of nature, sharing exceptionally profound knowledge among each other.
Due to the specialities of the class, Druids will not benifit from [Auto Combine on Bench].
Class Perk: Synergy
Active when there are at least (N) different druid chess pieces on the chessboard.
 (2) Druids: Two identical Druid★ can promote to a Druid★★.
 (4) Druids: Two identical Druid★★ can promote to a Druid★★★.

**Dwarf** (race; breakpoints in code: [1])

Dwarves are a bold and courageous species of humanoids that excel in creating and refining weapons and gadgets, allowing them to slay their foes from afar.
Species Trait: Take Aim
 Attack range increased by 300.

**Elemental** (race; breakpoints in code: [2, 4])

Elementals are unstable beings that radiate mysterious energy when attacked. This gives them a chance to turn their melee aggressors to stone.
Species Trait: Elemental Essence
 Active when there are at least (N) different elemental chess pieces on the chessboard.
 (2) Elementals: All friendly elementals have 30% chance to turn the attacker into stone for 4s when attacked by melee chess pieces.
 (4) Elementals: All friendly chess pieces have 30% chance to turn the attacker into stone for 4s when attacked by melee chess pieces.

**Elf** (race; breakpoints in code: [3, 6, 9])

Elves are nimble-footed fighters who rely on their refined reflexes to dodge deadly attacks.
Species Trait: Evasion
 Active when you have at least (N) different elf pieces on board. 
 (3) Elves: All friendly elves have +20% evasion. 
 (6) Elves: All friendly elves have additional +25% evasion.
 (9) Elves: All friendly elves have additional +30% evasion.

**Goblin** (race; breakpoints in code: [3, 6])

Goblins are a cunning species with a love for money, explosives, and less-than-reliable technology. Their drive to invent fuels them, giving the brightest among them a boost in stamina and fortitude.
Species Trait: Nanobots 
 Active when you have at least (N) different goblin pieces on board.
 (3) Goblins: Grants a random ally +15 armor and +10 HP regeneration. 
 (6) Goblins: Grants all allies +15 armor and +10 HP regeneration.

**God** (race; breakpoints in code: [999])

The highly worshipped Gods are considered as the creators of Drodo Island. Being both arrogant and prideful, the loyal beliefs from their creations the have forged the supreme powers that Gods possess, granting them the most significant potential among all creatures which can completely change the outcome of a battle.
Species Trait: Overweening Superiority
Active when all other species traits are inactive.
Active when you have at least (N) different god pieces on board.
(1) God: All allies have -50% ability cooldown.
(2) Gods: All allies have additional -50% ability cooldown.

**Human** (race; breakpoints in code: [2, 4, 6])

Humans are an intelligent species that values kindness and cooperation, and silences any who they deem unworthy.
Species Trait: Censorship
 Active when there are at least (N) different human chess pieces on the chessboard.
 (2) Humans: All friendly humans have 20% chance to silence target for 4s when attacking.
 (4) Humans: All friendly humans have 25% chance to silence target for 4s when attacking.
 (6) Humans: All friendly humans 30% chance to silence target for 4s when attacking.

**Hunter** (class; breakpoints in code: [3, 6])

Hunters are experienced survivors of the primal world outside of civilization. Their refined weapon techniques allow them to attack more effectively than others would think possible, and strike through the enemies' evasive techniques.
Class Perk: Deadly Shot
 Active when you have at least (N) different hunter pieces on board. 
 (3) Hunters: All friendly hunters have +25 base attack damage, and 25% chance to attack through evasion. 
 (6) Hunters: All friendly hunters have an additional +40 base attack damage, and 40% chance to attack through evasion.

**Knight** (class; breakpoints in code: [3, 6])

Knights are holy defenders of the realm. As they engage in battle, the Omniscience may bless them with a divine shield.
Class Perk: Divine Protection
 Active when you have at least (N) different knight pieces on board.
 (2) Knights: All friedly knights have 30% chance to proc a damage-reducing shield for 3 sec.
 (4) Knights: All friedly knights have 30% chance to proc a second shield. Shield effects stack.
 (6) Knights: All friedly knights have 30% chance to proc a third shield. Shield effects stack.

**Mage** (class; breakpoints in code: [3, 6])

Mages are keen intellectuals who have spent years studying arcane knowledge. This allows them to make their victims more vulnerable to magic damage.
Class Perk: Magic Vulnerability
 Active when you have at least (N) different mage pieces on board. 
 (3) Mages: All enemies have -40% magic resistance.
 (6) Mages: All enemies have additional -40% magic resistance.

**Mech** (class; breakpoints in code: [2, 4])

Mechs are equipped with unique contraptions to quickly recover from any kind of damage on the battlefield.
Class Perk: Self Repair
 Active when you have at least (N) different mech pieces on board.
 (2) Mechs: All friendly mechs have +15 HP regeneration. 
 (4) Mechs: All friendly mechs have an additional +25 HP regeneration.

**Naga** (race; breakpoints in code: [2, 4])

Nagas are cursed sea-dwellers that haunt the ocean floor. Their hardened, scaly skin acts as an effective defense against spells.
Species Trait: Scale Armor 
 Active when you have at least (N) different naga pieces on board. 
 (2) Nagas: All allies have +30% magic resistance.
 (4) Nagas: All allies have an additional +30% magic resistance.

**Ogre** (race; breakpoints in code: [1])

Ogres have two heads, or was it three? More heads mean more health for me!
Species Trait: Two Heads
 +10% Max HP .

**Orc** (race; breakpoints in code: [2, 4, 6])

Orcs are large, muscular, and slightly stupid fighters, making them excellent at tolerating pain and injury.
Species Trait: Battle Hardened 
 Active when you have at least (N) different orc pieces on board. 
 (2) Orcs: All friendly orcs have +200 max HP.
 (4) Orcs: All friendly orcs have an additional +300 max HP.
Coming Soon: 
 (6) Orcs: All friendly orcs have an additional +400 max HP.

**Priest** (class; breakpoints in code: [999])

Priests are blessed invokers of light and darkness, who express their determined spirits through dedicated service to the people. For numerous battles, with the mastery of light and shadow energies they have weaved the most reliable life-saving protections for their allies.
Class Perk: Barrier
 Active when you have at least (N) different priest pieces on board.
(1) Priest: Grants your Courier a holy barrier that reduces 20% of Courier damage taken.

**Satyr** (race; breakpoints in code: see table)

Demonic species risen from twisted elves, experting at concealing themselves in nightmare, and fanatic in spreading blood and agony.
Species Trait: Concealment
 Active when there are Satyrs in your waiting bench. 
 Hides your bench from other players' vision.

**Shaman** (class; breakpoints in code: [2])

Mortal mediators of the ancient elements.
Class Perk: Hex
 Active when there are at least (N) different shaman chess pieces on the chessboard.
 (2) Shamen: Hexes a random enemy for 6s when battle starts.

**Troll** (race; breakpoints in code: [2, 4])

Trolls are loyal followers of the Wild Gods, empowered with voodoo magic. This allowing them to become frenzied throughout their battles, attacking with relentless fervor.
Species Trait: Voodoo Frenzy
 Active when you have at least (N) different troll pieces on board.
 (2) Trolls: All friendly trolls have +35 attack speed. 
 (4) Trolls: All allies have +30 attack speed.

**Undead** (race; breakpoints in code: [2, 4, 6])

The Undead are horrifying apparitions that instill fear in their enemies, revealing their weaknesses.
Species Trait: Horror 
 Active when you have at least (N) different undead pieces on board. 
 (2) Undead: All enemies have -4 armor. 
 (4) Undead: All enemies have -6 armor.
Coming Soon: 
 (6) Undead: All enemies have -8 armor.

**Warlock** (class; breakpoints in code: [3, 6])

Warlocks are cursed sorcerers who use demonic power to drain the life force from their enemies, restoring themselves to continue their cursed battle.
Class Perk: Soul Leech
 Active when you have at least (N) different warlock pieces on board.
 (3) Warlocks: All allies have +20% lifesteal. 
 (6) Warlocks: All allies have +30% lifesteal.

**Warrior** (class; breakpoints in code: [3, 6, 9])

Warriors are fearless brawlers who possess a vast knowledge of arms and armor. They use this knowledge to fortify their defenses.
Species Trait: Fortified Armor
 Active when you have at least (N) different warrior pieces on board.
 (3) Warriors: All friendly warriors have +7 armor. 
 (6) Warriors: All friendly warriors have an additional +8 armor.
 (9) Warriors: All friendly warriors have an additional +9 armor.

### Effect text, April 2023 (last code snapshot)

These strings track the 2022-2023 patch notes and match the code more closely.

**Aqir** (race; breakpoints in code: [2, 4])

Species Trait: InfectionActive when there are at least (N) different Aqir chess pieces on the chessboard. (Multiple effects stack)
(2) Aqirs: Grants all non-Aqir allies: Upon death, if there is any Infected friendly piece of the same name on the chessboard, immediately summons a random Aqir piece.
(4) Aqirs: Rank 1 effect now also applies to all friendly Aqir pieces, and the summoned Aqir piece will randomly be +1 or +2 levels higher.
Aqirs do not infect on chess pieces with a expiration duration.
The higher the level of the infected friendly chess piece, the higher the level of the summoned Aqir piece.Rank 1 effect summons the same-level piece as the dying chess piece. Rank 2 summons a chess piece that can be 1 or 2 levels higher than the dying chess piece, randomly.

**Assassin** (class; breakpoints in code: [3, 6, 9])

The first movement of Assassin pieces in each round causes Assassins to leap over to the back of the enemy lineup. The first blow after Backstab Leap is always a critical strike with a random bonus damage amplifier.
Class Perk: Coup de GraceActive when you have at least (N) different Assassin pieces on board. (Multiple effects stack)
(3) Assassins: All friendly assassins have 25% chance to critical hit for 300% damage.
(6) Assassins: All assassins have 25% chance to critical hit for 400% damage.
(9) Assassins: All assassins have 5% chance to critical hit for 500% damage and gain a Backstab Leap move after killing an enemy. This Backstab Leap always deals the maximum possible critical damage.

**Beast** (race; breakpoints in code: [2, 4, 6])

Species Trait: Power of the WildActive when you have at least (N) different Beast pieces on board. (Multiple effects stack)
(2) Beasts: All allies have +15% attack damage, including summoned units.
(4) Beasts: All allies have +30% attack damage, including summoned units.
(6) Beasts: Attacks from all allies increase physical damage taken by the target by 10%. This effect stacks and can be applied by friendly summoned units.
SPECIES TRAIT: Power of the Wild
Active when you have at least (N) different Beast pieces on board. (Multiple effects stack)
(2) Beasts: All allies have +10% attack damage, including summoned units.
(4) Beasts: All allies have +50% attack damage, including summoned units.
(6) Beasts: Attaks from all allies increases damage done by next attack from Power of the Wild by 10. Works on summoned units.

**Centaur** (race; breakpoints in code: [2, 4])

**Demon** (race; breakpoints in code: [1])

Species Trait: Fel PowerActive when you control only 1 type of Demon on the chessboard.
(1) Demon: Physical damage dealt by all friendly Demon pieces is converted to Pure damage at 100% effectiveness. +5% extra effectiveness for each demon if more than one on board.

**Demon Hunter** (class; breakpoints in code: [2, 4])

Counts as a different enemy Demon type.
Class Perk: Shuttered SoulActive when you have at least (N) different Demon Hunter pieces on board. (Multiple effects stack)
(2) Demon Hunter: All friendly Demons now count as a single type.
(4) Demon Hunters: ALL damage dealt by allies is converted into Fel Power PURE damage and will not grant any mana for the victim.

**Dragon** (race; breakpoints in code: [3, 5])

SPECIES TRAIT:  AttunementActive when you have at least (N) different pieces of this species on board. (Multiple effects stack).
(3)dragons: All friendly Dragons have an initial mana value of 100.
(5) dragons: All allies have an initial mana value of 100.

**Druid** (class; breakpoints in code: [2])

CLASS PERK: SynergyActive when you have at least (N) different pieces of this class on board. (Multiple effects stack)
(2) Druids: Two identical Druid★ can promote to a Druid★★.
 (4) Druids: Two identical Druid★★ can promote to a Druid★★★.

**Dwarf** (race; breakpoints in code: [2])

Species Trait: Take AimActive when you have at least (N) different Dwarf pieces on board. (Multiple effects stack)
(2) Dwarves: All ranged allies’ physical damage deals 10% increased damage for every 1 square away from the target.

**Elemental** (race; breakpoints in code: [2, 4])

Elemental EssenceActive when you have at least (N) different Elemental pieces on board. (Multiple effects stack)
(2) Elementals: All friendly elementals have +25% chance to turn melee aggressors into stone for 3 sec.
(4) Elementals: All allies have +20% chance to turn melee and ranged aggressors into stone for 4 sec.
Stoned enemy summoned units are Purged, dealing 25% of their current health as pure damage.

**Elf** (race; breakpoints in code: [3, 6, 9])

Species Trait: EvasionActive when you have at least (N) different Elf pieces on board. (Multiple effects stack)
(3) Elves: All friendly elves have +30% evasion.
(6) Elves: All allies have +30% evasion.
(9) Elves: An illusion of the allied chess piece is created upon a successful dodge.

**Goblin** (race; breakpoints in code: [3, 6])

SPECIES TRAIT: NanobotsActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(3)Goblins: Grants a random ally+10 armor and +15 health regeneration.
(6)Goblins: Grants all allies+10 armor and +15 health regeneration.

**God** (race; breakpoints in code: [2, 4, 6])

SPECIES TRAIT: Overweening superiorityActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(2)Gods: All allies get the effect: skill cooldown time -33%.
(4)Gods: All allies get the effect: skill cooldown time -33%.
(6)Gods:  All allies get the effect: Using skills will reduce the current remaining cooldown time of active skills and items by 50%.
Overweening superiority does not reduce the cooldown time of passive items.

**Human** (race; breakpoints in code: [3, 6])

Species Trait: Spark of MindActive when you have at least (N) different Human pieces on board. (Multiple effects stack)
(3) Humans: Grants a Tome of Knowledge at the start of a round.
(6) Humans: Grants a Tome of Knowledge at the start of a round. An extra Tome of Knowledge is granted for winning a home battle, with at least 1 friendly Human survived.

**Hunter** (class; breakpoints in code: [3, 6, 9])

Hunter pieces always focus on attacking the lowest health enemy within attack range.
Class Perk: Aimed ShotActive when you have at least (N) different Hunter pieces on board. (Multiple effects stack)
(3) Hunters: All friendly hunters have +25 base attack damage and +50% chance to pierce through evasion.
(6) Hunters: All friendly hunters have +50 base attack damage and +100% chance to pierce through evasion.
(9) Hunters: All allies have +50 base attack damage and +50% chance to knock back the enemy for 2 grids on hit.
CLASS PERK: Aimed Shot
Active when you have at least (N) different hunter pieces on board. (Multiple effects stack)
(3) Hunters: All friendly hunters have +25 base attack damage and +25% chance to pierce through evasion.
(6) Hunters: All friendly hunters have +50 base attack damage and +50% chance to pierce through evasion.
(9) Hunters: All friendly allies have +75 base attack damage and +50% chance to knock back enemy.

**Knight** (class; breakpoints in code: [2, 4, 6])

Knight pieces who have nearby allies will gain a Devine Protection shield immediately.
Class Perk: Diving ProtectionActive when you have at least (N) different Knight pieces on board. (Multiple effects stack)
(2) Knights: All friedly knights have 30% chance to proc a shield every 3 seconds in combat for 3 seconds.
(4) Knights: All allies have +30% chance to proc a shield every 3 seconds in combat.
(6) Knights: All allies have +20% chance to proc a shield every 3 seconds in combat. Procing a shield instantly heals for 200.
A Divine Protection shield reduces incoming physical and magical damage by 50%. Does not work on pure damage.
Divine Protection activates per 3s and cannot protect knights hexed.

**Kobold** (race; breakpoints in code: [1, 2])

SPECIES TRAIT: Divided We StandActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(1) Kobold: If all friendly Kobold pieces on the chessboard can be paired with another Kobold by identical levels, summon a replica for each pair of identical friendly chess pieces when battle starts.
(2) Kobolds: Duplicated chess pieces now benefit from equipped items.
Due to the specialities of the species, Kobold will not benifit from Auto Combine on Bench.

**Mage** (class; breakpoints in code: [3, 6, 9])

For every standing friendly Mage on the chessboard, Mage pieces receive +10% bonus mana restoration, up to a maximum of 50%.
Class Perk: Magical VulnerabilityActive when you have at least (N) different Mage pieces on board. (Multiple effects stack)
(3) Mages: All enemies suffer -30% magic resistance.
(6) Mages: All enemies suffer -30% magic resistance.
(9) Mages: All enemies are cursed with frost incantation, which freezes them upon receiving magical damage. Lasts longer for more increased damage taken.

**Mech** (class; breakpoints in code: [3, 6])

Inventors arm themselves with the latest technology that allows them to receive +50% healing from other sources.
Class Perk: Heart of the MachineActive when you have at least (N) different Inventor pieces on board. (Multiple effects stack)
(3) Inventors: Summon an AWESOME Invention after winning a home battle with at least 1 friendly Inventor who survived.
(6) Inventors: Heart of the Machine now summons a Golden AWESOME Invention.
Heart of the Machine: An AWESOME Invention that allows a free recruitment on use. Golden Heart of the Machine exclusively and randomly offers 5 Legendary chess pieces to choose from.

**Monk** (class; breakpoints in code: [2, 4])

Monk pieces who have nearby allies will tend to move to safer squares when their health drops low.
Class Perk: Drunken StaggerActive when you have at least (N) different Monk pieces on board. (Multiple effects stack)
(2) Monks: All friendly Monks have a chance on dealing damage to instantly eliminate the target enemy with a low health.
(4) Monks: All friendly Monks have a chance on dealing damage to reduce the current health of the target enemy by half.
The chance increases based on the portion of enemy health lost.
The lower the health of the enemy, the higher the chance it may be eliminated by Rank 1 effect. The elimination chance is cot(hp*3.1)^(1/3)*(-ln(hp))/20 on a target with hp% health.

**Naga** (race; breakpoints in code: [2, 4, 6])

Species Trait: Scale ArmorActive when you have at least (N) different Naga pieces on board. (Multiple effects stack)
(2) Nagas: All allies gain 2 stack of shields, each stack absorbs 15% ability damage (instead of magical resistance)
(4) Nagas: All allies gain 4 stacks of shields above, and the stack reduces every 3 seconds. (instead of magical resistance)
(6) Nagas: Every stack explodes when it disappears, dealing a magical damage that equals to damage absorbed after the last explosion.
Cannot absorb pure damage

**Faceless** (race; breakpoints in code: [1])

Species Trait: Whisper of DarknessActive when you have at least (N) different Faceless pieces on board. (Multiple effects stack)
(1) Faceless: If you have no other active synergies except Whisper of Darkness, all friendly chess pieces are immune to all negative effects during combat.

**Ogre** (race; breakpoints in code: [2])

SPECIES TRAIT: Iron FistActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(2) Ogres:  Each ogre can eat up allies nearby (A cross of 4 grids) whose level is lower than itself and gains their 80% HP and Damage.

**Orc** (race; breakpoints in code: [2, 4, 6])

SPECIES TRAIT: Battle HardenedActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(2) Orcs: All allies have +100 Health.
(4) Orcs: All allies have +500 Health.
(6) Orcs: All allies have +8x(Courier health lost) Health.

**Pandaren** (race; breakpoints in code: [1, 2])

SPECIES TRAIT: HospitalityActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
7% chance to invite a random un-emerged chess piece of the same species with a chess piece on the board from the chess piece pool to join the team at the beginning of the round.
(2) Pandaren: Each pandaren chess piece on board has a 10% chance to invite a piece of the same name with it.
(1) Pandaren can invite RECLUSE Pandaren pieces, spare list pieces, special undead pieces, but cannot invite pieces you already have on board.

**Priest** (class; breakpoints in code: [2, 4])

Class Perk: BarrierActive when you have at least (N) different Priest pieces on board. (Multiple effects stack)
(2) Priests: Get a Tango when defeated in a home battle if your pieces on board are less than your max pieces capacity.
(4) Priest: Every Priest piece will Mind Control an enemy whose level equals to or less than itself if your pieces on board are less than your max pieces capacity. (Max control count = Your capacity - Your pieces on board)
Lasts for at most 9 secs
Interrupted if the caster died of the victim shapeshifted

**Satyr** (race; breakpoints in code: [1])

SPECIES TRAIT: ConcealmentActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(1) Satyr: All friendly Satyrs conceal themselves on the chessboard during Preparation.

**Shaman** (class; breakpoints in code: [2, 4, 6])

Shaman pieces spend 25% less mana when casting spells.
Class Perk: HexActive when you have at least (N) different Shaman pieces on board. (Multiple effects stack)
(2) Shamans: Transforms a random enemy chess piece to a random piece of the same level when battle starts.
(4) Shamans: Transforms all friendly Shaman pieces to random chess pieces that are 2 levels higher when battle starts.
(6) Shamans: All friendly Shaman pieces instantly transform into random chess pieces that are 2 levels higher after casting their active abilities.
This is a channeling spell can be interrupted and re-rechannel again.
Hexed chess pieces now have a 20 sec duration and will be removed from the field when expires.
Rank 2 Hex does not target temporary chess pieces.

**Tauren** (race; breakpoints in code: [2, 4])

SPECIES TRAIT: Totem of the ClanActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(2) Taurens: Summons a Taunt Totem at the start of the battle.
(4) Taurens: Summons a Woodpecker Totem and a Horn Totem at the start of the battle.

**Troll** (race; breakpoints in code: [2, 4, 6])

SPECIES TRAIT: Voodoo FrenzyActive when you have at least (N) different pieces of this species on board. (Multiple effects stack)
(2) Trolls: Increases the attack speed of all allies by 15.
(4) Trolls: Increases the attack speed of all allies by 45.
(6) Trolls: All allies gain +15 attack speed for each successful attack. Maximum 15 stacks.
SPECIES TRAIT: Voodoo Frenzy
Active when you have at least (N) different troll pieces on board. (Multiple effects stack)
(2) Trolls: Increases the attack speed of all allies by 15.
(4) Trolls: Increases the attack speed of all allies by 45.
(6) Trolls: All allies gain +10 attack speed for each successful attack. Maximum 20 stacks.

**Undead** (race; breakpoints in code: [3, 6, 9])

Species Trait: PlagueActive when you have at least (N) different Undead pieces on board. (Multiple effects stack)
（3）Undead: All non-undead pieces of both sides in the battle gain the plague effect: maximum health -25%.
（6）Undead: All non-undead pieces of both sides in the battle gain the plague effect: maximum health -25%.
（9）Undead: Chess pieces infected with the plague will be resurrected as skeleton soldiers to fight for the enemy after they die.
Melee pieces will be resurrected into Skeleton King and ranged pieces will be resurrected into Clinkz in the same rank as the dead piece, which lasts for 20 secs.
Melee chess pieces will be resurrected as Wraith King✪, while ranged chess pieces will be resurrected as Clinkz✪, with star level (the star level of the dead chess piece -1). Skeleton soldiers last for 10 seconds.

**Totem** (class; breakpoints in code: see table)

A stationary guardian that does cannot attack, cast or restore health.
The Totem has health related to the number of enemy chess pieces on board and takes 1 melee damage or 2 ranged damage at a time.
Scarecrow Totem's HP equals to 2.5x number of enemy chess pieces on board.
Woodpecker Totem's and Horn Totem's HP equals to 1x number of enemy chess pieces on board.

**Warlock** (class; breakpoints in code: [2, 4, 6])

Warlock piece convert 20% of their health into 40% mana when battle starts.
Class Perk: Soul LeechActive when you have at least (N) different Warlock pieces on board. (Multiple effects stack)
(2) Warlocks: All allies have All allies have +5% lifesteal from all damage dealt.
(4) Warlocks: All allies have All allies have +25% lifesteal from all damage dealt.
(6) Warlocks: Upon death of an chess piece, the lowest health ally drains 50% of its maximum health and current mana.

**Warrior** (class; breakpoints in code: [3, 6, 9])

Warriors have a double bonus from Fortified Armor while below 50% health.
Class Perk: Fortified ArmorActive when you have at least (N) different Warrior pieces on board. (Multiple effects stack)
（3） Warriors: All friendly warriors have +3 armor.
（6）Warriors: All friendly warriors have +9 armor.
（9）Warriors: Reflect pure damage equivalent to (2x current armor) when taking physical and magical damage.

**Wizard** (class; breakpoints in code: [2, 3])

Class Perk: Something from NothingActive when you have at least (N) different Wizard pieces on board. (Multiple effects stack)
(2) Wizards: Reduces the number of chess pieces required by 1 for any synergy requiring (4) pieces or more to be active.
(4) Wizards: If you have only 1 active non-Wizard synergy, unlocks all its potential during combat.

### Major synergy reworks after mid-2019 (S1/S2)

- Ogre: Iron Fist, eat adjacent lower-level allies (2019-09-26).
- Human: Spark of Mind, Tomes of Knowledge (2020-02-19). Changed to direct XP 2/6 (2023-11-29), then "(3) 2 XP per round; (6) courier level - 2 XP after a home win" (2023-12-06).
- Mech: Heart of the Machine, a free roll consumable; (6) Golden Heart offers five $5 pieces (2019-12-24 and later).
- Orc (6): +Nx courier HP lost as HP. N was 10 (2019-10-20), 8 (2019-10-30), 7 (2020-02-19), 10 (2020-11-25), 8 (2021-10-30); capped at 800 on 2024-06-23.
- Undead: Plague, -25% max HP to non-undead on both sides, 3/6/9 with skeleton resurrection at 9 (by 2023). Earlier, 2021-03-25: -3/-6/-2 armor plus wraith form at 6 (S2).
- Naga: ability-damage shields 2/4/6 (by 2023).
- Assassin: Backstab Leap (2020-01-15).
- Knight: guaranteed shield at battle start (2020-05-27).
- Class passives added 2021-12-26.
- Pandaren: invite mechanic, reworked 2022-12-24.
- Centaur (4) (2023-02-26). God (2)/(4) -33% CD and (6) cooldown halving (2023-02-26). Dragon (5) all allies start at 100 mana (2023-02-26).
- Druid: druids not consumed into a 3-star piece (2025-07-21).

## Other mechanics

- Board 8x8; each player owns a 4x8 half; bench of 8 (wiki; S4). Pieces combine 3 to 1 for the next star, up to 3 stars. Druids combine 2 to 1 with the synergy.
- Unit stats double per star in code (chess_axe HP 700/1400/2800, dmg 50-55/100-110/200-220; 2019-06-05).md` (in-pool pieces only, with cost and active ability id).
- Mana: see wiki Mana page (not re-derived). From 2020-05-27, every 3-star piece has +10 base mana regen (S1).
- Battle start: from 2020-05-27, higher-rank pieces act first and assassins count as rank 1. Before that, start order was random with a 0.75 s assassin delay (S1).
- Opponent selection: from 2019-07-19, you avoid the same opponent for 3/2/1 rounds with 8/6-7/4-5 players alive (S1).
- Eliminated players: half their items drop to others (2019-04-05). Combined items drop as components (2019-04-06). Unspent gold is partly redistributed (2020-03-17) (S1).
- Modes: 1x8 Ranked and Casual. 2x4 Casual (2019-06-26) and 2x4 Ranked (2019-07-13), with shared courier HP and Post Piece. Rank-gated ranked maps (2022-05-05). Candy Contest beta (2023-01-19). Code 2023 has a `p2` mode (`GetCurrMapInfo().is_2p`), details unknown.
- Monetization and cosmetics: Candy Store (candies from games); courier gacha and exchange; courier XP levels (2019-11-19). DAC Custom Game Pass (2019-05-31) with piece-rework votes and daily quests. Seasons with Queen rewards (e.g. "320 Candies + Crown + Legendary Courier"). Candy Store shut down 2023-08-07 (S1).
- Courier abilities seen in code/notes: pick/move piece, recall, sell, buy XP, reroll, lock, Post Piece (2x4), Dismiss (2020-05-27), relic/item slots.

- SSR pieces: alternate versions of some pieces (Chaos Knight Hellfire, Necrophos Corruption, and SSR Enigma, Juggernaut, Lich, Sand King in code) with stats comparable to 3-star units. In 2019 code a shop slot offers an SSR when two 1-10000 rolls both land on 1 at courier level 7+. They are outside the pool (S3, S5).
- Io: 5-cost Elf with no class and no 3-star form. It substitutes for any piece in a combine. 0.3% chance per shop slot, outside the pool (S3, S5).
- Pandaren "recluse" pieces (Storm, Ember, Earth, Void Spirit) are not sold in the shop; they come only from the Pandaren invite trait (S1).

## Chess pieces

Stats come from `npc_units_custom.txt`; abilities from the Lua `chess_ability_list` plus `npc_abilities_custom.txt` in each snapshot. Ability "values" list every variable in the data block: one number applies to all stars, three numbers are 1/2/3-star. Abilities scripted only in Lua have no data block and show "numbers unknown". Tables list in-pool pieces only; SSR and transformed forms are in the JSON.

### Roster and stats, snapshot 2019-02-13 (55 pieces in pool)

| Cost | Piece | Races | Classes | HP (1/2/3 star) | Armor | MR % | Damage (1/2/3 star) | Atk interval | Range | Mana | Ability |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Anti-Mage | elf | demonhunter | 500/1000/2000 | 5 | 10/20/30 | 45-55/90-110/180-220 | 1.2 | 205 | 0 | Mana Break |
| 1 | Axe | orc | warrior | 700/1400/2800 | 5 | 0 | 50-55/100-110/200-220 | 1.5 | 205 | 100 | Berserker's Call |
| 1 | Bat Rider | troll | knight | 500/1000/2000 | 5 | 0 | 40-55/80-110/160-220 | 1.3 | 400 | 100 | Sticky Napalm |
| 1 | Bounty Hunter | goblin | assassin | 550/1100/2200 | 5 | 0 | 50-70/100-140/200-280 | 1 | 205 | 100 | Shuriken Toss |
| 1 | Clockwerk | goblin | mech | 700/1400/2800 | 5/7/9 | 0 | 45-55/90-110/180-220 | 1.4 | 205 | 100 | Battery Assault |
| 1 | Drow Ranger | undead | hunter | 400/800/1600 | 5 | 0 | 45-50/90-100/180-200 | 1.3 | 600 | 0 | Marksmanship |
| 1 | Enchantress | beast | druid | 400/800/1500 | 5 | 0 | 50-60/100-120/200-240 | 1.5 | 400 | 100 | Nature's Attendants |
| 1 | Ogre Magi | ogre | mage | 700/1400/2800 | 5 | 0 | 55-65/110-130/220-260 | 1.4 | 205 | 100 | Bloodlust |
| 1 | Shadow Shaman | troll | shaman | 550/1100/2200 | 5 | 0 | 40-50/80-100/160-200 | 1.6 | 400 | 100 | Hex |
| 1 | Tinker | goblin | mech | 500/1000/2000 | 10 | 0 | 40-50/80-100/160-200 | 1.5 | 600 | 100 | Heat Seeking Missile |
| 1 | Tiny | element | warrior | 650/1300/2600 | 5 | 0 | 60-100/120-200/240-400 | 1.9 | 205 | 100 | Toss |
| 1 | Tusk | beast | warrior | 650/1300/2600 | 5 | 0 | 50-55/100-110/200-220 | 1.2 | 205 | 100 | Walrus PUNCH! |
| 2 | Beastmaster | orc | hunter | 600/1200/2400 | 5 | 0 | 60-70/120-140/240-280 | 1.1 | 205 | 100 | Wild Axes |
| 2 | Chaos Knight | demon | knight | 750/1500/3000 | 5 | 0 | 40-110/80-220/160-440 | 1.3 | 205 | 100 | Chaos Bolt |
| 2 | Crystal Maiden | human | mage | 450/900/1800 | 5 | 0 | 40-45/80-90/160-180 | 1.7 | 600 | 0 | Arcane Aura |
| 2 | Furion | elf | druid | 500/1000/1900 | 0 | 0 | 45-50/90-100/180-200 | 1.4 | 400 | 100 | Nature's Call |
| 2 | Juggernaut | orc | warrior | 600/1200/2400 | 5 | 0 | 65-70/130-140/260-280 | 1.1 | 205 | 100 | Blade Fury |
| 2 | Luna | elf | knight | 500/1000/2000 | 5 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 0 | Moon Glaives |
| 2 | Morphling | element | assassin | 500/1000/2000 | 5 | 0 | 45-60/90-120/180-240 | 1.3 | 400 | 100 | Wave Form |
| 2 | Puck | elf, dragon | mage | 450/900/1800 | 5 | 0 | 40-50/80-100/160-200 | 1.7 | 400 | 100 | Illusory Orb |
| 2 | Queen of Pain | demon | assassin | 550/1100/2200 | 0 | 0 | 55-60/110-120/220-240 | 1.2 | 400 | 100 | Scream of Pain |
| 2 | Slardar | naga | warrior | 650/1300/2600 | 5 | 0 | 70-100/140-200/280-400 | 1.2 | 205 | 100 | Corrosive Haze |
| 2 | Timbersaw | goblin | mech | 800/1600/3200 | 6/8/10 | 0 | 55-60/110-120/220-240 | 1.4 | 205 | 100 | Whirling Death |
| 2 | Treant Protector | elf | druid | 650/1300/2300 | 5 | 0 | 60-90/120-180/240-360 | 2 | 205 | 100 | Leech Seed |
| 2 | Witch Doctor | troll | warlock | 550/1100/2200 | 5 | 0 | 40-50/80-100/160-200 | 1.4 | 400 | 100 | Paralyzing Cask |
| 3 | Abaddon | undead | knight | 750/1500/3000 | 10 | 0 | 45-55/90-110/180-220 | 1.3 | 205 | 100 | Aphotic Shield |
| 3 | Lina | human | mage | 550/1100/2200 | 5 | 0 | 50-55/100-110/200-220 | 1.6 | 600 | 100 | Laguna Blade |
| 3 | Lycan | human, beast | warrior | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 205 | 100 | Transform |
| 3 | Omniknight | human | knight | 700/1400/2800 | 10 | 0 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Purification |
| 3 | Phantom Assassin | elf | assassin | 550/1100/2200 | 5 | 0 | 70-90/140-180/280-360 | 1.2 | 205 | 0 | Coup de Grace |
| 3 | Razor | element | mage | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 400 | 100 | Plasma Field |
| 3 | Sand King | beast | assassin | 550/1100/2200 | 10 | 0 | 45-60/90-120/180-240 | 1.4 | 205 | 100 | Burrowstrike |
| 3 | Shadow Fiend | demon | warlock | 450/900/1800 | 5 | 0 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | Requiem of Souls |
| 3 | Slark | naga | assassin | 500/1000/2000 | 5 | 0 | 50-60/100-120/200-240 | 1 | 205 | 100 | unknown |
| 3 | Sniper | dwarf | hunter | 450/900/1800 | 5 | 0 | 65-85/130-170/260-340 | 1.2 | 600 | 100 | Assassinate |
| 3 | TerrorBlade | demon | demonhunter | 650/1300/2600 | 5 | 10/20/30 | 45-65/90-130/180-260 | 1.4 | 205 | 100 | Metamorphosis |
| 3 | Venomancer | beast | warlock | 500/1000/2000 | 0 | 20 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Plague Ward |
| 3 | Viper | dragon | assassin | 550/1100/2200 | 5 | 20 | 50-60/100-120/200-240 | 1 | 400 | 100 | Viper Strike |
| 3 | Windranger | elf | hunter | 550/1100/2200 | 5 | 0 | 60-65/120-130/240-260 | 0.9 | 600 | 100 | Powershot |
| 4 | Alchemist | goblin | warlock | 1000/2000/4000 | 5 | 0 | 50-70/100-140/200-280 | 1 | 205 | 100 | unknown |
| 4 | Disruptor | orc | shaman | 700/1400/2800 | 5 | 0 | 45-50/90-100/180-200 | 1.5 | 600 | 100 | Static Storm |
| 4 | Doom | demon | warrior | 1050/2100/4200 | 5 | 0 | 110-120/220-240/440-480 | 1.3 | 205 | 100 | Doom |
| 4 | Dragon Knight | human, dragon | knight | 800/1600/3200 | 10 | 0 | 50-60/100-120/200-240 | 1.2 | 205 | 100 | Elder Dragon Form |
| 4 | Keeper of the Light | human | mage | 650/1300/2600 | 5 | 0 | 40-45/80-90/160-180 | 1.7 | 600 | 100 | Illuminate |
| 4 | Kunkka | human | warrior | 950/1900/3800 | 10 | 0 | 75-90/150-180/300-360 | 1.4 | 205 | 100 | Ghost Ship |
| 4 | Lone Druid | beast | druid | 800/1600/3000 | 0 | 20 | 50-60/100-120/200-240 | 1.3 | 400 | 100 | Spirit Bear |
| 4 | Medusa | naga | hunter | 750/1500/3000 | 5 | 10 | 50-60/100-120/200-240 | 1 | 600 | 100 | Stone Gaze |
| 4 | Necrophos | undead | warlock | 850/1700/3400 | 5 | 0 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Death Pulse |
| 4 | Templar Assassin | elf | assassin | 750/1500/3000 | 5 | 0 | 90-110/180-220/360-440 | 1.2 | 400 | 100 | Refraction |
| 4 | Troll Warlord | troll | warrior | 900/1800/3600 | 5 | 0 | 90-100/180-200/360-400 | 1.1 | 400 | 0 | Fervor |
| 5 | Enigma | element | warlock | 1000/2000/4000 | 5 | 40 | 45-55/90-110/180-220 | 1.5 | 400 | 100 | Midnight Pulse |
| 5 | Gyrocopter | dwarf | mech | 900/1800/3600 | 15 | 0 | 75-80/150-160/300-320 | 0.9 | 400 | 100 | Call Down |
| 5 | Lich | undead | mage | 850/1700/3400 | 5 | 20 | 40-45/80-90/160-180 | 1.5 | 400 | 100 | Chain Frost |
| 5 | Techies | goblin | mech | 1000/2000/4000 | 10 | 0 | 45-55/90-110/180-220 | 1.5 | 600 | 100 | Explosive Bomb |
| 5 | Tidehunter | naga | hunter | 950/1900/3800 | 5 | 0 | 45-55/90-110/180-220 | 2 | 205 | 100 | Ravage |

### Roster and stats, snapshot 2019-06-05 (59 pieces in pool)

| Cost | Piece | Races | Classes | HP (1/2/3 star) | Armor | MR % | Damage (1/2/3 star) | Atk interval | Range | Mana | Ability |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Anti-mage | elf | demonhunter | 500/1000/2000 | 5 | 10/20/30 | 45-55/90-110/180-220 | 1.2 | 205 | 0 | Mana Break |
| 1 | Axe | orc | warrior | 700/1400/2800 | 5 | 0 | 50-55/100-110/200-220 | 1.5 | 205 | 100 | Berserker's Call |
| 1 | Batrider | troll | knight | 500/1000/2000 | 5 | 0 | 40-55/80-110/160-220 | 1.3 | 400 | 100 | Sticky Napalm |
| 1 | Bounty Hunter | goblin | assassin | 550/1100/2200 | 5 | 0 | 50-70/100-140/200-280 | 1.1 | 205 | 100 | Shuriken Toss |
| 1 | Clockwerk | goblin | mech | 700/1400/2800 | 5/7/9 | 0 | 45-55/90-110/180-220 | 1.4 | 205 | 100 | Battery Assault |
| 1 | Drow Ranger | undead | hunter | 400/800/1600 | 5 | 0 | 45-50/90-100/180-200 | 1.3 | 600 | 0 | Marksmanship |
| 1 | Enchantress | beast | druid | 400/800/1500 | 5 | 0 | 50-60/100-120/200-240 | 1.5 | 400 | 100 | Nature's Attendants |
| 1 | Mars | god | warrior | 650/1300/2600 | 5/7/9 | 0 | 0-0/0-0/0-0 | 1.4 | 205 | 0 | Bulwark |
| 1 | Shadow Shaman | troll | shaman | 550/1100/2200 | 5 | 0 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Hex |
| 1 | Tinker | goblin | mech | 500/1000/2000 | 5 | 0 | 40-50/80-100/160-200 | 1.5 | 600 | 100 | Heat Seeking Missile |
| 1 | Tiny | element | warrior | 650/1300/2600 | 5 | 0 | 60-100/120-200/240-400 | 1.9 | 205 | 100 | Toss |
| 1 | Tusk | beast | warrior | 650/1300/2600 | 5 | 0 | 50-55/100-110/200-220 | 1.2 | 205 | 100 | Walrus PUNCH! |
| 1 | Winter Wyvern | dragon | mage | 550/1100/2200 | 0 | 10 | 40-55/80-110/160-220 | 1.4 | 600 | 100 | Cold Embrace |
| 2 | Beastmaster | orc | hunter | 600/1200/2400 | 5 | 0 | 60-70/120-140/240-280 | 1.3 | 205 | 100 | Wild Axes |
| 2 | Chaos Knight | demon | knight | 700/1400/2800 | 5 | 0 | 40-110/80-220/160-440 | 1.3 | 205 | 100 | Chaos Bolt |
| 2 | Crystal Maiden | human | mage | 450/900/1800 | 5 | 0 | 40-45/80-90/160-180 | 1.7 | 600 | 0 | Arcane Aura |
| 2 | Furion | elf | druid | 500/1000/1900 | 0 | 0 | 45-50/90-100/180-200 | 1.4 | 400 | 100 | Nature's Call |
| 2 | Juggernaut | orc | warrior | 600/1200/2400 | 5 | 0 | 65-70/130-140/260-280 | 1.1 | 205 | 100 | Blade Fury |
| 2 | Lich | undead | mage | 550/1100/2200 | 5 | 0 | 40-45/80-90/160-180 | 1.5 | 400 | 100 | Frost Shield |
| 2 | Luna | elf | knight | 500/1000/2000 | 5 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 0 | Moon Glaives |
| 2 | Mirana | elf | hunter | 600/1200/2400 | 0 | 0 | 50-70/100-140/200-280 | 1 | 600 | 100 | Sacred Arrow |
| 2 | Morphling | element | assassin | 550/1100/2200 | 5 | 0 | 50-65/100-130/200-260 | 1.3 | 400 | 100 | Waveform |
| 2 | Puck | elf, dragon | mage | 450/900/1800 | 5 | 0 | 40-50/80-100/160-200 | 1.7 | 400 | 100 | Illusory Orb |
| 2 | Queen of Pain | demon | assassin | 550/1100/2200 | 0 | 0 | 55-60/110-120/220-240 | 1.2 | 400 | 100 | Scream of Pain |
| 2 | Slardar | naga | warrior | 650/1300/2600 | 5 | 0 | 70-100/140-200/280-400 | 1.2 | 205 | 100 | Corrosive Haze |
| 2 | Timbersaw | goblin | mech | 800/1600/3200 | 6/8/10 | 0 | 55-60/110-120/220-240 | 1.4 | 205 | 100 | Whirling Death |
| 2 | Witch Doctor | troll | warlock | 550/1100/2200 | 5 | 0 | 40-50/80-100/160-200 | 1.4 | 400 | 100 | Paralyzing Cask |
| 3 | Abaddon | undead | knight | 750/1500/3000 | 10 | 0 | 45-55/90-110/180-220 | 1.3 | 205 | 100 | Aphotic Shield |
| 3 | Dazzle | troll | priest | 550/1100/2200 | 5 | 0 | 50-75/100-150/200-300 | 1.4 | 400 | 100 | Shallow Grave |
| 3 | Lina | human | mage | 600/1200/2400 | 5 | 0 | 50-55/100-110/200-220 | 1.3 | 600 | 100 | Laguna Blade |
| 3 | Lycan | human, beast | warrior | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 205 | 100 | Shapeshift |
| 3 | Omniknight | human | knight | 700/1400/2800 | 10 | 0 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Purification |
| 3 | Phantom Assassin | elf | assassin | 550/1100/2200 | 5 | 0 | 70-100/140-200/280-400 | 1.2 | 205 | 0 | Coup de Grace |
| 3 | Razor | element | mage | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 400 | 100 | Plasma Field |
| 3 | Shadow Fiend | demon | warlock | 450/900/1800 | 5 | 0 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | Requiem of Souls |
| 3 | Sniper | dwarf | hunter | 450/900/1800 | 5 | 0 | 65-85/130-170/260-340 | 1.1 | 600 | 100 | Assassinate |
| 3 | Terrorblade | demon | demonhunter | 700/1400/2800 | 5 | 10/20/30 | 55-65/110-130/220-260 | 1.4 | 205 | 100 | Metamorphosis |
| 3 | Treant Protector | elf | druid | 750/1500/2900 | 5 | 0 | 80-90/160-180/320-360 | 2 | 205 | 100 | Leech Seed |
| 3 | Venomancer | beast | warlock | 600/1200/2400 | 0 | 20 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Plague Ward |
| 3 | Viper | dragon | assassin | 550/1100/2200 | 5 | 20 | 50-60/100-120/200-240 | 1 | 400 | 100 | Viper Strike |
| 3 | Windranger | elf | hunter | 550/1100/2200 | 5 | 0 | 55-65/110-130/220-260 | 0.9 | 600 | 100 | Powershot |
| 4 | Alchemist | goblin, ogre | warlock | 1000/2000/4000 | 0 | 0 | 50-80/100-160/200-320 | 1.3 | 205 | 100 | Chemecal Rage |
| 4 | Disruptor | orc | shaman | 700/1400/2800 | 5 | 0 | 45-50/90-100/180-200 | 1.5 | 600 | 100 | Static Storm |
| 4 | Doom | demon | warrior | 1050/2100/4200 | 5 | 0 | 110-120/220-240/440-480 | 1.3 | 205 | 100 | Doom |
| 4 | Dragon Knight | human, dragon | knight | 800/1600/3200 | 10 | 0 | 50-60/100-120/200-240 | 1.2 | 205 | 100 | Elder Dragon Form |
| 4 | Keeper of the Light | human | mage | 650/1300/2600 | 5 | 0 | 40-45/80-90/160-180 | 1.7 | 600 | 100 | Illuminate |
| 4 | Kunkka | human | warrior | 950/1900/3800 | 10 | 0 | 75-90/150-180/300-360 | 1.3 | 205 | 100 | Ghost Ship |
| 4 | Lone Druid | beast | druid | 800/1600/3000 | 0 | 20 | 50-60/100-120/200-240 | 1.3 | 400 | 100 | Summon Spirit Bear |
| 4 | Medusa | naga | hunter | 750/1500/3000 | 5 | 10 | 50-70/100-140/200-280 | 1 | 600 | 100 | Stone Gaze |
| 4 | Necrophos | undead | warlock | 850/1700/3400 | 5 | 0 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Death Pulse |
| 4 | Templar Assassin | elf | assassin | 750/1500/3000 | 5 | 0 | 90-110/180-220/360-440 | 1.1 | 400 | 100 | Refraction |
| 4 | Troll Warlord | troll | warrior | 900/1800/3600 | 5 | 0 | 90-100/180-200/360-400 | 1.1 | 400 | 0 | Fervor |
| 5 | Death Prophet | undead | warlock | 1000/2000/4000 | 5 | 30 | 50-60/100-120/200-240 | 1.4 | 400 | 100 | Exorcism |
| 5 | Enigma | element | warlock | 1000/2000/4000 | 5 | 40 | 45-55/90-110/180-220 | 1.5 | 400 | 100 | Midnight Pulse |
| 5 | Gyrocopter | dwarf | mech | 900/1800/3600 | 15 | 0 | 75-80/150-160/300-320 | 0.9 | 400 | 100 | Call Down |
| 5 | Sven | demon | warrior | 1200/2400/4800 | 10 | 0 | 100-150/200-300/400-600 | 1.3 | 205 | 100 | God's Strength |
| 5 | Techies | goblin | mech | 1000/2000/4000 | 10 | 0 | 45-55/90-110/180-220 | 1.5 | 600 | 100 | Explosive Bomb |
| 5 | Tidehunter | naga | hunter | 950/1900/3800 | 5 | 0 | 45-55/90-110/180-220 | 2 | 205 | 100 | Ravage |
| 5 | Zeus | god | mage | 950/1900/3800 | 0 | 40 | 50-70/100-140/200-280 | 1.4 | 400 | 100 | Thundergod's Wrath |

### Roster and stats, snapshot 2020-07-02 (76 pieces in pool)

| Cost | Piece | Races | Classes | HP (1/2/3 star) | Armor | MR % | Damage (1/2/3 star) | Atk interval | Range | Mana | Ability |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Anti-mage | elf | demonhunter | 500/1000/2000 | 4 | 20 | 45-55/90-110/180-220 | 1.2 | 205 | 0 | Mana Break |
| 1 | Axe | orc | warrior | 750/1500/3000 | 4 | 0 | 50-55/100-110/200-220 | 1.5 | 205 | 100 | Berserker's Call |
| 1 | Bounty Hunter | goblin | assassin | 550/1100/2200 | 1 | 0 | 50-70/100-140/200-280 | 1.1 | 205 | 100 | Shuriken Toss |
| 1 | Clockwerk | goblin | mech | 750/1500/3000 | 4 | 0 | 45-55/90-110/180-220 | 1.4 | 205 | 100 | Battery Assault |
| 1 | Crystal Maiden | human | mage | 450/900/1800 | 1 | 5 | 40-45/80-90/160-180 | 1.7 | 600 | 0 | Arcane Aura |
| 1 | Dark Willow | elf | wizard | 500/1000/2000 | 1 | 0 | 40-60/80-120/160-240 | 1.2 | 400 | 100 | none |
| 1 | Drow Ranger | undead | hunter | 400/800/1600 | 2 | 0 | 45-50/90-100/180-200 | 1.3 | 600 | 0 | Precision Aura |
| 1 | Enchantress | beast | druid | 400/800/1500 | 2 | 0 | 50-60/100-120/200-240 | 1.5 | 400 | 100 | Nature's Attendants |
| 1 | Mars | god | warrior | 700/1400/2800 | 4 | 0 | 0-0/0-0/0-0 | 1.4 | 205 | 0 | Bulwark |
| 1 | Shadow Shaman | troll | shaman | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Hex |
| 1 | Tinker | goblin | mech | 500/1000/2000 | 3 | 0 | 40-50/80-100/160-200 | 1.5 | 600 | 100 | Heat Seeking Missile |
| 1 | Tiny | element | warrior | 550/1350/3400 | 3 | 0 | 60-100/120-200/240-400 | 1.9 | 205 | 100 | Toss |
| 1 | Tusk | beast | warrior | 650/1300/2600 | 2 | 0 | 50-55/100-110/200-220 | 1.2 | 205 | 100 | Walrus PUNCH! |
| 1 | Winter Wyvern | undead, dragon | mage | 550/1100/2200 | 0 | 10 | 40-55/80-110/160-220 | 1.4 | 600 | 100 | Cold Embrace |
| 1 | Witch Doctor | troll | warlock | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Paralyzing Cask |
| 2 | Abaddon | undead | knight | 700/1400/2800 | 5 | 0 | 45-55/90-110/180-220 | 1.3 | 205 | 100 | Aphotic Shield |
| 2 | Batrider | troll | knight | 550/1100/2200 | 4 | 0 | 40-55/80-110/160-220 | 1.3 | 400 | 100 | Firefly |
| 2 | Beastmaster | orc | hunter | 650/1300/2600 | 6 | 0 | 60-70/120-140/240-280 | 1.3 | 205 | 100 | Call of the Wild |
| 2 | Bloodseeker | orc | shaman | 600/1200/2400 | 3 | 10 | 55-60/110-120/220-240 | 1.1 | 205 | 100 | Blood Rite |
| 2 | Brewmaster | pandaman | monk | 800/1600/3200 | 4 | 0 | 50-60/100-120/200-240 | 1.3 | 205 | 100 | none |
| 2 | Chaos Knight | demon | knight | 700/1400/2800 | 6 | 0 | 40-110/80-220/160-440 | 1.4 | 205 | 100 | Chaos Bolt |
| 2 | Dazzle | troll | priest | 500/1000/2000 | 2 | 10 | 40-55/80-110/160-220 | 1.4 | 400 | 100 | Shallow Grave |
| 2 | Furion | elf | druid | 500/1000/1900 | 4 | 5 | 45-50/90-100/180-200 | 1.4 | 400 | 100 | Nature's Call |
| 2 | Juggernaut | orc | warrior | 600/1200/2400 | 4 | 0 | 65-70/130-140/260-280 | 1.1 | 205 | 100 | Blade Fury |
| 2 | Mirana | elf | hunter | 600/1200/2400 | 0 | 0 | 50-70/100-140/200-280 | 1 | 600 | 100 | Sacred Arrow |
| 2 | Morphling | element | assassin | 550/1100/2200 | 3 | 0 | 50-65/100-130/200-260 | 1.3 | 400 | 100 | Waveform |
| 2 | Ogre Magi | ogre | mage | 850/1700/3400 | 3 | 0 | 55-75/110-150/220-300 | 1.4 | 205 | 100 | Multicast |
| 2 | Oracle | god | priest | 500/1000/2000 | 2 | 10 | 40-50/80-100/160-200 | 1.6 | 400 | 100 | Fate's Edict |
| 2 | Slark | naga | assassin | 500/1000/2000 | 3 | 0 | 50-60/100-120/200-240 | 0.9 | 205 | 0 | Essence Shift |
| 2 | Sniper | dwarf | hunter | 450/900/1800 | 4 | 0 | 65-75/130-150/260-300 | 1.1 | 600 | 100 | Assassinate |
| 2 | Timbersaw | goblin | mech | 800/1600/3200 | 5 | 0 | 55-60/110-120/220-240 | 1.4 | 205 | 100 | Whirling Death |
| 3 | Legion Commander | human | knight | 700/1400/2800 | 5 | 0 | 50-70/100-140/200-280 | 1.2 | 205 | 0 | none |
| 3 | Lina | human | mage | 600/1200/2400 | 3 | 10 | 50-55/100-110/200-220 | 1.3 | 600 | 100 | Laguna Blade |
| 3 | Lycan | human, beast | warrior | 750/1500/3000 | 6 | 0 | 50-60/100-120/200-240 | 1.1 | 205 | 100 | Shapeshift |
| 3 | Omniknight | human | knight | 700/1400/2800 | 7 | 0 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Purification |
| 3 | Phantom Assassin | elf | assassin | 550/1100/2200 | 4 | 0 | 70-100/140-200/280-400 | 1.2 | 205 | 0 | Coup de Grace |
| 3 | Pudge | undead | warrior | 850/1700/3400 | 4 | 30 | 70-110/140-220/280-440 | 2 | 205 | 100 | Meat Hook |
| 3 | Razor | element | mage | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 400 | 100 | Plasma Field |
| 3 | Rubick | god | wizard | 600/1200/2400 | 2 | 20 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Spell Steal |
| 3 | Sand King | aqir | assassin | 600/1200/2400 | 6 | 0 | 45-60/90-120/180-240 | 1.4 | 205 | 100 | Burrowstrike |
| 3 | Shadow Fiend | demon | warlock | 500/1000/2000 | 3 | 20 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | Requiem of Souls |
| 3 | Slardar | naga | warrior | 750/1500/3000 | 5 | 0 | 80-110/160-220/320-440 | 1.5 | 205 | 100 | unknown |
| 3 | Terrorblade | demon | demonhunter | 700/1400/2800 | 4 | 10 | 55-65/110-130/220-260 | 1.2 | 205 | 100 | Metamorphosis |
| 3 | Treant Protector | elf | druid | 750/1500/2800 | 7 | 0 | 80-90/160-180/320-360 | 2 | 205 | 100 | Leech Seed |
| 3 | Venomancer | beast, aqir | warlock | 550/1100/2200 | 4 | 20 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Plague Ward |
| 3 | Viper | dragon | assassin | 550/1100/2200 | 5 | 20 | 50-60/100-120/200-240 | 1 | 400 | 100 | Viper Strike |
| 3 | Visage | undead, dragon | hunter | 550/1100/2200 | 5 | 30 | 55-75/110-150/220-300 | 1.4 | 600 | 100 | Summon Familiars |
| 4 | Alchemist | goblin, ogre | warlock | 1000/2000/4000 | 4 | 0 | 50-80/100-160/200-320 | 1.3 | 205 | 100 | Chemecal Rage |
| 4 | Broodmother | aqir | hunter | 900/1800/3600 | 10 | 5 | 80-110/160-220/320-440 | 1.2 | 205 | 100 | Spin Web |
| 4 | Chen | orc | priest | 800/1600/3200 | 6 | 10 | 40-60/80-120/160-240 | 1.4 | 400 | 100 | Holy Persuasion |
| 4 | Doom | demon | warrior | 1050/2100/4200 | 8 | 0 | 110-120/220-240/440-480 | 1.3 | 205 | 100 | Doom |
| 4 | Dragon Knight | human, dragon | knight | 800/1600/3200 | 10 | 0 | 60-80/120-160/240-320 | 1.4 | 205 | 100 | Elder Dragon Form |
| 4 | Grimstroke | demon | wizard | 750/1500/3000 | 3 | 20 | 70-90/140-180/280-360 | 1.4 | 400 | 100 | Stroke of Fate |
| 4 | Keeper of the Light | human | mage | 650/1300/2600 | 3 | 20 | 40-45/80-90/160-180 | 1.7 | 600 | 100 | Illuminate |
| 4 | Kunkka | human | warrior | 950/1900/3800 | 12/12/10 | 0 | 80-100/160-200/320-400 | 1.3 | 205 | 100 | Ghostship |
| 4 | Lone Druid | beast | druid | 800/1600/3000 | 7 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 100 | Summon Spirit Bear |
| 4 | Medusa | naga | hunter | 750/1500/3000 | 7 | 10 | 60-90/120-180/240-360 | 1 | 600 | 100 | Stone Gaze |
| 4 | Necrophos | undead | warlock | 800/1600/3200 | 3 | 30 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Death Pulse |
| 4 | Nyx Assassin | aqir, undead | assassin | 700/1400/2800 | 10 | 0 | 45-90/90-180/180-360 | 1.3 | 205 | 100 | unknown |
| 4 | Templar Assassin | elf | assassin | 750/1500/3000 | 6 | 0 | 90-110/180-220/360-440 | 1.1 | 400 | 100 | Refraction |
| 4 | Troll Warlord | troll | warrior | 900/1800/3600 | 8 | 0 | 90-100/180-200/360-400 | 1.1 | 400 | 0 | Fervor |
| 4 | Windranger | elf | hunter | 600/1200/2400 | 5 | 5 | 60-90/120-180/240-360 | 0.9 | 600 | 100 | Power Shackle Shot |
| 5 | Death Prophet | undead | warlock | 1000/2000/4000 | 5 | 40 | 50-60/100-120/200-240 | 1.4 | 400 | 100 | Exorcism |
| 5 | Disruptor | orc | shaman | 1000/2000/4000 | 6 | 20 | 50-70/100-140/200-280 | 1.5 | 600 | 100 | Static Storm |
| 5 | Enigma | element | warlock | 1000/2000/4000 | 6 | 30 | 45-55/90-110/180-220 | 1.5 | 400 | 100 | Midnight Pulse |
| 5 | Gyrocopter | dwarf | mech | 900/1800/3600 | 12 | 0 | 75-100/150-200/300-400 | 0.9 | 400 | 100 | Call Down |
| 5 | Huskar | troll | warrior | 1150/2300/4600 | 9 | 10 | 80-100/160-200/320-400 | 1.1 | 400 | 0 | Berserker's Blood |
| 5 | Invoker | elf | mage | 1000/2000/4000 | 5 | 20 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | unknown |
| 5 | Jakiro | dragon | mage | 1000/2000/4000 | 9 | 20 | 30-100/60-200/120-400 | 1.3 | 400 | 100 | none |
| 5 | Monkey King | beast | monk | 1100/2200/4400 | 10 | 5 | 80-145/160-290/320-580 | 1.2 | 205 | 0 | Jingu Mastery |
| 5 | Queen of Pain | demon | assassin | 950/1900/3800 | 6 | 20 | 75-100/150-200/300-400 | 1.2 | 400 | 100 | Sonic Wave |
| 5 | Snapfire | goblin | knight | 1100/2200/4400 | 12 | 0 | 70-120/140-240/280-480 | 1.3 | 600 | 100 | Lil' Shredder |
| 5 | Sven | demon | warrior | 1200/2400/4800 | 10 | 0 | 100-150/200-300/400-600 | 1.6 | 205 | 100 | God's Strength |
| 5 | Techies | goblin | mech | 1000/2000/4000 | 10 | 0 | 45-55/90-110/180-220 | 1.5 | 600 | 100 | Explosive Bomb |
| 5 | Tidehunter | naga | hunter | 1000/2000/4000 | 11 | 15 | 65-75/130-150/260-300 | 2 | 205 | 100 | Ravage |
| 5 | Zeus | god | mage | 950/1900/3800 | 7 | 40 | 50-70/100-140/200-280 | 1.4 | 400 | 100 | Thundergod's Wrath |

### Roster and stats, snapshot 2021-02-07 (79 pieces in pool)

| Cost | Piece | Races | Classes | HP (1/2/3 star) | Armor | MR % | Damage (1/2/3 star) | Atk interval | Range | Mana | Ability |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Anti-mage | elf | demonhunter | 500/1000/2000 | 4 | 20 | 45-55/90-110/180-220 | 1.2 | 205 | 0 | Mana Break |
| 1 | Axe | orc | warrior | 750/1500/3000 | 4 | 0 | 50-70/100-140/200-280 | 1.5 | 205 | 100 | Berserker's Call |
| 1 | Bounty Hunter | goblin | assassin | 550/1100/2200 | 1 | 0 | 50-70/100-140/200-280 | 1 | 205 | 100 | Shuriken Toss |
| 1 | Clockwerk | goblin | mech | 750/1500/3000 | 4 | 0 | 45-55/90-110/180-220 | 1.4 | 205 | 100 | Battery Assault |
| 1 | Crystal Maiden | human | mage | 450/900/1800 | 1 | 5 | 40-45/80-90/160-180 | 1.7 | 600 | 0 | Arcane Aura |
| 1 | Drow Ranger | undead | hunter | 400/800/1600 | 2 | 0 | 45-50/90-100/180-200 | 1.3 | 600 | 0 | Precision Aura |
| 1 | Enchantress | beast | druid | 400/800/1500 | 2 | 10 | 50-60/100-120/200-240 | 1.5 | 400 | 100 | Nature's Attendants |
| 1 | Luna | elf | knight | 500/1000/2000 | 1 | 0 | 40-50/80-100/160-200 | 1.3 | 400 | 0 | Moon Glaives |
| 1 | Mars | god | warrior | 700/1400/2800 | 4 | 0 | 50-80/100-160/200-320 | 1.9 | 205 | 0 | Bulwark |
| 1 | Oracle | god | priest | 450/900/1800 | 1 | 10 | 40-50/80-100/160-200 | 1.6 | 400 | 100 | Fate's Edict |
| 1 | Shadow Shaman | troll | shaman | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Shackles |
| 1 | Spirit Breaker | tauren | assassin | 650/1300/2600 | 3 | 0 | 50-65/100-130/200-260 | 1.3 | 205 | 0 | Greater Bash |
| 1 | Tinker | goblin | mech | 500/1000/2000 | 3 | 0 | 40-50/80-100/160-200 | 1.5 | 600 | 100 | Heat Seeking Missile |
| 1 | Tiny | element | warrior | 550/1350/3400 | 3 | 0 | 60-100/120-200/240-400 | 1.9 | 205 | 100 | Toss |
| 1 | Tusk | beast | warrior | 650/1300/2600 | 2 | 0 | 50-55/100-110/200-220 | 1.3 | 205 | 100 | Walrus PUNCH! |
| 1 | Vengeful Spirit | undead | demonhunter | 500/1000/2000 | 2 | 0 | 40-60/80-120/160-240 | 1.3 | 400 | 100 | Nether Swap |
| 1 | Witch Doctor | troll | warlock | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Paralyzing Cask |
| 2 | Abaddon | undead | knight | 700/1400/2800 | 4 | 0 | 45-55/90-110/180-220 | 1.3 | 205 | 100 | Aphotic Shield |
| 2 | Batrider | troll | knight | 550/1100/2200 | 4 | 0 | 40-55/80-110/160-220 | 1.3 | 400 | 100 | Firefly |
| 2 | Beastmaster | orc | hunter | 650/1300/2600 | 6 | 0 | 60-70/120-140/240-280 | 1.3 | 205 | 100 | Call of the Wild |
| 2 | Brewmaster | pandaman | monk | 800/1600/3200 | 4 | 0 | 50-60/100-120/200-240 | 1.3 | 205 | 100 | none |
| 2 | Chaos Knight | demon | knight | 700/1400/2800 | 6 | 0 | 40-120/80-240/160-480 | 1.5 | 205 | 100 | Chaos Bolt |
| 2 | Dazzle | troll | priest | 500/1000/2000 | 2 | 10 | 40-55/80-110/160-220 | 1.4 | 400 | 100 | Shallow Grave |
| 2 | Furion | elf | druid | 500/1000/1900 | 4 | 5 | 45-50/90-100/180-200 | 1.4 | 400 | 100 | Nature's Call |
| 2 | Juggernaut | orc | warrior | 600/1200/2400 | 4 | 0 | 65-70/130-140/260-280 | 1.1 | 205 | 100 | Blade Fury |
| 2 | Lion | demon | wizard | 550/1100/2200 | 2 | 5 | 45-55/90-110/180-220 | 1.6 | 600 | 100 | Mana Drain |
| 2 | Mirana | elf | hunter | 600/1200/2400 | 2 | 0 | 50-70/100-140/200-280 | 1 | 600 | 100 | Sacred Arrow |
| 2 | Morphling | element | assassin | 550/1100/2200 | 3 | 0 | 50-65/100-130/200-260 | 1.3 | 400 | 100 | Waveform |
| 2 | Ogre Magi | ogre | mage | 850/1700/3400 | 3 | 0 | 55-75/110-150/220-300 | 1.5 | 205 | 100 | Multicast |
| 2 | Puck | elf, dragon | mage | 500/1000/2000 | 2 | 50 | 40-50/80-100/160-200 | 1.7 | 400 | 100 | Illusory Orb |
| 2 | Slark | naga | assassin | 500/1000/2000 | 3 | 0 | 50-60/100-120/200-240 | 0.9 | 205 | 0 | Essence Shift |
| 2 | Sniper | dwarf | hunter | 450/900/1800 | 4 | 0 | 65-75/130-150/260-300 | 1.1 | 600 | 100 | Assassinate |
| 2 | Timbersaw | goblin | mech | 800/1600/3200 | 7 | 0 | 55-60/110-120/220-240 | 1.4 | 205 | 100 | Whirling Death |
| 2 | Visage | undead, dragon | hunter | 500/1000/2000 | 2 | 20 | 45-65/90-130/180-260 | 1.4 | 600 | 100 | Summon Familiars |
| 3 | Faceless Void | nraqi | assassin | 700/1400/2800 | 5 | 0 | 60-90/120-180/240-360 | 1.1 | 205 | 100 | Chronosphere |
| 3 | Grimstroke | demon | wizard | 650/1300/2600 | 2 | 20 | 50-70/100-140/200-280 | 1.4 | 400 | 100 | Stroke of Fate |
| 3 | Legion Commander | human | knight | 700/1400/2800 | 5 | 0 | 50-80/100-160/200-320 | 1.1 | 205 | 0 | none |
| 3 | Lina | human | mage | 600/1200/2400 | 3 | 10 | 50-55/100-110/200-220 | 1.3 | 600 | 100 | Laguna Blade |
| 3 | Lycan | human, beast | warrior | 750/1500/3000 | 6 | 0 | 50-60/100-120/200-240 | 1.1 | 205 | 100 | Shapeshift |
| 3 | Meepo | kobold | mech | 600/1200/1800 | 5 | 20 | 50-60/100-120/200-240 | 1 | 205 | 100 | Poof |
| 3 | Omniknight | human | knight | 700/1400/2800 | 7 | 0 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Purification |
| 3 | Razor | element | mage | 750/1500/3000 | 5 | 0 | 50-60/100-120/200-240 | 1.1 | 400 | 100 | Plasma Field |
| 3 | Rubick | god | wizard | 600/1200/2400 | 2 | 20 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Spell Steal |
| 3 | Sand King | aqir | assassin | 600/1200/2400 | 6 | 0 | 45-60/90-120/180-240 | 1.4 | 205 | 100 | Burrowstrike |
| 3 | Shadow Fiend | demon | warlock | 500/1000/2000 | 3 | 20 | 70-80/140-160/280-320 | 1.2 | 600 | 100 | Requiem of Souls |
| 3 | Slardar | naga | warrior | 750/1500/3000 | 5 | 0 | 80-110/160-220/320-440 | 1.5 | 205 | 100 | unknown |
| 3 | Terrorblade | demon | demonhunter | 700/1400/2800 | 4 | 10 | 55-65/110-130/220-260 | 1.2 | 205 | 100 | Metamorphosis |
| 3 | Treant Protector | elf | druid | 750/1500/2650 | 7 | 0 | 80-90/160-180/320-360 | 2 | 205 | 100 | Leech Seed |
| 3 | Venomancer | beast, aqir | warlock | 550/1100/2200 | 4 | 20 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Plague Ward |
| 3 | Viper | dragon | assassin | 550/1100/2200 | 5 | 20 | 50-60/100-120/200-240 | 1 | 400 | 100 | Viper Strike |
| 4 | Alchemist | goblin, ogre | warlock | 1000/2000/4000 | 4 | 0 | 50-80/100-160/200-320 | 1.3 | 205 | 100 | Chemecal Rage |
| 4 | Broodmother | aqir | hunter | 900/1800/3600 | 10 | 5 | 80-110/160-220/320-440 | 1.1 | 205 | 100 | Spin Web |
| 4 | Chen | orc | priest | 900/1800/3600 | 6 | 10 | 40-60/80-120/160-240 | 1.4 | 400 | 100 | Holy Persuasion |
| 4 | Doom | demon | warrior | 1050/2100/4200 | 8 | 0 | 110-120/220-240/440-480 | 1.3 | 205 | 100 | Doom |
| 4 | Dragon Knight | human, dragon | knight | 800/1600/3200 | 10 | 0 | 60-80/120-160/240-320 | 1.4 | 205 | 100 | Elder Dragon Form |
| 4 | Earthshaker | tauren | shaman | 700/1400/2800 | 10 | 0 | 45-90/90-180/180-360 | 1.3 | 205 | 100 | Fissure |
| 4 | Keeper of the Light | human | mage | 650/1300/2600 | 3 | 20 | 40-45/80-90/160-180 | 1.7 | 600 | 100 | Illuminate |
| 4 | Kunkka | human | warrior | 950/1900/3800 | 10 | 0 | 80-100/160-200/320-400 | 1.3 | 205 | 100 | Ghostship |
| 4 | Lone Druid | beast | druid | 800/1600/2800 | 7 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 100 | Summon Spirit Bear |
| 4 | Medusa | naga | hunter | 750/1500/3000 | 7 | 10 | 50-80/100-160/200-320 | 1 | 400 | 100 | Stone Gaze |
| 4 | Necrophos | undead | warlock | 800/1600/3200 | 3 | 30 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Death Pulse |
| 4 | Nyx Assassin | aqir, undead | assassin | 700/1400/2800 | 10 | 0 | 45-90/90-180/180-360 | 1.3 | 205 | 100 | Spiked Carapace |
| 4 | Pangolier | beast | assassin | 750/1500/3000 | 8 | 10 | 70-100/140-200/280-400 | 1 | 205 | 100 | Swashbuckle |
| 4 | Windranger | elf | hunter | 600/1200/2400 | 5 | 5 | 60-90/120-180/240-360 | 0.9 | 600 | 100 | Power Shackle Shot |
| 5 | Disruptor | orc | shaman | 1000/2000/4000 | 6 | 20 | 50-70/100-140/200-280 | 1.5 | 600 | 100 | Static Storm |
| 5 | Elder Titan | tauren, god | druid | 1000/2000/3000 | 10 | 20 | 100-130/200-260/400-520 | 1.3 | 205 | 100 | Earth Splitter |
| 5 | Enigma | element | warlock | 950/1900/3800 | 6 | 30 | 45-55/90-110/180-220 | 1.5 | 400 | 100 | Midnight Pulse |
| 5 | Gyrocopter | dwarf | mech | 900/1800/3600 | 12 | 0 | 75-100/150-200/300-400 | 0.9 | 400 | 100 | Call Down |
| 5 | Huskar | troll | warrior | 1150/2300/4600 | 10 | 20 | 100-110/200-220/400-440 | 1 | 400 | 0 | Berserker's Blood |
| 5 | Invoker | elf | mage | 1000/2000/4000 | 5 | 20 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | unknown |
| 5 | Jakiro | dragon | mage | 1000/2000/4000 | 9 | 20 | 30-100/60-200/120-400 | 1.3 | 400 | 100 | none |
| 5 | Monkey King | beast | monk | 1100/2200/4400 | 10 | 5 | 80-145/160-290/320-580 | 1 | 205 | 0 | Jingu Mastery |
| 5 | Queen of Pain | demon | assassin | 950/1900/3800 | 6 | 20 | 75-100/150-200/300-400 | 1.2 | 400 | 100 | Sonic Wave |
| 5 | Snapfire | goblin | knight | 1100/2200/4400 | 12 | 0 | 70-120/140-240/280-480 | 1.3 | 600 | 100 | Lil' Shredder |
| 5 | Techies | goblin | mech | 1000/2000/4000 | 10 | 0 | 45-55/90-110/180-220 | 1.5 | 600 | 100 | Explosive Bomb |
| 5 | Templar Assassin | elf | assassin | 950/1900/3800 | 8 | 0 | 90-120/180-240/360-480 | 1 | 400 | 100 | Refraction |
| 5 | Tidehunter | naga | hunter | 1000/2000/4000 | 11 | 15 | 65-75/130-150/260-300 | 2 | 205 | 100 | Ravage |
| 5 | Warlock | orc | warlock | 900/1800/3600 | 6 | 25 | 60-120/120-240/240-480 | 1.5 | 600 | 100 | Chaotic Offering |
| 5 | Zeus | god | mage | 950/1900/3800 | 7 | 40 | 50-70/100-140/200-280 | 1.4 | 400 | 100 | Thundergod's Wrath |

### Roster and stats, snapshot 2023-04-10 (88 pieces in pool)

| Cost | Piece | Races | Classes | HP (1/2/3 star) | Armor | MR % | Damage (1/2/3 star) | Atk interval | Range | Mana | Ability |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Abaddon | undead | knight | 550/1100/2200 | 4 | 0 | 45-55/90-110/180-220 | 1.3 | 205 | 100 | Aphotic Shield |
| 1 | Anti-mage | elf | demonhunter | 500/1000/2000 | 4 | 40 | 45-55/90-110/180-220 | 1.0 | 205 | 0 | Mana Break |
| 1 | Axe | orc | warrior | 750/1500/3000 | 4 | 0 | 50-70/100-140/200-280 | 1.5 | 205 | 100 | Berserker's Call |
| 1 | Bounty Hunter | goblin | assassin | 500/1000/2000 | 1 | 0 | 50-70/100-140/200-280 | 1 | 205 | 100 | Jinada Toss |
| 1 | Clockwerk | goblin | mech | 700/1400/2800 | 4 | 0 | 45-55/90-110/180-220 | 1.4 | 205 | 100 | Battery Assault |
| 1 | Crystal Maiden | human | mage | 450/900/1800 | 1 | 5 | 40-45/80-90/160-180 | 1.7 | 600 | 0 | Arcane Aura |
| 1 | Dazzle | troll | priest | 450/900/1800 | 1 | 10 | 45-60/90-120/180-240 | 1.2 | 400 | 100 | Shallow Grave |
| 1 | Enchantress | beast, centaur | druid | 400/800/1500 | 2 | 10 | 50-60/100-120/200-240 | 1.5 | 400 | 0 | Untouchable |
| 1 | Hoodwink | beast | hunter | 450/900/1800 | 2 | 0 | 40-60/80-120/160-240 | 1.2 | 400 | 100 | Hunter's Boomerang |
| 1 | Luna | elf | knight | 500/1000/2000 | 1 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 0 | Moon Glaives |
| 1 | Mars | god | warrior | 550/1100/2200 | 4 | 0 | 50-80/100-160/200-320 | 1.9 | 205 | 0 | Bulwark |
| 1 | Mirana | elf | hunter | 450/900/1800 | 2 | 0 | 40-50/80-100/160-200 | 1 | 600 | 100 | Sacred Arrow |
| 1 | Shadow Shaman | troll | shaman | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.4 | 400 | 100 | Croak! |
| 1 | Spirit Breaker | tauren | assassin | 600/1200/2400 | 3 | 0 | 40-60/80-120/160-240 | 1.4 | 205 | 0 | Greater Bash |
| 1 | Tinker | goblin | mech | 500/1000/2000 | 3 | 0 | 40-50/80-100/160-200 | 1.5 | 600 | 100 | Heat Seeking Missile |
| 1 | Tiny | element | warrior | 550/1350/3400 | 3 | 0 | 60-100/120-200/240-400 | 1.9 | 205 | 100 | Toss |
| 1 | Vengeful Spirit | undead | demonhunter | 500/1000/2000 | 2 | 0 | 40-80/80-160/160-320 | 1.3 | 400 | 0 | Nether Swap |
| 1 | Winter Wyvern | undead, dragon | mage | 600/1200/2400 | 0 | 10 | 40-55/80-110/160-220 | 1.4 | 600 | 100 | Cold Embrace |
| 1 | Witch Doctor | troll | warlock | 550/1100/2200 | 0 | 10 | 40-50/80-100/160-200 | 1.5 | 400 | 100 | Paralyzing Cask |
| 2 | Batrider | troll | knight | 550/1100/2200 | 4 | 0 | 40-55/80-110/160-220 | 1.3 | 400 | 100 | Firefly |
| 2 | Beastmaster | orc | hunter | 650/1300/2600 | 6 | 0 | 60-70/120-140/240-280 | 1.4 | 205 | 100 | Call of the Wild |
| 2 | Brewmaster | pandaman | monk | 850/1700/3400 | 5 | 20 | 50-60/100-120/200-240 | 1.3 | 205 | 100 | none |
| 2 | Centaur Warrunner | centaur | warrior | 750/1500/3000 | 4 | 30 | 55-60/110-120/220-240 | 1.1 | 205 | 0 | Double Edge |
| 2 | Chaos Knight | demon | knight | 700/1400/2800 | 6 | 0 | 40-160/80-320/160-640 | 1.5 | 205 | 0 | Chaos Strike |
| 2 | Faceless Void | nraqi | assassin | 600/1200/2400 | 4 | 0 | 50-80/120-180/240-360 | 1.3 | 205 | 100 | Chronosphere |
| 2 | Furion | elf | druid | 500/1000/1900 | 4 | 5 | 45-50/90-100/180-200 | 1.4 | 400 | 100 | Nature's Call |
| 2 | Juggernaut | orc | warrior | 600/1200/2400 | 4 | 0 | 65-70/130-140/260-280 | 1.1 | 205 | 100 | Blade Fury |
| 2 | Lion | demon | wizard | 550/1100/2200 | 2 | 5 | 45-55/90-110/180-220 | 1.6 | 600 | 200 | Mana Drain |
| 2 | Morphling | element | assassin | 550/1100/2200 | 3 | 0 | 50-65/100-130/200-260 | 1.3 | 400 | 100 | Waveform |
| 2 | Naga Siren | naga | wizard | 550/1100/2200 | 4 | 20 | 50-60/100-120/200-240 | 1.2 | 205 | 100 | Song of the Siren |
| 2 | Ogre Magi | ogre | mage | 850/1700/3400 | 3 | 0 | 55-75/110-150/220-300 | 1.5 | 205 | 100 | Multicast |
| 2 | Oracle | god | priest | 550/1100/2200 | 1 | 10 | 40-60/80-120/160-240 | 1.6 | 400 | 100 | Fate's Edict |
| 2 | Puck | elf, dragon | mage | 450/900/1800 | 2 | 50 | 40-50/80-100/160-200 | 1.7 | 400 | 100 | Illusory Orb |
| 2 | Pudge | undead | warrior | 850/1700/3400 | 4 | 20 | 60-90/120-180/240-360 | 2 | 205 | 100 | Meat Hook |
| 2 | Slark | naga | assassin | 500/1000/2000 | 4 | 0 | 50-70/100-140/200-280 | 0.9 | 205 | 0 | Essence Shift |
| 2 | Sniper | dwarf | hunter | 450/900/1800 | 4 | 0 | 65-75/130-150/260-300 | 1.0 | 600 | 100 | Assassinate |
| 2 | Timbersaw | goblin | mech | 800/1600/3200 | 7 | 0 | 55-60/110-120/220-240 | 1.4 | 205 | 100 | Whirling Death |
| 2 | Windranger | elf | hunter | 550/1100/2200 | 3 | 0 | 50-70/100-140/200-280 | 0.9 | 600 | 100 | Power Shackle Shot |
| 3 | Huskar | troll | warrior | 700/1400/2800 | 6 | 20 | 70-100/140-200/280-400 | 1.3 | 400 | 0 | Berserker's Blood |
| 3 | Legion Commander | human | knight | 700/1400/2800 | 5 | 0 | 50-100/100-200/200-400 | 1.2 | 205 | 0 | none |
| 3 | Lina | human | mage | 600/1200/2400 | 3 | 10 | 50-55/100-110/200-220 | 1.3 | 600 | 100 | Laguna Blade |
| 3 | Marci | elf | monk | 600/1200/2400 | 4 | 10 | 70-90/140-180/280-360 | 1.2 | 205 | 100 | Unleash |
| 3 | Meepo | kobold | mech | 650/1300/1950 | 4 | 10 | 50-60/100-120/200-240 | 1 | 205 | 100 | Poof |
| 3 | Muerta | undead | hunter | 650/1300/2600 | 3 | 0 | 40-80/80-160/160-320 | 1.2 | 400 | 100 | Pierce the Veil |
| 3 | Phantom Assassin | elf | assassin | 550/1100/2200 | 4 | 0 | 70-100/140-200/280-400 | 1.0 | 205 | 0 | Coup de Grace |
| 3 | Razor | element | mage | 650/1300/2600 | 4 | 0 | 50-60/100-120/200-240 | 1.1 | 400 | 100 | Plasma Field |
| 3 | Riki | satyr | assassin | 500/1000/2000 | 5 | 0 | 45-95/90-190/180-380 | 0.9 | 205 | 0 | Smoke Screen |
| 3 | Rubick | god | wizard | 600/1200/2400 | 2 | 20 | 50-60/100-120/200-240 | 1.4 | 205 | 100 | Spell Steal |
| 3 | Sand King | aqir | assassin | 600/1200/2400 | 6 | 0 | 45-60/90-120/180-240 | 1.4 | 205 | 100 | Burrowstrike |
| 3 | Shadow Fiend | demon | warlock | 600/1200/2400 | 3 | 20 | 70-80/140-160/280-320 | 1.2 | 600 | 100 | Requiem of Souls |
| 3 | Slardar | naga | warrior | 750/1500/3000 | 5 | 10/0/0 | 80-110/160-220/320-440 | 1.5 | 205 | 100 | Slithereen Crush |
| 3 | Terrorblade | demon | demonhunter | 750/1500/3000 | 5 | 20 | 40-70/80-140/160-280 | 1.1 | 205 | 100 | Metamorphosis |
| 3 | Treant Protector | elf | druid | 700/1400/2450 | 5 | 0 | 80-90/160-180/320-360 | 2 | 205 | 100 | Leech Seed |
| 3 | Undying | undead | priest | 900/1800/3600 | 2 | 20 | 60-90/120-180/240-360 | 1.7 | 205 | 100 | Decay |
| 3 | Venomancer | beast, aqir | warlock | 550/1100/2200 | 4 | 20 | 50-60/100-120/200-240 | 1.2 | 400 | 100 | Plague Ward |
| 4 | Alchemist | goblin, ogre | warlock | 1050/2100/4200 | 6 | 0 | 50-110/100-220/200-440 | 1.3 | 205 | 100 | Chemical Rage |
| 4 | Broodmother | aqir | hunter | 950/1900/3800 | 10 | 20 | 80-110/160-220/320-440 | 1.1 | 205 | 100 | Spin Web |
| 4 | Chen | orc | priest | 900/1800/3600 | 6 | 10 | 40-60/80-120/160-240 | 1.4 | 400 | 100 | Holy Persuasion |
| 4 | Doom | demon | warrior | 1050/2100/4200 | 8 | 0 | 110-120/220-240/440-480 | 1.3 | 205 | 100 | Doom |
| 4 | Dragon Knight | human, dragon | knight | 800/1600/3200 | 10 | 0 | 80-90/160-180/320-360 | 1.4 | 205 | 100 | Elder Dragon Form |
| 4 | Drow Ranger | undead | hunter | 600/1200/2400 | 5 | 10 | 80-100/160-200/320-400 | 1.0 | 600 | 100 | Precision Aura |
| 4 | Earthshaker | tauren | shaman | 700/1400/2800 | 10 | 0 | 80-120/160-240/320-480 | 1.4 | 205 | 100 | Fissure |
| 4 | Keeper of the Light | human | mage | 650/1300/2600 | 3 | 20 | 40-45/80-90/160-180 | 1.7 | 600 | 100 | Illuminate |
| 4 | Kunkka | human | warrior | 950/1900/3800 | 10 | 0 | 80-100/160-200/320-400 | 1.3 | 205 | 100 | Ghostship |
| 4 | Lich | undead | mage | 850/1700/3400 | 6 | 20 | 50-70/100-140/200-280 | 1.5 | 400 | 100 | Chain Frost |
| 4 | Lone Druid | beast | druid | 800/1600/2800 | 7 | 0 | 50-60/100-120/200-240 | 1.3 | 400 | 100 | Summon Spirit Bear |
| 4 | Magnus | centaur | assassin | 900/1800/3600 | 7 | 5 | 65-75/130-150/260-300 | 1.5 | 205 | 100 | Reverse Polarity |
| 4 | Medusa | naga | hunter | 750/1500/3000 | 7 | 10 | 50-80/100-160/200-320 | 1 | 400 | 100 | Stone Gaze |
| 4 | Monkey King | beast | monk | 900/1800/3600 | 8 | 0 | 70-110/140-220/280-440 | 1.1 | 205 | 0 | Jingu Mastery |
| 4 | Snapfire | goblin | knight | 800/1600/3200 | 10 | 0 | 70-110/140-220/280-440 | 1.2 | 600 | 100 | Lil' Shredder |
| 5 | Arc Warden | kobold | shaman | 950/1900/3800 | 7 | 0 | 90-150/180-300/360-600 | 1.1 | 400 | 0 | Tempest Double |
| 5 | Disruptor | orc | shaman | 1000/2000/4000 | 6 | 20 | 50-70/100-140/200-280 | 1.5 | 600 | 100 | Static Storm |
| 5 | Elder Titan | god, tauren | druid | 1000/2000/3000 | 7 | 0 | 100-130/200-260/400-520 | 1.3 | 205 | 100 | Earth Splitter |
| 5 | Enigma | element | warlock | 950/1900/3800 | 6 | 30 | 45-55/90-110/180-220 | 1.5 | 400 | 100 | Midnight Pulse |
| 5 | Gyrocopter | dwarf | mech | 900/1800/3600 | 12 | 0 | 75-100/150-200/300-400 | 0.9 | 400 | 0 | Flak Cannon |
| 5 | Invoker | elf | mage | 1000/2000/4000 | 7 | 30 | 70-80/140-160/280-320 | 1.1 | 600 | 100 | none |
| 5 | Jakiro | dragon | mage | 1000/2000/4000 | 9 | 20 | 30-100/60-200/120-400 | 1.3 | 400 | 100 | none |
| 5 | Leshrac | god, centaur | druid | 900/1800/3150 | 6 | 20 | 60-110/120-220/240-440 | 1.2 | 400 | 100 | Pulse Nova |
| 5 | Necrophos | undead | warlock | 950/1900/3800 | 6 | 30 | 50-100/100-200/200-400 | 1.2 | 400 | 100 | Reaper's Scythe |
| 5 | Primal Beast | beast | warrior | 1200/2400/4800 | 11 | 10 | 150-200/300-400/600-800 | 1.8 | 205 | 100 | Pulverize |
| 5 | Queen of Pain | demon | assassin | 950/1900/3800 | 6 | 20 | 75-100/150-200/300-400 | 1.2 | 400 | 100 | Sonic Wave |
| 5 | Spectre | undead | demonhunter | 1000/2000/4000 | 8 | 30 | 80-160/160-320/320-640 | 1.2 | 205 | 100 | Haunt |
| 5 | Techies | goblin | mech | 1000/2000/4000 | 10 | 0 | 45-55/90-110/180-220 | 1.5 | 600 | 100 | Explosive Bomb |
| 5 | Templar Assassin | elf | assassin | 950/1900/3800 | 8 | 0 | 90-120/180-240/360-480 | 1 | 400 | 100 | Refraction |
| 5 | Tidehunter | naga | hunter | 1000/2000/4000 | 10 | 15 | 65-75/130-150/260-300 | 2 | 205 | 100 | Ravage |
| 5 | Troll Warlord | troll | warrior | 1000/2000/4000 | 10 | 10 | 90-130/180-260/360-520 | 0.9 | 400 | 0 | Berserker's Rage |
| 5 | Warlock | orc | warlock | 850/1700/3400 | 6 | 25 | 60-120/120-240/240-480 | 1.5 | 600 | 100 | Chaotic Offering |
| 5 | Zeus | god | mage | 950/1900/3800 | 7 | 40 | 50-70/100-140/200-280 | 1.4 | 400 | 100 | Thundergod's Wrath |

### Abilities with numbers, snapshot 2019-06-05

- **Anti-mage** ($1) - Mana Break: Burns an opponent's mana on each attack, dealing bonus damage to the target based on mana burnt. [damage_per_burn 0.5; mana_per_hit 30 60 90]
- **Axe** ($1) - Berserker's Call: Axe taunts enemies within 1 grid, forcing them to attack him, while he gains bonus armor during the duration. [cooldown 10; mana cost 100; radius 192; bonus_armor 5 10 15; duration 2 3 4]
- **Batrider** ($1) - Sticky Napalm: Drenches an area of 2-grid radius in sticky oil, amplifying damage from Batrider's attacks and slowing enemy reactions. [cooldown 3; mana cost 100; damage 50 75 100; radius 375 375 375; duration 20; movement_speed_pct -3 -5 -7; turn_rate_pct -70 -70 -70; max_stacks 4 7 10]
- **Bounty Hunter** ($1) - Shuriken Toss: Hurls a deadly shuriken at an enemy unit, dealing damage. [cooldown 5; mana cost 100; bonus_damage 300 500 700; speed 1000 1000 1000; bounce_aoe 1200; ministun 0.1]
- **Clockwerk** ($1) - Battery Assault: Discharges high-powered shrapnel at random nearby enemy chess pieces within 2 grids, dealing minor magical damage and ministun. [cooldown 12 10 8; damage 50 75 100; radius 275 275 275 275; duration 5; interval 0.7]
- **Drow Ranger** ($1) - Marksmanship: Increase attack damage and speed for Drow Ranger. [damage_add 20 30 40]
- **Enchantress** ($1) - Nature's Attendants: Summons a cloud of wisps to randomly heal Enchantress and nearby allies within 3-grid range. [cooldown 15; mana cost 100; heal_interval 1.5; heal 25; radius 500; wisp_count 3 5 7]
- **Mars** ($1) - Bulwark: Mars enters the battle in defensive stance and cannot attack. Mars wields his massive shield to block a portion of each physical attack when hit from the front or sides. Periodically, Mars smashes enemies in 3-grid range in front of him with his shield, dealing physical damage. [cooldown 8; physical_damage_reduction 30 50 70; forward_angle 90; physical_damage_reduction_side 15 25 35; side_angle 180; shield_bash_radius 200; shield_bash_damage 150 300 450]
- **Shadow Shaman** ($1) - Hex: Transforms an enemy piece into a harmless creature, disabling their attacks and abilities. [cooldown 10 9 8; mana cost 100; movespeed 100; duration 4 6 8]
- **Tinker** ($1) - Heat Seeking Missile: Launches 3 rockets to strike random enemy chess pieces. [cooldown 8 6 4; mana cost 100; damage_per_missile 200 300 400]
- **Tiny** ($1) - Toss: Grabs a nearby enemy, and launches it at the farthest square from it to deal damage and stun where they land. [cooldown 10 8 6; mana cost 100; damage_radius 200; damage 100 200 300; stun_duration 1.5 2 2.5]
- **Tusk** ($1) - Walrus PUNCH!: Tusk connects with his mighty Walrus Punch, a critical strike so powerful it launches a nearby enemy within 1 grid into the air. [cooldown 7; mana cost 100; crit_multiplier 300 350 400; air_time 1.0; slow_duration 2.0; move_slow 40; push_length 1000]
- **Winter Wyvern** ($1) - Cold Embrace: Encases an allied piece in an icy cocoon, freezing them solid while healing a base amount as well as a percentage of their maximum health each second. The target is immune to physical damage while in the cocoon. [cooldown 15 12 9; mana cost 100; duration 2 3 4; heal_additive 20; heal_percentage 10]
- **Beastmaster** ($2) - Wild Axes: Beastmaster sends his axes flying and calls them home again, slicing through enemies, amplifying subsequent damage from Beastmaster. [cooldown 7; mana cost 100; radius 175; spread 450; range 1500; axe_damage 80 120 160; duration 10; damage_amp 10 20 30]
- **Chaos Knight** ($2) - Chaos Bolt: Throws a mysterious bolt of energy at the target unit. It stuns for a random duration and deals random damage. [cooldown 5; mana cost 100; chaos_bolt_speed 1000; stun_min 1 1 1; stun_max 2 3 4; damage_min 50 100 150; damage_max 200 300 400]
- **Crystal Maiden** ($2) - Arcane Aura: Grants periodic mana regeneration for all allied chess pieces. [mana_regain 8 14 20; radius 800]
- **Furion** ($2) - Nature's Call: Call a treant from a random tree surrounding the chessboard to battle for Nature's Prophet. [cooldown 7; mana cost 100]
- **Juggernaut** ($2) - Blade Fury: Causes a bladestorm of destructive force around Juggernaut, rendering him immune to magic and dealing damage to nearby enemies within 2 grids. [cooldown 12; mana cost 100; blade_fury_damage_tick 0.3; blade_fury_radius 250; blade_fury_damage 50 100 150; duration 5]
- **Lich** ($2) - Frost Shield: Applies a magical frost shield around an allied piece, granting bonus armor. While the shield is active, ice magic will be affecting enemy pieces within 4 squares every 1 second, doing minor damage. Enemy pieces attacking the protected ally will be impeded by the icy energy, which slows their attacks. [cooldown 10 8 6; mana cost 100; armor_add 10 15 20; attack_speed_slow -30 -45 -60; slow_duration 2; damage 20 30 40; interval 1; radius 600; duration 6]
- **Luna** ($2) - Moon Glaives: Empowers Luna's glaives, causing her attacks to bounce between enemy chess pieces. Deals less damage with each bounce. [mana cost 0; range 500; bounces 3 5 7; damage_reduction_percent 30]
- **Mirana** ($2) - Sacred Arrow: Fires a long-range arrow with deadly precision to an enemy chess in another chessboard currently under attack/defense from friendly chess pieces, dealing damage and stun to the target. Damage and stun duration increase with range. [cooldown 10 8 6; mana cost 100; min_damage 50 75 100; min_stun 1 1.5 2; max_damage 500 750 1000; max_stun 5 7.5 10; max_distance 6000]
- **Morphling** ($2) - Waveform: Morphling dissolves into liquid and surges forward to farthest enemy piece, damaging enemies in his path. [cooldown 10 8 6; mana cost 100; damage 200 300 400; speed 1250; width 200; charge_restore_time 0]
- **Puck** ($2) - Illusory Orb: Puck launches a magic orb that floats in a straight path, damaging enemy chess pieces along the way. [cooldown 5 4 3; mana cost 100; damage 150 200 250; radius 150; max_distance 800; orb_speed 651; orb_vision 450; vision_duration 3]
- **Queen of Pain** ($2) - Scream of Pain: The Queen of Pain lets loose a piercing scream around her, damaging nearby enemies within 3 grids. [cooldown 6; mana cost 100; damage 200 300 400; area_of_effect 425; projectile_speed 900]
- **Slardar** ($2) - Corrosive Haze: Severely reduces an enemy's armor to amplify physical damage it receives. [cooldown 3; mana cost 100; armor_reduction -15 -30 -45; duration 20]
- **Timbersaw** ($2) - Whirling Death: Timbersaw whirls extremely sharp edges, damaging enemies within 2 grids. [cooldown 6 5 4; mana cost 100; whirling_radius 300; whirling_damage 150 200 250; whirling_tick 0.3; stat_loss_pct 13; duration 14.0]
- **Witch Doctor** ($2) - Paralyzing Cask: Launches a cask of paralyzing powder that ricochets between enemy chess pieces, stunning and damaging those it hits. [cooldown 10; mana cost 100; damage 50 75 100; hero_duration 1; creep_duration 1; hero_damage 50 75 100; bounce_range 575 575 575; bounces 5 7 9; speed 1000; bounce_delay 0.3; bounces_tooltip 5 7 9]
- **Abaddon** ($3) - Aphotic Shield: Summons dark energies around a most-damaged ally unit, creating a shield that absorbs a set amount of damage before expiring. When the shield is destroyed it will burst and deal damage equal to the amount absorbed to an area of 3-grid radius around it. [cooldown 12; mana cost 100; duration 10; damage_absorb 100 150 200; radius 400]
- **Dazzle** ($3) - Shallow Grave: Bless allied pieces with Shallow Grave, preventing the targets from dying while under its protection, no matter how close to death. [cooldown 15; mana cost 100; count 1 2 3; duration 4]
- **Lina** ($3) - Laguna Blade: Fires off a bolt of lightning at a single enemy unit, dealing massive damage. [cooldown 10 7.5 5; mana cost 100; damage 500 750 1000; damage_delay 0.25; fiery_soul_duration 30; fiery_soul_attack_speed_bonus 40 60 80]
- **Lycan** ($3) - Shapeshift: Lycan assumes his lupine form, granting him bonus maximum HP. Summons at most 2 wolves in nearby vacant grids to battle for Lycan. [cooldown 60; mana cost 100; hp_per 20 30 40]
- **Omniknight** ($3) - Purification: Instantly heals a friendly chess which is damaged the most, and damages all nearby enemies within 2 grids. [cooldown 10; mana cost 100; heal 250 350 450; radius 260]
- **Phantom Assassin** ($3) - Coup de Grace: Phantom Assassin refines her combat abilities, gaining a chance of delivering a devastating critical strike to enemy units. [crit_chance 10; crit_bonus 300 450 600]
- **Razor** ($3) - Plasma Field: Releases a wave of energetic plasma that grows in power as it expands, but also zaps on contraction, dealing damage to enemy units caught in its path. Damage and slow increases with distance from Razor. The Plasma Field hits each unit twice, once each direction. [cooldown 10; mana cost 100; damage_min 10; damage_max 100 175 250; radius 400 450 500; speed 636; slow_min 0; slow_max 0; slow_duration 0]
- **Shadow Fiend** ($3) - Requiem of Souls: Shadow Fiend gathers his captured souls to release them as lines of demonic energy to damage enemies in the path. Lines of energy are created for every 2 souls captured. [cooldown 20; mana cost 100; damage 100 150 200; cast_time_tooltip 1.67; requiem_radius 700; requiem_reduction_ms 0; requiem_reduction_damage 0; requiem_reduction_tooltip 0; requiem_slow_duration 5.0; requiem_reduction_radius 500 600 700; requiem_soul_conversion 2 2 2; requiem_line_width_start 125; requiem_line_width_end 425; requiem_line_speed 700 700 700; soul_death_release 0]
- **Sniper** ($3) - Assassinate: Sniper locks onto a target enemy chess piece and, after a short aiming duration, fires a devastating shot that deals massive damage at long range. [cooldown 10; mana cost 100; damage 400 600 800; projectile_speed 2500 2500 2500; tooltip_range 2000 2500 3000; total_cast_time_tooltip 0.5; LinkedSpecialBonusOperation SPECIAL_BONUS_SUBTRACT]
- **Terrorblade** ($3) - Metamorphosis: Terrorblade transforms into a powerful demon with a ranged attack, granting him bonus attack speed and damage. Upon transformation, Terrorblade exchanges health percentage with an allied chess piece currently with highest health percentage. [cooldown 60; mana cost 100; damage 100 150 200; range 600; speed 50 75 100]
- **Treant Protector** ($3) - Leech Seed: Treant plants a life-sapping seed in an enemy chess, draining its health, while simultaneously slowing it. The seed heals friendly chess pieces around it. [cooldown 7; mana cost 100; damage_interval 1.5; leech_damage 30 45 60; movement_slow -50; radius 400; duration 5; projectile_speed 400]
- **Venomancer** ($3) - Plague Ward: Summons a plague ward in a nearby vacant square to attack enemy units. The ward is immune to magic. Attacks of the wards reduce the attack rate of the target by 0.3. [cooldown 5 4 3; mana cost 100]
- **Viper** ($3) - Viper Strike: Viper slows the targeted enemy unit's attack speed while also dealing poison damage over time. [cooldown 10 7 4; mana cost 100; duration 5; damage 150 250 350; bonus_movement_speed -40 -60 -80; bonus_attack_speed -40 -60 -80; projectile_speed 1200; max_charges 2; charge_restore_time 30.0]
- **Windranger** ($3) - Powershot: Windranger charges her bow for up to 1 second for a single powerful shot, damaging all enemies on its path. Damage dealt increases with charge time. [cooldown 10; mana cost 100; powershot_damage 400 600 800; damage_reduction 0.1; arrow_width 125 125 125; arrow_range 1200; arrow_speed 3000.0 3000.0 3000.0; tree_width 75 75 75; vision_radius 400; vision_duration 3.34 3.34 3.34]
- **Alchemist** ($4) - Chemecal Rage: Alchemist causes his Ogre to enter a chemically induced rage, reducing base attack cooldown and increasing movement speed and regenerations. Upon transformation, Alchemist sprays acidic chemicals around him, reducing the armor of nearby enemy pieces. [cooldown 25 20 15; mana cost 100; duration 15; transformation_time 0.35; base_attack_time 0.7 0.55 0.4; bonus_health 0; bonus_health_regen 40 60 80; bonus_mana_regen 0 0 0; bonus_movespeed 0 0 0; armor_reduce 4 7 10; acid_duration 15]
- **Disruptor** ($4) - Static Storm: Creates a damaging static storm that also silences all enemy units in the area of 4-grid radius for the duration. The damage starts off weak, but increases in power over the duration. [cooldown 15 12 9; mana cost 100; radius 400 500 600; pulses 8 12 16; damage_max 100 150 200; duration 2.5 3.5 4.5]
- **Doom** ($4) - Doom: Inflicts a curse that prevents an enemy chess piece from casting spells or using items, while taking damage over time. [cooldown 10; mana cost 100; duration 10 15 20; damage 60 90 120; deniable_pct 25 25 25]
- **Dragon Knight** ($4) - Elder Dragon Form: Dragon Knight takes the form of one of three powerful elder dragons, increasing his damage, while granting him new powers. LEVEL 1 Green Dragon - Corrosive Breath: attacks deal damage over time. LEVEL 2 Red Dragon - Splash Attack: attacks damage all enemy units in a 2-grid radius, with Corrosive Breath added to the targets. LEVEL 3 Blue Dragon - Frost Breath: slows enemy chess pieces in Splash Attack range, with Corrosive Breath added to the targets. [cooldown 60; mana cost 100; duration 60; bonus_movement_speed 30; bonus_attack_range 350; bonus_attack_damage 100 150 200; corrosive_breath_damage 30 30 30; corrosive_breath_duration 5.0 5.0 5.0; splash_radius 300; splash_damage_percent 50; frost_bonus_movement_speed -30; frost_bonus_attack_speed -30; frost_duration 3.0 3.0 3.0; frost_aoe 300]
- **Keeper of the Light** ($4) - Illuminate: Channels light energy, building power the longer it's channeled. Once released or interupted, a wave is sent forth in 3-grid width, dealing massive magical damage. [cooldown 10 8 6; mana cost 100; damage_per_second 100 150 200; max_channel_time 3; radius 300; range 1000; speed 1050.0; vision_radius 800 800 800; vision_duration 3.34 3.34 3.34; channel_vision_radius 300; channel_vision_interval 0.5; channel_vision_duration 10.34; channel_vision_step 150; total_damage 300 450 600]
- **Kunkka** ($4) - Ghost Ship: Summons a ghostly ship that sails through the battle before smashing apart, damaging and stunning all enemies within 3 squares of the wreckage. [cooldown 15 12 9; mana cost 100; damage 150 250 350; tooltip_delay 3.1; ghostship_distance 2000; ghostship_width 425 425 425; movespeed_bonus 10 10 10; buff_duration 0; stun_duration 1 1.5 2; ghostship_speed 650; ghostship_absorb 0]
- **Lone Druid** ($4) - Summon Spirit Bear: Summons a powerful Spirit Bear companion in a nearby vacant square to battle for Lone Druid. [cooldown 60; mana cost 100]
- **Medusa** ($4) - Stone Gaze: Any enemy units looking at Medusa will have their movement and attack speed slowed. If 2 seconds of total time is accumulated looking at Medusa while Stone Gaze is active, that unit will turn to stone. Petrified units are stunned, and take bonus physical damage. [cooldown 20; mana cost 100; radius 600 800 1000; duration 3 4 5; slow 30; stone_duration 2 2.5 3; face_duration 2.0; vision_cone 0.5; bonus_physical_damage 20]
- **Necrophos** ($4) - Death Pulse: Necrophos releases a wave of death around him, dealing damage to enemy chess pieces within 3 grids and healing allied chess pieces. [cooldown 5; mana cost 100; damage 100 150 200; area_of_effect 475; heal 100 175 250; projectile_speed 400; health_regen 2 3 4; mana_regen 2 3 4; regen_duration 6; hero_multiplier 6]
- **Templar Assassin** ($4) - Refraction: Templar Assassin becomes highly elusive, avoiding damage and gaining a bonus to her damage. The damage and avoidance effects are separate, and have a limited number of instances. [cooldown 6; mana cost 100; instances 4 6 8; bonus_damage 40 60 80; damage_threshold 5; duration 6]
- **Troll Warlord** ($4) - Fervor: With each continuous blow on the same target, Troll gains increased attack speed. If Troll changes targets, the stacks drop to zero. [attack_speed 15 20 25; max_stacks 6 9 12]
- **Death Prophet** ($5) - Exorcism: Unleashes evil spirits to drain the life of nearby enemy chess pieces. At the end of the spell's duration, Death Prophet is healed in proportion to the damage dealt. [cooldown 20 15 10; mana cost 100; radius 700 700 700; spirits 10 15 20; spirit_speed 500 500 500; max_distance 1000 1000 1000; give_up_distance 800 800 800; min_damage 100; max_damage 100; heal_percent 25 25 25; average_damage 100; ghost_spawn_rate 0.3]
- **Enigma** ($5) - Midnight Pulse: Steeps an area of 3-grid radius in dark resonance, damaging enemy chess pieces based on their max HP. [cooldown 20; mana cost 100; radius 450; damage_percent 6 9 12; duration 8]
- **Gyrocopter** ($5) - Call Down: Call down an aerial missile strike on enemies within an area of 4-grid radius. Two missiles arrive in succession, each dealing massive damage. [cooldown 10; mana cost 100; slow_duration_first 2; slow_duration_second 4; damage_first 200 300 400; damage_second 200 300 400; slow_first 30; slow_second 60; radius 600]
- **Sven** ($5) - God's Strength: Sven's attacks can deal 50% Cleave damage to enemy pieces within 2 sqaure. Active: Sven gets pumped up! The rogue strength grants bonus damage to Sven and all allied Demon pieces for 60 seconds. [cooldown 30; mana cost 100; gods_strength_damage 100 150 200; gods_strength_bonus_str 0]
- **Techies** ($5) - Explosive Bomb: Plant an explosive that automatically detonates after a brief delay, dealing massive physical damage to enemies within a range of 4-grid radius. [cooldown 10; mana cost 100; explode_delay 3; damage 700 1100 1500; range 600; scale 1.2 1.5 1.8]
- **Tidehunter** ($5) - Ravage: Slams the ground, causing tentacles to erupt in all directions, damaging and stunning all nearby enemies within 6-grid maximum range. [cooldown 30; mana cost 100; damage 150 250 350; radius 500 650 800; speed 725; duration 2 2.5 3]
- **Zeus** ($5) - Thundergod's Wrath: Strikes the current chessboard with a bolt of lightning, causing all enemy pieces on board to have 50% chance being damaged. The victims suffer from a base magical damage, and an additional magical damage based on the percentage of their current health. When surviving and winning a battle on other players' board, Zeus deals a percentage damage to the opponents' Courier based on their current health, instead of dealing a fixed damage. [cooldown 15 12 9; mana cost 100; damage 300 400 500; damage_per 15 25 35; damage_courier_per 15 25 35]

### Abilities with numbers, snapshot 2023-04-10

- **Abaddon** ($1) - Aphotic Shield: ACTIVE ABILITY: Activates on Full Mana. Summons dark energies around a most-damaged ally unit, creating a shield that absorbs a set amount of damage before expiring. When the shield is destroyed it will burst and deal damage equal to the amount absorbed to an area of 3-grid radius around it. [cooldown 10 8 6; mana cost 100; duration 10; radius 300; damage_absorb 100 200 300]
- **Anti-mage** ($1) - Mana Break: Mana Break damage does not grant the victim mana regeneration. Burns enemy's mana on every hit and deals physical damage as same amount of mana burnt. [percent_damage_per_burn 100 150 200; mana_per_hit 30 60 100; mana_per_hit_pct 0; illusion_percentage 100; move_slow 10 20 30; slow_duration 0.75]
- **Axe** ($1) - Berserker's Call: ACTIVE ABILITY: Activates on full mana. Axe taunts enemies within 1 grid, forcing them to attack him, while he gains bonus armor during the duration. [cooldown 10 8 6; mana cost 100; radius 192; bonus_armor 10 15 25; duration 2 3 5; applies_battle_hunger {"special_bonus_scepter": "+1"}]
- **Bounty Hunter** ($1) - Jinada Toss: Gets a bag of gold when killing an enemy for the first time. ACTIVE ABILITY: Activates on Full Mana. Hurls a deadly shuriken at the enemy of the lowest health, dealing damage and mini-stunning the target. [cooldown 9 7 5; mana cost 100; damage 300 400 600; ministun 1; gold_count 1 3 5]
- **Clockwerk** ($1) - Battery Assault: ACTIVE ABILITY: Activates on Full Mana. Discharges high-powered shrapnel at random nearby enemy chess pieces within 2 grids, dealing minor magical damage and ministun. [cooldown 12 10 8; mana cost 100; radius 275 275 275; duration 6 8 10; interval 0.7; damage 20 40 80; creep_damage_multiplier 1]
- **Crystal Maiden** ($1) - Arcane Aura: Aura: Grants periodic mana regeneration for all allied chess pieces. [mana_regain 8 12 20]
- **Dazzle** ($1) - Shallow Grave: Bless allied pieces with Shallow Grave, preventing the targets from dying while under its protection, no matter how close to death. The blessed ally takes +50% healing from all sources for the duration. [cooldown 15; mana cost 100; count 1 2 3; duration 4 4 5]
- **Enchantress** ($1) - Untouchable: Aura: Granting allies nearby to slow enemy attacks when attacked. [slow_attack_speed -15 -20 -30; slow_duration 3; radius 190]
- **Hoodwink** ($1) - Hunter's Boomerang: ACTIVE ABILITY: Activates on Full Mana. Tosses a boomerang at the target enemy in an arc. Upon contact, it returns to Hoodwink. The boomerang will deal magical damage as it passes through or hits enemies, and apply a Hunter's Mark that causes affected enemies to be slowed and be more vulnerable to spell damage and receive longer negative status effects. [cooldown 12 10 8; mana cost 100; speed 900; radius 125; damage 75 100 150; mark_duration 10.0 15.0 25.0; slow_pct 0; spell_amp 20 30 50; status_resistance 0; spread 400; min_throw_duration 0.5; max_throw_duration 1.2; debuff_amp 75 100 150]
- **Luna** ($1) - Moon Glaives: Empowers Luna's glaives, causing her attacks to bounce between enemy chess pieces. Deals less damage with each bounce. [mana cost 0; range 500; bounces 3 5 7; damage_reduction_percent 30]
- **Mars** ($1) - Bulwark: Mars wields his massive shield to block a portion of each physical attack when hit from the front or sides. Periodically, Mars smashes and knocks back enemies in 3-grid range in front of him with God's Rebuke, dealing physical damage. [cooldown 8; mana cost 0; physical_damage_reduction 30 45 60; forward_angle 90; physical_damage_reduction_side 15 23 30; side_angle 180; redirect_chance 0; redirect_range 0; redirect_speed_penatly 0; shield_bash_radius 200; shield_bash_damage 200 250 350]
- **Mirana** ($1) - Sacred Arrow: ACTIVE ABILITY: Activates on Full Mana. Fires a long-range arrow with deadly precision to an enemy chess in another chessboard currently under attack/defense from friendly chess pieces, dealing damage and stun to the target. Damage and stun duration increase with range. [cooldown 10 8 6; mana cost 100; min_damage 100 150 250; min_stun 1 1.5 2.5; max_damage 300 450 750; max_stun 3 4.5 7.5; max_distance 1267]
- **Shadow Shaman** ($1) - Croak!: ACTIVE ABILITY: Activates on Full Mana. Transform an enemy into a harmless creature, disabling their attacks, abilities and items. [cooldown 10 8 5; mana cost 100; AbilityCooldown 10 8 5; duration 4 5 6; movespeed 120]
- **Spirit Breaker** ($1) - Greater Bash: Attacks have a chance to stun and knockback the enemy and deal bonus damage. [per 15 25 40; stun 2; damage 100 150 250]
- **Tinker** ($1) - Heat Seeking Missile: ACTIVE ABILITY: Activates on Full Mana. Launches %missile_count% rockets to strike random enemy chess pieces. [cooldown 7 5 4; mana cost 100; damage_per_missile 200 250 300; missile_count 3 3 4]
- **Tiny** ($1) - Toss: ACTIVE ABILITY: Activates on Full Mana. Grabs a nearby enemy, and launches it at the farthest enemy 3 squares away from the victim, or directly up into the air, to deal damage and stun where they land. [cooldown 10 8 6; mana cost 100; damage_radius 200; damage 100 200 300; stun_duration 0.5 1 1.5]
- **Vengeful Spirit** ($1) - Nether Swap: Nether Swap: Activate before the start of the battle round.Swap positions with an enemy chess piece whose star level is no higher than(Nether Swap skill level), reduce their armor, and its mana cannot be restored when it is damaged. [max_star 1 2 3; armor_duration 6 9 12; armor_bonus -6 -9 -12]
- **Winter Wyvern** ($1) - Cold Embrace: ACTIVE ABILITY: Activates on Full Mana. Encases an ally in an icy cocoon, freezing them solid while healing a base amount as well as a percentage of their maximum health each second. The cocoon blocks all physical damage and reduces the attack speed of the enemies attacking it. [cooldown 10 8 6; mana cost 100; duration 2.0 3.0 4.0; heal_additive 20; heal_percentage 10; shard_cooldown_reduction 4.0; RequiresShard 1; shard_splinter_range 1200; attack_speed_reduce 10 20 30]
- **Witch Doctor** ($1) - Paralyzing Cask: ACTIVE ABILITY: Activates on Full Mana. Launches a cask of paralyzing powder that ricochets between enemy chess pieces, stunning and damaging those it hits. [cooldown 10 9 8; mana cost 100; damage 30 50 70; hero_duration 1.0; creep_duration 1; base_damage 30 40 50; bounce_range 575; bounces 4 5 7; speed 1000; bounce_delay 0.3; bounce_bonus_damage 10 15 20; creep_damage_pct 100]
- **Batrider** ($2) - Firefly: ACTIVE ABILITY: Activates on Full Mana. Batrider flis towards the farthest cell which allows him to attack an enemy, laying down a trail of flames from the air. The fire damages any enemies it touches, causing damage over time. [cooldown 20 15 10; mana cost 100; damage_per_second 30 45 75; movement_speed 0; radius 200; duration 10; tick_interval 0.5; tree_radius 200 200 200; bonus_vision 100 200 300]
- **Beastmaster** ($2) - Call of the Wild: ACTIVE ABILITY: Activates on Full Mana. Beastmaster summons his powerful companions to aid in the battlefield. Level 1: Summons a Boar. Level 2: Summons a Hawk. Level 3: Summons a Battle Bear with Taunt ability. [cooldown 15; mana cost 100; summon_duration 30]
- **Brewmaster** ($2): no active ability in data.
- **Centaur Warrunner** ($2) - Double Edge: Active Ability: Activates instantCentaur Warrunner strikes a mighty blow at melee range, dealing damage due to a percentage of his Max HP to himself and a small area around the target. [cooldown 10 9 8; mana cost 0; damage_base 100 150 200; damage_per 10; radius 200 250 300]
- **Chaos Knight** ($2) - Chaos Strike: Chaos Knight’s attacks have a chance to deal a critical strike of varying strength, in addition to a lifesteal for percentage of the damage. [chance 25; crit_min 120; crit_max 200 280 360; lifesteal 25; creep_multiplier 1.0]
- **Faceless Void** ($2) - Chronosphere: ACTIVE ABILITY: Activates on Full Mana. Creates a blister in spacetime, trapping all chess pieces caught in its sphere of influence and allows only Faceless Void to move freely in the sphere. [cooldown 25 20 15; mana cost 100; duration 3 4 5; radius 275 300 325]
- **Furion** ($2) - Nature's Call: ACTIVE ABILITY: Activates on Full Mana. Call treants from random trees surrounding the chessboard to battle for Nature's Prophet. Level 1: Summons 1 Treant. Level 2: Summons 2 Treants. Level 3: Summons 2 Greater Treants. [cooldown 7 10 10; mana cost 100; summon_count 1 2 2; summon_duration 30 20 20]
- **Juggernaut** ($2) - Blade Fury: ACTIVE ABILITY: Activates on full mana. Causes a bladestorm of destructive force around Juggernaut, rendering him immune to spells and dealing damage to nearby enemies within 2 grids. [cooldown 12; mana cost 100; blade_fury_damage_tick 0.5; blade_fury_radius 250; blade_fury_damage 100 150 250; duration 5.0; shard_bonus_move_speed 100; shard_attack_rate 1.0; shard_damage_pct 0]
- **Lion** ($2) - Mana Drain: Channeling: Absorbs the magical energies of an enemy piece by taking mana from it every second. After draining, consumes all mana to grant allies with Chakra Magic. Chakra Magic: Restores mana of an ally by the amount equal to Lion's current mana, and reduces cooldown of its ability currently on cooldown. [cooldown 5 4 3; mana cost 0; drain_count 1 2 3; duration 3; mana_per_second 20 30 40]
- **Morphling** ($2) - Waveform: ACTIVE ABILITY: Activates on Full Mana. Morphling dissolves into liquid and surges forward to the farthest enemy piece, damaging enemies in his path. [cooldown 12 10 8; mana cost 100; damage 150 250 350; speed 1250; width 200; AbilityCharges 0; AbilityCastRange 9999; AbilityCooldown 12 10 8]
- **Naga Siren** ($2) - Song of the Siren: Active: Activate on full mana. All enemies in range of the Naga Siren have a chance to be put into a sleep every 0.5 sec, where they cannot act or be attacked. Any non-ability damage will cause the victim to wake up. [cooldown 25 20 15; mana cost 100; radius 250 350 450; duration 7; sleep_duration 5 7 9]
- **Ogre Magi** ($2) - Multicast: ACTIVE ABILITY: Activates on Full Mana. Grants allied pieces with exceptional knowlegde of skill crafts, enabling them to cast their next spell twice in a row. [cooldown 10; mana cost 100; multi_count 1 2 3]
- **Oracle** ($2) - Fate's Edict: ACTIVE ABILITY: Activates on Full Mana. The Oracle enruptures enemies pieces, disarming the targets, amplifying ability damage on them, but granting 100% resistance to physical damage. [cooldown 12 10 8; mana cost 100; jingu_duration 4 4 5; jingu_count 1 2 3; damage_bonus 15 20 30]
- **Puck** ($2) - Illusory Orb: ACTIVE ABILITY: Activates on Full Mana. Puck launches a magic orb that floats in a straight path, damaging enemy units along the way and blinding the victims to miss some of their attacks. [cooldown 5 4 3; mana cost 100; damage 150 200 250; radius 150; max_distance 800; orb_speed 651; orb_vision 450; vision_duration 3.34 3.34 3.34; blind_duration 2 3 4; blind_miss_per 50]
- **Pudge** ($2) - Meat Hook: Active Ability: Activates on full mana. Launches a bloody hook toward the farthest enemy, dragging it back to Pudge and dealing pure damage, and alias will prioritize attacking this target. [cooldown 10 7 4; mana cost 100; hook_speed 1600.0; hook_width 100; hook_distance 9999; vision_radius 500; vision_duration 4.0; hook_followthrough_constant 0.65; hook_damage 300 450 600]
- **Slark** ($2) - Essence Shift: Slark steals the attack speed of enemy pieces with his attacks. [speed_per_attack_buff 5; speed_per_attack_debuff -5; max_stack_count 20 25 30]
- **Sniper** ($2) - Assassinate: Active Ability: Activate on full mana. Sniper locks onto a target enemy chess piece and, after a short aiming duration, fires a devastating shot that deals massive physical damage at long range. [cooldown 10 8 6; mana cost 100; damage 400 600 1000; projectile_speed 2500 2500 2500; tooltip_range 2000 2500 3000; total_cast_time_tooltip 1; LinkedSpecialBonusOperation SPECIAL_BONUS_SUBTRACT]
- **Timbersaw** ($2) - Whirling Death: ACTIVE ABILITY: Activates on full mana. Timbersaw whirls extremely sharp edges, damaging enemies within 2 grids. [cooldown 6 4 3; mana cost 100; whirling_radius 300; whirling_damage 200 250 300; tree_damage_scale 0; whirling_tick 0.3; stat_loss_pct 0; duration 0]
- **Windranger** ($2) - Power Shackle Shot: ACTIVE ABILITY: Activates on full mana. Windranger charges her bow, shackling the target to an enemy chess piece or tree in a line directly behind it. Then she releases a powerful arrow, which deals more damage the longer it is charged. The arrow damages enemies along its path. [cooldown 12 10 8; mana cost 100; powershot_damage 350 550 900; damage_reduction 10; arrow_width 125 125 125; arrow_range 1200; arrow_speed 3000.0 3000.0 3000.0; tree_width 75 75 75; vision_radius 400; vision_duration 3.34 3.34 3.34; shackle_duration 2.5 3.5 5; shackle_fail_duration 1; shackle_distance 600; xuli_duration 2]
- **Huskar** ($3) - Berserker's Blood: Huskar sets his spears aflame, dealing stackable pure damage over time with his regular attack. Each attack drains some of Huskar's health. Meanwhile, Huskar's injuries feed his power, giving increased attack speed, magic resistance and health regen based on missing health. [cooldown 0.0 0.0 0.0 0.0; mana cost 0 0 0 0; damage 15 20 25; health_cost 15 20 25; burning_spear_duration 5; burning_spear_damage 15 20 25; attack_speed_per_level 7 10 15; magic_resist_per_level 8; hp_regain_per_level 0.25]
- **Legion Commander** ($3): no active ability in data.
- **Lina** ($3) - Laguna Blade: ACTIVE ABILITY: Activates on full mana. Fires off a bolt of lightning at a single enemy unit, dealing massive magical damage. Casting Laguna Blade grants Lina a stackable Fiery Soul bonus, increasing her attack speed. [cooldown 15 10 7.5; mana cost 100; damage 500 750 1200; damage_delay 0.25; AbilityCooldown 15 10 7.5; pierce_immunity 0]
- **Marci** ($3) - Unleash: Marci taps a hidden power, gaining Fury charges that allow her to deliver a rapid sequence of strikes.The last strike in every Fury combo creates a damaging pulse for magical damage around the target that slows movement and attack speed of the victims. [cooldown 20 16 12; mana cost 100; duration 10; charges_per_flurry 3 4 5; flurry_bonus_attack_speed 400 500 600; time_between_flurries 1.75; pulse_radius 128 256 384; pulse_debuff_duration 2.0; pulse_damage 50 75 125; pulse_move_slow_pct 30.0; pulse_attack_slow_pct 20.0 30.0 40.0; max_time_window_per_hit 1.0; bonus_movespeed 15; recovery_fixed_attack_rate 2.0; shard_push_length 300]
- **Meepo** ($3) - Poof: When Meepo reachs Rank 3, it duplicates itslef. ACTIVE ABILITY: Activates on full mana. Drawing mystical energies from the earth, a Meepo teleports to the side of an enemy, dealing pure damage in arrival locations. [cooldown 8 7 6; mana cost 100; pure_damage 150 200 250; damage_radius 205]
- **Muerta** ($3) - Pierce the Veil: Muerta's attacks have a chance to fire a second shot at another target. Active Ability: Activates on full mana. Muerta transforms, becoming immune tophysical damage.， Muerta gains bonus attack damage. All of her attack damage is dealt as magical damage. [cooldown 25 20 15; mana cost 100; duration 5 6 7; transform_duration 0.35; base_damage_pct 100; modelscale 0; bonus_damage 75 100 125; double_shot_chance 30 40 50]
- **Phantom Assassin** ($3) - Coup de Grace: Phantom Assassin refines her combat abilities, gaining a chance of delivering a devastating critical strike to enemy units. [crit_chance 10; crit_bonus 300 450 600]
- **Razor** ($3) - Plasma Field: ACTIVE ABILITY: Activates on full mana. Releases a wave of energetic plasma that grows in power as it expands, but also zaps on contraction, dealing damage to enemy units caught in its path. Damage and slow increase with distance from Razor. The Plasma Field hits each unit twice, once in each direction. [cooldown 15 12 9; mana cost 100; damage_min 10; damage_max 100 175 300; radius 450; speed 636; slow_min 5; slow_max 10 15 25; slow_duration 1.5; second_ring_delay 0]
- **Riki** ($3) - Smoke Screen: Battle Cry Ability: Activates before the battle startsThrows down a smoke bomb at Riki's location if an enemy is nearby, otherwise throws at the centrosymmetric point on the chessboard from Riki. Enemies within the smoke are silenced and have a chance to miss attacks. [radius 250 300 400; duration 3 4 5; miss_rate 20 30 50]
- **Rubick** ($3) - Spell Steal: BATTLECRY ABILITY: Activates on battleRubick studies the trace magical essence of the enemy piece, learning the secrets of the spell the piece has. At the start of a combat, Rubick randomly steals the active ability and attack range of an enemy piece to replace Spell Steal. Rubick on the chessboard resets his ability to Spell Steal when losing a home battle. [ability_level 1 2 3]
- **Sand King** ($3) - Burrowstrike: ACTIVE ABILITY: Activates on Full Mana. Sand King burrows into the ground and tunnels forward, damaging and stunning enemies above him as he resurfaces. [cooldown 12 10 8; mana cost 100; damage 150 250 450; burrow_width 105; burrow_duration 1.5 1.5 2; burrow_speed 2000; burrow_anim_time 0.52]
- **Shadow Fiend** ($3) - Requiem of Souls: ACTIVE ABILITY: Activates on Full Mana. Shadow Fiend gathers his captured souls to release them as lines of demonic energy that damages enemies strike. For every friendly Demon with active Fel Power, Shadow Fiend releases an additional line of energy. Shadow Fiend no longer captures souls from slain enemies. [cooldown 10; mana cost 100; damage 150 200 250; cast_time_tooltip 1.67; requiem_radius 700 800 900; requiem_reduction_ms 10 15 25; requiem_reduction_damage 0; requiem_reduction_tooltip 0; requiem_slow_duration 5.0; requiem_reduction_radius 700; requiem_soul_conversion 1; requiem_line_width_start 200; requiem_line_width_end 300; requiem_line_speed 700; soul_death_release 0; base_soul_count 4 6 8]
- **Slardar** ($3) - Slithereen Crush: ACTIVE ABILITY: Activates on Full Mana. Slams the ground, deals physical damage and stun to nearby enemies. Victimcs are slowed afterwards. [cooldown 12 10 8; mana cost 100; crush_radius 300 350 400; crush_extra_slow -10 -20 -40; crush_attack_slow_tooltip -10 -20 -40; crush_extra_slow_duration 3 4 5; stun_duration 1.5 2 2.5; puddle_duration 0; puddle_radius 0; crush_damage 100 150 250; shard_amp_duration 5.0; shard_bonus_radius 75]
- **Terrorblade** ($3) - Metamorphosis: ACTIVE ABILITY: Activates on Full Mana. Terrorblade transforms into a powerful demonic form with a ranged attack, and removing most of status effects from himslef. Rank 2 or higher Terrorblade also transforms his nearest ally into a demon with a rank equal to the transformed chess piece. [cooldown 60; mana cost 100; mohua_friend_count 0 1 2; mohua_tb_hp 750 1500 3000; mohua_tb_ad 110 180 290]
- **Treant Protector** ($3) - Leech Seed: ACTIVE ABILITY: Activates on Full Mana. Plant a leech Seed in enemy's body, decreases its attack and movement speed and takes magical damage to heal enemies around per second. [cooldown 8; mana cost 100; damage_heal 60 90 120; damage_interval 1; movement_speed -10 -20 -30; radius 400; duration 4]
- **Undying** ($3) - Decay: ACTIVE ABILITY: Activates on Full Mana. Undying steals Max HP from enemies through an ally, and deals pure damage to heal the ally chosen. [cooldown 10 8 6; mana cost 100; radius 250 300 400; hp_per 6 9 15; damage 100 150 200]
- **Venomancer** ($3) - Plague Ward: ACTIVE ABILITY: Activates on Full Mana. Summons a plague ward in a nearby vacant square to attack enemy units. The ward is immune to spell damage. Attacks of the wards reduce the attack rate of the target by 0.3. [cooldown 4 4 6; mana cost 100; count 1 1 2; summon_duration 30 30 20; magic_ass 80]
- **Alchemist** ($4) - Chemical Rage: ACTIVE ABILITY: Activates on Full Mana. Alchemist causes his Ogre to enter a chemically induced rage, reducing base attack cooldown and increasing movement speed and regenerations. Upon transformation, Alchemist sprays acidic chemicals around him, reducing the armor of nearby enemy pieces. [cooldown 25 20 15; mana cost 100; duration 15; transformation_time 0.35; base_attack_time 0.6 0.55 0.5; bonus_health 0; bonus_health_regen 60 80 100; bonus_mana_regen 0; bonus_movespeed 0 0 0; armor_reduce 4 8 12; acid_duration 15]
- **Broodmother** ($4) - Spin Web: Spins a large web that roots any enemies captured and causes them to miss some of their attacks and lose evasion. If any enemy piece dies within the web, a Spiderling will be hatched from the corpse. Broodmother and all spiderlings have lifesteal attack on enemies within the web. [cooldown 8 7 5; mana cost 100; radius 205; duration 5 7 8; miss_per 40 60 100; lifesteal_per 40]
- **Chen** ($4) - Holy Persuasion: Active Ability: Activates on full mana. Summon the highest-level chess piece that died this round and increase its star level to the same level as the Holy Persuasion. [cooldown 12; mana cost 100; chess_star 1 2 3]
- **Doom** ($4) - Doom: ACTIVE ABILITY: Activates on full mana. Inflicts a curse that prevents an enemy chess piece from casting spells or using items, while taking damage over time. [cooldown 10; mana cost 100; duration 7 10 20; damage 60 90 150; deniable_pct 25 25 25; AbilityCooldown 10; does_break 0]
- **Dragon Knight** ($4) - Elder Dragon Form: Active Ability: Activate on full mana. Davion transforms into one of three powerful elder dragons, granting him new Dragon Breath powers. LEVEL 1 Red Dragon: Attacks deal splash damage to all enemy units in a 2-square range. LEVEL 2 Blue Dragon: Attacks slow movement speed and attack speed of enemy units in a 2-square range. LEVEL 3 Black Dragon: Attacks additionally deal magic damage over time to enemy units in a 2-square range, equal to a percentage of their health. [cooldown 60; mana cost 100; splash_percent 25; slow_percent -30; poison_percent 5; dragon_form_ad 235 370 590]
- **Drow Ranger** ($4) - Precision Aura: AuraDrow's experiences in battle grant her and her allies increased power, increasing the damage of ranged allies who do not face an melee aggressor. [attack_bonus 30 40 60]
- **Earthshaker** ($4) - Fissure: Slams the ground with a mighty totem, creating an impassable ridge of stone. Enemy chess pieces along the line are stunned and knocked to the nearest vacant squares, and suffer physical damage. [cooldown 10 8 6; mana cost 100; duration 6 8 12; stun 3 4 5; damage 250 400 700]
- **Keeper of the Light** ($4) - Illuminate: ACTIVE ABILITY: Activates on full mana. Channels light energy, building power the longer it's channeled. Once released or interrupted, a wave is sent forth in 3-grid width, dealing massive magical damage. [cooldown 10 8 6; mana cost 100; total_damage 300 450 750; max_channel_time 2; radius 300 300 350; range 1000; speed 900.0; vision_radius 800; vision_duration 3.34; AbilityCooldown 10 8 6; channel_vision_radius 300; channel_vision_interval 0.5; channel_vision_duration 10.34; channel_vision_step 150]
- **Kunkka** ($4) - Ghostship: ACTIVE ABILITY: Activates on Full Mana. Summons a ghostly ship that sails through the battle before smashing apart, damaging and stunning all enemies caught near the wreckage. [cooldown 15 12 9; mana cost 100; damage 250 350 600; ghostship_speed 650; stun_duration 1 1.5 2.5; ghostship_width 400 450 500; ghostship_distance 2000]
- **Lich** ($4) - Chain Frost: ACTIVE ABILITY: Activates on Full Mana. Releases an orb of frost that bounces between nearby enemy units, slowing and dealing magical damage each time it hits. [cooldown 15 12 9; mana cost 100; damage 120; jumps 4 6 10; jump_range 500; slow_movement_speed -30; slow_attack_speed -30; slow_duration 3; projectile_speed 800; vision_radius 800; bonus_jump_damage 20 30 40; initial_projectile_speed 1050; has_unlimited_jumps {"special_bonus_unique_lich_5": "+1"}]
- **Lone Druid** ($4) - Summon Spirit Bear: ACTIVE ABILITY: Activates on Full Mana. Summons a powerful Spirit Bear companion in a nearby vacant square to battle for Lone Druid. [cooldown 60; mana cost 100; summon_duration 40]
- **Magnus** ($4) - Reverse Polarity: Active Ability: Activates on full mana. Magnus changes the property of matter, sucking all enemies to him and stunning them with a powerful slam and dealing damage. [cooldown 15 12 9; mana cost 100; radius 350 400 500; stun_duration 1.5 1.5 2; stun_duration_step 0.5; damage 150 200 300]
- **Medusa** ($4) - Stone Gaze: ACTIVE ABILITY: Activates on Full Mana. Medusa magically splits her shot into several arrows to attack multiple enemies. Active:Any enemy units looking at Medusa will have their movement and attack speed slowed. If 2 seconds of the total time is accumulated looking at Medusa while Stone Gaze is active, that unit will turn to stone. Petrified units are stunned, and take bonus physical damage. [cooldown 25 20 15; mana cost 100; AbilityCastRange 600 800 1000; AbilityCooldown 25 20 15; AbilityManaCost 100; radius 600 800 1000; duration 3 4 5; slow 30; stone_duration 2 2.5 3; face_duration 2.0; vision_cone 0.5; bonus_physical_damage 10 10 10; speed_boost 50]
- **Monkey King** ($4) - Jingu Mastery: Monkey King's attacks awaken the Jingu Bang's power. Upon the fourth hit of his attack, Monkey King unleashes Boundless Strike, stunning and damaging enemies in a line. [attack_count 5 4 3; stun_duration 1 1 2; damage_multiple 150 200 300; damage_range 1200]
- **Snapfire** ($4) - Lil' Shredder: Active ability: Active Ability: Activate on full mana. Snapfire heats up her mounted turret to rapidly fire attacks that deal physical damage equal to her attack damage per hit. Each attack slows the enemy's attack speed and reduces enemy's armor, effect stackable. [cooldown 10 8 6; mana cost 100; damage 100 150 200; buffed_attacks 10 15 25; attack_speed_bonus 300; attack_range_bonus 500; buff_duration_tooltip 8; base_attack_time 0.75; armor_reduction_per_attack 1 1 2; armor_duration 5.0]
- **Arc Warden** ($5) - Tempest Double: Battle Cry Ability: Activates before the battle startsBriefly refocusing its fractured elements into a single form, the Arc Warden creates a perfect electrical duplication of itself. [mana cost 0; duration 10 20 30]
- **Disruptor** ($5) - Static Storm: ACTIVE ABILITY: Activates on Full Mana. Creates a damaging static storm that also silences all enemy chess pieces in the area for the duration. The damage starts off weak, but increases in power over the duration. [cooldown 25 20 10; mana cost 100; radius 300 400 500; pulses 8 10 16; damage_max 100 150 200; duration 4 5 8]
- **Elder Titan** ($5) - Earth Splitter: ACTIVE ABILITY: Activates on Full Mana. Sends forth a jagged crack in front of Elder Titan. After 3 seconds, the crack implodes, stunning while dealing damage to each enemy based on their maximum life. Half of the damage dealt is Magical damage, while the other half is Physical damage. [cooldown 30; mana cost 100; duration 1.5 1.5 1.5; damage_base 20 25 30; damage_per 10 15 20; radius 250]
- **Enigma** ($5) - Midnight Pulse: ACTIVE ABILITY: Activates on Full Mana. Steeps an area of 3-square radius in dark resonance, damaging enemy chess pieces within based on their maximum health. [cooldown 20 15 10; mana cost 100; radius 450; damage_per 5 7 10; duration 6; damage_tick 0.5]
- **Gyrocopter** ($5) - Flak Cannon: Gyrocopter’s attacks hit all enemy units in an area around it for a limited number of attacks, and the side gunner will keep attacking the furthest enemy unit away within its range. [cooldown 15 12 9; mana cost 0; max_attacks 4 6 10]
- **Invoker** ($5): no active ability in data.
- **Jakiro** ($5): no active ability in data.
- **Leshrac** ($5) - Pulse Nova: Active ability: Activates at full mana. Creates waves of damaging energy around Leshrac, to damage nearby enemy units.Magic damageDrains Leshrac’s mana with each pulse. [cooldown 8 6 4; mana cost 0; mana_cost_per_second 20; radius 300 350 400; damage 100 125 150]
- **Necrophos** ($5) - Reaper's Scythe: Active Ability: Activates on full mana. Stuns the target enemy hero, and then generates a Death Pulse centered on the target, dealing magical damage to enemy units and healing allied units.The higher the target's missing health, the higher the pure damage. [cooldown 10 7 4; mana cost 100; stun_duration 2.0; base_damage 200 300 400; hp_xishu 0.75 1.25 2.0; pulse_damage 200 300 400; area_of_effect 300 400 600]
- **Primal Beast** ($5) - Pulverize: Active Ability: Activates on full mana. Primal Beast channels his rage, grabbing a nearby victim with highest HP and slamming them into the ground repeatedly, damaging and ministunning any enemy caught in the AoE. [cooldown 12 9 6; mana cost 100; damage 100 150 250; splash_radius 400 450 550; interval 0.75; ministun 0.3 0.4 0.5; cast_range 205; animation_rate 1.5; channel_time 4 5 6]
- **Queen of Pain** ($5) - Sonic Wave: ACTIVE ABILITY: Activates on Full Mana. Creates a gigantic wave of sound in front of Queen of Pain, dealing heavy pure damage to all enemy units in its wake and disorienting the victims [cooldown 20 15 10; mana cost 100; starting_aoe 200; distance 900; final_aoe 350; speed 1100; damage 250 350 550; chaos_duration 2 3 4]
- **Spectre** ($5) - Haunt: Active Ability: Activate on full mana. Creates a spectral nemesis behind every enemy chess piece to attack them. Spectre and her haunts always attack with Desolate effect to targets that do not have allies around them. [cooldown 20 15 10; mana cost 100; duration 6 7 8]
- **Techies** ($5) - Explosive Bomb: ACTIVE ABILITY: Activates on Full Mana. Plant an explosive that automatically detonates after a brief delay, dealing massive physical damage to enemies within a range of 4-grid radius. [cooldown 12 10 8; mana cost 100; explode_delay 4; damage 800 1200 2000; range 600; scale 1.2 1.5 1.8]
- **Templar Assassin** ($5) - Refraction: ACTIVE ABILITY: Activates on Full Mana. Templar Assassin passively attacks with Phi Blades, slicing through her target, damaging enemies directly behind it for pure damage. Active: Templar Assassin becomes highly elusive, avoiding damage and gaining a bonus to her damage. The damage and avoidance effects are separate, and have a limited number of instances. [cooldown 6; mana cost 100; instances 4 6 9; bonus_damage 40 60 90; damage_threshold 5; duration 6]
- **Tidehunter** ($5) - Ravage: ACTIVE ABILITY: Activates on full mana. Slams the ground, causing tentacles to erupt in all directions, damaging and stunning all nearby enemies within a 6-grid maximum range. [cooldown 30 25 15; mana cost 100; damage 150 250 400; radius 450 550 750; speed 725; duration 3 3.5 4.5]
- **Troll Warlord** ($5) - Berserker's Rage: Each successful attack has a chance to activate Whirling Axes (Melee) or Whirling Axes (Ranged) depending on the distance from the target, dealing physical damage plus an additional attack. Causes slow or blind. Whirling Axes guarantees to proc when Troll Warlord switches his target. [axe_per 25; cd_axe_melee 3 2 1; cd_axe_ranged 3 2 1; damage_axe_melee 150 250 350; damage_axe_ranged 150 250 350]
- **Warlock** ($5) - Chaotic Offering: ACTIVE ABILITY: Activates on Full Mana. Summons a Golem from the depths, dealing random stuns and random pure damage to enemies hit. The Golem has Permanent Immolation and Flaming Fists on attack. During its random duration, any death from chess pieces of both sides has a chance to call upon another Golem to land. [cooldown 60; mana cost 100; stun_radius 250; stun_duration 1; damage 125; inferno_count 1 2 3; inferno_duration 7.5; another_inferno_per 5]
- **Zeus** ($5) - Thundergod's Wrath: Active Ability: Activates on full mana. Strikes all enemies on the chessboard standing on the squares of the same color as Zeus stands on, with a bolt of lightning that deals magical damage plus additional magical damage based on the remaining health of the victim. Zeus additionally deals Thunder damage based on the current Courier health to enemy Courier after winning a visiting battle while standing to the last. [cooldown 15 12 9; mana cost 100; damage 300 400 600; damage_per 15 25 45; damage_courier_per 15 25 45]

## Gaps

- No code dump after 2023-04-10. Piece stats, item numbers and synergy values for mid-2023 to 2026 exist only as patch-note deltas (S1/S2).
- Reddit (old.reddit.com and api.pullpush.io) was blocked or rate-limited, so community patch threads were not read. Wayback had no snapshots of the changelog pages. Chinese sources searched (18183, gamersky) covered the mobile Auto Chess, not DAC. NGA, Baidu Baike, bilibili wiki, dotaautochess.com and autochess.ru were not reached.
- Exact date the base-income cap dropped from 5 to 4: between S3 commits 2021-08-11 (810002b678) and 2022-01-19 (708352019a). No matching patch note found.
- Date of the pool change from 45/30/25/15/10 to 20/20/15/15/10: between S3 snapshots 2019-06-26 and 2019-12-25. Not located in S1; bisecting commits in Jul to Dec 2019 would settle it. The 2020-04-08 note "Chess Piece Pool Optimization" is a later, separate change.
- When the random legendary subset per game started: unknown (it existed by 2021-07-26; it was 8 pieces until 2023-02-26, then 10).
- Synergy effect values in English tooltips before about 2021 are often stale. Exact per-era values need reading `npc_abilities_custom.txt` and Lua per snapshot (breakpoints were extracted; effect magnitudes only spot-checked for Warlock).
- Mana rules (wiki Mana page) were not re-verified against code.
- Sources for courier levels 11-16 and the 2023 `p2` mode: unknown.
- Chinese names for items whose English string is missing in the 2023 file: `item_shengzheyiwu` is Sacred Relic and `item_dafu` is Reaver (2019 strings).

- Ability values for Lua-scripted abilities (for example Furion, Lone Druid, Venomancer ward, Invoker, Legion Commander, Brewmaster and Jakiro in some snapshots) are not in the data files; tooltip text is given where it exists.