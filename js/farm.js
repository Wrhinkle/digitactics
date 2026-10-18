// Digi-Farm: a Digimon World Championship-style care side game for the Little Legends you own. Rules and save only;
// js/farm-ui.js draws the room and js/farm-games.js runs the training mini-games.
// Client-only and outside the lockstep rules, like the rest of the cosmetics. Saved in localStorage 'dt.farm.v1'
// (separate from 'dt.cosmetics.v1' and 'dt.legendevo.v1', so older saves are untouched).
//
// Care runs on real time. Each resident's stats are simulated minute by minute from its last timestamp, so the farm
// keeps going while the page is closed. A catch-up covers at most CATCHUP_MIN minutes and adds at most
// OFFLINE_MISTAKES care mistakes, so nobody comes back to a wrecked pet. Nothing dies: at worst a legend is sad or sick.
// Legends in the Digi-Bank are paused.
//
// DT.Farm.state()                  { cap, residents, bank, theme, lights, meat, bandage, daily, ... }
// DT.Farm.pet(id)                  one legend's farm stats (a copy), after catching up
// DT.Farm.card(id)                 everything the per-legend card shows
// DT.Farm.feed(id, food) / clean(id) / cure(id, 'bandage'|'medicine') / setLights(on) / praise(id) / scold(id)
//         play(id, otherId?) / pet(id) / train(id, game, score 0..1) / canTrain(id)
//         -> { ok, msg, reward?: { bits, data }, ... }
// DT.Farm.moveIn(id) / moveOut(id) / swap(inId, outId) / buyExpansion() / buyTheme(id) / setTheme(id)
// DT.Farm.evoCare(id)              what the farm adds to the legendevo care (also registered with addCareSource)
// DT.Farm.drainEvents()            [{ id, type, ... }] since the last call: poop, toilet, sick, mistake, tantrum, ...
// DT.Farm.onChange(fn)
// DT.Farm._advance(ms)             test clock: moves farm time forward
const DT = window.DT;
const C = DT.Cosmetics;
const W = DT.Wallet;
const E = DT.LegendEvo;
const KEY = 'dt.farm.v1';

// ---------- numbers ----------
const MIN = 60000;
const CAP_START = 6, CAP_MAX = 12, CAP_STEP = 2;
const EXPAND_COST = [250, 400, 600]; // 6 -> 8 -> 10 -> 12
const CATCHUP_MIN = 2 * 60; // biological decline only; supplies and age retain real elapsed time
const CARETAKERS = [
  { id: 'basic', name: 'Basic caretaker', price: 100, needs: 40 },
  { id: 'full', name: 'Full care', price: 750, needs: 100 },
];
const CARE_DAY = 86400000;
const OFFLINE_MISTAKES = 2; // max care mistakes added by one catch-up longer than 30 minutes
// per-minute rates (stats run 0..100)
const RATE = {
  hunger: 100 / 600, // awake: full to empty in 10 h (half speed asleep)
  energy: 100 / 720, // awake: 12 h to empty
  sleep: 100 / 120, // lights off: empty to full in 2 h
  nap: 100 / 240, // collapsed with the lights on: half speed
  happy: 100 / 900, // 15 h to empty awake
  happyAsleep: 100 / 2400,
  disc: 100 / 3000,
  hungryHappy: 1 / 6, sickHappy: 1 / 6, poopHappy: 1 / 20, fatHappy: 1 / 30, boredHappy: 1 / 10,
};
const POOP_EVERY = 180; // awake minutes between poops (asleep counts half)
const POOP_MAX = 4; // piles per legend
const TOILET_DISC = 60; // at this discipline a legend uses the toilet corner instead of the floor
const DIRT_SICK = 240; // pile-minutes on the floor before sickness (1 pile for 4 h, 2 for 2 h, ...)
const PILE_NEGLECT = 180; // a pile older than this counts as poop neglect (once per pile)
const STARVE_MISTAKE = 60, STARVE_SICK = 180, SAD_MISTAKE = 120, SLEEPY_MISTAKE = 60, SICK_MISTAKE = 360;
const TANTRUM_MIN = 30; // minutes a tantrum lasts; ignoring it is a care mistake
const TANTRUM_CHANCE = 0.004; // per awake minute at discipline 0, scaled by (1 - discipline/100)^2
const SLEEPY_AT = 20, NAP_AT = 5, NAP_WAKE = 50;
const IDEAL_WEIGHT = [10, 20, 30, 40]; // by tier: baby, rookie, champion, ultimate (G)
const FAT = 1.4, THIN = 0.7;
const TRAIN_CAP = 300;
const MEAT_MAX = 3, MEAT_EVERY = 120, MEAT_BITS = 5; // free meat: +1 every 2 h, up to 3; 5 Bits when the tray is empty
const BANDAGE_MAX = 2, BANDAGE_EVERY = 360, MEDICINE_BITS = 20;
const DAILY = { bits: 60, data: 100 };
const PET_COOLDOWN = 15000, PRAISE_COOLDOWN = 10 * MIN;

