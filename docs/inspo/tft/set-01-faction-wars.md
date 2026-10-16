# TFT Set 1: Faction Wars (also called the Beta Set / Base set)

Sources:
- Community Dragon TFT data, patch 9.21: https://raw.communitydragon.org/9.21/cdragon/tft/en_us.json (champions, stats, abilities, traits, items, augments).
- Community Dragon character bins, patch 9.21: https://raw.communitydragon.org/9.21/game/data/characters/<unit>/<unit>.bin.json (spell calculations used to fill computed ability numbers).
- https://leagueoflegends.fandom.com/wiki/Beta_Set_(Teamfight_Tactics)
- https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-9-22-notes/
- https://leagueoflegends.fandom.com/wiki/Patch_(Teamfight_Tactics)/Sets
- https://leagueoflegends.fandom.com/wiki/Gold_(Teamfight_Tactics)
- https://leagueoflegends.fandom.com/wiki/Carousel_(Teamfight_Tactics)

Retrieved 2026-10-15. Stats and numbers are from the data patch listed, which is the last patch of the set unless noted; they can differ from launch values. Bracketed tags in the mechanics lists name the source of each line.

## Overview

| Field | Value |
|---|---|
| Set number | 1 |
| Name | Faction Wars (also called the Beta Set / Base set) |
| Live patches | 9.13 to 9.21 |
| Live dates (NA patch dates) | 2019-06-26 to 2019-11-05 |
| Data patch used | 9.21 |
| Theme | Champions grouped by League of Legends factions and races (origins such as Noble, Imperial, Demon, Yordle, Void, Pirate, Wild, Robot) plus combat classes. |
| Counts | 57 champion entries, 24 traits, 55 items, 0 augments |

Name note: Riot never gave Set 1 an official subtitle at launch. CDragon calls it "Base"; the wiki calls it the Beta Set and lists "Faction Wars" as a conjectural name.

## Set mechanics

- Core loop introduced: 8 players, shared champion pool, 5-card shop, buy XP, combine three copies into a higher star level, auto-battle against one opponent per round. [wiki]
- Shared draft (carousel) at the start and then every 6 rounds; lowest-health players pick first; first carousel lets all 8 pick at once. [wiki Carousel]
- Items: 8 components plus Spatula; two components combine into a completed item. Spatula items grant a trait. [CDragon]
- PvE rounds (minions, Krugs, wolves, raptors, dragon/herald) drop loot orbs with gold, champions, or items. [wiki Gold]
- Pirate trait (Gangplank, Graves, Pyke, Twisted Fate) dropped gold after combat. [wiki Gold]
- Board was 3 rows of 7 hexes per side (changed to 4 rows in 9.22). [9.22 notes]

## Economy and system changes

- Income: 5 gold passive per round, +1 per 10 gold held (max +5), win/loss streak bonus. Exact Set 1 streak breakpoints: unknown. [wiki Gold]
- Champion pool per champion at end of Set 1: tier 1 = 39, tier 2 = 26, tier 3 = 18, tier 4 = 13, tier 5 = 10. [9.22 notes, "before" values]
- Level 8 shop odds at end of Set 1: 15/20/35/22/8% (tier 1-5). Other levels: unknown. [9.22 notes]
- Player damage per stage at end of Set 1: 3/3/3/4/5/6 base. [9.22 notes]
- Mana per attack at end of Set 1: 6-10 for 1-star, 10 for 2-star and 3-star. [9.22 notes]
- XP: 4 gold buys 4 XP; 2 XP per round. [wiki Experience]
- Mystery Box loot system added in 9.18. [wiki Gold]

## Champions

Stats are 1-star base values. AS = attacks per second. Range is in hexes. Mana is start / max. Health and attack damage scale by star level (the wiki gives 180% at 2-star and 324% at 3-star). Ability numbers are listed as 1-star/2-star/3-star.

