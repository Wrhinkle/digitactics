// Copies the playable game into an explicitly selected static-hosting output directory.
// Script URLs get a version stamp so every player loads the same build (multiplayer needs identical code).
// Run: node tools/publish-web.cjs <output-parent-dir>; deploy that output using your own hosting account.
const fs = require('fs');
const path = require('path');

const root = path.join(__dirname, '..');
if (!process.argv[2]) throw new Error('Pass an output parent directory; no personal hosting destination is configured.');
const dest = path.resolve(process.argv[2], 'digitactics');
if (dest === root || root.startsWith(dest + path.sep) || dest.startsWith(root + path.sep)) throw new Error('Output must be outside the source repository.');
const FILES = ['style.css', 'js/data.js', 'js/items.js', 'js/sets.js', 'js/combat.js', 'js/duos.js', 'js/game.js', 'js/lockstep.js', 'js/models.js', 'js/credits-data.js', 'CREDITS.md', 'THIRD_PARTY_NOTICES.txt', 'docs/credits/MODELS.md', 'docs/credits/model-assets.json', 'docs/credits/contributors.json', 'docs/art/set-carousel-art.json', 'js/cosmetics.js', 'js/bundle.js', 'models/index.json',
  'icons/favicon.svg', 'icons/favicon-16.png', 'icons/favicon-32.png', 'icons/apple-touch-icon.png', 'icons/icon-192.png', 'icons/icon-512.png', 'icons/site.webmanifest'];
// set packages: every js/sets/*.js (index.html loads them with plain script tags)
for (const f of fs.readdirSync(path.join(root, 'js', 'sets'))) if (f.endsWith('.js')) FILES.push(`js/sets/${f}`);
const stamp = Date.now().toString(36);

fs.rmSync(dest, { recursive: true, force: true });
for (const rel of FILES) {
  fs.mkdirSync(path.dirname(path.join(dest, rel)), { recursive: true });
  fs.copyFileSync(path.join(root, rel), path.join(dest, rel));
}
for (const dir of ['files', 'portraits', 'legends', 'items']) fs.cpSync(path.join(root, 'models', dir), path.join(dest, 'models', dir), { recursive: true });
fs.cpSync(path.join(root, 'assets', 'client'), path.join(dest, 'assets', 'client'), { recursive: true }); // client art (js/client.js)

let html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
html = html.replace(/(src|href)="((?:js\/[\w./-]+\.js)|style\.css)"/g, `$1="$2?v=${stamp}"`);
html = html.replace('<meta name="viewport"', '<meta name="robots" content="noindex">\n  <meta name="viewport"');
fs.writeFileSync(path.join(dest, 'index.html'), html);

let n = 0, bytes = 0;
const walk = (d) => { for (const e of fs.readdirSync(d, { withFileTypes: true })) { const p = path.join(d, e.name); if (e.isDirectory()) walk(p); else { n++; bytes += fs.statSync(p).size; } } };
walk(dest);
console.log(`published ${n} files (${(bytes / 1048576).toFixed(1)} MB) to ${dest}, build ${stamp}`);
