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
  upgrades. All 37 combat Digivice upgrades, Injection's trait interaction, Booster/Burst
  counting, Binary Airdrop and Woodland copies are now implemented. Another 160
  reference battles cover Solo/Duos at two star levels with merged team upgrades
  and owner-specific Reserves gold inputs. Shared Shield/Rally casts now dispatch
  shield hooks, fixing First Aid and Censer interactions. Upgrade picks, grants,
  feeding commands and planning rules remain pending.

- Reckoning: Hook, Summon, Possess and Respite are implemented, including both
  summoned partner mappings, possession resurrection/decay and the lethal-protection
  zone. Thirty reference battles exercise all five casters at every star level on
  both board widths, including PvE stat scaling. Catalog schema 9 adds summon IDs
  and per-stage PvE multipliers. Shadow-item mechanic callbacks are connected to their equipment effects.
  All 27 traits at 61 breakpoints now pass 122 reference fights across two
  formations, with components and hook-free Shadow equipment. Eight additional
  fights check both Royal Rivals individually and together (the bonus turns off).
  Sentinel uses buff identity when passing its shield, preserving unrelated buffs;
  Chimera inheritance, Trickster copies, Phantom revival and Eternal summons are
  included. Catalog schema 10 exports rival-trait lists. All 60 held-item hook definitions
  (standard, Shadow and Radiant) now pass 192 additional fights at all three stages,
  including duplicate/mixed equipment and X-Antibody's HP bonus/reduced drawbacks.
  Summons bind inherited item hooks. Feather/Phantom revival clears burn accumulation
  as well as its source. Non-finite cooldowns from stacked slows are explicitly
  encoded and compared, rather than silently converted to JSON null. All ten
  combat augments and shared Duos augments now pass forty additional fights with
  production traits and items. A targeted summon-during-cast test verifies appended
  item observers execute in the same event without invalidating dispatch iteration.
  Offer grants and planning/economy rules remain pending.

- Dragonlands: Copycat's five random outcomes and all 20 held-item hook
  definitions are implemented, including Golden Armor's capped bank bonuses,
  per-holder reward limits, axe stacks and investor deaths. Eighteen ability
  fights require all five outcomes; 68 item fights cover all stages, duplicates,
  mixed gear and bank values of 0, 7 and 30 (capped to 15 in combat). Team reward
  counters are compared independently from unit state. Summoned items rebind hooks.
  All 27 traits at 72 breakpoints now pass 168 further reference fights, with
  every Mirage variant and seeded selection, Jade statue placement/crumbling,
  Mage recasts, Starcaller healing/execute outcomes and Lagoon/Core counters.
  Dragon marks count once per distinct shop line, including duplicate copies;
  Normal's Dragon trait limit and all three Monolith positions are covered.
  Catalog schema 11 stores Dragon marks and pace-specific trait limits. All 19
  combat augments plus mixed combinations pass 120 more Solo/Duos fights, covering
  all star stages, retained/capped training kills, Dragon execution, partner-owned
  state, healing amplification, and delayed team buffs. Training reward counters
  are compared separately. Pick grants and planning/settlement rules remain pending.


- Arcane: all four custom ability types across seven casters pass 84 reference
  fights across three star stages, both board widths and front/back placements.
  The exporter verifies all three rotating spell branches and all four gamble
  outcomes actually execute. All 26 traits at 69 breakpoints pass 138 fights;
  another 100 targeted Solo/Duos fights cover every two/three-member laboratory
  combination, all Envoy subsets including inactive counts, individual/all Junk
  King parts, and repeated component upgrades. Chained Belphemon, transformation
  stats, sponsored items, accumulated Syndicate HP and capped Bagra chest bonuses
  are included. Conquest/coin reward counters are compared independently.
  Catalog schema 12 includes swap/Envoy/laboratory metadata, item ingredients and
  exact self-upgrade mappings. Temporary upgrades and enemy targeting are shared
  helpers; Arcane preserves its distinct duplicate Blue Buff rule. All 29 item-hook
  definitions now pass 115 further fights, including all 16 contraband items,
  duplicate stat stacking, tier-order overrides, Syndicate eligibility and bounty
  counters. Coverage guards require delayed Ascension, repeat Doom shots, bounty
  rewards, defensive procs and contraband Hex bursts to actually execute. All ten
  combat augments plus mixed combinations pass 44 Solo/Duos fights, including
  priority-ordered Second Wind, summons, items and partner ownership.
  All 25 combat Anomalies are implemented, with kill accounting also preserved
  for the two utility identities. Another 192 fights cover all identities and
  mixed combinations across Solo/Duos, plus full/one-cell summon spaces, chained
  Devour, partner/share order, shield/contraband interactions, and synthetic
  119/120-mana Fireball boundaries. Coverage guards require revives, threshold
  bonuses, delayed Ascension and both summon species to occur. Anomaly offers,
  utility pick grants, persistence and planning/settlement remain pending.
