# Dota Underlords items

How items worked, then the item pool at each data snapshot, then every item the wiki documents.

## How item choices worked

- Beta (2019-06 to 2019-12-18): items came from neutral creep "loot rounds" at rounds 1, 2, 3, 10, 15 ... 50. After each wave you picked 1 of 3 items. Losing the wave dropped the offer to the previous wave's tier (from 2019-07-19; before that, losing removed 2 of the options). Item categories: equipment (offensive, defensive, support), global items (player-wide rules, not attached to a hero), alliance items (global items tied to one alliance, disabled 2019-08-01) and, from 2019-08-30, contraptions (placeable board objects that do not use a unit slot).
- 2019-12-18: creep rounds removed. Items offered every 5 rounds during normal PvP rounds; the quality no longer depended on winning. All global items removed; the pool became equipment plus contraptions.
- 2020-02-25 (Season One): item rounds at 1, 4, 8, 12, 16, 20 ... (every 4). Each offer can be rerolled once; the 3 shown items are excluded from the reroll. New category: Hats, which add an alliance to the wearer.
- Knockout: item pick on odd rounds and with the starter crew.
- Classic mode (2020-08-27): creep loot rounds return.
- Items can go on heroes and contraptions (Target Buddy can hold one) but not on Underlords. Ability items (Refresher Orb, Void Stone, Octarine Essence and similar) need a hero with a castable ability.
- Enno's Yoink! steals the enemy's lowest-tier item at combat start; Aegis, Battle Fury, Divine Rapier, Mask of Madness, Octarine Essence, Refresher Orb and Void Stone cannot be stolen.
- Divine Rapier transfers to the opponent who beats you while it is in combat.

### Item tier odds by round (Standard, final)

| Round | T1 % | T2 % | T3 % | T4 % | T5 % |
|---|---|---|---|---|---|
| 1 | 100 |  |  |  |  |
| 4 | 20 | 80 |  |  |  |
| 8 |  | 100 |  |  |  |
| 12 |  | 30 | 70 |  |  |
| 16 |  |  | 100 |  |  |
| 20 |  |  | 20 | 80 |  |
| 24 |  |  |  | 70 | 30 |
| 28 |  |  |  | 50 | 50 |
| 32 |  |  |  | 30 | 70 |
| 36 |  |  |  |  | 100 |
| 40 |  |  |  |  | 100 |

Round 8 row on the wiki contains a typo ("1999999" for tier 1); read as 100% tier 2. Round 16 changed from 70/30 (t3/t4) to 100% t3 on 2020-04-23.

### Beta loot-round waves

| Round | Wave | Enemies | Item tier odds |
|---|---|---|---|
| 1 | Warmup | 2 Melee Creeps | T1 100% |
| 2 | Sparring Match | 1 Mega Creep, 2 Ranged Creeps | T1 80%, T2 20% |
| 3 | Gear up | 2 Mega Creeps, 2 Melee Creeps, 2 Ranged Creeps | T1 70%, T2 30% |
| 10 | First Intermission | 1 Big Golem, 2 Small Golems (Mega-Warrior) | T1 20%, T2 80% |
| 15 | Dog and pony show | 1 Big Wolf, 4 Small Wolves (Mega-Assassin) | T2 70%, T3 30% |
| 20 | Bear Trap! | Tomato and Potato hellbears (Mega-Blood-Bound) | T2 30%, T3 70% |
| 25 | Survival of the fittest | Revtel Wildwing, Wildwing Hatchling (Mega-Elusive) | T3 70%, T4 30% |
| 30 | Big dinos, big rewards | Thunderhide Alpha, 2 Thunderhides (Mega-Warlock) | T3 20%, T4 80% (from 2019-10-24; was T3 50/T4 50) |
| 35 | Things are heating up | Black Dragon (Mega-Scaled) | T4 70%, T5 30% (was T3 20/T4 80) |
| 40 | You're about to be trolled | 4 Dark Troll Buddies, Dark Troll Boss, Dark Troll Priest (Mega-Troll) | T4 50%, T5 50% (was T4 70/T5 30) |
| 45 | We've reached the endgame... | The Year Beast (Mega-Demon) | T4 30%, T5 70% (was 50/50) |
| 50 | Correction, NOW we've reached the endgame... | Roshan (spell immune) | T5 100% (was T4 40/T5 60) |

Neutral units used "Mega" alliances (Mega-Warrior, Mega-Assassin and so on) that only neutrals could have.

## Final item pool (2020-11-19 data, enabled items)

Several final items list 6-value arrays in the data. The first value is the standard item and is what is shown here. Item ids ending in `_rot1` are the 2020-08-27 rework versions.

| Tier | Item | Type | Effect (rendered from game data and localization) |
|---|---|---|---|
| 1 | Chainmail | defensive | +7 Armor. |
| 1 | Claymore | offensive | +33 Attack Damage. |
| 1 | Gloves of Haste | offensive | +25 Attack Speed. |
| 1 | Headdress | defensive | The equipped hero, and the units within 1 cell left or right of the them at the start of combat, gain +20 health per second. |
| 1 | Hood of Defiance | defensive | The equipped hero, and the units within 1 cell left or right of the them at the start of combat, gain +30% Magic Resistance. |
| 1 | Kaya | offensive | +20% Increased Ability Damage. |
| 1 | Morbid Mask | defensive | +30% lifesteal. |
| 1 | Talisman of Evasion | defensive | +25% evasion. |
| 1 | Vitality Booster | defensive | The equipped hero, and the units within 1 cell left or right of the them at the start of combat, gain +250 Health. |
| 1 | Void Stone | support | +7 Mana per second. |
| 2 | Arcane Boots | support | When equipped hero has greater than 50% mana, grant 25 mana to all allies 2 cells away. (10 second cooldown) |
| 2 | Armlet of Mordiggian | offensive | +120 Attack Damage. +10 Armor. Equipped hero loses 80 health per second. |
| 2 | Barricade | placeable | Two placeable barriers to block enemies and reduce ranged attack damage by 50%. Invulnerable and immune to magic. |
| 2 | Blade Mail | defensive | +5 armor. Reflects 20% of damage taken back to the attacker. |
| 2 | Blink Dagger | support | Teleport to the enemy farthest away from equipped hero and gain 60 mana. (15 second cooldown) |
| 2 | Desolator | offensive | Attack target loses 5 Armor for 5 seconds. |
| 2 | Dragon Lance | offensive | RANGED ONLY. +35 Attack Damage. Equipped hero has unlimited range. |
| 2 | Heaven's Halberd | support | +25% Evasion. When equipped hero attacks, disarm the target for 3 seconds. (10 second cooldown) |
| 2 | Paladin Sword | offensive | +25 Attack Damage. Allied healing is increased by 20%. |
| 2 | Quelling Blade | offensive | +25 Attack Damage. Equipped hero will prefer to attack summoned units. Attacks against summoned units crit for 200% damage. |
| 2 | Stonehall Cloak | defensive | Equipped hero gets +50 health for every unit they have killed. (Health varies per game mode.) |
| 2 | Stonehall Pike | offensive | Equipped hero gains +10 and in addition +3 damage for every unit they have killed, to a maximum of +120 damage. (Damage varies per game mode.) |
| 2 | Target Buddy | placeable | Taunts enemies and draws fire. Can equip items. Can't heal. |
| 2 | Vanguard | defensive | +375 Health. 50% chance to block 60 damage when taking Physical damage. |
| 3 | Battle Fury | offensive | MELEE ONLY. +50 Attack Damage. Melee attacks cleave 50% damage to nearby units. |
| 3 | Craggy Coat | defensive | Equipped hero gains +15 Armor but at the cost of -30 Attack Speed. |
| 3 | Crown of Antlers | hat | Equipped hero is a Hunter . |
| 3 | Eul's Scepter | support | +10 Mana per second. Whenever a unit attacks the equipped hero, it gets cycloned into the air, becoming invulnerable for 2.5 seconds. (10 second cooldown) |
| 3 | Leg Breaker's Fedora | hat | Equipped hero is a Brute . |
| 3 | Mango Tree | placeable | At the start of combat and every 0.5 seconds afterwards, the Mango Tree throws a mango up to 2 cells away. Any unit that touches a mango eats it to gain 50 mana and 100 health. |
| 3 | Mask of Madness | offensive | Equipped hero is silenced. +15% Lifesteal. +60 Attack Speed. |
| 3 | Necronomicon | support | Whenever equipped hero casts a spell, summon a Brill Bruiser. (Max 2 units alive at once) |
| 3 | Octarine Essence | support | Reduces ability cooldowns by 30%. |
| 3 | Pipe of Insight | defensive | +50% Magic Resistance. Once the first enemy has 100% mana, apply a shield that blocks 250 magic damage to allies up to 2 cells away. (once per battle) |
| 3 | Pirate Hat | hat | Equipped hero is a Swordsman . |
| 3 | Ristul Circlet | hat | Equipped hero is a Demon . |
| 3 | Silver Edge | support | +30 Attack Speed. +300 Health. Whenever equipped hero attacks a unit, break that unit for 5 seconds. (5 second cooldown) |
| 3 | Skull Basher | offensive | +25 Attack Damage. 25% chance to deal an extra 100 damage and stun the target for 1.5 seconds. |
| 3 | Witless Shako | defensive | Equipped hero gains +1000 HP but at the cost of gaining 50% less Mana. |
| 4 | Black King Bar | defensive | +45 Attack Damage. Equipped hero becomes Magic Immune for 10 seconds once the first enemy has 100% mana. (once per battle) |
| 4 | Butterfly | offensive | +35 Attack Damage. +35 Attack Speed. +35% Evasion. |
| 4 | Dagon | offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. (12 second cooldown) |
| 4 | Diffusal Blade | offensive | Equipped hero's attacks burn 20 mana per hit, doing 30 damage per hit. Damage done by the equipped hero generates 0 mana for the target. |
| 4 | Eye of Skadi | defensive | +600 Health. Slows enemies' move speed by -35 and attack speed by -55 on attack for 2.5 seconds. |
| 4 | Kaden's Blade | offensive | Instead of their normal damage, equipped hero's attacks deal 9% of the target's Max Health as Pure Damage. |
| 4 | Maelstrom | offensive | 25% chance on attack to cast chain lightning for 125 damage to 4 targets. Does 50% more damage to summoned units. |
| 4 | Mekansm | defensive | When equipped hero has less than 50% health, heal allies up to 3 cells away for 750 Health. (once per battle) |
| 4 | Monkey King Bar | offensive | Equipped hero gains +90 Attack Damage and their attacks cannot miss. |
| 4 | Moon Shard | offensive | +80 Attack Speed. |
| 4 | Refresher Orb | support | After casting an ability, refresh all cooldowns and restore 50 mana to equipped hero. (once per battle) |
| 4 | Scythe of Vyse | support | +10 Mana per second. When the equipped hero takes damage, transform the offending unit into a pig for 3.5 seconds. (15 second cooldown) |
| 5 | Bloodthorn | offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5 seconds. Attacks against the silenced hero can't miss and crit for 140% damage. (10 second cooldown) |
| 5 | Divine Rapier | offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose a round to another player, they steal the Divine Rapier. |
| 5 | Heart of Tarrasque | defensive | +1500 Health. Equipped hero regenerates 2% of max health per second. |
| 5 | Horn of the Alpha | support | 2 seconds after the battle begins, summons a powerful Thunderhide Companion. (once per battle) |
| 5 | Radiance | offensive | Burn enemies for 60 damage per second within 2 cells. Burning enemies have a 17% chance to miss their attacks. |
| 5 | Satanic | defensive | +850 health. Whenever equipped hero is below 50% health, gain +250% lifesteal for 3 seconds. (10 second cooldown) |
| 5 | Shiva's Guard | defensive | +10 armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed by -45 for 4 seconds. (once per battle) |
| 5 | Vesture of the Tyrant | support | Whenever an ally within 1 cell of the equipped hero receives a killing blow, that ally survives for 6 seconds. |
| 5 | Vladmir's Offering | support | Allies within 2 cells get +8% damage, +5 armor and +10% lifesteal. |

