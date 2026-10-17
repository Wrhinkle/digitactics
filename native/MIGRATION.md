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
- Unit base stats, ability numeric scaling and selling: 3,996 form/pace cases.
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
  definitions in the actual legacy dispatcher, checking 126,058 callback entries as
  well as fight state. These verify infrastructure, not production set definitions.
  Damage-stage tests include armor/reduction changes made inside damage hooks.

- Production Faction Wars combat traits: all 29 traits and all 70 breakpoints,
  including ordered trait counting, emblems, duplicate-line unions and bonus counts.
  Twelve combat augments plus Crest's count modifier are implemented. Another 84
  reference fights use the real production hooks and compare throughout each fight;
  trait count values and insertion order are checked separately. Shared equipment is
  enabled. Faction Wars planning rewards, Tamer powers and Duos partner side effects
  remain pending; this is not a whole-set or whole-match completion claim.

Latest verification: `./native/build.ps1 -Publish` passes 1,803,899 assertions, compiles
without warnings, renders the workbench, purchases through the native UI, and runs
the self-contained executable. Fixtures compare against actual methods in the existing
JavaScript engine; they do not establish whole-match parity.

## Next implementation sequence

1. Extend production binding beyond Faction Wars traits/combat augments to the
   remaining sets, item hooks, powers and mechanics.
   Finish hook context operations (summons, board movement, side/trait state) and
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
