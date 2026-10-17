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
  Fates summoned item-hook rebinding and presentation movement timers are implemented.

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
and round state are retained in replay. Galaxies adds 442 battles and 104,242
cues across all 52 ability types, trait strikes, DigiXros assembly/ejection,
items and Galaxy modifiers. Replay retains bounty state; named-effect guards
cover transformations, rescue, overload, ejection and defensive item triggers.
Underlords and Knockout add 1,898 battles and 1,333,172 cues across all custom
abilities, hero passives, alliances, items and eight Tamer builds at all six ranks.
Replay carries stored duel/beast bonuses, global items, powers and Duos ownership.
The isolated Underlords cast fixtures retain disabled hook dispatch; production
fixtures retain the real hooks. Named-effect guards cover passive triggers,
revivals, summons, defensive effects and every Tamer cooldown cast.
The suite totals 5,642 battles and 2,684,410 cues across all sets. This verifies
the recorded presentation stream, not the renderer. Movement origin, elapsed time
and duration are now retained separately from board occupancy and action cooldowns.
The 5,699 gameplay traces compare these four values at every captured sample,
including 210,720 samples in transit (8,296 stunned and 36,967 fallen), and travel
durations of 0.2, 0.25, 0.3 and 0.4 seconds. Instant placement resets interpolation,
while dead units retain their last travel state, matching the original engine.
The new Presentation assembly converts this travel state into world-space positions,
target-facing angles, mirrored viewer teams and death visibility. Each scene owns
its event cursor and death times; it reuses a visible-unit buffer and does not mutate
combat. A build-time fixture adapter executes the original render3d.js methods with
GPU/DOM work stubbed, checking 282,124 outputs across 22 seven-/fourteen-column scene
traces. This is scene projection only: GPU drawing, model animation, effects and
production screens remain pending.

Native asset ingestion now exports all 718 indexed model files (905 keys), resolves
all 5,232 unit forms and retains model metadata/attribution. The build-time converter
uses the existing vendored meshopt decoder, reconstructs embedded GLB buffers and
packs them with gzip (135,924,220 bytes). The C# Assets assembly verifies SHA-256,
reads GLB chunks and quantized/interleaved accessors, including skin matrices and
animation channels. First/last elements of every accessor are checked using the
vendored Three.js BufferAttribute implementation: 992,532 component values and
1,335,036 total model checks. Packaged smoke loads and verifies all 718 payloads.
Native skeletal sampling now loads node hierarchy/rest transforms, skins and clips;
per-instance poses evaluate translation/scale and quaternion tracks, loop/clamp time,
compose world transforms and calculate mesh-relative skin palettes. Double-precision
hierarchy math and the original quaternion interpolation behavior avoid instability
in tiny-scale imported rigs. Zero-scale hidden attachments receive zero palettes.
The fixture adapter uses the vendored Three.js AnimationMixer/Matrix4 on all 718 rigs,
sampling selected node/joint transforms at four times per authored clip plus rest
poses. It supplies 2,609,044 reference and repair checks. All current source tracks
are LINEAR; STEP is supported, while CUBICSPLINE and morph targets require conversion.
Andiramon has one nonfinite animation clip, and ShineGreymon has four nonfinite
inverse binds (the earlier 201-component finding included both). The runtime holds
the previous valid key/bind pose for 35 corrupt keys and reconstructs four inverse
binds from rest-pose joint transforms. Invalid original transforms are excluded from
reference equality; repaired instances instead undergo finite world/palette checks
throughout every clip. Packaged smoke now constructs every rig too.
The texture pipeline now converts all 1,142 embedded images to RGBA8 PNG at build
time. Native C# decoding verifies PNG checksums, reverses scanline filters and
checks the decoded dimensions and pixel hashes against Pillow's source decoding.
The packaged smoke also decodes every image. Python/Pillow are not runtime dependencies.
Material decoding now matches the original GLTFLoader plus the actual material
correction block in render3d.js. All 1,200 materials and 1,142 texture samplers pass
26,052 checks for color/opacity, cutouts, sidedness, depth writing, unlit rendering,
roughness/specular values and map bindings. Native textures use SRGB8_ALPHA8 for
color and RGBA8 for data maps, retain glTF row orientation and generate mipmaps
only when the sampler requires them. GPU resources are context-thread-owned and
disposed explicitly; transfers preserve the caller's pixel-store/PBO state.
Packaged smoke uploads every texture and checks exact pixel readback, actual sampler
parameters, internal formats and mip dimensions. A non-default pixel-store probe
also checks transfer isolation and restoration.
The workbench now renders the first shop offer or most recently owned model below
the shop. Static vertex/index buffers are shared across mesh instances; GPU bone
palettes drive authored idle clips. Draws apply source rotation, sidedness, cutout
thresholds, depth-write behavior and color textures. The preview fits posed bounds
and uses provisional diffuse lighting, rather than the production arena setup.
Mesh loading covers 7,850,058 vertices in 2,688 triangle primitives across all 718
assets. Reference fixtures use the original GLTFParser and SkinnedMesh vertex
evaluation at two idle times: 66,228 checks cover sampled geometry/weights and posed
bounds. Nonfinite original bounds are excluded from reference equality; rig repair
checks remain separate. Accessor and buffer-view indexing is cached per asset to
avoid repeated JSON array traversal during mesh loading. Packaged smoke renders
GeoGreymon and Hagurumon through the shop purchase sequence and checks visible pixels.
The build now invokes the original modelClips preparation and writes native motion
data for every source, preserving anim=false and fallbackRoles behavior. Authored
tracks reference the existing repaired native clips; 459 generated fallback clips
carry explicit node targets and float32 key data. Prepared rest transforms and six
weapon-parent changes are retained. Content-addressed gzip payloads are verified by
length/SHA256 and native hierarchy/key validation before use. The 905 source mappings
share 667 payloads (1,606,946 bytes). Reference fixtures compare selected prepared
world transforms at rest and four times per valid clip, totaling 1,266,912 checks.
The workbench uses prepared rigs, and packaged smoke prepares all 905 sources.
Original nonfinite tracks remain covered by the existing repair checks instead of
reference equality. This does not establish rendering parity: production material
shaders (including normal/roughness/specular maps), transparent depth sorting,
production battle visuals and arena themes/lighting remain unfinished.

