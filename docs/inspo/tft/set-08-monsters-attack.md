# TFT Set 8: Monsters Attack!

Sources:
- Community Dragon TFT data, patch 13.5: https://raw.communitydragon.org/13.5/cdragon/tft/en_us.json (champions, stats, abilities, traits, items, augments).
- Community Dragon character bins, patch 13.5: https://raw.communitydragon.org/13.5/game/data/characters/<unit>/<unit>.bin.json (spell calculations used to fill computed ability numbers).
- Community Dragon TFT data, patch 13.11: https://raw.communitydragon.org/13.11/cdragon/tft/en_us.json (item and augment pool lists).
- https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-12-23-notes/
- https://leagueoflegends.fandom.com/wiki/Monsters_Attack_(Teamfight_Tactics)

Retrieved 2026-10-15. Stats and numbers are from the data patch listed, which is the last patch of the set unless noted; they can differ from launch values. Bracketed tags in the mechanics lists name the source of each line.

## Overview

| Field | Value |
|---|---|
| Set number | 8 |
| Name | Monsters Attack! |
| Live patches | 12.23 to 13.5 |
| Live dates (NA patch dates) | 2022-12-07 to 2023-03-21 |
| Data patch used | 13.5 |
| Theme | Saturday-morning superhero and monster-movie setting: Mecha:PRIME, Star Guardian, Anima Squad, LaserCorps, Underground, Supers, Threats (kaiju monsters), Civilians, A.D.M.I.N., Gadgeteens, Ox Force. |
| Counts | 59 champion entries, 27 traits, 148 items, 291 augments |

## Set mechanics

- Hero Augments: every champion has two (one carry, one support). Offered at one of the three augment rounds (2-1, 3-2, 4-2) to all players at the same cost bucket, semi-tailored to your board. 5% of games have none. 118 at launch. [12.23 notes]
- Item Anvils: component anvil at 4-7 (pick 1 of 3 components); completed-item anvils at 5-7 and 6-7 (pick 1 of 5). PvE bosses from 5-7 (Aurelion Sol, Urgot, Zac) drop completed anvils, sometimes Tome of Traits or Enhanced (Ornn) anvils. [12.23 notes]
- A.D.M.I.N. trait: player programs combat triggers and effects (cause + effect pairs). [CDragon]
- Threat trait: monster units with no shared synergy that are strong for their cost and gain stats per Threat. [CDragon]
- Over 100 returning augments plus 63 Heart, Crown, Crest and Soul augments for the new traits. [12.23 notes]

## Economy and system changes

- Physical spells work like magic spells: cannot be dodged and cannot crit without Jeweled Gauntlet-style effects. Crit chance over 100% converts to crit damage at 2:1. [12.23 notes]
- Grievous Wounds 50% to 30%. AD bonuses changed from flat to percent of base AD. Item power lowered, champion base power raised. [12.23 notes]
- Keywords standardized: Burn, Chill, Disarm, Mana Reave, Omnivamp, Shred, Stun, Sunder, Taunt, Wound. [12.23 notes]

## Champions

Stats are 1-star base values. AS = attacks per second. Range is in hexes. Mana is start / max. Health and attack damage scale by star level (the wiki gives 180% at 2-star and 324% at 3-star). Ability numbers are listed as 1-star/2-star/3-star.

| Champion | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana |
|---|---|---|---|---|---|---|---|---|---|
| Ashe | 1 | LaserCorps, Recon | 500 | 15 | 15 | 60 | 0.7 | 4 | 0 / 50 |
| Blitzcrank | 1 | A.D.M.I.N., Brawler | 650 | 45 | 45 | 65 | 0.5 | 1 | 40 / 80 |
| Galio | 1 | Civilian, Mascot | 650 | 45 | 45 | 50 | 0.55 | 1 | 70 / 140 |
| Gangplank | 1 | Supers, Duelist | 650 | 30 | 30 | 45 | 0.65 | 1 | 0 / 50 |
| Kayle | 1 | Underground, Duelist | 500 | 15 | 15 | 50 | 0.65 | 4 | 0 / 30 |
| Lulu | 1 | Gadgeteen, Heart | 500 | 15 | 15 | 30 | 0.7 | 4 | 0 / 50 |
| Lux | 1 | Star Guardian, Spellslinger | 500 | 15 | 15 | 40 | 0.7 | 4 | 0 / 60 |
| Nasus | 1 | Anima Squad, Mascot | 650 | 40 | 40 | 65 | 0.65 | 1 | 0 / 60 |
| Poppy | 1 | Gadgeteen, Defender | 650 | 45 | 45 | 60 | 0.55 | 1 | 40 / 80 |
| Renekton | 1 | LaserCorps, Brawler | 650 | 40 | 40 | 50 | 0.65 | 1 | 0 / 50 |
| Sylas | 1 | Anima Squad, Renegade | 700 | 45 | 45 | 50 | 0.55 | 1 | 30 / 80 |
| Talon | 1 | Ox Force, Renegade | 600 | 25 | 25 | 50 | 0.7 | 1 | 0 / 70 |
| Wukong | 1 | Mecha:PRIME, Defender | 650 | 40 | 40 | 60 | 0.6 | 1 | 50 / 100 |
| Annie | 2 | Gadgeteen, Ox Force, Spellslinger | 750 | 40 | 40 | 40 | 0.6 | 2 | 30 / 90 |
| Camille | 2 | A.D.M.I.N., Renegade | 750 | 45 | 45 | 65 | 0.75 | 1 | 0 / 70 |
| Draven | 2 | Mecha:PRIME, Ace | 600 | 15 | 15 | 65 | 0.75 | 4 | 70 / 140 |
| Ezreal | 2 | Underground, Recon | 550 | 20 | 20 | 55 | 0.7 | 4 | 0 / 55 |
| Fiora | 2 | Ox Force, Duelist | 800 | 50 | 50 | 55 | 0.8 | 1 | 30 / 80 |
| Jinx | 2 | Anima Squad, Prankster | 550 | 20 | 20 | 50 | 0.7 | 4 | 20 / 80 |
| Lee Sin | 2 | Supers, Heart, Brawler | 700 | 50 | 50 | 60 | 0.65 | 1 | 0 / 70 |
| Malphite | 2 | Supers, Mascot | 800 | 45 | 45 | 60 | 0.5 | 1 | 40 / 80 |
| Rell | 2 | Star Guardian, Defender | 800 | 50 | 50 | 60 | 0.5 | 1 | 80 / 150 |
| Sivir | 2 | Civilian, Sureshot | 600 | 20 | 20 | 65 | 0.75 | 4 | 0 / 70 |
| Vi | 2 | Underground, Brawler, Aegis | 700 | 45 | 45 | 50 | 0.6 | 1 | 0 / 80 |
| Yasuo | 2 | LaserCorps, Duelist | 750 | 40 | 40 | 45 | 0.75 | 1 | 0 / 80 |
| Yuumi | 2 | Star Guardian, Mascot, Heart | 550 | 20 | 20 | 40 | 0.7 | 4 | 0 / 50 |
| Alistar | 3 | Ox Force, Mascot, Aegis | 800 | 50 | 50 | 50 | 0.55 | 1 | 75 / 175 |
| Cho'Gath | 3 | Threat | 950 | 55 | 55 | 60 | 0.65 | 1 | 40 / 120 |
| Jax | 3 | Mecha:PRIME, Brawler | 800 | 45 | 45 | 45 | 0.8 | 1 | 0 / 0 |
| Kai'Sa | 3 | Star Guardian, Recon | 650 | 25 | 25 | 35 | 0.8 | 4 | 0 / 60 |
| LeBlanc | 3 | A.D.M.I.N., Spellslinger, Hacker | 650 | 30 | 30 | 45 | 0.7 | 4 | 0 / 60 |
| Nilah | 3 | Star Guardian, Duelist | 800 | 50 | 50 | 45 | 0.7 | 1 | 50 / 125 |
| Rammus | 3 | Threat | 950 | 55 | 55 | 60 | 0.65 | 1 | 60 / 120 |
| Riven | 3 | Anima Squad, Brawler, Defender | 700 | 45 | 45 | 50 | 0.65 | 1 | 30 / 90 |
| Senna | 3 | LaserCorps, Sureshot | 650 | 25 | 25 | 70 | 0.7 | 4 | 0 / 70 |
| Sona | 3 | Underground, Spellslinger, Heart | 650 | 25 | 25 | 40 | 0.7 | 4 | 0 / 40 |
| Vayne | 3 | Anima Squad, Recon, Duelist | 600 | 25 | 25 | 60 | 0.75 | 4 | 0 / 40 |
| Vel'Koz | 3 | Threat | 750 | 30 | 30 | 50 | 0.65 | 4 | 0 / 70 |
| Zoe | 3 | Gadgeteen, Prankster, Hacker | 600 | 20 | 20 | 40 | 0.7 | 4 | 0 / 30 |
| Aurelion Sol | 4 | Threat | 850 | 30 | 30 | 40 | 0.75 | 4 | 30 / 90 |
| Bel'Veth | 4 | Threat | 1000 | 50 | 50 | 75 | 0.9 | 2 | 0 / 30 |
| Ekko | 4 | Star Guardian, Prankster, Aegis | 1000 | 60 | 60 | 60 | 0.6 | 1 | 70 / 140 |
| Miss Fortune | 4 | Anima Squad, Ace | 750 | 30 | 30 | 60 | 0.7 | 4 | 0 / 120 |
| Samira | 4 | Underground, Sureshot, Ace | 700 | 30 | 30 | 70 | 0.75 | 4 | 0 / 75 |
| Sejuani | 4 | LaserCorps, Brawler | 1000 | 70 | 70 | 70 | 0.55 | 1 | 80 / 180 |
| Sett | 4 | Mecha:PRIME, Defender | 1000 | 60 | 60 | 90 | 0.65 | 1 | 80 / 180 |
| Soraka | 4 | A.D.M.I.N., Heart | 750 | 30 | 30 | 50 | 0.65 | 4 | 0 / 40 |
| Taliyah | 4 | Star Guardian, Spellslinger | 700 | 30 | 30 | 45 | 0.75 | 4 | 0 / 90 |
| Viego | 4 | Ox Force, Renegade | 950 | 50 | 50 | 60 | 0.75 | 1 | 0 / 50 |
| Zac | 4 | Threat | 1050 | 65 | 65 | 60 | 0.75 | 1 | 0 / 100 |
| Zed | 4 | LaserCorps, Duelist, Hacker | 1000 | 45 | 45 | 80 | 0.85 | 1 | 0 / 40 |
| Aphelios | 5 | Ox Force, Sureshot, Arsenal | 800 | 30 | 30 | 80 | 0.9 | 4 | 40 / 140 |
| Fiddlesticks | 5 | Corrupted, Threat | 1300 | 70 | 70 | 50 | 0.75 | 1 | 66 / 66 |
| Janna | 5 | Civilian, Spellslinger, Forecaster | 800 | 35 | 35 | 50 | 0.75 | 4 | 70 / 140 |
| Leona | 5 | Mecha:PRIME, Renegade, Aegis | 1100 | 60 | 60 | 75 | 0.7 | 1 | 0 / 75 |
| Mordekaiser | 5 | LaserCorps, Ace | 1200 | 70 | 70 | 70 | 0.7 | 1 | 60 / 150 |
| Nunu & Willump | 5 | Gadgeteen, Mascot | 1337 | 60 | 60 | 0 | 0.2 | 1 | 0 / 90 |
| Syndra | 5 | Star Guardian, Heart | 850 | 40 | 40 | 40 | 0.85 | 4 | 70 / 125 |
| Urgot | 5 | Threat | 1000 | 40 | 40 | 65 | 1 | 3 | 75 / 175 |

Units in the data without traits (summons, PvE, props) are left out: Archive Of Augments, Giant Crabgot, Hackerim, Mercenary Chest, Mutant Zac, Target Dummy, The Golden Egg, Volcanic Sol.

### Abilities

Computed values assume 100 Ability Power (the TFT base) and no items. Terms that scale with a stat other than AP are written as a percent of that stat (for example "275% AD"). "[unknown: X]" marks a value the data did not let us compute.

**Ashe** (1-cost) - Ranger's Focus: Ashe gains (12/18/25 + 40% AD) Attack Damage (40% Attack Damage + 12/18/25) for the next 5 seconds.

