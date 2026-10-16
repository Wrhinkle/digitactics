# Set 3: Chosen Children (partial)

Adapts: TFT Set 4: Fates. Only 19 of its 58 champions plus Ornn from Set 4.5 are covered: the Cyber Sleuth-family models ran out after Set 2.

Planned as the Digi-Destined partner set (TFT Set 4 Chosen). After Sets 1 and 2 only 1 Champion-level, 3 Ultimate-level and 38 Mega-level Cyber Sleuth-family models are left, so this file covers 4- and 5-cost slots with Mega-to-Mega lines and stops. The 1- to 3-cost slots wait for more Time Stranger rips.

20 shop lines (by cost: 3-cost 1, 4-cost 11, 5-cost 8). 18 have animated models for all three forms. Estimated new downloads: 33 files, about 64.4 MB.

## Set mechanic

**Partner Digimon** (adapts Chosen (TFT Set 4))

- From level 4 the shop can offer one Partner: a Digimon that is already at its Champion form (stage 1, the same as a 2-star) and costs 3x its base cost.
- One of its origin traits is marked and counts as 2. The Partner gets +200 max HP and one bonus picked at random when it is offered: +30 AP, +30% AD, or 25% lower mana cost.
- Only one Partner at a time. While you own one, no more are offered; selling it turns offers back on.
- Partner cost-tier odds by level (from TFT 10.19): L4 40/60/0/0/0, L5 20/50/30/0/0, L6 10/45/45/0/0, L7 0/30/40/30/0, L8 0/20/40/40/0, L9 0/0/30/40/30. A Partner is shown at least once by round 2-2.
- Three copies still digivolve: a Partner plus three Champion-form copies of the same line make its final form.

Engine work: Shop flag for a stage-1 offer, a per-unit marked trait that counts double, and the bonus roll. The Data Wisps from the current game are switched off in this set.

## Traits

