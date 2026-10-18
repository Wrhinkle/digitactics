// Bake original SVG/CSS animation frames; browser code is a build dependency only.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..'), out = path.join(root, 'native/content/emotes');
const source = fs.readFileSync(path.join(root, 'js/emotes.js'), 'utf8');
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
const sourceHash = hash(source + fs.readFileSync(path.join(root, 'js/mystery-content.js'), 'utf8') + fs.readFileSync(__filename, 'utf8'));
async function main() {
  let now = 0;
  const context = { sent: new Map(), RATE_MS: 6000, RATE_N: 3, performance: { now: () => now } };
  require('vm').createContext(context);
  require('vm').runInContext(source.slice(source.indexOf('function allow(slot)'), source.indexOf('const game =')), context);
  const cases = [];
  for (let i = 0; i < 1000; i++) { now += [0, 1, 1, 1, 5997, 1, 0, 1, 6000][i % 9]; const seat = Math.floor(i / 18) % 8; cases.push({ seat, now, allowed: context.allow(seat) }); }
  fs.writeFileSync(path.join(root, 'native/tests/fixtures/emote-rate.json.gz'), require('zlib').gzipSync(JSON.stringify(cases)));
  fs.mkdirSync(out, { recursive: true });
  const manifestPath = path.join(out, 'manifest.json');
  if (fs.existsSync(manifestPath)) {
    const previous = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
    if (previous.sourceHash === sourceHash && Object.values(previous.clips).every(c => fs.existsSync(path.join(out, c.file)) && hash(fs.readFileSync(path.join(out, c.file))) === c.sha256)) {
      console.log('Original emote frames verified; render cache is current.'); return;
    }
  }
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 1120 }, deviceScaleFactor: 1 });
    await page.goto(require('url').pathToFileURL(path.join(root, 'index.html')).href);
    await page.waitForFunction(() => window.DT?.Emotes && DT.Cosmetics.list('emote').length > 0);
    const items = await page.evaluate(() => DT.Cosmetics.list('emote').map(e => ({ id: e.id, data: e.data || {} })));
    const manifest = { sourceHash, tile: 160, columns: 8, frames: 52, frameMs: 50, duration: 2600, clips: {} };
    await page.evaluate(cssSource => {
      document.body.replaceChildren();
      document.documentElement.style.cssText = 'background:transparent!important;margin:0!important;';
      document.body.style.cssText = 'background:transparent!important;margin:0!important;padding:0!important;min-width:0!important;';
      new Function(cssSource + '\ninjectCss();')();
      const sheet = document.createElement('div'); sheet.id = 'native-emote-sheet';
      sheet.style.cssText = 'display:grid;grid-template-columns:repeat(8,160px);width:1280px;height:1120px;background:transparent;';
      document.body.append(sheet);
    }, source.slice(source.indexOf('function injectCss()'), source.indexOf('const rarityOrder')));
    for (const item of items) {
      await page.evaluate(id => {
        const sheet = document.getElementById('native-emote-sheet'); sheet.replaceChildren();
        const art = DT.Emotes.sticker(id); if (!art) throw Error('Missing original sticker: ' + id);
        for (let frame = 0; frame < 52; frame++) {
          const cell = document.createElement('div'); cell.style.cssText = 'width:160px;height:160px;padding:20px;box-sizing:border-box;overflow:hidden;background:transparent;';
          cell.innerHTML = art; sheet.append(cell);
          for (const animation of cell.getAnimations({ subtree: true })) { animation.pause(); animation.currentTime = frame * 50; }
        }
      }, item.id);
      const bytes = await page.locator('#native-emote-sheet').screenshot({ omitBackground: true, animations: 'allow' });
      if (bytes.readUInt32BE(16) !== 1280 || bytes.readUInt32BE(20) !== 1120) throw Error('Unexpected atlas size');
      const file = item.id + '.png'; fs.writeFileSync(path.join(out, file), bytes);
      manifest.clips[item.id] = { file, data: item.data, sha256: hash(bytes) };
      console.log('Emote: ' + item.id);
    }
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    console.log(`Exported ${items.length} original animated emotes, 52 frames each.`);
  } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
