# Full BRUTAL port status

The active objective is the complete native port described in `README.md`, including
production UI, rendering, all rulesets, Steam, save migration, packaging and updates.
The current executable is a migration workbench. It must not replace the public game yet.

## Completed and verified

- Native BRUTAL GLFW/OpenGL/ImGui host, UI purchase smoke test, renderer capture,
  locked packages, self-contained Windows build and bundled dependency notices.
- Deterministic RNG and base shared-shop selection across all 22 set/pace catalogs.
- Bench combining, owned-three-star exclusions, unit selling including Dragon premiums.
- Solo/Duos hex geometry and deterministic movement/target tie-breaking.
- Unit base stats, ability numeric scaling and selling: 5,232 form/pace cases.
- Damage math: outgoing modifier order, attribute advantage, overtime, physical/magic
  mitigation, damage reduction cap, shield absorption, and mana gained from damage.
  Effect activity and hook-supplied values are inputs; hook implementations are pending.

- Shared combat kernel: fixed tick, ordered unit updates, attacks, mana, movement,
  leaps, cast locks, stun, weakening, shields, healing, death and battle outcomes.
- All 14 shared ability types are exercised in 88 reference battles across star
  levels and Solo/Duos widths. State is compared throughout each fight, including
  target selection, cooldowns, HP/mana/shields and damage totals. Equipment and
  set hooks are deliberately disabled in these reference cases.

- Equipment catalog, additive stats/passives, deterministic Thief Glove rolls,
  attack/ability crits, dodge, on-hit effects, lifelines, burn/wounds, regeneration,
  auras and timed shared passives. Another 116 reference fights cover every shared
  completed item and mixed loadouts in all 22 variants. Item-specific hooks remain
  disabled; the native equipment layer does not yet execute those hooks.

- Combat hook dispatcher and lifecycle integration: source-group ordering, stable
  priorities, Duos owner filtering, recursive-call guard, typed mutable damage events,
  per-source state, global/unit timers, timed buffs, lethal recovery and end callbacks.
  Two additional reference battles enable all 23 hook event types through controlled
  definitions in the actual legacy dispatcher, checking the full callback traces as
  well as fight state. These verify infrastructure, not production set definitions.
  Damage-stage tests include armor/reduction changes made inside damage hooks.

- Production Faction Wars combat traits: all 29 traits and all 70 breakpoints,
  including ordered trait counting, emblems, duplicate-line unions and bonus counts.
  Twelve combat augments plus Crest's count modifier are implemented. Another 84
  reference fights use the real production hooks and compare throughout each fight;
  trait count values and insertion order are checked separately. Shared equipment is
  enabled. Faction Wars planning rewards and Tamer powers remain pending; this is
  not a whole-set or whole-match completion claim.
- Duos combat setup: per-partner augment ownership, temporary AS/defense/mana buffs
  at the original initialization stage, combined trait counts/caps, Crest Link and
  Sync Bond counting, Partner's Ace and Tag Team. Eighteen production Faction Wars
  team fights compare both paces and all star levels, with two owners per side and
  shared items. Duos match flow, portals, shared health and networking are pending.
  Other sets' count-changing hooks are not yet implemented.

- Non-shop combat definitions are now exported alongside shop content (without
  adding them to shop pools); stats/scaling are checked for all 5,232 unit forms.
  Runtime stages, fractional attack range, summoning with equipment/owner/stat
  multipliers, and hook board queries/movement are implemented. Dispatcher probes
  now grow units and summon during unit timers, checking RNG continuity, rejected
  spawns, dynamic unit counts and same-tick updates against the legacy engine.
  Fates summoned item-hook rebinding is implemented; presentation movement metadata remains pending.

- Fates combat: seven custom abilities; all 28 traits at all 62 breakpoints;
  both Fallen paths; persisted Deva/Blossom inputs; Partner HP/AP/AD/mana modifiers;
  time-rune recovery and summon-based traits. 105 additional production-reference
  fights compare counts, runtime stages, summons, rune state, kill counters and
  combat state. Partner shop generation, planning rewards
  and Duos mechanic-state merging remain pending.
