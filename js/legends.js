// Little Legends: the tactician models. Registers the 'legend' cosmetics, loads their GLBs from models/legends/
// (tools/convert: convert-all.cjs --legends, then optimize.mjs --legends) and animates them.
// Client-only: nothing here touches game state.
//
// DT.Legends.play(playerId, role)    role: 'dance' | 'happy' | 'sad' | 'taunt' | 'eat' | 'wave' | 'hit'; false if that
//                                    player's legend is not on screen
// DT.Legends.modelFor(legendId)      Promise of a template { root, clips, footprint, item, clone() } or null
// DT.Legends.createView(legendId, opts)  a LegendView for any scene: add view.group, call view.update(dt) each frame,
//                                    view.play(role), view.dispose(). opts: { height, yaw, ring: false, color }
// DT.Legends.anchor(playerId)        screen point above that player's legend on the board, or null
// DT.Legends.thumb(legendId)         Promise of a picture URL; DT.Legends.html(legendId, cls) is markup that fills in
// Every legendId above can also be 'id@form' (js/legendevo.js): the legend in a digivolved form, drawn with that
// form's battle model. DT.Social.lookOf returns such keys for legends that have digivolved.
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import * as SkeletonUtils from 'three/addons/utils/SkeletonUtils.js';
import { MeshoptDecoder } from 'three/addons/libs/meshopt_decoder.module.js';
import { prepareTemplate, makePlaceholder, loadTemplate, disposeSkeletons } from './render3d.js';
import { CompanionLocomotion } from './companion-locomotion.js';

const DT = window.DT;

