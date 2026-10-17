// Little Legend digivolution: each owned legend has a level, Digi-Data, care stats and a current form along a tree.
// Client-only and outside the lockstep rules, like the rest of the cosmetics. Saved in localStorage 'dt.legendevo.v1'
// (separate from 'dt.cosmetics.v1', so older saves and owned items are untouched).
//
// Tree data (this file):
//   FORMS  form id -> [model, level, name?]. model is a key in models/index.json ('line:stage' or 'extra:<Rip>') or
//          'legend:<id>' for a Little Legend model (models/legends/<id>.glb). Names come from DT.LINE, the legend
//          catalog, or the third entry. A legend's own base form is its catalog id. To add a form, add it here and
//          link it in EVO. Never remove or rename a form id: saves hold them.
//   EVO    form id -> 'child:care child:care ...'. care is the care stat that raises that branch's odds:
//          bond (games played with it equipped), battle (top 4s and wins), food (fed with Bits),
//          wild (the fun branch: likely with little care, rarer as care grows).
//   Tiers: 0 = Fresh / In-Training, 1 = Rookie, 2 = Champion, 3 = Ultimate. A Fresh legend can digivolve into an
//          In-Training form at level IT_LEVEL; Rookie, Champion and Ultimate need TIER_LEVEL as before.
//
// DT.LegendEvo.info(id)          everything the panel shows: level, Digi-Data, care, form, odds, next level, ...
// DT.LegendEvo.key(id, form?)    'id' or 'id@form' (what DT.Legends.createView / html take)
// DT.LegendEvo.parse(key)        { legend, form }
// DT.LegendEvo.formInfo(formId)  { id, name, level, tier, rank, model? , legend? }
// DT.LegendEvo.tree(id)          { root, tiers: [[form ids]] one row per level, levels: [level names], edges: [[from, to]] }
// DT.LegendEvo.odds(id)          [{ form, care, weight, pct }] for the next digivolution (pct sums to 100)
// DT.LegendEvo.digivolve(id) / revert(id) / buyLevel(id) / feed(id) / addDuplicate(id) / recordGame(key, place)
// DT.LegendEvo.look(loadout)     copy of a loadout whose .legend carries its form ('id@form'), for DT.Social.lookOf
// DT.LegendEvo.aiForm(id, seed)  a stable random evolved form for AI Tamers
// DT.LegendEvo.onChange(fn)
const DT = window.DT;
const C = DT.Cosmetics;
const KEY = 'dt.legendevo.v1';

