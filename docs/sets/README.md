# Digimon sets

Plan for rotating Digi-Tactics "sets" like TFT seasons: about 60 shop lines, 25-30 traits, a set mechanic and item twists per set, one set loaded per match. Every line is a 3-form evolution line (Rookie > middle form > final). Rookies may be shared between lines. Middle and final forms must be unique across all sets.

Rule added during planning: every board unit must use a model from the Cyber Sleuth family, so units look consistent. Allowed sources: Digimon Story Cyber Sleuth: Complete Edition, Digimon Links, Digimon World -next 0rder-, Digimon World Re:Digitize (PSP) and Digimon Story: Time Stranger. ReArise, New Century, Digimon Masters, Digimon World 1/2 and Sketchfab uploads are out for units. Digimon World 1 models can go to Little Legends and emotes.

## Files

| File | What it is |
|---|---|
| `digimon-db.json` | 733 distinct Digimon that have at least one model in `tools/convert/downloads-moved/dt_catalog.json`: level, attribute, fields, type, main evolution links, moves (all from Wikimon), every catalog source with asset id, animated flag, Cyber Sleuth-family flag and download status, where it is used today, and which set plan uses it. |
| `set-01-file-island.md/.json` | Set 1 = the current game (60 shop lines in `js/data.js`), mapped to TFT Set 1 and Dota Auto Chess, with a list of forms that still break the Cyber Sleuth-family rule. |
| `set-02-dragon-realm.md/.json` | Set 2, adapting TFT Set 7 Dragonlands. 60 lines, all Cyber Sleuth-family. |
| `set-03-chosen-children.md/.json` | Set 3, adapting TFT Set 4 Fates. Partial: 20 lines. This is where the unique models run out. |
| `download-plan.json` | Catalog assets the plans need that were not in `tools/convert/rips` yet, with set, unit, priority and size estimate. All are in the approved Cyber Sleuth-family download batch, so they are marked "downloading". |

The generator scripts (Wikimon fetch, database build, capacity, set builder) are in the session scratchpad (`.../scratchpad/sets/`), not in the repo. The database is rebuilt from `dt_catalog.json`, `models/index.json`, `tools/convert/rips/index.json`, `tools/convert/new-chains.json` and `js/data.js`.

## Links vs Cyber Sleuth

Checked on the converted rips: Digimon Links and Cyber Sleuth use the same asset ids (`chr092` Gatomon, `chr034` Lilithmon, `chr304` Kabuterimon) and the same base albedo texture (`chrNNNa01.png`) and animation sets (`_mainsequence.smd`). Links rips are lighter: 122 Links rips average 5.7k triangles with one material; 40 Cyber Sleuth rips average 8.9k triangles with about five materials, because they carry extra shading maps (`a01s`, `a01s_a`, `b01`, `env`). With our pipeline keeping the base texture at 512 px, they should look alike. Every Links and Cyber Sleuth rip converted so far has animation clips. Time Stranger's one rip (Gizamon) has none. Next Order and Re:Digitize rips were not sampled.

## Capacity

Counts of distinct Digimon (variants that Wikimon treats as separate Digimon count separately). "Available" = has a Cyber Sleuth-family model and is not a shop form in the current game (wild PvE forms are left out).

| Stage | In catalog (any game) | Cyber Sleuth family | Used by Set 1 shop lines | Available | of which second texture only |
|---|---|---|---|---|---|
| Rookie | 104 | 74 | 43 | 30 | 3 |
| Champion | 189 | 98 | 48 | 43 | 9 |
| Ultimate | 144 | 82 | 47 | 31 | 3 |
| Mega / Ultra | 245 | 134 | 25 | 95 | 12 |

Hybrid (Frontier) Digimon are counted in the stage they play (human spirits as Rookies, beast spirits as Champions, fused spirits as Megas). Armor Digimon count as Champions. "Second texture only" means the Digimon only exists as an alternate texture or mesh inside another Digimon's file (Geremon in the Numemon file, BlackGarurumon in the Garurumon file); they are recolors and every use is flagged.

