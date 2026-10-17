# Digi-Tactics native BRUTAL port

This development workbench is not yet a replacement for v1.0.14.
It is a native C# application using BRUTAL GLFW, OpenGL and ImGui. No Electron,
WebView, JavaScript interpreter or browser runs inside it.

## Build and run

Windows x64, .NET SDK 10.0.402, Node.js and Python with Pillow for the build-time content exporter,
and an OpenGL 4.5 capable graphics driver are required.

From the repository root:

```powershell
python -m pip install -r native/tools/requirements.txt
./native/build.ps1 -Publish
./native/artifacts/win-x64/DigiTactics.Desktop.exe
```

The publish output is self-contained and does not require an installed .NET runtime.
`build.ps1` checks the SDK installed at `%LOCALAPPDATA%\DigiTacticsBuild\dotnet`
before using `dotnet` from PATH. Package versions and transitive dependencies are locked.
For manual development, run dotnet commands from `native/` so `global.json` applies.

The sandbox starts with 50 gold. Buy units, refresh the shop, combine copies,
sell units, reset the sandbox, and select any of the 22 ruleset/pace catalogs.
Selling uses the original star/tier discount rules, including Dragonlands price premiums.
Below the shop, an animated native model preview shows the most recently owned unit
(or the first shop offer when the roster is empty). It uses authored or fallback idle clips,
GPU skinning and original textures with provisional preview lighting.
Attack, Special, Victory and Idle buttons exercise the native action controller.
It preserves action priorities, attack retrigger limits, clip speed limits and
weighted transitions, checked against the original renderer/mixer in 2,399,704
comparisons across 873 animated model sources.
Use **Test battle** to place the purchased roster into a native combat view; an empty
roster starts a six-unit demonstration. The view has a hex board, animated models,
health bars, cast labels, pause/resume and a return-to-shop button. Hover over a
health bar to see the unit name. This still does not run a complete match.

The shared combat presentation stream is checked against 156 original-engine battles
(42,626 ordered cues), including cast names, projectiles, bursts, critical damage,
leap origins and shared item effects. Fates and Faction Wars add 305 production
battles and 87,541 ordered cues, including traits, items, augments, revival, summon
labels and Duos effects. Auto Chess and Gizmos add 730 battles and 295,415 cues
covering transforms, mutations, summons, relics, defensive effects and Duos state.
Reckoning adds 392 battles and 128,714 cues, including hook pulls, possession,
revival, PvE scaling, shadow/radiant item effects and Duos. Dragonlands adds 374
battles and 137,414 cues covering all Copycat outcomes, traits, items, executes,
Mirage and Duos. Arcane adds 673 battles and 271,670 cues across abilities,
traits, contraband, augments and anomalies, including mana-boundary and lone
Devour cases. Coliseum adds 672 battles and 283,616 cues across abilities,
forms, Digi-Cores, traits and items. Galaxies adds 442 battles and 104,242 cues
across abilities, trait strikes, DigiXros ejection, items and Galaxy modifiers.
Underlords and Knockout add 1,898 battles and 1,333,172 cues across abilities,
hero passives, alliances, items and all eight Tamer builds at every rank. The
simulation now retains movement origins, elapsed time and durations, checked in
all 5,699 gameplay reference battles. A separate presentation module projects
world positions, target-facing angles, viewer mirroring and death visibility;
282,124 checks compare it with the original renderer across 22 scene traces.
The asset exporter decodes meshopt at build time and packs 718 embedded GLB models
with embedded RGBA8 PNG textures (about 295 MB), preserving skins, animations,
per-model options and credits.
All 5,232 unit forms resolve through the original model mappings. Native loading is
checked against 992,532 sampled Three.js accessor values, and the packaged executable
verifies every model payload and rig. Native skeletal sampling now handles node
hierarchies, linear/step tracks, quaternion interpolation and skin matrices, with
2,609,044 reference/repair checks across 718 rigs. The build converts 1,142 textures
with Pillow; the C# PNG decoder matches every decoded pixel hash, including alpha.
Python and Pillow are build dependencies only. Material decoding now matches 1,200
materials from the original loader, including the game's cutout/unlit corrections,
texture color spaces, UV channels, filtering and wrapping (26,052 checks).
The packaged GPU smoke uploads all 1,142 textures, checks exact pixel readback,
sampler settings, storage color formats and complete mip chains through BRUTAL OpenGL.
Native mesh loading assembles all 7,850,058 vertices across 718 assets; 74,844 checks
compare sampled geometry, normalized skin weights and both precise and template
bounding boxes with Three.js.
The workbench draws models with shared vertex/index buffers and GPU bone palettes;
packaged smoke verifies visible model pixels as well as UI purchase behavior.
The build also compiles the original motion preparation for all 905 model sources:
459 fallback clips, adjusted rest poses and six weapon-parenting changes. Native
sampling passes 1,266,912 reference checks against those prepared scenes. The 667
deduplicated motion payloads add about 1.6 MB; no JavaScript runs in the app.
Combat choreography now consumes real simulation cues in renderer order, drives
model actions and exposes facing, lunge/recoil, hit flash, cast highlight/label and
death transforms. It passes 610,546 original-renderer checks across 44 battle traces;
the packaged app also checks real model playback through battles in all 22 catalogs.
Battle drawing shares model geometry/textures across unit instances and applies
the original stage heights, footprint limits, source rotations and flyer offsets.
Production shaders, arena themes/lighting, particles/audio and production screens
remain unfinished.

