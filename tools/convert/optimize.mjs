// Shrinks the models for the web: models/src/*.glb (full-size output of the converters) -> models/files/*.glb.
// Textures become WebP at most SIZE px, meshes get meshopt compression, animations drop redundant keyframes.
// The game's GLTFLoader has the meshopt decoder registered, so it reads both versions.
// Run from this folder: node optimize.mjs [--size 256] [--meshopt high] [only-this-file.glb ...]
// Little Legends: node optimize.mjs --legends [file.glb ...] reads models/src-legends/ and writes models/legends/,
// keeping the LEGEND_ROLES clips instead (convert-all.cjs --legends fills models/src-legends).
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import sharp from 'sharp';
import { MeshoptEncoder } from 'meshoptimizer';
import { NodeIO, Logger } from '@gltf-transform/core';
import { ALL_EXTENSIONS } from '@gltf-transform/extensions';
import { dedup, prune, resample, textureCompress, meshopt } from '@gltf-transform/functions';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.join(HERE, '..', '..');
const LEGENDS = process.argv.includes('--legends');
const SRC = path.join(ROOT, 'models', LEGENDS ? 'src-legends' : 'src');
const OUT = path.join(ROOT, 'models', LEGENDS ? 'legends' : 'files');
// --size N: longest texture side in px for this run (default 512; the roster uses 256 for extras, bosses and Digimon World 1)
// --meshopt high: stronger mesh quantization (about 25% smaller; the October 2026 roster files use it)
const LEVEL = process.argv.includes('--meshopt') ? process.argv[process.argv.indexOf('--meshopt') + 1] : 'medium';
const SIZE = process.argv.includes('--size') ? Number(process.argv[process.argv.indexOf('--size') + 1]) : 512;

// first run: the full-size files still live in models/files
if (!LEGENDS && !fs.existsSync(SRC)) fs.renameSync(OUT, SRC);
fs.mkdirSync(OUT, { recursive: true });

// Clips the renderer plays, by role, in priority order. Everything else is dropped and the kept clips are renamed
// to the role so js/render3d.js can find them by name. Cyber Sleuth codes: bn01 neutral, ba01 attack, bs01 skill, bv01 victory
// (ReArise uses the same codes with an _rd suffix). Digimon New Century: idle/stand, attack_move (the hit) or attack_move_to
// (run in and hit), skill2 or skill2_move_to, win. Next Order: a0_base (idle), a0_atck, a0_shot, a0_feel.
const ROLES = {
  idle: [/^idle$/, /idle|wait|stand/, /_bn01/, /_bn0[0-9]/, /^a0_base_fa0/],
  attack: [/^attack$/, /_ba01/, /^attack_move$/, /^attack_move_to$/, /attack(?!.*(special|signature|back))/, /^a0_atck_fa0/],
  special: [/signature/, /special/, /skill1$|^skill\d?$/, /_bs01/, /^skill\d?_move_to$/, /skill(?!.*back)/, /^a0_shot_fa0/],
  win: [/victory/, /_bv01/, /^win$/, /celebrate|win/, /glad|happy/, /_fe01/, /^a0_feel_fa0/],
};
// Little Legend clips, read by js/legends.js. Digimon Links names: idle, move, glad, win, eat, damage, guard, down,
// getup, attack01, attack02, special01. js/legends.js plays sad as down then getup and builds dances from these.
// Cyber Sleuth / ReArise codes: bn01 idle, br01 run, bv01 win, fe01 field emote (happy), bd01 hit, bd03 knocked down (sad),
// bg01 guard, ba01 attack, bs01 skill (taunt).
const LEGEND_ROLES = {
  idle: [/idle|wait|stand/, /_bn01/, /_bn0[0-9]/],
  run: [/^move$|run|walk/, /_br01/, /_br0[0-9]/],
  win: [/victory/, /^win$/, /_bv01/],
  dance: [/dance|celebrate/],
  happy: [/glad|happy|joy/, /_fe01/],
  eat: [/^eat|eating/],
  hit: [/damage|^hit/, /_bd01/],
  sad: [/sad|^down$|lose/, /_bd03/],
  getup: [/getup/],
  taunt: [/taunt|special/, /^attack0?2$/, /_bs01/],
  attack: [/^attack0?1?$/, /_ba01/],
  guard: [/guard/, /_bg01/],
  wave: [/wave|greet/],
};
function selectClips(doc) {
  const anims = doc.getRoot().listAnimations();
  if (!anims.length) return '';
  const keep = new Map();
  for (const [role, res] of Object.entries(LEGENDS ? LEGEND_ROLES : ROLES)) {
    for (const re of res) {
      const a = anims.find((x) => !keep.has(x) && re.test(x.getName().toLowerCase()));
      if (a) { keep.set(a, role); break; }
    }
  }
  // no idle by name: the renderer used to fall back to the first clip
  if (![...keep.values()].includes('idle') && !keep.has(anims[0])) keep.set(anims[0], 'idle');
  for (const a of anims) {
    if (keep.has(a)) a.setName(keep.get(a));
    else {
      // disposing the animation alone leaves its samplers holding the keyframe accessors, which prune then keeps
      for (const s of a.listSamplers()) { s.getInput()?.dispose(); s.getOutput()?.dispose(); s.dispose(); }
      for (const c of a.listChannels()) c.dispose();
      a.dispose();
    }
  }
  return [...keep.values()].join(',');
}

await MeshoptEncoder.ready;
const io = new NodeIO().registerExtensions(ALL_EXTENSIONS).registerDependencies({ 'meshopt.encoder': MeshoptEncoder });
io.setLogger(new Logger(Logger.Verbosity.WARN));
const only = process.argv.slice(2).filter((a, i, all) => !a.startsWith('--') && !['--size', '--meshopt'].includes(all[i - 1]));
const list = fs.readdirSync(SRC).filter((f) => f.endsWith('.glb') && (!only.length || only.includes(f)));
let before = 0;
let after = 0;
for (const f of list) {
  const src = path.join(SRC, f);
  let clips = '';
  const out = path.join(OUT, f);
  try {
    const doc = await io.read(src);
    clips = selectClips(doc);
    // some Links rips carry near-black vertex colors that darken the texture (patch-textures.cjs does the same)
    if (LEGENDS) for (const m of doc.getRoot().listMeshes()) for (const p of m.listPrimitives()) p.setAttribute('COLOR_0', null);
    await doc.transform(
      dedup(),
      prune(),
      resample(),
      textureCompress({ encoder: sharp, targetFormat: 'webp', quality: 88, resize: [SIZE, SIZE] }),
      meshopt({ encoder: MeshoptEncoder, level: LEVEL }),
    );
    await io.write(out, doc);
  } catch (e) {
    // keep the game working with the original file
    console.log(`fail ${f}: ${e.message}`);
    fs.copyFileSync(src, out);
  }
  const a = fs.statSync(src).size;
  const b = fs.statSync(out).size;
  before += a;
  after += b;
  console.log(`${f.padEnd(44)} ${(a / 1e6).toFixed(2).padStart(6)} MB -> ${(b / 1e6).toFixed(2).padStart(5)} MB  ${clips}`);
}
console.log(`\n${list.length} files: ${(before / 1e6).toFixed(1)} MB -> ${(after / 1e6).toFixed(1)} MB`);