// Foods. hunger / weight / happy / energy / disc are added; sweet foods can be fed when full.
const FOODS = {
  meat: { name: 'Meat', icon: '🍖', bits: 0, hunger: 30, weight: 1, desc: `Free from the tray (+1 every ${MEAT_EVERY / 60} h, up to ${MEAT_MAX}). ${MEAT_BITS} Bits when it is empty.` },
  carrot: { name: 'Supercarrot', icon: '🥕', bits: 8, hunger: 25, weight: 0, energy: 6, healthy: true, desc: 'Fills without weight. Good for a chubby legend.' },
  bigmeat: { name: 'Giant Meat', icon: '🍗', bits: 15, hunger: 70, weight: 3, happy: 4, desc: 'Fills it right up. Heavy.' },
  sirloin: { name: 'Sirloin', icon: '🥩', bits: 25, hunger: 50, weight: 2, happy: 15, energy: 10, desc: 'A treat: happiness and energy.' },
  cake: { name: 'Digi-Cake', icon: '🍰', bits: 10, hunger: 10, weight: 3, happy: 20, disc: -5, sweet: true, desc: 'Very happy, but spoils it and adds weight. Fits even when full.' },
};
const TRAIN = {
  strength: { name: 'Strength', stat: 'str', game: 'Power Swing' },
  speed: { name: 'Speed', stat: 'spd', game: 'Beat Dash' },
  wisdom: { name: 'Wisdom', stat: 'wis', game: 'Data Memory' },
  stamina: { name: 'Stamina', stat: 'sta', game: 'Endurance Run' },
};
const THEMES = [
  { id: 'meadow', name: 'File Island Meadow', free: true, icon: '🌼', desc: 'Grass, flowers and a quiet pond.' },
  { id: 'tropical', name: 'Tropical Isle', price: 300, board: 'file-beach', icon: '🏝️', desc: 'Sand, palms and a turquoise sea. Free if you own the File Island Beach board.' },
  { id: 'snowy', name: 'Freezeland', price: 300, board: 'infinity-mountain', icon: '❄️', desc: 'Snow, pines and falling flakes. Free if you own the Infinity Mountain board.' },
  { id: 'sewer', name: 'Numemon Sewer', price: 200, icon: '🕳️', desc: 'Pipes, puddles and a lot of green. Free if you own the Dark Ocean board.', board: 'dark-ocean' },
  { id: 'toy', name: 'Toy Town Yard', price: 400, board: 'toy-town', icon: '🧸', desc: 'Blocks and candy colors. Free if you own the Toy Town board.' },
  { id: 'grid', name: 'Digital Grid', price: 600, board: 'digital-grid', icon: '💠', desc: 'Glowing data floor. Free if you own the Digital Grid board.' },
];
// rewards per action, before the daily cap
const REWARD = { feed: [2, 4], clean: [3, 5], cure: [3, 5], praise: [2, 3], scold: [3, 4], play: [2, 3] };

// ---------- save ----------
let save = null;
function fresh() {
  return { v: 1, cap: CAP_START, theme: 'meadow', themes: ['meadow'], residents: [], seen: [], lights: true, pets: {},
    meat: { n: MEAT_MAX, t: 0 }, bandage: { n: BANDAGE_MAX, t: 0 }, daily: { day: '', bits: 0, data: 0 } };
}
try {
  const s = JSON.parse(localStorage.getItem(KEY));
  if (s && typeof s === 'object' && s.pets) save = Object.assign(fresh(), s);
} catch (e) { /* blocked or corrupt */ }
if (!save) save = fresh();
const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(save)); } catch (e) { /* private mode */ } };

let offset = 0; // test clock
const now = () => Math.max(Date.now() + offset, save.caretaker?.clock || 0);
const listeners = new Set();
let events = [];
const push = (ev) => { events.push(ev); if (events.length > 200) events = events.slice(-200); };
function changed(id) {
  persist();
  for (const fn of listeners) { try { fn(id); } catch (e) { console.warn(e); } }
  if (E && E.refresh) E.refresh(id);
}

// ---------- helpers ----------
const clamp = (x, a, b) => Math.max(a, Math.min(b, x));
const hash = (s) => { let h = 2166136261; for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 16777619) >>> 0; } return h; };
function rand01(seed) {
  let t = (seed + 0x6d2b79f5) >>> 0;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}
const owned = () => C.list('legend').filter((x) => C.owns('legend', x.id)).map((x) => x.id);
// current form without E.info (info reads the odds, which call back into evoCare)
const formNow = (id) => (E ? E.formInfo(E.formOf(id) || id) : null);
const nameOf = (id) => { const f = formNow(id); return f ? f.name : (C.get('legend', id) || {}).name || id; };
const tierOf = (id) => { const f = formNow(id); return f ? f.tier || 0 : 0; };
const idealOf = (id) => IDEAL_WEIGHT[clamp(tierOf(id), 0, 3)];
const dayKey = (t) => { const d = new Date(t); return `${d.getFullYear()}-${d.getMonth() + 1}-${d.getDate()}`; };

