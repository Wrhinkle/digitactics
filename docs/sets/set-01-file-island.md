# Set 1: File Island

Adapts: TFT Set 1: Faction Wars (56 of its 57 champions) plus 4 Dota Auto Chess pieces that are not in Underlords.

The current game. The original Digi-Destined partners, Tamers, Savers, Frontier and Xros Wars lines with the Royal Knights and Demon Lords at 5 cost. TFT Set 1 was the base game with Runeterra factions and classes; this set is the base game with Digimon families and roles.

60 shop lines (by cost: 1-cost 12, 2-cost 13, 3-cost 13, 4-cost 12, 5-cost 10) and 27 wild PvE lines, all already in `js/data.js` (snapshot: data.js modified 2026-10-15T16:27:18.473Z). 54 lines have clips on all three converted models. Nothing in this set needs a download; it records which corpus slot each existing line covers.

## Cyber Sleuth-family check

Rule from the user: every board unit must use a Cyber Sleuth-family model (Cyber Sleuth CE, Digimon Links, Next Order, Re:Digitize, Time Stranger). Current state of this set:

- 41 forms use another source today (Sketchfab uploads, ReArise, Digimon Masters, New Century, Time Stranger) but have a Cyber Sleuth-family model in the catalog. They need the new rip swapped in:

  - agumon:0 Agumon: now Sketchfab upload; use links:302502, cs:324396, ts:602390 (second texture/mesh in the file)
  - agumon:1 Greymon: now Sketchfab upload; use links:304043, cs:335818
  - gabumon:0 Gabumon: now Sketchfab upload; use links:304052, cs:324615, ts:605069 (second texture/mesh in the file)
  - gabumon:1 Garurumon: now Sketchfab upload; use links:304006, cs:326174
  - gabumon:2 WereGarurumon: now Sketchfab upload; use links:304078
  - tentomon:0 Tentomon: now Sketchfab upload; use cs:324499, ts:603374
  - gomamon:1 Ikkakumon: now Sketchfab upload; use links:304106, cs:335820
  - biyomon:0 Biyomon: now Sketchfab upload; use links:304046, cs:324595, ts:603472
  - hagurumon:0 Hagurumon: now Sketchfab upload; use links:304012, cs:324621, ts:603333
  - hagurumon:1 Guardromon: now Sketchfab upload; use links:304091, cs:324628
  - hagurumon:2 Andromon: now Sketchfab upload; use links:302503
  - demidevimon:1 Devimon: now Sketchfab upload; use links:304003, cs:326172
  - guilmon:0 Guilmon: now Sketchfab upload; use links:304010, cs:324620, ts:607376
  - guilmon:1 Growlmon: now Sketchfab upload; use links:304009, cs:335819
  - renamon:1 Kyubimon: now Sketchfab upload; use links:304108, cs:335821
  - patamon:0 Patamon: now Sketchfab upload; use links:304072, cs:324600, ts:606167
  - patamon:2 MagnaAngemon: now Sketchfab upload; use links:304064
  - salamon:1 Gatomon: now Sketchfab upload; use links:304007, cs:326175
  - elecmon:0 Elecmon: now ReArise; use cs:324503, ts:604898 (second texture/mesh in the file)
  - elecmon:2 Panjyamon: now Masters; use links:304016 (second texture/mesh in the file)
  - hawkmon:0 Hawkmon: now Sketchfab upload; use links:304204, cs:324609, ts:608447
  - hawkmon:1 Aquilamon: now Sketchfab upload; use links:303996, cs:326168
  - hawkmon:2 Silphymon: now Sketchfab upload; use cs:324574
  - keramon:0 Keramon: now Sketchfab upload; use cs:324288, ts:608482
  - keramon:2 Infermon: now Sketchfab upload; use cs:324290
  - lopmon:1 Turuiemon: now ReArise; use cs:324482
  - gizamon:0 Gizamon: now TimeStranger; use ts:607154
  - candlemon:1 Wizardmon: now Sketchfab upload; use links:304124, cs:335839
  - veemon:0 Veemon: now Sketchfab upload; use links:304074, cs:324605, ts:608297
  - veemon:1 ExVeemon: now Sketchfab upload; use links:304103, cs:335814
  - veemon:2 Paildramon: now Sketchfab upload; use links:304132
  - dorumon:0 Dorumon: now Sketchfab upload; use links:304050, cs:324597, ts:605583
  - dorumon:1 Dorugamon: now Sketchfab upload; use links:304102, cs:335813
  - gaomon:0 Gaomon: now Sketchfab upload; use links:304005, cs:324616, ts:604925
  - lalamon:1 Sunflowmon: now Sketchfab upload; use links:304037, cs:335833
  - imperialdramon:1 Imperialdramon FM: now Sketchfab upload; use links:304153
  - imperialdramon:2 Imperialdramon PM: now Sketchfab upload; use links:304153 (second texture/mesh in the file), cs:324590
  - beelzemon:1 Beelzemon: now Sketchfab upload; use links:304000, cs:324633, redigitize
  - beelzemon:2 Beelzemon BM: now Sketchfab upload; use links:310678
  - machinedramon:1 Machinedramon: now Sketchfab upload; use links:304110
  - machinedramon:2 Chaosdramon: now Sketchfab upload; use links:304160