// Clip names written by optimize.mjs --legends: idle, run, win, happy (only some), eat, hit, sad, getup, taunt,
// attack, guard. A role maps to a clip name, a list of names played in order, or a list of dance steps.
// A dance step: { clip: name or [fallback names] or null, loops, speed, len (seconds, for clip: null), fx }.
// fx is a procedural move layered on top: spin, twirl, hop, sway, sway2, bounce, wave, droop, wiggle, bob, shake.
const DANCES = [
  [{ clip: 'run', loops: 2, fx: 'sway' }, { clip: 'run', loops: 2, fx: 'sway2' }, { clip: 'win', fx: 'spin' }, { clip: ['happy', 'taunt'], fx: 'hop' }],
  [{ clip: 'taunt', fx: 'hop' }, { clip: 'run', loops: 4, speed: 1.2, fx: 'twirl' }, { clip: 'win', fx: 'bounce' }],
  [{ clip: 'guard', speed: 0.8, fx: 'bounce' }, { clip: 'run', loops: 2, fx: 'hop' }, { clip: 'attack', fx: 'spin' }, { clip: 'win', fx: 'sway' }],
];
const LINKS_CLIPS = { idle: 'idle', run: 'run', happy: ['happy'], sad: ['sad', 'getup'], taunt: 'taunt', eat: 'eat', hit: 'hit' };
const PAGE = 'https://models.spriters-resource.com/mobile/digimonlinks/asset/';
const PRICE = { common: 150, uncommon: 250, rare: 400, epic: 900, legendary: 1800 };
// Only common and uncommon legends are sold directly; rare and up hatch from Digi-Eggs only.
const EGG_ONLY = { rare: true, epic: true, legendary: true };
// [id, name, stage, rarity, Models Resource asset, dance]; Fresh forms are the rarer ones
const LIST = [
  ['tokomon', 'Tokomon', 'In-Training', 'free', 304087, 0], ['tsunomon', 'Tsunomon', 'In-Training', 'free', 304172, 1],
  ['motimon', 'Motimon', 'In-Training', 'free', 304112, 2], ['bukamon', 'Bukamon', 'In-Training', 'common', 304198, 1],
  ['tanemon', 'Tanemon', 'In-Training', 'common', 304194, 0], ['yokomon', 'Yokomon', 'In-Training', 'common', 304196, 2],
  ['pagumon', 'Pagumon', 'In-Training', 'uncommon', 304207, 1], ['nyaromon', 'Nyaromon', 'In-Training', 'uncommon', 304206, 0],
  ['tsumemon', 'Tsumemon', 'In-Training', 'uncommon', 304210, 2], ['wanyamon', 'Wanyamon', 'In-Training', 'rare', 304089, 0],
  ['poyomon', 'Poyomon', 'Fresh', 'rare', 304084, 0], ['botamon', 'Botamon', 'Fresh', 'epic', 304081, 1],
  ['punimon', 'Punimon', 'Fresh', 'epic', 304167, 2], ['pabumon', 'Pabumon', 'Fresh', 'epic', 302508, 1],
  ['kuramon', 'Kuramon', 'Fresh', 'legendary', 304205, 2],
];
// October 2026 set (tools/convert/new-chains.json "legends"): [id, name, stage, rarity, source page, game, dance, clip overrides]
const RA = 'https://models.spriters-resource.com/mobile/digimonrearise/asset/';
const REARISE = 'Digimon ReArise';
// a dance with a stand-in for a clip the rip lacks
const swapDance = (d, from, to) => DANCES[d].map((s) => (s.clip === from || (Array.isArray(s.clip) && s.clip.includes(from)) ? { ...s, clip: to } : s));
const LIST2 = [
  ['koromon', 'Koromon', 'In-Training', 'common', 'https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324325/', 'Digimon Story Cyber Sleuth', 0],
  ['gummymon', 'Gummymon', 'In-Training', 'common', `${RA}327404/`, REARISE, 1],
  ['kokomon', 'Kokomon', 'In-Training', 'common', `${RA}327405/`, REARISE, 2],
  // static Digimon Masters rip: no clips, so every move is procedural (idle bob, hop walk, emote fx)
  ['upamon', 'Upamon', 'In-Training', 'common', 'https://models.spriters-resource.com/pc_computer/digimonmasters/asset/287804/', 'Digimon Masters', 0],
  ['demiveemon', 'DemiVeemon', 'In-Training', 'uncommon', `${RA}327401/`, REARISE, 0],
  ['gigimon', 'Gigimon', 'In-Training', 'uncommon', `${RA}327403/`, REARISE, 1],
  // no win clip: happy stands in
  ['dorimon', 'Dorimon', 'In-Training', 'uncommon', `${RA}327402/`, REARISE, 2, { dance: swapDance(2, 'win', ['happy', 'taunt']), happy: ['happy'] }],
  ['pichimon', 'Pichimon', 'Fresh', 'uncommon', `${RA}327396/`, REARISE, 0],
  ['yuramon', 'Yuramon', 'Fresh', 'uncommon', `${RA}327399/`, REARISE, 1],
  ['conomon', 'Conomon', 'In-Training', 'rare', `${RA}327393/`, REARISE, 2],
  ['pusurimon', 'Pusurimon', 'In-Training', 'rare', `${RA}327406/`, REARISE, 0],
  // no taunt clip: attack stands in
  ['yukimibotamon', 'YukimiBotamon', 'Fresh', 'rare', `${RA}327398/`, REARISE, 1, { taunt: 'attack', dance: swapDance(1, 'taunt', ['attack', 'happy']) }],
  ['zerimon', 'Zerimon', 'Fresh', 'rare', `${RA}327400/`, REARISE, 2],
  ['chibomon', 'Chibomon', 'Fresh', 'epic', `${RA}327392/`, REARISE, 0],
  ['jyarimon', 'Jyarimon', 'Fresh', 'epic', `${RA}327395/`, REARISE, 1],
  ['dodomon', 'Dodomon', 'Fresh', 'epic', `${RA}327394/`, REARISE, 2],
  ['pusumon', 'Pusumon', 'Fresh', 'legendary', `${RA}327397/`, REARISE, 0],
];
const entry = (id, name, stage, rarity, game, url, clips) => ({
  id, name, rarity: rarity === 'free' ? 'common' : rarity, price: rarity === 'free' ? 0 : PRICE[rarity], free: rarity === 'free' || undefined,
  eggOnly: EGG_ONLY[rarity] || undefined,
  desc: `${stage} Digimon.`,
  data: { file: `models/legends/${id}.glb`, clips, scale: 1, rotY: 0,
    credit: { name: `${name} (${game})`, url, author: 'The Models Resource', license: 'Game rip' } },
  preview: (el) => { el.innerHTML = html(DT.LegendEvo ? DT.LegendEvo.key(id) : id, 'lgprev', 'width:100%;height:100%;min-height:96px;background:transparent;border:0;box-shadow:none'); },
});
// Retro (Digimon World) set: static DW1 rips (models/index.json 'extra:DW1_*'), egg-only chase items with a 'Retro'
// tag. No clips, so they move like the digivolved forms: procedural idle bob, hop walk, slump, spin dances.
const RETRO = [
  ['retro_tokomon', 'Tokomon', 'In-Training', 'rare', 'extra:DW1_Tokomon'],
  ['retro_tsunomon', 'Tsunomon', 'In-Training', 'rare', 'extra:DW1_Tsunomon'],
  ['retro_poyomon', 'Poyomon', 'Fresh', 'rare', 'extra:DW1_Poyomon'],
  ['retro_punimon', 'Punimon', 'Fresh', 'epic', 'extra:DW1_Punimon'],
  ['retro_botamon', 'Botamon', 'Fresh', 'epic', 'extra:DW1_Botamon'],
];
const retroEntry = ([id, name, stage, rarity, model]) => {
  const src = (DT.MODEL_SOURCES && DT.MODEL_SOURCES[model]) || {};
  const e = entry(id, `Retro ${name}`, stage, rarity, 'Digimon World', src.url || 'https://models.spriters-resource.com/playstation/digimonworld/', {});
  e.desc = `${stage} Digimon. The original Digimon World model, with its own Digimon World digivolution tree.`;
  e.tag = 'Retro';
  e.data.model = model;
  e.data.static = true;
  delete e.data.file;
  return e;
};
const CATALOG = [
  ...LIST.map(([id, name, stage, rarity, asset, dance]) => entry(id, name, stage, rarity, 'Digimon Links', `${PAGE}${asset}/`, { ...LINKS_CLIPS, dance: DANCES[dance] })),
  ...LIST2.map(([id, name, stage, rarity, url, game, dance, over]) => entry(id, name, stage, rarity, game, url, { ...LINKS_CLIPS, dance: DANCES[dance], ...(over || {}) })),
  ...RETRO.map(retroEntry),
];
CATALOG.find((x) => x.id === 'upamon').data.static = true;
DT.Cosmetics.register('legend', CATALOG);

