# TFT Set 7.5: Dragonlands: Uncharted Realms

Sources:
- Community Dragon TFT data, patch 12.22: https://raw.communitydragon.org/12.22/cdragon/tft/en_us.json (champions, stats, abilities, traits, items, augments).
- Community Dragon character bins, patch 12.22: https://raw.communitydragon.org/12.22/game/data/characters/<unit>/<unit>.bin.json (spell calculations used to fill computed ability numbers).
- Community Dragon TFT data, patch 13.11: https://raw.communitydragon.org/13.11/cdragon/tft/en_us.json (item and augment pool lists).
- https://teamfighttactics.leagueoflegends.com/en-us/news/game-updates/teamfight-tactics-patch-12-17-notes/
- https://leagueoflegends.fandom.com/wiki/Dragonlands:_Uncharted_Realms_(Teamfight_Tactics)

Retrieved 2026-10-15. Stats and numbers are from the data patch listed, which is the last patch of the set unless noted; they can differ from launch values. Bracketed tags in the mechanics lists name the source of each line.

## Overview

| Field | Value |
|---|---|
| Set number | 7.5 |
| Name | Dragonlands: Uncharted Realms |
| Live patches | 12.17 to 12.22 |
| Live dates (NA patch dates) | 2022-09-08 to 2022-12-06 |
| Data patch used | 12.22 |
| Theme | Mid-set update to Dragonlands. Traits added: Darkflight, Lagoon, Monolith, Prodigy. Removed: Legend, Revel, Trainer. New 3-cost Dragons. |
| Counts | 64 champion entries, 29 traits, 138 items, 208 augments |

## Set mechanics

- Three Treasure Dragon versions: Chaos Dragon 30% of games, Order Dragon 30% (guarantees a Radiant item), classic 40%. All players get the same version. [12.17 notes]
- Dragons are no longer unique (still 2 slots). Dragon became a 1/2/3/4/5 trait: (1) HP, (2) +20 Armor & MR, (3) +10% damage, (4) +1 army size, (5) +10% attack speed. CDragon at 12.22 shows a sixth breakpoint ("Dragons ascend"). [12.17 notes, CDragon]
- Dragons cost 6/7/8 gold at tier 3/4/5. All-Dragon carousel past stage 5 chance 5% to 15%. [12.17 notes]
- Armory selection menus get a 1-second input lockout. [12.17 notes]

## Champions

Stats are 1-star base values. AS = attacks per second. Range is in hexes. Mana is start / max. Health and attack damage scale by star level (the wiki gives 180% at 2-star and 324% at 3-star). Ability numbers are listed as 1-star/2-star/3-star.

| Champion | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana |
|---|---|---|---|---|---|---|---|---|---|
| Ezreal | 1 | Tempest, Swiftshot | 500 | 15 | 15 | 35 | 0.7 | 3 | 0 / 40 |
| Karma | 1 | Jade, Dragonmancer | 500 | 20 | 20 | 40 | 0.7 | 4 | 0 / 50 |
| Leona | 1 | Mirage, Guardian | 650 | 45 | 45 | 75 | 0.55 | 1 | 40 / 80 |
| Malphite | 1 | Lagoon, Bruiser | 700 | 40 | 40 | 80 | 0.6 | 1 | 90 / 90 |
| Nasus | 1 | Shimmerscale, Guardian | 600 | 40 | 40 | 60 | 0.55 | 1 | 30 / 90 |
| Nidalee | 1 | Astral, Shapeshifter | 600 | 30 | 30 | 60 | 0.7 | 4 | 0 / 50 |
| Sejuani | 1 | Guild, Cavalier | 650 | 40 | 40 | 60 | 0.55 | 1 | 25 / 75 |
| Senna | 1 | Ragewing, Cannoneer | 500 | 15 | 15 | 50 | 0.7 | 4 | 15 / 75 |
| Sett | 1 | Ragewing, Dragonmancer | 650 | 40 | 40 | 60 | 0.7 | 1 | 30 / 60 |
| Skarner | 1 | Astral, Bruiser | 650 | 45 | 45 | 60 | 0.6 | 1 | 40 / 90 |
| Taliyah | 1 | Lagoon, Mage | 550 | 15 | 15 | 30 | 0.7 | 4 | 0 / 60 |
| Vladimir | 1 | Astral, Mage | 700 | 45 | 45 | 40 | 0.6 | 2 | 0 / 70 |
| Wukong | 1 | Jade, Warrior | 650 | 45 | 45 | 65 | 0.7 | 1 | 0 / 60 |
| Aphelios | 2 | Darkflight, Cannoneer | 500 | 20 | 20 | 60 | 0.7 | 4 | 80 / 175 |
| Braum | 2 | Scalescorn, Guardian | 800 | 60 | 60 | 85 | 0.6 | 1 | 30 / 60 |
| Gnar | 2 | Jade, Shapeshifter | 750 | 40 | 40 | 50 | 0.7 | 2 | 0 / 70 |
| Jax | 2 | Jade, Shimmerscale, Bruiser | 750 | 45 | 45 | 55 | 0.7 | 1 | 70 / 140 |
| Kai'Sa | 2 | Lagoon, Dragonmancer | 550 | 25 | 25 | 35 | 0.8 | 4 | 0 / 45 |
| Lillia | 2 | Scalescorn, Cavalier, Mage | 750 | 50 | 50 | 50 | 0.75 | 1 | 0 / 50 |
| Lux | 2 | Astral, Mage | 600 | 20 | 20 | 30 | 0.7 | 4 | 0 / 60 |
| Qiyana | 2 | Tempest, Assassin | 700 | 35 | 35 | 55 | 0.75 | 1 | 30 / 90 |
| Rell | 2 | Darkflight, Cavalier | 700 | 45 | 45 | 50 | 0.6 | 1 | 0 / 70 |
| Twitch | 2 | Guild, Swiftshot | 550 | 20 | 20 | 55 | 0.7 | 3 | 40 / 80 |
| Yone | 2 | Mirage, Warrior | 800 | 40 | 40 | 55 | 0.85 | 1 | 0 / 0 |
| Zac | 2 | Lagoon, Guardian | 800 | 45 | 45 | 60 | 0.6 | 1 | 0 / 60 |
| Zyra | 2 | Whispers, Evoker | 600 | 20 | 20 | 40 | 0.65 | 4 | 50 / 125 |
| Diana | 3 | Scalescorn, Assassin | 700 | 35 | 35 | 55 | 0.75 | 1 | 30 / 90 |
| Heimerdinger | 3 | Mage | 600 | 25 | 25 | 50 | 0.65 | 4 | 50 / 100 |
| Lee Sin | 3 | Tempest, Dragonmancer | 800 | 50 | 50 | 55 | 0.75 | 1 | 30 / 90 |
| Lulu | 3 | Evoker | 600 | 25 | 25 | 50 | 0.65 | 4 | 50 / 100 |
| Nomsy | 3 | Prodigy, Dragon, Mage | 650 | 30 | 30 | 60 | 0.75 | 4 | 0 / 90 |
| Nomsy | 3 | Prodigy, Dragon, Cannoneer | 650 | 30 | 30 | 60 | 0.75 | 4 | 0 / 90 |
| Nomsy | 3 | Prodigy, Dragon, Evoker | 650 | 30 | 30 | 60 | 0.75 | 4 | 0 / 90 |
| Nunu & Willump | 3 | Mirage, Cavalier | 850 | 50 | 50 | 65 | 0.55 | 1 | 40 / 100 |
| Olaf | 3 | Scalescorn, Bruiser, Warrior | 800 | 50 | 50 | 55 | 0.7 | 1 | 30 / 100 |
| Rakan | 3 | Ragewing, Guardian, Mystic | 800 | 50 | 50 | 55 | 0.75 | 2 | 15 / 75 |
| Rengar | 3 | Darkflight, Assassin | 700 | 30 | 30 | 70 | 0.75 | 1 | 70 / 140 |
| Seraphine | 3 | Lagoon, Evoker, Mystic | 750 | 30 | 30 | 50 | 0.7 | 3 | 100 / 175 |
| Sylas | 3 | Whispers, Mage, Bruiser | 800 | 50 | 50 | 60 | 0.6 | 1 | 60 / 120 |
| Tristana | 3 | Cannoneer | 600 | 25 | 25 | 60 | 0.75 | 4 | 50 / 100 |
| Varus | 3 | Astral, Swiftshot | 650 | 25 | 25 | 55 | 0.7 | 3 | 75 / 150 |
| Volibear | 3 | Shimmerscale, Dragonmancer | 800 | 50 | 50 | 50 | 0.75 | 1 | 0 / 40 |
| Zeri | 3 | Lagoon, Cannoneer | 600 | 20 | 20 | 65 | 0.75 | 4 | 20 / 70 |
| Zippy | 3 | Guild, Dragon | 800 | 50 | 50 | 85 | 0.75 | 1 | 40 / 100 |
| Daeja | 4 | Mirage, Dragon | 800 | 30 | 30 | 20 | 0.6 | 4 | 0 / 100 |
| Graves | 4 | Tempest, Cannoneer | 800 | 30 | 30 | 65 | 0.65 | 3 | 0 / 40 |
| Hecarim | 4 | Ragewing, Cavalier | 1000 | 60 | 60 | 60 | 0.75 | 1 | 30 / 90 |
| Idas | 4 | Shimmerscale, Dragon, Guardian | 950 | 60 | 60 | 90 | 0.55 | 1 | 80 / 160 |
| Jayce | 4 | Guild, Shapeshifter | 900 | 25 | 25 | 60 | 0.8 | 3 | 20 / 80 |
| Nilah | 4 | Lagoon, Assassin | 800 | 45 | 45 | 70 | 0.75 | 2 | 0 / 50 |
| Pantheon | 4 | Whispers, Warrior | 1000 | 50 | 50 | 60 | 0.65 | 1 | 30 / 60 |
| Shi Oh Yu | 4 | Jade, Mystic, Dragon | 850 | 60 | 60 | 75 | 0.75 | 1 | 30 / 100 |
| Sohm | 4 | Lagoon, Mage, Dragon | 800 | 30 | 30 | 30 | 0.75 | 4 | 0 / 40 |
| Sy'fen | 4 | Whispers, Bruiser, Dragon | 900 | 60 | 60 | 90 | 0.7 | 1 | 100 / 170 |
| Tyrant Swain | 4 | Darkflight, Dragon | 900 | 70 | 70 | 40 | 0.6 | 2 | 60 / 110 |
| Xayah | 4 | Ragewing, Swiftshot | 750 | 25 | 25 | 70 | 0.75 | 3 | 0 / 60 |
| Ao Shin | 5 | Tempest, Dragon | 800 | 35 | 35 | 60 | 0.8 | 4 | 0 / 190 |
| Aurelion Sol | 5 | Astral, Evoker, Dragon | 800 | 35 | 35 | 60 | 0.75 | 4 | 0 / 60 |
| Bard | 5 | Guild, Mystic, Bard | 850 | 40 | 40 | 60 | 0.7 | 4 | 160 / 240 |
| Shyvana | 5 | Ragewing, Shapeshifter, Dragon | 900 | 70 | 70 | 120 | 0.8 | 1 | 15 / 60 |
| Soraka | 5 | Jade, Starcaller | 800 | 30 | 30 | 40 | 0.7 | 4 | 100 / 200 |
| Terra | 5 | Monolith, Dragon | 1050 | 100 | 100 | 80 | 0.65 | 1 | 30 / 70 |
| Yasuo | 5 | Mirage, Dragonmancer, Warrior | 1000 | 50 | 50 | 90 | 1 | 1 | 0 / 25 |
| Zoe | 5 | Shimmerscale, Spell Thief, Mage | 800 | 20 | 20 | 45 | 0.75 | 4 | 40 / 110 |