- Coliseum: all six custom ability types across nine casters and all six form
  choices pass 90 reference battles. All 24 Digi-Core paths plus mixed combinations
  pass another 150 Solo/Duos fights across all star stages, with form modifiers,
  component equipment and Pirate Captain AP. Infection timers retain their own
  source/victim state, continue after caster death and transfer on victim death;
  coverage guards require spread, expiration and a dead source. Bakemon's All Out
  recovery, buffs and recurring self-damage are included. All 26 traits at 75
  breakpoints now pass 150 additional fights; 60 targeted Solo/Duos cases cover
  every Sensei subset, Justimon's priority-ordered stance membership, Rose Queen
  regrowth, prior Supreme selection and full/one-space Mech spawn boundaries.
  Catalog schema 13 includes authored form metadata. Sensei count suppression,
  dynamic trait membership and summon interactions match the reference engine.
  All 14 held-item hook definitions and ten combat augments, plus Arena's origin
  count modifier, pass 222 additional Solo/Duos battles across all star levels.
  These cover duplicate items, mixed equipment and cores, burns, summons, partner
  ownership and stage-scaled Lich Bane. Defensive procs, Void Staff, flail stacking
  and Doom execution have coverage guards. Doom preserves its distinct immediate
  first two ticks; Second Wind retains normal callback priority. Offers, consumable
  use, pick grants and planning/settlement remain pending.
- Underlords and Knockout now share all 50 active custom ability types plus the
  passive identity, hero kits and core combat mechanics. The original 348 isolated
  cast battles remain, with 744 additional battles covering every shop hero, all
  star levels, both modes and both board widths with the actual mechanic enabled.
  These include poison accumulation and refresh, damage-over-time/leech transfers,
  acid zones after caster death, frost auras, Refraction, Stone Gaze, Shallow Grave,
  Eclipse, attack-triggered kits, Meepo clones and all five helper summons. Coverage
  guards require key defensive effects, poison, transformations and all summons to
  occur. The summon search preserves the authored seven-column base-cell lookup
  on wider boards. Combat rewards and prior permanent Duel attack damage are
  carried separately from settlement. All 26 alliances at 50 breakpoints now pass
  200 further battles across both modes and board widths, plus 36 Duos interaction
  cases. These cover Legend's priority-ordered membership, Phantom's Rogue
  inheritance, merged partner counts, Shaman's two dragon helpers, Spirit's Delta
  Strike, Undead/Sage saves and accumulated Beast HP. Reward comparisons also run
  with alliances enabled. Shared Duos combat augments are supported. All 39 held-item
  hook definitions, the three global combat-hook definitions and Summoning Stone
  now pass 378 additional battles across both modes, all star stages and Solo/Duos.
  Tests include duplicate equipment, Meepo's inherited items, observer callbacks on
  summoned holders, reflection guards, Necro/Horn summons, defensive triggers,
  mixed loadouts and repeated owner-scoped globals. Coverage guards require delayed
  effects, both item summons and item-carrying clones to occur. Summoning Stone
  reads merged-side globals and applies before clone scaling. The other global
  identities are accepted as combat no-ops; their offer/economy/settlement behavior,
  item grants/transfers and full match planning remain pending. All eight Tamer
  builds now pass 192 additional battles: both modes, ranks 1-6 and Solo/Duos.
  Their rank stats, item-stat theft, ritual healing, enthrallment, archer/golem
  summons, guard/toast effects, deferred damage, poison and escape thresholds are
  included. Guards require every named power effect, a second escape, deferred
  payments and enthrallment to occur. Tamer cues do not dispatch mana-cast hooks.
  Catalog schema 14 exports Tamer definitions, unit identity and item tiers/hats;
  setup carries owner-scoped powers. Tamer selection/rank progression and crew
  offers still belong to the unfinished match flow.
- Auto Chess: all three custom abilities across ten casters, 25 traits at 51
  breakpoints, ten held-item hooks, two combat relics and the Druid strength gate
  are implemented. Another 340 reference battles cover casts, breakpoints, every
  held-item/relic identity, duplicate equipment, Solo/Duos ownership and mixed
  effects. Targeted cases verify Wizard breakpoint reductions, Demon/Slayer
  suppression, Olympos race exclusions, all Druid cap levels, transformation data
  and summon limits; guards require the four-helper cap and defensive effects to
  occur. Schema 15 carries authored summon counts and transformation modifiers.
  Priest loss reduction, relic grants/economy, Digi-Egg use, loot/crew offers and
  full-match progression remain pending. An audit of all active power registries
  found combat callbacks only on the eight Underlords builds already ported.