## Boundaries

- `src/DigiTactics.Assets`: model catalog, integrity checks and embedded GLB reading.
  Meshopt decoding stays in the build tools; the native loader reads quantized
  geometry, skin and animation accessors without executing JavaScript.
- `src/DigiTactics.Presentation`: arena geometry and per-battle visible unit state.
  Reads the simulation and consumes presentation cues without changing gameplay;
  contains no graphics or window APIs.
- `src/DigiTactics.Rules`: deterministic C# rules. No graphics, filesystem, clocks,
  networking, BRUTAL native calls, or global active-ruleset state in simulation objects.
  Content loading is a separate boundary. Match instances own their rules, random
  generator and shared pool; player rosters own shop exclusions.
- `src/DigiTactics.Desktop`: BRUTAL lifetime, native input, rendering and screens.
  Views issue commands to the rules layer. They cannot mutate its inventory directly.
- `tools/export-content.cjs`: build-time adapter to the legacy catalog and reference
  engine. Node is a development dependency only. This exporter deliberately extracts
  shop definitions and combat templates; it does not migrate executable set hooks.
- `tests`: standalone C# regression runner and checked-in reference traces from the
  shipping JavaScript engine. Exceptions fail the process and build.
- `content`: schema-versioned generated catalog. Edit the exporter or canonical
  content source, then regenerate; do not hand-edit this file.
- `artifacts`: ignored build output and renderer capture.

The renderer currently uses BRUTAL's supplied OpenGL ImGui backend for the migration
workbench. The final 3D game renderer is a separate workstream; this does not claim
to be a Vulkan renderer or the final game UI.

## Verified behavior

- Native BRUTAL window, input, UI and GPU draw loop.
- 4,000 exact seeded RNG comparisons with the shipping engine.
- 8,800 exact shop-roll comparisons across all 22 ruleset/pace catalogs.
- Shared-pool weighting, tier fallback, empty eligible pools and player-specific
  three-star exclusions; two-copy and three-copy evolution chains.
- Atomic sandbox purchases, bench capacity, repeated purchases and invalid slots.
- 5,232 unit-form reference cases for stats, ability scaling and selling prices.
- 1,200 damage reference cases covering mitigation, shields, attribute advantages,
  amplifier ordering, burn mana suppression and hook-supplied modifiers.
  Production hook coverage is tracked below and in `MIGRATION.md`.
- 88 shared-kernel reference battles exercise all 14 shared abilities, all star
  levels, Solo/Duos widths, empty sides, overlapping placements and time limits.
  Equipment is disabled for those isolated comparisons. Another 116 battles cover
  every shared completed item at one and three stars and mixed loadouts across
  every set/pace, including generated Thief Glove items. All 204 battles disable
  set/item hooks; these must still be ported and verified.
- Two hook-enabled reference battles verify all 23 callback types, priority/source
  ordering, Duos ownership, nested callbacks, timers, temporary buffs and damage-stage
  mutations. Controlled hook definitions exercise the real legacy dispatcher; native
  production binding is being migrated separately.
- Faction Wars: 84 production-hook fights cover all 29 traits at all 70 breakpoints,
  12 combat augments, Crest and counting edge cases (duplicate lines, repeated
  emblems and bonus traits). These compare the actual production hooks with native
  implementations. Other sets, powers and planning effects remain pending.
- Eighteen production Duos fights verify partner-owned combat augments, temporary
  buffs and combined trait counts, including Crest Link, Sync Bond, Partner's Ace
  and Tag Team. Shared team health, portal commands and round flow remain pending.
- 15,680 Solo/Duos hex-distance comparisons and 200 movement/target-selection
  traces, including occupied cells and deterministic tie-breaking.
- Non-shop combat definitions, runtime stage growth, fractional ranges and summons
  with owner/equipment/stat multipliers. Hook probes verify same-tick summon updates,
  rejected-spawn RNG behavior and board movement against the original engine.
- Fates: 105 reference fights cover seven custom abilities, 28 traits/all 62
  breakpoints, both Fallen paths and Partner combat modifiers. Partner shop offers and planning effects remain pending.
