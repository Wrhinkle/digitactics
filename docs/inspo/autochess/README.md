# Auto Chess reference data

Design reference for Digi-Tactics, compiled 2026-10-15. These files contain no game code. Each file lists its sources at the top. "unknown" marks values no source confirmed.

| File | Contents |
|---|---|
| `dota-auto-chess.md` | Dota Auto Chess (Drodo Studio's Dota 2 custom game, 2019-2026): dated balance eras, economy and rules by era (income, interest, streaks, XP tables, shop odds, pool sizes, creep rounds, drop tables, player damage), item lists with recipes (2019 and 2023), synergy breakpoints by era with effect text, other mechanics (Duo, Dismiss, relics, SSR, Io, Candy Store), full piece rosters with stats for five code snapshots, and ability numbers per star for 2019-06 and 2023-04. |
| `dota-auto-chess.json` | The same data, structured. `pieces_by_snapshot` covers 2019-02-13, 2019-06-05, 2020-07-02, 2021-02-07 and 2023-04-10 and includes every ability variable. Also contains `items_by_snapshot`, `synergy_breakpoints_by_snapshot`, `synergy_text_by_snapshot` and `economy`. |
| `auto-chess-standalone.md` | Auto Chess (Drodo/Dragonest standalone, mobile/PC/PS4): one section per season from the 2019 beta to Season 39 (Sep 2026), a mechanics timeline, the official catalog of 112 pieces with per-star stats and abilities, 24 races and 15 classes, both item systems, and economy rules. |
| `auto-chess-standalone.json` | The same data, structured: `seasons`, `mechanics_timeline`, `pieces`, `synergies`, `items`, `economy`, `gaps`. |

Main sources:
- Dota Auto Chess: the unpacked game files at https://github.com/zyi1992/autochess_vpk (2019-02 to 2023-04) and the Steam Workshop change notes (https://steamcommunity.com/sharedfiles/filedetails/changelog/1613886175).
- Standalone: the official site data and announcements (https://ac.dragonest.com), the TapTap official forum, and the App Store release notes.

Caveats:
- Dota Auto Chess numbers after April 2023 exist only as patch-note changes.
- Standalone mana values and the full shop-odds table are not published.
- Some standalone text is translated from Chinese and is marked "(translated)" or "(unofficial)".
