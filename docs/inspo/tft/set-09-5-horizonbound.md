# TFT Set 9.5: Horizonbound

Source: CommunityDragon `https://raw.communitydragon.org/13.22/cdragon/tft/en_us.json` (set data `TFTSet9_Stage2`) for champions, stats, abilities, traits, items and augments. MetaTFT lookup `https://data.metatft.com/lookups/TFTSet9_Stage2_latest_en_us.json` (retrieved 2026-10-15) for spell calculation formulas used to fill computed tooltip values, augment rarity where CDragon has none, and set-mechanic lists. Release date: esports.gg "Everything we know about TFT Set 9.5, Horizonbound".

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 9.5 (mid-set update of Set 9) |
| Name | Horizonbound |
| Riot launch patch | 13.18 |
| League client version at launch (CDragon path) | 13.18 |
| Live dates | September 13, 2023 to November 20, 2023 |
| Theme | Continues Runeterra Reforged; adds the Bilgewater and Ixtal regions. |
| Data snapshot used | Patch 13.22 (last patch of the set) |
| Champion count in this file | 71 |

## Set mechanics and system changes

- Last TFT mid-set. Riot retired the mid-set format starting with Set 10.
- New traits compared with Set 9 (from the data): Bilgewater, Ixtal, Reaver King, Vanquisher. No Set 9 trait was removed in the data, but several Set 9 traits changed membership.
- Champions added versus Set 9: Graves, Illaoi, Milio, Naafiri, Qiyana, Twisted Fate, Miss Fortune, Nautilus, Neeko, Quinn, Fiora, Mordekaiser, Nilah, Silco, Xayah, Gangplank. Removed: Maokai, Tristana, Viego, Kled, Teemo, Zed, Akshan, Garen, Kalista, Lissandra, Gwen, Lux, Urgot, Yasuo, Zeri, Senna.
- Bilgewater: Bilgerat attacks and abilities mark enemies; the mark stores a share of the damage Bilgerats deal, and after 1.5 seconds a cannonball hits the marked enemy for the stored damage plus a flat amount. (The Silver Serpent black market is a Set 16 Bilgewater mechanic, not Set 9.5.)
- Ixtal: the player gains elemental hexes whose bonus changes from game to game.
- Region Portals and Legends continue; new portals were added for the new regions (77 portal rows in the data).
- Mid-set augment pass changed economy augments across all three tiers.

## Champions (71)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Cassiopeia | 1 | Noxus, Shurima, Invoker | 500 | 20 | 20 | 40 | 0.7 | 4 | 0/30 |
| Cho'Gath | 1 | Void, Bruiser | 700 | 30 | 30 | 65 | 0.5 | 1 | 40/100 |
| Graves | 1 | Bilgewater, Gunner, Rogue | 700 | 40 | 40 | 55 | 0.65 | 1 | 30/80 |
| Illaoi | 1 | Bilgewater, Bastion | 700 | 40 | 40 | 60 | 0.55 | 1 | 30/70 |
| Irelia | 1 | Ionia, Challenger | 700 | 40 | 40 | 50 | 0.65 | 1 | 40/80 |
| Jhin | 1 | Ionia, Vanquisher | 504 | 14 | 14 | 54 | 0.7 | 5 | 44/114 |
| Kayle | 1 | Demacia, Slayer | 500 | 15 | 15 | 30 | 0.75 | 4 | 0/0 |
| Malzahar | 1 | Void, Sorcerer | 500 | 15 | 15 | 40 | 0.7 | 4 | 0/50 |
| Milio | 1 | Ixtal, Invoker | 500 | 20 | 20 | 40 | 0.7 | 4 | 0/60 |
| Orianna | 1 | Piltover, Sorcerer | 500 | 15 | 15 | 35 | 0.75 | 4 | 20/70 |
| Poppy | 1 | Demacia, Bastion | 700 | 40 | 40 | 55 | 0.55 | 1 | 30/90 |
| Renekton | 1 | Shurima, Bruiser | 650 | 45 | 45 | 50 | 0.6 | 1 | 50/110 |
| Samira | 1 | Noxus, Challenger | 500 | 15 | 15 | 45 | 0.7 | 4 | 0/30 |
| Ashe | 2 | Freljord, Vanquisher | 600 | 20 | 20 | 55 | 0.75 | 6 | 20/70 |
| Galio | 2 | Demacia, Invoker | 800 | 45 | 45 | 70 | 0.5 | 1 | 60/110 |
| Jinx | 2 | Zaun, Gunner | 600 | 20 | 20 | 50 | 0.75 | 4 | 10/70 |
| Kassadin | 2 | Void, Bastion | 800 | 45 | 45 | 50 | 0.7 | 1 | 50/100 |
| Naafiri | 2 | Darkin, Shurima, Challenger | 750 | 40 | 40 | 60 | 0.8 | 1 | 0/50 |
| Qiyana | 2 | Ixtal, Slayer, Rogue | 650 | 35 | 35 | 55 | 0.75 | 1 | 40/100 |
| Sett | 2 | Ionia, Juggernaut | 800 | 50 | 50 | 60 | 0.6 | 1 | 60/110 |
| Soraka | 2 | Targon, Invoker | 700 | 25 | 25 | 66 | 0.65 | 4 | 30/90 |
| Swain | 2 | Noxus, Strategist, Sorcerer | 800 | 45 | 45 | 45 | 0.65 | 2 | 60/135 |
| Taliyah | 2 | Shurima, Multicaster | 550 | 20 | 20 | 40 | 0.7 | 4 | 20/60 |
| Twisted Fate | 2 | Bilgewater, Multicaster | 550 | 20 | 20 | 40 | 0.7 | 4 | 25/75 |
| Vi | 2 | Piltover, Bruiser | 800 | 50 | 50 | 60 | 0.6 | 1 | 20/70 |
| Warwick | 2 | Zaun, Juggernaut, Challenger | 800 | 50 | 50 | 40 | 0.75 | 1 | 40/90 |
| Darius | 3 | Noxus, Juggernaut, Vanquisher | 750 | 50 | 50 | 70 | 0.7 | 1 | 20/80 |
| Ekko | 3 | Zaun, Piltover, Rogue | 800 | 40 | 40 | 50 | 0.8 | 1 | 0/45 |
| Jayce | 3 | Piltover, Gunner | 700 | 25 | 25 | 60 | 0.75 | 4 | 30/80 |
| Karma | 3 | Ionia, Invoker | 700 | 25 | 25 | 45 | 0.7 | 4 | 0/45 |
| Katarina | 3 | Noxus, Rogue | 800 | 40 | 40 | 50 | 0.75 | 1 | 20/80 |
| Miss Fortune | 3 | Bilgewater, Strategist | 650 | 25 | 25 | 50 | 0.7 | 4 | 0/50 |
| Nautilus | 3 | Bilgewater, Juggernaut | 950 | 40 | 40 | 80 | 0.5 | 1 | 60/140 |
| Neeko | 3 | Ixtal, Bastion | 800 | 45 | 45 | 50 | 0.65 | 2 | 50/120 |
| Quinn | 3 | Demacia, Slayer | 700 | 25 | 25 | 60 | 0.75 | 4 | 30/90 |
| Rek'Sai | 3 | Void, Bruiser, Slayer | 800 | 45 | 45 | 60 | 0.75 | 1 | 50/75 |
| Sona | 3 | Demacia, Multicaster | 700 | 25 | 25 | 45 | 0.65 | 4 | 35/80 |
| Taric | 3 | Targon, Bastion, Sorcerer | 800 | 50 | 50 | 60 | 0.6 | 1 | 15/85 |
| Vel'Koz | 3 | Void, Multicaster, Sorcerer | 700 | 30 | 30 | 40 | 0.7 | 4 | 0/60 |
| Aphelios | 4 | Targon, Gunner | 750 | 25 | 25 | 65 | 0.75 | 4 | 50/100 |
| Azir | 4 | Shurima, Strategist | 750 | 30 | 30 | 30 | 0.8 | 4 | 40/50 |
| Fiora | 4 | Demacia, Challenger | 950 | 45 | 45 | 75 | 0.9 | 1 | 60/120 |
| Jarvan IV | 4 | Demacia, Strategist | 1000 | 60 | 60 | 60 | 0.65 | 1 | 100/180 |
| Kai'Sa | 4 | Void, Challenger | 850 | 35 | 35 | 45 | 0.8 | 4 | 60/120 |
| Mordekaiser | 4 | Noxus, Slayer | 1000 | 50 | 50 | 70 | 0.55 | 1 | 30/80 |
| Nasus | 4 | Shurima, Juggernaut | 1000 | 55 | 55 | 60 | 0.7 | 1 | 70/150 |
| Nilah | 4 | Bilgewater, Vanquisher | 950 | 45 | 45 | 75 | 0.8 | 2 | 20/60 |
| Sejuani | 4 | Freljord, Bruiser | 1100 | 60 | 60 | 60 | 0.6 | 1 | 60/120 |
| Shen | 4 | Ionia, Bastion, Invoker | 1000 | 60 | 60 | 60 | 0.6 | 1 | 90/170 |
| Silco | 4 | Zaun, Sorcerer | 750 | 30 | 30 | 45 | 0.7 | 4 | 0/30 |
| Xayah | 4 | Ionia, Vanquisher | 750 | 25 | 25 | 65 | 0.75 | 4 | 20/100 |
| Aatrox | 5 | Darkin, Slayer, Juggernaut | 900 | 70 | 70 | 80 | 0.8 | 1 | 0/50 |
| Ahri | 5 | Ionia, Sorcerer | 850 | 40 | 40 | 50 | 0.85 | 4 | 0/50 |
| Bel'Veth | 5 | Void, Empress | 1100 | 60 | 60 | 80 | 0.85 | 2 | 20/70 |
| Gangplank | 5 | Bilgewater, Gunner, Reaver King | 1000 | 30 | 30 | 80 | 0.85 | 1 | 90/180 |
| Heimerdinger | 5 | Piltover, Technogenius | 750 | 30 | 30 | 40 | 0.75 | 4 | 80/160 |
| K'Sante | 5 | Shurima, Bastion | 1000 | 50 | 50 | 60 | 0.7 | 1 | 0/70 |
| Ryze (Ryze) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 0/999 |
| Ryze (RyzeDemacia) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 50/100 |
| Ryze (RyzeBandleCity) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 30 | 0.85 | 4 | 30/65 |
| Ryze (RyzeFreljord) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 15/60 |
| Ryze (RyzeIonia) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/100 |
| Ryze (RyzeNoxus) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 40/100 |
| Ryze (RyzePiltover) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 15/75 |
| Ryze (RyzeShadowIsles) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/70 |
| Ryze (RyzeShurima) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 60/120 |
| Ryze (RyzeTargon) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/90 |
| Ryze (RyzeZaun) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 30/90 |
| Ryze (RyzeBilgewater) | 5 | Wanderer, Invoker | 1111 | 30 | 30 | 50 | 0.85 | 4 | 0/75 |
| Ryze (RyzeIxtal) | 5 | Wanderer, Invoker | 1111 | 50 | 50 | 50 | 0.85 | 2 | 30/100 |
| Sion | 5 | Noxus, Bruiser | 900 | 65 | 65 | 70 | 0.6 | 1 | 70/160 |

### Abilities

**Cassiopeia** (1-cost) - Twin Fang: Deal 160/240/360 (AP) magic damage to the current target and Wound them for 5 seconds. If they are already Wounded, deal 48/72/108 (AP) bonus magic damage. Wound: Reduce healing received by 33%

**Cho'Gath** (1-cost) - Feast: Deal 270/370/500 + 12% max HP (HP+AP) magic damage to the lowest Health enemy in range. If this kills them, devour them and permanently gain 35 maximum Health (doubled in Hyper Roll). (Current Bonus: +(live counter) Health)

**Graves** (1-cost) - Smoke Grenade: Launch a smoke grenade at the current target. It explodes on impact, dealing 30/45/60 + 200/200/205% AD (AD+AP) physical damage to adjacent enemies, leaving behind a smoke cloud that Chills for 3/3.5/4 seconds. Chill: Reduce Attack Speed by 30%

**Illaoi** (1-cost) - Harsh Lesson: Deal 250/375/560 (AP) magic damage to the current target and link souls for 5 seconds. While linked, heal for 25/30/40% of all damage taken by the target.

**Irelia** (1-cost) - Defiant Dance: Enter a defensive stance and gain 350/400/450 (AP) Shield that rapidly decays over 3 seconds. When it expires, deal 70/100/150 (AP) magic damage + 30% of the damage absorbed to enemies around and in front of Irelia. Ionia Bonus: +25 Armor and Magic Resist

**Jhin** (1-cost) - Curtain Call: Take aim at the current target and deal 60/90/135 + 744% AD (AD+AP) physical damage to enemies in a line; each hit reduces this damage by 56%. Ionia Bonus: +25% Attack Damage

**Kayle** (1-cost) - Divine Ascent: Passive: Gains new Passive effects as your Tactician levels up. Level 1: Attacks deal 30/45/70 (AP) bonus magic damage. Level 6: Every 3rd attack launches a wave that deals 30/40/60 (AP) magic damage and 20% Shreds enemies for 3 seconds. Level 9: Every attack launches a wave and they travel farther. Shred: Reduce Magic Resist

**Malzahar** (1-cost) - Call of the Void: Open two portals near the current target. Destroy 50% of Shields and deal 220/330/500 (AP) magic damage to all enemies caught between the portals.

**Milio** (1-cost) - Ultra Mega Fire Kick!!!: Kick a ball at the current target that deals 220/330/500 (AP) magic damage and Stuns for 1.5 seconds. The ball bounces to the closest enemy behind the target, dealing 180/270/400 (AP) magic damage on impact and 90/140/200 (AP) magic damage to adjacent enemies.

**Orianna** (1-cost) - Command: Protect: Grant the lowest Health ally 200/225/275 (AP) Shield for 4 seconds and empowers Orianna's next attack to deal 290/435/650 (AP) bonus magic damage.

**Poppy** (1-cost) - Steadfast Hammer: Gain 250/325/400 (AP) Shield for 4 seconds. Deal 160/240/360 (AP) magic damage to the current target.

**Renekton** (1-cost) - Cull the Meek: Deal 180/270/400 (AP) magic damage to adjacent enemies. Heal 220/270/320 (AP) for the first enemy hit and another 30 (AP) for each additional one.

**Samira** (1-cost) - Flair: Shoot at the current target and deal 190/190/200% AD to the first enemy hit. Reduce their Armor by 10/15/20 (AP) for the rest of combat.

**Ashe** (2-cost) - Volley: Fire 8 arrows in a cone, each dealing 160/165/175% AD physical damage to the first enemy hit and Chilling them for 2 (AP) seconds. Chill: Reduce Attack Speed by 30%

**Galio** (2-cost) - Shield of Durand: Reduce damage taken by 25/25/35% and heal for 300/350/400 (AP) over 2.5 seconds. After, deal 200/300/450 (AP) magic damage to adjacent enemies.

**Jinx** (2-cost) - Fishbones!: Fire 5 rockets at random enemies within 2 hexes of the current target. Each rocket deals 15/20/35 + 150/155/160% AD (AD+AP) physical damage.

**Kassadin** (2-cost) - Force Pulse: Gain 250/300/400 (AP) Shield for 4 seconds. Deal 160/240/360 (AP) magic damage to enemies in a cone and Disarm them for 1.5/1.75/2 seconds. Disarm: Cannot move or attack, but CAN cast spells

**Naafiri** (2-cost) - Eviscerate: Passive: Gain 15% (AP) Omnivamp. Active: Deal 175/175/180% AD physical damage. Darkin Dagger: After casting, summon packmates that attack the first enemy hit for a total of 190/195/200% AD. Omnivamp: Heal for percentage of damage dealt

