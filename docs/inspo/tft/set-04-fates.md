# TFT Set 4: Fates

Sources:
- Community Dragon TFT data, patch 11.1: https://raw.communitydragon.org/11.1/cdragon/tft/en_us.json (champions, stats, abilities, traits, items, augments).
- Community Dragon character bins, patch 11.1: https://raw.communitydragon.org/11.1/game/data/characters/<unit>/<unit>.bin.json (spell calculations used to fill computed ability numbers).
- https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-10-19-notes/
- https://leagueoflegends.fandom.com/wiki/Chosen_(Teamfight_Tactics)
- https://leagueoflegends.fandom.com/wiki/Fates_(Teamfight_Tactics)

Retrieved 2026-10-15. Stats and numbers are from the data patch listed, which is the last patch of the set unless noted; they can differ from launch values. Bracketed tags in the mechanics lists name the source of each line.

## Overview

| Field | Value |
|---|---|
| Set number | 4 |
| Name | Fates |
| Live patches | 10.19 to 11.1 |
| Live dates (NA patch dates) | 2020-09-16 to 2021-01-20 |
| Data patch used | 11.1 |
| Theme | Spirit Blossom / lunar-festival setting with Ionian and Chinese mythology inspired skins (Cultist, Divine, Dusk, Elderwood, Enlightened, Exile, Fortune, Moonlight, Ninja, Spirit, Warlord). |
| Counts | 58 champion entries, 26 traits, 57 items, 0 augments |

Name note: CDragon set name is "Journeys" (internal).

## Set mechanics

- Chosen: shops can offer a Chosen champion that is already 2-star and costs 3x its 1-star price. One trait counts as 2, the unit gets +200 HP plus one bonus (at launch 400 HP, 30% spell power, 30 AD, or 25% reduced mana cost). Only one Chosen at a time; selling it re-enables Chosen offers. [10.19 notes, wiki Chosen]
- Chosen tier odds at launch by level: L1-3 100/0/0/0/0, L4 40/60/0/0/0, L5 20/50/30/0/0, L6 10/45/45/0/0, L7 0/30/40/30/0, L8 0/20/40/40/0, L9 0/0/30/40/30. Shown at least one Chosen by 2-2 from 10.21. [10.19 notes, wiki Chosen]
- Tier 5 champions that change how death works: Sett, Zilean, Lee Sin. [10.19 notes]
- Fortune trait: loss streak builds an orb payout on win. [CDragon trait text]
- Kayn trait: Kayn's Liberation / Rhaast's Embrace items chose his form. [CDragon items]

## Economy and system changes

- XP: level 7 needs 36 XP (was 32), level 8 needs 56 (was 50), level 9 needs 80 (was 66). [10.19 notes]
- Consecutive shops do not repeat unbought champions. [10.19 notes]
- Odds: L5 45/30/20/5/0, L6 30/35/25/10/0, L7 19/35/30/15/1. [10.19 notes]
- Carousel item tables fixed per carousel (first: 65% one of every item plus duplicate, 11% offense, 11% defense, 11% utility, 1.5% all Spatulas, 0.5% all Forces of Nature; see notes for later carousels). [10.19 notes]
- Completed items can no longer drop from orbs. Gargoyle Stoneplate replaced Sword Breaker; Sunfire Cape replaced Red Buff. [10.19 notes]

## Champions

Stats are 1-star base values. AS = attacks per second. Range is in hexes. Mana is start / max. Health and attack damage scale by star level (the wiki gives 180% at 2-star and 324% at 3-star). Ability numbers are listed as 1-star/2-star/3-star.

