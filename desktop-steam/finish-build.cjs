// After electron-builder: copies the build to the repo's dist/ as dist/DigiTactics-Steam/ (folder) and
// dist/DigiTactics-Steam.exe (portable single file), checks the Steam files are next to the exe and prints sizes.
const fs = require('fs');
const path = require('path');
const OUT = path.join(__dirname, 'out');
const DIST = path.join(__dirname, '..', 'dist');
const folder = path.join(DIST, 'DigiTactics-Steam');
const size = (p) => { const s = fs.statSync(p); if (!s.isDirectory()) return s.size; return fs.readdirSync(p).reduce((n, f) => n + size(path.join(p, f)), 0); };
const mb = (n) => `${(n / 1048576).toFixed(1)} MB`;
fs.mkdirSync(DIST, { recursive: true });
fs.rmSync(folder, { recursive: true, force: true });
// Smoke/rollback QA may run the unpacked executable while the portable archive is being built.
// Chromium can leave a crash diagnostic in its working directory; it is not a release asset.
fs.cpSync(path.join(OUT, 'win-unpacked'), folder, { recursive: true, filter: (src) => src !== path.join(OUT, 'win-unpacked', 'debug.log') });
fs.copyFileSync(path.join(OUT, 'DigiTactics-Steam.exe'), path.join(DIST, 'DigiTactics-Steam.exe'));
for (const f of ['Digi-Tactics.exe', 'steam_appid.txt', 'steam_api64.dll', 'resources/payload/game/index.html', 'resources/payload/main.js', 'resources/payload/version.json', 'resources/app.asar.unpacked/node_modules/steamworks.js/dist/win64/steam_api64.dll']) {
  if (!fs.existsSync(path.join(folder, f))) { console.error(`missing in the build: ${f}`); process.exit(1); }
}
const appid = fs.readFileSync(path.join(folder, 'steam_appid.txt'), 'utf8').trim();
if (appid !== '480') { console.error(`steam_appid.txt says ${appid}`); process.exit(1); }
require('./audit-package.cjs').auditPackage(folder);
console.log(`${folder}  ${mb(size(folder))}`);
console.log(`${path.join(DIST, 'DigiTactics-Steam.exe')}  ${mb(size(path.join(DIST, 'DigiTactics-Steam.exe')))}`);