## Item pool at After The Outlanders Update and Knockout (2019-12-06)

| Tier | Item | Type | Effect (rendered from game data and localization) |
|---|---|---|---|
| 1 | Blight Stone | offensive | Attack target loses 3 Armor. |
| 1 | Brooch of the Martyr | support | +50% Mana gained from receiving damage. +50% debuff resistance. |
| 1 | Chainmail | defensive | +7 Armor. |
| 1 | Claymore | offensive | +30 Attack Damage. |
| 1 | Embarrassment of Riches | global | Neutral rounds offer one additional item choice. |
| 1 | Gloves of Haste | offensive | +25 Attack Speed. |
| 1 | Hood of Defiance | defensive | +50% Magic Resistance. +10 Health Regen. |
| 1 | Tranquil Boots | support | +100% Move Speed. +25 Health Regen. |
| 1 | Vitality Booster | defensive | +500 Health. |
| 2 | Aegis of the Immortal | global | When you would otherwise take fatal damage, take no damage and destroy Aegis. |
| 2 | Arcane Boots | support | When equipped hero has greater than 50% mana, restore 25 mana to all allies 2 cells away. (once per battle) |
| 2 | Barricade | placeable | Two placeable barriers to block enemies and ranged attacks. Immune to spells. |
| 2 | Big-Time Contract | support | +500 Health. Equipped unit is Blood-Bound  in addition to its other types. |
| 2 | Blade Mail | defensive | Reflects 30% of damage taken back to the attacker. |
| 2 | Blink Dagger | support | Teleport to the enemy farthest away from equipped hero and gain 50 mana. (15s cooldown) |
| 2 | Brooch of the Aggressor | support | +100% Mana gained from attacks. |
| 2 | Celebratory Murder Gong | support | Every time the equipped hero gets a kill they generate 20 Hype for your Underlord. |
| 2 | Force Staff | support | When the equipped hero takes damage from a melee attack, push the attacker up to 6 cells away. (10s cooldown) |
| 2 | Helm of the Undying | defensive | +5 Armor. Equipped hero survives for an extra 4 seconds after receiving a killing blow. |
| 3 | Assassin's Veil | offensive | Equipped hero always attacks the enemy Underlord if able. Equipped hero's attacks against the enemy Underlord remove 10 Hype (3 second cooldown). |
| 3 | Bracers of Desperation | support | Equipped hero resets their cooldown and gets full mana when below 30% health. Attack target's magic resistance is reduced by 30%. |
| 3 | Dawning of Ristul | global | Healing for both crews is reduced by [unknown]%. |
| 3 | Healing Ward | placeable | Heals friendly units within 1 cell for 40hp/s. |
| 3 | Maelstrom | offensive | 25% chance on attack to cast chain lightning for 100 damage to 4 targets. |
| 3 | Mask of Madness | offensive | Hero is silenced. +20% Lifesteal. +60 Attack Speed. |
| 3 | Mekansm | support | When equipped hero has less than 50% health, heal allies up to 3 cells away for 500 Health. (once per battle) |
| 3 | Octarine Essence | support | Reduces ability cooldown by 30%. |
| 3 | Poaching Knife | offensive | Equipped hero has a 30% chance of producing 1 gold when they kill an enemy. Attacks against units with less than 20% health crit for 200%. |
| 3 | Sacred Relic | offensive | +60 Attack Damage. |
| 3 | Skull Basher | offensive | +25 Attack Damage. 25% chance to deal an extra 100 and stun targets for 1.5s. |
| 3 | Target Buddy | placeable | Taunts enemies and draws fire. Can equip items. Can't heal. |
| 3 | Vanguard | defensive | +500 Health. 50% chance to block 70 damage when attacked. |
| 4 | Battle Fury | offensive | MELEE ONLY. +70 Attack Damage. Melee attacks cleave 35% damage to nearby units. |
| 4 | Black King Bar | defensive | Equipped hero becomes Magic Immune for 7s once the first enemy has 100% mana. (once per battle) |
| 4 | Daedalus | offensive | +70 Attack Damage. 30% chance to Critical Hit for 200% damage. |
| 4 | Dagon | offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. (once per battle) |
| 4 | Eye of Skadi | offensive | +800 Health. Slows enemies move speed by -35 and attack speed by -45 on attack. |
| 4 | Friends and Family Discount | global | All units are 1 gold cheaper. |
| 4 | Moon Shard | offensive | +80 Attack Speed. |
| 4 | Pipe of Insight | defensive | +50% Magic Resistance. Once the first enemy has 100% mana, apply a shield that blocks 400 magic damage to allies up to 1 cell away. (once per battle) |
| 4 | Refresher Orb | support | After casting an ability, refresh all cooldowns and restore 50 mana to equipped hero. Additionally reduce remaining cooldown time by 50% and restore 25 mana to allies 1 cell away. (once per battle) |
| 4 | Scythe of Vyse | support | When the equipped hero takes damage, transform the offending unit into a pig for 4s. (15s cooldown) |
| 4 | Tombstone | placeable | Ally and enemy heroes spawn zombies when they die within 1 cell of the Tombstone. |
| 5 | A Higher Class of Criminal | global | Units in shop offered as if you were 1 level higher. |
| 5 | Assault Cuirass | defensive | Equipped hero and adjacent allies gain +15 Attack Speed and +10 Armor. Adjacent enemies lose 15 Attack Speed and 10 Armor. |
| 5 | Bloodthorn | offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5s. Attacks against the silenced hero can't miss and crit for 140%. (10s cooldown) |
| 5 | Desperate Measures | global | Your units gain +[unknown]% Attack Damage and +[unknown]% Mana gain on damage taken/received for each point of damage your Underlord has taken. |
| 5 | Divine Rapier | offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose a round to another player, they get the Divine Rapier. |
| 5 | Expanded Roster | global | +1 Unit cap. |
| 5 | Heart of Tarrasque | defensive | +2000 Health. Regenerate 2% of Max Health every 1s. |
| 5 | Radiance | offensive | Burns enemies up to 2 cells away for 60 damage per second. |
| 5 | Shiva's Guard | defensive | +10 Armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed for 4s. (once per battle) |

## Item pool at The Big Update (Underlords added) (2019-10-24)

