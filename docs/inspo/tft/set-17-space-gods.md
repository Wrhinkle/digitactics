# TFT Set 17: Space Gods

Source: CommunityDragon `https://raw.communitydragon.org/16.20 ("latest", current live client)/cdragon/tft/en_us.json` (set data `TFTSet17`) for champions, stats, abilities, traits, items and augments. MetaTFT lookup `https://data.metatft.com/lookups/TFTSet17_latest_en_us.json` (retrieved 2026-10-15) for spell calculation formulas used to fill computed tooltip values, augment rarity where CDragon has none, and set-mechanic lists. Realm of the Gods rules: invenglobal.com "Teamfight Tactics reveals Set 17 Space Gods" and esports.gg Set 17.1 coverage. Set 17 remains playable alongside Set 18 (Riot patch 18.1 notes).

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 17 |
| Name | Space Gods |
| Riot launch patch | TFT 17.1 |
| League client version at launch (CDragon path) | 16.8 |
| Live dates | April 15, 2026 to August 25, 2026 as the main set; still playable alongside Set 18 as of October 2026 |
| Theme | Cosmic sci-fi and fantasy; champions compete for the favor of gods (Dark Star, Galaxy Hunter, N.O.V.A., Space Groove, Stargazer, Mecha, Meeple). |
| Data snapshot used | Client 16.20 (current live data, includes post-Set 18 balance) |
| Champion count in this file | 64 |

## Set mechanics and system changes

- Realm of the Gods replaces the carousel. At the start of each game two random Gods from a pool of nine are introduced, each offering rewards. The player picks one God's offering, then gets a secondary offering from Pengu. Every God offering comes with a component.
- On the usual carousel rounds the player takes a Minor Blessing from one of the two Gods, which adds one vote toward that God. By the end of round 4-4 the votes decide which God the player worships; at 4-7 that God offers a Boon (an armory) and keeps dropping loot for the rest of the game.
- Catch-up: players with lower HP get higher-cost units from Pengu and component anvils instead of random components in PvE rounds.
- Stargazer charts a different constellation each game (the data stores one Stargazer trait per constellation, 8 entries). Choose Trait lets Miss Fortune pick Conduit, Challenger or Replicator mode, each with its own ability and trait. Replicator abilities trigger a second time at reduced effect. See traits.
- The God list in the data has 10 entries (Riot's launch text says nine in the pool).

## Champions (64)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Aatrox | 1 | N.O.V.A., Bastion | 700 | 45 | 45 | 50 | 0.6 | 1 | 30/90 |
| Briar | 1 | Anima, Primordian, Rogue | 650 | 35 | 35 | 40 | 0.75 | 1 | 0/40 |
| Caitlyn | 1 | N.O.V.A., Fateweaver | 500 | 15 | 15 | 65 | 0.55 | 4 | 0/0 |
| Cho'Gath | 1 | Dark Star, Brawler | 700 | 40 | 40 | 45 | 0.6 | 1 | 30/70 |
| Ezreal | 1 | Timebreaker, Sniper | 450 | 15 | 15 | 45 | 0.7 | 6 | 0/30 |
| Leona | 1 | Arbiter, Vanguard | 700 | 40 | 40 | 50 | 0.6 | 1 | 50/110 |
| Lissandra | 1 | Dark Star, Shepherd, Replicator | 450 | 15 | 15 | 30 | 0.7 | 4 | 0/30 |
| Nasus | 1 | Space Groove, Vanguard | 700 | 45 | 45 | 40 | 0.65 | 1 | 60/120 |
| Poppy | 1 | Meeple, Bastion | 700 | 45 | 45 | 60 | 0.65 | 1 | 30/100 |
| Rek'Sai | 1 | Primordian, Brawler | 700 | 45 | 45 | 50 | 0.6 | 1 | 40/100 |
| Talon | 1 | Stargazer, Rogue | 650 | 35 | 35 | 35 | 0.75 | 1 | 0/30 |
| Teemo | 1 | Space Groove, Shepherd | 450 | 15 | 15 | 15 | 0.7 | 4 | 0/50 |
| Twisted Fate | 1 | Stargazer, Fateweaver | 500 | 15 | 15 | 30 | 0.7 | 4 | 0/50 |
| Veigar | 1 | Meeple, Replicator | 500 | 15 | 15 | 30 | 0.7 | 4 | 10/50 |
| Akali | 2 | N.O.V.A., Marauder | 750 | 45 | 45 | 45 | 0.8 | 1 | 0/30 |
| Bel'Veth | 2 | Primordian, Challenger, Marauder | 750 | 45 | 45 | 47 | 0.75 | 2 | 0/50 |
| Gnar | 2 | Meeple, Brawler, Sniper | 550 | 20 | 20 | 48 | 0.75 | 6 | 0/5 |
| Gragas | 2 | Psionic, Brawler | 950 | 45 | 45 | 50 | 0.6 | 1 | 30/80 |
| Gwen | 2 | Space Groove, Rogue | 750 | 50 | 50 | 50 | 0.8 | 2 | 0/30 |
| Jax | 2 | Stargazer, Bastion | 950 | 45 | 45 | 50 | 0.65 | 1 | 20/80 |
| Jinx | 2 | Anima, Challenger | 550 | 20 | 20 | 55 | 0.75 | 4 | 20/80 |
| Meepsie | 2 | Meeple, Shepherd, Voyager | 950 | 45 | 45 | 20 | 0.65 | 1 | 0/55 |
| Milio | 2 | Timebreaker, Fateweaver | 550 | 20 | 20 | 30 | 0.7 | 4 | 0/40 |
| Mordekaiser | 2 | Dark Star, Conduit, Vanguard | 950 | 45 | 45 | 40 | 0.6 | 1 | 40/100 |
| Pantheon | 2 | Timebreaker, Brawler, Replicator | 900 | 45 | 45 | 50 | 0.6 | 1 | 20/80 |
| Pyke | 2 | Psionic, Voyager | 700 | 45 | 45 | 45 | 0.8 | 1 | 0/40 |
| Zoe | 2 | Arbiter, Conduit | 550 | 20 | 20 | 30 | 0.7 | 4 | 0/50 |
| Aurora | 3 | Anima, Voyager | 700 | 25 | 25 | 30 | 0.8 | 4 | 20/80 |
| Diana | 3 | Arbiter, Challenger | 850 | 50 | 50 | unknown | 0.8 | 1 | 0/40 |
| Fizz | 3 | Meeple, Rogue | 850 | 55 | 55 | 30 | 0.85 | 1 | 0/20 |
| Illaoi | 3 | Anima, Vanguard, Shepherd | 1100 | 50 | 50 | 50 | 0.65 | 1 | 40/100 |
| Kai'Sa | 3 | Dark Star, Rogue | 650 | 25 | 25 | 45 | 0.8 | 4 | 0/50 |
| Lulu | 3 | Stargazer, Replicator | 650 | 25 | 25 | 30 | 0.75 | 4 | 0/55 |
| Maokai | 3 | N.O.V.A., Brawler | 1100 | 40 | 40 | 60 | 0.7 | 1 | 30/100 |
| Miss Fortune (MissFortune) | 3 | Gun Goddess, Choose Trait | 650 | 30 | 30 | 50 | 0.75 | 6 | 0/100 |
| Miss Fortune (MissFortune_TraitClone) | 3 | Conduit, Challenger, Replicator | 1200 | 70 | 70 | 70 | 1 | 1 | 0/100 |
| Ornn | 3 | Space Groove, Bastion | 950 | 40 | 40 | 50 | 0.65 | 1 | 40/100 |
| Rhaast | 3 | Redeemer | 1200 | 60 | 60 | 60 | 0.65 | 1 | 30/90 |
| Samira | 3 | Space Groove, Sniper | 650 | 25 | 25 | 50 | 0.75 | 6 | 0/65 |
| Urgot | 3 | Mecha, Brawler, Marauder | 600 | 45 | 45 | 60 | 0.8 | 2 | 0/50 |
| Viktor | 3 | Psionic, Conduit | 650 | 25 | 25 | 30 | 0.8 | 4 | 20/80 |
| Aurelion Sol | 4 | Mecha, Conduit | 850 | 30 | 30 | 30 | 0.75 | 6 | 15/75 |
| Corki | 4 | Meeple, Fateweaver | 850 | 30 | 30 | 45 | 0.8 | 4 | 0/60 |
| Karma | 4 | Dark Star, Voyager | 850 | 30 | 30 | 40 | 0.8 | 4 | 10/55 |
| Kindred | 4 | N.O.V.A., Challenger | 850 | 30 | 30 | 58 | 0.8 | 6 | 0/40 |
| LeBlanc | 4 | Arbiter, Shepherd | 850 | 30 | 30 | 0 | 0.8 | 4 | 0/40 |
| Master Yi | 4 | Psionic, Marauder | 1100 | 65 | 65 | 60 | 0.85 | 1 | 20/60 |
| Morgana | 4 | Dark Lady, Conduit | 1300 | 70 | 70 | 60 | 0.65 | 1 | 45/95 |
| Nami | 4 | Space Groove, Replicator | 850 | 30 | 30 | 40 | 0.8 | 4 | 20/65 |
| Nunu & Willump | 4 | Stargazer, Vanguard | 1300 | 60 | 60 | 60 | 0.65 | 1 | 40/145 |
| Rammus | 4 | Meeple, Bastion | 1300 | 60 | 60 | 60 | 0.65 | 1 | 20/80 |
| Riven | 4 | Timebreaker, Rogue | 1100 | 60 | 60 | 0 | 0.85 | 1 | 0/20 |
| Tahm Kench | 4 | Oracle, Brawler | 1300 | 60 | 60 | 75 | 0.5 | 1 | 50/110 |
| The Mighty Mech | 4 | Mecha, Voyager | 1300 | 60 | 60 | 70 | 0.65 | 1 | 40/100 |
| Xayah | 4 | Stargazer, Sniper | 850 | 30 | 30 | 49 | 0.75 | 6 | 0/50 |
| Bard | 5 | Meeple, Conduit | 900 | 40 | 40 | 30 | 0.85 | 4 | 0/65 |
| Blitzcrank | 5 | Party Animal, Space Groove, Vanguard | 1000 | 50 | 50 | 50 | 0.9 | 1 | 30/120 |
| Fiora | 5 | Divine Duelist, Anima, Marauder | 1200 | 65 | 65 | 80 | 0.9 | 1 | 0/70 |
| Graves | 5 | Factory New | 900 | 40 | 40 | 60 | 0.75 | 4 | 0/60 |
| Jhin | 5 | Dark Star, Eradicator, Sniper | 900 | 40 | 40 | 80 | 0.9 | 6 | 0/44 |
| Shen | 5 | Bulwark, Timebreaker, Bastion | 1300 | 65 | 65 | 50 | 0.9 | 1 | 20/70 |
| Sona | 5 | Commander, Psionic, Shepherd | 900 | 40 | 40 | 35 | 0.9 | 4 | 0/25 |
| Vex | 5 | Doomer, Stargazer | 900 | 40 | 40 | 15 | 0.8 | 6 | 0/60 |
| Zed | 5 | Galaxy Hunter | 1100 | 60 | 60 | 85 | 0.85 | 1 | 40/100 |

### Abilities

**Aatrox** (1-cost) - Stellar Slash: Heal 325/400/650 (AP), then deal [ModifiedDamage: unknown] (AD+Armor) physical damage to the current target. N.O.V.A. Strike: Cleave the battlefield, briefly knocking up all enemies and dealing [ModifiedNovaDamage: unknown] (AD+Armor) physical damage.

**Briar** (1-cost) - Fish Frenzy: Passive: For every 1% missing Health, gain 2/2/2.5% (AP) Attack Speed Active: Deal 120/180/285% AD physical damage to the target, increased by 50% if they're a tank.

**Caitlyn** (1-cost) - Aim For The Head: Passive: Attacks have a 15% Lucky chance to fire an empowered Headshot, dealing 20/30/45 + 190/285/540% AD (AD+AP) physical damage. N.O.V.A. Strike: Mark all enemies, increasing damage taken by 10%. The first time marked targets drop below 50% Health, Headshot them for [ModifiedNovaHeadshotDamage: unknown] (AD+AP) physical damage. Lucky: Check twice and take the better outcome.

**Cho'Gath** (1-cost) - Accretion: Deal 210/290/420 + 8% max HP (HP+AP) magic damage to the lowest Health enemy in range and permanently gain 12/18/33 maximum Health. If this kills them, permanently gain 30/40/70 maximum Health instead. (Current Bonus: +(live counter) Health)

**Ezreal** (1-cost) - Temporal Shot: Active: Fire a blast at the current target that deals 14/21/32 + 170/255/380% AD (AD+AP) physical damage. Every 7 takedowns, gain a drone that deals 8/12/18% AD physical damage to the current target on cast. (Total Takedowns: (live counter))

**Leona** (1-cost) - Shield of Daybreak: Gain 420/480/620 (AP) Shield for 4 seconds. Bash the current target, dealing 100/150/225 + 120/180/270% Armor + 120/180/270% MR (Armor+MR) magic damage and stunning them for 1.75/1.75/2 seconds.

**Lissandra** (1-cost) - Dark Matter: Hurl a shard towards the current target, dealing 250/375/600 (AP) magic damage to the first target it hits. After hitting its initial target or at its final destination, the dagger explodes dealing 50/75/115 (AP) magic damage to nearby targets.

**Nasus** (1-cost) - Groovin' Susan: Transform for 6 seconds, temporarily gaining 300/400/600 max Health, entering {{TFT17_SpaceGroove_TheGroove}}, and dealing 30/45/70 + 2% max HP (HP+AP) magic damage to adjacent enemies each second.

**Poppy** (1-cost) - Huddle Up!: Gain 400/475/575 (AP) Shield for 4 seconds. For the duration, allies within two hexes gain 15/25/60 (AP) Armor and Magic Resistance. Meep Bonus: Meeps grant 125/160/210 (AP) Shield to the nearest [ModifiedNumMeeps: unknown] (set14AmpIcon) allies for 4 seconds.

**Rek'Sai** (1-cost) - Upheaval: Heal [TotalHealing: unknown] (HP+AP), then briefly knock up adjacent enemies and deal 80/120/180 (AP) magic damage.

**Talon** (1-cost) - Diviner's Judgment: Stab the target, causing them to bleed for 60/90/135 + 400/600/1100% AD (AD+AP) physical damage over 18 seconds. After the attack, leap to the highest percent Health enemy within 3 hexes.

**Teemo** (1-cost) - Double Time: Passive: Attacks deal 30/45/100 (AP) bonus magic damage and an additional 70/105/190 (AP) stacking magic damage over 6 seconds. While an enemy has 5 or more stacks, Teemo is in {{TFT17_SpaceGroove_TheGroove}}. Active: Gain 150% Attack Speed for 3 attacks.

**Twisted Fate** (1-cost) - Fate's Gambit: Draw a card with a value between 1 and 9 by Lucky chance, then throw it at the target. Based on the card drawn, deal between 30/45/70 (AP) and 680/1020/1550 (AP) magic damage. Overkill damage bounces to the nearest enemy. If a 9 is thrown, generate 1 gold. If a 1 is thrown, take 15% max health true damage. Lucky: Check twice and take the better outcome.

**Veigar** (1-cost) - Meepteor Shower: Call down a Meepteor on the target that deals 330/495/750 (AP) magic damage. Meep Bonus: An additional [ModifiedMiniMeeps: unknown] (set14AmpIcon) mini Meepteor are called down on nearby targets dealing 31/47/70 (AP) magic damage each.

**Akali** (2-cost) - Star Strike: Reposition next to the target to strike the most enemies. Then throw 5 piercing kunai, each dealing 4/6/9 + 39/59/88% AD physical damage to the first enemy hit, reduced to [ModifiedSecondaryDamage: unknown] (AD) for each subsequent target. Kunai remove 1 Armor, 2 if they crit. N.O.V.A. Strike: Slice all enemies, applying Wound and a bleed that deals 14/18/25% AD physical damage each second. Kunai increase the damage of the bleed by 12%.

**Bel'Veth** (2-cost) - Tidal Slashes: Unleash a flurry of [TotalNumSlashes: unknown] (AS) slashes at the current target over 2 seconds, dealing 3/5/7 + 22/33/50% AD (AD+AP) physical damage each.

**Gnar** (2-cost) - Slingshot Maneuver: Passive: Every 5th attack, launch a returning boomerang at the target that travels two hexes past the first enemy it hits. It deals 20/30/45 + 225/340/560% AD (AD+AP) physical damage, reduced by 75% per hit. Meep Bonus: [ModifiedNumMeeps: unknown] (set14AmpIcon) Meeps attack alongside Gnar, dealing [ModifiedMeepDPS: unknown] (AD+AS) physical damage per second.

**Gragas** (2-cost) - Chemical Rage: Heal [ModifiedHeal: unknown] (HP+AP) over 2 seconds. Then, deal 200/300/450 (AP) magic damage to target and enemies adjacent to them and 30% Chill them for 2 seconds. {{TFT_Keyword_Chill}}

**Gwen** (2-cost) - Dance n' Dice: Passive: Attacks deal magic damage. Gwen is in {{TFT17_SpaceGroove_TheGroove}} while targeting an enemy below 50% Health. Active: Dash to a nearby hex to snip the lowest percent Health enemy, dealing 155/235/440 (AP) magic damage to the target and 75/110/215 (AP) magic damage to enemies in a cone. If this kills, dash and snip again at 65% damage!

**Jax** (2-cost) - Counter Star-ike: Enter a defensive stance for 3 seconds, reducing incoming damage by 20/25/30 and gaining 400/450/500 (AP) Shield. When the stance ends, strike all nearby enemies, dealing 75/115/170% Armor + 75/115/170% MR (Armor+MR) magic damage and Stunning them for 1/1.25/1.5 second(s).

**Jinx** (2-cost) - Explosive Attitude: Fire a barrage of [ModifiedNumRockets: unknown] (AS) rockets in a cone, each dealing [TotalDamage: unknown] (AD) physical damage to the first target hit.

**Meepsie** (2-cost) - The Big Bang: Passive: Attacks deal 30/45/70 (AP+set14AmpIcon) bonus magic damage. Active: Leap to largest group of enemies within 2 hexes and slam the ground, dealing 180/270/425 (AP) magic damage in a three hex radius, reduced by 35% per hex from the epicenter. The closest 3 enemies are knocked up for 1/1/1.5 seconds. Meep Bonus: The passive's bonus damage is increased by 50% per Meep.

**Milio** (2-cost) - Mega Time Kick: Kick a ball at the current target that deals 200/300/470 (AP) magic damage. On impact, the ball has a 100% Lucky chance to bounce to a new target dealing 60/90/150 (AP) magic damage. These bounces can trigger additional bounces, but the odds halve with each bounce. Milio has a 40% chance to cast an additional time at an additional target. Bonus casts can trigger more bonus casts. Lucky: Check twice and take the better outcome.

**Mordekaiser** (2-cost) - Indestructible: Gain 350/450/625 (AP) Shield. Each second for the next 64 seconds, gain 75/100/115 (AP) more Shield and deal 45/70/100 (AP) magic damage to adjacent enemies. When this ability ends, consume the remaining Shield and heal for 40% of its value.

**Pantheon** (2-cost) - Advanced Defences: Gain 275/300/500 + 6% max HP (HP+AP) Shield and 15% Durability for 4 seconds. Over the duration, deal 30/45/70% AD physical damage each second to enemies in a cone.

**Pyke** (2-cost) - Marked for Death: Reposition up to one hex to throw a harpoon at the furthest enemy. The harpoon pulls the first enemy hit one hex forward and deals 60/90/135 (AP) physical damage. Then, teleport behind them and cleave, dealing 210/315/720% AD physical damage to them and 120/180/360% AD physical damage to nearby enemies.

**Zoe** (2-cost) - Paddle Star: Fire a paddle star at the current target, dealing 68/102/180 (AP) magic damage to the first target hit and 36/54/88 (AP) to others it passes through. When the missile reaches its destination, redirect it to a distant enemy, increasing its speed and repeating the damage. This redirect can occur 54 times.

**Aurora** (3-cost) - Hopped-Up Hacks: Open a 2 hex rift containing the target, Hacking enemies within for 4 seconds and dealing 80/120/190 (AP) magic damage to each, plus 400/600/960 (AP) magic damage split between all enemies hit. Hacked enemies store 10% of all damage they take. When the Hack ends, they take true damage equal to the stored damage. The Hack ends early if it would kill the target.

**Diana** (3-cost) - Pale Cascade: Passive: Attacks deal 52/78/135 (AP) bonus magic damage. Active: Gain 230/280/450 (AP) Shield and summon 3 encircling orbs for 3 seconds. Orbs deal 60/90/160 (AP) magic damage to enemies they pass through, and rotate faster based on Attack Speed.

**Fizz** (3-cost) - Meep Bait: Dash through the current target, dealing 140/210/310 (AP) magic damage. Every third cast also summons a Mega Meep after a delay, briefly knocking the target up and dealing [ModifiedChompDamage: unknown] (AP+set14AmpIcon) magic damage. Adjacent enemies take 50% damage. Meep Bonus: Add [ModifiedNumMeeps: unknown] (set14AmpIcon) Meep to the bait, increasing Mega Meep damage by [ModifiedMeepBonusDamage: unknown] (AP).

**Illaoi** (3-cost) - Test of Spirit: Gain 450/525/650 (AP) Shield for 3 seconds. Over the duration, drain 55/85/130 (AP) Health from the nearest 3 enemies. Then slam down, dealing 80/120/180 (AP) magic damage to all enemies within 2 hexes.

**Kai'Sa** (3-cost) - Bullet Cluster: Passive: On takedown, gain 10 mana. Active: Fire 16 missiles in a 2-hex radius around the current target, dealing 3/5/7 + 30/45/72% AD physical damage each.

**Lulu** (3-cost) - It's Raining Stars: Passive: Gain a different secondary effect each game based on the Stargazer constellation. Active: Call down something from the sky, dealing 150/225/360 (AP) magic damage to 3 nearby enemies and do some special effect based on this game's Stargazer Constellation.

**Maokai** (3-cost) - Grasp of Convergence: Passive: Gain 50% more max Health from all sources. Active: Converge an X-shape of vines on the target, dealing 100/150/225 (AP) magic damage to each enemy hit and Stunning them for 1.5/1.5/1.75 seconds. N.O.V.A. Strike: Send forth a wave of dragons that stun all enemies for 1.5/1.5/1.75 seconds. For the rest of combat, Maokai's attacks deal 12% max HP (HP) bonus physical damage.

**Miss Fortune (MissFortune)** (3-cost) - Gun Goddess Arsenal: Field Miss Fortune to choose whether she activates Conduit Mode, Challenger Mode, or Replicator Mode. The chosen mode determines her ability and her trait.

**Miss Fortune (MissFortune_TraitClone)** (3-cost) - Gun Goddess Arsenal: Field Miss Fortune to choose whether she activates Conduit Mode, Challenger Mode, or Replicator Mode. The chosen mode determines her ability and her trait.

**Ornn** (3-cost) - Disco Inferno: Passive: On combat start, forge a temporary completed item. If 3 items are already equipped, a random completed item becomes Radiant this combat instead. Active: Gain 125/200/500 (AP) Shield for 3 seconds, then breath fire in a cone dealing 180/270/430 (AP) magic damage. After any Shield on Ornn expires, Ornn enters {{TFT17_SpaceGroove_TheGroove}} for 2.5 seconds.

**Rhaast** (3-cost) - Divine Scythe: Gain 20% Durability for 1 seconds, healing 500/550/650 (AP) over the duration. Afterwards, slash forward in a line, dealing 400/600/1000% AD physical damage to enemies hit and knocking them up for 1 second.

**Samira** (3-cost) - Jump and Jive: Passive: Whenever an enemy is knocked up, shoot them, dealing 10/15/25 + 45/68/115% AD (AD+AP) physical damage and entering {{TFT17_SpaceGroove_TheGroove}} for 3 seconds. Active: Unleash a volley of bullets at the target, dealing 375/560/900% AD physical damage and knocking up for 0.75 seconds.

**Urgot** (3-cost) - Unstoppable Dreadnought: Passive - Proximity Blast: Whenever an enemy enters a 0.5% stat#31 (Range) hex radius, fire a blast in a cone towards the closest adjacent hex that deals 85/125/200% AD physical damage with 30% falloff per hex. Each adjacent hex has a 5 second cooldown, and starts combat on cooldown. Active: Gain 150/175/200 (AP) Shield for 3 seconds and reposition up to one hex to maximize the number of targets within Proximity Blast's radius. Reset Proximity Blast's cooldowns.

**Viktor** (3-cost) - Psionic Storm: Channel a one-hex psionic storm that follows enemies for 54 seconds. Each second, it grows larger and deals 200/300/530 (AP) magic damage to enemies within, reduced by 60% per hex from the epicenter.

**Aurelion Sol** (4-cost) - Deathbeam: Channel a deathbeam in a line towards the current target for 43 seconds. It deals 335/505/2000 (AP) magic damage per second, reduced by 80% per enemy it passes through. Deathbeam ignores 30% of the enemy's Magic Resist.

**Corki** (4-cost) - Asteroid Blaster: Strafe to a nearby position, unleashing 21 missiles split between the target and all enemies within two hexes. Missiles deal 5/7/24 + 28/42/280% AD (AD+AP) physical damage with a 20% Lucky chance of firing a mega missile that deals [ModifiedProcDamage: unknown] (AD) instead. Meep Bonus: Every 0 (set14AmpIcon) seconds, launch an Explosive Meep at the target, dealing 110/165/900% AD physical damage in a one hex radius on impact. Lucky: Check twice and take the better outcome.

**Karma** (4-cost) - Singularity: Gather the force of a black hole, dealing 630/945/5000 (AP) magic damage split between the target and the 2/2/4 closest enemies to them. The target takes an additional 180/270/1000 (AP) magic damage.

**Kindred** (4-cost) - Cosmic Pursuit: Passive: Attacks and Abilities mark the target. When an enemy reaches 3 marks, Wolf consumes the marks, dealing 10/15/100 + 115/175/900% AD (AD+AP) physical damage. Active: Jump up to 1 hex away and fire arrows at the nearest 3/3/5 targets, each dealing 0.75/1.15/6% AD physical damage. N.O.V.A. Strike: Gain 15% Damage Amp. Now and every 4.5 seconds after, add a mark to all enemies.

**LeBlanc** (4-cost) - Fracture Reality: Passive: Attacks deal 66/99/250 (AP) magic damage instead. Active: Summon 5 clones that attack alongside for 5 attacks, dealing 25/25/150% damage. For their final attack, clones fire a bolt that deals 95/145/750 (AP) magic damage.

**Master Yi** (4-cost) - Psi Strikes: Passive: Every third attack is a doubleslash that deals 70/105/550% AD bonus physical damage. Active: After brief meditation, enter a Psi-State for 5 seconds, gaining 10% Omnivamp, 70% Attack Speed, and increased movement speed. Twice a second, fire a psychic projection at a random nearby enemy that deals 20/30/200 + 55/85/600% AD (AD+AP) physical damage

**Morgana** (4-cost) - Dark Form: Transform for 3 seconds, gaining [ModifiedHealthGain: unknown] (AP+HP) Health and tethering to nearby champions. Over the duration, deal 100/150/3000 (AP) magic damage to the closest 3 enemies and restore 125/185/3000 (AP) health to the closest 2 injured allies.

**Nami** (4-cost) - Bubble Pop: Toss a disco bubble at the target that deals 460/690/5000 (AP) split between enemies in a one hex radius. The explosion sends 3 globs towards nearby enemies that deal 125/185/1000 (AP) magic damage. On cast, Nami enters {{TFT17_SpaceGroove_TheGroove}} for 3 seconds.

**Nunu & Willump** (4-cost) - Calamity: Gain 475/575/2000 (AP) Shield for 4 seconds. Summon an astrolabe to crash down on a nearby hex, dealing 120/180/2000 (AP) magic damage to enemies within 2 hexes. Then, push the astrolabe towards the end of the board, dealing 100/150/2000 (AP) magic damage. All enemies hit by the astrolabe are knocked up for 1.5/1.75/8 seconds.

**Rammus** (4-cost) - Gravitational Spin: Gain 675/825/2000 (AP) shield for 4 seconds. Then, strike enemies in a three hex line, dealing 50/75/700 + 50/75/1100% Armor (AP+Armor) magic damage. Meep Bonus: Reduce the damage of incoming attacks by [ModifiedFlatDR: unknown] (set14AmpIcon). After being attacked 20 times, deal [ModifiedPassiveDamage: unknown] (Armor+set14AmpIcon) magic damage in a two hex radius.

**Riven** (4-cost) - Time Warp: Passive: This Ability adapts to Attack Damage or Ability Power, whichever is higher. Attacks deal 75/115/300 (AP) magic75/115/300% AD physical damage. Active: Dash to a nearby hex, gaining 100/150/1200 Shield for 2 seconds and slashing adjacent enemies for 100/150/1000 (AP) magic100/150/1000% AD physical damage. Every third cast, leap into the air and launch a wave of energy that deals 180/270/2000 (AP) magic180/270/2000% AD physical damage to enemies in a line.

**Tahm Kench** (4-cost) - Tounge Lash: Passive: Once per combat after dropping below 35% Health, gain Shield for 4 seconds equal to 40% of healing received this combat. Active: Heal 300/360/1500 + 8.5% max HP (HP+AP), then tongue lash all enemies within two hexes dealing 45/60/1500 + 2% max HP (AP+HP) magic damage.

**The Mighty Mech** (4-cost) - Gravity Matrix: Enter a defensive stance for 4 seconds gaining 20/20/60% Durability. While in the defensive stance, attract nearby enemy projectiles and heal 900/1300/3000 (AP) over the duration. When this ends, release a shockwave that deals 80/120/3000% Armor + 80/120/3000% MR (Armor+MR) as physical damage in a 2-hex range.

**Xayah** (4-cost) - Stellar Ricochet: Passive: Attacks bounce to strike 3/3/5 times, dealing 60/60/30% reduced damage per target hit and leaving a Feather behind the final target. Active: Gain 75% Attack Speed for the next 6 attacks. At the end, recall all Feathers split between the closest 3 enemies, dealing 6/9/60 + 45/68/900% AD physical damage each. Xayah's current target takes 25/40/200% AD additional physical damage per feather.

**Bard** (5-cost) - Ultra Friendly Object: Summon a flying saucer over the current target that lasts 54 seconds. Each second it deals 240/360/3000 (AP) magic damage to the target, plus 135/205/1500 (AP) magic damage split between all enemies within 1 hex. The saucer deals 30% increased damage to Tanks. If an enemy under the UFO dies, Bard has a 15/20/100% chance to abduct them and create a 1-star copy on your bench. Meep bonus: On combat start, grant the nearest [ModifiedNumAllies: unknown] (set14AmpIcon) Meeple allies an additional Meep. Friends abducted: (live counter)

**Blitzcrank** (5-cost) - Party Crasher: Passive: Every 2/2/0.5 seconds, call down a bolt on the highest Health nearby enemy that deals 60/90/150 (AP) magic damage. Active: Summon a disco ball at the largest clump of enemies, then knock up the current target into it, dealing 150/225/999 (AP) magic damage. They crash down into the disco ball, dealing 175/265/5000 (AP) magic damage in a three hex radius. Enter {{TFT17_SpaceGroove_TheGroove}} for 1 second per enemy hit.

**Fiora** (5-cost) - Perfect Bladework: Passive: Every 2 attacks, reveal a Vital on the target. If a Vital exists, dash to attack it dealing 37/56/777% AD bonus true damage and heal for 15% of the damage dealt. Active: Reveal 6 Vitals on the target and quickly attack them all. If the target dies, remaining Vitals transfer to the nearest enemy. Upon striking the final Vital, create a two hex aura that heals allies within for 200/250/999 (AP) over 5 seconds.

**Graves** (5-cost) - Collateral Damage: Passive: Attacks fire 5 projectiles in a cone that deal 33% AD physical damage each. Active: Fire an explosive shell that deals 390/585/5555% AD physical damage to the target, and 30/45/777 + 135/200/3333% AD (AD+AP) physical damage to adjacent enemies.

**Jhin** (5-cost) - Space Opera: Passive: Jhin has a fixed attack speed of 0.9/0.9/1.4 and converts every 1% of bonus Attack Speed into 0.75 bonus Attack Damage. Active: Summon 4 spectral hands that fire alongside Jhin for the next 4 attacks. Each hand deals 4/6/44 + 41/62/644% AD physical damage per shot. The final shots deal 244% more damage and pierce through the most enemies in a line, dealing 44% reduced damage per target hit.

**Shen** (5-cost) - Reality Tear: Passive: On cast, attacks gain 25/40/777 + 1% max HP (HP+AP) stacking bonus magic damage. Starting from the third cast, gain true damage instead. Active: Gain 200/250/3456 + 15% max HP (HP+AP) Shield for 3 seconds, then slice open a rift on the largest group of champions dealing the Passive's bonus damage to all enemies within. Enemies have their Attack Speed slowed by 50/50/99% while allies within gain 80/80/999% Attack Speed, both rapidly decaying over 3 seconds. Shen always gains his rift's full bonus for this duration.

**Sona** (5-cost) - Psionic Crush: Hurl a chunk of magnetic debris at the nearest target without one, dealing 300/450/999 (AP) magic damage and sticking it to them. If an enemy with debris dies, it passes to the nearest enemy without one. Every 5 casts, instead rip off all debris dealing 120/180/999 (AP) magic damage, then crush all the debris onto the target, dealing 720/1100/9999 (AP) magic damage and briefly stunning them.

**Vex** (5-cost) - Lend Me a Hand, Shadow!: Passive: Every time Vex attacks, Shadow strikes a nearby enemy, dealing 27/40/250 (AP) magic damage. Whenever an enemy is struck 5 times by Shadow, Shadow strikes them again. Active: Shadow launches 3 empowered strikes, dealing 130/195/1000 (AP) magic damage instead.

**Zed** (5-cost) - Quantum Clone: Create a clone behind the target with 25/35/40% reduced max Health and 30 increased Mana cost. The clone inherits its creator's items, stats, and current Health, and can cast Quantum Clone.

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): Golem, Rift Scuttler, Training Dummy, Tome of Traits, Completed Item Anvil, Component Anvil, Artifact Item Anvil, Support item anvil, Mercenary Chest, Bia & Bayin, N.O.V.A. Team Attack, Swarmling Damage, Tempest Strike, TimebreakerCore, Cosmic Squid, Cosmic Scrapper, Cosmic Bruiser, Cosmic Flutterbye, Cosmic Gromp, Cosmic Elder Dragon, Black Hole, Mini Black Hole, Apex Primordian.

