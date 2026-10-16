// Converts the Models Resource rips in rips/x/<Name>/ to models/src/rip_<name>.glb (optimize.mjs then writes models/files), points models/index.json at them,
// and writes their credits into js/models.js (between the RIPS markers).
// FBX with animations goes through FBX2glTF; Digimon Masters OBJ files through obj2gltf.
// Run from this folder: node convert-all.cjs
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const HERE = __dirname;
const ROOT = path.join(HERE, '..', '..');
const FBX2GLTF = path.join(HERE, 'node_modules', 'fbx2gltf', 'bin', 'Windows_NT', 'FBX2glTF.exe');
const OUT = path.join(ROOT, 'models', 'src');

// game form key -> rip folder (one rip can serve several forms)
const MAP = {
  'tentomon:1': 'Kabuterimon', 'tentomon:2': 'MegaKabuterimon', 'palmon:1': 'Togemon', 'palmon:2': 'Lillymon',
  'gomamon:0': 'Gomamon', 'biyomon:1': 'Birdramon', 'demidevimon:0': 'DemiDevimon', 'demidevimon:2': 'NeoDevimon',
  'guilmon:2': 'WarGrowlmon', 'renamon:2': 'Taomon', 'terriermon:2': 'Rapidmon', 'patamon:1': 'Angemon',
  'salamon:0': 'Salamon', 'wormmon:0': 'Wormmon', 'wormmon:1': 'Stingmon', 'wormmon:2': 'Dinobeemon',
  'elecmon:0': 'Elecmon', 'elecmon:2': 'Panjyamon', 'keramon:1': 'Chrysalimon', 'lopmon:1': 'Turuiemon',
  'lopmon:2': 'Andiramon', 'gizamon:0': 'Gizamon', 'gizamon:1': 'Gesomon', 'gizamon:2': 'MarineDevimon',
  'candlemon:2': 'Mistymon', 'gaomon:2': 'MachGaogamon', 'lalamon:0': 'Lalamon', 'lalamon:2': 'Lilamon',
  'commandramon:0': 'Commandramon', 'commandramon:1': 'Sealsdramon', 'commandramon:2': 'Tankdramon',
  'myotismon:0': 'Myotismon', 'myotismon:1': 'VenomMyotismon', 'lucemon:0': 'Lucemon', 'lucemon:1': 'Lucemon_FM',
  'lucemon:2': 'Lucemon_SM', 'machinedramon:0': 'Megadramon',
  'goblimon:0': 'Goblimon', 'goblimon:1': 'Goblimon', 'goblimon:2': 'Goblimon',
  'kuwagamon:0': 'Kuwagamon', 'kuwagamon:1': 'Kuwagamon', 'kuwagamon:2': 'Kuwagamon',
  'ogremon:0': 'Ogremon', 'ogremon:1': 'Ogremon', 'ogremon:2': 'Ogremon',
  'etemon:0': 'Etemon', 'etemon:1': 'Etemon', 'etemon:2': 'Etemon',
  // second batch: animated replacements for static Sketchfab models
  'agumon:2': 'MetalGreymon', 'palmon:0': 'Palmon', 'gomamon:2': 'Zudomon', 'biyomon:2': 'Garudamon', 'renamon:0': 'Renamon',
  'terriermon:0': 'Terriermon', 'terriermon:1': 'Gargomon', 'salamon:2': 'Angewomon', 'betamon:0': 'Betamon', 'betamon:1': 'Seadramon',
  'betamon:2': 'MegaSeadramon', 'elecmon:1': 'Leomon', 'lopmon:0': 'Lopmon', 'dorumon:2': 'DoruGreymon', 'gaomon:1': 'Gaogamon',
  'imperialdramon:1': 'Imperialdramon_FM', 'imperialdramon:2': 'Imperialdramon_PM', 'beelzemon:0': 'Impmon',
  'gizamon:1': 'Gesomon2', 'gizamon:2': 'MarineDevimon2',
};
// Roster expansion (October 2026): new shop lines, same format as MAP. tools/convert/new-chains.json has the results.
const NEW = {
  'agumonsv:0': 'GeoGreymon', 'agumonsv:1': 'RizeGreymon', 'agumonsv:2': 'ShineGreymon',
  'falcomon:0': 'Falcomon', 'falcomon:1': 'Peckmon', 'falcomon:2': 'Crowmon',
  'kudamon:0': 'Kudamon', 'kudamon:1': 'Reppamon', 'kudamon:2': 'Chirinmon',
  'dracomon:0': 'Dracomon', 'dracomon:1': 'Coredramon_Green', 'dracomon:2': 'Wingdramon',
  'ryudamon:0': 'Ryudamon', 'ryudamon:1': 'Ginryumon', 'ryudamon:2': 'Hisyaryumon',
  'zubamon:0': 'Zubamon', 'zubamon:1': 'ZubaEagermon', 'zubamon:2': 'Durandamon',
  'vorvomon:0': 'Vorvomon', 'vorvomon:1': 'Lavorvomon', 'vorvomon:2': 'Lavogaritamon',
  'jazamon:0': 'Jazamon', 'jazamon:1': 'Jazardmon', 'jazamon:2': 'Jazarichmon',
  'armadillomon:0': 'Armadillomon', 'armadillomon:1': 'Ankylomon', 'armadillomon:2': 'Shakkoumon',
  'meicoomon:0': 'Meicoomon', 'meicoomon:1': 'Meicrackmon_Vicious_Mode', 'meicoomon:2': 'Raguelmon',
  'numemon:0': 'Numemon', 'numemon:1': 'BlackKingNumemon', 'numemon:2': 'PlatinumNumemon',
  'mamemon:0': 'Mamemon', 'mamemon:1': 'MetalMamemon', 'mamemon:2': 'PrinceMamemon',
  'woodmon:0': 'Woodmon', 'woodmon:1': 'Cherrymon', 'woodmon:2': 'Puppetmon',
  'gotsumon:0': 'Gotsumon', 'gotsumon:1': 'Starmon', 'gotsumon:2': 'SuperStarmon',
  'otamamon:0': 'Otamamon', 'otamamon:1': 'Gekomon', 'otamamon:2': 'ShogunGekomon',
  'blackgatomon:0': 'Gatomon', 'blackgatomon:1': 'LadyDevimon', 'blackgatomon:2': 'Lilithmon',
  'veedramon:0': 'Veedramon', 'veedramon:1': 'AeroVeedramon', 'veedramon:2': 'UlforceVeedramon',
  'hackmon:0': 'Hackmon', 'hackmon:1': 'BaoHuckmon', 'hackmon:2': 'Jesmon',
  'lunamon:0': 'Lunamon', 'lunamon:1': 'Lekismon', 'lunamon:2': 'Crescemon',
  'coronamon:0': 'Coronamon', 'coronamon:1': 'Firamon', 'coronamon:2': 'Flaremon',
  'agunimon:0': 'Agunimon', 'agunimon:1': 'BurningGreymon', 'agunimon:2': 'EmperorGreymon',
  'lobomon:0': 'Lobomon', 'lobomon:1': 'KendoGarurumon', 'lobomon:2': 'MagnaGarurumon',
  'kazemon:0': 'Kazemon', 'kazemon:1': 'Zephyrmon', 'kazemon:2': 'JetSilphymon',
  'monodramon:0': 'Monodramon', 'monodramon:1': 'Strikedramon', 'monodramon:2': 'Cyberdramon',
  'bearmon:0': 'Bearmon', 'bearmon:1': 'Grizzlymon', 'bearmon:2': 'GrapLeomon',
  'floramon:0': 'Floramon', 'floramon:1': 'Kiwimon', 'floramon:2': 'Blossomon',
  'dracmon:0': 'Dracmon', 'dracmon:1': 'Sangloupmon', 'dracmon:2': 'Matadormon',
  'fanbeemon:0': 'FanBeemon', 'fanbeemon:1': 'Waspmon', 'fanbeemon:2': 'CannonBeemon',
  'chessmon:0': 'PawnChessmon_Black', 'chessmon:1': 'KnightChessmon_Black', 'chessmon:2': 'KingChessmon',
  'shoutmon:0': 'Shoutmon', 'shoutmon:1': 'Shoutmon_X3', 'shoutmon:2': 'Shoutmon_X3',
  'beetlemon:0': 'Beetlemon', 'beetlemon:1': 'MetalKabuterimon', 'beetlemon:2': 'MetalKabuterimon',
};
// Spare forms, bosses, the Digimon World 1 set and misc Digimon Masters models: 'extra:<Folder>' keys. The roster maps them to
// line forms by adding more models/index.json keys that point at the same file.
const EXTRA = `
Groundramon Breakdramon Durandamon Lilithmon Koromon Gammamon Dorulumon Ballistamon Kunemon DW1_Agumon DW1_Greymon DW1_MetalGreymon
DW1_Patamon DW1_Airdramon DW1_Bakemon DW1_Birdramon DW1_Frigimon DW1_Meramon DW1_MudFrigimon DW1_Andromon DW1_Mamemon DW1_Monzaemon DW1_Mugendramon DW1_Vademon
DW1_Akatorimon DW1_BlueMeramon DW1_Brachiomon DW1_Cherrymon DW1_DemiMeramon DW1_Gigadramon DW1_Jijimon DW1_Tankmon DW1_Hagurumon DW1_Botamon DW1_Poyomon DW1_Punimon
DW1_Tokomon DW1_Tsunomon Omnimon Alphamon Piedmon Daemon Barbamon Leviamon Belphemon_Rage_Mode MetalSeadramon Azulongmon Zhuqiaomon
Baihumon Ebonwumon Diaboromon Kimeramon Crusadermon Leopardmon_Leopard_Mode Craniamon Dynasmon Magnamon Seraphimon Ophanimon WarGreymon
MetalGarurumon BanchoLeomon Sakuyamon HerculesKabuterimon Phoenixmon Magnadramon Plesiomon SaberLeomon Titamon Boltmon Armageddemon Monzaemon
Vademon Piximon Okuwamon GranKuwagamon Frigimon Vegiemon Nanimon Bakemon Mojyamon Sukamon`.trim().split(/\s+/);
for (const f of EXTRA) NEW[`extra:${f}`] = f;
// A map may also hold "_pick" { Folder: regex }, "_form" { Folder: credit name }, "_retexture" { Folder: { from.png: to.png } } and "_alias" { Folder: real rips/x folder }
// (an alias converts one rip folder a second time, e.g. for a recolor).
const EXT = { _pick: {}, _form: {}, _retexture: {}, _alias: {} };
// More form -> folder maps, one JSON file per set/batch in maps/ (so parallel batches don't edit this file): { "key": "Folder" }
for (const f of fs.existsSync(path.join(HERE, 'maps')) ? fs.readdirSync(path.join(HERE, 'maps')).filter((n) => n.endsWith('.json')).sort() : []) {
  for (const [k, v] of Object.entries(JSON.parse(fs.readFileSync(path.join(HERE, 'maps', f), 'utf8')))) if (k in EXT) Object.assign(EXT[k], v); else NEW[k] = v;
}
Object.assign(MAP, NEW);
const GAME = { Links: 'Digimon Links', ReArise: 'Digimon ReArise', CyberSleuth: 'Digimon Story Cyber Sleuth: Complete Edition', Masters: 'Digimon Masters', TimeStranger: 'Digimon Story: Time Stranger', digimonworld: 'Digimon World', DW1: 'Digimon World',
  digimonlinks: 'Digimon Links', digimonstorycybersleuthcompleteedition: 'Digimon Story Cyber Sleuth: Complete Edition', digimonnewcentury: 'Digimon New Century',
  DigimonWorld: 'Digimon World', NextOrder: 'Digimon World: Next Order', ReDigitize: 'Digimon World Re:Digitize' };