Units in the data without traits (summons, PvE, props) are left out: Jade Statue, Nomsy, Seastone, Target Dummy, The Golden Egg, Tome of Traits, Voidspawn, Zoe's Daisy.

### Abilities

Computed values assume 100 Ability Power (the TFT base) and no items. Terms that scale with a stat other than AP are written as a percent of that stat (for example "275% AD"). "[unknown: X]" marks a value the data did not let us compute.

**Ezreal** (1-cost) - Mystic Shot: Ezreal fires an energy bolt towards his target. The first enemy hit takes 200/275/415 magic damage and grants Ezreal 10 bonus Attack Speed, stacking up to 5 times.

**Karma** (1-cost) - Inner Flame: Karma fires a burst of energy towards her target, dealing 200/265/380 magic damage in a small area around the first enemy hit.

**Leona** (1-cost) - Solar Barrier: Leona creates a barrier around herself, reducing all incoming damage by 35/45/70 for 4 seconds.

**Malphite** (1-cost) - Coral Shield: Malphite shields himself for (200/250/325 + 10% max Health) (200/250/325 + 10% maximum health) for 5 seconds.

**Nasus** (1-cost) - Fury of the Dawn: Nasus surrounds himself in light for 6 seconds, gaining 225/350/650 maximum health and dealing 70/100/205 magic damage to adjacent enemies every second.

**Nidalee** (1-cost) - Primal Surge: Nidalee transforms into Cougar Form for the rest of combat, reducing her Attack Range to 1 and gaining bonus movement speed and 40/45/50 Attack Speed. While in cougar form, every 3rd attack swipes her target for 225% AD physical damage (225% Attack Damage).

**Sejuani** (1-cost) - Warrior's Wrath: Sejuani swings her mace wide, hitting all enemies in a cone for 6% max Health magic damage (6 maximum Health). She quickly strikes again, repeating the damage on her target and the enemy directly behind it and stunning them for 1.5/1.75/2 seconds.

**Senna** (1-cost) - Last Embrace: Senna launches black mist toward the farthest enemy, striking the first enemy hit and dealing 200%/240%/300% AD physical damage (200/240/300% Attack Damage) on a small area and an additional 300/450/650 magic damage to the target hit.

**Sett** (1-cost) - Knuckle Down: For the next 4 seconds, Sett gains 50/60/80 Armor and Magic Resist, and every other punch deals 150%/155%/170% AD bonus physical damage (150/155/170% Attack Damage).

**Skarner** (1-cost) - Crystalline Exoskeleton: Skarner shields himself for 300/375/450 for 8 seconds, and gains 160/180/200% Attack Speed while it holds.

**Taliyah** (1-cost) - Flowing Volley: Taliyah throws 3 stones at her target, each dealing 160/240/380 to the first enemy hit.

**Vladimir** (1-cost) - Transfusion: Vladimir deals 275/325/375 magic damage to the target and heals for 275/325/375 Health.

**Wukong** (1-cost) - Crushing Blow: Wukong slams his target with his staff, dealing (50/75/100 + 185%/200%/250% AD) physical damage (185/200/250% Attack Damage plus 50/75/100) and stunning them for 1/1.25/1.5 seconds.

**Aphelios** (2-cost) - Binding Eclipse<br>: Aphelios fires 5/5/7 bolts in a cone, each deaing 325%/350%/400% AD physical damage (325/350/400% Attack Damage). Enemies can only take damage from one bolt. He then stuns all enemies hit for 1.25/1.5/1.75 second.

**Braum** (2-cost) - Unbreakable: Braum raises his shield towards the largest group of enemies for 4 seconds, reducing his damage taken from that direction by [unknown: ModifiedShieldDR].

**Gnar** (2-cost) - Mega Gnar: Gnar transforms into Mega Form, jumping to his target and knocking up nearby enemies for 1 second. Gnar also gains 60/90/150 Attack Damage and 40/60/90 Armor, but reduces his Attack Range to 1.

**Jax** (2-cost) - Counter Strike: Jax dodges all incoming attacks for 2 seconds, then strikes all nearby enemies, dealing 225/350/750 magic damage and stunning them for 1/1.25/1.5 seconds.

**Kai'Sa** (2-cost) - Tidal Burst: Kai'Sa fires 4 missiles split between her target and up to 2 other targets within her attack range that each deal 40/55/85 magic damage. This increases by 2 missiles each cast, up to 12.

**Lillia** (2-cost) - Watch Out!: Lillia strikes a small area around her target's current location, dealing 215/275/370 magic damage spread amongst all enemies hit and 190/240/340 bonus magic damage to the enemy at the center of the blast.

**Lux** (2-cost) - Cosmic Flare: Lux fires a star towards the farthest enemy, dealing 325/410/525 magic damage to the first enemy struck, and 175/210/240 magic damage to subsequent enemies.

**Qiyana** (2-cost) - Elemental Blade: Qiyana dashes to the best position to strike enemies with her blade, dealing 225/300/450 magic damage in a short line and disarming enemies hit for 1.5/1.75/2 seconds.

**Rell** (2-cost) - Iron Bond: Rell forms a tether between herself and her lowest percent health ally, dealing 160/240/440 magic damage to all enemies between herself and her bound ally. She also grants herself and her bound ally 35/40/50% of Rell's current Armor and Magic Resist for 4 seconds.

**Twitch** (2-cost) - Blast Potion: Twitch hurls an exploding flask at his target, dealing (40/80/150 + 160% AD) physical damage (160% Attack Damage + 40/80/150) in an area and reducing the Armor of enemies hit by 40% for 5 seconds.

**Yone** (2-cost) - Way of the Hunter: Passive: Yone's attacks alternate between dealing 85/125/180 bonus magic damage and dealing 30%/35%/40% AD bonus physical damage (30/35/40% Attack Damage).

**Zac** (2-cost) - Unstable Current: Zac explodes outward towards enemies within 2 hexes, dealing 10/12/18 of their maximum Health as magic damage and healing himself for 220/250/300.

**Zyra** (2-cost) - Grasping Spines: Zyra summons vines in the row with the most enemies, dealing 225/325/550 magic damage and stunning them for 1/1.25/1.5 seconds.

**Diana** (3-cost) - Pale Cascade: Diana shields herself against 300/350/400 damage over 6 seconds and summons 5/6/7 orbs to orbit around her. These orbs explode for 90/105/120 magic damage when they hit an enemy, refreshing her Shield when the final one explodes.

**Heimerdinger** (3-cost) - Egg Toss: Heimerdinger lobs an egg at the highest percent Health enemy, dealing 225/325/475 magic damage and stunning them for 0.5 seconds.

**Lee Sin** (3-cost) - Dragon's Rage: Lee Sin kicks his target, stunning them for 1.25 seconds and knocking them back. The target and all enemies they hit take 215/285/385 magic damage and have their Magic Resist reduced by 40% for 5 seconds.

**Lulu** (3-cost) - Whimsy: Lulu enchants the 3 units nearest to her. Enchanted allies gain 40/50/70 bonus Attack Speed for 5 seconds. Enchanted enemies are stunned and transformed into a docile feline for 1 seconds, taking 20% increased damage. If there are less than 3 units nearby, Lulu will enchant herself.

**Nomsy** (3-cost) - Fireballs!: Nomsy bellows, lobbing a massive fireball towards a random enemy that explodes on impact, dealing 190/270/420 magic damage to enemies within 1 hex, and bouncing additional fireballs outward that deal 30/35/40% of the damage.

**Nomsy** (3-cost) - Fireballs!: Nomsy bellows, lobbing a massive fireball towards a random enemy that explodes on impact, dealing 190/270/420 magic damage to enemies within 1 hex, and bouncing additional fireballs outward that deal 30/35/40% of the damage.

**Nomsy** (3-cost) - Fireballs!: Nomsy bellows, lobbing a massive fireball towards a random enemy that explodes on impact, dealing 190/270/420 magic damage to enemies within 1 hex, and bouncing additional fireballs outward that deal 30/35/40% of the damage.

**Nunu & Willump** (3-cost) - Consume: Nunu encourages Willump to bite his target, dealing 375/525/925 magic damage. If Willump's target has less Health than he does before the bite, it deals an additional 50% damage.

**Olaf** (3-cost) - Recklessness: Passive: Olaf seeks glorious death, permanently gaining 4 Attack Damage (8 in Hyper Roll) whenever he dies (Current Bonus: @TFTUnitProperty.:TFT7_OlafPassiveBonusAD@ Attack Damage). Active: Olaf strikes his target, dealing 220%/230%/240% AD physical damage (220/230/240% Attack Damage) and gaining 35/45/60 Attack Speed for 6 seconds. If he's below 50% Health, the Attack Speed is doubled.

**Rakan** (3-cost) - Disarming Diversion: Rakan dashes to the furthest enemy within his attack range, disarming all units he passes through for 1.5 seconds, and taunting his target. Then for 4 seconds, Rakan creates a shield with 230/350/650 health on himself and his attacks grant him an additional 50/70/100 health shield.

**Rengar** (3-cost) - Unseen Predator: Rengar leaps to the lowest armor enemy, dealing (60/80/100 + 240%/245%/275% AD) physical damage (240/245/275% Attack Damage + 60/80/100). He then gains a burst of attack speed for his next 2 attacks.

**Seraphine** (3-cost) - Serenade of the Seas: Seraphine sings a serenade of the seas, granting a 160/220/300 shield and 20/35/65 magic damage on attacks to all allies within 2 hexes for 4 seconds.

**Sylas** (3-cost) - Petricite Burst: Sylas whirls his chains, dealing 140/200/320 magic damage to enemies hit and shielding himself for 365/415/525 for 6 seconds. If Sylas has already shielded himself, he hits in a larger area and applies Mana-Reave to enemies hit, increasing the cost of their next ability by 35%.

**Tristana** (3-cost) - Explosive Charge: Tristana fires up to 1 explosive charges, prioritizing the closest targets without a charge. Charges detonate after 1 seconds, dealing 140%/170%/210% AD physical damage (140/170/210% Attack Damage) to nearby enemies and an additional 130/160/200 magic damage to the target.

**Varus** (3-cost) - Chain of Constellations: Varus sends out a cosmic tendril towards his target that strikes the first enemy hit, dealing 300% AD physical damage (300% Attack Damage) and stunning them for 1.5/1.75/2 seconds. Tendrils then spread to 3 nearby enemies, dealing 150/250/400 magic damage and stunning them for the same duration.

**Volibear** (3-cost) - Relentless Storm: Volibear rages, gaining 400/700/1200 bonus Health. For the rest of combat, every 3rd attack deals 130/185/250 magic bonus magic damage to the target, and 130/185/250 magic damage to up to 4 additional enemies.

**Zeri** (3-cost) - Watershock Laser: Zeri fires a water pulse at the closest enemy within 3 hexes, or at the centermost enemy, stunning them briefly for 0.75 seconds. When the pulse explodes it deals 110%/115%/120% AD physical damage (110/115/120% Attack Damage) and creates a laser behind the target that deals 275/375/525 magic damage to all enemies hit.

**Zippy** (3-cost) - Somersault Assault: Zippy shields himself for 375/450/650 for 2 seconds and somersaults toward the highest percent health enemy, dealing 400%/425%/600% AD physical damage (400/425/600% Attack Damage) and knocking them back slightly.

**Daeja** (4-cost) - Windblast: Passive: Daeja's attacks launch three barrages that each deal 25/40/150 bonus magic damage and reduce the target's Magic Resist by 5. Active: Daeja sends a wind blast toward the most enemies in a line, dealing 250/335/1350 magic damage.

**Graves** (4-cost) - Quickdraw: Passive: Graves' attacks spread in a cone of 4 bullets in front of him, each striking the first enemy hit for 65/65/145% of his Attack Damage (damage per bullet: 65%/65%/145% AD). Active: Graves dashes towards his target, quickly fires two attacks, and grants himself 30/40/80 Armor and Magic Resist for 4 seconds.

