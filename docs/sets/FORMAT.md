# Set format

Every match plays one set: its Digimon lines, traits, items, augments, economy and set mechanic. This file is the contract for set authors. The working examples are `js/sets/faction-wars.js` + `faction-wars-mech.js` (the full original game) and `js/sets/_template.js` + `_template-mech.js` + `_template-art.js` (a small set that uses XP, interest, streaks, a 2-copy line, a 2-slot line and set art).

You never need to edit `js/combat.js`, `js/game.js`, `js/itemart.js` or `js/moves.js`. Everything a set does goes through the fields and hooks below.

## Files

| File | Holds | Registers with |
|---|---|---|
| `js/sets/<id>.js` | data: lines, traits, items, augments, rules, wild rounds | `DT.Sets.register({ ... })` |
| `js/sets/<id>-mech.js` | behavior: trait, item, augment and mechanic hooks, consumables, offer kinds, actions | `DT.Sets.extend('<id>', { ... })` |
| `js/sets/<id>-art.js` | item portraits, move looks, basic attack looks | `DT.Sets.extend('<id>', { art: { ... } })` |
| `docs/sets/<id>.md` | design notes, corpus mapping, balance notes | (docs only) |

`index.html` already loads all three files for: `faction-wars`, `galaxies`, `fates`, `reckoning`, `gizmos`, `dragonlands`, `arcane`, `coliseum`, `underlords`, `autochess`, `_template`. Empty stub files mean "not built yet": nothing registers, so the set picker and Chaos skip that set. Do not rename these files. A new set id needs one more script line in `index.html` (ask whoever owns the integration).

Files are plain scripts (no modules), wrapped like this:

```js
(function (G) {
  const DT = G.DT;
  // ...
  DT.Sets.register({ id: 'galaxies', ... });
})(typeof window !== 'undefined' ? window : globalThis);
```

They load after `js/data.js`, `js/items.js` and `js/sets.js`, before the engine. They also run headless in Node (`tools/headless.cjs`), so no DOM, `window`, `fetch` or timers.

### Determinism (multiplayer)

Every client runs the same game from one seed and one action log. Rules code must be deterministic:

- No `Math.random()`, `Date.now()`, `performance.now()` or timers. Use `c.rng()` (seeded) in hooks.
- No iteration over `Set`/`Map`/object keys whose order depends on timing. Object key order is fine (insertion order).
- Never call `rng()` inside a `sort` comparator (browsers sort differently). Score first, then pick.
- `node tests/sim.js` and `node tests/lockstep.js` run in strict mode: `Math.random` and `Date.now` throw.

## DT.Sets.register(def)

```js
DT.Sets.register({
  id: 'galaxies',                 // unique, lowercase; used in lobby records and logs
  name: 'Galaxies',               // picker and codex title
  mode: 'tft',                    // 'tft' | 'underlords' | 'autochess' (picker label; rules come from `rules`)
  desc: 'One short sentence for the picker.',
  colors: { main: '#7a8cff', accent: '#ffcf4a' },   // main: picker dot and border
  source: 'TFT Set 3 Galaxies. docs/inspo/tft/set-03-galaxies.md',
  order: 3,                       // picker order (lower first)
  hidden: false,                  // true: not in the picker or Chaos (tests can still play it)
  chaos: true,                    // false: shown in the picker but never picked by Chaos
  modelsById: false,              // true only for Faction Wars (models/index.json keys are '<lineId>:<stage>')
  roles: {},                      // stat templates to add or override (see Roles)
  lines: [ ... ],
  traits: { ... },
  items: { ... },
  augments: [ ... ],              // augments and Tamer powers (kind: 'power')
  powers: [ ... ],                // optional separate list of Tamer powers
  wisps: [ ... ],                 // optional; Faction Wars uses it for Data Wisps (any set can reuse the data slot)
  rules: { ... },                 // economy and round structure
  pve: (stage, round, game) => [ ... ],   // wild rounds, or { wave(stage, round, game), reward(ctx, won) }
  mechanic: { id, name, desc, hooks, actions, offerKinds },
  hooks: { ... },                 // set-level game/combat hooks (same as mechanic.hooks)
  abilities: { ... },             // new ability types
  lineAi: (line, defaults) => ({}),   // optional AI hints per line (see AI)
  codexRules: (rules) => [{ id, title, html }],  // optional codex Rules tab sections
  unitNote: (unit) => 'text' | ['text', ...] | null,  // optional tooltip / shop card lines for a unit (see Unit notes)
  sellValue: (unit, base) => gold,    // optional: what selling a unit pays (base = the engine table); the UI shows it too
  tamers: ['Tai', 'Matt', ...],   // 7 AI Tamer names
});
```

## Lines

Same shape as the old `DT.LINES`. Build them with `DT.line(id, tier, traits, attr, forms, abilityNames, abilityType, options)`:

```js
const L = DT.line;
L('agumon', 1, ['DR', 'Striker'], 'Vaccine', ['Agumon', 'Greymon', 'MetalGreymon'], ['Pepper Breath', 'Nova Blast', 'Giga Destroyer'], 'nuke'),
L('t_dragon5', 5, ['EMBER', 'VAN'], 'Virus', ['Wyrmling', 'Wyrm', 'Elder Wyrm'], ['Wyrm Breath', 'Wyrm Breath', 'Elder Breath'], 'aoe',
  { role: 'Bruiser', slots: 2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
```