// Which file to convert when a rip has several (regex on its path). Default: the 'With Animations' FBX, else one in an
// Animations folder, else the first FBX, else the first OBJ, else a DAE (FBX2glTF reads COLLADA too).
const PICK = {
  PawnChessmon_Black: /g146_d\.fbx$/i, KnightChessmon_Black: /m181_d\.fbx$/i, Kiwimon: /m140_m\.fbx$/i, MagnaGarurumon: /chr675\.fbx$/i,
  Gammamon: /\.obj$/i, DW1_Greymon: /Greymon1\.obj$/i, DW1_Bakemon: /Bakemon 1\.obj$/i, DW1_Meramon: /Meramon 1\.obj$/i, DW1_Vademon: /Vademon1\.obj$/i,
};
// Texture swaps after conversion: folder -> { texture the converter linked: file in the rip to use instead }.
const RETEXTURE = {
  Gatomon: { 'chr092a01.png': 'chr043a01.png' }, // the zip is Gatomon / BlackGatomon; chr043a01 is the BlackGatomon skin
  PawnChessmon_Black: { 'bingqishou(bai)_base.png': 'bingqishou(hei)_base.png' }, // bai = white, hei = black
  KnightChessmon_Black: { 'qiqishoubai_base.png': 'qiqishouhei_base.png' },
  L_Panjyamon: { 'chr072a01.png': 'chr409a01.png' }, // copy of the Links Leomon / IceLeomon zip; chr409a01 is the Panjyamon (IceLeomon) skin
};
// Credit names for NEW folders whose name is not the form name (others: underscores become spaces, DW1_ is dropped).
const NEW_FORM = { Gatomon: 'BlackGatomon', Coredramon_Green: 'Coredramon (Green)', PawnChessmon_Black: 'PawnChessmon (Black)', KnightChessmon_Black: 'KnightChessmon (Black)' };
for (const [k, v] of Object.entries(EXT._pick)) PICK[k] = new RegExp(v, 'i');
Object.assign(RETEXTURE, EXT._retexture);
Object.assign(NEW_FORM, EXT._form);
const ripDir = (folder) => path.join(HERE, 'rips', 'x', EXT._alias[folder] || folder);
const ONLY = process.argv.includes('--only') ? process.argv[process.argv.indexOf('--only') + 1].split(',') : null;
const NEW_ONLY = process.argv.includes('--new'); // convert only the NEW folders, keep the other models/src files

