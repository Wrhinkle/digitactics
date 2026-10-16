// One-shot local receiver: the browser POSTs the model bundle here. Writes bundle.bin, then exits.
const http = require('http');
const fs = require('fs');
http.createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', 'https://models.spriters-resource.com');
  res.setHeader('Access-Control-Allow-Private-Network', 'true');
  res.setHeader('Access-Control-Allow-Headers', 'content-type');
  if (req.method === 'OPTIONS') return res.end();
  const chunks = [];
  req.on('data', (c) => chunks.push(c));
  req.on('end', () => {
    fs.writeFileSync('bundle.bin', Buffer.concat(chunks));
    res.end('ok ' + Buffer.concat(chunks).length);
    console.log('received', Buffer.concat(chunks).length);
    setTimeout(() => process.exit(0), 200);
  });
}).listen(8199, '127.0.0.1');