// Fallbacks when the item's own clip map has nothing for a role, then a procedural move.
const ROLE_FALLBACK = {
  idle: ['idle'], run: ['run', 'walk', 'move'],
  dance: ['dance', 'win', 'happy'], happy: ['happy', 'win', 'dance'], sad: [['sad', 'getup'], 'sad', 'hit'],
  taunt: ['taunt', 'attack'], eat: ['eat'], wave: ['wave'], hit: ['hit'],
};
const PROC = { dance: 'twirl', happy: 'hop', sad: 'droop', taunt: 'wiggle', eat: 'bob', wave: 'wave', hit: 'shake' };
// Digivolved forms use the Rookie-and-up battle models, which only have idle / attack / special / win clips:
// happy plays win, taunt plays attack, sad is a procedural slump, and walking has no run clip, so the renderer hops.
const UNIT_CLIPS = { idle: 'idle', happy: 'win', taunt: 'attack', run: null, sad: null, eat: null, hit: null, wave: null,
  dance: [{ clip: 'idle', fx: 'sway' }, { clip: 'win', fx: 'spin' }, { clip: 'special', fx: 'hop' }, { clip: 'win', fx: 'bounce' }] };
const UNIT_PROC = { ...PROC, sad: 'slump' };
// height cap per form level, in board units (a hex tile is wider than 1.2, so big forms never cover the board)
const FORM_HEIGHT = { Fresh: 1, 'In-Training': 1, Rookie: 1.05, Champion: 1.15, Ultimate: 1.2 };

// ---------- models ----------
const loader = new GLTFLoader();
loader.setMeshoptDecoder(MeshoptDecoder);
const templates = new Map();
const baseId = (id) => String(id == null ? '' : id).split('@')[0];
const itemOf = (id) => DT.Cosmetics.get('legend', baseId(id)) || DT.Cosmetics.list('legend').find((x) => x.free) || null;
// the digivolved form in an 'id@form' key, or null
function formOf(key) {
  const E = DT.LegendEvo;
  if (!E || !String(key || '').includes('@')) return null;
  const p = E.parse(key);
  return p.form && E.reach(p.legend).includes(p.form) ? E.formInfo(p.form) : null;
}