**Qiyana** (2-cost) - Supreme Display of Talent: Dash to strike enemies in a line and deal 40/60/90 + 255/255/270% AD (AD+AP) physical damage, Stunning and knocking up the first target hit for 1.5 seconds and all other targets for 0.5 seconds.

**Sett** (2-cost) - Facebreaker: Grab an enemy from either side and slam them together, knocking them up. Deals 180/270/420 (AP) magic damage and Stuns them for 1.25/1.5/2 seconds. If only one enemy is grabbed, the damage and Stun duration are increased by 50%. Ionia Bonus: +180 max Health

**Soraka** (2-cost) - Astral Infusion: Heal the lowest health ally for 140/160/180 (AP). If the ally is below 50% Health, heal them for an additional [TooltipBonusHeal: unknown] (AP). Over the next 5 seconds, 5 stars hit the enemy closest to them. Each deals 120/180/280 (AP) magic damage.

**Swain** (2-cost) - Demonflare: Transform and gain 325/375/550 (AP) max Health. While transformed, deal 25/40/60 (AP) magic damage to enemies within 2 hexes every second. If already transformed, gain an additional 225/260/385 (AP) max Health and deal 100/160/300 (AP) magic damage to enemies within.

**Taliyah** (2-cost) - Seismic Shove: Passive: Whenever an enemy is knocked up or back by anything, throw a boulder towards them. It deals 120/180/270 (AP) magic damage to the first enemy hit. Active: Deal 220/330/495 (AP) magic damage to the current target and knock them up, Stunning them for 2 seconds.

**Twisted Fate** (2-cost) - Stacked Deck: Throw a hand of cards at the current target that deal 155/235/355 (AP) magic damage. After 1.25 seconds the cards explode, dealing 155/235/380 (AP) magic damage to nearby enemies.

**Vi** (2-cost) - Blast Shield: Gain 350/400/450 (AP) Shield for 4 seconds. Deal 300% AD physical damage to enemies within 1 hex of the current target and 20% Sunder them for 4 seconds. Sunder: Reduce Armor

**Warwick** (2-cost) - Primal Howl: Passive: Attacks heal for 30/35/40 (AP). Active: Gain 100% Attack Speed for 2.5 seconds. Then, Stun adjacent enemies for 1.25/1.5/2 seconds.

**Darius** (3-cost) - Noxian Guillotine: Deal 55/80/110 + 350% AD (AD+AP) physical damage to the current target. If they die, immediately cast this again with 90/95/100% damage.

**Ekko** (3-cost) - Phase Dive: Heal 20% of damage taken in the last 4 seconds and deal 255/380/570 (AP) magic damage to the current target.

**Jayce** (3-cost) - Acceleration Blast: Grant 30/40/50% (AP) Attack Speed to Jayce and 15% (AP) to the allies on his left and right for 3 seconds. Fire a blast at the current target that explodes on the first enemy hit. It deals 275/275/295% AD physical damage to enemies near the blast.

**Karma** (3-cost) - Inner Flame: Fire a burst of energy that explodes upon impact, dealing 200/300/470 (AP) magic damage to adjacent enemies. Every third cast launches 3 bursts. Ionia Bonus: +25 Ability Power

**Katarina** (3-cost) - Voracity: Throw three daggers that land next to enemies within 1 + 0.5% stat#28 (Range) hexes. Then, teleport to each and deal 145/220/350 (AP) magic damage to adjacent enemies and Wound them for 6 seconds. Wound: Reduce healing received by 33%

**Miss Fortune** (3-cost) - X Marks the Spot: Call down an X-shaped rain of bullets, destroying 35% of your target's shields and dealing 240/360/580 (AP) magic damage. For 5 seconds, enemies hit have incoming shields reduced by 35%.

**Nautilus** (3-cost) - Riptide: Passive: Gain 40% bonus Armor and Magic Resistance from all sources. Active: Deal 150/225/360 (AP) magic damage to enemies within two hexes. Knock them up and pull them together, Stunning them for 1.5/1.5/2 seconds.

**Neeko** (3-cost) - Pop Blossom: Leap towards the current target and gain 250/325/425 (AP) Shield for 3 seconds. Slam down after a brief delay, dealing 300/450/700 (AP) magic damage to enemies within two hexes.

**Quinn** (3-cost) - Skystrike: Launch Valor through the row or column with the most enemies, marking them to take 10% (AP) increased damage for 4 seconds. Then rain down arrows, dealing 550/555/565% AD physical damage split between all marked enemies.

**Rek'Sai** (3-cost) - Furious Bite: Deal 250/250/255% AD physical damage to the current target and mark them. If they are below 66% max Health, deal true damage instead. If the target is marked, deal 240/240/250% AD bonus true damage. If this kills the target, rip through them 1 hex and heal for 10% max HP (AP+HP).

**Sona** (3-cost) - Crescendo: Send a wave of sound at the largest clump of enemies and deal 170/255/420 (AP) magic damage to enemies hit; each hit reduces this damage by 33%. Allies hit by the wave gain 20/25/35% Attack Speed for the rest of combat.

**Taric** (3-cost) - Radiance: Gain 500/580/680 (AP) Shield for 4 seconds. It redirects 50% of the damage received by adjacent allies.

**Vel'Koz** (3-cost) - Plasma Fission: Fire a plasma bolt towards the current target that deals 220/330/550 (AP) magic damage to the first enemy hit. It then splits in two, dealing 50% less damage whenever it passes through an enemy.

**Aphelios** (4-cost) - Moonlight Vigil: Fire a moon blast at the largest group of enemies that deals 240/240/750% AD physical damage to enemies within 2 hexes. For 7 seconds, equip 3 Chakram, plus 1 more for each enemy hit by the blast. Attacks deal 7/7/15% AD bonus physical damage for each Chakram equipped. Aphelios heals for 75% (AP) of damage dealt by Chakrams.

**Azir** (4-cost) - Arise!: Passive: Every third attack, a Sand Soldier deals 110/160/500 (AP) magic damage to their target. Active: Summon a Sand Soldier to attack your target. If Azir already has 3 Soldiers, they all immediately deal 77/112/350 (AP) magic damage instead.

**Fiora** (4-cost) - Blade Waltz: Become untargetable and strike the current target 4 times. Each strike deals [TotalDamage: unknown] (AD) physical damage and 60/90/270 (AP) true damage. Heal for 15% of the total damage dealt. If the target dies, change targets to the nearest enemy.

**Jarvan IV** (4-cost) - Cataclysm: Leap on a nearby enemy. Deal 140/210/800 (AP) magic damage to enemies within 2 hexes and Stun them for 1.5/2/8 seconds.

**Kai'Sa** (4-cost) - Icathian Rain: Dash away from all enemies, then fire 15/15/25 missiles split across the 4 nearest enemies. Each missile deals 80/120/240 (AP) magic damage.

**Mordekaiser** (4-cost) - God of Death: Passive: Attacks deal 100/150/375 (AP) bonus magic damage, increased to 250/375/900 (AP) while God of Death is active. Active: Gain 40% max HP (HP) Shield and 1 range for 5 seconds. Killing enemies in this state steals 6/6/15% of their Health, Attack Damage, Ability Power, Armor, and Magic Resistance until combat ends.

**Nasus** (4-cost) - Soul Eater: Steal 4/4/15% maximum Health, 8% Attack Damage, 4/4/20 Armor, and 4/4/20 Magic Resist from the nearest 4/5/7 enemies for 8 (AP) seconds. While empowered, every third attack deals 380/380/700% AD physical damage.

**Nilah** (4-cost) - Formless Blade: Passive: Attacks strike in a cone, dealing 0.55/0.55/1 (AD) physical damage to up to 2 additional enemies. Every 3rd attack strikes in a line for [ModifiedEmpoweredDamage: unknown] (AD) physical damage and grants 18/18/40% stacking Attack Speed. Active: Gain 250/375/700 (AP) Shield for 3 seconds and dash to a safe location.

**Sejuani** (4-cost) - Fury of the North: Passive: Whenever an ally attacks a Chilled enemy, they deal 1/1/10% max HP (HP) bonus true damage. Active: Gain 600/700/2000 (AP) Shield for 4 seconds. Deal 160/240/1200 (AP) magic damage to nearby enemies and Chill them for 4 seconds. Chill: Reduce Attack Speed by 30%

**Shen** (4-cost) - Ki Barrier: Gain 350/450/2000 (AP) Shield and grant 225/275/1500 (AP) Shield to the 2 lowest-Health allies for 4 seconds. After shielding his allies, Shen's Shield refreshes with a burst, dealing 240/360/2500 (AP) magic damage to adjacent enemies. Ionia Bonus: +8% Damage Reduction

**Silco** (4-cost) - Undercity Tactics: Throw a vial the closest surrounded enemy, covering nearby hexes in chemicals for 6/6/10 seconds. Deal 65/100/400 (AP) magic damage per second to chemical-affected enemies, and heal affected allies for 20/25/120 (AP) per second. When possible, target unaffected groups.

**Xayah** (4-cost) - Featherstrike: Summon 7/7/15 feathers that converge on and pierce the current target, dealing 15/25/60 + 80/80/100% AD (AD+AP) physical damage. Each feather removes 6 Armor from the first target they hit until the end of combat. Ionia Bonus: Attacks restore an additional Mana 5 Mana

**Aatrox** (5-cost) - World Ender: Transform for 12/12/30 seconds, converting 80% of bonus Attack Speed to Attack Damage. While transformed, attacks deal 290/300/2500% AD physical damage in an area. Darkin Blade: Gain 250 Health and 10% (AP) Omnivamp. If another unit holds this weapon for at least 4 seconds, Aatrox will revive upon their death. Omnivamp: Heal for percentage of damage dealt

**Ahri** (5-cost) - Essence Thief: Steal essence from enemies around the current target, dealing 105/150/1000 (AP) magic damage and 20% Mana Reaving them. Every 3/3/1 casts, unleash a wave that deals 260/390/1999 (AP) magic damage to all enemies hit. This deals 33% more damage to enemies whose essence has been stolen. Ionia Bonus: +3 Mana per second Mana Reave: Increase max Mana until the next cast

**Bel'Veth** (5-cost) - Royal Maelstrom: Lash out at the lowest Health enemy in range 6/6/25 + 500% stat#3 (AS) times. Each lash deals 18/30/100 + 85% AD (AD+AP) physical damage and 2% of the target's max Health as true damage.

**Gangplank** (5-cost) - The Dreadway: Cutlass Passive: Gain 50 Armor and Magic Resist. Attacks ignite the target, dealing 250/250/2500% AD true damage over 2 seconds.Pistol Passive: Gain 3 range. Attacks grant 5 bonus mana. Active: Summon the Dreadway to sail across the board, crashing into the first enemy hit and dealing 450/675/9001 (AP) magic damage in a 3-hex radius. Allies the Dreadway passes through or hits with the crash are empowered by citrus, gaining 35/50/300% Attack Speed and immunity to crowd control for 3 seconds.

**Heimerdinger** (5-cost) - CH-3X Lightning Grenade: Hurl a grenade towards the largest group of enemies, dealing 150/225/3141 (AP) magic damage to enemies in a large area and Stunning them for 1.5/2/15 seconds.

**K'Sante** (5-cost) - All Out Strike: Heal for [HealAmount: unknown] (HP), then knock up the current target before smashing them to the edge of the battlefield. Deal 250/400/1000 (AP) magic damage and Stun them for 2/2.5/10 seconds. Deal 250/400/1000 (AP) magic damage to enemies the target collides with and Stun them for 1/1.25/5 seconds. If the target can't be pushed back any further, knock them off the battlefield. Otherwise, chase after them.

**Ryze (Ryze)** (5-cost) - Realm Warp: Bandle City: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with [ModifiedBonusMana: unknown] (AP) bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

**Ryze (RyzeDemacia)** (5-cost) - Realm Warp: Demacia: Create a portal to Demacia that protects allies for 2/2/5 seconds. Allies within the zone gain 600/900/9999 (AP) Shield. If an ally leaves the safety of the portal, they are healed for 250/375/9999 (AP). When the portal expires, enemies within are dealt 300/450/2000 (AP) magic damage.

**Ryze (RyzeBandleCity)** (5-cost) - Realm Warp: Bandle City: Create a portal to Bandle City under the most expensive unit on your bench. They are summoned to the battlefield with 25/70/200 (AP) bonus Mana and are...slightly smaller? If there are no units left on your bench, Ryze summons a random high tier unit instead.

**Ryze (RyzeFreljord)** (5-cost) - Realm Warp: Freljord: Create a portal to the Freljord under a random group of enemies for 2 seconds. The freezing winds deal 300/450/3500 (AP) magic damage to enemies within two hexes over the duration and apply Permafrost, a permanent Chill. If enemies are already affected by Permafrost, they are instead Stunned for the duration. Chill: Reduce Attack Speed by 30%

**Ryze (RyzeIonia)** (5-cost) - Realm Warp: Ionia: Create a portal to Ionia in the center of the battlefield, summoning forth a Spirit Tree that affects the 5/7/20 closest units. Allies gain 30/40/300% Attack Speed for 6 seconds and are healed for 400/500/3000 (AP) over the duration. Enemies dance, dealing 175/275/1500 (AP) magic damage and Stunning them for 2/2.5/20 seconds.

**Ryze (RyzeNoxus)** (5-cost) - Realm Warp: Noxus: Create a portal to Noxus that spews out a torrent of weapons and armor, granting 3/4/10 completed items to random allies with open slots, and throwing 5/5/15 axes towards random enemies that deal 150/225/2000 (AP) magic damage each. If an ally would recieve an item while all slots are full, they instead gain a stacking 10% damage increase for the rest of combat.

**Ryze (RyzePiltover)** (5-cost) - Realm Warp: Piltover: Create a portal to Piltover that launches an enforcer grenade towards the largest clump of enemies, dealing 275/425/3000 (AP) magic damage to enemies within two hexes and creating a containment area for 5/5/10 seconds. Enemies are unable to leave the area, and 30/30/500% of the damage dealt to them is also dealt to all other enemies within.

**Ryze (RyzeShadowIsles)** (5-cost) - Realm Warp: Shadow Isles: Mark the 1/1/10 ally's soul(s) with the lowest current health to be reclaimed by the Shadow Isles. After they die, they return to the battlefield with 40/80/300% (AP) of their maximum Health. If all living allies are already marked, Ryze instead resurrects a deceased ally. Ryze cannot resurrect himself.

**Ryze (RyzeShurima)** (5-cost) - Realm Warp: Shurima: Create a portal to Shurima, summoning a roaming sand tornado that deals 175/275/2500 (AP) magic damage to enemies hit and knocks them up, Stunning them for 1/1.25/3 second(s). The tornado has a chance to unearth buried treasures and gold as it travels.

**Ryze (RyzeTargon)** (5-cost) - Realm Warp: Targon: Create a portal to the skies above Mount Targon that beckons the current target. After a few seconds, they descend from the heavens as a meteor towards the largest group of enemies, dealing 18/30/100% (AP) of the initial target's maximum Health as magic damage. Enemies in the epicenter are briefly knocked up. If the target is the only enemy remaining, they never descend.

**Ryze (RyzeZaun)** (5-cost) - Realm Warp: Zaun: Create 5/8/25 portals to Zaun, causing hired guns to fire at nearby enemies. Deals 125/230/2006 (AP) magic damage and Wounds, 30% Shreds, and 30% Sunders for 6 seconds. The more gold you have, the more powerful the portals become. 25 Gold: +1 portal. Portals fire a rocket that deals 50% damage in a one-hex radius. 50 Gold: Double the portals. Summon 1 additional portals for every 20 Gold more. Wound: Reduce healing received by 50% Shred: Reduce Magic Resist Sunder: Reduce Armor

