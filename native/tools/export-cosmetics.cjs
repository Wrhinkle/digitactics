const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  let catalog;
  try {
    const page = await browser.newPage();
    await page.goto(require('url').pathToFileURL(path.join(root, 'index.html')).href);
    await page.waitForFunction(() => window.DT?.Shop && DT.Cosmetics.list('elimsound').length > 0);
    catalog = await page.evaluate(() => Object.fromEntries(DT.Cosmetics.KINDS.map(k => [k, JSON.parse(JSON.stringify(DT.Cosmetics.list(k)))])));
  } finally { await browser.close(); }
  fs.writeFileSync(path.join(root, 'native/src/DigiTactics.Storage/cosmetics.json'), JSON.stringify(catalog));
  const initial = { bits: 100000, owned: {}, equipped: {}, history: [], future: 'keep' };
  const storage = { 'dt.cosmetics.v1': JSON.stringify(initial) };
  const ctx = { DT: {}, console, localStorage: { getItem: k => storage[k] || null, setItem: (k,v) => storage[k] = v } }; ctx.window = ctx; vm.createContext(ctx);
  vm.runInContext(fs.readFileSync(path.join(root, 'js/cosmetics.js'), 'utf8'), ctx);
  for (const [kind, items] of Object.entries(catalog)) ctx.DT.Cosmetics.register(kind, items);
  const source = fs.readFileSync(path.join(root, 'js/shop.js'), 'utf8');
  vm.runInContext(`const C=DT.Cosmetics; let date='2026-10-17'; const today=()=>date;` + source.slice(source.indexOf('const KINDS'), source.indexOf('// Daily missions')) + source.match(/^const hash = .*$/m)[0] + source.slice(source.indexOf('const sellable'), source.indexOf('function eggPool')) + '\nthis.setDay=d=>date=d;this.priceOf=priceOf;', ctx);
  const prices = [], featured = [];
  for (const day of ['2026-10-17','2026-10-18','2026-11-01']) {
    ctx.setDay(day);
    featured.push({day,items:vm.runInContext('featured().map(x=>x.kind+"/"+x.item.id)',ctx)});
    for (const [kind, items] of Object.entries(catalog)) for (const item of items) prices.push({ day, kind, id:item.id, price:ctx.priceOf(kind,item) });
  }
  const cases = [];
  function act(op, kind, id, slot) {
    const result = ctx.DT.Cosmetics[op](kind,id,slot);
    cases.push({op,kind,id,slot,result,save:JSON.parse(storage['dt.cosmetics.v1']),equipped:Object.fromEntries(Object.keys(catalog).map(k=>[k,ctx.DT.Cosmetics.equipped(k)])),owned:Object.fromEntries(Object.entries(catalog).map(([k,items])=>[k,items.filter(it=>ctx.DT.Cosmetics.owns(k,it.id)).map(it=>it.id)]))});
  }
  for (const [kind, items] of Object.entries(catalog)) {
    act('equip',kind,'unknown',0);
    for (let i=0;i<items.length;i++) { const id=items[i].id; act('equip',kind,id,i%6); act('grant',kind,id); act('equip',kind,id,i%6); if(i%5===0)act('equip',kind,id,(i+1)%6); }
    act('equip',kind,null,0);
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/cosmetics.json.gz'),zlib.gzipSync(JSON.stringify({initial,cases,prices,featured})));
  console.log(`Exported ${Object.values(catalog).flat().length} cosmetics, ${cases.length} operations and ${prices.length} prices.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