- Fates held items: all 21 hook definitions, duplicate-item deduplication, artifact
  effects and summoned holders are exercised in 56 additional reference battles.
  Post-combat gold settlement is still part of the pending match-flow port.
- Fates combat augments and Partner count modifiers: 48 additional Solo/Duos
  reference fights cover ordering, owner scoping, items and trait interactions.
  Augment choices, grants and planning/economy behavior remain pending.
- Galaxies: 312 reference battles exercise all 52 custom abilities and their timed
  mechanic effects at every star level on both board widths. Another 48 battles
  verify all 26 combat traits and their breakpoints, including DigiXros fusion and
  pilot ejection. All 17 held-item hook definitions, nine combat augments, Stars
  counting and shared Duos augments are covered by 82 additional reference fights.
  Consumables, grants, rewards and planning rules remain pending.
- Gizmos: four custom abilities, Colossus immunity, spell wards and all 22
  standard/overclocked held-item hook definitions, verified by 76 reference battles.
  All 27 traits and 63 breakpoints, Mutation variants, Junkyard conversions,
  Innovator summons and Glutton combat inputs pass 154 more reference battles.
  All 37 combat upgrades, Injection and Booster/Burst counting pass another 160
  Solo/Duos battles, including temporary items and Woodland copies.
  Upgrade picks/grants, feeding commands and planning rules remain pending.
- Reckoning: four custom abilities across five casters, both summon partners,
  possession decay, Respite and PvE scaling pass 30 additional reference battles.
  All 27 traits and 61 breakpoints pass 130 more reference battles, including
  Sentinel, Chimera inheritance, revival and Royal Rival activation/deactivation.
  All 60 held-item hook definitions and X-Antibody pass 192 further reference
  fights with duplicate/mixed equipment, inherited items and Shadow drawbacks.
  All ten combat augments and shared Duos augments pass forty more fights;
  a dedicated test covers item listeners registered by summons during a cast.
  Offer grants and planning/economy rules remain pending.
- Dragonlands: Copycat's five outcomes and all 20 held-item hook definitions pass
  86 reference battles, including Golden Armor bank bonuses and reward counters.
  All 27 traits and 72 breakpoints pass 168 more fights, including Mirage variants,
  Jade statues, Mage recasts, Starcaller, Dragon marks/limits and Monolith positions.
  All 19 combat augments and mixed combinations pass 120 more Solo/Duos fights,
  including retained training kills and partner-owned state. Pick grants and
  planning/settlement rules remain pending.
- Arcane: four custom abilities across seven casters, all 26 traits at 69
  breakpoints, and 100 targeted Solo/Duos interactions pass 322 new reference
  battles. Coverage includes every rotating/gamble outcome, laboratory combinations,
  Envoy activation gaps, Belphemon, workshop parts and repeated component upgrades.
  All 29 item-hook definitions and ten combat augments pass 159 further fights,
  including contraband eligibility/stacking, bounty rewards, delayed procs and
  owner-scoped Duos augments. All 25 combat Anomalies and utility-identity kill
  accounting pass 192 further fights, including blocked summons, Devour ordering,
  revives, threshold effects and Fireball's mana boundary. Anomaly offers, utility
  pick grants, persistence and planning/settlement remain pending.
- Coliseum: six custom abilities across nine casters, six form choices and all
  24 Digi-Core paths pass 240 further reference battles. These include infection
  spread/expiration after caster death, All Out recovery, mixed cores and both
  board widths. All 26 traits at 75 breakpoints pass 150 more fights, with 60
  targeted cases for Sensei subsets, stance priority, plant regrowth, Supreme
  selection and crowded Mech spawning. All 14 item-hook definitions, ten combat
  augments and Arena's trait modifier pass 222 more Solo/Duos fights, including
  duplicate equipment, cores, summons and partner ownership. Offers, consumables,
  pick grants and planning/settlement remain pending.
- Underlords/Knockout: all 50 active custom casts, passive identity, hero kits
  and core timed/death mechanics are implemented. Alongside 348 isolated cast
  tests, 744 battles cover every shop hero at every star stage in both modes and
  board widths with mechanics enabled. Summons, poison, leech transfer, Grave,
  Eclipse, Refraction and combat reward counters are included. All 26 alliances
  at 50 breakpoints pass 200 further battles; 36 Duos cases exercise merged counts,
  Legend membership, Phantom's Rogue inheritance, Shaman summons and Spirit's
  Delta Strike. All 39 held-item hooks and four global combat effects pass 378
  further battles, including duplicate items, Meepo inheritance, summoned observer
  hooks, reflection, item summons and owner-scoped globals. Other globals retain
  their combat no-op behavior. Item grants/transfers, global economy/settlement,
  Tamer selection/rank progression and full match planning remain pending.
  All eight Tamer combat builds pass 192 further battles across ranks 1-6, both
  modes and Solo/Duos, including enthrallment, summons, Yoink, deferred damage,
  poison and escape thresholds. Schema 14 carries authored Tamer stats and item
  tier/hat metadata.
