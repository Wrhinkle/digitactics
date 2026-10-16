# TFT Set 13: Into the Arcane

Source: CommunityDragon `https://raw.communitydragon.org/15.6/cdragon/tft/en_us.json` (set data `TFTSet13`) for champions, stats, abilities, traits, items and augments. MetaTFT lookup `https://data.metatft.com/lookups/TFTSet13_latest_en_us.json` (retrieved 2026-10-15) for spell calculation formulas used to fill computed tooltip values, augment rarity where CDragon has none, and set-mechanic lists. Anomaly rules: mobalytics.gg "Set 13 guide: Anomalies" and esports.gg patch 14.23 coverage.

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 13 |
| Name | Into the Arcane |
| Riot launch patch | 14.23 (Riot renamed 2025 patches 13.3 to 13.6) |
| League client version at launch (CDragon path) | 14.23 |
| Live dates | November 20, 2024 to April 1, 2025 |
| Theme | The Arcane animated series: Piltover and Zaun factions (Rebel, Enforcer, Chem-Baron, Black Rose, Academy, Family, Experiment). |
| Data snapshot used | Client 15.6 (last patch of the set) |
| Champion count in this file | 60 |

## Set mechanics and system changes

- Anomalies: at round 4-6 a champion is placed in the middle of the board and gains one Anomaly effect for the rest of the game. The Anomaly can be rerolled for 1 gold as often as wanted; after the whole pool is seen it starts over. Once per game, one champion. 61 Anomalies below.
- Chem-Baron: gain Shimmer after each player combat (more on a loss streak of 3+). Every 100 Shimmer the Black Market offers contraband only Chem-Barons can use; skipping a market gives Chem-Barons max Health. See trait text.
- Riot changed patch naming in 2025: patches during Set 13 in 2025 were labeled 13.3 to 13.6, and from Set 14 onward the TFT patch number matches the set number (14.1, 15.1, ...). The League client version (CDragon path) still uses 15.x and 16.x.
- CDragon also contains variants TFTSet13_Evolved and TFTSet13_Evolved_MacaoMode. Their purpose is not documented in the data; they are not covered here.

## Champions (60)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Amumu | 1 | Automata, Watcher | 600 | 35 | 35 | 45 | 0.6 | 1 | 0/0 |
| Darius | 1 | Conqueror, Watcher | 600 | 40 | 40 | 55 | 0.7 | 1 | 30/70 |
| Draven | 1 | Conqueror, Pit Fighter | 500 | 15 | 15 | 55 | 0.7 | 4 | 30/60 |
| Irelia | 1 | Rebel, Sentinel | 700 | 40 | 40 | 45 | 0.6 | 1 | 30/70 |
| Lux | 1 | Academy, Sorcerer | 500 | 20 | 20 | 30 | 0.7 | 4 | 0/50 |
| Maddie | 1 | Enforcer, Sniper | 500 | 15 | 15 | 50 | 0.7 | 6 | 20/120 |
| Morgana | 1 | Black Rose, Visionary | 500 | 20 | 20 | 30 | 0.7 | 4 | 0/40 |
| Powder | 1 | Family, Scrap, Ambusher | 500 | 15 | 15 | 30 | 0.75 | 4 | 0/90 |
| Singed | 1 | Chem-Baron, Sentinel | 650 | 40 | 40 | 55 | 0.6 | 1 | 0/50 |
| Steb | 1 | Enforcer, Bruiser | 650 | 45 | 45 | 55 | 0.55 | 1 | 25/75 |
| Trundle | 1 | Scrap, Bruiser | 650 | 40 | 40 | 50 | 0.65 | 1 | 30/80 |
| Vex | 1 | Rebel, Visionary | 450 | 15 | 15 | 30 | 0.7 | 4 | 0/60 |
| Violet | 1 | Family, Pit Fighter | 650 | 40 | 40 | 46 | 0.8 | 1 | 20/65 |
| Zyra | 1 | Experiment, Sorcerer | 500 | 20 | 20 | 30 | 0.7 | 4 | 10/60 |
| Akali | 2 | Rebel, Quickstriker | 700 | 45 | 45 | 45 | 0.75 | 1 | 0/55 |
| Camille | 2 | Enforcer, Ambusher | 700 | 45 | 45 | 55 | 0.75 | 1 | 0/25 |
| Leona | 2 | Academy, Sentinel | 800 | 50 | 50 | 55 | 0.6 | 1 | 20/65 |
| Nocturne | 2 | Automata, Quickstriker | 700 | 45 | 45 | 63 | 0.8 | 1 | 0/40 |
| Rell | 2 | Conqueror, Sentinel, Visionary | 800 | 45 | 45 | 60 | 0.6 | 1 | 40/90 |
| Renata Glasc | 2 | Chem-Baron, Visionary | 600 | 20 | 20 | 35 | 0.7 | 4 | 20/85 |
| Sett | 2 | Rebel, Bruiser, Pit Fighter | 850 | 50 | 50 | 60 | 0.6 | 1 | 50/100 |
| Tristana | 2 | Emissary, Artillerist | 550 | 20 | 20 | 42 | 0.75 | 4 | 20/60 |
| Urgot | 2 | Experiment, Pit Fighter, Artillerist | 650 | 40 | 40 | 55 | 0.75 | 2 | 45/90 |
| Vander | 2 | Family, Watcher | 800 | 45 | 45 | 55 | 0.7 | 1 | 0/50 |
| Vladimir | 2 | Black Rose, Watcher, Sorcerer | 800 | 45 | 45 | 45 | 0.65 | 1 | 0/65 |
| Zeri | 2 | Firelight, Sniper | 600 | 20 | 20 | 51 | 0.75 | 6 | 0/3 |
| Ziggs | 2 | Scrap, Dominator | 600 | 20 | 20 | 35 | 0.7 | 4 | 15/60 |
| Blitzcrank | 3 | Automata, Dominator | 850 | 50 | 50 | 60 | 0.6 | 1 | 20/70 |
| Cassiopeia | 3 | Black Rose, Dominator | 700 | 25 | 25 | 40 | 0.75 | 4 | 0/40 |
| Ezreal | 3 | Academy, Rebel, Artillerist | 700 | 25 | 25 | 60 | 0.75 | 4 | 0/60 |
| Gangplank | 3 | Scrap, Form Swapper, Pit Fighter | 700 | 0 | unknown | 50 | 0.5 | 1 | 0/10 |
| Kog'Maw | 3 | Automata, Sniper | 650 | 25 | 25 | 15 | 0.75 | 4 | 0/40 |
| Loris | 3 | Enforcer, Sentinel | 850 | 50 | 50 | 50 | 0.65 | 1 | 40/80 |
| Nami | 3 | Emissary, Sorcerer | 700 | 25 | 25 | 40 | 0.7 | 4 | 0/60 |
| Nunu & Willump | 3 | Experiment, Bruiser, Visionary | 800 | 50 | 50 | 50 | 0.6 | 1 | 50/100 |
| Renni | 3 | Chem-Baron, Bruiser | 850 | 50 | 50 | 55 | 0.65 | 1 | 40/90 |
| Scar | 3 | Firelight, Watcher | 800 | 50 | 50 | 50 | 0.65 | 1 | 80/170 |
| Smeech | 3 | Chem-Baron, Ambusher | 777 | 50 | 50 | 70 | 0.8 | 1 | 20/70 |
| Swain | 3 | Conqueror, Form Swapper, Sorcerer | 650 | 0 | unknown | 50 | 0.5 | 1 | 0/10 |
| Twisted Fate | 3 | Enforcer, Quickstriker | 700 | 25 | 25 | 35 | 0.7 | 4 | 25/75 |
| Ambessa | 4 | Emissary, Conqueror, Quickstriker | 1100 | 50 | 50 | 70 | 0.8 | 1 | 30/70 |
| Corki | 4 | Scrap, Artillerist | 850 | 30 | 30 | 70 | 0.75 | 4 | 0/60 |
| Dr. Mundo | 4 | Experiment, Dominator | 1100 | 66 | 66 | 60 | 0.65 | 1 | 30/100 |
| Ekko | 4 | Firelight, Scrap, Ambusher | 1100 | 60 | 60 | 50 | 0.85 | 1 | 0/50 |
| Elise | 4 | Black Rose, Form Swapper, Bruiser | 750 | 0 | unknown | 50 | 0.5 | 1 | 0/10 |
| Garen | 4 | Emissary, Watcher | 1000 | 60 | 60 | 65 | 0.6 | 1 | 60/125 |
| Heimerdinger | 4 | Academy, Visionary | 800 | 30 | 30 | 40 | 0.75 | 4 | 0/40 |
| Illaoi | 4 | Rebel, Sentinel | 1100 | 60 | 60 | 70 | 0.65 | 1 | 65/125 |
| Silco | 4 | Chem-Baron, Dominator | 800 | 30 | 30 | 40 | 0.75 | 4 | 30/80 |
| Twitch | 4 | Experiment, Sniper | 800 | 30 | 30 | 71 | 0.75 | 6 | 0/40 |
| Vi | 4 | Enforcer, Pit Fighter | 1100 | 50 | 50 | 75 | 0.85 | 1 | 40/100 |
| Zoe | 4 | Rebel, Sorcerer | 800 | 30 | 30 | 40 | 0.75 | 4 | 20/80 |
| Caitlyn | 5 | Enforcer, Sniper | 900 | 40 | 40 | 80 | 0.55 | 13 | 0/50 |
| Jayce | 5 | Academy, Form Swapper | 900 | 0 | unknown | 50 | 0.5 | 1 | 0/10 |
| Jinx | 5 | Rebel, Ambusher | 900 | 40 | 40 | 60 | 0.8 | 4 | 0/50 |
| LeBlanc | 5 | Black Rose, Sorcerer | 900 | 40 | 40 | 50 | 0.85 | 4 | 45/90 |
| Malzahar | 5 | Automata, Visionary | 950 | 40 | 40 | 45 | 0.8 | 4 | 30/90 |
| Mordekaiser | 5 | Conqueror, Dominator | 1200 | 70 | 70 | 75 | 0.55 | 1 | 25/100 |
| Rumble | 5 | Junker King, Scrap, Sentinel | 1200 | 65 | 65 | 60 | 0.8 | 1 | 40/120 |
| Sevika | 5 | High Roller, Chem-Baron, Pit Fighter | 1277 | 60 | 60 | 80 | 0.9 | 1 | 0/50 |

### Abilities

**Amumu** (1-cost) - Obsolete Technology: Passive: Reduce all incoming damage by 15/20/30. Every second, emit sparks that deal 10/15/25 (AP) magic damage to adjacent enemies.

**Darius** (1-cost) - Decimate: Spin, dealing 240% AD physical damage to adjacent enemies and healing 150/175/200 (AP). Apply a 200% AD physical damage bleed to target over 4 seconds.

**Draven** (1-cost) - Spinning Axes: Passive: If Draven has an empowered axe in hand, his next attack will throw it, dealing a total of 10/15/25 + 140/140/150% AD physical damage. Empowered axes return to Draven after hitting an enemy. Active: Spin an empowered axe.

**Irelia** (1-cost) - Defiant Dance: Enter a defensive stance and gain 400/475/575 (AP) Shield that rapidly decays over 3 seconds. When it expires, deal 70/100/150 (AP) magic damage + 75% of the damage absorbed to enemies around and in front of Irelia.

**Lux** (1-cost) - Prismatic Barrier: Grant 160/180/240 (AP) Shield to the lowest current Health ally. Lux's next attack deals 360/540/900 (AP) bonus magic damage.

**Maddie** (1-cost) - Fan the Hammer: Fire 6 shots towards the farthest enemy that deal 10/15/25 + 125/125/140% AD physical damage to the first enemy they hit.

**Morgana** (1-cost) - Tormented Soul: Curse the nearest non-cursed enemy, dealing 530/800/1500 (AP) magic damage over 10 seconds and reducing the effectiveness of shields used on them by 50%.

**Powder** (1-cost) - Misfit Toy: Send a monkey towards the largest group of enemies, causing a 2-hex radius explosion on impact. Enemies hit take 420/550/735 (AP) magic damage, reduced by 25% for each hex they are away from the epicenter. Wound and 1% Burn applied for 5 seconds to all enemies hit. Burn: Deal a percent of the target's max Health as true damage every second Wound: Reduce healing received by 33%

**Singed** (1-cost) - Dangerous Mutations: Gain [ModifiedDurability: unknown] (AP) Durability and grant the ally who has dealt the most damage this round 100/115/130% Attack Speed, decaying over 4 seconds.

**Steb** (1-cost) - Field Medicine: Passive: On heal, heal the 2 closest allies for 25% of the amount. Active: Heal for 260/290/360 (AP) and strike target for 260/390/585 (AP) magic damage.

**Trundle** (1-cost) - Desperate Chomp: Heal 200/220/250 (AP) and chomp target for 280% AD physical damage. Both effects are increased by up to 75% based on Trundle's missing Health.

**Vex** (1-cost) - Looming Darkness: Deal 240/360/580 (AP) magic damage to target and create a one-hex radius zone of darkness around them. After a brief delay, deal 120/180/290 (AP) magic damage to enemies still in the zone.

**Violet** (1-cost) - 1-2-3 Combo: Jab target 2 times for 135% AD physical damage each. Then uppercut them, dealing 20/30/45 + 330% AD (AD+AP) physical damage and briefly knocking them up.

**Zyra** (1-cost) - Grasping Roots: Send vines towards the current target, Stunning them for 1 second and dealing 280/420/630 (AP) magic damage. Then smaller vines seek out the 2 nearest enemies and deal 95/140/215 (AP) magic damage to them. Experiment Bonus: Ability damage bleeds enemies for (live counter)% bonus true damage over 2 seconds.

**Akali** (2-cost) - Shuriken Flip: Throw a shuriken at target, dealing 80/120/185 (AP) magic damage and marking them to take 15% more damage for 4 seconds. Then dash away from target. After a brief delay, dash towards them and deal 240/360/550 (AP) magic damage.

**Camille** (2-cost) - Adaptive Strike: Kick the target, dealing 30/45/70 + 245/245/265% AD (AD+AP) Adaptive Damage. Heal for 33% of the damage dealt. Adaptive Damage: Uses the damage type the target resists less

**Leona** (2-cost) - Eclipse: Fortify for 3 seconds, gaining [ModifiedDurability: unknown] (AP) Durability. Afterwards, deal 115/175/270 (AP) magic damage to adjacent enemies.

**Nocturne** (2-cost) - Overdrive Blades: For 6 seconds, attacks also cause adjacent enemies to bleed for 15/25/40 + 120/120/140% AD physical damage over 1 second.

**Rell** (2-cost) - Shattering Strike: Gain 300/350/400 (AP) Shield for 4 seconds. Lance enemies in a line for 120/180/270 (AP) magic damage and steal 10/12/15 Armor and Magic Resist from enemies hit.

**Renata Glasc** (2-cost) - Loyalty Program: Fire a pair of missiles at target. Allies they pass through gain 95/115/135 (AP) Shield for 3 seconds. When they collide, they deal 310/465/700 (AP) magic damage to target and 130/195/290 (AP) magic damage to adjacent enemies.

**Sett** (2-cost) - Facebreaker: Pull in an enemy on either side and slam them together, dealing 220/330/500 (AP) magic damage and Stunning them for 1.5/1.5/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%.

**Tristana** (2-cost) - Draw a Bead: Fire a cannonball at target, dealing 50/75/115 + 525% AD physical damage. If they die, the cannonball ricochets to the nearest enemy, dealing the overkill damage. When it does, permanently gain (live counter)% Attack Damage. (Current Bonus: (live counter)% AD)

**Urgot** (2-cost) - Corrosive Charge: Fire an explosive charge, dealing 35/50/75 + 300/300/310% AD physical damage to target and 35/50/75 + 150/150/155% AD physical damage to adjacent enemies. 20% Sunder all enemies hit for 6 seconds. Sunder: Reduce Armor Experiment Bonus: Dash to targets. On cast, gain (live counter)% max Health Shield and (live counter)% Attack Speed for 5 seconds.

**Vander** (2-cost) - Hound of the Underground: Stop attacking and brace for 2.5 seconds, gaining 100/130/180 (AP) Armor and Magic Resist. Vander's next attack is replaced with a strike that deals 400% AD physical damage, increased by 100% AD physical damage for each 1 or 2 cost champion on your team.

**Vladimir** (2-cost) - Transfusion: Heal 200/240/300 (AP) and deal 140/210/325 (AP) magic damage to the target.

