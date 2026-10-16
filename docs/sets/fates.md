# Fates

Set id `fates`. Adapts TFT Set 4 Fates (patch 11.1 data) plus Set 4.5 Festival of Beasts (Ornn's Blacksmith artifacts and Samira's Daredevil). Corpus: `docs/inspo/tft/set-04-fates.md`, `docs/inspo/tft/set-04-5-festival-of-beasts.md`.

Files: `js/sets/fates.js` (data), `js/sets/fates-mech.js` (hooks, ability types, Partner mechanic), `js/sets/fates-art.js` (item, Crest and augment portraits, move looks, basic attacks), `js/sets/fates-models.js` (model keys).

## Overview

- 60 shop lines: 13 / 13 / 13 / 11 / 10 by cost. The 58 Set 4 champions plus Ornn and Samira from Set 4.5. Each line is a Digimon evolution line chosen to fit its champion (Diana is Lunamon, Yasuo is Ryudamon, Sett is Leomon to BanchoLeomon, Zilean is Clockmon to Millenniummon). No Digimon appears in two lines.
- 18 wild Digimon and summons (Festival of Beasts waves, the Dark Ocean Soulmon, Imperialdramon's stone guards, the Void Portal Tankmon).
- 28 traits: 11 origins, 5 unique traits (one 5-cost each) and 12 classes, all with exact breakpoints.
- Classic items: 8 components, 36 recipes, a Digi-Egg (the Spatula) that makes 8 Crests, 12 more Crests from drops, the Golden Digi-Egg, 10 artifacts, 6 consumables.
- 30 augments (10 Silver, 11 Gold, 9 Prismatic). Set 4 had none; the engine's augment rounds stay, and several augments support the Partner mechanic.
- Every form uses an existing model key (`node tests/models.cjs fates`: 0 gaps). Forms with no rip were swapped for Digimon that have one, keeping the role (for example Kalista's Divermon line became Syakomon / Coelamon / MarineAngemon, Morgana became Piddomon / Ophanimon / Lucemon FM).

## Mechanic: Partners (TFT Chosen)

- From level 4, each shop roll has a 33% chance (75% with the Destiny Seeker augment) to turn one slot into a Partner. Everyone sees one by round 2-2.
- A Partner is already in its Champion form (3 copies) and costs 3 times its base cost. Buying it takes the other two copies from the pool. If those copies are gone, or the bench is full, it arrives as a Rookie-form Partner and the difference is refunded.
- One of its non-unique traits is marked and counts twice (`bonusTraits`). It gains 200 HP plus one bonus: 300 HP, 40 ability power, 20% attack damage, or 20% lower max mana.
- You can own one Partner at a time; no offers appear while you have one. Selling it brings them back. It keeps its mark when it digivolves with plain copies (`onCombine`).
- Partner cost odds by level come from TFT patch 10.19: L4 40/60, L5 20/50/30, L6 10/45/45, L7 0/30/40/30, L8 0/20/40/40, L9 0/0/30/40/30.
- Augments: Partner Bond (Silver), Twin Destiny (Gold, marked trait counts once more), Soul Partners (Prismatic), Festival of Beasts (Prismatic, a 4-cost Partner at once).
- The shop card reads `player.shopMeta[slot]`: a Partner offer shows a Partner badge, the Champion form with its stars, and the marked trait as x2. Its hover card and the unit tooltip add a Partner line with the marked trait and the bonus (`unitNote`).

Festival of Beasts extras: artifacts are forged by the Digi-Forge line (Volcamon, adapting Ornn), drop rarely from stage 4 wild rounds (12% of the gold item orb) and come from two augments. Elusive/Spirit ideas were left out.

## Economy and rounds

Both paces are supported (`rules.hyper`, `rules.normal`).

- Hyper Roll (native pace, the default): engine Hyper Roll rules. 20 HP, level by stage, fixed income 3/5/6/7/8/8/9, 1-gold rerolls, Armory at x-1 of stages 2-6, augments at 2-2/3-2/4-2. Wild rounds 1-2, 1-3, x-4.
- Normal: the engine's standard TFT rules with Set 4's XP table (36/56/80 XP for levels 7/8/9, max level 9), Set 4 pool sizes (29/22/18/12/10) and Set 4 shop odds. Wild rounds 1-2..1-4 and x-7; from stage 4 the x-7 wave is the next stage's wave, since boards are stronger that late.
- Wild rounds (Festival of Beasts): 1: Goblimon and Kuwagamon. 2: Ogremon and Kuwagamon. 3: Lantern night (Etemon, Ogremon, Witchmon). 4: Deva Generals (Andiramon, Asuramon, Mammothmon). 5: Dark Ocean (MetalSeadramon, Whamon). 6: the Four Holy Beasts. 7+: Goldramon with the Holy Beasts.
- Loot: the base game's drops, plus rare artifacts.

## Traits

Ninja (Shinobi) only works with exactly 1 or 4 different Shinobi; the set's `onCountTraits` sets the count to 0 otherwise. Treasure Hunters (Fortune) counts lucky tokens per player in `player.mech.luck`; Deva Generals counts PvP wins in `player.mech.deva`.

| Trait | Kind | Adapts | Breakpoints | Effect per breakpoint | Lines |
|---|---|---|---|---|---|
| Dark Ocean | origin | Cultist | 3/6/9 | (3) Soulmon (Rookie form)<br>(6) Soulmon (Champion form)<br>(9) Soulmon (final form) | Keramon (1), Betamon (1), Gizamon (2), BlackGatomon (3), Syakomon (3), BlackAgumon (4), Dorumon (4), Clockmon (5) |
| Celestial | origin | Divine | 2/4/6 | (2) ascend 3s, -35% damage taken, +35% damage<br>(4) ascend 6s, -42% damage taken, +42% damage<br>(6) ascend 10s, -50% damage taken, +50% damage | Patamon (1), Gaomon (2), Hackmon (3), Salamon (3), BlackGabumon (4), Veemon (5) |
| Nightfall | origin | Dusk | 2/4 | (2) all allies +12 AP<br>(4) all allies +15 AP, Nightfall +35 more | DemiDevimon (1), Candlemon (2), Tapirmon (4), Guilmon (4), Myotismon (5) |
| Ancient Forest | origin | Elderwood | 3/6/9 | (3) +5 armor/MR, +2 AD/AP per stack<br>(6) +10 armor/MR, +3 AD/AP per stack<br>(9) +18 armor/MR, +7 AD/AP per stack | Mushroomon (1), Palmon (2), Lalamon (2), Chuumon (3), Aruraumon (3), Biyomon (4), Floramon (5), Volcamon (5) |
| Pilgrims | origin | Enlightened | 2/4/6 | (2) +30% mana gained<br>(4) +55% mana gained<br>(6) +85% mana gained | Zubamon (1), Otamamon (1), Hawkmon (2), Hackmon (3), Piddomon (4), Jazamon (4) |
| Ronin | origin | Exile | 1/2 | (1) 50% HP shield alone<br>(2) 50% HP shield alone, 50% omnivamp | Ryudamon (1), Ouryumon (5) |
| Treasure Hunters | origin | Fortune | 3/6 | (3) 30% dodge, tokens pay out on wins<br>(6) 40% dodge, tokens pay out on wins, +3 gold per win | Numemon (1), Coronamon (2), Commandramon (3), Gumdramon (3), Gomamon (4) |
| Lunar | origin | Moonlight | 3/5 | (3) 1 Lunar Digimon digivolves for the combat<br>(5) 2 Lunar Digimon digivolve for the combat | Lunamon (1), Gotsumon (1), Gabumon (2), Dracmon (2) |
| Shinobi | origin | Ninja | 1/4 | (1) the one Shinobi gains +35% AD and +35 AP<br>(4) all 4 gain +60% AD and +60 AP | Falcomon (2), Kotemon (3), Tentomon (3), Wormmon (4) |
| Onmyo Spirits | origin | Spirit | 2/4 | (2) allies +20% of the caster's max mana as attack speed<br>(4) allies +42% of the caster's max mana as attack speed | FanBeemon (2), Labramon (3), Meicoomon (3), Renamon (4) |
| Deva Generals | origin | Warlord | 3/6/9 | (3) +220 HP and +20 AP, +10% per PvP win (max 5)<br>(6) +450 HP and +40 AP, +10% per PvP win (max 5)<br>(9) +700 HP and +70 AP, +10% per PvP win (max 5) | Hagurumon (1), Terriermon (1), Kudamon (2), Agumon (2), Gumdramon (3), Dracomon (3), Paildramon (5) |
| Sovereign | unique | Emperor | 1 | (1) two guards with 40% of its max HP | Paildramon (5) |
| Bancho | unique | The Boss | 1 | (1) push-up break below 40% HP | Leomon (5) |
| Fallen Choice | unique | Tormented | 1 | (1) choose a path after 3 combats | Lopmon (5) |
| Digi-Forge | unique | Blacksmith (4.5) | 1 | (1) an artifact every 4 forge points | Volcamon (5) |
| Showtime | unique | Daredevil (4.5) | 1 | (1) double shots and shields, casts on style | Impmon (5) |
| Sage | class | Adept | 2/3 | (2) enemies -50% attack speed for 1.2s<br>(3) enemies -50% attack speed for 2.5s | Hackmon (3), Wormmon (4), Ouryumon (5) |
| Assassin | class | Assassin | 2/4 | (2) +15% crit, +30% crit damage<br>(4) +30% crit, +60% crit damage | Lunamon (1), Gizamon (2), Kotemon (3), Gumdramon (3), Jazamon (4) |
| Brawler | class | Brawler | 2/4/6 | (2) +300 max HP<br>(4) +650 max HP<br>(6) +1000 max HP | Mushroomon (1), Numemon (1), Dracmon (2), Agumon (2), Chuumon (3), BlackGabumon (4), Leomon (5) |
| Charmer | class | Dazzler | 2/4 | (2) -55% AD for 8s<br>(4) -90% AD for 8s | Gotsumon (1), Salamon (3), Piddomon (4), Floramon (5) |
| Duelist | class | Duelist | 2/4/6 | (2) +12% attack speed per attack<br>(4) +18% attack speed per attack<br>(6) +26% attack speed per attack | Zubamon (1), Ryudamon (1), Gaomon (2), Syakomon (3), Dracomon (3), Veemon (5) |
| Hunter | class | Hunter | 2/3/4 | (2) every 3.5s, a 140% AD strike<br>(3) every 3s, a 170% AD strike<br>(4) every 2.5s, a 200% AD strike | Gabumon (2), Labramon (3), Biyomon (4), BlackGabumon (4) |
| Keeper | class | Keeper | 2/4 | (2) 140 shield for 8s<br>(4) 240 shield for 10s | Keramon (1), Kudamon (2), Tentomon (3), Guilmon (4), Paildramon (5) |
| Sorcerer | class | Mage | 3/5/7 | (3) cast twice at 65% AP<br>(5) cast twice at 85% AP<br>(7) cast twice at 110% AP | Otamamon (1), Betamon (1), Coronamon (2), Lalamon (2), Aruraumon (3), Renamon (4), Myotismon (5) |
| Warder | class | Mystic | 2/4 | (2) all allies +14 MR<br>(4) all allies +32 MR | Hawkmon (2), Meicoomon (3), Tapirmon (4), Wormmon (4), Clockmon (5) |
| Shade | class | Shade | 2/3 | (2) +90 magic damage every 3rd attack<br>(3) +220 magic damage every 3rd attack | Falcomon (2), BlackGatomon (3), Lopmon (5) |
| Sharpshooter | class | Sharpshooter | 2/4/6 | (2) 1 bounce for 50% damage<br>(4) 2 bounces for 55% damage<br>(6) 3 bounces for 60% damage | Terriermon (1), DemiDevimon (1), FanBeemon (2), Commandramon (3), Dorumon (4), Impmon (5) |
| Vanguard | class | Vanguard | 2/4/6 | (2) +60 armor<br>(4) +140 armor<br>(6) +260 armor | Hagurumon (1), Patamon (1), Palmon (2), Candlemon (2), BlackAgumon (4), Gomamon (4), Volcamon (5) |

## Lines

| Cost | Adapts | Forms | Traits | Attr | Role | Abilities (type) |
|---|---|---|---|---|---|---|
| 1 | Diana | Lunamon > Lekismon > Crescemon | Lunar, Assassin | Data | Assassin | Luna Claw / Moon Night Kick / Lunatic Dance (dash) |
| 1 | Elise | Keramon > Chrysalimon > Infermon | Dark Ocean, Keeper | Virus | Bruiser | Crazy Giggle / Data Crusher / Hell's Grenade (drain) |
| 1 | Fiora | Zubamon > ZubaEagermon > Durandamon | Pilgrims, Duelist | Vaccine | Duelist | Twenty Dive / Road to Decade / Zwei Glanze (stun) |
| 1 | Garen | Hagurumon > Guardromon > Andromon | Deva Generals, Vanguard | Vaccine | Guardian | Cog Crusher / Destruction Grenade / Spiral Sword (slam) |
| 1 | Lissandra | Gotsumon > Icemon > IceDevimon | Lunar, Charmer | Data | Mystic | Rock Fist / Iceball Bomb / Frost Claw (aoe) |
| 1 | Maokai | Mushroomon > Woodmon > Cherrymon | Ancient Forest, Brawler | Virus | Bruiser | Laughing Smasher / Branch Drain / Cherry Bomb (slam) |
| 1 | Nami | Otamamon > Gekomon > ShogunGekomon | Pilgrims, Sorcerer | Virus | Mystic | Lullaby Bubble / Symphony Crusher / Musical Fist (stun) |
| 1 | Nidalee | Terriermon > Gargomon > Rapidmon | Deva Generals, Sharpshooter | Vaccine | Sniper | Terrier Tornado / Gargo Laser / Rapid Fire (nuke) |
| 1 | Tahm Kench | Numemon > Sukamon > PlatinumNumemon | Treasure Hunters, Brawler | Virus | Bruiser | Poop Toss / Poop Blast / Platinum Burp (shield) |
| 1 | Twisted Fate | Betamon > Seadramon > MegaSeadramon | Dark Ocean, Sorcerer | Virus | Mystic | Electric Shock / Ice Winder / Lightning Javelin (line) |
| 1 | Vayne | DemiDevimon > Devimon > SkullSatamon | Nightfall, Sharpshooter | Virus | Sniper | Demi Darts / Death Claw / Nail Bone (execute) |
| 1 | Wukong | Patamon > Angemon > MagnaAngemon | Celestial, Vanguard | Data | Guardian | Air Shot / Heaven's Knuckle / Heaven's Gate (stun) |
| 1 | Yasuo | Ryudamon > Ginryumon > Hisyaryumon | Ronin, Duelist | Vaccine | Duelist | Katana Attack / Metal Armor Blade / Legendary Dragon Blade (line) |
| 2 | Annie | Coronamon > Firamon > Apollomon | Treasure Hunters, Sorcerer | Vaccine | Mystic | Corona Flame / Fira Claw / Phoebus Blow (aoe) |
| 2 | Aphelios | Gabumon > Garurumon > WereGarurumon | Lunar, Hunter | Data | Sniper | Blue Blaster / Fox Fire / Wolf Claw (barrage) |
| 2 | Hecarim | Palmon > Togemon > Lillymon | Ancient Forest, Vanguard | Data | Guardian | Poison Ivy / Needle Spray / Flower Cannon (drain) |
| 2 | Janna | Hawkmon > Aquilamon > Silphymon | Pilgrims, Warder | Free | Support | Feather Slash / Blast Laser / Dual Sonic (rally) |
| 2 | Jarvan IV | Kudamon > Reppamon > Chirinmon | Deva Generals, Keeper | Vaccine | Guardian | Bullet Whirlwind / Rolling Tackle / Wind Cutter Sword (stun) |
| 2 | Jax | Gaomon > Gaogamon > MachGaogamon | Celestial, Duelist | Data | Duelist | Rolling Upper / Dash Double Claw / Winning Knuckle (slam) |
| 2 | Lulu | Lalamon > Sunflowmon > Lilamon | Ancient Forest, Sorcerer | Data | Support | Seed Blast / Sunshine Beam / Lila Shower (grow) |
| 2 | Pyke | Gizamon > Gesomon > Dragomon | Dark Ocean, Assassin | Virus | Assassin | Spiral Edge / Devil Bashing / Forbidden Trident (dash) |
| 2 | Sylas | Dracmon > Sangloupmon > Matadormon | Lunar, Brawler | Virus | Bruiser | Eye of Nightmares / Drain Slash / Thousand Arrows (line) |
| 2 | Teemo | FanBeemon > Waspmon > CannonBeemon | Onmyo Spirits, Sharpshooter | Virus | Sniper | Gear Stinger / Turbo Stinger / Nitro Stinger (weaken) |
| 2 | Thresh | Candlemon > Bakemon > Phantomon | Nightfall, Vanguard | Data | Guardian | Bonfire / Hell's Hand / Soul Chopper (rally) |
| 2 | Vi | Agumon > Greymon > MetalGreymon | Deva Generals, Brawler | Vaccine | Bruiser | Pepper Breath / Mega Flame / Trident Arm (aoe) |
| 2 | Zed | Falcomon > Peckmon > Crowmon | Shinobi, Shade | Vaccine | Assassin | Scratch Smash / Kunai Wing / Savage Emperor (dash) |
| 3 | Akali | Kotemon > Musyamon > Grademon | Shinobi, Assassin | Data | Assassin | Meteor Slash / Ninja Blade / Grade Slash (nuke) |
| 3 | Evelynn | BlackGatomon > LadyDevimon > Lilithmon | Dark Ocean, Shade | Virus | Assassin | Cat's Eye Hypnotism / Darkness Wave / Phantom Pain (execute) |
| 3 | Irelia | Hackmon > BaoHuckmon > Jesmon | Pilgrims, Celestial, Sage | Data | Duelist | Fifth Rush / Fif Cross / Judgment of the Blade (line) |
| 3 | Jinx | Commandramon > Sealsdramon > Darkdramon | Treasure Hunters, Sharpshooter | Virus | Sniper | M16 Assassin / Death Behind / Gigastick Lance (burst) |
| 3 | Kalista | Syakomon > Coelamon > MarineAngemon | Dark Ocean, Duelist | Virus | Duelist | Black Pearl Blast / Fossil Bite / Ocean Love (execute) |
| 3 | Katarina | Gumdramon > Arresterdramon > Slayerdramon | Deva Generals, Treasure Hunters, Assassin | Vaccine | Assassin | Fire Vortex / Spin Caliber / Tenryu Slash (barrage) |
| 3 | Kennen | Tentomon > Kabuterimon > MegaKabuterimon | Shinobi, Keeper | Vaccine | Bruiser | Super Shocker / Electro Shocker / Horn Buster (slam) |
| 3 | Kindred | Labramon > Dobermon > Astamon | Onmyo Spirits, Hunter | Vaccine | Sniper | Cure Liqueur / Grau Larm / Maverick (nuke) |
| 3 | Lux | Salamon > Gatomon > Angewomon | Celestial, Charmer | Vaccine | Mystic | Puppy Howl / Lightning Paw / Celestial Arrow (line) |
| 3 | Nunu & Willump | Chuumon > Tyrannomon > MetalTyrannomon | Ancient Forest, Brawler | Virus | Bruiser | Cheese Bomb / Wild Buster / Nuclear Laser (execute) |
| 3 | Veigar | Aruraumon > Vegiemon > Vademon | Ancient Forest, Sorcerer | Data | Mystic | Nemesis Ivy / Sharp Leaf / Unidentified Flying Kiss (blossom) |
| 3 | Xin Zhao | Dracomon > Coredramon > Wingdramon | Deva Generals, Duelist | Data | Duelist | Tail Smash / Blue Flare Breath / Blaze Sonic Breath (slam) |
| 3 | Yuumi | Meicoomon > Meicrackmon VM > Raguelmon | Onmyo Spirits, Warder | Data | Support | X Scratch / Berserk Sinking / Form Taranis (heal) |
| 4 | Aatrox | BlackAgumon > BlackGreymon > BlackWarGreymon | Dark Ocean, Vanguard | Virus | Bruiser | Spitfire / Great Antler / Terra Destroyer (slam) |
| 4 | Ahri | Renamon > Kyubimon > Taomon | Onmyo Spirits, Sorcerer | Data | Mystic | Diamond Storm / Fox Tail Inferno / Talisman of Light (aoe) |
| 4 | Ashe | Biyomon > Birdramon > Garudamon | Ancient Forest, Hunter | Vaccine | Sniper | Spiral Twister / Meteor Wing / Shadow Wing (barrage) |
| 4 | Cassiopeia | Tapirmon > Wizardmon > Wisemon | Nightfall, Warder | Data | Mystic | Nightmare Syndrome / Thunder Cloud / Dialogue (burst) |
| 4 | Jhin | Dorumon > Dorugamon > DoruGreymon | Dark Ocean, Sharpshooter | Data | Sniper | Dash Metal / Power Metal / Metal Meteor (nuke) |
| 4 | Morgana | Piddomon > Ophanimon > Lucemon FM | Pilgrims, Charmer | Vaccine | Mystic | Fire Phaser / Eden's Javelin / Paradise Lost (drain) |
| 4 | Riven | Guilmon > Growlmon > Gallantmon | Nightfall, Keeper | Virus | Duelist | Rock Breaker / Plasma Blade / Lightning Joust (slam) |
| 4 | Sejuani | Gomamon > Ikkakumon > Zudomon | Treasure Hunters, Vanguard | Vaccine | Guardian | Marching Fishes / Harpoon Torpedo / Vulcan's Hammer (burst) |
| 4 | Shen | Wormmon > Stingmon > Dinobeemon | Shinobi, Sage, Warder | Free | Guardian | Sticky Net / Spiking Strike / Hell Masquerade (shield) |
| 4 | Talon | Jazamon > Jazardmon > Jazarichmon | Pilgrims, Assassin | Data | Assassin | Jaza Claw / Laser Eye / Messer Spiral (execute) |
| 4 | Warwick | BlackGabumon > BlackGarurumon > BlackMetalGarurumon | Celestial, Hunter, Brawler | Virus | Bruiser | Little Horn / Howling Wolf / Garuru Tomahawk (frenzy) |
| 5 | Azir | Paildramon > Imperialdramon FM > Imperialdramon PM | Deva Generals, Keeper, Sovereign | Free | Sniper | Death Parade Blaster / Gigadeath / Omni Blade (line) |
| 5 | Ezreal | Floramon > Lotosmon > Lotosmon | Ancient Forest, Charmer | Data | Sniper | Rain of Pollen / Serpent Cure / Serpent Ruin (line) |
| 5 | Kayn | Lopmon > Turuiemon > Antylamon | Fallen Choice, Shade | Data | Assassin | Blazing Ice / Gauntlet Claw / Asipatravana (dash) |
| 5 | Lee Sin | Veemon > Magnamon > Magnamon X | Celestial, Duelist | Free | Duelist | Vee Headbutt / Magna Punch / Plasma Shoot (stun) |
| 5 | Lillia | Myotismon > VenomMyotismon > GranDracmon | Nightfall, Sorcerer | Virus | Mystic | Crimson Lightning / Venom Infusion / Crystal Revolution (lullaby) |
| 5 | Sett | Leomon > GrapLeomon > BanchoLeomon | Bancho, Brawler | Vaccine | Bruiser | Fist of the Beast King / The King of Fists / Flashy Boss Punch (showstopper) |
| 5 | Yone | Ouryumon > Zanbamon > Alphamon | Ronin, Sage | Vaccine | Duelist | Eiseiryuoujin / Vorpal Blade / Blade of the Dragon King (line) |
| 5 | Zilean | Clockmon > Kimeramon > Millenniummon | Dark Ocean, Warder | Virus | Support | Chrono Breaker / Heat Viper / Time Unlimited (rewind) |
| 5 | Set 4.5 Ornn | Volcamon > PileVolcamon > Volcanicdramon | Ancient Forest, Vanguard, Digi-Forge | Data | Guardian | Big Bang Voice / Volcanic Driver / Volcanic Flare (slam) |
| 5 | Set 4.5 Samira | Impmon > Beelzemon > Beelzemon BM | Showtime, Sharpshooter | Virus | Sniper | Bada Boom / Double Impact / Death Slinger (barrage) |

## Items

Components: Brave Blade (AD), Swift Feather (attack speed), Spirit Staff (AP), Mana Lantern (mana), Chain Mail (armor), Ward Cloak (MR), Giant Sash (HP), Sparring Glove (crit and dodge), Digi-Egg (the Spatula). Digi-Egg + component Crests: Celestial, Duelist, Nightfall, Sorcerer, Vanguard, Ancient Forest, Deva Generals, Assassin. Drop-only Crests: Dark Ocean, Pilgrims, Treasure Hunters, Lunar, Shinobi, Onmyo Spirits, Brawler, Keeper, Warder, Sharpshooter, Hunter, Charmer. Items carry AI tags (ad / ap / tank / util) and explicit AI build lists.

| Item | Recipe | Effect |
|---|---|---|
| Dragon Fang | Brave Blade + Brave Blade | Deals 8% more damage. Each kill gives 15% attack damage for the rest of combat. |
| Titan Slayer | Brave Blade + Swift Feather | Deals 6% more damage, and 30% more to enemies with over 1600 max HP. |
| Data Gunblade | Brave Blade + Spirit Staff | Heals for 25% of all damage dealt. |
| Lantern Spear | Brave Blade + Mana Lantern | Basic attacks restore 5 extra mana. |
| Guardian Wing | Brave Blade + Chain Mail | The first time it would be knocked out, it stays at 1 HP, cannot be hurt for 2 seconds, then heals 400 HP. |
| Blood Fang | Brave Blade + Ward Cloak | Heals for 20% of damage dealt. Below 40% HP, gains a 25% max HP shield once. |
| War Banner | Brave Blade + Giant Sash | When combat starts, it and the allies beside it in the same row gain 30% attack speed. |
| Infinity Edge | Brave Blade + Sparring Glove | Gains 35% more critical strike chance and 10% crit damage. |
| Far Shot Bow | Swift Feather + Swift Feather | Gains 1 attack range and 12% more attack speed. |
| Rage Wing | Swift Feather + Spirit Staff | Each basic attack gives 6% attack speed, stacking. |
| Volt Lantern | Swift Feather + Mana Lantern | Every third basic attack sends lightning to 3 enemies for 80 magic damage. |
| Titan Will | Swift Feather + Chain Mail | Attacking or being attacked gives 2% attack damage and 2 ability power, up to 25 times. |
| Twin Shot | Swift Feather + Ward Cloak | Basic attacks also hit a second nearby enemy for 60% damage. |
| Void Portal | Swift Feather + Giant Sash | When combat starts, nearby enemies attack it. When it is knocked out, a Tankmon rises in its place. |
| Armor Breaker | Swift Feather + Sparring Glove | Basic attacks cut the target's armor by 40% for 3 seconds. |
| Ultima Staff | Spirit Staff + Spirit Staff | A lot of ability power. |
| Echo Rod | Spirit Staff + Mana Lantern | Its ability deals 150 extra magic damage to the first target and up to 3 enemies near it. |
| Iron Locket | Spirit Staff + Chain Mail | When combat starts, it and allies within 2 hexes in the same row gain a shield of 250 (Rookie), 325 (Champion) or 450 (final form) for 8 seconds. |
| Mana Zap | Spirit Staff + Ward Cloak | When an enemy within 2 hexes casts, it takes 150 magic damage. |
| Flame Tome | Spirit Staff + Giant Sash | Damage burns the target for 1.5% max HP per second and cuts its healing by a third. |
| Jewel Gauntlet | Spirit Staff + Sparring Glove | Abilities can critically strike. +25% crit damage. |
| Blue Lantern | Mana Lantern + Mana Lantern | Max mana -10. After casting, keeps 20 mana. |
| Frost Heart | Mana Lantern + Chain Mail | Enemies within 2 hexes lose 30% attack speed. |
| Power Chalice | Mana Lantern + Ward Cloak | When combat starts, it and the allies beside it in the same row gain 30 ability power. |
| Holy Ring | Mana Lantern + Giant Sash | Every 5 seconds, heals allies within 2 hexes for 80 HP plus 10% of their missing HP. |
| Justice Hand | Mana Lantern + Sparring Glove | Deals 12% more damage and heals for 12% of damage dealt. |
| Thorn Mail | Chain Mail + Chain Mail | When hit by a basic attack, deals 60 magic damage to adjacent enemies (1.5s cooldown). |
| Stone Hide | Chain Mail + Ward Cloak | Gains 12 armor and magic resist for each enemy within 2 hexes. |
| Flame Core | Chain Mail + Giant Sash | Every 2 seconds, burns the nearest enemy within 2 hexes. |
| Stillness Shroud | Chain Mail + Sparring Glove | When combat starts, enemies in its column lose a third of their max mana. |
| Dragon Scale | Ward Cloak + Ward Cloak | Takes 40% less magic damage. |
| Zephyr | Ward Cloak + Giant Sash | When combat starts, the enemy across from it is stunned for 3 seconds. |
| Quick Silver | Ward Cloak + Sparring Glove | Immune to stuns for the first 12 seconds. |
| Giga Sash | Giant Sash + Giant Sash | A lot of HP. |
| Trap Claw | Giant Sash + Sparring Glove | Blocks the first magic damage it would take and stuns the attacker for 3 seconds. |
| Thief Glove | Sparring Glove + Sparring Glove | Each combat, gains two random completed items. |
| Golden Digi-Egg | Digi-Egg + Digi-Egg | +1 team size while held by a Digimon on your board. |

| Artifact | Stats | Effect |
|---|---|---|
| Ebonwumon Shell | +20 MR, +150 HP | Regenerates 3% max HP per second. |
| Undying Armor | +15% AD, +12% AS, +20 armor | 60% of the damage it takes is spread over the next 5 seconds instead, and that delayed damage cannot knock it out. |
| Glacial Horn | +20 armor, +150 HP | Enemies that hurt it lose 35% attack speed for 2 seconds. An enemy slowed 7 times is frozen for 2 seconds (once every 10 seconds). |
| Golden Fang | +30% AD, +20% crit | Basic attacks knock out enemies below 10% HP. Each knockout this way pays 1 gold after combat. |
| Yggdrasil Core | +30% AD, +30 AP, +25% AS, +25 armor, +25 MR, +250 HP, +25% crit | A bit of every stat. |
| Mana Zanbato | +15% AD, +12% AS, +15 mana | After its first cast each combat, it restores 100 mana over 4 seconds. |
| Dark Cleaver | +30% AD, +150 HP | Its basic attacks cut armor by 40% and its magic damage cuts magic resist by 20 for 3 seconds. |
| Holy Shield Sanctum | +20 armor, +20 MR, +150 HP | When combat starts, allies next to it gain 40 armor and magic resist. |
| Rocket Fist | +20 armor, +150 HP, +15 mana | When combat starts, stuns the farthest enemy for 2 seconds. |
| Chrono Paradox | +15 AP, +20 armor, +20 MR | The first hit that would drop it below 40% HP is blocked; it freezes in time, untouchable, for 2.5 seconds. |

| Consumable | Effect |
|---|---|
| Magnetic Remover | Drag onto a Digimon to return all of its items to your item bench. |
| Reforger | Drag onto a Digimon to return its items to your bench as random new items of the same kind. |
| Loaded Dice | Drag onto a Digimon to reroll your shop for free with only Digimon that share a trait with it. |
| Copy Data | Drag onto a Digimon to get a Rookie-form copy of it on your bench. |
| Holy Spirit | Use on the Fallen Choice line (Lopmon) to take the holy path: its damage heals it for 40% of damage dealt. Removes the Dark Spirit. |
| Dark Spirit | Use on the Fallen Choice line (Lopmon) to take the dark path: it deals 40% more damage for the first 10 seconds. Removes the Holy Spirit. |

| Augment | Tier | Effect |
|---|---|---|
| Red Envelope | Silver | Gain 8 gold. |
| Festival Stall | Silver | Gain 2 random components. |
| Lucky Dice | Silver | Gain 1 free reroll every round. |
| Paper Charm | Silver | Gain 5 Tamer HP. |
| Dojo Training | Silver | Your team gains 12% attack damage and 12 ability power. |
| Lantern Light | Silver | Your team starts combat with 20 extra mana. |
| Shield Wall | Silver | Digimon in your front row gain 30 armor, 30 magic resist and 200 HP. |
| Archers' Nest | Silver | Digimon in your back two rows gain 25% attack speed. |
| Partner Bond | Silver | Your Partner gains 300 HP and 25 ability power. |
| Destiny Seeker | Silver | Partners show up in your shop far more often. Gain 3 gold. |
| Lucky Pouch | Gold | Gain 18 gold. |
| Treasure Chest | Gold | Gain a random completed item and a component. |
| Egg Hunt | Gold | Gain a Digi-Egg. |
| Steady Offerings | Gold | Gain 2 extra gold every round. |
| Shrine Blessing | Gold | Your team heals for 12% of damage dealt. |
| Stand United | Gold | Your team gains 3% attack damage and 3 ability power for each active trait. |
| Tome of Traits | Gold | Gain a random Crest. |
| Apprentice Smith | Gold | Gain a random artifact. |
| Instant Digivolve | Gold | Gain a random Champion-form Digimon of cost 2 or 3. |
| Celestial Grace | Gold | The first time each of your Digimon drops below 50% HP, it takes 30% less damage for 4 seconds. |
| Twin Destiny | Gold | The marked trait of your Partner counts one more time. |
| Fortune's Favor | Prismatic | Gain 35 gold. |
| Grand Armory | Prismatic | Gain 2 random completed items. |
| Golden Egg | Prismatic | Gain a Golden Digi-Egg (+1 team size). |
| Master Smith | Prismatic | Gain 2 random artifacts. |
| Giant Growth | Prismatic | Your team gains 25% max HP. |
| Soul Partners | Prismatic | Your Partner gains 50% max HP and 50 ability power, and its marked trait counts one more time. |
| Mega Digivolve | Prismatic | Gain a random Champion-form Digimon of cost 4. |
| Festival of Beasts | Prismatic | Gain a Partner of cost 4 right away (if you have none) and 4 gold. |
| Lunar Crown | Prismatic | Your lowest-stage Digimon on the board digivolves one stage for every combat. |

Wild Digimon: Goblimon, Kuwagamon, Ogremon, Witchmon, Etemon, Asuramon, Andiramon, Mammothmon, Whamon, MetalSeadramon, Azulongmon, Zhuqiaomon, Baihumon, Ebonwumon, Goldramon, Soulmon, Golemon, Tankmon.

## Balance

All runs 2026-10-15 on the final numbers.

### Duel (`node tools/balance.cjs --duel 5000 8 1 --set fates`, pace-independent)

Every trait is inside 45-58%:

Sage 57.3, Lunar 56.7, Celestial 56.5, Shade 56.2, Nightfall 56.1, Onmyo Spirits 56.1, Duelist 55.7, Ancient Forest 55.7, Warder 55.4, Sorcerer 54.8, Shinobi 54.7, Keeper 54.6, Dark Ocean 54.1, Pilgrims 54.1, Deva Generals 54.0, Vanguard 53.6, Sharpshooter 53.2, Bancho 52.9, Hunter 52.7, Charmer 52.3, Treasure Hunters 51.4, Digi-Forge 50.7, Brawler 50.4, Assassin 50.1, Sovereign 49.0, Ronin 47.1, Fallen Choice 47.1, Showtime 46.6.

Lines range from 45% (Gizamon, Biyomon) to 58% (Palmon). Treasure Hunters, Deva Generals, Digi-Forge and Fallen Choice have economy or game-long effects the duel cannot see, so their duel rates were kept near or a little below 50%.

### Full games, Hyper Roll (`node tools/balance.cjs 300 8 --set fates`)

- Top-4 rate of lines on final boards, weighted by cost: 1-cost 49%, 2-cost 44%, 3-cost 58%, 4-cost 67%, 5-cost 71%. Within a cost the spread is about 10-15 points (the widest are 1-cost lines with under 130 boards). 5-costs run 68-77%.
- Games end at a median of 7-2 (6-4 to 7-4 in a 40-game study).
- Wild rounds won: stage 1 99%, 2 92%, 3 94%, 4 93%, 5 88%, 6 54%, 7 39%.

### Full games, Normal (`node tools/balance.cjs 300 8 --set fates --pace normal`)

- Top-4 rate by cost: 1-cost 37%, 2-cost 43%, 3-cost 56%, 4-cost 58%, 5-cost 56% (5-costs 51-66%). 3-cost rerolls are strong in standard pace, as in TFT, and 5-costs are rarely upgraded. The lowest lines are 1-costs left on losing boards (Hagurumon 19% and Patamon 28%, under 60 boards each).
- Games last 33.6 combat rounds (29-39) and end in stage 6 (140 games) or 7 (160).
- Wild rounds won: stage 1 83%, 2 95%, 3 98%, 4 79%, 5 25%, 6 26%.
- Trait spread on final boards: top-4 rate 45-69%.

### Partners

Placement of final boards by Partner bonus (40-game studies; boards without a Partner are mostly early knockouts):

| Pace | 300 HP | 40 AP | 20% AD | 20% lower mana | no Partner |
|---|---|---|---|---|---|
| Hyper | 49% top 4 | 48% | 64% | 55% | 39% |
| Normal | 56% | 56% | 52% | 49% | 37% |

Forcing one bonus for every Partner in a game (16 games each, Hyper, before the final AP/AD/mana trim): HP 53%, AP 57%, AD 56%, mana 63% top 4. No bonus dominates. Every player is offered a Partner (320/320 in Hyper, 307/320 in Normal).

Digi-Forge forged 1-10 artifacts per 40 games; both Fallen Choice paths came out between 1st and 6th.

## Tests

- `node tests/sim.js 20 --set fates`: 20 games (24 combat rounds a game), no pool leaks, strict determinism. With `--pace normal`: 20 games, 33 combat rounds a game.
- `node tests/lockstep.js 3 --set fates`: lockstep ok.
- `node tests/sets.cjs --set fates`: all fates checks ok.
- `node tests/moves.cjs <playwright> <out> soft --set fates`: spec check ok, no generated looks, PASS (run in a temp copy on port 8172).
- `node tests/models.cjs fates`: 234 forms, 0 gaps.
- A solo browser game (`DT.UI.newGame('fates')`) played to stage 4 with no page errors.

## Engine notes

- Crest ids carry the set prefix (`items.emblemIdPrefix: 'fat_'`, so `fat_emblem_<trait>`): Faction Wars and Dragonlands share some of this set's trait ids.
- Using a consumable shows "<item> used" (for example "Loaded Dice used").
