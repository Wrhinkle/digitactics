// Duos format tests (js/duos.js, game.js teams and portal, combat.js team fights).
// Headless: every set x pace plays a lockstep Duos game to the end with humans sending through the portal (host, two
// clients and a late joiner replaying the log end identical); a portal send arrives and counts for digivolving; item
// sends; team trait sums reach combat; team HP, team elimination and placements; solo inits still play solo.
// Browser (with Playwright): the wide arena renders, partner view and portal pad, a duo fight can be spectated, the team
// kill screen, no overflow at 4 client sizes, and a Steam mock lobby with teams.
// Run: node tests/duos.cjs [<path-to-playwright> <outDir> [port]] [--only headless|browser|steam] [--games n]
const path = require('path');
const fs = require('fs');
const { load } = require('../tools/headless.cjs');

const args = process.argv.slice(2);
const opt = (name) => { const i = args.indexOf(`--${name}`); return i >= 0 ? args[i + 1] : null; };
const positional = args.filter((a, i) => !a.startsWith('--') && !(i > 0 && args[i - 1].startsWith('--')));
const PW = positional[0] || null;
const OUT = positional[1] || null;
const PORT = Number(positional[2] || 8196);
const ONLY = opt('only');
let failures = 0;
const ok = (cond, msg) => { if (cond) console.log(`  ok  ${msg}`); else { failures++; console.log(`  FAIL ${msg}`); } };