| Champion | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana |
|---|---|---|---|---|---|---|---|---|---|
| Diana | 1 | Moonlight, Assassin | 500 | 30 | 20 | 50 | 0.65 | 1 | 0 / 70 |
| Elise | 1 | Cultist, Keeper | 600 | 30 | 20 | 45 | 0.65 | 2 | 0 / 75 |
| Fiora | 1 | Enlightened, Duelist | 600 | 35 | 30 | 45 | 0.75 | 1 | 0 / 75 |
| Garen | 1 | Warlord, Vanguard | 600 | 50 | 35 | 50 | 0.6 | 1 | 0 / 100 |
| Lissandra | 1 | Moonlight, Dazzler | 550 | 20 | 20 | 40 | 0.6 | 3 | 0 / 50 |
| Maokai | 1 | Elderwood, Brawler | 700 | 40 | 30 | 55 | 0.5 | 1 | 0 / 75 |
| Nami | 1 | Enlightened, Mage | 550 | 20 | 20 | 40 | 0.65 | 3 | 60 / 100 |
| Nidalee | 1 | Warlord, Sharpshooter | 550 | 30 | 20 | 45 | 0.7 | 4 | 0 / 80 |
| Tahm Kench | 1 | Fortune, Brawler | 700 | 40 | 30 | 85 | 0.5 | 1 | 0 / 0 |
| Twisted Fate | 1 | Cultist, Mage | 550 | 20 | 20 | 45 | 0.65 | 3 | 0 / 70 |
| Vayne | 1 | Dusk, Sharpshooter | 500 | 20 | 20 | 40 | 0.9 | 4 | 0 / 0 |
| Wukong | 1 | Divine, Vanguard | 650 | 40 | 30 | 55 | 0.65 | 1 | 0 / 50 |
| Yasuo | 1 | Exile, Duelist | 550 | 30 | 20 | 50 | 0.7 | 1 | 0 / 45 |
| Annie | 2 | Fortune, Mage | 750 | 40 | 40 | 45 | 0.6 | 2 | 0 / 65 |
| Aphelios | 2 | Moonlight, Hunter | 500 | 20 | 20 | 45 | 0.75 | 4 | 50 / 80 |
| Hecarim | 2 | Elderwood, Vanguard | 750 | 45 | 30 | 55 | 0.55 | 1 | 0 / 110 |
| Janna | 2 | Enlightened, Mystic | 550 | 20 | 20 | 40 | 0.6 | 4 | 30 / 60 |
| Jarvan IV | 2 | Warlord, Keeper | 600 | 45 | 30 | 60 | 0.6 | 1 | 50 / 100 |
| Jax | 2 | Divine, Duelist | 750 | 40 | 30 | 60 | 0.75 | 1 | 60 / 110 |
| Lulu | 2 | Elderwood, Mage | 500 | 20 | 20 | 50 | 0.6 | 3 | 60 / 120 |
| Pyke | 2 | Cultist, Assassin | 700 | 50 | 30 | 50 | 0.6 | 1 | 75 / 125 |
| Sylas | 2 | Moonlight, Brawler | 700 | 40 | 80 | 70 | 0.6 | 1 | 30 / 60 |
| Teemo | 2 | Spirit, Sharpshooter | 500 | 20 | 20 | 55 | 0.7 | 3 | 0 / 60 |
| Thresh | 2 | Dusk, Vanguard | 700 | 80 | 20 | 60 | 0.55 | 2 | 50 / 100 |
| Vi | 2 | Warlord, Brawler | 700 | 40 | 30 | 65 | 0.6 | 1 | 0 / 50 |
| Zed | 2 | Ninja, Shade | 600 | 20 | 20 | 55 | 0.75 | 1 | 0 / 0 |
| Akali | 3 | Ninja, Assassin | 650 | 20 | 20 | 55 | 0.85 | 1 | 0 / 20 |
| Evelynn | 3 | Cultist, Shade | 600 | 20 | 20 | 70 | 0.7 | 1 | 0 / 60 |
| Irelia | 3 | Enlightened, Divine, Adept | 800 | 40 | 40 | 55 | 0.65 | 1 | 50 / 100 |
| Jinx | 3 | Fortune, Sharpshooter | 650 | 20 | 20 | 75 | 0.75 | 4 | 70 / 120 |
| Kalista | 3 | Cultist, Duelist | 550 | 20 | 20 | 65 | 0.9 | 3 | 0 / 0 |
| Katarina | 3 | Warlord, Fortune, Assassin | 500 | 20 | 20 | 65 | 0.65 | 1 | 0 / 90 |
| Kennen | 3 | Ninja, Keeper | 650 | 30 | 30 | 60 | 0.65 | 2 | 50 / 125 |
| Kindred | 3 | Spirit, Hunter | 650 | 20 | 20 | 70 | 0.8 | 3 | 0 / 35 |
| Lux | 3 | Divine, Dazzler | 600 | 20 | 20 | 40 | 0.7 | 4 | 40 / 100 |
| Nunu & Willump | 3 | Elderwood, Brawler | 750 | 50 | 30 | 65 | 0.55 | 1 | 0 / 85 |
| Veigar | 3 | Elderwood, Mage | 600 | 20 | 20 | 40 | 0.6 | 3 | 0 / 45 |
| Xin Zhao | 3 | Warlord, Duelist | 700 | 45 | 30 | 65 | 0.85 | 1 | 30 / 60 |
| Yuumi | 3 | Spirit, Mystic | 650 | 20 | 20 | 40 | 0.6 | 3 | 0 / 40 |
| Aatrox | 4 | Cultist, Vanguard | 800 | 40 | 40 | 60 | 0.65 | 1 | 75 / 150 |
| Ahri | 4 | Spirit, Mage | 600 | 20 | 20 | 40 | 0.75 | 4 | 0 / 80 |
| Ashe | 4 | Elderwood, Hunter | 600 | 20 | 20 | 80 | 0.75 | 4 | 0 / 30 |
| Cassiopeia | 4 | Dusk, Mystic | 750 | 30 | 20 | 55 | 0.6 | 2 | 80 / 150 |
| Jhin | 4 | Cultist, Sharpshooter | 600 | 20 | 20 | 100 | 2 | 4 | 0 / 4 |
| Morgana | 4 | Enlightened, Dazzler | 750 | 30 | 60 | 50 | 0.7 | 3 | 60 / 120 |
| Riven | 4 | Dusk, Keeper | 800 | 35 | 30 | 70 | 0.75 | 1 | 0 / 20 |
| Sejuani | 4 | Fortune, Vanguard | 1000 | 60 | 40 | 45 | 0.55 | 1 | 70 / 150 |
| Shen | 4 | Ninja, Adept, Mystic | 950 | 60 | 40 | 60 | 0.65 | 1 | 50 / 100 |
| Talon | 4 | Enlightened, Assassin | 800 | 30 | 20 | 90 | 0.8 | 1 | 0 / 40 |
| Warwick | 4 | Divine, Hunter, Brawler | 950 | 40 | 40 | 85 | 0.8 | 1 | 0 / 70 |
| Azir | 5 | Warlord, Keeper, Emperor | 950 | 30 | 20 | 70 | 0.85 | 4 | 75 / 150 |
| Ezreal | 5 | Elderwood, Dazzler | 750 | 20 | 20 | 80 | 0.9 | 4 | 90 / 150 |
| Kayn | 5 | Tormented, Shade | 900 | 40 | 40 | 80 | 1 | 1 | 0 / 50 |
| Lee Sin | 5 | Divine, Duelist | 1000 | 50 | 50 | 60 | 0.85 | 1 | 0 / 40 |
| Lillia | 5 | Dusk, Mage | 950 | 50 | 80 | 40 | 0.75 | 2 | 80 / 120 |
| Sett | 5 | The Boss, Brawler | 1100 | 60 | 30 | 60 | 0.75 | 1 | 100 / 175 |
| Yone | 5 | Exile, Adept | 950 | 40 | 40 | 80 | 0.95 | 1 | 0 / 60 |
| Zilean | 5 | Cultist, Mystic | 750 | 20 | 20 | 45 | 0.75 | 3 | 100 / 150 |