**Blitzcrank** (1-cost) - Static Defenses: Blitzcrank creates an empowered field around himself, reducing all damage taken by [unknown: ModifiedDR] for 4 seconds.

**Galio** (1-cost) - Outta My Way!: Galio dashes towards the nearest enemy, dealing 150/225/335 magic damage. He then grants himself and the closest ally a 225/250/275 Health shield for 4 seconds.

**Gangplank** (1-cost) - Trial by Fire: Gangplank fires a flaming bullet at his target, which deals 210/315/475 magic damage and 33% Wounds the enemy for 8 seconds. Wound: reduces healing received

**Kayle** (1-cost) - Starfire Spellblade: Kayle's next 3 attacks deal 155% AD physical damage (155% Attack Damage) to her target, and 25/35/50 magic damage to enemies within 1 hex of the target.

**Lulu** (1-cost) - Glitterlance: Lulu fires a magic bolt at her target, dealing 270/405/605 magic damage to the first enemy hit and 135/205/305 to other enemies behind.

**Lux** (1-cost) - Lucent Singularity: Lux sends out a singularity to a random enemy. When it detonates, it reduces the target's Attack Damage by 40% for 4 seconds, and deals 200/300/450 to nearby enemies.

**Nasus** (1-cost) - Bonk!: Nasus slams his staff on his target, dealing (60/90/135 + 200% AD) physical damage (200% Attack Damage + 60/90/135) and 40% Chilling them for 3 seconds. Chill: reduce Attack Speed

**Poppy** (1-cost) - Buckler Toss: Poppy throws her buckler at the farthest enemy, dealing 160/240/320% of her Armor as magic damage. (total: 160%/240%/320% Armor) The buckler then bounces back, granting Poppy a shield that blocks 300/350/425 damage.

**Renekton** (1-cost) - Dominus: Renekton empowers himself for 8 seconds, gaining 200/250/300 maximum Health, and causing his attacks to deal 6% max Health (6% Maximum Health) as bonus magic damage.

**Sylas** (1-cost) - Battle Blast: Sylas whirls his chains around him, dealing 10%/12%/15% max Health (10/12/15% Maximum Health) magic damage to nearby enemies and healing himself for 150/170/190 Health.

**Talon** (1-cost) - OX-ian Diplomacy: Talon jumps to the furthest unit from the center of the board and stabs them for 170/250/370 magic damage. If this kills them, Talon heals for 150/200/250 Health.

**Wukong** (1-cost) - Stone Skin: Wukong spins his staff around him, dealing 140/210/315 magic damage to nearby enemies. He gains 60/70/80 Armor and Magic Resist for each enemy hit for 3 seconds.

**Annie** (2-cost) - Burst Shield: Annie blasts a cone of fire, dealing 140/210/325 magic damage to enemies in front of her, then creates a 300/350/425 Health shield on herself for 4 seconds.

**Camille** (2-cost) - Tactical Sweep: Camille sweeps with her leg, dealing (60/75/100 + 210% AD) physical damage (210% Attack Damage + 60/75/100) and Disarming enemies hit for 1.5/1.6/1.75 seconds. Disarm: target cannot move or attack

**Draven** (2-cost) - Whirling Death: Draven hurls 2 axes at his target's direction, which then come back to him. Enemies caught in their path take (60/95/145 + 130% AD) physical damage (130% Attack Damage + 60/95/145), reduced by 25% for each enemy they have already hit.

**Ezreal** (2-cost) - Sabotage : Ezreal fires an orb of physical sabotage at his target, 30/40/50% Chilling them for 4 seconds. Then he fires another shot, which deals 230/345/535 magic damage to the target and nearby enemies. Chill: reduce Attack Speed

**Fiora** (2-cost) - En Garde!: Fiora lunges at her enemy, dealing 250%/250%/260% AD physical damage (250/250/260% Attack Damage) to all enemies hit. For the next 8 seconds, she takes 20% less damage and her attacks restore 30/40/55 Health.

**Jinx** (2-cost) - Fishbones: Jinx fires a rocket at her target that explodes on impact, damaging all nearby enemies for 225/335/525 magic damage and Stunning them for 1.5 seconds. Stun: cannot move, attack, or cast Abilities

**Lee Sin** (2-cost) - Safeguard: Lee Sin dashes towards a nearby ally, shielding them and himself for 180/220/270 Health for 4 seconds. He then dashes back and deals 275%/275%/280% AD physical damage (275/275/280% Attack Damage) to his target. If there are no nearby allies, he gains a 360/440/540 Health shield instead.

**Malphite** (2-cost) - Ground Slam: Malphite slams the ground, dealing 150/225/350 magic damage to nearby enemies and granting himself 100/125/150 Armor for 4 seconds.

**Rell** (2-cost) - Iron Bond: Rell forms a tether between herself and her lowest percent Health ally, dealing 140/210/325 magic damage to all enemies between herself and her bound ally. She also grants herself and her bound ally 50/60/70% of Rell's current Armor and Magic Resist for 4 seconds.

**Sivir** (2-cost) - Pizza Delivery: Sivir throws her pizza at her target, dealing 400%/400%/410% AD physical damage (400/400/410% Attack Damage). It then splits into slices and gets delivered to the 3 nearest allies, healing them for 80/100/125 Health.

**Vi** (2-cost) - Blast Shield: Vi grants herself a 270/330/400 Health shield for 4 seconds. Then she deals 250% AD physical damage to nearby enemies (250% Attack Damage) and 50% Sunders them for 4 seconds. Sunder: reduce Armor

**Yasuo** (2-cost) - Steel Tempest: Yasuo fires a whirlwind at his target dealing 210/315/485 magic damage and knocking up units hit for 1.5 seconds. If the current position isn't ideal, he will dash up to 1 hex in order to find more units in a line.

**Yuumi** (2-cost) - Prowling Projectile: Yuumi fires a curved missile at the furthest target in her attack range, dealing 230/345/535 magic damage to the first unit hit.

**Alistar** (3-cost) - Pulverize: Alistar slams the ground underneath his target, dealing 140/210/350 magic damage and knocking them up for 1.5 seconds. He then roars, healing himself and the lowest health ally champion for 20% max Health (20% Maximum Health)

**Cho'Gath** (3-cost) - Cosmic Bellow: Passive: +75/100/150 Magic Resist Active: Cho'gath breathes a cosmic beam in a forward arc that deals (70/105/165 + 150%/190%/240% MR) magic damage (150/190/240% Magic Resist + 70/105/165) and 35/35/50% Mana Reaves the target. Mana Reave: increase maximum Mana until the next cast

**Jax** (3-cost) - Adaptive Strike: Passive: Jax leaps to his target and deals 80/125/210 bonus magic damage every 3rd attack. The bonus damage increases by 20/30/50 every 3rd attack, and stacks up to 7 times. Current Damage on hit: @TFTUnitProperty.:TFT8_Jax_TotalDamage@

**Kai'Sa** (3-cost) - Starcharged: Passive: Kai'Sa's attacks apply Plasma. Her next attack against a target with 2 stacks of Plasma deals 150/225/370 magic damage and removes the Plasma. Active: Kai'Sa gains 45% Attack Speed for 10 seconds. This bonus stacks!

**LeBlanc** (3-cost) - Sigil of Malice: LeBlanc fires 6/6/8 sigils at her target, each dealing 75/110/135 magic damage. If the target dies, she completes casting the remaining sigils at the nearest enemy but adds 1 more sigil.

**Nilah** (3-cost) - Apotheosis: Nilah whirls her whip-blade, dealing 90/135/225 magic damage to nearby enemies while healing herself and allies for 235/270/330 Health over 1 second. She finishes with a burst that deals 90/135/225 magic damage.

**Rammus** (3-cost) - Sand Slam: Passive: +60/90/150 Armor Active: Rammus powers himself up and leaps to the largest group of nearby enemies, dealing (30/50/80 + 150%/175%/205% Armor) magic damage (150/175/205% Armor + 30/50/80), knocking them up for 1.25 seconds, and reducing their Attack Damage by 40% for 4 seconds.

**Riven** (3-cost) - Bunny Blade: Riven gains a 325/400/500 Health shield for 4 seconds and empowers her next 3 attacks with 90/135/225 bonus magic damage. If Riven has more Health than her target, she deals 100% more bonus magic damage.

**Senna** (3-cost) - Piercing Darkness: Senna fires a laser beam in the direction of her target, dealing (45/65/125 + 170%/180%/190% AD) physical damage (170/180/190% Attack Damage + 45/65/125) to all enemies in a line.

**Sona** (3-cost) - Crescendo Sequence: Sona fires 2 beams that cycle between 3 phases: 1: Deal 175/265/430 magic damage to the nearest enemies 2: Heal the lowest Health allies for 200/250/325 Health 3: Deal 120/180/300 magic damage to the nearest enemies and Stun them for 1.5 seconds Stun: cannot move, attack, or cast Abilities

**Vayne** (3-cost) - Silver Bolt: Vayne fires a heavy bolt at her target, dealing (18/25/45 + 130%/130%/140% AD) true damage (130/130/140% Attack Damage + 18/25/45).

**Vel'Koz** (3-cost) - Frozen Tomb: Vel'koz Stuns the enemy champion that has dealt the most damage this round for 2/2.25/2.5 seconds and deals 275/415/680 magic damage to them. An enemy cannot be frozen again for 4 seconds after being frozen. Stun: cannot move, attack, or cast Abilities

**Zoe** (3-cost) - Trouble Bubble: Zoe launches a bubble at the nearest non-infected enemy, infecting them with a virus that deals 390/585/975 magic damage over 10 seconds. Infected enemies take 10/10/20% more damage.

**Aurelion Sol** (4-cost) - Meteor Shower: Aurelion Sol calls down 4/4/6 meteors on random enemies. Meteors deal 155/230/450 magic damage, then 10% Burn and 33% Wound enemies for 10 seconds. Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received

**Bel'Veth** (4-cost) - Endless Banquet: Bel'Veth dashes around her target and unleashes a flurry of 250% Attack Speed attacks (250% of her Attacks per second), each dealing (20/30/150 + 75%/75%/200% AD) physical damage (75/75/200% Attack Damage + 20/30/150). Each time this is cast, Bel'Veth gains 25% bonus Attack Speed for the rest of combat.

**Ekko** (4-cost) - Star Diver: Ekko dives in, granting himself a 550/700/3000 shield for 5 seconds and taunts nearby enemies. When the shield breaks, he emits a time field, dealing 100/150/1500 to nearby enemies and slowing their attack speed by 50% for 2 seconds.

**Miss Fortune** (4-cost) - Bullet Time: Miss Fortune fires 8 waves of bullets in her target's direction within channels for 2 seconds. Enemies take 40/60/180 magic damage from the first bullet that hits them, and 20/30/100 magic damage from any other bullets that hit.

**Samira** (4-cost) - Flair: Samira rapidly fires bullets that deal a total of (80/100/666 + 480%/490%/1250% AD) physical damage (480/490/1250% Attack Damage + 80/100/666). Bullets ricochet to nearby enemies for 20% less damage.

**Sejuani** (4-cost) - Cryo Grenade: Sejuani throws a cryo grenade at the closest enemy that explodes after a 2-second delay. Enemies within a 3 hexes are Stunned for 2/2.5/8 seconds, and take 125/200/1200 magic damage. Stun: cannot move, attack, or cast Abilities

**Sett** (4-cost) - Mecha SLAM!: Sett gains a 325/425/1500 Health shield and becomes immune to crowd control for 2 seconds. He charges up while the shield is active; when the shield breaks or expires, he deals 140%/150%/300% AD physical damage in a cone to all enemies caught within the blast. The blast damage increases, up to 280%/300%/600% AD, depending on how long Sett was charged up.

**Soraka** (4-cost) - Starcall: Soraka launches a star at her target, which deals 235/350/750 magic damage and heals Soraka for 8% of her maximum Health. Every 3 casts, she launches 3/3/5 stars at each of the nearest enemies.

**Taliyah** (4-cost) - Weaver's Wall: Taliyah summons a torrent of spiraling rocks in a line of enemy units, dealing 75/115/275 magic damage to all enemies it hits. After a brief delay, the wall explodes, dealing 225/335/999 additional magic damage to nearby enemies.