**Zeri** (2-cost) - Living Battery: Passive: Every 3rd attack is replaced with a spark that deals 10/15/20 + 200% AD physical damage to target and 10/15/20 + 200% AD physical damage to 2 nearby enemies.

**Ziggs** (2-cost) - Bomb Full of Bombs: Toss a bomb at target, dealing 180/270/480 (AP) magic damage. 3 mini-bombs fly out, dealing 80/120/180 (AP) magic damage to random enemies.

**Blitzcrank** (3-cost) - Static Field: Passive: After surviving damage, deal 3% of the damage absorbed as magic damage to target. Active: Gain 550/600/700 (AP) Shield for 4 seconds. Shock the nearest 3 enemies for 40/60/100 (AP) magic damage and reduce their damage by 10% for 4 seconds.

**Cassiopeia** (3-cost) - Thorned Miasma: Blast target for 240/360/580 (AP) magic damage. Every third cast, splash miasma to 2 enemies within 3 hexes, dealing 220/330/530 (AP) magic damage to each.

**Ezreal** (3-cost) - Essence Flux: Fire a shot towards current target that deals 20/30/50 + 135% AD physical damage to all enemies within 1 hex. Then, deal 20/30/50 + 135% AD physical damage to the unit in the center of the blast.

**Gangplank** (3-cost) - Harvest from Flames: Restore 120/140/160 (AP) Health. Then slash, dealing 360/360/375% AD physical damage to enemies in a line. If only one enemy is hit, the damage is doubled.

**Kog'Maw** (3-cost) - Upgrading Barrage Module: Passive: Attacks deal 48/72/120 (AP) bonus magic damage. Active: Gain 25% stacking Attack Speed for the rest of combat. After every 3 casts, gain +1 Range.

**Loris** (3-cost) - Piltover Bulwark: Gain 600/700/900 (AP) Shield for 4 seconds. It redirects 50% of damage taken by adjacent allies. When it expires, deal 150/225/360 (AP) magic damage in a cone.

**Nami** (3-cost) - Ocean's Ebb: Launch a wave at target that bounces 3 times to enemies within 3 hexes and deals 120/180/290 (AP) magic damage.

**Nunu & Willump** (3-cost) - ZOMBIE POWER!!: For 3 seconds, gain 0.55/0.55/0.6 (AP) Durability and create a 2-hex cloud of noxious fumes that deals 30/45/65 (AP) magic damage to enemies within. Afterwards, detonate the cloud and deal 150/225/340 (AP) magic damage to all enemies within. Experiment Bonus: After dealing damage, deal (live counter)% max HP bonus magic damage (1s cooldown).

**Renni** (3-cost) - Sludgerunner's Smash: Heal 400 + 15% max HP (HP+AP) over 1.5 seconds. For the duration, raise target into the air, Stunning them, and dealing 500% AD physical damage. Afterwards, slam them down, dealing 200% AD physical damage to all adjacent enemies.

**Scar** (3-cost) - Sumpsnipe Surprise: Lob bombs at the nearest 3 enemies, Stunning them for 1.5/1.5/1.75 seconds and dealing 80/120/180 (AP) magic damage to each. Heal 245/255/270 (AP).

**Smeech** (3-cost) - Scrap Hacker: Leap towards the enemy with the fewest items within 4 hexes. Slash 3 times, dealing a total of 70/105/170 + 500% AD physical damage. If they die, leap again, dealing 30% less damage.

**Swain** (3-cost) - Demonic Ascension: Heal 240/300/380 (AP) and ascend for 6 seconds. While ascended, heal 70/95/145 (AP) and deal 40/60/95 (AP) magic damage to adjacent enemies every second. On takedown, the ascension's duration is extended by 2 seconds.

**Twisted Fate** (3-cost) - Wild Cards: Throw 3 cards at different targets. Blue Card: Restore 90/110/140 (AP) Health to the lowest Health ally. Red Card: Deal 120/180/275 (AP) magic damage to the largest circle of enemies. Yellow Card: Deal 240/360/560 (AP) magic damage to target and Stun them for 1 second.

**Ambessa** (4-cost) - Unrelenting Huntress: Ambessa switches between two stances on cast: Chains: Gain +1 Range. Attacks deal 150/150/400% AD physical damage. On cast, dash to target and strike in a half-circle, dealing 250/250/600% AD physical damage to enemies hit. Fists: Gain 25/25/45% (AP) Omnivamp and attack twice as fast. On cast, briefly Stun target before slamming them into the ground, dealing 500/500/1500% AD physical damage, then dash away.

**Corki** (4-cost) - Broadside Barrage: Lock onto target and strafe to a nearby position, unleashing 21/21/35 missiles split between the target and all enemies within two hexes. Each missile deals 6/9/36 + 35/35/60% AD physical damage and reduces Armor by 1. Every 7th missile deals 6/9/36 + 35/35/60% AD physical damage and reduces Armor by 1.

**Dr. Mundo** (4-cost) - Maximum Dosage: Become energized and heal 650/750/2500 + 15% max HP (HP+AP) over 2 seconds. While energized, deal 120/180/1000 (AP) magic damage to a nearby enemy each second. Afterwards, deal 7/7/35% max HP (HP) magic damage to all enemies within 2 hexes. Experiment Bonus: Gain (live counter) max Health. On each takedown, gain (live counter) more max Health.

**Ekko** (4-cost) - Splitting Seconds: Summon an assault of afterimages that deals 285/430/1200 (AP) magic damage to the target and 145/215/400 (AP) magic damage to 2/2/4 other nearby enemies. Afterimages reduce their target's Magic Resist by 5 for the rest of combat.

**Elise** (4-cost) - Cocoon: Jump to a nearby hex and web all enemies within 2 hexes, Stunning them for 1.75/2/8 seconds and dealing 120/180/1200 (AP) magic damage. Heal 400/450/2000 (AP).

**Garen** (4-cost) - Demacian Justice: Passive: After dealing damage, heal 1.5/1.5/5% max HP (HP). Active: Gain 225/250/1500 + 15% max HP (HP+AP) Shield for 4 seconds. Slam a massive sword on target, dealing 250/250/1800% AD physical damage to them and 125/125/1200% AD physical damage to enemies within 2 hexes.

**Heimerdinger** (4-cost) - PROGRESSSSS!: Fire 5/5/7 missiles at random enemies that deal 52/78/275 (AP) magic damage. Each cast fires 1 more missile than the last.

**Illaoi** (4-cost) - Test of Spirit: Gain 50/50/90% Durability for 3 seconds. Over the duration, drain 50/75/225 (AP) Health from the nearest 4/4/10 enemies. Then slam down, dealing 60/90/700% Armor + 60/90/700% MR (Armor+MR) magic damage to all enemies within 2 hexes.

**Silco** (4-cost) - Canned Monstrosity: Throw a canister at target, dealing 140/200/1000 (AP) magic damage to them and releasing 4/4/8 monstrosities. Monstrosities attack 5 times and deal 38/58/100 (AP) magic damage per attack.

**Twitch** (4-cost) - Spray and Pray: For the next 8 attacks, gain 75% Attack Speed, infinite range, and replace attacks with a piercing bolt that targets random enemies. Bolts deal 18/25/120 + 140/140/400% AD physical damage, reduced by 40/40/10% for each enemy they pass through. Experiment Bonus: After every 5 attacks, deal physical damage to the nearest enemy equal to (live counter)% of their max Health.

**Vi** (4-cost) - Wrecking Crew: Gain 280/325/1200 (AP) Shield for 3 seconds, then Stun target for 1.5 seconds. Slam them down, dealing 600/600/1200% AD physical damage to them and causing a shockwave in their row. Enemies hit take 180/180/500% AD physical damage and are briefly knocked up.

**Zoe** (4-cost) - Paddle Star!: Launch a star at target that deals 140/210/450 (AP) magic damage. It bounces to the farthest enemy within 4 hexes, then bounces back to the target. This effect repeats 2/2/4 times, hitting a different enemy each time.

**Caitlyn** (5-cost) - Air Raid: Enter a sniper's stance and call in an airship that circles the battlefield for 5 seconds, dropping 4/4/20 bombs at a random cluster of enemies over the duration. Bombs deal 20/30/100 + 180/180/750% AD (AD AP) physical damage in a one-hex circle. Whenever an enemy is caught in the epicenter of an Air Raid blast, reduce their Armor and Magic Resist by 20 and fire a shot towards them, dealing 260/260/1350% AD physical damage.

**Jayce** (5-cost) - Special Delivery: Passive: Summon a placeable Hextech Forge. On cast, the 3 allies closest to it gain 200/275/1800 (AP) Shield for 4 seconds. If it's dead, revive it with 100% Health. Active: Summon 2 Hexgates and knock target into one of them, dealing 600/600/8000% AD physical damage and sending them flying back to their original position. While flying, they deal 250/250/8000% AD physical damage to all other enemies in the path.

**Jinx** (5-cost) - Ruin Everything: Jinx alternates between Zap, Flame Chompers, and Death Rocket for her ability. Zap: Deal 265/265/2000% AD physical damage to enemies in a line and Stun them for 1.25/1.5/10 seconds. Flame Chompers: Deal 265/265/2000% AD physical damage to 3 1-hex circles of enemies. Death Rocket: Fire a rocket at the center of the board that 100/100/300 + 700/700/9001% AD (AD+AP) physical damage to ALL enemies, reduced by 10% for each hex they are away from the epicenter.

**LeBlanc** (5-cost) - The Chains of Fate: Chain together the nearest 4/4/20 enemies for 5 seconds, dealing 590/888/5000 (AP) magic damage split between them. When one takes damage, 12/20/100% of the amount is split as bonus true damage to the others. LeBlanc's next 3 attacks deal 150/225/900 (AP) bonus magic damage, increased by 50% for each enemy killed by the initial damage.

**Malzahar** (5-cost) - Call of the Machine: Summon a gate in a 5-hex line across target. Enemies hit take 100/150/1000 (AP) magic damage and are 20% Shredded for 4 seconds. Malzahar spreads 5 stacks of infection between enemies hit. Infection deals 16/24/400 (AP) magic damage per second for the rest of combat. This effect can stack infinitely. When an infected target dies, they spread their stacks to nearby enemies. Shred: Reduce Magic Resist

**Mordekaiser** (5-cost) - Grasp of the Iron Revenant: Briefly gain 40% Durability and summon a massive claw, dealing 160/240/800 (AP) magic damage to the most enemies in a line. The 4 closest enemies take 25% more damage and are pulled towards Mordekaiser. For the next 10 seconds, gain 30% Omnivamp, +1 Attack Range, and replace every attack with a slam that deals 330/500/3000 (AP) magic damage to target and 85/125/1500 (AP) magic damage to all other enemies within 2 hexes.

**Rumble** (5-cost) - The Equalizer: Call down a rain of 5 missiles on target's row that each deal 450/675/4000 (AP) magic damage, Wound, and 1% Burn units hit for 5 seconds. For each missile that doesn't hit an enemy, restore 20 Mana. If there's only 1 enemy left, fire all of the missiles at them. Burn: Deal a percent of the target's max Health as true damage every second Wound: Reduce healing received by 33% (live counter)

**Sevika** (5-cost) - Beat the Odds: Randomly cast 1 of 3 spells, with a chance of a Jackpot! Flamethrower: {{Spell_TFT13_LieutenantSpell1_Tooltip}} Extendo-Punch: {{Spell_TFT13_LieutenantSpell2_Tooltip}} Chomp: {{Spell_TFT13_LieutenantSpell3_Tooltip}}

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): Golem, Rift Scuttler, Training Dummy, Voidspawn, Krug, Murk Wolf, Murkwolf, Razorbeak, Rift Herald, Elder Dragon, ApheliosTurret, EliseSpider, Sion, SRU_Horde_Mini, Blue_Monkey, SwainDemonForm, Hextech Forge, Tome of Traits, Completed Item Anvil, Component Anvil, Artifact Item Anvil, Support item anvil, Mercenary Chest.

## Traits (26)

### Academy

The Academy sponsors 3 items each game. Copies of sponsored items grant bonus max Health and Damage Amp. Academy units holding sponsored items gain double the amount, plus an additional 5% Health and Damage Amp.

| Units | Effect |
|---|---|
| 3 | 2% HP Damage Amp; gain 1 sponsored item. |
| 4 | 3% HP Damage Amp; gain 1 sponsored item. |
| 5 | 5% HP Damage Amp; gain 1 sponsored item. |
| 6 | 9% HP Damage Amp |

Champions: Lux (1), Leona (2), Ezreal (3), Heimerdinger (4), Jayce (5)

### Ambusher

Damage from Ambushers' Abilities can critically strike. They also gain bonus Critical Strike Chance and Critical Strike Damage.

| Units | Effect |
|---|---|
| 2 | 25% Crit, 10% Crit Damage |
| 3 | 35% Crit, 20% Crit Damage |
| 4 | 45% Crit, 25% Crit Damage |
| 5 | 55% Crit, 25% Crit Damage; also gain 15% Durability |

Champions: Powder (1), Camille (2), Smeech (3), Ekko (4), Jinx (5)

### Artillerist

Every 5 attacks, Artillerists launch a rocket that deals 125% Attack Damage around the target. They also gain Attack Damage.

| Units | Effect |
|---|---|
| 2-3 | 10% AD |
| 4-5 | 45% AD |
| 6 | 70% AD, Launch a rocket every 3 attacks that deals double damage. |

Champions: Tristana (2), Urgot (2), Ezreal (3), Corki (4)

### Automata

Automata gain a crystal when they deal damage. At 20 crystals, they blast their current target, dealing magic damage + 25% of damage dealt since the previous blast and reset. They also gain Armor and Magic Resist.

| Units | Effect |
|---|---|
| 2-3 | 150 damage, 25 Armor+MR |
| 4-5 | 450 damage, 60 Armor+MR |
| 6 | 1337 damage, 111 Armor+MR |

Champions: Amumu (1), Nocturne (2), Blitzcrank (3), Kog'Maw (3), Malzahar (5)

### Black Rose

Each Black Rose champion's star level increases Sion's power

| Units | Effect |
|---|---|
| 3 | Summon a chained Sion. He is freed after 5 casts from Black Rose units or when he drops below 65% health. |
| 4 | Sion grows stronger and his enemies take more damage from Black Rose units. |
| 5-6 | Sion unleashes dark magic and heals to full Health when freed. |
| 7 | When Sion dies, he restores to life with power beyond death! |

Champions: Morgana (1), Vladimir (2), Cassiopeia (3), Elise (4), LeBlanc (5)

### Bruiser

Your team gains 100 max Health. Bruisers gain more.

| Units | Effect |
|---|---|
| 2-3 | 20% HP |
| 4-5 | 45% HP |
| 6 | 80% HP |

Champions: Steb (1), Trundle (1), Sett (2), Nunu & Willump (3), Renni (3), Elise (4)

### Chem-Baron

Gain Shimmer after each player combat. If your loss streak is at least 3, gain more. At each stack of 100 Shimmer, the Black Market offers you contraband that only Chem-Barons can use. Chem-Barons gain max Health for each Black Market you pass on. Markets Skipped: (live counter) Current Health: (live counter) HP

| Units | Effect |
|---|---|
| 3 | 15 or 35; 20 HP |
| 4 | 15 or 50; 60 HP |
| 5 | 20 or 70; 100 HP |
| 6 | 25 or 90; 150 HP |

Champions: Singed (1), Renata Glasc (2), Renni (3), Smeech (3), Silco (4), Sevika (5)

### Conqueror

Conquerors' takedowns grant stacks of Conquest. After gaining enough Conquest, open War Chests full of loot! Conquerors gain Attack Damage and Ability Power, increased by 8% for each War Chest opened. War Chests Opened: (live counter) Current Stats: (live counter)% AD+AP

| Units | Effect |
|---|---|
| 2-3 | 22% AD+AP; 1x Conquest |
| 4-5 | 33% AD+AP; 3x Conquest |
| 6-8 | 40% AD+AP; 6x Conquest |
| 9 | 120% AD+AP; 20x Conquest |

Champions: Darius (1), Draven (1), Rell (2), Swain (3), Ambessa (4), Mordekaiser (5)

### Dominator

Combat start: Dominators gain a Shield for 15 seconds. When Dominators cast, they gain stacking Ability Power based on the Mana spent.

| Units | Effect |
|---|---|
| 2-3 | 350 Shield, 25% AP |
| 4-5 | 550 Shield, 45% AP |
| 6 | 800 Shield, 65% AP |