function newPet(id, t) {
  const w = idealOf(id);
  return { born: t, t, hunger: 80, energy: 90, happy: 70, disc: 30, weight: w, poop: [], poopClock: Math.floor(rand01(hash(id)) * 90),
    dirt: 0, sick: false, sickWhy: '', sickMin: 0, mistakes: 0, poopNeglect: 0, overfeed: 0, refusals: 0, goodBond: 0, goodFood: 0,
    str: 0, spd: 0, wis: 0, sta: 0, trained: 0, tantrum: 0, nap: false, starveMin: 0, sadMin: 0, sleepyMin: 0,
    flags: {}, lastPraise: 0, lastPet: 0, toilet: 0, pileN: 0 };
}

// ---------- room ----------
function syncRoom() {
  const own = owned();
  const t = now();
  save.cap = clamp(save.cap | 0 || CAP_START, CAP_START, CAP_MAX);
  save.residents = save.residents.filter((id, i, a) => own.includes(id) && a.indexOf(id) === i).slice(0, save.cap);
  // newly owned legends move in while there is room; the equipped one first
  const eq = C.equipped('legend');
  const order = [eq, ...own.filter((id) => id !== eq)].filter((id) => own.includes(id));
  for (const id of order) {
    if (save.seen.includes(id)) continue;
    save.seen.push(id);
    if (save.residents.length < save.cap && !save.residents.includes(id)) save.residents.push(id);
  }
  for (const id of save.residents) if (!save.pets[id]) save.pets[id] = newPet(id, t);
}

// ---------- simulation ----------
function stock(s, max, every, t) {
  if (!s.t) s.t = t;
  if (s.n >= max) { s.t = t; return; }
  const add = Math.floor((t - s.t) / (every * MIN));
  if (add > 0) { s.n = Math.min(max, s.n + add); s.t = s.n >= max ? t : s.t + add * every * MIN; }
}

function mistake(id, p, why, budget) {
  if (budget.left <= 0) return;
  budget.left--;
  p.mistakes++;
  push({ id, type: 'mistake', why });
}

const caretakerAt = (time) => save.caretaker && time >= save.caretaker.since && time < save.caretaker.until
  ? CARETAKERS.find((plan) => plan.id === save.caretaker.tier) || null : null;
const caretakerOf = () => caretakerAt(now());
const caretakerQuote = (id) => {
  const plan = CARETAKERS.find((x) => x.id === id);
  if (!plan) return null;
  const current = caretakerOf(), time = now(), until = save.caretaker?.until || 0;
  if (current && current.price > plan.price) return { price: 0, until, upgrade: false, blocked: true };
  if (current && current.price < plan.price) return { price: Math.ceil((plan.price - current.price) * ((until - time) / CARE_DAY)), until, upgrade: true, blocked: false };
  return { price: plan.price, until: Math.max(time, until) + CARE_DAY, upgrade: false, blocked: false };
};
function maintain(p, plan, ideal) {
  if (!plan) return;
  for (const field of ['hunger', 'energy', 'happy']) p[field] = Math.max(p[field], plan.needs);
  if (plan.id === 'full') { p.disc = 100; p.weight = ideal; p.nap = false; }
  p.poop.length = 0;
  for (const field of ['dirt', 'sickMin', 'starveMin', 'sadMin', 'sleepyMin', 'tantrum']) p[field] = 0;
  p.sick = false; p.sickWhy = '';
  p.flags.asleep = (!save.lights && p.energy < 100) || p.nap;
}
function hireCaretaker(id) {
  tick();
  const plan = CARETAKERS.find((x) => x.id === id);
  if (!plan) return fail('Unknown caretaker plan.');
  const quote = caretakerQuote(id), paid = quote.price;
  if (quote.blocked) return fail('Full care is already active. Choose Basic after it expires.');
  if (!W.spend(paid)) return fail(`Needs ${paid} Bits.`);
  const time = now(); save.caretaker = { tier: id, since: time, until: quote.until, clock: time };
  for (const resident of save.residents) { maintain(save.pets[resident], plan, idealOf(resident)); save.pets[resident].t = time; }
  changed(null);
  return { ok: true, msg: plan.name + (quote.upgrade ? ' upgraded for the remaining paid time.' : ' extended by 24 hours.'), paid, until: quote.until };
}