- 10 forms have no Cyber Sleuth-family model at all. Proposed replacements (taken out of the Set 2/3 pool so they do not clash):

  - demidevimon:2 NeoDevimon (now Masters) -> SkullSatamon (Cyber Sleuth)
  - gizamon:1 Gesomon (now digimonnewcentury) -> Coelamon (Cyber Sleuth)
  - gizamon:2 MarineDevimon (now digimonnewcentury) -> Dragomon (Cyber Sleuth)
  - candlemon:0 Candlemon (now Sketchfab upload) -> Impmon (shared Rookie; Cyber Sleuth)
  - candlemon:2 Mistymon (now Masters) -> Wisemon (Cyber Sleuth)
  - commandramon:0 Commandramon (now Masters) -> ToyAgumon (shared Rookie; Links / Cyber Sleuth)
  - commandramon:1 Sealsdramon (now Masters) -> Tankmon (Cyber Sleuth, already downloaded)
  - commandramon:2 Tankdramon (now Masters) -> MetalTyrannomon (Links)
  - myotismon:2 MaloMyotismon (now Sketchfab upload) -> GranDracmon (Cyber Sleuth)
  - imperialdramon:0 Imperialdramon DM (now Sketchfab upload) -> Veemon (shared Rookie; the line becomes Rookie > Mega > Mega like the Impmon line)

The roster agent was changing js/data.js while this was written; re-run the check after its work lands.

TFT Set 1 champions with no line in this set: Nidalee.

## Set mechanic

**Data Wisps** (adapts Wisps (TFT Set 18); TFT Set 1 had no set mechanic beyond the core loop)

- Already built (see README.md "Rules"): every other round from stage 2 a one-use Wisp sits in the sixth shop slot; 7 categories; unbought Wisps leave at the end of the round.

## Traits

The 30 traits already in the game, with the closest TFT Set 1 trait.

