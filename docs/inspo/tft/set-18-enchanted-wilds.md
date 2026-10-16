# TFT Set 18: Enchanted Wilds

Source: MetaTFT lookup `https://data.metatft.com/lookups/TFTSet18_latest_en_us.json` (metadata: release rls-18.4 PBE build, generated 2026-10-06; retrieved 2026-10-15) for champions, abilities, traits, items, augments, Wisps and encounters. Set 18 moved to Unreal Engine and CommunityDragon `https://raw.communitydragon.org/latest/cdragon/tft/en_us.json` (TFTSet18) stores names and base stats but no ability variables, so MetaTFT is the primary source here; CDragon base stats were cross-checked for the first unit. Mechanic text: Riot "Teamfight Tactics patch 18.1" notes (teamfighttactics.leagueoflegends.com), mobalytics.gg "Set 18 mechanic: Wisps", invenglobal.com (Unreal Engine transition). Ability values listed as "(scales with AttackDamage)" are the stored coefficients; the data does not state the unit (they are likely percent of AD). Values marked "unknown" sit between curve keys that the data does not define.

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 18 |
| Name | Enchanted Wilds |
| Riot launch patch | TFT 18.1 |
| League client version at launch (CDragon path) | 16.17 |
| Live dates | August 26, 2026 (patch notes published August 25) to present |
| Theme | A living, magical forest (Elderwood, Fae, Coven, Blossom, Lunar, Solar, Primal, Riftbeast, Thornmaiden). First TFT set on Unreal Engine. |
| Data snapshot used | MetaTFT PBE 18.4 data (October 6, 2026) |
| Champion count in this file | 67 (includes the Lux trait variants) |

## Set mechanics and system changes

- Wisps: forest spirits sold in the rightmost shop slot during the planning phase; they disappear after planning and reveal the fifth unit behind them. Wisps appear in every other shop and only one can be bought per round. They are split into 7 categories: Champion, Combat, Misc, Shop, Gold/XP, Risky, Item. Later Wisps are stronger and cost more. After stage 5 every other Wisp is guaranteed to be a Combat Wisp. Wisps are an iteration on Set 12 Charms.
- The carousel returned (it had been replaced by Realm of the Gods in Set 17), with higher odds of higher-cost champions.
- Set 17 Space Gods stays playable alongside Set 18.
- Targeting: units no longer retarget after coming out of crowd control.
- PvE loot missed at 4-7 or later PvE rounds overflows to the next PvE round.
- Opening encounters were overhauled: high-variance encounters removed; standardized options added (Completed Item Anvil start, Reroll Subscription, Item Forge).
- Avatar trait (Lux): Lux has nine trait variants; owning an Avatar converts other Avatars in your shop to its trait, and the chosen trait counts twice. Elder Dragon takes two team slots and gives +2 Riftbeast.
- Coven gathers Essence from kills and losses and converts it into rewards at thresholds; Blackthorn sacrifices the unit on the Blackthorn hex before combat for team HP. See traits.
- 4-star champion values exist in the data (curve column 4); this file shows 1 to 3 star values.

## Champions (67)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Akali | 1 | Inferno, Adaptor, Ravager | 650 | 35 | 35 | 40 | 0.75 | 1 | 0/25 |
| Camille | 1 | Coven, Ravager | 700 | 40 | 40 | 40 | 0.75 | 1 | 0/25 |
| Cinderling | 1 | Riftbeast, Hunter | 500 | 25 | 25 | 40 | 0.7 | 4 | 0/50 |
| Karma | 1 | Blossom, Spellweaver | 500 | 25 | 25 | 25 | 0.7 | 4 | 0/40 |
| Kobuko | 1 | Sprykin, Brawler | 700 | 40 | 40 | 55 | 0.65 | 1 | 30/90 |
| Leona | 1 | Solar, Defender | 750 | 40 | 40 | 50 | 0.65 | 1 | 30/90 |
| Ornn | 1 | Elderwood, Defender | 700 | 40 | 40 | 40 | 0.6 | 1 | 40/100 |
| Pebbles | 1 | Riftbeast, Invoker | 500 | 25 | 25 | 35 | 0.8 | 4 | 30/70 |
| Rakan | 1 | Fae, Juggernaut, Vanguard | 700 | 35 | 35 | unknown | 0.6 | 1 | 35/105 |
| Rek'Sai | 1 | Blackthorn, Brawler | 700 | 40 | 40 | 50 | 0.55 | 1 | 40/100 |
| Varus | 1 | Inferno, Rapidfire | 500 | 25 | 25 | 40 | 0.75 | 4 | 30/120 |
| Veigar | 1 | Blackthorn, Sprykin, Spellweaver | 500 | 25 | 25 | 25 | 0.7 | 4 | 0/30 |
| Xayah | 1 | Elderwood, Fae, Rapidfire | 450 | 25 | 25 | 45 | 0.7 | 4 | 0/50 |
| Yorick | 1 | Blossom, Juggernaut, Summoner | 650 | 35 | 35 | 50 | 0.6 | 1 | 50/110 |
| Alistar | 2 | Elderwood, Brawler | 950 | 45 | 45 | 50 | 0.6 | 1 | 30/90 |
| Caitlyn | 2 | Coven, Hunter | 550 | 30 | 30 | 50 | 0.7 | 4 | 0/3 |
| Elise | 2 | Coven, Vanguard | 900 | 45 | 45 | 20 | 0.65 | 1 | 20/70 |
| Elise | 2 | Coven, Vanguard | 900 | 45 | 45 | 20 | 0.65 | 1 | 20/70 |
| Gromp | 2 | Riftbeast, Adaptor | 550 | 30 | 30 | 45 | 0.7 | 4 | 0/45 |
| Kayle | 2 | Solar, Rapidfire | 550 | 30 | 30 | 10 | 0.75 | 4 | 0/0 |
| LeBlanc | 2 | Elderwood, Spellweaver | 550 | 30 | 30 | 30 | 0.7 | 4 | 0/40 |
| Murkwolf | 2 | Riftbeast, Ravager | 700 | 40 | 40 | 50 | 0.75 | 1 | 0/40 |
| Scuttlecrab | 2 | Riftbeast, Juggernaut | 950 | 45 | 45 | 50 | 0.65 | 1 | 40/100 |
| Sejuani | 2 | Solar, Juggernaut | 900 | 45 | 45 | 50 | 0.6 | 1 | 40/100 |
| Shen | 2 | Inferno, Defender | 900 | 45 | 45 | 50 | 0.65 | 1 | 30/90 |
| Teemo | 2 | Sprykin, Invoker | 550 | 30 | 30 | 35 | 0.75 | 4 | 0/50 |
| Warwick | 2 | Blackthorn, Ravager | 750 | 45 | 45 | 45 | 0.75 | 1 | 0/40 |
| Yunara | 2 | Blossom, Executioner | 550 | 30 | 30 | 42 | 0.75 | 4 | 0/35 |
| Azir | 3 | Blackthorn, Executioner, Summoner | 650 | 35 | 35 | 30 | 0.75 | 4 | 0/35 |
| Cassiopeia | 3 | Coven, Spellweaver | 650 | 35 | 35 | 30 | 0.75 | 4 | 0/30 |
| Diana | 3 | Lunar, Ravager, Vanguard | 850 | 50 | 50 | 30 | 0.8 | 1 | 0/40 |
| Fiddlesticks | 3 | Flora Fatalis, Defender, Spellweaver | unknown | 55 | 55 | 50 | 0.65 | 1 | 30/90 |
| Hecarim | 3 | Elderwood, Vanguard | 1100 | 50 | 50 | 50 | 0.65 | 1 | 50/110 |
| Kha'Zix | 3 | Rival | 950 | 55 | 55 | 40 | 0.85 | 1 | 0/25 |
| Kog'Maw | 3 | Caustic, Adaptor, Invoker | 650 | 35 | 35 | unknown | 0.8 | 4 | 15/55 |
| Krug | 3 | Riftbeast, Brawler | 1100 | 40 | 40 | 55 | 0.6 | 1 | 80/135 |
| Mama Beak | 3 | Riftbeast, Summoner, Rapidfire | 650 | 35 | 35 | 55 | 0.75 | 4 | 20/60 |
| Master Yi | 3 | Blossom, Adaptor | 850 | 55 | 55 | 60 | 0.8 | 1 | 0/3 |
| Rammus | 3 | Sprykin, Defender | 1100 | 50 | 50 | 50 | 0.6 | 1 | 30/80 |
| Rengar | 3 | Rival | 900 | 55 | 55 | 60 | 0.75 | 1 | 10/50 |
| Tristana | 3 | Fae, Sprykin, Hunter | 650 | 35 | 35 | 55 | 0.75 | 4 | 0/60 |
| Vi | 3 | Primal, Juggernaut | 1100 | 50 | 50 | 45 | 0.75 | 1 | 0/60 |
| Ahri | 4 | Blossom, Spellweaver | 850 | 40 | 40 | 40 | 0.8 | 4 | 20/100 |
| Amumu | 4 | Inferno, Juggernaut | 1300 | 60 | 60 | 70 | 0.7 | 1 | 30/125 |
| Aphelios | 4 | Lunar, Rapidfire | 850 | 40 | 40 | 60 | 0.8 | 4 | 20/70 |
| Brambleback | 4 | Riftbeast, Ravager | 1100 | 65 | 65 | 120 | 0.55 | 1 | 0/40 |
| Ezreal | 4 | Elderwood, Executioner | 850 | 40 | 40 | 45 | 0.75 | 4 | 0/30 |
| Lillia | 4 | Fae, Defender | 1300 | 60 | 60 | 40 | 0.65 | 1 | 40/140 |
| Malphite | 4 | Blackthorn, Monolith | 1300 | 70 | 70 | 70 | 0.55 | 1 | 30/80 |
| Morgana | 4 | Coven, Invoker | 1100 | 60 | 60 | 45 | 0.75 | 2 | 0/65 |
| Nidalee | 4 | Primal, Adaptor | 850 | 30 | 30 | 35 | 0.8 | 1 | 0/45 |
| Sentinel | 4 | Riftbeast, Vanguard, Invoker | 1300 | 60 | 60 | 75 | 0.6 | 1 | 60/150 |
| Sett | 4 | Blossom, Brawler | 1200 | 60 | 60 | 80 | 0.65 | 1 | 60/135 |
| Sivir | 4 | Primal, Hunter | 850 | 40 | 40 | 50 | 0.8 | 4 | 0/40 |
| Soraka | 4 | Flora Fatalis, Executioner | 850 | 40 | 40 | 30 | 0.75 | 4 | 0/30 |
| Zyra | 4 | Thornmaiden, Summoner | 850 | 40 | 40 | 40 | 0.75 | 4 | 0/45 |
| Alune | 5 | Attuned, Lunar, Spellweaver | 900 | 45 | 45 | 40 | 0.8 | 4 | 0/35 |
| Ashe | 5 | Blossom, Hunter | 900 | 45 | 45 | 75 | 0.8 | 5 | 0/80 |
| Draven | 5 | Bounty Seeker | 900 | 45 | 45 | 48 | 0.85 | 5 | 0/110 |
| Elder Dragon | 5 | Apex Predator, Riftbeast | 1800 | 75 | 75 | 125 | 0.5 | 2 | 20/60 |
| Gnar | 5 | Elderwood, Sprykin, Brawler | 900 | 55 | 55 | unknown | 0.9 | 2 | 0/70 |
| Gnar | 5 | Elderwood, Sprykin, Brawler | 900 | 55 | 55 | unknown | 0.9 | 2 | 0/70 |
| Ivern | 5 | Greenfather | unknown | 55 | 55 | 60 | 0.8 | 3 | 50/80 |
| Kennen | 5 | Inferno, Executioner | 1200 | 60 | 60 | 50 | 0.85 | 2 | 0/40 |
| Lux | 5 | Avatar | 900 | 40 | 40 | 40 | 0.8 | 5 | 20/70 |
| Maokai | 5 | Old Growth, Juggernaut | 1150 | 60 | 60 | 80 | 0.65 | 1 | 30/100 |
| Taric | 5 | Emerald Aspect, Vanguard | 1300 | 65 | 65 | 55 | 0.75 | 1 | 0/65 |

### Abilities

**Akali** (1-cost) - Kunai Strike: Adaptor [: unknown]: Throw a volley of kunai at the target, dealing 155/235/405 (scales with AttackDamage, AbilityPower) physical damage. If the target is Burning, deal an additional 35/52/80 (scales with AttackDamage) damage. Adaptor : If the Ability kills, cast it again.

**Camille** (1-cost) - Defensive Sweep: Slice the target for 160/240/400 (scales with AttackDamage, AbilityPower) physical damage and gain 60/90/200 Shield for 2 seconds.

**Cinderling** (1-cost) - Razor Leaves: Summon five razor-sharp leaves that converge on the current target, dealing a total of 340/510/770 (scales with AttackDamage, AbilityPower) physical damage and applying 20% Wound and 1% Burn for 4 seconds. Scarlet Buff: Cinderling gains 22% Attack Damage each cast. Wound: Reduces healing received. Burn: Deals a percent of the target’s max Health as true damage every second.

**Karma** (1-cost) - Karmic Bond: Tether the current target, dealing 280/420/630 (scales with AbilityPower) magic damage over 1.5 seconds. Then release a burst of power around them, dealing 125/185/300 (scales with AbilityPower) magic damage to all enemies in a 1 Hex radius and 30% Slowing them for 2 seconds. Slow: Reduce Attack Speed

**Kobuko** (1-cost) - Dance of Life: Restore 334/384/529 (scales with AbilityPower, HealthMax) Health over 2 seconds. The next attack is replaced with a bash that deals 140/175/220 (scales with HealthMax, AbilityPower) magic damage.

**Leona** (1-cost) - Shield Bash: Passive: Start combat with 60/80/100 (scales with AbilityPower) bonus Armor and Magic Resist that decays over 12 seconds. Active: Bash the current target, dealing 60/108/162 (scales with Armor) magic damage and Stunning them for 1.5 seconds.

**Ornn** (1-cost) - Bellows Breath: Active: Gain 400/460/550 (scales with AbilityPower) Shield for 4 seconds and deal 200/300/450 (scales with AbilityPower) magic damage to enemies in a cone. Quest: Each player combat, Ornn stores damage blocked as Forge Power, increased by 1.85 at 3-star. Gain an Artifact Anvil each time he gains enough Forge Power. (Forge Power: [Stack: unknown] / [GenericCalc1: unknown])

**Pebbles** (1-cost) - Azure Laser: Begin consuming 0.35 max Mana per second and channeling a laser on the target. Each second while casting, deal 160/240/360 (scales with AbilityPower) magic damage to them and reduce their Magic Resist by 3. Teal Buff: Gain 2 Mana Regen for every 4 seconds channeled.

**Rakan** (1-cost) - Entrancing Dance: Gain 290/340/435 (scales with AbilityPower) Shield for 4 seconds. Then grant the ally who has dealt the most damage this combat 1.9/2/2.3 (scales with AbilityPower) decaying Attack Speed for 4 seconds.

**Rek'Sai** (1-cost) - Uproot: Passive: Restore 17/21/27 (scales with HealthCalc1) Health each second, tripled for 3 seconds after casting. Active: Lunge out of the ground, Stunning adjacent enemies for 1 second and dealing 70/105/160 (scales with AbilityPower) magic damage to them.

**Varus** (1-cost) - Piercing Arrow: Wind up, then fire an arrow at the most enemies in line with the target. It deals 445/670/1070 (scales with AttackDamage, AbilityPower) physical damage to enemies hit, reduced by 40% for each enemy it passes through (minimum 40%).

**Veigar** (1-cost) - Primordial Burst: Launch a giant blast at the target that deals 190/285/430 (scales with AbilityPower) magic damage, increased to 285/430/640 (scales with AbilityPower) if they're below 30% max Health. If they die, permanently gain 2% Ability Power. Current bonus: [Stack: unknown]% [: unknown]

**Xayah** (1-cost) - Deadly Plumage: Gain 50% Attack Speed for the next 5 attacks. These attacks are replaced with feathers that deal 73/108/165 (scales with AttackDamage) physical damage and reduce Armor by 2 (scales with AbilityPower).

**Yorick** (1-cost) - Last Rites: Passive: On death, spawn a Spirit Walker with 330/530/880 (scales with HealthMax) max Health which immediately taunts, forcing enemies to attack it. Active: Restore 280/325/435 (scales with AbilityPower) Health and strike the target, dealing 150/225/340 (scales with AbilityPower) physical damage.

**Alistar** (2-cost) - Triumphant Roar: Roar, restoring 276/336/396 (scales with HealthCalc3, HealthMax) Health, cleansing disables, and healing the two lowest percent Health allies for 80/105/130 (scales with AbilityPower). Then slam the current target, dealing 100/150/225 (scales with AbilityPower) magic damage and Stunning them for 1.5 seconds.

**Caitlyn** (2-cost) - Headshot: Passive: Every third attack is replaced with a Headshot that deals 210/315/515 (scales with AttackDamage, AbilityPower) physical damage.

**Elise** (2-cost) - Spider Queen: Transform into a spider and gain 375/475/725 max Health. Attacks while in Spider Form deal 35/50/80 (scales with AbilityPower) bonus magic damage and heal for 55/90/170 (scales with AbilityPower). Subsequent casts grant 175% decaying Attack Speed for 4 seconds.

**Elise** (2-cost) - Spider Queen: Transform into a spider and gain 375/475/725 max Health. Attacks while in Spider Form deal 35/50/80 (scales with AbilityPower) bonus magic damage and heal for 55/90/170 (scales with AbilityPower). Subsequent casts grant 175% decaying Attack Speed for 4 seconds.

**Gromp** (2-cost) - Belchy Bubble: Adaptor [: unknown]: Belch a noxious bubble at the current target that explodes on the first enemy hit, dealing 225/340/535 (scales with AbilityPower) magic damage. Enemies within a 1 hex radius of the explosion take 160/240/375 (scales with AbilityPower) magic damage over 3 seconds. Adaptor [: unknown]: Heavily slows the target. Purple Buff: Every 5 seconds, Gromp gains 30% Ability Power.

**Kayle** (2-cost) - Solar Judgement: Passive: Kayle ascends based on her star level, granting her stacking bonuses. 1st Ascension: Attacks deal 65/97/110 (scales with AbilityPower) bonus magic damage. 2nd Ascension: Attacks 20% Shred enemies hit for 2 seconds. 3rd Ascension: Attacks fire waves, dealing 35 (scales with AbilityPower) magic damage to all other units hit. 4th Ascension: Gain infinite range. Every 3rd wave is much larger. Shred: Reduce Magic Resist

**LeBlanc** (2-cost) - Mirror Image: Passive: After player combat, your strongest LeBlanc has a 10%/15%/30% chance to create a copy of an ally on your board, increased by 4% per takedown. Active: Launch a mirror image at the current target, dealing 260/390/615 (scales with AbilityPower) magic damage and 100/150/230 (scales with AbilityPower) to adjacent enemies.

**Murkwolf** (2-cost) - Rending Claws: Leap to the lowest Health enemy within 3 Hexes and deal 500/1050/2250 (scales with PhysicalDamageCalc3, GenericCalc1) physical damage. The next 2 attacks gain 2.75 Attack Speed and deal 60/90/135 (scales with AttackDamage) bonus physical damage. Grey Buff: Gain Precision and 25% Critical Strike Chance, increased up to 75% based on missing Health.

**Scuttlecrab** (2-cost) - Can You Dig It?: Passive: Attacks are replaced by a dance that deals 50/75/112.5 (scales with AttackDamage) physical damage to all adjacent enemies. Active: Burrow underground, gaining 15% Durability for 3 seconds and healing 325/400/675 (scales with AbilityPower) over the duration. Green Buff: When allies fall below 50% Health, they restore 15% max Health.

**Sejuani** (2-cost) - Sun's Wrath: Gain 315/340/365 (scales with AbilityPower, HealthMax) Shield for 4 seconds. Then cleave in a cone, dealing 90/135/205 (scales with AbilityPower) magic damage and strike in a line, dealing 120/180/270 (scales with AbilityPower) magic damage to enemies hit.

**Shen** (2-cost) - Ki Barrier: Grant 375/450/575 (scales with AbilityPower) Shield to Shen and 225/300/400 (scales with AbilityPower) Shield to a nearby damaged ally for 4 seconds. Both of their next 3 attacks gain 40% Attack Speed and deal 35/55/80 (scales with AbilityPower) bonus magic damage.

