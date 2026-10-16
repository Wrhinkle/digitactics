// Searches the public Sketchfab API for a downloadable model per Digimon form.
// Run: node tests/find-models.cjs > models/candidates.json
const fs = require('fs'), vm = require('vm'), path = require('path');
const ctx = { console, Math }; ctx.globalThis = ctx; vm.createContext(ctx);
vm.runInContext(fs.readFileSync(path.join(__dirname, '..', 'js', 'data.js'), 'utf8'), ctx);
const DT = ctx.DT;
const norm = (s) => s.toLowerCase().replace(/[^a-z0-9]/g, '');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
(async () => {
  const out = {};
  for (const L of DT.LINES) {
    for (let s = 0; s < 3; s++) {
      const form = L.forms[s];
      const k = `${L.id}:${s}`;
      if (Object.values(out).some((o) => o.form === form)) { out[k] = { ...Object.values(out).find((o) => o.form === form) }; continue; }
      const q = encodeURIComponent(`${form.replace(/ (DM|FM|PM|SM|BM)$/, '')} digimon`);
      let res = [];
      try {
        const r = await fetch(`https://api.sketchfab.com/v3/search?type=models&q=${q}&downloadable=true&count=24`);
        res = (await r.json()).results || [];
      } catch (e) {}
      const base = norm(form.replace(/ (DM|FM|PM|SM|BM)$/, ''));
      if (/ (DM|FM|PM|SM|BM)$/.test(form) && s > 0) { out[k] = { ...out[`${L.id}:0`], form }; continue; }
      const strip = (n) => norm(n.toLowerCase()
        .replace(/digimon|links|linkz|mobile|cyber sleuth|story|re ?arise|new century|masters|world \d|animated|low ?poly|psx|dreamcast|style|rigged|model|3d|base|\(.*?\)|[a-z]+ line/g, ''));
      const BAD = /toy|black|lady|vs|cliff|forest|ruins|sculpt|the |shadowlord|paladin|mirage|bancho|clavis|heavy|pheonix|phoenix|terrior|silent|pyramid|cinder|ivy/i;
      const hits = res
        .filter((m) => norm(m.name).includes(base))
        .map((m) => ({ ...m, exact: strip(m.name) === base, bad: BAD.test(m.name) }))
        .filter((m) => !m.bad)
        .map((m) => ({ name: m.name, uid: m.uid, url: m.viewerUrl, license: m.license && m.license.label, faces: m.faceCount, animated: m.animationCount > 0, author: m.user && m.user.username, exact: m.exact }))
        .sort((a, b) => (a.faces > 80000) - (b.faces > 80000) || b.exact - a.exact || b.animated - a.animated || a.faces - b.faces);
      out[k] = { form, best: hits[0] || null, others: hits.slice(1, 4) };
      process.stderr.write(`${form.padEnd(22)} ${hits[0] ? `${hits[0].faces} faces ${hits[0].animated ? 'anim ' : ''}${hits[0].name}` : '-'}\n`);
      await sleep(400);
    }
  }
  console.log(JSON.stringify(out, null, 1));
})();