| Trait | Kind | Adapts | Breakpoints | Effect | Units |
|---|---|---|---|---|---|
| Dark Ocean | origin | Cultist | 3/5/7 | The first time your team has lost 50% of its total HP in a fight, Dragomon's Shadow rises under the largest group of enemies, deals its slam damage and knocks them up for 1.5s, then fights for you. It gains 10% HP and damage per form stage of your Dark Ocean units (Rookie 0, Champion 1, final 2). (3) Summon Dragomon's Shadow (1500 HP, slam 200 magic damage)<br>(5) Shadow 3000 HP, slam 400<br>(7) Shadow 5000 HP, slam 800 | 3: BlackWarGreymon (4), MegaGargomon (4), MarineAngemon (5) |
| Celestial | origin | Divine | 2/4/6 | After 6 basic attacks or when it first drops below 50% HP, a Celestial unit cleanses all crowd control and ascends: it takes 45% less damage and deals 45% bonus true damage for the duration. (2) Ascend for 3s<br>(4) Ascend for 6s<br>(6) Ascend for 9s | 2: MirageGaogamon Burst Mode (4), ShineGreymon Ruin Mode (5) |
| Witchelny | origin | Dusk | 2/4 | Witches and wizards of the Witchelny realm power up every ability on the team. (2) All allies +20 AP<br>(4) All allies +20 AP; Witchelny units +50 more | 3: Boltboutamon (4), BlackWarGreymon X (4), Ophanimon Falldown Mode (5) |
| Ancient Forest | origin | Elderwood | 3/5/7 | Every 2 seconds Ancient Forest units grow and gain the listed stats, stacking up to 5 times. (3) +20 armor/MR and +5 AD/AP per stack<br>(5) +30 armor/MR and +10 AD/AP per stack<br>(7) +40 armor/MR and +20 AD/AP per stack | 2: TyrantKabuterimon (4), Ebemon (5) |
| Pilgrims | origin | Enlightened | 2/4/6 | Wandering monks and sages gain more mana from every source. (2) +40% mana gained<br>(4) +70% mana gained<br>(6) +100% mana gained | 2: Apocalymon (4), Ravemon Burst Mode (4) |
| Ronin | origin | Exile | 1/2 | A Ronin with no adjacent ally when combat starts gains the bonus. (1) Shield equal to 50% max HP<br>(2) Shield, and 80% lifesteal | 1: Omnimon Zwart D (5) |
| Zodiac Armor | origin | Fortune | 3/5 | Each PvP loss with Zodiac Armor active adds a lucky token (max 8). Your next PvP win pays a loot orb worth 2 gold plus 1 gold per token; at 4+ tokens it also holds a component, at 7+ a completed item. With 5 Zodiac Armor every win also drops a rare orb (Digi-Egg or 4 gold). (3) Lucky Orb payout<br>(5) Lucky Orb payout plus a rare orb | 1: Titamon (4) |
| Lunar | origin | Moonlight | 3 | Under the full moon, Lunar Digimon grow. Ties go to the unit with the most items. (3) The lowest-stage Lunar unit digivolves one stage for the combat (two units with 4 Lunar from a Crest) | 0:  |
| Toho Braves | origin | Ninja | 1/4 exact | Japanese folklore warriors. Only active with exactly 1 or exactly 4 different Toho Braves. (1) The Toho Brave gains +40% AD and +40 AP<br>(4) All Toho Braves gain +100% AD and +100 AP | 1: Omnimon Alter-S (4) |
| Onmyo Spirits | origin | Spirit | 2/4 | The first time an Onmyo Spirit casts in a fight, all allies gain that much attack speed (a 70-mana spirit gives +24.5% / +49%). (2) Allies gain attack speed equal to 35% of the caster's max mana<br>(4) Equal to 70% of the caster's max mana | 2: MetalGarurumon X (3), Mastemon (4) |
| Deva Generals | origin | Warlord | 3/5/7 | Generals of the Holy Beasts. The bonus grows by 10% for each PvP fight they win this game (up to 5 wins). (3) +200 HP and +20 AP<br>(5) +450 HP and +40 AP<br>(7) +700 HP and +70 AP | 1: Chaosmon Valdur Arm (5) |
| Yellow Dragon | unique | Emperor | 1 | Chaosmon's line places two statues anywhere on your side. Statues do not move or attack, have 30% of its max HP and taunt adjacent enemies. They break when it is knocked out. (1) Deploys with two Deva Statues | 1: Chaosmon Valdur Arm (5) |
| Bancho | unique | The Boss | 1 | The first time the Bancho drops below 40% HP it leaves the fight to do push-ups: each one heals 15% max HP and gives +40% attack speed. At full HP it jumps back in. If its team loses first, it is defeated. (1) Push-up break | 1: Gankoomon (5) |
| Fallen Choice | unique | Tormented | 1 | After it has fought 3 combats, this line asks you to pick its final form: Cherubimon (Good) heals and shields allies with its ability; Cherubimon (Evil) drains HP and its ability hits twice. (1) Pick a final form after 3 fights | 1: Cherubimon (Good) (5) |
| Digi-Forge | unique | Blacksmith (Set 4.5) | 1 | After each PvP fight it survives, the Digi-Forge line adds 1 forge point; at 3 points it forges a random artifact item (the Set 4.5 Ornn item list, renamed) onto the unit with the most components. One artifact per game. (1) Forges an artifact | 1: Ebemon (5) |
| Sage | class (Duelist stats) | Adept | 2/3 | Sages calm the start of every fight. (2) Enemies -50% attack speed for 2s at the start of combat<br>(3) For 3.5s | 2: Omnimon Alter-S (4), Omnimon Zwart D (5) |
| Assassin | class (Assassin stats) | Assassin | 2/4 | Assassins leap to the enemy backline when combat starts. (2) +10% crit chance, +30% crit damage; abilities can crit<br>(4) +25% crit chance, +60% crit damage | 1: Ravemon Burst Mode (4) |
| Brawler | class (Bruiser stats) | Brawler | 2/4/6 | Brawlers gain max HP. (2) +250 max HP<br>(4) +500 max HP<br>(6) +800 max HP | 2: MirageGaogamon Burst Mode (4), Gankoomon (5) |
| Charmer | class (Mystic stats) | Dazzler | 2/4 | Charmers' abilities weaken attackers. (2) Enemies hit by an ability deal 40% less AD for 8s<br>(4) 80% less AD | 1: Apocalymon (4) |
| Duelist | class (Duelist stats) | Duelist | 2/4/6 | Each basic attack gives a Duelist attack speed. (2) +8% attack speed per attack (8 stacks)<br>(4) +12% per attack<br>(6) +18% per attack | 1: ShineGreymon Ruin Mode (5) |
| Hunter | class (Sniper stats) | Hunter | 2/3/4 | On the timer, every Hunter attacks the enemy with the lowest % HP for bonus damage. (2) Every 3.5s, 125% bonus damage<br>(3) Every 3s, 150%<br>(4) Every 2.5s, 175% | 3: MetalGarurumon X (3), TyrantKabuterimon (4), MirageGaogamon Burst Mode (4) |
| Keeper | class (Guardian stats) | Keeper | 2/4 | At the start of combat Keepers shield themselves and adjacent allies. Keepers get 50% more. (2) 150 shield for 8s<br>(4) 225 shield for 10s | 2: BlackWarGreymon X (4), Chaosmon Valdur Arm (5) |
| Sorcerer | class (Mystic stats) | Mage | 3/5/7 | Sorcerers cast their ability twice in a row with modified power. (3) Cast twice at 70% power<br>(5) Twice at 100%<br>(7) Twice at 130% | 2: Mastemon (4), Ophanimon Falldown Mode (5) |
| Warder | class (Support stats) | Mystic | 2/4 | Warders ward the team against abilities. (2) All allies +40 MR<br>(4) All allies +100 MR | 3: Boltboutamon (4), Omnimon Alter-S (4), MarineAngemon (5) |
| Shade | class (Assassin stats) | Shade | 2/3 | Shades teleport to the enemy backline when combat starts. Every third attack they stealth and their next attack deals the bonus. (2) +125 bonus magic damage<br>(3) +400 bonus magic damage | 1: Cherubimon (Good) (5) |
| Sharpshooter | class (Sniper stats) | Sharpshooter | 2/4 | Sharpshooter shots ricochet to nearby enemies. (2) Attacks and abilities bounce once for 45% damage<br>(4) Bounce twice for 50% damage | 1: MegaGargomon (4) |
| Vanguard | class (Guardian stats) | Vanguard | 2/4/6 | Vanguards gain armor. (2) +60 armor<br>(4) +150 armor<br>(6) +300 armor | 3: BlackWarGreymon (4), Titamon (4), Ebemon (5) |