function simulate(id, p, t) {
  if (!p.t) p.t = t;
  const ideal = idealOf(id);
  let from = p.t;
  if (t - from > CATCHUP_MIN * MIN) from = t - CATCHUP_MIN * MIN; // older time is simply skipped
  if (save.caretaker && p.t < save.caretaker.until && from >= save.caretaker.until) maintain(p, caretakerAt(save.caretaker.until - 1), ideal);
  const mins = Math.floor((t - from) / MIN);
  if (mins <= 0) { maintain(p, caretakerAt(t), ideal); return; }
  const budget = { left: mins > 30 ? OFFLINE_MISTAKES : 99 };
  for (let m = 0; m < mins; m++) {
    const mt = from + m * MIN;
    const mi = Math.floor(mt / MIN);
    const caretaker = caretakerAt(mt); maintain(p, caretaker, ideal);
    // sleep
    if (!save.lights) p.nap = false;
    else if (p.energy <= NAP_AT) p.nap = true;
    else if (p.nap && p.energy >= NAP_WAKE) p.nap = false;
    const asleep = (!save.lights && p.energy < 100) || p.nap;
    p.flags.asleep = asleep;
    // energy
    if (asleep) p.energy += save.lights ? RATE.nap : RATE.sleep;
    else p.energy -= RATE.energy;
    // hunger
    p.hunger -= asleep ? RATE.hunger / 2 : RATE.hunger;
    // happiness
    let dh = asleep ? RATE.happyAsleep : RATE.happy;
    if (p.hunger < 20) dh += RATE.hungryHappy;
    if (p.sick) dh += RATE.sickHappy;
    dh += p.poop.length * RATE.poopHappy;
    if (p.weight >= ideal * FAT) dh += RATE.fatHappy;
    if (!save.lights && !asleep) dh += RATE.boredHappy;
    p.happy -= dh;
    if (!caretaker) p.disc -= RATE.disc;
    p.energy = clamp(p.energy, 0, 100); p.hunger = clamp(p.hunger, 0, 100); p.happy = clamp(p.happy, 0, 100); p.disc = clamp(p.disc, 0, 100);
    maintain(p, caretaker, ideal);
    // starving
    if (p.hunger <= 0) {
      p.starveMin++;
      if (p.starveMin === STARVE_MISTAKE) mistake(id, p, 'hungry', budget);
      if (p.starveMin === STARVE_SICK && !p.sick) { p.sick = true; p.sickWhy = 'hungry'; p.sickMin = 0; push({ id, type: 'sick', why: 'hungry' }); }
      if (p.starveMin % 120 === 0) p.weight = Math.max(Math.round(ideal * 0.5), p.weight - 1);
    } else p.starveMin = 0;
    // sad
    if (p.happy <= 0) { p.sadMin++; if (p.sadMin === SAD_MISTAKE) mistake(id, p, 'sad', budget); } else p.sadMin = 0;
    // sleepy with the lights on
    if (save.lights && !p.nap && p.energy < SLEEPY_AT) { p.sleepyMin++; if (p.sleepyMin === SLEEPY_MISTAKE) mistake(id, p, 'lights', budget); }
    else if (!save.lights || p.energy >= SLEEPY_AT) p.sleepyMin = 0;
    // poop
    p.poopClock += asleep ? 0.5 : 1;
    if (p.poopClock >= POOP_EVERY) {
      p.poopClock = 0;
      if (caretaker) { /* Upkeep does not grant care progress. */ }
      else if (p.disc >= TOILET_DISC && !asleep) { p.toilet++; p.disc = clamp(p.disc + 1, 0, 100); push({ id, type: 'toilet' }); }
      else if (p.poop.length < POOP_MAX) {
        const a = rand01(hash(`${id}|pa|${mi}`)) * Math.PI * 2, r = 1.2 + rand01(hash(`${id}|pr|${mi}`)) * 2.6;
        p.poop.push({ t: mt, x: +(Math.cos(a) * r).toFixed(2), z: +(Math.sin(a) * r * 0.8 + 0.4).toFixed(2), n: false, k: ++p.pileN });
        push({ id, type: 'poop' });
      }
    }
    // dirt and neglect
    p.dirt += p.poop.length;
    for (const pile of p.poop) if (!pile.n && mt - pile.t >= PILE_NEGLECT * MIN) { pile.n = true; p.poopNeglect++; push({ id, type: 'neglect' }); }
    if (!p.sick && p.dirt >= DIRT_SICK) { p.sick = true; p.sickWhy = 'dirty'; p.sickMin = 0; push({ id, type: 'sick', why: 'dirty' }); }
    // sick, untreated
    if (p.sick) { p.sickMin++; if (p.sickMin === SICK_MISTAKE) mistake(id, p, 'sick', budget); }
    // tantrums
    if (p.tantrum && mt >= p.tantrum) { p.tantrum = 0; mistake(id, p, 'tantrum', budget); }
    else if (!caretaker && !p.tantrum && !asleep && !p.sick && rand01(hash(`${id}|tt|${mi}`)) < TANTRUM_CHANCE * (1 - p.disc / 100) ** 2) {
      p.tantrum = mt + TANTRUM_MIN * MIN;
      push({ id, type: 'tantrum' });
    }
  }
  p.t = from + mins * MIN;
  maintain(p, caretakerAt(t), ideal);
}

