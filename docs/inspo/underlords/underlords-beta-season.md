# Dota Underlords: beta season (June 2019 to February 2020)

The open beta ran from 2019-06-20 (closed beta 2019-06-13) until Season One on 2020-02-25. Valve called it the Beta Season or Season 0. Rules, rosters and alliances changed often; four data snapshots are shown below. Sources as in `underlords.md`.

## Systems unique to the beta

- **Jail**: The Big Update (2019-10-24) to 2019-12-18. Every 24 hours a set of heroes was barred from the pool. From 2019-11-15: 8-12 heroes per day, up to 3 from one alliance, one Ace per day, and tiers 1-4 balanced to within 1 ban of each other. Removed 2019-12-18 ("Jailbreak"). [N; W: Jail]
- **Hype**: 2019-10-24 to 2019-12-18. Underlords spent no resource but needed Hype above a threshold to cast. Passive Hype per second plus per-condition gains (Anessix +1/s per allied Demon, Hobgen +1/s per burning unit), +10 per kill, -5/s while stunned or hexed, double gain after the enemy Underlord dies. 2019-11-15: Hype instead scaled Underlord damage (up to +50%) and cooldown speed (up to 40% faster). [N: The Big Changelog, 2019-11-15]
- **Underlord talents**: 2019-10-24 to 2019-12-18. One talent picked every 5 rounds from round 10 to 35; round 20 pick was the ultimate (cast once per fight). Anessix and Hobgen had 22 talents each, 11 locked until unlocked by playing. Knockout allowed only the ultimate pick. [N; W: Anessix/Old Abilities, Hobgen/Old Abilities]
- **Global items**: Items that applied a player-wide rule rather than sitting on a hero (Recruiter, Expanded Roster, Friends and Family Discount, A Higher Class of Criminal, Aegis, Silver Lining, Smuggler...). Removed 2019-12-18. [N: Jull-tide Changelog]
- **Alliance items**: Global items tied to one alliance (Tooth and Claw, Coordinated Assault, Soul Sucking Syphon, Unstable Reactor, Font of Creation...). Moved to their own category 2019-07-19, disabled 2019-08-01; some effects were folded into alliances. [W: Items]
- **Ace tier**: 2019-08-30 to 2020-02-25. Tier 5 heroes were "Aces", each tied to one alliance, with an extra Ace effect while that alliance had at least one level active. Renamed Tier 5 and Ace effects removed at Season One. [N: 2019-08-31, 2020-02-25]

## Rule changes inside the beta

- 2019-07-04: streak gold reworked (see `underlords.md`).
- 2019-08-02: head-to-head shared combat; free reroll after a PvP loss; round damage removed for rounds 4-9 (starts round 11 at 2).
- 2019-08-23: XP granted before the shop refresh; blacklist limited to manual rerolls; T4 pool raised to 20; T4/T5 odds raised at high levels.
- 2019-08-30: Aces and contraptions; Assassin, Elusive, Warrior lose their 9-unit level.
- 2019-10-24: XP bought in 5s with a new curve; Underlords with Hype and talents; Duos and Freestyle; Jail; unit HP raised (the 2019-12-18 note says HP had been doubled from launch values and was then cut to 1.5x).
- 2019-11-15: Underlord rank on a fixed schedule; Hype reworked; round damage back at 1 per 10 rounds.
- 2019-11-26: Knockout.
- 2019-12-18: start at level 3 with 5 gold; interest cap 3; creep rounds and global items removed; items every 5 rounds; Underlord picked at round 10; HP cut 25%; player damage = 1 + power/3 per unit.

## Snapshot: Open beta launch (2019-06-20)

Source: SteamTracking/GameTracking-Underlords commit be6ef27 (scripts/units.json, synergies.json, items.json, abilities.json)

### Alliances

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6/9 | 9 | Bloodseeker, Bounty Hunter, Morphling, Phantom Assassin, Queen of Pain, Sand King, Slark, Templar Assassin, Viper | (3) All Assassins gain a 10% chance to Critical Hit for 300% Damage / (6) All Assassins gain a 20% chance to Critical Hit for 400% Damage / (9) All Assassins gain a 30% chance to Critical Hit for 500% Damage |
| Blood-Bound | 2 | 2 | Ogre Magi, Warlock | (2) When a Blood-Bound unit dies, all other Blood-Bound units deal +100% Attack Damage for the rest of the battle |
| Brawny (beta version) | 2/4 | 4 | Axe, Beastmaster, Disruptor, Juggernaut | (2) All Brawny units have their maximum HP increased by 200 / (4) All Brawny units have their maximum HP increased by 500 |
| Deadeye | 2 | 2 | Gyrocopter, Sniper | (2) All Deadeye units focus their attacks on the lowest-health enemy. |
| Demon | 1 | 5 | Chaos Knight, Doom, Queen of Pain, Shadow Fiend, Terrorblade | (1) All Demon units gain +50% Pure Damage. Active when you have only one type of Demon unit on the board |
| Demon Hunter | 1/2 | 2 | Anti-Mage, Terrorblade | (1) Invalidate your opponent's Demon Alliance bonus / (2) Invalidate your opponent's Demon Alliance bonus. All Demon units gain +50% Pure Damage. |
| Dragon | 3 | 3 | Dragon Knight, Puck, Viper | (3) All Dragon units start Combat with Full Mana |
| Druid | 2/4 | 4 | Enchantress, Lone Druid, Nature's Prophet, Treant Protector | (2) The lowest star ally Druid is upgraded a level. / (4) The 2 lowest star ally Druids are upgraded a level each. |
| Elusive | 3/6/9 | 9 | Anti-Mage, Luna, Mirana, Nature's Prophet, Phantom Assassin, Puck, Templar Assassin, Treant Protector, Windranger | (3) All Elusive units gain +20% Evasion / (6) All Elusive units gain +45% Evasion / (9) All Elusive units gain +75% Evasion |
| Heartless | 2/4/6 | 5 | Abaddon, Drow Ranger, Lich, Necrophos, Pudge | (2) All Enemies suffer -5 Armor / (4) All Enemies suffer -10 Armor / (6) All Enemies suffer -20 Armor |
| Human | 2/4/6 | 8 | Bloodseeker, Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Lina, Lycan, Omniknight | (2) All Human units gain a 20% Chance to Silence target for 4 seconds when attacking / (4) All Human units gain a 44% Chance to Silence target for 4 seconds when attacking / (6) All Human units gain a 66% Chance to Silence target for 4 seconds when attacking |
| Hunter | 3/6 | 7 | Beastmaster, Drow Ranger, Medusa, Mirana, Sniper, Tidehunter, Windranger | (3) All Hunters have a 20% chance of quickly performing 2 attacks. / (6) All Hunters have a 35% chance of quickly performing 2 attacks. |
| Inventor (beta version) | 2/4 | 5 | Clockwerk, Gyrocopter, Techies, Timbersaw, Tinker | (2) All Inventors gain +15 HP Regeneration / (4) All Inventors gain +40 HP Regeneration |
| Knight | 2/4/6 | 6 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight | (2) Knight units take 25% less physical and magic damage when standing 1 cell away from another Knight. / (4) Knight units take 35% less physical and magic damage when standing 1 cell away from another Knight. / (6) Knight units take 50% less physical and magic damage when standing 1 cell away from another Knight. |
| Mage | 3/6 | 7 | Crystal Maiden, Keeper of the Light, Lich, Lina, Ogre Magi, Puck, Razor | (3) Enemies suffer -40% Magic Resistance / (6) Enemies suffer -100% Magic Resistance |
| Primordial (beta version) | 2/4 | 5 | Arc Warden, Enigma, Morphling, Razor, Tiny | (2) When hit, Primordial units have a 30% chance to disarm melee attackers for 4 seconds. / (4) When hit, Allies have a 30% chance to disarm melee attackers for 4 seconds. |
| Savage (beta version) | 2/4/6 | 6 | Enchantress, Lone Druid, Lycan, Sand King, Tusk, Venomancer | (2) Allies gain +10% Attack Damage / (4) Allies gain +25% Attack Damage / (6) Allies gain +45% Attack Damage |
| Scaled | 2/4 | 4 | Medusa, Slardar, Slark, Tidehunter | (2) Allies gain +30% Magic Resistance / (4) Allies gain +50% Magic Resistance |
| Scrappy | 3/6 | 6 | Alchemist, Bounty Hunter, Clockwerk, Techies, Timbersaw, Tinker | (3) A Random Ally is granted +15 Armor and +10 HP Regeneration / (6) Allies gain +15 Armor and +10 HP Regeneration |
| Shaman (old, beta) | 2 | 3 | Arc Warden, Disruptor, Shadow Shaman | (2) A Random Enemy is turned into a Frog for 6 seconds at the start of Combat |
| Troll | 2/4 | 4 | Batrider, Shadow Shaman, Troll Warlord, Witch Doctor | (2) All Troll units gain +35 Attack Speed / (4) All Troll units gain +65 Attack Speed and other Allies gain +30 Attack Speed |
| Warlock | 2/4/6 | 7 | Alchemist, Enigma, Necrophos, Shadow Fiend, Venomancer, Warlock, Witch Doctor | (2) Allies gain +15% Lifesteal / (4) Allies gain +30% Lifesteal / (6) Allies gain +50% Lifesteal |
| Warrior | 3/6/9 | 10 | Axe, Doom, Juggernaut, Kunkka, Lycan, Pudge, Slardar, Tiny, Troll Warlord, Tusk | (3) All Warriors gain +10 Armor / (6) All Warriors gain +15 Armor / (9) All Warriors gain +25 Armor |