**Viego** (4-cost) - Heartbreaker: Viego becomes unstoppable, leaping to the largest cluster of nearby enemies and slamming his sword down on the enemy with the lowest Health, dealing 260/390/850 magic damage to the primary target and 125/185/400 magic damage to enemies nearby. If Viego kills an enemy, he casts again but deals 20/20/10% less damage.

**Zac** (4-cost) - Symbiotic Split: Passive: Upon dying, split into 2/2/4 smaller Zac Blobs with 40/40/50% of Zac's maximum Health that taunt nearby enemies. Active: Zac bounces into the air and slams down, dealing 100/150/1500 + 25% of his current Health as magic damage to nearby enemies. Then he heals 30% of his missing Health.

**Zed** (4-cost) - Empowered Kill Mode: Zed briefly becomes untargetable and teleports behind his target, reducing their Armor by 20/30/100 and entering KILL MODE for 7 seconds. In KILL MODE, Attacks deal 30/45/120 bonus physical damage and every 3rd attack deals 100% AD physical damage (135% Attack Damage + 30/45/120) to all adjacent enemies.

**Aphelios** (5-cost) - Weapons of the Faithful: Place Aphelios on the board to choose his Ability.

**Fiddlesticks** (5-cost) - Dark Harvest: Fiddlesticks teleports into the largest cluster of enemies and fears them for 1/1/5 seconds. For the next 5/5/30 seconds, Fiddlesticks drains the life from nearby enemies dealing 100/150/900 magic damage per second and healing for 20% of the damage dealt.

**Janna** (5-cost) - Twister!: Janna sends a twister towards the largest group of enemies. The twister expands when it reaches its destination, knocking all nearby enemies up in the air, Stunning them for 1.25/1.5/8 seconds, and dealing 50/75/1000 magic damage each second for 3 seconds. Stun: cannot move, attack, or cast Abilities

**Leona** (5-cost) - Deus Ex Machina: Leona locks onto her target and calls down an orbital laser strike. The laser deals 350/525/5000 true damage per second to the target until the target dies, and 320/480/2000 magic damage to nearby enemies.

**Mordekaiser** (5-cost) - Skylines: Mordekaiser transports the arena into his realm for 4/4/6 seconds. He 50/50/70% Shreds ALL enemies, then slams a building onto the arena that deals 420/630/5555 magic damage to enemies hit. Shred: reduce Magic Resist

**Nunu & Willump** (5-cost) - Biggest Roboball Ever!: Passive: Nunu and Willump roll a ball around the arena towards the furthest enemy. The ball deals 125/190/2000 magic damage to all enemies it passes through. When they reach their target, they will find a new destination. Nunu and Willump gain 30/30/90 Mana per second. Active: Nunu and Willump grow the ball, increasing their movement speed, the size of the ball, and the amount of damage by 40%.

**Syndra** (5-cost) - Force of Will: Syndra grants a 200/600/4000 Health shield to a random champion on your bench, then flings them into the arena inside an orb at her closest enemy. The orb deals 250/375/2500 magic damage to enemies hit. Traits of champions flung onto the board do not become active, except for Corrupted.

**Urgot** (5-cost) - Undertow: Passive: Urgot's claw fires 5/5/20 attacks per second, each dealing (40% AD + 2000% Attack Speed) (40% AD+2000% Attack Speed) physical damage. Active: Urgot summons a gigantic wave behind his enemies that deals 5/8/75% of their maximum Health as magic damage. The wave knocks enemies forward 1 hex, Stuns them for 1/1.5/5 seconds, and has a 15/25/100 chance to dredge up treasure (gold and loot). Stun: cannot move, attack, or cast Abilities

## Traits

### A.D.M.I.N.

A.D.M.I.N. programs a custom configuration per player each game.

| Units | Effect |
|---|---|
| 2-3 | [Initialize ADMIN cause and effect] |
| 4-5 | [Add another effect to the program]. Boost all tiers by 30% |
| 6 | Boost previous tiers by 100% |

### Ace

This Trait is active only when you have exactly 1 or 4 unique Aces.

| Units | Effect |
|---|---|
| 1 | Execute enemies under 15% Health |
| 4 | Execute enemies under 30% Health |

### Aegis

Your team gains bonus Magic Resist, and Aegis units gain more.

| Units | Effect |
|---|---|
| 2 | +20 Magic Resist, +40 for Aegis |
| 3 | +40 Magic Resist, +80 for Aegis |
| 4 | +60 Magic Resist, +120 for Aegis |
| 5 | +90 Magic Resist, +180 for Aegis |

### Anima Squad

Anima Squad members pose after champion kills, increasing their fame. Each point of fame grants 5 permanent Health to each Anima Squad member. They also gain: Total fame: @TFTUnitProperty.trait:TFT8_AnimaFameStacks@ Bonus Health: @TFTUnitProperty.trait:TFT8_AnimaFameHP@

| Units | Effect |
|---|---|
| 3-4 | 10% Attack Damage and 10 Ability Power |
| 5-6 | 35% Attack Damage and 35 Ability Power |
| 7 | 60% Attack Damage and 60 Ability Power |

### Arsenal

(1) When deploying the Arsenal champion, players may choose which weapon he will equip.

| Units | Effect |
|---|---|
| 1 | active (no extra values) |

### Brawler

Brawlers gain additional maximum Health.

| Units | Effect |
|---|---|
| 2-3 | +20% maximum Health |
| 4-5 | +45% maximum Health |
| 6-7 | +75% maximum Health |
| 8 | +110% maximum Health |

### Civilian

If there is a Civilian alive, your team is inspired to protect them by gaining Mana every 2 seconds.

| Units | Effect |
|---|---|
| 1 | +2 Mana |
| 2 | +4 Mana |
| 3 | +10 Mana |

### Corrupted

(1) Combat start: Lie dormant while absorbing the souls of allies that die. Gain 40 Ability Power for each soul. Once per combat at 70% Health (or when your team has died), come alive and fight.

| Units | Effect |
|---|---|
| 1 | variables: HealthThreshold=70; {d49caf5d}=40 |

### Defender

Innate: Taunt nearby enemies after the start of combat. Your team gains bonus Armor, and Defenders gain more. Taunt: enemies that are able and in range must attack the taunter

| Units | Effect |
|---|---|
| 2-3 | +30 Armor, +60 for Defenders |
| 4-5 | +80 Armor, +160 for Defenders |
| 6 | +200 Armor, +400 for Defenders |

### Duelist

Innate: Increased movement speed. Duelists' attacks grant bonus Attack Speed, up to 12 stacks.

| Units | Effect |
|---|---|
| 2-3 | +5% Attack Speed per stack |
| 4-5 | +9% Attack Speed per stack |
| 6-7 | +15% Attack Speed per stack |
| 8 | +24% Attack Speed per stack |

### Forecaster

Depending on the weather, The Forecaster grants a different bonus to adjacent allies. The weather changes every game.

| Units | Effect |
|---|---|
| 1 | variables: {18727ad0}=8; {1a727df6}=99; {1b727f89}=15; {59983c1c}=2000; {5a983daf}=600; {5b983f42}=400; {c4d287f1}=5; {d3255094}=20; {d52553ba}=100; {d625554d}=40; {e412bacb}=10; {e512bc5e}=10; {e612bdf1}=45 |

### Gadgeteen

Each round, Gadgeteens create random modified weapons with powerful effects that fall apart after one round. Gadgeteens also gain Damage and Damage Reduction for each item equipped to them.

| Units | Effect |
|---|---|
| 3-4 | Create 1 item, 3% per item. |
| 5 | Create 2 items, 10% per item. |

### Hacker

Hackers gain Omnivamp and summon a H4ckerr!m. Any unit placed in the rider hex will be sent to the enemy backline and is untargetable for the first [unknown: SecondsUntargetable] seconds of combat. Omnivamp: heal for some of damage dealt

| Units | Effect |
|---|---|
| 2 | 15% Omnivamp |
| 3 | 40% Omnivamp |
| 4 | 70% Omnivamp |

### Heart

When Heart units cast their Ability, your team gains stacking Ability Power for the rest of combat.

| Units | Effect |
|---|---|
| 2-3 | +4 Ability Power |
| 4-5 | +7 Ability Power |
| 6 | +10 Ability Power |

### LaserCorps

When a LaserCorp agent attacks or is hit by an attack, their combat drone deals magic damage to the agent's target (0.4 second cooldown). When a LaserCorp agent dies, their drone is reassigned to the nearest living agent. Drone lasers deal: Drone damage last round: @TFTUnitProperty.trait:TFT8_Lasercorps_DamageTRAKey@ magic damage

| Units | Effect |
|---|---|
| 3-4 | 25 magic damage |
| 5-6 | 45 magic damage |
| 7-8 | 80 magic damage |
| 9 | 99 magic damage, and each agent gets a 2nd drone |

### Mascot

Your team heals a percentage of their maximum Health every 2 seconds, and Mascots heal double the amount. When Mascots die, they retreat to the sidelines to cheer on your team. Your team's healing increases by 1% for each cheering Mascot.

| Units | Effect |
|---|---|
| 2-3 | 1.5% healing |
| 4-5 | 3% healing |
| 6-7 | 6% healing |
| 8 | 10% healing |

### Mecha:PRIME

Use the Mecha selector item to choose a PRIME. Combat start: The PRIME combines with the 2 nearest Mecha, absorbing 100% of their Base Health.

| Units | Effect |
|---|---|
| 3-4 | The PRIME gains 60 Ability Power and 60% Attack Damage |
| 5 | All Mechas gain 55 Ability Power and 55% Attack Damage; the PRIME gains double this amount |

### Ox Force

Ox Force units gain a range of Attack Speed that ramps as they lose Health. Once per combat, when they would drop below one health, they instead go to one health and become immune to damage for 1 second.

| Units | Effect |
|---|---|
| 2-3 | 15% - 30% Attack Speed |
| 4-5 | 40% - 80% Attack Speed |
| 6-7 | 75% - 150% Attack Speed |
| 8 | 150% - 300% Attack Speed |

### Prankster

| Units | Effect |
|---|---|
| 2 | Once per combat at 50% Health, Pranksters spawn a target dummy, move to a safe location, and restore 150 Health. |
| 3 | And, Prankster dummies stun the enemy that killed them for 1.5 seconds. |

### Recon

Innate: Gain [unknown: BonusRange] hex Attack Range. If there is an enemy nearby, Recon units will dash to safety before casting their Ability.

| Units | Effect |
|---|---|
| 2 | +20% Crit Chance |
| 3 | +75% Crit Chance |
| 4-999 | +100% Crit Chance and Recon Abilities can critically strike |

### Renegade

Renegade units deal bonus damage, and the last one standing deals more.

| Units | Effect |
|---|---|
| 3-5 | 35% bonus damage, extra 20% for last alive |
| 6 | 60% bonus damage, extra 40% for last alive |

### Spellslinger

Every 5 seconds, Spellslinger's next attack instead fires a magic orb at a random target, which explodes for 50% Ability Power as magic damage. Spellslingers gain:

| Units | Effect |
|---|---|
| 2-3 | +25 Ability Power |
| 4-5 | +60 Ability Power |
| 6-7 | +95 Ability Power |
| 8 | +140 Ability Power, and orbs deal 150% Ability Power |

### Star Guardian

Star Guardians gain more Mana in combat.

| Units | Effect |
|---|---|
| 3-4 | 40% Mana |
| 5-6 | 70% Mana |
| 7-8 | 120% Mana |
| 9 | 200% Mana |

### Supers

Bonus damage: @TFTUnitProperty.trait:TFT8_Supers_TotalBonusDamage@%

| Units | Effect |
|---|---|
| 3 | Combat start: Strike a pose that grants your team 18% bonus damage, which increases by 3% for every 3-star champion on your team. |

### Sureshot

Combat start: Sureshots gain bonus Attack Damage now, and every 4 seconds.

| Units | Effect |
|---|---|
| 2-3 | +6% Attack Damage |
| 4 | +16% Attack Damage |

### Threat

Threats do not have a Trait bonus, but instead have powerful Abilities and increased base stats.

| Units | Effect |
|---|---|
| 1 | active (no extra values) |

### Underground

The Underground must sneak through the sewers after each player combat and crack 10 locks to open a vault. When the vault opens, you may choose to take the loot now, or attempt another heist for even better rewards. Current Heist Level: @TFTUnitProperty.trait:TFT8_Underground_CurrentStage@