**Ryze (RyzeBilgewater)** (5-cost) - Realm Warp: Bilgewater: Create a portal to Bilgewater, summoning a giant treasure chest to fall on the largest cluster of enemies that deals 350/525/7777 (AP) magic damage. The size of this chest is increased by 2% for every 2 gold you have. Each enemy hit has a chance to knock gold or other treasures out of the chest, and that chance doubles if the chest kills the enemy.

**Ryze (RyzeIxtal)** (5-cost) - Realm Warp: Ixtal: Create a portal to Ixtal, coating the ground around the current target in a thicket of vines. After a brief delay, deal [TotalDamage: unknown] (Armor MR) magic damage to enemies within and Stun enemies at the epicenter for 2/2.5/10 seconds with a knockup. Allies within are wrapped in Barkskin, granting [ModifiedArmor: unknown] (Armor) Armor and [ModifiedMR: unknown] (MR) Magic Resistance for 4 (AP) seconds.

**Sion** (5-cost) - Glory in Death: Passive: Upon death, reanimate with 100% Health, decaying at 20/13/0% maximum Health per second. Sion can no longer use his Ability, but is immune to crowd control and gains 150/150/1000% (AP) Attack Speed. Active: Charge towards the most enemies within 3 + 0.5% stat#28 (Range) hexes. Deal 225/235/500% AD physical damage to any enemies along the way and knock them up, Stunning them for 1.25/1.5/15 seconds.

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): Voidspawn, Target Dummy, Tome of Traits, Completed Item Anvil, Component Anvil, Artifact Item Anvil, Support item anvil, Baron Nashor, Sun Disc, Rift Herald, Void Remora, Apex Turret, T-Hex, Noxian Banner, SLIME_Crab, Mercenary Chest, Void Totem.

## Traits (28)

### Bastion

Bastion champions gain Armor and Magic Resist. This is increased by 100% for the first 10 seconds of combat.

| Units | Effect |
|---|---|
| 2-3 | +20 Armor, +20 MR |
| 4-5 | +45 Armor, +45 MR |
| 6-7 | +75 Armor, +75 MR |
| 8 | +140 Armor, +140 MR |

Champions: Illaoi (1), Poppy (1), Kassadin (2), Neeko (3), Taric (3), Shen (4), K'Sante (5)

### Bilgewater

Bilgerat attacks and Abilities mark enemies. The mark stores a percentage of damage dealt to the enemy by Bilgerats. Marked enemies are struck by a cannonball after 1.5 seconds, dealing the stored damage plus a flat value as physical damage.

| Units | Effect |
|---|---|
| 3-4 | 100 + 40% damage dealt |
| 5-6 | 170 + 60% damage dealt |
| 7-8 | 325 + 70% damage dealt |
| 9 | 999 + 135% damage dealt; nearby enemies take 50% of cannonball damage |

Champions: Graves (1), Illaoi (1), Twisted Fate (2), Miss Fortune (3), Nautilus (3), Nilah (4), Gangplank (5)

### Bruiser

Your units gain 100 Health. Bruisers gain additional maximum Health.

| Units | Effect |
|---|---|
| 2 | +10% max HP |
| 3 | +12% max HP |
| 4-5 | +45% max HP |
| 6 | +80% max HP |

Champions: Cho'Gath (1), Renekton (1), Vi (2), Rek'Sai (3), Sejuani (4), Sion (5)

### Challenger

Challengers gain bonus Attack Speed. When their target dies, Challengers dash to a new target and increase their Attack Speed bonus by 50% for 2.5 seconds.

| Units | Effect |
|---|---|
| 2-3 | 30% AS |
| 4-5 | 65% AS |
| 6-7 | 105% AS |
| 8 | 150% AS |

Champions: Irelia (1), Samira (1), Naafiri (2), Warwick (2), Fiora (4), Kai'Sa (4)

### Darkin

Weapon powers are described in each Darkin's Ability tooltip.

| Units | Effect |
|---|---|
| 1 | Darkin are possessed by a weapon. When they die, the weapon possesses the nearest ally champion, granting them the weapon's power. |
| 2 | Weapon powers become 50% stronger |

Champions: Naafiri (2), Aatrox (5)

### Demacia

Your strongest Demacians become Elite and equip a Radiant item each combat. Elites grant Armor and Magic Resist to themselves and adjacent allies, but this does not stack. Radiant items are stronger versions of full items.

| Units | Effect |
|---|---|
| 3-4 | 1 Elite, +5 Armor, +5 MR |
| 5-6 | 2 Elites, +25 Armor, +25 MR |
| 7-8 | 3 Elites, +45 Armor, +45 MR |
| 9 | 6 Elites, +150 Armor, +150 MR |

Champions: Kayle (1), Poppy (1), Galio (2), Quinn (3), Sona (3), Fiora (4), Jarvan IV (4)

### Empress

When Bel'Veth kills an enemy, they leave behind Void Coral that she will consume. The first Coral increases her maximum Health by 50%, while further Coral heals her 20% maximum Health. When a Void Coral is consumed, she deals 10% maximum Health as magic damage to enemies within 2 hexes.

| Units | Effect |
|---|---|
| 1 | HexRange 2; PercentBonusMaxHealth 0.5; PercentHealthHeal 0.2; PercentHealthMagicDamage 0.1 |

Champions: Bel'Veth (5)

### Freljord

After 8 seconds, an ice storm strikes the battlefield. Enemies take a percentage of their max Health as true damage and gain debuffs. for 10 seconds Sunder: Reduce Armor Shred: Reduce Magic Resist

| Units | Effect |
|---|---|
| 2 | 10% HP, enemies are 30% Sundered and Shredded |
| 3 | 18% HP, enemies are also Stunned for 1.5 seconds |

Champions: Ashe (2), Sejuani (4)

### Gunner

When Gunner champions attack, they gain bonus Attack Damage, up to 8 stacks.

| Units | Effect |
|---|---|
| 2-3 | 7% AD per stack |
| 4-5 | 13% AD per stack |
| 6 | 25% AD per stack |

Champions: Graves (1), Jinx (2), Jayce (3), Aphelios (4), Gangplank (5)

### Invoker

Every 3 seconds, your units gain Mana.

| Units | Effect |
|---|---|
| 2-3 | 5 Mana to all |
| 4-5 | Additional 15 to Invokers |
| 6-7 | 20 Mana to all, additional 15 to Invokers |
| 8 | 40 Mana to all, and start with an additional 30 Ability Power |

Champions: Cassiopeia (1), Milio (1), Galio (2), Soraka (2), Karma (3), Shen (4), Ryze (Ryze) (5), Ryze (RyzeDemacia) (5), Ryze (RyzeBandleCity) (5), Ryze (RyzeFreljord) (5), Ryze (RyzeIonia) (5), Ryze (RyzeNoxus) (5), Ryze (RyzePiltover) (5), Ryze (RyzeShadowIsles) (5), Ryze (RyzeShurima) (5), Ryze (RyzeTargon) (5), Ryze (RyzeZaun) (5), Ryze (RyzeBilgewater) (5), Ryze (RyzeIxtal) (5)

### Ionia

Each Ionian has a unique bonus within their Ability. Every 4 seconds, a number of Ionians are enlightened to their spirit form, gaining 20 mana and doubling stat bonuses for 4 seconds.

| Units | Effect |
|---|---|
| 3-5 | 100% Ionian bonus, 1 enlightened |
| 6-8 | 200% Ionian bonus, 2 enlightened |
| 9 | 425% Ionian bonus, 4 enlightened |

Champions: Irelia (1), Jhin (1), Sett (2), Karma (3), Shen (4), Xayah (4), Ahri (5)

### Ixtal

Gain elemental hexes, which give a different bonus from game to game.

| Units | Effect |
|---|---|
| 2 | ElectricCapstoneAmp 0.25; ElectricStunDuration 3; FireBurnPercent 1; FireCapstoneBurnPercent 2; FireCapstoneDamagePercent 1.7; FireDamagePercent 1; FireDuration 5; IceBlockDuration 2; IceCapstoneHealthPercentDamage 0.35; IceHealPercent 0.3; IceHealthThreshold 30; NumberOfHexes 1; StoneBuffDuration 10; StoneCapstoneHeal 900; StoneDamageReduction 0.2; WindAS 0.5; WindBuffDuration 5; WindCapstoneAS 1; WindCapstoneBuffDuration 10; WoodBonusHealth 40; WoodBonusHealthHR 40; WoodCapstoneStatGrowth 10; WoodCapstoneTicRate 2; WoodPercentHealth 0.1 |
| 3 | ElectricCapstoneAmp 0.25; ElectricStunDuration 2; FireBurnPercent 1; FireCapstoneBurnPercent 2; FireCapstoneDamagePercent 1.7; FireDamagePercent 1; FireDuration 5; IceBlockDuration 2; IceCapstoneHealthPercentDamage 0.35; IceHealPercent 0.3; IceHealthThreshold 30; NumberOfHexes 2; StoneBuffDuration 8; StoneCapstoneHeal 900; StoneDamageReduction 0.2; WindAS 0.4; WindBuffDuration 5; WindCapstoneAS 1; WindCapstoneBuffDuration 10; WoodBonusHealth 40; WoodBonusHealthHR 40; WoodCapstoneStatGrowth 10; WoodCapstoneTicRate 2; WoodPercentHealth 0.1 |
| 4 | ElectricCapstoneAmp 0.25; ElectricStunDuration 2; FireBurnPercent 1; FireCapstoneBurnPercent 2; FireCapstoneDamagePercent 1.7; FireDamagePercent 1; FireDuration 5; IceBlockDuration 2; IceCapstoneHealthPercentDamage 0.35; IceHealPercent 0.3; IceHealthThreshold 30; NumberOfHexes 2; StoneBuffDuration 8; StoneCapstoneHeal 900; StoneDamageReduction 0.2; WindAS 0.4; WindBuffDuration 5; WindCapstoneAS 1; WindCapstoneBuffDuration 10; WoodBonusHealth 40; WoodBonusHealthHR 40; WoodCapstoneStatGrowth 10; WoodCapstoneTicRate 2; WoodPercentHealth 0.1 |

Champions: Milio (1), Qiyana (2), Neeko (3)

### Juggernaut

Juggernaut champions take less damage as their Health decreases.

| Units | Effect |
|---|---|
| 2-3 | 15% to 25% damage reduction |
| 4-5 | 20% to 35% damage reduction |
| 6 | 35% to 50% damage reduction |

Champions: Sett (2), Warwick (2), Darius (3), Nautilus (3), Nasus (4), Aatrox (5)

### Multicaster

Multicasters cast their Ability 1 additional time. Bonus casts have reduced effectiveness.

| Units | Effect |
|---|---|
| 2 | 55% reduction |
| 3 | 35% reduction |
| 4 | 35% reduction. Attacks grant 5 bonus Mana. |

Champions: Taliyah (2), Twisted Fate (2), Sona (3), Vel'Koz (3)

### Noxus

Noxus champions gain Health, Ability Power and Attack Damage. This is increased by 5% for each different opponent that either you have conquered in combat or is dead. Conquered bonus: (live counter)% / 35%

| Units | Effect |
|---|---|
| 3-4 | 160 HP, 16 AP, 16% AD |
| 5-6 | 300 HP, 30 AP, 30% AD |
| 7-8 | 420 HP, 42 AP, 42% AD |
| 9 | 750 HP, 75 AP, 75% AD |

Champions: Cassiopeia (1), Samira (1), Swain (2), Darius (3), Katarina (3), Mordekaiser (4), Sion (5)

### Piltover

Gain the T-Hex. Every time you lose a player combat, the T-Hex gains Charges. Winning converts the Charges to Power for the T-Hex and grants you loot based on the number of Charges converted.

| Units | Effect |
|---|---|
| 3-5 | Gain the T-Hex. |
| 6 | Gain bonus loot every round. Each loss counts as 2 losses instead. |

Champions: Orianna (1), Vi (2), Ekko (3), Jayce (3), Heimerdinger (5)

### Reaver King

Gangplank uses his cutlass passive when placed in the front 2 rows and his pistol passive when placed in the back 2 rows.

Champions: Gangplank (5)

### Rogue

| Units | Effect |
|---|---|
| 2-3 | When a Rogue falls below 35% Health, they briefly become untargetable and dash to an enemy within 4 hexes (preferring enemy backline). |
| 4 | And a Rogue's damage bleeds their target for 55% bonus magic damage over 2.5 seconds. |

Champions: Graves (1), Qiyana (2), Ekko (3), Katarina (3)

### Shurima

Every 4 seconds, Shurimans heal 5% maximum Health. After 8 seconds, select Shurimans Ascend and gain 20% maximum Health and 30% Attack Speed.

| Units | Effect |
|---|---|
| 2-3 | The strongest Shuriman Ascends |
| 4-5 | All Shurimans Ascend. +30% Ascension bonus |
| 6-8 | Instead, Ascend at combat start. +60% Ascension bonus |
| 9 | Ascend twice, at combat start and after 8 seconds. +111% Ascension bonus |

Champions: Cassiopeia (1), Renekton (1), Naafiri (2), Taliyah (2), Azir (4), Nasus (4), K'Sante (5)

### Slayer

Slayers gain 12% Omnivamp. Slayers deal bonus damage, doubled against units below 66% Health.

| Units | Effect |
|---|---|
| 2-3 | +6% bonus damage |
| 4-5 | +20% bonus damage |
| 6 | +40% bonus damage |

Champions: Kayle (1), Qiyana (2), Quinn (3), Rek'Sai (3), Mordekaiser (4), Aatrox (5)

### Sorcerer

Sorcerers gain bonus Ability Power. When an enemy dies after being damaged by a Sorcerer, they deal a percentage of that enemy's maximum Health to another enemy.

| Units | Effect |
|---|---|
| 2-3 | 25 AP; 7% max HP |
| 4-5 | 50 AP; 10% max HP |
| 6-7 | 90 AP; 12% max HP to 2 enemies |
| 8 | 135 AP; 15% max HP to 2 enemies |

Champions: Malzahar (1), Orianna (1), Swain (2), Taric (3), Vel'Koz (3), Silco (4), Ahri (5)

### Strategist

Combat start: Allies in the front 2 rows gain a Shield for 15 seconds. Allies in the back 2 rows gain Ability Power.

| Units | Effect |
|---|---|
| 2 | 250 Shield; 15 AP |
| 3 | 400 Shield; 25 AP |
| 4 | 600 Shield; 40 AP |
| 5 | 900 Shield; 60 AP |

Champions: Swain (2), Miss Fortune (3), Azir (4), Jarvan IV (4)

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

### Vanquisher

Damage from Vanquisher Abilities can critically strike. Vanquishers gain bonus Critical Strike Chance and Critical Strike Damage.

| Units | Effect |
|---|---|
| 2-3 | 15% Crit , 15% Crit Damage |
| 4-5 | 35% Crit , 35% Crit Damage |
| 6 | 55% Crit , 55% Crit Damage |

Champions: Jhin (1), Ashe (2), Darius (3), Nilah (4), Xayah (4)

### Void

Get a placeable void egg. At the start of combat, it hatches into an unspeakable horror and knocks up adjacent enemies. Each Void star level increases the horror's Health and Ability Power by 25%.

| Units | Effect |
|---|---|
| 3-5 | Void Remora |
| 6-7 | Rift Herald |
| 8 | Baron Nashor |

Champions: Cho'Gath (1), Malzahar (1), Kassadin (2), Rek'Sai (3), Vel'Koz (3), Kai'Sa (4), Bel'Veth (5)

### Wanderer

Ryze's ability changes depending on the Region Portal players voted for at the start of the game.

Champions: Ryze (Ryze) (5), Ryze (RyzeDemacia) (5), Ryze (RyzeBandleCity) (5), Ryze (RyzeFreljord) (5), Ryze (RyzeIonia) (5), Ryze (RyzeNoxus) (5), Ryze (RyzePiltover) (5), Ryze (RyzeShadowIsles) (5), Ryze (RyzeShurima) (5), Ryze (RyzeTargon) (5), Ryze (RyzeZaun) (5), Ryze (RyzeBilgewater) (5), Ryze (RyzeIxtal) (5)

