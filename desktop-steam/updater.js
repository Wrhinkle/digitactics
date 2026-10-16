// Self-updater for the Steam desktop build (runs in the main process, part of the payload). Updates come from GitHub
// Releases: each release (tag v<version>) carries payload-<version>.zip and a small manifest asset latest.json =
// { version, minShell, file, size, sha256, notes, date }.
//   1. GET <api>/repos/<owner>/<repo>/releases/latest (5 s timeout, once per launch, ETag cached so an unchanged
//      release costs a 304; offline, unreachable or rate-limited = skip quietly)
//   2. newer: fetch latest.json, then download the payload zip into %APPDATA%\Digi-Tactics\updates with progress
//   3. check size and sha256   4. extract to payload\staging (via staging.tmp)   5. tell the page "ready"
// boot.js swaps staging in on the next start; Restart in the page does that right away.
// Config: update-config.json next to this file { owner, repo }. Public releases require no credentials.
// Private QA can explicitly supply a personal DT_UPDATE_TOKEN in the process environment; it is never packaged
// or persisted. Legacy cfg.token is deliberately ignored. Authorization never follows a cross-origin redirect.
// Test overrides: DT_UPDATE_API (API base), DT_UPDATE_TOKEN ('none' = anonymous).
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { unzip } = require('./ziplib');

const CHECK_TIMEOUT_MS = 5000;
const STALL_MS = 30000;