## Traits (43)

### Anima

After losing a player combat, gain 20 Tech, plus additional Tech equal to 5 times the length of your loss streak. Additionally, gain 2 Tech per Anima takedown. Each time Animas get 100 Tech, they prototype new Anima Weapons. You can take them, or save your Tech to get more powerful weapons next time. Gain 85% Tech from losing if your partner wins a combat in Double Up.

| Units | Effect |
|---|---|
| 3-4 | Start Researching! |
| 5 | After each player combat, gain loot! |

Champions: Briar (1), Jinx (2), Aurora (3), Illaoi (3), Fiora (5)

### Arbiter

Scribe a unique divine law, allowing you to choose an effect to apply to Arbiters when a chosen cause occurs.

| Units | Effect |
|---|---|
| 2 | Choose a cause and effect for your law |
| 3 | Effects are stronger. |

Champions: Leona (1), Zoe (2), Diana (3), LeBlanc (4)

### Bastion

Your team gains 15 Armor and Magic Resist. Bastions gain more, and the value doubles in the first 10 seconds of combat.

| Units | Effect |
|---|---|
| 2-3 | 16 Armor+MR |
| 4-5 | 40 Armor+MR |
| 6 | 60 Armor+MR; Non-Bastions gain an additional 20 Armor+MR. |

Champions: Aatrox (1), Poppy (1), Jax (2), Ornn (3), Rammus (4), Shen (5)

### Brawler

Your team gains 5% Health. Brawlers gain more.

| Units | Effect |
|---|---|
| 2-3 | +25% maximum Health |
| 4-5 | +45% maximum Health |
| 6 | +65% maximum Health |

Champions: Cho'Gath (1), Rek'Sai (1), Gnar (2), Gragas (2), Pantheon (2), Maokai (3), Urgot (3), Tahm Kench (4)

### Bulwark

Summon a placeable relic. At the start of combat, it grants adjacent allies a 10% max Health shield and 10% Attack Speed.

| Units | Effect |
|---|---|
| 1 | AttackSpeed 0.1; AttackSpeedDuration 60; PercentHealthShield 0.1; ShieldDuration 60 |

Champions: Shen (5)

### Challenger

Your team gains 10% Attack Speed. Challengers gain bonus Attack Speed. When their target dies, Challengers dash to a new target and increase their Attack Speed bonus by 50% for 2.5 seconds.

| Units | Effect |
|---|---|
| 2 | 15% AS |
| 3 | 28% AS |
| 4 | 42% AS |
| 5 | 55% AS |

Champions: Bel'Veth (2), Jinx (2), Diana (3), Miss Fortune (MissFortune_TraitClone) (3), Kindred (4)

### Choose Trait

When you field Miss Fortune, choose between Conduit Mode, Challenger Mode, and Replicator Mode. Miss Fortune has a unique ability based on her mode and gains the associated trait.

Champions: Miss Fortune (MissFortune) (3)

### Commander

(1) Sona gives you a random Command Mod every 2 rounds which allows you to alter the way an ally behaves during combat. Command Mods last 2 player combats even if they are not equipped.

| Units | Effect |
|---|---|
| 1 | AS 0.15; HealthRatio 0.15; RoundsPerMod 2 |

Champions: Sona (5)

### Conduit

Innate: Conduits gain 20% additional Mana from all sources. Your team gains Mana Regen, increased for Conduits.

| Units | Effect |
|---|---|
| 2 | 1 TFTManaRegen / 3 TFTManaRegen |
| 3 | 1 TFTManaRegen / 5 TFTManaRegen |
| 4 | 2 TFTManaRegen / 7 TFTManaRegen |
| 5 | 2 TFTManaRegen / 9 TFTManaRegen |

Champions: Mordekaiser (2), Zoe (2), Miss Fortune (MissFortune_TraitClone) (3), Viktor (3), Aurelion Sol (4), Morgana (4), Bard (5)

### Dark Lady

Your team gains 4% Durability, increased to 10% when Morgana is in her Dark Form.

| Units | Effect |
|---|---|
| 1 | Durability 0.04; TransformedDurability 0.1 |

Champions: Morgana (4)

### Dark Star

| Units | Effect |
|---|---|
| 2-3 | Dark Stars create a black hole that consumes enemies at (live counter)%8% max health. |
| 4-5 | AND they gain 45% AD+AP. |
| 6-8 | AND the strongest Dark Star unit goes supermassive, gaining 70% effectiveness from Dark Star, and creates 2 minor Black Holes. |
| 9 | All Dark Stars are supermassive. At level 10, CONSUME EVERYONE. |

Champions: Cho'Gath (1), Lissandra (1), Mordekaiser (2), Kai'Sa (3), Karma (4), Jhin (5)

### Divine Duelist

Your Tactician heals for 15% of player damage dealt from winning. Fiora always wins a one on one duel.

| Units | Effect |
|---|---|
| 1 | DUHeal 1; PVEHP 1; PVENumWins 4; PlayerOmnivamp 0.15 |

Champions: Fiora (5)

### Doomer

Combat Start: Mark all enemies with Doom. The first time enemies are damaged each combat, their Doom is consumed, stealing 12% Attack Damage and Ability Power from them and granting it to your strongest Vex.

| Units | Effect |
|---|---|
| 1 | ADAP1 12 |

Champions: Vex (5)

### Eradicator

Enemies have 10% less Armor and Magic Resist.

| Units | Effect |
|---|---|
| 1 | PctResists 0.1 |

Champions: Jhin (5)

### Factory New

After participating in combat, open an armory to purchase a permanent upgrade for your strongest Graves. Every 3 upgrades, future upgrades will take an additional round. Next Upgrade: (live counter) Rounds.

| Units | Effect |
|---|---|
| 1 | NumberOfUpgradesBeforeRoundCostIncrease 3 |

Champions: Graves (5)

### Fateweaver

Innate: Fateweavers have Precision. {{TFT_Keyword_Precision}} Lucky: Check twice and take the better outcome.

| Units | Effect |
|---|---|
| 2-3 | Chance effects on abilities are Lucky. |
| 4 | Gain 20% Crit Chance and 20% Crit Damage. Critical strikes are also Lucky. |

Champions: Caitlyn (1), Twisted Fate (1), Milio (2), Corki (4)

### Galaxy Hunter

Zed is obtained from the Invader Zed augment. While at least one clone is alive, Zed gains 40% bonus Attack Damage.

| Units | Effect |
|---|---|
| 1 | BonusAD 0.4 |

Champions: Zed (5)

### Gun Goddess

When you field Miss Fortune, choose between Conduit Mode, Challenger Mode, and Replicator Mode. Miss Fortune has a unique ability based on her mode and gains the associated trait.

| Units | Effect |
|---|---|
| 1 | Allies 2; DamageAmp 0.01; FlatDamage 100; Mana 2; NumAttacks 3 |

Champions: Miss Fortune (MissFortune) (3)

### Marauder

Your team gains 5% Omnivamp. Marauders gain more Omnivamp, Attack Damage, and their Omnivamp overhealing is converted into Shield (up to 25% max Health.)

| Units | Effect |
|---|---|
| 2-3 | 5% Omnivamp, 18% AD |
| 4-5 | 7% Omnivamp, 35% AD |
| 6 | 10% Omnivamp, 55% AD. |

Champions: Akali (2), Bel'Veth (2), Urgot (3), Master Yi (4), Fiora (5)

### Mecha

Innate: Mecha units can transform into their Ultimate form, upgrading their ability and gaining 40% Health. Transformed Mechas take up two team slots and count twice for the Mecha trait. Use the Mecha-Former item to toggle the forms of your Mecha units

| Units | Effect |
|---|---|
| 3 | Energy Cells: Mechas gain 25% AD+AP. |
| 4-5 | Overclocked Cells: Increased to 40% AD+AP. |
| 6 | Precision Engineering: +1 max team size |

Champions: Urgot (3), Aurelion Sol (4), The Mighty Mech (4)

### Meeple

Meeple attract Meeps that empower Meeple abilities in meepy ways. They also gain bonus Health. Cloning time = Champion cost

