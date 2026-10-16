# Auto Chess (Drodo Studio / Dragonest standalone game) reference data

Compiled 2026-10-15 as design inspiration for Digi-Tactics. Covers the standalone game (mobile, PC/Epic, PS4), not the Dota 2 mod (see `dota-auto-chess.md`). "unknown" means no source was found.

## Sources

Season history:

- Official English site announcements, pulled in full through the site's own JSON API (`https://website-api.dragonest.com/resources?menu_mark[]=announcement|news|activity&per-page=100&page=N&lang=en`, header `appCode: e52b47a8c88`). Coverage: Jun 2019 to Mar 2023, then Mar-May 2025. Individual posts: `https://ac.dragonest.com/en/announcement/detail/<code>`.
- Same API with `lang=zh-cn` (Chinese official site). This adds Jul-Dec 2019 patch notes and Nov 2024 to Mar 2025 notes. Posts: `https://ac.dragonest.com/zh-cn/announcement/detail/<code>`.
- TapTap official forum for 多多自走棋 (app 197038): 1,535 official posts crawled via `webapiv2/feed/v7/by-group`. Full text taken from the server-rendered post pages (`https://www.taptap.cn/moment/<id>`). Coverage: Aug 2020 to Oct 2026. There is a hole between 2021-11 and 2022-06 and another between 2026-02-10 and 2026-08-11.
- Apple App Store version history for "Auto Chess - Global Teamfights" (https://apps.apple.com/us/app/auto-chess-global-teamfights/id1464482102). English release notes for v2.20.2 (2023-07-12) through v2.42.2 (2026-09-23).
- Wikipedia (https://en.wikipedia.org/wiki/Auto_Chess) for platform release dates. Press: droidgamers.com and thesixthaxis.com (2019-06-06) for Season 1.
- Fandom (autochess.fandom.com) has about 15 stub pages and nothing useful. Reddit (old.reddit, pullpush) returned rate limits or blocks. Many official posts are image-only (character cards, pass previews) and were not OCR'd.

Game data (piece, synergy, item and economy tables). Short keys are used in the economy section:

- `off_zh_char`: https://ac.dragonest.com/zh-cn/charactor (Nuxt state embedded in page; records updated up to 2026-09-04)
- `off_en_char`: https://ac.dragonest.com/en/charactor (records updated 2022-05-17 to 2026-01-29)
- `off_zh_eq`: https://ac.dragonest.com/zh-cn/equipment (records 2024-01-10 to 2026-01-29)
- `off_en_eq`: https://ac.dragonest.com/en/equipment (records 2020-09-24 to 2025-11-27; recipe-era items)
- `off_api`: https://website-api.dragonest.com/resources?menu_mark=announcement|news&per-page=50&page=N&appcode=e52b47a8c88&lang=en|zh-cn (155 EN + 211 ZH announcements, 143 EN + 173 ZH news, 2019-07 to 2025-05-30)
- `n_2019_12_12`: Official ZH announcement 12月12日停服更新公告 (resource e595ccbacb0, via off_api)
- `n_2020_03_19`: Official ZH announcement 3月19日停服更新公告 (e69073953b1)
- `n_2020_04_28`: Official EN/ZH notice PROBABILITY DISPLAY BUG 2020-04-28 (e7183e989aa / e7183e15e56)
- `n_2020_06_05`: Official ZH announcement 6月5日停服更新公告 (e78f19c4768)
- `n_2020_08_exp`: Official EN [EXPERIMENTAL] notes, 10 Aug 2020 section (e75f10cba5e) / ZH 体验服8月28日 (e866a61ac47)
- `n_2020_08_30`: Official EN 08/30 MAINTENANCE (1743db567e0)
- `n_2020_12_02`: Official ZH 12月3日停服维护公告 (e9d62f60d47)
- `n_2020_12_28`: Official EN Dev Log: Item mechanism (ea294a4af9a)
- `n_2021_05_08`: Official EN [EXPERIMENTAL] 5/8 MAINTENANCE (ebcf5eed7f6)
- `n_2022_07_28`: Official EN MAINTENANCE on July 27 (1823e90b2bd)
- `n_2022_09_08`: Official ZH 9月8日停服更新公告 (f1ef4c4562e)
- `n_2023_03_09`: Official ZH 3月9日停服更新公告 (f438797a7a4) / EN MAINTENANCE on March 8 (f4387dfb813)
- `n_2024_11_28`: Official ZH 11月28日停服更新公告 (fdb58e28d02)
- `n_2025_01_23`: Official ZH 1月23日停服更新公告 (195ef48ba55)
- `n_2025_02_13`: Official ZH 2月13日停服更新公告 (fdb58c64f55)
- `n_2025_03_27`: Official EN MAINTENANCE on 26th March (fdb57c9bb46) / ZH 3月27日 (fdb5ac589f6)
- `n_2025_05_29`: Official EN MAINTENANCE on 29th May (1971f3f0799) / ZH 5月29日 (fe738673b09)
- `g_4399_2019`: https://news.4399.com/dtzzq/xinde/m/876513.html (pubDate 2019-04-16)
- `g_biubiu_2020`: https://www.biubiu001.com/zzq/13090.html (2020-04-26)
- `g_taptap_2019`: https://www.taptap.cn/moment/15204571756366726 (2019-09-22)
- `g_taptap_pool_2019`: https://www.taptap.cn/moment/15223749984913766 (2019-12-05)
- `g_gamersky_pool_2019`: https://www.gamersky.com/handbooksy/201904/1176519.shtml (2019-04-24)
- `g_xiaomi_2020`: https://game.xiaomi.com/viewpoint/1127866428_1597305232470_16 (2020-08-13)
- `g_gouhuo_2019`: https://gouhuo.qq.com/guide/detail/3245224418943040 (2019-11-07)
- `g_bluestacks`: https://www.bluestacks.com/blog/game-guides/auto-chess/complete-guide-early-game-economy-en.html
- `dac_wiki`: https://dotaautochess.fandom.com/wiki/Chess_pieces (Dota Auto Chess mod, not the standalone; used only for comparison)

## Conventions

- Quality maps to cost: Common 1, Uncommon 2, Rare 3, Epic 4, Legendary 5 gold. Source: cost and quality changes stated together, e.g. "Storm Shaman Cost 5 → 4 Gold, Quality Legendary → Epic" (https://ac.dragonest.com/en/announcement/detail/eb6b7e63c64) and "Werewolf Rare → Uncommon (3 Gold → 2 Gold)" (https://ac.dragonest.com/en/announcement/detail/f0ed9727023). When a source gives only quality, the cost below comes from this mapping.
- Pieces known only from Chinese sources keep their Chinese name, with an English name added when an English source confirms it.
- Dates are UTC unless marked. The Chinese and global servers have run the same patches on the same dates since at least 2020. App Store notes and TapTap notes for the same patch match.
- "unknown" means no source found.

- Data version: Primary piece/synergy/item data = official Dragonest site Nuxt state, ZH (zh-cn) locale, records last updated 2026-09-04 (112 pieces, 24 races, 15 classes, 66 items). This is the illustration list on the site, not necessarily the live rotation; records were updated at different times (most per-piece records dated 2022-07-25, newer pieces 2023-2026). EN locale of the same site is older (84 pieces, records mostly 2022-05-17; items are the pre-2024 recipe system) and is attached per piece as ability_en_older / en_site_older_stats. Stats are raw strings from the site; attack_range units are inconsistent (grid values like 1/4 and engine units like 160). ability_cooldown_s is the site field waitingTime; it matches announced CDs (e.g. Valkyrie CD 10/8/6). Mana values are not published. Economy rules are assembled from official patch notes (2019-2025) and dated third-party guides; conflicts are listed as variants.

- Translations: text marked "(translated)" was translated from the official Chinese site for this file and is not official English. Numbers are copied unchanged. Names marked "(unofficial)" are literal translations.

## Seasons

One section per season, from the official announcements, TapTap and App Store notes. Each season lists its own source URLs.

### Season 0 / beta (Apr-May 2019)

- Dates: Android public beta Apr 18, 2019; iOS May 22, 2019 (Wikipedia). Standalone version announced Mar 15, 2019.
- No source in this crawl calls a "Season 0". The earliest official English posts start in June 2019.
- Pieces, races, classes, mechanics at launch: unknown from these sources. The Jan 2025 "Back to 2019" mode describes the 2019 version as "55 pieces without divinity wizard version" (App Store v2.32.2).

### Season 1 (about late May 2019 to Jul 21/24, 2019)

Sources: https://www.droidgamers.com/news/auto-chess-gets-season-1-a-battle-pass-and-an-account-level-system-in-a-massive-update/ ; https://www.thesixthaxis.com/2019/06/06/auto-chess-update-mobile-weekly-quests-challenges-chess-pass/ ; https://ac.dragonest.com/en/announcement/detail/e595ccb9fb1 ; https://ac.dragonest.com/en/announcement/detail/e595ccba105 ; https://ac.dragonest.com/en/announcement/detail/e595ccba175 ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbb281 ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbabca

- Dates: Season 1 and the Chess Pass were live by June 6, 2019, when Week 2 pass quests were added. Exact start unknown. The CN notice dated 2019-07-23 says S2 begins Jul 25 (CN time) and gives S1 participants a reward chest.
- Version: unknown.
- Chess Pass (battle pass): 200 tiers, weekly quests and season challenges. Pass holders got 100% Candy and EXP boost. An account level system was added at the same time.
- Weekly balance posts named a "Strong Synergy" and "Weak Synergy" of the week (e.g. Cave Clan weak on Jun 10, Warlock on Jun 18, Mage on Jun 27).
- Jun 27, 2019 update:
  - New race: Divinity. New pieces: God of Thunder and God of War (Divinity). Cost unknown.
  - New items: Axe of Fury, Teleport Dagger, Kira Axe, Claw Wand.
  - New modes: Novice Mode; Fantasy Mode (Friday-Sunday each week). Custom games, match history, formation recommendations.
  - Shop rule: after Round 5, pieces not bought in the previous round do not reappear when the shop refreshes.
  - Druid synergy counts pieces on the bench. Full bench auto-combines bought pieces. Team size raised to 4.
- Jul 9, 2019 (CN): new special 5-cost piece Strange Egg (奇异蛋), can appear at any level. The Fantasy mode team cap became 4.
- Jul 24, 2019 (CN, start of S2 changeover): new piece Shining Archer (光羽弩手). New rule: when a player is eliminated, each of their items has a 50% chance to go to a random surviving player. Warlock synergy changed from (3)/(6) to (2)/(4)/(6) with 10%/20%/30% lifesteal. Queen-rank seasonal avatar frame. Chess Pass voucher store added. Candy amounts multiplied by 10.

### Season 2 (Jul 21, 2019 to Oct 21, 2019; CN Jul 25)

Sources: https://ac.dragonest.com/en/news/detail/e595b9fb99b ; https://ac.dragonest.com/en/announcement/detail/e595ccba1e7 ; https://ac.dragonest.com/en/announcement/detail/e595ccba571 ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbb58e ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbb8af ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbc160

- Dates (official): Season S2 07.21-10.21; Chess Pass S2 07.21-09.22; off-season 09.23-09.24; "Chess Pass special event - DUO" 09.25-10.20. CN: S2 extended to Oct 21 on Aug 27.
- Version: unknown.
- New pieces:
  - Aug 9 (CN) / Aug 21 (PC): Fortune Teller (占卜师); PC also got Ghost Priest. Cost unknown.
  - Aug 27 (CN): Rogue Guard (沉沦守卫), Demon. Cost unknown.
  - Sep 20 (global): Winter Chiropteran. Race in 2019 unknown; given Dragon and Egersis in Jun 2020.
- Removed: Blight Sorcerer (凋零法师), temporarily, Aug 9 (CN). 独眼猎手 removed Aug 27 (CN; English name not confirmed).
- Modes: Duo (2v2) became a permanent casual mode on Sep 20 (global). CN fantasy modes 比翼双飞 (Aug 9), 甜蜜双排 and 从零开始 / "Start From Zero" (Aug 27).
- Mechanics:
  - Aug 27 (CN): synergy tiers now trigger separately per tier. Pieces moving 3 grids or fewer are no longer invulnerable while moving.
  - New items: Lucky Coin, Cloud Halberd, Heaven's Gift, Puppet Mask, Jade Pipe, Icearmor.
  - Sep 20: auto star-up and auto deploy. Two-player teams allowed in Ranked (Classic).
  - PC Aug 21: once you own a 3-star piece, its 1-star copies stop appearing in your shop. Players may concede after Round 15.
- Sep 29 (CN): Feathered synergy 20/40/60% → 20/35/50%.

### Season 3 (Oct 21/22, 2019 to about Jan 12, 2020)

Sources: https://ac.dragonest.com/en/announcement/detail/e595ccba48e ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbc63d ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbc558 ; https://ac.dragonest.com/en/announcement/detail/e595ccba73d ; https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbacb0 ; https://ac.dragonest.com/en/news/detail/e595b9fba7f ; https://ac.dragonest.com/en/announcement/detail/e595ccba7af

- Dates: S3 opened Oct 22 (CN time) per the Oct 18 notes. Ended with the Jan 12, 2020 update (S4 pass revealed Jan 13).
- Version: unknown.
- Ranked Duo added with S3.
- Creep rounds: win or draw gives a choice of 1 of 3 items; a loss gives 1 random item. Base drop rate reduced. Spectator mode, Auto (AI control) toggle, region flags and region leaderboard.
- New class: Wizard (巫师). Effect: race/class synergies that need 4 or more pieces need 1 fewer. Because of Wizard, Cave Clan (4) and Spirit (4) returned, and Glacier (6) and Mech (6) were added (CN Oct 18).
- New pieces (CN Oct 28):
  - Grand Herald (神使): Divinity / Wizard. At battle start it copies the ability of the enemy closest to its mirror point. Cost unknown in 2019; made 3-cost on May 29, 2020 (cost [3/5/7]).
  - Grimtouch (墨客): Demon / Wizard. Ink line that links targeted abilities across marked enemies. Cost unknown.
- Removed: Shadow Devil and Phantom Queen (CN Oct 28).
- Nov 13-27, 2019: Experimental Server opened. Kira (Ogre Mage) back with a new ability.
- Dec 12, 2019:
  - Ogre Mage returns. CN version of its ability: when the targeted ally casts, 30%/40%/50% chance to restore full mana and reset cooldown.
  - Piece skin system added. Penalty system for AFK/early quitters.
  - EXP purchase cost 5 → 4 gold for 4 EXP. Higher legendary odds.
  - "Winterfest" themed Chess Pass interface (Dec 13). CN calls this the "冰雪赛季" (Icy Snow season) pass.

### Season 4 (about Jan 12-13, 2020 to Apr 26, 2020)

Sources: https://ac.dragonest.com/en/announcement/detail/16f93644028 ; https://ac.dragonest.com/en/news/detail/e5c35d247da ; https://ac.dragonest.com/en/news/detail/e5cfa53292e ; https://ac.dragonest.com/en/announcement/detail/e5d35f01cf7 ; https://ac.dragonest.com/en/announcement/detail/e61281eb55f ; https://ac.dragonest.com/en/announcement/detail/e63a0406507 ; https://ac.dragonest.com/zh-cn/announcement/detail/e69073953b1 ; https://ac.dragonest.com/en/news/detail/170c369d974 ; https://ac.dragonest.com/en/announcement/detail/1717234047c ; https://ac.dragonest.com/en/announcement/detail/e6e08d25aae

- Dates: S4 Chess Pass revealed Jan 13, 2020. CN: S4 was set to end Mar 23, extended to Apr 20, then to Apr 26 23:59:59. Special Chess Pass "Abyssal Treasure" ran until Apr 19/20.
- Version: 1.0.1 (named in the Jan 17, 2020 "Strategy of New Season" article).
- New art direction (UI 2.0) and new piece models.
- New pieces (Jan 12):
  - Frostblaze Dragon: Dragon / Warlock (Mage instead of Warlock from Jun 2020).
  - Umbra: Dragon, Egersis / Hunter. 2 gold at release, 3 gold from Feb 7, 2020.
- New piece (Mar 19, CN): The Scryer (占星者): Feathered / Shaman. First multi-ability piece; it picks one of several abilities based on the enemy lineup. A planned "Raven"/"Blood Feather" race was dropped and folded into its passive. Cost unknown.
- Removed: Egersis Prophet, temporarily (Jan 12).
- New synergy tiers: Dragon (6), every ally starts with 100 mana. Mage (9), enemies lose 150% Magic Resistance.
- New mode: Trio, renamed "Quick" on Jan 19-20.
- Items: big recipe and quality rework. New: Nether Essence, Python Wand, Halberd of Judgement. Removed: Shield, Ring of Life, Kira Axe, Fish Fork, Teleport Dagger, Killerbot 300, Wraith Crystal.
- Feb 2020: Assassin (3)/(6) gained a guaranteed or chance-based crit on the first attack each round. Feathered (6)/(9) rebalanced.
- Shop (Mar 19, CN): 5-cost odds at levels 8/9: 2%/4% → 1%/3%. After Round 5, pieces not bought in the last 2 refreshes do not appear.
- Gacha system abolished (announced Apr 11, removed with the next version).

### Season 5 (Apr 27/28, 2020 to Jun 24, 2020)

Sources: https://ac.dragonest.com/en/announcement/detail/e70ede7bc5e ; https://ac.dragonest.com/en/announcement/detail/e7151628289 ; https://ac.dragonest.com/en/announcement/detail/e717de6e3a9 ; https://ac.dragonest.com/en/announcement/detail/e7183e989aa ; https://ac.dragonest.com/en/news/detail/e6eb0c611d5 ; https://ac.dragonest.com/en/news/detail/e71b6cef922 ; https://ac.dragonest.com/en/announcement/detail/e731df9ee61 ; https://ac.dragonest.com/en/announcement/detail/e75f11846c5 ; https://ac.dragonest.com/en/announcement/detail/e77bec67a1b ; https://ac.dragonest.com/en/announcement/detail/e78f15f2282

- Dates: S5 scheduled for Apr 26 16:00 UTC. Shipped with the Apr 27-28 maintenance (first anniversary). Ended Jun 24, 2020.
- Version: unknown.
- PC Early Access launched on the Epic Games Store Apr 29, 2020.
- New piece: Goddess of Light: Divinity / Priest. A Workshop article later calls it 2-cost.
- Piece race changes: Venomancer → Goblin, Kira / Warlock. Lord of Sand → Beast, Insectoid / Assassin. Poisonous Worm → Insectoid / Warlock (Beast added May 12).
- New race: Insectoid (2). When a non-Insectoid allied piece dies and you have a duplicate of it, summons a random Insectoid based on the highest cost among the living duplicates.
- Reworks: Kira (2), stacking HP/ATK when allied melee pieces die, up to 8 stacks. Divinity (2)/(3), cooldown reduction for Divinities and for allies whose race synergy is not active. Priest (2), the chess player blocks 20% of damage and recovers HP.
- Chess Pass: Senior Pass can be bought in stages (Lv 1-10, 11-30, 31-60, 61+).
- Shop odds: at level 5, 3-gold pieces appear 23% of the time (a display bug showed 15%).
- Novice mode became 15 rounds of fixed creeps. Competitive custom lobby with a Judge role.
- May 20: Anniversary mode (removed pieces return). New synergy Dwarf (2): ranged allies gain attack range and target the lowest-HP enemy.
- May 29: Dwarf Sniper becomes 2-cost; Grand Herald becomes 3-cost.
- Jun 4:
  - New piece: Ghost Kid: Egersis / Warrior. Cost unknown.
  - Priest (2) now gives "Green Essence" (consumable, heals the player 1-5 HP).
  - Mech (3)/(6) give "Heart of Mech" (free shop refresh) and "Golden Heart of Mech" (refresh showing 5 legendary pieces).
  - Items can be removed from pieces.
  - Fatigue: with 2 players left, fatigue starts 4 rounds later (no later than Round 50). HP loss rounds down.

### Season 6 (Jun 24, 2020 to about Aug 30, 2020)

Sources: https://ac.dragonest.com/en/announcement/detail/e7d08825a7f ; https://ac.dragonest.com/en/announcement/detail/e7bbbdee91b ; https://ac.dragonest.com/en/announcement/detail/e8433999c9e ; https://ac.dragonest.com/en/announcement/detail/e839af6e20f ; https://ac.dragonest.com/en/announcement/detail/e86d5a1c5d9 ; https://ac.dragonest.com/zh-cn/announcement/detail/e85c3f91183

- Dates: S6 and Chess/Senior Pass S6 opened with the Jun 24, 2020 maintenance.
- New pieces (Jun 24): Spider Queen: Insectoid / Hunter (Epic per the 2021 biography list, so 4). Thorn Predator: Insectoid / Assassin (Epic, 4).
- Removed: Shining Dragon, temporarily.
- Synergy changes: Dragon (6) → Dragon (3)/(5). Insectoid (4) added. Assassin (9) added. Warrior (9) reflects pure damage equal to armor. Human (3)/(6) give EXP Books after a win.
- New items: Dragonthorn Bracer, Teleport Dagger (back), Flicker Staff, Black Ice Lance, Holy Spirit Lance, Ice Platemail.
- New systems: in-game Championship (weekly, monthly and quarterly tiers inside each 8-week season; points are called "Runes"). Novice Tutorials mode.
- Matchmaking: you fight a real opponent each round in pairs. With an odd number of players, one fights a copy of a living player.
- Jul 30, 2020:
  - New pieces: Sacred Lancer: Glacier / Warrior (Legendary per biography list, 5). Lava Shaman: Cave Clan / Shaman. Cost unknown.
  - Skull Hunter reworked (summons a Porcupette).
  - New tiers: Cave Clan (6), HP scales with the player's missing HP. Shaman (4), hexes an enemy at start; Shamans killed by an enemy hex their killer into a random piece of the same cost and star.
  - EXP Books from Human can exceed the population cap (up to level 12).
  - The Source becomes Common (1-cost).
  - Item drops from Round 10: guaranteed 1 item per creep round won (2 in Round 40).
- Aug 7, 2020 (CN): Dragonest (Longyuan) took over self-publishing the CN version (previously the Tencent version).
- Aug 12: EXP from level 10 → 11 cut 40 → 20; level 11 → 12 cut 40 → 30 (except Quick Mode).

### Season 7 "Yggdrasil Ritual" (about Aug 30/31, 2020 to Oct 25, 2020)

Sources: https://ac.dragonest.com/en/news/detail/e8a318308ae ; https://ac.dragonest.com/en/news/detail/e8aa8e983a0 ; https://ac.dragonest.com/en/announcement/detail/1743db567e0 ; https://ac.dragonest.com/en/announcement/detail/e8f49c95941 ; https://ac.dragonest.com/en/announcement/detail/e933b6aad44 ; https://ac.dragonest.com/en/announcement/detail/e731df9ee61

- Dates: S7 Chess Pass "Yggdrasil Ritual" revealed Aug 29, 2020. Version 1.6.0 announced Aug 31, 2020 (image-only post). Exact season start unknown; S8 started Oct 25-30.
- Version: 1.6.0.
- New piece (Aug 30): Soul Devourer: Demon / Wizard (Uncommon per the 2024 rework note, so 2).
- Synergies: Wizard (4) activates the top tier of the single other active synergy. Assassin (6)/(9) apply to all allies. Egersis (6) added (enemies lose 12 armor). Knight shield chance 100% at round start.
- Economy:
  - When a player is eliminated, part of their gold is shared randomly among survivors.
  - Breaking a win streak of 5 or more pays the breaker gold equal to the streak holder's streak interest. Ties do not count from Oct 13.
  - EXP: Lv6→7 16→18, Lv7→8 24→28, Lv8→9 32→38, Lv9→10 40→48.
- Drodo Workshop (custom maps) launched Aug 27-30: Happy Battle, Smash Goblins, 3-Star Redaxe Chief.
- Sep 23: Storm Shaman 4 → 5 gold. Item drops Round 10-30 common only; Round 35-50 rare (Round 40 one of each).
- Oct 13: damage to the enemy player from summoned Insectoids halved.

### Season 8 (Oct 25-30, 2020 to Dec 21, 2020)

Sources: https://ac.dragonest.com/en/announcement/detail/e95c011d8c3 ; https://ac.dragonest.com/en/news/detail/e961f497dab ; https://www.taptap.cn/moment/68763320856871408 ; https://www.taptap.cn/moment/71209514719774514 ; https://ac.dragonest.com/en/announcement/detail/17592ad7663 ; https://ac.dragonest.com/en/announcement/detail/e9a8e8de72e ; https://ac.dragonest.com/en/announcement/detail/e9d63c80ba3

- Dates: Oct 25 maintenance added the new season chess player. CN "S8 season begins" posted Oct 30, 2020. Theme: CN 末日战场 ("doomsday battlefield").
- New piece: Avenge Knight: Human / Knight, Rare (3). Gains permanent ATK when it survives and wins.
- Returned: Stone Spirit: Spirit / Warrior, Common (1), new toss ability.
- Workshop: Clash of the Gods and Eternal Well added; Happy Battle maps removed (Nov 4).
- Nov 18: Egersis (6) gains "continue to survive for 5s after death" for allies killed by marked enemies. Warlock (6) steals the max HP and mana of killed enemies. Knight shield adds HP regen.
- Dec 2:
  - Creep drops: Rounds 10-35 give 1 common item; Round 40 gives 1 common and 1 rare; Round 45+ give 1 rare.
  - Item Shop (choose 1 of 3): common through Round 25, rare from Round 30.
  - EXP Lv10→11 20→24.
- PS4 release slipped from Oct 30 (announced Oct 30).

### Season 9 (Dec 21, 2020 to Feb 1, 2021)

Sources: https://ac.dragonest.com/en/announcement/detail/e9ffc85fd9c ; https://ac.dragonest.com/en/news/detail/ea05f855d1c ; https://www.taptap.cn/moment/89419248971023885 ; https://ac.dragonest.com/en/news/detail/ea294a4af9a ; https://ac.dragonest.com/en/news/detail/ea2fbeb9cc8 ; https://ac.dragonest.com/en/announcement/detail/ea2fde8bff6

- Dates: "Season 9 will start on December 21st" (Dec 16 notes). Pass theme set on Gaulke Mountain (CN 高尔克山).
- New piece (Dec 16): Cannon Granny: Goblin / Knight, Legendary (5). Fires cannons; damage became physical attacks on Jan 14, 2021.
- PS4 Early Access on PlayStation Store Dec 17, 2020 (Founder's Pack includes the S9 Senior Pass).
- Item mechanics dev log (Dec 28): item qualities Common/Rare/Epic/Legendary. The main creep drops exactly one item. Item Shop rare-only from Round 30.
- Experimental client build 2.2.1 (Dec 30, 2020).

### Season 10 "Dragon Blood Awakening" (Feb 1, 2021 to Mar 28, 2021)

Sources: https://ac.dragonest.com/en/announcement/detail/ea896325392 ; https://ac.dragonest.com/en/news/detail/ea4960e3ea0 ; https://ac.dragonest.com/en/news/detail/eaa044cea1c ; https://ac.dragonest.com/en/news/detail/ea90b585d4b ; https://ac.dragonest.com/en/news/detail/ea828922256 ; https://ac.dragonest.com/en/news/detail/ea79a800bde ; https://ac.dragonest.com/en/announcement/detail/eae402bab1f ; https://ac.dragonest.com/en/news/detail/eaa9d467c57

- Dates: "Season 10 will start on Feb 1, 2021." Version "Dragon Blood Awakening" released Jan 28. Full PS4 release the same day.
- New race: Pandaman (1)/(2)/(3). At round start, a 10%/25%/25% chance to "invite" a Pandaman, or to recruit a piece of the same race as a deployed piece (3 adds a 50% chance of an extra Pandaman). Split was 75% Pandaman / 25% other.
- New class: Martialist (1). 30% of non-pure damage taken by a Martialist becomes pure damage shared among same-race allies (cannot kill).
- New pieces:
  - Heaven Brew: Pandaman / Martialist, Uncommon (2). The only Pandaman in the shop.
  - Ember Blade: Pandaman / Assassin, Rare. Invite-only.
  - Thunder Trace: Pandaman / Mage, Rare. Invite-only.
- Returned (Feb 24): Phantom Queen: Demon / Assassin, Legendary (5). New ability Scream of Devil (confusion).
- Feb 24: Feathered (9) creates illusions on evade. Hunter (9) added (knockback).
- New systems: Chess Club (guilds, 16 members, 2000 candy to create). Reworked Starred Lineups.
- Workshop maps: Limited Legend (several 5-cost pieces banned per game, 7 remain) and Beloved One (asymmetric HP and gold by seat).
- Rules: Human and Mech rewards now trigger after any won battle, including creep rounds. Item drops when a player dies are spread more evenly.
- Esports: 2021 in-game Championship split into Spring, Summer and Autumn contests. Global Final planned for end of 2021.

### Season 11 "Lamp Spirit Legend" (Mar 28, 2021 to May 30, 2021)

Sources: https://ac.dragonest.com/en/announcement/detail/eb3e2dca436 ; https://ac.dragonest.com/en/announcement/detail/eb179e10ec7 ; https://ac.dragonest.com/en/news/detail/eb14382d96a ; https://ac.dragonest.com/en/news/detail/eb2e208e0c0 ; https://ac.dragonest.com/en/news/detail/eb3ae95ff6f ; https://ac.dragonest.com/en/news/detail/1784444d9af ; https://ac.dragonest.com/en/announcement/detail/eb986849337 ; https://ac.dragonest.com/en/activity/detail/eb9ecf1de1a ; https://ac.dragonest.com/en/announcement/detail/ebf5c339d7c

- Dates: S11 started 2021-03-28 16:00 UTC. Extended to May 30, 2021 15:59:59 UTC (announced May 20).
- Version: Experimental build 2.3.0 (Mar 2021).
- New pieces:
  - Space Walker: Beast / Martialist, Legendary (5). Builds "Chi" with attacks.
  - Jade General - Ji Xi (Apr 21): Pandaman / Shaman, Rare, invite-only.
- New tier: Martialist (2). Damage sharing extends to all allies.
- Removed: Ghost Kid, temporarily, from Ranked, Casual and some modes.
- Race change: Thorn Predator gains Egersis.
- Reworks:
  - Shaman (4): Shamans transform into a random piece costing +2 (2.5s transform, no items or synergies).
  - Pandaman: invite chance rises each round without an invite (pity), overall 12%.
  - Apr 21, Mage (9): enemies lose 95% Magic Resistance and are frozen on magic damage (freeze = magic damage / 400 seconds).
- Systems: Fragment System and Fragment Store (skins of removed pieces convert to "Fine Jigsaw"). Check-in system. Training Camp tutorial mode.
- Workshop "Personal Designer": build an 8-slot personal piece deck (5 fixed cost slots plus 3 flexible). Removed pieces such as Blight Sorcerer and Egersis Prophet are available there.
- Anniversary mode returned Apr 23 - May 6 with featured 10- and 15-gold pieces (Corruption Soulreaper, Ashes Hell Knight).

### Season 12 "Sleepless Night" (May 31, 2021 to Jul 25, 2021)

Sources: https://www.taptap.cn/moment/147415701856979572 ; https://ac.dragonest.com/en/announcement/detail/ebcf5eed7f6 ; https://ac.dragonest.com/en/news/detail/ebd554d8d0b ; https://ac.dragonest.com/en/news/detail/ebdf0a4a559 ; https://ac.dragonest.com/en/news/detail/179c1afa4fe ; https://ac.dragonest.com/en/announcement/detail/ec63452bea1

- Dates: S12 opened May 31, 2021 00:00 (UTC+8) with the new version. Pass preview "Sleepless Night" posted May 31.
- Version: Experimental build 2.4.0 (May 2021).
- New race: Horn (2). Summons a Horn Totem that is targeted first and takes only 1 melee or 2 ranged damage per hit.
- New pieces:
  - Sunchaser Servant: Horn / Assassin, Common (1).
  - Sunchaser Shaman: Horn / Shaman, Epic (4).
- Removed: Berserker, temporarily.
- Reworks: Glacier (6), all allies +50% attack speed plus stacking speed per attack (16 stacks). Shaman (4) target cost +1 or +2 (50/50 on CN).
- Mechanics:
  - Limited Legend in all normal modes: each game only 7 random 5-cost pieces plus Strange Egg can appear. Golden Heart of Mech, Pandaman and Shaman cannot bypass this on CN.
  - Player damage rework: piece quality damage (Epic/Legendary +1) plus star level (1/2/3) plus player level (Lv 9 +1, Lv 10+ +2).
- Systems: Medals (career, set, championship). 4v4 team casual mode (two teams of 4, match points by placement). AI players in Workshop. Piece biographies. Faster rounds: players who finish early move to preparation early.

### Season 13 "Ocean Voyage" (Jul 25/26, 2021 to Sep 26, 2021)

Sources: https://ac.dragonest.com/en/announcement/detail/ecbd6065c70 ; https://ac.dragonest.com/en/news/detail/17a89461a1d ; https://ac.dragonest.com/en/news/detail/17ad2f2b5ce ; https://ac.dragonest.com/en/announcement/detail/ed17b447159 ; https://ac.dragonest.com/en/announcement/detail/ed442031f31

- Dates: "S13 will start at 16:00, Jul 25, last 9 weeks, and end at 15:59:59, Sep 26 (UTC)."
- Version: Experimental build 2.5.0 (Jul 2021).
- New race: Civet (1). A Civet piece reaching 3 stars gives an extra 3-star copy. At battle start, each pair of identical pieces summons a Golem copy with 100% damage.
- New pieces:
  - Gem Artisan: Civet / Mech, Rare (3).
  - Cave Prodigy (猪猪牧师, CN "Shar"): Cave Clan / Priest, Epic (4). Resurrects the highest-cost unit killed that round. Aug 19.
- New tier: Priest (3). Adds "Red Essence": the player survives one fatal hit at 1 HP.
- Removed: Lava Shaman, temporarily (Aug 19).
- Cost changes Aug 19: Flame Wizard 3 → 2. God of War 1 → 3.
- Workshop: "2V2 Lite" (shared HP).
- Sep 2: Pandaman now invites only Pandamen (100/0). Rates 10%/20%/20%, plus 40% extra.

### Season 14 (Sep 26, 2021 to Nov 21/22, 2021)

Sources: https://ac.dragonest.com/en/announcement/detail/17bcd7f8e4f ; https://ac.dragonest.com/en/announcement/detail/ed885ead70d ; https://ac.dragonest.com/en/news/detail/17c0cabcd65 ; https://ac.dragonest.com/en/announcement/detail/ede1ec0c55c ; https://ac.dragonest.com/en/news/detail/eda18d73711

- Dates: "S14 will start at 16:00:00, September 26 and end at 15:59:59, November 21 (UTC), lasting 8 weeks." A Sep 30, 2021 English post is titled "S15 Senior Pass" (image-only); it conflicts with the S14 dates in the patch notes and is not resolved here.
- New race: The Greater (太古族). Official text: "When and only when the Greater Synergy is in effect, all allied pieces are immune to all control effects, and normal attacks have a 20% chance to cause 1.5 seconds spatial imprisonment." Jan 9, 2025 wording (CN): allies are immune to all negative effects when The Greater is the only synergy triggered, and its attacks no longer imprison.
- New pieces:
  - Dominator - Dominiel: The Greater / Assassin. Quality unknown.
  - Khan - Uriyang Modu (Oct 21): Horn, God / Druid, Legendary (5).
- New tier: Horn (4). Adds Sunchaser and Skydome Totems.
- New items: Blade Bracer, Crimson Heart, Stabbing Gloves, Kira's Wrath (gives the Kira synergy).
- New mode: Big Winner (image-only post). Closed again Nov 17.
- Mech (3)/(6): Hearts no longer drop in creep rounds. Jade Pipe renamed Conqueror's Armor; Lucky Coin renamed Lucky Pendant.

### Season 15 (Nov 22, 2021 to Jan 16, 2022)

Sources: https://ac.dragonest.com/en/announcement/detail/ee15bdb6dc3 ; https://ac.dragonest.com/en/announcement/detail/ee3bbed6227 ; https://www.taptap.cn/moment/211865222531516820 ; https://ac.dragonest.com/en/announcement/detail/ee799d8ae33

- Dates: "S15 will start at 16:00:00, November 22 and end at 15:59:59, January 16, 2022 (UTC), lasting 8 weeks." The CN PV has a ski-race theme.
- New piece: Redfox Swordsman - Forro·Vulpes: Beast / Assassin, Epic (4).
- Reworked: Shining Assassin, Epic → Legendary. Wind Field plus a piercing wind-blade passive.
- Race change: Lord of Sand loses Beast.
- Items: Bloody Club added. Frantic Mask, Infused Mask (renamed Bloodbath Skull) and Dragonbone Ballista (renamed Mania) reworked.
- New mode: Ranked (Team) 4v4, with Item Selection and "Duel" phases.
- Experimental Dec 6, 2021: Shadow Devil rework (Demon / Warlock, Rare); Venom becomes Uncommon; Dwarf (1) removed and Dwarf (2) reworked; Black Ice Lance → Burst Rifle and Holy Spirit Lance → Split Arrow (both grant Dwarf). Live date of these is unknown (the Dec 16 live notes are image-only).

### Season 16 (Jan 16, 2022 to Mar 13, 2022)

Sources: https://ac.dragonest.com/en/announcement/detail/eef2d466ddc ; https://ac.dragonest.com/en/announcement/detail/ef4c5e298dd ; https://ac.dragonest.com/en/announcement/detail/ef545b6b5c7

- Dates: "S16 will start at 16:00:00, January 16 and end at 15:59:59, March 13, 2022 (UTC), lasting 8 weeks."
- New piece: Dualplot - Taorive: Pandaman / Martialist, Legendary (5). Chi-stacking combo ability.
- Pandaman reworked to (1)/(2)/(4) at 15%/25%/30%; (4) adds a 20% chance of an extra Dualplot. Martialist reworked to (1)/(2)/(3), damage converted to pure damage over 5 seconds.
- Glacier became Glacier (5).
- Feb 10: new piece Bad Omen - Dutch Iron: Cave Clan / Prophet, Legendary (5). New class Prophet: 16 grids suffer "catastrophe" dealing 150 pure damage per second.

### Season 17 (Mar 13, 2022 to Jun 12/19, 2022)

Sources: https://ac.dragonest.com/en/announcement/detail/efb400e8120 ; https://ac.dragonest.com/en/announcement/detail/f0417ca81d9

- Dates: "S17 will start at 16:00:00, March 13, and end at 15:59:59, June 4, 2022 (UTC), lasting 12 weeks." Extended to Jun 12 (announced Apr 26). The S17 Chess Pass ended Jun 19 15:59:59.
- New piece: Resentful Murk: Egersis / Witcher, Common (1). Swaps places with the enemy nearest its mirror point.
- Witcher (2)/(4) reworked around counting Demon types; (4) gives all allies the Demon synergy and pure damage.
- Quality changes: Egersis Ranger Common → Uncommon; Shining Archer Uncommon → Common.
- Piece skins no longer have to be worn as a full series.
- Shop: after you buy a piece, the unbought pieces are excluded from the next 3 refreshes (including the end-of-round refresh).
- Fantasy mode rerun: Start From Zero (races and classes shuffled, more starting gold).
- Apr 26 (3rd anniversary):
  - Berserker returns reworked: Glacier / Warrior, Rare (3).
  - Evil Knight removed, temporarily.
  - Glacier becomes (2)/(4)/(6). Hunter (6)/(9) and Knight (6) reworked.
  - Fantasy Mode temporarily closed.

### Season 18 (Jun 19/20, 2022 to Sep 2022)

Sources: https://ac.dragonest.com/en/announcement/detail/f0ed9727023 ; https://www.taptap.cn/moment/287905050309690547 ; https://ac.dragonest.com/en/announcement/detail/1823e90b2bd

- Dates: "S18 will start at 16:00:00, June 19 (UTC)." CN: Jun 20 00:00 UTC+8.
- New piece: Otter Hunter: Beast / Hunter, Rare (3).
- Reworked: Razorclaw (permanent Claw Beast summon).
- Removed: Tusk Champion, temporarily.
- Cost changes: Werewolf 3 → 2; Heaven Brew 2 → 1.
- New fantasy mode: Chaos Arena. From Round 9, "Fate Badges" before each creep round give a piece an extra race or class.
- Cosmetics economy: Identify, Smelt, "Battle Loss" wear, Seals (stat trackers), unique Signature IDs, tradable chests, new currency Biscuit, Annual Senior Pass.
- Jul 27, 2022:
  - Reworks: Shining Dragon (Dragon, Feathered / Mage, 3 gold) and Frostblaze Dragon (Frost Fire Impact).
  - Returned: Shining Dragon, Lava Shaman, Tusk Champion, Evil Knight.
  - Lineup feature (CN 棋组): unlocked at Bishop 1; 55 public pieces plus 30 choice pieces; you pick 1 of 4 plans of 10 choice pieces.
  - Illusion Trial PvE mode. Real-time voice chat.
  - Creep Round 20 drops 2 common items. Round 25 choice upgraded to rare.

### Season 19 (Sep 8-12, 2022 to Dec 2022)

Sources: https://www.taptap.cn/moment/316694717930671422 ; https://www.taptap.cn/moment/318331444936051514 ; https://www.taptap.cn/moment/334458616067131373

- Dates: patch Sep 8, 2022 (UTC+8). S19 season pass opened Sep 12.
- New piece: Fire Spirit - Freya (火之灵-芙娅): Spirit / Shaman, 4 gold. Taunting ember form, then explodes and heals.
- New tier: Spirit (6) petrify.
- Mentor/apprentice system.
- Lineup feature removed and replaced by the Extended Chess Pool (CN 扩展棋库). From later notes, some pieces sit in an extended pool and rotate with the standard pool each season.
- Level 9 shop odds: 22/30/25/20/3 → 22/25/30/20/3.
- Oct 27, 2022 (CN):
  - New piece: Light Spirit - Licht (光之灵): Spirit / Priest, Legendary (5).
  - Spirit (2)/(4)/(6) reworked. Warlock (2)/(4)/(6) reworked (6: drains 3.5% max HP per hit).
  - Priest (1)/(2)/(4) reworked; (2)/(4) summon a "Holy Wand" structure that buffs adjacent allies.
  - Insectoid (4) can spawn up to cost +2.
  - Personal shop for trading.

### Season 20 (Dec 8-12, 2022 to Mar 12, 2023)

Sources: https://ac.dragonest.com/en/announcement/detail/f3152104210 ; https://ac.dragonest.com/en/announcement/detail/f3846031b4e ; https://www.taptap.cn/moment/362303527097205931

- Dates: "S20 Chess Pass will start on 12 December 2022 and end on 12 March 2023."
- New race: Soul Clan (2). Turns 2 grids into Soul Springs that grant HP/ATK based on Soul Clan totals.
- New pieces:
  - Soul Blade: Soul Clan / Witcher, Legendary (5).
  - Nightmare: Soul Clan / Warlock, Rare (3). This is the returned Egersis Prophet.
- Pool rotation: Tusk Champion, Berserker, Dark Spirit and Frostblaze Dragon to the Extended pool; Taboo Witcher, Nightmare, Rogue Guard and Soul Blade to the public pool.
- New casual feature: Rune Prophecy (pick in-game quests for rewards). Insurance Card (no rank loss for one game). Queen medal; Queen MMR shown as stars.
- Jan 11-12, 2023:
  - Returned: Ghost Kid as Soul Clan / Warrior, Rare (3).
  - New tier: Soul Clan (4).
  - Reworks: Dragon Knight, Fallen Witcher (corrupts allies into Fallen Witchers), Ember Blade, Shining Dragon.
  - Drodo Mart (in-game marketplace for cosmetics). Player level cap 600 → 1000.

### Season 21 (Mar 2023 to Jun 1, 2023)

Sources: https://ac.dragonest.com/en/announcement/detail/f4387dfb813 ; https://www.taptap.cn/moment/382587113478031802 ; https://www.taptap.cn/moment/397814740656587825

- Dates: Mar 8-9, 2023 patch: "Chess Pass [S20] ends; Chess Pass [S21] starts." Exact pass date unknown. Ended with the Jun 1 patch.
- New piece: Plume Warrior: Feathered / Martialist, Rare (3).
- Dualplot Legendary → Rare. Pandaman sell prices cut to 2/3/4.
- Martialist (2)/(4) reworked to execute low-HP enemies (4 adds a 15% chance to halve enemy HP).
- Egersis (6) reworked. Pandaman (4) simplified to 35%.
- Extended pool swap: out Shining Archer, Tusk Champion, Resentful Murk, Flame Wizard, Shadowcrawler, Evil Knight, Berserker, Storm Shaman, Space Walker. In Kira Mage, Wind Ranger, Nightmare, Fortune Teller, Thorn Predator, Cave Prodigy, Doom Arbiter, Rogue Guard.
- Preparation time changed for some rounds (e.g. Round 2 20s → 15s; Round 26 40s → 50s). Cumulative check-in.
- Apr 20, 2023: large item system rework. New items include Mana Crystal (储魔水晶, start with 40 mana), Ring of Life, Sunchaser Drum, Abyss Orb, Primeval Shard (dispel), Brood Mother's Egg (grants Insectoid), Kira's Grudge. Fury Mask and Bloodbath Skull removed. Stack-buying of passes allowed.

### Season 22 "Legend of the Seabed" (Jun 1, 2023 to Aug 31, 2023)

Sources: https://www.taptap.cn/moment/412981429916401987 ; https://www.taptap.cn/moment/410735685616533786 ; https://www.taptap.cn/moment/428215198331964969 ; App Store v2.20.2 (2023-07-12)

- Dates: patch Jun 1, 2023 (UTC+8). The App Store v2.20.2 note says "S22 Chess Pass will end on 9 September 2023", but the S23 pass is listed as starting Sep 4, 2023.
- Theme: CN PV title 关于海底的传说 ("about the legend of the seabed").
- New piece: 深潜之音 (Momola): Marine / Wizard, Uncommon (2). Song that puts enemies to sleep. English name unknown.
- Reworked: Siren. Marine (2)/(4)/(6) rebuilt around "Ocean's Blessing" damage absorption stacks.
- Cost change: God of War 3 → 1 (reworked).
- Club Battle (guild league). Item choice re-roll. New item crafting UI.
- Jul 13 (mid-season, v2.20.2):
  - Helicopter reworked.
  - Priest (2)/(4) reworked. If your board is not full, the player gains Green Essence when hit; (4) makes each Priest mind-control an enemy of equal or lower value.
  - Bad Omen class Prophet → Warlock. Fortune Teller Rare → Common.

### Season 23 "Doomsday Rampage" (Aug 31 / Sep 4, 2023 to Dec 3, 2023)

Sources: https://www.taptap.cn/moment/445890468023109485 ; https://www.taptap.cn/moment/443358120387805377 ; https://www.taptap.cn/moment/463667893125516991 ; App Store v2.21.2 and v2.22.2

- Dates: "S23 Chess Pass will start on 4 September 2023 and end on 3 December 2023." Patch Aug 31 (v2.21.2, 2023-08-30 UTC). CN PV theme 末日狂飙.
- New piece: Empress Light (晖光女帝): Civet / Shaman, Legendary (5). Creates a copy of herself; counts as two same pieces.
- Reworked: Gem Artisan. Civet (1)/(2). Shaman (2)/(4)/(6) with "Illusion Form" transformations.
- Strong vs weak dispel introduced.
- Legendary pool: all Extended-pool legendaries move to standard, and each game draws 8 random legendaries as the only available ones (Solo/Duo/Team casual, ranked, Triple Fun).
- Oct 19 (mid-season, v2.22.2):
  - Class Traits added for Knight, Shaman, Witcher, Warrior, Hunter, Mech, Mage, Warlock, Assassin, Martialist. Examples: Assassins leap behind the enemy line at battle start; Warlocks convert 20% HP into 40% mana.
  - Grand Herald reworked. Mech (3) only.
  - "Lucky card" subscription (10 Rune Keys per day).

### Season 24 "Arcade Invasion" (Nov 30 / Dec 4, 2023 to Mar 4, 2024)

Sources: https://www.taptap.cn/moment/478943593860434239 ; https://www.taptap.cn/moment/478528114662572265 ; https://www.taptap.cn/moment/494184701536239659 ; App Store v2.23.2 and v2.24.2

- Dates: patch Nov 30, 2023 (UTC+8). "Chess Pass S24 will end at 00:00 on March 4, 2024." CN PV theme 电玩入侵.
- New piece: Eclipse of Darkness: Egersis / Priest, Rare (3).
- Egersis rebuilt as (3)/(6)/(9) "Plague". Non-Egersis pieces on both sides lose 25%/50% max HP; at (9) infected pieces that die are revived as Egersis Warriors or Archers for the enemy.
- All Soul Clan pieces changed to Egersis (CN).
- Reworked: Rogue Guard. Ghost Kid Rare → Uncommon.
- Permanent "partner card" pass subscription (auto-activates and grants pass level 100 each season).
- Jan 11, 2024 (mid-season, v2.24.2):
  - Blight Sorcerer returns as Egersis. CN note: with this, every removed piece has returned.
  - "Item 3.0" system: item qualities Common/Uncommon/Rare/Epic/Legendary; Epic and Legendary only from chests.
  - Creep rounds give an item chest (intact on win: choose 1 of 4 with the last hidden; damaged on loss: random item).
  - Reforge: sacrifice items whose quality values sum to 1-2/3-4/5-6/7-8/9+ for a Common through Legendary chest; one reforge per creep round.
  - Item blocking: passed or reforged items stop appearing in chests.
  - Dragon (5) applies to all allies (100 starting mana). Scryer class Shaman → Mage.

### Season 25 "Space Celebration" (Feb 29 / Mar 4, 2024 to May 30, 2024)

Sources: https://www.taptap.cn/moment/511938695213878579 ; https://www.taptap.cn/moment/511120920417406539 ; App Store v2.25.2 and v2.26.3

- Dates: patch Feb 29, 2024 (UTC+8). "S25 Chess Pass will start on 4 March 2024."
- New race: Half-Orc (2)/(4). Charge-impact dash at battle start; at (4) every melee move is a charge.
- New pieces:
  - Sandbound Aegis (活砂圣女): Half-Orc / Warrior, Uncommon (2).
  - Evernight Watcher: Half-Orc / Assassin, Epic (4).
- Unicorn gains Half-Orc. Sunchaser Servant reworked. Hunter (3)/(6)/(9) reworked (extra attack chance).
- Creep drops: Round 10 chest Common → Uncommon (no creep drop); Round 15 Uncommon → Rare.
- Apr 10 (v2.26.3): Dualplot and Ember Blade reworked. Kira (2) "devour" (eats adjacent cheaper allies before battle for 60% of their HP/ATK). Pandaman (4) changed.

### Season 26 "Mountain-Sea Fantasy" (May 30, 2024 to Aug 29, 2024)

Sources: https://www.taptap.cn/moment/544943054058947145 ; https://www.taptap.cn/moment/544229813389364465 ; https://www.taptap.cn/moment/560161094530763707 ; App Store v2.27.2 and v2.28.2

- Dates: patch May 30, 2024 (UTC+8); v2.27.2 on 2024-05-29 UTC. CN PV theme 山海幻境.
- New piece: Mountain Chief (山魁·狩): Beast / Warrior, Legendary (5).
- Reworked: Soul Blade, Otter Hunter. Beast (6) adds stacking physical vulnerability.
- New items: Azees Gaze, Sigmar Hammer. Six items rotated out.
- Team modes: from Round 10, team item drops based on survivors, with dice rolls on contested picks. Mentorship reworked.
- Jul 11 (mid-season, v2.28.2):
  - Grimtouch and Soul Breaker (刺魂者; bounty gold on first kill) reworked.
  - Demon synergy rebuilt as "Demon Power" (pure base attacks, +10% per other Demon).
  - Warlock class trait. Witcher (4) makes all allies Demons. Kira devour can trigger Insectoid.
  - Divinity Imprint item (grants Divinity).

### Season 27 (Aug 29, 2024 to Nov 28, 2024)

Sources: https://www.taptap.cn/moment/577861842978736116 ; https://www.taptap.cn/moment/595622426532907418 ; App Store v2.29.2 and v2.30.2

- Dates: patch Aug 29, 2024. Theme name not found in text (the pass chessboard is 格斗潮流·燃至巅峰, "Fighting Trend").
- New piece: Underworld Enforcer (冥界执法官·砾): Egersis / Hunter, Rare (3).
- Reworked: Warpwood Sage, Shining Dragon (Scale Orb), Spider Queen.
- New items: Four-Leaf Clover, Crimson Ripper, Grand Magus Staff (+1 ability level).
- Armor damage-reduction formula changed. Martialist execute chance now depends on damage dealt.
- Player-interaction items: hammer, frying pan, like/insult.
- Oct 17 (mid-season, v2.30.2):
  - Reworks: Venom, Taboo Witcher (Mana Storm), Storm Shaman.
  - Quality changes: Taboo Witcher Common → Epic; Storm Shaman Epic → Legendary; Space Walker Legendary → Epic; Ember Blade, Thunder Trace, Jade General and Dualplot Rare → Epic.
  - Pandaman becomes (1)/(2). Feathered no longer gives evasion to non-Feathered allies. Shaman and Witcher reworked.
  - New mode: Happy Chess Hall (CN 欢乐棋馆): daily window, Donut entry fee, 2 lives per stage. Closed Jul 24, 2025.

### Season 28 "Chosen Radiance" (Nov 28, 2024 to Jan 23, 2025)

Sources: https://ac.dragonest.com/zh-cn/announcement/detail/fdb58e28d02 ; https://www.taptap.cn/moment/610909071989541448 ; https://www.taptap.cn/moment/610855780140190845 ; https://www.taptap.cn/moment/615860054091894386 ; App Store v2.31.2

- Dates: patch Nov 28, 2024 (UTC+8; delayed from the original slot). CN theme 天选圣辉.
- Season format: "Changed the season cycle for S28 to two months per season." The Chess Pass cycle became one month (S28 first and second halves; the second half started Jan 1, 2025). Pass point gain tripled. Staged Senior Pass and annual pass sales closed; a new annual plan added.
- New piece: Primeval Whisper - Fortunester (暮林旧神-福瑞斯特): Divinity, Half-Orc / Druid, Legendary (5).
- Reworked: Soul Devourer, Uncommon → Epic (Annihilation). Grimtouch Epic → Common (casts at battle start).
- Marine and Beast reworked.
- New mode: Holyselect War (圣选战争), built on Relics.
- Dec 12, 2024: "圣物加入单人休闲模式/单人排位模式". Relics added to Solo Casual and Solo Ranked.
- Closed: Medal system, mentorship, "lucky card" sales. Removed from mode select: Training Camp, Novice tutorial, Fantasy (梦境) modes.

### Season 29 "Silk Trend" (Jan 23, 2025 to Mar 27, 2025)

Sources: https://ac.dragonest.com/zh-cn/announcement/detail/195ef48ba55 ; https://www.taptap.cn/moment/631178459455947490 ; https://ac.dragonest.com/zh-cn/announcement/detail/fdb58c64f55 ; https://ac.dragonest.com/zh-cn/announcement/detail/fdb589813b0 ; App Store v2.32.2

- Dates: patch Jan 23, 2025 (UTC+8); v2.32.2 on 2025-01-22 UTC. CN theme 衣锦潮行.
- New piece: Carnation Elf - Lynn (花灵-莉): Feathered / Wizard, Common (1). Mar 13: ability replaced with Thorn Chain, cast at battle start.
- Assassin (3)/(6)/(9) and Divinity (2)/(4)/(6) reworked. Wizard no longer affects Druid.
- Summons get duration limits (e.g. Treants 30/30/20s, Insectoid heirs 15-20s).
- Creep drops: Round 10 adds a random common item; Round 15 chest becomes common plus a random uncommon item.
- New mode: Back to 2019 (nostalgia; 55 pieces from the 2019 version, random item drops and crafting). Closed Mar 27, 2025.
- Storage box. Holyselect War mode closed.
- Feb 13: new items Ancient Dragon Horn and Magic Cloak. Several items switch from % ATK to flat ATK.

### Season 30 "Tiger Ink" (Mar 27, 2025 to May 29, 2025)

Sources: https://ac.dragonest.com/en/announcement/detail/fdb57c9bb46 ; https://www.taptap.cn/moment/653967693761741630 ; https://ac.dragonest.com/en/announcement/detail/fdff744ea83 ; https://ac.dragonest.com/en/announcement/detail/fe56eef5113 ; App Store v2.33.2

- Dates: patch Mar 27, 2025 (UTC+8); v2.33.2 on 2025-03-26 UTC. The second half of the S30 Chess Pass started May 1, 2025. CN theme 虎墨沉香.
- New race: Night Demon (2). Fear when more than 1 enemy is nearby, 6s internal cooldown.
- New piece: Mist Phantom King·Wuril: Night Demon / Assassin. Quality not stated. HP 500/1000/2000.
- Desperate Doctor gains Night Demon (removed again Nov 2025). God of War given base ATK.
- New items: Greedy Gloves, Spell Prism, Herald Crystal.
- New relics: Strange Egg Encounter, Beast Night March, Chaos Contract. Jack's Treasure and Moree's Magical Elixir rotated out.
- Battle and economy:
  - Removed the 25% base-attack damage reduction vs creeps.
  - EXP to next level: Lv3 2→4, Lv4 4→8, Lv5 8→16, Lv6 16→24, Lv7 32→36, Lv9 56→60.
- Martialist class trait removed. Priest trait: immune to Priest mind control.
- May 8: broad balance pass (Mountain Chief, Devastator, Werewolf, Doom Arbiter, others).

### Season 31 "Dopamine Season" (May 29, 2025 to Jul 24, 2025)

Sources: https://ac.dragonest.com/en/announcement/detail/1971f3f0799 ; https://www.taptap.cn/moment/676840463298203451 ; https://www.taptap.cn/moment/679393631320148637 ; App Store v2.34.2

- Dates: patch May 29, 2025 (UTC+8); v2.34.2 on 2025-05-28 UTC. CN theme 多巴胺季.
- New race: Ancestor (2)/(4). (2): Ancestors steal (50 + 2% max HP) HP every 5s. (4): healing over 100 deals that much pure damage nearby (cap 300, 5s cooldown).
- New piece: Goddess of War·Valkyrie: Ancestor / Knight, Rare (3).
- Soul Breaker gains Ancestor.
- Reworked: Soul Reaper Epic → Legendary (Reaper's Scythe). Argali Knight Rare → Legendary (Blessing of King). Nightmare Rare → Epic.
- Chess pool:
  - "The Extended Chess Pool has been removed." Race/Class Blocking added: spend gold to block one race or class from your shop, one at a time.
  - Legendary draw per game 8 → 10, with 5 hidden; the first player to find and buy a hidden one gets 5 gold.
- New relic: Fishing Prohibited (free race/class blocking while active).
- New items: Demon Gloves (all damage pure), Twin Fangs.
- Creeps drop gold when defeated. EXP: Lv6 24→32, Lv7 36→48, Lv8 48→56, Lv9 60→64.
- Divinity (6) reworked.
- Jun 5: large balance pass. Dark Spirit Legendary → Rare. Warrior (6)/(9) armor only for Warriors.

### Season 32 "From the End" (Jul 24, 2025 to Sep 25, 2025)

Sources: https://www.taptap.cn/moment/697132391289848579 ; https://www.taptap.cn/moment/696795904157418971 ; App Store v2.35.2

- Dates: patch Jul 24, 2025 (UTC+8; delayed); v2.35.2 on 2025-07-23 UTC. CN theme 来自终末.
- New piece: Dark Lord·Erebus: Ancestor / Warlock, Uncommon (2).
- New items: Hat of Clarity (piece value +1), Steelbone Shield, Nullrot (strips race/class buffs), Fire Spirit's Last Wish (revive once per round).
- New ability type: Battlecry, with a Battlecry phase before combat. Carnation Elf, Grimtouch and Mist Phantom King moved to Battlecry.
- Pure damage no longer triggers lifesteal. Marine no longer absorbs pure damage.
- Dwarf (2), Hunter (9) reworked. Kira Mage and Dwarf Sniper reworked.
- 3D custom game rooms. Happy Chess Hall closed.

### Season 33 "The Sleeping Dragon Emerges" (Sep 25, 2025 to Nov 27, 2025)

Sources: https://www.taptap.cn/moment/719875800060399245 ; https://www.taptap.cn/moment/719237635226207415 ; App Store v2.36.2

- Dates: patch Sep 25, 2025 (UTC+8); v2.36.2 on 2025-09-24 UTC. CN theme 卧龙出山 (Three Kingdoms theme; a "crown chess player" Diaochan was previewed).
- New piece: Penitent Bishop·Mammon: Ancestor, Demon / Priest, Rare (3).
- New items: Priest's Crown (mind-controls an enemy summon), Light Orb.
- Reworked:
  - The Scryer: new passive [Divination], picks an available ability and lowers its mana cost by 25%.
  - Bad Omen, Shining Assassin, Shadowcrawler (Rare → Epic), Umbra, Soul Blade, Resentful Murk, Cannon Granny.
- Pandaman invite pity. Wizard class trait: never treated as a priority target.
- New mode: Summit Clash (紫禁之巅). Qualified players' Solo Ranked games during set daily windows also score "Summit Points".
- Matchmaking Lobby (no match-accept step). Top 1 leaderboard. In-game damage numbers.

### Season 34 "Winter Snowscape" (Nov 27, 2025 to Jan 29, 2026)

Sources: https://www.taptap.cn/moment/742806518088339047 ; https://www.taptap.cn/moment/742452060066156018 ; https://www.taptap.cn/moment/747854448776187466 ; App Store v2.37.2

- Dates: patch Nov 27, 2025 (UTC+8); v2.37.2 on 2025-11-26 UTC. CN theme 冬日雪景.
- New piece: Unknown Horror - ???: Warlock / Night Demon, Common (1). Channels pure damage on a grabbed enemy.
- New items: Warlord Blade (+50% vs Demon and Egersis), Egersis Pact.
- New relics: Thrill Box (transform bench pieces), Veil (hide board; skip a battle after taking damage).
- Egersis:
  - Plague reworked again.
  - Egersis pieces can no longer be bought from the shop; they only come from the Egersis Pact item or the Thrill Box relic.
- Mech: Golden Heart of Mech refreshes give "Golden Vouchers"; pieces it shows can only be bought with them.
- Relics per game 8 → 10. Opening a relic chest is now a choice of 1 of 4, with no random option.
- Attack-interval formula changed. Dec 11: mana gained when hit is now 20% of damage taken (cap 20), no randomness.
- Reworked or requalified: Evil Knight (Common), Underworld Enforcer (Uncommon), Ghost Kid (Rare), Blight Sorcerer (Epic), Nightmare, Thorn Predator, Soul Reaper, Egersis Ranger (Rare).
- Club Battle upgraded to Club Tournament (Starfire and Wildfire weeks). Title system. "Drodo Supply Station" replaced Surprise Card.

### Season 35 "物华弥新" collaboration (Jan 29, 2026 to about Mar 26, 2026)

Sources: https://www.taptap.cn/moment/765628521560147076 ; https://www.taptap.cn/moment/765523277920600459 ; App Store v2.38.2

- Dates: patch Jan 29, 2026 (UTC+8); v2.38.2 on 2026-01-28 UTC. TapTap posts call it the S35 "物华弥新" collaboration version (the pass includes a "Sword of Goujian" skin).
- New race: Watcher (2). Targeted abilities and item abilities against enemies hit +1 target.
- New piece: Hawk - Falcon (猎鹰-隼): Watcher / Mage, Common (1).
- Resentful Murk gains Watcher.
- New items: Fate Reversal, Dormant Crystal (+1 reforge value).
- New relics: Dual-Faced Coin, Time Rewind.
- Player damage rule: damage to the enemy player = (piece value / 3) + 1, rounded down. Temporary units count at half value.
- Battle speed-up: from second 31, combat runs at 3x, so a round lasts at most about 40 seconds.
- Knight, Warlock (lifesteal works on pure damage), Feathered, Insectoid and Glacier reworked.
- New mode: Drodo Brawl (多多大乱斗). 58 pieces from skin series, 9 new races (e.g. Dimension, Chosen Radiance, Childlike Innocence) and 11 new classes (e.g. Tamer, Fighter, Frenzied Warrior). New mechanics in this mode: Talent System and Enhance Items.
- Drodo Mart returned, linked to the auction house. Workshop mode closed. Harsher penalties for repeated surrenders in Solo Ranked.

### Season 36 (inferred start Mar 26, 2026)

Sources: App Store v2.39.2 (2026-03-26); TapTap sidebar post title "提前看！S36新装备&新圣物&新天赋 即将上线" (seen on https://www.taptap.cn/moment/511938695213878579 page sidebar). The CN official feed has no patch notes for Feb 10 to Aug 11, 2026.

- Dates: inferred from the two-month cadence and the v2.39.2 release date. Season number for this patch is an inference; the S36 preview post confirms "new items, new relics, new talents" for S36.
- New piece: Ronin - Nue: Watcher / Assassin, Rare (3). Two-stage stun strike scaling with target max HP.
- Talent System added to the main game. In the preparation phase of set rounds, pick 1 of 2 talents per tier:
  - Round 5: Relentless Push (+1 gold on win) / Quiet Reserve (+1 gold on loss).
  - Round 15: Honor the Worthy (owned pieces cost 1 less) / Elite Forces (shop odds as if 1 level higher).
  - Round 25: Siphon (player damage gives 25% lifesteal) / Cocoon (player takes 25% less damage).
  - Round 35: Odd Call (much higher Strange Egg odds) / Limit Break (level cap raised to 16).
  - From Sep 2026: from Round 45 all talents unlock and apply for every player.
- New item: Magicka Pendant (physical damage converted to magic).

### Season 37 (inferred start May 27, 2026)

Source: App Store v2.40.2 (2026-05-27).

- Dates: inferred, as above.
- New piece: Duke - Janus: Legendary (5). No race; gains three random different classes at the start of each battle. Ability Spotlight (blind; non-permanent enemies take % max HP pure damage).
- Rule: damaged item chests no longer block items.
- Dynamic chess pool counts in 8-player games: Common 18-21, Uncommon 15-18, Rare 12-15, Epic 9-12, Legendary 9-12 copies (excluding Pandaman and Egersis).

### Season 38 (about Jul 29, 2026 to Sep 23, 2026)

Sources: App Store v2.41.2 (2026-07-29); https://www.taptap.cn/moment/842038650718913477 (S38 second-half pass, Aug 27, 2026); https://www.taptap.cn/moment/836649532157594057

- Dates: S38 second-half pass posted Aug 27, 2026, consistent with a late-July start. Exact start not stated.
- New piece: The Exile - Slayardo (流亡者): Epic (4). Armed Stance plus Glacier (the note lists "Class: Armed Stance, Synergy: Glacier"). Attacks can "shatter" into illusions, which can shatter further.
- New synergy: Armed Stance. Pieces become Warriors in the front two rows and Assassins in the back two rows.
- New relics: Double Luck (buying an owned piece can give a 2-star directly; 22/20/18/16/14% by quality, changed to 25/22/19/16/13% on Aug 13) and Weighted Dice (1-2 gold refund on refreshes with no owned pieces).
- Aug 13: Heaven Brew Uncommon → Common. Thunder Spirit class Mage → Shaman. Water Spirit gains Feathered. Witcher (4) converts all damage to pure.

### Season 39 (Sep 23, 2026 to present)

Sources: https://www.taptap.cn/moment/851466287933555545 ; https://www.taptap.cn/moment/853950088999862935 ; https://www.taptap.cn/moment/859490366116596623 ; App Store v2.42.2 (2026-09-23)

- Dates: patch Sep 23, 2026 (UTC+8); v2.42.2 on 2026-09-23 UTC. S39 first-half pass posted Sep 29.
- New piece: Fel Titan - Zelatun (邪能泰坦-泽拉图恩): Demon / Warlock, Legendary (5). Battlecry Demon Gate summons random Demon pieces.
- New relic: Myriad Arms Ring (20 Spiral Forgers to swap items for random same-quality items).
- Demon Power stacks count live (cap 7), and enemy Witchers add stacks. Witcher now interferes with the enemy's Demon count, which can break or enable Wizard (4).
- Mana-gain modifiers apply to all mana sources. Battlecry only fires once per combat. Knight trait shield 3s → 4s.
- Oct 15, 2026: latest balance patch in the crawl.

---

## Mechanics timeline

| Mechanic | Introduced | How it worked | Source |
|---|---|---|---|
| Chess Pass (battle pass) and account levels | S1, by Jun 6, 2019 | 200 tiers from matches and weekly quests; paid tier gave 100% candy and EXP boost | https://www.droidgamers.com/news/auto-chess-gets-season-1-a-battle-pass-and-an-account-level-system-in-a-massive-update/ |
| Novice Mode, Fantasy Mode, custom games | Jun 27, 2019 | Fantasy (梦境) rotating rule modes, Fri-Sun at first, daily from Mar 2020; Fantasy closed Apr 2022, removed from mode select Nov 2024 | https://ac.dragonest.com/en/announcement/detail/e595ccba175 |
| Divinity race (gods) | Jun 27, 2019 | God of Thunder and God of War; cooldown reduction for allies; reworked Apr 2020 and Jan 2025 | same |
| Strange Egg | Jul 9, 2019 (CN) | Special 5-cost piece that can appear at any level; later the only fixed legendary under Limited Legend | https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbb281 |
| Item inheritance from eliminated players | Jul 24, 2019 (CN) | Each item of an eliminated player has a 50% chance to go to a random survivor; rate later scaled by players left | https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbabca |
| Duo (2v2) | Casual permanent Sep 20, 2019; Ranked Duo with S3 (Oct 2019) | Shared pieces with a partner | https://ac.dragonest.com/en/announcement/detail/e595ccba571 |
| Wizard class | Oct 2019 (S3) | Synergies needing 4+ pieces need 1 fewer; later Wizard (4) activates the top tier of the single other synergy | https://ac.dragonest.com/zh-cn/announcement/detail/e595ccbc558 |
| Creep-round item choice | Oct 18, 2019 | Win/draw: pick 1 of 3; loss: 1 random | https://ac.dragonest.com/en/announcement/detail/e595ccba48e |
| Spectator mode, Auto toggle | Oct 2019 (PC Aug 2019) | Watch friends; AI plays your turn | same |
| Experimental Server | Nov 2019 | Separate test client for upcoming pieces | https://ac.dragonest.com/en/news/detail/e595b9fba7f |
| Piece skins | Dec 12, 2019 | Skins per star level; full set from 9 copies | https://ac.dragonest.com/en/announcement/detail/e595ccba73d |
| Trio / Quick mode | Jan 12, 2020 | Faster mode; renamed Quick Jan 20 | https://ac.dragonest.com/en/announcement/detail/16f93644028 |
| Multi-ability piece (The Scryer) | Mar 19, 2020 (CN) | Casts one of several abilities based on the enemy's largest synergy | https://ac.dragonest.com/zh-cn/announcement/detail/e69073953b1 |
| Gacha abolished | Apr 2020 | Coupons converted to currency | https://ac.dragonest.com/en/announcement/detail/e6e08d25aae |
| Insectoid race | Apr 27, 2020 (S5) | Duplicate dies → summon a random Insectoid by cost | https://ac.dragonest.com/en/news/detail/e6eb0c611d5 |
| Staged Senior Pass purchase | Apr 27, 2020 | Buy pass in level stages; staged buying closed Nov 2024 | https://ac.dragonest.com/en/announcement/detail/e7151628289 |
| Anniversary mode | May 2020, Apr 2021 | Removed pieces return; featured 10/15-gold pieces | https://ac.dragonest.com/en/activity/detail/eb9ecf1de1a |
| Green Essence / Heart of Mech consumables | Jun 4, 2020 | Priest: player heal item; Mech: shop refresh items | https://ac.dragonest.com/en/announcement/detail/e78f15f2282 |
| In-game Championship | Jun 24, 2020 (S6) | Weekly/monthly/quarterly cups inside an 8-week season; "Runes" as points | https://ac.dragonest.com/en/announcement/detail/e7bbbdee91b |
| Paired matchmaking each round | Jun 24, 2020 | Players fight a real opponent in pairs; odd player fights a copy | https://ac.dragonest.com/en/announcement/detail/e7d08825a7f |
| Human EXP Books / overcap | Jun-Jul 2020 | Humans give EXP Books on wins; Books can exceed the level cap (to 12) | https://ac.dragonest.com/en/announcement/detail/e8433999c9e |
| Gold from eliminated players; streak-break bounty | Aug 30, 2020 | Part of an eliminated player's gold is shared; breaking a 5+ streak pays the streak interest | https://ac.dragonest.com/en/announcement/detail/1743db567e0 |
| Drodo Workshop (custom map modes) | Aug 27-30, 2020 | Happy Battle, Smash Goblins, Limited Legend, Beloved One, Personal Designer, 2V2 Lite and others; closed Jan 2026 | https://ac.dragonest.com/en/news/detail/ea79a800bde |
| Guaranteed creep item drops | Jul-Dec 2020 | One item from the main creep per round; common early, rare late | https://ac.dragonest.com/en/news/detail/ea294a4af9a |
| Pandaman invite race | Jan 27, 2021 (S10) | Chance per round to get a Pandaman not sold in the shop (except Heaven Brew) | https://ac.dragonest.com/en/news/detail/eaa044cea1c |
| Martialist class | Jan 27, 2021 | Damage sharing as pure damage; later execute mechanic (2023) | https://ac.dragonest.com/en/news/detail/ea5f3c6fc2f |
| Chess Club | Jan 27, 2021 | Guilds of 16; Club Battle Jun 2023; Club Tournament Nov 2025 | https://ac.dragonest.com/en/news/detail/ea828922256 |
| Fragment System | Mar 24, 2021 | Skin fragments and "Fine Jigsaw" from removed-piece skins | https://ac.dragonest.com/en/news/detail/eb3ae95ff6f |
| Limited Legend in normal modes | May 31, 2021 (S12) | Only 7 random 5-cost pieces (+ Strange Egg) per game; 8 random legendaries from Aug 2023; 10 with 5 hidden from May 2025 | https://ac.dragonest.com/en/announcement/detail/ebcf5eed7f6 |
| Player damage formula 1 | May 31, 2021 | Quality bonus + star level + player level | same |
| 4v4 team mode | May 31, 2021 (casual); Ranked Team Nov 2021 | Two teams of 4, match points by placement | https://ac.dragonest.com/en/news/detail/ebd554d8d0b |
| Medals | May 31, 2021 | Career/set/championship medals; closed Nov 2024 | https://ac.dragonest.com/en/news/detail/ebdf0a4a559 |
| Horn race totems | May 31, 2021 | Totem targeted first, takes 1 melee or 2 ranged damage per hit | https://www.taptap.cn/moment/147415701856979572 |
| Civet race | Jul 22, 2021 | Extra 3-star copy; Golem copies for identical pairs | https://ac.dragonest.com/en/announcement/detail/ecbd6065c70 |
| Red Essence | Aug 19, 2021 | Priest (3): survive one fatal hit at 1 HP | https://ac.dragonest.com/en/announcement/detail/ed17b447159 |
| The Greater race | Sep 23, 2021 | Control immunity; spatial imprisonment on hit | https://ac.dragonest.com/en/announcement/detail/ed885ead70d |
| Prophet class | Feb 10, 2022 | 16 catastrophe grids deal pure damage; Bad Omen moved to Warlock Jul 2023 | https://ac.dragonest.com/en/announcement/detail/ef4c5e298dd |
| Mix-and-match skins | Mar 13, 2022 | Skins no longer locked to a full series | https://ac.dragonest.com/en/announcement/detail/efb400e8120 |
| Shop exclusion of passed pieces | Jun 2019; Mar 2020; Mar 2022 | Pieces not bought skip the next refresh(es); 3 refreshes from Mar 2022 | https://ac.dragonest.com/en/announcement/detail/efb400e8120 |
| Chaos Arena fate badges | Jun 19, 2022 (S18) | From Round 9, badges add a race or class to a piece | https://ac.dragonest.com/en/announcement/detail/f0ed9727023 |
| Identify / Smelt / Seals (tradable cosmetics) | Jun 19, 2022 | Tradable attributes on skins; Drodo Mart market Jan 2023 | same |
| Lineup plan (棋组) | Jul 27, 2022 | 55 public + 30 choice pieces; pick a 10-piece plan; removed Sep 2022 | https://ac.dragonest.com/en/announcement/detail/1823e90b2bd |
| Illusion Trial | Jul 27, 2022 | PvE challenge with unlockable templates and affixes | same |
| Extended Chess Pool | Sep 8, 2022 (S19) | Some pieces sit in an extended pool rotated each season; removed May 2025 | https://www.taptap.cn/moment/316694717930671422 |
| Rune Prophecy | Dec 8, 2022 | Optional in-game quests in casual | https://ac.dragonest.com/en/announcement/detail/f3152104210 |
| Soul Clan springs | Dec 8, 2022 | Grids that buff pieces placed on them; race folded into Egersis Nov 2023 | same |
| Class Traits | Oct 19, 2023 | Each class gets a built-in trait (e.g. Assassins leap behind) | https://www.taptap.cn/moment/463667893125516991 |
| Item 3.0: chests, reforge, blocking | Jan 11, 2024 | Creep rounds give item chests; reforge items into chests; passed items blocked | https://www.taptap.cn/moment/494184701536239659 |
| Kira devour | Apr 10, 2024 | Kira eats adjacent cheaper allies before battle | App Store v2.26.3 |
| Demon Power | Jul 11, 2024 | Pure base attacks scaling with Demon count; live stacks from Sep 2026 | App Store v2.28.2 |
| Two-month seasons, monthly pass halves | Nov 28, 2024 (S28) | Season length 2 months; pass split into first and second halves | App Store v2.31.2 |
| Relics (圣物) | Holyselect War mode Nov 28, 2024; Solo Casual and Ranked Dec 12, 2024 | Relics picked from relic chests during a match (choose 1 of 4 from Nov 2025); 10 per game from Nov 2025; rotated in and out by patch | https://www.taptap.cn/moment/615860054091894386 |
| Back to 2019 mode | Jan 23 - Mar 27, 2025 | 2019 piece set and rules | App Store v2.32.2 |
| Race/Class Blocking | May 29, 2025 | Pay gold to block one race or class from your shop | App Store v2.34.2 |
| Battlecry | Jul 24, 2025 | Pre-combat phase for Battlecry abilities | App Store v2.35.2 |
| Summit Clash | Sep 25, 2025 | Solo Ranked in daily windows also scores Summit Points | App Store v2.36.2 |
| Shop-locked Egersis | Nov 27, 2025 | Egersis pieces only from Egersis Pact or Thrill Box | https://www.taptap.cn/moment/742806518088339047 |
| Player damage formula 2; battle speed-up | Jan 29, 2026 | Damage = value/3 + 1; 3x speed after 31s | https://www.taptap.cn/moment/765628521560147076 |
| Talent System | Drodo Brawl mode Jan 29, 2026; main game Mar 26, 2026 | Pick 1 of 2 talents at Rounds 5/15/25/35; all unlock at Round 45 from Sep 2026 | App Store v2.39.2 |
| Dynamic piece counts | May 27, 2026 | Copies per piece random within a quality range | App Store v2.40.2 |
| Armed Stance | Jul 29, 2026 | Class changes with row position (Warrior front, Assassin back) | App Store v2.41.2 |

Mechanics named in the request that the sources do not show: "Knight mode / Knight skills" (Knight exists only as a class), "Heroes" (the closest are Divinity/God races and "chess player" avatars, which are cosmetic), "Alchemy" (only a chest item 炼金秘藏 in Dec 2022, not a mechanic; the Feb 2020 dev log mentioned a planned "Alchemist" class that was never shipped under that name), "Oracle" (not found), "Turbo" (the fast mode is Trio/Quick). Chess players (couriers) are cosmetic in all sources; no gameplay perks were found.

---

## Chess pieces

Source: https://ac.dragonest.com/zh-cn/charactor (official site data, records updated up to 2026-09-04) with the older English site https://ac.dragonest.com/en/charactor . This is the site catalog of 112 pieces, not the live rotation at any one time. Stats are per star (1/2/3). Attack interval is seconds per attack. Range mixes grid units (1 = melee, 3-4 = ranged) and a few engine values. CD is the ability cooldown in seconds from the site; mana values are not published (abilities in the current game mostly run on cooldowns).

| Piece (EN / ZH) | Cost | Quality | Races | Classes | HP | ATK | Armor | MR | Atk interval | Range | CD | Ability | Updated |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Defector / 背叛者 | 1 | Common | Glacier | Shaman | 550 / 1100 / 2200 | 45 / 90 / 180 | 5 | 0 | 1.5 | 3 | 10/9/8 | Hex | 2022-07-25 |
| Flower Spirit (unofficial) / 花灵 | 1 | Common | Feathered | Wizard | 450 / 900 / 1800 | 40-70 / 80-140 / 160-280 | 1 | 0.0 | 1.1 | 3.13 | 8/7/6 | Spore Bomb (unofficial) | 2025-02-12 |
| Frost Knight / 冰霜骑士 | 1 | Common | Glacier | Knight | 500 / 1000 / 2000 | 47.5 / 95 / 190 | 5 | 0 | 1.3 | 3 | 16/13/10 | Snowbomb Blast | 2022-07-25 |
| Hawk / 猎鹰 | 1 | Common | Watcher | Mage | 550 / 1100 / 2200 | 40-60 / 80-120 / 160-240 | 1 | 10% | 1.5 | 4 | unknown | Eagle Eye | 2026-01-29 |
| Heaven Bomber / 轰天者 | 1 | Common | Goblin | Mech | 500 / 1000 / 2000 | 45 / 90 / 180 | 5 | 0 | 1.5 | 4 | 8/7/6 | Tracing Missile | 2022-07-25 |
| Ogre Mage / 基拉法师 | 1 | Common | Kira | Mage | 700 / 1400 / 2800 | 55-65 / 110-130 / 220-260 | 3 | 0 | 1.4 | 1 | 20/20/15 | Irritate | 2025-08-27 |
| Redaxe Chief / 赤斧酋长 | 1 | Common | Cave | Warrior | 700 / 1400 / 2800 | 52 / 105 / 210 | 5 | 0 | 1.5 | 1 | 10/8/6 | Taunts | 2022-07-25 |
| Resentful Murk / 怨影 | 1 | Common | Egersis | Witcher | 500 / 1000 / 2000 | 60 / 120 / 240 | 2 | 0% | 1.3 | 3 | 8 | Apparition | 2025-07-28 |
| Shining Archer / 光羽弩手 | 1 | Common | Feathered | Hunter | 450 / 900 / 1800 | 45 / 90 / 180 | 0 | 0 | 1 | 4 | 8/7/6 | Shooting Star | 2022-07-25 |
| Sky Breaker / 霸天者 | 1 | Common | Goblin | Mech | 700 / 1400 / 2800 | 50 / 100 / 200 | 5 / 7 / 9 | 0 | 1.4 | 1 | 12/10/8 | Battery Assault | 2022-07-25 |
| Soul Breaker / 刺魂者 | 1 | Common | Goblin | Assassin | 550 / 1100 / 2200 | 60 / 120 / 240 | 5 | 0 | 1 | 1 | 5 | Paralysis Shuriken | 2022-07-25 |
| Stone Spirit / 岩之灵 | 1 | Common | Spirits | Warrior | 700 / 1400 / 2800 | 80 / 160 / 320 | 5 | 0 | 1.8 | 1 | 10/8/6 | Toss | 2022-07-25 |
| Sunchaser Servant / 逐日侍卫 | 1 | Common | Horn | Assassin | 650 / 1300 / 2600 | 57 / 115 / 230 | 3 | 0 | 1.3 | 1 | 15/12/9 | Warrior Charge | 2022-07-25 |
| Taboo Witcher / 禁忌猎魔人 | 1 | Common | Feathered | Witcher | 600 / 1200 / 2400 | 50 / 100 / 200 | 5 | 10 / 20 / 30 | 1.2 | 1 | 0 | Soul Break | 2022-07-25 |
| The Source / 魔力之源 | 1 | Common | Human | Mage | 500 / 1000 / 2000 | 50 / 100 / 200 | 5 | 0 | 1.7 | 4 | 0 | Awaken | 2022-07-25 |
| Tusk Champion / 海象拳王 | 1 | Common | Beast | Warrior | 650 / 1300 / 2600 | 52.5 / 105 / 210 | 5 | 0 | 1.2 | 1 | 7 | Arctic Punch | 2022-07-25 |
| Unicorn / 独角兽 | 1 | Common | Beast | Druid | 400 / 800 / 1500 | 55 / 110 / 220 | 5 | 0 | 1.5 | 3 | 15 | Cure | 2022-07-25 |
| Unknown Horror / 未知怖物 | 1 | Common | Night Demon | Warlock | 550 / 1100 / 2200 | 40-50 / 80-100 / 160-220 | 3 | 10% | 1.5 | 3 | 6 | Endless Terror | 2025-11-27 |
| Winter Chiropteran / 冰蝠 | 1 | Common | Dragon, Egersis | Mage | 500 / 1000 / 2000 | 47 / 95 / 190 | 5 |  | 1.3 | 3 | 12/9/6 | Cool Healing | 2022-07-25 |
| Abyssal Guard / 海渊守卫 | 2 | Uncommon | Marine | Warrior | 650 / 1300 / 2600 | 85 / 170 / 340 | 5 | 0 | 1.2 | 1 | 4 | Corrosion | 2022-07-25 |
| Abyssalcrawler / 海渊夜行者 | 2 | Uncommon | Marine | Assassin | 500 / 1000 / 2000 | 60 / 120 / 240 | 5 | 0 | 1 | 1 | 0 | Spiral Crawler | 2022-07-25 |
| Desperate Doctor / 绝命巫医 | 2 | Uncommon | Glacier | Warlock | 550 / 1100 / 2200 | 45 / 90 / 180 | 5 | 0 | 1.4 | 3 | 10 | Ricocheting Cask | 2022-07-25 |
| Dwarf Sniper / 矮人火枪手 | 2 | Uncommon | Dwarf | Hunter | 450 / 900 / 1800 | 70 / 140 / 280 | 5 | 0 | 1.1 | 7 | 10 | Headshot | 2025-07-28 |
| Egersis Ranger / 不眠游侠 | 2 | Uncommon | Egersis | Hunter | 600 / 1200 / 2400 | 65 / 130 / 260 | 5 | 0 | 1.3 | 4 | 0 | Marksmanship | 2022-07-25 |
| Erebus (unofficial) / 厄瑞玻斯 | 2 | Uncommon | Ancestor | Warlock | 700 / 1400 / 2800 | 50-90 / 100-180 / 200-360 | 4 | 10% | 1.6 | 1 | unknown | Dark Shield (unofficial) | 2025-07-28 |
| Flame Wizard / 焰灵法师 | 2 | Uncommon | Human | Mage | 500 / 1000 / 2000 | 5- / 100 / 200 | 5 | 0 | 1.1 | 4 | 8/6/4 | Laguna Blade | 2022-05-25 |
| Goddess of Light / 光之女神 | 2 | Uncommon | Divinity | Priest | 450 / 900 / 1800 | 52 / 105 / 210 | 3 | 10% | 1.3 | 4 | 12/10/8 | Spiritbind | 2022-07-25 |
| Heaven Brew (Qin Lang) / 仙酿之主 | 2 | Uncommon | Pandaman | Martialist | 800 / 1600 / 3200 | 55 / 110 / 220 | 4 | 0 | 1.3 | 1 | 12/10/8 | Drunken Dream | 2022-07-25 |
| Hell Knight / 炼狱骑士 | 2 | Uncommon | Demon | Knight | 700 / 1400 / 2800 | 70 / 140 / 280 | 5 | 0 | 1.2 | 1 | 5 | Helling Bolt | 2022-07-25 |
| Lava Shaman / 熔岩萨满 | 2 | Uncommon | Cave | Shaman | 650 / 1300 / 2600 | 57.5 / 115 / 230 | 5 | 0.0 | 1.1 | 1 | 10 | Lava Swirl (name inferred from EN patch text) | 2022-07-28 |
| Light Blade / 灵刃骑士 | 2 | Uncommon | Feathered | Knight | 500 / 1000 / 2000 | 50 / 100 / 200 | 5 | 0 | 1.3 | 3 | 0 | Moon Glaives | 2022-07-25 |
| Living Sand Saint (unofficial) / 活砂圣女 | 2 | Uncommon | unknown-EN (半兽人) | Warrior | 650 / 1300 / 2600 | 55-60 / 110-120 / 220-240 | 4 | 0.0 | 1.1 | 1.6 | unknown | Living Sand Blade (unofficial) | 2024-03-07 |
| Ripper / 撕裂者 | 2 | Uncommon | Goblin | Mech | 800 / 1600 / 3200 | 57 / 115 / 230 | 6 / 8 / 10 | 0 | 1.4 | 1 | 6/5/4 | Reaping Death | 2022-07-25 |
| Shining Dragon / 光鳞龙 | 2 | Uncommon | Dragon, Feathered | Mage | 450 / 900 / 1800 | 45 / 90 / 180 | 5 | 0 | 1 | 3 | 5 | Light and Shadow (unofficial) | 2023-01-12 |
| Skull Hunter / 独眼猎手 | 2 | Uncommon | Cave | Hunter | 700 / 1400 / 2800 | 60-70 / 120-140 / 240-280 | 5 / 5 / 12 | 0 | 1.4 / 1.4 / 1.6 | 1 | 20/20/20 | Porcupette | 2025-08-27 |
| Soul Devourer / 噬魂巫师 | 2 | Uncommon | Demon | Wizard | 500 / 1000 / 2000 | 50 / 100 / 200 | 5 | 0 | 1.7 | 4 | 8/6/4 | Turning Tables | 2022-07-25 |
| Swordman / 剑客 | 2 | Uncommon | Cave | Warrior | 600 / 1200 / 2400 | 67.5 / 130 / 260 | 5 | 0 | 1.1 | 1 | 14/13/12 | Blade Fury | 2022-07-25 |
| Venom / 毒液 | 2 | Uncommon | Dragon | Assassin | 500 / 1000 / 2000 | 55 / 110 / 220 | 5 | 20 | 1 | 3 | 8/6/4 | Viper Strike | 2022-05-25 |
| Voice of the Deep (unofficial) / 深潜之音 | 2 | Uncommon | Marine | Wizard | 550 / 1100 / 2200 | 55 / 110 / 220 | 4 | 0% | 1.2 | 1 | unknown | Abyssal Chant (unofficial) | 2023-06-01 |
| Water Spirit / 水之灵 | 2 | Uncommon | Spirits | Assassin | 550 / 1100 / 2200 | 57 / 115 / 230 | 5 | 0 | 1.3 | 3 | 10/8/6 | Wave Slash | 2022-07-25 |
| Werewolf / 狼人 | 2 | Rare | Human, Beast | Warrior | 750 / 1500 / 3000 | 55 / 110 / 220 | 5 | 0 | 1.1 | 1 | 60 | Shapeshift | 2022-07-25 |
| Wisper Seer / 慧语先知 | 2 | Uncommon | Feathered | Druid | 500 / 1000 / 1900 | 47.5 / 95 / 190 | 2 | 0 | 1.4 | 3 | 7 | Nature's Call | 2022-07-25 |
| Argali Knight / 盘羊骑士 | 3 | Rare | Human | Knight | 750 / 1500 / 3000 | 55 / 110 / 220 | 10 | 0 | 1.4 | 1 | 12 | Purification | 2022-07-25 |
| Avenge Knight / 复仇骑士 | 3 | Rare | Human | Knight | 700 / 1400 / 2800 | 70 / 140 / 280 | 5 | 0 | 1.2 | 1 | 0 | Strike Back | 2022-07-25 |
| Berserker / 狂战士 | 3 | Rare | Glacier | Warrior | 1000 / 2000 / 4000 | 90-130 / 180-260 / 360-520 | 10 | 10 | 0.8 | 3 | 0 | Wind Furyaxe | 2025-08-27 |
| Bobo / 啵啵 | 3 | Rare | Beast | Hunter | 600 / 1200 / 2400 | 60 / 120 / 240 | 5 | 0 | 1.0 | 4 | 10/9/8 | Delicious Shell | 2022-07-25 |
| Dominator (Dominiel) / 太古支配者 | 3 | Rare | Greater (The Greater) | Assassin | 700 / 1400 / 2800 | 70 / 140 / 280 | 5 | 0 | 1.1 | 1 | 20/16/12 | Sword Field | 2022-07-25 |
| Eclipse of Darkness / 蚀光之暗 | 3 | Rare | Egersis | Priest | 750 / 1500 / 3000 | 60-90 / 120-180 / 240-360 | 5 | 0.0 | 1.5 | 1.6 | 10/8/6 | Blood Surge | 2025-02-12 |
| Ember Blade (Di Wei) / 黯焰之刃 | 3 | Rare | Pandaman | Assassin | 800 / 1600 / 3200 | 75 / 150 / 300 | 5 | 0 | 1.2 | 1 | 6/5/3 | Catching Fire | 2023-01-12 |
| Evil Knight / 邪灵骑士 | 3 | Rare | Egersis | Knight | 750 / 1500 / 3000 | 50 / 100 / 200 | 10 | 0 | 1 | 2 | 12 | Shield of Skeleton | 2022-07-28 |
| Fallen Witcher / 堕落猎魔人 | 3 | Rare | Demon | Witcher | 700 / 1400 / 2800 | 40-80 / 80-160 / 160-320 | 5 | 20 | 1.1 | 1 | 60 | Metamorphism | 2025-08-27 |
| Fortune Teller / 占卜师 | 3 | Rare | Glacier | Priest | 550 / 1100 / 2200 | 62 / 125 / 250 | 5 | 0 | 1.4 | 3 | 18 | Life Extension | 2022-07-25 |
| Gem Artisan / 宝石魔匠 | 3 | Rare | Civet | Mech | 800 / 1600 / 2400 | 55 / 110 / 220 | 6 | 20 | 1 | 1 | 8/6/4 | Gemstone Wave | 2022-07-25 |
| Ghost Kid / 鬼灵萝莉 | 3 | Rare | Soul Clan | Warrior | 850 / 1600 / 3400 | 90 / 180 / 360 | 5 | 0 | 1.5 | 1 | 15/12/9 | Chew Chew | 2023-01-12 |
| God of War / 战神 | 3 | Rare | Divinity | Warrior | 800 / 1600 / 3200 | 0 / 0 / 0 | 6 | 0 | 0 | 1 | 10/9/8 | Shield Crash | 2022-07-25 |
| Goddess of War (Valkyrie) / 女武神 | 3 | Rare | Ancestor | Knight | 700 / 1400 / 2800 | 70-110 / 140-220 / 280-440 | 7 | 0.0 | 1.6 | 1.6 | 10/8/6 | Shining Combo | 2025-07-28 |
| Grand Herald / 神使 | 3 | Rare | Divinity | Wizard | 500 / 1100 / 2200 | 50-60 / 100-120 / 200-240 | 2 | 20 | 1.4 | 4 | 0 | Take Control | 2025-08-27 |
| Jade General (Ji Xi) / 玉石之军 | 3 | Rare | Pandaman | Shaman | 900 / 1800 / 3600 | 65 / 130 / 260 | 9 | 0 | 1.5 | 1 | 8/6/4 | Rock-breaking | 2022-07-25 |
| Lord of Sand / 沙地之主 | 3 | Rare | Insectoid | Assassin | 600 / 1200 / 2400 | 52 / 105 / 210 | 10 | 0 | 1.4 | 1 | 10/8/6 | Burrow Dash | 2022-07-25 |
| Mist Phantom King (Wuril) / 雾霭魔君 | 3 | Rare | Night Demon | Assassin | 500 / 1000 / 2000 | 45-95 / 90-190 / 180-380 | 5 |  | 0 | 1.6 | unknown | Smoke Trick | 2025-07-28 |
| Nightmare (formerly Egersis Prophet) / 梦魇 | 3 | Rare | Soul Clan | Warlock | 650 / 1300 / 2600 | 55 / 110 / 220 | 5 | 30 | 1.4 | 3 | 15/10/5 | Wrecking Ball (unofficial) | 2022-12-09 |
| Penitent Bishop (Mammon) / 讨罪司教 | 3 | Rare | Ancestor, Demon | Priest | 650 / 1300 / 2600 | 50-90 / 100-180 / 200-360 | 4 | 10% | 1.7 | 3 | 5 | Ancestral Curse | 2025-09-28 |
| Poisonous Worm / 魔化毒虫 | 3 | Rare | Insectoid, Beast | Warlock | 600 / 1200 / 2400 | 55 / 110 / 220 | 0 | 20 | 1.2 | 3 | 5/4/4 | Plague Ward | 2022-07-25 |
| Qin Xuan / Plume Warrior / 秦璇 | 3 | Rare | Feathered | Martialist | 750 / 1500 / 3000 | 55 / 110 / 220 | 5 | 0.0 | 1.3 | 1 | 20/16/12 | Fist of Gale | 2023-03-09 |
| Shadow Devil / 暗影魔王 | 3 | Rare | Demon | Warlock | 550 / 1100 / 2200 | 80 / 160 / 320 | 5 | 0 | 1.1 | 4 | 15/12/10 | Requiem of Shadow | 2022-07-25 |
| Shadowcrawler / 幽影夜行者 | 3 | Rare | Feathered | Assassin | 600 / 1200 / 2400 | 70-110 / 140-220 / 280-440 | 4 | 0 | 0.9 | 1 | 0 | Coup de Ombre | 2025-08-27 |
| Thunder Spirit / 雷之灵 | 3 | Rare | Spirits | Mage | 750 / 1500 / 3000 | 55 / 110 / 220 | 5 | 0 | 1 | 3 | 10/9/8 | Lightning Nova | 2022-05-25 |
| Thunder Trace (Lei Rong) / 雷影之贤 | 3 | Rare | Pandaman | Mage | 700 / 1400 / 2800 | 75 / 150 / 300 | 3 | 10 | 1.4 | 3 | 5/4/3 | Ball Lightning | 2022-07-25 |
| Umbra / 影 | 3 | Rare | Dragon, Egersis | Hunter | 800 / 1600 / 3200 | 50 / 100 / 200 | 5 | 10% | 1.5 | 3 | 60 | Familiar | 2022-07-25 |
| Underworld Enforcer (unofficial) / 冥界执法官 | 3 | Rare | Egersis | Hunter | 650 / 1300 / 2600 | 40-80 / 80-160 / 160-320 | 3 | 0.0 | 1.2 | 4.69 | 7s/8s/9s | Death Hymn (unofficial) | 2025-02-12 |
| Warpwood Sage / 扭木贤者 | 3 | Rare | Feathered | Druid | 700 / 1400 / 2700 | 75 / 150 / 300 | 5 | 0 | 2 | 1 | 8 | Leech Seed | 2022-07-25 |
| Wind Ranger / 逐风游侠 | 3 | Rare | Feathered | Hunter | 550 / 1100 / 2200 | 60 / 120 / 240 | 5 | 0 | 0.9 | 4 | 10 | Powershot | 2022-07-25 |
| Cave Prodigy / 猪猪牧师 | 4 | Epic | Cave | Priest | 900 / 1800 / 3600 | 40-60 / 80-120 / 160-240 | 6 | 10 | 1.4 | 3 | 12/10/8 | Hero's Back | 2025-08-27 |
| Doom Arbiter / 末日审判官 | 4 | Epic | Demon | Warrior | 950 / 1900 / 3800 | 105 / 210 / 420 | 5 | 0 | 1.3 | 1 | 10 | Doom Curse | 2022-07-25 |
| Dragon Knight / 龙骑士 | 4 | Epic | Dragon, Human | Knight | 800 / 1600 / 3200 | 65 / 130 / 260 | 10 | 0 | 1.2 | 1 | 60 | Resonance | 2023-01-12 |
| Evernight Watcher / 永夜守望者 | 4 | Epic | unknown-EN (半兽人) | Assassin | 900 / 1800 / 3600 | 65-75 / 130-150 / 260-300 | 7 | 0.0 | 1.5 | 1.6 | unknown | Evernight Hold | 2024-03-07 |
| Fire Spirit / 火之灵 | 4 | Epic | Spirits | Shaman | 800 / 1600 / 3200 | 45 / 90 / 180 | 5 | 40 | 1.3 | 2 | 40 | Rebirth from Flames (unofficial) | 2022-09-09 |
| Grimtouch / 墨客 | 4 | Epic | Demon | Wizard | 750 / 1500 / 3000 | 80 / 160 / 320 | 5 | 0 | 1.4 | 3 | 20/15/10 | Ink Splash | 2022-07-25 |
| Pirate Captain / 海盗船长 | 4 | Epic | Human | Warrior | 950 / 1900 / 3800 | 82 / 165 / 330 | 8 | 0 | 1.3 | 1 | 13/11/9 | Ghostly ship | 2022-07-25 |
| Razorclaw / 利爪 | 4 | Epic | Beast | Druid | 800 / 1600 / 2800 | 50-60 / 100-120 / 200-240 | 7 | 0 | 1.3 | 3 | 60 | Summon Spirit Bear | 2025-08-27 |
| Redfox Swordsman (Forro Vulpes) / 赤狐剑士 | 4 | Epic | Beast | Assassin | 750 / 1500 / 3000 | 80 / 160 / 320 | 8 | 10% | 1 | 1 | 8/7/6 | Foxriken | 2022-07-25 |
| Siren / 海妖 | 4 | Epic | Marine | Hunter | 750 / 1500 / 3000 | 60 / 120 / 240 | 5 | 10 | 1.2 | 4 | 20 | Abyssal Gaze | 2022-07-25 |
| Soul Reaper / 灵魂收割者 | 4 | Epic | Egersis | Warlock | 800 / 1600 / 3200 | 55 / 110 / 220 | 5 | 0 | 1.2 | 3 | 6 | Heartstopper Pulse | 2022-05-25 |
| Spider Queen / 沼泽蛛后 | 4 | Epic | Insectoid | Hunter | 900 / 1800 / 3600 | 95 / 190 / 380 | 5 | 5% | 1.2 | 1 | 10 | Weave Web | 2022-07-25 |
| Storm Shaman / 风暴萨满 | 4 | Epic | Cave | Shaman | 800 / 1600 / 3200 | 47 / 95 / 190 | 5 | 0 | 1.5 | 4 | 16/13/10 | Static Storm | 2022-07-25 |
| Sunchaser Shaman / 逐日祭司 | 4 | Epic | Horn | Shaman | 700 / 1400 / 2800 | 67 / 135 / 270 | 10 | 0 | 1.3 | 1 | 10/8/6 | Earth Drumming | 2022-07-25 |
| The Exile (unofficial) / 流亡者 | 4 | Epic | Glacier | unknown-EN (武装姿态) | 900 / 1800 / 3600 | 90-110 / 180-220 / 360-440 | 4 | 10 | 200 | 1 | unknown | Shatter (unofficial) | 2026-09-04 |
| Thorn Predator / 荆棘掠食者 | 4 | Epic | Insectoid, Egersis | Assassin | 750 / 1500 / 3000 | 67 / 135 / 270 | 10 | 0 | 1.3 | 1 | 15/12/9 | Thorn Shell | 2022-07-25 |
| Tortola Elder / 龟甲长老 | 4 | Epic | Human | Mage | 650 / 1300 / 2600 | 42 / 85 / 170 | 5 | 0 | 1.5 | 4 | 10/8/6 | Kamehameha | 2022-07-25 |
| Venomancer / 萃毒者 | 4 | Epic | Goblin, Kira | Warlock | 1000 / 2000 / 4000 | 65 / 130 / 260 | 0 | 0 | 1.3 | 1 | 25/20/15 | Chemical Rage | 2022-07-25 |
| Bad Omen (Dutch Iron) / 灾祸之兆 | 5 | Legendary | Cave | Prophet (EN unverified) | 900 / 1800 / 3600 | 90 / 180 / 360 | 6 | 25% | 1.5 | 4 | 60 | Cataclysm: Demon Birth (unofficial) | 2022-07-25 |
| Cannon Granny / 火器大师 | 5 | Legendary | Goblin | Knight | 1100 / 2200 / 4400 | 95 / 190 / 380 | 12 | 0 | 1.3 | 4 | 12/10/8 | Art of Artillery (unofficial) | 2022-07-25 |
| Dark Spirit / 暗之灵 | 5 | Legendary | Spirits | Warlock | 1000 / 2000 / 4000 | 50 / 100 / 200 | 5 | 40 | 1.5 | 3 | 20 | Black Hole | 2022-07-25 |
| Devastator / 毁灭者 | 5 | Legendary | Goblin | Mech | 1000 / 2000 / 4000 | 50 / 100 / 200 | 10 | 0 | 1.5 | 4 | 12/12/12 | Delayed Action Bomb | 2022-07-25 |
| Dualplot (Taorive) / 晦暗之谋 | 5 | Legendary | Pandaman | Martialist | 1000 / 2000 / 4000 | 100 / 200 / 400 | 10 | 0 | 1.1 | 1 | 8/7/5 | Candle Lighting the Nine Yin (unofficial) | 2022-07-25 |
| Frostblaze Dragon / 霜炽龙 | 5 | Legendary | Dragon | Mage | 950 / 1900 / 3800 | 60 / 120 / 240 | 0 | 20% | 1.1 | 4 | 15 | Icebound Wall | 2022-07-28 |
| God of Thunder / 雷神 | 5 | Legendary | Divinity | Mage | 950 / 1900 / 3800 | 60 / 120 / 240 | 0 | 40 | 1.4 | 3 | 15/12/9 | Zeus' Punishment | 2022-07-25 |
| Helicopter / 矮人直升机 | 5 | Legendary | Dwarf | Mech | 900 / 1800 / 3600 | 77 / 155 / 310 | 10 | 0 | 0.9 | 3 | 14/12/10 | Call Down | 2022-07-25 |
| Khan / 烈日可汗 | 5 | Legendary | Horn, Divinity | Druid | 1000 / 2000 / 4000 | 115 / 230 / 460 | 10 | 20 | 1.3 | 1 | 30 | Earth Cracking | 2022-07-25 |
| Light Spirit / 光之灵 | 5 | Legendary | Spirits | Priest | 950 / 1900 / 3800 | 90 / 180 / 360 | 8 | 10 | 1.1 | 3 | 0 | Breaking Light (unofficial) | 2022-11-03 |
| Mountain Chief / 山魁 | 5 | Legendary | Beast | Warrior | 1200 / 2400 / 4800 | 150-200 / 300-400 / 600-800 | 10 | 10% | 1.5 | 1.6 | unknown | Collapsing Impact | 2025-02-12 |
| Old God of the Dusk Forest (unofficial) / 暮林旧神 | 5 | Legendary | Divinity, unknown-EN (半兽人) | Druid | 900 / 1800 / 3600 | 60-110 / 120-220 / 240-440 | 6 | 20% | 1.2s | 3.13 | 8/6/4 | Soul Flame (unofficial) | 2025-02-12 |
| Phantom Queen / 魅影女王 | 5 | Legendary | Demon | Assassin | 950 / 1900 / 3800 | 87 / 172 / 350 | 6 | 20 | 1.2 | 3 | 20/15/10 | Scream of Devil | 2022-07-25 |
| Radiant Empress (unofficial) / 晖光女帝 | 5 | Legendary | Civet | Shaman | 950 / 1900 / 3800 | 90-150 / 180-300 / 360-600 | 7 | 0.0 | 1.0 | 1.0 | unknown | Perfect Avatar (unofficial) | 2023-09-12 |
| Rogue Guard / 沉沦守卫 | 5 | Legendary | Demon | Warrior | 1200 / 2400 / 4800 | 150-200 / 300-420 / 600-840 | 10 | 0 | 1.6 | 1 | 60 | Crazy Dance | 2025-08-27 |
| Sacred Lancer / 冰川武士 | 5 | Legendary | Glacier | Warrior | 1000 / 2000 / 4000 | 90 / 180 / 360 | 10 | 10% | 1.1 | 3 | unknown | Cursed Lance | 2022-07-25 |
| Shining Assassin / 光羽斥候 | 5 | Legendary | Feathered | Assassin | 950 / 1900 / 3800 | 105 / 210 / 420 | 8 | 0 | 1 | 3 | 6 | T.A. Field | 2022-07-25 |
| Soul Blade / 亡魂之刃 | 5 | Legendary | Soul Clan | Witcher | 950 / 1900 / 3800 | 105 / 210 / 420 | 5 | 0 | 1.0 | 1 | 0 | Forbidden Soul Art (unofficial) | 2022-12-09 |
| Space Walker / 乾坤行者 | 5 | Legendary | Beast | Martialist | 1100 / 2200 / 4400 | 112 / 225 / 450 | 10 | 5 | 1 | 2 | unknown | Heaven-shocking | 2022-07-25 |
| Strange Egg / 奇异蛋 | 5 | Legendary | Feathered |  | 700 / 1400 /  | 50 / 100 /  | 0 / 0 /  | 0 / 0 /  | 1.3 / 1.3 /  | 3 / 3 /  | 0 | Ever Changing | 2022-05-25 |
| The Scryer / 占星者 | 5 | Legendary | Feathered | Shaman | 1000 / 2000 / 4000 | 82 / 165 / 330 | 5 | 0 | 1.1 | 4 | 10 | Raven Arcane | 2022-07-25 |
| Tsunami Stalker / 海啸追猎者 | 5 | Legendary | Marine | Hunter | 1000 / 2000 / 4000 | 50 / 100 / 200 | 5 | 0 | 2 | 1 | 25/20/15 | Ravaging Tentacles | 2022-05-25 |

### Abilities per star

For pieces that have an official English record, the English lines come from the older English site (2022 and later) and the current Chinese text follows, because numbers may have changed since. Pieces without an English record show a translation of the current Chinese text.

**Defector** (背叛者, 1-cost) - Hex (变形术)
- 1-star (official EN, older): Transforms an enemy piece into a harmless penguin, disabling their attacks and abilities in 4 seconds.
- 2-star (official EN, older): Transforms an enemy piece into a harmless penguin, disabling their attacks and abilities in 6 seconds.
- 3-star (official EN, older): Transforms an enemy piece into a harmless penguin, disabling their attacks and abilities in 8 seconds.
- 1-star (current ZH): 将一个随机敌方棋子变成无法攻击和使用技能的企鹅，持续4秒
- 2-star (current ZH): 将一个随机敌方棋子变成无法攻击和使用技能的企鹅，持续6秒
- 3-star (current ZH): 将一个随机敌方棋子变成无法攻击和使用技能的企鹅，持续8秒

**Flower Spirit (unofficial)** (花灵, 1-cost) - Spore Bomb (unofficial) (孢子炸弹)
- 1-star (translated): Gives an enemy a cursed ancient fairy lantern. After a delay, stuns the target and all enemies around it.
- 2-star (translated): Gives an enemy a cursed ancient fairy lantern. After a delay, stuns the target and all enemies around it.
- 3-star (translated): Gives an enemy a cursed ancient fairy lantern. After a delay, stuns the target and all enemies around it.

**Frost Knight** (冰霜骑士, 1-cost) - Snowbomb Blast (迷雾飞行)
- 1-star (official EN, older): Slow enemy units of 2-grid radius 1 seconds of their movement and taking bonus 50 magical damagess every time Frost Knight attacks. Get to at most 4 stacks and last 20 seconds
- 2-star (official EN, older): Slow enemy units of 2-grid radius 1 seconds of their movement and taking bonus 75 magical damagess every time Frost Knight attacks. Get to at most 6 stacks and last 20 seconds
- 3-star (official EN, older): Slow enemy units of 2-grid radius 1 seconds of their movement and taking bonus 100 magical damagess every time Frost Knight attacks. Get to at most 10 stacks and last 20 seconds
- 1-star (current ZH): 飞向最远的能攻击到敌人的格子，在地面留下一道毒雾的尾迹。毒雾尾迹会对经过的敌方棋子造成每秒20的持续伤害，持续10秒。
- 2-star (current ZH): 飞向最远的能攻击到敌人的格子，在地面留下一道毒雾的尾迹。毒雾尾迹会对经过的敌方棋子造成每秒30的持续伤害，持续10秒。
- 3-star (current ZH): 飞向最远的能攻击到敌人的格子，在地面留下一道毒雾的尾迹。毒雾尾迹会对经过的敌方棋子造成每秒60的持续伤害，持续10秒。

**Hawk** (猎鹰, 1-cost) - Eagle Eye (鹰眼)
- 1-star (official EN, older): Whenever an ally casts an active ability, Falcon instantly fires Arcane Missiles at enemies, dealing magical damage. Ability Type: Active Target: Single Impact: Enemy Damage Type: Magical Damage Magical Damage: 150 CD: 2
- 2-star (official EN, older): Whenever an ally casts an active ability, Falcon instantly fires Arcane Missiles at enemies, dealing magical damage. Ability Type: Active Target: Single Impact: Enemy Damage Type: Magical Damage Magical Damage: 250 CD: 2
- 3-star (official EN, older): Whenever an ally casts an active ability, Falcon instantly fires Arcane Missiles at enemies, dealing magical damage. Ability Type: Active Target: Single Impact: Enemy Damage Type: Magical Damage Magical Damage: 350 CD: 2
- 1-star (current ZH): 每当友方棋子施放主动技能，隼会立即对敌人施放出奥术飞弹，对目标造成魔法伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：魔法伤害 魔法伤害：150 冷却时间：2
- 2-star (current ZH): 每当友方棋子施放主动技能，隼会立即对敌人施放出奥术飞弹，对目标造成魔法伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：魔法伤害 魔法伤害：250 冷却时间：2
- 3-star (current ZH): 每当友方棋子施放主动技能，隼会立即对敌人施放出奥术飞弹，对目标造成魔法伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：魔法伤害 魔法伤害：350 冷却时间：2

**Heaven Bomber** (轰天者, 1-cost) - Tracing Missile (追踪导弹)
- 1-star (official EN, older): Launches 3 rockets to strike random enemy chess pieces, each missile deals 150 magical damage to a random enemy unit
- 2-star (official EN, older): Launches 3 rockets to strike random enemy chess pieces, each missile deals 250 magical damage to a random enemy unit
- 3-star (official EN, older): Launches 3 rockets to strike random enemy chess pieces, each missile deals 450 magical damage to a random enemy unit
- 1-star (current ZH): 发射3枚导弹分别对1个随机敌方棋子造成150魔法伤害
- 2-star (current ZH): 发射3枚导弹分别对1个随机敌方棋子造成250魔法伤害
- 3-star (current ZH): 发射3枚导弹分别对1个随机敌方棋子造成450魔法伤害

**Ogre Mage** (基拉法师, 1-cost) - Irritate (激活)
- 1-star (official EN, older): Buffs a random Ally for 30 seconds, giving the target ally 30% chances to restore 100 Mana and resets their Ability after casting
- 2-star (official EN, older): Buffs a random Ally for 30 seconds, giving the target ally 40% chances to restore 100 Mana and resets their Ability after casting
- 3-star (official EN, older): Buffs a random Ally for 30 seconds, giving the target ally 50% chances to restore 100 Mana and resets their Ability after casting
- 1-star (current ZH): 基拉法师激活随机友方棋子单位的潜力﹐使被激活的棋子在施放技能后立即回复全部的魔法值并重置技能冷却时间﹐该效果将在目标施放技能后失效。（友方数量：1）
- 2-star (current ZH): 基拉法师激活随机友方棋子单位的潜力﹐使被激活的棋子在施放技能后立即回复全部的魔法值并重置技能冷却时间﹐该效果将在目标施放技能后失效。（友方数量：2）
- 3-star (current ZH): 基拉法师激活随机友方棋子单位的潜力﹐使被激活的棋子在施放技能后立即回复全部的魔法值并重置技能冷却时间﹐该效果将在目标施放技能后失效。（友方数量：2）

**Redaxe Chief** (赤斧酋长, 1-cost) - Taunts (嘲讽)
- 1-star (official EN, older): Taunts enemies within 1 grids, forcing them to attack himself in 2.5 seconds, while he gains 10 bonus armor
- 2-star (official EN, older): Taunts enemies within 1 grids, forcing them to attack himself in 3.5 seconds, while he gains 15 bonus armor
- 3-star (official EN, older): Taunts enemies within 1 grids, forcing them to attack himself in 4.5 seconds, while he gains 20 bonus armor
- 1-star (current ZH): 获得10护甲并强迫1格范围敌方棋子攻击自己,持续2.5秒
- 2-star (current ZH): 获得15护甲并强迫1格范围敌方棋子攻击自己,持续3.5秒
- 3-star (current ZH): 获得20护甲并强迫1格范围敌方棋子攻击自己,持续4.5秒

**Resentful Murk** (怨影, 1-cost) - Apparition (幻影移形)
- 1-star (official EN, older): Take the mirror point symmetrical to the center of the chessboard as a reference, and target the enemy unit closest to that mirror point, swapping positions with the target, dealing 100 magical damage, and stunning it for 2 seconds. Ally normal attacks give priority to the target for 2 seconds. Pierces ability immunity.
- 2-star (official EN, older): Take the mirror point symmetrical to the center of the chessboard as a reference, and target the enemy unit closest to that mirror point, swapping positions with the target, dealing 200 magical damage, and stunning it for 2.5 seconds. Ally normal attacks give priority to the target for 3 seconds. Pierces ability immunity.
- 3-star (official EN, older): Take the mirror point symmetrical to the center of the chessboard as a reference, and target the enemy unit closest to that mirror point, swapping positions with the target, dealing 250 magical damage, and stunning it for 3 seconds. Ally normal attacks give priority to the target for 4 seconds. Pierces ability immunity.
- 1-star (current ZH): 以棋盘中心对称的自身镜像点为参照，锁定距离该镜像点最近的敌方单位，与目标单位交换位置并对其造成100点魔法伤害，目标单位眩晕2秒；我方单位的普通攻击会优先攻击目标单位，持续2秒。 无视技能免疫。
- 2-star (current ZH): 以棋盘中心对称的自身镜像点为参照，锁定距离该镜像点最近的敌方单位，与目标单位交换位置并对其造成200点魔法伤害，目标单位眩晕2.5秒；我方单位的普通攻击会优先攻击目标单位，持续3秒。 无视技能免疫。
- 3-star (current ZH): 以棋盘中心对称的自身镜像点为参照，锁定距离该镜像点最近的敌方单位，与目标单位交换位置并对其造成250点魔法伤害，目标单位眩晕3秒；我方单位的普通攻击会优先攻击目标单位，持续4秒。 无视技能免疫。

**Shining Archer** (光羽弩手, 1-cost) - Shooting Star (星之矢)
- 1-star (official EN, older): Deals 50 - 500 Magical Damage to the farthest piece, and stuns it for 1 - 5 seconds. The longer the distance, the greater the damage and the longer the stun duration.
- 2-star (official EN, older): Deals 75 - 750 Magical Damage to the farthest piece, and stuns it for 1.5 - 7.5 seconds. The longer the distance, the greater the damage and the longer the stun duration.
- 3-star (official EN, older): Deals 100 - 1000 Magical Damage to the farthest piece, and stuns it for 2 - 10 seconds. The longer the distance, the greater the damage and the longer the stun duration.
- 1-star (current ZH): 对距离自己最远的棋子造成30到300魔法伤害和1到5秒眩晕，距离越远伤害越大，晕眩时间越长。
- 2-star (current ZH): 对距离自己最远的棋子造成50到500魔法伤害和1.5到7.5秒眩晕，距离越远伤害越大，晕眩时间越长。
- 3-star (current ZH): 对距离自己最远的棋子造成70到700魔法伤害和2到10秒眩晕，距离越远伤害越大，晕眩时间越长。

**Sky Breaker** (霸天者, 1-cost) - Battery Assault (弹片冲击)
- 1-star (official EN, older): Discharges magical damages at random nearby enemy chess pieces within 2 grids, dealing 50 magical damages and stunning for 0.1 seconds every 0.7 seconds up to 5 seconds.
- 2-star (official EN, older): Discharges magical damages at random nearby enemy chess pieces within 2 grids, dealing 75 damages and stunning for 0.1 seconds every 0.7 seconds up to 5 seconds.
- 3-star (official EN, older): Discharges magical damages at random nearby enemy chess pieces within 2 grids, dealing 100 magical damages and stunning for 0.1 seconds every 0.7 seconds up to 5 seconds.
- 1-star (current ZH): 每0.7秒对2格内随机的1个敌方棋子造成50魔法伤害和0.1秒眩晕，持续5秒。
- 2-star (current ZH): 每0.7秒对2格内随机的1个敌方棋子造成75魔法伤害和0.1秒眩晕，持续5秒。
- 3-star (current ZH): 每0.7秒对2格内随机的1个敌方棋子造成125魔法伤害和0.1秒眩晕，持续5秒。

**Soul Breaker** (刺魂者, 1-cost) - Paralysis Shuriken (麻痹飞镖)
- 1-star (official EN, older): Hurls a shuriken at a random enemy unit, dealing 300 magical damages and stunning for 0.1 seconds.
- 2-star (official EN, older): Hurls a shuriken at a random enemy unit, dealing 500 magical damages and stunning for 0.1 seconds.
- 3-star (official EN, older): Hurls a shuriken at a random enemy unit, dealing 700 magical damages and stunning for 0.1 seconds.
- 1-star (current ZH): 向一个随机敌方棋子投掷麻痹飞镖造成300魔法伤害和0.1秒眩晕。
- 2-star (current ZH): 向一个随机敌方棋子投掷麻痹飞镖造成500魔法伤害和0.1秒眩晕。
- 3-star (current ZH): 向一个随机敌方棋子投掷麻痹飞镖造成700魔法伤害和0.1秒眩晕。

**Stone Spirit** (岩之灵, 1-cost) - Toss (投掷)
- 1-star (official EN, older): Grab a nearby enemy within 1 grids, and lunches it at the farthest grid, to deal 100 physical damages and stun for 1.5 seconds
- 2-star (official EN, older): Grab a nearby enemy within 1 grids, and lunches it at the farthest grid, to deal 200 physical damages and stun for 2 seconds
- 3-star (official EN, older): Grab a nearby enemy within 1 grids, and lunches it at the farthest grid, to deal 300 physical damages and stun for 2.5 seconds
- 1-star (current ZH): 将身边的1个敌人抛起，扔向离岩之灵最远且与敌方单位相邻的空格子上，若3格以外不存在这样的空格子，则原地抛起落下。落下后，对该敌人及落点周围的敌人造成100点物理伤害并眩晕1.5秒。 无视技能免疫。
- 2-star (current ZH): 将身边的1个敌人抛起，扔向离岩之灵最远且与敌方单位相邻的空格子上，若3格以外不存在这样的空格子，则原地抛起落下。落下后，对该敌人及落点周围的敌人造成200点物理伤害并眩晕2秒。 无视技能免疫。
- 3-star (current ZH): 将身边的1个敌人抛起，扔向离岩之灵最远且与敌方单位相邻的空格子上，若3格以外不存在这样的空格子，则原地抛起落下。落下后，对该敌人及落点周围的敌人造成400点物理伤害并眩晕2.5秒。 无视技能免疫。

**Sunchaser Servant** (逐日侍卫, 1-cost) - Warrior Charge (勇士冲锋)
- 1-star (translated): Charges at the farthest enemy unit, dealing 200 damage and stunning it for 1s. Enemy units hit along the way are knocked back to the nearest empty grid and stunned for 0.5s.
- 2-star (translated): Charges at the farthest enemy unit, dealing 300 damage and stunning it for 1.5s. Enemy units hit along the way are knocked back to the nearest empty grid and stunned for 0.5s.
- 3-star (translated): Charges at the farthest enemy unit, dealing 500 damage and stunning it for 2.5s. Enemy units hit along the way are knocked back to the nearest empty grid and stunned for 0.5s.

**Taboo Witcher** (禁忌猎魔人, 1-cost) - Soul Break (削魂)
- 1-star (official EN, older): Burns an opponent's 30 magic on each attack, dealing 60% of magic burnt bonus physical damages to the target
- 2-star (official EN, older): Burns an opponent's 60 magic on each attack, dealing 60% of magic burnt bonus physical damages to the target
- 3-star (official EN, older): Burns an opponent's 90 magic on each attack, dealing 60% of magic burnt bonus physical damages to the target
- 1-star (current ZH): 被动：每次攻击损毁敌方棋子30魔法值并造成损毁值120%的额外物理伤害。
- 2-star (current ZH): 被动：每次攻击损毁敌方棋子60魔法值并造成损毁值120%的额外物理伤害。
- 3-star (current ZH): 被动：每次攻击损毁敌方棋子90魔法值并造成损毁值120%的额外物理伤害。

**The Source** (魔力之源, 1-cost) - Awaken (唤醒)
- 1-star (official EN, older): Grants 10 magic regeneration for all allied chess pieces in every 2 seconds
- 2-star (official EN, older): Grants 15 magic regeneration for all allied chess pieces in every 2 seconds
- 3-star (official EN, older): Grants 20 magic regeneration for all allied chess pieces in every 2 seconds
- 1-star (current ZH): 被动：每2秒为友方棋子增加12魔法值
- 2-star (current ZH): 被动：每2秒为友方棋子增加18魔法值
- 3-star (current ZH): 被动：每2秒为友方棋子增加24魔法值

**Tusk Champion** (海象拳王, 1-cost) - Arctic Punch (北海神拳)
- 1-star (official EN, older): Next normal attack launches its target into air for 1 seconds and deals 3 times damages
- 2-star (official EN, older): Next normal attack launches its target into air for 1 seconds and deals 3.5 times damages
- 3-star (official EN, older): Next normal attack launches its target into air for 1 seconds and deals 4 times damages
- 1-star (current ZH): 下次普攻造成300%伤害并使敌方棋子被击飞1秒
- 2-star (current ZH): 下次普攻造成400%伤害并使敌方棋子被击飞1秒
- 3-star (current ZH): 下次普攻造成600%伤害并使敌方棋子被击飞1秒

**Unicorn** (独角兽, 1-cost) - Cure (治愈)
- 1-star (official EN, older): Passive: When Unicorn receives base attacks, reduces the attacker's ATK Speed by 5% for 5 seconds. Active: Heals 25 HP every 1.5 seconds to 3 random allies within 3 grids, lasting for 11 seconds.
- 2-star (official EN, older): Passive: When Unicorn receives base attacks, reduces the attacker's ATK Speed by 15% for 5 seconds. Active: Heals 25 HP every 1.5 seconds to 5 random allies within 3 grids, lasting for 11 seconds.
- 3-star (official EN, older): Passive: When Unicorn receives base attacks, reduces the attacker's ATK Speed by 25% for 5 seconds. Active: Heals 25 HP every 1.5 seconds to 7 random allies within 3 grids, lasting for 11 seconds.
- 1-star (current ZH): 被动：当独角兽受到普攻时，降低攻击者5%攻击速度，持续5秒。 主动：每1.5秒治疗3格内随机的3个友方棋子25生命，持续11秒。
- 2-star (current ZH): 被动：当独角兽受到普攻时，降低攻击者15%攻击速度，持续5秒。 主动：每1.5秒治疗3格内随机的5个友方棋子35生命，持续11秒。
- 3-star (current ZH): 被动：当独角兽受到普攻时，降低攻击者25%攻击速度，持续5秒。 主动：每1.5秒治疗3格内随机的7个友方棋子50生命，持续11秒。

**Unknown Horror** (未知怖物, 1-cost) - Endless Terror (无限恐怖)
- 1-star (official EN, older): Grabs a random enemy piece, immobilizing it and dealing continuous pure damage.
- 2-star (official EN, older): Grabs a random enemy piece, immobilizing it and dealing continuous pure damage.
- 3-star (official EN, older): Grabs a random enemy piece, immobilizing it and dealing continuous pure damage.
- 1-star (current ZH): 持续施法：抓住一个随机的敌方棋子，使其失去行动能力并持续受到纯粹伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：纯粹伤害 持续时间：4 每次伤害：75 伤害间隔：1 冷却时间：6
- 2-star (current ZH): 持续施法：抓住一个随机的敌方棋子，使其失去行动能力并持续受到纯粹伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：纯粹伤害 持续时间：6 每次伤害：100 伤害间隔：1 冷却时间：6
- 3-star (current ZH): 持续施法：抓住一个随机的敌方棋子，使其失去行动能力并持续受到纯粹伤害。 技能类型：主动 目标：单体 影响：敌方 伤害类型：纯粹伤害 持续时间： 8 每次伤害：25 伤害间隔：1 冷却时间：6

**Winter Chiropteran** (冰蝠, 1-cost) - Cool Healing (低温愈合)
- 1-star (official EN, older): Freezes an ally with HP that is lower than 50%, protecting them from losing HP and recovers their HP by 20+ 10%*HP for 2 seconds
- 2-star (official EN, older): Freezes an ally with HP that is lower than 50%, protecting them from losing HP and recovers their HP by 20+ 10%*HP for 3 seconds
- 3-star (official EN, older): Freezes an ally with HP that is lower than 50%, protecting them from losing HP and recovers their HP by 20+ 10%*HP for 4 seconds
- 1-star (current ZH): 冰冻一个血量低于50%的友方单位，每秒恢复20+10%生命并不受到攻击伤害，持续2秒。
- 2-star (current ZH): 冰冻一个血量低于50%的友方单位，每秒恢复30+15%生命并不受到攻击伤害，持续2秒。
- 3-star (current ZH): 冰冻一个血量低于50%的友方单位，每秒恢复40+20%生命并不受到攻击伤害，持续2秒。

**Abyssal Guard** (海渊守卫, 2-cost) - Corrosion (侵蚀)
- 1-star (official EN, older): Randomly reduces 15 armor to an enemy unit's for 20 seconds
- 2-star (official EN, older): Randomly reduces 30 armor to an enemy unit's for 20 seconds
- 3-star (official EN, older): Randomly reduces 45 armor to an enemy unit's for 20 seconds
- 1-star (current ZH): 使1个随机敌方棋子护甲降低15，持续20秒。
- 2-star (current ZH): 使1个随机敌方棋子护甲降低30，持续20秒。
- 3-star (current ZH): 使1个随机敌方棋子护甲降低45，持续20秒。

**Abyssalcrawler** (海渊夜行者, 2-cost) - Spiral Crawler (螺旋能量)
- 1-star (official EN, older): Every Base attack will steal 3% ATK Speed from the enemy. Can stack up to 20 times.
- 2-star (official EN, older): Every Base attack will steal 5% ATK Speed from the enemy. Can stack up to 20 times.
- 3-star (official EN, older): Every Base attack will steal 7% ATK Speed from the enemy. Can stack up to 20 times.
- 1-star (current ZH): 被动：每次普攻会偷取敌方棋子6%攻速，最多15层
- 2-star (current ZH): 被动：每次普攻会偷取敌方棋子8%攻速，最多15层
- 3-star (current ZH): 被动：每次普攻会偷取敌方棋子12%攻速，最多15层

**Desperate Doctor** (绝命巫医, 2-cost) - Ricocheting Cask (弹跳药剂)
- 1-star (official EN, older): Launches a cask of paralyzing powder that ricochets between enemy chess pieces up to 4 times, dealing 60 damages and stunning those it hits for 1 seconds
- 2-star (official EN, older): Launches a cask of paralyzing powder that ricochets between enemy chess pieces up to 6 times, dealing 80 damages and stunning those it hits for 1 seconds
- 3-star (official EN, older): Launches a cask of paralyzing powder that ricochets between enemy chess pieces up to 8 times, dealing 100 damages and stunning those it hits for 1 second
- 1-star (current ZH): 丢出在敌方棋子间最多弹跳5次的药剂，造成60伤害和1秒眩晕
- 2-star (current ZH): 丢出在敌方棋子间最多弹跳7次的药剂，造成80伤害和1秒眩晕
- 3-star (current ZH): 丢出在敌方棋子间最多弹跳9次的药剂，造成100伤害和1秒眩晕

**Dwarf Sniper** (矮人火枪手, 2-cost) - Headshot (瞄准设计)
- 1-star (official EN, older): Locks onto an target piece and, after 0.5 seconds of aiming, fire a shot that deals 400 magical damages to target.
- 2-star (official EN, older): Locks onto an target piece and, after 0.5 seconds of aiming, fire a shot that deals 600 magical damages to target.
- 3-star (official EN, older): Locks onto an target piece and, after 0.5 seconds of aiming, fire a shot that deals 800 magical damages to target.
- 1-star (current ZH): [瞄准射击]：矮人火枪手锁定一个随机敌方单位并在短暂蓄力后对其造成400伤害。
- 2-star (current ZH): [瞄准射击]：矮人火枪手锁定一个随机敌方单位并在短暂蓄力后对其造成600伤害。
- 3-star (current ZH): [瞄准射击]：矮人火枪手锁定一个随机敌方单位并在短暂蓄力后对其造成1000伤害。

**Egersis Ranger** (不眠游侠, 2-cost) - Marksmanship (射手遗志)
- 1-star (official EN, older): Increases 25% attack speed and 25% ATK for itself.
- 2-star (official EN, older): Increases 35% attack speed and 35% ATK for itself.
- 3-star (official EN, older): Increases 45% attack speed and 45% ATK for itself.
- 1-star (current ZH): 被动：提高自身25%攻击力和25%攻击速度
- 2-star (current ZH): 被动：提高自身50%攻击力和50%攻击速度
- 3-star (current ZH): 被动：提高自身80%攻击力和80%攻击速度

**Erebus (unofficial)** (厄瑞玻斯, 2-cost) - Dark Shield (unofficial) (黑暗护盾)
- 1-star (translated): Wraps an allied piece in a dark shield that reduces damage it takes and deals continuous magical damage to nearby enemies. Ability Type: Active Target: Area Impact: Ally/Enemy Range: 234 Damage per Second: 50 Damage Reduction %: 10 Duration: 10 CD: 10
- 2-star (translated): Wraps an allied piece in a dark shield that reduces damage it takes and deals continuous magical damage to nearby enemies. Ability Type: Active Target: Area Impact: Ally/Enemy Range: 234 Damage per Second: 75 Damage Reduction %: 15 Duration: 15 CD: 8
- 3-star (translated): Wraps an allied piece in a dark shield that reduces damage it takes and deals continuous magical damage to nearby enemies. Ability Type: Active Target: Area Impact: Ally/Enemy Range: 234 Damage per Second: 100 Damage Reduction %: 20 Duration: 20 CD: 6

**Flame Wizard** (焰灵法师, 2-cost) - Laguna Blade (神灭)
- 1-star (official EN, older): Deals 350 magical damages to 1 random enemy piece. After the launch of skill, attack speed increase by 40%, lasts 30 seconds, can be stacked 2 times
- 2-star (official EN, older): Deals 600 magical damages to 1 random enemy piece. After the launch of skill, attack speed increase by 60%, lasts 30 seconds,can be stacked 2 times
- 3-star (official EN, older): Deals 900 magical damages to 1 random enemy piece. After the launch of skill, attack speed increase by 80%, lasts 30 seconds,can be stacked 2 times
- 1-star (current ZH): 对1个随机敌方棋子造成350魔法伤害。同时自身攻速提升40%，持续30秒
- 2-star (current ZH): 对1个随机敌方棋子造成600魔法伤害。同时自身攻速提升60%，持续30秒
- 3-star (current ZH): 对1个随机敌方棋子造成900魔法伤害。同时自身攻速提升80%，持续30秒

**Goddess of Light** (光之女神, 2-cost) - Spiritbind (缚灵神言)
- 1-star (official EN, older): Imprisons 1 enemy pieces for 5 seconds, disarms them and reduces their Magic Resistance by 20%, but the pieces imprisoned will be immune to Physical Damage.
- 2-star (official EN, older): Imprisons 2 enemies pieces for 5 seconds, disarms them and reduces their Magic Resistance by 30%, but the pieces imprisoned will be immune to Physical Damage.
- 3-star (official EN, older): Imprisons 4 enemies pieces for 5 seconds, disarms them and reduces their Magic Resistance by 50%, but the pieces imprisoned will be immune to Physical Damage.
- 1-star (current ZH): 暂时禁锢1个敌方棋子4秒，被禁锢的棋子被缴械，同时魔法抗性降低20%，但免疫物理伤害。
- 2-star (current ZH): 暂时禁锢2个敌方棋子4秒，被禁锢的棋子被缴械，同时魔法抗性降低30%，但免疫物理伤害。
- 3-star (current ZH): 暂时禁锢4个敌方棋子4秒，被禁锢的棋子被缴械，同时魔法抗性降低50%，但免疫物理伤害。

**Heaven Brew (Qin Lang)** (仙酿之主, 2-cost) - Drunken Dream (醉生梦死)
- 1-star (translated): Sprays liquor over an area. Affected enemy units have a 30% chance for their attacks to be dodged for 5s. If they take ability damage during this time, they also burn for 100 total damage, and the liquor's duration is extended by 3s.
- 2-star (translated): Sprays liquor over an area. Affected enemy units have a 40% chance for their attacks to be dodged for 5s. If they take ability damage during this time, they also burn for 150 total damage, and the liquor's duration is extended by 4s.
- 3-star (translated): Sprays liquor over an area. Affected enemy units have a 50% chance for their attacks to be dodged for 5s. If they take ability damage during this time, they also burn for 200 total damage, and the liquor's duration is extended by 5s.

**Hell Knight** (炼狱骑士, 2-cost) - Helling Bolt (炼狱箭)
- 1-star (official EN, older): Deals 50 to 200 damagess and stuns a random enemy piece for 1 to 2 seconds
- 2-star (official EN, older): Deals 100 to 300 damagess and stuns a random enemy piece for 1 to 3 seconds
- 3-star (official EN, older): Deals 150 to 400 damages and stuns a random enemy piece for 1 to 4 seconds
- 1-star (current ZH): 对1个随机敌方棋子造成50到200的随机魔法伤害和1到2秒的随机眩晕。
- 2-star (current ZH): 对1个随机敌方棋子造成100到300的随机魔法伤害和1到3秒的随机眩晕。
- 3-star (current ZH): 对1个随机敌方棋子造成150到400的随机魔法伤害和1到4秒的随机眩晕。

**Lava Shaman** (熔岩萨满, 2-cost) - Lava Swirl (name inferred from EN patch text) (熔岩漩涡)
- 1-star (translated): Lava Shaman summons a lava swirl with a radius of 1.5 grids centered on itself. After 2s, all enemies inside take 150 pure damage and are silenced for 2.5s. CD 10s.
- 2-star (translated): Lava Shaman summons a lava swirl with a radius of 2.5 grids centered on itself. After 2s, all enemies inside take 200 pure damage and are silenced for 3s. CD 8s.
- 3-star (translated): Lava Shaman summons a lava swirl with a radius of 3 grids centered on itself. After 2s, all enemies inside take 300 pure damage and are silenced for 4s. CD 6s.

**Light Blade** (灵刃骑士, 2-cost) - Moon Glaives (灵刃)
- 1-star (official EN, older): Attacks bounce between enemy pieces for at most 3 times, reducing 30% damages for every time it bounces
- 2-star (official EN, older): Attacks bounce between enemy pieces for at most 6 times, reducing 30% damages for every time it bounces
- 3-star (official EN, older): Attacks bounce between enemy pieces for at most 9 times, reducing 30% damages for every time it bounces
- 1-star (current ZH): 被动：普攻会在敌方棋子间弹射弹射最多3次,每次弹射伤害递减30%。
- 2-star (current ZH): 被动：普攻会在敌方棋子间弹射弹射最多6次,每次弹射伤害递减30%。
- 3-star (current ZH): 被动：普攻会在敌方棋子间弹射弹射最多9次,每次弹射伤害递减30%。

**Living Sand Saint (unofficial)** (活砂圣女, 2-cost) - Living Sand Blade (unofficial) (活砂利刃)
- 1-star (translated): Living Sand Saint makes a powerful melee attack, dealing magical damage based on her max HP to the target, nearby enemy units, and herself.
- 2-star (translated): Living Sand Saint makes a powerful melee attack, dealing magical damage based on her max HP to the target, nearby enemy units, and herself.
- 3-star (translated): Living Sand Saint makes a powerful melee attack, dealing magical damage based on her max HP to the target, nearby enemy units, and herself.

**Ripper** (撕裂者, 2-cost) - Reaping Death (旋转齿轮)
- 1-star (official EN, older): Deals 150 pure damages to enemy units within 2 grids
- 2-star (official EN, older): Deals 200 pure damages to enemy units within 2 grids
- 3-star (official EN, older): Deals 250 pure damages to enemy units within 2 grids
- 1-star (current ZH): 对附近2格内的敌方棋子造成150纯粹伤害
- 2-star (current ZH): 对附近2格内的敌方棋子造成200纯粹伤害
- 3-star (current ZH): 对附近2格内的敌方棋子造成300纯粹伤害

**Shining Dragon** (光鳞龙, 2-cost) - Light and Shadow (unofficial) (光影)
- 1-star (translated): Locks onto the enemy piece with the highest attack, blinks to the farthest grid from which it can attack that piece, and links itself to it. While linked, the target takes 30 magical damage per second and is Weakened; the effect lasts 5s. If the distance between Shining Dragon and the target becomes >3 grids, or Shining Dragon dies while linked, the link breaks and the target is stunned for 2s. Ignores ability immunity. Weakened: affected unit's damage dealt - 70%.
- 2-star (translated): Locks onto the enemy piece with the highest attack, blinks to the farthest grid from which it can attack that piece, and links itself to it. While linked, the target takes 30 magical damage per second and is Weakened; the effect lasts 5s. If the distance between Shining Dragon and the target becomes >3 grids, or Shining Dragon dies while linked, the link breaks and the target is stunned for 2.5s. Ignores ability immunity. Weakened: affected unit's damage dealt - 80%.
- 3-star (translated): Locks onto the enemy piece with the highest attack, blinks to the farthest grid from which it can attack that piece, and links itself to it. While linked, the target takes 30 magical damage per second and is Weakened; the effect lasts 5s. If the distance between Shining Dragon and the target becomes >3 grids, or Shining Dragon dies while linked, the link breaks and the target is stunned for 3s. Ignores ability immunity. Weakened: affected unit's damage dealt -90%.

**Skull Hunter** (独眼猎手, 2-cost) - Porcupette (召唤豪猪)
- 1-star (official EN, older): Summons a Porcupette, who deals physical damage with base attacks
- 2-star (official EN, older): Summons a Porcupette, who deals magic damage with base attacks
- 3-star (official EN, older): Summons a Porcupette, who deals pure damage with base attacks and forces the enemies within 1 grid to attack itself for 5 seconds. CD: 30 seconds.
- 1-star (current ZH): 召唤豪猪协助战斗
- 2-star (current ZH): 召唤豪猪协助战斗
- 3-star (current ZH): 召唤豪猪协助战斗

**Soul Devourer** (噬魂巫师, 2-cost) - Turning Tables (灵魂连接)
- 1-star (official EN, older): Drains 15 mana from an enemy piece every second for 4 seconds. Then, transfers all his mana to 1 random allied piece, reducing their CD time by 2 second if their ability is on cooldown.
- 2-star (official EN, older): Drains 25 mana from an enemy piece every second for 3 seconds. Then, transfers all his mana to 2 random allied pieces, reducing their CD time by 4 second if their ability is on cooldown.
- 3-star (official EN, older): Drains 45 mana from an enemy piece every second for 2 seconds. Then, transfers all his mana to 3 random allied piece, reducing their CD time by 6 second if their ability is on cooldown.
- 1-star (current ZH): 消耗10点魔法值，每秒从一个敌方棋子处吸收15点魔法值，持续4秒。结束后，将自身所有魔法值转移给1个随机友方棋子，若友方棋子的技能处于冷却状态，则减少其2秒冷却时间。
- 2-star (current ZH): 消耗10点魔法值，每秒从一个敌方棋子处吸收25点魔法值，持续3秒。结束后，将自身所有魔法值转移给2个随机友方棋子，若友方棋子的技能处于冷却状态，则减少其4秒冷却时间。
- 3-star (current ZH): 消耗10点魔法值，每秒从一个敌方棋子处吸收45点魔法值，持续2秒。结束后，将自身所有魔法值转移给3个随机友方棋子，若友方棋子的技能处于冷却状态，则减少其6秒冷却时间。

**Swordman** (剑客, 2-cost) - Blade Fury (旋风斩)
- 1-star (official EN, older): Deals 75 magical damages per second to enemy units within 1 grids in 5 seconds
- 2-star (official EN, older): Deals 150 magical damages per second to enemy units within 1 grids in 5 seconds
- 3-star (official EN, older): Deals 225 magical damages per second to enemy units within 1 grids in 5 seconds
- 1-star (current ZH): 每秒对附近1格内敌方棋子造成70魔法伤害，持续5秒并在持续时间内获得技能免疫。
- 2-star (current ZH): 每秒对附近1格内敌方棋子造成140魔法伤害，持续5秒并在持续时间内获得技能免疫。
- 3-star (current ZH): 每秒对附近1格内敌方棋子造成210魔法伤害，持续5秒并在持续时间内获得技能免疫。

**Venom** (毒液, 2-cost) - Viper Strike (剧毒攻击)
- 1-star (official EN, older): Slows the targeted enemy unit's 40% attack speed and deals 150 magical damages per second in 5 seconds
- 2-star (official EN, older): Slows the targeted enemy unit's 60% attack speed and deals 250 magical damages per second in 5seconds
- 3-star (official EN, older): Slows the targeted enemy unit's 80% attack speed and deals 350 magical damages per second in 5 seconds
- 1-star (current ZH): 使1个随机敌方棋子每秒受到150魔法伤害并降低40%攻速，持续5秒。无视技能免疫
- 2-star (current ZH): 使1个随机敌方棋子每秒受到250魔法伤害并降低60%攻速，持续5秒。无视技能免疫
- 3-star (current ZH): 使1个随机敌方棋子每秒受到350魔法伤害并降低80%攻速，持续5秒。无视技能免疫

**Voice of the Deep (unofficial)** (深潜之音, 2-cost) - Abyssal Chant (unofficial) (深渊咏唱)
- 1-star (translated): Momola sings for 7s. Every 2s, each enemy piece within 2 grids has a 35% chance to fall asleep for 4s. Any non-ability damage wakes a sleeping piece.
- 2-star (translated): Momola sings for 7s. Every 2s, each enemy piece within 3 grids has a 35% chance to fall asleep for 6s. Any non-ability damage wakes a sleeping piece.
- 3-star (translated): Momola sings for 7s. Every 2s, each enemy piece within 4 grids has a 35% chance to fall asleep for 8s. Any non-ability damage wakes a sleeping piece.

**Water Spirit** (水之灵, 2-cost) - Wave Slash (海浪冲击)
- 1-star (official EN, older): Charges to the farthest enemy piece, dealing 200 magical damages to enemies in his path.
- 2-star (official EN, older): Charges to the farthest enemy piece, dealing 300 magical damages to enemies in his path.
- 3-star (official EN, older): Charges to the farthest enemy piece, dealing 400 magical damages to enemies in his path.
- 1-star (current ZH): 冲向最远能攻击到敌方的格子并对路径上的敌方棋子造成200魔法伤害。
- 2-star (current ZH): 冲向最远的敌方棋子身后并对路径上的敌方棋子造成300魔法伤害。
- 3-star (current ZH): 冲向最远的敌方棋子身后并对路径上的敌方棋子造成450魔法伤害。

**Werewolf** (狼人, 2-cost) - Shapeshift (解放兽性)
- 1-star (official EN, older): Transform into a wolf, granting him bonus 20 % maximum HP and summons 2 wolves to battle.
- 2-star (official EN, older): Transform into a wolf, granting him bonus 35 % maximum HP and summons 2 wolves to battle.
- 3-star (official EN, older): Transform into a wolf, granting him bonus 50% maximum HP and summons 2 wolves to battle.
- 1-star (current ZH): 变形为狼并获得20%最大/当前生命加成，同时召唤2只狼攻击敌方棋子。
- 2-star (current ZH): 变形为狼并获得35%最大/当前生命加成，同时召唤2只狼攻击敌方棋子。
- 3-star (current ZH): 变形为狼并获得50%最大/当前生命加成，同时召唤2只狼攻击敌方棋子。

**Wisper Seer** (慧语先知, 2-cost) - Nature's Call (自然之力)
- 1-star (official EN, older): Summons 1 Treant to random grids on the edge of the chessboard to assist in battle.
- 2-star (official EN, older): Summons 2 strong Treants to random grids on the edge of the chessboard to assist in battle.
- 3-star (official EN, older): Summons 2 powerful Treants to random grids on the edge of the chessboard to assist in battle.
- 1-star (current ZH): 在棋盘边缘随机的格子中召唤1个树人协助战斗
- 2-star (current ZH): 在棋盘边缘随机的格子中召唤2个较强的树人协助战斗
- 3-star (current ZH): 在棋盘边缘随机的格子中召唤2个强大的树人协助战斗

**Argali Knight** (盘羊骑士, 3-cost) - Purification (圣光洗礼)
- 1-star (official EN, older): Deals 200 healing to a friendly chess and 250 pure damages to all nearby enemy within 2 grids,
- 2-star (official EN, older): Deals 400 healing to a friendly chess and 350 pure damages to all nearby enemy within 2 grids
- 3-star (official EN, older): Deals 720 healing to a friendly chess and 450 pure damages to all nearby enemy within 2 grids
- 1-star (current ZH): 治疗1个友方棋子200生命值并对其周围2格范围内的敌方棋子造成200纯粹伤害。
- 2-star (current ZH): 治疗1个友方棋子400生命值并对其周围2格范围内的敌方棋子造成400纯粹伤害。
- 3-star (current ZH): 治疗1个友方棋子720生命值并对其周围2格范围内的敌方棋子造成720纯粹伤害。

**Avenge Knight** (复仇骑士, 3-cost) - Strike Back (反击)
- 1-star (translated): Passive: After being attacked, Avenge Knight has a 20% chance to counterattack. The counterattack has 40% lifesteal. If Avenge Knight wins a battle and survives, it permanently gains +10 attack. The bonus is shown on the ability icon.
- 2-star (translated): Passive: After being attacked, Avenge Knight has a 30% chance to counterattack. The counterattack has 40% lifesteal. If Avenge Knight wins a battle and survives, it permanently gains +15 attack. The bonus is shown on the ability icon.
- 3-star (translated): Passive: After being attacked, Avenge Knight has a 45% chance to counterattack. The counterattack has 40% lifesteal. If Avenge Knight wins a battle and survives, it permanently gains +25 attack. The bonus is shown on the ability icon.

**Berserker** (狂战士, 3-cost) - Wind Furyaxe (风之狂斧)
- 1-star (official EN, older): Passive: When attacking enemies within 1 grids, there is a 25% chance to trigger the effect: Deals 250 physical damage to enemy pieces within 1 grids of Berserker, and has a 30% chance to dodge base attacks of enemy pieces being damaged, lasting for 5 seconds. (CD: 4 seconds)\nPassive: When attacking enemies in 1 grids away, there is a 25% chance to trigger the effect: Deals 250 physical damage to enemy pieces in a sector area of Berserker, and the ATK Speed of enemy pieces being damaged will be reduced by 30%, lasting for 5 seconds. (CD: 4 seconds)
- 2-star (official EN, older): Passive: When attacking enemies within 1 grids, there is a 25% chance to trigger the effect: Deals 450 physical damage to enemy pieces within 1 grids of Berserker, and has a 30% chance to dodge base attacks of enemy pieces being damaged, lasting for 5 seconds. (CD: 3 seconds)\nPassive: When attacking enemies in 1 grids away, there is a 25% chance to trigger the effect: Deals 450 physical damage to enemy pieces in a sector area of Berserker, and the ATK Speed of enemy pieces being damaged will be reduced by 30%, lasting for 5 seconds. (CD: 3 seconds)
- 3-star (official EN, older): Passive: When attacking enemies within 1 grids, there is a 25% chance to trigger the effect: Deals 700 physical damage to enemy pieces within 1 grids of Berserker, and has a 30% chance to dodge base attacks of enemy pieces being damaged, lasting for 5 seconds. (CD: 2 seconds)\nPassive: When attacking enemies in 1 grids away, there is a 25% chance to trigger the effect: Deals 700 physical damage to enemy pieces in a sector area of Berserker, and the ATK Speed of enemy pieces being damaged will be reduced by 30%, lasting for 5 seconds. (CD: 2 seconds)
- 1-star (current ZH): 狂战士在攻击非相邻单位时，有概率触发效果：对扇形范围内的敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位攻击速度减少。狂战士在攻击相邻敌方单位时，有概率触发效果：以自身为中心对敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位普通攻击有概率落空 。当狂战士切换攻击目标时，必定触发一次风之狂斧。
- 2-star (current ZH): 狂战士在攻击非相邻单位时，有概率触发效果：对扇形范围内的敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位攻击速度减少。狂战士在攻击相邻敌方单位时，有概率触发效果：以自身为中心对敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位普通攻击有概率落空 。当狂战士切换攻击目标时，必定触发一次风之狂斧。
- 3-star (current ZH): 狂战士在攻击非相邻单位时，有概率触发效果：对扇形范围内的敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位攻击速度减少。狂战士在攻击相邻敌方单位时，有概率触发效果：以自身为中心对敌方单位造成一次普攻伤害并附加额外物理伤害，受到伤害的敌方单位普通攻击有概率落空 。当狂战士切换攻击目标时，必定触发一次风之狂斧。

**Bobo** (啵啵, 3-cost) - Delicious Shell (美味海螺)
- 1-star (official EN, older): Passive: After the battle begins, any ally has a 50% chance to gain the Delicious Shell state for 5 seconds when they are born. Active: Gives random 2 allies Delicious Shell to eat (This effect will give priority to allies that have not eaten Delicious Shells.), lasting 5 seconds. Delicious Shell: When attacking, the enemy unit will get stackable Vulnerability effect (Increases damage received by 1%), and when the Vulnerability accumulates to 10 stacks, the target's recovery effect is additionally reduced by 50%.
- 2-star (official EN, older): Passive: After the battle begins, any ally has a 60% chance to gain the Delicious Shell state for 5 seconds when they are born. Active: Gives random 3 allies Delicious Shell to eat (This effect will give priority to allies that have not eaten Delicious Shells.), lasting 5 seconds. Delicious Shell: When attacking, the enemy unit will get stackable Vulnerability effect (Increases damage received by 1%), and when the Vulnerability accumulates to 10 stacks, the target's recovery effect is additionally reduced by 50%.
- 3-star (official EN, older): Passive: After the battle begins, any ally has a 80% chance to gain the Delicious Shell state for 5 seconds when they are born. Active: Gives random 4 allies Delicious Shell to eat (This effect will give priority to allies that have not eaten Delicious Shells.), lasting 5 seconds. Delicious Shell: When attacking, the enemy unit will get stackable Vulnerability effect (Increases damage received by 1%), and when the Vulnerability accumulates to 10 stacks, the target's recovery effect is additionally reduced by 50%.
- 1-star (current ZH): 被动：战斗开始后，友方任意单位出生时有50%概率获赠‘美味贝壳’状态，持续5秒。 主动：赠送随机2个友方单位‘美味贝壳’食用（该效果会优先选择未食用过‘美味贝壳’的友方单位。），持续5秒。 美味贝壳：攻击时敌方单位获得可叠加的易伤效果（受到的伤害增加1%），易伤累计到10层时额外减少目标50%回复效果。
- 2-star (current ZH): 被动：战斗开始后，友方任意单位出生时有60%概率获赠‘美味贝壳’状态，持续5秒。 主动：赠送随机3个友方单位‘美味贝壳’食用（该效果会优先选择未食用过‘美味贝壳’的友方单位。），持续5秒。 美味贝壳：攻击时敌方单位获得可叠加的易伤效果（受到的伤害增加1%），易伤累计到10层时额外减少目标50%回复效果。
- 3-star (current ZH): 被动：战斗开始后，友方任意单位出生时有80%概率获赠‘美味贝壳’状态，持续5秒。 主动：赠送随机4个友方单位‘美味贝壳’食用（该效果会优先选择未食用过‘美味贝壳’的友方单位。），持续5秒。 美味贝壳：攻击时敌方单位获得可叠加的易伤效果（受到的伤害增加1%），易伤累计到10层时额外减少目标50%回复效果。

**Dominator (Dominiel)** (太古支配者, 3-cost) - Sword Field (剑之领域)
- 1-star (translated): Active: Creates a special space centered on a random enemy unit for 3.5s. Any unit that enters the space is Spatially Imprisoned. While the space exists, own attack speed +100%. Passive: Immune to Spatial Imprisonment. Spatial Imprisonment: cannot move, turn, attack, or cast abilities.
- 2-star (translated): Active: Creates a special space centered on a random enemy unit for 4.5s. Any unit that enters the space is Spatially Imprisoned. While the space exists, own attack speed +100%. Passive: Immune to Spatial Imprisonment. Spatial Imprisonment: cannot move, turn, attack, or cast abilities.
- 3-star (translated): Active: Creates a special space centered on a random enemy unit for 6s. Any unit that enters the space is Spatially Imprisoned. While the space exists, own attack speed +100%. Passive: Immune to Spatial Imprisonment. Spatial Imprisonment: cannot move, turn, attack, or cast abilities.

**Eclipse of Darkness** (蚀光之暗, 3-cost) - Blood Surge (鲜血之潮)
- 1-star (official EN, older): Eclipse of Darkness allows the specified ally piece to absorb max HP from nearby enemies, dealing pure damage to heal the target.
- 2-star (official EN, older): Eclipse of Darkness allows the specified ally piece to absorb max HP from nearby enemies, dealing pure damage to heal the target.
- 3-star (official EN, older): Eclipse of Darkness allows the specified ally piece to absorb max HP from nearby enemies, dealing pure damage to heal the target.
- 1-star (current ZH): 蚀光之暗使指定的友方棋子目标从附近的敌方棋子身上吸取最大生命值，并造成纯粹伤害用于治疗目标。
- 2-star (current ZH): 蚀光之暗使指定的友方棋子目标从附近的敌方棋子身上吸取最大生命值，并造成纯粹伤害用于治疗目标。
- 3-star (current ZH): 蚀光之暗使指定的友方棋子目标从附近的敌方棋子身上吸取最大生命值，并造成纯粹伤害用于治疗目标。

**Ember Blade (Di Wei)** (黯焰之刃, 3-cost) - Catching Fire (星火燎原)
- 1-star (translated): Attacks every enemy piece in the target area, then returns to its original position. Each hit counts as a normal attack and deals a fixed 30*(current number of enemies) physical damage. Killing an enemy piece resets this ability's cooldown.
- 2-star (translated): Attacks every enemy piece in the target area, then returns to its original position. Each hit counts as a normal attack and deals a fixed 60*(current number of enemies) physical damage. Killing an enemy piece resets this ability's cooldown.
- 3-star (translated): Attacks every enemy piece in the target area, then returns to its original position. Each hit counts as a normal attack and deals a fixed 120*(current number of enemies) physical damage. Killing an enemy piece resets this ability's cooldown.

**Evil Knight** (邪灵骑士, 3-cost) - Shield of Skeleton (白骨之盾)
- 1-star (official EN, older): Creates a shield that absorbs 200 damages in 10 seconds for an allied piece. When the shield is destroyed it will burst and deal damages equal to the amount absorbed to an area of 3 radius around it.
- 2-star (official EN, older): Creates a shield that absorbs 300 damages in 10 seconds for an allied piece. When the shield is destroyed it will burst and deal damages equal to the amount absorbed to an area of 3 radius around it.
- 3-star (official EN, older): Creates a shield that absorbs 600 damages in 10 seconds for an allied piece. When the shield is destroyed it will burst and deal damages equal to the amount absorbed to an area of 3 radius around it.
- 1-star (current ZH): 为1个友方棋子添加一个持续10秒且最多吸收200伤害的护盾，护盾失效时对附近3格范围内敌方棋子造成等同于吸收伤害量的魔法伤害。
- 2-star (current ZH): 为1个友方棋子添加一个持续10秒且最多吸收300伤害的护盾，护盾失效时对附近3格范围内敌方棋子造成等同于吸收伤害量的魔法伤害。
- 3-star (current ZH): 为1个友方棋子添加一个持续10秒且最多吸收600伤害的护盾，护盾失效时对附近3格范围内敌方棋子造成等同于吸收伤害量的魔法伤害。

**Fallen Witcher** (堕落猎魔人, 3-cost) - Metamorphism (魔化)
- 1-star (official EN, older): Transform to a piece with ranged attack and change HP with a random allied units, granting him bonus 50 % attack speed and 80 damages for 60 seconds. Upon transformation.
- 2-star (official EN, older): Transform to a piece with ranged attack and and change HP with a random allied units, granting him bonus 70% attack speed and 100 damages for 60 seconds. Upon transformation.
- 3-star (official EN, older): Transform to a piece with ranged attack andand change HP with a random allied units, granting him bonus 100 % attack speed and 160 damages for 60 seconds. Upon transformation.
- 1-star (current ZH): 魔化：变身为可怕的恶魔，攻击方式变更为远程，并增加攻击力和攻击速度。
- 2-star (current ZH): 同时腐化身边距离最近的友方棋子，使其变身为堕落猎魔人
- 3-star (current ZH): 同时腐化身边距离最近的友方棋子，使其变身为堕落猎魔人

**Fortune Teller** (占卜师, 3-cost) - Life Extension (续命)
- 1-star (official EN, older): Prevents 1 allied piece from being eliminated within 5 seconds
- 2-star (official EN, older): Prevents 2 allied piece from being eliminated within 5 seconds
- 3-star (official EN, older): Prevents 3 allied piece from being eliminated within 5 seconds
- 1-star (current ZH): 使1个友方棋子在5秒内不会被击败
- 2-star (current ZH): 使2个友方棋子在5秒内不会被击败
- 3-star (current ZH): 使3个友方棋子在5秒内不会被击败

**Gem Artisan** (宝石魔匠, 3-cost) - Gemstone Wave (魔法石波动)
- 1-star (official EN, older): Uses gemstone to teleport Gem Artisan (including itself and all pieces of the same name) to somewhere near a random enemy unit, dealing 200 pure damage to nearby enemies within 1 grid for every Gem Artisan that reaches the area.
- 2-star (official EN, older): Uses gemstone to teleport Gem Artisan (including itself and all pieces of the same name) to somewhere near a random enemy unit, dealing 250 pure damage to nearby enemies within 1 grid for every Gem Artisan that reaches the area.
- 3-star (official EN, older): Uses gemstone to teleport Gem Artisan (including itself and all pieces of the same name) to somewhere near a random enemy unit, dealing 300 pure damage to nearby enemies within 1 grid for every Gem Artisan that reaches the area.
- 1-star (current ZH): 运用魔法石将宝石魔匠（包括自身和所有同名棋子）传送至随机敌方单位附近，每传送一名宝石魔匠到达该区域，对附近1格敌人造成200点纯粹伤害。
- 2-star (current ZH): 运用魔法石将宝石魔匠（包括自身和所有同名棋子）传送至随机敌方单位附近，每传送一名宝石魔匠到达该区域，对附近1格敌人造成250点纯粹伤害。
- 3-star (current ZH): 运用魔法石将宝石魔匠（包括自身和所有同名棋子）传送至随机敌方单位附近，每传送一名宝石魔匠到达该区域，对附近1格敌人造成300点纯粹伤害。

**Ghost Kid** (鬼灵萝莉, 3-cost) - Chew Chew (咬咬)
- 1-star (official EN, older): Anna throws the Big Head to the farthest enemy, dragging the target back to her and dealing 200 Physical Damage. If the target is dragged to be within 1 grid around Anna, deals a total of 150 Magical Damage over 3 times, and stuns the target for 4.5 seconds.
- 2-star (official EN, older): Anna throws the Big Head to the farthest enemy, dragging the target back to her and dealing 300 Physical Damage. If the target is dragged to be within 1 grid around Anna, deals a total of 300 Magical Damage over 3 times, and stuns the target for 4.5 seconds.
- 3-star (official EN, older): Anna throws the Big Head to the farthest enemy, dragging the target back to her and dealing 500 Physical Damage. If the target is dragged to be within 1 grid around Anna, deals a total of 200 Magical Damage over 3 times, and stuns the target for 4.5 seconds.
- 1-star (current ZH): 安娜向一个距离最远的敌方棋子丢出大脑袋，将该棋子拖回身前并造成200点物理伤害。如果拖回后的棋子与安娜相距1格以内，造成3次总计150点的魔法伤害，并使其眩晕4.5秒。
- 2-star (current ZH): 安娜向一个距离最远的敌方棋子丢出大脑袋，将该棋子拖回身前并造成300点物理伤害。如果拖回后的棋子与安娜相距1格以内，造成3次总计300点的魔法伤害，并使其眩晕4.5秒。
- 3-star (current ZH): 安娜向一个距离最远的敌方棋子丢出大脑袋，将该棋子拖回身前并造成500点物理伤害。如果拖回后的棋子与安娜相距1格以内，造成3次总计600点的魔法伤害，并使其眩晕4.5秒。

**God of War** (战神, 3-cost) - Shield Crash (持盾猛击)
- 1-star (official EN, older): Reduces Physical Damage by 30% upon receiving attack from the front, and by 15% from the side. Meanwhile, deals 300 Physical Damage to enemies within 3 grids ahead every 8 seconds.Also knocks back enemies within range for 1 grid.
- 2-star (official EN, older): Reduces Physical Damage by 45% upon receiving attack from the front, and by 22% from the side. Meanwhile, deals 400 Physical Damage to enemies within 3 grids ahead every 8 seconds. Also knocks back enemies within range for 1 grid.
- 3-star (official EN, older): Reduces Physical Damage by 60% upon receiving attack from the front, and by 30% from the side. Meanwhile, deals 500 Physical Damage to enemies within 3 grids ahead every 8 seconds. Also knocks back enemies within range for 1 grid.
- 1-star (current ZH): 正面物理减伤30%,侧面物理减伤15%，每隔10秒，对前方3格内敌方棋子造成250伤害，并击退1格。
- 2-star (current ZH): 正面物理减伤45%,侧面物理减伤22%，每隔9秒，对前方3格内敌方棋子造成350伤害，并击退1格。
- 3-star (current ZH): 正面物理减伤60%,侧面物理减伤30%，每隔8秒，对前方3格内敌方棋子造成500伤害，并击退1格。

**Goddess of War (Valkyrie)** (女武神, 3-cost) - Shining Combo (闪耀连击)
- 1-star (official EN, older): Ability: [Shining Combo] Rapidly swings a warhammer, dealing physical damage with base attacks to surrounding enemies. The final strike slams the hammer into the ground, dealing additional damage and stunning enemies. Swing/Slam Extra DMG: 15/30/45 Stun Duration: 0.5/0.5/1 Swing Range: 160 Slam Range: 160/160/234 CD: 10/8/6
- 2-star (official EN, older): Ability: [Shining Combo] Rapidly swings a warhammer, dealing physical damage with base attacks to surrounding enemies. The final strike slams the hammer into the ground, dealing additional damage and stunning enemies. Swing/Slam Extra DMG: 15/30/45 Stun Duration: 0.5/0.5/1 Swing Range: 160 Slam Range: 160/160/234 CD: 10/8/6
- 3-star (official EN, older): Ability: [Shining Combo] Rapidly swings a warhammer, dealing physical damage with base attacks to surrounding enemies. The final strike slams the hammer into the ground, dealing additional damage and stunning enemies. Swing/Slam Extra DMG: 15/30/45 Stun Duration: 0.5/0.5/1 Swing Range: 160 Slam Range: 160/160/234 CD: 10/8/6
- 1-star (current ZH): 技能：[闪耀连击] 快速挥舞战锤，对周围的敌人造成普通攻击物理伤害。最后一次打击时，将战锤击向地面，还会对敌人造成眩晕。 挥击/砸地额外伤害：15 / 30 / 45 眩晕时间：0.5 / 0.5 / 1 挥击范围：160 砸地范围：160 / 160 / 234
- 2-star (current ZH): 技能：[闪耀连击] 快速挥舞战锤，对周围的敌人造成普通攻击物理伤害。最后一次打击时，将战锤击向地面，还会对敌人造成眩晕。 挥击/砸地额外伤害：15 / 30 / 45 眩晕时间：0.5 / 0.5 / 1 挥击范围：160 砸地范围：160 / 160 / 234
- 3-star (current ZH): 技能：[闪耀连击] 快速挥舞战锤，对周围的敌人造成普通攻击物理伤害。最后一次打击时，将战锤击向地面，还会对敌人造成眩晕。 挥击/砸地额外伤害：15 / 30 / 45 眩晕时间：0.5 / 0.5 / 1 挥击范围：160 砸地范围：160 / 160 / 234

**Grand Herald** (神使, 3-cost) - Take Control (掌控)
- 1-star (official EN, older): Passive: Reduces Skill CD by 10%. Active: Clones one skill of the nearest centrosymmetric enemy when the battle starts
- 2-star (official EN, older): Passive: Reduces Skill CD by 20%. Active: Clones one skill of the nearest centrosymmetric enemy when the battle starts
- 3-star (official EN, older): Passive: Reduces Skill CD by 30%. Active: Clones one skill of the nearest centrosymmetric enemy when the battle starts
- 1-star (current ZH): 主动：战斗开始时，神使会随机选择一个敌方棋子单位，复制他的主动技能和攻击射程，代替掌控。 位于棋盘上的神使会在主场战斗回合战败时或者棋子升星的时候将自己的技能重置为掌控。 复制的技能跟随神使的棋子星级变化。
- 2-star (current ZH): 主动：战斗开始时，神使会随机选择一个敌方棋子单位，复制他的主动技能和攻击射程，代替掌控。 位于棋盘上的神使会在主场战斗回合战败时或者棋子升星的时候将自己的技能重置为掌控。 复制的技能跟随神使的棋子星级变化。
- 3-star (current ZH): 主动：战斗开始时，神使会随机选择一个敌方棋子单位，复制他的主动技能和攻击射程，代替掌控。 位于棋盘上的神使会在主场战斗回合战败时或者棋子升星的时候将自己的技能重置为掌控。 复制的技能跟随神使的棋子星级变化。

**Jade General (Ji Xi)** (玉石之军, 3-cost) - Rock-breaking (石破天惊)
- 1-star (translated): Summons 2 boulders from the edge of the board. Each one smashes along a row or column, stunning enemy units in that line for 2s and dealing 250 physical damage. Killing an enemy unit resets this ability's cooldown.
- 2-star (translated): Summons 2 boulders from the edge of the board. Each one smashes along a row or column, stunning enemy units in that line for 2s and dealing 350 physical damage. Killing an enemy unit resets this ability's cooldown.
- 3-star (translated): Summons 3 boulders from the edge of the board. Each one smashes along a row or column, stunning enemy units in that line for 2.5s and dealing 450 physical damage. Killing an enemy unit resets this ability's cooldown.

**Lord of Sand** (沙地之主, 3-cost) - Burrow Dash (流沙穿刺)
- 1-star (official EN, older): Burrows into the ground and tunnels to the farthest piece on the board, dealing 150 damages and stun enemy units in the path for 1.5 seconds
- 2-star (official EN, older): Burrows into the ground and tunnels to the farthest piece on the board, dealing 250 damages and stun enemy units in the path for 1.5 seconds
- 3-star (official EN, older): Burrows into the ground and tunnels to the farthest piece on the board, dealing 450 damages and stun enemy units in the path for 1.5 seconds
- 1-star (current ZH): 钻到最远能攻击到敌人的格子并对路径上的敌方棋子造成150魔法伤害和1.5秒眩晕。
- 2-star (current ZH): 钻到最远能攻击到敌人的格子并对路径上的敌方棋子造成250魔法伤害和1.5秒眩晕。
- 3-star (current ZH): 钻到最远能攻击到敌人的格子并对路径上的敌方棋子造成450魔法伤害和1.5秒眩晕。

**Mist Phantom King (Wuril)** (雾霭魔君, 3-cost) - Smoke Trick (烟幕迷踪)
- 1-star (official EN, older): Instantly deploys a smoke grenade. If there are enemy pieces nearby, the grenade will be deployed at the location of Wuril; otherwise, it will be deployed at the symmetric point on the board from Wuril's position. Enemies within the smoke grenade will be silenced and blinded (with a chance of missing attacks).
- 2-star (official EN, older): Instantly deploys a smoke grenade. If there are enemy pieces nearby, the grenade will be deployed at the location of Wuril; otherwise, it will be deployed at the symmetric point on the board from Wuril's position. Enemies within the smoke grenade will be silenced and blinded (with a chance of missing attacks).
- 3-star (official EN, older): Instantly deploys a smoke grenade. If there are enemy pieces nearby, the grenade will be deployed at the location of Wuril; otherwise, it will be deployed at the symmetric point on the board from Wuril's position. Enemies within the smoke grenade will be silenced and blinded (with a chance of missing attacks).
- 1-star (current ZH): 立即投放一颗烟雾弹。如果身边有敌方棋子,烟雾弹将投放在乌日尔所在的位置;否则将投放在棋盘上乌日尔位置的中心对称点。 烟雾弹中的敌方棋子会被沉默并且致盲(攻击有概率丢失)
- 2-star (current ZH): 立即投放一颗烟雾弹。如果身边有敌方棋子,烟雾弹将投放在乌日尔所在的位置;否则将投放在棋盘上乌日尔位置的中心对称点。 烟雾弹中的敌方棋子会被沉默并且致盲(攻击有概率丢失)
- 3-star (current ZH): 立即投放一颗烟雾弹。如果身边有敌方棋子,烟雾弹将投放在乌日尔所在的位置;否则将投放在棋盘上乌日尔位置的中心对称点。 烟雾弹中的敌方棋子会被沉默并且致盲(攻击有概率丢失)

**Nightmare (formerly Egersis Prophet)** (梦魇, 3-cost) - Wrecking Ball (unofficial) (破坏球)
- 1-star (translated): Over 5s, summons 1 orb every 0.2s that deals 30 physical damage to 1 enemy. When the ability ends, restores HP equal to 25% of the damage dealt.
- 2-star (translated): Over 5s, summons 1 orb every 0.2s that deals 65 physical damage to 1 enemy. When the ability ends, restores HP equal to 25% of the damage dealt.
- 3-star (translated): Over 5s, summons 1 orb every 0.2s that deals 30/65/115 physical damage to 1 enemy. When the ability ends, restores HP equal to 25% of the damage dealt.

**Penitent Bishop (Mammon)** (讨罪司教, 3-cost) - Ancestral Curse (先民诅咒)
- 1-star (official EN, older): Penitent Bishop curses the enemy it attacks. When a cursed enemy is healed, Penitent Bishop heals allies around the target by a percentage. When a cursed enemy takes damage, Penitent Bishop deals the same type of damage, scaled by a percentage, to enemies around the target. Damage & Healing Share Percentage: 30% Effect Range: 27 CD: 5
- 2-star (official EN, older): Penitent Bishop curses the enemy it attacks. When a cursed enemy is healed, Penitent Bishop heals allies around the target by a percentage. When a cursed enemy takes damage, Penitent Bishop deals the same type of damage, scaled by a percentage, to enemies around the target. Damage & Healing Share Percentage: 40% Effect Range: 313 CD: 5
- 3-star (official EN, older): Penitent Bishop curses the enemy it attacks. When a cursed enemy is healed, Penitent Bishop heals allies around the target by a percentage. When a cursed enemy takes damage, Penitent Bishop deals the same type of damage, scaled by a percentage, to enemies around the target. Damage & Healing Share Percentage: / 60% Effect Range: 391 CD: 5
- 1-star (current ZH): 讨罪司教对攻击的敌方目标施加诅咒。被诅咒的敌方目标受到治疗时，讨罪司教会对目标周围的友方棋子按百分比进行治疗。被诅咒的敌方目标受到伤害时，讨罪司教会对目标周围的敌方棋子按百分比造成同类型伤害。 技能类型：主动 目标：范围 影响：友方/敌方 伤害和治疗共享百分比：30% 作用范围：273
- 2-star (current ZH): 讨罪司教对攻击的敌方目标施加诅咒。被诅咒的敌方目标受到治疗时，讨罪司教会对目标周围的友方棋子按百分比进行治疗。被诅咒的敌方目标受到伤害时，讨罪司教会对目标周围的敌方棋子按百分比造成同类型伤害。 技能类型：主动 目标：范围 影响：友方/敌方 伤害和治疗共享百分比： 40% 作用范围：313 冷却时间：5
- 3-star (current ZH): 讨罪司教对攻击的敌方目标施加诅咒。被诅咒的敌方目标受到治疗时，讨罪司教会对目标周围的友方棋子按百分比进行治疗。被诅咒的敌方目标受到伤害时，讨罪司教会对目标周围的敌方棋子按百分比造成同类型伤害。 技能类型：主动 目标：范围 影响：友方/敌方 伤害和治疗共享百分比：60% 作用范围：391

**Poisonous Worm** (魔化毒虫, 3-cost) - Plague Ward (瘟疫守卫)
- 1-star (official EN, older): Summons an unmovable plague ward which slows 40% of target's attack speed in 5 seconds. Plague ward is immune to ability damage.
- 2-star (official EN, older): Summons an unmovable strong plague ward which slows 40% of target's attack speed in 5 seconds. Plague ward is immune to ability damage.
- 3-star (official EN, older): Summons two unmovable stronger plague wards which slows 40% of target's attack speed in 5 seconds. Plague ward is immune to ability damage.
- 1-star (current ZH): 召唤1个攻击使敌方棋子在5秒内攻速降低40%的毒虫守卫。
- 2-star (current ZH): 召唤1个攻击使敌方棋子在5秒内攻速降低40%的毒虫守卫。
- 3-star (current ZH): 召唤2个攻击使敌方棋子在5秒内攻速降低40%的毒虫守卫。

**Qin Xuan / Plume Warrior** (秦璇, 3-cost) - Fist of Gale (疾风烈气拳)
- 1-star (official EN, older): Plume Warrior wraps her fists with a gale for 10 seconds. She will take a short rest and attack again in 2 seconds after the gale effect disappears or she deals the last blow.\nGale: Reach the upper limit of the ATK Speed within 2 seconds and release the Fist of Gale that can punch up to 3 times, and the last blow will deal 60 magical damage to enemy pieces within 2 grids around the target, and reduce their ATK Speed by 30% for 2 seconds.
- 2-star (official EN, older): Plume Warrior wraps her fists with a gale for 10 seconds. She will take a short rest and attack again in 2 seconds after the gale effect disappears or she deals the last blow.\nGale: Reach the upper limit of the ATK Speed within 2 seconds and release the Fist of Gale that can punch up to 4 times, and the last blow will deal 120 magical damage to enemy pieces within 2 grids around the target, and reduce their ATK Speed by 40% for 2 seconds.
- 3-star (official EN, older): Plume Warrior wraps her fists with a gale for 10 seconds. She will take a short rest and attack again in 2 seconds after the gale effect disappears or she deals the last blow.\nGale: Reach the upper limit of the ATK Speed within 2 seconds and release the Fist of Gale that can punch up to 5 times, and the last blow will deal 240 magical damage to enemy pieces within 3 grids around the target, and reduce their ATK Speed by 50% for 2 seconds.
- 1-star (current ZH): 风羽武者用疾风包裹双拳，持续10秒。在疾风效果消失或释放最后一击后，风羽武者将会短暂休息，2秒后再次攻击。\n疾风：在2秒内达到自身攻速上限并释放疾风烈气拳。疾风烈气拳最多可出拳3次，最后一击将对目标周围2格的敌方棋子造成60点魔法伤害，并减少其30%攻击速度2秒。
- 2-star (current ZH): 风羽武者用疾风包裹双拳，持续10秒。在疾风效果消失或释放最后一击后，风羽武者将会短暂休息，2秒后再次攻击。\n疾风：在2秒内达到自身攻速上限并释放疾风烈气拳。疾风烈气拳最多可出拳4次，最后一击将对目标周围2格的敌方棋子造成120点魔法伤害，并减少其40%攻击速度2秒。
- 3-star (current ZH): 风羽武者用疾风包裹双拳，持续10秒。在疾风效果消失或释放最后一击后，风羽武者将会短暂休息，2秒后再次攻击。\n疾风：在2秒内达到自身攻速上限并释放疾风烈气拳。疾风烈气拳最多可出拳5次，最后一击将对目标周围3格的敌方棋子造成240点魔法伤害，并减少其50%攻击速度2秒。

**Shadow Devil** (暗影魔王, 3-cost) - Requiem of Shadow (影之歌)
- 1-star (official EN, older): After 1.7s delay, Shadow Devil releases 8 shockwaves around itself, each dealing 200 magical damage. For every piece in the lineup that activates a Demon Synergy, the number of shockwaves +1.
- 2-star (official EN, older): After 1.7s delay, Shadow Devil releases 9 shockwaves around itself, each dealing 250 magical damage. For every piece in the lineup that activates a Demon Synergy, the number of shockwaves +1.
- 3-star (official EN, older): After 1.7s delay, Shadow Devil releases 10 shockwaves around itself, each dealing 300 magical damage. For every piece in the lineup that activates a Demon Synergy, the number of shockwaves +1.
- 1-star (current ZH): 在延迟1.7秒后，以自身为中心向四周释放8道冲击波，每道冲击波造成200点魔法伤害，且阵容中每有一个触发了恶魔羁绊的棋子，冲击波的数量就会额外+1
- 2-star (current ZH): 在延迟1.7秒后，以自身为中心向四周释放9道冲击波，每道冲击波造成250点魔法伤害，且阵容中每有一个触发了恶魔羁绊的棋子，冲击波的数量就会额外+1
- 3-star (current ZH): 在延迟1.7秒后，以自身为中心向四周释放10道冲击波，每道冲击波造成300点魔法伤害，且阵容中每有一个触发了恶魔羁绊的棋子，冲击波的数量就会额外+1

**Shadowcrawler** (幽影夜行者, 3-cost) - Coup de Ombre (幽影之击)
- 1-star (official EN, older): Has a 10% chance to deal 3 times damages on attack
- 2-star (official EN, older): Has a 10%chance to deal 4.5 times damages on attack
- 3-star (official EN, older): Has a 10%chance to deal 6 times damages on attack
- 1-star (current ZH): 被动：普攻有10%概率造成300%伤害。
- 2-star (current ZH): 被动：普攻有15%概率造成450%伤害。
- 3-star (current ZH): 被动：普攻有20%概率造成600%伤害。

**Thunder Spirit** (雷之灵, 3-cost) - Lightning Nova (闪电新星)
- 1-star (official EN, older): Release a wave of plasma that grows in power as it expands, dealing 100 damages in max radius of 3 and 10 as minimum damages.
- 2-star (official EN, older): Release a wave of plasma that grows in power as it expands, dealing 175 damages in max radius of 3 and 10 as minimum damages.
- 3-star (official EN, older): Release a wave of plasma that grows in power as it expands, dealing 250 damages in max radius of 3 and 10 as minimum damages.
- 1-star (current ZH): 释放逐渐扩散至3格范围的等离子场对敌方棋子造成50到125伤害,距离越远伤害越高。
- 2-star (current ZH): 释放逐渐扩散至4格范围的等离子场对敌方棋子造成75到225伤害,距离越远伤害越高。
- 3-star (current ZH): 释放逐渐扩散至4格范围的等离子场对敌方棋子造成125到325伤害,距离越远伤害越高。

**Thunder Trace (Lei Rong)** (雷影之贤, 3-cost) - Ball Lightning (风驰电掣)
- 1-star (translated): Turns into lightning and rampages across the battlefield, dealing magical damage to enemy pieces it hits. Damage increases by 60 for each grid traveled, with a minimum of 60 damage. Killing an enemy piece resets this ability's cooldown.
- 2-star (translated): Turns into lightning and rampages across the battlefield, dealing magical damage to enemy pieces it hits. Damage increases by 90 for each grid traveled, with a minimum of 90 damage. Killing an enemy piece resets this ability's cooldown.
- 3-star (translated): Turns into lightning and rampages across the battlefield, dealing magical damage to enemy pieces it hits. Damage increases by 150 for each grid traveled, with a minimum of 150 damage. Killing an enemy piece resets this ability's cooldown.

**Umbra** (影, 3-cost) - Familiar (佣兽)
- 1-star (official EN, older): Increases Umbra's current/max HP by 80% and summons 2 Familiars to battle. Familiars share HP with Umbra.
- 2-star (official EN, older): Increases Umbra's current/max HP by 80% and summons 2 strong Familiars to battle. Familiars share HP with Umbra.
- 3-star (official EN, older): Increases Umbra's current/max HP by 80% and summons 2 stronger Familiars to battle. Familiars share HP with Umbra.
- 1-star (current ZH): 当前/最大生命值增加70%并召唤2个佣兽参与战斗，佣兽和自己共享生命值。
- 2-star (current ZH): 当前/最大生命值增加90%并召唤2个强大的佣兽参与战斗，佣兽和自己共享生命值
- 3-star (current ZH): 当前/最大生命值增加110%并召唤2个更加强大的佣兽参与战斗，佣兽和自己共享生命值

**Underworld Enforcer (unofficial)** (冥界执法官, 3-cost) - Death Hymn (unofficial) (死亡颂歌)
- 1-star (translated): Sells its soul to Death: becomes immune to physical damage, gains attack, and all damage it deals becomes magical damage (except pure damage). Passive: attacks have a chance to fire one more shot. Trigger chance: 30%
- 2-star (translated): Sells its soul to Death: becomes immune to physical damage, gains attack, and all damage it deals becomes magical damage (except pure damage). Passive: attacks have a chance to fire one more shot. Trigger chance: 40%
- 3-star (translated): Sells its soul to Death: becomes immune to physical damage, gains attack, and all damage it deals becomes magical damage (except pure damage). Passive: attacks have a chance to fire one more shot. Trigger chance: 50%

**Warpwood Sage** (扭木贤者, 3-cost) - Leech Seed (生命之种)
- 1-star (official EN, older): Deals 12.5 Magical damage to a random enemy piece within 3 grids every 0.25 seconds, and recovers his own HP based on damage dealt for 5 seconds.
- 2-star (official EN, older): Deals 18.75 Magical damage to a random enemy piece within 3 grids every 0.25 seconds, and recovers his own HP based on damage dealt for 5 seconds.
- 3-star (official EN, older): Deals 25 Magical damage to a random enemy piece within 3 grids every 0.25 seconds, and recovers his own HP based on damage dealt for 5 seconds.
- 1-star (current ZH): 使周围3格随机一个敌方棋子每0.25秒受到15魔法伤害并根据伤害治疗自己，持续5秒
- 2-star (current ZH): 使周围3格随机一个敌方棋子每0.25秒受到20魔法伤害并根据伤害治疗自己，持续5秒
- 3-star (current ZH): 使周围3格随机一个敌方棋子每0.25秒受到30魔法伤害并根据伤害治疗自己，持续5秒

**Wind Ranger** (逐风游侠, 3-cost) - Powershot (穿云箭)
- 1-star (official EN, older): Charges the bow for up to 2 seconds, dealing at most 400 damages to enemy units in the path. For each enemy that powershot hits, its damages is reduced by 10%
- 2-star (official EN, older): Charges the bow for up to 2 seconds, dealing at most 600 damages to enemy units in the path. For each enemy that powershot hits, its damages is reduced by 10%
- 3-star (official EN, older): Charges the bow for up to 2 seconds, dealing at most 800 damages to enemy units in the path. For each enemy that powershot hits, its damages is reduced by 10%
- 1-star (current ZH): 蓄力最多2秒并射出箭矢对路径上的敌方棋子造成最多400魔法伤害，每穿过1名敌方棋子伤害降低-5%。被穿云箭命中的敌方单位护甲减少4，持续10s
- 2-star (current ZH): 蓄力最多2秒并射出箭矢对路径上的敌方棋子造成最多600魔法伤害，每穿过1名敌方棋子伤害降低-5%。被穿云箭命中的敌方单位护甲减少6，持续10s
- 3-star (current ZH): 蓄力最多2秒并射出箭矢对路径上的敌方棋子造成最多800魔法伤害，每穿过1名敌方棋子伤害降低-5%。被穿云箭命中的敌方单位护甲减少10，持续10s

**Cave Prodigy** (猪猪牧师, 4-cost) - Hero's Back (英雄归来)
- 1-star (official EN, older): Summons a unit of the highest cost that has been killed in this round, grants it with 100 initial mana. If it’s a chess piece or a summon, it will have the same star level as Cave Prodigy; if it’s a creep, then it will stay the same.
- 2-star (official EN, older): Summons a unit of the highest cost that has been killed in this round, grants it with 100 initial mana. If it’s a chess piece or a summon, it will have the same star level as Cave Prodigy; if it’s a creep, then it will stay the same.
- 3-star (official EN, older): Summons a unit of the highest cost that has been killed in this round, grants it with 100 initial mana. If it’s a chess piece or a summon, it will have the same star level as Cave Prodigy; if it’s a creep, then it will stay the same.
- 1-star (current ZH): 猪猪牧师将复活一个在本回合已阵亡的单位，并使其星级和猪猪牧师相等。复活目标优先选择野怪单位，其次为售价最高的棋子单位。（冷却时间：）
- 2-star (current ZH): 猪猪牧师将复活一个在本回合已阵亡的单位，并使其星级和猪猪牧师相等。复活目标优先选择野怪单位，其次为售价最高的棋子单位。（冷却时间：）
- 3-star (current ZH): 猪猪牧师将复活一个在本回合已阵亡的单位，并使其星级和猪猪牧师相等。复活目标优先选择野怪单位，其次为售价最高的棋子单位。（冷却时间：）

**Doom Arbiter** (末日审判官, 4-cost) - Doom Curse (末世诅咒)
- 1-star (official EN, older): Silence an enemy piece, dealing 60 pure damages in 10 seconds
- 2-star (official EN, older): Silence an enemy piece, dealing 90 pure damages in 15 seconds
- 3-star (official EN, older): Silence an enemy piece, dealing 120 pure damages in 20 seconds
- 1-star (current ZH): 沉默1个敌方棋子并使其每秒受到50纯粹伤害，持续10秒。
- 2-star (current ZH): 沉默1个敌方棋子并使其每秒受到75纯粹伤害，持续15秒。
- 3-star (current ZH): 沉默1个敌方棋子并使其每秒受到120纯粹伤害，持续20秒。

**Dragon Knight** (龙骑士, 4-cost) - Resonance (共鸣)
- 1-star (official EN, older): Transforms into a powerful dragon form, granting additional 2 grids of attack range and 100 attack damages. Normal attack deals consitent magical damage.
- 2-star (official EN, older): Transforms into a powerful dragon form, granting additional 2 grids of attack range and 125 attack damages. Normal attack deals consitent magical damage.
- 3-star (official EN, older): Transforms into a powerful dragon form, granting additional 2 grids of attack range and 150 attack damages. Normal attack deals consitent magical damage.
- 1-star (current ZH): 变身为“龙”形态使攻击距离增加2格、攻击力增加100点，普通攻击变为半径2格的范围攻击。
- 2-star (current ZH): 变身为“龙”形态使攻击距离增加2格、攻击力增加150点，普通攻击变为半径2格的范围攻击，使受击棋子受到减少40%攻击速度，持续5秒。
- 3-star (current ZH): 变身为“龙”形态使攻击距离增加2格、攻击力增加200点，普通攻击变为半径2格的范围攻击，使受击棋子受到每秒最大生命值5%的魔法伤害并减少40%攻击速度，持续5秒（该持续伤害不可叠加）

**Evernight Watcher** (永夜守望者, 4-cost) - Evernight Hold (永夜之握)
- 1-star (translated): Evernight Watcher alters the properties of matter, forcibly dragging nearby enemies in front of him, then strikes them with a powerful shock that deals damage and stuns.
- 2-star (translated): Evernight Watcher alters the properties of matter, forcibly dragging nearby enemies in front of him, then strikes them with a powerful shock that deals damage and stuns.
- 3-star (translated): Evernight Watcher alters the properties of matter, forcibly dragging nearby enemies in front of him, then strikes them with a powerful shock that deals damage and stuns.

**Fire Spirit** (火之灵, 4-cost) - Rebirth from Flames (unofficial) (浴火重生)
- 1-star (translated): Fire Spirit turns into Ember form and taunts enemies within 2 grids for 5s. When Ember form ends, Fire Spirit deals 400 magical damage to enemies within 2 grids and restores 100% of its own HP. Ember: cannot attack or move; inherits 100% of Fire Spirit's attributes and benefits from Fire Spirit's items and synergy bonuses.
- 2-star (translated): Fire Spirit turns into Ember form and taunts enemies within 2 grids for 5s. When Ember form ends, Fire Spirit deals 600 magical damage to enemies within 2 grids and restores 100% of its own HP. Ember: cannot attack or move; inherits 100% of Fire Spirit's attributes and benefits from Fire Spirit's items and synergy bonuses.
- 3-star (translated): Fire Spirit turns into Ember form and taunts enemies within 2 grids for 5s. When Ember form ends, Fire Spirit deals 1000 magical damage to enemies within 2 grids and restores 100% of its own HP. Ember: cannot attack or move; inherits 100% of Fire Spirit's attributes and benefits from Fire Spirit's items and synergy bonuses.

**Grimtouch** (墨客, 4-cost) - Ink Splash (泼墨)
- 1-star (official EN, older): Releases a forward ink wave, deals 100 Magical Damage to enemies along the path, each piece hit increases damage by 30, accompanied with 1x Ink Touch effect. Ink Touch: Target skill attack on any Ink Touched pieces would be imposed on all other directional Ink Touched pieces
- 2-star (official EN, older): Releases a forward ink wave, deals 150 Magical Damage to enemies along the path, each piece hit increases damage by 45, accompanied with 2x Ink Touch effect. Ink Touch: Target skill attack on any Ink Touched pieces would be imposed on all other directional Ink Touched pieces
- 3-star (official EN, older): Releases a forward ink wave, deals 200 Magical Damage to enemies along the path, each piece hit increases damage by 60, accompanied with 4x Ink Touch effect. Ink Touch: Target skill attack on any Ink Touched pieces would be imposed on all other directional Ink Touched pieces
- 1-star (current ZH): 向前方挥出一道墨迹，对路径上的敌方棋子造成100魔法伤害，每命中一个棋子伤害增加30，并附带1层墨印效果。墨印：以该棋子为目标的指向性技能会同时作用于全部受到该效果影响的棋子。
- 2-star (current ZH): 向前方挥出一道墨迹，对路径上的敌方棋子造成150魔法伤害，每命中一个棋子伤害增加45，并附带2层墨印效果。墨印：以该棋子为目标的指向性技能会同时作用于全部受到该效果影响的棋子。
- 3-star (current ZH): 向前方挥出一道墨迹，对路径上的敌方棋子造成200魔法伤害，每命中一个棋子伤害增加60，并附带4层墨印效果。墨印：以该棋子为目标的指向性技能会同时作用于全部受到该效果影响的棋子。

**Pirate Captain** (海盗船长, 4-cost) - Ghostly ship (幽灵船)
- 1-star (official EN, older): Summon a ghostly ship smashes the target area, causing 1 seconds of stunning and 150 damages.
- 2-star (official EN, older): Summon a ghostly ship smashes the target area, causing 1.5 seconds of stunning and 250 damages.
- 3-star (official EN, older): Summon a ghostly ship smashes the target area, causing 2 seconds of stunning and 350 damages.
- 1-star (current ZH): 召唤幽灵船撞击目标区域对其中敌方棋子造成200伤害和1秒眩晕。
- 2-star (current ZH): 召唤幽灵船撞击目标区域对其中敌方棋子造成300伤害和1.5秒眩晕。
- 3-star (current ZH): 召唤幽灵船撞击目标区域对其中敌方棋子造成500伤害和2秒眩晕。

**Razorclaw** (利爪, 4-cost) - Summon Spirit Bear (熊灵伙伴)
- 1-star (official EN, older): Summon a spirit bear with its attack has a 20% chance to entangle an enemy unit for 3 seconds. Summoned (Spirit Bear) Magic Resistance 10
- 2-star (official EN, older): Summon a strong spirit bear with its attack has a 20% chance to entangle an enemy unit for 3 seconds. Summoned (Spirit Bear) Magic Resistance 10
- 3-star (official EN, older): Summon a stronger spirit bear with its attack has a 20% chance to entangle an enemy unit for 3 seconds. Summoned (Spirit Bear) Magic Resistance 10
- 1-star (current ZH): 主动：利爪会在登场时召唤利爪兽协助战斗。
- 2-star (current ZH): 主动：利爪会在登场时召唤利爪兽协助战斗。
- 3-star (current ZH): 主动：利爪会在登场时召唤利爪兽协助战斗。

**Redfox Swordsman (Forro Vulpes)** (赤狐剑士, 4-cost) - Foxriken (狐里剑)
- 1-star (translated): Rolls toward a random enemy unit, then thrusts 2 times in the target's direction. Each thrust counts as a normal attack and adds 15 physical damage. Ignores ability immunity.
- 2-star (translated): Rolls toward a random enemy unit, then thrusts 3 times in the target's direction. Each thrust counts as a normal attack and adds 20 physical damage. Ignores ability immunity.
- 3-star (translated): Rolls toward a random enemy unit, then thrusts 4 times in the target's direction. Each thrust counts as a normal attack and adds 30 physical damage. Ignores ability immunity.

**Siren** (海妖, 4-cost) - Abyssal Gaze (凝视)
- 1-star (official EN, older): During 3 seconds, any enemy units in 4 radius looking at Siren will have their attack speed slowed for 30%. If 2 seconds of total time is accumulated looking at Siren, that piece will turn to stone for 2 seconds.
- 2-star (official EN, older): During 4 seconds, any enemy units in 6 radius looking at Siren will have their attack speed slowed for 30%. If 2 seconds of total time is accumulated looking at Siren, that piece will turn to stone for 2.5 seconds.
- 3-star (official EN, older): During 5 seconds, any enemy units in 7 radius looking at Siren will have their attack speed slowed for 30%. If 2 seconds of total time is accumulated looking at Siren, that piece will turn to stone for 3 seconds.
- 1-star (current ZH): 使4格范围内面向自己的敌方棋子攻速降低20%，持续3秒，期间持续面向自己超过2秒的敌方棋子会被石化2秒。 无视技能免疫 石化：无法攻击，移动或释放技能，受到的物理伤害提高20%
- 2-star (current ZH): 使6格范围内面向自己的敌方棋子攻速降低20%，持续4秒，期间持续面向自己超过2秒的敌方棋子会被石化3秒。 无视技能免疫 石化：无法攻击，移动或释放技能，受到的物理伤害提高20%
- 3-star (current ZH): 使7格范围内面向自己的敌方棋子攻速降低20%，持续5秒，期间持续面向自己超过2秒的敌方棋子会被石化4秒。 无视技能免疫 石化：无法攻击，移动或释放技能，受到的物理伤害提高20%

**Soul Reaper** (灵魂收割者, 4-cost) - Heartstopper Pulse (竭心脉冲)
- 1-star (official EN, older): Deal 100 magical damages to enemy units and 100 heal to allied units in 3 radius.
- 2-star (official EN, older): Deal 150 magical damages to enemy units and 150 heal to allied units in 3 radius.
- 3-star (official EN, older): Deal 200 magical damages to enemy units and 200 heal to allied units in 3 radius.
- 1-star (current ZH): 对3格范围内的敌方棋子造成100魔法伤害,并为范围内友方棋子恢复100生命。
- 2-star (current ZH): 对3格范围内的敌方棋子造成175魔法伤害,并为范围内友方棋子恢复150生命。
- 3-star (current ZH): 对3格范围内的敌方棋子造成250魔法伤害,并为范围内友方棋子恢复200生命。

**Spider Queen** (沼泽蛛后, 4-cost) - Weave Web (织网)
- 1-star (official EN, older): Weave a web that lasts 4 seconds. The enemies on the web cannot move and 40% of their Base attack can be evaded. If the enemy dies during the effect, a small spider will be summoned.
- 2-star (official EN, older): Weave a web that lasts 6 seconds. The enemies on the web cannot move and 60% of their Base attack can be evaded. If the enemy dies during the effect, a small spider will be summoned.
- 3-star (official EN, older): Weave a web that lasts 10 seconds. The enemies on the web cannot move and 100% of their Base attack can be evaded. If the enemy dies during the effect, a small spider will be summoned.
- 1-star (current ZH): 编制一张持续5秒的蛛网，处于蛛网中的敌方棋子无法移动且其普攻有50%概率被闪避，如敌方棋子在效果期间死亡，会孵化出一只小蜘蛛。
- 2-star (current ZH): 编制一张持续7秒的蛛网，处于蛛网中的敌方棋子无法移动且其普攻有70%概率被闪避，如敌方棋子在效果期间死亡，会孵化出一只小蜘蛛。
- 3-star (current ZH): 编制一张持续10秒的蛛网，处于蛛网中的敌方棋子无法移动且其普攻有100%概率被闪避，如敌方棋子在效果期间死亡，会孵化出一只小蜘蛛。

**Storm Shaman** (风暴萨满, 4-cost) - Static Storm (离子风暴)
- 1-star (official EN, older): Creates a damaging static storm that also silence enemy units in the area for 2 seconds. Dealing as maximum as 150 magical damages per second.
- 2-star (official EN, older): Creates a damaging static storm that also silence enemy units in the area for 3 seconds. Dealing as maximum as 200 magical damages per second.
- 3-star (official EN, older): Creates a damaging static storm that also silence enemy units in the area for 4 seconds. Dealing as maximum as 250 magical damages per second.
- 1-star (current ZH): 创造风暴沉默2格范围内敌方单位并每秒造成魔法伤害，时间越长伤害越高，最高每秒150魔法伤害，持续2秒。
- 2-star (current ZH): 创造风暴沉默3格范围内敌方单位并每秒造成魔法伤害，时间越长伤害越高，最高每秒200魔法伤害，持续3秒。
- 3-star (current ZH): 创造风暴沉默4格范围内敌方单位并每秒造成魔法伤害，时间越长伤害越高，最高每秒250魔法伤害，持续4秒。

**Sunchaser Shaman** (逐日祭司, 4-cost) - Earth Drumming (大地鼓动)
- 1-star (translated): Slams the spirit drum into the ground, creating a straight line of impassable rubble that lasts 6s. Enemy pieces in the area are knocked back to the nearest empty grid, stunned for 2s, and take 300 physical damage.
- 2-star (translated): Slams the spirit drum into the ground, creating a straight line of impassable rubble that lasts 8s. Enemy pieces in the area are knocked back to the nearest empty grid, stunned for 3s, and take 400 physical damage.
- 3-star (translated): Slams the spirit drum into the ground, creating a straight line of impassable rubble that lasts 12s. Enemy pieces in the area are knocked back to the nearest empty grid, stunned for 5s, and take 600 physical damage.

**The Exile (unofficial)** (流亡者, 4-cost) - Shatter (unofficial) (破碎)
- 1-star (translated): Attacks have a chance to shatter its own existence, creating an illusion of itself. Illusions can also shatter further and create more illusions. Illusions inherit the original's passive item effects (except Eternity and Immortal).
- 2-star (translated): Attacks have a chance to shatter its own existence, creating an illusion of itself. Illusions can also shatter further and create more illusions. Illusions inherit the original's passive item effects (except Eternity and Immortal).
- 3-star (translated): 

**Thorn Predator** (荆棘掠食者, 4-cost) - Thorn Shell (荆棘外壳)
- 1-star (official EN, older): The Predator activates a Thorn Shell which can reflect and disable the damage received (each attacker can only be applied once), and stun the attacker for 1 seconds. The Thorn Shell lasts for 4 seconds.
- 2-star (official EN, older): The Predator activates a Thorn Shell which can reflect and disable the damage received (each attacker can only be applied once), and stun the attacker for 1.5 seconds. The Thorn Shell lasts for 5 seconds.
- 3-star (official EN, older): The Predator activates a Thorn Shell which can reflect and disable the damage received (each attacker can only be applied once), and stun the attacker for 2.5 seconds. The Thorn Shell lasts for 6 seconds.
- 1-star (current ZH): 掠食者开启荆棘外壳，开启状态下，可以反弹并无效化受到的伤害（每个攻击者最多作用1次），同时眩晕攻击者1秒。荆棘外壳持续4秒。
- 2-star (current ZH): 掠食者开启荆棘外壳，开启状态下，可以反弹并无效化受到的伤害（每个攻击者最多作用1次），同时眩晕攻击者1.5秒。荆棘外壳持续5秒。
- 3-star (current ZH): 掠食者开启荆棘外壳，开启状态下，可以反弹并无效化受到的伤害（每个攻击者最多作用1次），同时眩晕攻击者2.5秒。荆棘外壳持续6秒。

**Tortola Elder** (龟甲长老, 4-cost) - Kamehameha (龟派气功)
- 1-star (official EN, older): Channels for at most 3 seconds then release a pulsing wave, dealing at most 300 magical damages to enemy units in its path.
- 2-star (official EN, older): Channels for at most 3 seconds then release a pulsing wave, dealing at most 500 magical damages to enemy units in its path.
- 3-star (official EN, older): Channels for at most 3 seconds then release a pulsing wave, dealing at most 800 magical damages to enemy units in its path.
- 1-star (current ZH): 蓄力最多2秒后发射冲击波对路径上的敌方棋子最多造成300魔法伤害。
- 2-star (current ZH): 蓄力最多2秒后发射冲击波对路径上的敌方棋子最多造成450魔法伤害。
- 3-star (current ZH): 蓄力最多2秒后发射冲击波对路径上的敌方棋子最多造成750魔法伤害。

**Venomancer** (萃毒者, 4-cost) - Chemical Rage (化学狂暴)
- 1-star (official EN, older): Generates acid mist for 15 seconds that reduces the enemy Armor by 7 if affected by the mist. Meanwhile, enters a chemically induced rage that reduces basic attack cooldown to 0.7, and restores HP by 50 every second for 60 seconds.
- 2-star (official EN, older): Generates acid mist for 15 seconds that reduces the enemy Armor by 7 if affected by the mist. Meanwhile, enters a chemically induced rage that reduces basic attack cooldown to 0.55, and restores HP by 75 every second for 60 seconds.
- 3-star (official EN, older): Generates acid mist for 15 seconds that reduces the enemy Armor by 10 if affected by the mist. Meanwhile, enters a chemically induced rage that reduces basic attack cooldown to 0.4, and restores HP by 100 every second for 60 seconds.
- 1-star (current ZH): 释放持续15秒的酸雾并进入狂暴使攻击间隔降低至0.7且每秒恢复50生命值，持续15秒，酸雾中的敌人护甲减少4
- 2-star (current ZH): 释放持续15秒的酸雾并进入狂暴使攻击间隔降低至0.6且每秒恢复75生命值，持续15秒，酸雾中的敌人护甲减少7
- 3-star (current ZH): 释放持续15秒的酸雾并进入狂暴使攻击间隔降低至0.5且每秒恢复100生命值，持续15秒，酸雾中的敌人护甲减少10

**Bad Omen (Dutch Iron)** (灾祸之兆, 5-cost) - Cataclysm: Demon Birth (unofficial) (灾变·魔诞)
- 1-star (translated): Summons 1 Lava Demon(s) that fight for 8s, dealing 150 pure damage to enemies within 2 grids and stunning them for 1s. If an enemy unit dies while a Lava Demon is alive, there is a 10% chance to summon 1 more Lava Demon. Lava Demons have the Burn and Lava Fist abilities. Burn: enemy units within 2 grids take 30 magical damage per second. Lava Fist: attacks deal an extra 50 pure damage to enemy units within 1 grid of the target.
- 2-star (translated): Summons 2 Lava Demon(s) that fight for 8s, dealing 150 pure damage to enemies within 2 grids and stunning them for 1s. If an enemy unit dies while a Lava Demon is alive, there is a 10% chance to summon 1 more Lava Demon. Lava Demons have the Burn and Lava Fist abilities. Burn: enemy units within 2 grids take 30 magical damage per second. Lava Fist: attacks deal an extra 50 pure damage to enemy units within 1 grid of the target.
- 3-star (translated): Summons 3 Lava Demon(s) that fight for 8s, dealing 150 pure damage to enemies within 2 grids and stunning them for 1s. If an enemy unit dies while a Lava Demon is alive, there is a 10% chance to summon 1 more Lava Demon. Lava Demons have the Burn and Lava Fist abilities. Burn: enemy units within 2 grids take 30 magical damage per second. Lava Fist: attacks deal an extra 50 pure damage to enemy units within 1 grid of the target.

**Cannon Granny** (火器大师, 5-cost) - Art of Artillery (unofficial) (火炮艺术)
- 1-star (translated): Switches weapon to a cannon: +600% attack speed, range increased to 8 grids. Each cannon shot deals 175 physical damage and reduces the target's attack speed by 25% for 3s. Lasts up to 8s or 10 attacks.
- 2-star (translated): Switches weapon to a cannon: +600% attack speed, range increased to 8 grids. Each cannon shot deals 250 physical damage and reduces the target's attack speed by 25% for 4s. Lasts up to 8s or 15 attacks.
- 3-star (translated): Switches weapon to a cannon: +600% attack speed, range increased to 8 grids. Each cannon shot deals 350 physical damage and reduces the target's attack speed by 25% for 5s. Lasts up to 8s or 25 attacks.

**Dark Spirit** (暗之灵, 5-cost) - Black Hole (黑洞)
- 1-star (official EN, older): Deals pure damage equals to 4% of the max HP per second for enemy pieces within 3 grids. It lasts for 8 seconds.
- 2-star (official EN, older): Deals pure damage equals to 6% of the max HP per second for enemy pieces within 3 grids. It lasts for 8 seconds.
- 3-star (official EN, older): Deals pure damage equals to 8% of the max HP per second for enemy pieces within 3 grids. It lasts for 8 seconds.
- 1-star (current ZH): 对3范围区域内敌方棋子造成每秒4%最大生命值的纯粹伤害，持续8秒。
- 2-star (current ZH): 每秒对3范围区域内敌方棋子造成6%最大生命值的纯粹伤害，持续8秒。
- 3-star (current ZH): 对3范围区域内敌方棋子造成每秒8%最大生命值的纯粹伤害，持续8秒。

**Devastator** (毁灭者, 5-cost) - Delayed Action Bomb (延时炸弹)
- 1-star (official EN, older): Plant an explosive that will detonate after 3 seconds and deal 500 physical damages to enemy units in 4 radius.
- 2-star (official EN, older): Plant an explosive that will detonate after 3 seconds and deal 800 physical damages to enemy units in 4 radius.
- 3-star (official EN, older): Plant an explosive that will detonate after 3 seconds and deal 1100 physical damages to enemy units in 4 radius.
- 1-star (current ZH): 埋设3秒后引爆的炸药桶对4格范围内的敌方棋子造成500物理伤害。
- 2-star (current ZH): 埋设3秒后引爆的炸药桶对4格范围内的敌方棋子造成800物理伤害。
- 3-star (current ZH): 埋设3秒后引爆的炸药桶对4格范围内的敌方棋子造成1100物理伤害。

**Dualplot (Taorive)** (晦暗之谋, 5-cost) - Candle Lighting the Nine Yin (unofficial) (烛照九阴)
- 1-star (translated): Passive: Each time an allied Pandaman unit casts an ability, Dualplot gains 1 stack of Qi. At certain Qi stacks, the ability gains follow-up combos. Active: Deals 100 magical damage to nearby enemy units and creates a shield that absorbs 200 physical damage for 6s. Each enemy unit damaged adds 50 shield. At 3 Qi, follow-up combo: dashes to the farthest grid from which it can attack an enemy piece, dealing 200 magical damage to enemies along the way. At 6 Qi, follow-up combo: summons 5 prayer beads on the board; enemy units within bead range have attack speed reduced by 50%. Dualplot vanishes from the board and reappears 2s later in the bead area with the most enemies, dealing 400 magical damage, split evenly if there are multiple enemies in range.
- 2-star (translated): Passive: Each time an allied Pandaman unit casts an ability, Dualplot gains 1 stack of Qi. At certain Qi stacks, the ability gains follow-up combos. Active: Deals 150 magical damage to nearby enemy units and creates a shield that absorbs 300 physical damage for 6s. Each enemy unit damaged adds 75 shield. At 3 Qi, follow-up combo: dashes to the farthest grid from which it can attack an enemy piece, dealing 300 magical damage to enemies along the way. At 6 Qi, follow-up combo: summons 5 prayer beads on the board; enemy units within bead range have attack speed reduced by 50%. Dualplot vanishes from the board and reappears 2s later in the bead area with the most enemies, dealing 600 magical damage, split evenly if there are multiple enemies in range.
- 3-star (translated): Passive: Each time an allied Pandaman unit casts an ability, Dualplot gains 1 stack of Qi. At certain Qi stacks, the ability gains follow-up combos. Active: Deals 200 magical damage to nearby enemy units and creates a shield that absorbs 400 physical damage for 6s. Each enemy unit damaged adds 100 shield. At 3 Qi, follow-up combo: dashes to the farthest grid from which it can attack an enemy piece, dealing 400 magical damage to enemies along the way. At 6 Qi, follow-up combo: summons 5 prayer beads on the board; enemy units within bead range have attack speed reduced by 50%. Dualplot vanishes from the board and reappears 2s later in the bead area with the most enemies, dealing 800 magical damage, split evenly if there are multiple enemies in range.

**Frostblaze Dragon** (霜炽龙, 5-cost) - Icebound Wall (霜火冲击)
- 1-star (official EN, older): Builds an Icebound wall on your right, dealing 200 magical damage to enemy passing through the wall and reducing their attack speed by 30% for 3 seconds. All pieces won't target enemies on the other side of the wall.
- 2-star (official EN, older): Builds an Icebound wall on your right, dealing 300 magical damage to enemy passing through the wall and reducing their attack speed by 30% for 3 seconds. All pieces won't target enemies on the other side of the wall.
- 3-star (official EN, older): Builds an Icebound wall on your right, dealing 400 magical damage to enemy passing through the wall and reducing their attack speed by 30% for 3 seconds. All pieces won't target enemies on the other side of the wall.
- 1-star (current ZH): 霜炽龙同时释放寒冰弹与烈焰弹。 寒冰弹：对随机一个敌方单位及其周围一格造成100点魔法伤害并附加冰标记，受到寒冰弹伤害的敌方单位攻速降低50%，持续5s。 烈焰弹：对随机一个敌方单位及其周围一格造成100点魔法伤害并附加火标记，受到烈焰弹伤害的敌方单位每秒受到50点魔法伤害，持续5s。 冰标记和火标记的持续时间无限，当敌方单位同时存在冰标记和火标记时，标记将融合并引爆，造成500点魔法伤害，引爆后标记消失。 无视技能免疫。
- 2-star (current ZH): 霜炽龙同时释放寒冰弹与烈焰弹。 寒冰弹：对随机一个敌方单位及其周围一格造成150点魔法伤害并附加冰标记，受到寒冰弹伤害的敌方单位攻速降低60%，持续5s。 烈焰弹：对随机一个敌方单位及其周围一格造成150点魔法伤害并附加火标记，受到烈焰弹伤害的敌方单位每秒受到75点魔法伤害，持续5s。 冰标记和火标记的持续时间无限，当敌方单位同时存在冰标记和火标记时，标记将融合并引爆，造成800点魔法伤害，引爆后标记消失。 无视技能免疫。
- 3-star (current ZH): 霜炽龙同时释放寒冰弹与烈焰弹。 寒冰弹：对随机一个敌方单位及其周围一格造成200点魔法伤害并附加冰标记，受到寒冰弹伤害的敌方单位攻速降低70%，持续5s。 烈焰弹：对随机一个敌方单位及其周围一格造成200点魔法伤害并附加火标记，受到烈焰弹伤害的敌方单位每秒受到100点魔法伤害，持续5s。 冰标记和火标记的持续时间无限，当敌方单位同时存在冰标记和火标记时，标记将融合并引爆，造成1100点魔法伤害，引爆后标记消失。 无视技能免疫。

**God of Thunder** (雷神, 5-cost) - Zeus' Punishment (落雷)
- 1-star (official EN, older): Summons thunder to punish at least 1 enemy piece, and has a 50% chance of dealing 15% of the target piece's remaining HP +250 Magical damage equals to it.
- 2-star (official EN, older): Summons thunder to punish at least 1 enemy piece, and has a 50% chance of dealing 25% of the target piece's remaining HP +350 Magical damage equals to it.
- 3-star (official EN, older): Summons thunder to punish at least 1 enemy piece, and has a 50% chance of dealing 35% of the target piece's remaining HP +450 Magical damage equals to it.
- 1-star (current ZH): 召唤雷电攻击敌方棋子，所有敌人均有50%概率受到雷击，造成目标剩余生命值15%+300魔法伤害，至少会雷击1个敌方棋子。
- 2-star (current ZH): 召唤雷电攻击敌方棋子，所有敌人均有50%概率受到雷击，造成目标剩余生命值25%+400魔法伤害，至少会雷击1个敌方棋子。
- 3-star (current ZH): 召唤雷电攻击敌方棋子，所有敌人均有50%概率受到雷击，造成目标剩余生命值35%+600魔法伤害，至少会雷击1个敌方棋子。

**Helicopter** (矮人直升机, 5-cost) - Call Down (空中打击)
- 1-star (official EN, older): Call down 2 aerial missiles strike enemy units in 4 radius. Each of them deals 200 magical damages
- 2-star (official EN, older): Call down 2 aerial missiles strike enemy units in 4 radius. Each of them deals 300 magical damages
- 3-star (official EN, older): Call down 2 aerial missiles strike enemy units in 4 radius. Each of them deals 400 magical damages
- 1-star (current ZH): 召唤2枚导弹轰炸4格范围区域，每枚导弹造成250魔法伤害。
- 2-star (current ZH): 召唤2枚导弹轰炸4格范围区域，每枚导弹造成400魔法伤害。
- 3-star (current ZH): 召唤2枚导弹轰炸4格范围区域，每枚导弹造成600魔法伤害。

**Khan** (烈日可汗, 5-cost) - Earth Cracking (裂土为王)
- 1-star (official EN, older): Slash a crack ahead with the axe, pulling enemy units within range toward the crack after 3 seconds and stunning them for 2 seconds while dealing damage equals to their maximum HP 10%+100 (50% is magical damage, 50% is physical damage).
- 2-star (official EN, older): Slash a crack ahead with the axe, pulling enemy units within range toward the crack after 3 seconds and stunning them for 2.5 seconds while dealing damage equals to their maximum HP 15%+150 (50% is magical damage, 50% is physical damage).
- 3-star (official EN, older): Slash a crack ahead with the axe, pulling enemy units within range toward the crack after 3 seconds and stunning them for 3 seconds while dealing damage equals to their maximum HP 20%+200 (50% is magical damage, 50% is physical damage).
- 1-star (current ZH): 用斧头向前劈出一道裂痕，3秒后将范围内的敌方单位拉向裂痕，并使其昏迷2秒，同时造成基于它们自身最大生命值10%+100的伤害（50%为魔法伤害，50%为物理伤害）。
- 2-star (current ZH): 用斧头向前劈出一道裂痕，3秒后将范围内的敌方单位拉向裂痕，并使其昏迷2.5秒，同时造成基于它们自身最大生命值15%+150的伤害（50%为魔法伤害，50%为物理伤害）。
- 3-star (current ZH): 用斧头向前劈出一道裂痕，3秒后将范围内的敌方单位拉向裂痕，并使其昏迷3秒，同时造成基于它们自身最大生命值20%+200的伤害（50%为魔法伤害，50%为物理伤害）。

**Light Spirit** (光之灵, 5-cost) - Breaking Light (unofficial) (破击之光)
- 1-star (translated): Passive: After every 3 normal attacks, the next normal attack petrifies the target for 1s. Passive: When an enemy is hit by a control effect, Light Spirit sends its Armor of Light to attack the target two more times, each dealing 100 pure damage (CD 5s). Control effects include silence, stun, disarm, petrify, taunt, and knock-up.
- 2-star (translated): Passive: After every 2 normal attacks, the next normal attack petrifies the target for 1s. Passive: When an enemy is hit by a control effect, Light Spirit sends its Armor of Light to attack the target two more times, each dealing 200 pure damage (CD 5s). Control effects include silence, stun, disarm, petrify, taunt, and knock-up.
- 3-star (translated): Passive: After every 2 normal attacks, the next normal attack petrifies the target for 1s. Passive: When an enemy is hit by a control effect, Light Spirit sends its Armor of Light to attack the target two more times, each dealing 300 pure damage (CD 5s). Control effects include silence, stun, disarm, petrify, taunt, and knock-up.

**Mountain Chief** (山魁, 5-cost) - Collapsing Impact (山崩地裂)
- 1-star (translated): Shou unleashes its fury, grabs the enemy piece with the highest HP within attack range, and repeatedly slams it into the ground, dealing undispellable damage and stun. Deals physical damage and a brief stun to all enemies in the area.
- 2-star (translated): Shou unleashes its fury, grabs the enemy piece with the highest HP within attack range, and repeatedly slams it into the ground, dealing undispellable damage and stun. Deals physical damage and a brief stun to all enemies in the area.
- 3-star (translated): Shou unleashes its fury, grabs the enemy piece with the highest HP within attack range, and repeatedly slams it into the ground, dealing undispellable damage and stun. Deals physical damage and a brief stun to all enemies in the area.

**Old God of the Dusk Forest (unofficial)** (暮林旧神, 5-cost) - Soul Flame (unofficial) (灵魂之焰)
- 1-star (translated): Creates destructive soul fire around itself, continuously dealing magical damage to nearby enemy pieces and consuming its own mana until mana runs out.
- 2-star (translated): Creates destructive soul fire around itself, continuously dealing magical damage to nearby enemy pieces and consuming its own mana until mana runs out.
- 3-star (translated): Creates destructive soul fire around itself, continuously dealing magical damage to nearby enemy pieces and consuming its own mana until mana runs out.

**Phantom Queen** (魅影女王, 5-cost) - Scream of Devil (魔音)
- 1-star (official EN, older): Deals 200 magical damages to nearby enemy units within 3 grids
- 2-star (official EN, older): Deals 300 magical damages to nearby enemy units within 3 grids
- 3-star (official EN, older): Deals 400 magical damages to nearby enemy units within 3 grids
- 1-star (current ZH): 向前发出声波，对命中的所有敌人造成300纯粹伤害与1.5秒的混乱效果。 混乱：无法普攻，无法释放技能；棋子将不受控制，漫无目的地行走。
- 2-star (current ZH): 向前发出声波，对命中的所有敌人造成400纯粹伤害与2秒的混乱效果。 混乱：无法普攻，无法释放技能；棋子将不受控制，漫无目的地行走。
- 3-star (current ZH): 向前发出声波，对命中的所有敌人造成600纯粹伤害与3秒的混乱效果。 混乱：无法普攻，无法释放技能；棋子将不受控制，漫无目的地行走。

**Radiant Empress (unofficial)** (晖光女帝, 5-cost) - Perfect Avatar (unofficial) (完美化身)
- 1-star (translated): At the start of battle, Radiant Empress gathers yellow sand around a gem core to create a perfect avatar of herself. On the board, Radiant Empress counts as two pieces with the same name. The avatar inherits all of the original's item effects.
- 2-star (translated): At the start of battle, Radiant Empress gathers yellow sand around a gem core to create a perfect avatar of herself. On the board, Radiant Empress counts as two pieces with the same name. The avatar inherits all of the original's item effects.
- 3-star (translated): At the start of battle, Radiant Empress gathers yellow sand around a gem core to create a perfect avatar of herself. On the board, Radiant Empress counts as two pieces with the same name. The avatar inherits all of the original's item effects.

**Rogue Guard** (沉沦守卫, 5-cost) - Crazy Dance (群魔乱舞)
- 1-star (official EN, older): Passive: Deals 50% splash physical damage to enemies within 2.5 grids. Active: All ally Demon pieces' attack increases by 150 for 60 seconds. Also increases its own ATK which equals to number of allied pieces with Demon synergy*35.
- 2-star (official EN, older): Passive: Deals 50% splash physical damage to enemies within 2.5 grids. Active: All ally Demon pieces' attack increases by 200 for 60 seconds. Also increases its own ATK which equals to number of allied pieces with Demon synergy*45.
- 3-star (official EN, older): Passive: Deals 50% splash physical damage to enemies within 2.5 grids. Active: All ally Demon pieces' attack increases by 250 for 60 seconds. Also increases its own ATK which equals to number of allied pieces with Demon synergy*60.
- 1-star (current ZH): 被动：2.5范围内造成50%攻击力的溅射伤害。 主动：令所有友方恶魔获得60秒的150攻击力，自身额外增加友方拥有恶魔羁绊的棋子数量*20的攻击力。
- 2-star (current ZH): 被动：2.5范围内造成50%攻击力的溅射伤害。 主动：令所有友方恶魔获得60秒的200攻击力，自身额外增加友方拥有恶魔羁绊的棋子数量*30的攻击力。
- 3-star (current ZH): 被动：2.5范围内造成50%攻击力的溅射伤害。 主动：令所有友方恶魔获得60秒的250攻击力，自身额外增加友方拥有恶魔羁绊的棋子数量*40的攻击力。

**Sacred Lancer** (冰川武士, 5-cost) - Cursed Lance (诅咒魔枪)
- 1-star (official EN, older): Feeds her Lance with blood, sacrificing 15 HP at each base attack, and multiple attacks will stack additional 20 Magic damage per second to the target, lasts 8 seconds; for each loss of 10% HP, increases 10% ATK speed, 10% Magic resistance and 0.5% HP regeneration per second.
- 2-star (official EN, older): Feeds her Lance with blood, sacrificing 15 HP at each base attack, and multiple attacks will stack additional 30 Magic damage per second to the target, lasts 8 seconds; for each loss of 10% HP, increases 15% ATK speed, 10% Magic resistance and 0.5% HP regeneration per second.
- 3-star (official EN, older): Feeds her Lance with blood, sacrificing 15 HP at each base attack, and multiple attacks will stack additional 50 Magic damage per second to the target, lasts 8 seconds; for each loss of 10% HP, increases 25% ATK speed, 10% Magic resistance and 0.5% HP regeneration per second.
- 1-star (current ZH): 用血液喂养魔枪，每次普攻牺牲自身15点生命值，对敌方额外造成每秒15点的魔法伤害，持续8秒；每损失10%生命值，增加自己10%攻击速度，10%魔法抗性和每秒0.5%的生命恢复。
- 2-star (current ZH): 用血液喂养魔枪，每次普攻牺牲自身15点生命值，对敌方额外造成每秒25点的魔法伤害，持续8秒；每损失10%生命值，增加自己15%攻击速度，10%魔法抗性和每秒0.5%的生命恢复。
- 3-star (current ZH): 用血液喂养魔枪，每次普攻牺牲自身15点生命值，对敌方额外造成每秒50点的魔法伤害，持续8秒；每损失10%生命值，增加自己25%攻击速度，10%魔法抗性和每秒0.5%的生命恢复。

**Shining Assassin** (光羽斥候, 5-cost) - T.A. Field (风随吾身)
- 1-star (official EN, older): Gain 5 times of avoidance effects and 40 bonus to damage, last for 6 seconds.
- 2-star (official EN, older): Gain 7 times of avoidance effects and 60 bonus to damage, last for 6 seconds.
- 3-star (official EN, older): Gain 9 times of avoidance effects and 80 bonus to damage, last for 6 seconds.
- 1-star (current ZH): 主动：在身边召唤持续6秒的风之结界，增加40攻击力并可免疫5次伤害； 被动：普通攻击化为宽度为2格且穿透后方6格区域的风刃，对区域内敌方单位造成50%纯粹伤害。
- 2-star (current ZH): 主动：在身边召唤持续6秒的风之结界，增加60攻击力并可免疫7次伤害； 被动：普通攻击化为宽度为2格且穿透后方6格区域的风刃，对区域内敌方单位造成75%纯粹伤害。
- 3-star (current ZH): 主动：在身边召唤持续6秒的风之结界，增加80攻击力并可免疫9次伤害； 被动：普通攻击化为宽度为2格且穿透后方6格区域的风刃，对区域内敌方单位造成100%纯粹伤害。

**Soul Blade** (亡魂之刃, 5-cost) - Forbidden Soul Art (unofficial) (魂灵禁术)
- 1-star (translated): Passive: At the start of battle, Soul Blade has 2 stacks of Soul Mark. Each active ability cast grants 3 stacks; each normal attack consumes 1 stack. Soul Mark: deals extra physical damage equal to 100% of the piece's attack * current Soul Mark stacks. Active: Soul Blade leaves a Soul Body on its current grid, then teleports to the farthest enemy unit it can attack. Soul Body: cannot attack or move, inherits 100% of the piece's base attributes, takes 100% damage, and taunts enemy units within 1 grid for 2.5s. If the Soul Body is still alive when the duration ends, it spends 50% of its remaining HP to heal the allied Soul Clan piece with the lowest current HP.
- 2-star (translated): Passive: At the start of battle, Soul Blade has 4 stacks of Soul Mark. Each active ability cast grants 3 stacks; each normal attack consumes 1 stack. Soul Mark: deals extra physical damage equal to 100% of the piece's attack * current Soul Mark stacks. Active: Soul Blade leaves a Soul Body on its current grid, then teleports to the farthest enemy unit it can attack. Soul Body: cannot attack or move, inherits 100% of the piece's base attributes, takes 100% damage, and taunts enemy units within 1 grid for 2.5s. If the Soul Body is still alive when the duration ends, it spends 50% of its remaining HP to heal the allied Soul Clan piece with the lowest current HP.
- 3-star (translated): Passive: At the start of battle, Soul Blade has 6 stacks of Soul Mark. Each active ability cast grants 3 stacks; each normal attack consumes 1 stack. Soul Mark: deals extra physical damage equal to 100% of the piece's attack * current Soul Mark stacks. Active: Soul Blade leaves a Soul Body on its current grid, then teleports to the farthest enemy unit it can attack. Soul Body: cannot attack or move, inherits 100% of the piece's base attributes, takes 100% damage, and taunts enemy units within 1 grid for 2.5s. If the Soul Body is still alive when the duration ends, it spends 50% of its remaining HP to heal the allied Soul Clan piece with the lowest current HP.

**Space Walker** (乾坤行者, 5-cost) - Heaven-shocking (棒打乾坤)
- 1-star (translated): Gains 1 stack of Qi each time a normal attack damages an enemy unit. At 4 Qi, Heaven-shocking can be cast. Active (Heaven-shocking): the next normal attack deals 175% damage to enemy units in a straight line and stuns them for 0.5s.
- 2-star (translated): Gains 1 stack of Qi each time a normal attack damages an enemy unit. At 4 Qi, Heaven-shocking can be cast. Active (Heaven-shocking): the next normal attack deals 225% damage to enemy units in a straight line and stuns them for 1s.
- 3-star (translated): Gains 1 stack of Qi each time a normal attack damages an enemy unit. At 4 Qi, Heaven-shocking can be cast. Active (Heaven-shocking): the next normal attack deals 350% damage to enemy units in a straight line and stuns them for 2s.

**Strange Egg** (奇异蛋, 5-cost) - Ever Changing (百变)
- 1-star (official EN, older): If you need 1 piece to rank up your piece, drag the Strange Egg to the target piece, and it'll be whatever you want it to be.
- 2-star (official EN, older): If you need 1 piece to rank up your piece, drag the Strange Egg to the target piece, and it'll be whatever you want it to be.
- 3-star (official EN, older): 
- 1-star (current ZH): 将奇异蛋拖至任意1星棋子上时，可作为该棋子升星的材料，每次升星最多使用1颗奇异蛋
- 2-star (current ZH): 将奇异蛋拖至任意2星棋子上时，可作为该棋子升星的材料，每次升星最多使用1颗奇异蛋
- 3-star (current ZH): 

**The Scryer** (占星者, 5-cost) - Raven Arcane (血羽秘术)
- 1-star (official EN, older): Active: Casts an ability based on the highest activated tier Class/Race Synergy of the enemy's lineup (with at least 3 chess pieces). Passive: Regenerates 10 Mana per second. When the first enemy's Mana is full, Silence it for 3 seconds. Mage/Shaman/Divinity: Electromagnetic Pulse, Warrior/Cave Clan/Goblin: Darkfeathered Servants, Mech/Warlock/Marine: Blood Surge, Hunter/Assassin/Glacier: Sonic Attack, Knight/Human/Egersis: Bolt of Lightning, Beast/Feathered/Demon: Earth's Fury, Druid/Spirit/Dragon: Whirlwind Attack
- 2-star (official EN, older): Active: Casts an ability based on the highest activated tier Class/Race Synergy of the enemy's lineup (with at least 3 chess pieces). Passive: Regenerates 10 Mana per second. When the first enemy's Mana is full, Silence it for 3 seconds. Mage/Shaman/Divinity: Electromagnetic Pulse, Warrior/Cave Clan/Goblin: Darkfeathered Servants, Mech/Warlock/Marine: Blood Surge, Hunter/Assassin/Glacier: Sonic Attack, Knight/Human/Egersis: Bolt of Lightning, Beast/Feathered/Demon: Earth's Fury, Druid/Spirit/Dragon: Whirlwind Attack
- 3-star (official EN, older): Active: Casts an ability based on the highest activated tier Class/Race Synergy of the enemy's lineup (with at least 3 chess pieces). Passive: Regenerates 10 Mana per second. When the first enemy's Mana is full, Silence it for 3 seconds. Mage/Shaman/Divinity: Electromagnetic Pulse, Warrior/Cave Clan/Goblin: Darkfeathered Servants, Mech/Warlock/Marine: Blood Surge, Hunter/Assassin/Glacier: Sonic Attack, Knight/Human/Egersis: Bolt of Lightning, Beast/Feathered/Demon: Earth's Fury, Druid/Spirit/Dragon: Whirlwind Attack
- 1-star (current ZH): 被动：+10每秒魔法恢复，敌方第一个棋子魔法值达到上限时，沉默该棋子，持续3秒。 主动：根据对方阵容最多棋子组成的职业/种族(至少3个棋子)不同，使用不同的技能。 法师/萨满/神/牧师/灵猫族：电磁脉冲；战士/洞洞/哥布林/猎魔人：暗羽仆从； 工匠/术士/犀族/熊猫人/基拉：气血涌动； 猎人/潜行者/冰川/恶魔/矮人：灵雀高音； 骑士/海族/光羽/武师：天外惊雷；野兽/虫/人类/巫师：大地狂怒； 德鲁伊/灵族/龙/不眠：疾风龙卷
- 2-star (current ZH): 被动：+10每秒魔法恢复，敌方第一个棋子魔法值达到上限时，沉默该棋子，持续3秒。 主动：根据对方阵容最多棋子组成的职业/种族(至少3个棋子)不同，使用不同的技能。 法师/萨满/神/牧师/灵猫族：电磁脉冲； 战士/洞洞/哥布林/猎魔人：暗羽仆从； 工匠/术士/犀族/熊猫人/基拉：气血涌动； 猎人/潜行者/冰川/恶魔/矮人：灵雀高音； 骑士/海族/光羽/武师：天外惊雷； 野兽/虫/人类/巫师：大地狂怒； 德鲁伊/灵族/龙/不眠：疾风龙卷
- 3-star (current ZH): 被动：+10每秒魔法恢复，敌方第一个棋子魔法值达到上限时，沉默该棋子，持续3秒。主动：根据对方阵容最多棋子组成的职业/种族(至少3个棋子)不同，使用不同的技能。 法师/萨满/神/牧师/灵猫族：电磁脉冲； 战士/洞洞/哥布林/猎魔人：暗羽仆从； 工匠/术士/犀族/熊猫人/基拉：气血涌动； 猎人/潜行者/冰川/恶魔/矮人：灵雀高音； 骑士/海族/光羽/武师：天外惊雷； 野兽/虫/人类/巫师：大地狂怒； 德鲁伊/灵族/龙/不眠：疾风龙卷

**Tsunami Stalker** (海啸追猎者, 5-cost) - Ravaging Tentacles (毁灭之触)
- 1-star (official EN, older): Slam the ground, causing the tentacles to erupt in all directions and in at most 3 radius, dealing 100 magical damages and stunning enemy units for 2 seconds.
- 2-star (official EN, older): Slam the ground, causing the tentacles to erupt in all directions and in at most 5 radius, dealing 200 magical damages and stunning enemy units for 2 seconds.
- 3-star (official EN, older): Slam the ground, causing the tentacles to erupt in all directions and in at most 6 radius, dealing 300 magical damages and stunning enemy units for 2 seconds.
- 1-star (current ZH): 召唤触手对3格范围内的敌方棋子造成100魔法伤害并眩晕3秒。
- 2-star (current ZH): 召唤触手对5格范围内的敌方棋子造成200魔法伤害并眩晕4秒。
- 3-star (current ZH): 召唤触手对6格范围内的敌方棋子造成300魔法伤害并眩晕5秒。

## Synergies (races and classes)

Source: official character pages (ZH and EN). Breakpoints are the bracketed or leading numbers in the effect text. The Chinese text is the newer record; it is given in translation, followed by the older official English text when one exists.

### Races

**Night Demon** (夜魔)
- Current effect (translated, ZH record 2025-04-01): Night Demon[2]: Allied Night Demon pieces gain: if there is more than 1 enemy nearby, they lose control from fear. Fear has a 6s internal cooldown and cannot trigger consecutively.
- Official English (record 2025-04-01): Night Demon[2]: Ally Night Demon pieces gain the effect: If there are more than 1 enemy nearby, they will lose control due to fear. The effect of fear has a built-in CD of 6 seconds and cannot trigger consecutively.
- Pieces: Unknown Horror, Mist Phantom King (Wuril)

**Greater (The Greater)** (太古族)
- Current effect (translated, ZH record 2021-12-30): 1: Only while The Greater synergy is active, all allied pieces are immune to all control effects, and normal attacks have a 25% chance to Spatially Imprison the enemy unit for 2s. Spatial Imprisonment: cannot move, turn, attack, or cast abilities.
- Official English (record 2021-12-30): When only the Greater Synergy is in effect, all allied pieces will be immune to all control effects. Normal attacks have a 25% chance to cast 2s Spatial Imprisonment and cause 50 physical damage to enemy units.   Spatial Imprisonment: Cannot move, turn, attack, or cast abilities.
- Pieces: Dominator (Dominiel)

**Human** (人族)
- Current effect (translated, ZH record 2020-06-18): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: After winning a battle against a player, if at least one allied Human piece survives on the board, drops EXP Book x1. 6: After winning a battle against a player, if at least one allied Human piece survives on the board, drops EXP Book x4.
- Official English (record 2020-06-18): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 3: After winning a battle against another player, if at least one ally Human survives, gets EXP Book x1 6: After winning a battle against another player, if at least one ally Human survives, gets EXP Book x4
- Pieces: The Source, Flame Wizard, Werewolf, Avenge Knight, Argali Knight, Tortola Elder, Pirate Captain, Dragon Knight

**Feathered** (光羽族)
- Current effect (translated, ZH record 2021-12-16): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: All allied Feathered have a 30% chance to evade enemy attacks. 6: All allied Feathered have a 55% chance to evade enemy attacks; other allied pieces have a 35% chance. 9: Allied Feathered pieces 60% evasion, all allied pieces 44% evasion. A successful evade creates an illusion of the piece for 7s; trigger interval 4s. Illusion: a copy of the original unit that inherits 100% attack and takes 100% damage, but does not inherit abilities, synergies, or items. Each unit can have only one illusion at a time.
- Official English (record 2021-12-16): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 3：All allied Feathered have 20% chance of evasion 6：All allied Feathered gain a 40% chance to evade attacks, and other Allies gain a 20% chance to evade attacks 9：All allied Feathered gain a 60% chance to evade attacks, and other Allies gain a 44% chance to evade attacks.Once successfully evade, the ally creates an illusion for 7s.
- Pieces: Shining Archer, Taboo Witcher, Shining Dragon, Wisper Seer, Light Blade, Qin Xuan / Plume Warrior, Shadowcrawler, Wind Ranger, Warpwood Sage, The Scryer, Strange Egg, Shining Assassin, Flower Spirit (unofficial)

**Demon** (恶魔)
- Current effect (translated, ZH record 2020-02-14): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 1: When there is only 1 kind of Demon on the board, its attacks deal 50% extra pure damage to enemies.
- Official English (record 2020-02-14): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 1: Attack deals 50% extra pure damage to the target. Active when only one kind of Hunter demon on the chessboard
- Pieces: Soul Devourer, Hell Knight, Penitent Bishop (Mammon), Shadow Devil, Fallen Witcher, Grimtouch, Doom Arbiter, Rogue Guard, Phantom Queen

**Goblin** (哥布林)
- Current effect (translated, ZH record 2020-11-26): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: One random ally gets armor +15, HP regeneration +20. 6: All allies get armor +15, HP regeneration +20.
- Official English (record 2020-08-12): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 3: Random allied unit armor +15, HP Regeneration +15 6: All allied armor +15, HP Regeneration +15
- Pieces: Soul Breaker, Heaven Bomber, Sky Breaker, Ripper, Venomancer, Cannon Granny, Devastator

**Beast** (野兽)
- Current effect (translated, ZH record 2022-01-13): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: All allies attack +15%; can be inherited by summons. 4: All allies attack +40%; can be inherited by summons. 6: All allies attack +40%. Enemy pieces attacked by a unit with the Beast effect take 16 extra physical damage; this damage stacks and can be inherited by summons.
- Official English (record 2021-12-16): Deploying pieces of the same class activates synergies. Only the highest synergy level for each Class is in effect at any given time 2：All allies have +15% attack damage, including summoned units 4：All allies have +30% attack damage, including summoned units 6：Increases 30% ATK for all allies, including the summoned. The enemy will take 16 extra Physical Damage when attacked by an ally who benefits from the Beast Synergy, this effect can be stacked
- Pieces: Unicorn, Tusk Champion, Werewolf, Bobo, Poisonous Worm, Redfox Swordsman (Forro Vulpes), Razorclaw, Space Walker, Mountain Chief

**Cave** (洞洞族)
- Current effect (translated, ZH record 2021-03-24): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: All allied pieces max HP +150. 4: All allied pieces max HP +400. 6: All allied pieces HP +600, plus extra HP equal to the chess player's missing HP percentage * 900.
- Official English (record 2020-07-30): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: All allies +100 max HP 4: All allies +350 max HP 6：All allies +350 HP. Ally HP is boosted equal to 700x the percentage of the chess player’s missing HP
- Pieces: Redaxe Chief, Lava Shaman, Swordman, Skull Hunter, Cave Prodigy, Storm Shaman, Bad Omen (Dutch Iron)

**Marine** (海族)
- Current effect (translated, ZH record 2025-07-28): [2]: All allied pieces gain 20% ability damage absorption. [4]: All allied pieces gain 60% ability damage absorption. [6]: All allied pieces gain 60% ability damage absorption. Marine pieces can trigger an explosion every 3s with a 3-grid radius, dealing magical damage to enemy pieces in range equal to the ability damage absorbed during that period. If less than 100 ability damage was absorbed in the period, there is no explosion and the absorbed amount resets.
- Official English (record 2020-07-30): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: All allies magical resistence +30% 4: All allies magical resistence +60%
- Pieces: Voice of the Deep (unofficial), Abyssalcrawler, Abyssal Guard, Siren, Tsunami Stalker

**Egersis** (不眠)
- Current effect (translated, ZH record 2025-11-27): Egersis[3]: All non-Egersis pieces on both sides get Plague: starting HP -10%. Egersis[6]: All non-Egersis pieces on both sides get Plague: starting HP -50%. Egersis[9]: All non-Egersis pieces on both sides get Plague: starting HP -50%, and infected pieces lose 5% max HP every 3s. (The reduction is based on starting HP.)
- Official English (record 2021-12-30): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: All enemy -4 armor 4: All enemy -8 armor 6: All enemy -12 armor,If a unit is killed by these enemies, it will continue to fight for 3.5s seconds. (it won't receive normal attacks, nor can it be chosen by allies' abilities or cast abilities during the period)
- Pieces: Resentful Murk, Winter Chiropteran, Egersis Ranger, Eclipse of Darkness, Umbra, Evil Knight, Thorn Predator, Soul Reaper, Underworld Enforcer (unofficial)

**Glacier** (冰川族)
- Current effect (translated, ZH record 2022-02-10): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: All allied pieces attack speed +20%. 5: All allied pieces attack speed +40%; each attack adds 8% attack speed, stacking up to 10 times.
- Official English (record 2021-12-30): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: All allied Glacier attack speed +20% 4: Increase all allied pieces ATK Speed by 50%; Each attack grants them an extra stack of 8% ATK Speed bonus, up to 15 stacks
- Pieces: Frost Knight, Defector, Desperate Doctor, Fortune Teller, Berserker, Sacred Lancer, The Exile (unofficial)

**Pandaman** (熊猫人)
- Current effect (translated, ZH record 2025-08-27): (1) At the start of the round, 12% chance to invite a random Pandaman piece. (2) At the start of the round, each allied Pandaman on the board has a 12% chance to invite a Pandaman piece with the same name.
- Official English (record 2021-12-17): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks  1：At the beginning of the round, there is a 10% chance to invite a Pandaman. 2：At the beginning of the round, there is a 20% chance to invite a Pandaman. 3：At the beginning of the round, there is a 20% chance to invite a Pandaman.There is a 40% chance to invite an extra Pandaman.
- Pieces: Heaven Brew (Qin Lang), Jade General (Ji Xi), Thunder Trace (Lei Rong), Ember Blade (Di Wei), Dualplot (Taorive)

**Soul Clan** (魂族)
- Current effect (translated, ZH record 2022-12-09): Turns 2 grids on the board into Soul Springs. Soul Spring: a piece placed on a Soul Spring before battle gains max HP equal to 18% of the total base max HP of Soul Clan pieces on the board, and attack equal to 18% of the total base attack of Soul Clan pieces on the board. (When a Soul Clan piece dies, its share of the bonus is lost.)
- Pieces: Ghost Kid, Nightmare (formerly Egersis Prophet), Soul Blade

**Civet** (灵猫族)
- Current effect (translated, ZH record 2021-07-22): When a Civet piece upgrades to 3 stars, you get an extra 3-star copy of that piece. At the start of each round's battle, for each pair of identical pieces on your board, summons a Golem copy with 100% damage ability.
- Official English (record 2021-07-22): Creates a duplicate when a Civet piece ranks up to 3 stars. When battle starts each round, if two identical pieces exist on the board, summons a Golem copy with 100% damage ability for each couple of identical pieces.
- Pieces: Gem Artisan, Radiant Empress (unofficial)

**Horn** (犀族)
- Current effect (translated, ZH record 2021-11-18): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. Horn (2): At the start of battle, summons a Horn Totem (HP = total enemy pieces*2). Horn Totem: attacked first when within enemy attack range. Horn (4): At the start of battle, summons one Horn Totem (HP = total enemy pieces*2), one Sunchaser Totem (HP = total enemy pieces*1), and one Skydome Totem (HP = total enemy pieces*1). Horn Totem: attacked first when within enemy attack range. Sunchaser Totem: after each normal attack, every enemy unit has a 10% chance to take 100 physical damage and be disarmed for 1s. Skydome Totem: after each ability cast, every enemy unit has a 50% chance to take 200 magical damage and be silenced for 5s. Totems cannot move, attack, or regenerate HP, and take only 1 damage from a melee hit or 2 from a ranged hit.
- Official English (record 2021-10-19): Horn(2): Summons a Horn Totem (HP equals to total enemy pieces*3) when battle starts;  Horn(4): Summons a Horn Totem (HP equals to total enemy pieces*3), a Sunchaser Totem (HP equals to total enemy pieces*1), and a Skydome Totem (HP equals to total enemy pieces*1) when battle starts.  Horn Totem: will be targeted first when within enemy attack range.  Sunchaser Totem: Causes all enemy units to have a 10% chance of taking 250 physical damage and being disarmed for 2 seconds after each normal attack. Skydome Totem: Causes all enemy units to have a 50% chance to take 500 magical damage and be silenced for 5 seconds after each ability cast. Totem cannot move, attack or restore HP. Receives only 1 melee attack damage or 2 ranged attack damage each time.
- Pieces: Sunchaser Servant, Sunchaser Shaman, Khan

**Insectoid** (虫族)
- Current effect (translated, ZH record 2020-06-18): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: When any allied non-Insectoid piece dies, if an allied piece with the same name is on the board, summons a random Insectoid piece costing the highest sell price among the same-name pieces. 2: When any allied piece dies, if an allied piece with the same name is on the board, summons a random Insectoid piece based on the highest sell price among the same-name pieces, or that price +1.
- Official English (record 2020-06-18): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2：When there are duplicate non-Insectoid allied pieces on the chessboard and one of them dies, summons a random insectoid piece based on the highest cost among the living duplicates. 4：When there are duplicate non-Insectoid allied pieces on the chessboard and one of them dies, summons a random insectoid piece based on the highest cost among the living duplicates.
- Pieces: Lord of Sand, Poisonous Worm, Thorn Predator, Spider Queen

**Divinity** (神族)
- Current effect (translated, ZH record 2021-12-16): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. Divinity (2): Ability cooldown -40% for all Divinity pieces and allied pieces whose race synergy is not active. Divinity (4): Ability cooldown -60% for all Divinity pieces and allied pieces whose race synergy is not active.
- Official English (record 2021-12-16): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: Reduces Ability CD by 40% for all allied Divinities and other allies whose Race's synergies are not triggered. 4: Reduces Ability CD by 60% for all allied Divinities and other allies whose Race's synergies are not triggered.
- Pieces: Goddess of Light, Grand Herald, God of War, Khan, God of Thunder, Old God of the Dusk Forest (unofficial)

**Dragon** (龙族)
- Current effect (translated, ZH record 2021-03-03): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: At the start of battle, 3 allied Dragons start with 100 mana. (If there are more than 3, Dragons placed further left get the effect first.) 5: At the start of battle, 6 allied pieces start with 100 mana. (If there are more than 6, pieces placed further left get the effect first.)
- Official English (record 2020-06-18): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 3: At the start of the battle, 3 allied Dragons have 100 mana. (When there are more than 3 allied Dragons on the Chessboard, those deployed on the relative left will get the effects in priority.) 5: At the start of the battle, 5 Allies have 100 mana. (When there are more than 5 allies on the Chessboard, those deployed on the relative left will get the effects in priority.)
- Pieces: Winter Chiropteran, Shining Dragon, Venom, Umbra, Dragon Knight, Frostblaze Dragon

**Kira** (基拉)
- Current effect (translated, ZH record 2021-12-16): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: Whenever an allied melee piece dies, all allied Kira gain 20% current HP and 20% attack; stacks up to 6 times.
- Official English (record 2021-12-16): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: If any allied melee pieces die, increases current HP by 20%, ATK by 20% for all allied Kiras, can be stacked 6 times
- Pieces: Ogre Mage, Venomancer

**Spirits** (灵族)
- Current effect (translated, ZH record 2021-01-18): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: All allied Spirits have a 35% chance to petrify a melee attacker for 3.5s. 4: All allied pieces have a 35% chance to petrify a melee attacker for 3.5s.
- Official English (record 2020-02-14): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: All allied Spirits have 30% chance to turn the melee attacker into stone for 4 seconds Petrified: No Moving, No attacking, can not use skills 4: All allies have 50% chance to turn the melee attacker into stone for 4 seconds. Petrified: No Moving, No attacking, can not use skills
- Pieces: Stone Spirit, Water Spirit, Thunder Spirit, Fire Spirit, Light Spirit, Dark Spirit

**Dwarf** (矮人族)
- Current effect (translated, ZH record 2025-07-28): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: All allied ranged Dwarf pieces gain: any physical damage dealt to enemies gets +15% damage per 100 yards of distance.
- Official English (record 2021-12-16): Active when you have certain number pieces of this race on board, each level of the same synergy effect stacks 2: Increases all ranged allies' attack range by 2 grids and deals extra damage equals to the number of grids between the two sides * 5%, giving priority to the enemy with the lowest HP within the attack range.
- Pieces: Dwarf Sniper, Helicopter

**Watcher** (守望者)
- Current effect (translated, ZH record 2026-01-29): All allied pieces gain: targeted abilities or item abilities cast on enemies get +1 target.
- Official English (record 2026-01-29): All allies gain the effect: Unit-targeted or Item abilities cast on enemies can affect +1 additional targets.
- Pieces: Hawk

**Ancestor** (先民)
- Current effect (translated, ZH record 2025-05-30): Ancestor[2]: Every 5s, all Ancestor pieces steal (50+2% max HP) HP from the nearest enemy piece to heal themselves. Ancestor[4]: All allied pieces gain: whenever accumulated healing received exceeds 100, deals pure damage equal to the accumulated healing to nearby enemies. Single-hit damage cap 300, CD 5s.
- Official English (record 2025-05-30): Ancestor[2]: All Ancestor pieces steal (50 + 2% of max HP) HP every 5 seconds from the nearest enemy to heal themselves. Ancestor[4]: All allies gain the effect: Whenever total healing received exceeds 100, nearby enemies take pure damage equal to the total healing received. Damage dealt has a single-instance cap of 300 and a 5-second CD.
- Pieces: Erebus (unofficial), Penitent Bishop (Mammon), Goddess of War (Valkyrie)

**unknown-EN (半兽人)** (半兽人)
- Current effect (translated, ZH record 2024-03-05): Orc (unofficial)[2]: At the start of battle, all allied Orc pieces charge toward the target with the most enemy units around it.
- Pieces: Living Sand Saint (unofficial), Evernight Watcher, Old God of the Dusk Forest (unofficial)

### Classes

**Prophet (EN unverified)** (预言家)
- Current effect (translated, ZH record 2022-02-10): After battle starts, 16 grids on the board undergo a cataclysm. Pieces standing on or passing through them take 150 pure damage per second.
- Pieces: Bad Omen (Dutch Iron)

**Martialist** (武师)
- Current effect (translated, ZH record 2022-01-13): (1) 20% of non-pure damage taken by all allied Martialists is converted into pure damage over 5s. The damage over time cannot kill. (2) 30% of non-pure damage taken by all allied Martialists is converted into pure damage over 5s. The damage over time cannot kill. (3) 30% of non-pure damage taken by all allied pieces is converted into pure damage over 5s; killing an enemy clears this damage over time. The damage over time cannot kill.
- Pieces: Heaven Brew (Qin Lang), Qin Xuan / Plume Warrior, Dualplot (Taorive), Space Walker

**Wizard** (巫师)
- Current effect (translated, ZH record 2020-08-31): 2: Races/classes that need 4 or more pieces need 1 fewer piece to activate. 4: Races/classes that need 4 or more pieces need 1 fewer piece to activate. If only one non-Wizard race/class synergy is active, all of its tiers are unlocked.
- Official English (record 2020-08-31): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 2: Required Pieces -1 for Synergies of Race/Class that requires 4 and above Pieces 4: 1 less chess piece is required for activating synergy with at least 4 unique chess pieces; if there's only the synergy of one race/class is activated other than the Wizard synergy, then the highest tier of this synergy will be activated immediately
- Pieces: Voice of the Deep (unofficial), Soul Devourer, Grand Herald, Grimtouch, Flower Spirit (unofficial)

**Priest** (牧师)
- Current effect (translated, ZH record 2021-08-19): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 1: When the chess player takes more than 2 damage, blocks 20% of it (reduction rounded). 2: When the chess player takes more than 2 damage, blocks {0} of it; when the chess player takes more than 8 damage, gets Green Essence x1. 3: When the chess player takes more than 2 damage, blocks 20% of it; when the chess player takes 8-14 damage, gets Green Essence x1; when the chess player takes 15 or more damage, gets a Revival Essence (unofficial) x1.
- Official English (record 2021-08-19): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 1: When the chess player receives more than 2 damage, it'll block 20% damage for the chess player 2: When the chessplayer receives more than 2 damage, blocks {0} damage for the chessplayer; and if the chessplayer receives more than 8 damage, gets Green Essence x1 3:When the chess player receives more than 2 damage, blocks 20% of damage for the chess player; when the chess player receives 8 to 14 damage, gets Green Essence x1; when the chess player receives more than 15 damage, gets Red Essence x1.
- Pieces: Goddess of Light, Penitent Bishop (Mammon), Eclipse of Darkness, Fortune Teller, Cave Prodigy, Light Spirit

**Mech** (工匠)
- Current effect (translated, ZH record 2020-06-03): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: After winning a battle against a player, if at least one allied Mech survives on the board, gets Heart of Mech x1. 6: After winning a battle against a player, if at least one allied Mech survives on the board, gets Golden Heart of Mech x1.
- Official English (record 2020-06-03): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 3: After winning a battle against another player, if at least one ally Mech survives, gets Heart of Mech x1. 6: After winning a battle against another player, if at least one ally Mech survives, gets Golden Heart of Mech x1.
- Pieces: Heaven Bomber, Sky Breaker, Ripper, Gem Artisan, Devastator, Helicopter

**Witcher** (猎魔人)
- Current effect (translated, ZH record 2020-02-14): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 1: If the enemy has Demons, their Demon type count is increased by 1. 2: All allied Demons count as one type, and enemy Demon effects are disabled.
- Official English (record 2020-02-14): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 1: If there's any Demon pieces on the enemy Chessboard, the Demon types will be increased by 1. 2: All allied demons deem to be one，The enemy demon effect is invalid
- Pieces: Resentful Murk, Taboo Witcher, Fallen Witcher, Soul Blade

**Druid** (德鲁伊)
- Current effect (translated, ZH record 2020-02-14): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: 2 identical 1-star Druids on the board can upgrade to 2 stars. 4: 2 identical Druids of the same star level on the board can upgrade.
- Official English (record 2020-02-14): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 2: Every 2 1-star Druid on the board could combined to one 2-star Druid 4: Every 2 2-star Druid on the board could combined to one 3-star Druid
- Pieces: Unicorn, Wisper Seer, Warpwood Sage, Razorclaw, Khan, Old God of the Dusk Forest (unofficial)

**Shaman** (萨满)
- Current effect (translated, ZH record 2021-12-07): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: At the start of battle, turns 1 random enemy piece into a penguin for 6s. 4: At the start of battle, turns 1 random enemy piece into a penguin. All allied Shamans gain: transform into a random piece with sell price +1/+2 (80%/20% chance); the transformed piece does not inherit synergies or items. (The opening transformation takes 2s.)
- Official English (record 2021-08-19): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 2: Change a random enemy unit into a penguin when battle starts for 6s 4：At the start of a battle, hexes a random enemy into penguin for 6s. And all Shamans get a buff: when killed by an enemy piece, hexes the enemy into a random chess piece of the same cost and the same star level.Possibility to turn into a random Chess Piece which costs +1 or +2 （+1:40%,+2:60%）
- Pieces: Defector, Lava Shaman, Jade General (Ji Xi), Fire Spirit, Sunchaser Shaman, Storm Shaman, Radiant Empress (unofficial), The Scryer

**Mage** (法师)
- Current effect (translated, ZH record 2025-08-27): Class trait: for each living allied Mage piece on the board, Mage pieces gain +10% mana gain, up to +50% → 60%. [3] Enemy pieces magic resistance - 30%. [6] Enemy pieces magic resistance - 70%. [9] Enemy pieces magic resistance - 70%; enemy units are Frozen when they take magical damage. Freeze duration = actual magical damage taken / 300 seconds. Frozen: cannot normal attack, move, or cast abilities.
- Official English (record 2021-01-19): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 3: All enemy lose 40% Magical Resistence 6: All enemy lose 90% Magical Resistence 9: All enemy lose 130% Magical Resistence
- Pieces: Hawk, Winter Chiropteran, Ogre Mage, The Source, Flame Wizard, Shining Dragon, Thunder Trace (Lei Rong), Thunder Spirit, Tortola Elder, Frostblaze Dragon, God of Thunder

**Warlock** (术士)
- Current effect (translated, ZH record 2021-11-04): Deploy the required number of different pieces of this class to activate; tiers stack. 2: All allied pieces' attacks and ability damage gain 15% lifesteal. 4: All allied pieces' attacks and ability damage gain 25% lifesteal. 6: All allied pieces' attacks and ability damage gain 40% lifesteal. When an enemy unit dies, the nearest allied piece with the Warlock effect absorbs its max HP and current mana.
- Official English (record 2020-02-14): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 2: Grants 10% lifesteal to all allied units and its skill 4: Grants 30% lifesteal to all allied units and its skill 6: Grants 60% lifesteal to all allied units and its skill
- Pieces: Unknown Horror, Erebus (unofficial), Desperate Doctor, Nightmare (formerly Egersis Prophet), Shadow Devil, Poisonous Worm, Soul Reaper, Venomancer, Dark Spirit

**Assassin** (潜行者)
- Current effect (translated, ZH record 2021-05-07): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: All allied Assassins' attacks have a 15% chance to deal 3x damage; the first normal attack after entering battle has a 50% chance to deal 3x damage. 6: All allied Assassins' attacks have a 20% chance to deal 4x damage; the first normal attack after entering battle has a 100% chance to deal 4x damage. 9: All allied Assassins' attacks have a 30% chance to deal 5x damage; the first normal attack after entering battle has a 100% chance to deal 5x damage.
- Official English (record 2020-08-31): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 3: All allied Assassins gain a 15% chance to exert 300% damage, and a 40% chance to deal 300% damages with their first Base attack in each round 6: All allied Assassins gain a 20% chance to exert 350% damage, and a 60% chance to deal 350% damages with their first Base attack in each round 9：All allied Assassins have 30% chance to deal 400% damages, and have 100% chance to deal 400% damages with their first base attack in each round
- Pieces: Sunchaser Servant, Soul Breaker, Water Spirit, Abyssalcrawler, Venom, Mist Phantom King (Wuril), Dominator (Dominiel), Ember Blade (Di Wei), Lord of Sand, Shadowcrawler, Evernight Watcher, Redfox Swordsman (Forro Vulpes), Thorn Predator, Phantom Queen, Shining Assassin

**Knight** (骑士)
- Current effect (translated, ZH record 2022-02-10): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 2: Every 2s, all allied Knights have a 25% chance to gain a damage-reduction shield for 2s that gives 70% magic resistance, 25 armor, and 15 HP regeneration per second. Always triggers at the start of battle. 4: Every 2s, all allied Knights have a 45% chance to gain a damage-reduction shield for 2s that gives 70% magic resistance, 25 armor, and 20 HP regeneration per second. Always triggers at the start of battle. 6: Every 2s, all allied Knights have a 70% chance to gain a damage-reduction shield for 2s that gives 70% magic resistance, 25 armor, and 25 HP regeneration per second. Always triggers at the start of battle.
- Official English (record 2020-11-20): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 2: All allied Knights gain a 100% chance at the beginning of each round and then a 25% chance in every 3 seconds, to obtain a damage-reducing shield, which grants +75% Magic Resistance, +30 Armor. The shield lasts for 2 seconds. 4: All allied Knights gain a 100% chance at the beginning of each round and then a 48% chance in every 2 seconds, to obtain a damage-reducing shield, which grants +75% Magic Resistance, +30 Armor. The shield lasts for 2 seconds. 6: All allied Knights gain a 100% chance at the beginning of each round and then a 65% chance in every 2 seconds, to obtain a damage-reducing shield, which grants +75% Magic Resistance, +30 Armor. The shield lasts for 2 seconds.
- Pieces: Frost Knight, Hell Knight, Light Blade, Goddess of War (Valkyrie), Avenge Knight, Evil Knight, Argali Knight, Dragon Knight, Cannon Granny

**Hunter** (猎人)
- Current effect (translated, ZH record 2025-07-28): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: Allied Hunters' normal attacks have a 20% chance to always hit and attack one extra time. 6: Allied Hunters' normal attacks have a 50% chance to always hit and attack one extra time. 9: All allied normal attacks knock enemies back 100 yards and deal 50 physical damage.
- Official English (record 2020-02-14): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 3: All allied Hunters attack +35 and grants 30% chance to pierce through evasion 6: All allied Hunters attack +80 and grants 65% chance to pierce through evasion
- Pieces: Shining Archer, Egersis Ranger, Skull Hunter, Dwarf Sniper, Bobo, Umbra, Wind Ranger, Spider Queen, Siren, Tsunami Stalker, Underworld Enforcer (unofficial)

**Warrior** (战士)
- Current effect (translated, ZH record 2021-11-18): Deploy the required number of different pieces of this class to activate; for different tiers, only the highest applies. 3: All allied Warriors armor +8. 6: All allied Warriors armor +16. 9: All allied Warriors armor +20. When taking physical or magical damage, reflects pure damage equal to that piece's armor.
- Official English (record 2020-07-30): Active when you have certain number pieces of this class on board, each level of the same synergy effect stacks 3: All allied warrior armor +5 6: All allied warrior armor +12 9: All allied warrior armor +18. When receiving physical or magical damage, reflects pure damage equals to the value of the recipient's Armor
- Pieces: Stone Spirit, Tusk Champion, Redaxe Chief, Living Sand Saint (unofficial), Swordman, Werewolf, Abyssal Guard, Ghost Kid, God of War, Berserker, Doom Arbiter, Pirate Captain, Sacred Lancer, Rogue Guard, Mountain Chief

**unknown-EN (武装姿态)** (武装姿态)
- Current effect (translated, ZH record 2026-09-04): Pieces change class based on their board position: Warrior in the front two rows, Assassin in the back two rows.
- Pieces: The Exile (unofficial)

## Items

Two item systems are documented. In the older system (English site, records 2020-2022), basic items drop in creep rounds and combine by recipe. In the current system (Chinese site, records 2024-2026), items have quality tiers and no recipes; creep rounds give items and Item Chests (pick from offered items), and Relics exist since late 2024. The exact switch date was not found; the season history above tracks the related patch notes.

### Current system (official ZH site, https://ac.dragonest.com/zh-cn/equipment)

| Item (EN / ZH) | Quality | Category | Stats | Effect | Updated |
|---|---|---|---|---|---|
| Defy Fate (unofficial) / 逆命 | Common | Lifesteal |  | Defy Fate: Unique Passive. Damage dealt by the carrier to enemy pieces of higher value than itself +25%. | 2026-01-29 |
| Ironbite Sword (unofficial) / 蚀铁剑 | Common | Physical | Shatter: Unique Passive. The carrier's normal attacks reduce the target's armor by 5 for 5s. |  | 2025-11-27 |
| Spirit Amulet (unofficial) / 精神护符 | Common | Defense | Evasion +25% | Vigilance: the carrier is immune to mind control. | 2025-11-27 |
| Mana Storage Crystal (unofficial) / 储魔水晶 | Common | Mana Regen | The piece gains 40 mana at the start of battle. Multiple Mana Storage Crystals do not stack. |  | 2025-11-27 |
| Blink Ring (unofficial) / 闪烁戒指 | Common | Mana Regen | Evasion +10% | Blink: Unique Active. The carrier teleports to the farthest grid from which it can attack an enemy and gains 50 mana. (CD: 15s) | 2025-11-27 |
| Magicka Staff / 魔法杖 | Common | Magic | 0 | Magical damage +15% | 2025-11-27 |
| Life Crystal / 生命晶石 | Common | Defense | HP +300 |  | 2025-11-27 |
| Broken Sword / 断剑 | Common | Physical | Attack +15% |  | 2025-11-27 |
| Wooden Club / 木棍 | Common | Physical | Attack speed +15% |  | 2025-11-27 |
| Cattlehide Armor / 牛皮甲 | Common | Defense | Armor +6 | 0 | 2024-01-10 |
| Magicka Crystal / 魔力晶石 | Common | Mana Regen | 0 | Mana gain +50% | 2025-11-27 |
| Inert Crystal (unofficial) / 失活晶石 | Uncommon | Special |  | Attack +25. High Buyback: Unique Passive. This item's reforge value +1. | 2026-01-29 |
| Egersis Pact / 不眠契约 | Uncommon | Special | Carrier is immune to Plague (Egersis race effect) | [Pact Sealed]: The carrier is immune to Plague (Egersis race effect). If it survives a won battle, at the start of the next round the carrier is turned into a random Egersis piece of the same quality and star level. Only one piece is converted per battle. | 2025-11-27 |
| Priest's Crown (unofficial) / 牧师头冠 | Uncommon | Lifesteal | Attack lifesteal +15% | [Mind Control]: Unique Active. Activates when a summon is detected on the board; takes control of an enemy summon of lower level than the carrier. Lasts up to (controller level x2+2) seconds; the controlled piece temporarily fights for the controller. (CD: 60s) | 2025-11-27 |
| Steelbone Shield (unofficial) / 钢骨护盾 | Uncommon | Defense | Armor +5 | [Near Death]: Unique Passive. Whenever the holder's HP drops below 50% in a home battle, it has a chance to permanently gain +1 armor. The permanent armor only applies while carrying this item. | 2025-11-27 |
| Hat of Clarity (unofficial) / 无惑帽 | Uncommon | Special | HP +300 | [Clarity]: Unique Passive. At the start of battle, the carrier's own value +1. | 2025-11-27 |
| Sigma Hammer (unofficial) / 西格玛重锤 | Uncommon | Special | Concussion: Unique Passive. A melee carrier's normal attacks have a 20% chance to stun the enemy for 3s; a ranged carrier's have a 10% chance to stun for 3s. |  | 2025-11-27 |
| War Horn / 战争号角 | Uncommon | Summon | Inspire: Unique Passive. Summons created by the carrier gain attack speed +60%. |  | 2025-11-27 |
| Dwarven Oath (unofficial) / 矮人誓约 | Uncommon | Special, Physical | Dwarven Blood: Unique Passive. The carrier also gains the Dwarf synergy. |  | 2025-11-27 |
| Counter Amulet (unofficial) / 反击护符 | Uncommon | Defense, Special | Counter: Unique Active. Triggers when damaged; for 4s, 50% of damage taken is dealt back to the attacker as pure damage. (CD: 30s) |  | 2025-11-27 |
| Tower Shield (unofficial) / 塔盾 | Uncommon | Defense | 100% chance to block 30 damage when hit by a normal attack. | Block: does not stack; the highest value applies. | 2025-11-27 |
| Brood Mother's Egg (unofficial) / 虫母之卵 | Uncommon | Special |  | Mimic Parasitism: Unique Passive. The carrier also gains the Insectoid synergy, and pieces with the same name as the carrier join the Brood queue. | 2025-11-27 |
| Sunchaser War Drum (unofficial) / 逐日战鼓 | Uncommon | Mana Regen | 0 | Devout Blessing: Unique Active. Allied pieces within 234 yards of the carrier gain 30 mana. (CD: 15s) | 2025-11-27 |
| Chalice of Gifts (unofficial) / 馈赠圣杯 | Uncommon | Defense |  | Gift: Unique Active. When the carrier or an allied piece is below 50% HP, restores 400 HP to the carrier and the allied unit with the lowest current HP. (CD: 20s) | 2025-11-27 |
| Pendant of Mercy (unofficial) / 慈心坠 | Uncommon | Defense |  | Ceasefire: Unique Active. Disarms the enemy unit with the highest attack for 3s. (CD: 20s) | 2025-11-27 |
| Blade of the Wa King (unofficial) / 瓦王之刃 | Rare | Lifesteal | [Demonbane]: Damage dealt by the carrier to Demon and Egersis pieces +50%. | Attack +25, attack lifesteal +25% | 2025-11-27 |
| Orb of Light (unofficial) / 光之法球 | Rare | Defense | [Holy Light]: Unique Active. Activates when several allied pieces are injured; restores 500 HP to all allied pieces within 313 yards of the carrier. (CD: 12s) |  | 2025-11-27 |
| Fire Spirit's Last Wish (unofficial) / 火之灵遗愿 | Uncommon | Special | HP +600 | [Rebirth]: Unique Passive. Deathrattle: revives once in place; works once per round. The revived piece loses buffs from races/classes. | 2025-11-27 |
| Purge (unofficial) / 净蚀 | Rare | Special | Attack speed +30% | [Counter]: Unique Active. Counters the current attack target, removing all of its race/class buffs. (CD: 5s) | 2025-11-27 |
| Eye of Gork (unofficial) / 高尔克之瞳 | Rare | Defense | HP +300 | Frost Curse: Unique Passive. Damage dealt by the carrier reduces the target's attack speed by 50% and healing received by 50% for 5s. | 2025-11-27 |
| Bloodthirsty Bone Chain (unofficial) / 嗜血骨链 | Rare | Lifesteal | Ability lifesteal +30% |  | 2025-11-27 |
| Guardian Shield / 守护之盾 | Rare | Defense | 100% chance to block 30 damage when damaged. | Guard: Unique Active. Allies within 234 yards gain a 100 chance to block 30 damage from normal attacks for 8s. (CD: 30s) Block: does not stack; the highest value applies. | 2025-11-27 |
| Mithril Armor (unofficial) / 秘银甲 | Rare | Defense | Armor +8 | Futility: Unique Passive. Blocks a targeted ability or item effect. (CD: 15s) | 2025-11-27 |
| Magic Power Ring (unofficial) / 魔能戒指 | Rare | Magic | Magical damage +45% |  | 2025-11-27 |
| Abyssal Jewel (unofficial) / 深渊珠宝 | Rare | Defense | 0 | Embrace the Abyss: Unique Passive. Allies within 313 yards of the carrier gain 30% magic resistance. | 2025-11-27 |
| Sacrificial Staff (unofficial) / 献祭法杖 | Rare | Magic |  | Sacrifice: Unique Passive. Each time the carrier kills an enemy unit it gains 1 stack of Offering; it loses 4 stacks each time it dies. Offering stacks up to 10. Mystic Rite: Unique Passive. Each Offering stack increases the carrier's magical damage by 6%. | 2025-11-27 |
| Magic Crystal Axe (unofficial) / 魔晶斧 | Rare | Physical | Attack +15% | Shatter: Unique Passive. The carrier's normal attacks reduce the target's armor by 10 for 5s. | 2025-11-27 |
| Axe of Fury / 狂战斧 | Rare | Physical | Attack +30% | Cleave: Unique Passive. The carrier's melee normal attacks deal pure damage equal to 30% of the attack damage to enemy units within 160 yards of the target. | 2025-11-27 |
| Lightning Hammer (unofficial) / 雷电之锤 | Rare | Physical | Attack speed +30% | Chain Lightning: Unique Passive. The carrier's normal attacks have a 20% chance to release 1 lightning bolt at an enemy unit that bounces up to 3 times, dealing 100 magical damage each time. (CD: 0.5s) | 2025-11-27 |
| Frantic Mask / 疯狂面具 | Rare | Physical, Special | Attack lifesteal +20%, attack speed +15% | Derangement: silences itself; cannot be dispelled. | 2025-11-27 |
| Radiant Crown (unofficial) / 璀璨冠冕 | Epic | Defense, Special | Evasion +25% | Imperial Might: Unique Passive. Every 2s, deals 100 pure damage to enemy units within 234 yards of the carrier. Imperial Radiance: Unique Passive. Enemy units within 234 yards of the carrier have a 25% chance to miss attacks. | 2025-11-27 |
| Exorcism Armor (unofficial) / 驱魔甲 | Epic | Defense | Armor +6, magic resistance +30% | Purify: Unique Passive. When the carrier gets a negative status, it dispels all its negative statuses and is immune to negative statuses for the next 3s. (CD: 15s) Cannot dispel negative statuses from items of higher quality. | 2025-11-27 |
| Arcane Chain (unofficial) / 秘法之链 | Epic | Magic, Control | Magical damage +30% | Greater Imprisonment: Unique Active. Binds enemy units within 200 yards of the highest-value piece so they cannot move for 5s, and deals 200 magical damage. (CD: 15s) | 2025-11-27 |
| Morningstar (unofficial) / 晨星锤 | Epic | Physical, Control | Attack +30% | Mighty Blow: Unique Active. Stuns an enemy unit within 100 yards for 5s and deals 100 pure damage; ignores ability immunity. (CD: 7s) | 2025-11-27 |
| Book of the Dead (unofficial) / 亡者之书 | Epic | Special | Summoning Pact: Unique Passive. 100% chance for summon count +1. | Raise Souls: Unique Active. Awakens 1 Egersis Archer and 1 Egersis Swordsman; only the original carrier can use it. (CD: 45s) The Egersis Archer and Swordsman's level is based on the carrier's value. | 2025-11-27 |
| Shadow Blade (unofficial) / 影刃 | Epic | Physical, Defense, Special | Evasion +40%, attack speed +20% | Shadow Strike: Unique Passive. Activates when damaged; Shadow Blade's bonuses are doubled for 5s. (CD: 20s) | 2025-11-27 |
| Magic Mirror (unofficial) / 魔镜 | Rare | Special | Attack +35 | Inner Demon: Unique Active. Creates 2 mirror copies of the carrier that cannot cast abilities, lasting 7s; copies deal 100% of the original's damage. Only the original carrier can use it. (CD: 15s) | 2025-11-27 |
| Eternity (unofficial) / 永恒 | Epic | Defense | HP +600 | Eternal Shield: Unique Passive. Triggers when damaged; the carrier dispels its negative statuses and gains ability immunity for 5s. (CD: 30s) | 2025-11-27 |
| Orb of Refresh / 刷新球 | Epic | Magic, Mana Regen | Mana gain +200% | Refresh: Unique Active. After the carrier casts its ability, resets the ability cooldown and gains 100 mana. (CD: 30s) | 2025-11-27 |
| Thunderous Heavy Staff (unofficial) / 万钧重棍 | Epic | Physical, Special | Attack speed +40% | Crushing Weight: Unique Passive. The carrier's normal attacks always hit and deal an extra 70 fixed physical damage. | 2025-11-27 |
| Demon Gloves / 恶魔手套 | Legendary | Special | Attack +50 | Ancient Demon: Unique Passive. All damage dealt by the piece carrying this item is converted to pure damage. | 2025-11-27 |
| Sky Mirror Shield (unofficial) / 天镜盾 | Legendary | Defense | 100% chance to block 50 damage when hit by a normal attack. | Tit for Tat: 30% chance to reflect all damage and targeted negative effects. Block: does not stack; the highest value applies. | 2025-11-27 |
| Plain Longsword (unofficial) / 古朴的长剑 | Legendary | Physical | Attack +500 | If the wearer loses a home battle, it drops randomly to another player. Chance to get it from a Legendary Item Chest: 1%. Cannot be reforged. | 2025-11-27 |
| Immortal (unofficial) / 不朽 | Legendary | Defense | HP +900 | Purify: Unique Passive. Triggers on taking lethal damage; dispels its negative statuses and is immune to all damage for 3s. Works only once per battle. | 2025-11-27 |
| Kira Mark (unofficial) / 基拉印记 | Legendary | Special | HP +900 | Grudge Assimilation: Unique Passive. At the start of battle, the carrier's own value +2 and it gains the Kira synergy. Maximum value bonus is +2. | 2025-11-27 |
| Crimson Heart (unofficial) / 赤红之心 | Epic | Defense | HP +900 | Counter Axe: Unique Passive. When the carrier is hit by a normal attack, 50% chance to deal 150 physical damage to enemy units within 234 yards and restore 150 HP. (CD: 1s) | 2025-11-27 |
| Scythe of Vyse / 邪恶镰刀 | Legendary | Magic, Control | Magical damage +60% | Hex: Unique Active. Turns the highest-value enemy unit into a penguin for 5s. (CD: 15s) | 2025-11-27 |
| Frenzied Ravager (unofficial) / 狂劫 | Legendary | Physical | Attack +45% | Fury: Unique Passive. The carrier's normal attacks have a 25% chance to deal 400% damage. | 2025-11-27 |
| Mjollnir / 雷鸣重锤 | Legendary | Physical | Attack speed +60% | Lightning Strike: Unique Passive. The carrier's normal attacks have a 20% chance to release 1 lightning bolt at an enemy unit that bounces up to 5 times, dealing 200 magical damage each time. (CD: 0.5s) | 2025-11-27 |
| Barbaric Cuirass / 蛮荒战甲 | Legendary | Physical, Defense | Armor +15 | Armor Aura: Unique Passive. Allied units within 300 yards of the carrier gain 10 armor. Aggression Aura: Unique Passive. Enemy units within 300 yards of the carrier lose 10 armor. | 2025-11-27 |
| Bloodbath Skull (formerly Infused Mask) / 浴血之颅 | Legendary | Physical, Lifesteal | Normal attack lifesteal +45%, ability lifesteal +45% | Bloodbath: Unique Active. For every 4% HP the carrier loses, gains 1% normal attack lifesteal and 1% ability lifesteal. | 2025-11-27 |
| Cloak of Invisibility (unofficial) / 隐身斗篷 | Common | Defense | Magic resistance +25% | Conceal: the carrier hides its position during the preparation round. | 2024-01-10 |
| Darcy's Blessing (unofficial) / 达西恩赐 | Epic | Defense | HP regeneration +30 | Divine Wood Radiance: Unique Active. Activates when several allied pieces are injured; restores 500 HP to the carrier and allied units within 234 yards. (CD: 20s) Faith: Unique Passive. Healing done by the carrier +20%. | 2024-01-10 |
| Storm Sign (unofficial) / 风暴指示 | Epic | Defense | The piece gains 40 mana at the start of battle | Cloud of Order: Unique Passive. When the carrier casts its ability, it immediately releases a Cloud of Order on the area with the most enemies, silencing enemies within 234 yards and reducing their magic resistance by 30% for 3s. (CD: 20s) | 2024-01-10 |
| Dragonbone Cannon (unofficial) / 龙骨大炮 | Epic | Defense, Special | Evasion +40% | Self-defense Cannon: when taking melee damage, pushes the attacker to the grid farthest from itself and stuns it for 1s. (CD: 5s) | 2024-01-10 |
| Scepter of Judgment (unofficial) / 裁决权杖 | Legendary | Magic | Magical damage +30% | Mana Burst: Unique Active. Deals 800+5% max HP pure damage to 1 random enemy unit. Every 5% magical damage adds 2.5% max HP pure damage. (CD: 15s) | 2024-01-10 |

English stats and effects in this table are translated from the Chinese site unless the item had an official English name.

### Older recipe system (official EN site, https://ac.dragonest.com/en/equipment)

| Item | Quality | Category | Stats | Effect | Recipe | Updated |
|---|---|---|---|---|---|---|
| Magicka Crystal | Common | Magic | +100% Mana restoration on hit |  |  | 2022-05-17 |
| Wooden Club | Common | Physic | +15% Attack speed |  |  | 2022-05-17 |
| Cattlehide Armor | Common | Defense | +5 Armor |  |  | 2022-05-17 |
| Dracula Mask | Common | Physic | +5% Lifesteal when dealing base attack |  |  | 2022-05-17 |
| Resistance Cloak | Common | Defense | +15% Magic resistance |  |  | 2022-05-17 |
| Broken Sword | Common | Physic | +20 ATK |  |  | 2022-05-17 |
| Life Crystal | Common | Defense | +250 HP |  |  | 2022-05-17 |
| Magicka Staff | Common | Magic | +10% Ability damage |  |  | 2022-05-17 |
| Rune Hammer | Common | Physic | +20 ATK |  |  | 2022-05-17 |
| Lucky Coin | Common | Defense | +10% Evasion |  |  | 2022-05-17 |
| Egersis Pact | Common | Defense | Non-Egersis carriers of Lv.5 or lower are immune to Plague (Egersis racial effect). [Pact Sealed]: If the carrier survives a home battle, at the start of the next round they will transform into a random Egersis piece of the same cost and star level. |  |  | 2025-11-27 |
| EXP Book |  | Consumables |  | Get 2 EXP for your chess player (only 1 EXP in Quick mode) |  | 2022-05-17 |
| Golden Heart of Mech |  | Consumables |  | Use it to immediately refresh the store once, and 5 Legendary Pieces will show up |  | 2022-05-17 |
| Heart of Mech |  | Consumables |  | Use it to immediately refresh the store once |  | 2022-05-17 |
| Green Essence |  | Consumables |  | Use it to recover 1-5 HP for your chess player |  | 2022-05-17 |
| Dragonthorn Bracer | Epic | Magic / Physic | Ability lifesteal +10% |  |  | 2022-05-17 |
| Nether Essence | Epic | Magic | +40% Mana restoration on injury |  |  | 2022-05-17 |
| Ghost Blade | Epic | Physic | +30 ATK |  |  | 2022-05-17 |
| Wraith Shard | Epic | Physic | +30% Attack speed |  |  | 2022-05-17 |
| Mysterial Staff | Epic | Magic | +15% Ability damage +100% Mana restoration on hit |  |  | 2022-05-17 |
| Dragon Blood Axe | Epic | Defense | +650 HP |  |  | 2022-05-17 |
| Ice Platemail | Legendary | Defense | +8 Armor +15% Evasion rate | Decreases ATK Speed of the attacker by 25% for 5 seconds when receiving base attacks. | Lucky Coin + Cattlehide Armor | 2022-05-17 |
| Burst Rifle | Legendary | Physic | +10% Evasion Rate +25 ATK | The carrier's ranged base attack will additionally attack 1 enemy within the range, dealing damage equals to 15% ATK. | Broken Sword + Lucky Coin | 2022-05-17 |
| Teleport Dagger | Legendary | Magic | +20 ATK +100% Mana Restoration on hit | Active: Flashes to the farthest grid that can attack the enemy and gains 50 Mana. CD: 15 seconds. | Broken Sword + Magicka Crystal | 2022-05-17 |
| Python Wand | Legendary | Physic | +15 ATK +350 HP | Active: Increases 100% Magic Resistance, and immune to Abilities for 4 seconds. CD: 30 seconds | Life Crystal + Rune Hammer | 2022-05-17 |
| Frantic Mask | Legendary | Physic | +10% Lifesteal when dealing base attack | Passive: +40% attack speed. Silence the holder | Dracula Mask + Wooden Club | 2022-05-17 |
| Voodoo Staff | Legendary | Magic | +30% Ability damage |  | Magicka Staff + Magicka Staff | 2022-05-17 |
| Steel Platemail | Legendary | Defense | +5 Armor | Grants +3 Armor for allies within 1 grid | Cattlehide Armor + Cattlehide Armor | 2022-05-17 |
| Orb of Regen | Legendary | Magic | -20% Ability CD +100% Mana restoration on hit |  | Magicka Crystal + Magicka Crystal | 2022-05-17 |
| Crystal Sword | Legendary | Physic | +20 ATK | Passive: Have a 15% chance to deal 1.5 times of damage while base attack | Broken Sword + Broken Sword | 2022-05-17 |
| Dragonbone Shield | Legendary | Defense | +250 HP | Passive: Have a 50% chances to block 50 damage while being attacked | Life Crystal + Cattlehide Armor | 2022-05-17 |
| Pulse Staff | Legendary | Magic | +10% Ability damage +100% Mana restoration on hit | Active: deals 350 Magical damage to 1 random enemy piece. CD: 15 seconds(The initial CD is 2 seconds when entering a battle.) | Magicka Crystal + Magicka Staff | 2022-05-17 |
| Anti-Maga Cloak | Legendary | Defense | +20% Magic resistance | Active: grants a shield that can block 150 Magical Damage for all allies within 2 grids for 5 seconds. CD: 30 seconds | Resistance Cloak + Resistance Cloak | 2022-05-17 |
| Blade Mail | Legendary | Defense | +5 Armor +10 ATK | Active: Reflect 100% of the damage received in 4 seconds back to the attacker. Cooldown: 30 seconds. | Broken Sword + Cattlehide Armor | 2022-05-17 |
| Maelstorm | Legendary | Physic | +25 ATK | Passive: Base attack has a 25% chance to deal 100 Magical Damage to up to 3 enemies. CD: 0.5 seconds. | Rune Hammer + Rune Hammer | 2022-05-17 |
| Oblivion Staff | Legendary | Physic | +20 ATK +10% Attack speed | Passive: Base attack has a 35% can not be avoided and -35% target's HP restoration for 5 seconds | Wooden Club + Rune Hammer | 2022-05-17 |
| Shadow Edge | Legendary | Physic | +15% Evasion +15% Attack speed | Passive: deals Pure Damage equals to 20% of the Attack Damage to enemies within 1 grids with melee attacks | Wooden Club + Lucky Coin | 2022-05-17 |
| Cloud Halberd | Legendary | Defense | +15% Evasion +250 HP | Active: Disarms the enemy which has the highest Attack for 3 seconds. CD: 30 seconds. | Lucky Coin + Life Crystal | 2022-05-17 |
| Puppet Mask | Legendary | Defense | +15% Magic resistance | Passive: +15% lifesteal when dealing base attack to allies within 1 grids | Dracula Mask + Resistance Cloak | 2022-05-17 |
| Spilt Arrow | Legendary | Defense / Physic | +10% Evasion Rate +25 ATK +650 Max HP | The carrier gets "Dwarf" synergy additionally. The carrier's ranged base attack will additionally attack 2 enemies within the range, dealing damage equals to 25% ATK. | Burst Rifle + Dragon Blood Axe | 2022-05-17 |
| Flicker Staff | Immortal | Magic / Defense | +20 ATK +100% Mana Restoration on hit +40% Mana Restoration on injury | Active: Flashes to the farthest grid that can attack the enemy and gains 50 Mana, then reflects 100% of the damage received in 6 seconds back to the attacker. CD: 15 seconds | Teleport Dagger + Nether Essence | 2020-09-24 |
| Halberd of Judgement | Immortal | Defense | +15% Evasion +800 HP | Active: Disarms the enemy which has the highest Attack for 5 seconds. CD: 30 seconds. | Cloud Halberd + Dragon Blood Axe | 2020-09-24 |
| Monkey King Cane | Immortal | Physic | +80 ATK +15% Attack speed | Passive: Base attack has a 100% can not be avoided and -70% target's HP restoration for 5 seconds | Ghost Blade + Oblivion Staff | 2020-09-24 |
| Barbaric Cuirass | Immortal | Defense |  | Passive: +8 armor and 15% attack speed to allies within 1 grid. In the meantime, -8 enemy's armor and 15% attack speed within the same scope | Steel Platemail + Wraith Shard | 2020-09-24 |
| Mjollnir | Immortal | Physic | +50 ATK +30% Attack speed | Passive: Base attak has a 25% chance to deal 200 damage to 5 enemies at most. CD: 0.5 seconds. | Maelstorm + Wraith Shard | 2020-09-24 |
| Dragonbone Ballista | Immortal | Physic | +40 ATK | Passive: Base attack has a 10% chance to deal 4 times of damage | Crystal Sword + Ghost Blade | 2020-09-24 |
| Origin Wand | Immortal | Magic | +20% Ability damage +150% Mana restoration on hit | Active: deals 10% of target's Max HP +450 Magical damage to 1 random enemy piece. CD: 15 seconds(The initial CD is 2 seconds when entering a battle.) | Pulse Staff + Mysterial Staff | 2020-09-24 |
| Orb of Refresh | Immortal | Magic | +200% Mana restoration on hit +15% Ability damage | Passive: Refreshes ability CD immediately after casting it. CD: 30 seconds | Orb of Regen + Mysterial Staff | 2020-09-24 |
| Infused Mask | Immortal | Physic | +10% Lifesteal +50% ATK Speed | Reduces enemy’s Armor by 8 with base attacks, lasts for 5 seconds | Frantic Mask + Wraith Shard | 2020-09-24 |
| Scythe of Vyse | Immortal | Magic | +45% Ability damage +100% Mana restoration on hit | Active: Hex an random enemy to a penguin for 5 seconds. CD: 30 seconds | Mysterial Staff + Voodoo Staff | 2020-09-24 |
| Heart of Tarrasque | Immortal | Defense | +15% Magica resistance | Grants +20% Base Attack lifesteal and +20% Ability lifesteal for allies within 1 grid | Puppet Mask + Dragonthorn Bracer | 2020-09-24 |
| Axe of Fury | Immortal | Physic | +15% Attack speed +15% Evasion +35 ATK | Passive: Deals Pure Damage equals to 30% of the Attack Damage to enemies within 2 grids with melee attacks | Shadow Edge + Ghost Blade | 2020-09-24 |
| Claw Wand | Immortal | Defense | ATK +20 HP +350 +40% Mana Restoration on injury | Active: Increases 100% Magic Resistance, and immune to Abilities for 9 seconds. CD: 30 seconds | Python Wand + Nether Essence | 2020-09-24 |
| Heaven's Gift | Immortal | Defense | +625 HP | Passive: Have a 50% chances to block 50 damage while being attacked Active: Restores HP equals to 10% of their own HP for all allies within 3 grids, and has a 100% chance to block 10 damage for 10 seconds. CD: 30 seconds | Dragonbone Shield + Dragon Blood Axe | 2020-09-24 |
| Jade Pipe | Immortal | Defense | +20% Magic resistance +40% Mana restoration on injury | Active: Provides a shield that blocks 200 Magical Damage for all allies within 3 grids for 5 seconds. CD: 30 seconds | Anti-Maga Cloak + Nether Essence | 2020-09-24 |
| Icearmor | Legendary | Magic | +10 Armor +15% Evasion rate +10% Ability lifesteal | Active: Deals 300 Magical Damage to enemies within 5 grids and reduces their ATK Speed by 50% for 5 seconds. CD: 30 seconds. | Dragonthorn Bracer + Ice Platemail | 2022-05-17 |

## Economy and rules

Source keys refer to the game-data Sources list above. There is no single official rules page; these rules are assembled from official patch notes (2019-2026) and dated third-party guides. Where sources conflict, each version is listed.

### Players, HP, board

- 8 players in Classic. In Duo, 2 players share HP against 3 other teams (official game introduction, https://ac.dragonest.com/en/gameIntroduction).
- Starting player HP: 100 (g_biubiu_2020).
- Max pieces on board = player level; 8 bench slots (g_4399_2019; memuplay guide https://www.memuplay.com/blog/auto-chess-economy-guide.html).
- Star-up: 3 identical pieces of the same star combine into the next star, up to 3 stars. The Druid synergy allows 2-for-1 combines.

### Gold

| Rule | Value | Source |
|---|---|---|
| Base income | Rounds 1-4 pay 1/2/3/4 gold; round 5 onward pays 5 | g_biubiu_2020, g_gouhuo_2019, g_bluestacks |
| Win bonus | +1 gold for winning a player round (not creep rounds) | g_biubiu_2020, g_taptap_2019, g_gouhuo_2019 |
| Interest | +1 per 10 gold held, max +5 (at 50+ gold) | g_4399_2019, g_biubiu_2020, g_bluestacks |
| Gold cap | 100 | g_4399_2019, g_taptap_2019 |
| Streak bonus | Win or loss streaks, max +3. Thresholds conflict: 3-4 +1, 5-6 +2, 7+ +3 (g_biubiu_2020, Apr 2020); 3-5 +1, 6-8 +2, 9+ +3 (g_bluestacks, may describe the Dota mod); bonus starts after 2 in a row (g_taptap_2019). Current thresholds unknown. | as listed |
| Streak-break bounty | From 2020-08-30: breaking an opponent's win streak of 5+ pays gold equal to that opponent's streak gold; ties do not count (from 2020-10-13) | n_2020_08_30 |
| Eliminated players | From 2020-08-30: part of an eliminated player's gold goes to random survivors | n_2020_08_30 |
| Creep gold | From 2025-05-29: creeps drop gold when defeated | n_2025_05_29 |

### Experience and levels

- +1 XP per round, win or lose (g_4399_2019, g_biubiu_2020).
- Buying XP: 5 gold for 4 XP at launch; 4 gold for 4 XP from 2019-12-12 (n_2019_12_12). No later change found.
- Human synergy gives EXP Books after wins (from mid-2020). Books can push past the normal cap, to level 12.
- Max level: 10 in normal play per g_biubiu_2020. Patch notes give costs for levels 10-11 and 11-12, and a 2026 talent can raise the cap to 16.

XP needed to go from level N to N+1:

| From level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2019-04 (g_4399_2019; levels 10-11 from the "old" values in n_2020_08_exp) | 1 | 1 | 2 | 4 | 8 | 16 | 24 | 32 | 40 | 40 | 40 |
| After 2020-08-30 (n_2020_08_exp, n_2020_08_30) | 1* | 1* | 2* | 4* | 8* | 18 | 28 | 38 | 48 | 20 | 30 |
| After 2020-12-02 (n_2020_12_02) | 1* | 1* | 2* | 4* | 8* | 18* | 28* | 38* | 48* | 24 | 30* |
| After 2025-03-27 (n_2025_03_27) | 1* | 1* | 4 | 8 | 16 | 24 | 36 | 48 (old value in the 2025-05-29 note) | 60 | unknown | unknown |
| After 2025-05-29, latest known (n_2025_05_29) | 1* | 1* | 4 | 8 | 16 | 32 | 48 | 56 | 64 | unknown | unknown |

\* No change found; value carried from the previous row. The "old" values quoted on 2025-03-27 (level 6: 16, level 7: 32, level 9: 56) do not match the 2020 table, so there were further changes between 2020 and 2025 that these sources do not record. Holy Selection War mode used its own table from 2024-11-28 (n_2024_11_28).

### Shop

- 5 slots; refresh costs 2 gold; locking keeps the shop for one round (g_4399_2019, g_biubiu_2020, g_taptap_2019).
- Each slot rolls a cost tier from the level odds, then a random piece in that tier (g_xiaomi_2020).
- From 2020-03-19, after round 5, pieces not bought in the previous 2 refreshes do not reappear (n_2020_03_19).
- 4-cost (Epic) pieces appear from level 5 and 5-cost (Legendary) from level 8 (g_taptap_2019, g_gouhuo_2019).

Shop odds (% for 1/2/3/4/5-cost). No full official standalone table was found. This table is the Dota Auto Chess one; it matches every standalone value that could be confirmed (marked).

| Level | Odds | Standalone confirmation |
|---|---|---|
| 1 | 100/0/0/0/0 | unconfirmed |
| 2 | 70/30/0/0/0 | unconfirmed |
| 3 | 60/35/5/0/0 | unconfirmed |
| 4 | 50/35/15/0/0 | unconfirmed |
| 5 | 40/35/23/2/0 | 3-cost 23% confirmed (n_2020_04_28) |
| 6 | 33/30/30/7/0 | unconfirmed |
| 7 | 30/30/30/10/0 | unconfirmed |
| 8 | 24/30/30/15/1 | confirmed (g_xiaomi_2020); 5-cost was 2% before 2020-03-19 (n_2020_03_19) |
| 9 | 22/25/30/20/3 | 22/30/25/20/3 until 2022-09-08, then 22/25/30/20/3 (n_2022_09_08); 5-cost was 4% before 2020-03-19 |
| 10 | 19/25/25/25/6 | unconfirmed |

### Piece pool

- Copies per piece conflict: 45/30/25/15/10 for costs 1-5 (g_gamersky_pool_2019, g_xiaomi_2020) versus 50/40/30/20/10 (g_taptap_pool_2019, a player post).
- Selling returns the piece to the pool; eliminated players' pieces return too. 1-star pieces and 2-star 1-cost pieces sell for full price; others sell at a loss. Exact sell formula unknown (g_4399_2019, g_gouhuo_2019).
- 2021-05: Limited Legend bans several 5-cost pieces per game; the Golden Heart of Mech can still roll them (n_2021_05_08).
- 2025-05-29: 10 Legendary pieces (was 8) are available per match, 5 of them hidden; the first buyer of a hidden one gets 5 gold. The Extended Chess Pool was removed and replaced by gold-cost Race/Class blocking, one at a time (n_2025_05_29).
- Pandaman invite-only pieces are not in the public pool (EN news "GO GO Pandaman", 2021-02-03).

### Rounds and creeps

- Creep rounds 1, 2, 3, 10, 15, 20, 25, 30, 35, 40, 45, 50 (g_biubiu_2020); g_taptap_2019 describes every 5th round from 10.
- Named creeps: round 20 Ursa Kira, round 25 Swamp Wildwing (n_2020_12_28).
- Item drops:
  - 2019-10-18: a win or draw in a creep round gives a pick of 1 of 3 items; a loss gives 1 random item.
  - 2020-09-23: rounds 10-30 drop 1 common item; rounds 35-50 (except 40) drop 1 rare.
  - 2020-12: the main creep guarantees 1 item per round; round 40 drops 2 (n_2020_12_28).
  - 2022-07-28: round 20 common drops 1 -> 2; the round 25 pick-of-3 changes from common to rare (n_2022_07_28).
  - 2025-01-23 and 2025-02-13: Item Chests added to rounds 10-20 (n_2025_01_23, n_2025_02_13).
- Prep timers (solo, seconds) changed 2023-03-09: round 2 20->15, round 3 25->15, rounds 26 and 31 40->50, round 36 30->40 (n_2023_03_09).
- Fatigue: after round 50 each round halves current HP; Quick mode starts fatigue at round 16 (ZH dev log 2020-01-09). From 2020-06-05, when two players remain, fatigue starts after 5 rounds and no longer kills outright (n_2020_06_05).
- From 2026-01-29, battles run at 3x speed after 31 seconds (TapTap, https://www.taptap.cn/moment/765628521560147076).

### Player damage

| Period | Rule | Source |
|---|---|---|
| 2019-2021 | Each surviving enemy unit deals 1/2/3 for 1/2/3 stars; summons count like their summoner | g_4399_2019; gamersky 2019-06 |
| 2021-05-31 to 2026-01 | Per surviving unit: quality damage (Common/Uncommon/Rare 0, Epic/Legendary 1) + star level (1/2/3), plus once per round a player-level bonus (level 9: +1, level 10+: +2) | n_2021_05_08; confirmed live by n_2023_03_09 |
| From 2026-01-29 | Per surviving unit: floor(piece value / 3) + 1; temporary units count at half value | TapTap, https://www.taptap.cn/moment/765628521560147076 |

### Synergy-linked economy

- Mech (3)/(6): after a home win with a Mech alive, gain a Heart of Mech (free shop refresh) or a Golden Heart of Mech (a refresh showing 5 Legendary pieces). Not dropped in creep rounds from 2021-09-23.
- Human (3)/(6): EXP Books after a win with a Human alive (1 and 4 books in the 2020 text).
- Priest: Green Essence consumable heals the player (from 2020-06-04).

## Gaps

Season history:

- Season 0 and Season 1: exact start dates, launch piece list, launch synergies. No archived official notes before June 2019 were found.
- Costs of 2019-2020 additions where only the name and race/class were given: God of Thunder, God of War, Fortune Teller, Ghost Priest, Rogue Guard, Winter Chiropteran, Shining Archer, Grimtouch (at release), Frostblaze Dragon, The Scryer, Ghost Kid, Lava Shaman.
- English names for the CN-only pieces 独眼猎手 (removed Aug 2019) and 深潜之音 (Jun 2023).
- Quality of Dominator - Dominiel (Sep 2021) and Mist Phantom King·Wuril (Mar 2025).
- Exact season start dates for S1, S4, S7, S19, S21, and S36-S38. S36 and S37 start dates are inferred from the two-month cadence and App Store release dates, not stated.
- Season themes for S1-S6, S14, S16-S21 and S27. Many pass previews are image-only.
- Live content of the Dec 16, 2021 update (image-only); unclear whether the Shadow Devil and Dwarf reworks shipped then. Shadow Devil's return date to the normal pool is unknown.
- CN official feed has no posts for Nov 2021 - Jun 2022 (covered by the English site) or Feb 10 - Aug 11, 2026 (covered only by the short App Store notes, which say "for more update details, please refer to the in-game announcement").
- Image-only official posts (character cards, "New Version Synergy Adjustments" Jun 2020, "Mage(9) Synergy Rework" Apr 2021, "Berserker Removal and Glacier Rework" May 2021, "Limited Legend in rank modes", S32-S36 equipment previews) were not OCR'd.
- Client version numbers before mid-2023 are sparse: 1.0.1 (Jan 2020), 1.6.0 (Aug 2020), experimental builds 2.2.1, 2.3.0, 2.4.0, 2.5.0 (Dec 2020 - Jul 2021). App Store notes give 2.20.2 (Jul 2023) through 2.42.2 (Sep 2026).
- Reddit, the Wayback Machine and fandom were not usable or had nothing; community data (meta, tier lists) is not included.
- Conflict noted but not resolved: the English "S15 Senior Pass" post dated Sep 30, 2021 against patch notes giving S14 as Sep 26 - Nov 21, 2021. The S22 pass end date (Sep 9, 2023) also overlaps the S23 pass start (Sep 4, 2023).

Game data:

- Mana pools / mana per attack: not published on the official site. A 2019 guide mentions abilities cast at 100 mana; the game later moved most abilities to cooldowns (waitingTime / CD values).
- English names missing for 8 pieces added 2023-2026 (EN announcements stop at 2023-03 and resume only Mar-May 2025): 花灵, 厄瑞玻斯, 活砂圣女, 深潜之音, 冥界执法官, 流亡者, 晖光女帝, 暮林旧神. English names for 2 races/classes (半兽人, 武装姿态) and most current-system items are also missing.
- Full standalone shop-odds table, current streak thresholds, current pool sizes, XP for levels 10+, exact sell-value formula and the creep lineup per round: not found in official sources.
- The ZH site list is an illustration catalog; which pieces are in the live rotation at any time is not stated there.
