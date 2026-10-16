# Dota Underlords heroes

Per-hero reference: stats at six data snapshots, ability text, 3-star effects, Ace effects, old abilities and the wiki change log. See `underlords.md` for conventions and sources.

Stat notes: HP and damage are 1-star/2-star/3-star. "Atk rate (s)" is seconds per attack (lower is faster). Range is in cells (1 = melee). "-" means the field is not set in that data file. 3-star effects were added on 2020-05-08 and later; they appear in the ability list as "<Hero> 3 Star Effect".

### Abaddon
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Fallen, Knight.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Heartless, Knight | 750/1500/3000 | 45-55/90-110/180-220 | 1.3 | 1 | 10 | 0 | 100 | 325 | yes |
| Pre-Big Update 2019-10-10 | T3 | Heartless, Knight | 750/1500/3000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | yes |
| Big Update 2019-10-24 | T3 | Heartless, Knight | 1500/3000/6000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | yes |
| Outlanders 2019-12-06 | T3 | Heartless, Knight | 1500/3000/6000 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | yes |
| Season One 2020-02-25 | T3 | Heartless, Knight | 1125/2250/4500 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | yes |
| Final 2020-11-19 | T3 | Fallen, Knight | 1125/2250/5400 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | yes |

Abilities (last wiki state):
- **Aphotic Shield** (cooldown 12.00, mana 100, magical): Abaddon protects a friendly unit with a shield that absorbs [100/300/600] damage for 10 seconds. When the shield is destroyed it bursts and deals [100/300/600] damage to enemies up to 2 cells away. Removes certain types of negative buffs and stuns on cast.
  - Abaddon casts Aphotic Shield based on the amount of damage taken recently by each unit.
  - Units that have Aphotic Shield on them do not generate mana from being attacked.
  - Units that have Blade Mail on them fully reflect damage taken by Aphotic Shield.
- **Abaddon 3 Star Effect**: At three stars, Abaddon gains the ability to apply the Curse of Avernus to enemies he attacks. Once Abaddon has attacked the same enemy unit 3 times in a row that unit becomes silenced for 2 seconds and Abaddon gains +30 Attack Speed against it. This refreshes on each additional landed attack, but starts again from 0 attacks if Abaddon changes targets.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Aphotic Shield: Abaddon protects a friendly unit with a shield that absorbs [100/300/500] damage for 10 seconds.  When the shield is destroyed it bursts and deals [100/300/500] damage to enemies up to 2 cells away.  Removes certain types of negative buffs and stuns on cast. (cd 12, mana 100)
- Pre-Big Update 2019-10-10: Aphotic Shield: Abaddon protects a friendly unit with a shield that absorbs [100/300/600] damage for 10 seconds.  When the shield is destroyed it bursts and deals [100/300/600] damage to enemies up to 2 cells away.  Removes certain types of negative buffs and stuns on cast. (cd 12, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Health changed from [1,125/2,250/4,500] to [1,125/2,250/5,400]
- July 03, 2020
  - Clarified Abaddon's 3 Star Effect description. The Curse (and the bonus) refresh each time Abaddon successfully lands an attack on his target, but on changing targets Abaddon must land 3 strikes to Curse the new target and receive the Attack Speed bonus again.
- July 02, 2020
  - Abaddon gains the ability to apply the Curse of Avernus to enemies he attacks. Once Abaddon has attacked the same enemy unit 3 times in a row that unit becomes silenced for 2 seconds and Abaddon gains +30 Attack Speed against it. The bonus is lost if Abaddon changes targets.
- July 19, 2019
  - Attack Damage Minimum changed from [45, 90, 180] to [45, 90, 216]
  - Attack Damage Maximum changed from [55, 110, 220] to [55, 110, 264]
  - Aphotic Shield:
  - Damage Absorb changed from [100, 300, 500] to [100, 300, 600]
- June 26, 2019
  - Base attack time changed: 1.3 -> 1.5
- June 17, 2019
  - Aphotic Shield Absorption/Damage increased from 100/150/200 to 100/300/500.

</details>

### Alchemist
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Brute, Rogue, Poisoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Scrappy, Warlock | 1000/2000/4000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Pre-Big Update 2019-10-10 | T4 | Scrappy, Warlock | 1000/2000/4000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Big Update 2019-10-24 | T4 | Scrappy, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Outlanders 2019-12-06 | T4 | Scrappy, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T4 | Scrappy, Warlock | 1500/3000/6000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 305 | no |
| Final 2020-11-19 | T3 | Brute, Rogue, Poisoner | 1300/2600/6240 | 40-60/80-120/192-288 | 1 | 1 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Acid Spray** (cooldown 10.0, mana 100, physical): Alchemist sprays high-pressure acid, affecting enemies up to 2 cells away from the target. Enemy units who step across the contaminated terrain take [10/20/35] damage per second and have their armor reduced by [-3/-6/-9].
  - Acid Spray lasts 10 seconds.
  - Acid Spray only stacks damage (separate damage ticks) when casted again before the previous one expires, but does not stack armor reduction.
- **Alchemist 3 Star Effect**: At the start of combat, Alchemist causes his Ogre to enter a chemically induced rage, increasing attack speed by 75 and becoming magic immune for 5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Acid Spray: Alchemist sprays high-pressure acid, affecting enemies up to 3 cells away from the target.  Enemy units who step across the contaminated terrain take [15/25/35] damage per second and have their armor reduced by [-5/-8/-11]. (cd 10, mana 100)

<details><summary>Wiki change log (3 patches)</summary>

- November 19, 2020
  - Acid Spray:
  - Damage changed from [10, 20, 30] to [10, 20, 35]
  - Radius changed from [2, 2, 3] to 2
- August 27, 2020
  - Tier changed from 4 to 3. Now has Brute Rogue Poisoner alliances
  - Health changed from [1,500/3,000/6,000] to [1,300/2,600/6,240]
  - Attack Damage Minimum changed from [50, 100, 200] to [40, 80, 192]
  - Attack Damage Maximum changed from [70, 140, 280] to [60, 120, 288]
  - Health regen changed from 5 to 0
  - Acid Spray:
  - Radius changed from 3 to [2, 2, 3] cells
  - Damage per second changed from [15, 25, 35] to [10, 20, 30]
  - Armor reduction changed from [-4, -7, -10] to [-3, -6, -9]
  - 3 Star Effect: Alchemist gains Chemical Rage at the start of battle, gaining Attack Speed and becoming Magic Immune for a few seconds.
- January 16, 2020
  - Acid Spray:
  - Armor Reduction changed from [-5, -8, -11] to [-4, -7, -10]

</details>

### Anti-Mage
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Rogue, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Elusive, Demon Hunter | 500/1000/2000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 10/20/30 | 100 | 310 | yes |
| Pre-Big Update 2019-10-10 | T1 | Elusive, Demon Hunter | 500/1000/2000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 100 | 310 | yes |
| Big Update 2019-10-24 | T1 | Elusive, Assassin | 1000/2000/4000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 | yes |
| Outlanders 2019-12-06 | T1 | Elusive, Assassin | 1000/1800/4000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 | yes |
| Final 2020-11-19 | T1 | Rogue, Hunter | 750/1500/3000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 | yes |

Abilities (last wiki state):
- **Mana Break**: Passive: The Anti-Mage burns [30/60/90] of the opponent's mana on each attack, dealing 50% of the mana burned as damage to the target.
  - Only the unit currently being attacked is affected by Mana Break. Extra targets from items such as Battle Fury or Maelstrom are not affected.
  - Mana Break completely cancels the casting of abilities if the unit being attacked is in the middle of casting an ability and that unit loses the mana required to cast it.
  - This prevents Keeper of the Light from casting Keeper of the Light completely, whereas other interruptions make him cast an Illuminate of lower strength.
- **Anti-Mage 3 Star Effect** (magical): Anti-Mage's Mana Break does an extra 200 damage to his target if his attack burns all remaining mana.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Mana Break: PASSIVE: The Anti-Mage burns [30/60/90] of the opponent's mana on each attack, dealing 50% of the mana burned as damage to the target. (cd -, mana -)
- Big Update 2019-10-24: Mana Break: PASSIVE: The Anti-Mage burns [30/60/90] of the opponent's mana on each attack, dealing 50% of the mana burned as damage to the target. He burns [2/4/6] hype when hitting Underlords. (cd -, mana -)
- Outlanders 2019-12-06: Mana Break: PASSIVE: The Anti-Mage burns [30/60/90] of the opponent's mana on each attack, dealing 50% of the mana burned as damage to the target. He burns [4/6/12] hype when hitting Underlords. (cd -, mana -)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Alliances changed from Elusive Assassin to Rogue Hunter
  - Health changed from [1000, 1800, 4000] to [750, 1500, 3000]
  - 3 star effect: Anti-Mage deals bonus damage during Mana Break if he lowers his target's Mana to 0.
- December 18, 2019
  - Removed from the game.
- November 21, 2019
  - Maximum Health changed from [1000, 2000, 4000] to [1000, 1800, 4000]
- November 14, 2019
  - Mana Break
  - Hype burned changed from [2, 4, 6] to [4, 6, 12]
- October 24, 2019
  - Alliance changed from Elusive Demon Hunter to Elusive Assassin
  - Maximum Mana changed from 100 to 0
  - Mana Break:
  - Also burns [2/4/6] Hype from Underlords.

</details>

### Arc Warden
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Primordial, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Primordial (beta version), Shaman (old, beta) | 600/1100/2200 | 75-85/150-170/250-290 | 1.7 | 4 | 5 | 5 | 100 | 280 | yes |
| Pre-Big Update 2019-10-10 | T4 | Primordial, Shaman (old, beta) | 600/1200/2400 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 | yes |
| Big Update 2019-10-24 | T4 | Primordial, Shaman (old, beta) | 1200/2400/4800 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 | yes |
| Outlanders 2019-12-06 | T4 | Primordial, Shaman (old, beta) | 1200/2400/4800 | 75-85/150-170/300-350 | 1.0 | 4 | 5 | 5 | 0 | 280 | yes |
| Season One 2020-02-25 | T1 | Primordial, Summoner | 650/1300/2600 | 35-45/70-90/140-180 | 1 | 4 | 5 | 5 | 100 | 280 | yes |
| Final 2020-11-19 | T1 | Primordial, Summoner | 650/1300/2600 | 35-45/70-90/140-180 | 1 | 4 | 5 | 5 | 100 | 280 | no |

Abilities (last wiki state):
- **Tempest Double** (cooldown 60.0, mana 100): Arc Warden creates a copy of himself with the same health he currently has. Clones do 50% less damage and gain 20% less mana per clone (stacks multiplicatively).
  - The clone can damage players at the end of the round.
  - With Primordial alliance active, the clones do not spawn Eidolons on death.
- **Arc Warden 3 Star Effect**: Tempest Double clones copy Arc Warden's item.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Tempest Double: Arc Warden creates a copy of himself.  The duplicate can use all of Arc Warden's current items and spells and has separate cooldowns, but generates -50% mana from outgoing attacks, and no mana upon recieving damage. (cd 60, mana 100)
- Pre-Big Update 2019-10-10: Tempest Double: Arc Warden creates a copy of himself.  The duplicate can use all of Arc Warden's current items and spells and has separate cooldowns, but generates [unknown]% mana from outgoing attacks, and no mana upon recieving damage. (cd 3, mana -)
- Big Update 2019-10-24: Tempest Double: Arc Warden creates a copy of himself with the same health he currently has.  The duplicate can use all of Arc Warden's current items and will attempt to attack the same enemy as his summoner. Clones cannot create Clones. (cd 3, mana -)
- Outlanders 2019-12-06: Tempest Double: Arc Warden creates a copy of himself with the same health he currently has.  The duplicate can use all of Arc Warden's current items and will attempt to attack the same enemy as his summoner. Clones cannot create Clones, and they do 50% less damage. (cd 3, mana -)

<details><summary>Wiki change log (11 patches)</summary>

- July 03, 2020
  - Tempest Double
  - Clones gain 20% less mana per clone (stacks multiplicatively).
- July 02, 2020
  - Tempest Double
  - 3 Star Effect: Clones now clone items.
- June 18, 2020
  - Arc Warden clones no longer spawn Eidolons.
- February 28, 2020
  - Clones can now deal damage to players at the end of the round.
- February 25, 2020
  - Tier changed from 4 to 1
  - Alliances changed from Primordial Shaman to Primordial Summoner
  - Maximum Mana changed from 0 to 100
  - Maximum Health changed from [900, 1800, 3600] to [650, 1300, 2600]
  - Attack Damage Minimum changed from [75, 150, 300] to [35, 70, 140]
  - Attack Damage Maximum changed from [85, 170, 350] to [45, 90, 180]
  - Tempest Double:
  - Tempest Double no longer copies equipped items and now reads 'Arc Warden creates a copy of himself with the same health he currently has.'
  - [Undocumented]: Clones do 50% less damage.
  - Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies can also make Tempest Double copies...
- December 18, 2019
  - Removed from the game.
- November 07, 2019
  - Tempest Double
  - Clones have a -50% debuff to their attack damage.
- September 12, 2019
  - Maximum Health changed from [700, 1400, 2800] to [600, 1200, 2400].
- September 05, 2019
  - Tempest Double:
  - There is now a 3 second cooldown on creating another clone whenever a clone dies.
- July 24, 2019
  - Attack Speed changed from [0.59, 0.67, 0.77] to [1.0, 1.0, 1.0]
  - Maximum Mana changed from 100 to 0
  - Maximum Health changed from [600, 1100, 2650] to [700, 1400, 2800]
  - Gold Cost changed from 3 to 4
  - Draft Tier changed from 3 to 4
  - Tempest Double:
  - Cooldown changed from 60 to 0
  - Mana Cost changed from 100 to 0
  - Now reads: Arc Warden creates a copy of himself with the same health he currently has. The clone can use all of Arc Warden's current items and will attempt to attack the same enemy as his summoner. Whenever this clone dies, Arc Warden will create a new one.
- July 19, 2019
  - Attack Speed changed from 0.59 to [0.59, 0.67, 0.77]
  - Maximum Health changed from [600, 1100, 2200] to [600, 1100, 2650]
  - Attack Damage Minimum changed from [75, 150, 250] to [75, 150, 300]
  - Attack Damage Maximum changed from [85, 170, 290] to [85, 170, 350]

</details>

### Axe
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Brawny, Brute.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Brawny (beta version), Warrior | 700/1400/2800 | 50-55/100-110/200-220 | 1.5 | 1 | 5 | 0 | 50 | 295 | yes |
| Pre-Big Update 2019-10-10 | T1 | Brawny, Warrior | 805/1610/3220 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 | yes |
| Big Update 2019-10-24 | T1 | Brawny, Brute | 1610/3220/6440 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 | yes |
| Outlanders 2019-12-06 | T1 | Brawny, Brute | 1851/3703/7406 | 50-55/100-110/200-220 | 1.5 | 1 | 5/10/15 | 0 | 50 | 295 | yes |
| Season One 2020-02-25 | T5 | Brawny, Brute | 3000/6000/12000 | 50-60/100-120/200-240 | 1.5 | 1 | 15 | 0 | 50 | 295 | yes |
| Final 2020-11-19 | T5 | Brawny, Brute | 3000/6000/12000 | 50-60/100-120/200-240 | 1.5 | 1 | 15 | 0 | 50 | 295 | yes |

Abilities (last wiki state):
- **Counter Helix** (pure): When attacked, Axe has a [ 18/20/22 ]% chance to perform a helix counter attack, dealing [ 50/100/150 ] pure damage to all enemies within 1 cell.
- **Culling Blade** (cooldown 0.5, mana 0): If Axe's target is below 25% health, he instantly kills them. With his axe.
  - If the target heals above the health threshold while Axe is casting Culling Blade, it will have no effect on the target.
  - Templar Assassin's Refraction blocks Culling Blade.
  - Abaddon's Aphotic Shield does not block Culling Blade.

Older abilities (wiki):
- **Berserker's Call** (cooldown 10.0, mana 50): Axe taunts enemy unit 1 cell away, forcing them to attack him for [2/3/4] seconds. While Berserker's Call is active, Axe gains [15/25/35] bonus armor.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Berserker's Call: Axe taunts enemy units 1 cell away, forcing them to attack him.  While Berserker’s Call is active, Axe gains [15/25/35] bonus armor. (cd 10, mana 50)
- Big Update 2019-10-24: Berserker's Call: Axe taunts enemy units 1 cell away, forcing them to attack him for [2/3/4] seconds.  While Berserker’s Call is active, Axe gains [15/25/35] bonus armor. (cd 10, mana 50)

<details><summary>Wiki change log (7 patches)</summary>

- September 24, 2020
  - Culling Blade:
  - No longer blocked by Aphotic Shield.
- July 02, 2020
  - Fixed Axe's Counter Helix killing Spiderlings even when they are invulnerable due to Broodmother's 3 Star Effect.
- February 25, 2020
  - Tier changed from 1 to 5
  - Armor changed from [5, 10, 15] to [15, 15, 15]
  - Maximum Health changed from [1385, 2775, 5555] to [3000, 6000, 12000]
  - Attack Damage Maximum changed from [55, 110, 220] to [60, 120, 240]
  - Removed ability Berserker's Call
  - Added ability Counter Helix:
  - When attacked, Axe has a [ 18, 20, 22 ]% chance to perform a helix counter attack, dealing [ 50, 100, 150 ] pure damage to all enemies within 1 cell.
  - Added ability Culling Blade:
  - If Axe's target is below 25% health, he instantly kills them. With his axe.
- November 07, 2019
  - Maximum Health changed from [1610, 3220, 6440] to [1851, 3703, 7406].
- October 24, 2019
  - Alliances changed from Brawny Warrior to Brawny Brute
- September 26, 2019
  - Armor changed from 5 to [5, 10, 15].
  - Maximum Health changed from [700, 1400, 2800] to [805, 1610, 3220].
- July 12, 2019
  - Berserker's Call: Since you can't cast spells while taunted, and since we are using the Silence effect, getting taunted now officially silences.

</details>

### Batrider
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Troll, Knight.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Troll, Knight | 500/1000/2000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |
| Pre-Big Update 2019-10-10 | T1 | Troll, Knight | 500/1000/2000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |
| Big Update 2019-10-24 | T1 | Troll, Knight | 1000/2000/4000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |
| Outlanders 2019-12-06 | T1 | Troll, Knight | 1000/2000/4000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |
| Season One 2020-02-25 | T1 | Troll, Knight | 750/1500/3000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |
| Final 2020-11-19 | T1 | Troll, Knight | 750/1500/3000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | yes |

Abilities (last wiki state):
- **Sticky Napalm** (cooldown 2.00, mana 20, magical): Batrider throws flaming sticky oil, coating the ground 1 cells away from point of impact. The oil adds [30/50/80] damage per stack to Batrider's attacks, and slows the movement and attack speed of enemies by 5%. Additional casts of Sticky Napalm continue to increase damage and slow percentages, up to [4/7/10] stacks.
  - Has a duration of 20 seconds.
  - Unlike most abilities, damage from Sticky Napalm generates mana.
  - However, this does not count as a separate damage instance and will not increase mana gain if Batrider would have already gained the maximum amount of mana with no Sticky Napalm.
  - Slows turn rate of affected targets by 70% (flat, not affected by stack count).
  - Damage increase affects all damage dealt by Batrider that is at least 5. Radiance and Orb of Venom do not proc it.
  - If multiple Batriders apply Sticky Napalm on the same unit, they all get the benefit but the damage is determined by the level of Sticky Napalm that was first applied.
  - However, the maximum stack count is not bound to the Sticky Napalm debuff itself, but to the Batrider that's casting the skill (a 1-star Batrider will not be able to increase the stack count once it's 4 or more, even if the Sticky Napalm debuff was first applied by a 2-star Batrider).
- **Batrider 3 Star Effect**: Sticky Napalm sets enemies on fire for 5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Sticky Napalm: Batrider throws sticky oil, coating the ground 2 cells away from point of impact.  The oil adds [20/40/80] damage per stack to Batrider's attacks, and slows the movement and attack speed of enemies by -5%.  Additional casts of Sticky Napalm continue to increase damage and slow percentages, up to 10 stacks. (cd 2, mana 20)
- Big Update 2019-10-24: Sticky Napalm: Batrider throws sticky oil, coating the ground 1 cells away from point of impact.  The oil adds [20/40/80] damage per stack to Batrider's attacks, and slows the movement and attack speed of enemies by 5%.  Additional casts of Sticky Napalm continue to increase damage and slow percentages, up to [4/7/10] stacks. (cd 2, mana 20)

<details><summary>Wiki change log (3 patches)</summary>

- May 08, 2020
  - Sticky Napalm:
  - Damage changed from [20, 40, 80] to [30, 50, 80].
  - At 3 stars, Sticky Napalm now sets enemies on fire for 5 seconds.
- January 24, 2020
  - Change minimum damage threshold for Sticky Napalm damage to proc from 25 to 5.
  - Fixed a bug where Radiance and Orb of Venom could trigger Sticky Napalm damage.
- October 24, 2019
  - Sticky Napalm:
  - Cell Range changed from 2 to 1

</details>

### Beastmaster
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Brawny, Hunter, Shaman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Brawny (beta version), Hunter | 600/1200/2400 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |
| Pre-Big Update 2019-10-10 | T2 | Brawny, Hunter | 600/1200/2400 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |
| Big Update 2019-10-24 | T2 | Brawny, Hunter | 1200/2400/4800 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |
| Outlanders 2019-12-06 | T2 | Brawny, Hunter | 1200/2400/4800 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T2 | Brawny, Hunter | 900/1800/3600 | 60-70/120-140/240-280 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |
| Final 2020-11-19 | T3 | Brawny, Hunter, Shaman | 1000/2000/4800 | 70-80/140-160/336-384 | 1.1 | 1 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Wild Axes** (cooldown 7.00, mana 100, physical): Beastmaster sends his axes flying and calls them home again, slicing through enemy units along their path for [60/100/140] damage. Each axe can hit an enemy once, and amplifies subsequent damage from Beastmaster by [10/20/30]%.
  - The damage amplification has a duration of 10s.
  - The damage amplification also affects item damage from Beastmaster.
- **Shaman Alliance Bonus**: On summoning, and every time it kills an enemy, the Shaman Alliance creature gains +[10/50/100] Attack Speed for 4 seconds.
- **Beastmaster 3 Star Effect**: At three stars, Beastmaster takes 75% less damage from summoned units and is immune to any magical effects from them.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Wild Axes: Beastmaster sends his axes flying and calls them home again, slicing through enemy units along their path for [60/100/140] damage.  Each axe can hit an enemy once, and amplifies subsequent damage from Beastmaster by [10/20/30]%. (cd 7, mana 100)

<details><summary>Wiki change log (2 patches)</summary>

- August 27, 2020
  - Tier changed from 2 to 3. Now has Brawny Hunter Shaman alliances.
  - Health changed from [900/1,800/3,600] to [1,000/2,000/4,800]
  - Attack Damage Minimum changed from [60, 120, 240] to [70, 140, 336]
  - Attack Damage Maximum changed from [70, 140, 280] to [80, 160, 384]
- June 18, 2020
  - At three stars, Beastmaster takes 75% less damage from summons and is immune to their magic.

</details>

### Bloodseeker
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Blood-Bound, Deadeye.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Human, Assassin | 500/1000/2000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 | yes |
| Pre-Big Update 2019-10-10 | T1 | Human, Deadeye, Assassin | 500/1000/2000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 | yes |
| Big Update 2019-10-24 | T1 | Blood-Bound, Deadeye | 1000/2000/4000 | 40-60/100-120/160-190 | 1.7 | 1 | 5 | 20 | 100 | 850 | yes |
| Outlanders 2019-12-06 | T1 | Blood-Bound, Deadeye | 1200/2400/4600 | 40-60/100-120/160-190 | 1.7 | 1 | 0 | 20 | 100 | 850 | yes |
| Season One 2020-02-25 | T1 | Blood-Bound, Deadeye | 900/1800/3450 | 40-60/100-120/160-190 | 1.7 | 1 | 0 | 20 | 100 | 850 | yes |
| Final 2020-11-19 | T1 | Blood-Bound, Deadeye | 900/1800/3450 | 40-60/100-120/160-190 | 1.7 | 1 | 0 | 20 | 100 | 850 | no |

Abilities (last wiki state):
- **Bloodrage**: Passive: As Bloodseeker's enemy's health drops, he gains attack speed, up to a maximum of 300 at 30% health. Whenever Bloodseeker kills a target, he regains 35% of their max health. At 3 stars, this occurs when any enemy dies within 1 cell of Bloodseeker.
- **Bloodseeker 3 Star Effect**: At three stars, Bloodseeker gains the Critical Hit bonus of the current active level of Assassin Alliance, if any.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Bloodrage: PASSIVE: Every second Bloodseeker takes damage equal to 7% of his max health.  For every % of health missing, he gains 5% attack speed.  On a kill, he regains 35% of his max health. (cd -, mana -)
- Pre-Big Update 2019-10-10: Bloodrage: PASSIVE: Every second Bloodseeker takes damage equal to 7% of his max health.  For every % of health missing, he gains [unknown]% attack speed.  On a kill, he regains 35% of his max health. (cd -, mana -)
- Big Update 2019-10-24: Bloodrage: PASSIVE: Every second Bloodseeker takes damage equal to 7% of his max health.  As he loses health, he gains attack speed, up to a maximum of 500 at 20% health.  On a kill, he regains 35% of his max health. (cd -, mana -)
- Outlanders 2019-12-06: Bloodrage: PASSIVE: Every second Bloodseeker takes damage equal to 7% of his max health.  As he loses health, he gains attack speed, up to a maximum of 500 at [25/30/35]% health.  Whenever an adjacent enemy dies, he regains 35% of his max health. (cd -, mana -)

<details><summary>Wiki change log (8 patches)</summary>

- July 16, 2020
  - 3 Star Effect: Bloodseeker heals when any enemy within 1 cell of him dies, not just his current target. In addition, he gains the Critical Hit chance and bonus of the current active level of Assassin Alliance, if any.
- March 19, 2020
  - Bloodrage:
  - Bonus Attack Speed changed from 500 to 300
  - Target health at which Bloodseeker reaches maximum bonus Attack Speed changed from [25, 30, 35] to 30
- March 05, 2020
  - Bloodrage
  - Now reads: As Bloodseeker's enemy's health drops, he gains attack speed, up to a maximum of 500 at [25, 30, 35]% health. Whenever Bloodseeker kills a target, he regains 35% of their max health
- December 05, 2019
  - Bloodrage
  - Bloodseeker now heals if any adjacent enemy unit dies, no matter what kills it.
- November 07, 2019
  - Armor changed from 5 to 0.
  - Maximum Health changed from [1000, 2000, 4000] to [1200, 2400, 4600].
  - Bloodrage
  - Bloodseeker now reaches maximum attack speed at [25, 30, 35]% health.
- October 24, 2019
  - Alliances changed from Human Deadeye Assassin to Blood-Bound Deadeye
- September 19, 2019
  - Bloodrage:
  - Now ramps up to his existing maximum attack speed bonus of 500 earlier, at 20% health (was 0%).
- August 30, 2019
  - Alliance changed from Human Assassin to Human Deadeye Assassin.

</details>

### Bounty Hunter
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Rogue, Assassin.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Scrappy, Assassin | 550/1100/2200 | 45-60/90-120/180-240 | 1 | 1 | 5 | 0 | 100 | 315 | yes |
| Pre-Big Update 2019-10-10 | T1 | Scrappy, Assassin | 550/1100/2200 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | yes |
| Big Update 2019-10-24 | T1 | Scrappy, Assassin | 1100/2200/4400 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T1 | Scrappy, Assassin | 1100/1980/4400 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T1 | Scrappy, Assassin | 825/1485/3300 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | no |
| Final 2020-11-19 | T1 | Rogue, Assassin | 825/1650/3300 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | yes |

Abilities (last wiki state):
- **Shuriken Toss** (cooldown 5.00, mana 100, magical): Bounty Hunter hurls a deadly shuriken at an enemy unit, dealing [300/500/700] damage and mini-stunning the target.
- **Bounty Hunter 3 Star Effect**: Bounty Hunter will generate 1 gold when killing an enemy hero.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Shuriken Toss: Bounty Hunter hurls a deadly shuriken at an enemy unit, dealing [300/500/700] damage and mini-stunning the target. (cd 5, mana 100)

<details><summary>Wiki change log (3 patches)</summary>

- August 27, 2020
  - 3 Star Effect: Bounty Hunter gives the player 1 Gold during combat for each hero he kills.
- November 21, 2019
  - Maximum Health changed from [1100, 2200, 4400] to [1100, 1980, 4400]
- August 30, 2019
  - Attack Damage Minimum changed from [45, 90, 180] to [50, 100, 200].
  - Attack Damage Maximum changed from [60, 120, 240] to [70, 140, 280].

</details>

### Bristleback
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Brawny, Savage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T4 | Brawny, Savage | 2200/4400/8800 | 130-150/260-300/520-600 | 1.7 | 1 | 10 | 0 | 20 | 300 | yes |
| Outlanders 2019-12-06 | T4 | Brawny, Savage | 2200/4400/8800 | 130-150/260-300/520-600 | 1.7 | 1 | 10 | 0 | 20 | 300 | yes |
| Season One 2020-02-25 | T2 | Brawny, Savage | 1350/2700/5250 | 100-130/200-260/400-520 | 1.7 | 1 | 5 | 0 | 20 | 300 | yes |
| Final 2020-11-19 | T2 | Brawny, Savage | 1350/2700/5400 | 100-130/200-260/400-520 | 1.7 | 1 | 5 | 0 | 20 | 300 | yes |

