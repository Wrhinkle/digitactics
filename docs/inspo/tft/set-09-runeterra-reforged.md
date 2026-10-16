# TFT Set 9: Runeterra Reforged

Source: CommunityDragon `https://raw.communitydragon.org/13.17/cdragon/tft/en_us.json` (set data `TFTSet9`) for champions, stats, abilities, traits, items and augments. MetaTFT lookup `https://data.metatft.com/lookups/TFTSet9_latest_en_us.json` (retrieved 2026-10-15) for spell calculation formulas used to fill computed tooltip values, augment rarity where CDragon has none, and set-mechanic lists. Mechanics text: Riot "Runeterra Reforged Mechanics Overview" (teamfighttactics.leagueoflegends.com/en-gb/news/game-updates/runeterra-reforged-mechanics-overview).

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 9 |
| Name | Runeterra Reforged |
| Riot launch patch | 13.12 |
| League client version at launch (CDragon path) | 13.12 |
| Live dates | June 14, 2023 to September 12, 2023 (replaced by mid-set 9.5 Horizonbound in patch 13.18) |
| Theme | The regions of Runeterra (League of Legends lore). Origins are regions such as Demacia, Noxus, Ionia, Shurima, Zaun. |
| Data snapshot used | Patch 13.17 (last patch before 9.5) |
| Champion count in this file | 69 entries (includes region variants of Ryze) |

## Set mechanics and system changes

- Region Portals replaced the opening carousel. Three portals appear at the start; each player moves to one to vote; one portal is picked at random with votes weighting the odds. The chosen portal sets a rule for the whole lobby. Riot listed at least 28 portals at launch; this file lists every portal in the MetaTFT data (62 rows, including variants). Examples from Riot: Zaun (no interest, flat 3 gold per round from 2-1), Piltover (augments disabled), Shurima (4-7 raptors replaced by a Treasure Armory rerollable for 2 gold), Shadow Isles (collect souls from deaths, loot per stage).
- Legends: before queueing, the player picks one of 15 Legends. A Legend shapes the leftmost of the three augment choices at each augment round. Riot: the 2-1 Legend augment is equal power to its tier; the 3-2 and 4-2 Legend augments are slightly weaker than same-tier augments.
- Augment rerolls changed: each augment card can be rerolled individually; Riot listed 18 total augment rerolls per game. Over 100 new augments were added.
- Unique champions: Ryze (Wanderer) has a different spell depending on the Region Portal the lobby voted for (this file lists all 11 Ryze variants). Heimerdinger (Technogenius) gives a placeable Apex Turret with 3 upgrade slots; turret upgrades appear in the shop for 6 gold.
- Set-specific items in data: Chempunk mods (Zaun), Piltover charges, Crown of Demacia, Darkin blade (World Ender), Baron's Head, Scroll of Traits, Masterwork Upgrade.
- Shimmerscale items from Set 7 and radiant items appear in the item pool through augments and portals.

## Champions (69)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Cassiopeia | 1 | Noxus, Shurima, Invoker | 500 | 20 | 20 | 40 | 0.7 | 4 | 0/30 |
| Cho'Gath | 1 | Void, Bruiser | 700 | 30 | 30 | 50 | 0.5 | 1 | 30/90 |
| Irelia | 1 | Ionia, Challenger | 700 | 40 | 40 | 50 | 0.65 | 1 | 40/80 |
| Jhin | 1 | Ionia, Deadeye | 504 | 14 | 14 | 54 | 0.7 | 3 | 44/144 |
| Kayle | 1 | Demacia, Slayer | 500 | 15 | 15 | 30 | 0.75 | 4 | 0/0 |
| Malzahar | 1 | Void, Sorcerer | 500 | 15 | 15 | 40 | 0.7 | 4 | 0/50 |
| Maokai | 1 | Shadow Isles, Bastion | 700 | 40 | 40 | 60 | 0.5 | 1 | 50/125 |
| Orianna | 1 | Piltover, Sorcerer | 500 | 15 | 15 | 40 | 0.7 | 4 | 50/100 |
| Poppy | 1 | Yordle, Demacia, Bastion | 600 | 40 | 40 | 45 | 0.55 | 1 | 90/150 |
| Renekton | 1 | Shurima, Bruiser | 650 | 45 | 45 | 50 | 0.6 | 1 | 50/110 |
| Samira | 1 | Noxus, Challenger | 500 | 15 | 15 | 50 | 0.7 | 4 | 0/30 |
| Tristana | 1 | Yordle, Gunner | 400 | 15 | 15 | 45 | 0.7 | 4 | 0/40 |
| Viego | 1 | Shadow Isles, Rogue | 600 | 30 | 30 | 45 | 0.75 | 1 | 0/50 |
| Ashe | 2 | Freljord, Deadeye | 600 | 20 | 20 | 50 | 0.75 | 4 | 20/70 |
| Galio | 2 | Demacia, Invoker | 800 | 45 | 45 | 70 | 0.5 | 1 | 60/120 |
| Jinx | 2 | Zaun, Gunner | 600 | 20 | 20 | 55 | 0.75 | 4 | 0/75 |
| Kassadin | 2 | Void, Bastion | 800 | 45 | 45 | 50 | 0.7 | 1 | 50/100 |
| Kled | 2 | Noxus, Yordle, Slayer | 550 | 40 | 40 | 58 | 0.75 | 1 | 0/175 |
| Sett | 2 | Ionia, Juggernaut | 800 | 50 | 50 | 60 | 0.6 | 1 | 40/90 |
| Soraka | 2 | Targon, Invoker | 700 | 25 | 25 | 55 | 0.65 | 4 | 30/90 |
| Swain | 2 | Noxus, Strategist, Sorcerer | 800 | 50 | 50 | 45 | 0.65 | 2 | 20/70 |
| Taliyah | 2 | Shurima, Multicaster | 550 | 20 | 20 | 40 | 0.7 | 4 | 0/60 |
| Teemo | 2 | Yordle, Strategist, Multicaster | 500 | 20 | 20 | 40 | 0.7 | 4 | 0/50 |
| Vi | 2 | Piltover, Bruiser | 800 | 50 | 50 | 60 | 0.6 | 1 | 0/80 |
| Warwick | 2 | Zaun, Juggernaut, Challenger | 800 | 50 | 50 | 40 | 0.75 | 1 | 40/90 |
| Zed | 2 | Ionia, Rogue, Slayer | 600 | 35 | 35 | 55 | 0.7 | 1 | 0/70 |
| Akshan | 3 | Shurima, Deadeye | 700 | 25 | 25 | 60 | 0.75 | 4 | 30/110 |
| Darius | 3 | Noxus, Juggernaut | 750 | 50 | 50 | 70 | 0.75 | 1 | 30/90 |
| Ekko | 3 | Zaun, Piltover, Rogue | 800 | 40 | 40 | 50 | 0.8 | 1 | 0/45 |
| Garen | 3 | Demacia, Juggernaut | 850 | 45 | 45 | 70 | 0.75 | 1 | 0/80 |
| Jayce | 3 | Piltover, Gunner | 700 | 25 | 25 | 65 | 0.75 | 4 | 40/120 |
| Kalista | 3 | Shadow Isles, Challenger | 650 | 25 | 25 | 45 | 0.85 | 4 | 0/80 |
| Karma | 3 | Ionia, Invoker | 700 | 25 | 25 | 45 | 0.7 | 4 | 0/50 |
| Katarina | 3 | Noxus, Rogue | 800 | 40 | 40 | 50 | 0.75 | 1 | 20/80 |
| Lissandra | 3 | Freljord, Invoker | 800 | 40 | 40 | 50 | 0.7 | 2 | 30/90 |
| Rek'Sai | 3 | Void, Bruiser | 800 | 40 | 40 | 65 | 0.7 | 1 | 30/75 |
| Sona | 3 | Demacia, Multicaster | 700 | 25 | 25 | 40 | 0.65 | 4 | 40/90 |
| Taric | 3 | Targon, Bastion, Sorcerer | 800 | 50 | 50 | 60 | 0.6 | 1 | 0/65 |
| Vel'Koz | 3 | Void, Multicaster, Sorcerer | 700 | 30 | 30 | 40 | 0.7 | 4 | 0/60 |
| Aphelios | 4 | Targon, Deadeye | 750 | 30 | 30 | 65 | 0.8 | 4 | 70/120 |
| Azir | 4 | Shurima, Strategist | 750 | 30 | 30 | 30 | 0.75 | 4 | 40/50 |
| Gwen | 4 | Shadow Isles, Slayer | 1000 | 50 | 50 | 55 | 0.8 | 2 | 0/35 |
| Jarvan IV | 4 | Demacia, Strategist | 1000 | 60 | 60 | 60 | 0.65 | 1 | 100/180 |
| Kai'Sa | 4 | Void, Challenger | 850 | 35 | 35 | 45 | 0.8 | 4 | 30/125 |
| Lux | 4 | Demacia, Sorcerer | 750 | 30 | 30 | 45 | 0.7 | 4 | 0/40 |
| Nasus | 4 | Shurima, Juggernaut | 1000 | 60 | 60 | 60 | 0.65 | 1 | 70/150 |
| Sejuani | 4 | Freljord, Bruiser | 1000 | 60 | 60 | 60 | 0.6 | 1 | 60/120 |
| Shen | 4 | Ionia, Bastion, Invoker | 1000 | 60 | 60 | 60 | 0.6 | 1 | 70/150 |
| Urgot | 4 | Zaun, Deadeye | 1000 | 60 | 60 | 75 | 0.7 | 1 | 40/100 |
| Yasuo | 4 | Ionia, Challenger | 1000 | 50 | 50 | 75 | 0.8 | 1 | 50/110 |
| Zeri | 4 | Zaun, Gunner | 750 | 25 | 25 | 65 | 0.8 | 4 | 0/50 |
| Aatrox | 5 | Darkin, Slayer, Juggernaut | 1100 | 70 | 70 | 80 | 0.8 | 1 | 0/50 |
| Ahri | 5 | Ionia, Sorcerer | 850 | 40 | 40 | 50 | 0.85 | 4 | 0/50 |
| Bel'Veth | 5 | Void, Empress | 1100 | 50 | 50 | 80 | 0.85 | 2 | 20/70 |
| Heimerdinger | 5 | Piltover, Yordle, Technogenius | 750 | 30 | 30 | 40 | 0.75 | 4 | 80/160 |
| K'Sante | 5 | Shurima, Bastion | 1000 | 60 | 60 | 60 | 0.7 | 1 | 0/60 |
| Ryze (Ryze) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 0/999 |
| Ryze (RyzeDemacia) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 50/110 |
| Ryze (RyzeBandleCity) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 30 | 0.85 | 4 | 30/75 |
| Ryze (RyzeFreljord) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/65 |
| Ryze (RyzeIonia) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 50/125 |
| Ryze (RyzeNoxus) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 70/120 |
| Ryze (RyzePiltover) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/100 |
| Ryze (RyzeShadowIsles) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 40/80 |
| Ryze (RyzeShurima) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 90/150 |
| Ryze (RyzeTargon) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/100 |
| Ryze (RyzeZaun) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 40/100 |
| Senna | 5 | Shadow Isles, Gunner, Redeemer | 900 | 30 | 30 | 65 | 0.8 | 4 | 75/150 |
| Sion | 5 | Noxus, Bruiser | 1000 | 60 | 60 | 70 | 0.6 | 1 | 70/160 |

### Abilities

**Cassiopeia** (1-cost) - Twin Fang: Deal 170/255/385 (AP) magic damage to the current target and Wound them for 5 seconds. If they are already Wounded, deal 51/76.5/115.5 (AP) bonus magic damage. Wound: Reduce healing received by 33%

**Cho'Gath** (1-cost) - Feast: Deal 250/335/450 + 14% max HP (HP+AP) magic damage to the lowest Health enemy in range. If this kills them, devour them and permanently gain 30/35/40 maximum Health (doubled in Hyper Roll). (Current Bonus: +(live counter) Health)

**Irelia** (1-cost) - Defiant Dance: Enter a defensive stance and gain 350/400/450 (AP) Shield that rapidly decays over 3 seconds. When it expires, deal 70/100/150 (AP) magic damage + 30% of the damage absorbed to enemies around and in front of Irelia. Ionia Bonus: +25 Armor and Magic Resist

**Jhin** (1-cost) - Curtain Call: Take aim at the current target and deal 60/90/135 + 744% AD (AD+AP) physical damage to enemies in a line; each hit reduces damage by 40%. Ionia Bonus: +25% Attack Damage

**Kayle** (1-cost) - Divine Ascent: Passive: Gains new Passive effects as your Tactician levels up. Level 1: Attacks deal 35/50/75 (AP) bonus magic damage. Level 6: Every 3rd attack launches a wave that deals 25/30/45 (AP) magic damage and 40% Shreds enemies for 3 seconds. Level 9: Every attack launches a wave and they travel farther. Shred: Reduce Magic Resist

**Malzahar** (1-cost) - Call of the Void: Open two portals near the current target. Destroy 50% of Shields and deal 205/310/460 (AP) magic damage to all enemies caught between the portals.

**Maokai** (1-cost) - Sap Magic: Passive: Whenever an enemy uses their Ability, gain 5 Mana. Active: On next attack, heal for 220/260/300 (AP).

**Orianna** (1-cost) - Command: Protect: Grant the lowest Health ally 225/250/325 (AP) Shield for 4 seconds and empowers Orianna's next attack to deal 260/390/585 (AP) bonus magic damage.

**Poppy** (1-cost) - Steadfast Hammer: Gain 310/330/350 (AP) Shield for 3 seconds. Deal 140/210/315 (AP) magic damage to the current target. 4-Star Upgrade: Knock the target into the largest clump of enemies, dealing 33% of the target's max Health as magic damage to the target and 15% of that amount to the clump. Stun them for 0.25 second.

**Renekton** (1-cost) - Cull the Meek: Deal 180/270/400 (AP) magic damage to adjacent enemies. Heal 220/270/320 (AP) for the first enemy hit and another 30 (AP) for each additional one.

**Samira** (1-cost) - Flair: Shoot at the current target and deal 200/200/210% AD to the first enemy hit. Reduce their Armor by 10/15/20 (AP) for the rest of combat.

**Tristana** (1-cost) - Rapid Fire: Gain 70% (AP) Attack Speed for 4 seconds. For the duration, attacks explode on impact and deal 60% AD physical damage to enemies within 1 hex. 4-Star Upgrade: Every 10th attack deals 100% AD bonus physical damage and ricochets to the largest clump of enemies, dealing 50% AD physical damage to enemies within 1 hex.

**Viego** (1-cost) - Blade of the Ruined King: Deal 110/165/250 (AP) magic damage to the current target. For the rest of combat, Viego's attacks deal 20/30/45 (AP) bonus stacking magic damage. (Current Bonus: +(live counter) magic damage)

**Ashe** (2-cost) - Volley: Fire 8 arrows in a cone, each dealing 160/160/170% AD physical damage to the first enemy hit and Chilling them for 2 (AP) seconds. Chill: Reduce Attack Speed by 30%

**Galio** (2-cost) - Shield of Durand: Reduce damage taken by 25/25/35% and heal for 350/400/450 (AP) over 4 seconds. After, deal 200/300/450 (AP) magic damage to adjacent enemies.

**Jinx** (2-cost) - Fishbones!: Fire 5 rockets at random enemies within 2 hexes of the current target. Each rocket deals 10/15/25 + 150/150/160% AD (AD+AP) physical damage.