// ---------- tree data ----------
const IT = 'In-Training', R = 'Rookie', CH = 'Champion', U = 'Ultimate';
const FORMS = {
  // In-Training forms drawn with a Little Legend model ('legend:<id>'). A legend with the same id uses it as its base.
  koromon: ['legend:koromon', IT], demiveemon: ['legend:demiveemon', IT], gigimon: ['legend:gigimon', IT],
  gummymon: ['legend:gummymon', IT], kokomon: ['legend:kokomon', IT], dorimon: ['legend:dorimon', IT],
  pusurimon: ['legend:pusurimon', IT], bukamon: ['legend:bukamon', IT], tanemon: ['legend:tanemon', IT],
  nyaromon: ['legend:nyaromon', IT],
  // Rookies
  agumon: ['agumon:0', R], gabumon: ['gabumon:0', R], tentomon: ['tentomon:0', R], palmon: ['palmon:0', R],
  gomamon: ['gomamon:0', R], biyomon: ['biyomon:0', R], hagurumon: ['hagurumon:0', R], demidevimon: ['demidevimon:0', R],
  guilmon: ['guilmon:0', R], renamon: ['renamon:0', R], terriermon: ['terriermon:0', R], patamon: ['patamon:0', R],
  salamon: ['salamon:0', R], betamon: ['betamon:0', R], wormmon: ['wormmon:0', R], elecmon: ['elecmon:0', R],
  hawkmon: ['hawkmon:0', R], keramon: ['keramon:0', R], lopmon: ['lopmon:0', R], gizamon: ['gizamon:0', R],
  candlemon: ['candlemon:0', R], veemon: ['veemon:0', R], dorumon: ['dorumon:0', R], gaomon: ['gaomon:0', R],
  lalamon: ['lalamon:0', R], commandramon: ['commandramon:0', R], goblimon: ['goblimon:0', R], impmon: ['beelzemon:0', R],
  lucemon: ['lucemon:0', R],
  dracomon: ['dracomon:0', R, 'Dracomon'], monodramon: ['monodramon:0', R, 'Monodramon'], gotsumon: ['gotsumon:0', R, 'Gotsumon'], armadillomon: ['armadillomon:0', R, 'Armadillomon'],
  kudamon: ['kudamon:0', R, 'Kudamon'], meicoomon: ['meicoomon:0', R, 'Meicoomon'], floramon: ['floramon:0', R, 'Floramon'], otamamon: ['otamamon:0', R, 'Otamamon'],
  dracmon: ['dracmon:0', R, 'Dracmon'], bearmon: ['bearmon:0', R, 'Bearmon'],
  // Champions
  greymon: ['agumon:1', CH], garurumon: ['gabumon:1', CH], kabuterimon: ['tentomon:1', CH], togemon: ['palmon:1', CH],
  ikkakumon: ['gomamon:1', CH], birdramon: ['biyomon:1', CH], guardromon: ['hagurumon:1', CH], devimon: ['demidevimon:1', CH],
  growlmon: ['guilmon:1', CH], kyubimon: ['renamon:1', CH], gargomon: ['terriermon:1', CH], angemon: ['patamon:1', CH],
  gatomon: ['salamon:1', CH], seadramon: ['betamon:1', CH], stingmon: ['wormmon:1', CH], leomon: ['elecmon:1', CH],
  aquilamon: ['hawkmon:1', CH], kurisarimon: ['keramon:1', CH], turuiemon: ['lopmon:1', CH], gesomon: ['gizamon:1', CH],
  wizardmon: ['candlemon:1', CH], exveemon: ['veemon:1', CH], dorugamon: ['dorumon:1', CH], gaogamon: ['gaomon:1', CH],
  sunflowmon: ['lalamon:1', CH], sealsdramon: ['commandramon:1', CH], ogremon: ['ogremon:0', CH], kuwagamon: ['kuwagamon:0', CH],
  tyrannomon: ['tyrannomon:0', CH],
  geogreymon: ['agumonsv:0', CH, 'GeoGreymon'], veedramon: ['veedramon:0', CH, 'Veedramon'], coredramon: ['dracomon:1', CH, 'Coredramon'], strikedramon: ['monodramon:1', CH, 'Strikedramon'],
  numemon: ['numemon:0', CH, 'Numemon'], sukamon: ['extra:Sukamon', CH, 'Sukamon'], starmon: ['gotsumon:1', CH, 'Starmon'], ankylomon: ['armadillomon:1', CH, 'Ankylomon'],
  reppamon: ['kudamon:1', CH, 'Reppamon'], blackgatomon: ['blackgatomon:0', CH, 'BlackGatomon'], kiwimon: ['floramon:1', CH, 'Kiwimon'], woodmon: ['woodmon:0', CH, 'Woodmon'],
  gekomon: ['otamamon:1', CH, 'Gekomon'], sangloupmon: ['dracmon:1', CH, 'Sangloupmon'], grizzlymon: ['bearmon:1', CH, 'Grizzlymon'], bakemon: ['extra:Bakemon', CH, 'Bakemon'],
  lekismon: ['lunamon:1', CH, 'Lekismon'],
  // Ultimates
  metalgreymon: ['agumon:2', U], weregarurumon: ['gabumon:2', U], megakabuterimon: ['tentomon:2', U], lillymon: ['palmon:2', U],
  zudomon: ['gomamon:2', U], garudamon: ['biyomon:2', U], andromon: ['hagurumon:2', U], neodevimon: ['demidevimon:2', U],
  wargrowlmon: ['guilmon:2', U], taomon: ['renamon:2', U], rapidmon: ['terriermon:2', U], magnaangemon: ['patamon:2', U],
  angewomon: ['salamon:2', U], megaseadramon: ['betamon:2', U], dinobeemon: ['wormmon:2', U], panjyamon: ['elecmon:2', U],
  silphymon: ['hawkmon:2', U], infermon: ['keramon:2', U], andiramon: ['lopmon:2', U], marinedevimon: ['gizamon:2', U],
  mistymon: ['candlemon:2', U], paildramon: ['veemon:2', U], dorugreymon: ['dorumon:2', U], machgaogamon: ['gaomon:2', U],
  lilamon: ['lalamon:2', U], tankdramon: ['commandramon:2', U], etemon: ['etemon:0', U], skullgreymon: ['skullgreymon:0', U],
  megadramon: ['machinedramon:0', U], myotismon: ['myotismon:0', U],
  rizegreymon: ['agumonsv:1', U, 'RizeGreymon'], aeroveedramon: ['veedramon:1', U, 'AeroVeedramon'], groundramon: ['extra:Groundramon', U, 'Groundramon'],
  wingdramon: ['dracomon:2', U, 'Wingdramon'], cyberdramon: ['monodramon:2', U, 'Cyberdramon'], monzaemon: ['extra:Monzaemon', U, 'Monzaemon'],
  blackkingnumemon: ['numemon:1', U, 'BlackKingNumemon'], platinumnumemon: ['numemon:2', U, 'PlatinumNumemon'], nanimon: ['extra:Nanimon', U, 'Nanimon'],
  superstarmon: ['gotsumon:2', U, 'SuperStarmon'], shakkoumon: ['armadillomon:2', U, 'Shakkoumon'], chirinmon: ['kudamon:2', U, 'Chirinmon'], ladydevimon: ['blackgatomon:1', U, 'LadyDevimon'],
  meicrackmon: ['meicoomon:1', U, 'Meicrackmon VM'], blossomon: ['floramon:2', U, 'Blossomon'], cherrymon: ['woodmon:1', U, 'Cherrymon'], shogungekomon: ['otamamon:2', U, 'ShogunGekomon'],
  matadormon: ['dracmon:2', U, 'Matadormon'], grapleomon: ['bearmon:2', U, 'GrapLeomon'], crescemon: ['lunamon:2', U, 'Crescemon'], okuwamon: ['extra:Okuwamon', U, 'Okuwamon'],
  // Retro (Digimon World) trees: the static DW1 models where they exist. DW1 has no Koromon, Gabumon, Biyomon, Elecmon,
  // Leomon or Numemon model, so those use the Cyber Sleuth Koromon legend and the shop-line models.
  retro_tokomon: ['legend:retro_tokomon', IT], retro_tsunomon: ['legend:retro_tsunomon', IT], dw1_koromon: ['legend:koromon', IT, 'Koromon'],
  dw1_agumon: ['extra:DW1_Agumon', R, 'Agumon'], dw1_patamon: ['extra:DW1_Patamon', R, 'Patamon'], dw1_gabumon: ['gabumon:0', R, 'Gabumon'],
  dw1_biyomon: ['biyomon:0', R, 'Biyomon'], dw1_elecmon: ['elecmon:0', R, 'Elecmon'],
  dw1_greymon: ['extra:DW1_Greymon', CH, 'Greymon'], dw1_meramon: ['extra:DW1_Meramon', CH, 'Meramon'], dw1_birdramon: ['extra:DW1_Birdramon', CH, 'Birdramon'],
  dw1_airdramon: ['extra:DW1_Airdramon', CH, 'Airdramon'], dw1_kokatorimon: ['extra:DW1_Akatorimon', CH, 'Kokatorimon'], dw1_frigimon: ['extra:DW1_Frigimon', CH, 'Frigimon'],
  dw1_bakemon: ['extra:DW1_Bakemon', CH, 'Bakemon'], dw1_tankmon: ['extra:DW1_Tankmon', CH, 'Tankmon'], dw1_leomon: ['elecmon:1', CH, 'Leomon'],
  dw1_numemon: ['numemon:0', CH, 'Numemon'],
  dw1_metalgreymon: ['extra:DW1_MetalGreymon', U, 'MetalGreymon'], dw1_andromon: ['extra:DW1_Andromon', U, 'Andromon'], dw1_mamemon: ['extra:DW1_Mamemon', U, 'Mamemon'],
  dw1_monzaemon: ['extra:DW1_Monzaemon', U, 'Monzaemon'], dw1_vademon: ['extra:DW1_Vademon', U, 'Vademon'], dw1_bluemeramon: ['extra:DW1_BlueMeramon', U, 'BlueMeramon'],
  dw1_gigadramon: ['extra:DW1_Gigadramon', U, 'Gigadramon'], dw1_brachiomon: ['extra:DW1_Brachiomon', U, 'Brachiomon'], dw1_cherrymon: ['extra:DW1_Cherrymon', U, 'Cherrymon'],
  dw1_jijimon: ['extra:DW1_Jijimon', U, 'Jijimon'], phoenixmon: ['extra:Phoenixmon', U, 'Phoenixmon'],
};