**Teemo** (2-cost) - Fungus Among Us: Throw 2 clusters of mushrooms that deal 55/82/155 (scales with AbilityPower) magic damage to the 3 nearest enemies. Then hurl a giant mushroom that deals 135/200/310 (scales with AbilityPower) magic damage to the target. Each cast has a 10%/12%/15% chance to forage an extra mushroom that can be collected for bonus effects. Red: 1 Reroll (Reds Foraged: [Stack: unknown]) Green: 1 Tactician Health (Greens Foraged: [GenericCalc1: unknown]) Yellow: 2 XP (Yellows Foraged: [GenericCalc2: unknown])

**Warwick** (2-cost) - Jaws of The Beast: Bite the current target, dealing 215/325/500 (scales with AttackDamage) physical damage and healing for 0.2 (scales with AbilityPower) of the damage dealt. Gain 20% Attack Speed for the rest of combat.

**Yunara** (2-cost) - Cultivation of Spirit: Dash, then launch an orb at the current target that deals 170/255/395 (scales with AttackDamage, PhysicalDamageCalc3) physical damage and splits, dealing 59.5/89.25/138.25 (scales with PhysicalDamageCalc1) physical damage to 2 nearby enemies.

**Azir** (3-cost) - Arise!: Gain 2.5 Attack Speed and summon 2 soldiers for the next 6 attacks. These attacks are replaced by commands which direct each soldier to deal 46/69/110 (scales with AbilityPower) magic damage per attack.

**Cassiopeia** (3-cost) - Noxious Blast: Poison the target and the nearest non-poisoned enemy, dealing 420/630/1020 (scales with AbilityPower) magic damage over 15 seconds. Poisons stack.

**Diana** (3-cost) - Pale Barrier: Gain 150/275/500 shield for 2 seconds and send out 6 moonlight orbs spread among enemies within 2 hexes, each dealing 75/115/180 (scales with AbilityPower) magic damage.

**Fiddlesticks** (3-cost) - Harvest: Reduce the Magic Resist of the 3 nearest enemies by 10. Then drain life from them over 2 seconds, healing for 410/485/850 (scales with AbilityPower) Health and dealing 70/105/170 (scales with AbilityPower) magic damage to each over the duration.

**Hecarim** (3-cost) - Spirit of Dread: Gain 50 Armor and Magic Resist for 3 seconds and restore 375/475/685 (scales with AbilityPower) Health over the duration. Launch spectral riders at the 3 nearest enemies, dealing 80/120/195 (scales with AbilityPower) magic damage and Stunning them for 1.5/unknown/1.75 seconds.

**Kha'Zix** (3-cost) - Taste Their Fear: Leap to the farthest enemy within 3 Hexes, dealing 265/370/535 (scales with AbilityPower) magic damage. If they have no adjacent allies, deal 285/410/605 (scales with AbilityPower) magic damage instead and gain 10 mana.{Augment.Variant.RivalsAugment.AbilityTooltip}

**Kog'Maw** (3-cost) - Raining Artillery: Adaptor [: unknown]: Launch acid at the target and the other nearest enemy, dealing 165/250/425 (scales with AttackDamage, AbilityPower) physical damage. Enemies below 50% max Health take 231/350/595 (scales with PhysicalDamageCalc1) physical damage instead. Adaptor[: unknown]: Damages enemies over time.

**Krug** (3-cost) - Rock and Roll: Passive: On death, split into two Kruglettes with 182/232/312 (scales with HealthMax) Health which immediately taunt, forcing enemies to attack them. Active: Gain 175/225/325 (scales with AbilityPower) max Health, then roll into the target, dealing 223/288/423 (scales with AttackDamage, HealthMax) damage. Slate Buff: On death, Krug and Kruglettes Shield allies for 8% max Health.

**Mama Beak** (3-cost) - Flock Family: Summon 4 untargetable Tiny Beaks nearby for 5 (scales with AbilityPower) seconds. Whenever Mama Beak attacks, Tiny Beaks attack the same enemy, dealing 22/33/52 (scales with AttackDamage) physical damage. Orange Buff: Dealing physical damage reduces enemy Armor by 2.

**Master Yi** (3-cost) - Wuju Style: Passive: Every third attack is a Double Strike. On takedown, gain a burst of movement speed. Adaptor [: unknown]: Double Strikes grant 0.15 (scales with AbilityPower) stacking Attack Speed. Adaptor [: unknown]: Double Strikes deal bonus damage and heal.

**Rammus** (3-cost) - Defensive Ball Curl: Taunt, forcing enemies to attack this champion. For 4 seconds, gain 400/550/700 (scales with AbilityPower) Shield and 60 Armor and Magic Resist. When the Shield breaks, deal 50/75/120 (scales with Armor, MagicResist) physical damage to enemies in 2 Hexes.

**Rengar** (3-cost) - Savagery: Jump to the lowest percent Health enemy within 3 hexes and stab them, dealing 255/385/650 (scales with AttackDamage) physical damage. Then heal for 60/90/90 (scales with AbilityPower), increased to up to 120/180/180 (scales with AbilityPower) based on their missing Health.{Augment.Variant.RivalsAugment.AbilityTooltip}

**Tristana** (3-cost) - Explosive Charge: Attach an explosive charge to the target that lasts 4 seconds. While active, gain infinite range and 0.6 (scales with AbilityPower) Attack Speed. After the duration, the charge explodes and splits damage between all enemies within 2 hexes, dealing 175/265/420 (scales with AttackDamage) physical damage + 10/15/25 (scales with AttackDamage) for each attack while casting. If an enemy with an explosive charge on them dies, it attaches to Tristana's new target.

**Vi** (3-cost) - Furious Fists: Passive: On attack, restore 22 (scales with HealthMax) Health. Active: Unleash a primal roar, restoring 200/265/360 (scales with AbilityPower) Health. Then gain 1.85/2/2.25 Attack Speed, 15% Durability, and is unstoppable for 3 seconds.

**Ahri** (4-cost) - Spirit Bomb: Launch a spirit bomb at the location within 4 hexes that has the most surrounding enemies. It deals 455/685/3500 (scales with AbilityPower) magic damage to enemies in a 3 hex radius, reduced by 21% per hex away from the epicenter.

**Amumu** (4-cost) - Tantrum: Passive: Every second, restore 33/33.13/56 (scales with HealthCalc2, HealthMax) Health and deal 12/18/150 (scales with AbilityPower) magic damage to enemies within 1 hex. Active: Deal 100/150/2000 (scales with AbilityPower) magic damage to enemies within 2 hexes and Stun them for 1/1/6 seconds, increased to 2.5/2.5/12.5 (scales with GenericCalc2) seconds if the target is Burning. Burn: Deals a percent of the target’s max Health as true damage every second.

**Aphelios** (4-cost) - Moonlight's Onslaught: Equip Severum and swipe the target [GenericCalc1: unknown] times over 2 seconds, each dealing 70/105/850 (scales with AttackDamage) physical damage. After the onslaught finishes, fire a blast that deals 490/735/5225 (scales with AttackDamage, AbilityPower) physical damage split among all enemies in a 2 Hex radius.

**Brambleback** (4-cost) - Crimson Fury: Passive: When the target dies, leap at the next target, dealing 135/205/1000 (scales with AttackDamage) physical damage. Active: Gain 100%/unknown/300% Attack Damage for 8 seconds. During this time ignore 0.45/0.45/0.9 (scales with GenericCalc2) Armor. Red Buff: Attacks 1% Burn enemies and heal Brambleback for 4% max Health.

**Ezreal** (4-cost) - Forest's Flurry: Blink away from the current target, deal 270/405/1200 (scales with AttackDamage) physical damage to them, and gain 0.3/0.3/1 (scales with AbilityPower) Attack Speed. Every 4th cast consumes the Attack Speed granted from Forest's Flurry and fires a blast through the largest group of enemies that deals 385/580/2500 (scales with AttackDamage) physical damage, reduced by 25% for each enemy it passes through (minimum 50%).

**Lillia** (4-cost) - Lilting Lullaby: Restore 350/525/800 (scales with AbilityPower) Health and send 4 butterflies at nearby enemies. Enemies hit take 90/135/2000 (scales with AbilityPower) magic damage and Sleep for 1.5/1.75/8 seconds. If they take 1000 damage, they wake up and take an additional 10% max Health magic damage.

**Malphite** (4-cost) - Petrified Bark: Gain 700/850/2000 (scales with AbilityPower) Shield for 4 seconds and become petrified. When the shield breaks, unleash a wave of dark energy, dealing 82/123/3398.6 (scales with MagicResist, Armor, AbilityPower) magic damage to enemies within 2/2/3 hexes.

**Morgana** (4-cost) - Withering Curse: Passive: Gain 25% Omnivamp. Active: Fire a dark blast at 3 nearby enemies, dealing 60/90/500 (scales with AbilityPower) magic damage to them and cursing them for 4 seconds. Then, spawn a 2 Hex withering zone for the same duration that deals 33/50/500 (scales with AbilityPower) magic damage per second. Cursed enemies take 22/33/240 (scales with AbilityPower) more damage per curse.

**Nidalee** (4-cost) - Javelin Toss: Adaptor [: unknown]: Gain 150% Attack Speed for the next 3/unknown/unknown attacks. These attacks are replaced with javelins that deal 185/275/2000 (scales with AbilityPower) magic damage. The 3rd attack instead targets the furthest enemy with the least items and deals 330/500/3000 (scales with AbilityPower) magic damage. Adaptor [: unknown]: Transform into a Melee cougar Assassin that swipes enemies.

**Sentinel** (4-cost) - Azure Shockwave: Gain 480/580/2130 (scales with AbilityPower, HealthMax) Shield for 4 seconds. Slam the ground, sending a fissure in the direction of the most enemies. Enemies hit are briefly knocked up, take 100/150/3500 (scales with AbilityPower) magic damage, and are Mana Reaved for 10. Blue Buff: Each time Sentinel casts, allies gain 2 Mana Regen. Mana Reave: Increase the Mana cost of the next Ability cast.

**Sett** (4-cost) - Haymaker: Passive: Upon falling below 40% max Health the first time each combat, gain 100 mana. Active: Wind up a big punch, rapidly healing for 369/419/2144 (scales with HealthCalc2, HealthCalc3) before dealing 222/297/5072 (scales with AttackDamage, HealthMax) physical damage in a large cone.

**Sivir** (4-cost) - Boomerang Blade: Throw a large crossblade that deals [: unknown] physical damage to the current target and bounces 8 times between nearby enemies, dealing [: unknown] physical damage each bounce. When this kills an enemy, bounce 3/unknown/6 additional times.

**Soraka** (4-cost) - Starcall: Call down a star on the current target, dealing 225/335/1000 (scales with AbilityPower) magic damage. If a star has previously fallen on them, call down 3 additional stars that each deal 100/150/1000 (scales with AbilityPower) magic damage.

**Zyra** (4-cost) - Rampant Growth: Spawn 2/unknown/4 plants around the battlefield that attack the nearest enemy 10 times. Each attack deals 35/53/225 (scales with AbilityPower) magic damage.

**Alune** (5-cost) - Moonfall: Rain 9 moonshards split among the 3 nearest enemies, dealing 55/83/500 (scales with AbilityPower) magic damage each. If the moon is full, instead crash it onto the board, dealing 2350/3600/7500 (scales with AbilityPower) magic damage split among all enemies.

**Ashe** (5-cost) - Spirit Rift: Fire an arrow through the most enemies in a line that deals 465/700/1000 (scales with AttackDamage) physical damage, reduced by 80%/unknown/40% per enemy hit (minimum 20%). If no enemies are hit refund 70% of mana spent. The arrow leaves a trail for 3/unknown/20 seconds that deals 11/17/220 (scales with AttackDamage, PhysicalDamageCalc3) + 1.5%/unknown/10% max Health physical damage per second to enemies within and 20% Slows them. Slow: Reduce Attack Speed

**Draven** (5-cost) - Whirling Death: Passive: Attacks target random enemies in range and apply a bleed that deals 150/225/3000 (scales with AttackDamage) physical damage over 12 seconds. Every attack has a 0.12 (scales with AbilityPower) chance to deal 1.5/unknown/10 bonus physical damage and apply 2 bleeds instead. Active: Throw two giant axes towards the enemy with the most bleeds. Deal 100/150/400 (scales with AttackDamage) physical damage to enemies hit and consume their bleed, instantly dealing the remaining damage. The axes then return, dealing 30/45/100 (scales with AttackDamage) physical damage to enemies hit. If no enemies are hit refund 50% of mana spent.

**Elder Dragon** (5-cost) - Heat Without Equal: Passive: Attacks deal 75%/unknown/100% damage to enemies adjacent to the target. Active: Become invulnerable and fly into the air. On return, stun all enemies for 1.25/unknown/99 seconds, gain 15%/unknown/100% Omnivamp, and Ignite them for 3/unknown/99 seconds. Ignited enemies take 2%/unknown/20% max Health physical damage per second. Replace this ability with Flame Breath and immediately cast it. Flame Breath: Spew fire at the most enemies in a line through the target, dealing 305/465/7200 (scales with AttackDamage, AbilityPower) physical damage, reduced by 20% for each enemy it passes through (minimum 20%.) Ignite those enemies for 3/unknown/99 seconds. Elder Dragon Buff: Damaging an enemy below 12% Health executes them.

**Gnar** (5-cost) - Rage Gene: Passive: Gain 5/unknown/50 Rage per second and 5/unknown/20 Rage per attack. Active: Transform into Mega Gnar and leap into the largest group of enemies within 3 hexes. Then, deal 100/150/2000 (scales with AttackDamage) physical damage to enemies within 2/unknown/10 hexes, reduce their Armor and Magic Resist by 15/20/250 (scales with AbilityPower), and Stun them for 1/unknown/15 second. Mega Gnar gains 400/600/15000 Health, and replaces his Ability with Grab n' Throw. Grab n' Throw: Throw the target at the farthest enemy, dealing 380/570/9000 (scales with AttackDamage) physical damage to them and 135/205/4500 (scales with AttackDamage) physical damage to enemies they pass through. If there's only 1 enemy left, throw them off the battlefield instead.

**Gnar** (5-cost) - Rage Gene: Passive: Gain 5/unknown/50 Rage per second and 5/unknown/20 Rage per attack. Active: Transform into Mega Gnar and leap into the largest group of enemies within 3 hexes. Then, deal 100/150/2000 (scales with AttackDamage) physical damage to enemies within 2/unknown/10 hexes, reduce their Armor and Magic Resist by 15/20/250 (scales with AbilityPower), and Stun them for 1/unknown/15 second. Mega Gnar gains 400/600/15000 Health, and replaces his Ability with Grab n' Throw. Grab n' Throw: Throw the target at the farthest enemy, dealing 380/570/9000 (scales with AttackDamage) physical damage to them and 135/205/4500 (scales with AttackDamage) physical damage to enemies they pass through. If there's only 1 enemy left, throw them off the battlefield instead.

**Ivern** (5-cost) - Triggerseed: Passive: Shields from this Ability can critically strike with Precision. Active: Grant 2/unknown/10 allies [ShieldCalc1: unknown] Shield and 10%/15%/100% Damage Amp for 6 seconds. Then deal 155/235/2000 (scales with AbilityPower) magic damage to enemies adjacent to them. After casting 6 times, also grant 2 stacking Attack Speed to targets.

**Kennen** (5-cost) - Firestorm: Charge up, gaining 15% Ability Power per Burning enemy. Then, gain 230/300/3000 (scales with AbilityPower) Shield for 2.5 seconds and rush through a nearby group of enemies, dealing 80/120/1500 (scales with AbilityPower) magic damage to each. After rushing, unleash a 2 Hex firestorm that deals a total of 475/715/5000 (scales with AbilityPower) magic damage split among all enemies hit over 2 seconds.

**Lux** (5-cost) - Final Spark: Passive: On cast, all allies that share a trait with Lux gain 3/3/100 (scales with AbilityPower) mana. Active: Fire a laser towards the largest group of enemies that deals 375/565/6500 (scales with AbilityPower) magic damage, reduced by 25%/unknown/10% for each enemy hit.

**Maokai** (5-cost) - Sow the Seeds: Passive: After every 650/unknown/300 damage blocked, a sapling jumps towards a nearby enemy and deals 80.5/80.5/862.5 (scales with HealthMax) magic damage. On death, 3/5/10 saplings jump out. Active: Deal 69/69/1150 (scales with HealthMax) magic damage to target and restore 330/400/2000 (scales with AbilityPower) + 10%/unknown/100% missing Health.

**Taric** (5-cost) - Emerald Radiance: Passive: The first time Taric or his paired ally drops below 50% Health, unleash emerald energy from both of them, granting 270/345/11300 (scales with HealthMax) Shield to allies within 3 Hexes for 3/unknown/99 seconds. Active: Restore 275/450/3000 (scales with AbilityPower) Health. Taric and his paired ally deal 100/150/1000 (scales with AbilityPower) bonus magic damage on their next 2 attacks.

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): The Golden Egg, The Tower, Golem, Azir, Tiny Beak, Kruglette, , , Nunu and Willump, Spirit Walker, , Lifebloom, Deepwood Protector, Drag a Sprykin over BFF and hold to set them as the rider!, Drag a Sprykin over BFF and hold to set them as the rider!, Rift Scuttler, Training Dummy, Voidspawn, Krug, Murk Wolf, Murkwolf, Razorbeak, Rift Herald, Elder Dragon, Tome of Traits, Completed Item Anvil, Component Anvil, Artifact Item Anvil, Support item anvil, Mercenary Chest.

## Traits (35)

### Adaptor

Innate: Adaptor abilities change depending on if their Attack Damage or Ability Power is higher. Adaptors gain Ability Power or Attack Damage, depending on which is higher.

| Units | Effect |
|---|---|
| 2 | 25% AD OR AP |
| 3 | 35% AD OR AP |
| 4 | 50% AD OR AP |

Champions: Akali (1), Gromp (2), Kog'Maw (3), Master Yi (3), Nidalee (4)

### Apex Predator

Elder Dragon takes up 2 team slots and grants +2 to the Riftbeast trait.

Champions: Elder Dragon (5)

### Attuned

Your strongest Alune cycles to a new phase of the moon after each cast. While the moon is at or below half full, your team gains 7% Durabilty. While above, your team gains 7% Damage Amp.

Champions: Alune (5)

### Avatar

Having an Avatar on your bench or board transforms all other Avatars in your shop to the Trait of that Avatar. An Avatar's chosen Trait is counted twice for Trait bonuses.

Champions: Lux (5)

### Blackthorn

The ally on the Blackthorn hex is sacrificed before combat, granting your champions Health. Blackthorn champions gain additional stats based on the sacrifice's Role, Star Level, and Cost.

| Units | Effect |
|---|---|
| 2-3 | 175 HP |
| 4-5 | 350 HP; Bonus is unknown stronger |
| 6 | 600 HP; Bonus is 60% stronger |

Champions: Rek'Sai (1), Veigar (1), Warwick (2), Azir (3), Malphite (4)

### Blossom

After combat, your Wisps are empowered. Blossom champions gain Attack Damage, Ability Power, and 1.1 max Health.

| Units | Effect |
|---|---|
| 3-4 | Wisps are upgraded, 12% AD+AP |
| 5-6 | Wisps are in every shop, 30% AD+AP |
| 7-8 | Gain 4 gold after buying a Wisp, 50% AD+AP |
| 9-10 | You can buy 2 Wisps per round, 55% AD+AP |
| 11 | Wisps overflow with power, 100% AD+AP |

Champions: Karma (1), Yorick (1), Yunara (2), Master Yi (3), Ahri (4), Sett (4), Ashe (5)

### Bounty Seeker

Choose a bounty. Your strongest Draven can progress on the bounty to earn its reward. Upon completing a bounty, choose another. {TraitTags.DravenUniqueTrait.BountyDescription}{TraitTags.DravenUniqueTrait.Tracker}

Champions: Draven (5)

### Brawler

Your team gains 120 max Health. Brawlers gain more.

| Units | Effect |
|---|---|
| 2-3 | 25% HP |
| 4-5 | 40% HP |
| 6 | 65% HP |

Champions: Kobuko (1), Rek'Sai (1), Alistar (2), Krug (3), Sett (4), Gnar (5), Gnar (5)

### Caustic

Caustic damage 30% Shreds and Sunders enemies for 4 seconds. Shred: Reduce Magic Resist Sunder: Reduce Armor

Champions: Kog'Maw (3)

### Coven

Gather Essence by killing champions, plus more by losing player combat. Beginning at 40 Essence, choose to convert Essence to rewards or continue gathering Essence. Each reward requires increasing amounts of Essence.

