// Interactive review of the models the native model sweep flagged (t-pose, bind-pose, motionless, height jumps).
// The page renders each model through the game's own pipeline: the index.html rule/data scripts, then
// js/render3d.js (model library, prepareTemplate, UnitView) and js/model-motion.js as ES modules, the same
// sources tools/build.cjs packs into js/bundle.js. Verdicts are appended to parity/model-verdicts.json.
//
//   node tools/parity/model-viewer.cjs [--port 8765] [--sweep <dir with sweep.json>] [--open] [--background]
//
// --open opens the page in the default browser. --background restarts this server as a detached, windowless
// process (log: parity/model-viewer.log) and exits.
const http = require('http');
const fs = require('fs');
const path = require('path');
const { spawn, execFileSync } = require('child_process');

const ROOT = path.join(__dirname, '..', '..');
const PAGE_DIR = path.join(__dirname, 'model-viewer');
const VERDICTS = path.join(ROOT, 'parity', 'model-verdicts.json');

function arg(name, dflt) {
  const a = process.argv;
  for (let i = 2; i < a.length; i++) {
    if (a[i] === `--${name}`) return a[i + 1];
    if (a[i].startsWith(`--${name}=`)) return a[i].slice(name.length + 3);
  }
  return dflt;
}
const flag = (name) => process.argv.includes(`--${name}`);
const PORT = Number(arg('port', 8765));
const URL_BASE = `http://127.0.0.1:${PORT}/`;

function openBrowser(url) {
  // cmd's start hands the URL to the default browser; the cmd process itself gets no window
  spawn('cmd.exe', ['/d', '/s', '/c', `start "" "${url}"`], { windowsVerbatimArguments: true, windowsHide: true, detached: true, stdio: 'ignore' }).unref();
}

if (flag('background')) {
  fs.mkdirSync(path.join(ROOT, 'parity'), { recursive: true });
  const log = fs.openSync(path.join(ROOT, 'parity', 'model-viewer.log'), 'a');
  const args = process.argv.slice(1).filter((a) => a !== '--background');
  // detached: no console is attached (DETACHED_PROCESS), windowsHide: SW_HIDE
  const child = spawn(process.execPath, args, { cwd: ROOT, detached: true, windowsHide: true, stdio: ['ignore', log, log] });
  child.unref();
  console.log(`model viewer started in the background (pid ${child.pid}) at ${URL_BASE}`);
  process.exit(0);
}

// The sweep lives in the git-ignored parity/ folder, usually only in the main checkout.
function sweepDir() {
  const given = arg('sweep', null);
  if (given) return path.resolve(given);
  const roots = [ROOT];
  try {
    const common = execFileSync('git', ['rev-parse', '--path-format=absolute', '--git-common-dir'], { cwd: ROOT, windowsHide: true, encoding: 'utf8' }).trim();
    roots.push(path.dirname(common));
  } catch { /* not a git checkout */ }
  for (const r of roots) for (const d of ['parity/final/model-sweep', 'parity/model-sweep']) {
    if (fs.existsSync(path.join(r, d, 'sweep.json'))) return path.join(r, d);
  }
  return null;
}
const SWEEP = sweepDir();

// The classic scripts index.html loads before js/bundle.js, in order: rules, sets, models.js, cosmetics.js.
function gameScripts() {
  const html = fs.readFileSync(path.join(ROOT, 'index.html'), 'utf8');
  return [...html.matchAll(/<script\s+src="([^"?]+)(?:\?[^"]*)?"/g)].map((m) => m[1]).filter((f) => f !== 'js/bundle.js');
}

const TYPES = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css', '.json': 'application/json',
  '.glb': 'model/gltf-binary', '.gltf': 'model/gltf+json', '.bin': 'application/octet-stream', '.png': 'image/png', '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg', '.webp': 'image/webp', '.svg': 'image/svg+xml', '.ico': 'image/x-icon', '.ktx2': 'image/ktx2' };

function sendFile(res, base, rel) {
  const file = path.join(base, rel);
  if (!file.startsWith(base + path.sep) && file !== base) { res.writeHead(403); return res.end(); }
  fs.readFile(file, (err, data) => {
    if (err) { res.writeHead(404); return res.end('not found'); }
    res.writeHead(200, { 'Content-Type': TYPES[path.extname(file).toLowerCase()] || 'application/octet-stream', 'Cache-Control': 'no-cache' });
    res.end(data);
  });
}
const json = (res, code, obj) => { res.writeHead(code, { 'Content-Type': 'application/json', 'Cache-Control': 'no-cache' }); res.end(JSON.stringify(obj)); };

function readVerdicts() {
  try { return JSON.parse(fs.readFileSync(VERDICTS, 'utf8')); } catch { return { entries: [] }; }
}
const VERDICT_KINDS = new Set(['broken', 'fine', 'partial', 'clear']);

const server = http.createServer((req, res) => {
  const url = decodeURIComponent(req.url.split('?')[0]);
  if (req.method === 'POST' && url === '/api/verdict') {
    let body = '';
    req.on('data', (d) => { body += d; if (body.length > 1e6) req.destroy(); });
    req.on('end', () => {
      let v;
      try { v = JSON.parse(body); } catch { return json(res, 400, { error: 'bad json' }); }
      if (!v || typeof v.source !== 'string' || !VERDICT_KINDS.has(v.verdict)) return json(res, 400, { error: 'need source and verdict (broken|fine|partial|clear)' });
      const entry = { source: v.source, index: Number.isInteger(v.index) ? v.index : null, verdict: v.verdict, notes: String(v.notes || '').slice(0, 4000),
        flags: Array.isArray(v.flags) ? v.flags.map(String) : [], at: new Date().toISOString() };
      const all = readVerdicts();
      all.entries.push(entry);
      fs.mkdirSync(path.dirname(VERDICTS), { recursive: true });
      const tmp = `${VERDICTS}.tmp`;
      fs.writeFileSync(tmp, JSON.stringify(all, null, 2));
      fs.renameSync(tmp, VERDICTS);
      json(res, 200, { ok: true, entry, count: all.entries.length });
    });
    return;
  }
  if (url === '/api/verdicts') return json(res, 200, readVerdicts());
  if (url === '/api/sweep') {
    if (!SWEEP) return json(res, 404, { error: 'sweep.json not found; pass --sweep <dir>' });
    return sendFile(res, SWEEP, 'sweep.json');
  }
  if (url === '/' || url === '/index.html' || url === '/model-viewer.html') {
    // served from the repo root so render3d.js resolves models/index.json and models/files/* as in the game
    const page = fs.readFileSync(path.join(PAGE_DIR, 'index.html'), 'utf8')
      .replace('<!--GAME_SCRIPTS-->', gameScripts().map((s) => `<script src="${s}"></script>`).join('\n'));
    res.writeHead(200, { 'Content-Type': TYPES['.html'], 'Cache-Control': 'no-cache' });
    return res.end(page);
  }
  if (url.startsWith('/mv/')) return sendFile(res, PAGE_DIR, url.slice(4));
  if (url.startsWith('/sweep/')) return SWEEP ? sendFile(res, SWEEP, url.slice(7)) : (res.writeHead(404), res.end());
  return sendFile(res, ROOT, url.slice(1));
});
server.listen(PORT, '127.0.0.1', () => {
  console.log(`${new Date().toISOString()} model viewer at ${URL_BASE} (root ${ROOT}, sweep ${SWEEP || 'not found'})`);
  if (flag('open')) openBrowser(URL_BASE);
});