| Field | Meaning |
|---|---|
| `id` | unique within the set. Prefer a set prefix for new lines (`gal_`, `fat_`) so ids never collide in shared tables. |
| `tier` | cost 1-5 (shop odds and pool size come from the tier) |
| `traits` | trait ids; a unit counts once per trait however many copies are on the board |
| `attr` | `'Vaccine' \| 'Data' \| 'Virus' \| 'Free'` (Vaccine > Virus > Data > Vaccine, +20%) |
| `forms` | 3 names: base, ★★, ★★★ |
| `abil` | 3 ability names (one per form); the move look is keyed by name |
| `type` | ability type: `nuke aoe stun slam shield heal barrage drain dash weaken rally line execute chain` or one from `def.abilities` |
| `role` | stat template (`Guardian Striker Sniper Mystic Assassin Bruiser Duelist Support` or one from `def.roles`). If missing, the first trait whose kind is `'role'` (Faction Wars style). |
| `shop` | `false` for wild (PvE-only) units |
| `boss` | wild boss (codex grouping only) |
| `levels` | 3 stage names for the codex (default Rookie, Champion, Ultimate) |
| `hpMul`, `adMul` | stat multipliers |
| `range`, `mana`, `startMana`, `as`, `armor`, `mr` | override the template |
| `radius` | area size for aoe / slam / drain |
| `shots` | barrage base shots (default 4) |
| `combine` | copies needed to digivolve this line (default `rules.copies`, 3). Auto Chess style: `combine: 2`. |
| `slots` | team slots the unit uses (default 1). Dragonlands dragons: `slots: 2`. |
| `noSell` | `true`: the unit cannot be sold (the UI says so; the AI never sells it). Also works as a field on one unit. |
| `noItems` | `true`: the unit cannot hold items (equipping is refused with `'cantHold'`; the AI skips it). Also works as a field on one unit. Underlords Tamers use both. |
| `leap` | jump to the enemy backline at combat start (default from the template; Assassin leaps) |
| `model` | model index key(s): `'agumon'` (uses `agumon:0`, `agumon:1`, `agumon:2`), `'extra:Foo'` (one key for all forms), `['k0', 'k1', 'k2']`, or `{ 0: 'k0', 2: 'k2' }` |
| `unassigned` | `true`: always the placeholder creature (models get assigned later) |
| `color` | placeholder and portrait color (default: first non-role/class trait color) |
| `ai` | AI hints, see AI |
| `abilText` | string or `(stage, value) => string` for the codex, instead of the type's text |

A line with no `model` (and a set without `modelsById`) renders the placeholder creature, in the shop, the codex, portraits and on the board. Model keys are the keys of `models/index.json`; any existing key works (`tools/convert/new-chains.json` lists the ripped ones).

Stats: `DT.unitStats(line, stage)` = template stats x `(1 + 0.18 x (tier - 1))` x `[1, 1.8, 3.24][stage]` x `hpMul` / `adMul`. Ability value = type base x `[1, 1.7, 3][stage]` x `(1 + 0.25 x (tier - 1))` at 100 AP.

## Roles (stat templates)

```js
roles: {
  Brawler: { hp: 800, ad: 50, as: 0.6, range: 1, armor: 40, mr: 30, mana: 80, startMana: 20,
    ai: { place: 'front', tank: 2, carry: false, dmg: 'ad' } },
},
```

`leap: true` on a template makes its units jump to the backline. The `ai` block is explained under AI.

## Traits

Same shape as the old `DT.TRAITS`, plus `hooks` and `ai`:

```js
traits: {
  EMBER: { name: 'Embers', kind: 'origin', color: '#ff7a3a', bp: [2, 4], lv: [0.15, 0.35],
    desc: 'Basic attacks from Embers burn their target.',
    fmt: (v, i) => `burn on hit, +${DT.pct(v)} vs burning`,   // codex text per breakpoint (or bpDesc)
    bpDesc: ['...', '...'],          // optional, used instead of fmt
    notes: 'Optional extra line in the codex.',
    icon: 'EM',                      // optional badge text or image URL
    crestSymbol: 'flame',            // optional symbol for auto-drawn emblem art (DT.ItemArt.kit.SYM keys)
    heal: [0.1, 0.2],                // any extra per-breakpoint arrays: read with c.val('heal')
    hooks: { ... },                  // usually added in <id>-mech.js
    ai: { bonus: (a) => 0 },         // optional AI value, see AI
  },
}
```

- `kind` is free text (`family`, `role`, `origin`, `class`, `alliance`, `race`...). The codex makes its filters from the kinds. Random emblems prefer traits whose kind is not `role` or `class`.
- `bp` = unit counts for each breakpoint; `lv[i]` = the value at breakpoint `i`. In hooks, `c.lv` is 1-based and `c.v === lv[c.lv - 1]`.
- Traits count different shop lines (copies count once; wild units and the bench never count). A unit's `bonusTraits: { EMBER: 1 }` adds to the count (Chosen, dragons' +3 trait); mechanics can change counts with `onCountTraits`.
- A trait with no hooks does nothing in combat. Stat effects are a few lines in `onCombatStart`.

## Items

Two ways, which can be mixed.

### TFT style (components and recipes)

```js
items: {
  system: 'tft',
  components: {
    sword: { name: 'Power Chip', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    egg: { name: 'Digi-Egg', short: 'EG', color: '#fff3b0', stats: {}, special: true, desc: '...' },  // special: not a stat component
  },
  // [a, b, name, desc, passives, extraStats, { id, tags, aiRank, hooks, cat, color, color2, short, noDrop }] or an object with those fields
  recipes: [
    ['sword', 'sword', 'Omni Blade', 'Deals 12% more damage.', { amp: 0.12 }, { ad: 0.15 }, { tags: ['ad'] }],
  ],
  emblemFrom: 'egg',                      // special component that makes emblems
  emblems: { sword: 'Striker' },          // egg + sword = Striker emblem
  dropEmblems: ['DATS'],                  // emblems with no recipe (Armory, loot)
  dropEmblemSource: 'the Armory and loot',   // codex text: where drop-only emblems come from (default: the Armory, if
                                          // the pace has one, and loot)
  emblemName: (T) => `${T.name} Crest`,   // default "<Trait> Emblem"
  emblemIdPrefix: 'fat_',                 // optional: emblem ids become 'fat_emblem_<trait>' (default 'emblem_<trait>');
                                          // use it when another set has a trait with the same id
  special: [{ id: 'golden_egg', name: 'Golden Digi-Egg', desc: '...', p: { teamSize: 1 }, from: ['egg', 'egg'] }],
  list: [ ... ],                          // any other items (below)
  aiRoles: { ap: [...], ad: [...], tank: [...] },   // optional; default from tags
  armory: (ctx) => ['id', 'id', 'id'],    // optional Armory options instead of the default
  core: [...],                            // optional list of completed items random drops use (default: all completed recipes and list items)
}
```

