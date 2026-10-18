# Digi-Tactics native BRUTAL port

This development build now starts a playable native match screen. It is not yet
a production replacement for v1.0.14.
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

The default launch offers Solo or Duos against AI across all 22 ruleset/pace
variants. Buy units, select a bench unit and then a hex to place it, select an
item and then a holder to equip it, and use Ready for battle / Next round to
progress. Standings buttons scout players and switch the watched battle. Offers,
carousels, loot, Wisps and Duos transfers use the native action boundary.
This first playable screen still needs the remaining production presentation,
mode details, audio, networking, persistence and release work described below.

Use `--match-smoke` for the hidden click-through UI test (`--duos` selects Duos).
Use `--workbench` for the earlier shop/model diagnostic screen; `--smoke` continues
to run its asset/GPU/animation checks. The diagnostic sandbox starts with 50 gold. Buy units, refresh the shop, combine copies,
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

## Duos portal transfers and AI sends

`MatchPortal` implements ordered eligibility checks and transfers for bench units
and inventory items. Full recipient benches accept units that immediately merge;
board merges keep the original anchor and return excess equipment. Transfers
retain pool ownership and clear the recipient's max-star shop offers after merges.
`MatchPlayer` now owns send usage and incoming arrival records; `Reset` clears
both for a new round. The current Wide Portal send-limit bonus is recognized.

Required portal hooks dispatch combine, partner-combine, sender-portal and
recipient-portal events in original order. `PartnerCopies` gates AI demand on
partner availability and remaining sends. `AiTurn` prioritizes immediate merges,
then partner collections, then excess components, with stable ties and the
original four-send guard.

8,800 original-game scenarios across all 22 variants pass 962,840 comparisons.
These cover planning/combat/results and carousel restrictions, unsellable units,
invalid requests, full benches, board/bench merges, item capacity, send limits,
arrival payloads, ordered hook dispatch, shop exclusions, pool counts and RNG.
The fixture instruments hook dispatch rather than applying augment effects;
concrete Duos augment handlers, Care Package gifts, round-reset wiring and the
production command/UI dispatcher remain pending. Run with `--portals-only`.

## Duos planning augment effects and Care Package

`DuosPlanning` implements Wide Portal gold, Proud Partner's per-round three-reward
limit, Pass the Gear, Shared Interest, Care Package and Shared Treasure. Partner
rewards require a living partner and preserve original RNG consumption even when
inventory capacity discards an item. Proud Partner tracks the pace's cumulative
round number, including the opening stage's different length.

`MatchLoot.GiveUnit` reserves the original number of pool copies, allocates a unit
and either places it on the bench or returns its copies and grants sale gold.
`MatchPortal.Gift` records successful bench arrivals as gifts, combines through
recipient and partner hooks, and clears max-star shop offers. Gifts consume no
portal sends and do not fire ordinary portal-send hooks. Human combat board locks
remain respected during gift merges.

The concrete shared-augment handlers are exercised alongside portal transfers in
8,800 original-game scenarios across all 22 variants: 1,034,330 comparisons of
inventory, gold, income, Proud Partner counters, roster, pool, arrivals, callbacks,
shops and RNG. Mode combine hooks are isolated in these traces. The base portal
suite now also checks gift flags (969,554 comparisons). Run the new suite with
`--duos-planning-only`. Production hook composition and match/UI integration remain
pending; the existing native combat/trait/settlement layers handle the other Duos
combat, trait and damage effects.

Validation: 147,196,271 full-suite checks pass, along with the self-contained
Windows package asset, texture, purchase UI and battle-rendering smoke tests.

## AI standard offer selection

Catalog schema 28 exports augment and power tiers/tags under `OfferAi`.
`AiOffers.Select` implements armory, item-tier, augment and power ranking, including
carry-role preference, craftable components, consumable penalties, emblem
breakpoints, stage/tag weighting and the original random draw per option. It
preserves first-option ties and empty-list behavior.

All current generic-offer bonus functions are native: 19 augment bonuses across
Faction Wars, Fates, Galaxies, Gizmos, Arcane and Coliseum, plus the 12 Underlords
global-item bonuses shared with Knockout. Fates checks the owned Partner modifier;
Arcane checks unit traits on board and bench; Gizmos includes augment trait bonuses
while the other relevant board-trait scores omit them as the original does.

19,800 generated rosters across all 22 variants exercise 79,200 selections with
158,400 selection/RNG comparisons. Offers vary option count, board/bench contents,
equipment, augments, inventory, health, streak sign and stage. The reference runs
the original bonus functions. Run with `--ai-offers-only`. Custom mechanic offers
still require the explicit custom-score callback, and choice execution plus full
AI-turn/desktop integration remain pending.

Validation: 147,354,671 full-suite checks and packaged BRUTAL smoke tests pass
with schema 28, including the existing battle and rendering reference checks.

## AI custom offer scoring

Schema 29 exports item tags/ranks, power Tamer identities and Arcane anomaly tags.
The native `AiOffers.Select(player, offer, stage)` overload now scores every current
custom offer kind without a scoring callback: Gizmos upgrades; Reckoning blessing,
radiant and Tome choices; Dragonlands treasure; Arcane anomaly, market and junk;
Coliseum core, form and Gambit; Underlords Tamer/crew; Auto Chess loot/relics.

Scoring preserves Coliseum's second RNG draw per option, Radiant base-item
ownership, Digi-Egg merge and pool eligibility, stage/gold thresholds, and Arcane's
human-center versus AI-holder anomaly targeting. Target fallback uses the original
stable value/item/UID ordering and skips already anomalous units.

44 reference traces cover human and AI rosters in all 22 variants, comparing
61,200 custom selections and subsequent RNG states (122,400 checks). The reference
uses actual custom offer scorers, with option lists drawn from their content.
Run with `--ai-custom-offers-only`. Scoring does not execute the chosen effect:
remaining custom offer handlers and the full AI-turn/production dispatcher still
need integration.

Validation: 147,477,071 full-suite checks and packaged BRUTAL smoke tests pass
with schema 29.

## Auto Chess relic and Gizmos upgrade offers

Schema 30 marks relic items explicitly. `MatchOffers` now generates Auto Chess
relic choices, excludes held/rerolled relics, replaces only relic globals and
applies Forge, Egg Nest and Card acquisition rewards before combining. Unrelated
globals survive replacement; item-cap behavior and free-roll grants match the
original. Persistent relic income/shop/team-size/sale hooks remain separate work.

Gizmos upgrade choices read the match's `GizmosGates` and scheduled gate index,
default to Gold when the gate is absent, and fall back to all eligible upgrades
when fewer than three remain in the tier. Choosing a valid upgrade adds it,
dispatches the required augment-pick handler and combines. Game-start gate rolls
and remaining concrete augment-pick effects still need integration.

Eight focused traces cover both paces, human/AI players, gate tiers, exhausted
choices, exclusions, full inventory, unrelated globals, free-roll grants and
board-lock-sensitive merges: 120,948 checks. Generic offer and round-flow fixtures
now use the original relic/upgrade handlers instead of mock custom choices.
The focused suite isolates augment-pick effects while verifying their dispatch;
run it with `--relic-upgrades-only`. Full match and desktop integration remain
pending.

Validation: 147,597,875 full-suite checks and packaged BRUTAL smoke tests pass
with schema 30.

## Underlords Tamer and starter-crew offers

`MatchOffers` now generates one of each Tamer's two builds in fixed Tamer order,
using four independent RNG draws. Tamer selection applies a valid power only when
the player has none, dispatches the shared power-pick hook and combines the roster.
Tamer rerolls intentionally do not exclude previous builds, matching the source.

Starter-crew options draw from alliances with at least two low-cost shop lines,
sort each pair, reject duplicates/exclusions and retain the 40-attempt limit.
Selection shuffles available matching lines, reserves at most five distinct units
and fills only free bench slots before combining. It preserves board locks and
max-star shop cleanup.

Sixteen focused traces across Underlords/Knockout, both paces and human/AI seats
pass 247,528 comparisons of offers, rerolls, powers, hook dispatch, rosters, pool
counts, shops and RNG. Generic offer/round fixtures now use the actual original
Underlords offer handlers. Power-pick mechanic effects remain delegated to the
required hook interface; full match/desktop integration is still pending. Run the
focused suite with `--underlords-offers-only`.

Validation: 147,845,355 full-suite checks and packaged BRUTAL smoke tests pass.

## Arcane offers and persistent choice state

`ArcanePlanning` now implements Black Market, Junk King Workshop and Anomaly
choices through `MatchOffers`. Market options shuffle the correct contraband tier,
respect the extra-option flag and Syndicate level, then apply exclusions. Passing
adds permanent Syndicate health and increments skips; valid item choices obey the
inventory cap. Workshop choices append valid upgrades, including stacking Spares.

Anomaly options charge for rerolls, preserve the old options when unaffordable,
track seen options and reset that history when fewer than three remain. Choices
preserve unrelated unit modifiers, record anomaly history and extra-pick state,
and grant 8 gold when no eligible target exists. Crest Copy and Dual Wielding use
the original trait-gap and preferred-item logic. AI scoring and choice application
now share the target selector, including human center-hex preference and AI holder
preference.

Eight focused traces cover both paces, human/AI seats and two seeds: 136,808 checks
of offers, gold, inventory, modifiers, histories, extra picks, market/workshop state
and RNG. All 27 Anomalies are selected, including both immediate utility rewards.
Generic offer/round fixtures now use Arcane's actual handlers. Run the focused
suite with `--arcane-offers-only`. Production round hooks, combat setup from this
persistent state and full match/desktop integration remain pending.

Validation: 147,982,115 full-suite checks and packaged BRUTAL smoke tests pass.

## Integrated native AI turns

`MatchAi.Turn` now connects native offer selection/execution, on-AI-turn dispatch,
XP and shopping, Duos portal transfers, bench cleanup, board arrangement, crafting
and equipment, then a final arrangement. `PlanPlayers` processes alive AI seats in
seat order and leaves human seats alone. Pairing and health synchronization stay
with `MatchRounds`, avoiding a second RNG-consuming pairing pass.

Mode integration remains explicit through required `IMatchAiHooks`: turn hooks,
live capacity, base capacity, placement checks, equip-hook presence and rerolls.
Base capacity excludes item bonuses; the arrangement algorithm adds those itself.
Overflow refunds retain shared-pool ownership. No production no-op hook defaults
are supplied.

48 Solo/Duos traces cover all 22 variants over 10,080 AI turns and pass 374,157
comparisons. They compare economy, shop reservations, ordered boards/benches,
equipment, arrivals, RNG and call order. Base traces isolate mode hooks while
preserving native equipment restrictions; four additional Coliseum traces use its
actual purchase, combine, sale and AI-core handlers and verify modifiers/refunds.
Run with `--ai-turns-only`. These establish component integration; the production
mode dispatcher, complete match loop and desktop UI remain pending.

## Native carousel rounds

Schema 31 exports stage-specific carousel tiers and item policy, including
Reckoning's shadow-item chance. `MatchCarousel` reserves up to nine pool copies,
prioritizes the lowest eligible tier for the first seven, shuffles pick order and
stably orders later rounds by health. AI options are scored once each, preserving
RNG order and first-option ties.

Picks validate the current player and availability, grant a unit or full-bench
gold, return that unit's reservation when necessary, deliver the item, combine
through required recipient/partner hooks and clear max-star shop offers. `Step`
runs AI picks up to a human; `Finish` assigns remaining first-available choices
and releases unclaimed reservations. The state exposes current options, order and
turn for future UI/command integration.

2,200 original carousels across all 22 variants in Solo/Duos provide 1,075,916
comparisons. They exercise health ties, automatic finish, exhausted/partly exhausted
pools, full benches, item caps, invalid/out-of-turn picks, merge callback order,
shadow items, roster and shop state, pool counts and RNG. Callbacks are instrumented
in these traces; production mode dispatch and carousel UI remain pending. Run with
`--carousel-only`.

Validation: 149,432,188 full-suite checks and packaged BRUTAL smoke tests pass.

## Auto Chess persistent planning hooks

`AutoChessPlanning` implements relic income, Recruiter shop levels (capped at 10),
Master Key team size, Monopoly Card round rerolls and Bargain Tag sale income.
Global-list order and repeated entries are preserved. The set's normal-pace base
income uses the cumulative scheduled round; hyper income remains unchanged.

Digi-Egg AI selects the highest tier/stage eligible near-merge, retaining the first
roster group on ties and checking the shared pool. It uses the native inventory
command for up to four eggs, preserving human combat locks and merge behavior.
Auto Chess loot-offer scoring shares this same target selection.

2,048 reference scenarios across both paces and human/AI seats pass 180,812 checks:
relic combinations and duplicates, income, shop levels, capacity, sale gold, free
rolls, egg targets, inventories, ordered rosters, shop clearing, pool counts and
RNG. Custom offer regression passes another 122,400 checks. Run the new coverage
with `--autochess-planning-only`. Production mode dispatcher wiring remains pending.

Validation: the self-contained Windows package builds and all BRUTAL smoke checks pass.

## Shop roll policies and special offers

`PlayerShop` now exposes `IShopRollHooks` for pre-roll level, odds and size changes
and post-roll special offers. Refreshes return existing reservations before hooks,
clear metadata, clamp hook-driven sizes to 1–8, roll with optional fractional odds,
run post-roll effects and clear owned three-star offers. Paid and free rerolls use
that same path. Replacement offers keep pool reservation ownership in the shop.

Per-slot metadata supplies prices, display tags and purchase-time stage, trait and
modifier overrides. Purchases copy mutable metadata before buy/combine hooks and
clear the consumed entry. Full-bench post-hook overflow follows the original
combine behavior. Loaded Dice bypasses normal roll hooks and clears old special
metadata; release and max-star cleanup clear it too.

Controlled original-engine hooks exercise resized shops, fractional odds, level
boosts, zero-cost offers, replacement reservations, special units, merges, full
benches, free/paid rerolls and max-star exclusions across all 22 variants. Fates
traces also run the actual Loaded Dice implementation after a special roll. These
verify the shared shop extension contract; concrete Fates/Dragonlands special-roll
handlers and production dispatcher registration remain pending.

Validation: the full regression passed 149,797,814 checks. After the final metadata
cleanup and allocation-free odds lookup, 184,724 shop-roll comparisons and
9,345,774 planning/AI/shop regressions passed. The self-contained Windows package
was rebuilt for the final changes.
All final packaged BRUTAL smoke checks passed.

## Fates Partner shop integration

`FatesPlanning` supplies native roll, purchase and combine handlers. Partner
configuration is read from the already-exported catalog odds/chances, and the
player retains whether a Partner has been seen. Eligibility checks owned Partners,
level, max-star exclusions and the shared pool. Forced first offers, Seeker chance,
tier fallback, uniform line/slot selection, non-unique marked traits and four stat
bonuses preserve original RNG order.

The shop holds one reserved copy. A successful purchase takes two more copies when
available and the bench has space; otherwise it becomes a Rookie Partner and
refunds twice the tier cost. Merges preserve the first Partner source's modifiers
and bonus traits even when an ordinary unit anchors the merge. Selling the owned
Partner allows new offers without resetting first-offer history.

640 reference traces (5,120 shop refreshes) across normal/hyper and human/AI seats
pass 1,333,402 checks. They cover chance/forced/Seeker offers, pool depletion,
full-bench refunds, ordinary-anchor merges through three stars, sales, prices,
metadata, inventory, pool ownership and RNG. Use `--fates-partners-only`.
The handlers connect through `PlayerShop.RollHooks` and `PlanningShopCommands`;
registration in the complete production match dispatcher remains pending.

Validation: the self-contained Windows build and all packaged BRUTAL smoke checks pass.

## Dragonlands shop integration

Schema 32 exports the mode/pace-specific Dragon replacement level and chance.
`DragonlandsShop` applies Doot odds before rolling, then consumes the pending
Stargazer guarantee, applies Dragon replacements and sets Dragon prices/tags.
Doot redistribution preserves fractional probabilities and the original choice of
upper tiers. Stargazer eligibility uses the player's unboosted level and excludes
the first rolled line; ordinary rolling still honors level boosts and Doots.
Dragon replacements sample available non-Dragons of the same tier by remaining
pool copies. All replacements use shared shop reservation ownership and respect
that player's max-star exclusions. The persistent player state now holds Doots
and the pending Stargazer flag for later round/combat hook integration.

480 traces cover 5,760 refreshes and 1,662,478 original-engine comparisons across
normal and hyper. Coverage includes fractional/large/zero Doot counts, boosted
rolls, free and paid rerolls, exhausted Stargazer/non-Dragon pools, owned max-star
units, prices, purchases, merges, sales, metadata, pool counts and RNG.
Run with `--dragon-shops-only`. Production match-dispatcher registration and the
hooks that award Doots/Stargazer guarantees remain pending.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Shared match shop dispatcher

`MatchShopRolls.Install` registers every player's roll handlers and keeps the
current round available for forced Fates offers. It dispatches Gizmos High End
Shopping and Golden Ticket, Auto Chess Recruiter, Underlords Higher Class of
Criminal, Galaxies' server shop, Fates Partners and Dragonlands rolls in original
augment/global/set order. Ownership is read on each roll so acquiring or removing
an effect changes subsequent refreshes; duplicate globals retain their stacking.

`MatchState` accepts an initializer after seats/teams exist and before initial
shop rolls. This permits game-start mechanics and shop registration to share the
correct RNG sequence without returning and rerolling already-created offers.
The match also owns the selected Galaxy. Existing isolated callers keep their
unmodified shop behavior when they omit initialization.