### Heroes in pool (60)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Elusive, Demon Hunter | 500/1000/2000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 10/20/30 | 100 | 310 |
| T1 | Axe | Brawny (beta version), Warrior | 700/1400/2800 | 50-55/100-110/200-220 | 1.5 | 1 | 5 | 0 | 50 | 295 |
| T1 | Batrider | Troll, Knight | 500/1000/2000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bloodseeker | Human, Assassin | 500/1000/2000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 |
| T1 | Bounty Hunter | Scrappy, Assassin | 550/1100/2200 | 45-60/90-120/180-240 | 1 | 1 | 5 | 0 | 100 | 315 |
| T1 | Clockwerk | Scrappy, Inventor (beta version) | 700/1400/2800 | 45-55/90-110/180-220 | 1.4 | 1 | 5/7/9 | 0 | 100 | 310 |
| T1 | Drow Ranger | Heartless, Hunter | 400/800/1600 | 45-50/90-100/180-200 | 1.3 | 4 | 5 | 0 | 100 | 285 |
| T1 | Enchantress | Savage (beta version), Druid | 400/800/1500 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 |
| T1 | Ogre Magi | Blood-Bound, Mage | 800/1600/3000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 100 | 290 |
| T1 | Shadow Shaman | Troll, Shaman (old, beta) | 550/1100/2200 | 40-50/80-100/160-200 | 1.6 | 3 | 5 | 0 | 100 | 285 |
| T1 | Tinker | Scrappy, Inventor (beta version) | 500/1000/2000 | 40-50/80-100/160-200 | 1.5 | 4 | 10 | 0 | 100 | 290 |
| T1 | Tiny | Primordial (beta version), Warrior | 650/1300/2600 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 |
| T1 | Tusk | Savage (beta version), Warrior | 650/1300/2600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 100 | 300 |
| T1 | Warlock | Blood-Bound, Warlock | 650/1450/2850 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 |
| T2 | Beastmaster | Brawny (beta version), Hunter | 600/1200/2400 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T2 | Chaos Knight | Demon, Knight | 750/1500/3000 | 40-110/80-220/160-440 | 1.3 | 1 | 5 | 0 | 100 | 320 |
| T2 | Crystal Maiden | Human, Mage | 450/900/1800 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 |
| T2 | Juggernaut | Brawny (beta version), Warrior | 600/1200/2400 | 65-70/130-140/260-280 | 1.1 | 1 | 5 | 0 | 100 | 300 |
| T2 | Luna | Elusive, Knight | 500/1000/2000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Morphling | Primordial (beta version), Assassin | 500/1000/2000 | 45-60/90-120/180-240 | 1.3 | 3 | 5 | 0 | 100 | 280 |
| T2 | Nature's Prophet | Elusive, Druid | 500/1000/2000 | 45-50/90-100/180-200 | 1.4 | 3 | 0 | 0 | 50 | 290 |
| T2 | Puck | Elusive, Dragon, Mage | 450/900/1800 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 100 | 290 |
| T2 | Pudge | Heartless, Warrior | 1000/2500/5500 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin | 550/1100/2200 | 55-60/110-120/220-240 | 1.2 | 3 | 0 | 0 | 100 | 290 |
| T2 | Slardar | Scaled, Warrior | 650/1300/2600 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T2 | Timbersaw | Scrappy, Inventor (beta version) | 800/1600/3200 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 |
| T2 | Treant Protector | Elusive, Druid | 650/1300/2300 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 |
| T2 | Witch Doctor | Troll, Warlock | 550/1100/2200 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 |
| T3 | Abaddon | Heartless, Knight | 750/1500/3000 | 45-55/90-110/180-220 | 1.3 | 1 | 10 | 0 | 100 | 325 |
| T3 | Arc Warden | Primordial (beta version), Shaman (old, beta) | 600/1100/2200 | 75-85/150-170/250-290 | 1.7 | 4 | 5 | 5 | 100 | 280 |
| T3 | Lina | Human, Mage | 550/1100/2200 | 50-55/100-110/200-220 | 1.6 | 4 | 5 | 0 | 100 | 290 |
| T3 | Lycan | Human, Savage (beta version), Warrior | 750/1500/3000 | 50-60/100-120/200-240 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Omniknight | Human, Knight | 700/1400/2800 | 50-60/100-120/200-240 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T3 | Phantom Assassin | Elusive, Assassin | 550/1100/2200 | 70-90/140-180/280-360 | 1.2 | 1 | 5 | 0 | 100 | 305 |
| T3 | Razor | Primordial (beta version), Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.1 | 3 | 5 | 0 | 100 | 285 |
| T3 | Sand King | Savage (beta version), Assassin | 550/1100/2200 | 45-60/90-120/180-240 | 1.4 | 1 | 10 | 0 | 100 | 290 |
| T3 | Shadow Fiend | Demon, Warlock | 450/900/1800 | 70-80/140-160/280-320 | 1.1 | 4 | 5 | 0 | 100 | 305 |
| T3 | Slark | Scaled, Assassin | 500/1000/2000 | 50-60/72-92/105-145 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T3 | Sniper | Deadeye, Hunter | 450/900/1800 | 125-150/180-210/330-360 | 1.6 | 7 | 5 | 0 | 100 | 285 |
| T3 | Terrorblade | Demon, Demon Hunter | 650/1300/2600 | 45-65/90-130/180-260 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 |
| T3 | Venomancer | Savage (beta version), Warlock | 500/1000/2000 | 50-60/100-120/200-240 | 1.2 | 3 | 0 | 20 | 100 | 275 |
| T3 | Viper | Dragon, Assassin | 550/1100/2200 | 50-60/100-120/150-240 | 1 | 3 | 5 | 20 | 100 | 275 |
| T3 | Windranger | Elusive, Hunter | 550/1100/2200 | 60-65/120-130/240-260 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T4 | Alchemist | Scrappy, Warlock | 1000/2000/4000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 |
| T4 | Disruptor | Brawny (beta version), Shaman (old, beta) | 700/1400/2800 | 45-50/90-100/180-200 | 1.5 | 4 | 5 | 0 | 100 | 295 |
| T4 | Doom | Demon, Warrior | 1050/2100/4200 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 |
| T4 | Dragon Knight | Human, Dragon, Knight | 800/1600/3200 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 |
| T4 | Keeper of the Light | Human, Mage | 650/1300/2600 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 |
| T4 | Kunkka | Human, Warrior | 950/1900/3800 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T4 | Lone Druid | Savage (beta version), Druid | 800/1600/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 100 | 325 |
| T4 | Medusa | Scaled, Hunter | 850/1700/3300 | 50-60/100-120/200-240 | 1 | 4 | 5 | 10 | 100 | 275 |
| T4 | Mirana | Elusive, Hunter | 750/1500/3000 | 55-65/110-130/220-260 | 1 | 4 | 5 | 0 | 100 | 310 |
| T4 | Necrophos | Heartless, Warlock | 850/1700/3400 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 |
| T4 | Templar Assassin | Elusive, Assassin | 750/1500/3000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 |
| T4 | Troll Warlord | Troll, Warrior | 900/1800/3600 | 90-100/180-200/360-400 | 1.1 | 3 | 5 | 0 | 100 | 290 |
| T5 | Enigma | Primordial (beta version), Warlock | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 |
| T5 | Gyrocopter | Deadeye, Inventor (beta version) | 900/1800/3600 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 |
| T5 | Lich | Heartless, Mage | 850/1700/3400 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T5 | Techies | Scrappy, Inventor (beta version) | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 |
| T5 | Tidehunter | Scaled, Hunter | 950/1900/3800 | 45-55/90-110/180-220 | 2 | 1 | 5 | 0 | 100 | 300 |

