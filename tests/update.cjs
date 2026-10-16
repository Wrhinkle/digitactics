// Self-updater test for the Steam desktop build (desktop-steam/boot.js + updater.js), against a fake GitHub API on
// 127.0.0.1:8189 (DT_UPDATE_API) and a throwaway user-data folder (DT_USERDATA), so neither GitHub nor the real
// %APPDATA%\Digi-Tactics is touched. The fake API serves releases/latest (with ETag / 304), the asset API URL and the
// browser download URL, both redirecting to a "storage" path like GitHub does.
// The app runs without a window in DT_UPDATE_TEST mode: it notes events in a JSON-lines file, restarts as soon as an
// update is ready and quits when the check has nothing more to do.
//   offline       nothing listening / unroutable host / rate limited / private repo without token: skips quietly
//   corrupt       wrong sha256, then wrong size: rejected, nothing staged
//   update        anonymous public download, verification, swap, restart and ETag; also private QA with a personal
//                 environment token, which goes to the API but never to the storage host
//   crash         newer payload whose main.js throws: rolled back to the previous version, marked bad, not retried
//   hang          newer payload whose page never comes up: rolled back by the trial watchdog
// Run: node tests/update.cjs [path-to-Digi-Tactics.exe | dev] [--publish]   (default dev: electron . in desktop-steam;
// run npm run stage first). --publish (with an exe only: it restages desktop-steam/payload) makes the update with
// publish-update.cjs --target, the real publish path.
const { spawn, spawnSync } = require('child_process');
const crypto = require('crypto');
const fs = require('fs');
const http = require('http');
const os = require('os');
const path = require('path');
const DS = path.join(__dirname, '..', 'desktop-steam');
const { zipDir } = require(path.join(DS, 'ziplib.js'));

const target = process.argv.slice(2).find((a) => !a.startsWith('--')) || 'dev';
const VIA_PUBLISH = process.argv.includes('--publish');
const BUNDLED = target === 'dev' ? path.join(DS, 'payload') : path.join(path.dirname(target), 'resources', 'payload');
const BASE_VER = JSON.parse(fs.readFileSync(path.join(BUNDLED, 'version.json'), 'utf8')).version;
const WORK = path.join(os.tmpdir(), `dt-update-test-${Date.now()}`);
const SRV = path.join(WORK, 'srv');
fs.mkdirSync(SRV, { recursive: true });
const PORT = 8189;
const API = `http://127.0.0.1:${PORT}`;
const results = [];
const check = (name, ok, detail) => { results.push({ name, ok: !!ok }); console.log(`${ok ? 'PASS' : 'FAIL'} ${name}${detail ? `  (${detail})` : ''}`); };

// ---- fake GitHub ----
let requests = [];
let mode = 'ok'; // ok | ratelimit | private
const server = http.createServer((req, res) => {
  const u = new URL(req.url, API);
  const auth = req.headers.authorization || '';
  const rec = { path: u.pathname, auth, status: 200 };
  requests.push(rec);
  const send = (status, hdr, body) => { rec.status = status; res.writeHead(status, hdr); res.end(body); };
  if (/^\/repos\/[^/]+\/[^/]+\/releases\/latest$/.test(u.pathname)) {
    if (mode === 'ratelimit') return send(403, { 'x-ratelimit-remaining': '0' }, '{"message":"API rate limit exceeded"}');
    if (mode === 'private' && !auth) return send(404, {}, '{"message":"Not Found"}');
    const f = path.join(SRV, 'release.json');
    if (!fs.existsSync(f)) return send(404, {}, '{"message":"Not Found"}');
    const raw = fs.readFileSync(f);
    const etag = `"${crypto.createHash('sha1').update(raw).digest('hex')}"`;
    if (req.headers['if-none-match'] === etag) return send(304, { etag }, '');
    const r = JSON.parse(raw);
    const body = JSON.stringify({
      tag_name: r.tag_name, name: r.name, body: r.body, html_url: `https://github.com/test/test/releases/tag/${r.tag_name}`,
      assets: r.assets.map((n, i) => ({ id: i + 1, name: n, size: fs.statSync(path.join(SRV, n)).size, url: `${API}/repos/o/r/releases/assets/${encodeURIComponent(n)}`, browser_download_url: `${API}/download/${encodeURIComponent(n)}` })),
    });
    return send(200, { 'content-type': 'application/json', etag }, body);
  }
  let m = u.pathname.match(/^\/repos\/[^/]+\/[^/]+\/releases\/assets\/(.+)$/) || u.pathname.match(/^\/download\/(.+)$/);
  if (m) {
    if (u.pathname.startsWith('/repos/') && req.headers.accept !== 'application/octet-stream') return send(415, {}, 'want octet-stream');
    return send(302, { location: `http://localhost:${PORT}/storage/${m[1]}?sig=x` }, ''); // another origin, like objects.githubusercontent.com
  }
  m = u.pathname.match(/^\/storage\/(.+)$/);
  if (m) {
    const f = path.join(SRV, decodeURIComponent(m[1]));
    if (!f.startsWith(SRV) || !fs.existsSync(f)) return send(404, {}, '');
    rec.status = 200;
    res.writeHead(200, { 'content-length': fs.statSync(f).size, 'content-type': 'application/octet-stream' });
    fs.createReadStream(f).pipe(res);
    return;
  }
  send(404, {}, '');
});

