// Minimal zip writer and reader (deflate or stored, no zip64) for the self-updater and publish-update.cjs.
// The payload is well under 4 GB and 65535 files. Uses only Node's zlib, so it runs in the Electron main process.
//   await zipDir(srcDir, outFile, { prefix, override, skip })   writes every file under srcDir (prefix: folder name
//     inside the zip; override: { 'rel/path': Buffer } replaces or adds files; skip(rel) leaves files out)
//   await unzip(zipFile, outDir)                 extracts, checking each entry's CRC-32 and size; rejects unsafe paths
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const { promisify } = require('util');
const deflateRaw = promisify(zlib.deflateRaw);
const inflateRaw = promisify(zlib.inflateRaw);

const crc32 = zlib.crc32 || ((buf) => {
  let c = ~0;
  for (let i = 0; i < buf.length; i++) { c ^= buf[i]; for (let k = 0; k < 8; k++) c = (c >>> 1) ^ (0xedb88320 & -(c & 1)); }
  return ~c >>> 0;
});
// already-compressed formats are stored, which is faster and no bigger
const STORE_EXT = new Set(['.glb', '.webp', '.png', '.jpg', '.jpeg', '.mp3', '.ogg', '.m4a', '.woff2', '.zip', '.gz']);

function listFiles(dir, rel = '') {
  const out = [];
  for (const e of fs.readdirSync(path.join(dir, rel), { withFileTypes: true })) {
    const r = rel ? `${rel}/${e.name}` : e.name;
    if (e.isDirectory()) out.push(...listFiles(dir, r)); else if (e.isFile()) out.push(r);
  }
  return out.sort();
}

function dosTime(d) {
  return {
    time: (d.getHours() << 11) | (d.getMinutes() << 5) | Math.floor(d.getSeconds() / 2),
    date: ((Math.max(1980, d.getFullYear()) - 1980) << 9) | ((d.getMonth() + 1) << 5) | d.getDate(),
  };
}

async function zipDir(srcDir, outFile, opts = {}) {
  const prefix = opts.prefix ? `${opts.prefix.replace(/\/+$/, '')}/` : '';
  const override = opts.override || {};
  const files = [...new Set(listFiles(srcDir).filter((f) => !(opts.skip && opts.skip(f))).concat(Object.keys(override)))].sort();
  if (files.length > 65000) throw new Error('too many files for a plain zip');
  const fd = fs.openSync(outFile, 'w');
  const central = [];
  let offset = 0;
  const write = (buf) => { fs.writeSync(fd, buf); offset += buf.length; };
  try {
    for (const rel of files) {
      const full = path.join(srcDir, rel);
      const data = override[rel] || fs.readFileSync(full);
      const crc = crc32(data);
      let method = 0;
      let body = data;
      if (!STORE_EXT.has(path.extname(rel).toLowerCase()) && data.length > 64) {
        const z = await deflateRaw(data, { level: 6 });
        if (z.length < data.length) { method = 8; body = z; }
      }
      if (offset + body.length > 0xfffffff0) throw new Error('zip larger than 4 GB');
      const name = Buffer.from(prefix + rel, 'utf8');
      const { time, date } = dosTime(override[rel] ? new Date() : fs.statSync(full).mtime);
      const local = Buffer.alloc(30);
      local.writeUInt32LE(0x04034b50, 0); local.writeUInt16LE(20, 4); local.writeUInt16LE(0x0800, 6); local.writeUInt16LE(method, 8);
      local.writeUInt16LE(time, 10); local.writeUInt16LE(date, 12); local.writeUInt32LE(crc, 14); local.writeUInt32LE(body.length, 18);
      local.writeUInt32LE(data.length, 22); local.writeUInt16LE(name.length, 26); local.writeUInt16LE(0, 28);
      const at = offset;
      write(local); write(name); write(body);
      const c = Buffer.alloc(46);
      c.writeUInt32LE(0x02014b50, 0); c.writeUInt16LE(20, 4); c.writeUInt16LE(20, 6); c.writeUInt16LE(0x0800, 8); c.writeUInt16LE(method, 10);
      c.writeUInt16LE(time, 12); c.writeUInt16LE(date, 14); c.writeUInt32LE(crc, 16); c.writeUInt32LE(body.length, 20); c.writeUInt32LE(data.length, 24);
      c.writeUInt16LE(name.length, 28); c.writeUInt32LE(at, 42);
      central.push(c, name);
    }
    const cdStart = offset;
    for (const b of central) write(b);
    const end = Buffer.alloc(22);
    end.writeUInt32LE(0x06054b50, 0); end.writeUInt16LE(files.length, 8); end.writeUInt16LE(files.length, 10);
    end.writeUInt32LE(offset - cdStart, 12); end.writeUInt32LE(cdStart, 16);
    write(end);
  } finally { fs.closeSync(fd); }
  return { files: files.length, bytes: offset };
}

