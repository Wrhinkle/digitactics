# TFT Set 15: K.O. Coliseum

Source: CommunityDragon `https://raw.communitydragon.org/15.23/cdragon/tft/en_us.json` (set data `TFTSet15`) for champions, stats, abilities, traits, items and augments. MetaTFT lookup `https://data.metatft.com/lookups/TFTSet15_latest_en_us.json` (retrieved 2026-10-15) for spell calculation formulas used to fill computed tooltip values, augment rarity where CDragon has none, and set-mechanic lists. Power Up rules: mobalytics.gg "Set 15 mechanic: Power Ups" and invenglobal.com Set 15 coverage.

Conventions: `a/b/c` means 1-star/2-star/3-star. Ability numbers are base values at 100 Ability Power; values written as `N% AD` (or `% Armor`, `% MR`, `% max HP`) scale with that stat. `[Name: unknown]` marks a value the source data did not expose. Attack speed is attacks per second; range is in hexes.

## Overview

| Field | Value |
|---|---|
| Set | 15 |
| Name | K.O. Coliseum |
| Riot launch patch | TFT 15.1 |
| League client version at launch (CDragon path) | 15.15 |
| Live dates | July 30, 2025 to December 2, 2025 |
| Theme | Anime fighting tournament (Battle Academia, Soul Fighter, Mighty Mech, Star Guardian, Supreme Cells, Luchador, The Crew). |
| Data snapshot used | Client 15.23 (last patch of the set) |
| Champion count in this file | 64 |

## Set mechanics and system changes

- Power Ups: twice per game (rounds 1-3 and 3-6) the player receives a Power Snax item. Using it on a champion opens a choice of three Power Ups for that champion. A Power Up is a champion-attached ability modifier; it can later be removed and moved to another unit, or replaced with a new one. In the data, Power Ups are stored as traits with the "MechanicTrait" prefix; they are listed in their own section below.
- Portals are gone from the data (no portal list for Set 15). Opening encounters remain.
- Crystal Gambit earns Gem Power from kills and losses and offers a cash-out or Double Down every 3 player combats; Mentor is active only with exactly 1 or 4 unique Mentors; Rogue Captain (Twisted Fate) draws Bounty Cards each round; Stance Master lets Lee Sin pick a stance (Duelist, Executioner, Juggernaut) with its own ability and trait; Monster Trainer lets Lulu choose which monster replaces her in combat. See traits.

## Champions (64)

| Name | Cost | Traits | HP | Armor | MR | AD | AS | Range | Mana (start/max) |
|---|---|---|---|---|---|---|---|---|---|
| Aatrox | 1 | Mighty Mech, Juggernaut, Heavyweight | 650 | 40 | 40 | 60 | 0.65 | 1 | 20/80 |
| Ezreal | 1 | Battle Academia, Prodigy | 500 | 20 | 20 | 50 | 0.7 | 4 | 0/50 |
| Garen | 1 | Battle Academia, Bastion | 700 | 40 | 40 | 55 | 0.6 | 1 | 20/80 |
| Gnar | 1 | Luchador, Sniper | 500 | 20 | 20 | 50 | 0.7 | 6 | 0/40 |
| Kalista | 1 | Soul Fighter, Executioner | 500 | 20 | 20 | 55 | 0.7 | 4 | 10/90 |
| Kayle | 1 | Wraith, Duelist | 500 | 25 | 25 | 30 | 0.7 | 4 | 0/0 |
| Kennen | 1 | Supreme Cells, Protector, Sorcerer | 700 | 40 | 40 | 45 | 0.65 | 1 | 30/85 |
| Lucian | 1 | Mighty Mech, Sorcerer | 500 | 20 | 20 | 30 | 0.7 | 4 | 0/40 |
| Malphite | 1 | The Crew, Protector | 700 | 55 | 55 | 60 | 0.55 | 1 | 0/100 |
| Naafiri | 1 | Soul Fighter, Juggernaut | 650 | 45 | 45 | 55 | 0.8 | 1 | 0/30 |
| Rell | 1 | Star Guardian, Bastion | 750 | 40 | 40 | 50 | 0.6 | 1 | 0/40 |
| Sivir | 1 | The Crew, Sniper | 450 | 20 | 20 | 50 | 0.7 | 6 | 0/50 |
| Syndra | 1 | Crystal Gambit, Star Guardian, Prodigy | 450 | 20 | 20 | 40 | 0.7 | 4 | 0/30 |
| Zac | 1 | Wraith, Heavyweight | 650 | 40 | 40 | 60 | 0.55 | 1 | 30/75 |
| Dr. Mundo | 2 | Luchador, Juggernaut | 800 | 40 | 40 | 50 | 0.6 | 1 | 30/90 |
| Gangplank | 2 | Mighty Mech, Duelist | 750 | 55 | 55 | 50 | 0.8 | 1 | 0/50 |
| Janna | 2 | Crystal Gambit, Protector, Strategist | 600 | 25 | 25 | 35 | 0.7 | 4 | 10/60 |
| Jhin | 2 | Wraith, Sniper | 550 | 25 | 25 | 64 | 0.9 | 6 | 0/4 |
| Kai'Sa | 2 | Supreme Cells, Duelist | 550 | 25 | 25 | 45 | 0.7 | 4 | 0/50 |
| Katarina | 2 | Battle Academia, Executioner | 800 | 55 | 55 | 35 | 0.75 | 1 | 0/30 |
| Kobuko | 2 | Mentor, Heavyweight | 800 | 40 | 40 | 50 | 0.65 | 1 | 30/85 |
| Lux | 2 | Soul Fighter, Sorcerer | 550 | 20 | 20 | 35 | 0.75 | 4 | 10/50 |
| Rakan | 2 | Battle Academia, Protector | 800 | 50 | 50 | 50 | 0.6 | 1 | 25/75 |
| Shen | 2 | The Crew, Bastion, Edgelord | 800 | 45 | 45 | 60 | 0.6 | 1 | 25/75 |
| Vi | 2 | Crystal Gambit, Juggernaut | 800 | 45 | 45 | 50 | 0.6 | 1 | 20/75 |
| Xayah | 2 | Star Guardian, Edgelord | 550 | 25 | 25 | 45 | 0.75 | 4 | 20/70 |
| Xin Zhao | 2 | Soul Fighter, Bastion | 700 | 45 | 45 | 60 | 0.65 | 1 | 10/65 |
| Ahri | 3 | Star Guardian, Sorcerer | 650 | 30 | 30 | 40 | 0.75 | 4 | 10/50 |
| Caitlyn | 3 | Battle Academia, Sniper | 650 | 30 | 30 | 55 | 0.8 | 6 | 0/60 |
| Darius | 3 | Supreme Cells, Heavyweight | 850 | 60 | 60 | 50 | 0.8 | 1 | 0/30 |
| Jayce | 3 | Battle Academia, Heavyweight | 850 | 60 | 60 | 90 | 0.7 | 1 | 0/40 |
| Kog'Maw | 3 | Monster Trainer | 650 | 30 | 30 | 15 | 0.8 | 6 | 0/50 |
| Lulu | 3 | Monster Trainer | 650 | 25 | 25 | 45 | 0.75 | 4 | 0/100 |
| Malzahar | 3 | Wraith, Prodigy | 800 | 30 | 30 | 40 | 0.7 | 4 | 0/30 |
| Neeko | 3 | Star Guardian, Protector | 850 | 50 | 50 | 45 | 0.6 | 1 | 20/70 |
| Rammus | 3 | Monster Trainer | 1100 | 60 | 60 | 50 | 0.65 | 1 | 30/90 |
| Senna | 3 | Mighty Mech, Executioner | 650 | 30 | 30 | 55 | 0.75 | 4 | 0/60 |
| Smolder | 3 | Monster Trainer | 650 | 30 | 30 | 50 | 0.75 | 4 | 0/50 |
| Swain | 3 | Crystal Gambit, Bastion, Sorcerer | 950 | 50 | 50 | 60 | 0.6 | 1 | 20/80 |
| Udyr | 3 | Mentor, Juggernaut, Duelist | 900 | 50 | 50 | 45 | 0.7 | 1 | 40/80 |
| Viego | 3 | Soul Fighter, Duelist, Edgelord | 850 | 60 | 60 | 30 | 0.85 | 1 | 0/40 |
| Yasuo | 3 | Mentor, Edgelord | 850 | 60 | 60 | 70 | 0.75 | 1 | 0/40 |
| Ziggs | 3 | The Crew, Strategist | 650 | 30 | 30 | 20 | 0.8 | 4 | 0/90 |
| Akali | 4 | Supreme Cells, Executioner | 1050 | 65 | 65 | 30 | 0.7 | 1 | 0/40 |
| Ashe | 4 | Crystal Gambit, Duelist | 850 | 35 | 35 | 68 | 0.8 | 4 | 0/70 |
| Jarvan IV | 4 | Mighty Mech, Strategist | 1100 | 60 | 60 | 60 | 0.65 | 1 | 45/150 |
| Jinx | 4 | Star Guardian, Sniper | 850 | 35 | 35 | 70 | 0.75 | 6 | 10/80 |
| K'Sante | 4 | Wraith, Protector | 950 | 60 | 60 | 60 | 0.7 | 1 | 30/90 |
| Karma | 4 | Mighty Mech, Sorcerer | 850 | 35 | 35 | 40 | 0.75 | 4 | 0/65 |
| Leona | 4 | Battle Academia, Bastion | 1200 | 60 | 60 | 60 | 0.6 | 1 | 30/90 |
| Poppy | 4 | Star Guardian, Heavyweight | 1100 | 60 | 60 | 65 | 0.6 | 1 | 45/105 |
| Ryze | 4 | Mentor, Executioner, Strategist | 850 | 35 | 35 | 50 | 0.8 | 4 | 10/60 |
| Samira | 4 | Soul Fighter, Edgelord | 850 | 45 | 45 | 50 | 0.75 | 4 | 0/15 |
| Sett | 4 | Soul Fighter, Juggernaut | 1100 | 60 | 60 | 60 | 0.7 | 1 | 40/100 |
| Volibear | 4 | Luchador, Edgelord | 1050 | 65 | 65 | 80 | 0.85 | 1 | 0/40 |
| Yuumi | 4 | Battle Academia, Prodigy | 850 | 35 | 35 | 40 | 0.8 | 4 | 0/40 |
| Braum | 5 | The Champ, Luchador, Bastion | 1200 | 70 | 70 | 80 | 0.75 | 1 | 0/60 |
| Gwen | 5 | Soul Fighter, Sorcerer | 1150 | 60 | 60 | 20 | 0.85 | 2 | 0/40 |
| Lee Sin | 5 | Stance Master | 1200 | 75 | 75 | 70 | 0.9 | 1 | 0/100 |
| Seraphine | 5 | Star Guardian, Prodigy | 900 | 40 | 40 | 50 | 0.8 | 4 | 0/140 |
| Twisted Fate | 5 | Rogue Captain, The Crew | 900 | 40 | 40 | 30 | 0.85 | 4 | 40/120 |
| Varus | 5 | Wraith, Sniper | 900 | 40 | 40 | 60 | 0.8 | 6 | 10/85 |
| Yone | 5 | Mighty Mech, Edgelord | 1300 | 75 | 75 | 40 | 0.9 | 1 | 80/180 |
| Zyra | 5 | Crystal Gambit, Rosemother | 750 | 30 | 30 | 60 | 0.75 | 4 | 40/100 |

### Abilities

**Aatrox** (1-cost) - Blade Vigor: Heal 100/105/110 + 4% max HP (AP+HP) and deal 25/40/55 + 120/180/270% AD (AD+AP) physical damage to the current target. Healing and damage are increased by up to 100% based on Aatrox's missing health.

**Ezreal** (1-cost) - Rising Spell Force: Fire a blast at the current target, dealing 230/350/600 (AP) magic damage and 115/175/300% AD physical damage. Potential ((live counter)set14AmpIcon): (live counter)

**Garen** (1-cost) - Mighty Blade of Justice: Heal 330/360/390 + 8% max HP (HP+AP) and deal 10% max HP + 80/100/125% AD (HP+AD) physical damage to the target. Potential ((live counter)set14AmpIcon): (live counter)

**Gnar** (1-cost) - Gadagadagada!: Passive: Gain 8% (AP) Attack Speed for each attack against the same target, up to 80% (AP). Active: For 6 seconds, gain 90% Attack Damage and grant two nearby allies 20/25/35% Attack Damage, prioritizing Attack champions.

**Kalista** (1-cost) - Spirit Spear: Hurl a spear at the farthest enemy, dealing 60/90/135 + 380/570/900% AD physical damage to the first enemy hit. If the enemy dies the spear continues, dealing overkill damage to the next enemy hit.

**Kayle** (1-cost) - Unleash the Demon: Passive: Attacks deal 33/50/75 (AP) bonus magic damage. Gain additional power based on your Tactician level. Level 6: Every 3rd attack launches a wave that deals 50/75/110 (AP) magic damage and 20% Shreds enemies for 3 seconds. Level 9: Every attack launches a wave. Waves travel farther. Shred: Reduce Magic Resist

**Kennen** (1-cost) - Eye of the Hurricane: Heal 210/250/300 + 5% max HP (HP+AP), then deal 100/150/225 (AP) magic damage to the 2 nearest enemies and Stun them for 1 seconds.

**Lucian** (1-cost) - Twin Cannon Fire: Fire 4 shots towards the target. Each deals 85/130/225 (AP) magic damage to the first enemy hit. For each miss, restore 8/8/15 mana.

**Malphite** (1-cost) - Regolith Resistance: Passive: Gain 12 Armor for each item equipped. Active: Deal 40/60/90 + 270/405/610% Armor (Armor+AP) physical damage to enemies in a cone.

**Naafiri** (1-cost) - Spinning Soul-Saw: Gain 90/110/150 (AP) shield for 2 seconds, then deal 150/225/340% AD physical damage to the target.

**Rell** (1-cost) - Unbreakable Celestial Lance: Deal 130/195/295 (AP) magic damage in a short line through the target, then reduce damage taken by 20/30/45 (AP) for 4 seconds.

**Sivir** (1-cost) - Star Cutter: Launch a returning blade 2-hexes through the current target. It deals 20/30/45 + 170/255/385% AD (AD+AP) physical damage to enemies hit, reduced by 40% per hit.

**Syndra** (1-cost) - Bright Star Blast: Deal 215/325/485 (AP) magic damage to the target and reduces their Magic Resist by 10.

**Zac** (1-cost) - Reality Ripple: Heal 240/280/350 (AP) and deal 100/150/225 + 7% max HP (AP+HP) magic damage to enemies within a 1-hex radius.

**Dr. Mundo** (2-cost) - Steel Chair Slam: Passive: Every 7 seconds the next attack deals 8% max HP + 50/75/125% AD (AD+HP) bonus physical damage. Active: Gain 240/280/350 (AP) maximum health over 1.5 seconds and immediately trigger the passive.

**Gangplank** (2-cost) - Golden Giga Gun: Deal 305/460/815% AD physical damage to the lowest health enemy within 2 hexes. If the target dies, [ModifiedGoldChance: unknown]% AP chance to gain 1 gold. Gold Earned: (live counter)

**Janna** (2-cost) - Crystal Chrysalis: Grant 80/100/135 (AP) shield to the 2 lowest Health allies for 4 seconds and fire 4 butterflies dealing [DamageTotal: unknown] (AP) magic damage split between the 2 closest enemies.

**Jhin** (2-cost) - The Curtain Falls: Passive: Attack Speed is always 0.75/0.75/0.85. Convert 1% bonus Attack Speed into 0.8% Attack Damage. Every fourth attack is enhanced to deal 30/45/70 + 190/285/440% AD physical damage.

**Kai'Sa** (2-cost) - Missile Massacre: Passive: Kills grant 1% permanent Attack Damage (+(live counter)% AD). Active: Fire 8 missiles divided between the target and nearby enemies. Each missile deals 6/8/14 + 34/51/80% AD (AD+AP) physical damage.

**Katarina** (2-cost) - Teleports Behind You: Blink to the enemy with the lowest health within 2 hexes and deal 140/210/325 (AP) magic damage to enemies in a 1-hex radius. Potential ((live counter)set14AmpIcon): (live counter)

**Kobuko** (2-cost) - Golden Paw Mantra: Gain 325/375/450 (AP) Shield for 3 seconds. The next attack deals 18% max HP + 100/125/155% AD (AD+HP) bonus physical damage and Stuns for 1.5 seconds.

**Lux** (2-cost) - Soulfire Explosion: Fire an energy ball towards the largest group of enemies containing the target. On arrival, 20% Shred enemies in a 1-hex radius for 5 seconds and deal 300/450/700 (AP) magic damage split between them plus 90/135/210 (AP) magic damage to each. Shred: Reduce Magic Resist

**Rakan** (2-cost) - All Eyes On Me: Heal 350/400/625 (AP) and deal 55/85/130 (AP) magic damage to the 3 closest enemies. Potential ((live counter)set14AmpIcon): (live counter)

**Shen** (2-cost) - Power Suit: Full Charge: Shield 400/475/575 (AP) for 4 seconds. The next 3 attacks deal 75/115/175% MR (MR) bonus true damage.

**Vi** (2-cost) - Shining Knuckle Explosion: Passive: Gain 2 Armor+MR for each component equipped to your team. Active: Heal 230/280/330 (AP), deal 150/225/350% AD physical damage in a 1-hex radius centered on the target, and 20% Sunder enemies hit for 4 seconds. Sunder: Reduce Armor

**Xayah** (2-cost) - Sharp Feathers Shine: Gain 80% (AP) attack speed for 3 attacks. These attacks deal 50/75/115% AD bonus physical damage to the target and 2 nearby enemies

**Xin Zhao** (2-cost) - Guarding Crescent: Heal 290/360/450 (AP), deal 100/150/235% AD physical damage to enemies in a 2-hex radius, and mark the current target. For 3 seconds, take 40% reduced damage from unmarked enemies.

**Ahri** (3-cost) - Pure Heart Breaker: Deal 460/690/1275 (AP) magic damage to the current target, increased up to 50% based on the target's missing health. If the target dies, deal 80% of overkill damage split between the 2 closest enemies, plus 10/15/25 (AP) magic damage per target.

**Caitlyn** (3-cost) - Magic Bouncing Bullet: Fire a bullet at the farthest enemy, dealing 30/45/70 + 330/495/790% AD (AD+AP) physical damage to the first target hit. The bullet bounces to a nearby enemy, dealing 85/130/205% AD physical damage. Potential ((live counter)set14AmpIcon): (live counter)

**Darius** (3-cost) - Grand Guillotine: Heal 90/130/170 (AP), then deal 215/325/520% AD physical damage to the current target, increased by 60% against Tanks. If the target dies, immediately recast dealing 75% damage.

**Jayce** (3-cost) - Thundering Hammerstrike: Deal 20/30/50 + 200/300/480% AD (AD+AP) physical damage to enemies in a 1-hex radius centered on the target. Potential ((live counter)set14AmpIcon): (live counter)

**Kog'Maw** (3-cost) - Static Surge: Passive: Attacks deal 44/66/110 (AP) bonus magic damage. Increase Max Attack Speed to 6. Active: The next attack deals 55/85/130 (AP) bonus magic damage to the target and the 2 closest enemies, then 30% Shred and Sunder them for 4 seconds. On-cast, gain 15% Attack Speed for the rest of combat. Shred: Reduce Magic Resist Sunder: Reduce Armor

**Lulu** (3-cost) - Choose A Monster: Field Lulu to choose a monster to train! Each monster has unique abilities.

**Malzahar** (3-cost) - Insidious Whispers: Infect the closest 1 uninfected targets, dealing 515/775/1315 (AP) magic damage over 15 seconds. If an infected target dies, the infection spreads to the nearest uninfected target for any remaining time. If all enemies are infected, this effect can stack.

**Neeko** (3-cost) - Falling Flower Eruption: Gain 40% Durability and heal 300/350/450 (AP) over 2 seconds, then deal 120/180/290 (AP) magic damage to enemies in a 2-hex radius.

**Rammus** (3-cost) - Durian Defense: Gain 550/675/800 (AP) Shield for 4 seconds. Leap to the largest group within 2 hexes, dealing 40/60/100% Armor + 40/60/100% MR (Armor+MR) magic damage and Taunting enemies hit. Gain 15/35/55 (AP) max Health per enemy Taunted. When the Shield breaks, shoot spines at the nearest 3 enemies, repeating the damage and Stunning them for 1/1.25/1.5 second.