// Template for a digivolved form: its battle model, with clips named the way stepsFor expects.
// Template from a battle model key in models/index.json ('line:stage' or 'extra:<Rip>'), with clips named the way
// stepsFor expects. Used for digivolved forms and for the retro legends, whose base model is a static DW1 rip.
function unitTemplate(item, model, form) {
  return loadTemplate(model).then((base) => {
    if (!base) return null;
    const clips = {};
    if (base.clip) clips.idle = base.clip;
    for (const [role, c] of Object.entries(base.actions || {})) clips[role] = c;
    const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[model];
    return {
      root: base.root, clip: base.clip, footprint: base.footprint, clips, form, proc: UNIT_PROC, source: model,
      item: { ...item, data: { ...item.data, clips: UNIT_CLIPS } },
      clone: () => { const o = SkeletonUtils.clone(base.root); if (src && src.rotY) o.rotation.y = src.rotY; return o; },
    };
  });
}

function formModel(item, f) {
  const k = `${item.id}@${f.id}`;
  if (templates.has(k)) return templates.get(k);
  const p = f.legend ? modelFor(f.legend) : unitTemplate(item, f.model, f);
  templates.set(k, p);
  return p;
}

function modelFor(legendId) {
  const item = itemOf(legendId);
  if (!item) return Promise.resolve(null);
  const f = formOf(legendId);
  if (f) return formModel(item, f);
  if (templates.has(item.id)) return templates.get(item.id);
  if (item.data && item.data.model) { const t = unitTemplate(item, item.data.model, null); templates.set(item.id, t); return t; }
  const p = new Promise((res) => {
    if (location.protocol === 'file:' || !item.data || !item.data.file) return res(null);
    loader.load(item.data.file, (g) => {
      const t = prepareTemplate(g, null);
      t.clips = {};
      for (const c of g.animations || []) t.clips[c.name] = c;
      if (!(g.animations || []).length) t.proc = UNIT_PROC; // static rip (Upamon): move like the digivolved forms
      t.item = item;
      t.clone = () => {
        const o = SkeletonUtils.clone(t.root);
        if (item.data.rotY) o.rotation.y = item.data.rotY;
        return o;
      };
      res(t);
    }, undefined, (e) => { console.warn('Legend model failed to load', item.data.file, e); res(null); });
  });
  templates.set(item.id, p);
  return p;
}

const clipOf = (t, names) => { for (const n of [].concat(names)) if (t.clips[n]) return t.clips[n]; return null; };

// role -> dance steps, or null when the model has no clip for it
function stepsFor(t, role) {
  if (!t || !t.clips) return null;
  const own = t.item.data && t.item.data.clips && t.item.data.clips[role];
  for (const c of [own, ...(ROLE_FALLBACK[role] || [role])]) {
    if (!c) continue;
    const steps = [].concat(c).map((s) => (typeof s === 'string' ? { clip: s } : s))
      .filter((s) => s.clip == null ? s.len : clipOf(t, s.clip));
    if (steps.some((s) => s.clip != null)) return steps;
  }
  return null;
}

// procedural moves: k runs 0..1 over the step, n is the step's loop count
const ease = (k) => k * k * (3 - 2 * k);
const FX = {
  spin(v, k) { v.rotation.y += ease(k) * Math.PI * 2; v.position.y = Math.sin(Math.PI * k) * 0.12; },
  twirl(v, k, n) { v.rotation.y += ease(k) * Math.PI * 2 * Math.max(1, Math.round(n / 2)); v.position.y = Math.abs(Math.sin(k * Math.PI * n)) * 0.18; },
  hop(v, k, n, d) { v.position.y = Math.abs(Math.sin(k * Math.PI * Math.max(1, Math.round(d / 0.45)))) * 0.3; },
  sway(v, k, n) { const s = Math.sin(k * Math.PI * 2 * n); v.position.x = s * 0.2; v.rotation.z = -s * 0.2; },
  sway2(v, k, n) { const s = Math.sin(k * Math.PI * 2 * n); v.position.x = -s * 0.2; v.rotation.z = s * 0.2; v.rotation.y += s * 0.5; },
  bounce(v, k, n, d) {
    const b = Math.sin(k * Math.PI * Math.max(2, Math.round(d / 0.35)));
    v.position.y = Math.max(0, b) * 0.16;
    const sq = b < 0 ? 1 + 0.12 * b : 1 + 0.06 * b; // squash on landing, stretch in the air
    v.scale.set(v.userData.s / Math.sqrt(sq), v.userData.s * sq, v.userData.s / Math.sqrt(sq));
  },
  wave(v, k) { const s = Math.sin(k * Math.PI * 6) * Math.sin(Math.PI * k); v.rotation.z = s * 0.3; v.position.y = Math.abs(s) * 0.08; },
  droop(v, k) { const s = Math.sin(Math.PI * k); v.rotation.x = 0.35 * s; v.position.y = -0.05 * s; },
  // heavy slump forward with a sag, held, then back up (digivolved forms have no sad clip)
  slump(v, k) {
    const s = Math.min(1, k * 4, (1 - k) * 3);
    v.rotation.x = 0.5 * s;
    v.rotation.z = Math.sin(k * Math.PI * 3) * 0.05 * s;
    const w = v.userData.s * (1 + 0.06 * s);
    v.scale.set(w, v.userData.s * (1 - 0.12 * s), w);
  },
  wiggle(v, k) { v.rotation.z = Math.sin(k * Math.PI * 8) * 0.3 * Math.sin(Math.PI * k); },
  bob(v, k) { v.rotation.x = Math.max(0, Math.sin(k * Math.PI * 6)) * 0.35; },
  // static models (no clips, e.g. Upamon): a slow bounce and sway so they never stand frozen
  idle(v, t) {
    const b = Math.sin(t * 3.2);
    v.position.y = Math.max(0, b) * 0.035;
    v.rotation.z = Math.sin(t * 1.6) * 0.06;
    const sq = 1 + 0.035 * b;
    v.scale.set(v.userData.s / Math.sqrt(sq), v.userData.s * sq, v.userData.s / Math.sqrt(sq));
  },
  shake(v, k) { v.position.z = -0.15 * Math.sin(Math.PI * k); v.rotation.z = Math.sin(k * Math.PI * 10) * 0.15 * (1 - k); },
};