| Units | Effect |
|---|---|
| 3 | 2 Essence per kill, 22 Essence per loss |
| 4 | 2 per kill, 28 per loss |
| 5-6 | 3 per kill, 35 per loss |
| 7 | 10 per kill, 60 per loss |

Champions: Camille (1), Caitlyn (2), Elise (2), Elise (2), Cassiopeia (3), Morgana (4)

### Defender

Your team gains 12 Armor and Magic Resist. Defenders gain more.

| Units | Effect |
|---|---|
| 2-3 | 25 ArmorMR |
| 4-5 | 60 ArmorMR |
| 6 | 115 ArmorMR |

Champions: Leona (1), Ornn (1), Shen (2), Fiddlesticks (3), Rammus (3), Lillia (4)

### Elderwood

Gain placeable Elderwood plants.

| Units | Effect |
|---|---|
| 3-4 | A Stonebark Tree and a Lifebloom |
| 5-6 | A second Stonebark Tree and Stonebark Trees gain 200 Health |
| 7-8 | and the Deepwood Protector |
| 9-10 | Plants star up to 2-star |
| 11 | Plants star up to 3-star and the forest comes to life |

Champions: Ornn (1), Xayah (1), Alistar (2), LeBlanc (2), Hecarim (3), Ezreal (4), Gnar (5), Gnar (5)

### Emerald Aspect

Drag an ally onto your strongest Taric to pair them, granting the ally powerful bonuses from Taric's Ability.

Champions: Taric (5)

### Executioner

| Units | Effect |
|---|---|
| 2 | Executioners gain Precision and 20% Critical Strike Chance. |
| 3 | Additionally, enemies bleed for 25% bonus true damage over 3 seconds. |
| 4 | Bleed damage increased to 35%. |

Champions: Yunara (2), Azir (3), Ezreal (4), Soraka (4), Kennen (5)

### Fae

Your team's damage, healing, and shielding attracts Pixies. Each Pixie grants Fae champions Attack Damage and Ability Power, and after they fall below 50% Health, they heal for each Pixie.

| Units | Effect |
|---|---|
| 2-3 | 4.5% ADAP and 2.5% Heal. |
| 4 | 7% ADAP and 4% Heal. After attracting 7 Pixies, start attracting Golden Pixies instead which grant gold. |

Champions: Rakan (1), Xayah (1), Tristana (3), Lillia (4)

### Flora Fatalis

Flora Fatalis champions harvest enemies on takedown, gaining:

| Units | Effect |
|---|---|
| 1 | 10 Mana |
| 2 | and grant a 0.08 max Health heal to the lowest Health ally. |

Champions: Fiddlesticks (3), Soraka (4)

### Greenfather

Gain 1 seed when your strongest Ivern casts, plus 3 every combat. Ivern uses 5 seeds to grow a hex on your board that grants a bonus to its occupant. Seeds: [Stack: unknown] / 5 Current Biome:

Champions: Ivern (5)

### Hunter

Hunters gain Attack Damage. If a Hunter hasn't swapped targets for 3 seconds, they gain 10% Damage Amp.

| Units | Effect |
|---|---|
| 2 | 20% AD |
| 3 | 30% AD |
| 4 | 45% AD |
| 5 | 60% AD |

Champions: Cinderling (1), Caitlyn (2), Tristana (3), Sivir (4), Ashe (5)

### Inferno

Inferno damage Burns and Wounds enemies for 3 seconds. Inferno Burns stack with other Burns.

| Units | Effect |
|---|---|
| 2 | 1% Burn, 33% Wound |
| 3-4 | After combat, 1 of your shop slots without an Inferno champion ignites, rolling a champion one tier higher |
| 5-6 | Ignite 2 shop slots, 3% Burn |
| 7 | Ignite 4 shop slots, 4% Burn |

Champions: Akali (1), Varus (1), Shen (2), Amumu (4), Kennen (5)

### Invoker

Your team gains Mana Regen, increased for Invokers.

| Units | Effect |
|---|---|
| 2 | 1 Mana Regen / 3 Mana Regen |
| 3 | unknown Mana Regen / 4 Mana Regen |
| 4 | 2 Mana Regen / 6 Mana Regen |
| 5 | 2 Mana Regen / 8 Mana Regen |

Champions: Pebbles (1), Teemo (2), Kog'Maw (3), Morgana (4), Sentinel (4)

### Juggernaut

Your team gains Durability. Juggernauts gain more.

| Units | Effect |
|---|---|
| 2-3 | 4% Durability or 20% Durability |
| 4-5 | unknown Durability or 30% Durability |
| 6 | 8% Durability or 42% Durability |

Champions: Rakan (1), Yorick (1), Scuttlecrab (2), Sejuani (2), Vi (3), Amumu (4), Maokai (5)

### Lunar

Lunar champions and adjacent allies gain Attack Speed and Ability Power. Lunar champions gain 100% more.

| Units | Effect |
|---|---|
| 2 | 7% AS, 7% AP |
| 3 | 10% AS, 12% AP |
| 4 | 14% AS, 16% AP |
| 5 | 18% AS, 20% AP |

Champions: Diana (3), Aphelios (4), Alune (5)

### Monolith

Monoliths gain 10 Armor and Magic Resist for each enemy targeting them.

Champions: Malphite (4)

### Old Growth

Whenever an enemy within 3 hexes dies, your strongest Old Growth champion gains 30 permanent max Health. Current: [: unknown]

Champions: Maokai (5)

### Primal

Choose one of four Primal Blessings.

| Units | Effect |
|---|---|
| 2-3 |  |
| 4 |  |

Champions: Vi (3), Nidalee (4), Sivir (4)

### Rapidfire

Your team gains 10% Attack Speed. Rapidfire champions gain more on every attack, up to 10 stacks.

| Units | Effect |
|---|---|
| 2 | +3% AS per Attack |
| 3 | +5% AS per Attack |
| 4 | +8% AS per Attack |
| 5 | +12% AS per Attack |

Champions: Varus (1), Xayah (1), Kayle (2), Mama Beak (3), Aphelios (4)

### Ravager

Ravagers gain 10% Omnivamp. Additionally, Ravagers deal bonus damage, doubled against units below 50% Health.

| Units | Effect |
|---|---|
| 2-3 | 12% Bonus Damage |
| 4-5 | 25% Bonus Damage |
| 6 | 40% Bonus Damage |

Champions: Akali (1), Camille (1), Murkwolf (2), Warwick (2), Diana (3), Brambleback (4)

### Riftbeast

| Units | Effect |
|---|---|
| 3-4 | Use the Alpha Mark to grant a Riftbeast their unique Buff |
| 5-6 | Every 3 combats, your next shop is overrun with Riftbeasts. |
| 7-9 | On combat start and every 5 seconds Riftbeasts grow gaining 6% AD 6% AP 1.06 AS +5 Armor +5 MR +50 HP +1 Mana Regen |
| 10 | +2 maximum team size |

Champions: Cinderling (1), Pebbles (1), Gromp (2), Murkwolf (2), Scuttlecrab (2), Krug (3), Mama Beak (3), Brambleback (4), Sentinel (4), Elder Dragon (5)

### Rival

| Units | Effect |
|---|---|
| 1 | Only active while fielding 1 Rival. Rivals collect takedowns, gaining 3 if they takedown another Rival. |
| 1 | Rivals collect takedowns, gaining 3 if they takedown another Rival. Takedowns evolve Kha'Zix, permanently granting him your choice of Executioner, Rapidfire, Ravager, or Spellweaver. Rengar grants 2/3/5 gold every 8 takedowns. After 30 takedowns, your team gains 5% Attack Damage plus 0.2% AD per additional takedown. Kha'Zix Takedowns: [: unknown] / [: unknown] Rengar Takedowns: [: unknown] Gold Earned: [: unknown] |
| 2 | Rivals can be fielded together, and their abilities grant each other bonuses. |

Champions: Kha'Zix (3), Rengar (3)

### Solar

| Units | Effect |
|---|---|
| 3 | Your champions gain a 5% max Health shield and deal 8% bonus magic damage. Gain additional bonuses for each unique 3-star 3star champion: 1 3star: Increase shield and magic damage by 1% for each 3-star. 3 3star: 1.15 AS and 12 Armor+MR 5 3star: Convert 40% of the bonus magic damage to true damage. 8 3star: Every 4 seconds of combat, a 3-star champion ascends to 4-star. |

Champions: Leona (1), Kayle (2), Sejuani (2)

### Spellweaver

Your team gains 10% Ability Power. Spellweavers gain more, plus extra Ability Power whenever a Spellweaver casts an Ability.

| Units | Effect |
|---|---|
| 2-3 | 10% AP, +1% AP per cast |
| 4-5 | 30% AP, +unknown AP per cast |
| 6 | 55% AP, +2% AP per cast |

Champions: Karma (1), Veigar (1), LeBlanc (2), Cassiopeia (3), Fiddlesticks (3), Ahri (4), Alune (5)

### Sprykin

Gain the Big Furry Friend. Drop a Sprykin on the BFF to pick its Rider.

| Units | Effect |
|---|---|
| 3-4 | The Rider gains 15% HP and 15% AS |
| 5-6 | 40% HP 35% AS, and unknown of the BFF's Ability applies to your Sprykin champions. |
| 7 | 55% HP 55% AS, and 100% of the BFF's Ability applies to your Sprykin champions. |

Champions: Kobuko (1), Veigar (1), Teemo (2), Rammus (3), Tristana (3), Gnar (5), Gnar (5)

### Summoner

Summoners empower their summons in different ways. Yorick: +30% Health Azir: +20% Damage Mama Beak: +45% Damage Zyra: +4 Plant Attacks

| Units | Effect |
|---|---|
| 2 | Empower Summons |
| 3 | Improve each effect by 50% |

Champions: Yorick (1), Azir (3), Mama Beak (3), Zyra (4)

### Thornmaiden

Your team gains 5% Durability, increased to 10% if 6 or more Zyra plants are alive.

Champions: Zyra (4)

### Vanguard

At Combat Start and after dropping below 50% Health, Vanguards gain a max Health Shield for 10 seconds.

| Units | Effect |
|---|---|
| 2-3 | 0.18 max Health |
| 4-5 | 0.3 max Health |
| 6 | 0.4 max Health. Gain 5% Durability while Shielded. |

Champions: Rakan (1), Elise (2), Elise (2), Diana (3), Hecarim (3), Sentinel (4), Taric (5)

## Traits with no champion (granted by augments, items or mechanics) (2)

### Eclipse

After 8 seconds of combat, the Eclipse kills the lowest Health enemy, repeating every 3.5 seconds.

### Prowler

Nidalee's abilities Track enemies they damage. Tracked enemies take 10% more damage from abilities, and 20%% more from Nidalee's abilities.

## Items

### Components (10)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| B.F. Sword |  | 10% AD | AD +10% Attack Damage |
| Chain Vest |  | 20 Armor | Armor +20 Armor |
| Frying Pan |  |  | ...why else would it be here? |
| Giant's Belt |  | 150 HP | HP +150 Health |
| Needlessly Large Rod |  | 10% AP | AP +10% Ability Power |
| Negatron Cloak |  | 20 MR | MR +20 Magic Resist |
| Recurve Bow |  | 10% AS | AS +10% Attack Speed |
| Sparring Gloves |  | 20% Crit | Crit +20% Critical Strike Chance |
| Spatula |  |  | It must do something... |
| Tear Of The Goddess |  | 1 Mana Regen | Mana Regen +1 Mana Regeneration |

### Completed items (39)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Adaptive Helm | Tear Of The Goddess + Negatron Cloak | 20 MR 3 Mana Regen | Gain an additional 15% Mana from all sources. The wearer gains an additional bonus based on their Role: Tanks and Fighters: Gain 80 Armor and Magic Resistance. Other Roles: Gain 10% Attack Damage and Ability Power. |
| Archangel's Staff | Needlessly Large Rod + Tear Of The Goddess | 30% AP 1 Mana Regen | Combat start: Gain 20% Ability Power every 5 seconds in combat. |
| Bloodthirster | B.F. Sword + Negatron Cloak | 18 AD% 18 AP% 20 MR 20 Omnivamp% | Once per combat at 50% Health, gain a 30% max Health Shield that lasts up to 5 seconds. |
| Blue Buff | Tear Of The Goddess + Tear Of The Goddess | 15% AD 15% AP 5 Mana Regen | Gain 10% additional Attack Damage and Ability Power from all sources. |
| Bramble Vest | Chain Vest + Chain Vest | 50 Armor | Gain 6% max Health. Take 5% reduced damage from attacks. When struck by any attack, deal 100 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Crownguard | Needlessly Large Rod + Chain Vest | 100 HP 20 AP 20 Armor | Combat start: Gain a 25% max Health Shield for 8 seconds. When the shield expires, gain 20% Ability Power. |
| Deathblade | B.F. Sword + B.F. Sword | 55% AD 10% Damage Amp | Perfect peace and calm for the holder - and all who face it.{ItemTags.Deathblade.DeadlierBladeStacks} |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | 60 MR | Gain 6% max Health. Every 2 seconds, heal 2.5% max Health. |
| Edge of Night | B.F. Sword + Chain Vest | 10% AD 10% AP 20 Armor 15% AS | At 40% Health, briefly become untargetable, shed negative effects, and heal 15% missing health. |
| Evenshroud | Negatron Cloak + Giant's Belt | 20 MR 250 HP | 30% Sunder enemies within 2 hexes. Gain 15 Armor and Magic Resist for the first 15 seconds of combat. Sunder: Reduce Armor |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | 100 HP 25 Armor 25 MR | Gain 10 Armor and 10 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | Recurve Bow + B.F. Sword | 15% AD 15% AP 15% AS 15% Damage Amp | Gain 15% additional Damage Amp against Tanks. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | 10% AP 10% AS | Gain 7% stacking Attack Speed every second. |
| Hand Of Justice | Tear Of The Goddess + Sparring Gloves | 20% Crit 1 Mana Regen | Gain 2 effects: • 18% Attack Damage and 18% Ability Power. • 15% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | 20% AD 20% AP 15% Omnivamp 1 Mana Regen | Heal the lowest percent Health ally for 20% of damage dealt. Ally Healing: [HealthCalc1: unknown] |
| Infinity Edge | B.F. Sword + Sparring Gloves | 35% AD 35% Crit | Gain Precision. Precision: Ability damage can critically strike. Extra Precision grants 10% Critical Strike Damage. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | 250 HP 15% AP 35 MR | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 150% of the Mana spent. Shred: Reduce Magic Resist |
| Jeweled Gauntlet | Sparring Gloves + Needlessly Large Rod | 35% AP 35% Crit | Gain Precision. Precision: Ability damage can critically strike. Extra Precision grants 10% Critical Strike Damage. |
| Kraken's Fury | Recurve Bow + Negatron Cloak | 10% AD 20 MR 10% AS | Attacks grant 3.5% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 15% Attack Speed. |
| Last Whisper | Recurve Bow + Sparring Gloves | 15% AD 20% AS 20% Crit | Attacks and Ability damage 30% Sunders the target for 3 seconds. This effect does not stack. Sunder: Reduce Armor |
| Morellonomicon | Giant's Belt + Needlessly Large Rod | 150 HP 20% AP 1 Mana Regen | Attacks and Abilities deal 1% Burn and 33% Wound to enemies for 10 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Nashor's Tooth | Recurve Bow + Giant's Belt | 150 HP 15% AP 10% AS 20% Crit | Attacks grant 2 bonus Mana, increased to 4 if they critically strike. |
| Protector's Vow | Chain Vest + Tear Of The Goddess | 25 Armor 25 MR 1 Mana Regen | Combat Start: Gain 20 Mana. At 40% Health, gain 15 Mana and a Shield equal to 20% max Health. |
| Quicksilver | Negatron Cloak + Sparring Gloves | 20 MR 15% AS 20% Crit | Combat Start: Unstoppable for 18 seconds. Gain 3% stacking Attack Speed every second. |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | 55% AP 15% Damage Amp | This humble hat can help you make, or unmake, the world itself.{ItemTags.RabadonsDeathcap.DeadlierCapsStacks} |
| Red Buff | Recurve Bow + Recurve Bow | 45% AS 3% Damage Amp | Attacks and Abilities 1% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Spear of Shojin | Tear Of The Goddess + B.F. Sword | 15% AD 15% AP 1 Mana Regen | Attacks grant 5 bonus Mana. |
| Spirit Visage | Giant's Belt + Tear Of The Goddess | 300 HP 10% Durability 2 Mana Regen | Regenerate 2% of missing Health each second. |
| Steadfast Heart | Chain Vest + Sparring Gloves | [: unknown] 20 Armor 20% Crit | Gain 5% Durability. While above 50% Health, instead gain 15% Durability.{Augment.Variant.HeartofSteel.Tooltip} |
| Sterak's Gage | B.F. Sword + Giant's Belt | 300 HP 45% AD | At 60% Health, gain a Shield equal to 40% of the wearer's maximum Health that rapidly decays over 4 seconds. |
| Striker's Flail | Giant's Belt + Sparring Gloves | 150 HP 10% AS 20% Crit 10% Damage Amp | Critical Strikes grant 5% Damage Amp for 5 seconds, stacking up to 4 times. |
| Sunfire Cape | Chain Vest + Giant's Belt | 150 HP 20 Armor | Gain 8% max Health. Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Tactician's Cape | Spatula + Frying Pan |  | Your team gains +1 maximum team size. 10% chance to drop 1 gold after 10 seconds of combat. ...and a bit of Luck. |
| Tacticians Crown | Spatula + Spatula |  | Your team gains +1 maximum team size. 10% chance to drop 1 gold when you win combat. ...the Heart of a hero... |
| Tacticians Shield | Frying Pan + Frying Pan |  | Your team gains +1 maximum team size. 10% chance to drop 1 gold when the holder dies. Imbued with a Philosopher's wisdom... |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | 150 HP 20% Crit | Each round: Equip 2 random items [Consumes 3 item slots.] |
| Titan's Resolve | Recurve Bow + Chain Vest | 20 Armor 10% AS | Gain 2% Attack Damage and Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 10% Damage Amp and become unstoppable. |
| Void Staff | Recurve Bow + Tear Of The Goddess | 35% AP 15% AS 1 Mana Regen | Attacks and Ability damage 30% Shred the target for 5 seconds. This effect does not stack. Shred: Reduce Magic Resist |
| Warmog's Armor | Giant's Belt + Giant's Belt | 500 HP | Gain 18% max Health. |

### Emblems (21)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Blackthorn Emblem | Spatula + Giant's Belt | 250 HP 15% AD 15% AP | The holder gains the Blackthorn trait. |
| Blossom Emblem | Spatula + Needlessly Large Rod | 250 HP 10% AD 10% AP | The holder gains the Blossom trait. |
| Brawler Emblem | Frying Pan + Giant's Belt | 150 HP | The holder gains the Brawler trait. Attacks deal 2.5% of the holder's max Health as magic damage. |
| Coven Emblem |  | 150 HP | The holder gains the Coven trait. |
| Defender Emblem |  | 30 Armor 30 MR | The holder gains the Defender trait. Combat Start: Gain 6% Attack Speed for every ally that starts in the front row. |
| Elderwood Emblem | Spatula + Chain Vest | 25% AS 25 Armor 25 MR | The holder gains the Elderwood trait. |
| Executioner Emblem | Frying Pan + Sparring Gloves | 20% Crit 8% Crit Damage | The holder gains the Executioner trait. Damage executes enemies below 8% of their max Health. |
| Fae Emblem | Spatula + B.F. Sword | 200 HP 10% AD 10% AP | The holder gains the Fae trait. |
| Flora Fatalis Emblem |  | 250 HP 2 Mana Regen | The holder gains the Flora Fatalis trait. They gain 50% increased benefits from Flora Fatalis. |
| Flora Fatalis Emblem |  | 250 HP 2 Mana Regen | The holder gains the Flora Fatalis trait. |
| Hunter Emblem | Frying Pan + B.F. Sword | 25% AD | The holder gains the Hunter trait. Takedowns grant 15% Attack Damage. |
| Inferno Emblem | Spatula + Recurve Bow | 40% AS | The holder gains the Inferno trait. |
| Invoker Emblem | Frying Pan + Tear Of The Goddess | 2 Mana Regen | The holder gains the Invoker trait. On cast, gain Ability Power equal to 8% of Mana spent. |
| Juggernaut Emblem |  | 250 HP | The holder gains the Juggernaut trait. Gain 10 mana whenever a Juggernaut dies. |
| Lunar Emblem | Spatula + Tear Of The Goddess | 1.2 AS 3 Mana Regen | The holder gains the Lunar trait. |
| Primal Emblem | Spatula + Sparring Gloves | 1.35 AS 250 HP 20% Crit | The holder gains the Primal trait. |
| Rapidfire Emblem | Frying Pan + Recurve Bow | 20% AS | The holder gains the Rapidfire trait. Attacks deal 1% of the target's max Health as true damage. |
| Ravager Emblem | Frying Pan + Negatron Cloak | 20% AD 20% AP 20 Armor 20 MR | The holder gains the Ravager trait. Gain 3% Damage Amp per 300 Health restored. |
| Spellweaver Emblem | Frying Pan + Needlessly Large Rod | 25% AP | The holder gains the Spellweaver trait. Gain 2 Mana whenever an ally casts their Ability. |
| Sprykin Emblem | Spatula + Negatron Cloak | 10% AS 15 Armor 15 MR | The holder gains the Sprykin trait. If the holder is riding the BFF, gain an additional 20% AS 15 Armor 15 MR |
| Vanguard Emblem | Frying Pan + Chain Vest | 25 Armor 25 MR | The holder gains the Vanguard trait. If the holder survives 22 seconds in player combat, gain 1 player health. |