| Trait | Kind | Closest Set 1 trait | Breakpoints | Effect | Units |
|---|---|---|---|---|---|
| Dragon's Roar | family | Dragon (75% MR) -> here max HP | 2/4/6 | Dragon's Roar Digimon gain max HP. (2) +200 HP; (4) +450 HP; (6) +800 HP | 9: MetalGreymon (1), WarGrowlmon (2), ShineGreymon (3), Wingdramon (3), Cyberdramon (3), Paildramon (4), DoruGreymon (4), Imperialdramon PM (5), UlforceVeedramon (5) |
| Nature Spirits | family | Wild (attack speed) | 2/4/6 | Nature Spirits gain attack speed. (2) +30% AS; (4) +55% AS; (6) +90% AS | 7: WereGarurumon (1), Taomon (2), Rapidmon (2), Panjyamon (3), Jazarichmon (3), MachGaogamon (4), MagnaGarurumon (4) |
| Jungle Troopers | family | none in Set 1 (regeneration; closest is Set 4 Elderwood) | 2/4 | Jungle Troopers regenerate max HP every second. (2) 2.5% HP/s; (4) 5% HP/s | 5: MegaKabuterimon (1), Lillymon (1), Dinobeemon (2), Puppetmon (3), Lilamon (4) |
| Wind Guardians | family | Yordle (dodge) | 2/3 | All allies gain a chance to dodge basic attacks. (2) 15% dodge; (3) 28% dodge | 3: Garudamon (1), Crowmon (1), Silphymon (3) |
| Deep Savers | family | none in Set 1 (shield; closest is Set 4 Keeper) | 2/4 | Deep Savers start combat with a shield. (2) 35% HP shield; (4) 60% HP shield | 4: Zudomon (1), MegaSeadramon (2), Crescemon (2), MarineDevimon (3) |
| Metal Empire | family | Knight (damage block) / Robot | 2/4 | Metal Empire Digimon take less damage. (2) -18% dmg taken; (4) -32% dmg taken | 5: Andromon (1), PrinceMamemon (2), Infermon (3), Tankdramon (4), Chaosdramon (5) |
| Nightmare Soldiers | family | none in Set 1 (omnivamp; closest is Set 7 Ragewing) | 2/4 | Nightmare Soldiers heal for a share of all damage they deal. (2) 15% omnivamp; (4) 25% omnivamp | 6: NeoDevimon (1), Matadormon (2), Mistymon (3), Raguelmon (4), MaloMyotismon (5), Beelzemon BM (5) |
| Virus Busters | family | Robot (start with mana) | 2/3 | All allies start combat with bonus mana. (2) +8 mana; (3) +20 mana | 3: MagnaAngemon (2), Angewomon (2), Andiramon (3) |
| Demon Lords | family | Demon | 1/2/3 | Demon Lords gain ability power. With 2 they deal more damage, and with 3 their abilities also burn. (1) +25 AP; (2) +50 AP, +15% dmg; (3) +80 AP, +30% dmg, burn | 4: Lilithmon (4), Lucemon SM (5), Beelzemon BM (5), Daemon (5) |
| Royal Knights | family | Noble / Imperial | 1/2/3 | Royal Knights gain damage and max HP for every active trait on your team, this one included. (1) +5% dmg & HP per trait; (2) +7% dmg & HP per trait; (3) +9% dmg & HP per trait | 5: Jesmon (4), Imperialdramon PM (5), UlforceVeedramon (5), Omnimon (5), Alphamon (5) |
| DATS | family | none in Set 1 (Digimon-native) | 2/3/4 | DigiSoul Charge: the first time an ally is knocked out, or after 10 seconds, DATS Digimon heal and gain attack damage and ability power. (2) DigiSoul: +12% AD, +12 AP; (3) DigiSoul: +25% AD, +25 AP; (4) DigiSoul: +45% AD, +45 AP | 5: Crowmon (1), Chirinmon (2), ShineGreymon (3), MachGaogamon (4), Lilamon (4) |
| Legendary Warriors | family | Shapeshifter (transform for HP) | 2 | Spirit Evolution (Agunimon and Lobomon): the first time a Legendary Warrior drops below 50% HP, it heals and fights harder for the rest of combat. (2) +30% heal & AS at 50% HP | 2: EmperorGreymon (4), MagnaGarurumon (4) |
| Xros Fusion | family | none in Set 1 (Digimon-native; closest is Set 7 Guild) | 2/3/4 | DigiXros: at the start of combat, each Xros Fusion Digimon adds a share of the other Xros Fusion Digimon's max HP and attack damage to its own. (2) +12% of others' HP & AD; (3) +14% of others' HP & AD; (4) +15% of others' HP & AD | 4: Shakkoumon (2), Silphymon (3), Paildramon (4), Omnimon (5) |
| Insectoid | family | Void (Set 1) / Insect (Underlords) | 2/3/4 | Every 2 seconds a swarm bites random enemies for magic damage. A bitten enemy misses its next basic attack. (2) 1 bite for 30 every 2s; (3) 2 bites for 45 every 2s; (4) 3 bites for 60 every 2s | 4: MegaKabuterimon (1), Dinobeemon (2), CannonBeemon (2), GranKuwagamon (3) |
| Pranksters | family | Pirate (gold after combat) | 2/4 | Every second basic attack of a Prankster throws muck: the target deals less damage for 3 seconds. You also gain gold after every combat. (2) muck -45%, +1 gold/round; (4) muck -60%, +2 gold/round | 4: PlatinumNumemon (1), ShogunGekomon (1), SuperStarmon (1), PrinceMamemon (2) |
| Knight Order | family | Knight | 2/3 | Knights take less damage. A Knight standing next to another Knight takes that much less again. (2) -12% dmg, x2 beside a Knight; (3) -17% dmg, x2 beside a Knight | 3: Wingdramon (3), Durandamon (3), Hisyaryumon (4) |
| Flame | family | Elementalist / Set 2 Inferno | 2/4 | Flame Digimon's abilities burn their targets for 1.5% max HP per second and cut their healing by a third for 5 seconds. Flame Digimon deal more damage to burning enemies. (2) burn, +15% vs burning; (4) burn, +30% vs burning | 6: Garudamon (1), WarGrowlmon (2), EmperorGreymon (4), Lavogaritamon (4), Phoenixmon (5), Daemon (5) |
| Frost | family | Glacial (stun chance) | 2/3 | Frost Digimon's basic attacks have a chance to freeze the target for 1 second. (2) 30% freeze chance; (3) 50% freeze chance | 4: WereGarurumon (1), Zudomon (1), MegaSeadramon (2), Crescemon (2) |
| Undead | family | Phantom | 2/3 | The first time an Undead Digimon would be knocked out, it stays at 1 HP and can't be hurt for 1 second, then heals. (2) cheat death, heal 5%; (3) cheat death, heal 10% | 3: Matadormon (2), Mistymon (3), MaloMyotismon (5) |
| Holy Beasts | family | Guardian (armor to allies) | 2/3 | All allies gain armor, magic resist and ability power. Holy Beasts gain double. (2) +4 armor/MR/AP (x2 own); (3) +10 armor/MR/AP (x2 own) | 3: Chirinmon (2), Andiramon (3), Phoenixmon (5) |
| Fallen Angels | family | Hextech (disable) / Underlords Heartless (armor shred) | 2/3/4 | All enemies lose armor and magic resist. (2) enemies -6 armor/MR; (3) enemies -12 armor/MR; (4) enemies -20 armor/MR | 4: NeoDevimon (1), Lilithmon (4), Raguelmon (4), Lucemon SM (5) |
| Guardian | role | Guardian | 2/4/6 | Guardians gain armor and magic resist. (2) +25 armor/MR; (4) +50 armor/MR; (6) +90 armor/MR | 9: Zudomon (1), Andromon (1), SuperStarmon (1), Shakkoumon (2), Panjyamon (3), Andiramon (3), Lavogaritamon (4), Chaosdramon (5), Alphamon (5) |
| Striker | role | none (flat AD; closest is Set 7 Warrior) | 2/4/6 | Strikers gain attack damage. (2) +25% AD; (4) +45% AD; (6) +75% AD | 8: MetalGreymon (1), WereGarurumon (1), WarGrowlmon (2), Silphymon (3), MarineDevimon (3), ShineGreymon (3), Paildramon (4), Omnimon (5) |
| Sniper | role | Ranger | 2/4/6 | Snipers deal more damage for each hex between them and their target. (2) +5%/hex; (4) +9%/hex; (6) +13%/hex | 9: MegaKabuterimon (1), Garudamon (1), Rapidmon (2), Crescemon (2), CannonBeemon (2), Infermon (3), Tankdramon (4), Imperialdramon PM (5), Beelzemon BM (5) |
| Mystic | role | Sorcerer | 2/4/6 | Mystics gain ability power. (2) +20 AP; (4) +40 AP; (6) +70 AP | 10: Lillymon (1), ShogunGekomon (1), MagnaAngemon (2), MegaSeadramon (2), Mistymon (3), DoruGreymon (4), Lilithmon (4), MaloMyotismon (5), Lucemon SM (5), Phoenixmon (5) |
| Assassin | role | Assassin | 2/4 | Assassins always leap to the enemy backline when combat starts. The trait grants critical strike chance (160% damage). (2) 25% crit; (4) 50% crit | 6: NeoDevimon (1), Crowmon (1), Taomon (2), Dinobeemon (2), Matadormon (2), Cyberdramon (3) |
| Bruiser | role | Brawler | 2/4 | Bruisers gain max HP. (2) +20% max HP; (4) +40% max HP | 6: PlatinumNumemon (1), Wingdramon (3), GranKuwagamon (3), EmperorGreymon (4), Raguelmon (4), Daemon (5) |
| Duelist | role | Blademaster / Set 4 Duelist | 2/4/6 | Each basic attack gives a Duelist attack speed, stacking up to 12 times. (2) +6% AS per attack; (4) +9% AS per attack; (6) +13% AS per attack | 8: PrinceMamemon (2), Jazarichmon (3), Durandamon (3), MachGaogamon (4), MagnaGarurumon (4), Jesmon (4), Hisyaryumon (4), UlforceVeedramon (5) |
| Support | role | none in Set 1 (healer) | 2/4 | Your team's healing is stronger. Every 4 seconds, each Support heals the lowest-health ally. (2) +10% healing, 2.5% heal/4s; (4) +25% healing, 5% heal/4s | 4: Angewomon (2), Chirinmon (2), Puppetmon (3), Lilamon (4) |