Item ids come from the name (`'Omni Blade'` -> `omni_blade`) unless `id` is given. Recipes for every component pair are not required; a component dropped on a unit with no matching recipe just becomes another held item.

Emblem ids are `emblem_<trait id in lower case>`, or `<emblemIdPrefix>emblem_<trait>`. In code, look them up with `DT.emblemOf('TRAIT')` (the active set's `DT.EMBLEM_OF` table) instead of building the string. Faction Wars, Dragonlands (`drg_`), Fates (`fat_`) and Gizmos (`giz_`) shared trait ids, so the last three use a prefix.

An item made by more than one component pair lists them all in `recipes: [[a, b], [c, d], ...]` (on a recipe row's option object or a `list` item). The first pair is its `from` unless `from` is given. `DT.recipesOf(id)` returns every pair; the item card and codex show all of them (Reckoning's Shadow items: Spore + component either way, or two Spores).

### Any other items (`items.list`)

```js
list: [
  { id: 'gal_star_shard', name: 'Star Shard', cat: 'radiant', desc: '...', stats: { ap: 30 }, p: { amp: 0.1 }, tags: ['ap'], color: '#ffd84a', color2: '#7a4a10' },
  { id: 'ul_blink', name: 'Blink Dagger', cat: 'tiered', tier: 1, desc: '...', stats: {}, tags: ['carry'] },   // Underlords tiers
  { id: 'ul_banner', name: 'Battle Banner', cat: 'global', desc: 'Your team gains 10% AS.', hooks: { ... } },  // affects the whole team
  { id: 'remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', desc: '...' },     // use() in the mech file
]
```

Item fields: `id, name, desc, cat, stats, p, hooks, from, recipes, tier, tags, aiRank, ai: { bonus(ctx) }, color, color2, short, noDrop, use, preview, errors`.

Categories (`cat`): `component`, `completed`, `emblem`, `radiant`, `artifact`, `support`, `shadow`, `consumable`, `global`, `special`, or any set-specific name. The engine treats them alike except:

- `component`: combines with another component on a unit (via `recipes`).
- `consumable`: used up when dragged onto a unit; runs `use(ctx, unit, idx)` and returns a result string for the UI. An error code means nothing happened: `'noItems'`, `'benchFull'`, `'full'`, `'locked'`, `'cantHold'`, `'maxed'`, `'poolEmpty'`, `'phase'`, `'busy'`, or a code of your own with its message in the item's `errors: { code: 'Message' }`. Anything else (the item id is the usual return) counts as used, and the UI says "<item> used". While the item is dragged over a Digimon, the card shows `preview(unit, player, game)` (a string or `{ text, ok, detail }`; `ok: false` marks it as refused), or `preview: 'remover' | 'reforger'` for the built-in item-removal texts, or else the item's own `desc`.
- `global`: not held by a unit; picking or equipping it adds it to the player's `globals`, whose hooks run for the whole team every combat.
- `completed` (with no `noDrop`): handed out by random drops (Armory, loot orbs, Thief Glove).

`stats` keys applied by the engine: `ad` (fraction of base AD), `as` (fraction), `ap`, `mana`, `armor`, `mr`, `hp`, `crit`, `dodge`, `omnivamp`. Other keys are allowed for hooks to read (add a label to `DT.STAT_LABEL` for the item text).

`p` (passives) is the engine's item passive library. Numbers from several items add up in `u.P`. Any set can use these:

| Key | Effect |
|---|---|
| `amp: 0.12` | deal 12% more damage |
| `vamp: 0.2` | heal for 20% of damage dealt (omnivamp) |
| `dr: 0.12` | take 12% less damage (cap 60% total) |
| `regen: 0.012` | regenerate 1.2% max HP per second |
| `critDmg: 0.1` | +10% crit damage |
| `abilCrit: true` | abilities can crit |
| `lifeline: { at, shield, as, ad, armor }` | once below `at` HP: shield (fraction of max HP), attack speed, AD fraction, armor/MR |
| `slayer: 0.25` | +25% damage to enemies with 1600+ max HP |
| `guardbreak: 0.25` | +25% damage to shielded enemies |
| `shojin: 5` | attacks restore 5 extra mana |
| `rage: 0.05` | +5% attack speed per attack, stacking |
| `kraken: 0.03` | +3% AD per attack, stacking |
| `titan: true` | attacking or being attacked: +2% AD and 2 AP, up to 25 stacks |
| `split: 0.5` | attacks also hit a second enemy for 50% |
| `shred: 0.3` | attacks cut armor by 30% for 3 s |
| `volt: 80` | every 3rd attack: lightning to 3 enemies for 80 magic damage |
| `castAS: 0.5` | after casting, +50% attack speed for 5 s |
| `bluebuff: true` | max mana -10, keep 20 mana after casting |
| `zap: 150` | enemies within 2 hexes that cast take 150 magic damage |
| `archangel: 15` | +15 AP every 5 s |
| `manaRegen: 10` | +10 mana every 3 s |
| `crown: true` | 25% max HP shield at start, +30 AP after 8 s |
| `redemption: true` | every 5 s heal allies within 2 hexes for 80 + 10% missing HP |
| `sunfire: true` | every 2 s burn the nearest enemy within 2 hexes |
| `gargoyle: 10` | +10 armor and MR per enemy within 2 hexes |
| `veil: 25` | enemies within 2 hexes lose 25 armor and MR |
| `burn: true` | all damage burns (1.5% max HP/s, healing cut by a third, 5 s) |
| `thorns: 40` | when attacked, 40 magic damage to adjacent enemies (1.5 s cooldown) |
| `ccImmune: 15` | immune to stuns for the first 15 s |
| `thief: true` | gains two random completed items each combat |
| `teamSize: 1` | +1 team size while held on a board unit |
| `emblem: 'TRAIT'` | the holder gains the trait (set automatically on emblems) |

For anything else, give the item `hooks` (combat hooks fire for its holder; see Hooks). A hook on an item fires once per holder however many copies it holds; `c.count` is the number of copies.

## Augments and Tamer powers

```js
augments: [
  { id: 'pocket', tier: 1, name: 'Spare Change', desc: 'Gain 8 gold.', tags: ['econ'], hooks: { onPick(c) { c.player.gold += 8; } } },
  { id: 'training', tier: 1, name: 'Combat Training', desc: '...', tags: ['combat'],
    hooks: { onCombatStart(c) { for (const u of c.mine) { u.ad += u.baseAd * 0.12; u.ap += 12; } } } },
  { id: 'ul_hero', kind: 'power', tier: 2, name: 'Hero of the Hour', desc: '...', hooks: { ... } },
],
```

- `tier`: 1 Silver, 2 Gold, 3 Prismatic. Each augment round rolls one tier for the lobby (`rules.augmentTierOdds`).
- `tags` (AI): `econ` (taken early), `items`, `combat` (later). `ai: { bonus(ctx) }` adds to its score.
- `kind: 'power'` makes it a Tamer power (Underlords Underlord): offered by an offer of kind `power`, stored as `player.power = { id, rank }`. Rank goes up at each label in `rules.powerRanks` (`onRankUp` fires). Power hooks see `c.rank`.
- Combat hooks of an augment or power run for that player's team every combat. `onPick` runs once when chosen.

## Rules (economy and rounds)

Any key left out uses the engine default for the pace being played (`DT.PACE_DEFAULTS.hyper` or `.normal` in `js/sets.js`; see Pace below). `DT.Sets.DEFAULT_RULES` is the Hyper Roll table.

```js
rules: {
  startHp: 20, startGold: 3, planTime: 30, itemCap: 10, rerollCost: 1, winGold: 2, winGoldPvE: 0, shopSize: 5,
  poolSize: [0, 30, 25, 18, 10, 9],                  // copies per cost
  shopOdds: [null, [100, 0, 0, 0, 0], ...],          // index = level, values = % per cost
  // levels: Hyper Roll (level set by stage) ...
  levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
  // ... or XP (set levelByStage: null)
  xp: { table: [0, 2, 2, 6, 10, 20, 36, 48, 72], perRound: 2, buyCost: 4, buyXp: 4, startLevel: 1, maxLevel: 9 },
  teamSize: (level) => level,                        // optional
  // income: fixed per stage ... or base + interest + streak (set incomeByStage: null)
  incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
  baseIncome: 5,
  interest: { per: 10, max: 5 },
  streak: [[2, 1], [4, 2], [5, 3]],                  // [streak length, gold]
  // damage when a player loses
  lossDamage: (stage) => 2,
  unitDamage: (unit, stage) => [1, 2, 3][unit.stage],   // optional, per surviving enemy unit (TFT style)
  pveLossDamage: (stage) => (stage === 1 ? 1 : null),   // null: use lossDamage
  // rounds
  roundsInStage: (stage) => (stage === 1 ? 3 : 4),
  roundType: (stage, round) => 'carousel' | 'pve' | 'pvp',
  carouselTiers: (stage) => [1, 2],                  // costs offered on carousel rounds
  carouselItems: 'component',                        // 'component' | 'completed' | (game, stage) => itemId
  armoryStages: [2, 3, 4, 5, 6], armoryRound: 1,     // Armory = pick 1 of 3 items
  augmentStages: [2, 3, 4], augmentRound: 2, augmentRerolls: 1,
  augmentTierOdds: [0.35, 0.82],
  offers: [{ kind: 'power', stage: 3, round: 2 }, { kind: 'itemTier', tier: 2, stage: 4, round: 1, count: 3, rerolls: 0 }],
  powerRanks: ['5-1', '7-1'],
  copies: 3, maxStage: 2,                            // digivolve rule
  // optional, label lists instead of the stage list + round pairs above
  augmentRounds: ['2-1', '3-2', '4-2'],              // replaces augmentStages / augmentRound
  armoryRounds: ['2-1', '3-1'],                      // replaces armoryStages / armoryRound
  combatTime: { overtime: 30, limit: 45 },           // seconds: overtime damage ramp starts, combat ends as a draw
  aiLevelTarget: (stage, round) => 6,                // XP rules: the level AI Tamers buy XP toward (default stage + 2, +1 from stage 4)
  hyper: { ... },                                    // per-pace blocks, see Pace
  normal: { ... },
}
```

- `baseIncome` may be a number or `(stage, round) => gold`.
- Offer kinds: `armory` (default generator: components through stage 3, completed items later, sometimes a consumable or emblem), `augment`, `power` (from Tamer powers), `itemTier` (Underlords: pick 1 of `count` items of `tier`, no combining needed), or a kind defined in `mechanic.offerKinds`.
- Mid-game carousels: return `'carousel'` from `roundType`; later carousels let the lowest HP pick first.
- Starting round 1-1 does not have to be a carousel.

## Pace (Hyper Roll and Normal)

Every set plays in two paces. The solo player or the lobby host picks the pace next to the set. It is part of game setup: `new DT.Game({ seed, setId, pace })` and the lockstep init `{ seed, slots, setId, pace }`.

| | `'hyper'` (Hyper Roll) | `'normal'` (standard TFT) |
|---|---|---|
| HP | 20, flat loss damage (2 in stages 1-4, 4 in 5-7, 6 after) | 100; a loss costs stage damage (0/1/3/5/7/9/15 for stages 1-7, 150 from stage 8) plus 1/1/2 per surviving enemy unit by form |
| Levels | set by the stage, no XP | XP: 4 XP for 4 gold, +2 XP a round, levels 1-10 |
| Income | fixed by stage, no interest, no streaks | 5 a round (less in stage 1), interest 1 per 10 gold (max 5), win/loss streak gold |
| Rerolls | 1 gold | 2 gold |
| Rounds | 1-1 carousel, 1-2 and 1-3 wild; then x-1 to x-3 PvP, x-4 wild | 1-1 carousel, 1-2 to 1-4 wild; then x-1 to x-3 PvP, x-4 carousel, x-5 and x-6 PvP, x-7 wild |
| Offers | Armory at x-1 of stages 2-6, augments at 2-2, 3-2, 4-2 | augments at 2-1, 3-2, 4-2, no Armory |
| Timers | 30 s planning, combat overtime at 30 s, draw at 45 s | 40 s planning, overtime at 35 s, draw at 50 s |
| AI | levels by stage | buys XP toward `aiLevelTarget(stage, round)` (4 at 2-1, 6 at 3-2, 7 at 4-1, 8 at 5-1, 9 at 6-1) |
| Length | about 25 combats | about 35-40 combats, ending in stage 6-7 |

The full tables are `DT.PACE_DEFAULTS.hyper` and `DT.PACE_DEFAULTS.normal` in `js/sets.js`.

### Set fields

```js
DT.Sets.register({
  ...
  nativePace: 'hyper',        // the pace the flat `rules` keys describe. Default: 'normal' for mode 'autochess' and
                              // 'underlords' (their own economy), else 'hyper'.
  rules: {
    // flat keys: the native pace's rules, exactly as before. Pace-independent keys (below) apply to both paces.
    itemCap: 10, augmentTierOdds: [0.25, 0.85],
    // optional per-pace overrides, applied last
    hyper: { armoryStages: [3, 4, 6] },
    normal: { offers: [{ kind: 'augment', stage: 2, round: 1 }, { kind: 'drg_treasure', stage: 5, round: 1 }] },
  },
  codexRules: (rules, pace) => [ ... ],   // the second argument is the pace being shown
});
```

How the rules for pace P are built (`DT.Sets.rulesFor(id, P)`):

1. `DT.PACE_DEFAULTS[P]`.
2. The set's flat `rules`. If P is the set's native pace, all of them. Otherwise only the pace-independent keys: the pace keys (`DT.PACE_KEYS`, listed below) are dropped, because they describe the other pace.
3. If P is not the native pace and the mode has a profile for P (`DT.PACE_MODES[mode][P]`, an object or `(nativeRules) => object`), that profile. Auto Chess and Underlords use this for their Hyper Roll compression.
4. `rules[P]` from the set file (and from `DT.Sets.extend(id, { rules: { normal: {...} } })`; extend patches merge into the pace blocks).

Pace keys (`DT.PACE_KEYS`): `startHp startGold planTime rerollCost winGold winGoldPvE levelByStage xp incomeByStage baseIncome interest streak lossDamage unitDamage pveLossDamage roundsInStage roundType carouselTiers armoryStages armoryRound armoryRounds augmentStages augmentRound augmentRounds offers powerRanks combatTime aiLevelTarget`.

Everything else is pace-independent: `itemCap shopSize poolSize shopOdds copies maxStage teamSize carouselItems augmentRerolls augmentTierOdds` and any set-specific keys. Traits, lines, items, augments and mechanic hooks are shared by both paces.

What this means for a set author:

- A set with only flat Hyper Roll rules and nothing else plays Normal with the engine's standard TFT rules. That is enough when the set has no round-specific offers.
- Anything tied to a round label must be restated in `rules.normal` (or `rules.hyper` for a native-normal set): extra `offers` (a Treasure Dragon pick, loot rounds), `armoryRounds`, custom `roundType`, `pveLossDamage` for a special wild round, `powerRanks`.
- Wild rounds: Normal plays them at 1-2, 1-3, 1-4 and x-7; Hyper Roll at 1-2, 1-3 and x-4. `pve(stage, round, game)` should key on the stage, and treat stage 1 rounds generically (`round - 1` = the wild round number).

### Pace in hooks

- `game.pace` (`'hyper' | 'normal'`), `game.R` (the rules for that pace), `game.roundNo()` (rounds played so far, 1 at 1-1, counted with this pace's `roundsInStage`).
- Game hook contexts: `c.pace`, `c.R`. Combat contexts: `c.info.pace`.
- Write timing by stage where you can (an augment that pays out "at the start of each stage" works in both paces). Code that counts rounds (`(stage - 1) * 5 + round`) should use `c.game.roundNo()` instead, and code that scales with game length can read `c.pace`.

### Chaos pace

The lobby host may pick `'chaos'` as the pace; the host resolves it from the seed (`DT.Sets.resolvePace('chaos', seed)`) before writing the init, the same way as a Chaos set. A missing `pace` in an init (lobbies and logs from older builds) plays the set's native pace, which is Hyper Roll for every TFT-mode set.

## Wild rounds (PvE)

```js
pve: (stage, round, game) => [['goblimon', 0, 2, 4], ['kuwagamon', 1, 3, 5]],   // [lineId, stage, col 0-6, row 4-7 (4 = front)]
// or
pve: { wave: (stage, round, game) => [...], reward: (ctx, won) => [{ kind: 'blue', gold: 2 }, { kind: 'gold', items: ['omni_blade'] }, { kind: 'gold', unit: 'agumon' }] },
```

Wild units are lines with `shop: false` (they never count for traits). Without `reward`, the default drops: stage 1 components and a unit, later gold, components, completed items, emblems and consumables.

## Set mechanic

```js
mechanic: {
  id: 'wisps', name: 'Data Wisps', desc: 'Shown in the codex.',
  hooks: { onRoundStart(c) { ... }, onAiTurn(c) { ... } },     // global listeners (all players, every combat)
  actions: { wisp: (c, args) => ... },                        // player actions: the UI sends act('wisp', args)
  offerKinds: {
    galaxy: {
      name: 'Galaxy', sub: 'Pick one.',
      options: (c, exclude) => ['a', 'b', 'c'],               // ids
      choose: (c, id) => { ... },
      aiScore: (c, id) => 0,
      card: (id) => ({ name, desc, img }),                     // how the UI shows an option
    },
  },
  // optional: an announcement the UI shows when the game starts (above the 1-1 carousel, until dismissed).
  // UI only, no rules: read game.mech (set in onGameStart). Galaxies shows its Galaxy this way.
  intro: (game) => ({ title: "This game's Galaxy", name, desc, img, color }) | null,
},
```

Actions arrive from the network in multiplayer: validate `args`. Mechanic state must live on the game or player objects so every client computes it the same way: `game.mech` (game-wide, e.g. the rolled galaxy) and `player.mech` (per player) are plain objects for this. Combat hooks see them as `c.info.mech` and `c.side(team).mech`. A board unit's `mods` object (set data such as a Chosen bonus or a Power Up) is copied to its combat unit as `u.mods`, and carries over when it digivolves. Per-slot shop data goes in `player.shopMeta[slot] = { cost, unit: { stage, bonusTraits: {...}, mods: {...} }, tag, note }` (set it in `onShopRolled`); `cost` replaces the price and `unit` fields are copied onto the bought unit (Fates Partner). The shop card reads it for any set: `tag` is a badge on the card (Partner, Dragon), `unit.stage` shows that form and its stars, `unit.bonusTraits` marks the traits it counts extra (x2), and the hover card adds `note` and the set's `unitNote` for the unit.

### Unit notes

`unitNote(unit)` (in `register` or `extend`) returns a line or a list of lines of plain text that the unit tooltip shows for board, bench and combat units and the shop card hover shows for a shop slot. `unit` is a plain view: `{ line (id), stage, mods, bonusTraits, items }`. Use it for per-unit set data the stats do not show: Coliseum's Digi-Core path and form, Arcane's Anomaly, Fates' Partner.

## Hooks

A hook is `function (c, e)` or `{ order: n, fn(c, e) }`. Lower `order` runs first (default 0). Ties run in source order: traits (in board count order), augments, Tamer power, global items, held items, then the set mechanic and set hooks.

Where hooks live:

| Source | Fires |
|---|---|
| `traits.X.hooks` | while the trait is active for a team; game hook `onRoundEnd` after that team's combat |
| `items.list[].hooks` / `extend({ items: { id: { hooks } } })` | combat: for the holder (`onX` when the holder is the subject); `global` items for the whole team |
| `augments[].hooks` | combat: the player's team; game: that player |
| Tamer power hooks | like augments, with `c.rank` |
| `mechanic.hooks`, `hooks` | every event, every player |

### Combat hooks

`c` (context): `c.t` (seconds), `c.team`, `c.state` (scratch object, one per source per combat), `c.units`, `c.mine` (units of `c.team`); trait sources add `c.id, c.T, c.lv, c.n, c.v, c.val(key), c.members, c.isMember(u)`; items add `c.unit` (holder), `c.item`, `c.count`; augments `c.A`; powers `c.P, c.rank`.

Helpers on `c`: `rng()`, `dist(a, b)`, `survivors(team)`, `enemiesOf(u)`, `alliesOf(u)`, `enemiesNear(team, center, radius)`, `alliesNear(team, center, radius)`, `unitAt(c, r)`, `adjacent(u)`, `neighbors(col, row)`, `freeCellNear(u, toward)`, `lineTargets(u, tg)` (enemies on the hex line from u through tg, up to 8 hexes), `move(u, c, r, dur, { leap })` (move a unit to a free cell: hooks, charges, blinks, escapes; false if the cell is taken), `place(u, c, r)` (put a unit that is off the grid back on a free cell: revives, ejected pilots), `remove(u)` (take a unit off the grid without a knockout), `kill(u, by)`, `castAgain(u, tg)` (cast its ability again now; it fires `onCast` again, so guard against loops), `target(u, mode)` (`closest farthest lowestHp highestHp highestAd random backline`), `damage(src, tg, amount, kind)` (`'phys' | 'magic' | 'true'`), `abilityHit(src, tg, amount)`, `heal(tg, amount)`, `shield(tg, amount)`, `stun(tg, s)`, `freeze(tg, s)`, `silence(tg, s)`, `burn(src, tg)`, `weaken(tg, fraction, s)`, `blind(tg)`, `buff(u, stat, v, s)` (negative v = debuff; `as` is a fraction, the rest flat), `summon(team, lineId, stage, col, row, opts)`, `every(u, key, period)`, `after(seconds, fn, unit?)`, `text(u, 'Frozen')`, `event({ type, ... })`, `teamState(team)`, `counts(team)`, `traitLevel(team, id)`, `activeTraits(team)`, `info` (`{ stage, round, type, mech }` from the game), `side(team)` (`{ augs, power, globals, mech, buffs }`).

Unit fields hooks may change (combat copies only; the board is never touched): `maxHp hp ad baseAd as range armor mr mana maxMana ap shield crit critDmg dodge omnivamp dr amp regen perHexAmp vsBurningAmp abilBurn abilCrit atkAs atkAsMax leap invulnUntil weak weakUntil blind burn buffs lifelines P traits items line stage team c r homeR alive state`; read-only: `mods`, `summoned`. `u.state` is per-unit scratch space. `teamState(team).healAmp` boosts all healing for that team.

| Hook | When | Subject | `e` |
|---|---|---|---|
| `onCombatStart` | after units and items are set up, per team | (team) | `{}` (items: `e.unit`) |
| `onCombatReady` | after every team's start; read finished stats | (team) | `{}` |
| `onTick` | every tick (1/30 s), after units act | (team) | `{ dt }` |
| `onUnitTick` | each unit's update, after burn and timers | unit | `{ unit, dt }` |
| `onAttack` | a basic attack landed (not missed or dodged) | attacker | `{ unit, target, dmg, crit }` |
| `onBeforeDamage` | before mitigation; change `e.mult` | source | `{ unit, target, amount, kind, crit, isBurn, mult }` |
| `onBeforeDamageTaken` | after armor/MR; add `e.dr`, change `e.mult` | target | `{ unit, source, amount, kind, dr, mult }` |
| `onDamageDealt` | after damage | source | `{ unit, target, amount, kind, crit, isBurn, killed }` |
| `onDamageTaken` | after damage, target still alive | target | `{ unit, source, amount, kind }` |
| `onLethal` | HP fell to 0; set `e.unit.hp > 0` to survive | target | `{ unit, source, amount }` |
| `onCast` | after an ability resolves | caster | `{ unit, target, value }` |
| `onKill` | a unit killed another | killer | `{ unit, victim }` |
| `onDeath` | a unit died | the dead unit | `{ unit, killer }` |
| `onShield` | a shield was added (abilities, `c.shield`) | shielded unit | `{ unit, amount }` |
| `onHeal` | HP went up | healed unit | `{ unit, amount }` |
| `onCombatEnd` | combat finished | (team) | `{ winner }` |
| `onAnyAttack`, `onAnyCast`, `onAnyKill`, `onAnyDeath`, `onAnyDamageDealt`, `onAnyHeal`, `onAnyShield` | same events for every unit on both teams | any | as above |

"Subject" events reach the subject team's trait/augment/power listeners, the subject's held items, and the set mechanic. Check `c.isMember(e.unit)` when a trait effect is for members only.

### Game hooks

`c`: `c.game`, `c.player`, `c.stage`, `c.round`, `c.R` (rules), `rng()`, `pick(list)`, `shuffle(list)`, `give(itemId)`, `randomComponent(withSpecial)`, `randomCompleted()`, `randomEmblem(preferBoard)`, `giveUnit(costs, stage)`, `refreshShop(levelBoost)`, `combine()`; trait sources add `c.lv, c.v, c.val(key)`.

| Hook | When | `e` |
|---|---|---|
| `onGameStart` | once, before the first shops (player is null) | `{}` |
| `onRoundStart` | each player, after income, shop and offer | `{}` |
| `onRoundEnd` | after a player's combat (traits: with that combat's levels) | `{ won, pve, gold }`: add to `e.gold` to pay out |
| `onIncome` | computing income (also for the UI; no side effects) | `{ base, interest, streak, augment, bonus }` |
| `onShop` | before a shop roll | `{ level, odds, size, boost }` (set `odds` to an array, `size` 1-8) |
| `onShopRolled` | after a shop roll | `{ shop }` (set `player.shopMeta`) |
| `onBuy`, `onSell` | a unit bought / sold | `{ unit, slot }` / `{ unit }` |
| `onCombine` | a unit digivolved | `{ unit, from }` |
| `onPick` | this augment or power was chosen (mechanic: any) | `{ id, kind }` |
| `onRankUp` | the player's Tamer power ranked up | `{ rank }` |
| `onAiTurn` | an AI player's turn, after its offer pick | `{}` |
| `onEliminate` | a player was knocked out | `{ placement }` |
| `onTeamSize` | computing team size | `{ size }` |
| `canPlace` | a unit is placed on the board | `{ unit, c, r, ok }` (set `ok = false` to refuse) |
| `canEquip` | an item would go on a unit (player equips, AI gives, not consumables) | `{ unit, item, combine, ok, reason }`: `combine` is the item it would make with a held component, or null. Set `ok = false` and `reason` (`'full'`, `'cantHold'` or your own) to refuse. Underlords: one item per hero. |
| `onCountTraits` | `(counts, units, augs)`: change counts in place (augments and mechanic) | |

Consumables: `use(c, unit, idx)` on the item (or `hooks.onUse`).

## New ability types

```js
abilities: {
  summon: {
    base: 0,
    cast(c, u, tg, v) { const cell = c.freeCellNear(u, tg); if (cell) c.summon(u.team, 'gal_drone', u.stage, cell.c, cell.r); },
    text: (line, stage, v) => 'Summons a drone.',
  },
},
```

`v` is the ability value (base x stage x tier x AP/100). Moves without a look get one from their name.

## AI

The AI Tamers read metadata only, so a new set needs no AI code:

- Lines: `ai: { place: 'front' | 'back' | 'flank', tank: 0-3, carry: true, dmg: 'ad' | 'ap', value: 0 }`. Defaults come from the stat template (`ROLE_AI` in `js/sets.js`, or `roles.X.ai`); a line whose ability heals, shields or rallies builds AP. `def.lineAi(line, defaults)` can return hints for every line.
- Items: `tags` (`ad`, `ap`, `tank`; others are free) and `aiRank` (higher first) build the AI's wish lists (`ITEM_ROLE`). Or give `items.aiRoles` explicitly. `ai: { bonus(ctx) }` adds to an item's Armory score.
- Augments and powers: `tags` (`econ`, `items`, `combat`) and `ai: { bonus(ctx) }`.
- Traits: `ai: { bonus({ active, stage, n, counts, game }) }` makes a trait more attractive (Royal Knights like many active traits).
- Mechanic offers: `offerKinds.X.aiScore(c, id)`. Mechanic decisions (buying a Wisp) go in `onAiTurn`.
- `ai.value` adds to a line's score when the AI arranges its board, and a value of 6 or more makes the AI buy the line from the shop even when no trait of its is started (single-trait units such as Coliseum's Justimon and Labramon; Dragonlands' Dragons have 6).

## Art and move looks (`<id>-art.js`)

```js
DT.Sets.extend('galaxies', {
  art: {
    prerendered: false,     // true after tools/render-item-icons.cjs --set galaxies (uses models/items/sets/galaxies/)
    items: {                // item id, 'aug:<id>' or 'wisp:<id>' -> painter(p, kit)
      gal_star_shard: (p, k) => {
        k.frame(p, 'square', 'gold', ['#3a2a10'], { w: 8 });
        p.obj(() => p.at(64, 64, 1, 0, () => p.part(k.star(0, 0, 34, 15), '#ffd84a')));
      },
    },
    moves: { 'Star Shower': { elem: 'holy', charge: 'sky', impact: 'holy', fx: [['meteor', { count: 3 }]] } },   // js/moves.js spec format
    basic: { gal_ranger: { kind: 'shot', elem: 'holy', head: 'star', trail: 'holy', size: 0.22 } },
  },
});
```

- Painters draw on a 128 x 128 SVG with the `js/itemart.js` picture builder `p` (`part`, `line`, `tube`, `glow`, `sparkle`, `at`, `obj`...). `kit` has `frame(p, shape, metal, bgColors, { w })` (shapes `square circle hex shield`; metals `chrome steel gold bronze silver prism dark wood stone glass`), `crest(p, color, symbol)`, the motif painters `M`, symbols `SYM`, shapes (`circ ell rr poly star ngon HEART EGG FLAME DROP BOLT`) and colors (`COL METAL lt dk mix`).
- Items with no painter get a generated badge in their colors. Emblems with no painter get a crest in the trait color (`crestSymbol`).
- The Faction Wars art is shared: it shows for an id only while your item, augment or Wisp with that id has the same name as the Faction Wars one.
- Move specs and basic attack looks follow the format at the top of `js/moves.js`. Your specs win over the shared table. Abilities without a spec get an element look from their name.

## Testing your set

```
node tests/sim.js 20 --set <id> --pace normal       # 20 full 8-AI games, pool/gold/cap invariants, strict determinism
node tests/lockstep.js 3 --set <id> --pace hyper    # host, clients and a late joiner stay identical
node tools/balance.cjs 200 8 --set <id> --pace normal   # top-4 rate by trait, line, wild round win rates, game length
node tools/balance.cjs --duel 5000 8 1 --set <id>
node tests/sets.cjs                        # every registered set: solo to round 2, each pace to stage 3, Chaos, lobby config
node tests/sets.cjs <playwright> <outDir> 8164   # also in the browser
```

In the browser console: `DT.UI.newGame('<id>', 'normal')` starts a solo game of any set, hidden or not, in either pace (no pace: the one picked on the Play page).

Without `--pace`, the tools play the set's native pace. Check both paces before handing a set over.


## Duos format

Duos is independent of set and pace: `new DT.Game({ setId, pace, format: 'duos', teams })`. `teams` is four disjoint pairs of seats 0?7, defaulting to adjacent seats. The same format and assignment are stored in the lockstep init. No format means solo.

`js/duos.js` defines shared augment data, team validation and trait merging. Rules can override `duo: { teamHp, sends, unitScale }` in the set or pace rules. Defaults are the pace's HP, one portal send per Hyper Roll round/two per Normal round, and half of the usual per-survivor damage. Team damage is applied once per team; individual wild fights use the larger loss, not their sum.

- `game.partnerOf(player)`, `game.teamOf(player)`, `game.teamSide(team)` expose partners and the combined side. `canPortal(player, { uid })` or `{ item: index }` validates transfers; `applyAction(player, 'portal', args)` performs the ordered action. Only bench units and unequipped items can be sent during planning.
- Game-hook contexts expose `c.partner`. `onPortal(c, e)` receives `{ from, to, unit, item }`; `onPartnerCombine` supports partner digivolution rewards.
- Team side inputs have `cols: 14`, `parts`, `owners` and precomputed `counts`. A trait's optional `duo` value is `'sum'` (default), `'max'`, or a numeric weight for the lower partner's contribution. Summed counts cap at the highest breakpoint, preserving any greater count already reached by one board.
- Combat units carry `owner`; owner-specific contexts provide `c.owner`, `c.mine`, `c.partnerUnits()` and the partner's top board value. Traits affect the whole combat side; augments, powers and global items are scoped to their owner. Use the combat helpers' grid dimensions, not hard-coded seven-column positions.
- Mark set-provided duo augments `duo: true`; shared duo augments are available in every set but excluded from solo offers.
- `Combat.lastKills[side]` records presentation-only final-hit metadata, including owner, form, source UID and victim. Death events also carry `by`. Neither consumes RNG nor changes outcomes. Kill screens use these records even for AI-only fights and fall back to the strongest board unit if no kill occurred.

Validation: `tests/duos.cjs` covers all registered sets and both paces, portal combines, shared HP/placements, combined traits and deterministic replay. `--only browser` adds real UI flows and local Firebase persistence/fallback checks. The Steam mock suite accepts `DT_DUOS=1`; `tests/killcam.cjs` covers actual elimination playback including the second Duos partner. Keep live platform tests separate from mock transport results.