**Kassadin** (2-cost) - Force Pulse: Gain 250/300/400 (AP) Shield for 4 seconds. Deal 135/200/300 (AP) magic damage to enemies in a cone and Disarm them for 1.5/1.75/2 seconds. Disarm: Cannot move or attack, but CAN cast spells

**Kled** (2-cost) - Skaaaaaaarl!: Passive: Start combat with 30% max HP (HP) Shield. When it expires, dismount and gain 65/70/75% (AP) stacking Attack Speed for the rest of combat. Active: Remount and gain 30% max HP (HP) Shield. 4-Star Upgrade: Execute enemies under 25% Health.

**Sett** (2-cost) - Facebreaker: Grab an enemy from either side and slam them together, knocking them up. Deals 180/270/420 (AP) magic damage and Stuns them for 1.25/1.5/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%. Ionia Bonus: +200 maximum Health

**Soraka** (2-cost) - Astral Infusion: Heal the lowest health ally for 170/190/220 (AP). If the ally is below 50% Health, heal them for an additional [TooltipBonusHeal: unknown] (AP). Over the next 5 seconds, 5 stars hit the enemy closest to them. Each deals 125/185/290 (AP) magic damage.

**Swain** (2-cost) - Demonflare: Transform and gain 375/450/650 (AP) max Health for 5 seconds. While transformed, deal 35/50/75 (AP) magic damage to enemies within 2 hexes every second.

**Taliyah** (2-cost) - Seismic Shove: Passive: Whenever an enemy is knocked up or back by anything, throw a boulder towards them. It deals 125/190/290 (AP) magic damage to the first enemy hit. Active: Deal 160/240/370 (AP) magic damage to your target and knock them up, Stunning them for 2 seconds.

**Teemo** (2-cost) - Noxious Trap: Hurl an explosive heat-seeking mushroom at the current target. When it detonates, enemies within 1 hex are Wounded and dealt 260/390/585 (AP) magic damage over 3 seconds. 4-Star Upgrade: Increase the explosion radius by 1 hex. Wound: Reduce healing received by 33%

**Vi** (2-cost) - Blast Shield: Gain 325/350/400 (AP) Shield for 4 seconds. Deal 200% AD physical damage to enemies within 1 hex of the current target and 40% Sunder them for 4 seconds. Sunder: Reduce Armor

**Warwick** (2-cost) - Primal Howl: Passive: Attacks heal for 30/35/40 (AP). Active: Increase the Passive healing by 50% for 2.5 seconds. Then, Stun adjacent enemies for 1.25/1.5/2 seconds.

**Zed** (2-cost) - Living Shadow: Create a shadow at the furthest enemy within 2 + 0.5% stat#28 (Range) hexes. Zed and his shadow slash adjacent enemies, dealing 25/40/50 + 140/140/150% AD (AD+AP) physical damage. Ionia Bonus: +15% Critical Strike Chance and Critical Strike Damage

**Akshan** (3-cost) - Comeuppance: Lock on to the farthest enemy and unleash a hail of 6 shots toward them. Each deals 20/35/60 + 125% AD (AD+AP) physical damage to the first enemy hit.

**Darius** (3-cost) - Noxian Guillotine: Deal 100/120/150 + 350% AD (AD+AP) physical damage to the current target. If they die, immediately cast this again with 82/85/90% damage.

**Ekko** (3-cost) - Phase Dive: Heal 20% of damage taken in the last 4 seconds and deal 300/450/725 (AP) magic damage to the current target.

**Garen** (3-cost) - Judgement: Spin [SpinsTooltip: unknown] (AS) times every second for the next 4 (AP) seconds. Each spin deals 80/82/85% AD physical damage to adjacent enemies.

**Jayce** (3-cost) - Acceleration Blast: Grant 20/25/30% (AP) Attack Speed to Jayce and allies on his left and right for 3 seconds. Fire a blast at the current target that explodes on the first enemy hit. It deals 240/240/250% AD physical damage to enemies near the blast.

**Kalista** (3-cost) - Pierce and Rend: Passive: Attacks impale a spear in the target, which deals 18/27/45 (AP) true damage when removed. Kalista rips the spears out if it would kill the target. Active: Impale 6 spears in the current target.

**Karma** (3-cost) - Inner Flame: Fire a burst of energy that explodes upon impact, dealing 170/255/420 (AP) magic damage to adjacent enemies. Every third cast launches 3 bursts. Ionia Bonus: +20 Ability Power

**Katarina** (3-cost) - Voracity: Throw three daggers that land next to enemies within 1 + 0.5% stat#28 (Range) hexes. Then, teleport to each and deal 130/195/320 (AP) magic damage to adjacent enemies and Wound them for 6 seconds. Wound: Reduce healing received by 33%

**Lissandra** (3-cost) - Frozen Tomb: Stun the current target for 2 seconds and deal 160/240/400 (AP) magic damage to enemies within 2 hexes of them. They take 10% more damage for the next 4 seconds.

**Rek'Sai** (3-cost) - Furious Bite: Deal 315% AD physical damage to the current target. If they are below 70% Health, deal [TooltipExecuteDamage: unknown] (AD+AP) true damage instead. If this kills the target, rip through them 1 hex and heal for 10% max HP (AP+HP).

**Sona** (3-cost) - Crescendo: Send a wave of sound at the largest clump of enemies and deal 105/155/255 (AP) magic damage to enemies hit. Allies hit by the wave gain 35/40/45% Attack Speed for 5 seconds.

**Taric** (3-cost) - Radiance: Gain 550/650/750 (AP) Shield for 4 seconds. It absorbs damage received by adjacent allies.

**Vel'Koz** (3-cost) - Plasma Fission: Fire a plasma bolt towards the current target that deals 230/345/560 (AP) magic damage to the first enemy hit. It then splits in two, dealing 50% less damage whenever it passes through an enemy.

**Aphelios** (4-cost) - Moonlight Vigil: Fire a moon blast at the largest group of enemies that deals 200/200/550% AD physical damage to enemies within 2 hexes. For 7 seconds, equip 3 Chakram, plus 1 more for each enemy hit by the blast. Attacks deal 10/10/30% AD bonus physical damage for each Chakram equipped. Aphelios heals for 50% (AP) of damage dealt by Chakrams.

**Azir** (4-cost) - Arise!: Passive: Every third attack, a Sand Soldier deals 105/160/550 (AP) magic damage to their target. Active: Summon a Sand Soldier to attack your target. If Azir already has 3 Soldiers, they all immediately deal 73.5/112/385 (AP) magic damage instead.

**Gwen** (4-cost) - Snip Snip!: Dash around the current target and snip 3 times in a cone, each dealing 100/150/400 (AP) magic damage to enemies hit. Every 3 casts, gain 120/120/300 Armor and Magic Resist for 3/3/5 seconds.

**Jarvan IV** (4-cost) - Cataclysm: Leap on a nearby enemy. Deal 140/210/800 (AP) magic damage to enemies within 2 hexes and Stun them for 1.75/2.25/8 seconds.

**Kai'Sa** (4-cost) - Icathian Rain: Dash away from all enemies, then fire 15/15/25 missiles split across the 4 nearest enemies. Each missile deals 75/111/240 (AP) magic damage.

**Lux** (4-cost) - Torrent of Light: Channel a barrage of light at the current target that deals 735/1100/2750 (AP) magic damage over 3 seconds, reducing their Magic Resist by 15/15/40 every second. If they die, shift to a new target.

**Nasus** (4-cost) - Soul Eater: Steal 5/5/15% maximum Health, 10% Attack Damage, 5/5/30 Armor, and 5/5/30 Magic Resist from the nearest 4/5/9 enemies for 8 (AP) seconds. While empowered, every third attack deals 350/350/700% AD physical damage.

**Sejuani** (4-cost) - Fury of the North: Passive: Whenever an ally attacks a Chilled enemy, they deal 1.5/1.5/10% max HP (HP) bonus true damage. Active: Gain 600/700/2000 (AP) Shield for 4 seconds. Deal 160/240/1200 (AP) magic damage to nearby enemies and Chill them for 4 seconds. Chill: Reduce Attack Speed by 30%

**Shen** (4-cost) - Ki Barrier: Gain 350/450/2000 (AP) Shield and grant 225/275/1500 (AP) Shield to the 2 lowest-Health allies for 4 seconds. After shielding his allies, Shen's Shield refreshes with a burst, dealing 240/360/2500 (AP) magic damage to adjacent enemies. Ionia Bonus: +9% Damage Reduction

**Urgot** (4-cost) - Disdain: Passive: Whenever an enemy enters Attack Range, fire a blast from the leg facing that direction that deals 40/60/500 + 220% AD (AD+AP) physical damage. Each of Urgot's 6 legs has a 4/4/1 second cooldown. Active: Gain 350/450/1200 (AP) Shield for 5 seconds and dash behind the current target. Reset all leg cooldowns.

**Yasuo** (4-cost) - Last Breath: Send a whirlwind at the furthest enemy within 3 hexes, knocking up and Stunning all enemies hit. Dash to and slash the original target, dealing 55/85/300 + 500/500/1500% AD (AD+AP) physical damage to them. Slam them into the ground and deal 300/300/750% AD physical damage to enemies within 1 hex. Ionia Bonus: +15% Omnivamp Omnivamp: Heal for percentage of damage dealt

**Zeri** (4-cost) - Surge: Passive: Execute enemies below 12% (AP) Health. Active: For the next 8/8/15 seconds, attacks chain lightning to 3/3/5 additional enemies, dealing 45/45/100% AD physical damage.

**Aatrox** (5-cost) - World Ender: Transform for 12/12/30 seconds, gaining 15% (AP) Omnivamp and converting 80% of bonus Attack Speed to Attack Damage. While transformed, attacks deal 275/275/2500% AD physical damage in an area. Omnivamp: Heal for percentage of damage dealt

**Ahri** (5-cost) - Essence Thief: Steal essence from enemies around the current target, dealing 90/135/1000 (AP) magic damage and 20% Mana Reaving them. Every 3/3/1 casts, unleash a wave that deals 230/350/1888 (AP) magic damage to all enemies hit. This deals 33% more damage to enemies whose essence has been stolen. Ionia Bonus: +3 Mana per second Mana Reave: Increase max Mana until the next cast

**Bel'Veth** (5-cost) - Royal Maelstrom: Lash out at the lowest Health enemy in range 6/6/30 + 500% stat#3 (AS) times. Each lash deals 18/30/50 + 65% AD (AD+AP) physical damage and 2% of the target's max Health as true damage.

**Heimerdinger** (5-cost) - CH-3X Lightning Grenade: Hurl a grenade towards the largest group of enemies, dealing 150/225/3141 (AP) magic damage to enemies in a large area and Stunning them for 1.5/2/15 seconds. 4-Star Upgrade: YES YES YES, IT WORKS!!!

**K'Sante** (5-cost) - All Out Strike: Heal for [HealAmount: unknown] (HP), then knock up the current target before smashing them to the edge of the battlefield. Deal 250/400/1000 (AP) magic damage and Stun them for 2/2.5/10 seconds. Deal 250/400/1000 (AP) magic damage to enemies the target collides with and Stun them for 1/1.25/5 seconds. If the target can't be pushed back any further, knock them off the battlefield. Otherwise, K'Sante will chase after them.

**Ryze (Ryze)** (5-cost) - Realm Warp: Bandle City: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with [ModifiedBonusMana: unknown] (AP) bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

**Ryze (RyzeDemacia)** (5-cost) - Realm Warp: Demacia: Create a portal to Demacia that protects allies for 2/2/5 seconds. Allies within the zone gain 600/900/9999 (AP) Shield. If an ally leaves the safety of the portal, they are healed for 250/375/9999 (AP). When the portal expires, enemies within are dealt 300/450/999 (AP) magic damage.

**Ryze (RyzeBandleCity)** (5-cost) - Realm Warp: Bandle City: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with 25/70/200 (AP) bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

**Ryze (RyzeFreljord)** (5-cost) - Realm Warp: Freljord: Create a portal to the Freljord under a random group of enemies for 2 seconds. The freezing winds deal 300/450/3500 (AP) magic damage to enemies within two hexes over the duration and apply Permafrost, a permanent Chill. If enemies are already affected by Permafrost, they are instead Stunned for the duration. Chill: Reduce Attack Speed by 30%

**Ryze (RyzeIonia)** (5-cost) - Realm Warp: Ionia: Create a portal to Ionia in the center of the battlefield, summoning forth a Spirit Tree that affects the 5/7/20 closest units. Allies gain 30/40/300% Attack Speed for 6 seconds and are healed for 400/500/3000 (AP) over the duration. Enemies dance, dealing 175/275/1500 (AP) magic damage and Stunning them for 2/2.5/20 seconds.

**Ryze (RyzeNoxus)** (5-cost) - Realm Warp: Noxus: Create a portal to Noxus that spews out a torrent of weapons and armor, granting 3/4/10 completed items to random allies with open slots, and throwing 4/5/12 axes towards random enemies that deal 150/225/2000 (AP) magic damage each. If an ally would recieve an item while all slots are full, they instead gain a stacking 10% damage increase for the rest of combat.

**Ryze (RyzePiltover)** (5-cost) - Realm Warp: Piltover: Create a portal to Piltover that launches an enforcer grenade towards the largest clump of enemies, dealing 275/425/3000 (AP) magic damage to enemies within two hexes and creating a containment area for 3/4/8 seconds. Enemies are unable to leave the area, and 15/30/500% of the damage dealt to them is also dealt to all other enemies within.

**Ryze (RyzeShadowIsles)** (5-cost) - Realm Warp: Shadow Isles: Mark the 1/1/10 ally's soul(s) with the lowest current health to be reclaimed by the Shadow Isles. After they die, they return to the battlefield with 40/80/300% (AP) of their maximum Health. If all living allies are already marked, Ryze instead resurrects a deceased ally. Ryze cannot resurrect himself.

**Ryze (RyzeShurima)** (5-cost) - Realm Warp: Shurima: Create a portal to Shurima, summoning a sand tornado that travels across the battlefield, dealing 175/275/2500 (AP) magic damage to enemies hit and knocking them up, Stunning them for 1/1.25/3 second. The tornado has a chance to unearth buried treasures and gold as it travels.

**Ryze (RyzeTargon)** (5-cost) - Realm Warp: Targon: Create a portal to the skies above Mount Targon that beckons the current target. After a few seconds, they descend from the heavens as a meteor towards the largest group of enemies, dealing 18/30/100% (AP) of the initial target's maximum Health as magic damage. Enemies in the epicenter are briefly knocked up. If the target is the only enemy remaining, they never descend.

**Ryze (RyzeZaun)** (5-cost) - Realm Warp: Zaun: Create 5/8/25 portals to Zaun, causing hired guns to fire at nearby enemies. Deals 110/200/2006 (AP) magic damage and Wounds, 40% Shreds, and 40% Sunders for 6 seconds. The more gold you have, the more powerful the portals become. 25 Gold: +1 portal. Portals fire a rocket that deals 50% damage in a one-hex radius. 50 Gold: Double the portals. Summon 1 additional portals for every 20 Gold more. Wound: Reduce healing received by 50% Shred: Reduce Magic Resist Sunder: Reduce Armor

**Senna** (5-cost) - Dawning Shadow: Fire a massive beam at the furthest enemy, dealing 235/250/2000% AD physical damage to all enemies hit. Any allies hit gain 200/275/4000 (AP) Shield for 4/4/15 seconds.

