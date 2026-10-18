// Render the original synthesis offline. JavaScript is a build dependency only.
// node native/tools/export-audio.cjs [playwright-package] [output-directory]
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const { chromium } = require(process.argv[2] || '../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
const out = path.resolve(process.argv[3] || path.join(__dirname, '../artifacts/audio-content'));
const rate = 44100;
const source = Object.fromEntries(['audio', 'music', 'soundboard', 'elimsounds', 'mystery-content'].map(id => [id, fs.readFileSync(path.join(root, `js/${id}.js`), 'utf8')]));
const sourceHash = crypto.createHash('sha256').update(JSON.stringify(source)).update(fs.readFileSync(__filename)).digest('hex');
function wave(samples) {
  const bytes = Buffer.from(samples, 'base64');
  const result = Buffer.alloc(44 + bytes.length);
  result.write('RIFF'); result.writeUInt32LE(36 + bytes.length, 4); result.write('WAVEfmt ', 8);
  result.writeUInt32LE(16, 16); result.writeUInt16LE(1, 20); result.writeUInt16LE(2, 22);
  result.writeUInt32LE(rate, 24); result.writeUInt32LE(rate * 4, 28);
  result.writeUInt16LE(4, 32); result.writeUInt16LE(16, 34); result.write('data', 36); result.writeUInt32LE(bytes.length, 40);
  bytes.copy(result, 44); return result;
}
(async () => {
  fs.mkdirSync(out, { recursive: true });
  const manifestPath = path.join(out, 'manifest.json');
  if (fs.existsSync(manifestPath)) {
    const previous = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
    if (previous.sourceHash === sourceHash && Object.values(previous.clips).every(clip => {
      const file = path.join(out, clip.file);
      return fs.existsSync(file) && crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex') === clip.sha256;
    })) { console.log('Original audio assets verified; render cache is current.'); return; }
  }
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage();
    const manifest = { sampleRate: rate, channels: 2, sourceHash, clips: {} };
    // A fresh page per clip keeps AudioNodes and generated reverb buffers bounded.
    async function render(request, seconds = 5) {
      await page.goto('about:blank');
      const result = await page.evaluate(async ({ source, request, seconds, rate }) => {
        let rng = 143;
        Math.random = () => { rng = (Math.imul(rng, 1664525) + 1013904223) >>> 0; return rng / 4294967296; };
        window.DT = { Cosmetics: { get: (_, theme) => ({ data: { theme } }) } };
        const ctx = new OfflineAudioContext(2, Math.ceil(seconds * rate), rate);
        window.AudioContext = function () { return ctx; };
        new Function(source.audio + '\nDT.NativeAudio = { SOUNDS, KILL, CLIPS, GAP, start() { ensure(); master.gain.value = 0.4; sfxBus.gain.value = 1; musicBus.gain.value = 1; boardBus.gain.value = 1; } };')();
        new Function(source.soundboard.slice(0, source.soundboard.indexOf('DT.Cosmetics.register')))();
        new Function(source.elimsounds.slice(0, source.elimsounds.indexOf('const stats =')))();
        new Function(source['mystery-content'].replace(/export /g, '') +
          '\nfor (const kind of ["sound", "elimsound"]) for (const s of sounds(kind)) DT.Sfx.defineClip(s.id, (t, out) => s.synth(t, out, DT.Sfx.engine), s.data.dur);')();
        new Function(source.music.replace('own.load();', '') + '\nDT.NativeMusic = { TRACKS, schedule(id) { ac = E().ac; attach(); const tr = TRACKS[id]; const p = new Player(tr, 0, 0, 0); const end = tr.bars.length * 240 / tr.bpm; p.schedule(end - 1e-6); return { end, loop: tr.loopBar * 240 / tr.bpm }; } };')();
        if (request === null) return {
          sounds: Object.keys(DT.NativeAudio.SOUNDS), clips: Object.keys(DT.NativeAudio.CLIPS),
          clipDurations: Object.fromEntries(Object.entries(DT.NativeAudio.CLIPS).map(([id, clip]) => [id, clip.dur || 1])),
          themes: Object.keys(DT.NativeAudio.KILL), gaps: DT.NativeAudio.GAP,
          tracks: Object.fromEntries(Object.entries(DT.NativeMusic.TRACKS).map(([id, tr]) => [id, { duration: tr.bars.length * 240 / tr.bpm, name: tr.name }]))
        };
        DT.NativeAudio.start();
        let loop = null, end = null;
        if (request.kind === 'music') ({ loop, end } = DT.NativeMusic.schedule(request.id));
        else if (request.kind === 'clip') DT.NativeAudio.CLIPS[request.id].fn(0, DT.Sfx.engine.boardBus);
        else DT.NativeAudio.SOUNDS[request.id](0, request.options || {});
        const audio = await ctx.startRendering();
        let length = audio.length;
        const left = audio.getChannelData(0), right = audio.getChannelData(1);
        while (length > 1 && Math.abs(left[length - 1]) < 1 / 32768 && Math.abs(right[length - 1]) < 1 / 32768) length--;
        // Keep enough silent tail to avoid truncating an inaudible but still decaying envelope.
        length = Math.min(audio.length, length + 128);
        let peak = 0, energy = 0, clipped = 0;
        const pcm = new Uint8Array(length * 4), view = new DataView(pcm.buffer);
        for (let i = 0; i < length; i++) for (let c = 0; c < 2; c++) {
          const sample = c ? right[i] : left[i];
          if (!Number.isFinite(sample)) throw Error('Non-finite audio');
          peak = Math.max(peak, Math.abs(sample)); energy += sample * sample;
          if (Math.abs(sample) > 1) clipped++;
          view.setInt16(i * 4 + c * 2, Math.round(Math.max(-1, Math.min(1, sample)) * 32767), true);
        }
        let binary = '';
        for (let i = 0; i < pcm.length; i += 8192) binary += String.fromCharCode(...pcm.subarray(i, i + 8192));
        return { samples: btoa(binary), frames: length, peak, rms: Math.sqrt(energy / (length * 2)), clipped,
          loopFrame: loop === null ? null : Math.round(loop * rate), endFrame: end === null ? null : Math.round(end * rate) };
      }, { source, request, seconds, rate });
      return result;
    }
    const inventory = await render(null);
    const requests = inventory.sounds.filter(id => !['cast', 'tick', 'hit', 'killscreen', 'egg_reveal', 'coins'].includes(id))
      .map(id => ({ key: id, kind: 'sound', id }));
    for (const last of [false, true]) requests.push({ key: `tick_${last ? 'last' : 'normal'}`, id: 'tick', options: { last } });
    for (const big of [false, true]) requests.push({ key: `hit_${big ? 'big' : 'normal'}`, id: 'hit', options: { big } });
    for (const elem of ['default', 'fire', 'water', 'ice', 'thunder', 'plant', 'wind', 'holy', 'dark', 'metal', 'beast'])
      for (let stage = 0; stage < 3; stage++) requests.push({ key: `cast_${elem}_${stage}`, id: 'cast', options: { elem, stage } });
    for (const theme of inventory.themes) for (const big of [false, true])
      requests.push({ key: `killscreen_${theme}_${big ? 'big' : 'normal'}`, id: 'killscreen', options: { variant: theme, big } });
    for (const rarity of ['common', 'rare', 'epic', 'legendary']) requests.push({ key: `egg_reveal_${rarity}`, id: 'egg_reveal', options: { rarity } });
    for (let n = 2; n <= 7; n++) requests.push({ key: `coins_${n}`, id: 'coins', options: { n: n * 15 } });
    for (const id of inventory.clips) requests.push({ key: `clip_${id}`, kind: 'clip', id });
    for (const [id, track] of Object.entries(inventory.tracks)) requests.push({ key: `music_${id}`, kind: 'music', id, seconds: track.duration + 4 });
    for (const request of requests) {
      const rendered = await render(request, request.seconds || (request.id === 'champion' ? 15 : 5));
      const { samples, ...metadata } = rendered;
      if (metadata.rms < 0.00001 || metadata.clipped) throw Error(`Invalid audio ${request.key}: ${JSON.stringify(metadata)}`);
      const file = request.key + '.wav', buffer = wave(samples);
      fs.writeFileSync(path.join(out, file), buffer);
      manifest.clips[request.key] = { file, ...metadata, gap: inventory.gaps[request.id] || 0,
        duration: request.kind === 'clip' ? inventory.clipDurations[request.id] : null,
        sha256: crypto.createHash('sha256').update(buffer).digest('hex') };
      console.log(`${request.key}: ${(metadata.frames / rate).toFixed(2)}s`);
    }
    fs.writeFileSync(path.join(out, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
    console.log(`Rendered ${requests.length} original audio assets.`);
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });

