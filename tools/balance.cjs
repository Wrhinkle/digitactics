// Balance report: plays many 8-AI games and prints top-${FORMAT === 'duos' ? 2 : 4} rate and average placement by trait and by line,
// plus wild-round win rates by stage. Each player's last board before elimination is what counts.
// Run: node tools/balance.cjs [games] [workers] [--set <id>] [--pace hyper|normal]   (default faction-wars, native pace;
// each set and pace reports on its own, with game length: combat rounds per game and the stage games end in)
// Duel mode: node tools/balance.cjs --duel [fights] [workers] [stage]. Random 7-unit boards with the same cost mix
// (1,2,2,3,3,4,5), each built around a random focus trait, fight each other with no items. Prints the win rate of
// boards that have each trait level active and of boards that field each line, so cost does not skew the numbers.
const { fork } = require('child_process');
const headless = require('./headless.cjs');

const SET_ARG = headless.argSet(process.argv);
const PACE_ARG = headless.argPace(process.argv);
const fi = process.argv.indexOf('--format');
const FORMAT = fi >= 0 && process.argv[fi+1] === 'duos' ? 'duos' : 'solo';
const ARGV = headless.stripSet(process.argv.filter((_,i)=>fi<0 || (i!==fi && i!==fi+1)));

function load(setId) {
  const DT = headless.load();
  const id = setId || SET_ARG || DT.Sets.DEFAULT;
  if (!DT.Sets.has(id)) { console.error(`unknown set ${id}; registered: ${DT.Sets.ids().join(', ')}`); process.exit(1); }
  DT.Sets.activate(id, PACE_ARG || undefined);
  return DT;
}

function play(seeds, setId) {
  const DT = load(setId);
  const out = { players: [], pve: {}, games: [] };
  for (const seed of seeds) {
    const g = new DT.Game({ seed, allAI: true, setId: DT.SET_ID, pace: PACE_ARG || undefined, format: FORMAT });
    let combats = 0, sends = 0;
    const last = new Map();
    let guard = 0;
    while (!g.over && guard++ < 200) {
      for (const p of g.alivePlayers()) {
        const units = p.boardUnits();
        const counts = g.duo ? g.teamSide(g.teamOf(p)).counts : DT.countTraits(units, p.augs);
        last.set(p.id, {
          traits: Object.keys(counts).filter((t) => DT.traitLevel(t, counts[t]) > 0).map((t) => `${t}${DT.traitLevel(t, counts[t])}`),
          lines: [...new Set(units.map((u) => u.line))],
          stage: g.stage,
        });
      }
      g.startCombat();
      const res = g.finishCombat();
      if (g.roundType !== 'carousel') combats++;
      if (g.isPvE) {
        const s = g.stage;
        out.pve[s] = out.pve[s] || [0, 0];
        for (const r of res) { out.pve[s][1]++; if (r.aWon) out.pve[s][0]++; }
      }
      sends += g.players.reduce((n,p)=>n+(p.portal||0),0);
      g.nextRound();
    }
    for (const p of g.players) out.players.push({ place: p.placement, ...last.get(p.id) });
    out.games.push({ combats, sends, end: g.roundLabel, stage: g.stage });
  }
  return out;
}