- Auto Chess: three custom ability types across ten casters, all 25 traits at 51
  breakpoints, ten item hooks, two combat relics and the Druid strength gate pass
  340 additional battles. Cases cover Wizard/Demon/Olympos count rules, summon
  limits, transformation modifiers, duplicate items and Solo/Duos ownership.
  Schema 15 exports summon counts and transformation data. Loot, Digi-Eggs, relic
  grants/economy, Priest loss reduction and full-match progression remain pending.
- Full combat traces are split into content-addressed gzip shards. A manifest
  verifies hashes and battle counts; the runner still reads one battle at a time
  and comparisons retain all recorded samples.
- Headless native renderer smoke mode and self-contained Windows packaging.

## Remaining migration work

1. Full match state: board placement, economy, rounds, all set hooks, AI, carousel,
   items, augments, rewards, and Duos transfers. Keep shop parity tests as the lower
   layer; add complete command-log fixtures before porting each behavior.
2. Complete combat simulation: hook-driven item effects, full timed effects, custom abilities,
   traits and deterministic replay hashes. Compare identical seeds and command logs,
   including every set-specific hook, against the original engine.
3. Native 3D pipeline: glTF import, materials, skeletal animation, cameras, lighting,
   board assets, effects and model/texture validation. Preserve asset attribution.
4. Production UI: home, lobby, match controls, collection, loot, farm, Codex, settings,
   audio, accessibility and scalable input/layout. The debug sandbox is not final UI.
5. Platform: Steam identity, lobbies, reliable transport, reconnect/host handover,
   updater, crash recovery, installer and diagnostics. Give these explicit adapters;
   keep network I/O outside the deterministic simulation.
6. Profile migration: versioned import from the existing Electron profile, preserving
   cosmetics, progress, match history and settings. Never overwrite the original;
   validate a copy and make rollback possible before switching the production app.
7. Acceptance: full gameplay parity, native visual regression checks, live two-PC Steam
   tests, profile migration tests and measured frame-time/allocation budgets before a
   production BRUTAL release replaces the existing Windows build.

Do not label a milestone as the complete port or point the production updater at it.
The existing JavaScript application remains the reference implementation during migration.

BRUTAL is Copyright (c) 2024 Ahwoo Limited. Distributed builds preserve BRUTAL's
NOTICE and package licenses. Digi-Tactics remains unofficial, non-commercial fan work;
the existing project credits and third-party notices still apply.

## Match economy foundation

Native player economy now implements starting balances, XP purchases and caps,
round XP, stage-based leveling, base income, interest, streaks and win rewards.
Income exposes an explicit hook boundary for future set and augment integration.
Declarative round schedules preserve opening stages, recurring PvE/carousel rounds
and Reckoning's extra carousel without a finite stage limit. Export refuses unknown
income/schedule formulas instead of silently approximating them.

The original game generates 300 economy transitions and 200 stages per variant;
99,466 assertions cover all 22 mode/pace combinations. These components are not yet
wired into the desktop workbench or a complete match coordinator. Planning, shared
pool ownership, set hooks, AI, rewards, settlement and multiplayer remain pending.

## Planning ownership and shared shops

`PlanningRoster` now preserves ordered board positions, nine bench slots, swaps,
slot-cap enforcement, team-size items, combat board locks, merge anchors, item
priority/overflow and inherited unit modifiers. `PlayerShop` connects this roster
and player economy to a shared `ShopPool`, including paid/free rerolls, locked
shops, purchase capacity, selling and personal three-star offer removal.
Reference fixtures exercise 15,400 planning commands and 8,800 two-player shop
commands across all 22 variants. Set-specific planning/shop hooks are deliberately
disabled in these reference traces; hook implementations, special offers and the
full match coordinator still need integration. The desktop still uses its sandbox.

## Match state, pairing and elimination

`MatchState` owns player economies, ordered rosters and shops sharing one seeded
pool. Solo and Duos pairing preserves human rematch avoidance, ghost opponents,
custom team validation and stable team member order. Team health synchronizes
individual hook deltas, applies Team Guard rounding, and eliminates partners
together. Eliminations preserve tie ordering, placements, spectating behavior and
return owned units, offers and reserved unit drops to the pool.

`MatchLossRules` preserves stage damage, surviving-unit damage, PvE overrides and
Duos scaling for every mode, including unbounded Underlords stage progression.
Original-engine fixtures cover 132 seeded match-state sequences and every unit
form's loss values at selected stages including 1000 (796,076 comparisons). Match settlement
hooks, rewards, round orchestration, offers, AI and desktop integration remain
unfinished; this does not yet establish playable full-match parity.

