# Underlords

Dota Underlords (Valve, 2019-2020) as a Digi-Tactics set. Base: Season One and the final patch (2020-11-19), with beta ideas (creep rounds, global items) where they add to the game. Mode `underlords`, set ids `underlords` and `underlords_ko` (Knockout). Corpus: `docs/inspo/underlords/`.

Files: `js/sets/underlords.js` (data and rules), `underlords-mech.js` (alliances, hero kits, 3-star effects, items, Tamers, offers, economy hooks), `underlords-art.js` (item and Tamer portraits, move looks, basic attacks), `underlords-models.js` (model keys for every form).

## Contents

- 62 hero lines: the whole final hero pool (16 / 14 / 13 / 12 / 7 by tier). Each line is a three-form Digimon line, cost = Underlords tier, 2-3 alliances exactly as the hero had them. `src` on each line names the hero.
- 4 Tamer lines (the Underlords), 11 summon lines and 7 creep lines (not in the shop).
- 26 alliances, all from the final pool, with the Underlords breakpoints.
- 71 items: 59 tiered items (6 hats) and 12 global items. No recipes.
- 8 Tamer powers (4 Tamers x 2 builds).
- Two paces (Hyper Roll and Normal) plus the Knockout variant.

### Alliances

| Alliance | Underlords | Units | Effect |
|---|---|---|---|
| Shinobi | Assassin | 3/6 | leap to the back line; 12/22% crit for 220/280%, crits wound |
| Beast Kings | Brawny | 2/4 | +200/450 HP, plus 6/12 HP per knockout by your Beast Kings this game (x star) |
| Oni | Brute | 2/4 | attacks weaken (-30/50% damage, 4 s); 80/120 magic on apply |
| Legend | Champion | 1 | member of every active alliance; +3% damage and +8% HP per alliance |
| Demon Lords | Demon | 1 | a cast silences your other Demons 2.5 s and gives the caster +50% damage per living Demon for 4 s |
| Dragon's Roar | Dragon | 2 | +15 AP, abilities burn and splash 40% next to the target |
| Undead | Fallen | 3/6 | first fall heals allies within 2 (150) and gives +6% damage; at 6 each rises once at 40% |
| Holy Healers | Healer | 2/4 | +15/30% team healing |
| Nightmare Soldiers | Heartless | 2/4/6 | enemies -18/35/55 armor |
| Partners | Human | 2/4/6 | allies +2/4/7 mana per second |
| Hunters | Hunter | 3/6 | 35/45% chance of 1/2 extra attacks |
| Knights | Knight | 2/4/6 | -15/20/25% damage, doubled beside a Knight |
| Wizards | Mage | 3/6 | enemies take +20/40% magic damage |
| Sage | Magus | 1 | once at 50% HP: invulnerable but inactive for 3 s |
| Toxic | Poisoner | 3 | attacks add poison (4 dps and -4% attack speed per stack, 10 stacks, 6 s) |
| Rogues | Rogue | 2/4 | 17/32% evasion of basic attacks, +8/16 AD per evade |
| Nature Spirits | Savage | 2/4/6 | +4/6/9 AD per landed attack (members / + summons / everyone) |
| Deep Savers | Scaled | 2/4 | allies +30/55 MR |
| Shamans | Shaman | 2/4/6 | combat-start wolf / wildwing / black dragon |
| Legendary Spirits | Spirit | 3 | three charged Spirits strike together (130 x star each) and stun |
| Summoners | Summoner | 2/4 | summons +20/40% damage |
| Blademasters | Swordsman | 3/6 | +10/22% damage per adjacent enemy |
| Jungle Troopers | Troll | 2/4 | +25/50% AS (others +8/16%) |
| Wind Guardians | Vigilant | 2/4 | focus enemy casters, +40/50% on the next hit |
| Dark Area | Void | 3 | every ally: 25% chance on attack for 1% max HP true damage (0.5 s cooldown) |
| Metal Empire | Warrior | 3/6 | +45/100 armor |