| Units | Effect |
|---|---|
| 3 | 2 locks per win, 3 per loss |
| 4 | 2 locks per win, 4 per loss |
| 5 | 3 locks per win, 5 per loss |
| 6 | 4 locks per win, 7 per loss |

## Items

Item and augment membership comes from the "TFTSet8" item/augment lists in the patch 13.11 CDragon file. Values come from the patch 13.5 file when the item exists there, else from 13.11. The list is the full pool the game data allows for the set mode, so it includes items obtainable only through augments, anvils or special rounds.

### Components

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| B.F. Sword | - | 0.1 AD | +10% Attack Damage |
| Chain Vest | - | 20 Armor | +20 Armor |
| Giant's Belt | - | 150 HP | +150 Health |
| Needlessly Large Rod | - | 10 AP | +10 Ability Power |
| Negatron Cloak | - | 20 MR | +20 Magic Resist |
| Recurve Bow | - | 10 Attack Speed % | +10% Attack Speed |
| Sparring Gloves | - | 20 Crit % | +20 Critical Strike Chance |
| Spatula | - | - | It must do something... |
| Tear of the Goddess | - | 15 Mana | +15 Mana |

### Completed items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Archangel's Staff | TFT_Item_NeedlesslyLargeRod + TFT_Item_TearOfTheGoddess | 20 AP, 30 Mana | Combat start: Grant 20 Ability Power every 5 seconds. |
| Bloodthirster | TFT_Item_BFSword + TFT_Item_NegatronCloak | 0.2 AD, 20 MR | Grant 20% Omnivamp. Once per combat at 40% Health, gain a 25% maximum Health shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Buff | TFT_Item_TearOfTheGoddess + TFT_Item_TearOfTheGoddess | 10 AP, 40 Mana | Abilities cost 10 less Mana to cast. If the holder gets at least 1 takedown within 3 seconds of casting, gain 10 Mana. [Unique - only 1 per champion] |
| Bramble Vest | TFT_Item_ChainVest + TFT_Item_ChainVest | 70 Armor | Negates 75% bonus damage from incoming critical strikes. When struck by an attack, deal 75/100/150 magic damage to all nearby enemies (once every 2.5 seconds). [Direct damage item] |
| Chalice of Power | TFT_Item_NegatronCloak + TFT_Item_TearOfTheGoddess | 20 MR, 30 Mana | Combat start: Grant 25 Ability Power to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Deathblade | TFT_Item_BFSword + TFT_Item_BFSword | 0.66 AD | Perfect peace and calm for the holder - and all who face it. |
| Dragon's Claw | TFT_Item_NegatronCloak + TFT_Item_NegatronCloak | 70 MR | Every 2 seconds, regenerate 4% maximum Health. |
| Edge of Night | TFT_Item_BFSword + TFT_Item_ChainVest | 0.1 AD, 20 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, gain 15% bonus Attack Speed. [Unique - only 1 per champion] |
| Gargoyle Stoneplate | TFT_Item_ChainVest + TFT_Item_NegatronCloak | 30 Armor, 30 MR | Grants 15 Armor and 15 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | TFT_Item_BFSword + TFT_Item_RecurveBow | 0.3 AD, 20 AP, 10 Attack Speed % | Abilities and attacks deal 25% more damage to enemies with more than 1600 maximum Health. |
| Guardbreaker | TFT_Item_GiantsBelt + TFT_Item_SparringGloves | 0.2 AD, 20 AP, 150 HP, 20 Crit % | After damaging a shielded enemy, Abilities and attacks deal 30% more damage for 3 seconds. |
| Guinsoo's Rageblade | TFT_Item_RecurveBow + TFT_Item_NeedlesslyLargeRod | 10 AP, 10 Attack Speed % | Attacks grant 5% bonus Attack Speed. This effect stacks. |
| Hand Of Justice | TFT_Item_TearOfTheGoddess + TFT_Item_SparringGloves | 15 Mana, 20 Crit % | Grant 2 effects: +15% Attack Damage and +15 Ability Power.15% Omnivamp. Each round, randomly double 1 of these effects. Omnivamp: heal for some of damage dealt |
| Hextech Gunblade | TFT_Item_BFSword + TFT_Item_NeedlesslyLargeRod | 0.1 AD, 10 AP | Grant 22% Omnivamp, which also heals the lowest percent Health ally. Omnivamp: heal for some of damage dealt |
| Infinity Edge | TFT_Item_BFSword + TFT_Item_SparringGloves | 0.3 AD, 35 Crit % | Damage from an Ability can critically strike. |
| Ionic Spark | TFT_Item_NeedlesslyLargeRod + TFT_Item_NegatronCloak | 10 AP, 20 MR | 50% Shred enemies within 2 hexes. When enemies cast an Ability, they are also zapped for magic damage equal to 185% of their maximum Mana. [Direct damage item] Shred: reduce Magic Resist |
| Jeweled Gauntlet | TFT_Item_NeedlesslyLargeRod + TFT_Item_SparringGloves | 25 AP, 35 Crit % | Damage from an Ability can critically strike. |
| Last Whisper | TFT_Item_RecurveBow + TFT_Item_SparringGloves | 0.1 AD, 10 Attack Speed %, 20 Crit % | Any physical damage dealt 50% Sunders the target for 5 seconds. This effect does not stack. [Unique - only 1 per champion] Sunder: reduce Armor |
| Locket of the Iron Solari | TFT_Item_NeedlesslyLargeRod + TFT_Item_ChainVest | 10 AP, 20 Armor | Combat start: Shields the holder and allies within 2 hexes in the same row for 300/350/400 damage for 15 seconds. ​​[Aura item] |
| Morellonomicon | TFT_Item_NeedlesslyLargeRod + TFT_Item_GiantsBelt | 25 AP, 150 HP | Magic or true damage from the holder's Ability 10% Burns and 33% Wounds enemies for 10 seconds. [Unique - only 1 per champion] Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received |
| Protector's Vow | TFT_Item_TearOfTheGoddess + TFT_Item_ChainVest | 20 Armor, 45 Mana | Once per combat: At 40% Health, gain a 25% maximum Health shield that lasts up to 5 seconds and 35 Armor and 35 Magic Resist for the rest of combat. |
| Quicksilver | TFT_Item_SparringGloves + TFT_Item_NegatronCloak | 30 Attack Speed %, 30 MR, 20 Crit % | Combat start: Grant immunity to crowd control for 18 seconds. [Unique - only 1 per champion] |
| Rabadon's Deathcap | TFT_Item_NeedlesslyLargeRod + TFT_Item_NeedlesslyLargeRod | 70 AP | This humble hat can help you make, or unmake, the world itself. |
| Rapid Firecannon | TFT_Item_RecurveBow + TFT_Item_RecurveBow | 55 Attack Speed % | Grant 1 bonus Attack Range and your attacks cannot miss. |
| Redemption | TFT_Item_TearOfTheGoddess + TFT_Item_GiantsBelt | 150 HP, 15 Mana | Heal allies within 1 hex for 15% of their missing Health every 5 seconds. They also take 25% less multi-target damage for 5 seconds (damage reduction does not stack). [Aura item] |
| Runaan's Hurricane | TFT_Item_NegatronCloak + TFT_Item_RecurveBow | 0.2 AD, 10 Attack Speed %, 20 MR | Attacks fire a bolt at a nearby enemy, dealing 50% Attack Damage as physical damage. ​​[Direct damage item] |
| Shroud of Stillness | TFT_Item_ChainVest + TFT_Item_SparringGloves | 20 Armor, 250 HP, 20 Crit % | Combat start: Shoot a beam that 35% Mana Reaves enemies. [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast |
| Spear of Shojin | TFT_Item_BFSword + TFT_Item_TearOfTheGoddess | 0.1 AD, 15 AP, 15 Mana | Every 3rd attack restores 20 additional Mana. |
| Statikk Shiv | TFT_Item_RecurveBow + TFT_Item_TearOfTheGoddess | 15 AP, 10 Attack Speed %, 15 Mana | Every 3rd attack unleashes chain lightning that bounces to 4 enemies, dealing 30 magic damage and 50% Shredding them for 5 seconds. [Direct damage item] Shred: reduce Magic Resist |
| Sunfire Cape | TFT_Item_ChainVest + TFT_Item_GiantsBelt | 20 Armor, 250 HP | Every 2 seconds, an enemy within 2 hexes is 10% Burned and 33% Wounded for 10 seconds. [Unique - only 1 per champion] Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received |
| Thief's Gloves | TFT_Item_SparringGloves + TFT_Item_SparringGloves | 40 Crit % | Each round: Equip 2 random items. [Consumes 3 item slots.] |
| Titan's Resolve | TFT_Item_ChainVest + TFT_Item_RecurveBow | 10 Attack Speed %, 20 Armor | Grants 2% Attack Damage and 2 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, grant 25 Armor and 25 Magic Resist. |
| Warmog's Armor | TFT_Item_GiantsBelt + TFT_Item_GiantsBelt | 800 HP | Tough as teak, strong as oak, and eternal as petricite. |
| Zeke's Herald | TFT_Item_BFSword + TFT_Item_GiantsBelt | 0.1 AD, 250 HP | Combat start: Grant 20% Attack Speed to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Zephyr | TFT_Item_NegatronCloak + TFT_Item_GiantsBelt | 15 Attack Speed %, 20 MR, 150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| Zz'Rot Portal | TFT_Item_RecurveBow + TFT_Item_GiantsBelt | 10 Attack Speed %, 150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. Taunt: enemies that are able and in range must attack the taunter |

### Spatula items and emblems

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| A.D.M.I.N. Emblem | TFT_Item_NegatronCloak + TFT_Item_Spatula | 20 MR | The holder gains the A.D.M.I.N. Trait. [Unique - only 1 per champion] |
| Ace Emblem | - | 150 HP | The holder gains the Ace Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Aegis Emblem | - | 150 HP | The holder gains the Aegis Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Anima Squad Emblem | TFT_Item_Spatula + TFT_Item_NeedlesslyLargeRod | 10 AP | The holder gains the Anima Squad Trait. [Unique - only 1 per champion] |
| Brawler Emblem | - | 150 HP | The holder gains the Brawler Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Civilian Emblem | - | 150 HP | The holder gains the Civilian Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Defender Emblem | - | 150 HP | The holder gains the Defender Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Duelist Emblem | TFT_Item_Spatula + TFT_Item_RecurveBow | 10 Attack Speed % | The holder gains the Duelist Trait. [Unique - only 1 per champion] |
| Gadgeteen Emblem | - | 150 HP | The holder gains the Gadgeteen Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Hacker Emblem | - | 150 HP | The holder gains the Hacker Trait. [Unique - only 1 per champion] |
| Heart Emblem | TFT_Item_Spatula + TFT_Item_TearOfTheGoddess | 15 Mana | The holder gains the Heart Trait. [Unique - only 1 per champion] |
| LaserCorps Emblem | TFT_Item_Spatula + TFT_Item_BFSword | 0.1 AD | The holder gains the LaserCorps Trait. [Unique - only 1 per champion] |
| Mascot Emblem | TFT_Item_Spatula + TFT_Item_GiantsBelt | 150 HP | The holder gains the Mascot Trait. [Unique - only 1 per champion] |
| Mecha: PRIME Emblem | - | 150 HP | The holder gains the Mecha: PRIME Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Ox Force Emblem | TFT_Item_Spatula + TFT_Item_ChainVest | 20 Armor | The holder gains the Ox Force Trait. [Unique - only 1 per champion] |
| Prankster Emblem | - | 150 HP | The holder gains the Prankster Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Recon Emblem | - | 150 HP | The holder gains the Recon Trait. [Unique - only 1 per champion] |
| Renegade Emblem | TFT_Item_SparringGloves + TFT_Item_Spatula | 20 Crit % | The holder gains the Renegade Trait. [Unique - only 1 per champion] |
| Spellslinger Emblem | - | 150 HP | The holder gains the Spellslinger Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Star Guardian Emblem | - | 150 HP | The holder gains the Star Guardian Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Sureshot Emblem | - | 150 HP | The holder gains the Sureshot Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |
| Tactician's Crown | TFT_Item_Spatula + TFT_Item_Spatula | - | Your team gains +1 maximum team size. |
| The Underground Emblem | - | 150 HP | The holder gains the Underground Trait. [Unique - only 1 per champion Elusive - cannot be crafted.] |

### Ornn artifacts

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Visage | - | 25 MR, 250 HP | Regenerate 2.5% maximum Health each second. |
| Death's Defiance | - | 0.1 AD, 20 Attack Speed %, 20 Armor | Grant 25% Omnivamp. 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] Omnivamp: heal for some of damage dealt |
| Eternal Winter | - | 25 Armor, 150 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead, but only once every 15 seconds. [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Gold Collector | - | 0.2 AD, 30 Crit % | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 40% chance to drop 1 gold. [Unique - only 1 per champion] |
| Infinity Force | - | 0.25 AD, 25 AP, 25 Attack Speed %, 25 Armor, 25 MR, 25 HP, 25 Mana | Tons of EVERYTHING! |
| Manazane | - | 0.15 AD, 15 Attack Speed %, 30 Mana | Once per combat: After casting an Ability, restore 120 Mana over 5 seconds. [Unique - only 1 per champion] |
| Obsidian Cleaver | - | 0.2 AD, 150 HP | Attacks and Abilities 50% Shred and 50% Sunder enemies for 5 seconds. [Unique - only 1 per champion] Shred: reduce Magic Resist Sunder: reduce Armor |
| Ornn Item Anvil | - | - | Ornn Item Anvil |
| Randuin's Sanctum | - | null Armor, null MR, 150 HP | Combat start: Grant 40 Armor and 40 Magic Resistance to the holder and their adjacent allies. [Aura item] |
| Rocket-Propelled Fist | - | 25 Armor, 150 HP, 15 Mana | Combat start: The farthest enemy is pulled into melee range and Stunned for 1.5 seconds. Allies within range will prioritize attacking that enemy. [Unique - only 1 per champion] Stun: cannot move, attack, or cast Abilities |
| Zhonya's Paradox | - | 40 AP, 40 Armor, 40 MR | Once per combat: At 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Absolution | - | 300 HP, 15 Mana | Heal allies within 2 hexes for 25% of their missing Health every 5 seconds. They take 25% less multi-target damage for 5 seconds (damage reduction does not stack). ​​[Aura item] |
| Blessed Bloodthirster | - | 0.4 AD, 20 MR | Grant 40% Omnivamp. Once per combat: At 40% Health, gain a 40% maximum Health shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Blessing | - | 50 AP, 40 Mana | Abilities cost 10 less Mana to cast. If the holder gets at least 1 takedown within 3 seconds of casting, gain 20 Mana. [Unique - only 1 per champion] |
| Brink of Dawn | - | 0.1 AD, 20 Armor | Once per combat: At 60% Health and again at 30% Health, briefly become untargetable and shed negative effects. Then, gain 40% bonus Attack Speed. This effect stacks. [Unique - only 1 per champion] |
| Bulwark's Oath | - | 20 Armor, 45 Mana | Once per combat: At 40% Health, gain 70 Armor and 70 Magic Resist for the rest of combat, and a 50% maximum Health shield that lasts up to 10 seconds. |
| Chalice of Charity | - | 20 MR, 15 Mana | Combat start: Grant 45 Ability Power and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​[Aura item] Omnivamp: heal for some of damage dealt |
| Covalent Spark | - | 10 AP, 20 MR | 50% Shred enemies within 3 hexes. When enemies cast an Ability, they are also zapped for magic damage equal to 250% of their maximum Mana. Also, regenerate 1% maximum Health per second. [Direct damage item] Shred: reduce Magic Resist |
| Demonslayer | - | 0.5 AD, 40 AP, 10 Attack Speed % | Abilities and attacks deal 70% more damage to enemies with more than 1600 maximum Health. |
| Dragon's Will | - | 140 MR | Every 1.5 seconds, regenerate 8% maximum Health. |
| Dvarapala Stoneplate | - | 20 Armor, 20 MR | Grant 30 Armor and 30 Magic Resist for each enemy targeting the holder. Also, regenerate 2% maximum Health each second. |
| Eternal Whisper | - | 0.35 AD, 10 Attack Speed %, 55 Crit % | Any physical damage dealt 50% Sunders the target for the rest of combat. This effect does not stack. [Unique - only one per champion] Sunder: reduce Armor |
| Fist of Fairness | - | 15 Mana, 20 Crit % | Grant 2 effects: 50% Attack Damage and 50 Ability Power.30% Omnivamp. Omnivamp: heal for some of damage dealt |
| Glamorous Gauntlet | - | 50 AP, 75 Crit % | Damage from an Ability can critically strike. |
| Guinsoo's Reckoning | - | 10 AP, 25 Attack Speed % | Attacks grant 10% bonus Attack Speed. This effect stacks. |
| Hextech Lifeblade | - | 0.1 AD, 40 AP | Grant 35% Omnivamp, which also heals the lowest percent Health ally. Omnivamp: heal for some of damage dealt |
| Locket of Targon Prime | - | 10 AP, 20 Armor | Combat start: Shields the holder and allies within 3 hexes in the same row for 400/500/600 damage for 60 seconds. Your team gains 200 Health. ​​[Aura item] |
| Luminous Deathblade | - | 1.2 AD | It glows in the presence of enemies. Or friends. Or anything alive, really. |
| Mistral | - | 20 Attack Speed %, 20 MR, 150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 8 seconds. Your team gains 15% Attack Speed. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| More More-ellonomicon | - | 45 AP, 150 HP | Magic or true damage from the holder's Ability 60% Burns and 33% Wounds enemies for 30 seconds. [Unique - only 1 per champion] Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received |
| Quickestsilver | - | 70 Attack Speed %, 20 MR, 20 Crit % | Combat start: Grant immunity to crowd control for 30 seconds. [Unique - only 1 per champion] |
| Rabadon's Ascended Deathcap | - | 125 AP | It's witnessed - and unleashed - miracles and calamities both. |
| Rapid Lightcannon | - | 99 Attack Speed % | Grant 2 bonus Attack Range and your attacks cannot miss. |
| Rascal's Gloves | - | 40 Crit % | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] |
| Rosethorn Vest | - | 120 Armor | Negates all bonus damage from incoming critical strikes. On being hit by an attack, deal 125/175/225 magic damage to all nearby enemies (once every 1.5 seconds). [Direct damage item] |
| Runaan's Tempest | - | 0.2 AD, 20 Attack Speed %, 20 MR | Attacks fire a bolt at a nearby enemy, dealing 100% Attack Damage as physical damage. ​​[Direct damage item] |
| Shroud of Reverence | - | 20 Armor, 250 HP, 20 Crit % | Combat start: Shoot a wider beam that 50% Mana Reaves enemies. Your team gains 15 starting Mana. [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast |
| Spear of Hirana | - | 0.1 AD, 50 AP, 15 Mana | Every 3rd attack restores 40 additional Mana. |
| Statikk's Favor | - | 40 AP, 10 Attack Speed %, 15 Mana | Every 3rd attack unleashes chain lightning that bounces to 8 enemies, dealing 75 magic damage and 50% Shredding them for 5 seconds. [Direct damage item] Shred: reduce Magic Resist |
| Sunlight Cape | - | 20 Armor, 350 HP | Every 1.5 seconds, an enemy within 3 hexes is 60% Burned and 33% Wounded for 30 seconds. [Unique - only 1 per champion] Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received |
| Titan's Vow | - | 30 Attack Speed %, 30 Armor | Grant 3% Attack Damage and 3 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 50 Armor and 50 Magic Resist. |
| Urf-Angel's Staff | - | 50 AP, 30 Mana | Combat start: Gain 30 Ability Power every 4 seconds. |
| Warmog's Pride | - | 1300 HP | Regenerate 1% maximum Health per second. |
| Willbreaker | - | 0.4 AD, 40 AP, 150 HP, 20 Crit % | After damaging a shielded enemy, Abilities and attacks deal 70% more damage for 3 seconds. |
| Zeke's Harmony | - | 0.1 AD, 150 HP | Combat start: Grant 50% Attack Speed and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​[Aura item] Omnivamp: heal for some of damage dealt |
| Zenith Edge | - | 0.55 AD, 75 Crit % | Damage from an Ability can critically strike. |
| Zz'Rots Invitation | - | 10 Attack Speed %, 150 HP | Combat start: A Radiant Voidmother with bonus Health and Attack Damage arises and Taunts nearby enemies. Taunt: enemies that are able and in range must attack the taunter |

### Shimmerscale items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Determined Investor | - | 150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and 15 gold. |
| Diamond Hands | - | 20 AP, 200 HP | Once per combat: At 66% Health and 33% Health, become invulnerable for 1 seconds and grant 1 gold. |
| Draven's Axe | - | 5 AD, 5 Attack Speed % | Gain 1 Attack Damage per gold in your bank (up to 80 gold). Attacks grant 1 stack, up to 100 times. At full stacks, grant 5 gold and 1 item component(s). |
| Gambler's Blade | - | 10 AP, 10 Attack Speed % | Grant 1% bonus Attack Speed per gold in your bank (up to 80 gold). Each attack has a 7% chance to drop 1 gold. |
| Goldmancer's Staff | - | 20 AP, 20 Mana | Grant 1 Ability Power per gold in your bank (up to 80 gold) and a 40% chance to drop 2 gold on enemy kill. |
| Mogul's Mail | - | 350 HP | Grants 1 Armor, 1 Magic Resist, and 8 Health when taking damage, stacking up to 40 times. At full stacks, grant 2 gold. |
| Needlessly Big Gem | - | 500 HP | Once per combat: If the holder is alive after 15 seconds of combat, grant 1 gold per 2 living allies, and each living ally deals 1% more damage per gold in your bank (up to 80 gold). |

### Consumables and other

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| 1 completed item | - | - | - |
| 1 component | - | - | - |
| 1 loot orb | - | - | - |
| Binding Eclipse | - | - | Aphelios casts forth a lunar spotlight towards the largest group of units, stunning all enemies hit and dealing damage to them. |
| Chainsword Bloodthirster | - | 0.3 AD, 20 MR | Grant 25% Omnivamp. Once per combat at 40% Health, gain a 25% maximum Health shield that lasts up to 8 seconds. While the shield is active, the holder gains 50% Attack Damage. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. Omnivamp: heal for some of damage dealt |
| Champion Duplicator | - | - | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Completed Item Key | - | - | Gain a Completed Item Key |
| Component Item Key | - | - | Component Item Key |
| Duskwave | - | - | Aphelios unleashes a wave of energy in a cone in his target's direction, locking onto and dealing damage to all enemies hit. |
| Gadgeteen Titan's Resolve | - | 10 Attack Speed %, 20 Armor | Grants 2% Attack Damage, 2 Ability Power, and 2% Attack Speed when attacking or taking damage, stacking up to 30 times. At full stacks, grant 30 Armor and 30 Magic Resist. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. |
| Hand of Nondeterministic Justice | - | 15 Mana, 20 Crit % | Grant 2 effects: 25% Attack Damage and 25 Ability Power.20% Omnivamp. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. Omnivamp: heal for some of damage dealt |
| Induction-Powered Warmog's Armor | - | 900 HP | Regenerate 0.5% maximum Health per second. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. |
| Jump-Started Spear of Shojin | - | 0.1 AD, 20 AP, 45 Mana | Every 3rd attack restores 20 additional Mana. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. |
| Lesser Champion Duplicator | - | - | Use on a champion of Tier 3 or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Loaded Dice | - | - | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover | - | - | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetized Ionic Spark | - | 10 AP, 20 MR | 50% Shred enemies within 2 hexes. When they cast an Ability, they are zapped for magic damage equal to 250% of their maximum Mana. The holder Taunts every 4 seconds. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. [Direct damage item] Shred: reduce Magic Resist Taunt: enemies that are able and in range must attack the taunter |
| OVERFLOW_ERROR // Giant Slayer | - | 0.3 AD, 20 AP, 10 Attack Speed % | If the target has more than 1 maximum Health, deal 30% more damage with Abilities and attacks. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. |
| Onslaught | - | - | Aphelios enters an onslaught, slinging chakrams and dealing massive damage to nearby enemies. |
| Overclocked Sunfire Cape | - | 20 Armor, 300 HP | Every 1 second, 10% Burn and 33% Wound an enemy within 3 hexes for 10 seconds. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. [Unique - only 1 per champion] Burn: deals some of the target's maximum Health as true damage Wound: reduces healing received |
| PRIME Selector | - | - | Use this on a Mecha to make it the PRIME |
| Prison Break! | - | - | A Random 5 cost champion. |
| Prison Break! | - | - | A Random Two Star 4 cost champion. |
| Reforger | - | - | Use on a champion to unequip all items and randomly transform them into different new ones. [Same quality: Reforging will grant a new item of a similar type and quality. Cannot be used on champions in combat, or on Tactician's Crown. Consumable - This item disappears when used.] |
| Shroud of Even Still-erness | - | 20 Armor, 250 HP, 20 Crit % | Combat start: Shoot a beam that 35% Mana Reaves, 20% Chills, and reduces the movement speed of enemies by 20% for 3 seconds. Self-destructs after @TFTUnitProperty.trait:TFT8_GenAE_Durability@ round of player combat. [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast Chill: reduce Attack Speed |
| Tome of Traits | - | - | Gain a Tome of Traits |

## Augments

Tier is inferred from the augment icon art suffix (I/II/III or 1/2/3) or, failing that, the apiName; "Hero augment" marks augments whose icon is a champion portrait. Tiers changed between patches, so treat them as approximate. Silver = I, Gold = II, Prismatic = III.

| Augment | Tier | Effect |
|---|---|---|
| Ancient Archives I | Gold (II) | Gain 1 Tome of Traits and 4 gold. |
| Ascension | Gold (II) | After 15 seconds of combat, your units deal 50% more damage. |
| Axiom Arc II | Gold (II) | Your units gain 30 Mana on kill. |
| Battlemage II | Gold (II) | Your units that start combat in the front 2 rows gain 25 Armor and 25 Ability Power. |
| Big Friend II | Gold (II) | Your units that start combat next to a unit with over 1600 Health take 10% less damage for the rest of combat. |
| Built Different II | Gold (II) | Your units with no Traits active gain 190-355 Health and 40-70% Attack Speed (based on current Stage). |
| Calculated Loss | Gold (II) | After losing your combat, gain 2 gold and a free Shop refresh. |
| Celestial Blessing II | Gold (II) | Your units heal for 15% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 300 Health. |
| Clear Mind | Gold (II) | If you have no champions on your bench at the end of a player combat round, gain 3 experience points. |
| Cluttered Mind | Gold (II) | If your bench is full at the end of a player combat round, gain 3 experience points. Gain 4 random Tier 1 champions. |
| Combat Training | Gold (II) | Your champions permanently gain 1% Attack Damage every time they kill an enemy. Champions start with 8% bonus Attack Damage. |
| Component Grab Bag | Gold (II) | Gain 3 random item components. |
| Cybernetic Implants II | Gold (II) | Your champions holding an item gain 120 Health and 20% bonus Attack Damage. |
| Cybernetic Shell II | Gold (II) | Your champions holding an item gain 120 Health and 40 Armor. |
| Cybernetic Uplink II | Gold (II) | Your champions holding an item gain 120 Health and restore 2.5 Mana per second. |
| Double Trouble II | Gold (II) | When you field exactly 2 copies of a non-threat champion, they both gain 30% Attack Damage and 30 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Electrocharge II | Gold (II) | When your units receive critical strikes, they deal 50-110 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Exiles II | Gold (II) | Your units that start combat with no adjacent allies gain a 35% maximum Health shield for 10 seconds. |
| Featherweights II | Gold (II) | Your Tier 1 and 2 champions gain 30% Attack Speed and Move Speed. |
| First Aid Kit II | Gold (II) | All healing and shielding on your units is increased by 35%. |
| Gadgeteen Crest | Gold (II) | Gain a Gadgeteen Emblem and an Annie. |
| Hustler | Gold (II) | You earn no interest, but gain 3 gold at the start of every player combat round. |
| Jeweled Lotus | Gold (II) | Your units' Abilities can critically strike. Your units gain 20% Critical Strike Chance. |
| Knife's Edge II | Gold (II) | Your units that start combat in the front 2 rows gain 35% Attack Damage. |
| LaserCorps Heart | Gold (II) | Your team counts as having 1 additional LaserCorps. Gain a Yasuo. |
| Last Stand | Gold (II) | The first time you would be eliminated or reduced to 1 Health, instead remain alive. After this effect triggers, your units permanently gain 180 Health, 18 Armor and Magic Resist, and 18% Omnivamp. |
| Luden's Echo II | Gold (II) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 50-110 (based on current Stage) magic damage. |
| Makeshift Armor II | Gold (II) | Your units without items equipped gain 45 Armor and Magic Resist. |
| Metabolic Accelerator | Gold (II) | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Phony Frontline | Gold (II) | Gain 2 Target Dummies. |
| Portable Forge | Gold (II) | Open an Armory and choose 1 of 3 unique Artifacts crafted by Ornn. |
| Preparation II | Gold (II) | Champions on your bench permanently gain 35 Health, 4% Attack Damage, and 4 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Rich Get Richer | Gold (II) | Gain 12 gold. Your maximum interest is increased to 7. |
| Rich Get Richer+ | Gold (II) | Gain 20 gold. Your maximum interest is increased to 7. |
| Salvage Bin | Gold (II) | Gain a random completed item. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Scoped Weapons I | Gold (II) | Your units that start combat in the back 2 rows gain +2 Attack Range and 10% Attack Speed. |
| Second Wind II | Gold (II) | After 10 seconds of combat, your units heal 60% of their missing Health. |
| Stand United II | Gold (II) | Your units gain 2.5% Attack Damage and 2.5 Ability Power per Trait active across your army. |
| Sunfire Board | Gold (II) | At the start of combat, burn all enemies for 10% of their maximum Health over 20 seconds and reduce healing received by 33%. |
| Supers Heart | Gold (II) | Your team counts as having 1 additional Super. Gain a Malphite. |
| Threat Level: Maximum | Gold (II) | Combat start: Threats gain 66 maximum Health for every Threat champion on your board. |
| Three's Company | Gold (II) | Gain 4 random Tier 3 champions. |
| Thrill of the Hunt II | Gold (II) | Your units heal 550 Health on kill. |
| Trade Sector | Gold (II) | Gain a free Shop refresh each round. |
| Trade Sector+ | Gold (II) | Gain a free Shop refresh every round. Gain 10 gold now. |
| Tri Force II | Gold (II) | Your Tier 3 champions gain 125 Health, 15 starting Mana, and 15% Attack Speed. |
| True Twos | Gold (II) | Gain a random 2-star Tier 1 champion and a random 2-star Tier 2 champion. |
| Underground Heart | Gold (II) | Your team counts as having 1 additional Underground. Gain a Vi. |
| A.D.M.I.N. Crown | Gold (II) [from apiName] | Gain an A.D.M.I.N. Emblem, a Hand of Justice, and a Camille. |
| Ace Crown | Gold (II) [from apiName] | Gain an Ace Emblem, a Runaan's Hurricane, and 2 gold. |
| Aegis Crown | Gold (II) [from apiName] | Gain an Aegis Emblem, a Redemption, and an Alistar. |
| Anima Squad Crown | Gold (II) [from apiName] | Gain an Anima Squad Emblem, a Tear of the Goddess, and a Riven. |
| Brawler Crown | Gold (II) [from apiName] | Gain a Brawler Emblem, a Protector's Vow, and a Riven. |
| Civilian Crown | Gold (II) [from apiName] | Gain a Civilian Emblem, a Guinsoo's Rageblade, and a Sivir. |
| Defender Crown | Gold (II) [from apiName] | Gain a Defender Emblem, a Protector's Vow, and a Riven. |
| Duelist Crown | Gold (II) [from apiName] | Gain a Duelist Emblem, an Edge of Night, and a Nilah. |
| Hacker Crown | Gold (II) [from apiName] | Gain a Hacker Emblem, a Hand of Justice, and a LeBlanc. |
| Heart Crown | Gold (II) [from apiName] | Gain a Heart Emblem, a Guardbreaker, and a Sona. |
| LaserCorps Crown | Gold (II) [from apiName] | Gain a LaserCorps Emblem, a Hand of Justice, and a Senna. |
| Mascot Crown | Gold (II) [from apiName] | Gain a Mascot Emblem, a Gargoyle Stoneplate, and a Malphite. |
| Mecha: PRIME Crown | Gold (II) [from apiName] | Gain a Mecha: PRIME Emblem, a Bloodthirster, and a Jax. |
| Ox Force Crown | Gold (II) [from apiName] | Gain an Ox Force Emblem, a Titan's Resolve, and an Alistar. |
| Prankster Crown | Gold (II) [from apiName] | Gain a Prankster Emblem, a Giant's Belt, and a Zoe. |
| Recon Crown | Gold (II) [from apiName] | Gain a Recon Emblem, a Recurve Bow, and an Ezreal. |
| Renegade Crown | Gold (II) [from apiName] | Gain a Renegade Emblem, a Bloodthirster, and a Camille. |
| Spellslinger Crown | Gold (II) [from apiName] | Gain a Spellslinger Emblem, a Tear of the Goddess, and a Sona. |
| Star Guardian Crown | Gold (II) [from apiName] | Gain a Star Guardian Emblem, a Chalice of Power and a Nilah. |
| Sureshot Crown | Gold (II) [from apiName] | Gain a Sureshot Emblem, an Infinity Edge, and a Senna. |
| Absolute Corruption | Hero augment | Gain a Fiddlesticks. Combat start: Fiddlesticks grants its nearest ally 30 Ability Power and allows them to share its Corrupted souls. |
| Absolution | Hero augment | Gain a Senna. She gains 20% Attack Speed and bonus 20% every 5 seconds. |
| Adaptive Defensives | Hero augment | Gain a Camille. She gains 20% Attack Damage. The first time she drops below 50% Health, she gains a shield equal to 500% of her Attack Damage. |
| Aim Assist | Hero augment | Gain a LeBlanc. For every 3 sigils fired by her Ability, she fires an extra sigil at the lowest Health enemy. |
| Armor Piercing Rounds | Hero augment | Gain an Aphelios. His attacks 20% Sunder enemies for the rest of combat (this can stack). |
| Armored-dillo | Hero augment | Gain a Rammus. When you field him, your team gains 35 Armor, doubled for himself. |
| Back for Blood | Hero augment | Gain a Bel'Veth. She has 25% Omnivamp, tripled while under 50% Health. |
| Be the Stone | Hero augment | Gain a Taliyah. Combat start: She shields the 3 highest Health allies for 50% of their maximum Health. |
| Behemoth | Hero augment | Gain an Alistar. He has 450 bonus Health and his Ability deals bonus damage equal to 30% of his maximum Health. |
| Bigger, Better Buckler | Hero augment | Gain a Poppy. She gains 180 Armor. |
| Boxing Lessons | Hero augment | Gain a Vi. When you field her, your team gains 180 bonus Health. |
| Bunny Mercenary | Hero augment | Gain a Miss Fortune. Her Ability covers a wider cone. When she casts her ability, she gains a 200 Health shield for 2 seconds. |
| Burning Spirit | Hero augment | Gain an Annie. When you field her, your team gains 15 Ability Power, tripled while they are below 50% Health. |
| Category Five | Hero augment | Gain a Janna. She gains 80 Ability Power and her ability does True Damage. |
| Channeled Ferromancy | Hero augment | Gain a Rell. When you field her, your team gains 20 Armor and Magic Resist, and an additional 5 when they cast Abilities (this can stack). |
| Chronobreak | Hero augment | Gain an Ekko. When you field him, he Stuns all enemies for 2 seconds after 10 seconds of combat. |
| Cleansing Safeguard | Hero augment | Gain a Lee Sin. His Ability costs 30 less Mana to cast. When he casts, he sheds all negative effects and heals 18% of his missing Health. |
| Contagious Laughter | Hero augment | Gain a Nunu. When you field him, your team gains 15 Ability Power and an additional 10 every 5 seconds. |
| Contempt for the Weak | Hero augment | Gain a Zed. When you field him, your team gains 7% Attack Speed. On takedown, your team dashes to a new enemy and triples this bonus for 2.5 seconds. |
| Corps Focus | Hero augment | Gain an Ashe. When you field her, your team gains 15% Attack Speed, doubled while her Ability is active. |
| Corps Formation | Hero augment | Gain a Senna. Combat start: Your strongest Senna grants herself and adjacent allies 30% Attack Damage. |
| Cosmic Barrier | Hero augment | Gain a Cho'Gath. When you field him, your team gains 35 Magic Resist, doubled for Cho'Gath. |
| Cull the Meek | Hero augment | Gain a Renekton. When you field him, your team deals 8% bonus damage, tripled against enemies below 50% Health. |
| Cyclone | Hero augment | Gain a Wukong. He has 10 Ability Power and +1 hex Ability range. |
| Daredevil | Hero augment | Gain a Samira. Your team gains 3.5% Attack Speed for each 10 Tactician Health missing. |
| Delivery Tips | Hero augment | Gain a Sivir. Every 2 Ability casts, your strongest Sivir drops 1 gold with a 5% chance to drop 1 extra gold. |
| Divine Ascent | Hero augment | Gain a Kayle. She gains bonuses per star level: 1: 50% Attack Speed 2: And +3 Attack Range 3: And attacks fire 2 extra bolts that deal 50% damage |
| Double Bubble | Hero augment | Gain a Zoe. Her Ability launches a second bubble at a different target, dealing 70% damage. |
| Dynamic Defenses | Hero augment | Gain a Blitzcrank and a Gargoyle Stoneplate. When he casts his Ability, its duration increases by 2 seconds per enemy targeting him. |
| Eclipse Prime | Hero augment | Gain a Leona. When you field her, your team takes 20 less damage. |
| Edgelord | Hero augment | Gain a Talon. He gains 50 Ability Power. After killing an enemy, he restores 40 Mana. |
| Elastic Slingshot | Hero augment | Gain a Zac. Combat start: He flings his 2 nearest allies to the enemy backline, Stunning enemies for 4 seconds. |
| Empowered Reserves | Hero augment | Gain a Syndra. When you field her, your team gains 5 Ability Power per champion on your bench. |
| Endless Pizza | Hero augment | Gain a Sivir. When you field her, your team heals 12% of their missing Health and sheds all negative effects every 8 seconds. |
| Energy Void | Hero augment | Gain a Cho'Gath. Each time his Ability hits an enemy, he steals 70% of their Magic Resist. |
| Evasion | Hero augment | Gain a Jax. When you field him, your team gains 100% Dodge Chance for 2 seconds the first time they fall below 50% Health each combat. |
| Everyone Goes BOOM! | Hero augment | Gain a Jinx. When you field her, your team gains 9% Attack Speed, tripled after the first enemy dies each combat. |
| Exaggerated Reporting | Hero augment | Gain a Janna. Forecaster's effects are increased by 100%. |
| Extinction Event | Hero augment | Gain an Aurelion Sol. He has 15 Ability Power and his Ability's meteors are larger. |
| Flaming Ricochet | Hero augment | Gain a Gangplank. His Ability bounces to a second target, dealing 90% damage. |
| Foster Growth | Hero augment | Gain a Lulu. When you field her, she and allies holding an item gain 40 Ability Power. |
| Frontline Fencing | Hero augment | Gain a Fiora. While her Ability is active, she gains 250 Armor and Magic Resist. |
| Frostburn | Hero augment | Gain a Vel'Koz. His Ability deals an additional 400% of its damage over the next 20 seconds (this can stack). |
| Frozen Tundra | Hero augment | Gain a Vel'Koz. Combat start: He 30% Chills enemies for 10 seconds. |
| Get Excited! | Hero augment | Gain a Jinx. She gains 40% Attack Speed and Move speed. When she scores a takedown this amount is tripled for 7 seconds. |
| Get Paid | Hero augment | Gain a Gangplank. When you field him, your team has a 50% chance to gain 1 gold on kill. |
| Gifted | Hero augment | Gain a Nilah. She gains 45 Ability Power. Combat start: Your strongest Nilah grants her nearest ally a temporary copy of one of her normal items. |
| Glacial Prison | Hero augment | Gain a Sejuani. She has 20 Armor and Magic Resist, and her Ability hits the entire board. |
| Growth Spurt | Hero augment | Gain a Lulu. When she casts her Ability, she gains 100 bonus Health and 20 Ability Power (this can stack). |
| Guardian Spirit | Hero augment | Gain a Malphite. When you field him, your team restores 20% of their missing Health when they cast Abilities. |
| Heartstopper | Hero augment | Gain a Viego. He has 10 Ability Power, and his Ability deals up to 50% increased damage based on the enemy's missing Health. |
| Hextech Retribution | Hero augment | Gain a Camille. When you field her, your team deals 8% bonus damage, increasing by an additional 5% whenever an ally dies. |
| Hold the Line | Hero augment | Gain a Rell. She has 80 Ability Power and takes 35% less damage. |
| Illuminating Singularity | Hero augment | Gain a Lux. Every 2nd cast, her Ability deals 135% more damage. |
| Impact Velocity | Hero augment | Gain an Aurelion Sol. His Ability Stuns enemies hit for 1.75 seconds. |
| Infuse | Hero augment | Gain a Soraka. When you field her, your team restores 20 Mana every 5 seconds. |
| Into The Night | Hero augment | Gain a Vayne and an Edge of Night. When you field her, allies holding Edge of Night gain 35% Attack Speed. |
| Invigorate | Hero augment | Gain a Lee Sin. When you field him, your team gains 20% Attack Speed, doubled for 4 seconds after being shielded. |
| Jubilant Veil | Hero augment | Gain a Nilah. Combat start: She and her 5 nearest allies gain immunity to crowd control for 30 seconds. |
| Justice Punch | Hero augment | Gain a Galio. His Ability costs 70 less Mana to cast, deals 150% more damage and knocks up its target for 2 seconds. |
| Kingslayer | Hero augment | Gain a Sylas. He gains 25% maximum Health and deals 35% increased damage. |
| Laser Focus | Hero augment | Gain an Ashe. She gains 50% Attack Speed, doubled while her Ability is active. |
| League of Draven | Hero augment | Gain a Draven. He has 25% bonus Attack Damage. When he kills an enemy, there is a 35% chance to gain 1 gold. |
| Locked and Loaded | Hero augment | Gain an Aphelios. When you field him, your team gains 5% Attack Damage plus an extra 5% every 5 seconds. |
| Lucent Barrier | Hero augment | Gain a Lux. Combat start: She grants her 2 nearest allies a 500 Health shield and 35 Ability Power. |
| Make It Rain | Hero augment | Gain a Miss Fortune. After every 3 player combats fielded, she drops 8-20 gold (based on current Stage). |
| Mirror Image | Hero augment | Gain a LeBlanc. Combat start: Your strongest LeBlanc duplicates her nearest ally (without items or traits at 75% Health). |
| Multi-Shot | Hero augment | Gain a Kai'Sa. Every 3 attacks, she fires a bonus attack at a nearby enemy. |
| Not So Heavy Metal | Hero augment | Gain a Mordekaiser. He gains 20 Ability Power every 4 seconds. |
| OX-ian Rage | Hero augment | Gain a Talon. When you field him, your team gains 10% Attack Damage and 10 Ability Power, tripled when they are below 50% Health. |
| Obliterate | Hero augment | Gain a Mordekaiser. Combat start: He 20% Shreds and 20% Sunders enemies. |
| Partners in Crime | Hero augment | Gain a Viego. When you field him, your team gains 12% Omnivamp, tripled when 2 or less allies remain. |
| Perfected Solar Flare | Hero augment | Gain a Leona. She has 30 Ability Power, and her Abilities cost 30 less Mana to cast. |
| Petricite Chains | Hero augment | Gain a Sylas. When you field him, your strongest Sylas grants your team 30 starting Mana, and his Ability 35% Mana Reaves enemies. |
| Power Grid | Hero augment | Gain a Sona. She gains 10 Ability Power and her Ability fires 1 additional beam. |
| Power Overwhelming | Hero augment | Gain a Syndra. Allies summoned by her Ability gain 100% Attack Damage and 100 Ability Power, Armor, and Magic Resist. |
| Predatory Precision | Hero augment | Gain a Yuumi. She gains 5 Ability Power, 75% Critical Strike Chance, and her Ability can critically strike. |
| Punch Protocol | Hero augment | Gain a Sett. He has 40 Armor and Magic Resist. His Ability knocks back the first enemy struck, destroying them if knocked off the arena. |
| Raider's Spoils | Hero augment | Gain an Ezreal. Combat start: Your strongest Ezreal grants his nearest ally a temporary Artifact for the rest of combat. |
| Re-Energize | Hero augment | Gain a Wukong. When you field him, your team restores 50% of their Mana after their first cast each combat. |
| Reflector Shield | Hero augment | Gain an Annie. While her Ability's shield is active, enemies that attack her take 200 magic damage (once every 0.1 seconds). |
| Regenerative Shields | Hero augment | Gain Sett and a Protector's Vow. After your team's shields break, Sett recharges them at 40% strength for 3 seconds. |
| Reign of Anger | Hero augment | Gain a Renekton. He gains 65% Attack Speed and an extra 4% per 100 missing Health. |
| Relentless Assault | Hero augment | Gain a Jax. After every 3rd attack, he gains 12% Attack Speed for the rest of combat (this can stack). |
| Resonance | Hero augment | Gain an Ekko. His Ability hits all enemies within 2 hexes and deals bonus damage equal to 400% of his Magic Resistance. |
| Reverberation | Hero augment | Gain a Riven. She has 15 Ability Power and her Ability is also cast on her lowest Health ally. |
| Righteous Range | Hero augment | Gain a Kayle. Combat start: She and her 2 nearest allies gain +2 Attack Range and 25% Attack Speed. |
| Rising Spell Force | Hero augment | Gain an Ezreal. When he casts his Ability, he gains 20% Attack Speed and 20 Ability Power for the rest of combat (this can stack). |
| Rising Tide | Hero augment | Gain an Urgot. Combat start: He grants adjacent allies 30% Attack Speed. |
| Rock Solid | Hero augment | Gain a Malphite. He has Ability Power equal to 180% of his Armor, and his Ability's Armor duration is increased by 4 seconds. |
| Rocket Grab | Hero augment | Gain a Blitzcrank. He has 400 bonus Health. Combat start: He pulls the farthest enemy and Stuns them for 1.5 seconds. |
| Ruthless Blades | Hero augment | Gain a Draven. When you field him, your team gains 75% Critical Strike Chance. |
| Safety First | Hero augment | Gain a Galio. When you field him, your team gains 20 Armor and Magic Resist, doubled for 4 seconds after being shielded. |
| Shadow Jutsu | Hero augment | Gain a Zed. He has 20% Attack Damage, and for the rest of combat, he gains 20% of an enemy's Attack Damage on kill. |
| Shatter | Hero augment | Gain a Sejuani. Her Ability Stuns for 1 more seconds, and enemies take 33% more damage while Stunned. |
| Shiny | Hero augment | Gain an Urgot. His Ability has an extra 25% chance to dredge up treasure. |
| Siphoning Winds | Hero augment | Gain a Yasuo. He has 60% Omnivamp. |
| Sleepy Time | Hero augment | Gain a Zoe. Combat start: She Stuns the 4 lowest Health enemies for 4 seconds. |
| Smash! | Hero augment | Gain an Alistar. He restores 15 Mana per second and his Ability hits all enemies within 1 hex(es). |
| Soul Eater | Hero augment | Gain a Nasus. When you field him, your team heals 300 Health on takedowns. |
| Spiked Shell | Hero augment | Gain a Rammus. When struck by an attack, he deals magic damage equal to 150% of his Armor to all nearby enemies (once every 2 seconds). |
| Spirit of the Exile | Hero augment | Gain a Yasuo. When you field him, allies who start combat with no adjacent allies gain 22% Attack Speed. |
| Spread Shot | Hero augment | Gain a Vayne. Her Ability also fires at 2 other nearby enemies, dealing 65% damage. |
| Stacks on Stacks | Hero augment | Gain a Nasus. He has 15% Attack Damage, which increases permanently by 2% when he casts his Ability. |
| Star-Crossed | Hero augment | Gain a Kai'Sa. She grants 100% of her Ability's Attack Speed bonus to her nearest ally. |
| Steadfast Presence | Hero augment | Gain a Poppy. Combat start: Your strongest Poppy grants your team a 325 Health shield. |
| Stoneweaver | Hero augment | Gain a Taliyah. She has 20 Ability Power, and her Ability deals 20% more damage to enemies above 1600 maximum Health. |
| Style & Flair | Hero augment | Gain a Samira. She has 30% Attack Damage. When she scores a takedown, she restores 30% of her maximum Mana. |
| Supersize | Hero augment | Gain a Zac. He gains 1000 bonus Health and is immune to crowd control. |
| They See Me Rolling | Hero augment | Gain a Nunu. He has 25 Ability Power and 20% movement speed. |
| Traumatic Memories | Hero augment | Gain a Fiddlesticks. When an ally dies, Fiddlesticks gains 350 bonus Health for the rest of combat. |
| Triumphant Return | Hero augment | Gain a Riven. Combat start: She grants herself and adjacent allies a shield for 15 seconds equal to 40% of their maximum Health. |
| Undercurrent | Hero augment | Gain a Sona. Combat start: Your strongest Sona grants her 3 nearest allies 50 Mana. |
| Unrelenting Force | Hero augment | Gain a Vi. When she takes or deals damage, she gains 3% Attack Damage and 3 Ability Power, Armor, and Magic Resist, stacking up to 25 times. |
| Upgrade: Berserk | Hero augment | Gain a Soraka. After 15 seconds of combat, all of her Ability casts are empowered stars. |
| Vitality of the Ox | Hero augment | Gain a Fiora. When you field her, your team heals 2% of their maximum Health when they attack. |
| Voidmother | Hero augment | Gain a Bel'Veth. Combat start: Your strongest Bel'Veth creates 2 Voidspawns with 60% of their normal Health in front of her. |
| Zoomies! | Hero augment | Gain a Yuumi. When you field her, your team gains 22% Attack Speed, and zooms towards their targets. |
| Ancient Archives II | Prismatic (III) | Gain 2 Tome of Traits and 8 gold. |
| Band of Thieves II | Prismatic (III) | Gain 2 Thief's Gloves. |
| Battlemage III | Prismatic (III) | Your units that start combat in the front 2 rows gain 35 Armor and 35 Ability Power. |
| Binary Airdrop | Prismatic (III) | Your champions equipped with 2 items temporarily gain a random completed item at the start of combat. Gain 1 random item components. |
| Birthday Present | Prismatic (III) | Gain a 2-star champion every time you level up. The champion's tier is your level minus 4 (min: Tier 1). |
| Built Different III | Prismatic (III) | Your units with no Traits active gain 270-600 Health and 50-80% Attack Speed (based on current Stage). |
| Celestial Blessing III | Prismatic (III) | Your units heal for 25% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 400 Health. |
| Cruel Pact | Prismatic (III) | Buying XP costs 6 Health instead of gold. Heal 2 Health each round. |
| Cursed Crown | Prismatic (III) | Gain +2 maximum team size, but take 100% increased player damage. |
| Cybernetic Implants III | Prismatic (III) | Your champions holding an item gain 200 Health and 30% bonus Attack Damage. |
| Cybernetic Shell III | Prismatic (III) | Your champions holding an item gain 200 Health and 60 Armor. |
| Cybernetic Uplink III | Prismatic (III) | Your champions holding an item gain 200 Health and restore 3.5 Mana per second. |
| Double Trouble III | Prismatic (III) | When you field exactly 2 copies of a non-threat champion, they both gain 40% Attack Damage and 40 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Electrocharge III | Prismatic (III) | When your units receive critical strikes, they deal 115-235 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Featherweights III | Prismatic (III) | Your Tier 1 and 2 champions gain 50% Attack Speed and Move Speed. |
| Future Sight II | Prismatic (III) | Know who you will fight next. Gain a Radiant Zephyr. |
| Gadgeteen Crown | Prismatic (III) | Gain a Gadgeteen Emblem, a Negatron Cloak, and an Annie. |
| Golden Ticket | Prismatic (III) | Each time your Shop is refreshed, you have a 50% chance to gain a free refresh. |
| High End Shopping | Prismatic (III) | Champions appear in your Shop as if you were 1 level higher. Gain 3 gold. |
| High Roller | Prismatic (III) | Gain 3 Loaded Dice and 5 gold. |
| Item Grab Bag II | Prismatic (III) | Gain 2 random completed items and 2 Reforgers. |
| Knife's Edge III | Prismatic (III) | Your units that start combat in the front 2 rows gain 55% Attack Damage. |
| Level Up! | Prismatic (III) | When you buy experience points, gain an additional 3. You can now reach level 10. |
| Living Forge | Prismatic (III) | Gain a random Ornn artifact now and after every 10 player combats. |
| Lucky Gloves | Prismatic (III) | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves. |
| Luden's Echo III | Prismatic (III) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 90-210 (based on current Stage) magic damage. |
| March of Progress | Prismatic (III) | Gain 5 bonus experience points at the start of every player combat round. You can no longer use gold to level up. |
| New Recruit | Prismatic (III) | Gain +1 maximum team size. |
| Preparation III | Prismatic (III) | Champions on your bench permanently gain 50 Health, 7% Attack Damage, and 7 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Radiant Relics | Prismatic (III) | Open an Armory and choose 1 of 5 unique Radiant items. |
| Stand United III | Prismatic (III) | Your units gain 3.5% Attack Damage and 3.5 Ability Power per Trait active across your army. |
| Supers Soul | Prismatic (III) | Your team counts as having 1 additional Super. Gain a Titan's Resolve and a Lee Sin. |
| The Golden Egg | Prismatic (III) | Gain a massive golden egg that hatches in 11 turns. Victorious player combats accelerate the hatch timer by an additional turn. |
| Think Fast | Prismatic (III) | Shop refreshes are free until the end of this round. Traits and other augments do not benefit from these free shops. Gain 3 gold. |
| Tri Force III | Prismatic (III) | Your Tier 3 champions gain 200 Health, 25 starting Mana, and 25% Attack Speed. |
| Underground Soul | Prismatic (III) | Your team counts as having 1 additional Underground. Gain a Zz'Rot Portal and an Ezreal. |
| Urf's Grab Bag II | Prismatic (III) | Gain 1 Spatula and 3 random item component(s). |
| Verdant Veil | Prismatic (III) | Your units are immune to crowd control effects for the first 20 seconds of combat and gain 15% bonus Attack Speed. |
| Windfall | Prismatic (III) | Gain 25 gold. |
| Windfall+ | Prismatic (III) | Gain 35 gold. |
| Windfall++ | Prismatic (III) | Gain 45 gold. |
| Wise Spending | Prismatic (III) | When you refresh your Shop, gain 2 experience point(s). |
| Woodland Charm | Prismatic (III) | Your highest Health champion is cloned. You cannot equip items on the clone. |
| AFK | Silver (I) | You cannot perform actions for the next 3 rounds. Afterwards, gain 18 gold. |
| Band of Thieves I | Silver (I) | Gain 1 Thief's Gloves. |
| Battlemage I | Silver (I) | Your units that start combat in the front 2 rows gain 15 Armor and 15 Ability Power. |
| Big Friend I | Silver (I) | Your units that start combat next to a unit with over 1600 Health take 7% less damage for the rest of combat. |
| Celestial Blessing I | Silver (I) | Your units heal for 10% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 200 Health. |
| Consistency | Silver (I) | Gain double win and loss streak gold. |
| Cybernetic Implants I | Silver (I) | Your champions holding an item gain 80 Health and 10% bonus Attack Damage. |
| Cybernetic Shell I | Silver (I) | Your champions holding an item gain 80 Health and 30 Armor. |
| Cybernetic Uplink I | Silver (I) | Your champions holding an item gain 80 Health and restore 2 Mana per second. |
| Electrocharge I | Silver (I) | When your units receive critical strikes, they deal 30-90 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Exiles I | Silver (I) | Your units that start combat with no adjacent allies gain a 25% maximum Health shield for 10 seconds. |
| Featherweights I | Silver (I) | Your Tier 1 and 2 champions gain 20% Attack Speed and Move Speed. |
| First Aid Kit I | Silver (I) | All healing and shielding on your units is increased by 25%. |
| Future Sight I | Silver (I) | Know who you will fight next. Gain a Zephyr. |
| Gadgeteen Heart | Silver (I) | Your team counts as having 1 additional Gadgeteen. Gain a Poppy. |
| Item Grab Bag I | Silver (I) | Gain 1 random completed item. |
| Knife's Edge I | Silver (I) | Your units that start combat in the front 2 rows gain 25% Attack Damage. |
| Lategame Specialist | Silver (I) | When you reach Level 9, gain 40 gold. |
| Luden's Echo I | Silver (I) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 35-80 (based on current Stage) magic damage. |
| Makeshift Armor I | Silver (I) | Your units without items equipped gain 30 Armor and Magic Resist. |
| Pandora's Bench | Silver (I) | Gain 2 gold. At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Pandora's Items | Silver (I) | Gain a random component. At the start of each round, items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Preparation I | Silver (I) | Champions on your bench permanently gain 25 Health, 3% Attack Damage, and 3 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Recombobulator | Silver (I) | Champions on your board permanently transform into random champions 1 Tier higher. Gain 2 Magnetic Removers. |
| Second Wind I | Silver (I) | After 10 seconds of combat, your units heal 40% of their missing Health. |
| Stand United I | Silver (I) | Your units gain 1.5% Attack Damage and 1.5 Ability Power per Trait active across your army. |
| Thrill of the Hunt I | Silver (I) | Your units heal 350 Health on kill. |
| Tiny Titans | Silver (I) | Your Tactician heals 30 Health, grows larger, and has 130 maximum Health. |
| Tri Force I | Silver (I) | Your Tier 3 champions gain 75 Health, 10 starting Mana, and 10% Attack Speed. |
| A.D.M.I.N. Crest | unknown | Gain an A.D.M.I.N. Emblem and a Blitzcrank. |
| A.D.M.I.N. Heart | unknown | Your team counts as having 1 additional A.D.M.I.N. Gain a Camille. |
| Ace Crest | unknown | Gain an Ace Emblem and 2 gold. |
| Aegis Crest | unknown | Gain an Aegis Emblem and a Vi. |
| Anima Squad Crest | unknown | Gain an Anima Squad Emblem and a Sylas. |
| Anima Squad Heart | unknown | Your team counts as having 1 additional Anima Squad. Gain a Jinx. |
| Brawler Crest | unknown | Gain a Brawler Emblem and a Lee Sin. |
| Brawler Heart | unknown | Your team counts as having 1 additional Brawler. Gain a Vi. |
| Civilian Crest | unknown | Gain a Civilian Emblem and a Galio. |
| Defender Crest | unknown | Gain a Defender Emblem and a Poppy. |
| Defender Heart | unknown | Your team counts as having 1 additional Defender. Gain a Rell. |
| Duelist Crest | unknown | Gain a Duelist Emblem and a Yasuo. |
| Duelist Heart | unknown | Your team counts as having 1 additional Duelist. Gain a Fiora. |
| Hacker Crest | unknown | Gain a Hacker Emblem and 2 gold. |
| Hacker Heart | unknown | Your team counts as having 1 additional Hacker. Gain 2 gold. |
| Heart Crest | unknown | Gain a Heart Emblem and a Yuumi. |
| Heart Heart | unknown | Your team counts as having 1 additional Heart. Gain a Lee Sin. |
| Mascot Crest | unknown | Gain a Mascot Emblem and a Yuumi. |
| Mascot Heart | unknown | Your team counts as having 1 additional Mascot. Gain a Yuumi. |
| Mecha: PRIME Crest | unknown | Gain a Mecha: PRIME Emblem and a Draven. |
| Ox Force Crest | unknown | Gain an Ox Force Emblem and an Annie. |
| Ox Force Heart | unknown | Your team counts as having 1 additional Ox Force. Gain a Fiora. |
| Prankster Crest | unknown | Gain a Prankster Emblem and a Jinx. |
| Recon Crest | unknown | Gain a Recon Emblem. |
| Recon Heart | unknown | Your team counts as having 1 additional Recon. Gain an Ezreal. |
| Renegade Crest | unknown | Gain a Renegade Emblem and a Talon. |
| Renegade Heart | unknown | Your team counts as having 1 additional Renegade. Gain a Camille. |
| Spellslinger Crest | unknown | Gain a Spellslinger Emblem and an Annie. |
| Spellslinger Heart | unknown | Your team counts as having 1 additional Spellslinger. Gain an Annie. |
| Star Guardian Crest | unknown | Gain a Star Guardian Emblem and a Rell. |
| Star Guardian Heart | unknown | Your team counts as having 1 additional Star Guardian. Gain a Yuumi. |
| Sureshot Crest | unknown | Gain a Sureshot Emblem and a Sivir. |
| Sureshot Heart | unknown | Your team counts as having 1 additional Sureshot. Gain a Sivir. |