- All 21 Fates held-item hook definitions are bound once per item ID per holder,
  including artifacts, deferred damage, lethal protection, execute gold and death
  summons. Fifty additional fights cover individual/duplicate/mixed equipment with
  production traits; six more verify summoned item hooks and explicit suppression.
  Collector gold is recorded for later round settlement, which remains pending.
- Combat reference traces use deterministic gzip storage to avoid checking in an
  increasingly large plaintext file. Tests decompress and compare the full data.

- Fates augment combat binding: all 11 combat augments, Twin/Soulmates count
  modifiers, Partner-only bonuses, Lunar Crown priority, and Duos owner scoping.
  Forty-eight additional Solo/Duos fights include production traits, items and
  Partner forms. Shared Duos combat augments now have a set-independent binder.
  Augment selection, instant grants and round/economy hooks remain unported.

- Galaxies: all 52 custom abilities and the supporting combat mechanic callbacks
  (poison, frenzy, cyclone, mana tax, hex, revive, meep accounting and delayed
  spell effects). 312 reference battles exercise every custom cast at all three
  stages on both board widths, with normal/datastream/darkocean combat modifiers.
  Poison intentionally follows the shipped context API: it grants damage mana.
  All 26 combat traits are implemented across 48 breakpoints, including DigiXros
  fusion/ejection, bounty inputs, pirate kill accounting, Overload and Timekeeper.
  Forty-eight additional reference fights compare production trait combinations;
  the DigiXros fixture requires both pilots to eject. Attack-speed buff accumulation
  preserves the original addition order to avoid shifting attacks by one tick.
  All 17 held-item hook definitions and nine combat augments are now bound,
  plus the Stars trait-count modifier and shared Duos augments. Forty-two item
  fights cover duplicate/mixed loadouts; forty augment fights cover Solo/Duos
  ownership with production traits and items. Summons rebind item hooks.
  Consumables, augment grants, rewards and other planning rules remain pending.

- Gizmos: four custom abilities (Snipe, Grab, Devour, Visions), Colossus immunity,
  spell wards and the combat mechanic callbacks. All 22 standard/overclocked
  held-item hook definitions are bound, including backup revival, decoy summons,
  shroud restoration, ally healing, auras and timed slows. Twenty-four ability
  battles and 52 item battles compare the production callbacks with traits disabled.
  All 27 traits across 63 breakpoints are now bound, with all seven Mutation
  variants plus seeded selection, Junkyard item conversions, Innovator summons,
  Transformer modes and persisted Glutton stat inputs. Another 154 reference
  battles cover two formations with component and mixed hook-item loadouts.
  Catalog schema 8 preserves component categories and recipe order for temporary
  upgrades. Digivice upgrades, feeding commands and planning rules remain pending.

Latest verification: `./native/build.ps1 -Publish` passes 12,200,973 assertions, compiles
without warnings, renders the workbench, purchases through the native UI, and runs
the self-contained executable. Fixtures compare against actual methods in the existing
JavaScript engine; they do not establish whole-match parity.

## Next implementation sequence

1. Extend production binding beyond Faction Wars and Fates trait/ability coverage to the
   remaining sets, item hooks, powers and mechanics.
   Finish hook context side/trait state and
   presentation event metadata. Extend reference battles to real set definitions
   before claiming full combat parity.
2. Set-defined ability dispatch; port each ruleset with explicit coverage
   and deterministic full-match trace comparisons.
3. Full match state and commands: planning board, economy, round progression, AI,
   offers, carousel, items, Duos and shared health. Special shop offers are pending too.
4. Native asset pipeline and 3D renderer/animation; production screens and audio.
5. Steam and updater adapters, profile import/rollback, end-to-end acceptance and release.

Maintain pure rules with no renderer/network dependency. The build-time JavaScript
exporters are reference adapters, not a JavaScript runtime inside the native app.
Preserve source attribution and existing player profiles. Keep work on
`feature/brutal-port` until production acceptance criteria are satisfied.