| Champion | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana |
|---|---|---|---|---|---|---|---|---|---|
| Camille | 1 | Hextech, Blademaster | 550 | 30 | 20 | 55 | 0.6 | 1 | 75 / 125 |
| Darius | 1 | Imperial, Knight | 600 | 40 | 20 | 50 | 0.5 | 1 | 0 / 100 |
| Elise | 1 | Demon, Shapeshifter | 500 | 20 | 20 | 45 | 0.6 | 2 | 0 / 100 |
| Fiora | 1 | Noble, Blademaster | 450 | 25 | 20 | 40 | 1 | 1 | 0 / 75 |
| Garen | 1 | Noble, Knight | 600 | 40 | 20 | 50 | 0.6 | 1 | 0 / 100 |
| Graves | 1 | Pirate, Gunslinger | 450 | 30 | 20 | 55 | 0.55 | 1 | 0 / 0 |
| Kassadin | 1 | Void, Sorcerer | 550 | 35 | 20 | 40 | 0.55 | 1 | 0 / 0 |
| Kha'Zix | 1 | Void, Assassin | 500 | 20 | 20 | 55 | 0.6 | 1 | 0 / 65 |
| Mordekaiser | 1 | Phantom, Knight | 550 | 40 | 20 | 50 | 0.5 | 1 | 50 / 150 |
| Nidalee | 1 | Wild, Shapeshifter | 500 | 20 | 20 | 50 | 0.65 | 3 | 0 / 85 |
| Tristana | 1 | Yordle, Gunslinger | 500 | 20 | 20 | 50 | 0.65 | 4 | 0 / 50 |
| Vayne | 1 | Noble, Ranger | 550 | 25 | 20 | 40 | 0.7 | 3 | 0 / 0 |
| Warwick | 1 | Wild, Brawler | 650 | 30 | 20 | 50 | 0.6 | 1 | 50 / 150 |
| Ahri | 2 | Wild, Sorcerer | 450 | 20 | 20 | 50 | 0.55 | 3 | 0 / 75 |
| Blitzcrank | 2 | Robot, Brawler | 650 | 35 | 20 | 50 | 0.5 | 1 | 0 / 125 |
| Braum | 2 | Glacial, Guardian | 650 | 75 | 20 | 40 | 0.6 | 1 | 0 / 75 |
| Jayce | 2 | Hextech, Shapeshifter | 600 | 35 | 20 | 60 | 0.65 | 1 | 50 / 100 |
| Lissandra | 2 | Glacial, Elementalist | 500 | 20 | 20 | 40 | 0.6 | 2 | 0 / 95 |
| Lucian | 2 | Noble, Gunslinger | 600 | 25 | 20 | 65 | 0.65 | 3 | 0 / 35 |
| Lulu | 2 | Yordle, Sorcerer | 500 | 20 | 20 | 50 | 0.6 | 2 | 50 / 150 |
| Pyke | 2 | Pirate, Assassin | 600 | 25 | 20 | 60 | 0.6 | 1 | 50 / 125 |
| Rek'Sai | 2 | Void, Brawler | 650 | 20 | 20 | 50 | 0.6 | 1 | 50 / 150 |
| Shen | 2 | Ninja, Blademaster | 700 | 30 | 20 | 65 | 0.7 | 1 | 100 / 150 |
| Twisted Fate | 2 | Pirate, Sorcerer | 500 | 20 | 20 | 40 | 0.7 | 3 | 0 / 50 |
| Varus | 2 | Demon, Ranger | 500 | 25 | 20 | 60 | 0.7 | 4 | 0 / 75 |
| Zed | 2 | Ninja, Assassin | 550 | 25 | 20 | 65 | 0.7 | 1 | 0 / 75 |
| Aatrox | 3 | Demon, Blademaster | 700 | 35 | 20 | 65 | 0.65 | 1 | 0 / 75 |
| Ashe | 3 | Glacial, Ranger | 550 | 20 | 20 | 65 | 0.7 | 4 | 0 / 125 |
| Evelynn | 3 | Demon, Assassin | 550 | 20 | 20 | 70 | 0.6 | 1 | 0 / 75 |
| Gangplank | 3 | Pirate, Gunslinger, Blademaster | 700 | 20 | 20 | 60 | 0.65 | 1 | 0 / 100 |
| Katarina | 3 | Imperial, Assassin | 450 | 20 | 20 | 65 | 0.65 | 1 | 0 / 85 |
| Kennen | 3 | Ninja, Yordle, Elementalist | 550 | 20 | 20 | 65 | 0.65 | 2 | 50 / 150 |
| Morgana | 3 | Demon, Sorcerer | 650 | 30 | 20 | 50 | 0.6 | 2 | 50 / 150 |
| Poppy | 3 | Yordle, Knight | 700 | 40 | 20 | 50 | 0.5 | 1 | 0 / 75 |
| Rengar | 3 | Wild, Assassin | 550 | 20 | 20 | 70 | 0.6 | 1 | 0 / 75 |
| Shyvana | 3 | Dragon, Shapeshifter | 650 | 30 | 20 | 50 | 0.7 | 1 | 0 / 85 |
| Veigar | 3 | Yordle, Sorcerer | 500 | 20 | 20 | 50 | 0.55 | 3 | 0 / 75 |
| Vi | 3 | Hextech, Brawler | 700 | 25 | 20 | 60 | 0.65 | 1 | 75 / 150 |
| Volibear | 3 | Glacial, Brawler | 750 | 30 | 20 | 70 | 0.65 | 1 | 0 / 75 |
| Akali | 4 | Ninja, Assassin | 650 | 20 | 20 | 80 | 0.8 | 1 | 0 / 25 |
| Aurelion Sol | 4 | Dragon, Sorcerer | 650 | 20 | 20 | 40 | 0.6 | 3 | 0 / 125 |
| Brand | 4 | Demon, Elementalist | 700 | 25 | 20 | 60 | 0.6 | 3 | 50 / 150 |
| Cho'Gath | 4 | Void, Brawler | 1000 | 20 | 20 | 70 | 0.6 | 1 | 50 / 150 |
| Draven | 4 | Imperial, Blademaster | 650 | 25 | 20 | 65 | 0.75 | 3 | 0 / 50 |
| Gnar | 4 | Wild, Yordle, Shapeshifter | 700 | 30 | 20 | 50 | 0.7 | 2 | 0 / 100 |
| Jinx | 4 | Hextech, Gunslinger | 550 | 20 | 20 | 75 | 0.7 | 3 | 0 / 0 |
| Kindred | 4 | Phantom, Ranger | 600 | 20 | 20 | 60 | 0.7 | 3 | 50 / 150 |
| Leona | 4 | Noble, Guardian | 800 | 100 | 20 | 45 | 0.55 | 1 | 50 / 100 |
| Sejuani | 4 | Glacial, Knight | 800 | 40 | 20 | 45 | 0.55 | 1 | 50 / 150 |
| Anivia | 5 | Glacial, Elementalist | 750 | 20 | 20 | 40 | 0.8 | 3 | 50 / 125 |
| Kai'Sa | 5 | Void, Ranger, Assassin | 700 | 20 | 20 | 55 | 1.25 | 2 | 50 / 125 |
| Karthus | 5 | Phantom, Sorcerer | 850 | 25 | 20 | 65 | 0.65 | 3 | 40 / 125 |
| Kayle | 5 | Noble, Knight | 750 | 40 | 20 | 70 | 1 | 3 | 50 / 125 |
| Miss Fortune | 5 | Pirate, Gunslinger | 750 | 20 | 20 | 75 | 0.95 | 3 | 0 / 150 |
| Pantheon | 5 | Dragon, Guardian | 850 | 70 | 20 | 75 | 0.75 | 1 | 150 / 200 |
| Swain | 5 | Imperial, Demon, Shapeshifter | 850 | 25 | 20 | 65 | 0.65 | 2 | 0 / 100 |
| Yasuo | 5 | Exile, Blademaster | 750 | 35 | 20 | 75 | 1 | 1 | 0 / 25 |

