// Export the original offer card's chosen ItemArt token, rather than guessing from the offer kind.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const { ruleFiles } = require('../../tools/headless.cjs');
const root = path.resolve(__dirname, '../..'), out = path.join(root, 'native/content/choice-art');
const files = ruleFiles();
const sha = bytes => crypto.createHash('sha256').update(bytes).digest('hex');

async function main() {
  const browser = await chromium.launch({ channel: 'msedge', headless: true, args: ['--disable-gpu'] });
  try {
    const page = await browser.newPage();
    await page.route('**/*', async route => {
      const url = new URL(route.request().url());
      if (url.hostname !== 'choices.local') return route.abort();
      if (url.pathname === '/') return route.fulfill({ contentType: 'text/html', body: '<!doctype html>'
        + files.map(file => `<script src="/${file}"></script>`).join('')
        + '<script type="module">await import("/js/itemart.js");window.choiceArtReady=true;</script>' });
      const file = path.resolve(root, '.' + decodeURIComponent(url.pathname));
      if (!file.startsWith(root + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) return route.fulfill({ status: 404, body: '' });
      const types = { '.js': 'text/javascript', '.json': 'application/json', '.webp': 'image/webp', '.png': 'image/png' };
      return route.fulfill({ contentType: types[path.extname(file)] || 'application/octet-stream', body: fs.readFileSync(file) });
    });
    await page.goto('http://choices.local/');
    await page.waitForFunction(() => window.choiceArtReady);
    const setIds = await page.evaluate(() => DT.Sets.pickable().map(set => set.id));
    const manifest = { version: 1, size: 128, source: Object.fromEntries([...files, 'js/itemart.js', 'js/ui.js'].map(file => [file, sha(fs.readFileSync(path.join(root, file)))])), variants: {}, samples: [] };
    const images = new Map();
    for (const set of setIds) for (const pace of ['normal', 'hyper']) {
      const result = await page.evaluate(async ({ set, pace }) => {
        DT.Sets.activate(set, pace);
        const def = DT.Sets.def(set), kinds = DT.SET.mechanic.offerKinds || {};
        const entries = {}, pngs = {}, originalImg = DT.ItemArt.img;
        let tokens = [];
        DT.ItemArt.img = (id, size, cls) => { tokens.push(id); return originalImg(id, size, cls); };
        async function add(kind, id, card, tier = null) {
          tokens = []; const sourceCard = card();
          if (tokens.length > 1) throw Error(`Multiple choice images need explicit native layout: ${set}/${kind}/${id}`);
          const token = tokens[0] || null;
          const painted = !!sourceCard.img;
          if (painted && !token) throw Error(`Non-ItemArt choice image: ${set}/${kind}/${id}`);
          if (painted && !pngs[token]) {
            const canvas = document.createElement('canvas'); canvas.width = canvas.height = 128;
            if (!await DT.ItemArt.paint(canvas, token)) throw Error(`Choice artwork did not paint: ${set}/${token}`);
            const pixels = canvas.getContext('2d').getImageData(0, 0, 128, 128).data;
            if (!pixels.some((value, index) => index % 4 === 3 && value > 0)) throw Error(`Empty choice artwork: ${set}/${token}`);
            pngs[token] = canvas.toDataURL('image/png').split(',')[1];
          }
          (entries[kind] ||= {})[id] = { token, file: painted ? token : null, tier };
        }
        try {
          const augments = [...new Map([...DT.AUGMENTS, ...(DT.Duos?.AUGMENTS || [])].map(a => [a.id, a])).values()];
          for (const [kind, definitions] of [['augment', augments], ['power', DT.POWERS]])
            for (const definition of definitions)
              await add(kind, definition.id, () => ({ img: DT.ItemArt.img('aug:' + definition.id, 64, 'augart') }), definition.tier || 1);
          for (const [kind, spec] of Object.entries(kinds)) {
            let ids = [];
            if (kind === 'col_core') ids = (def.powerUps || []).map(x => x.id);
            else if (kind === 'col_form') ids = Object.values(def.forms || {}).flat().map(x => x.id);
            else if (kind === 'col_gambit') ids = ['cash:0', 'double:0'];
            else if (kind === 'arcAnomaly') ids = Object.keys(DT.ArcaneInfo.ANOM);
            else if (kind === 'arcJunk') ids = Object.keys(DT.ArcaneInfo.JUNK);
            else if (kind === 'upgrade') ids = DT.AUGMENTS.map(x => x.id);
            else if (kind === 'ul_tamer') ids = DT.POWERS.map(x => x.id);
            else if (['rec_bless', 'rec_radiant', 'rec_tome', 'arcMarket', 'ac_loot', 'ac_relic'].includes(kind)) ids = Object.keys(DT.ITEMS);
            if (kind === 'arcMarket') ids.push('arc_pass:0');
            if (kind === 'drg_treasure') {
              ids = ['gold:0', 'unit:unknown', 'chaos', 'dup:0'];
              for (const item of Object.keys(DT.ITEMS)) for (const tag of ['comp', 'item', 'emblem', 'rad']) ids.push(tag + ':' + item);
            }
            for (const id of ids) await add(kind, id, () => spec.card(id));
          }
        } finally { DT.ItemArt.img = originalImg; }
        const samples = [];
        for (const [kind, choices] of Object.entries(entries)) {
          const tiers = new Set();
          for (const [id, art] of Object.entries(choices)) {
            const item = DT.ITEMS[id];
            if (['rec_bless', 'rec_radiant'].includes(kind) && item?.cat !== 'radiant') continue;
            if (kind === 'rec_tome' && !item?.emblem) continue;
            if (kind === 'ac_relic' && !item?.relic) continue;
            if (kind === 'ac_loot' && !item?.component) continue;
            if (kind === 'arcMarket' && item?.cat !== 'contraband') continue;
            if (kind === 'augment' && !DT.AUGMENTS.some(a => a.id === id)) continue;
            if (!art.file || tiers.has(art.tier)) continue;
            tiers.add(art.tier);
            samples.push({ kind, key: id, id: kind === 'drg_treasure' ? 'dup:5' : kind === 'col_gambit' ? 'cash:25' : id });
          }
        }
        return { entries, pngs, samples };
      }, { set, pace });
      const hashes = {};
      for (const [token, png] of Object.entries(result.pngs)) { const bytes = Buffer.from(png, 'base64'), hash = sha(bytes); hashes[token] = hash; images.set(hash, bytes); }
      for (const choices of Object.values(result.entries)) for (const entry of Object.values(choices)) if (entry.file) entry.file = hashes[entry.file];
      manifest.variants[set + '/' + pace] = result.entries;
      for (const sample of result.samples) manifest.samples.push({ variant: set + '/' + pace, kind: sample.kind, id: sample.id, art: result.entries[sample.kind][sample.key] });
      console.log(`${set}/${pace}: ${Object.values(result.entries).reduce((n, choices) => n + Object.keys(choices).length, 0)} choice mappings`);
    }
    fs.mkdirSync(out, { recursive: true });
    for (const [hash, bytes] of images) fs.writeFileSync(path.join(out, hash + '.png'), bytes);
    fs.writeFileSync(path.join(out, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
    // Only this exporter's content-addressed files are eligible for cleanup.
    for (const file of fs.readdirSync(out)) if (/^[a-f0-9]{64}\.png$/.test(file) && !images.has(file.slice(0, -4))) fs.unlinkSync(path.join(out, file));
    console.log(`Exported ${images.size} original choice images across ${Object.keys(manifest.variants).length} variants.`);
  } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