| Units | Effect |
|---|---|
| 3-4 | 2 set14AmpIcon, 100 HP |
| 5-6 | 3 set14AmpIcon, 350 HP |
| 7-9 | 4 set14AmpIcon, 450 HP. Create a Cloning Slot on your bench. Gain gold and a 1-star copy of the champion placed there when cloning completes. |
| 10 | 6 set14AmpIcon, 550 HP. SUMMON THE FOUR MEEPLORDS! |

Champions: Poppy (1), Veigar (1), Gnar (2), Meepsie (2), Fizz (3), Corki (4), Rammus (4), Bard (5)

### N.O.V.A.

Aatrox: Ally Damage 30% Shred and Sunders enemiesAatrox: Shred and Sunder enemies Caitlyn: Grant allies 20% Attack SpeedCaitlyn: Grant Attack Speed Akali: Allies gain PrecisionAkali: Allies gain Precision Maokai: Allies heal 12% max HealthMaokai: Heal allies Kindred: Shield the strongest Tank for 800Kindred: Shield an ally Emblem: Allies deal 10% stacking bonus true damageEmblem: Deal bonus true damage

| Units | Effect |
|---|---|
| 2-4 | 6 seconds into combat, N.O.V.A. grant a power surge to allies based on champions. |
| 5 | Gain a Striker selector. The chosen N.O.V.A. activates their Strike during the power surge. |

Champions: Aatrox (1), Caitlyn (1), Akali (2), Maokai (3), Kindred (4)

### Oracle

Every 3 rounds, Tahm Kench grants a reward! Rounds Remaining: (live counter) Last Reward: (live counter)

| Units | Effect |
|---|---|
| 1 | rounds 3 |

Champions: Tahm Kench (4)

### Party Animal

Once per combat, after falling below 45% percent Health, become untargetable and repair 15% max Health per second. Upon reaching full Health, or when no other allies remain, return to combat. If fully healed, for the rest of combat Blitzcrank is in {{TFT17_SpaceGroove_TheGroove}} and Party Crasher's passive fires bolts four times as fast.

| Units | Effect |
|---|---|
| 1 | HealthThreshold 0.45; PercentHealthHeal 0.15 |

Champions: Blitzcrank (5)

### Primordian

8% of damage taken contributes to damage dealt.

| Units | Effect |
|---|---|
| 2 | Dealing damage spawns Swarmlings based on unique Primordian star level. |
| 3 | Spawn 45% more Swarmlings! After each player combat, gain a random 1 or 2-cost champion. |

Champions: Briar (1), Rek'Sai (1), Bel'Veth (2)

### Psionic

Gain Psionic items that can be equipped to any ally.

| Units | Effect |
|---|---|
| 2-3 | Gain the (live counter) |
| 4 | Gain the (live counter), Psionic items gain extra effects on Psionic units |

Champions: Gragas (2), Pyke (2), Viktor (3), Master Yi (4), Sona (5)

### Redeemer

Teamwide Attack Speed: (live counter)% AS Teamwide Resists: (live counter) Armor+MR

| Units | Effect |
|---|---|
| 1 | For each non-unique trait you have active, your team gains 2% Attack Speed, and 2 Armor and Magic Resist. |

Champions: Rhaast (3)

### Replicator

Replicator abilities occur a second time at reduced effectiveness.

| Units | Effect |
|---|---|
| 2-3 | 22% strength |
| 4 | 50% strength |

Champions: Lissandra (1), Veigar (1), Pantheon (2), Lulu (3), Miss Fortune (MissFortune_TraitClone) (3), Nami (4)

### Rogue

Rogues gain Attack Damage and Ability Power. The first time they fall below 50% health, they slip into shadows. Enemies targeting them are redirected to a nearby unit, preferring Tanks.

| Units | Effect |
|---|---|
| 2 | 12% AD AP |
| 3 | 25% AD AP |
| 4 | 45% AD AP |
| 5 | 60% AD AP |

Champions: Briar (1), Talon (1), Gwen (2), Fizz (3), Kai'Sa (3), Riven (4)

### Shepherd

Shepherds summon the Bond of the Stars to aid them in battle. Bia and Bayin's power are increased by the total star level of all Shepherds.

| Units | Effect |
|---|---|
| 3-4 | Summon Bia |
| 5-6 | Summon Bayin |
| 7 | Bia and Bayin's bond grows deeper |

Champions: Lissandra (1), Teemo (1), Meepsie (2), Illaoi (3), LeBlanc (4), Sona (5)

### Sniper

Snipers gain Damage Amp, increased against targets farther away.

| Units | Effect |
|---|---|
| 2 | 18% Damage Amp; +2% Damage Amp per hex |
| 3 | 25% Damage Amp; +3% Damage Amp per hex |
| 4 | 35% Damage Amp; +4% Damage Amp per hex |

Champions: Ezreal (1), Gnar (2), Samira (3), Xayah (4), Jhin (5)

### Space Groove

{{TFT17_SpaceGroove_TheGroove}}: (live counter)% AS, (live counter)% HPRegen

| Units | Effect |
|---|---|
| 1-2 | Groovians can enter {{TFT17_SpaceGroove_TheGroove}}. While in it, they gain Attack Speed and max Health Regen, increased per Groovian on your team. |
| 3-4 | All Groovians start combat in {{TFT17_SpaceGroove_TheGroove}} for 4 seconds. |
| 5-6 | Each second spent in {{TFT17_SpaceGroove_TheGroove}} grants 5% stacking Attack Damage and Ability Power. |
| 7-9 | Increase these effects by 10%! |
| 10 | {{TFT17_SpaceGroove_Groove}} |

Champions: Nasus (1), Teemo (1), Gwen (2), Ornn (3), Samira (3), Nami (4), Blitzcrank (5)

### Stargazer (Stargazer_Fountain)

Stargazers chart a different constellation every game. This game: The Fountain. Allies in empowered hexes heal 1% max Health every 2 seconds. Stargazers in empowered hexes heal an additional 3% and gain stacking Attack Damage and Ability Power every 2 seconds. More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3-4 | 4% AD AP |
| 5 | 9% AD AP |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Huntress)

Stargazers chart a different constellation every game. This game: The Huntress. Combat Start: Mark the highest Health enemies. Allies in empowered hexes gain 15% Attack Speed. Stargazers in empowered hexes gain more and heal for 15% of their max Health when a marked enemy dies. More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3-4 | 15% AS, 3 marks |
| 5-6 | 45% AS, 5 marks |
| 7 | 70% AS, 9 marks |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Medallion)

Stargazers chart a different constellation every game. This game: The Medallion. (Current Bonus: (live counter)% Damage Amp) More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3 | Allies in empowered hexes gain 15% Damage Amp, which increases by 5% for each 3-star ally. |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Mountain)

Stargazers chart a different constellation every game. This game: The Mountain Every 5 player combats, gain a Stargazer Emblem. Stargazers in empowered hexes gain various bonuses. (Combats Remaining: (live counter)) More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3 | Gain 15% Health |
| 4 | AND 15% Attack Damage and Ability Power |
| 5 | AND 10 Armor and Magic Resist |
| 6 | AND 8% Attack Speed |
| 7 | AND 5% Durability |
| 8 | AND increase all other bonuses by 10% |
| 9 | Mountain_ADAP 0.15; Mountain_AS 0.08; Mountain_DR 0.05; Mountain_Health 0.15; Mountain_Resists 10; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease 0.1 |
| 10 | Mountain_ADAP 0.15; Mountain_AS 0.08; Mountain_DR 0.05; Mountain_Health 0.15; Mountain_Resists 10; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease 0.1 |
| 11 | Mountain_ADAP 0.15; Mountain_AS 0.08; Mountain_DR 0.05; Mountain_Health 0.15; Mountain_Resists 8; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease 0.1 |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Serpent)

Stargazers chart a different constellation every game. This game: The Serpent. Allies in empowered hexes gain 5% Durability. Stargazers in empowered hexes gain more and poison enemies, repeating a portion of damage dealt as magic damage over 3 seconds. More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3-4 | 5% Durability, 25% poison damage |
| 5-6 | 10% Durability, 45% poison damage |
| 7 | 15% Durability, 60% poison damage |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Shield)

Stargazers chart a different constellation every game. This game: The Altar. More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3 | When ANY champion dies, they are sacrificed to the Altar. Allies in empowered hexes gain 8% Health and 8% Attack Speed. After 60 sacrifices, Stargazers in empowered hexes gain an additional 20% HP and 18% AS Sacrifices: ((live counter) / 60) |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer_Wolf)

Stargazers chart a different constellation every game. This game: The Boar. Gain gold after winning player combat. Allies in empowered hexes gain 8% Health, Attack Damage, and Ability Power. Stargazers gain more. (Gold Earned: (live counter)) More hexes reveal at each player level

| Units | Effect |
|---|---|
| 3 | 1 gold, 2% HP, 10% AD+AP |
| 4 | 2 gold, 8% HP, 15% AD+AP |
| 5 | 4 gold, 15% HP, 25% AD+AP |
| 6 | 6 gold, 22% HP, 30% AD+AP |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Stargazer (Stargazer)

Stargazers chart a different constellation every game. Stargazers in empowered hexes gain various bonuses, starting at (3) units. More hexes reveal at each player level.

| Units | Effect |
|---|---|
| 3-4 | Family_BaseSharePercent 0.1; Family_HexesToReveal_TOOLTIPONLY 3; Family_Stats 10; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Fountain_StackingADAP 2; Huntress_AS 0.3; Huntress_AS_Teamwide 0.15; Huntress_Heal 0.05; MaxStacks null; Medallion_DA 0.2; Medallion_IncreasePer3Star 20; Mountain_Health 0.2; Mountain_RoundsPerEmblem 5; NumMarks 3; Serpent_DR 0.15; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Serpent_Poison 0.3; Shield_AS_Teamwide 8; Shield_CashoutAS 18; Shield_CashoutHP 20; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_ADAP 10; Wolf_Gold 1; Wolf_Health 0.02; Wolf_Health_Teamwide 0.08 |
| 5-6 | Family_BaseSharePercent 0.1; Family_HexesToReveal_TOOLTIPONLY 2; Family_Stats 35; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Fountain_StackingADAP 4; Huntress_AS 0.45; Huntress_AS_Teamwide 0.15; Huntress_Heal 0.08; MaxStacks null; Medallion_DA 0.3; Medallion_IncreasePer3Star 20; Mountain_ADAP 0.12; Mountain_Health 0.3; Mountain_RoundsPerEmblem 5; NumMarks 5; Serpent_DR 0.2; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Serpent_Poison 0.45; Shield_AS_Teamwide 8; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_ADAP 15; Wolf_Gold 2; Wolf_Health 0.08; Wolf_Health_Teamwide 0.08 |
| 7 | Family_BaseSharePercent 0.1; Family_EmpoweredSharePercent 0.3; Family_Stats 35; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Fountain_StackingADAP 4; Huntress_AS 0.6; Huntress_AS_Teamwide 0.15; Huntress_Heal 0.1; MaxStacks null; Medallion_DA 0.4; Medallion_IncreasePer3Star 20; Mountain_AS null; Mountain_DR null; Mountain_Health 0.45; Mountain_Resists 12; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease null; NumMarks 7; Serpent_DR 0.25; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Serpent_Poison 0.6; Shield_AS_Teamwide 8; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_ADAP 25; Wolf_Gold 4; Wolf_Health 0.15; Wolf_Health_Teamwide 0.08 |
| 8 | Family_BaseSharePercent 0.1; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Huntress_AS_Teamwide 0.15; MaxStacks null; Medallion_IncreasePer3Star 20; Mountain_ADAP null; Mountain_AS null; Mountain_DR null; Mountain_Resists null; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease null; NumMarks 3; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Shield_AS_Teamwide 8; Shield_CashoutAS 18; Shield_CashoutHP 20; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_ADAP 10; Wolf_Gold 1; Wolf_Health 0.02; Wolf_Health_Teamwide 0.08 |
| 9 | Family_BaseSharePercent 0.1; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Huntress_AS_Teamwide 0.15; MaxStacks null; Medallion_IncreasePer3Star 20; Mountain_ADAP 0.12; Mountain_AS 0.12; Mountain_DR 0.06; Mountain_Health 0.12; Mountain_Resists 12; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease null; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Shield_AS_Teamwide 8; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_Health_Teamwide 0.08 |
| 10 | Family_BaseSharePercent 0.1; Fountain_HealthRegen 0.04; Fountain_HealthRegen_Teamwide 0.02; Huntress_AS_Teamwide 0.15; MaxStacks null; Medallion_IncreasePer3Star 20; Mountain_ADAP 0.12; Mountain_AS 0.12; Mountain_DR 0.06; Mountain_Health 0.12; Mountain_Resists 12; Mountain_RoundsPerEmblem 5; Mountain_StatIncrease 0.1; Serpent_DR_Teamwide 0.08; Serpent_Duration 3; Shield_AS_Teamwide 8; Shield_Duration 10; Shield_Health_Teamwide 8; Shield_Interval 1; Shield_NumDeaths 60; Wolf_Health_Teamwide 0.08 |

Champions: Talon (1), Twisted Fate (1), Jax (2), Lulu (3), Nunu & Willump (4), Xayah (4), Vex (5)

### Timebreaker

Rerolls on Loss: (live counter) XP on Win: (live counter)

| Units | Effect |
|---|---|
| 2 | Allies gain 12% Attack Speed. |
| 3 | AND When you lose, gain free rerolls. When you win, store XP in a Temporal Core (scales with stage). |
| 4 | AND Timebreakers gain an additional 50% Attack Speed |

Champions: Ezreal (1), Milio (2), Pantheon (2), Riven (4), Shen (5)

### Vanguard

Vanguards gain 5% Durability while Shielded. Combat start and 50% Health: Gain a max Health Shield for 10 seconds.

| Units | Effect |
|---|---|
| 2-3 | 16% max Health |
| 4-5 | 30% max Health |
| 6 | 40% max Health; 8% Durability while Shielded |

Champions: Leona (1), Nasus (1), Mordekaiser (2), Illaoi (3), Nunu & Willump (4), Blitzcrank (5)

### Voyager

Combat Start: Your Tanks gain a Shield for 15 seconds. Your other allies gain Damage Amp. Voyagers gain double.

| Units | Effect |
|---|---|
| 2 | 175 Shield; 9% Damage Amp |
| 3 | 250 Shield; 15% Damage Amp |
| 4 | 350 Shield; 18% Damage Amp |
| 5 | 500 Shield; 22% Damage Amp |
| 6 | 700 Shield; 27% Damage Amp |

Champions: Meepsie (2), Pyke (2), Aurora (3), Karma (4), The Mighty Mech (4)

## Traits with no champion (granted by augments, items or mechanics) (1)

### God-Blessed

(live counter) (live counter) (live counter) (live counter) (live counter) (live counter) (live counter) (live counter) (live counter) (live counter)

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
| Tear of the Goddess |  |  | TFTManaRegen +1 Mana Regen |

### Completed items (39)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Adaptive Helm | Negatron Cloak + Tear of the Goddess | +20 MR | Gain an additional 15% Mana from all sources. The holder gains an additional bonus based on their Role: Tanks and Fighters: Gain 30 Armor and Magic Resistance. Other Roles: Gain 10% Attack Damage and Ability Power. |
| Archangel's Staff | Needlessly Large Rod + Tear of the Goddess | +30 AP | Combat start: Gain 20% Ability Power every 5 seconds in combat. |
| Bloodthirster | B.F. Sword + Negatron Cloak | +15% AD, +15 AP, +20 MR, +20% Omnivamp | Once per combat at 40% Health, gain a 25% max Health Shield that lasts up to 5 seconds. |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | +15% AD, +15 AP | Gain 10% additional Attack Damage and Ability Power from all sources. |
| Bramble Vest | Chain Vest + Chain Vest | +50 Armor | Gain 6% max health. Take 5% reduced damage from attacks. When struck by any attack, deal 100 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Crownguard | Needlessly Large Rod + Chain Vest | +20 AP, +20 Armor, +100 HP | Combat Start: Gain a 25% max Health Shield for 8 seconds. When the Shield expires, gain 25% Ability Power. |
| Deathblade | B.F. Sword + B.F. Sword | +55% AD | Perfect peace and calm for the holder - and all who face it. (live counter) |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | +60 MR | Gain 6% max health. Every 2 seconds, heal 2.5% max Health. |
| Edge of Night | B.F. Sword + Chain Vest | +10% AD, +10 AP, +15% AS, +20 Armor | At 60% Health, briefly become untargetable, shed negative effects, and heal 20% missing health. |
| Evenshroud | Negatron Cloak + Giant's Belt | +20 MR, +250 HP | 30% Sunder enemies within 2 hexes. Gain 15 Armor and Magic Resist for the first 15 seconds of combat. Sunder: Reduce Armor |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | +25 Armor, +25 MR, +100 HP | Gain 10 Armor and 10 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | B.F. Sword + Recurve Bow | +15% AD, +15 AP, +15% AS, +15% DamageAmp | Gain 15% additional Damage Amp against Tanks. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | +10 AP, +10% AS | Gain 7% stacking Attack Speed every second. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | +20% Crit | Gain 2 effects:15% Attack Damage and 15% Ability Power.12% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | +20% AD, +20 AP, +18 Omnivamp, +15% Omnivamp | Heal the lowest percent Health ally for 20% of damage dealt. Ally Healing: (live counter) |
| Infinity Edge | B.F. Sword + Sparring Gloves | +35% AD, +35% Crit | Gain Precision. {{TFT_Keyword_Precision}} |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | +15 AP, +35 MR, +250 HP | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 150% of the Mana spent Shred: Reduce Magic Resist |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | +35 AP, +35% Crit | Gain Precision. {{TFT_Keyword_Precision}} |
| Kraken's Fury | Negatron Cloak + Recurve Bow | +10% AD, +10% AS, +20 MR | Attacks grant 3.5% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 15% Attack Speed. |
| Last Whisper | Recurve Bow + Sparring Gloves | +15% AD, +20% AS, +20% Crit | Damage from attacks and Abilities 30% Sunder the target for 3 seconds. This effect does not stack. Sunder: Reduce Armor |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | +20 AP, +150 HP | Attacks and Abilities deal 1% Burn and 33% Wound to enemies for 10 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Nashor's Tooth | Recurve Bow + Giant's Belt | +15 AP, +10% AS, +150 HP, +20% Crit | Attacks grant 2 bonus Mana, increased to 4 if they critically strike. |
| Protector's Vow | Tear of the Goddess + Chain Vest | +25 Armor, +25 MR | Combat Start: Gain 20 Mana. At 40% Health, gain 15 Mana and a Shield equal to 20% max Health. |
| Quicksilver | Sparring Gloves + Negatron Cloak | +15% AS, +20 MR, +20% Crit | Combat Start: Gain immunity to crowd control for 18 seconds. Gain 3% stacking Attack Speed every second. |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | +55 AP | This humble hat can help you make, or unmake, the world itself. (live counter) |
| Red Buff | Recurve Bow + Recurve Bow | +45% AS | Attacks and Abilities 1% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Spear of Shojin | B.F. Sword + Tear of the Goddess | +15% AD, +15 AP | Attacks grant 5 bonus Mana. |
| Spirit Visage | Tear of the Goddess + Giant's Belt | +300 HP | Regenerate 2% of missing Health each second. |
| Steadfast Heart | Chain Vest + Sparring Gloves | +20 Armor, +250 HP, +20% Crit | Gain 5% Durability. While above 50% Health, instead gain 15% Durability. (live counter) |
| Sterak's Gage | B.F. Sword + Giant's Belt | +45% AD, +300 HP | At 60% Health, gain a Shield equal to 40% of the wearer's maximum Health that rapidly decays over 4 seconds. |
| Striker's Flail | Giant's Belt + Sparring Gloves | +10% AS, +150 HP, +20% Crit | Critical Strikes grant 5% Damage Amp for 5 seconds, stacking up to 4 times. |
| Sunfire Cape | Chain Vest + Giant's Belt | +20 Armor, +150 HP | Gain 8% max Health. Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Tactician's Cape | Spatula + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold after 10 seconds of combat. "...and a bit of Luck." |
| Tactician's Crown | Spatula + Spatula |  | Your team gains +1 max team size. 10% chance to drop 1 gold when you win combat. "...the Heart of a hero..." |
| Tactician's Shield | Frying Pan + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold when the holder dies. "Imbued with a Philosopher's wisdom..." |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | +150 HP, +20% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] (live counter) |
| Titan's Resolve | Chain Vest + Recurve Bow | +10% AS, +20 Armor | Gain 2% Attack Damage and 2% Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 10% Damage Amp and gain immunity to crowd control. |
| Void Staff | Recurve Bow + Tear of the Goddess | +35 AP, +15% AS | Damage from attacks and Abilities 30% Shred the target for 5 seconds. This effect does not stack. Shred: Reduce Magic Resist |
| Warmog's Armor | Giant's Belt + Giant's Belt | +500 HP | Gain 18% max Health. |