### Abilities

Computed values assume 100 Ability Power (the TFT base) and no items. Terms that scale with a stat other than AP are written as a percent of that stat (for example "275% AD"). "[unknown: X]" marks a value the data did not let us compute.

**Camille** (1-cost) - The Hextech Ultimatum: Camille singles out and leaps on an enemy, dealing 200/325/450 magic damage and rooting them for 4/5/6 seconds. Allies within range will prioritize attacking Camille's target.

**Darius** (1-cost) - Decimate: Darius swings his axe in a circle, dealing 150/200/250 magic damage and healing himself for 100/150/200 health for each enemy hit.

**Elise** (1-cost) - Spider Form: Elise transforms into a giant spider and summons 1/2/4 spiderlings to fight alongside her. While in spider form, Elise gains 60/90/120 bonus lifesteal. Spiderlings benefit from active Demon trait bonuses.

**Fiora** (1-cost) - Riposte: Fiora enters a defensive stance for 1.5 seconds, becoming immune to damage and enemy spell effects. As she exits this stance, she ripostes, dealing 100/250/400 magic damage to a nearby enemy and stunning them for 1.5 seconds.

**Garen** (1-cost) - Judgement: Garen spins his sword for 4 seconds, becoming immune to magic damage and dealing 360/585/810 magic damage to nearby enemies over the duration.

**Graves** (1-cost) - Buckshot: Passive: Graves' basic attacks deal 5% bonus physical damage and hit all enemies in a cone in front of him. Graves applies on-hit effects to all enemies hit.

**Kassadin** (1-cost) - Nether Blade: Passive: Kassadin's basic attacks steal 25/50/75 mana from his target and grant him a 25/50/75 damage shield for 4 seconds.

**Kha'Zix** (1-cost) - Taste their Fear: Kha'Zix slashes the nearest enemy, dealing 150/250/350 magic damage. If the enemy has no adjacent teammates, this damage is increased to 400/600/800.

**Mordekaiser** (1-cost) - Obliterate: Mordekaiser slams his mace on the two spaces in front of him, dealing 250/500/750 magic damage to enemies within.

**Nidalee** (1-cost) - Primal Surge: Nidalee heals herself and an ally, restoring 150/375/600 health over 6 seconds, then transforms into cat form. While in cat form, Nidalee's range is reduced to 1 hex and she gains 20/70/120 Bonus Attack Damage.

**Tristana** (1-cost) - Explosive Charge: Tristana places a bomb on her current target which will detonate after she Basic Attacks them 3 times or 4 seconds pass without Tristana attacking them. When the bomb detonates, it deals 70/110/150 magic damage to all nearby enemies, plus 50 for each Basic Attack, up to a maximum of 175/275/375.

**Vayne** (1-cost) - Silver Bolts: Passive: Whenever Vayne Basic Attacks the same target 3 times, she deals 8/12/16 of her target's Maximum Health as true damage.

**Warwick** (1-cost) - Infinite Duress: Warwick pounces on the weakest enemy, stunning them for 1.5 seconds and striking them 3 times, dealing 150/225/300 magic damage. Each strike triggers on-hit effects and heals Warwick for 100% of the damage dealt.