## Snapshot: Late beta, before The Big Update (2019-10-10)

Source: GameTracking-Underlords commit 9923524

### Alliances

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6 | 8 | Bloodseeker, Bounty Hunter, Morphling, Phantom Assassin, Queen of Pain, Sand King, Slark, Templar Assassin | (3) All Assassins gain a 15% chance to Critical Hit for 300% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 50% for 3 seconds. / (6) All Assassins gain a 25% chance to Critical Hit for 400% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 100% for 3 seconds. |
| Blood-Bound | 2 | 2 | Ogre Magi, Warlock | (2) When a Blood-Bound unit dies, it gives +125% Attack Damage, in addition to any existing Blood-Bound bonus it has, to a random friendly Blood-Bound unit. |
| Brawny | 2/4 | 4 | Axe, Beastmaster, Disruptor, Juggernaut | (2) All Brawny units gain +30 maximum HP for each kill they have. / (4) All Brawny units gain +60 maximum HP for each kill they have. |
| Deadeye | 2 | 3 | Bloodseeker, Gyrocopter, Sniper | (2) All Deadeye units focus their attacks on the lowest-health enemy. |
| Demon | 1 | 5 | Chaos Knight, Doom, Queen of Pain, Shadow Fiend, Terrorblade | (1) All Demon units gain +50% Pure Damage. Active when you have only one type of Demon unit on the board |
| Demon Hunter | 1/2 | 2 | Anti-Mage, Terrorblade | (1) Invalidate your opponent's Demon Alliance bonus. / (2) Invalidate your opponent's Demon Alliance bonus. All Demon and all Demon Hunter units gain +50% Pure Damage. |
| Dragon | 2 | 3 | Dragon Knight, Puck, Viper | (2) All Dragon units unlock an additional draconic ability. |
| Druid | 2/4 | 4 | Enchantress, Lone Druid, Nature's Prophet, Treant Protector | (2) At the beginning of combat, the lowest star ally Druid is upgraded a level. / (4) At the beginning of combat, the 2 lowest star ally Druids are upgraded a level each. |
| Elusive | 3/6 | 9 | Anti-Mage, Luna, Mirana, Nature's Prophet, Phantom Assassin, Puck, Templar Assassin, Treant Protector, Windranger | (3) All Elusive units gain +25% Evasion. / (6) All Elusive units gain +50% Evasion. |
| Heartless | 2/4/6 | 5 | Abaddon, Drow Ranger, Lich, Necrophos, Pudge | (2) All Enemies lose 5 Armor / (4) All Enemies lose 10 Armor / (6) All Enemies lose 15 Armor |
| Human | 2/4/6 | 8 | Bloodseeker, Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Lina, Lycan, Omniknight | (2) All Human units gain a 20% Chance to Silence target for 4 seconds when attacking / (4) All Human units gain a 44% Chance to Silence target for 4 seconds when attacking / (6) All Human units gain a 66% Chance to Silence target for 4 seconds when attacking |
| Hunter | 3/6 | 7 | Beastmaster, Drow Ranger, Lycan, Medusa, Mirana, Sniper, Windranger | (3) All Hunters have a 25% chance of quickly performing 2 attacks. / (6) All Hunters have a 40% chance of quickly performing 2 attacks. |
| Inventor | 2/4 | 5 | Clockwerk, Gyrocopter, Techies, Timbersaw, Tinker | (2) Allied Inventors explode on death dealing 15% of their Max Health to all enemies 1 cell away. / (4) Allied Inventors explode on death dealing 30% of their Max Health to all enemies 1 cell away. |
| Knight | 2/4/6 | 6 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight | (2) Knight units take 15% less physical and magic damage and an additional 15% less when standing 1 cell away from another Knight. / (4) Knight units take 20% less physical and magic damage and an additional 20% less when standing 1 cell away from another Knight. / (6) Knight units take 25% less physical and magic damage and an additional 25% less when standing 1 cell away from another Knight. |
| Mage | 3/6 | 7 | Crystal Maiden, Keeper of the Light, Lich, Lina, Ogre Magi, Puck, Razor | (3) Enemies take 40% more Magic damage / (6) Enemies take 100% more Magic damage |
| Primordial | 2/4 | 5 | Arc Warden, Enigma, Morphling, Razor, Tiny | (2) Allied Primordial units spawn a rank 1 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / (4) Allied Primordial units spawn a rank 2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. |
| Savage | 2/4/6 | 6 | Enchantress, Lone Druid, Lycan, Sand King, Tusk, Venomancer | (2) Allied Savage units gain +4 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (4) Allied Savage units gain +8 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (6) Allied Savage units gain +15 Attack Damage each time they land an attack on an enemy or cast a damaging spell. |
| Scaled | 2/4 | 5 | Medusa, Slardar, Slark, Tidehunter, Viper | (2) Allies gain +30% Magic Resistance / (4) Allies gain +50% Magic Resistance |
| Scrappy | 2/4/6 | 7 | Alchemist, Bounty Hunter, Clockwerk, Sniper, Techies, Timbersaw, Tinker | (2) A Random Scrappy is granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / (4) All Scrappy units gain +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does / (6) Allies gain +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does |
| Shaman (old, beta) | 2 | 3 | Arc Warden, Enigma, Shadow Shaman | (2) Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 4 cells in a random direction. |
| Troll | 2/4 | 4 | Batrider, Shadow Shaman, Troll Warlord, Witch Doctor | (2) All Troll units gain +35 Attack Speed and other Allies gain +10 Attack Speed / (4) All Troll units gain +65 Attack Speed and other Allies gain +30 Attack Speed |
| Warlock | 2/4/6 | 7 | Alchemist, Disruptor, Necrophos, Shadow Fiend, Venomancer, Warlock, Witch Doctor | (2) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 50% of the damage dealt. / (4) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 70% of the damage dealt. / (6) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 100% of the damage dealt. |
| Warrior | 3/6 | 10 | Axe, Doom, Juggernaut, Kunkka, Pudge, Slardar, Tidehunter, Tiny, Troll Warlord, Tusk | (3) All Warriors gain +10 Armor / (6) All Warriors gain +20 Armor |