| Tier | Item | Type | Effect (rendered from game data and localization) |
|---|---|---|---|
| 1 | Blight Stone | offensive | Attack target loses 3 Armor. |
| 1 | Brooch of the Martyr | support | +50% Mana gained from receiving damage. +50% debuff resistance. |
| 1 | Chainmail | defensive | +10 Armor. |
| 1 | Claymore | offensive | +30 Attack Damage. |
| 1 | Embarrassment of Riches | global | Neutral rounds offer one additional item choice. |
| 1 | Gloves of Haste | offensive | +25 Attack Speed. |
| 1 | Hood of Defiance | defensive | +50% Magic Resistance. +10 Health Regen. |
| 1 | Tranquil Boots | support | +100% Move Speed. +25 Health Regen. |
| 1 | Vitality Booster | defensive | +500 Health. |
| 2 | Aegis of the Immortal | global | When you would otherwise take fatal damage, take no damage and destroy Aegis. |
| 2 | Arcane Boots | support | When equipped hero has greater than 50% mana, restore 25 mana to all allies 2 cells away. (once per battle) |
| 2 | Barricade | placeable | Two placeable barriers to block enemies and ranged attacks. Immune to spells. |
| 2 | Big-Time Contract | support | +500 Health. Equipped unit is Blood-Bound  in addition to its other types. |
| 2 | Blade Mail | defensive | Reflects 30% of damage taken back to the attacker. |
| 2 | Blink Dagger | support | Teleport to the enemy farthest away from equipped hero and gain 50 mana. (15s cooldown) |
| 2 | Brooch of the Aggressor | support | +100% Mana gained from attacks. |
| 2 | Celebratory Murder Gong | support | Every time the equipped hero gets a kill they generate 20 Hype for your Underlord. |
| 2 | Force Staff | support | When the equipped hero takes damage from a melee attack, push the attacker up to 6 cells away. (10s cooldown) |
| 2 | Helm of the Undying | defensive | +5 Armor. Equipped hero survives for an extra 4 seconds after receiving a killing blow. |
| 2 | Smuggler | global | Quality of items from neutral rounds are offered from 1 wave higher. |
| 2 | Target Buddy | placeable | Taunts enemies and draws fire. Can equip items. Can't heal. |
| 3 | Assassin's Veil | offensive | Equipped hero always attacks the enemy Underlord if able. Equipped hero's attacks against the enemy Underlord remove 10 Hype (3 second cooldown). |
| 3 | Bracers of Desperation | support | Equipped hero resets their cooldown and gets full mana when below 30% health. Attack target's magic resistance is reduced by 30%. |
| 3 | Dawning of Ristul | global | Healing for both crews is reduced by [unknown]%. |
| 3 | Healing Ward | placeable | Heals friendly units within 1 cell for 40hp/s. |
| 3 | Maelstrom | offensive | 25% chance on attack to cast chain lightning for 100 damage to 4 targets. |
| 3 | Mask of Madness | offensive | Hero is silenced. +20% Lifesteal. +60 Attack Speed. |
| 3 | Mekansm | support | When equipped hero has less than 50% health, heal allies up to 3 cells away for 500 Health. (once per battle) |
| 3 | Octarine Essence | support | Reduces ability cooldown by 50%. |
| 3 | Poaching Knife | offensive | Equipped hero has a 30% chance of producing 1 gold when they kill an enemy. Attacks against units with less than 20% health crit for 200%. |
| 3 | Sacred Relic | offensive | +60 Attack Damage. |
| 3 | Skull Basher | offensive | +25 Attack Damage. 25% chance to deal an extra 100 and stun targets for 1.5s. |
| 3 | Vanguard | defensive | +500 Health. 50% chance to block 70 damage when attacked. |
| 4 | Battle Fury | offensive | MELEE ONLY. +70 Attack Damage. Melee attacks cleave 35% damage to nearby units. |
| 4 | Black King Bar | defensive | Equipped hero becomes Magic Immune for 7s once the first enemy has 100% mana. (once per battle) |
| 4 | Daedalus | offensive | +70 Attack Damage. 30% chance to Critical Hit for 200% damage. |
| 4 | Dagon | offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. (once per battle) |
| 4 | Eye of Skadi | offensive | +800 Health. Slows enemies move speed by -35 and attack speed by -45 on attack. |
| 4 | Friends and Family Discount | global | All units are 1 gold cheaper. |
| 4 | Moon Shard | offensive | +80 Attack Speed. |
| 4 | Pipe of Insight | defensive | +50% Magic Resistance. Once the first enemy has 100% mana, apply a shield that blocks 400 magic damage to allies up to 1 cell away. (once per battle) |
| 4 | Refresher Orb | support | After casting an ability, refresh all cooldowns and restore 50 mana to equipped hero. Additionally reduce remaining cooldown time by 50% and restore 25 mana to allies 1 cell away. (once per battle) |
| 4 | Scythe of Vyse | support | When the equipped hero takes damage, transform the offending unit into a pig for 4s. (15s cooldown) |
| 4 | Tombstone | placeable | Ally and enemy heroes spawn zombies when they die within 1 cell of the Tombstone. |
| 5 | A Higher Class of Criminal | global | Units in shop offered as if you were 1 level higher. |
| 5 | Assault Cuirass | defensive | Equipped hero and adjacent allies gain +15 Attack Speed and +10 Armor. Adjacent enemies lose 15 Attack Speed and 10 Armor. |
| 5 | Bloodthorn | offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5s. Attacks against the silenced hero can't miss and crit for 140%. (10s cooldown) |
| 5 | Desperate Measures | global | Your units gain +[unknown]% Attack Damage and +[unknown]% Mana gain on damage taken/received for each point of damage your Underlord has taken. |
| 5 | Divine Rapier | offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose a round to another player, they get the Divine Rapier. |
| 5 | Expanded Roster | global | +1 Unit cap. |
| 5 | Heart of Tarrasque | defensive | +2000 Health. Regenerate 2% of Max Health every 1s. |
| 5 | Radiance | offensive | Burns enemies up to 2 cells away for 60 damage per second. |
| 5 | Shiva's Guard | defensive | +10 Armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed for 4s. (once per battle) |

## Item pool at Late beta, before The Big Update (2019-10-10)

| Tier | Item | Type | Effect (rendered from game data and localization) |
|---|---|---|---|
| 1 | Blight Stone | offensive | Attack target loses 3 Armor. |
| 1 | Brooch of the Martyr | support | +50% Mana gained from receiving damage. +50% debuff resistance. |
| 1 | Chainmail | defensive | +10 Armor. |
| 1 | Claymore | offensive | +30 Attack Damage. |
| 1 | Embarrassment of Riches | global | Neutral rounds offer one additional item choice. |
| 1 | Gloves of Haste | offensive | +25 Attack Speed. |
| 1 | Hood of Defiance | defensive | +50% Magic Resistance. +10 Health Regen. |
| 1 | Tranquil Boots | support | +100% Move Speed. +25 Health Regen. |
| 1 | Vitality Booster | defensive | +250 Health. |
| 2 | Aegis of the Immortal | global | When you would otherwise take fatal damage, take no damage and destroy Aegis. |
| 2 | Arcane Boots | support | When equipped hero has greater than 50% mana, restore 25 mana to all allies 2 cells away. (once per battle) |
| 2 | Barricade | placeable | Two placeable barriers to block enemies and ranged attacks. Immune to spells. |
| 2 | Big-Time Contract | support | +250 Health. Equipped unit is Blood-Bound  in addition to its other types. |
| 2 | Blade Mail | defensive | Reflects 30% of damage taken back to the attacker. |
| 2 | Blink Dagger | support | Teleport to the enemy farthest away from equipped hero and gain 50 mana. (15s cooldown) |
| 2 | Brooch of the Aggressor | support | +100% Mana gained from attacks. |
| 2 | Fall From Grace | global | Human  units count as Heartless  instead when counting Alliances. |
| 2 | Force Staff | support | When the equipped hero takes damage from a melee attack, push the attacker up to 6 cells away. (10s cooldown) |
| 2 | Helm of the Undying | defensive | +5 Armor. Equipped hero survives for an extra 4 seconds after receiving a killing blow. |
| 2 | Smuggler | global | Quality of items from neutral rounds are offered from 1 wave higher. |
| 2 | Target Buddy | placeable | Taunts enemies and draws fire. Can equip items. |
| 3 | Bracers of Desperation | support | Equipped hero resets their cooldown and gets full mana when below 30% health. Attack target's magic resistance is reduced by 30%. |
| 3 | Dawning of Ristul | global | Healing for both crews is reduced by [unknown]%. |
| 3 | Healing Ward | placeable | Heals friendly units within 1 cell for 20hp/s. |
| 3 | Maelstrom | offensive | 25% chance on attack to cast chain lightning for 100 damage to 4 targets. |
| 3 | Mask of Madness | offensive | Hero is silenced. +20% Lifesteal. +60 Attack Speed. |
| 3 | Mekansm | support | When equipped hero has less than 50% health, heal allies up to 3 cells away for 250 Health. (once per battle) |
| 3 | Octarine Essence | support | Reduces ability cooldown by 50%. |
| 3 | Poaching Knife | offensive | Equipped hero has a 30% chance of producing 1 gold when they kill an enemy. Attacks against units with less than 20% health crit for 200%. |
| 3 | Sacred Relic | offensive | +60 Attack Damage. |
| 3 | Skull Basher | offensive | +25 Attack Damage. 25% chance to deal an extra 100 and stun targets for 1.5s. |
| 3 | Vanguard | defensive | +250 Health. 50% chance to block 70 damage when attacked. |
| 4 | Battle Fury | offensive | MELEE ONLY. +70 damage. Melee attacks cleave 35% damage to nearby units. |
| 4 | Black King Bar | defensive | Equipped hero becomes Magic Immune for 7s once the first enemy has 100% mana. (once per battle) |
| 4 | Daedalus | offensive | +70 Attack Damage. 30% chance to Critical Hit for 200% damage. |
| 4 | Dagon | offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. (once per battle) |
| 4 | Eye of Skadi | offensive | +400 Health. Slows enemies move speed by -35 and attack speed by -45 on attack. |
| 4 | Friends and Family Discount | global | All units are 1 gold cheaper. |
| 4 | Moon Shard | offensive | +80 Attack Speed. |
| 4 | Pipe of Insight | defensive | +50% Magic Resistance. Once the first enemy has 100% mana, apply a shield that blocks 400 magic damage to allies up to 1 cell away. (once per battle) |
| 4 | Refresher Orb | support | After casting an ability, refresh all cooldowns and restore 50 mana to equipped hero. Additionally reduce remaining cooldown time by 50% and restore 25 mana to allies 1 cell away. (once per battle) |
| 4 | Scythe of Vyse | support | When the equipped hero takes damage, transform the offending unit into a pig for 4s. (15s cooldown) |
| 4 | Tombstone | placeable | Ally and enemy heroes spawn zombies when they die within 1 cell of the Tombstone. |
| 5 | A Higher Class of Criminal | global | Units in shop offered as if you were 1 level higher. |
| 5 | Assault Cuirass | defensive | Equipped hero and adjacent allies gain +15 Attack Speed and +10 Armor. Adjacent enemies lose 15 Attack Speed and 10 Armor. |
| 5 | Bloodthorn | offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5s. Attacks against the silenced hero can't miss and crit for 140%. (10s cooldown) |
| 5 | Desperate Measures | global | Your units gain +[unknown]% Attack Damage and +[unknown]% Mana gain on damage taken/received for each point of damage your Underlord has taken. |
| 5 | Divine Rapier | offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose a round to another player, they get the Divine Rapier. |
| 5 | Expanded Roster | global | +1 Unit cap. |
| 5 | Heart of Tarrasque | defensive | +1000 Health. Regenerate 2% of Max Health every 1s. |
| 5 | Radiance | offensive | Burns enemies up to 2 cells away for 60 damage per second. |
| 5 | Shiva's Guard | defensive | +10 Armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed for 4s. (once per battle) |

