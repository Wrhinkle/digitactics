// Set engine check.
// Headless (always): every registered set (hidden ones too) plays a solo game to round 2 through the lockstep log,
// replays identically, keeps the pool whole and never touches Math.random / Date.now; Chaos picks the same set from a
// seed every time; lobby configs (lockstep init) carry the set to every client, and old configs without one play the
// default set.
// Browser (with a Playwright path): the same solo games through the real UI, the codex shows the active set, the
// Play page picker starts the chosen set (and Chaos), and a lobby created with a chosen set stores it and starts its
// game with it, against a local fake Firebase (tests/fake-firebase.cjs), also with database rules that refuse the
// lobby setId field.
// Run: node tests/sets.cjs [<path-to-playwright> <outDir> [port]] [--set <id>]
const path = require('path');
const { load, argSet, stripSet } = require('../tools/headless.cjs');

const ONLY = argSet(process.argv);
const argv = stripSet(process.argv);
const fails = [];
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails.push(what); };

// ---------------- headless ----------------
const DT = load({ strict: true });
const Lock = DT.Lock;
const ids = DT.Sets.ids().filter((id) => !ONLY || id === ONLY);
check(ids.length > 0, `registered sets: ${DT.Sets.ids().join(', ')}`);

function poolOk(g) {
  const counts = {};
  for (const L of DT.SHOP_LINES) counts[L.id] = g.pool[L.id];
  for (const p of g.players) {
    for (const u of p.allUnits()) if (DT.LINE[u.line].shop) counts[u.line] += DT.unitCopies(u);
    for (const id of p.shop) if (id) counts[id] += 1;
    for (const o of p.orbs) if (o.unit) counts[o.unit] += 1;
    if (g.carousel) for (const o of g.carousel.opts) if (!o.by && o.line === undefined) return false;
  }
  if (g.carousel) for (const o of g.carousel.opts) if (!o.by) counts[o.line] += 1;
  return DT.SHOP_LINES.every((L) => counts[L.id] === DT.POOL_SIZE[L.tier]);
}

// A solo game the way the UI runs it: one human in slot 0, every change a lockstep entry. The human buys, fields
// units and picks offers. Returns { game, log } once the game reaches stage 2 (or `rounds` more rounds).
function soloTo(setId, seed, rounds = 0, pace = undefined, stage = 2) {
  const init = { seed, setId, at: 0, pace };
  const g = Lock.create(init);
  const log = [];
  const push = (e) => { log.push(e); return Lock.apply(g, e); };
  const human = g.players[0];
  let guard = 0;
  const target = rounds ? null : stage;
  let done = 0;
  while (!g.over && guard++ < 400) {
    if (g.roundType === 'carousel' && g.phase === 'planning') {
      if (g.carousel && g.carouselTurn() === human) push({ k: 'a', s: 0, a: 'carousel', g: {} });
      push({ k: 'next', at: 0 });
    } else if (g.phase === 'planning') {
      if (human.offer) push({ k: 'a', s: 0, a: 'offer', g: { i: 0, kind: human.offer.kind } });
      for (let i = 0; i < human.shop.length; i++) if (human.shop[i] && g.canBuy(human, i) === 'ok') push({ k: 'a', s: 0, a: 'buy', g: { slot: i } });
      const b = human.bench.findIndex(Boolean);
      if (b >= 0) push({ k: 'a', s: 0, a: 'move', g: { from: { type: 'bench', i: b }, to: { type: 'board', c: (guard * 3) % 7, r: 4 + (guard % 4) } } });
      if (human.items.length && human.boardUnits().length) push({ k: 'a', s: 0, a: 'equip', g: { idx: 0, uid: human.boardUnits()[0].uid } });
      push({ k: 'start', at: 0 });
    } else if (g.phase === 'combat') push({ k: 'end', at: 0 });
    else if (g.phase === 'result') { push({ k: 'next', at: 0 }); done++; }
    if (!poolOk(g)) throw new Error(`${setId}: pool leak at ${g.roundLabel}`);
    if (target && g.stage >= target && g.phase === 'planning') break;
    if (rounds && done >= rounds) break;
  }
  return { game: g, log, init };
}

for (const id of ids) {
  let res = null;
  try {
    res = soloTo(id, 4242);
    const g = res.game;
    check(g.setId === id && DT.SET_ID === id, `${id}: solo game plays set ${g.setId}`);
    check(g.stage >= 2 || g.over, `${id}: reached round ${g.roundLabel}`);
    check(g.players.every((p) => p.shop.every((l) => !l || (DT.LINE[l].set === id && DT.LINE[l].shop))), `${id}: shops only hold the set's lines`);
    // a replay of the log on a fresh game ends identical (late joiner / desync reload)
    const again = Lock.create(res.init);
    for (const e of res.log) Lock.apply(again, e);
    check(Lock.digest(again) === Lock.digest(g), `${id}: log replay is identical`);
    // a few more rounds, with combat
    const more = soloTo(id, 777, 5);
    check(!!more.game.roundLabel, `${id}: 5 more rounds from 1-1, ended at ${more.game.roundLabel}`);
  } catch (e) {
    check(false, `${id}: ${e.stack || e}`);
  }
}

