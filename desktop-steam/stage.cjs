// Stages the updatable payload into desktop-steam/payload: the main-process files (main.js, preload.js, steam.js,
// updater.js, ziplib.js, update-config.json (GitHub owner/repo only; falls back to update-config.example.json), version.json) plus the game folder with the files the page loads (the
// same list as desktop/build.py) and all 3D models and client art. electron-builder ships it as resources/payload;
// publish-update.cjs zips the same folder for the self-updater. Run: node stage.cjs  (npm run stage)
const fs = require('fs');
const path = require('path');
const ROOT = path.join(__dirname, '..');
const PAYLOAD = path.join(__dirname, 'payload');
const OUT = path.join(PAYLOAD, 'game');
const MAIN_FILES = ['main.js', 'preload.js', 'steam.js', 'updater.js', 'ziplib.js'];
const GAME_FILES = ['index.html', 'style.css', 'js/data.js', 'js/items.js', 'js/sets.js', 'js/combat.js', 'js/duos.js', 'js/game.js', 'js/lockstep.js',
  'js/models.js', 'js/credits-data.js', 'README.md', 'SECURITY.md', 'docs/ONLINE_AND_UPDATES.md', 'docs/DEVELOPMENT.md', 'CREDITS.md', 'THIRD_PARTY_NOTICES.txt', 'docs/credits/MODELS.md', 'docs/credits/model-assets.json', 'docs/credits/contributors.json', 'docs/art/set-carousel-art.json', 'js/cosmetics.js', 'js/bundle.js', 'models/index.json',
  'icons/favicon.svg', 'icons/favicon-16.png', 'icons/favicon-32.png', 'icons/apple-touch-icon.png', 'icons/icon-192.png', 'icons/icon-512.png', 'icons/site.webmanifest'];

fs.rmSync(PAYLOAD, { recursive: true, force: true });
fs.rmSync(path.join(__dirname, 'game'), { recursive: true, force: true }); // old layout (before the self-updater)
fs.mkdirSync(PAYLOAD, { recursive: true });
for (const f of MAIN_FILES) fs.copyFileSync(path.join(__dirname, f), path.join(PAYLOAD, f));
// Ship only the repository address. Never copy credentials from legacy config or update-token.txt.
// Private QA may supply its own DT_UPDATE_TOKEN at runtime; public releases need no credentials.
const cfgName = fs.existsSync(path.join(__dirname, 'update-config.json')) ? 'update-config.json' : 'update-config.example.json';
const cfg = JSON.parse(fs.readFileSync(path.join(__dirname, cfgName), 'utf8'));
if (!/^[\w.-]+$/.test(cfg.owner || '') || !/^[\w.-]+$/.test(cfg.repo || '')) throw new Error('Invalid update repository');
fs.writeFileSync(path.join(PAYLOAD, 'update-config.json'), JSON.stringify({ owner: cfg.owner, repo: cfg.repo }, null, 2));
const files = GAME_FILES.concat(fs.readdirSync(path.join(ROOT, 'js', 'sets')).filter((f) => f.endsWith('.js')).sort().map((f) => `js/sets/${f}`));
for (const rel of files) {
  fs.mkdirSync(path.dirname(path.join(OUT, rel)), { recursive: true });
  fs.copyFileSync(path.join(ROOT, rel), path.join(OUT, rel));
}
for (const sub of ['files', 'portraits', 'legends', 'items']) {
  const src = path.join(ROOT, 'models', sub);
  if (fs.existsSync(src)) fs.cpSync(src, path.join(OUT, 'models', sub), { recursive: true });
}
// client art (js/client.js): Home banners, mode cards, emblems, renders
if (fs.existsSync(path.join(ROOT, 'assets', 'client'))) fs.cpSync(path.join(ROOT, 'assets', 'client'), path.join(OUT, 'assets', 'client'), { recursive: true });
// version.json: the payload version (bumped by publish-update.cjs), the oldest shell (boot.js SHELL) it runs on, and the
// multiplayer protocol version from js/net-steam.js
const ver = JSON.parse(fs.readFileSync(path.join(__dirname, 'version.json'), 'utf8'));
const proto = (fs.readFileSync(path.join(ROOT, 'js', 'net-steam.js'), 'utf8').match(/PROTO_VERSION = '([^']+)'/) || [])[1] || '';
fs.writeFileSync(path.join(PAYLOAD, 'version.json'), JSON.stringify({ version: ver.version, minShell: ver.minShell || 1, proto, date: new Date().toISOString() }, null, 2));
let n = 0;
let bytes = 0;
const walk = (d) => { for (const e of fs.readdirSync(d, { withFileTypes: true })) { const p = path.join(d, e.name); if (e.isDirectory()) walk(p); else { n++; bytes += fs.statSync(p).size; } } };
walk(PAYLOAD);
require('./audit-package.cjs').auditPackage(PAYLOAD);
console.log(`staged payload ${ver.version}: ${n} files, ${(bytes / 1048576).toFixed(1)} MB in ${PAYLOAD}`);
console.log(`updates from github.com/${cfg.owner}/${cfg.repo} releases; no credentials packaged (anonymous updates require public releases)`);