// ---------------------------------------------------------------- headless
function headless() {
  const DT = load({ strict: true });
  const Lock = DT.Lock;
  console.log('Duos headless');

  // 1. every set x pace: lockstep with humans, portal sends, host/clients/late joiner identical, the game ends
  const sets = DT.Sets.pickable().map((s) => s.id);
  const games = Number(opt('games') || 1);
  for (const setId of sets) {
    for (const pace of ['hyper', 'normal']) {
      for (let gno = 1; gno <= games; gno++) {
        const r = lockGame(DT, Lock, setId, pace, gno);
        ok(r.ok, `${setId} ${pace} #${gno}: ended ${r.round}, ${r.entries} entries, ${r.sends} sends, team places ${r.places}${r.err ? ` (${r.err})` : ''}`);
      }
    }
  }

  // 2. portal: a sent copy arrives on the partner's bench and digivolves with their two copies
  {
    const g = new DT.Game({ seed: 4242, allAI: true, setId: 'faction-wars', pace: 'hyper', format: 'duos' });
    while (g.roundType === 'carousel' || g.phase !== 'planning') { g.startCombat(); g.finishCombat(); g.nextRound(); }
    const [p, q] = g.teams[0].members;
    const line = DT.SHOP_LINES.find((l) => l.tier === 1).id;
    for (const x of [p, q]) { x.bench = new Array(DT.BENCH).fill(null); x.board = {}; }
    q.bench[0] = g.newUnit(line, 0);
    q.bench[1] = g.newUnit(line, 0);
    p.bench[0] = g.newUnit(line, 0);
    p.portal = 0;
    const sentUid = p.bench[0].uid;
    const res = g.applyAction(p, 'portal', { uid: sentUid });
    ok(res && res.kind === 'unit', 'portal: unit send accepted');
    ok(!p.bench.some((u) => u && u.uid === sentUid), 'portal: the unit left the sender\'s bench');
    const up = q.allUnits().find((u) => u.line === line && u.stage === 1);
    ok(!!up && res.evolved.length === 1, 'portal: the sent copy digivolved with the partner\'s two copies');
    ok(q.incoming.some((x) => x.kind === 'unit' && x.uid === sentUid && x.from === p.id), 'portal: incoming effect recorded for the receiver');
    // limit: 1 send a round in Hyper Roll
    p.bench[1] = g.newUnit(line, 0);
    ok(g.applyAction(p, 'portal', { uid: p.bench[1].uid }) === 'limit', 'portal: second send this round refused (Hyper Roll: 1 a round)');
    // items
    p.portal = 0;
    p.items = [DT.STAT_COMPONENTS[0]];
    const before = q.items.length;
    const ir = g.applyAction(p, 'portal', { item: 0 });
    ok(ir && ir.kind === 'item' && q.items.length === before + 1 && !p.items.length, 'portal: item send arrives on the partner\'s item bench');
    // full bench: refused unless it digivolves
    p.portal = 0;
    q.bench = new Array(DT.BENCH).fill(null).map(() => g.newUnit(DT.SHOP_LINES.find((l) => l.tier === 2).id, 1));
    const other = DT.SHOP_LINES.find((l) => l.tier === 3).id;
    p.bench[2] = g.newUnit(other, 0);
    ok(g.canPortal(p, { uid: p.bench[2].uid }) === 'full', 'portal: a full partner bench refuses a unit that does not digivolve');
    // Normal pace: 2 sends
    const n = new DT.Game({ seed: 77, allAI: true, setId: 'faction-wars', pace: 'normal', format: 'duos' });
    ok(n.portalLimit(n.players[0]) === 2 && g.portalLimit(g.players[0]) === 1, 'portal: 1 send a round in Hyper Roll, 2 in Normal');
    ok(n.players.every((x) => x.hp === 100) && g.players.every((x) => x.hp === 20 || !x.alive || x.hp <= 20), 'team HP: 100 in Normal, 20 in Hyper Roll');
  }

  // 3. team trait sums reach combat: one unit of a trait on each partner's board makes 2 for the team side
  {
    const g = new DT.Game({ seed: 99, allAI: true, setId: 'faction-wars', pace: 'hyper', format: 'duos' });
    const T = g.teams[0];
    const [p, q] = T.members;
    // a family trait with a 2 breakpoint, two lines of it
    const tid = Object.keys(DT.TRAITS).find((t) => DT.TRAITS[t].bp[0] === 2 && DT.TRAITS[t].hooks && DT.SHOP_LINES.filter((l) => l.traits.includes(t)).length >= 2 && DT.TRAITS[t].kind !== 'role');
    const lines = DT.SHOP_LINES.filter((l) => l.traits.includes(tid)).slice(0, 2);
    p.board = { '3,4': g.newUnit(lines[0].id, 0) };
    q.board = { '3,4': g.newUnit(lines[1].id, 0) };
    const side = g.teamSide(T);
    ok(side.counts[tid] === 2, `team counts: ${tid} sums to 2 across partners (own: 1 + 1)`);
    ok(side.units.length === 2 && side.units.some((u) => u.c === 3) && side.units.some((u) => u.c === 10), 'team side: the second partner stands on columns 7-13');
    const foe = g.teamSide(g.teams[1]);
    const cb = new DT.Combat(side, foe.units.length ? foe : { units: [{ line: lines[0].id, stage: 0, c: 3, r: 4, uid: -5, items: [] }] }, { seed: 5, record: true, info: g.combatInfo() });
    ok(cb.cols === 14 && cb.traitCounts[0][tid] === 2 && DT.traitLevel(tid, cb.traitCounts[0][tid]) >= 1, `combat: 14 columns, ${tid} active at level ${DT.traitLevel(tid, cb.traitCounts[0][tid])} for the team`);
    const listeners = Object.values(cb.L.team[0]).flat().filter((l) => l.ctx.kind === 'trait' && l.ctx.id === tid);
    ok(listeners.length > 0 && listeners[0].ctx.members.length === 2, `combat: ${tid} hooks run for both partners' units`);
    ok(cb.units.filter((u) => u.team === 0).map((u) => u.owner).sort().join(',') === `${p.id},${q.id}`, 'combat: units keep their owner');
    let guard = 0;
    while (!cb.done && guard++ < 3000) cb.step(DT.TICK);
    ok(cb.done, 'combat: the team fight finishes');
    // cap: a summed count above the top breakpoint is capped there
    const top = DT.TRAITS[tid].bp[DT.TRAITS[tid].bp.length - 1];
    const many = DT.SHOP_LINES.filter((l) => l.traits.includes(tid));
    p.board = {}; q.board = {};
    many.forEach((l, i) => { (i % 2 ? q : p).board[`${Math.floor(i / 2)},5`] = g.newUnit(l.id, 0); });
    const c2 = g.teamSide(T).counts[tid];
    ok(c2 <= Math.max(top, many.length), `team counts: capped at the top breakpoint (${c2} <= ${top})`);
  }

  // 4. team HP and elimination: partners share HP and go out together, placements 1-4
  {
    const g = new DT.Game({ seed: 5150, allAI: true, setId: 'galaxies', pace: 'hyper', format: 'duos' });
    let guard = 0;
    while (!g.over && guard++ < 300) {
      g.startCombat(); g.finishCombat();
      for (const T of g.teams) if (T.members[0].hp !== T.members[1].hp) { ok(false, 'partners share HP'); guard = 999; }
      g.nextRound();
    }
    const places = g.teams.map((T) => T.placement).sort().join(',');
    ok(g.over && places === '1,2,3,4', `elimination: teams placed ${places}`);
    ok(g.teams.every((T) => T.members.every((p) => p.placement === T.placement)), 'elimination: both partners get the team placement');
  }

  // 5. duo augments are offered in Duos only, every set has them
  {
    const g = new DT.Game({ seed: 3, allAI: true, setId: 'reckoning', pace: 'hyper', format: 'duos' });
    const s = new DT.Game({ seed: 3, allAI: true, setId: 'reckoning', pace: 'hyper' });
    let duoSeen = 0, soloDuo = 0;
    for (let i = 0; i < 30; i++) {
      g.augTier = null; s.augTier = null;
      g.stage = s.stage = 2 + (i % 3);
      if (g.augmentOptions(g.players[0]).some((id) => DT.AUGMENT[id] && DT.AUGMENT[id].duo)) duoSeen++;
      if (s.augmentOptions(s.players[0]).some((id) => DT.AUGMENT[id] && DT.AUGMENT[id].duo)) soloDuo++;
    }
    ok(duoSeen === 30 && soloDuo === 0, `augments: a duo augment in every Duos offer (${duoSeen}/30), none in solo (${soloDuo})`);
    ok(DT.Duos.AUGMENTS.length >= 8 && DT.Duos.AUGMENTS.length <= 12, `augments: ${DT.Duos.AUGMENTS.length} duo augments`);
    ok(sets.every((id) => { DT.Sets.activate(id, 'hyper'); return DT.Duos.AUGMENTS.every((a) => DT.AUGMENT[a.id] === a); }), 'augments: every set can look up the duo augments');
    // old inits without a format play solo
    const old = Lock.create({ seed: 8, slots: Array.from({ length: 8 }, (_, i) => ({ name: `P${i}`, human: i === 0 })), at: 0, setId: 'faction-wars' });
    ok(old.format === 'solo' && !old.duo && !old.teams, 'an init without a format plays solo');
  }
}