function createUpdater({ boot, log, emit }) {
  const D = boot.dirs;
  const updatesDir = path.join(boot.userData, 'updates');
  const cacheFile = path.join(boot.userData, 'update-cache.json');
  const cfg = (() => { try { return JSON.parse(fs.readFileSync(path.join(__dirname, 'update-config.json'), 'utf8')); } catch (e) { return {}; } })();
  const api = (process.env.DT_UPDATE_API || cfg.api || 'https://api.github.com').replace(/\/+$/, '');
  const envToken = process.env.DT_UPDATE_TOKEN;
  const token = envToken === 'none' ? '' : envToken || '';
  const repoOk = /^[\w.-]+$/.test(cfg.owner || '') && /^[\w.-]+$/.test(cfg.repo || '');
  const releaseUrl = `${api}/repos/${cfg.owner}/${cfg.repo}/releases/latest`;
  let state = { phase: 'idle', version: boot.version, shell: boot.shell };
  let running = null;
  let checked = false;
  const set = (patch, event) => {
    state = { ...state, ...patch };
    if (event) emit(event, state);
  };
  const headers = (extra) => ({ 'User-Agent': 'Digi-Tactics-updater', 'X-GitHub-Api-Version': '2022-11-28', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...extra });
  const readCache = () => { try { return JSON.parse(fs.readFileSync(cacheFile, 'utf8')); } catch (e) { return null; } };

  async function withTimeout(ms, fn) {
    const ctl = new AbortController();
    const t = setTimeout(() => ctl.abort(), ms);
    try { return await fn(ctl.signal); } finally { clearTimeout(t); }
  }

  // the latest release, through the ETag cache
  async function fetchRelease() {
    const cache = readCache();
    return withTimeout(CHECK_TIMEOUT_MS, async (signal) => {
      const h = headers({ Accept: 'application/vnd.github+json' });
      if (cache && cache.etag && cache.url === releaseUrl && cache.release) h['If-None-Match'] = cache.etag;
      const res = await fetch(releaseUrl, { headers: h, signal });
      if (res.status === 304) return cache.release;
      if (res.status === 404) throw new Error(token ? 'no release found (or the token cannot read the repo)' : 'no release found (a private repo needs a token)');
      if (res.status === 403 || res.status === 429) throw new Error(`GitHub refused the check (HTTP ${res.status}, rate limit ${res.headers.get('x-ratelimit-remaining') || '?'} left)`);
      if (!res.ok) throw new Error(`release HTTP ${res.status}`);
      const r = await res.json();
      const release = {
        tag: String(r.tag_name || ''),
        page: String(r.html_url || ''),
        assets: (Array.isArray(r.assets) ? r.assets : []).map((a) => ({ name: String(a.name), url: String(a.url), download: String(a.browser_download_url || ''), size: Number(a.size) || 0 })),
      };
      try { fs.writeFileSync(cacheFile, JSON.stringify({ url: releaseUrl, etag: res.headers.get('etag') || '', release })); } catch (e) { /* ignore */ }
      return release;
    });
  }

  // GET an asset; with a token through the API (redirects followed by hand so the token stays on the API host)
  async function getAsset(asset, signal) {
    let url = token ? asset.url : (asset.download || asset.url);
    let h = headers({ Accept: 'application/octet-stream' });
    for (let hop = 0; hop < 6; hop++) {
      const res = await fetch(url, { headers: h, signal, redirect: 'manual' });
      if (res.status >= 300 && res.status < 400 && res.headers.get('location')) {
        const next = new URL(res.headers.get('location'), url);
        if (next.origin !== new URL(url).origin) h = { 'User-Agent': h['User-Agent'], Accept: 'application/octet-stream' };
        url = next.toString();
        try { await res.body?.cancel(); } catch (e) { /* ignore */ }
        continue;
      }
      if (!res.ok || !res.body) throw new Error(`asset ${asset.name}: HTTP ${res.status}`);
      return res;
    }
    throw new Error(`asset ${asset.name}: too many redirects`);
  }

  async function fetchManifest(release) {
    const a = release.assets.find((x) => x.name === 'latest.json');
    if (!a) throw new Error(`release ${release.tag} has no latest.json`);
    const m = await withTimeout(15000, async (signal) => (await getAsset(a, signal)).json());
    if (!m || typeof m.version !== 'string' || !/^[0-9a-f]{64}$/i.test(String(m.sha256)) || !(Number(m.size) > 0)) throw new Error('latest.json is missing fields');
    if (release.tag.replace(/^v/, '') !== m.version) throw new Error(`latest.json says ${m.version} but the release is ${release.tag}`);
    const file = String(m.file || `payload-${m.version}.zip`);
    const asset = release.assets.find((x) => x.name === file);
    if (!asset) throw new Error(`release ${release.tag} has no ${file}`);
    return { ...m, asset, size: Number(m.size), minShell: Number(m.minShell) || 1, sha256: m.sha256.toLowerCase(), page: release.page };
  }

  async function download(m, file) {
    const part = `${file}.part`;
    const ctl = new AbortController();
    let stall = setTimeout(() => ctl.abort(), STALL_MS);
    let res;
    try { res = await getAsset(m.asset, ctl.signal); } catch (e) { clearTimeout(stall); throw e; }
    const hash = crypto.createHash('sha256');
    const out = fs.createWriteStream(part);
    let got = 0;
    let lastEmit = 0;
    try {
      const reader = res.body.getReader();
      for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        clearTimeout(stall);
        stall = setTimeout(() => ctl.abort(), STALL_MS);
        got += value.length;
        if (got > m.size) throw new Error(`download is larger than the manifest says (${m.size} bytes)`);
        hash.update(value);
        if (!out.write(value)) await new Promise((r) => out.once('drain', r));
        if (Date.now() - lastEmit > 250) { lastEmit = Date.now(); set({ received: got, total: m.size }, 'progress'); }
      }
    } finally {
      clearTimeout(stall);
      await new Promise((r) => out.end(r));
    }
    set({ received: got, total: m.size }, 'progress');
    const sum = hash.digest('hex');
    if (got !== m.size) { fs.rmSync(part, { force: true }); throw new Error(`size mismatch: got ${got}, expected ${m.size}`); }
    if (sum !== m.sha256) { fs.rmSync(part, { force: true }); throw new Error(`sha256 mismatch: got ${sum}`); }
    fs.renameSync(part, file);
  }

  async function run() {
    if (!boot.updatesOn || !repoOk) {
      log(`update: off (${!boot.updatesOn ? 'running from source' : 'no GitHub repo in update-config.json'})`);
      set({ phase: 'off' }, 'off');
      return state;
    }
    log(`update: checking ${cfg.owner}/${cfg.repo} (have ${boot.version}, shell ${boot.shell}, ${token ? 'with' : 'without'} token)`);
    set({ phase: 'checking' }, 'checking');
    let m;
    let release;
    try {
      release = await fetchRelease();
      if (boot.cmpVer(release.tag.replace(/^v/, ''), boot.version) <= 0) {
        log(`update: up to date (latest release ${release.tag || '?'})`);
        set({ phase: 'current', latest: release.tag.replace(/^v/, ''), page: release.page }, 'current');
        return state;
      }
      log(`update: ${release.tag} is available`);
      m = await fetchManifest(release);
    } catch (e) {
      log(`update: check skipped (${e.name === 'AbortError' ? 'timeout' : e.message})`);
      set({ phase: 'offline' }, 'offline');
      return state;
    }
    const info = { latest: m.version, notes: String(m.notes || ''), date: String(m.date || ''), page: m.page };
    if (boot.badList().includes(m.version)) { log(`update: ${m.version} failed to start before, not retrying`); set({ phase: 'current', ...info }, 'current'); return state; }
    if (m.minShell > boot.shell) {
      log(`update: ${m.version} needs shell ${m.minShell} (have ${boot.shell})`);
      set({ phase: 'shell', ...info }, 'shell');
      return state;
    }
    // already downloaded and extracted by an earlier run
    try {
      const sv = JSON.parse(fs.readFileSync(path.join(D.staging, 'version.json'), 'utf8'));
      if (sv.version === m.version) { set({ phase: 'ready', ...info }, 'ready'); return state; }
    } catch (e) { /* no staging */ }
    set({ phase: 'downloading', received: 0, total: m.size, ...info }, 'available');
    const file = path.join(updatesDir, `payload-${m.version.replace(/[^\w.-]/g, '_')}.zip`);
    const tmp = `${D.staging}.tmp`;
    try {
      fs.mkdirSync(updatesDir, { recursive: true });
      for (const f of fs.readdirSync(updatesDir)) if (path.join(updatesDir, f) !== file) fs.rmSync(path.join(updatesDir, f), { recursive: true, force: true });
      let have = false;
      if (fs.existsSync(file) && fs.statSync(file).size === m.size) {
        const sum = crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
        have = sum === m.sha256;
      }
      if (!have) { fs.rmSync(file, { force: true }); log(`update: downloading ${m.version} (${m.size} bytes) from ${release.tag}`); await download(m, file); }
      set({ phase: 'extracting' }, 'extracting');
      fs.rmSync(tmp, { recursive: true, force: true });
      const r = await unzip(file, tmp);
      const sv = JSON.parse(fs.readFileSync(path.join(tmp, 'version.json'), 'utf8'));
      if (sv.version !== m.version || !fs.existsSync(path.join(tmp, 'main.js')) || !fs.existsSync(path.join(tmp, 'game', 'index.html'))) throw new Error(`payload contents do not match ${m.version}`);
      fs.rmSync(D.staging, { recursive: true, force: true });
      fs.renameSync(tmp, D.staging);
      fs.rmSync(file, { force: true });
      log(`update: ${m.version} ready (${r.files} files), applies on restart`);
      set({ phase: 'ready' }, 'ready');
    } catch (e) {
      log(`update: ${m.version} rejected: ${e.message}`);
      try { fs.rmSync(tmp, { recursive: true, force: true }); } catch (e2) { /* ignore */ }
      try { fs.rmSync(file, { force: true }); fs.rmSync(`${file}.part`, { force: true }); } catch (e2) { /* ignore */ }
      set({ phase: 'error', error: e.message }, 'error');
    }
    return state;
  }

  return {
    // at most one check per launch (GitHub allows 60 unauthenticated API calls per hour per IP)
    check: () => {
      if (checked && !running) return Promise.resolve(state);
      checked = true;
      return (running = running || run().finally(() => { running = null; }));
    },
    state: () => state,
  };
}

module.exports = { createUpdater };