### Heroes in pool (60)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Elusive, Demon Hunter | 500/1000/2000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 100 | 310 |
| T1 | Axe | Brawny, Warrior | 805/1610/3220 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 |
| T1 | Batrider | Troll, Knight | 500/1000/2000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bloodseeker | Human, Deadeye, Assassin | 500/1000/2000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 |
| T1 | Bounty Hunter | Scrappy, Assassin | 550/1100/2200 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 |
| T1 | Drow Ranger | Heartless, Hunter | 400/800/1600 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 |
| T1 | Enchantress | Savage, Druid | 400/800/1500 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 |
| T1 | Ogre Magi | Blood-Bound, Mage | 800/1600/3000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 |
| T1 | Razor | Primordial, Mage | 500/1000/2000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 |
| T1 | Shadow Shaman | Troll, Shaman (old, beta) | 550/1100/2200 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 100 | 285 |
| T1 | Tiny | Primordial, Warrior | 650/1300/2600 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 |
| T1 | Tusk | Savage, Warrior | 700/1400/2800 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 |
| T1 | Venomancer | Savage, Warlock | 500/1000/2000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 |
| T1 | Warlock | Blood-Bound, Warlock | 650/1450/2850 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 |
| T2 | Beastmaster | Brawny, Hunter | 600/1200/2400 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T2 | Chaos Knight | Demon, Knight | 750/1500/3000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 100 | 320 |
| T2 | Lina | Human, Mage | 550/1100/2200 | 50-55/100-110/200-220 | 1.6 | 4 | 5 | 0 | 100 | 290 |
| T2 | Luna | Elusive, Knight | 500/1000/2000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Morphling | Primordial, Assassin | 500/1000/2000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 |
| T2 | Nature's Prophet | Elusive, Druid | 500/1000/2000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 |
| T2 | Puck | Elusive, Dragon, Mage | 450/900/1800 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 |
| T2 | Pudge | Heartless, Warrior | 1000/2500/5500 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin | 550/1100/2200 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 |
| T2 | Slardar | Scaled, Warrior | 650/1300/2600 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T2 | Timbersaw | Scrappy, Inventor | 800/1600/3200 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 |
| T2 | Windranger | Elusive, Hunter | 550/1100/2300 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T2 | Witch Doctor | Troll, Warlock | 550/1100/2200 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 |
| T3 | Abaddon | Heartless, Knight | 750/1500/3000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 |
| T3 | Clockwerk | Scrappy, Inventor | 950/1800/3500 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 |
| T3 | Crystal Maiden | Human, Mage | 550/1100/2000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 |
| T3 | Juggernaut | Brawny, Warrior | 600/1200/2400 | 65-70/140-150/280-300 | 1.1/1.0/0.8 | 1 | 5 | 0 | 100 | 300 |
| T3 | Lycan | Human, Savage, Hunter | 750/1500/3600 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Omniknight | Human, Knight | 700/1400/3360 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T3 | Phantom Assassin | Elusive, Assassin | 550/1100/2200 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 |
| T3 | Shadow Fiend | Demon, Warlock | 450/900/1800 | 70-80/140-160/336-384 | 1.1 | 3 | 5 | 0 | 100 | 305 |
| T3 | Slark | Scaled, Assassin | 500/1000/2400 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T3 | Sniper | Scrappy, Deadeye, Hunter | 450/900/2160 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 |
| T3 | Terrorblade | Demon, Demon Hunter | 650/1300/3120 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 |
| T3 | Tinker | Scrappy, Inventor | 750/1500/3500 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 |
| T3 | Treant Protector | Elusive, Druid | 750/1500/2500 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 |
| T3 | Viper | Scaled, Dragon | 550/1100/3000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 |
| T4 | Alchemist | Scrappy, Warlock | 1000/2000/4000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 |
| T4 | Arc Warden | Primordial, Shaman (old, beta) | 600/1200/2400 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 |
| T4 | Doom | Demon, Warrior | 1050/2100/4200 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 |
| T4 | Dragon Knight | Human, Dragon, Knight | 1000/2000/4000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 |
| T4 | Keeper of the Light | Human, Mage | 550/1100/2200 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 150 | 330 |
| T4 | Kunkka | Human, Warrior | 950/1900/3800 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T4 | Lone Druid | Savage, Druid | 800/1600/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 |
| T4 | Mirana | Elusive, Hunter | 750/1500/3000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 |
| T4 | Necrophos | Heartless, Warlock | 850/1700/3400 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 |
| T4 | Sand King | Savage, Assassin | 750/1500/3200 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 |
| T4 | Templar Assassin | Elusive, Assassin | 750/1500/3000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 |
| T4 | Tidehunter | Scaled, Warrior | 1100/2700/5500 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 |
| T5 | Disruptor | Brawny, Warlock | 900/1600/3400 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 |
| T5 | Enigma | Primordial, Shaman (old, beta) | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 |
| T5 | Gyrocopter | Deadeye, Inventor | 900/1800/3600 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 |
| T5 | Lich | Heartless, Mage | 850/1700/3400 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T5 | Medusa | Scaled, Hunter | 850/1700/3300 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 |
| T5 | Techies | Scrappy, Inventor | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 |
| T5 | Troll Warlord | Troll, Warrior | 1200/2000/4000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 |

## Snapshot: The Big Update (Underlords added) (2019-10-24)

Source: GameTracking-Underlords commit cc2226d