**Senna** (3-cost) - Super Beam Cannon: Over 1 second, deal 40/60/95 + 400/600/960% AD (AD+AP) physical damage in a line through the current target. Enemies after the first take [SecondaryDamage: unknown] (AD+AP) instead. When no targets are left, regain mana based on the time remaining.

**Smolder** (3-cost) - Burn Blast: Passive: Damage Burns and Wounds for 5 seconds. Ignore 30% of the target's Armor. Active: Deal 290/435/700% AD physical damage to the target, and 50/75/120 (AP) magic damage to the 3 nearest targets. Every 2 casts, fire a second blast at the next closest enemy, dealing 75% damage. {{TFT_Keyword_Burn}} {{TFT_Keyword_Wound}}

**Swain** (3-cost) - Glittering Plumage Regalia: Gain 300/340/380 + 10% max HP (HP+AP) max Health, then deal 25/35/55 (AP) magic damage every second to enemies in a 2-hex radius. Subsequent casts heal 300/340/380 + 10% max HP (HP+AP) and increase the damage by 6/9/14 (AP).

**Udyr** (3-cost) - Blazing Phoenix Stance: Make 3 rapid attacks. Each heals Udyr for 150/190/250 (AP) and deals 35/50/80% AD additional damage to enemies in a 1-hex radius.

**Viego** (3-cost) - Combo of the Ruined King: Passive: Attacks deal 30/45/80 + 100% AD (AD+AP) magic damage. Active: The next three attacks unleash a devastating combo. Pommel Bash: Deal 100/150/270 (AP) magic damage. Rising Slash: Deal 100/150/270 (AP) magic damage and briefly Stun the target. Falling Slash: Deal 210/315/560 (AP) magic damage.

**Yasuo** (3-cost) - Unsheath the Storm: Deal 30/45/70 + 140/210/335% AD physical damage to the 3 closest enemies.

**Ziggs** (3-cost) - Orbital Ordinance: Passive: Attacks are bouncing bombs that deal 50/75/120 + 100% AD (AP+AD) magic damage to the first enemy hit. Active: Deal 230/345/550 (AP) magic damage in a 1-hex radius centered on the target.

**Akali** (4-cost) - Piercing Phantom Strike: Mark the farthest unmarked target within 4 hexes, then dash through all marked enemies in order. Each dash deals 95/145/1000 (AP) magic damage to its target and 75/115/1500 (AP) magic damage to other enemies hit. If only one enemy remains, dash 3 times instead

**Ashe** (4-cost) - Gates of Avarosa: The next 8 attacks fire bonus arrows dealing [TotalDamage: unknown] (AD+AP) total physical damage split between the target and up to two random enemies within 2 hexes.

**Jarvan IV** (4-cost) - Mega Drill Crash: Gain 400/450/1500 + 15% max HP (AP+HP) Shield for 4 seconds, then slam into a group of enemies, dealing 100/150/3000 (AP) magic damage to enemies within a 2 hex radius and Stunning them for 1.5/1.75/8 seconds.

**Jinx** (4-cost) - Star Rocket Blast Off!: Passive: Attacks grant 6/6/30% (AP) stacking Attack Speed, up to 60/60/300% (AP). Critical strikes instead grant 9/9/40% (AP). Active: Deal 200/300/900% AD physical damage to the target and 575/875/4000% AD physical damage split across a 2-hex radius centered on the target.

**K'Sante** (4-cost) - Breakout Force: Passive (ALL OUT): On first death instead heal to full, gain 125/125/300% Attack Speed, convert 50% of Armor and Magic Resist into Attack Damage, and set max mana to 30. Then dash through the target and briefly Stun them. While ALL OUT, lose 6/6/0% Health each second and heal 30% max HP (HP) on kill. Active: Gain 0.45/0.5/0.85 (AP) Durability for 3 seconds, then deal 120/180/1200% Armor + 120/180/1200% MR (Armor+MR) physical damage to the target. Active (ALL OUT): . Deal 350/525/3000% AD physical damage to the target.

**Karma** (4-cost) - Destruction Disc: Fire an energy disc that deals 1160/1750/6500 (AP) total magic damage in a 1-hex radius centered on the target.

**Leona** (4-cost) - Aegis of Unbreakable Dawn: Shield for 650/775/2000 (AP) for 4 seconds and deal 50/75/800% Armor + 50/75/800% MR (Armor+MR) magic damage to enemies in a 2-hex radius. Potential ((live counter)set14AmpIcon): (live counter)

**Poppy** (4-cost) - Spinning Galaxy Hammer: Gain 320/370/900 + 10% max HP (HP+AP) Shield for 4 seconds. When it breaks, deal 250/375/4000% AD physical damage to the target and 125/200/1500% AD physical damage to other enemies in a 1-hex radius centered on the target. The target is Stunned for 0.5/0.5/8-2.5/2.5/10 seconds based on how long the shield held, and other enemies are briefly Stunned.

**Ryze** (4-cost) - Tsunami Beam: Deal 810/1215/7000 (AP) magic damage to the target over 3 seconds. Enemies in a 1-hex radius around the target take 110/165/1200 (AP) over the same duration.

**Samira** (4-cost) - Soul Shot / Inner Arsenal Unleashed: Passive: After casting, gain 1 Style and 5% Omnivamp, up to 4 stacks. At max stacks, the next cast is Inner Arsenal Unleashed. Soul Shot: Deal 105/160/650% AD physical damage to the target and dash a short distance. Inner Arsenal Unleashed: Double the passive bonus and leap to a nearby group of enemies, dealing 50/75/225 + 230/345/2200% AD (AD+AP) to all enemies in a 2-hex radius. Reset your Style to 0.

**Sett** (4-cost) - Maximum Grit Overdrive: Over a short duration, heal 260/335/1500 + 12% max HP (HP+AP) and deal 140/210/2000% AD physical damage to the target. Then, deal 120/180/750% AD + 4% max HP (HP+AD) physical damage in a cone. Enemies in the center line take [ModifiedTrueDamage: unknown] more physical damage, increased based on healing received this combat.

**Volibear** (4-cost) - Ursine Instinct: Passive: Every 4th attack deals 125/190/750% AD physical damage in a 1-hex radius centered on the target. When changing targets, immediately slam for 70% damage. Active: For 5 seconds gain 125/125/500% Attack Speed, 10/10/25% (AP) Omnivamp, and crowd control immunity.

**Yuumi** (4-cost) - Read Or Die: Launch 4 pages dealing 26/40/150 (AP) magic damage each, split between the target and enemies within 2 hexes. Each subsequent cast launches 4 more pages. Potential ((live counter)set14AmpIcon): (live counter)

**Braum** (5-cost) - El Tigre Tornado: Stun the target and spin them around for 1.5 seconds, dealing 575/865/9001% AD physical damage to the target and 310/465/4501% AD to other enemies in a 2-hex radius. Then throw the target forward, dealing 100/150/2000% AD in a 1-hex area around the target. If the target drops below [ExecuteThreshold: unknown] (AP) health, throw them off the board instead.

**Gwen** (5-cost) - Soul Stitch / Rip The Threads: Passive: Attacks deal 10/15/400 (AP) magic damage, and 40/60/1000 (AP) magic damage split between enemies in a cone. Every 3rd cast is Rip the Threads. Soul Stitch: Fire 3 needles through the board, dealing 35/55/1000 (AP) magic damage to enemies hit and leaving threads behind. Rip The Threads: Deal 320/450/9999 (AP) split between all enemies and 80/120/9001 (AP) bonus magic damage to each. Enemies near at least one thread take 10% more damage.

**Lee Sin** (5-cost) - A Demonstration of Skill: Field Lee Sin to choose a stance. Each stance has a unique ability, and grants Lee Sin a bonus trait.

**Seraphine** (5-cost) - Ultimate Friendship Bomb: Passive: Ultimate Friendship Bomb's damage scales on Power of Friendship, which is based on all of your team's bonus stats (AD+AP+Armor+MR+AS+Crit+Crit Damage+HP). Stats on Seraphine contribute more. Active: Fire a friendship bomb towards the largest group of enemies within 1 + 0.5% stat#31 (Range) hexes, dealing (live counter) (Power of Friendship) magic damage in a 2-hex radius.

**Twisted Fate** (5-cost) - 52 Card Calamity: Passive: Attacks bounce between 3 enemies, dealing 35/55/999 (AP) bonus magic damage and marking enemies hit. Attacks deal 30% reduced damage per hit. Active: Deal 230/350/9999% AD physical damage to the 4/4/20 closest enemies, and detonate their marks for 15/25/500 (AP) magic damage each.

**Varus** (5-cost) - Level Zero: 100 Shot Demon: Passive: Gain mana when units die. Enemies grant 5, allies 3. Active: Fire 3 + 200% stat#4 (AS) piercing arrows per second at random enemies until Varus is out of mana. Each arrow costs 12/12/6 mana and deals 10/15/200 + 90/135/666% AD (AD+AP) physical damage, reduced by 35% per hit.

**Yone** (5-cost) - Dual Blade Technique: Inner Reflection: Passive: Attacks grant 5/5/25% stacking Attack Speed and Movement Speed, and alternate between dealing 25/40/400% AD bonus true damage and 100/160/999 (AP) bonus magic damage. Active: Send an echo towards the two furthest enemies, dealing 140/210/9999% AD physical damage and briefly Stunning enemies hit.

**Zyra** (5-cost) - Crystal Thorn Expansion: Summon 4/4/20 untargetable Thornspitters that deal 90/135/999 (AP) per attack and attack 4 times. Non-plant allies gain 35/50/1009% (AP) Attack Speed decaying over 4 seconds.

Other units in the set data without traits (summons, PvE monsters, mode or tutorial units): Tome of Traits, Completed Item Anvil, Component Anvil, Artifact Item Anvil, Support item anvil, Mercenary Chest, The Mighty Mech, Grasping Root, Deadly Spine, Ekko, Test Cost, Golem, Rift Scuttler, Training Dummy.

## Traits (26)

### Bastion

Your team gains 10 Armor and Magic Resist. Bastions gain more, and the value doubles in the first 10 seconds of combat.

| Units | Effect |
|---|---|
| 2-3 | 18 Armor+MR |
| 4-5 | 40 Armor+MR |
| 6 | 75 Armor+MR; Non-Bastions gain an additional 25 Armor+MR. |

Champions: Garen (1), Rell (1), Shen (2), Xin Zhao (2), Swain (3), Leona (4), Braum (5)

### Battle Academia

Battle Academia champions upgrade their abilities and gain Potential set14AmpIcon. Potential improves their abilities. Prismatic: Field 7 Battle Academia champions, then earn 140 points by equipping items on champions. Prismatic: Earn 1 point for every Completed item a champions uses in combat. Graduate at 140 points. ((live counter)/140). True Potential Unlocked: +10 set14AmpIcon

| Units | Effect |
|---|---|
| 3-4 | 3 set14AmpIcon |
| 5-6 | 5 set14AmpIcon |
| 7 | 7 set14AmpIcon |
| 7 | DA 0.5; DR 0.25; NumPotential 7; PrismaticItemRequirement 140; UltimateNumPotential 10 |

Champions: Ezreal (1), Garen (1), Katarina (2), Rakan (2), Caitlyn (3), Jayce (3), Leona (4), Yuumi (4)

### Crystal Gambit

Kills and losses during player combats earn Gem Power. Every 3 player combats choose to convert Gem Power into rewards or Double Down. While Double Down is active losses grant 100% more Gem Power, but wins lose 50% Gem Power and cash out immediately. Gem Power gains increase at stages 3 and 4. .

| Units | Effect |
|---|---|
| 3-4 | (live counter) per kill; (live counter) per loss |
| 5-6 | AND every (live counter) kills, gain bonus loot and reroll a new kill number. (Kills: (live counter)) |
| 7-9 | 130% rewards. Crystal Gambit units gain 300 HP and 15% Damage Amp |
| 10 | 7PeiceMult 1.3; 7PieceAmp 0.15; 7PieceHealth 300; DamageReduction 0.99; GPPerDeathPVE 3; GPPerRoundPVE 15; NumCombats 3; NumCombatsPM 2; NumCombatsPVE 3; PercentIncreaseGPPVE 1; PercentIncreasedGP 1; PercentLossPVE 0.5; PercentLost 0.5 |

Champions: Syndra (1), Janna (2), Vi (2), Swain (3), Ashe (4), Zyra (5)

### Duelist

Duelists gain Attack Speed on each attack, stacking up to 12 times.

| Units | Effect |
|---|---|
| 2-3 | 4% AS |
| 4-5 | 7% AS |
| 6 | 10% AS; Duelists gain 12% Durability |

Champions: Kayle (1), Gangplank (2), Kai'Sa (2), Udyr (3), Viego (3), Ashe (4)

### Edgelord

Edgelords gain Omnivamp and Attack Damage. While attacking enemies under 50% Health, they gain 40% Attack Speed.

| Units | Effect |
|---|---|
| 2-3 | 10% Omnivamp, 15% AD |
| 4-5 | 12% Omnivamp, 30% AD |
| 6 | 15% Omnivamp, 60% AD |

Champions: Shen (2), Xayah (2), Viego (3), Yasuo (3), Samira (4), Volibear (4), Yone (5)

### Executioner

Executioners gain Critical Strike Chance and Critical Strike Damage. Their Ability can critically strike.

| Units | Effect |
|---|---|
| 2 | 25% Crit; 10% Crit Damage |
| 3 | 35% Crit; 12% Crit Damage |
| 4 | 50% Crit; 18% Crit Damage |
| 5 | 55% Crit; 28% Crit Damage |

Champions: Kalista (1), Katarina (2), Senna (3), Akali (4), Ryze (4)

### Heavyweight

Your team gains 100 Health. Heavyweights gain additional bonus Health, and Attack Damage equal to a percentage of their Health.

| Units | Effect |
|---|---|
| 2-3 | 20% HP / 0.2% HP to AD |
| 4-5 | 40% HP / 0.4% HP to AD |
| 6 | 65% HP / 0.6% HP to AD |

Champions: Aatrox (1), Zac (1), Kobuko (2), Darius (3), Jayce (3), Poppy (4)

### Juggernaut

Juggernauts gain Durability, increased above 50% health. When a Juggernaut dies, other Juggernauts heal for 10% of their max Health.

| Units | Effect |
|---|---|
| 2-3 | 15% or 25% Durability |
| 4-5 | 20% or 30% Durability |
| 6 | 25% or 35% Durability |

Champions: Aatrox (1), Naafiri (1), Dr. Mundo (2), Vi (2), Udyr (3), Sett (4)

### Luchador

Luchadors gain bonus Attack Damage. At 50% health, Luchadors cleanse negative effects, heal, and leap back into the fight, Stunning enemies in a 1-hex radius for 1 seconds.

| Units | Effect |
|---|---|
| 2-3 | 15% AD; 25% HP heal |
| 4 | 40% AD; 50% HP heal |

Champions: Gnar (1), Dr. Mundo (2), Volibear (4), Braum (5)

### Mentor

This trait is active only when you have exactly 1 or 4 unique Mentors. Kobuko: 6% Damage Reduction Udyr: 8% Attack Damage and Ability Power Yasuo: 10% Attack Speed Ryze: Attacks grant 2 bonus Mana

| Units | Effect |
|---|---|
| 1 | Allies gain the bonus |
| 2-3 | Mentors gain all bonuses and upgrade their abilities. Other allies gain nothing. |
| 4 | ADAP 0.08; AS 0.1; Durability 0.06; Mana 2 |

Champions: Kobuko (2), Udyr (3), Yasuo (3), Ryze (4)

### Mighty Mech

Gain The Mighty Mech. Mighty Mechs heal it for 12% of the damage they deal. Unlock the Mightiest Mech Each Mighty Mech champion's star level increases The Mighty Mech's power.

| Units | Effect |
|---|---|
| 3-4 | The Mighty Mech arrives |
| 5-6 | Blade Protocol: Level 1 |
| 7 | Blade Protocol: Level 2 |
| 7 | AutoAttackDamage 120; GalioHealRatio 0.12; Health 2000; NumItems 3 |

Champions: Aatrox (1), Lucian (1), Gangplank (2), Senna (3), Jarvan IV (4), Karma (4), Yone (5)

### Monster Trainer

Choose which monster Lulu summons to replace her in combat!

| Units | Effect |
|---|---|
| 1 | FirstEvolveLevel 15; HealthGainPerLevel 0.01; SecondEvolveLevel 30 |

Champions: Kog'Maw (3), Lulu (3), Rammus (3), Smolder (3)

### Prodigy

Your team gains Mana Regen. Prodigies gain more.

| Units | Effect |
|---|---|
| 2 | 1 TFTManaRegen / 3 TFTManaRegen |
| 3 | 1 TFTManaRegen / 4 TFTManaRegen |
| 4 | 1 TFTManaRegen / 6 TFTManaRegen |
| 5 | 1 TFTManaRegen / 7 TFTManaRegen / Prodigy abilities heal an ally for 12% of the damage dealt. |

Champions: Ezreal (1), Syndra (1), Malzahar (3), Yuumi (4), Seraphine (5)

### Protector

Units gain 5% Durability while shielded. Once per combat at 50% Health, Protectors shield themselves and their closest ally for a percent of their maximum Health. Shields stack.

| Units | Effect |
|---|---|
| 2-3 | 18% HP shield |
| 4-5 | 36% HP shield |
| 6 | 55% HP shield |

Champions: Kennen (1), Malphite (1), Janna (2), Rakan (2), Neeko (3), K'Sante (4)

### Rogue Captain

Twisted Fate upgrades the Crew Ship to deal 15% of it's damage as true damage and draws Bounty Cards each round that grant random rewards. Bounty Cards get better for each player combat. (Rounds Fielded: (live counter)) Current Bounty Card: (live counter) In Choncc's Treasure, Twisted Fate will not draw a Bounty Card until Stage 3-5.

| Units | Effect |
|---|---|
| 1 | Bounty_CasterEmpower_DA 0.02; Bounty_FreeRerolls_NumRerolls 2; Bounty_GainAttackSpeed_AS 0.15; Bounty_GainGoldLarge_MinGold 12; Bounty_GainGold_MinGold 2; Bounty_GainXP_Amount 12; Bounty_Omnivamp_Omnivamp 0.15; Bounty_PermanentAP_AP 18; Bounty_TankEmpower_MaxHealth 40; Bounty_TeamShield_Duration 8; Bounty_TeamShield_Shield 160; Bounty_TheCrew_GainHealth_PermanentADAP 5; Bounty_TheCrew_GainHealth_PermanentHealth 60; Bounty_TheCrew_GoldPerStar_GoldAmount 1; Bounty_TheCrew_RocketUpgrade_TrueDamagePercent 0.15; Bounty_ThinkFast_Cadence 3; Bounty_ThinkFast_Gold 5 |

Champions: Twisted Fate (5)

### Rosemother

Gain 1/1/8 placeable plants, based on Zyra's star level. Plants in the front two rows grow into durable Grasping Roots, while plants in the back two rows grow into Deadly Spines. Rosemother plants benefit from Zyra's Ability Power and Attack Speed. When Zyra casts, her plant restores 35% Health and gains 35% Attack Speed for the rest of combat. If the plant is dead, she revives it instead.

| Units | Effect |
|---|---|
| 1 | 1Star 1; 2Star 1; 3Star 8; BonusHealthAndAS 0.35 |

Champions: Zyra (5)

### Sniper

Snipers gain Damage Amp, increased against targets farther away.

| Units | Effect |
|---|---|
| 2 | 13% Damage Amp; +3% Damage Amp per hex |
| 3 | 18% Damage Amp; +5% Damage Amp per hex |
| 4 | 25% Damage Amp; +7% Damage Amp per hex |
| 5 | 25% Damage Amp; +10% Damage Amp per hex |

Champions: Gnar (1), Sivir (1), Jhin (2), Caitlyn (3), Jinx (4), Varus (5)

### Sorcerer

Sorcerers gain bonus Ability Power. When an enemy dies after being damaged by a Sorcerer, they deal a percentage of that enemy's maximum Health to another enemy.

| Units | Effect |
|---|---|
| 2-3 | 20% AP; 8% max HP |
| 4-5 | 55% AP; 10% max HP |
| 6 | 80% AP; 12% max HP to 2 enemies |

Champions: Kennen (1), Lucian (1), Lux (2), Ahri (3), Swain (3), Karma (4), Gwen (5)

### Soul Fighter

Soul Fighters gain bonus Health, and gain Attack Damage and Ability Power every second up to 8 stacks. At max stacks deal bonus true damage. Prismatic: Field 8, then win 10 combats. Prismatic: Defeat 10 players in combat. ((live counter)) MAXIMUM SOUL POWER.

