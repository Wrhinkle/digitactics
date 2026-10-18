// Bake the original asynchronous itemCanvas result, including set art and legacy fallbacks.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..'), out = path.join(root, 'native/content/item-tokens');
async function main() {
  fs.mkdirSync(out, { recursive: true });
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage();
    await page.route('**/*', async route => {
      const url = new URL(route.request().url());
      if (url.hostname !== 'tokens.local') return route.abort();
      const file = path.resolve(root, '.' + decodeURIComponent(url.pathname));
      if (!file.startsWith(root + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) return route.fulfill({ status: 404, body: '' });
      const types = { '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json', '.webp': 'image/webp', '.png': 'image/png', '.css': 'text/css' };
      return route.fulfill({ contentType: types[path.extname(file)] || 'application/octet-stream', body: fs.readFileSync(file) });
    });
    await page.goto('http://tokens.local/index.html');
    await page.waitForFunction(() => window.DT?.ItemArt && DT.Sets);
    const sets = await page.evaluate(() => DT.Sets.ids());
    const manifest = { version: 1, size: 128, sets: {} };
    let count = 0; const files = new Set();
    for (const set of sets) {
      const icons = await page.evaluate(async set => {
        DT.Sets.activate(set);
        const { itemCanvas } = await import('/js/icons.js?set=' + encodeURIComponent(set));
        const result = {};
        for (const id of Object.keys(DT.ITEMS)) {
          const canvas = itemCanvas(id); await canvas.ready;
          result[id] = canvas.toDataURL('image/png').split(',')[1];
        }
        for (const augment of set === '_template' ? [] : [...new Map([...DT.AUGMENTS, ...(DT.Duos?.AUGMENTS || [])].map(a => [a.id, a])).values()]) {
          const id = 'aug:' + augment.id;
          if (!DT.ItemArt.has(id)) continue; // Original UI also omits artwork when no painter exists.
          const canvas = document.createElement('canvas'); canvas.width = canvas.height = 128;
          if (!await DT.ItemArt.paint(canvas, id)) throw Error(`Missing original augment art: ${set}/${id}`);
          result[id] = canvas.toDataURL('image/png').split(',')[1];
        }
        return result;
      }, set);
      manifest.sets[set] = {};
      for (const [id, png] of Object.entries(icons)) {
        const bytes = Buffer.from(png, 'base64');
        const hash = crypto.createHash('sha256').update(bytes).digest('hex');
        const file = hash + '.png';
        if (!files.has(file)) fs.writeFileSync(path.join(out, file), bytes);
        files.add(file); manifest.sets[set][id] = file; count++;
      }
      console.log(`${set}: ${Object.keys(icons).length} item tokens`);
    }
    const catalog = JSON.parse(fs.readFileSync(path.join(root, 'native/content/catalog.json'), 'utf8'));
    for (const variant of catalog.variants)
      for (const item of variant.items)
        if (!manifest.sets[variant.id.split('/')[0]]?.[item.id]) throw Error(`Missing native item artwork: ${variant.id}/${item.id}`);
    fs.writeFileSync(path.join(out, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
    for (const file of fs.readdirSync(out))
      if (/^[a-f0-9]{64}\.png$/.test(file) && !files.has(file)) fs.unlinkSync(path.join(out, file));
    console.log(`Exported ${count} item mappings, ${files.size} unique PNGs across ${sets.length} rulesets.`);
  } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