const MIX = [1, 2, 2, 3, 3, 4, 5];
function duel(seeds, stage, setId) {
  const DT = load(setId);
  const rng = DT.makeRng(seeds[0] * 31 + 7);
  const pick = (a) => a[Math.floor(rng() * a.length)];
  const fams = Object.keys(DT.TRAITS);
  const g = new DT.Game({ seed: 5, allAI: true, setId: DT.SET_ID, pace: PACE_ARG || undefined, format: FORMAT });
  const p = g.players[0];
  const top = g.maxTier;
  function board() {
    const focus = pick(fams);
    const want = 2 + Math.floor(rng() * 4);
    const used = new Set();
    let n = 0;
    for (const tier of MIX) {
      let lines = DT.SHOP_LINES.filter((l) => l.tier === tier && !used.has(l.id));
      if (!lines.length) lines = DT.SHOP_LINES.filter((l) => !used.has(l.id));
      const f = lines.filter((l) => l.traits.includes(focus));
      if (n < want && f.length) { lines = f; n++; }
      used.add(pick(lines).id);
    }
    p.board = {}; p.bench = new Array(DT.BENCH).fill(null); p.level = 7; p.teamBonus = 0;
    [...used].filter(Boolean).forEach((id, i) => { p.bench[i] = { uid: i + 1, line: id, stage: DT.LINE[id].tier === top ? Math.min(stage, 1) : stage, items: [] }; });
    g.aiArrange(p);
    return { units: p.boardSide() };
  }
  const rows = [];
  for (const seed of seeds) {
    const a = board(), b = board();
    const cb = DT.simulate(a, b, seed);
    for (const [side, team] of [[a, 0], [b, 1]]) {
      const counts = cb.traitCounts[team];
      rows.push({ win: cb.winner === team ? 1 : cb.winner === -1 ? 0.5 : 0,
        traits: Object.keys(counts).filter((t) => DT.traitLevel(t, counts[t]) > 0).map((t) => `${t} lv${DT.traitLevel(t, counts[t])}`),
        lines: side.units.map((u) => u.line) });
    }
  }
  return rows;
}

function report(rows, keyOf, title, min) {
  const m = new Map();
  for (const r of rows) for (const k of new Set(keyOf(r))) { const a = m.get(k) || [0, 0]; a[0] += r.win; a[1]++; m.set(k, a); }
  const list = [...m.entries()].filter(([, a]) => a[1] >= min).map(([k, [w, n]]) => [k, w / n, n]).sort((a, b) => b[1] - a[1]);
  console.log(`\n${title} (win rate, boards; min ${min})`);
  for (const [k, r, n] of list) console.log(`  ${k.padEnd(24)} ${(r * 100).toFixed(1).padStart(5)}%  n=${n}`);
}