// interleaved games of two sets in one process each keep their own set
if (!ONLY && ids.length >= 2) {
  const a = Lock.create({ seed: 5, setId: ids[0], at: 0 });
  const b = Lock.create({ seed: 5, setId: ids[1], at: 0 });
  Lock.apply(a, { k: 'next', at: 0 });
  Lock.apply(b, { k: 'next', at: 0 });
  Lock.apply(a, { k: 'start', at: 0 });
  check(a.players[0].shop.every((l) => !l || DT.Sets.get(ids[0]).LINE[l]) && b.players[0].shop.every((l) => !l || DT.Sets.get(ids[1]).LINE[l]), 'interleaved games of two sets stay separate');
}

// ---------- Chaos ----------
{
  const pool = DT.Sets.chaosPool();
  const picks = Array.from({ length: 200 }, (_, i) => DT.Sets.resolve('chaos', i * 7919 + 13));
  check(picks.every((p) => pool.includes(p)), `chaos picks only eligible sets (${pool.join(', ')})`);
  check(picks.every((p, i) => DT.Sets.resolve('chaos', i * 7919 + 13) === p), 'chaos: same seed, same set (repeat)');
  const DT2 = load();
  check(picks.every((p, i) => DT2.Sets.resolve('chaos', i * 7919 + 13) === p), 'chaos: same seed, same set (fresh load)');
  // with a second eligible set, the seed spreads games over both
  const probe = load();
  const tdef = probe.Sets.def('_template');
  if (tdef) {
    probe.Sets.register({ ...tdef, id: 'chaos-probe', hidden: false, lines: tdef.lines.map((l) => ({ ...l, id: `cp_${l.id}`, ai: undefined })) });
    probe.Sets.extend('chaos-probe', {});
    const pp = probe.Sets.chaosPool();
    const spread = new Set(Array.from({ length: 200 }, (_, i) => probe.Sets.resolve('chaos', i * 31 + 1)));
    check(pp.length >= 2 && spread.size === pp.length, `chaos spreads over ${pp.length} eligible sets (${[...spread].join(', ')})`);
    const seed = 99123;
    const g = probe.Lock.create({ seed, setId: 'chaos', at: 0 });
    check(g.chaos && g.setId === probe.Sets.resolve('chaos', seed), `a 'chaos' game plays the seed's set (${g.setId})`);
  }
  check(DT.Sets.resolve('nope', 1) === DT.Sets.DEFAULT && DT.Sets.resolve(undefined, 1) === DT.Sets.DEFAULT, 'unknown or missing set ids fall back to the default set');
}

// ---------- lobby config through the lockstep harness ----------
for (const id of ids.slice(0, ONLY ? 1 : 3)) {
  const slots = Array.from({ length: 8 }, (_, i) => ({ name: i < 2 ? `P${i}` : DT.TAMERS[i % 7], human: i < 2 }));
  const init = { seed: 31337, slots, planTime: 30, at: 0, setId: id };
  const host = Lock.create(init);
  const client = Lock.create(JSON.parse(JSON.stringify(init))); // as read back from the database
  let now = 0;
  const log = [];
  for (let k = 0; k < 400 && !host.over && host.stage < 3; k++) {
    now += 500;
    const e = Lock.decide(host, { now, online: new Set([0, 1]), done: new Set([0, 1]) });
    if (!e) continue;
    e.at = now;
    log.push(e);
    Lock.apply(host, e);
    Lock.apply(client, e);
  }
  check(host.setId === id && client.setId === id, `${id}: lobby init carries the set to host and client`);
  check(Lock.digest(host) === Lock.digest(client), `${id}: host and client identical at ${host.roundLabel} (${log.length} entries)`);
}
{
  const old = Lock.create({ seed: 1, slots: null, at: 0 });
  check(old.setId === DT.Sets.DEFAULT, 'a lobby init without setId (older lobbies) plays the default set');
}