function tick(t = now()) {
  t = Math.max(t, save.caretaker?.clock || 0);
  if (save.caretaker) save.caretaker.clock = t;
  syncRoom();
  stock(save.meat, MEAT_MAX, MEAT_EVERY, t);
  stock(save.bandage, BANDAGE_MAX, BANDAGE_EVERY, t);
  if (save.daily.day !== dayKey(t)) save.daily = { day: dayKey(t), bits: 0, data: 0 };
  for (const id of save.residents) simulate(id, save.pets[id], t);
}

// ---------- rewards ----------
function reward(id, bits, data) {
  tick();
  const d = save.daily;
  const b = clamp(Math.round(bits), 0, DAILY.bits - d.bits);
  let x = clamp(Math.round(data), 0, DAILY.data - d.data);
  if (b > 0) { d.bits += b; W.add(b, 'Digi-Farm'); }
  if (x > 0) { const used = E && E.grantData ? E.grantData(id, x) : 0; d.data += used; x = used; }
  return { bits: b, data: x };
}
const rewardFor = (id, kind, k = 1) => reward(id, REWARD[kind][0] * k, REWARD[kind][1] * k);

// ---------- actions ----------
const isResident = (id) => save.residents.includes(id);
function petOf(id) { tick(); return isResident(id) ? save.pets[id] : null; }
const asleepNow = (p) => !!p.flags.asleep;
const fail = (msg, extra) => ({ ok: false, msg, ...extra });

function feed(id, food = 'meat') {
  const p = petOf(id);
  const f = FOODS[food];
  if (!p || !f) return fail('Not in the room.');
  if (asleepNow(p)) return fail(`${nameOf(id)} is asleep.`);
  if (p.tantrum && p.disc < 50) { p.refusals++; changed(id); return fail(`${nameOf(id)} refuses to eat! Scold it.`, { refused: true }); }
  if (p.hunger >= 100 && !f.sweet) { p.refusals++; changed(id); return fail(`${nameOf(id)} is full.`, { refused: true }); }
  // pay: free meat from the tray, else Bits
  let paid = 0;
  if (food === 'meat' && save.meat.n > 0) { if (save.meat.n >= MEAT_MAX) save.meat.t = now(); save.meat.n--; }
  else { paid = food === 'meat' ? MEAT_BITS : f.bits; if (!W.spend(paid)) return fail(`Needs ${paid} Bits.`); }
  const before = p.hunger;
  const over = Math.max(0, before + f.hunger - 100);
  p.hunger = clamp(before + f.hunger, 0, 100);
  p.weight = clamp(p.weight + f.weight + (over >= 20 ? 1 : 0), Math.round(idealOf(id) * 0.5), Math.round(idealOf(id) * 2.5));
  if (over >= 20 || (f.sweet && before >= 90)) p.overfeed++;
  if (f.happy) p.happy = clamp(p.happy + f.happy, 0, 100);
  if (f.energy) p.energy = clamp(p.energy + f.energy, 0, 100);
  if (f.disc) p.disc = clamp(p.disc + f.disc, 0, 100);
  p.poopClock += 15; // eating moves the next poop closer
  if (before < 70) p.goodFood += f.healthy ? 0.75 : 0.5;
  p.starveMin = 0;
  const rw = before < 80 ? rewardFor(id, 'feed') : { bits: 0, data: 0 };
  changed(id);
  return { ok: true, msg: `${nameOf(id)} ate the ${f.name}.`, paid, reward: rw, hunger: p.hunger, weight: p.weight };
}

function clean(id) {
  tick();
  let n = 0;
  for (const rid of save.residents) { const p = save.pets[rid]; n += p.poop.length; p.poop = []; p.dirt = 0; }
  if (!n) return fail('Nothing to clean.');
  const rw = id && isResident(id) ? rewardFor(id, 'clean') : { bits: 0, data: 0 };
  changed(id);
  return { ok: true, msg: `Cleaned ${n} pile${n === 1 ? '' : 's'}.`, piles: n, reward: rw };
}
// one pile, clicked in the room
function cleanPile(id, k) {
  const p = petOf(id);
  if (!p) return fail('');
  const i = p.poop.findIndex((x) => x.k === k);
  if (i < 0) return fail('');
  p.poop.splice(i, 1);
  if (!p.poop.length) p.dirt = 0;
  const rw = rewardFor(id, 'clean', 0.4);
  changed(id);
  return { ok: true, msg: 'Cleaned.', reward: rw };
}

function cure(id, kind = 'bandage') {
  const p = petOf(id);
  if (!p) return fail('Not in the room.');
  if (!p.sick) return fail(`${nameOf(id)} is not sick.`);
  if (kind === 'bandage') { if (save.bandage.n <= 0) return fail('No bandages left. They refill over time.'); if (save.bandage.n >= BANDAGE_MAX) save.bandage.t = now(); save.bandage.n--; }
  else { if (!W.spend(MEDICINE_BITS)) return fail(`Medicine needs ${MEDICINE_BITS} Bits.`); p.happy = clamp(p.happy - 5, 0, 100); }
  p.sick = false; p.sickWhy = ''; p.sickMin = 0;
  if (p.dirt >= DIRT_SICK) p.dirt = 0;
  p.starveMin = 0;
  const rw = rewardFor(id, 'cure');
  changed(id);
  return { ok: true, msg: `${nameOf(id)} feels better.`, reward: rw };
}