async function unzip(zipFile, outDir) {
  const fh = await fs.promises.open(zipFile, 'r');
  try {
    const { size } = await fh.stat();
    const tailLen = Math.min(size, 65557);
    const tail = Buffer.alloc(tailLen);
    await fh.read(tail, 0, tailLen, size - tailLen);
    let e = -1;
    for (let i = tailLen - 22; i >= 0; i--) if (tail.readUInt32LE(i) === 0x06054b50) { e = i; break; }
    if (e < 0) throw new Error('not a zip file');
    const count = tail.readUInt16LE(e + 10);
    const cdSize = tail.readUInt32LE(e + 12);
    const cdStart = tail.readUInt32LE(e + 16);
    if (cdStart + cdSize > size) throw new Error('zip directory out of range');
    const cd = Buffer.alloc(cdSize);
    await fh.read(cd, 0, cdSize, cdStart);
    const root = path.resolve(outDir);
    let p = 0;
    let n = 0;
    for (let i = 0; i < count; i++) {
      if (cd.readUInt32LE(p) !== 0x02014b50) throw new Error('bad zip directory');
      const method = cd.readUInt16LE(p + 10);
      const crc = cd.readUInt32LE(p + 16);
      const csize = cd.readUInt32LE(p + 20);
      const usize = cd.readUInt32LE(p + 24);
      const nlen = cd.readUInt16LE(p + 28);
      const xlen = cd.readUInt16LE(p + 30);
      const clen = cd.readUInt16LE(p + 32);
      const at = cd.readUInt32LE(p + 42);
      const name = cd.toString('utf8', p + 46, p + 46 + nlen);
      p += 46 + nlen + xlen + clen;
      if (name.endsWith('/')) continue;
      const dest = path.resolve(root, name);
      if (name.includes('\\') || path.isAbsolute(name) || !dest.startsWith(root + path.sep)) throw new Error(`unsafe path in zip: ${name}`);
      const lh = Buffer.alloc(30);
      await fh.read(lh, 0, 30, at);
      if (lh.readUInt32LE(0) !== 0x04034b50) throw new Error(`bad entry header: ${name}`);
      const dataAt = at + 30 + lh.readUInt16LE(26) + lh.readUInt16LE(28);
      const body = Buffer.alloc(csize);
      await fh.read(body, 0, csize, dataAt);
      let data;
      if (method === 0) data = body;
      else if (method === 8) data = await inflateRaw(body);
      else throw new Error(`unsupported zip method ${method}: ${name}`);
      if (data.length !== usize || crc32(data) !== crc) throw new Error(`damaged entry: ${name}`);
      await fs.promises.mkdir(path.dirname(dest), { recursive: true });
      await fs.promises.writeFile(dest, data);
      n++;
    }
    return { files: n };
  } finally { await fh.close(); }
}

module.exports = { zipDir, unzip, listFiles, crc32 };
