// Bake original SVG/CSS animation frames; browser code is a build dependency only.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const { exportPreview, exportNativeReferences } = require('./emote-preview-export.cjs');
const root = path.resolve(__dirname, '../..'), out = path.join(root, 'native/content/emotes');
const source = fs.readFileSync(path.join(root, 'js/emotes.js'), 'utf8');
const mysterySource = fs.readFileSync(path.join(root, 'js/mystery-content.js'), 'utf8');
const artBoundary = mysterySource.indexOf('// Distinct eight-beat');
if (artBoundary < 0) throw Error('Missing mystery sticker source boundary');
const mysteryArtSource = mysterySource.slice(0, artBoundary);
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
const sourceHash = hash(source + mysteryArtSource + fs.readFileSync(__filename, 'utf8') + fs.readFileSync(path.join(__dirname,'emote-preview-export.cjs'),'utf8'));
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
  const referenceDirectory=path.join(root,'native/artifacts/emote-preview-reference');
  let referencesCurrent=false;
  try {
    const reference=JSON.parse(fs.readFileSync(path.join(referenceDirectory,'manifest.json'),'utf8'));
    referencesCurrent=reference.sourceHash===sourceHash&&reference.clipCount>0&&reference.pages.length===Math.ceil(reference.clipCount/12)*2&&reference.pages.every(page=>hash(fs.readFileSync(path.join(referenceDirectory,page.file)))===page.sha256);
  } catch (_) { /* missing references are rebuilt with the atlas */ }
  if (fs.existsSync(manifestPath)) {
    const previous = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
    if (referencesCurrent && previous.sourceHash === sourceHash && Object.values(previous.clips).every(c => c.preview?.length && [c,...c.preview].every(a=>fs.existsSync(path.join(out,a.file))&&hash(fs.readFileSync(path.join(out,a.file)))===a.sha256))) {
      console.log('Original emote frames verified; render cache is current.'); return;
    }
  }
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 1120 }, deviceScaleFactor: 1 });
    // Render source artwork directly. index.html may reference an older checked-in
    // browser bundle; an atlas must never claim a new source hash for stale art.
    const artContext = {}; require('vm').createContext(artContext);
    require('vm').runInContext(mysteryArtSource.replace(/export /g, ''), artContext);
    require('vm').runInContext(source.slice(source.indexOf('const OL ='), source.indexOf('// ---------- styles')).replace(/export /g, '') + ';this.items=EMOTES.map(e=>({id:e.id,data:e.data||{},art:stickerHTML(e.id)}));', artContext);
    const items = artContext.items;
    if (!items.length || items.some(item => !item.art)) throw Error('Missing source emote artwork');
    await page.setContent('<!doctype html><html><head></head><body></body></html>');
    const manifest = { sourceHash, tile: 160, columns: 8, frames: 52, frameMs: 50, duration: 2600, clips: {} };
    const previewChecks={sourceHash,clips:{},layerCount:0,decodedBytes:0,maxAtlasHeight:0};
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
      await page.evaluate(item => {
        const sheet = document.getElementById('native-emote-sheet'); sheet.replaceChildren();sheet.style.height='1120px';
        const art = item.art;
        for (let frame = 0; frame < 52; frame++) {
          const cell = document.createElement('div'); cell.style.cssText = 'width:160px;height:160px;padding:20px;box-sizing:border-box;overflow:hidden;background:transparent;';
          cell.innerHTML = art; sheet.append(cell);
          for (const animation of cell.getAnimations({ subtree: true })) { animation.pause(); animation.currentTime = frame * 50; }
        }
      }, item);
      const bytes = await page.locator('#native-emote-sheet').screenshot({ omitBackground: true, animations: 'allow' });
      if (bytes.readUInt32BE(16) !== 1280 || bytes.readUInt32BE(20) !== 1120) throw Error('Unexpected atlas size');
      if (item.id.startsWith('mystery_')) {
        const problem = await page.evaluate(async data => {
          const image = new Image(); image.src = 'data:image/png;base64,' + data; await image.decode();
          const canvas = document.createElement('canvas'); canvas.width = 1280; canvas.height = 1120;
          const ctx = canvas.getContext('2d'); ctx.drawImage(image, 0, 0);
          const poses = new Set();
          for (let frame = 0; frame < 52; frame++) {
            const pixels = ctx.getImageData(frame % 8 * 160, Math.floor(frame / 8) * 160, 160, 160).data;
            let visible = false, signature = 2166136261;
            for (let y = 0; y < 160; y++) for (let x = 0; x < 160; x++) {
              const at = (y * 160 + x) * 4, alpha = pixels[at + 3];
              if (alpha) {
                visible = true;
                if (x < 2 || y < 2 || x >= 158 || y >= 158) return 'clipped frame ' + frame;
              }
              for (let c = 0; c < 4; c++) signature = Math.imul(signature ^ pixels[at + c], 16777619);
            }
            if (!visible) return 'empty frame ' + frame;
            poses.add(signature);
          }
          return poses.size < 4 ? 'frozen animation' : null;
        }, bytes.toString('base64'));
        if (problem) throw Error(item.id + ': ' + problem);
      }
      const file = item.id + '.png'; fs.writeFileSync(path.join(out, file), bytes);
      const preview=await exportPreview(page,out,item);
      manifest.clips[item.id] = { file, data: item.data, sha256: hash(bytes),preview:preview.layers };
      previewChecks.clips[item.id]=preview.comparison;
      for(const layer of preview.layers){previewChecks.layerCount++;previewChecks.decodedBytes+=layer.columns*layer.rows*160*160*4;previewChecks.maxAtlasHeight=Math.max(previewChecks.maxAtlasHeight,layer.rows*160);}
      console.log('Emote: ' + item.id);
    }
    await exportNativeReferences(page,referenceDirectory,items,sourceHash);
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    fs.writeFileSync(path.join(root,'native/artifacts/emote-preview-comparisons.json'),JSON.stringify(previewChecks,null,2)+'\n');
    console.log(`Exported ${items.length} original 52-frame match emotes and ${previewChecks.layerCount} independently looping preview layers; ${previewChecks.decodedBytes} decoded bytes if all previews were loaded, maximum atlas height ${previewChecks.maxAtlasHeight}.`);
  } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