**Ahri** (2-cost) - Orb of Deception: Ahri fires an orb in a line, dealing 100/200/300 magic damage to all enemies it passes through. It then returns to her, dealing 100/200/300 true damage to all enemies it passes through.

**Blitzcrank** (2-cost) - Rocket Grab: Blitzcrank pulls the furthest enemy to him, dealing 250/550/850 magic damage and stunning them for 2.5 seconds. His next attack after pulling knocks up for 1 second. Allies within range will prioritize attacking Blitzcrank's target.

**Braum** (2-cost) - Unbreakable: Braum puts up his shield for 4 seconds, absorbing and stopping all incoming missiles and reducing his damage taken from that direction by 70/80/90%.

**Jayce** (2-cost) - Transform Mercury Cannon: Jayce knocks a nearby enemy away, dealing 200/350/500 magic damage and stunning them for 2.5/4.25/6 seconds. He then transforms into cannon form and gains max attack speed for his next 3/5/7 Basic Attacks. While in cannon form, Jayce's Attack Range is increased by 3 hexes.

**Lissandra** (2-cost) - Frozen Tomb: Lissandra encases an enemy in ice, stunning them for 1.5 seconds and dealing 175/325/475 magic damage to all enemies in the surrounding area. If she is below 50 health, Lissandra instead encases herself, becoming untargetable for 2 seconds and dealing 175/325/475 magic damage to all enemies in the surrounding area.

**Lucian** (2-cost) - Relentless Pursuit: Lucian dashes away from his current target, then Basic Attacks them and fires a second shot which deals 125/250/375 magic damage.

**Lulu** (2-cost) - Wild Growth: Lulu makes 1/2/3 allies giant, granting them 300/400/500 bonus health and knocking up adjacent enemies near them for 1.25 seconds.

**Pyke** (2-cost) - Phantom Undertow: Pyke leaves an afterimage at his location, then dashes behind the farthest enemy. After 1 second, his afterimage returns to Pyke, dealing 150/200/250 magic damage to all enemies it passes through and stunning them for 1.5/2/2.5 seconds.

**Rek'Sai** (2-cost) - Burrow: Rek'Sai burrows, becoming untargetable for 1 second and healing for 150/300/450 health. She emerges at her target, dealing 200/350/500 magic damage and knocking them up for 1.75 seconds.

**Shen** (2-cost) - Spirit's Refuge: Shen creates a zone around himself for 3/4/5 seconds, granting all nearby allies 100% chance to dodge incoming basic attacks.

**Twisted Fate** (2-cost) - Pick a Card: Twisted Fate randomly picks a card, then throws it at his current target, dealing 150/250/350 magic damage and creating a special effect based on the color of the card. Blue: Restores 30/50/70 mana to all allies adjacent to Twisted Fate. Red: Deals 150/250/350 to all enemies adjacent to the target. Gold: Stuns the target for 2/3/4 seconds.

**Varus** (2-cost) - Piercing Arrow: Varus channels for 1.5 seconds, then fires a piercing arrow, dealing 300/550/800 magic damage to all enemies in its path.

**Zed** (2-cost) - Razor Shuriken: Zed throws a shuriken in a line, dealing 200/350/500 magic damage to all enemies in its path.

**Aatrox** (3-cost) - The Darkin Blade: Aatrox cleaves the area in front of him, dealing 300/600/900 magic damage to all enemies within.

**Ashe** (3-cost) - Enchanted Crystal Arrow: Ashe fires an arrow at the farthest enemy. It stops on the first target hit, dealing 200/400/600 magic damage and stunning them for 1/1.5/2 seconds per Hex traveled.

**Evelynn** (3-cost) - Last Caress: Evelynn deals 200/300/400 magic damage to up to 3 nearby enemies, then teleports away. This damage is increased to 600/1200/2000 against targets below 50 health.

**Gangplank** (3-cost) - Powder Kegs: Passive: Gangplank periodically places barrels near enemies. Active: Gangplank shoots his barrels, causing them all to explode, dealing 150/250/350 magic damage to nearby enemies and applying on-hit effects.

**Katarina** (3-cost) - Death Lotus: Katarina channels for 2.5 seconds, throwing knives at 4/6/8 nearby enemies, dealing 750/1125/1500 magic damage over the duration and reducing healing by 80% to her targets for 3 seconds.

**Kennen** (3-cost) - Slicing Maelstrom: Kennen summons a storm around himself for 3 seconds, calling down lightning bolts on all enemies in the area every 0.5 second and dealing 225/450/675 magic damage over the duration. Each enemy struck by 3 lightning bolts is stunned for 1.5 seconds.

**Morgana** (3-cost) - Soul Shackles: Morgana fires chains to all nearby enemies, dealing 175/300/425 magic damage. After 3 seconds, all chained enemies still within range of Morgana are stunned for 2/4/6 seconds and take 175/300/425 magic damage again.