Units in the data without traits (summons, PvE, props) are left out: Galio, Nexus Minion, Sand Guard.

### Abilities

Computed values assume 100 Ability Power (the TFT base) and no items. Terms that scale with a stat other than AP are written as a percent of that stat (for example "275% AD"). "[unknown: X]" marks a value the data did not let us compute.

**Diana** (1-cost) - Pale Cascade: Diana shields herself against 200/300/400 damage over 4 seconds and summons 4/5/6 orbs to orbit around her. These orbs explode for 70/80/90 magic damage when they hit an enemy, refreshing her Shield when the final one explodes.

**Elise** (1-cost) - Spider Form: Elise transforms into a spider, gaining 35/40/45 maximum Health and causing her attacks to restore 40/80/160 Health.

**Fiora** (1-cost) - Riposte: Fiora enters a defensive stance for 1.5 seconds, becoming immune to damage and enemy spell effects. Afterwards, she strikes, dealing 250/400/600 magic damage to a nearby enemy and stunning them for 1.5/2/3 seconds.

**Garen** (1-cost) - Judgement: Garen spins his sword for 4 seconds, dealing 450/675/1125 magic damage to nearby enemies and reducing incoming magic damage by 80% over the duration.

**Lissandra** (1-cost) - 1000 Daggers: Lissandra hurls a dagger toward the enemy with the highest Attack Damage, dealing 350/450/550 magic damage to the first target it hits. After hitting its initial target or at its final destination, the dagger explodes dealing 175/225/275 magic damage to nearby targets.

**Maokai** (1-cost) - Bramble Smash: Maokai smashes the ground, sending forward a shockwave that deals 225/350/550 magic damage to enemies hit and slows their Attack Speed by 50% for 3 seconds.

**Nami** (1-cost) - Aqua Prison: Nami traps the closest enemy in her bubble, dealing 225/300/450 magic damage and stunning them for 2.5/3/4 seconds.

**Nidalee** (1-cost) - Javelin Toss: Nidalee throws her javelin at her farthest enemy, dealing 225/350/600 magic damage plus 20% more for each hex traveled.

**Tahm Kench** (1-cost) - Thick Skin: Tahm Kench takes 15/25/75 reduced damage from all sources.

**Twisted Fate** (1-cost) - Wild Cards: Twisted Fate throws three cards in a cone that deal 250/350/475 magic damage to each enemy they pass through.

**Vayne** (1-cost) - Silver Bolts: Passive: Vayne's third attack on a target deals 50/90/140 bonus true damage.

**Wukong** (1-cost) - Crushing Blow: Wukong slams his target with his staff, dealing 250/265/280 of his Attack Damage as physical damage and stunning them for 1.5/2/3 seconds. (Total Damage: 250%/265%/280% AD)

**Yasuo** (1-cost) - Striking Steel: Yasuo strikes forward two hexes, attacking for 180/200/225 of his Attack Damage (total: 180%/200%/225% AD). If he can't hit two targets from where he's standing, he'll first dash to a place where he can.

**Annie** (2-cost) - Burst Shield: Annie blasts a cone of fire, dealing 250/350/450 magic damage to enemies in front of her, then creates a Shield with 500/700/900 health on herself for 8 seconds.

**Aphelios** (2-cost) - Sentry Turret: Aphelios summons a turret for 7/8/9 seconds. An extension of his will, the turret deals damage as Aphelios, applies Aphelios' on-hit effects, and dies when Aphelios dies.

**Hecarim** (2-cost) - Spirit Of Life: Hecarim creates an aura around him for 3 seconds. Each enemy within takes 250/400/600 magic damage, and heals Hecarim for 250/400/600, over the duration.

**Janna** (2-cost) - Eye of the Storm: Janna shields her 2/3/4 lowest-Health allies for 250/300/400 for 4 seconds. Shielded allies also gain 20/30/60 Attack Damage for that duration.

**Jarvan IV** (2-cost) - Dragon Strike: Jarvan IV throws his standard near the farthest enemy and pulls himself toward it with his lance, dealing 175/250/750 magic damage to enemies he passes through, knocking them up and stunning them for 1 second.

**Jax** (2-cost) - Counter Strike: Jax dodges all incoming attacks for 2 seconds, then strikes all nearby enemies, dealing 150/250/450 magic damage and stunning them for 1.5 seconds.

**Lulu** (2-cost) - Wild Growth: Lulu makes a low-Health ally giant, knocking up nearby enemies for 1 second. Hugeified allies gain 400/600/950 bonus Health for the rest of combat.

**Pyke** (2-cost) - Phantom Undertow: Pyke leaves a phantom at his location, then dashes behind the farthest enemy. After 1 second, his phantom returns to Pyke, dealing 125/200/375 magic damage to all enemies it passes through and stunning them for 2/2.5/4 seconds.