| Units | Effect |
|---|---|
| 2-3 | 120 HP, 1% AD+AP, +10% damage |
| 4-5 | 240 HP, 2.5% AD+AP, +16% damage |
| 6-7 | 425 HP, 4% AD+AP, +22% damage |
| 8 | 600 HP, 4% AD+AP, +28% damage |
| 8 | BonusTrueDamage 0.3; FlatHealth 700; MaxStacks 8; PlayerDamageToPrismatic 10; StatsPerSecond 0.04 |

Champions: Kalista (1), Naafiri (1), Lux (2), Xin Zhao (2), Viego (3), Samira (4), Sett (4), Gwen (5)

### Stance Master

When you field Lee Sin, choose between Duelist Stance, Executioner Stance, and Juggernaut Stance! Each stance has a unique ability, and grants Lee Sin the associated trait.

Champions: Lee Sin (5)

### Star Guardian

Star Guardians have a unique Teamwork bonus that is granted to all Star Guardians. Every Star Guardian fielded increases the bonus! Prismatic: Field 8, then spend 16000 mana. Prismatic: Spend 16000 mana ((live counter)) THE STARS AWAKEN. Rell: Gain (live counter) shieldRell: Gain shields Syndra: Gain (live counter)% Ability Power every 3 secondsSyndra: Gain Ability Power Xayah: Every 3rd attack deals (live counter) (increases per stage) magic damageXayah: Magic damage on attack Ahri: After casting, gain (live counter) Mana over 2 secondsAhri: On cast, gain Mana Neeko: +(live counter)% healing and shieldingNeeko: Increase heals & shields Poppy: At 40% Health, heal (live counter)% HPPoppy: Heal at low Health Jinx: +(live counter)% Attack Speed. +(live counter)% AS after takedowns, decaying over 3sJinx: Gain Attack Speed Seraphine: Gain (live counter) Armor+MR+HP and (live counter)% AD+AS+Crit+Crit DamageSeraphine: Gain every stat Emblem: Increase bonuses by 10%!Emblem: Increase other bonuses

| Units | Effect |
|---|---|
| 2 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1; SyndraTimer 3 |
| 3 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.05; SyndraTimer 3 |
| 4 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.1; SyndraTimer 3 |
| 5 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.12; SyndraTimer 3 |
| 6 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.14; SyndraTimer 3 |
| 7 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.16; SyndraTimer 3 |
| 8 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.18; SyndraTimer 3 |
| 8 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.18; SyndraTimer 3 |
| 9 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.2; SyndraTimer 3 |
| 9 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.2; SyndraTimer 3 |
| 10 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 1.5; SyndraTimer 3 |
| 10 | AhriDuration 2; EmblemBonus 0.1; ManaToPrismatic 16000; Multiplier 2; SyndraTimer 3 |

Champions: Rell (1), Syndra (1), Xayah (2), Ahri (3), Neeko (3), Jinx (4), Poppy (4), Seraphine (5)

### Strategist

Combat Start: Allies in the front 2 rows gain a shield for 15 seconds. Allies in the back 2 rows gain Damage Amp. Strategists gain triple.

| Units | Effect |
|---|---|
| 2 | 150 Shield; 4% Damage Amp |
| 3 | 200 Shield; 6% Damage Amp |
| 4 | 300 Shield; 10% Damage Amp |
| 5 | 400 Shield; 14% Damage Amp |

Champions: Janna (2), Ziggs (3), Jarvan IV (4), Ryze (4)

### Supreme Cells

The Cell who dealt the most damage last combat is Supreme. When the Supreme Cell dies, the Cell with the highest current damage becomes Supreme. Cells gain Damage Amp. The Supreme Cell gains more and executes enemies under 10% Health.

| Units | Effect |
|---|---|
| 2 | 8% Damage Amp / 12% Damage Amp |
| 3 | 18% Damage Amp / 30% Damage Amp |
| 4 | 28% Damage Amp / 50% Damage Amp. Gain a second Supreme |

Champions: Kennen (1), Kai'Sa (2), Darius (3), Akali (4)

### The Champ

The Champ's victories against players grant Poro-fans equal to his star level. On loss, Poro-fans prevent 1 Tactician damage each, then lose all of your Poro-fans. Poro Fanbase: (live counter)

| Units | Effect |
|---|---|
| 1 | PctPorosLost 100; PlayerDamagePrevented 1; PlayerHealthPerVictoryDU 1; PoroAmountForHealPVE 20; PorosPerVictory 1; PorosPerVictoryDU 1; TacticianHealPVE 1 |

Champions: Braum (5)

### The Crew

Crew champions gain 5% Health and Attack Speed for each Crew member fielded. Every 3-star Crew champion grants an additional bonus. (1x 3StarEnabled3StarDisabled): +1 XP per paid reroll, and Crew unit odds never drop with player level. (2x 3StarEnabled3StarDisabled): +1 free reroll / round. (3x 3StarEnabled3StarDisabled): Deal 280 damage to fire a rocket dealing 75. (4x 3StarEnabled3StarDisabled): Fire rockets 2.5% more often per player level. (5x 3StarEnabled3StarDisabled): Fire the Planet Cracker.

| Units | Effect |
|---|---|
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |
| 2 | BaseDamageThreshold 280; BaseRocketDamage 75; RocketRatePerLevel 0.03; StatBonus 0.05; XPPerReroll 1 |

Champions: Malphite (1), Sivir (1), Shen (2), Ziggs (3), Twisted Fate (5)

### Wraith

Every 4 seconds, the Shadow Realm strikes the 3 closest enemies, dealing total magic damage equal to a portion of damage dealt by Wraiths since the last trigger. Your lowest health Wraith heals for 18% of damage dealt by Wraiths and the Shadow Realm.

| Units | Effect |
|---|---|
| 2-3 | 20% damage. |
| 4-5 | 45% damage. |
| 6 | 60% damage. |

Champions: Kayle (1), Zac (1), Jhin (2), Malzahar (3), K'Sante (4), Varus (5)

## Power Ups (147)

### 100 Push Ups

Gain [TFTTrait.TFT15_MechanicTrait_100PushUps.1:DR: unknown]% Damage Reduction. For every 3 rerolls you spend gold on, gain [TFTTrait.TFT15_MechanicTrait_100PushUps.1:DAPerReroll: unknown]% permanent Damage Amp. (Current: (live counter)% Damage Amp)

| Units | Effect |
|---|---|
| 1 | DAPerReroll 0.01; DR 0.15; NumRerolls 3 |

### 2X Contender

Vi, Ahri, Darius, Yasuo, Jinx, Braum, and Ekko gain 5% Damage Amp and 3% Durability for each unique copy fielded. This unit gains 75% more.

| Units | Effect |
|---|---|
| 1 | Amp 0.05; DR 0.03; percent 0.75 |

### 2XKO

After two takedowns, stun enemies within 2 hexes for 2 seconds and gain 35% Damage Amp for the rest of combat.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.35; StunDuration 2 |

### 404 Power Up

Gain 8% Attack Damage, Ability Power, Damage Amp, and 8 Armor and Magic Resistance. How did you find this?

| Units | Effect |
|---|---|
| 1 | Stats 8 |

### Adaptive Skin

When taking damage, gain [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:ResistsPerDamageInstancePhysical: unknown] Armor or [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:ResistsPerDamageInstanceMagic: unknown] Magic Resistance based on the damage type, up to [TFTTrait.TFT15_MechanicTrait_AdaptiveSkin.1:TotalResists: unknown] total resists.

| Units | Effect |
|---|---|
| 1 | ResistsPerDamageInstanceMagic 10; ResistsPerDamageInstancePhysical 3; TotalResists 80 |

### All Out

At combat start go ALL OUT, gaining 20% Damage Reduction and 60% Attack Damage. Health no longer decays while ALL OUT.

| Units | Effect |
|---|---|
| 1 | AD 0.6; Durability 0.2 |

### Annihilation

Gain 12% Damage Amp, increased to 28% Damage Amp after the first takedown each combat.

| Units | Effect |
|---|---|
| 1 | BaseDA 12; IncreasedDA 28 |

### Artistic KO

Every 4th attack deals 35% more damage. Overkill damage bounces to the nearest enemy.

| Units | Effect |
|---|---|
| 1 | IncreasedDamage 0.35; NumEnemies 1 |

### Assassinate

| Units | Effect |
|---|---|
| 1 | ([TFTTrait.TFT15_MechanicTrait_Rogue.1:MinUnits: unknown]) Combat Start: Jump into the enemy's backline and deal 20% less damage for the first 8 seconds of combat. |
| 2 | ([TFTTrait.TFT15_MechanicTrait_Rogue.2:MinUnits: unknown]) Both champions jump. |

### Atomic

Store 10% max Health at combat start and 2% more every second. On death, deal the stored Health as magic damage in a large radius.

| Units | Effect |
|---|---|
| 1 | PercentMaxHealthInitial 10; PercentMaxHealthPerSecond 2 |

### Attack Expert

Gain 5% Attack Damage. All other sources of Attack Damage give [TFTTrait.TFT15_MechanicTrait_AttackExpert.1:PctBonusAD: unknown]% more.

| Units | Effect |
|---|---|
| 1 | BaseAD 0.05; PctBonusAD 35 |

### Attempt #37

Ekko's spell prioritizes the closest enemy, but he heals 20% Max Health on cast and deals 6% more magic damage each time his spell hits the target.

| Units | Effect |
|---|---|
| 1 | BonusDamagePerAttack 0.06; HealPercent 0.2 |

### Best Defense

When granting or receiving shields, add 100% of the shield's value as bonus physical damage on the next attack.

| Units | Effect |
|---|---|
| 1 | PercentDamage 1 |

### Bestest Boy

Every 5 seconds, Lulu feeds a snack to her Monster: 13% HP Healing for Rammus, 14% Attack Damage for Smolder, or 14% Ability Power for Kog'Maw.

| Units | Effect |
|---|---|
| 1 | AD 0.14; AP 14; Cadence 5; PercentHeal 0.13 |

### Bladenado

Attacks deal 25% of Basic Attack Damage as physical damage to enemies within attack range of the target.

| Units | Effect |
|---|---|
| 1 | PercentDamage 0.25 |

### Blink Attack

When switching targets, blink to the next target. The next attack deals [TFTTrait.TFT15_MechanicTrait_TimeSkip.1:PctAdditionalDamage: unknown]% more as magic damage.

| Units | Effect |
|---|---|
| 1 | PctAdditionalDamage 200 |

### Blood Brothers

Damage dealt by Yone and Yasuo bleeds enemies for 20% of damage dealt over 2 seconds.

| Units | Effect |
|---|---|
| 1 | BleedDuration 2; BleedPct 20 |

### Bludgeoner

Attacks and Abilities ignore 45% of enemy's Armor.

| Units | Effect |
|---|---|
| 1 | PercentIgnore 45 |

### Body Change

At combat start, take the form of the nearest ally, granting them and this champion a [TFTTrait.TFT15_MechanicTrait_BodyChange.1:PctHealth: unknown]% Health shield.

| Units | Effect |
|---|---|
| 1 | PctHealth 0.3 |

### Bonus Bloom

Gain an additional plant. Plants deal 20% less damage and have 30% less Health.

| Units | Effect |
|---|---|
| 1 | PctOfDamage 0.2; PctOfHealth 0.3 |

### Bullet Hell

Fire [TFTTrait.TFT15_MechanicTrait_BulletHell.1:IncreasedProjectiles: unknown]% more projectiles with abilities.

| Units | Effect |
|---|---|
| 1 | IncreasedProjectiles 30 |

### Caretaker

On cast, the lowest percent Health ally gains [TFTTrait.TFT15_MechanicTrait_Caretaker.1:ShieldAmount: unknown] (AP) shield for [TFTTrait.TFT15_MechanicTrait_Caretaker.1:Duration: unknown] seconds.

| Units | Effect |
|---|---|
| 1 | Duration 6; ShieldAmount 135 |

### Classy

This champion's last-listed trait is [TFTTrait.TFT15_MechanicTrait_Classy.1:IncreasedEffectiveness: unknown]% more effective on them.

| Units | Effect |
|---|---|
| 1 | IncreasedEffectiveness 45 |

### Colossal

Take up two unit slots. Gain Health, Durability, and Damage Amp.

| Units | Effect |
|---|---|
| 1 | 900 HP; 20% Durability; 10% Damage Amp. |
| 2 | 1250 HP; 25% Durability; 20% Damage Amp. |

### Corner Carry

Gain 30% Attack Speed. At 75% and 25% health, dash to the safest position and gain infinite Range.

| Units | Effect |
|---|---|
| 1 | AS 0.3 |

### Corrosive

30% Shred and Sunder enemies within 2 hexes. Take 10% less damage from Shredded or Sundered enemies.

| Units | Effect |
|---|---|
| 1 | DamageReduction 10; Shred 30 |

### Corrupted

Start dormant. Gain [TFTTrait.TFT15_MechanicTrait_Corrupted.1:DAPerSoul: unknown]% Damage Amp and [TFTTrait.TFT15_MechanicTrait_Corrupted.1:HealthPerSoul: unknown] Health whenever any unit dies. At [TFTTrait.TFT15_MechanicTrait_Corrupted.1:HealthThreshold: unknown]% Health, awaken and blink toward enemies, stunning them for [TFTTrait.TFT15_MechanicTrait_Corrupted.1:StunDuration: unknown] seconds.

| Units | Effect |
|---|---|
| 1 | DAPerSoul 0.05; HealthPerSoul 80; HealthThreshold 0.6; StunDuration 1.5 |

### Crimson Veil

Gain 20% Omnivamp. Overhealing from Omnivamp converts to shield. (Max: 40% Health)

| Units | Effect |
|---|---|
| 1 | MaxPercentHealthShield 40; Omnivamp 20 |

### Critical Threat

Abilities can Critically Strike. Every [TFTTrait.TFT15_MechanicTrait_CriticalThreat.1:Interval: unknown] seconds, gain [TFTTrait.TFT15_MechanicTrait_CriticalThreat.1:CriticalStrikeChance: unknown]% Critical Strike Chance.

| Units | Effect |
|---|---|
| 1 | CriticalStrikeChance 5; Interval 3 |

### Cyclone Rush

Gain Attack Speed. At combat start, disable the opposite enemy for 3 seconds.

| Units | Effect |
|---|---|
| 1 | 15% AS. |
| 2 | 25% AS. Disable two enemies. |

### Dark Amulet

Heal for 15% of magic damage dealt. On cast, take 8% max Health as true damage, but gain 12% stacking Ability Power.

| Units | Effect |
|---|---|
| 1 | HealPct 15; HealthLossPct 8; StackingAP 12 |

### Demolitionist

Ability damage Stuns for [TFTTrait.TFT15_MechanicTrait_Demolitionist.1:StunDuration: unknown] second.

| Units | Effect |
|---|---|
| 1 | StunDuration 1 |

### Denting Blows

Vi's spell now shields for 4 seconds instead of healing, but is increased by 30%. When converted, strike quickly strike 4 times, each hit reducing the targets resists by 5.

| Units | Effect |
|---|---|
| 1 | FlurryCount 4; ResistShred 5; ShieldBoost 0.3; ShieldDuration 4 |

### Desperado

After every 8th attack, fire five projectiles at the closest enemies that deal 100% of Basic Attack Damage as physical damage.

| Units | Effect |
|---|---|
| 1 | NumAttacks 8; PercentBADDamage 100 |

### Doom Barrage

Arrows cost 2 less Mana during Varus's Ability.

| Units | Effect |
|---|---|
| 1 | ManaReduction 2 |

### Doomsayer

After [TFTTrait.TFT15_MechanicTrait_Doomsayer.1:Countdown: unknown] seconds, if still alive, deal [TFTTrait.TFT15_MechanicTrait_Doomsayer.1:PercentOfDamageTaken: unknown]% of damage taken this combat as magic damage to all enemies.

| Units | Effect |
|---|---|
| 1 | Countdown 15; PercentOfDamageTaken 0.4 |

### Double Down

Gain 18% Damage Amp. When this unit casts, your other Powered Up unit gains 20% of the Mana spent.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.18; ManaPercent 0.2 |

### Doublestrike

Attacks have a [TFTTrait.TFT15_MechanicTrait_Doublestrike.1:ProcChance: unknown]% chance to trigger an additional attack.

| Units | Effect |
|---|---|
| 1 | ProcChance 30 |

### Dread Note

| Units | Effect |
|---|---|
| 1 | ([TFTTrait.TFT15_MechanicTrait_DreadNote.1:MinUnits: unknown]) If alive at [TFTTrait.TFT15_MechanicTrait_DreadNote.1:FirstDeath: unknown] and [TFTTrait.TFT15_MechanicTrait_DreadNote.1:SecondDeath: unknown] seconds, kill the enemy with the most Health |
| 2 | ([TFTTrait.TFT15_MechanicTrait_DreadNote.2:MinUnits: unknown]) At [TFTTrait.TFT15_MechanicTrait_DreadNote.2:FirstDeath: unknown] and [TFTTrait.TFT15_MechanicTrait_DreadNote.2:SecondDeath: unknown] seconds instead. |

### Drift Duo

Senna and Lucian both gain 3 mana regen. If both are 3-star, gain +1 Mighty Mech.

| Units | Effect |
|---|---|
| 1 | ManaRegen 3 |

### Efficient

Abilities cost 15 less Mana to cast.

| Units | Effect |
|---|---|
| 1 | Reduction 15 |

### Essence Share

On cast, the 2 nearest allies gain [TFTTrait.TFT15_MechanicTrait_EssenceShare.1:ManaPct: unknown]% of Mana spent.

| Units | Effect |
|---|---|
| 1 | ManaPct 0.35; NumAllies 2 |

### Fairy Tail

On cast spawn 2 fairies that each deal [TFTTrait.TFT15_MechanicTrait_FairyTail.1:APDamage1: unknown]-[TFTTrait.TFT15_MechanicTrait_FairyTail.1:APDamage4: unknown] (based on Stage) magic damage to the target over the next [TFTTrait.TFT15_MechanicTrait_FairyTail.1:Duration: unknown] seconds.

| Units | Effect |
|---|---|
| 1 | APDamage1 75; APDamage2 120; APDamage3 170; APDamage4 260; Duration 8 |

### Fan Service

Xayah and Rakan gain bonus stats based on each other's star level.

| Units | Effect |
|---|---|
| 1 | 8% AD+AS / 150 HP, 5 Armor+MR |
| 2 | 15% AD+AS / 275 HP, 10 Armor+MR |

### Fighter Spirit

| Units | Effect |
|---|---|
| 1 | Gain [TFTTrait.TFT15_MechanicTrait_FighterSpirit.1:Health: unknown] Health, [TFTTrait.TFT15_MechanicTrait_FighterSpirit.1:ADAP: unknown]% Attack Damage, and [TFTTrait.TFT15_MechanicTrait_FighterSpirit.1:ADAP: unknown]% Ability Power for each Fighter or Assassin fielded. |

### Final Ascent

Ascend an additional time at levels 7 and 10. Level 7: Gain 6% Ability Power every 3 attacks. Level 10: Launch 2 additional waves that deal 60% damage.

| Units | Effect |
|---|---|
| 1 | APPerAttack 6; Level1 7; Level2 10; NumAttacks 3; ReducedWaveDamage 0.6 |

### Final Boss

Gain 10% Omnivamp. Whenever an ally dies, gain 4.5% stacking Damage Amp.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.05; Omnivamp 0.1 |

### Final Form

After round 5-2 if this champion is 3-star, become 4-star at combat start.

| Units | Effect |
|---|---|
| 1 | AppropriateRound 2; AppropriateRoundEventPM 6; AppropriateRoundPVE 7; AppropriateStage 5; AppropriateStageEventPM 3; AppropriateStagePVE 2 |

### Finalist

Gain [TFTTrait.TFT15_MechanicTrait_Finalist.1:InitialDA: unknown]% Damage Amp and [TFTTrait.TFT15_MechanicTrait_Finalist.1:InitialDR: unknown]% Damage Reduction, increased by [TFTTrait.TFT15_MechanicTrait_Finalist.1:IncreasedDA: unknown]% Damage Amp and [TFTTrait.TFT15_MechanicTrait_Finalist.1:IncreasedDR: unknown]% Durability for each eliminated player.

| Units | Effect |
|---|---|
| 1 | IncreasedDA 0.03; IncreasedDR 0.03; InitialDA 0.15; InitialDR 0.08 |

### Frost Touch