// Lines follow Digimon World, Digimon Story and Wikimon evolutions where a model exists. Fresh legends with an
// In-Training model go through it first (Botamon -> Koromon -> Agumon). Numemon and Sukamon are the DW1-style
// poor-care branches (the Wild care stat). Edges are only added, or moved where the form stays reachable, so every
// form a save holds stays on its legend's tree (tests/legendevo.cjs checks this).
const EVO = {
  // In-Training legends and forms
  tokomon: 'patamon:bond biyomon:food goblimon:wild',
  tsunomon: 'gabumon:bond elecmon:battle gizamon:wild',
  motimon: 'tentomon:bond palmon:food keramon:wild',
  bukamon: 'gomamon:bond betamon:battle otamamon:food gizamon:wild',
  tanemon: 'palmon:bond lalamon:food floramon:battle wormmon:wild',
  yokomon: 'biyomon:bond hawkmon:battle palmon:food floramon:wild',
  pagumon: 'demidevimon:bond gizamon:battle dracmon:food goblimon:wild',
  nyaromon: 'salamon:bond lopmon:food renamon:battle candlemon:wild',
  tsumemon: 'keramon:bond demidevimon:battle dracmon:food impmon:wild',
  wanyamon: 'gaomon:bond dorumon:battle terriermon:food bearmon:wild',
  koromon: 'agumon:bond dracomon:battle gabumon:food betamon:wild',
  demiveemon: 'veemon:bond monodramon:battle',
  gigimon: 'guilmon:bond agumon:battle gotsumon:food goblimon:wild',
  gummymon: 'terriermon:bond renamon:battle gaomon:food armadillomon:wild',
  kokomon: 'lopmon:bond wormmon:food',
  dorimon: 'dorumon:bond monodramon:battle armadillomon:food',
  pusurimon: 'kudamon:bond lucemon:battle patamon:food meicoomon:wild',
  upamon: 'armadillomon:bond gizamon:battle patamon:food veemon:wild',
  conomon: 'meicoomon:bond salamon:battle lopmon:food',
  // Fresh legends
  botamon: 'koromon:bond gigimon:battle demiveemon:food',
  punimon: 'gabumon:bond elecmon:battle betamon:food',
  poyomon: 'patamon:bond salamon:food lucemon:battle',
  pabumon: 'tentomon:bond hagurumon:battle palmon:food',
  kuramon: 'keramon:bond impmon:battle candlemon:wild',
  chibomon: 'demiveemon:bond',
  dodomon: 'dorimon:bond',
  jyarimon: 'gigimon:bond',
  zerimon: 'gummymon:bond',
  pusumon: 'pusurimon:bond',
  pichimon: 'bukamon:bond',
  yuramon: 'tanemon:bond',
  yukimibotamon: 'nyaromon:bond',
  // Retro (Digimon World): Botamon -> Koromon -> Agumon, Poyomon -> Tokomon -> Patamon / Biyomon, Punimon -> Tsunomon ->
  // Elecmon / Gabumon, as in DW1; Numemon is the poor-care branch of every DW1 Rookie
  retro_botamon: 'dw1_koromon:bond',
  retro_poyomon: 'retro_tokomon:bond',
  retro_punimon: 'retro_tsunomon:bond',
  dw1_koromon: 'dw1_agumon:bond dw1_gabumon:battle',
  retro_tokomon: 'dw1_patamon:bond dw1_biyomon:food',
  retro_tsunomon: 'dw1_elecmon:bond dw1_gabumon:battle',
  dw1_agumon: 'dw1_greymon:bond dw1_meramon:battle dw1_birdramon:food dw1_numemon:wild',
  dw1_gabumon: 'dw1_frigimon:bond dw1_kokatorimon:battle dw1_tankmon:food dw1_numemon:wild',
  dw1_patamon: 'dw1_leomon:bond dw1_bakemon:battle dw1_airdramon:food dw1_numemon:wild',
  dw1_biyomon: 'dw1_birdramon:bond dw1_airdramon:battle dw1_kokatorimon:food dw1_numemon:wild',
  dw1_elecmon: 'dw1_leomon:bond dw1_bakemon:battle dw1_meramon:food dw1_numemon:wild',
  dw1_greymon: 'dw1_metalgreymon:bond dw1_mamemon:battle dw1_andromon:food',
  dw1_meramon: 'dw1_andromon:bond dw1_bluemeramon:battle dw1_mamemon:food',
  dw1_birdramon: 'phoenixmon:bond dw1_gigadramon:battle',
  dw1_airdramon: 'dw1_gigadramon:bond phoenixmon:food',
  dw1_kokatorimon: 'phoenixmon:bond dw1_brachiomon:battle',
  dw1_frigimon: 'dw1_mamemon:bond dw1_brachiomon:battle dw1_monzaemon:food',
  dw1_bakemon: 'dw1_vademon:bond dw1_cherrymon:food skullgreymon:battle',
  dw1_tankmon: 'dw1_andromon:bond dw1_gigadramon:battle',
  dw1_leomon: 'dw1_andromon:bond dw1_mamemon:battle dw1_jijimon:food',
  dw1_numemon: 'dw1_monzaemon:bond etemon:battle dw1_vademon:wild',
  // Rookie -> Champion
  agumon: 'greymon:bond tyrannomon:battle geogreymon:food numemon:wild',
  gabumon: 'garurumon:bond leomon:battle veedramon:food numemon:wild',
  tentomon: 'kabuterimon:bond kuwagamon:battle guardromon:food numemon:wild',
  palmon: 'togemon:bond sunflowmon:food kuwagamon:battle numemon:wild',
  gomamon: 'ikkakumon:bond seadramon:battle gesomon:wild',
  biyomon: 'birdramon:bond aquilamon:battle veedramon:food numemon:wild',
  hagurumon: 'guardromon:bond sealsdramon:battle',
  demidevimon: 'devimon:bond wizardmon:food ogremon:wild',
  guilmon: 'growlmon:bond tyrannomon:battle veedramon:food',
  renamon: 'kyubimon:bond wizardmon:food',
  terriermon: 'gargomon:bond turuiemon:food',
  patamon: 'angemon:bond leomon:battle veedramon:food ogremon:wild',
  salamon: 'gatomon:bond wizardmon:food blackgatomon:wild',
  betamon: 'seadramon:bond ikkakumon:food gesomon:battle sukamon:wild',
  wormmon: 'stingmon:bond kuwagamon:battle',
  elecmon: 'leomon:bond gaogamon:battle veedramon:food ogremon:wild',
  hawkmon: 'aquilamon:bond birdramon:battle',
  keramon: 'kurisarimon:bond devimon:wild',
  lopmon: 'turuiemon:bond gargomon:food lekismon:battle',
  gizamon: 'gesomon:bond seadramon:battle sukamon:wild',
  candlemon: 'wizardmon:bond devimon:wild',
  veemon: 'exveemon:bond tyrannomon:battle veedramon:food',
  dorumon: 'dorugamon:bond growlmon:battle coredramon:food',
  gaomon: 'gaogamon:bond leomon:battle garurumon:food',
  lalamon: 'sunflowmon:bond togemon:food',
  commandramon: 'sealsdramon:bond guardromon:food',
  goblimon: 'ogremon:bond tyrannomon:battle',
  impmon: 'devimon:bond wizardmon:food ogremon:wild',
  lucemon: 'angemon:bond gatomon:food devimon:wild',
  dracomon: 'coredramon:bond veedramon:battle growlmon:food',
  monodramon: 'strikedramon:bond veedramon:battle coredramon:food',
  gotsumon: 'starmon:bond guardromon:battle ogremon:wild',
  armadillomon: 'ankylomon:bond guardromon:battle',
  kudamon: 'reppamon:bond leomon:battle angemon:food',
  meicoomon: 'gatomon:bond blackgatomon:wild',
  floramon: 'kiwimon:bond togemon:food woodmon:wild',
  otamamon: 'gekomon:bond seadramon:battle',
  dracmon: 'sangloupmon:bond devimon:battle bakemon:wild',
  bearmon: 'grizzlymon:bond leomon:battle',
  // Champion -> Ultimate
  greymon: 'metalgreymon:bond groundramon:battle skullgreymon:wild',
  tyrannomon: 'metalgreymon:bond megadramon:battle',
  ogremon: 'andromon:food etemon:wild',
  garurumon: 'weregarurumon:bond skullgreymon:wild',
  leomon: 'panjyamon:bond weregarurumon:battle',
  kabuterimon: 'megakabuterimon:bond andromon:food',
  kuwagamon: 'dinobeemon:bond okuwamon:battle megakabuterimon:food',
  guardromon: 'andromon:bond tankdramon:battle',
  togemon: 'lillymon:bond lilamon:food',
  sunflowmon: 'lilamon:bond lillymon:food',
  ikkakumon: 'zudomon:bond megaseadramon:battle',
  seadramon: 'megaseadramon:bond marinedevimon:wild',
  gesomon: 'marinedevimon:bond megaseadramon:battle',
  birdramon: 'garudamon:bond silphymon:food',
  aquilamon: 'silphymon:bond garudamon:battle',
  kurisarimon: 'infermon:bond megadramon:battle',
  sealsdramon: 'tankdramon:bond andromon:food',
  devimon: 'neodevimon:bond myotismon:battle',
  wizardmon: 'mistymon:bond myotismon:wild',
  growlmon: 'wargrowlmon:bond megadramon:wild',
  kyubimon: 'taomon:bond angewomon:food',
  gargomon: 'rapidmon:bond andiramon:food',
  turuiemon: 'andiramon:bond rapidmon:food',
  angemon: 'magnaangemon:bond angewomon:food',
  gatomon: 'angewomon:bond magnaangemon:food',
  stingmon: 'dinobeemon:bond paildramon:battle',
  exveemon: 'paildramon:bond megadramon:battle',
  dorugamon: 'dorugreymon:bond megadramon:wild',
  gaogamon: 'machgaogamon:bond weregarurumon:battle',
  geogreymon: 'rizegreymon:bond metalgreymon:battle skullgreymon:wild',
  veedramon: 'aeroveedramon:bond metalgreymon:battle cyberdramon:food',
  coredramon: 'groundramon:bond wingdramon:battle',
  strikedramon: 'cyberdramon:bond wargrowlmon:battle',
  numemon: 'monzaemon:bond etemon:battle blackkingnumemon:food platinumnumemon:wild',
  sukamon: 'etemon:bond nanimon:battle platinumnumemon:wild',
  starmon: 'superstarmon:bond andromon:battle',
  ankylomon: 'shakkoumon:bond zudomon:battle groundramon:food',
  reppamon: 'chirinmon:bond magnaangemon:food',
  blackgatomon: 'ladydevimon:bond angewomon:food meicrackmon:wild',
  kiwimon: 'blossomon:bond lillymon:food',
  woodmon: 'cherrymon:bond blossomon:food',
  gekomon: 'shogungekomon:bond megaseadramon:battle',
  sangloupmon: 'matadormon:bond myotismon:battle',
  grizzlymon: 'grapleomon:bond panjyamon:battle',
  bakemon: 'myotismon:bond neodevimon:battle',
  lekismon: 'crescemon:bond andiramon:food',
};
Object.assign(FORMS, {
  ginryumon:['ryudamon:1',CH,'Ginryumon'],hisyaryumon:['ryudamon:2',U,'Hisyaryumon'],
  peckmon:['falcomon:1',CH,'Peckmon'],crowmon:['falcomon:2',U,'Crowmon'],
  dobermon:['ac:Dobermon',CH,'Dobermon'],musyamon:['ac:Musyamon',CH,'Musyamon'],grademon:['drg:Grademon',U,'Grademon'],
  firamon:['coronamon:1',CH,'Firamon'],flaremon:['coronamon:2',U,'Flaremon'],
  waspmon:['fanbeemon:1',CH,'Waspmon'],cannonbeemon:['fanbeemon:2',U,'CannonBeemon'],
  coelamon:['gizamon:1',CH,'Coelamon'],frigimon:['extra:DW1_Frigimon',CH,'Frigimon'],mammothmon:['ac:Mammothmon',U,'Mammothmon'],
});
Object.assign(EVO, {
  ryudamon:'ginryumon:bond coredramon:battle',ginryumon:'hisyaryumon:bond',
  falcomon:'peckmon:bond aquilamon:battle',hyokomon:'peckmon:bond birdramon:battle',peckmon:'crowmon:bond garudamon:battle',
  gazimon:'dobermon:bond garurumon:battle',dobermon:'weregarurumon:bond',
  kotemon:'musyamon:bond guardromon:battle',musyamon:'grademon:bond',
  coronamon:'firamon:bond',firamon:'flaremon:bond',lunamon:'lekismon:bond',
  fanbeemon:'waspmon:bond kabuterimon:battle',waspmon:'cannonbeemon:bond',
  syakomon:'coelamon:bond seadramon:battle',crabmon:'coelamon:bond ikkakumon:battle',coelamon:'megaseadramon:bond zudomon:battle',
  penguinmon:'frigimon:bond seadramon:battle',frigimon:'mammothmon:bond zudomon:battle',
});
const KIDS = {};
for (const [f, s] of Object.entries(EVO)) KIDS[f] = s.split(/\s+/).map((x) => x.split(':'));
// Discovered companions share the care branches of their species, with independent saved progress.
const children = id => KIDS[C.get('legend',id)?.data?.evoRoot || id] || [];