// ---------- view ----------
const RING = new THREE.RingGeometry(0.58, 0.7, 40);
const DISC = new THREE.CircleGeometry(0.58, 40);

export class LegendView {
  constructor(legendId, opts = {}) {
    const item = itemOf(legendId);
    this.id = item ? item.id : legendId;
    this.form = formOf(legendId);
    this.key = this.form ? `${this.id}@${this.form.id}` : this.id;
    this.height = (opts.height || 1.0) * (this.form ? FORM_HEIGHT[this.form.level] || 1.2 : (item && item.data && item.data.scale) || 1);
    this.group = new THREE.Group();
    this.body = new THREE.Group();
    this.group.add(this.body);
    this.mats = [];
    if (opts.ring !== false) {
      const ringMat = new THREE.MeshBasicMaterial({ color: opts.color || '#ffcf4a', transparent: true, opacity: 0.9, side: THREE.DoubleSide });
      const discMat = new THREE.MeshBasicMaterial({ color: '#000000', transparent: true, opacity: 0.25 });
      const ring = new THREE.Mesh(RING, ringMat);
      const disc = new THREE.Mesh(DISC, discMat);
      ring.rotation.x = disc.rotation.x = -Math.PI / 2;
      ring.position.y = 0.16;
      disc.position.y = 0.155;
      ring.scale.setScalar(0.8);
      disc.scale.setScalar(0.8);
      this.group.add(disc, ring);
      this.mats.push(ringMat, discMat);
    }
    this.yaw = opts.yaw != null ? opts.yaw : Math.PI;
    this.body.rotation.y = this.yaw;
    this.time = 0;
    this.moving = false;
    this.hasRun = false;
    this.mixer = null;
    this.seq = null;
    this.ready = false;
    this.setVisual(new THREE.Group(), 1); // empty until the model arrives
    modelFor(this.key).then((t) => {
      if (this.disposed) return;
      if (t) this.attach(t);
      else { this.placeholder = true; this.setVisual(makePlaceholder(DT.LINE[this.form ? this.form.line : this.id] || DT.LINE.patamon), this.height); }
      this.ready = true;
    });
  }

  setVisual(obj, scale) {
    if (this.visual) {
      this.body.remove(this.visual);
      if (this.placeholder) this.visual.traverse((o) => { if (o.isMesh) o.material.dispose(); });
    }
    this.visual = obj;
    obj.userData.rotY = obj.rotation.y;
    obj.scale.multiplyScalar(scale);
    obj.userData.s = obj.scale.x;
    this.body.add(obj);
  }