### Zaun

Zaun champions create random chem-mods that only they can use. Champions can be modded once, and mods can be removed by benching or selling the champion.

| Units | Effect |
|---|---|
| 2-3 | Gain 1 chem-mod |
| 4-5 | Gain 2 chem-mods and Overcharge them |
| 6 | Gain 5 chem-mods and Overcharge them |

Champions: Jinx (2), Warwick (2), Ekko (3), Silco (4)

## Traits with no champion (granted by augments, items or mechanics) (4)

### Deadeye

Innate: +1 Range Every 3 seconds, Deadeyes attack the enemy with the highest percent Health and deal bonus damage.

| Units | Effect |
|---|---|
| 2-3 | +20% damage |
| 4-5 | +65% damage |
| 6 | +150% damage |

### Redeemer

Whenever an ally gains a Shield, Senna grants them 6% stacking Attack Speed for the rest of combat.

| Units | Effect |
|---|---|
| 1 | AttackSpeed 0.06 |

### Shadow Isles

After dealing or receiving damage 10 times, Shadow Isles champions gain a Shield for 15 seconds and become Spectral for the rest of combat. Spectral champions gain Mana every second.

| Units | Effect |
|---|---|
| 2-3 | 45% HP Shield, 6 Mana |
| 4-5 | 80% HP Shield, 12 Mana |
| 6 | 115% HP Shield, 18 Mana |

### Yordle

Your units gain 10% Attack Speed per star level. If you have three 3-star champions, your Yordles can become 4-star, which gives their Ability a wacky upgrade! Ties for most items are broken by the Yordle fielded most recently.

| Units | Effect |
|---|---|
| 3-4 | Your strongest 3-star Yordle |
| 5 | Two 3-star Yordles |

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
| Adaptive Helm | Negatron Cloak + Tear of the Goddess | +15 AP, +20 MR, +15 Mana | Combat start: Gain different bonuses based on starting position. Front Two Rows: 35 Armor and Magic Resist. Gain 1 Mana when struck by an attack. Back Two Rows: 15 Ability Power. Gain 10 Mana every 3 seconds. |
| Archangel's Staff | Needlessly Large Rod + Tear of the Goddess | +20 AP, +15 Mana | Combat start: Gain 25 Ability Power every 5 seconds in combat. |
| Bloodthirster | B.F. Sword + Negatron Cloak | +20% AD, +20 MR | Gain 20% Omnivamp. Once per combat at 40% Health, gain a 25% max Health Shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | +25 AP, +20 Mana | Max mana reduced by 10. If the holder gets a takedown within 3 seconds of casting, gain 10 Mana. [Unique - only 1 per champion] |
| Bramble Vest | Chain Vest + Chain Vest | +75 Armor | Negates 50% bonus damage from incoming critical strikes. When struck by any attack, deal 75/100/150 star magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Crownguard | Needlessly Large Rod + Chain Vest | +20 AP, +20 Armor, +100 HP | Combat start: Gain a 30% max Health Shield for 8 seconds. When the shield expires, gain 35 Ability Power. |
| Deathblade | B.F. Sword + B.F. Sword | +66% AD | Perfect peace and calm for the holder - and all who face it. |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | +65 MR | Every 2 seconds, heal 5% max Health. |
| Edge of Night | B.F. Sword + Chain Vest | +10% AD, +20 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, gain 15% bonus Attack Speed. [Unique - only 1 per champion] |
| Evenshroud | Negatron Cloak + Giant's Belt | +20 MR, +150 HP | 30% Sunder enemies within 2 hexes. Gain 20 Armor and Magic Resist for the first 10 seconds of combat. Sunder: Reduce Armor |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | +30 Armor, +30 MR | Gain 10 Armor and 10 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | B.F. Sword + Recurve Bow | +30% AD, +20 AP, +10% AS | Deal 25% more damage to enemies with more than 1600 maximum Health. |
| Guardbreaker | Giant's Belt + Sparring Gloves | +10 AP, +20% AS, +150 HP, +20% Crit | After damaging a Shield, deal 25% more damage for 3 seconds. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | +10 AP, +18% AS | Attacks grant 4% stacking Attack Speed. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | +15 Mana, +20% Crit | Gain 2 effects:15% Attack Damage and 15 Ability Power.15% Omnivamp. Each round, randomly double 1 of these effects. Omnivamp: heal for some of damage dealt |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | +10% AD, +10 AP, +22 Omnivamp | Gain 22% Omnivamp, which also heals the lowest percent Health ally. Healing: (live counter) Omnivamp: heal for some of damage dealt |
| Infinity Edge | B.F. Sword + Sparring Gloves | +35% AD, +35% Crit | Abilities can critically strike. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | +15 AP, +25 MR | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 160% of their maximum Mana. Shred: Reduce Magic Resist |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | +30 AP, +35% Crit | Abilities can critically strike. |
| Last Whisper | Recurve Bow + Sparring Gloves | +15% AD, +25% AS, +20% Crit | Physical damage 30% Sunders the target for 3 seconds. This effect does not stack. [Unique - only 1 per champion] Sunder: Reduce Armor |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | +25 AP, +150 HP | Ability damage deals 1% Burn and 33% Wound to enemies for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Nashor's Tooth | Recurve Bow + Giant's Belt | +30 AP, +10% AS, +150 HP | After casting an Ability, gain 40% Attack Speed for 5 seconds. |
| Night Harvester | Chain Vest + Sparring Gloves | +15% AD, +15 AP, +25 Armor, +20% Crit | Deal 12% bonus damage. After falling below 60% Health, increase this to 25% for the rest of combat. |
| Protector's Vow | Tear of the Goddess + Chain Vest | +20 Armor, +30 Mana | Once per combat at 40% Health, gain a 25% max Health Shield that lasts 5 seconds and gain 20 Armor and 20 Magic Resist. |
| Quicksilver | Sparring Gloves + Negatron Cloak | +30% AS, +30 MR, +20% Crit | Combat start: Gain immunity to crowd control for 18 seconds. [Unique - only 1 per champion] |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | +70 AP | This humble hat can help you make, or unmake, the world itself. |
| Rapid Firecannon | Recurve Bow + Recurve Bow | +35% AS | Deal 8% bonus damage. Gain Range 1 bonus Attack Range. |
| Redemption | Tear of the Goddess + Giant's Belt | +150 HP, +15 Mana | Heal allies within 1 hex for 15% of their missing Health every 5 seconds. They also take 10% less damage for 5 seconds (damage reduction does not stack). Healing: (live counter) |
| Runaan's Hurricane | Negatron Cloak + Recurve Bow | +20% AD, +10% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 50% Attack Damage AD as physical damage. |
| Spear of Shojin | B.F. Sword + Tear of the Goddess | +25% AD, +25 AP, +15 Mana | Attacks grant 5 bonus Mana. |
| Statikk Shiv | Recurve Bow + Tear of the Goddess | +15 AP, +20% AS, +15 Mana | Every 3rd attack deals 30 magic damage and 30% Shreds 4 enemies for 5 seconds. Shred: Reduce Magic Resist |
| Sterak's Gage | B.F. Sword + Giant's Belt | +15% AD, +200 HP | Once per combat at 60% Health, gain 20% max Health and 35% Attack Damage. |
| Sunfire Cape | Chain Vest + Giant's Belt | +20 Armor, +250 HP | Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Tactician's Crown | Spatula + Spatula |  | Your team gains +1 maximum team size. |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | +150 HP, +20% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] |
| Titan's Resolve | Chain Vest + Recurve Bow | +10% AS, +20 Armor | Gain 2% Attack Damage and 2 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 20 Armor and 20 Magic Resist. |
| Warmog's Armor | Giant's Belt + Giant's Belt | +800 HP | Tough as teak, strong as oak, and eternal as petricite. |

### Emblems (24)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Bastion Emblem |  | +150 HP | The holder gains the Bastion trait. [Unique - only 1 per champion.] |
| Bilgewater Emblem | Negatron Cloak + Spatula | +20 MR | The holder gains the Bilgewater trait. [Unique - only 1 per champion] |
| Bruiser Emblem |  | +150 HP | The holder gains the Bruiser trait. [Unique - only 1 per champion.] |
| Challenger Emblem | Spatula + Recurve Bow | +10% AS | The holder gains the Challenger trait. [Unique - only 1 per champion.] |
| Deadeye Emblem |  | +150 HP | The holder gains the Deadeye trait. [Unique - only 1 per champion] |
| Demacia Emblem |  | +150 HP | The holder gains the Demacia trait. [Unique - only 1 per champion.] |
| Freljord Emblem |  | +150 HP | The holder gains the Freljord trait. [Unique - only 1 per champion] |
| Gunner Emblem |  | +150 HP | The holder gains the Gunner trait. [Unique - only 1 per champion.] |
| Invoker Emblem |  | +150 HP | The holder gains the Invoker trait. [Unique - only 1 per champion] |
| Ionia Emblem | Spatula + B.F. Sword | +10% AD | The holder gains the Ionia trait. Ionia Bonus: 25% Attack Speed. [Unique - only 1 per champion] |
| Ixtal Emblem |  | +150 HP | The holder gains the Ixtal trait. [Unique - only 1 per champion] |
| Juggernaut Emblem | Spatula + Chain Vest | +20 Armor | The holder gains the Juggernaut trait. [Unique - only 1 per champion.] |
| Noxus Emblem | Spatula + Giant's Belt | +150 HP | The holder gains the Noxus Trait. [Unique - only 1 per champion] |
| Piltover Emblem |  | +150 HP | The holder gains the Piltover trait. [Unique - only 1 per champion] |
| Rogue Emblem |  | +150 HP | The holder gains the Rogue trait. [Unique - only 1 per champion.] |
| Shadow Isles Emblem |  | +150 HP | The holder gains the Shadow Isles trait. [Unique - only 1 per champion.] |
| Shurima Emblem | Spatula + Needlessly Large Rod | +10 AP | The holder gains the Shurima trait. [Unique - only 1 per champion] |
| Slayer Emblem |  | +150 HP | The holder gains the Slayer trait. [Unique - only 1 per champion] |
| Sorcerer Emblem | Spatula + Tear of the Goddess | +15 Mana | The holder gains the Sorcerer trait. [Unique - only 1 per champion.] |
| Strategist Emblem |  | +150 HP | The holder gains the Strategist trait. [Unique - only 1 per champion] |
| Targon Emblem |  | +150 HP | The holder gains the Targon trait. [Unique - only 1 per champion.] |
| Vanquisher Emblem | Sparring Gloves + Spatula | +20% Crit | The holder gains the Vanquisher trait. [Unique - only 1 per champion] |
| Void Emblem |  | +150 HP | The holder gains the Void trait. [Unique - only 1 per champion] |
| Zaun Emblem |  | +150 HP | The holder gains the Zaun trait. [Unique - only 1 per champion] |