// ---------- economy ----------
const MAX_LEVEL = 10;
const DATA_FOR = [0, 100, 100, 150, 200, 200, 250, 300, 300, 400]; // Digi-Data to go from level i to i+1 (2000 to max)
const TIER_LEVEL = [1, 3, 6, 10]; // level needed to digivolve into tier t
const IT_LEVEL = 2; // level needed for a Fresh legend to digivolve into an In-Training form
const DUP_DATA = { common: 100, uncommon: 160, rare: 250, epic: 600, legendary: 1200 };
const BITS_PER_DATA = 0.6; // buying a level: 1200 Bits from level 1 to 10
const GAME_DATA = 25, TOP4_DATA = 25; // Digi-Data for the equipped legend per finished game
const FEED_BITS = 30, REVERT_BITS = 50;
const CARE_CAP = 20, CARE_BASE = 20, CARE_PER = 4, WILD_BASE = 24, WILD_MIN = 4;
const CARE = {
  bond: { name: 'Bond', how: 'Games played with it equipped' },
  battle: { name: 'Battle', how: 'Top 4 finishes (1st counts triple)' },
  food: { name: 'Food', how: `Feed it for ${FEED_BITS} Bits` },
  wild: { name: 'Wild', how: 'Likely with little care; rarer as care grows' },
};