### Emblems (19)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Anima Emblem |  | +250 HP | The holder gains the Anima trait. |
| Arbiter Emblem | Spatula + Negatron Cloak | +30% AS, +20 Armor, +20 MR | The holder gains the Arbiter trait. |
| Bastion Emblem | Frying Pan + Chain Vest | +30 Armor, +30 MR | The holder gains the Bastion trait. After the first 10 seconds of combat, the holder gains 70% Attack Speed. |
| Brawler Emblem | Frying Pan + Giant's Belt | +250 HP | The holder gains the Brawler trait. Attacks deals 3% of the holder's max health as magic damage. |
| Challenger Emblem | Frying Pan + Recurve Bow | +30% AS | The holder gains the Challenger trait. On takedown, the holder's next 3 attacks each heal for 5% of max health. |
| Dark Star Emblem | Spatula + B.F. Sword | +18% AD, +18 AP | The holder gains the Dark Star trait. |
| Marauder Emblem | Frying Pan + B.F. Sword | +20% AD, +10% Omnivamp | The holder gains the Marauder trait and has a 33% chance to drop 1 gold on kill. Gold dropped this game: (live counter) |
| Meeple Emblem | Spatula + Chain Vest | +20 Armor, +20 MR | The holder gains the Meeple trait and 4% Damage Amp and 50 Health per set14AmpIcon. |
| N.O.V.A. Emblem | Spatula + Sparring Gloves | +200 HP, +20% Crit | The holder gains the N.O.V.A. trait. N.O.V.A. Strike: Gain 25% Attack Speed and 10 Armor and Magic Resist. Other allies gain 10% Attack Speed and 10 Armor and Magic Resist. |
| Primordian Emblem | Spatula + Giant's Belt | +450 HP | The holder gains the Primordian trait. |
| Psionic Emblem |  | +20% AD, +20 AP, +20% AS, +300 HP | The holder gains the Psionic trait. |
| Rogue Emblem | Frying Pan + Sparring Gloves | +20% Crit, +25% Omnivamp | The holder gains the Rogue trait. At 50% health, gain 25% Omnivamp and immunity to crowd control for 5 seconds. |
| Shepherd Emblem | Frying Pan + Tear of the Goddess |  | The holder gains the Shepherd trait. On cast, grant 8% of max Mana to the Bond of the Stars. |
| Sniper Emblem |  | +20% AS | The holder gains the Sniper trait and +1 Attack Range. |
| Space Groove Emblem | Spatula + Tear of the Goddess | +200 HP | The holder gains the Space Groove trait. On cast, the holder enters {{TFT17_SpaceGroove_TheGroove}} for 2.5 seconds. Can only occur once every 4 seconds. |
| Stargazer Emblem | Spatula + Needlessly Large Rod | +15% AD, +15 AP | The holder gains the Stargazer trait. |
| Timebreaker Emblem | Spatula + Recurve Bow | +15% AS, +200 HP | The holder gains the Timebreaker trait. |
| Vanguard Emblem | Frying Pan + Negatron Cloak | +20 Armor, +20 MR | The holder gains the Vanguard trait. Gain 2% Ability Power whenever an ally gains a shield. |
| Voyager Emblem | Frying Pan + Needlessly Large Rod | +20% AD, +20 AP | The holder gains the Voyager trait. Combat Start: The holder and allies within 1 hex in the same row gain a bonus based on the holder's role. - Tanks: 10 Armor and Magic Resist - Fighters/Assassins: 10% Omnivamp - Other Roles: 10% Attack Speed |

### Artifacts (Ornn) (53)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Aegis of Dawn |  | +50 Armor, +400 HP | Every 2.5 seconds, steal 3 Armor from enemies within 1-hex and heal (live counter)% of the holder's Armor. If Aegis of Dusk is also equipped, trigger this item's effect every 1.25 seconds instead. Healing increases based on Stage. |
| Aegis of Dusk |  | +70 MR, +400 HP | Every 2.5 seconds, steal 3 Magic Resist from enemies within 1 hex and deal (live counter)% of the holder's Magic Resist as magic damage. If Aegis of Dawn is also equipped, trigger this item's effect every 1.25 seconds instead. Damage increases based on Stage. |
| Ahri's Aura |  | +20 AP | The holder is orbited by 3 foxfires. Each one deals 50 (AP) magic damage and the orbit expands to hit the holder's current target. For every 20 Mana the holder spends, foxfires travel 5% faster for the rest of combat. |
| Blacksmith's Gloves |  | +200 HP, +30% Crit | Each round: Equip 2 random Ornn Artifacts. [Consumes 3 item slots.] (live counter) |
| Blighting Jewel |  | +60 AP | Dealing magic damage reduces the target's Magic Resist by 6. If their Magic Resist is 0, grant the holder 2 Mana instead. Ability damage can only trigger on each enemy once every 0.5 seconds. |
| Cappa Juice |  | +25% AD, +25 AP, +25% AS | The holder dons a Hat on each takedown. The holder gains 1% Attack Damage and Ability Power per Hat. On death lose 20% of all Hats. (Hats: (live counter)) |
| Corrupt Vampiric Scepter |  | +60% AS | Attacks deal an additional 60% Attack Damage AD as physical damage and heal the holder for the damage dealt. The holder cannot cast their Ability or gain Mana. |
| Dawncore |  | +15% AD, +15 AP | Reduce the holder's max Mana by 10. Subsequent spellcasts reduce max Mana by 10%, to a minimum of 10. |
| Death's Defiance |  | +25% AD, +15% AS, +40 Armor, +25% Omnivamp | 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] |
| Deathfire Grasp |  | +30 AP | Combat start: Blast the current target for 40% of their max Health as magic damage. Repeat this every 13 seconds. |
| Eternal Pact |  | +40 AP | Combat start: Form a pact with the highest Health allied champion, if they die, gain 10 Mana regen and 40% Ability Power. When the ally uses their ability, gain 15 Mana. Every 6 seconds, shield your ally for 200% of the holder's Ability Power. |
| Eternal Winter |  | +20 Armor, +200 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead (Cooldown: 15 seconds). [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Evelynn's Instinct |  | +10% AD, +10 AP, +40% AS, +15% Omnivamp | When switching targets, blink to the next target. Attacks and Abilities execute the holder's target below 12% of their Health. Kills grant the holder 50% Attack Speed decaying over 3 seconds. |
| Fishbones |  | +50% AD, +50% AS | The holder's attacks target random enemies. |
| Flickerblades |  | +10 AP, +20% AS | Attacks grant 5% stacking Attack Speed. Every 3 attacks also grant 2% Attack Damage and 2% Ability Power. |
| Forbidden Idol |  | +25 Armor, +25 MR, +200 HP | Shields have 50% of their value converted to max Health instead. |
| Gold Collector |  | +35% AD, +20% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 33% chance to drop goldCoins 1 gold. Gold Collected: (live counter)g [Unique - only 1 per champion] |
| Hellfire Hatchet |  | +20% AD, +150 HP, +20% Omnivamp | Attacks deal 3% of the holder's max Health as bonus physical damage. For every 1% missing Health, gain 1% Attack Speed. |
| Hullcrusher |  | +60 Armor, +60 MR | Combat Start: If there are no adjacent allies, gain 400 Health, 10% Attack Damage, and 10% Ability Power. |
| Infinity Force |  | +30% AD, +30 AP, +30% AS, +30 Armor, +30 MR, +300 HP | Tons of EVERYTHING! |
| Innervating Locket |  | +150 HP | The holder gains 1% of their total Mana whenever they're hit by an attack. Each cast restores 20% of the holder's max Health over 3 seconds. |
| Lesser Mirrored Persona |  | +10% AD, +10 AP, +10% AS, +10 Armor, +10 MR, +100 HP | Share 15% of the holder's bonus Attack Damage, Ability Power, Attack Speed, Armor, Magic Resist, and Health with other Mirrored Persona holders. Can't be Reforged Unique: one per champion |
| Lich Bane |  | +55 AP, +10% AS | The holder's first attack after each Ability cast deals (live counter) bonus magic damage. Damage increases based on Stage. |
| Lightshield Crest |  | +55 Armor, +55 MR | Every 3 seconds, Shields the lowest percent Health ally for 70% of the holder's combined Armor and Magic Resist for 5 seconds. On death grants this shield to all allies. |
| Luden's Tempest |  | +60% AD, +60 AP | 100% of overkill damage plus 100 is dealt as magic damage to the two enemies nearest to the target. |
| Manazane |  | +15% AD, +10 AP, +15% AS | After casting the first time in combat, gain 100 Mana over 5 seconds. [Unique - only 1 per champion] |
| Mending Echoes |  | +200 HP | Increase healing on the holder by 25%. When the holder gives or receives a heal, grant 40% of the heals value to the lowest health ally as well. |
| Mirrored Persona |  | +10% AD, +10 AP, +10% AS, +10 Armor, +10 MR, +100 HP | Every 6 player combats gain a lesser copy of Mirrored Persona ((live counter)/6). Share 15% of the holder's bonus Attack Damage, Ability Power, Attack Speed, Armor, Magic Resist, and Health with other Mirrored Persona holders. Can't be Reforged, Lesser copies do not produce copies. Unique: one per champion |
| Mittens |  | +65% AS | Shrinks the holder, granting them increased movement speed and immunity to Slow, Burn, and Wound. {{TFT_Keyword_Chill}} {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}} |
| Obsidian Cleaver |  | +150 HP | Damage dealt 30% Shreds and 30% Sunders enemies for 15 seconds. Your team gains 8% Attack Damage and 8 Ability Power. ​​[Support item] [Unique - only 1 per champion] Shred: Reduce Magic Resist Sunder: Reduce Armor |
| Prowler's Claw |  | +35% AD, +300 HP, +45% Crit | After killing a target, shed negative effects and dash to the farthest target within 4 hexes. The next 2 critical attacks deal 50% bonus Critical Strike Damage. |
| Randuin's Omen |  | +150 HP | Combat start: Grant Armor 30 Armor and MR 30 Magic Resistance to the holder and adjacent allies. ​​[Support item] |
| Rapid Firecannon |  | +65% AS | Gain +1 Attack Range, increased by 1 whenever the holder kills an enemy. |
| Seeker's Armguard |  | +25 AP, +10 Armor, +10 MR, +20% Omnivamp | Takedowns increase the holder's Armor, Magic Resist, and Ability Power by 20. |
| Shadow Puppet |  | +10% AS, +10 Armor, +10 MR | Spawn a clone that copies the holder's items. The clone has 60% max Health and deals 40% damage. [Unique - only 1 per champion] |
| Silvermere Dawn |  | +1.4 AD, +80 Armor, +80 MR | Grants immunity to Stuns and the holder's attacks Stun the target for 0.8 seconds. The holder's Attack Speed is locked at 0.5. |
| Sniper's Focus |  | +20% AD, +20 AP, +20% AS | Gain 40% Damage Amp against targets 4 or more hexes away. |
| Soraka's Miracle |  | +100 HP | The first 2 times the holder drops below 50% Health, a Miracle occurs, healing them for 20% of their max Health. If the holder survives player combat, gain 1 player Health for each Miracle this combat. |
| Spectral Cutlass |  | +40% AD, +300 HP, +20% Crit | Combat start: Teleport the holder to the mirrored hex on the enemy's side of the board. After 8 seconds, the holder returns to their original location. |
| Statikk Shiv |  | +15 AP, +50% AS | Every 3rd attack deals 30 + 50% of the holder's Ability Power as additional magic damage to 4 enemies. |
| Suspicious Trench Coat |  | +15% AS, +100 HP | Once per combat at 60% Health, the holder splits into 3 copies of themself each with 33% of their max Health. [Unique - only 1 per champion] |
| Talisman Of Ascension |  | +20% AD, +20 AP, +450 HP, +1.2 DamageAmp | After 22 seconds gain 100% max Health and 120% Damage Amp for the rest of combat. |
| The Indomitable |  | +20 Armor, +20 MR, +500 HP | The holder's Move Speed is drastically reduced. Gain 15% max Health, stun immunity, and pull the current target into melee range. |
| Thresh's Lantern |  | +35 Armor, +35 MR, +400 HP | After 6 seconds of combat, pull the leftmost benched unit onto the battlefield. While that unit lives, 20% of all damage the holder would take is redirected to them. Traits of champions flung onto the board do not become active |
| Titanic Hydra |  | +20% AD, +20% AS, +300 HP | Attacks deal 4% of the holder's max Health plus 6% of their Attack Damage as bonus physical damage to the target and adjacent enemies. |
| Trickster's Glass |  | +10% AS, +10 Armor, +10 MR, +15% Crit | Summon a clone with 70% base Health and +10% max Mana. You cannot equip items to the clone. The clone benefits from active traits [Unique - only 1 per champion] |
| Unending Despair |  | +40 Armor, +350 HP | Whenever a Shield on the holder breaks, 150% of that Shield's initial value is dealt to the nearest enemy as magic damage. |
| Varus's Obsession |  |  | Combat Start: Your strongest Tank becomes the Soulmate. For each second they are alive, the holder gains 3% stacking Attack Damage and Ability Power. The Soulmate heals for 20% of damage the holder deals. |
| Void Gauntlet |  | +300 HP | Combat start: Store 30% max Health and 2.5% more every second. On death, unleash the stored Health as magic damage split between enemies within 4-hexes. [Unique - only 1 per champion] |
| Wit's End |  | +25% AS, +20 Armor, +20 MR, +400 HP | Attacks deal (live counter) bonus magic damage. Heals the holder for 30% of all magic damage dealt. Damage increases based on Stage. |
| Yasuo's Bladework |  | +30% AD, +20% AS | Every 3 seconds, your next attack is a double attack. |
| Zeke's Bleak Herald |  | +10% AD, +150 HP | When combat begins, the holder reduces the Attack Speed of all allies within 1 hexes in the same row by AS 20%. The holder then gains AS 60% Attack Speed for each affected ally. |
| Zhonya's Paradox |  | +25 AP, +25 Armor, +25 MR | Once per combat at 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items (48)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Biomatter Preserver |  | +650 HP | Gain 5% max Health and deploy 3 Life Orbs. Every 8 seconds of combat, one drops restoring 18% of the holder's missing Health. At (4): If the holder is Psionic, they gain 30% increased healing from all sources. Recommended user: Gragas. |
| Drone Uplink |  |  | A drone repeats 25% of damage from the holder's attacks and Abilities to the same targets every 3 seconds. At (4): If the holder is Psionic, gain an additional mini-drone that repeats 30% of damage. Recommended users: Sona and Viktor. |
| Kayle's Radiant Exaltation |  | +50% AD, +50 AP, +30% AS | Behold, the righteous flame! |
| Malware Matrix |  | +15% AS, +10% Omnivamp | Attack Champion: Dealing physical damage to an enemy reduces the target's Armor by 2. Magic Champion: Dealing magic damage to an enemy reduces the target's Magic Resist by 2. At (4): If the holder is Psionic, every 3rd attack cleaves, dealing 100 (AD) physical damage to nearby enemies. Recommended users: Master Yi and Pyke. Ability Damage Cooldown: 1 seconds. |
| Radiant Adaptive Helm |  | +40 MR | Gain an additional 30% Mana from all sources. The wearer gains an additional bonus based on their Role: Tank/Fighter: Gain 80 Armor and Magic Resistance. Marksman/Caster: Gain 40% Attack Damage and Ability Power. |
| Radiant Archangel's Staff |  | +60 AP | Combat start: Gain 40% Ability Power every 5 seconds in combat. |
| Radiant Bloodthirster |  | +30% AD, +30 AP, +40 MR, +40% Omnivamp | Once per combat: At 40% Health, gain a 50% max Health Shield that lasts up to 5 seconds. |
| Radiant Blue Buff |  | +30% AD, +30 AP | Gain 20% additional Attack Damage and Ability Power from all sources. |
| Radiant Bramble Vest |  | +100 Armor | Gain 12% max health. Take 10% reduced damage from attacks. When struck by any attack, deal 200 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Radiant Crownguard |  | +40 AP, +40 Armor, +200 HP | Combat Start: Gain a 50% max Health Shield for 8 seconds. When the Shield expires, gain 50% Ability Power. |
| Radiant Deathblade |  | +100% AD | It glows in the presence of enemies. Or friends. Or anything alive, really. (live counter) |
| Radiant Dragon's Claw |  | +120 MR | Gain 28% max health. Every 2 seconds, heal 5% max Health. |
| Radiant Edge of Night |  | +30% AD, +30 AP, +30% AS, +40 Armor | At 60% Health, briefly become untargetable, shed negative effects, and heal all missing health. |
| Radiant Evenshroud |  | +40 MR, +500 HP | 30% Sunder enemies within 3 hexes. Gain 50 Armor and Magic Resist for the first 20 seconds of combat. Sunder: Reduce Armor |
| Radiant Field |  | +400 HP | Gain 20% max health. Every second, deal magic damage in a 1-hex radius equal to 3% of the holder's max Health. Radius increases every 6 seconds. Recommended Roles: Attack or Magic Tank |
| Radiant Gargoyle Stoneplate |  | +50 Armor, +50 MR, +300 HP | Gain 20 Armor and 20 Magic Resist for each enemy targeting the holder. |
| Radiant Giant Slayer |  | +30% AD, +30 AP, +30% AS, +30% DamageAmp | Gain 30% additional Damage Amp against Tanks. |
| Radiant Guinsoo's Rageblade |  | +20 AP, +25% AS | Gain 16% stacking Attack Speed every second. |
| Radiant Hand of Justice |  | +40% Crit | Gain 2 effects:30% Attack Damage and 30% Ability Power.24% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Radiant Hextech Gunblade |  | +40% AD, +40 AP, +35 Omnivamp, +30% Omnivamp | Heal the lowest percent Health ally for 40% of damage dealt. Ally Healing: (live counter) |
| Radiant Infinity Edge |  | +75% AD, +75% Crit | Gain Precision. {{TFT_Keyword_Precision}} |
| Radiant Ionic Spark |  | +30 AP, +70 MR, +500 HP | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 300% of the Mana spent. [Direct damage item] Shred: Reduce Magic Resist |
| Radiant Jeweled Gauntlet |  | +75 AP, +75% Crit | Gain Precision. {{TFT_Keyword_Precision}} |
| Radiant Kraken's Fury |  | +20% AD, +20% AS, +40 MR | Attacks grant 7% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 30% Attack Speed. |
| Radiant Last Whisper |  | +45% AD, +40% AS, +40% Crit | Damage from attacks and Abilities 30% Sunder the target for the rest of combat. This effect does not stack. Sunder: Reduce Armor |
| Radiant Lucky Item Chest |  |  | Use on a champion to open an armory of Radiant items especially suited for them. These items are based on the champion's recommended items. |
| Radiant Morellonomicon |  | +35 AP, +300 HP | Attacks and Abilities deal 2% Burn and 33% Wound to enemies for 10 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Radiant Nashor's Tooth |  | +30 AP, +20% AS, +300 HP, +40% Crit | Attacks grant 4 bonus Mana, increased to 6 if they critically strike. |
| Radiant Protector's Vow |  | +50 Armor, +50 MR | Combat Start: Gain 40 Mana. At 40% Health, gain 30 Mana and a Shield equal to 50% max Health. |
| Radiant Quicksilver |  | +40% AS, +40 MR, +40% Crit | Combat Start: Gain immunity to crowd control for 45 seconds. Gain 6% stacking Attack Speed every second. |
| Radiant Rabadon's Deathcap |  | +100 AP | It's witnessed - and unleashed - miracles and calamities both. (live counter) |
| Radiant Red Buff |  | +80% AS | Attacks and Abilities 2% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Radiant Spear of Shojin |  | +30% AD, +30 AP | Attacks grant 10 bonus Mana. |
| Radiant Spirit Visage |  | +700 HP | Regenerate 4% of missing Health each second. |
| Radiant Steadfast Heart |  | +40 Armor, +600 HP, +40% Crit | Gain 10% durability. While above 50% Health, instead gain 30% Durability. (live counter) |
| Radiant Sterak's Gage |  | +90% AD, +600 HP | At 60% Health, gain a Shield equal to 80% of the wearer's maximum Health that rapidly decays over 6 seconds. |
| Radiant Striker's Flail |  | +20% AS, +300 HP, +40% Crit | Critical Strikes grant 10% Damage Amp for 5 seconds, stacking up to 4 times. |
| Radiant Sunfire Cape |  | +40 Armor, +300 HP | Gain 16% max Health. Every 2 seconds, deal 2% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: reduces healing received |
| Radiant Thief's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] (live counter) |
| Radiant Titan's Resolve |  | +20% AS, +40 Armor | Gain 4% Attack Damage and 4% Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 20% Damage Amp and gain immunity to crowd control. |
| Radiant Void Staff |  | +75 AP, +30% AS | Damage from attacks and Abilities 30% Shred the target for the rest of combat. This effect does not stack. Shred: Reduce Magic Resist |
| Radiant Warmog's Armor |  | +1200 HP | Gain 36% max Health. |
| Semiconductor |  | +300 HP | Every 4 attacks and every 12 times being attacked, zap the 3 nearest enemies, dealing 7.5% of enemy Health as magic damage. Recommended users: Gragas and Master Yi. |
| Solar Eclipse |  | +800 HP | Gain 18% max Health. Every second, deal magic damage in a 2 hex radius equal to 5% of the holder's max Health, and gain max Health equal to 20% of damage dealt. Radius increases every 6 seconds. Recommended Roles: Attack or Magic Tank |
| Sympathetic Implant |  | +20% AS | Every 5 seconds, gain: Magic Champions: 1 Mana Regen. Attack Champions: 15% Attack Speed. At (4): If the holder is Psionic, their abilities deal 25% of their ability damage as true damage instead. Recommended users: Sona and Viktor. |
| Target-Lock Optics |  | +30% AD, +30 AP, +30% AS | When the holder damages each enemy for the first time each combat, gain: Attack Champions: 9% Attack Damage Magic Champions: 7% Ability Power At (4): If the holder is Psionic, they heal 30% of their max Health whenever their target dies. Recommended users: Pyke and Master Yi |
| Virtue of the Martyr |  | +150 HP | Every 5 seconds, heal your team for 7% of their max Health. When the holder dies, the healing increases to 14% max Health for 2 extra heals. Healing: (live counter) ​​[Support item] |
| Zz'Rot Portal |  | +150 HP | Summon a large Voidspawn. Its strength increases with each Stage. ​​[Support item] [Unique - only 1 per champion] |