## Settlement and loot

`MatchSettlement` connects resolved native battles to gold, streaks, opponents,
Solo health, shared Duos damage, post-combat merges and elimination. Required
reward/round-end callbacks preserve ordering and partner-owned unit views; each
settlement instance rejects a second application. Native combat objects remain
available to mechanic hooks through the resolved battle and unit records.

`MatchLoot` implements generic PvE drops, explicit custom drop specs, shared IDs
and RNG, unit reservations, orb positions, AI auto-collection, item-cap handling,
bench overflow refunds and three-star offer cleanup. Modes with custom PvE rules
use native mode reward handlers (see the verification below). MatchPlayer
now stores complete loot orbs instead of only reserved unit names.

Reference checks cover 396 settlement rounds with instrumented hook effects
(88,754 comparisons), plus 44 generic/custom-spec loot traces (275,848 comparisons).
These verify orchestration and base loot behavior, not the remaining per-set game
hooks, offers, AI or full desktop match integration.

## Native mode reward policies

All current mode-specific PvE reward generators now run in C#: Fates artifacts,
Arcane artifacts and consumables, Gizmos Overclock Kits, Reckoning shadow drops,
Dragonlands treasure outcomes and preferred emblems, Underlords round-scaled gold,
and Auto Chess tiered choices with pace-adjusted rounds. Auto Chess choices and
Treasure Dragon outcomes are preserved as typed player mechanic state for later
planning offers. Generic modes retain the base generator.

132 original-handler traces exercise 15,840 reward calls across all 22 variants,
three seeds, AI/human collection, occupied boards and full/available benches.
2,884,584 comparisons verify rewards, orbs, inventories, shops, pool reservations,
mechanic state and RNG consumption. Auto Chess reward selection is implemented in the offer engine; its UI, the
Treasure Dragon offer and other concrete planning hooks remain pending. The desktop still uses its migration workbench.

## Offers and round transitions

`MatchOffers` implements schedule precedence, lobby-wide augment tiers, owned and
rerolled exclusions, the Duos augment slot, power/item-tier selection, generic
armories and choice side effects. It consumes Auto Chess loot choices directly.
Custom armories, custom offers, augment effects and power effects dispatch through
required hook interfaces; their remaining concrete handlers are not replaced by
silent defaults.

`MatchRounds` wires round progression, per-player XP/income, locked shops, offer
creation, power ranks, round-start callbacks, AI/pairing or carousel dispatch,
shared health synchronization and settlement phase transitions. Reference traces
verify 530,022 offer checks and 559,864 round-flow checks across all 22 variants
in Solo/Duos. These traces instrument game hooks and AI/carousel callbacks to test
ordering; they do not establish completion of those concrete systems or desktop
match integration.

## Custom armories and Treasure Dragon

The native offer engine now runs Arcane's artifact armory and Reckoning's shadow
armory directly. Treasure Dragon offers support classic/order/chaos variants,
won/lost treasure outcomes, Treasure Hoard option counts, duplicate/exclusion
retries, preferred emblems, and every bundle reward. Unit bundles preserve pool
reservations, bench/full-pool gold fallbacks, merges and max-star shop cleanup.
Preferred-emblem selection is shared with the PvE reward generator.

36 original-handler traces cover 4,320 generation/selection cycles across both
paces, three seeds and full/available benches (285,156 comparisons). General
offer and round-flow traces now execute the real Treasure Dragon handler too.
Other custom offer kinds, concrete game-start/round hooks (including Treasure
variant initialization), item editing, AI, carousel and desktop match integration
remain pending.

## Planning items and Reckoning upgrades

`PlanningItems` now equips and combines items using the complete exported recipe
map, preserves rejection order and combat board locks, handles globals and
inventory swaps, and enforces duplicate-trait, no-item and Underlords slot rules.
Consumables use native transformations where implemented and delegate remaining
mechanics through a required handler. Roster updates retain unit IDs and ordered positions.

Reckoning's Radiant Blessing, Radiant Armory and Tome now use native options and
choice effects. Radiant options prefer held base items; upgrades replace the first
matching copy on the board, then unit bench, then item bench, with inventory
fallback. Existing offer/round traces now exercise these real handlers.

39,600 item-command scenarios plus component recipe pairs produce 239,726 checks
across all 22 variants. Six original-handler Radiant/Tome traces add 6,486 checks
for selection, exclusions, upgrade order, inventory limits and RNG. The item trace
instruments consumable/veto callbacks to verify dispatch, not consumable effects.
Other custom offers, concrete match hooks and desktop integration remain pending.

## Inventory consumable transformations