Set 1 still has 10 forms with no Cyber Sleuth-family model; replacing them uses 2 Champions, 4 Ultimates and 1 Mega from the pool. After that:

| Rule | New lines possible | Sets of 60 (after Set 1) |
|---|---|---|
| Shared Rookie OK, unique Champion-level middle and unique final | 41 (32 without recolors) | 0.7 |
| Same, but the middle may be an Ultimate when the final is a Mega, plus some Mega-to-Mega mode changes at 4-5 cost (the game already does this for Imperialdramon and Beelzemon) | about 80 | 1.3 |
| Unique final only (shared Champions OK) | 120 | 2.0 |

So under the rule this plan uses (second row), there is Set 1, one full new set and a third of another: **Set 1 + Set 2 + 20 lines of Set 3**. Under "unique final only" there would be room for two full new sets. Turning the in-game wild PvE forms into shop lines would add 7 Champions, 4 Ultimates and 14 Megas.

Without the Cyber Sleuth-family rule (any catalog game), the first count was about 120 new lines strict, 160-200 flexible and 250-290 unique-final-only, so 2 to 4 new sets.

Time Stranger has 450+ Digimon but only 75 models (Fresh to Rookie) are ripped. Every new Champion, Ultimate or Mega rip from it raises capacity directly. The approved batch also brings Next Order Vita and 4 more Re:Digitize models that are not in `dt_catalog.json`; they may add a few lines once their names are known.

## Set list

| Set | Name | Adapts | Lines | Mechanic | New downloads (est.) |
|---|---|---|---|---|---|
| 1 | File Island | TFT Set 1 Faction Wars (56 of 57 champions) + 4 Dota Auto Chess pieces | 60 (in game) | Data Wisps (already built) | 43 files to re-source Set 1, about 82 MB |
| 2 | Dragon Realm | TFT Set 7 Dragonlands (58) + 2 Set 7.5 slots | 60 | Mega Dragons: 2-slot dragon lines, +3 trait, Treasure Goldramon | 108 files, about 228 MB |
| 3 | Chosen Children (partial) | TFT Set 4 Fates: 19 of 58 champions + Ornn (4.5) | 20 | Partner Digimon (Chosen): a stage-1 shop offer at 3x cost with one trait doubled | 26 files, about 50 MB |

Total: 177 files, about 360 MB (estimates: Links 1.7 MB, Cyber Sleuth 2.4 MB, Time Stranger 4.4 MB from its one rip, Next Order 2.4 MB assumed). Set 2 has 49 lines with animated sources on all three forms (the rest include Next Order or Time Stranger models whose clips were not checked). 36 of its 60 lines have Wikimon evolution links Rookie > Champion and Champion > final; the rest are game-design pairings, marked in the canon column.

Why this order: Dragonlands is the corpus set with the best fit for what the Cyber Sleuth family has (Breakdramon, Goldramon, Megidramon, Examon, Slayerdramon, Ouryumon, Metallicdramon, Volcanicdramon, Dorugoramon, WarGreymon X), its mechanic is distinct from Set 1, and it keeps TFT history readable (Set 1, then Set 7). Fates was next because the Chosen mechanic is cheap to build and fits Digimon partners. The first draft (before the rule) also had a full Fates set and a Dota Underlords set ("Spiral Mountain", with Dark Master commanders); both relied on ReArise, New Century and Masters models (Devas, Saiyu Warriors, Frontier spirits, Olympos XII) and were dropped. Their plans are archived in the scratchpad.

If more models arrive, the queue is: finish Set 3 (Fates, 39 more lines), then Dota Underlords (85 heroes and Underlords as commanders), TFT Set 3 Galaxies (per-game rule modifiers), TFT Set 6 Gizmos and Gadgets, TFT Set 9 Runeterra Reforged.

## Reuse policy

