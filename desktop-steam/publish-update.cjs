// Publishes a new version of the Steam desktop build as a GitHub release (repo from update-config.json, default
// Wrhinkle/digitactics). The in-game updater (updater.js) reads the latest release.
//
//   node desktop-steam/publish-update.cjs [--full] [--notes "what changed"] [options]
//
// Steps: rebuild js/bundle.js, bump desktop-steam/version.json, stage the payload (stage.cjs), zip it, sha256, write
// latest.json, then `gh release create v<version>` with payload-<version>.zip and latest.json.
// --full also rebuilds dist/DigiTactics-Steam/ with electron-builder and attaches it as DigiTactics-Steam-<version>.zip,
// the file friends download the first time (it updates itself from there). Use --full for the first release and
// whenever boot.js SHELL changes; plain releases are payload only and link the last full zip in their notes.
//
// Options:
//   --full              rebuild the full portable build and attach it as well (about 200 MB)
//   --notes "text"      release notes, shown in the game and on the release page
//   --bump patch|minor|major   which part to bump (default patch)   --version x.y.z  set it   --no-bump  keep it
//   --esbuild <path>    esbuild package for tools/build.cjs (default: DT_ESBUILD, then a sibling repo's node_modules)
//   --no-bundle         do not rebuild js/bundle.js
//   --proto-ok          rules files changed but older builds can still play with this one (see the checklist)
//   --dry-run           build and zip, create no release
//   --target <dir>      write the release files and release.json to a local folder instead of GitHub (tests; keeps
//                       version.json as it was)
//
// Checklist before publishing:
//   1. Rules or messages changed (js/game.js, combat.js, lockstep.js, data.js, items.js, sets.js, js/sets/*.js except
//      *-models.js, net.js, net-steam.js) in a way an older build cannot play with: bump PROTO_VERSION in
//      js/net-steam.js. Lobbies are filtered by dt_ver, so players on different versions never meet in a lobby.
//      The script stops if those files changed since the last publish and PROTO_VERSION did not, until you pass --proto-ok.
//   2. Changed boot.js, Electron, steamworks.js, koffi or the Steam DLLs: bump SHELL in boot.js and "minShell" in
//      version.json, and publish with --full. Older builds then show "needs a new full download" with the release link.
//   3. Builds never contain credentials. Anonymous updates require public releases. Private testers download
//      through their own GitHub access, or explicitly supply their own DT_UPDATE_TOKEN at runtime.
//   4. Run node tests/update.cjs and npm run smoke (desktop-steam) first.
const { spawnSync } = require('child_process');
const crypto = require('crypto');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { zipDir } = require('./ziplib');

const HERE = __dirname;
const ROOT = path.join(HERE, '..');
const args = process.argv.slice(2);
const flag = (n) => args.includes(n);
const opt = (n) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : undefined; };
const FULL = flag('--full');
const DRY = flag('--dry-run');
const TARGET = opt('--target');
const cfgFile = fs.existsSync(path.join(HERE, 'update-config.json')) ? 'update-config.json' : 'update-config.example.json';
const cfg = JSON.parse(fs.readFileSync(path.join(HERE, cfgFile), 'utf8'));
const REPO = `${cfg.owner}/${cfg.repo}`;
const STATE_FILE = path.join(HERE, 'publish-state.json');
const OUT = path.join(HERE, 'out', 'updates');
const mb = (n) => `${(n / 1048576).toFixed(1)} MB`;