Abilities (last wiki state):
- **Quill Spray** (cooldown [3.00/3.00/1.50], mana 20, physical): Bristleback sprays quills from his back, damaging all enemy units 2 cells away. Enemies take [20/40/80] damage along with an additional [30/32/34] damage for each stack of Quill Spray they have been hit with this round up to a maximum of 5 stacks.
- **Bristleback 3 Star Effect**: Quill Spray's cooldown reduced to 1.5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Quill Spray: Bristleback sprays quills from his back, damaging all enemy units 2 cells away. Enemies take [40/80/160] damage along with an additional [60/64/68] damage for each stack of Quill Spray they have been hit with this round up to a maximum of 5 stacks. (cd 3, mana 20)

<details><summary>Wiki change log (5 patches)</summary>

- September 04, 2020
  - Quill Spray:
  - Cooldown changed from [3, 3, 1] to [3, 3, 1.5]
- August 27, 2020
  - Maximum Health changed from [1,350/2,700/5,250] to [1,350/2,700/5,400]
- July 16, 2020
  - 3 Star Effect: Quill Spray cooldown lowered to 1 second.
- February 28, 2020
  - Armor changed from 10 to 5.
- February 25, 2020
  - Tier changed from 4 to 2
  - Maximum Health changed from [1650, 3300, 6600] to [1350, 2700, 5250]
  - Attack Damage Minimum changed from [130, 260, 520] to [100, 200, 400]
  - Attack Damage Maximum changed from [150, 300, 600] to [130, 260, 520]
  - Quill Spray:
  - Base damage changed from [40, 80, 160] to [20, 40, 80]
  - Quill stack damage changed from [60, 64, 68] to [30, 32, 34]

</details>

### Broodmother
- Status: Rotated out at the end (still in final data). First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Insect, Warlocks.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T4 | Insect, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 275 | yes |
| Outlanders 2019-12-06 | T4 | Insect, Warlock | 2000/4000/8000 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 275 | yes |
| Season One 2020-02-25 | T3 | Insect, Warlock | 1300/2600/6240 | 50-70/100-140/240-336 | 1 | 1 | 5 | 0 | 100 | 275 | yes |
| Final 2020-11-19 | T3 | Insect, Warlock | 1300/2600/6240 | 50-70/100-140/240-336 | 1 | 1 | 5 | 0 | 100 | 275 | no |

Abilities (last wiki state):
- **Spawn Spiderlings** (cooldown 11.0, mana 100): Broodmother injects her young into an enemy unit, dealing [150/250/350] damage. If the target is killed [4/6/12] spiderlings will hatch. Spiderlings are untargetable units that deal low damage and have 1 health.
  - Broodmother prefers to cast Spawn Spiderlings on her current target.
- **Broodmother 3 Star Effect**: At three stars, while alive Broodmother makes all allied Spiderlings invulnerable.