## Units

Stats are the Rookie (stage 0) values from `DT.ROLES` and the tier formula in `DT.unitStats`; HP and AD multipliers come from the corpus champion compared with the average of its cost in that set (clamped 0.85-1.15). Model sources are The Models Resource asset ids. "anim" = all three forms have animated sources.

### 3-cost

| Adapts | Forms (Rookie > Champion > final) | Traits | Role | HP / AD / range | Ability | Models | anim | Canon |
|---|---|---|---|---|---|---|---|---|
| Kindred (Set 4) | Gabumon > CresGarurumon (Mega) > MetalGarurumon X (Mega) | Onmyo Spirits, Hunter | Sniper | 653 / 86 / 3 | execute: Metal Storm: the wolf mauls the target and cuts its healing by 50% while the lamb leaps away. | in game; links 308180; links 304069 | yes | main / none |

### 4-cost

| Adapts | Forms (Rookie > Champion > final) | Traits | Role | HP / AD / range | Ability | Models | anim | Canon |
|---|---|---|---|---|---|---|---|---|
| Aatrox (Set 4) | BlackAgumon > BlackGreymon > BlackWarGreymon (Mega) | Dark Ocean, Vanguard | Guardian | 1109 / 59 / 1 | slam: Dark Terra Force: pulls the 2 farthest enemies in, then slams the ground in front. | links 302502; links 304043; links 304040 (have) | yes | main / main |
| Ahri (Set 4) | Renamon > BlueMeramon (Ultimate) > Mastemon (Mega) | Onmyo Spirits, Sorcerer | Mystic | 628 / 52 / 4 | aoe: Holy Desire: charges a spirit orb for 1.5s and lobs it at a random enemy for large area damage. | in game; links 304023; links 304022 | yes | two-step / none |
| Ashe (Set 4) | Wormmon > HerculesKabuterimon (Mega) > TyrantKabuterimon (Mega) | Ancient Forest, Hunter | Sniper | 628 / 97 / 4 | barrage: Shine of Bee: for 5s gains attack speed and every attack fires a flurry of stingers. | in game; links 304058 (have); cs 324583 | yes | two-step / listed |
| Cassiopeia (Set 4) | DemiDevimon > Boltmon (Mega) > Boltboutamon (Mega) | Witchelny, Warder | Support | 761 / 46 / 2 | stun: Pernicious Waltz: a cone that stuns for 2.5s; stunned enemies take 20% more damage. | in game; links 304159 (have); dwnext 342147 | unknown | two-step / none |
| Jhin (Set 4) | Terriermon > GranLocomon (Mega) > MegaGargomon | Dark Ocean, Sharpshooter | Sniper | 628 / 97 / 4 | passive (new type): Burst Shot: attacks at a fixed rate; every fourth shot deals 444% AD. | in game; links 304203; cs 324562 | yes | none / none |
| Morgana (Set 4) | Patamon > Jijimon (Mega) > Apocalymon (Mega) | Pilgrims, Charmer | Mystic | 702 / 52 / 3 | drain: Darkness-Zone: curses the ground under a random enemy for 5s, shredding 40% MR; heals for part of the damage. | in game; links 304224; links 310677 | yes | two-step / none |
| Riven (Set 4) | Guilmon > ChaosGallantmon (Mega) > BlackWarGreymon X (Mega) | Witchelny, Keeper | Guardian | 1109 / 69 / 1 | line: Dark Terra Force: dashes and shields, then slashes forward; every third cast sends a wave across the board. | in game; links 304054; links 304076 | yes | two-step / none |
| Sejuani (Set 4) | Armadillomon > RustTyranomon (Mega) > Titamon (Mega) | Zodiac Armor, Vanguard | Guardian | 1275 / 59 / 1 | stun: Soul Core Attack: throws a giant firecracker at the closest enemy; after 2s it explodes and stuns. | in game; links 304116; links 304171 (have) | yes | two-step / none |
| Shen (Set 4) | Kotemon > Omnimon Alter-B (Mega) > Omnimon Alter-S (Mega) | Toho Braves, Sage, Warder | Duelist | 1204 / 72 / 1 | shield: Supreme Sword: dashes past the target, shields itself and taunts adjacent enemies for 4s. | ts 605322; links 304226; links 308181 | no | none / listed |
| Talon (Set 4) | Falcomon > Alphamon Ouryuken (Mega) > Ravemon Burst Mode (Mega) | Pilgrims, Assassin | Assassin | 801 / 115 / 1 | execute: Crimson Formation: stabs for 250% AD; a kill makes it untargetable for 1s. | in game; cs 324489; links 304220 | yes | none / none |
| Warwick (Set 4) | Gaomon > MirageGaogamon (Mega) > MirageGaogamon Burst Mode (Mega) | Celestial, Hunter, Brawler | Sniper | 850 / 97 / 1 | drain: Full Moon Meteor Impact: gains attack speed and 40% lifesteal for the rest of combat; takedowns speed up allies who share a trait. | in game; links 304044; links 304044 | yes | main / main |

