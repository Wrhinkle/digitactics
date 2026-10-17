# Digi-Tactics native BRUTAL port

This is the first executable migration milestone, not a replacement for v1.0.14.
It is a native C# application using BRUTAL GLFW, OpenGL and ImGui. No Electron,
WebView, JavaScript interpreter or browser runs inside it.

## Build and run

Windows x64, .NET SDK 10.0.402, Node.js for the build-time content exporter,
and an OpenGL 4.5 capable graphics driver are required.

From the repository root:

```powershell
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

## Boundaries

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
  Eclipse, Refraction and combat reward counters are included. Alliances,
  held/global items, Tamers and planning/settlement remain pending; the current
  mechanic binder requires NoTraits and rejects unsupported hooks.
- Full combat traces are gzip-compressed in source control and decompressed by the
  test runner one battle at a time; comparisons retain all recorded samples.
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