Older abilities (wiki):
- **Spin Web** (cooldown 25, mana 100): Broodmother spins a web 2 cells wide on top of an enemy unit. Enemies on the web lose [20/30/40] attack speed, 80% movement speed, and [50/75/100]% cast speed.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Spin Web: Broodmother spins a web 2 cells wide on top of an enemy unit. Enemies on the web lose [20/30/40] attack speed, 80% movement speed, and [50/75/100]% cast speed. (cd 25, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- May 28, 2020
  - 3 Star Effect: All friendly Spiderlings (both hers and Insect Alliance Spiderlings) are invulnerable while Broodmother is alive.
- April 30, 2020
  - Maximum Health changed from [1200, 2400, 5760] to [1300, 2600, 6240]
  - Spawn Spiderlings:
  - Broodmother's Spiderlings cause targets they bite to suffer a miss chance when the Insect Alliance is active.
- April 03, 2020
  - Attack Damage Minimum changed from [50, 100, 200] to [50, 100, 240]
  - Attack Damage Maximum changed from [70, 140, 280] to [70, 140, 336]
- March 05, 2020
  - Spawn Spiderlings:
  - Broodmother now prefers to cast Spawn Spiderlings on her current target.
- February 25, 2020
  - Removed ability Spin Web
  - Added ability Spawn Spiderlings:
  - Broodmother injects her young into an enemy unit, dealing [ 150, 250, 350 ] damage. If the target is killed [ 4, 6, 12 ] spiderlings will hatch. Spiderlings are untargetable units that deal low damage and have 1 health.
- December 18, 2019
  - Gold Cost changed from 4 to 3
  - Maximum Health changed from [1500, 3000, 6000] to [1200, 2400, 5760]

</details>

### Chaos Knight
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Demon, Knight.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Demon, Knight | 750/1500/3000 | 40-110/80-220/160-440 | 1.3 | 1 | 5 | 0 | 100 | 320 | yes |
| Pre-Big Update 2019-10-10 | T2 | Demon, Knight | 750/1500/3000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 100 | 320 | yes |
| Big Update 2019-10-24 | T2 | Demon, Knight | 1500/3000/6000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 100 | 320 | yes |
| Outlanders 2019-12-06 | T2 | Demon, Knight | 1500/3000/6000 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 75 | 320 | yes |
| Season One 2020-02-25 | T2 | Demon, Knight | 1125/2250/4500 | 55-130/110-240/200-480 | 1.3 | 1 | 5 | 0 | 75 | 320 | yes |
| Final 2020-11-19 | T2 | Demon, Knight | 1125/2250/4500 | 55-130/110-260/220-520 | 1.3 | 1 | 5 | 0 | 75 | 320 | yes |

Abilities (last wiki state):
- **Chaos Bolt** (cooldown 5.00, mana 75, magical): Chaos Knight throws a mysterious bolt of energy that deals [100/150/200]-[200/300/400] damage and stuns for [1/2/3]-[2/3/4] seconds.
  - Cast range is 3 cells.
- **Chaos Knight 3 Star Effect**: At three stars, Chaos Knight's allied Knights always count as adjacent to a friendly Knight.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Chaos Bolt: Chaos Knight throws a mysterious bolt of energy that stuns an enemy unit for a random duration as well as dealing a random amount of damage. (cd 5, mana 100)
- Big Update 2019-10-24: Chaos Bolt: Chaos Knight throws a mysterious bolt of energy that deals [50/100/150]-[200/300/400] damage and stuns for 1-[2/3/4] seconds. (cd 5, mana 100)
- Outlanders 2019-12-06: Chaos Bolt: Chaos Knight throws a mysterious bolt of energy that deals [100/150/200]-[200/300/400] damage and stuns for [1/2/3]-[2/3/4] seconds. (cd 5, mana 75)

<details><summary>Wiki change log (4 patches)</summary>

- August 27, 2020
  - Attack Damage Minimum changed from [55, 110, 200] to [55, 110, 220]
  - Attack Damage Maximum changed from [135, 240, 480] to [130, 260, 520]
- May 14, 2020
  - At three stars, Chaos Knight's allied Knights always count as adjacent to a friendly Knight.
- November 21, 2019
  - Maximum Mana changed from 100 to 75
  - Chaos Bolt:
  - Min Damage changed from [50, 100, 150] to [100, 150, 200]
  - Min Stun changed from [1, 1, 1] to [1, 2, 3]
  - Mana Cost changed from 100 to 75
- June 25, 2019
  - Damage Increased from [40-110, 80-220, 160-440] to [55-135, 110-240, 200-480].

</details>

### Clockwerk
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 2, Scrappy, Inventor.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Scrappy, Inventor (beta version) | 700/1400/2800 | 45-55/90-110/180-220 | 1.4 | 1 | 5/7/9 | 0 | 100 | 310 | yes |
| Pre-Big Update 2019-10-10 | T3 | Scrappy, Inventor | 950/1800/3500 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 | yes |
| Big Update 2019-10-24 | T3 | Scrappy, Inventor | 1800/3600/7000 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 | yes |
| Outlanders 2019-12-06 | T3 | Scrappy, Inventor | 1800/3600/7000 | 55-65/110-130/220-260 | 1.4 | 1 | 10/13/15 | 0 | 100 | 310 | yes |
| Season One 2020-02-25 | T2 | Scrappy, Inventor | 1350/2700/5250 | 50-60/100-120/200-240 | 1.5 | 1 | 8/10/12 | 0 | 100 | 310 | no |
| Final 2020-11-19 | T2 | Scrappy, Inventor | 1350/2700/5250 | 50-60/100-120/200-240 | 1.5 | 1 | 8/10/12 | 0 | 100 | 310 | no |

Abilities (last wiki state):
- **Battery Assault** (cooldown [12.00/10.00/8.00], mana 100, magical): Clockwerk discharges high-powered shrapnel at random enemy units 1 cell away every 0.7 seconds for 5 seconds, dealing [50/75/100] magical damage and mini-stunning them.
  - If Clockwerk recasts Battery Assault before the first cast ends, it only refreshes the duration and does not stack.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Battery Assault: Clockwerk discharges high-powered shrapnel at random enemy units 1 cell away for 5 seconds, dealing [50/75/100] magical damage and mini-stunning them. (cd [12/10/8], mana 100)
- Big Update 2019-10-24: Battery Assault: Clockwerk discharges high-powered shrapnel at random enemy units 1 cell away every [0.7/0.7/0.35] seconds for 5 seconds, dealing [50/75/100] magical damage and mini-stunning them. (cd [12/10/8], mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- January 09, 2020
  - Battery Assault:
  - Delay between damage procs changed from [0.7, 0.7, 0.35] to 0.7
- December 18, 2019
  - Gold Cost changed from 3 to 2
  - Attack Rate changed from 0.714 to 0.667
  - Armor changed from [10, 13, 15] to [8, 10, 12]
  - Attack Damage Minimum changed from [55, 110, 220] to [50, 100, 200]
  - Attack Damage Maximum changed from [65, 130, 260] to [60, 120, 240]
- July 24, 2019
  - Armor changed from [5, 7, 10] to [10, 13, 15]
  - Maximum Health changed from [800, 1600, 3200] to [950, 1800, 3500]
- July 19, 2019
  - Armor changed from [5, 7, 9] to [5, 7, 10]
  - Maximum Health changed from [700, 1400, 2800] to [800, 1600, 3200]
  - Attack Damage Minimum changed from [45, 90, 180] to [55, 110, 220]
  - Gold Cost changed from 1 to 3
  - Draft Tier changed from 1 to 3
  - Attack Damage Maximum changed from [55, 110, 220] to [65, 130, 260]
  - Battery Assault:
  - Damage Tick Rate changed from 0.7 to [0.7, 0.7, 0.35]

</details>

### Crystal Maiden
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Human, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Human, Mage | 450/900/1800 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 | yes |
| Pre-Big Update 2019-10-10 | T3 | Human, Mage | 550/1100/2000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 | yes |
| Big Update 2019-10-24 | T2 | Human, Mage | 1000/2000/4000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 | yes |
| Outlanders 2019-12-06 | T2 | Human, Mage | 1000/2000/4000 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 275 | yes |
| Season One 2020-02-25 | T1 | Human, Mage | 700/1400/2800 | 40-45/80-90/160-180 | 1.5 | 4 | 5 | 0 | 100 | 275 | yes |
| Final 2020-11-19 | T1 | Human, Mage | 700/1400/2800 | 40-45/80-90/160-180 | 1.5 | 4 | 5 | 0 | 100 | 275 | yes |

Abilities (last wiki state):
- **Frostbite** (cooldown [10/8/6], mana 100, magical): Encases an enemy unit in ice, prohibiting movement and attack, while dealing [100/150/200] damage per second for 5 seconds. Targets affected by frostbite cannot be healed.
  - The afflicted unit can still cast spells.
- **Crystal Maiden 3 Star Effect**: At three stars, all allies have their cooldowns reduced by 20% while Crystal Maiden is alive.

Older abilities (wiki):
- **Arcane Aura**: Passive: Every 2 seconds, Crystal Maiden grants [8/14/20] mana to all friendly units.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Arcane Aura: PASSIVE: Every 2 seconds, Crystal Maiden grants [8/14/20] mana to all friendly units. (cd -, mana -)
- Big Update 2019-10-24: Frostbite: Encases an enemy unit in ice, prohibiting movement and attack, while dealing [unknown] damage per second for [3/4/5] seconds. (cd [10/8/6], mana 100)
- Outlanders 2019-12-06: Frostbite: Encases an enemy unit in ice, prohibiting movement and attack, while dealing [100/125/150] damage per second for [3/4/5] seconds. Targets affected by Frostbite cannot be healed. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (8 patches)</summary>

- May 21, 2020
  - At three stars, friendly units gain 20% cooldown reduction.
- April 10, 2020
  - Attack Rate changed from 0.588 to 0.667
  - Maximum Health changed from [600, 1200, 2400] to [700, 1400, 2800]
  - Frostbite:
  - Duration changed from [3, 4, 5] to 5
- February 25, 2020
  - Tier changed from 2 to 1
  - Maximum Health changed from [750, 1500, 3000] to [600, 1200, 2400]
  - Frostbite:
  - Duration changed from [4, 5, 6] to [3, 4, 5]
- January 17, 2020
  - Crystal Maiden will no longer cast Frostbite on a target that is currently frostbitten.
- January 16, 2020
  - Frostbite:
  - Damage changed from [100, 125, 150] to [100, 150, 200]
  - Duration changed from [3, 4, 5] to [4, 5, 6]
- December 05, 2019
  - Frostbite:
  - Now prevents the target from healing for the duration of Frostbite's effects.
- October 24, 2019
  - Tier changed from 3 to 2
  - Removed Arcane Aura.
  - Added Frostbite - Encases an enemy unit in ice, prohibiting movement and attack, while dealing [100/125/150] damage per second for [3/4/5] seconds.
- August 01, 2019
  - Maximum Health changed from (450, 900, 1800) to (550, 1100, 2000)
  - Gold Cost changed from 2 to 3

</details>

### Dazzle
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Troll, Healer, Poisoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T2 | Troll, Healer | 1500/3000/6000 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 | yes |
| Outlanders 2019-12-06 | T2 | Troll, Healer | 1500/3000/6000 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 | yes |
| Season One 2020-02-25 | T2 | Troll, Healer | 1125/2250/4500 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 | yes |
| Final 2020-11-19 | T1 | Troll, Healer, Poisoner | 900/1800/3600 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 | yes |

Abilities (last wiki state):
- **Shadow Wave** (cooldown [10/8/6], mana 100, physical): Dazzle sends out a bolt of power that arcs between up to [3/4/5] allies that are 3 cells apart, healing each ally for [80/140/200] while damaging any enemy units 1 cell away for [80/140/200]. Dazzle is always healed by Shadow Wave.
- **Dazzle 3 Star Effect**: Dazzle offers a Shallow Grave to all friendly units up to 1 cell away. All blessed units survive for an extra 5 seconds after receiving a killing blow.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Shadow Wave: Dazzle sends out a bolt of power that arcs between up to [3/4/5] allies that are 3 cells apart, healing each ally for [80/140/200] while damaging any enemy units 1 cell away for [80/140/200]. Dazzle is always healed by Shadow Wave. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (3 patches)</summary>

- August 27, 2020
  - Tier changed from 2 to 1. Now also a Poisoner.
  - Health changed from [1,125/2,250/4,500] to [900/1,800/3,600]
  - Shadow Wave:
  - Damage/Healing changed from [100, 170, 240] to [80, 140, 200]
- May 28, 2020
  - 3 Star Effect: Dazzle gains Shallow Grave. All friendly units within 1 cell of Dazzle survive an extra 5 seconds after receiving a killing blow.
- April 30, 2020
  - Shadow Wave:
  - Damage/Healing changed from [80, 140, 200] to [100, 170, 240]

</details>

### Death Prophet
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Fallen, Heartless.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T4 | Fallen, Heartless | 1250/2500/5000 | 50-60/100-120/200-240 | 1.25 | 3 | 5 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Exorcism** (cooldown [20.00/15.00/10.00], mana 100, physical): Death Prophet sends out a stream of up to [6/8/10] evil spirits for a duration of 6 seconds. They roam the battlefield in a 3 cell radius, dealing [83/100/110] damage to all enemies they pass through and returning to heal Death Prophet for 25% of the damage dealt.

### Disruptor
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 4, Brawny, Warlocks.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Brawny (beta version), Shaman (old, beta) | 700/1400/2800 | 45-50/90-100/180-200 | 1.5 | 4 | 5 | 0 | 100 | 295 | yes |
| Pre-Big Update 2019-10-10 | T5 | Brawny, Warlock | 900/1600/3400 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 | yes |
| Big Update 2019-10-24 | T5 | Brawny, Warlock | 1800/3200/6800 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 | yes |
| Outlanders 2019-12-06 | T5 | Brawny, Warlock | 1800/3200/6800 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 | yes |
| Season One 2020-02-25 | T4 | Brawny, Warlock | 1000/2000/4000 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 | yes |
| Final 2020-11-19 | T4 | Brawny, Warlock | 1000/2000/4000 | 54-60/108-120/216-240 | 1.5 | 4 | 5 | 0 | 100 | 295 | no |

Abilities (last wiki state):
- **Static Storm** (cooldown [15.00/12.00/9.00], mana 100, magical): Disruptor creates a damaging static storm that silences all enemy units up to 2 cells away from the center for [3/4/5] seconds. The damage starts off weak but ramps up, doing a total of [100/200/400] over the duration of the storm.
  - Disruptor centres Static Storm on a random enemy.
- **Ace effect (2019-08-30 to 2020-02-25, Warlock Ace):** The duration of the effect of the Alliance is now increased by 1 second per Alliance level.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Static Storm: Disruptor creates a damaging static storm that also silences all enemy units up to 2 cells away from his target for [3/4/5] seconds.  The damage starts off weak, but ramps up in power until reaching its maximum damage of [100/150/200]. (cd [15/12/9], mana 100)
- Pre-Big Update 2019-10-10: Static Storm: Disruptor creates a damaging static storm that also silences all enemy units up to 2 cells away from his target for [3/4/5] seconds.  The damage starts off weak, but ramps up in power until reaching its maximum damage of [unknown]. (cd [15/12/9], mana 100) / Ace/alliance bonus (internal id disruptor_warlock_bonus): see the Ace effect entry (cd -, mana -)
- Big Update 2019-10-24: Static Storm: Disruptor creates a damaging static storm that silences all enemy units up to 2 cells away from the center for [3/4/5] seconds.  The damage starts off weak but ramps up, doing a total of [115/250/425] over the duration of the storm. (cd [15/12/9], mana 100) / Ace/alliance bonus (internal id disruptor_warlock_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (7 patches)</summary>

- April 30, 2020
  - Static Storm:
  - Total Damage changed from [115, 250, 425] to [100, 200, 400]
- February 25, 2020
  - Removed Ace effect.
  - Tier changed from Ace to 4
  - Maximum Health changed from [1350, 2400, 5100] to [1000, 2000, 4000]
- December 02, 2019
  - Ace tier Effect: Instead of increasing number of links per Alliance level, the duration of the link is now increased by 1 second per Alliance level.
- September 26, 2019
  - Disruptor:
  - Fixed an issue where total damage dealt could be wrong.
  - No longer gives units in the storm a miss chance.
- September 12, 2019
  - Ace tier Effect:
  - Now reads: The effect of the Warlocks Alliance is now applied to 1 target per Alliance level.
- August 30, 2019
  - Draft Tier changed from 4 to Ace for the Warlock Alliance.
  - Alliances changed from Brawny Shaman Warlock to Brawny Warlock.
  - Maximum Health changed from [700, 1400, 2800] to [900, 1600, 3400].
  - Attack Damage Minimum changed from [45, 90, 180] to [54, 108, 216].
  - Attack Damage Maximum changed from [50, 100, 200] to [60, 120, 240].
  - Ace Effect: The effect of the Warlock Alliance is now applied to 2 units in addition to the warlock itself, rather than one unit.
- July 04, 2019
  - Alliances changed from Brawny Shaman to Brawny Shaman Warlock.
  - Static Storm:
  - Also blinds the targets inside the cloud for a 25% miss chance.

</details>

### Doom
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Demon, Brute.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Demon, Warrior | 1050/2100/4200 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 | yes |
| Pre-Big Update 2019-10-10 | T4 | Demon, Warrior | 1050/2100/4200 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 | yes |
| Big Update 2019-10-24 | T4 | Demon, Brute | 2100/4200/8400 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 | yes |
| Outlanders 2019-12-06 | T4 | Demon, Brute | 2100/4200/8400 | 110-120/220-240/440-480 | 1.3 | 1 | 5 | 0 | 100 | 285 | yes |
| Season One 2020-02-25 | T4 | Demon, Brute | 1575/3150/6300 | 110-120/220-240/440-480 | 1.1 | 1 | 15 | 0 | 100 | 285 | yes |
| Final 2020-11-19 | T4 | Demon, Brute | 1575/3150/6300 | 110-120/220-240/440-480 | 1.1 | 1 | 15 | 0 | 100 | 285 | yes |

Abilities (last wiki state):
- **Doom** (cooldown 20, mana 100, pure): Doom invokes a powerful curse that prevents an enemy from casting spells and applies break to them, while also taking [60/90/120] damage per second. Lasts 20 seconds.
  - Targets a random unit.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Doom: Doom invokes a powerful curse that prevents an enemy from casting spells or using items, while also taking [60/90/120] damage per second.  Lasts 20 seconds. (cd 10, mana 100)
- Big Update 2019-10-24: Doom: Doom invokes a powerful curse that prevents an enemy from casting spells and applies Break to them, while also taking [60/90/120] damage per second.  Lasts 20 seconds. (cd 20, mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- July 09, 2020
  - Attack Rate changed from 0.769 to 0.909
  - Armor changed from 5 to 15
- October 24, 2019
  - Alliances changed from Demon Warrior to Demon Brute
  - Doom:
  - Cooldown changed from 10 to 20
- August 23, 2019
  - Doom's Doom now disables the Brawny health buff.
- August 08, 2019
  - Doom's Doom now uniformly disables passive bonuses, including alliance passive bonuses.
  - Ability description changed to reflect this.
  - Doom's cosmetics upgrade each star level.
- June 14, 2019
  - Doom no longer uses base cosmetics.

</details>

### Dragon Knight
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Human, Dragon, Knight.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Human, Dragon, Knight | 800/1600/3200 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 | yes |
| Pre-Big Update 2019-10-10 | T4 | Human, Dragon, Knight | 1000/2000/4000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 | yes |
| Big Update 2019-10-24 | T4 | Human, Dragon, Knight | 2000/4000/8000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 | yes |
| Outlanders 2019-12-06 | T4 | Human, Dragon, Knight | 2000/4000/8000 | 50-60/100-120/200-240 | 1.2 | 1 | 10 | 0 | 100 | 295 | yes |
| Season One 2020-02-25 | T5 | Human, Dragon, Knight | 1400/2800/5600 | 55-75/110-150/220-300 | 1.2 | 1 | 10 | 0 | 100 | 295 | yes |
| Final 2020-11-19 | T5 | Human, Dragon, Knight | 1400/2800/5600 | 55-75/110-150/220-300 | 1.2 | 1 | 10 | 0 | 50 | 295 | yes |

Abilities (last wiki state):
- **Breathe Fire** (cooldown [7.00/6.00/5.00], mana 50, magical): Unleashes a breath of fire in front of Dragon Knight that burns enemies for [150/200/250] damage and reduces the damage their attacks deal by [25/35/45]% for 4 seconds.
  - Breathe Fire has a range of 3.
- **Elder Dragon Form** (cooldown 60.00, mana 0, magical): Unlocked by the Dragon alliance bonus Passive: At the beginning of combat Dragon Knight takes the form of one of three powerful elder dragons, increasing his movement speed by 30% and attack damage by [40/80/120] and granting him new powers. Green Dragon: Attacks deal 10 poison damage per second for 5 seconds and Poison the target. Red Dragon: In addition to poisoning enemies, attacks damage all enemy units 1 cell away from the target for 50% damage. Blue Dragon: In addition to poisoning enemies, attacks slow movement speed by -30% and attack speed by -30% to enemy units 1 cell away from the target for 3 seconds.
  - Attack range in Elder Dragon form is 3.
  - Splash damage is physical damage and is calculated using the damage the original target would take, pre reductions.
  - The dragon Dragon Knight transforms into is based off of its level.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Elder Dragon Form: Dragon Knight takes the form of one of three powerful elder dragons, increasing his movement speed by 30% and granting him new powers.  Green Dragon: Attacks deal 30 poison damage per second for 5.0 seconds.  Red Dragon: Attacks damage all enemy units 1 cell away from his target for 50% damage, with Corrosive Breath added to the targets.  Blue Dragon: Slows movement speed by -30% and attack speed by -30% of enemy units 1 cell away for 3.0 seconds, with Corrosive Breath added to the targets. (cd 60, mana 100)
- Pre-Big Update 2019-10-10: dragon_knight_breathe_fire: description unknown (cd [10/8/6], mana 100)
- Big Update 2019-10-24: Breathe Fire: Unleashes a breath of fire in front of Dragon Knight that burns enemies for [200/350/500] damage and reduces the damage their attacks deal by [25/40/55] for 6 seconds. (cd [10/8/6], mana 100)
- Outlanders 2019-12-06: Breathe Fire: Unleashes a breath of fire in front of Dragon Knight that burns enemies for [150/250/400] damage and reduces the damage their attacks deal by [25/40/55]% for 6 seconds. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (9 patches)</summary>

- August 27, 2020
  - Elder Dragon Form:
  - Attack damage bonus changed from [50/100/150] to [40/80/120]
- April 30, 2020
  - Maximum Health changed from [1350, 2700, 5400] to [1400, 2800, 5600]
  - Elder Dragon Form:
  - Attacks now apply Poison to the target.
  - Corrosive breath damage per second changed from 30 to 10.
- March 05, 2020
  - Breathe Fire:
  - Cooldown changed from [10, 8, 6] to [7, 6, 5]
  - Duration changed from 6 to 4
  - Mana Cost changed from 100 to 50
- February 25, 2020
  - Tier changed from 4 to 5
  - Attack Damage Minimum changed from [45, 90, 180] to [55, 110, 220]
  - Attack Damage Maximum changed from [55, 110, 230] to [75, 150, 300]
- January 24, 2020
  - Maximum Health changed from [1400, 2800, 5600] to [1350, 2700, 5400]
  - Attack Damage Minimum changed from [50, 100, 200] to [45, 90, 180]
  - Attack Damage Maximum changed from [60, 120, 240] to [55, 110, 230]
  - Breathe Fire:
  - Enemy Attack Damage Reduction changed from [25, 40, 55]% to [25, 35, 45]%
  - Damage changed from [150, 250, 400] to [150, 200, 250]
- January 16, 2020
  - Maximum Health changed from [1500, 3000, 6000] to [1400, 2800, 5600]
- November 21, 2019
  - Breathe Fire:
  - Damage changed from [200, 350, 500] to [150, 250, 400]
- June 26, 2019
  - Elder Dragon Form bonus damage changed from 100/150/200 to 50/100/150.
  - Elder Dragon Form is now marked as passive and cannot be silenced.
  - Now properly aims at the targets in front.
  - Fixed bug that caused Elder Form to take 0.5 seconds to start.
- June 25, 2019
  - Breathe Fire is now his base ability.
  - Dragon Alliance now unlocks Elder Dragon Form
  - Hit Points increased from 800/1600/3200 to 1000/2000/4000.
  - Gained +10 Health Regeneration.

</details>

### Drow Ranger
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Heartless, Vigilant, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Heartless, Hunter | 400/800/1600 | 45-50/90-100/180-200 | 1.3 | 4 | 5 | 0 | 100 | 285 | yes |
| Pre-Big Update 2019-10-10 | T1 | Heartless, Hunter | 400/800/1600 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 | yes |
| Big Update 2019-10-24 | T1 | Heartless, Hunter | 800/1600/3200 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 | yes |
| Outlanders 2019-12-06 | T1 | Heartless, Hunter | 800/1600/3200 | 56-62/112-125/225-250 | 1.2 | 4 | 5 | 0 | 100 | 285 | yes |
| Season One 2020-02-25 | T1 | Heartless, Vigilant, Hunter | 600/1200/2400 | 50-56/100-112/200-224 | 1.2 | 4 | 5 | 0 | 100 | 285 | yes |
| Final 2020-11-19 | T1 | Heartless, Vigilant, Hunter | 600/1200/2400 | 50-56/100-112/200-224 | 1.2 | 4 | 5 | 0 | 100 | 285 | yes |

Abilities (last wiki state):
- **Precision Aura**: Passive: Drow Ranger adds [10/20/30]% bonus to attack speed to all allies 1 cell away.
  - The aura of multiple Drow Rangers do not stack.
- **Drow Ranger 3 Star Effect**: At three stars, Drow Ranger's arrows become Frost Arrows. These arrows slow a hit unit's movement by 50% and its Attack Speed by 20. The effect lingers for 2 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Precision Aura: PASSIVE: Drow Ranger adds a [10/20/30]% bonus to attack speed to all allies 1 cell away. (cd -, mana -)

<details><summary>Wiki change log (5 patches)</summary>

- July 02, 2020
  - 3 Star Effect: Drow Ranger's arrows become Frost Arrows. These arrows slow a hit unit's movement by 50% and its Attack Speed by 20. The effect lingers for 2 seconds.
- April 03, 2020
  - Attack Damage Minimum changed from [56, 112, 225] to [50, 100, 200]
  - Attack Damage Maximum changed from [62, 125, 250] to [56, 112, 224]
- February 25, 2020
  - Alliances changed from Heartless Hunter to Heartless Vigilant Hunter
- September 26, 2019
  - Attack Rate changed from 0.769 to 0.833.
  - Attack Damage Minimum changed from [45, 90, 180] to [56, 112, 225].
  - Attack Damage Maximum changed from [50, 100, 200] to [62, 125, 250].
- June 17, 2019
  - Precision Aura now gives +10/+20/+30 Attack Speed to allies in a 1 cell radius.

</details>

### Earth Spirit
- Status: In the final pool. First seen in data: Outlanders 2019-12-06.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Spirit, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Outlanders 2019-12-06 | T3 | Spirit, Warrior | 1700/3400/6800 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T2 | Spirit, Warrior | 1275/2550/5100 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 | yes |
| Final 2020-11-19 | T2 | Spirit, Warrior | 1275/2550/5100 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Geomagnetic Grip** (cooldown 8.0, mana 100, magical): Earth Spirit summons a Stone Remnant off the map and pulls it towards his current target. Enemies struck by the incoming Stone Remnant are silenced for 3 seconds and take [100/150/200] damage.
  - Will cast even if the target is magic immune.
- **Earth Elemental**: Passive: When Earth Spirit executes a Delta Slam, he silences affected enemy units for 3 seconds.
- **Earth Spirit 3 Star Effect**: Geomagnetic Grip stuns enemies for 1.5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Outlanders 2019-12-06: Geomagnetic Grip: Earth Spirit summons a Stone Remnant off the map and pulls it towards his current target. Enemies struck by the incoming Stone Remnant are silenced for 3 seconds and take [100/150/200] damage. (cd [20/18/16], mana 100) / Earth Elemental: PASSIVE: When Earth Spirit executes a Delta Slam, he silences affected enemy units for 3 seconds. (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- May 21, 2020
  - Geomagnetic Grip:
  - At three stars, units hit by Geomagnetic Grip are also stunned for 1.5 seconds.
- March 19, 2020
  - Geomagnetic Grip:
  - Cooldown changed from [20, 18, 16] to 8
- March 05, 2020
  - Will now cast even if the target is magic immune.
- January 16, 2020
  - Tier changed from 3 to 2

</details>

### Ember Spirit
- Status: In the final pool. First seen in data: Outlanders 2019-12-06.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Spirit, Assassin, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Outlanders 2019-12-06 | T2 | Spirit, Assassin | 1100/2200/4400 | 80-100/160-200/320-400 | 1.5 | 1 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T3 | Spirit, Assassin | 825/1650/3300 | 80-100/160-200/320-400 | 1.5 | 1 | 5 | 0 | 100 | 305 | yes |
| Final 2020-11-19 | T3 | Spirit, Assassin, Swordsman | 825/1650/3960 | 80-100/160-200/384-480 | 1.5 | 1 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Sleight of Fist** (cooldown 10, mana 100, physical): Ember Spirit dashes around with blazing speed, attacking all enemies within [2/2/3] cells, then returning to his start location. Attacks deal [100/150/200] bonus damage.
- **Fire Elemental**: Passive: When Ember Spirit executes a Delta Slam, he sets affected enemy units on fire for 3 seconds.
- **Ember Spirit 3 Star Effect**: Sleight of Fist's area of effect radius is increased.

Ability text rendered from beta game data (shown when it changed):
- Outlanders 2019-12-06: Sleight of Fist: Ember Spirit dashes around with blazing speed, attacking all enemies within [2/2/3] cells, then returning to his start location. Attacks deal [100/200/300] bonus damage. (cd 10, mana 100) / Fire Elemental: PASSIVE: When Ember Spirit executes a Delta Slam, he sets affected enemy units on fire for 3 seconds. (cd -, mana -)

<details><summary>Wiki change log (3 patches)</summary>

- August 27, 2020
  - Now also a Swordsman
  - Health changed from [825/1,650/3,300] to [825/1,650/3,960]
  - Attack Damage Minimum changed from [80, 160, 320] to [80, 160, 384]
  - Attack Damage Maximum changed from [100, 200, 400] to [100, 200, 480]
- January 16, 2020
  - Tier changed from 2 to 3
- January 09, 2020
  - Sleight of Fist:
  - Bonus Damage changed from [100, 200, 300] to [100, 150, 200]

</details>

### Enchantress
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Shaman, Healer.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Savage (beta version), Druid | 400/800/1500 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 | yes |
| Pre-Big Update 2019-10-10 | T1 | Savage, Druid | 400/800/1500 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 | yes |
| Big Update 2019-10-24 | T1 | Savage, Druid, Healer | 800/1600/3000 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T1 | Savage, Druid, Healer | 800/1600/3000 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T1 | Savage, Druid, Healer | 600/1200/2250 | 60-70/110-130/210-250 | 1.5 | 2 | 5 | 0 | 100 | 315 | no |
| Final 2020-11-19 | T1 | Shaman, Healer | 700/1400/2800 | 60-70/120-140/240-280 | 1.5 | 2 | 5 | 0 | 100 | 315 | yes |

Abilities (last wiki state):
- **Nature's Attendants** (cooldown 15.00, mana 100): Enchantress summons a cloud of [3/5/7] wisps that heal all allies up to 4 cells away. For 11 seconds, allies heal 40 health every 1.5 seconds.
  - Effect continues even when Enchantress dies.
- **Shaman Alliance Bonus**: Each time it is attacked the Shaman Alliance creature has a [10/25/40]% chance to slow the attacker's Attack Speed by 80 for the duration of its attack.
- **Enchantress 3 Star Effect**: Enchantress becomes Untouchable. Each time an enemy attacks her she has a 50% chance of slowing their attack speed by 80 for the duration of their attack.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Nature's Attendants: Enchantress summons a cloud of [3/5/7] wisps that heal all allies up to 4 cells away.  For 11 seconds, allies heal 25 health every 1.5 seconds. (cd 15, mana 100)
- Pre-Big Update 2019-10-10: Nature's Attendants: Enchantress summons a cloud of [3/5/7] wisps that heal all allies up to 4 cells away.  For 11 seconds, allies heal 40 health every 1.5 seconds. (cd 15, mana 100)

<details><summary>Wiki change log (2 patches)</summary>

- August 27, 2020
  - Alliance changed from Savage Druid Healer to Shaman Healer
  - Health changed from [600/1,200/2,250] to [700/1,400/2,800]
  - Attack Damage Minimum changed from [60/110/210] to [60/120/240]
  - Attack Damage Maximum changed from [70/130/250] to [70/140/280]
  - 3 Star Effect: Enchantress gains Untouchable. When attacked, Enchantress has a chance to greatly slow her attacker's Attack Speed.
- October 24, 2019
  - Alliances changed from Savage Druid to Savage Druid Healer

</details>

### Enigma
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Void, Primordial.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T5 | Primordial (beta version), Warlock | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T5 | Primordial, Shaman (old, beta) | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 | yes |
| Big Update 2019-10-24 | T5 | Primordial, Shaman (old, beta) | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T5 | Primordial, Shaman (old, beta) | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 3 | 5 | 40 | 100 | 290 | yes |
| Season One 2020-02-25 | T3 | Void, Primordial | 900/1800/3600 | 35-50/70-100/140-200 | 1.5 | 3 | 5 | 40 | 100 | 290 | yes |
| Final 2020-11-19 | T3 | Void, Primordial | 900/1800/3600 | 35-50/70-100/140-200 | 1.5 | 3 | 5 | 40 | 100 | 290 | no |

Abilities (last wiki state):
- **Midnight Pulse** (cooldown 20.00, mana 100, pure): Enigma steeps an area underneath the enemy with the highest max health in dark resonance, dealing [5/6/6]% of an enemy's max health for [5/7/9] seconds to all enemy units up to [1/1/2] cell away from the target and slowing them by 80%.
- **Enigma 3 Star Effect**: Midnight Pulse's area of effect is tripled.
- **Ace effect (2019-08-30 to 2020-02-25, Primordial Ace):** Allied eidolons cast Malefice on their killer, stunning them for 0.4s every 2 seconds. The effect lasts 2 seconds per Alliance level. The target takes 50 damage per Alliance level each time they are stunned.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Midnight Pulse: Enigma steeps an area in dark resonance, dealing [5/7/9]% of an enemy's max health for 10 seconds to all enemy units up to [2/3/4] cells away from the target. (cd 20, mana 100)
- Pre-Big Update 2019-10-10: Midnight Pulse: Enigma steeps an area in dark resonance, dealing [5/7/9]% of an enemy's max health for 10 seconds to all enemy units up to [1/2/3] cells away from the target. (cd 20, mana 100) / Ace/alliance bonus (internal id enigma_shaman_bonus): see the Ace effect entry (cd -, mana -)
- Big Update 2019-10-24: Midnight Pulse: Enigma steeps an area in dark resonance, dealing [2.5/3.5/4.5]% of an enemy's max health for 10 seconds to all enemy units up to [1/2/3] cells away from the target. (cd 20, mana 100) / Ace/alliance bonus (internal id enigma_primordial_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (10 patches)</summary>

- May 21, 2020
  - Midnight Pulse:
  - Damage changed from [5, 6, 7]% to [5, 6, 6]%
- May 08, 2020
  - Midnight Pulse:
  - At 3 stars, Midnight Pulse's radius is 2 cells rather than 1.
  - Damage changed from [4, 5, 6]% to [5, 6, 7]%.
- February 25, 2020
  - Ace effect removed.
  - Tier changed from Ace to 3
  - Alliances changed from Primordial Shaman to Void Primordial
  - Maximum Health changed from [1500, 3000, 6000] to [900, 1800, 3600]
  - Attack Damage Minimum changed from [45, 90, 180] to [35, 70, 140]
  - Attack Damage Maximum changed from [55, 110, 220] to [50, 100, 200]
  - Midnight Pulse:
  - Enigma will always target the enemy with the highest Maximum Health
  - Additionally slows enemies in the affected are by 80%
  - Max Health Damage Percent per Second changed from [2.5, 3.5, 4.5] to [4, 5, 6]
  - Duration changed from 10 to [5, 7, 9]
  - Radius changed from [1, 2, 3] to 1 (Midnight Pulse now always covers a 3x3 area)
- December 18, 2019
  - Removed from the game.
- October 24, 2019
  - Enigma changed from the ACE OF SHAMANS to the ACE OF PRIMORDIALS
  - Old Ace Effect: Enemies affected by Hexes, Silences or Stuns will not generate mana when attacked. +15% chance of finding if you have at least one level of the Shaman Alliance completed.
  - New Ace Effect: Allied eidolons cast Malefice on their killer, stunning them for 0.4s every 2 seconds. The effect lasts 2 seconds per Primordial Alliance level. The target takes 50 damage per Primordial Alliance level each time they are stunned. +15% chance of finding if you have at least one level of the Primordial Alliance completed.
  - Midnight Pulse:
  - Damage changed from [5%, 7%, 9%] to [2.5%, 3.5%, 4.5%]
- September 12, 2019
  - Ace tier Effect:
  - Now prevents gaining mana from attacks while Hexed, Silenced or Stunned, instead of draining it.
- September 02, 2019
  - Ace tier Effect:
  - Now: Enemies affected by Hexes, Silences or Stuns will generate -200% mana. (Was: Extends the effects of the Shaman Alliance to your entire army.)
- August 30, 2019
  - Ace for the Shaman Alliance.
  - Ace Effect: Extends the effects of the Shaman Alliance to your entire army.
- July 19, 2019
  - Alliance changed from Primordial Warlock to Primordial Shaman
  - Midnight Pulse:
  - Radius changed from [2, 3, 4] to [1, 2, 3]
- June 27, 2019
  - Midnight pulse updated to be square shaped.

</details>

### Faceless Void
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Void, Assassin.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T5 | Primordial, Assassin | 2000/4000/8000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T5 | Primordial, Assassin | 2000/3600/8000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T5 | Void, Assassin | 1500/2700/6000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T5 | Void, Assassin | 1500/3000/6000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 | yes |

Abilities (last wiki state):
- **Chronocube** (cooldown 60.0, mana 100): Creates a blister in spacetime, stunning all units caught 2 cells away from the center of the cube for [2/3/4]s. Faceless Void moves very quickly inside the cube and gains 75 attack speed. Only Faceless Void is unaffected. When cast, Faceless Void attempts to teleport all friendly Assassins out of the cube.
  - Faceless Void is not affected by any Chronocube on the board.
- **Ace effect (2019-08-30 to 2020-02-25, Assassin Ace):** Allied Assassins have a 5% chance per Assassin Alliance level to dodge attacks by jumping backward in time, fully avoiding both physical and magical damage.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Chronocube: Creates a blister in spacetime, stunning all units caught 2 cells away from the center of the cube for [4/5/6]s. Faceless Void moves very quickly inside the cube and gains 50 attack speed. Only Faceless Void is unaffected. (cd [15/12/9], mana 100) / Ace/alliance bonus (internal id faceless_void_assassin_bonus): see the Ace effect entry (cd -, mana -)
- Outlanders 2019-12-06: Chronocube: Creates a blister in spacetime, stunning all units caught 2 cells away from the center of the cube for [2/3/4]s. Faceless Void moves very quickly inside the cube and gains 75 attack speed. Only Faceless Void is unaffected. When cast, Faceless Void attempts to teleport all friendly Assassins out of the cube. (cd 60, mana 100) / Ace/alliance bonus (internal id faceless_void_assassin_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Health changed from [1,500/2,700/6,000] to [1,500/3,000/6,000]
- February 25, 2020
  - Ace effect removed.
  - Tier changed from Ace to 5.
  - Alliances changed from Primordial Assassin to Void Assassin
- November 21, 2019
  - Maximum Health changed from [2000, 4000, 8000] to [2000, 3600, 8000]
- November 14, 2019
  - Chronocube:
  - Effect duration changed from [4, 5, 6] to [2, 3, 4]
  - Bonus attack speed to Faceless Void when inside the cube changed from 50 to 75
- October 31, 2019
  - Chronocube:
  - Cooldown changed [15, 12, 9] -> 60.

</details>

### Gyrocopter
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 5, Deadeye, Inventor.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T5 | Deadeye, Inventor (beta version) | 900/1800/3600 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 | yes |
| Pre-Big Update 2019-10-10 | T5 | Deadeye, Inventor | 900/1800/3600 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 | yes |
| Big Update 2019-10-24 | T5 | Deadeye, Inventor | 1800/3600/7200 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T5 | Deadeye, Inventor | 1800/3600/7200 | 75-80/150-160/300-320 | 0.9 | 6 | 15 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T5 | Deadeye, Inventor | 1350/2700/5400 | 65-75/130-150/260-300 | 0.9 | 6 | 15 | 0 | 100 | 315 | no |
| Final 2020-11-19 | T5 | Deadeye, Inventor | 1350/2700/5400 | 65-75/130-150/260-300 | 0.9 | 6 | 15 | 0 | 100 | 315 | no |

Abilities (last wiki state):
- **Call down** (cooldown 10.00, mana 100, magical): Gyrocopter launches a missile strike, hitting all enemies up to 3 cells away from point of impact. Two missiles arrive in succession, the first dealing [200/300/400] damage and 30% slow for 2 seconds; the final one deals [200/300/400] damage and 60% slow for 4 seconds.
  - Gyrocopter centres Call down on a random enemy.
- **Ace effect (2019-08-30 to 2020-02-25, Deadeye Ace):** Attacks from Deadeye units pierce their main target, dealing 75% damage per Deadeye Alliance level to the unit behind them.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Call Down: Gyrocopter launches a missile strike, hitting all enemies up to 3 cells away from point of impact.  Two missiles arrive in succession, the first dealing [200/300/400] damage and 30% slow for 2 seconds; the final one deals [200/300/400] damage and 60% slow for 4 seconds. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Call Down: Gyrocopter launches a missile strike, hitting all enemies up to 3 cells away from point of impact.  Two missiles arrive in succession, the first dealing [200/300/400] damage and 30% slow for 2 seconds; the final one deals [200/300/400] damage and 60% slow for 4 seconds. (cd 10, mana 100) / Ace/alliance bonus (internal id gyrocopter_deadeye_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- January 24, 2020
  - Attack Damage Minimum changed from [75, 150, 300] to [65, 130, 260]
  - Attack Damage Maximum changed from [80, 160, 320] to [75, 150, 300]
  - Ace tier Effect:
  - Pierce percent changed from 100 to 75
- October 24, 2019
  - Old Ace Effect: Grants Deadeye units True Strike. Units with True Strike cannot miss. +15% chance of finding if you have at least one level of the Deadeye Alliance completed.
  - New Ace Effect: Attacks from Deadeye units pierce their main target, dealing 100% damage per Deadeye Alliance level to the unit behind them. +15% chance of finding if you have at least one level of the Deadeye Alliance completed.
- August 30, 2019
  - Ace for the Deadeye Alliance.
  - Ace Effect: Grants Deadeye units True Strike. Units with True Strike cannot miss.
- July 12, 2019
  - No longer targets units in the air when casting Call Down.

</details>

### Io
- Status: Rotated out at the end (still in final data). First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Primordial, Druid.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T3 | Primordial, Druid | 1100/2200/4000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 | yes |
| Outlanders 2019-12-06 | T3 | Primordial, Druid | 1100/2200/4000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 | yes |
| Season One 2020-02-25 | T3 | Primordial, Druid | 800/1600/3000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 | yes |
| Final 2020-11-19 | T3 | Primordial, Druid | 800/1600/3000 | 40-45/80-90/160-180 | 1.7 | 3 | 5 | 0 | 100 | 275 | no |

Abilities (last wiki state):
- **Tether** (cooldown 2): Io tethers to the nearest allied hero and pulls itself towards it. Io and the tethered hero gain [30/50/80] attack speed and [5/8/12]% damage reduction. The tether will break if Io is more than 4 cells away.
  - Io has a fixed pattern for choosing which one to tether to, depending on who hosts the fight:
  - If you host the fight, the priority is right -> left -> down -> top.
  - If the opponent hosts, it's top -> right -> left -> down.
  - Break will not snap the tether.
  - The tether's target will not receive benefits while broken.
- **Relocate** (cooldown 60, mana 100): Io teleports away for 4.0 seconds to revive a dead hero. The two units return with full health and are purged of negative effects.
  - The revived hero is chosen completely randomly.
  - Revived units keep their existing cooldowns.
- **Io 3 Star Effect**: Relocate resets the revived hero's cooldowns.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Tether: Io tethers to the nearest allied unit and pulls itself towards it. Io and the tethered unit gain [50/80/110] attack speed and [10/20/30]% damage reduction. The tether will break if Io is more than 4 cells away. (cd 2, mana -) / Relocate: Io teleports away for 4 seconds to revive a dead hero. The two units return with full health and are purged of negative effects. (cd 60, mana 100)
- Outlanders 2019-12-06: Tether: Io tethers to the nearest allied hero and pulls itself towards it. Io and the tethered hero gain [50/80/110] attack speed and [10/20/30]% damage reduction. The tether will break if Io is more than 4 cells away. (cd 2, mana -) / Relocate: Io teleports away for 4 seconds to revive a dead hero. The two units return with full health and are purged of negative effects. (cd 60, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- July 02, 2020
  - Fixed an issue with Io's 3 Star Effect sometimes triggering when it shouldn't.
  - Fixed a longstanding interaction issue between Aegis of the Immortal and Io's Relocate which could lead to two revived copies of a Hero existing at once. Now Io will only consider selecting an Aegis-carrying Hero for Relocate if that Hero has already died its second time (i.e. been revived by Aegis and killed again).
- May 14, 2020
  - Maximum Health changed from [900, 1800, 3300] to [800, 1600, 3000]
  - Tether:
  - Fixed a major issue where Tether would increase, rather than reduce, damage to Io and the target unit. Damage taken by Io and tethered target changed from [+10, +20, +30]% to [-5, -8, -12]%
  - Attack Speed buff changed from [50, 80, 110] to [30, 50, 80]
  - Relocate:
  - At three stars, the revived hero also has their cooldowns reset.
- April 30, 2020
  - Maximum Health changed from [825, 1650, 3000] to [900, 1800, 3300]
- March 19, 2020
  - Tether:
  - Tethers no longer snap when Io is broken. However, the tether target will not gain any benefit from the tether if the target itself is broken.
- January 24, 2020
  - Fixed a bug where Refresher Orb would not refresh Relocate
- November 14, 2019
  - Relocate:
  - Units keep their existing cooldowns when brought back.

</details>

### Juggernaut
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Brawny, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Brawny (beta version), Warrior | 600/1200/2400 | 65-70/130-140/260-280 | 1.1 | 1 | 5 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T3 | Brawny, Warrior | 600/1200/2400 | 65-70/140-150/280-300 | 1.1/1.0/0.8 | 1 | 5 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T3 | Brawny, Warrior | 1200/2400/4800 | 65-70/140-150/280-300 | 1.1/1.0/0.8 | 1 | 5 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T3 | Brawny, Warrior | 1200/2600/4800 | 65-70/155-165/280-300 | 1.0/0.9/0.8 | 1 | 5 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T3 | Brawny, Warrior | 900/1950/3600 | 65-70/155-165/280-300 | 1/0.9/0.8 | 1 | 5 | 0 | 100 | 300 | yes |
| Final 2020-11-19 | T2 | Brawny, Swordsman | 900/1800/3600 | 60-75/120-150/240-300 | 1/0.9/0.8 | 1 | 5 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Blade Fury** (cooldown 12.00, mana 100, magical): Juggernaut spins with his blade, rendering him immune to magic and dealing [50/100/150] damage per second to enemy units 1 cell away for 5 seconds. Juggernaut may also start spinning to avoid a magical attack.
  - Juggernaut continues to attack the target normally while spinning.
  - The damage from Blade Fury will not generate mana to targets if Juggernaut has Diffusal Blade.
- **Juggernaut 3 Star Effect**: While Juggernaut is alive, all allies heroes gains 15% of Juggernaut's Max HP.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Blade Fury: Juggernaut spins with his blade, rendering him immune to magic and dealing [50/100/150] damage per second to enemy units 1 cell away for 5 seconds. (cd 12, mana 100)
- Big Update 2019-10-24: Blade Fury: Juggernaut spins with his blade, rendering him immune to magic and dealing [50/100/150] damage per second to enemy units 1 cell away for 5 seconds. Juggernaut may also start spinning to avoid a magical attack. (cd 12, mana 100)

<details><summary>Wiki change log (9 patches)</summary>

- August 27, 2020
  - Changed from Tier 3 Brawny Warrior to Tier 2 Brawny Swordsman.
  - Health changed from [900/1,950/3,600] to [900/1,800/3,600]
  - Attack Damage Minimum changed from [65, 155, 280] to [60, 120, 240]
  - Attack Damage Maximum changed from [70, 165, 300] to [75, 150, 300]
  - 3 Star Effect: While Juggernaut is alive, all allied heroes gain 15% of Juggernaut's Max HP.
- June 18, 2020
  - At three stars, Juggernaut gains Blade Dance: each attack has a 20% chance to Critical Hit for 200% Damage.
- November 21, 2019
  - Attack Rate changed from [0.909, 1.0, 1.25] to [1.0, 1.111, 1.25]
  - Maximum Health changed from [1200, 2400, 4800] to [1200, 2600, 4800]
  - Attack Damage Minimum changed from [65, 140, 280] to [65, 155, 280]
  - Attack Damage Maximum changed from [70, 150, 300] to [70, 165, 300]
- August 23, 2019
  - Now Tier 3.
  - Blade Fury now activates if there is at least 1 enemy in range (was 2 enemies).
- August 15, 2019
  - Blade Fury:
  - Juggernaut continues to attack his current target normally while spinning.
  - Properly purges stuns, evens those applied by the hit that triggered Blade Fury.
- August 08, 2019
  - If Juggernaut is eligible to cast Blade Fury (has enough mana, not on cooldown) and he is about to take magic damage, he immediately casts Blade Fury and takes no magic damage.
  - Juggernaut no longer spins while silenced.
  - Juggernaut no longer enters T-pose while spinning.
- August 01, 2019
  - Juggernaut will only cast Blade Fury when he's surrounded by at least 2 enemies.
- July 24, 2019
  - Juggernaut Attack Speed changed from 0.91 to [0.91, 1.0, 1.25]
  - Juggernaut Min Damage changed from [65, 130, 260] to [65, 140, 280]
  - Juggernaut Max Damage changed from [70, 140, 280] to [70, 150, 300]
- June 27, 2019
  - Blade Fury purges on activation.

</details>

### Keeper of the Light
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Human, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Human, Mage | 650/1300/2600 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 | yes |
| Pre-Big Update 2019-10-10 | T4 | Human, Mage | 550/1100/2200 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 150 | 330 | yes |
| Big Update 2019-10-24 | T4 | Human, Mage | 1100/2200/4400 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 | yes |
| Outlanders 2019-12-06 | T4 | Human, Mage | 1100/2200/4400 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 | yes |
| Season One 2020-02-25 | T4 | Human, Mage | 825/1650/3300 | 40-45/80-90/160-180 | 1.7 | 4 | 5 | 0 | 100 | 330 | yes |
| Final 2020-11-19 | T5 | Human, Mage | 1100/2200/4400 | 55-60/110-120/220-240 | 1.7 | 4 | 5 | 0 | 100 | 330 | yes |

Abilities (last wiki state):
- **Illuminate** (cooldown [8.00/6.00/4.00], mana 100, magical): The Keeper of the Light channels energy, building power the longer it's channeled. Once released, a wave is sent forth that deals [175/225/275] damage per second channeled.
  - Illuminate has a maximum channel duration of 4 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Illuminate: The Keeper of the Light channels energy, building power the longer it's channeled.  Once released, a wave is sent forth that deals [150/200/250] damage per second channeled. (cd [10/8/6], mana 100)
- Pre-Big Update 2019-10-10: Illuminate: The Keeper of the Light channels energy, building power the longer it's channeled.  Once released, a wave is sent forth that deals [150/200/250] damage per second channeled. (cd [10/8/6], mana 150)
- Big Update 2019-10-24: Illuminate: The Keeper of the Light channels energy, building power the longer it's channeled.  Once released, a wave is sent forth that deals [150/200/250] damage per second channeled. (cd [10/8/6], mana 100)
- Outlanders 2019-12-06: Illuminate: The Keeper of the Light channels energy, building power the longer it's channeled.  Once released, a wave is sent forth that deals [100/150/200] damage per second channeled. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- August 27, 2020
  - Tier changed from 4 to 5
  - Health changed from [825/1,650/3,300] to [1,100/2,200/4,400]
  - Attack Damage Minimum changed from [40, 80, 160] to [55, 110, 220]
  - Attack Damage Maximum changed from [45, 90, 180] to [60, 120, 240]
  - Illuminate:
  - Damage per second channeling changed from [100, 150, 200] to [175, 225, 275]
  - Cooldown changed from [10.00/8.00/6.00] to [8.00/6.00/4.00] seconds
- November 14, 2019
  - Illuminate
  - Channel time changed from 3 to 4
  - Damage per second channeling changed from [150, 200, 250] to [100, 150, 200]
- October 24, 2019
  - Maximum Mana changed from 150 to 100
  - Illuminate:
  - Mana Cost changed from 150 to 100
- June 25, 2019
  - Illuminate now costs 150 mana.
  - Health decreased to 550/1100/2200.

</details>

### Kunkka
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Human, Warrior, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Human, Warrior | 950/1900/3800 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T4 | Human, Warrior | 950/1900/3800 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T4 | Human, Warrior | 1900/3800/7600 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T4 | Human, Warrior | 1900/3800/7600 | 75-90/150-180/300-360 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T4 | Human, Warrior | 1500/3000/6000 | 90-110/180-220/360-440 | 1.4 | 1 | 10 | 0 | 100 | 300 | no |
| Final 2020-11-19 | T2 | Human, Warrior, Swordsman | 1200/2400/4800 | 65-85/130-170/260-340 | 1.4 | 1 | 5 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Ghostship** (cooldown 15.00, mana 100, magical): Kunkka summons a ghostly ship that sails through the battle before smashing apart, dealing [50/125/250] damage and stunning all enemies up to [2/2/3] cells away from the wreckage for [1.0/1.3/1.7] seconds.
  - Kunkka centers Ghostship on a random enemy.
- **Kunkka 3 Star Effect** (physical): Kunkka gains Tidebringer. Every 4 attacks, Kunkka takes a mighty swing and deals 150 bonus damage to his target and cleaves for 75%.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Ghostship: Kunkka summons a ghostly ship that sails through the battle before smashing apart, dealing [150/250/350] damage and stunning all enemies up to [2/3/4] cells away from the wreckage for [1.4/1.7/2] seconds. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Ghostship: Kunkka summons a ghostly ship that sails through the battle before smashing apart, dealing [50/150/250] damage and stunning all enemies up to [2/2/3] cells away from the wreckage for [1.4/1.7/2] seconds. (cd 15, mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- August 27, 2020
  - Tier changed from 4 to 2. Now has Human Warrior Swordsman alliances.
  - Maximum Health changed from [1,500/3,000/6,000] to [1,200/2,400/4,800]
  - Attack Damage Minimum changed from [90, 180, 360] to [65, 130, 260]
  - Attack Damage Maximum changed from [110, 220, 440] to [85, 170, 340]
  - Armor reduced from 10 to 5
  - Ghostship:
  - Damage changed from [50, 150, 250] to [50, 125, 250]
  - Stun duration changed from [1.4/1.7/2] to [1.0/1.3/1.7] seconds.
- January 09, 2020
  - Maximum Health changed from [1425, 2850, 5700] to [1500, 3000, 6000]
  - Attack Damage Minimum changed from [75, 150, 300] to [90, 180, 360]
  - Attack Damage Maximum changed from [90, 180, 360] to [110, 220, 440]
- July 19, 2019
  - Ghostship:
  - Damage changed from [150, 250, 350] to [50, 150, 250]
  - Cooldown changed from 10 to 15
- June 27, 2019
  - Ghostship crash cell size changed: [2, 3, 4] -> [2, 2, 3].

</details>

### Legion Commander
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Human, Champion.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T3 | Human, Champion | 1000/2000/4800 | 50-60/100-120/240-288 | 1.5 | 1 | 10 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T3 | Human, Champion | 900/1800/3600 | 40-50/80-100/180-228 | 1.25 | 1 | 5 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T2 | Human, Champion | 675/1150/2300 | 60-75/96-120/184-240 | 1.25 | 1 | 5 | 0 | 100 | 315 | yes |
| Final 2020-11-19 | T2 | Human, Champion | 600/1200/2400 | 50-60/100-120/200-240 | 1.25 | 1 | 5 | 0 | 100 | 315 | yes |

Abilities (last wiki state):
- **Duel** (cooldown 40, mana 100): Legion Commander targets the lowest health enemy 1 cell away and they are forced to attack each other for [4.0/4.75/5.5]s. Neither hero can use abilities nor be healed. If either hero dies during the duration, the hero winning the Duel gains permanent [5/8/12] bonus damage. ([20/32/48] bonus damage in Knockout)
- **Elemental Charge**: Passive: When Legion Commander executes a Delta Slam as part of the Spirit Alliance, she deals double damage.
- **Legion Commander 3 Star Effect**: At the start of a Duel, any enemies outside the Duel currently targeting Legion Commander will pick other targets for their regular attacks.
- **Will of the Bullsnake**: Unlocked by the Dragon alliance bonus Passive: When Legion Commander wins a Duel she instantly heals for 30% of her maximum health.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Duel: Legion Commander targets the lowest health enemy 1 cell away and they are forced to attack each other for [4.0/4.75/5.5]s. Neither hero can use abilities. If either hero dies during the duration, the hero winning the Duel gains permanent [5/10/15] bonus damage. (cd 40, mana 100)
- Outlanders 2019-12-06: Duel: Legion Commander targets the lowest health enemy 1 cell away and they are forced to attack each other for [4.0/4.75/5.5]s. Neither hero can use abilities. If either hero dies during the duration, the hero winning the Duel gains permanent [5/10/15] bonus damage. (cd 40, mana 100) / Elemental Charge: PASSIVE: When Legion Commander executes a Delta Slam as part of the Spirit Alliance, she deals double damage. (cd -, mana -)

<details><summary>Wiki change log (9 patches)</summary>

- August 27, 2020
  - Maximum Health changed from [675/1,150/2,300] to [600/1,200/2,400]
  - Attack Damage Minimum changed from [60, 96, 184] to [50, 100, 200]
  - Attack Damage Maximum changed from [75, 120, 240] to [60, 120, 240]
  - 3 Star Effect: At the start of a Duel, any units targeting Legion Commander (other than her Duel partner) will pick other targets.
- August 06, 2020
  - 3 Star Effect: Allies within attack range of Legion Commander's Duel target will attack that target along with her during the Duel.
- April 23, 2020
  - Attack Damage Minimum changed from [40, 80, 160] to [60, 96, 184]
  - Attack Damage Maximum changed from [50, 100, 200] to [75, 120, 240]
  - Legion Commander no longer receives a buff when a Blood-bound ally dies, but still buffs all Blood-bound allies when she dies.
- April 10, 2020
  - Fixed an issue where Legion Commander could still heal from the last hit of a duel.
  - Maximum Health changed from [675, 1350, 2700] to [675, 1150, 2300]
  - Attack Damage Minimum changed from [40, 80, 160] to [40, 60, 120]
  - Attack Damage Maximum changed from [50, 100, 200] to [50, 75, 150]
- March 19, 2020
  - Duel:
  - Both Legion Commander and her target cannot receive healing while the Duel is active.
- January 24, 2020
  - Attack Damage Minimum changed from [40, 80, 180] to [40, 80, 160]
  - Attack Damage Maximum changed from [50, 100, 228] to [50, 100, 200]
  - Duel:
  - Bonus Damage per duel win changed from [5, 10, 15] to [5, 8, 12]
- December 18, 2019
  - Gold Cost changed from 3 to 2
- November 21, 2019
  - Armor changed from 10 to 5
  - Maximum Health changed from [1000, 2000, 4800] to [900, 1800, 3600]
  - Attack Damage Minimum changed from [50, 100, 240] to [40, 80, 180]
  - Attack Damage Maximum changed from [60, 120, 288] to [50, 100, 228]
- October 31, 2019
  - Attack Rate changed 0.667 -> 0.8.

</details>

### Lich
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Fallen, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T5 | Heartless, Mage | 850/1700/3400 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |
| Pre-Big Update 2019-10-10 | T5 | Heartless, Mage | 850/1700/3400 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |
| Big Update 2019-10-24 | T5 | Heartless, Mage | 1700/3400/6800 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |
| Outlanders 2019-12-06 | T5 | Heartless, Mage | 1700/3400/6800 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |
| Season One 2020-02-25 | T5 | Heartless, Mage | 1275/2550/5100 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |
| Final 2020-11-19 | T1 | Fallen, Mage | 750/1500/3000 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | yes |

Abilities (last wiki state):
- **Frost Shield** (cooldown 20.00, mana 50, magical): Lich applies a shield of ice to a random ally, reducing damage from attacks by [10/20/30]. While the shield is active, enemy units within 2 cell will take [20/40/60] damage per second and lose 50% Movement Speed and 25 Attack Speed. The shield lasts 7 seconds.
- **Lich 3 Star Effect** (cooldown 30.00, mana 100, magical): Lich gains Chain Frost. Lich releases an orb of frost that bounces between nearby enemy units up to 7 times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing 200 damage each time it hits.
  - Has a bounce range of 3.
- **Ace effect (2019-08-30 to 2020-02-25, Mage Ace):** Your army's mana generation from attacks is increased 10% per Mage Alliance level.

Older abilities (wiki):
- **Chain Frost** (cooldown [15.00/13.00/10.00], mana 100): Lich releases an orb of frost that bounces between nearby enemy units up to [7/10/13] times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing [200/300/400] magical damage each time it hits.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Chain Frost: Lich releases an orb of frost that bounces between nearby enemy units up to [6/9/12] times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing [200/300/400] damage each time it hits. (cd [10/8/6], mana 100)
- Pre-Big Update 2019-10-10: Chain Frost: Lich releases an orb of frost that bounces between nearby enemy units up to [6/9/12] times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing [200/300/400] damage each time it hits. (cd [10/8/6], mana 100) / Ace/alliance bonus (internal id lich_mage_bonus): see the Ace effect entry (cd -, mana -)
- Outlanders 2019-12-06: Chain Frost: Lich releases an orb of frost that bounces between nearby enemy units up to [6/9/12] times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing [200/300/400] damage each time it hits. (cd [15/13/10], mana 100) / Ace/alliance bonus (internal id lich_mage_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Tier changed from 5 to 1.
  - Alliance changed from Heartless Mage to Fallen Mage
  - Health changed from [1,275/2,550/5,100] to [750/1,500/3,000]
  - Ability: Frost Shield: Lich applies a shield of ice to a random ally, reducing damage from attacks. While the shield is active, enemy units nearby take damage and lose Movement Speed and Attack Speed.
  - Chain Frost is now unlocked by Lich's 3 Star Effect as a second ability.
- February 28, 2020
  - Chain Frost:
  - Bounces changed from [6, 9, 12] to [7, 10, 13]
  - Bounce Range changed from 2 to 3
- February 25, 2020
  - Ace effect removed.
  - Tier changed from Ace to 5.
- November 27, 2019
  - Chain Frost:
  - Cooldown changed from [10, 8, 6] to [15, 13, 10]
- September 19, 2019
  - Chain Frost:
  - Bounce range changed to 2 (was 4).
- August 30, 2019
  - Ace for the Mage Alliance.
  - Ace Effect: Your army's mana generation from attacks is increased 10% per Mage Alliance level.

</details>

### Lifestealer
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Heartless, Brute.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T3 | Heartless, Brute | 1350/2700/6480 | 55-60/110-120/220-240 | 1.7/1.6/1.5 | 1 | 5 | 0 | 0 | 300 | yes |
| Outlanders 2019-12-06 | T3 | Heartless, Brute | 1350/2700/6480 | 55-60/110-120/220-240 | 1.5/1.4/1.3 | 1 | 5 | 0 | 0 | 300 | yes |
| Season One 2020-02-25 | T3 | Heartless, Brute | 1010/2025/4860 | 55-60/110-120/220-240 | 1.5/1.4/1.3 | 1 | 5 | 0 | 0 | 300 | yes |
| Final 2020-11-19 | T3 | Heartless, Brute | 1000/2000/4800 | 55-60/110-120/264-288 | 1.5/1.4/1.3 | 1 | 5 | 0 | 75 | 300 | yes |

Abilities (last wiki state):
- **Feast** (physical): Passive: Lifestealer's attacks damage enemies and heal himself for [4.5/6/7]% of his target's max health.
- **Lifestealer 3 Star Effect** (cooldown 18.00, mana 75): Lifestealer gains Rage. On cast Lifestealer becomes Magic Immune and gains +30% Move Speed and +30 Attack Speed. The effects last for 5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Feast: PASSIVE: Lifestealer's attacks damage enemies and heal himself for [3/4.5/6]% of his target's max health. (cd -, mana -)
- Outlanders 2019-12-06: Feast: PASSIVE: Lifestealer's attacks damage enemies and heal himself for [4.5/6/7.5]% of his target's max health. (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- November 19, 2020
  - Feast:
  - Percent of target HP per hit changed from [4.5, 6, 7.5] to [4.5, 6, 7]
  - 3 Star Effect:
  - Bonus Attack Speed changed from 50 to 30
  - Bonus Movement Speed Percent changed from 50 to 30
- August 27, 2020
  - Health changed from [1,010/2,025/4,860] to [1,000/2,000/4,800]
  - Attack Damage Minimum changed from [55, 110, 220] to [55, 110, 264]
  - Attack Damage Maximum changed from [60, 110, 220] to [60, 120, 288]
- August 06, 2020
  - 3 Star Effect: Lifestealer gains Rage. On cast Lifestealer becomes Magic Immune and gains +50% Move Speed and +50 Attack Speed. The effects last for 5 seconds.
  - Maximum Mana changed from 0 to 75
- November 07, 2019
  - Attack Rate changed from [0.588, 0.625, 0.667] to [0.667, 0.714, 0.769].
  - Feast:
  - Lifesteal percent changed from [3, 4.5, 6]% to [4.5, 6, 7.5]%.

</details>

### Lina
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Human, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Human, Mage | 550/1100/2200 | 50-55/100-110/200-220 | 1.6 | 4 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Human, Mage | 550/1100/2200 | 50-55/100-110/200-220 | 1.6 | 4 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T3 | Human, Mage | 1200/2400/4800 | 50-55/100-110/200-220 | 1.4 | 4 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T3 | Human, Mage | 1200/2400/4800 | 50-55/100-110/200-220 | 1.4 | 4 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T3 | Human, Mage | 900/1800/3600 | 50-55/100-110/200-220 | 1.4 | 4 | 5 | 0 | 100 | 290 | no |
| Final 2020-11-19 | T4 | Human, Mage | 1000/2000/4000 | 65-70/130-140/260-280 | 1.4 | 4 | 5 | 0 | 75 | 290 | yes |

Abilities (last wiki state):
- **Laguna Blade** (cooldown [8.00/6.00/4.00], mana 75, magical): Lina unleashes a bolt of lightning, dealing [750/950/1900] damage to a single unit and setting them on fire for 5.0 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Laguna Blade: Lina unleashes a bolt of lightning, dealing [500/750/1000] damage to a single unit. (cd [10/8/6], mana 100)
- Pre-Big Update 2019-10-10: Laguna Blade: Lina unleashes a bolt of lightning, dealing [500/750/1200] damage to a single unit. (cd [10/8/6], mana 100)
- Big Update 2019-10-24: Laguna Blade: Lina unleashes a bolt of lightning, dealing [600/800/1500] damage to a single unit and setting them on fire for 5 seconds. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Tier changed from 3 to 4
  - Health changed from [1,200/2,400/4,800] to [1,000/2,000/4,000]
  - Max Mana changed from 100 to 75
  - Attack Damage Minimum changed from [50/100/200] to [65/130/260]
  - Attack Damage Maximum changed from [55/110/220] to [70/140/280]
  - Laguna Blade
  - Mana cost changed from 100 to 75
  - Cooldown changed from [10.00/8.00/6.00] to [8.00/6.00/4.00] seconds
  - Damage changed from [600, 800, 1500] to [750, 950, 1900]
- December 18, 2019
  - Removed from the game.
- October 24, 2019
  - Attack Rate changed from 0.625 to 0.714
  - Tier changed from 2 to 3
  - Laguna Blade:
  - Damage changed from [500, 750, 1200] to [600, 800, 1500]
  - Now sets targets on fire for 5.0
- August 01, 2019
  - Gold Cost changed from 3 to 2
- July 19, 2019
  - Laguna Blade:
  - Damage changed from [500, 750, 1000] to [500, 750, 1200]

</details>

### Lone Druid
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Savage, Shaman, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Savage (beta version), Druid | 800/1600/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 100 | 325 | yes |
| Pre-Big Update 2019-10-10 | T4 | Savage, Druid | 800/1600/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | yes |
| Big Update 2019-10-24 | T4 | Savage, Druid | 1600/3200/6000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | yes |
| Outlanders 2019-12-06 | T4 | Savage, Druid | 1600/3200/6000 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | yes |
| Season One 2020-02-25 | T4 | Savage, Druid, Summoner | 1200/2400/4500 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | yes |
| Final 2020-11-19 | T4 | Savage, Shaman, Summoner | 1200/2400/4800 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | yes |

Abilities (last wiki state):
- **Summon Spirit Bear** (cooldown 60.00, mana 60): Summons a powerful Spirit Bear companion.
  - Spirit Bear deals damage to players at the end of the round.
- **Shaman Alliance Bonus**: Lone Druid forms a Spirit Link with the Shaman Alliance creature. Whenever either damages an enemy, both lifesteal for [5/20/50]%.
- **Entangling Claws**: Passive: Spirit Bear's attacks have a chance to entangle the target, rooting and disarming them.
  - Has a 25% chance to entangle for 3.00 seconds and has no cooldown.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Summon Spirit Bear: Summons a powerful Spirit Bear companion. (cd 60, mana 100)
- Pre-Big Update 2019-10-10: Summon Spirit Bear: Summons a powerful Spirit Bear companion. (cd 60, mana 60)

<details><summary>Wiki change log (4 patches)</summary>

- August 27, 2020
  - Alliances changed from Savage Druid Summoner to Savage Summoner Shaman
  - Health changed from [1,200/2,400/4,500] to [1,200/2,400/4,800]
- March 05, 2020
  - Spirit Bear:
  - No longer damages opponent at end of round.
  - Entangling Claws:
  - No longer stuns. Instead, roots and disarms.
- February 25, 2020
  - Alliances changed from Savage Druid to Savage Druid Summoner
- July 19, 2019
  - Maximum Mana changed from 100 to 60
  - Summon Spirit Bear:
  - Mana Cost changed from 100 to 60
  - Spirit Bear:
  - Alliances changed from Savage to Savage Druid

</details>

### Luna
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Knight, Vigilant.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Elusive, Knight | 500/1000/2000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |
| Pre-Big Update 2019-10-10 | T2 | Elusive, Knight | 500/1000/2000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |
| Big Update 2019-10-24 | T2 | Elusive, Knight | 1000/2000/4000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |
| Outlanders 2019-12-06 | T2 | Elusive, Knight | 1000/2000/4000 | 50-60/120-144/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |
| Season One 2020-02-25 | T2 | Vigilant, Knight | 750/1500/3000 | 50-60/100-120/200-240 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |
| Final 2020-11-19 | T2 | Vigilant, Knight | 750/1500/3000 | 60-70/120-140/240-280 | 1.3 | 3 | 5 | 0 | 100 | 325 | yes |

Abilities (last wiki state):
- **Moon Glaives** (physical): Passive: Luna's glaives bounce [3/5/7] times between enemies 2 cells away. Glaives deal 30% less damage with each bounce.
  - The number of bounces does not include the initial attack.
  - This results in a total damage amplification of [253/294/314]%.
  - The bounce damage is unaffected by the damage that previous targets took.
  - Moon Glaives' bounces cannot trigger attack modifiers, including lifesteal.
- **Luna 3 Star Effect** (cooldown 40.0, mana 100, magical): At three stars, Luna gains Eclipse. In Luna's time of need, Selemene blocks the light in a radius of 2 cells around Luna and showers enemies with 15 beams of lunar energy, one every 0.3 seconds, doing 150 damage per beam and stunning for 0.5 seconds, with a maximum of 5 beams per enemy unit.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Moon Glaives: PASSIVE: Luna's glaives bounce between enemies 2 cells away.  Glaives deal 30% less damage with each bounce. (cd -, mana -)
- Big Update 2019-10-24: Moon Glaives: PASSIVE: Luna's glaives bounce [3/5/7] times between enemies 2 cells away.  Glaives deal 30% less damage with each bounce. (cd -, mana -)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Attack Damage Minimum changed from [50, 100, 200] to [60, 120, 240]
  - Attack Damage Maximum changed from [60, 120, 240] to [70, 140, 280]
- June 12, 2020
  - Luna 3 Star Effect (Eclipse)
  - Beam Damage changed from 200 to 150
- May 28, 2020
  - Attack Damage Minimum changed from [50, 120, 200] to [50, 100, 200]
  - Attack Damage Maximum changed from [60, 144, 240] to [60, 120, 240]
  - 3 Star Effect: Luna gains Eclipse. In Luna's time of need, Selemene blocks the light in a radius of 2 cells around Luna and showers enemies with 15 beams of lunar energy, one every 0.3 seconds, doing 200 Magical Damage per beam and stunning for 0.5 seconds, with a maximum of 5 beams per enemy unit.
- February 25, 2020
  - Alliances changed from Elusive Knight to Vigilant Knight
- December 18, 2019
  - Removed from the game.
- November 21, 2019
  - Attack Damage Minimum changed from [50, 100, 200] to [50, 120, 200]
  - Attack Damage Maximum changed from [60, 120, 240] to [60, 144, 240]

</details>

### Lycan
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Human, Savage, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Human, Savage (beta version), Warrior | 750/1500/3000 | 50-60/100-120/200-240 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |
| Pre-Big Update 2019-10-10 | T3 | Human, Savage, Hunter | 750/1500/3600 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |
| Big Update 2019-10-24 | T3 | Human, Savage, Hunter | 1500/3000/7200 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T3 | Human, Savage, Hunter | 1500/3000/7200 | 65-78/130-156/312-375 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T3 | Human, Savage, Summoner | 1125/2250/5400 | 50-60/100-120/240-288 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |
| Final 2020-11-19 | T3 | Human, Savage, Summoner | 1125/2250/5400 | 50-60/100-120/240-288 | 1.1 | 1 | 5 | 0 | 100 | 315 | yes |

Abilities (last wiki state):
- **Shapeshift** (cooldown 60.00, mana 100): Lycan summons [2/2/4] wolves transforms and gains [30/40/50]% to his max health, his move speed is boosted to 650 and gains 40% chance to Critical Hit for [150/175/200]% Damage.
- **Lycan 3 Star Effect**: Shapeshift summons more wolves.

Older abilities (wiki):
- **Summon Wolves** (cooldown 60.00, mana 100): Lycan summons two wolves to fight alongside him. Lycan then transforms and gains [30/40/50]% to his max health, his move speed is boosted to 650 and gains 40% chance to critically attack for [150/175/200]% bonus damage.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Summon Wolves: Lycan summons two wolves to fight alongside him.  Lycan then transforms and gains [30/40/50]% to his max health, his move speed is boosted to 650 and gains 40% chance to critically attack for [150/175/200]% bonus damage. (cd 60, mana 100)
- Pre-Big Update 2019-10-10: Summon Wolves: Lycan summons two wolves to fight alongside him.  Lycan then transforms and gains [40/50/60]% to his max health, his move speed is boosted to 650 and gains 40% chance to critically attack for [200/225/250]% bonus damage. (cd 60, mana 100)
- Big Update 2019-10-24: Shapeshift: Lycan transforms and gains [40/50/60]% to his max health, his move speed is boosted to 650 and gains 40% chance to Critical Hit for [200/225/250]% Damage. (cd 60, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- July 27, 2020
  - Shapeshift:
  - Wolves summoned changed from [2, 2, 3] to [2, 2, 4]
- February 25, 2020
  - Alliances changed from Human Savage Hunter to Human Savage Summoner
  - Attack Damage Minimum changed from [65, 130, 312] to [50, 100, 240]
  - Attack Damage Maximum changed from [78, 156, 375] to [60, 120, 288]
  - Shapeshift:
  - Once again summons two wolves to fight for Lycan.
  - Health percentage gain upon transformation changed from [40, 50, 60]% to [30, 40, 50]%
  - Critical strike percentage gain upon transformation changed from [200, 225, 250]% to [150, 175, 200]%
  - Savage Wolf:
  - Alliances changed from Savage Hunter to Savage
- September 26, 2019
  - Attack Damage Minimum changed from [50, 100, 240] to [65, 130, 312].
  - Attack Damage Maximum changed from [60, 120, 288] to [78, 156, 375].
  - Summon Wolves is now Shapeshift:
  - No wolves are summoned.
  - Max HP gain from [30, 40, 50] to [40, 50, 60].
  - Critical Hit bonus changed from [150, 175, 200] to [200, 225, 250].
- August 08, 2019
  - When Lycan transforms and he gains 30% max health, he heals by the full 30% each time, rather than this being based on his current health at the time of transforming.
- July 24, 2019
  - Alliance changed from Human Warrior Savage to Human Hunter Savage
  - Lycan's Wolves are Hunter Savage
- July 19, 2019
  - Maximum Health changed from [750, 1500, 3000] to [750, 1500, 3600]
  - Attack Damage Minimum changed from [50, 100, 200] to [50, 100, 240]
  - Attack Damage Maximum changed from [60, 120, 240] to [60, 120, 288]
  - Summoned Wolves now have phased movement and can run through other units

</details>

### Magnus
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Savage, Shaman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T2 | Shaman (old, beta), Brute | 1500/3000/7000 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 0 | 280 | yes |
| Outlanders 2019-12-06 | T2 | Shaman (old, beta), Brute | 1875/3750/7500 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 0 | 280 | yes |
| Season One 2020-02-25 | T1 | Savage, Druid | 1100/2200/4400 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 100 | 280 | yes |
| Final 2020-11-19 | T1 | Savage, Shaman | 1100/2200/4400 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 100 | 280 | yes |

Abilities (last wiki state):
- **Empower** (cooldown 7): Magnus enrages a friendly unit, granting it +[30/50/80]% damage and [30/50/80]% Cleave for 20 seconds.
  - Magnus casts first on himself, then on the closest, unbuffed ally.
- **Shaman Alliance Bonus**: Whenever it is attacked by a melee unit, the Shaman Alliance creature has a [10/20/30]% chance to push the attacker up to 6 cells away.
- **Magnus 3 Star Effect** (cooldown 40, mana 100, magical): At three stars, Magnus gains Reverse Polarity. On cast, enemies within 2 cells take 100 damage and are stunned for 1 second.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Empower: Magnus enrages an ally granting him [30/50/80]% damage and [30/50/80]% Cleave for 20 seconds. (cd 7, mana -)
- Outlanders 2019-12-06: Empower: Magnus enrages a friendly unit, granting it +[30/50/80]% damage and [30/50/80]% Cleave for 20 seconds. (cd 7, mana -)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Alliances changed from Savage Druid to Savage Shaman.
- July 27, 2020
  - Magnus 3 Star Effect
  - Duration changed from 2 seconds to 1 second
- July 23, 2020
  - Max Max Mana changed from 0 to 100
  - 3 Star Effect: Magnus gains Reverse Polarity. On cast, enemies within 2 cells take 100 damage and are stunned for 2 seconds. Mana Cost 100, Cooldown 40.
- February 25, 2020
  - Tier changed from 2 to 1
  - Alliances changed from Shaman Brute to Savage Druid
  - Maximum Health changed from [1405, 2810, 5625] to [1100, 2200, 4400]
- December 18, 2019
  - Removed from the game.
- November 07, 2019
  - Maximum Health changed from [1500, 3000, 7000] to [1875, 3750, 7500].

</details>

### Medusa
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Scaled, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Scaled, Hunter | 850/1700/3300 | 50-60/100-120/200-240 | 1 | 4 | 5 | 10 | 100 | 275 | yes |
| Pre-Big Update 2019-10-10 | T5 | Scaled, Hunter | 850/1700/3300 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | yes |
| Big Update 2019-10-24 | T5 | Scaled, Hunter | 1700/3400/6600 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | yes |
| Outlanders 2019-12-06 | T5 | Scaled, Hunter | 1700/3400/6600 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | yes |
| Season One 2020-02-25 | T5 | Scaled, Hunter | 1275/2550/4950 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | yes |
| Final 2020-11-19 | T5 | Scaled, Hunter | 1275/2550/5100 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | yes |

Abilities (last wiki state):
- **Stone Gaze** (cooldown 20.00, mana 100): Any enemy unit up to 3 cells away looking at Medusa will have their movement and attack speed slowed by 30%. If 2 seconds of total time is accumulated looking at Medusa, that unit will turn to stone. Petrified units are stunned, and take bonus 20% physical damage.
  - Stone Gaze's scream duration is [4/5/6] seconds.
- **Split Shot**: Passive: Medusa magically shoots [1/2/3] extra arrows, hitting additional targets within 1 cell of her primary target. The additional arrows deal [0/-20/-30]% less damage.
  - Split Shot also applies item effects on the secondary targets, with Silver Edge being the only exception.
  - Damage reduction happens separately and independently for all Split Shot targets.
- **Ace effect (2019-08-30 to 2020-02-25, Hunter Ace):** Hunters deal 5% extra Attack Damage per Hunter Alliance level for each other Hunter attacking the same target.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Stone Gaze: Any enemy unit up to 4 cells away looking at Medusa will have their movement and attack speed slowed by 30%.  If 2 seconds of total time is accumulated looking at Medusa, that unit will turn to stone.  Petrified units are stunned, and take bonus 20% physical damage. (cd 20, mana 100) / medusa_split_shot: description unknown (cd -, mana -)
- Pre-Big Update 2019-10-10: Stone Gaze: Any enemy unit up to 3 cells away looking at Medusa will have their movement and attack speed slowed by 30%.  If 2 seconds of total time is accumulated looking at Medusa, that unit will turn to stone.  Petrified units are stunned, and take bonus 20% physical damage. (cd 20, mana 100) / medusa_split_shot: description unknown (cd -, mana -) / Ace/alliance bonus (internal id medusa_naga_bonus): see the Ace effect entry (cd -, mana -)
- Big Update 2019-10-24: Stone Gaze: Any enemy unit up to 3 cells away looking at Medusa will have their movement and attack speed slowed by 30%.  If 2 seconds of total time is accumulated looking at Medusa, that unit will turn to stone.  Petrified units are stunned, and take bonus 20% physical damage. (cd 20, mana 100) / Split Shot: PASSIVE: Medusa magically shoots up to [2/3/4] extra arrows, hitting additional targets within 1 cell of her primary target. The additional arrows deal -20 percent of her primary arrow's damage. (cd -, mana -) / Ace/alliance bonus (internal id medusa_hunter_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (10 patches)</summary>

- September 04, 2020
  - Split Shot:
  - Damage reduction for extra shots changed from -20 to [0, -20, -30]
  - Extra shots changed from 2 to [1, 2, 3]
- August 27, 2020
  - Health changed from [1,275/2,550/4,950] to [1,275/2,550/5,100]
- February 25, 2020
  - Ace effect removed
  - Tier changed from Ace to 5.
  - Split Shot:
  - Extra Shots changed from [2, 3, 4] to 2
- October 24, 2019
  - Medusa changed from the ACE OF SCALED to the ACE OF HUNTERS
  - Old Ace Effect: Grants Scaled units Retaliate. When a unit with Retaliate is attacked, they apply a 25 dmg/sec per Alliance level debuff to their attacker. +15% chance of finding if you have at least one level of the Scaled Alliance completed.
  - New Ace Effect: Hunters deal 5% extra Attack Damage per Hunter Alliance level for each other Hunter attacking the same target. +15% chance of finding if you have at least one level of the Hunter Alliance completed.
- August 30, 2019
  - Ace for the Scaled Alliance.
  - Attack Range changed from 4 to 3.
  - Ace Effect: Grants Scaled units Retaliate. When a unit with Retaliate is attacked, they apply a 25 dmg/sec per Alliance level debuff to their attacker.
  - Stone Gaze:
  - Radius changed from 4 to 3.
- July 24, 2019
  - Attack Damage Minimum changed from [50, 100, 200] to [75, 150, 300]
  - Attack Damage Maximum changed from [60, 120, 240] to [80, 160, 320]
- July 19, 2019
  - Gold Cost changed from 4 to 5
  - Draft Tier changed from 4 to 5
  - Split Shot:
  - Damage Modifier changed from -40% to -20%
  - Extra Shots changed from 2 to [2, 3, 4]
- July 12, 2019
  - Split Shot:
  - Changed extra shots from 3 to 2.
  - Will ignore nearby targets that happen to be outside Medusa's attack range.
- July 04, 2019
  - Split Shot Now applies item effects on secondary shots.
- June 27, 2019
  - Split Shot extra shot damage reduction changed: [-40, -30, -20 ] -> -40.

</details>

### Meepo
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Rogue, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T2 | Rogue, Summoner | 1100/2200/4400 | 50-60/100-120/200-240 | 1.1 | 1 | 5 | 0 | 75 | 310 | yes |

Abilities (last wiki state):
- **Poof** (cooldown [15.00/13.00/11.00] seconds, mana 75, magical): Meepo teleports to a random enemy, dealing [150/250/350] damage to all adjacent enemy units.
- **Divided We Stand**: Whenever Meepo casts an ability, he creates a copy of himself with the same health he currently has. If Meepo or any clone dies, all Meepos die.
  - When a copy of Meepo is created with an item, the copy's item is created not on cooldown.
- **Meepo 3 Star Effect**: Clones of Meepo copy Meepo's item as well.

<details><summary>Wiki change log (1 patches)</summary>

- September 24, 2020
  - Poof:
  - Meepo will shift target cells as needed while channeling, instead of interrupting the channel and starting again whenever his original target cell becomes invalid.

</details>

### Mirana
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Vigilant, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Elusive, Hunter | 750/1500/3000 | 55-65/110-130/220-260 | 1 | 4 | 5 | 0 | 100 | 310 | yes |
| Pre-Big Update 2019-10-10 | T4 | Elusive, Hunter | 750/1500/3000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 | yes |
| Big Update 2019-10-24 | T4 | Elusive, Hunter | 1500/3000/6000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 | yes |
| Outlanders 2019-12-06 | T4 | Elusive, Hunter | 1500/3000/6000 | 66-78/132-156/264-312 | 1 | 4 | 5 | 0 | 70 | 310 | yes |
| Season One 2020-02-25 | T4 | Vigilant, Hunter | 1125/2250/4500 | 56-68/112-136/224-272 | 1 | 4 | 5 | 0 | 70 | 310 | yes |
| Final 2020-11-19 | T4 | Vigilant, Hunter | 1125/2250/4500 | 70-80/140-160/280-320 | 1 | 4 | 5 | 0 | 70 | 310 | yes |

Abilities (last wiki state):
- **Sacred Arrow** (cooldown 8.00, mana 70, magical): Mirana fires a long-range arrow with deadly precision, stunning her target for [3/5/7] seconds and dealing [400/600/1400] damage.
  - Mirana leaps to a corner of the board and casts Sacred Arrow only when she lands.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Sacred Arrow: Mirana fires a long-range arrow with deadly precision, stunning her target for [3/5/7] seconds and dealing [400/800/1800] damage. (cd 8, mana 100)
- Pre-Big Update 2019-10-10: Sacred Arrow: Mirana fires a long-range arrow with deadly precision, stunning her target for [3/5/7] seconds and dealing [400/800/1800] damage. (cd 8, mana 70)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Health changed from [1,500/3,000/6,000] to [1,125/2,250/4,500]
  - Attack Damage Minimum changed from [56, 112, 224] to [70, 140, 280]
  - Attack Damage Maximum changed from [68, 136, 272] to [80, 160, 320]
- April 03, 2020
  - Attack Damage Minimum changed from [66, 132, 264] to [56, 112, 224]
  - Attack Damage Maximum changed from [78, 156, 312] to [68, 136, 272]
  - Sacred Arrow:
  - Damage changed from [400, 800, 1800] to [400, 600, 1400]
- February 25, 2020
  - Alliances changed from Elusive Hunter to Vigilant Hunter
- December 18, 2019
  - Removed from the game.
- July 19, 2019
  - Maximum Mana changed from 100 to 70
  - Attack Damage Minimum changed from [55, 110, 220] to [66, 132, 264]
  - Attack Damage Maximum changed from [65, 130, 260] to [78, 156, 312]
  - Sacred Arrow:
  - Mana Cost changed from 100 to 70

</details>

### Morphling
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Primordial, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Primordial (beta version), Assassin | 500/1000/2000 | 45-60/90-120/180-240 | 1.3 | 3 | 5 | 0 | 100 | 280 | yes |
| Pre-Big Update 2019-10-10 | T2 | Primordial, Assassin | 500/1000/2000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 | yes |
| Big Update 2019-10-24 | T2 | Primordial, Assassin | 1000/2000/4000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 | yes |
| Outlanders 2019-12-06 | T3 | Primordial, Mage | 1250/2500/4000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 | yes |
| Season One 2020-02-25 | T3 | Primordial, Mage | 935/1875/3000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 | yes |
| Final 2020-11-19 | T3 | Primordial, Mage | 935/1875/3000 | 45-60/90-120/180-240 | 1.1 | 3 | 5 | 0 | 75 | 280 | no |

Abilities (last wiki state):
- **Waveform** (cooldown 6.00, mana 75, magical): Morphling dissolves into liquid and surges forward, damaging enemy units in his path for [100/175/325] damage. Morphling is invulnerable during Waveform.
  - Has a range of 4.
- **Morphling 3 Star Effect**: At three stars, Morphling gains the in-combat bonuses of all active enemy Alliances as if he were an enemy Champion.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Waveform: Morphling dissolves into liquid and surges forward, damaging enemy units in his path for [150/250/350] damage.  Morphling is invulnerable during Waveform. (cd [10/8/6], mana 100)
- Pre-Big Update 2019-10-10: Waveform: Morphling dissolves into liquid and surges forward, damaging enemy units in his path for [150/250/350] damage.  Morphling is invulnerable during Waveform. (cd [8/6/4], mana 75)
- Outlanders 2019-12-06: Waveform: Morphling dissolves into liquid and surges forward, damaging enemy units in his path for [100/175/450] damage.  Morphling is invulnerable during Waveform. (cd [8/6/4], mana 75)

<details><summary>Wiki change log (5 patches)</summary>

- July 16, 2020
  - 3 Star Effect: Morphling gains the in-combat bonuses of all active enemy Alliances as if he were an enemy Champion.
- January 16, 2020
  - Waveform:
  - Damage changed from [100, 175, 450] to [100, 175, 325]
  - Cooldown changed from [8, 6, 4] to 6
- November 21, 2019
  - Maximum Mana changed from 100 to 75
  - Maximum Health changed from [1000, 2000, 4000] to [1250, 2500, 4000]
  - Waveform:
  - Damage changed from [200, 350, 450] to [100, 175, 450]
  - Mana Cost changed from 100 to 75
- November 14, 2019
  - Changed from Assassin to Mage
  - Maximum Mana changed from 75 to 100
  - Tier changed from 2 to 3
  - Waveform:
  - Damage changed from [150, 250, 350] to [200, 350, 450]
  - Mana Cost changed from 75 to 100
- July 19, 2019
  - Attack Speed changed from 0.77 to 0.91.
  - Maximum Mana changed from 100 to 75.
  - Waveform:
  - Cooldown changed from [10, 8, 6] to [8, 6, 4].
  - Mana Cost changed from 100 to 75.

</details>

### Nature's Prophet
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Shaman, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Elusive, Druid | 500/1000/2000 | 45-50/90-100/180-200 | 1.4 | 3 | 0 | 0 | 50 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Elusive, Druid | 500/1000/2000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T2 | Druid, Shaman (old, beta) | 1000/2000/4000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Druid, Shaman (old, beta) | 1000/2000/4000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T2 | Druid, Summoner | 750/1500/3000 | 56-62/112-125/225-250 | 1.3 | 3 | 0 | 0 | 80 | 290 | yes |
| Final 2020-11-19 | T2 | Shaman, Summoner | 900/1800/3600 | 55-65/110-130/220-260 | 1.3 | 3 | 0 | 0 | 80 | 290 | yes |

Abilities (last wiki state):
- **Nature's Call** (cooldown 6.00, mana 80): Nature's Prophet summons a Treant. This Treant is considered a druid.
  - At 1 and 2 stars, Nature's Prophet always summons his lesser treants next to his current position.
- **Shaman Alliance Bonus** (cooldown 15.00, magical): The Shaman Alliance creature gains Wrath of Nature. The creature blasts a target for [100/225/400] damage. The blast bounces [3/4/5] times to other enemies, increasing the damage by 15% per bounce.
- **Nature's Prophet 3 Star Effect**: Treants spawn on the enemy's side of the board.

Older abilities (wiki):
- **Wrath of Nature** (cooldown [15.00/13.00/11.00], mana 100): Nature's Prophet zaps a target for [250/275/300] magical damage. This bounces [3/4/5] times across enemies in the board, increasing the damage by [10/13/15]% per bounce.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Nature's Call: Nature's Prophet summons a Treant from the edge of the board. This Treant is considered a druid. (cd 6, mana 50)
- Pre-Big Update 2019-10-10: natures_prophet_wrath_of_nature: description unknown (cd [15/13/11], mana 100)
- Big Update 2019-10-24: Wrath of Nature: Nature's Prophet zaps a target for [250/275/300] damage. This bounces [3/4/5] times across enemies in the board, increasing the damage by [10/13/15]% per bounce. (cd [15/13/11], mana 100)

<details><summary>Wiki change log (8 patches)</summary>

- August 27, 2020
  - Alliances changed from Druid Summoner to Shaman Summoner
  - Health changed from [1,000/2,000/4,000] to [900/1,800/3,600]
  - Attack Damage Minimum changed from [56, 112, 225] to [55, 110, 220]
  - Attack Damage Maximum changed from [62, 125, 250] to [65, 130, 260]
- May 08, 2020
  - Nature's Call:
  - At 3 stars, trees spawn near backline enemies.
- April 03, 2020
  - Maximum Mana changed from 100 to 80
  - Nature's Call:
  - Mana Cost changed from 100 to 80
  - Nature's Prophet's Lesser Treant:
  - Maximum Health changed from [650, 1125, 1500] to [650, 1200, 1800]
- February 25, 2020
  - Alliances changed from Druid Shaman to Druid Summoner
  - Removed ability Wrath of Nature
  - Readded ability Nature's Call:
  - Mana Cost changed from 50 to 100
  - Lesser Treant:
  - Maximum Health changed from [525, 900, 1200] to [650, 1125, 1500]
- December 18, 2019
  - Removed from the game.
- October 24, 2019
  - Alliances changed from Elusive Druid to Druid Shaman
- September 26, 2019
  - Attack Rate changed from 0.714 to 0.769.
  - Maximum Mana changed from 50 to 100.
  - Attack Damage Minimum changed from [45, 90, 180] to [56, 112, 225].
  - Attack Damage Maximum changed from [50, 100, 200] to [62, 125, 250].
  - Removed Nature's Call ability.
  - Added Wrath of Nature:
  - Nature's Prophet zaps a target for [250, 275, 300] damage. This bounces [3, 4, 5] times across enemies in the board, increasing the damage by [10, 13, 15]% per bounce.
  - Mana Cost: 100.
  - Cooldown: [15, 13, 11].
- July 19, 2019
  - Nature's Call:
  - Cast Animation length changed from 1.97 to 1.0.
  - Lesser Treant:
  - Now spawn near Nature's Prophet.
  - Attack Speed changed from 0.67 to 0.83.
  - Maximum Health changed from [300, 500, 700] to [350, 600, 800].

</details>

### Necrophos
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 4, Heartless, Warlocks, Healer.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Heartless, Warlock | 850/1700/3400 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | yes |
| Pre-Big Update 2019-10-10 | T4 | Heartless, Warlock | 850/1700/3400 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | yes |
| Big Update 2019-10-24 | T4 | Heartless, Healer | 1700/3400/6800 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | yes |
| Outlanders 2019-12-06 | T4 | Heartless, Healer | 1700/3400/6800 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | yes |
| Season One 2020-02-25 | T4 | Heartless, Warlock, Healer | 1275/2550/5100 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | yes |
| Final 2020-11-19 | T4 | Heartless, Warlock, Healer | 1275/2550/5100 | 50-60/100-120/200-240 | 1.2 | 3 | 5 | 0 | 100 | 280 | no |

Abilities (last wiki state):
- **Death Pulse** (cooldown 7.00, mana 100, magical): Necrophos releases a wave of death around him affecting enemy and friendly units up to 3 cells away. Death Pulse deals [100/150/250] damage to enemies and heals allies for [50/75/125].

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Death Pulse: Necrophos releases a wave of death around him affecting enemy and friendly units up to 3 cells away.  Death Pulse deals [100/150/200] damage to enemies and heals allies for [100/175/250]. (cd 5, mana 100)
- Pre-Big Update 2019-10-10: Death Pulse: Necrophos releases a wave of death around him affecting enemy and friendly units up to 3 cells away.  Death Pulse deals [100/150/200] damage to enemies and heals allies for [100/175/250]. (cd 7.5, mana 100)
- Outlanders 2019-12-06: Death Pulse: Necrophos releases a wave of death around him affecting enemy and friendly units up to 3 cells away.  Death Pulse deals [100/150/200] damage to enemies and heals allies for [100/175/250]. (cd 5, mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- April 30, 2020
  - Death Pulse:
  - Heal changed from [50, 75, 100] to [50, 75, 125]
  - Damage changed from [100, 150, 200] to [100, 150, 250]
  - Cooldown changed from 5 to 7
- February 25, 2020
  - Alliances changed from Heartless Healer to Heartless Warlock Healer
  - Death Pulse:
  - Heal amount changed from [100, 175, 250] to [50, 75, 100]
- November 27, 2019
  - Death Pulse:
  - Cooldown changed from 7.5 to 5
- October 24, 2019
  - Alliances changed from Heartless Warlock to Heartless Healer
- July 19, 2019
  - Death Pulse:
  - Cooldown changed from 5 to 7.5.

</details>

### Nyx Assassin
- Status: Rotated out at the end (still in final data). First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Insect, Assassin.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T1 | Insect, Assassin | 1100/2200/4400 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 | yes |
| Outlanders 2019-12-06 | T1 | Insect, Assassin | 1100/1980/4400 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 | yes |
| Season One 2020-02-25 | T1 | Insect, Assassin | 825/1485/3300 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 | yes |
| Final 2020-11-19 | T1 | Insect, Assassin | 825/1485/3300 | 50-70/100-140/200-280 | 1.5 | 1 | 5 | 0 | 100 | 315 | no |

Abilities (last wiki state):
- **Spiked Carapace** (cooldown 10, mana 100): Nyx Assassin pops spikes out of its carapace, negating and reflecting 100% of the damage dealt (max once from each source), as well as stunning the source of the damage for [1.0/1.5/2.0]s.
  - Spiked Carapace lasts 2 seconds.
- **Vendetta** (physical): Passive: After teleporting to attack a unit, Nyx Assassin deals 200 bonus damage on his next attack and applies break to the enemy for 4s, disabling all passive effects.
  - The bonus damage does not contribute towards critical hits.
- **Nyx Assassin 3 Star Effect**: Nyx Assassin also applies Vendetta after 4 successive attacks. The counter is reset if Vendetta triggers due to teleporting.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Spiked Carapace: Nyx Assassin pops spikes out of its carapace, negating and reflecting 100% of the damage dealt (max once from each source), as well as stunning the source of the damage for [1.0/1.5/2.0]s. (cd 10, mana 100) / Vendetta: PASSIVE: After teleporting to attack a unit, Nyx Assassin deals 200 bonus damage on his next attack and applies Break to the enemy for 4s, disabling all passive effects. (cd -, mana -)

<details><summary>Wiki change log (2 patches)</summary>

- July 16, 2020
  - 3 Star Effect: Nyx Assassin also applies Vendetta after every 4 successive attacks. The counter is reset if Vendetta triggers due to teleporting.
- November 21, 2019
  - Maximum Health changed from [1100, 2200, 4400] to [1100, 1980, 4400]

</details>

### Ogre Magi
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 2, Blood-bound, Brute, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Blood-Bound, Mage | 800/1600/3000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T1 | Blood-Bound, Mage | 800/1600/3000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 | yes |
| Big Update 2019-10-24 | T1 | Blood-Bound, Mage | 1600/3200/6000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Blood-Bound, Brute, Mage | 1600/3200/6000 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 | yes |
| Season One 2020-02-25 | T2 | Blood-Bound, Brute, Mage | 1200/2400/4500 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 | yes |
| Final 2020-11-19 | T2 | Blood-Bound, Brute, Mage | 1200/2400/4500 | 55-65/110-130/220-260 | 1.4 | 1 | 5 | 0 | 0 | 290 | no |

Abilities (last wiki state):
- **Bloodlust** (cooldown 5.00): Ogre Magi incites a frenzy in a friendly unit, increasing its movement speed by [8/10/12]% and attack speed by [30/65/100]%. If cast on Ogre Magi himself, the attack speed bonus is [50/90/130]% instead.
  - Has a duration of 30 seconds.
- **Ogre Magi 3 Star Effect**: Ogre Magi has a 75% chance to Multicast Bloodlust to 2 targets.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Bloodlust: Ogre Magi incites a frenzy in a friendly unit, increasing its movement speed by [8/10/12]% and attack speed by [30/40/50]%. If cast on Ogre Magi himself, the attack speed bonus is [35/55/75]% instead. (cd 5, mana 100)
- Pre-Big Update 2019-10-10: Bloodlust: Ogre Magi incites a frenzy in a friendly unit, increasing its movement speed by [8/10/12]% and attack speed by [30/65/100]%. If cast on Ogre Magi himself, the attack speed bonus is [50/90/130]% instead. (cd 5, mana -)

<details><summary>Wiki change log (7 patches)</summary>

- May 08, 2020
  - Bloodlust:
  - At 3 stars, Ogre Magi has a 75% chance per cast to Multicast Bloodlust at 2 targets rather than 1.
- December 05, 2019
  - Draft tier changed from 1 to 2.
- November 21, 2019
  - Alliances changed from Blood-bound Mage to Blood-bound Brute Mage.
- August 30, 2019
  - Ogre Magi will cast Bloodlust on himself first, and then on allies ordered by distance, closest first.
- August 15, 2019
  - Bloodlust:
  - Mana cost is now 0. Cooldown is still 5 seconds.
  - Ogre Magi now has 0 max mana.
- August 01, 2019
  - Bloodlust:
  - Self Bonus Attack Speed changed from (35, 55, 75) to (50, 90, 130)
  - Other Bonus Attack Speed changed from (30, 40, 50) to (30, 65, 100)
- June 17, 2019
  - Bloodlust self buff increased from 30/50/70 to 35/55/75.

</details>

### Omniknight
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Human, Knight, Healer.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Human, Knight | 700/1400/2800 | 50-60/100-120/200-240 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T3 | Human, Knight | 700/1400/3360 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T3 | Human, Knight, Healer | 1400/2800/6720 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T3 | Human, Knight, Healer | 1610/3220/7728 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 60 | 300 | yes |
| Season One 2020-02-25 | T3 | Human, Knight, Healer | 1100/2200/5280 | 40-50/80-100/190-240 | 1.4 | 1 | 10 | 0 | 60 | 300 | yes |
| Final 2020-11-19 | T3 | Human, Knight, Healer | 1100/2200/5280 | 40-50/80-100/192-240 | 1.4 | 1 | 10 | 0 | 60 | 300 | yes |

Abilities (last wiki state):
- **Purification** (cooldown [10.0/8.0/6.0], mana 60, pure): Omniknight instantly heals a friendly unit for [200/400/720] and damages all enemy units [1/1/2] cell away.
  - Purification damage amounts are the same as the healing provided by the ability.
- **Omniknight 3 Star Effect**: Purification's area of effect is tripled.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Purification: Omniknight instantly heals a friendly unit for [200/400/600] and damages all enemy units 1 cell away. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Purification: Omniknight instantly heals a friendly unit for [200/400/720] and damages all enemy units 1 cell away. (cd 10, mana 100)
- Outlanders 2019-12-06: Purification: Omniknight instantly heals a friendly unit for [200/400/720] and damages all enemy units 1 cell away. (cd [10/8/6], mana 60)

<details><summary>Wiki change log (7 patches)</summary>

- June 12, 2020
  - Purification
  - At three stars, Purification now has a 2-cell radius instead of 1 cell.
- January 16, 2020
  - Maximum Health changed from [1205, 2415, 5795] to [1100, 2200, 5280]
- November 21, 2019
  - Maximum Mana changed from 100 to 60
  - Maximum Health changed from [1400, 2800, 6720] to [1610, 3220, 7728]
  - Purification:
  - Cooldown changed from 10 to [10, 8, 6]
  - Mana Cost changed from 100 to 60
- October 24, 2019
  - Alliances changed from Human Knight to Human Knight Healer
- July 19, 2019
  - Maximum Health changed from [700, 1400, 2800] to [700, 1400, 3360].
  - Attack Damage Minimum changed from [40, 80, 160] to [40, 80, 190].
  - Attack Damage Maximum changed from [50, 100, 200] to [50, 100, 240].
  - Purification:
  - Heal changed from [200, 400, 600] to [200, 400, 720].
- June 26, 2019
  - Base Damage Changed: [50-60, 100-120, 200-240] -> [40-50, 80-100, 160-200]
- June 17, 2019
  - Purification Heal/Damage increased from 200/300/400 to 200/400/600.

</details>

### Pangolier
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Savage, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T4 | Savage, Swordsman | 1400/2800/5600 | 60-70/120-140/240-280 | 1.1 | 1 | 10 | 10 | 70 | 330 | yes |

Abilities (last wiki state):
- **Shield Crash** (cooldown [15.00/13.00/11.00], mana 70, magical): Pangolier jumps in the air and slams back into the ground, doing [150/300/450] damage to enemies within 2 cell and gaining 8% damage reduction per enemy for 10 seconds. After the jump all enemies in attack range focus on Pangolier.

<details><summary>Wiki change log (1 patches)</summary>

- September 24, 2020
  - Shield Crash:
  - Fixed taunt not working.

</details>

### Phantom Assassin
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Assassin, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Elusive, Assassin | 550/1100/2200 | 70-90/140-180/280-360 | 1.2 | 1 | 5 | 0 | 100 | 305 | yes |
| Pre-Big Update 2019-10-10 | T3 | Elusive, Assassin | 550/1100/2200 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 | yes |
| Big Update 2019-10-24 | T3 | Elusive, Assassin | 1100/2200/4400 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 | yes |
| Outlanders 2019-12-06 | T3 | Elusive, Assassin | 1100/1980/4400 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T3 | Elusive, Assassin | 825/1485/3300 | 70-90/140-180/300-400 | 1.2 | 1 | 5 | 0 | 100 | 305 | no |
| Final 2020-11-19 | T1 | Assassin, Swordsman | 825/1650/3300 | 40-60/80-120/160-240 | 1.2 | 1 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Coup de Grace** (physical): Passive: Phantom Assassin has a [15/20/25]% chance of delivering a devastating [300/450/600]% critical strike to enemy units.
- **Phantom Assassin 3 Star Effect**: Phantom Assassin gains the bonus of the current active level of Rogue Alliance, if any.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Coup de Grace: PASSIVE: Phantom Assassin has a 10% chance of delivering a devastating [300/450/600]% critical strike to enemy units. (cd -, mana -)
- Pre-Big Update 2019-10-10: Coup de Grace: PASSIVE: Phantom Assassin has a [15/20/25]% chance of delivering a devastating [300/450/600]% critical strike to enemy units. (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- August 27, 2020
  - Tier changed from 3 to 1
  - Health changed from [1,100/1,980/4,400] to [825/1,650/3,300]
  - Attack Damage Minimum changed from [70/140/300] to [40/80/160]
  - Attack Damage Maximum changed from [90/180/400] to [60/120/240]
  - 3 Star Effect: Phantom Assassin gains the benefit of your current Rogue Alliance, if any.
- December 18, 2019
  - Removed from the game.
- November 21, 2019
  - Maximum Health changed from [1100, 2200, 4400] to [1100, 1980, 4400]
- July 19, 2019
  - Attack Damage Minimum changed from [70, 140, 280] to [70, 140, 300].
  - Attack Damage Maximum changed from [90, 180, 360] to [90, 180, 400].
  - Coup de Grace:
  - Crit Chance changed from 10 to [15, 20, 25].

</details>

### Puck
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Dragon, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Elusive, Dragon, Mage | 450/900/1800 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Elusive, Dragon, Mage | 450/900/1800 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 | yes |
| Big Update 2019-10-24 | T2 | Elusive, Dragon, Mage | 900/1800/3600 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Elusive, Dragon, Mage | 900/1800/3600 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 | yes |
| Season One 2020-02-25 | T2 | Elusive, Dragon, Mage | 675/1350/2700 | 40-50/80-100/160-200 | 1.7 | 3 | 5 | 0 | 60 | 290 | no |
| Final 2020-11-19 | T3 | Dragon, Mage | 850/1700/4080 | 50-60/100-120/240-288 | 1.7 | 3 | 5 | 0 | 60 | 290 | yes |

Abilities (last wiki state):
- **Illusory Orb** (cooldown [6.00/4.50/0.50], mana 60, magical): Puck launches a magical orb that floats in a straight path, damaging enemy units along the way for [200/225/250] damage.
- **Puck 3 Star Effect**: Illusory Orb's cooldown reduced to 0.5 seconds
- **Phase Shift** (cooldown [6.00/4.00/2.00]): Unlocked by the Dragon alliance bonus Passive: Just before taking damage, Puck briefly shifts into another dimension where it is immune from harm for 0.5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Illusory Orb: Puck launches a magic orb that floats in a straight path, damaging enemy units along the way for [150/200/250] damage. (cd [5/4/3], mana 100)
- Pre-Big Update 2019-10-10: Illusory Orb: Puck launches a magic orb that floats in a straight path, damaging enemy units along the way for [150/200/250] damage. (cd [5/4/3], mana 60)

<details><summary>Wiki change log (7 patches)</summary>

- September 24, 2020
  - Illusory Orb:
  - Puck no longer casts Illusory Orb at out of range targets.
- September 04, 2020
  - Illusory Orb:
  - Damage changed from [200, 250, 300] to [200, 225, 250]
  - Cooldown changed from [6, 5, 0.5] to [6, 4.5, 0.5]
- August 27, 2020
  - Tier changed from 2 to 3. Now has Dragon Mage alliances.
  - Health changed from [900/1,800/3,600] to [850/1,700/4,080]
  - Attack Damage Minimum changed from [40, 80, 160] to [50, 100, 240]
  - Attack Damage Maximum changed from [50, 100, 200] to [60, 120, 288]
  - 3 Star Effect: Illusory Orb's cooldown is greatly reduced.
- December 18, 2019
  - Removed from the game.
- August 15, 2019
  - Phase Shift:
  - Will no longer trigger if Puck is hexed or stunned or force-attacking a target.
  - Properly purges stuns, evens those applied by the hit that triggered Phase Shift.
- July 19, 2019
  - Maximum Mana changed from 100 to 60.
  - Illusory Orb:
  - Mana Cost changed from 100 to 60.
- June 25, 2019
  - Dragon Alliance now unlocks Phase Shift.

</details>

### Pudge
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Heartless, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Heartless, Warrior | 1000/2500/5500 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |
| Pre-Big Update 2019-10-10 | T2 | Heartless, Warrior | 1000/2500/5500 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |
| Big Update 2019-10-24 | T2 | Heartless, Warrior | 2000/5000/11000 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |
| Outlanders 2019-12-06 | T2 | Heartless, Warrior | 2000/5000/9350 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |
| Season One 2020-02-25 | T2 | Heartless, Warrior | 1500/3750/7010 | 45-55/90-110/135-165 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |
| Final 2020-11-19 | T2 | Heartless, Warrior | 1650/3300/6600 | 45-55/90-110/180-220 | 1.7 | 1 | 5 | 5 | 100 | 280 | yes |

Abilities (last wiki state):
- **Meat Hook** (cooldown [20.00/18.00/15.00], mana 100, physical): Pudge launches a bloody hook towards his farthest enemy, dragging the victim back and dealing [150/220/290] damage. Pudge and allies 1 cell away will direct their attacks to the victim until it dies.
  - Targets the farthest away enemy hero (in fields; horizontals, verticals and diagonals counting the same).
  - If there are multiple enemies within the same range, the actual, physical distance is used as a tiebreaker.
  - Summons are ignored unless there are no enemy heroes left.
- **Pudge 3 Star Effect**: At three stars, Pudge becomes a Flesh Heap and gains 500 Max HP and 10% Magic Resistance each time an enemy dies within 1 cell of him.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Meat Hook: Pudge launches a bloody hook towards his farthest enemy, dragging the victim back and dealing [150/220/290] damage.  Pudge and allies 1 cell away will direct their attacks to the victim until it dies. (cd 10, mana 100)
- Outlanders 2019-12-06: Meat Hook: Pudge launches a bloody hook towards his farthest enemy, dragging the victim back and dealing [150/220/290] damage.  Pudge and allies 1 cell away will direct their attacks to the victim until it dies. (cd [20/18/15], mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Health changed from [1,500/3,750/7,010] to [1,650/3,300/6,600]
  - Attack Damage Minimum changed from [45, 90, 135] to [45, 90, 180]
  - Attack Damage Maximum changed from [55, 110, 165] to [55, 110, 220]
- June 18, 2020
  - Fixed an issue with Pudge's 3 Star Effect (Flesh Heap) where magic resistance could sometimes go negative.
- June 12, 2020
  - At three stars, Pudge becomes a Flesh Heap. Pudge gains 500 Max HP and 10% Magic Resistance each time an enemy dies within 1 cell of him.
- November 21, 2019
  - Maximum Health changed from [2000, 5000, 11000] to [2000, 5000, 9350]
- November 07, 2019
  - Meat Hook:
  - Cooldown changed from [10, 10, 10] to [20, 18, 15].

</details>

### Queen of Pain
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Demon, Assassin, Poisoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Demon, Assassin | 550/1100/2200 | 55-60/110-120/220-240 | 1.2 | 3 | 0 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Demon, Assassin | 550/1100/2200 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T2 | Demon, Assassin | 1100/2200/4400 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Demon, Assassin | 1100/1980/4400 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T2 | Demon, Assassin | 825/1485/3300 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T2 | Demon, Assassin, Poisoner | 825/1650/3300 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 60 | 290 | yes |

Abilities (last wiki state):
- **Scream of Pain** (cooldown 6.00, mana 60, magical): The Queen of Pain lets loose a piercing scream around her, damaging all enemies up to 3 cells away for [200/300/400] damage.
  - Scream of Pain counts as separate projectiles that can be blocked by Barricade.
- **Queen of Pain 3 Star Effect**: At three stars, the Queen of Pain captivates nearby enemies, allowing her a 25% chance to evade attacks from enemies up to 2 cells away.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Scream Of Pain: The Queen of Pain lets loose a piercing scream around her, damaging all enemies up to 3 cells away for [200/300/400] damage. (cd 6, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- September 04, 2020
  - Maximum Mana changed from 100 to 60
  - Scream of Pain:
  - Mana Cost changed from 100 to 60
- August 27, 2020
  - Health changed from [825/1,485/3,300] to [825/1,650/3,300]
- June 18, 2020
  - At three stars, the Queen of Pain captivates nearby enemies, allowing her a 25% chance to evade attacks from enemies 2 cells away.
- March 19, 2020
  - Assassin blink duration changed from 1.0 to 1.25
- November 21, 2019
  - Maximum Health changed from [1100, 2200, 4400] to [1100, 1980, 4400]
- July 19, 2019
  - Armor changed from 0 to 5.

</details>

### Razor
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Primordial, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Primordial (beta version), Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.1 | 3 | 5 | 0 | 100 | 285 | yes |
| Pre-Big Update 2019-10-10 | T1 | Primordial, Mage | 500/1000/2000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 | yes |
| Big Update 2019-10-24 | T1 | Primordial, Mage | 1000/2000/4000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 | yes |
| Outlanders 2019-12-06 | T1 | Primordial, Mage | 1000/2000/4000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 | yes |
| Season One 2020-02-25 | T1 | Primordial, Mage | 750/1500/3000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 | yes |
| Final 2020-11-19 | T1 | Primordial, Mage | 750/1500/3000 | 40-50/80-100/160-200 | 1.1 | 3 | 5 | 0 | 100 | 285 | no |

Abilities (last wiki state):
- **Plasma Field** (cooldown 10.00, mana 100, magical): Razor releases a ring of plasma that grows in power as it expands, but also zaps on contraction, slowing and dealing up to [150/200/250] damage to enemy units up to 3 cells away. Damage and slow increases with distance from Razor. The Plasma Field hits each unit twice, once in each direction.
  - Has a minimum damage of 40.
- **Razor 3 Star Effect**: At three stars, each time Razor attacks an enemy, he steals 7 Attack Damage, up to 10 times. This is reset for Razor, but not the enemy, when changing targets.
  - Can steal up to 70 damage.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Plasma Field: Razor releases a ring of plasma that grows in power as it expands, but also zaps on contraction, slowing and dealing up to [100/175/250] to damage to enemy units up to 3 cells away.  Damage and slow increases with distance from Razor.  The Plasma Field hits each unit twice, once in each direction. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Plasma Field: Razor releases a ring of plasma that grows in power as it expands, but also zaps on contraction, slowing and dealing up to [50/100/150] to damage to enemy units up to 3 cells away.  Damage and slow increases with distance from Razor.  The Plasma Field hits each unit twice, once in each direction. (cd 10, mana 100)
- Outlanders 2019-12-06: Plasma Field: Razor releases a ring of plasma that grows in power as it expands, but also zaps on contraction, slowing and dealing up to [100/200/300] to damage to enemy units up to 3 cells away.  Damage and slow increases with distance from Razor.  The Plasma Field hits each unit twice, once in each direction. (cd 10, mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- May 14, 2020
  - At three stars, Razor steals 7 attack damage from his target on each successful attack, up to a maximum of 70 damage. This is reset for Razor, but not the target, on switching targets.
- January 16, 2020
  - Plasma Field:
  - Minimum Damage changed from 25 to 40
  - Maximum Damage changed from [100, 200, 300] to [150, 200, 250]
- November 07, 2019
  - Plasma Field:
  - Minimum damage changed from 10 to 25.
  - Maximum damage changed from [50, 100, 150] to [100, 200, 300].
- July 19, 2019
  - Maximum Health changed from [750, 1500, 3000] to [500, 1000, 2000].
  - Attack Damage Minimum changed from [50, 100, 200] to [40, 80, 160].
  - Gold Cost changed from 3 to 1.
  - Draft Tier changed from 3 to 1.
  - Attack Damage Maximum changed from [60, 120, 240] to [50, 100, 200].
  - Plasma Field:
  - Max Damage changed from [100, 175, 250] to [50, 100, 150].

</details>

### Rubick
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Mage, Magus.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T4 | Mage, Magus | 1300/2600/5200 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 20 | 100 | 310 | yes |

Abilities (last wiki state):
- **Fade Bolt** (cooldown [10.00/8.00/6.00], mana 100, magical): Rubick creates a powerful stream of arcane energy that strikes a targets and jumps 4 additional times, dealing [75/125/175] damage and reducing their attack damage by 20% for 5 seconds.
- **Spell Steal**: Passive: Whenever an enemy within [1/2/3] cells casts a spell, Rubick will cast a copy of that spell at his current star level.

### Sand King
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 5, Savage, Insect.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Savage (beta version), Assassin | 550/1100/2200 | 45-60/90-120/180-240 | 1.4 | 1 | 10 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T4 | Savage, Assassin | 750/1500/3200 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T4 | Savage, Insect | 1500/3000/6400 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T4 | Savage, Insect | 1500/3000/6400 | 70-80/140-160/270-320 | 1.4 | 1 | 10 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T5 | Savage, Insect | 1500/3000/6000 | 70-80/140-160/270-320 | 1.3 | 1 | 10 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T5 | Savage, Insect | 1500/3000/6000 | 70-80/140-160/270-320 | 1.3 | 1 | 10 | 0 | 100 | 290 | no |

Abilities (last wiki state):
- **Burrowstrike** (cooldown [12.00/10.00/8.00], mana 100, magical): Sand King burrows down into the ground and tunnels forward, hitting enemies for [200/325/450] damage and stunning them for 2.5 seconds. Applies a stack of Caustic Finale to all enemies hit.
  - Burrowstrike has a range of 4 and targets the furthest enemy (in fields; horizontals, verticals and diagonals counting the same).
- **Caustic Finale** (magical): Passive: Whenever Sand King hits an enemy they are poisoned for 3.5 seconds. When the poison expires, it bursts for [50/100/200] damage and slows the attack speed of all enemies adjacent to the target by -35 for 1.75 seconds. Damage is halved if the target is still alive.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Burrowstrike: Sand King burrows into the ground and tunnels forward, hitting enemies for [150/250/350] damage and stunning them for 2 seconds.  Applies a stack of Caustic Finale to all enemies hit.  Caustic Finale  PASSIVE: Whenever Sand King hits an enemy he reduces his targets attack speed for 3 seconds.  If a target dies while Caustic Finale is active it will explode, damaging its allies 1 cell away. (cd [12/10/8], mana 100) / sandking_caustic_finale: description unknown (cd -, mana -)
- Big Update 2019-10-24: Burrowstrike: Sand King burrows into the ground and tunnels forward, hitting enemies for [150/250/350] damage and stunning them for 2 seconds. Applies a stack of Caustic Finale to all enemies hit. (cd [12/10/8], mana 100) / Caustic Finale: PASSIVE: Whenever Sand King hits an enemy they are poisoned for 3.5 seconds. When the poison expires, it bursts for [50/100/200] damage and slows the attack speed of all enemies adjacent to the target by -35 for 1.75 seconds. Damage is halved if the target is still alive. (cd -, mana -)

<details><summary>Wiki change log (7 patches)</summary>

- April 23, 2020
  - Attack Rate changed from 0.714 to 0.769
  - Maximum Health changed from [1300, 2600, 5200] to [1500, 3000, 6000]
- February 25, 2020
  - Tier changed from 4 to 5
  - Maximum Health changed from [1125, 2250, 4800] to [1300, 2600, 5200]
  - Burrowstrike:
  - Damage changed from [150, 250, 350] to [200, 325, 450]
  - Stun Duration changed from 2 to 2.5
- November 21, 2019
  - Fixed Caustic Finale going off after every attack.
- October 24, 2019
  - Alliances changed from Savage Assassin to Savage Insect
- September 05, 2019
  - Caustic Finale:
  - Fixed an issue where duration was not set properly, leading to Caustic Finale doing the timer-expired rather than the on-death damage much of the time.
- August 15, 2019
  - Caustic Finale:
  - Reworked: Whenever Sand King damages an enemy with an attack or ability, the target is poisoned for 3.5 seconds. When the poison expires, it bursts and deals damage and slows the attack speed of all enemies adjacent to the target. Damage is halved if the target is still alive when the poison expires.
  - Attack Slow Duration changed from 3.5 to 1.75.
  - AoE Damage if the target dies changed from [60, 110, 160] to [50, 100, 200]
  - AoE Damage on Expiration changed from [55, 100, 190] to [25, 50, 100]
- July 19, 2019
  - Maximum Health changed from [550, 1100, 2200] to [750, 1500, 3200].
  - Attack Damage Minimum changed from [45, 90, 180] to [70, 140, 270].
  - Gold Cost changed from 3 to 4.
  - Draft Tier changed from 3 to 4.
  - Attack Damage Maximum changed from [60, 120, 240] to [80, 160, 320].
  - Caustic Finale:
  - Attack Slow changed from -30 to -35.
  - Attack Slow duration changed from 3 to 3.5.
  - Damage on expiration changed from [45, 90, 180] to [55, 100, 190].
  - Damage on kill changed from [50, 100, 150] to [60, 110, 160].
  - Burrowstrike now displaces the target unit from its cell.

</details>

### Shadow Demon
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Heartless, Demon.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T1 | Heartless, Demon | 1200/2400/4800 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 | yes |
| Outlanders 2019-12-06 | T1 | Heartless, Demon | 1200/2400/4800 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 | yes |
| Season One 2020-02-25 | T1 | Heartless, Demon | 900/1800/3600 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 | yes |
| Final 2020-11-19 | T1 | Heartless, Demon | 900/1800/3600 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 | yes |

Abilities (last wiki state):
- **Demonic Purge** (cooldown [20/15/10], mana 100, magical): Repeatedly purges the target enemy unit, removing positive buffs, applying break, and slowing the target for 5 seconds. The unit slowly regains its movement/attack speed until the end of the duration, upon which [500/800/1100] damage is dealt.
- **Shadow Demon 3 Star Effect**: Shadow Demon applies Soul Catcher to the target of Demonic Purge. The unit loses 30% of its current health at the start, and gains half that back after the purge finishes if it survives.
  - Directly manipulates enemies' health. Does not deal damage.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Demonic Purge: Repeatedly purges the target enemy unit, removing positive buffs, applying Break, and slowing the target for 5 seconds. The unit slowly regains its movement/attack speed until the end of the duration, upon which [500/800/1100] damage is dealt. (cd [20/15/10], mana 100)

<details><summary>Wiki change log (1 patches)</summary>

- May 28, 2020
  - 3 Star Effect: Shadow Demon gains Soul Catcher, which activates during Demonic Purge. At the start of Demonic Purge, the affected unit loses 30% of its current health. At the end of the Purge, if the unit surives, it regains half that amount.

</details>

### Shadow Fiend
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Demon, Warlocks.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Demon, Warlock | 450/900/1800 | 70-80/140-160/280-320 | 1.1 | 4 | 5 | 0 | 100 | 305 | yes |
| Pre-Big Update 2019-10-10 | T3 | Demon, Warlock | 450/900/1800 | 70-80/140-160/336-384 | 1.1 | 3 | 5 | 0 | 100 | 305 | yes |
| Big Update 2019-10-24 | T3 | Demon, Warlock | 900/1800/3600 | 70-80/140-160/336-384 | 1.1 | 3 | 5 | 0 | 100 | 305 | yes |
| Outlanders 2019-12-06 | T3 | Demon, Warlock | 900/1800/3600 | 77-88/154-176/369-422 | 1.1 | 3 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T3 | Demon, Warlock | 675/1350/2700 | 77-88/154-176/369-422 | 1.1 | 3 | 5 | 0 | 100 | 305 | yes |
| Final 2020-11-19 | T3 | Demon, Warlock | 675/1350/2700 | 77-88/154-176/369-422 | 1.1 | 3 | 5 | 0 | 100 | 305 | no |

Abilities (last wiki state):
- **Requiem of Souls** (cooldown 20.00, mana 100, magical): Shadow fiend channels the souls of the damned and unleashes demonic energy all around him, dealing [100/175/250] damage to anyone caught in the blast.
  - Requiem of Souls deals its damage in lines and can hit enemies multiple times.
  - The line count is [6/8/10].
- **Shadow Fiend 3 Star Effect**: At three stars, Shadow Fiend gains Presence of the Dark Lord. Enemies within 2 cells suffer -8 armor due to Shadow Fiend's terrifying presence.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Requiem of Souls: Shadow Fiend channels the souls of the damned and unleashes demonic energy all around him, dealing [100/175/250] damage to anyone caught in the blast. (cd 20, mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- July 23, 2020
  - 3 Star Effect: Shadow Fiend gains Presence of the Dark Lord. Enemies within 2 cells suffer -8 armor due to Shadow Fiend's terrifying presence.
- November 21, 2019
  - Attack Damage Minimum changed from [70, 140, 336] to [77, 154, 369]
  - Attack Damage Maximum changed from [80, 160, 384] to [88, 176, 422]
  - Requiem of Souls:
  - Max projectile width changed from 256 to 425
- August 01, 2019
  - Attack Range changed from 4 to 3
  - Requiem of Souls:
  - Projectile Start Width changed from 125 to 128
  - Projectile End Width changed from 425 to 256
- July 19, 2019
  - Attack Damage Minimum changed from [70, 140, 280] to [70, 140, 336].
  - Attack Damage Maximum changed from [80, 160, 320] to [80, 160, 384].

</details>

### Shadow Shaman
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Troll, Summoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Troll, Shaman (old, beta) | 550/1100/2200 | 40-50/80-100/160-200 | 1.6 | 3 | 5 | 0 | 100 | 285 | yes |
| Pre-Big Update 2019-10-10 | T1 | Troll, Shaman (old, beta) | 550/1100/2200 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 100 | 285 | yes |
| Big Update 2019-10-24 | T1 | Troll, Shaman (old, beta) | 1100/2200/4400 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 70 | 285 | yes |
| Outlanders 2019-12-06 | T1 | Troll, Shaman (old, beta) | 1100/2200/4400 | 40-50/80-100/160-200 | 1.25 | 3 | 5 | 0 | 70 | 285 | yes |
| Season One 2020-02-25 | T3 | Troll, Summoner | 850/1700/3400 | 50-60/100-120/200-240 | 1.25 | 3 | 5 | 0 | 70 | 285 | yes |
| Final 2020-11-19 | T3 | Troll, Summoner | 850/1700/4080 | 50-60/100-120/240-288 | 1.25 | 3 | 5 | 0 | 70 | 285 | yes |

Abilities (last wiki state):
- **Mass Serpent Ward** (cooldown 20.0, mana 70): Shadow Shaman traps an enemy unit by summoning up to 4 Serpent Wards.
  - One serpent ward is summoned in each of the four cardinal directions surrounding the target.
  - A ward will not be summoned if the space is occupied or off of the board.
  - Tries to target enemies and position wards such that all 4 can be summoned.
- **Shadow Shaman 3 Star Effect**: Shadow Shaman hexes the enemy unit targeted by Mass Serpent Ward for 7 seconds.

Older abilities (wiki):
- **Hex** (cooldown 8.00, mana 70): Shadow Shaman transforms an enemy unit into a harmless creature, disabling their attacks and abilities for [4/5/6] seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Hex: Shadow Shaman transforms an enemy unit into a harmless creature, disabling their attacks and abilities for [4/6/8] seconds. (cd 10, mana 100)
- Big Update 2019-10-24: Hex: Shadow Shaman transforms an enemy unit into a harmless creature, disabling their attacks and abilities for [4/6/8] seconds. (cd 8, mana 70)

<details><summary>Wiki change log (11 patches)</summary>

- November 19, 2020
  - 3 Star Effect:
  - Hex duration changed from 5 to 7
- August 27, 2020
  - Health changed from [850/1,700/3,400] to [850/1,700/4,080]
  - Attack Damage Minimum changed from [50, 100, 200] to [50, 100, 240]
  - Attack Damage Maximum changed from [60, 120, 240] to [60, 120, 288]
- May 08, 2020
  - Mass Serpent Ward:
  - Chicken is back on the menu! At 3 stars, Shadow Shaman Hexes the enemy unit for 5 seconds in addition to summoning wards.
- April 23, 2020
  - Maximum Health changed from [750, 1500, 3000] to [850, 1700, 3400]
  - Attack Damage Minimum changed from [40, 80, 160] to [50, 100, 200]
  - Attack Damage Maximum changed from [50, 100, 200] to [60, 120, 240]
- March 19, 2020
  - Maximum Mana changed from 100 to 70
  - Mass Serpent Ward:
  - Cooldown changed from 60 to 20
  - Mana Cost changed from 100 to 70
  - Shadow Shaman will try to target an enemy, and position wards, such that all 4 wards can be summoned.
  - Ward Maximum Health changed from [300, 400, 500] to [360, 480, 600]
- February 25, 2020
  - Tier changed from 1 to 3
  - Alliances changed from Troll Druid to Troll Summoner
  - Maximum Mana changed from 70 to 100
  - Maximum Health changed from [825, 1650, 3300] to [750, 1500, 3000]
  - Removed ability Hex
  - Added ability Mass Serpent Ward:
  - Shadow Shaman traps an enemy unit by summoning up to 4 Serpent Wards.
  - Serpent Ward:
  - Maximum Health [ 300, 400, 500 ]
  - Attack Damage Minimum [ 45, 60, 75 ]
  - Attack Damage Maximum [ 60, 75, 90 ]
  - Attack Rate 1.4
  - Magic Resist 50
- January 16, 2020
  - Hex:
  - Will not hex the same target twice unless all other targets have been hexed already.
- December 18, 2019
  - Alliances changed from Troll Shaman to Troll Druid
  - Hex:
  - Duration changed from [4, 6, 8] to [4, 5, 6]
- October 24, 2019
  - Maximum Mana changed from 100 to 70
  - Hex:
  - Cooldown changed from 10 to 8
  - Mana Cost changed from 100 to 70
- September 26, 2019
  - Attack Rate changed from 0.625 to 0.8.
- August 23, 2019
  - Hex will no longer be cast on an already-hexed enemy.

</details>

### Slardar
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Scaled, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Scaled, Warrior | 650/1300/2600 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Scaled, Warrior | 650/1300/2600 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |
| Big Update 2019-10-24 | T2 | Scaled, Warrior | 1300/2600/5200 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Scaled, Warrior | 1300/2600/5200 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |
| Season One 2020-02-25 | T2 | Scaled, Warrior | 975/1950/3900 | 70-100/140-200/280-400 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |
| Final 2020-11-19 | T1 | Scaled, Warrior | 900/1800/3600 | 60-85/120-170/240-340 | 1.2 | 1 | 5 | 0 | 75 | 290 | yes |

Abilities (last wiki state):
- **Corrosive Haze** (cooldown 3.00, mana 75): Slardar modifies enemy armor by [-10/-20/-30] for 20 seconds.
  - Slardar prefers to cast Corrosive Haze on his current attack target.
- **Slardar 3 Star Effect** (physical): Slardar bashes the target after casting Corrosive Haze, dealing 100 Physical damage and stunning for 1.5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Corrosive Haze: Slardar reduces enemy armor by [-15/-30/-45] for 20 seconds. (cd 3, mana 75)
- Big Update 2019-10-24: Corrosive Haze: Slardar modifies enemy armor by [-15/-30/-45] for 20 seconds. (cd 3, mana 75)

<details><summary>Wiki change log (4 patches)</summary>

- November 19, 2020
  - Attack Damage Minimum changed from [50, 100, 200] to [60, 120, 240]
  - Attack Damage Maximum changed from [80, 160, 320] to [85, 170, 340]
- August 27, 2020
  - Tier changed from 2 to 1
  - Health changed from [975/1,950/3,900] to [900/1,800/3,600]
  - Attack Damage Minimum changed from [70/140/280] to [50/100/200]
  - Attack Damage Maximum changed from [100/200/400] to [80/160/320]
  - Corrosive Haze:
  - Armor reduction changed from [-15/-30/-45] to [-10/-20/-30]
- May 14, 2020
  - Corrosive Haze
  - At three stars, Slardar also bashes the target for 100 Physical damage and a stun of 1.5 seconds.
- June 27, 2019
  - Amp Damage is now purgeable.

</details>

### Slark
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Scaled, Assassin.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Scaled, Assassin | 500/1000/2000 | 50-60/72-92/105-145 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T3 | Scaled, Assassin | 500/1000/2400 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T3 | Scaled, Assassin | 1000/2000/4800 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T3 | Scaled, Assassin | 1000/1800/4800 | 50-60/72-92/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T4 | Scaled, Assassin | 900/1800/3600 | 60-70/90-110/126-174 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |
| Final 2020-11-19 | T3 | Scaled, Assassin | 800/1600/3840 | 50-60/78-98/138-178 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Essence Shift**: Passive: Slark increases his damage by [3/4/5] and his attack speed by [3/4/5] with each attack.
  - The stacks from Essence Shift last for the entire round.
  - Missed attacks will not grant stacks.
- **Pounce**: Passive: After landing from a leap, Slark disarms his target for 3 seconds.
- **Slark 3 Star Effect** (cooldown 7.00, magical): Slark gains Dark Pact. After a short delay, Slark sacrifices some of his blood, purging negative effects and damaging enemies within 2 cells for a total of 300 damage. Slark himself takes 50% of the damage too.
  - The self-inflicted damage can be reduced or amplified, but can never kill Slark.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Essence Shift: PASSIVE: Every attack by Slark lowers his target's damage by [3/4/5] and attack speed by [3/5/7].  Slark's damage and attack speed is increased by the same amount drained from his target. (cd -, mana -)
- Pre-Big Update 2019-10-10: Essence Shift: PASSIVE: Every attack by Slark lowers his target's damage by [3/4/5] and attack speed by [3/5/7].  Slark's damage and attack speed is increased by the same amount drained from his target. (cd -, mana -) / slark_pounce: description unknown (cd -, mana -)
- Big Update 2019-10-24: Essence Shift: PASSIVE: Every attack by Slark lowers his target's damage by [3/4/5].  Slark's damage is increased by [3/4/5] and his attack speed is increased by [1/2/3]. (cd -, mana -) / Pounce: PASSIVE: After landing from a leap, Slark disarms his target for 3 seconds. (cd -, mana -)

<details><summary>Wiki change log (7 patches)</summary>

- September 04, 2020
  - Essence Shift:
  - Stolen Damage changed from [2, 3, 4] to [3, 4, 5]
  - Stolen Attack Speed changed from [2.0, 3.0, 3.0] to [3.0, 4.0, 5.0]
- August 27, 2020
  - Tier changed from 4 to 3
  - Health changed from [900/1,800/3,600] to [800/1,600/3,840]
  - Attack Damage Minimum changed from [60, 90, 126] to [50, 78, 138]
  - Attack Damage Maximum changed from [70, 110, 174] to [60, 98, 178]
  - Health regen changed from 5 to 0
  - Essence Shift:
  - Bonus Damage gain per attack changed from [3/5/6] to [2/3/4]
  - Attack Speed gain per attack changed from [3/4/5] to [2/3/3]
  - 3 Star Effect: Slark gains Dark Pact. After a short delay, Slark sacrifices some of his life blood, purging negative effects and damaging enemies nearby. Slark himself takes some of the damage too.
- January 24, 2020
  - Attack Damage Minimum changed from [50, 72, 126] to [60, 90, 126]
  - Attack Damage Maximum changed from [60, 92, 174] to [70, 110, 174]
  - Essence Shift:
  - Bonus Damage gain per attack changed from [3, 4, 5] to [3, 5, 6]
  - Attack Speed gain per attack changed from [2.0, 3.5, 5.0] to [3.0, 4.0, 5.0]
- December 18, 2019
  - Gold Cost changed from 3 to 4
  - Maximum Health changed from [750, 1350, 3600] to [900, 1800, 3600]
  - Essence Shift:
  - Stolen Attack Speed changed from [1, 2, 3] to [2, 3.5, 5]
- November 21, 2019
  - Maximum Health changed from [1000, 2000, 4800] to [1000, 1800, 4800]
  - essence shift no longer reduces attack speed and damage from the target (Slark still gains the effects)
- October 24, 2019
  - Essence Shift:
  - Stolen Attack Speed changed from [3, 5, 7] to [1, 2, 3]
- July 19, 2019
  - Maximum Health changed from [500, 1000, 2000] to [500, 1000, 2400].
  - Attack Damage Minimum changed from [50, 72, 105] to [50, 72, 126].
  - Attack Damage Maximum changed from [60, 92, 145] to [60, 92, 174].

</details>

### Snapfire
- Status: In the final pool. First seen in data: Outlanders 2019-12-06.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Brawny, Dragon.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Outlanders 2019-12-06 | T1 | Dragon, Inventor | 1250/2500/5000 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 | yes |
| Season One 2020-02-25 | T1 | Brawny, Dragon | 935/1875/3750 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 | yes |
| Final 2020-11-19 | T1 | Brawny, Dragon | 950/1900/3800 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 | yes |

Abilities (last wiki state):
- **Ol' Scatterblast** (cooldown 0.00, mana 0, physical): After 4 basic attacks, Snapfire readies her shotgun, blasting an enemy within 2 cells for [150/250/350] in a cone 3 cells wide and slows the attack of affected units by [-10/-15/-20]. The effect lasts [1/2/4] seconds.
  - Doesn't trigger Refresher Orb.
  - Includes missed attacks.
- **Snapfire 3 Star Effect**: Ol' Scatterblast also reduces enemy armor by 5 for the duration of the debuff.
- **Mortimer Kisses** (cooldown 60.00, mana 100, magical): Unlocked by the Dragon alliance bonus Mortimer spits [3/4/5] globs at enemies up to 5 cells away. Each glob deals [50/80/150] damage to all enemies within 1 cell of the impact and lights them on fire for 5 seconds. They also lose 20 Attack Speed for [1/2/3] seconds.
  - Snapfire cannot attack while casting Mortimer Kisses.
  - The ability is interrupted if Snapfire gets stunned, silenced, hexed or cycloned by Eul's Scepter.

Ability text rendered from beta game data (shown when it changed):
- Outlanders 2019-12-06: Ol' Scatterblast: After 4 basic attacks Snapfire readies her shotgun, blasting a nearby enemy for [150/250/350] in a cone and slows the attack of affected units by [-15/-20/-25]% (cd 1, mana 0)

<details><summary>Wiki change log (7 patches)</summary>

- August 27, 2020
  - Health changed from [935/1,875/3,750] to [950/1,900/3,800]
- August 02, 2019
  - 3 Star Effect: Ol' Scatterblast also reduces enemy armor by 5 for the duration of the debuff.
  - Ol' Scatterblast:
  - Debuff duration changed from [1.0, 2.0, 3.0] to [1.0, 2.0, 4.0]
  - Debuff Attack Speed loss changed from [15, 20, 25] to [10, 15, 20]
- June 12, 2020
  - Mortimer Kisses
  - Damage changed from [100, 150, 200] to [50, 80, 150]
- February 25, 2020
  - Alliances changed from Dragon Inventor to Brawny Dragon
- December 18, 2019
  - Ol' Scatterblast:
  - No longer triggers Refresher Orb.
- December 05, 2019
  - Ol' Scatterblast:
  - Fixed an issue where the blast cone could be inconsistent in some cases.
- November 27, 2019
  - Mortimer Kisses:
  - Damage changed from [150, 250, 450] to [100, 150, 200]
  - Sets targets on fire.

</details>

### Sniper
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Scrappy, Deadeye, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Deadeye, Hunter | 450/900/1800 | 125-150/180-210/330-360 | 1.6 | 7 | 5 | 0 | 100 | 285 | yes |
| Pre-Big Update 2019-10-10 | T3 | Scrappy, Deadeye, Hunter | 450/900/2160 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 | yes |
| Big Update 2019-10-24 | T3 | Scrappy, Deadeye, Hunter | 900/1800/4320 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 | yes |
| Outlanders 2019-12-06 | T3 | Scrappy, Deadeye, Hunter | 900/1800/4320 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 | yes |
| Season One 2020-02-25 | T3 | Scrappy, Deadeye, Hunter | 675/1350/3240 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 | no |
| Final 2020-11-19 | T3 | Scrappy, Deadeye, Hunter | 675/1350/3240 | 125-150/180-210/396-432 | 1.6/1.6/1.3 | 7 | 5 | 0 | 100 | 285 | no |

Abilities (last wiki state):
- **Assassinate** (cooldown 12.00, mana 100, magical): Sniper takes aim at an enemy unit for 2 seconds, then fires a devastating shot that deals [1000/2000/3000] damage and mini-stuns the target. Underlords will not be targeted.
  - Assassinate targets the highest health unit that has current HP lower than Assassinate's damage. As a result, Assassinate is lethal to targets with no Magic Resistance.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Assassinate: Sniper takes aim at an enemy unit for 2 seconds, then fires a devastating shot that deals [1000/2000/3000] damage and mini-stuns the target. (cd 10, mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- January 16, 2020
  - Assassinate:
  - Cooldown changed from 10 to 12
  - If no units are alive on the enemy side except the Underlord, Sniper will be able to cast Assassinate on the Underlord after all. In all other circumstances Sniper will still not cast on the enemy Underlord.
- January 09, 2020
  - Assassinate will no longer target Underlords.
- August 16, 2019
  - Sniper's cooldown actually cools down.
  - Prevented Sniper casting Assassinate on dead units.
- August 15, 2019
  - Is now Scrappy in addition to Deadeye and Hunter.
  - Assassinate:
  - Will not restart the cast if the target dies. He will simply switch to the next valid target and go from there.
- July 19, 2019
  - Attack Speed changed from 0.63 to [0.63, 0.63, 0.77].
  - Maximum Health changed from [450, 900, 1800] to [450, 900, 2160].
  - Attack Damage Minimum changed from [125, 180, 330] to [125, 180, 396].
  - Attack Damage Maximum changed from [150, 210, 360] to [150, 210, 432].

</details>

### Spectre
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Void, Demon.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T3 | Void, Demon | 950/1900/4560 | 55-65/110-130/264-312 | 0.9 | 1 | 5 | 0 | 70 | 310 | yes |

Abilities (last wiki state):
- **Spectral Dagger** (cooldown [15.00/13.00/12.00], mana 70, magical): Spectre flings a dagger at an enemy hero, dealing [300/450/600] damage and drawing a Shadow Path which slows the movement speed of enemy units by 50 and Attack Speed by 50 for 4 seconds. Spectre runs to attack this unit and gains 175 Attack Speed and 100% bonus movement speed and phasing movement for 4 seconds.
- **Spectre 3 Star Effect** (magical): While Spectre is alive, whenever a Demon casts a spell all Allied Demons erupt in flames and wreathe their targets in fire, doing 200 damage in a wide swathe and setting units hit on fire.

<details><summary>Wiki change log (2 patches)</summary>

- November 19, 2020
  - Spectral Dagger:
  - Damage changed from [300, 500, 700] to [300, 450, 600]
  - Cooldown changed from [15, 13, 11] to [15, 13, 12]
  - Bonus Attack Speed changed from 200 to 175
- September 04, 2020
  - Spectral Dagger:
  - Duration of the buff changed from 5.0 to 4.0

</details>

### Spirit Breaker
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Savage, Brute.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T2 | Savage, Brute | 1200/2400/4800 | 55-70/110-140/220-280 | 1.5 | 1 | 5 | 0 | 100 | 330 | yes |

Abilities (last wiki state):
- **Charge of Darkness** (cooldown 15.00, mana 100, physical): Spirit Breaker charges towards a distant enemy, hitting it and enemies along the way for [100/150/200] damage and stunning them for [1.0/1.3/1.7] seconds.
- **Spirit Breaker 3 Star Effect** (magical): Spirit Breaker gains Greater Bash. Spirit Breaker's attacks have a 25% chance of dealing 150 bonus damage and stunning the target for 2 seconds.

<details><summary>Wiki change log (2 patches)</summary>

- November 19, 2020
  - Charge of Darkness:
  - Fixed a bug where Spirit Breaker would not damage the target of the Charge.
  - Damage per bash changed from [100, 175, 250] to [100, 150, 200]
- September 04, 2020
  - Charge of Darkness:
  - Charge now damages the target enemy just like enemies along the way.

</details>

### Storm Spirit
- Status: In the final pool. First seen in data: Outlanders 2019-12-06.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Spirit, Mage.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Outlanders 2019-12-06 | T2 | Spirit, Mage | 1000/2000/4000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 | yes |
| Season One 2020-02-25 | T2 | Spirit, Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 | yes |
| Final 2020-11-19 | T2 | Spirit, Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 | yes |

Abilities (last wiki state):
- **Ball Lightning** (cooldown [5/4/3], mana 40, magical): Storm Spirit becomes volatile electricity, charging across the battlefield and dealing [80/120/180] damage to each unit he passes through, plus [15/20/25] damage per cell he has traveled.
- **Storm Elemental**: Passive: When Storm Spirit executes a Delta Slam, he disarms affected enemy units for 3 seconds.
- **Storm Spirit 3 Star Effect** (magical): Storm gains Overload. Casting a spell creates an electrical charge which is released in a burst on Storm's next attack, dealing 150 magical damage and slowing the target's Attack Speed by 50 for 1.5 seconds. A Triangular Attack by Storm counts as an attack, in which case all units Storm hits in the Attack take the damage and are slowed.

Ability text rendered from beta game data (shown when it changed):
- Outlanders 2019-12-06: Ball Lightning: Storm Spirit becomes volatile electricity, charging across the battlefield and dealing [70/120/225] damage to each unit he passes through, plus [10/20/30] damage per cell he has traveled. (cd [5/4/3], mana 40) / Storm Elemental: PASSIVE: When Storm Spirit executes a Delta Slam, he disarms affected enemy units for 3 seconds. (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- August 06, 2020
  - Storm gains Overload. Casting a spell creates an electrical charge which is released in a burst on Storm's next attack, dealing 150 magical damage and slowing the target's Attack Speed by 50 for 1.5 seconds. A Triangular Attack by Storm counts as an attack, in which case all units Storm hits in the Attack take the damage and are slowed.
- May 08, 2020
  - Ball Lightning:
  - Damage changed from [90, 140, 210] to [80, 120, 180].
- January 16, 2020
  - Ball Lightning:
  - Base Damage changed from [70, 120, 225] to [90, 140, 210]
  - Extra Damage (per cell traveled) changed from [10, 20, 30] to [15, 20, 25]
- November 27, 2019
  - Ball Lightning:
  - Damage changed from [70, 140, 300] to [70, 120, 225]
  - Damage per cell changed from [10, 20, 40] to [10, 20, 30]

</details>

### Sven
- Status: In the final pool. First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Rogue, Knight, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T5 | Human, Scaled, Knight | 1900/3800/7600 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 200 | 300 | yes |
| Outlanders 2019-12-06 | T5 | Human, Scaled, Knight | 1900/3800/7600 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 200 | 300 | yes |
| Season One 2020-02-25 | T4 | Human, Scaled, Knight | 1425/2850/5700 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 150 | 300 | yes |
| Final 2020-11-19 | T4 | Rogue, Knight, Swordsman | 1425/2850/5700 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 80 | 300 | yes |

Abilities (last wiki state):
- **God's Strength** (cooldown 60, mana 80): Sven enrages and gains [100/150/200]% damage for 60 seconds.
  - Does not stack, casting this ability again while it's still active only refreshes the duration.
- **Cleave**: Passive: This unit's melee attacks strike nearby enemies for [30/45/70]%.
- **Ace effect (2019-08-30 to 2020-02-25, Knight Ace):** When Sven casts God's Strength, all allied Knights lose their Knight Alliance bonus and gain 50% damage per Knight Alliance level for the remainder of the round.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: God's Strength: Sven enrages and gains [100/150/200]% damage for 60 seconds. (cd 60, mana 200) / Great Cleave: PASSIVE: This unit's melee attacks strike nearby enemies for [30/45/70]%. (cd -, mana -) / Ace/alliance bonus (internal id sven_knight_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- September 04, 2020
  - Maximum Mana changed from 100 to 80
  - God's Strength:
  - Mana Cost changed from 100 to 80
- August 27, 2020
  - Alliances changed from Human Scaled Knight to Rogue Knight Swordsman.
- January 24, 2020
  - Maximum Mana changed from 200 to 150
  - God's Strength:
  - Mana Cost changed from 200 to 150
- December 18, 2019
  - Gold Cost changed from 5 to 4
  - Ace tier effect removed

</details>

### Techies
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 5, Scrappy, Inventor.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T5 | Scrappy, Inventor (beta version) | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | yes |
| Pre-Big Update 2019-10-10 | T5 | Scrappy, Inventor | 1000/2000/4000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | yes |
| Big Update 2019-10-24 | T5 | Scrappy, Inventor | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | yes |
| Outlanders 2019-12-06 | T5 | Scrappy, Inventor | 2000/4000/8000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | yes |
| Season One 2020-02-25 | T5 | Scrappy, Inventor | 1500/3000/6000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | no |
| Final 2020-11-19 | T5 | Scrappy, Inventor | 1500/3000/6000 | 45-55/90-110/180-220 | 1.5 | 4 | 10 | 0 | 100 | 310 | no |

Abilities (last wiki state):
- **Remote Mines** (cooldown 10.00, mana 100, physical): The gang plants an explosive that detonates after 3 seconds and deals [700/1100/1500] damage to targets [3/3/4] cells away. Does half-damage to enemies half the max distance away.
  - Techies always attempts to put their bomb next to the enemy he's attacking.
  - The cast range is 2, so if the enemy they're attacking is further away than that, they will place the bomb next to themselves.
  - They will prioritize empty vertical and horizontal fields first,
  - If there are none, they will place the bomb in an empty diagonal field,
  - If there are none, they will continue auto-attacking until there is.
  - Deals 50% damage to enemies that are not within range [1/1/2] of the bomb when it explodes.
- **Ace effect (2019-08-30 to 2020-02-25, Inventor Ace):** Grants Inventor units Chain Reaction. If an enemy dies as a result of an Inventor Alliance explosion, they explode just as if they were a part of your Inventor Alliance, damaging nearby enemies.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Remote Mines: The gang plants an explosive that detonates after 3 seconds and deals [700/1100/1500] damage to targets 4 cells away.  Does half-damage to enemies half the max distance away. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Remote Mines: The gang plants an explosive that detonates after 3 seconds and deals [700/1100/1500] damage to targets [3/3/4] cells away.  Does half-damage to enemies half the max distance away. (cd 10, mana 100) / Ace/alliance bonus (internal id techies_inventor_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (4 patches)</summary>

- December 18, 2019
  - Removed from the game.
- August 30, 2019
  - Ace for the Inventor Alliance.
  - Ace Effect: Grants Inventor units Chain Reaction. If an enemy dies as a result of Unstable Reactor, they explode just as if they had Unstable Reactor.
- August 08, 2019
  - Fixed unexploded Techies Bombs doing player damage on round over.
- July 19, 2019
  - Remote Mines:
  - Radius changed from 4 to [3, 3, 4].

</details>

### Templar Assassin
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Vigilant, Void, Assassin.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Elusive, Assassin | 750/1500/3000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 | yes |
| Pre-Big Update 2019-10-10 | T4 | Elusive, Assassin | 750/1500/3000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 | yes |
| Big Update 2019-10-24 | T4 | Elusive, Assassin | 1500/3000/6000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 | yes |
| Outlanders 2019-12-06 | T4 | Elusive, Assassin | 1500/2700/6000 | 90-110/180-220/360-440 | 1.2 | 3 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T4 | Vigilant, Void, Assassin | 1125/2025/4500 | 90-110/180-220/360-440 | 1.375 | 3 | 5 | 0 | 100 | 305 | yes |
| Final 2020-11-19 | T4 | Vigilant, Void, Assassin | 1125/2250/4500 | 90-110/180-220/360-440 | 1.375 | 3 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Refraction** (cooldown 6.00, mana 100): Templar Assassin becomes elusive, avoiding damage and gaining +[40/60/80]% Attack Damage. The damage and avoidance effects are separate, and have [4/6/8] instances each.
  - Refraction has a duration of 6 seconds.
  - Refraction's damage threshold is 5.
  - Templar Assassin generates no mana from being attacked while Refraction is up.
- **Meld** (magical): Passive: After teleporting to her target, Templar Assassin does [50/100/150] extra damage on her first attack and lowers her target's armor by [2/4/6] for 5 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Refraction: Templar Assassin becomes elusive, avoiding damage and gaining [40/60/80]% bonus to her damage.  The damage and avoidance effects are separate, and have [4/6/8] instances each. (cd 6, mana 100)
- Pre-Big Update 2019-10-10: Refraction: Templar Assassin becomes elusive, avoiding damage and gaining [40/60/80]% bonus to her damage.  The damage and avoidance effects are separate, and have [4/6/8] instances each. (cd 6, mana 100) / templar_assassin_meld: description unknown (cd -, mana -)
- Big Update 2019-10-24: Refraction: Templar Assassin becomes elusive, avoiding damage and gaining [40/60/80]% bonus to her damage.  The damage and avoidance effects are separate, and have [4/6/8] instances each. (cd 6, mana 100) / Meld: PASSIVE: After teleporting to her target, Templar Assassin does [50/100/150] extra damage on her first attack and lowers her target's armor by [2/4/6] for 5 seconds. (cd -, mana -)
- Outlanders 2019-12-06: Refraction: Templar Assassin becomes elusive, avoiding damage and gaining +[40/60/80] Attack Damage.  The damage and avoidance effects are separate, and have [4/6/8] instances each. (cd 6, mana 100) / Meld: PASSIVE: After teleporting to her target, Templar Assassin does [50/100/150] extra damage on her first attack and lowers her target's armor by [2/4/6] for 5 seconds. (cd -, mana -)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Health changed from [1,500/2,700/6,000] to [1,125/2,250/4,500]
- April 03, 2020
  - Attack Rate changed from 0.833 to 0.727
- February 25, 2020
  - Alliances changed from Elusive Assassin to Vigilant Void Assassin
- December 18, 2019
  - Removed from the game.
- November 21, 2019
  - Maximum Health changed from [1500, 3000, 6000] to [1500, 2700, 6000]
- September 19, 2019
  - Added Meld:
  - When first attacking after teleporting, Templar Assassin fires a projectile that does [50, 100, 150] damage on impact and decreases her target's armor by [2, 4, 6] for 5 seconds.

</details>

### Terrorblade
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Demon, Hunter, Fallen.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Demon, Demon Hunter | 650/1300/2600 | 45-65/90-130/180-260 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 | yes |
| Pre-Big Update 2019-10-10 | T3 | Demon, Demon Hunter | 650/1300/3120 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 | yes |
| Big Update 2019-10-24 | T3 | Demon, Hunter | 1300/2600/6240 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 | yes |
| Outlanders 2019-12-06 | T3 | Demon, Hunter | 1300/2600/6240 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 10/20/30 | 100 | 310 | yes |
| Season One 2020-02-25 | T3 | Demon, Hunter | 975/1950/4680 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 30 | 80 | 310 | yes |
| Final 2020-11-19 | T3 | Demon, Hunter, Fallen | 975/1950/4680 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 30 | 80 | 310 | yes |

Abilities (last wiki state):
- **Metamorphosis** (cooldown 60.00, mana 80): Terrorblade transforms into a powerful demon, gaining a ranged attack of 4 cells as well as [50/100/200] bonus to damage and [50/75/100] to attack speed. Terrorblade also exchanges health values with another ally when this ability starts.
  - Terrorblade exchanges his health percentages with a friendly unit that has the highest health percentage at cast time.
  - If there is no friendly unit with a health percentage higher than his own, Terrorblade does not exchange health when casting Metamorphosis.
- **Terrorblade 3 Star Effect** (cooldown 6.00, mana 80, pure): At three stars, Terrorblade can summon demonic energy to gain +125% Pure Damage against enemy Demons. The bonus lasts 5 seconds each cast.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Metamorphosis: Terrorblade transforms into a powerful demon, gaining a ranged attack of 4 cells as well as [50/100/200]% bonus to damage and attack speed. Terrorblade also exchanges health values with another ally when this ability starts. (cd 60, mana 100)
- Big Update 2019-10-24: Metamorphosis: Terrorblade transforms into a powerful demon, gaining a ranged attack of 4 cells as well as [50/100/200] bonus to damage and [50/75/100] to attack speed. Terrorblade also exchanges health values with another ally when this ability starts. (cd 60, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Now also a Fallen.
- July 09, 2020
  - Terrorblade 3 Star Effect:
  - Pure Damage Bonus Percent vs Demons changed from 100 to 125
  - Now an active ability: duration 5 seconds, cooldown 6 seconds.
- May 21, 2020
  - Maximum Mana changed from 100 to 80
  - Magic Resistance changed from [10, 20, 30] to 30
  - Metamorphosis:
  - Mana Cost changed from 100 to 80
  - At three stars, Terrorblade gains +100% Pure Damage against Demons (or an enemy Champion with an active Demon Alliance).
- October 24, 2019
  - Alliances changed from Demon Demon Hunter to Demon Hunter
- September 19, 2019
  - Fixed an issue where Terrorblade could Sunder with the wrong unit at cast time, including dead units.
- July 19, 2019
  - Maximum Health changed from [650, 1300, 2600] to [650, 1300, 3120].
  - Attack Damage Minimum changed from [45, 90, 180] to [45, 90, 216].
  - Attack Damage Maximum changed from [65, 130, 260] to [65, 130, 312].

</details>

### Tidehunter
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Scaled, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T5 | Scaled, Hunter | 950/1900/3800 | 45-55/90-110/180-220 | 2 | 1 | 5 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T4 | Scaled, Warrior | 1100/2700/5500 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T4 | Scaled, Warrior | 2200/5400/11000 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T4 | Scaled, Warrior | 2200/5400/11000 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T4 | Scaled, Warrior | 1650/4050/8250 | 60-70/120-150/250-300 | 2 | 1 | 10 | 0 | 100 | 300 | yes |
| Final 2020-11-19 | T4 | Scaled, Warrior | 1650/3300/6600 | 60-70/120-140/240-280 | 2 | 1 | 10 | 0 | 100 | 300 | yes |

Abilities (last wiki state):
- **Ravage** (cooldown 30.00, mana 100, magical): Tidehunter slams the ground, causing tentacles to erupt in all directions, dealing [100/150/250] damage and stunning all enemy units [1/2/3] cells away for [2/2.25/3] seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Ravage: Tidehunter slams the ground, causing tentacles to erupt in all directions, dealing [150/250/350] damage and stunning all enemy units [2/3/4] cells away for [2/2.5/3] seconds. (cd 30, mana 100)
- Pre-Big Update 2019-10-10: Ravage: Tidehunter slams the ground, causing tentacles to erupt in all directions, dealing [100/150/250] damage and stunning all enemy units [1/2/3] cells away for [2/2.25/3] seconds. (cd 30, mana 100)

<details><summary>Wiki change log (6 patches)</summary>

- August 27, 2020
  - Health changed from [2,200/5,400/11,000] to [1,650/3,300/6,600]
  - Attack Damage Minimum changed from [60, 120, 250] to [60, 120, 240]
  - Attack Damage Maximum changed from [70, 150, 300] to [70, 140, 280]
- February 25, 2020
  - Rejoined the hero roster.
- December 18, 2019
  - Removed from the game.
- September 05, 2019
  - Ravage:
  - Now applies its damage after the stun is applied, so the Shaman (old) Ace tier Effect interacts properly.
- August 15, 2019
  - Armor changed from 5 to 10.
  - Maximum Health changed from [850, 1700, 3400] to [1100, 2700, 5500].
  - Attack Damage Minimum changed from [45, 90, 180] to [60, 120, 250].
  - Attack Damage Maximum changed from [55, 110, 220] to [70, 150, 300].
- July 19, 2019
  - Alliance changed from Scaled Hunter to Scaled Warrior.
  - Maximum Health changed from [950, 1900, 3800] to [850, 1700, 3400].
  - Gold Cost changed from 5 to 4.
  - Draft Tier changed from 5 to 4.
  - Health Regeneration changed from 5 to 0.
  - Ravage:
  - Radius changed from [2, 3, 4] to [1, 2, 3].
  - Damage changed from [150, 250, 350] to [100, 150, 250].
  - Duration changed from [2, 2.5, 3] to [2, 2.25, 3].

</details>

### Timbersaw
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 2, Scrappy, Inventor.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Scrappy, Inventor (beta version) | 800/1600/3200 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Scrappy, Inventor | 800/1600/3200 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T2 | Scrappy, Inventor | 1600/3200/6400 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Scrappy, Inventor | 1600/3200/6400 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T2 | Scrappy, Inventor | 1200/2400/4800 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | no |
| Final 2020-11-19 | T2 | Scrappy, Inventor | 1200/2400/4800 | 55-60/110-120/220-240 | 1.4 | 1 | 6/8/10 | 0 | 100 | 290 | no |

Abilities (last wiki state):
- **Whirling Death** (cooldown [6.00/5.00/4.00], mana 100, pure): The sharp edges of Timbersaw's suit of armor whirl about, damaging enemies up to 2 cells away for [150/200/250] damage.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Whirling Death: The sharp edges of Timbersaw's suit of armor whirl about, damaging enemies up to 2 cells away for [150/200/250] damage. (cd [6/5/4], mana 100)

### Tinker
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in data, rotated out; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 3, Scrappy, Inventor.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Scrappy, Inventor (beta version) | 500/1000/2000 | 40-50/80-100/160-200 | 1.5 | 4 | 10 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T3 | Scrappy, Inventor | 750/1500/3500 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T3 | Scrappy, Inventor | 1500/3000/7000 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T3 | Scrappy, Inventor | 1500/3000/7000 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T3 | Scrappy, Inventor | 1125/2250/5250 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 | no |
| Final 2020-11-19 | T3 | Scrappy, Inventor | 1125/2250/5250 | 60-72/120-144/270-330 | 1.3 | 4 | 5 | 0 | 100 | 290 | no |

Abilities (last wiki state):
- **Heat-Seeking Missile** (cooldown [8.00/6.00/0.50], mana 100, magical): Tinker fires a salvo of rockets at [2/3/4] random enemies dealing [350/450/450] damage on impact.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Heat-Seeking Missile: Tinker fires a salvo of rockets at 3 random enemies dealing [250/350/450] damage on impact. (cd [8/6/4], mana 100)
- Pre-Big Update 2019-10-10: Heat-Seeking Missile: Tinker fires a salvo of rockets at [2/3/4] random enemies dealing [350/450/450] damage on impact. (cd [8/6/0.5], mana 100)

<details><summary>Wiki change log (2 patches)</summary>

- July 24, 2019
  - Attack Speed changed from 0.67 to 0.77
  - Armor changed from 10 to 5
  - Attack Damage Minimum changed from [50, 100, 225] to [60, 120, 270]
  - Attack Damage Maximum changed from [60, 120, 275] to [72, 144, 330]
- July 19, 2019
  - Maximum Health changed from [500, 1000, 2000] to [750, 1500, 3500].
  - Attack Damage Minimum changed from [40, 80, 160] to [50, 100, 225].
  - Gold Cost changed from 1 to 3.
  - Draft Tier changed from 1 to 3.
  - Attack Damage Maximum changed from [50, 100, 200] to [60, 120, 275].
  - Heat-Seeking Missile:
  - Cooldown changed from [8, 6, 4] to [8, 6, 0.5].
  - Damage Per Missile changed from [250, 350, 450] to [350, 450, 450].
  - Targets changed from 3 to [2, 3, 4].

</details>

### Tiny
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Primordial, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Primordial (beta version), Warrior | 650/1300/2600 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T1 | Primordial, Warrior | 650/1300/2600 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T1 | Primordial, Warrior | 1300/2600/5200 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T1 | Primordial, Warrior | 1300/2600/5200 | 60-100/120-200/240-400 | 1.9 | 1 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T1 | Primordial, Warrior | 1000/2000/4000 | 65-110/130-220/260-440 | 1.9 | 1 | 5 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T1 | Primordial, Warrior | 1000/2000/4000 | 65-110/130-220/260-440 | 1.9 | 1 | 5 | 0 | 100 | 290 | no |

Abilities (last wiki state):
- **Toss** (cooldown [10/9/8], mana 100, physical): Tiny picks up a random adjacent enemy, and launches it at the farthest enemy target. At impact, the target deals [100/200/300] damage to all enemies 1 cell away and stuns them for [1/1.5/2] seconds.
  - If there are no free cells available near enemies, Tiny will toss his enemy into the nearest free cell rather than not casting.
- **Tiny 3 Star Effect**: At three stars, each time Tiny tosses an enemy, he grows in size, gaining +75 Attack Damage and +5 Armor but losing 10 Attack Speed, up to a maximum of 5 times.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Toss: Tiny picks up a random adjacent enemy, and launches it at the farthest enemy target.  At impact, the target deals [100/200/300] damage to all enemies 1 cell away and stuns them for [1.5/2.0/2.5] seconds. (cd [10/8/6], mana 100)
- Big Update 2019-10-24: Toss: Tiny picks up a random adjacent enemy, and launches it at the farthest enemy target.  At impact, the target deals [100/200/300] damage to all enemies 1 cell away and stuns them for [1.0/1.5/2.0] seconds. (cd [10/9/8], mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- May 21, 2020
  - At three stars, each time Tiny Tosses an enemy, he grows in size, gaining +75 Attack Damage and +5 Armor but losing 10 Attack Speed, up to a maximum of 5 times.
- May 14, 2020
  - Toss
  - If there are no free cells available near enemies, Tiny will toss his enemy into the nearest free cell rather than not casting.
- January 16, 2020
  - Maximum Health changed from [975, 1950, 3900] to [1000, 2000, 4000]
  - Attack Damage Minimum changed from [60, 120, 240] to [65, 130, 260]
  - Attack Damage Maximum changed from [100, 200, 400] to [110, 220, 440]
- October 24, 2019
  - Toss:
  - Stun Duration changed from [1.5, 2.0, 2.5] to [1.0, 1.5, 2.0]
  - Cooldown changed from [10, 8, 6] to [10, 9, 8]
- September 05, 2019
  - Toss:
  - Tiny can now toss Target Buddy.

</details>

### Treant Protector
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 3, Shaman, Healer.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Elusive, Druid | 650/1300/2300 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 | yes |
| Pre-Big Update 2019-10-10 | T3 | Elusive, Druid | 750/1500/2500 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 | yes |
| Big Update 2019-10-24 | T3 | Druid, Brute | 1500/3000/5000 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 | yes |
| Outlanders 2019-12-06 | T3 | Druid, Brute | 1500/3000/5000 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 | yes |
| Season One 2020-02-25 | T3 | Druid, Brute | 1125/2250/3750 | 60-90/120-180/240-360 | 2 | 1 | 5 | 0 | 100 | 270 | yes |
| Final 2020-11-19 | T3 | Shaman, Healer | 1125/2250/5400 | 60-90/120-180/288-432 | 2 | 1 | 5 | 0 | 100 | 270 | yes |

Abilities (last wiki state):
- **Leech Seed** (cooldown 10.00, mana 100, magical): Treant Protector plants a life-sapping seed in an enemy unit, draining its health by [100/150/200] every 1 seconds, while simultaneously slowing its attack and movement speed by -50%. The seed heals friendly units up to 1 cell away from the target. Lasts 5 seconds. Jumps to a new target within 1 cell if the current target dies.
  - Leech Seed can trigger up to 4 times in total.
- **Shaman Alliance Bonus**: The Shaman Alliance creature gives allied units within 1 cell +[2/5/12] Armor and +[5/10/30] HP Regen.
- **Treant Protector 3 Star Effect**: Treant Protector gains Living Armor. Units healed by Leech Seed gain +5 Armor. Jumps to a new target within 1 cell if the current target dies.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Leech Seed: Treant Protector plants a life-sapping seed in an enemy unit, draining its health by [60/90/120] every 1.5 seconds, while simultaneously slowing its attack and movement speed by -50%.  The seed heals friendly units up to 3 cells away from the target. Lasts 5 seconds. (cd 7, mana 100)
- Pre-Big Update 2019-10-10: Leech Seed: Treant Protector plants a life-sapping seed in an enemy unit, draining its health by [100/150/200] every 1.5 seconds, while simultaneously slowing its attack and movement speed by -50%.  The seed heals friendly units up to 1 cells away from the target. Lasts 5 seconds. (cd 7, mana 100)
- Big Update 2019-10-24: Leech Seed: Treant Protector plants a life-sapping seed in an enemy unit, draining its health by [100/150/200] every 1.5 seconds, while simultaneously slowing its attack and movement speed by -50%.  The seed heals friendly units up to 1 cell away from the target. Lasts 5 seconds. Jumps to a new target within 1 cell if the current target dies. (cd 7, mana 100)

<details><summary>Wiki change log (7 patches)</summary>

- August 27, 2020
  - Alliances changed from Brute Druid to Shaman Healer.
  - Health changed from [1,125/2,250/3,750] to [1,125/2,250/5,400]
  - Attack Damage Minimum changed from [60, 120, 240] to [60, 120, 288]
  - Attack Damage Maximum changed from [90, 180, 360] to [90, 180, 432]
- May 28, 2020
  - 3 Star Effect: Treant Protector gains Living Armor. During Leech Seed, friendly units healed by the Seed gain +5 Armor.
- January 16, 2020
  - Leech Seed:
  - Cooldown changed from 7 to 10
- October 24, 2019
  - Alliances changed from Elusive Druid to Druid Brute
- September 19, 2019
  - Leech Seed:
  - If the target of Leech Seed dies before the ability duration expires, the seed jumps to another target for its remaining duration.
- July 24, 2019
  - Leech Seed:
  - Projectile Speed changed from 600 to 800
  - Leech Damage and Heal changed from [80, 110, 150] to [100, 150, 200]
- July 19, 2019
  - Maximum Health changed from [650, 1300, 2300] to [750, 1500, 2500].
  - Gold Cost changed from 2 to 3.
  - Draft Tier changed from 2 to 3.
  - Leech Seed:
  - Damage changed from [60, 90, 120] to [80, 110, 150].
  - Radius changed from 3 to 1.
  - Expires when the target dies.

</details>

### Troll Warlord
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Troll, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T4 | Troll, Warrior | 900/1800/3600 | 90-100/180-200/360-400 | 1.1 | 3 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T5 | Troll, Warrior | 1200/2000/4000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T5 | Troll, Warrior | 2400/4000/8000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T5 | Troll, Warrior | 2400/4000/8000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T5 | Troll, Warrior | 1800/3000/6000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T5 | Troll, Warrior | 1500/3000/6000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | yes |

Abilities (last wiki state):
- **Fervor**: Passive: Troll Warlord rains blow after blow gaining [12/16/20]% bonus increased attack speed per stack. If Troll changes targets, the stack drops to zero. This effect can only stack up to [8/12/16] times.
- **Ace effect (2019-08-30 to 2020-02-25, Troll Ace):** Grants your units Mini-Bash: While attacking, your team has a 5% chance per Troll Alliance level of stunning their target for 0.25 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Fervor: PASSIVE: Troll Warlord rains blow after blow gaining [15/20/25]% bonus increased attack speed per stack.  If Troll changes targets, the stacks drop to zero.  This effect can only stack up to [6/9/12] times. (cd -, mana -)
- Pre-Big Update 2019-10-10: Fervor: PASSIVE: Troll Warlord rains blow after blow gaining [15/20/25]% bonus increased attack speed per stack.  If Troll changes targets, the stacks drop to zero.  This effect can only stack up to [6/9/12] times. (cd -, mana -) / Ace/alliance bonus (internal id troll_warlord_troll_bonus): see the Ace effect entry (cd -, mana -)
- Outlanders 2019-12-06: Fervor: PASSIVE: Troll Warlord rains blow after blow gaining [8/10/13]% bonus increased attack speed per stack.  If Troll changes targets, the stacks drop to zero.  This effect can only stack up to [12/19/24] times. (cd -, mana -) / Ace/alliance bonus (internal id troll_warlord_troll_bonus): see the Ace effect entry (cd -, mana -)

<details><summary>Wiki change log (6 patches)</summary>

- September 04, 2020
  - Fervor:
  - Bonus Attack Speed per stack changed from [8, 10, 13] to [12, 16, 20]
  - Maximum stacks changed from [12, 19, 24] to [8, 12, 16]
- February 25, 2020
  - Ace effect removed.
  - Tier changed from Ace to 5.
- November 07, 2019
  - Fervor:
  - Bonus attack speed changed from [15, 20, 25] to [8, 10, 13].
  - Max Stacks changed from [6, 9, 12] to [12, 19, 24].
- August 31, 2019
  - The change in Attack Rate is actually 1.0 -> 0.77.
- August 30, 2019
  - Ace for the Troll Alliance.
  - Attack Rate changed from 1.0 to 1.3.
  - Ace Effect: Grants your units Mini-Bash: While attacking, your team has a 5% chance per Troll Alliance level of stunning their target for 0.25 seconds.
- July 19, 2019
  - Attack Speed changed from 0.91 to 1.0.
  - Maximum Health changed from [900, 1800, 3600] to [1200, 2000, 4000].
  - Attack Damage Minimum changed from [90, 180, 360] to [180, 360, 520].
  - Gold Cost changed from 4 to 5.
  - Draft Tier changed from 4 to 5.
  - Attack Damage Maximum changed from [100, 200, 400] to [200, 400, 600].

</details>

### Tusk
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Savage, Warrior.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Savage (beta version), Warrior | 650/1300/2600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T1 | Savage, Warrior | 700/1400/2800 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | yes |
| Big Update 2019-10-24 | T1 | Savage, Warrior | 1400/2800/5600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | yes |
| Outlanders 2019-12-06 | T1 | Savage, Warrior | 1400/2800/5600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | yes |
| Season One 2020-02-25 | T1 | Savage, Warrior | 1050/2100/4200 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | yes |
| Final 2020-11-19 | T1 | Savage, Warrior | 1150/2300/4600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | yes |

Abilities (last wiki state):
- **Walrus Punch** (cooldown 4.00, mana 60, physical): Tusk connects with his mighty Walrus Punch, a critical strike that deals [300/350/400]% damage and launches its victim into the air for [1/1/1.5] second(s). Upon landing, the victim is slowed by 40% for [2/2/3] seconds and loses [0/0/40] Attack Speed. Tusk cannot be killed while punching.
- **Tusk 3 Star Effect**: Walrus Punch slows enemies for a longer duration and causes them to lose Attack Speed as well as Movement Speed.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Walrus PUNCH!: Tusk connects with his mighty Walrus Punch, a critical strike so powerful it launches its victim into the air. Upon landing, the victim is slowed by 40%. (cd 7, mana 100)
- Pre-Big Update 2019-10-10: Walrus PUNCH!: Tusk connects with his mighty Walrus Punch, a critical strike so powerful it launches its victim into the air. Upon landing, the victim is slowed by 40%. (cd 4, mana 60)
- Big Update 2019-10-24: Walrus PUNCH!: Tusk connects with his mighty Walrus Punch, a critical strike that deals [300/350/400]% damage and launches its victim into the air for 1 second. Upon landing, the victim is slowed by 40% for 2 seconds. Tusk cannot be killed while punching. (cd 4, mana 60)

<details><summary>Wiki change log (6 patches)</summary>

- November 19, 2020
  - Maximum Health changed from [1050, 2100, 4200] to [1150, 2300, 4600]
- May 08, 2020
  - Walrus PUNCH!:
  - At 3 stars, Walrus Punch also slows the enemy's attack speed by 40.
  - Air Time changed from 1.0 to [1.0, 1.0, 1.5].
  - Slow duration changed from 2.0 to [2.0, 2.0, 3.0].
- March 19, 2020
  - Walrus Punch:
  - Can still be cast while Tusk is broken.
- September 26, 2019
  - Maximum Mana changed from 100 to 60.
  - Tusk:
  - Mana Cost changed from 100 to 60.
  - Cooldown changed from 7 to 4.
- September 19, 2019
  - Walrus Punch:
  - Tusk is now invulnerable while mid-Punch.
- July 19, 2019
  - Maximum Health changed from [650, 1300, 2600] to [700, 1400, 2800].

</details>

### Vengeful Spirit
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Fallen, Heartless.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T1 | Fallen, Heartless | 850/1700/3400 | 45-55/90-110/180-220 | 1.3 | 2 | 5 | 0 | 60 | 300 | yes |

Abilities (last wiki state):
- **Wave of Terror** (cooldown [14.00/12.00/10.00], mana 60, magical): Vengeful Spirit lets loose a wicked cry in a straight path, damaging enemy units along the way for [100/175/200] damage, and reducing their armor by [3/5/7] for 6 seconds.
- **Vengeful Spirit 3 Star Effect** (magical): When Vengeful Spirit dies, she deals 300 damage to all enemies within 2 cells.

### Venomancer
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 1, Scaled, Summoner, Poisoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Savage (beta version), Warlock | 500/1000/2000 | 50-60/100-120/200-240 | 1.2 | 3 | 0 | 20 | 100 | 275 | yes |
| Pre-Big Update 2019-10-10 | T1 | Savage, Warlock | 500/1000/2000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 | yes |
| Big Update 2019-10-24 | T1 | Savage, Warlock | 1000/2000/4000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 | yes |
| Outlanders 2019-12-06 | T1 | Savage, Warlock, Scaled | 1000/2000/4000 | 50-55/100-110/200-220 | 1.2 | 3 | 0 | 20 | 100 | 275 | yes |
| Season One 2020-02-25 | T1 | Scaled, Summoner | 750/1500/3000 | 35-45/70-90/140-180 | 1.2 | 3 | 0 | 20 | 80 | 275 | yes |
| Final 2020-11-19 | T1 | Scaled, Summoner, Poisoner | 750/1500/3000 | 35-45/70-90/140-180 | 1.2 | 3 | 0 | 20 | 80 | 275 | yes |

Abilities (last wiki state):
- **Plague Ward** (cooldown [5/5/3], mana 80): Venomancer summons 1 Plague Ward to fight alongside him. Plague Wards apply Poison stacks to their target and have [50/50/99]% Magic Resistance.
- **Venomancer 3 Star Effect**: Plague Wards are immune to Magical damage.

Older abilities (wiki):
- **Venomous Gale** (cooldown [10/8/6], mana 80): Launches a ball of venom in a line; Enemies hit take [30/60/120] MAGIC damage and are Poisoned. Affected units lose 50% movement speed and 25 attack speed for 15 seconds.
- **Poison Sting**: Passive: Each attack applies a stack of Poison to the target.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Plague Ward: Venomancer summons a Plague Ward to fight alongside him. (cd 5, mana 100)
- Pre-Big Update 2019-10-10: venomancer_venomous_gale: description unknown (cd [10/8/6], mana 100)
- Big Update 2019-10-24: Venomous Gale: Launches a ball of venom in a line, poisoning enemy units so that they take [30/60/120] damage as well as [10/20/40] damage every 3s for a duration of 15s. Affected units lose 50% movement speed and 25 attack speed. (cd [10/8/6], mana 100)

<details><summary>Wiki change log (12 patches)</summary>

- August 27, 2020
  - Now also a Poisoner. Venomancer's attacks now no longer poison if the Poisoner alliance is not active. Plague Ward attacks do still poison however.
- May 08, 2020
  - Plague Ward:
  - At 3 stars, Venomancer's Plague Wards have 99% Magic Resistance rather than 50%.
  - Cooldown changed from 5 to [5, 5, 3].
- April 03, 2020
  - Maximum Mana changed from 100 to 80
  - Plague Ward:
  - Mana Cost changed from 100 to 80
- February 25, 2020
  - Attack Damage Minimum changed from [50, 100, 200] to [35, 70, 140]
  - Attack Damage Maximum changed from [55, 110, 220] to [45, 90, 180]
  - Removed ability Venomous Gale
  - Added ability Poison Sting:
  - PASSIVE: Each attack applies a stack of Poison to the target.
  - Readded ability Plague Ward:
  - Plague Ward:
  - Attack Damage Minimum changed from [50, 100, 150] to [10, 20, 40]
  - Attack Damage Maximum changed from [55, 110, 170] to [15, 30, 60]
  - Added ability Poison Sting:
  - PASSIVE: Each attack applies a stack of Poison to the target.
- January 30, 2020
  - Venomancer:
  - Now poisons enemies.
- November 27, 2019
  - Alliances changed from Savage Warlocks to Savage Warlocks Scaled.
- October 31, 2019
  - Venomancer:
  - Venomancer only gains 1 bonus stack on cast of Venomous Gale and does not gain per tick of that ability.
- October 02, 2019
  - Venomancer:
  - While Venomancer still only targets units within his attack range (3 cells), the actual Gale projectile travels up to 5 cells.
  - Projectile damage increased to [30, 60, 120] from [25, 50, 100].
- September 26, 2019
  - Removed Venomancer ability.
  - Added Venomancer:
  - Launches a ball of venom in a line, poisoning enemy units so that they take [25, 50, 100] damage as well as [10, 20, 40] damage every 3s for a duration of 15s. Affected units lose 50% movement speed and 25 Attack Rate.
  - Mana Cost: 100.
  - Cooldown: [10, 8, 6].
- August 01, 2019
  - Venomancer:
  - Number of Wards spawned changed from (1, 1, 3) to (1, 1, 1)
  - Maximum Health changed from (200, 200, 240) to (200, 220, 240)
  - Attack Damage Minimum changed from (50, 80, 120) to (50, 100, 150)
  - Attack Range changed from 6 to 8
  - Attack Damage Maximum changed from (55, 100, 140) to (55, 110, 170)
  - Cooldown changed from 5/6/7 to 5
- July 19, 2019
  - Maximum Health changed from [550, 1100, 2200] to [500, 1000, 2000].
  - Gold Cost changed from 3 to 1.
  - Draft Tier changed from 3 to 1.
  - Attack Damage Maximum changed from [60, 120, 240] to [55, 110, 220].
  - Venomancer:
  - Number of wards changed from [1, 2, 4] to [1, 1, 3].
  - Armor changed from 5 to 0.
  - Maximum Health changed from 200 to [200, 200, 240].
  - Attack Damage Minimum changed from 50 to [50, 80, 120].
  - Attack Damage Maximum changed from 100 to [55, 100, 140].
- July 04, 2019
  - Health increased from [500, 1000, 2000] to [550, 1100, 2200]
  - Changes to Venomancer
  - Summon Count increased from 1 Ward to [1, 2, 4] Wards.
  - Ward is always Rank 1.
  - Cast Point reduced from 0.5s to 0.13s.
  - Cooldown increased from 5 to [5, 6, 7].
  - Wards have 50% magic resistance.

</details>

### Viper
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Dragon, Poisoner.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Dragon, Assassin | 550/1100/2200 | 50-60/100-120/150-240 | 1 | 3 | 5 | 20 | 100 | 275 | yes |
| Pre-Big Update 2019-10-10 | T3 | Scaled, Dragon | 550/1100/3000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 | yes |
| Big Update 2019-10-24 | T3 | Scaled, Dragon | 1100/2200/6000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 | yes |
| Outlanders 2019-12-06 | T3 | Scaled, Dragon | 1100/2200/6000 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 50 | 275 | yes |
| Season One 2020-02-25 | T3 | Dragon, Assassin | 825/1650/4500 | 50-60/100-120/240-360 | 1 | 3 | 5 | 20 | 70 | 275 | yes |
| Final 2020-11-19 | T4 | Dragon, Poisoner | 1100/2200/4400 | 60-80/120-160/240-320 | 1 | 3 | 5 | 20 | 70 | 275 | yes |

Abilities (last wiki state):
- **Nethertoxin** (cooldown 6.0, mana 70, pure): Viper poisons a cell, dealing [100/150/225] damage per second to any enemy unit in it, rooting them and applying break.
  - Lasts 5 seconds.
- **Corrosive Skin** (magical): Unlocked by the Dragon alliance bonus Passive: Viper exudes an infectious toxin that applies a stack of Poison and slows the attack speed by [20/30/40] of any enemy that damages it for 2 seconds. The acid exudate also increases Viper's magic resistance by [30/50/70].

Older abilities (wiki):
- **Viper 3 Star Effect**: At three stars, Viper performs a Viper Strike every 5 attacks. The enemy unit takes 250 damage per second and loses 80% move speed and 80 Attack Speed, with the slowing tapering off over the 3 second duration.
- **Viper Strike** (cooldown 3.00, mana 100): Viper's acidic venom slows an enemy unit's movement and attack speed by [-40/-60/-80]% while also dealing [150/250/350] poison damage over time. The slowing effect fades over the duration of the poison.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Viper Strike: Viper's acidic venom slows an enemy unit's movement and attack speed by [-40/-60/-80]% while also dealing [150/250/350] poison damage over time.  The slowing effect fades over the duration of the poison. (cd 3, mana 100)
- Pre-Big Update 2019-10-10: viper_nethertoxin: description unknown (cd 5, mana 50)
- Big Update 2019-10-24: Nethertoxin: Viper poisons a cell, dealing [50/100/150] damage per second to any enemy unit in it, rooting them and applying Break. (cd 5, mana 50)
- Outlanders 2019-12-06: Nethertoxin: Viper poisons a cell, dealing [100/200/300] damage per second to any enemy unit in it, rooting them and applying Break. (cd 5, mana 50)

<details><summary>Wiki change log (11 patches)</summary>

- November 19, 2020
  - Maximum Health changed from [900, 1800, 3600] to [1100, 2200, 4400]
- August 27, 2020
  - Tier changed from 3 to 4. Alliances changed from Dragon Assassin to Dragon Poisoner.
  - Health changed from [825/1,650/4,500] to [900/1,800/3,600]
  - Attack Damage Minimum changed from [50, 100, 240] to [60, 120, 240]
  - Attack Damage Maximum changed from [60, 120, 360] to [80, 160, 320]
- June 12, 2020
  - At three stars, Viper performs a Viper Strike every 5 attacks. The enemy unit takes 250 damage per second and loses 80% move speed and 80 Attack Speed, with the slowing tapering off over the 3 second duration.
- February 25, 2020
  - Corrosive Skin:
  - Instead of dealing damage over time, Corrosive Skin now applies a stack of Poison to Viper's attacker.
- January 16, 2020
  - Nethertoxin:
  - Damage changed from [100, 200, 300] to [100, 150, 225]
- December 18, 2019
  - Alliances changed from Scaled Dragon to Dragon Assassin
- November 07, 2019
  - Nethertoxin:
  - Damage changed from [50, 100, 150] to [100, 200, 300].
- October 10, 2019
  - Max Mana is now 50.
- October 02, 2019
  - Viper is now a Scaled Dragon (was Dragon Assassin).
  - Removed Viper ability.
  - Added Viper.
- July 12, 2019
  - Maximum Health changed from [550, 1100, 2200] to [550, 1100, 3000].
  - Attack Damage Minimum changed from [50, 100, 150] to [50, 100, 240].
  - Attack Damage Maximum changed from [60, 120, 240] to [60, 120, 360].
- June 25, 2019
  - Dragon Alliance now unlocks Corrosive Skin.

</details>

### Void Spirit
- Status: In the final pool. First seen in data: Outlanders 2019-12-06.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 4, Void, Spirit.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Outlanders 2019-12-06 | T5 | Spirit, Warlock | 1700/3400/6800 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Season One 2020-02-25 | T4 | Void, Spirit | 1275/2550/5100 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 | yes |
| Final 2020-11-19 | T4 | Void, Spirit | 1275/2550/5100 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 | yes |

Abilities (last wiki state):
- **Dissimilate** (cooldown [15.00/12.00/9.00], mana 100, magical): Void Spirit enters the aether, disappearing and creating 3 portals. After 1.3 seconds, he exits one of the portals and deals [200/300/400] damage to enemies within 1 cell, reducing their armor by [10/15/20] for 5 seconds.
- **Void Elemental**: Passive: When Void Spirit executes a Delta Slam, he breaks affected enemy units for 3 seconds.
- **Ace effect (2019-08-30 to 2020-02-25, Spirit Ace):** Charged Spirit units deal 100% more attack damage.

Ability text rendered from beta game data (shown when it changed):
- Outlanders 2019-12-06: Dissimilate: Void Spirit enters the aether, disappearing and creating 3 portals. After 1.3 seconds he exits one of the portals and deals [200/300/400] damage to enemies within 1 cell, reducing their armor by [10/15/20] for 5 seconds. (cd [15/12/9], mana 100) / Ace/alliance bonus (internal id void_spirit_spirit_bonus): see the Ace effect entry (cd -, mana -) / Void Elemental: PASSIVE: When Void Spirit executes a Delta Slam, he breaks affected enemy units for 3 seconds. (cd -, mana -)

<details><summary>Wiki change log (1 patches)</summary>

- February 25, 2020
  - Ace effect removed.
  - Tier changed from Ace to 4
  - Alliances changed from Spirit Warlock to Void Spirit

</details>

### Warlock
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Blood-Bound, Warlocks, Healer.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T1 | Blood-Bound, Warlock | 650/1450/2850 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T1 | Blood-Bound, Warlock | 650/1450/2850 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T1 | Blood-Bound, Warlock, Healer | 1300/2900/5700 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T1 | Blood-Bound, Warlock, Healer | 1300/2900/5700 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T1 | Blood-Bound, Warlock, Healer | 975/2175/4275 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T1 | Blood-Bound, Warlock, Healer | 975/2175/4275 | 40-60/80-120/160-220 | 1.7 | 4 | 0 | 0 | 100 | 290 | no |

Abilities (last wiki state):
- **Shadow Word** (cooldown 6.00, mana 100, magical): Warlock whispers an incantation, immediately restoring [168/337/675] health to a friendly unit and [56/112/225] a second over 3 seconds, or damaging an enemy unit for [75/150/300], every second for 3 seconds.
- **Warlock 3 star effect**: At three stars, while alive Warlock prevents friendly Demons from silencing each other when casting abilities.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Shadow Word: Warlock whispers an incantation, restoring [75/150/200] health to a friendly unit, or damaging an enemy unit for [100/150/200], every second for 6 seconds. (cd 20, mana 100)
- Pre-Big Update 2019-10-10: Shadow Word: Warlock whispers an incantation, restoring [75/150/300] health to a friendly unit, or damaging an enemy unit for [75/150/300], every second for 3 seconds. (cd 6, mana 100)
- Big Update 2019-10-24: Shadow Word: Warlock whispers an incantation, immediately restoring [168/337/675] health to a friendly unit and [56/112/225] a second over 3 seconds, or damaging an enemy unit for [75/150/300], every second for 3 seconds. (cd 6, mana 100)

<details><summary>Wiki change log (4 patches)</summary>

- May 14, 2020
  - At three stars, Warlock prevents friendly Demons from silencing each other when casting abilities.
- October 24, 2019
  - Alliances changed from Blood-Bound Warlock to Blood-Bound Warlock Healer
- September 19, 2019
  - Shadow Word
  - Now heals 75% of the total heal instantly, and the remaining 25% over the duration of the buff. This results in [168, 337, 675] instant heal and [56, 112, 225] over the remaining duration.
  - Damage per second changed from [50, 100, 150] to [75, 150, 300].
- July 04, 2019
  - Shadow Word
  - Cast Point reduced from 0.5s to 0.4s.
  - Cooldown reduced from 20s to 6s.
  - Damage from [ 100, 150, 200 ] to [ 50, 100, 150 ].
  - Heal from [ 75, 150, 200 ] to [ 50, 125, 175 ].
  - Duration from 6 seconds to 3 seconds.

</details>

### Weaver
- Status: Rotated out at the end (still in final data). First seen in data: Big Update 2019-10-24.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 1, Insect, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Big Update 2019-10-24 | T2 | Insect, Hunter | 900/1800/3600 | 45-55/90-110/207-253 | 0.9 | 2 | 5 | 0 | 20 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Insect, Elusive, Scrappy | 900/1800/3600 | 60-70/120-140/260-300 | 0.9 | 2 | 5 | 0 | 20 | 290 | yes |
| Season One 2020-02-25 | T1 | Insect, Hunter | 675/1350/2700 | 45-55/90-110/180-220 | 0.9 | 2 | 5 | 0 | 20 | 290 | yes |
| Final 2020-11-19 | T1 | Insect, Hunter | 675/1350/2700 | 45-55/90-110/180-220 | 0.9 | 2 | 5 | 0 | 20 | 290 | no |

Abilities (last wiki state):
- **Shukuchi** (cooldown [6/5/4], mana 20, magical): Weaver shifts out of visibility, becoming untargetable and quickly moving through a target up to 3 cells away. Weaver deals [60/120/200] damage to each enemy it passes through.
  - Weaver moves at a speed of 700 during Shukuchi.
  - Weaver will not Shukuchi while evading, unless about to receive magic damage.
- **Weaver 3 Star Effect**: For 4 seconds after a Shukuchi, Weaver's attacks burn 20 Mana, doing 2 damage per Mana burnt.

Ability text rendered from beta game data (shown when it changed):
- Big Update 2019-10-24: Shukuchi: Weaver shifts out of visibility, becoming untargetable and quickly moving through a target up to 3 cells away. Weaver deals [60/120/200] damage to each enemy it passes through. (cd [6/5/4], mana 20)

<details><summary>Wiki change log (7 patches)</summary>

- May 08, 2020
  - Shukuchi:
  - At 3 stars, Weaver gains 4 seconds of Mana Break after each Shukuchi, burning up to 20 Mana per hit and doing 2 damage per Mana burnt.
- February 25, 2020
  - Tier changed from 2 to 1
  - Alliances changed from Scrappy Insect Hunter to Insect Hunter
  - Attack Damage Minimum changed from [60, 120, 260] to [45, 90, 180]
  - Attack Damage Maximum changed from [70, 140, 300] to [55, 110, 220]
- December 18, 2019
  - Alliances changed from Scrappy Elusive Insect to Scrappy Insect Hunter
- December 05, 2019
  - Can no longer Shukuchi while rooted.
- November 26, 2019
  - Weaver is now a Scrappy Elusive Insect.
- November 21, 2019
  - Can no longer Shukuchi while silenced.
- November 14, 2019
  - Changed from Hunter to Elusive
  - Attack Damage Minimum changed from [45, 90, 207] to [60, 120, 260]
  - Attack Damage Maximum changed from [55, 110, 253] to [70, 140, 300]
  - Weaver will not Shukuchi while evading, unless about to receive magic damage.

</details>

### Windranger
- Status: In the final pool. First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 2, Vigilant, Hunter.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T3 | Elusive, Hunter | 550/1100/2200 | 60-65/120-130/240-260 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |
| Pre-Big Update 2019-10-10 | T2 | Elusive, Hunter | 550/1100/2300 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |
| Big Update 2019-10-24 | T2 | Elusive, Hunter | 1100/2200/4600 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |
| Outlanders 2019-12-06 | T2 | Elusive, Hunter | 1100/2200/4600 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |
| Season One 2020-02-25 | T2 | Vigilant, Hunter | 825/1650/3450 | 50-60/100-120/230-260 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |
| Final 2020-11-19 | T2 | Vigilant, Hunter | 825/1650/3300 | 50-60/100-120/200-240 | 0.9 | 4 | 5 | 0 | 100 | 290 | yes |

Abilities (last wiki state):
- **Powershot** (cooldown 10.00, mana 100, magical): Windranger charges her bow for up to 1.5 seconds for a single powerful shot, dealing up to [200/400/600] damage. For each enemy that Powershot hits, its damage is reduced by 10%.
- **Windranger 3 Star Effect**: Windranger gains 100% evasion and Magic Immunity while casting Powershot. This lasts for 3 seconds.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Powershot: Windranger charges her bow for up to 1.5 seconds for a single powerful shot, dealing up to [400/600/800] damage. For each enemy that Powershot hits, its damage is reduced by 10%. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Powershot: Windranger charges her bow for up to 1.5 seconds for a single powerful shot, dealing up to [200/400/600] damage. For each enemy that Powershot hits, its damage is reduced by 10%. (cd 10, mana 100)

<details><summary>Wiki change log (5 patches)</summary>

- August 27, 2020
  - Health changed from [1,100/2,200/4,600] to [825/1,650/3,300]
  - Attack Damage Minimum changed from [50, 100, 230] to [50, 100, 200]
  - Attack Damage Maximum changed from [60, 120, 260] to [60, 120, 240]
- July 23, 2020
  - 3 Star Effect: At the start of charging her bow for her Powershot, Windranger gains 100% evasion and becomes magic immune. This lasts for 3 seconds.
- February 25, 2020
  - Alliances changed from Elusive Hunter to Vigilant Hunter
- December 18, 2019
  - Removed from the game.
- July 19, 2019
  - Maximum Health changed from [550, 1100, 2200] to [550, 1100, 2300].
  - Attack Damage Minimum changed from [60, 120, 240] to [50, 100, 230].
  - Gold Cost changed from 3 to 2.
  - Draft Tier changed from 3 to 2.
  - Attack Damage Maximum changed from [65, 130, 260] to [60, 120, 260].
  - Powershot:
  - Damage changed from [400, 600, 800] to [200, 400, 600].

</details>

### Witch Doctor
- Status: Rotated out at the end (still in final data). First seen in data: Beta launch 2019-06-20.
- Presence by snapshot: Beta launch 2019-06-20: in pool; Pre-Big Update 2019-10-10: in pool; Big Update 2019-10-24: in pool; Outlanders 2019-12-06: in pool; Season One 2020-02-25: in pool; Final 2020-11-19: in data, rotated out
- Wiki infobox (last state the wiki recorded): tier 2, Troll, Warlocks.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Beta launch 2019-06-20 | T2 | Troll, Warlock | 550/1100/2200 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | yes |
| Pre-Big Update 2019-10-10 | T2 | Troll, Warlock | 550/1100/2200 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | yes |
| Big Update 2019-10-24 | T2 | Troll, Warlock | 1100/2200/4400 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | yes |
| Outlanders 2019-12-06 | T2 | Troll, Warlock | 1100/2200/4400 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | yes |
| Season One 2020-02-25 | T2 | Troll, Warlock | 825/1650/3300 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | yes |
| Final 2020-11-19 | T2 | Troll, Warlock | 825/1650/3300 | 40-50/80-100/160-200 | 1.4 | 3 | 5 | 0 | 100 | 300 | no |

Abilities (last wiki state):
- **Paralyzing Cask** (cooldown 10.00, mana 100, magical): Witch Doctor throws a cask of paralyzing powder that bounces [6/7/8] times between enemy units, stunning and dealing [50/75/100] damage to those it hits. At 3 stars, Witch Doctor also curses units hit by the cask for 4 seconds, doing 20 damage per second. At the end, the unit takes an extra 40% of all damage taken during the curse.
  - Stun duration is 1 second.
  - Bounce range is 4.
- **Witch Doctor 3 Star Effect** (magical): Witch Doctor curses units hit by Paralyzing Cask with Maledict for 4 seconds, doing 20 damage per second. At the end, the unit takes an extra 40% of all damage taken during the curse.

Ability text rendered from beta game data (shown when it changed):
- Beta launch 2019-06-20: Paralyzing Cask: Witch Doctor throws a cask of paralyzing powder that bounces [5/7/9] times between enemy units, stunning and dealing [50/75/100] damage to those it hits. (cd 10, mana 100)
- Pre-Big Update 2019-10-10: Paralyzing Cask: Witch Doctor throws a cask of paralyzing powder that bounces [5/7/9] times between enemy units, stunning and dealing [50/75/100] damage to those it hits. (cd 15, mana 100)
- Outlanders 2019-12-06: Paralyzing Cask: Witch Doctor throws a cask of paralyzing powder that bounces [6/7/8] times between enemy units, stunning and dealing [50/75/100] damage to those it hits. (cd 10, mana 100)

<details><summary>Wiki change log (3 patches)</summary>

- May 21, 2020
  - Paralyzing Cask:
  - At three stars, Witch Doctor also curses units hit by the cask for 4 seconds, doing 20 damage per second. At the end, the unit takes an extra 40% of all damage taken during the curse.
- November 27, 2019
  - Paralyzing Cask:
  - Bounces changed from [5, 7, 9] to [6, 7, 8]
  - Cooldown changed from 15 to 10
- August 01, 2019
  - Paralyzing Cask:
  - Cooldown changed from 10 to 15

</details>

### Wraith King
- Status: In the final pool. First seen in data: Final 2020-11-19.
- Presence by snapshot: Beta launch 2019-06-20: absent; Pre-Big Update 2019-10-10: absent; Big Update 2019-10-24: absent; Outlanders 2019-12-06: absent; Season One 2020-02-25: absent; Final 2020-11-19: in pool
- Wiki infobox (last state the wiki recorded): tier 5, Fallen, Swordsman.

| Snapshot | Tier | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | In pool |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Final 2020-11-19 | T5 | Fallen, Swordsman | 2000/4000/8000 | 90-110/180-220/360-440 | 1.3 | 1 | 10 | 10 | 75 | 310 | yes |

Abilities (last wiki state):
- **Wraithfire Blast** (cooldown [6.00/4.00/2.00], mana 75, magical): Wraith King sears an enemy within 3 cells with spectral fire, stunning them for 2 seconds and dealing [100/200/300] damage per second for 5 seconds.
- **Raise Dead**: Passive: Heroes spawn zombies when they die within [1/2/3] cells of Wraith King
  - Does not raise a zombie from a Fallen hero that is in the process of reviving.
  - When a dying hero is in range of both friendly and enemy Wraith Kings, the zombie created will be of the same team as the dying hero.

<details><summary>Wiki change log (2 patches)</summary>

- September 24, 2020
  - Raise Dead:
  - When a dying hero is in range of both friendly and enemy Wraith Kings, the zombie created will be of the same team as the dying hero.
- September 04, 2020
  - Raise Dead:
  - Wraith King will no longer raise a zombie from a Fallen hero that is in the process of reviving.
  - Zombie:
  - Maximum Health changed from [450, 750, 1050] to [400, 700, 950]

</details>