// ---------- pace: Hyper Roll and Normal ----------
{
  check(DT.Sets.nativePace('faction-wars') === 'hyper' && DT.Sets.nativePace('autochess') === 'normal', 'native pace: TFT sets hyper, Auto Chess normal');
  const fwH = DT.Sets.rulesFor('faction-wars', 'hyper'), fwN = DT.Sets.rulesFor('faction-wars', 'normal');
  check(fwH.startHp === 20 && !fwH.xp && fwH.levelByStage && !fwH.interest && fwH.rerollCost === 1, 'faction-wars hyper: 20 HP, levels by stage, no interest, 1-gold rerolls');
  check(fwN.startHp === 100 && fwN.xp && fwN.xp.buyCost === 4 && fwN.xp.buyXp === 4 && fwN.xp.perRound === 2 && fwN.interest && fwN.interest.max === 5 && fwN.streak && fwN.unitDamage && fwN.roundsInStage(2) === 7 && fwN.roundType(2, 4) === 'carousel',
    'faction-wars normal: 100 HP, 4 XP for 4 gold, interest, streaks, unit damage, carousel at x-4');
  const acH = DT.Sets.rulesFor('autochess', 'hyper'), acN = DT.Sets.rulesFor('autochess', 'normal');
  check(acN.startHp === 100 && acN.xp && acN.xp.buyCost === 5 && acN.roundsInStage(2) === 5, 'autochess normal: its own economy (native rules)');
  check(acH.startHp === 20 && !acH.xp && acH.levelByStage && acH.rerollCost === 1 && acH.roundsInStage(2) === 4 && acH.offers.some((o) => o.kind === 'ac_loot' && o.stage === 2 && o.round === 1),
    'autochess hyper: mode profile (20 HP, stage levels, 4-round stages) plus the set loot rounds');
  const dlN = DT.Sets.rulesFor('dragonlands', 'normal');
  check(dlN.offers.some((o) => o.kind === 'drg_treasure') && dlN.augmentTierOdds[0] === 0.25, 'dragonlands normal: rules.normal offers plus pace-independent keys (augment odds)');
  const chaosP = Array.from({ length: 100 }, (_, i) => DT.Sets.resolvePace('chaos', i * 7919 + 3));
  check(new Set(chaosP).size === 2 && chaosP.every((p, i) => load().Sets.resolvePace('chaos', i * 7919 + 3) === p), 'chaos pace: both paces, same seed same pace on a fresh load');
  const old = Lock.create({ seed: 9, setId: 'faction-wars', at: 0 });
  check(old.pace === 'hyper' && Lock.create({ seed: 9, setId: 'autochess', at: 0 }).pace === 'normal', 'an init without pace plays the set native pace');
  // extend() patches merge into the pace blocks
  const X = load();
  X.Sets.extend('dragonlands', { rules: { normal: { startHp: 77 } } });
  const xr = X.Sets.rulesFor('dragonlands', 'normal');
  check(xr.startHp === 77 && xr.offers.some((o) => o.kind === 'drg_treasure'), 'extend({ rules: { normal } }) merges into the set normal block');
  // hooks see the pace
  const gN = new DT.Game({ seed: 3, allAI: true, setId: 'faction-wars', pace: 'normal' });
  check(gN.pace === 'normal' && gN.ctx(gN.players[0]).pace === 'normal' && gN.combatInfo().pace === 'normal' && gN.combatInfo().timeLimit === 50 && gN.roundNo(2, 1) === 5,
    'game.pace, c.pace, c.info.pace, the combat timer and game.roundNo() follow the pace');
}
const PACE_IDS = ids;
for (const id of PACE_IDS) {
  for (const pace of DT.PACES) {
    try {
      const res = soloTo(id, 4343, 0, pace, 3);
      const g = res.game;
      check(g.pace === pace && DT.SET.pace === pace && (g.stage >= 3 || g.over), `${id} ${pace}: solo game reached ${g.roundLabel} (${g.R.startHp} HP, ${g.R.xp ? 'XP' : 'stage levels'})`);
      const again = Lock.create(res.init);
      for (const e of res.log) Lock.apply(again, e);
      check(Lock.digest(again) === Lock.digest(g), `${id} ${pace}: log replay is identical`);
      // lobby: host and client from the same init, to stage 3
      const slots = Array.from({ length: 8 }, (_, i) => ({ name: i < 2 ? `P${i}` : DT.TAMERS[i % 7], human: i < 2 }));
      const init = { seed: 2024, slots, planTime: 30, at: 0, setId: id, pace };
      const host = Lock.create(init);
      const client = Lock.create(JSON.parse(JSON.stringify(init)));
      let now = 0, n = 0;
      for (let k = 0; k < 800 && !host.over && host.stage < 3; k++) {
        now += 500;
        if (host.phase === 'planning' && host.R.xp && k % 5 === 0) { const e = { k: 'a', s: 1, a: 'buyXp', at: now }; Lock.apply(host, e); Lock.apply(client, e); n++; }
        const e = Lock.decide(host, { now, online: new Set([0, 1]), done: new Set([0, 1]) });
        if (!e) continue;
        e.at = now;
        n++;
        Lock.apply(host, e);
        Lock.apply(client, e);
      }
      check(client.pace === pace && Lock.digest(host) === Lock.digest(client) && (host.stage >= 3 || host.over), `${id} ${pace}: lockstep host and client identical at ${host.roundLabel} (${n} entries)`);
    } catch (e) {
      check(false, `${id} ${pace}: ${e.stack || e}`);
    }
  }
}