- Rookies are shared freely, but not two lines with the same Rookie at the same cost in one set (checked by the build script; no conflicts).
- Middle and final forms are unique across all sets. No middle or final form is used twice across Sets 1-3.
- Flagged instead of forbidden: recolors (a form that only exists as a second texture or mesh in another file: Set 2 uses 12, Set 3 uses 12), level-shifted lines (Ultimate middle; 20 in Set 2, 19 in Set 3), Mega-to-Mega lines (3 in Set 2, 16 in Set 3), lines with no Wikimon evolution link, and forms that came from the old roster plan in `new-chains.json`. Each set's "Reuse list" and "Unit notes" name them.
- Set 1 has five lines that repeat a form (Omnimon, Alphamon, Phoenixmon, Daemon at two or three stages; Beelzemon uses one model for two stages). There is nothing left in the Cyber Sleuth-family pool to fix them; they wait for new rips.

## Engine work the plans assume

- A unit's stat role separate from its traits. In Set 1 the role is itself a trait; Sets 2-3 give each unit one of the 8 `DT.ROLES` stat templates and a class trait adapted from the corpus (several classes can share a template).
- New ability types used by some lines: `passive` (no mana bar), `summon`, `copy`, timed self-buffs. Each is marked "(new type)" in the set files.
- Set 2: board size counted in slots (dragons take 2), a per-unit marked trait worth +3, one-purchase dragons, the Treasure Goldramon PvE round.
- Set 3: Partner offers in the shop (stage-1 unit, 3x cost, one marked trait counting double).
- Stats in the set files are Rookie values from `DT.ROLES` and the tier formula in `DT.unitStats`, with HP and AD multipliers taken from the corpus champion relative to its cost tier (clamped 0.85-1.15).

## What blocks covering the whole corpus

The corpus has 1,913 champion slots: 1,562 TFT (25 sets), 85 Underlords, 154 Dota Auto Chess entries (about 120 distinct pieces, many shared with Underlords) and 112 standalone Auto Chess pieces. The plans cover 140 (60 + 60 + 20). **1,773 slots have no unit.** Under the Cyber Sleuth-family rule the most that unique models allow is about 140 slots with unique middle + final forms, or about 180 with unique finals only, so at least 1,733 slots cannot get their own unit with the models that exist today. Even ignoring the rule, the whole catalog (189 Champion-stage and 389 Ultimate/Mega forms) caps out near 290 lines with unique middle and final forms, or about 390 with unique finals only. Covering every corpus slot would need roughly five times more distinct Digimon models than exist on The Models Resource for these games. The open TFT Set 1 slot is Nidalee (Set 1 has 12 one-cost lines for 13 one-cost champions).

## Open questions

- Should second-texture recolors (BlackGarurumon, Geremon, Coredramon (Blue), ShineGreymon Burst/Ruin Mode, Omnimon Zwart and others) count as unique units? The plans use 24 (12 in Set 2, 12 in Set 3) and flag each one.
- Is "unique final only" acceptable for later sets? It doubles what is left after Set 2 and would let Set 3 be finished with shared Champions.
- Wild PvE lines are board units too. In the current game Tyrannomon and SkullGreymon still use Sketchfab models; both have Links models in the batch.
- Re:Digitize asset ids are unknown (the site returns HTTP 403 to scripts), so VictoryGreymon, Jyarimon and the other Re:Digitize forms have names but no asset ids in the database.
- Next Order models: animation clips were not checked; 9 Set 2 lines use one.
- `js/data.js` changed several times during this work (66 > 60 shop lines, lines swapped for Cyber Sleuth-family ones). Set 1 reflects data.js as saved at 11:27 on 2026-10-15; re-run the Set 1 build after the roster work settles.

## Other uses for the batch

The Cyber Sleuth-family batch also brings humans (Cyber Sleuth's Takumi, Nokia, Arata, Yuuko and others; Digimon Masters Tamers are out of family) that could be tactician or avatar options next to the Little Legends, and Links decor (statues of Agumon & Gabumon or MetalGarurumon & WarGreymon, Christmas trees, vending machines, train props) for boards. Cyber Sleuth "Eater" models could be PvE monsters for a Cyber Sleuth-themed board.