// ---------- save ----------
let save = { v: 1, legends: {}, games: [] };
try { const s = JSON.parse(localStorage.getItem(KEY)); if (s && typeof s === 'object' && s.legends) save = Object.assign(save, s); } catch (e) { /* blocked or corrupt */ }
const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(save)); } catch (e) { /* private mode */ } };
const listeners = new Set();
function changed(id) {
  persist();
  for (const fn of listeners) { try { fn(id); } catch (e) { console.warn(e); } }
  // the renderer, title card and Tamers list redraw on a 'legend' change
  if (C.notify) C.notify('legend');
}

const hash = (s) => { let h = 2166136261; for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 16777619) >>> 0; } return h; };
// one mulberry32 step: spreads the FNV hash evenly over 0..1
function rand01(seed) {
  let t = (seed + 0x6d2b79f5) >>> 0;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}

const isLegend = (id) => !!(id && C.get('legend', id));
function rec(id) {
  let r = save.legends[id];
  if (!r) r = save.legends[id] = { xp: 0, form: id, path: [], nonce: 0, dex: [id], care: { bond: 0, battle: 0, food: 0 } };
  if (!r.care) r.care = { bond: 0, battle: 0, food: 0 };
  if (!Array.isArray(r.path)) r.path = [];
  if (!Array.isArray(r.dex)) r.dex = [id];
  if (!r.form || !reach(id).has(r.form)) { r.form = id; r.path = []; }
  return r;
}
// read-only view for legends with no record yet (doesn't create one)
const peek = (id) => save.legends[id] ? rec(id) : { xp: 0, form: id, path: [], nonce: 0, dex: [id], care: { bond: 0, battle: 0, food: 0 } };