Attacks have a 15% chance to Stun the target for 1 second.

| Units | Effect |
|---|---|
| 1 | DamagePerStage 55; PercentageChanceToTrigger 15; StunDuration 1 |

### Fury Break

Gain 30% Attack Speed. When your other Powered Up unit dies, gain a burst of 300% Attack Speed that decays over 4 seconds. If you die first, give them the burst instead.

| Units | Effect |
|---|---|
| 1 | AS 0.3; ASBurst 3; Duration 4 |

### Fusion Dance

At combat start, fuse with the nearest ally, gaining [TFTTrait.TFT15_MechanicTrait_FusionDance.1:StatShare: unknown]% of their Health, Attack Damage, and Ability Power.

| Units | Effect |
|---|---|
| 1 | StatShare 0.45 |

### Gather Force

On cast, gain stacking Attack Damage equal to 40% of Mana spent.

| Units | Effect |
|---|---|
| 1 | ADPctFromMana 40 |

### Gem Core

The Mighty Mech converts 8% of damage taken into Attack Damage and Ability Power, and gains 100 Armor and Magic Resist. Crystal Rose champions are treated as Mighty Mechs for the Mech's abilities.

| Units | Effect |
|---|---|
| 1 | Absorption 0.08; Resists 100 |

### Golden Edge

Attacking 72 times grants (live counter) gold. This gold value increases by 1 each time it activates. (Attacks: (live counter) / 72)

| Units | Effect |
|---|---|
| 1 | NumAttacksPerProc 72 |

### Golden Gloves

Whenever you gain interest, permanently gain 2% Attack Damage per gold collected. (Current Bonus: (live counter)% AD)

| Units | Effect |
|---|---|
| 1 | AD 0.02 |

### Hat Trick

Gain 16% Attack Damage and Ability Power. Each takedown, don a hat that grants 1% of each. On death, lose 50% of your hats. (Current Hats: (live counter))

| Units | Effect |
|---|---|
| 1 | ADAPPerHat 1; BaseADAP 16; PercentHatLoss 0.5 |

### Heart of Gold

If this champion survives a player combat, gain 2 gold.

| Units | Effect |
|---|---|
| 1 | NumGold 2 |

### Hemorrhage

Abilities deal 60% bonus damage as true damage over 4 seconds.

| Units | Effect |
|---|---|
| 1 | BleedDuration 4; PercentBleed 60 |

### Hero's Arc

Gain [TFTTrait.TFT15_MechanicTrait_HerosArc.1:DA: unknown]% Damage Amp per player level. At level 10 gain an additional [TFTTrait.TFT15_MechanicTrait_HerosArc.1:DamageIncrease: unknown]% Damage Amp.

| Units | Effect |
|---|---|
| 1 | DA 0.05; DamageIncrease 0.35 |

### Hungry Hero

Gain [TFTTrait.TFT15_MechanicTrait_BloodFury.1:BaseHealth: unknown] Health and [TFTTrait.TFT15_MechanicTrait_BloodFury.1:BaseAD: unknown]% Attack Damage. Each round, you may pay [TFTTrait.TFT15_MechanicTrait_BloodFury.1:PlayerHealthPerSacrifice: unknown] player health for [TFTTrait.TFT15_MechanicTrait_BloodFury.1:HealthPerSacrifice: unknown] HP and [TFTTrait.TFT15_MechanicTrait_BloodFury.1:ADPerSacrifice: unknown] AD. (Total: (live counter) HP (live counter) AD)

| Units | Effect |
|---|---|
| 1 | ADPerSacrifice 0.04; BaseAD 0.1; BaseAP null; BaseHealth 100; HealthPerSacrifice 150; PlayerHealthPerSacrifice 3 |

### Hyperactive

Takedowns grant 100% Attack Speed that rapidly decays over 3 seconds.

| Units | Effect |
|---|---|
| 1 | AttackSpeed 100; DecayDuration 3 |

### Ice Bender

Combat Start: Ryze is frozen for the first 3 seconds of combat. His Ability 20% Chills enemies for 1 second, and deals 20% bonus damage.

| Units | Effect |
|---|---|
| 1 | Chill 20; ChillDuration 1; Duration 3; PctSpellDamage 0.2 |

### Infernal Speed

Gain [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:BaseAS: unknown]% Attack Speed. On takedown, generate a Cinder. Grant your team [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:CinderAS: unknown]% bonus AS for every [TFTTrait.TFT15_MechanicTrait_InfernalSpeed.1:NumCinders: unknown] Cinders. (Cinders: (live counter); Team AS: (live counter)%)

| Units | Effect |
|---|---|
| 1 | BaseAS 10; CinderAS 2; NumCinders 8 |

### Inner Fire

Every 2 seconds, Burn and Wound the closest non-Burning enemy for 10 seconds. Take 15% less damage from Burning enemies.

| Units | Effect |
|---|---|
| 1 | DamageReduction 0.15; Duration 10; ICD 2 |

### Kahunahuna

Every [TFTTrait.TFT15_MechanicTrait_Kahunahuna.1:NumAttacks: unknown] attacks, deal [TFTTrait.TFT15_MechanicTrait_Kahunahuna.1:ADDamage: unknown] (AD) true damage.

| Units | Effect |
|---|---|
| 1 | ADDamage 175; NumAttacks 6 |

### Keen Eye

Attacks and Abilities ignore 45% of enemy's Magic Resistance.

| Units | Effect |
|---|---|
| 1 | PercentIgnore 45 |

### Killer Instinct

Abilities target the lowest Health enemy in range instead. Gain [TFTTrait.TFT15_MechanicTrait_KillerInstinct.1:ManaRegen: unknown] Mana Regen.

| Units | Effect |
|---|---|
| 1 | ManaRegen 3 |

### Living Wall

Gain 20 Armor and Magic Resistance. At combat start allies in the back row gain 1.5% Attack Speed for every 30 combined Armor and MR this champion has.

| Units | Effect |
|---|---|
| 1 | AS 1.5; PerXResists 30; Resists 20 |

### Mage

Cast Abilities twice, but deal [TFTTrait.TFT15_MechanicTrait_Mage.1:DAMultiplier: unknown]% less total damage.

| Units | Effect |
|---|---|
| 1 | DAMultiplier 0.2 |

### Magic Expert

Gain [TFTTrait.TFT15_MechanicTrait_MagicExpert.1:BaseAP: unknown]% Ability Power. All other sources of Ability Power give [TFTTrait.TFT15_MechanicTrait_MagicExpert.1:PctBonusAP: unknown]% more.

| Units | Effect |
|---|---|
| 1 | BaseAP 10; PctBonusAP 35 |

### Mana Rush

Start combat with full Mana.

### Mastermind

At combat start, gain 50 Mana and give 25 Mana to all allies in a line in front of this champion.

| Units | Effect |
|---|---|
| 1 | AllyMana 25; SelfishMana 50 |

### Max Arcana

Gain 15% Ability Power, and permanently gain 1% every 2 takedowns. (Current: (live counter)% AP)

| Units | Effect |
|---|---|
| 1 | Amount 1; Base 15 |

### Max Attack

Gain 12% Attack Damage, and permanently gain 1% every 2 takedowns. (Current: (live counter)% AD)

| Units | Effect |
|---|---|
| 1 | Amount 0.01; Base 0.12 |

### Max Speed

Gain 15% Attack Speed, and permanently gain 1% every 3 takedowns. (Current: (live counter)% AS)

| Units | Effect |
|---|---|
| 1 | Amount 0.01; Base 0.15 |

### Max Vitality

At combat start gain 40 permanent Health. Every 10 seconds this champion survives, gain 20 additional permanent Health. (Current: (live counter) HP)

| Units | Effect |
|---|---|
| 1 | Amount 20; Interval 10; PerCombatAmount 40; PerCombatAmountEventPM 80 |

### Mech Pilot

| Units | Effect |
|---|---|
| 1 | Pilot the Mech, granting it your stats: 35% HP, 45% AD+AP+AS. Eject when at 60%. |
| 2 | The Mech benefits from the pilots' traits. |

### Mechablade

Gain Attack Speed equal to 5% maximum Health. Attacks grant 5 bonus Mana.

| Units | Effect |
|---|---|
| 1 | BonusMana 5; PercentHealthAsAS 0.05 |

### Mechador

When you field Mighty Mech and Luchador, The Mighty Mech gains 100 Health and becomes a Luchador granting +1 to the trait.

| Units | Effect |
|---|---|
| 1 | Health 100 |

### Midas Touch

Attacks and Abilities execute enemies below 10% Health. Kills have a 35% chance to drop a gold. (Gold: (live counter))

| Units | Effect |
|---|---|
| 1 | PercentChanceGold 35; PercentHealthThreshold 10 |

### Mind Battery

After the first cast each combat, gain 5 Mana Regen and 30% Ability Power.

| Units | Effect |
|---|---|
| 1 | AP 30; ManaRegen 5 |

### Momentum

(1) Gain 60% Attack Speed and moves significantly faster.

| Units | Effect |
|---|---|
| 1 | BonusAS 0.6; BonusMS 0.5 |

### Needlework

Fire 2 bonus needles with every 1st and 2nd cast that deal 45% damage.

| Units | Effect |
|---|---|
| 1 | BonusNeedles 2; ReducedDamage 0.45 |

### Not Done Yet

Once this champion has died 6 times, gain [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:PctHealth: unknown]% Health, [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:Omnivamp: unknown]% Omnivamp, and [TFTTrait.TFT15_MechanicTrait_NotDoneYet.1:Stats: unknown]% Damage Amp. (Deaths: (live counter))

| Units | Effect |
|---|---|
| 1 | NumDeathsStage2 6; NumDeathsStage3 6; NumDeathsStage4 6; Omnivamp 10; PctHealth 18; Stats 25 |

### Noxious Trap

Damage dealt Burns, Wounds, 20% Shreds, and 20% Sunders enemies for 4 seconds.

| Units | Effect |
|---|---|
| 1 | Duration 4 |

### On The Edge

Start combat at 65% Health, but gain 35% Damage Amp.

| Units | Effect |
|---|---|
| 1 | DA 0.35; HealthPercent 0.65 |

### Ordinary

If this champion has no active traits, gain [TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:HP1: unknown]-[TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:HP4: unknown] Health and [TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:AS1: unknown]-[TFTTrait.TFT15_MechanicTrait_BuiltDifferent.1:AS4: unknown]% Attack Speed (based on current Stage).

| Units | Effect |
|---|---|
| 1 | AS1 40; AS2 55; AS3 80; AS4 100; HP1 300; HP2 400; HP3 750; HP4 1000; Multiplier 0.2 |

### Over 9000

At the start of each round, gain a random permanent stat bonus. Current: +(live counter)% AD, +(live counter)% AP, +(live counter)% AS, +(live counter) Armor, +(live counter) MR, +(live counter) HP, +(live counter) TFTManaRegen

| Units | Effect |
|---|---|
| 1 | AD 0.04; AP 4; AS 0.04; Armor 8; Health 35; MR 8; Mana 1 |

### Pack Tactics

Gain 2 packmates that deal 22% (AD) physical damage. When Naafiri uses her Ability, packmates dash to the lowest Health enemy within 2 hexes.

| Units | Effect |
|---|---|
| 1 | DamagePercent 0.22; NumHexes 2; NumPackmates 2 |

### Power Font

At combat start and every 3 seconds, gain 1 Mana Regen.

| Units | Effect |
|---|---|
| 1 | Cadence 3; Mana 1 |

### Precision

Set Attack Speed to [TFTTrait.TFT15_MechanicTrait_Precision.1:ForcedAttackSpeed: unknown]. Convert [TFTTrait.TFT15_MechanicTrait_Precision.1:ASScale: unknown]% Attack Speed to [TFTTrait.TFT15_MechanicTrait_Precision.1:ADScale: unknown]% Attack Damage. Attacks deal [TFTTrait.TFT15_MechanicTrait_Precision.1:AttackAmount: unknown]% more damage and grant [TFTTrait.TFT15_MechanicTrait_Precision.1:Mana: unknown] more Mana.

| Units | Effect |
|---|---|
| 1 | ADScale 0.01; ASScale 0.01; AttackAmount 1.3; ForcedAttackSpeed 0.7; Mana 5 |

### Pure Heart

Every 4 seconds heal this champion and allies within 2-hexes for 18% of their missing Health.

| Units | Effect |
|---|---|
| 1 | HealTickRate 4; MissingHealthHeal 0.18 |

### Pursuit

Lucian's Ability dashes, fires +1 additional bullets, and grants 8% Ability Power for the rest of combat.

| Units | Effect |
|---|---|
| 1 | APGain 8; NumAdditionalShots 1 |

### Quick Tag

On death, spawn a unit of the same role with the same number of items. This unit is 1 cost lower, and has 15% reduced Health.

| Units | Effect |
|---|---|
| 1 | PercentHealth 0.15 |

### Ramping Rage

Attacks grant 3.5% stacking Attack Speed.

| Units | Effect |
|---|---|
| 1 | ASperAttack 0.04 |

### Rare Treat

Your Monster has +15 Monster Trainer levels.

| Units | Effect |
|---|---|
| 1 | Levels 15 |

### Regenerative

Heal [TFTTrait.TFT15_MechanicTrait_MetabolicExplosion.1:PctRegen: unknown]% max Health every [TFTTrait.TFT15_MechanicTrait_MetabolicExplosion.1:Interval: unknown] seconds. Gain 10% increased healing from all sources.

| Units | Effect |
|---|---|
| 1 | IncreasedHealing 10; Interval 3; PctRegen 6.5 |

### Resistant

Reduce damage taken by [TFTTrait.TFT15_MechanicTrait_Resistant.1:FlatDR: unknown].

| Units | Effect |
|---|---|
| 1 | FlatDR 25; Resists null |

### Rising Chaos

On cast, fire an additional orb that deals 7% damage to a random enemy. Subsequent casts add an additional orb.

| Units | Effect |
|---|---|
| 1 | PercentDamage 0.07 |

### Robo Ranger

Gain [TFTTrait.TFT15_MechanicTrait_RobustRanger.1:StatsPct: unknown]% of The Mighty Mech's Attack Damage and Ability Power, and [TFTTrait.TFT15_MechanicTrait_RobustRanger.1:HealthPct: unknown]% of its Health.

| Units | Effect |
|---|---|
| 1 | HealthPct 10; StatsPct 50 |

### Rocket Grab

At combat start, gain a 20% Health shield for 4 seconds. When your target dies, pull the next target into melee range and regain the shield.

| Units | Effect |
|---|---|
| 1 | ShieldDuration 4; ShieldPercent 0.2 |

### Round Two

On death, summon a Training Dummy equipped with the same items. It has [TFTTrait.TFT15_MechanicTrait_RoundTwo.1:PercentHealth: unknown]% of this unit's health.

| Units | Effect |
|---|---|
| 1 | PercentHealth 60 |

### S.O.L.E. Fighter

If this champion is the only Soul Fighter fielded, it gains the benefits of Soul Fighter (4).

### Selfish

Gain [TFTTrait.TFT15_MechanicTrait_Selfish.1:DR: unknown]% Durability. Heal for [TFTTrait.TFT15_MechanicTrait_Selfish.1:HealPct: unknown]% of damage dealt by all other allies.

| Units | Effect |
|---|---|
| 1 | DR 10; HealPct 5 |

### Serious Slam

The first attack on each enemy deals [TFTTrait.TFT15_MechanicTrait_SeriousSlam.1:AttackScale: unknown]% (AD) physical damage and Stuns for [TFTTrait.TFT15_MechanicTrait_SeriousSlam.1:KnockupDuration: unknown] seconds.

| Units | Effect |
|---|---|
| 1 | AttackScale 2.1; KnockupDuration 1 |

### Shadow Clone

Create a perfect copy of this champion with the same items equipped that deals [TFTTrait.TFT15_MechanicTrait_ShadowJutsu.1:DamagePercent: unknown]% damage and has [TFTTrait.TFT15_MechanicTrait_ShadowJutsu.1:HealthMod: unknown]% health.

| Units | Effect |
|---|---|
| 1 | DamagePercent 0.22; HealthMod 0.5 |

### Shine Brighter

Every other cast, Lux's spell explodes in a 2-hex radius, and deals 30% more damage.

| Units | Effect |
|---|---|
| 1 | percent 0.3 |

### Singularity

Become a 2-hex singularity, dealing 1% (HP) as magic damage every second. When enemies die within the radius, gain 15 permanent Health. (Total: (live counter) HP)

| Units | Effect |
|---|---|
| 1 | NumHexes 2; PercentHealth 0.01; PermanentHealth 15 |

### Sky Piercer

Damage 30% Shreds and Sunders enemies for 6 seconds. Deal 5% increased damage against Shredded and Sundered enemies.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.05; Duration 6 |

### Snax Time

Braum's casts also throw 5 Poros at random enemies. Each deals 200% of Braum's Basic Attack Damage as physical damage. This is safe for Poros. Braum is a professional.

| Units | Effect |
|---|---|
| 1 | DamagePercent 2; PoroNum 5 |

### Socialite

Combat start: Mark adjacent hexes. Allies in marked hexes gain 6% Damage Amp and 60 Health. This champion gains 300% of the bonus.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.06; Health 60; ManaPerSecond null; NumRounds 99; PercentIncrease 3 |

### Solar Breath

Every 2 seconds, Burn and Wound the closest non-Burning enemy for 10 seconds. Deal 10% more damage to Burning enemies.

| Units | Effect |
|---|---|
| 1 | DamageAmp 0.1; Duration 10; HexRange 12; ICD 2 |

### Soul Chipper

Dealing magic damage reduces Magic Resistance by 5 for the rest of combat.

| Units | Effect |
|---|---|
| 1 | MagicResistReduce 5 |

### Space Ace

Damage this champion deals contributes 75% more towards launching rockets from The Crew's ship.

| Units | Effect |
|---|---|
| 1 | percent 0.75 |

### Spiky Shell

Gain 40 Armor. Enemies attacking this champion take 25 + 8 per stage physical damage. (1 second cooldown per attacker)

| Units | Effect |
|---|---|
| 1 | Armor 40; Damage 25; DamagePerStage 8; ICD 1 |

### Spirit Sword

Gain 20% Critical Strike Chance. Attacks that critically strike deal [TFTTrait.TFT15_MechanicTrait_SpiritSword.1:BonusMagicDamage: unknown]% bonus magic damage.

| Units | Effect |
|---|---|
| 1 | BonusMagicDamage 30; CritChance 0.2 |

### Stalking Dart

Every 10 Mana Ahri gains fires a foxfire dart towards her target, dealing 7.5% of her spell's damage as magic damage.

| Units | Effect |
|---|---|
| 1 | PercentDamage 0.08 |

### Stand Alone

If this champion starts combat alone in its row, gain [TFTTrait.TFT15_MechanicTrait_StandStrong.1:HealthRatio: unknown]% Health.

| Units | Effect |
|---|---|
| 1 | HealthRatio 0.3 |

### Stand United

Shen's ability also shields the lowest health ally for 50% of his shield and gains 25 stacking Magic Resist on cast.

| Units | Effect |
|---|---|
| 1 | MagicResist 25; ShieldPercent 0.5 |

### Star Sailor

| Units | Effect |
|---|---|
| 1 | Gain 33% increased bonuses from Star Guardian. |
| 2 | And heal the other Star Sailor for 15% of damage dealt. |

### Star Student

| Units | Effect |
|---|---|
| 1 | Gain 200 Health and +40% Potential set14AmpIcon |
| 2 | Gain 1 XP per round, and another 1 for each surviving Star Student. |

### Starry Knight

Gain 7% Health and 10% Ability Power for every 1-cost fielded, doubled if they're 3-star.

| Units | Effect |
|---|---|
| 1 | AP 10; Health 0.07; Mult 2 |

### Storm Bender

| Units | Effect |
|---|---|
| 1 | Summon a storm that echoes 22% of damage dealt as magic damage. |
| 2 | Both champions contribute damage. The storm is bigger. |

### Stretchy Arms

Gain 10% Attack Speed. On takedown, gain +1 Attack Range.

| Units | Effect |
|---|---|
| 1 | BaseAS 0.1 |

### Strong Spark

Every 3 seconds, zap the closest enemy for 11% of their max Health as magic damage. Whenever an enemy is Stunned by any source, zap them.

| Units | Effect |
|---|---|
| 1 | Cadence 3; Damage 0.11 |

### Super Genius

Every 2 seconds gain Ability Power equal to your Mana Regen.

| Units | Effect |
|---|---|
| 1 | Cadence 2 |

### Super Mega