44 reference traces across all 22 variants compare two players' starting shops
and 7,040 later refreshes, passing 582,288 checks. They cover effect ownership
changes, duplicate globals, free/paid rolls, level boosts, shop size persistence,
Partner metadata, Doot/Star state, max-star exclusions, shared pool and RNG.
Run with `--match-shop-rolls-only`. The full production match controller, game-start
mechanics and the remaining non-shop dispatcher hooks still need integration.

Validation: 153,375,892 full-suite checks and all packaged BRUTAL smoke checks passed.

## Native game-start mechanics

`MatchStart.Create` initializes native mode selections and opening rewards, installs
shop dispatch, then allows `MatchState` to roll initial shops. The match stores its
seed, Galaxy, Gizmos gates/mutation, Dragonlands Mirage/Treasure and Arcane
portal/sponsor. Schema 33 exports gate pools, ordered weighted Galaxies and Arcane
portal IDs. The factory accepts the live round accessor used by Partner shops.

Galaxy selection uses the original unsigned seed-string hash without advancing
match RNG. Armory components are rolled once and shared across players; Shrine
items go to every seat. Gizmos and Dragonlands preserve their two initial RNG
choices. Arcane shuffles sponsors before choosing the portal, then grants gold,
items or reserved Champion units in player order before any shop rolls.

5,808 original-constructor references across every mode/pace, Solo/Duos and 132
seeds pass 1,350,848 checks. Seeds include unsigned boundary values and cover all
15 Galaxies, seven mutations, four Mirages, three Treasures and ten Arcane portals.
Comparisons include teams/health, economy, inventories, unit IDs, first shops,
metadata, shared pool and RNG. `--match-start-only` runs this coverage.
These references intentionally stop before first-round offers/hooks, carousel or
AI planning; those still need connection in the full match controller.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Opening-round orchestration

`MatchRounds.Start` performs the separate opening transition after `MatchStart`:
carousel modes immediately start their carousel; other modes create each player's
offer, dispatch round-start effects, clear max-star shops, then plan AI, pair seats
and synchronize Duos health. It does not grant later-round income/XP, reset free
rolls/transfers or refresh the already-created shops. Repeated calls cannot replay
opening rewards; constructing at a later position or advancing closes this path.

264 constructor references across all 22 variants and Solo/Duos pass 47,592 checks.
They use the actual game-start mechanics, shop handlers, offers, carousel generation
and initial automatic carousel picks. Controlled round-start/AI callbacks verify
ordering and team health synchronization; they do not claim full production AI
or mode-round-hook integration. Opening state, inventories, rosters, pool, pending
offers, carousel turns, pairing and RNG match. Use `--opening-rounds-only`.
Existing round regressions pass another 735,775 checks. The production controller
still needs to supply the remaining mode hooks and connect combat and presentation.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Galaxies persistent planning

`GalaxiesPlanning` adds round-start trade/bigbang free rolls, Cluster's pool-backed
upgrade at 1-2, Mirror copiers and Superdense/Big Bang team-size items. Bigger
Galaxy's capacity bonus begins at stage 2. Cluster changes the existing unit's
stage without a combine event and leaves subsequent max-star shop clearing to
the round controller, preserving the original order.

Shop commands now have Galaxies handlers for capped per-line Bounty Hunter
purchase history and Salvage. Salvage runs after sold equipment is returned,
processes the last matching inventory entry, preserves ingredient order, and
respects component/emblem/category restrictions and inventory space. AI copier
use scores eligible roster units once per attempt, uses stable first ties and
runs the existing item command up to three times, including combat-lock behavior.

1,920 reference cases across all 15 Galaxies, both paces and human/AI seats pass
616,240 checks. Coverage includes scarce Cluster copies, full benches/inventories,
duplicate salvage items, bounty caps, max-star shops, purchasing/selling, copier
choices, ordered roster changes, inventory, shared pool and RNG. Run with
`--galaxies-planning-only`. Post-combat rewards, combat-state transfer and production
mode dispatcher registration remain pending.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Galaxies post-combat rewards

`GalaxiesPlanning.RoundEnd` consumes resolved combat and scoped match context,
returning bonus gold for settlement to credit. Data Pirate rewards run first:
up to six recorded kills each roll trait-level gold and component chances. Then
Treasure/Big Bang PvE items, Plunder gold and capped Meep gold follow in original
order. Missing combat still permits Galaxy PvE item rewards. Plunder excludes
summons and units currently absorbed as DigiXros pilots. Inventory saturation
does not skip reward RNG draws. Team-wide pirate/Meep counters are read from the
native combat result; unit filtering uses the settlement's ownership view.

20 real battles and 4,800 reward scenarios pass 14,776 comparisons, including
terminal units, battle duration/winner, pirate/Meep counters, payout amounts,
inventory order and RNG. Coverage spans both paces, trait levels, reward Galaxies,
PvE/PvP, missing combat, full inventory, summons, absorbed pilots and Duos-style
owner filtering. Run with `--galaxies-settlement-only`. Full match dispatcher and
persistent combat-input integration remain pending.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Fates AI consumables and combat persistence

`FatesPlanning.AiTurn` chooses Fallen paths, Copy Data targets and the highest-value
board target for Loaded Dice in original order. Path RNG is drawn even if a path
was already chosen. Copy priority counts roster forms below three stars, with
stable count/tier ties. Dice selection runs after copying/merging and can clear
Partner metadata from the refreshed shop.

`PlanningItems.UseConsumable` exposes the original direct-use path separately from
the equipment gesture. Fates AI bypasses the equipment gesture's board lock, while
the resulting combine operation still respects locked board units. The existing
Equip command retains its lock checks. Both paths dispatch the same native
consumable implementations and required combine hooks.

`FatesPlanning.RoundEnd` returns Golden Fang gold and retains Blossom ability power
(up to 60), using non-summoned allied units from the settlement ownership view.
Missing combat grants nothing. Combat-input projection and remaining Fates trait /
augment planning hooks still need integration in the production match controller.

512 AI traces pass 57,008 comparisons across both paces and human/AI combat locks,
including full benches, existing paths, inventory/pool exhaustion, Partner merges,
item returns and shop metadata. Twenty real battles and 2,400 reward scenarios add
7,518 checks for terminal rewards, cap behavior, owner filtering and RNG. Run with
`--fates-ai-only` or `--fates-settlement-only`. All 3,667,262 shared item regressions
also pass after the direct-use API change.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

## Shared match capacity and movement

`MatchPlacement` computes live team capacity from level, persistent `TeamBonus`,
board equipment, Gizmos New Recruit, Underlords Tamer presence and roster globals,
Auto Chess Master Key and Bigger Galaxy. Power-derived Tamer capacity requires a
valid selected Tamer power and an on-board Tamer. Duplicate global items stack.
Movement delegates to the existing roster command using this live capacity, so
swaps and removal of size-bonus holders trigger the correct excess-unit handling.
Human combat board locks remain enforced. Current modes define no extra canPlace
listeners. The AI arrangement base intentionally excludes equipment and capacity
listeners, matching the original arrangement algorithm's separate selection pass.

2,112 reference traces execute 50,688 moves across all 22 variants and pass
1,350,706 comparisons. They cover persistent bonuses, team-size equipment, Tamers,
Dragon slot costs, full benches, swaps, capacity loss, ordered eviction and human
combat locks. Run with `--match-placement-only`.

The shop dispatcher also now applies Higher Class of Criminal in Underlords
Knockout. The expanded 44-trace shop fixture includes Knockout's global ownership
changes/duplicates and passes all 582,288 comparisons. Production controller
registration and the hooks that award persistent team bonuses remain pending.

Validation: all 156,820,580 full-suite checks and packaged BRUTAL smoke checks passed.

## Underlords persistent Tamer units

`UnderlordsPlanning.PowerPicked` creates the selected power's Tamer only when the
player has no existing Tamer on either board or bench. It selects the original
role-preferred free board cell without a capacity check; a full board falls back
to the bench. If both are full the allocated unit is discarded, preserving the
original UID progression. A bench Tamer blocks a second creation just like a board
Tamer. `RankUp` updates the first owned Tamer in place at ranks 3 and 5 and caps
its form at three stars, preserving its identity and roster position.

1,152 reference traces cover all eight powers in standard and Knockout Underlords,
both paces and human/AI seats, passing 1,253,824 checks. They include empty/partial/
full boards, full benches, existing Tamers, repeat picks, ranks 1–10, capacity,
shop cleanup, pool state, placement order and consumed IDs. Run with
`--underlords-tamers-only`. The production offer/round dispatcher must call these
handlers from power-pick and rank-up events; remaining Underlords planning effects
and combat-input integration are still pending.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Underlords economy, item offers and combined equipment

`UnderlordsPlanning` now handles Recruiter free rolls, the pending loss free roll,
win-streak reset, gold cap, loss-streak income cap and Knockout round income.
Catalog schema 34 exports their original rule values and round-specific item odds.
Item offers roll once per offer, apply Smuggler and Riches, exclude owned globals,
and preserve the original shuffle order and random consumption. Offer rerolls keep
the rolled tier and marker. Combined units return excess equipment in reverse order,
respecting inventory capacity, and retain the first item.

2,048 reference cases across standard/Knockout and both paces pass 23,242 checks
(`--underlords-planning-only`). They cover repeated round-start calls, offers and
rerolls, duplicate globals, positive/negative streaks, gold above the cap, returned
item order and full inventories. These are tested mode handlers; production hook
composition and Underlords round-end rewards still require integration.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.

Full regression validation after this change: **158,097,646 native checks passed**, including all 5,699 reference battles.


### Underlords post-combat planning state

`UnderlordsPlanning.RoundEnd` now carries Beast kill and Legion duel gains into
persistent player state, pays Bounty gold, and schedules the loss free roll.
It processes Silver Lining, Aegis and Vicious Intent in owned-global order before
the mechanic hook, including duplicate listeners and globals removed mid-dispatch.
Divine Rapier transfers from the first board holder to a living opponent on a
non-PvE loss, with the original ghost restriction and inventory-cap behavior.
Owner-scoped combat results preserve Duos reward attribution.

24 original-engine battles (including summoners) drive 5,760 reward cases and
46,104 passing checks (`--underlords-settlement-only`). Coverage includes missing
combat, wins/losses, PvE, ghosts, owner views, absent/dead opponents, duplicate
globals, lethal Aegis thresholds, full inventories and bench-only Rapiers.
The self-contained Windows publish and packaged BRUTAL smoke checks passed.
Production dispatcher registration and projecting persistent counters into the
next combat remain part of the full match-controller integration.


### Dragonlands planning and Golden Armor

`DragonlandsPlanning.RoundStart` snapshots capped bank gold and full-health status
for combat, consumes Calculated Loss free rolls when owned, and preserves the
one-per-game augment reroll tracking and Treasure Hoard reroll bonus. Golden Armor
removes golden items from inventory and benched units, counts the active board's
GOLD trait, and draws missing golden equipment in catalog order with the original
RNG consumption. Existing board equipment stays equipped; Diamond Hands is never
drawn and Golden Crown requires the third trait tier.

1,024 original-engine cases across both paces pass 21,504 checks
(`--dragon-planning-only`), covering repeated calls, trait tiers, owned equipment,
bench stripping, full inventory, 138 actual item-grant cases, 42 Crown-grant cases,
and augment/treasure offers. Persistent fields are ready for the combat projection;
Dragonlands post-combat and on-pick effects and production hook composition remain
unfinished. Self-contained Windows publish and packaged BRUTAL smoke checks passed.


### Dragonlands post-combat rewards

`DragonlandsPlanning.RoundEnd` applies active traits in their original count order,
then owned augments, then the set mechanic. Stargazer progress schedules its third-
round shop bonus; Lagoon converts casts into gold/components; Starcaller heals;
Digi-Core and combat-training gains retain their caps. Calculated Loss schedules a
free roll. Golden equipment pays combat gold, accumulates Treasure Axe attacks,
and upgrades the first owned Investor to Diamond Hands while granting its reward.
The original shared team counters remain shared when a Duos owner view filters
units. Missing combat still permits callbacks that depend only on persistent state.

24 actual original-engine battles cover all seven combat reward counters with
nonzero outcomes. 5,760 settlement cases pass 51,912 checks
(`--dragon-settlement-only`), including trait activation and ordering, stack caps,
multiple payouts, missing combat, PvE, owner views, missing/bench/board Investors,
full inventories, equipment replacement order and subsequent RNG state.
The Windows package and BRUTAL smoke checks passed. Augment-pick effects and full
production match-controller composition remain outstanding.


### Dragonlands augment-pick rewards

`DragonlandsPlanning.AugmentPicked` implements all 15 Dragonlands augment pick
handlers: gold, lesser duplicators, components, board-preferred emblems, crests,
fixed unit grants, radiant items, Crown and Dragon Pact. Fixed grants reserve one
pool copy only when a bench slot exists; otherwise they pay the line price without
allocating a unit ID. Successful grants invoke a required combine callback before
any later grant, preserving multi-grant merge timing and allowing the production
controller to dispatch partner-combine effects. Pact draws from available tier-four
Dragons and grants capacity even when none remain.

2,720 cases cover every Dragonlands augment in both paces, human/AI seats, planning/
combat board locks, near/full benches, depleted pools, merge chains, inventory caps,
board-preferred emblems, equipment carryover, callback counts, pool/shop state,
unit IDs and RNG. All 228,600 checks passed (`--dragon-augments-only`). The production
offer dispatcher still needs to register this handler with its combined callback;
these checks validate the complete pick operation against the original engine.

Validation: **158,445,766 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Fates trait rewards and augment planning effects

`FatesPlanning.RoundEnd` now runs Treasure, Deva, Fallen and Forge trait callbacks
in count order before combat-derived rewards. Treasure retains loss tokens and
pays on wins; Fallen waits for room for both path items; Forge uses the strongest
board smith, avoids artifacts already in inventory, and preserves unpaid progress
when capped or full. These callbacks still run when combat state is absent.

Fates augment picks now grant their original gold, health, rolls, equipment,
artifacts and reserved units. Festival grants a Partner with three reserved copies,
or its extra gold when the player already owns one, the bench is full, or the pool
is exhausted. Partner modifiers use the same generator as shop offers. A required
combine callback preserves Festival's immediate combine and caller-owned dispatch.
Round-start dice rolls and Offerings income are also implemented.

2,048 trait-reward sequences pass 32,768 checks, including 216 actual Fallen grants
and 344 Forge grant sequences (`--fates-trait-rewards-only`). 2,400 augment cases
pass 218,960 checks, including 48 Festival grants (`--fates-augments-only`).
The existing 1,333,402 Partner-shop and 7,518 combat-settlement checks also pass.
Self-contained Windows publish and all packaged BRUTAL smoke checks passed.
Production hook composition and persistent-state combat projection remain pending;
these handlers do not yet constitute the full production match controller.


### Gizmos persistent planning hooks

`GizmosPlanning` implements Gate payouts, Pirate chest dice and loss progression,
chest payouts, Petit unit grants, Calculated Loss/Hyper Roll/Junkyard rewards,
Reserves snapshots, free-roll upgrades, and all upgrade-pick rewards. Callback
order preserves low-gold thresholds before chest payouts, and Reserves captures
gold before the Gate payout. Petit grants require bench room and an available
eligible pool copy; a required callback performs the immediate combine.

Glutton selling feeds the first owned Gekomon, caps its health/AP gains and retains
unrelated modifiers. Glutton combines retain the maximum health and AP from their
sources. These handlers implement the match shop interface for later composition.

2,048 planning cases pass 191,634 checks, including 1,536 chest gains, 948 Petit
grants and 1,052 chest payouts (`--gizmos-planning-only`). 4,640 upgrade-pick cases
pass 418,064 checks (`--gizmos-augments-only`), covering both paces, human/AI board
locks, inventory caps, prior upgrade counts, equipment carryover, Glutton merges,
pool state, IDs and RNG. Full production match-controller registration and combat
projection remain pending.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Galaxies augment planning and Auto Chess Priest

`GalaxiesPlanning` now grants all augment-pick rewards, including the reserved
Champion-form tier-four unit, and applies augment income and recurring free rolls
before Galaxy mechanic effects. 1,680 cases across both paces and all galaxies
pass 144,368 checks (`--galaxy-augments-only`), covering full inventories/benches,
depleted pools, board locks, combine behavior, pool refunds, IDs and RNG.

`AutoChessPlanning.RoundEnd` implements Priest's loss-damage refund before damage
is applied. It uses the current round's loss rules and surviving non-summoned enemy
units, floors the trait-scaled amount, and leaves wins or missing-combat results
untouched. 2,048 original-engine cases pass (`--autochess-priest-only`), spanning
both paces, trait tiers, PvE/PvP, both sides, stages, unit tiers/forms and summons.
Both handlers still require production match-controller registration.

Validation: **159,453,608 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Arcane round-start offers and portal tiers

`ArcanePlanning.RoundStart` now applies recurring augment and portal free rolls,
forces the Golden/Prismatic portal augment tier through the shared `MatchOffers`
instance, and opens deferred offers in original priority order: existing offer,
pending Anomaly, queued Market, then Chaosdramon Junk. Markets advance their tier
counter before options are drawn. Junk progress pauses while a higher-priority
offer is active, counts a bench Chaosdramon, uses the pace-specific interval, and
excludes owned parts while always offering spare parts.

512 original-engine traces run ten rounds with repeated callbacks, passing 51,712
checks (`--arcane-rounds-only`). They cover both paces, portal tier effects on later
augment options, pre-existing offers, full anomaly history, market queues, all Junk
parts owned, board/bench/absent Chaosdramon, and subsequent RNG state. The existing
136,808 Arcane offer and 529,782 shared offer checks also pass. Production round
hook registration and the remaining Arcane planning effects are still outstanding.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Arcane augment picks, Shimmer and inherited Anomalies