// ---------- feature probe: engine paths no shipped set uses yet ----------
// A throwaway set (in its own context) with a wild 1-1, Tamer powers that rank up, tiered item picks, a global item,
// a mechanic offer kind and action, a summon ability, Chosen-style shop slots (price + bonus trait + mods), a placement
// rule, a team-size hook, interest, streaks and unit damage. Full AI games and a lockstep replay must run clean.
if (!ONLY) {
  const P = load({ strict: true });
  const t = P.Sets.def('_template');
  const counts = { rank: 0, galaxy: 0, summons: 0, chosen: 0, global: 0, place: 0, action: 0 };
  P.Sets.register({
    ...t,
    id: 'probe',
    hidden: true,
    lines: t.lines.map((l) => ({ ...l, ai: undefined, type: l.id === 't_arc1' ? 'summon' : l.type })),
    items: {
      list: [
        { id: 'p_t1a', name: 'Tier One A', tier: 1, cat: 'tiered', stats: { ad: 0.2 }, tags: ['ad'] },
        { id: 'p_t1b', name: 'Tier One B', tier: 1, cat: 'tiered', stats: { ap: 20 }, tags: ['ap'] },
        { id: 'p_t1c', name: 'Tier One C', tier: 1, cat: 'tiered', stats: { armor: 25 }, tags: ['tank'] },
        { id: 'p_t1d', name: 'Tier One D', tier: 1, cat: 'tiered', stats: { hp: 200 }, tags: ['tank'] },
        { id: 'p_banner', name: 'Banner', tier: 2, cat: 'global', desc: 'Team +10% AS.', ai: { bonus: () => 20 }, hooks: { onCombatStart(c) { counts.global++; for (const u of c.mine) u.as *= 1.1; } } },
        { id: 'p_t2a', name: 'Tier Two A', tier: 2, cat: 'tiered', stats: { ad: 0.3 }, tags: ['ad'] },
        { id: 'p_t2b', name: 'Tier Two B', tier: 2, cat: 'tiered', stats: { ap: 30 }, tags: ['ap'] },
      ],
    },
    powers: [
      { id: 'p_war', tier: 2, name: 'Warlord', desc: 'Team +5 AD per rank.', hooks: { onCombatStart(c) { for (const u of c.mine) u.ad += 5 * c.rank; }, onRankUp() { counts.rank++; } } },
      { id: 'p_sage', tier: 2, name: 'Sage', desc: 'Team +10 AP per rank.', hooks: { onCombatStart(c) { for (const u of c.mine) u.ap += 10 * c.rank; }, onRankUp() { counts.rank++; } } },
    ],
    abilities: {
      summon: {
        base: 0,
        cast(c, u, tg) { const cell = c.freeCellNear(u, tg); if (cell && c.summon(u.team, 't_slime', 0, cell.c, cell.r)) counts.summons++; },
        text: () => 'Summons a slime.',
      },
    },
    rules: {
      ...t.rules,
      roundType: (stage, round) => (stage === 1 ? 'pve' : round === 4 ? 'pve' : stage === 3 && round === 1 ? 'carousel' : 'pvp'),
      offers: [{ kind: 'itemTier', tier: 1, stage: 1, round: 2 }, { kind: 'itemTier', tier: 2, stage: 2, round: 4 }, { kind: 'power', stage: 2, round: 2 }, { kind: 'galaxy', stage: 2, round: 1 }],
      armoryStages: [], augmentStages: [3],
      powerRanks: ['4-1', '5-1'],
    },
    mechanic: {
      id: 'probe-mech', name: 'Probe', desc: 'Test mechanic.',
      hooks: {
        onGameStart(c) { c.game.mech.galaxy = c.pick(['big', 'small']); counts.galaxy++; },
        onShopRolled(c, e) { if (c.stage >= 2 && e.shop[0]) { c.player.shopMeta = e.shop.map(() => null); c.player.shopMeta[0] = { cost: 3 * P.LINE[e.shop[0]].tier, unit: { bonusTraits: { [P.LINE[e.shop[0]].traits[0]]: 1 }, mods: { chosen: true } } }; } },
        onBuy(c, e) { if (e.unit.mods && e.unit.mods.chosen) counts.chosen++; },
        canPlace(c, e) { if (e.c === 0 && e.r === 4) { counts.place++; e.ok = false; } },
        onTeamSize(c, e) { if (c.game.mech.galaxy === 'big') e.size += 1; },
        onCombatStart(c) { if (c.info.mech.galaxy === 'big') for (const u of c.units) if (u.mods && u.mods.chosen) u.ad *= 1.1; },
        onAiTurn(c) { if (c.rng() < 0.2) c.game.applyAction(c.player, 'pray', { n: 1 }); },
      },
      actions: { pray: (c, args) => { counts.action++; c.player.mech.prayers = (c.player.mech.prayers || 0) + (args.n === 1 ? 1 : 0); return true; } },
      offerKinds: {
        galaxy: { name: 'Blessing', options: (c) => ['gold', 'hp', 'item'], choose: (c, id) => { if (id === 'gold') c.player.gold += 3; else if (id === 'hp') c.player.hp += 5; else c.give(c.pick(['p_t1a', 'p_t1b'])); }, aiScore: (c, id) => (id === 'gold' ? 1 : 0) },
      },
    },
  });
  P.Sets.extend('probe', {});
  try {
    for (let s = 1; s <= 3; s++) {
      const g = new P.Game({ seed: s * 101, allAI: true, setId: 'probe' });
      let guard = 0;
      while (!g.over && guard++ < 200) {
        g.startCombat();
        g.finishCombat();
        for (const p of g.players) if (p.alive && p.boardCount() > p.cap()) throw new Error(`${p.name} over cap`);
        g.nextRound();
      }
      if (!g.over) throw new Error(`probe game ${s} did not end`);
    }
    check(counts.galaxy === 3 && counts.rank > 0 && counts.summons > 0 && counts.chosen > 0 && counts.global > 0 && counts.action > 0,
      `probe set: 3 AI games with ranks ${counts.rank}, summons ${counts.summons}, chosen buys ${counts.chosen}, global item combats ${counts.global}, actions ${counts.action}`);
    const slots = Array.from({ length: 8 }, (_, i) => ({ name: `P${i}`, human: i === 0 }));
    const init = { seed: 555, slots, at: 0, setId: 'probe' };
    const a = P.Lock.create(init);
    const b = P.Lock.create(init);
    const log = [];
    for (let now = 0, k = 0; k < 600 && !a.over && a.stage < 4; k++) {
      now += 1000;
      if (a.phase === 'planning' && a.players[0].offer) { const e = { k: 'a', s: 0, a: 'offer', g: { i: 0 }, at: now }; log.push(e); P.Lock.apply(a, e); P.Lock.apply(b, e); }
      if (a.phase === 'planning' && k % 3 === 0) { const e = { k: 'a', s: 0, a: 'pray', g: { n: 1 }, at: now }; log.push(e); P.Lock.apply(a, e); P.Lock.apply(b, e); }
      const e = P.Lock.decide(a, { now, online: new Set([0]), done: new Set([0]) });
      if (!e) continue;
      e.at = now;
      log.push(e);
      P.Lock.apply(a, e);
      P.Lock.apply(b, e);
    }
    const late = P.Lock.create(init);
    for (const e of log) P.Lock.apply(late, e);
    check(P.Lock.digest(a) === P.Lock.digest(b) && P.Lock.digest(a) === P.Lock.digest(late) && a.players[0].mech.prayers > 0,
      `probe set: lockstep host, client and late joiner identical at ${a.roundLabel} (power ${a.players[0].power && a.players[0].power.id})`);
  } catch (e) {
    check(false, `probe set: ${e.stack || e}`);
  }
}