**Sion** (5-cost) - Glory in Death: Passive: Upon death, reanimate with 100% Health, decaying at 20/13/0% maximum Health per second. Sion can no longer use his Ability, but is immune to crowd control and gains 150/150/1000% (AP) Attack Speed. Active: Charge towards the most enemies within 3 + 0.5% stat#28 (Range) hexes. Deal 225/235/500% AD physical damage to any enemies along the way and knock them up, Stunning them for 1.25/1.5/15 seconds.

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): Voidspawn, Target Dummy, Tome of Traits, Completed Item Anvil, Component Anvil, Ornn Item Anvil, Baron Nashor, Sun Disc, Rift Herald, Void Remora, Apex Turret, T-Hex, Noxian Banner, SLIME_Crab, Mercenary Chest, Void Totem.

## Traits (28)

### Bastion

Bastion champions gain Armor and Magic Resist. This is increased by 100% for the first 10 seconds of combat.

| Units | Effect |
|---|---|
| 2-3 | +25 Armor, +25 MR |
| 4-5 | +50 Armor, +50 MR |
| 6-7 | +95 Armor, +95 MR |
| 8 | +240 Armor, +240 MR |

Champions: Maokai (1), Poppy (1), Kassadin (2), Taric (3), Shen (4), K'Sante (5)

### Bruiser

Your units gain 100 Health. Bruisers gain additional maximum Health.

| Units | Effect |
|---|---|
| 2-3 | +10% max HP |
| 4-5 | +40% max HP |
| 6 | +70% max HP |

Champions: Cho'Gath (1), Renekton (1), Vi (2), Rek'Sai (3), Sejuani (4), Sion (5)

### Challenger

Challengers gain bonus Attack Speed. When their target dies, Challengers dash to a new target and increase their Attack Speed bonus by 50% for 2.5 seconds.

| Units | Effect |
|---|---|
| 2-3 | 30% AS |
| 4-5 | 60% AS |
| 6-7 | 95% AS |
| 8 | 140% AS |

Champions: Irelia (1), Samira (1), Warwick (2), Kalista (3), Kai'Sa (4), Yasuo (4)

### Darkin

When Aatrox or the holder of the Darkin Blade dies, the Darkin Blade is equipped to the nearest allied champion, granting them 350 Health and 15% Omnivamp. After being equipped to a champion for 4 seconds, Aatrox will revive upon their death instead.

| Units | Effect |
|---|---|
| 1 | BonusHealth 350; BonusOmnivamp 0.15; RespawnTimer 4 |

Champions: Aatrox (5)

### Deadeye

Innate: +1 Range Every 3 seconds, Deadeyes attack the enemy with the highest percent Health and deal bonus damage.

| Units | Effect |
|---|---|
| 2-3 | +20% damage |
| 4-5 | +65% damage |
| 6 | +150% damage |

Champions: Jhin (1), Ashe (2), Akshan (3), Aphelios (4), Urgot (4)

### Demacia

Your strongest Demacians become Elite and equip a random Radiant item each combat. Elites grant Armor and Magic Resist to themselves and adjacent allies, but this does not stack. Radiant items are stronger versions of full items.

| Units | Effect |
|---|---|
| 3-4 | 1 Elite, +5 Armor, +5 MR |
| 5-6 | 2 Elites, +30 Armor, +30 MR |
| 7-8 | 4 Elites, +40 Armor, +40 MR |
| 9 | 6 Elites, +100 Armor, +100 MR |

Champions: Kayle (1), Poppy (1), Galio (2), Garen (3), Sona (3), Jarvan IV (4), Lux (4)

### Empress

When Bel'Veth kills an enemy, they leave behind Void Coral that she will consume. The first Coral increases her maximum Health by 50%, while further Coral heals her 20% maximum Health. When a Void Coral is consumed, she deals 10% maximum Health as magic damage to enemies within 2 hexes.

| Units | Effect |
|---|---|
| 1 | HexRange 2; PercentBonusMaxHealth 0.5; PercentHealthHeal 0.2; PercentHealthMagicDamage 0.1 |

Champions: Bel'Veth (5)

### Freljord

After 8 seconds, an ice storm strikes the battlefield. Enemies take a percentage of their maximum Health as true damage and gain debuffs. for 10 seconds Sunder: Reduce Armor Shred: Reduce Magic Resist Mana Reave: Increase max Mana until the next cast

| Units | Effect |
|---|---|
| 2 | 5% HP, enemies are 40% Sundered and Shredded |
| 3 | 10% HP, enemies are also 30% Mana Reaved |
| 4 | 15% HP, enemies are also stunned for 1.5 seconds |

Champions: Ashe (2), Lissandra (3), Sejuani (4)

### Gunner

When Gunner champions attack, they gain bonus Attack Damage, up to 8 stacks.

| Units | Effect |
|---|---|
| 2-3 | 6% AD per stack |
| 4-5 | 14% AD per stack |
| 6 | 25% AD per stack |

Champions: Tristana (1), Jinx (2), Jayce (3), Zeri (4), Senna (5)

### Invoker

Every 3 seconds, your units gain Mana.

| Units | Effect |
|---|---|
| 2-3 | 5 Mana |
| 4-5 | Additional 15 to Invokers |
| 6 | 20 Mana to all, additional 15 to Invokers |

Champions: Cassiopeia (1), Galio (2), Soraka (2), Karma (3), Lissandra (3), Shen (4), Ryze (Ryze) (5), Ryze (RyzeDemacia) (5), Ryze (RyzeBandleCity) (5), Ryze (RyzeFreljord) (5), Ryze (RyzeIonia) (5), Ryze (RyzeNoxus) (5), Ryze (RyzePiltover) (5), Ryze (RyzeShadowIsles) (5), Ryze (RyzeShurima) (5), Ryze (RyzeTargon) (5), Ryze (RyzeZaun) (5)

### Ionia

Each Ionian has a unique bonus within their Ability. Every 4 seconds, a number of Ionians are enlightened to their spirit form, gaining 20 mana and doubling stat bonuses for 4 seconds.

| Units | Effect |
|---|---|
| 3-5 | 100% Ionian bonus, 1 enlightened |
| 6-8 | 200% Ionian bonus, 2 enlightened |
| 9 | 325% Ionian bonus, 3 enlightened |

Champions: Irelia (1), Jhin (1), Sett (2), Zed (2), Karma (3), Shen (4), Yasuo (4), Ahri (5)

### Juggernaut

Juggernaut champions take less damage as their Health decreases.

| Units | Effect |
|---|---|
| 2-3 | 15% to 25% damage reduction |
| 4-5 | 25% to 40% damage reduction |
| 6 | 35% to 50% damage reduction |

Champions: Sett (2), Warwick (2), Darius (3), Garen (3), Nasus (4), Aatrox (5)

### Multicaster

Multicasters cast their Ability multiple times. Bonus casts have 66% reduced effectiveness.

| Units | Effect |
|---|---|
| 2-3 | 1 extra cast |
| 4 | 2 extra casts |

Champions: Taliyah (2), Teemo (2), Sona (3), Vel'Koz (3)

### Noxus

Noxus champions gain Health, Ability Power and Attack Damage. This is increased by 5% for each different opponent that either you have conquered in combat or is dead. Conquered bonus: (live counter)% / 35%

| Units | Effect |
|---|---|
| 3-5 | 170 HP, 17 AP, 17% AD |
| 6-8 | 340 HP, 34 AP, 34% AD |
| 9 | 700 HP, 70 AP, 70% AD |

Champions: Cassiopeia (1), Samira (1), Kled (2), Swain (2), Darius (3), Katarina (3), Sion (5)

### Piltover

Gain the T-Hex. Every time you lose a player combat, the T-Hex gains Charges. Winning converts the Charges to Power for the T-Hex and grants you loot based on the number of Charges converted.

| Units | Effect |
|---|---|
| 3-5 | Gain the T-Hex. |
| 6 | Gain bonus loot every round. Each loss counts as 2 losses instead. |

Champions: Orianna (1), Vi (2), Ekko (3), Jayce (3), Heimerdinger (5)

### Redeemer

Whenever an ally gains a Shield, Senna grants them 6% stacking Attack Speed for the rest of combat.

| Units | Effect |
|---|---|
| 1 | AttackSpeed 0.06 |

Champions: Senna (5)

### Rogue

| Units | Effect |
|---|---|
| 2-3 | When a Rogue falls below 50% Health, they briefly become untargetable and dash to an enemy within 4 hexes (preferring enemy backline). |
| 4 | Additionally, a Rogue's first attack on an enemy bleeds them for 50% of their max Health as magic damage over 5 seconds. |

Champions: Viego (1), Zed (2), Ekko (3), Katarina (3)

### Shadow Isles

After dealing or receiving damage 10 times, Shadow Isles champions gain a Shield for 15 seconds and become Spectral for the rest of combat. Spectral champions gain Mana every second.

| Units | Effect |
|---|---|
| 2-3 | 45% HP Shield, 6 Mana |
| 4-5 | 80% HP Shield, 12 Mana |
| 6 | 115% HP Shield, 18 Mana |

Champions: Maokai (1), Viego (1), Kalista (3), Gwen (4), Senna (5)

### Shurima

Every 4 seconds, Shurimans heal 5% maximum Health. After 8 seconds, select Shurimans Ascend and gain 33% maximum Health and 45% Attack Speed.

| Units | Effect |
|---|---|
| 3-4 | The strongest Shuriman Ascends |
| 5-6 | All Shurimans Ascend |
| 7-8 | Instead, Ascend at combat start. +25% Ascension bonus |
| 9 | Ascend twice, at combat start and after 8 seconds. +25% Ascension bonus |

Champions: Cassiopeia (1), Renekton (1), Taliyah (2), Akshan (3), Azir (4), Nasus (4), K'Sante (5)

### Slayer

Slayers gain 12% Omnivamp. Slayers deal bonus damage, doubled against units below 66% Health.

| Units | Effect |
|---|---|
| 2 | +5% bonus damage |
| 3 | +10% bonus damage |
| 4 | +20% bonus damage |
| 5 | +30% bonus damage |
| 6 | +40% bonus damage |

Champions: Kayle (1), Kled (2), Zed (2), Gwen (4), Aatrox (5)

### Sorcerer

Sorcerers gain bonus Ability Power. When an enemy dies after being damaged by a Sorcerer, they deal a percentage of that enemy's maximum Health to another enemy.

| Units | Effect |
|---|---|
| 2-3 | 25 AP; 7% max HP |
| 4-5 | 50 AP; 10% max HP |
| 6-7 | 80 AP; 12% max HP to 2 enemies |
| 8 | 120 AP; 20% max HP to 2 enemies |

Champions: Malzahar (1), Orianna (1), Swain (2), Taric (3), Vel'Koz (3), Lux (4), Ahri (5)

### Strategist

Combat Start: Allies in the front 2 rows gain a Shield for 15 seconds. Allies in the back 2 rows gain Ability Power.

| Units | Effect |
|---|---|
| 2 | 250 Shield; 15 AP |
| 3 | 400 Shield; 25 AP |
| 4 | 600 Shield; 40 AP |
| 5 | 900 Shield; 60 AP |

Champions: Swain (2), Teemo (2), Azir (4), Jarvan IV (4)

### Targon

All of your units' healing and shielding is improved.

| Units | Effect |
|---|---|
| 2 | 18% increased healing and shielding |
| 3 | 35% increased healing and shielding |
| 4 | 60% increased healing and shielding |

Champions: Soraka (2), Taric (3), Aphelios (4)

### Technogenius

Gain a placeable Apex Turret with 3 upgrade slots. Upgrades to the Apex Turret will show up in your shop for 6 gold. The Apex Turret shares Heimerdinger's Attack Speed and Ability Power. You may only have 1 Apex Turret.

| Units | Effect |
|---|---|
| 1 | GoldCost 6 |

Champions: Heimerdinger (5)

### Void

Get a placeable void egg. At the start of combat, it hatches into an unspeakable horror and knocks up adjacent enemies. Each Void star level increases the horror's Health and Ability Power by 25%.

| Units | Effect |
|---|---|
| 3-5 | Void Remora |
| 6-7 | Rift Herald |
| 8 | Baron Nashor |

Champions: Cho'Gath (1), Malzahar (1), Kassadin (2), Rek'Sai (3), Vel'Koz (3), Kai'Sa (4), Bel'Veth (5)

### Wanderer

Ryze's spell changes depending on the Region Portal players voted for at the start of the game.

Champions: Ryze (Ryze) (5), Ryze (RyzeDemacia) (5), Ryze (RyzeBandleCity) (5), Ryze (RyzeFreljord) (5), Ryze (RyzeIonia) (5), Ryze (RyzeNoxus) (5), Ryze (RyzePiltover) (5), Ryze (RyzeShadowIsles) (5), Ryze (RyzeShurima) (5), Ryze (RyzeTargon) (5), Ryze (RyzeZaun) (5)

### Yordle

Your units gain 10% Attack Speed per star level. If you have three 3-star champions, your Yordles can become 4-star, which gives their Ability a wacky upgrade! Ties for most items are broken by the Yordle fielded most recently.

| Units | Effect |
|---|---|
| 3-4 | Your strongest 3-star Yordle |
| 5 | Two 3-star Yordles |

Champions: Poppy (1), Tristana (1), Kled (2), Teemo (2), Heimerdinger (5)

### Zaun

Zaun champions create random chem-mods that only they can use. Champions can be modded once, and mods can be removed by benching or selling the champion.

| Units | Effect |
|---|---|
| 2-3 | Gain 1 chem-mod |
| 4-5 | Gain 2 chem-mods |
| 6 | Gain 3 chem-mods and Overcharge them |

Champions: Jinx (2), Warwick (2), Ekko (3), Urgot (4), Zeri (4)

## Items

### Components (9)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| B.F. Sword |  | +10% AD | AD +10% Attack Damage |
| Chain Vest |  | +20 Armor | Armor +20 Armor |
| Giant's Belt |  | +150 HP | HP +150 Health |
| Needlessly Large Rod |  | +10 AP | AP +10 Ability Power |
| Negatron Cloak |  | +20 MR | MR +20 Magic Resist |
| Recurve Bow |  | +10% AS | AS +10% Attack Speed |
| Sparring Gloves |  | +20% Crit | +20 Critical Strike Chance |
| Spatula |  |  | It must do something... |
| Tear of the Goddess |  | +15 Mana | Mana +15 Mana |