function run(cmd, argv, opts = {}) {
  console.log(`> ${[path.basename(cmd), ...argv].join(' ')}`);
  const r = spawnSync(cmd, argv, { stdio: 'inherit', windowsHide: true, ...opts });
  if (r.status !== 0) throw new Error(`${path.basename(cmd)} failed (exit ${r.status}${r.error ? `, ${r.error.message}` : ''})`);
}
const sha256 = (file) => new Promise((res, rej) => {
  const h = crypto.createHash('sha256');
  fs.createReadStream(file).on('data', (d) => h.update(d)).on('error', rej).on('end', () => res(h.digest('hex')));
});
function cmp(a, b) {
  const pa = a.split('.').map(Number);
  const pb = b.split('.').map(Number);
  for (let i = 0; i < 3; i++) if ((pa[i] || 0) !== (pb[i] || 0)) return (pa[i] || 0) < (pb[i] || 0) ? -1 : 1;
  return 0;
}
function bumpVersion(v) {
  if (opt('--version')) return opt('--version');
  if (flag('--no-bump')) return v;
  const p = v.split('.').map((x) => parseInt(x, 10) || 0);
  while (p.length < 3) p.push(0);
  const part = opt('--bump') || 'patch';
  if (part === 'major') { p[0]++; p[1] = 0; p[2] = 0; } else if (part === 'minor') { p[1]++; p[2] = 0; } else p[2]++;
  return p.join('.');
}
function rulesSnapshot() {
  const files = ['js/game.js', 'js/combat.js', 'js/duos.js', 'js/lockstep.js', 'js/data.js', 'js/items.js', 'js/sets.js', 'js/net.js', 'js/net-steam.js', 'js/net-guard.js']
    .concat(fs.readdirSync(path.join(ROOT, 'js', 'sets')).filter((f) => f.endsWith('.js') && !f.endsWith('-models.js')).sort().map((f) => `js/sets/${f}`));
  const hashes = {};
  for (const f of files) if (fs.existsSync(path.join(ROOT, f))) hashes[f] = crypto.createHash('sha256').update(fs.readFileSync(path.join(ROOT, f))).digest('hex').slice(0, 16);
  const proto = (fs.readFileSync(path.join(ROOT, 'js', 'net-steam.js'), 'utf8').match(/PROTO_VERSION = '([^']+)'/) || [])[1] || '';
  return { proto, hashes };
}
function findEsbuild() {
  const given = opt('--esbuild') || process.env.DT_ESBUILD;
  if (given) return path.resolve(given);
  try { return path.dirname(require.resolve('esbuild/package.json', { paths: [ROOT, HERE] })); } catch (e) { /* not installed here */ }
  const repos = path.join(ROOT, '..');
  for (const d of fs.readdirSync(repos)) {
    const p = path.join(repos, d, 'node_modules', 'esbuild');
    if (fs.existsSync(path.join(p, 'package.json'))) return p;
  }
  throw new Error('esbuild not found: pass --esbuild <path-to-esbuild-package> or set DT_ESBUILD');
}

