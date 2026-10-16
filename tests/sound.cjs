// Sound check: music tracks follow the phases, the soundboard panel plays clips with its rate limit,
// remote clips and the mute setting work, and the own-music option saves and plays a local file.
// Run: node tests/sound.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8141;

// a 1.5 s 440 Hz WAV file to stand in for the player's own music
function wav() {
  const rate = 22050, n = Math.floor(rate * 1.5);
  const b = Buffer.alloc(44 + n * 2);
  b.write('RIFF', 0); b.writeUInt32LE(36 + n * 2, 4); b.write('WAVE', 8); b.write('fmt ', 12);
  b.writeUInt32LE(16, 16); b.writeUInt16LE(1, 20); b.writeUInt16LE(1, 22); b.writeUInt32LE(rate, 24); b.writeUInt32LE(rate * 2, 28);
  b.writeUInt16LE(2, 32); b.writeUInt16LE(16, 34); b.write('data', 36); b.writeUInt32LE(n * 2, 40);
  for (let i = 0; i < n; i++) b.writeInt16LE(Math.round(Math.sin((2 * Math.PI * 440 * i) / rate) * 8000), 44 + i * 2);
  return b;
}

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  const args = ['--autoplay-policy=no-user-gesture-required', '--use-gl=angle', '--use-angle=swiftshader'];
  let browser;
  try { browser = await chromium.launch({ args }); } catch { browser = await chromium.launch({ channel: 'msedge', args }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' || m.type() === 'warning') errors.push(m.text()); });
  page.on('requestfailed', (r) => errors.push(`request failed: ${r.url()}`));
  const checks = [];
  const check = (name, ok, info) => { checks.push({ name, ok: !!ok }); console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}${info !== undefined ? ` ${JSON.stringify(info)}` : ''}`); };
  const ev = (fn, arg) => page.evaluate(fn, arg);
  const music = () => ev(() => window.DT.Music.state());

  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2000);
  await ev(() => window.DT.Music.own.clear());
  await page.click('#startBtn');
  await page.waitForTimeout(400);
  await page.click('#timerBtn');
  await page.locator('.choice[data-carousel]:not(.taken)').first().click();
  await page.click('#carouselGo');
  await page.waitForTimeout(1500);

  let m = await music();
  check('audio running', await ev(() => window.DT.Sfx.running));
  check('planning track plays', m.playing && m.source === 'builtin' && m.track === 'planning', m);
  for (let i = 0; i < 30 && (await music()).section === 'intro'; i++) await page.waitForTimeout(300);
  m = await music();
  check('planning reaches the verse after the 4-bar intro', m.section === 'verse', m.section);

  // soundboard panel
  await page.keyboard.press('g');
  check('G opens the soundboard', await page.locator('#sbPanel:not(.hidden)').count() === 1);
  check('6 pads, 4 free clips equipped', await page.locator('#sbPanel .sb-pad').count() === 6 && await page.locator('#sbPanel .sb-pad:not(.empty)').count() === 4);
  await page.screenshot({ path: path.join(out, 'soundboard.png') });
  // three presses back to back (a screenshot in between can take longer than the 5 s window here)
  await page.keyboard.press('1');
  await page.keyboard.press('2');
  await page.keyboard.press('3');
  check('key 1 plays a clip and shows a label', (await ev(() => window.DT.Sfx.stats['clip:digivice'])) === 1 && (await page.locator('.sb-label').count()) >= 1);
  let sb = await ev(() => ({ ...window.DT.Soundboard.stats }));
  check('rate limit: 2 per 5 s, the third is blocked', sb.sent === 2 && sb.blocked === 1, sb);
  // clips from other players
  const other = await ev(() => window.DT.UI.game().players.find((p) => p !== window.DT.UI.game().human).id);
  await ev((s) => { for (let i = 0; i < 3; i++) window.DT.Social.emit('sound', s, 'airhorn'); }, other);
  sb = await ev(() => ({ ...window.DT.Soundboard.stats }));
  check('remote clips are rate limited too', sb.played === 4 && sb.droppedRemote === 1, sb);
  await page.screenshot({ path: path.join(out, 'soundboard-labels.png') });
  await ev((s) => { window.DT.Sfx.set({ muteOthers: true }); window.DT.Social.emit('sound', s + 1, 'gong'); window.DT.Sfx.set({ muteOthers: false }); }, other);
  sb = await ev(() => ({ ...window.DT.Soundboard.stats }));
  check('mute other players works', sb.mutedRemote === 1 && sb.played === 4, sb);
  await page.keyboard.press('Escape');
  check('Escape closes the panel', await page.locator('#sbPanel.hidden').count() === 1);
  // every clip renders without throwing
  const clipErr = await ev(() => { try { for (const id of window.DT.Soundboard.clips) window.DT.Sfx.clip(id, true); return null; } catch (e) { return String(e); } });
  check('all clips synthesize', !clipErr, clipErr || undefined);
  check('18 sound cosmetics registered', (await ev(() => window.DT.Cosmetics.list('sound').length)) === 18);

  // sounds other features call
  const names = ['killscreen', 'emote', 'egg_wobble', 'egg_crack', 'egg_reveal', 'purchase', 'coins'];
  await ev((ns) => { for (const n of ns) window.DT.Sfx.play(n, n === 'killscreen' ? { variant: 'ks_mega', big: true } : n === 'egg_reveal' ? { rarity: 'legendary' } : { n: 60 }); }, names);
  const st = await ev(() => window.DT.Sfx.stats);
  check('cosmetic sounds play', names.every((n) => st[n] >= 1), names.map((n) => `${n}:${st[n] || 0}`).join(' '));

  // phase change crossfades to the combat track and back
  await page.waitForTimeout(5200); // let the rate limit window pass for later checks
  await page.keyboard.press('Space');
  await page.waitForTimeout(400);
  m = await music();
  const last = m.starts[m.starts.length - 1];
  check('combat track starts on Fight with a crossfade', m.track === 'combat' && last.track === 'combat' && last.crossfade, last);
  for (let i = 0; i < 20 && (await music()).players > 1; i++) await page.waitForTimeout(300);
  m = await music();
  check('old track is released after the fade', m.track === 'combat' && m.players === 1, m);
  if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
  await page.waitForTimeout(4000);
  m = await music();
  check('back to planning after the round', m.phase === 'planning' && m.track === 'planning', m);
  check('planning resumes at its next section', m.section !== 'intro', m.section);

  // settings popover and own music
  await page.click('#soundBtn');
  check('Sound button opens the settings', await page.locator('#sndPop:not(.hidden)').count() === 1);
  await page.locator('#spFile').setInputFiles({ name: 'test-tone.wav', mimeType: 'audio/wav', buffer: wav() });
  await page.waitForTimeout(1500);
  m = await music();
  const own = await ev(() => window.DT.Music.own.list.length);
  check('own file saved and playing instead of the built-in track', own === 1 && m.source === 'own' && m.track === 'test-tone', m);
  await page.screenshot({ path: path.join(out, 'settings.png') });
  const stored = await ev(() => new Promise((res) => {
    setTimeout(() => res('timeout'), 5000);
    const r = indexedDB.open('dt-music', 1);
    r.onerror = () => res(String(r.error));
    r.onsuccess = () => { const q = r.result.transaction('tracks').objectStore('tracks').count(); q.onsuccess = () => { res(q.result); r.result.close(); }; };
  }));
  check('file is in IndexedDB', stored === 1, stored);
  await page.waitForTimeout(2000);
  m = await music();
  check('own track loops after it ends', m.source === 'own' && m.playing, m);
  await page.fill('#spMusic', '30');
  await page.locator('#spMusic').dispatchEvent('input');
  check('music slider saves', (await ev(() => window.DT.Sfx.settings.music)) === 0.3);
  await page.click('#spList [data-rm]');
  await page.waitForTimeout(1500);
  m = await music();
  check('removing the file returns to the built-in track', m.source === 'builtin' && m.playing && (await ev(() => window.DT.Sfx.settings.source)) === 'builtin', m);
  await page.click('#spMute');
  await page.waitForTimeout(1300);
  m = await music();
  check('Mute all stops the music', !m.playing && (await page.textContent('#soundBtn')) === 'Muted', m);
  await page.click('#spMute');
  await page.waitForTimeout(500);
  check('unmute restarts it', (await music()).playing);
  await page.keyboard.press('Escape');

  console.log('sounds:', JSON.stringify(await ev(() => window.DT.Sfx.stats)));
  console.log('soundboard:', JSON.stringify(await ev(() => window.DT.Soundboard.stats)));
  const failed = checks.filter((c) => !c.ok).length;
  console.log(`${checks.length - failed}/${checks.length} checks passed`);
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  server.kill();
  await Promise.race([browser.close(), new Promise((r) => setTimeout(r, 10000))]);
  process.exit(failed || errors.length ? 1 : 0);
})();