- Reference fixtures are streamed into eleven content-addressed gzip shards with
  a manifest containing SHA-256 hashes and per-file/total battle counts. The native
  runner checks integrity and counts while reading one battle at a time. Export
  publishes the manifest only after every shard succeeds, then removes obsolete
  shards. This preserves prior usable fixtures on failed exports and avoids the
  monolithic file's GitHub size warning without dropping samples or coverage.

Shared presentation events now retain cast names, projectile/impact types, amplified
hit flags, damage kind and critical flags, floating labels, area position/radius/team
and leap origins. A separate 156-battle reference suite compares 42,626 ordered cues
from all 14 shared abilities and 36 shared items, including criticals and all three
damage kinds. It also catches duplicate shield cues. Fates and Faction Wars presentation callbacks now add 305 production battles
with 87,541 ordered cues across abilities, trait breakpoints, items, augments and
Solo/Duos. Reference replay preserves side modifiers and checks the original
battle duration/winner; guards require all authored floating labels to occur.
Fates covers Lunar growth, Ascension, Push-ups, Soulmon, Rewind, shield/revival,
artifact and bounce-projectile cues. Faction Wars covers DigiSoul, Spirit Evolution,
Insectoid shots/blind misses, Muck, freezing and Undying. Auto Chess and Gizmos add
730 battles and 295,415 ordered cues: transformations, summon and mutation labels,
silence, Stone, Divine Shield stacks, item beams, revival, decoys, X-Antibody,
Overload bursts, Copy, Smoke Bomb and blocked spell feedback. Named-effect guards
require all of these to occur; production replays include relics, all mutation
choices, Glutton stats, augment ownership and Duos partner state. The event suite
also includes 392 Reckoning battles and 128,714 cues: hook-pull leap origins,
Sentinel and resurrection feedback, possession/pet commands, self-damage kind,
shadow/radiant item projectiles, Cannoneer and death bursts, spell blocks and
Respite saves. PvE modifiers and Duos setup are retained in the reference replays.
Exact event order catches death bursts that occur after damage and its nested
callbacks. Dragonlands adds 374 battles and 137,414 cues, covering all five
Copycat outcomes, Jade/Mirage/Cannoneer bursts, Thunder beams, executions,
transformation/hero feedback, wishes, Monolith and gold-item labels. Its replay
setup preserves Mirage, bank balances, Dragon limits, round pace and Duos state.
Arcane adds 673 battles and 271,670 cues across all four custom abilities, traits,
items/contraband, augments and anomalies. Replay carries sponsor items, stored
side state, anomaly selections and 119/120-mana boundary overrides. Guards require
every anomaly label and named trigger to occur; Devour emits its consumption cue
only when an ally exists. The suite now covers 2,630 battles and 963,380 cues.
Coliseum adds 672 battles and 283,616 cues across its six custom abilities, six
forms, 24 Digi-Cores, traits and items. Named-effect guards include Mega Mech,
Supreme, Luchador, Bulk Up, Spirit Evolution, Overdrive, Ascension, All Out,
Vanish and Stasis. Form/core selections, stored Royal Court AP, Supreme selection
and round state are retained in replay. The suite totals 3,302 battles and
1,246,996 cues. Galaxies and Underlords presentation callbacks and complete replay
coverage remain pending; this is not a renderer.

Latest verification: the native test suite passes 102,011,307 assertions.
`./native/build.ps1 -Publish` compiles without warnings, renders the workbench,
purchases through the native UI, and runs the self-contained executable. Fixtures compare against actual methods in the existing
JavaScript engine; they do not establish whole-match parity.

## Next implementation sequence

1. Audit combat registry coverage and remaining cross-set mechanics.
   Finish hook context side/trait state and
   presentation event metadata. Extend reference battles to real set definitions
   before claiming full combat parity.
2. Complete combat acceptance and replay/event metadata for every ruleset,
   then integrate deterministic full-match trace comparisons.
3. Full match state and commands: planning board, economy, round progression, AI,
   offers, carousel, items, Duos and shared health. Special shop offers are pending too.
4. Native asset pipeline and 3D renderer/animation; production screens and audio.
5. Steam and updater adapters, profile import/rollback, end-to-end acceptance and release.

Maintain pure rules with no renderer/network dependency. The build-time JavaScript
exporters are reference adapters, not a JavaScript runtime inside the native app.
Preserve source attribution and existing player profiles. Keep work on
`feature/brutal-port` until production acceptance criteria are satisfied.