async function main() {
  const verFile = path.join(HERE, 'version.json');
  const verRaw = fs.readFileSync(verFile, 'utf8');
  const ver = JSON.parse(verRaw);
  const state = fs.existsSync(STATE_FILE) ? JSON.parse(fs.readFileSync(STATE_FILE, 'utf8')) : {};
  const next = bumpVersion(ver.version);
  if (!/^\d+\.\d+\.\d+$/.test(next)) throw new Error(`bad version ${next}`);
  if (state.version && !TARGET && cmp(next, state.version) <= 0) throw new Error(`version ${next} is not newer than the last release ${state.version}`);
  if (!TARGET && !DRY) {
    const r = spawnSync('gh', ['release', 'view', `v${next}`, '--repo', REPO], { windowsHide: true, stdio: 'ignore' });
    if (r.error) throw new Error(`gh not found: ${r.error.message}`);
    if (r.status === 0) throw new Error(`release v${next} already exists in ${REPO}`);
  }

  // checklist 1: rules changed without a protocol bump
  const snap = rulesSnapshot();
  if (state.rules && !TARGET) {
    const changed = Object.keys(snap.hashes).filter((f) => state.rules.hashes[f] !== snap.hashes[f]);
    if (changed.length && snap.proto === state.rules.proto && !flag('--proto-ok')) {
      console.error(`Rules files changed since ${state.version}: ${changed.join(', ')}`);
      console.error(`PROTO_VERSION is still '${snap.proto}'. If older builds cannot play with this one, bump PROTO_VERSION in js/net-steam.js.`);
      console.error('If older builds can still play with it (for example only comments or wording changed), rerun with --proto-ok.');
      process.exit(2);
    }
  }

  if (!flag('--no-bundle')) run(process.execPath, [path.join(ROOT, 'tools', 'build.cjs'), findEsbuild()]);
  fs.writeFileSync(verFile, JSON.stringify({ ...ver, version: next }, null, 2) + '\n');
  try {
    run(process.execPath, [path.join(HERE, 'stage.cjs')]);
    fs.mkdirSync(OUT, { recursive: true });
    for (const f of fs.readdirSync(OUT)) fs.rmSync(path.join(OUT, f), { recursive: true, force: true });

    let full = state.full || null;
    let fullZip = null;
    if (FULL) {
      run(process.execPath, [path.join(HERE, 'node_modules', 'electron-builder', 'cli.js'), '--win', 'dir', 'portable'], { cwd: HERE });
      run(process.execPath, [path.join(HERE, 'finish-build.cjs')]);
      fullZip = path.join(OUT, `DigiTactics-Steam-${next}.zip`);
      console.log('zipping the full build...');
      await zipDir(path.join(ROOT, 'dist', 'DigiTactics-Steam'), fullZip, { prefix: 'DigiTactics-Steam' });
      full = { version: next, file: path.basename(fullZip), size: fs.statSync(fullZip).size, sha256: await sha256(fullZip), date: new Date().toISOString() };
      console.log(`full build zip ${mb(full.size)}`);
    }

    const payloadName = `payload-${next}.zip`;
    const payloadZip = path.join(OUT, payloadName);
    const z = await zipDir(path.join(HERE, 'payload'), payloadZip);
    const size = fs.statSync(payloadZip).size;
    const notes = opt('--notes') || '';
    const latest = { version: next, minShell: ver.minShell || 1, file: payloadName, size, sha256: await sha256(payloadZip), notes, date: new Date().toISOString(), proto: snap.proto };
    const manifest = path.join(OUT, 'latest.json');
    fs.writeFileSync(manifest, JSON.stringify(latest, null, 2));
    console.log(`payload ${next}: ${z.files} files, zip ${mb(size)}, sha256 ${latest.sha256}`);

    const howTo = 'Download the DigiTactics-Steam zip, unzip it anywhere, keep Steam running and double-click Digi-Tactics.exe. No GitHub credentials are included. Automatic updates work with public releases; while this repo is private, download new builds using your own GitHub access. Updates apply on restart.';
    const body = [notes, '',
      fullZip ? `**Download:** ${path.basename(fullZip)} (${mb(full.size)}).` : full ? `**Download:** ${full.file} from release v${full.version}; it updates itself to ${next}.` : '',
      howTo, '', `payload-${next}.zip and latest.json are for the in-game updater.`].join('\n').trim();
    const files = [payloadZip, manifest].concat(fullZip ? [fullZip] : []);

    if (DRY) { console.log(`dry run: files are in ${OUT}, no release created`); fs.writeFileSync(verFile, verRaw); return; }
    if (TARGET) {
      fs.writeFileSync(verFile, verRaw); // test publishes do not move the real version number
      fs.mkdirSync(TARGET, { recursive: true });
      for (const f of files) fs.copyFileSync(f, path.join(TARGET, path.basename(f)));
      fs.writeFileSync(path.join(TARGET, 'release.json'), JSON.stringify({ tag_name: `v${next}`, name: `Digi-Tactics ${next}`, body, assets: files.map((f) => path.basename(f)) }, null, 2));
      console.log(`release v${next} written to ${TARGET}`);
      return;
    }
    const notesFile = path.join(os.tmpdir(), `dt-release-notes-${Date.now()}.md`);
    fs.writeFileSync(notesFile, body);
    try {
      run('gh', ['release', 'create', `v${next}`, ...files, '--repo', REPO, '--title', `Digi-Tactics ${next}`, '--notes-file', notesFile, '--latest']);
    } finally { fs.rmSync(notesFile, { force: true }); }
    fs.writeFileSync(STATE_FILE, JSON.stringify({ version: next, date: latest.date, sha256: latest.sha256, size, repo: REPO, full, rules: snap }, null, 2) + '\n');
    console.log('');
    console.log(`published version ${next}${fullZip ? ' (with full build)' : ''}`);
    console.log(`release: https://github.com/${REPO}/releases/tag/v${next}`);
  } catch (e) {
    fs.writeFileSync(verFile, verRaw); // no release was created: keep the old version number
    throw e;
  }
}

main().catch((e) => { console.error(`publish failed: ${e.message}`); process.exit(1); });