### Artifacts (Ornn) (32)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Aegis of Dawn |  | 400 HP 50 Armor | Every 2.5 seconds, steal 3 Armor from enemies within 1-hex and heal [GenericCalc1: unknown] of the holder's Armor. If Aegis of Dusk is also equipped, trigger this item's effect every 1.25 seconds instead. Healing increases based on Stage. |
| Aegis of Dusk |  | 400 HP 70 MR | Every 2.5 seconds, steal 3 Magic Resist from enemies within 1-hex and deal 18% of the holder's Magic Resist as magic damage. If Aegis of Dawn is also equipped, trigger this item's effect every 1.25 seconds instead. Damage increases based on Stage. |
| Blighting Jewel |  | 30% AP 2 Mana Regen | Dealing magic damage reduces the target’s Magic Resist by 3. If their Magic Resist is 0, grant the holder 4 Mana instead. Ability damage can only trigger on each enemy once every 0.5 seconds. |
| Cappa Juice |  | 25% AD 25% AP 25% AS | The holder dons a Hat on each takedown. The holder gains 1% Attack Damage and Ability Power per Hat. On death lose 20% of all Hats. |
| Dawncore |  | 20% AD 20% AP 1 Mana Regen | Reduce the holder's max Mana by 10. Subsequent spellcasts reduce max Mana by 5%, to a minimum of 15. |
| Death's Defiance |  | 25% AD 40 Armor 15% AS 25% Omnivamp | 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. Unique - only 1 per champion |
| Eternal Pact |  | 35% AP 1 Mana Regen | Combat start: Form a pact with the highest Health allied champion, If they die, gain 10 Mana regen and 40% Ability Power. When the ally uses their ability, gain 10 Mana. Every 6 seconds, shield your ally for 200% of the holder's Ability Power. |
| Fishbones |  | 25% AD 25% AS | The holder's attacks target random enemies. Gain +2 Attack Range. |
| Flickerblades |  | 10% AP 20% AS | Attacks grant 4% stacking Attack Speed. Every 3 attacks also grant 2% Attack Damage and 2% Ability Power. |
| Forbidden Idol |  | 500 HP 2 Mana Regen | Shields have 40% of their value converted to max Health instead. |
| Gambler's Blade |  | 55% AS | Grant 1% bonus Attack Speed per gold in your bank (up to 30 gold). Each attack has a 6% chance to drop 1 gold. Gold generated this game: [: unknown] Unique - only 1 per champion |
| Gold Collector |  | 35% AD 20% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions on champions have a 33% chance to drop 1 gold. Gold collected: [: unknown] Unique - only 1 per champion |
| Hellfire Hatchet |  | 400 HP 20% Omnivamp | Attacks deal 3% of the holder's max Health as bonus physical damage. For every 1% missing Health, gain 1.5% Attack Speed. |
| Horizon Focus |  | 250 HP 20 Armor 20 MR 2 Mana Regen | Stunning an enemy causes lightning to strike them, dealing 30% of their max Health as magic damage. |
| Hullcrusher |  | 60 Armor 60 MR | Combat Start: If there are no adjacent allies, gain 400 Health, 10% Attack Damage, and 10% Ability Power. |
| Infinity Force |  | 300 HP 30% AD 30% AP 30 Armor 30 MR 1.3 AS | Tons of EVERYTHING! |
| Lich Bane |  | 55% AP 10% AS | The holder’s first attack after each Ability cast deals [GenericCalc1: unknown] bonus magic damage. Damage increases based on Stage. |
| Lightshield Crest |  | 55 Armor 55 MR | Every 3 seconds, Shields the lowest percent Health ally for 70% of the holder’s combined Armor and Magic Resist for 5 seconds. On death grants this shield to all allies. |
| Luden's Tempest |  | 55% AD 55% AP | 100% of overkill damage plus 130 is dealt as magic damage to the two enemies nearest the target. |
| Manazane |  | 15% AD 15% AP 1 Mana Regen | After casting the first time in combat, gain 110 Mana over 5 seconds. |
| Mittens |  | 65% AS 15% Damage Amp | Shrinks the holder, granting them increased movement speed and immunity to Slow, Burn, and Wound. Slow: Reduces Attack Speed. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Mogul's Mail |  | 300 HP | Grants 1 Armor, 1 Magic Resist, and 5 Health when taking damage, stacking up to 35 times. At full stacks, grant gold 1 gold and continue gaining gold 1 gold every 11 seconds. Gold generated this game: [: unknown] Unique - only 1 per champion |
| Rapid Firecannon |  | 55% AS 5% Damage Amp | Gain +1 Attack Range, increased by 1 whenever the holder kills an enemy. |
| Seeker's Armguard |  | 25% AP 10 Armor 10 MR 20% Omnivamp | Takedowns increase the holder’s Armor, Magic Resist, and Ability Power by 20. |
| Silvermere Dawn |  | 150% AD 30 Armor 30 MR 30 Omnivamp% | Grants immunity to Stuns and the holder's attacks Stun the target for 0.8 seconds. The holder’s Attack Speed is locked at 0.5. |
| Statikk Shiv |  | 25% AP 40% AS | Every 3rd attack deals 15 + 35% of the holder's Ability Power as additional magic damage to 6 enemies. |
| Talisman of Ascension |  | 450 HP 20% AD 20% AP | After 22 seconds gain 100% max Health, 12 Mana Regen, and 120% Damage Amp for the rest of combat. |
| The Indomitable |  | 500 HP 20 Armor 20 MR | The holder's Move Speed is drastically reduced. Gain 15% max Health, become unstoppable, and pull the current target into melee range. |
| Titanic Hydra |  | 300 HP 20% AD 20% AS | Attacks deal 2% of the holder's max Health plus 4% of their Attack Damage as bonus physical damage to the target and adjacent enemies. |
| Void Gauntlet |  | 300 HP 10% Durability | Combat Start: Store 30% max Health and 2.5% more every second. On death, unleash the stored Health as magic damage split between enemies within 4 hexes. Unique - only 1 per champion |
| Wit's End |  | 300 HP 20 Armor 20 MR 25% AS | Attacks deal [GenericCalc1: unknown] bonus magic damage. Heals the holder for 25% of all magic damage dealt. Damage increases based on Stage. |
| Zhonya's Paradox |  | 25% AP 25 Armor 25 MR | Once per combat at 40% Health, become invulnerable and untargetable for 3 seconds. Unique - only 1 per champion |

### Radiant items (40)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Radiant Adaptive Helm |  | 40 MR 7 Mana Regen | Gain an additional 30% Mana from all sources. The wearer gains an additional bonus based on their Role: Tanks and Fighters: Gain 80 Armor and Magic Resistance. Other Roles: Gain 40% Attack Damage and Ability Power. |
| Radiant Archangel's Staff |  | 0.6 AP 2 Mana Regen | Combat start: Gain 40% Ability Power every 5 seconds in combat. |
| Radiant Blast Potion |  |  | After 8 seconds of combat, knock up enemies within 4 hexes for 2 seconds. [Single Use - This item is destroyed after activating in combat.] |
| Radiant Bloodthirster |  | 40 AD% 40 AP% 40 MR 40 Omnivamp% | Once per combat at 50% Health, gain a 60% max Health Shield that lasts up to 5 seconds. |
| Radiant Blue Buff |  | 30% AD 30% AP 10 Mana Regen | Gain 20% additional Attack Damage and Ability Power from all sources. |
| Radiant Bramble Vest |  | 100 Armor | Gain 12% max Health. Take 10% reduced damage from attacks. When struck by any attack, deal 200 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Radiant Crownguard |  | 200 HP 40 AP% 40 Armor | Combat start: Gain a 50% max Health Shield for 8 seconds. When the shield expires, gain 50% Ability Power. |
| Radiant Deathblade |  | 100% AD 20% Damage Amp | It glows in the presence of enemies. Or friends. Or anything alive, really.{ItemTags.Deathblade.DeadlierBladeStacks} |
| Radiant Dragon's Claw |  | 120 MR | Gain 28% max Health. Every 2 seconds, heal 5% max Health. |
| Radiant Edge of Night |  | 30% AD 30% AP 40 Armor 30% AS | At 40% Health, briefly become untargetable, shed negative effects, and heal all missing health. |
| Radiant Evenshroud |  | 500 HP 40 MR | 30% Sunder enemies within 3 hexes. Gain 50 Armor and Magic Resist for the first 20 seconds of combat. Sunder: Reduce Armor |
| Radiant Gargoyle Stoneplate |  | 400 HP 50 Armor 50 MR | Gain 20 Armor and 20 Magic Resist for each enemy targeting the holder. |
| Radiant Giant Slayer |  | 30% AD 30% AP 30% AS 30% Damage Amp | Gain 30% additional Damage Amp against Tanks. |
| Radiant Guinsoo's Rageblade |  | 20% AP 25% AS | Gain 16% stacking Attack Speed every second. |
| Radiant Hand of Justice |  | 40% Crit 2 Mana Regen | Gain 2 effects: • 35% Attack Damage and 35% Ability Power. • 30% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Radiant Health Potion |  |  | When the holder drops below 50% health, heal for 850 Health and heal allies for 150 Health over 3 seconds. [Single Use - This item is destroyed after activating in combat.] |
| Radiant Hextech Gunblade |  | 40% AD 40% AP 30% Omnivamp 2 Mana Regen | Heal the lowest percent Health ally for 40% of damage dealt. Ally Healing: [HealthCalc1: unknown] |
| Radiant Infinity Edge |  | 75% AD 75% Crit | Gain Precision. Precision: Ability damage can critically strike. Extra Precision grants 10% Critical Strike Damage. |
| Radiant Ionic Spark |  | 500 HP 30% AP 70 MR | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 300% of the Mana spent. Shred: Reduce Magic Resist |
| Radiant Jeweled Gauntlet |  | 75% AP 75% Crit | Gain Precision. Precision: Ability damage can critically strike. Extra Precision grants 10% Critical Strike Damage. |
| Radiant Kraken's Fury |  | 20% AD 40 MR 20% AS | Attacks grant 7% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 30% Attack Speed. |
| Radiant Last Whisper |  | 45% AD 40% AS 40% Crit | Attacks and Ability damage 30% Sunders the target for the rest of combat. This effect does not stack. Sunder: Reduce Armor |
| Radiant Lucky Item Chest |  |  | Use on a champion to open an armory of items especially suited for them. These items are based on the champion's recommended items. |
| Radiant Mana Potion |  |  | The holder gains 10 Mana. On cast, restore 80 Mana to the holder and 10 Mana to allies over 5 seconds. [Single Use - This item is destroyed after activating in combat.] |
| Radiant Morellonomicon |  | 300 HP 35% AP 2 Mana Regen | Attacks and Abilities deal 2% Burn and 33% Wound to enemies for 10 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Radiant Nashor's Tooth |  | 300 HP 30% AP 20% AS 40% Crit | Attacks grant 4 bonus Mana, increased to 6 if they critically strike. |
| Radiant Protector's Vow |  | 50 Armor 50 MR 2 Mana Regen | Combat Start: Gain 40 Mana. At 40% Health, gain 30 Mana and a Shield equal to 50% max Health |
| Radiant Quicksilver |  | 40 MR 40% AS 40% Crit | Combat Start: Unstoppable for 45 seconds. Gain 6% stacking Attack Speed every second. |
| Radiant Rabadon's Deathcap |  | 100% AP 30% Damage Amp | It's witnessed - and unleashed - miracles and calamities both.{ItemTags.RabadonsDeathcap.DeadlierCapsStacks} |
| Radiant Red Buff |  | 80% AS 6% Damage Amp | Attacks and Abilities 2% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Radiant Spear of Shojin |  | 30% AD 30% AP 2 Mana Regen | Attacks grant 10 bonus Mana. |
| Radiant Spirit Visage |  | 700 HP 18% Durability 4 Mana Regen | Regenerate 4% of missing Health each second. |
| Radiant Steadfast Heart |  | [: unknown] 40 Armor 40% Crit | Gain 10% Durability. While above 50% Health, instead gain 30% Durability.{Augment.Variant.HeartofSteel.Tooltip} |
| Radiant Sterak's Gage |  | 600 HP 90% AD | At 60% Health, gain a Shield equal to 80% of the wearer's maximum Health that rapidly decays over 6 seconds. |
| Radiant Striker's Flail |  | 300 HP 20% AS 40% Crit 20% Damage Amp | Critical Strikes grant 10% Damage Amp for 5 seconds, stacking up to 4 times. |
| Radiant Sunfire Cape |  | 150 HP 20 Armor | Gain 8% max Health. Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. Burn: Deals a percent of the target’s max Health as true damage every second. Wound: Reduces healing received. |
| Radiant Thief's Gloves |  | 150 HP 20% Crit | Each round: Equip 2 random Radiant items [Consumes 3 item slots.] |
| Radiant Titan's Resolve |  | 40 Armor 20% AS | Gain 4% Attack Damage and 4% Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 20% Damage Amp and unstoppable. |
| Radiant Void Staff |  | 75% AP 30% AS 2 Mana Regen | Attacks and Ability damage 30% Shred the target for the rest of combat. This effect does not stack. Shred: Reduce Magic Resist |
| Radiant Warmog's Armor |  | 1200 HP | Gain 36% max Health. |

### Set-specific and other items (5)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Alpha Mark |  |  | Select a Riftbeast to gain its unique effect! Alphas selected: [Stack: unknown]/2 If you already have 2 Alphas, using this selector will replace the oldest Alpha. |
| Alpha Mark |  |  | Select a Riftbeast to gain its unique effect! |
| Blast Potion |  |  | After 8 seconds of combat, knock up enemies within 3 hexes for 1.75 seconds. [Single Use - This item is destroyed after activating in combat.] |
| Health Potion |  |  | When the holder drops below 50% health, heal for 750 Health over 3 seconds. [Single Use - This item is destroyed after activating in combat.] |
| Mana Potion |  |  | The holder gains 10 Mana. On cast, restore 70 Mana over 5 seconds. [Single Use - This item is destroyed after activating in combat.] |

### Consumables (2)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Lucky Item Chest |  |  | Use on a champion to open an armory of items especially suited for them. These items are based on the champion's recommended items. |
| Masterwork Upgrade |  |  | Use on a champion to choose a Radiant version of that champion's craftable completed items. Upgrade the item you choose to its Radiant equivalent and unequip it. |

## Augments (260)

Tier is the "rarity" field from MetaTFT. The list is the augment pool in the MetaTFT data (PBE build 18.4, generated 2026-10-06) and may include augments not yet on live servers.