**Hecarim** (4-cost) - Onslaught of Shadows: Hecarim summons spectral riders that charge through his target, dealing 200/300/1500 magic damage and stunning enemies hit for 1.5/2/8 seconds.

**Idas** (4-cost) - Golden Scales: Idas hardens her scales for 2 seconds, reducing incoming damage by 30/50/300. She then roars, healing herself for 450/600/3000 Health and shielding other allies for 150/250/1200 for 5 seconds. The shield grants 30% Attack Speed while it holds.

**Jayce** (4-cost) - Mercurial Justice: Jayce transforms into his melee form, then slams the ground around his target, dealing 300/400/1500 magic damage and gaining 40 Armor and Magic Resist. He replaces his Ability with Mercurial Judgement for the rest of combat. Mercurial Judgment: Jayce calls lightning upon his target, smiting them for 400/550/2000 magic damage and dealing an additional 225/275/1200 magic damage to other enemies in the area. He then heals for 100/150/500 health and an additional 20% per enemy struck.

**Nilah** (4-cost) - Slipstream: Nilah cracks her whip-blade at her target, dashing through them and dealing (130/200/700 + 200%/225%/500% AD) physical damage (200/225/500% Attack Damage + 130/200/700). If this does damage to a shield, she gains a shield equal to 80/100/300% of the damage she deals to it for 4 seconds. She instantly recasts if the target is killed or remains shielded.

**Pantheon** (4-cost) - Aegis Assault: Passive: Pantheon takes 20/25/40% less damage. Active: Pantheon braces his shield for 2 seconds, doubling the effectiveness of his passive and dealing (70/110/700 + 265%/280%/380% AD) physical damage (265/280/380% Attack Damage + 70/110/700) in the area in front of him over the duration. Enemies hit have their incoming healing reduced by 50% for 3 seconds.

**Shi Oh Yu** (4-cost) - Jade Form: Shi Oh Yu enters Jade stance, gaining [unknown: ModifiedDamageReduction] damage reduction, immunity to crowd control, and empowering her next 3 attacks with special effects that deal 250%/300%/1000% AD damage (250/300/1000% Attack Damage). Attack 1: Deals 250%/300%/1000% AD physical damage and knocks the target into the air for 1 seconds. Attack 2: Deals 250%/300%/1000% AD true damage. Attack 3: Ends the stance, dealing 250%/300%/1000% AD physical damage to all enemies in a line and knocking them up for 1 seconds.

**Sohm** (4-cost) - Tideblossom: Sohm sends out a wave to an unmarked target, marking them with a Tideblossom and dealing 165/225/600 magic damage to enemies along the way. Tideblossoms transfer to a nearby target if the enemy dies. After 3 casts, his next cast instead summons a vortex under each Tideblossom, dealing 285/400/1400 magic damage to enemies in the area.

**Sy'fen** (4-cost) - Rampage: Sy'fen charges toward the farthest enemy within 2.5 hexes, dealing 150%/170%/450% AD physical damage (150/170/450% of Attack Damage) and knocking up enemies he passes through. After charging, Sy'fen bites an enemy, dealing 450%/550%/2500% AD physical damage (450/550/2500% Attack Damage) and ignoring 33% of the target's Armor. This bite cannot be dodged.

**Tyrant Swain** (4-cost) - Dragon Master's Decree: Swain releases his dragonling flock, launching 8 dragonlings toward nearby enemies that each deal 90/130/600 magic damage to their target, then return to Swain and heal him for 12% of his missing Health.

**Xayah** (4-cost) - Feathers FLY!: For 4 seconds, whenever Xayah attacks she fires a feather behind her target and 2/2/4 nearby enemies. At the end of this duration Xayah recalls her feathers, dealing (13/20/75 + 20%/20%/30% AD) physical damage (20/20/30% Attack Damage + 13/20/75) per feather to enemies struck.

**Ao Shin** (5-cost) - Lightning Rain: Ao Shin fires a barrage of 20/20/100 lightning strikes at random enemies. Each strike deals 185/275/2000 magic damage and reduces 10 current Mana from his target.

**Aurelion Sol** (5-cost) - Black Hole: Aurelion Sol summons an unstable black hole underneath a random enemy. After 2 seconds it implodes, dealing 370/600/4000 magic damage to all enemies in the area and increasing their damage taken by 20% for 10 seconds. Black holes generated after 18 seconds of combat are larger and deal 20% more damage.

**Bard** (5-cost) - Tempered Fate: Bard sends magical energy toward the largest group of enemies, stunning them for 1.25/2/15 seconds and causing them to take 15/20/9999 increased damage while stunned. If he hits at least 1 enemy, Bard dances in celebration.

**Shyvana** (5-cost) - Dragon's Descent: Shyvana transforms into Dragon Form for the rest of combat, replacing her ability with Flame Breath. She then becomes untargetable before dive-bombing the largest group of enemies, dealing 80/150/1500 magic damage and stunning them for 1.25/1.25/10 seconds. After landing, Shyvana casts Flame Breath again immediately. Flame Breath: Shyvana breathes fire in a cone, dealing up to 700/1200/20000 magic damage. While breathing fire, Shyvana is always enraged.

**Soraka** (5-cost) - Starfall: Passive: While any ally is below 50% Health, Soraka gains an additional 15 mana per attack. Active: Soraka calls down a shower of 14 stars over the next 2 seconds. Allies are healed for 105/165/1000 each time a star hits them.

**Terra** (5-cost) - Earthquake: Terra stomps three times causing an earthquake around them, dealing (200% AD + 150%/200%/2500% Armor + 150%/200%/2500% MR) physical damage (150/200/2500% of their combined Armor and Magic Resist plus 200% of their Attack Damage.) At the end of the earthquake Terra's scales are hardened for 8 seconds, granting 130/165/4000 Armor and Magic Resist.

**Yasuo** (5-cost) - Sweeping Blade: Yasuo shields himself for 180/240/2500 for 2/2/20 second and dashes through his target, slashing nearby enemies for 150%/180%/3000% AD physical damage (150/180/3000% Attack Damage). Every third cast his slash deals 250% damage, hits a larger area, and knocks up enemies for 1.5 seconds. If Yasuo hits only the last enemy left alive, he repeatedly slashes them until they die.

**Zoe** (5-cost) - Spell Thief: Zoe nabs a spell from another dimension and casts it as if it were her own.

## Traits

### Assassin

Innate: Leap to the enemy backline when combat starts. Assassins' Abilities can critically strike and they gain bonus Critical Strike Chance and bonus Critical Strike Damage.

| Units | Effect |
|---|---|
| 2-3 | +20% Crit Chance and +10% Crit Damage |
| 4-5 | +50% Crit Chance and +25% Crit Damage |
| 6 | +75% Crit Chance and +75% Crit Damage |

### Astral

After each player combat, gain an Astral Orb. The total star level of your Astral champions increases the orb quality. Astral champions gain bonus Ability Power.

| Units | Effect |
|---|---|
| 3-4 | 5 Ability Power |
| 5-7 | 30 Ability Power |
| 8 | 75 Ability Power |

### Bard

(1) Allies that survive player combat have a 2% chance to create a Doot. Bard always creates a Doot when dancing. Each Doot you collect increases your Shop odds for higher-tier champions by 1%.

| Units | Effect |
|---|---|
| 1 | variables: {b8705dd9}=2 |

### Bruiser

Your team gains bonus maximum Health. Bruisers gain double this bonus.

| Units | Effect |
|---|---|
| 2-3 | 180 Health |
| 4-5 | 375 Health |
| 6-7 | 700 Health |
| 8 | 1500 Health |

### Cannoneer

Every 5th attack fires a cannon shot that explodes for physical damage around the target.

| Units | Effect |
|---|---|
| 2-3 | 150% Attack Damage |
| 4-5 | 300% Attack Damage |
| 6 | 500% Attack Damage |

### Cavalier

Innate: Charge quickly towards their target whenever they move. Cavaliers gain Armor and Magic Resist. At the start of combat and after each charge, gain 200% the amount for 4 seconds.

| Units | Effect |
|---|---|
| 2 | 35 Armor, 35 Magic Resist |
| 3 | 50 Armor, 50 Magic Resist |
| 4 | 65 Armor, 65 Magic Resist |
| 5 | 85 Armor, 85 Magic Resist |
| 6 | 110 Armor, 110 Magic Resist |

### Darkflight

Summon a sacrificial altar. Combat start: the unit placed on the altar is sacrificed, granting a copy of one of the items and bonus Health to each Darkflight champion. Emblems and non-craftable items are corrupted when they're copied, instead becoming Darkflight Essence.

| Units | Effect |
|---|---|
| 2-3 | 500 Health |
| 4-5 | 750 Health |
| 6-7 | 1000 Health |
| 8 | 1600 Health |

### Dragon

Innate: Require 2 team slots. Dragons provide +3 to the marked trait, and gain additional bonuses based on how many Dragons are on your team.

| Units | Effect |
|---|---|
| 1 | 500 bonus Health |
| 2 | and 20 Armor and Magic Resist |
| 3 | and 10% bonus damage |
| 4 | and +1 to team size |
| 5 | and heal 40 Health every second |
| 6 | and your Dragons ascend! |

### Dragonmancer

Use the Dragonmancer Blessing item to choose a Hero. The Hero gains massively increased Health and Ability Power, which increases by 20% per star level of your Dragonmancers.

| Units | Effect |
|---|---|
| 2-3 | +275 Health, +25 Ability Power |
| 4-5 | +575 Health, +40 Ability Power |
| 6-7 | +775 Health, +50 Ability Power |
| 8-999 | +1000 Health, +70 Ability Power |

### Evoker

Evokers gain Mana whenever an ally or enemy casts an Ability.

| Units | Effect |
|---|---|
| 2 | 5 Mana |
| 3 | 8 Mana |
| 4-999 | 10 Mana |

### Guardian

Once per combat at 50% Health, Guardians shield themselves and their closest ally for a percent of their maximum Health. Shields stack!

| Units | Effect |
|---|---|
| 2-3 | 25% of max health shield |
| 4-5 | 40% of max health shield |
| 6-7 | 80% of max health shield |
| 8-999 | 130% of max health shield |

### Guild

Grant a unique bonus to your team; Guild members gain double the amount. Increases for each Guild member in play! Sejuani: +@TFTUnitProperty.trait:TFT7_Trait_Guild_SejuaniBonus@ Health Twitch: +@TFTUnitProperty.trait:TFT7_Trait_Guild_TwitchBonus@% Attack Speed Zippy: +@TFTUnitProperty.trait:TFT7_Trait_Guild_DragonBonus@ Armor and Magic Resist Jayce: +@TFTUnitProperty.trait:TFT7_Trait_Guild_JayceBonus@ Attack Damage and Ability Power Bard: +@TFTUnitProperty.trait:TFT7_Trait_Guild_BardBonus@ Mana per attack Emblem: +@TFTUnitProperty.trait:TFT7_Trait_Guild_EmblemBonus@% Omnivamp

| Units | Effect |
|---|---|
| 1 | 100% Guild bonus |
| 2 | 110% Guild bonus |
| 3 | 120% Guild bonus |
| 4 | 130% Guild bonus |
| 5 | 140% Guild bonus |
| 6 | 150% Guild bonus |
| 7 | 160% Guild bonus |
| 8 | 175% Guild bonus |

### Jade

Summon movable Jade Statues that grow in power. Each combat, allies who start combat adjacent to a statue gain maximum Health healing every 2 seconds and bonus Attack Speed. When a statue is destroyed, it deals 33% of its Health as magic damage to nearby enemies.

| Units | Effect |
|---|---|
| 3-4 | 1 statue, +2% healing, +10% Attack Speed |
| 5-6 | 2 statues, +3% healing, +22% Attack Speed |
| 7-8 | 3 statues, +5% healing, +55% Attack Speed |
| 9 | 4 statues, +8% healing, +88% Attack Speed |