`ModeItemConsumables` implements every mode's remover and reforger, plus the
Gizmos Overclock Kit. The match-backed `PlanningItems` constructor runs these
handlers before delegating remaining consumables. Inventory capacity is checked
before consumption; item returns retain order. Reforging preserves each mode's
component, emblem, artifact, radiant, contraband-tier and special-item rules,
including random-stream advancement and empty candidate lists.

48 traces execute the original consumable handlers with three seeds across both
paces of all eight affected sets. The 132,768 checks cover all eligible held item
IDs, empty equipment, multiple equipped items, full inventories, result codes,
returned items and RNG. Export also checks that Gizmos radiant flags still match
the catalog's radiant category. Other mode mechanics and complete desktop match
integration remain pending.

## Unit-copy consumables

The native inventory command now implements Gizmos Copy Data, Galaxies Copier,
Fates Copy Data, Dragonlands standard/lesser duplicators, Arcane standard/lesser
copiers and the Auto Chess Digi-Egg. Copies reserve the correct number of pool
units, preserve each mode's rejection order and tier limits, and run ordered merge
callbacks before clearing personal max-star shop offers. Auto Chess copies the
target's current form; other copiers create a base form. Galaxies retains its ID
allocation even when a full-bench copy is rejected.

12 reference traces exercise original equip/use/combine methods across both paces
of all six affected sets, producing 1,786,872 checks. They cover every shop and
non-shop line, stage limits, exhausted pools, full benches, combat board locks,
chained merges, item overflow, pool and shop state, merge callback IDs and source
order, next-unit IDs and RNG. The original Galaxies/Auto Chess full-bench test
counts locked board units even though merging excludes them; native behavior
retains the resulting discarded-extra/pool-reservation behavior. Mode-specific
combine callback effects still require the concrete match hook implementation.
Remaining custom offers and full desktop match integration are pending.

## Shop, path and offer consumables

Loaded Dice now rebuilds the shop using native trait matching, personal max-star
exclusions, its exact tier fallback order and uniform eligible-line selection.
Availability is checked before returning existing shop reservations. Fates Holy
and Dark path items validate Lopmon and existing choices, consume the opposing
choice item, and retain the selected path in typed player mechanics.

Reckoning's Tome validates planning phase and existing offers before creating four
shuffled emblem choices. The Coliseum extractor removes only the power-up modifier,
preserves unrelated modifiers, and returns a core (or stores it when the item bench
is full). Equipment commands now receive the actual match phase and derive human
board locking from it; AI phase restrictions are no longer conflated with board
locking.

Six original-handler traces produce 490,320 comparisons across both paces. Cases
include human/AI players, board/bench targets, all phases, busy offers, item-cap
boundaries, existing path choices, empty and preserved modifiers, pool exhaustion,
low/high-tier fallback, personal max-star exclusion, shop reservations and RNG.
Other custom offers still require implementation. Full match mechanics, shop metadata and desktop integration remain
pending.

## Coliseum Digi-Core choices

Catalog schema 23 exports ordered Coliseum power-up tags and per-unit choice
categories. `ColiseumPlanning` owns core use, targeted options, selection and
refunds, and shares core-return logic with the extractor. Core use requires
planning, no existing offer and a shop unit. It records the unit ID and opens
three choices with one reroll. Selection preserves other modifiers, clears the
pending target and applies the chosen power-up; a missing target or unknown
power-up returns the core, storing it when the inventory is full.

Two original-handler traces across both paces produce 219,868 checks over every
Coliseum line. They verify phase and board locks, human/AI use, busy offers,
existing-power exclusions, reroll exclusions and limits, target removal before
and after rerolling, preserved modifiers, choice rejection, refunds and RNG.
The general offer and round-flow exporters now use the original core handlers.

Every catalogued consumable now has a native handler. This does not complete the
match layer: other custom offers, concrete game
hooks, shop metadata and the production desktop match flow remain pending.

## Coliseum form and Jewel Gambit choices

The native offer engine now handles all three Coliseum offer kinds. Form choices
resolve the pending unit, clear its target ID, preserve unrelated modifiers and
replace bonus traits only when the selected form supplies a trait. Missing units
and invalid forms retain the original rejection behavior.

Jewel Gambit uses typed player state for gems, trait level, combat count, readiness
and Double Down. Options use the original rounded gem labels. Cash-out reads the
current state rather than the label, applies the level-three multiplier, rounds
gold as the original does and rolls component/completed-item rewards at the
25/45/80 thresholds, even when inventory capacity prevents accepting them. Double
Down preserves gems and resets its combat count and readiness.

Two reference traces provide 332,112 checks across both paces, using a controlled
offer schedule to exercise the real original and native offer engines. Coverage
includes all Coliseum unit lines, invalid and removed targets, trait replacement,
modifier preservation, rerolls, fractional thresholds, full inventories, invalid
indices, direct handler fallbacks, complete state after choices and RNG. Original
Coliseum form/gambit handlers also replace mocks in the general offer exporters.
The round/combat hooks that create and evolve these choices, broader AI behavior,
other mode offers and production match integration remain pending.