Champions: Ziggs (2), Blitzcrank (3), Cassiopeia (3), Dr. Mundo (4), Silco (4), Mordekaiser (5)

### Emissary

This trait is active only when you have exactly 1 or 4 unique Emissaries. Ambessa: Allies gain 2 Armor and Magic Resist for each opponent defeated. ((live counter)/7) Garen: On Combat Start, Garen and allies to his left and right gain 12% of his max Health. Nami: Allies' attacks grant 3 bonus Mana. Tristana: Allies gain 6% Attack Speed per star level.

| Units | Effect |
|---|---|
| 1 | Gain that Emissary's bonus |
| 4 | Gain all bonuses. Emissaries gain 250 HP and 20% Damage Amp |

Champions: Tristana (2), Nami (3), Ambessa (4), Garen (4)

### Enforcer

Combat Start: Enforcers gain Shield and Damage Amp. The highest Health enemy units become WANTED! When a Wanted enemy dies, Enforcers gain 30% Attack Speed.

| Units | Effect |
|---|---|
| 2-3 | 1 unit; [PercentMaxHealthShield*100: unknown]% HP, 12% Damage Amp |
| 4-5 | 2 units; [PercentMaxHealthShield*100: unknown]% HP, 20% Damage Amp |
| 6-7 | 4 units; [PercentMaxHealthShield*100: unknown]% HP, 33% Damage Amp |
| 8-9 | 5 units; [PercentMaxHealthShield*100: unknown]% HP, 44% Damage Amp |
| 10 | ALL enemies; [PercentMaxHealthShield*100: unknown]% HP, 150% Damage Amp; Combat Start: Confiscate all enemy items! |

Champions: Maddie (1), Steb (1), Camille (2), Loris (3), Twisted Fate (3), Vi (4), Caitlyn (5)

### Experiment

Gain Laboratory hexes on your board. Combat start: Experiments standing on Laboratory hexes gain the Experiment bonuses of all Experiments on Laboratory hexes, plus max Health.

| Units | Effect |
|---|---|
| 3-4 | 2 Laboratories, 100 HP |
| 5-6 | 3 Laboratories, 450 HP |
| 7 | Experiment bonuses increase by 100%! |

Champions: Zyra (1), Urgot (2), Nunu & Willump (3), Dr. Mundo (4), Twitch (4)

### Family

Family members support each other, reducing their max Mana and gaining extra bonuses. (live counter)

| Units | Effect |
|---|---|
| 3 | 25% reduction, 20% Durability |
| 4 | 30% reduction, 20% AS |
| 5 | 40% reduction, heist on topside! After combat, progress the heist, increased for each surviving Family member! |

Champions: Powder (1), Violet (1), Vander (2)

### Firelight

Every 6 seconds, Firelights dash. While dashing, they attack with infinite range and heal a percentage of the damage taken since their last dash.

| Units | Effect |
|---|---|
| 2 | 25% of damage taken |
| 3 | 40% of damage taken |
| 4 | 45% of damage taken. While dashing, gain a massive burst of Attack Speed. |

Champions: Zeri (2), Scar (3), Ekko (4)

### Form Swapper

Innate: Form Swappers change their stats and ability based on if they're placed in the front 2 rows or back 2 rows. Frontline Form Swappers gain Durability. Backline Form Swappers gain Damage Amp.

| Units | Effect |
|---|---|
| 2 | 15% Durability or 20% Damage Amp |
| 3 | 22% Durability or 30% Damage Amp |
| 4 | 33% Durability or 44% Damage Amp |

Champions: Gangplank (3), Swain (3), Elise (4), Jayce (5)

### High Roller

When casting, Sevika rolls a random Jinx modification to her Ability and gains 80% Durability for 1.5 seconds. Mods: -Rocket: Launch 8 rockets that deal 100 physical damage each -Shield: Gain 500 Shield for 4 seconds -Coin: Gain 3 gold -Triple: Enhance her rolled ability! In Choncc's Treasure, the Coin modification grants less gold in earlier stages.

| Units | Effect |
|---|---|
| 1 | Bonus1NumRockets 8; Bonus1RocketDamage 100; Bonus2ShieldAmount 500; Bonus2ShieldDuration 4; Bonus4Gold 3; Durability 0.8; Duration 1.5 |

Champions: Sevika (5)

### Junker King

Every 3 rounds, open an armory to purchase permanent upgrades to your strongest Rumble's mech. Rounds Until Upgrade: (live counter)

| Units | Effect |
|---|---|
| 1 | NumRounds1Star 3; NumRounds1Star_HR 2 |

Champions: Rumble (5)

### Pit Fighter

Pit Fighters gain 15% Omnivamp and deal bonus true damage. Once per combat at 50% Health, they heal a percentage of their max Health over 2 seconds.

| Units | Effect |
|---|---|
| 2-3 | 6% true damage, 10% HP |
| 4-5 | 12% true damage, 35% HP |
| 6-7 | 22% true damage, 55% HP |
| 8 | 35% true damage, 80% HP |

Champions: Draven (1), Violet (1), Sett (2), Urgot (2), Gangplank (3), Vi (4), Sevika (5)

### Quickstriker

Quickstrikers move faster and gain Attack Speed, based on their target's missing Health.

| Units | Effect |
|---|---|
| 2 | 20-60% AS |
| 3 | 30-80% AS |
| 4 | 40-100% AS. On target death, Quickstrikers dash to a new target and gain 200 shield for 3 seconds. |

Champions: Akali (2), Nocturne (2), Twisted Fate (3), Ambessa (4)

### Rebel

Rebels gain 15% max Health. After your team loses 25% of their Health, a smoke signal appears, granting Rebels 60% Attack Speed for 5 seconds and extra power for the rest of combat.

| Units | Effect |
|---|---|
| 3-4 | 20% AD+AP |
| 5-6 | 40% AD+AP, 12% HP |
| 7-9 | 45% AD+AP, 15% HP, and Stun all enemies for 2 seconds |
| 10 | The smoke signal triggers at Combat Start and every 12 seconds. |

Champions: Irelia (1), Vex (1), Akali (2), Sett (2), Ezreal (3), Illaoi (4), Zoe (4), Jinx (5)

### Scrap

Combat start: Components held by Scrap units temporarily turn into full items. Scrap units gain Shield for 24 seconds for each component held by your team, including those that make up a full item. Shield Amount: (live counter)

| Units | Effect |
|---|---|
| 2-3 | 1 component, 25 Shield |
| 4-5 | 3 components, 35 Shield |
| 6-8 | All components, and full items become lucky! 45 Shield |
| 9 | Generate Radiant items! 60 Shield |

Champions: Powder (1), Trundle (1), Ziggs (2), Gangplank (3), Corki (4), Ekko (4), Rumble (5)

### Sentinel

Your team gains Armor and Magic Resist. Sentinels gain triple.

| Units | Effect |
|---|---|
| 2-3 | 12 Armor+MR |
| 4-5 | 25 Armor+MR |
| 6 | 42 Armor+MR |

Champions: Irelia (1), Singed (1), Leona (2), Rell (2), Loris (3), Illaoi (4), Rumble (5)

### Sniper

Snipers gain Damage Amp and deal more damage to targets farther away.

| Units | Effect |
|---|---|
| 2-3 | 5% Damage Amp; 5% damage per hex |
| 4-5 | 35% Damage Amp; 10% damage per hex |
| 6 | 75% Damage Amp; 20% damage per hex; Snipers gain +5 Attack Range |

Champions: Maddie (1), Zeri (2), Kog'Maw (3), Twitch (4), Caitlyn (5)

### Sorcerer

Your team gains 10 Ability Power. Sorcerers gain more.

| Units | Effect |
|---|---|
| 2-3 | 20 AP |
| 4-5 | 55 AP |
| 6 | 90 AP, Abilities reduce their target's damage by 20% for 3 seconds |

Champions: Lux (1), Zyra (1), Vladimir (2), Nami (3), Swain (3), Zoe (4), LeBlanc (5)

### Visionary

Whenever Visionaries gain Mana, they gain more.

| Units | Effect |
|---|---|
| 2-3 | 25% Mana |
| 4-5 | 50% Mana |
| 6 | 75% Mana, Abilities heal an ally for 16% of damage dealt |

Champions: Morgana (1), Vex (1), Rell (2), Renata Glasc (2), Nunu & Willump (3), Heimerdinger (4), Malzahar (5)

### Watcher

Watchers gain Durability, increased while above 50% Health.

| Units | Effect |
|---|---|
| 2-3 | 15% or 30% Durability |
| 4-5 | 25% or 45% Durability |
| 6 | 40% or 50% Durability |

Champions: Amumu (1), Darius (1), Vander (2), Vladimir (2), Scar (3), Garen (4)

## Traits with no champion (granted by augments, items or mechanics) (7)

### Geniuses

When Heimerdinger casts, Ekko releases 3 afterimages, each dealing 50% damage. When Ekko casts, Heimerdinger fires 3 missiles, each dealing 150% damage.

| Units | Effect |
|---|---|
| 2 | NumEkkoAfterImages 3; NumHeimerMissiles 3; ReducedEkkoDamage 0.5; ReducedHeimerDamage 1.5 |

### Martial Law

When Ambessa casts, Caitlyn fires an empowered attack at the target, dealing 200% damage. Ambessa gains 18% of Caitlyn's Attack Damage.

| Units | Effect |
|---|---|
| 2 | ADShare 0.18; CaitAD 2 |

### Menaces

Powder gains Dominator but no longer benefits from Family. When her monkey explodes, it creates 3 of Silco's monstrosities.

| Units | Effect |
|---|---|
| 2 | NumMonstrosities 3 |

### Reunion

When Vi casts, Ekko releases 3 afterimages towards her target each dealing 40% damage. When Ekko casts, Vi slams an earthquake towards his target dealing 100% damage.

| Units | Effect |
|---|---|
| 2 | EkkoAfterImageReducedDamage 0.4; ViEarthquakeReducedDamage 1 |

### Sisters

When Jinx scores a takedown, Vi gains 80% bonus Attack Damage for 10 seconds. When Vi scores a takedown, Jinx gains 80% bonus Attack Speed for 10 seconds.

| Units | Effect |
|---|---|
| 2 | ADBuff 0.8; ASBuff 0.8; JinxDuration 10; ViDuration 10 |

### Unlikely Duo

Jinx and Sevika gain 10% Attack Damage and 100 Health. Whenever one casts, they grant the other 10 mana. Sevika's arm is luckier.

| Units | Effect |
|---|---|
| 2 | AD 0.1; Health 100; Mana 10 |

### What Could Have Been

Vander gains [TFTTrait.TFT13_Teamup_BloodBrothers.1:MaxHealthGainPerCast: unknown] permanent max Health for each time Silco casts. Silco gains [TFTTrait.TFT13_Teamup_BloodBrothers.1:APGainPerDeath: unknown] permanent Ability Power for each time Vander dies. Bonuses: (live counter) HP / (live counter) AP

| Units | Effect |
|---|---|
| 2 | APGainPerDeath 6; MaxHealthGainPerCast 30 |

## Items

### Components (10)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| B.F. Sword |  | +10% AD | AD +10% Attack Damage |
| Chain Vest |  | +20 Armor | Armor +20 Armor |
| Frying Pan |  |  | ...why else would it be here? |
| Giant's Belt |  | +150 HP | HP +150 Health |
| Needlessly Large Rod |  | +10 AP | AP +10 Ability Power |
| Negatron Cloak |  | +20 MR | MR +20 Magic Resist |
| Recurve Bow |  | +10% AS | AS +10% Attack Speed |
| Sparring Gloves |  | +20% Crit | +20 Critical Strike Chance |
| Spatula |  |  | It must do something... |
| Tear of the Goddess |  | +15 Mana | Mana +15 Mana |

### Completed items (39)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Adaptive Helm | Negatron Cloak + Tear of the Goddess | +10 AP, +20 MR, +15 Mana | Combat start: Gain different bonuses based on starting position. Front Two Rows: 40 Armor and Magic Resist. Gain 1 Mana when struck by an attack. Back Two Rows: 15 Ability Power. Gain 10 Mana every 3 seconds. |
| Archangel's Staff | Needlessly Large Rod + Tear of the Goddess | +20 AP, +15 Mana | Combat start: Gain 30 Ability Power every 5 seconds in combat. |
| Bloodthirster | B.F. Sword + Negatron Cloak | +15% AD, +15 AP, +20 MR, +20% Omnivamp | Once per combat at 40% Health, gain a 25% max Health Shield that lasts up to 5 seconds. |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | +15% AD, +15 AP, +30 Mana, +5% DamageAmp | Gain 10 Mana after casting. When the holder gets a takedown, they deal 5% more damage for 8 seconds. [Unique - only 1 per champion] |
| Bramble Vest | Chain Vest + Chain Vest | +65 Armor | Gain 7% max health. Take 8% reduced damage from attacks. When struck by any attack, deal 100 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Crownguard | Needlessly Large Rod + Chain Vest | +20 AP, +20 Armor, +100 HP | Combat start: Gain a 25% max Health Shield for 8 seconds. When the shield expires, gain 25 Ability Power. |
| Deathblade | B.F. Sword + B.F. Sword | +55% AD | Perfect peace and calm for the holder - and all who face it. |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | +75 MR | Gain 9% max health. Every 2 seconds, heal 2.5% max Health. |
| Edge of Night | B.F. Sword + Chain Vest | +10% AD, +20 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, gain 15% bonus Attack Speed. [Unique - only 1 per champion] |
| Evenshroud | Negatron Cloak + Giant's Belt | +20 MR, +150 HP | 30% Sunder enemies within 2 hexes. Gain 25 Armor and Magic Resist for the first 15 seconds of combat. Sunder: Reduce Armor |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | +25 Armor, +25 MR, +100 HP | Gain 10 Armor and 10 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | B.F. Sword + Recurve Bow | +25% AD, +25 AP, +10% AS, +20% DamageAmp | Gain 20% additional Damage Amp against enemies with more than 1750 max Health. |
| Guardbreaker | Giant's Belt + Sparring Gloves | +10 AP, +20% AS, +150 HP, +20% Crit, +15% DamageAmp | After damaging a Shield, gain 15% additional Damage Amp for 3 seconds. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | +10 AP, +10% AS | Attacks grant 5% stacking Attack Speed. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | +15 Mana, +20% Crit | Gain 2 effects:15% Attack Damage and 15 Ability Power.12% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | +20% AD, +20 AP, +15 Omnivamp, +15% Omnivamp | Heal the lowest percent Health ally for 25% of damage dealt. Ally Healing: (live counter) |
| Infinity Edge | B.F. Sword + Sparring Gloves | +35% AD, +35% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | +15 AP, +25 MR, +150 HP | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 160% of the Mana spent. Shred: Reduce Magic Resist |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | +35 AP, +35% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Last Whisper | Recurve Bow + Sparring Gloves | +15% AD, +20% AS, +20% Crit | Physical damage 30% Sunders the target for 3 seconds. This effect does not stack. [Unique - only 1 per champion] Sunder: Reduce Armor |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | +25 AP, +10% AS, +150 HP | Attacks and Abilities deal 1% Burn and 33% Wound to enemies for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Nashor's Tooth | Recurve Bow + Giant's Belt | +10 AP, +10% AS, +150 HP | After casting an Ability, gain 60% Attack Speed for 5 seconds. |
| Protector's Vow | Tear of the Goddess + Chain Vest | +20 Armor, +30 Mana | Once per combat at 40% Health, gain a 25% max Health Shield that lasts 5 seconds and gain 20 Armor and 20 Magic Resist. |
| Quicksilver | Sparring Gloves + Negatron Cloak | +30% AS, +20 MR, +20% Crit | Combat start: Gain immunity to crowd control for 18 seconds. During this time, gain 3% Attack Speed every 2 seconds. [Unique - only 1 per champion] |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | +50 AP | This humble hat can help you make, or unmake, the world itself. |
| Red Buff | Recurve Bow + Recurve Bow | +35% AS | Attacks and Abilities 1% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Redemption | Tear of the Goddess + Giant's Belt | +150 HP, +15 Mana | Heal allies within 1 hex for 15% of their missing Health every 5 seconds. They also gain 10% Durability for 5 seconds (this does not stack). Healing: (live counter) |
| Runaan's Hurricane | Negatron Cloak + Recurve Bow | +25% AD, +10% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 60% Attack Damage AD as physical damage. |
| Spear of Shojin | B.F. Sword + Tear of the Goddess | +15% AD, +15 AP, +15 Mana | Attacks grant 5 bonus Mana. |
| Statikk Shiv | Recurve Bow + Tear of the Goddess | +15 AP, +15% AS, +15 Mana | Every 3rd attack deals 35 magic damage and 30% Shreds 4 enemies for 5 seconds. Shred: Reduce Magic Resist |
| Steadfast Heart | Chain Vest + Sparring Gloves | +20 Armor, +200 HP, +20% Crit | Gain 8% Durability. While above 50% Health, instead gain 15% Durability. |
| Sterak's Gage | B.F. Sword + Giant's Belt | +15% AD, +150 HP | Once per combat at 60% Health, gain 25% max Health and 35% Attack Damage. |
| Sunfire Cape | Chain Vest + Giant's Belt | +20 Armor, +150 HP | Gain 8% max Health. Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Tactician's Cape | Spatula + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold after 10 seconds of combat. "...and a bit of Luck." |
| Tactician's Crown | Spatula + Spatula |  | Your team gains +1 max team size. 10% chance to drop 1 gold when you win combat. "...the Heart of a hero..." |
| Tactician's Shield | Frying Pan + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold when the holder dies. "Imbued with a Philosopher's wisdom..." |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | +150 HP, +20% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] (live counter) |
| Titan's Resolve | Chain Vest + Recurve Bow | +10% AS, +20 Armor | Gain 2% Attack Damage and 2 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 20 Armor and 20 Magic Resist. |
| Warmog's Armor | Giant's Belt + Giant's Belt | +600 HP | Gain 12% max health. |

