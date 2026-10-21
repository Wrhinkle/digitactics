#!/usr/bin/env node
// Ask Codex (OpenAI CLI, logged in) a question about one or more images and print its answer.
//
//   node tools/parity/vision-check.cjs <image1> [image2 ...] [--question "..."] [--out answer.txt] [--model <m>] [--timeout-sec 600]
//   node tools/parity/vision-check.cjs --pair <state> [--size 1600x900]      # parity/<state>/<size>/{electron,native}.png
//
// Runs: codex exec --skip-git-repo-check -s read-only --ephemeral -o <tmp> --image=<a> --image=<b> -- "<question>"
// The process is hidden (windowsHide, stdin ignored). Exit code 0 on success; the answer goes to stdout.
// Library use: const { visionCheck } = require('./vision-check.cjs'); await visionCheck([a, b], question)
'use strict';
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { spawn } = require('node:child_process');

const DEFAULT_QUESTION = 'List every visual difference between image 1 (reference) and image 2 (candidate), most important first.';

function codexCommand() {
  if (process.platform !== 'win32') return { file: 'codex', prefix: [] };
  // Run the npm entry script with this Node so no cmd.exe shim (and no console window) is involved.
  const roots = [process.env.APPDATA && path.join(process.env.APPDATA, 'npm'), ...String(process.env.PATH || '').split(path.delimiter)].filter(Boolean);
  for (const root of roots) {
    const script = path.join(root, 'node_modules', '@openai', 'codex', 'bin', 'codex.js');
    if (fs.existsSync(script)) return { file: process.execPath, prefix: [script] };
  }
  throw new Error('Codex CLI not found: expected node_modules/@openai/codex/bin/codex.js under %APPDATA%\\npm or PATH.');
}

function visionCheck(images, question = DEFAULT_QUESTION, { model, timeoutSec = 600, cwd } = {}) {
  if (!images.length) return Promise.reject(new Error('Provide at least one image.'));
  for (const image of images) if (!fs.existsSync(image)) return Promise.reject(new Error('Missing image: ' + image));
  const temp = fs.mkdtempSync(path.join(os.tmpdir(), 'vision-check-'));
  const answerFile = path.join(temp, 'answer.txt');
  const { file, prefix } = codexCommand();
  const args = [...prefix, 'exec', '--skip-git-repo-check', '-s', 'read-only', '--ephemeral', '--color', 'never', '-o', answerFile];
  if (model) args.push('-m', model);
  for (const image of images) args.push('--image=' + path.resolve(image));
  args.push('--', question);
  return new Promise((resolve, reject) => {
    const child = spawn(file, args, { cwd: cwd || temp, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'] });
    let stdout = '', stderr = '';
    child.stdout.on('data', d => { stdout += d; });
    child.stderr.on('data', d => { stderr += d; });
    const timer = setTimeout(() => { child.kill(); reject(new Error(`codex timed out after ${timeoutSec}s`)); }, timeoutSec * 1000);
    child.on('error', error => { clearTimeout(timer); reject(error); });
    child.on('close', code => {
      clearTimeout(timer);
      let answer = '';
      try { answer = fs.readFileSync(answerFile, 'utf8').trim(); } catch { /* fall back to stdout */ }
      fs.rmSync(temp, { recursive: true, force: true });
      if (code !== 0) return reject(new Error(`codex exited ${code}: ${(stderr || stdout).trim().slice(-2000)}`));
      resolve(answer || stdout.trim());
    });
  });
}

async function main() {
  const argv = process.argv.slice(2);
  const images = []; let question = DEFAULT_QUESTION, out, model, timeoutSec = 600, pair, size = '1600x900';
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === '--question' || a === '-q') question = argv[++i];
    else if (a === '--out') out = argv[++i];
    else if (a === '--model') model = argv[++i];
    else if (a === '--timeout-sec') timeoutSec = Number(argv[++i]);
    else if (a === '--pair') pair = argv[++i];
    else if (a === '--size') size = argv[++i];
    else if (a === '--image' || a === '-i') images.push(argv[++i]);
    else if (a.startsWith('--image=')) images.push(a.slice(8));
    else if (a === '-h' || a === '--help') { console.log(fs.readFileSync(__filename, 'utf8').split('\n').slice(1, 9).join('\n')); return; }
    else images.push(a);
  }
  if (pair) {
    const dir = path.join(__dirname, '..', '..', 'parity', pair, size);
    images.unshift(path.join(dir, 'electron.png'), path.join(dir, 'native.png'));
  }
  const started = Date.now();
  const answer = await visionCheck(images, question, { model, timeoutSec });
  if (out) { fs.mkdirSync(path.dirname(path.resolve(out)), { recursive: true }); fs.writeFileSync(out, answer + '\n'); }
  process.stdout.write(answer + '\n');
  process.stderr.write(`vision-check: ${images.length} image(s), ${((Date.now() - started) / 1000).toFixed(1)}s\n`);
}

module.exports = { visionCheck, DEFAULT_QUESTION };
if (require.main === module) main().catch(error => { console.error(error.message); process.exit(1); });