## Item pool at Open beta launch (2019-06-20)

| Tier | Item | Type | Effect (rendered from game data and localization) |
|---|---|---|---|
| 1 | Blight Stone | offensive | Target gets -3 Armor. |
| 1 | Boots of Speed | support | +100% Move Speed. |
| 1 | Brooch of the Martyr | support | +50% Mana gained from receiving damage. |
| 1 | Chainmail | defensive | +5 Armor. |
| 1 | Claymore | offensive | +21 Attack Damage. |
| 1 | Cloak | defensive | +15% Magic Resistance. |
| 1 | Embarrassment of Riches | global | Neutral rounds offer one additional item choice. |
| 1 | Gloves of Haste | offensive | +20 Attack Speed. |
| 1 | Soul Sucking Syphon | global | Allied units 1 cell away from Warlocks  heal for 25% of all damage caused by abilities and items. |
| 1 | Strange Bedfellows | global | Demon Hunters  gain +30% Pure Damage for each unique allied Demon  unit. |
| 1 | Unstable Reactor | global | Allied Inventors  explode on death dealing 20% of their Max Health to all enemies 1 cell away. |
| 1 | Vitality Booster | defensive | +250 Health. |
| 2 | Aegis of the Immortal | global | Prevent your next loss of life and destroy Aegis. Win streaks are preserved. |
| 2 | Age of Chivalry | global | Non-Knight units gain +10% damage reduction for each allied Knight  unit 1 cell away. |
| 2 | Arcane Boots | support | When equipped hero has greater than 50% mana, restore 25 mana to all allies 1 cell away. (once per battle) |
| 2 | Blade Mail | defensive | Reflects 30% of damage taken back to the attacker. |
| 2 | Blink Dagger | support | At the start of the battle, teleport behind enemy lines to the enemy farthest away from equiped hero. |
| 2 | Brooch of the Aggressor | support | +100% Mana gained from attacks. |
| 2 | Completing the Cycle | global | When a Druid  dies, it heals allies 1 cell away over time for 30% of its total health for 6 seconds. |
| 2 | Coordinated Assault | global | Troll  units give +25 Attack Speed to allies 1 cell away. |
| 2 | Fall From Grace | global | Human  units count as Heartless  when counting Alliances. |
| 2 | Forged in Battle | global | Brawny  units start the round with +50 Max Health for every unit they have killed. |
| 2 | Indomitable Will | global | Debuffs last 50% as long on Human  units. |
| 2 | Mask of Madness | offensive | Hero is silenced. +10% Lifesteal. +40 Attack Speed. |
| 2 | Octarine Essence | support | Reduces ability cooldown by 50%. |
| 2 | Silver Lining | global | Get 1 gold when you lose a fight. |
| 2 | Smuggler | global | Quality of items from neutral rounds are offered from 1 wave higher. |
| 2 | Summoning Stone | global | Allied summoned units have +150 Health and +30 Attack Speed. |
| 2 | Tooth and Claw | global | Savage  units apply a stackable 10 damage/s bleed on attack. |
| 2 | Unstoppable | global | Warriors  survive for an extra 2 seconds after receiving a killing blow. |
| 3 | A Higher Class of Criminal | global | Units in shop offered as if you were 1 level higher. |
| 3 | Check the Bodies | global | Scrappy  units have a 20% chance of producing 1 gold when they kill an enemy. |
| 3 | Dragon's Hoard | global | Dragon  units gain 1% Attack damage for every gold you own at the beginning of combat. |
| 3 | Elusive Targets | global | Elusive   units are invisible for the first 3 seconds of a fight. |
| 3 | Eye of Skadi | offensive | +400 Health. Slows enemies on attack. |
| 3 | Final Flash | global | Mages  reset their cooldown and get full mana when below 30% health. |
| 3 | Font of Creation | global | Primordial  units spawn Eidolons on death. Eidolons are weak ranged Primordial  units that don't spawn Eidolons. |
| 3 | Hood of Defiance | defensive | +50% Magic Resistance. +10 Health Regen. |
| 3 | Hunter's Focus | global | Hunters  deal 10% extra Attack Damage for each other Hunters  attacking the same target. |
| 3 | Mekansm | support | When equipped hero has less than 50% health, heal allies up to 3 cells away for 250 Health. (once per battle) |
| 3 | Pocket Sand | global | Assassins  blind enemies 1 cell away from where they land for 2 seconds (50% miss chance). |
| 3 | Retaliate | global | Units attacking Scaled  units take 80 damage/sec for 3 seconds. This timer resets each time the Scaled  unit is attacked. |
| 3 | Sacred Relic | offensive | +60 Attack Damage. |
| 3 | Shaman Pluck | global | Units attacking a Shaman  have a 5% chance of being turned into a chicken. |
| 3 | Skull Basher | offensive | +25 Attack Damage. 25% chance to stun targets for 1.5s. |
| 3 | Vanguard | defensive | +250 Health. 50% chance to block 70 damage when attacked. |
| 3 | Wicked Intent | global | Opponent's healing is reduced by 25% per unique Heartless  unit you have. |
| 4 | Battle Fury | offensive | MELEE ONLY. Melee attacks cleave 30% damage to nearby units. |
| 4 | Black King Bar | defensive | Equipped hero becomes Magic Immune for 7s once the first enemy has 100% mana. (once per battle) |
| 4 | Daedalus | offensive | +80 Attack Damage. 30% chance to Critical Hit for 235% damage. |
| 4 | Dagon | offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. (once per battle) |
| 4 | Friends and Family Discount | global | All units are 1 gold cheaper. |
| 4 | Maelstrom | offensive | 25% chance on attack to cast chain lightning for 100 damage to 4 targets. |
| 4 | Moon Shard | offensive | +80 Attack Speed. |
| 4 | Radiance | offensive | Burns enemies up to 2 cells away for 60 damage per second. |
| 4 | Refresher Orb | support | Refresh all cooldowns and restore 50 mana after casting an ability. (once per battle) |
| 4 | Vicious Intent | global | Each of your surviving units, including summons, deals 1 additional damage to opponents if they lose to you. |
| 5 | Assault Cuirass | defensive | Equipped hero and adjacent allies have +15 Attack Speed and +10 Armor. Adjacent enemies have -15 Attack Speed and -10 Armor. |
| 5 | Bloodthorn | offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5s. Attacks against the silenced hero can't miss and crit for 140%. (10s cooldown) |
| 5 | Desperate Measures | global | Your units gain +1% Attack Damage per 5 points of damage your Underlord has taken. |
| 5 | Divine Rapier | offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose a round to another player, they get the Divine Rapier. |
| 5 | Expanded Roster | global | +1 Unit cap. |
| 5 | Heart of Tarrasque | defensive | +1000 Health. Regenerate 2% of Max Health every 1s. |
| 5 | Recruiter | global | The first reroll each round is free. |
| 5 | Shiva's Guard | defensive | +10 Armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed for 4s. (once per battle) |

## Every item on the wiki

Tier and effect as the wiki last recorded them. "Seen enabled" lists the first and last data snapshot where the item was enabled (Season One has no item data, so a 2020 item may show only "Final").

