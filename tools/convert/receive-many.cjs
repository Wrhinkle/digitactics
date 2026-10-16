// Local receiver for model bundles posted from a models.spriters-resource.com tab (the site blocks scripted downloads).
// POST http://127.0.0.1:8199/?name=<file> writes incoming/<file>. Stays up until killed.
const http = require('http');
const fs = require('fs');
const path = require('path');
const DIR = path.join(__dirname, 'incoming');
fs.mkdirSync(DIR, { recursive: true });
http.createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', 'https://models.spriters-resource.com');
  res.setHeader('Access-Control-Allow-Private-Network', 'true');
  res.setHeader('Access-Control-Allow-Headers', 'content-type');
  if (req.method === 'OPTIONS') return res.end();
  const name = path.basename(new URL(req.url, 'http://x').searchParams.get('name') || 'bundle.bin');
  const chunks = [];
  req.on('data', (c) => chunks.push(c));
  req.on('end', () => {
    const buf = Buffer.concat(chunks);
    fs.writeFileSync(path.join(DIR, name), buf);
    console.log('received', name, buf.length);
    res.end('ok ' + buf.length);
  });
}).listen(8199, '127.0.0.1');