**Poppy** (3-cost) - Keeper's Verdict: Poppy creates a shockwave in a straight line that can hit up to 1/2/3 enemies, dealing 300/500/700 magic damage, knocking up and stunning them for 2/3/4 seconds.

**Rengar** (3-cost) - Savagery: Rengar leaps to and stabs the weakest enemy, dealing 200%/300%/400% AD physical damage and applying on-hit effects to his target. After this leap, Rengar gains 30/50/70 Attack Speed and 25 Critical Strike Chance for the next 6 seconds.

**Shyvana** (3-cost) - Dragon's Descent: Shyvana dashes away from her current target and transforms into dragon form. While in dragon form, Shyvana's Basic Attack becomes ranged, she gains 100/150/200 Attack Damage, and her Basic Attacks burn her enemies for 3 seconds, dealing 200/300/400 magic damage.

**Veigar** (3-cost) - Primordial Burst: Veigar blasts an enemy with magical energy, dealing 350/650/950 magic damage. If Veigar is a higher star level than his target, this damage is increased to 19999.

**Vi** (3-cost) - Assault and Battery: Vi charges at the farthest enemy, knocking aside and dealing 300/500/700 magic damage to all enemies along the way. When she reaches her target, she deals 300/500/700 magic damage and knocks them up for 2/2.5/3 seconds.

**Volibear** (3-cost) - Thunder Claws: Volibear empowers his attacks for the next 20 seconds, causing them to chain to up to 3 nearby enemies. These chains deal 80% physical damage and apply on-hit effects.

**Akali** (4-cost) - Five Point Strike: Akali throws kunai at her target, dealing 150 magic damage. There is a 25% chance that this spell will critically strike for 225 magic damage instead.

**Aurelion Sol** (4-cost) - Voice of Light: Aurelion Sol breathes fire in a line, dealing 250/500/750 magic damage to all enemies in the area.

**Brand** (4-cost) - Pyroclasm: Brand launches a bouncing fireball at an enemy. This fireball will bounce to nearby enemies up to 4/6/20 times, dealing 250/450/650 magic damage each time it hits.

**Cho'Gath** (4-cost) - Rupture: Cho'Gath ruptures a large area, dealing 200/400/600 magic damage and knocking up all enemies within for 1.5/1.75/2 seconds.

**Draven** (4-cost) - Spinning Axes: Draven starts spinning an axe, granting him 150% bonus physical damage on-hit on his next attack. It will return to his location after striking the target and if Draven catches it, it will refresh the buff. Draven can spin up to two axes at a time.

**Gnar** (4-cost) - GNAR!: Gnar leaps towards an enemy and transforms into Mega Gnar, throwing nearby enemies, dealing 200/300/400 magic damage and stunning them for 2 seconds. While in Mega Gnar form, Gnar gains 250/450/650 Health and 50/100/150 Attack Damage.

**Jinx** (4-cost) - Get Excited!: Passive: Jinx gets excited as she helps take down enemy units. After her first takedown, Jinx gains 60/80/100 bonus Attack Speed. After her second takedown, Jinx swaps to her rocket launcher, causing her Basic Attacks to deal 100/200/300 bonus magic damage to all enemies in a small area around her target.

**Kindred** (4-cost) - Lamb's Respite: Kindred creates a zone around themselves for 3/4/5 seconds that prevents allies within from dropping below 300/600/900 health or dying.

**Leona** (4-cost) - Solar Flare: Leona calls down a solar ray in an area, dealing 175/250/325 magic damage to all enemies within and stunning the enemy in the center for 5/7/9 seconds.

**Sejuani** (4-cost) - Glacial Prison: After a 2 second delay, Sejuani creates a glacial prison on an enemy, dealing 100/175/250 magic damage to all nearby enemeis and stunning them for 2/3.5/5 seconds.

**Anivia** (5-cost) - Glacial Storm: Anivia creates a large hailstorm that lasts for 6 seconds, dealing 800/1100/1400 magic damage over the duration and slowing the attack speed of all enemies inside of it by 50/70/90%.

**Kai'Sa** (5-cost) - Killer Instinct: Kai'Sa dashes to the furthest enemy unit, gaining +50/75/100% Attack Speed and a shield that blocks 400/700/1000 damage for 3 seconds.

**Karthus** (5-cost) - Requiem: Karthus channels for 2.25 seconds, then deals 350/600/850 magic damage to 5/7/9 random enemies.

**Kayle** (5-cost) - Divine Judgement: Kayle shields her 1/2/3 weakest allies, making them immune to all damage for 2/2.5/3 seconds.

**Miss Fortune** (5-cost) - Bullet Time: Miss Fortune channels and fires waves of bullets in a cone for 3 seconds, dealing 1300/2000/2700 magic damage to all enemies within over the duration.

**Pantheon** (5-cost) - Grand Starfall: Pantheon leaps in the air, crashing down towards the farthest enemy, stunning them for 2 seconds. Enemies in Pantheon's path take 10/20/30 of their Maximum Health as magic damage. They then burn for an additional 20% of their Maximum Health as true damage over 10 seconds, reducing healing by 80%.