const files = (d) => fs.readdirSync(d, { withFileTypes: true }).flatMap((e) => (e.isDirectory() ? files(path.join(d, e.name)) : [path.join(d, e.name)]));
const index = JSON.parse(fs.readFileSync(path.join(HERE, 'rips', 'index.json'), 'utf8'));
const meta = Object.fromEntries(index.map((e) => [e.name.replace(/ /g, '_'), e]));
meta.Imperialdramon_FM = meta.Imperialdramon_PM = meta.Imperialdramon_FM_PM;
meta.L_Panjyamon = meta.Leomon;

function pickModel(folder) {
  const all = files(ripDir(folder));
  if (PICK[folder]) return all.find((f) => PICK[folder].test(f));
  const fbx = all.filter((f) => /\.fbx$/i.test(f) && !/splituv/i.test(f));
  return fbx.find((f) => /with anim/i.test(f)) || fbx.find((f) => /[\\/]animations[\\/]/i.test(f)) || fbx.find((f) => !/iphone/i.test(f)) || fbx[0]
    || all.find((f) => /\.obj$/i.test(f)) || all.find((f) => /\.dae$/i.test(f));
}
// FBX / DAE through FBX2glTF, OBJ through obj2gltf; writes <dir>/<name>.glb
function convertOne(pick, dir, name) {
  if (/\.obj$/i.test(pick)) execFileSync(process.execPath, [path.join(HERE, 'node_modules', 'obj2gltf', 'bin', 'obj2gltf.js'), '-i', pick, '-o', path.join(dir, `${name}.glb`), '-b'], { stdio: 'pipe', windowsHide: true });
  else execFileSync(FBX2GLTF, ['-b', '--pbr-metallic-roughness', '-i', pick, '-o', path.join(dir, name)], { stdio: 'pipe', windowsHide: true, timeout: 600000 });
}
async function retexture(folder, glb) {
  const swap = RETEXTURE[folder];
  if (!swap) return;
  const { NodeIO } = require('@gltf-transform/core');
  const all = files(ripDir(folder));
  const io = new NodeIO();
  const doc = await io.read(glb);
  for (const t of doc.getRoot().listTextures()) {
    const to = swap[path.basename(t.getURI() || '')] || swap[`${t.getName()}.png`] || swap[t.getName()];
    const img = to && all.find((f) => path.basename(f) === to);
    if (img) t.setImage(fs.readFileSync(img)).setMimeType('image/png');
  }
  await io.write(glb, doc);
}