if (process.argv[2] === '--worker') {
  process.on('message', (m) => { process.send(m.duel ? duel(m.seeds, m.stage, m.setId) : play(m.seeds, m.setId)); process.exit(0); });
} else if (ARGV[2] === '--duel') {
  const fights = Number(ARGV[3] || 2000);
  const workers = Math.max(1, Number(ARGV[4] || 4));
  const stage = Number(ARGV[5] || 1);
  const seeds = Array.from({ length: fights }, (_, i) => i * 7 + 1);
  const t0 = Date.now();
  const chunks = Array.from({ length: workers }, (_, w) => seeds.filter((_, i) => i % workers === w));
  Promise.all(chunks.map((c) => new Promise((res) => {
    const cp = fork(__filename, ['--worker', '--format', FORMAT, ...(PACE_ARG ? ['--pace', PACE_ARG] : [])], { windowsHide: true });
    cp.on('message', res);
    cp.send({ duel: true, seeds: c, stage, setId: SET_ARG });
  }))).then((parts) => {
    const DT = load();
    console.log(`Set: ${DT.SET.name} (${DT.SET_ID}, ${DT.SET.mode})`);
    const rows = parts.flat();
    const min = Math.max(20, Math.round(rows.length * 0.01));
    report(rows, (r) => r.traits.map((t) => t.replace(/ lv\d$/, '')).map((t) => DT.TRAITS[t].name), 'Trait active (any level)', min);
    report(rows, (r) => r.traits, 'Trait levels', min);
    report(rows, (r) => r.lines.map((l) => `${l} (${DT.LINE[l].tier})`), 'Lines', min);
    console.log(`\n${DT.SET_ID}: ${fights} fights at stage ${stage}, ${((Date.now() - t0) / 1000).toFixed(1)}s`);
  });
} else {
  const games = Number(ARGV[2] || 40);
  const workers = Math.max(1, Number(ARGV[3] || 4));
  const seeds = Array.from({ length: games }, (_, i) => (i + 1) * 104729);
  const t0 = Date.now();
  const chunks = Array.from({ length: workers }, (_, w) => seeds.filter((_, i) => i % workers === w)).filter((c) => c.length);
  Promise.all(chunks.map((c) => new Promise((res, rej) => {
    const cp = fork(__filename, ['--worker', '--format', FORMAT, ...(PACE_ARG ? ['--pace', PACE_ARG] : [])], { windowsHide: true });
    cp.on('message', res);
    cp.on('error', rej);
    cp.send({ seeds: c, setId: SET_ARG });
  }))).then((parts) => {
    const DT = load();
    console.log(`Set: ${DT.SET.name} (${DT.SET_ID}, ${DT.SET.mode}), pace ${DT.SET.pace}, format ${FORMAT}`);
    const players = parts.flatMap((p) => p.players);
    const gl = parts.flatMap((p) => p.games);
    const pve = {};
    for (const p of parts) for (const s in p.pve) { pve[s] = pve[s] || [0, 0]; pve[s][0] += p.pve[s][0]; pve[s][1] += p.pve[s][1]; }
    const agg = (keyOf) => {
      const m = new Map();
      for (const p of players) for (const k of keyOf(p)) {
        const a = m.get(k) || { n: 0, top4: 0, sum: 0, win: 0 };
        a.n++; a.sum += p.place; if (p.place <= (FORMAT === 'duos' ? 2 : 4)) a.top4++; if (p.place === 1) a.win++;
        m.set(k, a);
      }
      return [...m.entries()].map(([k, a]) => ({ k, n: a.n, top4: a.top4 / a.n, avg: a.sum / a.n, win: a.win / a.n })).sort((a, b) => a.avg - b.avg);
    };
    const show = (title, rows, min) => {
      console.log(`\n${title} (min ${min} boards; top-${FORMAT === 'duos' ? 2 : 4} rate, average place, win rate, boards)`);
      for (const r of rows.filter((x) => x.n >= min)) console.log(`  ${r.k.padEnd(22)} top${FORMAT === 'duos' ? 2 : 4} ${(r.top4 * 100).toFixed(0).padStart(3)}%  avg ${r.avg.toFixed(2)}  win ${(r.win * 100).toFixed(0).padStart(3)}%  n=${r.n}`);
    };
    const name = (t) => (DT.TRAITS[t] ? DT.TRAITS[t].name : t);
    const min = Math.max(5, Math.round(games * 0.1));
    show('Traits active on the final board (any level)', agg((p) => [...new Set(p.traits.map((t) => name(t.replace(/\d+$/, ''))))]), min);
    show('Trait levels', agg((p) => p.traits.map((t) => `${t.replace(/\d+$/, '')} lv${t.match(/\d+$/)[0]}`)), min);
    show('Lines on the final board', agg((p) => p.lines.map((l) => `${l} (${DT.LINE[l].tier})`)), min);
    console.log('\nWild rounds won by stage:', Object.entries(pve).map(([s, [w, n]]) => `${s}: ${(100 * w / n).toFixed(0)}% of ${n}`).join(', '));
    if (FORMAT === 'duos') console.log('Portal sends per game:', (gl.reduce((n,g)=>n+g.sends,0)/gl.length).toFixed(1));
    const ends = {};
    for (const x of gl) ends[x.stage] = (ends[x.stage] || 0) + 1;
    const cs = gl.map((x) => x.combats).sort((a, b) => a - b);
    console.log(`\nGame length (${DT.SET.pace}): ${(cs.reduce((a, b) => a + b, 0) / cs.length).toFixed(1)} combat rounds a game (min ${cs[0]}, median ${cs[cs.length >> 1]}, max ${cs[cs.length - 1]}); ends in stage ${Object.entries(ends).map(([s, n]) => `${s}: ${n}`).join(', ')}`);
    const tr = agg((p) => [...new Set(p.traits.map((t) => t.replace(/\d+$/, '')))]).filter((r) => r.n >= min);
    if (tr.length) console.log(`Trait spread: ${tr.length} traits on 10%+ of boards; top-${FORMAT === 'duos' ? 2 : 4} rate ${(Math.min(...tr.map((r) => r.top4)) * 100).toFixed(0)}%-${(Math.max(...tr.map((r) => r.top4)) * 100).toFixed(0)}%, avg place ${Math.min(...tr.map((r) => r.avg)).toFixed(2)}-${Math.max(...tr.map((r) => r.avg)).toFixed(2)}`);
    console.log(`\n${DT.SET_ID} ${DT.SET.pace}: ${games} games, ${players.length} placements, ${((Date.now() - t0) / 1000).toFixed(1)}s`);
  });
}