| Name | Tier | Effect |
|---|---|---|
| Advanced Loan | Gold | Gain 20 Gold. Your next augment is one tier lower. |
| Advanced Loan+ | Gold | Gain 33 Gold. Your next augment is one tier lower. |
| Arcane Viktor-y | Gold | After 8 seconds of combat, stun the enemy team for 1.5 seconds. Repeat this after 18 seconds of combat. TFT Macao Open, 2024 |
| Ascension | Gold | After 12 seconds of combat, your units gain 35% Damage Amp. |
| Augmented Power | Silver | Your next augment is one tier higher. |
| Backline Blueprint | Gold | Gain a 3-cost non-Tank champion and an Emblem that matches their class. |
| Backup Bows | Silver | Gain a Recurve Bow. After your champions attack 1000 times, get 2 more. |
| Band of Thieves | Silver | Gain 1 Thief's Gloves. |
| Band of Thieves II | Prismatic | Gain 2 Thief's Gloves. After 5 player combats, gain another. |
| Band of Thieves II+ | Prismatic | Gain 2 Thief's Gloves. After 5 player combats, gain another. |
| Band of Thieves II++ | Prismatic | Gain 2 Thief's Gloves. After 3 player combats, gain another. |
| Baron's Lair | Prismatic | Gain a random Tank item. After 8 seconds of combat, your team gains 4% bonus Attack Damage and Ability Power every second for the rest of combat. TFT Paris Open, 2025 |
| Beast Within | Gold | Primal grants an additional Blessing. Gain a Vi now and a Nidalee after 3 rounds. |
| Beast Within | Gold | Primal grants an additional Blessing. Gain a Vi now and a Sivir after 3 rounds. |
| Beast Within+ | Gold | Primal grants an additional Blessing. Gain a Vi and a Nidalee. |
| Beast Within+ | Gold | Primal grants an additional Blessing. Gain a Vi and a Sivir. |
| Belt Overflow | Prismatic | Gain 4 Giant's Belts. Your Giant's Belts grant +85 bonus Health. |
| Big Grab Bag | Gold | Gain 3 random components, 2 gold, and 1 Reforger. |
| Birthday Present | Prismatic | Gain a 2-star champion and 1 gold each time you level up. The champion's cost tier is your level minus 4 (max: 5-cost). |
| Birthday Reunion | Gold | Gain a random 2-star 2-cost champion. At Level 6, gain a random item component. At Level 9, gain a random 2-star 5-cost champion. Inkborn Fables Tactician's Crown, 2024 |
| Blossom's Call | Gold | When you buy a Wisp, gain a random Blossom champion based on the Wisp's cost. Gain a Yunara, a Yorick, and a Karma. |
| Bodyguard Training | Gold | Allies gain 15 Armor and Magic Resist, increased by 2 per player level. Gizmos & Gadgets Championship, 2022 |
| Bonus Gifts | Gold | Whenever you open a loot orb, you have a 0.25 chance to gain a gray loot orb. Gain 1 gray loot orbs. |
| Bonus Gifts+ | Gold | Whenever you open a loot orb, you have a 0.25 chance to gain a gray loot orb. Gain 2 gray loot orbs. |
| Booster Pack | Gold | Gain 12 gold of random champions. (At least one 3-cost guaranteed!) |
| Booster Pack+ | Gold | Gain 18 gold of random champions. (At least one 4-cost guaranteed!) |
| Booster Pack++ | Gold | Gain 26 gold of random champions. (At least one 5-cost guaranteed!) |
| Boxing Lessons | Silver | Your team gains 30 Health for each champion that starts combat in the front row. |
| Branching Out | Silver | Gain a random Emblem. |
| Branching Out+ | Silver | Gain a random Emblem and a Reforger. |
| Bronze For Life I | Gold | Your team gains 2.5% Damage Amp for each Bronze-tier trait. |
| Bronze For Life II | Prismatic | Your team gains 2.5% Damage Amp and 3 Armor and Magic Resist for each Bronze-tier trait. |
| Build A Bud | Prismatic | Gain a random 3-star 1-cost champion. Gain 6 gold. |
| Buried Treasures III | Prismatic | Gain a random item component now and at the start of the next 6 rounds. |
| Calculated Loss | Gold | After losing your combat, gain 2 gold and a free Shop reroll. |
| Call To Chaos | Prismatic | Gain a powerful and random reward. |
| Called Shot | Silver | Set your win streak to +4. Gain 4 gold. |
| Capital Gains I | Silver | Store gold equal to the interest you earn. On 3-3, gain gold equal to 100% of the stored gold. Gain 1 gold now. |
| Capital Gains II | Gold | Store gold equal to the interest you earn. On 3-3, gain gold equal to 125% of the stored gold. Gain 3 gold now. |
| Caretaker's Ally | Silver | Gain a random 2-cost champion now. Gain the same one again every time you level up. Champion: |
| Caretaker's Favor | Gold | Gain a component anvil when you reach level 5, 6, 7, and 8. Component anvils offer 4 choices. |
| Carve a Path | Silver | Gain a B.F. Sword. After your champions deal 65000 physical damage, gain 2 more. |
| Celestial Blessing I | Silver | Your team gains 12% Omnivamp. Excess healing is converted to a shield up to 200 Health. |
| Celestial Blessing II | Gold | Your team gains 18% Omnivamp. Excess healing is converted to a shield up to 400 Health. |
| Celestial Blessing III | Prismatic | Your team gains 25% Omnivamp. Excess healing is converted to a shield up to 1000 Health. |
| Challenger's Grace | Gold | At the start of combat, allies gain 25% Attack Speed for 3 seconds. On takedown, allies dash to their next target and gain this bonus again. |
| Champ Delivery | Silver | Gain 3 2-cost champions. After 6 rounds, gain 3 more. |
| Champ Delivery+ | Silver | Gain 3 3-cost champions. After 6 rounds, gain 3 more. |
| Champ Delivery++ | Silver | Gain 3 4-cost champions. After 6 rounds, gain 3 more. |
| Chosen of the Sun | Silver | Gain a Leona, a Kayle, and a Sejuani. Leona gains 70 permanent max Health each round, increased by 10 for each unique 3-star champion fielded. |
| Clear Mind | Gold | If there are no champions on your bench at the end of player combat, gain 3 XP. |
| Clockwork Accelerator | Gold | Your team gains 10% Attack Speed every 3 seconds in combat. |
| Cluttered Mind | Gold | Gain 4 1-cost champions now. If your bench is full at the end of player combat, gain 3 XP. |
| Cognitive Overload | Gold | Gain 7 gold, 1 2-star 1-cost champion, 1 2-cost champion, 2 3-cost champions, 1 XP, and 1 Shop reroll. |
| Cognitive Tax | Silver | Gain 8 gold and 1 XP. |
| Cognitive Tax+ | Silver | Gain 12 gold and 3 XP. |
| Comeback Story | Prismatic | Your team gains 4 Health and 0.3% Attack Speed per missing player Health. |
| Commerce Core | Prismatic | Gain 16 free Shop rerolls now and 3 every round for the rest of the game. |
| Component Buffet | Silver | Whenever you would get a component, gain a component anvil instead. Gain a random component.The anvil offers 4 choices. |
| Component Quest | Prismatic | Gain a random component and 1 gold. The next 7 times you win a player combat, gain a different random component. Afterwards, gain 1 gold. |
| Construct a Companion | Gold | The next 1-cost champion you buy is 3-star. Gain 2 gold. |
| Consuming Flora | Gold | Gain a Flora Fatalis emblem. The champion holding the emblem gains 50% increased benefits from Flora Fatalis. Gain 3 gold. |
| Cooking Pot | Gold | At the start of each turn, all allies holding a Frying Pan or Spatula item grant the nearest champion 60 permanent Health. Gain a Frying Pan. |
| Coronation | Prismatic | Gain a Tactician's Crown. Tactician's Crown, Cape, and Shield grant the holder 1.2 Attack Speed, 25% Attack Damage, and 30% Ability Power. |
| Corrosion | Silver | Enemies in the first two rows lose 4 Armor and Magic Resist every 2 seconds. |
| Coven Acolyte | Gold | When you choose to continue gathering Coven essence, heal 2 Tactician health. Gain a Camille, an Elise, and a Caitlyn. |
| Crafted Crafting | Silver | Whenever you craft a completed item, gain 2 rerolls. |
| Cry Me A River | Gold | Gain a Tear of the Goddess. Your team gains 1 Mana Regen. After 12 seconds in combat, increase this to 4. |
| Cursed Crown | Prismatic | Gain +2 max team size, but you take double player damage when you lose a player combat. |
| Cybernetic Implants | Gold | Your champions holding an item gain 100 Health and 15% Attack Damage. Gain a B.F. Sword. |
| Cybernetic Uplink | Gold | Your champions holding an item gain 100 Health and 2 Mana Regen. Gain a Tear of the Goddess. |
| Dark Ritual | Gold | Converting Coven Essence to rewards grants Coven champions Ability Power instead of loot. Gain a Camille, an Elise, and a Caitlyn. |
| Deadlier Blades | Prismatic | Gain a Deathblade. Deathblades permanently gain 1% Attack Damage whenever their holders score a takedown. |
| Deadlier Caps | Prismatic | Gain a Rabadon's Deathcap. Rabadon's Deathcaps grant 1 Mana Regen and permanently gain 1% Ability Power whenever their holders score a takedown. |
| Dummify | Silver | Lose all champions on your board and bench. Gain a Training Dummy with 60% of their combined health. The dummy gains 1150 Health per stage. Gain a random non-tank 2-star 2 cost champion. |
| Duo Queue | Gold | Gain 2 random 5-cost champions and 2 copies of a random component. |
| Early Learnings | Gold | Your team gains 5% Attack Damage & Ability Power. This increases by 1% after each player combat. 1-cost champions gain double. |
| Electrocharge I | Silver | Every 4 attacks an ally takes, they deal 30-90 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Electrocharge II | Gold | Every 4 attacks an ally takes, they deal 50-140 (based on current Stage) magic damage to nearby enemies (1 second cooldown). |
| Embiggen | Gold | Fae pixies are now larger and grant 50% more Attack Damage and Ability Power. Fae champions gain 10% Attack Damage and Ability Power immediately. Gain a Xayah and a Rakan. |
| Epic Rolldown | Gold | When you reach Level 8, gain 20 Shop rerolls. Runeterra Reforged Championship, 2023 |
| Epoch | Gold | Now, and at the start of every stage, gain 4 XP and 2 free rerolls. |
| Epoch+ | Gold | Now, and at the start of every stage, gain 8 XP and 2 free rerolls. |
| Exclusive Customization | Gold | Gain a Lucky Item Chest, a Magnetic Remover and 7 gold. |
| Expected Unexpectedness | Prismatic | Now and at the start of the next 2 stages, roll 3 dice. Gain various rewards based on their total. |
| Expedition | Silver | At the start of every round, lose the champion in the rightmost bench slot. After losing 33 gold of champions in this way, gain a powerful reward. Gain a 3-cost champion now. |
| Explosive Growth | Gold | Now and at the start of the next 3 rounds, gain 7 XP. |
| Explosive Growth+ | Gold | Now and at the start of the next 3 rounds, gain 9 XP. |
| Extra Buckles | Silver | Gain a Giant's Belt. After your champions take 75000 damage, get 2 more. |
| Feeling Lucky | Silver | Gain 12 gold, then flip a coin. If heads, gain 4 more gold. |
| Find Your Center | Silver | Your champion that starts combat in the center of the front row gains 15% Damage Amp and 25% max Health. |
| Flame On | Silver | Your 1 and 2-cost Inferno champions gain 0.2 Attack Damage and Ability Power. The one who did the most damage last combat gains double. Gain a Varus and an Akali. |
| Flexible | Prismatic | Gain 1 random Emblem. At the start of every Stage, gain a random Emblem. Your team gains 30 Health for each Emblem they are holding. |
| Flowing Tears | Silver | Gain a Tear of the Goddess. After your champions spend 6500 Mana, gain 2 more. |
| Focused Fire | Silver | Your team gains 10% Attack Damage. Every 5 seconds, they gain 5% more. |
| Forge A Friend | Silver | The next 1-cost champion you buy is 3-star. Gain 5 gold. |
| Forged In Strength | Prismatic | Gain 1 random Artifact. When you drop below 35 Health, gain 1 more random Artifact, 1 random completed item, and 2 random components. Remix Rumble Championship, 2023 |
| FOURcing | Gold | Gain a random 4-cost champion. For each item equipped to that champion, your team gains 100 health. |
| Frontline Foundation | Gold | Gain a 2-star 1-cost Tank champion and an Emblem that matches their class. |
| Future Focused | Silver | Your Tactician loses 20 Health, but after 5 player combats, gain 3 4-cost champions and 8 gold. |
| Gain 21 Gold | Gold | Gain 21 gold. |
| Giant and Mighty | Prismatic | Your team becomes large, gaining 200 Health and 10% max Health. |
| Gilded Steel | Gold | Gain two 5-cost champions. If you field at least one 5-cost champion, your 1 to 4-cost champions gain 8% Durability. K.O. Coliseum Tactician's Crown, 2025 |
| Glass Cannon I | Silver | Allies that start combat in the back row begin combat at 80% Health but gain 16% Damage Amp. |
| Glass Cannon II | Gold | Allies that start combat in the back row begin combat at 80% Health but gain 25% Damage Amp. |
| Going Long | Prismatic | You no longer gain interest. Gain 10 gold now. Gain 4 XP after every player combat. Interest is extra gold you gain per 10g saved. |
| Gold Destiny | Gold | Gain a random Gold Augment and 3 gold. |
| Gold Destiny+ | Gold | Gain a random Gold Augment and 5 gold. |
| Golden Gamble | Prismatic | Gain 2 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed Item anvils. |
| Golden Gamble+ | Prismatic | Gain 3 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed Item anvils. |
| Golden Gamble++ | Prismatic | Gain 6 gold and flip a coin. If heads, gain a Radiant Lucky Item Chest. If tails, gain 2 Completed Item anvils. |
| Good For Something I | Silver | Champions that aren't holding items have a 40% chance to drop 1 gold on death. |
| Group Hug I | Silver | Combat start: Champions grant other adjacent champions 6 Armor and Magic Resist. This effect stacks. |
| Group Hug II | Gold | Combat start: Champions grant other adjacent champions 9 Armor and Magic Resist. This effect stacks. |
| Hard Bargain | Gold | You cannot choose your Carousel rewards. Gain an additional 6 player health and 8 gold each carousel round. When you do not pick a Carousel Reward, you are given an unpicked champion and the item it carries. |
| Hard Commit | Prismatic | Gain a random emblem. Now and at the start of each stage, gain a 1-star champion of that trait with a cost equal to the Stage (max 5) and 3 gold. |
| Healing Orbs I | Silver | When an enemy dies, a nearby ally is healed for 250 Health. |
| Healing Orbs II | Gold | When an enemy dies, a nearby ally is healed for 575 Health. |
| Heart of Steel | Gold | Gain a Steadfast Heart. Every 10 seconds of combat its holder survives, Steadfast Heart gains additional 18 permanent max Health. |
| Hedge Fund | Prismatic | Gain 22 gold. Your max interest is increased to 10. |
| Heroic Grab Bag | Gold | Gain 2 Lesser Champion Duplicators. Gain 4 gold. |
| Heroic Grab Bag+ | Gold | Gain 2 Lesser Champion Duplicators. Gain 8 gold. |
| Heroic Grab Bag++ | Gold | Gain 2 Lesser Champion Duplicators. Gain 14 gold. |
| Hold the Line | Prismatic | Champions in your back 2 rows gain 10% Ability Power and 9% Attack Damage for each champion that starts combat in your front row. |
| Hustler | Gold | You no longer gain interest but gain 3 gold at the start of every player combat round. Gain 3 gold now. Interest is extra gold you gain per 10g saved. |
| Infinity Protection | Gold | Gain 3 gold now. On Stage 3-7, gain an Infinity Force. Infinity Force gives allies in the same row a 12% Health shield. Monsters Attack Championship, 2023 |
| Invested+ | Prismatic | Gain 26 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Invested++ | Prismatic | Gain 45 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Investment Strategy I | Gold | Your champions gain 9 permanent max health per interest you earn. Gain 6 gold now. |
| Investment Strategy II | Prismatic | Your champions gain 12 permanent max health per interest you earn. Your max interest is increased by 2. Gain 10 gold now. |
| Iron Assets | Silver | Gain a component anvil and 3 gold. |
| It's Me, Baby | Gold | Gain a championship hex in the bottom right corner. The champion in it gains 15% Damage Amp and 15% Attack Speed, and drops 1 gold every 5 takedowns. Lore & Legends Tactician's Crown, 2026 |
| Item Extraction | Gold | The first 3 times you sacrifice a champion in the Blackthorn hex, gain a copy of a random component they're holding. Gain an Azir, Veigar, and a Rek'Sai. |
| Item Grab Bag I | Silver | Gain 1 random completed item. |
| Jeweled Lotus I | Gold | Your team gains 10% Critical Strike Chance and Precision. Precision: Ability damage can critically strike. Additional Precision grants 10% Critical Strike Damage. |
| Jeweled Lotus II | Prismatic | Your team gains 25% Critical Strike Chance, 10% Critical Strike Damage, and Precision. Precision: Ability damage can critically strike. Additional Precision grants 10% Critical Strike Damage. |
| Kick Start | Silver | Gain a random 2-star 2-cost champion and 1 gold. |
| Kingslayer | Silver | After winning player combat, gain 1 gold. If they had more health than you, gain 6 Gold instead. Gain 1 gold. |
| Know Your Enemy | Gold | You can see who you will fight next. Your team gains 8% Damage Amp, increased to 15% if you and your opponent have any of the same traits active. |
| Late Game Scaling | Gold | Gain 2 XP at the start of player combat rounds. Your 5-cost champions gain 12% Health and 12% Attack Speed. Cyber City Tactician's Crown, 2025 |
| Late Game Specialist | Silver | When you reach Level 9, gain 27 gold. |
| Latent Forge | Silver | After 8 player combats, gain an Artifact anvil. |
| Legion Of Threes | Gold | Gain a random Emblem. Your 3-cost champions and all allies equipped with an Emblem gain 150 Health and 12% Attack Speed. Reckoning Championship, 2021 |
| Level Up! | Prismatic | When you buy XP, gain an additional 2. Gain 10 immediately. |
| Living Forge | Prismatic | Gain an Artifact Anvil now and after every 10 player combats. |
| Loaded Dice | Silver | Your dice and coins are luckier. Roll a die and gain gold equal to the number rolled. Then flip a coin and gain 1 gold if you flip heads. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves and 4 gold. |
| Lucky Gloves+ | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves now, then another after 4 player combats. |
| Luxury Subscription | Prismatic | Gain a package containing a 5-cost champion, a 2-star 1-cost champion, and 5 gold. At the start of the next 2 stages, gain the package again. |
| Magic Roll | Gold | Roll 3 dice. Gain rewards based on their total. {Augment.Variant.MagicRoll.Reward} |
| Makeshift Armor I | Silver | Champions without items equipped gain 30 Armor and Magic Resist. |
| Makeshift Armor II | Gold | Champions without items equipped gain 50 Armor and Magic Resist. |
| Malicious Monetization | Gold | Gain 4 gold. For the next 2 rounds, enemy champions drop 2 gold when killed. |
| Master of All Origins | Prismatic | Open an armory and choose one of 3 traits. Gain a Lux with that trait and a Spear of Shojin. Traits on your board are more likely to appear in the armory. Lux provides +2 to her chosen trait |
| Max Build | Gold | Gain 10 free Shop rerolls. When you reach level 9, gain 3 free Shop rerolls and a Champion Duplicator. Galaxies Championship, 2020 |
| Min-Max | Prismatic | Gain a Golden Item Remover, 4 random components, and 4 gold. |
| Missed Connections | Silver | Gain a copy of each 1-cost champion. |
| Money Hungry | Gold | Gain 7 gold now, then 7 gold at the start of every stage. Picking up gold makes your Tactician larger. |
| Money Hungry+ | Gold | Gain 13 gold now, then 7 gold at the start of every stage. Picking up gold makes your Tactician larger. |
| Money Monsoon | Prismatic | Gain 7 gold now and every round for the rest of the game. |
| Nature's Shelter | Gold | Champions that start combat adjacent to an Elderwood plant gain 1.15 Attack Speed and heal 2% of their maximum Health every 2 seconds. Gain an Alistar, an Ornn, and a Xayah. |
| Nesting Anvils | Prismatic | Gain an Artifact Anvil. After you open it, gain a Component Anvil. After you open that, gain 4 gold. |
| Nesting Anvils+ | Prismatic | Gain an Artifact Anvil. After you open it, gain a Component Anvil. After you open that, gain 8 gold. |
| Nesting Dolls | Prismatic | After a champion dies in combat, spawn a copy of them one star level lower with 70% health. Gain 3 free rerolls. |
| Nesting Dolls | Prismatic | After a champion dies in combat, spawn a copy of them one star level lower with 60% health. Gain 3 free rerolls. |
| Nesting Dolls | Prismatic | After a champion dies in combat, spawn a copy of them one star level lower with 60% health. Gain 3 free rerolls. |
| NO SCOUT NO PIVOT | Gold | Champions that fight in player combat cannot be benched or sold. Your team gains 1% Health, and 1% Attack Damage and Ability Power after player combat. |
| Omega Riftbeast | Gold | You can now choose a second Alpha Riftbeast. Alpha Riftbeasts gain 10% max Health and 10% Attack Speed. Gain a Scuttlecrab, a Cinderling, and a Pebbles. |
| One Buff Two Buff | Prismatic | Gain a Red Buff, a Blue Buff, and a Champion Duplicator. |
| One, Two, Five! | Silver | Gain 1 random component, 2 gold, and 1 random 5-cost champion. |
| Ones Two Three | Silver | Gain 2 1-cost champions, 1 2-cost champion, and 1 3-cost champion. |
| Pandora's Bench | Silver | At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. Gain 4 gold. |
| Pandora's Items I | Silver | Round start: items on your bench are randomized. Gain 1 random component. |
| Pandora's Items II | Gold | Round start: items on your bench are randomized. Gain 2 random components and 4 gold. |
| Pandora's Items III | Prismatic | Round start: items on your bench are randomized. Gain 1 Radiant item. |
| Partial Ascension | Silver | After 12 seconds of combat, your units gain 20% Damage Amp. |
| Patience Is A Virtue | Silver | Gain 4 rerolls now. Each round, gain 1 free reroll if you did not buy a champion last round. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. |
| Pilfer | Gold | Each round, gain a 1-star copy of the first champion you killed last combat. The first time you gain 21 gold worth of champions in this way, gain a Thief's Gloves. |
| Plot Armor | Gold | Your team gains 8 Armor and Magic Resist. After falling below 0.5 Health, increase this to 40 for the rest of combat. |
| Portable Forge | Gold | Choose 1 of 4 Artifacts. Artifacts are more powerful items with a unique effect. |
| Prismatic Destiny | Prismatic | Gain a random Prismatic Augment and 4 gold. |
| Prismatic Destiny+ | Prismatic | Gain a random Prismatic Augment and 7 gold. |
| Prismatic Ticket | Prismatic | Each time your shop is rerolled you have a 45% chance to gain a free reroll. |
| Promised Protection | Gold | Gain a Protector's Vow. Protector's Vow grants its holder 20% additional Mana from all sources and 200 max Health. |
| Quick Streaks | Silver | You gain twice as much progress towards win and loss streaks. Gain 2 gold. |
| Radiant Rascal | Prismatic | Gain a Radiant Thief's Gloves item. This equips 2 random Radiant items every round. |
| Radiant Relics | Prismatic | Choose 1 of 5 Radiant Items. Gain a Magnetic Remover. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 cost tier higher (max 5). Gain 2 Magnetic Removers and 3 free rerolls. |
| Replication | Gold | Choose 1 of 3 components. For the next 2 rounds, gain a copy of that component. |
| Residual Magic | Silver | Your team gains 60 Health. Each time you purchase a Wisp, increase this by 10. |
| Residual Magic + | Silver | Your team gains 75 Health. Each time you purchase a Wisp, increase this by 10. |
| Residual Magic ++ | Silver | Your team gains 90 Health. Each time you purchase a Wisp, increase this by 10. |
| Retribution | Prismatic | Gain 2 Hands of Justice. Allies equipped with Hand of Justice gain Precision and 25% Critical Strike Chance. Precision: Ability damage can critically strike. Additional Precision grants 10% Critical Strike Damage. |
| Rolling For Days | Silver | Gain 10 free Shop rerolls. |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 8 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin+ | Gold | Gain 1 random completed item now, and 1 component after 4 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Seraphim's Staff | Gold | Gain an Archangel's Staff. Archangel's Staff grants 2 additional Mana Regen if its holder has 90% or more Ability Power. |
| Shimmerscale Essence | Prismatic | Gain a Gambler's Blade. After 6 player combats, gain a Mogul's Mail. |
| Shopping Spree | Prismatic | When you level up, gain a number of Shop refreshes equal to your level +1. Gain 6 gold. |
| Silver Destiny | Silver | Gain a random Silver Augment and 2 gold. |
| Silver Destiny+ | Silver | Gain a random Silver Augment and 4 gold. |
| Silver Destiny++ | Silver | Gain a random Silver Augment and 7 gold. |
| Silver Spoon | Silver | Gain 10 XP. |
| Slammin' | Gold | Gain 3 gold. After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slammin'+ | Gold | Gain 4 gold and 10 XP now. After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slice of Life | Silver | Twice per stage, gain a random champion. Its cost increases by Stage. This effect ends after receiving 1 5-cost champion. |
| Slightly Magic Roll | Silver | Roll a die. Gain rewards based on the number rolled. {Augment.Variant.SlightlyMagicRoll.Reward} |
| Small Furry Friend | Gold | While Sprykin is active, gain a second BFF summon that is 45% as powerful. Gain a Teemo, a Veigar, and a Kobuko. |
| Small Grab Bag | Silver | Gain 2 random item components. |
| Solo Leveling | Gold | For the next 5 combats, your team size is 1, but the champion you field has massively increased stats. Gain 1 XP for every kill they get. Afterwards, gain 2 item components. |
| Solo Plate | Gold | Gain a Gargoyle Stoneplate. Allies holding Gargoyle Stoneplate gain 17% max Health if they start combat alone in their row. |
| Soul Awakening | Prismatic | Combat start: Your team gains 1.5% Attack Damage and Ability Power per second for 10 seconds. At max stacks, deal 12% bonus true damage. |
| Speedy Double Kill | Gold | Gain a Guinsoo's Rageblade. When 2 players are eliminated, gain 40 gold. Fates Championship, 2021 |
| Spirit Of Redemption | Gold | Gain a Spirit Visage. Every 5 seconds, Spirit Visage heals allies within 1 hex for 7.5% of their missing Health. |
| Spreading Roots | Gold | Gain a random Emblems. After 3 combats, gain another. |
| Spreading Roots+ | Gold | Gain 2 random Emblems and 2 gold. |
| Staffsmith | Gold | Gain 2 random completed items that build from Needlessly Large Rod. |
| Stand United | Silver | Your units gain 1.5% Attack Damage and 1.5% Ability Power for each non-unique Trait active across your team. |
| Starring Up | Prismatic | After 3-starring 5 champions, set your level to 9. Gain 2 rerolls now and at the start of every round. |
| Subscription Service | Prismatic | Now and at the start of each stage, open a shop of 4 unique 4-cost champions and gain 6 gold. |
| Sun and Moon | Gold | Your Lunar champions gain 100 Health and 8% Damage Amp for each unique Solar champion fielded. Gain a Leona, a Kayle, and a Diana. |
| Sun and Moon+ | Gold | Your Lunar champions gain 100 Health and 8% Damage Amp for each unique Solar champion fielded. Gain a Leona, a Kayle, and an Aphelios. |
| Sweet Treats | Prismatic | Gain an Artifact anvil. Your team gains 16 Health for each item equipped on champions. |
| Sword Overflow | Prismatic | Gain 4 BF Swords. Your BF Swords grant +4% Attack Speed. |
| Swordsmith | Gold | Gain 2 random completed items that build from B.F. Sword. |
| Tactician's Kitchen | Prismatic | Gain a random Emblem. After 3 rounds, gain a Tactician's Cape. |
| Team Building | Silver | Gain a Lesser Champion Duplicator. Gain another after 5 player combats. This item allows you to copy a 3-cost or less champion. |
| The Golden Dragon | Gold | Gain a Mogul's Mail. Champions holding Mogul's Mail take up +1 team slot but gain 600 Health and 15% Durability. Mogul's Mail gives gold as well as combat power. Dragonlands Championship, 2022 |
| The Golden Egg | Prismatic | Gain a golden egg that hatches in 11 turns for a huge amount of loot. Winning player combat speeds up the hatch timer by an extra turn. |
| The Tower | Silver | Gain a giant Training Dummy. Every 4 seconds, it zaps the 3 nearest enemies dealing 5% max Health true damage. |
| The Trait Tree | Prismatic | Gain 3 random Emblems and 2 gold. |
| The Trait Tree+ | Prismatic | Gain 3 random Emblems, a Reforger, and 4 gold. |
| Time Skip | Gold | Disable your Shop for the next 3 rounds. Then gain 32 XP. |
| Tons of Stats! | Gold | Your team gains 44 Health, 4% Attack Damage, 4% Ability Power, 4 Armor, 4 Magic Resist, 4% Attack Speed, and 4 Mana. |
| TONS of Stats! | Prismatic | Your team gains 88 Health, 8% Attack Damage, 8% Ability Power, 8 Armor, 8 Magic Resist, 8% Attack Speed, and 8 Mana. |
| Trade Sector | Gold | Gain a Shop reroll every round. Gain 2 gold. |
| Trade Sector+ | Gold | Gain a Shop reroll every round. Gain 8 gold. |
| Trait Ladder | Prismatic | Gain a random emblem. After fielding 2 non-unique traits in a player combat, gain a reward. Then increase the number of traits needed and the size of the reward. |
| Twin Guardians | Silver | Your team gains 2 Armor and Magic Resist for each ally that starts combat in the front 2 rows. |
| Twin Guardians | Silver | If you have exactly 2 allies in your first row, grant them 100 Health, 35 Armor, and 35 Magic Resist. |
| U.R.F | Gold | Gain a Spatula. Champions holding Spatula or Frying Pan items gain 20% Attack Speed and 3 Mana Regen. |
| Unrivaled | Gold | You can field Rengar and Kha'Zix together with no penalties, and they gain bonuses when the other casts. Gain a Rengar and a Kha'Zix. |
| Unrivaled | Gold | You can field Rengar and Kha'Zix together with no penalties, and they gain bonuses when the other casts. Gain a Rengar and a Kha'Zix. |
| Upward Mobility | Prismatic | Buying XP costs 1 less. Gain 2 Health and 2 Shop rerolls whenever you level up. |
| Urf's Grab Bag | Prismatic | Gain 1 Spatula, 3 random components, and 5 gold. |
| Verticality I | Silver | Champions gain 1.5% Attack Damage, 1.5% Ability Power, 1.5 Armor, and 1.5 Magic Resist for each ally that shares a trait with them. |
| Verticality II | Gold | Champions gain 2.5% Attack Damage, 2.5% Ability Power, 2 Armor, and 2 Magic Resist for each ally that shares a trait with them. |
| Verticality III | Prismatic | Champions gain 4% Attack Damage, 4% Ability Power, 3.5 Armor, and 3.5 Magic Resist for each ally that shares a trait with them. |
| Wand Overflow | Prismatic | Gain 4 Needlessly Large Rods. Your Needlessly Large Rods grant +5% Attack Speed. |
| Warpath | Gold | Gain a 2-star 2-cost champion. After dealing 80 player damage, gain a chest of high cost champions and items. |
| We Stick Together | Prismatic | Gain a random Emblem and a completed item anvil. Allies that share a trait with that Emblem gain 30% Attack Speed. {Augment.Variant.WeStickTogether.Tooltip} {Augment.Variant.WeStickTogether.Trait} Into The Arcane Tactician's Crown, 2025 |
| Weight The Worth | Gold | The first time you 3-star a champion, gain a copy of them each round for the rest of the game. 3-starring the same champion again grants additional copies. Gain a Lesser Champion Duplicator. |
| Wisp Rebate | Silver | Each time you purchase a Wisp, gain 2 gold. Gain 4 gold now. |
| Wisp Rebate | Silver | Each time you purchase a Wisp, gain 2 gold. Gain 4 gold now. |
| Wisp Rebate+ | Silver | Each time you purchase a Wisp, gain 2 gold. Gain 8 gold now. |
| Worth the Wait | Gold | Gain a random 1-cost champion. Gain another copy of them at the start of each round for the rest of the game. |
| Worth the Wait II | Prismatic | Gain 2 copies of a random 2-cost champion. Gain another copy of them at the start of each round for the rest of the game. |
| Woven Magic | Gold | Gain a random item component. Every 2200 Mana spent by your champions, grants an additional component (max: [: unknown]/3). |
| Young and Wild and Free | Silver | You can always move freely on Carousel rounds. Gain 5 gold. |