**Sylas** (2-cost) - Chain Lash: Sylas lashes his chains out in a 3-hex line, dealing 250/400/700 magic damage to enemies hit and increasing the mana cost of their next spell by 33%.

**Teemo** (2-cost) - Sporecloud Dart: Teemo fires a dart at the enemy with the highest Attack Speed. The dart explodes into a cloud of spores on contact, poisoning nearby enemies. Poisoned enemies take 300/450/900 magic damage over 2.5/3/4.5 seconds and are blinded for the duration.

**Thresh** (2-cost) - Spectral Lantern: Thresh throws his lantern to the lowest-Health ally, shielding them and nearby allies against 250/450/750 damage for 4 seconds.

**Vi** (2-cost) - Denting Blow: Vi's attack blasts through her target, dealing 250/400/800 magic damage to all enemies in a cone and reducing their Armor by 40/60/80% for 8 seconds.

**Zed** (2-cost) - Contempt For The Weak: Passive: Every third attack, Zed deals 40/50/75 bonus magic damage and steals 25/30/35% of the target's current Attack Damage.

**Akali** (3-cost) - Five Point Strike: Akali throws kunai at her target, dealing 175/250/400 magic damage.

**Evelynn** (3-cost) - Last Caress: Evelynn deals 350/600/1500 magic damage to up to 3 nearby enemies, then teleports away. This damage is increased to 875/1500/3750 against targets below 50 Health.

**Irelia** (3-cost) - Bladestorm: Irelia launches a storm of blades in front of her, dealing 200/300/550 magic damage and disarming enemies for 2.5/3/3.5 seconds.

**Jinx** (3-cost) - Fishbones: Jinx fires a rocket at her target that explodes on impact, damaging all nearby enemies for 150/250/450 magic damage and stunning them for 1.5/2/2.5 seconds.

**Kalista** (3-cost) - Rend: Passive: Kalista's spears remain in her target, each one capable of dealing 4/6/9 of the target's maximum Health in magic damage when removed. She removes the spears when doing so would kill the target.

**Katarina** (3-cost) - Death Lotus: Katarina channels for 2.5 seconds, throwing knives at 4/6/8 nearby enemies, dealing 750/900/1600 magic damage over the duration and reducing healing by 50% to her targets for 5 seconds.

**Kennen** (3-cost) - Slicing Maelstrom: Kennen summons a storm around himself for 3 seconds, calling down lightning bolts on all enemies in the area every 0.5 second and dealing 300/450/1200 magic damage over the duration. Each enemy struck by 3 lightning bolts is stunned for 1.5 seconds.

**Kindred** (3-cost) - Dance of Dread: Wolf mauls Kindred's target, dealing 450/650/1000 magic damage and reducing their healing by 50% for 5 seconds. Meanwhile, Lamb leaps away from Kindred's target.

**Lux** (3-cost) - Light Binding: Lux fires a sphere of light towards her farthest enemy. Enemies in the sphere's path take 300/400/700 magic damage and are stunned for 1.5/2/3 seconds.

**Nunu & Willump** (3-cost) - Consume: Willump bites his target, dealing 450/650/1800 magic damage. If Willump's target has less Health than he does before the bite, it deals an additional 50% damage and becomes true damage.

**Veigar** (3-cost) - Dark Blossom: Veigar blasts the enemy with the lowest Health, dealing 450/600/900 magic damage. If this kills his target, Veigar permanently gains 1/1/2 Spell Power.

**Xin Zhao** (3-cost) - Crescent Guard: Xin Zhao sweeps around himself, dealing 330/340/350 of his Attack Damage (total: 330%/340%/350% AD) to nearby enemies and gaining 50/60/75 Armor and Magic Resist for the rest of combat.

**Yuumi** (3-cost) - Zoomies: Yuumi dashes to the lowest-Health ally, healing them for 30/45/75 of their missing Health and granting them 30/40/60 Attack Speed for 5 seconds. She then repeats this on the farthest ally.

**Aatrox** (4-cost) - Infernal Chains: Aatrox pulls 2/2/5 of the farthest enemies toward himself, then slams the ground in front of himself, dealing 250/450/1250 magic damage to all enemies hit.

**Ahri** (4-cost) - Spirit Bomb: Ahri channels energy into her spirit orb for 1.5 seconds, then lobs it toward a random enemy, dealing 500/700/3000 magic damage to all enemies in a large area. If Ahri is interrupted, she releases her orb early reducing the size of the impact area.

**Ashe** (4-cost) - Hunter's Focus: Ashe gains 45/65/300 Attack Speed for 5 seconds, and her attacks fire a flurry of arrows for 225% AD physical damage.

**Cassiopeia** (4-cost) - Petrifying Gaze: Cassiopeia projects a cone of magic energy at her target, dealing 275/400/2000 magic damage to all enemies hit, stunning them for 2.5/3/5 seconds, and causing them to take 20 increased damage while stunned.

**Jhin** (4-cost) - Whisper: Passive: Jhin always attacks 0.85/0.9/1.2 times per second. He converts each 1% of bonus Attack Speed into 0.8 Attack Damage. Passive: Every fourth shot, Jhin deals 444/500/1234 of his Attack Damage. (Total: 444%/500%/1234% AD)

**Morgana** (4-cost) - Hallowed Ground: Morgana hallows the ground beneath a random target, dealing 250/400/2000 magic damage to enemies over 5 seconds and shredding 40% of their Magic Resistance while they remain inside. Morgana heals herself for 20/25/30% of the damage dealt.