`ArcanePlanning.AugmentPicked` implements all Arcane pick rewards, including
Shimmer's optional Mushroomon grant and unconditional immediate combine, market
Shimmer, health/gold/rolls/items, reserved Champion-form units, and extra Anomalies.
Shimmer converts each 100 points into a queued Market. Mutant queues an immediate
Anomaly when one has already been chosen, otherwise it adds a future extra choice.
The new income handler applies the recurring income augment.

Arcane now implements the shop-hook interface. Its combine handler retains an
existing Anomaly; otherwise it copies all modifiers from the first anomalous source
over the anchor's modifiers, preserving unrelated anchor keys as the original does.

2,400 original-engine cases pass 230,976 checks (`--arcane-augments-only`), covering
both paces, human/AI board locks, inventory and pool constraints, absent and existing
Anomalies, Shimmer thresholds, immediate combine counts, inherited modifiers, pool
state, IDs and RNG. Each case opens the next queued offer to verify the connection
to round-start behavior. Existing Arcane round and offer checks also pass. Runtime
match-controller registration, Arcane post-combat and AI hooks remain outstanding.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Arcane AI contraband and copier use

`ArcanePlanning.AiTurn` assigns contraband in reverse inventory order to the highest-
value eligible Syndicate unit, breaking ties by UID and including equipped Syndicate
emblems. It then processes copiers in reverse order, preferring board targets with
the most owned Rookie copies, then unit value and UID. Each copy re-evaluates the
roster and pool after any merges. Full benches skip copying; direct consumable use
matches the original target-lock bypass while merges still respect human board locks.

1,024 reference cases pass 118,896 checks (`--arcane-ai-only`) across both paces,
human/AI seats, planning/combat phases, full benches/inventories, depleted pools,
small/full copiers, natural/emblem Syndicate units, merge equipment overflow and
Anomaly inheritance. Inventory and unit order, modifiers, pool/shop cleanup, IDs
and RNG all match. Full match-controller registration and Arcane post-combat
progression remain outstanding.

Validation: self-contained Windows publish and all packaged BRUTAL smoke checks passed.


### Arcane post-combat progression

`ArcanePlanning.RoundEnd` processes Academy sponsorship gifts, Syndicate Shimmer,
Conquest progress/chests and High Roller coins in trait-count order, followed by
Anomaly-finding rewards and board equipment payouts. Conquest chest gold credits
the economy directly; other payouts return as settlement bonus gold. Shimmer uses
the pace and current loss streak, and Academy only grants newly reached tiers.
Owner-filtered combat units drive Conquest, coins, bounty and Anomaly kill rewards.
Shield payout lookup requires the matching board UID and team in that filtered view.

24 original-engine battles exercise all four combat reward counters with nonzero
results. 5,760 cases pass 34,632 checks (`--arcane-settlement-only`), including
missing combat, PvE, trait order/tiers, full inventory, empty sponsorship, preloaded
progress, multiple chests, streak thresholds, owner views and unmatched shield UIDs.
The full production match controller still needs to register these handlers and
project their persistent state into subsequent battles.

Validation: **159,889,824 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Reckoning planning and AI

`ReckoningPlanning` handles Dragon Kin egg creation and hatching, deferred Radiant
Blessings, augment rewards, income and Darkness loss damage. Catalog schema 35
exports the pace-specific Blessing threshold and gold. Eggs drop individually so
AI collection preserves original inventory limits, orb IDs and random draws.

AI crafting consumes Shadow components in inventory order, crafts egg emblems at
trait breakpoints, chooses Tome rewards from four shuffled options and refunds up
to three removers. Holder rankings and trait counts retain the original snapshot
timing across crafting passes.

1,536 original-engine planning traces pass 58,160 checks; 1,024 AI traces pass
103,560 checks. Coverage includes existing offers/carousels delaying Blessings,
multiple eggs, human/AI collection, inventory capacity, stage-dependent crafting,
Tome/remover guard limits, persistent roster/shop/pool state and RNG continuity.
Use `--reckoning-planning-only` and `--reckoning-ai-only` for targeted checks.
Production match-controller registration and full application acceptance remain
outstanding; this module is not a completed game release.

Validation: **160,051,544 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Shared unit lifecycle integration

`MatchStart.Create` now installs `MatchUnitLifecycle` and exposes its shop, item,
portal and carousel commands through `MatchState.Units`. The roster uses the
installed lifecycle when a merge caller does not supply an explicit callback,
covering offer rewards, loot collection, treasure rewards and settlement cleanup.
Explicit command callbacks use the same mode handlers. Portal/carousel callbacks
separate owner and partner effects so Proud Partner is not paid twice.

The composed lifecycle preserves Arcane Anomalies, Fates Partner marks, Coliseum
forms and core refunds, Gizmos Glutton stats, and Underlords' one-item limit. It
also dispatches mode purchase/sale behavior, Auto Chess sale income and Duos sends.
Standalone low-level roster tests can still supply their own callbacks.

4,960 original-engine scenarios pass 947,276 checks (`--unit-lifecycle-only`),
covering all 22 variants in Solo/Duos, direct merges, purchases, offer choices,
loot, copiers, portals, carousel picks and settlement cleanup. Snapshots check
unit order/modifiers/items, capped inventory, partner rewards, shop exclusions,
shared pool counts, IDs and RNG. All 5,808 match-start cases also verify automatic
lifecycle installation. Full round/offer/AI/settlement dispatch and persistent
combat projection still need to be composed into the production match controller.

Validation: **161,004,628 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Faction Wars planning and Data Wisps

`FactionWarsPlanning` implements augment pick rewards, income, recurring free
rolls, Wisp selection/expiry, all 15 Wisp purchases and AI purchase decisions.
Catalog schema 36 exports Wisp IDs, base costs and minimum stages. Copy/evolve
Wisps preserve the original roster order, pool reservations, refunds and merge
behavior; temporary attack speed/armor/mana rewards accumulate in `NextBuffs`.
Human Wisp actions reject non-planning phases, while the AI hook follows the
original independent purchase path and its short-circuit RNG consumption.

2,784 original-engine traces pass 2,536,556 checks (`--faction-planning-only`).
Every Wisp has successful purchases in both paces. Tests include missing Wisps,
insufficient gold, full inventory/bench, depleted pools, board locks, modifier
preservation, immediate merges, preferred Crests, boosted shop refreshes, augment
unit rewards, repeated round-start selection/expiry, IDs and RNG continuity.
The production controller still needs to register the handlers and expose the
Wisp action in the native planning UI; combat projection must consume NextBuffs.

Validation: the full native regression suite passed, including all 5,699 reference battles. The expanded full-bench Faction Wars fixture independently passed all 2,536,556 checks. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Composed augment and Tamer choice effects

`MatchOffers(state, units, phase)` now supplies `MatchOfferEffects`, dispatching
mode and Duos pick rewards through the shared unit lifecycle. Immediate merges
inside Fates/Dragonlands/Arcane rewards retain their original timing, followed
by the offer's final merge and player-specific max-star shop cleanup. Unknown
custom offer kinds fail explicitly instead of silently discarding a choice.

Coliseum now implements its remaining augment pick rewards and sponsor income,
including preferred Crests, artifact selection, upgraded-unit grants, and Digi-Core
overflow banking. All other pick handlers use the existing tested mode modules;
Underlords powers create their persistent Tamer unit through the same path.

17,280 original-engine choice scenarios pass 4,173,072 checks
(`--offer-effects-only`) across all 22 variants and Solo/Duos. Each test calls the
composed `MatchOffers.Choose`, checking rewards, mechanic state, partner payouts,
unit flags/modifiers/order, health, capacity, inventory, shared pool, shop cleanup,
IDs and RNG. Cases include full benches/inventories, depleted pools, human combat
locks and both augment/upgrade and Tamer offers. Production round/AI/settlement
composition, battle projection and native match UI integration remain outstanding.

Validation: **167,714,256 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Composed AI turns and Duos ranking metadata

`MatchAi(state, offers, units)` now supplies `MatchAiEffects`, routing each mode's
AI preparation through the shared inventory and unit lifecycle. The dispatcher
uses live capacity rules and installed shop hooks. Turn callbacks receive the
actual phase and capacity queries receive the current stage, including calls made
outside normal planning. No current mode adds a placement veto; Underlords remains
the mode with equipment restrictions.

132 original-engine traces cover 16,632 AI turns across all 22 variants, three
seeds, Solo/Duos, and 18 sequential planning/combat/carousel contexts. Real match
initialization, offer selection/pick effects, Wisps, consumables, shopping, shop
hooks, transfers, merges, bench trimming, equipment and arrangement run together.
710,579 checks pass (`--composed-ai-only`), including player state, inventory,
roster order/modifiers, shared pools, IDs and final RNG continuity.

These integrated tests exposed missing shared Duos augment tags/tiers in the
catalog's AI metadata. Shared augments live outside the mode's own enumerable
augment entries; the exporter now includes them explicitly. This fixes decisions
such as choosing Shared Interest versus Bank. The offer-effect fixture likewise
now includes every shared Duos augment: 25,024 scenarios and 6,044,656 checks pass.
Production round transitions, settlement dispatch, combat projection and native
match UI still need integration; these results are not full-game acceptance.

Validation: **170,296,419 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Composed planning sessions and round transitions

`MatchPlanningSession` owns match initialization, the shared unit lifecycle,
offers, AI and rounds. Opening rounds start automatically; advancing finishes an
active carousel before starting the next round. Carousel picks advance the AI
pick queue. Combat creation and settlement remain the caller's responsibility.

`MatchRoundEffects` composes income, mode rewards/offers, Tamer rank-ups, Duos
transfers, carousels and AI turns. Care Package gifts run before set mechanics;
Gizmos interleaves them with its augment listeners so Reserves snapshots gold at
the original point, even if a gift causes a Proud Partner payout. Coliseum's
recurring augment roll and sponsor income are included in the composed path.

132 original-game sessions pass 1,126,287 checks (`--planning-session-only`).
Each covers opening plus 24 actual round advances: 3,168 transitions across all
22 variants, three seeds and Solo/Duos. Over 11,000 Care Package arrivals occur.
Tests compare economy/streaks, offers, AI results, persistent economy state,
rosters, items, transfers, carousel reservation/order/picks, opponent pairings,
team health, pool counts, IDs and RNG. They deliberately skip combat between
planning rounds; combat projection and settlement dispatch still need integration,
followed by production UI, multiplayer, migration and release acceptance.

Validation: **171,422,706 full-suite native checks passed**, including all 5,699 reference battles. Self-contained Windows publish and all packaged BRUTAL smoke checks passed.

### Planning state projected into combat

`MatchCombatProjection` builds isolated player and Duos combat inputs from the
live planning session, including owner IDs, board offsets, unit modifiers,
combined traits, buffs, powers and mode mechanic state. Duos preserves the
original merge order, including unset versus explicitly false mechanic flags.

1,408 original-engine scenarios pass 40,568 checks (`--combat-projection-only`).
They cover every mode/pace and both formats, RNG continuity and snapshot isolation
when planning inputs subsequently change. Battle creation and settlement dispatch
remain to be composed before production match UI integration.

Validation: **171,463,274 full-suite native checks passed**, including all 5,699
reference battles. Self-contained Windows publish and packaged BRUTAL smoke checks
passed.

### Composed settlement effects

`MatchPlanningSession.Resolve` now routes resolved fights through
`MatchSettlementEffects`, applying PvE rewards and each mode's post-battle state
changes through the shared unit lifecycle. Buffs clear before final merges and
elimination. Faction Wars now pays Fungus trait gold after combat; core elimination
also clears pending Wisps.

Seven existing battle-result suites now exercise the composed dispatcher, along
with Reckoning's sequential planning/reward traces. An additional 136 original
Faction Wars cases cover Fungus thresholds, wins/losses and PvE/PvP. Cleanup checks
cover all 22 variants. Gizmos retains its standalone mode tests; its composed
reward-triggered merges still need full match coverage.

Battle creation, PvE wave selection and complete match-loop acceptance remain
outstanding, followed by production UI, audio, multiplayer, migration and release
acceptance. This is an integration milestone, not a production release.

Validation: **171,463,590 full-suite native checks passed**, including all 5,699
reference battles. Self-contained Windows publish and packaged BRUTAL smoke checks
passed.

### Complete native match loop

The native session now owns battle preparation, simulation and settlement.
`StartCombat` resolves pending offers, collects loot, fills available board slots,
finishes carousels and constructs Solo, Duos, ghost or individual PvE battles.
`MatchBattle` retains participants, seeds, combat inputs, trait counts and the
simulation for presentation and settlement. `FinishCombat` resolves every battle
and applies persistent rewards before the next round.

Catalog schema 37 adds combat timers and deduplicated PvE formations. The audited
wave functions become constant after their late-stage branches; stage 10 is the
last stored stage. Dragonlands keeps separate Classic, Order and Chaos treasure
bosses. 2,548 original-engine cases verify formation order, coordinates, stars,
UIDs, timers and array isolation across all variants, all treasure bosses and
stages through 1,000 (21,322 checks; `--pve-waves-only`).

88 complete original-engine matches now pass 2,908,770 checks
(`--full-match-only`): 2,571 rounds and 8,799 battles, including 319 ghost fights
and 4,165 PvE fights. Each mode/pace runs Solo and Duos with two seeds, including
unattended human turns. Checks cover preparation, combat participants/seeds,
terminal unit state, winners/timing, traits, economy, offers, shops, loot orbs,
rosters/modifiers, shared pool, transfers, carousel choices, elimination,
placements, next IDs and final RNG continuity through match end.

These integrated runs exposed a cast-item iteration crash: Zap damage can cause
death summons to append combat units. The native loop now visits the growing list,
matching the original JavaScript array iteration.

The desktop still presents the migration workbench. Production match screens,
player action routing, audio/visual polish, multiplayer, progress/settings
migration, packaging/update acceptance and release remain outstanding. Full match
parity is stronger engine evidence, not full product acceptance.

Validation: **174,393,682 full-suite native checks passed**, including 88 complete
matches and all 5,699 isolated reference battles. Self-contained Windows publish
and packaged BRUTAL smoke checks passed.

### Shared player action boundary

`MatchPlanningSession.Actions` routes player commands through the composed match
services. It supports purchases/sales, movement, rerolls/locks, equipment and item
swaps, XP, offers/rerolls, carousel picks, loot collection, Duos portals and Faction
Wars Wisps. Seat claim/release updates human control and advances a released
carousel turn. Commands for eliminated players or completed matches are ignored,
except seat changes as in the original game.

Results use typed variants for flags, status/item IDs, evolved units, loot and
transfers. Purchases now expose their evolved units without changing the existing
boolean convenience API. Numeric arguments reject missing, fractional, negative,
string and out-of-range indices; native seat-name arguments are text, capped at
16 UTF-16 code units. Transport callers must authorize the sending player before
passing a command to this boundary.

88 original-engine action traces exercise 41,005 commands across every variant
and Solo/Duos. 15,277,814 checks (`--match-actions-only`) compare result payloads,
seat/control changes, phase restrictions, shops, economy, inventory, loot,
rosters/modifiers, offers, transfers, carousels, pool counts, IDs and RNG. The
traces include actual combat and settlement between command sequences, invalid
arguments, combat board locks, successful transfers and Wisp purchases, plus
commands sent after elimination/match completion.

Production match screens still need to use this boundary. This does not complete
UI, multiplayer transport, audio, save migration or release acceptance.

Validation: **189,671,496 full-suite native checks passed**, including all action
traces, 88 complete matches and 5,699 isolated reference battles. Self-contained
Windows publish and packaged BRUTAL smoke checks passed.

### First playable native match screen

The default Windows application now opens a local-match menu and the native match
screen instead of the diagnostic shop. Solo and Duos, all ruleset/pace selections,
shopping, bench/hex placement, equipment, inventory swapping, XP, offers, carousels,
loot, Wisps, transfers, standings/scouting, battle viewing and round progression
connect to the tested native session and action boundary.

Planning renders the real rigged models and idles on the board. Battles reuse the
live match simulation, with per-view mirroring, health labels and animation; other
fights advance on fixed ticks before settlement. Viewports account for framebuffer
scaling. Planning GPU resources retire when their models are no longer present,
and same-UID form changes replace the model. Windows' installed Segoe UI font is
used when available; no system font is redistributed. Display names/descriptions
for items, traits, augments, powers and Wisps are exported as build-time data.

`--workbench` retains the diagnostic UI. `--match-smoke` sends actual ImGui mouse
events through the menu, carousel, purchase and two placement controls, checks 3D
planning, live PvE, and mirrored PvP rendering, and runs five settlements and
round advances. Duos also checks the 14-column team battlefield.
`--duos` repeats that flow in Duos. Captures are written to
`native/artifacts/match-ui.bmp`, `match-ui.battle.bmp` and `match-ui.pvp.bmp`
(and `match-duos.*`).

This is the first playable local screen, not completed production UI. Rich mode
panels and choice descriptions, further UI polish/accessibility, audio and visual
effects, multiplayer, player migration, settings, installer/updater and release
acceptance remain outstanding. The click-through scenarios currently cover
Faction Wars Normal in Solo/Duos; engine coverage across all variants remains in
the full-match and action parity suites.

Validation: self-contained Windows publish passed. Packaged asset, animation,
GPU and diagnostic rendering smoke checks passed. Packaged Solo and Duos match UI
click-through tests passed through five rounds, including mirrored PvP; planning,
PvE and Duos PvP captures were visually inspected. Gameplay code was unchanged in
this UI milestone; the preceding full rules suite passed 189,671,496 checks.