Native ModelAnimator now implements UnitView action priorities, the 60% attack
retrigger gate, .55-second attack and 1.1-second special speed limits, repeating
victory clips, .08-second fades and .15-second idle recovery. Per-property normal
weight blending preserves partial-track rest values and quaternion interpolation.
Fading matches Three.js float32 control timestamps/results with double arithmetic,
including the finished-event ordering that restarts idle after its current update.
Reference traces invoke the original play/endOneShot methods and AnimationMixer:
2,399,704 state/pose checks cover 873 sources with valid animated reference clips.
Goblimon's sensitive blend case additionally compares every local transform.
Static sources and original nonfinite clips are excluded from this reference suite;
the separate rig repair tests still cover corrupt authored data. The workbench
exposes action buttons and packaged smoke exercises action protection while drawing.

CombatChoreography now connects authoritative simulation events to an independent
model driver. It preserves consume-before-view-sync ordering, once-only victory
actions, view replacement on form changes, cast priority for shots, and the original
accepted/rejected-attack lunge distances. Per-frame outputs carry smoothed facing,
body lunge/recoil, damage flash, cast intensity/ring pulse/label lifetime and death
shrink/sink transforms. Simulation time controls effects; frame delta advances models
and facing. The desktop driver caches prepared rigs and owns playback per unit,
including random visual-only idle offsets. No gameplay state is changed.
Reference traces execute the original selected consume cases, UnitView.update and
death/label updates with only GPU/audio/DOM operations stubbed. There are 610,546
checks across 44 battles, including both viewing orientations and 7/14-column boards.
Element colors and particle/audio effects are not established by this fixture.
Packaged smoke drives real prepared models through battles in all 22 catalogs and
checks finite poses. Production battle visuals and effect materials are still pending.

The native battle workbench now draws real combat on a hex board. Test battle uses
the purchased roster or a six-unit demonstration, with pause/resume and return-to-shop
controls. Health bars, hover names and cast labels accompany skeletal animation,
movement, facing, lunges and death transforms. GpuModel owns shared geometry/textures;
ModelRenderer owns the shader and reusable bone buffer; each actor owns its animator.
The preview uses the same rendering path. Models use the original stage-height,
footprint, rotation and flyer-offset formulas. Fresh template bounding boxes now
match the original Box3 path (including local box corners transformed to world),
raising mesh reference coverage to 74,844 checks across all 718 models.
The packaged test now renders four seconds of a twelve-unit battle in addition to
the shop/model preview checks, with 25 model draw calls using nine shared models.
Captured frames were visually inspected. This is not a complete match or production
renderer: arena themes, lighting/PBR, transparent depth sorting, particles, audio,
full unit HUDs and missing-model placeholders still need completion.

Latest verification: the native test suite passes 125,445,589 assertions.
The self-contained publish compiles without warnings, renders the workbench,
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