**Riven** (4-cost) - Sweeping Strikes: Riven dashes and shields herself for 175/250/750, then slashes forward dealing 175/250/750 magic damage to nearby enemies. Every third cast, Riven leaps into the air and launches forward a large wave of energy that deals 300/450/1350 magic damage.

**Sejuani** (4-cost) - Firecracker: Sejuani throws a giant firecracker at the closest enemy that explodes after a 2-second delay. Enemies in the explosion are dealt 100/200/800 magic damage and are stunned for 2.5/3/6 seconds.

**Shen** (4-cost) - Shadow Dash: Shen dashes past his target, shielding himself against 500/800/4000 damage and taunting all adjacent enemies for 4/4/8 seconds.

**Talon** (4-cost) - Truestrike: Talon stabs his target, striking them for 240/250/275% of his Attack Damage plus 85/135/400 bonus physical damage (total: (85/135/400 + 240%/250%/275% AD)). If this kills them, Talon becomes unstoppable and untargetable while he leaps to the lowest Armor enemy.

**Warwick** (4-cost) - Primal Hunger: Warwick gains 135/150/500 Attack Speed, 40/40/200% Lifesteal, and moves faster for the rest of combat. While in this state, if Warwick scores a takedown he howls, granting himself and all allies who share a Trait with him an additional 60/75/200% Attack Speed for 3 seconds.

**Azir** (5-cost) - Emperor's Divide: Azir calls forth a wall of charging soldiers from behind him, dealing 200/350/8888 magic damage to and slowing all enemies hit for 4 seconds. Enemies close to Azir are knocked back, while enemies further away are knocked up for 2 seconds.

**Ezreal** (5-cost) - Flux Barrage: Ezreal fires a barrage toward the enemy with the highest Attack Speed, healing allies it passes through for 250/450/9999 and granting them 50% Attack Speed, and damaging enemies it passes through for 250/450/9999 magic damage and slowing their Attack Speed by 50% for 5 seconds.

**Kayn** (5-cost) - Reaping Slash: Kayn dashes toward his target then slashes all adjacent enemies, dealing 375/575/6666 magic damage and increasing the mana cost of their next spell by 33%. If the slash hits exactly one target, Kayn immediately casts again. As Rhaast: Reaping Slash heals Kayn for 50% of the damage it deals. As Shadow Assassin: Reaping Slash deals an extra 50% damage for the first 10 seconds of combat.

**Lee Sin** (5-cost) - God Fist: Lee Sin punches his target, dealing 250/450/1000 magic damage, knocking them back to the edge of the battlefield, and stunning them for 2/3/10 seconds. Any enemies the target collides with during the knockback receive half the damage and are stunned for 1/1/10 second. If the target cannot be pushed back any further, they are knocked out of the battlefield instead, removing them from combat. If the target is still alive after being punched, Lee Sin will dash to them.

**Lillia** (5-cost) - Lilting Lullaby: Lillia casts a magical lullaby, causing 2/3/10 waking enemies with the highest current Health to fall asleep for 3/4/8 seconds. Enemies will awaken if they take 500 damage while sleeping, causing an additional 500/750/5000 magic damage.

**Sett** (5-cost) - Showstopper: Sett grabs his target and slams them forward, dealing 40/60/400 of their maximum Health in magic damage to them, and 20/30/200 of the slammed enemy's maximum Health to all other enemies in a large area.

**Yone** (5-cost) - Seal Fate: Yone strikes along a path in front of him, dealing 600/1200/9999 magic damage split between all enemies hit. Enemies hit are marked for death, reducing their Armor and Magic Resist by 60%. Yone's spell is then replaced with Unforgotten until there are no remaining enemies marked for death. Unforgotten (20 mana): Yone dashes to the lowest-Health enemy that is marked for death, dealing 200/400/1000 plus their missing health as magic damage.

**Zilean** (5-cost) - Rewind Fate: Zilean places a protective Time Rune on the 2/3/10 allies with the lowest Health other than himself. When they would die, they instead resurrect after 3.5/3/1 seconds, returning to combat with 350/500/3000 Health and shedding all negative effects. After resurrecting, they have 50/75/500 bonus Attack Speed for the rest of combat.

## Traits

### Adept

Adepts calm the flow of battle, reducing the Attack Speed of all enemies by 50% for a few seconds at the start of combat.

| Units | Effect |
|---|---|
| 2 | 2 seconds |
| 3 | 3.5 seconds |
| 4 | 6 seconds |

### Assassin

Innate: At the start of combat, Assassins leap to the enemy backline. Assassins gain bonus Critical Strike Damage and Chance, and their spells can critically strike.

| Units | Effect |
|---|---|
| 2-3 | +30% Critical Strike Damage & +10% Critical Strike Chance |
| 4-5 | +60% Critical Strike Damage & +25% Critical Strike Chance |
| 6 | +90% Critical Strike Damage & +40% Critical Strike Chance |

### Brawler

Brawlers gain bonus Health.

| Units | Effect |
|---|---|
| 2-3 | 400 Health |
| 4-5 | 800 Health |
| 6-7 | 1200 Health |
| 8 | 1800 Health |

### Cultist

Once your team loses 50% of their Health, Galio is summoned, slamming into the largest cluster of enemies and knocking them up. Galio's strength increases based on the total star level of all active Cultists.

| Units | Effect |
|---|---|
| 3-5 | Tyrant Galio |
| 6-8 | Demon Lord Galio |
| 9 | Supreme Overlord Galio |

### Dazzler