### Alliances

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6 | 9 | Anti-Mage, Bounty Hunter, Faceless Void, Morphling, Nyx Assassin, Phantom Assassin, Queen of Pain, Slark, Templar Assassin | (3) All Assassins gain a 15% chance to Critical Hit for 300% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 50% for 3 seconds. / (6) All Assassins gain a 25% chance to Critical Hit for 400% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 100% for 3 seconds. |
| Blood-Bound | 2 | 3 | Bloodseeker, Ogre Magi, Warlock | (2) When a Blood-Bound unit dies, it gives +125% Attack Damage, in addition to any existing Blood-Bound bonus it has, to a random friendly Blood-Bound unit. |
| Brawny | 2/4 | 5 | Axe, Beastmaster, Bristleback, Disruptor, Juggernaut | (2) All Brawny units gain +15 maximum HP for each kill by any of your team Brawny units throughout the game. / (4) All Brawny units gain +30 maximum HP for each kill by any of your team Brawny units throughout the game. |
| Brute | 2/4 | 5 | Axe, Doom, Lifestealer, Magnus, Treant Protector | (2) Whenever a Brute attacks an enemy it applies a -15% damage debuff for 3 seconds. When the debuff is initially applied it deals 50 damage. Brutes prefer to attack enemies that do not have the debuff. / (4) Whenever a Brute attacks an enemy it applies a -40% damage debuff for 3 seconds. When the debuff is initially applied it deals 100 damage. Brutes prefer to attack enemies that do not have the debuff. |
| Champion | 1 | 1 | Legion Commander | (1) Champions receive all Alliance bonuses. |
| Deadeye | 2 | 3 | Bloodseeker, Gyrocopter, Sniper | (2) All Deadeye units focus their attacks on the lowest-health enemy. Melee Deadeye units gain phasing movement. |
| Demon | 1 | 6 | Chaos Knight, Doom, Queen of Pain, Shadow Demon, Shadow Fiend, Terrorblade | (1) Whenever a Demon casts a spell, for 4 seconds all Demons are silenced and the caster gains +50% bonus Pure damage for each allied Demon. |
| Dragon | 2 | 3 | Dragon Knight, Puck, Viper | (2) All Dragon units unlock an additional draconic ability. |
| Druid | 2/4 | 5 | Enchantress, Io, Lone Druid, Nature's Prophet, Treant Protector | (2) At the beginning of combat, 1 lowest star ally Druids are upgraded a level. / (4) At the beginning of combat, 2 lowest star ally Druids are upgraded a level. |
| Elusive | 3/6 | 7 | Anti-Mage, Luna, Mirana, Phantom Assassin, Puck, Templar Assassin, Windranger | (3) All Elusive units have a 10% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked. / (6) All Elusive units have a 20% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked. |
| Healer | 3 | 5 | Dazzle, Enchantress, Necrophos, Omniknight, Warlock | (3) All friendly healing is amplified by 30%. |
| Heartless | 2/4/6 | 7 | Abaddon, Drow Ranger, Lich, Lifestealer, Necrophos, Pudge, Shadow Demon | (2) All Enemies lose 5 Armor / (4) All Enemies lose 10 Armor / (6) All Enemies lose 15 Armor |
| Human | 2/4/6 | 9 | Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Legion Commander, Lina, Lycan, Omniknight, Sven | (2) Allies generate 4 mana per second. / (4) Allies generate 7 mana per second. / (6) Allies generate 10 mana per second. |
| Hunter | 3/6 | 9 | Beastmaster, Drow Ranger, Lycan, Medusa, Mirana, Sniper, Terrorblade, Weaver, Windranger | (3) All Hunters have a 25% chance of quickly performing 2 attacks. / (6) All Hunters have a 40% chance of quickly performing 2 attacks. |
| Insect | 3 | 4 | Broodmother, Nyx Assassin, Sand King, Weaver | (3) Spiderlings constantly swarm enemies from the edge of the board. Spiderlings are untargetable units that deal low damage and have 1 health. Enemies bitten by spiderlings have a 30% chance to miss their attacks for 0.5 seconds. Spiderlings are replenished upon death up to a maximum of 12. |
| Inventor | 2/4 | 5 | Clockwerk, Gyrocopter, Techies, Timbersaw, Tinker | (2) Allied Inventors explode on death dealing 10% of their Max Health to all enemies 1 cell away. / (4) Allied Inventors explode on death dealing 20% of their Max Health to all enemies 1 cell away. |
| Knight | 2/4/6 | 7 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight, Sven | (2) Knight units take 15% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 15% less physical and magical damage. / (4) Knight units take 20% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 20% less physical and magical damage. / (6) Knight units take 25% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 25% less physical and magical damage. |
| Mage | 3/6 | 7 | Crystal Maiden, Keeper of the Light, Lich, Lina, Ogre Magi, Puck, Razor | (3) Enemies take 40% more Magic damage / (6) Enemies take 100% more Magic damage |
| Primordial | 2/4/6 | 7 | Arc Warden, Enigma, Faceless Void, Io, Morphling, Razor, Tiny | (2) Allied Primordial units spawn a rank 1 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / (4) Allied Primordial units spawn a rank 2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / (6) Allied Primordial units spawn a rank 3 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. |
| Savage | 2/4/6 | 7 | Bristleback, Enchantress, Lone Druid, Lycan, Sand King, Tusk, Venomancer | (2) Allied Savage units gain +4 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (4) Allied Savage units gain +8 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (6) Allied Savage units gain +15 Attack Damage each time they land an attack on an enemy or cast a damaging spell. |
| Scaled | 2/4 | 6 | Medusa, Slardar, Slark, Sven, Tidehunter, Viper | (2) Allies gain +30% Magic Resistance / (4) Allies gain +50% Magic Resistance |
| Scrappy | 2/4/6 | 7 | Alchemist, Bounty Hunter, Clockwerk, Sniper, Techies, Timbersaw, Tinker | (2) A Random Scrappy are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / (4) All Scrappy Units are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / (6) All Allies are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. |
| Shaman (old, beta) | 2/4 | 5 | Arc Warden, Enigma, Magnus, Nature's Prophet, Shadow Shaman | (2) Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 2 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle. / (4) Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 4 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle. |
| Troll | 2/4 | 5 | Batrider, Dazzle, Shadow Shaman, Troll Warlord, Witch Doctor | (2) All Troll units gain +35 Attack Speed and other Allies gain +10 Attack Speed / (4) All Troll units gain +65 Attack Speed and other Allies gain +30 Attack Speed |
| Warlock | 2/4/6 | 7 | Alchemist, Broodmother, Disruptor, Shadow Fiend, Venomancer, Warlock, Witch Doctor | (2) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 50% of the damage dealt. / (4) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 70% of the damage dealt. / (6) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 100% of the damage dealt. |
| Warrior | 3/6 | 8 | Juggernaut, Kunkka, Pudge, Slardar, Tidehunter, Tiny, Troll Warlord, Tusk | (3) All Warriors gain +10 Armor / (6) All Warriors gain +20 Armor |

### Heroes in pool (72)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Elusive, Assassin | 1000/2000/4000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 |
| T1 | Axe | Brawny, Brute | 1610/3220/6440 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 |
| T1 | Batrider | Troll, Knight | 1000/2000/4000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bloodseeker | Blood-Bound, Deadeye | 1000/2000/4000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 |
| T1 | Bounty Hunter | Scrappy, Assassin | 1100/2200/4400 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 |
| T1 | Drow Ranger | Heartless, Hunter | 800/1600/3200 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 |
| T1 | Enchantress | Savage, Druid, Healer | 800/1600/3000 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 |
| T1 | Nyx Assassin | Insect, Assassin | 1100/2200/4400 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 |
| T1 | Ogre Magi | Blood-Bound, Mage | 1600/3200/6000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 |
| T1 | Razor | Primordial, Mage | 1000/2000/4000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 |
| T1 | Shadow Demon | Heartless, Demon | 1200/2400/4800 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 |
| T1 | Shadow Shaman | Troll, Shaman (old, beta) | 1100/2200/4400 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 70 | 285 |
| T1 | Tiny | Primordial, Warrior | 1300/2600/5200 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 |
| T1 | Tusk | Savage, Warrior | 1400/2800/5600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 |
| T1 | Venomancer | Savage, Warlock | 1000/2000/4000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 |
| T1 | Warlock | Blood-Bound, Warlock, Healer | 1300/2900/5700 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 |
| T2 | Beastmaster | Brawny, Hunter | 1200/2400/4800 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T2 | Chaos Knight | Demon, Knight | 1500/3000/6000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 100 | 320 |
| T2 | Crystal Maiden | Human, Mage | 1000/2000/4000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 |
| T2 | Dazzle | Troll, Healer | 1500/3000/6000 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 |
| T2 | Luna | Elusive, Knight | 1000/2000/4000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Magnus | Shaman (old, beta), Brute | 1500/3000/7000 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 0 | 280 |
| T2 | Morphling | Primordial, Assassin | 1000/2000/4000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 |
| T2 | Nature's Prophet | Druid, Shaman (old, beta) | 1000/2000/4000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 |
| T2 | Puck | Elusive, Dragon, Mage | 900/1800/3600 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 |
| T2 | Pudge | Heartless, Warrior | 2000/5000/11000 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin | 1100/2200/4400 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 |
| T2 | Slardar | Scaled, Warrior | 1300/2600/5200 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T2 | Timbersaw | Scrappy, Inventor | 1600/3200/6400 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 |
| T2 | Weaver | Insect, Hunter | 900/1800/3600 | 45-55/90-110/207-253 | 0.9 | 2 | 5 | 0 | 20 | 290 |
| T2 | Windranger | Elusive, Hunter | 1100/2200/4600 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T2 | Witch Doctor | Troll, Warlock | 1100/2200/4400 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 |
| T3 | Abaddon | Heartless, Knight | 1500/3000/6000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 |
| T3 | Clockwerk | Scrappy, Inventor | 1800/3600/7000 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 |
| T3 | Io | Primordial, Druid | 1100/2200/4000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 |
| T3 | Juggernaut | Brawny, Warrior | 1200/2400/4800 | 65-70/140-150/280-300 | 1.1/1.0/0.8 | 1 | 5 | 0 | 100 | 300 |
| T3 | Legion Commander | Human, Champion | 1000/2000/4800 | 50-60/100-120/240-288 | 1.5 | 1 | 10 | 0 | 100 | 315 |
| T3 | Lifestealer | Heartless, Brute | 1350/2700/6480 | 55-60/110-120/220-240 | 1.7/1.6/1.5 | 1 | 5 | 0 | 0 | 300 |
| T3 | Lina | Human, Mage | 1200/2400/4800 | 50-55/100-110/200-220 | 1.4 | 4 | 5 | 0 | 100 | 290 |
| T3 | Lycan | Human, Savage, Hunter | 1500/3000/7200 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Omniknight | Human, Knight, Healer | 1400/2800/6720 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T3 | Phantom Assassin | Elusive, Assassin | 1100/2200/4400 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 |
| T3 | Shadow Fiend | Demon, Warlock | 900/1800/3600 | 70-80/140-160/336-384 | 1.1 | 3 | 5 | 0 | 100 | 305 |
| T3 | Slark | Scaled, Assassin | 1000/2000/4800 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T3 | Sniper | Scrappy, Deadeye, Hunter | 900/1800/4320 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 |
| T3 | Terrorblade | Demon, Hunter | 1300/2600/6240 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 |
| T3 | Tinker | Scrappy, Inventor | 1500/3000/7000 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 |
| T3 | Treant Protector | Druid, Brute | 1500/3000/5000 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 |
| T3 | Viper | Scaled, Dragon | 1100/2200/6000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 |
| T4 | Alchemist | Scrappy, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 |
| T4 | Arc Warden | Primordial, Shaman (old, beta) | 1200/2400/4800 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 |
| T4 | Bristleback | Brawny, Savage | 2200/4400/8800 | 130-150/260-300/520-600 | 1.7 | 1 | 10 | 0 | 20 | 300 |
| T4 | Broodmother | Insect, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 275 |
| T4 | Doom | Demon, Brute | 2100/4200/8400 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 |
| T4 | Dragon Knight | Human, Dragon, Knight | 2000/4000/8000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 |
| T4 | Keeper of the Light | Human, Mage | 1100/2200/4400 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 |
| T4 | Kunkka | Human, Warrior | 1900/3800/7600 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T4 | Lone Druid | Savage, Druid | 1600/3200/6000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 |
| T4 | Mirana | Elusive, Hunter | 1500/3000/6000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 |
| T4 | Necrophos | Heartless, Healer | 1700/3400/6800 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 |
| T4 | Sand King | Savage, Insect | 1500/3000/6400 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 |
| T4 | Templar Assassin | Elusive, Assassin | 1500/3000/6000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 |
| T4 | Tidehunter | Scaled, Warrior | 2200/5400/11000 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 |
| T5 | Disruptor | Brawny, Warlock | 1800/3200/6800 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 |
| T5 | Enigma | Primordial, Shaman (old, beta) | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 |
| T5 | Faceless Void | Primordial, Assassin | 2000/4000/8000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 |
| T5 | Gyrocopter | Deadeye, Inventor | 1800/3600/7200 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 |
| T5 | Lich | Heartless, Mage | 1700/3400/6800 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T5 | Medusa | Scaled, Hunter | 1700/3400/6600 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 |
| T5 | Sven | Human, Scaled, Knight | 1900/3800/7600 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 200 | 300 |
| T5 | Techies | Scrappy, Inventor | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 |
| T5 | Troll Warlord | Troll, Warrior | 2400/4000/8000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 |