function levelOf(xp) {
  let lv = 1, left = xp;
  while (lv < MAX_LEVEL && left >= DATA_FOR[lv]) { left -= DATA_FOR[lv]; lv++; }
  return { level: lv, data: lv >= MAX_LEVEL ? 0 : left, need: lv >= MAX_LEVEL ? 0 : DATA_FOR[lv] };
}
const XP_MAX = DATA_FOR.reduce((a, b) => a + b, 0);

// ---------- forms ----------
const stageOf = (id) => { const it = C.get('legend', id); return it?.data?.level || (it && /^Fresh/.test(it.desc || '') ? 'Fresh' : 'In-Training'); };
const TIER = { Fresh: 0, 'In-Training': 0, Rookie: 1, Champion: 2, Ultimate: 3 };
const RANK = { Fresh: 0, 'In-Training': 1, Rookie: 2, Champion: 3, Ultimate: 4 }; // row order in the tree
function formInfo(formId) {
  if (FORMS[formId]) {
    const [model, level, nm] = FORMS[formId];
    const [line, st] = model.split(':');
    if (line === 'legend') {
      const it = C.get('legend', st);
      return { id: formId, name: it ? it.name : nm || st, level, tier: TIER[level], rank: RANK[level], legend: st };
    }
    const L = DT.LINE && DT.LINE[line];
    const extra = line === 'extra';
    return { id: formId, name: nm || (L ? L.forms[Number(st)] : formId), level, tier: TIER[level], rank: RANK[level], model, line, stage: extra ? st : Number(st) };
  }
  const it = C.get('legend', formId);
  if (!it) return null;
  const level = stageOf(formId);
  return { id: formId, name: it.name, level, tier: TIER[level], rank: RANK[level], legend: formId };
}
// level needed to digivolve into this form
const entryLevel = (f) => (f.tier > 0 ? TIER_LEVEL[f.tier] : IT_LEVEL);

const reachCache = new Map();
function reach(id) {
  if (reachCache.has(id)) return reachCache.get(id);
  const seen = new Set([id]);
  const q = [id];
  while (q.length) for (const [k] of children(q.shift())) if (!seen.has(k) && FORMS[k]) { seen.add(k); q.push(k); }
  reachCache.set(id, seen);
  return seen;
}