// Little Legends: node convert-all.cjs --legends converts these to models/src-legends/<id>.glb (then optimize.mjs --legends).
// A value is a rip folder in rips/x/, or 'src:<file>' to reuse a full-size board model from models/src.
// Keep the ids in step with the catalog in js/legends.js. The Links rips are legend_<Name> folders; the October 2026 set
// (Cyber Sleuth, ReArise, Digimon Masters) uses the plain folder names.
const LEGEND_MAP = Object.fromEntries([
  ...['Botamon', 'Kuramon', 'Pabumon', 'Poyomon', 'Punimon', 'Bukamon', 'Motimon', 'Nyaromon', 'Pagumon', 'Tanemon', 'Tokomon', 'Tsumemon', 'Tsunomon', 'Wanyamon', 'Yokomon']
    .map((n) => [n.toLowerCase(), `legend_${n}`]),
  ...['Koromon', 'Chibomon', 'Conomon', 'Dodomon', 'Jyarimon', 'Pichimon', 'Pusumon', 'YukimiBotamon', 'Yuramon', 'Zerimon', 'DemiVeemon', 'Dorimon',
    'Gigimon', 'Gummymon', 'Kokomon', 'Pusurimon', 'Upamon'].map((n) => [n.toLowerCase(), n]),
]);
if (process.argv.includes('--legends')) {
  const LOUT = path.join(ROOT, 'models', 'src-legends');
  fs.mkdirSync(LOUT, { recursive: true });
  const done = [];
  for (const [id, from] of Object.entries(LEGEND_MAP)) {
    if (ONLY && !ONLY.includes(id)) continue;
    const out = path.join(LOUT, `${id}.glb`);
    try {
      if (from.startsWith('src:')) fs.copyFileSync(path.join(OUT, from.slice(4)), out);
      else {
        const pick = pickModel(from);
        if (!pick) throw new Error('no model file');
        convertOne(pick, LOUT, id);
      }
      done.push(`${id}.glb`);
      console.log(`ok   ${id.padEnd(16)} ${Math.round(fs.statSync(out).size / 1024)} KB  <- ${from}`);
    } catch (err) {
      console.log(`FAIL ${id}: ${String(err.stderr || err.message).slice(0, 200)}`);
    }
  }
  // Cyber Sleuth legends need the same stand-up fix as the board rips
  if (done.length) process.stdout.write(execFileSync(process.execPath, [path.join(HERE, 'fix-upright.mjs'), '--rips', '--dir', LOUT, ...done], { cwd: HERE, windowsHide: true }));
  process.exit(0);
}