function setLights(on) {
  tick();
  save.lights = on == null ? !save.lights : !!on;
  for (const id of save.residents) { const p = save.pets[id]; if (save.lights && p.energy >= 100) p.nap = false; p.flags.asleep = (!save.lights && p.energy < 100) || p.nap; }
  changed(null);
  return { ok: true, lights: save.lights, msg: save.lights ? 'Lights on.' : 'Lights off. Good night!' };
}

function praise(id) {
  const p = petOf(id);
  if (!p) return fail('Not in the room.');
  if (p.tantrum) {
    p.tantrum = 0; p.disc = clamp(p.disc - 8, 0, 100); p.happy = clamp(p.happy + 5, 0, 100);
    changed(id);
    return { ok: true, msg: `${nameOf(id)} got away with it. Discipline went down.`, spoiled: true };
  }
  const t = now();
  const fresh = t - p.lastPraise >= PRAISE_COOLDOWN;
  p.lastPraise = t;
  p.happy = clamp(p.happy + (fresh ? 10 : 2), 0, 100);
  if (fresh) { p.goodBond += 0.34; p.disc = clamp(p.disc + 1, 0, 100); }
  const rw = fresh ? rewardFor(id, 'praise') : { bits: 0, data: 0 };
  changed(id);
  return { ok: true, msg: `${nameOf(id)} is proud.`, reward: rw };
}

function scold(id) {
  const p = petOf(id);
  if (!p) return fail('Not in the room.');
  if (p.tantrum) {
    p.tantrum = 0; p.disc = clamp(p.disc + 15, 0, 100); p.happy = clamp(p.happy - 3, 0, 100);
    const rw = rewardFor(id, 'scold');
    changed(id);
    return { ok: true, msg: `${nameOf(id)} calmed down. Discipline went up.`, reward: rw };
  }
  p.happy = clamp(p.happy - (p.sick ? 15 : 12), 0, 100);
  p.disc = clamp(p.disc + 3, 0, 100);
  changed(id);
  return { ok: true, msg: `${nameOf(id)} did nothing wrong and is upset.`, unfair: true };
}

function play(id, otherId) {
  const p = petOf(id);
  if (!p) return fail('Not in the room.');
  if (asleepNow(p)) return fail(`${nameOf(id)} is asleep.`);
  if (p.sick) return fail(`${nameOf(id)} is sick. Cure it first.`);
  if (p.energy < 10) return fail(`${nameOf(id)} is too tired.`);
  const o = otherId && otherId !== id ? petOf(otherId) : null;
  const was = p.happy;
  for (const q of [p, o]) {
    if (!q || asleepNow(q) || q.sick) continue;
    q.happy = clamp(q.happy + 15, 0, 100); q.energy = clamp(q.energy - 8, 0, 100); q.hunger = clamp(q.hunger - 5, 0, 100); q.goodBond += 0.34;
  }
  const rw = was < 95 ? rewardFor(id, 'play') : { bits: 0, data: 0 };
  changed(id);
  return { ok: true, msg: o ? `${nameOf(id)} and ${nameOf(otherId)} played together.` : `${nameOf(id)} played with the ball.`, reward: rw, with: o ? otherId : null };
}

function petIt(id) {
  const p = petOf(id);
  if (!p) return fail('');
  const t = now();
  if (t - p.lastPet < PET_COOLDOWN) return { ok: true, msg: '', small: true };
  p.lastPet = t;
  p.happy = clamp(p.happy + 3, 0, 100);
  p.goodBond += 0.05;
  changed(id);
  return { ok: true, msg: '' };
}

function canTrain(id) {
  const p = petOf(id);
  if (!p) return 'Not in the room.';
  if (asleepNow(p)) return `${nameOf(id)} is asleep.`;
  if (p.sick) return `${nameOf(id)} is sick. Cure it first.`;
  if (p.energy < 15) return `${nameOf(id)} is too tired. Turn the lights off for a nap.`;
  if (p.hunger < 10) return `${nameOf(id)} is too hungry to train.`;
  if (p.tantrum && p.disc < 40) return `${nameOf(id)} won't listen. Scold it first.`;
  return null;
}