**Swain** (5-cost) - Demonflare: Swain transforms into demon form for 6 seconds. While in demon form, Swain drains health from nearby enemies, dealing 480/960/1440 magic damage and healing himself for 360/720/1080 over the duration for each enemy in range. At the end of the transformation, Swain sends out a Soul Flare, dealing 300/600/900 magic damage to all nearby enemies.

**Yasuo** (5-cost) - Steel Tempest: Yasuo stabs his sword into the two spaces in front of him, dealing 150/350/550 magic damage and applying on-hit effects to enemies within. Every third cast, Yasuo instead throws a tornado in a line that travels 6 hexes and additionally knocks enemies up for 1.5 seconds.

## Traits

### Assassin

Innate: At the start of combat, Assassins leap to the farthest enemy. Assassins gain bonus Critical Strike Damage and Chance.

| Units | Effect |
|---|---|
| 3-5 | +75% Critical Strike Damage & +5% Critical Strike Chance |
| 6-8 | +150% Critical Strike Damage & +20% Critical Strike Chance |
| 9 | +225% Critical Strike Damage & +30% Critical Strike Chance |

### Blademaster

Blademaster Basic Attacks have a 45% chance to trigger additional attacks against their target. These additional attacks deal damage like Basic Attacks and trigger on-hit effects.

| Units | Effect |
|---|---|
| 3-5 | One Additional Attack |
| 6-8 | Two Additional Attacks |
| 9 | Four Additional Attacks |

### Brawler

Brawlers gain bonus Maximum Health.

| Units | Effect |
|---|---|
| 2-3 | 250 Bonus Health |
| 4-5 | 500 Bonus Health |
| 6 | 900 Bonus Health |

### Demon

Basic Attacks from Demons have a 40% chance to reduce their target's Mana by 20 and restore Mana to the Demon.

| Units | Effect |
|---|---|
| 2-3 | 15 Mana restored |
| 4-5 | 30 Mana restored |
| 6 | 45 Mana restored |

### Dragon

| Units | Effect |
|---|---|
| 2 | Dragons gain 75% resistance to Magic Damage |

### Elementalist

Innate: Elementalists gain double mana from Basic Attacks.

| Units | Effect |
|---|---|
| 3 | At the start of combat, summon a Golem with 2200 Health to fight alongside your allies |

### Exile

| Units | Effect |
|---|---|
| 1 | If an Exile has no adjacent allies at the start of combat, they gain a shield equal to 100% of their maximum Health |

### Glacial

Basic Attacks from Glacials have a chance to stun their target for 1.5 seconds.

| Units | Effect |
|---|---|
| 2-3 | 20% Chance to Stun |
| 4-5 | 33% Chance to Stun |
| 6 | 50% Chance to Stun |

### Guardian

| Units | Effect |
|---|---|
| 2 | At the start of combat, Guardians grant +40 Armor to adjacent allies (this bonus can stack any number of times) |

### Gunslinger

Gunslinger Basic Attacks have a 50% chance to fire additional bullets at other enemies within range. These bullets deal damage like Basic Attacks and apply on-hit effects.

| Units | Effect |
|---|---|
| 2-3 | Attack 1 extra enemies |
| 4-5 | Attack 2 extra enemies |
| 6 | Attack 4 extra enemies |

### Hextech

At the start of combat, an ally Hextech launches and detonates a pulse bomb, temporarily disabling nearby enemy items for 5 seconds.

| Units | Effect |
|---|---|
| 2-3 | 1 Hex Radius |
| 4 | 2 Hex Radius |

### Imperial

At the start of combat, select Imperials are empowered to deal double damage.

| Units | Effect |
|---|---|
| 2-3 | 1 Random Imperial |
| 4 | All Imperials |

### Knight

All allies block a flat amount of damage from all sources.

| Units | Effect |
|---|---|
| 2-3 | 15 Damage Blocked |
| 4-5 | 30 Damage Blocked |
| 6 | 60 Damage Blocked |

### Ninja

Ninjas gain bonus Attack Damage and Spell Power. This trait is only active when you have exactly 1 or 4 unique Ninjas.

| Units | Effect |
|---|---|
| 1 | Ninja gains +50 Attack Damage & +50% Spell Power |
| 4 | All Ninjas gain +80 Attack Damage & +80% Spell Power |

### Noble

At the start of combat, select allies gain +50 Armor, +50 Magic Resistance, and heal 30 Health on-hit.

| Units | Effect |
|---|---|
| 3-5 | 1 Random Ally |
| 6 | All Allies |

### Phantom

| Units | Effect |
|---|---|
| 2 | At the start of combat, a random enemy's Health is set to 100 |

### Pirate

| Units | Effect |
|---|---|
| 3 | At the end of each combat against another player, a treasure chest spawns on your arena containing up to 4 gold |

### Ranger

Every 3 seconds, Rangers have a chance to double their Attack Speed for 3 seconds.

| Units | Effect |
|---|---|
| 2-3 | 40% Chance to Double Attack Speed |
| 4 | 70% Chance to Double Attack Speed |