Jinx gains 30% Attack Damage. Every 3rd cast is twice as big, and splits 50% more damage among enemies hit.

| Units | Effect |
|---|---|
| 1 | AD 0.3; SplitDamageIncrease 0.5 |

### Superstar

Your team gains 4% Damage Amp, increased by [TFTTrait.TFT15_MechanicTrait_Superstar.1:DAPer3Star: unknown]% for each 3-Star on your team. This champion gains 200% of the bonus.

| Units | Effect |
|---|---|
| 1 | DABase 4; DAPer3Star 3; Multiplier 2 |

### Supremacy

While this champion is the Supreme Cell, they execute enemies under 18% Health instead.

| Units | Effect |
|---|---|
| 1 | ExecutePct 0.18 |

### Supreme Surge

Whenever an enemy is executed by a Supreme Cell, heal 15% missing health and gain 15 mana.

| Units | Effect |
|---|---|
| 1 | Mana 15; percent 0.15 |

### Surge 66

Gain 20% Attack Speed and 10% Ability Power. After 15 and 66 attacks, this amount doubles. (Attacks: (live counter))

| Units | Effect |
|---|---|
| 1 | AP 10; AS 20; FirstThreshold 15; SecondThreshold 66 |

### Tank-zilla

Gain [TFTTrait.TFT15_MechanicTrait_KaijuSize.1:HealthRatio: unknown]% max Health and grow larger. After [TFTTrait.TFT15_MechanicTrait_KaijuSize.1:Countdown: unknown] seconds, Stun all enemies for 1.5 seconds.

| Units | Effect |
|---|---|
| 1 | Countdown 15; HealthRatio 0.2 |

### Team Captain

| Units | Effect |
|---|---|
| 1 | This champion's Star Guardian Teamwork bonus is increased by 15% per star level. |
| 2 | 20% bonus per star level. |

### Tentacle Smash

After 4 seconds, Stun enemies within 3 hexes for 1 second. Repeat every 8 seconds.

| Units | Effect |
|---|---|
| 1 | Cadence 8; InitialDelay 4; StunDuration 1 |

### Thrillseeker

| Units | Effect |
|---|---|
| 1 | ([TFTTrait.TFT15_MechanicTrait_Thrillseeker.1:MinUnits: unknown]) Kills heal [TFTTrait.TFT15_MechanicTrait_Thrillseeker.1:HealPercent: unknown]% max Health and grant [TFTTrait.TFT15_MechanicTrait_Thrillseeker.1:AS: unknown]% Attack Speed for 3 seconds. |
| 2 | ([TFTTrait.TFT15_MechanicTrait_Thrillseeker.2:MinUnits: unknown]) [TFTTrait.TFT15_MechanicTrait_Thrillseeker.2:AS: unknown]% AS on kill. Execute enemies below [TFTTrait.TFT15_MechanicTrait_Thrillseeker.2:ExecuteThreshold: unknown]% Health. |

### Tiny Terror

Gain [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:AS: unknown]% Attack Speed. At [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:Threshold: unknown]% Health, double it for the rest of combat. Then, shrink in size for [TFTTrait.TFT15_MechanicTrait_TinyTerror.1:Duration: unknown] seconds, dodging all attacks.

| Units | Effect |
|---|---|
| 1 | AS 0.15; Duration 3; Threshold 0.7 |

### Trickster

| Units | Effect |
|---|---|
| 1 | At low Health, gain 4% Damage Amp, spawn a Training Dummy, and dash to the furthest enemy. |
| 2 | 6% Damage Amp; Dummies gain 1250 HP |

### Ultra Stance

Gain 100 Health and grant an additional +1 to the selected Stance's trait.

| Units | Effect |
|---|---|
| 1 | BonusHealth 100; BonusTraitAmount 1 |

### Unflinching

Gain [TFTTrait.TFT15_MechanicTrait_Unflinching.1:OmnivampPct: unknown]% Omnivamp. For the first [TFTTrait.TFT15_MechanicTrait_Unflinching.1:Duration: unknown] seconds of combat, gain immunity to crowd control.

| Units | Effect |
|---|---|
| 1 | Duration 15; OmnivampPct 25 |

### Unstoppable

Gain 100 Health and immunity to crowd control. Charge to new targets, Stunning enemies passed through for 0.75 seconds.

| Units | Effect |
|---|---|
| 1 | Health 100; StunDuration 0.75 |

### Untouchable

Become untargetable while [TFTTrait.TFT15_MechanicTrait_Untouchable.1:NumAllies: unknown]+ allies are alive. If this champion ends the fight at full Health, permanently gain [TFTTrait.TFT15_MechanicTrait_Untouchable.1:DamageAmpScaling: unknown]% Damage Amp. (Current: (live counter)% Damage Amp)

| Units | Effect |
|---|---|
| 1 | DamageAmpScaling 0.02; NumAllies 4 |

### Veteran

This unit is a Mentor. They grant 4% Damage Amp, and at Mentor 4 they gain 30% Attack Damage and Ability Power instead of upgrading their spell.

| Units | Effect |
|---|---|
| 1 | Amp 0.04; Stats 0.3 |

### Warming Up

At combat start or when this activates gain [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Resists: unknown] Armor and Magic Resistance. Every second, convert [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Conversion: unknown] resists into [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Conversion: unknown]% Attack Damage. (Up to [TFTTrait.TFT15_MechanicTrait_WarmingUp.1:Resists: unknown])

| Units | Effect |
|---|---|
| 1 | Conversion 3; Resists 48 |

### Weights

Gain 10 Weights. At 1+, move and attack 50% slower. Player victories drop 1 Weights, 2 on a loss. At 0, move faster and gain 50% Attack Speed and 12% Damage Reduction. (Weights Left: (live counter))

| Units | Effect |
|---|---|
| 1 | AS 0.5; Durability 0.12; NumWeights 10; PercentReduction 0.5; WeightPerCombat 1 |

### Wind Wall

Yasuo gains 60% Crit Chance, and takes 15% reduced damage from non-adjacent enemies.

| Units | Effect |
|---|---|
| 1 | CritChance 0.6; ReducedDamage 0.15 |

### Wonder Twins

| Units | Effect |
|---|---|
| 1 | ([TFTTrait.TFT15_MechanicTrait_WonderTwins.1:MinUnits: unknown]) When fielding 2+ copies of this unit, grant both [TFTTrait.TFT15_MechanicTrait_WonderTwins.2:Stats: unknown]% of a stat based on Role. At 3-Star, gain a 2-Star copy. |
| 2 | ([TFTTrait.TFT15_MechanicTrait_WonderTwins.2:MinUnits: unknown]) [TFTTrait.TFT15_MechanicTrait_WonderTwins.2:Stats: unknown]% bonus stats. |

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
| Adaptive Helm | Negatron Cloak + Tear of the Goddess | +20 MR | Gain an additional 15% Mana from all sources. The wearer gains an additional bonus based on their Role: Tanks and Fighters: Gain 35 Armor and Magic Resistance. Other Roles: Gain 15% Attack Damage and Ability Power. |
| Archangel's Staff | Needlessly Large Rod + Tear of the Goddess | +30 AP | Combat start: Gain 20% Ability Power every 5 seconds in combat. |
| Bloodthirster | B.F. Sword + Negatron Cloak | +20% AD, +20 AP, +20 MR, +20% Omnivamp | Once per combat at 40% Health, gain a 25% max Health Shield that lasts up to 5 seconds. |
| Blue Buff | Tear of the Goddess + Tear of the Goddess | +15% AD, +15 AP | Gain 10% additional Attack Damage and Ability Power from all sources. |
| Bramble Vest | Chain Vest + Chain Vest | +65 Armor | Gain 7% max health. Take 8% reduced damage from attacks. When struck by any attack, deal 100 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Crownguard | Needlessly Large Rod + Chain Vest | +20 AP, +20 Armor, +100 HP | Combat Start: Gain a 25% max Health Shield for 8 seconds. When the Shield expires, gain 25% Ability Power. |
| Deathblade | B.F. Sword + B.F. Sword | +55% AD | Perfect peace and calm for the holder - and all who face it. (live counter) |
| Dragon's Claw | Negatron Cloak + Negatron Cloak | +75 MR | Gain 9% max health. Every 2 seconds, heal 2.5% max Health. |
| Edge of Night | B.F. Sword + Chain Vest | +10% AD, +10 AP, +15% AS, +20 Armor | At 60% Health, briefly become untargetable and shed negative effects. |
| Evenshroud | Negatron Cloak + Giant's Belt | +20 MR, +150 HP | 30% Sunder enemies within 2 hexes. Gain 25 Armor and Magic Resist for the first 15 seconds of combat. Sunder: Reduce Armor |
| Gargoyle Stoneplate | Chain Vest + Negatron Cloak | +25 Armor, +25 MR, +100 HP | Gain 10 Armor and 10 Magic Resist for each enemy targeting the holder. |
| Giant Slayer | B.F. Sword + Recurve Bow | +20% AD, +20 AP, +15% AS, +15% DamageAmp | Gain 15% additional Damage Amp against Tanks. |
| Guinsoo's Rageblade | Recurve Bow + Needlessly Large Rod | +10 AP, +10% AS | Gain 7% stacking Attack Speed every second. |
| Hand Of Justice | Tear of the Goddess + Sparring Gloves | +20% Crit | Gain 2 effects:15% Attack Damage and 15% Ability Power.12% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Hextech Gunblade | B.F. Sword + Needlessly Large Rod | +20% AD, +20 AP, +18 Omnivamp, +15% Omnivamp | Heal the lowest percent Health ally for 20% of damage dealt. Ally Healing: (live counter) |
| Infinity Edge | B.F. Sword + Sparring Gloves | +35% AD, +35% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Ionic Spark | Needlessly Large Rod + Negatron Cloak | +15 AP, +35 MR, +250 HP | 30% Shred enemies within 2 hexes. When enemies cast an Ability, deal magic damage equal to 150% of the Mana spent. Shred: Reduce Magic Resist |
| Jeweled Gauntlet | Needlessly Large Rod + Sparring Gloves | +35 AP, +35% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Kraken's Fury | Negatron Cloak + Recurve Bow | +10% AD, +10% AS, +20 MR | Attacks grant 4% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 40% Attack Speed. |
| Last Whisper | Recurve Bow + Sparring Gloves | +15% AD, +20% AS, +20% Crit | Damage from attacks and Abilities 30% Sunder the target for 3 seconds. This effect does not stack. Sunder: Reduce Armor |
| Morellonomicon | Needlessly Large Rod + Giant's Belt | +20 AP, +150 HP | Attacks and Abilities deal 1% Burn and 33% Wound to enemies for 10 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Nashor's Tooth | Recurve Bow + Giant's Belt | +15 AP, +10% AS, +150 HP, +20% Crit | Attacks grant 2 bonus Mana, increased to 4 if they critically strike. |
| Protector's Vow | Tear of the Goddess + Chain Vest | +25 Armor, +25 MR | Combat Start: Gain 20 Mana. At 40% Health, gain 15 Mana and a Shield equal to 20% max Health. |
| Quicksilver | Sparring Gloves + Negatron Cloak | +15% AS, +20 MR, +20% Crit | Combat Start: Gain immunity to crowd control for 18 seconds. Gain 3% stacking Attack Speed every second. |
| Rabadon's Deathcap | Needlessly Large Rod + Needlessly Large Rod | +50 AP | This humble hat can help you make, or unmake, the world itself. (live counter) |
| Red Buff | Recurve Bow + Recurve Bow | +45% AS | Attacks and Abilities 1% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Spear of Shojin | B.F. Sword + Tear of the Goddess | +15% AD, +10 AP | Attacks grant 5 bonus Mana. |
| Spirit Visage | Tear of the Goddess + Giant's Belt | +300 HP | Regenerate 2.5% of missing Health each second. |
| Steadfast Heart | Chain Vest + Sparring Gloves | +20 Armor, +250 HP, +20% Crit | Gain 10% Durability. While above 50% Health, instead gain 18% Durability. |
| Sterak's Gage | B.F. Sword + Giant's Belt | +40% AD, +300 HP | At 60% Health, gain a Shield equal to 50% of the wearer's maximum Health that rapidly decays over 5 seconds. |
| Striker's Flail | Giant's Belt + Sparring Gloves | +10% AS, +150 HP, +20% Crit | Critical Strikes grant 5% Damage Amp for 5 seconds, stacking up to 4 times. |
| Sunfire Cape | Chain Vest + Giant's Belt | +20 Armor, +150 HP | Gain 8% max Health. Every 2 seconds, deal 1% Burn and 33% Wound to an enemy within 2 hexes for 10 seconds. Burn: Deals a percent of the target�s max Health as true damage every second Wound: Reduces healing received |
| Tactician's Cape | Spatula + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold after 10 seconds of combat. "...and a bit of Luck." |
| Tactician's Crown | Spatula + Spatula |  | Your team gains +1 max team size. 10% chance to drop 1 gold when you win combat. "...the Heart of a hero..." |
| Tactician's Shield | Frying Pan + Frying Pan |  | Your team gains +1 max team size. 10% chance to drop 1 gold when the holder dies. "Imbued with a Philosopher's wisdom..." |
| Thief's Gloves | Sparring Gloves + Sparring Gloves | +150 HP, +20% Crit | Each round: Equip 2 random items. [Consumes 3 item slots.] (live counter) |
| Titan's Resolve | Chain Vest + Recurve Bow | +10% AS, +20 Armor | Gain 2% Attack Damage and 2% Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 10% Damage Amp and gain immunity to crowd control. |
| Void Staff | Recurve Bow + Tear of the Goddess | +35 AP, +15% AS | Damage from attacks and Abilities 30% Shred the target for 5 seconds. This effect does not stack. Shred: Reduce Magic Resist |
| Warmog's Armor | Giant's Belt + Giant's Belt | +600 HP | Gain 12% max health. |

### Emblems (18)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Bastion Emblem | Frying Pan + Chain Vest | +20 Armor | The holder gains the Bastion trait. Gain 15% of Armor as Attack Damage. Gain 15% of Magic Resistance as Ability Power. |
| Battle Academia Emblem | Spatula + Tear of the Goddess |  | The holder gains the Battle Academia trait, and 120 Health and 6% Damage Amp per set14AmpIcon. |
| Crystal Gambit Emblem | Spatula + Giant's Belt | +150 HP | The holder gains the Crystal Gambit trait. |
| Duelist Emblem | Frying Pan + Recurve Bow | +10% AS | The holder gains the Duelist trait. Attacks deal 12 bonus true damage, plus 6 bonus true damage for each time the target was damaged by this effect. |
| Edgelord Emblem | Frying Pan + B.F. Sword | +10% AD | The holder gains the Edgelord trait. Dealing physical damage to an enemy reduces their Armor by 2. |
| Executioner Emblem | Frying Pan + Sparring Gloves | +20% Crit | The holder gains the Executioner trait. Attacks and Abilities that critically strike deal 20% bonus true damage over 3 seconds. |
| Heavyweight Emblem | Frying Pan + Giant's Belt | +150 HP | The holder gains the Heavyweight trait, and gains 150 max Health on takedown. |
| Juggernaut Emblem | Frying Pan + Negatron Cloak | +20 MR | The holder gains the Juggernaut trait. While above 50% Health, gain 30% Attack Speed. |
| Luchador Emblem | Spatula + Sparring Gloves | +20% Crit | The holder gains the Luchador trait. |
| Prodigy Emblem | Frying Pan + Tear of the Goddess |  | The holder gains the Prodigy trait. On cast, shield the lowest percent Health ally for 250% of mana spent. |
| Protector Emblem |  | +150 HP | The holder gains the Protector trait. When a shield on the holder breaks, heal the holder and the nearest ally for 20% of the shield's initial value. |
| Sniper Emblem |  | +150 HP | The holder gains the Sniper trait and 2 range. |
| Sorcerer Emblem | Frying Pan + Needlessly Large Rod | +10 AP | The holder gains the Sorcerer trait. The holder gains 18% more Ability Power from all sources. |
| Soul Fighter Emblem | Spatula + B.F. Sword | +10% AD | The holder gains the Soul Fighter trait. |
| Star Guardian Emblem | Spatula + Needlessly Large Rod | +10 AP | The holder gains the Star Guardian trait. Teamwork: Non-emblem Star Guardian bonuses are increased by 10%. This effect stacks. [Unique - only 1 per champion.] |
| Strategist Emblem | Spatula + Negatron Cloak | +20 MR | The holder gains the Strategist trait. Combat Start: The holder and allies within 1 hex in the same row gain bonuses. Front 2 rows: 20 Armor and Magic Resist Back 2 rows: 15% Attack Speed |
| Supreme Cells Emblem | Spatula + Recurve Bow | +10% AS | The holder gains the Supreme Cells trait. |
| Wraith Emblem | Spatula + Chain Vest | +20 Armor | The holder gains the Wraith trait. |

### Artifacts (Ornn) (38)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Artifact Anvil |  |  | Artifact Anvil |
| Blacksmith's Gloves |  | +200 HP, +30% Crit | Each round: Equip 2 random Ornn Artifacts. [Consumes 3 item slots.] (live counter) |
| Blighting Jewel |  | +50 AP | Dealing magic damage reduces the target's Magic Resist by 4. If their Magic Resist is 0, grant the holder 3 Mana instead. |
| Corrupt Vampiric Scepter |  | +60% AS | Attacks deal an additional 60% Attack Damage AD as physical damage and heal the holder for the damage dealt. The holder cannot cast their Ability or gain Mana. |
| Dawncore |  | +15% AD, +15 AP | Reduce the holder's max Mana by 10. Subsequent spellcasts reduce max Mana by 10%, to a minimum of 15. |
| Death's Defiance |  | +10% AD, +25% AS, +30 Armor, +25% Omnivamp | 50% of the damage the holder receives is instead dealt over 4 seconds as non-lethal damage. [Unique - only 1 per champion] |
| Deathfire Grasp |  | +30 AP | Combat start: Blast the current target for 40% of their max Health as magic damage. Repeat this every 13 seconds. |
| Eternal Winter |  | +20 Armor, +200 HP | Enemies who damage the holder are 20% Chilled for 1.5 seconds. After 7 Chills from this item, the attacker is Stunned instead (Cooldown: 15 seconds). [Unique - only 1 per champion] Chill: reduce Attack Speed Stun: cannot move, attack, or cast Abilities |
| Fishbones |  | +50% AD, +50% AS | The holder's attacks target random enemies. |
| Flickerblades |  | +10 AP, +15% AS | Attacks grant 6% stacking Attack Speed. Every 5 attacks also grant 3% Attack Damage and 4% Ability Power. |
| Forbidden Idol |  | +25 Armor, +25 MR, +200 HP | Shields have 50% of their value converted to max Health instead. |
| Gold Collector |  | +25% AD, +30% Crit | Attacks and Abilities execute enemies below 12% of their maximum Health. Executions have a 33% chance to drop goldCoins 1 gold. Gold Collected: (live counter)g [Unique - only 1 per champion] |
| Horizon Focus |  | +20 Armor, +20 MR, +250 HP | Stunning an enemy causes lightning to strike them, dealing 30% of their max Health as magic damage. |
| Hullcrusher |  | +35 Armor, +35 MR | Combat Start: If there are no adjacent allies, gain 600 Health, 20% Attack Damage, and 20% Ability Power. |
| Infinity Force |  | +30% AD, +30 AP, +30% AS, +30 Armor, +30 MR, +300 HP | Tons of EVERYTHING! |
| Innervating Locket |  | +150 HP | The holder gains 1% of their total Mana whenever they're hit by an attack. Each cast restores 20% of the holder's max Health over 3 seconds. |
| Lich Bane |  | +30 AP, +30% AS | The holder's first attack after each Ability cast deals (live counter) bonus magic damage. Damage increases based on Stage. |
| Lightshield Crest |  | +55 Armor, +55 MR | Every 3 seconds, Shields the lowest percent Health ally for 70% of the holder's combined Armor and Magic Resist for 5 seconds. On death grants this shield to all allies. |
| Luden's Tempest |  | +45% AD, +45 AP | 100% of overkill damage plus 100 is dealt as magic damage to the three enemies nearest the target. |
| Manazane |  | +15% AD, +10 AP, +15% AS | After casting the first time in combat, gain 100 Mana over 5 seconds. [Unique - only 1 per champion] |
| Mittens |  | +75% AS | Shrinks the holder, granting them increased movement speed and immunity to Chill. Chill: reduce Attack Speed |
| Obsidian Cleaver |  | +150 HP | Damage dealt 30% Shreds and 30% Sunders enemies for 15 seconds. Your team gains 8% Attack Damage and 8 Ability Power. ​​[Support item] [Unique - only 1 per champion] Shred: Reduce Magic Resist Sunder: Reduce Armor |
| Prowler's Claw |  | +30% AD, +200 HP, +50% Crit | After killing a target, shed negative effects and dash to the farthest target within 4 hexes. The next 2 critical attacks deal 60% bonus Critical Strike Damage. |
| Randuin's Omen |  | +150 HP | Combat start: Grant Armor 30 Armor and MR 30 Magic Resistance to the holder and adjacent allies. ​​[Support item] |
| Rapid Firecannon |  | +75% AS | Gain +1 Attack Range, increased by 1 whenever the holder kills an enemy. |
| Seeker's Armguard |  | +40 AP, +30 Armor, +30 MR | Takedowns increase the holder's Armor, Magic Resist, and Ability Power by 15, increased to 20 if they score the kill. |
| Silvermere Dawn |  | +1.4 AD, +80 Armor, +80 MR | Grants immunity to Stuns and the holder's attacks Stun the target for 0.8 seconds. The holder's Attack Speed is locked at 0.5. |
| Sniper's Focus |  | +15% AD, +15 AP, +15% AS | Gain Range 1 Attack Range. Gain 9% Damage Amp against a target for each hex between the holder and the target. |
| Spectral Cutlass |  | +40% AD, +40 Armor, +40 MR, +20% Crit | Combat start: Teleport the holder to the mirrored hex on the enemy's side of the board. After 8 seconds, the holder returns to their original location. |
| Statikk Shiv |  | +40 AP, +40% AS | Every 3rd attack deals 40 + 40% of the holder's Ability Power as additional magic damage to 4 enemies. |
| Suspicious Trench Coat |  | +15% AS, +100 HP | Once per combat at 60% Health, the holder splits into 3 copies of themself each with 33% of their max Health. [Unique - only 1 per champion] |
| Talisman Of Ascension |  | +20% AD, +20 AP, +600 HP, +1.2 DamageAmp | After 22 seconds gain 100% max Health and 120% Damage Amp for the rest of combat. |
| The Indomitable |  | +25 Armor, +25 MR, +400 HP | The holder's Move Speed is drastically reduced. Gain 10% max Health, stun immunity, and pull the current target into melee range. |
| Titanic Hydra |  | +20% AD, +20% AS, +300 HP | Attacks deal 3% of the holder's max Health plus 8% of their Attack Damage as bonus physical damage to the target and adjacent enemies. |
| Trickster's Glass |  | +10% AS, +10 Armor, +10 MR, +15% Crit | Summon a clone with 70% base Health and +10% max Mana. You cannot equip items to the clone. The clone benefits from active traits [Unique - only 1 per champion] |
| Unending Despair |  | +40 Armor, +350 HP | Whenever a Shield on the holder breaks, 150% of that Shield's initial value is dealt to the nearest enemy as magic damage. |
| Wit's End |  | +30% AS, +30 Armor, +30 MR, +300 HP | Attacks deal (live counter) bonus magic damage. Heals the holder for 35% of all magic damage dealt. Damage increases based on Stage. |
| Zhonya's Paradox |  | +40 AP, +30 Armor, +30 MR | Once per combat at 40% Health, become invulnerable and untargetable for 3 seconds. [Unique - only 1 per champion] |