### Lagoon

Summon a Seastone which grants loot based on the number of Abilities cast by Lagoon champions over time. Lagoon champions also gain Ability Power and Attack Speed. Total casts: @TFTUnitProperty.trait:TFT7_LagoonCount@ Gold granted: @TFTUnitProperty.trait:TFT7_LagoonRewardCountGold@ Rewards granted: @TFTUnitProperty.trait:TFT7_LagoonRewardCount@@TFTUnitProperty.trait:TFT_Modes_TRA1@

| Units | Effect |
|---|---|
| 3-5 | 8 Ability Power, 8% Attack Speed |
| 6-8 | 30 Ability Power, 30% Attack Speed |
| 9-11 | 55 Ability Power, 55% Attack Speed |
| 12-999 | 200 Ability Power, 200% Attack Speed |

### Mage

Mages cast twice and have modified total Ability Power.

| Units | Effect |
|---|---|
| 3-4 | 80% Ability Power |
| 5-6 | 105% Ability Power |
| 7-8 | 135% Ability Power |
| 9 | 195% Ability Power |

### Mirage

Mirage champions gain a different Trait bonus from game to game.

| Units | Effect |
|---|---|
| 2-3 | variables: {03907b27}=40; {2c8fe3dc}=2; {34a68396}=20; {48ffe45c}=20; {67deaf34}=10; {7845f61e}=5; {7fb6d810}=50; {835c3290}=6; {9ea1c8a7}=50; {b04db727}=200; {bd0f4998}=9; {e32ac3a7}=20; {f25ff422}=10 |
| 4-5 | variables: {03907b27}=65; {2c8fe3dc}=4; {34a68396}=33; {48ffe45c}=30; {67deaf34}=10; {7845f61e}=8; {7fb6d810}=50; {835c3290}=12; {9ea1c8a7}=65; {b04db727}=300; {bd0f4998}=9; {e32ac3a7}=30; {f25ff422}=10 |
| 6-7 | variables: {03907b27}=65; {2c8fe3dc}=6; {34a68396}=50; {48ffe45c}=45; {67deaf34}=10; {7845f61e}=12; {7fb6d810}=50; {835c3290}=20; {9ea1c8a7}=80; {b04db727}=450; {bd0f4998}=9; {e32ac3a7}=40; {f25ff422}=10 |
| 8 | variables: {03907b27}=110; {2c8fe3dc}=8; {34a68396}=100; {48ffe45c}=60; {67deaf34}=10; {7845f61e}=20; {7fb6d810}=50; {835c3290}=30; {9ea1c8a7}=100; {b04db727}=600; {bd0f4998}=9; {e32ac3a7}=50; {f25ff422}=10 |

### Monolith

(3) Terra empowers 3 hexes on the battlefield. Combat start: units standing in the hex at the start of combat gain 18% damage reduction.

| Units | Effect |
|---|---|
| 3 | variables: {5064373e}=18 |

### Mystic

Your team gains Magic Resist.

| Units | Effect |
|---|---|
| 2 | 40 Magic Resist |
| 3 | 80 Magic Resist |
| 4 | 150 Magic Resist |
| 5 | 300 Magic Resist |

### Prodigy

(3) Nomsy gains a random trait each game. She summons a former Trainer to aid her in battle, who also gains this trait. Trainers' Ability Power is always equal to Nomsy's Ability Power. When Nomsy dies, Trainers gain 100% Attack Speed.

| Units | Effect |
|---|---|
| 3 | variables: AttackSpeed=1 |

### Ragewing

Innate: Convert Mana to Rage; attacks generate 15 Rage. After casting an Ability, enrage for 4 seconds: 25% Attack Speed but can't gain Rage. Gain stats when enraged:

| Units | Effect |
|---|---|
| 2-3 | 50% Attack Speed and 15% Omnivamp (healing for a percentage of damage dealt) |
| 4-5 | 100% Attack Speed and 30% Omnivamp |
| 6-7 | 150% Attack Speed and 45% Omnivamp |
| 8-999 | 325% Attack Speed and 85% Omnivamp |

### Scalescorn

Scalescorn champions take 15% reduced damage from enemies with more than 1900 health, and they also deal a percent of their damage as additional magic damage.

| Units | Effect |
|---|---|
| 2-3 | 15% magic damage |
| 4-5 | 50% magic damage |
| 6 | 90% magic damage |

### Shapeshifter

Transforming grants bonus maximum Health.

| Units | Effect |
|---|---|
| 2-3 | 60% bonus Health |
| 4-999 | 150% bonus Health |

### Shimmerscale

Grant exclusive random Shimmerscale items. You can remove Shimmerscale items by benching the holder.

| Units | Effect |
|---|---|
| 3-4 | 1 unique item |
| 5-6 | 2 unique items |
| 7-8 | 4 unique items |
| 9 | 5 unique items, which includes Crown of Champions |

### Spell Thief

(1) Zoe nabs a new Ability after each cast and at the start of every round.

| Units | Effect |
|---|---|
| 1 | active (no extra values) |

### Starcaller

(1) The first Starcaller to cast their Ability during player combat heals you for (2/3/75), depending on their star level. Excess healing disintegrates an enemy champion.

| Units | Effect |
|---|---|
| 1 | variables: {1b929fa6}=1; {31d66d45}=1; {42df8264}=1; {4979f1a4}=2; {509811bd}=2; {56034ed0}=75; {9b24c7a2}=2; {f305b0e7}=3; {fa4f482e}=20; {fd44bf8b}=30 |

### Swiftshot

Innate: gain 2 hex Attack Range. Swiftshots gain Attack Speed for each hex between themselves and their target.

| Units | Effect |
|---|---|
| 2 | 10% Attack Speed |
| 3 | 15% Attack Speed |
| 4 | 25% Attack Speed |
| 5-999 | 40% Attack Speed |

### Tempest

After 8 seconds, lightning strikes the battlefield. Enemies are stunned for 1 second and take a percent of their maximum Health as true damage. Then, Tempest champions deal increased damage.

| Units | Effect |
|---|---|
| 2-3 | 10% lightning, 10% unit damage |
| 4-5 | 20% lightning, 30% unit damage |
| 6-7 | 30% lightning, 60% unit damage |
| 8 | 45% lightning, 120% unit damage |

### Warrior

Warrior attacks have a 50% chance to increase the damage of their next attack.

| Units | Effect |
|---|---|
| 2-3 | +75% damage |
| 4-5 | +150% damage |
| 6 | +275% damage |

### Whispers

Whispers damage shrinks enemies, reducing their Armor and Magic Resist by 40% for 6 seconds. When they damage a shrunken enemy, Whispers gain stacking bonuses:

| Units | Effect |
|---|---|
| 2-3 | +1 Attack Damage and Ability Power |
| 4-5 | +3 Attack Damage and Ability Power |
| 6 | +7 Attack Damage and Ability Power |

## Items

Item and augment membership comes from the "TFTSet7_Stage2" item/augment lists in the patch 13.11 CDragon file. Values come from the patch 12.22 file when the item exists there, else from 13.11. The list is the full pool the game data allows for the set mode, so it includes items obtainable only through augments, anvils or special rounds.

### Components

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| B.F. Sword | - | 10 AD | +10 Attack Damage |
| Chain Vest | - | 20 Armor | +20 Armor |
| Giant's Belt | - | 150 HP | +150 Health |
| Needlessly Large Rod | - | 10 AP | +10 Ability Power |
| Negatron Cloak | - | 20 MR | +20 Magic Resist |
| Recurve Bow | - | 10 Attack Speed % | +10% Attack Speed |
| Sparring Gloves | - | 5 Crit % | +5 Critical Strike Chance +10 Dodge Chance |
| Spatula | - | - | It must do something... |
| Tear of the Goddess | - | 15 Mana | +15 Mana |