## Units

| Line | Adapts | Forms | Traits | Role | HP / AD / range | Ability | Clips per form | Notes |
|---|---|---|---|---|---|---|---|---|
| agumon (1) | Darius (TFT Set 1) | Agumon > Greymon > MetalGreymon | Dragon's Roar, Striker | Striker | 620 / 60 / 1 | nuke: Giga Destroyer. Deals 260 magic damage to the target. | 4 / 4 / 4 |  |
| gabumon (1) | Warwick (TFT Set 1) | Gabumon > Garurumon > WereGarurumon | Nature Spirits, Frost, Striker | Striker | 713 / 69 / 1 | stun: Wolf Claw. Deals 165 magic damage and stuns the target for 1.5s. | 4 / 2 / 1 |  |
| tentomon (1) | Elise (TFT Set 1) | Tentomon > Kabuterimon > MegaKabuterimon | Jungle Troopers, Insectoid, Sniper | Sniper | 480 / 55 / 4 | aoe: Horn Buster. Deals 160 magic damage to the target and enemies within 1 hex. | 4 / 4 / 4 |  |
| palmon (1) | Tristana (TFT Set 1) | Palmon > Togemon > Lillymon | Jungle Troopers, Mystic | Mystic | 480 / 40 / 3 | stun: Flower Cannon. Deals 165 magic damage and stuns the target for 1.5s. | 4 / 4 / 4 |  |
| gomamon (1) | Mordekaiser (TFT Set 1) | Gomamon > Ikkakumon > Zudomon | Deep Savers, Frost, Guardian | Guardian | 720 / 45 / 1 | slam: Vulcan's Hammer. Slams the ground, dealing 120 magic damage to enemies within 1 hex and stunning them for 1s. | 4 / 4 / 4 |  |
| biyomon (1) | Vayne (TFT Set 1) | Biyomon > Birdramon > Garudamon | Wind Guardians, Flame, Sniper | Sniper | 480 / 55 / 4 | barrage: Wing Blade. Fires 4 shots at random enemies, each dealing 68 magic damage. | 4 / 4 / 4 |  |
| hagurumon (1) | Garen (TFT Set 1) | Hagurumon > Guardromon > Andromon | Metal Empire, Guardian | Guardian | 720 / 45 / 1 | shield: Gatling Attack. Gains a 300 shield and taunts nearby enemies. | 4 / 4 / 4 |  |
| demidevimon (1) | Kha'Zix (TFT Set 1) | DemiDevimon > Devimon > NeoDevimon | Nightmare Soldiers, Fallen Angels, Assassin | Assassin | 520 / 65 / 1 | dash: Neo Inferno. Dashes to the lowest-health enemy and strikes for 210 magic damage. | 4 / 4 / 0 |  |
| numemon (1) | Graves (TFT Set 1) | Numemon > BlackKingNumemon > PlatinumNumemon | Pranksters, Bruiser | Bruiser | 760 / 50 / 1 | weaken: Platinum Sludge. Deals 190 magic damage and makes the target deal 30% less damage for 4s. | 4 / 4 / 4 |  |
| otamamon (1) | Kassadin (TFT Set 1) | Otamamon > Gekomon > ShogunGekomon | Pranksters, Mystic | Mystic | 480 / 40 / 3 | stun: Musical Fist. Deals 165 magic damage and stuns the target for 1.5s. | 4 / 4 / 4 |  |
| gotsumon (1) | Fiora (TFT Set 1) | Gotsumon > Starmon > SuperStarmon | Pranksters, Guardian | Guardian | 720 / 45 / 1 | shield: Cosmic Ray. Gains a 300 shield and taunts nearby enemies. | 4 / 4 / 4 |  |
| falcomon (1) | Camille (TFT Set 1) | Falcomon > Peckmon > Crowmon | DATS, Wind Guardians, Assassin | Assassin | 520 / 72 / 1 | dash: Ether Burst. Dashes to the lowest-health enemy and strikes for 210 magic damage. | 4 / 4 / 4 |  |
| guilmon (2) | Jayce (TFT Set 1) | Guilmon > Growlmon > WarGrowlmon | Dragon's Roar, Flame, Striker | Striker | 732 / 71 / 1 | aoe: Atomic Blaster. Deals 200 magic damage to the target and enemies within 1 hex. | 4 / 4 / 4 |  |
| renamon (2) | Ahri (TFT Set 1) | Renamon > Kyubimon > Taomon | Nature Spirits, Assassin | Assassin | 614 / 84 / 1 | dash: Talisman of Light. Dashes to the lowest-health enemy and strikes for 263 magic damage. | 4 / 4 / 4 |  |
| terriermon (2) | Lucian (TFT Set 1) | Terriermon > Gargomon > Rapidmon | Nature Spirits, Sniper | Sniper | 566 / 65 / 4 | barrage: Tri-Beam. Fires 4 shots at random enemies, each dealing 85 magic damage. | 4 / 4 / 4 |  |
| patamon (2) | Blitzcrank (TFT Set 1) | Patamon > Angemon > MagnaAngemon | Virus Busters, Mystic | Mystic | 566 / 47 / 3 | nuke: Gate of Destiny. Deals 325 magic damage to the target. | 4 / 4 / 4 |  |
| salamon (2) | Shen (TFT Set 1) | Salamon > Gatomon > Angewomon | Virus Busters, Support | Support | 614 / 41 / 3 | heal: Heaven's Charm. Heals the 1 lowest-health allies for 213. | 4 / 4 / 4 |  |
| betamon (2) | Lissandra (TFT Set 1) | Betamon > Seadramon > MegaSeadramon | Deep Savers, Frost, Mystic | Mystic | 566 / 47 / 3 | stun: Thunder Javelin. Deals 206 magic damage and stuns the target for 1.5s. | 4 / 4 / 4 |  |
| wormmon (2) | Zed (TFT Set 1) | Wormmon > Stingmon > Dinobeemon | Jungle Troopers, Insectoid, Assassin | Assassin | 614 / 77 / 1 | dash: Hell Masquerade. Dashes to the lowest-health enemy and strikes for 263 magic damage. | 4 / 4 / 4 |  |
| lunamon (2) | Varus (TFT Set 1) | Lunamon > Lekismon > Crescemon | Deep Savers, Frost, Sniper | Sniper | 566 / 65 / 4 | line: Lunatic Dance. Fires through the target, dealing 238 magic damage to every enemy in a line. | 4 / 4 / 4 |  |
| armadillomon (2) | Braum (TFT Set 1) | Armadillomon > Ankylomon > Shakkoumon | Xros Fusion, Guardian | Guardian | 782 / 53 / 1 | shield: Justice Beam. Gains a 375 shield and taunts nearby enemies. | 4 / 4 / 4 |  |
| kudamon (2) | Lulu (TFT Set 1) | Kudamon > Reppamon > Chirinmon | DATS, Holy Beasts, Support | Support | 614 / 41 / 3 | rally: Holy Breath. Shields the 2 lowest-health allies for 88 and gives them 30% attack speed for 4s. | 4 / 4 / 4 |  |
| fanbeemon (2) | Rek'Sai (TFT Set 1) | FanBeemon > Waspmon > CannonBeemon | Insectoid, Sniper | Sniper | 566 / 65 / 4 | barrage: Royal Smasher. Fires 4 shots at random enemies, each dealing 85 magic damage. | 4 / 4 / 4 |  |
| mamemon (2) | Twisted Fate (TFT Set 1) | Mamemon > MetalMamemon > PrinceMamemon | Pranksters, Metal Empire, Duelist | Duelist | 802 / 65 / 1 | aoe: Smiley Warhead. Deals 200 magic damage to the target and enemies within 1 hex. | 4 / 4 / 4 |  |
| dracmon (2) | Pyke (TFT Set 1) | Dracmon > Sangloupmon > Matadormon | Undead, Nightmare Soldiers, Assassin | Assassin | 614 / 77 / 1 | execute: Fuerza Mortal. Strikes the target for 263 magic damage, doubled if it is below 50% HP. | 4 / 4 / 4 |  |
| elecmon (3) | Vi (TFT Set 1) | Elecmon > Leomon > Panjyamon | Nature Spirits, Guardian | Guardian | 979 / 61 / 1 | slam: Ice Fist. Slams the ground, dealing 180 magic damage to enemies within 1 hex and stunning them for 1s. | 4 / 4 / 0 |  |
| hawkmon (3) | Katarina (TFT Set 1) | Hawkmon > Aquilamon > Silphymon | Wind Guardians, Xros Fusion, Striker | Striker | 843 / 82 / 1 | aoe: Static Force. Deals 240 magic damage to the target and enemies within 1 hex. | 4 / 4 / 4 |  |
| keramon (3) | Gangplank (TFT Set 1) | Keramon > Kurisarimon > Infermon | Metal Empire, Sniper | Sniper | 653 / 75 / 4 | barrage: Spider Shooter. Fires 4 shots at random enemies, each dealing 102 magic damage. | 4 / 4 / 4 |  |
| lopmon (3) | Ashe (TFT Set 1) | Lopmon > Turuiemon > Andiramon | Virus Busters, Holy Beasts, Guardian | Guardian | 901 / 61 / 1 | shield: Deva Spear. Gains a 450 shield and taunts nearby enemies. | 4 / 4 / 4 |  |
| gizamon (3) | Aatrox (TFT Set 1) | Gizamon > Gesomon > MarineDevimon | Deep Savers, Striker | Striker | 843 / 82 / 1 | drain: Evil Wing. Deals 270 magic damage to the target, healing for 60% of damage dealt. | 0 / 4 / 4 |  |
| candlemon (3) | Morgana (TFT Set 1) | Candlemon > Wizardmon > Mistymon | Nightmare Soldiers, Undead, Mystic | Mystic | 653 / 54 / 3 | aoe: Phantom Fire. Deals 240 magic damage to the target and enemies within 1 hex. | 4 / 4 / 0 |  |
| agumonsv (3) | Veigar (TFT Set 1) | GeoGreymon > RizeGreymon > ShineGreymon | DATS, Dragon's Roar, Striker | Striker | 843 / 82 / 1 | nuke: Glorious Burst. Deals 390 magic damage to the target. | 4 / 4 / 4 |  |
| dracomon (3) | Shyvana (TFT Set 1) | Dracomon > Coredramon > Wingdramon | Dragon's Roar, Knight Order, Bruiser | Bruiser | 1034 / 68 / 1 | aoe: Sonic Blast. Deals 240 magic damage to the target and enemies within 1 hex. | 4 / 4 / 4 |  |
| woodmon (3) | Kennen (TFT Set 1) | Woodmon > Cherrymon > Puppetmon | Jungle Troopers, Support | Support | 707 / 48 / 3 | weaken: Puppet Pummel. Deals 285 magic damage and makes the target deal 30% less damage for 4s. | 4 / 4 / 4 |  |
| monodramon (3) | Evelynn (TFT Set 1) | Monodramon > Strikedramon > Cyberdramon | Dragon's Roar, Assassin | Assassin | 707 / 88 / 1 | execute: Desolation Claw. Strikes the target for 315 magic damage, doubled if it is below 50% HP. | 4 / 4 / 4 |  |
| jazamon (3) | Rengar (TFT Set 1) | Jazamon > Jazardmon > Jazarichmon | Nature Spirits, Duelist | Duelist | 925 / 75 / 1 | dash: Jazarich Strike. Dashes to the lowest-health enemy and strikes for 315 magic damage. | 4 / 4 / 4 |  |
| zubamon (3) | Poppy (TFT Set 1) | Zubamon > ZubaEagermon > Durandamon | Knight Order, Duelist | Duelist | 925 / 75 / 1 | line: Durandal. Fires through the target, dealing 285 magic damage to every enemy in a line. | 4 / 4 / 4 |  |
| okuwamon (3) | Volibear (TFT Set 1) | Kuwagamon > Okuwamon > GranKuwagamon | Insectoid, Bruiser | Bruiser | 1034 / 68 / 1 | execute: Dimension Scissor. Strikes the target for 315 magic damage, doubled if it is below 50% HP. | 4 / 4 / 4 |  |
| veemon (4) | Sejuani (TFT Set 1) | Veemon > ExVeemon > Paildramon | Dragon's Roar, Xros Fusion, Striker | Striker | 955 / 92 / 1 | stun: Desperado Blaster. Deals 289 magic damage and stuns the target for 1.5s. | 4 / 4 / 4 |  |
| dorumon (4) | Aurelion Sol (TFT Set 1) | Dorumon > Dorugamon > DoruGreymon | Dragon's Roar, Mystic | Mystic | 739 / 62 / 3 | nuke: Brave Metal. Deals 455 magic damage to the target. | 4 / 4 / 4 |  |
| gaomon (4) | Cho'Gath (TFT Set 1) | Gaomon > Gaogamon > MachGaogamon | DATS, Nature Spirits, Duelist | Duelist | 1047 / 85 / 1 | drain: Winning Knuckle. Deals 315 magic damage to the target, healing for 60% of damage dealt. | 4 / 4 / 4 |  |
| lalamon (4) | Kindred (TFT Set 1) | Lalamon > Sunflowmon > Lilamon | DATS, Jungle Troopers, Support | Support | 801 / 54 / 3 | heal: Lila Shower. Heals the 1 lowest-health allies for 298. | 4 / 4 / 4 |  |
| commandramon (4) | Jinx (TFT Set 1) | Commandramon > Sealsdramon > Tankdramon | Metal Empire, Sniper | Sniper | 739 / 85 / 4 | barrage: Hyper Cannon. Fires 4 shots at random enemies, each dealing 119 magic damage. | 0 / 0 / 0 |  |
| agunimon (4) | Brand (TFT Set 1) | Agunimon > BurningGreymon > EmperorGreymon | Legendary Warriors, Flame, Bruiser | Bruiser | 1170 / 77 / 1 | slam: Pyro Dragons. Slams the ground, dealing 210 magic damage to enemies within 1 hex and stunning them for 1s. | 4 / 4 / 4 |  |
| lobomon (4) | Monkey King (Dota Auto Chess (2023-04-10)) | Lobomon > KendoGarurumon > MagnaGarurumon | Legendary Warriors, Nature Spirits, Duelist | Duelist | 1152 / 93 / 1 | line: Magna Missile. Fires through the target, dealing 333 magic damage to every enemy in a line. | 4 / 4 / 4 |  |
| hackmon (4) | Akali (TFT Set 1) | Hackmon > BaoHuckmon > Jesmon | Royal Knights, Duelist | Duelist | 1047 / 85 / 1 | dash: Schwertgeist. Dashes to the lowest-health enemy and strikes for 368 magic damage. | 4 / 4 / 4 |  |
| blackgatomon (4) | Chen (Dota Auto Chess (2023-04-10)) | BlackGatomon > LadyDevimon > Lilithmon | Demon Lords, Fallen Angels, Mystic | Mystic | 739 / 62 / 3 | drain: Phantom Pain. Deals 315 magic damage to the target, healing for 60% of damage dealt. | 4 / 4 / 4 |  |
| meicoomon (4) | Gnar (TFT Set 1) | Meicoomon > Meicrackmon VM > Raguelmon | Fallen Angels, Nightmare Soldiers, Bruiser | Bruiser | 1170 / 77 / 1 | drain: Darkness Zone. Deals 315 magic damage to the target, healing for 60% of damage dealt. | 4 / 4 / 4 |  |
| vorvomon (4) | Leona (TFT Set 1) | Vorvomon > Lavorvomon > Lavogaritamon | Flame, Guardian | Guardian | 1109 / 69 / 1 | slam: Volcanic Howl. Slams the ground, dealing 210 magic damage to enemies within 1 hex and stunning them for 1s. | 4 / 3 / 3 |  |
| ryudamon (4) | Draven (TFT Set 1) | Ryudamon > Ginryumon > Hisyaryumon | Knight Order, Duelist | Duelist | 1047 / 85 / 1 | execute: Hisho Ryuuha. Strikes the target for 368 magic damage, doubled if it is below 50% HP. | 4 / 4 / 4 |  |
| myotismon (5) | Swain (TFT Set 1) | Myotismon > VenomMyotismon > MaloMyotismon | Nightmare Soldiers, Undead, Mystic | Mystic | 826 / 69 / 3 | drain: Pandemonium Lost. Deals 360 magic damage to the target and adjacent enemies, healing for 60% of damage dealt. | 4 / 4 / 3 |  |
| imperialdramon (5) | Pantheon (TFT Set 1) | Imperialdramon DM > Imperialdramon FM > Imperialdramon PM | Dragon's Roar, Royal Knights, Sniper | Sniper | 826 / 95 / 4 | aoe: Omni Sword. Deals 320 magic damage to the target and enemies within 2 hex. | 0 / 4 / 4 |  |
| lucemon (5) | Karthus (TFT Set 1) | Lucemon > Lucemon FM > Lucemon SM | Demon Lords, Fallen Angels, Mystic | Mystic | 826 / 69 / 3 | aoe: Divine Atonement. Deals 320 magic damage to the target and enemies within 2 hex. | 4 / 4 / 4 |  |
| beelzemon (5) | Miss Fortune (TFT Set 1) | Impmon > Beelzemon > Beelzemon BM | Demon Lords, Nightmare Soldiers, Sniper | Sniper | 826 / 95 / 4 | barrage: Corona Destroyer. Fires 6 shots at random enemies, each dealing 136 magic damage. | 4 / 3 / 3 | stage 2 reuses the model of an earlier stage (Beelzemon BM) |
| machinedramon (5) | Elder Titan (Dota Auto Chess (2023-04-10)) | Megadramon > Machinedramon > Chaosdramon | Metal Empire, Guardian | Guardian | 1424 / 77 / 1 | slam: Hyper Mugen Cannon. Slams the ground, dealing 240 magic damage to enemies within 2 hex and stunning them for 1s. | 4 / 4 / 4 |  |
| veedramon (5) | Yasuo (TFT Set 1) | Veedramon > AeroVeedramon > UlforceVeedramon | Royal Knights, Dragon's Roar, Duelist | Duelist | 1345 / 114 / 1 | line: Shining V Force. Fires through the target, dealing 380 magic damage to every enemy in a line. | 4 / 4 / 4 |  |
| omnimon (5) | Kai'Sa (TFT Set 1) | WarGreymon > Omnimon > Omnimon | Royal Knights, Xros Fusion, Striker | Striker | 1066 / 103 / 1 | execute: Supreme Cannon. Strikes the target for 420 magic damage, doubled if it is below 50% HP. | 4 / 3 / 3 | stage 2 reuses the model of an earlier stage (Omnimon) stages 1 and 2 are the same Digimon (Omnimon) |
| alphamon (5) | Kayle (TFT Set 1) | Alphamon > Alphamon > Alphamon | Royal Knights, Guardian | Guardian | 1238 / 77 / 1 | slam: Ouryuken. Slams the ground, dealing 240 magic damage to enemies within 2 hex and stunning them for 1s. | 3 / 3 / 3 | stage 1 reuses the model of an earlier stage (Alphamon) stage 2 reuses the model of an earlier stage (Alphamon) stages 1 and 2 are the same Digimon (Alphamon) stages 0 and 1 are the same Digimon (Alphamon) |
| phoenixmon (5) | Anivia (TFT Set 1) | Phoenixmon > Phoenixmon > Phoenixmon | Holy Beasts, Flame, Mystic | Mystic | 826 / 69 / 3 | aoe: Starlight Explosion. Deals 320 magic damage to the target and enemies within 2 hex. | 4 / 4 / 4 | stage 1 reuses the model of an earlier stage (Phoenixmon) stage 2 reuses the model of an earlier stage (Phoenixmon) stages 1 and 2 are the same Digimon (Phoenixmon) stages 0 and 1 are the same Digimon (Phoenixmon) |
| daemon (5) | Jakiro (Dota Auto Chess (2023-04-10)) | Daemon > Daemon > Daemon | Demon Lords, Flame, Bruiser | Bruiser | 1307 / 86 / 1 | aoe: Chaos Flare. Deals 320 magic damage to the target and enemies within 2 hex. | 4 / 4 / 4 | stage 1 reuses the model of an earlier stage (Daemon) stage 2 reuses the model of an earlier stage (Daemon) stages 1 and 2 are the same Digimon (Daemon) stages 0 and 1 are the same Digimon (Daemon) |