### Radiant items (39)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| Radiant Adaptive Helm |  | +40 MR | Gain an additional 30% Mana from all sources. The wearer gains an additional bonus based on their Role: Tank/Fighter: Gain 90 Armor and Magic Resistance. Marksman/Caster: Gain 60% Attack Damage and Ability Power. |
| Radiant Archangel's Staff |  | +55 AP | Combat start: Gain 35% Ability Power every 4 seconds in combat. |
| Radiant Bloodthirster |  | +45% AD, +45 AP, +20 MR, +40% Omnivamp | Once per combat: At 40% Health, gain a 40% max Health Shield that lasts up to 5 seconds. |
| Radiant Blue Buff |  | +30% AD, +30 AP | Gain 20% additional Attack Damage and Ability Power from all sources. |
| Radiant Bramble Vest |  | +100 Armor | Gain 15% max health. Take 25% reduced damage from attacks. When struck by any attack, deal 175 magic damage to all adjacent enemies. Cooldown: 2 seconds |
| Radiant Crownguard |  | +40 AP, +40 Armor, +200 HP | Combat Start: Gain a 50% max Health Shield for 8 seconds. When the Shield expires, gain 50% Ability Power. |
| Radiant Deathblade |  | +1.05 AD | It glows in the presence of enemies. Or friends. Or anything alive, really. (live counter) |
| Radiant Dragon's Claw |  | +115 MR | Gain 15% max health. Every 2 seconds, heal 10% max Health. |
| Radiant Edge of Night |  | +30% AD, +30 AP, +30% AS, +20 Armor | At 60% Health, briefly become untargetable, shed negative effects, and heal all missing health. |
| Radiant Evenshroud |  | +30 MR, +500 HP | 30% Sunder enemies within 3 hexes. Gain 70 Armor and Magic Resist for the first 20 seconds of combat. Sunder: Reduce Armor |
| Radiant Gargoyle Stoneplate |  | +50 Armor, +50 MR, +250 HP | Heal 1.5% max Health each second. Gain 15 Armor and 15 Magic Resist for each enemy targeting the holder. |
| Radiant Giant Slayer |  | +50% AD, +50 AP, +20% AS, +25% DamageAmp | Gain 25% additional Damage Amp against Tanks. |
| Radiant Guinsoo's Rageblade |  | +30 AP, +25% AS | Gain 13% stacking Attack Speed every second. |
| Radiant Hand of Justice |  | +40% Crit | Gain 2 effects:35% Attack Damage and 35% Ability Power.20% Omnivamp. While above 50% health, double the Attack Damage and Ability Power. While below 50% Health, double the Omnivamp. |
| Radiant Hextech Gunblade |  | +40% AD, +40 AP, +35 Omnivamp, +30% Omnivamp | Heal the lowest percent Health ally for 40% of damage dealt. Ally Healing: (live counter) |
| Radiant Infinity Edge |  | +65% AD, +75% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Radiant Ionic Spark |  | +30 AP, +70 MR, +500 HP | 30% Shred enemies within 3 hexes. When enemies cast an Ability, deal magic damage equal to 200% of the Mana spent.. Also, heal 1.5% max Health per second. [Direct damage item] Shred: Reduce Magic Resist |
| Radiant Jeweled Gauntlet |  | +70 AP, +75% Crit | Abilities can critically strike. If the holder's abilities can already critically strike, gain 10% Critical Strike Damage instead. |
| Radiant Kraken's Fury |  | +20% AD, +20% AS, +20 MR | Attacks grant 7% stacking Attack Damage, up to 15 attacks. After 15 attacks, gain 80% Attack Speed. |
| Radiant Last Whisper |  | +45% AD, +25% AS, +55% Crit | Damage from attacks and Abilities 30% Sunder the target for the rest of combat. This effect does not stack. Sunder: Reduce Armor |
| Radiant Lucky Item Chest |  |  | Use on a champion to open an armory of Radiant items especially suited for them. These items are based on the champion's recommended items. |
| Radiant Morellonomicon |  | +50 AP, +150 HP | Attacks and Abilities deal 2% Burn and 33% Wound to enemies for 8 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Radiant Nashor's Tooth |  | +30 AP, +10% AS, +200 HP, +35% Crit | Attacks grant 4 bonus Mana, increased to 8 if they critically strike. |
| Radiant Protector's Vow |  | +50 Armor, +50 MR | Combat Start: Gain 20 Mana. At 40% Health, gain 20 Mana and a Shield equal to 45% max Health. |
| Radiant Quicksilver |  | +40% AS, +30 MR, +40% Crit | Combat Start: Gain immunity to crowd control for 45 seconds. Gain 6% stacking Attack Speed every second. |
| Radiant Rabadon's Deathcap |  | +80 AP | It's witnessed - and unleashed - miracles and calamities both. (live counter) |
| Radiant Red Buff |  | +65% AS | Attacks and Abilities 2% Burn and 33% Wound enemies for 5 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: Reduces healing received |
| Radiant Spear of Shojin |  | +30% AD, +25 AP | Attacks grant 10 bonus Mana. |
| Radiant Spirit Visage |  | +600 HP | Regenerate 5% of missing Health each second. |
| Radiant Steadfast Heart |  | +40 Armor, +500 HP, +20% Crit | Gain 16% durability. While above 40% Health, instead gain 30% Durability. |
| Radiant Sterak's Gage |  | +80% AD, +600 HP | At 60% Health, gain a Shield equal to 100% of the wearer's maximum Health that rapidly decays over 6 seconds. |
| Radiant Striker's Flail |  | +25% AS, +150 HP, +35% Crit | Critical Strikes grant 8% Damage Amp for 8 seconds, stacking up to 4 times. |
| Radiant Sunfire Cape |  | +40 Armor, +300 HP | Gain 12% max Health. Every 1.5 seconds, deal 2% Burn and 33% Wound to an enemy within 3 hexes for 8 seconds. Burn: Deals a percent of the target's max Health as true damage every second Wound: reduces healing received |
| Radiant Thief's Gloves |  | +150 HP, +20% Crit | Each round: Equip 2 random Radiant items. [Consumes 3 item slots.] (live counter) |
| Radiant Titan's Resolve |  | +30% AS, +35 Armor | Gain 3% Attack Damage and 3% Ability Power when attacking or taking damage, stacking up to 25 times. At full stacks, gain 20% Damage Amp and gain immunity to crowd control. |
| Radiant Void Staff |  | +60 AP, +60% AS | Damage from attacks and Abilities 30% Shred the target for the rest of combat. This effect does not stack. Shred: Reduce Magic Resist |
| Radiant Warmog's Armor |  | +1000 HP | Gain 20% max health. Heal 1.5% max Health per second. |
| Virtue of the Martyr |  | +150 HP | Every 5 seconds, heal your team for 7% of their max Health. When the holder dies, the healing increases to 14% max Health for 2 extra heals. Healing: (live counter) ​​[Support item] |
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

### Set-specific and other items (32)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| 1 completed item |  |  |  |
| 1 component |  |  |  |
| 1 loot orb |  |  |  |
| 2 completed items |  |  |  |
| 2 components |  |  |  |
| 2 loot orbs |  |  |  |
| Changing Glamour |  |  | Use this on the Changeling to transform them into a new unit! Refreshes each round |
| Combination Core |  |  | Mighty Mech champions heal The Mighty Mech for (live counter)% of the damage they deal. By our powers combined! |
| Completed Item Anvil |  |  | Gain a Completed Item Anvil |
| Component Item Key |  |  | Component Item Key |
| Crown of Demacia |  | +30% AS, +300 HP | If the holder is a Tank, regenerate 3-21% maximum Health every 2 seconds (based on stage). Otherwise, gain 10% AD Attack Damage and AP Ability Power every 2 seconds instead. If the holder of this item dies, you instantly lose the fight. You can remove this item by benching the holder. |
| Crystalline Reinforcements |  | +1000 HP | Your team gains 1000 health. Whenever a unit dies, a crystalline copy one star level lower joins the fight! |
| Determined Investor |  | +150 HP | After dying during combat 8 times, this item is destroyed. Upon destruction, grant the item Diamond Hands, 1 Champion Duplicator, and goldCoins 5 gold. |
| Diamond Hands |  | +25 AP, +200 HP | Once per combat: At 50% Health, become invulnerable for 1.5 seconds and grant goldCoins 1 gold. |
| Double Down |  |  | Increase Gem Power gains for the next 3 player combats. If you win, lose a large amount of your Gem Power and immediately cash out. |
| Duelist Stance |  |  | Passive: On attack, deal 40 (AD) physical damage to a nearby enemy. Active: Strike swiftly, sending 10 (AS) afterimages to the nearest 2 enemies. Each deals 65 (AD) physical damage and reduces armor by 5 (AP). |
| Executioner Stance |  |  | Deal 330 (AD+AP) physical damage to the lowest Health enemy within 3 hexes and Stun them for 1.5 seconds. Other enemies in a 1-hex radius take 130 (AD) physical damage. After 3 kills with this ability all enemies instantly take 180 (AD) physical damage and are Stunned for 1.5 seconds. |
| Gambler's Blade |  | +10 AP, +35% AS | Grant AS 1% bonus Attack Speed per goldCoins gold in your bank (up to goldCoins 30 gold). Each attack has a 5% chance to drop goldCoins 1 gold. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Hidden Technique |  |  | TFT15_HiddenTech_ShopData_Description |
| Juggernaut Stance |  |  | Briefly gain 60% Durability, then deal 500 (AD+AP) physical damage to the target, 200 (AD) physical damage to all enemies behind the target, and 150 (AD) physical damage to other enemies in a 2-hex radius. |
| Kog'Maw |  |  |  |
| Mega Blade Upgrade |  | +25% AS, +10% Omnivamp | Deal an additional 10% of all damage dealt as true damage. Bring down the blade of justice! |
| Mighty Blade |  | +25% AD | Attacks now cleave in a cone dealing 50% physical damage to adjacent enemies. We slice through evil with unbreakable resolve. |
| Mogul's Mail |  | +100 HP | Grants 1 Armor, 1 Magic Resist, and 10 Health when taking damage, stacking up to 35 times. At full stacks, grant goldCoins 1 gold and continue gaining goldCoins 1 gold every 10 seconds. Gold generated this game: (live counter) [Unique - only 1 per champion] |
| Needlessly Big Gem |  | +150 HP | Your team gains 5% Damage Amp. Each second the holder is alive, your team gains an additional 1% Damage Amp, up to a total of 20%. ​​[Support item] |
| Open the Chest |  |  |  |
| Play it Safe |  |  |  |
| Rammus |  |  |  |
| Sentinel Swarm |  |  | Every 3 seconds, summon a powerful Sentinel equipped with Completed, Artifact, and Radiant items. |
| Smolder |  |  |  |
| Zephyr |  | +150 HP | Combat start: Summon a whirlwind on the opposite side of the arena that removes the closest enemy from combat for 5 seconds. Your team gains 2% Attack Speed. [Support item] [Ignores crowd control immunity.] [Unique - only 1 per champion] (live counter) |
| Zz'Rot Portal |  | +10% AS, +150 HP | Combat start: Taunt. On death, a Voidspawn arises, Taunting nearby enemies. The Voidspawn has massively reduced effectiveness when created by a summoned unit. Taunt: enemies that are able and in range must attack the taunter |

### Consumables (42)

