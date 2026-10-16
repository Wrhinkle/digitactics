# Auto Chess (`autochess`)

Mode `autochess`. Dota Auto Chess as it played in mid-2019 (the 2019-06-26 build: 60 pieces, the Wizard class just added), with a few ideas from the standalone Auto Chess folded in. Files: `js/sets/autochess.js` (data), `js/sets/autochess-mech.js` (hooks), `js/sets/autochess-art.js` (item art, move looks, basic attacks). Corpus: `docs/inspo/autochess/`.

## Counts

- 62 shop lines (14 / 15 / 14 / 12 / 7 by cost), 3 summons, 17 creeps. Every line is a different Digimon, `unassigned: true` (placeholder models), with `src` naming the Dota Auto Chess piece it adapts.
- 25 traits: 13 races, 12 classes. Race and class are counted separately.
- Items: 13 drop parts in 3 tiers, 33 two-part recipes, the Digi-Egg wildcard, 11 relics.
- 3 new ability types: `summon`, `transform`, `storm`.
- 203 move looks, a basic attack look for every line, art for every item and relic.

## Roster

Based on the 2019-06-26 roster (Rubick and Grimstroke in, Mirana out), plus Lion (2019-07-30) and Oracle (2019-08-30) so Wizard and Priest have more than one piece. Each piece became a Digimon line whose type fits the race and whose role fits the class.