Numbers are scaled to this engine (armor and MR are flat, roughly 6x Dota armor; mana pools are about 75% of Dota's) and then tuned in duels.

### Heroes and 3-star effects

Every hero ability is a hook-driven ability type (`ul_*` in `underlords-mech.js`) or a passive kit (`KIT`): Mana Break, Moon Glaives, Coup de Grace, Essence Shift, Feast, Counter Helix, Fervor, Precision Aura. All T1-T3 heroes have their Season One 3-star effect on the third form (for example Lich casts Chain Frost, Luna gains Eclipse, Pudge gets Flesh Heap, Shadow Shaman hexes, Bounty Hunter earns gold per knockout, Juggernaut shares 15% of his HP, Legion Commander's duel wins add permanent attack damage for the whole game). T4 and T5 heroes had none in the final data, so they have none here.

### Models

Every form uses an existing `models/index.json` key (`underlords-models.js`; `node tests/models.cjs underlords underlords_ko`: 0 gaps). Forms that had no rip were swapped for Digimon that do, keeping the role (for example Ember Spirit ends in EmperorGreymon, Void Spirit is the Lucemon line, Medusa the Lunamon line). Lich's rookie is Gammamon (ReArise); everything else is Cyber Sleuth family or an existing converted extra. No new rips were converted.

## Tamers (Underlords)

Picked at round 10 (offer kind `ul_tamer`): all four are offered, each with one of its two builds at random. The Tamer is a board unit (`tamer` lines): it does not use a team slot (`onTeamSize` +1 while it is on the board), cannot be sold and cannot hold items (the `noSell` and `noItems` line flags), has no mana and uses its abilities on cooldowns (first one at half cooldown). It ranks up every 5 rounds (rank 6 at round 35); the form changes at ranks 3 and 5, stats come from a per-rank table. A surviving Tamer deals 1 damage (2 from stage 7).

| Tamer | From | Core | Build A | Build B |
|---|---|---|---|---|
| Anessa (Lopmon line) | Anessix | Demon Archer companion, does not attack | Dark Support: team +40-55% AD for 6 s every 8 s; Enthrall marks an enemy, it rises for you if it falls | Healing Support: Rejuvenating Ritual heal now and over 6 s; Demonic Golem crashes by the farthest enemy and stuns |
| Hob (Candlemon line) | Hobgen | attacks burn | Fire Starter: Implosion 640-1560 split; Friendly Fire 8 bombs | Fire Support: Shockwave split + stun; Let's Go Crazy: board range, team +30-90% AS for 6 s |
| Jules (Bearmon line) | Jull | +10 armor/MR per adjacent unit | Aggressive Tank: Roundhouse (taunt, 50% DR, x2 AS, lifesteal); Barrels of Fun | Healing Tank: To Your Health; Happy Hour (half of each hit is taken over 10 s) |
| Enno (Chuumon line) | Enno | Escape Artist at 50% (and 25% from rank 4) | Healin' N' Stealin': poison darts that heal; Yoink! steals the stats of the enemy's lowest-tier item | Rabid Furball: Death Spin (damage per poison stack); All Out Attack (3 stacks, damage, 1.1 s stun to all) |

## Items

`items.list` with cat `tiered` (tier 1-5, held by a hero, one item per hero as in Underlords: the `canEquip` hook refuses a second item, and a hero that digivolves keeps one item and sends the rest to the item bench) and cat `global` (go to `player.globals`). Item rounds follow Season One: rounds 1, 4, 8, 12 ... 40, pick 1 of 3 with one reroll, and the tier is rolled per player from the Season One odds table (round 4: 20% T1 / 80% T2 ... round 36+: T5). The engine `itemTier` offer is regenerated in `onRoundStart` with the rolled tier.

- Hats (add an alliance): Crown of Antlers (Hunters), Leg Breaker's Fedora (Oni), Pirate Hat (Blademasters), Ristul Circlet (Demon Lords) at T3; Ogre Cap (Jungle Troopers), Mantle of the Flayed Twins (Nightmare Soldiers) at T4.
- Global items (beta): Embarrassment of Riches (T1, 4 choices), Silver Lining, Aegis of the Immortal, Summoning Stone (T2), Vicious Intent, Dawning of Ristul, Coordinated Assault, Age of Chivalry (T3), Recruiter, Smuggler (T4), Expanded Roster, A Higher Class of Criminal (T5).
- Divine Rapier changes hands when you lose to a player while it is on your board.

## Economy and rules

Normal is the native pace (the Underlords economy). Values chosen where the research conflicts:

| Rule | Value | Note |
|---|---|---|
| HP | 100 | |
| Start | level 3, 5 gold | |
| Income | 5 per round | CONFLICT: wiki 5, 2019-12-18 notes say 7; kept 5 |
| Interest | 1 per 10, max 3 | |
| PvP win | +1 gold | |
| Streaks | wins 3-4: 1, 5-6: 2, 7: 3, 8: 4 then reset; losses capped at 2 | |
| Free reroll | after a PvP loss | |
| Gold cap | 100 | |
| Reroll / XP | 2 gold / 5 gold for 5 XP, +1 XP a round | XP curve 1/1/3/5/10/20/25/35/50 (the cumulative 2019-10-24 curve; the wiki table differs) |
| Shop odds | final table, levels 1-10 | |
| Pool | 30 / 20 / 18 / 12 / 10 | |
| Damage | round damage (+1 per 10 rounds) + per surviving enemy floor(1 + (tier + 2 x star) / 3); Tamer 1 (2 late); summons 0 | |
| Rounds | 5 per stage; creeps at rounds 1, 2, 3 and every 5th from 10 (beta loot waves, Digimon creeps) | Season One had no creeps; Classic mode did |
| Tamer ranks | every 5 rounds from round 10, rank 6 at 35 | the corpus says "max at round 40" with 6 ranks; kept every 5 rounds |

Hyper pace (`rules.hyper`, over the engine's Underlords profile): 20 HP, level by stage (3-10), fixed income 5-10, no interest or streaks, 1-gold rerolls, flat loss damage 2/3/4/5 by stage, same rounds, items and Tamer schedule.

### Knockout (`underlords_ko`)

4 lives (1 per lost round), 2 copies digivolve, start at level 5 and level up every 2 rounds (no XP buying), a starter crew of 5 from two alliances at 1-1 (offer `ul_crew`), Tamer at 1-2 ranking up every 2 rounds, an item every odd round, base gold 6 rising by 1/2 a round, no interest or streaks, 2 rounds per stage. Both paces play the same Knockout rules.

## Balance

Tools: `node tools/balance.cjs --duel 12000 8 1 --set underlords` (alliances and lines, pace-independent) and a full-game script that runs 8-AI games in each pace with Tamers picked at random and compares global items picked vs passed in the same offers.

Duel, 12000 fights at 2-star: every alliance between 48.3% (Nature Spirits) and 57.3% (Shinobi); lines between 44.7% (Slark) and 55.9% (Treant).

Full games, 160 per pace, Tamers picked at random (average placement, 4.5 is even; one standard error is about 0.18):

| Build | Normal | Hyper |
|---|---|---|
| Anessa: Healing Support | 4.01 | 4.89 |
| Anessa: Dark Support | 4.47 | 4.55 |
| Hob: Fire Starter | 4.30 | 4.23 |
| Hob: Fire Support | 4.58 | 4.73 |
| Jules: Aggressive Tank | 4.60 | 4.36 |
| Jules: Healing Tank | 5.08 | 4.54 |
| Enno: Healin' N' Stealin' | 4.52 | 4.27 |
| Enno: Rabid Furball | 4.48 | 4.40 |

By Tamer: Normal 4.23 / 4.44 / 4.50 / 4.83 (Anessa, Hob, Enno, Jules), Hyper 4.72 / 4.48 / 4.34 / 4.46. The builds that lead in one pace trail in the other, so most of the remaining spread is sample noise; Happy Hour got one more second after these runs.

Global items, placement when picked minus placement when offered and passed (negative = better): all within -1.1 to +0.3, most within +-0.4; Summoning Stone was -0.96 in Hyper and was cut from +40% to +30%. Expanded Roster and A Higher Class of Criminal (T5) are taken often by the AI but show no placement advantage over passing them.

Game length: Normal median 36 rounds (29-47), Hyper median 24 (21-29).

One item per hero (2026-10-16, `node tools/balance.cjs 300 4 --set underlords`, before / after): alliance top-4 rates barely moved. Hyper: 29-93% before, 25-95% after (the ends are small samples: Jungle Troopers n=75, Wind Guardians n=138); the common alliances stay within 4 points except Demon Lords (64% to 54%). Normal: 21-84% before, 8-84% after (Metal Empire n=40 is the low end); game length 35.8 to 36.1 combat rounds.

## Engine notes

- Board moves for ability effects (hook, charge, blink, poof, escape) use the combat API's `c.move` and `c.lineTargets`.
- One item per hero comes from the `canEquip` game hook; the AI gives finished items to the most valuable heroes that can still take one.
- The Tamer keeps its seat through the `noSell` and `noItems` line flags; only the team size still needs a hook (`onTeamSize`).