function train(id, game, score) {
  const why = canTrain(id);
  const g = TRAIN[game];
  if (why) return fail(why);
  if (!g) return fail('Unknown training.');
  const p = save.pets[id];
  score = clamp(Number(score) || 0, 0, 1);
  const gain = 2 + Math.round(8 * score);
  const gains = { [g.stat]: gain };
  if (g.stat !== 'sta') gains.sta = 1 + (score >= 0.6 ? 1 : 0);
  for (const [k, v] of Object.entries(gains)) p[k] = clamp((p[k] || 0) + v, 0, TRAIN_CAP);
  p.energy = clamp(p.energy - 15, 0, 100);
  p.hunger = clamp(p.hunger - 8, 0, 100);
  p.weight = Math.max(Math.round(idealOf(id) * 0.5), p.weight - (score >= 0.7 ? 2 : 1));
  p.trained++;
  if (score >= 0.8) p.happy = clamp(p.happy + 5, 0, 100);
  const rw = reward(id, 3 + Math.round(3 * score), 6 + Math.round(8 * score));
  changed(id);
  return { ok: true, msg: `${g.name} +${gain}`, gains, reward: rw, score };
}

// ---------- room management ----------
function admit(id) {
  if (!save.pets[id]) save.pets[id] = newPet(id, now()); else save.pets[id].t = now();
  maintain(save.pets[id], caretakerOf(), idealOf(id));
}
function moveIn(id) {
  tick();
  if (!C.owns('legend', id) || isResident(id)) return fail('');
  if (save.residents.length >= save.cap) return fail('The room is full. Send someone to the Digi-Bank first.');
  save.residents.push(id);
  admit(id); // banked time is paused
  changed(id);
  return { ok: true, msg: `${nameOf(id)} moved in.` };
}
function moveOut(id) {
  tick();
  if (!isResident(id)) return fail('');
  if (save.residents.length <= 1) return fail('Keep at least one legend in the room.');
  save.residents = save.residents.filter((x) => x !== id);
  changed(id);
  return { ok: true, msg: `${nameOf(id)} went to the Digi-Bank.` };
}
function swap(inId, outId) {
  tick();
  if (!isResident(outId) || isResident(inId) || !C.owns('legend', inId)) return fail('');
  save.residents[save.residents.indexOf(outId)] = inId;
  admit(inId);
  changed(inId);
  return { ok: true, msg: `${nameOf(inId)} moved in. ${nameOf(outId)} went to the Digi-Bank.` };
}
const expandCost = () => (save.cap >= CAP_MAX ? 0 : EXPAND_COST[(save.cap - CAP_START) / CAP_STEP] || EXPAND_COST[EXPAND_COST.length - 1]);
function buyExpansion() {
  tick();
  if (save.cap >= CAP_MAX) return fail('The room is as big as it gets.');
  const cost = expandCost();
  if (!W.spend(cost)) return fail(`Needs ${cost} Bits.`);
  save.cap += CAP_STEP;
  // bank legends move in to fill the new space
  for (const id of owned()) if (save.residents.length < save.cap && !isResident(id)) { save.residents.push(id); admit(id); }
  changed(null);
  return { ok: true, msg: `The room now holds ${save.cap}.` };
}
const themeOf = (id) => THEMES.find((x) => x.id === id) || THEMES[0];
const ownsTheme = (id) => { const th = THEMES.find((x) => x.id === id); return !!th && (th.free || save.themes.includes(id) || (th.board && C.owns('board', th.board))); };
function buyTheme(id) {
  const th = THEMES.find((x) => x.id === id);
  if (!th) return fail('');
  if (ownsTheme(id)) return setTheme(id);
  if (!W.spend(th.price)) return fail(`Needs ${th.price} Bits.`);
  save.themes.push(id);
  return setTheme(id);
}
function setTheme(id) {
  if (!ownsTheme(id)) return fail('');
  save.theme = id;
  changed(null);
  return { ok: true, msg: `${themeOf(id).name}.` };
}

// ---------- legendevo care ----------
function evoCare(id) {
  const p = save.pets[id];
  if (!p) return null;
  const ideal = idealOf(id);
  const fat = p.weight >= ideal * FAT;
  const total = (p.str || 0) + (p.spd || 0) + (p.wis || 0) + (p.sta || 0);
  const out = {
    bond: Math.min(8, Math.floor(p.goodBond)),
    battle: Math.min(8, Math.floor(total / 25)),
    food: Math.min(8, Math.floor(p.goodFood)),
    wild: Math.min(30, p.mistakes * 3 + p.poopNeglect * 2 + (fat ? 6 : 0) + (p.sick ? 3 : 0) + (p.overfeed >= 5 ? 2 : 0)),
  };
  const parts = [];
  for (const k of ['bond', 'battle', 'food']) if (out[k]) parts.push(`${E.CARE[k].name} +${out[k]}`);
  if (out.wild) parts.push(`Wild +${out.wild}`);
  out.note = parts.length ? `Digi-Farm: ${parts.join(', ')}` : '';
  out.why = { mistakes: p.mistakes, poopNeglect: p.poopNeglect, fat, sick: p.sick, overfeed: p.overfeed, training: total, goodBond: p.goodBond, goodFood: p.goodFood };
  return out;
}
if (E && E.addCareSource) E.addCareSource((id) => { const x = evoCare(id); return x && (x.bond || x.battle || x.food || x.wild) ? x : null; });