## Snapshot: After The Outlanders Update and Knockout (2019-12-06)

Source: GameTracking-Underlords commit 98a2dcc (last client-side copy of the data files; they were moved server-side on 2020-02-04)

### Alliances

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6 | 9 | Anti-Mage, Bounty Hunter, Ember Spirit, Faceless Void, Nyx Assassin, Phantom Assassin, Queen of Pain, Slark, Templar Assassin | (3) All Assassins gain a 15% chance to Critical Hit for 300% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 50% for 3 seconds. / (6) All Assassins gain a 25% chance to Critical Hit for 400% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 100% for 3 seconds. |
| Blood-Bound | 2 | 3 | Bloodseeker, Ogre Magi, Warlock | (2) When a Blood-Bound unit dies, it gives +125% Attack Damage, in addition to any existing Blood-Bound bonus it has, to a random friendly Blood-Bound unit. |
| Brawny | 2/4 | 5 | Axe, Beastmaster, Bristleback, Disruptor, Juggernaut | (2) All Brawny units gain +15 maximum HP for each kill by any of your team Brawny units throughout the game. / (4) All Brawny units gain +30 maximum HP for each kill by any of your team Brawny units throughout the game. |
| Brute | 2/4 | 6 | Axe, Doom, Lifestealer, Magnus, Ogre Magi, Treant Protector | (2) Whenever a Brute attacks an enemy it applies a -30% damage debuff for 4 seconds. When the debuff is initially applied it deals 80 damage. Brutes prefer to attack enemies that do not have the debuff. / (4) Whenever a Brute attacks an enemy it applies a -60% damage debuff for 4 seconds. When the debuff is initially applied it deals 150 damage. Brutes prefer to attack enemies that do not have the debuff. |
| Champion | 1 | 1 | Legion Commander | (1) Champions receive all Alliance bonuses. In addition, they gain 8% extra damage and 8% max health per active Alliance. |
| Deadeye | 2 | 3 | Bloodseeker, Gyrocopter, Sniper | (2) All Deadeye units focus their attacks on the lowest-health enemy. Melee Deadeye units gain phasing movement. |
| Demon | 1 | 6 | Chaos Knight, Doom, Queen of Pain, Shadow Demon, Shadow Fiend, Terrorblade | (1) Whenever a Demon casts a spell, for 4 seconds, allied Demons are silenced and the caster gains +50% bonus Pure damage for each allied Demon. |
| Dragon | 2 | 4 | Dragon Knight, Puck, Snapfire, Viper | (2) All Dragon units unlock an additional draconic ability. |
| Druid | 2/4 | 5 | Enchantress, Io, Lone Druid, Nature's Prophet, Treant Protector | (2) At the beginning of combat, 1 lowest star ally Druids are upgraded a level. / (4) At the beginning of combat, 2 lowest star ally Druids are upgraded a level. |
| Elusive | 3/6 | 8 | Anti-Mage, Luna, Mirana, Phantom Assassin, Puck, Templar Assassin, Weaver, Windranger | (3) All Elusive units have a 10% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked. / (6) All Elusive units have a 20% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked. |
| Healer | 3 | 5 | Dazzle, Enchantress, Necrophos, Omniknight, Warlock | (3) All friendly healing is amplified by 30%. |
| Heartless | 2/4/6 | 7 | Abaddon, Drow Ranger, Lich, Lifestealer, Necrophos, Pudge, Shadow Demon | (2) All Enemies lose 3 Armor / (4) All Enemies lose 8 Armor / (6) All Enemies lose 15 Armor |
| Human | 2/4/6 | 9 | Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Legion Commander, Lina, Lycan, Omniknight, Sven | (2) Allies generate 4 mana per second. / (4) Allies generate 7 mana per second. / (6) Allies generate 10 mana per second. |
| Hunter | 3/6 | 8 | Beastmaster, Drow Ranger, Lycan, Medusa, Mirana, Sniper, Terrorblade, Windranger | (3) All Hunters have a 25% chance of quickly performing 2 attacks. / (6) All Hunters have a 40% chance of quickly performing 2 attacks. |
| Insect | 3 | 4 | Broodmother, Nyx Assassin, Sand King, Weaver | (3) Spiderlings constantly swarm enemies from the edge of the board. They are untargetable units that deal low damage and have 1 health. Enemies bitten by them have a 30% chance to miss attacks for 0.5s. Spiderlings replenish on death to a maximum of 12. |
| Inventor | 2/4 | 6 | Clockwerk, Gyrocopter, Snapfire, Techies, Timbersaw, Tinker | (2) Allied Inventors explode on death dealing 10% of their Max Health to all enemies 1 cell away. / (4) Allied Inventors explode on death dealing 20% of their Max Health to all enemies 1 cell away. |
| Knight | 2/4/6 | 7 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight, Sven | (2) Knight units take 15% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 15% less physical and magical damage. / (4) Knight units take 20% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 20% less physical and magical damage. / (6) Knight units take 25% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 25% less physical and magical damage. |
| Mage | 3/6 | 9 | Crystal Maiden, Keeper of the Light, Lich, Lina, Morphling, Ogre Magi, Puck, Razor, Storm Spirit | (3) Enemies take 25% more Magic damage / (6) Enemies take 75% more Magic damage |
| Primordial | 2/4/6 | 7 | Arc Warden, Enigma, Faceless Void, Io, Morphling, Razor, Tiny | (2) Allied Primordial units spawn a rank 1 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / (4) Allied Primordial units spawn a rank 2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / (6) Allied Primordial units spawn a rank 3 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. |
| Savage | 2/4/6 | 7 | Bristleback, Enchantress, Lone Druid, Lycan, Sand King, Tusk, Venomancer | (2) Allied Savage units gain +4 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (4) Allied Savage units gain +7 Attack Damage each time they land an attack on an enemy or cast a damaging spell. / (6) Allied Savage units gain +15 Attack Damage each time they land an attack on an enemy or cast a damaging spell. |
| Scaled | 2/4 | 7 | Medusa, Slardar, Slark, Sven, Tidehunter, Venomancer, Viper | (2) Allies gain +20% Magic Resistance / (4) Allies gain +45% Magic Resistance |
| Scrappy | 2/4/6 | 8 | Alchemist, Bounty Hunter, Clockwerk, Sniper, Techies, Timbersaw, Tinker, Weaver | (2) A Random Scrappy are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / (4) All Scrappy Units are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / (6) All Allies are granted +9 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. |
| Shaman (old, beta) | 2/4 | 5 | Arc Warden, Enigma, Magnus, Nature's Prophet, Shadow Shaman | (2) Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 2 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle. / (4) Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 4 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle. |
| Spirit | 3 | 4 | Earth Spirit, Ember Spirit, Storm Spirit, Void Spirit | (3) When a Spirit casts an ability they enter a charged state. When 3 Spirits are in a charged state they perform the forbidden technique of the triangular Delta Slam, each dealing 200 magic damage to all enemies caught inside and applying their elemental effects. |
| Troll | 2/4 | 5 | Batrider, Dazzle, Shadow Shaman, Troll Warlord, Witch Doctor | (2) All Troll units gain +35 Attack Speed and other Allies gain +10 Attack Speed / (4) All Troll units gain +65 Attack Speed and other Allies gain +30 Attack Speed |
| Warlock | 2/4/6 | 8 | Alchemist, Broodmother, Disruptor, Shadow Fiend, Venomancer, Void Spirit, Warlock, Witch Doctor | (2) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 50% of the damage dealt. / (4) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 3 seconds. When either linked hero deals damage, both units are healed 70% of the damage dealt. / (6) Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 3.5 seconds. When either linked hero deals damage, both units are healed 90% of the damage dealt. |
| Warrior | 3/6 | 9 | Earth Spirit, Juggernaut, Kunkka, Pudge, Slardar, Tidehunter, Tiny, Troll Warlord, Tusk | (3) All Warriors gain +7 Armor / (6) All Warriors gain +17 Armor |