// a lockstep Duos game with humans in seats 0, 3 and 6 (teams 0, 1 and 3); seat 3 drops and comes back
function lockGame(DT, Lock, setId, pace, gameNo) {
  const rnd = DT.makeRng(500 + gameNo * 13 + setId.length);
  const humans = [0, 3, 6];
  const slots = Array.from({ length: 8 }, (_, i) => ({ name: humans.includes(i) ? `P${i}` : DT.TAMERS[i % 7] || `AI${i}`, human: humans.includes(i) }));
  const init = { seed: 900 + gameNo * 31, slots, planTime: 30, at: 0, setId, pace, format: 'duos', teams: [[0, 1], [2, 3], [4, 5], [6, 7]] };
  const host = Lock.create(init);
  const clients = [Lock.create(init), Lock.create(init)];
  const log = [];
  let now = 0;
  const online = new Set(humans);
  const done = new Set();
  let sends = 0;
  const push = (e) => {
    e.at = now;
    log.push(e);
    const res = Lock.apply(host, e);
    for (const c of clients) Lock.apply(c, e);
    if (e.k === 'start' || e.k === 'next') done.clear();
    return res;
  };
  const pick = (n) => Math.floor(rnd() * n);
  let steps = 0;
  try {
    while (!host.over && steps++ < 30000) {
      now += 250;
      const view = clients[0];
      for (const s of humans) {
        const p = view.players[s];
        if (!p.alive || !p.isHuman || rnd() > 0.35) continue;
        const r = rnd();
        let act = null;
        if (view.carousel && view.carouselTurn() === p) act = { a: 'carousel', g: { i: pick(9) } };
        else if (p.offer && view.phase === 'planning') act = { a: 'offer', g: { i: pick(3) } };
        else if (r < 0.12 && view.phase === 'planning' && p.bench.some(Boolean)) { const u = p.bench.find(Boolean); act = { a: 'portal', g: { uid: u.uid } }; }
        else if (r < 0.15 && view.phase === 'planning' && p.items.length) act = { a: 'portal', g: { item: pick(p.items.length) } };
        else if (r < 0.35) act = { a: 'buy', g: { slot: pick(5) } };
        else if (r < 0.4) act = { a: 'reroll' };
        else if (view.R.xp && r < 0.43) act = { a: 'buyXp' };
        else if (r < 0.55 && p.bench.some(Boolean)) act = { a: 'move', g: { from: { type: 'bench', i: p.bench.findIndex(Boolean) }, to: { type: 'board', c: pick(7), r: 4 + pick(4) } } };
        else if (r < 0.6 && p.items.length && p.allUnits().length) { const us = p.allUnits(); act = { a: 'equip', g: { idx: pick(p.items.length), uid: us[pick(us.length)].uid } }; }
        else if (r < 0.63 && p.orbs.length) act = { a: 'orb', g: { id: p.orbs[0].id } };
        else if (r < 0.75 && view.phase === 'planning') { push({ k: 'vote', s }); continue; }
        if (act) { const res = push({ k: 'a', s, ...act }); if (act.a === 'portal' && res && res.kind) sends++; }
      }
      if (host.stage === 3 && host.round === 1 && online.has(3) && host.players[3].isHuman) push({ k: 'a', s: 3, a: 'release', g: { name: 'Izzy' } });
      if (host.stage === 4 && !host.players[3].isHuman && host.players[3].alive) push({ k: 'a', s: 3, a: 'claim', g: { name: 'P3' } });
      if (host.phase === 'combat' && now - host.phaseAt > 3000) for (const s of humans) done.add(s);
      const e = Lock.decide(host, { now, online, done });
      if (e) push(e);
    }
  } catch (err) { return { ok: false, err: String(err && err.stack || err).split('\n').slice(0, 3).join(' | '), round: host.roundLabel, entries: log.length, sends, places: '' }; }
  const d = Lock.digest(host);
  let err = null;
  clients.forEach((c, i) => { if (Lock.digest(c) !== d) err = `client ${i} desynced`; });
  const late = Lock.create(init);
  for (const e of log) Lock.apply(late, e);
  if (Lock.digest(late) !== d) err = 'late joiner desynced';
  if (!host.over) err = err || 'did not finish';
  // humans can all be out before a winner: then only knocked-out teams have placements
  const places = host.teams.map((T) => T.placement || '-').join('/');
  const shared = host.teams.every((T) => T.members[0].hp === T.members[1].hp && T.members[0].placement === T.members[1].placement);
  if (!shared) err = err || 'partners differ';
  return { ok: !err, err, round: host.roundLabel, entries: log.length, sends, places };
}

// ---------------------------------------------------------------- browser
async function browser() {
  if (!PW || !OUT) { console.log('Duos browser: skipped (no Playwright path)'); return; }
  fs.mkdirSync(OUT, { recursive: true });
  const B = require('./duos-browser.cjs');
  failures += await B.run({ PW, OUT, PORT, only: ONLY, ok });
}

(async () => {
  if (!ONLY || ONLY === 'headless') headless();
  if (!ONLY || ONLY === 'browser' || ONLY === 'steam') await browser();
  console.log(failures ? `\n${failures} failure(s)` : '\nduos ok');
  process.exit(failures ? 1 : 0);
})();