// ---------- set hooks and fields used by the shipped sets (FORMAT.md) ----------
// canEquip, noSell / noItems, sellValue, multi-pair recipes, emblem id prefixes, unitNote, mechanic.intro and the
// combat move helpers, checked on the sets that use them.
if (!ONLY) {
  const Q = load({ strict: true });
  try {
    // Underlords: one item per hero (canEquip) and a Tamer that cannot be sold or hold items
    const g = new Q.Game({ seed: 9, setId: 'underlords', allAI: true });
    const p = g.players[0];
    const hero = Q.SHOP_LINES.find((l) => !l.tamer);
    const u = g.newUnit(hero.id, 0);
    p.bench[0] = u;
    const items = Object.keys(Q.ITEMS).filter((id) => Q.ITEMS[id].cat === 'tiered').slice(0, 2);
    p.items = items.slice();
    const r1 = g.equipItem(p, 0, u.uid), r2 = g.equipItem(p, 0, u.uid);
    check(r1 === items[0] && r2 === 'full' && u.items.length === 1, `underlords: one item per hero (${r1}, ${r2})`);
    const tl = Q.LINES.find((l) => l.tamer);
    const t = g.newUnit(tl.id, 0);
    p.bench[1] = t;
    check(g.equipItem(p, 0, t.uid) === 'cantHold' && !Q.canSell(t) && g.sell(p, t.uid) === false && p.bench[1] === t, 'underlords: the Tamer cannot hold items or be sold');
    // Dragonlands: the sell value shown is the gold paid (sellValue hook)
    Q.Sets.activate('dragonlands');
    const d = Q.SHOP_LINES.find((l) => l.slots === 2);
    const gd = new Q.Game({ seed: 3, setId: 'dragonlands', allAI: true });
    const pd = gd.players[0];
    const du = gd.newUnit(d.id, 0);
    pd.bench[0] = du;
    const before = pd.gold, shown = Q.sellValue(du);
    gd.sell(pd, du.uid);
    check(shown === d.price && pd.gold - before === shown, `dragonlands: a Dragon sells for its price (${shown} shown, ${pd.gold - before} paid)`);
    check(!!Q.emblemOf('MAGE') && Q.emblemOf('MAGE').startsWith('drg_emblem_'), `dragonlands: prefixed emblem ids (${Q.emblemOf('MAGE')})`);
    // Reckoning: Shadow items have three recipes
    Q.Sets.activate('reckoning');
    const sh = Object.values(Q.ITEMS).find((it) => it.cat === 'shadow' && it.from[0] !== it.from[1]);
    check(Q.recipesOf(sh.id).length === 3 && Q.recipesOf(sh.id).every(([a, b]) => Q.combineItems(a, b) === sh.id), `reckoning: ${sh.name} has 3 recipes`);
    // emblem ids never repeat across sets with different trait meanings
    const seen = {};
    let clash = [];
    for (const id of Q.Sets.ids()) {
      const S = Q.Sets.activate(id);
      for (const k of S.EMBLEM_IDS) { if (seen[k] && seen[k] !== id && !(seen[k].startsWith('underlords') && id.startsWith('underlords'))) clash.push(`${k} (${seen[k]}, ${id})`); seen[k] = seen[k] || id; }
    }
    check(!clash.length, `emblem ids are unique across sets${clash.length ? ': ' + clash.join(', ') : ''}`);
    // unit notes, the Galaxy intro and the AI value
    Q.Sets.activate('fates');
    const fl = Q.SHOP_LINES[0];
    check(Q.unitNote({ line: fl.id, stage: 1, mods: { partner: true, trait: fl.traits[0], bonus: 'ap' } }).some((x) => /Partner/.test(x)), 'fates: Partner unit note');
    const gg = new Q.Game({ seed: 4, setId: 'galaxies', allAI: true });
    const intro = Q.mechanicIntro(gg);
    check(!!(intro && intro.name) && !gg.players.some((x) => x.offer && x.offer.kind === 'galaxy'), `galaxies: the Galaxy is announced at game start (${intro && intro.name})`);
    // combat helpers: move refuses a taken cell, lineTargets finds the line
    Q.Sets.activate('faction-wars');
    const A = Q.SHOP_LINES[0].id, B = Q.SHOP_LINES[1].id;
    const cb = new Q.Combat({ units: [{ line: A, stage: 0, c: 3, r: 4, items: [] }, { line: B, stage: 0, c: 4, r: 4, items: [] }] }, { units: [{ line: A, stage: 0, c: 3, r: 4, items: [] }] }, { seed: 1, record: false });
    const api = cb.api;
    const [a0, a1] = cb.units.filter((x) => x.team === 0);
    const e0 = cb.units.find((x) => x.team === 1);
    check(api.move(a0, a1.c, a1.r) === false && api.move(a0, 2, 5, 0.2) === true && a0.c === 2 && a0.r === 5 && api.lineTargets(a1, e0)[0] === e0, 'combat: move and lineTargets helpers');
  } catch (e) {
    check(false, `set hooks: ${e.stack || e}`);
  }
}

