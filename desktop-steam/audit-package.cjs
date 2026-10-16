// Reject updater credentials in a staged payload or unpacked desktop distribution.
// Only report file names, never matching values. Run: node audit-package.cjs <directory>
const fs = require('fs');
const path = require('path');

function auditPackage(directory) {
  const root = path.resolve(directory);
  const payload = fs.existsSync(path.join(root, 'resources/payload')) ? path.join(root, 'resources/payload') : root;
  const config = JSON.parse(fs.readFileSync(path.join(payload, 'update-config.json'), 'utf8'));
  if (Object.keys(config).sort().join(',') !== 'owner,repo') throw new Error('Package update config must contain only owner and repo');
  const known = [];
  const oldToken = path.join(__dirname, 'update-token.txt');
  if (fs.existsSync(oldToken)) known.push(fs.readFileSync(oldToken, 'utf8').trim());
  const oldConfig = path.join(__dirname, 'update-config.json');
  if (fs.existsSync(oldConfig)) known.push(JSON.parse(fs.readFileSync(oldConfig, 'utf8')).token || '');
  const secrets = known.filter((s) => typeof s === 'string' && s.length >= 8).map((s) => Buffer.from(s));
  let count = 0;
  function walk(dir) {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const file = path.join(dir, entry.name);
      const rel = path.relative(root, file);
      if (entry.isSymbolicLink()) throw new Error(`Unexpected package symlink: ${rel}`);
      if (entry.isDirectory()) { walk(file); continue; }
      if (/^(?:update-token\.txt|\.env(?:\..*)?)$/i.test(entry.name)) throw new Error(`Credential file in package: ${rel}`);
      const bytes = fs.readFileSync(file);
      if (secrets.some((s) => bytes.includes(s)) || /\b(?:github_pat_[A-Za-z0-9_]{30,}|gh[pousr]_[A-Za-z0-9]{30,})\b/.test(bytes.toString('latin1'))) {
        throw new Error(`Possible credential in package: ${rel}`);
      }
      count++;
    }
  }
  walk(root);
  console.log(`Credential audit passed: ${count} files; repository-only update config`);
  return count;
}

if (require.main === module) {
  try { auditPackage(process.argv[2] || path.join(__dirname, 'payload')); }
  catch (e) { console.error(e.message); process.exitCode = 1; }
}
module.exports = { auditPackage };