  attach(t) {
    this.setVisual(t.clone(), Math.min(this.height, 1.5 / Math.max(t.footprint, 0.01)));
    this.t = t;
    this.locomotion = new CompanionLocomotion(this.visual, t.source || ('legend:' + t.item.id), !!clipOf(t, ROLE_FALLBACK.run));
    this.hasRun = !this.locomotion.usesTravelHop;
    if (!t.clip) return;
    this.mixer = new THREE.AnimationMixer(this.visual);
    this.idle = this.mixer.clipAction(clipOf(t, ['idle']) || t.clip);
    this.idle.play();
    const run = clipOf(t, ROLE_FALLBACK.run);
    this.run = run ? this.mixer.clipAction(run) : null;
    this.hasRun = !this.locomotion.usesTravelHop;
    this.cur = this.idle; // the action holding the pose
    if (this.moving) { this.moving = false; this.setMoving(true); }
  }

  base() { return this.moving && this.run ? this.run : this.idle; }

  fadeTo(a, t = 0.12) {
    if (this.cur === a) return;
    if (this.cur) this.cur.fadeOut(t);
    a.reset().setEffectiveWeight(1).fadeIn(t).play();
    this.cur = a;
  }

  setMoving(on) {
    on = !!on;
    if (on === this.moving) return;
    this.moving = on;
    if (on && this.seq && this.seq.clips) this.seq = null;
    if (this.mixer && !(this.seq && this.seq.clips)) this.fadeTo(this.base());
  }

  // emote / reaction; returns true when it started
  play(role) {
    let steps = this.moving || !this.mixer ? null : stepsFor(this.t, role);
    const proc = (this.t && this.t.proc) || PROC;
    if (!steps) steps = [{ clip: null, len: role === 'dance' ? 2.6 : role === 'wave' ? 1.4 : proc[role] === 'slump' ? 1.6 : 0.9, fx: proc[role] || 'hop' }];
    this.seq = { role, steps, i: -1, clips: steps.some((s) => s.clip != null) };
    this.nextStep();
    return true;
  }

  nextStep() {
    const s = this.seq;
    s.i++;
    if (s.i >= s.steps.length) { this.seq = null; if (this.mixer) this.fadeTo(this.base(), 0.2); return; }
    const st = s.steps[s.i];
    const clip = st.clip != null && this.mixer ? clipOf(this.t, st.clip) : null;
    const loops = st.loops || 1;
    const speed = st.speed || 1;
    s.t0 = this.time;
    s.n = loops;
    if (clip) {
      const a = this.mixer.clipAction(clip);
      if (this.cur === a) a.reset(); else this.fadeTo(a, s.i ? 0.08 : 0.12);
      a.setLoop(loops > 1 ? THREE.LoopRepeat : THREE.LoopOnce, loops);
      a.clampWhenFinished = true;
      a.timeScale = speed;
      s.dur = (clip.duration * loops) / speed;
    } else {
      if (this.mixer) this.fadeTo(this.base());
      s.dur = st.len || 0.9;
    }
  }

  update(dt) {
    this.locomotion?.restore();
    this.time += dt;
    if (this.seq && this.time - this.seq.t0 >= this.seq.dur) this.nextStep();
    if (this.mixer) this.mixer.update(dt);
    let d = this.yaw - this.body.rotation.y;
    d = Math.atan2(Math.sin(d), Math.cos(d));
    this.body.rotation.y += d * Math.min(1, dt * 10);
    const v = this.visual;
    v.position.set(0, 0, 0);
    v.rotation.set(0, v.userData.rotY, 0);
    v.scale.setScalar(v.userData.s);
    const s = this.seq;
    const st = s && s.steps[s.i];
    if (st && st.fx && FX[st.fx]) FX[st.fx](v, Math.min(1, (this.time - s.t0) / s.dur), s.n, s.dur);
    else if (!s && !this.mixer && this.t && !this.moving) FX.idle(v, this.time);
    v.position.y += this.locomotion?.apply(this.moving, this.time, dt) || 0;
  }

  dispose() {
    this.disposed = true;
    if (this.group.parent) this.group.parent.remove(this.group);
    if (this.mixer) this.mixer.stopAllAction();
    if (this.placeholder && this.visual) this.visual.traverse((o) => { if (o.isMesh) o.material.dispose(); });
    else disposeSkeletons(this.visual); // cloned skeletons keep a bone texture on the GPU
    for (const m of this.mats) m.dispose();
  }
}