## Wisps (375)

Source: MetaTFT lookup "charms" (Wisps reuse the internal Charm system from Set 12). Tier is the internal Wisp tier; rounds are the stage window in which the Wisp can appear; cost is gold. Rows marked "upgrade" are the boosted version of another Wisp.

| Wisp | Category | Tier | Rounds | Cost | Effect |
|---|---|---|---|---|---|
| Heated Rivalry | Champion | 1 | 2-1 to 2-7 | 3 | Gain a Rengar or a Kha'Zix. They gain 3 takedowns. |
| Heated Rivalry (upgrade) | Champion | 1 | 2-1 to 2-7 | 3 | Gain a Rengar or a Kha'Zix. They gain 5 takedowns. |
| Lost Travelers | Champion | 1 | 2-1 to 2-7 | 3 | Gain 3 gold worth of random champions. |
| Lost Travelers (upgrade) | Champion | 1 | 2-1 to 2-7 | 3 | Gain 4 gold worth of random champions. |
| Mitosis | Champion | 1 | 2-1 to 2-7 | 1 | Gain a Tiny Champion Duplicator. |
| Mitosis (upgrade) | Champion | 1 | 2-1 to 2-7 | 1 | Gain a Tiny Champion Duplicator. |
| Apprentice | Champion | 2 | 2-1 to 2-7 |  | Gain a random 2-star 1-cost champion. |
| Apprentice (upgrade) | Champion | 2 | 2-1 to 2-7 |  | Gain a random 2-star 1-cost champion. |
| Big Guns | Champion | 2 | 4-2 to 4-7 |  | Gain 2 random 1-star 4-cost champions. |
| Big Guns (upgrade) | Champion | 2 | 4-2 to 4-7 |  | Gain 2 random 1-star 4-cost champions. |
| Forest Twins | Champion | 2 | 2-1 to 2-7 | 4 | Gain 2 copies of a 2-cost champion you don't have. |
| Forest Twins (upgrade) | Champion | 2 | 2-1 to 2-7 | 4 | Gain 3 copies of a 2-cost champion you don't have. |
| Journeyman | Champion | 2 | 2-1 to 3-4 |  | Gain a random 2-star 2-cost champion. |
| Journeyman (upgrade) | Champion | 2 | 2-1 to 3-4 |  | Gain a random 2-star 2-cost champion. |
| Propagate | Champion | 2 | 2-1 to 4-1 | 2 | Gain a copy of the first allied champion to die. |
| Propagate (upgrade) | Champion | 2 | 2-1 to 4-1 | 2 | Gain a copy of the first allied champion to die. |
| Solar Gift | Champion | 2 | 2-1 to 3-4 | 6 | Gain a Leona, Sejuani, and Kayle |
| Three Me | Champion | 2 | 3-1 to 4-1 |  | Gain a random 2-star 3-cost champion. |
| Three Me (upgrade) | Champion | 2 | 3-1 to 4-1 |  | Gain a random 2-star 3-cost champion. |
| Circle of Elders | Champion | 3 | 6-1 to 10-1 | 45 | Gain one of each 5-cost champion. |
| Circle of Elders (upgrade) | Champion | 3 | 6-1 to 10-1 | 45 | Gain one of each 5-cost champion. |
| Forest Guide | Champion | 3 | 2-1 to 2-7 |  | Gain a random 4-cost champion. |
| Forest Guide (upgrade) | Champion | 3 | 2-1 to 2-7 |  | Gain a random 4-cost champion. |
| Grandmaster | Champion | 3 | 6-1 to 10-1 |  | Gain a random 2-star 5-cost champion. |
| Grandmaster (upgrade) | Champion | 3 | 6-1 to 10-1 |  | Gain a random 2-star 5-cost champion. |
| Hero Of Prophecy | Champion | 3 | 6-1 to 10-1 | 35 | Gain a 5-cost champion. Gain them again each round FOREVER. |
| Hero Of Prophecy (upgrade) | Champion | 3 | 6-1 to 10-1 | 35 | Gain a 5-cost champion. Gain them again each round FOREVER. |
| Smurfing | Champion | 3 | 3-5 to 4-1 |  | Gain a random 5 cost champion. |
| Smurfing (upgrade) | Champion | 3 | 3-5 to 4-1 |  | Gain a random 5-cost champion. |
| Bark Armor | Combat | 1 | 2-1 to 2-7 | 0 | Your champions gain 150 shield for 10 seconds. |
| Bark Armor (upgrade) | Combat | 1 | 2-1 to 2-7 | 0 | Your champions gain 225 shield for 10 seconds. |
| Big Boom | Combat | 1 | 3-1 to 4-1 | 1 | The first ally to die explodes, dealing 25% Max Health magic damage. |
| Big Boom (upgrade) | Combat | 1 | 3-1 to 4-1 | 1 | The first 2 allies to die explode, dealing 25% Max Health magic damage. |
| Borrowed Gear | Combat | 1 | 3-1 to 4-7 | 1 | At the start of combat, a champion gains a temporary recommended item. |
| Borrowed Gear (upgrade) | Combat | 1 | 3-1 to 4-7 | 1 | At the start of combat, a champion gains a temporary recommended item and 250 Health. |
| Bunch-o'-Belts | Combat | 1 | 3-5 to 4-7 | 1 | Your allies gain 7 temporary, non-removable Giant's Belts. |
| Bunch-o'-Belts (upgrade) | Combat | 1 | 3-5 to 4-7 | 1 | Your allies gain 10 temporary, non-removable Giant's Belts. |
| Drizzle | Combat | 1 | 4-2 to 5-7 | 0 | After 8 seconds of combat, your team gains 10 Mana. |
| Drizzle (upgrade) | Combat | 1 | 4-2 to 5-7 | 0 | After 8 seconds of combat, your team gains 15 Mana. |
| Flow | Combat | 1 | 4-2 to 10-1 |  | Your champions are unstoppable and gain 15% Attack Speed for 10 seconds. |
| Flow (upgrade) | Combat | 1 | 4-2 to 10-1 |  | Your champions are unstoppable and gain 15% Attack Speed for 15 seconds. |
| Found Friend | Combat | 1 | 2-1 to 2-7 | 0 | Gain a temporary Training Dummy with a Giant's Belt. |
| Found Friend (upgrade) | Combat | 1 | 2-1 to 2-7 | 0 | Gain a Training Dummy with a Sunfire Cape. |
| Grove Armor | Combat | 1 | 4-2 to 5-7 | 0 | Your highest Health champion gains 15 Armor and 15 Magic Resist. |
| Grove Armor (upgrade) | Combat | 1 | 4-2 to 5-7 | 0 | Your highest Health champion gains 25 Armor and 25 Magic Resist. |
| Hugify | Combat | 1 | 3-1 to 4-1 | 1 | Your highest Health unit grows, gaining 350 Health. |
| Hugify (upgrade) | Combat | 1 | 3-1 to 4-1 | 1 | Your highest Health unit grows, gaining 500 Health. |
| Hummingbird | Combat | 1 | 3-1 to 4-1 | 1 | Your highest Attack Speed unit shrinks, gaining 20% Attack Speed. |
| Hummingbird (upgrade) | Combat | 1 | 3-1 to 4-1 | 1 | Your highest Attack Speed unit shrinks, gaining 35% Attack Speed. |
| Lightning Strike | Combat | 1 | 4-2 to 5-7 | 1 | Deal 10% Max Health true damage to all enemies. |
| Lightning Strike (upgrade) | Combat | 1 | 4-2 to 5-7 | 1 | Deal 18% Max Health true damage to all enemies. |
| Moonrise | Combat | 1 | 4-2 to 5-7 |  | After 8 seconds, increase Lunar's effects by 100%. |
| Moonrise (upgrade) | Combat | 1 | 4-2 to 5-7 |  | After 8 seconds, increase Lunar's effects by 175%. |
| Petrified Dummy | Combat | 1 | 3-1 to 4-7 | 2 | Your first champion to die turns into a 900 Health dummy with their defenses. |
| Petrified Dummy (upgrade) | Combat | 1 | 3-1 to 4-7 | 2 | Your first champion to die turns into a 1350 Health dummy with their defenses. |
| Petrify Shields | Combat | 1 | 4-2 to 10-1 | 1 | 10% Slow enemies and reduce their shields by 20% for 10 seconds. |
| Petrify Shields (upgrade) | Combat | 1 | 4-2 to 10-1 | 1 | 10% Slow enemies and reduce their shields by 30% for 10 seconds. |
| Prolific Power | Combat | 1 | 4-2 to 10-1 | 0 | Your champions gain 4% Attack Damage and Ability Power. |
| Prolific Power (upgrade) | Combat | 1 | 4-2 to 10-1 | 0 | Your champions gain 6% Attack Damage and Ability Power. |
| Quicken | Combat | 1 | 3-5 to 4-7 | 2 | On cast, your champions gain 30% Attack Speed for 4 seconds. |
| Quicken (upgrade) | Combat | 1 | 3-5 to 4-7 | 2 | On cast, your champions gain 40% Attack Speed for 5 seconds. |
| Rain | Combat | 1 | 3-1 to 4-7 | 1 | Your team gains 1 Mana Regen. |
| Rain (upgrade) | Combat | 1 | 3-1 to 4-7 | 1 | Your team gains 2 Mana Regen. |
| Resistant | Combat | 1 | 4-2 to 10-1 | 2 | Tanks reduce instances of incoming damage by 10. |
| Resistant (upgrade) | Combat | 1 | 4-2 to 10-1 | 2 | Tanks reduce instances of incoming damage by 18. |
| Returning Friend | Combat | 1 | 4-2 to 5-7 | 0 | Gain a temporary Training Dummy with two Giant's Belts. |
| Returning Friend (upgrade) | Combat | 1 | 4-2 to 5-7 | 0 | Gain a Training Dummy with Three Giant's Belts. |
| Stealthy | Combat | 1 | 4-2 to 5-7 | 0 | Your 3 strongest Fighters or Assassins are untargetable for 8 seconds. |
| Stealthy (upgrade) | Combat | 1 | 4-2 to 5-7 | 0 | 3 random champions on the back row are untargetable for 12 seconds. |
| Sunfire Sorcery | Combat | 1 | 3-5 to 4-7 | 2 | Burn and Wound enemies for 10 seconds. |
| Sunfire Sorcery (upgrade) | Combat | 1 | 3-5 to 4-7 | 2 | Burn and Wound enemies for 15 seconds. |
| Tattered Armor | Combat | 1 | 3-5 to 4-7 | 1 | 30% Shred and Sunder enemies for 10 seconds. |
| Tattered Armor (upgrade) | Combat | 1 | 3-5 to 4-7 | 1 | 30% Shred and Sunder enemies for 15 seconds. |
| Ultra Ascension | Combat | 1 | 3-1 to 4-1 | 1 | After 25 seconds, allies gain 300% Damage Amp. |
| Ultra Ascension (upgrade) | Combat | 1 | 3-1 to 4-1 | 1 | After 23 seconds, allies gain 300% Damage Amp. |
| Verdant Vitality | Combat | 1 | 4-2 to 10-1 | 0 | Your champions gain 50 Health. |
| Verdant Vitality (upgrade) | Combat | 1 | 4-2 to 10-1 | 0 | Your champions gain 75 Health. |
| Wrapped In Thorns | Combat | 1 | 3-5 to 10-1 | 2 | Your Tanks reflect 12% of damage taken to adjacent enemies every 4 seconds. |
| Wrapped In Thorns (upgrade) | Combat | 1 | 3-5 to 10-1 | 2 | Your Tanks reflect 18% of damage taken to adjacent enemies every 4 seconds. |
| Aftershock | Combat | 2 | 4-2 to 10-1 | 2 | After 8 seconds, stun all enemies for 1.5 seconds. |
| Aftershock (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | After 8 and 18 seconds, stun all enemies for 1.5 seconds. |
| Backrow Star | Combat | 2 | 4-2 to 10-1 | 1 | A random back row champion gains 100% Attack Speed for 7 seconds. |
| Backrow Star (upgrade) | Combat | 2 | 4-2 to 10-1 | 1 | A random back row champion gains 130% Attack Speed for 8 seconds. |
| Barrier | Combat | 2 | 5-1 to 10-1 | 3 | Allies gain 1250 Shield, decaying over 6 seconds. |
| Barrier (upgrade) | Combat | 2 | 5-1 to 10-1 | 3 | Allies gain 1250 Shield, decaying over 9 seconds. |
| Bear's Visit | Combat | 2 | 3-5 to 5-7 | 3 | Gain the Bear Primal buff for this combat. |
| Booster Shot | Combat | 2 | 5-1 to 10-1 | 6 | Up to 2 1-star champions temporarily star up during combat. |
| Booster Shot (upgrade) | Combat | 2 | 5-1 to 10-1 | 6 | Up to 3 1-star champions temporarily star up during combat. |
| Bulwark | Combat | 2 | 4-2 to 10-1 | 3 | Your Tanks gain a 20% Max Health shield upon falling below 50% Health. |
| Bulwark (upgrade) | Combat | 2 | 4-2 to 10-1 | 3 | Your Tanks gain a 30% Max Health shield upon falling below 50% Health. |
| Counterspell | Combat | 2 | 5-1 to 10-1 | 2 | Increase all enemies' Max Mana by 15 until their next cast. |
| Counterspell (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Increase all enemies' Max Mana by 22 until their next cast. |
| Diversified | Combat | 2 | 4-2 to 5-7 | 3 | Your team gains 30 Health for each active non-unique trait. |
| Diversified (upgrade) | Combat | 2 | 4-2 to 5-7 | 3 | Your team gains 45 Health for each active non-unique trait. |
| Downpour | Combat | 2 | 5-1 to 10-1 | 2 | Your team gains 2 Mana Regen. |
| Downpour (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Your team gains 3 Mana Regen. |
| Fellowship | Combat | 2 | 5-1 to 10-1 | 3 | Champions gain 4% Max Health Shield and 4% Damage Amp per adjacent ally. |
| Fellowship (upgrade) | Combat | 2 | 5-1 to 10-1 | 3 | Champions gain 6% Max Health Shield and 6% Damage Amp per adjacent ally. |
| Homing Fireflies | Combat | 2 | 3-5 to 10-1 | 2 | Your Casters spawn a firefly per 20 Mana spent. Each deals 110 magic damage. |
| Homing Fireflies (upgrade) | Combat | 2 | 3-5 to 10-1 | 2 | Your Casters spawn a firefly per 15 Mana spent. Each deals 110 magic damage. |
| Improved Reach | Combat | 2 | 5-1 to 10-1 | 1 | Your Fighters gain 1 range. |
| Improved Reach (upgrade) | Combat | 2 | 5-1 to 10-1 | 1 | Your Fighters gain 2 range. |
| Iron Core | Combat | 2 | 4-2 to 5-7 | 1 | Your front center champion gains 6% Max Health per front-row unit. |
| Iron Core (upgrade) | Combat | 2 | 4-2 to 5-7 | 1 | Your front center champion gains 8% Max Health per front-row unit. |
| Ironwood | Combat | 2 | 4-2 to 10-1 | 2 | Enemies attacking your highest health champion deal 14% less damage. |
| Ironwood (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | Enemies attacking your highest health champion deal 20% less damage. |
| Killing Frenzy | Combat | 2 | 5-1 to 10-1 | 2 | Fighters execute targets under 12% Health. They gain 1.3 Attack Speed on kill. |
| Killing Frenzy (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Fighters execute targets under 15% Health. They gain 1.6 Attack Speed on kill. |
| Living Soil | Combat | 2 | 4-2 to 5-7 |  | Every 4 seconds, roots stun an enemy for 1 second. |
| Living Soil (upgrade) | Combat | 2 | 4-2 to 5-7 |  | Every 4 seconds, roots stun 2 enemies for 1 second. |
| Mana-Rich Soil | Combat | 2 | 5-1 to 10-1 | 2 | Your Casters have 0.82 reduced Max Mana. |
| Mana-Rich Soil (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Your Casters have 0.75 reduced Max Mana. |
| Marksmen's Gale | Combat | 2 | 4-2 to 10-1 | 2 | Your Marksmen's attacks grant 3% stacking Attack Speed. |
| Marksmen's Gale (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | Your Marksmen's attacks grant 5% stacking Attack Speed. |
| Marksmen's Marks | Combat | 2 | 5-1 to 10-1 | 3 | Your Marksmen have a 30% chance to double attack. |
| Marksmen's Marks (upgrade) | Combat | 2 | 5-1 to 10-1 | 3 | Your Marksmen have a 45% chance to double attack. |
| Mercenary Force | Combat | 2 | 4-2 to 5-7 | 3 | Gain 3% Damage Amp for every 10 gold you have. |
| Mercenary Force (upgrade) | Combat | 2 | 4-2 to 5-7 | 3 | Gain 4% Damage Amp for every 10 gold you have. |
| Moonlight Ritual | Combat | 2 | 3-1 to 4-7 | 2 | Your lowest star 1-cost or 2-cost champion stars up in combat. |
| Moonlight Ritual (upgrade) | Combat | 2 | 3-1 to 4-7 | 2 | Your two lowest star 1-cost or 2-cost champions star up in combat. |
| Nature's Ally | Combat | 2 | 4-2 to 10-1 | 2 | Every 3 ally deaths, summon a random Riftbeast. |
| Nature's Ally (upgrade) | Combat | 2 | 3-1 to 4-7 | 2 | Every 3 ally deaths, summon a random Riftbeast that's sometimes 2-star. |
| Phantom Armor | Combat | 2 | 3-1 to 5-7 | 2 | Gain a temporary Warmog's Armor. |
| Phantom Armor (upgrade) | Combat | 2 | 3-1 to 5-7 | 2 | Gain a temporary The Indomitable. |
| Phantom Gloves | Combat | 2 | 3-1 to 5-7 | 3 | Gain a temporary Thief's Gloves. |
| Phantom Gloves (upgrade) | Combat | 2 | 3-1 to 5-7 | 3 | Gain a temporary Mittens. |
| Phantom Splash | Combat | 2 | 4-2 to 10-1 | 2 | Combat start: Up to 2 inactive traits activate at the lowest level. |
| Phantom Splash (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | Combat Start: Up to 4 inactive traits activate at the lowest level. |
| Phantom Vest | Combat | 2 | 3-1 to 5-7 | 2 | Gain a temporary Bramble Vest. |
| Phantom Vest (upgrade) | Combat | 2 | 3-1 to 5-7 | 2 | Gain a temporary Aegis of Dawn. |
| Plated Shields | Combat | 2 | 5-1 to 10-1 | 2 | Shields are 15% stronger. Allies gain 150 shield for 10 seconds. |
| Plated Shields (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Shields are 20% stronger. Allies gain 300 shield for 10 seconds. |
| Potted Lifebloom | Combat | 2 | 3-1 to 4-7 | 1 | Gain a temporary Lifebloom. |
| Potted Lifebloom (upgrade) | Combat | 2 | 3-1 to 4-7 | 1 | Gain a temporary Lifeblossom. |
| Potted Stonebark | Combat | 2 | 3-1 to 4-7 | 1 | Gain a temporary Stonebark Tree. |
| Potted Stonebark (upgrade) | Combat | 2 | 3-1 to 4-7 | 1 | Gain a temporary Stonebark Tree. |
| Preppers | Combat | 2 | 3-1 to 5-7 |  | Your stacking items start next combat partially stacked. |
| Preppers (upgrade) | Combat | 2 | 3-1 to 5-7 |  | Your stacking items start next combat extra stacked. |
| Regeneration | Combat | 2 | 3-5 to 4-7 | 2 | Your team heals 1% max Health per second. |
| Regeneration (upgrade) | Combat | 2 | 3-5 to 4-7 | 2 | Your team heals 2% max Health per second. |
| Revenge | Combat | 2 | 5-1 to 10-1 | 2 | Whenever an ally dies, your champions gain 4% stacking Damage Amp. |
| Revenge (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Whenever an ally dies, your champions gain 6% stacking Damage Amp. |
| Scrappy | Combat | 2 | 4-2 to 5-7 | 3 | Combat start: 2 champions without items gain a temporary recommended item. |
| Scrappy (upgrade) | Combat | 2 | 4-2 to 5-7 | 3 | Combat Start: 3 champions without items gain a temporary recommended item. |
| Snacktime! | Combat | 2 | 4-2 to 10-1 | 2 | The BFF eats enemies he damages under 16% Max Health. |
| Snacktime! (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | The BFF eats enemies he damages under 22% Max Health. |
| Solitude's Cloak | Combat | 2 | 5-1 to 10-1 | 2 | Champions with no adjacent allies gain 10% Max Health and 10% Damage Amp. |
| Solitude's Cloak (upgrade) | Combat | 2 | 5-1 to 10-1 | 2 | Champions with no adjacent allies gain 15% Max Health and 15% Damage Amp. |
| Stand Alone | Combat | 2 | 3-5 to 10-1 | 2 | Champions alone in their row gain 1.15 Health and 15% Damage Amp. |
| Stand Alone (upgrade) | Combat | 2 | 3-5 to 10-1 | 2 | Champions alone in their row gain 1.2 Health and 20% Damage Amp. |
| Supercritical | Combat | 2 | 4-2 to 10-1 | 2 | Allies with Precision gain 25% Critical Strike Damage. |
| Supercritical (upgrade) | Combat | 2 | 4-2 to 10-1 | 2 | Allies with Precision gain 40% Critical Strike Damage. |
| Tiger's Visit | Combat | 2 | 3-5 to 5-7 | 3 | Gain the Tiger Primal buff for this combat. |
| Tiger's Visit (upgrade) | Combat | 2 | 3-5 to 5-7 | 3 | Gain the Tiger Primal buff for this combat. |
| Training Yard | Combat | 2 | 4-2 to 5-7 | 3 | Gain 2 Training Dummies each with a random tank item. |
| Training Yard (upgrade) | Combat | 2 | 4-2 to 5-7 | 3 | Gain 3 Training Dummies each with a random tank item. |
| Turtle's Visit | Combat | 2 | 3-5 to 5-7 | 3 | Gain the Turtle Primal buff for this combat. |
| Turtle's Visit (upgrade) | Combat | 2 | 3-5 to 5-7 | 3 | Gain the Turtle Primal buff for this combat. |
| Yordle Spirit | Combat | 2 | 4-2 to 5-7 | 2 | Allies have a 99% chance to dodge attacks for 4 seconds. |
| Yordle Spirit (upgrade) | Combat | 2 | 4-2 to 5-7 | 2 | Allies have a 99% chance to dodge attacks for 5 seconds. |
| Barrier | Combat | 3 |  | 3 | Allies gain 7500 Shield, decaying over 30 seconds. |
| Blaze | Combat | 3 |  | 3 | Spread a raging fire across the board! |
| Blaze | Combat | 3 | 5-1 to 10-1 | 3 | Fire spreads across the board, dealing true damage to enemies. |
| Blaze (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Fire spreads across the board, dealing more true damage to enemies. |
| Clone Companion | Combat | 3 |  | 5 | Clone your highest damage champion. The clone deals 1 damage. |
| Clone Companion | Combat | 3 | 5-1 to 10-1 | 5 | Clone your highest damage champion. The clone deals 0.2 damage. |
| Clone Companion (upgrade) | Combat | 3 | 5-1 to 10-1 | 5 | Clone your highest damage champion. The clone deals 0.3 damage. |
| Combust | Combat | 3 |  | 3 | Your champions explode on death, dealing 100% Max Health magic damage. |
| Combust | Combat | 3 | 5-1 to 10-1 | 3 | Your champions explode on death, dealing 12% Max Health magic damage. |
| Combust (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Your champions explode on death, dealing 18% Max Health magic damage. |
| Counterspell | Combat | 3 |  | 2 | Increase all enemies' Max Mana by 150 until their next cast. |
| Downpour | Combat | 3 |  | 2 | Your team gains 15 Mana Regen. |
| Giant Growth | Combat | 3 | 5-1 to 10-1 | 4 | Your highest Health unit grows, gaining 800 Health. |
| Giant Growth (upgrade) | Combat | 3 | 5-1 to 10-1 | 4 | Your 2 highest Health units grow, gaining 800 Health. |
| Giant's Aura | Combat | 3 |  | 3 | Your highest Health ally becomes an absolute menace. |
| Giant's Aura | Combat | 3 | 5-1 to 10-1 | 3 | Your highest Health ally deals 2% max Health damage per second in an area. |
| Giant's Aura (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Your highest Health ally deals 3% max Health damage per second in an area. |
| Hand Of Baron | Combat | 3 | 5-1 to 10-1 | 6 | Your team gains Baron Buff, increasing their stats. |
| Hand Of Baron (upgrade) | Combat | 3 | 5-1 to 10-1 | 6 | Your team gains Baron Buff, greatly increasing their stats. |
| Hero's Entrance | Combat | 3 | 6-1 to 10-1 | 2 | After 8 seconds, the leftmost benched champion joins combat and gains 2 recommended items. |
| Hero's Entrance (upgrade) | Combat | 3 | 6-1 to 10-1 | 2 | After 5 seconds, the leftmost benched champion joins combat and gains 2 recommended items. |
| Heroic Sacrifice | Combat | 3 | 4-2 to 5-7 | 5 | Your 3 strongest Champions gain HEROIC POWER, but other allies are sacrificed. |
| Heroic Sacrifice (upgrade) | Combat | 3 | 4-2 to 5-7 | 5 | Your 3 strongest Champions gain HEROIC POWER, but other allies are sacrificed. |
| Hireling | Combat | 3 |  |  | 3 temporary 2-star 5-costs with 2 recommended items each join your team. |
| Hireling | Combat | 3 | 6-1 to 10-1 |  | A temporary 1-star 5-cost with 1 recommended items joins your team. |
| Hireling (upgrade) | Combat | 3 | 6-1 to 10-1 |  | A temporary 1-star 5-cost with 2 recommended items joins your team. |
| Hugify | Combat | 3 |  | 1 | Your 10 highest Health units grow, gaining 2500 Health. |
| Hummingbird | Combat | 3 |  | 1 | Your 10 highest Attack Speed units shrink, gaining 100% Attack Speed. |
| Infliction | Combat | 3 | 5-1 to 10-1 | 4 | Slow, Burn, Shred, and Sunder enemies for 8 seconds. |
| Infliction (upgrade) | Combat | 3 | 5-1 to 10-1 | 4 | Slow, Burn, Shred, and Sunder enemies for 12 seconds. |
| Killer's Regret | Combat | 3 | 5-1 to 10-1 | 1 | Your first 3 champions to die stun their killer for 1.5 seconds. |
| Killer's Regret (upgrade) | Combat | 3 | 5-1 to 10-1 | 1 | Your first 4 champions to die stun their killer for 1.75 second. |
| Late Bloomer | Combat | 3 |  |  | A squad of 5 temporary 4-star 1-costs joins your team. |
| Late Bloomer | Combat | 3 | 4-2 to 5-7 |  | A temporary 3-star 1-cost joins your team. |
| Late Bloomer (upgrade) | Combat | 3 | 4-2 to 5-7 |  | A temporary 3-star 1-cost with 1recommend item joins your team. |
| Lightning Storm | Combat | 3 |  | 3 | Every 0.15 seconds deal 5% Max Health true damage to an enemy. |
| Lightning Storm | Combat | 3 | 5-1 to 10-1 | 3 | Every 1 second deal 5% Max Health true damage to an enemy. |
| Lightning Storm (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Every 1 seconds deal 7% Max Health true damage to an enemy. |
| Nature's Wrath | Combat | 3 | 6-1 to 10-1 | 1 | Kill EVERYONE without an item after 10 seconds. |
| Nature's Wrath (upgrade) | Combat | 3 | 6-1 to 10-1 | 1 | Kill EVERYONE without an item after 8 seconds. |
| Phantom Armor | Combat | 3 |  | 2 | Gain 6 temporary Radiant Warmog's Armors and 5 Removers. |
| Phantom Emblem | Combat | 3 | 5-1 to 10-1 | 2 | Gain a temporary emblem of your most active trait. |
| Phantom Emblem (upgrade) | Combat | 3 | 5-1 to 10-1 | 2 | Gain a temporary emblem of your most active trait. |
| Radiantize | Combat | 3 | 5-1 to 10-1 | 3 | After 8 seconds, 1 random equipped item temporarily becomes Radiant. |
| Radiantize (upgrade) | Combat | 3 | 4-2 to 10-1 | 3 | After 5 seconds, 1 random equipped item temporarily becomes Radiant. |
| Stand Alone | Combat | 3 |  | 2 | Champions alone in their row gain 2 Health and 150% Damage Amp. |
| Tattered Armor | Combat | 3 |  | 1 | 90% Shred and Sunder enemies for 45 seconds. |
| Treetop Archers | Combat | 3 |  | 3 | An implausible number of archers fire from the trees. |
| Treetop Archers | Combat | 3 | 5-1 to 10-1 | 3 | Archers fire from the trees, dealing 500 physical damage every 3 seconds. |
| Treetop Archers (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Archers fire from the trees, dealing 900 physical damage every 3 seconds. |
| Tremors | Combat | 3 |  | 3 | Every 3 seconds, Stun enemies for 1.5 seconds. |
| Tremors | Combat | 3 | 5-1 to 10-1 | 3 | Every 8 seconds, Stun enemies for 1 second. |
| Tremors (upgrade) | Combat | 3 | 5-1 to 10-1 | 3 | Every 8 seconds, Stun enemies for 1.5 seconds. |
| Ultra Ascension | Combat | 3 |  | 1 | After 12 seconds, allies gain 300% Damage Amp. |
| Barter | GoldXP | 1 | 3-5 to 5-7 | 2 | The next champion sold this round sells for 200% gold. |
| Barter (upgrade) | GoldXP | 1 | 3-5 to 5-7 | 2 | The next champion sold this round sells for 250% gold. |
| Beggar's Wisp | GoldXP | 1 | 2-1 to 10-1 | 0 | The wilds couldn't find a wisp for you this roll. Gain 3 gold. |
| Beggar's Wisp (upgrade) | GoldXP | 1 | 2-1 to 10-1 | 0 | The wilds couldn't find a wisp for you this roll. Gain 5 gold. |
| Blood Money | GoldXP | 1 | 3-1 to 4-7 | 2 | Gain 1 gold for every champion you kill. |
| Blood Money (upgrade) | GoldXP | 1 | 3-1 to 4-7 | 2 | Gain 1 gold for every champion you kill. Gain 1 gold. |
| Coin Flip | GoldXP | 1 | 2-1 to 2-7 | 0 | Flip a coin, gain 1 gold if it's heads. |
| Coin Flip (upgrade) | GoldXP | 1 | 2-1 to 2-7 | 0 | Flip 2 coins, gain 1 gold if it's heads. |
| Die Roll | GoldXP | 1 | 3-1 to 4-1 | 2 | Roll a six-sided die. Gain gold equal to the result. |
| Die Roll (upgrade) | GoldXP | 1 | 3-1 to 4-1 | 2 | Roll two six-sided dice. Gain gold equal to the result. |
| Experienced | GoldXP | 1 | 2-1 to 2-7 | 1 | Gain 2 XP. |
| Experienced (upgrade) | GoldXP | 1 | 2-1 to 2-7 | 1 | Gain 2 XP. |
| Pocket Change | GoldXP | 1 | 3-1 to 4-1 | 1 | Your one-star champions drop 1 gold on death. |
| Pocket Change (upgrade) | GoldXP | 1 | 3-1 to 4-1 | 1 | Your one-star champions drop 1 gold on death. Gain 1 gold. |
| Room for a Shroom | GoldXP | 1 | 4-2 to 5-7 | 0 | Gain 2 random mushrooms. |
| Room for a Shroom (upgrade) | GoldXP | 1 | 4-2 to 5-7 | 0 | Gain 3 random mushrooms. |
| Truce | GoldXP | 1 | 2-1 to 2-7 | 0 | Gain 1 gold. Your opponent gains 1 gold. |
| Truce (upgrade) | GoldXP | 1 | 2-1 to 2-7 | 0 | Gain 2 gold. Your opponent gains 1 gold. |
| Bronze Spoon | GoldXP | 2 | 2-1 to 4-1 | 3 | Gain 4 XP. |
| Bronze Spoon (upgrade) | GoldXP | 2 | 2-1 to 4-1 | 3 | Gain 4 XP. |
| Golden Goose | GoldXP | 2 | 4-2 to 5-7 | 1 | Gain 1 gold for every 1700 damage your highest damage champion deals. |
| Golden Goose (upgrade) | GoldXP | 2 | 4-2 to 5-7 | 1 | Gain 1 gold for every 1000 damage your highest damage champion deals. |
| Grow Up | GoldXP | 2 | 3-1 to 4-7 | 7 | Gain 10 XP. |
| Grow Up (upgrade) | GoldXP | 2 | 3-1 to 4-7 | 7 | Gain 12 XP. |
| Life Debt | GoldXP | 2 | 5-1 to 10-1 | 0 | Gain 1 gold for every 12 missing Player Health. |
| Life Debt (upgrade) | GoldXP | 2 | 5-1 to 10-1 | 0 | Gain 1 gold for every 12 missing Player Health. Then heal 3 Health. |
| Slow Study | GoldXP | 2 | 3-5 to 4-7 | 2 | For each ally champion alive after 15 seconds, gain 2 XP. |
| Slow Study (upgrade) | GoldXP | 2 | 3-5 to 4-7 | 2 | For each ally champion alive after 12 seconds, gain 2 XP. |
| Truce | GoldXP | 3 |  | 0 | Gain 100 gold. Your opponent gains 1 gold. |
| Artifactinate | Item | 2 | 2-1 to 3-4 | 2 | Lose 3 components, including ones built into items. Gain an Artifact anvil. |
| Artifactinate (upgrade) | Item | 2 | 2-1 to 3-4 | 2 | Lose 3 components, including ones built into items. Gain an Artifact anvil and 1 Reforger. |
| Blood and Iron | Item | 2 | 3-1 to 4-1 | 3 | After 20 champion deaths, gain a component anvil. |
| Blood and Iron (upgrade) | Item | 2 | 2-1 to 2-7 | 3 | After 16 champion deaths, gain a component anvil. |
| Curio Cart | Item | 2 | 3-1 to 4-7 | 0 | Open an item store. Items cost 0 to 15 gold. |
| Curio Cart (upgrade) | Item | 2 | 3-5 to 5-7 | 0 | Open an item store. Items cost 0 to 15 gold. Gain 1 Gold. |
| Idle Craftsman | Item | 2 | 3-1 to 4-1 | 4 | A component on your bench turns into a completed item it builds. |
| Idle Craftsman (upgrade) | Item | 2 | 3-1 to 4-1 | 4 | A component on your bench turns into a completed item it builds. |
| Peddler | Item | 2 | 4-2 to 5-7 | 0 | Open a random item store. Random items cost 0 to 14 gold. |
| Peddler (upgrade) | Item | 2 | 4-2 to 5-7 | 0 | Open a random item store. Random items cost 0 to 14 gold. Gain 2 Gold. |
| Blast Potion | Misc | 1 | 3-1 to 4-7 | 1 | Gain a temporary equippable Blast Potion. |
| Blast Potion (upgrade) | Misc | 1 | 3-1 to 4-7 | 1 | Gain a temporary equippable Radiant Blast Potion. |
| Doodad Bag | Misc | 1 | 3-1 to 4-1 |  | Gain 2 consumables that give permanent Armor and Magic Resist or Health. |
| Doodad Bag (upgrade) | Misc | 1 | 3-1 to 4-1 |  | Gain 3 consumables that give permanent Armor and Magic Resist or Health. |
| Doodad Jar | Misc | 1 | 2-1 to 2-7 |  | Gain 1 consumable that gives permanent Armor and Magic Resist or Health. |
| Doodad Jar (upgrade) | Misc | 1 | 2-1 to 2-7 |  | Gain 2 consumables that give permanent Armor and Magic Resist or Health. |
| Early Fix | Misc | 1 | 2-1 to 2-7 | 0 | Gain 1 Item Reforger. |
| Early Fix (upgrade) | Misc | 1 | 2-1 to 2-7 | 0 | Gain 2 Item Reforgers. |
| Forest Mage | Misc | 1 | 3-5 to 10-1 | 1 | Fill your shop with Wisps. You can only buy one. |
| Forest Mage (upgrade) | Misc | 1 | 3-5 to 10-1 | 1 | Fill your shop with upgraded Wisps. You can only buy one. |
| Health Potion | Misc | 1 | 3-1 to 4-7 | 1 | Gain a tempoary equippable Health Potion. |
| Health Potion (upgrade) | Misc | 1 | 3-1 to 4-7 | 1 | Gain a temporary equippable Radiant Health Potion. |
| Knick-Knack Bag | Misc | 1 | 3-1 to 4-1 |  | Gain 2 consumables that give permanent Attack Damage or Attack Speed. |
| Knick-Knack Bag (upgrade) | Misc | 1 | 3-1 to 4-1 |  | Gain 3 consumables that give permanent Attack Damage or Attack Speed. |
| Knick-Knack Jar | Misc | 1 | 2-1 to 2-7 |  | Gain 1 consumable that gives permanent Attack Damage or Attack Speed. |
| Knick-Knack Jar (upgrade) | Misc | 1 | 2-1 to 2-7 |  | Gain 2 consumables that give permanent Attack Damage or Attack Speed. |
| Lesser Chaos | Misc | 1 | 2-1 to 3-4 | 1 | Cast a random Wisp. |
| Lesser Chaos (upgrade) | Misc | 1 | 2-1 to 3-4 | 1 | Cast a random Wisp worth 1 or more gold. |
| Mana Potion | Misc | 1 | 3-1 to 4-7 | 1 | Gain a temporary equippable Mana Potion. |
| Mana Potion (upgrade) | Misc | 1 | 3-1 to 4-7 | 1 | Gain a temporary equippable Radiant Mana Potion. |
| Salvager | Misc | 1 | 4-2 to 5-7 | 0 | Any units you bench have items removed and broken apart. |
| Salvager (upgrade) | Misc | 1 | 4-2 to 5-7 | 0 | Any units you bench have items removed and broken apart. Gain 2 Gold. |
| Terraforming | Misc | 1 | 4-2 to 10-1 | 2 | Gain 7 seeds at start of combat. |
| Terraforming (upgrade) | Misc | 1 | 4-2 to 10-1 | 2 | Gain 10 seeds at start of combat. |
| Thingamajig Bag | Misc | 1 | 3-1 to 4-1 |  | Gain 2 consumables that give permanent Ability Power or Mana Regen. |
| Thingamajig Bag (upgrade) | Misc | 1 | 3-1 to 4-1 |  | Gain 3 consumables that give permanent Ability Power or Mana Regen. |
| Thingamajig Jar | Misc | 1 | 2-1 to 2-7 |  | Gain 1 consumable that gives permanent Ability Power or Mana Regen. |
| Thingamajig Jar (upgrade) | Misc | 1 | 2-1 to 2-7 |  | Gain 2 consumables that give permanent Ability Power or Mana Regen. |
| Doodad Sack | Misc | 2 | 5-1 to 10-1 |  | Gain 3 consumables that give permanent Armor and Magic Resist or Health. |
| Doodad Sack (upgrade) | Misc | 2 | 5-1 to 10-1 |  | Gain 4 consumables that give permanent Armor and Magic Resist or Health. |
| Healing Pool | Misc | 2 | 2-1 to 3-4 | 2 | Heal 5 Player Health. |
| Healing Pool (upgrade) | Misc | 2 | 3-5 to 5-7 | 2 | Heal 7 Player Health. |
| Knick-Knack Sack | Misc | 2 | 5-1 to 10-1 |  | Gain 3 consumables that give permanent Attack Damage or Attack Speed. |
| Knick-Knack Sack (upgrade) | Misc | 2 | 5-1 to 10-1 |  | Gain 4 consumables that give permanent Attack Damage or Attack Speed. |
| Minor Blood Ritual | Misc | 2 | 2-1 to 3-4 | 3 | Lose 3 Player Health. Gain a Lesser Champion Duplicator. |
| Minor Blood Ritual (upgrade) | Misc | 2 | 2-1 to 3-4 | 3 | Lose 1 Player Health. Gain a Lesser Champion Duplicator. |
| Potioncraft | Misc | 2 | 5-1 to 10-1 | 2 | Gain three temporary equippable Potions. |
| Potioncraft (upgrade) | Misc | 2 | 5-1 to 10-1 | 2 | Gain three temporary equippable Radiant Potions. |
| Sinister Deal | Misc | 2 | 2-1 to 3-4 | 0 | Lose 4 Player Health. Gain 2 Gold. |
| Sinister Deal (upgrade) | Misc | 2 | 2-1 to 3-4 | 0 | Lose 2 Player Health. Gain 2 Gold. |
| Thingamajig Sack | Misc | 2 | 5-1 to 10-1 |  | Gain 3 consumables that give permanent Ability Power or Mana Regen. |
| Thingamajig Sack (upgrade) | Misc | 2 | 5-1 to 10-1 |  | Gain 4 consumables that give permanent Ability Power or Mana Regen. |
| Blood Ritual | Misc | 3 | 4-2 to 4-7 | 6 | Lose 10 Player Health. Gain a Champion Duplicator. |
| Blood Ritual (upgrade) | Misc | 3 | 4-2 to 4-7 | 6 | Lose 8 Player Health. Gain a Champion Duplicator. |
| Greater Chaos | Misc | 3 | 5-1 to 10-1 | 4 | Cast 2 random Wisps. |
| Greater Chaos (upgrade) | Misc | 3 | 5-1 to 10-1 | 4 | Cast 2 random Wisps worth 2 or more gold. |
| Solar Gift (upgrade) |  |  |  | 6 | Gain a Leona, Sejuani, and Kayle |
| Component Bounty | Risky | 1 | 3-1 to 4-7 | 4 | If you win the next player combat, gain a random component. |
| Component Bounty (upgrade) | Risky | 1 | 3-1 to 4-7 | 4 | If you win the next player combat, gain a random component. |
| Drought | Risky | 1 | 2-1 to 2-7 | 1 | Add 2 to your loss streak. |
| Drought (upgrade) | Risky | 1 | 2-1 to 2-7 | 1 | Add 3 to your loss streak. |
| Essence Theft | Risky | 1 | 2-1 to 4-1 | 1 | Gain 50% bonus Coven Essence this combat. |
| Essence Theft (upgrade) | Risky | 1 | 2-1 to 4-1 | 1 | Gain 90% bonus Coven Essence this combat. |
| Fertilize | Risky | 1 | 2-1 to 3-4 | 1 | Gain 1 XP for each enemy that dies. |
| Fertilize (upgrade) | Risky | 1 | 2-1 to 3-4 | 1 | Gain 1 XP for each enemy that dies. Gain 2 XP. |
| Flood | Risky | 1 | 2-1 to 2-7 | 1 | Add 1 to your win streak. |
| Flood (upgrade) | Risky | 1 | 2-1 to 2-7 | 1 | Add 2 to your win streak. |
| Minor Gambit | Risky | 1 | 2-1 to 2-7 | 1 | If you win the next player combat, gain 2 Gold. |
| Minor Gambit (upgrade) | Risky | 1 | 2-1 to 2-7 | 1 | If you win the next player combat, gain 3 Gold. |
| Minor Polymorph | Risky | 1 | 2-1 to 3-4 | 0 | A random 1-cost you own permanently transforms into a 2-cost. |
| Minor Polymorph (upgrade) | Risky | 1 | 2-1 to 3-4 | 0 | The highest star level 1-cost you own permanently transforms into a 2-cost. |
| Polymorph | Risky | 1 | 2-1 to 4-1 | 0 | A random 2-cost you own permanently transforms into a 3-cost. |
| Polymorph (upgrade) | Risky | 1 | 2-1 to 4-1 | 0 | The highest star level 2-cost you own permanently transforms into a 3-cost. |
| Take One With Ya | Risky | 1 | 2-1 to 2-7 | 1 | If you lose and kill at least one enemy, gain 4 XP. |
| Take One With Ya (upgrade) | Risky | 1 | 2-1 to 2-7 | 1 | If you lose and kill at least one enemy, gain 6 XP. |
| Trophy Hunter | Risky | 1 | 2-1 to 4-7 | 2 | Next player combat, your Rivals' takedowns count twice. |
| Trophy Hunter (upgrade) | Risky | 1 | 2-1 to 4-7 | 2 | Next player combat, your Rivals' takedowns count twice. |
| Cutpurse | Risky | 2 | 3-5 to 4-7 | 2 | Every 5 seconds, your champions have a 20% chance to drop 1 Gold. |
| Cutpurse (upgrade) | Risky | 2 | 3-5 to 4-7 | 2 | Every 4 seconds, your champions have a 20% chance to drop 1 Gold. |
| Flip Frenzy | Risky | 2 | 3-1 to 4-1 | 1 | Flip a coin, gain 1 gold if it's heads. Continue until you flip tails. |
| Flip Frenzy (upgrade) | Risky | 2 | 3-1 to 4-1 | 1 | Flip a coin, gain 1 gold if it's heads. Continue until you flip tails. |
| Golden Road | Risky | 2 | 2-1 to 3-4 | 2 | After 3 player combats, gain 2 Gold per fight won. |
| Golden Road (upgrade) | Risky | 2 | 2-1 to 3-4 | 2 | After 3 player combats, gain 2 Gold per fight won plus 1 Gold. |
| Good Loss | Risky | 2 | 3-1 to 4-1 | 4 | For the next 3 player combats, gain 4 XP if you lose. |
| Good Loss (upgrade) | Risky | 2 | 3-1 to 4-1 | 4 | For the next 3 player combats, gain 6 XP if you lose. |
| Major Gambit | Risky | 2 | 4-2 to 5-7 | 3 | If you win the next player combat, gain 8 Gold. |
| Major Gambit (upgrade) | Risky | 2 | 4-2 to 5-7 | 3 | If you win next combat, gain 10 Gold. |
| Major Polymorph | Risky | 2 | 4-2 to 5-7 | 0 | A random 4-cost you own permanently transforms into a 5-cost. |
| Major Polymorph (upgrade) | Risky | 2 | 4-2 to 5-7 | 0 | The highest star level 4-cost you own permanently transforms into a 5-cost. |
| Payday | Risky | 2 | 3-5 to 4-7 | 3 | For each ally champion that survives combat, gain 2 Gold. |
| Payday (upgrade) | Risky | 2 | 3-5 to 4-7 | 3 | For each ally champion that survives combat, gain 3 Gold. |
| Abandon Ship | Risky | 3 | 3-1 to 3-4 | 0 | Sell your board and bench, gain 6 gold and 8 rerolls. |
| Abandon Ship (upgrade) | Risky | 3 | 3-1 to 3-4 | 0 | Sell your board and bench, gain 10 gold and 10 rerolls. |
| All Fours | Shop | 1 | 5-1 to 5-7 |  | Reroll your shop with all 4-cost champions. |
| All Fours (upgrade) | Shop | 1 | 5-1 to 5-7 |  | Reroll your shop with all 4-cost champions. Gain 2 gold. |
| All Ones | Shop | 1 | 2-1 to 2-7 |  | Reroll your shop with all 1-cost champions. |
| All Ones (upgrade) | Shop | 1 | 2-1 to 2-7 |  | Reroll your shop with all 1-cost champions. Gain 1 Gold. |
| All Threes | Shop | 1 | 3-5 to 4-7 |  | Reroll your shop with all 3-cost champions. |
| All Threes (upgrade) | Shop | 1 | 3-5 to 4-7 |  | Reroll your shop with all 3-cost champions. Gain 2 gold. |
| All Twos | Shop | 1 | 2-1 to 3-4 |  | Reroll your shop with all 2-cost champions. |
| All Twos (upgrade) | Shop | 1 | 2-1 to 3-4 |  | Reroll your shop with all 2-cost champions. Gain 1 gold. |
| Flash Fire | Shop | 1 | 3-5 to 5-7 | 1 | Reroll your shop. Inferno lights 2 shop slots on fire, upgrading them. |
| Flash Fire (upgrade) | Shop | 1 | 3-5 to 5-7 | 1 | Reroll your shop. Inferno lights 3 shop slots on fire, upgrading them. |
| Freeroller | Shop | 1 | 2-1 to 4-1 | 2 | Gain 2 rerolls. |
| Freeroller (upgrade) | Shop | 1 | 2-1 to 4-1 | 2 | Gain 3 rerolls. |
| Search Party | Shop | 1 | 3-5 to 4-7 | 3 | Each of your shops has a 4-cost in it this round until you buy one. |
| Search Party (upgrade) | Shop | 1 | 3-5 to 4-7 | 3 | Each of your shops has a 4-cost in it this round until you buy one. |
| Starfall | Shop | 1 | 3-1 to 4-1 | 1 | The first 6 times you reroll your shop this round, the lowest cost champion becomes 2-star. |
| Starfall (upgrade) | Shop | 1 | 3-1 to 4-1 | 1 | The first 8 times you reroll your shop this round, the lowest cost champion becomes 2-star. |
| Animate Shop | Shop | 2 | 4-2 to 5-7 | 8 | For 18 seconds, your shop rerolls for free every 2 seconds. |
| Animate Shop (upgrade) | Shop | 2 | 4-2 to 5-7 | 8 | For 24 seconds, your shop rerolls for free every 2 seconds. |
| Jungling | Shop | 2 | 3-1 to 4-7 | 4 | Overrun the shop with Riftbeasts. |
| Jungling (upgrade) | Shop | 2 | 3-1 to 4-7 | 4 | Overrun the shop with Riftbeasts. |
| Lucky 7 | Shop | 2 | 6-1 to 10-1 |  | Gain 7 shop rerolls. |
| Lucky 7 (upgrade) | Shop | 2 | 6-1 to 10-1 |  | Gain 10 shop rerolls. |
| Middle Path | Shop | 2 | 3-5 to 4-7 |  | Reroll your shop with all 2-star 3-cost champions. |
| Middle Path (upgrade) | Shop | 2 | 3-5 to 4-7 |  | Reroll your shop with all 2-star 3-cost champions. |
| Refreshing Light | Shop | 2 | 3-5 to 4-7 |  | Gain a free reroll per star level of each unique Solar champion. |
| Refreshing Light (upgrade) | Shop | 2 | 3-5 to 4-7 |  | Gain a free reroll per star level of each unique Solar champion. |
| Rolling Bones | Shop | 2 | 3-1 to 4-7 | 3 | You have a 50% chance to get a free reroll with each kill. |
| Rolling Bones (upgrade) | Shop | 2 | 3-1 to 4-7 | 3 | 60% chance to get a reroll with each kill, and 1 more if you win. |
| Roly-Polys | Shop | 2 | 3-1 to 4-1 | 3 | For each ally champion alive after 18 seconds, gain a reroll. |
| Roly-Polys (upgrade) | Shop | 2 | 3-1 to 4-1 | 3 | For each ally champion alive after 16 seconds, gain a reroll. |
| All Fives | Shop | 3 | 6-1 to 10-1 |  | Reroll your shop with all 5-cost champions. |
| All Fives (upgrade) | Shop | 3 | 6-1 to 10-1 |  | Reroll your shop with all 5-cost champions. Gain 5 gold. |
| Border Village | Shop | 3 | 3-1 to 4-1 |  | Reroll your shop with all 2-star 2-cost champions. |
| Border Village (upgrade) | Shop | 3 | 3-1 to 4-1 |  | Reroll your shop with all 2-star 2-cost champions. |
| Lucky 7 | Shop | 3 |  |  | Gain 777 shop rerolls. |
| Ride the Wave | Shop | 3 | 4-2 to 5-7 | 3 | The next 2 rerolls this round will have 2+ champions fielded last combat. |
| Ride the Wave (upgrade) | Shop | 3 | 4-2 to 5-7 | 3 | The next 3 rerolls this round will have 2+ champions fielded last combat. |
| Starting Town | Shop | 3 | 2-1 to 3-4 |  | Reroll your shop with all 2-star 1-cost champions. |
| Starting Town (upgrade) | Shop | 3 | 2-1 to 3-4 |  | Reroll your shop with all 2-star 1-cost champions. |

## Opening encounters (20)

Source: MetaTFT lookup "encounters".

| Encounter | Effect |
|---|---|
| Golden Gala | Leona changes all augments to gold tier this game. |
| Prismatic Party | Elder Dragon changes all augments to prismatic tier this game. |
| Cheaper Levels | Ashe modifies each level to cost 2 XP less. |
| Completed Item Anvil | Yunara grants you a completed item anvil. |
| Component Anvils | Sivir grants you two component anvils! |
| Emblem Ensemble | Lux begins your journey with 3 random emblems. |
| Final Showdown | The last two surviving players gain 70 gold for an epic final fight. |
| Golden Finale | Ivern turns the last augment to gold this game. |
| Golden Prelude | Amumu changes the first augment to be gold this game. |
| Gold Subscription | Kobuko grants you gold at the start of every stage. |
| Howling Abyss | Gain a team of 5 random 1-cost champions. |
| Item Forge | All players start with 2 completed item anvils. |
| Prismatic Finisher | Karma changes the last augment to be prismatic this game. |
| No Encounter | It's a calm day in the Enchanted Wilds. No encounter this game. |
| Prismatic Opener | Morgana changes the first augment to prismatic this game. |
| Reroll Subscription | Kayle grants you free Shop rerolls every stage. |
| Scouting Party | Gain +1 Augment rerolls this game. |
| Upgraded Start | Leblanc upgrades your starting champion to 2-star. |
| 2 Cost Start | Start with a random 2-cost champion this game. |
| 3 Cost Start | Start with a random 3-cost champion this game. |