### Completed items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Archangel's Staff | TFT_Item_NeedlesslyLargeRod + TFT_Item_TearOfTheGoddess | 10 AP, 15 Mana | Grant 20 bonus Ability Power. Combat start: Grant 20 Ability Power every 5 seconds. |
| Banshee's Claw | TFT_Item_GiantsBelt + TFT_Item_SparringGloves | 150 HP | Combat start: Grant a shield to the holder and allies within 1 hex in the same row that blocks the first enemy Ability, up to 600 damage. ​​ [Aura item] |
| Bloodthirster | TFT_Item_BFSword + TFT_Item_NegatronCloak | 10 AD, 20 MR | Grant 25% Omnivamp. Once per combat: At 40% Health, gain a 25% maximum Health shield that lasts up to 5 seconds. |
| Blue Buff | TFT_Item_TearOfTheGoddess + TFT_Item_TearOfTheGoddess | 30 Mana | Combat Start: Grant 20 bonus starting Mana. After casting an Ability, set Mana to 20. [Unique - only 1 per champion] |
| Bramble Vest | TFT_Item_ChainVest + TFT_Item_ChainVest | 45 Armor | Grant 40 bonus Armor. Negates 75% bonus damage from incoming critical strikes. When struck by an attack, deal 75/100/150 magic damage to all nearby enemies (once every 2.5 seconds). [Direct damage item] |
| Chalice of Power | TFT_Item_NegatronCloak + TFT_Item_TearOfTheGoddess | 20 MR, 15 Mana | Combat start: Grant 30 Ability Power to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Deathblade | TFT_Item_BFSword + TFT_Item_BFSword | 20 AD | Grant 25/45/65 bonus Attack Damage. |
| Dragon's Claw | TFT_Item_NegatronCloak + TFT_Item_NegatronCloak | 40 MR | Grant 80 bonus Magic Resist. Every 2 seconds, regenerate 1.2% maximum Health for each enemy targeting the holder. If the holder is a Dragon, increase all bonuses and effects by 20%. |
| Edge of Night | TFT_Item_BFSword + TFT_Item_ChainVest | 10 AD, 20 Armor | Once per combat: At 60% Health, briefly become stealthed, becoming untargetable and shedding negative effects. Then, grant 40% bonus Attack Speed. [Unique - only 1 per champion] |
| Gargoyle Stoneplate | TFT_Item_ChainVest + TFT_Item_NegatronCloak | 20 Armor, 20 MR | Grant 16 Armor and 16 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | TFT_Item_BFSword + TFT_Item_RecurveBow | 10 AD, 10 Attack Speed % | Abilities and attacks deal 20% bonus damage. If the target has more than 1900 maximum Health, the bonus increases to 45%. |
| Guinsoo's Rageblade | TFT_Item_RecurveBow + TFT_Item_NeedlesslyLargeRod | 10 AP, 10 Attack Speed % | Attacks grant 6% bonus Attack Speed. This effect stacks. |
| Hand Of Justice | TFT_Item_TearOfTheGoddess + TFT_Item_SparringGloves | 15 Mana, 15 Crit % | Grant 2 effects: +15 Attack Damage and +15 Ability Power.15% Omnivamp. Each round, randomly double 1 of these effects. |
| Hextech Gunblade | TFT_Item_BFSword + TFT_Item_NeedlesslyLargeRod | 10 AD, 10 AP | Grant 25% Omnivamp. Damage heals the lowest percent Health ally for the same amount. |
| Infinity Edge | TFT_Item_BFSword + TFT_Item_SparringGloves | 10 AD, 15 Crit % | Grant 60% bonus Critical Strike Chance and 10% bonus Critical Strike Damage. Each point of Critical Strike Chance above 100% converts to 1% Critical Strike Damage. [Unique - only 1 per champion] |
| Ionic Spark | TFT_Item_NeedlesslyLargeRod + TFT_Item_NegatronCloak | 10 AP, 20 MR | Enemies within 2 hexes have their Magic Resist reduced by 50%. When they cast an Ability, they are zapped for magic damage equal to 185% of their maximum Mana. [Direct damage item] |
| Jeweled Gauntlet | TFT_Item_NeedlesslyLargeRod + TFT_Item_SparringGloves | 10 AP, 15 Crit % | Grant 10% Critical Strike Damage and 30 bonus Ability Power. Magic and true damage from an Ability can critically strike. |
| Last Whisper | TFT_Item_RecurveBow + TFT_Item_SparringGloves | 10 Attack Speed %, 15 Crit % | Grant 10% bonus Attack Speed. Dealing physical damage from an attack or Ability reduces the target's Armor by 50% for 5 seconds. This effect does not stack. [Unique - only 1 per champion] |
| Locket of the Iron Solari | TFT_Item_NeedlesslyLargeRod + TFT_Item_ChainVest | 10 AP, 20 Armor | Combat start: Shield the holder and allies within 2 hexes in the same row for 300/350/400 damage for 15 seconds. ​​[Aura item] |
| Morellonomicon | TFT_Item_NeedlesslyLargeRod + TFT_Item_GiantsBelt | 10 AP, 150 HP | Grant 20 bonus Ability Power. Magic or true damage from an Ability burns the target, dealing 10% of the target's maximum Health as true damage over 10 seconds, and reducing healing by 50% for the duration. [Unique - only 1 per champion] |
| Protector's Vow | TFT_Item_TearOfTheGoddess + TFT_Item_ChainVest | 20 Armor, 15 Mana | Grant 15 bonus starting Mana. Once per combat: At 50% Health, allies within 3 hexes gain a 10% maximum Health shield that lasts up to 2 seconds. Shielded allies gain 15 Armor and 15 Magic Resist for the rest of combat. ​​[Aura item] |
| Quicksilver | TFT_Item_SparringGloves + TFT_Item_NegatronCloak | 20 MR | Grant 20% bonus Attack Speed. Combat start: Grant immunity to crowd control for 15 seconds. [Unique - only 1 per champion] |
| Rabadon's Deathcap | TFT_Item_NeedlesslyLargeRod + TFT_Item_NeedlesslyLargeRod | 20 AP | Grant 60 bonus Ability Power. |
| Rapid Firecannon | TFT_Item_RecurveBow + TFT_Item_RecurveBow | 20 Attack Speed % | Grant 30% bonus Attack Speed and 1 bonus Attack Range. Attacks cannot miss. |
| Redemption | TFT_Item_TearOfTheGoddess + TFT_Item_GiantsBelt | 150 HP, 15 Mana | Heal allies within 1 hex for 12% of their missing Health every 5 seconds. They also take 25% less multi-target damage for 5 seconds (damage reduction does not stack). [Aura item] |
| Runaan's Hurricane | TFT_Item_NegatronCloak + TFT_Item_RecurveBow | 10 Attack Speed %, 20 MR | Grant 20 bonus Attack Damage. Attacks fire a bolt at a nearby enemy, dealing 70% Attack Damage as physical damage. ​​[Direct damage item] |
| Shroud of Stillness | TFT_Item_ChainVest + TFT_Item_SparringGloves | 20 Armor | Combat start: Shoot a beam that increases the maximum Mana of affected enemies by 35% until they cast. [Unique - only 1 per champion] |
| Spear of Shojin | TFT_Item_BFSword + TFT_Item_TearOfTheGoddess | 10 AD, 15 Mana | Grant 20 bonus Ability Power. Attacks restore 5 additional Mana. |
| Statikk Shiv | TFT_Item_RecurveBow + TFT_Item_TearOfTheGoddess | 10 Attack Speed %, 15 Mana | Grant 10% bonus Attack Speed. Every 3rd attack unleashes chain lightning that bounces to 4 enemies, dealing 50 magic damage and reducing their Magic Resist by 50% for 5 seconds. [Direct damage item] |
| Sunfire Cape | TFT_Item_ChainVest + TFT_Item_GiantsBelt | 20 Armor, 150 HP | Grant 150 bonus Health. Every 2 seconds, an enemy within 2 hexes is burned for 10% of their maximum Health as true damage over 10 seconds, and reducing healing by 50% for the duration. [Unique - only 1 per champion] |
| Thief's Gloves | TFT_Item_SparringGloves + TFT_Item_SparringGloves | 15 Crit % | Each round: Equip 2 random items. [Consumes 3 item slots.] |
| Titan's Resolve | TFT_Item_ChainVest + TFT_Item_RecurveBow | 10 Attack Speed %, 20 Armor | Grant 2 Attack Damage and 2 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, grant 25 Armor and 25 Magic Resist. |
| Warmog's Armor | TFT_Item_GiantsBelt + TFT_Item_GiantsBelt | 300 HP | Grant 700 bonus Health. |
| Zeke's Herald | TFT_Item_BFSword + TFT_Item_GiantsBelt | 10 AD, 150 HP | Combat start: Grant 30% Attack Speed to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Zephyr | TFT_Item_NegatronCloak + TFT_Item_GiantsBelt | 20 MR, 150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| Zz'Rot Portal | TFT_Item_RecurveBow + TFT_Item_GiantsBelt | 10 Attack Speed %, 150 HP | Combat start: Taunt all enemies that are in range to attack the holder. On death, a Voidspawn arises, taunting nearby enemies. Voidspawns that arise from summoned units are 25% effective. |