### Support items (12)

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
| Support Frame |  |  | !!DO NOT PICK - NOT YET IMPLEMENTED!! 4 Range, Hybrid Caster Specialized in debilitating enemies and empowering allies. |
| The Eternal Flame |  | +150 HP, +8% DamageAmp | While the holder is alive, all enemies are 33% Wounded and all allies gain 8% Damage Amp. This effect refreshes every 5 seconds. ​​[Support item] Wound: Reduces healing received |
| Unstable Treasure Chest |  | +150 HP | When the holder dies, the 4 highest percent health allies gain a temporary completed item. ​​[Support item] |
| Zeke's Herald |  | +150 HP | Combat start: Grant AS 30% Attack Speed to the holder and allies within 2 hexes in the same row. ​​[Support item] |

### Set-specific and other items (101)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Aim-Assistant |  |  | Deal 5% bonus damage per hex distance from the target. |
| All Done Stop transforming, and allow me to star up |  |  |  |
| Animapocalypse |  | +30% AD, +30 AP | Store 20% of damage dealt. Every 6 seconds and on death, deal the stored damage as true damage to all enemies, reduced by 5% for each hex they are from the epicenter. Recommended Roles: Attack or Magic Caster |
| Anomaly |  |  | Evolve the holder, granting an effect based on their role. Tank: Gain 1100 health and grow in size. Marksman: Gain 75% Attack Speed. On cast, knock back their target if within 3 hexes of them. Fighter/Assassin: Gain 35 Ability Power & 35% Attack Damage. Triple it when below 85% Health. Caster: For every 50 Mana spent, throw a 2-hex fireball that deals 5% max Health true damage, 33% Wounds, and 1% Burns for 5 seconds. Specialist: Gain 6 stars that orbit this champion. Each star deals 70 magic damage and the orbit expands to hit the champion's current target. |
| AP Rounds |  |  | Attacks and Abilities ignore 30% of enemy Armor |
| AP Rounds+ |  |  | Attacks and Abilities ignore 60% of enemy Armor |
| Assault Frame |  |  | 4 Range, Attack Marksman Specialized in dealing damage to foes. |
| Battle Bunny Crossbow |  | +20% AD, +25% AS, +35% Crit | Gain Precision. Every 4 critical strikes, fire 5 piercing arrows that can critically strike towards the target. Each deals 80% AD physical damage. {{TFT_Keyword_Precision}} Max 3 stacks per Ability cast. Recommended Roles: Attack Marksman or Fighter |
| Biomatter Preserver |  | +250 HP | Gain 5% max Health and deploy 3 Life Orbs. Every 8 seconds of combat, one drops restoring 18% of the holder's missing Health. Recommended user: Gragas. |
| Blast Radius |  |  | Secondary blast radius is increased by 1 hex, dealing 50% less for each additional hex. |
| Blast Radius+ |  |  | Secondary blast radius is increased by 2 hexes, dealing 30% less for each additional hex. |
| Blast Radius++ |  |  | Secondary blast radius is increased by 3 hexes, dealing 30% less for each additional hex. |
| Broken Prototype |  | +100 HP | Combat start: Gain a 15% max Health shield. |
| Buckshot |  |  | Increase the number of Attack projectiles by 2. Spread increased by 20%. |
| Buckshot+ |  |  | Increase the number of Attack projectiles by 4. Spread increased by 30%. |
| Buckshot++ |  |  | Increase the number of Attack projectiles by 6. Spread increased by 40%. |
| Bunny Prime Ballista |  | +30% AD, +50% AS, +50% Crit | Gain Precision. Every 3 critical strikes, fire 6 piercing arrows that can critically strike towards the target. Each deals 100% AD physical damage. On takedown, fire a burst of 12 arrows from the target. {{TFT_Keyword_Precision}} Max 3 stacks per Ability cast. Recommended Roles: Attack Marksman or Fighter |
| Challenger Mode |  |  | Miss Fortune gains the Challenger trait. |
| Changing Glamour |  |  | Use this on the Changeling to transform them into a new champion! Refreshes each round |
| Choke |  |  | Reduce projectile spread by 75%. |
| Completed Item Anvil |  |  | Gain a Completed Item Anvil |
| Conduit Mode |  |  | Miss Fortune gains the Conduit trait. |
| Coolant |  |  | Mana Cost reduced by 10 |
| Coolant+ |  |  | Mana Cost reduced by 20 |
| Crown of Demacia |  | +30% AS, +300 HP | If the holder is a Tank, regenerate 3-21% maximum Health every 2 seconds (based on stage). Otherwise, gain 10% AD Attack Damage and AP Ability Power every 2 seconds instead. If the holder of this item dies, you instantly lose the fight. You can remove this item by benching the holder. |
| Cyclonic Slicers |  | +20% AD, +20 AP, +15 Armor, +15 MR | Gain 3 orbiting razors. Razors deal [Damage: unknown] magic damage, and 30% Shred and Sunder enemies for 4 seconds. Deal 10% increased damage to Shredded or Sundered enemies. Damage increases based on Stage. Recommended Roles: Attack or Magic Fighter |
| Deep Freeze |  | +30 Armor, +30 MR, +550 HP | The first time you fall below 90%, 60%, 40%, and 20% health, gain a 20% max health shield for 4 seconds. When this happens, release a frost nova that stuns enemies within 2 hexes for 2 seconds and deals magic damage equal to 15% of their max Health. The radius increases by 1 each nova. Recommended Roles: Attack or Magic Tank |
| Determined Investor |  | +150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and goldCoins 5 gold. |
| Diamond Hands |  | +25 AP, +200 HP | Once per combat: At 50% Health, become invulnerable for 1.5 seconds and grant goldCoins 1 gold. |
| Double Tap |  |  | 35% chance to attack twice. |
| Drone Uplink |  |  | A drone repeats 20% of damage from the holder's attacks and Abilities to the same targets every 3 seconds. Recommended users: Sona and Viktor. |
| Echoing Batblades |  | +30% AD, +20% AS | Every 3 attacks, fire 10 batblades split between the 3 nearest enemies. Each blade deals 50% AD physical damage. Recommended Role: Attack Marksman |
| Emergency Shielding |  |  | At 40% Health, gain a 50% max Health shield that decays over 2.5 seconds. |
| Emergency Shielding+ |  |  | At 40% Health, gain a 75% max Health shield that decays over 4 seconds. |
| Evolved Embershot |  | +40% AD, +45 AP | For every 15 mana spent, fire a projectile at a nearby enemy dealing [ProjectileDamage: unknown] + 50% of the holder's Ability Power magic damage, and applying 1% Burn and 33% Wound to enemies for 10 seconds. Each projectile leaves a lingering field that deals [FieldDamagePerSecond: unknown] magic damage to enemies each second. Damage increases based on Stage. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received Recommended Roles: Magic Caster |
| Fission |  |  | +2 Mana Regen and 10% Attack Damage |
| Fission+ |  |  | +3 Mana Regen and 20% Attack Damage |
| Fission++ |  |  | +5 Mana Regen and 30% Attack Damage |
| Fragmentation Rounds |  |  | Attacks send 2 fragments to nearby enemies on hit that deal 15% damage. |
| Fragmentation Rounds+ |  |  | Attacks send 3 fragments to nearby enemies on hit that deal 20% damage. |
| Gambler's Blade |  | +55% AS | Grant AS 1% bonus Attack Speed per goldCoins gold in your bank (up to goldCoins 30 gold). Each attack has a 6% chance to drop goldCoins 1 gold. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Gold |  |  | (live counter) Gold |
| Grav Booster |  |  | On takedown, dash to the next target and attack 40% faster for 2 attacks. |
| Grav Booster+ |  |  | On takedown, dash to the next target and attack 40% faster for 3 attacks. |
| Guiding Hex |  | +15 AP | Every 3 seconds, fire a bolt at a random enemy in Attack Range, dealing [ProjectileDamage: unknown] magic damage. Aurora Bonus: If this item is equipped on Aurora, she gains 12 Ability Power when the bolt hits. Damage increases based on Stage. |
| Heartseeker |  |  | +10% Crit Chance and +5% Crit Damage |
| Heartseeker+ |  |  | +25% Crit Chance and +10% Crit Damage |
| Heartseeker++ |  |  | +40% Crit Chance and +18% Crit Damage |
| Heavy Plating |  |  | Gain 300 Base Health, 20 Armor, and 20 Magic Resistance. |
| Iceblast Armor |  | +30 Armor, +30 MR, +250 HP | The first time you fall below 70% and 35% health, gain a 20% max health shield for 4 seconds and release a frost nova that stuns enemies within 3 hexes for 1.5 seconds. Recommended Roles: Attack or Magic Tank |
| Laser Ballistics |  |  | Attacks travel an additional hex and pierce enemies, reducing damage by 60% per target pierced. |
| Laser Ballistics+ |  |  | Attacks travel an additional 2 hexes and pierce enemies, reducing damage by 60% per target pierced. |
| LaserBallistics++ |  |  | Attacks travel an additional 3 hexes and pierce enemies, reducing damage by 60% per target pierced. |
| Latent Explosion |  |  | Store 15% of damage dealt to enemies. On takedown, enemies release the stored damage as physical damage in a two-hex radius. |
| Leaky Prototype |  | +15 AP | Every 5 seconds, this item loses 3 Ability Power and gains 1 Mana Regen. |
| Leeching Implants |  |  | +10% Omnivamp and 10% Attack Damage |
| Leeching Implants+ |  |  | +15% Omnivamp and 20% Attack Damage |
| Leonine Lamentation |  | +40 Armor, +40 MR, +400 HP | Every 15% max health lost, fire crescent projectiles in 3 lines dealing physical damage equal to 50% of the holder's Armor and Magic Resist. Steal 10 Armor and Magic Resist from all enemies hit. Recommended Roles: Attack or Magic Tank |
| Lioness's Lament |  | +25 Armor, +25 MR, +200 HP | Every 15% max health lost, fire crescent projectiles in a line dealing physical damage equal to 50% of the holder's Armor and Magic Resist. Steal 3 Armor and Magic Resist from all enemies hit. Recommended Roles: Attack or Magic Tank |
| Malware Matrix |  | +15% AS, +10% Omnivamp | Attack Champion: Dealing physical damage to an enemy reduces the target's Armor by 2. Magic Champion: Dealing magic damage to an enemy reduces the target's Magic Resist by 2. Recommended users: Master Yi and Pyke. Ability Damage Cooldown: 0.75 seconds. |
| Marksman Frame |  |  | 25% chance to attack twice. |
| Meltthrough |  |  | Every second, reduce the Armor of enemies within 2-hexes by 4, doubled for adjacent enemies. |
| Mogul's Mail |  | +300 HP | Grants 1 Armor, 1 Magic Resist, and 5 Health when taking damage, stacking up to 35 times. At full stacks, grant goldCoins 1 gold and continue gaining goldCoins 1 gold every 11 seconds. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Nanomachines |  |  | Restore 3% max Health each second. |
| Needlessly Big Gem |  | +150 HP | Your team gains 5% Damage Amp. Each second the holder is alive, your team gains an additional 1% Damage Amp, up to a total of 20%. ​​[Support item] |
| Omniweapon |  | +100% AD, +100 AP, +100% AS, +100 Armor, +100 MR, +10000 HP, +75% Crit, +100% Omnivamp | The pinnacle of Anima technology. |
| OwO Blaster |  | +35% AD, +75% AS | Every attack, fire 9 laser blasts random targets in range, dealing 40% of the holder's Attack Damage as physical damage. Recommended Roles: Attack Marksman |
| Power Frame |  |  | Gain Precision and increase Ability damage by 5%. {{TFT_Keyword_Precision}} |
| Precision Scope |  |  | +1 Range & 12% Attack Damage |
| Precision Scope+ |  |  | +2 Range & 24% Attack Damage |
| Precision Scope++ |  |  | +3 Range & 36% Attack Damage |
| Reactive Armor |  |  | Taking damage from enemies grants 4 bonus Armor and Magic Resistances, up to 50. |
| Reinforced Frame |  |  | !!DO NOT PICK - NOT YET IMPLEMENTED!! 1 Range, Attack Tank Specialized in absorbing damage and protecting allies. |
| Replicator Mode |  |  | Miss Fortune gains the Replicator trait. |
| Rev-Up |  |  | Gain 8% Attack Speed for each attack against the same target, up to 80%. |
| Rev-Up+ |  |  | Gain 15% Attack Speed for each attack against the same target, up to 150%. |
| Ripper Bullets |  |  | Attacks reduce enemy Armor and Magic Resistance by 1. |
| Ripper Bullets+ |  |  | Attacks reduce enemy Armor and Magic Resistance by 2. |
| Rocket Swarm |  | +15% AD | Every 30 Mana spent, fire a barrage of 4 rockets at the target, each dealing 30% AD physical damage. Jinx Bonus: If this item is equipped on Jinx, instead replace all ability bullets with rockets that deal 40% increased damage. |
| Savage Slicer |  | +10% AD, +10% Omnivamp | Attacks hit the target and all adjacent enemies for 30% of Basic Attack Damage. Omnivamp applies 150% to this damage. Briar Bonus: If this item is equipped on Briar, on cast this effect happens 15 times. |
| Searing Shortbow |  | +15% AD, +45 AP | For every 20 mana spent, fire a projectile at a nearby enemy, dealing [ProjectileDamage: unknown] + 75% of the holder's Ability Power magic damage and applying 1% Burn and 33% Wound to enemies for 3 seconds. Damage increases based on Stage. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received Recommended Role: Magic Caster |
| Semiconductor |  | +200 HP | Every 4 attacks and every 8 times being attacked, zap the 3 nearest enemies, dealing 7% of enemy Health as magic damage. Recommended users: Gragas and Master Yi. |
| Sentinel Swarm |  |  | Every 3 seconds, summon a powerful Sentinel equipped with Completed, Artifact, and Radiant items. |
| Sheer Mass |  |  | Gain 25% max Health and size. |
| Shockwave |  |  | Combat Start: Unleash a shockwave forward that deals 15% of Graves' max Health as magic damage and stun enemies hit for 2 seconds. |
| Sparking Prototype |  | +10% AD | Every 4 seconds, gain 30% Attack Speed for 4 seconds. |
| Striker Selector |  |  | Use this on a N.O.V.A. champion to activate their Strike effect when the Power Surge occurs. |
| Sympathetic Detonation |  |  | A second one-hex radius explosion occurs on the nearest enemy, dealing 30% damage. |
| Sympathetic Implant |  | +15% AS | Every 5 seconds, gain: Magic Champions: 1 Mana Regen. Attack Champions: 15% Attack Speed. Recommended users: Sona and Viktor. |
| Tankbuster |  |  | Gain 15% Damage Amp against Tanks. |
| Target-Lock Optics |  | +10% AD, +10 AP, +20% AS | When the holder damages each enemy for the first time each combat, gain: Attack Champions: 9% Attack Damage Magic Champions: 7% Ability Power Recommended users: Pyke and Master Yi |
| Tentacle Slam |  | +150 HP, +10% Durability | Every 5 seconds, summon a tentacle that slams down on the target, dealing [SlamDamage: unknown] physical damage and healing the holder for 12% max health. Illaoi Bonus: If this item is equipped on Illaoi, an additional tentacle slams targets drained by her ability at 35% effectiveness. Damage increases based on Stage. |
| The Annihilator |  | +20% AD, +25 AP | Store 25% of damage dealt. On death or after 16 seconds in combat, deal the stored damage as true damage to all enemies, reduced by 15% for each hex they are away from the epicenter. Recommended Roles: Attack or Magic Caster |
| Triple Tap |  |  | 18% chance to attack thrice. |
| Tune-Up |  |  | Does nothing, but it's free! |
| Unceasing Cyclone |  | +30% AD, +30 AP, +40 Armor, +40 MR | Gain 6 orbiting razors. Razors deal [Damage: unknown] true damage, and 30% Shred and Sunder enemies for the rest of combat. Deal 30% increased damage to Shredded or Sundered enemies. Damage increases based on Stage. Recommended Roles: Attack or Magic Fighter |
| UwU Blaster |  | +30% AD, +45% AS | Every attack, fire 3 laser blasts random targets in range, dealing 40% of the holder's Attack Damage as physical damage. Recommended Role: Attack Marksman |
| Vayne's Chromablades |  | +30% AD, +50% AS | Every 5 attacks, fire 30 chromablades split between all enemies. Each blade deals 50% AD physical damage. Recommended Role: Attack Marksman |
| Void Coefficient |  |  | Each cast reduces Mana Cost by 15% (to a minimum of 10) |
| Zephyr |  | +150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. Your team gains 2% Attack Speed. [Support item] [Ignores crowd control immunity.] [Unique - only 1 per champion] (live counter) |
| Zz'Rot Portal |  | +10% AS, +150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. The Voidspawn has massively reduced effectiveness when created by a summoned unit. Taunt: enemies that are able and in range must attack the taunter |

### Consumables (51)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Blessing: Prosperity |  |  | Permanently grant a champion 80 Health, which increases by 4 Health for each interest you earn. |
| Blessing: Prosperity + |  |  | Permanently grant a champion 100 Health, which increases by 8 Health for each interest you earn. |
| Blessing: Size |  |  | Permanently increase a champion's Health by 350 and size by 20%. [Consumable - This item disappears when used.] |
| Blessing: Speed |  |  | Permanently increase a champion's Attack Speed by 20% and decrease their size by 20%. [Consumable - This item disappears when used.] |
| Blessing: Wealth |  |  | Permanently grant a champion 8% Attack Damage and Ability Power, and a 12% chance to generate 1 gold on takedown. |
| Champion Duplicator |  |  | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Command Mod: Analyze |  |  | Command a unit to grant you 1 XP every 3 seconds it is alive during player combat. (Good on backliners) |
| Command Mod: Brace |  |  | Command a unit to gain a 20% Health shield for 8 seconds at the start of combat. (Good on tanks) |
| Command Mod: Disguise |  |  | Command a unit to become untargetable until all other allies are dead. |
| Command Mod: Disrupt |  |  | Command a unit to Stun enemies within 3 hexes for 1 second when it dies. (Good on tanks) |
| Command Mod: Doublestrike |  |  | Command a unit to have a 25% chance to attack twice when attacking. (Good on carries) |
| Command Mod: Exhaust |  |  | Command a unit's attacks to Burn, Wound, and Chill. Gain 8% Attack Speed. (Good on anyone) |
| Command Mod: Fabricate |  |  | Command a unit to gain a temporary recommended item at the start of combat. (Good on anyone) |
| Command Mod: Optimize |  |  | Command a unit to reduce their Mana cost by 15%. (Good on anyone with Mana) |
| Command Mod: Transmute |  |  | Command a unit to drop 2 gold each time it kills an enemy during player combat. (Good on carries) |
| Golden Item Remover |  |  | Use on a champion to unequip all items as many times as you want. Converts all future Magnetic Removers to Reforgers. |
| Gwen's Scissors |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Gwen's Scissors (2 uses left!) |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Gwen's Scissors (3 uses left!) |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Gwen's Scissors (4 uses left!) |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Gwen's Scissors (5 uses left!) |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Hunger For Power Anomaly |  |  | Use on a unit to grant them the Hunger For Power anomaly. Sell the unit to regain this consumable. Hunger For Power Combat start: Consume an adjacent ally Champion and gain 75% of their Health and Attack Damage. [Consumable - This item disappears when used.] |
| Lesser Champion Duplicator |  |  | Use on a 3-cost champion or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Loaded Dice |  |  | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Lucky Item Chest |  |  | Use on a champion to open an armory of items especially suited for them. These items are based on the champion's recommended items. |
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
| Masterwork Upgrade |  |  | Use on a champion to choose a Radiant version of that champion's craftable completed items. Upgrade the item you choose to its Radiant equivalent and unequip it. |
| Mecha-Former |  |  | Use this on a Mecha unit to change their form. Transformed Mechas take up two team slots and count twice for the Mecha trait. |
| Pocket Recombobulator |  |  | Use on a 1 or 2 cost champion to upgrade them to a unit of 1 higher cost. Not for use on living, mechanical, experimental, imaginary, or reanimated creatures . Keep out of reach of Yordles. |
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
| Salvager |  |  | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| Tiny Champion Duplicator |  |  | Use on a 1-cost champion to create a 1-star copy of them. |
| Training Dummy |  |  | Use on a champion to spawn a movable Training Dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |

## Augments (273)

Tier comes from the augment icon file name (I/1 = Silver, II/2 = Gold, III/3 = Prismatic); "unknown" where the name carries no marker. The list is the set's augment pool as stored in the source data and can include entries that were disabled or rotated during the set.

