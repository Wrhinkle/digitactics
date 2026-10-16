# Dota Underlords: Season One (February 2020 to November 2020)

Season One started on 2020-02-25 when the game left Early Access. It added City Crawl, a paid Battle Pass, Hats and a hero rotation. On 2020-08-27 "New Blood On The Streets" rotated the roster again inside Season One. The last patch was 2020-11-19. No Season Two launched.

## Season One launch changes (2020-02-25)

- Item rounds every 4 rounds (1, 4, 8, 12 ...), one reroll per offer.
- Ace effects removed; Ace tier renamed Tier 5.
- Poison healing reduction raised from 10% to 15% per stack.
- Rotated out: Alchemist, Bounty Hunter, Clockwerk, Enchantress, Gyrocopter, Kunkka, Sniper, Timbersaw, Tinker; Scrappy and Inventor alliances; Aeon Disk, Healing Ward.
- Back in: Arc Warden, Magnus, Luna, Windranger, Mirana, Tidehunter, Enigma, Nature's Prophet, Templar Assassin.
- New alliances: Summoner (2/4: summons +20/40% damage), Vigilant (2/4: retarget enemy casters), Void (3: 40% chance for +5% max-HP pure damage, later 4%).
- New items: Armlet of Mordiggian, Crown of Antlers, Dragon Lance, Kaden's Blade, Leg Breaker's Fedora, Mango Tree, Mantle of the Flayed Twins, Quelling Blade, Ristul Circlet; Big-Time Contract renamed Ogre Cap.
- Hats: items that give the wearer an extra alliance (Ogre Cap: Blood-Bound; Mantle of the Flayed Twins: Deadeye; Crown of Antlers: Hunter; Leg Breaker's Fedora: Brute; Ristul Circlet: Demon; later Pirate Hat: Swordsman).

## Mid-season changes

- 2020-04-23: round-16 item odds changed to 100% tier 3.
- 2020-04-30: pool sizes cut to 30/20/15/12/10.
- 2020-05-08 onward: 3-star effects added hero by hero (for example Shadow Shaman hexes at 3 stars, Lich gains Chain Frost, Luna gains Eclipse).
- 2020-05-28: shop tier odds changed to favour 3-starring tier 1-3 heroes.
- 2020-06-12: tier-3 pool raised to 18.

## New Blood On The Streets (2020-08-27)

- Classic mode (no Underlords, creep rounds).
- Rank compression (see Ranked in `underlords.md`).
- New heroes: Vengeful Spirit, Meepo, Spirit Breaker, Spectre, Death Prophet, Pangolier, Rubick, Wraith King.
- Returning: Anti-Mage, Bounty Hunter, Enchantress, Phantom Assassin, Kunkka, Alchemist, Puck, Lina.
- New alliances: Fallen (3/6), Shaman (2/4/6, combat-start summon), Swordsman (3/6), Rogue (2/4), Poisoner (3), Magus (1).
- New items: Craggy Coat, Heaven's Halberd, Monkey King Bar, Necronomicon, Paladin Sword, Pirate Hat, Witless Shako. Vitality Booster, Hood of Defiance and Headdress became row auras (wearer and allies 1 cell left/right).
- Rotated out: Arc Warden, Bloodseeker, Broodmother, Disruptor, Enigma, Io, Morphling, Necrophos, Nyx Assassin, Ogre Magi, Razor, Sand King, Shadow Fiend, Tiny, Warlock, Weaver, Witch Doctor; Blood-Bound, Deadeye, Druid, Insect, Primordial, Warlock alliances; Aegis, Crystalys, Force Staff, Hand of Midas, Mantle of the Flayed Twins, Ogre Cap, Orb of Venom, Tombstone.

## City Crawl

Single-player campaign added with Season One (2020-02-25). Four districts (Little Revtel, New Tenebrous, Port Slipshade, The Warrens), each with 36 challenges: 13 Objectives (match tasks tied to a specific Underlord), 19 Street Fights (Knockout-style fights vs fixed boards or restricted bots; 2 lives, no timer, levels 6/8/10 by round, fixed base gold per fight, free reroll on loss) and 4 Puzzles (single round, limited units/cells/items and a net-worth cap; score = net worth, unit count, fight time, ranked on leaderboards). Clearing nodes opens paths. Street Fight item drops (14 per Underlord). City Keys skip a node. After clearing the city you can Prestige up to 5 times: Street Fights get harder (your units deal 25% less damage, +10% per prestige, up to 65%) and Street Fight items get stronger. Part of the content required the paid Battle Pass. [W: City Crawl]

## Battle Pass

From 2020-02-25. Over 100 reward levels, 1,000 XP per level. Many rewards free; full access (all City Crawl content and all rewards) cost $4.99 and gave no gameplay advantage. Match XP by place: Standard/Duos 400/300/250/200/150/100/50/30, Knockout 150/120/100/80/60/40/20/10. Daily challenges worth 200/300/400 (kill units, earn end-of-turn gold, cast spells, deal magic damage, heal, lifesteal, summon). City Crawl tasks gave 300/600/1,000 XP. Rewards were cosmetics: boards, board props, wanted posters, Underlord outfits, win-streak effects, sprays, and Street Fight items. [N: Season One; W: Season One Battle Pass]

## Snapshot: Season One launch (City Crawl) (2020-02-25)

Source: Fortify-Labs/Fortify services/shared/src/assets/s1_units.json (Season One unit data). This file has unit data only (no alliance or item data).

Alliance breakpoints changed at Season One per the patch notes: Deadeye 2 -> 1 unit; Healer 3 -> 2/4 (25/50% healing); new Summoner 2/4, Vigilant 2/4, Void 3; Knight reduction 15/22/28% -> 15/20/25%; Savage reworked to +6/8/10 attack damage per hit for Savage units / all heroes / all units. Other breakpoints were unchanged from the 2019-12-06 snapshot as far as the notes show.

### Alliance membership

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | unknown | 7 | Ember Spirit, Faceless Void, Nyx Assassin, Queen of Pain, Slark, Templar Assassin, Viper | unknown for this snapshot (see final text and change log) |
| Blood-Bound | unknown | 3 | Bloodseeker, Ogre Magi, Warlock | unknown for this snapshot (see final text and change log) |
| Brawny | unknown | 6 | Axe, Beastmaster, Bristleback, Disruptor, Juggernaut, Snapfire | unknown for this snapshot (see final text and change log) |
| Brute | unknown | 5 | Axe, Doom, Lifestealer, Ogre Magi, Treant Protector | unknown for this snapshot (see final text and change log) |
| Champion | unknown | 1 | Legion Commander | unknown for this snapshot (see final text and change log) |
| Deadeye | unknown | 1 | Bloodseeker | unknown for this snapshot (see final text and change log) |
| Demon | unknown | 6 | Chaos Knight, Doom, Queen of Pain, Shadow Demon, Shadow Fiend, Terrorblade | unknown for this snapshot (see final text and change log) |
| Dragon | unknown | 3 | Dragon Knight, Snapfire, Viper | unknown for this snapshot (see final text and change log) |
| Druid | unknown | 5 | Io, Lone Druid, Magnus, Nature's Prophet, Treant Protector | unknown for this snapshot (see final text and change log) |
| Healer | unknown | 4 | Dazzle, Necrophos, Omniknight, Warlock | unknown for this snapshot (see final text and change log) |
| Heartless | unknown | 7 | Abaddon, Drow Ranger, Lich, Lifestealer, Necrophos, Pudge, Shadow Demon | unknown for this snapshot (see final text and change log) |
| Human | unknown | 7 | Crystal Maiden, Dragon Knight, Keeper of the Light, Legion Commander, Lycan, Omniknight, Sven | unknown for this snapshot (see final text and change log) |
| Hunter | unknown | 7 | Beastmaster, Drow Ranger, Medusa, Mirana, Terrorblade, Weaver, Windranger | unknown for this snapshot (see final text and change log) |
| Insect | unknown | 4 | Broodmother, Nyx Assassin, Sand King, Weaver | unknown for this snapshot (see final text and change log) |
| Knight | unknown | 7 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight, Sven | unknown for this snapshot (see final text and change log) |
| Mage | unknown | 7 | Crystal Maiden, Keeper of the Light, Lich, Morphling, Ogre Magi, Razor, Storm Spirit | unknown for this snapshot (see final text and change log) |
| Primordial | unknown | 6 | Arc Warden, Enigma, Io, Morphling, Razor, Tiny | unknown for this snapshot (see final text and change log) |
| Savage | unknown | 6 | Bristleback, Lone Druid, Lycan, Magnus, Sand King, Tusk | unknown for this snapshot (see final text and change log) |
| Scaled | unknown | 6 | Medusa, Slardar, Slark, Sven, Tidehunter, Venomancer | unknown for this snapshot (see final text and change log) |
| Spirit | unknown | 4 | Earth Spirit, Ember Spirit, Storm Spirit, Void Spirit | unknown for this snapshot (see final text and change log) |
| Summoner | unknown | 6 | Arc Warden, Lone Druid, Lycan, Nature's Prophet, Shadow Shaman, Venomancer | unknown for this snapshot (see final text and change log) |
| Troll | unknown | 5 | Batrider, Dazzle, Shadow Shaman, Troll Warlord, Witch Doctor | unknown for this snapshot (see final text and change log) |
| Vigilant | unknown | 5 | Drow Ranger, Luna, Mirana, Templar Assassin, Windranger | unknown for this snapshot (see final text and change log) |
| Void | unknown | 4 | Enigma, Faceless Void, Templar Assassin, Void Spirit | unknown for this snapshot (see final text and change log) |
| Warlock | unknown | 6 | Broodmother, Disruptor, Necrophos, Shadow Fiend, Warlock, Witch Doctor | unknown for this snapshot (see final text and change log) |
| Warrior | unknown | 8 | Earth Spirit, Juggernaut, Pudge, Slardar, Tidehunter, Tiny, Troll Warlord, Tusk | unknown for this snapshot (see final text and change log) |

### Heroes in pool (63)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Arc Warden | Primordial, Summoner | 650/1300/2600 | 35-45/70-90/140-180 | 1 | 4 | 5 | 5 | 100 | 280 |
| T1 | Batrider | Troll, Knight | 750/1500/3000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bloodseeker | Blood-Bound, Deadeye | 900/1800/3450 | 40-60/100-120/160-190 | 1.7 | 1 | 0 | 20 | 100 | 850 |
| T1 | Crystal Maiden | Human, Mage | 700/1400/2800 | 40-45/80-90/160-180 | 1.5 | 4 | 5 | 0 | 100 | 275 |
| T1 | Drow Ranger | Heartless, Vigilant, Hunter | 600/1200/2400 | 50-56/100-112/200-224 | 1.2 | 4 | 5 | 0 | 100 | 285 |
| T1 | Magnus | Savage, Druid | 1100/2200/4400 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 100 | 280 |
| T1 | Nyx Assassin | Insect, Assassin | 825/1485/3300 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 |
| T1 | Razor | Primordial, Mage | 750/1500/3000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 |
| T1 | Shadow Demon | Heartless, Demon | 900/1800/3600 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 |
| T1 | Snapfire | Brawny, Dragon | 935/1875/3750 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 |
| T1 | Tiny | Primordial, Warrior | 1000/2000/4000 | 65-110/130-220/260-440 | 1.9 | 1 | 5 | 0 | 100 | 290 |
| T1 | Tusk | Savage, Warrior | 1050/2100/4200 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 |
| T1 | Venomancer | Scaled, Summoner | 750/1500/3000 | 35-45/70-90/140-180 | 1.2 | 3 | 0 | 20 | 80 | 275 |
| T1 | Warlock | Blood-Bound, Warlock, Healer | 975/2175/4275 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 |
| T1 | Weaver | Insect, Hunter | 675/1350/2700 | 45-55/90-110/180-220 | 0.9 | 2 | 5 | 0 | 20 | 290 |
| T2 | Beastmaster | Brawny, Hunter | 900/1800/3600 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T2 | Bristleback | Brawny, Savage | 1350/2700/5250 | 100-130/200-260/400-520 | 1.7 | 1 | 5 | 0 | 20 | 300 |
| T2 | Chaos Knight | Demon, Knight | 1125/2250/4500 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 75 | 320 |
| T2 | Dazzle | Troll, Healer | 1125/2250/4500 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 |
| T2 | Earth Spirit | Spirit, Warrior | 1275/2550/5100 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 |
| T2 | Legion Commander | Human, Champion | 675/1150/2300 | 60-75/96-120/184-240 | 1.25 | 1 | 5 | 0 | 100 | 315 |
| T2 | Luna | Vigilant, Knight | 750/1500/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Nature's Prophet | Druid, Summoner | 750/1500/3000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 80 | 290 |
| T2 | Ogre Magi | Blood-Bound, Brute, Mage | 1200/2400/4500 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 |
| T2 | Pudge | Heartless, Warrior | 1500/3750/7010 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin | 825/1485/3300 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 |
| T2 | Slardar | Scaled, Warrior | 975/1950/3900 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T2 | Storm Spirit | Spirit, Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 |
| T2 | Windranger | Vigilant, Hunter | 825/1650/3450 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T2 | Witch Doctor | Troll, Warlock | 825/1650/3300 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 |
| T3 | Abaddon | Heartless, Knight | 1125/2250/4500 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 |
| T3 | Broodmother | Insect, Warlock | 1300/2600/6240 | 50-70/100-140/240-336 | 1 | 1 | 5 | 0 | 100 | 275 |
| T3 | Ember Spirit | Spirit, Assassin | 825/1650/3300 | 80-100/160-200/320-400 | 1.5 | 1 | 5 | 0 | 100 | 305 |
| T3 | Enigma | Void, Primordial | 900/1800/3600 | 35-50/70-100/140-200 | 1.5 | 3 | 5 | 40 | 100 | 290 |
| T3 | Io | Primordial, Druid | 800/1600/3000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 |
| T3 | Juggernaut | Brawny, Warrior | 900/1950/3600 | 65-70/155-165/280-300 | 1/0.9/0.8 | 1 | 5 | 0 | 100 | 300 |
| T3 | Lifestealer | Heartless, Brute | 1010/2025/4860 | 55-60/110-120/220-240 | 1.5/1.4/1.3 | 1 | 5 | 0 | 0 | 300 |
| T3 | Lycan | Human, Savage, Summoner | 1125/2250/5400 | 50-60/100-120/240-288 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Morphling | Primordial, Mage | 935/1875/3000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 |
| T3 | Omniknight | Human, Knight, Healer | 1100/2200/5280 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 60 | 300 |
| T3 | Shadow Fiend | Demon, Warlock | 675/1350/2700 | 77-88/154-176/369-422 | 1.1 | 3 | 5 | 0 | 100 | 305 |
| T3 | Shadow Shaman | Troll, Summoner | 850/1700/3400 | 50-60/100-120/200-240 | 1.25 | 3 | 5 | 0 | 70 | 285 |
| T3 | Terrorblade | Demon, Hunter | 975/1950/4680 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 30 | 80 | 310 |
| T3 | Treant Protector | Druid, Brute | 1125/2250/3750 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 |
| T3 | Viper | Dragon, Assassin | 825/1650/4500 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 70 | 275 |
| T4 | Disruptor | Brawny, Warlock | 1000/2000/4000 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 |
| T4 | Doom | Demon, Brute | 1575/3150/6300 | 110-120/220-240/440-480 | 1.1 | 1 | 15 | 0 | 100 | 285 |
| T4 | Keeper of the Light | Human, Mage | 825/1650/3300 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 |
| T4 | Lone Druid | Savage, Druid, Summoner | 1200/2400/4500 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 |
| T4 | Mirana | Vigilant, Hunter | 1125/2250/4500 | 56-68/112-136/224-272 | 1 | 4 | 5 | 0 | 70 | 310 |
| T4 | Necrophos | Heartless, Warlock, Healer | 1275/2550/5100 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 |
| T4 | Slark | Scaled, Assassin | 900/1800/3600 | 60-70/90-110/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T4 | Sven | Human, Scaled, Knight | 1425/2850/5700 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 150 | 300 |
| T4 | Templar Assassin | Vigilant, Void, Assassin | 1125/2025/4500 | 90-110/180-220/360-440 | 1.375 | 3 | 5 | 0 | 100 | 305 |
| T4 | Tidehunter | Scaled, Warrior | 1650/4050/8250 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 |
| T4 | Void Spirit | Void, Spirit | 1275/2550/5100 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 |
| T5 | Axe | Brawny, Brute | 3000/6000/12000 | 50-60/100-120/200-240 | 1.5 | 1 | 15 | 0 | 50 | 295 |
| T5 | Dragon Knight | Human, Dragon, Knight | 1400/2800/5600 | 55-75/110-150/220-300 | 1.2 | 1 | 10 | 0 | 100 | 295 |
| T5 | Faceless Void | Void, Assassin | 1500/2700/6000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 |
| T5 | Lich | Heartless, Mage | 1275/2550/5100 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T5 | Medusa | Scaled, Hunter | 1275/2550/4950 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 |
| T5 | Sand King | Savage, Insect | 1500/3000/6000 | 70-80/140-160/270-320 | 1.3 | 1 | 10 | 0 | 100 | 290 |
| T5 | Troll Warlord | Troll, Warrior | 1800/3000/6000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 |

## Snapshot: Final patch (after New Blood On The Streets) (2020-11-19)

### Alliances

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6 | 7 | Bounty Hunter, Ember Spirit, Faceless Void, Phantom Assassin, Queen of Pain, Slark, Templar Assassin | (3) All Assassins gain a 15% chance to Critical Hit for 325% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 75% for 3 seconds. / (6) All Assassins gain a 25% chance to Critical Hit for 400% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 100% for 3 seconds. |
| Brawny | 2/4 | 5 | Axe, Beastmaster, Bristleback, Juggernaut, Snapfire | (2) All Brawny units gain +5 maximum health for each kill by any of your team Brawny units throughout the game. Bonus is multiplied by star level of Brawny unit. Summons count at 0.25 kill. (HP bonus varies by game mode.) / (4) All Brawny units gain +10 maximum health for each kill by any of your team Brawny units throughout the game. Bonus is multiplied by star level of Brawny unit. Summons count at 0.25 kill. (HP bonus varies by game mode.) |
| Brute | 2/4 | 5 | Alchemist, Axe, Doom, Lifestealer, Spirit Breaker | (2) Whenever a Brute attacks an enemy it applies a -30% damage debuff for 4 seconds. When the debuff is initially applied it deals 80 damage. Brutes prefer to attack enemies that do not have the debuff. / (4) Whenever a Brute attacks an enemy it applies a -50% damage debuff for 4 seconds. When the debuff is initially applied it deals 120 damage. Brutes prefer to attack enemies that do not have the debuff. |
| Champion | 1 | 1 | Legion Commander | (1) Champions receive all Alliance bonuses. In addition, they gain 3% extra damage and 8% max health per active Alliance. |
| Demon | 1 | 6 | Chaos Knight, Doom, Queen of Pain, Shadow Demon, Spectre, Terrorblade | (1) Whenever a Demon casts a spell, for 4 seconds, allied Demons are silenced and the caster gains +50% bonus Pure damage for each allied Demon. |
| Dragon | 2 | 4 | Dragon Knight, Puck, Snapfire, Viper | (2) All Dragon units unlock an additional draconic ability. |
| Fallen | 3/6 | 6 | Abaddon, Death Prophet, Lich, Terrorblade, Vengeful Spirit, Wraith King | (3) When each Fallen unit first dies, allies within 2 cells are healed for 250 HP and gain +10% bonus damage to attacks and spells. A Fallen unit can revive 0 times per combat, resurrecting with 40% health. / (6) When each Fallen unit first dies, allies within 2 cells are healed for 250 HP and gain +10% bonus damage to attacks and spells. A Fallen unit can revive 1 times per combat, resurrecting with 40% health. |
| Healer | 2/4 | 4 | Dazzle, Enchantress, Omniknight, Treant Protector | (2) All friendly healing is amplified by 25%. / (4) All friendly healing is amplified by 45%. |
| Heartless | 2/4/6 | 6 | Death Prophet, Drow Ranger, Lifestealer, Pudge, Shadow Demon, Vengeful Spirit | (2) All Enemies lose 4 Armor. / (4) All Enemies lose 9 Armor. / (6) All Enemies lose 15 Armor. |
| Human | 2/4/6 | 8 | Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Legion Commander, Lina, Lycan, Omniknight | (2) Allies generate 4 mana per second. / (4) Allies generate 8 mana per second. / (6) Allies generate 15 mana per second. |
| Hunter | 3/6 | 7 | Anti-Mage, Beastmaster, Drow Ranger, Medusa, Mirana, Terrorblade, Windranger | (3) All Hunters have a 30% chance of quickly performing 2 attacks. / (6) All Hunters have a 38% chance of quickly performing 3 attacks. |
| Knight | 2/4/6 | 7 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight, Sven | (2) Knight units take 15% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 15% less physical and magical damage. / (4) Knight units take 20% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 20% less physical and magical damage. / (6) Knight units take 25% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 25% less physical and magical damage. |
| Mage | 3/6 | 7 | Crystal Maiden, Keeper of the Light, Lich, Lina, Puck, Rubick, Storm Spirit | (3) Enemies take 35% more Magic damage. / (6) Enemies take 70% more Magic damage. |
| Magus | 1 | 1 | Rubick | (1) Once per battle, when a Magus unit's health drops below 50%, the unit becomes invulnerable and unable to attack for 3 seconds. |
| Poisoner | 3 | 5 | Alchemist, Dazzle, Queen of Pain, Venomancer, Viper | (3) Attacks from Poisoners apply stacks of Poison. Enemies lose 7 Attack Speed per stack of Poison. |
| Rogue | 2/4 | 5 | Alchemist, Anti-Mage, Bounty Hunter, Meepo, Sven | (2) Allied Rogue units gain 20% evasion. Whenever a Rogue unit evades an attack, it gains +10 Attack Damage. / (4) Allied Rogue units gain 40% evasion. Whenever a Rogue unit evades an attack, it gains +20 Attack Damage. |
| Savage | 2/4/6 | 7 | Bristleback, Lone Druid, Lycan, Magnus, Pangolier, Spirit Breaker, Tusk | (2) Allied Savage units gain +5 Attack Damage each time they land an attack. / (4) Allied Savage units and all allied summons gain +8 Attack Damage each time they land an attack. / (6) All allied units gain +13 Attack Damage each time they land an attack. |
| Scaled | 2/4 | 5 | Medusa, Slardar, Slark, Tidehunter, Venomancer | (2) Allies gain +30% Magic Resistance / (4) Allies gain +55% Magic Resistance |
| Shaman | 2/4/6 | 6 | Beastmaster, Enchantress, Lone Druid, Magnus, Nature's Prophet, Treant Protector | (2) When combat starts, a Small Wolf is summoned to help. / (4) When combat starts, a Wildwing Hatchling is summoned to help. / (6) When combat starts, a Black Dragon is summoned to help. |
| Spirit | 3 | 4 | Earth Spirit, Ember Spirit, Storm Spirit, Void Spirit | (3) When a Spirit casts an ability they enter a charged state. When 3 Spirits are in a charged state they perform the forbidden technique of the triangular Delta Slam, each dealing 130 magic damage to all enemies caught inside and applying their elemental effects for 1.5 seconds. Damage and effect duration are multiplied by 1.5 at 2* and 2 at 3*. |
| Summoner | 2/4 | 6 | Lone Druid, Lycan, Meepo, Nature's Prophet, Shadow Shaman, Venomancer | (2) Allied summoned units gain +20% damage. / (4) Allied summoned units gain +40% damage. |
| Swordsman | 3/6 | 7 | Ember Spirit, Juggernaut, Kunkka, Pangolier, Phantom Assassin, Sven, Wraith King | (3) Each Swordsman unit gains +10% bonus damage for each enemy unit adjacent to it. / (6) Each Swordsman unit gains +25% bonus damage for each enemy unit adjacent to it. |
| Troll | 2/4 | 4 | Batrider, Dazzle, Shadow Shaman, Troll Warlord | (2) All Troll units gain +40 Attack Speed and other Allies gain +20 Attack Speed. / (4) All Troll units gain +70 Attack Speed and other Allies gain +40 Attack Speed. |
| Vigilant | 2/4 | 5 | Drow Ranger, Luna, Mirana, Templar Assassin, Windranger | (2) Whenever an enemy unit casts a spell, Allied Vigilant units target that enemy if it is within their range. The damage of the next attack against this target is increased by 40%. / (4) Whenever an enemy unit casts a spell, All allied units target that enemy if it is within their range. The damage of the next attack against this target is increased by 50%. |
| Void | 3 | 4 | Faceless Void, Spectre, Templar Assassin, Void Spirit | (3) All allied heroes have a 40% chance to deal an additional 4% of the target's Max Health as Pure Damage. This effect can only occur once every 0.5 seconds. |
| Warrior | 3/6 | 7 | Earth Spirit, Kunkka, Pudge, Slardar, Tidehunter, Troll Warlord, Tusk | (3) All Warriors gain +10 Armor. / (6) All Warriors gain +25 Armor. |

### Heroes in pool (62)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Rogue, Hunter | 750/1500/3000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 |
| T1 | Batrider | Troll, Knight | 750/1500/3000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bounty Hunter | Rogue, Assassin | 825/1650/3300 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 |
| T1 | Crystal Maiden | Human, Mage | 700/1400/2800 | 40-45/80-90/160-180 | 1.5 | 4 | 5 | 0 | 100 | 275 |
| T1 | Dazzle | Troll, Healer, Poisoner | 900/1800/3600 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 |
| T1 | Drow Ranger | Heartless, Vigilant, Hunter | 600/1200/2400 | 50-56/100-112/200-224 | 1.2 | 4 | 5 | 0 | 100 | 285 |
| T1 | Enchantress | Shaman, Healer | 700/1400/2800 | 60-70/120-140/240-280 | 1.5 | 2 | 5 | 0 | 100 | 315 |
| T1 | Lich | Fallen, Mage | 750/1500/3000 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T1 | Magnus | Savage, Shaman | 1100/2200/4400 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 100 | 280 |
| T1 | Phantom Assassin | Assassin, Swordsman | 825/1650/3300 | 40-60/80-120/160-240 | 1.2 | 1 | 5 | 0 | 100 | 305 |
| T1 | Shadow Demon | Heartless, Demon | 900/1800/3600 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 |
| T1 | Slardar | Scaled, Warrior | 900/1800/3600 | 60-85/120-170/240-340 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T1 | Snapfire | Brawny, Dragon | 950/1900/3800 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 |
| T1 | Tusk | Savage, Warrior | 1150/2300/4600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 |
| T1 | Vengeful Spirit | Fallen, Heartless | 850/1700/3400 | 45-55/90-110/180-220 | 1.3 | 2 | 5 | 0 | 60 | 300 |
| T1 | Venomancer | Scaled, Summoner, Poisoner | 750/1500/3000 | 35-45/70-90/140-180 | 1.2 | 3 | 0 | 20 | 80 | 275 |
| T2 | Bristleback | Brawny, Savage | 1350/2700/5400 | 100-130/200-260/400-520 | 1.7 | 1 | 5 | 0 | 20 | 300 |
| T2 | Chaos Knight | Demon, Knight | 1125/2250/4500 | 55-130/110-260/220-520 | 1.3 | 1 | 5 | 0 | 75 | 320 |
| T2 | Earth Spirit | Spirit, Warrior | 1275/2550/5100 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 |
| T2 | Juggernaut | Brawny, Swordsman | 900/1800/3600 | 60-75/120-150/240-300 | 1/0.9/0.8 | 1 | 5 | 0 | 100 | 300 |
| T2 | Kunkka | Human, Warrior, Swordsman | 1200/2400/4800 | 65-85/130-170/260-340 | 1.4 | 1 | 5 | 0 | 100 | 300 |
| T2 | Legion Commander | Human, Champion | 600/1200/2400 | 50-60/100-120/200-240 | 1.25 | 1 | 5 | 0 | 100 | 315 |
| T2 | Luna | Vigilant, Knight | 750/1500/3000 | 60-70/120-140/240-280 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Meepo | Rogue, Summoner | 1100/2200/4400 | 50-60/100-120/200-240 | 1.1 | 1 | 5 | 0 | 75 | 310 |
| T2 | Nature's Prophet | Shaman, Summoner | 900/1800/3600 | 55-65/110-130/220-260 | 1.3 | 3 | 0 | 0 | 80 | 290 |
| T2 | Pudge | Heartless, Warrior | 1650/3300/6600 | 45-55/90-110/180-220 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin, Poisoner | 825/1650/3300 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 60 | 290 |
| T2 | Spirit Breaker | Savage, Brute | 1200/2400/4800 | 55-70/110-140/220-280 | 1.5 | 1 | 5 | 0 | 100 | 330 |
| T2 | Storm Spirit | Spirit, Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 |
| T2 | Windranger | Vigilant, Hunter | 825/1650/3300 | 50-60/100-120/200-240 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T3 | Abaddon | Fallen, Knight | 1125/2250/5400 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 |
| T3 | Alchemist | Brute, Rogue, Poisoner | 1300/2600/6240 | 40-60/80-120/192-288 | 1 | 1 | 5 | 0 | 100 | 305 |
| T3 | Beastmaster | Brawny, Hunter, Shaman | 1000/2000/4800 | 70-80/140-160/336-384 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T3 | Ember Spirit | Spirit, Assassin, Swordsman | 825/1650/3960 | 80-100/160-200/384-480 | 1.5 | 1 | 5 | 0 | 100 | 305 |
| T3 | Lifestealer | Heartless, Brute | 1000/2000/4800 | 55-60/110-120/264-288 | 1.5/1.4/1.3 | 1 | 5 | 0 | 75 | 300 |
| T3 | Lycan | Human, Savage, Summoner | 1125/2250/5400 | 50-60/100-120/240-288 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Omniknight | Human, Knight, Healer | 1100/2200/5280 | 40-50/80-100/192-240 | 1.4 | 1 | 10 | 0 | 60 | 300 |
| T3 | Puck | Dragon, Mage | 850/1700/4080 | 50-60/100-120/240-288 | 1.7 | 3 | 5 | 0 | 60 | 290 |
| T3 | Shadow Shaman | Troll, Summoner | 850/1700/4080 | 50-60/100-120/240-288 | 1.25 | 3 | 5 | 0 | 70 | 285 |
| T3 | Slark | Scaled, Assassin | 800/1600/3840 | 50-60/78-98/138-178 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T3 | Spectre | Void, Demon | 950/1900/4560 | 55-65/110-130/264-312 | 0.9 | 1 | 5 | 0 | 70 | 310 |
| T3 | Terrorblade | Demon, Hunter, Fallen | 975/1950/4680 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 30 | 80 | 310 |
| T3 | Treant Protector | Shaman, Healer | 1125/2250/5400 | 60-90/120-180/288-432 | 2 | 1 | 5 | 0 | 100 | 270 |
| T4 | Death Prophet | Fallen, Heartless | 1250/2500/5000 | 50-60/100-120/200-240 | 1.25 | 3 | 5 | 0 | 100 | 300 |
| T4 | Doom | Demon, Brute | 1575/3150/6300 | 110-120/220-240/440-480 | 1.1 | 1 | 15 | 0 | 100 | 285 |
| T4 | Lina | Human, Mage | 1000/2000/4000 | 65-70/130-140/260-280 | 1.4 | 4 | 5 | 0 | 75 | 290 |
| T4 | Lone Druid | Savage, Shaman, Summoner | 1200/2400/4800 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 |
| T4 | Mirana | Vigilant, Hunter | 1125/2250/4500 | 70-80/140-160/280-320 | 1 | 4 | 5 | 0 | 70 | 310 |
| T4 | Pangolier | Savage, Swordsman | 1400/2800/5600 | 60-70/120-140/240-280 | 1.1 | 1 | 10 | 10 | 70 | 330 |
| T4 | Rubick | Mage, Magus | 1300/2600/5200 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 20 | 100 | 310 |
| T4 | Sven | Rogue, Knight, Swordsman | 1425/2850/5700 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 80 | 300 |
| T4 | Templar Assassin | Vigilant, Void, Assassin | 1125/2250/4500 | 90-110/180-220/360-440 | 1.375 | 3 | 5 | 0 | 100 | 305 |
| T4 | Tidehunter | Scaled, Warrior | 1650/3300/6600 | 60-70/120-140/240-280 | 2 | 1 | 10 | 0 | 100 | 300 |
| T4 | Viper | Dragon, Poisoner | 1100/2200/4400 | 60-80/120-160/240-320 | 1 | 3 | 5 | 20 | 70 | 275 |
| T4 | Void Spirit | Void, Spirit | 1275/2550/5100 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 |
| T5 | Axe | Brawny, Brute | 3000/6000/12000 | 50-60/100-120/200-240 | 1.5 | 1 | 15 | 0 | 50 | 295 |
| T5 | Dragon Knight | Human, Dragon, Knight | 1400/2800/5600 | 55-75/110-150/220-300 | 1.2 | 1 | 10 | 0 | 50 | 295 |
| T5 | Faceless Void | Void, Assassin | 1500/3000/6000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 |
| T5 | Keeper of the Light | Human, Mage | 1100/2200/4400 | 55-60/110-120/220-240 | 1.7 | 4 | 5 | 0 | 100 | 330 |
| T5 | Medusa | Scaled, Hunter | 1275/2550/5100 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 |
| T5 | Troll Warlord | Troll, Warrior | 1500/3000/6000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 |
| T5 | Wraith King | Fallen, Swordsman | 2000/4000/8000 | 90-110/180-220/360-440 | 1.3 | 1 | 10 | 10 | 75 | 310 |