### Completed items (37)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Archangel's Staff | Needlessly Large Rod + Tear of the Goddess | +20 AP, +30 Mana | Combat start: Grant AP 20 Ability Power every 5 seconds. |
| Bloodthirster | B.F. Sword + Negatron Cloak | +20% AD, +20 MR | Grant 20% Omnivamp. Once per combat at 40% Health, gain a 25% maximum Health shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | +10 AP, +40 Mana | Abilities cost Mana 10 less Mana to cast. If the holder gets at least 1 takedown within 3 seconds of casting, gain Mana 10 Mana. [Unique - only 1 per champion] |
| Bramble Vest | Chain Vest + Chain Vest | +70 Armor | Negates 75% bonus damage from incoming critical strikes. When struck by an attack, deal 75/100/150 star magic damage to all nearby enemies (once every 2 seconds). |
| Chalice of Power | Negatron Cloak + Tear of the Goddess | +45 MR, +45 Mana | Combat start: Grant AP 20 Ability Power to the holder and allies within 1 hex in the same row. |
| Deathblade | B.F. Sword + B.F. Sword | +66% AD | Perfect peace and calm for the holder - and all who face it. |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | +75 MR | Every 2 seconds, regenerate 5% maximum Health. |
| Edge of Night | B.F. Sword + Chain Vest | +10% AD, +20 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, gain AS 15% bonus Attack Speed. [Unique - only 1 per champion] |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | +30 Armor, +30 MR | Grants Armor 15 Armor and MR 15 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | B.F. Sword + Recurve Bow | +30% AD, +20 AP, +10% AS | Abilities and attacks deal 25% more damage to enemies with more than 1600 maximum Health. |
| Guardbreaker | Giant's Belt + Sparring Gloves | +20% AD, +20 AP, +150 HP, +20% Crit | After damaging a shielded enemy, Abilities and attacks deal 25% more damage for 3 seconds. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | +10 AP, +18% AS | Attacks grant AS 4% bonus Attack Speed. This effect stacks. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | +15 Mana, +20% Crit | Grant 2 effects:AD +15% Attack Damage and AP +15 Ability Power.15% Omnivamp. Each round, randomly double 1 of these effects. Omnivamp: heal for some of damage dealt |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | +10% AD, +10 AP, +22 Omnivamp | Grant 22% Omnivamp, which also heals the lowest percent Health ally. Healing: (live counter) Omnivamp: heal for some of damage dealt |
| Infinity Edge | B.F. Sword + Sparring Gloves | +35% AD, +35% Crit | Damage from an Ability can critically strike. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | +10 AP, +20 MR | 50% Shred enemies within 2 hexes. When enemies cast an Ability, they are also zapped for magic damage equal to 160% of their maximum Mana. Shred: reduce Magic Resist |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | +30 AP, +35% Crit | Damage from an Ability can critically strike. |
| Last Whisper | Recurve Bow + Sparring Gloves | +10% AD, +10% AS, +20% Crit | Any physical damage dealt 50% Sunders the target for 3 seconds. This effect does not stack. [Unique - only 1 per champion] Sunder: reduce Armor |
| Locket of the Iron Solari | Needlessly Large Rod + Chain Vest | +10 AP, +20 Armor | Combat start: Shields the holder and allies within 1 hexes in the same row for 275/325/375 star damage for 8 seconds. |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | +25 AP, +150 HP | Damage from the holder's Ability 1% Burns and 33% Wounds enemies for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Protector's Vow | Tear of the Goddess + Chain Vest | +20 Armor, +45 Mana | Once per combat: At 40% Health, gain a 25% maximum Health shield that lasts up to 5 seconds and Armor 25 Armor and MR 25 Magic Resist for the rest of combat. |
| Quicksilver | Sparring Gloves + Negatron Cloak | +30% AS, +30 MR, +20% Crit | Combat start: Grant immunity to crowd control for 18 seconds. [Unique - only 1 per champion] |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | +70 AP | This humble hat can help you make, or unmake, the world itself. |
| Rapid Firecannon | Recurve Bow + Recurve Bow | +55% AS | Grant Range 1 bonus Attack Range and your attacks cannot miss. |
| Redemption | Tear of the Goddess + Giant's Belt | +150 HP, +15 Mana | Heal allies within 1 hex for 15% of their missing Health every 5 seconds. They also take 25% less multi-target damage for 5 seconds (damage reduction does not stack). Healing: (live counter) Multi-target damage blocked: (live counter) ​​[Aura item] |
| Runaan's Hurricane | Negatron Cloak + Recurve Bow | +20% AD, +10% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 50% Attack Damage AD as physical damage. |
| Shroud of Stillness | Chain Vest + Sparring Gloves | +20 Armor, +250 HP, +20% Crit | Combat start: Shoot a beam that 30% Mana Reaves enemies. [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast |
| Spear of Shojin | B.F. Sword + Tear of the Goddess | +10% AD, +25 AP, +30 Mana | Attacks restore Mana 5 additional Mana. |
| Statikk Shiv | Recurve Bow + Tear of the Goddess | +15 AP, +10% AS, +15 Mana | Every 3rd attack unleashes chain lightning that bounces to 4 enemies, dealing 30 magic damage and 50% Shredding them for 5 seconds. Shred: reduce Magic Resist |
| Sunfire Cape | Chain Vest + Giant's Belt | +20 Armor, +250 HP | Every 2 seconds, an enemy within 2 hexes is 1% Burned and 33% Wounded for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Tactician's Crown | Spatula + Spatula |  | Your team gains +1 maximum team size. |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | +40% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] |
| Titan's Resolve | Chain Vest + Recurve Bow | +10% AS, +20 Armor | Grants AD 2% Attack Damage and AP 2 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, grant Armor 25 Armor and MR 25 Magic Resist. |
| Warmog's Armor | Giant's Belt + Giant's Belt | +800 HP | Tough as teak, strong as oak, and eternal as petricite. |
| Zeke's Herald | B.F. Sword + Giant's Belt | +25% AD, +250 HP | Combat start: Grant AS 15% Attack Speed to the holder and allies within 1 hex in the same row. |
| Zephyr | Negatron Cloak + Giant's Belt | +15% AS, +20 MR, +150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| Zz'Rot Portal | Recurve Bow + Giant's Belt | +10% AS, +150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. The Voidspawn has massively reduced effectiveness when created by a summoned unit. Taunt: enemies that are able and in range must attack the taunter |

### Emblems (21)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Bastion Emblem |  | +150 HP | The holder gains the Bastion trait. [Unique - only 1 per champion.] |
| Bruiser Emblem |  | +150 HP | The holder gains the Bruiser trait. [Unique - only 1 per champion.] |
| Challenger Emblem | Spatula + Recurve Bow | +10% AS | The holder gains the Challenger trait. [Unique - only 1 per champion.] |
| Deadeye Emblem |  | +150 HP | The holder gains the Deadeye trait. [Unique - only 1 per champion] |
| Demacia Emblem | Negatron Cloak + Spatula | +20 MR | The holder gains the Demacia trait. [Unique - only 1 per champion.] |
| Freljord Emblem |  | +150 HP | The holder gains the Freljord trait. [Unique - only 1 per champion] |
| Gunner Emblem |  | +150 HP | The holder gains the Gunner trait. [Unique - only 1 per champion.] |
| Invoker Emblem |  | +150 HP | The holder gains the Invoker trait. [Unique - only 1 per champion] |
| Ionia Emblem | Spatula + B.F. Sword | +10% AD | The holder gains the Ionia trait. Ionia Bonus: 33% Attack Speed. [Unique - only 1 per champion] |
| Juggernaut Emblem | Spatula + Chain Vest | +20 Armor | The holder gains the Juggernaut trait. [Unique - only 1 per champion.] |
| Noxus Emblem | Spatula + Giant's Belt | +150 HP | The holder gains the Noxus Trait. [Unique - only 1 per champion] |
| Piltover Emblem |  | +150 HP | The holder gains the Piltover trait. [Unique - only 1 per champion] |
| Rogue Emblem |  | +150 HP | The holder gains the Rogue trait. [Unique - only 1 per champion.] |
| Shadow Isles Emblem |  | +150 HP | The holder gains the Shadow Isles trait. [Unique - only 1 per champion.] |
| Shurima Emblem | Spatula + Needlessly Large Rod | +10 AP | The holder gains the Shurima trait. [Unique - only 1 per champion] |
| Slayer Emblem | Sparring Gloves + Spatula | +20% Crit | The holder gains the Slayer trait. [Unique - only 1 per champion] |
| Sorcerer Emblem | Spatula + Tear of the Goddess | +15 Mana | The holder gains the Sorcerer trait. [Unique - only 1 per champion.] |
| Strategist Emblem |  | +150 HP | The holder gains the Strategist trait. [Unique - only 1 per champion] |
| Targon Emblem |  | +150 HP | The holder gains the Targon trait. [Unique - only 1 per champion.] |
| Void Emblem |  | +150 HP | The holder gains the Void trait. [Unique - only 1 per champion] |
| Zaun Emblem |  | +150 HP | The holder gains the Zaun trait. [Unique - only 1 per champion] |