// ---------- views ----------
function moodOf(p, id) {
  if (asleepNow(p)) return { icon: '💤', text: 'Sleeping' };
  if (p.sick) return { icon: '🤒', text: 'Sick' };
  if (p.tantrum) return { icon: '💢', text: 'Throwing a tantrum' };
  if (p.hunger < 20) return { icon: '🍖', text: 'Hungry' };
  if (save.lights && p.energy < SLEEPY_AT) return { icon: '🥱', text: 'Sleepy' };
  if (p.poop.length >= 2) return { icon: '💩', text: 'Wants a clean room' };
  if (p.happy < 25) return { icon: '💧', text: 'Sad' };
  if (p.weight >= idealOf(id) * FAT) return { icon: '🐷', text: 'Chubby' };
  if (p.happy >= 80) return { icon: '♪', text: 'Happy' };
  return { icon: '', text: 'Content' };
}

function card(id) {
  const p = petOf(id);
  if (!p) return null;
  const t = now();
  const ideal = idealOf(id);
  const i = E.info(id);
  return {
    id, name: nameOf(id), base: (C.get('legend', id) || {}).name || id, form: i.form, formInfo: i.formInfo, key: i.key, level: i.level,
    ageDays: Math.floor((t - p.born) / (24 * 60 * MIN)), hunger: p.hunger, energy: p.energy, happy: p.happy, disc: p.disc,
    weight: p.weight, ideal, fat: p.weight >= ideal * FAT, thin: p.weight <= ideal * THIN, poop: p.poop.length, sick: p.sick, sickWhy: p.sickWhy,
    tantrum: !!p.tantrum, asleep: asleepNow(p), mistakes: p.mistakes, poopNeglect: p.poopNeglect, overfeed: p.overfeed,
    str: p.str, spd: p.spd, wis: p.wis, sta: p.sta, trainCap: TRAIN_CAP, mood: moodOf(p, id), next: i.next, canDigivolve: i.canDigivolve,
    odds: i.odds, evo: evoCare(id), canTrain: canTrain(id),
  };
}

function state() {
  tick();
  return {
    cap: save.cap, capMax: CAP_MAX, expandCost: expandCost(), residents: save.residents.slice(), bank: owned().filter((id) => !isResident(id)),
    theme: save.theme, lights: save.lights, meat: { ...save.meat, max: MEAT_MAX }, bandage: { ...save.bandage, max: BANDAGE_MAX },
    daily: { ...save.daily, capBits: DAILY.bits, capData: DAILY.data }, now: now(),
    caretaker: caretakerOf()?.id || null, careUntil: save.caretaker?.until || 0, caretakers: CARETAKERS.map((plan) => ({ ...plan, dailyPrice: plan.price, ...caretakerQuote(plan.id) })),
    themes: THEMES.map((th) => ({ ...th, owned: ownsTheme(th.id) })),
  };
}

const Farm = {
  FOODS, TRAIN, THEMES, DAILY, RATE, CAP_START, CAP_MAX, EXPAND_COST, CATCHUP_MIN, OFFLINE_MISTAKES, POOP_EVERY, POOP_MAX, TOILET_DISC,
  DIRT_SICK, PILE_NEGLECT, IDEAL_WEIGHT, FAT, MEAT_MAX, MEAT_EVERY, MEAT_BITS, BANDAGE_MAX, BANDAGE_EVERY, MEDICINE_BITS, TANTRUM_MIN, REWARD,
  KEY, now, tick, state, card, evoCare, themeOf, ownsTheme, nameOf, CARETAKERS, hireCaretaker, caretakerQuote,
  pet(id) { const p = petOf(id); return p ? JSON.parse(JSON.stringify(p)) : null; },
  piles() { tick(); return save.residents.flatMap((id) => save.pets[id].poop.map((x) => ({ id, ...x }))); },
  setPilePos(id, k, x, z) { const p = save.pets[id]; const pile = p && p.poop.find((q) => q.k === k); if (pile) { pile.x = +x.toFixed(2); pile.z = +z.toFixed(2); persist(); } },
  feed, clean, cleanPile, cure, setLights, praise, scold, play, petIt, canTrain, train,
  moveIn, moveOut, swap, buyExpansion, buyTheme, setTheme,
  drainEvents() { const e = events; events = []; return e; },
  onChange(fn) { listeners.add(fn); return () => listeners.delete(fn); },
  _advance(ms) { offset += ms; tick(); changed(null); return now(); },
  _offset: () => offset,
  _save: () => save,
  _reset() { save = fresh(); offset = 0; events = []; persist(); changed(null); },
};
tick();
persist();
setInterval(() => { tick(); persist(); }, 30000); // keeps a long open session in step; the UI also ticks while open
C.onChange((k) => { if (k === 'legend') { const before = save.residents.join(); syncRoom(); if (save.residents.join() !== before) changed(null); } });

DT.Farm = Farm;