| Name | Recipe | Stats | Effect |
|---|---|---|---|
| A Hungry Snack |  |  | Consume 3 player health for Health HP, Attack Damage AD and Ability Power AP. |
| Champion Duplicator |  |  | Use on a champion to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
| Golden Item Remover |  |  | Use on a champion to unequip all items as many times as you want. Converts all future Magnetic Removers to Reforgers. |
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
| Masterwork Upgrade |  |  | Use on a unit to open an armory containing Radiant versions of that unit's craftable completed items. Upgrade the item you choose to its Radiant equivalent, and unequip it. |
| Pocket Recombobulator |  |  | Use on a 1 or 2 cost champion to upgrade them to a unit of 1 higher cost. Not for use on living, mechanical, experimental, imaginary, or reanimated creatures . Keep out of reach of Yordles. |
| Power Remover |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (2 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (3 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (4 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (5 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (6 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (7 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (8 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Remover (9 uses left) |  |  | Use on a champion to remove their Power Up and regain a Power Snax. |
| Power Snax |  |  | Power Up: Use on a champion to choose a Power Up! Sell that champion or use a Power Remover to regain a Power Snax. |
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
| Unlock Power (2 uses left) |  |  | Power Up: Use on a champion to choose a Power Up! Sell that champion or use a Power Remover to regain a Power Snax. |

## Augments (321)

Tier comes from the augment icon file name (I/1 = Silver, II/2 = Gold, III/3 = Prismatic); "unknown" where the name carries no marker. The list is the set's augment pool as stored in the source data and can include entries that were disabled or rotated during the set.

| Name | Tier | Effect |
|---|---|---|
| A Golden Quest | Gold | The first time you have 165 or more gold, gain a 2-star 5-cost champion and 2 items tailored to them. |
| A Magic Roll | Gold | Roll 3 dice. Gain rewards based on their total. Reward: (live counter) |
| A Story of Seven | Gold | Collect Golden Balls by defeating players in combat. Collect all 7 for a reward, then restart the quest with more powerful rewards. Gain a 2-star 1-cost champion to start your journey. |
| Adaptive Style | Gold | On attack, Duelists gain 2% Attack Damage and Ability Power, stacking up to 15 times. Duelist Tanks instead gain 2 Armor and Magic Resist. Gain a Kai'Sa and Kayle. |
| AFK | Silver | You cannot perform actions for the next 3 rounds. Afterwards, gain 20 gold. |
| Aim For The Top! | Gold | You are more likely to fight stronger players, and you know who you will fight. When you lose, gain 2 XP. After your first win and every 3 wins after, gain a component. Foes vanquished: (live counter) |
| All That Shimmers | Gold | Choose a gold-generating Artifact item and gain a Magnetic Remover. |
| All That Shimmers+ | Gold | Choose a gold-generating Artifact. Gain a Magnetic Remover and 4 gold. |
| An Exalted Adventure | Prismatic | Gain three 2-cost champions. If you 3-star two of them, gain an orb filled with loot. Gain a Lesser Champion Duplicator at the start of next 2 stages. Champions: (live counter), (live counter), (live counter) |
| Another One! | Gold | The next completed item you make grants an additional copy of it. Gain 5 gold now. |
| Another One!+ | Gold | The next completed item you make grants another copy of it. Gain 1 component. |
| Ascension | Gold | After 15 seconds of combat, gain 60% Damage Amp. |
| At What Cost | Prismatic | Immediately go to level 6 and gain 4 XP. You don't get to choose your future augments. |
| Augmented Power | Silver | Your next augment is one tier higher. |
| Aura Farming | Gold | Gain a 2-star 5-cost champion. You cannot field it until 4-3. |
| Backup Dancers | Gold | Combat start: Every 3 seconds, champions in your 3 rightmost bench slots grant your team 3% Attack Speed per slot. |
| Band of Thieves I | Silver | Gain 1 Thief's Gloves. |
| Bastion Circlet | Prismatic | Gain a Bastion Emblem, a Giant's Belt, and a Leona. |
| Battle Academia Circlet | Prismatic | Gain a Battle Academia Emblem and a Rakan. |
| Battle Academia Crest | Gold | Gain a Battle Academia Emblem. |
| Battle Academia Crown | Prismatic | Gain a Battle Academia Emblem, a Spirit Visage, and a Garen. |
| Belt Overflow | Prismatic | Gain 5 Giant's Belts. Your Giant's Belts grant +60 bonus Health. |
| Best Friends I | Silver | Units isolated in pairs gain 12% Attack Speed and 12 Armor at the start of combat. |
| Best Friends II | Gold | Units isolated in pairs gain 15% Attack Speed and 20 Armor at the start of combat. |
| Big Friend I | Silver | Your units that start combat next to an ally with over 1750 Health gain 9% Durability. |
| Big Friend II | Gold | Your units that start combat next to an ally with over 1750 Health gain 12% Durability. |
| Big Grab Bag | Gold | Gain 3 random components, 2 gold, and 1 Reforger. Reforgers allow you to remake any item. |
| Birthday Present | Prismatic | Gain a 2-star champion and 1 gold every time you level up. The champion's cost tier is your level minus 4 (max: 5-cost). |
| Blazing Soul I | Gold | Combat start: Your highest Attack Speed champion gains 20% Ability Power and 20% Attack Speed. Repeat on another ally every 3 seconds. |
| Blazing Soul II | Prismatic | Combat start: Your highest Attack Speed champion gains 35% Ability Power and 35% Attack Speed. Repeat on another ally every 3 seconds. |
| Blood Rage | Gold | Your units gain 12-25% Damage Amp based on their missing Health. Maximum Damage Amp gained at 30% Health. |
| Branching Out | Silver | Gain a random Emblem. |
| Branching Out+ | Silver | Gain a random Emblem and a Reforger. Reforgers allow you to remake any item. |
| Bronze For Life I | Gold | Your team gains 3% Damage Amp for each Bronze-tier trait. Damage Amp Bonus: (live counter)% |
| Bronze For Life II | Prismatic | Your team gains 3% Damage Amp and 1.5% Durability for each Bronze-tier trait. Damage Amp Bonus: (live counter)% Durability Bonus: (live counter)% |
| Build a Bud! | Prismatic | Gain a random 3-star 1-cost champion. Gain 8 gold. |
| Buried Treasures III | Prismatic | Gain a random item component at the start of the next 6 rounds (including this round). |
| Burning Bright | Gold | Gain an Udyr. Your strongest Udyr becomes an Attack Fighter and sends bouncing gusts of flame out with every attack. His spell no longer rapidly attacks, but instead grants him stacking Attack Speed. |
| Calculated Loss | Gold | After losing your combat, gain 2 gold and a free Shop reroll. |
| Call to Chaos | Prismatic | Gain a powerful and random reward. Reward: (live counter) |
| Called Shot | Silver | Set your win streak to 4. Gain 4 gold. |
| Care Package | Gold | Now and at the start of the next 2 stages, receive a care package with loot and 3 gold. |
| Caretaker's Ally | Silver | Gain a random 2-cost champion now. Gain the same one again every time you level up. Champion: (live counter) |
| Caretaker's Chosen | Prismatic | As you level, gain more powerful items. Level 4: Component anvil Level 6: Completed item anvil Level 8: Choose 1 of 5 Radiant items |
| Caretaker's Favor | Gold | Gain a component anvil when you reach level 5, 6, 7, and 8. The anvil offers 4 choices. |
| Chaotic Evolution | Silver | Starring up a champion grants them one of the following: 100 Health, 4% Durability, 10% Ability Power, 10% Attack Damage, 10% Attack Speed. Bonuses stack. |
| Class President | Gold | Combat start: Powered Up Battle Academia champions and allies adjacent to one gain 70 Health, 8% AD, and 8% AP. Battle Academia units gain double. Gain a Katarina |
| Clear Mind | Gold | If there are no champions on your bench at the end of player combat, gain 3 XP. |
| Climb The Ladder I | Silver | Each time an ally dies, allies that share at least one trait with them gain 3% Ability Power, 3% Attack Damage, 3 Armor, and 3 Magic Resist. |
| Climb The Ladder II | Gold | Each time an ally dies, allies that share at least one trait with them gain 5% Ability Power, 5% Attack Damage, 5 Armor, and 5 Magic Resist. |
| Cloning Facility | Gold | Empower the hex in the center of the third row. Summon a clone of the champion in it with 80% base Health and 10% increased Mana cost. |
| Cluttered Mind | Gold | Gain 4 random 1-cost champions now. If your bench is full at the end of player combat, gain 3 XP. |
| Component Buffet | Silver | Whenever you would get a component, gain a component anvil instead. Gain a random component. The anvil offers 4 choices. |
| Cooking Pot | Gold | At the start of each turn, all units holding a Frying Pan or Spatula item grant the nearest champion 40 permanent Health. Gain a Frying Pan. |
| Coronation | Prismatic | Gain a Tactician's Crown. Tactician's Crown, Shield, and Cape grant the holder an additional 20% Attack Speed, 25% Attack Damage, and 30% Ability Power. |
| Corrosion | Silver | Enemy champions in the first two rows lose 3 Armor and Magic Resist every 2 seconds. |
| Crafted Crafting | Silver | Whenever you craft a completed item, gain 2 rerolls. |
| Crown's Will | Gold | Gain a Needlessly Large Rod. Your units gain 10% Ability Power and 8 Armor. |
| Cry Me A River | Gold | Gain a Tear of the Goddess. Your team gains 1 Mana Regen. After 15 seconds in combat, increase this to 2. |
| Crystal Gambit Circlet | Prismatic | Gain a Crystal Gambit Emblem and a Janna. |
| Crystal Gambit Crest | Gold | Gain a Crystal Gambit Emblem. |
| Crystal Gambit Crown | Prismatic | Gain a Crystal Gambit Emblem, an Adaptive Helm, and a Janna. |
| Cybernetic Uplink II | Gold | Your champions holding an item gain 120 Health and restore 2 Mana per second. |
| Cybernetic Uplink III | Prismatic | Your champions holding an item gain 180 Health and restore 3 Mana per second. |
| Dazzling Display | Gold | Enemies who take damage from Sorcerer Abilities deal 20% less damage for 6 seconds. Gain a Lux and a Lucian. |
| Deadlier Blades | Prismatic | Gain a Deathblade. Deathblades gain [AD: unknown]% Attack Damage per [Takedowns: unknown] takedowns the wearer gets. |
| Deadlier Caps | Prismatic | Gain a Rabadon's Deathcap. Deathcaps gain [AP: unknown]% Ability Power per [Takedowns: unknown] takedowns the wearer gets. |
| Deepening Shadows | Gold | At combat start and whenever the Shadow Realm strikes, Wraith champions gain 6% Attack Damage and 6% Ability Power. Gain a Jhin and Zac. |
| Delayed Start | Silver | Sell your board and bench. Gain 4 random 2-star 1-cost champions. Disable your Shop for the next 3 rounds. |
| Double Trouble | Gold | When you field exactly 2 copies of a champion, they both gain 33% Attack Damage and 33% Ability Power, Armor, and Magic Resist. When you 3-star, gain a 2-star copy. |
| Duelist Circlet | Prismatic | Gain a Duelist Emblem, an Ashe, and a BF Sword. |
| Duelist Crest | Gold | Gain a Duelist Emblem. |
| Duelist Crown | Prismatic | Gain a Duelist Emblem, a Kraken's Fury, and a Gangplank. |
| Dummify | Silver | Lose all champions on your board and bench. Gain a Training Dummy with 80% of their combined health. The dummy gains 1000 Health per stage. |
| Duo Queue | Gold | Gain 2 random 5-cost champions and 2 copies of a random component. |
| Edgelord Circlet | Prismatic | Gain an Edgelord Emblem, a Samira, and a Recurve Bow. |
| Edgelord Crest | Gold | Gain an Edgelord Emblem. |
| Edgelord Crown | Prismatic | Gain an Edgelord Emblem, a Hand of Justice, and a Yasuo. |
| Efficient Shopper | Silver | For the next 4 carousels, gain a second copy of the champion taken. |
| Epoch | Gold | Now, and at the start of every stage, gain 4 XP and 2 free rerolls. |
| Epoch+ | Gold | Now, and at the start of every stage, gain 8 XP and 3 free rerolls. |
| Escape Velocity | Gold | Gain 1 Velocity per reroll, and 10 when you level up. Crew units gain 1% Attack Speed for every 3 Velocity. (Velocity: (live counter)) |
| Evil Beyond Measure | Gold | Your champions deal 4.5% of their damage as True Damage for each Star level. |
| Exclusive Customization | Gold | Gain a Lucky Item Chest and a Magnetic Remover and 5 gold. Use a Lucky Item Chest on a champion to choose from an armory of recommended items! |
| Executioner Circlet | Prismatic | Gain an Executioner Emblem, an Akali, and a Sparring Gloves. |
| Executioner Crest | Gold | Gain an Executioner Emblem. |
| Executioner Crown | Prismatic | Gain a Executioner Emblem, a Striker's Flail, and a Katarina. |
| Expected Unexpectedness | Prismatic | Now and at the start of the next 2 stages, roll 3 dice. Gain various rewards based on their total. |
| Explosive Growth | Gold | At the start of the next 4 rounds, gain 8 XP. |
| Explosive Growth+ | Gold | At the start of the next 4 rounds, gain 11 XP. |
| Eye For An Eye | Silver | For every 15 ally champions that die, gain a random component (max 3). Deaths Remaining: (live counter) |
| Eye For An Eye+ | Silver | Gain a random component. For every 16 ally champions that die, gain another component (max 2). Deaths Remaining: (live counter) |
| Final Ascension | Prismatic | Your team gains 15% Damage Amp. After 15 seconds, this increases to 45% Damage Amp. |
| Find Your Center | Silver | Your champion that starts combat in the center of the front row gains 15% Damage Amp and 15% max Health. |
| Firesale | Silver | Each round, steal a random 3-cost or lower champion from the shop. Gain 1 gold. |
| Flexible | Prismatic | Gain 1 random emblem. At the start of every Stage, gain a random emblem. Your team gains 20 Health for each emblem they are holding. |
| Forward Thinking | Gold | Lose all your gold. After 5 player combats, gain back the original amount and another 70 gold. Incoming Gold: (live counter) |
| Fully Adapted | Gold | Gain an Adaptive Helm. Champions holding this item gain both effects. |
| Game Plan | Gold | Mark 1 hex per Strategist fielded. Allies standing in marked hexes gain 150 Health and 10% Damage Amp, Strategists gain 100% more. Gain a Janna and Ziggs. |
| Giant and Mighty | Prismatic | Your team gets larger, gaining 300 Health and +4% max Health. |
| Give 'em the Chair! | Gold | Gain a Dr. Mundo. Your strongest Dr. Mundo becomes a Ranged Marksman. He now throws chairs that deal heavy damage, but cost Health. |
| Glass Cannon I | Silver | Units that start combat in the back row begin combat at 90% health but gain 13% Damage Amp. |
| Glass Cannon II | Gold | Units that start combat in the back row begin combat at 90% health but gain 20% Damage Amp. |
| Going Long | Prismatic | You no longer gain interest. Gain 15 gold now. Round start: gain 4 XP. Interest is extra gold you gain per 10g saved. |
| Gold Destiny | Gold | Gain a random Gold Augment and 4 gold. |
| Gold Destiny+ | Gold | Gain a random Gold Augment and 6 gold. |
| Gold For Dummies | Gold | Gain a Training Dummy. Every 10 seconds, all Training Dummies grant 1 gold. |
| Golden Guillotine | Gold | Executioner kills have a chance to drop loot, scaling with Critical Strike Chance. The value of the loot given can also critically strike, granting even more loot. Gain a Katarina and Kalista. |
| Golemify | Gold | Lose all champions on your board and bench. Gain a Golem with 60% of their combined Health and 50% of their combined Attack Damage. The Golem gains 1000 Health per stage. |
| Good For Something I | Silver | Champions that aren't holding items have a 50% chance to drop 1 gold on death. |
| Grand Slam | Gold | Luchadors gain 10% Attack Speed. When Luchador is active, Luchadors leap into battle at combat start. Luchador leaps deal 300% of their Basic Attack Damage. Gain a Dr. Mundo. |
| Hard Bargain | Gold | You cannot choose your Carousel rewards. Gain an additional 4 player health and 9 gold each carousel round. When you do not pick a Carousel Reward, you are given an unpicked champion and the item it carries |
| Hard Commit | Prismatic | Gain a random emblem. Now and at the start of each stage, gain a 1-star champion of that trait with a cost equal to the Stage (max 5) and 2 gold. |
| Healing Orbs I | Silver | When an enemy dies, the nearest ally is healed for 220. |
| Healing Orbs II | Gold | When an enemy dies, the nearest ally is healed for 440. |
| Heavy Is The Crown | Gold | Gain a Crown of Demacia, which provides significant bonuses to the wearer. If the wearer dies, lose the round. |
| Heavyweight Circlet | Prismatic | Gain a Heavyweight Emblem, a Poppy, and a Giant's Belt. |
| Hedge Fund | Prismatic | Gain 25 gold. Your max interest is increased to 10. Interest is extra gold you gain per 10g saved. |
| Hefty Rolls | Gold | Your team gains [Health: unknown] Health and size for each time you've rolled this game. Rolls: (live counter) Health:(live counter) |
| Hero 101 | Gold | The first time you win 4 player combats (Wins: (live counter)), all Battle Academia champions gain 100 Health and 15% reduced max Mana. Gain a Garen and Ezreal. |
| Heroic Grab Bag | Gold | Gain 2 Lesser Champion Duplicators. Gain 7 gold. This item allows you to copy a 3-cost or less champion. |
| Heroic Grab Bag+ | Gold | Gain 2 Lesser Champion Duplicators. Gain 9 gold. This item allows you to copy a 3-cost or less champion. |
| Heroic Grab Bag++ | Gold | Gain 2 Lesser Champion Duplicators. Gain 13 gold. This item allows you to copy a 3-cost or less champion. |
| High Voltage | Gold | Gain an Ionic Spark. Your Ionic Sparks have +3 hex radius and do 15% more damage. |
| Hold The Line | Prismatic | Combat Start: Champions in your back row gain 10% Ability Power and 9% Attack Damage for each champion in your front row. |
| Howling Gale | Gold | For every 550 Mana your team spends, summon a whirlwind that knocks up enemies. |
| Hustler | Gold | You earn no interest, but gain 3 gold at the start of every player combat round. Gain 3 gold now. Interest is extra gold you gain per 10g saved. |
| I'll Be The Head | Prismatic | The Mighty Mech gains 45% Attack Speed. Gain a 3 Mighty Mech Champions. Field 7 Mighty Mech with a total of 14 Star Levels and reach level 10 to activate MECHA-MERGE PROTOCOL. |
| I'll Be The Head | Gold | The Mighty Mech gains 25% Attack Speed. Gain a Gangplank. Field 7 Mighty Mech with a total of 14 Star Levels and reach level 10 to activate MECHA-MERGE PROTOCOL. |
| Invested+ | Prismatic | Gain 26 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Invested++ | Prismatic | Gain 45 gold. At the start of each round, gain 1 Shop reroll for every 10 gold above 50 (max 80 gold). |
| Investment Strategy I | Gold | Your champions gain 8 permanent max Health per interest you earn. |
| Investment Strategy II | Prismatic | Your champions gain 8 permanent max Health per interest you earn. Your max interest is increased to 7. Gain 3 gold. |
| Iron Assets | Silver | Gain a component anvil and 4 gold. |
| Isekai | Prismatic | Summon a magical hero from another universe to fight for you. This hero is a Prodigy this game. |
| Isekai | Prismatic | Summon a magical hero from another universe to fight for you. This hero is a Sorcerer this game. |
| Isekai | Prismatic | Summon a magical hero from another universe to fight for you. This hero is a Strategist this game. |
| Item Collector I | Silver | For each unique item they are holding, your team gains 1% Attack Damage and 1% Ability Power. |
| Item Collector II | Gold | Your team gains 20 Health. For each unique item they are holding, your team gains bonus 5 Health, 1% Attack Damage, and 1% Ability Power. |
| Item Grab Bag I | Silver | Gain 1 random completed item. |
| Juggernaut Circlet | Prismatic | Gain a Juggernaut Emblem, an Udyr, and a Giant's Belt. |
| Keepers I | Silver | Combat start: Grant units with adjacent allies a 130 Health Shield for 8 seconds. This Shield stacks. |
| Keepers II | Gold | Combat start: Grant units with adjacent allies a 210 Health Shield for 8 seconds. This Shield stacks. |
| Know Your Enemy | Gold | Your team gains 10% Damage Amp. Gain 18% Damage Amp instead if you and your opponent have any of the same traits active. |
| Knuckledusters | Gold | Gain a Vi. Your strongest Vi becomes an Attack Fighter and has a chance to drop loot on kill. She now deals heavy single target damage and gains Attack Damage for item components equipped on your team. |
| Last Second Save | Gold | The first time each ally falls below 35% Health, they restore 22% maximum Health. |
| Lategame Specialist | Silver | When you reach Level 9, gain 33 gold. |
| Latent Forge | Silver | After 8 player combats, gain an Artifact anvil. The anvil offers 4 choices. Artifacts are more powerful items with a unique effect. |
| Learn From The Best | Gold | Whenever you star-up a champion they permanently gain 50% of every unique Mentor bonus of Mentors on your board or bench. Gain a Yasuo. |
| Level Up! | Prismatic | When you buy XP, gain an additional 2. Gain 12 immediately. |
| Lineup | Silver | Your team gains 2 Armor and Magic Resist for each unit that starts combat in the front two rows. |
| Lit Fuse | Silver | Units that start in the back two rows split 60 total Mana after 6 seconds of combat. |
| Little Buddies | Gold | Your 4-cost and 5-cost champions gain 55 Health and 6% Attack Speed for every 1-cost and 2-cost champion on your board. |
| Living Forge | Prismatic | Gain an Artifact anvil now and after every 10 player combats. Artifacts are more powerful items with a unique effect. |
| Luchador Circlet | Prismatic | Gain a Luchador Emblem and a Dr Mundo. |
| Luchador Crest | Gold | Gain a Luchador Emblem. |
| Luchador Crown | Prismatic | Gain a Luchador Emblem, a Sterak's Gage, and a Dr Mundo. |
| Lucky Gloves | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves. |
| Lucky Gloves+ | Prismatic | Thief's Gloves will always give your champions ideal items. Gain 2 Sparring Gloves. |
| Lunch Money | Silver | Every 8 damage you deal to enemy tacticians gives you 2 gold. Total Payouts: (live counter) Gold |
| Mace's Will | Gold | Gain a Sparring Gloves. Your team gains 6% Attack Speed and 20% Critical Strike Chance. |
| Malicious Monetization | Gold | Gain 2 gold. For the next 3 rounds, enemy champions drop 2 gold when killed. |
| Mess Hall | Gold | After 10 seconds of combat, your champions gain 20% Attack Speed and attacks also hurl delicious projectiles that deal 140% Attack Damage as magic damage. |
| Missed Connections | Silver | Gain a copy of each 1-cost champion. |
| Moonlight | Gold | Combat start: 1 random 1-cost champion is upgraded to 3-star for that round and gains 45% Attack Damage and 45% Ability Power. |
| Nine Lives | Prismatic | Set your player health to 9. On combat loss, you only lose 1 health and gain loot. You can't gain health. |
| Nine Thousand Volts | Silver | Gain a Kennen. Your strongest Kennen becomes a ranged Marksman. He throws piercing Shurikens that can stun and deal magic damage. |
| Noble Sacrifice | Gold | When your first ally dies each combat, grant your team 25 + 10% of that ally's Armor and Magic Resistance. |
| One Buff, Two Buff | Prismatic | Gain a Red Buff, a Blue Buff, and a Champion Duplicator. |
| One, Two, Five! | Silver | Gain 1 random component, 2 gold, and 1 random 5-cost champion. |
| Ones Twos Three | Silver | Gain 2 1-cost champion, 2 2-cost champions, and 1 3-cost champion. |
| Over Encumbered | Silver | For the next stage, you only get 2 bench slots. After, get 2 item components. |
| Overheal | Gold | Every third attack deals 110% damage and heals 60% of the damage. Excess healing is converted to a shield up to 400 Health. |
| Pair of Fours | Gold | If your team has exactly 2 4-cost champions, they each gain 374 Health and 24.4% Attack Speed. Gain a random 4-cost. |
| Pandora's Bench | Gold | At the start of every round, champions on the 3 rightmost bench slots transform into random champions of the same cost. Gain 2 gold. |
| Pandora's Items | Silver | Round start: items on your bench are randomized. Gain 1 random component. |
| Pandora's Items II | Gold | Round start: items on your bench are randomized. Gain 2 random components. |
| Pandora's Items III | Prismatic | Round start: items on your bench are randomized. Gain 1 random Radiant item. |
| Patience is a Virtue | Silver | Each round, gain 2 free rerolls if you did not buy a champion last round. |
| Patient Study | Gold | After player combat, gain 2 XP if you won or 3 XP if you lost. |
| Perfect Form | Silver | Gain a Zac. Your strongest Zac becomes a Specialist. He now bounces around the battlefield dealing damage in a one hex radius. |
| Piercing Sight | Gold | Enemies take 1% damage as bonus true damage, increased by 1% up to 20% every time they take damage. |
| Pilfer | Gold | Each round, gain a 1-star copy of the first champion you killed last combat. |
| Placebo | Silver | Your team gains 1% Attack Speed. Gain 8 gold. |
| Placebo+ | Silver | Your team gains 1% Attack Speed. Gain 15 gold. |
| Plot Armor | Gold | Your team gains 10 Armor and Magic Resist. After falling below 50% health, increase this to 40 for the rest of combat. |
| Pocket Recombobulator | Silver | Get 2 Pocket Recombobulators, a consumable that transform a 1 or 2-cost champion into one of a higher cost. They keep their Star level. Also receive 2 Reforgers. |
| Portable Forge | Gold | Choose 1 of 4 Artifacts. Artifacts are more powerful items with a unique effect. |
| Power of Friendship | Gold | Star Guardians who start combat adjacent to at least one other Star Guardian gain 1 Mana Regeneration, 20 Armor, and 20 Magic Resist. Gain a Rell. |
| Power Upload | Gold | You can give the Mighty Mech a Power Up. When it has Power Up the Mighty Mech gains 150 Health and 10% Damage Amp. Gain an Aatrox, a Senna, and a Power Remover. |
| Precise Shot | Gold | Every 4 seconds, all Sniper champions fire a shot that deals 85% of their Basic Attack Damage to the lowest health enemy in range. Gain a Jhin. |
| Preemptive Protection | Gold | Combat start: Protectors shield themselves and their closest ally for 18% of their maximum health. Shields stack. Gain a Rakan. |
| Preparation I | Silver | Champions on your bench permanently gain 30 Health, 2% Attack Damage, and 2% Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Preparation II | Gold | Champions on your bench permanently gain 45 Health, 3% Attack Damage, and 3% Ability Power every round. Champions start with 1 stack of this effect, and can stack up to 4 times. |
| Prismatic Destiny | Prismatic | Gain a random Prismatic Augment and 5 gold. |
| Prismatic Ticket | Prismatic | Each time your Shop is rerolled, you have a 45% chance to gain a free reroll. |
| Prizefighter | Gold | Gain 2 item components. Every 5 wins gives you an item component. |
| Prodigy Circlet | Prismatic | Gain a Prodigy Emblem, a Malzahar, and a Needlessly Large Rod. |
| Prodigy Crest | Gold | Gain a Prodigy Emblem. |
| Prodigy Crown | Prismatic | Gain a Prodigy Emblem, a Spear of Shojin, and an Ezreal. |
| Protagonist | Prismatic | Gain a random 1-cost Champion Augment. After 3 turns, gain a copy of the champion at the start of every turn. |
| Protector Circlet | Prismatic | Gain a Protector Emblem, a Neeko, and a Giant's Belt. |
| Protector Crest | Gold | Gain a Protector Emblem. |
| Protector Crown | Prismatic | Gain a Protector Emblem, a Protector's Vow, and a Neeko. |
| Psychic Forge | Gold | Round start Completed Items on benched units turn into one of their recommended items. Gain a random completed item. |
| Pumping Up I | Silver | Your team gains 6% Attack Speed now. Each round after, they gain 0.5% more. (current Attack Speed: (live counter)%) |
| Pumping Up II | Gold | Your team gains 10% Attack Speed now. Each round after, they gain 1% more. (current Attack Speed: (live counter)%) |
| Pumping Up III | Prismatic | Your team gains 16% Attack Speed now. Each round after, they gain 2% more. (current Attack Speed: (live counter)%) |
| Pyromaniac | Gold | Gain a Red Buff. Your Burns deal 50% increased damage. |
| Quality Over Quantity | Prismatic | Champions holding exactly 1 item upgrade that item to Radiant. Champions holding Radiant items gain 4% Health. Gain 2 Magnetic Removers. Thief's Gloves counts as multiple items. |
| Quiet Quitting | Gold | For the next 3 player combats, you cannot use your shop. After, gain 16 gold and a Completed Item Anvil. |
| Radiant Refactor | Prismatic | Gain a Masterwork Upgrade and 1 component anvil. Masterwork Upgrade upgrades an item to Radiant! |
| Radiant Relics | Prismatic | Choose 1 of 5 Radiant items. Gain a Magnetic Remover. Radiant items are very powerful versions of completed items. |
| Raid Boss | Gold | Your strongest Bastion champion gains 20% max Health. When other Bastions die, they transfer 20% of their Armor and Magic Resist to it. Gain a Shen and Rell. |
| Raining Gold | Gold | Gain 8 gold now and 1 gold every round. |
| Raining Gold+ | Gold | Gain 18 gold now and 1 gold every round. |
| Recombobulator | Silver | Champions on your board permanently transform into random champions 1 cost tier higher (max 5). Gain 2 Magnetic Removers. |
| ReinFOURcement | Gold | The next 4-cost champion you buy is instantly upgraded to 2-star. Gain 12 gold. |
| Replication | Gold | Choose 1 of 3 components. For the next 2 rounds, gain a copy of that component. |
| Reroll Transfer | Silver | For every 1 unused Augment Reroll, gain 3 free shop rerolls. Gain 3 gold. Does not include the round this augment is selected. |
| Restart Mission | Silver | Remove all champions on your board and bench. Gain 2 random 2-star 3-costs, 3 2-star 2-costs, and 1 2-star 1-cost champion. |
| Rigged Shop+ | Silver | Your next shop and every 4 shops will contain all 3-cost champions. Gain 5 rerolls. |
| Ring The Bell | Gold | After 15 seconds, your Heavyweight champions gain 30% max Health and 20% Damage Amp. Gain a Kobuko and a Zac. |
| Risky Moves | Silver | Your Tactician loses 20 Health, but after 7 player combats, gain 30 gold. |
| Roll The Dice | Prismatic | Gain a Rascal's Gloves item. This equips 2 random Radiant items every round. Radiant items are very powerful versions of completed items. |
| Rolling For Days I | Silver | Gain 11 free Shop rerolls. |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 7 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin | Gold | Gain 1 random completed item now, and 1 component after 3 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Salvage Bin+ | Gold | Gain 1 random completed item now, and 1 component after 4 player combats. Selling champions breaks completed items into components (excluding Tactician's items and Emblems). |
| Sated Spellweaver | Gold | After casting an Ability, champions gain 15% Omnivamp for 3 seconds. Excess healing is converted to a shield up to 500 Health. |
| Scapegoat | Gold | Gain a Training Dummy and 4 gold. If it is the first to die each player combat, gain 1 gold. |
| Schoolyard Justice | Silver | Gain a Garen. Your strongest Garen becomes a Fighter. He no longer Heals, but deals more damage and executes weak enemies. |
| Scoreboard Scrapper | Gold | Every round, if you're in the bottom 4, your team permanently gains 1.5% Attack Damage and Ability Power. If you're in the Top 4, they have 10% more Health. |
| Second Wind I | Silver | After 10 seconds of combat, your team heals 40% of their missing Health. |
| Shimmerscale Essence | Prismatic | Gain a Mogul's Mail. In 5 rounds, gain a Gamblers Blade. These items give gold as well as combat power. |
| Shopping Spree | Prismatic | When you level up, gain a number of free shop refreshes equal to your level+1. Gain 2 gold. |
| Silver Destiny | Silver | Gain a random Silver Augment and 3 gold. |
| Silver Destiny+ | Silver | Gain a random Silver Augment and 4 gold. |
| Silver Destiny++ | Silver | Gain a random Silver Augment and 7 gold. |
| Silver Spoon | Silver | Gain 10 XP. |
| Slammin' | Gold | Gain 1 random component(s). After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slammin'+ | Gold | Gain 1 random component(s) and 10 XP now. After each player combat, if there are no items on your bench (other than consumables), gain 2 XP. |
| Slice of Life | Silver | Twice per stage, gain a random champion. Its cost increases per stage. This effect ends after receiving 1 5-cost champion. |
| Sniper Circlet | Prismatic | Gain a Sniper Emblem, a Jinx, and a Recurve Bow. |
| Sniper Crest | Gold | Gain a Sniper Emblem. |
| Sniper Crown | Prismatic | Gain a Sniper Emblem, an Infinity Edge, and a Caitlyn. |
| Solo Leveling | Gold | For the next 5 combats, your team size is 1 but the champion you field has massively increased stats. Gain 1 XP for every kill they get. Afterwards, gain 2 item components. |
| Sorcerer Circlet | Prismatic | Gain an Sorcerer Emblem, a Karma, and a Tear of the Goddess. |
| Sorcerer Crest | Gold | Gain an Sorcerer Emblem. |
| Sorcerer Crown | Prismatic | Gain a Sorcerer Emblem, a Jeweled Gauntlet, and an Ahri. |
| Soul Fighter Circlet | Prismatic | Gain a Soul Fighter Emblem and a Kalista. |
| Soul Fighter Crest | Gold | Gain a Soul Fighter Emblem. |
| Soul Fighter Crown | Prismatic | Gain a Soul Fighter Emblem, a Spear of Shojin, and a Kalista. |
| Space Camp | Gold | Whenever you upgrade a 1-cost Crew champion, gain a random 1-3 cost Crew champion. Gain a Malphite. |
| Spear's Will | Gold | Your team gains 10% Attack Damage and 5 Mana. Gain a B.F. Sword. |
| Spirit Link | Gold | Champions restore 5% of their max Health every 4 seconds. Increase the healing by 0.5% for every 10 missing player Health. |
| Spoils of War I | Silver | Enemies have a 25% chance to drop loot when killed. |
| Spoils of War II | Gold | Enemies have a 30% chance to drop loot when killed. |
| Spoils of War III | Prismatic | Enemies have a 45% chance to drop loot when killed. |
| Stand United I | Silver | Gain 1.5% Attack Damage and 1.5% Ability Power for each non-unique Trait active across your team. |
| Star Guardian Circlet | Prismatic | Gain a Star Guardian Emblem and a Rell. |
| Star Guardian Crest | Gold | Gain a Star Guardian Emblem. |
| Star Guardian Crown | Prismatic | Gain a Star Guardian Emblem, a Steadfast Heart, and a Rell. |
| Starfall | Gold | Gain a Neeko. Your strongest Neeko becomes a Magic Fighter and gains increased Attack Speed. Her Ability deals more damage, and increases its radius with every cast. |
| Stars are Born | Gold | The first 1-cost and 2-cost champions you buy are instantly upgraded to 2-star. Gain 7 gold. |
| Starter Kit | Prismatic | Gain a 4-cost champion, a 2-star 1-cost champion that shares a trait with them, and 8 gold. At the start of the next 2 stages, gain that 4-cost champion again. |
| Strategist Circlet | Prismatic | Gain a Strategist Emblem, a Ziggs, and a Tear of the Goddess. |
| Sunfire Board | Gold | Combat Start: Burn all enemies for 15% of their max Health over 15 seconds and reduce healing received by 33%. |
| Supreme Cells Circlet | Prismatic | Gain a Supreme Cells Emblem and an Akali. |
| Supreme Cells Crest | Gold | Gain a Supreme Cells Emblem. |
| Supreme Cells Crown | Prismatic | Gain a Supreme Cells Emblem, a Last Whisper, and a Kai'sa. |
| Survivor | Silver | After 3 players are eliminated, gain 92 gold. |
| Sword Overflow | Prismatic | Gain 5 BF Swords. Your BF Swords grant +2% Attack Speed. |
| Table Scraps | Silver | After the next 2 carousels, gain one champion that was not taken and its item. Gain 1 gold. |
| Tactician's Kitchen | Prismatic | Gain a random Emblem. After 6 combats, gain a Tactician's Cape. |
| Teacher's Council | Gold | Mentor bonuses are granted to your team regardless of how many Mentors you play. Gain a Kobuko. |
| Team Building | Silver | Gain a Lesser Champion Duplicator. Gain another after 5 player combats. This item allows you to copy a 3-cost or less champion. |
| Teaming Up I | Silver | Gain 1 random component and 2 random Tier 3 champions. |
| Tectonic Titan | Silver | Gain a Malphite. Your strongest Malphite becomes a Fighter, gaining increased Attack Speed. He now charges at nearby enemies, knocking them up and dealing heavy damage. |
| Tesla Coil | Gold | Each of your allies with 3 items equipped emits 2 arcs of lightning every 5 seconds, dealing 240-270 magic damage based on stage. |
| The Darkness Within | Gold | The first 3 Wraith champions to die each combat summon a monstrosity from the Shadow Realm. Gain a Jhin and Zac. |
| The God's Eye Opens | Gold | Every 4 seconds the God's Eye grants a random Soul Fighter 200 shield and 12% stacking Attack Speed for the rest of combat. Gain a Kalista and Naafiri. |
| The Golden Egg | Prismatic | Gain a golden egg that hatches in 11 turns for a huge amount of loot. Winning player combat speeds up the hatch timer by an extra turn. |
| The Strongest Survive | Gold | Whenever an ally dies, they grant the Supreme Cell with the most damage this round 8% of their maximum health. Gain a Kai'Sa and Kennen. |
| Thorn-Plated Armor | Gold | Gain a Bramble Vest. Your Bramble Vests deal 20-210% more damage (based on Stage). |
| Time Skip | Gold | Disable your Shop for the next 3 rounds. Then gain 28 XP. |
| Tiniest Titan+ | Prismatic | Gain 2 player health and 1 gold after every player combat. Your Tactician also moves faster. Gain 15 gold now. |
| Tiny Team | Gold | When you field 3 or less Star Guardians, increase Star Guardian bonuses by an additional 100%. Gain a Syndra and Rell. |
| Tiny, but Deadly | Prismatic | Your team is 50% smaller, but moves and attacks 30% faster. |
| Titanic Titan | Silver | Increase your current and max player health by 25. On carousel rounds you are released earlier, but are much slower. |
| Tomb Raider I | Gold | For the next 3 players eliminated, choose one of their completed items to keep. |
| Tomb Raider II | Prismatic | Gain 1 completed item anvil. Every time a player is eliminated, choose one of their completed items to keep. |
| Tons of Stats! | Gold | Your team gains 44 Health, 4% Attack Damage, 4% Ability Power, 4 Armor, 4 Magic Resist, 4% Attack Speed, and 4 Mana. |
| Tournament Favorites | Gold | Soul Fighters gain Fame when they deal damage. Each Fame breakpoint grants loot. Gain a Naafiri and Kalista. Fame: (live counter)/(live counter) |
| Tournament Titans | Gold | Luchadors gain 5% Attack Speed. Whenever you win player combats, Luchadors grow in size and permanently gain 2% Attack Damage and 20 Health. Gain a Dr Mundo and Gnar. (Total: (live counter)% AD; (live counter) Health) |
| Tower Defense | Gold | Gain a Training Dummy equipped with a random emblem that fires ranged attacks at enemies. It upgrades as the game goes on. |
| Trade Sector | Gold | Gain a free Shop reroll every round. Gain 4 gold. |
| Transfer Students | Gold | At the start of every stage and after every carousel round, gain 2 random 1-3 cost Battle Academia champions. Gain an Ezreal. |
| Treasure Hunt | Gold | Earn a locked chest each Stage between now and Stage [MaxNumChests: unknown]. You can unlock each chest when you spend [GoldNeededPerChest: unknown] Gold on shop rerolls. These chests persist until opened. |
| Trifecta I | Gold | Gain 2 3-cost champions. Combat Start: 3 random 3-cost champions gain 220 Health and 20% Attack Speed. |
| Trifecta II | Prismatic | Gain 3 3-cost champions. Allies gain 5% Attack Speed. Combat Start: 3 random 3-cost champions gain 300 Health and 25% Attack Speed. |
| Twilight Assault | Gold | Gain a Shen. Your strongest Shen becomes a ranged Marksman gaining +3 Range. His ability no longer shields but instead deals additional damage. |
| Two Much Value | Gold | Gain 1 reroll for every 2 unique two-cost champions fielded last combat. Gain 2 two-cost unit. |
| Two Trick | Gold | Gain a random 2-star 2-cost and 2 random 2-star 1-cost champions. |
| Unceasing | Gold | Whenever an ally Juggernaut dies, all other Juggernauts gain a 10% max Health shield for 5 seconds and gain 2% more Durability. Gain an Aatrox and Dr. Mundo. |
| Underdogs | Silver | Whenever your team has fewer allies alive than your opponent, regenerate 4% missing Health each second (Max: 150). |
| Unlimited Power | Prismatic | Gain a Nashor's Tooth, a Jeweled Gauntlet, a Needlessly Large Rod and a Magnetic Remover. Useful for Magic Carries! |
| Upward Mobility | Prismatic | Buying XP costs 1 less. Gain 2 Health and 2 free rerolls whenever you level up. |
| Vampiric Vitality | Gold | You heal for 12% of the damage you deal to enemy Tacticians. Your units gain 12% Omnivamp. |
| Wand Overflow | Prismatic | Gain 5 Needlessly Large Rods. Your Needlessly Large Rods grant +5% Attack Speed. |
| Wandering Trainer I | Gold | Gain a Training Dummy with 2 permanently attached Emblems. Gain 1 gold. |
| Wandering Trainer II | Prismatic | Gain a Training Dummy with 3 permanently attached Emblems. Gain 8 gold. |
| Warpath | Gold | Gain a 2-star 2-cost champion. After dealing 80 player damage, gain a chest of high cost champions and items. (Current: (live counter)) |
| Water Lotus I | Gold | Your team gains 5% Critical Strike chance, and their Abilities can critically strike. If an Ability critically strikes, restore 9% of your Mana over 3 seconds. |
| Water Lotus II | Prismatic | Your team gains 20% Critical Strike chance, and their Abilities can critically strike. If an Ability critically strikes, restore 15% of your Mana over 3 seconds. |
| Worth the Wait | Gold | Gain a random 2-star 1-cost champion. After 2 rounds, gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Worth the Wait II | Prismatic | Gain a random 2-cost champion. Gain another copy of them at the start of each round for the rest of the game. Champion: (live counter) |
| Wraith Circlet | Prismatic | Gain a Wraith Emblem and a K'Sante. |
| Wraith Crest | Gold | Gain a Wraith Emblem. |
| Wraith Crown | Prismatic | Gain a Wraith Emblem, a Hextech Gunblade, and a Malzahar. |
| Young and Wild and Free | Silver | You can always move freely on Carousel rounds. Gain 5 gold. |

## Opening encounters (19)

Source: MetaTFT lookup "encounters" list. Duplicate text rows are merged.

| Host champion | Effect | Stages |
|---|---|---|
| Yuumi | The arena is filled with crabs carrying bonus loot. | 1 |
| Gwen | It's the ultimate showdown! All augments are Prismatic this game. | 1 |
| LeeSin | This battle begins with a Prismatic augment! | 1 |
| Zyra | Get ready for a big finale with a Prismatic third augment! | 1 |
| Braum | Receive a cash prize at the start of each Stage! | 1 |
| Akali | Get highly varied loot at the start of each stage! | 1 |
| Seraphine | The arena is filled with crabs carrying bonus loot. After Stage 5 they are DEADLY HAZARDS. | 1 |
| TwistedFate | Get two component anvils. Choose your weapon! | 1 |
| Yasuo | Go for the gold with three Gold augments! | 1 |
| Yone | Pick up a signature weapon from this Artifact Anvil. | 1 |
| Lulu | Train your Golem! It has 3 random emblems. | 1 |
| Ezreal | Ekko upgrades your starting champion to 2-star. | 1 |
| Viego | Start with a random 3-cost champion this game. | 1 |
| Jinx | Augments are given earlier this game. | 1 |
| Kobuko | The second Power Snax is given at 2-6 instead of 3-6. | 1 |
| Ahri | Beach Episode! Gain a beach ball that Tacticians can toss around! | 1 |
| Sett | After Top 4, remaining players gain 10 gold and two Component Anvils. | 1 |
| JarvanIV | Carousels have more competitors on them! | 1 |
| Thresh | It's the Grand Finals! You're on your own, champion. | 1 |

## Armory offerings (139)

Source: MetaTFT lookup "armory_items" (choices offered in armories and similar pick screens).

| Offering | Effect |
|---|---|
| Completed Item Anvil | Gain a Completed Item Anvil |
| 100 gold | 100 gold |
| 36 gold | 36 gold |
| 34 gold | 34 gold |
| 30 gold | 30 gold |
| 18 gold | 18 gold |
| 64 gold | 64 gold |
| 15 gold | 15 gold |
| Artifact Anvil | Artifact Anvil |
| 76 gold | 76 gold |
| 16 gold | 16 gold |
| 12 gold | 12 gold |
| Component Item Key | Component Item Key |
| 154 gold | 154 gold |
| 126 gold | 126 gold |
| 24 gold | 24 gold |
| 55 gold | 55 gold |
| 25 gold | 25 gold |
| 54 gold | 54 gold |
| 26 gold | 26 gold |
| 20 gold | 20 gold |
| 22 gold | 22 gold |
| 44 gold | 44 gold |
| 40 gold | 40 gold |
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
| 2-cost champion | 2-cost champion |
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
| Random Component | Random Component |
| Tactician's Crown | Tactician's Crown |
| Tactician's Shield | Tactician's Shield |
| Thief's Gloves | Thief's Gloves |
| Spatula | Spatula |
| Masterwork Upgrade | Masterwork Upgrade |
| 9 gold | 9 gold |
| 8 gold | 8 gold |
| Tactician's Cape | Tactician's Cape |
| 3 gold | 3 gold |
| 2 gold | 2 gold |
| 1 gold | 1 gold |
| 7 Gold | 7 Gold |
| 6 gold | 6 gold |
| 5 gold | 5 gold |
| 4 gold | 4 gold |
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
| Lesser Champion Duplicator | Use on a 3-cost champion or less to create a 1-star copy on your bench. [Consumable - This item disappears when used.] |
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