const sha = (f) => crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function makePayload(version, kind) {
  const zip = path.join(WORK, `payload-${version}.zip`);
  const override = { 'version.json': Buffer.from(JSON.stringify({ version, minShell: 1, date: new Date().toISOString() })) };
  let skip = null;
  if (kind === 'crash') {
    override['main.js'] = Buffer.from("throw new Error('test: this payload fails to start');\n");
    skip = (f) => f.startsWith('game/') && f !== 'game/index.html'; // small: only what the updater checks
  }
  if (kind === 'hang') {
    override['game/index.html'] = Buffer.from('<!doctype html><title>broken</title><p>no game here</p>');
    skip = (f) => f.startsWith('game/') && f !== 'game/index.html';
  }
  await zipDir(BUNDLED, zip, { override, skip });
  return zip;
}
// a release: latest.json with the good zip's size and sha256; serveAs swaps in a damaged file under the same name
function release(zip, version, { serveAs } = {}) {
  for (const f of fs.readdirSync(SRV)) fs.rmSync(path.join(SRV, f));
  const file = `payload-${version}.zip`;
  fs.copyFileSync(serveAs || zip, path.join(SRV, file));
  fs.writeFileSync(path.join(SRV, 'latest.json'), JSON.stringify({ version, minShell: 1, file, size: fs.statSync(zip).size, sha256: sha(zip), notes: `test ${version}`, date: new Date().toISOString() }));
  fs.writeFileSync(path.join(SRV, 'release.json'), JSON.stringify({ tag_name: `v${version}`, name: `Digi-Tactics ${version}`, body: '', assets: [file, 'latest.json'] }));
}

// launches the app and waits until `until(events)` is true (events from every process of the run, including restarts)
function launch(ud, api, until, { timeout = 360000, env = {} } = {}) {
  const notes = path.join(ud, 'test-events.jsonl');
  fs.mkdirSync(ud, { recursive: true });
  fs.rmSync(notes, { force: true });
  const [cmd, argv, cwd] = target === 'dev' ? [require(path.join(DS, 'node_modules', 'electron')), ['.'], DS] : [target, [], path.dirname(target)];
  const child = spawn(cmd, argv, { cwd, env: { ...process.env, DT_USERDATA: ud, DT_UPDATE_API: api, DT_UPDATE_TEST: notes, DT_UPDATE_TOKEN: 'none', ...env }, windowsHide: true, stdio: 'ignore' });
  child.on('error', (e) => console.error(e));
  const t0 = Date.now();
  return new Promise((resolve) => {
    const tick = setInterval(() => {
      let ev = [];
      try { ev = fs.readFileSync(notes, 'utf8').trim().split('\n').filter(Boolean).map((l) => JSON.parse(l)); } catch (e) { /* not yet */ }
      const done = until(ev);
      if (done || Date.now() - t0 > timeout) {
        clearInterval(tick);
        setTimeout(() => resolve({ ev, timedOut: !done, ms: Date.now() - t0 }), 4000); // let the last process quit
      }
    }, 500);
  });
}
const dones = (ev) => ev.filter((e) => e.event === 'done');
const starts = (ev) => ev.filter((e) => e.event === 'start');
const boots = (ev) => ev.filter((e) => e.event === 'boot');
const pv = (ud) => { try { return JSON.parse(fs.readFileSync(path.join(ud, 'payload', 'app', 'version.json'), 'utf8')).version; } catch (e) { return null; } };
const exists = (ud, rel) => fs.existsSync(path.join(ud, 'payload', rel));
const badOf = (ud) => { try { return JSON.parse(fs.readFileSync(path.join(ud, 'payload', 'bad.json'), 'utf8')).versions; } catch (e) { return []; } };
const tail = (ud) => { try { return fs.readFileSync(path.join(ud, 'main.log'), 'utf8').trim().split('\n').slice(-6).join('\n'); } catch (e) { return ''; } };
const phase0 = (r) => (dones(r.ev)[0] || {}).phase;