### Heroes in pool (77)

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Elusive, Assassin | 1000/1800/4000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 |
| T1 | Axe | Brawny, Brute | 1851/3703/7406 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 |
| T1 | Batrider | Troll, Knight | 1000/2000/4000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 |
| T1 | Bloodseeker | Blood-Bound, Deadeye | 1200/2400/4600 | 40-60/100-120/160-190 | 1.7 | 1 | 0 | 20 | 100 | 850 |
| T1 | Bounty Hunter | Scrappy, Assassin | 1100/1980/4400 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 |
| T1 | Drow Ranger | Heartless, Hunter | 800/1600/3200 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 |
| T1 | Enchantress | Savage, Druid, Healer | 800/1600/3000 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 |
| T1 | Nyx Assassin | Insect, Assassin | 1100/1980/4400 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 |
| T1 | Razor | Primordial, Mage | 1000/2000/4000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 |
| T1 | Shadow Demon | Heartless, Demon | 1200/2400/4800 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 |
| T1 | Shadow Shaman | Troll, Shaman (old, beta) | 1100/2200/4400 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 70 | 285 |
| T1 | Snapfire | Dragon, Inventor | 1250/2500/5000 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 |
| T1 | Tiny | Primordial, Warrior | 1300/2600/5200 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 |
| T1 | Tusk | Savage, Warrior | 1400/2800/5600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 |
| T1 | Venomancer | Savage, Warlock, Scaled | 1000/2000/4000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 |
| T1 | Warlock | Blood-Bound, Warlock, Healer | 1300/2900/5700 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 |
| T2 | Beastmaster | Brawny, Hunter | 1200/2400/4800 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 |
| T2 | Chaos Knight | Demon, Knight | 1500/3000/6000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 75 | 320 |
| T2 | Crystal Maiden | Human, Mage | 1000/2000/4000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 |
| T2 | Dazzle | Troll, Healer | 1500/3000/6000 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 |
| T2 | Ember Spirit | Spirit, Assassin | 1100/2200/4400 | 80-100/160-200/320-400 | 1.5 | 1 | 5 | 0 | 100 | 305 |
| T2 | Luna | Elusive, Knight | 1000/2000/4000 | 50-60/120-144/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 |
| T2 | Magnus | Shaman (old, beta), Brute | 1875/3750/7500 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 0 | 280 |
| T2 | Nature's Prophet | Druid, Shaman (old, beta) | 1000/2000/4000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 |
| T2 | Ogre Magi | Blood-Bound, Brute, Mage | 1600/3200/6000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 |
| T2 | Puck | Elusive, Dragon, Mage | 900/1800/3600 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 |
| T2 | Pudge | Heartless, Warrior | 2000/5000/9350 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 |
| T2 | Queen of Pain | Demon, Assassin | 1100/1980/4400 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 |
| T2 | Slardar | Scaled, Warrior | 1300/2600/5200 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 |
| T2 | Storm Spirit | Spirit, Mage | 1000/2000/4000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 |
| T2 | Timbersaw | Scrappy, Inventor | 1600/3200/6400 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 |
| T2 | Weaver | Insect, Elusive, Scrappy | 900/1800/3600 | 60-70/120-140/260-300 | 0.9 | 2 | 5 | 0 | 20 | 290 |
| T2 | Windranger | Elusive, Hunter | 1100/2200/4600 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 |
| T2 | Witch Doctor | Troll, Warlock | 1100/2200/4400 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 |
| T3 | Abaddon | Heartless, Knight | 1500/3000/6000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 |
| T3 | Clockwerk | Scrappy, Inventor | 1800/3600/7000 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 |
| T3 | Earth Spirit | Spirit, Warrior | 1700/3400/6800 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 |
| T3 | Io | Primordial, Druid | 1100/2200/4000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 |
| T3 | Juggernaut | Brawny, Warrior | 1200/2600/4800 | 65-70/155-165/280-300 | 1.0/0.9/0.8 | 1 | 5 | 0 | 100 | 300 |
| T3 | Legion Commander | Human, Champion | 900/1800/3600 | 40-50/80-100/180-228 | 1.25 | 1 | 5 | 0 | 100 | 315 |
| T3 | Lifestealer | Heartless, Brute | 1350/2700/6480 | 55-60/110-120/220-240 | 1.5/1.4/1.3 | 1 | 5 | 0 | 0 | 300 |
| T3 | Lina | Human, Mage | 1200/2400/4800 | 50-55/100-110/200-220 | 1.4 | 4 | 5 | 0 | 100 | 290 |
| T3 | Lycan | Human, Savage, Hunter | 1500/3000/7200 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 |
| T3 | Morphling | Primordial, Mage | 1250/2500/4000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 |
| T3 | Omniknight | Human, Knight, Healer | 1610/3220/7728 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 60 | 300 |
| T3 | Phantom Assassin | Elusive, Assassin | 1100/1980/4400 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 |
| T3 | Shadow Fiend | Demon, Warlock | 900/1800/3600 | 77-88/154-176/369-422 | 1.1 | 3 | 5 | 0 | 100 | 305 |
| T3 | Slark | Scaled, Assassin | 1000/1800/4800 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 |
| T3 | Sniper | Scrappy, Deadeye, Hunter | 900/1800/4320 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 |
| T3 | Terrorblade | Demon, Hunter | 1300/2600/6240 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 |
| T3 | Tinker | Scrappy, Inventor | 1500/3000/7000 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 |
| T3 | Treant Protector | Druid, Brute | 1500/3000/5000 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 |
| T3 | Viper | Scaled, Dragon | 1100/2200/6000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 |
| T4 | Alchemist | Scrappy, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 |
| T4 | Arc Warden | Primordial, Shaman (old, beta) | 1200/2400/4800 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 |
| T4 | Bristleback | Brawny, Savage | 2200/4400/8800 | 130-150/260-300/520-600 | 1.7 | 1 | 10 | 0 | 20 | 300 |
| T4 | Broodmother | Insect, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 275 |
| T4 | Doom | Demon, Brute | 2100/4200/8400 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 |
| T4 | Dragon Knight | Human, Dragon, Knight | 2000/4000/8000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 |
| T4 | Keeper of the Light | Human, Mage | 1100/2200/4400 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 |
| T4 | Kunkka | Human, Warrior | 1900/3800/7600 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 |
| T4 | Lone Druid | Savage, Druid | 1600/3200/6000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 |
| T4 | Mirana | Elusive, Hunter | 1500/3000/6000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 |
| T4 | Necrophos | Heartless, Healer | 1700/3400/6800 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 |
| T4 | Sand King | Savage, Insect | 1500/3000/6400 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 |
| T4 | Templar Assassin | Elusive, Assassin | 1500/2700/6000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 |
| T4 | Tidehunter | Scaled, Warrior | 2200/5400/11000 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 |
| T5 | Disruptor | Brawny, Warlock | 1800/3200/6800 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 |
| T5 | Enigma | Primordial, Shaman (old, beta) | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 |
| T5 | Faceless Void | Primordial, Assassin | 2000/3600/8000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 |
| T5 | Gyrocopter | Deadeye, Inventor | 1800/3600/7200 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 |
| T5 | Lich | Heartless, Mage | 1700/3400/6800 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 |
| T5 | Medusa | Scaled, Hunter | 1700/3400/6600 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 |
| T5 | Sven | Human, Scaled, Knight | 1900/3800/7600 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 200 | 300 |
| T5 | Techies | Scrappy, Inventor | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 |
| T5 | Troll Warlord | Troll, Warrior | 2400/4000/8000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 |
| T5 | Void Spirit | Spirit, Warlock | 1700/3400/6800 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 |