## Wild PvE lines

goblimon (Goblimon), kuwagamon (Kuwagamon), ogremon (Ogremon), tyrannomon (Tyrannomon), etemon (Etemon), skullgreymon (SkullGreymon), sukamon (Sukamon), nanimon (Nanimon), monzaemon (Monzaemon), frigimon (Frigimon), bakemon (Bakemon), kimeramon (Kimeramon), piedmon (Piedmon), metalseadramon (MetalSeadramon), crusadermon (Crusadermon), leopardmon (Leopardmon), craniamon (Craniamon), dynasmon (Dynasmon), magnamon (Magnamon), diaboromon (Diaboromon), armageddemon (Armageddemon), seraphimon (Seraphimon), ophanimon (Ophanimon), magnadramon (Magnadramon), leviamon (Leviamon), barbamon (Barbamon), belphemon (Belphemon RM)

## Items and augments

- Crests (8) (Spatula items (Set 1)): Already in the game: Digi-Egg + component gives a trait Crest.
- Golden Digi-Egg (Force of Nature (Spatula + Spatula)): Already in the game: +1 team size.
- Worth adding: Yuumi Chip (Yuumi (Spatula + Tear, Set 1)): Holder gains the Mystic role trait.
- Worth adding: Runaan's Feather (Runaan's Hurricane (Set 1)): Attacks hit one extra enemy for 50% damage.
- Worth adding: Frozen Mallet (Frozen Mallet (Set 1, Glacial)): Holder gains Frost.
- Augments: TFT Set 1 had none; the game's 27 augments stay as they are.

## Reuse list

Lines where one model or Digimon fills two stages (fix candidates are in README.md):

- beelzemon: stage 2 reuses the model of an earlier stage (Beelzemon BM)
- omnimon: stage 2 reuses the model of an earlier stage (Omnimon); stages 1 and 2 are the same Digimon (Omnimon)
- alphamon: stage 1 reuses the model of an earlier stage (Alphamon); stage 2 reuses the model of an earlier stage (Alphamon); stages 1 and 2 are the same Digimon (Alphamon); stages 0 and 1 are the same Digimon (Alphamon)
- phoenixmon: stage 1 reuses the model of an earlier stage (Phoenixmon); stage 2 reuses the model of an earlier stage (Phoenixmon); stages 1 and 2 are the same Digimon (Phoenixmon); stages 0 and 1 are the same Digimon (Phoenixmon)
- daemon: stage 1 reuses the model of an earlier stage (Daemon); stage 2 reuses the model of an earlier stage (Daemon); stages 1 and 2 are the same Digimon (Daemon); stages 0 and 1 are the same Digimon (Daemon)