### 5-cost

| Adapts | Forms (Rookie > Champion > final) | Traits | Role | HP / AD / range | Ability | Models | anim | Canon |
|---|---|---|---|---|---|---|---|---|
| Azir (Set 4) | Dracomon > Chaosmon (Mega) > Chaosmon Valdur Arm (Mega) | Deva Generals, Keeper, Yellow Dragon | Mystic | 826 / 72 / 4 | line: Bancho Blade: a wall of soldiers charges forward from behind it, damaging and slowing every enemy it passes. | in game; cs 324508; cs 324508 | yes | none / none |
| Kayn (Set 4) | Lopmon > Pandamon (Ultimate) > Cherubimon (Good) (Mega) / Cherubimon (Evil) | Fallen Choice, Shade | Assassin | 850 / 129 / 1 | dash: Lightning Spear: dashes to the target and slashes adjacent enemies, raising their next mana cost by 33%; recasts if it hits one enemy. Final form is chosen (Fallen Choice). | in game; links 304027; cs 324576; cs 324575 | yes | two-step / none |
| Lee Sin (Set 4) | Agumon > ShineGreymon Burst Mode (Mega) > ShineGreymon Ruin Mode (Mega) | Celestial, Duelist | Duelist | 1228 / 85 / 1 | stun: Corona Blaze Sword: punches the target to the edge of the board, stunning it and everyone it hits. | in game; links 304032 (have); links 304032 (have) | yes | two-step / none |
| Lillia (Set 4) | Salamon > Rosemon Burst Mode (Mega) > Ophanimon Falldown Mode (Mega) | Witchelny, Sorcerer | Mystic | 826 / 58 / 2 | stun: Flame Hellscythe: puts the 2 healthiest awake enemies to sleep for 3s; damage wakes them with a burst. | in game; links 304031; links 305588 | yes | none / none |
| Sett (Set 4) | Shoutmon > OmniShoutmon (Ultimate) > Gankoomon (Mega) | Bancho, Brawler | Bruiser | 1503 / 77 / 1 | slam: Tekken Seisai: grabs the target and slams it forward for 40% of its max HP, splashing 20% nearby. | links 304033 (have); cs 324541; links 304201 | yes | main / none |
| Zilean (Set 4) | Gomamon > Plesiomon (Mega) > MarineAngemon (Mega) | Dark Ocean, Warder | Support | 760 / 51 / 3 | rally: Ocean Love: places a revive bubble on the 2 lowest-HP allies; one that would die revives after 3.5s with 350 HP. | in game; links 304192 (have); links 304021 | yes | main / none |
| Yone (Set 4) | Gabumon > Omnimon Zwart (Mega) > Omnimon Zwart D (Mega) | Ronin, Sage | Duelist | 1170 / 109 / 1 | line: Supreme Cannon: strikes along a path, splitting damage between all enemies hit and cutting armor and MR by 60%. | in game; links 304026 (have); links 304227 | yes | two-step / main |
| Ornn (Set 4.5) | Hagurumon > MetalEtemon (Mega) > Ebemon (Mega) | Ancient Forest, Vanguard, Digi-Forge | Guardian | 1176 / 89 / 1 | line: Crusher Bone: an elemental charges from behind the farthest enemy back to it, slowing everything it passes; hits knock up. | in game; links 304083; links 304190 | yes | none / none |

