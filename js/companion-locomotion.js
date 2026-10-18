// Movement fallback shared with native CompanionLocomotion. Authored runs always win.
import * as THREE from 'three';

const GLIDERS = new Set([
  'ryudamon:2', 'hagurumon:0', 'biyomon:1', 'biyomon:2', 'demidevimon:0', 'demidevimon:1',
  'dracomon:2', 'extra:Bakemon', 'extra:DW1_Airdramon', 'extra:DW1_Bakemon', 'extra:DW1_Birdramon',
  'extra:DW1_Gigadramon', 'extra:DW1_Vademon', 'extra:Phoenixmon', 'falcomon:2',
  'fanbeemon:0', 'fanbeemon:1', 'fanbeemon:2', 'hawkmon:1', 'lalamon:0', 'lalamon:1', 'lalamon:2',
  'lucemon:0', 'machinedramon:0', 'palmon:2', 'patamon:1', 'patamon:2', 'salamon:2',
  'tentomon:0', 'tentomon:1', 'tentomon:2', 'wormmon:1', 'wormmon:2',
  // Same source assets or the same airborne forms in other catalog sets.
  'phoenixmon:0', 'phoenixmon:1', 'phoenixmon:2', 'bakemon:0', 'bakemon:1', 'bakemon:2',
  'ac:Sunflowmon', 'ac:Aquilamon', 'drg:Sunflowmon', 'drg:Aquilamon',
  'extra:DW1_Hagurumon', 'extra:Vademon', 'drg:Tentomon', 'ac:Airdramon', 'drg:Airdramon',
  'ac:Devimon', 'ac:Gigadramon', 'drg:Gigadramon', 'catalog:CS_DemiDevimon:chr097',
  'catalog:CS_Hagurumon:chr009', 'catalog:CS_Lalamon:chr056', 'catalog:CS_Lucemon:chr390',
  'catalog:CS_Angemon:chr087', 'catalog:CS_Bakemon:chr013', 'catalog:CS_Birdramon:chr308',
  'catalog:CS_Stingmon:chr091', 'catalog:L_Hagurumon:chr009', 'catalog:L_Aquilamon:chr015',
  'catalog:L_Devimon:chr093', 'catalog:L_Sunflowmon:chr058', 'catalog:L_Waspmon:chr454',
  'catalog:L_MagnaAngemon:chr101', 'catalog:TS_Funbeemon:chr042', 'catalog:TS_Lalamon:chr056',
  'catalog:TS_Lucemon:chr390', 'catalog:TS_PicoDevimon:chr097', 'catalog:TS_Solarmon_Hagurumon:chr009',
  'catalog:TS_Tentomon:chr303', 'ac:Dianamon', 'ac:Flymon',
  'catalog:CS_Seadramon:chr347', 'ac:Phantomon', 'drg:Phantomon',
]);
// Only reviewed weight-bearing forelimbs; never infer this from humanoid arm names alone.
const FORELEG_SOURCES = new Set(['ac:Dobermon', 'bearmon:1', 'ac:Grizzlymon']);
const HIP = /(?:^|[_ ])(?:momo|thigh|upleg|leg)(?:0?1)?[_ ]?([lr])(?:[_ ]|$)|(?:^|[_ ])([lr])[_ ](?:momo|thigh|leg)$/di;
const KNEE = /(?:^|[_ ])(?:hiza|knee\d*|shin|calf)(?:[_ ]?[lr])?(?:[_ ]|$)/i;
const FOOT = /(?:^|[_ ])(?:kakato|foot\d*|ankle|ancle|unkle|asi)(?:[_ ]?[lr])?(?:[_ ]|$)/i;
const SHOULDER = /(?:^|[_ ])(?:upperarm|arm)[_ ]?([lr])(?:[_ ]|$)|(?:^|[_ ])([lr])[_ ](?:upperarm|arm)$/di;
const ELBOW = /(?:^|[_ ])(?:elbow|forearm)(?:[_ ]?[lr])?(?:[_ ]|$)/i;
const HAND = /(?:^|[_ ])hand(?:[_ ]?[lr])?(?:[_ ]|$)/i;
const boneName = value => value.replace(/_0\d+$/, '');
const point = b => new THREE.Vector3().setFromMatrixPosition(b.matrixWorld);
const below = (b, ancestor) => { for (let p = b.parent; p; p = p.parent) if (p === ancestor) return true; return false; };