// ---------- pictures ----------
const THUMB = 160;
const thumbs = new Map();
const thumbUrl = new Map();
let tctx = null;
function thumb(legendId) {
  const item = itemOf(legendId);
  if (!item) return Promise.resolve(null);
  if (thumbs.has(item.id)) return thumbs.get(item.id);
  const p = modelFor(item.id).then((t) => {
    if (!tctx) {
      const canvas = document.createElement('canvas');
      canvas.width = canvas.height = THUMB;
      const r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true, preserveDrawingBuffer: true });
      r.setPixelRatio(1);
      r.setSize(THUMB, THUMB, false);
      r.outputColorSpace = THREE.SRGBColorSpace;
      r.setClearColor(0x000000, 0);
      const scene = new THREE.Scene();
      scene.add(new THREE.HemisphereLight('#ffffff', '#3a4660', 2.2));
      const key = new THREE.DirectionalLight('#fff6e8', 2.4);
      key.position.set(1.5, 3, 4);
      scene.add(key);
      tctx = { canvas, r, scene, cam: new THREE.PerspectiveCamera(28, 1, 0.01, 50) };
    }
    const { r, scene, cam, canvas } = tctx;
    let obj;
    if (t) {
      obj = t.clone();
      const c = clipOf(t, ['idle']) || t.clip;
      if (c) { const m = new THREE.AnimationMixer(obj); m.clipAction(c).play(); m.update(0.3); }
    } else obj = makePlaceholder(DT.LINE[item.id] || DT.LINE.patamon);
    scene.add(obj);
    obj.updateMatrixWorld(true);
    const view = Math.max(1.1, Math.min(1.5, (t ? t.footprint : 1) * 0.9));
    const d = view / 2 / Math.tan((cam.fov * Math.PI) / 360);
    cam.position.set(Math.sin(0.4) * d, 0.5 + d * 0.15, Math.cos(0.4) * d);
    cam.lookAt(0, 0.48, 0);
    r.render(scene, cam);
    scene.remove(obj);
    if (!t) obj.traverse((o) => { if (o.isMesh) o.material.dispose(); });
    let url = canvas.toDataURL('image/webp', 0.86);
    if (!url.startsWith('data:image/webp')) url = canvas.toDataURL('image/png');
    thumbUrl.set(item.id, url);
    for (const el of document.querySelectorAll(`.pt[data-lk="${item.id}"]`)) if (!el.querySelector('img')) el.insertAdjacentHTML('beforeend', `<img src="${url}" alt="" style="object-fit:contain">`);
    return url;
  });
  thumbs.set(item.id, p);
  return p;
}

function html(legendId, cls = '', style = '') {
  const item = itemOf(legendId);
  if (!item) return '';
  // a digivolved form shows its battle portrait
  const f = formOf(legendId);
  // an In-Training form drawn with another legend's model shows that legend's picture
  if (f && f.legend && f.legend !== item.id) return html(f.legend, cls, style);
  if (f && f.model && DT.Portraits) return DT.Portraits.html(f.line, f.stage, cls).replace('style="', `data-lform="${item.id}@${f.id}" style="${style};`);
  const url = thumbUrl.get(item.id);
  if (!url) thumb(item.id);
  return `<span class="pt ${cls}" data-lk="${item.id}" style="--fc:#8a6d1f;${style}"><b>${item.name.slice(0, 2)}</b>${url ? `<img src="${url}" alt="" style="object-fit:contain">` : ''}</span>`;
}

// ---------- API ----------
// The renderer keeps .legend (the viewer's) and .oppLegend (the combat opponent's, with .playerId).
function viewOf(playerId) {
  const r = DT.UI && DT.UI.renderer && DT.UI.renderer();
  const g = DT.UI && DT.UI.game && DT.UI.game();
  if (!r || !g) return null;
  if (g.human && g.human.id === playerId) return r.legend ? r.legend.view : null;
  if (r.oppLegend && r.oppLegend.playerId === playerId) return r.oppLegend.view;
  return null;
}

DT.Legends = {
  LegendView,
  modelFor,
  createView: (legendId, opts) => new LegendView(legendId, opts),
  play(playerId, role) { const v = viewOf(playerId); return v ? v.play(role) : false; },
  anchor(playerId) {
    const v = viewOf(playerId);
    if (!v) return null;
    const p = v.group.position;
    return DT.UI.renderer().project(p.x, p.y + v.height * v.group.scale.y + 0.3, p.z);
  },
  credits: () => DT.Cosmetics.list('legend').map((x) => x.data && x.data.credit).filter(Boolean),
  thumb,
  html,
};