Canon column: whether Wikimon lists the evolution Rookie > Champion and Champion > final. "main" = a bolded main evolution, "listed" = any listed evolution, "two-step" = linked through one Digimon in between, "none" = no link (a game-design pairing).

### Unit notes

- Kindred (Gabumon > CresGarurumon > MetalGarurumon X): middle form CresGarurumon is Mega Level-shifted at 3-cost: no Champion-level Cyber Sleuth-family model is left.
- Aatrox (BlackAgumon > BlackGreymon > BlackWarGreymon): BlackAgumon > BlackGreymon > BlackWarGreymon was the BlackAgumon chain in the roster plan. Champion and final are black second textures of in-game models.
- Ahri (Renamon > BlueMeramon > Mastemon): level-shifted: middle form BlueMeramon is Ultimate Level-shifted. BlueMeramon is a second texture in the Links Meramon file. No canon link.
- Ashe (Wormmon > HerculesKabuterimon > TyrantKabuterimon): level-shifted: middle form HerculesKabuterimon is Mega Mega to Mega line. HerculesKabuterimon (Links) is already downloaded.
- Cassiopeia (DemiDevimon > Boltmon > Boltboutamon): level-shifted: middle form Boltmon is Mega Mega to Mega line (no canon link; both are patchwork-monster Digimon). Boltboutamon is a Next Order model.
- Jhin (Terriermon > GranLocomon > MegaGargomon): level-shifted: middle form GranLocomon is Mega Mega to Mega line with no canon or theme link (filler).
- Morgana (Patamon > Jijimon > Apocalymon): level-shifted: middle form Jijimon is Mega Mega to Mega line with no canon or theme link (filler). Apocalymon was a boss candidate in the roster plan.
- Riven (Guilmon > ChaosGallantmon > BlackWarGreymon X): level-shifted: middle form ChaosGallantmon is Mega Mega to Mega line. Both forms are second textures (ChaosGallantmon in the Gallantmon file, BlackWarGreymon X in the WarGreymon X file).
- Sejuani (Armadillomon > RustTyranomon > Titamon): level-shifted: middle form RustTyranomon is Mega Mega to Mega line with no canon link (filler). Titamon is already downloaded.
- Shen (Kotemon > Omnimon Alter-B > Omnimon Alter-S): level-shifted: middle form Omnimon Alter-B is Mega Mega to Mega pair (Omnimon Alter-B and Alter-S).
- Talon (Falcomon > Alphamon Ouryuken > Ravemon Burst Mode): level-shifted: middle form Alphamon Ouryuken is Mega Mega to Mega line. Alphamon Ouryuken is a second mesh in the Cyber Sleuth Alphamon file.
- Warwick (Gaomon > MirageGaogamon > MirageGaogamon Burst Mode): level-shifted: middle form MirageGaogamon is Mega Mega to Mega mode change.
- Azir (Dracomon > Chaosmon > Chaosmon Valdur Arm): level-shifted: middle form Chaosmon is Mega Mega to Mega mode change.
- Kayn (Lopmon > Pandamon > Cherubimon (Good)): level-shifted: middle form Pandamon is Ultimate Uses two finals: Cherubimon (Good) and Cherubimon (Evil).
- Lee Sin (Agumon > ShineGreymon Burst Mode > ShineGreymon Ruin Mode): level-shifted: middle form ShineGreymon Burst Mode is Mega Mega to Mega mode change. Both are extra meshes or textures in the Links ShineGreymon file (ShineGreymon itself is in the game).
- Lillia (Salamon > Rosemon Burst Mode > Ophanimon Falldown Mode): level-shifted: middle form Rosemon Burst Mode is Mega Mega to Mega line with no canon link. Rosemon Burst Mode is a second mesh in the Links Rosemon file.
- Sett (Shoutmon > OmniShoutmon > Gankoomon): level-shifted: middle form OmniShoutmon is Ultimate Level-shifted.
- Zilean (Gomamon > Plesiomon > MarineAngemon): level-shifted: middle form Plesiomon is Mega Mega to Mega line. Plesiomon is already downloaded.
- Yone (Gabumon > Omnimon Zwart > Omnimon Zwart D): level-shifted: middle form Omnimon Zwart is Mega Mega to Mega pair. Omnimon Zwart is a second texture in the Omnimon file.
- Ornn (Hagurumon > MetalEtemon > Ebemon): level-shifted: middle form MetalEtemon is Mega Set 4.5 slot. Mega to Mega line with no canon link (filler).