Dazzlers' spells reduce the Attack Damage of enemies hit for 8 seconds.

| Units | Effect |
|---|---|
| 2-3 | 40% Attack Damage reduction |
| 4 | 80% Attack Damage reduction |

### Divine

Upon attacking 6 times or dropping below 50% Health, Divine champions remove all crowd control and ascend, taking 45% reduced damage and dealing 45% bonus true damage for the duration.

| Units | Effect |
|---|---|
| 2-3 | 3 seconds |
| 4-5 | 6 seconds |
| 6-7 | 9 seconds |
| 8 | 15 seconds |

### Duelist

Innate: Duelists gain bonus Movement Speed. Duelists' attacks grant Attack Speed, up to 8 stacks.

| Units | Effect |
|---|---|
| 2-3 | +12% Attack Speed per stack |
| 4-5 | +20% Attack Speed per stack |
| 6-7 | +35% Attack Speed per stack |
| 8 | +60% Attack Speed per stack |

### Dusk

Dusk champions increase all allies' Spell Power.

| Units | Effect |
|---|---|
| 2-3 | All allies gain 20% Spell Power |
| 4-5 | And Dusk champions gain an additional 50% |
| 6 | 40% Spell Power for all, and an extra 70% for Dusk champions |

### Elderwood

Every two seconds all Elderwood champions grow, gaining bonus stats. This effect stacks up to five times.

| Units | Effect |
|---|---|
| 3-5 | 20 Armor and Magic Resist, 5 Attack Damage and Spell Power |
| 6-8 | 30 Armor and Magic Resist, 10 Attack Damage and Spell Power |
| 9 | 40 Armor and Magic Resist, 20 Attack Damage and Spell Power |

### Emperor

The Emperor deploys with two Sand Guards who can be placed anywhere on the battlefield. They do not move or attack, and die when their Emperor does.

| Units | Effect |
|---|---|
| 1 | active (no extra values) |

### Enlightened

Enlightened champions generate more Mana.

| Units | Effect |
|---|---|
| 2-3 | 40% increase |
| 4-5 | 70% increase |
| 6 | 100% increase |

### Exile

If an Exile has no adjacent allies at the start of combat, they gain:

| Units | Effect |
|---|---|
| 1 | A shield equal to 50% of their maximum health |
| 2 | And 80% Lifesteal |

### Fortune

| Units | Effect |
|---|---|
| 3-5 | Winning combat against a player will give bonus orbs. The longer you've gone without an orb, the bigger the payout! |
| 6 | Wins give an extra bonus orb with rare loot. |

### Hunter

Every few seconds, all Hunters will attack the lowest percent Health enemy, dealing bonus damage.

| Units | Effect |
|---|---|
| 2 | 125% bonus damage every 3.5 seconds |
| 3 | 150% bonus damage every 3 seconds |
| 4 | 175% bonus damage every 2.5 seconds |
| 5 | 200% bonus damage every 2 seconds |

### Keeper

At the start of combat, Keepers grant themselves and all adjacent allies a Shield for a duration. This Shield is 50% stronger on Keepers.

| Units | Effect |
|---|---|
| 2-3 | 150 Shield, 8 seconds |
| 4-5 | 225 Shield, 10 seconds |
| 6 | 350 Shield, 12 seconds |

### Mage

Mages cast twice and have modified Spell Power.

| Units | Effect |
|---|---|
| 3-5 | 80% of their total Spell Power |
| 6-8 | 120% of their total Spell Power |
| 9 | 180% of their total Spell Power |

### Moonlight

(3) At the start of combat, a number of Moonlight Champions star up once until combat ends. (Prioritizes the lowest star-level champions. If tied, champions with the most items are chosen.)

| Units | Effect |
|---|---|
| 3-4 | 1 Champion |
| 5 | 2 Champions |

### Mystic

All allies gain Magic Resistance.

| Units | Effect |
|---|---|
| 2-3 | 40 Magic Resistance |
| 4-5 | 100 Magic Resistance |
| 6 | 200 Magic Resistance |

### Ninja

Ninjas gain bonus Attack Damage and Spell Power. This trait is only active when you have exactly 1 or 4 unique Ninjas.

| Units | Effect |
|---|---|
| 1 | Ninja gains +50 Attack Damage & +50% Spell Power |
| 4 | All Ninjas gain +140 Attack Damage & +140% Spell Power |

### Shade

Innate: When combat starts, Shades teleport to the enemy backline. Shades dip into the shadows after every 3 attacks, stealthing and causing their next basic attack to deal bonus magic damage.

| Units | Effect |
|---|---|
| 2 | 125 bonus damage |
| 3 | 400 bonus damage |
| 4-28 | 750 bonus damage |

### Sharpshooter

Sharpshooters' attacks and spells ricochet to nearby enemies, dealing reduced damage.

| Units | Effect |
|---|---|
| 2-3 | 1 ricochets with 55% reduced damage |
| 4-5 | 2 ricochets with 50% reduced damage |
| 6 | 3 ricochets with 45% reduced damage |

### Spirit

The first time a Spirit casts their spell, all allies gain Attack Speed based on the spell's mana cost.

| Units | Effect |
|---|---|
| 2-3 | 35% of mana cost |
| 4-28 | 70% of mana cost |

### The Boss

When The Boss first drops below 40% Health, he leaves combat to start doing sit-ups. Each sit-up restores 15% of his maximum Health and gives him 40% Attack and Movement Speed. If he reaches full Health he returns to combat Pumped Up: all of his attacks and spells deal true damage. If all of his allies die, he will immediately return to combat.