### Artifacts (Ornn) (14)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Visage |  | +25 MR, +450 HP | Heal 2.5% max Health every second. |
| Blacksmith's Gloves |  |  | Each round: Equip 2 random Ornn Artifacts. [Consumes 3 item slots.] |
| Death's Defiance |  | +10% AD, +25% AS, +30 Armor | Gain 25% Omnivamp. 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] Omnivamp: heal for some of damage dealt |
| Deathfire Grasp |  | +50 AP, +30 Mana | Combat start: Blast the current target for 20% of their max Health as magic damage. For the next 10 seconds, deal 25% more damage. Damage dealt to the blasted target is increased to 35%. [Unique - only 1 per champion] |
| Eternal Winter |  | +20 Armor, +200 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead (Cooldown: 15 seconds). [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Gold Collector |  | +20% AD, +30% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 40% chance to drop goldCoins 1 gold. [Unique - only 1 per champion] |
| Hullcrusher |  | +30% AS, +35 Armor, +35 MR | Combat start: If there are no adjacent allies, gain 550 Health. |
| Infinity Force |  | +25% AD, +25 AP, +25% AS, +25 Armor, +25 MR, +250 HP, +25 Mana | Tons of EVERYTHING! |
| Manazane |  | +15% AD, +10 AP, +15% AS, +15 Mana | After casting the first time in combat, gain 120 Mana over 5 seconds. [Unique - only 1 per champion] |
| Obsidian Cleaver |  | +250 HP | Damage dealt 30% Shreds and 30% Sunders enemies for 15 seconds. Your team gains 8% Attack Damage and 8 Ability Power. ​​[Support item] [Unique - only 1 per champion] Shred: Reduce Magic Resist Sunder: Reduce Armor |
| Randuin's Omen |  | +250 HP | Combat start: Grant Armor 30 Armor and MR 30 Magic Resistance to the holder and adjacent allies. ​​[Support item] |
| Sniper's Focus |  | +15% AD, +15 AP, +15% AS | Gain Range 1 Attack Range. Damage is increasedy by 8% for each hex between the holder and their target. |
| Trickster's Glass |  | +10% AS, +10 Armor, +10 MR, +15% Crit | Summon a clone with 70% max Health and +30% max Mana. You cannot equip items to the clone. The clone benefits from active traits [Unique - only 1 per champion] |
| Zhonya's Paradox |  | +45 AP, +30 Armor, +30 MR | Once per combat at 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items (38)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Absolution |  | +200 HP, +15 Mana | Heal allies within 2 hexes for 25% of their missing Health every 5 seconds. They also take 10% less damage for 5 seconds (damage reduction does not stack). Healing: (live counter) |
| Blessed Bloodthirster |  | +40% AD, +20 MR | Gain 40% Omnivamp. Once per combat: At 40% Health, gain a 40% max Health Shield that lasts up to 5 seconds. Omnivamp: heal for some of damage dealt |
| Blue Blessing |  | +50 AP, +20 Mana | Max mana reduced by 10. If the holder gets a takedown within 3 seconds of casting, gain 20 Mana. [Unique - only 1 per champion] |
| Brink of Dawn |  | +30% AD, +30 Armor | Once per combat: At 60% Health, briefly become untargetable and shed negative effects. Then, heal 100% missing health and gain 85% bonus Attack Speed. [Unique - only 1 per champion] |
| Bulwark's Oath |  | +40 Armor, +30 Mana | Once per combat at 40% Health, gain a 50% max Health Shield that lasts 10 seconds and gain 60 Armor and 60 Magic Resist. |
| Covalent Spark |  | +15 AP, +25 MR | 30% Shred enemies within 3 hexes. When enemies cast an Ability, deal magic damage equal to 250% of their maximum Mana. Also, regenerate 1.5% maximum Health per second. [Direct damage item] Shred: Reduce Magic Resist |
| Demonslayer |  | +50% AD, +40 AP, +10% AS | Deal 60% more damage to enemies with more than 1600 maximum Health. |
| Dragon's Will |  | +130 MR | Every 2 seconds, heal 10% max Health. |
| Dvarapala Stoneplate |  | +30 Armor, +30 MR | Gain 20 Armor and 20 Magic Resist for each enemy targeting the holder. Also, regenerate 2% maximum Health each second. |
| Equinox |  | +30 MR, +400 HP | 30% Sunder enemies within 2 hexes. Gain 50 Armor and Magic Resist for the first 15 seconds of combat. Sunder: Reduce Armor |
| Eternal Whisper |  | +45% AD, +25% AS, +55% Crit | Physical damage 30% Sunders the target for the rest of combat. This effect does not stack. [Unique - only 1 per champion] Sunder: Reduce Armor |
| Fist of Fairness |  | +15 Mana, +20% Crit | Gain 2 effects:50% Attack Damage and 50 Ability Power.30% Omnivamp. Omnivamp: heal for some of damage dealt |
| Glamorous Gauntlet |  | +55 AP, +75% Crit | Abilities can critically strike. |
| Guinsoo's Reckoning |  | +10 AP, +20% AS | Attacks grant 8% stacking Attack Speed. |
| Hextech Lifeblade |  | +10% AD, +40 AP, +35 Omnivamp | Gain 35% Omnivamp. Damage dealt heals the lowest percent Health ally for the same amount. Healing: (live counter) |
| Jak'sho the Protean |  | +15 AP, +30 MR, +15 Mana | Combat start: The holder gains two effects, regardless of starting position. Front Two Rows: 45 Armor and Magic Resist. Gain 1 Mana when struck by an attack. Back Two Rows: 45 Ability Power. Gain 15 Mana every 3 seconds. |
| Luminous Deathblade |  | +1.25 AD | It glows in the presence of enemies. Or friends. Or anything alive, really. |
| Midnight Reaper |  | +25% AD, +25 AP, +50 Armor, +20% Crit | Deal 20% bonus damage. After falling below 80% Health, increase this to 45% for the rest of combat. |
| More More-ellonomicon |  | +40 AP, +150 HP | Ability damage deals 3% Burn and 33% Wound to enemies for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: Reduces healing received |
| Quickestsilver |  | +50% AS, +40 MR, +40% Crit | Combat start: Gain immunity to crowd control for 30 seconds. [Unique - only 1 per champion] |
| Rabadon's Ascended Deathcap |  | +140 AP | It's witnessed - and unleashed - miracles and calamities both. |
| Rapid Lightcannon |  | +50% AS | Deal 15% bonus damage. Gain Range 2 bonus Attack Range. |
| Rascal's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] |
| Rosethorn Vest |  | +140 Armor | Negates 70% bonus damage from incoming critical strikes. When struck by any attack, deal 125/175/225 star magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Royal Crownshield |  | +50 AP, +40 Armor, +100 HP | Combat start: Gain a 40% max Health Shield for 8 seconds. When the shield expires, gain 50 Ability Power. |
| Runaan's Tempest |  | +35% AD, +20% AS, +20 MR | Attacks fire a bolt at a nearby enemy, dealing 100% Attack Damage AD as physical damage. |
| Spear of Hirana |  | +45% AD, +45 AP, +15 Mana | Attacks grant 10 bonus Mana. |
| Statikk's Favor |  | +50 AP, +20% AS, +15 Mana | Every 3rd attack deals 70 magic damage and 30% Shreds 8 enemies for 5 seconds. Shred: Reduce Magic Resist |
| Sterak's Megashield |  | +30% AD, +400 HP | Once per combat at 60% Health, gain 30% max Health and 55% Attack Damage. |
| Sunlight Cape |  | +20 Armor, +300 HP | Every 1.5 seconds, deal 2% Burn and 33% Wound to an enemy within 3 hexes for 30 seconds. [Unique - only 1 per champion] Burn: Deals a percent of the target's maximum Health as true damage every second Wound: reduces healing received |
| The Baron's Gift |  | +55 AP, +20% AS, +200 HP | After casting an Ability, gain 60% Attack Speed for 8 seconds. |
| Titan's Vow |  | +30% AS, +35 Armor | Gain 3% Attack Damage and 3 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 50 Armor and 50 Magic Resist. |
| Urf-Angel's Staff |  | +50 AP, +15 Mana | Combat start: Gain 35 Ability Power every 4 seconds in combat. |
| Virtue of the Martyr |  | +250 HP | Every 5 seconds, heal your team for 6.5% of their max Health. When the holder dies, the healing increases to 13% max Health and continues for 10 seconds. Healing: (live counter) ​​[Support item] |
| Warmog's Pride |  | +1300 HP | Regenerate 1% maximum Health per second. |
| Willbreaker |  | +30 AP, +40% AS, +150 HP, +20% Crit | After damaging a Shield, deal 50% more damage for 3 seconds. |
| Zenith Edge |  | +65% AD, +75% Crit | Abilities can critically strike. |
| Zz'Rot Portal |  | +250 HP | Combat start: Summon a large Voidspawn that forces nearby enemies to attack it. ​​[Support item] |

### Support items (6)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Aegis of the Legion |  | +250 HP | Combat start: Grant 30% Attack Speed and 15 Armor and Magic Resist to the holder, adjacent allies, and all allies behind them for 12 seconds. ​​[Support item] |
| Banshee's Veil |  | +250 HP | Combat start: Grant the holder and allies within 1 hexes in the same row immunity to crowd control and 15% Attack Speed for 18 seconds. [Support item] [Unique - only 1 per champion] |
| Chalice of Power |  | +250 HP | Combat start: Grant 25 Ability Power and 10 Mana to the holder and allies within 2 hexes in the same row. ​​[Support item] |
| Locket of the Iron Solari |  | +250 HP | Combat start: The holder and allies within 2 hexes in the same row gain a 250 Shield, 20 Armor, and 20 Magic Resist for 20 seconds. ​​[Support item] |
| Shroud of Stillness |  | +250 HP | Combat start: Shoot a beam that 30% Mana Reaves enemies. Your team gains 60 Health. ​​[Support item] [Unique - only 1 per champion] Mana Reave: increase maximum Mana until the next cast |
| Zeke's Herald |  | +250 HP | Combat start: Grant AS 30% Attack Speed to the holder and allies within 2 hexes in the same row. ​​[Support item] |

### Set-specific and other items (16)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Baron Nashor's Head |  |  | The terror from the Void was no match for the ingenuity, firepower, and weapons of dubious legality from Piltover. |
| Blue Buff |  | +10 AP, +40 Mana | Max mana reduced by 10. If the holder gets a takedown within 3 seconds of casting, gain 10 Mana. [Unique - only 1 per champion] |
| Charges |  |  | Start with [TooltipStartingCharge: unknown] Charge. Gain Charges each time you lose. The larger your loss streak, the more Charges you gain per loss. When you win, all Charges are converted to Power for the T-Hex. Current loss streak: (live counter) (doubled in Hyper Roll) Charges granted next loss: +(live counter) Current loot value on victory: (live counter) gold [This does not count as an item.] |
| Crown of Demacia |  | +40% AS, +400 HP | If the holder begins combat in the front 2 rows, regenerate 2-14% maximum Health every 2 seconds (based on stage). If they start in the back 2 rows, gain 1-7% AD Attack Damage and AP Ability Power every 2 seconds instead. If the holder of this item dies, you instantly lose the fight. You can remove this item by benching the holder. |
| Goldinator |  |  | Apex Turret's damage executes enemies under 12% Health. Executions have a 100% chance to turn enemies into gold. 1 Count: 1 gold 2 Count: 2 gold 3 Count: 4 gold |
| Goldmancer's Staff |  | +30 AP | Grant AP 1 Ability Power per goldCoins gold in your bank (up to goldCoins 30 gold) and a 40% chance to drop goldCoins 2 gold on enemy kill. Gold generated this game: (live counter) |
| Mechano-Swarm |  |  | Apex Turret fires rockets in a wide cone every 3 attacks. Each rocket deals magic damage, Burns, and 33% Wounds for 10 seconds. 1 Count: 50 magic damage; 1% Burn 2 Count: 200 magic damage; 1% Burn 3 Count: 200 magic damage; 2% Burn Burn: Deal a percentage of the target's max Health as true damage every second Wound: Reduce healing |
| Mogul's Mail |  | +150 HP | Grants 1 Armor, 1 Magic Resist, and 6 Health when taking damage, stacking up to 40 times. At full stacks, grant goldCoins 2 gold. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Needlessly Big Gem |  | +250 HP | If the holder is alive after 15 seconds of combat, gain goldCoins 1 gold per 2 living allies and grant living allies 30% bonus damage. ​​[Support item] |
| Power |  |  | Gain Power by winning combats after gaining Charges. More Power gives more stats. [This does not count as an item.] |
| Refractor Beam |  |  | Apex Turret's Ability fires multiple laser beams that deal increased damage. It gains 100% of all Mana that Heimerdinger generates from traits, items, and augments. The turret's Ability can critically strike if Heimerdinger's Ability can, and it shares critical strike chance with Heimerdinger. 1 Count: 3 beams, 20% increased damage 2 Count: 3 beams, 50% increased damage 3 Count: 3 beams, 100% increased damage |
| Repair-o-matic |  |  | When the Apex Turret would die, it instead becomes untargetable and repairs itself over 7 seconds if Heimerdinger is still alive. The Apex Turret gains bonus Armor and Magic Resist. 1 Count: 10 Armor and Magic Resist 2 Count: 50 Armor and Magic Resist 3 Count: 100 Armor and Magic Resist |
| Shrink Module |  |  | Apex Turret's damage shrinks all enemies hit, Shredding and Sundering them for 4 seconds. 1 Count: 20% Shred and Sunder 2 Count: 30% Shred and Sunder 3 Count: 50% Shred and Sunder Shred: Reduce Magic Resist Sunder: Reduce Armor |
| The Darkin Blade |  | +100% AD, +100 AP, +1000 HP | If the holder of this item gets a takedown, they will be consumed and Aatrox will be reborn! |
| Zephyr |  | +250 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. Your team gains 8% Attack Speed. [Support item] [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| Zz'Rot Portal |  | +10% AS, +150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. The Voidspawn has massively reduced effectiveness when created by a summoned unit. Taunt: enemies that are able and in range must attack the taunter |

### Consumables (17)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Adaptive Implant |  |  | Mod: While above 50% Health, gain 40% Attack Damage and Ability Power. While below, gain 40% Omnivamp. Overcharge: Always gain both effects, and their power is increased by 60%. Omnivamp: Heal for percentage of damage dealt |
| Champion Duplicator |  |  | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Eagle's Antimagic |  |  | Your team gains temporary magic resist. |
| Eagle's Armor |  |  | Your team gains temporary armor. |
| Golden Item Remover |  |  | Use on a champion to unequip all items as many times as you want |
| Hextech Exoskeleton |  |  | Mod: Deal 12% more damage. Take 12% less damage. Every 5 seconds, heal for 20% maximum Health. Overcharge: You are immune to crowd control, and the heal is increased to 30% maximum Health. |
| Lesser Champion Duplicator |  |  | Use on a champion of Tier 3 or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Loaded Dice |  |  | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover |  |  | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Masterwork Upgrade |  |  | Use on a unit to open an armory containing Radiant versions of that unit's craftable completed items. Upgrade the item you choose to its Radiant equivalent, and unequip it. |
| Reforger |  |  | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Robotic Arm |  |  | Mod: Attacks have a 40% chance to trigger 1 additional attack. Overcharge: Attacks and Abilities deal 30% bonus true damage. |
| Scroll of Knowledge |  |  | Use on a unit to open a special armory that offers every available emblem that matches that unit's inherent traits. |
| Shimmer Injector |  |  | Mod: Gain 44% Attack Speed. The first time you would be reduced to 1 Health, increase the Attack Speed bonus by 100% and heal for 44% maximum Health. Overcharge: Instead increase the Attack Speed bonus by 200% and heal for 100% maximum Health. |
| Target Dummy |  |  | Use on a champion to spawn a movable target dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |
| Unstable Chemtank |  |  | Mod: Gain 40% maximum Health and explode on death, dealing 25% of your maximum Health as magic damage to enemies within 2 hexes. Overcharge: Explosion radius is increased to 3 hexes. |
| Virulent Bioware |  |  | Mod: For 3 seconds after ability cast, dealing damage applies a plague causing the target and enemies within 1 hex to take 20% more damage for 5 seconds. Overcharge: Plague duration increases to 10 seconds and plague deals 6% of the target's maximum Health per second as magic damage. |

## Augments (333)

Tier comes from the augment icon file name (I/1 = Silver, II/2 = Gold, III/3 = Prismatic); "unknown" where the name carries no marker. The list is the set's augment pool as stored in the source data and can include entries that were disabled or rotated during the set.

| Name | Tier | Effect |
|---|---|---|
| A Cut Above | Gold | Gain a Deathblade. Champions holding Deathblade have a 25% chance to drop 1 gold on kill. |
| Adrenaline Rush | Gold | Your Juggernauts deal 10% more damage, increased to 20% for the rest of combat when they fall below 60% Health. Gain 2 Juggernauts. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| All Natural I | Silver | Your champions with no items equipped gain 120 Health and heal for 1% of their max Health each second. |
| All Natural II | Gold | Your champions with no items equipped gain 150 Health and heal for 2% of their max Health each second. |
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
| Big Grab Bag | Gold | Gain 2 random components, 1 gold, and a Reforger. |
| Bilgewater Crest | Gold | Gain a Bilgewater Emblem and a Twisted Fate. |
| Bilgewater Crown | Prismatic | Gain a Bilgewater Emblem, a Night Harvester, and a Nautilus. |
| Binary Airdrop | Prismatic | Your champions equipped with 2 items temporarily gain a random completed item at the start of combat. |
| Birthday Present | Prismatic | Gain 2 gold and a 2-star champion every time you level up. The champion's tier is your level minus 4 (min: Tier 1). |
| Blinding Speed | Prismatic | Gain a Rapid Firecannon, a Guinsoo's Rageblade, a Recurve Bow, and a Magnetic Remover. |
| Blood Money | Silver | Gain 1 gold per 3 Health your Tactician loses. |
| Branching Out | Silver | Gain a random Emblem and a Reforger. |
| Branching Out | Silver | Gain a random Emblem and a Reforger. |
| Bronze Ticket | Silver | Every 4 shop refreshes, gain a free refresh. |
| Bronze Ticket | Silver | Every 4 shop refreshes, gain a free refresh. |
| Bruiser Crest | Gold | Gain a Bruiser Emblem and a Vi. |
| Bruiser Crown | Prismatic | Gain a Bruiser Emblem, a Redemption, and a Rek'Sai. |
| Bruiser Heart | Silver | Your team counts as having 1 additional Bruiser. Gain a Vi. |
| Built Different II | Gold | Your units with no Traits active gain 250-500 Health and 40-60% Attack Speed (based on current Stage). |
| Built Different III | Prismatic | Your units with no Traits active gain 300-600 Health and 50-70% Attack Speed (based on current Stage). |
| Buried Treasures I | Silver | Gain a random item component at the start of the next 2 rounds (including this round). |
| Buried Treasures I | Silver | Gain a random item component at the start of the next 2 rounds (including this round). |
| Buried Treasures II | Gold | Gain a random item component at the start of the next 3 rounds (including this round). |
| Buried Treasures II | Gold | Gain a random item component at the start of the next 3 rounds (including this round). |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 6 rounds (including this round). |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 6 rounds (including this round). |
| Capricious Forge | Gold | Gain a Blacksmith's Gloves, which equips two temporary Artifacts each round. |
| Caretaker's Ally | Silver | Each time you level up, gain the same random Tier 2 champion. Gain one now. |
| Caretaker's Ally | Silver | Each time you level up, gain the same random Tier 2 champion. Gain one now. |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4 - Component Anvil Level 6 - Completed Item Anvil Level 7 - Open a Radiant Item armory |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4 - Component Anvil Level 6 - Completed Item Anvil Level 7 - Open a Radiant Item armory |
| Caretaker's Favor | Gold | Gain a Component Anvil when you reach level 5, 6, 7, and 8. |
| Caretaker's Favor | Gold | Gain a Component Anvil when you reach level 5, 6, 7, and 8. |
| Challenger Crest | Gold | Gain a Challenger Emblem and a Warwick. |
| Challenger Crown | Prismatic | Gain a Challenger Emblem, a Hand of Justice, and a Naafiri. |
| Challenger Heart | Silver | Your team counts as having 1 additional Challenger. Gain a Warwick. |
| Chemtech Enhancements | Gold | Your units with Zaun's chem-mods gain 500 Health. Allies that start combat adjacent to them gain 250 Health. Gain 2 Zaun units. |
| Combat Caster | Gold | Your units gain 90-170 Shield (based on current Stage) for 6 seconds after casting their ability. |
| Component Buffet | Silver | Whenever you would get a random component, instead gain a Component Anvil. Gain a random component. |
| Consistency | Silver | Gain double win and loss streak gold. |
| Contagion | Gold | Combat start: Infect the highest Health enemy, causing them to take 18% more damage. Every 5 seconds or on death, the infection spreads to 2 nearby enemies. |
| Cruel Pact | Prismatic | Buying XP costs 5 Health instead of gold. Heal 2 Health before each player combat. |
| Cursed Crown | Prismatic | Gain +2 maximum team size, but take 100% increased player damage. |
| Cutting Corners | Silver | Leveling up costs 3 XP less. You can now reach Level 10. |
| Cutting Corners | Silver | Leveling up costs 3 XP less. You can now reach Level 10. |
| Cybernetic Bulk I | Silver | Your champions holding an item gain 225 Health. |
| Cybernetic Bulk II | Gold | Your champions holding an item gain 300 health. |
| Cybernetic Bulk III | Prismatic | Your champions holding an item gain 500 health. |
| Cybernetic Leech I | Silver | Your units holding an item gain 80 Health and 10% Omnivamp. |
| Cybernetic Leech II | Gold | Your units holding an item gain 120 Health and 15% Omnivamp. |
| Cybernetic Leech III | Prismatic | Your units holding an item gain 250 Health and 20% Omnivamp. |
| Dedication | Gold | The first time you field at least 4 distinct units of the same trait in player combat, gain an Emblem for that trait. |
| Defensive Dash | Gold | Combat start and on takedown: Your Challenger units gain 100-250 Shield (based on current Stage) for 2.5 seconds. Gain 2 Challenger units. |
| Demacia Crest | Gold | Gain a Demacia Emblem and a Galio. |
| Demacia Crown | Prismatic | Gain a Demacia Emblem, a Recurve Bow, and a Quinn. |
| Demonflare | Gold | Gain a Swain. Your strongest Swain deals 2.5% increased damage per 100 max Health. |
| Double Trouble II | Gold | When you field exactly 2 copies of a champion, they both gain 30% Attack Damage and 30 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Double Trouble III | Prismatic | When you field exactly 2 copies of a champion, they both gain 40% Attack Damage and 40 Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Dueling Gunners | Gold | Your Gunner units gain 8% Attack Speed, plus 2% per Gunner stack. Gain 2 Gunners. |
| Early Education | Gold | Your units permanently gain 1 Ability Power whenever they kill an enemy. Units start with 15 bonus Ability Power. |
| Endless Hordes | Prismatic | Gain +3 maximum team size, but your units can only hold 1 item and their total health is reduced by 15%. Gain 4 gold. |
| Endless Hordes + | Prismatic | Gain +3 maximum team size, but your units can only hold 1 item and their total health is reduced by 15%. Gain 14 gold. |
| Endurance Training | Gold | Your units permanently gain 18 Health every time they kill an enemy. Units start with 100 bonus Health. |
| Escort Quest | Gold | Gain a Training Dummy. Each round, if it survives combat, gain 3 gold. |
| Final Ascension | Prismatic | Your units deal 15% more damage. After 15 seconds, this effect triples. |
| Final Grab Bag | Silver | Gain a random component, 10 gold, and a Reforger. |
| Final Grab Bag II | Gold | Gain 2 random components, 12 gold, and a Magnetic Remover. |
| Final Reserves | Prismatic | The first time you would be eliminated or reduced to 1 Health, instead remain alive. Then, at the beginning of your next planning phase gain 60 XP and set your gold to 66. Excess gold is converted to XP. |
| Freljord Heart | Gold | Your team counts as having 1 additional Freljord. Gain an Ashe. |
| Freljord Soul | Prismatic | Your team counts as having 1 additional Freljord. Gain a Redemption and an Ashe. |
| Frequent Flier | Gold | After you refresh your shop 8 times, your refreshes cost 1 gold for the rest of the game. (Refreshes this game: (live counter)) |
| Gargantuan Resolve | Gold | Gain a Titan's Resolve. Your Titan's Resolves can continue stacking to 40 instead of 25. |
| Giant Grab Bag | Prismatic | Gain 3 random components, 4 gold, and a Lesser Champion Duplicator. |
| Gifts from the Fallen | Gold | Your units gain 3% Attack Damage, 3 Ability Power, 3 Armor, and 3 Magic Resist. Each time an allied unit dies, gain these stats again. |
| Golden Ticket | Prismatic | Each time your Shop is refreshed, you have a 45% chance to gain a free refresh. |
| Gotta Go Fast! | Silver | Your units generate 10% more Mana and move 35% faster. |
| Gotta Go Fast! II | Gold | Your units generate 20% more Mana and move 60% faster. |
| Gotta Go Fast!!! III | Prismatic | Your units generate 30% more Mana and move 100% faster. |
| Gunner Crest | Gold | Gain a Gunner Emblem and a Jinx. |
| Gunner Crown | Prismatic | Gain a Gunner Emblem, a Runaan's Hurricane, and a Jayce. |
| Gunner Heart | Silver | Your team counts as having 1 additional Gunner. Gain a Jinx. |
| Harmacist I | Silver | Your units heal for 10% of the damage they deal, and they convert 20% of excess healing to true damage on their next attack. |
| Harmacist II | Gold | Your units heal for 15% of the damage they deal, and they convert 25% of excess healing to true damage on their next attack. |
| Harmacist III | Prismatic | Your units heal for 25% of the damage they deal, and they convert 30% of excess healing to true damage on their next attack. |
| Healing Orbs I | Silver | When an enemy dies, the nearest allied unit is healed for 250. |
| Healing Orbs II | Gold | When an enemy dies, the nearest allied unit is healed for 500. |
| Hedge Fund | Prismatic | Gain 23 gold. Your maximum interest is increased to 10. |
| Hedge Fund | Prismatic | Gain 23 gold. Your maximum interest is increased to 10. |
| Hedge Fund+ | Prismatic | Gain 32 gold. Your maximum interest is increased to 10. |
| Hedge Fund++ | Prismatic | Gain 44 gold. Your maximum interest is increased to 10. |
| Hustler | Gold | You earn no interest, but gain 3 gold at the start of every player combat round. |
| Idealism | Gold | Gain a Hand of Justice. Champions holding Hand of Justice deal 12% increased damage. |
| Impenetrable Bulwark | Prismatic | Gain a Bramble Vest, a Dragon's Claw, a Giant's Belt, and a Magnetic Remover. |
| Inconsistency | Silver | If your streak is less than 3, gain 2 gold. |
| Indomitable Will | Silver | When your units score a takedown they shed all negative effects and become immune to crowd control for 10 seconds. |
| Infernal Contract | Prismatic | Your max level is 7. Gain 75 gold. |
| Infusion | Gold | Your units restore 20 Mana every 6 seconds. |
| Invoker Crest | Gold | Gain an Invoker Emblem and a Soraka. |
| Invoker Crown | Prismatic | Gain a Invoker Emblem, a Jeweled Gauntlet, and a Karma. |
| Invoker Heart | Silver | Your team counts as having 1 additional Invoker. Gain a Soraka. |
| Ionia Crest | Gold | Gain a Ionia Emblem and a Sett. |
| Ionia Crown | Prismatic | Gain an Ionia Emblem, a Guardbreaker, and a Karma. |
| Iron Assets | Silver | Gain a Component Anvil and 4 gold. |
| It Pays To Learn | Silver | Gain 10 XP and 8 gold. |
| It Pays to Learn II | Gold | Gain 16 XP and 12 gold. |
| It Pays to Learn III | Prismatic | Gain 24 XP and 16 gold. |
| Item Grab Bag I | Silver | Gain 1 random completed item. |
| Item Grab Bag II | Gold | Gain a random completed item, a random component, and 1 gold. |
| Item Grab Bag III | Prismatic | Gain 2 random completed items and 4 Gold. |
| Ixtal Heart | Gold | Your team counts as having 1 additional Ixtal. Gain a Qiyana. |
| Ixtal Soul | Prismatic | Your team counts as having 1 additional Ixtal. Gain a Crownguard and a Neeko. |
| Jeweled Lotus I | Silver | Combat start: Your strongest unit gains 40% Critical Strike Chance and their abilities can critically strike. |
| Jeweled Lotus I | Silver | Combat start: Your strongest unit gains 40% Critical Strike Chance and their abilities can critically strike. |
| Jeweled Lotus II | Gold | Your units' Abilities can critically strike. Your units gain 15% Critical Strike chance. |
| Jeweled Lotus II | Gold | Your units' Abilities can critically strike. Your units gain 15% Critical Strike chance. |
| Jeweled Lotus III | Prismatic | Your units' Abilities can critically strike. Your units gain 45% Critical Strike chance. |
| Jeweled Lotus III | Prismatic | Your units' Abilities can critically strike. Your units gain 45% Critical Strike chance. |
| Job Well Done | Gold | Gain a Completed Item Anvil and 1 random component. |
| Job's Done | Silver | Gain 1 Component Anvil and 1 random component. |
| Juggernaut Crest | Gold | Gain an Juggernaut Emblem and a Sett. |
| Juggernaut Crown | Prismatic | Gain an Juggernaut Emblem, a Bloodthirster, and a Darius. |
| Juggernaut Heart | Silver | Your team counts as having 1 additional Juggernaut. Gain a Sett. |
| Know Your Enemy | Gold | Your units deal 12% more damage, increased to 18% if you have any of your opponent's traits active. |
| Knowledge Download 1+ | Silver | Gain 20 XP. |
| Knowledge Download I | Silver | Gain 12 XP. |
| Knowledge Download II | Gold | Gain 22 XP. |
| Knowledge Download II+ | Gold | Gain 28 XP. |
| Knowledge Download III | Prismatic | Gain 36 XP. |
| Knowledge Download III+ | Prismatic | Gain 36 XP. |
| Large Forge | Prismatic | Gain an Artifact Anvil and a Component Item Anvil. |
| Last Stand | Gold | The first time you would be eliminated or reduced to 1 Health, instead remain alive. After this effect triggers, your units permanently gain 180 Health, 18 Armor and Magic Resist, and 18% Omnivamp. |
| Lategame Specialist | Silver | When you reach Level 9, gain 44 gold. |
| Latent Forge | Silver | Gain an Artifact Anvil after 8 player combats. |
| Latent Forge | Silver | Gain an Artifact Anvil after 8 player combats. |
| Level Up! | Prismatic | When you buy XP, gain an additional 3. Gain 2 immediately. You can now reach level 10. |
| Level Up! | Prismatic | When you buy XP, gain an additional 3. Gain 2 immediately. You can now reach level 10. |
| Library Card | Gold | Gain a Tome of Traits and a Component Anvil. |
| Living Forge | Prismatic | Gain an Artifact Anvil now and after every 10 player combats. |
| Living Forge | Prismatic | Gain an Artifact Anvil now and after every 10 player combats. |
| Long Distance Pals II | Gold | Combat start: Your two furthest units form a bond, sharing 22% of their Armor, Magic Resist, Attack Damage, and Ability Power with each other. |
| Loving Invocation | Gold | Your team gains 2 Ability Power until the end of combat each time your Invokers cast their Abilities. Gain 2 Invokers. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 3 Sparring Gloves. |
| Magic Wand | Gold | Gain a Needlessly Large Rod. Your units gain 18 Ability Power. |
| Mana Burn | Gold | Gain a Shroud of Stillness. All enemies take 1.5% of their maximum Health as true damage each second until the first time they cast their Ability. |
| March of Progress | Prismatic | Gain 3 XP now, and bonus XP equal to your level at the start of every player combat round. You can no longer use gold to level up. |
| Martyr | Gold | Whenever one of your units dies, all allies heal for 10% of their maximum Health. |
| Masterful Job | Prismatic | Gain an Artifact Anvil and a Completed Item Anvil. |
| Medium Forge | Gold | Gain a Completed Item Anvil and 1 Gold. |
| Metabolic Accelerator | Gold | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Metabolic Accelerator | Gold | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Missed Connections | Silver | Gain a copy of each Tier 1 champion. |
| Money Money Money! | Prismatic | Gain 25 gold. In 3 turns, gain 25 gold again. |
| Money Money! | Gold | Gain 16 gold. In 3 turns, gain 16 gold again. |
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
| Overwhelming Force | Prismatic | Gain a Deathblade and an Infinity Edge, a BF Sword, and a Magnetic Remover. |
| Pandora's Bench | Silver | Gain 2 gold. At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Pandora's Items | Silver | Gain 1 random item component(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items | Silver | Gain 1 random item component(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items II | Gold | Gain 1 random completed item(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items II | Gold | Gain 1 random completed item(s). Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items III | Prismatic | Gain a random Radiant item. Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Pandora's Items III | Prismatic | Gain a random Radiant item. Round start: items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Partial Ascension | Silver | After 15 seconds of combat, your units deal 30% more damage. |
| Parting Gifts | Gold | When a unit dies, they pass a temporary copy of one of their items to the nearest ally with open slots and shield them for 50% of their maximum Health. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. You can now reach Level 10. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. You can now reach Level 10. |
| Perfected Repetition | Gold | Each time your Multicasters cast their Ability, they gain 9 Ability power (max: 90) until the end of combat. Gain 2 Multicasters. |
| Petricite Shackles | Gold | Your Demacians deal 12% more damage, increased to 18% against Noxians and enemies with 100 or more maximum Mana. Gain 2 Demacians. |
| Phreaky Friday | Prismatic | Gain an Infinity Force. After 5 player combats, gain another. |
| Phreaky Friday + | Prismatic | Gain an Infinity Force. After 3 player combats, gain another. |
| Piltover Heart | Gold | Your team counts as having 1 additional Piltover. Gain a Vi. |
| Piltover Soul | Prismatic | Your team counts as having 1 additional Piltover. Gain a Giant Slayer and a Jayce. |
| Portable Forge | Gold | Open an Armory and choose 1 of 2 unique Artifacts. |
| Portable Forge | Gold | Open an Armory and choose 1 of 2 unique Artifacts. |
| Portable Forge+ | Gold | Open an Armory and choose 1 of 3 unique Artifacts. |
| Portable Forge++ | Gold | Open an Armory and choose 1 of 4 unique Artifacts. |
| Pumping Up I | Silver | Your units gain 8% Attack Speed. Each round, increase this by 0.5%. (current Attack Speed: (live counter)%) |
| Pumping Up I | Silver | Your units gain 8% Attack Speed. Each round, increase this by 0.5%. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your units gain 8% Attack Speed. Each round, increase this by 1%. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your units gain 8% Attack Speed. Each round, increase this by 1%. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your units gain 12% Attack Speed. Each round, increase this by 2%. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your units gain 12% Attack Speed. Each round, increase this by 2%. (current Attack Speed: (live counter)%) |
| Radiant Relics | Prismatic | Open an Armory and choose 1 of 5 unique Radiant items. Gain a Magnetic Remover. |
| Ravenous Hunter | Gold | Gain a Warwick. When your strongest Warwick takes or deals damage, he gains 4% Attack Damage and 4 Ability Power, stacking up to 55 times. His passive healing is replaced with magic damage on his attacks. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 Tier higher. Gain 2 Magnetic Removers. |
| Red Buff | Silver | Your units' attacks burn their targets for 5% of their maximum Health over 5 seconds and reduce their targets' healing received by 50%. |
| Rejuvenating Flames | Gold | Allies heal for 8% of their max Health every 5 seconds. Allies that started combat in an elemental hex heal for 15% instead. Gain 2 Ixtal champions. |
| Return on Investment | Gold | After you refresh your shop 18 times, gain a Tactician's Crown (once only). (Refreshes this game: (live counter)) |
| Rich Get Richer | Gold | Gain 12 gold. Your maximum interest is increased to 7. |
| Rich Get Richer | Gold | Gain 12 gold. Your maximum interest is increased to 7. |
| Rich Get Richer+ | Gold | Gain 18 gold. Your maximum interest is increased to 7. |
| Riftwalk | Gold | Gain a Kassadin. Before casting, your strongest Kassadin gains 38 Ability Power and blinks to the furthest enemy within 3 hexes, but his ability no longer disarms or shields. His Mana cost is reduced by 30. |
| Rising Infamy | Gold | Round start: Get a level (live counter) treasure chest. Your cannon barrages permanently improve future chests. Gain 2 Bilgerats. (Next level: (live counter) Barrages). |
| Rising Infamy+ | Gold | Round start: Get a level (live counter) treasure chest. Your cannon barrages permanently improve future chests. Gain 2 Bilgerats. (Next level: (live counter) Barrages). |
| Rising Infamy++ | Gold | Round start: Get a level (live counter) treasure chest. Your cannon barrages permanently improve future chests. Gain 2 Bilgerats. (Next level: (live counter) Barrages). |
| Risky Moves | Silver | Your Tactician loses 20 Health, but after 7 player combats, gain 30 gold. |
| Rogue Crest | Gold | Gain a Rogue Emblem and a Qiyana. |
| Rogue Crown | Prismatic | Gain a Rogue Emblem, a Hand of Justice, and a Katarina. |
| Rogue Heart | Silver | Your team counts as having 1 additional Rogue. Gain a Qiyana. |
| Roll The Dice | Prismatic | Gain a Rascal's Gloves, which equips two random Radiant items every round. |
| Rolling For Days I | Silver | Gain 10 free shop refreshes. These shop refreshes carry over between rounds. |
| Rolling For Days II | Gold | Gain 15 free shop refreshes. These shop refreshes carry over between rounds. |
| Rolling For Days III | Prismatic | Gain 25 free shop refreshes. These shop refreshes carry over between rounds. |
| Salvage Bin | Gold | Gain a random completed item now, and a component after 7 player combats. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Salvage Bin+ | Gold | Gain a random completed item now, and a component after 4 player combats. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Scrappy Inventions | Gold | At the start of combat, up to 5 components on your champions turn into completed items for the rest of combat. At the start of the next 2 stage(s), gain a component. |
| Sentinel's Spirit | Gold | While in spirit form, your Ionians gain 10% Attack Speed and shield themselves for 15% of their maximum Health. Gain 2 Ionians. |
| Shimmering Inventors | Gold | Your Piltover units deal 7% more damage for every 10 gold you have (max: 35%), and have a 33% chance to drop 1 gold at round start. Gain an Ekko. |
| Shoplifting | Gold | At the start of each turn, gain the highest Tier champion in your shop for free. |
| Shopping Spree | Prismatic | Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. |
| Shopping Spree | Prismatic | Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. |
| Shurima Crest | Gold | Gain a Shurima Emblem and a Taliyah. |
| Shurima Crown | Prismatic | Gain a Shurima Emblem, a Protector's Vow, and a Naafiri. |
| Silver Spoon | Silver | Gain 10 XP. |
| Silver Ticket | Gold | Each time your shop is refreshed, you have a 30% chance to gain a free refresh. |
| Slayer Crest | Gold | Gain a Slayer Emblem and a Qiyana. |
| Slayer Crown | Prismatic | Gain a Slayer Emblem, a Night Harvester, and a Quinn. |
| Slayer Heart | Silver | Your team counts as having 1 additional Slayer. Gain a Qiyana. |
| Slayer's Resolve | Gold | When your Slayers take or deal damage, they gain 2 stacking Armor and Magic Resistance for the rest of combat (max: 25 stacks). Gain 2 Slayers. |
| Sleight of Hand | Gold | Gain a Thief's Gloves. Champions holding Thief's Gloves gain 200 Health and 20% Attack Speed. |
| Small Forge | Silver | Gain a Component Anvil and 2 gold. |
| Social Distancing I | Silver | Combat start: Units with no adjacent allies gain 10% Attack Damage and 10 Ability Power. |
| Social Distancing II | Gold | Combat start: Units with no adjacent allies gain 16% Attack Damage and 16 Ability Power. |
| Social Distancing III | Prismatic | Combat start: Units with no adjacent allies gain 25% Attack Damage and 25 Ability Power. |
| Sorcerer Crest | Gold | Gain a Sorcerer Emblem and a Swain. |
| Sorcerer Crown | Prismatic | Gain a Sorcerer Emblem, a Spear of Shojin, and a Vel'Koz. |
| Sorcerer Heart | Silver | Your team counts as having 1 additional Sorcerer. Gain a Swain. |
| Spoils of War I | Silver | When you kill an enemy unit, there's a 25% chance to drop loot. |
| Spoils of War I | Silver | When you kill an enemy unit, there's a 25% chance to drop loot. |
| Spoils of War II | Gold | When you kill an enemy unit, there's a 30% chance to drop loot. |
| Spoils of War II | Gold | When you kill an enemy unit, there's a 30% chance to drop loot. |
| Spoils of War III | Prismatic | When you kill an enemy unit, there's a 40% chance to drop loot. |
| Spoils of War III | Prismatic | When you kill an enemy unit, there's a 40% chance to drop loot. |
| Stable Evolution | Gold | Your Void units gain 60 Health and 6% Attack Damage and Ability Power for each star level they have. Gain 2 Void units. |
| Stars are Born | Gold | The first Tier 1 champion and Tier 2 champion you buy are upgraded to 2-star. Gain 6 gold. |
| Stars are Born | Gold | The first Tier 1 champion and Tier 2 champion you buy are upgraded to 2-star. Gain 6 gold. |
| Starter Kit | Prismatic | Gain a Tier 4 champion and a 2-star Tier 1 champion that shares a trait with them. At the next 2 stages, gain the Tier 4 champion again. |
| Starter Kit | Prismatic | Gain a Tier 4 champion and a 2-star Tier 1 champion that shares a trait with them. At the next 2 stages, gain the Tier 4 champion again. |
| Stationary Support I | Silver | After 8 player combats, gain 1 Training Dummy(s) equiped the with 1 random Support Item(s), which cannot be removed. |
| Stationary Support II | Gold | Gain 1 Training Dummy(s). They have 1 random Support Item(s) equipped, which cannot be removed. |
| Stationary Support III | Prismatic | Gain 1 Training Dummy(s). They have 2 random Support Item(s) equipped, which cannot be removed. |
| Stellacorn's Blessing | Gold | While you have the Targon trait activated, your units gain 40% Attack Speed for 4 seconds after being healed or shielded. Gain 2 Targonians. |
| Stolen Vitality | Gold | When your Vanquisher's attacks and Abilities critically strike, they heal the ally with the most missing Health by 1% of that ally's max Health. Gain 2 Vanquishers. |
| Strategist Heart | Gold | Your team counts as having 1 additional Strategist. Gain a Swain. |
| Strategist Soul | Prismatic | Your team counts as having 1 additional Strategist. Gain an Adaptive Helm and a Swain. |
| Support Cache | Gold | Open an Armory and choose 1 of 4 unique Support items. |
| Tactical Superiority | Gold | Your champions gain 4% Attack Damage and Ability Power for each of their active traits, doubled when you field a Strategist. Gain 2 Strategists. |
| Tactician's Tools | Prismatic | Gain 2 Spatulas and 1 Component Anvil. |
| Targon Heart | Gold | Your team counts as having 1 additional Targon. Gain a Soraka. |
| Targon Soul | Prismatic | Your team counts as having 1 additional Targon. Gain a Redemption and a Taric. |
| Teaming Up I | Silver | Gain 1 random component and 2 random Tier 3 champions. |
| Teaming Up II | Gold | Gain 1 random Support item and 1 random Tier 4 champion. |
| Teaming Up III | Prismatic | Gain 1 Support Anvil, 1 random component, and 2 random Tier 4 champions. |
| The Boss | Gold | Gain a Sett. The first time your strongest Sett falls below 60% Health, he takes a break to do sit-ups. Each sit-up heals him 10% of his max Health and grants 40% Attack Speed and 40 Ability Power. |
| The Golden Egg | Prismatic | Gain a massive golden egg that hatches in 11 turns. Victorious player combats accelerate the hatch timer by an additional turn. |
| Three's a Crowd | Gold | Your units gain 100 Health for each unique Tier 3 unit you field. |
| Three's Company | Gold | Gain 4 random Tier 3 champions. |
| Tiniest Titan | Prismatic | Your Tactician is small and speedy. After each PVP round, heal 2 Health and gain 2 gold. |
| Tiniest Titan | Prismatic | Your Tactician is small and speedy. After each PVP round, heal 2 Health and gain 2 gold. |
| Tiniest Titan+ | Prismatic | Your Tactician is small and speedy. After each PVP round, heal 2 Health and gain 2 gold. Gain 8 gold now. |
| Tiny Grab Bag | Silver | Gain a random component, 2 gold, a Magnetic Remover. |
| Tiny Power I | Silver | Your units gain 6% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Power II | Gold | Your units gain 10% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Power III | Prismatic | Your units gain 15% Attack Damage, Ability Power, and Attack Speed. |
| Tiny Titans | Silver | Your Tactician heals 30 Health, grows larger, and gains 30 maximum health. |
| Tiny Titans | Silver | Your Tactician heals 30 Health, grows larger, and gains 30 maximum health. |
| Titanic Strength | Gold | Bruisers gain increased Attack Damage equal to 1.5% of their maximum Health. Gain 2 Bruisers. |
| Tons of Stats! | Gold | Your team gains 44 Health, 4% Attack Damage, 4 Ability Power, 4 Armor, 4 Magic Resist, 4% Attack Speed, and 4 Mana. |
| Total Domination | Gold | Noxus units' attacks and Abilities execute enemies below 5% Health, increased by 1% for each player you've Conquered. Gain 2 Noxians. |
| Trade Sector | Gold | Gain a free Shop refresh each round. Gain 2 gold. |
| Trade Sector | Gold | Gain a free Shop refresh each round. Gain 2 gold. |
| Training Reward | Silver | Gain 5 gold and a Lesser Champion Duplicator. |
| Training Reward II | Gold | Gain 12 gold and a Lesser Champion Duplicator. |
| Training Reward III | Prismatic | Gain 24 gold and a Champion Duplicator. |
| Transfusion I | Silver | Your units gain 20 Health, plus 2 Health per missing Tactician Health. |
| Transfusion I | Silver | Your units gain 20 Health, plus 2 Health per missing Tactician Health. |
| Transfusion II | Gold | Your units gain 40 Health, plus 3 Health per missing Tactician Health. |
| Transfusion II | Gold | Your units gain 40 Health, plus 3 Health per missing Tactician Health. |
| Transfusion III | Prismatic | Your units gain 50 Health, plus 5 Health per missing Tactician Health. |
| Transfusion III | Prismatic | Your units gain 50 Health, plus 5 Health per missing Tactician Health. |
| Two Healthy | Gold | Your units gain 100 Health for each unique Tier 2 unit you field. |
| Unified Resistance I | Silver | If you have 3 or more units in the same row at the start of combat, they all gain 15 Armor and Magic Resist. |
| Unified Resistance II | Gold | If you have 3 or more units in the same row at the start of combat, they all gain 25 Armor and Magic Resist. |
| Unleashed Arcana | Prismatic | Gain a Jeweled Gauntlet, a Rabadon's Deathcap, a Needlessly Large Rod, and a Magnetic Remover. |
| Urf's Grab Bag | Prismatic | Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Vampiric Blades | Gold | Your units gain 10% Omnivamp. Your Rogues gain 15% instead. Gain 2 Rogues. |
| Vanquisher Crest | Gold | Gain a Vanquisher Emblem and an Ashe. |
| Vanquisher Crown | Prismatic | Gain a Vanquisher Emblem, a Hand of Justice, and a Darius. |
| Vanquisher Heart | Silver | Your team counts as having 1 additional Vanquisher. Gain an Ashe. |
| Void Crest | Gold | Gain a Void Emblem and a Kassadin. |
| Void Crown | Prismatic | Gain a Void Emblem, an Adaptive Helm, and a Rek'Sai. |
| Wandering Trainer | Prismatic | Gain a Training Dummy. It has 3 random Emblems equipped, which cannot be removed. Gain 6 gold. |
| Well-Earned Comforts I | Silver | Your units gain 70 Health for each item equipped. |
| Well-Earned Comforts II | Gold | Your units gain 111 Health for each item equipped. |
| Well-Earned Comforts III | Prismatic | Your units gain 120 Health and 6% Attack Speed for each item equipped. |
| Wellness Trust | Prismatic | Round start: Gain 3 gold. If you have at least 40 gold, your Tactician heals 2 Health. |
| What Doesn't Kill You | Gold | After losing your combat, gain 2 gold. Every 4 losses, gain a random component. Losses until next component: (live counter) |
| What The Forge | Prismatic | All completed items you own or receive are transformed into random Artifacts (excluding Tactician's Crown and Emblems). Units gain 110 Health per equipped Artifact. |
| Winds of War | Gold | Gain a Galio. Your strongest Galio's ability gets larger with each cast, and enemies hit take magic damage equal to 12% of his max Health each second. |
| You Have My Bow | Gold | Gain a Recurve Bow. Your units gain 12% Attack Speed. |
| You Have My Sword | Gold | Gain a B.F. Sword. Your units gain 15% Attack Damage. |
| Young and Wild and Free | Silver | You can always move freely on Carousel rounds. Gain 1 random item component(s). |
| Zaun Crest | Gold | Gain a Zaun Emblem and a Warwick. |
| Zaun Crown | Prismatic | Gain a Zaun Emblem, a Guardbreaker, and an Ekko. |
| Zaun Heart | Silver | Your team counts as having 1 additional Zaun. Gain a Warwick. |

## Legends (15)

Legends were picked before the game. Each Legend changed which augments the player was offered in the early, mid and late augment rounds. Source: MetaTFT lookup.

| Legend | Description | Early augments | Mid augments | Late augments |
|---|---|---|---|---|
| Aurelion Sol: The Ancient | Become high level and play awesome units! | Cutting Corners: Leveling up costs 4 XP less. (Level 4 to 5 costs 6 XP instead of 10.) / Patient Study: After player combat, gain 2 XP if you won or 3 XP if you lost. / Level Up!: When you buy XP, gain an additional 2. Gain 8 immediately. | Knowledge Download I: Gain 12 XP. / Knowledge Download II: Gain 22 XP. / Knowledge Download III: Gain 36 XP. | It Pays To Learn: Gain 10 XP and 8 gold. / It Pays to Learn II: Gain 16 XP and 12 gold. / It Pays to Learn III: Gain 24 XP and 16 gold. |
| Bard: The Traveler | Gain even more power when you level up! | Caretaker's Ally: Gain a random 2-cost champion now. Gain the same one again every time you level up. / Caretaker's Favor: Gain a component anvil when you reach level 5, 6, 7, and 8. The anvil offers 4 choices. / Caretaker's Chosen: As you level, gain more powerful items. Level 4: component anvil Level 6: completed item anvil Level 8: choose 1 of 5 Radiant items | Small Forge: Gain a Component Anvil and 2 gold. / Medium Forge: Gain a Completed Item Anvil and 1 Gold. / Large Forge: Gain an Artifact Anvil and a Component Item Anvil. | Teaming Up I: Gain 1 random component and 2 random Tier 3 champions. / Teaming Up II: Gain 1 random Support item and 2 random 4-cost champions. / Teaming Up III: Gain 1 Support Anvil, 1 random component, and 2 random Tier 4 champions. |
| Caitlyn: The Peacekeeper | Get strong units at the start of the game! | Ones Twos Three: Gain 3 1-cost champion, 2 2-cost champions, and 1 3-cost champion. / Stars are Born: The first 1-cost and 2-cost champions you buy are instantly upgraded to 2-star. Gain 1 gold. / Starter Kit: Gain a 4-cost champion and a 2-star 1-cost champion that shares a trait with them. At the start of the next 2 stages, gain that 4-cost champion again. | Knowledge Download I: Gain 12 XP. / Knowledge Download II: Gain 22 XP. / Knowledge Download III: Gain 36 XP. | Rolling For Days I: Gain 8 free Shop rerolls that never expire. / Rolling For Days II: Gain 15 free Shop rerolls that never expire. / Rolling For Days III: Gain 25 free Shop rerolls that never expire. |
| Draven: The Executioner | Win big and get bigger payouts! | Spoils of War I: Enemies have a 25% chance to drop loot when killed. / Spoils of War II: Enemies have a 30% chance to drop loot when killed. / Spoils of War III: Enemies have a 40% chance to drop loot when killed. | Balanced Budget: At the start of the next 4 rounds, gain 4 gold. / Balanced Budget: At the start of the next 4 rounds, gain 7 gold. / Balanced Budget III: At the start of the next 4 rounds, gain 11 gold. | Rolling For Days I: Gain 8 free Shop rerolls that never expire. / Rolling For Days II: Gain 15 free Shop rerolls that never expire. / Rolling For Days III: Gain 25 free Shop rerolls that never expire. |
| Ezreal: The Item Collector | The more items, the better! | Buried Treasures I: Gain a random item component at the start of the next 2 rounds (including this round). / Buried Treasures II: Gain a random item component at the start of the next 3 rounds (including this round). / Buried Treasures III: Gain a random item component at the start of the next 6 rounds (including this round). | Tiny Grab Bag: Gain a random component, 2 gold, a Magnetic Remover. / Big Grab Bag: Gain 3 random components, 2 gold, and 1 Reforger. Reforgers allow you to remake any item. / Giant Grab Bag: Gain 3 random components, 4 gold, and a Lesser Champion Duplicator. | Well-Earned Comforts I: Your units gain 70 Health for each item equipped. / Well-Earned Comforts II: Your units gain 111 Health for each item equipped. / Well-Earned Comforts III: Your units gain 120 Health and 6% Attack Speed for each item equipped. |
| Lee Sin: The Playmaker | Reroll your Shop for perfect champions! | Silver Ticket: Gain a free Shop reroll after every 4 rerolls. / Trade Sector: Gain a free Shop reroll every round. Gain 2 gold. / Shopping Spree: Gain 1 gold per round. When you level up, gain a number of free shop refreshes equal to your level, which carry over between rounds. | Training Reward: Gain 5 gold and a Lesser Champion Duplicator. / Training Reward II: Gain 12 gold and a Lesser Champion Duplicator. / Training Reward III: Gain 24 gold and a Champion Duplicator. | Money!: Gain 11 gold. In 4 turns, gain 11 gold again. / Money Money!: Gain 16 gold. In 3 turns, gain 16 gold again. / Money Money Money!: Gain 25 gold. In 3 turns, gain 25 gold again. |
| Master Yi: The Wuju Master | Grow stronger and stronger with every fight! | Pumping Up I: Your team gains 8% Attack Speed now. Each round after, they gain 0.5% more. (current Attack Speed: (live counter)%) / Pumping Up II: Your team gains 8% Attack Speed now. Each round after, they gain 1% more. (current Attack Speed: (live counter)%) / Pumping Up III: Your team gains 12% Attack Speed now. Each round after, they gain 2% more. (current Attack Speed: (live counter)%) | Battle Ready: Your team deals 3% more damage and takes 3% less damage. / Battle Ready II: Your team deals 6% more damage and takes 6% less damage. / Battle Ready III: Your team deals 8% more damage and takes 8% less damage. | Gotta Go Fast!: Your units generate 10% more Mana and move 35% faster. / Gotta Go Fast! II: Your units generate 20% more Mana and move 60% faster. / Gotta Go Fast!!! III: Your units generate 30% more Mana and move 100% faster. |
| Ornn: The Forgemaster | Gain the power of Ornn's creations! | Latent Forge: After 8 player combats, gain an Artifact anvil. The anvil offers 4 choices. Artifacts are more powerful items with a unique effect. / Portable Forge: Choose 1 of 2 Artifacts. Artifacts are more powerful items with a unique effect. / Living Forge: Gain an Artifact anvil now and after every 10 player combats. Artifacts are more powerful items with a unique effect. | Small Forge: Gain a Component Anvil and 2 gold. / Medium Forge: Gain a Completed Item Anvil and 1 Gold. / Large Forge: Gain an Artifact Anvil and a Component Item Anvil. | Job's Done: Gain 1 Component Anvil and 1 random component. / Job Well Done: Gain a Completed Item Anvil and 1 random component. / Masterful Job: Gain an Artifact Anvil and a Completed Item Anvil. |
| Pengu: The Tactician | Play it safe! Great for beginners! | Tiny Titans: Increase your current and maximum player health by 30. / Metabolic Accelerator: Gain 2 player health after every player combat. Your Tactician also moves faster. / Tiniest Titan: Gain 2 player health and 1 gold after every player combat. Your Tactician also moves faster. | Item Grab Bag I: Gain 1 random completed item. / Item Grab Bag II: Gain a random completed item, a random component, and 1 gold. / Item Grab Bag III: Gain 2 random completed items and 4 Gold. | Final Grab Bag: Gain a random component, 10 gold, and a Reforger. / Final Grab Bag II: Gain 2 random components, 12 gold, and a Magnetic Remover. / Urf's Grab Bag: Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Poro: The Imporoble  | No tailoring, just a classic game of TFT! |  |  |  |
| Tahm Kench: The Glutton | Collect as much gold as possible! | AFK: You cannot perform actions for the next 3 rounds. Afterwards, gain 18 gold. / Rich Get Richer: Gain 12 gold. Your max interest is increased to 7. Interest is extra gold you gain per 10g saved. / Hedge Fund: Gain 20 gold. Your max interest is increased to 10. Interest is extra gold you gain per 10g saved. | Balanced Budget: At the start of the next 4 rounds, gain 4 gold. / Balanced Budget: At the start of the next 4 rounds, gain 7 gold. / Balanced Budget III: At the start of the next 4 rounds, gain 11 gold. | Money!: Gain 11 gold. In 4 turns, gain 11 gold again. / Money Money!: Gain 16 gold. In 3 turns, gain 16 gold again. / Money Money Money!: Gain 25 gold. In 3 turns, gain 25 gold again. |
| Twisted Fate: The Gambler | Get the exact items you need! | Pandora's Items: Round start: items on your bench are randomized (except Tactician's Crown and Spatula). Gain 1 random component. / Pandora's Items II: Round start: items on your bench are randomized (excluding Tactician's Crown and Spatula). Gain 1 random completed item. / Pandora's Items III: Round start: items on your bench are randomized (excluding Tactician's Crown and Spatula). Gain 1 random Radiant item. | Item Grab Bag I: Gain 1 random completed item. / Item Grab Bag II: Gain a random completed item, a random component, and 1 gold. / Item Grab Bag III: Gain 2 random completed items and 4 Gold. | Teaming Up I: Gain 1 random component and 2 random Tier 3 champions. / Teaming Up II: Gain 1 random Support item and 2 random 4-cost champions. / Teaming Up III: Gain 1 Support Anvil, 1 random component, and 2 random Tier 4 champions. |
| Urf: The Emblem Collector | For the player that loves emblems! | Branching Out: Gain a random Emblem and a Reforger. Reforgers allow you to remake any item. / Ancient Archives I: Gain 1 Tome of Traits and 3 gold. / Ancient Archives II: Gain 2 Tome of Traits and 5 gold. | Tiny Grab Bag: Gain a random component, 2 gold, a Magnetic Remover. / Big Grab Bag: Gain 3 random components, 2 gold, and 1 Reforger. Reforgers allow you to remake any item. / Giant Grab Bag: Gain 3 random components, 4 gold, and a Lesser Champion Duplicator. | Final Grab Bag: Gain a random component, 10 gold, and a Reforger. / Final Grab Bag II: Gain 2 random components, 12 gold, and a Magnetic Remover. / Urf's Grab Bag: Gain 2 random components, a Champion Duplicator, and a Spatula. |
| Veigar: The Caster | Deal big damage with your champions' abilities! | Jeweled Lotus I: Combat start: Your strongest unit gains 40% Critical Strike Chance and their abilities can critically strike. / Jeweled Lotus II: Your team gains 15% Critical Strike chance, and their Abilities can critically strike. / Jeweled Lotus III: Your team gains 40% Critical Strike chance, and their Abilities can critically strike. | Tiny Power I: Your units gain 6% Attack Damage, Ability Power, and Attack Speed. / Tiny Power II: Your units gain 10% Attack Damage, Ability Power, and Attack Speed. / Tiny Power III: Your units gain 15% Attack Damage, Ability Power, and Attack Speed. | Partial Ascension: After 15 seconds of combat, your units deal 30% more damage. / Ascension: After 15 seconds of combat, your units deal 50% more damage. / Final Ascension: Your team deals 15% more damage. After 15 seconds, they deal 45% more damage. |
| Vladimir: The Hemomancer | Trade your health for power! | Transfusion I: Your units gain 20 Health, plus 2 Health per missing Tactician Health. / Transfusion II: Your units gain 40 Health, plus 3 Health per missing Tactician Health. / Transfusion III: Your units gain 50 Health, plus 5 Health per missing Tactician Health. | Battle Ready: Your team deals 3% more damage and takes 3% less damage. / Battle Ready II: Your team deals 6% more damage and takes 6% less damage. / Battle Ready III: Your team deals 8% more damage and takes 8% less damage. | Partial Ascension: After 15 seconds of combat, your units deal 30% more damage. / Ascension: After 15 seconds of combat, your units deal 50% more damage. / Final Ascension: Your team deals 15% more damage. After 15 seconds, they deal 45% more damage. |

## Portals (77)

Source: MetaTFT lookup "portals" list.

| Name | Group | Effect |
|---|---|---|
| Spatula | Spatula | Everyone gets a spatula! |
| Ixaocan | Ixtal | Each time players star up 20 units, all players gain increasingly valuable loot. |
| Gold | Gold | Everyone gains 8 Gold. |
| VIP Lounge | Bilgewater | Twice per game, Bilgewater Trader Finn will randomly appear and offer Completed, Artifact, Support, or Radiant items. You can choose one to keep for free! Stage 4-7 is replaced by a treasure armory round. Each reroll costs 1. |
| Rock'n'Rollin | Ixtal | Each time players star up 20 units, all players gain increasingly valuable loot. Gain free shop rerolls equal to the stage number plus 1 at the start of each stage. These only last for the round you get them. |
| Valar's Hollow | Freljord | On stage 4-2 gain a component item anvil, and on 6-2, gain a Support item anvil. |
| Finn's Market | Bilgewater | Twice per game, Bilgewater Trader Finn will randomly appear and offer Completed, Artifact, Support, or Radiant items. You can choose one to keep for free! |
| The Golden Era | Shurima | Each champion on a component carousel has 2 components on them instead of 1. Every time you build a completed item, gain 3 gold. |
| Slaughter Docks | Bilgewater | Gain free shop rerolls equal to the stage number plus 1 at the start of every other stage, beginning at stage 3! These only last for the round you get them. |
| Beats by Scuttle | Piltover | All augments offered this game will be prismatic. PvE monsters are replaced by scuttle crabs that grant extra loot. |
| Tome of Traits | Tome | Everyone gets a Tome of Traits. |
| Chill Beats | Freljord | On stage 2-3 gain a component item anvil, and on 3-3, gain a Support item anvil. Randomly on either stage 2-5 or 3-5, gain an Artifact Anvil. On stage 4-5, gain a Masterwork Upgrade consumable that lets you upgrade one equipped item into its radiant version. |
| Mystery Box | LootOrb | Everyone gets a loot orb of mysterious value. |
| Health | Health | Everyone gains 8 tactician health. |
| Cardinal Arcology | Ixtal | You will always be offered a Silver, Gold, and Prismatic augment, in that order. |
| Ionian Duet | Ionia | A spot on your bench becomes the grove. The champion placed in it contributes their non-unique traits as though they were on your team. Gain a Scroll of Knowledge on stage 3-5. Use it on a champion to open an armory offering every emblem that matches their traits. |
| Rat Town | Bilgewater | Starting at stage 2, Lucky Shops appear randomly once per stage. These feature champions tailored to your team's active traits. |
| Crescendo | Zaun | Every time you select an augment, gain 4 gold per augment you have. You will always be offered a Silver, Gold, and Prismatic augment, in that order. |
| Opening Act | Demacia | Gain 3 component anvils at the start of the game. The first augment offered this game will be prismatic. |
| Serpentine River | Ixtal | The stage 2 and 4 carousels are replaced by voting rounds. Everyone gets a component anvil and the winning reward! |
| Center Stage | Demacia | Gain a Crown of Demacia on stage 2-1. If the holder dies, you instantly lose the fight. On stage 3-5, gain a Scroll of Knowledge. Use it on a champion to open an armory offering every emblem that matches their traits. On stage 4-5, gain a Masterwork Upgrade consumable that lets you upgrade one equipped item into its radiant version. |
| Battle of the Sands | Noxus | At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. Twice per stage, get a free high-tier champion. The champion's tier increases with game time. |
| Ruptured Harmony | Void | Travel to a random Remastered Portal location that is not currently shown. |
| Component Anvil | Component | Everyone gains a component anvil. |
| Valar's Hollow | Freljord | On stage 2-3 gain a component item anvil, and on 3-3, gain a Support item anvil. |
| High Notes | Bilgewater | Starting at stage 2, Lucky Shops appear randomly once per stage. These feature champions tailored to your team's active traits. When you buy XP, gain an additional 2. |
| Rerolls | Rerolls | Everyone gains 5 free shop rerolls that last forever. |
| Slaughter Docks | Bilgewater | Gain free shop rerolls equal to the stage number plus 1 at the start of each stage! These only last for the round you get them. |
| Support Item | SupportItem | Everyone gets a Support Item Anvil! |
| Rat Town | Bilgewater | Starting at stage 3, Lucky Shops appear randomly once per stage. These feature champions tailored to your team's active traits. |
| Foodfight Tactics | BandleCity | Gain a Spatula on stage 2-1. Each round, champions equipped with Spatulas feed Poro Snax to their closest ally and grant them 20 permanent Health for each Spatula component they have. On stage 2-5 and 4-5, gain a Tactician's Crown. |
| Processional Dirge | ShadowIsles | When you reach 40 Health, receive a blessing from the Targonian gods. Every time a player dies, all players open an armory containing 4 of the eliminated players' items. |
| Basement Show | Zaun | There is no interest this game. Instead, starting on stage 2-1, gain 3 gold every round. Whenever you would be offered an augment, gain a completed item anvil instead. |
| Warlord's Palace | Shurima | Stage 4-7 is replaced by a treasure armory round. Each reroll costs 1 gold. |
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
| God-Willow's Grove | Ionia | A spot on your bench becomes the grove. The champion placed in it contributes their non-unique traits as though they were on your team. |
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
| Warlord's Palace | Shurima | Stage 7-1 is replaced by a treasure armory round. Each reroll costs 1 gold. |
| The Rupture | Void | Travel to a random location that is not currently shown. |
| Fleshing Arena | Noxus | Once per stage, get a free high-tier unit. The unit's tier increases with game time. |
| Bandle Cafeteria | BandleCity | Gain a Spatula on stage 2-1. Each round, champions equipped with Spatulas feed Poro Snax to their closest ally and grant them 20 permanent Health for each Spatula component they have. |
| Placidium Library | Ionia | Gain a Scroll of Knowledge on stage 6-3. Use it on a champion to open an armory offering every emblem that matches their traits. |
| Marus Omegnum | Targon | On stage 4-1 and 7-3, gain a Tactician's Crown. |
| Marus Omegnum | Targon | On stage 2-5 and 3-6, gain a Tactician's Crown. |