// Rows by level (Fresh, In-Training, Rookie, Champion, Ultimate; empty rows dropped). Within a row, forms sit under
// the average position of their parents so the edges cross less.
function tree(id) {
  const order = [id];
  const seen = new Set([id]);
  const edges = [];
  const parents = {};
  for (let i = 0; i < order.length; i++) for (const [k] of children(order[i])) {
    if (!FORMS[k]) continue;
    edges.push([order[i], k]);
    (parents[k] = parents[k] || []).push(order[i]);
    if (!seen.has(k)) { seen.add(k); order.push(k); }
  }
  const rankOf = (f) => { const fi = formInfo(f); return fi && fi.rank != null ? fi.rank : 0; };
  const byRank = new Map();
  for (const f of order) { const r = f === id ? -1 : Math.max(rankOf(f), rankOf(id) + 1); if (!byRank.has(r)) byRank.set(r, []); byRank.get(r).push(f); }
  const ranks = [...byRank.keys()].sort((a, b) => a - b);
  const pos = {};
  const tiers = ranks.map((r) => {
    const row = byRank.get(r);
    const at = (f) => { const ps = (parents[f] || []).filter((p) => pos[p] != null); return ps.length ? ps.reduce((n, p) => n + pos[p], 0) / ps.length : 0; };
    const sorted = row.map((f, i) => [f, at(f), i]).sort((a, b) => a[1] - b[1] || a[2] - b[2]).map((x) => x[0]);
    sorted.forEach((f, i) => { pos[f] = sorted.length > 1 ? i / (sorted.length - 1) : 0.5; });
    return sorted;
  });
  const lv = (f) => (formInfo(f) || {}).level;
  return { root: id, tiers, levels: tiers.map((row) => lv(row[0])), edges };
}

// ---------- odds ----------
// Extra care from other systems (the Digi-Farm, js/farm.js): fn(id) -> { bond, battle, food, wild, note } or null.
// bond / battle / food add to the saved care (still capped at CARE_CAP); wild adds straight to the Wild branch weight.
const careSources = [];
function careBonus(id) {
  const b = { bond: 0, battle: 0, food: 0, wild: 0, notes: [] };
  if (!id) return b;
  for (const fn of careSources) {
    let x = null;
    try { x = fn(id); } catch (e) { console.warn(e); }
    if (!x) continue;
    for (const k of ['bond', 'battle', 'food', 'wild']) b[k] += Math.max(0, Number(x[k]) || 0);
    if (x.note) b.notes.push(String(x.note));
  }
  return b;
}
function weightsFor(r, from, id) {
  const cap = (n) => Math.min(CARE_CAP, Math.max(0, n || 0));
  const bonus = careBonus(id);
  const care = { bond: cap((r.care.bond || 0) + bonus.bond), battle: cap((r.care.battle || 0) + bonus.battle), food: cap((r.care.food || 0) + bonus.food) };
  const total = care.bond + care.battle + care.food;
  return children(from).filter(([k]) => FORMS[k]).map(([form, c]) => ({
    form, care: c, weight: c === 'wild' ? Math.max(WILD_MIN, WILD_BASE - total) + bonus.wild : CARE_BASE + CARE_PER * care[c],
  }));
}
// whole percents that add up to exactly 100 (largest remainder)
function withPct(ws) {
  const sum = ws.reduce((n, w) => n + w.weight, 0) || 1;
  const raw = ws.map((w) => (100 * w.weight) / sum);
  const out = ws.map((w, i) => ({ ...w, pct: Math.floor(raw[i]) }));
  let left = 100 - out.reduce((n, w) => n + w.pct, 0);
  const order = raw.map((x, i) => [x - Math.floor(x), i]).sort((a, b) => b[0] - a[0]);
  for (let j = 0; left > 0 && order.length; j = (j + 1) % order.length, left--) out[order[j][1]].pct++;
  return out;
}
function odds(id) { const r = peek(id); return withPct(weightsFor(r, r.form, id)); }

// The roll for the next digivolution. Seeded from the legend id, its level, its form and the stored nonce,
// so reloading the page always gives the same result; a revert bumps the nonce.
function roll(id) {
  const r = peek(id);
  const ws = weightsFor(r, r.form, id);
  if (!ws.length) return null;
  const { level } = levelOf(r.xp);
  const x = rand01(hash(`${id}|${level}|${r.form}|${r.nonce}`)) * ws.reduce((n, w) => n + w.weight, 0);
  let acc = 0;
  for (const w of ws) { acc += w.weight; if (x < acc) return w.form; }
  return ws[ws.length - 1].form;
}

function nextLevel(id) {
  const r = peek(id);
  const f = formInfo(r.form);
  const ws = f ? weightsFor(r, r.form, id) : [];
  if (!ws.length) return null;
  return Math.max(...ws.map((w) => entryLevel(formInfo(w.form))));
}

function info(id) {
  if (!isLegend(id)) return null;
  const r = peek(id);
  const lv = levelOf(r.xp);
  const next = nextLevel(id);
  const owned = C.owns('legend', id);
  return {
    id, owned, form: r.form, formInfo: formInfo(r.form), base: formInfo(id), path: r.path.slice(), dex: r.dex.slice(),
    care: { ...r.care }, careBonus: careBonus(id), careCap: CARE_CAP, level: lv.level, data: lv.data, need: lv.need, maxLevel: MAX_LEVEL, xp: r.xp,
    next, canDigivolve: owned && next != null && lv.level >= next, canRevert: owned && r.path.length > 0,
    odds: odds(id), levelBits: owned && lv.level < MAX_LEVEL ? bitsFor(lv.need - lv.data) : 0, feedBits: FEED_BITS, revertBits: REVERT_BITS,
    key: key(id),
  };
}
const bitsFor = (data) => Math.max(5, Math.ceil((data * BITS_PER_DATA) / 5) * 5);

