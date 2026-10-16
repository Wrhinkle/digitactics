# Dota Underlords reference (Valve, 2019-2020)

Design reference for Digi-Tactics. Covers the open beta (June 2019) through the last patch (2020-11-19), including The Big Update, Season One and City Crawl, and the August 2020 "New Blood On The Streets" rotation. Values come from game data files, Valve patch notes and the community wiki. Missing values are marked unknown; nothing is estimated.

## Sources

1. Dota Underlords Wiki, page source pulled through the MediaWiki API at dotaunderlords.fandom.com (same wiki as underlords.gamepedia.com; underlords.fandom.com returned HTTP 402/404). 944 pages: heroes, alliances, items, Underlords, mechanics, modes, patch pages. Retrieved 2026-10-15. https://dotaunderlords.fandom.com/wiki/Dota_Underlords_Wiki
2. Steam Community announcements for app 1046930, all 117 posts from 2019-06-26 to 2020-11-19, pulled through the ISteamNews API. Key posts: The Big Changelog (2019-10-24), The Outlanders Update (2019-11-26), Jull-tide Changelog (2019-12-18), Dota Underlords Season One (2020-02-25), A Top Tier Update (2020-05-28), New Blood On The Streets (2020-08-27), The Update In Which Spectre Takes a Dagger (2020-11-19, last patch). https://store.steampowered.com/news/app/1046930
3. SteamTracking/GameTracking-Underlords (GitHub), a git history of the game client. Data files `scripts/units.json`, `abilities.json`, `synergies.json`, `items.json` exist from 2019-06-14 until 2020-02-04, when they moved server-side. Snapshots used: be6ef27 (2019-06-20), 9923524 (2019-10-10), cc2226d (2019-10-24), 98a2dcc (2019-12-06). Localization (`dac_english.txt`, `dac_abilities_english.txt`, `items_english.txt`) and `scripts/events/*.json` up to the final build. https://github.com/SteamTracking/GameTracking-Underlords
4. Fortify-Labs/Fortify (GitHub), a community stats platform that shipped dumps of the server-side data: `s1_units.json` (Season One) and `s1_5_units.json`, `s1_5_synergies.json`, `s1_5_items.json` (values match the 2020-11-19 patch notes, so treated as final). https://github.com/Fortify-Labs/Fortify/tree/master/services/shared/src/assets
5. One Reddit developer answer (Spirit alliance targeting), quoted through the wiki: https://www.reddit.com/r/underlords/comments/ermj8f/the_developers_have_answered_1/
6. Press coverage used only to confirm the name of the 2019-10-24 update: Inven Global, "The Big Update has hit Dota Underlords" https://www.invenglobal.com/articles/9437/

Liquipedia was not used (its API requires gzip and terms acceptance, and it has little Underlords data). underlords.com was not fetched; its update pages mirror the Steam posts.