| Name | Tier | Effect |
|---|---|---|
| A Magic Roll | Gold | Roll 3 dice. Gain rewards based on their total. Reward: (live counter) |
| Advanced Loan | Gold | Gain 20 Gold. Your next augment is one tier lower. |
| Advanced Loan+ | Gold | Gain 33 Gold. Your next augment is one tier lower. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 17 gold. |
| After @LargeQuestRounds@ player combats, gain +1 to all non-unique traits. | unknown |  |
| After @MedQuestRounds@ player combats, gain a completed item anvil and @MedQuestGold@ gold. | unknown |  |
| Ahri's Boon | Gold | Your team gains 2% Attack Damage and Ability Power per level. Gain 2 XP each round for the rest of the game. |
| An Exalted Adventure | Prismatic | Gain three 2-cost champions. If you 3-star two of them, gain an orb filled with loot. Gain a Lesser Champion Duplicator at the start of next 2 stages. Champions: (live counter), (live counter), (live counter) |
| Anima Commander | Gold | Gain a Briar, a Jinx, and an Illaoi. Increase your current and max player Health by 5. |
| Apotheotic Forge | Gold | Choose 1 of 4 Divine Artifacts. Artifacts are more powerful items with a unique effect. |
| Arcane Viktor-y | Gold | After 8 seconds of combat, stun the enemy team for 1.5 seconds. Repeat this after 18 seconds of combat. TFT Macao Open, 2024 |
| Ascension | Gold | After 12 seconds of combat, your team gains 35% Damage Amp. |
| At What Cost | Prismatic | Immediately go to level 6 and gain 8 XP. You don't get to choose your future augments. |
| Augmented Power | Silver | Your next augment is one tier higher. |
| Aura Farming | Gold | Gain a 2-star 5-cost champion holding a recommended item. You cannot field it until Stage 4-3. |
| Aurelion Sol's Boon | Gold | Choose 1 of 3 quests to prove yourself to Aurelion Sol. Chosen Quest: (live counter) |
| Backline Blueprint | Gold | Gain a 3-cost non-Tank champion and an Emblem that matches their last-listed trait. |
| Backup Bows | Silver | Gain a Recurve Bow. After your team attacks 1000 times, gain 2 more. Attacks: (live counter) |
| Band of Thieves | Silver | Gain 1 Thief's Gloves. |
| Band of Thieves II | Prismatic | Gain 2 Thief's Gloves. After 6 player combats, gain another. |
| Band of Thieves II+ | Prismatic | Gain 2 Thief's Gloves. After 5 player combats, gain another. |
| Band of Thieves II++ | Prismatic | Gain 2 Thief's Gloves. After 3 player combats, gain another. |
| Baron's Lair | Prismatic | Gain a random Tank item. After 8 seconds of combat, your team gains 5% bonus Attack Damage and Ability Power every second for the rest of combat. TFT Paris Open, 2025 |
| Belt Overflow | Prismatic | Gain 4 Giant's Belts. Your Giant's Belts grant +85 bonus Health. |
| Best Friends I | Silver | Allies isolated in pairs gain 10% Attack Speed and 9 Armor and Magic Resist at the start of combat. |
| Best Friends II | Gold | Allies isolated in pairs gain 15% Attack Speed and 14 Armor and Magic Resist at the start of combat. |
| Big Grab Bag | Gold | Gain 3 random components, 2 gold, and 1 Reforger. Reforgers allow you to remake any item. |
| Birthday Present | Prismatic | Gain a 2-star champion and 1 gold every time you level up. The champion's cost tier is your level minus 4 (max: 5-cost). |
| Birthday Reunion | Gold | Gain 1 gold. At Level 5, gain a random 2-star 2-cost champion. At Level 7, gain a Thief's Gloves. At Level 9, gain a random 2-star 5-cost champion. Inkborn Fables Tactician's Crown, 2024 |
| Blood Offering | Gold | Gain a Bloodthirster. Combat start: Allies holding Bloodthirsters lose 15% Health but gain a 20% Health shield and 12% Attack Damage and Ability Power. |
| Blood Price | Gold | Gain 25 Tactician Health. Your rightmost shop slot costs Tactician Health instead of gold. |
| Bodyguard Training | Gold | Allies gain 15 Armor and Magic Resist, increased by 2 per player level. Gizmos & Gadgets Championship, 2022 |
| Bonk! | Silver | Gain a Nasus. Your strongest Nasus becomes an Attack Fighter with a single target Ability that gets stronger with each kill. |
| Boon of Resurrection | Gold | The first time you would be eliminated, instead remain at 1 Health |
| Boon of the Scrapper | Gold | Combat start: Up to 3 components temporarily turn into completed items. Gain 3 removers and a salvager. Salvagers allow you to break completed items into components |
| Booster Pack | Gold | Gain 12 gold of random champions. (At least one 3-cost guaranteed!) (Each champion has a small chance of being holographic!) |
| Booster Pack+ | Gold | Gain 18 gold of random champions. (At least one 4-cost guaranteed!) (Each champion has a small chance of being holographic!) |
| Booster Pack++ | Gold | Gain 26 gold of random champions. (At least one 5-cost guaranteed!) (Each champion has a small chance of being holographic!) |
| Boxing Lessons | Silver | Your team gains 30 Health for each ally that starts combat in the front row. |
| Branching Out | Silver | Gain a random Emblem. |
| Branching Out+ | Silver | Gain a random Emblem and a Reforger. Reforgers allow you to remake any item. |
| Bronze For Life I | Gold | Your team gains 2% Damage Amp for each Bronze-tier trait. Damage Amp Bonus: (live counter)% |
| Bronze For Life II | Prismatic | Your team gains 2% Damage Amp and 4 Armor and Magic Resist for each Bronze-tier trait. Damage Amp: (live counter)% Armor and Magic Resist: (live counter) |
| Build a Bud | Prismatic | Gain a random 3-star 1-cost champion. Gain 10 gold. |
| Buried Treasures | Prismatic | Gain a random item component now and at the start of the next 5 rounds. |
| Calculated Loss | Gold | After losing your combat, gain 2 gold and a free Shop reroll. |
| Call to Chaos | Prismatic | Gain a powerful and random reward. Reward: (live counter) |
| Care Package | Gold | Now and at the start of the next 2 Stages, receive a care package with loot and 1 gold. |
| Caretaker's Ally | Silver | Gain a random 2-cost champion now. Gain the same one again every time you level up. Champion: (live counter) |
| Carve a Path | Silver | Gain a B.F. Sword. After your team deals 65000 physical damage, gain 2 more. Damage dealt: (live counter) |
| Charge Transfer I | Silver | Your team gains 7% Damage Amp. 50% of overkill damage is dealt to the nearest enemy as magic damage. |
| Charge Transfer II | Gold | Your team gains 10% Damage Amp. 80% of overkill damage is dealt to the nearest enemy as magic damage. |
| Clear Mind | Gold | If there are no champions on your bench at the end of player combat, gain 3 XP. |
| Climb The Ladder I | Silver | Each time an ally dies, allies that share at least one trait with them gain 3.5% Ability Power, 3.5% Attack Damage, 3.5 Armor, and 3.5 Magic Resist. |
| Climb The Ladder II | Gold | Each time an ally dies, allies that share at least one trait with them gain 7% Ability Power, 7% Attack Damage, 6 Armor, and 6 Magic Resist. |
| Clockwork Accelerator | Gold | Your team gains 10% Attack Speed every 3 seconds in combat. |
| Cluttered Mind | Gold | Gain 4 random 1-cost champions now. If your bench is full at the end of player combat, gain 3 XP. |
| Cognitive Overload | Gold | Gain 7 gold, 1 2-star 1-cost champion, 1 2-cost champion, 2 3-cost champions, 1 XP, and 1 Shop reroll. |
| Cognitive Tax | Silver | Gain 8 gold and 1 XP. |
| Cognitive Tax+ | Silver | Gain 12 gold and 3 XP. |
| Comeback Story | Prismatic | Your team gains 4 Health and 0.4% Attack Speed per missing player Health. (live counter) |
| Commerce Core | Prismatic | Gain 16 free Shop rerolls now and 3 every round for the rest of the game. |
| Component Heist | Prismatic | After 6 player combats, gain 1 of every item component. Gain a random component now. (live counter) (live counter) |
| Concentration | Gold | Conduit Abilities last 25% to 50% longer, depending on the Ability. Gain a Zoe and a Mordekaiser. |
| Construct a Companion | Gold | The next 1-cost champion you buy is 3-star. Gain 2 gold. |
| Continuous Conjuration | Silver | Gain a Needlessly Large Rod. After your team deals 42000 magic damage, gain 2 more. Damage dealt: (live counter) |
| Contract Killer | Gold | Gain a Pyke. Your strongest Pyke becomes an Attack Fighter, generates gold on team takedown, and gains a reset on-kill Ability. |
| Corrosion | Silver | Enemies in the first two rows lose 4 Armor and Magic Resist every 2 seconds. |
| Cosmic Restart | Gold | Call upon the Gods to sell all units on your board and bench. Gain 2 random Emblems and 15 free Shop rerolls. |
| Crafted Crafting | Silver | Whenever you craft a completed item, gain 2 rerolls. |
| Crash Test Dummies | Gold | Gain 2 Training Dummies. Combat start: Your Training Dummies launch themselves towards a group of enemies and Stun them for 1 seconds. |
| Cry Me A River | Gold | Gain a Tear of the Goddess. Your team gains 1 Mana Regen. After 12 seconds in combat, increase this to 4. |
| Cursed Crown | Prismatic | Gain +2 max team size and your team gains 4% Durability, but you take double player damage when you lose a player combat. |
| Cybernetic Implants | Gold | Allies holding an item gain 100 Health and 15% Attack Damage. Gain a B.F. Sword. |
| Cybernetic Uplink | Gold | Allies holding an item gain 100 Health and 2 Mana Regen. Gain a Tear of the Goddess. |
| Deadlier Blades | Prismatic | Gain a Deathblade. Deathblades permanently gain [AD: unknown]% Attack Damage whenever their holders score a takedown. |
| Deadlier Caps | Prismatic | Gain a Rabadon's Deathcap. Rabadon's Deathcaps grant 1 Mana Regen and permanently gain [AP: unknown]% Ability Power whenever their holders score a takedown. |
| Divine Amendment | Gold | Arbiters add a second effect to their law. Gain a Leona and a Zoe. |
| Dummify | Silver | Lose all champions on your board and bench. Gain a Training Dummy with 60% of their combined Health. The Training Dummy gains 1000 Health per stage. Gain a non-Tank 2-star 2-cost champion. |
| Duo Queue | Gold | Gain 2 random 5-cost champions and 2 copies of a random component. |
| Early Learnings | Gold | Your team gains 5% Attack Damage & Ability Power. This increases by 1% after each player combat. 1-cost champions gain double. (live counter) TFT Vegas Open, 2023 |
| Ekko's Boon | Gold | Gain the Anomaly, an item which grants units a powerful evolution depending on their role. Gain 2 Magnetic Removers. |
| Electrocharge I | Silver | Every 4 attacks an ally takes, they deal 30-90 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Electrocharge II | Gold | Every 4 attacks an ally takes, they deal 50-140 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Epic Rolldown | Gold | When you reach Level 8, gain 20 Shop rerolls. Runeterra Reforged Championship, 2023 |
| Epoch | Gold | Now, and at the start of every stage, gain 4 XP and 2 free rerolls. |
| Epoch+ | Gold | Now, and at the start of every stage, gain 8 XP and 2 free rerolls. |
| Evelynn's Boon | Gold | Your team gains 10% Durability. Lose an additional 1 Tactician Health when you lose combat. |
| Exclusive Customization | Gold | Gain a Lucky Item Chest, a Magnetic Remover, and 7 gold. Use a Lucky Item Chest on a champion to choose from an armory of recommended items! |
| Exclusive Customization II | Prismatic | Gain 2 Lucky Item Chests, a Magnetic Remover, and 7 gold. Use a Lucky Item Chest on a champion to choose from an armory of recommended items! |
| Exiles I | Silver | Your champions that start combat with no adjacent champions gain a 20% max Health shield for 10 seconds. |
| Exiles II | Gold | Your champions that start combat with no adjacent champions gain a 30% max Health shield for 10 seconds. |
| Expected Unexpectedness | Prismatic | Now and at the start of the next 2 stages, roll 3 dice. Gain various rewards based on their total. |
| Expedition | Silver | At the start of every round, lose the champion in the rightmost bench slot. After losing 33 gold of champions in this way, gain a powerful reward. Gain a 3-cost champion now. (live counter) |
| Explosive Growth | Gold | Now and at the start of the next 3 rounds, gain 7 XP. |
| Explosive Growth+ | Gold | Now and at the start of the next 3 rounds, gain 10 XP. |
| Feed the Flames | Gold | Gain a Sunfire Cape. Your team gains 12% Omnivamp when attacking Burning enemies. |
| Feeling Lucky | Silver | Gain 12 gold, then flip a coin. If heads, gain 4 more gold. (live counter) |
| Find Your Center | Silver | Your champion that starts combat in the center of the front row gains 15% Damage Amp and 25% max Health. |
| Firesale | Silver | Each round, steal a random 3-cost or lower champion from the shop. Gain 1 gold. |
| Flexible | Prismatic | Gain 1 random Emblem. At the start of every Stage, gain a random Emblem. Your team gains 30 Health for each Emblem they are holding. |
| Flowing Tears | Silver | Gain a Tear of the Goddess. After your team spends 6500 Mana, gain 2 more. Mana spent: (live counter) |
| Focused Fire | Silver | Your team gains 10% Attack Damage. Every 5 seconds, they gain 5% more. |
| Forge a Friend | Silver | The next 1-cost champion you buy is 3-star. Gain 5 gold. |
| Forged in Strength | Prismatic | Gain 1 random Artifact. When you drop below 30 Health, gain 3 more random Artifacts. Remix Rumble Championship, 2023 |
| Forward Thinking | Gold | Lose all your gold. After 5 player combats, gain back the original amount and another 62 gold. Incoming Gold: (live counter) |
| Frontline Foundation | Gold | Gain a 2-star 1-cost Tank champion and an Emblem that matches their last-listed trait. |
| Gain @Gold@ Gold | Gold | Gain 21 gold. |
| Giant and Mighty | Prismatic | Your team becomes large, gaining 200 Health and 10% max Health. |
| Gilded Steel | Gold | Gain 2 non-Tank 5-cost champions. If you field at least one 5-cost champion, your 1 to 4-cost champions gain 8% Durability. K.O. Coliseum Tactician's Crown, 2025 |
| Glass Cannon I | Silver | Allies that start combat in the back row begin combat at 80% Health but gain 16% Damage Amp. |
| Glass Cannon II | Gold | Allies that start combat in the back row begin combat at 80% Health but gain 25% Damage Amp. |
| Going Long | Prismatic | You no longer gain interest. Gain 16 gold now. Round start: gain 4 XP. Interest is extra gold you gain per 10g saved. |
| Golden Gamble | Prismatic | Gain 2 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed item anvils. (live counter) |
| Golden Gamble+ | Prismatic | Gain 3 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed item anvils. (live counter) |
| Golden Gamble++ | Prismatic | Gain 6 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed item anvils. (live counter) |
| Good For Something I | Silver | Champions that aren't holding items have a 40% chance to drop 1 gold on death. |
| Group Hug I | Silver | Combat start: Allies grant other adjacent allies 6 Armor and Magic Resist. This effect stacks. |
| Group Hug II | Gold | Combat start: Allies grant other adjacent allies 9 Armor and Magic Resist. This effect stacks. |
| Hard Commit | Prismatic | Gain a random Emblem. Now and at the start of each Stage, gain a 1-star champion of that trait with a cost equal to the Stage (max 5) and 3 gold. |
| Healing Orbs I | Silver | When an enemy dies, the nearest ally is healed for 250. |
| Healing Orbs II | Gold | When an enemy dies, the nearest ally is healed for 575. |
| Heart of Steel | Gold | Gain a Steadfast Heart. Every 10 seconds of combat its holder survives, Steadfast Heart gains additional [Health: unknown] permanent max Health. |
| Heart of the Swarm | Prismatic | All 3-star champions now count towards Swarmling power. At level 9, while fielding 6 unique 3-star champions, Primordians summon the Apex Primordian. Gain 3 Primordian Champions and 2 Tiny Duplicators. Unique 3-Star Champions Fielded: (live counter) / 6 Levels: (live counter) / 9 |
| Heat Death | Gold | Gain a Mordekaiser. Your strongest Mordkaiser becomes a Magic Fighter with a slowly growing aura that deals damage over time. |
| Heavy Is the Crown | Gold | Gain a Crown of Demacia, which provides significant bonuses to the wearer. If the holder dies, lose the round. |
| Hedge Fund | Prismatic | Gain 22 gold. Your max interest is increased to 10. Interest is extra gold you gain per 10g saved. |
| Hedge Fund+ | Prismatic | Gain 30 gold. Your max interest is increased to 10. Interest is extra gold you gain per 10g saved. |
| Heroic Grab Bag | Gold | Gain 2 Lesser Champion Duplicators and 5 gold. This item allows you to copy a 3-cost or less champion. |
| Heroic Grab Bag+ | Gold | Gain 2 Lesser Champion Duplicators and 8 gold. This item allows you to copy a 3-cost or less champion. |
| Heroic Grab Bag++ | Gold | Gain 2 Lesser Champion Duplicators and 14 gold. This item allows you to copy a 3-cost or less champion. |
| High Voltage | Gold | Gain an Ionic Spark. Your Ionic Sparks have +3 hex radius and do 25% more damage. |
| Hold the Line | Prismatic | Allies in your back 2 rows gain 9% Ability Power and 8% Attack Damage for each ally that starts combat in your front row. |
| Hustler | Gold | You no longer gain interest but gain 3 gold at the start of every player combat round. Gain 3 gold now. Interest is extra gold you gain per 10g saved. |
| Indiscriminate Killer | Gold | Gain a Giant Slayer. Giant Slayers gain their Damage Amp against all enemies instead of just Tanks. |
| Infinity Protection | Gold | Gain 3 gold now. On Stage 3-7, gain an Infinity Force. Infinity Force gives allies in the same row a 12% Health shield. Monsters Attack Championship, 2023 |
| Invested+ | Prismatic | Gain 26 gold. After every combat, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Invested++ | Prismatic | Gain 40 gold. After every combat, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Iron Assets | Silver | Gain a component anvil and 3 gold. |
| Item Grab Bag | Silver | Gain 1 random completed item. |
| Jeweled Lotus I | Gold | Your team gains 10% Critical Strike Chance and Precision. {{TFT_Keyword_Precision}} |
| Jeweled Lotus II | Prismatic | Your team gains 25% Critical Strike Chance, 10% Critical Strike Damage, and Precision. {{TFT_Keyword_Precision}} |
| Kahunahuna | Gold | Every 5th attack deals bonus true damage equal to 100% Basic Attack Damage. |
| Kayle's Boon | Gold | For each unique completed item on your team, your team gains 10 Health, 1% Attack Damage, and 1% Ability Power. |
| Kick Start | Silver | Gain a random 2-star 2-cost champion and 1 gold. |
| Late Game Scaling | Gold | Gain 2 XP at the start of player combat rounds. Your 5-cost champions gain 12% Health and 12% Attack Speed. Cyber City Tactician's Crown, 2025 |
| Late Game Specialist | Silver | When you reach Level 9, gain 27 gold. |
| Latent Forge | Silver | After 8 player combats, gain an Artifact anvil. The anvil offers 4 choices. Artifacts are more powerful items with a unique effect. |
| Legion of Threes | Gold | Gain a random Emblem. Your 3-cost champions and all allies equipped with an Emblem gain 150 Health and 12% Attack Speed. Reckoning Championship, 2021 |
| Level Up! | Prismatic | When you buy XP, gain an additional 2. Gain 6 immediately. |
| Lineup | Silver | Your team gains 2 Armor and Magic Resist for each ally that starts combat in the front two rows. |
| Little Buddies | Gold | Your 4-cost and 5-cost champions gain 65 Health and 7% Attack Speed for every 1-cost and 2-cost champion on your board. (live counter) |
| Living Forge | Prismatic | Gain an Artifact anvil now and after every 9 player combats. Artifacts are more powerful items with a unique effect. |
| Loot Singularity | Gold | When the Black Hole consumes enemies, it turns them into space matter. After acquiring enough, the Black Hole turns it into loot! Gain a Cho'gath and a Lissandra. Space matter and loot quality increase each time. Gain 2/3/4 space matter depending on Dark Star breakpoint. Space Matter: (live counter) / (live counter) |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves and 4 gold. |
| Lucky Gloves+ | Prismatic | Thief's Gloves will always give your champions recommended items. Gain 2 Sparring Gloves now, then another after 4 player combats. |
| Lunch Money | Silver | Every 8 damage you deal to enemy tacticians gives you 2 gold. Total Payouts: (live counter) Gold |
| Luxury Subscription | Prismatic | Gain a package containing a 5-cost champion, a 2-star 1-cost champion, and 7 gold. At the start of the next 2 stages, gain the package again. |
| Mace's Will | Gold | Gain a Sparring Gloves. Your team gains 25% Critical Strike Chance. |
| Makeshift Armor I | Silver | Allies without items equipped gain 30 Armor and Magic Resist. |
| Makeshift Armor II | Gold | Allies without items equipped gain 50 Armor and Magic Resist. |
| Malicious Monetization | Gold | Gain 4 gold. For the next 2 rounds, enemy champions drop 2 gold when killed. (live counter) |
| May the Fours Be With You | Gold | Your 4-cost champions and Jhin gain 144 Health and 14% Attack Speed. Gain a Jhin. |
| Min-Max | Prismatic | Gain a Golden Item Remover and 4 random components. Golden Item Remover is a Magnetic Remover with infinite uses. |
| Misfits | Gold | Your team gains 55 Health for each active Unique trait. Gain a Rhaast and a Miss Fortune. (live counter) Unique traits belong to 1 champion and are shown in orange. |
| Missed Connections | Silver | Gain a copy of each 1-cost champion. |
| Money Hungry | Gold | Gain 7 gold now, then 7 gold at the start of every stage. Picking up gold makes your Tactician larger. |
| Money Monsoon | Prismatic | Gain 7 gold now and every round for the rest of the game. |
| New Recruit | Prismatic | Gain +1 max team size and 1 four-cost champion. |
| New Recruit+ | Prismatic | Gain +1 max team size and 2 four-cost champions. |
| NO SCOUT NO PIVOT | Gold | Champions that fight in player combat cannot be benched or sold. Your team gains 1% Health, and 1% Attack Damage and Ability Power after player combat. (live counter) |
| On a Roll | Silver | Whenever you star up a champion that you fielded last combat, gain 1 free Shop rerolls. Gain 6 gold. |
| One Buff, Two Buff | Prismatic | Gain a Red Buff, a Blue Buff, and a Champion Duplicator. |
| One Two Three | Silver | Gain 2 1-cost champion, 1 2-cost champion, and 1 3-cost champion. |
| One, Two, Five! | Silver | Gain 1 random component, 2 gold, and 1 random 5-cost champion. |
| Pandora's Items | Silver | Round start: items on your bench are randomized. Gain 1 random component. |
| Pandora's Items II | Gold | Round start: items on your bench are randomized. Gain 2 random components. |
| Pandora's Items III | Prismatic | Round start: items on your bench are randomized. Gain 1 random Radiant item. |
| Partial Ascension | Silver | After 12 seconds of combat, your team gains 20% Damage Amp. |
| Patience is a Virtue | Silver | Gain 4 rerolls now. Each round, gain 1 free rerolls if you did not buy a champion last round. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. |
| Pilfer | Gold | Each round, gain a 1-star copy of the first champion you killed last combat. The first time you gain 19 gold worth of champions in this way, gain a Thief's Gloves. (live counter) |
| Plot Armor | Gold | Your team gains 8 Armor and Magic Resist. After falling below 50% Health, increase this to 40 for the rest of combat. |
| Portable Forge | Gold | Choose 1 of 4 Artifacts. Artifacts are more powerful items with a unique effect. |
| Prismatic Ticket | Prismatic | Each time your Shop is rerolled, you have a 45% chance to gain a free reroll. |
| Prizefighter | Gold | Gain 2 item components. Every 5 wins gives you an item component. |
| Radiant Rascal | Prismatic | Gain a Rascal's Gloves item. This equips 2 random Radiant items every round. Radiant items are very powerful versions of completed items. |
| Reach for the Stars | Gold | Gain a Jax. Your strongest Jax becomes a Magic Fighter that deal bonus magic damage on attack and gains stacking Attack Speed on cast. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 cost tier higher (max 5). Gain 2 Magnetic Removers and 3 rerolls. |
| ReinFOURcement | Gold | The next 4-cost champion you buy with gold is instantly upgraded to 2-star. Gain 7 gold. |
| Replication | Gold | Choose 1 of 3 components. For the next 2 rounds, gain a copy of that component. |
| Restart Mission | Silver | Remove all champions on your board and bench. Gain 2 random 2-star 3-costs, 2 2-star 2-costs, and 1 2-star 1-cost champion. |
| Retribution | Prismatic | Gain 2 Hands of Justice. Allies equipped with Hand of Justice gain Precision and 25% Critical Strike Chance. {{TFT_Keyword_Precision}} |
| Risky Moves | Silver | Your Tactician loses 20 Health, but after 7 player combats, gain 26 gold. |
| Rolling For Days I | Silver | Gain 10 free Shop rerolls. |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 8 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin+ | Gold | Gain 1 random completed item now, and 1 component after 4 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Savings Account | Gold | After you earn 50 gold in interest, gain 25 gold. Your max interest is increased to 7. Gain 2 gold now. Gold earned: (live counter) |
| Second Wind | Silver | After 10 seconds of combat, your team heals 40% of their missing Health. |
| Second Wind II | Gold | After 10 seconds of combat, your team heals 60% of their missing Health. |
| Self Destruct | Gold | Gain a Gragas. Your strongest Gragas becomes a Magic Fighter with a massive explosion, but also damages himself. |
| Seraphim's Staff | Gold | Gain an Archangel's Staff. Archangel's Staff grants 2 additional Mana Regen if its holder has 90% or more Ability Power. |
| Shieldmaiden | Silver | Gain a Leona. Your strongest Leona becomes an Attack Fighter that dashes through enemies to deal physical damage, stunning the first target hit. |
| Shimmerscale Essence | Prismatic | Gain a Mogul's Mail. In 7 rounds, gain a Gamblers Blade. These items give gold as well as combat power. |
| Shopping Spree | Prismatic | When you level up, gain a number of free shop refreshes equal to your level+1. Gain 2 gold. |
| Side Effects | Gold | When an ally is healed, they deal 50% of the healing to their target as magic damage. Every 5 seconds, allies heal for 4% of their max Health. |
| Silver Spoon | Silver | Gain 10 XP. |
| Size Matters | Silver | Gain a Giant's Belt. After a champion starts combat with more than [HealthRequired: unknown] max Health, gain a Warmog's Armor. |
| Slammin' | Gold | Gain 3 gold. After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slammin'+ | Gold | Gain 4 gold and 10 XP now. After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slice of Life | Silver | Twice per stage, gain a random champion. Its cost increases by Stage. This effect ends after receiving 1 5-cost champion. |
| Slightly Magic Roll | Silver | Roll a die. Gain rewards based on the number rolled. Reward: (live counter) |
| Small Grab Bag | Silver | Gain 2 random item components. |
| Solo Leveling | Gold | For the next 5 combats, your team size is 1 but the champion you field has massively increased stats. Gain 1 XP for every kill they get. Afterwards, gain 2 item components. |
| Solo Plate | Gold | Gain a Gargoyle Stoneplate. Combat start: Allies holding Gargoyle Stoneplate gain 17% max Health if they're the only one in their row. |
| Soraka's Boon | Gold | Your team gains 2.5 Health for each missing player health. At the start of each player combat, gain an additional 1 player health. |
| Soul Awakening | Prismatic | Combat start: Your team gains 1.5% Attack Damage and Ability Power per second for 10 seconds. At max stacks, deal 12% bonus true damage. |
| Speedy Double Kill | Gold | Gain a Guinsoo's Rageblade. When 2 players are eliminated, gain 40 gold. Fates Championship, 2021 |
| Spirit of Redemption | Gold | Gain a Spirit Visage. Every 5 seconds, Spirit Visage heals allies within 1 hex for 7.5% of their missing Health. |
| Spreading Roots | Gold | Gain 2 random Emblems and 1 gold. |
| Spreading Roots+ | Gold | Gain 2 random Emblems, a Reforger, and 2 gold. |
| Staffsmith | Gold | Gain 2 random completed items that build from Needlessly Large Rod. |
| Stand United | Silver | Gain 1.5% Attack Damage and 1.5% Ability Power for each non-unique Trait active across your team. Current Bonus: (live counter)% AD (live counter)% AP |
| Stellar Combo | Silver | Gain an Aatrox. Your strongest Aatrox becomes an Attack Fighter with an ability that swaps between three different strikes. |
| Subscription Service | Prismatic | Now and at the start of each Stage, open a Shop of 4 unique 4-cost champions and gain 6 gold. |
| Sunfire Board | Gold | Combat Start: Burn all enemies for 18% of their max Health over 18 seconds and reduce healing received by 33%. |
| Survivor | Silver | Gain 4 gold. After 3 players are eliminated, gain 100 more gold. |
| Sweet Treats | Prismatic | Gain an Artifact anvil. Your team gains 16 Health for each item equipped on champions. Health Gained: (live counter) Magic N' Mayhem Tactician's Crown, 2024 |
| Sword Overflow | Prismatic | Gain 4 B.F. Swords. Your B.F. Swords grant +4% Attack Speed. |
| Swordsmith | Gold | Gain 2 random completed items that build from B.F. Sword. |
| Tactician's Kitchen | Prismatic | Gain a random Emblem. After 3 rounds, gain a Tactician's Cape. |
| Team Building | Silver | Gain a Lesser Champion Duplicator. Gain another after 5 player combats. This item allows you to copy a 3-cost or less champion. |
| Teaming Up | Silver | Gain 1 random component and 2 random 3-cost champions. |
| Termeepnal Velocity | Silver | Gain a Poppy. Your strongest Poppy becomes a ranged Attack Caster that flings a high-velocity Meep at her target. |
| The Golden Dragon | Gold | Gain a Mogul's Mail. Champions holding Mogul's Mail take up 2 team slots but gain 700 Health and 20% Durability. Mogul's Mail gives gold as well as combat power. Dragonlands Championship, 2022 |
| The Golden Egg | Prismatic | Gain a golden egg that hatches in 11 turns for a huge amount of loot. Winning player combat speeds up the hatch timer by an extra turn. |
| The Tower | Silver | Gain a giant Training Dummy with increased Health (increases with Stage). Every 4 seconds, it zaps the 3 nearest enemies dealing 5% max Health true damage. |
| The Trait Tree | Prismatic | Gain 3 random Emblems, a Reforger, and 2 gold. |
| The Trait Tree+ | Prismatic | Gain 3 random Emblems, a Reforger, and 4 gold. |
| Thresh's Boon | Gold | Each round, roll a die. Get a bonus based on that roll. Health: (live counter) Attack Speed: (live counter)% |
| Timestream | Gold | Your team gains 100 Health. Timebreakers gain 7 Health and 0.3% Attack Speed every time you reroll (including free rerolls). Gain an Ezreal and a Pantheon. (Health: (live counter), Attack Speed: (live counter)%) |
| Tiniest Titan | Prismatic | Gain 2 player Health and 2 gold after every player combat. Your Tactician also moves faster. Gain 10 gold now. |
| Tiny Titans | Silver | Increase your current and max player health by 20. |
| Tiny, but Deadly | Prismatic | Your team becomes small, gaining 33% Attack Speed and Movement Speed. |
| Tons of Stats! | Gold | Your team gains 44 Health, 4% Attack Damage, 4% Ability Power, 4 Armor, 4 Magic Resist, 4% Attack Speed, and 4 Mana. |
| Tour of the Galaxy | Gold | Voyagers gain 130 Health, 13% Attack Damage, and 13% Ability Power, increased by 10% each time you fight a new player. You prefer to fight players you haven't fought recently. Gain a Pyke and a Meepsie. |
| Trade Sector | Gold | Gain a free Shop reroll every round. Gain 2 gold. |
| Treasure Hunt | Gold | Gain a locked chest each Stage between now and Stage [MaxNumChests: unknown]. Unlock each chest when you spend [GoldNeededPerChest: unknown] gold on Shop rerolls. These chests persist until opened. |
| Trifecta I | Gold | Gain 2 3-cost champions. Combat Start: 3 random 3-cost champions gain 220 Health and 20% Attack Speed. |
| Trifecta II | Prismatic | Gain 3 3-cost champions. Allies gain 5% Attack Speed. Combat Start: 3 random 3-cost champions gain 300 Health and 25% Attack Speed. |
| Twin Guardians | Silver | If you have exactly 2 allies in your first row, grant them 100 bonus Health, 35 Armor, and 35 Magic Resist. |
| Two Much Value | Gold | Gain 1 reroll for every 2 unique 2-cost champions fielded last player combat. Gain 1 2-cost champions. |
| Two Tanky | Gold | When you field exactly 2 copies of a champion, they both gain 375 Health. When one copy dies, the other gains a 35% max Health Shield for 12 seconds. When you 3-star, gain a 2-star copy. |
| U.R.F | Gold | Gain a Spatula. Champions holding Spatula or Frying Pan items gain 20% Attack Speed and 3 Mana Regen. |
| Upward Mobility | Prismatic | Buying XP costs 1 less. Gain 2 Health and 1 free rerolls whenever you level up. |
| Urf's Gambit | Gold | If you win your next player combat, gain a Spatula. If you lose, gain a Frying Pan. Gain a component anvil and 3 gold now. |
| Urf's Grab Bag | Prismatic | Gain 1 Spatula, 3 random components, and 5 gold. |
| Vampiric Vitality I | Silver | You heal for 12% of the damage you deal to enemy Tacticians. Your team gains 10% Omnivamp. |
| Varus's Boon | Gold | Your team gains 15 health for each total star level in your army, and your 5 cost odds increase by 2%. |
| Wand Overflow | Prismatic | Gain 4 Needlessly Large Rods. Your Needlessly Large Rods grant +5% Attack Speed. |
| Warlord's Honor | Gold | Every round, your team gain 5% Attack Damage and Ability Power. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Warpath | Gold | Gain a 2-star 2-cost champion. After dealing 80 player damage, gain a chest of high cost champions and items. (Current: (live counter)) |
| We Stick Together | Prismatic | Gain a random Emblem and a completed item anvil. Allies that share a trait with that Emblem gain 30% Attack Speed. (live counter) Into The Arcane Tactician's Crown, 2025 |
| Win Out | Prismatic | When you reach level 9, immediately level to 10 and gain 8 free Shop rerolls. Gain 10 XP now. |
| Wise Spending | Prismatic | You can no longer buy XP. Every time you spend gold to reroll your Shop, gain 2 XP. Gain 1 gold now. |
| Worth the Wait | Gold | Gain a random 1-cost champion. Gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Worth the Wait II | Prismatic | Gain 2 copies of a random 2-cost champion. Gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Woven Magic | Gold | Gain a random item component. Every 2200 Mana your team spends grants an additional component (max (live counter)/3). Mana spent: (live counter)/2200 |
| Yasuo's Boon | Gold | Increase the power of Yasuo's hexes by 50%. If you only have 2 hexes, gain 8 gold. |
| Yasuo's Golden Hex | Silver | Paint a Golden Hex. The unit in the hex gains 250 health, 25% attack speed, and drops 1 gold on kill. |
| Yasuo's Share The Spotlight | Gold | Yasuo's hexes also affect adjacent units at 100% effectiveness. |
| You Have My Bow | Gold | Gain a Recurve Bow. Your team gains 12% Attack Speed. |
| Your team gains @SmallQuestADAP@ Attack Damage and Ability Power. | unknown | Your team gains 15 Attack Damage and Ability Power. |