### Native audio playback

The playable desktop now uses BRUTAL SDL for a bounded stereo PCM mixer. The
build renders the existing WebAudio synthesis offline into 114 assets: action
and combat cues, cast elements/star levels, cosmetic stings, soundboard clips,
the victory jingle and both complete original music tracks. No browser or
JavaScript runtime is shipped for playback. Music changes phase with a fade,
loops after its intro, and retains the final notes/reverb over the next loop.
The menu has effects/music volume controls; a match has a mute button. A missing
audio device leaves gameplay available.

`build.ps1` installs pinned Playwright build tooling into ignored artifacts and
uses installed Microsoft Edge for offline synthesis. The exporter checks source
and asset hashes before reusing its cache. Generated audio lives in
`native/artifacts/audio-content` and is copied into the desktop output. To render
it independently: `node native/tools/export-audio.cjs`. `--audio-smoke` checks
all PCM assets and music loop bounds, effect rate limiting and consumption of
silent PCM by an actual BRUTAL SDL playback stream. It requires an audio device
(or SDL's dummy audio driver). Graphical smoke runs use zero playback volume.

This audio integration is still in progress: soundboard/cosmetic UI and network events, custom music, phase resume
positions, focus handling, device reconnect and persistent/migrated settings
remain to be completed. Reward cards and base unit stat tooltips are now present;
mode progress panels and the other production UI/release requirements above are
still outstanding.

Audio validation: 114 generated assets passed finite-sample, non-silence and
no-clipping checks. Cache hashes verified on the next exporter invocation.
Packaged audio checks passed, including a forced loop rollover with release tail,
rate limiting and SDL queue consumption. Packaged Solo/Duos click-through checks
passed. A separate Duos run with an unavailable SDL audio driver also passed.


Combat audio now resolves each ability's original element and stage, including
set-specific move overrides. Its separate presentation cursor follows only the
viewed battle and discards historical sounds on scouting switches. Cast variants
share the source sound-family rate limit; healing sounds are limited to healing
ultimates, as in the original renderer. `export-ability-elements.cjs` preserves
all 22 variants' move resolutions. `export-audio-cues.cjs` executes the original
renderer event handler with visuals stubbed and records 22 battle traces.
`--audio-cues-only` passed 11,736 checks and matched 3,627 cues (501 casts and 27
ultimate healing cues included), with repeated-frame and scouting-history checks.
Packaged audio, Solo and Duos smoke checks passed after this integration.

### Native audio preferences

The native app persists music/effects volumes, mute and music-off in
`%LOCALAPPDATA%/DigiTactics/native/audio-settings.json`. Settings are available
from the menu and the in-match Sound settings button. Slider writes are debounced
and the final edit is flushed at exit. The storage project has no renderer or
gameplay dependencies. It writes through a flushed temporary file and atomic
replacement, retaining the previous file as `.bak`. Unknown JSON fields survive
edits. Malformed/newer files remain untouched and a visible error explains why
saving is disabled. An exclusive writer lock and optimistic document comparison
prevent two running native instances from silently overwriting one another.
Graphical/audio smoke runs do not load or save real user preferences.

`--settings-only` passed 20 checks covering persistence, backup, contention,
invalid values/documents, future versions, unknown fields and temporary cleanup.
Packaged Solo/Duos clicks now also open/close the audio panel and toggle mute.
Those flows and the packaged audio playback check passed. Existing Electron or
browser data has not been migrated yet; progress, cosmetics, soundboard/custom
music and the remaining settings still require their migration and native UI.

### Legacy profile backup and staging

The existing game's Settings > Data page now has **Export save backup**. It
captures every durable `dt.`/`dt-` localStorage value exactly (including unknown
keys and values that are not valid JSON), plus all `dt-music/tracks` records,
original music bytes, metadata and File names/timestamps. Rebuildable portrait
caches and session-only state are excluded. A changed save or unreadable music
file aborts export instead of silently omitting data. The current JSON transport
supports up to 256 MiB of music and rejects larger libraries explicitly. The
exporter never clears or edits the source save.

`DigiTactics.Desktop.exe --stage-profile <backup.json>` validates the archive and
music SHA-256 hashes, then preserves its exact bytes under
`%LOCALAPPDATA%/DigiTactics/native/imports/<archive-sha256>.json`. Music names are
metadata, never extraction paths. Restaging the same backup is idempotent;
conflicting stored data is preserved and reported. This command does **not**
apply progress or settings: the native profile consumers and import UI remain
unfinished, so this is a migration preparation step rather than completed save
migration. The source backup and current native preferences remain untouched.

Validation: isolated Edge tests covered empty profiles (without creating a music
database), raw/unknown storage, binary music, File metadata, corrupt music and
both a direct download and the rebuilt game's real Settings-page button. The
browser-produced fixture passed 18 native integrity/preservation checks,
including corrupt payloads, duplicate IDs/fields, future format versions,
byte-for-byte staging, repeated staging and conflict protection. The Windows
package was rebuilt and its match smoke check passed.

### First native profile import

The menu can select a legacy backup through BRUTAL SDL's file chooser, preview
its identity and entry/music counts, and explicitly import it. The import uses
the already-validated preview bytes, preserves the complete original archive,
then atomically creates `profile.json`. An existing native profile is never
replaced, including an unreadable or newer-format profile. All saved storage
strings remain intact; names, Bits and historical games/wins/top-four totals are
read for the menu, and the imported name is used for the human match seat.
Invalid progress sections display Unavailable rather than being overwritten.

`--import-profile <backup.json>` uses the same transaction without the UI.
`--profile-directory <directory>` supplies an isolated profile location for
verification; default production storage remains the native LocalAppData folder.
The existing `--stage-profile` command still only preserves a backup.

Validation: 32 profile/staging checks passed, including restart readback, source
changes after preview, all raw fields, exact original backup bytes, competing
imports and damaged/future native-profile protection. Packaged CLI import into
an isolated directory and a match click-through using that profile passed; Duos
also passed. The imported-profile menu capture was visually checked. The native
OS file chooser itself has not yet been exercised by automated UI control.
Cosmetic/farm consumers, custom music playback, settings application and native
match-progress updates still need completion; full migration is not yet done.

### Native match history persistence

Native Solo/Duos games now capture the last living board and append a result
when the player receives a placement. History uses the original seed/seat key to
prevent duplicate awards to the history counters, retains the latest 30 matches,
and updates games, wins, top-four/top-two finishes, placement sum, trait use and
unique unit-line use. The result keeps its original timestamp across retries.
Profile writes are atomic, keep the prior file as `.bak`, preserve untouched raw
save domains and root metadata, and reject stale instances. Save failures remain
visible and can be retried. Invalid history is preserved instead of reset.

`export-match-history.cjs` runs the original `recordMatch` implementation to
produce 42 sequential reference updates. `--history-only` passed 568 checks:
reference parity, duplicate prevention, and complete native placement-to-disk
flows for all 22 variants in both Solo and Duos, including restart readback,
last-board retention, untouched imported domains, backups and stale-writer
protection. The 32 profile/import checks and packaged Solo/Duos flows also passed.
The gameplay engine itself was unchanged. The subsequent reward-tracking work
below adds detailed statistics; Bits payouts and other post-match progression
still require integration.

### Match reward calculation and tracking

Native history now records PvP wins, eliminations, highest unit star level,
highest player level and placement. Settlement observations are idempotent and
handle PvE, ghosts and team matches according to the original tracker.
`--history-only` passed 656 checks, including persisted reward statistics.

`MatchRewards.Calculate` implements placement Bits, PvP/elimination bonuses,
first-game bonuses, deterministic daily missions and the original duplicate
award retention. It returns updated shop/wallet documents without writing them,
preserving unknown fields. `--rewards-only` passed 4,660 comparisons against
225 sequential original award cases and 44 complete original tracker traces
covering every variant in Solo and Duos. Both fixture exporters run in the native
build pipeline. The Windows package and Solo/Duos UI smoke checks passed.

The following payout integration now connects this calculator to native profile
writes, including Little Legend rewards.

### Atomic native match payouts

Completing a native Solo/Duos match now saves Bits, daily missions, Little Legend
care/Digi-Data and match history together through one atomic profile replacement.
The standings panel shows the saved Bits breakdown and legend award. Failures
expose a retry action; the original result timestamp, reward day and statistics
remain fixed. Failed calculations or writes never publish a partial award.
Repeated completion and reopened results use the original duplicate retention.
An existing history entry with missing Bits can receive its cached reward total
without incrementing the historical counters again.

`LegendRewards` uses an embedded catalog generated from the original 37 legends,
evolution reachability and XP constants. It preserves equipped-legend fallback,
care increments, XP caps, form/record repair and the 60-game reward history.
The exporter executes the original cosmetics registry and LegendEvo code to
produce 12,025 cases; 24,050 save/award comparisons passed. The existing 4,660
Bits/mission and tracker comparisons also passed.

The 807 history/persistence checks include complete matches across every variant
in Solo and Duos, wallet/history reward agreement, legend rewards across restart,
malformed-domain rejection, lock failure and retry, fixed reward dates and
no-write/no-payment duplicate completion. The Windows package and Solo/Duos UI
smokes passed. Those UI smokes exercise five rounds, not the final reward panel;
complete payouts are verified through the native session/storage integration.

This enables match payouts only. The native cosmetic store, legend management,
Digi-Farm, custom music, remaining settings and multiplayer are still unfinished;
the full port is not ready for production release.


### Native collection and direct purchases

The menu now opens Collection and store with the original 415 cosmetics across
seven categories. Imported ownership, free items, linked unlocks and equipped
items are read from the preserved cosmetic save. Players can equip owned items,
choose/clear six-slot emote and sound wheels, and confirm direct purchases using
Bits. Daily featured prices follow the original date hash and rounding. Secret,
linked and egg-only items cannot be bought directly; owned items cannot be bought
twice. Wallet debit and ownership are saved in one atomic profile update, keeping
unknown fields and ownership IDs. Errors leave the existing profile untouched.
Equipping a Little Legend also determines which legend receives match rewards.

The embedded catalog is exported from the original game in an isolated Edge
context. The build refreshes the original bundle before export. Native runtime
code uses only the generated data and C# collection rules. `--cosmetics-only`
passed 24,991 checks, including 1,345 operations against the original registry,
1,245 original daily prices, eligibility, duplicate purchases and balance checks.

The rebuilt package passed `--collection-smoke --profile-directory <isolated>`:
actual mouse clicks opened the store, confirmed a Bukamon purchase and equipped
it. Disk readback verified 341 Bits from an imported 491, equipped Bukamon and
preserved unknown imported ownership. The collection screenshot was inspected;
Solo/Duos match UI checks passed. The collection smoke requires a fresh isolated
fixture profile with enough Bits and no owned Bukamon.

This is the functional collection foundation. Cosmetic previews and in-match
visual/audio application, Digi-Egg purchasing, legend evolution controls and the
Digi-Farm remain to be ported. The current text list still needs the production
collection layout and interaction polish before release.


### Native Digi-Egg purchases

Collection and store now offers the Mixed Digi-Egg and all seven category eggs,
with the original 350/250-Bit prices, eligible item pools and rarity odds.
Confirmation rolls a rarity and then an item within that rarity. New cosmetics
are granted immediately; duplicates refund 40% of item price, except Little
Legends below max level, which receive the original rarity-based Digi-Data.
Near-cap legends consume only the remaining XP space; max-level duplicates use
the Bits refund. Refund history retains the original reason and timestamp.

Wallet debit, ownership, refund and legend progression are calculated before a
single atomic profile update. The pending rolls and timestamp remain fixed if a
save needs retrying. Failure leaves the purchase pending and the profile intact.
A successful save dismisses confirmation and displays the result. Collection
wrapped text uses BRUTAL's raw Text API so percent signs in odds/descriptions
are rendered literally rather than interpreted as printf formatting.

`export-eggs.cjs` executes the original roll and hatch accounting with controlled
random draws. `--eggs-only` passed 7,694 checks: original odds plus 2,560 complete
transactions across all eight eggs, new/duplicate/near-cap/max/poor wallets,
locked-file rejection, retry, restart and backup preservation. Regression checks
also passed: 24,991 collection checks, 4,660 match reward/tracker comparisons and
24,050 legend match reward comparisons.

The self-contained Windows package passed the extended collection UI smoke:
buy/equip Bukamon, confirm a Little Legend egg, verify its grant or duplicate
Digi-Data and the final balance (91 from the fixture's 491 Bits), then continue
through the five-round Solo match smoke. Duos passed too. The egg screenshot was
inspected, including the corrected percentage labels. Hatch animations, cosmetic
previews/effects, evolution management and Digi-Farm remain unfinished.


### Discovered companion progression coverage

The full cosmetic registry contains 77 legends: the 37 base legends plus 40
companions registered by mystery-register.js. The original reward exporter only
loaded the base legend module. It now uses the complete exported registry, so an
owned, equipped discovered companion receives its own match care and Digi-Data
rather than falling back to the first free legend. Species-specific evolution
reachability uses each companion's evoRoot while keeping its independent record.

The reward fixture now contains 25,025 original sequential updates across all
77 legends. A catalog-order/coverage assertion prevents future omissions. The
native history tests also finish a match with an equipped discovered companion
and verify after restart that only that companion's legend record was created.
This corrects progression coverage; the native evolution UI and farm systems
remain unfinished.

Validation: 50,051 legend checks, 4,660 match reward/tracker checks, 810 history
and persistence checks, and 7,694 egg checks passed. The Windows package rebuilt
and its match UI smoke passed.


### Native Little Legend evolution controls

Owned legends now have an Evolution and care panel in Collection and store.
It shows current form, level, Digi-Data, next level requirement and care-weighted
branch odds. Digivolve, revert, buy the rest of a level and feed use native rules.
Form history, discovered forms, XP, care and the revert nonce retain the original
save format. Wallet and legend changes are committed in one profile replacement.
Evolution rolls use the original legend/level/form/nonce seed; reloads keep the
same outcome and reverting increments the nonce.

The embedded evolution catalog includes 254 forms and companion-specific branch
roots. Odds preserve original branch order, care caps, largest-remainder integer
percentages and the saved Digi-Farm bonuses for training, bond, food, weight,
sickness and neglect. This reads existing farm state; it does not yet simulate
real-time/offline farm progression or provide the farm controls.

`--evolution-only` passed 17,377 comparisons against original JavaScript:
5,365 form/level/odds/roll views and 4,004 actions with result and both save-domain
comparisons. Cases cover all 77 legends, reachable forms, unowned/poor wallets,
invalid-form repair, imported farm care and missing progression records. The
fresh-record cases were added after the packaged UI caught an integer-vs-double
JSON care-value issue that existing-record fixtures did not expose.

`--evolution-smoke --profile-directory <isolated>` clicks through buying and
equipping Bukamon, two level purchases, feeding, evolution, reversion and a second
evolution, then an egg purchase and the normal five-round Solo match test. It
requires a fresh isolated legacy fixture with at least 1,000 Bits. The test passed
with 2,000 starting Bits; disk readback confirmed Gizamon, XP 200, food care 1,
nonce 1 and 1,400 remaining Bits after the egg. Unknown imported ownership was
preserved. The evolution screenshot was inspected and Duos UI checks passed.

Tree visualization, animated form previews, in-match legend rendering, farm
simulation/UI and other production presentation work remain unfinished.


### Native farm time simulation

`FarmSimulation.Advance` now calculates the original farm's resident sync,
equipped-first admission, bank pause behavior, supply refill and local daily
rollover. It simulates sleep/naps, energy, hunger, happiness, discipline, weight,
starvation, sickness, dirt, neglect, toilets, piles and tantrums minute by minute.
Catch-up preserves the 12-hour maximum and two-mistake cap for gaps over 30
minutes. Pile placement and tantrums use the original seeded random function.
Unknown saved fields remain intact and emitted events retain the 200-event limit.

`export-farm-simulation.cjs` executes the original farm with a controlled clock.
`--farm-only` passed 424,718 save/event comparisons over 770 snapshots, covering
all 77 equipped legends, fresh farms, imported state, room capacity, banked pets,
lights, threshold states, fractional minutes, clock reversal and long gaps.
Numeric comparisons allow at most 1e-9 difference; structure and event order are
also checked. Collection (24,997) and evolution (17,377) regressions passed, and
the Windows package rebuilt with Solo/Duos UI smokes passing.

The new fixture exposed a fresh-wallet bug: the default zero was an Int32 JSON
value but the wallet reader expects Int64. The default now uses Int64, with
regressions for absent/empty wallet data and insufficient-balance purchases.

Farm simulation currently returns a pure snapshot and events. It is not yet
scheduled or persisted by the desktop app. Live aging will be enabled with the
native care actions, rewards and room UI; imported farm saves remain untouched
in the meantime. This is a simulation component, not a completed Digi-Farm port.


### Live farm care and atomic rewards

The menu now opens a functional Digi-Farm panel with resident selection, five
foods, room/pile cleaning, bandages/medicine, lights, praise, scolding, petting,
ball play and paired play. It shows pet needs, supplies and daily reward use.
The native care rules preserve refusals, overfeeding, costs, free supplies,
cooldowns, care changes and the 60-Bit/100-Digi-Data daily reward caps. Data rewards
respect the legend XP cap and leave unused daily Data capacity available.

`FarmProgress` catches up existing native profiles at startup and every 30
seconds, including while matches run. Opening the farm explicitly also catches
up and can initialize a fresh profile. Automatic updates do not create a profile
before first import. Care actions calculate farm, wallet and legend changes
before one atomic profile replacement. Failed writes retain the existing state,
report an error and can be retried. Future farm versions are preserved.

`export-farm-care.cjs` executes 5,544 original care transactions across all 77
legends and full/hungry/asleep/tantrum/sick/poor/near-cap states. `--farm-only`
passed 424,718 simulation comparisons and 27,731 care/persistence checks,
including complete result payloads, all saved domains, event lists, blocked-file
rollback, retry, restart, backups and unknown/future data preservation.

The packaged collection smoke now continues through feeding, ball play, praise,
petting and both light states, checking the saved daily rewards, bond care and
supplies before the Solo match flow. That test and the Duos match smoke passed.
The farm screenshot was inspected. The current panel is functional text/buttons;
training games, bank transfers, expansion/themes and the animated farm room still
need completion, as do the broader production port requirements.


### Farm bank, expansion and theme management

The farm's Room, bank and themes view now supports moving residents out/in,
swapping a selected resident with a banked legend, buying capacity upgrades and
buying/selecting all six original themes. Banked time remains paused: returning
pets keep their stats and start a fresh simulation timestamp. At least one
resident stays in the room. Expansions retain the original 250/400/600-Bit prices,
add two places up to the normal 12-resident cap and fill space in catalog order.
Theme ownership includes the original board-linked unlocks, so owning a matching
board avoids charging for its farm theme. Each transaction uses the existing
atomic farm/profile controller.

The source fixture now covers 11,396 care and room transactions, including full
rooms, the last resident, poor wallets, maximum capacity, existing banked pets,
new pets, swaps, free/owned/board-linked themes and invalid IDs. The final
`--farm-only` run passed 424,718 simulation comparisons and 56,993 care/room/save
checks. Invalid theme data is preserved and reported rather than crashing.

The extended packaged evolution smoke also sends residents to the bank, brings
them back, swaps them, expands the room and buys/selects Numemon Sewer. Disk
readback verified capacity 8, restored Tsunomon/Motimon, sewer ownership/selection,
954 remaining Bits and unknown imported ownership. The room controls screenshot
was inspected; the integrated Solo flow and Duos UI smoke passed. The Windows
package was rebuilt after the final validation guard.

Theme selection is persisted but the animated/theme-specific room rendering is
still pending. Training rules and mini-games also remain, along with the broader
native port's multiplayer, presentation and release requirements.

### Farm training outcomes

The native farm controller now applies all four training results using the original
eligibility rules, score thresholds, stat caps, energy/hunger costs, weight changes,
happiness gains and daily Bits/Data limits. Training updates farm, wallet and legend
progress in one atomic profile transaction. A failed write leaves every domain
unchanged; retry and restart are covered by the persistence checks.

The source exporter now streams compressed cases instead of retaining VM objects,
which exceeded Node's heap limit with the expanded coverage. It exports 23,716
care/room/training transactions across all 77 companions. Validation passed
424,718 simulation comparisons and 118,599 care/result/save checks. The Windows
self-contained package built successfully; Solo and Duos match UI smokes passed.

This checkpoint implements training outcomes only. Power Swing, Beat Dash, Data
Memory and Endurance Run still need their native gameplay and input interfaces.
The full port remains in progress; this is not a production release.

### Playable native farm training

Power Swing, Beat Dash, Data Memory and Endurance Run are now playable from the
farm panel using native BRUTAL ImGui drawing and keyboard/mouse controls. The
frame-driven C# models preserve swing windows, note scoring, deterministic memory
sequences and endurance drain. Memory callbacks use wall time separately from the
clamped animation step, including after frame stutters. The panel includes the
countdown, feedback, score, stat/reward receipt, repeat and quit controls. Quit
preserves the current browser game's zero-score training result behavior.

Training eligibility is shared with the existing care rules. A finished result
uses the atomic farm/profile transaction; failed writes retain the result for an
explicit retry without rerunning the game. Periodic farm saves do not run over a
pending training result. The result screen exposes all gained stats, Bits and
Digi-Data. This remains the functional native UI, with production visual polish
and the animated farm environment still pending.

`export-training-games.cjs` executes the original factories with deterministic
timers and a minimal DOM. Its 80 runs cover four frame intervals, idle, automatic,
rapid, sparse and stuttering input schedules: 52,777 frames and 360,359 native
comparisons passed. The full farm checks also passed (424,718 simulation and
118,599 transaction/persistence checks). Both targeted suites are included in
the regular native build/test workflow.

The self-contained Windows build passed `--training-smoke` on a fresh isolated
profile: all four games are played through real ImGui keyboard/mouse events,
results are reopened from disk, a locked save is retried, and quit is checked.
Solo and Duos match UI smoke tests passed. Training screenshots and result screens
were inspected. To run the training UI check, supply a new, empty
`--profile-directory` and optionally `--capture`; existing trained pets are not
suitable test fixtures. This checkpoint does not constitute the completed port
or a production release.

### Companion model assets and visual selection

The model package now includes the 32 dedicated Little Legend GLBs that were
missing from the battle-model export. The expanded catalog contains 750 assets
and 937 prepared sources. Dedicated companions retain every authored animation;
they do not pass through the battle-model fallback-clip generator. Texture
conversion, integrity hashes, meshopt decoding and prepared rig checks apply to
the added files through the existing asset pipeline.

`CompanionCatalog` resolves all 77 base companions and the original accepted
`id@form` keys, including retro and discovered companions. Its generated metadata
retains model source, height, clip-role mappings and procedural reaction mappings.
The browser's reachable-form/parser checks determine the 188 accepted explicit
form IDs; an invalid or unreachable suffix falls back to the base companion.
There are 14,898 original selection cases, including unknown IDs and invalid
suffixes. Dedicated static Upamon remains static in the authored rig; its exported
procedural policy is available for the forthcoming companion animator.

Validation passed 1,383,730 model/accessor checks, 1,175 texture pixel checks,
1,364,128 prepared-motion checks, 59,701 companion checks, 2,477,248 action/mixer
checks, 26,778 material checks, 76,359 mesh checks and 2,748,100 skeletal checks.
The existing corrupt-clip repair path remains covered. The self-contained Windows
package passed `--companion-model-smoke`, rendering all 77 base companions through
the native GPU renderer with visible-pixel checks for every model. The combined
preview was inspected. Solo and Duos match UI smoke tests also passed.

This supplies the original assets and selection policy for farm, evolution and
match companions. It does not yet implement the companion reaction animator,
animated farm room, themed decorations or in-match companion presentation. Those
remain part of the full port, along with the other production acceptance work.

### Companion reaction plans and procedural motion

`CompanionMotion` selects the original authored reaction/dance steps, including
item overrides, ordered fallback clips, loop counts, speed and procedural-only
fallbacks while moving or without a mixer. It also evaluates all thirteen original
procedural effects: spin, twirl, hop, sway, sway2, bounce, wave, droop, slump,
wiggle, bob, idle and shake. The resulting position, Euler rotation, scale and
matrix retain the original Three.js XYZ rotation convention.

The new exporter executes the original `stepsFor`, `clipOf` and FX functions.
Across all base and explicit-form visual policies it produced 7,950 reaction
plans and 1,872 transform samples. All 54,750 native comparisons passed, including
matrix components and static/clipless fallbacks. The suite is included in the
regular build and available through `--companion-motion-only`. The self-contained
Windows package also builds successfully.

This is the selection/math layer for the companion animator. Timed step playback,
clip blending, walking transitions and connection to the animated farm scene are
still pending; no claim of completed farm presentation or production release is
made by this checkpoint.

### Timed companion animation playback

`CompanionAnimator` now executes reaction sequences with their original durations,
loop counts and speed, blends idle/run/reaction skeletal tracks, clamps finished
clips, handles interrupted reactions and smooths facing changes. Procedural
movement overlays are applied alongside clip playback; static companions receive
the original idle motion when standing without an active reaction. `GpuModel`
provides the companion height/footprint placement used by the GPU path.

The reference exporter runs the original LegendView methods and Three.js mixer
against the prepared models. All 265 base/form visual configurations were checked
through idle/walk transitions, every reaction, multi-step dances, facing changes
and rapid interruptions. The 2,909,021 comparisons of sequence state, action time,
weights, enabled/paused flags, transforms and finite source bone matrices passed.
Nonfinite matrices from the existing corrupt source clip are excluded from this
reference comparison; native repair coverage remains in the skeletal suite.

The Windows package was rebuilt. The companion GPU smoke now uses the new
controller and placement, rather than a single sampled idle pose. All 77 base
companions rendered visibly after walking, reaction and dance operations; the
combined animated-pose capture was inspected. The new targeted check is
`--companion-playback-only`, and its exporter/check also run in the regular build.

Farm movement, themed room construction, care-event choreography and the farm UI
still need to connect to this controller. The full native port remains active and
this is not a production release.

### Native farm room integration

The farm now displays its room alongside the care controls. `FarmView` renders the
selected theme, animated resident models, current evolved forms, selectable name
labels and the remaining free-meat tray. It synchronizes admissions, bank removals
and form changes while retaining existing resident positions. Care actions trigger
companion reactions only after their save succeeds. The existing full-width
training panel remains available over this view.

The room exporter executes the original procedural builders in an isolated
browser and bakes their mesh groups, transforms, colors and canvas texture pixels.
All six themes are included: meadow (208 meshes), tropical (260), snowy (219),
sewer (161), toy (196) and grid (151). Each includes its six original canvas
textures, care stations, fence and theme decorations. Native playback does not
execute JavaScript. Geometry and textures use the BRUTAL GPU renderer.

The Windows package passed the new `--farm-room-smoke` for all six themes, with
resident, selected-theme, draw-call and visible-surface checks. Theme screenshots
were inspected. The integrated evolution/farm UI smoke now checks that the room
actually renders the selected sewer theme after bank operations and expansion;
that flow, the four training games and Solo/Duos match smokes passed after the
final rebuild.

This is the first integrated room rendering pass. Residents currently retain
arrival positions while their idle/reaction animations play. Wandering, sleeping,
friend interactions, pile visuals, moving water/decorations, snowfall, night
lighting, shadows, material polish and improved overlapping labels remain to be
completed. Snow particle data is exported but not rendered yet. These limitations
remain part of the active full-port goal; this checkpoint is not a release gate.

### Farm resident choreography

Residents now wander, approach friends, react to mood and sickness, separate
when crowded, walk to one of six bed slots, sleep with a breathing pose, and wake
when their saved needs change. Overflow sleepers retain their position. Models
without a walk clip use the original hopping motion. Movement is presentation
state and does not mutate saved needs. Newly admitted pets with no asleep flag
are treated as awake, matching the original game.

The seeded original-JavaScript fixture covers 18 combinations of population,
frame interval and random seed, with 600 frames each. All 817,200 state and
animation-command comparisons passed. The rebuilt Windows app also passed all
six room themes with sleep/wake assertions, the integrated evolution/farm/Solo
flow, four training games and the Duos match smoke. Awake and sleeping meadow
captures were inspected.

This supersedes the stationary-resident limitation above. Care-event routing,
pile visuals, moving room decorations, snow, night lighting, shadows, material
polish and label improvements remain part of the active full-port goal.

### Farm care movement and reactions

Successful feeding now pauses resident movement for the original eating period.
Petting faces the camera, respects sleep and tantrums, and preserves a longer
existing animation hold. Pair play routes both residents to their shared midpoint
before playing their reactions; solo play uses the dance animation. Training
routes residents to the exercise station and displays the score-based reaction
once its result has been saved. Praise, scolding, healing, feeding refusal and
room cleaning use the original result-specific animations. Failed profile writes
do not trigger these reactions.

The 817,200 original movement comparisons still pass, together with 12 care
transition assertions covering movement interruption, sleeping pets, pair arrival
and training arrival/results. The rebuilt Windows app passed the integrated
farm/evolution/Solo flow and all four training games, including save retry.
Food props, care bubbles and audio, simulation-event toilet routing and pile
visuals remain to be integrated along with the previously listed room effects.

### Animated room decorations

Room schema 2 preserves dynamic ancestor transforms for water, tropical palms,
sewer drips and grid crystals. Native playback evaluates the original motion
formulas, retaining static parent transforms and each descendant mesh offset.
This also moves all branches and coconuts with their palm instead of treating
individual exported meshes as independent objects.

The exporter samples the original room callbacks at seven times from zero to
123 seconds. All 14,224 world-matrix component comparisons pass, alongside the
817,212 resident/care checks. The Windows build passed the six-theme GPU smoke
and sleep/wake checks; the tropical render was inspected. Snowfall still requires
native particle rendering. Night lighting, shadows and material fidelity remain
open parts of the full port.

### Native snowfall

The snowy room now renders its original 260 square snow particles in one GPU
batch, with camera-facing geometry matching the original perspective point size
and opacity. Particle positions retain float32 rounding, per-particle fall speed,
sinusoidal drift and the original reset height. Theme changes dispose the snow
batch; other rooms contain no snow. Reference snapshots live in a test fixture
rather than the distributed room content.

All 546,000 particle components match 700 frames of original JavaScript output,
including varying frame intervals and repeated particle resets. The combined
farm movement, care, decoration and snow checks total 1,377,436 passes. The rebuilt
Windows app passed all six room GPU checks, including theme-specific particle
counts and sleep/wake transitions. The snowy render was inspected. This closes
the snowfall rendering limitation above; lighting, shadows, materials and other
remaining full-port requirements are still open.

### Farm day/night lighting and emission

Farm rendering now uses the original hemisphere, sun and lamp colors, positions,
intensity transitions and lamp distance/decay. The saved lights flag controls
night mode. Exported room materials retain linear emissive colors and intensity;
the lamp bulb switches between its original day and night emission. Residents
receive the same room lighting. Other renderers retain their existing lighting.

All 4,320 reference intensity comparisons pass across theme and day/night
changes, bringing the combined farm checks to 1,381,756. The Windows build passed
all six room rendering and night/sleep checks plus the Duos match smoke. Meadow
day/night and grid night captures were inspected.

This is the diffuse and emissive lighting pass. Specular/metallic material
response, shadow maps, the sky gradient/night overlay and complete original
material fidelity remain unfinished and are still required for the full port.

### Room roughness and metalness

Exported room materials now retain their original roughness and metalness.
The farm shader uses the bundled Three.js isotropic GGX distribution, correlated
Smith visibility and Schlick Fresnel approximation for direct sun/lamp specular
lighting. It retains the original minimum roughness and derivative-based geometry
roughness, and removes the metallic portion from diffuse lighting. Camera
position is provided explicitly for view-dependent highlights.

The rebuilt Windows app compiled the shader and passed all six day/night room
smokes plus the Duos match smoke. Sewer, tropical and grid-night renders were
inspected. Existing farm motion/light reference checks remain green (1,381,756).
These checks do not establish pixel-identical shading. Companion material
roughness/metalness, shadow maps, sky/night overlays and broader visual acceptance
remain open work under the full-port goal.

### Companion surface lighting

Farm companions now use their corrected material roughness and specular intensity
in GGX lighting. The renderer samples roughness from the green channel and
specular intensity from alpha, with separate linear GPU textures so these data
maps cannot accidentally share an sRGB decode with color maps. It preserves the
original game-rip correction that forces imported models to non-metallic.

The rebuilt app passed the six-theme farm smoke, all 77 base companions under
farm lighting, and the Duos match smoke. The 77-companion capture was inspected.
Companion normal maps and authored emission still require rendering support;
this checkpoint does not claim complete material or visual parity.

### Imported model emission

The material loader and renderer now preserve emissive RGB, the emissive-strength
extension and emissive color textures. Emissive textures use sRGB decoding, and
emission is added in linear color before display conversion. Unlit materials
continue to follow their original loader behavior.

The refreshed original-loader fixture passes 32,943 checks across 1,233 materials,
including emission values and bindings. Catalog inspection found zero nonzero
emissive materials and zero emissive maps after original loader corrections, so
this does not change the current model appearance. The Windows package passed
77-companion rendering, all six farm themes and the Duos match smoke; the companion
capture was inspected. Nonzero emission rendering is implemented but not proven
by those current-asset smoke checks. Normal maps and shadow rendering remain open.

### Model normal maps and tangents

Catalog inspection identified one corrected material with a normal map:
Vulcanusmon. Its mapped primitive has authored tangents. The mesh loader now
preserves tangent vectors and handedness in a separate vertex stream, and the
shader skins and transforms them before applying the linear normal texture and
its scale. A derivative tangent frame is available for mapped meshes without
an authored tangent stream. Geometry roughness remains based on the unperturbed
surface normal. The GPU smoke includes Vulcanusmon in addition to 77 companions.

The original-loader checks passed for 34,176 material values, 84,531 mesh values
(including tangent samples), and 2,748,100 skeletal values. The Windows package
passed the 78-model GPU smoke, all six farm rooms and the Duos match smoke; its
model capture was inspected. These checks establish asset/animation consistency
and successful rendering, not pixel-identical normal-map shading. Shadow maps
and broader visual acceptance remain required parts of the full-port goal.

### Farm sun shadows

The farm now renders a 1024-square depth map using the original sun position,
orthographic bounds, near/far planes and -0.0015 comparison bias. Exported meshes
retain cast/receive-shadow flags. Resident shadows follow current skeletal poses,
movement and sleep transforms, with alpha-cutout materials respected. The map
uses the bundled Three.js PCF-soft sampling pattern and attenuates only direct
sun illumination. The shadow framebuffer restores prior read/draw bindings.

All six Windows room smokes pass and now verify both caster draws and a visible
pixel difference when shadows are disabled. Meadow, tropical and grid captures
were inspected, including a final soft-shadow meadow capture. The integrated
profile/evolution/farm/Solo flow and Duos match smoke also passed.

Shadow rendering is implemented, but exact original shadow-side/culling behavior
and broader pixel-level visual acceptance remain to be audited. Sky/night
overlays, care props and feedback, production UI, multiplayer and release gates
remain part of the active full-port objective.

### Material facing and shadow sides

Room export now preserves material side and explicit shadow-side settings. Native
rendering applies front/back/double-sided culling and the original shadow-pass
side reversal, including imported companion meshes. Static room draw calls also
set winding from their transform determinant, preventing prior actor state from
changing room culling. The pass restores the conventional back-face setting.

Visual review exposed a previously hidden billboard convention mismatch: native
billboards faced local -Z while the original sprite planes face +Z. Their
orientation is now corrected, retaining the care-station icons with culling on.

All six farm room checks (including visible shadow differences), the 78-model
GPU check and Duos match smoke passed. After the billboard correction the Windows
package was rebuilt, the six-theme smoke rerun, and the tropical capture inspected.
This addresses the shadow-side limitation above. Broader visual acceptance and
the remaining full-port requirements are still open.

### Farm atmosphere layers

The farm now draws its theme sky gradient and original eight star positions
behind the 3D scene, then composites the original blue radial night tint over it.
The tint interpolates premultiplied CSS gradient colors and uses a 0.8-second
CSS-ease fade. Layers are restricted to the room viewport and leave the adjacent
care controls unchanged. The original opaque water can obscure the background
sky and stars at some camera sizes, as in the source layering.

The Windows six-theme smoke now verifies night-opacity fade-in, an intermediate
fade-out value, and full return to day. It passed with the existing shadow and
sleep/wake checks. The integrated farm/evolution/Solo flow passed; night meadow
and integrated farm captures were inspected. Exact CSS interrupted-transition
shortening and device-scale star sizing still need visual fidelity review.
Broader UI, care feedback, multiplayer and release acceptance remain unfinished.

### Care sounds and manual play selection

The default Play action now selects the nearest awake, healthy room companion
before saving the action, preserving the original last-resident tie rule. It
falls back to solo play only when no eligible partner is present. Explicit
partner controls remain available. Clicking a resident name now selects and
pets it, matching the original scene interaction.

Saved care receipts drive original sounds for feeding/refusal, cleaning,
healing, lights, praise, scolding and play/petting. Failed profile writes cannot
trigger feedback. Existing training result sounds remain unchanged.

The combined farm suite passes 1,381,772 checks, including nearest-partner ties,
sleep/sickness eligibility, busy partners, and success/refusal sound selection.
The rebuilt Windows app passed the integrated farm/evolution/Solo flow and audio
smoke. Food props, bubbles, rewards and simulation-event presentation remain
unfinished parts of farm fidelity; the full-port goal remains active.

### Selected resident ring

The selected farm resident now has the original pulsing gold ring: 40 segments,
0.42/0.52 inner/outer radii, 0.9 opacity and a 0.03 ground offset. It follows the
resident position, uses the shared animation clock, respects scene depth and is
tinted by the night overlay. Selection is synchronized from the care controls
before rendering; missing or banked residents do not retain a ring.

The Windows build passed all six room GPU checks, including selection visibility
and missing-resident removal, and the integrated farm/evolution/Solo flow. The
meadow capture was inspected with the ring visible under the selected resident.
Remaining care props, bubbles and event feedback are still part of the active
full-port goal, along with the broader UI, multiplayer and release work.

### Feeding props

The room exporter now also executes the original food builders for meat, carrot,
giant meat, sirloin and cake. Native feeding displays those meshes in front of
the resident using its current facing, at the original height and offset. Props
last 1.6 seconds, shrink over the final half-second, and preserve giant-meat scale.
They participate in room lighting and their original shadow flags. Geometry is
cached by food type and disposed with the farm view. Refused or unsaved actions
do not create food props.

The rebuilt Windows six-theme smoke covers all five food types, refusal, initial
appearance, shrink-period lifetime and expiry. Carrot and cake captures were
inspected. The integrated profile/evolution/farm/Solo smoke also passed. Care
bubbles, reward feedback and simulation-event/pile presentation remain open,
along with the broader full-port objective.

### Farm pile presentation

Saved resident waste piles now render at their saved positions using the original
four visible meshes, material and shadow flags. They use the original grow-in
overshoot, neglected-pile wobble, and rising/shrinking cleanup animation. Banked
residents no longer contribute visible piles. Food and pile geometry share the
prop cache and draw path. Scene cleanup targets call the existing saved CleanPile
action for the pile owner without changing the selected resident.

The Windows six-theme smoke verifies two saved piles, projected cleanup targets,
removal animation and expiry in every theme. The meadow pile render was inspected.
The integrated profile/evolution/farm/Solo flow passed. Scene targets currently
use projected 28-pixel hit areas; exact mesh raycasting and an automated pointer
cleanup check remain pending. New simulation-event placement near the resident,
toilet routing, bubbles and reward feedback remain open farm fidelity work.

### Farm scene raycasting

Scene clicks now intersect the original closed polygonal hit cylinders: eight
sides for piles and ten for residents. Picking accounts for pile growth/wobble,
resident movement, breathing and sleeping tilt, and chooses the nearest hit
across both kinds. Resident hits select/pet; pile hits clean the owning resident
without changing selection. Care-control/name-button clicks retain priority.
Removed piles stop accepting hits immediately while their exit animation plays.

All 5,712 raycast fixtures from Three.js pass across scales and rotations. The
combined farm checks total 1,387,484. The rebuilt six-theme GPU smoke verifies
projected pile-center hits and removal from picking; the integrated farm/evolution/
Solo flow passed. This replaces the prior fixed 28-pixel targets. A full automated
pointer-to-persisted-cleanup UI check remains pending, along with simulation-event
feedback and the broader full-port requirements.

Farm pointer integration check: run `DigiTactics.Desktop.exe --farm-pointer-smoke --profile-directory <new-empty-directory> --capture farm-pointer.bmp`. The hidden native window sends ImGui mouse events to a rendered pile owned by a nonselected resident. It checks duplicate pile keys across owners, a locked-save failure followed by retry, persisted cleanup/reward after reopening the profile, and unchanged selection. Existing profiles are rejected before fixture setup.

Farm simulation events now flow through FarmProgress only after their snapshot commits. Live pile coordinates are saved beside the resident in the same transaction; offline catch-up retains the original simulation positions. The room handles toilet routing/arrival, tantrum animation, care notices, and an away summary. FarmEventChecks covers failed-save suppression, retry, one-time draining, reloaded coordinates and events generated by care actions; --farm-only and --farm-choreography-only pass. Floating event bubbles remain part of the presentation work still to complete.

Saved farm rewards now show a gold Bits/Data popup after 350 ms, with the original 1.4-second ease-out rise/fade and 1.5-second removal. Positions are captured at display time rather than following later movement. Name and reward anchors use companion height and sleeping posture. Farm event checks cover popup suppression, timing, text, fixed position and expiry; the pointer smoke capture verifies the rendered reward after a failed-save retry. Mood icons and non-reward event symbols remain outstanding.

Farm mood bubbles now follow the original ten-state priority rules, with 10000 generated source comparisons. Color symbols are exported from the Windows browser glyph rendering into a tracked atlas; no browser is used at runtime. Native bubbles bob above companions, show descriptive hover text, and suppress unselected/unhovered name labels. The care panel also names the selected mood. The rendered pointer smoke includes hungry/happy residents and passes with the atlas loaded. Remaining farm presentation includes non-reward event symbols and final layout/transition fidelity.

FarmPops now renders care and event symbols as well as delayed saved rewards. The tracked 18-symbol atlas covers all foods, refusal, medicine/bandage, praise/scolding, sleeping pets, piles, mistakes, tantrums, toilet arrival and play music. Toilet/play feedback is emitted from choreography at arrival; spontaneous tantrums also display their symbol. Care symbols follow saved receipt flags, while small repeat pets suppress hearts. Targeted symbol/lifecycle tests, original farm choreography comparisons, Windows publish and the evolution/farm/match UI smoke pass; farm-feed capture confirms native atlas rendering.

Native networking now has a separate DigiTactics.Networking assembly. NetworkProtocol ports the version-9 message schema and bounded parser from js/net-guard.js: every lobby, presence, snapshot, log, intent and effect message; action arguments; Steam ID bounds; payload length, nesting, node and string limits. The exporter compares 18461 original parser results, plus native oversized-payload and malformed-Unicode checks (`--network-only`). Malformed unpaired Unicode escapes are rejected safely by the native parser. This is the validation foundation only: ordered-log replication, snapshot recovery, lobby authority, Steam API transport and UI integration are still required before multiplayer is available. The native app does not advertise network compatibility yet.

ReplicatedLog now implements host-authoritative ordered entries, throttled gap requests, conflict resets, head reconciliation, bounded pending entries and atomic snapshot assembly. It rejects wrong-room/nonhost messages and stale snapshot requests; partial snapshots retain the current log, timeouts drain buffered live entries, and a new expected host can replace an incomplete old-host snapshot. The --network-only suite includes 2160 source-extracted original log transitions (8640 state checks) plus 19 authorization/recovery cases. Windows publish passes. Room authority must supply the expected host and schedule snapshot retry/timeout calls; Steam callbacks, lobby integration and match replay remain outstanding.

LobbyAuthority now owns validated lobby identity, Steam-supplied membership, atomic seat moves, seat release, bans, presence updates, intent ownership and successor election after the original six-second grace. It returns copies of mutable lobby/presence data. Tests compare 2700 source-extracted seat/successor decisions and exercise unauthorized actions, host changes, bans and presence timestamps. Native intent replay suppression additionally handles sequence zero strictly (the JS truthy fallback treated prior zero as missing). --network-only passes 35239 checks and Windows publish succeeds. A room controller still needs to connect authority, replication, callbacks and outgoing messages before Steam play can be enabled.

NetworkRoom connects lobby authority and replicated history to authenticated message routing, outgoing packets, seat/presence requests, intents, effects, initial match data, settings updates and bans. It includes per-peer rate buckets, snapshot retry/timeout, seat response timeout, host-clock estimates and ordinary host-departure succession. A three-client in-memory test exercises join, seats, presence, settings, effects, impersonation rejection, dropped-log repair, reconnect snapshots, host departure, continued history and bans. All 35271 networking checks and Windows publish pass. Steam callbacks/native transport and game replay/UI are not connected yet; restarted-host recovery while the old host remains a member and lobby removal/limit handling also remain to be completed.

Room recovery now supports a host that retains its lobby identity but loses match init/history while remaining a Steam member: it authorizes the seated successor, confirms that handover to other peers, and restores the successor snapshot. A healthy host uses its local copy instead of provoking a takeover. Host closure cancels pending requests and blocks late traffic; reaching the 32768-entry limit closes the lobby without overflowing history. Three-client restart/closure tests and the exact log-cap boundary pass. --network-only totals 35283 checks; Windows publish passes. Steam-backed identity bootstrap, API callbacks, transport and match/UI integration remain required for playable multiplayer.

SteamMessageTransport now binds Windows x64 ISteamNetworkingMessages directly from an initialized Steam API module, with the original identity/message layouts, channel and send flags. It requires relay-only configuration, checks membership and blocked peers, bounds message count/bytes before copying, and releases each native message before dispatching application callbacks. Invalid UTF-8 is rejected. Run native/test-steam.ps1 to compile the existing fake Steam DLL and run 16 ABI/buffer/release/session/traffic checks; no live Steam account is needed. Windows publish passes. The owning Steam client (initialization/shutdown, callbacks, matchmaking and identity bootstrap) and its connection to NetworkRoom still need implementation; this transport is not yet enabled in the game UI.

SteamClient now owns InitFlat, identity interfaces, manual callback dispatch, async call results, message transport and orderly shutdown. It enforces one owner per process and game-thread callback dispatch, frees callback storage before user handlers, limits work per frame, and restores development AppID environment overrides after success/failure. native/test-steam.ps1 passes 31 transport/lifecycle checks, including partial initialization failure. A live Windows smoke against the installed steam_api64.dll successfully initialized app 480, obtained identity, configured relay-only networking and dispatched 20 callbacks before shutdown. Reproduce with `DigiTactics.Desktop.exe --steam-client-smoke --steam-api <full-dll-path>`. Matchmaking and NetworkRoom/game/UI integration remain unfinished. API lifecycle reference: https://partner.steamgames.com/doc/sdk/api .

### Native Steam matchmaking checkpoint
SteamMatchmaking now creates/joins/leaves lobbies, lists protocol-compatible rooms, reads members/owner/metadata, validates owner-only metadata and ownership changes, and dispatches membership/data/invite callbacks on the game thread. Requests complete through manual Steam call-result dispatch with bounded waits. native/test-steam.ps1 passes 46 transport/lifecycle/matchmaking checks. A live private app-480 lobby smoke passed creation, ownership/membership, metadata write/read and leave; reproduce with --steam-lobby-smoke --steam-api <full-dll-path>. No invitations are sent. NetworkRoom/game/UI integration remains unfinished. Callback reference: https://partner.steamgames.com/doc/api/ISteamMatchmaking .

### Steam room connection checkpoint
SteamRoomSession now binds one native NetworkRoom to Steam membership, queued relay messages, discovery metadata and ownership handoff. Joining waits for an authenticated snapshot; timeout/cancellation, lost membership, closure and switching rooms clean up sessions and room-scoped bans. It accepts verified members each second, matching the original bridge's handling of incoming relay requests. native/test-steam.ps1 passes 62 checks including native peer packet/seat routing, nonmember rejection, reconnect snapshots, timeout and cancellation cleanup. The 35,283 network contract/controller checks pass. A live private app-480 --steam-room-smoke passed creation, host/seat, metadata, closure and leave. This is connection infrastructure, not completed multiplayer gameplay: lobby UI, lockstep match/replay integration, friends/personas and end-to-end multi-account acceptance remain unfinished.

### Native lockstep rules checkpoint
LockstepMatch ports js/lockstep.js shared-log application, votes, phase/turn timing, host phase decisions, watched-combat duration and the original digest/FNV hash. It uses the existing native rules engine and excludes ghost opponents from watched-player decisions. Duration queries simulate a separate combat copy. export-lockstep.cjs generates 44 reference traces spanning every pickable set, normal/hyper pace and Solo/Duos; --lockstep-only passes 34,796 original-engine comparisons including wrong-phase/duplicate entries, action results, clocks, votes, decisions and hashes. The fixture is part of native/build.ps1 and the full test entrypoint. Reconnect log replay, desync handling, host intent scheduling and multiplayer screen integration remain to be connected.

### Multiplayer match replay checkpoint
MatchReplica connects authenticated NetworkRoom history to LockstepMatch. It constructs the exact initialized variant/teams, applies new entries once, rebuilds after replacement snapshots or conflicting history, and checks host hashes before live guest application. Replayed actions do not emit live effects. Desync replay is limited to once per minute unless history is explicitly replaced; persistent replay disagreements are reported. Twenty-one integration checks cover reconnect, repaired/persistent desync, cooldown, snapshots, effect suppression, and equivalent JSON numeric/legacy falsy format inputs. All 34,796 original lockstep comparisons still pass; Windows self-contained publish succeeds. Host scheduling and multiplayer UI remain unfinished.

### Native multiplayer host scheduling checkpoint
MatchHostController orders verified seat-owner intents through the shared log at the original 200ms cadence. It bounds queues and per-player input rate, prioritizes phase deadlines, stamps next-round hashes, logs human/AI seat changes, releases offline seats after twenty seconds and continues from a surviving host's replica. Rebuild/host changes clear transient input and combat-completion state. Twenty-five integration checks exercise the actual opening carousel and subsequent ready/combat/result flow, stale/unauthorized input, queue/rate limits, deadline priority, return-to-AI, reclaimed seats, disposal and host succession. The 21 replica checks still pass and Windows self-contained publish succeeds. Multiplayer screen/game presentation integration is still outstanding.

### Rendered multiplayer match checkpoint
The existing BRUTAL MatchScreen can now attach a NetworkRoom/MatchReplica/MatchHostController, route its controls through host intents, follow logged phase transitions and rebuild its visual match after replay. Local seat selection no longer assumes seat zero; online combat completion reports done instead of settling rules locally. --network-match-smoke exercises two in-process room controllers through real ImGui clicks on a rendered seat-one guest, checks 3D planning/combat, ready/settlement/next round and final host/guest hash equality. It passes in Solo and Duos; local --match-smoke also passes in both formats after the refactor. Captured online combat was visually inspected. This test uses in-process packet routing, not two live Steam accounts. Public lobby browsing/creation screen, online reward/history recording, spectators and complete multi-account acceptance still need integration; the attachment is not yet exposed as a completed menu flow.

### Steam lobby menu checkpoint
The native main menu now exposes Steam connection, compatible room discovery, room creation with ruleset/pace/format/visibility, seat selection, host start and leave. Steam events and tasks are pumped on the rendering thread; a seated initialized room attaches the replicated 3D match. Hosts must occupy a seat before starting. Supply --steam-api <absolute steam_api64.dll path> for development, or place the library beside the executable. --steam-lobby-ui-smoke forces private visibility and passed against live Steam app 480 using actual ImGui clicks through connect/create/free-seat/take-seat/start/leave/back. Lobby capture was visually inspected. Local match and rendered network Duos checks still pass. Friends-room discovery/avatars, spectator flow, online history/rewards and full multi-account acceptance are still unfinished; this is not a release candidate.

### Online match progress checkpoint
History/reward tracking now accepts the actual local seat and preserves the original mp:seed:seat identity. NetworkMatchHistory reconstructs statistics and the last living board while MatchReplica replays history, without replaying visual effects or saving intermediate results. Verified live settlements and completed rebuilds save through the existing atomic profile transaction; reconnects return cached awards without a second payout. A persistent hash disagreement suppresses uncertain payouts. The rendered online match now uses this recorder and its save-retry UI. --network-history-only passes 240 checks across Solo/Duos for seat-one placement, replay equality, all saved domains, duplicate reconnect, locked write/retry and desync suppression. Existing history (810) and replay (21) checks pass; Windows publish and rendered online Duos pass. Recent-player lists and broader multi-account/reconnect acceptance remain outstanding.

### Friends-only room discovery checkpoint
SteamFriends reads immediate Steam friends' current app/lobby and persona name, requests lobby metadata, and completes on the game-thread pump after matching LobbyDataUpdate callbacks or a bounded 1.5-second timeout. Failed/stale metadata is excluded. The lobby UI merges compatible friends' rooms with public results, deduplicates room IDs and shows the friends present. native/test-steam.ps1 now passes 70 transport/client/matchmaking/room/friends checks; Windows publish and the live private-lobby UI flow pass. Discovery does not send invitations or messages. API references: https://partner.steamgames.com/doc/api/ISteamFriends and https://partner.steamgames.com/doc/api/ISteamMatchmaking . Avatars, spectator flow and live multi-account acceptance remain unfinished.

### Steam avatar checkpoint
SteamFriends now requests unknown persona information and reads medium avatars through the native image-size/RGBA APIs. It rejects empty/oversized images and failed reads, preserves RGBA channels and caches up to 64 image handles. Lobby seats upload avatars to bounded GPU textures with two-second refresh; leaving defers texture disposal until the next frame so queued ImGui draw commands stay valid. native/test-steam.ps1 passes 77 checks including pending download, bounds, failed read and cache reuse. Windows publish and live private-lobby UI smoke pass; the captured live avatar was visually inspected. API references: https://partner.steamgames.com/doc/api/ISteamFriends#GetMediumFriendAvatar and https://partner.steamgames.com/doc/api/ISteamUtils#GetImageRGBA . Spectators, recent-player records, full multi-account acceptance and production UI polish remain outstanding.

### Native spectator checkpoint
Active Steam rooms now attach unseated users as spectators. Spectators can scout the 3D board and standings, take an available living seat, or return from a seat to spectating without leaving the room. Seat changes rebuild the replica/presentation and bind progress only for a controlled seat. Shop/ready/loot/pick controls are hidden or disabled while spectating; no spectator player intent or completion vote is sent. --spectator-smoke passes through spectator -> seat one -> shop/ready/combat -> spectator with real ImGui input and final host/guest hash equality, in Solo and Duos. Local match smoke also passes; Windows publish succeeds and spectator capture was inspected. Real multi-account acceptance, recent-player recording, cosmetics/network effects, production UI/audio polish and release packaging still remain.

Recent-player persistence checkpoint (2026-10-18): native Steam rooms and completed online matches now remember other human players in the existing dt.client.v1 recent list. The native lobby exposes that history. Names/self filtering, stable ordering, the strict one-minute refresh interval, and the 20-entry cap match 37 extracted original JavaScript scenarios (117 checks). Multiplayer history, rewards, and recent players commit atomically; reconnects do not refresh history and failed writes retain their initial timestamp. Online-history checks: 248; local-history regression checks: 810. Windows self-contained publish and actual private Steam lobby UI connect/create/seat/start/leave smoke passed. Full production UI, remaining social effects, multi-account Steam acceptance, and release packaging gates remain outstanding; this checkpoint is not the completed port.

Late-reconnect progress checkpoint (2026-10-18): fixed a reproduced mismatch where rebuilding a complete multiplayer log recorded the final match round for players eliminated earlier. MatchHistoryRecorder now freezes the first placement, final living board, reward statistics, timestamp, and encountered humans without writing; NetworkMatchHistory commits only after a verified rebuild or live settlement. Full Solo and Duos matches compare every saved domain for all eight seats between live elimination and complete-log recovery, including a later seat/name change and a check against intermediate replay writes. Online-history checks: 584; local-history checks: 810; replica/desync checks: 21. Windows self-contained publish and rendered Solo/Duos network-match smoke passed. This remains a development checkpoint, not release completion.

Steam timeout recovery checkpoint (2026-10-18): timed-out create/join requests retain bounded callback tracking so a later native success leaves the abandoned lobby. Cleanup waits for a newer join to the same lobby and preserves accepted membership. Search timeouts still release the search slot; unresolved native operations are capped before issuing another call. A supplied monotonic clock makes the 30-second boundary deterministic in native-DLL tests. Steam checks: 90, covering late create/join successes, failures, retry races, existing membership, request bounds, and the deadline boundary. Windows self-contained publish and real private Steam lobby UI connect/create/seat/start/leave passed. Multi-account gameplay/recovery acceptance and the broader production/release requirements remain outstanding.

Native soundboard checkpoint (2026-10-18): equipped six-slot clips now play in local and online matches through ephemeral room effects, with G/1-6 controls, per-player limits, unknown-clip rejection, spectator restrictions, and 2.3-second labels in the standings. Audio has a separate soundboard bus and persisted volume/mute-others controls. Offline audio export now includes original clip durations for the original 45% music duck with 50ms attack and 400ms recovery. Verified 15 soundboard behavior checks, 21 settings persistence checks, all 114 exported PCM assets with SDL playback/duck checks, Solo/Duos rendered network input with exactly-once peer effect receipt, spectator regression, and Windows self-contained publish. Captures visually checked; the activity display was moved out of the shop area. Emote art/animation, broader production UI, per-player social moderation, multi-account acceptance, and release packaging remain unfinished.

Audio preference migration checkpoint (2026-10-18): first native use now applies imported dt.audio.v1 volume/mute controls, including soundboard preferences, with dt.sound legacy-mode fallback. Existing native choices take precedence. Original archive values and unknown audio fields (including custom music source selection) remain preserved. Settings follow --profile-directory instead of always using the global profile. Importing while the app is open updates the mixer; failed slider writes remain dirty and retry after contention instead of being discarded. Verified 40 storage/settings checks, 32 profile preservation checks, a real mixer binding smoke covering live import, lock/retry, exit flush and restart, all 114 PCM assets, and Windows publish. Custom music playback itself and the remaining full-port acceptance requirements are still outstanding.

Player controls checkpoint (2026-10-18): lobby and match screens now expose per-player mute/unmute and host-only removal. Mutes use the original dt.muted ordered set, preserve imported entries, cap at 256, and commit only on successful profile writes. Incoming social effects are filtered by sender identity without suppressing gameplay traffic. Guest controls omit removal and host controls omit self-removal; removal uses the existing authenticated room ban path. Verified 9 mute migration/bounds checks, 35,283 protocol/log/authority/room checks, rendered Solo/Duos mute/unmute and host removal with peer announcement/seat clearing, profile-backed UI persistence, and spectator regression. Captures inspected and Windows publish passed. Emote rendering, production UI polish, real multi-account acceptance, packaging/updater and other full-port requirements remain incomplete.

Animated emote sticker checkpoint (2026-10-18): all 72 original SVG/CSS stickers are baked into 52-frame transparent sheets at 20fps for their 2.6-second display lifetime. Runtime uses native GPU textures with unused-frame cleanup; no browser runtime. Equipped slots, T/1-6 controls, standings stickers, ownership checks, per-player rate limits, ephemeral network delivery, existing player mutes, and persisted dt.emotes.muteOthers are connected. Muting clears active remote stickers. Export retains original per-item data for later companion poses and retro pop-ups. Verified 4,901 checks including 1,000 extracted original rate decisions, hashes/dimensions/nonempty frames for all sheets, animation variation, ownership and expiry. Windows publish, rendered Solo/Duos equipped-effect delivery, and spectator regression passed; atlas and UI captures inspected. Companion procedural dances, retro 3D pop-ups, AI result reactions, arena sticker placement and final visual polish remain unfinished alongside the broader port/release requirements.

Arena companion and emote-pose checkpoint (2026-10-18): the equipped local companion and saved evolution form now render in planning and combat using the existing native rig/clip player and original arena corner. Original emote body offsets (ten moves, easing and flip pivot) layer over companion animation without changing game state. Camera framing now includes that corner; visual review also fixed tooltips collapsing to a character-wide column. Invalid saved appearance values retain progress and fall back to the prior/default visual. Verified 72,421 comparisons against 8,100 original JS poses, all 78 GPU smoke models with an emote overlay, local five-round UI flow, rendered network Solo/Duos companions, spectator regression, and Windows publish. Captures inspected. Opponent appearance/network loadouts, arena companion movement, equipped dance clips, retro pop-ups, arena sticker placement and broader production/release requirements remain unfinished.

Arena companion travel checkpoint (2026-10-18): right-clicking the planning or combat arena now selects a ground-plane destination, clamped to the original bounds. Native travel uses the original speed, yaw, exact arrival state, walk/idle transitions and fallback hop for models without a run clip. A brief destination ring gives click feedback; spectators cannot move a companion. This cosmetic movement stays outside the shared match log. Verified 12,183 comparisons against 2,000 extracted original JS movement steps, including redirects, zero/large deltas, boundaries and arrival. Matched scaled Math.hypot behavior to fix a one-frame arrival discrepancy. Rendered Solo/Duos tests perform actual right-click travel and continue through synchronized combat; spectator regression and Windows publish passed. Remaining companion/social work includes opponent appearances, equipped dances, retro pop-ups and arena sticker placement; broader production/release requirements remain active.

Opponent companion checkpoint: room presence now shares equipped cosmetic loadouts and evolved forms. Native PvP displays the opposite seat's companion in the far corner, including mirrored views and emote body animation; AI appearance uses original deterministic selection. Original-source fixtures cover 3,444 loadout/form cases (--loadouts-only). Solo/Duos five-round rendered smoke and two-peer network UI smoke pass, including intact peer loadout delivery. This is an incremental checkpoint, not the completed port/release.

Arena sticker checkpoint: active emotes now project above local/opponent companions with original arena-edge constraints, entry overshoot, row entry motion and exit scale/translation/opacity. Atlas padding is accounted for when matching CSS artwork dimensions. --emote-layout-only checks 36 original browser animation samples plus visibility boundaries (149 assertions). Solo/Duos network UI smoke verifies an arena sticker draw; spectator smoke and captured moving-companion review pass. Retro model popups, equipped dances and final UI styling remain separate unfinished port work.

Dance retargeting foundation: DanceSkeleton applies original character-space choreography channels to native RigPose bones, including parent compensation, per-role limits, stubby-ear substitution, blend weights and limbless root compensation. --dance-skeleton-only verifies 323,136 values from 3,672 original Dancer.apply traces using all 51 dance poses over four synthetic skeletal configurations. These are algorithm checks, not rendered real-model acceptance. Native choreography generation, exported real-model bindings, root placement and match/preview integration remain unfinished.

Continuous choreography checkpoint: all 11 standard dances and 40 mystery unlock variations now evaluate natively in DanceChoreography, including original tempos, phrase changes, root turns and optional channels. --dance-choreography-only passes 978,914 comparisons from 20,349 source poses around integer beats and phrase transitions. The 323,136 skeletal checks now feed native choreography into native retargeting before comparing to the original Dancer. Desktop build passes. Actual model bindings and live dance rendering still need integration; this does not claim completed equipped-dance playback.

Equipped dance integration checkpoint: exported original bone-role analysis for all 216 distinct companion/form model sources; native choreography now drives their bones and root transforms. Arena dance-role emotes use the local or shared opponent dance loadout, continue for max(3.6s, 8 beats), fade at the original boundaries and stop on walking. --dance-bindings-only passes 330,160 original real-model bone/root/placement checks (2e-4 component tolerance for float math and non-unit source quaternions). Native blending now retains the original dot>=1 behavior. GPU smoke renders all 77 base companions with dance poses and verifies expiry/walking cancellation; Solo/Duos network UI smoke clicks My Dance and verifies playback. Store previews, kill screens, retro popups and final presentation polish remain unfinished.

Retro model checkpoint: the nine retro emote models now appear beside local/opponent companions with original hug, cane, hover, shiver, flex, float, swoop, salute and stomp motion. Native placement follows the camera-right direction and retains the 2.5-second pop / 3-second cleanup behavior. --retro-motion-only passes 34,038 source comparisons; GPU smoke covers all nine model draws plus arena trigger/cleanup, with a captured model sheet reviewed. Duos network smoke passes. Floating glyphs, smoke particles and the UFO beam are still unfinished and this checkpoint does not represent the complete retro effect.

Retro particle checkpoint: floating glyphs use the original 64px canvas artwork; smoke uses the original sphere geometry and Vademon's beam uses the original open cylinder with additive blending. Native particle state preserves spawn intervals, stomp/exit bursts, fixed per-frame velocity, fading, expansion and cleanup. --retro-particles-only passes 33,816 source trace comparisons including delayed frames. GPU smoke verifies glyphs, beam, stomp puffs and nine-puff exit burst; captured effects reviewed; Solo/Duos network UI smoke passes. This remains an incremental full-port checkpoint, not a completed release.

AI emote checkpoint: native rounds now schedule the original occasional AI elimination/win/loss reactions from equipped emote moods, with 500-2000ms delay, human/local exclusions, PvE/ghost filtering and per-round selection limits. The presentation random stream stays separate from lockstep and no network action is emitted. --ai-emotes-only passes 9,489 source comparisons across 3,000 rounds, including exact random draw counts; Solo/Duos five-round UI smoke confirms delayed reactions reach display. Solo/Duos network smoke and 24,997 cosmetic regression checks pass. Full production UI, other cosmetic presentation, audio completion and release gates remain outstanding.

Board scene asset checkpoint: all 37 original themes export to deterministic compressed scene data, retaining 5,321 mesh/particle/line instances, geometry groups, vertex colors, material properties, canvas textures and UV transforms, lighting/surface metadata and animation tick counts. BoardScene loads and validates geometry, references, matrices and texture payloads; --board-scenes-only passes 225 inventory checks. Repeated export is byte-identical and Desktop Release builds without warnings. These are initial scene states only: native board rendering, animation, event reactions and theme transitions remain unfinished.

Board mesh rendering checkpoint: BoardMesh converts original material groups and UV matrices; BoardMeshLayer owns cached native geometry/textures and draws mesh instances with original vertex/instance colors, flat shading, alpha cutoff, additive blending and wireframe materials. Canvas row orientation and sRGB texture handling are preserved. --board-scenes-only passes 2,578 checks including 120 Three UV-transform references. --board-mesh-smoke renders all 37 themes (4,861 material/instance draws), checks actual framebuffer vertex colors, wireframe interiors and state reset, and captures an overview sized to the real framebuffer. Companion and farm GPU regression smoke pass. This mesh layer is not yet integrated into live matches; board particles, line primitives, exact environment lighting/fog, animation/reactions, surfaces and transitions remain to be implemented.

Board point/line checkpoint: native board geometry now renders all source primitive types present in the 37 themes, including 33 point objects and two line objects. Point sprites retain original perspective size attenuation, viewport scaling, material/texture colors, UV transforms and texture orientation; lines use their original topology. GPU smoke covers all 4,890 initially visible material/instance draws and checks framebuffer point sizes at two depths, fixed-size points, top/bottom texture colors, line pixels and render-state reset. --board-scenes-only passes 2,683 conversion/inventory checks; companion and farm GPU regression smoke pass. Scene previews were reviewed. Weather motion, scene reactions, exact lighting/fog, tile surfaces and live-match integration remain unfinished.

Board lighting checkpoint: native scene previews use each theme's linear-space hemisphere sky/ground colors, sun color/intensity and original (-8,18,10) light direction. The shared material shader now accepts board lighting separately from farm lighting and applies original smoothstep fog after sRGB conversion, respecting material fog opt-outs. BoardLighting retains flash decay, hemisphere multipliers and sun tint controls; 5,328 comparisons with original buildEnv light updates pass (8,011 total --board-scenes-only checks). GPU smoke verifies near/mid/far fog pixels and fog-disabled materials; all 37 previews reviewed; companion and farm regression smoke pass. Sky backgrounds, board shadows, animation-driven controls, surface appearance and live-match integration still remain.

Board sky checkpoint: the scene exporter now retains each original 2x256 canvas gradient, and BoardBackground renders those textures or the original solid sky without depth writes. All 37 backgrounds are checked on the GPU against every original RGBA pixel (one-byte tolerance for sRGB conversion), including vertical orientation; composed scene previews were reviewed. Board geometry GPU smoke and all 8,011 asset/lighting checks pass. Board shadows, animated scenery/weather and reactions, tile/bench/item surfaces, transitions and live-match integration remain outstanding.

Board shadow checkpoint: shared SceneShadowMap supports the original 2048px arena shadow camera (-8,18,10), 28x28 orthographic area, near/far 1/50 and -0.0005 depth bias. Board scenery respects original cast/receive flags, with the existing PCF-soft sampler now parameterized by map size and bias. The farm retains its original camera/settings; framebuffer bindings and viewport are restored after the depth pass. GPU smoke verifies caster counts across all 37 themes and confirms 424 File Island receiving pixels darken when shadows are enabled. Scene previews reviewed; farm and companion GPU regression checks pass. Live unit/companion board shadows will be connected with match integration; scene animation, surface appearance and transitions remain unfinished.

Board surface checkpoint: exported original gameplay tile/slot/pedestal geometry directly from render3d.js constructors. BoardSurfaces preserves all theme colors, planning enemy colors, emissive glows, roughness, metalness and texture assignments; BoardSurfaceView draws shared geometry with original positions and the 14-column/18-slot Duos layout. GPU previews now render 75 Solo or 140 Duos surfaces; all 37 themes cover planning and combat. Original applySurface/tileLook comparisons add 1,850 checks, for 9,861 --board-scenes-only checks total. Full composed previews reviewed. Hover/flash feedback, Duos environment scaling/fog, scene animation/reactions, transitions and live-match wiring remain unfinished.

Board feedback checkpoint: BoardSurfaceFeedback preserves tile drag-hover priority, combat flash fading/expiry, planning suppression, theme glow fallback, own-bench highlights and item hover/drag targets. BoardSurfaceView uses this state without mutating shared theme materials. Corrected the combined Duos layout to hide the original item shelf: 112 tiles plus 18 slots = 130 surfaces. Exported original highlight/drawItems methods produce 99,012 passing source comparisons; existing 9,861 board checks pass. GPU smoke confirms 195 pixels brighten with combined hover/flash feedback and covers hidden Duos pedestals. Match input/combat-event wiring, animated scenery/weather, board transitions and complete Duos environment behavior remain unfinished.

Live board integration checkpoint: ArenaEnvironment now composes the themed sky, scenery, surfaces, lighting/fog and shadow/color passes in PlanningView and BattleView, replacing the flat ArenaBoard placeholder. Units cast and receive board shadows. MatchScreen selects the equipped board and the mirrored PvP home opponent's shared board; unknown IDs fall back to File Island without using them as paths. Theme replacement constructs resources before disposing the previous set. Duos scales scenery X by 1.8, uses fog 65/120 and restores theme fog in planning. Solo/Duos five-round match and network smoke pass; GPU checks cover theme changes, fallback and fog restoration, and live screenshots were reviewed. Scenery/weather animation, theme transitions, companion lighting/shadows, item tokens and actual input/combat-event feedback wiring remain unfinished; production UI and other full-port gates remain outstanding.

Live burst feedback checkpoint: CombatBoardBursts consumes simulation burst events once and maps the original radius selection, mirror transform, viewing-team color and event-time +0.45 expiry to native surface flashes. BattleView applies the events using combat time; environment feedback now survives theme-resource changes. Extracted original burst code covers every Solo/Duos origin, radii 0-3, both teams and both orientations: 105,120 comparisons pass, alongside 99,012 surface-feedback and 9,861 board checks. Solo/Duos five-round UI smoke passes. Input hover/drag wiring, scenery/weather animation and theme reactions/transitions remain unfinished.

Board motion checkpoint: exported explicit source motion metadata for the shared sea helper and File Island crystal instances. Native BoardSceneMotion updates wave vertices in existing GPU buffers and evaluates crystal bob/rotation matrices; the same animated geometry is used by scene passes. MatchScreen maintains a continuous capped presentation clock across planning, combat and theme replacement. 73,896 original vertex/matrix comparisons pass at varied times, with GPU verification showing 230 changed pixels; existing board checks and Solo/Duos five-round match smoke pass. This implements only shared sea motion and File Island crystals: other theme motion, particles, animated textures, light/fog reactions and transitions remain unfinished.

Shared board particle checkpoint: native animation now advances all 23 systems created by the original particles helper, preserving seeded initial positions/speeds, per-axis velocity, time-based wobble, Float32 writes and single-span box wrapping. Frame deltas are capped at 0.1s and theme replacement starts a fresh particle state without a large clock jump. Source traces cover 2,400 frames per system and compare 359,040 positions, including zero/delayed frames and wrapping. GPU verification confirms 161 changed dust pixels; all earlier board checks and Solo/Duos five-round match smoke pass. Other custom particle systems, moving props, animated textures, theme reactions/transitions and remaining presentation work are still unfinished.

Beach motion checkpoint: exported seeded palm transforms and the surf ring's original base matrix. Native rendering now reproduces palm sway plus surf scale and opacity without compounding the exported initial scale. Motion fixtures now pass 75,440 source comparisons; all other board checks pass. GPU beach animation changes 11,796 pixels, and Solo/Duos five-round match smoke passes. Other theme-specific animation, reactions/transitions and the remaining full-port acceptance gates are still unfinished.

Village/Toy Town motion checkpoint: seeded rocking eggs, bobbing/rotating blocks, spinning/orbiting tops and the five-car ring train now animate natively using original formulas and shared transform evaluation. Prop metadata validates kind, instance count and finite values. Original motion comparisons now total 79,664; existing board suites pass. Isolated GPU checks verify each of the four effects independently (41/559/118/260 changed pixels), and Solo/Duos five-round match smoke passes. Theme-specific effects, reactions, transitions and the broader full-port acceptance work remain unfinished.

Desert/coliseum effects checkpoint: native desert heat haze preserves scrolling UV offsets and pulsing opacity; updates derive from immutable initial UVs to avoid accumulation. Coliseum banners preserve source orientation/sway and torch flames preserve their volume-balanced scale animation. Original motion/UV comparisons now total 96,088, including UV reset; existing board suites pass. Isolated GPU checks show 405/202/8 changed pixels for haze/banners/flames. Solo/Duos five-round match smoke passes. Remaining theme effects, reactions/transitions and full-port UI/audio/network/release acceptance remain unfinished.

Savanna/mountain/grid checkpoint: native gear transforms preserve standing and flat gear rotation; mountain stars and grid tile outlines pulse with original opacity; grid polyhedra bob/rotate and the floor scrolls with the original modulo wrap. Shared scrolling UV updates preserve immutable initial coordinates. Original motion comparisons now total 106,120 with appropriate Float32 tolerance for 60x grid UVs. Isolated GPU checks verify all five effects (2,581/100/454/870/34,891 changed pixels); stars use an upward camera to inspect the sky dome. Board suites and Solo/Duos five-round match smoke pass. Lighthouse beam, expanded theme animations/reactions/transitions, production UI and remaining full-port acceptance gates remain unfinished.

Lighthouse checkpoint: exported local beam matrices and the stationary parent transform; native rotating-parent evaluation composes both beams around the lighthouse without rotating the tower's position. Metadata validates matrix lengths, finite values and angular speed. 106,376 original motion comparisons now pass, including both beam world matrices at eight times; isolated GPU verification changes 2,795 pixels. Existing board suites and Solo/Duos five-round match smoke pass. Expanded/mystery board motion and stateful reactions, transitions, production UI and remaining release gates remain unfinished.

Stateful spiral checkpoint: BoardSpiralMotion preserves seeded helix positions, combat speed approach, phase accumulation after instance writes, and frame-dependent lower-islet drift. ArenaEnvironment passes planning/combat state into motion updates. Export instrumentation tags original extraThemes helper inputs without rewriting its animation callbacks. A 360-frame source trace covers two combat/planning cycles and zero/normal/delayed deltas; total motion checks are 653,576. Isolated GPU checks show 13,293/456 changed pixels for rocks/islets; existing suites and Solo/Duos five-round match smoke pass. Spiral crystals/halos/lightning, remaining theme effects and reactions, presentation-state continuity across view replacement, and broader full-port acceptance gates remain unfinished.

Spiral crystals checkpoint: the four crystals now bob and rotate with original fixed vertical scale, while their additive halos bob and pulse independently with seeded phases. Expanded source traces include every crystal and halo matrix across 360 frames, bringing motion checks to 699,656. Isolated GPU verification shows 76/184 changed pixels, and board suites plus Solo/Duos five-round match smoke pass. Spiral lightning, animation-state continuity across view replacement, remaining theme reactions/effects and full-port release gates remain unfinished.

Arena continuity checkpoint: MatchScreen now owns one ArenaEnvironment shared by planning and combat views. Changing rounds or watched combat disposes unit-view resources without resetting same-theme particle/spiral state or destroying arena GPU resources. Standalone views retain explicit ownership of their own environments. New combat views clear prior combat flashes before consuming their event stream. GPU smoke verifies shared clock/theme retention and rendering after both injected views are disposed. Solo/Duos five-round match and network match smoke pass. Theme replacement still creates fresh theme state as before; lightning, other expanded/mystery effects, transitions and broader full-port acceptance gates remain unfinished.

Spiral lightning checkpoint: native presentation preserves the seeded bolt and schedule RNG streams (including geometry/setup consumption), combat-entry strike at the prior tick time, scheduled strikes after visibility updates, flicker gaps and flash decay. Exported bolt metadata retains original positions/colors/flash parameters; scene lighting consumes the decayed flash once. A 1,600-frame trace covers repeated planning/combat transitions, zero/delayed frames, all bolt visibility and visible world matrices; total motion comparisons are 707,672. GPU checks verify 74 lit bolt pixels, flicker hiding and hemisphere flash. Board suites and Solo/Duos five-round match smoke pass. Other expanded/mystery theme effects, complete game-signal integration, transitions and broader full-port acceptance gates remain unfinished.

Board phase correction: MatchScreen supplies actual combat activity independently from whether the combat view is still displayed. Results now stop combat-only scheduling and slow spiral rocks toward their noncombat speed. Newly created themes record their initial combat state without synthesizing an entry transition. Source lightning fixtures now cover 3,200 frames from planning/combat initial states through combat/result/planning transitions; total motion comparisons are 715,960. GPU checks verify 12 seconds of result display produces no repeat flashes and a newly loaded combat theme emits no false entry flash. Board suites, Solo/Duos match and network match smoke pass. Full stage/win/evolution signals, other theme effects and broader release acceptance remain unfinished.

Castle effects checkpoint: native moon color approaches the source stage-dependent red target, halo opacity pulses, candle flames scale reciprocally, and seeded bats circle/flap before closing into the original five-second combat-entry swarm. Initial combat state suppresses false entry events; MatchScreen now supplies the current stage to the shared environment. 1,233,600 source comparisons cover 1,200 frames across stage changes, initial phases and swarm expiry. Isolated GPU checks verify flames/bats/moon/halo (9/262/1,336/3,697 changed pixels, with a close-up candle camera). Existing board suites and Solo/Duos five-round match smoke pass. Remaining expanded/mystery effects, win/evolution reactions, transitions and broader full-port acceptance gates remain unfinished.

City window material correction: board export now preserves emissive texture references and BoardMeshLayer binds them through the shared room renderer. Kowloon's original city material uses the same window map/UV transform for color and emission; walls no longer receive uniform white emission. Reference validation and an original material assertion pass. GPU comparison darkens 49,650 wall pixels and verifies that subsequent untextured emissive draws reset the texture state. Existing board suites, all six farm-room themes and Solo/Duos five-round match smoke pass. Kowloon scan/sign/rain/fog animation, remaining expanded/mystery effects and broader full-port acceptance remain unfinished.

Kowloon effects checkpoint: native scan band/lattice preserve vertical sweep, modulo wrap and opacity envelope; neon signs retain seeded phases and original dim-color flicker. Combat weather eases rain size/opacity and fog distances, with source per-frame fog overriding the static Duos distances. A 900-frame trace passes 31,500 source comparisons including combat entry/exit and zero/delayed frames. Isolated GPU checks verify band/lattice/sign/rain updates (574/4,857/73/929 changed pixels), and composed tests verify Duos fog override and result easing. Existing board suites and Solo/Duos five-round match smoke pass. Other expanded/mystery effects, win/evolution reactions, transitions and broader full-port acceptance remain unfinished.

Circuit Core checkpoint: native data pulses preserve accumulated combat speed, Float32 position writes and hiding beneath the chip. Curved-screen and floor code preserve their distinct scrolling directions/modulo behavior. The rising local-player win signal drives screen/floor opacity and scene-light flash with original decay; held wins and newly loaded won states do not retrigger. MatchScreen derives wins from the local player's own Solo/Duos battle rather than the watched battle. 299,200 source comparisons cover 1,600 frames and initial win states. Isolated GPU checks verify pulses/screens/floor (154/9,171/20,940 changed pixels); composed checks cover scene flash and edge behavior. Existing board suites, Solo/Duos match and network match smoke pass. Other expanded/mystery effects, evolution reactions, transitions and broader full-port acceptance remain unfinished.

Flame Temple checkpoint: native outer/inner flames preserve shared-object scale/rotation and prior-frame flare timing. Geysers retain ambient triggers and randomized evolution delays; lava scroll/color, ember size, hemisphere flash and sun tint/intensity respond together to evolution. MatchScreen supplies the local roster's total evolution stage, and newly loaded existing counts do not trigger a flare. 589,200 source comparisons cover 1,200 frames, stage/combat changes, rising/falling evolution totals and controlled random delays (fixture RNG resets after Three object creation). GPU checks verify five isolated effects and combined lighting/initial-state behavior. Existing board suites, Solo/Duos match and network match smoke pass. Remaining expanded/mystery effects, transitions, production UI and broader full-port acceptance gates remain unfinished.

Underwater castle motion checkpoint: native caustic layers scroll in their original directions; light shafts vary width, kelp sways, clams open, and fish scatter for the original combat-entry interval before easing back. Preserves original seeded transforms and school-dependent scaling, including source zero-scale school entries. 1,604,000 source comparisons cover 1,000 frames from planning/combat initial states, repeated transitions and scatter recovery. All six isolated effects change GPU pixels; existing board suites and Solo/Duos five-round match smoke pass. Transparent ordering/renderOrder fidelity, remaining expanded/mystery effects, transitions, production UI and broader full-port acceptance remain unfinished.

Board transparency checkpoint: all 37 board exports preserve original bounding-sphere sort centers, object creation order and explicit renderOrder. The native board transparency pass sorts animated object transforms by camera clip-space depth, with source render-order and ID tie breaks. Verified against 800 original Three comparisons, overlapping alpha-blended GPU triangles (including reversed camera and explicit order), all-theme GPU checks and Solo/Duos match smoke. This sorts board scenery within its existing pass; global interleaving with units, surfaces and companions remains outstanding.

Freezeland now animates the original four snowmen and three aurora curtains, and eases combat blizzards in and out with the source snow drift/wrapping, point size, aurora opacity, fog distance and linear fog color. The shared arena applies weather fog in Solo and Duos. Verified with 1,257,000 comparisons over 600 original JavaScript frames (repeated phase transitions and zero/delayed updates), all-theme GPU smoke including isolated snowmen/auroras, and Solo/Duos match smoke. Remaining themes and full production acceptance are still outstanding.

Numemon Sewer now preserves the source scrolling pipe outflow, circling/bobbing rubber duck, Numemon squash and sway, and four-second win jumps. Win edges use the previous scene time, held results do not restart the celebration, and initial won state does not emit a false event. Original sludge waves and particle systems remain active. Verified with 208,000 original JavaScript comparisons over two 800-frame traces, all-theme GPU smoke including close-up flowing outflow and win jumps, and Solo/Duos match smoke. Full port acceptance remains outstanding.

Witchelny Library now preserves both orbiting book-page sets, floating candles/flames/halos, counter-rotating rune circles, and evolution-driven gold circle/spark color, spin acceleration, opacity, hemisphere flash and warm sunlight tint. Initial evolution counts suppress false events; held counts, decreases and subsequent increases match the source. Verified with 1,510,800 original JavaScript comparisons across two 600-frame traces, isolated GPU effect checks, live-arena light/tint and initial-event checks, and Solo/Duos match smoke. Full production port acceptance is still outstanding.

Royal Knights Coliseum now preserves its crowd wave, combat jumps, win cheers (including priority over combat), swaying banners, crowd-height-aware camera flashes, and seeded victory confetti with original gravity, per-frame drag, rotations and five-second visibility. Initial combat/won state suppresses false events; held and repeated results match the original. Verified with 1,258,080 source comparisons across two 800-frame traces sampling crowd/confetti matrices and every camera flash, isolated GPU effects, all-theme GPU smoke and Solo/Duos match smoke. Full port acceptance remains outstanding.

Battle Studio now preserves six sweeping spotlight beams and floor pools, combat speed/color easing, HSL stage strips, scrolling LED walls, audience/glowstick movement, the two-part swinging camera crane, and seeded victory confetti. Beam orientation uses the original up-to-target quaternion and length scaling; pool render order stays explicit. Verified with 1,293,200 original JavaScript comparisons across two 600-frame traces, all eight effect groups on the GPU, all-theme GPU smoke and Solo/Duos match smoke. Remaining themes, production UI and full release acceptance are still outstanding.

Trailmon Station now preserves the train's 16-second loop and combat-triggered arrivals after the seven-second cooldown, all five carriages and engine, attached steam with offscreen opacity suppression, and rotating station clock. Combat events use the previous scene timestamp and suppress an initial false transition. Verified with 361,204 comparisons over two 1,400-frame original traces plus strict cooldown/loop boundary checks, isolated GPU effects (including the dark clock hand against a contrasting background), all-theme GPU smoke and Solo/Duos match smoke. Full production acceptance remains outstanding.

Overdell Cemetery now preserves ghost orbits and combat haste (including the source callback ordering), combat fog, and green lightning with its own seeded 2-to-5-second scheduler. Shared lightning retains Spiral Mountain's separate entry-strike and delay behavior through explicit scheduling parameters. Verified with 380,096 comparisons across two 1,600-frame original traces, existing Spiral lightning comparisons, moving-ghost GPU checks, green lightning pixels, live wide-board combat/result fog, all-theme GPU smoke and Solo/Duos match smoke. Full production acceptance remains outstanding.

Server Pyramid now preserves combat-triggered corner-beam blaze and width, animated blue brazier flames, six stage-driven glyph-band fades and scrolling glyph texture. Combat edges use the prior scene timestamp and initial combat suppresses a false blaze. Verified with 185,600 original comparisons across two 800-frame traces covering stage increases/decreases and combat transitions, generic flame comparisons, isolated GPU effects, all-theme GPU smoke and Solo/Duos match smoke. Full production acceptance remains outstanding.