### Artifacts (Ornn) (15)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Visage |  | +25 MR, +500 HP | Regenerate 2.5% maximum Health each second. |
| Blacksmith's Gloves |  |  | Each round: Equip 2 random Ornn Artifacts. [Consumes 3 item slots.] |
| Death's Defiance |  | +10% AD, +25% AS, +30 Armor | Grant 25% Omnivamp. 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] Omnivamp: heal for some of damage dealt |
| Deathfire Grasp |  | +50 AP, +30 Mana | Combat Start: Fire a blast of energy at the current target, marking them and dealing 20% of their maximum Health as magic damage. For the next 10 seconds, the holder deals 35% increased damage to the marked enemy and 25% increased damage to other enemies. [Unique - only 1 per champion] |
| Eternal Winter |  | +25 Armor, +200 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead, but only once every 15 seconds. [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Gold Collector |  | +20% AD, +30% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 40% chance to drop goldCoins 1 gold. [Unique - only 1 per champion] |
| Hullcrusher |  | +30% AS, +40 Armor, +40 MR | Combat start: If there are no adjacent allies, gain an additional HP 600 Health. |
| Infinity Force |  | +25% AD, +25 AP, +25% AS, +25 Armor, +25 MR, +250 HP, +25 Mana | Tons of EVERYTHING! |
| Manazane |  | +15% AD, +15% AS, +30 Mana | Once per combat: After casting an Ability, restore Mana 120 Mana over 5 seconds. [Unique - only 1 per champion] |
| Obsidian Cleaver |  | +25% AD, +250 HP | Attacks and Abilities 50% Shred and 50% Sunder enemies for 5 seconds. [Unique - only 1 per champion] Shred: reduce Magic Resist Sunder: reduce Armor |
| Randuin's Omen |  | +150 HP | Combat start: Grant Armor 40 Armor and MR 40 Magic Resistance to the holder and their adjacent allies. |
| Rocket-Propelled Fist |  | +25 Armor, +150 HP, +15 Mana | Combat start: The farthest enemy is pulled into melee range and Stunned for 1.5 seconds. Allies within range will prioritize attacking that enemy. [Unique - only 1 per champion] Stun: cannot move, attack, or cast Abilities |
| Sniper's Focus |  | +15% AD, +15 AP, +15% AS | Grant Range 1 bonus Attack Range. Attacks and Abilities deal 10% increased damage for each hex between the holder and their target. |
| Trickster's Glass |  | +10% AS, +10 Armor, +10 MR, +15% Crit | Summon a clone with 70% maximum Health and 30% increased maximum Mana. You cannot equip items to the clone. The clone benefits from active traits [Unique - only 1 per champion] |
| Zhonya's Paradox |  | +45 AP, +30 Armor, +30 MR | Once per combat: At 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items (36)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Absolution |  | +300 HP, +15 Mana | Heal allies within 2 hexes for 25% of their missing Health every 5 seconds. They also take 25% less multi-target damage for 5 seconds (damage reduction does not stack). Healing: (live counter) Multi-target damage blocked: (live counter) ​​[Aura item] |
| Blessed Bloodthirster |  | +40% AD, +20 MR | Grant 40% Omnivamp. Once per combat: At 40% Health, gain a 40% maximum Health shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Blessing |  | +50 AP, +40 Mana | Abilities cost Mana 10 less Mana to cast. If the holder gets at least 1 takedown within 3 seconds of casting, gain Mana 20 Mana. [Unique - only 1 per champion] |
| Brink of Dawn |  | +30% AD, +30 Armor | Once per combat: At 60% Health and again at 30% Health, briefly become untargetable and shed negative effects. Then, gain AS 50% bonus Attack Speed. This effect stacks. [Unique - only 1 per champion] |
| Bulwark's Oath |  | +20 Armor, +60 Mana | Once per combat: At 40% Health, gain a 50% maximum Health shield that lasts up to 10 seconds and Armor 70 Armor and MR 70 Magic Resist for the rest of combat. |
| Chalice of Charity |  | +45 MR, +45 Mana | Combat start: Grant AP 45 Ability Power and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​Omnivamp: heal for some of damage dealt |
| Covalent Spark |  | +10 AP, +20 MR | 50% Shred enemies within 3 hexes. When enemies cast an Ability, they are also zapped for magic damage equal to 250% of their maximum Mana. Also, regenerate 1% maximum Health per second. Shred: reduce Magic Resist |
| Demonslayer |  | +50% AD, +40 AP, +10% AS | Abilities and attacks deal 60% more damage to enemies with more than 1600 maximum Health. |
| Dragon's Will |  | +150 MR | Every 1.5 seconds, regenerate 10% maximum Health. |
| Dvarapala Stoneplate |  | +30 Armor, +30 MR | Grants Armor 30 Armor and MR 30 Magic Resist for each enemy targeting the holder. Also, regenerate 2% maximum Health each second. |
| Eternal Whisper |  | +35% AD, +10% AS, +55% Crit | Any physical damage dealt 50% Sunders the target for the rest of combat. This effect does not stack. [Unique - only 1 per champion] Sunder: reduce Armor |
| Fist of Fairness |  | +15 Mana, +20% Crit | Grant 2 effects:AD 50% Attack Damage and AP 50 Ability Power.30% Omnivamp. Omnivamp: heal for some of damage dealt |
| Glamorous Gauntlet |  | +55 AP, +75% Crit | Damage from an Ability can critically strike. |
| Guinsoo's Reckoning |  | +10 AP, +20% AS | Attacks grant AS 8% bonus Attack Speed. This effect stacks. |
| Hextech Lifeblade |  | +10% AD, +40 AP, +35 Omnivamp | Grant 35% Omnivamp, which also heals the lowest percent Health ally. Healing: (live counter) Omnivamp: heal for some of damage dealt |
| Locket of Targon Prime |  | +10 AP, +20 Armor | Combat start: Shields the holder and allies within 3 hexes in the same row for 275/325/375 star damage for 60 seconds. Your team gains HP 200 Health. |
| Luminous Deathblade |  | +1.25 AD | It glows in the presence of enemies. Or friends. Or anything alive, really. |
| Mistral |  | +20% AS, +20 MR, +300 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 8 seconds. Your team gains AS 15% Attack Speed. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| More More-ellonomicon |  | +40 AP, +150 HP | Damage from the holder's Ability 3% Burns and 33% Wounds enemies for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Quickestsilver |  | +50% AS, +40 MR, +40% Crit | Combat start: Grant immunity to crowd control for 30 seconds. [Unique - only 1 per champion] |
| Rabadon's Ascended Deathcap |  | +140 AP | It's witnessed - and unleashed - miracles and calamities both. |
| Rapid Lightcannon |  | +125% AS | Grant Range 2 bonus Attack Range and your attacks cannot miss. |
| Rascal's Gloves |  | +40% Crit | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] |
| Rosethorn Vest |  | +120 Armor | Negates all bonus damage from incoming critical strikes. When struck by an attack, deal 125/175/225 star magic damage to all nearby enemies (once every 2 seconds). |
| Runaan's Tempest |  | +30% AD, +20% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 100% Attack Damage AD as physical damage. |
| Shroud of Reverence |  | +20 Armor, +400 HP, +20% Crit | Combat start: Shoot a wider beam that 50% Mana Reaves enemies. Your team gains Mana 25 starting Mana. [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast |
| Spear of Hirana |  | +25% AD, +45 AP, +30 Mana | Attacks restore Mana 10 additional Mana. |
| Statikk's Favor |  | +40 AP, +10% AS, +15 Mana | Every 3rd attack unleashes chain lightning that bounces to 8 enemies, dealing 65 magic damage and 50% Shredding them for 5 seconds. Shred: reduce Magic Resist |
| Sunlight Cape |  | +20 Armor, +300 HP | Every 1.5 seconds, an enemy within 3 hexes is 2% Burned and 33% Wounded for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: reduces healing received |
| Titan's Vow |  | +20% AS, +30 Armor | Grant AD 3% Attack Damage and AP 3 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain Armor 60 Armor and MR 60 Magic Resist. |
| Urf-Angel's Staff |  | +50 AP, +30 Mana | Combat start: Gain AP 30 Ability Power every 4 seconds. |
| Warmog's Pride |  | +1300 HP | Regenerate 1% maximum Health per second. |
| Willbreaker |  | +40% AD, +40 AP, +150 HP, +20% Crit | After damaging a shielded enemy, Abilities and attacks deal 50% more damage for 3 seconds. |
| Zeke's Harmony |  | +35% AD, +350 HP | Combat start: Grant AS 35% Attack Speed and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​Omnivamp: heal for some of damage dealt |
| Zenith Edge |  | +65% AD, +75% Crit | Damage from an Ability can critically strike. |
| Zz'Rot's Invitation |  | +10% AS, +150 HP | Combat start: A Radiant Voidmother with bonus Health and Attack Damage arises and Taunts nearby enemies. Taunt: enemies that are able and in range must attack the taunter |

### Set-specific and other items (16)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Baron Nashor's Head |  |  | The terror from the Void was no match for the ingenuity, firepower, and weapons of dubious legality from Piltover. |
| Charges |  |  | Start with [TooltipStartingCharge: unknown] Charge. Gain Charges each time you lose. The larger your loss streak, the more Charges you gain per loss. When you win, all Charges are converted to Power for the T-Hex. Current loss streak: (live counter) (doubled in Hyper Roll) Charges granted next loss: +(live counter) Current loot value on victory: (live counter) gold [This does not count as an item.] |
| Crown of Demacia |  | +40% AS, +400 HP | If the holder begins combat in the front 2 rows, regenerate 2-14% maximum Health every 2 seconds (based on stage). If they start in the back 2 rows, gain 1-7% AD Attack Damage and AP Ability Power every 2 seconds instead. If the holder of this item dies, you instantly lose the fight. You can remove this item by benching the holder. |
| Determined Investor |  | +150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and goldCoins 5 gold. |
| Diamond Hands |  | +30 AP, +300 HP | Once per combat: At 50% Health, become invulnerable for 1.5 seconds and grant goldCoins 2 gold. |
| Draven's Axe |  | +15% AD, +10% AS | Gain AD 1% Attack Damage per goldCoins gold in your bank (up to goldCoins 60 gold). Attacks grant 1 stack, up to 100 times. At full stacks, grant goldCoins 9 gold. Gold generated this game: (live counter) |
| Gambler's Blade |  | +20 AP, +15% AS | Grant AS 1% bonus Attack Speed per goldCoins gold in your bank (up to goldCoins 60 gold). Each attack has a 6% chance to drop goldCoins 1 gold. Gold generated this game: (live counter) |
| Goldinator |  |  | Apex Turret's damage executes enemies under 12% Health. Executions have a 100% chance to turn enemies into gold. 1 Count: 1 gold 2 Count: 2 gold 3 Count: 4 gold |
| Goldmancer's Staff |  | +20 AP | Grant AP 1 Ability Power per goldCoins gold in your bank (up to goldCoins 60 gold) and a 40% chance to drop goldCoins 2 gold on enemy kill. Gold generated this game: (live counter) |
| Mechano-Swarm |  |  | Apex Turret fires rockets in a wide cone every 3 attacks. Each rocket deals magic damage, Burns, and 33% Wounds for 10 seconds. 1 Count: 50 magic damage; 1% Burn 2 Count: 80 magic damage; 2% Burn 3 Count: 120 magic damage; 3% Burn Burn: Deal a percentage of the target's max Health as true damage every second Wound: Reduce healing |
| Mogul's Mail |  | +150 HP | Grants Armor 1 Armor, MR 1 Magic Resist, and HP 6 Health when taking damage, stacking up to 40 times. At full stacks, grant goldCoins 2 gold. Gold generated this game: (live counter) |
| Needlessly Big Gem |  | +500 HP | Once per combat: If the holder is alive after 15 seconds of combat, grant goldCoins 1 gold per 2 living allies, and each living ally deals 1% more damage per goldCoins gold in your bank (up to goldCoins 60 gold). Gold generated this game: (live counter) |
| Power |  |  | Gain Power by winning combats after gaining Charges. More Power gives more stats. [This does not count as an item.] |
| Repair-o-matic |  |  | When the Apex Turret would die, it instead becomes untargetable and repairs itself over 7 seconds if Heimerdinger is still alive. The Apex Turret gains bonus Armor and Magic Resist. 1 Count: 10 Armor and Magic Resist 2 Count: 50 Armor and Magic Resist 3 Count: 100 Armor and Magic Resist |
| Shrink Module |  |  | Apex Turret's damage shrinks all enemies hit, Shredding and Sundering them for 6 seconds. 1 Count: 40% Shred and Sunder 2 Count: 50% Shred and Sunder 3 Count: 70% Shred and Sunder Shred: Reduce Magic Resist Sunder: Reduce Armor |
| The Darkin Blade |  | +100% AD, +100 AP, +1000 HP | If the holder of this item gets a takedown, they will be consumed and Aatrox will be reborn! |

### Consumables (17)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Adaptive Implant |  |  | Mod: While above 50% Health, gain 30% Attack Damage and Ability Power. While below, gain 30% Omnivamp. Overcharge: Always gain both effects, and their power is increased by 60%. Omnivamp: Heal for percentage of damage dealt |
| Champion Duplicator |  |  | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Eagle's Antimagic |  |  | Your team gains temporary magic resist. |
| Eagle's Armor |  |  | Your team gains temporary armor. |
| Golden Item Remover |  |  | Use on a champion to unequip all items as many times as you want |
| Hextech Exoskeleton |  |  | Mod: Gain 30 Armor and Magic Resistance. Every 5 seconds, cleanse all negative effects and heal for 12% maximum Health. Overcharge: Each time the holder heals, deal 150% of the heal amount as magic damage to adjacent enemies. |
| Lesser Champion Duplicator |  |  | Use on a champion of Tier 3 or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Loaded Dice |  |  | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Masterwork Upgrade |  |  | Use on a unit to open an armory containing Radiant versions of that unit's craftable completed items. Upgrade the item you choose to its Radiant equivalent, and unequip it. |
| Reforger |  |  | Use on a champion to unequip all items and randomly transform them into different new ones. [Same quality: Reforging will grant a new item of a similar type and quality. Cannot be used on champions in combat, or on Tactician's Crown. Consumable - This item disappears when used.] |
| Robotic Arm |  |  | Mod: Attacks have a 40% chance to trigger 1 additional attack. Overcharge: Attacks deal 50% Attack Damage as bonus true damage. |
| Scroll of Knowledge |  |  | Use on a unit to open a special armory that offers every available emblem that matches that unit's inherent traits. |
| Shimmer Injector |  |  | Mod: Gain 25% Attack Speed. The first time you would be reduced to 1 Health, increase the Attack Speed bonus by 100% and heal for 35% maximum Health. Overcharge: Instead increase the Attack Speed bonus by 400% and heal for 100% maximum Health. |
| Target Dummy |  |  | Use on a champion to spawn a movable target dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |
| Unstable Chemtank |  |  | Mod: Gain 33% maximum Health and explode on death, dealing 30% of your maximum Health as magic damage to enemies within 2 hexes. Overcharge: Explosion deals 35% of your maximum Health as magic damage to enemies within 3 hexes instead. |
| Virulent Bioware |  |  | Mod: Release a virulent plague whenever you deal Ability damage, causing enemies within 1 hex of the target to take 20% more damage for 4 seconds. Overcharge: Plague duration is 10 seconds and plague deals 5% maximum Health magic damage per second. |

## Augments (330)

Tier comes from the augment icon file name (I/1 = Silver, II/2 = Gold, III/3 = Prismatic); "unknown" where the name carries no marker. The list is the set's augment pool as stored in the source data and can include entries that were disabled or rotated during the set.

| Name | Tier | Effect |
|---|---|---|
| A Cut Above | Gold | Gain a Deathblade. Champions holding Deathblade have a 33% chance to drop 1 gold on kill. |
| Adrenaline Rush | Gold | Your Juggernauts deal 10% more damage, increased to 25% for the rest of combat when they fall below 50% Health. Gain 2 Juggernauts. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| All Natural I | Silver | Your champions with no items equipped gain 120 Health and heal for 1% of their max Health each second. |
| All Natural II | Gold | Your champions with no items equipped gain 150 Health and heal for 2% of their max Health each second. |
| All That Shimmers | Gold | Open an Armory and choose 1 of 3 Shimmerscale items. Shimmerscale items aid gold generation. |
| Ancient Archives I | Gold | Gain 1 Tome of Traits and 3 gold. |
| Ancient Archives I | Gold | Gain 1 Tome of Traits and 3 gold. |
| Ancient Archives II | Prismatic | Gain 2 Tome of Traits and 5 gold. |
| Ancient Archives II | Prismatic | Gain 2 Tome of Traits and 5 gold. |
| Army Building | Silver | Gain a Lesser Champion Duplicator. This happens a second time after 7 player combats. |
| Ascension | Gold | After 15 seconds of combat, your units deal 50% more damage. |
| Balanced Budget | Silver | At the start of the next 4 rounds, gain 4 gold. |
| Balanced Budget II | Gold | At the start of the next 4 rounds, gain 6 gold. |
| Balanced Budget III | Prismatic | At the start of the next 4 rounds, gain 11 gold. |
| Bastion Crest | Gold | Gain a Bastion Emblem and a Kassadin. |
| Bastion Crown | Prismatic | Gain a Bastion Emblem, a Sunfire Cape, and a Taric. |
| Bastion Heart | Silver | Your team counts as having 1 additional Bastion. Gain a Kassadin. |
| Battle Ready | Silver | Your team deals 3% more damage and takes 3% less damage. |
| Battle Ready II | Gold | Your team deals 6% more damage and takes 6% less damage. |
| Battle Ready III | Prismatic | Your team deals 8% more damage and takes 8% less damage. |
| Big Grab Bag | Gold | Gain 2 random components, 8 gold, and a Reforger. |
| Binary Airdrop | Prismatic | Your champions equipped with 2 items temporarily gain a random completed item at the start of combat. Gain 1 random item components. |
| Birthday Present | Prismatic | Gain 2 gold and a 2-star champion every time you level up. The champion's tier is your level minus 4 (min: Tier 1). |
| Blinding Speed | Prismatic | Gain a Rapid Firecannon, a Guinsoo's Rageblade, and a Magnetic Remover. |
| Blood Money | Silver | Gain 1 gold per 3 Health your Tactician loses. |
| Branching Out | Silver | Gain a random Emblem and a Reforger. |
| Branching Out | Silver | Gain a random Emblem and a Reforger. |
| Bronze Ticket | Silver | Every 4 shop refreshes, gain a free refresh. |
| Bronze Ticket | Silver | Every 4 shop refreshes, gain a free refresh. |
| Bruiser Crest | Gold | Gain a Bruiser Emblem and a Vi. |
| Bruiser Crown | Prismatic | Gain a Bruiser Emblem, a Redemption, and a Rek'Sai. |
| Bruiser Heart | Silver | Your team counts as having 1 additional Bruiser. Gain a Vi. |
| Built Different II | Gold | Your units with no Traits active gain 200-425 Health and 40-60% Attack Speed (based on current Stage). |
| Built Different III | Prismatic | Your units with no Traits active gain 270-600 Health and 50-70% Attack Speed (based on current Stage). |
| Buried Treasures I | Silver | Gain a random item component at the start of the next 2 rounds (including this round). |
| Buried Treasures I | Silver | Gain a random item component at the start of the next 2 rounds (including this round). |
| Buried Treasures II | Gold | Gain a random item component at the start of the next 3 rounds (including this round). |
| Buried Treasures II | Gold | Gain a random item component at the start of the next 3 rounds (including this round). |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 5 rounds (including this round). |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 5 rounds (including this round). |
| Capricious Forge | Gold | Gain a Blacksmith's Gloves, which equips two temporary Ornn Artifacts each round. |
| Caretaker's Ally | Silver | Each time you level up, gain the same random Tier 2 champion. |
| Caretaker's Ally | Silver | Each time you level up, gain the same random Tier 2 champion. |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4 - Gain a component anvil Level 6 - Gain a completed item anvil Level 7 - Open a radiant item armory |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4 - Gain a component anvil Level 6 - Gain a completed item anvil Level 7 - Open a radiant item armory |
| Caretaker's Favor | Gold | Gain a component anvil when you reach level 5, 6, 7, and 8 |
| Caretaker's Favor | Gold | Gain a component anvil when you reach level 5, 6, 7, and 8 |
| Challenger Crest | Gold | Gain a Challenger Emblem and a Warwick. |
| Challenger Crown | Prismatic | Gain a Challenger Emblem, a Hand of Justice, and a Kalista. |
| Challenger Heart | Silver | Your team counts as having 1 additional Challenger. Gain a Warwick. |
| Chemtech Enhancements | Gold | Your units with Zaun's chem-mods gain 500 Health. Allies that start combat adjacent to them gain 100 Health. Gain 2 Zaun units. |
| Combat Caster | Gold | Your units gain 90-170 Shield (based on current Stage) for 6 seconds after casting their spell. |
| Component Buffet | Silver | Whenever you would get a random component, instead gain a Component Anvil. Gain a random component. |
| Consistency | Silver | Gain double win and loss streak gold. |
| Contagion | Gold | Combat start: Infect the highest Health enemy, causing them to take 20% more damage. Every 5 seconds or on death, the infection spreads to 2 nearby enemies. |
| Cruel Pact | Prismatic | Buying XP costs 5 Health instead of gold. Heal 2 Health before each player combat. |
| Cursed Crown | Prismatic | Gain +2 maximum team size, but take 100% increased player damage. |
| Cutting Corners | Silver | Leveling up costs 3 XP less. You can now reach Level 10. |
| Cutting Corners | Silver | Leveling up costs 3 XP less. You can now reach Level 10. |
| Cybernetic Bulk I | Silver | Your champions holding an item gain 250 Health. |
| Cybernetic Bulk II | Gold | Your champions holding an item gain 350 health. |
| Cybernetic Bulk III | Prismatic | Your champions holding an item gain 500 health. |
| Cybernetic Leech I | Silver | Your units holding an item gain 80 Health and 13% Omnivamp. |
| Cybernetic Leech II | Gold | Your units holding an item gain 120 Health and 18% Omnivamp. |
| Cybernetic Leech III | Prismatic | Your units holding an item gain 225 Health and 25% Omnivamp. |
| Deadeye Crest | Gold | Gain a Deadeye Emblem and an Ashe. |
| Deadeye Crown | Prismatic | Gain a Deadeye Emblem, a Guinsoo's Rageblade, and an Akshan. |
| Deadeye Heart | Silver | Your team counts as having 1 additional Deadeye. Gain an Ashe. |
| Dedication | Gold | The first time you field at least 4 distinct units of the same trait in player combat, gain an Emblem for that trait. |
| Defensive Dash | Gold | Combat start and on takedown: Your Challenger units gain 75-225 Shield (based on current Stage) for 2.5 seconds. Gain 2 Challenger units. |
| Demacia Crest | Gold | Gain a Demacia Emblem and a Galio. |
| Demacia Crown | Prismatic | Gain a Demacia Emblem, a Recurve Bow, and a Garen. |
| Demonflare | Gold | Gain a Swain. Your strongest Swain deals 4% increased damage per 100 max Health. |
| Double Trouble II | Gold | When you field exactly 2 copies of a champion, they both gain 33% Attack Damage and 33 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Double Trouble III | Prismatic | When you field exactly 2 copies of a champion, they both gain 44% Attack Damage and 44 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Dueling Gunners | Gold | Your Gunner units gain 8% Attack Speed, plus 2% per Gunner stack. Gain 2 Gunners. |
| Early Education | Gold | Your units permanently gain 1 Ability Power whenever they kill an enemy. Units start with 15 bonus Ability Power. |
| Endless Hordes | Prismatic | Gain +3 maximum team size, but your units can only hold 1 item and their total health is reduced by 20%. Gain 6 gold. |
| Endless Hordes + | Prismatic | Gain +3 maximum team size, but your units can only hold 1 item and their total health is reduced by 20%. Gain 9 gold. |
| Endurance Training | Gold | Your units permanently gain 18 Health every time they kill an enemy. Units start with 100 bonus Health. |
| Escort Quest | Gold | Gain a Training Dummy. Each round, if it survives combat, gain 3 gold. |
| Final Ascension | Prismatic | Your units deal 15% more damage. After 15 seconds, this effect triples. |
| Final Grab Bag | Silver | Gain a random component, 10 gold, and a Reforger. |
| Final Grab Bag II | Gold | Gain 2 random components, 12 gold, and a Magnetic Remover. |
| Final Reserves | Prismatic | The first time you would be eliminated or reduced to 1 Health, instead remain alive. After this effect triggers, at the beginning of your next planning phase gain 80 XP and 40 gold. |
| Freljord Heart | Silver | Your team counts as having 1 additional Freljord. Gain an Ashe. |
| Freljord Soul | Prismatic | Your team counts as having 1 additional Freljord. Gain an Archangel's Staff and a Lissandra. |
| Frequent Flier | Gold | After you refresh your shop 8 times, your refreshes cost 1 for the rest of the game. (Refreshes this game: (live counter)) |
| Gargantuan Resolve | Gold | Gain a Titan's Resolve. Your Titan's Resolves can continue stacking to 35 instead of 25. |
| Giant Grab Bag | Prismatic | Gain 3 random components, 10 gold, and a Lesser Champion Duplicator. |
| Gifts from the Fallen | Gold | Your units gain 4% Attack Damage, 4 Ability Power, 4 Armor, and 4 Magic Resist. Each time an allied unit dies, gain these stats again. |
| Glacial Breeze | Gold | When your Freljordian Storm strikes, your units are shielded for 15% of their maximum Health and gain 8% Attack Speed. Gain 2 Freljord units. |
| Golden Ticket | Prismatic | Each time your Shop is refreshed, you have a 50% chance to gain a free refresh. |
| Gotta Go Fast! | Silver | Your units generate 10% more Mana and move 20% faster. |
| Gotta Go Fast! II | Gold | Your units generate 25% more Mana and move 40% faster. |
| Gotta Go Fast!!! III | Prismatic | Your units generate 35% more Mana and move 50% faster. |
| Gunner Crest | Gold | Gain a Gunner Emblem and a Jinx. |
| Gunner Crown | Prismatic | Gain a Gunner Emblem, a Runaan's Hurricane, and a Jayce. |
| Gunner Heart | Silver | Your team counts as having 1 additional Gunner. Gain a Jinx. |
| Harmacist I | Silver | Your units heal for 10% of the damage they deal, and they convert 20% of excess healing to true damage on their next attack. |
| Harmacist II | Gold | Your units heal for 15% of the damage they deal, and they convert 25% of excess healing to true damage on their next attack. |
| Harmacist III | Prismatic | Your units heal for 25% of the damage they deal, and they convert 30% of excess healing to true damage on their next attack. |
| Haunted Shell | Gold | If Shadow Isles is active, your units gain 13 Armor and Magic Resistance for the rest of combat each time an ally dies. Gain 2 Shadow Isles units. |
| Healing Orbs I | Silver | When an enemy dies, the nearest allied unit is healed for 300. |
| Healing Orbs II | Gold | When an enemy dies, the nearest allied unit is healed for 500. |
| Hedge Fund | Prismatic | Gain 22 gold. Your maximum interest is increased to 10. |
| Hedge Fund | Prismatic | Gain 22 gold. Your maximum interest is increased to 10. |
| Hedge Fund+ | Prismatic | Gain 32 gold. Your maximum interest is increased to 10. |
| Hedge Fund++ | Prismatic | Gain 44 gold. Your maximum interest is increased to 10. |
| Hustler | Gold | You earn no interest, but gain 3 gold at the start of every player combat round. |
| Idealism | Gold | Gain a Hand of Justice. Champions holding Hand of Justice deal 13% increased damage. |
| Impenetrable Bulwark | Prismatic | Gain a Bramble Vest, a Dragon's Claw, and a Magnetic Remover. |
| Inconsistency | Silver | If your streak is less than 3, gain 2 gold. |
| Indomitable Will | Silver | When your units score a takedown they shed all negative effects and become immune to crowd control for 10 seconds. |
| Infernal Contract | Prismatic | Your max level is 7. Gain 85 gold. |
| Infusion | Gold | Your units restore 20 Mana every 5 seconds. |
| Invoker Crest | Gold | Gain an Invoker Emblem and a Soraka. |
| Invoker Crown | Prismatic | Gain a Invoker Emblem, a Jeweled Gauntlet, and a Karma. |
| Invoker Heart | Silver | Your team counts as having 1 additional Invoker. Gain a Soraka. |
| Ionia Crest | Gold | Gain a Ionia Emblem and a Sett. |
| Ionia Crown | Prismatic | Gain an Ionia Emblem, a Guardbreaker, and a Karma. |
| Iron Assets | Silver | Gain a Component Anvil and 2 gold. |
| It Pays To Learn | Silver | Gain 10 XP and 8 gold. |
| It Pays to Learn II | Gold | Gain 16 XP and 12 gold. |
| It Pays to Learn III | Prismatic | Gain 20 XP and 16 gold. |
| Item Grab Bag I | Silver | Gain 1 random completed item. |
| Item Grab Bag II | Gold | Gain a random completed item, a random component, and 5 gold. |
| Item Grab Bag III | Prismatic | Gain 2 random completed items and 8 Gold. |
| Jeweled Lotus I | Silver | Combat start: Your strongest unit gains 40% Critical Strike Chance and their spells can critically strike. |
| Jeweled Lotus I | Silver | Combat start: Your strongest unit gains 40% Critical Strike Chance and their spells can critically strike. |
| Jeweled Lotus II | Gold | Your units' Abilities can critically strike. Your units gain 15% Critical Strike chance. |
| Jeweled Lotus II | Gold | Your units' Abilities can critically strike. Your units gain 15% Critical Strike chance. |
| Jeweled Lotus III | Prismatic | Your units' Abilities can critically strike. Your units gain 45% Critical Strike chance. |
| Jeweled Lotus III | Prismatic | Your units' Abilities can critically strike. Your units gain 45% Critical Strike chance. |
| Job Well Done | Gold | Gain a Completed Item Anvil and a Component Anvil. |
| Job's Done | Silver | Gain 2 Component Anvils. |
| Juggernaut Crest | Gold | Gain an Juggernaut Emblem and a Sett. |
| Juggernaut Crown | Prismatic | Gain an Juggernaut Emblem, a Bloodthirster, and a Darius. |
| Juggernaut Heart | Silver | Your team counts as having 1 additional Juggernaut. Gain a Sett. |
| Know Your Enemy | Gold | Your units deal 12% increased damage. If you and your opponent have any of the same traits activated, they deal 20% increased damage instead. |
| Knowledge Download 1+ | Silver | Gain 20 XP. |
| Knowledge Download I | Silver | Gain 12 XP. |
| Knowledge Download II | Gold | Gain 22 XP. |
| Knowledge Download II+ | Gold | Gain 28 XP. |
| Knowledge Download III | Prismatic | Gain 36 XP. |
| Knowledge Download III+ | Prismatic | Gain 36 XP. |
| Large Forge | Prismatic | Gain an Ornn Item Anvil and a Component Item Anvil. |
| Last Stand | Gold | The first time you would be eliminated or reduced to 1 Health, instead remain alive. After this effect triggers, your units permanently gain 180 Health, 18 Armor and Magic Resist, and 18% Omnivamp. |
| Lategame Specialist | Silver | When you reach Level 9, gain 44 gold. |
| Latent Forge | Silver | Gain an Ornn Item Anvil after 8 player combats. |
| Latent Forge | Silver | Gain an Ornn Item Anvil after 8 player combats. |
| Level Up! | Prismatic | When you buy XP, gain an additional 3. Gain 2 immediately. You can now reach level 10. |
| Level Up! | Prismatic | When you buy XP, gain an additional 3. Gain 2 immediately. You can now reach level 10. |
| Library Card | Gold | Gain a Tome of Traits and a Component Anvil. |
| Living Forge | Prismatic | Gain an Ornn Item Anvil now and after every 10 player combats. |
| Living Forge | Prismatic | Gain an Ornn Item Anvil now and after every 10 player combats. |
| Long Distance Pals II | Gold | Combat start: Your two furthest units form a bond, sharing 25% of their Armor, Magic Resist, Attack Damage, and Ability Power with each other. |
| Loving Invocation | Gold | Your team gains 2 Ability Power until the end of combat each time your Invokers cast their Abilities. Gain 2 Invokers. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 3 Sparring Gloves. |
| Magic Wand | Gold | Gain a Needlessly Large Rod. Your units gain 20% Ability Power. |
| Mana Burn | Gold | Gain a Shroud of Stillness. All enemies take 2% of their maximum Health as true damage each second until the first time they cast their Ability. |
| March of Progress | Prismatic | Gain 3 XP now, and bonus XP equal to your level at the start of every player combat round. You can no longer use gold to level up. |
| Martyr | Gold | Whenever one of your units dies, all allies heal for 10% of their maximum Health. |
| Masterful Job | Prismatic | Gain an Ornn Item Anvil and a Completed Item Anvil |
| Medium Forge | Gold | Gain a Completed Item Anvil and 7 Gold. |
| Metabolic Accelerator | Gold | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Metabolic Accelerator | Gold | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Missed Connections | Silver | Gain a copy of each Tier 1 champion. |
| Money Money Money! | Prismatic | Gain 25 gold. In 4 turns, gain 25 gold again. |
| Money Money! | Gold | Gain 16 gold. In 4 turns, gain 16 gold again. |
| Money! | Silver | Gain 11 gold. In 4 turns, gain 11 gold again. |
| Morning Light | Gold | When your Bastion units drop below 50% Health, they heal for 40% of their maximum Health over 4 seconds. Gain 2 Bastion units. |
| Multicaster Soul | Prismatic | Your team counts as having 1 additional Multicaster. Gain a Jeweled Gauntlet and a Vel'Koz. |
| Not Today | Gold | Gain an Edge of Night. Champions holding Edge of Night gain 35% Attack Speed. |
| Noxus Crest | Gold | Gain a Noxus Emblem and a Swain. |
| Noxus Crown | Prismatic | Gain a Noxus Emblem, a Sparring Gloves, and a Darius. |
| On a Roll | Silver | Whenever you star up a champion, gain a free shop refresh, up to 2 time(s) per turn. Gain 2 Gold. |
| One Twos Three | Silver | Gain 1 Tier one unit, 2 Tier two units, and 1 Tier three unit. |
| One Twos Three | Silver | Gain 1 Tier one unit, 2 Tier two units, and 1 Tier three unit. |
| One, Two, Five! | Silver | Gain 1 random component, 2 gold, and 1 random Tier 5 champion(s). |
| Overcharged Manafont | Gold | Combat start: Your Sorcerers gain 10 mana. The first time each Sorcerer gets a takedown each combat, they gain 20 Mana. Gain 2 Sorcerers. |
| Overwhelming Force | Prismatic | Gain a Deathblade and an Infinity Edge, and a Magnetic Remover. |
| Pandora's Bench | Silver | Gain 2 gold. At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Pandora's Items | Silver | Gain 1 random item component(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items | Silver | Gain 1 random item component(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items II | Gold | Gain 1 random completed item(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items II | Gold | Gain 1 random completed item(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items III | Prismatic | Gain a random Radiant item. Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items III | Prismatic | Gain a random Radiant item. Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Partial Ascension | Silver | After 15 seconds of combat, your units deal 30% more damage. |
| Parting Gifts | Gold | When a unit dies, they pass a temporary copy of one of their items to the nearest ally with open slots and shield them for 40% of their maximum Health. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. You can now reach Level 10. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. You can now reach Level 10. |
| Perfected Repetition | Gold | Each time your Multicasters cast their Ability, they gain 6 Ability power (max: 90) until the end of combat. Gain 2 Multicasters. |
| Petricite Shackles | Gold | Your Demacians deal 15% more damage, increased to 22% against Noxians and enemies with 100 or more maximum Mana. Gain 3 Demacians. |
| Phreaky Friday | Prismatic | Gain an Infinity Force. After 5 player combats, gain another. |
| Phreaky Friday + | Prismatic | Gain an Infinity Force. After 3 player combats, gain another. |
| Piltover Heart | Gold | Your team counts as having 1 additional Piltover. Gain a Vi. |
| Piltover Soul | Prismatic | Your team counts as having 1 additional Piltover. Gain a Giant Slayer and a Jayce. |
| Portable Forge | Gold | Open an Armory and choose 1 of 2 unique Artifacts crafted by Ornn. |
| Portable Forge | Gold | Open an Armory and choose 1 of 2 unique Artifacts crafted by Ornn. |
| Portable Forge | Gold | Open an Armory and choose 1 of 3 unique Artifacts crafted by Ornn. |
| Portable Forge | Gold | Open an Armory and choose 1 of 4 unique Artifacts crafted by Ornn. |
| Pumping Up I | Silver | Your units gain 8% Attack Speed. Each round, increase this by 0.5%. (current Attack Speed: (live counter)%) |
| Pumping Up I | Silver | Your units gain 8% Attack Speed. Each round, increase this by 0.5%. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your units gain 10% Attack Speed. Each round, increase this by 1%. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your units gain 10% Attack Speed. Each round, increase this by 1%. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your units gain 12% Attack Speed. Each round, increase this by 2%. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your units gain 12% Attack Speed. Each round, increase this by 2%. (current Attack Speed: (live counter)%) |
| Radiant Relics | Prismatic | Open an Armory and choose 1 of 5 unique Radiant items. Gain a Magnetic Remover. |
| Ravenous Hunter | Gold | Gain a Warwick. When your strongest Warwick takes or deals damage, he gains 5% Attack Damage and 5 Ability Power, stacking up to 40 times. His passive healing is replaced with magic damage on his attacks. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 Tier higher. Gain 2 Magnetic Removers. |
| Red Buff | Silver | Your units' attacks burn their targets for 5% of their maximum Health over 5 seconds and reduce their targets' healing received by 50%. |
| Return on Investment | Gold | After you refresh your shop 20 times, gain a Tactician's Crown (once only). (Refreshes this game: (live counter)) |
| Rich Get Richer | Gold | Gain 12 gold. Your maximum interest is increased to 7. |
| Rich Get Richer | Gold | Gain 12 gold. Your maximum interest is increased to 7. |
| Rich Get Richer+ | Gold | Gain 18 gold. Your maximum interest is increased to 7. |
| Riftwalk | Gold | Gain a Kassadin. Before casting, your strongest Kassadin gains 32 Ability Power and blinks to the furthest enemy within 3 hexes, but his spell no longer disarms or shields. His Mana cost is reduced by 30. |
| Risky Moves | Silver | Your Tactician loses 20 Health, but after 7 player combats, gain 30 gold. |
| Rogue Crest | Gold | Gain a Rogue Emblem and a Zed. |
| Rogue Crown | Prismatic | Gain a Rogue Emblem, a Hand of Justice, and a Katarina. |
| Rogue Heart | Silver | Your team counts as having 1 additional Rogue. Gain a Zed. |
| Roll The Dice | Prismatic | Gain a Rascal's Gloves, which equips two random Radiant items every round. |
| Rolling For Days I | Silver | Gain 10 free shop refreshes. These shop refreshes carry over between rounds. |
| Rolling For Days II | Gold | Gain 15 free shop refreshes. These shop refreshes carry over between rounds. |
| Rolling For Days III | Prismatic | Gain 25 free shop refreshes. These shop refreshes carry over between rounds. |
| Salvage Bin | Gold | Gain a random completed item now, and a component after 7 player combats. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Salvage Bin+ | Gold | Gain a random completed item now, and a component after 4 player combats. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Scoped Weapons | Gold | Your units that start combat in the back 2 rows gain +2 Attack Range and 15% Attack Speed. |
| Scrappy Inventions | Gold | At the start of combat, up to 5 components on your champions turn into completed items for the rest of combat. At the start of the next 2 stage(s), gain a component. |
| Seeing Double I | Silver | Gain 1 completed item(s) made from two of the same component and 2 gold. |
| Seeing Double II | Gold | Gain 1 completed item(s) made from two of the same component and 10 gold. |
| Seeing Double III | Prismatic | Gain 2 completed item(s) made from two of the same component and 4 gold. |
| Sentinel's Spirit | Gold | While in spirit form, your Ionians gain 15% Attack Speed and shield themselves for 15% of their maximum Health. Gain 3 Ionians. |
| Shadow Isles Crest | Gold | Gain a Shadow Isles Emblem, a Maokai, and 1 gold. |
| Shadow Isles Crown | Prismatic | Gain a Shadow Isles Emblem, a Tear of the Goddess, and a Kalista. |
| Shadow Isles Heart | Silver | Your team counts as having 1 additional Shadow Isles. Gain a Viego and 1 gold. |
| Shimmering Inventors | Gold | Your Piltover units deal 7% more damage for every 10 gold you have (max: 35%), and have a 33% chance to drop 1 gold at round start. Gain an Ekko. |
| Shoplifting | Gold | At the start of each turn, gain the highest Tier champion in your shop for free. |
| Shopping Spree | Prismatic | Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. |
| Shopping Spree | Prismatic | Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. |
| Shurima Crest | Gold | Gain a Shurima Emblem and a Taliyah. |
| Shurima Crown | Prismatic | Gain a Shurima Emblem, a Protector's Vow, and an Akshan. |
| Shurima's Legacy | Gold | After 8 seconds of combat, the Sun Disc attacks enemies for 160-800 magic damage every 2 seconds (based on current Stage). Gain 3 Shurimans. |
| Silver Spoon | Silver | Gain 10 XP. |
| Silver Ticket | Gold | Each time your shop is refreshed, you have a 30% chance to gain a free refresh. |
| Slayer Crest | Gold | Gain a Slayer Emblem and a Kled. |
| Slayer Crown | Prismatic | Gain a Slayer Emblem, a Titan's Resolve, and a Kled. |
| Slayer Heart | Silver | Your team counts as having 1 additional Slayer. Gain a Kled. |
| Slayer's Resolve | Gold | When your Slayers take or deal damage, they gain 2 stacking Armor and Magic Resistance for the rest of combat (max: 25 stacks). Gain 2 Slayers. |
| Sleight of Hand | Gold | Gain a Thief's Gloves. Champions holding Thief's Gloves gain 30% Attack Speed. |
| Small Forge | Silver | Gain a Component Anvil and 4 gold. |
| Social Distancing I | Silver | Combat start: Units with no adjacent allies gain 12% Attack Damage and Ability Power. |
| Social Distancing II | Gold | Combat start: Units with no adjacent allies gain 18% Attack Damage and Ability Power. |
| Social Distancing III | Prismatic | Combat start: Units with no adjacent allies gain 25% Attack Damage and Ability Power. |
| Sorcerer Crest | Gold | Gain a Sorcerer Emblem and a Swain. |
| Sorcerer Crown | Prismatic | Gain a Sorcerer Emblem, a Spear of Shojin, and a Vel'Koz. |
| Sorcerer Heart | Silver | Your team counts as having 1 additional Sorcerer. Gain a Swain. |
| Spoils of War I | Silver | When you kill an enemy unit, there's a 25% chance to drop loot. |
| Spoils of War I | Silver | When you kill an enemy unit, there's a 25% chance to drop loot. |
| Spoils of War II | Gold | When you kill an enemy unit, there's a 30% chance to drop loot. |
| Spoils of War II | Gold | When you kill an enemy unit, there's a 30% chance to drop loot. |
| Spoils of War III | Prismatic | When you kill an enemy unit, there's a 40% chance to drop loot. |
| Spoils of War III | Prismatic | When you kill an enemy unit, there's a 40% chance to drop loot. |
| Stable Evolution | Gold | Your Void units gain 60 Health and 6% Attack Damage and Ability Power for each star level they have. Gain 3 Void units. |
| Stars are Born | Gold | The first Tier 1 champion and Tier 2 champion you buy are upgraded to 2-star. Gain 6 gold. |
| Stars are Born | Gold | The first Tier 1 champion and Tier 2 champion you buy are upgraded to 2-star. Gain 6 gold. |
| Starter Kit | Prismatic | Gain a Tier 4 champion and a 2-star Tier 1 champion that shares a trait with them. At the next 2 stages, gain the Tier 4 champion again. |
| Starter Kit | Prismatic | Gain a Tier 4 champion and a 2-star Tier 1 champion that shares a trait with them. At the next 2 stages, gain the Tier 4 champion again. |
| Stellacorn's Blessing | Gold | While you have the Targon trait activated, your units gain 45% Attack Speed for 4 seconds after being healed or shielded. Gain 2 Targonians. |
| Strategist Heart | Gold | Your team counts as having 1 additional Strategist. Gain a Swain. |
| Strategist Soul | Prismatic | Your team counts as having 1 additional Strategist. Gain a Chalice of Power and a Swain. |
| Suppressing Fire | Gold | The target of your Deadeye units' shared attack deals 35% reduced damage for 8 seconds. Gain 2 Deadeyes. |
| Tactical Superiority | Gold | Your champions gain 5% Attack Damage and Ability Power for each of their active traits, doubled when you field a Strategist. Gain 2 Strategists. |
| Tactician's Tools | Prismatic | Gain two Spatulas. |
| Targon Heart | Gold | Your team counts as having 1 additional Targon. Gain a Soraka. |
| Targon Soul | Prismatic | Your team counts as having 1 additional Targon. Gain a Redemption and a Taric. |
| The Boss | Gold | Gain a Sett. The first time your strongest Sett falls below 60% Health, he takes a break to do sit-ups. Each sit-up heals him 10% of his max Health and grants 45% Attack Speed and Ability Power. |
| The Golden Egg | Prismatic | Gain a massive golden egg that hatches in 11 turns. Victorious player combats accelerate the hatch timer by an additional turn. |
| Three's a Crowd | Gold | Your units gain 111 Health for each unique Tier 3 unit you field. |
| Three's Company | Gold | Gain 4 random Tier 3 champions. |
| Tiniest Titan | Prismatic | Your tactician is small and speedy, heals 2 Health after a PVP round, and grants 2 gold per round. |
| Tiniest Titan | Prismatic | Your tactician is small and speedy, heals 2 Health after a PVP round, and grants 2 gold per round. |
| Tiny Grab Bag | Silver | Gain a random component, 4 gold, a Magnetic Remover. |
| Tiny Power I | Silver | Your units gain 7% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Power II | Gold | Your units gain 10% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Power III | Prismatic | Your units gain 16% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Titans | Silver | Your Tactician heals 30 Health, grows larger, and gains 30 maximum health. |
| Tiny Titans | Silver | Your Tactician heals 30 Health, grows larger, and gains 30 maximum health. |
| Titanic Strength | Gold | Bruisers gain increased Attack Damage equal to 1.5% of their maximum Health. Gain 2 Bruisers. |
| Tons of Stats! | Gold | Your team gains 35 Health, 5% Attack Damage, 5 Ability Power, 5 Armor, 5 Magic Resist, 5% Attack Speed, and 5 Mana. |
| Total Domination | Gold | Noxus units' attacks and Abilities execute enemies below 8% Health, increased by 1% for each player you've Conquered. Gain 3 Noxians. |
| Trade Sector | Gold | Gain a free Shop refresh each round. Gain 2 gold. |
| Trade Sector | Gold | Gain a free Shop refresh each round. Gain 2 gold. |
| Training Reward | Silver | Gain 7 gold and a Lesser Champion Duplicator. |
| Training Reward II | Gold | Gain 15 gold and a Lesser Champion Duplicator. |
| Training Reward III | Prismatic | Gain 24 gold and a Champion Duplicator. |
| Transfusion I | Silver | Your units gain 20 Health, plus 2 Health per missing Tactician Health. |
| Transfusion I | Silver | Your units gain 20 Health, plus 2 Health per missing Tactician Health. |
| Transfusion II | Gold | Your units gain 40 Health, plus 3 Health per missing Tactician Health. |
| Transfusion II | Gold | Your units gain 40 Health, plus 3 Health per missing Tactician Health. |
| Transfusion III | Prismatic | Your units gain 50 Health, plus 5 Health per missing Tactician Health. |
| Transfusion III | Prismatic | Your units gain 50 Health, plus 5 Health per missing Tactician Health. |
| Two Healthy | Gold | Your units gain 111 Health for each unique Tier 2 unit you field. |
| Unburdened I | Silver | Your units without items equipped gain 30% Attack Speed. |
| Unburdened II | Gold | Your units without items equipped gain 55% Attack Speed. |
| Unified Resistance I | Silver | If you have 3 or more units in the same row at the start of combat, they all gain 18 Armor and Magic Resist. |
| Unified Resistance II | Gold | If you have 3 or more units in the same row at the start of combat, they all gain 30 Armor and Magic Resist. |
| Unleashed Arcana | Prismatic | Gain a Jeweled Gauntlet, a Rabadon's Deathcap, and a Magnetic Remover. |
| Unstable Yordle Delivery | Gold | Round start: Gain a random Yordle or 1-5 gold (based on current Stage). Gain 3 Yordles now. |
| Urf's Grab Bag | Prismatic | Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Vampiric Blades | Gold | Your units gain 10% Omnivamp. Your Rogues gain 20% instead. Gain 2 Rogues. |
| Void Crest | Gold | Gain a Void Emblem and a Kassadin. |
| Void Crown | Prismatic | Gain a Void Emblem, a Zz'Rot Portal, and a Rek'Sai. |
| Wandering Trainer | Prismatic | Gain a Training Dummy. It has 3 random Emblems equipped, which cannot be removed. Gain 4 gold. |
| Well-Earned Comforts I | Silver | Your units gain 60 Health for each item equipped. |
| Well-Earned Comforts II | Gold | Your units gain 111 Health for each item equipped. |
| Well-Earned Comforts III | Prismatic | Your units gain 120 Health and 8% Attack Speed for each item equipped. |
| Wellness Trust | Prismatic | Round start: Gain 3 gold. If you have at least 40 gold, your Tactician heals 2 Health. |
| What Doesn't Kill You | Gold | After losing your combat, gain 2 gold. Every 5 losses, gain a random component. Losses until next component: (live counter) |
| What The Forge | Prismatic | All completed items you own or receive are transformed into random Ornn Artifacts (excluding Tactician's Crown and Emblems). Units gain 160 Health per equipped Artifact. |
| Winds of War | Gold | Gain a Galio. Your strongest Galio's spell is larger, and enemies hit take magic damage equal to 8% of his max Health each second. |
| You Have My Bow | Gold | Gain a Recurve Bow. Your units gain 15% Attack Speed. |
| You Have My Sword | Gold | Gain a B.F. Sword. Your units gain 18% Attack Damage. |
| Young and Wild and Free | Silver | You can always move freely on Carousel rounds. Gain 1 random item component(s). |
| Zaun Crest | Gold | Gain a Zaun Emblem and a Warwick. |
| Zaun Crown | Prismatic | Gain a Zaun Emblem, a Guardbreaker, and an Ekko. |
| Zaun Heart | Silver | Your team counts as having 1 additional Zaun. Gain a Warwick. |

## Legends (15)

Legends were picked before the game. Each Legend changed which augments the player was offered in the early, mid and late augment rounds. Source: MetaTFT lookup.

| Legend | Description | Early augments | Mid augments | Late augments |
|---|---|---|---|---|
| Aurelion Sol: The Ancient | Become high level and play awesome units! | Cutting Corners: Leveling up costs 3 XP less. You can now reach Level 10. / Patient Study: After player combat, gain 2 XP if you won or 3 XP if you lost. You can now reach Level 10. / Level Up!: When you buy XP, gain an additional 3. Gain 2 immediately. You can now reach level 10. | Knowledge Download I: Gain 12 XP. / Knowledge Download II: Gain 22 XP. / Knowledge Download III: Gain 36 XP. | It Pays To Learn: Gain 10 XP and 8 gold. / It Pays to Learn II: Gain 16 XP and 12 gold. / It Pays to Learn III: Gain 20 XP and 16 gold. |
| Bard: The Traveler | Gain even more power when you level up! | Caretaker's Ally: Each time you level up, gain the same random Tier 2 champion. / Caretaker's Favor: Gain a component anvil when you reach level 5, 6, 7, and 8 / Caretaker's Chosen: As you level, gain more powerful items. Level 4 - Gain a component anvil Level 6 - Gain a completed item anvil Level 7 - Open a radiant item armory | Small Forge: Gain a Component Anvil and 4 gold. / Medium Forge: Gain a Completed Item Anvil and 6 Gold. / Large Forge: Gain an Artifact Anvil and a Component Item Anvil. | Job's Done: Gain 2 Component Anvils. / Teaming Up II: Gain 1 random Support item(s), 1 gold, and 1 Reforger(s). / Teaming Up III: Gain 1 random Support item(s), 18 gold, and 2 Reforger(s). |
| Caitlyn: The Peacekeeper | Get strong units at the start of the game! | One Twos Three: Gain 1 Tier one unit, 2 Tier two units, and 1 Tier three unit. / Stars are Born: The first Tier 1 champion and Tier 2 champion you buy are upgraded to 2-star. Gain 6 gold. / Starter Kit: Gain a Tier 4 champion and a 2-star Tier 1 champion that shares a trait with them. At the next 2 stages, gain the Tier 4 champion again. | Knowledge Download I: Gain 12 XP. / Knowledge Download II: Gain 22 XP. / Knowledge Download III: Gain 36 XP. | Rolling For Days I: Gain 10 free shop refreshes. These shop refreshes carry over between rounds. / Rolling For Days II: Gain 15 free shop refreshes. These shop refreshes carry over between rounds. / Rolling For Days III: Gain 25 free shop refreshes. These shop refreshes carry over between rounds. |
| Draven: The Executioner | Win big and get bigger payouts! | Spoils of War I: When you kill an enemy unit, there's a 25% chance to drop loot. / Spoils of War II: When you kill an enemy unit, there's a 30% chance to drop loot. / Spoils of War III: When you kill an enemy unit, there's a 40% chance to drop loot. | Balanced Budget: At the start of the next 4 rounds, gain 4 gold. / Balanced Budget II: At the start of the next 4 rounds, gain 6 gold. / Balanced Budget III: At the start of the next 4 rounds, gain 11 gold. | Rolling For Days I: Gain 10 free shop refreshes. These shop refreshes carry over between rounds. / Rolling For Days II: Gain 15 free shop refreshes. These shop refreshes carry over between rounds. / Rolling For Days III: Gain 25 free shop refreshes. These shop refreshes carry over between rounds. |
| Ezreal: The Item Collector | The more items, the better! | Buried Treasures I: Gain a random item component at the start of the next 2 rounds (including this round). / Buried Treasures II: Gain a random item component at the start of the next 3 rounds (including this round). / Buried Treasures III: Gain a random item component at the start of the next 6 rounds (including this round). | Tiny Grab Bag: Gain a random component, 4 gold, a Magnetic Remover. / Big Grab Bag: Gain 2 random components, 8 gold, and a Reforger. / Giant Grab Bag: Gain 3 random components, 10 gold, and a Lesser Champion Duplicator. | Well-Earned Comforts I: Your units gain 80 Health for each item equipped. / Well-Earned Comforts II: Your units gain 111 Health for each item equipped. / Well-Earned Comforts III: Your units gain 120 Health and 8% Attack Speed for each item equipped. |
| Lee Sin: The Playmaker | Refresh your shop for perfect units! | Bronze Ticket: Every 4 shop refreshes, gain a free refresh. / Trade Sector: Gain a free Shop refresh each round. Gain 2 gold. / Shopping Spree: Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. | Training Reward: Gain 7 gold and a Lesser Champion Duplicator. / Training Reward II: Gain 15 gold and a Lesser Champion Duplicator. / Training Reward III: Gain 24 gold and a Champion Duplicator. | Money!: Gain 11 gold. In 4 turns, gain 11 gold again. / Money Money!: Gain 16 gold. In 4 turns, gain 16 gold again. / Money Money Money!: Gain 25 gold. In 4 turns, gain 25 gold again. |
| Master Yi: The Wuju Master | Grow stronger and stronger with every fight! | Pumping Up I: Your units gain 8% Attack Speed. Each round, increase this by 0.5%. (current Attack Speed: (live counter)%) / Pumping Up II: Your units gain 8% Attack Speed. Each round, increase this by 1%. (current Attack Speed: (live counter)%) / Pumping Up III: Your units gain 12% Attack Speed. Each round, increase this by 2%. (current Attack Speed: (live counter)%) | Battle Ready: Your team deals 3% more damage and takes 3% less damage. / Battle Ready II: Your team deals 6% more damage and takes 6% less damage. / Battle Ready III: Your team deals 8% more damage and takes 8% less damage. | Gotta Go Fast!: Your units generate 10% more Mana and move 35% faster. / Gotta Go Fast! II: Your units generate 25% more Mana and move 60% faster. / Gotta Go Fast!!! III: Your units generate 35% more Mana and move 100% faster. |
| Ornn: The Forgemaster | Gain the power of Ornn's creations! | Latent Forge: Gain an Artifact Anvil after 8 player combats. / Portable Forge: Open an Armory and choose 1 of 2 unique Artifacts. / Living Forge: Gain an Artifact Anvil now and after every 10 player combats. | Small Forge: Gain a Component Anvil and 4 gold. / Medium Forge: Gain a Completed Item Anvil and 6 Gold. / Large Forge: Gain an Artifact Anvil and a Component Item Anvil. | Job's Done: Gain 2 Component Anvils. / Job Well Done: Gain a Completed Item Anvil and a Component Anvil. / Masterful Job: Gain an Artifact Anvil and a Completed Item Anvil |
| Pengu: The Tactician | Play it safe! Great for beginners! | Tiny Titans: Your Tactician heals 30 Health, grows larger, and gains 30 maximum health. / Metabolic Accelerator: Your Tactician moves faster and heals 2 Health after a PvP round. / Tiniest Titan: Your tactician is small and speedy, heals 2 Health after a PVP round, and grants 2 gold per round. | Item Grab Bag I: Gain 1 random completed item. / Item Grab Bag II: Gain a random completed item, a random component, and 5 gold. / Item Grab Bag III: Gain 2 random completed items and 8 Gold. | Final Grab Bag: Gain a random component, 10 gold, and a Reforger. / Final Grab Bag II: Gain 2 random components, 12 gold, and a Magnetic Remover. / Urf's Grab Bag: Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Poro: The Imporoble  | No tailloring, just a classic game of TFT! |  |  |  |
| Tahm Kench: The Glutton | Collect as much gold as possible! | AFK: You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. / Rich Get Richer: Gain 12 gold. Your maximum interest is increased to 7. / Hedge Fund: Gain 23 gold. Your maximum interest is increased to 10. | Balanced Budget: At the start of the next 4 rounds, gain 4 gold. / Balanced Budget II: At the start of the next 4 rounds, gain 6 gold. / Balanced Budget III: At the start of the next 4 rounds, gain 11 gold. | Money!: Gain 11 gold. In 4 turns, gain 11 gold again. / Money Money!: Gain 16 gold. In 4 turns, gain 16 gold again. / Money Money Money!: Gain 25 gold. In 4 turns, gain 25 gold again. |
| Twisted Fate: The Gambler | Get the exact items you need! | Pandora's Items: Gain 1 random item component(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). / Pandora's Items II: Gain 1 random completed item(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). / Pandora's Items III: Gain a random Radiant item. Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). | Item Grab Bag I: Gain 1 random completed item. / Item Grab Bag II: Gain a random completed item, a random component, and 5 gold. / Item Grab Bag III: Gain 2 random completed items and 8 Gold. | Final Grab Bag: Gain a random component, 10 gold, and a Reforger. / Teaming Up II: Gain 1 random Support item(s), 1 gold, and 1 Reforger(s). / Teaming Up III: Gain 1 random Support item(s), 18 gold, and 2 Reforger(s). |
| Urf: The Emblem Collector | For the player that loves emblems! | Branching Out: Gain a random Emblem and a Reforger. / Ancient Archives I: Gain 1 Tome of Traits and 3 gold. / Ancient Archives II: Gain 2 Tome of Traits and 5 gold. | Tiny Grab Bag: Gain a random component, 4 gold, a Magnetic Remover. / Big Grab Bag: Gain 2 random components, 8 gold, and a Reforger. / Giant Grab Bag: Gain 3 random components, 10 gold, and a Lesser Champion Duplicator. | Final Grab Bag: Gain a random component, 10 gold, and a Reforger. / Final Grab Bag II: Gain 2 random components, 12 gold, and a Magnetic Remover. / Urf's Grab Bag: Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Veigar: The Caster | Deal big damage with your champions' abilities! | Jeweled Lotus I: Combat start: Your strongest unit gains 40% Critical Strike Chance and their abilities can critically strike. / Jeweled Lotus II: Your units' Abilities can critically strike. Your units gain 15% Critical Strike chance. / Jeweled Lotus III: Your units' Abilities can critically strike. Your units gain 45% Critical Strike chance. | Tiny Power I: Your units gain 6% Attack Damage, Ability Power, and Attack Speed. / Tiny Power II: Your units gain 10% Attack Damage, Ability Power, and Attack Speed. / Tiny Power III: Your units gain 15% Attack Damage, Ability Power, and Attack Speed. | Partial Ascension: After 15 seconds of combat, your units deal 30% more damage. / Ascension: After 15 seconds of combat, your units deal 50% more damage. / Final Ascension: Your units deal 15% more damage. After 15 seconds, this effect triples. |
| Vladimir: The Hemomancer | Trade your health for power! | Transfusion I: Your units gain 20 Health, plus 2 Health per missing Tactician Health. / Transfusion II: Your units gain 40 Health, plus 3 Health per missing Tactician Health. / Transfusion III: Your units gain 50 Health, plus 5 Health per missing Tactician Health. | Battle Ready: Your team deals 3% more damage and takes 3% less damage. / Battle Ready II: Your team deals 6% more damage and takes 6% less damage. / Battle Ready III: Your team deals 8% more damage and takes 8% less damage. | Partial Ascension: After 15 seconds of combat, your units deal 30% more damage. / Ascension: After 15 seconds of combat, your units deal 50% more damage. / Final Ascension: Your units deal 15% more damage. After 15 seconds, this effect triples. |

## Portals (62)

Source: MetaTFT lookup "portals" list.

| Name | Group | Effect |
|---|---|---|
| Spatula | Spatula | Everyone gets a spatula! |
| Ixaocan | Ixtal | Each time players star up 20 units, all players gain increasingly valuable loot. |
| Gold | Gold | Everyone gains 8 Gold. |
| Valar's Hollow | Freljord | On stage 4-2 gain a component item anvil. On 6-2, gain a Support item anvil. |
| Finn's Market | Bilgewater | Twice per game, Bilgewater Trader Finn will randomly appear and offer Completed, Artifact, Support, or Radiant items. You can choose one to keep for free! |
| Slaughter Docks | Bilgewater | Gain free shop rerolls equal to the stage number plus 1 at the start of every other stage, beginning at stage 3! These only last for the round you get them. |
| Tome of Traits | Tome | Everyone gets a Tome of Traits. |
| Mystery Box | LootOrb | Everyone gets a loot orb of mysterious value. |
| Health | Health | Everyone gains 8 tactician health. |
| Cardinal Arcology | Ixtal | You will always be offered a Silver, Gold, and Prismatic augment, in that order. |
| Rat Town | Bilgewater | Starting at stage 2, Lucky Shops appear randomly once per stage. These feature units tailored to your army's active traits. |
| Serpentine River | Ixtal | The stage 2 and 4 carousels are replaced by voting rounds. Everyone gets a component anvil and the winning reward! |
| Component Anvil | Component | Everyone gains a component anvil. |
| Valar's Hollow | Freljord | On stage 2-3 gain a component item anvil. On 3-3, gain a Support item anvil. |
| Rerolls | Rerolls | Everyone gains 5 free shop rerolls that last forever. |
| Slaughter Docks | Bilgewater | Gain free shop rerolls equal to the stage number plus 1 at the start of each stage! These only last for the round you get them. |
| Support Item | SupportItem | Everyone gets a Support Item Anvil! |
| Rat Town | Bilgewater | Starting at stage 3, Lucky Shops appear randomly once per stage. These feature units tailored to your army's active traits. |
| Warlord's Palace | Shurima | Stage 4-7 is replaced by a treasure armory round. Each reroll costs 1. |
| Yorick's Graveyard | ShadowIsles | Every time a player dies, all players open an armory containing 4 of the eliminated players' items. |
| The Sump | Zaun | There is no interest this game. Instead, starting on stage 2-1, gain 3 gold every round. |
| Immortal Bastion | Noxus | Players start with 115 health. |
| Thresh's Sanctum | ShadowIsles | When ANY unit dies, collect their soul. Every 40 souls, gain a loot orb. |
| House Lightshield | Demacia | Gain a Crown of Demacia on stage 3-1. If the holder dies, you instantly lose the fight. |
| Hall of the Nine | Void | At the start of every other stage, everyone gets the same loot from a highly varied pool. |
| Placidium Library | Ionia | Gain a Scroll of Knowledge on stage 3-5. Use it on a champion to open an armory offering every emblem that matches their traits. |
| Hearth-Home | Freljord | On stage 4-5, gain a Masterwork Upgrade consumable that lets you upgrade one equipped item into its radiant version. |
| Targon Prime | Targon | When you reach 40 Health, receive a blessing from the Targonian gods. |
| House Lightshield | Demacia | Gain a Crown of Demacia on stage 2-1. If the holder dies, you instantly lose the fight. |
| Ornn's Forge | Freljord | Randomly on either stage 2-5 or 3-5, gain an Artifact Anvil. |
| Immortal Bastion | Noxus | Players start with 115 health. |
| Shuriman Bazaar | Shurima | Each champion on a component carousel has 2 components on them instead of 1. |
| Ecliptic Vaults | Zaun | Every time you select an augment, gain 4 gold per augment you have. |
| Marus Omegnum | Targon | On stage 2-5 and 4-5, gain a Tactician's Crown. |
| Thresh's Sanctum | ShadowIsles | When ANY unit dies, collect their soul. Every 20 souls, gain a loot orb. |
| Shifting Sands | Shurima | At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Hall of the Nine | Void | At the start of each stage, everyone gets the same loot from a highly varied pool. |
| Unstable Rift | Void | Gain an unstable completed item that pops off and transforms into a random item every round. |
| God-Willow's Grove | Ionia | A spot on your bench becomes the grove. The champion placed in it contributes their non-unique traits as though they were in your army. |
| Stillwater Hold | Piltover | Whenever you would be offered an augment, gain a completed item anvil instead. |
| The Dreaming Pool | Ionia | At the start of each stage, gain a champion that fits your team. |
| Fleshing Arena | Noxus | Twice per stage, get a free high-tier champion. The champion's tier increases with game time. |
| The Summit | Targon | Starting at stage 2, gain a Lesser Champion Duplicator at the start of every other stage. On stage 6 and beyond, gain a Champion Duplicator instead. |
| Glasc Industries | Zaun | Every time you build a completed item, gain 3 gold. |
| Bandle Cafeteria | BandleCity | Gain a Spatula on stage 3-1. Each round, champions equipped with Spatulas feed Poro Snax to their closest ally and grant them 40 permanent Health for each Spatula component they have. |
| Yuumi's Zoom Zone | BandleCity | When you buy XP, gain an additional 2. |
| Targon Prime | Targon | When you reach 10 Health, receive a blessing from the Targonian gods. |
| The Summit | Targon | Starting at stage 3, gain a Lesser Champion Duplicator at the start of each stage. On stage 5 and beyond, gain a Champion Duplicator instead. |
| Ornn's Forge | Freljord | Randomly on either stage 4-1 or 6-3, gain an Artifact Anvil. |
| Ehrenmount | Demacia | Gain 3 component anvils at the start of the game. |
| Scuttle Puddle | BandleCity | PvE monsters are replaced by scuttle crabs that grant extra loot. |
| Noxkraya | Noxus | Each time you take an augment, a random arena hex appears. Champions on an arena hex gain an item that they can use. |
| The University | Piltover | The first augment offered this game will be prismatic. |
| Jayce's Workshop | Piltover | All augments offered this game will be prismatic. |
| Hearth-Home | Freljord | On stage 7-1, gain a Masterwork Upgrade consumable that lets you upgrade one equipped item into its radiant version. |
| Warlord's Palace | Shurima | Stage 7-1 is replaced by a treasure armory round. Each reroll costs 1. |
| The Rupture | Void | Travel to a random location that is not currently shown. |
| Fleshing Arena | Noxus | Once per stage, get a free high-tier unit. The unit's tier increases with game time. |
| Bandle Cafeteria | BandleCity | Gain a Spatula on stage 2-1. Each round, champions equipped with Spatulas feed Poro Snax to their closest ally and grant them 20 permanent Health for each Spatula component they have. |
| Placidium Library | Ionia | Gain a Scroll of Knowledge on stage 6-3. Use it on a champion to open an armory offering every emblem that matches their traits. |
| Marus Omegnum | Targon | On stage 4-1 and 7-3, gain a Tactician's Crown. |
| Marus Omegnum | Targon | On stage 2-5 and 3-6, gain a Tactician's Crown. |