| Cost | Lines (Dota Auto Chess piece) |
|---|---|
| 1 | Lopmon (Anti-Mage), Agumon (Axe), FanBeemon (Batrider), Keramon (Bounty Hunter), Hagurumon (Clockwerk), Dracmon (Drow Ranger), Kudamon (Enchantress), Coronamon (Mars), Tentomon (Shadow Shaman), Kokuwamon (Tinker), Gotsumon (Tiny), Bearmon (Tusk), Dracomon (Winter Wyvern), Candlemon (Lion) |
| 2 | Hackmon (Beastmaster), Goblimon (Chaos Knight), Salamon (Crystal Maiden), Palmon (Nature's Prophet), BlackAgumon (Juggernaut), Tsukaimon (Lich), Lalamon (Luna), Hyokomon (Morphling), Gumdramon (Puck), Floramon (Queen of Pain), Gomamon (Slardar), Solarmon (Timbersaw), Kunemon (Witch Doctor), Labramon (Rubick), Lunamon (Oracle) |
| 3 | Gizamon (Abaddon), Wormmon (Dazzle), Biyomon (Lina), Gabumon (Lycan), Patamon (Omniknight), Falcomon (Phantom Assassin), SnowGoblimon (Razor), Gazimon (Shadow Fiend), Commandramon (Sniper), Monodramon (Viper), Hawkmon (Windranger), Mushroomon (Treant Protector), Guilmon (Terrorblade), Dorumon (Venomancer) |
| 4 | Tapirmon (Alchemist), Armadillomon (Disruptor), Impmon (Doom), Veemon (Dragon Knight), Shamanmon (Keeper of the Light), Kotemon (Kunkka), Gaomon (Lone Druid), Betamon (Medusa), DemiDevimon (Necrophos), Renamon (Templar Assassin), DoKunemon (Troll Warlord), Psychemon (Grimstroke) |
| 5 | DexDorugamon (Death Prophet), Vorvomon (Enigma), Jazamon (Gyrocopter), Terriermon (Techies), Lucemon (Sven), Syakomon (Tidehunter), Elecmon (Zeus) |

Move names are canon (Wikimon via `docs/sets/digimon-db.json`). Where a Rookie had no recorded move (Vorvomon, Jazamon) it reuses its Champion's move. Summons: Tanemon (Palmon), Vegiemon (Dorumon), Kumamon (Gaomon).

## Races and classes

Dota Auto Chess race names were adapted to Digimon families: Elf to Fairy, Orc to Dinosaur, Troll to Insect, Goblin to Machine, Element to Elemental, Naga to Aquan, Dwarf to Cyborg, God to Olympos, Human stays Human (humanoid Digimon). Demon Hunter is Demon Slayer. Ogre (one piece) was dropped; Alchemist is Machine only.

| Trait | Kind | Breakpoints | Effect (final values) | Dota Auto Chess source |
|---|---|---|---|---|
| Beast | race | 2/4/6 | all allies +8/16/26% AD | Beast, Power of the Wild |
| Dragon | race | 3 | Dragons start with 70% mana | Dragon (3), full mana |
| Undead | race | 2/4/6 | enemies -15/22/35 armor | Undead, Horror |
| Demon | race | 1 | only with one kind of Demon (or Demon Slayer 2): attacks add 40% true damage; an enemy Demon Slayer cancels it | Demon, Fel Power (50%) |
| Fairy | race | 3/6/9 | Fairies +28/42/56% dodge | Elf, Evasion |
| Human | race | 2/4/6 | 18/25/32% chance on attack to silence 3 s | Human, Censorship (4 s) |
| Machine | race | 3/6 | 3: one random ally +25 armor, 1% HP/s; 6: all allies +30 armor, 1.2% HP/s | Goblin, Nanobots |
| Dinosaur | race | 2/4 | Dinosaurs +220/260 HP | Orc, Battle Hardened |
| Insect | race | 2/4 | Insects +25% AS; at 4 all allies +3% more | Troll, Voodoo Frenzy |
| Elemental | race | 2/4 | Elementals +15 armor/MR; 30% (at 4: 40%, all allies) to stone a melee attacker 1.5 s | Elemental, Elemental Essence |
| Aquan | race | 2/4 | all allies +22/50 MR | Naga, Scale Armor |
| Cyborg | race | 1 | +1 range, +10% AD | Dwarf, Take Aim |
| Olympos | race | 1/2 | only while no other race is active: all allies +2/2.5 mana per attack | God, -50% cooldown |
| Warrior | class | 3/6/9 | Warriors +20/45/80 armor | Warrior, Fortified Armor |
| Mage | class | 3/6 | team magic damage +14/36% | Mage, -40% MR |
| Hunter | class | 3/6 | Hunters +22/50% AD | Hunter, Deadly Shot |
| Assassin | class | 3/6 | leap; 7% crit for 190% / 13% crit for 230% | Assassin, Coup de Grace |
| Knight | class | 2/4/6 | when hit: 1/2/3 rolls of 30% for an 18% damage shield (3 s, stacks) | Knight, Divine Protection |
| Druid | class | 2/4 | 2-copy digivolve; Champion Druids need 2 Druids and finals need 4 to fight at full strength; heal the lowest ally 3/5% max HP every 4 s | Druid, Synergy |
| Mech | class | 2/4 | Mechs regenerate 1.5/3.5% HP per second | Mech, Self Repair |
| Shaman | class | 2 | hex a random enemy for 4 s at the start | Shaman, Hex (6 s) |
| Warlock | class | 3/6 | all allies +4/9% lifesteal | Warlock, Soul Leech (15/25% in code) |
| Demon Slayer | class | 1/2 | cancel enemy Demon power, +15/25% AS, +25/40% damage to Demons; at 2 your Demons ignore the one-kind rule | Demon Hunter, Shattered Soul |
| Priest | class | 1/2 | -20/35% Tamer damage on a loss, +10/25% healing | Priest, Barrier (-20%) |
| Wizard | class | 2 | rule change: other traits that need 4+ units need one fewer; Wizards +35 AP | Wizard (2), 2019-06-26 |

Count rules live in `onCountTraits` (`countRules` in the mech file): Wizard first, then Demon (removed when two or more kinds without Demon Slayer 2), then Olympos (removed when any other race is active). Removing the count keeps the trait panel and the balance reports honest.

## Mechanics

- **Druid gate.** Druid lines carry `combine: 2`, so 2 copies make a Champion and 4 make a final. A set-level `onCombatStart` hook scales a Druid down to the form its trait level allows (Champions need Druid 2, finals need Druid 4), like Dota Auto Chess where (2) and (4) Druids unlock each step. Wizard can turn 3 Druids into the 4 breakpoint, as it did in Dota Auto Chess (the standalone later removed that).
- **Wizard rule.** With 2 Wizards, any other race or class whose breakpoint is 4 or more needs one fewer unit (Fairy 6 with 5, Aquan 4 with 3, Warrior 9 with 8, Druid 4 with 3).
- **Digi-Egg (Io / Strange Egg).** A consumable item. Drag it onto a Rookie or Champion and it hatches into one more copy of that form, taken from the shared pool, then digivolving runs. It drops from creep rounds from round 15 (5% per roll) and from the Strange Egg Nest relic. AI Tamers use it on a form one copy short of digivolving (`onAiTurn`). The standalone's Strange Egg was a buyable piece; an item keeps the shared-pool count exact without engine changes.
- **Creep loot.** Creep rounds 1, 2, 3, 10, 15, ... 50 (x-5 from stage 2, five rounds a stage). Win: the next round offers 1 of 3 items (offer kind `ac_loot`). Lose: one random item drops on the board. Tier odds by round follow the 2019 drop table compressed to one item per round: rounds 1-3 tier 1 only; 10: 60/40; 15: 30/50/20; 20: 20/40/35/5; 25: 5/35/40/20; 30: 0/25/45/30; 35+: 0/10/45/45 (tier 4 = a finished item).
- **Relics.** Pick 1 of 3 at 3-2, 5-2 and 7-2 (right after the creep rounds 10, 20, 30); a new relic replaces the old one (offer kind `ac_relic`). Relics are global items: Pirate Treasure (+2 gold a round), Weaponsmith's Forge (a finished item), Great Vault (interest cap 8), Light and Shadow Charm (double streak gold), Recruiter Badge (shop rolls one level higher), Grace of Nature (team regen), Sin Reaper (+10% damage), Strange Egg Nest (2 Digi-Eggs), Monopoly Card (a free reroll each round), Master Key (+1 team size), Bargain Tag (+1 gold per sale).

## Items

Dota Auto Chess drops parts that work on their own and combine by recipe. The engine combines two parts on one Digimon, so three-part and chained recipes (Desolator, Mjollnir, Daedalus, Dagon 1-5) were folded into two-part recipes. Every name maps to its Dota item in the description.

- Tier 1 parts: Attack Chip (Blades of Attack), Defense Chip (Chainmail), Speed Chip (Quarterstaff), Recovery Disk (Ring of Health), Vampire Fang (Morbid Mask), Resist Cape (Cloak), MP Chip (Void Stone).
- Tier 2: HP Chip (Vitality Booster), Brain Chip (Staff of Wizardry), Power Claw (Broadsword).
- Tier 3: Hyper Chip (Hyperstone), Dark Edge (Demon Edge), Holy Ring (Ultimate Orb).
- Recipes (33): Crystal Claw (Crystalys), Grand Claw (Daedalus), Thunder Disk (Maelstrom), Thunder Hammer (Mjollnir), Berserk Mask (Mask of Madness), Fury Axe (Battle Fury), Corrosion Blade (Desolator), Black Gear (Black King Bar), Digicore (Heart of Tarrasque), Chrome Digizoid Armor (Assault Cuirass), Reflect Mail (Blade Mail), Divine Sword (Divine Rapier), Refresh Disk (Refresher Orb), Data Hood (Hood of Defiance), Brave Shield (Vanguard), Numemon Wand (Scythe of Vyse), Moon Chip (Moon Shard), True Strike Lance (Monkey King Bar), Hellfire Rod (Dagon), Mystic Staff, Sage Orb (Kaya), Steady Disk (Perseverance), Blood Ring (Satanic), Stun Hammer (Skull Basher), Frost Ring (Eye of Skadi), Medical Kit (Mekansm), Radiant Crest (Radiance), Dragon Lance, Soul Disk (Soul Booster), Iron Shield (Poor Man's Shield), Wind Feather (Butterfly), Iron Helm (Helm of Iron Will), Ward Cape (Pipe of Insight).
- Items hold at most 3 per Digimon (engine limit; Dota Auto Chess had 6 slots).

## Economy and rounds (values chosen)

| Rule | Value | Source and conflicts |
|---|---|---|
| Start | 100 HP, 1 gold | Dota courier HP 100 |
| Base income | min(round, 5) | 2019-02 to 2020-05 code. Chosen over the 2020-05-27 rule min(round/2, 5) and the later cap of 4. |
| Interest | 1 per 10 gold, max 5 | Mid-2019 code. The wiki's cap of 4 is from 2021. |
| Streaks | 3-4: +1, 5-6: +2, 7+: +3 (wins or losses) | Mid-2019 code formula. Not the wiki's 3-5/6-8/9+ or the 2021 table 2-4/5-9/10+. |
| Win gold | +1 per won fight (creeps too) | all eras |
| Reroll | 2 gold | all eras |
| XP | +1 a round from round 2; buy 4 XP for 5 gold; max level 10 | 2019 code (cost 5 until 2020-05-27, table max 10) |
| Level XP (cumulative) | 1, 2, 4, 8, 16, 32, 56, 88, 128 | 2019 table |
| Shop odds | 2019-01 to 2021 table, levels 1-10 | chosen over the 2022/2023 tables |
| Pool | 45/30/25/15/10 copies by cost | 2019-06 snapshots (the wiki era). Later 20/20/15/15/10. |
| Player damage | floor(1 + unit level / 3) per surviving enemy, unit level = cost + 2 x (stars - 1), plus a base of 0 through stage 2, then stage - 2 (max 6) | per-unit formula from all code snapshots; the base is ours (2019 had none) so games end near round 35 |
| Creep rounds | 1, 2, 3, 10, 15, ..., 50; only the per-unit damage on a loss | 2019 code. Creep damage was removed in 2020-05; kept here at the per-unit rate. |
| Rounds | 5 a stage; the AI levels toward stage + 2 | |

Not carried over: fatigue after round 50 (games end well before it), the per-game legendary subset, SSR pieces, Dismiss, Duo mode, bench auto-combine rules, item drops from eliminated players.

## Balance

Tools: `node tools/balance.cjs --duel 12000 6 1 --set autochess` (random 7-unit boards with the same cost mix, no items) and `node tools/balance.cjs 160 6 --set autochess` (full 8-AI games).

Final numbers (2026-10-15):

- Duel, stage 1, 12000 fights: every race and class between 45.4% (Fairy) and 57.7% (Dinosaur). Others: Druid 57.2, Warrior 57.1, Wizard 57.0, Machine 56.8, Aquan 56.5, Mech 55.5, Olympos 55.4, Dragon 55.4, Insect 55.1, Mage 55.1, Demon 54.5, Knight 54.5, Warlock 54.3, Shaman 53.7, Human 53.5, Elemental 52.7, Demon Slayer 52.4, Undead 50.9, Priest 50.0, Beast 50.0, Hunter 49.8, Assassin 49.0, Cyborg 47.9.
- Duel lines by cost (average, min-max): 1: 50.2 (44.2-56.8), 2: 50.0 (43.8-57.6), 3: 49.8 (37.9-56.4), 4: 49.9 (42.8-55.0), 5: 50.2 (45.8-53.5). The lows are the Druid lines (Mushroomon, Palmon, Gaomon): a duel fields one copy of each line, so a lone Druid is held to the lower form by the Druid gate. In real games they cost half as many copies.
- Duel at stage 2 (all final forms) is less even (Fairy 31%, Druid 38%, Wizard 62%) for the same reason: a final-form Druid without Druid 4 fights as a Champion, and Fairy contains two Druids.
- Full games, 240 games: Druid top-4 60%, average place 3.97 (it was 82% and 2.66 before the gate). Wizard top-4 50%, average 4.08 (not a must-pick). Late-game traits that need 3+ units with items lead: Assassin 79% (n=90), Cyborg 69%, Fairy 67%, Demon Slayer 65%, Warlock 64%. Lowest: Machine 44%, Mech 46%, Warrior 47%. Assassin stays the strongest full-game trait even after four nerfs to the trait and its lines; its duel rate is 49%, so the edge comes from leaping onto item carries.
- Game length: games end at stage 6-9, rounds 29-41 (sim: 20 games, median 7-4/8-1), close to Dota Auto Chess. Creep rounds are won 69% in stage 1 and 83-100% later.

Tuning history: Olympos started at 80% (mana per attack cut from 5/9 to 2/2.5), Druid at 69% duel / 82% full (gate added, heal cut), Insect 4 at 83% (all-ally speed cut to 3%), Elemental at 39% (armor/MR added, stone chance up).

## Tests

- `node tests/sim.js 20 --set autochess`: pass (pool, caps and gold invariants, strict determinism; 709 rounds, 2964 fights).
- `node tests/lockstep.js 5 --set autochess`: lockstep ok.
- `node tests/sets.cjs --set autochess`: pass headless and in the browser (port 8179, temp copy): solo game, codex, picker, lobby, no page errors.
- `node tests/moves.cjs <playwright> <out> soft --set autochess`: pass; 82 lines, no generated looks, every spec valid, every ability played once.
- Solo browser game through the real UI to round 4-2: creep loot and relic offers work, trait panel, shop, XP and combat render.

## Engine notes

- The item drag preview only combines with a held component that has a recipe with the dragged one, so partial recipe tables work.
- Trait texts use `bpDesc` when a trait has it and `fmt` otherwise (unit tooltip, trait panel and item cards).