## Coliseum roster hooks

`PlanningShopCommands` now routes match purchases, sales and purchase-triggered
merges through required `IMatchShopHooks`. Purchase hooks run before bench placement
and can return a modified unit. Sale hooks run after gold, item and pool returns.
Merge results now include hook-updated unit records.

`ColiseumPlanning` implements these hooks. Human purchases open form choices only
during planning when no offer is active. AI purchases score each form against the
current board's trait counts and breakpoints, with Labramon's front-line preference.
Catalog schema 24 exports that front-line classification. Merges preserve the first
truthy power-up and form, discard unrelated modifiers, restore stance traits and
refund extra cores. Sales return a held core, including inventory-full storage.
The Arena count modifier now skips unknown bonus-trait keys like the original.

Eight 1,200-command reference traces use the original shop, offer, movement and
mechanic handlers for human/AI players in both paces. Their 1,814,372 checks cover
purchases, refusals, phases, form selection, swaps, merges, item overflow, sales,
core refunds/storage, ordered rosters, pending offers, pool reservations and RNG.
The match command layer is ready for a composed hook dispatcher; low-level shop
methods remain available for isolated economy fixtures. Round/combat hooks, other
mode shop mechanics, the global dispatcher and production desktop integration are
still pending.

## Coliseum round-start rewards and offers

Catalog schema 25 exports each pace's core-grant rounds. `ColiseumPlanning.RoundStart`
queues those cores, drains stored cores as inventory space permits, grants one
reroll per fielded three-star Crew unit and rolls Pirate Captain rewards in the
original order. Typed pirate state retains the growing reward count, last reward
and permanent AP. `MatchPlayer.NextBuffs` stores the armor reward while preserving
other pending combat buffs.

A ready Jewel Gambit takes priority over form offers for human players. AI chooses
its Gambit option using health, streak and the shared random stream, then resolves
all missing forms in roster order. Human form choices remain queued one at a time,
and existing scheduled offers block them without suppressing round rewards.

12 original `nextRound` traces cover 1,800 advances and 175,911 comparisons across
both paces, human/AI players and three seeds. The native test drives `MatchRounds`
through the concrete round-start handler and compares economy, shops, inventory,
rerolls, mechanics, offers, modifiers, bonuses and RNG. The reference and native
fixtures both omit general AI planning and carousel actions; these tests establish
the round-start behavior, not complete-match parity. Combat settlement mechanics,
AI core assignment, the global hook dispatcher and production integration remain
pending.

## Coliseum post-combat progression

`ColiseumPlanning.RoundEnd` consumes the settlement context's player-specific unit
view. Supreme Virus remembers the first highest-damage eligible unit, including
in PvE. Jewel Gambit counts only dead, non-summoned enemies, applies stage and
Double Down multipliers, grants the single per-round kill-threshold reward and
preserves the original automatic half-value cash-out on a Double Down win.
Champ fans accumulate on wins and heal up to the stage's base loss damage on a
loss. Missing combat data produces no changes; real combat contexts must retain
unit state so traits and damage are not silently omitted.

24 completed reference/native battles, including Lotosmon's summoned plants,
feed 5,760 controlled post-combat contexts. The 36,174 checks validate terminal
battle results, traits and damage, then compare gold, health, inventory, Supreme
selection, fans, Gambit state, kill progress and RNG. Contexts vary declared
win/loss/draw results, trait counts, stages, PvE, full inventory, missing combat
and the original Duos owner filtering, including ownerless units. These contexts
exercise the hook; complete-match orchestration remains a separate requirement.
The composed hook dispatcher, other mode mechanics and
production match integration remain pending.

## AI item holders and Coliseum core assignment

Catalog schema 26 exports shared unit AI hints (carry, tank, damage, placement and
value). Coliseum's duplicate choice-category/front-line tables are removed; its
core and form logic now reads those hints. `PlanningAi.Holders` ranks the board
stably by unit value and equipment, prioritizes completed carry builds, selects
up to two carries and applies the original tank priorities. A unit may occur as
both carry and tank, as in the reference.

`ColiseumPlanning.AiTurn` assigns at most four cores. It recomputes holders each
time, skips existing power-ups, falls back to the highest-value eligible board
unit and preserves unrelated modifiers. Selection uses the original shuffled
choice list and shared match RNG without opening a human offer.

19,800 board/bench scenarios across all 22 variants provide 858,884 reference
comparisons. They exercise line/unit no-item flags, stable ties, component versus
completed equipment, overlapping holder roles, existing modifiers, core counts,
fallback selection and the four-assignment bound. Existing core/form/shop suites
also verify the shared-hint migration. General AI shopping,
positioning, the composed hook dispatcher and full desktop match integration are
still pending.

