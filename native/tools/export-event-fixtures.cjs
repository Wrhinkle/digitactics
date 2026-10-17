// Ordered presentation events from the original shared kernel and migrated set hooks.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const originalHooks = Object.fromEntries(['buildHooks', 'fireTeam', 'fireGlobal', 'has'].map(name => [name, DT.Combat.prototype[name]]));
for (const name of ['buildHooks', 'fireTeam', 'fireGlobal']) DT.Combat.prototype[name] = function () {};
DT.Combat.prototype.has = () => false;
const kinds = ['nuke', 'aoe', 'stun', 'slam', 'shield', 'heal', 'barrage', 'drain', 'weaken', 'rally', 'line', 'execute', 'chain', 'dash'];
const traces = [], casts = new Set(), eventTypes = new Set();
function presentationEvents(cb) {
  return cb.events.map(e => {
    eventTypes.add(e.type);
    if (e.type === 'cast') casts.add(cb.units[e.at].line.type);
    return { time: e.t, type: e.type === 'dmg' ? 'damage' : e.type, unit: e.at ?? e.from ?? -1,
      target: e.to ?? e.by ?? null, amount: e.amount || 0,
      kind: e.kind ? { phys: 'Physical', magic: 'Magic', true: 'True' }[e.kind] : null,
      critical: !!e.crit, text: e.text ?? e.name ?? null,
      cell: e.type === 'leap' ? { column: e.fc, row: e.fr } : e.type === 'burst' ? { column: e.c, row: e.r } : null,
      radius: e.radius || 0, team: e.team ?? null, big: !!e.big };
  });
}
function battle(set, primary, stage, cols, items, label) {
  DT.Sets.activate(set, 'hyper');
  const pool = DT.SHOP_LINES.filter(l => kinds.includes(l.type));
  const seed = 7531 + traces.length, rng = DT.makeRng(seed);
  const entries = () => Array.from({ length: 6 }, (_, i) => ({
    line: i === 0 ? primary : pool[Math.floor(rng() * pool.length)].id,
    stage, c: i * 2 % cols, r: 4 + Math.floor(i / cols), items, uid: i + 1,
  }));
  const a = entries(), b = entries();
  const cb = new DT.Combat({ units: a, noTraits: true, cols }, { units: b, noTraits: true, cols }, { seed });
  let ticks = 0;
  while (!cb.done && ticks < 2000) { cb.step(); ticks++; }
  if (!cb.done) throw new Error('Unfinished event battle');
  const events = presentationEvents(cb);
  const convert = es => es.map(u => ({ line: u.line, stage, column: u.c, row: u.r, items, uid: u.uid }));
  traces.push({ name: label, variant: `${set}/hyper`, seed, cols, ticks, sideA: convert(a), sideB: convert(b), events });
}
for (const kind of kinds) {
  let choice;
  for (const set of DT.Sets.pickable()) {
    DT.Sets.activate(set.id, 'hyper');
    const line = DT.SHOP_LINES.find(l => l.type === kind);
    if (line) { choice = [set.id, line.id]; break; }
  }
  if (!choice) throw new Error(`No caster for ${kind}`);
  for (const stage of [0, 1, 2]) for (const cols of [7, 14])
    battle(...choice, stage, cols, [], `${kind}/${stage}/${cols}`);
}
DT.Sets.activate('faction-wars', 'hyper');
const core = [...DT.CORE_ITEMS], primary = DT.SHOP_LINES.find(l => l.type === 'nuke').id;
for (const item of core) for (const stage of [0, 2])
  battle('faction-wars', primary, stage, stage === 0 ? 7 : 14, [item], `item/${item}/${stage}`);