### Spatula items and emblems

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Assassin Emblem | - | 150 HP | The holder gains the Assassin trait. [Unique - only 1 per champion |
| Astral Emblem | - | 150 HP | The holder gains the Astral trait. [Unique - only 1 per champion |
| Bruiser Emblem | - | 150 HP | The holder gains the Bruiser trait. [Unique - only 1 per champion] |
| Cannoneer Emblem | - | 150 HP | The holder gains the Cannoneer trait. [Unique - only 1 per champion |
| Cavalier Emblem | TFT_Item_Spatula + TFT_Item_ChainVest | 20 Armor | The holder gains the Cavalier trait. [Unique - only 1 per champion |
| Darkflight Emblem | - | 150 HP | The holder gains the Darkflight trait. |
| Dragonmancer Emblem | TFT_Item_Spatula + TFT_Item_NeedlesslyLargeRod | 10 AP | The holder gains the Dragonmancer trait. |
| Evoker Emblem | - | 150 HP | The holder gains the Evoker trait. [Unique - only 1 per champion |
| Guardian Emblem | TFT_Item_Spatula + TFT_Item_GiantsBelt | 150 HP | The holder gains the Guardian trait. [Unique - only 1 per champion |
| Guild Emblem | - | 150 HP | The holder gains the Guild trait. Guild Bonus: 3% Omnivamp [Unique - only 1 per champion |
| Jade Emblem | - | 150 HP | The holder gains the Jade trait. [Unique - only 1 per champion |
| Lagoon Emblem | TFT_Item_SparringGloves + TFT_Item_Spatula | 10 Crit % | The holder gains the Lagoon trait. [Unique - only 1 per champion] |
| Legend Emblem | - | 150 HP | The holder gains the Legend trait. |
| Mage Emblem | TFT_Item_Spatula + TFT_Item_TearOfTheGoddess | 15 Mana | The holder gains the Mage trait. [Unique - only 1 per champion |
| Mirage Emblem | TFT_Item_NegatronCloak + TFT_Item_Spatula | 20 MR | The holder gains the Mirage trait. [Unique - only 1 per champion] |
| Mystic Emblem | - | 150 HP | The holder gains the Mystic trait. [Unique - only 1 per champion |
| Revel Emblem | - | 150 HP | The holder gains the Revel trait. [Unique - only 1 per champion |
| Scalescorn Emblem | - | 150 HP | The holder gains the Scalescorn trait. [Unique - only 1 per champion |
| Swiftshot Emblem | TFT_Item_Spatula + TFT_Item_RecurveBow | 10 Attack Speed % | The holder gains the Swiftshot trait. [Unique - only 1 per champion |
| Tactician's Crown | TFT_Item_Spatula + TFT_Item_Spatula | - | Your team gains +1 maximum team size. |
| Tempest Emblem | - | 150 HP | The holder gains the Tempest trait. [Unique - only 1 per champion |
| Warrior Emblem | - | 150 HP | The holder gains the Warrior trait. [Unique - only 1 per champion |

### Ornn artifacts

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Visage | - | 25 MR, 150 HP | Regenerate 5% maximum Health each second. |
| Death's Defiance | - | 10 AD, 20 Attack Speed %, 20 Armor | Grant 25% Omnivamp. 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] |
| Eternal Winter | - | 25 Armor, 150 HP | Enemies who damage the holder have their Attack Speed slowed by 20% for 2 seconds. After being slowed this way 7 times, the attacker is Frozen for the duration instead. An enemy can only be Frozen once every 15 seconds. [Unique - only 1 per champion] |
| Gold Collector | - | 20 AD, 20 Crit % | Attacks and Abilities execute enemies below 10% of their maximum Health. Executions have a 40% chance to generate 1 gold. [Unique - only 1 per champion] |
| Infinity Force | - | 25 AD, 25 AP, 25 Attack Speed %, 25 Armor, 25 MR, 25 HP | - |
| Manazane | - | 15 AD, 15 Attack Speed %, 30 Mana | Once per combat: After casting a spell, restore 150 Mana over 5 seconds. [Unique - only 1 per champion] |
| Obsidian Cleaver | - | 20 AD, 150 HP | Attacks and Abilities shred enemy Armor and Magic Resistance by 60% for 5 seconds. [Unique - only 1 per champion] |
| Randuin's Sanctum | - | null Armor, null MR, 150 HP | Combat start: Grants 40 Armor and 40 Magic Resistance to all adjacent allies, including the holder. [Aura item] |
| Rocket-Propelled Fist | - | 25 Armor, 150 HP, 15 Mana | Combat start: The farthest enemy is pulled into melee range and stunned for 1.5 seconds. Allies within range will prioritize attacking that enemy. [Unique - only 1 per champion] |
| Zhonya's Paradox | - | 40 AP, 40 Armor, 40 MR | Once per combat: At 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Absolution | - | 150 HP, 15 Mana | Heal allies within 2 hexes for 22% of their missing Health every 5 seconds. They also take 25% less multi-target damage for 5 seconds (damage reduction does not stack). Radiant Bonus: 150 Health. ​​[Aura item] |
| Banshee's Silence | - | 150 HP | Combat start: Grant a shield to your team that blocks the first enemy Ability, up to 600 damage. Radiant Bonus: +100 Health for all allies. ​​[Aura item] |
| Blessed Bloodthirster | - | 10 AD, 20 MR | Grant 35% Omnivamp. Once per combat: At 40% Health, gain a 35% maximum Health shield that lasts up to 5 seconds. Radiant Bonus: 20 Attack Damage. |
| Blue Blessing | - | 10 AP, 30 Mana | Grant 20 bonus starting Mana. After casting an Ability, set Mana to 30. Radiant Bonus: 10 Ability Power. [Unique - only 1 per champion] |
| Brink of Dawn | - | 10 AD, 20 Armor | Once per combat: At 60% Health and again at 30% Health, briefly become stealthed, becoming untargetable and shedding negative effects. Then, grant 40% bonus Attack Speed. This effect stacks. [Unique - only 1 per champion] |
| Bulwark's Oath | - | 20 Armor, 15 Mana | Grant 30 bonus starting Mana. Once per combat: At 50% Health, allies within 8 hexes gain a 20% maximum Health shield that lasts up to 3 seconds. Shielded allies gain 30 Armor and 30 Magic Resist for the rest of combat. ​​[Aura item] |
| Chalice of Charity | - | 20 MR, 15 Mana | Combat start: Grant 45 Ability Power and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Covalent Spark | - | 10 AP, 20 MR | Enemies within 3 hexes have their Magic Resist reduced by 50%. When they cast an Ability, they are zapped for magic damage equal to 250% of their maximum Mana. Radiant Bonus: Regenerate 1% max Health per second. [Direct damage item] |
| Demonslayer | - | 10 AD, 10 Attack Speed % | Abilities and attacks deal 40% bonus damage. If the target has more than 1900 maximum Health, the bonus increases to 70%. |
| Dragon's Will | - | 40 MR | Grant 140 bonus Magic Resist. Every 1.5 seconds, regenerate 2.5% maximum Health for each enemy targeting the holder. If the holder is a Dragon, increase all bonuses and effects by 20%. |
| Dvarapala Stoneplate | - | 20 Armor, 20 MR | Grants 30 Armor and 30 Magic Resist for each enemy targeting the holder. Radiant Bonus: Regenerate 2% max Health each second. |
| Eternal Whisper | - | 10 Attack Speed %, 15 Crit % | Grant 35% bonus Attack Speed. Dealing physical damage reduces the target's Armor by 50% for the rest of combat. This effect does not stack. Radiant Bonus: 35% Critical Strike Chance. [Unique - only one per champion] |
| Fist of Fairness | - | 15 Mana, 15 Crit % | Grant 2 effects: 40 Attack Damage and 40 Ability Power.40% Omnivamp. |
| Glamorous Gauntlet | - | 10 AP, 15 Crit % | Grant 40% Critical Strike Damage and 50 bonus Ability Power. Magic and true damage from an Ability can critically strike. |
| Guinsoo's Reckoning | - | 10 AP, 10 Attack Speed % | Attacks grant +10% bonus Attack Speed. This effect stacks. Radiant Bonus: 15% Attack Speed. |
| Hextech Lifeblade | - | 10 AD, 10 AP | Grant 35% Omnivamp. Damage dealt heals the lowest percent Health ally for the same amount. Radiant Bonus: +30 Ability Power. |
| Locket of Targon Prime | - | 10 AP, 20 Armor | Combat start: Shields the holder and allies within 3 hexes in the same row for 400/500/600 damage for 60 seconds. Radiant Bonus: +200 Health for all allies. ​​[Aura item] |
| Luminous Deathblade | - | 20 AD | Grant 40/70/100 bonus Attack Damage. |
| Mistral | - | 20 MR, 150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 8 seconds. Radiant Bonus: 15% Attack Speed for all allies. [Ignores crowd control immunity.] [Unique - only 1 per champion] |
| More More-ellonomicon | - | 10 AP, 150 HP | Grant 35 bonus Ability Power. Magic or true damage from an Ability burns the target, dealing 60% of the target's maximum Health as true damage over 30 seconds, and reducing healing by 50% for the duration. [Unique - only 1 per champion] |
| Quickestsilver | - | 20 MR | Grant 60% bonus Attack Speed. Combat start: Grant immunity to crowd control for 30 seconds. [Unique - only 1 per champion] |
| Rabadon's Ascended Deathcap | - | 20 AP | Grant 105 bonus Ability Power. |
| Rapid Lightcannon | - | 20 Attack Speed % | Grant 55% bonus Attack Speed and 2 Attack Range. Attacks cannot miss. |
| Rascal's Gloves | - | 15 Crit % | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] |
| Rosethorn Vest | - | 40 Armor | Grant 120 bonus Armor. Negates all bonus damage from incoming critical strikes. On being hit by an attack, deal 125/175/225 magic damage to all nearby enemies (once every 1.5 seconds). [Direct damage item] |
| Runaan's Tempest | - | 20 AD, 10 Attack Speed %, 20 MR | Grant 20 Attack Damage. Attacks fire a bolt at a nearby enemy, dealing 110% Attack Damage as physical damage. Radiant Bonus: 30% Attack Speed. ​​[Direct damage item] |
| Shroud of Reverence | - | 20 Armor | Combat start: Shoot a wider beam that increases the maxiumum Mana of affected enemies by 50% until they cast. Radiant Bonus: 15 starting Mana for all allies. [Unique - only one per champion] |
| Spear of Hirana | - | 10 AD, 15 Mana | Grant 40 bonus Ability Power. Attacks restore 10 additional Mana. |
| Statikk's Favor | - | 10 Attack Speed %, 15 Mana | Grants 30% bonus Attack Speed. Every 3rd attack unleashes chain lightning that bounces to 8 enemies, dealing 75 magic damage and reducing their Magic Resist by 50% for 5 seconds. [Direct damage item] |
| Sunlight Cape | - | 20 Armor, 150 HP | Grant 200 bonus Health. Every 1.5 seconds, an enemy within 3 hexes is burned for 60% of their maximum Health as true damage over 30 seconds, and reducing healing by 50% for the duration. [Unique - only 1 per champion] |
| Titan's Vow | - | 10 Attack Speed %, 20 Armor | Grant 3 Attack Damage and 3 Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, grant 50 Armor and 50 Magic Resist. |
| Urf-Angel's Staff | - | 10 AP, 15 Mana | Grant 30 bonus Ability Power. Combat start: Grant 30 Ability Power every 4 seconds. |
| Warmog's Pride | - | 300 HP | Grant 1300 bonus Health. Radiant Bonus: Regenerate 1% max Health per second. |
| Zeke's Harmony | - | 10 AD, 150 HP | Combat start: Grant 50% Attack Speed and 10% Omnivamp to the holder and allies within 1 hex in the same row. ​​[Aura item] |
| Zenith Edge | - | 10 AD, 15 Crit % | Grants 60% bonus Critical Strike Chance and 50% Critical Strike Damage. Each point of Critical Strike Chance above 100% converts to 1% Critical Strike Damage. [Unique - only 1 per champion] |
| Zz'Rots Invitation | - | 10 Attack Speed %, 150 HP | Combat start: A Radiant Voidmother with bonus Health and Attack Damage arises and taunts all enemies that are in range to attack her. Radiant Voidmothers that arise from summoned units are 25% effective. |

### Shimmerscale items

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Crown Of Champions | - | 100 HP | Every 5 seconds, the next attack deals 9001x the amount of gold in your bank in true damage. |
| Determined Investor | - | 150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and 15 gold. |
| Determined Investor | - | 150 HP | After dying during combat 4 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and 15 gold. |
| Diamond Hands | - | 20 AP, 200 HP | Once per combat: At 66% Health and 33% Health, become invulnerable for 1 seconds and grant 1 gold. |
| Diamond Hands | - | 20 AP, 200 HP | Once per combat: At 66% Health and 33% Health, become invulnerable for 1 seconds and grant 2 gold. |
| Draven's Axe | - | 5 AD, 5 Attack Speed % | Gain 1 Attack Damage per gold in your bank (up to 80 gold). Attacks grant 1 stack, up to 100 times. At full stacks, grant 5 gold and 1 item component(s). |
| Draven's Axe | - | 5 AD, 5 Attack Speed % | Gain 3 Attack Damage per gold in your bank (up to 15 gold). Attacks grant 1 stack, up to 50 times. At full stacks, grant 5 gold and 1 item component(s). |
| Gambler's Blade | - | 10 AP, 10 Attack Speed % | Grant 1% bonus Attack Speed per gold in your bank (up to 80 gold). Each attack has a 7% chance to drop 1 gold. |
| Gambler's Blade | - | 10 AP, 10 Attack Speed % | Grant 3% bonus Attack Speed per gold in your bank (up to 15 gold). Each attack has a 10% chance to drop 1 gold. |
| Goldmancer's Staff | - | 20 AP, 20 Mana | Grant 1 Ability Power per gold in your bank (up to 80 gold) and a 40% chance to drop 2 gold on enemy kill. |
| Goldmancer's Staff | - | 20 AP, 20 Mana | Grant 4 Ability Power per gold in your bank (up to 15 gold) and a 50% chance to drop 2 gold on enemy kill. |
| Mogul's Mail | - | 350 HP | Grants 1 Armor, 1 Magic Resist, and 8 Health when taking damage, stacking up to 40 times. At full stacks, grant 2 gold. |
| Mogul's Mail | - | 350 HP | Grants 1 Armor, 1 Magic Resist, and 8 Health when taking damage, stacking up to 40 times. At full stacks, grant 3 gold. |
| Needlessly Big Gem | - | 500 HP | Once per combat: If the holder is alive after 15 seconds of combat, grant 1 gold per 2 living allies, and each living ally deals 1% more damage per gold in your bank (up to 80 gold). |
| Needlessly Big Gem | - | 500 HP | If the holder is alive after 15 seconds of combat, your units deal 25% + 3% more damage per gold you have (maximum 15 gold). For every 2 units alive when this happens, gain 1 gold. |
| Shimmerscale Emblem | TFT_Item_Spatula + TFT_Item_BFSword | 10 AD | The holder gains the Shimmerscale trait. [Unique - only 1 per champion |

### Consumables and other

| Item | Recipe | Stats | Effect |
|---|---|---|---|
| Champion Duplicator | - | - | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Darkflight Essence | - | 13 AD, 13 AP, 13 Armor, 13 MR, 130 HP | When the holder dies, grant this item's stats to each Darkflight champion. [Aquired from copying a non-Craftable item via the Darkflight Trait]. |
| Delicious Snax | - | - | Nomsy needs to eat this many more Snax to level up! [This does not count as an item.] |
| Dragon Blessing | - | - | Use on a Dragonmancer to make them your Dragonmancer Hero. [Cannot be used during combat.] |
| Lesser Champion Duplicator | - | - | Use on a champion of Tier 3 or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Loaded Dice | - | - | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover | - | - | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Reforger | - | - | Use on a champion to unequip all items and randomly transform them into different new ones. [Same quality: Reforging will grant a new item of a similar type and quality. Cannot be used on champions in combat, or on Tactician's Crown. Consumable - This item disappears when used.] |
| Target Dummy | - | - | Use on a champion to spawn a movable target dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |

## Augments

Tier is inferred from the augment icon art suffix (I/II/III or 1/2/3) or, failing that, the apiName; "Hero augment" marks augments whose icon is a champion portrait. Tiers changed between patches, so treat them as approximate. Silver = I, Gold = II, Prismatic = III.

| Augment | Tier | Effect |
|---|---|---|
| Ancient Archives I | Gold (II) | Gain 1 Tome of Traits. |
| Ascension | Gold (II) | After 15 seconds of combat, your units deal 50% more damage. |
| Axiom Arc II | Gold (II) | Your units gain 40 Mana on kill. |
| Base Camp | Gold (II) | Scalescorns gain 7% damage each time they start combat in the same hex, stacking up to 4 times. Gain a Lillia. |
| Battlemage II | Gold (II) | Your units that start combat in the front 2 rows gain 25 Armor and 25 Ability Power. |
| Beast's Den | Gold (II) | Units that start combat next to Shapeshifters gain 25% Attack Speed and Movement Speed. Shapeshifters always gain this bonus. Gain a Gnar. |
| Best Friends II | Gold (II) | Units isolated in pairs gain 15% Attack Speed and 25 Armor at the start of combat. |
| Better Together | Gold (II) | Aura items that buff your team have their effects increased by 33%. Gain a Locket of the Iron Solari. |
| Big Friend II | Gold (II) | Your units that start combat next to a unit with over 1800 Health take 18% less damage for the rest of combat. |
| Bruiser Crest | Gold (II) | Gain a Bruiser Emblem and a Jax. |
| Built Different II | Gold (II) | Your units with no Traits active gain 300-525 Health and 40-85% Attack Speed (based on current Stage). |
| Calculated Loss | Gold (II) | After losing your combat, gain 2 gold and a free Shop refresh. |
| Cannoneer Crest | Gold (II) | Gain a Cannoneer Emblem and an Aphelios. |
| Cavalier Crest | Gold (II) | Gain a Cavalier Emblem and a Sejuani. |
| Celestial Blessing II | Gold (II) | Your units heal for 20% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 350 Health. |
| Clear Mind | Gold (II) | If you have no champions on your bench at the end of a round, gain 3 experience points. |
| Cluttered Mind | Gold (II) | Gain 2 Tier 1 Champions. If your bench is full at the end of a round, gain 3 experience points. |
| Combat Training | Gold (II) | Your champions permanently gain 1 Attack Damage every time they kill an enemy. Champions start with 8 bonus Attack Damage. |
| Component Grab Bag | Gold (II) | Gain 3 random item components. |
| Cybernetic Implants II | Gold (II) | Your champions holding an item gain 150 Health and 15 Attack Damage. |
| Cybernetic Shell II | Gold (II) | Your champions holding an item gain 150 Health and 30 Armor. |
| Cybernetic Uplink II | Gold (II) | Your champions holding an item gain 150 Health and restore 3 Mana per second. |
| Darkflight Crest | Gold (II) | Gain a Darkflight Emblem and a Rell. |
| Devastating Charge | Gold (II) | Cavalier's first attack after charging deals 80 magic damage, increased by 1% for each Armor and Magic Resist they have. Gain a Rell. |
| Double Trouble II | Gold (II) | If you have exactly 2 copies of a champion on your board, they both gain 30 Attack Damage, Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Dragon Imperialist | Gold (II) | If you only have 1 Dragon, Dragons execute enemies they damage who are below 18% Health. Gain a Nomsy. |
| Dragonmancer Crest | Gold (II) | Gain a Dragonmancer Emblem and a Kai'Sa. |
| Electrocharge II | Gold (II) | When your units receive critical strikes, they deal 85-145 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Essence Theft | Gold (II) | Mage's attacks drain 8 Mana from their target and gain 4 Mana. Gain a Lux. |
| Eternal Protection | Gold (II) | When one of your Jade champions would die, the nearest Jade Statue protects them, sacrificing 70% of its max Health and transferring 150% of the sacrificed Health to the champion. Gain a Gnar. |
| Evoker Crest | Gold (II) | Gain an Evoker Emblem and a Zyra. |
| Exiles II | Gold (II) | Your units that start combat with no adjacent allies gain a 35% maximum Health shield for 10 seconds. |
| Eye of the Storm | Gold (II) | The unit in the center of the board gains 45 Ability Power, increased to 135 after Tempest's lightning strikes. Gain a Qiyana. |
| Featherweights II | Gold (II) | Your Tier 1 and 2 champions gain 35% Attack Speed and Move Speed. |
| First Aid Kit II | Gold (II) | All healing and shielding on your units is increased by 35%. |
| Gadget Expert | Gold (II) | Direct damage items deal 25% more damage as true damage. Gain a Statikk Shiv. |
| Gear Upgrades | Gold (II) | Guild champions holding an item gain 35 Armor and an additional 250% of their base Guild Bonus. Gain a Twitch. |
| Guardian Crest | Gold (II) | Gain a Guardian Emblem and a Zac. |
| Guild Crest | Gold (II) | Gain a Guild Emblem and a Twitch. |
| Hallucinate | Gold (II) | Mirage champions take 90% less damage for the first 6 seconds of combat. Gain a Yone. |
| Hero-In-Training | Gold (II) | The Dragonmancer nearest to the Dragonmancer Hero gains 66% of the Dragonmancer Trait's bonuses. Gain a Sett. |
| Heroic Presence | Gold (II) | Every 2 seconds, Guardians taunt all enemies within range. Enemies that attack a Guardian's shield take magic damage equal to 7% of the shielded unit's max Health (up to once per second). Gain a Braum. |
| High Tide | Gold (II) | Until they cast 2 times, Lagoon champions attack 30% slower but gain 12 Mana per second. Gain a Kai'sa. |
| Hot Shot | Gold (II) | Cannoneer's cannon shots burn their targets, dealing 6% of the target's maximum Health as true damage over 2 seconds, and reducing healing by 50% for the duration of the burn. Gain an Aphelios. |
| Hustler | Gold (II) | If you have less than 20 gold at the end of a round, gain 2 gold. |
| Jade Heart | Gold (II) | Your team counts as having 1 additional Jade. Gain a Gnar. |
| Jeweled Lotus | Gold (II) | Magic and true damage from your units' Abilities can critically strike. Your units gain 25% Critical Strike Chance. |
| Knife's Edge II | Gold (II) | Your units that start combat in the front 2 rows gain 25 Attack Damage. |
| Lagoon Crest | Gold (II) | Gain a Lagoon Emblem and a Zac. |
| Last Stand | Gold (II) | The first time you would be eliminated or reduced to 1 Health, instead remain alive. After this effect triggers, your units permanently gain 200 Health, 20 Armor and Magic Resist, and 20% Omnivamp. |
| Loot Master | Gold (II) | Gain a random component every 4 player combats when you have at least Guild (2) active. Gain a Twitch. |
| Luden's Echo II | Gold (II) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 55-160 (based on current Stage) magic damage. |
| Mage Crest | Gold (II) | Gain a Mage Emblem and a Vladimir. |
| Metabolic Accelerator | Gold (II) | Your Tactician moves faster and heals 2 Health after a PvP round. |
| Mirage Crest | Gold (II) | Gain a Mirage Emblem and a Nunu. |
| Oasis | Gold (II) | Every 6 seconds, Lagoon champions remove all negative effects on themselves and heal for 18% of their missing Health. Gain a Zac. |
| Pay to Win | Gold (II) | If you purchase a Shop Refresh during the Planning Phase, champions holding a Shimmerscale Item and all Shimmerscale champions gain 35% damage when the next combat begins. Gain a Jax. |
| Penitence | Gold (II) | Jade Statue explosions deal 100% more damage. Enemies are disarmed for 2.5 seconds the first time they attack a Jade Statue or are damaged by a Jade Statue's explosion. Gain a Gnar. |
| Personal Training | Gold (II) | At the end of combat, champions that started combat adjacent to Bruisers permanently gain 40 Health, increased by 20 if they survived. Gain a Jax. |
| Portable Forge | Gold (II) | Open an Armory and choose 1 of 3 unique Artifacts crafted by Ornn. |
| Preparation II | Gold (II) | Champions on your bench permanently gain 35 Health, 4 Attack Damage, and 4 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Press the Attack | Gold (II) | Your Swiftshots' attacks apply a stack on their target for 5 seconds. Every third stack deals true damage equal to 4% of the target's maximum Health. Gain a Twitch. |
| Protectors of the Cosmos | Gold (II) | Combat start: Your team gains a 35 Health shield for every item component equipped on an Astral champion, including those that are part of a full item. Gain a Lux. |
| Rich Get Richer | Gold (II) | Gain 10 gold. Your maximum interest is increased to 7. |
| Rich Get Richer+ | Gold (II) | Gain 18 gold. Your maximum interest is increased to 7. |
| Ricochet | Gold (II) | Cannoneer's cannon shots bounce once, dealing 40% less damage. Gain an Aphelios. |
| Salvage Bin | Gold (II) | Gain a random completed item. Selling champions breaks apart their full items into components (excluding Tactician's Crown). |
| Scalescorn Crest | Gold (II) | Gain a Scalescorn Emblem and a Braum. |
| Scoped Weapons I | Gold (II) | Your units that start combat in the back 2 rows gain +1 Attack Range and 15% Attack Speed. |
| Scorch | Gold (II) | Every 5th instance of Ragewing champion's damage deals 20% more damage and is converted to true damage. Gain a Senna. |
| Second Wind II | Gold (II) | After 10 seconds of combat, your units heal 60% of their missing Health. |
| Shimmerscale Heart | Gold (II) | Your team counts as having 1 additional Shimmerscale. Gain a Jax. |
| Stand United II | Gold (II) | Your units gain 2 Attack Damage and 3 Ability Power per Trait active across your army. |
| Sunfire Board | Gold (II) | At the start of combat, burn all enemies for 10% of their maximum Health over 20 seconds and reduce healing received by 50%. |
| Swiftshot Crest | Gold (II) | Gain a Swiftshot Emblem and a Twitch. |
| Tantrum | Gold (II) | After casting their first Ability, Ragewings restore 100% of their maximum Rage. Gain a Senna. |
| Tempest Crest | Gold (II) | Gain a Tempest Emblem and a Qiyana. |
| Terrify | Gold (II) | Your units take 15% less damage from enemies shrunken by Whisper champions. Gain a Sylas. |
| Three's Company | Gold (II) | Gain 3 random Tier 3 champions. |
| Thrill of the Hunt II | Gold (II) | Your units heal 700 Health on kill. |
| Tiamat | Gold (II) | Warrior's enhanced attacks deal 50% of their damage to enemies in a 1-hex area around their target. Gain a Yone. |
| Titanic Strength | Gold (II) | Bruisers gain 1.5% of their Health as Attack Damage. Gain a Jax. |
| Trade Sector | Gold (II) | Gain a free Shop refresh each round. |
| Trade Sector+ | Gold (II) | Gain a free Shop refresh every round. Gain 8 gold now. |
| Tri Force II | Gold (II) | Your Tier 3 champions gain 233 Health, 23 starting Mana, and 23% Attack Speed. |
| True Twos | Gold (II) | Gain a random 2-star Tier 1 champion and a random 2-star Tier 2 champion. |
| Warrior Crest | Gold (II) | Gain a Warrior Emblem and a Yone. |
| Age of Dragons | Prismatic (III) | If your team has 3+ Dragons, every 4th attack from a Dragon fires a blast that deals magic damage equal to 30% of the target's maximum Health. Gain a Nomsy, Zippy, and a Tier 4 Dragon. |
| Ancient Archives II | Prismatic (III) | Gain 2 Tome of Traits. |
| Assassin Crown | Prismatic (III) | Gain an Assassin Emblem, a Hand of Justice, and a Rengar. |
| Band of Thieves II | Prismatic (III) | Gain 2 Thief's Gloves. |
| Battlemage III | Prismatic (III) | Your units that start combat in the front 2 rows gain 35 Armor and 35 Ability Power. |
| Best Friends III | Prismatic (III) | Units isolated in pairs gain 20% Attack Speed and 35 Armor at the start of combat. |
| Binary Airdrop | Prismatic (III) | Your champions equipped with 2 items temporarily gain a random completed item at the start of combat. Gain 2 random item components. |
| Birthday Present | Prismatic (III) | Gain a 2-star champion and 1 gold every time you level up. The champion's tier is your level minus 4 (min: Tier 1). |
| Blue Battery II | Prismatic (III) | Your units gain 20 Ability Power. After casting their Ability, set the caster's Mana to 20. |
| Bruiser Crown | Prismatic (III) | Gain a Bruiser Emblem, a Redemption, and a Sylas. |
| Built Different III | Prismatic (III) | Your units with no Traits active gain 350-650 Health and 50-95% Attack Speed (based on current Stage). |
| Cannoneer Crown | Prismatic (III) | Gain a Cannoneer Emblem, a Giant Slayer, and a Zeri. |
| Cavalier Crown | Prismatic (III) | Gain a Cavalier Emblem, a Redemption, and a Nunu. |
| Cavalier Unity | Prismatic (III) | The Cavalier trait grants its bonuses to all of your champions. This does not increase your number of Cavaliers. Gain a Rell. |
| Celestial Blessing III | Prismatic (III) | Your units heal for 35% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 450 Health. |
| Cruel Pact | Prismatic (III) | Buying XP costs 6 Health instead of gold. Heal 3 Health each round. |
| Cursed Crown | Prismatic (III) | Gain +2 maximum team size, but take 100% increased player damage. |
| Cybernetic Implants III | Prismatic (III) | Your champions holding an item gain 250 Health and 25 Attack Damage. |
| Cybernetic Shell III | Prismatic (III) | Your champions holding an item gain 250 Health and 40 Armor. |
| Cybernetic Uplink III | Prismatic (III) | Your champions holding an item gain 250 Health and restore 4 Mana per second. |
| Darkflight Crown | Prismatic (III) | Gain a Darkflight Emblem, a Titan's Resolve, and a Rengar. |
| Double Trouble III | Prismatic (III) | If you have exactly 2 copies of a champion on your board, they both gain 40 Attack Damage, Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Dragon Soul | Prismatic (III) | Your team counts as having 1 additional Dragon. Gain a Zippy and a Gargoyle Stoneplate. |
| Dragonmancer Crown | Prismatic (III) | Gain a Dragonmancer Emblem, a Hand of Justice, and a Lee Sin. |
| Electrocharge III | Prismatic (III) | When your units receive critical strikes, they deal 115-235 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Evoker Crown | Prismatic (III) | Gain an Evoker Emblem, a Spear of Shojin, and a Seraphine. |
| Exiles III | Prismatic (III) | Your units that start combat with no adjacent allies gain a 50% maximum Health shield for 10 seconds. |
| Featherweights III | Prismatic (III) | Your Tier 1 and 2 champions gain 60% Attack Speed and Move Speed. |
| Future Sight II | Prismatic (III) | Know who you will fight next. Gain a Radiant Zephyr. |
| Golden Ticket | Prismatic (III) | Each time your Shop is refreshed, you have a 50% chance to gain a free refresh. |
| Guardian Crown | Prismatic (III) | Gain a Guardian Emblem, a Sunfire Cape, and a Rakan. |
| Guild Crown | Prismatic (III) | Gain a Guild Emblem, a Protector's Vow, and a Twitch. |
| High Roller | Prismatic (III) | Gain 3 Loaded Dice and 8 gold. |
| Item Grab Bag II | Prismatic (III) | Gain 2 random completed items and 2 Reforgers. |
| Jade Soul | Prismatic (III) | Your team counts as having 1 additional Jade. Gain a Titan's Resolve and a Gnar. |
| Knife's Edge III | Prismatic (III) | Your units that start combat in the front 2 rows gain 35 Attack Damage. |
| Lagoon Crown | Prismatic (III) | Gain a Lagoon Emblem, a Spear of Shojin, and a Zeri. |
| Living Forge | Prismatic (III) | Gain a random Ornn artifact now and after every 10 player combats. |
| Lucky Gloves | Prismatic (III) | Thief's Gloves will always give your champions ideal items. Gain a Thief's Gloves. |
| Luden's Echo III | Prismatic (III) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 90-240 (based on current Stage) magic damage. |
| Mage Crown | Prismatic (III) | Gain a Mage Emblem, a Spear of Shojin, and a Lux. |
| March of Progress | Prismatic (III) | Gain 5 bonus experience points per round. You can no longer use gold to level up. |
| Mirage Crown | Prismatic (III) | Gain a Mirage Emblem, a Hand of Justice, and a Nunu. |
| Mystic Soul | Prismatic (III) | Your team counts as having 1 additional Mystic. Gain a Chalice of Power and a Rakan. |
| New Recruit | Prismatic (III) | Gain +1 maximum team size. |
| Part-Time Assassin | Prismatic (III) | The Assassin Trait grants its bonuses to your champions that start combat in the middle 2 rows. This does not increase the number of Assassins. Gain a Qiyana. |
| Preparation III | Prismatic (III) | Champions on your bench permanently gain 50 Health, 7 Attack Damage, and 7 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Radiant Relics | Prismatic (III) | Open an Armory and choose 1 of 5 unique Radiant items. |
| Ragewing Soul | Prismatic (III) | Your team counts as having 1 additional Ragewing. Gain a Guinsoo's Rageblade and a Rakan. |
| Scalescorn Crown | Prismatic (III) | Gain a Scalescorn Emblem, a Hand of Justice, and a Diana. |
| Scoped Weapons II | Prismatic (III) | Your units that start combat in the back 2 rows gain infinite Attack Range. |
| Shapeshifter Soul | Prismatic (III) | Your team counts as having 1 additional Shapeshifter. Gain a Sunfire Cape and an Gnar. |
| Shimmerscale Soul | Prismatic (III) | Your team counts as having 1 additional Shimmerscale. Gain a Titan's Resolve and a Volibear. |
| Stand United III | Prismatic (III) | Your units gain 3 Attack Damage and 4 Ability Power per Trait active across your army. |
| Swiftshot Crown | Prismatic (III) | Gain a Swiftshot Emblem, a Guinsoo's Rageblade, and a Varus. |
| Tempest Crown | Prismatic (III) | Gain a Tempest Emblem, a Hand of Justice, and a Lee Sin. |
| The Golden Egg | Prismatic (III) | Gain a massive golden egg that hatches in 10 turns. Victorious player combats accelerate the hatch timer by an additional turn. |
| Think Fast | Prismatic (III) | Shop refreshes are free until the end of this round. Traits and other augments do not benefit from these free shops. Gain 8 gold. |
| Tri Force III | Prismatic (III) | Your Tier 3 champions gain 333 Health, 33 starting Mana, and 33% Attack Speed. |
| Urf's Grab Bag II | Prismatic (III) | Gain 1 Spatula and 3 random item component(s). |
| Verdant Veil | Prismatic (III) | Your units are immune to crowd control effects for the first 20 seconds of combat. |
| Warrior Crown | Prismatic (III) | Gain a Warrior Emblem, a Giant Slayer, and a Yone. |
| Windfall | Prismatic (III) | Gain 25 gold. |
| Windfall+ | Prismatic (III) | Gain 35 gold. |
| Windfall++ | Prismatic (III) | Gain 45 gold. |
| Wise Spending | Prismatic (III) | When you refresh your Shop, gain 2 experience point(s). |
| Woodland Charm | Prismatic (III) | Your highest Health champion is cloned. You cannot equip items on the clone. |
| AFK | Silver (I) | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| Assassin Heart | Silver (I) | Your team counts as having 1 additional Assassin. Gain a Qiyana. |
| Axiom Arc I | Silver (I) | Your units gain 25 Mana on kill. |
| Band of Thieves I | Silver (I) | Gain 1 Thief's Gloves. |
| Battlemage I | Silver (I) | Your units that start combat in the front 2 rows gain 15 Armor and 15 Ability Power. |
| Best Friends I | Silver (I) | Units isolated in pairs gain 10% Attack Speed and 15 Armor at the start of combat. |
| Big Friend I | Silver (I) | Your units that start combat next to a unit with over 1800 Health take 10% less damage for the rest of combat. |
| Blue Battery I | Silver (I) | Your units gain 10 Ability Power. After casting their Ability, set the caster's Mana to 10. |
| Bruiser Heart | Silver (I) | Your team counts as having 1 additional Bruiser. Gain a Jax. |
| Built Different I | Silver (I) | Your units with no Traits active gain 250-400 Health and 30-75% Attack Speed (based on current Stage). |
| Cannoneer Heart | Silver (I) | Your team counts as having 1 additional Cannoneer. Gain an Aphelios. |
| Cavalier Heart | Silver (I) | Your team counts as having 1 additional Cavalier. Gain a Rell. |
| Celestial Blessing I | Silver (I) | Your units heal for 12% of the damage dealt by attacks and Abilities. Excess healing is converted to a shield up to 250 Health. |
| Consistency | Silver (I) | Gain double win and loss streak gold. |
| Cybernetic Implants I | Silver (I) | Your champions holding an item gain 100 Health and 8 Attack Damage. |
| Cybernetic Shell I | Silver (I) | Your champions holding an item gain 100 Health and 20 Armor. |
| Cybernetic Uplink I | Silver (I) | Your champions holding an item gain 100 Health and restore 2 Mana per second. |
| Darkflight Heart | Silver (I) | Your team counts as having 1 additional Darkflight. Gain an Aphelios. |
| Double Trouble I | Silver (I) | If you have exactly 2 copies of a champion on your board, they both gain 20 Attack Damage, Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Dragonmancer Conference | Silver (I) | Every 2 rounds, a random Dragonmancer appears on your bench. Gain a Kai'Sa. |
| Electrocharge I | Silver (I) | When your units receive critical strikes, they deal 50-110 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Evoker Heart | Silver (I) | Your team counts as having 1 additional Evoker. Gain a Zyra. |
| Exiles I | Silver (I) | Your units that start combat with no adjacent allies gain a 25% maximum Health shield for 10 seconds. |
| Featherweights I | Silver (I) | Your Tier 1 and 2 champions gain 25% Attack Speed and Move Speed. |
| First Aid Kit I | Silver (I) | All healing and shielding on your units is increased by 25%. |
| Future Sight I | Silver (I) | Know who you will fight next. Gain a Zephyr. |
| Guardian Heart | Silver (I) | Your team counts as having 1 additional Guardian. Gain a Zac. |
| Guild Heart | Silver (I) | Your team counts as having 1 additional Guild. Gain a Twitch. |
| Item Grab Bag I | Silver (I) | Gain 1 random completed item. |
| Knife's Edge I | Silver (I) | Your units that start combat in the front 2 rows gain 15 Attack Damage. |
| Lagoon Heart | Silver (I) | Your team counts as having 1 additional Lagoon. Gain a Kai'Sa. |
| Lategame Specialist | Silver (I) | When you reach Level 9, gain 45 gold. |
| Luden's Echo I | Silver (I) | When your units cast and deal Ability damage, the first target hit and a nearby enemy take 40-130 (based on current Stage) magic damage. |
| Mage Heart | Silver (I) | Your team counts as having 1 additional Mage. Gain a Lillia. |
| Mirage Heart | Silver (I) | Your team counts as having 1 additional Mirage. Gain a Yone. |
| Mystic Heart | Silver (I) | Your team counts as having 1 additional Mystic. Gain a Rakan. |
| Pandora's Bench | Silver (I) | Gain 2 gold. At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. |
| Pandora's Items | Silver (I) | Gain a random component. At the start of each round, items on your bench are randomized (excluding Tactician's Crown, Spatula, and consumables). |
| Preparation I | Silver (I) | Champions on your bench permanently gain 25 Health, 3 Attack Damage, and 3 Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Ragewing Heart | Silver (I) | Your team counts as having 1 additional Ragewing. Gain a Rakan. |
| Recombobulator | Silver (I) | Champions on your board permanently transform into random champions 1 Tier higher. Dragons can only transform into Dragons, and non-Dragons into non-Dragons. Gain 2 Magnetic Removers. |
| Scalescorn Heart | Silver (I) | Your team counts as having 1 additional Scalescorn. Gain a Braum. |
| Second Wind I | Silver (I) | After 10 seconds of combat, your units heal 40% of their missing Health. |
| Shapeshifter Heart | Silver (I) | Your team counts as having 1 additional Shapeshifter. Gain a Gnar. |
| Shimmerscale Crest | Silver (I) | Gain a Shimmerscale Emblem and a Jax. |
| Soul Siphon | Silver (I) | Darkflight champions benefit from the first-listed trait of a sacrificed champion. This does not increase the trait count for that trait. Gain an Aphelios. |
| Stand United I | Silver (I) | Your units gain 1 Attack Damage and 2 Ability Power per Trait active across your army. |
| Swiftshot Heart | Silver (I) | Your team counts has having 1 additional Swiftshot. Gain a Twitch. |
| Tempest Heart | Silver (I) | Your team counts as having 1 additional Tempest. Gain a Qiyana. |
| Thrill of the Hunt I | Silver (I) | Your units heal 400 Health on kill. |
| Tiny Titans | Silver (I) | Your Tactician heals 30 Health, grows larger, and has 130 maximum Health. |
| Tri Force I | Silver (I) | Your Tier 3 champions gain 133 Health, 13 starting Mana, and 13% Attack Speed. |
| Warrior Heart | Silver (I) | Your team counts as having 1 additional Warrior. Gain a Yone. |
| Whispers Heart | Silver (I) | Your team counts as having 1 additional Whispers. Gain a Zyra. |

