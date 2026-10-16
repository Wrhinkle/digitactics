// Loads the rules engine (data, items, sets, combat, game, lockstep) into a Node vm context, in the order index.html
// lists them, so tests and tools always see the same files and every registered set.
//   const { load, argSet } = require('../tools/headless.cjs');
//   const DT = load({ strict: true });   // strict: Math.random and Date.now throw while a game runs (determinism check)
//   const setId = argSet(process.argv);  // value of --set <id> (or --set=<id>), else null
//   const pace = argPace(process.argv);  // value of --pace hyper|normal|chaos (or --pace=...), else null (native pace)
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.join(__dirname, '..');
// rules scripts: everything index.html loads before models.js except the client-only files
const RULES = /^js\/(data|items|sets|combat|duos|game|lockstep)\.js$|^js\/sets\//;

function ruleFiles() {
  const html = fs.readFileSync(path.join(ROOT, 'index.html'), 'utf8');
  const files = [...html.matchAll(/<script\s+src="([^"?]+)(?:\?[^"]*)?"/g)].map((m) => m[1]).filter((f) => RULES.test(f));
  return files.filter((f) => fs.existsSync(path.join(ROOT, f)));
}

function load(opts = {}) {
  const strictMath = Object.create(Math);
  const ctx = { console, Math: opts.strict ? strictMath : Math };
  if (opts.strict) {
    ctx.Math.random = () => { throw new Error('Math.random used in game rules (use the seeded rng: c.rng() / game.rng())'); };
    const RealDate = Date;
    ctx.Date = class extends RealDate { static now() { throw new Error('Date.now used in game rules'); } };
  }
  ctx.globalThis = ctx;
  vm.createContext(ctx);
  for (const f of ruleFiles()) vm.runInContext(fs.readFileSync(path.join(ROOT, f), 'utf8'), ctx, { filename: f });
  return ctx.DT;
}

function argOpt(argv, name) {
  for (let i = 0; i < argv.length; i++) {
    if (argv[i] === `--${name}`) return argv[i + 1] || null;
    if (argv[i].startsWith(`--${name}=`)) return argv[i].slice(name.length + 3);
  }
  return null;
}
const argSet = (argv) => argOpt(argv, 'set');
const argPace = (argv) => argOpt(argv, 'pace');
// argv without --set <id> and --pace <p>, so positional arguments keep their places
function stripSet(argv) {
  const out = [];
  for (let i = 0; i < argv.length; i++) {
    if (argv[i] === '--set' || argv[i] === '--pace') { i++; continue; }
    if (argv[i].startsWith('--set=') || argv[i].startsWith('--pace=')) continue;
    out.push(argv[i]);
  }
  return out;
}

module.exports = { load, ruleFiles, argSet, argPace, stripSet, ROOT };