## Opening encounters (19)

Source: MetaTFT lookup "encounters" list. Duplicate text rows are merged.

| Host champion | Effect | Stages |
|---|---|---|
| TwistedFate | Twisted Fate deals out all gold tier augments this game. | 1 |
| Morgana | Morgana bewitches all augments to prismatic tier this game. | 1 |
| Graves | Graves enhances the last augment to be prismatic this game. | 1 |
| Shen | Shen's time rift turns the first augment to prismatic this game. | 1 |
| MissFortune | Start with a random 3-cost champion this game. | 1 |
| IvernMinion | Start with a random 2-cost champion this game. | 1 |
| Viktor | Viktor upgrades your starting champion to 2-star. | 1 |
| Poppy | Poppy grants you two component anvils. | 1 |
| TahmKench | Tahm Kench grants you highly varied loot at the start of each stage! | 1 |
| Rhaast | Rhaast begins your journey with 3 random emblems. | 1 |
| Zoe | Zoe grants you gold at the start of every stage. | 1 |
| Lissandra | Gain a team of 5 random 1-cost champions. | 1 |
| Fiora | The last two surviving players gain 70 gold for an epic final fight. | 1 |
| Talon | Gain +1 Augment rerolls this game. | 1 |
| Belveth | Gain a Tiny Champion Duplicator. On 3-5, gain Lesser Champion Duplicator. | 1 |
| Ornn | On 3-3, all players gain an Artifact Anvil. | 1 |
| LeBlanc | LeBlanc manipulates augments to be offered on 3-1, 3-2, and 3-3 this game. | 1 |
| Ezreal | On 2-1, gain 5 free rerolls. | 1 |
| Sona | Sona modifies each level to cost 2 XP less. | 1 |

## Gods (Realm of the Gods) (10)

Source: MetaTFT lookup "set17.gods". Category is the reward type stored in the data; the per-god boon lists are empty in this file.

| God | Title | Category |
|---|---|---|
| Ahri | God of Opulence | Gold |
| Ekko | God of Time | Unique |
| Kayle | God of Order | Item |
| Yasuo | God of the Abyss | Unique |
| Evelynn | God of Temptation | Unique |
| Soraka | God of Stars | Health |
| Varus | God of Love | Unique |
| Thresh | God of Pacts | Unique |
| Aurelion Sol | God of Wonders | Gold |
| God_Pengu | God_Pengu_Title | Augment |

## God offerings and market choices (354)

Source: MetaTFT lookup "armory_items". In Set 17 these include the Realm of the Gods offerings ("MarketOffering"), God boons and Pengu offerings.