### Emblems (21)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Academy Emblem |  | +150 HP | The holder gains the Academy trait. This item is always considered to be sponsored. |
| Ambusher Emblem | Sparring Gloves + Frying Pan | +20% Crit | The holder gains the Ambusher trait. |
| Artillerist Emblem | B.F. Sword + Frying Pan | +10% AD | The holder gains the Artillerist trait. |
| Automata Emblem | Spatula + Negatron Cloak | +20 MR | The holder gains the Automata trait. |
| Black Rose Emblem | Needlessly Large Rod + Spatula | +10 AP | The holder gains the Black Rose trait. |
| Bruiser Emblem | Giant's Belt + Frying Pan | +150 HP | The holder gains the Bruiser trait. |
| Conqueror Emblem | B.F. Sword + Spatula | +10% AD | The holder gains the Conqueror trait. |
| Dominator Emblem |  | +150 HP | The holder gains the Dominator trait. |
| Enforcer Emblem | Spatula + Chain Vest | +20 Armor | The holder gains the Enforcer trait. |
| Experiment Emblem | Giant's Belt + Spatula | +150 HP | The holder gains the Experiment trait. Experiment Bonus: After 8 seconds or on death, summon a copy of this unit with (live counter)% max Health. |
| Family Emblem | Spatula + Tear of the Goddess | +15 Mana | The holder gains the Family trait. |
| Firelight Emblem | Sparring Gloves + Spatula | +20% Crit | The holder gains the Firelight trait. |
| Pit Fighter Emblem | Negatron Cloak + Frying Pan | +20 MR | The holder gains the Pit Fighter trait. |
| Quickstriker Emblem | Recurve Bow + Frying Pan | +10% AS | The holder gains the Quickstriker trait. |
| Rebel Emblem | Spatula + Recurve Bow | +10% AS | The holder gains the Rebel trait. |
| Scrap Emblem |  | +150 HP | The holder gains the Scrap trait. |
| Sentinel Emblem | Chain Vest + Frying Pan | +20 Armor | The holder gains the Sentinel trait. |
| Sniper Emblem |  | +150 HP | The holder gains the Sniper trait and +2 range. |
| Sorcerer Emblem | Needlessly Large Rod + Frying Pan | +10 AP | The holder gains the Sorcerer trait. |
| Visionary Emblem | Tear of the Goddess + Frying Pan | +15 Mana | The holder gains the Visionary trait. |
| Watcher Emblem |  | +150 HP | The holder gains the Watcher trait. |