### Robot

(1) Robots start combat at full Mana

| Units | Effect |
|---|---|
| 1 | active (no extra values) |

### Shapeshifter

Shapeshifters gain bonus Maximum Health when they transform.

| Units | Effect |
|---|---|
| 3-5 | 60% Bonus Maximum Health |
| 6 | 100% Bonus Maximum Health |

### Sorcerer

Innate: Sorcerers gain double Mana from Basic Attacks. All allies have increased Spell Power.

| Units | Effect |
|---|---|
| 3-5 | +40% Spell Power |
| 6-8 | +120% Spell Power |
| 9 | +200% Spell Power |

### Void

Basic Attacks and Spells from select Void champions deal true damage.

| Units | Effect |
|---|---|
| 2-3 | 1 Random Void champion |
| 4 | All Void champions |

### Wild

Select allies' Basic Attacks are empowered to grant +10% Bonus Attack Speed (stacks up to 5 times).

| Units | Effect |
|---|---|
| 2-3 | Wild Allies Only |
| 4 | All Allies, and allies' basic attacks cannot be dodged. |

### Yordle

Yordles gain a chance to dodge enemy Basic Attacks.

| Units | Effect |
|---|---|
| 3-5 | 30% Chance to Dodge |
| 6-8 | 60% Chance to Dodge |
| 9 | 90% Chance to Dodge |

## Items

Items come from the patch 9.21 CDragon file. Legacy entries left in that file from earlier sets were dropped: where two items share a recipe, the one with this set's icon tag was kept; disabled items (negative ids) and internal placeholders were dropped.

### Components

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| B. F. Sword | - | - | +15 Attack Damage |
| Chain Vest | - | 20 Armor | +20 Armor |
| Giant's Belt | - | 200 HP | +200 Health |
| Needlessly Large Rod | - | 20 AP | +20% Spell Power |
| Negatron Cloak | - | 20 MR | +20 Magic Resist |
| Recurve Bow | - | 20 Attack Speed % | +20% Attack Speed |
| Sparring Gloves | - | - | +10% Critical Strike Chance +10% Dodge Chance |
| Spatula | - | - | It must do something... |
| Tear of the Goddess | - | 20 Mana | +20 Mana |