| Tier | Item | Type | Effect | Wiki status | Seen enabled |
|---|---|---|---|---|---|
| 1 | Arcane Boots | Support | When equipped hero has greater than 50% mana, restore 25 mana to all allies 2 cell away. | listed | Beta launch to Final |
| 1 | Big-Time Contract | Support | Equipped unit is Blood-bound in addition to its other types. | removed | Pre-Big Update to Outlanders |
| 1 | Blight Stone | Offensive | Attack target loses 3 Armor. | removed | Beta launch to Outlanders |
| 1 | Boots of Speed | Support | +100% Move Speed. | removed | Beta launch |
| 1 | Brooch of the Martyr | Support | +50% Mana gained from receiving damage. +50% debuff resistance. | removed | Beta launch to Outlanders |
| 1 | Chainmail | Defensive | +7 Armor. | listed | Beta launch to Final |
| 1 | Claymore | Offensive | +33 Attack Damage. | listed | Beta launch to Final |
| 1 | Cloak | Defensive | +15% Magic Resistance. | removed | Beta launch |
| 1 | Embarrassment of Riches | Global | Neutral rounds offer one additional item choice. | removed | Beta launch to Outlanders |
| 1 | Gloves of Haste | Offensive | +25 Attack Speed | listed | Beta launch to Final |
| 1 | Headdress | Defensive | The equipped hero, and the units within 1 cell left or right side of them at the start of combat, gain +20 health per second. | listed | Final |
| 1 | Hood of Defiance | Defensive | The equipped hero, and the units within 1 cell left or right of them at the start of combat, gain +30% Magic Resistance. | listed | Beta launch to Final |
| 1 | Kaya | Offensive | +20% Increased Ability Damage. | listed | Final |
| 1 | Morbid Mask | Defensive | +30% lifesteal. | listed | Final |
| 1 | Ogre Cap | Hat | Equipped unit is Blood-bound. | out of rotation | - |
| 1 | Talisman of Evasion | Defensive | +25% evasion. | listed | Final |
| 1 | Tranquil Boots | Support | +100% Move Speed. +25 Health Regen. | removed | Pre-Big Update to Outlanders |
| 1 | Vitality Booster | Defensive | The equipped hero, and the units within 1 cell left or right of them at the start of combat, gain +250 Health. | listed | Beta launch to Final |
| 1 | Void Stone | Support | +7 Mana per second. | listed | Final |
| 2 | Aegis of the immortal | Global | When you would otherwise take fatal damage, take no damage and destroy Aegis. | removed | Beta launch to Outlanders |
| 2 | Armlet of Mordiggian | Offensive | +120 Attack Damage. +10 Armor. Equipped hero loses 80 health per second. | listed | Final |
| 2 | Barricade | Contraption | Two placable barriers to block enemies and reduce ranged attack damage by 50%. Invulnerable and immune to magic. | listed | Pre-Big Update to Final |
| 2 | Blade Mail | Defensive | +5 armor. Reflects 20% of damage taken back to the attacker. | listed | Beta launch to Final |
| 2 | Blink Dagger | Support | Teleport to the enemy farthest away from equipped hero and gain 60 mana. | listed | Beta launch to Final |
| 2 | Brooch of the Aggressor | Support | 100% Mana gained from attacks. | removed | Beta launch to Outlanders |
| 2 | Celebratory Murder Gong | Support | Every time the equipped hero gets a kill they generate 20 Hype for your Underlord. | removed | Big Update to Outlanders |
| 2 | Completing the Cycle | Global | When a Druid dies, allies up to 2 cells away are healed for [3%/5%/7%] of their max health per second for 6 seconds. | removed | Beta launch |
| 2 | Coordinated Assault | Global | Troll units give [+10/+20/+30] Attack Speed to allies 1 cell away. | removed | Beta launch |
| 2 | Crystalys | Offensive | +25 Attack Damage. 20% chance to Critical Hit for 200% damage. | out of rotation | - |
| 2 | Desolator | Offensive | Attack target loses 5 Armor for 5 seconds. | listed | Final |
| 2 | Dragon Lance | Offensive | RANGED ONLY. +35 Attack Damage. Equipped hero has unlimited range. | listed | Final |
| 2 | Fall From Grace | Global | Human units count as Heartless instead when counting Alliances. | removed | Beta launch to Pre-Big Update |
| 2 | Force Staff | Support | When the equipped hero takes damage from a melee attack, push the attacker up to 6 cells away. | out of rotation | Pre-Big Update to Outlanders |
| 2 | Heaven's Halberd | Support | +25% Evasion. When equipped hero attacks, disarm the target for 3 seconds. (10 seconds cooldown) | listed | Final |
| 2 | Helm of the Undying | Defensive | +5 Armor. Equipped hero survives for an extra 4.0 seconds after receiving a killing blow. | removed | Pre-Big Update to Outlanders |
| 2 | Mantle of the Flayed Twins | Hat | Equipped hero is a Deadeye | out of rotation | - |
| 2 | Orb of Venom | Support | Enemies damaged by the equipped hero's attacks are Poisoned. | out of rotation | - |
| 2 | Paladin Sword | Offensive | +25 Attack Damage. Allied healing is increased by 20% | listed | Final |
| 2 | Quelling Blade | Offensive | +25 Attack Damage. Equipped hero will prefer to attack summoned units. Attacks against summoned units crit for 200% damage. | listed | Final |
| 2 | Silver Lining | Global | Get 1 charge when you win a fight. When you lose a fight, if charges remain, get 1 gold and lose 1 charge.. | removed | Beta launch |
| 2 | Smuggler | Global | Quality of items from neutral rounds are offered from 1 wave higher. | removed | Beta launch to Big Update |
| 2 | Stonehall Cloak | Defensive | Equipped hero gets +50 health for every unit they have killed. (+200 health in Knockout). Summons count as 0.25. | listed | Final |
| 2 | Stonehall Pike | Offensive | Equipped hero gains +10 and in addition +3 damage for every unit they have killed, to a maximum of +120 damage. (+12 damage per kill in Knockout). | listed | Final |
| 2 | Summoning Stone | Global | Allied summoned units have +150 Health and +45 Attack Speed. | removed | Beta launch |
| 2 | Target Buddy | Contraption | Taunts enemies and draws fire. Can equip items. Can't heal. | listed | Pre-Big Update to Final |
| 2 | Vanguard | Defensive | +375 Health. 50% chance to block 60 damage when taking Physical Damage. | listed | Beta launch to Final |
| 3 | Aeon Disk | Defensive | +500 health. When equipped hero's health is reduced below 50%, they become invulnerable, untargetable, and disarmed for 2 seconds. | removed | - |
| 3 | Assassin's Veil | Offensive | Equipped hero always attacks the enemy Underlord if able. Equipped hero's attacks against the enemy Underlord remove 10 Hype (3.0 second cooldown). | removed | Big Update to Outlanders |
| 3 | Battle Fury | Offensive | MELEE ONLY. +50 Attack Damage. Melee attacks cleave 50% damage to nearby units. | listed | Beta launch to Final |
| 3 | Bracers of Desperation | Support | Equipped hero resets their cooldown and gets full mana when below 30% health. Attack target's magic resistance is reduced by 30%. | removed | Pre-Big Update to Outlanders |
| 3 | Craggy Coat | Defensive | Equipped hero gains +15 Armor but at the cost of -30 Attack Speed. | listed | Final |
| 3 | Crown of Antlers | Hat | Equipped hero is a Hunter | listed | Final |
| 3 | Dawning of Ristul | Global | Healing for both crews is reduced by 50%. | removed | Pre-Big Update to Outlanders |
| 3 | Dragon's Hoard | Global | Dragon units gain [0.5%/1%/1.5%] Attack damage for every gold you own at the beginning of combat. | removed | Beta launch |
| 3 | Elusive Targets | Global | Elusive units have 100% evasion for the first [2/3/4] seconds of a fight. | removed | Beta launch |
| 3 | Eul's Scepter | Support | +10 Mana per second. Whenever a unit attacks the equipped hero, it gets cycloned into the air, becoming invulnerable for 2.5 seconds. | listed | Final |
| 3 | Final Flash | Global | Mages reset their cooldown and get full mana when below [10%/20%/30%] health. | removed | Beta launch |
| 3 | Hand of Midas | Support | +30 Attack Speed. 25% chance of getting 1 gold whenever equipped hero kills an enemy. | out of rotation | - |
| 3 | Leg Breaker's Fedora | Hat | Equipped hero is a Brute | listed | Final |
| 3 | Mango Tree | Contraption | At the start of combat and every 2.0 seconds afterwards, the Mango Tree throws a mango up to 2 cells away. Any unit that touches a mango eats it to gain 50 mana and 100 health. | listed | Final |
| 3 | Mask of Madness | Offensive | Equipped hero is silenced. 15% Lifesteal. +60 Attack Speed. | listed | Beta launch to Final |
| 3 | Necronomicon | Support | Whenever equipped hero casts a spell, summon a Brill Bruiser. (Max 2 units alive at once) | listed | Final |
| 3 | Octarine Essence | Support | Reduces ability cooldown by 30% | listed | Beta launch to Final |
| 3 | Pipe of Insight | Defensive | +50% Magic Resistance. Once the first enemy has 100% mana, apply a shield that blocks 250 magic damage to allies up to 2 cells away. (once a battle) | listed | Pre-Big Update to Final |
| 3 | Pirate Hat | Hat | Equipped hero is a Swordsman. | listed | Final |
| 3 | Poaching Knife | Offensive | Equipped hero has a 30% chance of producing 1 gold when they kill an enemy. Attacks against units with less than 20% health crit for 200%. | removed | Pre-Big Update to Outlanders |
| 3 | Ristul Circlet | Hat | Equipped hero is a Demon. | listed | Final |
| 3 | Sacred Relic | Offensive | +60 Attack Damage. | removed | Beta launch to Outlanders |
| 3 | Silver Edge | Support | +30 Attack Speed. +300 Health. Whenever equipped hero attacks a unit, break that unit for 5 seconds. | listed | Final |
| 3 | Skull Basher | Offensive | +25 Attack Damage. 25% chance to deal an extra 100 damage and stun the target for 1.5 seconds. | listed | Beta launch to Final |
| 3 | Vicious Intent | Global | Each of your surviving units, including summons, deal 1 additional damage to opponents if they lose to you. | removed | Beta launch |
| 3 | Witless Shako | Defensive | Equipped hero gains +1000 HP but at the cost of gaining 50% less Mana. | listed | Final |
| 4 | Black King Bar | Defensive | +45 Attack Damage. Equipped hero becomes Magic Immune for 10s once the first enemy has 100% mana. | listed | Beta launch to Final |
| 4 | Butterfly | Offensive | +35 Attack Damage. +35 Attack Speed. +35% Evasion. | listed | Final |
| 4 | Daedalus | Offensive | +70 Attack Damage. 30% chance to Critical Hit for 200% damage. | removed | Beta launch to Outlanders |
| 4 | Dagon | Offensive | Deal 800 damage to a random enemy unit that has 50% or less Health. | listed | Beta launch to Final |
| 4 | Diffusal Blade | Offensive | Equipped hero's attacks burn 20 mana per hit, doing 30 damage per hit. Damage done by the equipped hero generates 0 mana for the target. | listed | Final |
| 4 | Eye of Skadi | Defensive | +600 Health. Slows enemies' move speed by -35 and attack speed by -55 on attack for 2.5 seconds. | listed | Beta launch to Final |
| 4 | Friends and Family Discount | Global | All units are 1 gold cheaper. | removed | Beta launch to Outlanders |
| 4 | Healing Ward | Contraption | Heals friendly units within 1 cell for 50 health per second. | removed | Pre-Big Update to Outlanders |
| 4 | Kaden's Blade | Offensive | Instead of their normal damage, equipped hero's attacks deal 9% of the target's Max Health as Pure Damage. | listed | Final |
| 4 | Maelstrom | Offensive | 25% chance on attack to cast chain lightning for 125 damage to 4 targets. Does 50% more damage to summoned units. | listed | Beta launch to Final |
| 4 | Mekansm | Support | When equipped hero has less than 50% health, heal allies up to 3 cells away for 750 health. | listed | Beta launch to Final |
| 4 | Monkey King Bar | Offensive | Equipped hero gains +90 Attack Damage and their attack cannot miss. | listed | Final |
| 4 | Moon Shard | Offensive | +80 Attack Speed | listed | Beta launch to Final |
| 4 | Recruiter | Global | Rerolls cost 1 gold less. | removed | Beta launch |
| 4 | Refresher Orb | Support | After casting an ability, refresh all cooldowns and restore 50 mana to equipped hero. | listed | Beta launch to Final |
| 4 | Scythe of Vyse | Support | +10 Mana per Second. When the equipped hero takes damage, transform the offending unit into a pig for 3.5s. | listed | Pre-Big Update to Final |
| 5 | A Higher Class of Criminal | Global | Units in shop offered as if you were 1 level higher. | removed | Beta launch to Outlanders |
| 5 | Aegis of the Immortal | Support | Equipped hero will resurrect with 50% of its max health 3 seconds after dying. | out of rotation | Beta launch to Outlanders |
| 5 | Assault Cuirass | Defensive | Equipped hero and adjacent allies gain +15 Attack Speed and +10 Armor. Adjacent enemies lose 15 Attack Speed and 10 Armor. | removed | Beta launch to Outlanders |
| 5 | Bloodthorn | Offensive | +70 Attack Damage. When an enemy hero has more than 75% mana, silence them for 5s. Attacks against the silenced hero can't miss and crit for 140%. | listed | Beta launch to Final |
| 5 | Desperate Measures | Global | Your units gain +0.2% Attack Damage and +0.2% Mana gain on damage taken/received for each point of damage your Underlord has taken. | removed | Beta launch to Outlanders |
| 5 | Divine Rapier | Offensive | +330 Attack Damage. If Divine Rapier is in combat and you lose the round to another player, they get the Divine Rapier. | listed | Beta launch to Final |
| 5 | Expanded Roster | Global | +1 Unit cap. | removed | Beta launch to Outlanders |
| 5 | Heart of Tarrasque | Defensive | +1500 Health. Regenerate 2% of Max Health every 1s. | listed | Beta launch to Final |
| 5 | Horn of the Alpha | Support | 2.0 seconds after the battle begins, summon a powerful Thunderhide Companion. | listed | Final |
| 5 | Radiance | Offensive | Burns enemies up for 60 damage per second within 2 cells. Burning enemies have a 17% chance to miss their attacks. | listed | Beta launch to Final |
| 5 | Satanic | Defensive | +850 health. Whenever equipped hero is below 50% health, gain +250% lifesteal for 3.0 seconds. | listed | Final |
| 5 | Shiva's Guard | Defensive | +10 Armor. Emits a freezing wave that deals 250 damage to enemies within 3 cells and slows their movement and attack speed for 4s. | listed | Beta launch to Final |
| 5 | Tombstone | Contraption | Ally and enemy heroes spawn zombies when they die within 1 cell of the Tombstone. | out of rotation | Pre-Big Update to Outlanders |
| 5 | Vesture of the Tyrant | Support | Whenever an ally within 1 cell of the equipped hero receives a killing blow, that ally survives for 6.0 seconds. | listed | Final |
| 5 | Vladmir | Support | Allies within 2 cells get +8% damage, +5 armor and +10% lifesteal. | listed | - |
| - | Age of Chivalry | Global | Non-Knight units take [7.5/10/12.5]% less physical and magical damage for each allied Knight unit 1 cell away. | removed | Beta launch |
| - | Check the Bodies | Global | Scrappy units have a [10%/20%/30%] chance of producing 1 gold when they kill an enemy. | removed | Beta launch |
| - | Font of Creation | Global | Primordial units spawn Eidolons on death. Eidolons are weak ranged Primordial units that don't spawn Eidolons. | removed | Beta launch |
| - | Forged in Battle | Global | Brawny units start the round with [+25/+50/+75] Max Health for every unit they have killed. | removed | Beta launch |
| - | Hunter's Focus | Global | Hunters deal [5%/7%/10%] extra Attack Damage for each other Hunters attacking the same target. | removed | Beta launch |
| - | Indomitable Will | Global | Debuffs last [25%/50%/75%] less time on Human units. | removed | Beta launch |
| - | Pocket Sand | Global | Assassins blind enemies 1 cell away from where they land for [1/2/3] seconds (50% miss chance and 50% less mana generated when attacked). | removed | Beta launch |
| - | Retaliate | Global | Units attacking Scaled units take [30/45/60] damage/sec for 3 seconds. This timer resets each time the Scaled unit is attacked. | removed | Beta launch |
| - | Shaman Pluck | Global | Units attacking a Shaman have a [7%/12%/17%] chance of being turned into a chicken. | removed | Beta launch |
| - | Soul Sucking Syphon | Global | Allied units 1 cell away from Warlocks heal for [25%/35%/50%] of all damage caused by abilities and enemies. | removed | Beta launch |
| - | Strange Bedfellows | Global | Demon Hunters gain [+30%/+40%/+50%] Pure Damage for each unique allied Demon unit. | removed | Beta launch |
| - | Tooth and Claw | Global | Savage units apply a stackable [7/10/13] damage/sec bleed on attack. | removed | Beta launch |
| - | Unstable Reactor | Global | Allied Inventors explode on death dealing [10%/20%/30%] of their Max Health to all enemies 1 cell away. | removed | Beta launch |
| - | Unstoppable | Global | Warriors survive for an extra [1/2/3] seconds after receiving a killing blow. | removed | Beta launch |
| - | Wicked Intent | Global | Opponent's healing is reduced by [10%/15%/25%] per unique Heartless unit you have. | removed | Beta launch |