### Artifacts (Ornn) (33)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Visage |  | +20 Armor, +20 MR, +400 HP | Heal 3.5% max Health every second. |
| Blacksmith's Gloves |  | +200 HP, +30% Crit | Each round: Equip 2 random Ornn Artifacts. [Consumes 3 item slots.] (live counter) |
| Blighting Jewel |  | +40 AP, +15 Mana | Dealing magic damage reduces the target's Magic Resist by 4. If their Magic Resist is 0, grant the holder 5 Mana instead. |
| Corrupt Vampiric Scepter |  | +60% AS | Attacks deal an additional 60% Attack Damage AD as physical damage and heal the holder for the damage dealt. The holder cannot cast their Ability or gain Mana. |
| Death's Defiance |  | +10% AD, +25% AS, +30 Armor, +25% Omnivamp | 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] |
| Deathfire Grasp |  | +30 AP, +15 Mana | Combat start: Blast the current target for 40% of their max Health as magic damage. Repeat this every 13 seconds. |
| Eternal Winter |  | +20 Armor, +200 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead (Cooldown: 15 seconds). [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Fishbones |  | +20% AD, +50% AS | Doubles the holder's attack range, and causes each of their attacks to target a random enemy. |
| Forbidden Idol |  | +25 Armor, +25 MR, +200 HP | Shields have 50% of their value converted to max Health instead. |
| Gold Collector |  | +25% AD, +30% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 33% chance to drop goldCoins 1 gold. Gold Collected: (live counter)g [Unique - only 1 per champion] |
| Horizon Focus |  | +20 Armor, +20 MR, +250 HP, +15 Mana | Stunning an enemy causes lightning to strike them, dealing 30% of their max Health as magic damage. |
| Hullcrusher |  | +35 Armor, +35 MR | Combat Start: If there are no adjacent allies, gain 600 Health, 20% Attack Damage, and 20 Ability Power. |
| Infinity Force |  | +25% AD, +25 AP, +25% AS, +25 Armor, +25 MR, +250 HP, +25 Mana | Tons of EVERYTHING! |
| Innervating Locket |  | +150 HP, +15 Mana | The holder gains 2% of their total Mana whenever they're hit by an attack. Each cast restores 20% of the holder's max Health over 3 seconds. |
| Lich Bane |  | +30 AP, +30% AS | The holder's first attack after each Ability cast deals (live counter) bonus magic damage. Damage increases based on Stage. |
| Lightshield Crest |  | +55 Armor, +55 MR | Every 3 seconds, Shields the lowest percent Health ally for 70% of the holder's combined Armor and Magic Resist for 5 seconds. On death grants this shield to all allies. |
| Luden's Tempest |  | +45% AD, +45 AP | 100% of overkill damage plus 100 is dealt as magic damage to the three enemies nearest the target. |
| Manazane |  | +15% AD, +10 AP, +15% AS, +15 Mana | After casting the first time in combat, gain 120 Mana over 5 seconds. [Unique - only 1 per champion] |
| Mittens |  | +60% AS | Shrinks the holder, granting them increased movement speed and immunity to Chill. Chill: reduce Attack Speed |
| Obsidian Cleaver |  | +150 HP | Damage dealt 30% Shreds and 30% Sunders enemies for 15 seconds. Your team gains 8% Attack Damage and 8 Ability Power. ​​[Support item] [Unique - only 1 per champion] Shred: Reduce Magic Resist Sunder: Reduce Armor |
| Prowler's Claw |  | +40% AD, +200 HP, +50% Crit | After killing a target, shed negative effects and dash to the farthest target within 4 hexes. The next 2 critical attacks deal 60% bonus Critical Strike Damage. |
| Randuin's Omen |  | +150 HP | Combat start: Grant Armor 30 Armor and MR 30 Magic Resistance to the holder and adjacent allies. ​​[Support item] |
| Rapid Firecannon |  | +66% AS | Gain +1 Attack Range, increased by 1 whenever the holder kills an enemy. |
| Seeker's Armguard |  | +30 AP, +30 Armor, +30 MR | Takedowns increase the holder's Armor, Magic Resist, and Ability Power by 15, increased to 20 if they score the kill. |
| Silvermere Dawn |  | +1.2 AD, +50 Armor, +50 MR | Grants immunity to Stuns and the holder's attacks Stun the target for 0.8 seconds. The holder's Attack Speed is locked at 0.5. |
| Sniper's Focus |  | +15% AD, +15 AP, +15% AS | Gain Range 2 Attack Range. Gain 9% Damage Amp against a target for each hex between the holder and the target. |
| Spectral Cutlass |  | +40% AD, +40 Armor, +40 MR, +20% Crit | Combat start: Teleport the holder to the mirrored hex on the enemy's side of the board. After 8 seconds, the holder returns to their original location. |
| Suspicious Trench Coat |  | +15% AS, +100 HP | Once per combat at 66% Health, the holder splits into 3 copies of themself each with 25% of their max Health. [Unique - only 1 per champion] |
| Talisman Of Ascension |  | +20% AD, +20 AP, +300 HP, +1.2 DamageAmp | After 22 seconds gain 100% max Health and 120% Damage Amp for the rest of combat. |
| Trickster's Glass |  | +10% AS, +10 Armor, +10 MR, +15% Crit | Summon a clone with 70% base Health and +10% max Mana. You cannot equip items to the clone. The clone benefits from active traits [Unique - only 1 per champion] |
| Unending Despair |  | +40 Armor, +350 HP | Whenever a Shield on the holder breaks, 150% of that Shield's initial value is dealt to the nearest enemy as magic damage. |
| Wit's End |  | +30% AS, +30 MR | Attacks deal (live counter) bonus magic damage. Heals the holder for 35% of all magic damage dealt. Damage increases based on Stage. |
| Zhonya's Paradox |  | +40 AP, +30 Armor, +30 MR | Once per combat at 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items (38)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Absolution |  | +400 HP, +15 Mana | Heal allies within 2 hexes for 25% of their missing Health every 5 seconds. They also gain 10% Durability for 5 seconds (this does not stack). Healing: (live counter) |
| Blessed Bloodthirster |  | +40% AD, +40 AP, +20 MR, +40% Omnivamp | Once per combat: At 40% Health, gain a 40% max Health Shield that lasts up to 5 seconds. |
| Blue Blessing |  | +60% AD, +60 AP, +30 Mana, +20% DamageAmp | Gain 10 Mana after casting. When the holder gets a takedown, they deal 20% more damage for 12 seconds. [Unique - only 1 per champion] |
| Brink of Dawn |  | +30% AD, +30 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, heal 100% missing health and gain 85% bonus Attack Speed. [Unique - only 1 per champion] |
| Bulwark's Oath |  | +40 Armor, +30 Mana | Once per combat at 40% Health, gain a 50% max Health Shield that lasts 10 seconds and gain 60 Armor and 60 Magic Resist. |
| Covalent Spark |  | +15 AP, +40 MR, +200 HP | 30% Shred enemies within 3 hexes. When enemies cast an Ability, deal magic damage equal to 200% of the Mana spent.. Also, heal 1.5% max Health per second. [Direct damage item] Shred: Reduce Magic Resist |
| Crest of Cinders |  | +60% AS | Attacks and Abilities 2% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Demonslayer |  | +50% AD, +50 AP, +10% AS, +30% DamageAmp | Gain 30% additional Damage Amp against enemies with more than 1750 max Health. |
| Dragon's Will |  | +115 MR | Gain 15% max health. Every 2 seconds, heal 10% max Health. |
| Dvarapala Stoneplate |  | +50 Armor, +50 MR, +250 HP | Gain 15 Armor and 15 Magic Resist for each enemy targeting the holder. Also, heal 1.5% max Health each second. |
| Equinox |  | +30 MR, +500 HP | 30% Sunder enemies within 3 hexes. Gain 70 Armor and Magic Resist for the first 20 seconds of combat. Sunder: Reduce Armor |
| Eternal Whisper |  | +45% AD, +25% AS, +55% Crit | Physical damage 30% Sunders the target for the rest of combat. This effect does not stack. [Unique - only 1 per champion] Sunder: Reduce Armor |
| Fist of Fairness |  | +15 Mana, +40% Crit | Gain 2 effects:35% Attack Damage and 35 Ability Power.20% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Glamorous Gauntlet |  | +70 AP, +75% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Guinsoo's Reckoning |  | +10 AP, +20% AS | Attacks grant 10% stacking Attack Speed. |
| Hextech Lifeblade |  | +40% AD, +40 AP, +35 Omnivamp, +30% Omnivamp | Heal the lowest percent Health ally for 40% of damage dealt. Ally Healing: (live counter) |
| Jak'sho the Protean |  | +25 AP, +30 MR, +15 Mana | Combat start: Gain different bonuses based on starting position. Front Two Rows: 60 Armor and Magic Resist. Gain 2 Mana when struck by an attack. Back Two Rows: 40 Ability Power. Gain 20 Mana every 3 seconds. |
| Legacy of the Colossus |  | +40 Armor, +500 HP, +20% Crit | Gain 16% durability. While above 40% Health, instead gain 30% Durability. |
| Luminous Deathblade |  | +1.05 AD | It glows in the presence of enemies. Or friends. Or anything alive, really. |
| More More-ellonomicon |  | +50 AP, +25% AS, +150 HP | Attacks and Abilities deal 2% Burn and 33% Wound to enemies for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Quickestsilver |  | +50% AS, +30 MR, +40% Crit | Combat start: Gain immunity to crowd control for 45 seconds. For 18 seconds, gain 7% Attack Speed every 2 seconds. [Unique - only 1 per champion] |
| Rabadon's Ascended Deathcap |  | +80 AP | It's witnessed - and unleashed - miracles and calamities both. |
| Rascal's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] (live counter) |
| Rosethorn Vest |  | +100 Armor | Gain 15% max health. Take 25% reduced damage from attacks. When struck by any attack, deal 175 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Royal Crownshield |  | +40 AP, +40 Armor, +200 HP | Combat start: Gain a 50% max Health Shield for 8 seconds. When the shield expires, gain 50 Ability Power. |
| Runaan's Tempest |  | +50% AD, +20% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 110% Attack Damage AD as physical damage. |
| Spear of Hirana |  | +35% AD, +35 AP, +20 Mana | Attacks grant 10 bonus Mana. |
| Statikk's Favor |  | +50 AP, +20% AS, +15 Mana | Every 3rd attack deals 95 magic damage and 30% Shreds 8 enemies for 5 seconds. Shred: Reduce Magic Resist |
| Sterak's Megashield |  | +30% AD, +400 HP | Once per combat at 60% Health, gain 40% max Health and 60% Attack Damage. |
| Sunlight Cape |  | +40 Armor, +300 HP | Gain 12% max Health. Every 1.5 seconds, deal 2% Burn and 33% Wound to an enemy within 3 hexes for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's max Health as true damage every second Wound: reduces healing received |
| The Baron's Gift |  | +30 AP, +20% AS, +200 HP | After casting an Ability, gain 120% Attack Speed for 8 seconds. |
| Titan's Vow |  | +30% AS, +35 Armor | Gain 3% Attack Damage and 3 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 50 Armor and 50 Magic Resist. |
| Urf-Angel's Staff |  | +60 AP, +15 Mana | Combat start: Gain 40 Ability Power every 4 seconds in combat. |
| Virtue of the Martyr |  | +150 HP | Every 5 seconds, heal your team for 7% of their max Health. When the holder dies, the healing increases to 14% max Health for 2 extra heals. Healing: (live counter) ​​[Support item] |
| Warmog's Pride |  | +1000 HP | Gain 20% max health. Heal 1.5% max Health per second. |
| Willbreaker |  | +30 AP, +30% AS, +150 HP, +20% Crit, +30% DamageAmp | After damaging a Shield, gain 30% additional Damage Amp for 3 seconds. |
| Zenith Edge |  | +70% AD, +75% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Zz'Rot Portal |  | +150 HP | Summon a large Voidspawn. Its strength increases with each Stage. ​​[Support item] [Unique - only 1 per champion] |

### Support items (11)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Accomplice's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random Support items. [Consumes 3 item slots.] (live counter) |
| Aegis of the Legion |  | +150 HP | Combat start: Grant 25% Attack Speed and 15 Armor and Magic Resist to the holder, adjacent allies in the same row, and all allies behind them for 12 seconds. ​​[Support item] |
| Banshee's Veil |  | +150 HP | Combat start: Grant the holder and allies within 1 hexes in the same row immunity to crowd control and 25% Attack Speed for 18 seconds. [Support item] [Unique - only 1 per champion] |
| Chalice of Power |  | +150 HP | Combat start: Grant 25 Ability Power and 10 Mana to the holder and allies within 2 hexes in the same row. ​​[Support item] |
| Knight's Vow |  | +50 HP, +15% Omnivamp | Combat start: Grant 200 Health and 15% Omnivamp to the holder and allies within 1 hex in the same row. ​​[Support item] |
| Locket of the Iron Solari |  | +150 HP | Combat start: The holder and allies within 2 hexes in the same row gain a 250 Shield, 20 Armor, and 20 Magic Resist for 20 seconds. ​​[Support item] |
| Moonstone Renewer |  | +150 HP | Every 4 seconds, grant a 55-325 Shield (based on Stage) to the 2 lowest percent health allies for 4 seconds. ​​[Support item] |
| Shroud of Stillness |  | +150 HP | Combat start: Shoot a beam that 30% Mana Reaves enemies. Your team gains 60 Health. [Support item] [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast (live counter) |
| The Eternal Flame |  | +150 HP, +8% DamageAmp | While the holder is alive, all enemies are 33% Wounded and all allies gain 8% Damage Amp. This effect refreshes every 5 seconds. ​​[Support item] Wound: Reduces healing received |
| Unstable Treasure Chest |  | +150 HP | When the holder dies, the 4 highest percent health allies gain a temporary completed item. ​​[Support item] |
| Zeke's Herald |  | +150 HP | Combat start: Grant AS 30% Attack Speed to the holder and allies within 2 hexes in the same row. ​​[Support item] |

### Set-specific and other items (52)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Confiscated! |  |  | This dangerous item has been confiscated by the Enforcers. You can have it back after serving your time. |
| Continue |  |  |  |
| Convert to Vials |  |  |  |
| Destabilized Chemtank |  | +15 Armor, +15 MR, +200 HP | At 50% Health, gain a 25% max Health Shield. Every 8 seconds afterwards, gain a 10% max Health Shield. Recommended users: Renni and Singed |
| Destabilized Chemtank II |  | +25 Armor, +25 MR, +350 HP | At 50% Health, gain a 50% max Health Shield. Every 8 seconds afterwards, gain a 10% max Health Shield. Recommended users: Renni and Singed |
| Determined Investor |  | +150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and goldCoins 5 gold. |
| Diamond Hands |  | +25 AP, +200 HP | Once per combat: At 50% Health, become invulnerable for 1.5 seconds and grant goldCoins 1 gold. [Unique - only 1 per champion] |
| Dimensional Heirloom |  | +5000 HP | Combat start: Creates a pocket Anomaly that summons powerful allies from across the Convergence every 4 seconds. {{TFT13_ChemBaronOnlyItem}} |
| Executioner's Chainblade |  | +25% AD, +10% AS | Attacks and Abilities execute enemies under 10% Health. On kill, gain 10% Durability and 10% Damage Amp for 5 seconds (refreshes on kills). Recommended users: Smeech and Sevika |
| Executioner's Chainblade II |  | +40% AD, +15% AS | Attacks and Abilities execute enemies under 15% Health. On kill, gain 15% Durability and 15% Damage Amp for 5 seconds (refreshes on kills). Recommended users: Smeech and Sevika |
| Finishing Touch! |  |  | Your mech is complete! Rumble permanently gains 25 Ability Power and 300 Health. |
| Flamethrower |  |  | Flamethrower: Constantly deal 30% / 45% / 600% AP magic damage in a cone. |
| Flesh Ripper |  | +15% AD, +20% Crit | After dropping below 40% Health, become briefly untargetable, restore 25% max Health, and gain 10% Attack Damage and 10% Omnivamp. {{TFT13_ChemBaronOnlyItem}} Recommended users: Smeech and Sevika |
| Flesh Ripper II |  | +25% AD, +35% Crit | After dropping below 40% Health, become briefly untargetable, restore 40% max Health, and gain 20% Attack Damage and 15% Omnivamp. {{TFT13_ChemBaronOnlyItem}} Recommended users: Smeech and Sevika |
| Gambler's Blade |  | +10 AP, +35% AS | Grant AS 1% bonus Attack Speed per goldCoins gold in your bank (up to goldCoins 30 gold). Each attack has a 5% chance to drop goldCoins 1 gold. [Unique - only 1 per champion] Gold generated this game: (live counter) |
| Gold |  |  | (live counter) Gold |
| Junkyard Titan! |  |  | Junkyard Titan!: Unlock all other upgrades! Gain 90% Durability. |
| Miniature Champion |  |  | Gain (live counter) AP Ability Power, (live counter) AD Attack Damage, (live counter) Armor Armor, and (live counter) MR Magic Resist. Original Champion: (live counter) |
| Mogul's Mail |  | +100 HP | Grants 1 Armor, 1 Magic Resist, and 7 Health when taking damage, stacking up to 35 times. At full stacks, grant goldCoins 1 gold and continue gaining goldCoins 1 gold every 8 seconds. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Needlessly Big Gem |  | +150 HP | Your team gains 5% Damage Amp. Each second the holder is alive, your team gains an additional 1% Damage Amp, up to a total of 20%. ​​[Support item] |
| Perfected Destabilized Chemtank |  | +30 Armor, +30 MR, +700 HP | At 50% Health, gain a 60% max Health Shield. Every 4 seconds afterwards, gain a 20% max Health Shield. Perfected Bonus: After gaining Shield, fire a vial of chemicals at 3 nearby enemies that deals 15% max Health magic damage. Recommended users: Renni and Singed |
| Perfected Executioner's Chainblade |  | +50% AD, +30% AS | Attacks and Abilities execute enemies under 20% Health. On kill, gain 15% Durability and 15% Damage Amp for 10 seconds (refreshes on kills). Perfected Bonus: Executes drop 3 Gold and heals the holder for 400. Recommended users: Smeech and Sevika |
| Perfected Flesh Ripper |  | +60% AD, +60% Crit | After dropping below 40% Health, become briefly Untargetable, restore 100% max Health, and gain 60% Attack Damage and 25% Omnivamp. Perfected Bonus: Upon dying for the first time, revive with 100% max Health! {{TFT13_ChemBaronOnlyItem}} Recommended users: Smeech and Sevika |
| Perfected Piltoven Hexarmor |  | +50 Armor, +50 MR, +300 HP | Gain 5 Armor and Magic Resist when taking damage, stacking up to 15 times. Perfected Bonus: At max stacks, deal 50% Armor+MR as true damage to enemies in a 4-hex radius after being attacked. (Cooldown: 3 seconds) Recommended users: Renni and Singed |
| Perfected Shimmer Bloom |  | +60 AP, +450 HP | Increase healing and shielding by 40%. Every 6 seconds, gain 400 Shield for 10 seconds and grant the same Shield to the 3 lowest Health allies. Perfected Bonus: Shielding or healing allies grants them 25 Ability Power and 25% Attack Speed. (Cooldown: 2s) Recommended users: Renni and Renata Glasc |
| Perfected Unleashed Toxins |  | +30 AP, +30 Mana | Attacks grant 10 bonus Mana. After casting, launch 4 missiles at the current target that deal 250 magic damage. Perfected Bonus: Casts also launch a wave of toxins that deals 150 magic damage. Recommended users: Silco and Renata Glasc |
| Perfected Virulent Virus |  | +40 AP, +45% AS | After dealing ability damage, enemies affected take an extra 25% damage over 3 seconds as true damage. Perfected Bonus: Infected enemies deal 15% less damage. When they die, they join your side with 800 Health that decays over 8 seconds. Recommended users: Silco and Renata Glasc |
| Perfected Voltaic Saber |  | +45% AD, +45 AP, +45% Omnivamp | After every 6 instances of damage, send jolts towards the 3 lowest health enemies, dealing 300 physical damage. Perfected Bonus: Every time the holder fires bolts, they gain 20% Attack Speed. Recommended users: Sevika and Smeech |
| Petricite Rod |  |  | Petricite Rod: Every 5 seconds, fire a blast at the furthest enemy within 4 hexes that deals 350% / 525% / 6000% AP magic damage over 15 seconds. |
| Piltoven Hexarmor |  | +25 Armor, +25 MR, +100 HP | Gain 3 Armor and Magic Resist when taking damage, stacking up to 15 times. At max stacks, deal 50% Armor+MR as magic damage to enemies in a 1-hex radius after being attacked. (Cooldown: 2 seconds) Recommended users: Renni and Singed |
| Piltoven Hexarmor II |  | +40 Armor, +40 MR, +175 HP | Gain 5 Armor and Magic Resist when taking damage, stacking up to 15 times. At max stacks, deal 75% Armor+MR as magic damage to enemies in a 1-hex radius after being attacked. (Cooldown: 2 seconds) Recommended users: Renni and Singed |
| Portable Anomaly |  | +25% AD, +30 AP, +350 HP | The holder gains your Anomaly evolution. |
| Repairing Microbots |  |  | Repairing Microbots: Every 3 seconds, restore 6% / 6% / 25% max HP. |
| Salvaged Contraption |  | +10 Armor, +10 MR, +200 HP | Reduce all incoming damage by 10. Recommended users: Renni and Singed |
| Salvaged Gauntlet |  | +25% AD, +10% AS | Attacks deal an additional 15 true damage. Recommended users: Smeech and Sevika |
| Salvaged Revolver |  | +20 AP, +15 Mana | Gain 10 Ability Power every 5 seconds. Recommended users: Silco and Renata Glasc |
| Self Destruct Button |  |  | Self Destruct Button: Increase Armor and Magic Resist by 25%. On death, deal 160% / 240% / 2000% Armor+MR as magic damage to enemies within 2 hexes. |
| Sentinel Swarm |  |  | Every 3 seconds, summon a powerful Sentinel equipped with Completed, Artifact, and Radiant items. |
| Shimmer Bloom |  | +20 AP, +150 HP | Increase healing and shielding by 20%. Every 8 seconds, gain 300 Shield for 10 seconds and grant the same Shield to the 2 lowest Health allies. Recommended users: Renni and Renata Glasc |
| Shimmer Bloom II |  | +35 AP, +250 HP | Increase healing and shielding by 35%. Every 8 seconds, gain 525 Shield for 10 seconds and grant the same Shield to the 2 lowest Health allies. Recommended users: Renni and Renata Glasc |
| Spare Parts! |  |  | Keep your gold! Rumble permanently gains 3 Ability Power and 50 Health. |
| Spite |  | +150 HP | Combat start: The holder and all adjacent allies gain 15% Attack Damage and 20 Ability Power. When the holder dies, Stun all enemies within 2-hexes for 2 seconds. ​​[Support item] |
| Tankbuster |  |  | Tankbuster: Every 3 seconds, deal 120% / 180% / 1600% AP as true damage to the enemy with the most Armor and Magic Resist. |
| Thief's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] (live counter) |
| Unleashed Toxins |  | +15 AP, +10 Mana | Attacks grant 5 bonus Mana. After casting, launch 3 missiles at the current target that deal 100 magic damage. Recommended users: Silco and Renata Glasc |
| Unleashed Toxins II |  | +25 AP, +15 Mana | Attacks grant 5 bonus Mana. After casting, launch 3 missiles at the current target that deal 175 magic damage. Recommended users: Silco and Renata Glasc |
| Virulent Virus |  | +25 AP, +15% AS | After dealing ability damage, enemies affected take an extra 15% damage over 3 seconds as true damage. Recommended users: Silco and Renata Glasc |
| Virulent Virus II |  | +40 AP, +25% AS | After dealing ability damage, enemies affected take an extra 25% damage over 3 seconds as true damage. Recommended users: Silco and Renata Glasc |
| Voltaic Saber |  | +15% AD, +15 AP, +20% Omnivamp | After every 6 instances of damage, send jolts towards the 2 lowest health enemies, dealing 150 physical damage. Recommended users: Sevika and Smeech |
| Voltaic Saber II |  | +25% AD, +25 AP, +35% Omnivamp | After every 6 instances of damage, send jolts towards the 2 lowest health enemies, dealing 250 physical damage. Recommended users: Sevika and Smeech |
| Zephyr |  | +150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. Your team gains 2% Attack Speed. [Support item] [Ignores crowd control immunity.] [Unique - only 1 per champion] (live counter) |
| Zz'Rot Portal |  | +10% AS, +150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. The Voidspawn has massively reduced effectiveness when created by a summoned unit. Taunt: enemies that are able and in range must attack the taunter |

### Consumables (31)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| A Chromatic Snack |  |  | Consume 3 player health for Health HP and Damage Amp Damage Amp. |
| Champion Duplicator |  |  | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Golden Item Remover |  |  | Use on a champion to unequip all items as many times as you want. Converts all future Magnetic Removers to Reforgers. |
| Lesser Champion Duplicator |  |  | Use on a 3-cost champion or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Lesser Shimmer Duplicator |  |  | Use on a 3 cost or lower Chem-Baron to create a 1-star copy of them. [Consumable - This item disappears when used.] |
| Loaded Dice |  |  | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (10 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (2 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (3 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (4 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (5 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (6 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (7 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (8 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (9 uses left!) |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Masterwork Upgrade |  |  | Use on a unit to open an armory containing Radiant versions of that unit's craftable completed items. Upgrade the item you choose to its Radiant equivalent, and unequip it. |
| Perfected Shimmer |  |  | Use on a Chem-Baron to restore 6 player health. [Consumable - This item disappears when used.] |
| Reforger |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (10 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (2 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (3 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (4 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (5 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (6 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (7 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (8 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (9 uses left!) |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Shimmer Duplicator |  |  | Use on a Chem-Baron to create a 1-star copy of them. [Consumable - This item disappears when used.] |
| Tiny Champion Duplicator |  |  | Use on a 1-cost champion to create a 1-star copy of them. |
| Training Dummy |  |  | Use on a champion to spawn a movable Training Dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |

## Augments (302)

Tier comes from the augment icon file name (I/1 = Silver, II/2 = Gold, III/3 = Prismatic); "unknown" where the name carries no marker. The list is the set's augment pool as stored in the source data and can include entries that were disabled or rotated during the set.

| Name | Tier | Effect |
|---|---|---|
| A Golden Find | Gold | Champions evolved by the Anomaly drop 2 gold every 3 kills. Gain 10 free rerolls. |
| A Magic Roll | Gold | Roll 3 dice. Gain rewards based on their total. Reward: (live counter) |
| Academic Research | Gold | Whenever you build an item, gain a completed item anvil instead. That anvil always offers an Academy item and the item built. Gain 1 random components. Gain a Lux and an Ezreal. |
| Academy Crest | Gold | Gain an Academy Emblem and a Leona. |
| Academy Crown | Prismatic | Gain an Academy Emblem and a random sponsored item. |
| Adrenaline Burst | Gold | Combat start and every 6 seconds, all Quickstrikers attack 85% faster for 2.5 seconds. Gain a Nocturne and Akali. |
| Aerial Warfare | Gold | Firelight champions gain 9% Attack Damage and 9 Ability Power when they start a Firelight dash. They gain another 3% for every hex traveled that combat. Gain a Scar and Zeri. |
| All That Shimmers | Gold | Choose a gold-generating Artifact items and gain a Magnetic Remover. |
| All That Shimmers+ | Gold | Choose a gold-generating Artifact. Gain a Magnetic Remover and 4 gold. |
| Ambusher Crest | Gold | Gain an Ambusher Emblem and a Camille. |
| Ambusher Crown | Prismatic | Gain an Ambusher Emblem, a Hand of Justice, and a Camille. |
| An Exalted Adventure | Prismatic | Gain three 2-cost champions. If you 3-star two of them, gain an orb filled with loot. Gain a Lesser Champion Duplicator at the start of next 2 stages. Champions: (live counter), (live counter), (live counter) |
| Anger Issues | Prismatic | All your current and future completed items transform into Guinsoo's Rageblades that grant 45 Armor and Magic Resist. Every 2 Rageblade stacks grant 1.5% Attack Damage and Ability Power. |
| Another Anomaly | Gold | After the Anomaly round on 4-6, gain an item that duplicates the chosen Anomaly Effect onto the equipped champion. |
| Artillerist Crest | Gold | Gain an Artillerist Emblem and a Tristana. |
| Artillerist Crown | Prismatic | Gain an Artillerist Emblem, a Runaan's Hurricane, and a Tristana. |
| At What Cost | Prismatic | Immediately go to level 6 and gain 4 XP. You don't get to choose your future augments. |
| Automata Crest | Gold | Gain an Automata Emblem and a Nocturne. |
| Automata Crown | Prismatic | Gain an Automata Emblem, a Guardbreaker, and a Nocturne. |
| Backup | Silver | Your team gains 12% Attack Speed if at least 4 allies start combat in the back two rows. |
| Bad Luck Protection | Gold | Your team can no longer critically strike. Convert each 1% Critical Strike Chance into 1% Attack Damage. Gain a Sparring Gloves. |
| Band of Thieves I | Silver | Gain 1 Thief's Gloves. |
| Battle Scars | Gold | Every time Watchers are attacked, they gain 2% Attack Damage. Every time Watchers are hit by an ability, they gain 4 Ability Power. Gain a Vander. |
| Beggars Can Be Choosers | Silver | You get +3 Augment rerolls for all other augment choices. Gain 7 gold. |
| Belt Overflow | Prismatic | Gain 5 Giant's Belts. Your Giant's Belts grant +60 bonus Health. |
| Big Grab Bag | Gold | Gain 3 random components, 2 gold, and 1 Reforger. Reforgers allow you to remake any item. |
| Birthday Present | Prismatic | Gain a 2-star champion and 1 gold every time you level up. The champion's cost tier is your level minus 4 (max: 5-cost). |
| Black Rose Crest | Gold | Gain a Black Rose Emblem. |
| Black Rose Crown | Prismatic | Gain a Black Rose Emblem, a Spear of Shojin, and a Cassiopeia. |
| Blade Dance | Silver | Gain an Irelia. Your strongest Irelia gains 60% Attack Speed and gains a brand new Ability that dashes her between two targets, dealing physical damage to both. |
| Blazing Soul I | Gold | Combat start: Your highest Attack Speed champion gains 20 Ability Power and 20% Attack Speed. Repeat on another ally every 3 seconds. |
| Blazing Soul II | Prismatic | Combat start: Your highest Attack Speed champion gains 45 Ability Power and 35% Attack Speed. Repeat on another ally every 3 seconds. |
| Blinding Speed | Prismatic | Gain a Giant Slayer, Guinsoo's Rageblade, a Recurve Bow and a Magnetic Remover. Useful for Attack Carries! |
| Blistering Strikes | Silver | Your team's attacks Burn their targets for 5% of their max Health over 5 seconds. Attacks also reduce their targets' healing received by 33%. |
| Branching Out | Silver | Gain a random Emblem and a Reforger. Reforgers allow you to remake any item. |
| BRB | Gold | You cannot perform actions for the next 3 rounds. Afterwards, gain 2 completed item anvils. |
| Bronze For Life I | Gold | Your team gains 3% Damage Amp for each Bronze-tier trait. Damage Amp Bonus: (live counter)% |
| Bronze For Life II | Prismatic | Your team gains 3.5% Damage Amp and 1% Durability for each Bronze-tier trait. Damage Amp Bonus: (live counter)% Durability Bonus: (live counter)% |
| Bruiser Crest | Gold | Gain a Bruiser Emblem and a Sett. |
| Bruiser Crown | Prismatic | Gain a Bruiser Emblem, a Redemption, and a Sett. |
| Brutal Revenge | Gold | Gain 2 Rennis. Your strongest Renni's Ability costs 10 less and causes her to lunge towards the farthest enemy within 2 hexes, dealing 110% damage to the target and reduced damage to enemies in her path. |
| Build a Bud! | Prismatic | Gain a random 3-star 1-cost champion and 10 gold. |
| Built Different | Gold | Your units with no Traits active gain 300-600 Health and 45-75% Attack Speed (based on current Stage). |
| Bulky Buddies I | Silver | Allies that start combat next to exactly 1 other ally gain 100 Health. When that champion dies, the other gains a 10% max Health Shield for 10 seconds. |
| Bulky Buddies II | Gold | Allies that start combat next to exactly 1 other ally gain 175 Health. When that champion dies, the other gains a 15% max Health Shield for 10 seconds. |
| Bulky Buddies III | Prismatic | Allies that start combat next to exactly 1 other ally gain 330 Health. When that champion dies, the other gains a 18% max Health Shield for 10 seconds. |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 6 rounds (including this round). |
| Calculated Enhancement | Prismatic | Each combat, 4 random champions in your last two rows gain 40% Attack Damage and 50 Ability Power. |
| Call to Chaos | Prismatic | Gain a powerful and random reward. Reward: (live counter) |
| Called Shot | Silver | Set your win streak to +4. Gain 4 gold. |
| Caretaker's Ally | Silver | Gain a random 2-cost champion now. Gain the same one again every time you level up. Champion: (live counter) |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4: component anvil Level 6: completed item anvil Level 7: choose 1 of 5 Radiant items |
| Caretaker's Favor | Gold | Gain a component anvil when you reach level 5, 6, 7, and 8. The anvil offers 4 choices. |
| Category Five | Gold | Gain a Runaan's Hurricane. Your Runaan's Hurricanes shoot 1 extra bolts, each dealing 85% of the original damage. |
| Clear Mind | Gold | If there are no champions on your bench at the end of player combat, gain 3 XP. |
| Climb The Ladder I | Silver | Each time an ally dies, allies that share at least one trait with them gain 3 Ability Power, 3% Attack Damage, 3 Armor, and 3 Magic Resist. |
| Climb The Ladder II | Gold | Each time an ally dies, allies that share at least one trait with them gain 6 Ability Power, 6% Attack Damage, 6 Armor, and 6 Magic Resist. |
| Clockwork Accelerator | Gold | Your team gains 10% Attack Speed every 3 seconds in combat. |
| Cloning Facility | Gold | Empower the hex in the center of the third row. Summon a clone of the champion in it with 80% base Health and 10% increased Mana cost. |
| Cluttered Mind | Gold | Gain 4 random 1-cost champions now. If your bench is full at the end of player combat, gain 3 XP. |
| Combat Medic | Silver | Gain a Steb. Your strongest Steb's Ability's Mana cost is reduced by 10, but no longer heals. Steb's spell grants 30% Omnivamp and strikes 3 times, each dealing 65%. |
| Component Buffet | Silver | Whenever you would get a component, gain a component anvil instead. Gain a random component. The anvil offers 4 choices. |
| Conqueror Crest | Gold | Gain a Conqueror Emblem and a Rell. |
| Conqueror Crown | Prismatic | Gain a Conqueror Emblem, a Last Whisper, and a Rell. |
| Cooking Pot | Gold | At the start of each turn, all units holding a Frying Pan or Spatula item grant the nearest champion 50 permanent Health. Gain a Frying Pan. |
| Coronation | Prismatic | Gain a Tactician's Crown. Tactician's Crown, Shield, and Cape grant the holder an additional 25% Attack Speed, 25% Attack Damage, and 35 Ability Power. |
| Corrosion | Silver | Enemy champions in the first two rows lose 3 Armor and Magic Resist every 2 seconds. |
| Crafted Crafting | Silver | Whenever you craft a completed item, gain 2 rerolls. |
| Crimson Pact | Gold | Gain a Vladimir. Your strongest Vladimir gains +3 Range and gains 5 bonus Mana each attack. His Ability no longer heals but grants 10% Damage Amp, deals 80% bonus damage, and spreads additional damage to the nearest enemy. |
| Crown Guarded | Gold | Gain a Crownguard. Your Crownguards' start of combat effect is 100% stronger and the shield lasts 5 seconds longer. |
| Crown's Will | Gold | Gain a Needlessly Large Rod. Your units gain 10 Ability Power and 10 Armor. |
| Delayed Start | Silver | Sell your board and bench. Gain 4 random 2-star 1 cost champions. Disable your Shop for the next 3 rounds. |
| Diversified Portfolio | Silver | Each round, gain 1 gold for every 3 non-unique traits active. Gain 1 gold. Gold Bonus: (live counter) |
| Diversified Portfolio+ | Silver | Each round, gain 1 gold for every 3 non-unique traits active. Gain 4 gold. Gold Bonus: (live counter) |
| Domination | Gold | Dominators gain 10% Attack Speed while shielded. When a Dominator gets a kill, all Dominators gain 100 Shield for 3 seconds. Gain a Cassiopeia. |
| Dominator Crest | Gold | Gain a Dominator Emblem and a Blitzcrank. |
| Dominator Crown | Prismatic | Gain a Dominator Emblem, a Protector's Vow, and a Blitzcrank. |
| Dual Purpose | Prismatic | The first time you buy XP each round, gain 2 gold. Whenever you buy XP, reroll your Shop. |
| Dummify | Silver | Lose all champions on your board and bench. Gain a Training Dummy with 100% of their combined health. The dummy gains 1000 Health per stage. |
| Duo Queue | Gold | Gain 2 random 5-cost champions and 2 copies of a random component. |
| Enforcer Crest | Gold | Gain an Enforcer Emblem. |
| Enforcer Crown | Prismatic | Gain an Enforcer Emblem, an Infinity Edge, and a Maddie. |
| Epoch | Gold | Now, and at the start of every stage, gain 6 XP and 2 free rerolls. |
| Epoch+ | Gold | Now, and at the start of every stage, gain 8 XP and 3 free rerolls. |
| Expected Unexpectedness | Prismatic | Now and at the start of the next 2 stages, roll 3 dice. Gain various rewards based on their total. |
| Experiment Crest | Gold | Gain an Experiment Emblem and an Urgot. |
| Experiment Crown | Prismatic | Gain an Experiment Emblem, a Bloodthirster, and an Urgot. |
| Eye For An Eye | Silver | For every 15 ally champions that die, gain a random component (max 4). Deaths Remaining: (live counter) |
| Eye For An Eye+ | Silver | Gain a random component. For every 16 ally champions that die, gain another component (max 3). Deaths Remaining: (live counter) |
| Family Crest | Gold | Gain a Family Emblem. |
| Family Crown | Prismatic | Gain a Family Emblem and a Redemption. |
| Final Polish | Prismatic | Gain a Support Anvil and a completed item anvil. |
| Find Your Center | Silver | Your champion that starts combat in the center of the front row gains 15% Damage Amp and 15% max Health. |
| Fine Vintage | Silver | Completed items left on your bench for 4 rounds transform into Support Anvils. |
| Firelight Crest | Gold | Gain a Firelight Emblem and a Zeri. |
| Firelight Crown | Prismatic | Gain a Firelight Emblem, a Protector's Vow, and a Scar. |
| Firesale | Silver | Each round, steal a random 3-cost or lower champion from the shop. Gain 1 gold. |
| Flexible | Prismatic | Gain 1 random emblem. At the start of every Stage, gain a random emblem. Your team gains 40 Health for each emblem they are holding. |
| Flurry of Blows | Prismatic | Gain a Zeke's Herald. Champions buffed by Zeke's also gain 45% Critical Strike Chance. |
| Forbidden Magic | Gold | Every 2 takedowns by Black Rose champions or Sion grant Sion permanent 3% Attack Damage and 15 Max Health. Gain 3 Black Rose champions. (Stacks: (live counter)) |
| Forward Thinking | Gold | Lose all your gold. After 5 player combats, gain back the original amount and another 70 gold. Incoming Gold: (live counter) |
| Fractured Crystals | Gold | When an Automata champion fires their blast, they fire a second blast at the closest enemy dealing 70% of the original damage. Gain an Amumu and Nocturne. |
| Ghost of Friends Past | Prismatic | Whenever an allied champion dies, your current team and all future allies permanently gain 7 health, 1% Attack Damage, or 1 Ability Power, based on the dying champion's role. Health: (live counter) Attack Damage: (live counter)% Ability Power: (live counter) |
| Glass Cannon I | Silver | Units that start combat in the back row begin combat at 90% health but gain 15% Damage Amp. |
| Glass Cannon II | Gold | Units that start combat in the back row begin combat at 90% health but gain 25% Damage Amp. |
| Gloves Off | Gold | Gain a Vander. Your strongest Vander gains a brand new Ability that no longer grants resistances, but deals 70% increased damage and punches the target backwards, dealing 35% of the original damage to all enemies hit. |
| Going Long | Prismatic | You no longer gain interest. Gain 15 gold now. Round start: gain 4 XP. Interest is extra gold you gain per 10g saved. |
| Gold For Dummies | Gold | Gain a Training Dummy. Every 10 seconds, all Training Dummies grant 1 gold. |
| Golemify | Gold | Lose all champions on your board and bench. Gain a Golem with 90% of their combined Health and 60% of their combined Attack Damage. The Golem gains 500 Health per stage. |
| Good For Something I | Silver | Champions that aren't holding items have a 50% to drop 1 gold on death. |
| Greater Moonlight | Prismatic | Combat Start: 1 random 1-cost champion is upgraded to 4-star for that round and gains 5% Attack Damage and 5 Ability Power. |
| Hall of Mirrors | Prismatic | Combat start: All champions in your front row become clones of the champion in the center of the row. Clones have 100% of their original health, and deal 40% less damage. |
| Hard Commit | Prismatic | Gain a random emblem. Now and at the start of each stage, gain a 1-star champion of that trait with a cost equal to the Stage (max 5). |
| Health is Wealth I | Silver | Your team gains 10% Omnivamp. Get a bonus of 8 gold when your team first accumulates 10000 total champion healing. Total Healing: (live counter) |
| Health is Wealth II | Gold | Your team gains 15% Omnivamp. Get a bonus of 15 gold when your team first accumulates 10000 total champion healing. Total Healing: (live counter) |
| Heavily Smash | Gold | Every 4 seconds, Bruisers deal 7% bonus max Health bonus physical damage on their next attack. Gain a Steb and a Trundle. |
| Hedge Fund | Prismatic | Gain 25 gold. Your max interest is increased to 10. Interest is extra gold you gain per 10g saved. |
| Heroic Grab Bag | Gold | Gain 2 Lesser Champion Duplicators and 9 gold. This item allows you to copy a 3-cost or less champion. |
| High Voltage | Gold | Gain an Ionic Spark. Your Ionic Sparks have +3 hex radius and do 25% more damage. |
| I'm the Carry Now | Prismatic | Get a Golem with tailored offensive items. It gets stronger at the start of each Stage. |
| Immovable Object | Prismatic | Gain a Randuin's Omen. Its range is increased by 1 hex and its effect is increased by 50%. |
| Inspiring Epitaph | Gold | When a unit dies, the nearest ally gains a 20% max Health Shield and 10% stacking Attack Speed. |
| Invested+ | Prismatic | Gain 26 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Invested++ | Prismatic | Gain 45 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Investment Strategy I | Gold | Your champions gain 8 permanent max health per interest you earn. |
| Investment Strategy II | Prismatic | Gain 5 gold. Your champions gain 9 permanent max health per interest you earn. Your max interest is increased to 7. |
| Iron Assets | Silver | Gain a component anvil and 4 gold. The anvil offers 4 choices. |
| Item Collector I | Silver | Your team gains 10 Health. For each unique item they are holding, your team gains bonus 2 Health, 1 Attack Damage, and 1 Ability Power. |
| Item Collector II | Gold | Your team gains 20 Health. For each unique item they are holding, your team gains bonus 5 Health, 1.5 Attack Damage, and 1.5 Ability Power. |
| Item Grab Bag I | Silver | Gain 1 random completed item. |
| Kingslayer | Silver | After winning player combat, gain 1 gold. If they had more health than you, gain 6 Gold instead. Gain 1 gold now. |
| Lategame Specialist | Silver | When you reach Level 9, gain 33 gold. |
| Latent Forge | Silver | After 8 player combats, gain an Artifact anvil. The anvil offers 4 choices. Artifacts are more powerful items with a unique effect. |
| Law Enforcement | Gold | Enforcer champions gain 10% Attack Damage. Every 6 Wanted enemy deaths grant 6 gold. Gain a Steb and Maddie. |
| Level Up! | Prismatic | When you buy XP, gain an additional 2. Gain 12 immediately. |
| Lineup | Silver | Your team gains 2.5 Armor and Magic Resist for each unit that starts combat in the front two rows. |
| Little Buddies | Gold | Your 4-cost and 5-cost champions gain 65 Health and 7% Attack Speed for every 1-cost and 2-cost champion on your board. |
| Living Forge | Prismatic | Gain an Artifact anvil now and after every 9 player combats. Artifacts are more powerful items with a unique effect. |
| Long Distance Pals | Gold | Combat start: Your 2 units furthest from each other form a bond, sharing 22% of their Armor, Magic Resist, Attack Damage, and Ability Power with each other. |
| Loot Explosion | Gold | Ambusher kills have a chance to drop loot, scaling with Critical Strike Chance. The value of the loot given can also critically strike, granting even more loot. Gain a Camille and a Powder. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves. In 7 rounds, get another Sparring Gloves. |
| Lucky Gloves+ | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 3 Sparring Gloves. |
| Lunch Money | Silver | Every 8 damage you deal to enemy tacticians gives you 2 gold. (Current: (live counter)) |
| Mace's Will | Gold | Gain a Sparring Gloves. Your team gains 8% Attack Speed and 20% Critical Strike Chance. |
| Mad Chemist | Silver | Gain a Singed. Your strongest Singed cannot attack but constantly runs, leaving a poison trail dealing magic damage over time. His Ability will always target himself and instead grants 30% Omnivamp and Move Speed. |
| Malicious Monetization | Gold | Gain 2 gold. For the next 3 rounds, enemy champions drop 2 gold when killed. |
| Manaflow I | Silver | Your units that start combat in the back row gain 2 additional Mana per attack. |
| Manaflow II | Gold | Your units that start combat in the back row gain 4 additional Mana per attack. |
| Max Cap | Prismatic | Your max level is 7. Gain 1 Tactician's Shield which increases your team size by +1, and 60 gold. |
| Mentorship I | Silver | If an ally starts combat next to a higher-cost ally, it gains 12% Attack Speed and 120 Health. |
| Mentorship II | Gold | If an ally starts combat next to a higher-cost ally, it gains 18% Attack Speed and 220 Health. |
| Missed Connections | Silver | Gain a copy of each 1-cost champion. |
| Moonlight | Gold | Combat start: 1 random 1-cost champion is upgraded to 3-star for that round and gains 45% Attack Damage and 45 Ability Power. |
| NO SCOUT NO PIVOT | Gold | Units can no longer be benched or sold after fighting in a player combat. After each player combat, units that fought gain 20 Health, 1.5% Attack Damage, and 1.5% Ability Power. |
| Noble Sacrifice | Gold | When your first ally dies each combat, grant your team 25 + 10% of that ally's Armor and Magic Resistance. |
| Not Today | Gold | Gain an Edge of Night. Champions holding this item gain 35% Attack Speed. |
| Noxian Guillotine | Gold | Conquerors execute enemies below 12% Health. When they do, they gain 5 Armor and Magic Resist for the rest of combat. Gain a Darius and Draven. |
| One Buff, Two Buff | Prismatic | Gain a Red Buff, a Blue Buff, and a Champion Duplicator. |
| One For All I | Silver | Your team gains 2% max Health and 1.5% Damage Amp for each unique one-cost champion on your board. Gain 2 one-costs. |
| One For All II | Gold | Your team gains 3% max Health and 2% Damage Amp for each unique one-cost champion on your board. Gain 3 one-costs. |
| One, Two, Five! | Silver | Gain 1 random component, 2 gold, and 1 random 5-cost champion. |
| Ones Twos Three | Silver | Gain 2 1-cost champion, 2 2-cost champions, and 1 3-cost champion. |
| Over Encumbered | Silver | For the next stage, you only get 1 bench slot. After, get 3 item components. |
| Overheal | Gold | Every third attack deals an additional 150% damage and heals 50% of the damage. Excess healing is converted to a shield up to 300 Health. |
| Paint the Town Blue | Gold | When the first 4 Rebels die each combat, summon a copy of themselves that is one-star lower with 400 less Health. Gain an Akali and an Irelia. |
| Pair of Fours | Gold | If your team has exactly 2 four-cost champions, they each gain 404 Health and 24.4% Attack Speed. Gain a random 4-cost. |
| Pandora's Bench | Silver | Gain 2 gold. At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Pandora's Items | Silver | Round start: items on your bench are randomized. Gain 1 random component. |
| Pandora's Items II | Gold | Round start: items on your bench are randomized. Gain 2 random components. |
| Pandora's Items III | Prismatic | Round start: items on your bench are randomized. Gain 1 random Radiant item. |
| Patience is a Virtue | Silver | Each round, gain 2 free rerolls if you did not buy a champion last round. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. |
| Phreaky Friday | Prismatic | Gain an Infinity Force. After 5 player combats, gain another. Infinity Force: Artifact that offers tons of offensive and defensive stats |
| Phreaky Friday + | Prismatic | Gain an Infinity Force. After 3 player combats, gain another. Infinity Force: Artifact that offers tons of offensive and defensive stats |
| Piercing Lotus I | Gold | Your team gains 5% Critical Strike chance, and their Abilities can critically strike. Critical strikes 20% Shred and Sunder the target for 3 seconds. |
| Piercing Lotus II | Prismatic | Your team gains 20% Critical Strike chance, and their Abilities can critically strike. Critical strikes 20% Shred and Sunder the target for 3 seconds. |
| Pilfer | Gold | Each round, gain a 1-star copy of the first champion you killed last combat. |
| Pit Fighter Crest | Gold | Gain a Pit Fighter Emblem and an Urgot. |
| Pit Fighter Crown | Prismatic | Gain a Pit Fighter Emblem, a Sterak's Gage, and a Gangplank. |
| Placebo | Silver | Gain 8 gold. Your team gains 1% Attack Speed. |
| Placebo+ | Silver | Gain 15 gold. Your team gains 1% Attack Speed. |
| Portable Forge | Gold | Choose 1 of 4 Artifacts. Artifacts are more powerful items with a unique effect. |
| Power Up | Silver | Your next augment is one tier higher. |
| Powered Shields | Gold | While Shielded, your units gain 10% Durability. The first time allies fall below 50% Health, they gain 100-225 Shield (based on Stage) for 3 seconds. |
| Prismatic Pipeline | Prismatic | The next non-player combat round will drop an additional Prismatic Orb full of amazing loot. |
| Prismatic Ticket | Prismatic | Each time your Shop is rerolled, you have a 45% chance to gain a free reroll. |
| Prizefighter | Gold | Gain 2 item components. Every 5 wins gives you an item component. |
| Pumping Up I | Silver | Your team gains 6% Attack Speed now. Each round after, they gain 0.5% more. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your team gains 8% Attack Speed now. Each round after, they gain 1% more. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your team gains 16% Attack Speed now. Each round after, they gain 2% more. (current Attack Speed: (live counter)%) |
| Pyromaniac | Gold | Gain a Red Buff. Your Burns deal 50% increased damage. |
| Quality Over Quantity | Prismatic | Units holding exactly 1 item upgrade that item to Radiant. Units holding Radiant items gain 4% Health. Gain 2 Magnetic Removers. Thief's Gloves counts as multiple items. |
| Quickstriker Crest | Gold | Gain a Quickstriker Emblem and an Akali. |
| Quickstriker Crown | Prismatic | Gain a Quickstriker Emblem, a Guinsoo's Rageblade, and a Nocturne. |
| Radiant Refactor | Prismatic | Gain a Masterwork Upgrade and 1 component anvil. Masterwork Upgrade upgrades an item to Radiant! |
| Radiant Relics | Prismatic | Choose 1 of 5 Radiant items. Gain a Magnetic Remover. Radiant items are very powerful versions of completed items. |
| Raining Gold | Gold | Gain 8 gold now and 1 gold every round. |
| Raining Gold+ | Gold | Gain 18 gold now and 1 gold every round. |
| Rebel Crest | Gold | Gain a Rebel Emblem and an Akali. |
| Rebel Crown | Prismatic | Gain a Rebel Emblem, a Jeweled Gauntlet, and an Akali. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 cost tier higher (max 5). Gain 2 Magnetic Removers. |
| ReinFOURcement | Gold | The next 4-cost champion you buy is instantly upgraded to 2-star. Get 12 gold. |
| Replication | Gold | Choose 1 of 3 components. For the next 2 rounds, gain a copy of that component. |
| Reroll Transfer | Silver | For every 1 unused Augment Reroll, gain 3 free shop rerolls. Gain 3 gold. Does not include the round this augment is selected. |
| Restart Mission | Silver | Remove all champions on your board and bench. Gain 2 random 2-star 3-costs, 3 2-star 2-costs, and 1 2-star 1-cost champion. |
| Rigged Shop+ | Silver | Your next shop and every 4 shops will contain all 3-cost champions. Gain 9 rerolls. |
| Risky Moves | Silver | Your Tactician loses 20 Health, but after 7 player combats, gain 30 gold. |
| Rocket Collection | Gold | Artillerist rockets deal 15% increased damage. Every 65 rockets your Artillerists fire, gain a Collector (max 2). Gain a Tristana and Urgot. (Rockets Fired: (live counter)) |
| Rocket Collection+ | Gold | Artillerist rockets deal 15% increased damage. Every 65 rockets your Artillerists fire, gain a Collector (max 2). Gain a Tristana and Urgot. (Rockets Fired: (live counter)) |
| Roll The Dice | Prismatic | Gain a Rascal's Gloves item. This equips 2 random Radiant items every round. Radiant items are very powerful versions of completed items. |
| Rolling For Days I | Silver | Gain 11 free Shop rerolls. |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 7 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 3 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin+ | Gold | Gain 1 random completed item now, and 1 component after 4 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Sated Spellweaver | Gold | After casting an Ability, champions gain 20% Omnivamp for 3 seconds. Excess healing is converted to a shield up to 300 Health. |
| Scapegoat | Gold | Gain a Training Dummy and 4 gold. If it is the first to die each player combat, gain 1 gold. |
| Scavenger | Gold | The first 5 enemy champions that are killed each combat grant a champion on your team a temporary completed item. |
| Scoreboard Scrapper | Gold | Every round, if you're in the bottom 4, your team permanently gains 1.5% Attack Damage and Ability Power. If you're in the top 4, they have 10% more Health. |
| Scrap Crest | Gold | Gain a Scrap Emblem and a Ziggs. |
| Scrap Crown | Prismatic | Gain a Scrap Emblem, a Ziggs, and 2 random components. |
| Sentinel Crest | Gold | Gain a Sentinel Emblem and a Rell. |
| Sentinel Crown | Prismatic | Gain a Sentinel Emblem, a Crownguard, and a Loris. |
| Shield Bash | Gold | Sentinels gain 5% bonus Armor and Magic Resist. Every 4 seconds, their next attack deals 75% of their total resistances as magic damage. Gain a Loris. |
| Shimmerscale Essence | Prismatic | Gain a Mogul's Mail. In 5 rounds, gain a Gamblers Blade. These items give gold as well as combat power. |
| Shop Glitch | Gold | During non-player combat rounds, your shop refreshes for free every 2.5 seconds for 30 seconds. |
| Shopping Spree | Prismatic | When you level up, gain a number of free shop refreshes equal to your level+1. Gain 4 gold. |
| Silver Spoon | Silver | Gain 10 XP. |
| Slammin' | Gold | Gain 1 random Component(s). After each player combat, if there are no items on your bench (other than Consumables), gain 2 XP. |
| Slammin'+ | Gold | Gain 1 random component(s) and 10 XP now. After each player combat, if there are no items on your bench (other than Consumables), gain 2 XP. |
| Sniper Crest | Gold | Gain a Sniper Emblem and a Zeri. |
| Sniper Crown | Prismatic | Gain a Sniper Emblem, an Infinity Edge, and a Zeri. |
| Sniper's Nest | Gold | Snipers gain +8% Damage Amp for each round fought from the same starting hex (Maximum +32%). Gain a Zeri. |
| Sorcerer Crest | Gold | Gain a Sorcerer Emblem and a Vladimir. |
| Sorcerer Crown | Prismatic | Gain a Sorcerer Emblem, an Adaptive Helm, and a Vladimir. |
| Spear's Will | Gold | Your team gains 10% Attack Damage and 10 Mana. Gain a B.F. Sword. |
| Spirit Link | Gold | Your team restores 6% of their max Health every 4 seconds. Increase the healing by 0.5% for every 10 missing player Health. |
| Spoils of War I | Silver | Enemies have a 25% chance to drop loot when killed. |
| Spoils of War II | Gold | Enemies have a 30% chance to drop loot when killed. |
| Spoils of War III | Prismatic | Enemies have a 45% chance to drop loot when killed. |
| Sponging | Prismatic | Combat start: Up to 6 champions with 1 or fewer items gain a copy of a random completed item from the nearest itemized ally. |
| Starry Night | Gold | 1-cost and 2-cost units in your shop have a chance to be 2-star. Gain 6 gold. Current Chances: (live counter)% Chances increase with player level. |
| Starry Night+ | Gold | 1-cost and 2-cost units in your shop have a chance to be 2-star. Gain 8 gold. Current Chances: (live counter)% Chances increase with player level. |
| Subscription Service | Prismatic | Now, and at the start of each Stage, open a Shop of 4 unique 4-cost champions and gain 8 gold. |
| Superstars I | Silver | Your team deals 5% more damage, increased by 2% for every 3-star champion on your team. Gain 2 rerolls. Bonus: (live counter)% |
| Superstars II | Gold | Your team deals 7% more damage, increased by 4% for every 3-star champion on your team. Gain 4 rerolls. Bonus: (live counter)% |
| Support Cache | Gold | Choose 1 of 4 Support items. |
| Support Mining | Silver | Gain a Training Dummy. When it dies 7 times, gain a random Support Item and remove the Training Dummy. |
| Support Mining+ | Silver | Gain a Training Dummy. When it dies 5 times, gain a random Support Item and remove the Training Dummy. |
| Survivor | Silver | After 3 players are eliminated, gain 92 gold. |
| Sword Overflow | Prismatic | Gain 5 BF Swords. Your BF Swords grant +3% Attack Speed. |
| Table Scraps | Silver | After the next 3 carousels, gain one unit that was not taken and its item. Gain 1 gold. |
| Tactician's Kitchen | Prismatic | Gain a random Emblem. After 3 player combats, gain a Tactician's Cape. |
| Team Building | Silver | Gain a Lesser Champion Duplicator. Gain another after 5 player combats. This item allows you to copy a 3-cost or less champion. |
| Teaming Up I | Silver | Gain 1 random component and 2 random Tier 3 champions. |
| Teaming Up II | Gold | Gain 1 random Support item and 2 random 4-cost champions. |
| The Golden Egg | Prismatic | Gain a golden egg that hatches in 11 turns for a huge amount of loot. Winning player combat speeds up the hatch timer by an extra turn. |
| The Mutation Survives | Gold | Experiments gain 12% Health and grant a special hex. The Experiment in the hex is killed on combat start, granting their Experiment bonus to other laboratory hexes. Gain 3 Experiment champions. |
| This WILL Work | unknown | Gain a Powder. Your strongest Powder's explosion radius is increased by two hexes, has different damage falloff, but deals 50% damage to ALLIES. |
| Thorn-Plated Armor | Gold | Gain a Bramble Vest. Your Bramble Vests deal 5-100% more damage (based on Stage) and heal the holder for 50% of the damage dealt. |
| Tiniest Titan | Prismatic | Gain 2 player health and 1 gold after every player combat. Your Tactician also moves faster. |
| Tiniest Titan+ | Prismatic | Gain 2 player health and 1 gold after every player combat. Your Tactician also moves faster. Gain 15 gold now. |
| Titanic Titan | Silver | Increase your current and max player health by 25. On carousel rounds you are released earlier, but are much slower. |
| Tomb Raider I | Gold | For the next 3 players eliminated, choose one of their completed items to keep. |
| Tomb Raider II | Prismatic | Gain 1 completed item Anvil. Every time a player is eliminated, choose one of their completed items to keep. |
| Top of the Scrap Heap | Gold | Every 6 components Scrap champions convert, gain a random component (max 5). Gain a Powder and a Trundle. Converted: (live counter)/6 |
| Tower Defense | Gold | Gain a Training Dummy equipped with a random emblem that fires ranged attacks at enemies. It upgrades as the game goes on. |
| Trade Sector | Gold | Gain a free Shop reroll every round. Gain 4 gold. |
| Training Arc | Gold | Pit Fighters permanently gain 1.5% Attack Damage if they lost the last combat. If they won, gain 50 Health instead. Gain an Urgot. |
| Trait Tracker | Gold | The first time you activate 8 non-unique traits for 1 combat, gain 6 random emblems. |
| Trait: Geniuses | Prismatic | When Heimerdinger casts, Ekko releases 3 afterimages, each dealing 50% damage. When Ekko casts, Heimerdinger fires 3 missiles, each dealing 150% damage. Gain a Heimerdinger, Ekko, and a Jeweled Gauntlet. |
| Trait: Martial Law | Gold | When Ambessa casts, Caitlyn fires an empowered attack at the target, dealing 200% damage. Ambessa gains 18% of Caitlyn's Attack Damage. Gain a Caitlyn and Ambessa. |
| Trait: Menaces | Gold | While fielded with Silco, Powder gains Dominator, but no longer benefits from Family. When her monkey explodes, it creates 3 of Silco's monstrosities. Gain a 2-star Powder and a Silco. |
| Trait: Reunion | Gold | When Vi casts, Ekko releases 3 afterimages towards her target dealing 40% damage. When Ekko casts, Vi slams an earthquake towards his target dealing 100% damage. Gain a Vi and Ekko. |
| Trait: Sisters | Gold | Gain the Sister Trait. When Vi scores a takedown, Jinx gains 80% bonus Attack Speed for 10 seconds. When Jinx scores a takedown, Vi gains 80% bonus Attack Damage for 10 seconds. Gain a Vi and Jinx. |
| Trait: Unlikely Duo | Gold | Jinx and Sevika gain 10% Attack Damage and 100 Health. Whenever one casts, they grant the other 10 mana. Sevika's arm is luckier. Gain a Jinx and Sevika. |
| Trait: What Could Have Been | Prismatic | Vander gains 30 permanent max Health each time Silco casts. Silco gains 6 permanent Ability Power each time Vander dies. Gain a 2-star Vander, a Silco, and a Spear of Shojin. |
| Trifecta I | Gold | Gain 2 3-cost champions. Combat Start: 3 random 3-cost champions gain 250 Health and 23% Attack Speed. |
| Trifecta II | Prismatic | Gain 3 3-cost champions. Allies gain 9% Attack Speed. Combat Start: 3 random 3-cost champions gain 330 Health and 33% Attack Speed. |
| Trolling | Silver | Gain a Trundle. Your strongest Trundle's Ability no longer heals, but grants him 140% Attack Speed for 5 seconds, and permanently grants him 1.5% Attack Damage. His maximum mana is reduced by 50. |
| Two Much Value | Gold | Gain 1 reroll for every 2 unique two-cost champions fielded last combat. Gain 2 two-cost unit. |
| Two Trick | Gold | Gain a random 2-star two-cost and 2 random 2-star one-cost champions. |
| Unleash The Beast | Gold | Gain a Sterak's Gage. When its effect triggers, the holder gains 35% Attack Speed for the rest of combat and immunity to crowd control for 5 seconds. |
| Upward Mobility | Prismatic | Buying XP costs 1 less. Gain 2 Health and 3 free rerolls whenever you level up. |
| Vampiric Vitality | Gold | You heal for 20% of the damage you deal to enemy Tacticians. Your units gain 12% Omnivamp. |
| Visionary Crest | Gold | Gain a Visionary Emblem and a Renata Glasc. |
| Visionary Crown | Prismatic | Gain a Visionary Emblem, a Spear of Shojin, and a Renata Glasc. |
| Void Swarm | Prismatic | Gain a Zz'Rot Portal and another after every 10 player combats. Zz'Rot Portal Voidlings gain 40% Attack Speed and 40% Omnivamp. |
| Void Swarm | Prismatic | Gain a Zz'Rot Portal and another after every 6 player combats. Zz'Rot Portal Voidlings gain 40% Attack Speed and 25% Omnivamp. |
| Voidcaller | Gold | For every 275 Mana Visionaries spend during combat, summon a Voidling, up to 5 Voidlings. Gain a Rell and Morgana. The Voidling has 400-600 Health based on Stage. |
| Wand Overflow | Prismatic | Gain 5 Needlessly Large Rods. Your Needlessly Large Rods grant +5% Attack Speed. |
| Wandering Trainer I | Gold | Gain 1 gold and a Training Dummy with 2 permanently attached Emblems. |
| Wandering Trainer II | Prismatic | Gain 6 gold and a Training Dummy with 3 permanently attached Emblems. |
| War for the Undercity | Gold | Everytime you choose to save your Shimmer on a Black Market, heal 4 player health and gain 6 gold. Gain a Renni. |
| Warpath | Gold | After dealing 60 player damage, gain a chest of high cost champions and items. (Current: (live counter)) |
| Watcher Crest | Gold | Gain a Watcher Emblem and a Vander. |
| Watcher Crown | Prismatic | Gain a Watcher Emblem, a Steadfast Heart, and a Scar. |
| Welcome to the Playground | Gold | If at least 2 Family members are alive after 15 seconds in combat or at the end of combat, gain a random copy of a Vander, Powder, or Violet. Gain a Powder and Violet. |
| What Doesn't Kill You | Gold | Gain 2 gold after losing a player combat. Gain a random component after every 4 losses. Losses until next component: (live counter) |
| Why Not Both? | Gold | While you field 2 of the same Form Swapper in different forms, both gain 30 Ability Power, Armor, and Magic Resist, and 30% Attack Damage. When you 3-star a Form Swapper, gain a 2-star copy. Gain a Swain and Gangplank. |
| Worth the Wait | Gold | Gain a random two-star 1-cost champion. After 2 rounds, gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Worth the Wait II | Prismatic | Gain a random 2-cost champion. Gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Young and Wild and Free | Silver | You can always move freely on Carousel rounds. Gain 5 gold. |

## Opening portals (39)

Source: MetaTFT lookup "portals" for Set 13.

| Name | Group | Effect |
|---|---|---|
| Golden Finale | Augments | The last augment offered this game will be gold tier. |
| Champion Delivery | Champions | Twice per stage, gain a high cost champion. The value goes up with game time. |
| Completed Anvil | Items | Start with 1 completed item anvil. |
| Golden Gala | Augments | Vander serves up all gold augments this game. |
| Loaded Carousels | Unique | Each champion on a component carousel has 2 components on them instead of 1. |
| Make 'Em Cook | Items | Start with a Spatula and a Frying Pan. Reforgers wont drop. |
| Loot Subscription | Unique | Every stage, gain random loot from a highly varied pool. |
| Treasure Armory | Unique | On stage 4-7, choose a 5-piece package of powerful loot. |
| Crab Rave | Unique | Dancing crabs replace monsters and drop bonus loot, but crabs on Stage 5+ are EXTREMELY DANGEROUS. |
| Upgraded Champion | Champions | Start with a 2-star 1-cost champion. |
| Spatula | Unique | Start with a Spatula. |
| Prismatic Prelude | Augments | Heimderdinger makes the first augment offered this game prismatic tier. |
| Champion Conference | Champions | Every stage, Caitlyn supplies you with a champion that fits your team. |
| Loot Subscription | Unique | Sevika scrounges you up some loot from a highly varied pool every other stage. All players gain the same loot. |
| Pot of Gold | Gold | At stage 6-1, all living players split a pot of 250 gold. |
| Champion Duplicator | Champions | Start the game with a Champion Duplicator. |
| Treasure Golems | Unique | Start with a Golem permanently equipped with an Unstable Treasure Chest. On Stage 3-5, the Golem equips another one. |
| Wind-up Monkey | Unique | Powder has released a wind-up monkey. Something will happen on 6-1! |
| Item Payout | Gold | Every time you build a completed item, gain 2 gold. |
| Tactician's Crown | Items | Start with a Tactician's Crown (gain +1 team size). |
| Component Anvils | Items | Vi gives you 2 component anvils. |
| Support Anvil | Items | Start with 1 Support item anvil. |
| Artifact Anvil | Items | Jayce crafts you an Artifact anvil. |
| 3-Cost Champion | Champions | Gremlin gave you a random 3-cost champion. |
| Ascending Augments | Augments | Ekko is tinkering with augment rarity this game. The augments will be silver, then gold, then prismatic |
| Anvil Buffet | Items | All items drop as component anvils. |
| Gold Opener | Gold | Start with 7 more gold. |
| Gold Subscription | Gold | Every other stage, Caitlyn gives everyone a random amount of gold. All players gain the same amount. |
| Prismatic Finale | Augments | Heimerdinger makes the last augment offered this game prismatic tier. |
| Gold Subscription | Gold | Every stage, Caitlyn gives everyone random amount of gold. All players gain the same amount. |
| Prismatic Party | Augments | Ekko makes all augments offered this game prismatic. |
| Radiant Item | Items | On stage 3-7, gain a Magnetic Remover and select 1 of 5 Radiant items (very powerful versions of completed items). |
| Scuttle Puddle | Unique | Monsters are replaced by crabs that drop bonus loot. |
| Mel's Blessing | Unique | Mel will grant you powerful loot at 40 player health. |
| Golden Prelude | Augments | The first augment offered this game will be gold tier. |
| Frying Pan | Unique | Start with a Frying Pan. |
| 3 Champions | Champions | Start with 3 random 1-cost champions. |
| Trainer Golems | Unique | Ambessa summons you a Golem with 3 permanently attached emblems. |
| No encounter. | Default | There is no encounter this game. |

## Anomalies (61)

Source: MetaTFT lookup ("goops" is the internal name for Anomaly effects).

| Anomaly | Type | Effect |
|---|---|---|
| Arcana Overwhelming | (Magic Damage) | Gain AP Ability Power equal to 25% of Magic Resist. Gain 65 Magic Resist. |
| Attack Expert | (Attack Damage) | This champion gains 55% more Attack Damage from all sources. |
| Avalanche of Armor | (Attack Damage) | Gain AD Attack Damage equal to 30% of Armor. Gain 50 Armor Armor. |
| Avarice Incarnate | (Damage Amp) | Gain 5 gold. Gain 15% Damage Amp and 1% more per 2 gold you have (up to 40%). |
| Berserker Rage | (Power) | Gain 25 Ability Power & 25% Attack Damage. Triple it when below 50% Health. |
| Bully | (Damage Amp) | Gain 12% Damage Amp, increased to 36% against units with a lower star level. |
| Bulwark | (Tank) | Gain 40 Armor & 40 Magic Resist. Triple it when above 50% health. |
| Calling Card | (Utility) | Gain an emblem that matches a non-unique trait from this champion. |
| Center of the Universe | (Power) | Gain 6 stars that orbit this champion. Each star deals 75 magic damage and the orbit expands to hit the champion's current target. |
| Comeback Story | (Damage Amp) | Gain 6% Damage Amp for every 10 player health you have lost this game. |
| Cosmic Rhythm | (Mana) | No longer gain Mana but instead cast their Ability every 4 seconds. |
| Cull The Weak | (Power) | Gain up to 100% Critical Strike Chance based on target's missing Health. Every 3 seconds, target the lowest percent Health enemy in range. |
| Deep Roots | (Tank) | Gain 700 Health, 25 Armor, and 25 Magic Resist. They cannot move or be stunned, and pull their target into range. |
| Defense Expert | (Tank) | This champion gains 66% more Armor and Magic Resist from all sources. |
| Diving In | (Power) | On first cast, leap to enemies and stun in a 2-hex radius for 1.5 seconds and gain 25% Attack Speed and 20% Omnivamp. |
| Dragonsoul | (Power) | This champion's first attack against a new target fires a Dragonsoul blast, dealing 20% max Health true damage. |
| Dramatic Entrance | (Utility) | Combat start: Leap off the battlefield. After 6 seconds, arrive with 55% increased max Health and Stun all enemies for 2 seconds. |
| Dual Wielding | (Utility) | Gain 2 completed items that fit this champion's role. |
| Eagle Eye | (Attack Damage) | Gain +1 Range. Every 2 seconds this champion doesn't move, gain 6% Attack Damage. |
| Energy Absorption | (Magic Damage) | Gain 30 Ability Power. Each time an ally dies, gain 5% of their Ability Power. |
| Essence of Navori | (Mana) | Max mana reduced by 10%, increased by 10% each time this champion casts (up to 40%). |
| Fireball | (Power) | For every 70 Mana spent, throw a 2-hex fireball that deals 6% max Health true damage and Burns for 5 seconds. |
| Force of Friendship | (Damage Amp) | Gain 1% Damage Amp for each star level on your board. Three-star champions grant 8% Damage Amp instead. |
| Fortified | (Tank) | Gain 12% max Health. Every 4 seconds, gain another 12% (up to 5 times). |
| Freestyling | (Damage Amp) | Gain 4.5% Damage Amp for each activated non-unique trait you have. |
| Giant-Sized | (Tank) | Gain 1000 Health and grow greatly in size. |
| Headhunter | (Attack Speed) | This champion gains 15% Attack Speed. Every 1 kills, they permanently gain 1% more (doubled in Hyper Roll). (Total Kills: (live counter)) |
| Heavy Hitter | (Tank) | Attacks deal bonus physical damage equal to 20% of this champion's max Health. (Cooldown: 2.5 seconds) |
| Hivemind | (Power) | Gain a copy of the first champion this champion kills each player combat. Gain 25% Damage Amp. |
| Hunger For Power | (Utility) | Combat start: Consume the nearest ally and gain 60% of their Health and Attack Damage. |
| Hypervelocity | (Attack Speed) | Gain 10% Attack Speed. On cast, gain 15% stacking Attack Speed for the rest of combat. |
| Into the Unknown | (Power) | This champion disappears for 3 rounds. When they return, they gain 50 Ability Power and 50% Attack Damage. |
| Invisibility | (Critical Strike Chance) | Every 6 seconds, turn invisible for 1 second. The next attack has 100% Critical Strike Chance. |
| Kill Streak | (Mana) | Gain 20 Mana each kill. |
| Knockout | (Attack Damage) | Gain 15% Attack Damage. The next attack after casting gains 35% Critical Strike Chance and deals 90% true damage. |
| Laser Eyes | (Magic Damage) | Constantly deal 100% Ability Power magic damage per second in a 3-hex line and Burn enemies for 3 seconds. |
| Last Chance | (Power) | On first death, revive to full Health, gain 50% Attack Speed and 20% Omnivamp but lose 12.5% Health each second. |
| Legacy Of Shurima | (Attack Speed) | After 10 seconds of combat, ascend, gaining 50% max Health and 60% Attack Speed |
| Mage Armor | (Tank) | Gain Armor and Magic Resist equal to 50% of Ability Power. |
| Magic Expert | (Magic Damage) | This champion gains 40% more Ability Power from all sources. |
| Magic Training | (Magic Damage) | Gain 15 Ability Power. On cast, your team gains 2 Ability Power for every 20 Mana spent. |
| Mini Mees | (Attack Damage) | Gain 3 minis that deal 30% (AD) Attack Damage every 3 attacks. Their attacks reduce Armor by 5. |
| Miniaturize | (Utility) | Transform into an item that gives 75% of their Attack Damage, Ability Power, Armor, and Magic Resist. Equipped items pop off. |
| Nothing Wasted | (Mana) | When an ally dies, gain 70% of their current Mana. |
| One Thousand Cuts | (Power) | Attacks deal 30 bonus true damage, plus 12 true damage for each time the target was damaged by this effect. |
| Power Absorption | (Attack Damage) | Gain 30% Attack Damage. Each time an ally dies, gain 9% of their Attack Damage. |
| Protective Shielding | (Tank) | Combat Start and every 8 seconds: Grant 12% max Health Shield to self and 2 lowest health allies for 4 seconds. |
| Repulsor | (Utility) | Gain 35% Attack Speed. On cast, knock back their target if within 2 hexes of them. |
| Scuttle Familiar | (Tank) | On death, summon a crab with 100% of their maximum Health, Armor, and Magic Resist. |
| Share Your Energy | (Utility) | Combat start: grant 15% of their Attack Damage, Ability Power, Health, Armor, and Magic Resist to adjacent allies. |
| Slime Time | (Magic Damage) | Every 2 seconds, deal 130% (AP) magic damage in a 1-hex radius and heal this champion for 100% of damage dealt. |
| Slow Cooker | (Tank) | Every second, deal magic damage in a 1-hex radius equal to 3% of their max Health. Radius increases every 6 seconds. |
| Stoneskin | (Tank) | Combat start: Gain 225 Armor and Magic Resist. Every second, reduce this bonus by 10. |
| Strength Training | (Attack Damage) | Gain 20% Attack Damage. Every 3 times they attack, your team gains 4% Attack Damage. |
| The Finisher | (Power) | This champion's attacks and Ability executes enemy champions at 15% health or lower. |
| Thornskin | (Tank) | Gain 60 Armor and 60 Magic Resist. When damaged, deal 20% of the damage blocked to adjacent enemies as magic damage. |
| Titanic Strikes | (Attack Damage) | Attacks deal an additional 40% Attack Damage to the target and adjacent enemies. |
| Touch of Frost | (Utility) | Ability casts 20% Chill (reduce Attack Speed) enemies for 4 seconds. When Chilled enemies die, they stun the nearest enemy for 1 second. |
| Ultimate Hero | (Utility) | After 3 rounds, star up a 3-star 1-cost champion to 4 stars! |
| Unstoppable Force | (Tank) | On takedown, charge the next target and Stun them for 1.5 seconds, then heal 10% max Health. |
| Wolf Familiars | (Attack Damage) | Summon 2 untargetable wolves with 50% of this champion's Attack Damage and 0.9 Attack Speed. |