### Completed items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Bloodthirster | B. F. Sword + Negatron Cloak | - | Basic Attacks heal the wearer for 40% of the damage dealt. |
| Cursed Blade | Recurve Bow + Negatron Cloak | - | Basic Attacks have a 20% chance on-hit to decrease the target's star level by 1 for the rest of combat. |
| Deathblade | B. F. Sword + B. F. Sword | - | Whenever the wearer kills or participates in killing an enemy, gain +15 Attack Damage for the remainder of combat. This effect can stack any number of times. Starts with 1 stacks. |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | - | Wearer gains 75% resistance to magic damage. |
| Frozen Heart | Tear of the Goddess + Chain Vest | - | Reduces the Attack Speed of adjacent enemies by 35%. |
| Giant Slayer | B. F. Sword + Recurve Bow | - | The wearer's basic attacks deal an additional 5% of the target's Maximum Health as true damage. |
| Guardian Angel | B. F. Sword + Chain Vest | - | When the wearer dies, they cleanse negative effects and revive with up to 500 Health after a 2 second delay. This effect can trigger once per combat. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | - | Basic Attacks grant the wearer +5% bonus Attack Speed for the rest of combat. This effect can stack any number of times. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | 20 Mana | At the beginning of each planning phase, the wearer gains one of the following:Basic attacks and spells deal +40% Damage Basic attacks heal 40 Health on-hit |
| Hextech Gunblade | B. F. Sword + Needlessly Large Rod | - | Basic Attacks and spells heal the wearer for 25% of the damage dealt. |
| Hush | Tear of the Goddess + Negatron Cloak | - | Basic Attacks have a 33% chance to silence the target on-hit, preventing the enemy from gaining mana for 4 seconds. |
| Iceborn Gauntlet | Chain Vest + Sparring Gloves | 20 Armor | When the wearer dodges, they create a zone of ice. Enemies inside the zone have their Attack Speed reduced by 35%. |
| Infinity Edge | B. F. Sword + Sparring Gloves | - | The wearer gains +125% Critical Strike Damage. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | - | Whenever an enemy casts a spell, the wearer deals 125 true damage to the caster. |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | 20 AP | The wearer's spells can critically strike. [Unique - Only One Per Champion] |
| Locket of the Iron Solari | Needlessly Large Rod + Chain Vest | - | When combat begins, the wearer and all allies within 2 hexes in the same row gain a shield that blocks 300 damage for 8 seconds. |
| Luden's Echo | Needlessly Large Rod + Tear of the Goddess | - | When the wearer deals damage with their spell, the first target hit and up to 3 nearby enemies are dealt an additional 180 magic damage. |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | - | When the wearer deals damage with their spell, they burn the target, dealing 20% of the target's Maximum Health as true damage over 10 seconds and reducing healing by 80% for the duration of the burn. A champion can only have one burn effect at a time, preferring the strongest. [Unique - Only One Per Champion] |
| Phantom Dancer | Recurve Bow + Chain Vest | - | Wearer dodges all critical strikes. [Unique - Only One Per Champion] |
| Quicksilver | Sparring Gloves + Negatron Cloak | 20 MR | The wearer gains a shield that prevents the next crowd control effect from applying. This shield refreshes after 3 seconds. |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | - | Wearer gains +50% Spell Power Amplification. (All sources of Spell Power are 50% more effective) |
| Rapid Firecannon | Recurve Bow + Recurve Bow | - | Wearer gains +100% Attack Range. |
| Red Buff | Chain Vest + Giant's Belt | - | Wearer's Basic Attacks burn the target on-hit, dealing 20% of the target's Maximum Health as true damage over 10 seconds and reducing healing by 80% for the duration of the burn. A champion can only have one burn effect at a time, preferring the strongest. [Unique - Only One Per Champion] |
| Redemption | Tear of the Goddess + Giant's Belt | - | When the wearer falls below 30% Health, nearby allies are healed for 1200 Health after a 2.5 second delay. This effect can trigger once per combat. |
| Repeating Crossbow | Recurve Bow + Sparring Gloves | 20 Attack Speed % | When the wearer dies, Repeating Crossbow is re-equipped to a new ally. Each time Repeating Crossbow is re-equipped, it grants an additional +30% Attack Speed and +30% Critical Strike Chance for the remainder of combat. This effect can stack any number of times. |
| Seraph's Embrace | Tear of the Goddess + Tear of the Goddess | - | After casting their spell, the wearer restores 20 Mana. |
| Spear of Shojin | B. F. Sword + Tear of the Goddess | - | After casting their spell, the wearer's Basic Attacks restore 18% of their Maximum Mana. |
| Statikk Shiv | Recurve Bow + Tear of the Goddess | - | Every third Basic Attack from the wearer deals 100 magical damage to 3 enemies. |
| Sword Breaker | Chain Vest + Negatron Cloak | - | Wearer's Basic Attacks have a 33% chance to disarm the target for 3 seconds, preventing that enemy from Basic Attacking. |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | - | At the beginning of each planning phase, the wearer equips 2 temporary items. Temporary items increase in power based on your player level. [Consumes Three Item Slots] |
| Thornmail | Chain Vest + Chain Vest | - | When the wearer is hit by a Basic Attack, they reflect 100% of the mitigated damage as magic damage. |
| Titanic Hydra | Recurve Bow + Giant's Belt | - | Basic Attacks deal an additional 3% of the wearer's Maximum Health as magic damage to the target and adjacent enemies behind them. |
| Trap Claw | Giant's Belt + Sparring Gloves | 200 HP | At the beginning of combat, the wearer gains a shield that blocks the first enemy spell that hits them. The enemy that breaks the shield is stunned for 4 seconds. |
| Warmog's Armor | Giant's Belt + Giant's Belt | - | Wearer regenerates 6% of their missing Health per second. |
| Zeke's Herald | B. F. Sword + Giant's Belt | - | When combat begins, the wearer and all allies within 2 hexes in the same row gain +15% Attack Speed for the rest of combat. |
| Zephyr | Negatron Cloak + Giant's Belt | - | When combat begins, the wearer summons a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 6 seconds. [Unique - Only One Per Champion] |

### Spatula items and emblems

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Blade of the Ruined King | Recurve Bow + Spatula | 20 Attack Speed % | Extra +20 Wearer is also a Blademaster [Unique - Only One Per Champion] |
| Darkin | Tear of the Goddess + Spatula | 20 Mana | Extra +20 Wearer is also a Demon [Unique - Only One Per Champion] |
| Force of Nature | Spatula + Spatula | - | Wearer's team gains +1 maximum team size. |
| Frozen Mallet | Spatula + Giant's Belt | 200 HP | Extra +200 Wearer is also glacial [Unique - Only One Per Champion] |
| Knight's Vow | Chain Vest + Spatula | 20 Armor | Extra +20 Wearer is also a Knight [Unique - Only One Per Champion] |
| Mittens | Sparring Gloves + Spatula | - | Wearer is also a Yordle Extra +10% Critical Strike Chance, +10% Dodge Chance [Unique - Only One Per Champion] |
| Runaan's Hurricane | Negatron Cloak + Spatula | - | Extra +20 Basic Attack fire an additional missile at another nearby enemy, dealing 75% of the wearer's Attack Damage and applying on-hit effects. |
| Youmuu's Ghostblade | B. F. Sword + Spatula | - | Extra +15 Wearer is also an Assassin [Unique - Only One Per Champion] |
| Yuumi | Needlessly Large Rod + Spatula | 20 AP | Extra +20 Wearer is also a Sorcerer [Unique - Only One Per Champion] |

### Consumables and other

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Neeko's Help | - | - | Place on a champion to create a one-star copy of them. [CONSUMABLE - This item disappears when used.] |