Priority when sources disagree: game data for stats and alliance numbers, then patch notes, then the wiki. The wiki lags in places (for example it lists Enno's health as 1300-2800; the final data says 1600-3100). Conflicts that could not be settled are marked CONFLICT.

## Files

- `underlords.md`: this overview (rules, economy, modes, Underlords, alliances, final roster).
- `underlords.json`: all data in one file (heroes at six snapshots, alliances, items, Underlords, rules).
- `underlords-heroes.md`: every hero with per-snapshot stats, abilities, 3-star and Ace effects, change logs.
- `underlords-items.md`: item mechanics and the item pool at each snapshot.
- `underlords-beta-season.md`: June 2019 to February 2020.
- `underlords-season-1.md`: February 2020 to November 2020.

## Conventions

- attack_rate_seconds: Seconds between attacks (lower is faster). The wiki labels this "Attack Speed".
- attack_range_cells: Range in board cells; 1 = melee.
- star_arrays: Arrays of 3 are 1-star/2-star/3-star values.
- underlord_arrays: Underlord arrays of 6 are ranks 1-6 (rank-up every 5 rounds, max at round 40) in the final data. Older Underlord data used 8 or 10 ranks.
- item_arrays_final: Several final-patch items carry 6-value arrays. The first value matches the standard in-match item. The other five are not documented; they are probably City Crawl prestige item levels (unconfirmed).
- tiers: Tier = shop cost in gold. Tier 5 was called "Ace" from 2019-08-30 to 2020-02-25.
- Snapshots: beta_launch = Open beta launch (2019-06-20); pre_big_update = Late beta, before The Big Update (2019-10-10); big_update = The Big Update (Underlords added) (2019-10-24); outlanders = After The Outlanders Update and Knockout (2019-12-06); season_one = Season One launch (City Crawl) (2020-02-25); final = Final patch (after New Blood On The Streets) (2020-11-19).

## Timeline

| Date | Update | What changed |
|---|---|---|
| 2019-06-13/20 | Closed then open beta | 60 heroes, 23 alliances, equipment + global + alliance items, creep loot rounds, 3/6/9 alliances for Assassin, Elusive, Warrior. |
| 2019-07-04 | Hot Dogs, Updates, and Fireworks | Streak gold reworked; win streak resets at 8. |
| 2019-07-12 | Proto Pass the Popcorn | Free Proto Pass (test battle pass). |
| 2019-07-18 | Mid-Season Gameplay Update | Large balance pass; alliance items moved to own category. |
| 2019-08-01/02 | Head to Head / August Update | Shared head-to-head combat, Casual vs Ranked queues, visible Rank Score, achievements, free reroll after a loss, alliance items disabled. |
| 2019-08-15 | Lords of White Spire | Elo for top rank. |
| 2019-08-30 | Ace Tier Heroes and Contraptions | Tier 5 becomes "Ace" with alliance-linked Ace effects; contraption items; 9-unit alliance levels removed. |
| 2019-10-24 | The Big Update (Valve's title; no source calls it the "Big Ugly Update") | Underlords Anessix and Hobgen with Hype and talents; Duos; Freestyle; Jail; new alliances Brute, Champion, Healer, Insect; Demon, Knight, Elusive and Shaman (hex) bonuses reworked; Demon Hunter removed; 12 new heroes (Valve's count); XP bought in steps of 5. |
| 2019-11-15 | We All Grow Together | Underlord rank decoupled from level; Hype reworked; Jail rules changed; round damage restored. |
| 2019-11-26 | The Outlanders Update | Spirit alliance (Earth, Ember, Storm, Void Spirit), Snapfire; Knockout mode; tutorial. |
| 2019-12-06 | We Ranked Up Our Knockout Game | Ranked Knockout. |
| 2019-12-18 | Jull-tide | Jull; Underlord pick moved to round 10 with two fighting styles; Hype, talents, Jail, global items and creep rounds removed; start at level 3 with 5 gold; HP of all units cut 25%; new player-damage formula; item every 5 rounds; first hero rotation (14 heroes out), after which no in-pool hero kept Elusive or the old Shaman alliance. |
| 2020-01-30 | Hey, it's Ennother Underlord | Enno; Poison status. |
| 2020-02-25 | Season One | Out of Early Access. City Crawl, Season One Battle Pass ($4.99), Hats, items every 4 rounds with one reroll, Ace effects removed (Tier 5), rotation: Scrappy and Inventor out; Summoner, Vigilant, Void in. |
| 2020-05-08 to 05-28 | 3-star effects; A Top Tier Update | Heroes gain extra effects at 3 stars; shop odds changed to favour 3-starring T1-T3. |
| 2020-04-30, 06-12 | Pool changes | Pool shrunk to 30/20/15/12/10, then T3 18. |
| 2020-08-27 | New Blood On The Streets | Classic mode; rank compression; new alliances Fallen, Shaman (summon), Swordsman, Rogue, Poisoner, Magus; 8 new heroes; 17 heroes and 6 alliances rotated out. |
| 2020-11-19 | The Update In Which Spectre Takes a Dagger | Last patch (balance only). |

## Economy and rules

### Gold

- Start: 5 (2019-12-18 onward; also on the Gold wiki page). Earlier value: unknown. [N: Jull-tide Changelog]
- Income per round: CONFLICT: the Gold wiki page says 5 per round; the 2019-12-18 changelog says "Round Gold increased from 5 to 7". No later note reverting it was found. Treat as unresolved.
- Interest: 1 gold per 10 gold held at the end of the preparation phase. Capped at 3 from 2019-12-18 ("rewarded at a maximum of 30 gold"). Earlier cap: unknown from the sources read. [W: Gold; N: 2019-12-18]
- Gold cap: 100 gold held. [W: Gold]
- PvP win: +1 gold for winning a round against a player; counts toward interest. [W: Gold]
- Duos streaks: A Duos player only keeps a win streak while the team takes no damage. [W: Duos]
- Other gold: Bounty Hunter 3-star effect: 1 gold per hero he kills (final). Beta global items: Silver Lining, Check the Bodies, Poaching Knife, Hand of Midas (item). [W: Gold; item pages]

Win streak gold (final):

| 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|---|---|---|---|---|---|---|
| 0 | 0 | 1 | 1 | 2 | 2 | 3 | 4 |

Streak ends at 8 wins or on a loss. 2019-07-04: changed from 3-4 wins 1g / 5-6 wins 2g / 7+ wins 3g to 3-4 1g / 5-6 2g / 7 3g / 8 4g, and streaks reset at 8 wins.

Loss streak gold (final):

| 1 | 2 | 3 | 4 | 5+ |
|---|---|---|---|---|
| 0 | 0 | 1 | 1 | 2 |

Loss streak counts rounds where you took damage; it ends when you beat a player. Loot (creep) rounds give no streak gold. 2019-07-04: changed from 3-4 losses 1g / 5-6 2g / 7+ 3g to 3-4 1g / 5+ 2g.

Sell refunds:

| Star | Tier | Gold invested | Refund |
|---|---|---|---|
| 1-star | 1 | 1 | 1 |
| 2-star | 1 | 3 | 3 |
| 3-star | 1 | 9 | 5 |
| 1-star | 2 | 2 | 2 |
| 2-star | 2 | 6 | 4 |
| 3-star | 2 | 18 | 6 |
| 1-star | 3 | 3 | 3 |
| 2-star | 3 | 9 | 5 |
| 3-star | 3 | 27 | 7 |
| 1-star | 4 | 4 | 4 |
| 2-star | 4 | 12 | 6 |
| 3-star | 4 | 36 | 8 |
| 1-star | 5 | 5 | 5 |
| 2-star | 5 | 15 | 7 |
| 3-star | 5 | 45 | 9 |

### Shop

- 5 slots; reroll costs 2 gold.
- Free reroll: After losing a round to a player, the next round has one free reroll; it does not stack (2019-08-02). Draws stopped granting it on 2019-12-28. [N]
- Blacklist: A manual reroll cannot show heroes that were in the previous shop. The automatic refresh at the start of a round can repeat unbought heroes (2019-08-23). [W: Shop]
- Roll procedure: For each slot: roll a tier from the level odds, then pick uniformly among all copies of that tier still in the pool. Example from the wiki: chance of Disruptor in a tier-4 slot with 3 Disruptors owned and 11 other tier-4 heroes at 12 copies = 9 / (11*12 + 9) = 6.4%. [W: Shop]
- Shop lock: unknown (no shop lock mechanic found in the sources read)
- Beta Ace bonus: From 2019-08-30 to 2020-02-25: with at least one level of an alliance active, when an Ace appears it is 15% more likely to be that alliance's Ace. [N: 2019-08-31]

Tier odds, Standard and Duos (final; Wiki template (after the 2020-05-28 "A Top Tier Update" change, whose patch-note table was an image). Columns are player level 1-10; rows are tiers.):

| Tier \ Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| T1 | 80 | 70 | 55 | 45 | 35 | 25 | 20 | 18 | 15 | 12 |
| T2 | 20 | 30 | 35 | 40 | 40 | 35 | 30 | 24 | 21 | 18 |
| T3 |  |  | 10 | 15 | 25 | 35 | 40 | 35 | 30 | 28 |
| T4 |  |  |  |  |  | 5 | 10 | 20 | 28 | 32 |
| T5 |  |  |  |  |  |  |  | 3 | 6 | 10 |

Tier odds, Knockout (levels 5-10):

| Tier \ Level | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|
| T1 | 40 | 35 | 25 | 22 | 22 | 15 |
| T2 | 35 | 30 | 30 | 27 | 25 | 21 |
| T3 | 25 | 30 | 35 | 35 | 30 | 28 |
| T4 |  | 5 | 10 | 15 | 20 | 30 |
| T5 |  |  |  | 1 | 3 | 6 |

Tier odds as set on 2019-08-30 (Ace era):

| Tier \ Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 10+ |
|---|---|---|---|---|---|---|---|---|---|---|---|
| T1 | 100 | 70 | 60 | 50 | 40 | 30 | 25 | 22 | 20 | 15 | 15 |
| T2 |  | 30 | 35 | 35 | 35 | 30 | 30 | 27 | 23 | 21 | 20 |
| T3 |  |  | 5 | 15 | 25 | 30 | 30 | 30 | 29 | 28 | 25 |
| T4 |  |  |  |  |  | 10 | 15 | 20 | 25 | 30 | 30 |
| ace |  |  |  |  |  |  |  | 1 | 3 | 6 | 10 |

"10+" = level 10 with the global item A Higher Class of Criminal. On 2019-09-12 tier-4 odds were cut by 5 points at levels 6-8 (exact redistribution not stated).

### Hero pool

| Period | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| final_from_2020-06-12 | 30 | 20 | 18 | 12 | 10 |
| 2020-04-30_to_2020-06-12 | 30 | 20 | 15 | 12 | 10 |
| before_2020-04-30 | 45 | 30 | 25 | 20 | 10 |

Values as quoted in the 2020-04-30 patch note ("was"). Tier 4 had been raised to 20 on 2019-08-23; its earlier value is unknown. Duos: Pool size doubled. Sold heroes and all heroes of an eliminated player return to the pool.

Highest possible copies per match (final pool, Standard): T1 three 3-stars or ten 2-stars; T2 and T3 two 3-stars or six 2-stars; T4 one 3-star or four 2-stars; T5 one 3-star or three 2-stars.

### XP and levels

- 1 XP per round. [W: Player Level]
- 5 gold for 5 XP from 2019-10-24 (before: 4-XP increments). [N: The Big Changelog]
- Start: Level 3 from 2019-12-18 ("matches are now jump-started with all players at Level 3"). Earlier: level 1. [N: Jull-tide]
- Unit cap: Equals player level (minimum 3 after 2019-12-18). Contraptions do not count against the cap (2019-08-31). Global item Expanded Roster gave +1 (removed 2019-12-18). [W: Player Level; N: 2019-08-31, 2019-12-18]
- Knockout: Start at level 5 with 1 XP, +1 XP per round, 2 XP per level, XP cannot be bought: level-up every 2 rounds. [W: Knockout]

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| Cumulative XP from 2019-10-24 | 0 | 1 | 2 | 5 | 10 | 20 | 40 | 65 | 100 | 150 |
| Cumulative XP before 2019-10-24 | 0 | 1 | 2 | 4 | 8 | 16 | 32 | 56 | 88 | 128 |

Wiki table (XP needed to leave each level): 1->2: 1, 2->3: 1, 3->4: 5, 4->5: 15, 5->6: 20, 6->7: 25, 7->8: 35, 8->9: 40, 9->10: 45. The wiki Player Level table (date not given) differs from the 2019-10-24 curve after level 3. The 2020-05-28 patch says "Fixed the XP table in the Rulebook not matching the game" without numbers. Which curve was live at the end is unknown.

Duos: shared level, 2 XP per round; XP to reach each level: L2 2, L3 2, L4 10, L5 30, L6 40, L7 50, L8 70, L9 80, L10 100.

### Combining

- Standard/Duos: 3 copies of a 1-star make a 2-star; 3 copies of a 2-star make a 3-star (9 total). [W: Heroes Pool table]
- Knockout: 2 copies combine: 2x 1-star = 2-star, 2x 2-star = 3-star (4 total). [W: Knockout]

### Round structure

- Preparation (shop, positioning) then combat. Exact timer lengths: unknown.
- Beta creep rounds: Until 2019-12-18 neutral "loot rounds" replaced PvP at rounds 1, 2, 3, 10, 15, 20, 25, 30, 35, 40, 45, 50. Each dropped a choice of items. Losing gave the previous wave's item tier (2019-07-19). Removed 2019-12-18; returned only in Classic mode (2020-08-27). [W: Loot Rounds; N]
- Items: 2019-12-18: Items every 5 rounds during normal PvP rounds; quality does not depend on win/loss. 2020-02-25: Standard: rounds 1, 4, 8, 12, 16, 20... (every 4). Each pick shows 3 items; one reroll per pick, the 3 shown are excluded from the reroll. Knockout: Item pick on odd rounds and on the starter-crew round.
- Underlord pick: Standard/Duos: round 10 (from 2019-12-18; before that, picked before the match from 2019-10-24). Knockout: round 2 / end of round 1. [N: Jull-tide; W: Underlords, Knockout]
- Underlord rank: Rank 1 at pick, +1 rank every 5 rounds, max rank 6 at round 40 (final). Knockout: rank-up every 2 rounds. [W: Underlords, Knockout]
- Head to head: From 2019-08-02 players are paired and fight in one shared combat on one board. With an odd number of players alive, one player fights a clone of a real player. [N: August Update (#213), 2019-08-02]
- Length: About 30 minutes for Standard after 2019-12-18; Knockout about 10 minutes. [N]

### Player damage

- Health: 100 (stated on the Duos wiki page: "Similar to Standard mode, teams start with 100 health"). [W: Duos]
- Loser takes: sum over surviving enemy heroes of floor(1 + power/3), plus Underlord damage, plus round damage. Power = tier (1-star), tier+2 (2-star), tier+4 (3-star). Summons deal 0. Surviving Underlord: 1 before round 30, 2 from round 30. Round damage starts at 0 and rises by 1 every 10 rounds. [W: Damage; N: 2019-12-18]

| Tier | 1-star | 2-star | 3-star |
|---|---|---|---|
| T1 | 1 | 2 | 2 |
| T2 | 1 | 2 | 3 |
| T3 | 2 | 2 | 3 |
| T4 | 2 | 3 | 3 |
| T5 | 2 | 3 | 4 |

- 2019-08-02: round damage removed for rounds 4-9; starts at round 11 at 2, then 3 from round 20, 4 from round 30.
- 2019-11-15: "Round Damage is back, at 1 per every 10 rounds" (1 at 10-19, 2 at 20-29).
- 2019-12-18: unit damage changed to 1 + (Power Level / 3); higher-tier units deal more.
- Before 2019-12-18 the per-unit damage rule is unknown from the sources read (Vicious Intent, a beta global item, added +1 per surviving unit).
- Duos: If both partners win, their damage is summed against the losing team. If one player of each team wins, only the difference is applied to the team that dealt less. Equal damage = no loss. [W: Duos]
- Knockout: Players start with 4 lives and lose 1 per lost round. [W: Knockout]

### Beta systems that were removed

- **Jail**: The Big Update (2019-10-24) to 2019-12-18. Every 24 hours a set of heroes was barred from the pool. From 2019-11-15: 8-12 heroes per day, up to 3 from one alliance, one Ace per day, and tiers 1-4 balanced to within 1 ban of each other. Removed 2019-12-18 ("Jailbreak"). [N; W: Jail]
- **Hype**: 2019-10-24 to 2019-12-18. Underlords spent no resource but needed Hype above a threshold to cast. Passive Hype per second plus per-condition gains (Anessix +1/s per allied Demon, Hobgen +1/s per burning unit), +10 per kill, -5/s while stunned or hexed, double gain after the enemy Underlord dies. 2019-11-15: Hype instead scaled Underlord damage (up to +50%) and cooldown speed (up to 40% faster). [N: The Big Changelog, 2019-11-15]
- **Underlord talents**: 2019-10-24 to 2019-12-18. One talent picked every 5 rounds from round 10 to 35; round 20 pick was the ultimate (cast once per fight). Anessix and Hobgen had 22 talents each, 11 locked until unlocked by playing. Knockout allowed only the ultimate pick. [N; W: Anessix/Old Abilities, Hobgen/Old Abilities]
- **Global items**: Items that applied a player-wide rule rather than sitting on a hero (Recruiter, Expanded Roster, Friends and Family Discount, A Higher Class of Criminal, Aegis, Silver Lining, Smuggler...). Removed 2019-12-18. [N: Jull-tide Changelog]
- **Alliance items**: Global items tied to one alliance (Tooth and Claw, Coordinated Assault, Soul Sucking Syphon, Unstable Reactor, Font of Creation...). Moved to their own category 2019-07-19, disabled 2019-08-01; some effects were folded into alliances. [W: Items]
- **Ace tier**: 2019-08-30 to 2020-02-25. Tier 5 heroes were "Aces", each tied to one alliance, with an extra Ace effect while that alliance had at least one level active. Renamed Tier 5 and Ace effects removed at Season One. [N: 2019-08-31, 2020-02-25]

## Modes

- **Standard**: 8-player free-for-all, last player standing. Casual and Ranked queues from 2019-08-02. There was no separate mode called "Free-for-all"; Standard is the FFA mode.
- **Duos**: Added 2019-10-24. 8 teams of 2; shared health pool (100) and shared level; partners can send heroes and gold. Pool size doubled. Ranked Duos added with the 2019-12-18 leaderboard update. [N; W: Duos]
- **Freestyle**: Added 2019-10-24 as a sandbox: place any unit, item or Underlord, unlimited gold, change ranks and game speed, share boards by code. Later grouped under Training with the tutorial. [N; W: Training]
- **Knockout**: Added 2019-11-26 (casual), ranked from 2019-12-06. Pick 1 of 3 starter crews of 5 heroes from two alliances (one reroll); start at level 5 and level every 2 rounds; 2 copies combine; item every odd round; Underlord picked after round 1; 4 lives, lose 1 per loss; paired against players with similar health; no interest, no streak gold; base gold starts at 6 and rises (+1 on even rounds, +2 on odd rounds: 6, 7, 9, 10, 12 ... 19 at round 10). Average 10 minutes. [N: Outlanders Update; W: Knockout]
- **Classic**: Added 2020-08-27 as an option in Standard Casual and private matches: no Underlords, creep loot rounds return. [N: New Blood On The Streets; W: Classic Mode]
- **City Crawl**: Single-player campaign added with Season One (2020-02-25). Four districts (Little Revtel, New Tenebrous, Port Slipshade, The Warrens), each with 36 challenges: 13 Objectives (match tasks tied to a specific Underlord), 19 Street Fights (Knockout-style fights vs fixed boards or restricted bots; 2 lives, no timer, levels 6/8/10 by round, fixed base gold per fight, free reroll on loss) and 4 Puzzles (single round, limited units/cells/items and a net-worth cap; score = net worth, unit count, fight time, ranked on leaderboards). Clearing nodes opens paths. Street Fight item drops (14 per Underlord). City Keys skip a node. After clearing the city you can Prestige up to 5 times: Street Fights get harder (your units deal 25% less damage, +10% per prestige, up to 65%) and Street Fight items get stronger. Part of the content required the paid Battle Pass. [W: City Crawl]
- **Bot matches / Tutorial**: Bot games at several difficulties; "Hardcore (XP)" bots count for Battle Pass XP. Tutorial with Ricky Ravenhook added 2019-11-26 and auto-starts for players with fewer than 3 games. [N; W: Training]
- **Private lobbies**: Party/private matches; custom jails via board code were supported from 2019-10-31 until the jail was removed. [N: 2019-10-31]

## Ranked

- Ranks, low to high: Upstart, Grifter, Outlaw, Enforcer, Smuggler, Lieutenant, Boss, Big Boss, Lord of White Spire.
- Each rank below Lord has five minor levels I-V (green, blue, red, pink, gold).
- Visible Rank Score changes by finishing place only, not opponent rank. Bonus points when reaching a new minor rank. A major rank, once reached, cannot be lost. Ranked unlocks after 5 casual games. Casual queue added. [N: Ranked, 2019-07-26]
- Party rank changes by place among non-party players: party of 2: +88, +63, +33, +0, -33, -63, -88; party of 3: +75, +50, +15, -15, -50, -75. Place is counted only among non-party players. Parties up to 3; highest member rank is used for matchmaking.
- Solo rank changes and minor-rank thresholds: unknown.
- Reached at 7,600 Rank Score; on promotion the score is moved to about 15,000. From 2019-08-15 Lords use Elo with K=3 on scores x10; equal-score lobbies give +105/+75/+45/+15/-15/-45/-75/-105 by place. Players below Big Boss are ignored for a Lord's update. Leaderboard (underlords.com) updates hourly; must play 5 ranked games in 5 days to appear. [N: Lords of White Spire, 2019-08-14; W: Ranked]
- Ranked Knockout (2019-12-06) and Ranked Duos had their own leaderboards from 2019-12-18. [N]
- Reset: 2020-08-27: ranks above Grifter dropped one major rank; Grifter and lower reset to minor level I; Lords set to Big Boss I. [N: New Blood On The Streets]

## Battle passes

- **Proto Pass**: Free test battle pass from 2019-07-12 for all beta players. Match points by place: Standard 150/120/100/90/80/70/60/50, Knockout 100/80/60/50/40/30/20/10 (Knockout values added later). Daily challenges, one reroll each per 23 hours. Achievements also gave Proto Pass XP (2019-08-02). [W: Proto Pass; N]
- **Season One Battle Pass**: From 2020-02-25. Over 100 reward levels, 1,000 XP per level. Many rewards free; full access (all City Crawl content and all rewards) cost $4.99 and gave no gameplay advantage. Match XP by place: Standard/Duos 400/300/250/200/150/100/50/30, Knockout 150/120/100/80/60/40/20/10. Daily challenges worth 200/300/400 (kill units, earn end-of-turn gold, cast spells, deal magic damage, heal, lifesteal, summon). City Crawl tasks gave 300/600/1,000 XP. Rewards were cosmetics: boards, board props, wanted posters, Underlord outfits, win-streak effects, sprays, and Street Fight items. [N: Season One; W: Season One Battle Pass]
- **Season 2**: No Season 2 launched. Valve's last content update was New Blood On The Streets (2020-08-27), a rotation within Season One; the last patch was 2020-11-19. The client file named season_02_battle_pass.json is registered in events.json as "Season 1, EventID:2".

## Underlords

Underlords were player avatars that stood on the board and fought. They were added on 2019-10-24 (Anessix, Hobgen), with Jull on 2019-12-18 and Enno on 2020-01-30. Six more names (Grek, O'nia, Quiet, Turath, Wren, Carl) appear in lore text but were never playable.

How the system changed:

1. 2019-10-24 to 2019-11-15: pick an Underlord before the match. Abilities required Hype; each Underlord generated Hype passively and from its own condition (Anessix: allied Demons; Hobgen: burning enemies), +10 per kill, -5/s while stunned or hexed, doubled once the enemy Underlord died. Underlord rank followed player level (10 ranks). A talent was picked every 5 rounds from round 10 to 35; the round-20 pick was the ultimate.
2. 2019-11-15: rank decoupled from player level and grew on a fixed schedule (max equal to old rank 8). Abilities fired on cooldown; Hype now boosted damage (up to +50%) and cooldown speed (up to 40%). Ultimates cast once per fight.
3. 2019-12-18 (Jull update): Hype and talents removed. Each Underlord got two "fighting styles" (builds), each a core ability plus an ultimate. At round 10 (round 2 in Knockout) you choose among the Underlords, each offered with one of its two builds at random. Core abilities start each fight at 50% cooldown.
4. Final form: no mana; abilities start on cooldown each fight. Rank 1 at pick, +1 rank every 5 rounds, rank 6 at round 40. A surviving Underlord deals 1 damage to the loser (2 from round 30). Underlords cannot hold items.
5. Classic mode (2020-08-27) removed Underlords entirely.

### Anessix, Mistress of Secrets
Final stats by rank 1-6 (game data): HP 1500/1875/2250/2625/3000/3375; armor 2/3/3/3/4/4; damage none (does not attack); attack rate unset s; range 0; magic resist 0 or unset; HP regen 20; move speed 275.

**Core**
- Passive Ability: Anessix summons a companion to fight at her side.
  - The summoned companion has the Demon alliance.
  - Anessix always tries spawning her minion towards the closest enemy - which can be a diagonal - if that space is taken, she will always summon behind herself.
  - If Anessix dies, the summoned companion will automatically die as well.
**Build: Damage Support**
- The End of Medicine (cooldown 8.0): All allied units gain 25-35% increased attack damage.
- Enthrall (cooldown 10.0): Anessix marks a target for 6.0 seconds, reducing the target's armor by 10 and forcing your army to attack it. If the target dies, Anessix brings them back as a Demon to fight for her.
  - Any unit that must move more than 2 cells to attack the enthralled unit will disregard the new target and keep its old target.
  - The revived unit has the Demon alliance in addition to its other alliances.
  - The revived unit has its abilities' and item's cooldowns refreshed and is fully capable of using them. Although the item is not displayed when selecting the unit, it still works normally.
  - Targets the closest enemy unit.
**Build: Healing Support**
- Rejuvenating Ritual (cooldown 8.0): Anessix takes damage equal to 5% of her max health. All allied units heal 61-180 hitpoints immediately, then 8-30 hitpoints a second for the next 6 seconds.
- Summon Demonic Golem (cooldown 15.0): Anessix summons a Demonic Golem, crashing behind enemy lines and stunning every enemy within 2 cells for 2 seconds.
  - The Golem crash lands next to an enemy unit, stunning all enemies within 2 cells for 2 seconds.
  - Stuns nearby enemies 0.25 seconds after spawning.
  - Targets the farthest enemy unit on the field for the summoning location.
- Summon Demon Archer (wiki, ranks 1-6): HP 300/375/450/525/600/675; damage 42/48, 48/54, 54/60, 60/66, 66/72, 72/78; armor 3/4/5/6/7/8; attack rate 1.00; range 4; move 330.
- Summon Demonic Golem (wiki, ranks 1-6): HP 1875/2250/2625/3000/3375/3750; damage 210/230, 220/245, 230/260, 240/280, 250/290, 270/310; armor 0/0/0/0/0/0; attack rate 0.8; range 1; move 260.

**Before 2019-12-18 (Hype and talents era)**
- _Abilities_
  - Passive Ability: Anessix generates a base of 1.5 Hype Per Second (HPS), with an additonal 1 HPS for each allied demon on the board. She summons (and re-summons) a companion to fight at her side.
  - Martyr's Boon: Anessix takes damage equal to 20% of her max health. All allied units heal for 11 hitpoints a second for the next 8 seconds.
  - Pure Pain: Anessix targets a random enemy, dealing 15 pure damage every second, over 7 seconds.
- _Ultimates_
  - Enthrall: Anessix marks a target for her army for 5 seconds, forcing them to attack it. If the target dies, Annessix brings them back as a Demon to fight for her.
  - Summon Demonic Golem: Anessix calls in an unholy favor and summons a Demonic Golem.
- _Round 10_
  - Brill Bruiser [talent for Anessix Passive]: A brill bruiser demon spawns instead. The demon stands nearby, blocks ranged attacks, and its attacks have a chance to bash.
  - Chaotic Hunter [talent for Anessix Passive]: Anessix's companion gains a 25% chance of quickly performing 2 extra attacks.
  - Rejuvenating Ritual [talent for Martyr's Boon]: Martyr's Boon now delivers the first 50% of the heal immediately, and the rest over the duration of the spell.
  - The End Of Medicine [talent for Martyr's Boon]: Instead of healing, Martyr's Boon now grants 25% increased attack damage.
- _Round 15_
  - Broken Hearts, Broken Bones [talent for Pure Pain]: The target is BROKEN while affected by Pure Pain.
  - Sharp Pain [talent for Pure Pain]: Pure pain's damage is increased by 50%.
  - Despite All My Rage [talent for Anessix Passive]: Whenever Anessix's companion is killed, they come back angrier. Companion gains 150% attack damage every time they resurrect.
  - Confidence Boost [talent for Martyr's Boon]: Anessix gains +40% Hype generation while affected by Martyr's Boon.
- _Round 25_
  - Exploit Weakness [talent for Anessix Passive]: Anessix's companion's attacks BREAK their targets for 3 seconds.
  - Transfusion [talent for Pure Pain]: Anessix and her companion heal for the same amount of damage done by Pure Pain.
  - Phantom Pain [talent for Pure Pain]: Your army gains 1 armor while an enemy unit is afflicted by Pure Pain.
  - Instant Regret [talent for Martyr's Boon]: Friendly units affected by Martyr's Boon BREAK their killer for 5 seconds.
- _Round 30_
  - A Demon's Revenge [talent for Anessix Passive]: Whenever Anessix's companion is killed, the killer takes 100 pure damage and is broken for 3 seconds.
  - Heavy Wounds [talent for Pure Pain]: Targets afflicted by Pure Pain cannot be healed.
  - Sympathy Pains [talent for Pure Pain]: Anessix now targets a second enemy, dealing 50% of Pure Pain's damage to it.
  - Small Wound [talent for Martyr's Boon]: Anessix no longer sacrifices health when casting Martyr's Boon.
- _Round 35_
  - Fully Refreshed [talent for Enthrall]: When a target is captured via Enthrall, they come back with 100% mana.
  - Bring It Down! [talent for Enthrall]: The target is BROKEN while targeted by Anessix's Army.
  - Crash [talent for Summon Demonic Golem]: The Golem crashes down, stunning every enemy within 2 cells for 2 seconds.
  - Absolute Unit [talent for Summon Demonic Golem]: The Golem's health is increased by 100% and will taunt his enemy into attacking the Golem.

### Hobgen, Arson Aficionado
Final stats by rank 1-6 (game data): HP 1800/2325/2850/3375/3900/4425; armor 6/6/7/7/8/8; damage 98-104/110-116/122-128/134-140/146-152/158-164; attack rate 1.5 s; range 3; magic resist 0 or unset; HP regen 10; move speed 275.

**Core**
- Passive Ability: Hobgen's attacks light enemies on fire for 3 seconds.
**Build: High Damage Dealer**
- Implosion (cooldown 12.0, magical): Hobgen launches a fireball at a target that deals [700/900/1100/1300/1500/1700] damage split evenly amongst all targets adjacent and setting them all on fire.
  - Fire lasts 5.0 seconds.
- Friendly Fire (cooldown 13.0, pure): Hobgen's buddies throw 8 bombs in the battle field dealing [210/250/300/360/430/500] damage to enemies caught in the blast.
  - Each bomb has a radius of 1 cell.
  - Sets targets on fire for 5.0 seconds.
**Build: Support Damage Dealer**
- Shockwave (cooldown 12.0, magical): Hobgen launches a fireball at a target that deals [405/540/675/810/945/1080] damage split evenly amongst all targets adjacent, stunning them for 1.50 seconds and setting them all on fire.
  - Fire lasts 5.0 seconds.
- Let's Go Crazy (cooldown 10.0): Hobgen's attacks now reach the entire battlefield, and your entire crew gains [15/25/35/45/55/70/80/90] increased attack speed. Hobgen will continually switch targets to units that are not on fire.
  - Affect units spawned after Hobgen casts Let's Go Crazy, not just units alive at the time he casts.

**Before 2019-12-18 (Hype and talents era)**
- _Abilities_
  - Passive Ability: Hobgen generates 1.5 Hype Per Second (HPS), with an additional 1.0 HPS for each unit he's lit on fire. Any enemy Hobgen attacks is set on fire for 3 seconds.
  - Explosivo!: Hobgen launches a fireball at a target that deals 150 damage split evenly amongst all targets adjacent and setting them all on fire.
  - Super-Charged: Hobgen Super-Charges the closest friendly unit, setting them on fire and granting them 13 attack speed.
- _Ultimates_
  - Let's Go Crazy: Hobgen is Silenced, his attack range is now 8, and gains Super-Charged and all its upgrades. Hobgen will continually switch targets to units that are not on fire.
  - Friendly Fire: Hobgen's buddies throw 8 bombs in the battle field dealing 500 damage to enemies directly hit. Enemies caught in the explosion take half damage.
- _Round 10_
  - Burning Down The House [talent for Hobgen Passive]: Whenever Hobgen attacks a unit on fire, the fire will spread to a random adjacent enemy.
  - Pyrotechnics [talent for Hobgen Passive]: When an enemy dies while on fire, their mana is distributed across all your units.
  - Burn Out [talent for Super-Charged]: Super-Charged units have a 17% chance of stunning targets that are on fire for 1.25 seconds.
  - Fire Break [talent for Super-Charged]: Super-Charged units gain 40% damage reduction.
- _Round 15_
  - Grease Fire [talent for Explosivo!]: Hobgen gains 3 hype per unit hit in the blast.
  - Implosion [talent for Explosivo!]: Doubles the effect of Explosivo! on the main target.
  - Kaboom [talent for Super-Charged]: Whenever a Super-Charged unit dies, it explodes doing 20% of its max health in damage to adjacent enemies and setting them on fire for 5 seconds.
  - Hot Rod [talent for Hobgen Passive]: Hobgen's attack speed is increased by 15.
- _Round 25_
  - Afterburn [talent for Explosivo!]: The fireball sets the area it lands on fire for 3 seconds with a 20 damage per second effect on enemies standing in it.
  - Drain Bomb [talent for Explosivo!]: The explosion no longer damages and instead drains 100 points of Mana.
  - Feedback Loop [talent for Super-Charged]: Super-Charged units gain mana from damage caused by abilities. Upon casting, they deal damage equal to the mana cost to adjacent enemies.
  - Molten Armor [talent for Hobgen Passive]: Hobgen gains 1 armor and sets adjacent attackers on fire.
- _Round 30_
  - Shockwave [talent for Explosivo!]: Enemies hit are stunned for 1 second and pushed away from the main target.
  - Spark [talent for Super-Charged]: Whenever a unit is Super-Charged, its cooldowns are reset.
  - Keep It Secret [talent for Hobgen Passive]: Whenever an enemy is set on fire, they are silenced for 1.5 seconds. This effect can happen again every time the fire expires.
  - Melting Point [talent for Hobgen Passive]: Units on fire lose 1 armor a second until the fire is extinguished.
- _Round 35_
  - Ya Burnt! [talent for Let's Go Crazy]: While under the effects of Let's Go Crazy, Hobgen doubles the damage of rate of all burning units.
  - Fire Suppression [talent for Let's Go Crazy]: Enemy units that are on fire have their healing reduced by 50%.
  - Sticky Strike [talent for Friendly Fire]: The bombs add a permanent damage over time effect that deals 5% of the unit's current health.
  - Danger Close [talent for Friendly Fire]: The bombs will only land on the enemy's side of the board.

### Jull, Icewrack Bootlegger
Final stats by rank 1-6 (game data): HP 1900/2450/2750/3250/3550/4100; armor 8/8/12/12/16/16; damage 140-170/150-180/160-190/170-200/180-210/190-220; attack rate 2 s; range 1; magic resist 15; HP regen 20/30/30/30/40/40; move speed 275.

**Core**
- Passive Ability: Jull gains 5 armor per adjacent unit, friendly or enemy.
**Build: Aggressive Tank**
- Roundhouse (cooldown 10.0): Jull gains 50% damage reduction, 200 attack speed, and 50% lifesteal for 5 seconds.
- Barrels of Fun (cooldown 12.0): Jull calls in barrels from the edges of the board. If they hit an ally, they gain 50 attack speed. If they hit an enemy, they take 20% more magical and physical damage for the rest of the fight.
  - Ignores Spiderlings
**Build: Healing Tank**
- To Your Health (cooldown 10.0): Jull gains 50% damage reduction for 5 seconds. Adjacent allies are healed for 100% of damage taken by Jull during this time.
- Happy Hour (cooldown 10.0): Jull buys a round for all members of your army. When taking damage, 50% of it is received immediately, and the rest is taken over 10 seconds.

### Enno, Scavenger King
Final stats by rank 1-6 (game data): HP 1600/1900/2200/2500/2800/3100; armor 5; damage 90-115/100-125/110-135/120-145/130-155/140-165; attack rate 0.7 s; range 2; magic resist 0 or unset; HP regen -; move speed 300.

**Core**
- Escape Artist (cooldown 8.0): The first time Enno drops below 25/50/75% health, he escapes in a cloud of smoke, purging all negative effects on him and reappearing elsewhere on the board. Enemies adjacent to Enno when he lands are stunned for 1.5 seconds.
**Build: Healin' N' Stealin'!**
- Dr. Enno's Soothing Balm (cooldown 3.0): Enno leaps into the air and shoots a poison dart at up to [3/4/4/5/5/5] enemies that are within 3 cells applying a stack of Poison. Each time Enno hits an enemy with a poison dart, Dr. Enno triages and heals all allies, healing the most injured one first for [30/30/45/45/60/60] and successively halving the amount on each additional ally.
- Yoink!: Enno steals the lowest tier item from the enemy crew at the start of combat. Whenever an enemy dies, Enno loots them if they have a better item equipped. All items return to their original owners at the end of combat.
  - The following items cannot be stolen:
  - Aegis of the Immortal
  - Battle Fury
  - Divine Rapier
  - Mask of Madness
  - Octarine Essence
  - Refresher Orb
  - Void Stone
  - Hat items
**Build: Rabid Furball**
- Death Spin (cooldown 3.0, magical): Enno leaps into the air and shoots a poison dart at up to [3/4/4/5/5/5] enemies that are within 3 cells, applying a stack of Poison and dealing [25/25/35/35/45/45] physical damage per stack.
- All Out Attack! (cooldown 10.0, physical): Enno launches an All Out Attack against all enemies, applying 3 stacks of Poison, dealing 50-175 physical damage and stunning them for 1.5 seconds.

### Talent tables found in Season One client files

From scripts/events/season_02_underlords.json, added 2020-02-25. Jull and Enno entries are placeholder names ("Jull Talent 1"), and no patch note or wiki page records talents returning after 2019-12-18, so this looks like an unshipped structure.

- Anessix: rank-level 15: Demon Warrior, Demonic Boost, And Some Mana Too, The End of Medicine; rank-level 20: I'm About to Break, Be Quiet, Demonic Rage, Sweet Talk, Sweet Talk; rank-level 25: Enthrall Unlock, Summon Golem; rank-level 30: Demonic Break, Your Pain is My Gain, Did You See That?, Instant Regret; rank-level 35: A Demon's Revenge, Hype Builder, Pain Again, Small Wound; rank-level 40: Enthrall Longer, Bring It Down!, Golem Stun, Absolute Unit
- Jull: rank-level 15: Jull Talent 1, Jull Talent 2, Jull Talent 12, Jull Talent 13; rank-level 20: Jull Talent 3, Jull Talent 6, Jull Talent 7, Jull Talent 14; rank-level 25: Jull Talent 17, Jull Talent 18; rank-level 30: Jull Talent 4, Jull Talent 8, Jull Talent 9, Jull Talent 15; rank-level 35: Jull Talent 5, Jull Talent 10, Jull Talent 11, Jull Talent 16; rank-level 40: Jull Talent 19, Jull Talent 20, Jull Talent 21, Jull Talent 22
- Enno: rank-level 15: Enno Talent 1, Enno Talent 2, Enno Talent 12, Enno Talent 13; rank-level 20: Enno Talent 3, Enno Talent 6, Enno Talent 7, Enno Talent 14; rank-level 25: Enno Talent 17, Enno Talent 18; rank-level 30: Enno Talent 4, Enno Talent 8, Enno Talent 9, Enno Talent 15; rank-level 35: Enno Talent 5, Enno Talent 10, Enno Talent 11, Enno Talent 16; rank-level 40: Enno Talent 19, Enno Talent 20, Enno Talent 21, Enno Talent 22
- Hobgen: rank-level 15: Burning Down The House, Pyrotechnics, Supercharge Target Fire, Supercharge Damage Reduce; rank-level 20: Grease Fire, Implosion, Supercharge Explode, Hot Rod; rank-level 25: Let's Go Crazy Unlock, Friendly Fire Unlock; rank-level 30: Afterburn, Manabomb, Supercharge Ability Mana, Molten Armor; rank-level 35: Shockwave, Supercharge Reset Cooldowns, Keep It Secret, Melting Point; rank-level 40: Let's Get Nuts, Fire Suppression, Air Strike DoT, Precision Strikes

## Alliances (final, 2020-11-19)

Units needed and effect text are rendered from the final synergy data and localization. Hero lists count only heroes in the final pool. Out-of-rotation and removed alliances follow, then the neutral-only Mega alliances.

| Alliance | Units needed | # heroes | Heroes | Effect by level (units) |
|---|---|---|---|---|
| Assassin | 3/6 | 7 | Bounty Hunter, Ember Spirit, Faceless Void, Phantom Assassin, Queen of Pain, Slark, Templar Assassin | (3) All Assassins gain a 15% chance to Critical Hit for 325% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 75% for 3 seconds. / (6) All Assassins gain a 25% chance to Critical Hit for 400% Damage. On a crit, the target is WOUNDED and healing taken is reduced by 100% for 3 seconds. |
| Brawny | 2/4 | 5 | Axe, Beastmaster, Bristleback, Juggernaut, Snapfire | (2) All Brawny units gain +5 maximum health for each kill by any of your team Brawny units throughout the game. Bonus is multiplied by star level of Brawny unit. Summons count at 0.25 kill. (HP bonus varies by game mode.) / (4) All Brawny units gain +10 maximum health for each kill by any of your team Brawny units throughout the game. Bonus is multiplied by star level of Brawny unit. Summons count at 0.25 kill. (HP bonus varies by game mode.) |
| Brute | 2/4 | 5 | Alchemist, Axe, Doom, Lifestealer, Spirit Breaker | (2) Whenever a Brute attacks an enemy it applies a -30% damage debuff for 4 seconds. When the debuff is initially applied it deals 80 damage. Brutes prefer to attack enemies that do not have the debuff. / (4) Whenever a Brute attacks an enemy it applies a -50% damage debuff for 4 seconds. When the debuff is initially applied it deals 120 damage. Brutes prefer to attack enemies that do not have the debuff. |
| Champion | 1 | 1 | Legion Commander | (1) Champions receive all Alliance bonuses. In addition, they gain 3% extra damage and 8% max health per active Alliance. |
| Demon | 1 | 6 | Chaos Knight, Doom, Queen of Pain, Shadow Demon, Spectre, Terrorblade | (1) Whenever a Demon casts a spell, for 4 seconds, allied Demons are silenced and the caster gains +50% bonus Pure damage for each allied Demon. |
| Dragon | 2 | 4 | Dragon Knight, Puck, Snapfire, Viper | (2) All Dragon units unlock an additional draconic ability. |
| Fallen | 3/6 | 6 | Abaddon, Death Prophet, Lich, Terrorblade, Vengeful Spirit, Wraith King | (3) When each Fallen unit first dies, allies within 2 cells are healed for 250 HP and gain +10% bonus damage to attacks and spells. A Fallen unit can revive 0 times per combat, resurrecting with 40% health. / (6) When each Fallen unit first dies, allies within 2 cells are healed for 250 HP and gain +10% bonus damage to attacks and spells. A Fallen unit can revive 1 times per combat, resurrecting with 40% health. |
| Healer | 2/4 | 4 | Dazzle, Enchantress, Omniknight, Treant Protector | (2) All friendly healing is amplified by 25%. / (4) All friendly healing is amplified by 45%. |
| Heartless | 2/4/6 | 6 | Death Prophet, Drow Ranger, Lifestealer, Pudge, Shadow Demon, Vengeful Spirit | (2) All Enemies lose 4 Armor. / (4) All Enemies lose 9 Armor. / (6) All Enemies lose 15 Armor. |
| Human | 2/4/6 | 8 | Crystal Maiden, Dragon Knight, Keeper of the Light, Kunkka, Legion Commander, Lina, Lycan, Omniknight | (2) Allies generate 4 mana per second. / (4) Allies generate 8 mana per second. / (6) Allies generate 15 mana per second. |
| Hunter | 3/6 | 7 | Anti-Mage, Beastmaster, Drow Ranger, Medusa, Mirana, Terrorblade, Windranger | (3) All Hunters have a 30% chance of quickly performing 2 attacks. / (6) All Hunters have a 38% chance of quickly performing 3 attacks. |
| Knight | 2/4/6 | 7 | Abaddon, Batrider, Chaos Knight, Dragon Knight, Luna, Omniknight, Sven | (2) Knight units take 15% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 15% less physical and magical damage. / (4) Knight units take 20% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 20% less physical and magical damage. / (6) Knight units take 25% less physical and magical damage. If attacked from the front, and within 1 cell of an allied Knight, Knight units take an additional 25% less physical and magical damage. |
| Mage | 3/6 | 7 | Crystal Maiden, Keeper of the Light, Lich, Lina, Puck, Rubick, Storm Spirit | (3) Enemies take 35% more Magic damage. / (6) Enemies take 70% more Magic damage. |
| Magus | 1 | 1 | Rubick | (1) Once per battle, when a Magus unit's health drops below 50%, the unit becomes invulnerable and unable to attack for 3 seconds. |
| Poisoner | 3 | 5 | Alchemist, Dazzle, Queen of Pain, Venomancer, Viper | (3) Attacks from Poisoners apply stacks of Poison. Enemies lose 7 Attack Speed per stack of Poison. |
| Rogue | 2/4 | 5 | Alchemist, Anti-Mage, Bounty Hunter, Meepo, Sven | (2) Allied Rogue units gain 20% evasion. Whenever a Rogue unit evades an attack, it gains +10 Attack Damage. / (4) Allied Rogue units gain 40% evasion. Whenever a Rogue unit evades an attack, it gains +20 Attack Damage. |
| Savage | 2/4/6 | 7 | Bristleback, Lone Druid, Lycan, Magnus, Pangolier, Spirit Breaker, Tusk | (2) Allied Savage units gain +5 Attack Damage each time they land an attack. / (4) Allied Savage units and all allied summons gain +8 Attack Damage each time they land an attack. / (6) All allied units gain +13 Attack Damage each time they land an attack. |
| Scaled | 2/4 | 5 | Medusa, Slardar, Slark, Tidehunter, Venomancer | (2) Allies gain +30% Magic Resistance / (4) Allies gain +55% Magic Resistance |
| Shaman | 2/4/6 | 6 | Beastmaster, Enchantress, Lone Druid, Magnus, Nature's Prophet, Treant Protector | (2) When combat starts, a Small Wolf is summoned to help. / (4) When combat starts, a Wildwing Hatchling is summoned to help. / (6) When combat starts, a Black Dragon is summoned to help. |
| Spirit | 3 | 4 | Earth Spirit, Ember Spirit, Storm Spirit, Void Spirit | (3) When a Spirit casts an ability they enter a charged state. When 3 Spirits are in a charged state they perform the forbidden technique of the triangular Delta Slam, each dealing 130 magic damage to all enemies caught inside and applying their elemental effects for 1.5 seconds. Damage and effect duration are multiplied by 1.5 at 2* and 2 at 3*. |
| Summoner | 2/4 | 6 | Lone Druid, Lycan, Meepo, Nature's Prophet, Shadow Shaman, Venomancer | (2) Allied summoned units gain +20% damage. / (4) Allied summoned units gain +40% damage. |
| Swordsman | 3/6 | 7 | Ember Spirit, Juggernaut, Kunkka, Pangolier, Phantom Assassin, Sven, Wraith King | (3) Each Swordsman unit gains +10% bonus damage for each enemy unit adjacent to it. / (6) Each Swordsman unit gains +25% bonus damage for each enemy unit adjacent to it. |
| Troll | 2/4 | 4 | Batrider, Dazzle, Shadow Shaman, Troll Warlord | (2) All Troll units gain +40 Attack Speed and other Allies gain +20 Attack Speed. / (4) All Troll units gain +70 Attack Speed and other Allies gain +40 Attack Speed. |
| Vigilant | 2/4 | 5 | Drow Ranger, Luna, Mirana, Templar Assassin, Windranger | (2) Whenever an enemy unit casts a spell, Allied Vigilant units target that enemy if it is within their range. The damage of the next attack against this target is increased by 40%. / (4) Whenever an enemy unit casts a spell, All allied units target that enemy if it is within their range. The damage of the next attack against this target is increased by 50%. |
| Void | 3 | 4 | Faceless Void, Spectre, Templar Assassin, Void Spirit | (3) All allied heroes have a 40% chance to deal an additional 4% of the target's Max Health as Pure Damage. This effect can only occur once every 0.5 seconds. |
| Warrior | 3/6 | 7 | Earth Spirit, Kunkka, Pudge, Slardar, Tidehunter, Troll Warlord, Tusk | (3) All Warriors gain +10 Armor. / (6) All Warriors gain +25 Armor. |

### Alliances not in the final pool

- **Blood-Bound** (in pool Beta launch 2019-06-20 to Season One 2020-02-25; units 2 per Outlanders 2019-12-06; last heroes: Bloodseeker, Ogre Magi, Warlock). Effect (wiki, last recorded state): When a Blood-Bound unit dies, all Blood-Bound units except Legion Commander gain +125 Attack Damage.
- **Brawny (beta version)** (in pool Beta launch 2019-06-20 to Beta launch 2019-06-20; units 2/4 per Beta launch 2019-06-20; last heroes: Axe, Beastmaster, Disruptor, Juggernaut). Effect (game data Beta launch 2019-06-20): All Brawny units have their maximum HP increased by 200 / All Brawny units have their maximum HP increased by 500
- **Deadeye** (in pool Beta launch 2019-06-20 to Season One 2020-02-25; units 2 per Outlanders 2019-12-06; last heroes: Bloodseeker). Effect (wiki, last recorded state): All Deadeye units focus their attacks on the lowest-health enemy. Melee Deadeye units gain phasing movement.
- **Demon Hunter** (in pool Beta launch 2019-06-20 to Pre-Big Update 2019-10-10; units 1/2 per Pre-Big Update 2019-10-10; last heroes: Anti-Mage, Terrorblade). Effect (wiki, last recorded state): Invalidate your opponent's Demon Alliance bonus. / Invalidate your opponent's Demon Alliance bonus. All Demon and Demon Hunter units gain +50% Pure Damage.
- **Druid** (in pool Beta launch 2019-06-20 to Season One 2020-02-25; units 2/4 per Outlanders 2019-12-06; last heroes: Io, Lone Druid, Magnus, Nature's Prophet, Treant Protector). Effect (wiki, last recorded state): At the beginning of combat, the lowest star ally Druid is upgraded a level. / At the beginning of combat, the 2 lowest star ally Druid are upgraded a level each.
- **Elusive** (in pool Beta launch 2019-06-20 to Outlanders 2019-12-06; units 3/6 per Outlanders 2019-12-06; last heroes: Anti-Mage, Luna, Mirana, Phantom Assassin, Puck, Templar Assassin, Weaver, Windranger). Effect (wiki, last recorded state): All Elusive units have a 10% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked. / All Elusive units have a 20% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked.
- **Insect** (in pool Big Update 2019-10-24 to Season One 2020-02-25; units 3 per Outlanders 2019-12-06; last heroes: Broodmother, Nyx Assassin, Sand King, Weaver). Effect (wiki, last recorded state): Spiderlings constantly swarm enemies from the edge of the board. They are untargetable units that deal low damage and die if they take any damage. Enemies bitten by any Spiderling have a 20% chance to miss their attacks for 0.5s. Spiderlings replenish on death to a maximum of 6. / Spiderlings constantly swarm enemies from the edge of the board. They are untargetable units that deal low damage and die if they take any damage. Enemies bitten by any Spiderling have a 40% chance to miss their attacks for 0.5s. Spiderlings replenish on death to a maximum of 12.
- **Inventor** (in pool Pre-Big Update 2019-10-10 to Outlanders 2019-12-06; units 2/4 per Outlanders 2019-12-06; last heroes: Clockwerk, Gyrocopter, Snapfire, Techies, Timbersaw, Tinker). Effect (wiki, last recorded state): Allied Inventors explode on death dealing 10% of their Max Health to all enemies 1 cell away. / Allied Inventors explode on death dealing 20% of their Max Health to all enemies 1 cell away.
- **Inventor (beta version)** (in pool Beta launch 2019-06-20 to Beta launch 2019-06-20; units 2/4 per Beta launch 2019-06-20; last heroes: Clockwerk, Gyrocopter, Techies, Timbersaw, Tinker). Effect (game data Beta launch 2019-06-20): All Inventors gain +15 HP Regeneration / All Inventors gain +40 HP Regeneration
- **Primordial** (in pool Pre-Big Update 2019-10-10 to Season One 2020-02-25; units 2/4/6 per Outlanders 2019-12-06; last heroes: Arc Warden, Enigma, Io, Morphling, Razor, Tiny). Effect (wiki, last recorded state): Allied Primordial units spawn a rank 1 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons. / Allied Primordial units spawn a rank 2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons.
- **Primordial (beta version)** (in pool Beta launch 2019-06-20 to Beta launch 2019-06-20; units 2/4 per Beta launch 2019-06-20; last heroes: Arc Warden, Enigma, Morphling, Razor, Tiny). Effect (game data Beta launch 2019-06-20): When hit, Primordial units have a 30% chance to disarm melee attackers for 4 seconds. / When hit, Allies have a 30% chance to disarm melee attackers for 4 seconds.
- **Savage (beta version)** (in pool Beta launch 2019-06-20 to Beta launch 2019-06-20; units 2/4/6 per Beta launch 2019-06-20; last heroes: Enchantress, Lone Druid, Lycan, Sand King, Tusk, Venomancer). Effect (game data Beta launch 2019-06-20): Allies gain +10% Attack Damage / Allies gain +25% Attack Damage / Allies gain +45% Attack Damage
- **Scrappy** (in pool Beta launch 2019-06-20 to Outlanders 2019-12-06; units 2/4/6 per Outlanders 2019-12-06; last heroes: Alchemist, Bounty Hunter, Clockwerk, Sniper, Techies, Timbersaw, Tinker, Weaver). Effect (wiki, last recorded state): A Random Scrappy is granted +7 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / All Scrappy units gain +7 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does. / Allies gain +7 Armor and +8 HP Regeneration. Armor and HP are doubled whenever you have fewer units alive than your opponent does.
- **Shaman (old, beta)** (in pool Beta launch 2019-06-20 to Outlanders 2019-12-06; units 2/4 per Outlanders 2019-12-06; last heroes: Arc Warden, Enigma, Magnus, Nature's Prophet, Shadow Shaman). Effect (wiki, last recorded state): Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 2 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle. / Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 4 cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle.
- **Warlock** (in pool Beta launch 2019-06-20 to Season One 2020-02-25; units 2/4/6 per Outlanders 2019-12-06; last heroes: Broodmother, Disruptor, Necrophos, Shadow Fiend, Warlock, Witch Doctor). Effect (wiki, last recorded state): Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2 seconds. When either linked hero deals damage, both units are healed 50% of the damage dealt. / Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 2.5 seconds. When either linked hero deals damage, both units are healed 60% of the damage dealt. / Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 3 seconds. When either linked hero deals damage, both units are healed 70% of the damage dealt.

### Mega alliances (neutral creeps only)

- **Mega-Assassin** (units 2): All Mega-Assassins gain a 25% chance to Critical Hit for 400% Damage
- **Mega-Blood-Bound** (units 2): When a Mega-Blood-Bound unit dies, it gives +1000% Attack Damage, and any existing Mega-Blood-Bound bonus it has, to a random friendly Mega-Blood-Bound unit.
- **Mega-Demon** (units 1): All Mega-Demon units gain +100% Pure Damage
- **Mega-Elusive** (units 2): All Mega-Elusive units gain +100% Evasion
- **Mega-Scaled** (units 1): Allies gain +75% Magic Resistance
- **Mega-Troll** (units 3): All Mega-Troll units gain +100 Attack Speed
- **Mega-Warlock** (units 2): Whenever a Mega-Warlock casts a spell, they form a link with the Ally who has the lowest health for 3 seconds. When either linked unit deals damage, both units are healed 200% of the damage dealt.
- **Mega-Warrior** (units 2): Allied Mega-Warriors gain +30 Armor

### Alliance change history (wiki)

- **Assassin**: April 30, 2020: Healing reduction on Wounded enemies changed from [50, 100] to [75, 100]; April 03, 2020: Critical Bonus changed from [300, 375] to [325, 400]; January 09, 2020: Critical Hit bonus damage changed from [300, 400] to [300, 375]; September 12, 2019: On a critical strike, Assassin's wound the target reducing healing done by [50%, 100%].; September 05, 2019: Assassin targeting and Blink Dagger targeting is now a bit more random between left and right when selecting the farthest target.; August 30, 2019: Now gives bonuses at 3/6 heroes, down from 3/6/9 heroes. / Assassin Level 1 (3 heroes): 300% crit damage chance increased to 15%, up from 10%. / Assassin Level 2 (6 heroes): 400% crit damage chance increased to 25%, up from 20%.; August 01, 2019: Level 3 Alliance (9 heroes) - In addition to the existing bonuses level 3 now also has "All Assassins also blind enemies 1 cell away from where they land for 2 seconds (50% miss chance and 50% less mana when attacked)"; July 19, 2019: Fixed boxed in Assassins not jumping toward their enemies. / Assassins that have an enemy in their attack range will attack instead of jumping.; June 17, 2019: Assassins are untargettable for one frame when landing after a jump.
- **Blood-Bound**: February 25, 2020: OLD: When a Blood-Bound unit dies, it gives +125% Attack Damage, in addition to any existing Blood-Bound bonus it has, to a random friendly Blood-Bound unit. / NEW: When a Blood-Bound unit dies, all Blood-Bound units gain +125 Attack Damage.; October 10, 2019: Changed to: When a Blood-bound unit dies, it gives +125% Attack Damage, in addition to any existing Blood-Bound bonus it has, to a random friendly Blood-Bound unit.; July 19, 2019: Blood-bound Alliance extra damage changed from +100% to +125%.
- **Brawny**: August 27, 2020: Multiplier to Brawny Bonus HP changed from 4x for a 3 Star hero to 3x. ( [1, 2, 4] changed to [1, 2, 3].); June 12, 2020: Changed Health bonus from [15, 30] per kill to vary by star level of the Brawny unit. 1 star Brawny units get [5, 10] bonus Health per kill; 2 star units get [10, 20] bonus Health per kill; and 3 star units get [20, 40] bonus Health per kill.; February 25, 2020: Summoned units now count as 0.25 of a kill for the purposes of the Brawny maximum health bonus.; October 24, 2019: New: All Brawny units gain [15/30] maximum HP for each kill by any of your team Brawny units throughout the game.; August 01, 2019: Changed from "All Brawny units have their Maximum HP increased by 200/500" to "All Brawny units gain +30/60 maximum HP for each kill they have."
- **Brawny (beta version)**: August 27, 2020: Multiplier to Brawny Bonus HP changed from 4x for a 3 Star hero to 3x. ( [1, 2, 4] changed to [1, 2, 3].); June 12, 2020: Changed Health bonus from [15, 30] per kill to vary by star level of the Brawny unit. 1 star Brawny units get [5, 10] bonus Health per kill; 2 star units get [10, 20] bonus Health per kill; and 3 star units get [20, 40] bonus Health per kill.; February 25, 2020: Summoned units now count as 0.25 of a kill for the purposes of the Brawny maximum health bonus.; October 24, 2019: New: All Brawny units gain [15/30] maximum HP for each kill by any of your team Brawny units throughout the game.; August 01, 2019: Changed from "All Brawny units have their Maximum HP increased by 200/500" to "All Brawny units gain +30/60 maximum HP for each kill they have."
- **Brute**: November 19, 2020: Damage Reduction changed from [30, 60] to [30, 50] / Damage on apply changed from [80, 150] to [80, 120]; December 02, 2019: Changed damage debuff from [30, 70] to [30, 60]. / Changed debuff duration from 3 to 4.; November 07, 2019: Debuff to enemy attack damage changed from [15, 40]% to [30, 70]%. / Damage on debuff being applied changed from [50, 100] to [80, 150].
- **Champion**: December 28, 2019: Damage bonus per active alliance changed from 8% to 3%. Health bonus remains 8%.; November 26, 2019: Lowered the health and damage bonus percent from 10 to 8%.; November 21, 2019: Now also grants 10% bonus damage and max health per Alliance affecting the Champion (excluding her own).
- **Deadeye**: February 25, 2020: Unit requirement changed from [ 2 ] to [ 1 ].; October 24, 2019: In addition to the standard Alliance bonus, Melee Deadeye units gain phasing movement.; September 19, 2019: Added a small health difference buffer to prevent Deadeye units constantly changing targets.
- **Demon**: December 05, 2019: Dead allied Demons and ally summoned Demons now count towards the damage bonus.; November 07, 2019: When a Demon casts, now only allied Demons are silenced.; October 24, 2019: New: Whenever a Demon casts a spell, for 4 seconds all Demons are silenced and the caster gains +50% bonus Pure damage for each allied Demon.
- **Demon Hunter**: October 24, 2019: Removed from the game. Demon Hunters have taken up other jobs on the streets of White Spire.; August 08, 2019: Level 2 Alliance (2 heroes) - changed from "Invalidate your opponent's Demon Alliance bonus. All Demon units gain +50% Pure Damage. All Demon Hunters gain +50% Pure Damage." to "Invalidate your opponent's Demon Alliance bonus. All Demon and Demon Hunter units gain +50% Pure Damage.". This means that Terrorblade no longer gets the double bonus.; August 02, 2019: Demon Hunter alliance: extra Pure Damage only applies at Level 2 of the alliance. Extra Pure Damage is now +50% for all Demon Hunters regardless of number of Demons on the board.; August 01, 2019: In addition to the existing bonuses both levels now also have "All Demon Hunters gain 30% Pure Damage for each unique allied Demon unit."
- **Dragon**: June 25, 2019: Now requires 2 units. / Now unlocks an additional ability for each member of the Alliance.
- **Druid**: June 14, 2019: Added a new effect for the Druid Alliance that upgrades the star rating on a Druid.
- **Elusive**: October 24, 2019: New: All Elusive units have a [10/20]% chance to gain +100% Evasion and +35 Attack Speed for 3 seconds when attacked.; August 30, 2019: Now gives bonuses at 3/6 heroes, down from 3/6/9 heroes. / Elusive Level 1 (3 heroes): Evasion increased to 25%, up from 20%. / Elusive Level 2 (6 heroes): Evasion increased to 50%, up from 45%.; August 08, 2019: Level 3 Alliance (9 heroes) - changed from "All units gain +75% evasion" to "All heroes gain +75% evasion".; August 01, 2019: Level 3 Alliance (9 heroes) - changed from "Allied Elusive units gain +75% evasion" to "All units gain +75% evasion."
- **Fallen**: September 04, 2020: Item cooldowns are no longer refreshed on revive. / HP Percent for reviving units changed from 50 to 40
- **Healer**: April 10, 2020: Heal Bonus Percent changed from [25, 50] to [25, 45]
- **Heartless**: September 04, 2020: Armor debuff changed from [4, 10, 15] to [4, 9, 15]; August 27, 2020: Armor Debuff changed from [4, 8, 15] to [4, 10, 15]; June 18, 2020: Armor Debuff changed from [4, 9, 16] to [4, 8, 15]; January 16, 2020: Armor debuff to enemies changed from [3, 8, 15] to [4, 9, 16].; November 21, 2019: Armor Reduction changed from [ -3, -10, -15 ] to [-3, -8, -15].; November 07, 2019: Heartless Alliance changed from [-5, -10, -15] to [-3, -10, -15].; July 19, 2019: Heartless Alliance changed from [-5, -10, -20] to [-5, -10, -15].
- **Human**: April 10, 2020: Mana Gain changed from [4, 7, 10] to [4, 8, 15]; October 24, 2019: New: Allies generate [4/7/10] mana per second.; June 27, 2019: Alliance Silence: Is now purgeable.
- **Hunter**: January 16, 2020: Extra Shot Chance changed from [25, 35] to [30, 38]; January 09, 2020: Bonus Attack Chance changed from [25, 40] to [25, 35]. / Number of Bonus Attacks changed from 2 to [2, 3].; August 08, 2019: Level 1 (3 heroes): Extra Attacks chance raised from 20% to 25%. / Level 2 (6 heroes): Extra Attacks chance raised from 35% to 40%.; June 17, 2019: Chance to trigger extra attack increased from 15%/25% to 20%/35%. / Reduced extra attacks from 2 to 1.
- **Insect**: April 23, 2020: Miss Chance changed from [20, 35] to [20, 40].; December 19, 2019: Changed from a 3-unit Alliance to a 2/4 unit Alliance. / Number of insects changed from 12 to [6, 12] / Insect debuff miss chance changed from 30% to [20, 35]%.; November 21, 2019: Spawn rate of spiderlings changed to 3 every 0.5 seconds. This means it'll take 2 seconds to spawn the initial batch of 12.
- **Inventor**: August 01, 2019: Changed from "Allied Inventors get 15/40 HP regen" to "Allied Inventors explode on death dealing 15/30% of their Max Health to all enemies 1 cell away"
- **Inventor (beta version)**: August 01, 2019: Changed from "Allied Inventors get 15/40 HP regen" to "Allied Inventors explode on death dealing 15/30% of their Max Health to all enemies 1 cell away"
- **Knight**: August 27, 2020: Base Physical and Magical damage reduction changed from [15, 19, 23] to [15, 20, 25]. / Bonus Physical and Magical damage reduction changed from [15, 19, 23] to [15, 20, 25].; April 30, 2020: Base physical/magical damage reduction changed from [15, 20, 25] to [15, 19, 23]. As always, this reduction is doubled if a Knight is standing next to a friendly Knight and is attacked from the front or side.; February 25, 2020: Physical and Magic damage reduction changed from [ 15, 22, 28 ]% to [ 15, 20, 25 ]%; January 16, 2020: Damage reduction and adjacency bonus changed from [15/20/25]% to [15/22/28]%; November 07, 2019: Fixed a bug where Knights would not get their bonus magical damage reduction when standing next to friendly knights.; October 24, 2019: Knight Alliance changed. / Old: Knight units take [15/20/25]% less physical and magical damage and an additional [15/20/25%] when standing 1 cell away from another Knight. / New: Knight units take [15/20/25]% less physical and magical damage. If attacked from the front, and within 1 cell of an allied knight, Knight units take an additional [15/20/25%] less physical and magical damage.; July 19, 2019: Knight Alliance damage reduction bonus changed from [ -15, -20, -30 ] to [ -15, -20, -25 ]; June 27, 2019: Default damage reduction changed: [ -15, -25, -35 ] -> [ -15, -20, -30 ]. / Adjacent to other knights damage reduction changed: [ -20, -25, -30 ] -> [ -15, -20, -25 ].; June 25, 2019: Now gives a constant 15/25/35% damage reduction and an additional 20/25/30% when next to another knight.; June 19, 2019: Knights will now try to stay in formation with other Knights.
- **Mage**: April 10, 2020: Extra Magic Damage changed from [35, 75] to [35, 70]; January 16, 2020: Extra Magic Damage changed from [25, 75] to [35, 75]; November 27, 2019: Extra Magic Damage changed from [40, 100] to [25, 75]
- **Primordial**: March 19, 2020: Will try to spawn Eidolons up to 2 cells away if nearby cells are occupied.; January 24, 2020: Eidolon: / Attack Rate changed from 0.667 to [0.667, 0.8] / Armor changed from [3, 5] to [3, 8]; December 18, 2019: Now a 2/4 alliance, the third level removed from the game.; November 27, 2019: Eidolon: / Armor changed from [5, 10, 10] to [3,5,8] / Maximum Health changed from [600, 1200, 2400] to [600, 1200, 1800]; November 21, 2019: Eidolon: / Armor changed from [5, 10, 15] to [5, 10, 10]; October 24, 2019: Primordial Alliance changed from 2/4 to 2/4/6 allowing for strong Rank 3 Eidolons.; August 01, 2019: Changed from "Enemies attacking Primordial/Allied units have a 25% chance to have their attack speed slowed by 50% each attack. This effect is applied at the start of the attack and lasts for the duration of the attack." to "Allied Primordial units spawn a rank 1/2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons."; July 24, 2019: Level 1: Enemies attacking Primordial units have a 25% chance to have their attack speed slowed by 50 each attack. This effect is applied at the start of the attack and lasts for the duration of the attack. / Level 2: Enemies attacking friendly units have a 25% chance to have their attack speed slowed by 50 each attack. This effect is applied at the start of the attack and lasts for the duration of the attack.; July 19, 2019: Primordial Alliance now also has a 10% chance to disarm ranged attackers for 4 seconds.
- **Primordial (beta version)**: March 19, 2020: Will try to spawn Eidolons up to 2 cells away if nearby cells are occupied.; January 24, 2020: Eidolon: / Attack Rate changed from 0.667 to [0.667, 0.8] / Armor changed from [3, 5] to [3, 8]; December 18, 2019: Now a 2/4 alliance, the third level removed from the game.; November 27, 2019: Eidolon: / Armor changed from [5, 10, 10] to [3,5,8] / Maximum Health changed from [600, 1200, 2400] to [600, 1200, 1800]; November 21, 2019: Eidolon: / Armor changed from [5, 10, 15] to [5, 10, 10]; October 24, 2019: Primordial Alliance changed from 2/4 to 2/4/6 allowing for strong Rank 3 Eidolons.; August 01, 2019: Changed from "Enemies attacking Primordial/Allied units have a 25% chance to have their attack speed slowed by 50% each attack. This effect is applied at the start of the attack and lasts for the duration of the attack." to "Allied Primordial units spawn a rank 1/2 Eidolon on death. Eidolons are ranged Primordial units that don't spawn Eidolons."; July 24, 2019: Level 1: Enemies attacking Primordial units have a 25% chance to have their attack speed slowed by 50 each attack. This effect is applied at the start of the attack and lasts for the duration of the attack. / Level 2: Enemies attacking friendly units have a 25% chance to have their attack speed slowed by 50 each attack. This effect is applied at the start of the attack and lasts for the duration of the attack.; July 19, 2019: Primordial Alliance now also has a 10% chance to disarm ranged attackers for 4 seconds.
- **Rogue**: November 19, 2020: Evasion Percent changed from [20, 30] to [20, 40]
- **Savage**: August 27, 2020: Bonus damage per attack chaned from [5, 8, 10] to [5, 8, 13]; April 03, 2020: Bonus damage per attack changed from [5, 7, 10] to [5, 8, 10]; March 05, 2020: The Savage alliance bonus now applies to [All allied Savage units, All allied Savage units and all allied summons of any alliance, All allied units] / Bonus Attack Damage per Attack changed from [5, 6, 10] to [5, 7, 10]; February 25, 2020: OLD: Allied Savage units gain +[ 5, 10, 20 ] Attack Damage each time they land an attack on an enemy or cast a damaging spell. / NEW: [ Allied Savage units, All allied heroes, All allied units ] gain +[ 6, 8, 10 ] Attack Damage each time they land an attack.; December 19, 2019: Bonus damage per attack increased from [4, 7, 15] to [5, 10, 20]; November 27, 2019: Bonus damage per attack increased from [3, 6, 12] to [4, 7, 15]; October 31, 2019: Bonus Attack Damage per attack decreased from [4, 8, 15] to [3, 6, 12]; October 24, 2019: Attack Damage gain decreased from [7/15/25] to [4/8/15].; October 10, 2019: Changed to: Allied Savage units gain +[4, 8, 15] Attack Damage each time they land an attack on an enemy or cast a damaging spell. / Tweaked Savage numbers to [7, 15, 25] from [4, 8, 15].; October 02, 2019: Changed to: Allied Savage units gain +[4, 8, 15] Attack Damage each time they land an attack on an enemy. This replaces the bleed effect that would be applied each time they landed an attack.; August 01, 2019: Changed from "Allies gain +10/25/45% Attack Damage" to "Allied Savage units apply a stackable 5/10/15 damage/s bleed on attack for 3s. Duration refreshes each attack."
- **Savage (beta version)**: August 27, 2020: Bonus damage per attack chaned from [5, 8, 10] to [5, 8, 13]; April 03, 2020: Bonus damage per attack changed from [5, 7, 10] to [5, 8, 10]; March 05, 2020: The Savage alliance bonus now applies to [All allied Savage units, All allied Savage units and all allied summons of any alliance, All allied units] / Bonus Attack Damage per Attack changed from [5, 6, 10] to [5, 7, 10]; February 25, 2020: OLD: Allied Savage units gain +[ 5, 10, 20 ] Attack Damage each time they land an attack on an enemy or cast a damaging spell. / NEW: [ Allied Savage units, All allied heroes, All allied units ] gain +[ 6, 8, 10 ] Attack Damage each time they land an attack.; December 19, 2019: Bonus damage per attack increased from [4, 7, 15] to [5, 10, 20]; November 27, 2019: Bonus damage per attack increased from [3, 6, 12] to [4, 7, 15]; October 31, 2019: Bonus Attack Damage per attack decreased from [4, 8, 15] to [3, 6, 12]; October 24, 2019: Attack Damage gain decreased from [7/15/25] to [4/8/15].; October 10, 2019: Changed to: Allied Savage units gain +[4, 8, 15] Attack Damage each time they land an attack on an enemy or cast a damaging spell. / Tweaked Savage numbers to [7, 15, 25] from [4, 8, 15].; October 02, 2019: Changed to: Allied Savage units gain +[4, 8, 15] Attack Damage each time they land an attack on an enemy. This replaces the bleed effect that would be applied each time they landed an attack.; August 01, 2019: Changed from "Allies gain +10/25/45% Attack Damage" to "Allied Savage units apply a stackable 5/10/15 damage/s bleed on attack for 3s. Duration refreshes each attack."
- **Scaled**: November 19, 2020: Magic Resistance changed from [20, 50] to [30, 55]; April 10, 2020: Magic Resistance changed from [20, 45] to [20, 50]; November 27, 2019: Magic Resistance changed from [30, 50] to [20, 45]
- **Scrappy**: January 09, 2020: Bonus Armor changed from 8/16 to 7/14. HP Regen unchanged at 8/16.; December 28, 2019: Armor bonus changed from 9 to 8 and disadvantaged armor bonus changed from 18 to 16.; July 19, 2019: Scrappy Alliance now takes 2 units per level (down from 3) : / Level 1 - A random scrappy unit is granted +9 Armor and +8 HP regeneration. / Level 2 - All scrappy units are granted +9 Armor and +8 HP regeneration. / Level 3 - All allies are granted +9 Armor and +8 HP regeneration. / [Armor and HP are doubled if you start the fight with less units than your opponent] is changed to [Armor and HP are doubled whenever you have fewer units alive than your opponent does].; June 25, 2019: Now provides 8 armor and 9 health regeneration. It provides double that amount if your starting army is smaller in size than your opponent's.
- **Shaman**: September 24, 2020: Fixed a bug where Alliance summons could do damage at the end of the round. / Regularized which Shaman unit summons the creature. The units are sorted by star level, then by draft tier, then by position (preferring backline heroes), and finally if there is a tie the hero bought (or combined if 2 or 3star) first wins.
- **Shaman (old, beta)**: October 24, 2019: New: Units attacking a Shaman have a 17% chance of being turned into a chicken and hop [2/4] cells in a random direction. Each shaman can only turn each enemy into a chicken once per battle.; September 02, 2019: Units now again have a 17% chance of being turned into a chicken when attacking a Shaman, up from 10%.; August 30, 2019: Now requires 2 heroes to activate, down from 3 heroes to activate. / Units now have a 10% chance of being turned into a chicken when attacking a Shaman, down from 17%.; August 01, 2019: Changed from "Enemies affected by Hexes, Silences or Stuns will generate -200% mana" to "Units attacking a Shaman have a 17% chance of being turned into a chicken and hop 4 cells in a random direction."; July 04, 2019: Now requires 3 Shamans (up from 2). / Now reads: Enemies affected by Hexes, Silences or Stuns will generate -200% mana when attacked. / Previous it read: A Random Enemy is turned into a Frog for 6 seconds at the Start of Combat.
- **Spirit**: September 04, 2020: The damage and effect duration a Spirit hero does during a Delta Slam now varies by their star level. / Base damage changed from 200 to 130 / Base effect duration changed from 3.0 to 1.5 / 2 star multiplier is 1.5x (damage 195, duration 2.25) / 3 star multiplier is 2x (damage 260, duration 3.0); May 14, 2020: Undocumented changes / Damage per Spirit during Delta Slam changed from 225 to 200; March 05, 2020: Spirit units no longer gain mana from damage they deal during a Delta Slam.; February 03, 2020: Fixed an issue where units not on the ground could be counted for Delta Slam targeting.; January 24, 2020: Delta Slam no longer considers Spiderlings when determining the triangle that damage the most enemies. / Fixed Delta Slam not killing Spiderlings.; January 16, 2020: Damage per Spirit during Delta Slam changed from 200 to 225
- **Summoner**: February 25, 2020: Added to the game.
- **Troll**: November 19, 2020: Attack Speed Bonus for Trolls changed from [35, 65] to [40, 70] / Attack Speed Bonus for other allies changed from [15, 30] to [20, 40]; August 27, 2020: Bonus Attack Speed to non-Troll allies changed from [10, 30] to [15, 30].; July 19, 2019: Troll Alliance Tier 1 now provides 10% attack speed bonus to ALL allies.; June 19, 2019: Units will now try to stay in formation with Trolls if the player has Coordinated Assault.
- **Vigilant**: April 03, 2020: Bonus damage on first hit changed from 50 to [40, 50]; March 05, 2020: Whenever an enemy unit casts a spell and is targeted by the Vigilant alliance, the first attack from each unit due to that targeting change does 50% more damage.
- **Void**: April 03, 2020: Percent of target max health as bonus pure damage changed from 5% to 4%; March 19, 2020: Added 0.5 second cooldown to damage proc. Now the extra pure damage can not proc more frequently than once every 0.5 seconds, regardless of how quickly the unit is attacking.; February 25, 2020: Added to the game.
- **Warlock**: December 28, 2019: Lifesteal changed from [50, 70, 90] to [50, 60, 70]. Link duration changed from [2, 3, 3.5] to [2, 2.5, 3]; November 27, 2019: Lifesteal Percent changed from [50, 70, 100] to [50, 70, 90] / Lifesteal Duration changed from 2 to [2, 3, 3.5]; September 12, 2019: Healing changed from [50%, 80%, 130%] -> [50%, 70%, 100%].; August 01, 2019: Healing Link duration changed from 3 seconds to 2 seconds; July 19, 2019: Alliance lifesteal changed from [50%, 100%, 150%] to [50%, 80%, 130%] / Alliance now targets the unit with the lowest health % not lowest health value.; July 12, 2019: Warlock alliance no longer triggers on item activate, just spell activate.; July 04, 2019: Now reads: Whenever a Warlock casts a spell, they form a link with the Ally who has the lowest health for 3 seconds. When either linked hero deals damage, both units are healed [50, 100, 150]% of the damage dealt.; June 19, 2019: Units will now try to stay in formation with Warlocks if the player has Soul Sucking Syphon.
- **Warrior**: September 04, 2020: Bonus armor changed from [10, 20] to [10, 25]; August 27, 2020: Bonus Armor changed from [9, 19] to [10, 20]; January 09, 2020: Armor bonus changed from [9, 18] to [9, 19]; December 28, 2019: Armor bonus changed from [7, 17] to [9, 18]; November 21, 2019: * Armor buff for allied warriors changed from [7,20] to [7, 17].; November 07, 2019: Armor buff for allied warriors changed from [10, 20] to [7, 20].; August 30, 2019: Now gives bonuses at 3/6 heroes, down from 3/6/9 heroes. / Warrior Level 1 (3 heroes): no changes (Armor bonus is +10). / Warrior Level 2 (6 heroes): Armor increased to +20, up from +15.

## Final hero roster (2020-11-19, 62 heroes in pool)

Stats from final game data; ability and 3-star text from the wiki (last state it recorded). Full detail per hero in `underlords-heroes.md`.

| Tier | Hero | Alliances | HP 1/2/3* | Damage 1/2/3* | Atk rate (s) | Range | Armor | MR % | Mana | Move | Ability | 3-star effect |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| T1 | Anti-Mage | Rogue, Hunter | 750/1500/3000 | 45-55/90-110/180-220 | 1.2 | 1 | 5 | 30/40/50 | 0 | 310 | Mana Break: Passive: The Anti-Mage burns [30/60/90] of the opponent's mana on each attack, dealing 50% of the mana burned as damage to the target. | Anti-Mage's Mana Break does an extra 200 damage to his target if his attack burns all remaining mana. |
| T1 | Batrider | Troll, Knight | 750/1500/3000 | 40-55/80-110/160-220 | 1.3 | 3 | 5 | 0 | 20 | 290 | Sticky Napalm: Batrider throws flaming sticky oil, coating the ground 1 cells away from point of impact. The oil adds [30/50/80] damage per stack to Batrider's attacks, and slows the movement and attack speed of enemies by 5%. Additional casts of Sticky Napalm continue to increase damage and slow percentages, up to [4/7/10] stacks. | Sticky Napalm sets enemies on fire for 5 seconds. |
| T1 | Bounty Hunter | Rogue, Assassin | 825/1650/3300 | 50-70/100-140/200-280 | 1 | 1 | 5 | 0 | 100 | 315 | Shuriken Toss: Bounty Hunter hurls a deadly shuriken at an enemy unit, dealing [300/500/700] damage and mini-stunning the target. | Bounty Hunter will generate 1 gold when killing an enemy hero. |
| T1 | Crystal Maiden | Human, Mage | 700/1400/2800 | 40-45/80-90/160-180 | 1.5 | 4 | 5 | 0 | 100 | 275 | Frostbite: Encases an enemy unit in ice, prohibiting movement and attack, while dealing [100/150/200] damage per second for 5 seconds. Targets affected by frostbite cannot be healed. | At three stars, all allies have their cooldowns reduced by 20% while Crystal Maiden is alive. |
| T1 | Dazzle | Troll, Healer, Poisoner | 900/1800/3600 | 40-60/80-120/160-240 | 1.7 | 3 | 5 | 0 | 100 | 310 | Shadow Wave: Dazzle sends out a bolt of power that arcs between up to [3/4/5] allies that are 3 cells apart, healing each ally for [80/140/200] while damaging any enemy units 1 cell away for [80/140/200]. Dazzle is always healed by Shadow Wave. | Dazzle offers a Shallow Grave to all friendly units up to 1 cell away. All blessed units survive for an extra 5 seconds after receiving a killing blow. |
| T1 | Drow Ranger | Heartless, Vigilant, Hunter | 600/1200/2400 | 50-56/100-112/200-224 | 1.2 | 4 | 5 | 0 | 100 | 285 | Precision Aura: Passive: Drow Ranger adds [10/20/30]% bonus to attack speed to all allies 1 cell away. | At three stars, Drow Ranger's arrows become Frost Arrows. These arrows slow a hit unit's movement by 50% and its Attack Speed by 20. The effect lingers for 2 seconds. |
| T1 | Enchantress | Shaman, Healer | 700/1400/2800 | 60-70/120-140/240-280 | 1.5 | 2 | 5 | 0 | 100 | 315 | Nature's Attendants: Enchantress summons a cloud of [3/5/7] wisps that heal all allies up to 4 cells away. For 11 seconds, allies heal 40 health every 1.5 seconds. | Enchantress becomes Untouchable. Each time an enemy attacks her she has a 50% chance of slowing their attack speed by 80 for the duration of their attack. |
| T1 | Lich | Fallen, Mage | 750/1500/3000 | 40-45/80-90/160-180 | 1.5 | 3 | 5 | 20 | 100 | 295 | Frost Shield: Lich applies a shield of ice to a random ally, reducing damage from attacks by [10/20/30]. While the shield is active, enemy units within 2 cell will take [20/40/60] damage per second and lose 50% Movement Speed and 25 Attack Speed. The shield lasts 7 seconds. | Lich gains Chain Frost. Lich releases an orb of frost that bounces between nearby enemy units up to 7 times, slowing their attack and movement speed by -65% for 2.5 seconds and dealing 200 damage each time it hits. |
| T1 | Magnus | Savage, Shaman | 1100/2200/4400 | 55-65/110-130/220-260 | 1.7 | 1 | 5 | 0 | 100 | 280 | Empower: Magnus enrages a friendly unit, granting it +[30/50/80]% damage and [30/50/80]% Cleave for 20 seconds. | At three stars, Magnus gains Reverse Polarity. On cast, enemies within 2 cells take 100 damage and are stunned for 1 second. |
| T1 | Phantom Assassin | Assassin, Swordsman | 825/1650/3300 | 40-60/80-120/160-240 | 1.2 | 1 | 5 | 0 | 100 | 305 | Coup de Grace: Passive: Phantom Assassin has a [15/20/25]% chance of delivering a devastating [300/450/600]% critical strike to enemy units. | Phantom Assassin gains the bonus of the current active level of Rogue Alliance, if any. |
| T1 | Shadow Demon | Heartless, Demon | 900/1800/3600 | 45-65/90-130/180-260 | 1.6 | 3 | 5 | 0 | 100 | 310 | Demonic Purge: Repeatedly purges the target enemy unit, removing positive buffs, applying break, and slowing the target for 5 seconds. The unit slowly regains its movement/attack speed until the end of the duration, upon which [500/800/1100] damage is dealt. | Shadow Demon applies Soul Catcher to the target of Demonic Purge. The unit loses 30% of its current health at the start, and gains half that back after the purge finishes if it survives. |
| T1 | Slardar | Scaled, Warrior | 900/1800/3600 | 60-85/120-170/240-340 | 1.2 | 1 | 5 | 0 | 75 | 290 | Corrosive Haze: Slardar modifies enemy armor by [-10/-20/-30] for 20 seconds. | Slardar bashes the target after casting Corrosive Haze, dealing 100 Physical damage and stunning for 1.5 seconds. |
| T1 | Snapfire | Brawny, Dragon | 950/1900/3800 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 0 | 100 | 295 | Ol' Scatterblast: After 4 basic attacks, Snapfire readies her shotgun, blasting an enemy within 2 cells for [150/250/350] in a cone 3 cells wide and slows the attack of affected units by [-10/-15/-20]. The effect lasts [1/2/4] seconds. | Ol' Scatterblast also reduces enemy armor by 5 for the duration of the debuff. |
| T1 | Tusk | Savage, Warrior | 1150/2300/4600 | 50-55/100-110/200-220 | 1.2 | 1 | 5 | 0 | 60 | 300 | Walrus Punch: Tusk connects with his mighty Walrus Punch, a critical strike that deals [300/350/400]% damage and launches its victim into the air for [1/1/1.5] second(s). Upon landing, the victim is slowed by 40% for [2/2/3] seconds and loses [0/0/40] Attack Speed. Tusk cannot be killed while punching. | Walrus Punch slows enemies for a longer duration and causes them to lose Attack Speed as well as Movement Speed. |
| T1 | Vengeful Spirit | Fallen, Heartless | 850/1700/3400 | 45-55/90-110/180-220 | 1.3 | 2 | 5 | 0 | 60 | 300 | Wave of Terror: Vengeful Spirit lets loose a wicked cry in a straight path, damaging enemy units along the way for [100/175/200] damage, and reducing their armor by [3/5/7] for 6 seconds. | When Vengeful Spirit dies, she deals 300 damage to all enemies within 2 cells. |
| T1 | Venomancer | Scaled, Summoner, Poisoner | 750/1500/3000 | 35-45/70-90/140-180 | 1.2 | 3 | 0 | 20 | 80 | 275 | Plague Ward: Venomancer summons 1 Plague Ward to fight alongside him. Plague Wards apply Poison stacks to their target and have [50/50/99]% Magic Resistance. | Plague Wards are immune to Magical damage. |
| T2 | Bristleback | Brawny, Savage | 1350/2700/5400 | 100-130/200-260/400-520 | 1.7 | 1 | 5 | 0 | 20 | 300 | Quill Spray: Bristleback sprays quills from his back, damaging all enemy units 2 cells away. Enemies take [20/40/80] damage along with an additional [30/32/34] damage for each stack of Quill Spray they have been hit with this round up to a maximum of 5 stacks. | Quill Spray's cooldown reduced to 1.5 seconds. |
| T2 | Chaos Knight | Demon, Knight | 1125/2250/4500 | 55-130/110-260/220-520 | 1.3 | 1 | 5 | 0 | 75 | 320 | Chaos Bolt: Chaos Knight throws a mysterious bolt of energy that deals [100/150/200]-[200/300/400] damage and stuns for [1/2/3]-[2/3/4] seconds. | At three stars, Chaos Knight's allied Knights always count as adjacent to a friendly Knight. |
| T2 | Earth Spirit | Spirit, Warrior | 1275/2550/5100 | 55-60/110-120/220-240 | 1.5 | 1 | 10 | 0 | 100 | 300 | Geomagnetic Grip: Earth Spirit summons a Stone Remnant off the map and pulls it towards his current target. Enemies struck by the incoming Stone Remnant are silenced for 3 seconds and take [100/150/200] damage. | Geomagnetic Grip stuns enemies for 1.5 seconds. |
| T2 | Juggernaut | Brawny, Swordsman | 900/1800/3600 | 60-75/120-150/240-300 | 1/0.9/0.8 | 1 | 5 | 0 | 100 | 300 | Blade Fury: Juggernaut spins with his blade, rendering him immune to magic and dealing [50/100/150] damage per second to enemy units 1 cell away for 5 seconds. Juggernaut may also start spinning to avoid a magical attack. | While Juggernaut is alive, all allies heroes gains 15% of Juggernaut's Max HP. |
| T2 | Kunkka | Human, Warrior, Swordsman | 1200/2400/4800 | 65-85/130-170/260-340 | 1.4 | 1 | 5 | 0 | 100 | 300 | Ghostship: Kunkka summons a ghostly ship that sails through the battle before smashing apart, dealing [50/125/250] damage and stunning all enemies up to [2/2/3] cells away from the wreckage for [1.0/1.3/1.7] seconds. | Kunkka gains Tidebringer. Every 4 attacks, Kunkka takes a mighty swing and deals 150 bonus damage to his target and cleaves for 75%. |
| T2 | Legion Commander | Human, Champion | 600/1200/2400 | 50-60/100-120/200-240 | 1.25 | 1 | 5 | 0 | 100 | 315 | Duel: Legion Commander targets the lowest health enemy 1 cell away and they are forced to attack each other for [4.0/4.75/5.5]s. Neither hero can use abilities nor be healed. If either hero dies during the duration, the hero winning the Duel gains permanent [5/8/12] bonus damage. ([20/32/48] bonus damage in Knockout) | At the start of a Duel, any enemies outside the Duel currently targeting Legion Commander will pick other targets for their regular attacks. |
| T2 | Luna | Vigilant, Knight | 750/1500/3000 | 60-70/120-140/240-280 | 1.3 | 3 | 5 | 0 | 100 | 325 | Moon Glaives: Passive: Luna's glaives bounce [3/5/7] times between enemies 2 cells away. Glaives deal 30% less damage with each bounce. | At three stars, Luna gains Eclipse. In Luna's time of need, Selemene blocks the light in a radius of 2 cells around Luna and showers enemies with 15 beams of lunar energy, one every 0.3 seconds, doing 150 damage per beam and stunning for 0.5 seconds, with a maximum of 5 beams per enemy unit. |
| T2 | Meepo | Rogue, Summoner | 1100/2200/4400 | 50-60/100-120/200-240 | 1.1 | 1 | 5 | 0 | 75 | 310 | Poof: Meepo teleports to a random enemy, dealing [150/250/350] damage to all adjacent enemy units. | Clones of Meepo copy Meepo's item as well. |
| T2 | Nature's Prophet | Shaman, Summoner | 900/1800/3600 | 55-65/110-130/220-260 | 1.3 | 3 | 0 | 0 | 80 | 290 | Nature's Call: Nature's Prophet summons a Treant. This Treant is considered a druid. | Treants spawn on the enemy's side of the board. |
| T2 | Pudge | Heartless, Warrior | 1650/3300/6600 | 45-55/90-110/180-220 | 1.7 | 1 | 5 | 5 | 100 | 280 | Meat Hook: Pudge launches a bloody hook towards his farthest enemy, dragging the victim back and dealing [150/220/290] damage. Pudge and allies 1 cell away will direct their attacks to the victim until it dies. | At three stars, Pudge becomes a Flesh Heap and gains 500 Max HP and 10% Magic Resistance each time an enemy dies within 1 cell of him. |
| T2 | Queen of Pain | Demon, Assassin, Poisoner | 825/1650/3300 | 55-60/110-120/220-240 | 1.2 | 3 | 5 | 0 | 60 | 290 | Scream of Pain: The Queen of Pain lets loose a piercing scream around her, damaging all enemies up to 3 cells away for [200/300/400] damage. | At three stars, the Queen of Pain captivates nearby enemies, allowing her a 25% chance to evade attacks from enemies up to 2 cells away. |
| T2 | Spirit Breaker | Savage, Brute | 1200/2400/4800 | 55-70/110-140/220-280 | 1.5 | 1 | 5 | 0 | 100 | 330 | Charge of Darkness: Spirit Breaker charges towards a distant enemy, hitting it and enemies along the way for [100/150/200] damage and stunning them for [1.0/1.3/1.7] seconds. | Spirit Breaker gains Greater Bash. Spirit Breaker's attacks have a 25% chance of dealing 150 bonus damage and stunning the target for 2 seconds. |
| T2 | Storm Spirit | Spirit, Mage | 750/1500/3000 | 50-60/100-120/200-240 | 1.6 | 3 | 5 | 0 | 40 | 290 | Ball Lightning: Storm Spirit becomes volatile electricity, charging across the battlefield and dealing [80/120/180] damage to each unit he passes through, plus [15/20/25] damage per cell he has traveled. | Storm gains Overload. Casting a spell creates an electrical charge which is released in a burst on Storm's next attack, dealing 150 magical damage and slowing the target's Attack Speed by 50 for 1.5 seconds. A Triangular Attack by Storm counts as an attack, in which case all units Storm hits in the Attack take the damage and are slowed. |
| T2 | Windranger | Vigilant, Hunter | 825/1650/3300 | 50-60/100-120/200-240 | 0.9 | 4 | 5 | 0 | 100 | 290 | Powershot: Windranger charges her bow for up to 1.5 seconds for a single powerful shot, dealing up to [200/400/600] damage. For each enemy that Powershot hits, its damage is reduced by 10%. | Windranger gains 100% evasion and Magic Immunity while casting Powershot. This lasts for 3 seconds. |
| T3 | Abaddon | Fallen, Knight | 1125/2250/5400 | 45-55/90-110/216-264 | 1.5 | 1 | 10 | 0 | 100 | 325 | Aphotic Shield: Abaddon protects a friendly unit with a shield that absorbs [100/300/600] damage for 10 seconds. When the shield is destroyed it bursts and deals [100/300/600] damage to enemies up to 2 cells away. Removes certain types of negative buffs and stuns on cast. | At three stars, Abaddon gains the ability to apply the Curse of Avernus to enemies he attacks. Once Abaddon has attacked the same enemy unit 3 times in a row that unit becomes silenced for 2 seconds and Abaddon gains +30 Attack Speed against it. This refreshes on each additional landed attack, but starts again from 0 attacks if Abaddon changes targets. |
| T3 | Alchemist | Brute, Rogue, Poisoner | 1300/2600/6240 | 40-60/80-120/192-288 | 1 | 1 | 5 | 0 | 100 | 305 | Acid Spray: Alchemist sprays high-pressure acid, affecting enemies up to 2 cells away from the target. Enemy units who step across the contaminated terrain take [10/20/35] damage per second and have their armor reduced by [-3/-6/-9]. | At the start of combat, Alchemist causes his Ogre to enter a chemically induced rage, increasing attack speed by 75 and becoming magic immune for 5 seconds. |
| T3 | Beastmaster | Brawny, Hunter, Shaman | 1000/2000/4800 | 70-80/140-160/336-384 | 1.1 | 1 | 5 | 0 | 100 | 305 | Wild Axes: Beastmaster sends his axes flying and calls them home again, slicing through enemy units along their path for [60/100/140] damage. Each axe can hit an enemy once, and amplifies subsequent damage from Beastmaster by [10/20/30]%. | At three stars, Beastmaster takes 75% less damage from summoned units and is immune to any magical effects from them. |
| T3 | Ember Spirit | Spirit, Assassin, Swordsman | 825/1650/3960 | 80-100/160-200/384-480 | 1.5 | 1 | 5 | 0 | 100 | 305 | Sleight of Fist: Ember Spirit dashes around with blazing speed, attacking all enemies within [2/2/3] cells, then returning to his start location. Attacks deal [100/150/200] bonus damage. | Sleight of Fist's area of effect radius is increased. |
| T3 | Lifestealer | Heartless, Brute | 1000/2000/4800 | 55-60/110-120/264-288 | 1.5/1.4/1.3 | 1 | 5 | 0 | 75 | 300 | Feast: Passive: Lifestealer's attacks damage enemies and heal himself for [4.5/6/7]% of his target's max health. | Lifestealer gains Rage. On cast Lifestealer becomes Magic Immune and gains +30% Move Speed and +30 Attack Speed. The effects last for 5 seconds. |
| T3 | Lycan | Human, Savage, Summoner | 1125/2250/5400 | 50-60/100-120/240-288 | 1.1 | 1 | 5 | 0 | 100 | 315 | Shapeshift: Lycan summons [2/2/4] wolves transforms and gains [30/40/50]% to his max health, his move speed is boosted to 650 and gains 40% chance to Critical Hit for [150/175/200]% Damage. | Shapeshift summons more wolves. |
| T3 | Omniknight | Human, Knight, Healer | 1100/2200/5280 | 40-50/80-100/192-240 | 1.4 | 1 | 10 | 0 | 60 | 300 | Purification: Omniknight instantly heals a friendly unit for [200/400/720] and damages all enemy units [1/1/2] cell away. | Purification's area of effect is tripled. |
| T3 | Puck | Dragon, Mage | 850/1700/4080 | 50-60/100-120/240-288 | 1.7 | 3 | 5 | 0 | 60 | 290 | Illusory Orb: Puck launches a magical orb that floats in a straight path, damaging enemy units along the way for [200/225/250] damage. | Illusory Orb's cooldown reduced to 0.5 seconds |
| T3 | Shadow Shaman | Troll, Summoner | 850/1700/4080 | 50-60/100-120/240-288 | 1.25 | 3 | 5 | 0 | 70 | 285 | Mass Serpent Ward: Shadow Shaman traps an enemy unit by summoning up to 4 Serpent Wards. | Shadow Shaman hexes the enemy unit targeted by Mass Serpent Ward for 7 seconds. |
| T3 | Slark | Scaled, Assassin | 800/1600/3840 | 50-60/78-98/138-178 | 1/0.8/0.6 | 1 | 5 | 0 | 100 | 300 | Essence Shift: Passive: Slark increases his damage by [3/4/5] and his attack speed by [3/4/5] with each attack. | Slark gains Dark Pact. After a short delay, Slark sacrifices some of his blood, purging negative effects and damaging enemies within 2 cells for a total of 300 damage. Slark himself takes 50% of the damage too. |
| T3 | Spectre | Void, Demon | 950/1900/4560 | 55-65/110-130/264-312 | 0.9 | 1 | 5 | 0 | 70 | 310 | Spectral Dagger: Spectre flings a dagger at an enemy hero, dealing [300/450/600] damage and drawing a Shadow Path which slows the movement speed of enemy units by 50 and Attack Speed by 50 for 4 seconds. Spectre runs to attack this unit and gains 175 Attack Speed and 100% bonus movement speed and phasing movement for 4 seconds. | While Spectre is alive, whenever a Demon casts a spell all Allied Demons erupt in flames and wreathe their targets in fire, doing 200 damage in a wide swathe and setting units hit on fire. |
| T3 | Terrorblade | Demon, Hunter, Fallen | 975/1950/4680 | 45-65/90-130/216-312 | 1.4 | 1 | 5 | 30 | 80 | 310 | Metamorphosis: Terrorblade transforms into a powerful demon, gaining a ranged attack of 4 cells as well as [50/100/200] bonus to damage and [50/75/100] to attack speed. Terrorblade also exchanges health values with another ally when this ability starts. | At three stars, Terrorblade can summon demonic energy to gain +125% Pure Damage against enemy Demons. The bonus lasts 5 seconds each cast. |
| T3 | Treant Protector | Shaman, Healer | 1125/2250/5400 | 60-90/120-180/288-432 | 2 | 1 | 5 | 0 | 100 | 270 | Leech Seed: Treant Protector plants a life-sapping seed in an enemy unit, draining its health by [100/150/200] every 1 seconds, while simultaneously slowing its attack and movement speed by -50%. The seed heals friendly units up to 1 cell away from the target. Lasts 5 seconds. Jumps to a new target within 1 cell if the current target dies. | Treant Protector gains Living Armor. Units healed by Leech Seed gain +5 Armor. Jumps to a new target within 1 cell if the current target dies. |
| T4 | Death Prophet | Fallen, Heartless | 1250/2500/5000 | 50-60/100-120/200-240 | 1.25 | 3 | 5 | 0 | 100 | 300 | Exorcism: Death Prophet sends out a stream of up to [6/8/10] evil spirits for a duration of 6 seconds. They roam the battlefield in a 3 cell radius, dealing [83/100/110] damage to all enemies they pass through and returning to heal Death Prophet for 25% of the damage dealt. | - |
| T4 | Doom | Demon, Brute | 1575/3150/6300 | 110-120/220-240/440-480 | 1.1 | 1 | 15 | 0 | 100 | 285 | Doom: Doom invokes a powerful curse that prevents an enemy from casting spells and applies break to them, while also taking [60/90/120] damage per second. Lasts 20 seconds. | - |
| T4 | Lina | Human, Mage | 1000/2000/4000 | 65-70/130-140/260-280 | 1.4 | 4 | 5 | 0 | 75 | 290 | Laguna Blade: Lina unleashes a bolt of lightning, dealing [750/950/1900] damage to a single unit and setting them on fire for 5.0 seconds. | - |
| T4 | Lone Druid | Savage, Shaman, Summoner | 1200/2400/4800 | 50-60/100-120/200-240 | 1.3 | 3 | 0 | 20 | 60 | 325 | Summon Spirit Bear: Summons a powerful Spirit Bear companion. | - |
| T4 | Mirana | Vigilant, Hunter | 1125/2250/4500 | 70-80/140-160/280-320 | 1 | 4 | 5 | 0 | 70 | 310 | Sacred Arrow: Mirana fires a long-range arrow with deadly precision, stunning her target for [3/5/7] seconds and dealing [400/600/1400] damage. | - |
| T4 | Pangolier | Savage, Swordsman | 1400/2800/5600 | 60-70/120-140/240-280 | 1.1 | 1 | 10 | 10 | 70 | 330 | Shield Crash: Pangolier jumps in the air and slams back into the ground, doing [150/300/450] damage to enemies within 2 cell and gaining 8% damage reduction per enemy for 10 seconds. After the jump all enemies in attack range focus on Pangolier. | - |
| T4 | Rubick | Mage, Magus | 1300/2600/5200 | 50-60/100-120/200-240 | 1.3 | 2 | 5 | 20 | 100 | 310 | Fade Bolt: Rubick creates a powerful stream of arcane energy that strikes a targets and jumps 4 additional times, dealing [75/125/175] damage and reducing their attack damage by 20% for 5 seconds. | - |
| T4 | Sven | Rogue, Knight, Swordsman | 1425/2850/5700 | 110-130/220-260/440-520 | 1.7 | 1 | 5 | 0 | 80 | 300 | God's Strength: Sven enrages and gains [100/150/200]% damage for 60 seconds. | - |
| T4 | Templar Assassin | Vigilant, Void, Assassin | 1125/2250/4500 | 90-110/180-220/360-440 | 1.375 | 3 | 5 | 0 | 100 | 305 | Refraction: Templar Assassin becomes elusive, avoiding damage and gaining +[40/60/80]% Attack Damage. The damage and avoidance effects are separate, and have [4/6/8] instances each. | - |
| T4 | Tidehunter | Scaled, Warrior | 1650/3300/6600 | 60-70/120-140/240-280 | 2 | 1 | 10 | 0 | 100 | 300 | Ravage: Tidehunter slams the ground, causing tentacles to erupt in all directions, dealing [100/150/250] damage and stunning all enemy units [1/2/3] cells away for [2/2.25/3] seconds. | - |
| T4 | Viper | Dragon, Poisoner | 1100/2200/4400 | 60-80/120-160/240-320 | 1 | 3 | 5 | 20 | 70 | 275 | Nethertoxin: Viper poisons a cell, dealing [100/150/225] damage per second to any enemy unit in it, rooting them and applying break. | - |
| T4 | Void Spirit | Void, Spirit | 1275/2550/5100 | 70-80/140-160/280-320 | 1 | 1 | 5 | 0 | 100 | 305 | Dissimilate: Void Spirit enters the aether, disappearing and creating 3 portals. After 1.3 seconds, he exits one of the portals and deals [200/300/400] damage to enemies within 1 cell, reducing their armor by [10/15/20] for 5 seconds. | - |
| T5 | Axe | Brawny, Brute | 3000/6000/12000 | 50-60/100-120/200-240 | 1.5 | 1 | 15 | 0 | 50 | 295 | Counter Helix: When attacked, Axe has a [ 18/20/22 ]% chance to perform a helix counter attack, dealing [ 50/100/150 ] pure damage to all enemies within 1 cell. | - |
| T5 | Dragon Knight | Human, Dragon, Knight | 1400/2800/5600 | 55-75/110-150/220-300 | 1.2 | 1 | 10 | 0 | 50 | 295 | Breathe Fire: Unleashes a breath of fire in front of Dragon Knight that burns enemies for [150/200/250] damage and reduces the damage their attacks deal by [25/35/45]% for 4 seconds. | - |
| T5 | Faceless Void | Void, Assassin | 1500/3000/6000 | 140-170/280-340/560-680 | 1.2 | 1 | 5 | 0 | 100 | 290 | Chronocube: Creates a blister in spacetime, stunning all units caught 2 cells away from the center of the cube for [2/3/4]s. Faceless Void moves very quickly inside the cube and gains 75 attack speed. Only Faceless Void is unaffected. When cast, Faceless Void attempts to teleport all friendly Assassins out of the cube. | - |
| T5 | Keeper of the Light | Human, Mage | 1100/2200/4400 | 55-60/110-120/220-240 | 1.7 | 4 | 5 | 0 | 100 | 330 | Illuminate: The Keeper of the Light channels energy, building power the longer it's channeled. Once released, a wave is sent forth that deals [175/225/275] damage per second channeled. | - |
| T5 | Medusa | Scaled, Hunter | 1275/2550/5100 | 75-80/150-160/300-320 | 1 | 3 | 5 | 10 | 100 | 275 | Stone Gaze: Any enemy unit up to 3 cells away looking at Medusa will have their movement and attack speed slowed by 30%. If 2 seconds of total time is accumulated looking at Medusa, that unit will turn to stone. Petrified units are stunned, and take bonus 20% physical damage. | - |
| T5 | Troll Warlord | Troll, Warrior | 1500/3000/6000 | 180-200/360-400/520-600 | 1.3 | 3 | 5 | 0 | 100 | 290 | Fervor: Passive: Troll Warlord rains blow after blow gaining [12/16/20]% bonus increased attack speed per stack. If Troll changes targets, the stack drops to zero. This effect can only stack up to [8/12/16] times. | - |
| T5 | Wraith King | Fallen, Swordsman | 2000/4000/8000 | 90-110/180-220/360-440 | 1.3 | 1 | 10 | 10 | 75 | 310 | Wraithfire Blast: Wraith King sears an enemy within 3 cells with spectral fire, stunning them for 2 seconds and dealing [100/200/300] damage per second for 5 seconds. | - |

### Heroes not in the final pool (23)

| Hero | Last snapshot in pool | Alliances then | Tier then |
|---|---|---|---|
| Arc Warden | Season One 2020-02-25 | Primordial, Summoner | T1 |
| Bloodseeker | Season One 2020-02-25 | Blood-Bound, Deadeye | T1 |
| Broodmother | Season One 2020-02-25 | Insect, Warlock | T3 |
| Clockwerk | Outlanders 2019-12-06 | Scrappy, Inventor | T3 |
| Disruptor | Season One 2020-02-25 | Brawny, Warlock | T4 |
| Enigma | Season One 2020-02-25 | Void, Primordial | T3 |
| Gyrocopter | Outlanders 2019-12-06 | Deadeye, Inventor | T5 |
| Io | Season One 2020-02-25 | Primordial, Druid | T3 |
| Morphling | Season One 2020-02-25 | Primordial, Mage | T3 |
| Necrophos | Season One 2020-02-25 | Heartless, Warlock, Healer | T4 |
| Nyx Assassin | Season One 2020-02-25 | Insect, Assassin | T1 |
| Ogre Magi | Season One 2020-02-25 | Blood-Bound, Brute, Mage | T2 |
| Razor | Season One 2020-02-25 | Primordial, Mage | T1 |
| Sand King | Season One 2020-02-25 | Savage, Insect | T5 |
| Shadow Fiend | Season One 2020-02-25 | Demon, Warlock | T3 |
| Sniper | Outlanders 2019-12-06 | Scrappy, Deadeye, Hunter | T3 |
| Techies | Outlanders 2019-12-06 | Scrappy, Inventor | T5 |
| Timbersaw | Outlanders 2019-12-06 | Scrappy, Inventor | T2 |
| Tinker | Outlanders 2019-12-06 | Scrappy, Inventor | T3 |
| Tiny | Season One 2020-02-25 | Primordial, Warrior | T1 |
| Warlock | Season One 2020-02-25 | Blood-Bound, Warlock, Healer | T1 |
| Weaver | Season One 2020-02-25 | Insect, Hunter | T1 |
| Witch Doctor | Season One 2020-02-25 | Troll, Warlock | T2 |

## Gaps and open questions

- Base gold per round after 2019-12-18: wiki says 5, patch note says raised from 5 to 7. Unresolved.
- Which XP curve was live at the end (wiki table vs the 2019-10-24 curve). Unknown.
- Season One (2020-02-25) alliance breakpoints and items: no data file found; only patch-note text. Final-patch data is complete.
- Ability numbers for Season One and the final patch come from the wiki, not from game data (abilities.json moved server-side on 2020-02-04 and no later dump was found). Some wiki ability pages may lag the last patches.
- Rank Score thresholds per minor rank and solo rank-change values. Unknown.
- Board dimensions and phase timer lengths. Not stated in the sources read.
- Earlier interest cap and start gold before 2019-12-18. Unknown.
- The 6-value arrays on final items are not explained in any source.
- Underlord talent tables in the Season One client files (placeholder names) were probably never shipped.