| Units | Effect |
|---|---|
| 1 | variables: HealPercent=15; {a8ca7859}=40 |

### Tormented

Tormented can be transformed after participating in 3 combats, enhancing their abilities.

| Units | Effect |
|---|---|
| 1 | variables: {5ef02437}=3 |

### Vanguard

Vanguard champions gain bonus Armor.

| Units | Effect |
|---|---|
| 2-3 | 120 Armor |
| 4-5 | 300 Armor |
| 6-7 | 750 Armor |
| 8 | 2000 Armor |

### Warlord

Warlords have bonus Health and Spell Power. Each victorious combat they participate in increases the bonus by 10%, stacking up to 5 times.

| Units | Effect |
|---|---|
| 3-5 | 200 Health & 20 Spell Power |
| 6-8 | 450 Health & 40 Spell Power |
| 9 | 700 Health & 70 Spell Power |

## Items

Items come from the patch 11.1 CDragon file. Legacy entries left in that file from earlier sets were dropped: where two items share a recipe, the one with this set's icon tag was kept; disabled items (negative ids) and internal placeholders were dropped.

### Components

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| B. F. Sword | - | 15 AD | +15 Attack Damage |
| Chain Vest | - | 25 Armor | +25 Armor |
| Giant's Belt | - | 200 HP | +200 Health |
| Needlessly Large Rod | - | 15 AP | +15% Spell Power |
| Negatron Cloak | - | 25 MR | +25 Magic Resist |
| Recurve Bow | - | 15 Attack Speed % | +15% Attack Speed |
| Sparring Gloves | - | 10 Crit % | +10% Critical Strike Chance +10% Dodge Chance |
| Spatula | - | - | It must do something... |
| Tear of the Goddess | - | 15 Mana | +15 Mana |

### Completed items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Bloodthirster | B. F. Sword + Negatron Cloak | 15 AD, 25 MR | Basic attacks heal the holder for 40% of the damage dealt. |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | 30 Mana | After casting their spell, the wearer's Mana is set to 20. [Unique - Only One Per Champion] |
| Bramble Vest | Chain Vest + Chain Vest | 50 Armor | Negates bonus damage from incoming critical hits. On being hit by a Basic Attack, deal 80/100/150 magic damage to all nearby enemies (once every 2.5 seconds). |
| Chalice of Power | Negatron Cloak + Tear of the Goddess | 25 MR, 15 Mana | When combat begins, the wearer and all allies within 1 hex in the same row gain 35 Spell Power for the rest of combat. |
| Deathblade | B. F. Sword + B. F. Sword | 30 AD | Contributing to a kill grants the holder +20 additional Attack Damage for the rest of combat. This effect can stack any number of times (starting at 1). |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | 50 MR | Reduces incoming magic damage by 60%. |
| Frozen Heart | Tear of the Goddess + Chain Vest | 25 Armor, 15 Mana | Reduces the Attack Speed of nearby enemies by 50%. Each Frozen Heart a champion carries beyond the first increases the radius of this effect. |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | 25 Armor, 25 MR | The holder gains 15 Armor and 15 Magic Resist for each enemy targeting them. |
| Giant Slayer | B. F. Sword + Recurve Bow | 15 AD, 15 Attack Speed % | The holder's spells and basic attacks do 10% bonus damage. If the target has more than 1750 maximum Health, the bonus increases to 90%. |
| Guardian Angel | B. F. Sword + Chain Vest | 15 AD, 25 Armor | Prevents the wearer's first death, placing them in stasis instead. After 2 seconds, they return with 400 Health and shed all negative effects. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | 15 AP, 15 Attack Speed % | Basic Attacks grant +6% bonus Attack Speed for the rest of combat. This effect can stack any number of times. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | 15 Mana, 10 Crit % | At the beginning of each planning phase, the wearer gains one of the following:Basic attacks and spells deal +45% Damage. Basic attacks and spells heal for 45% of damage dealt. |
| Hextech Gunblade | B. F. Sword + Needlessly Large Rod | 15 AD, 15 AP | The holder's spells heal them for 33% of the damage dealt. Excess healing fuels a shield that protects the holder against up to 400 damage. |
| Infinity Edge | B. F. Sword + Sparring Gloves | 15 AD, 75 Crit % | Grants 75% Critical Strike Chance (including components). Each point of Critical Strike Chance above 100% becomes +1% Critical Strike Damage. [Unique - Only One Per Champion] |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | 15 AP, 25 MR | Enemies within 2 hexes have their Magic Resist reduced by 40% (does not stack). When they cast a spell, they are zapped taking magic damage equal to 225% of their max Mana. |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | 15 AP, 20 Crit % | The holder's spells can cause critical hits, and the holder gains +40% bonus critical strike damage. |
| Last Whisper | Recurve Bow + Sparring Gloves | 15 Attack Speed %, 20 Crit % | When the wearer inflicts a critical hit, the target's Armor is reduced by 80% for 3 seconds. This effect does not stack. [Unique - Only One Per Champion] |
| Locket of the Iron Solari | Needlessly Large Rod + Chain Vest | 15 AP, 25 Armor | When combat begins, the wearer and all allies within 2 hexes in the same row gain a shield that blocks 300/375/500 damage for 8 seconds. |
| Luden's Echo | Needlessly Large Rod + Tear of the Goddess | 15 AP, 15 Mana | When the holder casts their spell, the first target dealt magic damage and up to 3 nearby enemies are dealt an additional 180 magic damage, plus 180 if they are crowd-controlled. |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | 15 AP, 200 HP | When the holder deals magic damage with their spell, they burn the target, dealing 25% of the target's maximum Health as true damage over 10 seconds, and reducing healing by 50% for the duration of the burn. [Unique - Only One Per Champion] |
| Quicksilver | Sparring Gloves + Negatron Cloak | 25 MR | The wearer is immune to crowd control for the first 10 seconds of combat. [Unique - Only One Per Champion] |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | 30 AP | The holder gains 50 additional Spell Power. |
| Rapid Firecannon | Recurve Bow + Recurve Bow | 30 Attack Speed % | The holder gains 200% Attack Range, and their Basic Attacks can't miss. |
| Redemption | Tear of the Goddess + Giant's Belt | 200 HP, 15 Mana | When the wearer dies, allies are healed for 800 Health. |
| Runaan's Hurricane | Negatron Cloak + Recurve Bow | 15 Attack Speed %, 25 MR | Basic Attacks fire a bolt at another nearby enemy, dealing 90% of the wearer's Attack Damage and applying on-hit effects. These bolts can critical strike. |
| Shroud of Stillness | Chain Vest + Sparring Gloves | 25 Armor | When combat begins, shoots a beam straight ahead that delays affected enemies' first spellcast, increasing their max Mana by 33% until they cast. [Unique - Only One Per Champion] |
| Spear of Shojin | B. F. Sword + Tear of the Goddess | 15 AD, 15 Mana | The holder's basic attacks restore 5 Mana on-hit. |
| Statikk Shiv | Recurve Bow + Tear of the Goddess | 15 Attack Speed %, 15 Mana | Every third Basic Attack from the wearer deals 80 magic damage to 3/4/5 enemies, and 240 true damage if they are shielded. |
| Sunfire Cape | Chain Vest + Giant's Belt | 25 Armor, 200 HP | Every 2 seconds, a random enemy within 2 hexes is burned for 25% of their maximum health over 10 seconds. Any healing they receive is reduced by 50%. [Unique - Only One Per Champion] |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | 20 Crit % | At the beginning of each planning phase, the wearer equips 2 temporary items. Temporary items increase in power based on your player level. [Consumes Three Item Slots] |
| Titan's Resolve | Chain Vest + Recurve Bow | 15 Attack Speed %, 25 Armor | When the wearer takes damage or inflicts a critical hit, they gain a 2% stacking damage bonus. Stacks up to 25 times, at which point the wearer gains 25 Armor and Magic Resistance, and increases in size. |
| Trap Claw | Giant's Belt + Sparring Gloves | 200 HP | Blocks the first enemy spell that hits the wearer, and stuns the spell's caster for 4 seconds. |
| Warmog's Armor | Giant's Belt + Giant's Belt | 400 HP | Grants 1000 bonus Health (including components). |
| Zeke's Herald | B. F. Sword + Giant's Belt | 15 AD, 200 HP | When combat begins, the wearer and all allies within 1 hexes in the same row gain +35% Attack Speed for the rest of combat. |
| Zephyr | Negatron Cloak + Giant's Belt | 25 MR, 200 HP | When combat begins, the wearer summons a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. [Unique - Only One Per Champion] |
| Zz'Rot Portal | Recurve Bow + Giant's Belt | 15 Attack Speed %, 200 HP | At the start of combat, the wearer taunts enemies within 2 hexes for 1 second. When the wearer dies, a Construct with 1500/2250/3000 Health arises and taunts enemies again. |