## Set items

| Item | Adapts | Effect |
|---|---|---|
| Digivice of Fate | Chosen (mechanic) | Holder becomes your Partner if you have none: one of its origin traits counts double and it gains +200 HP. Consumed. |
| Deva Banner | Warlord's Banner (Set 4) | Holder gains the Deva Generals trait. |
| Moon Stone | Moonlight / Aspect of Dusk | Holder gains the Witchelny trait. |
| Forest Sprout | Elderwood Sprout | Holder gains the Ancient Forest trait. |
| Monk's Bead | Mage's Hat / Enlightened | Holder gains the Saiyu Pilgrims trait. |
| Vanguard Plate | Vanguard's Cuirass | Holder gains the Vanguard trait. |
| Dueling Saber | Duelist's Zeal | Holder gains the Duelist trait. |
| Halo of Light | Sword of the Divine | Holder gains the Celestial trait. |
| Jogress Pair: Holy / Fallen | Kayn's Liberation / Rhaast's Embrace | Given to the Fallen Choice line after 3 fights; equipping one picks Cherubimon (Good) or Cherubimon (Evil). |
| Artifacts (Digi-Forge) | Ornn artifacts (Set 4.5) | the forge: Vulcan's Hammer (+40% AD; abilities hit twice in a cone), Mjolnir Gauntlet (attacks chain lightning to 3 enemies), Gold Collector (kills give 1 gold), Zhonya's Mirror (once at 30% HP: 2s invulnerable), Obsidian Fang (each attack -3 armor and MR on target, stacking 10). |

