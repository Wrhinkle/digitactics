// Isolated browser profile: never opens or changes the player's actual saves.
const fs = require('fs'), path = require('path'), assert = require('assert/strict');
const { chromium } = require(process.argv[2] || '../native/artifacts/audio-tools/node_modules/playwright');
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage({ acceptDownloads: true });
    await page.route('http://localhost/profile-test', route => route.fulfill({ contentType: 'text/html', body: '<button data-profile-backup>Export save backup</button>' }));
    await page.goto('http://localhost/profile-test');
    await page.evaluate(() => { window.DT = { Client: { toast(message) { window.backupError = message; } } }; });
    await page.addScriptTag({ content: fs.readFileSync(path.join(__dirname, '../js/profile-backup.js'), 'utf8') });
    let backup = await page.evaluate(() => DT.ProfileBackup.snapshot());
    assert.deepEqual(backup.storage, {}); assert.equal(backup.music.length, 0);
    assert.equal(await page.evaluate(async () => (await indexedDB.databases()).some(db => db.name === 'dt-music')), false);
    const expected = { 'dt.cosmetics.v1': '{"bits":491,"owned":{"legend":["unknown-future-id"]}}', 'dt.client.v1': '{"matches":[{"place":1}],"future":true}', 'dt.audio.v1': '{"source":"own","music":0.2}', 'dt.future': 'not JSON: retained exactly', 'dt-unknown': 'also retained' };
    await page.evaluate(async expected => {
      for (const [key, value] of Object.entries(expected)) localStorage.setItem(key, value);
      localStorage.setItem('unrelated', 'excluded'); localStorage.setItem('dtp:cache', 'rebuildable portrait');
      await new Promise((resolve, reject) => {
        const req = indexedDB.open('dt-music', 1);
        req.onupgradeneeded = () => req.result.createObjectStore('tracks', { keyPath: 'id' });
        req.onerror = () => reject(req.error);
        req.onsuccess = () => {
          const db = req.result, tx = db.transaction('tracks', 'readwrite');
          tx.objectStore('tracks').put({ id: 'track-1', name: '../music \u03a9', type: 'audio/wav', use: 'combat', size: 5, added: 123, future: { keep: true }, blob: new File([new Uint8Array([0, 1, 2, 254, 255])], 'original.wav', { type: 'audio/wav', lastModified: 456 }) });
          tx.oncomplete = () => { db.close(); resolve(); }; tx.onerror = () => reject(tx.error);
        };
      });
    }, expected);
    backup = await page.evaluate(() => DT.ProfileBackup.snapshot());
    assert.deepEqual(backup.storage, expected); assert.equal(backup.music.length, 1);
    assert.equal(backup.music[0].metadata.future.keep, true); assert.equal(backup.music[0].data, 'AAEC/v8=');
    assert.deepEqual(backup.music[0].blob, { type: 'audio/wav', name: 'original.wav', lastModified: 456 });
    assert.equal(backup.music[0].sha256, require('crypto').createHash('sha256').update(Buffer.from([0,1,2,254,255])).digest('hex'));
    const downloaded = page.waitForEvent('download'); await page.click('[data-profile-backup]');
    const download = await downloaded; const actual = JSON.parse(fs.readFileSync(await download.path(), 'utf8'));
    assert.deepEqual(actual.storage, expected); assert.deepEqual(actual.music, backup.music);
    assert.equal(await page.evaluate(() => localStorage.getItem('unrelated')), 'excluded');
    backup.exportedAt = '2026-10-17T00:00:00.000Z';
    fs.writeFileSync(path.join(__dirname, '../native/tests/fixtures/legacy-profile.json'), JSON.stringify(backup, null, 2) + '\n');
    await page.evaluate(async () => {
      await new Promise((resolve, reject) => {
        const req = indexedDB.open('dt-music'); req.onsuccess = () => {
          const db = req.result, tx = db.transaction('tracks', 'readwrite');
          tx.objectStore('tracks').put({ id: 'broken', blob: 'not a blob' });
          tx.oncomplete = () => { db.close(); resolve(); }; tx.onerror = () => reject(tx.error);
        };
      });
    });
    await assert.rejects(() => page.evaluate(() => DT.ProfileBackup.snapshot()), /unreadable/);
    // Also exercise the compiled game and its real Settings page, in a separate temporary origin.
    const game = await browser.newPage({ acceptDownloads: true });
    await game.goto(require('url').pathToFileURL(path.resolve(__dirname, '../index.html')).href);
    await game.click('#cnSettings');
    await game.waitForSelector('#settingsBody [data-profile-backup]');
    const gameDownload = game.waitForEvent('download');
    await game.click('#settingsBody [data-profile-backup]');
    const gameFile = await gameDownload;
    const gameBackup = JSON.parse(fs.readFileSync(await gameFile.path(), 'utf8'));
    assert.equal(gameBackup.format, 'digitactics-profile'); assert.equal(gameBackup.version, 1);
    assert.equal(await game.locator('#settingsBody [data-profile-backup]').textContent(), 'Backup downloaded');
    console.log('Profile backup checks passed: empty profile, no database creation, raw storage, binary music, unknown metadata, download and corrupt-file rejection.');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