### Spatula items and emblems

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Aspect of Dusk | Needlessly Large Rod + Spatula | 15 AP | The holder gains the Dusk trait. [Unique - Only One Per Champion] |
| Duelist's Zeal | Recurve Bow + Spatula | 15 Attack Speed % | The holder gains the Duelist trait. [Unique - Only One Per Champion] |
| Elderwood Sprout | Negatron Cloak + Spatula | 25 MR | The holder gains the Elderwood trait. [Unique - Only One Per Champion] |
| Force of Nature | Spatula + Spatula | - | Wearer's team gains +1 maximum team size. |
| Mage's Hat | Tear of the Goddess + Spatula | 15 Mana | The holder gains the Mage trait. [Unique - Only One Per Champion] |
| Sword of the Divine | Spatula + B. F. Sword | 15 AD | The holder gains the Divine trait. [Unique - Only One Per Champion] |
| Vanguard's Cuirass | Chain Vest + Spatula | 25 Armor | The holder gains the Vanguard trait. [Unique - Only One Per Champion] |
| Warlord's Banner | Spatula + Giant's Belt | 200 HP | The wearer gains the Warlord trait. [Unique - Only One Per Champion] |
| Youmuu's Ghostblade | Sparring Gloves + Spatula | 10 Crit % | The wearer gains the Assassin trait. [Unique - Only One Per Champion] |

### Consumables and other

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Kayn's Liberation | - | - | Place on Kayn to free him from Rhaast, permanently improving his Reaping Slash spell to deal bonus damage at the start of combat. [KAYN CONSUMABLE - This item disappears when used and DESTROYS Rhaast's Embrace] [Can only be used during the Planning Phase] |
| Neeko's Help | - | - | Place on a champion to create a one-star copy of them. [CONSUMABLE - This item disappears when used.] |
| Rhaast's Embrace | - | - | Place on Kayn to force Rhaast to overtake him, permanently improving his Reaping Slash spell to heal him. [KAYN CONSUMABLE - This item disappears when used and DESTROYS Kayn's Liberation] [Can only be used during the Planning Phase] |