(async () => {
const converted = {};
const newFolders = new Set(Object.values(NEW));
for (const folder of [...new Set(Object.values(MAP))]) {
  const name = `rip_${folder.toLowerCase()}`;
  const out = path.join(OUT, `${name}.glb`);
  // --new / --only: keep the existing models/src file of the folders not converted this run
  if ((ONLY && !ONLY.includes(folder)) || (NEW_ONLY && !newFolders.has(folder))) {
    if (fs.existsSync(out)) converted[folder] = { file: `models/files/${name}.glb`, kept: true };
    continue;
  }
  const pick = pickModel(folder);
  try {
    if (!pick) throw new Error('no model file');
    convertOne(pick, OUT, name);
    await retexture(folder, out);
    converted[folder] = { file: `models/files/${name}.glb`, src: path.relative(path.join(HERE, 'rips', 'x'), pick), kb: Math.round(fs.statSync(out).size / 1024) };
    console.log(`ok   ${folder.padEnd(16)} ${converted[folder].kb} KB  <- ${converted[folder].src}`);
  } catch (err) {
    console.log(`FAIL ${folder}: ${String(err.stderr || err.message).slice(0, 200)}`);
  }
}

// stand Z-up conversions back up and apply upright.json corrections (fix-upright.mjs)
const done = Object.keys(converted).filter((f) => !converted[f].kept).map((f) => `rip_${f.toLowerCase()}.glb`);
if (done.length) process.stdout.write(execFileSync(process.execPath, [path.join(HERE, 'fix-upright.mjs'), ...done], { cwd: HERE, windowsHide: true }));

const mi = JSON.parse(fs.readFileSync(path.join(ROOT, 'models', 'index.json'), 'utf8'));
const credits = {};
for (const [key, folder] of Object.entries(MAP)) {
  const c = converted[folder];
  if (!c) continue;
  mi[key] = c.file;
  const m = meta[folder] || meta[EXT._alias[folder]] || {};
  const nu = newFolders.has(folder) ? NEW_FORM[folder] || folder.replace(/^(DW1|CS|L|TS|NO|RD)_/, '').replace(/_/g, ' ') : null;
  credits[key] = { form: nu || folder.replace(/2$/, '').replace('Lucemon_FM', 'Lucemon Chaos Mode').replace('_SM', ' Shadowlord Mode').replace('_FM', ' Fighter Mode').replace('_PM', ' Paladin Mode'), uid: `rip-${folder.toLowerCase()}`, rip: true,
    name: `${nu || folder.replace(/2$/, '').replace(/_/g, ' ')} (${GAME[m.game] || m.game})`, author: 'The Models Resource', license: 'Game rip', url: m.page };
}
fs.writeFileSync(path.join(ROOT, 'models', 'index.json'), JSON.stringify(mi, null, 1));

const mjs = path.join(ROOT, 'js', 'models.js');
let src = fs.readFileSync(mjs, 'utf8').replace(/\n  \/\/ RIPS-START[\s\S]*?\/\/ RIPS-END\n/, '\n');
const block = `\n  // RIPS-START (generated by tools/convert/convert-all.cjs: game rips from The Models Resource, converted to GLB)\n  Object.assign(DT.MODEL_SOURCES, ${JSON.stringify(credits, null, 2).replace(/\n/g, '\n  ')});\n  // RIPS-END\n`;
// the generated block goes before the hand-kept MODEL-OPTIONS section, which merges over it
src = src.includes('\n  // MODEL-OPTIONS') ? src.replace(/\n+  \/\/ MODEL-OPTIONS/, `${block}\n  // MODEL-OPTIONS`) : src.replace(/\n\}\)\(typeof window/, `${block}})(typeof window`);
fs.writeFileSync(mjs, src);
console.log(`${Object.keys(converted).length} models converted, ${Object.keys(credits).length} forms mapped`);
})();