// Replay production Fates and Faction Wars cases from the verified gameplay corpus. Retain its seeds,
// side modifiers and Duos ownership; no synthetic callback probes are reconstructed.
Object.assign(DT.Combat.prototype, originalHooks);
const directory = path.join(__dirname, '../tests/fixtures/combat');
const manifest = JSON.parse(fs.readFileSync(path.join(directory, 'manifest.json')));
for (const setId of ['fates', 'faction-wars']) {
  const shard = manifest.files.find(f => f.file.startsWith(`${setId}-`));
  const compressed = fs.readFileSync(path.join(directory, shard.file));
  if (require('node:crypto').createHash('sha256').update(compressed).digest('hex') !== shard.sha256)
    throw new Error(`${setId} gameplay fixture integrity failure`);
  const corpus = JSON.parse(require('node:zlib').gunzipSync(compressed));
  if (corpus.length !== shard.battles) throw new Error(`${setId} gameplay fixture count mismatch`);
  for (const trace of corpus.filter(t => (t.name.startsWith('fates-') || t.name.startsWith('faction-')) && !t.summonItemProbe)) {
    const [set, pace] = trace.variant.split('/'); DT.Sets.activate(set, pace);
    const setup = trace.setup || { sideA: { augments: trace.augsA }, sideB: { augments: trace.augsB } };
    const buffs = b => b && ({ as: b.attackSpeed, armor: b.armor, mana: b.mana });
    function side(entries, effects) {
      const units = entries.map(u => ({ ...u, c: u.column, r: u.row,
        mods: u.partner ? { partner: true, bonus: u.partner.bonus, trait: u.partner.trait } : undefined }));
      const result = { units, cols: trace.cols, noTraits: effects.noTraits, augs: effects.augments,
        counts: effects.counts, mech: effects.fates, buffs: buffs(effects.buffs), partnerTop: effects.partnerTop };
      if (effects.parts) {
        result.parts = effects.parts.map(p => ({ owner: p.owner, augs: p.augments, buffs: buffs(p.buffs), partnerTop: p.partnerTop,
          units: units.filter(u => u.owner === p.owner) }));
        result.counts = DT.Duos.teamCounts(result.parts);
      }
      return result;
    }
    const cb = new DT.Combat(side(trace.sideA, setup.sideA), side(trace.sideB, setup.sideB), { seed: trace.seed });
    let ticks = 0; while (!cb.done && ticks < 2000) { cb.step(); ticks++; }
    if (!cb.done || ticks !== trace.tick || cb.winner !== trace.winner || Math.abs(cb.t - trace.time) > 1e-10)
      throw new Error(`Production replay setup differs from gameplay fixture: ${trace.name}`);
    traces.push({ name: trace.name, variant: trace.variant, seed: trace.seed, cols: trace.cols,
      sideA: trace.sideA, sideB: trace.sideB, setup, ticks, events: presentationEvents(cb) });
  }
}
for (const kind of kinds) if (!casts.has(kind)) throw new Error(`No cast coverage: ${kind}`);
for (const type of ['shot', 'hit', 'beam', 'burst', 'dmg', 'heal', 'shield', 'cast', 'death', 'text', 'leap'])
  if (!eventTypes.has(type)) throw new Error(`No event coverage: ${type}`);
const allEvents = traces.flatMap(t => t.events);
for (const kind of ['Physical', 'Magic', 'True'])
  if (!allEvents.some(e => e.kind === kind)) throw new Error(`No damage cue coverage: ${kind}`);
if (!allEvents.some(e => e.critical) || !allEvents.some(e => e.big)) throw new Error('Missing critical or amplified effect coverage');
for (const [prefix, labels] of [
  ['fates-', ['+1 gold', '+2 AP', 'Ascend', 'Frenzy', 'Frozen', 'Full Moon', 'Guardian Wing', 'Lone Blade', 'Pumped Up', 'Push-ups', 'Rewind', 'Rocket Fist', 'Showtime', 'Soulmon', 'Spirit Rush', 'Stasis', 'Tankmon', 'Trap Claw', 'Zephyr']],
  ['faction-', ['DigiSoul', 'Spirit Evolution', 'Muck', 'Frozen', 'Undying', 'Miss']],
]) {
  const labelsSeen = new Set(traces.filter(t => t.name.startsWith(prefix)).flatMap(t => t.events).filter(e => e.type === 'text').map(e => e.text));
  for (const label of labels) if (!labelsSeen.has(label)) throw new Error(`Missing ${prefix} presentation coverage: ${label}`);
}
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/events.json.gz'), require('node:zlib').gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} ordered presentation traces.`);