| Offering | Effect |
|---|---|
| Lovers + | Gain a 4-cost and a 3-cost that share a trait. |
| 12 Tactician Health | 12 Tactician Health |
| Artifact Item | After 8 rounds, gain a random Artifact. |
| Blood Pact ++ | Gain 13 gold. For each other player that picks this, lose 1 Tactician Health. |
| Blue Loot Orb | Blue Loot Orb |
| Component Anvil | Drag this to the Shop to open an Armory full of component items! You will have 30 seconds to choose. |
| Wealth Quest | When you reach 50 gold, gain a component anvil and 4 gold. |
| Roll the Bones | Roll 3 dice. Gain gold equal to the total. |
| Gray Loot Orb | Gray Loot Orb |
| Acceleration Hex | Place the hex on a champion. Allies in the hex gain 30% Attack Speed. [Consumable - This item disappears when used.] |
| Hard Pivot | Sell your board and bench. Gain 15 free rerolls, and your next 3 shops have all 5 costs. |
| 2-star 4-cost | Gain a random 2-star 4 cost champion. Lose 3 Tactician Health. |
| 5-cost champion | 5-cost champion |
| Soraka's Embrace | At combat start, grant the ally who died first the previous combat 500 to 900 Shield, based on the current Stage. |
| Lesser Champion Duplicator | Use on a 3-cost champion or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Speed Increase | Your Tactician moves faster! |
| Starry Quest | The next time you star up a champion, gain 10 gold. |
| 16 XP | 16 XP |
| 16 free rerolls | 16 free rerolls |
| Completed Item Anvil | In 5 rounds, gain a Completed Item Anvil. |
| Combat Hack | Your next augment selection will display all combat augments in the first slot, and that slot has 2 extra rerolls. |
| Craftsmanship | Gain 1 Reforgers now and one at the start of each stage. Whenever you use a Reforger, gain 2 gold. |
| 2 Salvagers | Use on a champion to remove all their items and break them down into components. [Consumable - This item disappears when used.] |
| 12 gold | Gain 12 gold. You cannot buy champions for 2 rounds. |
| 2 5-cost champions. | After 4 rounds, gain 2 5-cost champions. |
| 2-star 3-cost | Gain a random 2-star 3 cost champion. Lose 3 Tactician Health. |
| The Lovers | Gain a 3-cost and a 2-cost that share a trait. |
| Augment Wheel | Gain either: 1 random Prismatic augment, 2 random Gold augments, or 3 random Silver augments and [SilverGold: unknown] gold. |
| Starcrossed Upgrade ++ | Your next shop contains all [Tier: unknown]-cost units. The first one you buy is free and upgraded to 2-star. |
| Silver Augment | Open an armory of 3 Silver augments and choose one. |
| Starlight Hex | Place the hex on a champion. Allies in the hex gain 3 Mana Regen. [Consumable - This item disappears when used.] |
| Boon of Omnipotence | When you reach level [LevelReq: unknown], gain +1 to all non-unique traits. |
| Trade Sector | Gain a free Shop reroll every round. Gain 4 gold. |
| 5-cost shop | After 3 rounds, get a free shop reroll and your next shop contains all 5-cost champions. |
| Starcrossed Upgrade | Your next shop contains all 2-cost units. The first one you buy is free and upgraded to 2-star. |
| 14 XP | 14 XP |
| Gold Loot Orb | Gold Loot Orb |
| Pandora's Seat | At the start of every round, champions on the 2 rightmost bench slots transform into random champions of the same cost. |
| 36 XP. | 36 XP. |
| Starcrossed Upgrade | Your next shop contains all [Tier: unknown]-cost units. The first one you buy is free and upgraded to 2-star. |
| Tome of Traits | Drag this Tome to the Shop to open an Armory full of emblems! You will have 30 seconds to choose. |
| Finalist Gambit | Gain 3 gold. If you make it to top 4, gain 30 more gold. |
| Anomaly | Next planning phase, gain an Anomaly which lets you apply a powerful evolution to one champion. You can spend gold to reroll the offering. |
| God Artifact Anvil | Drag this to the Shop to open an Armory full of God Artifact items! You will have 30 seconds to choose. |
| Component Anvil | After 3 rounds, gain a component anvil. |
| Blood Pact | Gain 8 gold. For each other player that picks this, lose 2 Tactician Health. |
| Low Health Quest | The first time you fall to 30 health, gain 21 gold. |
| Shimmerscale Essence | Gain a Mogul's Mail. In 5 rounds, gain a Gamblers Blade. These items give gold as well as combat power. |
| Two 3-star 1-cost Champions | Two 3-star 1-cost Champions |
| Gold Every Turn | Gain 1 gold each round for the rest of the game. |
| 18 gold | Gain 18 gold. You cannot buy champions for 2 rounds. |
| Starter Kit | Gain a 4-cost champion, a 2-star 1-cost champion that shares a trait with them, and 8 gold. At the start of the next 2 stages, gain that 4-cost champion again. |
| Sacrifice | Lose 10 Tactician Health and gain 20 gold. |
| Augment Rerolls | You get +2 Augment rerolls for all future augment choices. Gain 6 gold. |
| Steamroll | Gain a random 2- cost and 1 gold. Repeat after each player combat until you lose one. |
| Blessing: Wealth | Permanently grant a champion 8% Attack Damage and Ability Power, and a 12% chance to generate 1 gold on takedown. |
| Reroll Quest | After rolling the shop 15 times, gain 2 component anvils. |
| 3 gold | 3 gold |
| 2 Tiny Duplicators | Use on a 1-cost champion to create a 1-star copy of them. |
| Rainbow Shops | Your next 3 shops will each contain one unit of each tier. Gain 1 free reroll. |
| 7 free rerolls | Just keep rolling |
| Better Together + | Gain 6 Tactician Health. If at least 5 players choose this, gain 3 gold. |
| Owned 3 cost | Gain a copy of a 3-cost champion you own and 2 gold. If you don't own any 3-cost champions, gain a random one |
| Storm Hex | Place the hex on a champion. The first time an ally in the hex hits an enemy with an Ability, that enemy is Stunned for 1.75 seconds. [Consumable - This item disappears when used.] |
| Divine Investment | Your maximum interest is increased by 1. Gain 4 gold. |
| 4-cost | Gain a random 1-star 4 cost champion. Lose 4 Tactician Health. |
| Divine Investment | Your maximum interest is increased by 1. Gain 6 gold. |
| 4 free rerolls | Just keep rolling |
| Better Together | Gain 6 Tactician Health. If at least 5 players choose this, gain 2 gold. |
| Anvil Transformation | Whenever you would get a component, gain a Component Anvil instead. Gain 2 gold. |
| Level Quest | When you reach level 9, gain 20 gold. |
| Super Parting Gift | The next time you select God Pengu's gift, gain an extra copy of the champion and 2 gold. |
| Level Up! | When you buy XP, gain an additional 2. Gain 12 immediately. |
| Divine Empathy | At 66 and 33 player Health, gain a random component. |
| Increased Interest | Increase your maximum interest by [Interest: unknown]. |
| Steamroll+ | Gain a random 3-cost and 1 gold. Repeat after each player combat until you lose one. |
| Cosmic Hex | Place the hex on a champion. Allies in the hex gain 10% max Health, and gain 35 permanent Health each round. [Consumable - This item disappears when used.] |
| Humility | Set your loss streak to 5 and gain 6 Tactician Health. |
| Frontline Dummy | Gain a training dummy with a Brawler Emblem, a Bastion Emblem, and a Vanguard emblem. |
| 2 1-cost champions | 2 1-cost champions |
| 3-cost champion | 3-cost champion and 1 gold. |
| Random Boon | Gain a random boon from ANY god, and 3 gold. Chosen Boon: (live counter) (live counter) |
| Random Boon | Gain a random boon from ANY god, and 2 gold. Chosen Boon: (live counter) (live counter) |
| 4-cost champion | 4-cost champion |
| Random Boon | Gain a random boon from ANY god, and 5 gold. Chosen Boon: (live counter) (live counter) |
| Augment Rerolls | You get +2 Augment rerolls for all future augment choices. Gain 4 gold. |
| Cashflow | Gain 2 gold each round. |
| Duplicator Deal | Gain 2 Lesser Champion Duplicators. You cannot use your shop for 2 rounds. |
| 25 gold | 25 gold |
| 10 gold | Make it rain! |
| 6 gold | Make it rain! |
| Random Component | Random Component |
| 8 XP. | Learn and grow |
| 8 gold | Make it rain! |
| 2 gold | 2 gold |
| Mini Recombobulate | Transform all 1 and 2-Cost champions on your board into random ones of a higher cost. |
| Chest of Greed | Gain 2 gold, plus 12 gold split between all players who pick this (rounded down.) |
| 4 gold | 4 gold |
| Chest of Greed | Gain 3 gold, plus 16 gold split between all players who pick this (rounded down.) |
| Completed Item | Gain a random completed item. Lose 5 Tactician Health. |
| Duplicator Bundle | Gain 4 Tiny Champion Duplicators and 1 Lesser Champion Duplicator. |
| Chest of Greed | Gain 4 gold, plus 25 gold split between all players who pick this (rounded down.) |
| Blue Loot Orb and 2 Gold | Blue Loot Orb and 2 Gold |
| Scuttle Party | Your next PVE round will be replaced with a Scuttle Crab round with bonus loot! |
| 5 free rerolls | Just keep rolling |
| 5 gold | 5 gold |
| Cryogenic Hex | Place the hex on a champion. The first time an ally in the hex falls below 40% Health, they freeze for 1 second and heal 30% of their max Health. [Consumable - This item disappears when used.] |
| Armored Dummy | Gain a training dummy with a Warmog's Armor, Bramble Vest, and Dragon's Claw. |
| Artifact Item | After 5 rounds, gain a random Artifact. |
| Prismatic Augment | Open an armory of 3 Prismatic augments and choose one. |
| Ephemeral Rerolls | At the start of each round, if you don't have a free reroll, gain a free reroll. |
| 6 XP. | Learn and grow |
| Spatula | After 6 rounds, gain a Spatula. |
| Trait Quest | After fielding 6 non-unique traits in a player combat, gain a random Emblem and a Reforger. |
| Random Emblem | Gain a random Emblem. Lose 3 Tactician Health. |
| Completed Item Anvil | Drag this to the Shop to open an Armory full of completed items! You will have 30 seconds to choose. |
| 2-cost champion | 2-cost champion |
| Mystery Loot | You received: (live counter) |
| Mystery Loot | What could it be? |
| Golden Hex | Use on a champion to paint the hex below them, turning it into a Golden hex which empowers champions within. The empowered champion gains 250 Health, 25% Attack Speed, and drops 1 gold on kill. [Consumable - This item disappears when used.] |
| Treasure Armory | At the start of the next round, get an offering of loot you can reroll for 1 gold. |
| Lovers ++ | Gain a 5-cost and a 4-cost that share a trait. |
| Gold Augment | Open an armory of 3 Gold augments and choose one. |
| 4-cost champion | After 3 rounds, gain a 4-cost champion. |
| Precise Smithing | Reforging items and Artifacts grants an anvil of the same value instead. Gain 1 component and 2 reforger now, and 1 reforgers every stage. Includes components and Radiant Items, but does not affect Emblems or Tactician's Items. |
| Two 3-star 2-cost Champions | Two 3-star 2-cost Champions |
| 3% Increased 5-Cost Odds | Permanently increase your shop odds for 5-costs by 3%. |
| Solar Hex | Place the hex on a champion. Allies in the hex deal 15% bonus magic damage over 5 seconds. This damage 33% Wounds, and 1% Burns over the duration. [Consumable - This item disappears when used.] Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Divine Refund | Gain 2 gold. The next time you create an item, gain a copy of one of the components at random. |
| Divine Intervention | After falling to 30 player health, gain a special loot orb. |
| Shared Wealth + | Gain 10 gold. If at least 5 players choose this, gain an additional 2 gold. |
| Blood Pact + | Gain 10 gold. For each other player that picks this, lose 2 Tactician Health. |
| 2-star 2-cost | Gain a 2-star 2-cost champion. Lose 3 Tactician Health. |
| 20 free rerolls | 20 free rerolls |
| Shared Wealth | Gain 6 gold. If at least 5 players choose this, gain an additional 1 gold. |
| 3-star 1-cost | Gain a random 3-star 1 cost champion. Lose 3 Tactician Health. |
| Shared Wealth + | Gain 8 gold. If at least 5 players choose this, gain an additional 2 gold. |
| 2 5-costs | Gain 2 random 1-star 5 cost champions. Lose 1 Tactician Health. |
| 1% Increased 5-Cost Odds | Permanently increase your shop odds for 5-costs by 1%. |
| 4-Cost Shop | Your next shop contains all 4-cost units. The first one you buy is free. |
| 3-Cost Shop | Your next shop contains all 3-cost units. The first one you buy is free. |
| Owned 4 cost | Gain a copy of a 4-cost champion you own and 3 gold. If you don't own any 4-cost champions, gain a random one |
| 4-cost | 4-cost |
| 2-star Jinx | Gain a 2-star Jinx |
| 2-star Aurora | Gain a 2-star Aurora |
| 1-star Aurora | Gain a 1-star Aurora |
| 1-star Illaoi | Gain a 1-star Illaoi |
| 3-star Fiora | Gain a 3-star Fiora |
| 1-star Fiora | Gain a 1-star Fiora |
| 3-star Briar | Gain a 3-star Briar |
| 2-star Briar | Gain a 2-star Briar |
| 1-star Briar | Gain a 1-star Briar |
| 2-star Fiora | Gain a 2-star Fiora |
| 3-star Jinx | Gain a 3-star Jinx |
| 2-star Illaoi | Gain a 2-star Illaoi |
| 1-star Jinx | Gain a 1-star Jinx |
| 3-star Illaoi | Gain a 3-star Illaoi |
| 3-star Aurora | Gain a 3-star Aurora |
| Completed Item Anvil | Gain a Completed Item Anvil |
| 34 gold | 34 gold |
| 30 gold | 30 gold |
| 18 gold | 18 gold |
| 15 gold | 15 gold |
| 16 gold | 16 gold |
| 12 gold | 12 gold |
| 55 gold | 55 gold |
| 20 gold | 20 gold |
| 40 gold | 40 gold |
| 5-cost: Blitzcrank | Party Animal Space Groove Vanguard |
| 4-cost: Morgana | Dark Lady |
| 2-star 1-cost: Lissandra | Dark Star Replicator Shepherd |
| 1-cost: Caitlyn | N.O.V.A. Fateweaver |
| 4-cost: Aurelion Sol | Mecha Conduit |
| 2-star 1-cost: Leona | Arbiter Vanguard |
| 4-cost: Kindred | N.O.V.A. Challenger |
| 4-cost: Riven | Timebreaker Rogue |
| 3-cost: Rhaast | Redeemer |
| 2-star 1-cost: Aatrox | N.O.V.A. Bastion |
| 5-cost: Vex | Doomer |
| 2-cost: Meepsie | Meeple Voyager Shepherd |
| 1-cost: Nasus | Space Groove Vanguard |
| 5-cost: Graves | Factory New |
| 1-cost: Poppy | Meeple Bastion |
| 2-star 1-cost: Cho'Gath | Dark Star Brawler |
| 1-cost: Twisted Fate | Stargazer Fateweaver |
| 2-cost: Jinx | Anima Challenger |
| 2-star 1-cost Talon | Stargazer Rogue |
| 4-cost: Nami | Space Groove Replicator |
| 2-cost: Gwen | Space Groove Rogue |
| 1-cost: Leona | Arbiter Vanguard |
| 5-cost: Bard | Meeple Conduit |
| 4-cost: Rammus | Meeple Bastion |
| 1-cost: Cho'Gath | Dark Star Brawler |
| 2-cost: Mordekaiser | Dark Star Vanguard Conduit |
| 1-cost: Talon | Stargazer Rogue |
| 2-cost: Zoe | Arbiter Conduit |
| 4-cost: Tahm Kench | Arcana Brawler |
| 1-cost: Teemo | Space Groove Shepherd |
| 1-cost: Veigar | Meeple Replicator |
| 1-cost: Ezreal | Timebreaker Sniper |
| 2-cost: Milio | Timebreaker Fateweaver |
| 3-cost: Illaoi | Anima Vanguard Shepherd |
| 1-cost: Briar | Primordian Anima Rogue |
| 3-cost: Diana | Arbiter Challenger |
| 3-cost: Viktor | Psionic Conduit |
| 4-cost: Master Yi | Psionic Marauder |
| 3-cost: Miss Fortune | Gun Goddess |
| 5-cost: Zed | Galaxy Slayer |
| 2-cost: Gnar | Meeple Sniper |
| 2-star 1-cost: Teemo | Space Groove Shepherd |
| 3-cost: Lulu | Stargazer Replicator |
| 3-cost: Fizz | Meeple Rogue |
| 2-cost: Pyke | Psionic Voyager |
| 3-cost: Aurora | Anima Voyager |
| 1-cost: Aatrox | N.O.V.A. Bastion |
| 2-star 1-cost: Rek'Sai | Primordian Brawler |
| 2-cost: Bel'Veth | Primordian Challenger Marauder |
| 3-cost: Kai'Sa | Dark Star Rogue |
| 2-cost: Jax | Stargazer Bastion |
| 4-cost: Nunu | Stargazer Vanguard |
| 3-cost: Maokai | N.O.V.A. Brawler |
| 2-cost: Akali | N.O.V.A. Marauder |
| 1-cost: Rek'Sai | Primordian Brawler |
| 3-cost: Ornn | Space Groove Bastion |
| 4-cost: LeBlanc | Arbiter Shepherd |
| 2-star 1-cost: Veigar | Meeple Replicator |
| 2-star 1-cost: Twisted Fate | Stargazer Fateweaver |
| 4-cost: Xayah | Stargazer Sniper |
| 2-star 1-cost: Nasus | Space Groove Vanguard |
| 5-cost: Fiora | Divine Duelist Anima Marauder |
| 2-star 1-cost: Briar | Primordian Anima Rogue |
| 2-star 1-cost: Poppy | Meeple Bastion |
| 3-cost: Urgot | Mecha Brawler Marauder |
| 1-cost: Lissandra | Dark Star Replicator Shepherd |
| 5-cost: Jhin | Destroyer Dark Star Sniper |
| 2-cost: Gragas | Psionic Brawler |
| 4-cost: The Mighty Mech | Mecha Voyager |
| 2-star 1-cost: Caitlyn | N.O.V.A. Fateweaver |
| 5-cost: Sona | Commander Psionic Shepherd |
| 2-star 1-cost: Ezreal | Timebreaker Sniper |
| 5-cost: Shen | Bulwark Bastion |
| 4-cost: Karma | Dark Star Voyager |
| 3-cost: Samira | Space Groove Sniper |
| 4-cost: Corki | Meeple Fateweaver |
| 2-cost: Pantheon | Timebreaker Brawler Replicator |
| 2 star Zac, Aurora, Urgot, and Samira | 2 star Zac, Aurora, Urgot, and Samira |
| 12 radiant items | Covalent Spark, Jak'sho the Protean, Bulwark's Oath, Spear of Hirana, Rabadon's Ascended Deathcap, Glamorous Gauntlet, Guinsoo's Reckoning, Runaan's Tempest, Luminous Deathblade, Zenith Edge, Fist of Fairness, Eternal Whisper |
| 2 Tacticians' Capes | 2 Tacticians' Capes |
| Dummy with Shroud | A training dummy with a Shroud of Stillness, an item which increases enemies' mana costs. |
| Dummy with 2 Zeke's | A training dummy with 2 Zeke's Heralds, which provides Attack Speed to nearby allies. |
| 2 3-cost champions | 2 3-cost champions |
| 5 4-cost champions | 5 4-cost champions |
| 6 5-cost champions | 6 5-cost champions |
| 2-star 2-cost champion | 2-star 2-cost champion |
| 3-star 5-cost champion | 3-star 5-cost champion |
| 7 5-cost champions | 7 5-cost champions |
| Dummy with 2 Lockets | A training dummy with 2 Lockets of The Iron Solari, an item which shields allies on combat start. |
| 3 2-cost champions | 3 2-cost champions |
| All double component items | Deathblade, Rabadon's Deathcap, Red Buff, Bramble Vest, Dragon's Claw, Warmog's Armor, Blue Buff, Thief's Gloves. |
| 2 4-cost champions | 2 4-cost champions |
| 2 Deathblades | 2 Deathblades |
| 2 Reforgers | 2 Reforgers |
| 5 1-cost champions | 5 1-cost champions |
| 3 Masterwork Upgrades | 3 Masterwork Upgrades |
| 3-cost champion | 3-cost champion |
| 2 Item Removers | 2 Item Removers |
| 2-star 3-cost champion | 2-star 3-cost champion |
| 3 3-cost champions | 3 3-cost champions |
| 3 Radiant Items | 3 Radiant Items |
| 2 Radiant Items | 2 Radiant Items |
| Golem with 2 Aegises. | A golem with 2 Aegises of the Legion, an item which provides Armor, Magic Resist, and Attack Speed to allies behind it. |
| 2-star 5-cost champion | 2-star 5-cost champion |
| 3 4-cost champions | 3 4-cost champions |
| 6 Lesser Champion Duplicators | 6 Lesser Champion Duplicators |
| 1-star 4-cost champion | 1-star 4-cost champion |
| 4 components | 4 components |
| 1-star 5-cost champion | 1-star 5-cost champion |
| 2 2-star 2-cost champions | 2 2-star 2-cost champions |
| 9 components | 9 components |
| 2-star 4-cost champion | 2-star 4-cost champion |
| 3 5-cost champions | 3 5-cost champions |
| 4 2-cost champions | 4 2-cost champions |
| 4 5-cost champions | 4 5-cost champions |
| 2 Champion Duplicators | 2 Champion Duplicators |
| 3 Spatulas | 3 Spatulas |
| Tiny Champion Duplicator | Use on a 1-cost champion to create a 1-star copy of them. |
| Tome of Traits and Reforger | Tome of Traits and Reforger |
| Tactician's Crown | Tactician's Crown |
| Tactician's Shield | Tactician's Shield |
| Thief's Gloves | Thief's Gloves |
| Spatula | Spatula |
| Masterwork Upgrade | Masterwork Upgrade |
| 9 gold | 9 gold |
| 8 gold | 8 gold |
| Tactician's Cape | Tactician's Cape |
| 1 gold | 1 gold |
| 7 Gold | 7 Gold |
| 6 gold | 6 gold |
| 8 Rerolls | 8 Rerolls |
| 5-cost champion and 10 Gold | 5-cost champion and 10 Gold |
| Reforger and 5 Gold | Reforger and 5 Gold |
| Support Anvil | Support Anvil |
| Spatula and Component Anvil | Spatula and Component Anvil |
| Artifact Item | Artifact Item |
| 11 Rerolls | 11 Rerolls |
| 2 Completed Items and Reforger | 2 Completed Items and Reforger |
| 1 Training Dummy | 1 Training Dummy |
| Loaded Dice | Loaded Dice |
| 2 Lesser Champion Duplicators | 2 Lesser Champion Duplicators |
| 3 Lesser Champion Duplicators | 3 Lesser Champion Duplicators |
| Remover and 5 Gold | Remover and 5 Gold |
| Completed Item Anvil | Completed Item Anvil |
| Spatula and 5 Gold | Spatula |
| 10 gold | 10 gold |
| 2 Components | 2 Components |
| 4 4-cost champions | 4 4-cost champions |
| Radiant Item | Radiant Item |
| Component Anvil | Component Anvil |
| Completed Item | Completed Item |
| Lesser Rune of Allegiance | Place on a Tier 1, 2, or 3 champion to transfer them to your partner. Any items held will pop off. Once used, this item regenerates in 5 rounds. At 4-1 it upgrades into a Rune of Allegiance. [Cannot be used in combat.] [CONSUMABLE - This item disappears when used.] |
| 3-star 1-cost champion and 7 gold | 3-star 1-cost champion and 7 gold |
| 4-cost Champion and 4 Gold | 4-cost Champion and 4 Gold |
| Artifact Anvil | Artifact Anvil |
| Rune of Allegiance | Use on any champion to transfer them to your partner. Any items held will pop off. Once used, this item regenerates in 7 rounds. [Cannot be used in combat.] [Consumable - this item disappears when used.] |
| Champion Duplicator and 10 Gold | Champion Duplicator and 10 Gold |
| Champion Duplicator | Champion Duplicator |
| Magnetic Remover (8 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (9 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Reforger | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Magnetic Remover (2 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Reforger (10 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Magnetic Remover (3 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (4 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (5 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (6 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover (7 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Magnetic Remover | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Radiant Lucky Item Chest | Use on a champion to open an armory of Radiant items especially suited for them. These items are based on the champion's recommended items. |
| Reforger | Unequip all items and randomly transform them into new ones of a similar type and quality. Trash to Treasure: Completed items transform into Artifacts and do not consume the Reforger [Consumable - This item disappears when used.] |
| Champion Duplicator | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Pocket Recombobulator | Use on a 1 or 2 cost champion to upgrade them to a unit of 1 higher cost. Not for use on living, mechanical, experimental, imaginary, or reanimated creatures . Keep out of reach of Yordles. |
| Masterwork Upgrade | Use on a champion to choose a Radiant version of that champion's craftable completed items. Upgrade the item you choose to its Radiant equivalent and unequip it. |
| Training Dummy | Use on a champion to spawn a movable Training Dummy nearby. [Cannot be used in combat. Consumable - This item disappears when used.] |
| Loaded Dice | Use on a champion to reroll a special Shop that contains only champions sharing a Trait with the source champion. [Consumable - This item disappears when used.] |
| Magnetic Remover (10 uses left!) | Use on a champion to unequip all items. [Cannot be used on champions in combat.] [Consumable - This item disappears when used.] |
| Lucky Item Chest | Use on a champion to open an armory of items especially suited for them. These items are based on the champion's recommended items. |
| Reforger (5 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (4 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (7 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (6 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (3 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (2 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (9 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
| Reforger (8 uses left!) | Unequip all items and randomly transform them into new ones of a similar type and quality. [Consumable - This item disappears when used.] |