## Augments worth adapting

| Augment | Adapts | Effect |
|---|---|---|
| Crest of Courage | Set 4 had no augments; adapted from the Chosen bonus | Your next Partner offer is guaranteed and costs 2x instead of 3x. |
| Lucky Streak | Fortune trait | Gain 2 lucky tokens now; Zodiac Armor payouts +1 gold per token. |
| Dark Ocean Tide | Cultist (Galio) | Dragomon's Shadow rises at 30% team HP lost instead of 50%. |
| Deva Oath | Warlord | Deva Generals start the game with 2 win stacks. |
| Full Moon | Moonlight | Lunar triggers on 2 units with 3 Lunar. |

## Reuse list

Champion or final forms that are not fully unique to this set:

- BlackGreymon (stage 1, Aatrox): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Greymon
- BlackWarGreymon (stage 2, Aatrox): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of War Greymon
- BlueMeramon (stage 1, Ahri): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Meramon
- ChaosGallantmon (stage 1, Riven): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Dukemon
- BlackWarGreymon X (stage 2, Riven): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of War Greymon (X-Antibody)
- Alphamon Ouryuken (stage 1, Talon): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Alphamon
- MirageGaogamon Burst Mode (stage 2, Warwick): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Mirage Gaogamon
- Chaosmon Valdur Arm (stage 2, Azir): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Chaosmon
- ShineGreymon Burst Mode (stage 1, Lee Sin): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Shine Greymon
- ShineGreymon Ruin Mode (stage 2, Lee Sin): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Shine Greymon
- Rosemon Burst Mode (stage 1, Lillia): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Rosemon
- Omnimon Zwart (stage 1, Yone): only exists (in Cyber Sleuth-family games) as a second texture or mesh inside the file of Omegamon

## Downloads for this set

33 catalog files were not downloaded when this was written, about 64.4 MB (estimates per game). All are Cyber Sleuth-family assets and are in the approved download batch (status: downloading). The list is in this set's JSON and in download-plan.json.

## Open checks

- trait Ancient Forest: 2 units, first breakpoint 3
- trait Zodiac Armor: 1 units, first breakpoint 3
- trait Lunar: 0 units, first breakpoint 3
- trait Deva Generals: 1 units, first breakpoint 3
- trait Assassin: 1 units, first breakpoint 2
- trait Charmer: 1 units, first breakpoint 2
- trait Duelist: 1 units, first breakpoint 2
- trait Sorcerer: 2 units, first breakpoint 3
- trait Shade: 1 units, first breakpoint 2
- trait Sharpshooter: 1 units, first breakpoint 2