// ---------- actions ----------
const W = () => DT.Wallet;
function addData(id, n) {
  const r = rec(id);
  const before = levelOf(r.xp).level;
  const room = XP_MAX - r.xp;
  const used = Math.max(0, Math.min(room, Math.round(n)));
  r.xp += used;
  return { used, extra: Math.round(n) - used, before, level: levelOf(r.xp).level };
}

const LegendEvo = {
  FORMS, EVO, CARE, MAX_LEVEL, DATA_FOR, TIER_LEVEL, IT_LEVEL, DUP_DATA, BITS_PER_DATA, FEED_BITS, REVERT_BITS, GAME_DATA, TOP4_DATA,
  info, odds, tree, formInfo, reach: (id) => [...reach(id)],
  formOf(id) { const r = save.legends[id]; return r && r.form && r.form !== id && reach(id).has(r.form) ? r.form : null; },
  key, parse,
  digivolve(id) {
    const i = info(id);
    if (!i || !i.canDigivolve) return null;
    const to = roll(id);
    if (!to) return null;
    const r = rec(id);
    const from = r.form;
    r.path.push(from);
    r.form = to;
    if (!r.dex.includes(to)) r.dex.push(to);
    changed(id);
    return { from, to };
  },
  revert(id) {
    const r = save.legends[id];
    if (!r || !r.path.length || !C.owns('legend', id)) return null;
    if (!W().spend(REVERT_BITS)) return false;
    const from = r.form;
    r.form = r.path.pop();
    r.nonce = (r.nonce || 0) + 1;
    changed(id);
    return { from, to: r.form };
  },
  // buys the rest of the current level with Bits
  buyLevel(id) {
    const i = info(id);
    if (!i || !i.owned || i.level >= MAX_LEVEL) return null;
    if (!W().spend(i.levelBits)) return false;
    const res = addData(id, i.need - i.data);
    changed(id);
    return res;
  },
  feed(id) {
    if (!C.owns('legend', id)) return null;
    const r = rec(id);
    if (r.care.food >= CARE_CAP) return null;
    if (!W().spend(FEED_BITS)) return false;
    r.care.food++;
    changed(id);
    return r.care.food;
  },
  // a duplicate from an egg becomes Digi-Data; null when the legend is already max level (the store refunds Bits then)
  addDuplicate(id) {
    if (!isLegend(id)) return null;
    if (peek(id).xp >= XP_MAX) return null;
    const it = C.get('legend', id);
    const n = DUP_DATA[it.rarity] || DUP_DATA.common;
    const res = addData(id, n);
    changed(id);
    return { data: res.used, level: res.level, levels: res.level - res.before, extraData: res.extra };
  },
  // once per finished game, for the legend equipped now (DT.Shop.award calls this)
  recordGame(gameKey, place) {
    if (!place || save.games.includes(gameKey)) return null;
    const id = C.equipped('legend');
    if (!isLegend(id)) return null;
    save.games = [gameKey, ...save.games].slice(0, 60);
    const r = rec(id);
    r.care.bond++;
    if (place <= 4) r.care.battle += place === 1 ? 3 : 1;
    const res = addData(id, GAME_DATA + (place <= 4 ? TOP4_DATA : 0));
    changed(id);
    return { legend: id, data: res.used, level: res.level };
  },
  look(l) {
    if (!l || !l.legend) return l;
    return { ...l, legend: key(String(l.legend).split('@')[0], l.legendForm || null) };
  },
  aiForm(id, seed) {
    if (!isLegend(id)) return null;
    const h = hash(`${seed}|evo`);
    const depth = [1, 2, 2, 3, 3][h % 5];
    let f = id;
    for (let d = 0; d < depth; d++) {
      const kids = children(f).filter(([k]) => FORMS[k]);
      if (!kids.length) break;
      f = kids[hash(`${seed}|${d}`) % kids.length][0];
    }
    return f === id ? null : f;
  },
  onChange(fn) { listeners.add(fn); return () => listeners.delete(fn); },
  _roll: roll,
  addCareSource(fn) { careSources.push(fn); return () => { const i = careSources.indexOf(fn); if (i >= 0) careSources.splice(i, 1); }; },
  careBonus,
  // Digi-Data from other systems (the Digi-Farm's daily rewards); returns how much was used
  grantData(id, n) {
    if (!C.owns('legend', id) || !(n > 0)) return 0;
    const res = addData(id, n);
    if (res.used) changed(id);
    return res.used;
  },
  // something outside changed the odds (farm care): redraw panels without touching the save
  refresh(id) { for (const fn of listeners) { try { fn(id); } catch (e) { console.warn(e); } } },
  _reset(id) { delete save.legends[id]; changed(id); },
};

// 'id@form' when the form is a valid evolved form of that legend, else the plain id
function key(id, form) {
  if (!id) return id;
  id = String(id).split('@')[0];
  if (form === undefined) form = LegendEvo.formOf(id);
  return form && form !== id && isLegend(id) && reach(id).has(form) ? `${id}@${form}` : id;
}
function parse(k) {
  const [legend, form] = String(k || '').split('@');
  return { legend, form: form && FORMS[form] ? form : null };
}

DT.LegendEvo = LegendEvo;