## AI crafting and equipment

Catalog schema 27 exports each mode's item role preferences. `AiInventory.Assign`
ports the five original passes: finished items and globals, preferred recipes,
special-component recipes and emblems, leftover components, and the final pass
for modes with equip hooks. It preserves holder order, trait thresholds, recipe
order, recursive special recipes, inventory limits and Underlords restrictions.
Because native roster records are immutable, each equipment decision resolves
the holder's current record before inspecting its items or traits.

`PlanningItems` now provides shared recipe lookup, crafting and direct bench-item
assignment. AI and future command integration can use these native operations
without duplicating ingredient removal or equipment eligibility.

26,400 original-game scenarios across all 22 variants provide 791,222 comparisons
of recipe indices, inventory, globals, equipment, craft/give results and RNG.
The full suite passes 139,309,200 checks; the self-contained Windows package also
passes the BRUTAL asset, texture, UI and battle-rendering smoke checks. General AI
shopping, positioning, the composed hook dispatcher and production match
integration remain pending.

## AI roster preferences and placement search

`PlanningAi` now implements purchase preferences, weakest sellable bench selection,
trait counts for team selection and ordered free-cell search. Purchase preferences
include owned copies, trait breakpoints, expensive units, explicit line values
and partner demand. Bench ranking protects pairs and equipped units, preserving
stable ties and unit/line sale restrictions. Team-selection counts add a living
partner's board traits; they intentionally omit augments, as the original AI does.
Placement search preserves front/back/flank ordering and requires the caller's
placement rule, so mode restrictions cannot be silently skipped.

19,800 generated roster scenarios across all 22 variants provide 3,178,800 original
comparisons. They vary capacity, gold, duplicate forms, bonus traits, items,
unsellable units, partner availability/demand and placement vetoes. Partner portal
availability and capacity are explicit inputs here; portal orchestration, purchase
execution, trait scoring, whole-board arrangement and full AI turns remain pending.
The full native suite passes 142,488,000 checks. The packaged Windows application
passes the BRUTAL rendering and purchase smoke.

## AI trait scoring and roster arrangement

`PlanningAi.TraitValue` and `Score` implement breakpoint progress, active-trait
bonuses, stage-dependent preferences and every current mode-specific trait bonus.
This includes Reckoning rivals and Auto Chess Demon, Olympos, Druid and Wizard
rules. Trait definitions are cached by the planner rather than rebuilt per score.

`Arrange` rebuilds board and bench in original stable order, prioritizes team-size
item holders, accounts for multi-slot units, places front-line units first and
preserves unsellable units on the bench before selling overflow. Overflow refunds
gold and items without normal sale hooks, and requires a pool-return callback.
The caller supplies base capacity, stage, partner and placement validation.
Detached board placement stays internal to the rules assembly and validates cell
occupancy and unit uniqueness.

The expanded 19,800-scenario AI suite passes 5,213,392 comparisons across all 22
variants. In addition to earlier purchase and placement checks, it compares every
line's trait score, owned-unit scores, ordered final board and bench, refund gold,
returned items and ordered pool-return requests. Full AI turn orchestration,
leveling, portal transfers, purchase execution and desktop integration remain
pending; these APIs do not claim a complete playable match.

Validation: the full native suite passes 144,522,592 checks, including 5,699
reference battles. The self-contained Windows package passes the BRUTAL asset,
texture, purchase UI and battle-rendering smoke tests.

## AI leveling and shopping execution

`AiShopping` now buys XP toward the standard stage/round targets or Hyper's
stage-based target, preserving the 20-purchase guard and maximum level. Shopping
runs an initial buy pass, then at most 15 rerolls while free rolls or the original
gold reserve permit. A full bench triggers one weakest-unit sale and purchase
retry. Bench cleanup stops at six units or when no sellable unit remains.

Purchases, combinations and sales use `PlanningShopCommands` and its required
mechanic hooks. Capacity and eligible partner demand are evaluated live. Reroll
execution is a required callback so mode-specific shop refresh can be composed
with the shared AI loop. Offer choice and on-AI-turn hooks must precede shopping;
portal transfers must precede bench cleanup, followed by arrangement, equipment
and final arrangement. The production dispatcher still needs those connections.

72 original-game traces across all 22 variants provide 669,795 passing checks for
gold, XP, shop reservations, free/paid rerolls, ordered benches, pool counts and
RNG. Base traces isolate generic shop behavior; six additional Coliseum traces
use its concrete purchase/combine/sale handlers and compare form modifiers and
core refunds. Other special shop metadata/pricing handlers remain pending.
The targeted suite runs with `--ai-shopping-only` and is included in the full
suite. This milestone does not claim full AI turn or production match completion.