export class CompanionLocomotion {
  constructor(scene, source, authored) {
    this.scene = scene; this.kind = authored ? 'authored' : GLIDERS.has(source) ? 'glide' : 'hop';
    this.weight = 0; this.legs = []; this.saved = [];
    if (this.kind !== 'hop') return;
    const bones = new Set(); scene.traverse(o => { if (o.isSkinnedMesh) for (const bone of o.skeleton.bones) bones.add(bone); });
    const candidates = new Map();
    scene.updateMatrixWorld(true);
    for (const hip of bones) {
      const name = boneName(hip.name); let match = HIP.exec(name), foreleg = false;
      if (!match && FORELEG_SOURCES.has(source)) { match = SHOULDER.exec(name); foreleg = !!match; }
      if (!match) continue;
      const [start, end] = match.indices[1] || match.indices[2], side = name[start].toLowerCase() === 'l' ? 1 : -1;
      const knee = [...bones].find(b => (foreleg ? ELBOW : KNEE).test(boneName(b.name)) && below(b, hip));
      const foot = knee && [...bones].find(b => (foreleg ? HAND : FOOT).test(boneName(b.name)) && below(b, knee));
      if (!foot) continue;
      const key = (name.slice(0, start) + '?' + name.slice(end)).toLowerCase();
      if (!candidates.has(key)) candidates.set(key, []);
      candidates.get(key).push({ hip, knee, foot, side });
    }
    const pairs = [...candidates].filter(([, p]) => p.length === 2 && p[0].side + p[1].side === 0).sort(([a], [b]) => a < b ? -1 : a > b ? 1 : 0).map(([, p]) => p);
    if (pairs.length < 1 || pairs.length > 2) return;
    const lateral = point(pairs[0].find(p => p.side > 0).hip).sub(point(pairs[0].find(p => p.side < 0).hip));
    if (lateral.lengthSq() < 1e-16) return;
    lateral.normalize(); this.lateral = lateral;
    for (let pair = 0; pair < pairs.length; pair++) for (const c of pairs[pair]) {
      const thigh = point(c.knee).sub(point(c.hip)), shin = point(c.foot).sub(point(c.knee));
      if (thigh.lengthSq() < 1e-18 || shin.lengthSq() < 1e-18) { this.legs = []; return; }
      this.legs.push({ ...c, pair, bend: thigh.cross(shin).dot(lateral) >= 0 ? 1 : -1 });
    }
    this.root = this.legs[0].hip;
    while (this.root.parent && this.root !== scene) this.root = this.root.parent;
    if (this.legs.some(leg => !below(leg.hip, this.root))) { this.legs = []; return; }
    this.kind = 'grounded';
  }
  get usesTravelHop() { return this.kind === 'hop'; }
  reset() { this.restore(); this.weight = 0; }
  restore() {
    for (const [bone, rotation, position] of this.saved) { bone.quaternion.copy(rotation); bone.position.copy(position); }
    this.saved = [];
  }
  apply(moving, time, delta) {
    this.weight = Math.max(0, Math.min(1, this.weight + (moving ? 1 : -1) * delta * 5));
    if (!this.weight || this.kind === 'authored' || this.kind === 'hop') return 0;
    if (this.kind === 'glide') return Math.sin(time * 4) * .025 * this.weight;
    this.scene.updateMatrixWorld(true);
    const floor = Math.min(...this.legs.map(l => point(l.foot).y));
    const lateral = point(this.legs.find(l => l.pair === 0 && l.side > 0).hip).sub(point(this.legs.find(l => l.pair === 0 && l.side < 0).hip)).normalize();
    const turn = (bone, angle) => {
      this.saved.push([bone, bone.quaternion.clone(), bone.position.clone()]);
      const inverse = (bone.parent?.matrixWorld || new THREE.Matrix4()).clone().invert();
      const axis = lateral.clone().transformDirection(inverse);
      bone.quaternion.premultiply(new THREE.Quaternion().setFromAxisAngle(axis, angle));
      this.scene.updateMatrixWorld(true);
    };
    for (const leg of this.legs) {
      const phase = time * 8.5 + (leg.side < 0 ? Math.PI : 0) + leg.pair * Math.PI;
      const stride = Math.sin(phase), hip = stride * .3 * this.weight, knee = Math.max(0, stride) * .28 * leg.bend * this.weight;
      turn(leg.hip, hip); turn(leg.knee, knee); turn(leg.foot, -hip - knee);
    }
    this.saved.push([this.root, this.root.quaternion.clone(), this.root.position.clone()]);
    const lift = floor - Math.min(...this.legs.map(l => point(l.foot).y));
    const offset = new THREE.Vector3(0, lift, 0);
    if (this.root.parent) {
      const inverse = this.root.parent.matrixWorld.clone().invert(), e = inverse.elements;
      offset.set(lift * e[4], lift * e[5], lift * e[6]);
    }
    this.root.position.add(offset); this.scene.updateMatrixWorld(true);
    return 0;
  }
}
