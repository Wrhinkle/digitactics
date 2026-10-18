const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..'), out = path.join(root, 'native/content/portraits');
async function main() {
  const models = JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'),'utf8'));
  const portraits = JSON.parse(fs.readFileSync(path.join(root,'models/portraits/index.json'),'utf8'));
  const keys = [...new Set(Object.values(models.variants).flatMap(lines => Object.values(lines).flat()).filter(Boolean))].sort();
  const browser = await chromium.launch({channel:'msedge',headless:true});
  fs.mkdirSync(out,{recursive:true});
  try {
    const page = await browser.newPage(), cache = new Map(), manifest = {};
    for (const key of keys) {
      const entry = portraits[key];
      if (!entry || entry.src !== models.sources[key].asset) throw Error(`Missing or stale portrait: ${key}`);
      if (!cache.has(entry.file)) {
        const bytes = fs.readFileSync(path.join(root,'models/portraits',entry.file));
        const png = await page.evaluate(async data => {
          const image = new Image(); image.src = 'data:image/webp;base64,'+data; await image.decode();
          const canvas = document.createElement('canvas'); canvas.width=image.width; canvas.height=image.height;
          canvas.getContext('2d').drawImage(image,0,0); return canvas.toDataURL('image/png').split(',')[1];
        }, bytes.toString('base64'));
        const data = Buffer.from(png,'base64'), file = crypto.createHash('sha256').update(data).digest('hex')+'.png';
        fs.writeFileSync(path.join(out,file),data); cache.set(entry.file,file);
      }
      manifest[key] = cache.get(entry.file);
    }
    fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
    console.log(`Exported ${keys.length} current model portraits (${new Set(Object.values(manifest)).size} unique PNGs).`);
  } finally {await browser.close();}
}
main().catch(error=>{console.error(error);process.exitCode=1;});