## Item change logs (wiki)

- **Arcane Boots**: April 23, 2020: Cooldown changed from 15 to 10.; December 18, 2019: Cooldown changed from once per battle to 15 seconds.; July 19, 2019: Range changed from 1 to 2.
- **Big-Time Contract**: February 25, 2020: Renamed to Ogre Cap; December 18, 2019: Tier changed from 2 to 1. / No longer grants bonus health.; October 24, 2019: Health Bonus changed from 250 to 500; October 10, 2019: Players can now get more than 1 of this item again.; October 02, 2019: Players can only have a maximum of 1 of this item now.; August 01, 2019: Changed from "Equipped unit is Blood-bound in addition to its other types." to "+250 Health. Equipped unit is Blood-bound in addition to its other types."; July 12, 2019: Fixed Big-Time Contract sometimes not applying.
- **Blight Stone**: December 18, 2019: Removed from the game.; June 27, 2019: Fixed stacking on every hit. It will stack if different units are attacking with it.
- **Brooch of the Martyr**: December 18, 2019: Removed from the game.; August 01, 2019: Changed from "+50% Mana gained from receiving damage." to "+50% Mana gained from receiving damage. +50% debuff resistance."
- **Chainmail**: October 31, 2019: Bonus Armor changed 10 -> 7.; August 01, 2019: Bonus Armor changed from +7 to +10; July 19, 2019: Bonus Armor changed from 10 to 7.
- **Claymore**: January 24, 2020: Attack damage increased from +30 to +33; August 01, 2019: Attack Damage changed from +21 to +30
- **Cloak**: August 01, 2019: Cloak has been removed from the game.
- **Embarrassment of Riches**: December 18, 2019: Removed from the game.
- **Gloves of Haste**: August 01, 2019: Attack Speed changed from +20 to +25
- **Headdress**: August 27, 2020: The equipped hero, and allied units within 1 cell left or right of them at the start of combat, gain +20 health per second.
- **Hood of Defiance**: August 27, 2020: The equipped hero, and allied units within 1 cell left or right of them at the start of combat, gain +30% Magic Resistance.; August 01, 2019: Tier changed from 3 to 1.
- **Morbid Mask**: January 24, 2020: Lifesteal changed from 20% to 30%
- **Ogre Cap**: February 25, 2020: Renamed to Ogre Cap; December 18, 2019: Tier changed from 2 to 1. / No longer grants bonus health.; October 24, 2019: Health Bonus changed from 250 to 500; October 10, 2019: Players can now get more than 1 of this item again.; October 02, 2019: Players can only have a maximum of 1 of this item now.; August 01, 2019: Changed from "Equipped unit is Blood-bound in addition to its other types." to "+250 Health. Equipped unit is Blood-bound in addition to its other types."; July 12, 2019: Fixed Big-Time Contract sometimes not applying.
- **Talisman of Evasion**: December 19, 2019: Evasion bonus increased from 20% to 25%.
- **Tranquil Boots**: December 18, 2019: Removed from the game.; August 01, 2019: Health Regen changed from +16 to +25.
- **Vitality Booster**: August 27, 2020: The equipped hero, and allied units within 1 cell left or right of them at the start of combat, gain +250 Health.; December 18, 2019: Health Bonus changed from 500 to 375; October 24, 2019: Health Bonus changed from 250 to 500
- **Void Stone**: August 27, 2020: Mana per second changed from 10 to 7.
- **Aegis of the immortal**: December 18, 2019: Removed from the game.; July 12, 2019: Now only activates on damage that would otherwise be fatal. It no longer preserves streaks.
- **Armlet of Mordiggian**: April 23, 2020: Health loss per second changed from 100 to 80.; April 03, 2020: Bonus Attack Damage changed from 90 to 120.
- **Barricade**: August 27, 2020: Gives two placeable barriers to block enemies and reduce ranged attack damage by 50%. Invulnerable and immune to magic.; February 25, 2020: Tier changed from 3 to 2 / Maximum Health changed from [750, 750, 750] to [600, 600, 600]; January 24, 2020: Number of Barricades changed from 2 to 3 / Tier changed from 2 to 3 / Maximum Health per Barricade changed from [600, 600, 600] to [750, 750, 750]; December 18, 2019: Health changed from 800 to 600.
- **Blade Mail**: August 27, 2020: Tier changed from 3 to 2. Damage Reflection percentage changed from 30 to 20.; January 24, 2020: Armor bonus now shows during the preparation phase.; December 18, 2019: Tier changed from 2 to 3. / Now grants +5 armor.
- **Blink Dagger**: April 23, 2020: Mana gain changed from 50 to 60.; July 12, 2019: Units blinking using Blink Dagger will clear their active target and reacquire once the blink is done. Fixes units chasing their original enemy.; July 04, 2019: Now generates 50 mana when activated. / Cooldown reduced from 60s to 15s.
- **Brooch of the Aggressor**: December 18, 2019: Removed from the game.
- **Celebratory Murder Gong**: December 18, 2019: Removed from the game.
- **Completing the Cycle**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Heal percentage changed from 5% to 3/5/7% of max health and heal range changed from 1 cell to 2 cells
- **Coordinated Assault**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Attack speed buff changed from +25 to +10/20/30; July 12, 2019: Coordinated Assault auras don't stack. NOTE: More than one copy will still increase the bonus.; July 04, 2019: It should be truly coordinated since it actually works now.; June 25, 2019: Now stacks.
- **Dragon Lance**: August 27, 2020: Attack Damage changed from 20 to 35.
- **Fall From Grace**: October 24, 2019: Fall From Grace: removed from the game.; July 12, 2019: Fall From Grace: item description changed to make clearer that the item turns Human into Heartless, it does not add Heartless as an additional alliance.
- **Force Staff**: April 23, 2020: Pushed unit takes 50% longer to travel away.; November 07, 2019: No longer disabled by Break.; August 01, 2019: Now pushes melee attackers away from the equipped hero when taking damage.; July 19, 2019: Tier changed from 3 to 2.
- **Helm of the Undying**: December 18, 2019: Removed from the game.; November 14, 2019: Fixed an issue where Break would lead to an Undying unit doing end-of-round damage. / If a unit with the Helm survives the fight, it won't be killed at the end of the round.; November 07, 2019: As before, Break will prevent a unit with the Helm from entering Undying state. Now, in addition, if the unit is broken after the Helm activates, it will become targetable and killable again.; August 08, 2019: Undying units, if they are in the undying state, will never deal damage to the opponent, but will still count when determining which side won a round.
- **Orb of Venom**: January 30, 2020: Now poisons targets.; January 24, 2020: Damage Per Second changed from 30 to 35
- **Quelling Blade**: March 19, 2020: Will not target summons outside of the carrying unit's attack range.
- **Silver Lining**: July 19, 2019: Can only have one. / Charges start at 1. / [Undocumented] Removed from the game.; July 12, 2019: Fix Silver Lining crash.; July 04, 2019: Now reads "Get 1 charge when you win a fight. When you lose a fight, if charges remain, get 1 gold and lose 1 charge." / Previously read: "Get 1 gold when you lose a fight"; June 26, 2019: Fixed it granting 2 gold instead of 1.
- **Smuggler**: November 14, 2019: Removed from the game.
- **Stonehall Cloak**: February 28, 2020: Summons only count for 0.25 of a kill.
- **Stonehall Pike**: April 30, 2020: Stonehall Pike now confers a base bonus damage of +10 / Damage per kill changed from +4 to +3 (+16 to +12 in Knockout) / Total bonus damage is capped at +120; February 28, 2020: Damage bonus increased from 3 to 4. / Summons only count for 0.25 of a kill.; December 19, 2019: Damage bonus decreased from 4 to 3.
- **Summoning Stone**: September 26, 2019: With most summons removed, removed the Summoning Stone item.; July 19, 2019: Attack Speed Bonus changed from 30 to 45.
- **Target Buddy**: February 25, 2020: Tier changed from 3 to 2 / Armor changed from 10 to 5 / Maximum Health changed from 1500 to 1200; December 18, 2019: Health changed from 2000 to 1500. / Added festive hat.; October 31, 2019: Tier changed 2 -> 3.; October 10, 2019: Can no longer be healed.
- **Vanguard**: April 23, 2020: Damage Blocked changed from 70 to 60.; December 18, 2019: Tier changed from 3 to 2 / Health bonus changed from 500 to 375; October 24, 2019: Health Bonus changed from 250 to 500
- **Aeon Disk**: February 25, 2020: Removed from the game.; December 28, 2019: Tier changed from 4 to 3.
- **Assassin's Veil**: December 18, 2019: Removed from the game.
- **Battle Fury**: August 27, 2020: Cleave Damage changed from 35 to 50%.; January 09, 2020: Bonus Attack Damage changed from 35 to 50 / Fixed an interaction with Ember Spirit's ability Sleight of Fist where Battle Fury could cleave damage to 4 additional targets, rather than 2, on each Fist hit.; December 18, 2019: Tier changed from 4 to 3; July 19, 2019: Damage added as 70. / Cleave % changed from 30 to 35.
- **Bracers of Desperation**: December 18, 2019: Removed from the game.
- **Dawning of Ristul**: December 18, 2019: Removed from the game.
- **Dragon's Hoard**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Damage percentage per gold changed from 1% to 0.5/1.0/1.5%
- **Elusive Targets**: July 19, 2019: Likewise, in the same game, player A could be offered Elusive Targets at tier 2 with a duration of 3s while player B could be offered Elusive Targets at tier 1 with a duration of 2s. All players will have the same number of Alliance items per tier in their item pool. / Alliance items no longer have an inherent Tier and are in their own Alliance category. / Invisibility changed to 100% evasion for 2/3/4s
- **Eul's Scepter**: August 27, 2020: Mana per second changed from from 5 to 10.
- **Final Flash**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Remaining health percent trigger changed from 30% to 10/20/30%
- **Hand of Midas**: February 25, 2020: Tier changed from 2 to 3.; January 24, 2020: Percent Chance to gain gold on kill changed from 30% to 25%
- **Mango Tree**: February 28, 2020: Can no longer equip items.
- **Mask of Madness**: December 18, 2019: Lifesteal decreased from 20 to 15.; July 19, 2019: Tier changed from 2 to 3. / Attack Speed increased from 40 to 60. / Lifesteal increased from 10 to 20.
- **Octarine Essence**: October 31, 2019: Cooldown reduction changed 50% -> 30%.; August 01, 2019: Tier changed from 2 to 3.
- **Pipe of Insight**: August 27, 2020: Shield HP changed from 225 to 250. Radius changed from 1 to 2 cells.; January 24, 2020: Tier changed from 4 to 3.
- **Poaching Knife**: December 18, 2019: Removed from the game.
- **Sacred Relic**: December 18, 2019: Removed from the game.
- **Vicious Intent**: August 01, 2019: Vicious Intent has been removed from the game.; July 19, 2019: Tier changed from 4 to 3.
- **Black King Bar**: January 06, 2020: Can now trigger while the unit which has it equipped is casting its own ability.; December 19, 2019: Magic immunity duration increased from 5s to 10s.; December 18, 2019: Now also grants +45 Attack Damage.; June 27, 2019: Purges the user on activation.; June 17, 2019: Now grants Magic Immune for 7 seconds once the first enemy has 100% mana (once per battle).
- **Daedalus**: December 18, 2019: Removed from the game.; July 19, 2019: Crit Multiplier changed from 235 to 200. / Damage changed from 80 to 70.
- **Dagon**: December 18, 2019: Cooldown changed from once per battle to 12.0 seconds.
- **Diffusal Blade**: January 16, 2020: Clarified tooltip to mention the damage it does per hit. / Damage per hit changed from 20 to 30
- **Eye of Skadi**: April 23, 2020: Loss to enemy attack speed changed from -45 to -55.; October 24, 2019: Health Bonus changed from 400 to 800; July 19, 2019: Tier changed from 3 to 4.
- **Friends and Family Discount**: December 18, 2019: Removed from the game.
- **Healing Ward**: February 25, 2020: Removed from the game.; December 18, 2019: Tier changed from 3 to 4. / Heal per second changed from 40 to 50.; October 24, 2019: Heal per Second changed from 20 to 40
- **Kaden's Blade**: September 04, 2020: Kaden's Blade's damage is now affected by changes to attack damage on a percentage basis (like the Swordsman and Fallen buffs and like Medusa's Split Shot damage decrease).; August 27, 2020: Damage change from 7% to 9% of target's Max Health.; March 19, 2020: When the carrier is broken, the carrier does its normal attack instead.
- **Maelstrom**: August 27, 2020: Tier changed from 3 to 4. 25% chance on attack to cast chain lightning for 125 damage to 4 targets. Does 50% more damage to summoned units.; December 28, 2019: Damage now reduced by 20% each lightning leap, so only the first target hit each cast takes full damage.; July 19, 2019: Tier changed from 4 to 3.
- **Mekansm**: January 24, 2020: Tier changed from 3 to 4; December 18, 2019: Heal amount increased from 500 to 750; October 24, 2019: Heal amount changed from 250 to 500
- **Recruiter**: August 01, 2019: Recruiter has been removed from the game.; July 19, 2019: Changed from 'First Reroll is Free' to 'Rerolls now cost 1 gold". / Tier changed from 5 to 4.
- **Refresher Orb**: December 18, 2019: No longer provides benefits to allies.; August 15, 2019: In addition to its current actions, halves the remaining cooldown time of active cooldowns of allies within 1 cell and restores 25 mana to them.
- **Scythe of Vyse**: January 09, 2020: Fixed an issue where Scythe could waste its charge on an un-piggable enemy.; December 18, 2019: Now additionally provides +10 Mana per second. / Pig duration changed from 4.0 to 3.5 seconds.
- **A Higher Class of Criminal**: December 18, 2019: Removed from the game.; July 19, 2019: Tier changed from 3 to 5.
- **Aegis of the Immortal**: March 19, 2020: Resurrection time changed from 5 to 3 / Is not disabled by break.
- **Assault Cuirass**: December 18, 2019: Removed from the game.
- **Bloodthorn**: August 15, 2019: No longer targets silenced/hexed/passive-ability units.
- **Desperate Measures**: December 18, 2019: Removed from the game.; July 19, 2019: ; June 25, 2019: Now also increases mana gained on receving/dealing damage by the same amount as attack damage.
- **Divine Rapier**: July 12, 2019: Fixed Divine Rapier being removed at the start of combat. Losing a Divine Rapier now no longer removes a potential item slot.
- **Expanded Roster**: December 18, 2019: Removed from the game.
- **Heart of Tarrasque**: August 27, 2020: Health Bonus changed from 2000 to 1500.; October 24, 2019: Health Bonus changed from 1000 to 2000
- **Horn of the Alpha**: September 24, 2020: No longer does player damage at the end of the round.; January 24, 2020: Attack Damage Minimum changed from 100 to 120 / Attack Damage Maximum changed from 150 to 160 / Attack Rate changed from 0.935 to 1.0 / Maximum Health changed from 3750 to 4500
- **Radiance**: December 18, 2019: Burning enemies have a 17% chance to miss their attacks.; July 19, 2019: Tier changed from 4 to 5.
- **Satanic**: April 23, 2020: Lifesteal changed from 200% to 250%. / Cooldown changed from 15 to 10.; April 03, 2020: Health percent when Lifesteal activates changed from 30% to 50% / Activation Cooldown changed from 20 to 15 seconds; January 24, 2020: Bonus health changed from +750 to +850.
- **Tombstone**: January 16, 2020: Zombie: / Attack rate changed from 0.833 to 1.0; December 18, 2019: Tier changed from 4 to 5. / Zombie: / Attack Damage Minimum changed from [50, 70, 90] to [75, 105, 135] / Attack Damage Maximum changed from [70, 90, 110] to [105, 135, 165]; October 24, 2019: Armor changed from 20 to 10; September 19, 2019: Raise Zombies: / Range changed from 2 cells to 1.
- **Vesture of the Tyrant**: January 09, 2020: Duration of the Undying effect changed from 4.0 to 6.0; December 19, 2019: OLD: "Allies within 1 cell can't die until the equipped hero dies." / NEW: "Whenever an ally within 1 cell of the equipped hero receives a killing blow, that ally survives for 4.0 seconds."
- **Vladmir**: August 27, 2020: Tier changed from 4 to 5.; January 16, 2020: Damage percent increase changed from 10% to 8%; December 28, 2019: Tier changed from 3 to 4.; December 23, 2019: Multiple of this item no longer stack.
- **Age of Chivalry**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Damage reduction for both physical and magical damage changed from -10 to -7/-10/-12.5 per adjacent Knight
- **Check the Bodies**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Atack speed buff changed from +25 to +10/20/30.
- **Font of Creation**: August 01, 2019: Attack Damage Minimum changed from (40, 50) to (40, 80) / Attack Damage Maximum changed from (60, 70) to (60, 120) / Armor changed from 20 to (5, 10) / Maximum Health changed from 300 to (300, 600); July 24, 2019: Eidolon HP changed from [300, 400, 500] to 300 / Eidolon Max Damage changed from [70, 80, 90] to [60, 70, 80]; July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Primordials now produce higher rank Eidolons based on the tier of Font of Creation. Eidolon stats affected: / Health changed from 240 to 300/400/500. / Damage changed from 50-80 to 40-70/50-80/60-90
- **Forged in Battle**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Max health bonus changed from +50 to +25/+50/+75
- **Hunter's Focus**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Damage bonus changed from +10% to +5%/+7%/+10%
- **Indomitable Will**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Debuff resist changed from 50% to 25%/50%/75%
- **Pocket Sand**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Effect duration changed from 2 seconds to 1/2/3 seconds; June 25, 2019: Now also slows mana gained on receiving damage by 50%. / Will debuff a random enemy unit within 2 cells if no units are within 1 cell of the assassin landing.
- **Retaliate**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Damage per second changed from 80 to 30/45/60
- **Shaman Pluck**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Hex chance changed from 17% to 7%/12%/17% / Ignores magic-immune enemies. / Chance increased from 5% to 17%.
- **Soul Sucking Syphon**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Spell lifesteal percentage changed from 25% to 25/35/50%; June 21, 2019: Soul Sucking Syphon will now only attract Warlocks and Mages. The item still works for other unit types, but only those two will try to get close to benefit.
- **Strange Bedfellows**: July 19, 2019: Each player can have Alliance items appear at different tiers. For example, player A could be offered Strange Bedfellows at tier 1 where it would provide a +30% bonus while player B could be offered Strange Bedfellows at tier 3 where it would provide a +50% bonus. / Alliance items no longer have an inherent Tier and are in their own Alliance category. / Bonus damage changed from +30% to +30/40/50%.
- **Tooth and Claw**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Bleed damage per stack changed from 10 to 7/10/13; July 12, 2019: Tooth and Claw now has proper duration.; July 04, 2019: Fixed not stacking correctly.
- **Unstable Reactor**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Explosion damage changed from 20% to 10/20/30% of Max Health.
- **Unstoppable**: July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Invulnerability duration changed from 2 to 1/2/3; June 26, 2019: Fixed some cases that caused this item to not work reliably.
- **Wicked Intent**: August 01, 2019: Wicked Intent has been removed from the game.; July 19, 2019: Alliance items no longer have an inherent Tier and are in their own Alliance category. / Heal prevention percentage changed from 25% to 10/15/25% per Heartless unit; July 12, 2019: Fix Wicked Intent amplifying regen instead of dampening it. / Fix crazy numbers on healing display when Wicked Intent is in play.; June 25, 2019: Now also blocks HP regen.