async function main() {
  await new Promise((r) => server.listen(PORT, '127.0.0.1', r));
  console.log(`target ${target}, bundled payload ${BASE_VER}, work ${WORK}`);
  const packagedConfig = JSON.parse(fs.readFileSync(path.join(BUNDLED, 'update-config.json'), 'utf8'));
  check('package: update config contains only owner and repo', Object.keys(packagedConfig).sort().join(',') === 'owner,repo');
  const v1 = '9.0.1';
  const good = await makePayload(v1, 'good');

  // ---- offline and refusals ----
  let ud = path.join(WORK, 'ud-offline');
  let r = await launch(ud, 'http://127.0.0.1:8191', (ev) => dones(ev).length >= 1, { timeout: 120000 });
  check('offline (refused): skips and keeps the bundled version', !r.timedOut && phase0(r) === 'offline' && starts(r.ev)[0].version === BASE_VER && !exists(ud, 'staging'), `phase ${phase0(r)}`);
  r = await launch(ud, 'http://10.255.255.1:81', (ev) => dones(ev).length >= 1, { timeout: 120000 });
  const s0 = starts(r.ev)[0];
  const d0 = dones(r.ev)[0];
  check('offline (unroutable): gives up after the 5 s timeout', !r.timedOut && d0.phase === 'offline' && d0.t - s0.t < 8000, d0 && s0 ? `${d0.t - s0.t} ms after the page came up` : 'no events');
  release(good, v1);
  mode = 'ratelimit';
  r = await launch(ud, API, (ev) => dones(ev).length >= 1, { timeout: 120000 });
  check('rate limited (403): skips quietly', !r.timedOut && phase0(r) === 'offline' && !exists(ud, 'staging'), `phase ${phase0(r)}`);
  mode = 'private';
  r = await launch(ud, API, (ev) => dones(ev).length >= 1, { timeout: 120000 });
  check('private repo without a token (404): skips quietly', !r.timedOut && phase0(r) === 'offline' && !exists(ud, 'staging'), `phase ${phase0(r)}`);
  mode = 'ok';

  // ---- corrupt (public repo, no token: browser download URL) ----
  const bad = path.join(WORK, 'corrupt.zip');
  const buf = fs.readFileSync(good);
  buf[Math.floor(buf.length / 2)] ^= 0xff;
  fs.writeFileSync(bad, buf);
  release(good, v1, { serveAs: bad });
  ud = path.join(WORK, 'ud-main');
  requests = [];
  r = await launch(ud, API, (ev) => dones(ev).length >= 1);
  const e1 = r.ev.find((e) => e.event === 'error');
  check('corrupt zip (wrong sha256): rejected, nothing staged', !r.timedOut && e1 && /sha256/.test(e1.error) && !exists(ud, 'staging') && !exists(ud, 'app') && starts(r.ev).length === 1, e1 && e1.error);
  check('no token: assets come through the browser download URL', requests.some((q) => q.path.startsWith('/download/')) && !requests.some((q) => q.path.includes('/releases/assets/')));
  const short = path.join(WORK, 'short.zip');
  fs.writeFileSync(short, fs.readFileSync(good).subarray(0, 100000));
  release(good, v1, { serveAs: short });
  r = await launch(ud, API, (ev) => dones(ev).length >= 1);
  const e2 = r.ev.find((e) => e.event === 'error');
  check('truncated zip (wrong size): rejected, nothing staged', !r.timedOut && e2 && /size/.test(e2.error) && !exists(ud, 'staging'), e2 && e2.error);
  check('no leftover downloads after rejections', !fs.existsSync(path.join(ud, 'updates')) || fs.readdirSync(path.join(ud, 'updates')).length === 0);

  // ---- update (public repo, no token or local credentials required) ----
  if (VIA_PUBLISH) {
    for (const f of fs.readdirSync(SRV)) fs.rmSync(path.join(SRV, f));
    const p = spawnSync(process.execPath, [path.join(DS, 'publish-update.cjs'), '--no-bundle', '--version', v1, '--notes', 'test update', '--target', SRV], { stdio: 'inherit', windowsHide: true });
    check('publish-update.cjs --target writes the release (latest.json, payload zip)', p.status === 0 && fs.existsSync(path.join(SRV, 'release.json')) && fs.existsSync(path.join(SRV, `payload-${v1}.zip`)));
  } else release(good, v1);
  mode = 'ok';
  requests = [];
  const ANONYMOUS = { DT_UPDATE_TOKEN: '' };
  r = await launch(ud, API, (ev) => dones(ev).length >= 2, { env: ANONYMOUS });
  const st = starts(r.ev);
  check('update: downloads, verifies and becomes ready', r.ev.some((e) => e.event === 'ready') && phase0(r) === 'ready', `events ${r.ev.map((e) => e.event).join(',')}`);
  check('update: restart swaps in the new payload, which starts and confirms', !r.timedOut && st.length === 2 && st[0].version === BASE_VER && st[1].version === v1 && st[1].trial === true && st[1].healthy === true && dones(r.ev)[1].phase === 'current',
    st.map((s) => `${s.version}${s.trial ? ' trial' : ''}${s.healthy ? ' ok' : ''}`).join(' -> '));
  check('update: payload\\app is the new version, trial marker gone', pv(ud) === v1 && !exists(ud, 'trial.json') && !exists(ud, 'staging'), `app ${pv(ud)}`);
  check('anonymous update: no Authorization header on any request', requests.length > 0 && requests.every((q) => !q.auth));
  check('anonymous update: uses public asset URLs', requests.some((q) => q.path.startsWith('/download/')) && !requests.some((q) => q.path.includes('/releases/assets/')));
  check('ETag: the check after the restart is a 304', requests.some((q) => q.path.endsWith('/releases/latest') && q.status === 304));
  requests = [];
  r = await launch(ud, API, (ev) => dones(ev).length >= 1, { env: ANONYMOUS });
  check('update: next plain start runs the new version, app.old cleaned up', starts(r.ev)[0].version === v1 && !starts(r.ev)[0].trial && !exists(ud, 'app.old'));
  check('one release check per launch', requests.filter((q) => q.path.endsWith('/releases/latest')).length === 1);

  // Private QA credentials belong to the tester's environment, never the downloadable package.
  mode = 'private';
  requests = [];
  const TOKEN = { DT_UPDATE_TOKEN: 'test-token-123' };
  const privateUd = path.join(WORK, 'ud-private');
  r = await launch(privateUd, API, (ev) => dones(ev).length >= 2, { env: TOKEN });
  check('private QA: personal environment credential permits update and restart', !r.timedOut && pv(privateUd) === v1 && starts(r.ev).some((s) => s.version === v1 && s.healthy));
  const apiReq = requests.filter((q) => q.path.startsWith('/repos/'));
  const storeReq = requests.filter((q) => q.path.startsWith('/storage/'));
  check('private QA: token sent to API, never to storage redirect', apiReq.length > 0 && apiReq.every((q) => q.auth === 'Bearer test-token-123') && storeReq.length > 0 && storeReq.every((q) => !q.auth));
  check('private QA: credential not written to installed config or release cache', !fs.readFileSync(path.join(privateUd, 'payload/app/update-config.json'), 'utf8').includes(TOKEN.DT_UPDATE_TOKEN) && !fs.readFileSync(path.join(privateUd, 'update-cache.json'), 'utf8').includes(TOKEN.DT_UPDATE_TOKEN));
  mode = 'ok';

  // ---- crash: new main.js throws ----
  const v2 = '9.0.2';
  release(await makePayload(v2, 'crash'), v2);
  r = await launch(ud, API, (ev) => dones(ev).length >= 2, { env: ANONYMOUS });
  check('crash: rolled back to the previous version and restarted', !r.timedOut && boots(r.ev).map((s) => s.version).join(',') === `${v1},${v1}` && pv(ud) === v1 && !exists(ud, 'trial.json'),
    `boots ${boots(r.ev).map((s) => s.version).join(',')}, app ${pv(ud)}`);
  check('crash: the bad version is not downloaded again', badOf(ud).includes(v2) && dones(r.ev)[1] && dones(r.ev)[1].phase === 'current', `bad ${badOf(ud)}`);
  if (r.timedOut) console.log(tail(ud));

  // ---- hang: new page never comes up, the trial watchdog rolls back ----
  const v3 = '9.0.3';
  release(await makePayload(v3, 'hang'), v3);
  r = await launch(ud, API, (ev) => dones(ev).length >= 2, { env: { ...ANONYMOUS, DT_TRIAL_TIMEOUT_MS: '20000' } });
  const hb = boots(r.ev);
  check('hang: watchdog rolls back after the trial timeout', !r.timedOut && hb.map((b) => b.version).join(',') === `${v1},${v3},${v1}` && hb[1].trial && starts(r.ev).every((x) => x.version === v1) && pv(ud) === v1 && badOf(ud).includes(v3),
    `boots ${hb.map((b) => `${b.version}${b.trial ? ' (trial)' : ''}`).join(' -> ')}`);
  if (r.timedOut) console.log(tail(ud));

  server.close();
  const failed = results.filter((x) => !x.ok).length;
  console.log(`\n${results.length - failed}/${results.length} passed`);
  if (!failed) fs.rmSync(WORK, { recursive: true, force: true });
  process.exit(failed ? 1 : 0);
}
main().catch((e) => { console.error(e); process.exit(1); });