## Ace effects (2019-08-30 to 2020-02-25)

- **Disruptor** (Warlock): The duration of the effect of the Alliance is now increased by 1 second per Alliance level.
- **Enigma** (Primordial): Allied eidolons cast Malefice on their killer, stunning them for 0.4s every 2 seconds. The effect lasts 2 seconds per Alliance level. The target takes 50 damage per Alliance level each time they are stunned.
- **Faceless Void** (Assassin): Allied Assassins have a 5% chance per Assassin Alliance level to dodge attacks by jumping backward in time, fully avoiding both physical and magical damage.
- **Gyrocopter** (Deadeye): Attacks from Deadeye units pierce their main target, dealing 75% damage per Deadeye Alliance level to the unit behind them.
- **Lich** (Mage): Your army's mana generation from attacks is increased 10% per Mage Alliance level.
- **Medusa** (Hunter): Hunters deal 5% extra Attack Damage per Hunter Alliance level for each other Hunter attacking the same target.
- **Sven** (Knight): When Sven casts God's Strength, all allied Knights lose their Knight Alliance bonus and gain 50% damage per Knight Alliance level for the remainder of the round.
- **Techies** (Inventor): Grants Inventor units Chain Reaction. If an enemy dies as a result of an Inventor Alliance explosion, they explode just as if they were a part of your Inventor Alliance, damaging nearby enemies.
- **Troll Warlord** (Troll): Grants your units Mini-Bash: While attacking, your team has a 5% chance per Troll Alliance level of stunning their target for 0.25 seconds.
- **Void Spirit** (Spirit): Charged Spirit units deal 100% more attack damage.

## Beta global and alliance items

See `underlords-items.md` for the full item pool at each beta snapshot. Global items as the wiki last recorded them:

- Embarrassment of Riches (tier 1): Neutral rounds offer one additional item choice.
- Aegis of the immortal (tier 2): When you would otherwise take fatal damage, take no damage and destroy Aegis.
- Completing the Cycle (tier 2): When a Druid dies, allies up to 2 cells away are healed for [3%/5%/7%] of their max health per second for 6 seconds.
- Coordinated Assault (tier 2): Troll units give [+10/+20/+30] Attack Speed to allies 1 cell away.
- Fall From Grace (tier 2): Human units count as Heartless instead when counting Alliances.
- Silver Lining (tier 2): Get 1 charge when you win a fight. When you lose a fight, if charges remain, get 1 gold and lose 1 charge..
- Smuggler (tier 2): Quality of items from neutral rounds are offered from 1 wave higher.
- Summoning Stone (tier 2): Allied summoned units have +150 Health and +45 Attack Speed.
- Dawning of Ristul (tier 3): Healing for both crews is reduced by 50%.
- Dragon's Hoard (tier 3): Dragon units gain [0.5%/1%/1.5%] Attack damage for every gold you own at the beginning of combat.
- Elusive Targets (tier 3): Elusive units have 100% evasion for the first [2/3/4] seconds of a fight.
- Final Flash (tier 3): Mages reset their cooldown and get full mana when below [10%/20%/30%] health.
- Vicious Intent (tier 3): Each of your surviving units, including summons, deal 1 additional damage to opponents if they lose to you.
- Friends and Family Discount (tier 4): All units are 1 gold cheaper.
- Recruiter (tier 4): Rerolls cost 1 gold less.
- A Higher Class of Criminal (tier 5): Units in shop offered as if you were 1 level higher.
- Desperate Measures (tier 5): Your units gain +0.2% Attack Damage and +0.2% Mana gain on damage taken/received for each point of damage your Underlord has taken.
- Expanded Roster (tier 5): +1 Unit cap.
- Age of Chivalry (tier alliance item, no tier): Non-Knight units take [7.5/10/12.5]% less physical and magical damage for each allied Knight unit 1 cell away.
- Check the Bodies (tier alliance item, no tier): Scrappy units have a [10%/20%/30%] chance of producing 1 gold when they kill an enemy.
- Font of Creation (tier alliance item, no tier): Primordial units spawn Eidolons on death. Eidolons are weak ranged Primordial units that don't spawn Eidolons.
- Forged in Battle (tier alliance item, no tier): Brawny units start the round with [+25/+50/+75] Max Health for every unit they have killed.
- Hunter's Focus (tier alliance item, no tier): Hunters deal [5%/7%/10%] extra Attack Damage for each other Hunters attacking the same target.
- Indomitable Will (tier alliance item, no tier): Debuffs last [25%/50%/75%] less time on Human units.
- Pocket Sand (tier alliance item, no tier): Assassins blind enemies 1 cell away from where they land for [1/2/3] seconds (50% miss chance and 50% less mana generated when attacked).
- Retaliate (tier alliance item, no tier): Units attacking Scaled units take [30/45/60] damage/sec for 3 seconds. This timer resets each time the Scaled unit is attacked.
- Shaman Pluck (tier alliance item, no tier): Units attacking a Shaman have a [7%/12%/17%] chance of being turned into a chicken.
- Soul Sucking Syphon (tier alliance item, no tier): Allied units 1 cell away from Warlocks heal for [25%/35%/50%] of all damage caused by abilities and enemies.
- Strange Bedfellows (tier alliance item, no tier): Demon Hunters gain [+30%/+40%/+50%] Pure Damage for each unique allied Demon unit.
- Tooth and Claw (tier alliance item, no tier): Savage units apply a stackable [7/10/13] damage/sec bleed on attack.
- Unstable Reactor (tier alliance item, no tier): Allied Inventors explode on death dealing [10%/20%/30%] of their Max Health to all enemies 1 cell away.
- Unstoppable (tier alliance item, no tier): Warriors survive for an extra [1/2/3] seconds after receiving a killing blow.
- Wicked Intent (tier alliance item, no tier): Opponent's healing is reduced by [10%/15%/25%] per unique Heartless unit you have.