// ---------------- browser ----------------
async function browserPart() {
  const { spawn } = require('child_process');
  const { chromium } = require(argv[2]);
  const { installFakeFirebase } = require('./fake-firebase.cjs');
  const out = argv[3] || __dirname;
  const PORT = Number(argv[4] || process.env.PORT || 8164);
  const URL = `http://localhost:${PORT}/`;
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true, stdio: 'ignore' });
  await new Promise((r) => setTimeout(r, 600));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); } catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const errors = [];
  const newPage = async (fake) => {
    const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
    page.on('pageerror', (e) => errors.push(String(e.stack || e)));
    page.on('console', (m) => { if (m.type() === 'error') errors.push(m.text()); });
    if (fake) await installFakeFirebase(page, fake);
    await page.goto(URL);
    await page.waitForFunction(() => window.DT && window.DT.UI && window.DT.SetPicker, null, { timeout: 20000 });
    await page.waitForTimeout(800);
    return page;
  };
  // advance the solo game with the same entries the UI commits
  const toStage2 = (page) => page.evaluate(() => {
    const ui = window.__digi;
    const g = ui.game;
    for (let i = 0; i < 40 && !g.over && !(g.stage >= 2 && g.phase === 'planning'); i++) {
      if (g.roundType === 'carousel' && g.phase === 'planning') { if (g.carousel && g.carouselTurn() === g.human) DT.Lock.apply(g, { k: 'a', s: g.human.id, a: 'carousel', g: {}, at: 0 }); DT.Lock.apply(g, { k: 'next', at: 0 }); }
      else if (g.phase === 'planning') DT.Lock.apply(g, { k: 'start', at: 0 });
      else if (g.phase === 'combat') DT.Lock.apply(g, { k: 'end', at: 0 });
      else DT.Lock.apply(g, { k: 'next', at: 0 });
    }
    DT.UI.refresh();
    return { set: g.setId, active: DT.SET_ID, round: g.roundLabel, shop: g.human.shop.filter(Boolean).every((l) => DT.LINE[l].set === g.setId), cards: document.querySelectorAll('#shop .card[data-slot]').length };
  });
  try {
    // ---- solo, every set, through the UI ----
    const page = await newPage(null);
    for (const id of ids) {
      await page.evaluate((s) => DT.UI.newGame(s), id);
      await page.waitForTimeout(400);
      const r = await toStage2(page);
      check(r.set === id && r.active === id && r.shop && r.cards > 0, `browser ${id}: solo game reached ${r.round} with the set's shop`);
      await page.waitForTimeout(700);
      await page.screenshot({ path: path.join(out, `sets-${id}.png`) });
      const cx = await page.evaluate(() => {
        DT.Codex.openFull('traits');
        const firstTrait = Object.values(DT.TRAITS)[0].name;
        const txt = document.getElementById('cxFull').innerText;
        DT.Codex.close();
        return { ok: txt.includes(firstTrait), firstTrait };
      });
      check(cx.ok, `browser ${id}: codex shows the set's traits (${cx.firstTrait})`);
    }
    // ---- the Play page picker ----
    const pick = await page.evaluate(async () => {
      DT.UI.showTitle();
      DT.Sets.def('_template').hidden = false; // so the picker has two sets and Chaos
      DT.Client.go('play');
      DT.SetPicker.render();
      await new Promise((r) => setTimeout(r, 200));
      const chips = [...document.querySelectorAll('#setPickSolo [data-set]')].map((b) => b.dataset.set);
      document.querySelector('#setPickSolo [data-set="_template"]').click();
      document.getElementById('playSoloBtn').click();
      await new Promise((r) => setTimeout(r, 400));
      const a = window.__digi.game.setId;
      DT.UI.showTitle();
      DT.Client.go('play');
      document.querySelector('#setPickSolo [data-set="chaos"]').click();
      document.getElementById('playSoloBtn').click();
      await new Promise((r) => setTimeout(r, 400));
      const g = window.__digi.game;
      const c = { set: g.setId, chaos: g.chaos, expect: DT.Sets.resolve('chaos', g.seed) };
      DT.SetPicker.select('faction-wars');
      return { chips, a, c };
    });
    check(['faction-wars', '_template', 'chaos'].every((x) => pick.chips.includes(x)), `picker lists ${pick.chips.join(', ')}`);
    check(pick.a === '_template', 'Play with the Template chip starts a Template game');
    check(pick.c.chaos && pick.c.set === pick.c.expect, `Play with Chaos starts the seed's set (${pick.c.set})`);
    // ---- the pace toggle: mode select and party, remembered, starts the chosen pace; the HUD follows it ----
    const pp = await page.evaluate(async () => {
      const wait = (ms) => new Promise((r) => setTimeout(r, ms));
      DT.UI.showTitle();
      DT.Client.go('play');
      DT.SetPicker.render();
      await wait(200);
      const onSelect = document.querySelectorAll('#setPickSolo [data-pace]').length;
      document.querySelector('#setPickSolo [data-pace="normal"]').click();
      await wait(100);
      const onParty = document.querySelectorAll('#pacePickParty [data-pace]').length;
      const remembered = localStorage.getItem('dt.pace.v1');
      document.getElementById('playSoloBtn').click();
      await wait(500);
      const g = window.__digi.game;
      const xpBar = document.getElementById('xpBar');
      const econ = document.getElementById('econLine');
      const hudN = { pace: g.pace, hp: g.human.hp, xpBar: !!xpBar && !xpBar.classList.contains('hidden'), econ: !!econ && !econ.classList.contains('hidden') && /Interest/.test(econ.textContent), planMs: g.planMs };
      DT.Codex.openFull('rules');
      const rulesN = document.getElementById('cxFull').innerText.toLowerCase().includes('pace: normal');
      DT.Codex.close();
      DT.UI.showTitle();
      DT.Client.go('play');
      document.querySelector('#pacePickParty [data-pace="hyper"]').click();
      document.getElementById('playSoloBtn').click();
      await wait(500);
      const h = window.__digi.game;
      const xb = document.getElementById('xpBar');
      const hudH = { pace: h.pace, hp: h.human.hp, xpBar: !!xb && !xb.classList.contains('hidden') };
      DT.Codex.openFull('rules');
      const rulesH = document.getElementById('cxFull').innerText.toLowerCase().includes('pace: hyper roll');
      DT.Codex.close();
      return { onSelect, onParty, remembered, hudN, hudH, rulesN, rulesH };
    });
    check(pp.onSelect === 2 && pp.onParty === 2 && pp.remembered === 'normal', 'pace toggle on the mode select and the party screen; the choice is remembered');
    check(pp.hudN.pace === 'normal' && pp.hudN.hp === 100 && pp.hudN.xpBar && pp.hudN.econ && pp.hudN.planMs === 40000 && pp.rulesN, `Normal solo game: 100 HP, XP bar, interest/streak line, 40 s timer, codex Rules says Normal (${JSON.stringify(pp.hudN)})`);
    check(pp.hudH.pace === 'hyper' && pp.hudH.hp === 20 && !pp.hudH.xpBar && pp.rulesH, `Hyper Roll solo game: 20 HP, no XP bar, codex Rules says Hyper Roll (${JSON.stringify(pp.hudH)})`);
    await page.evaluate(() => { DT.UI.showTitle(); DT.Client.go('play'); });
    await page.waitForTimeout(300);
    await page.screenshot({ path: path.join(out, 'sets-picker.png') });
    await page.close();

    // ---- lobbies (local fake Firebase) ----
    for (const [label, fake, choice] of [['lobby', {}, '_template'], ['lobby, rules refuse setId', { rejectLobbySetId: true }, '_template'], ['lobby, Chaos', {}, 'chaos']]) {
      const p = await newPage(fake);
      await p.evaluate(() => { DT.Sets.def('_template').hidden = false; });
      await p.click('#mpBtn');
      await p.waitForSelector('#mpName', { timeout: 15000 });
      await p.fill('#mpName', 'Host');
      await p.fill('#mpLobbyName', 'set test');
      await p.selectOption('#mpSet', choice);
      const paceChoice = label === 'lobby' ? 'normal' : label === 'lobby, Chaos' ? 'chaos' : 'hyper';
      await p.selectOption('#mpPace', paceChoice);
      await p.click('#mpCreate');
      await p.waitForSelector('#mpLink', { timeout: 15000 });
      await p.waitForTimeout(400);
      if (label === 'lobby') await p.screenshot({ path: path.join(out, 'sets-lobby.png') });
      const rec = await p.evaluate(() => {
        const lobbies = window.__fakeDb.getAt('tft/lobbies') || {};
        const [id, l] = Object.entries(lobbies)[0];
        const pres = window.__fakeDb.getAt(`tft/presence/${id}`) || {};
        return { id, setId: l.setId || null, presence: Object.values(pres).map((x) => x.lobbySet || null), shown: DT.Net.lobbySet() };
      });
      if (fake.rejectLobbySetId) check(!rec.setId && rec.presence.includes(choice) && rec.shown === choice, `${label}: lobby record has no setId, the host presence carries ${choice}`);
      else check(rec.setId === choice && rec.shown === choice, `${label}: lobby record stores setId ${rec.setId}`);
      await p.click('#mpStart');
      await p.waitForFunction(() => window.__digi && window.__digi.session && window.__digi.game, null, { timeout: 15000 });
      const started = await p.evaluate((lid) => {
        const init = window.__fakeDb.getAt(`tft/games/${lid}/init`);
        const g = window.__digi.game;
        const l = window.__fakeDb.getAt(`tft/lobbies/${lid}`) || {};
        return { init: init.setId, chaos: !!init.chaos, expect: DT.Sets.resolve(init.chaos ? 'chaos' : init.setId, init.seed), game: g.setId, active: DT.SET_ID,
          lobbyPace: l.pace || null, initPace: init.pace, chaosPace: !!init.chaosPace, gamePace: g.pace, expectPace: DT.Sets.resolvePace(init.chaosPace ? 'chaos' : init.pace, init.seed) };
      }, rec.id);
      const want = choice === 'chaos' ? started.expect : choice;
      check(started.init === want && started.game === want && started.active === want && started.chaos === (choice === 'chaos'), `${label}: init.setId ${started.init}, the game plays it`);
      const wantPace = paceChoice === 'chaos' ? started.expectPace : paceChoice;
      check(started.lobbyPace === paceChoice && started.initPace === wantPace && started.gamePace === wantPace && started.chaosPace === (paceChoice === 'chaos'),
        `${label}: lobby pace ${started.lobbyPace}, init.pace ${started.initPace}, the game plays it`);
      await p.close();
    }
  } finally {
    const real = errors.filter((e) => !/favicon|ERR_FILE_NOT_FOUND/.test(e));
    check(!real.length, `browser: no page errors${real.length ? `: ${real.slice(0, 5).join(' | ')}` : ''}`);
    await browser.close();
    server.kill();
  }
}

(async () => {
  if (argv[2]) await browserPart();
  console.log(fails.length ? `\n${fails.length} failed` : '\nsets ok');
  process.exit(fails.length ? 1 : 0);
})();
