// Signature move looks, one spec per ability name, plus a small basic-attack spec per line.
// Visual only: the combat sim (js/combat.js) decides damage, targets and timing; these specs only say how it looks.
// The effect primitives are in js/moveprims.js (deliveries, impacts) built on js/vfx.js (pools, particles).
//
// ---------- move spec format ----------
// MOVES['Ability Name'] = {
//   elem:    'fire' | 'ice' | 'thunder' | 'water' | 'plant' | 'wind' | 'holy' | 'dark' | 'metal' | 'beast'
//            (sets the default colors, the cast banner color and the cast sound)
//   colors:  ['#main', '#highlight']        optional, overrides the element palette
//   release: seconds after the cast until the move leaves the caster (default 0.3). Clamped to the model's
//            special clip, so the hit lands on the animation's swing.
//   charge:  wind-up at the caster during release: 'aura' (default) | 'mouth' | 'hands' | 'back' | 'sky' | 'ground' | 'none'
//   impact:  default impact for every hit: 'blast' | 'frost' | 'zap' | 'splash' | 'holy' | 'dark' | 'poison' |
//            'leaf' | 'metal' | 'hit' | 'wind' | 'none'
//   shake:   camera shake amplitude on the first hit (0.05 small, 0.15 big, 0.3 huge)
//   scale:   optional [rookie, champion, ultimate] size multipliers (default STAGE_SCALE)
//   fx:      list of delivery steps, each [primitive, options]. All steps start at release + options.at.
// }
// Delivery primitives and their main options (every step also takes at, color ('a' | 'b' | 'w' | '#hex'),
// impact, impactSize, and to: 'targets' (default) | 'center' | 'self'):
//   orb      projectile: size, speed|dur, arc, trail, head (sprite cell), count, gap, from, side, wings, wobble
//   breath   stream cone from the mouth: dur, width, cell, mix, size, speed, rate
//   beam     laser: width, dur, charge, twin, from, glyph ('x' | 'tri'), core, overshoot, spiral
//   missiles count, size, dur, from, organic, curve, arc, smoke
//   bolt     lightning: from ('sky' | 'caster'), count, width, height, area (extra random strikes)
//   slash    arcs at the target: count, size, claw, spread, vertical, gap, dur
//   dash     afterimages along the leap, then slashes: ghosts, slashes, claw, dur, size
//   erupt    ground eruption: kind ('flame' | 'water' | 'dark' | 'ice' | 'rock' | 'thorn' | 'metal'), count, height, area
//   meteor   size, count, height, dur, trail
//   sphere   energy ball that grows, then launches: size, grow, from ('above' | 'mouth' | 'hands'), trail, cell, core
//   wave     sword wave along the ground: width, count, dur, flat
//   arrows   arrows of light: count, rain, size, cell, gap
//   cross    holy cross on the target: size, orbs, dur
//   vortex   swirling disc on the ground: size, dur, cell, up
//   portal   gate behind the target that pulls it in: size, dur
//   spikes   line of spikes from caster to target: count, kind, size, gap
//   whip     lash from the caster: dur, width, count, sway, lightning, leafy, from
//   swarm    flying sprites (bubbles, notes, bats, fish, cards, feathers...): cell, count, size, speed, wobble,
//            spin, spread, fan, hold, arc, gap, color2, tint, glow
//   shock    ground shockwave: size, hammer, debris, cell
//   dome     shield bubble: size, orbit (cell), count
//   heal     on each healed ally: cell, color2, size, motes
//   tornado  rising spiral: cell, height, dur, size, psize
//   rings    flying rings: count, size, gap, dur, from
//   volley   gunfire tracers: count, rate, size, guns, cell, spread, speed, from
//   streak   body charge in a blazing streak: width, dur, trail
//   wheel    spinning wheel or saw: size, cell, dur, saw
//   net      web thrown at the target: size
//   pillar   light from the sky: size, height, dur
//   rain     falling petals / feathers / snow: cell, count, height, size, dur
//   glyph    seal at the caster: glyph ('x' | 'tri' | 'cross' | 'circle'), size, dur, place ('ground')
//   drain    life flows from targets to the caster
// from: 'mouth' (default) | 'chest' | 'hands' | 'back' | 'sky'.
// trail: 'flame' | 'smoke' | 'spark' | 'bubble' | 'leaf' | 'petal' | 'dark' | 'ice' | 'holy' | 'wind' | 'data' | 'zap' | 'note'.
// Sprite cells: glow spark flame smoke bubble feather note card leaf petal shard streak cross gear bat fish square
//   needle star ring seed blade bolt paw drop snow skull heart hex dot ember wisp.
//
// Stun moves get circling stars and drain moves send life back automatically, from the impact.
//
// ---------- basic attack format ----------
// BASIC[lineId] = { kind, elem?, colors?, head?, trail?, size?, speed?, arc?, spin? }
//   kind: melee 'claw' | 'bite' | 'punch' | 'horn' | 'slash' | 'peck' | 'kick' | 'tail' | 'sting', or 'shot' (ranged)
//   head / trail / size / speed / arc / spin: for 'shot', the flying sprite
//
// A new line with no specs still works: specFor() builds one from the ability name keywords and the line's
// ability type, and basicFor() picks a claw or an element shot.

const G = typeof window !== 'undefined' ? window : globalThis;
const DT = (G.DT = G.DT || {});

export const PALETTE = {
  fire: ['#ff5a1a', '#ffd36a'], ice: ['#7fdcff', '#ffffff'], thunder: ['#ffe94a', '#a8dcff'], water: ['#2f9bff', '#bff0ff'],
  plant: ['#5cff6a', '#ffb3e6'], wind: ['#b8fff0', '#ffffff'], holy: ['#fff0a0', '#ffffff'], dark: ['#a04aff', '#ff3a7a'],
  metal: ['#ffb04a', '#d8e4f4'], beast: ['#ffffff', '#ff9a5a'],
};
export const STAGE_SCALE = [1, 1.35, 1.75];

export const MOVES = {
  // ---- Agumon line ----
  'Pepper Breath': { elem: 'fire', charge: 'mouth', release: 0.22, impact: 'blast', fx: [['orb', { size: 0.2, speed: 13, arc: 0.15, trail: 'flame' }]] },
  'Nova Blast': { elem: 'fire', charge: 'mouth', release: 0.3, impact: 'blast', shake: 0.1, fx: [['orb', { size: 0.42, speed: 11, arc: 0.2, trail: 'flame', impactSize: 1.4 }]] },
  'Giga Destroyer': { elem: 'fire', colors: ['#ff7a2a', '#ffe0a0'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.22,
    fx: [['missiles', { count: 2, size: 0.17, from: 'chest', organic: true, curve: 1.4, arc: 1.4, dur: 0.6, impactSize: 1.6 }]] },

  // ---- Gabumon line ----
  'Blue Blaster': { elem: 'ice', colors: ['#3a8bff', '#bfe8ff'], charge: 'mouth', release: 0.22, impact: 'frost', fx: [['breath', { dur: 0.32, width: 0.25, cell: 'flame', size: 0.32, speed: 9 }]] },
  'Howling Blaster': { elem: 'ice', colors: ['#2a6bff', '#9fe0ff'], charge: 'mouth', release: 0.28, impact: 'frost', shake: 0.08,
    fx: [['breath', { dur: 0.55, width: 0.45, cell: 'flame', mix: 'snow', size: 0.42, speed: 10, rate: 320 }]] },
  'Wolf Claw': { elem: 'beast', colors: ['#5ab0ff', '#e8f6ff'], charge: 'hands', release: 0.25, impact: 'hit', shake: 0.12,
    fx: [['streak', { width: 0.6, dur: 0.22, trail: 'wind' }], ['slash', { at: 0.08, count: 3, claw: true, size: 1.4, spread: 0.5 }]] },

  // ---- Tentomon line ----
  'Super Shocker': { elem: 'thunder', charge: 'sky', release: 0.25, impact: 'zap', fx: [['bolt', { from: 'caster', width: 0.07 }]] },
  'Electro Shocker': { elem: 'thunder', charge: 'hands', release: 0.2, impact: 'zap', shake: 0.08,
    fx: [['sphere', { size: 0.45, grow: 0.3, from: 'hands', trail: 'zap', cell: 'bolt', impactSize: 1.4 }], ['bolt', { at: 0.62, to: 'center', count: 1, area: 2 }]] },
  'Horn Buster': { elem: 'thunder', colors: ['#c8ff4a', '#ffffff'], charge: 'mouth', release: 0.28, impact: 'zap', shake: 0.15,
    fx: [['beam', { width: 0.3, dur: 0.45, charge: 0.12, to: 'center' }], ['bolt', { at: 0.15, to: 'center', count: 2, area: 3, height: 8 }]] },

  // ---- Palmon line ----
  'Poison Ivy': { elem: 'plant', colors: ['#3fd04a', '#c6ff7a'], charge: 'hands', release: 0.2, impact: 'poison', fx: [['whip', { dur: 0.4, width: 0.1, count: 2, sway: 1.2 }]] },
  'Needle Spray': { elem: 'plant', colors: ['#c8ff6a', '#fff6c0'], charge: 'aura', release: 0.25, impact: 'hit',
    fx: [['volley', { count: 16, rate: 0.02, cell: 'needle', size: 0.5, spread: 0.5, from: 'chest', speed: 20 }]] },
  'Flower Cannon': { elem: 'plant', colors: ['#ff7ad8', '#fff0fa'], charge: 'hands', release: 0.3, impact: 'leaf', shake: 0.1,
    fx: [['glyph', { glyph: 'circle', size: 1.4, dur: 0.6 }], ['beam', { width: 0.24, dur: 0.45, charge: 0.1, color: '#ff8ae0', core: '#ffffff' }], ['swarm', { at: 0.05, cell: 'petal', count: 14, size: 0.25, speed: 12, wobble: 0.6 }]] },

  // ---- Gomamon line ----
  'Marching Fishes': { elem: 'water', colors: ['#3aa0ff', '#ffd27a'], charge: 'ground', release: 0.2, impact: 'splash',
    fx: [['swarm', { cell: 'fish', count: 14, size: 0.42, speed: 5, wobble: 1.4, fan: true, spread: 0.6, arc: 0.2, color2: '#ff9a4a', tint: true }], ['shock', { at: 0.35, debris: false, cell: 'bubble' }]] },
  'Harpoon Torpedo': { elem: 'water', colors: ['#3aa0ff', '#e6f6ff'], charge: 'ground', release: 0.28, impact: 'splash', shake: 0.12,
    fx: [['missiles', { count: 1, size: 0.2, from: 'mouth', curve: 0.3, arc: 0.3, dur: 0.4, impact: 'splash', impactSize: 1.4 }], ['shock', { at: 0.3, debris: false, cell: 'drop' }]] },
  "Vulcan's Hammer": { elem: 'metal', colors: ['#ffb04a', '#fff2c0'], charge: 'sky', release: 0.3, impact: 'metal', shake: 0.3,
    fx: [['shock', { hammer: true, cell: 'spark' }], ['erupt', { at: 0.2, kind: 'rock', to: 'center', area: true, count: 4, height: 1.2 }]] },

  // ---- Biyomon line ----
  'Spiral Twister': { elem: 'plant', colors: ['#3fff7a', '#d0ffd8'], charge: 'mouth', release: 0.2, impact: 'blast',
    fx: [['orb', { size: 0.22, speed: 11, arc: 0.3, trail: 'flame', side: 0.3, wobble: 0.5 }]] },
  'Meteor Wing': { elem: 'fire', charge: 'sky', release: 0.22, impact: 'blast', fx: [['orb', { size: 0.26, speed: 10, from: 'sky', arc: 0.8, trail: 'flame' }]] },
  'Wing Blade': { elem: 'fire', colors: ['#ff6a1a', '#fff0a0'], charge: 'hands', release: 0.25, impact: 'blast', shake: 0.08,
    fx: [['orb', { size: 0.32, speed: 10, arc: 0.4, trail: 'flame', wings: true, impactSize: 1.3 }]] },

  // ---- Hagurumon line ----
  'Darkness Gear': { elem: 'metal', colors: ['#7a5aa0', '#d0c0ff'], charge: 'aura', release: 0.2, impact: 'metal',
    fx: [['dome', { orbit: 'gear', count: 4 }], ['swarm', { at: 0.15, cell: 'gear', count: 3, size: 0.4, speed: 7, spin: 10, wobble: 0.6 }]] },
  'Guardian Barrage': { elem: 'metal', charge: 'aura', release: 0.25, impact: 'blast',
    fx: [['dome', { orbit: 'hex', count: 6 }], ['missiles', { at: 0.1, count: 4, size: 0.1, from: 'chest', curve: 1, arc: 1.2, dur: 0.5, impactSize: 0.6 }]] },
  'Gatling Attack': { elem: 'metal', colors: ['#ffb04a', '#e8f0ff'], charge: 'aura', release: 0.25, impact: 'blast', shake: 0.1,
    fx: [['dome', { size: 1.2 }], ['missiles', { at: 0.05, count: 6, size: 0.12, from: 'chest', curve: 1.2, arc: 1.0, dur: 0.5, gap: 0.05, impactSize: 0.8 }]] },

  // ---- DemiDevimon line ----
  'Demi Dart': { elem: 'dark', colors: ['#c05aff', '#9aff6a'], charge: 'none', release: 0.1, impact: 'poison',
    fx: [['dash', { ghosts: 4, slashes: 0, dur: 0.18 }], ['orb', { at: 0.12, head: 'needle', size: 0.22, speed: 16, arc: 0.05, halo: false, color: '#c05aff', headColor: '#e8f0ff' }]] },
  'Death Claw': { elem: 'dark', colors: ['#8a2aff', '#ff3a7a'], charge: 'none', release: 0.12, impact: 'dark',
    fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.2 }], ['whip', { at: 0.12, dur: 0.3, width: 0.16, sway: 0.4, leafy: false }], ['slash', { at: 0.24, count: 2, claw: true, size: 1.3 }]] },
  'Neo Inferno': { elem: 'dark', colors: ['#9a2aff', '#ff5a3a'], charge: 'none', release: 0.12, impact: 'dark', shake: 0.15,
    fx: [['dash', { ghosts: 6, slashes: 1, dur: 0.2, size: 1.5 }], ['erupt', { at: 0.22, kind: 'dark', height: 2.4 }]] },

  // ---- Guilmon line ----
  'Pyro Sphere': { elem: 'fire', colors: ['#ff4a1a', '#ffd06a'], charge: 'mouth', release: 0.25, impact: 'blast', fx: [['orb', { size: 0.3, speed: 12, arc: 0.2, trail: 'flame', impactSize: 1.2 }]] },
  'Pyro Blaster': { elem: 'fire', colors: ['#ff3a1a', '#ffe08a'], charge: 'mouth', release: 0.28, impact: 'blast', shake: 0.12,
    fx: [['orb', { size: 0.48, speed: 11, arc: 0.15, trail: 'flame', impactSize: 1.6 }]] },
  'Atomic Blaster': { elem: 'fire', colors: ['#ff3a6a', '#ffe0ea'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.2,
    fx: [['beam', { twin: true, from: 'chest', width: 0.22, dur: 0.55, charge: 0.1, core: '#ffffff' }]] },

  // ---- Renamon line ----
  'Diamond Storm': { elem: 'ice', colors: ['#9ad8ff', '#ffffff'], charge: 'none', release: 0.12, impact: 'frost',
    fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.18, color: '#ffd27a' }], ['swarm', { at: 0.1, cell: 'shard', count: 14, size: 0.28, speed: 14, fan: true, spread: 0.9, wobble: 0.3, arc: 0.6 }]] },
  'Dragon Wheel': { elem: 'fire', colors: ['#3a7aff', '#bfe8ff'], charge: 'none', release: 0.12, impact: 'blast', shake: 0.1,
    fx: [['dash', { ghosts: 6, slashes: 0, dur: 0.2 }], ['wheel', { at: 0.05, size: 0.6, dur: 0.3 }]] },
  'Talisman of Light': { elem: 'holy', colors: ['#ffe08a', '#ffffff'], charge: 'none', release: 0.12, impact: 'holy', shake: 0.1,
    fx: [['dash', { ghosts: 4, slashes: 0, dur: 0.16 }], ['glyph', { glyph: 'circle', at: 0.05, size: 1.4, dur: 0.7, place: 'ground' }], ['swarm', { at: 0.1, cell: 'card', count: 6, size: 0.32, speed: 9, fan: true, spread: 0.8, hold: 0.15, spin: 4, wobble: 0.4 }]] },

  // ---- Terriermon line ----
  'Bunny Blast': { elem: 'wind', colors: ['#9affc8', '#ffffff'], charge: 'mouth', release: 0.18, impact: 'wind', fx: [['orb', { size: 0.18, speed: 15, arc: 0.1, trail: 'wind' }]] },
  'Gargo Laser': { elem: 'metal', colors: ['#7affb0', '#ffffff'], charge: 'hands', release: 0.2, impact: 'metal', fx: [['volley', { count: 6, rate: 0.03, size: 0.45, guns: 2, cell: 'streak' }]] },
  'Tri-Beam': { elem: 'wind', colors: ['#4aff9a', '#e8fff0'], charge: 'hands', release: 0.22, impact: 'zap', fx: [['beam', { width: 0.14, dur: 0.35, glyph: 'tri', core: '#ffffff' }]] },

  // ---- Patamon line ----
  'Boom Bubble': { elem: 'wind', colors: ['#bfe8ff', '#ffffff'], charge: 'mouth', release: 0.22, impact: 'wind',
    fx: [['orb', { head: 'bubble', size: 0.24, speed: 9, arc: 0.25, trail: 'bubble', headColor: '#e8f8ff' }]] },
  'Hand of Fate': { elem: 'holy', colors: ['#ffd84a', '#fffbe0'], charge: 'hands', release: 0.3, impact: 'holy', shake: 0.12,
    fx: [['beam', { width: 0.3, dur: 0.45, charge: 0.12, from: 'hands', core: '#ffffff', spiral: true }]] },
  'Gate of Destiny': { elem: 'holy', colors: ['#ffd84a', '#c08aff'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.18, fx: [['portal', { size: 1.4, dur: 1.1 }]] },

  // ---- Salamon line ----
  'Puppy Howl': { elem: 'holy', colors: ['#ffe8a0', '#ffffff'], charge: 'mouth', release: 0.2, impact: 'none',
    fx: [['shock', { to: 'self', size: 1.6, debris: false, cell: 'note' }], ['heal', { cell: 'note', color: '#ffe08a', color2: '#ffffff' }]] },
  'Lightning Paw': { elem: 'holy', colors: ['#fff0a0', '#ffffff'], charge: 'aura', release: 0.2, impact: 'none',
    fx: [['glyph', { glyph: 'circle', size: 1.6, dur: 0.5 }], ['heal', { cell: 'paw', color: '#ffe08a', color2: '#ffffff' }]] },
  "Heaven's Charm": { elem: 'holy', colors: ['#ffc83a', '#fff0b0'], charge: 'sky', release: 0.3, impact: 'none', shake: 0.05,
    fx: [['cross', { to: 'self', size: 1.0, impact: 'holy' }], ['heal', { at: 0.25, cell: 'star', color: '#fff0a0', color2: '#ffffff' }]] },

  // ---- Betamon line ----
  'Electric Shock': { elem: 'thunder', colors: ['#5ab8ff', '#ffffff'], charge: 'aura', release: 0.2, impact: 'zap', fx: [['bolt', { from: 'caster', width: 0.06 }]] },
  'Ice Blast': { elem: 'ice', charge: 'mouth', release: 0.25, impact: 'frost', fx: [['breath', { dur: 0.45, width: 0.3, cell: 'snow', mix: 'shard', size: 0.3, speed: 10, rate: 260 }]] },
  'Thunder Javelin': { elem: 'thunder', colors: ['#ffe94a', '#9ad0ff'], charge: 'mouth', release: 0.28, impact: 'zap', shake: 0.15,
    fx: [['bolt', { from: 'caster', width: 0.2, dur: 0.4 }], ['bolt', { at: 0.05, count: 2, width: 0.12 }]] },

  // ---- Wormmon line ----
  'Sticky Net': { elem: 'plant', colors: ['#e8ffe0', '#9aff6a'], charge: 'none', release: 0.12, impact: 'poison',
    fx: [['dash', { ghosts: 3, slashes: 0, dur: 0.16 }], ['net', { at: 0.1, size: 0.9 }]] },
  'Spiking Strike': { elem: 'plant', colors: ['#7aff4a', '#ffffff'], charge: 'none', release: 0.12, impact: 'hit',
    fx: [['dash', { ghosts: 5, slashes: 1, dur: 0.18 }], ['volley', { at: 0.18, count: 1, cell: 'needle', size: 1.2, spread: 0, speed: 30, from: 'chest' }]] },
  'Hell Masquerade': { elem: 'plant', colors: ['#ffe94a', '#7aff4a'], charge: 'none', release: 0.12, impact: 'hit', shake: 0.12,
    fx: [['dash', { ghosts: 6, slashes: 6, dur: 0.2, size: 1.2, spread: 1.2 }]] },

  // ---- Elecmon line ----
  'Super Thunder Strike': { elem: 'thunder', colors: ['#ffe94a', '#ff8a4a'], charge: 'aura', release: 0.25, impact: 'zap', shake: 0.08,
    fx: [['shock', { debris: false, cell: 'spark' }], ['bolt', { from: 'caster', launch: 'back' }]] },
  'Fist of the Beast King': { elem: 'fire', colors: ['#ffb02a', '#fff2b0'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.18,
    fx: [['orb', { size: 0.5, speed: 10, arc: 0.1, from: 'hands', trail: 'flame', wings: true, impactSize: 1.5 }], ['shock', { at: 0.3, size: 1.5, debris: false }]] },
  'Ice Fist': { elem: 'ice', colors: ['#8ae0ff', '#ffffff'], charge: 'hands', release: 0.3, impact: 'frost', shake: 0.2,
    fx: [['shock', { cell: 'snow' }], ['erupt', { at: 0.05, kind: 'ice', to: 'center', area: true, count: 6, height: 1.6 }]] },

  // ---- Hawkmon line ----
  'Beak Pummel': { elem: 'beast', colors: ['#ffffff', '#ff6a4a'], charge: 'none', release: 0.15, impact: 'hit',
    fx: [['streak', { width: 0.4, dur: 0.18, trail: 'wind' }], ['slash', { at: 0.06, count: 4, size: 0.8, gap: 0.05, spread: 1.4 }]] },
  'Blast Rings': { elem: 'wind', colors: ['#ffd84a', '#ffffff'], charge: 'mouth', release: 0.22, impact: 'wind', fx: [['rings', { count: 4, size: 0.35, gap: 0.07 }]] },
  'Static Force': { elem: 'holy', colors: ['#ff8ad8', '#ffffff'], charge: 'hands', release: 0.2, impact: 'holy', shake: 0.15,
    fx: [['sphere', { size: 0.6, grow: 0.35, from: 'hands', trail: 'holy', impactSize: 1.7 }]] },

  // ---- Keramon line ----
  'Crazy Giggle': { elem: 'dark', colors: ['#ff3a5a', '#ffd0e0'], charge: 'mouth', release: 0.2, impact: 'dark', fx: [['orb', { size: 0.22, speed: 13, arc: 0.15, trail: 'data' }]] },
  'Data Crusher': { elem: 'metal', colors: ['#4affd8', '#ffffff'], charge: 'mouth', release: 0.2, impact: 'metal', fx: [['orb', { head: 'square', size: 0.22, speed: 14, arc: 0.2, trail: 'data', spin: 8 }]] },
  'Spider Shooter': { elem: 'dark', colors: ['#ff4a8a', '#ffe0ff'], charge: 'mouth', release: 0.2, impact: 'dark', fx: [['orb', { size: 0.2, speed: 15, arc: 0.1, trail: 'dark', count: 2, gap: 0.05 }]] },

  // ---- Lopmon line ----
  'Blazing Ice': { elem: 'ice', colors: ['#8ad8ff', '#ffffff'], charge: 'mouth', release: 0.2, impact: 'frost',
    fx: [['dome', { orbit: 'snow', count: 5 }], ['spikes', { at: 0.05, count: 5, kind: 'ice', size: 0.8 }]] },
  'Kongou Kick': { elem: 'beast', colors: ['#ffd27a', '#ffffff'], charge: 'aura', release: 0.2, impact: 'hit',
    fx: [['dome', { orbit: 'hex', count: 6 }], ['slash', { count: 1, vertical: true, size: 1.3 }]] },
  'Deva Spear': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'sky', release: 0.25, impact: 'holy', shake: 0.08,
    fx: [['dome', { size: 1.25, orbit: 'star', count: 6 }], ['volley', { at: 0.05, count: 1, cell: 'needle', size: 1.6, spread: 0, speed: 24 }]] },

  // ---- Gizamon line ----
  'Spiral Saw': { elem: 'water', colors: ['#a0c8ff', '#ffffff'], charge: 'none', release: 0.2, impact: 'hit', fx: [['wheel', { size: 0.42, saw: true, cell: 'spark', dur: 0.32 }]] },
  'Coral Crusher': { elem: 'water', colors: ['#ff6aa0', '#ffe0ea'], charge: 'hands', release: 0.22, impact: 'splash', fx: [['whip', { count: 3, dur: 0.42, width: 0.15, sway: 1.5, leafy: false }]] },
  'Evil Wing': { elem: 'dark', colors: ['#7a2aff', '#ff3a6a'], charge: 'hands', release: 0.22, impact: 'dark',
    fx: [['swarm', { cell: 'bat', count: 12, size: 0.32, speed: 9, wobble: 1.2, color: '#5a1a9a', color2: '#ff3a6a' }]] },

  // ---- Candlemon line ----
  'Wax Flame': { elem: 'fire', colors: ['#ffa02a', '#fff4b0'], charge: 'mouth', release: 0.2, impact: 'blast', fx: [['orb', { size: 0.26, speed: 10, arc: 0.5, trail: 'flame' }]] },
  'Thunder Ball': { elem: 'thunder', colors: ['#ffe94a', '#ffffff'], charge: 'hands', release: 0.2, impact: 'zap', shake: 0.1,
    fx: [['sphere', { size: 0.42, grow: 0.3, from: 'hands', trail: 'zap', cell: 'bolt' }], ['bolt', { at: 0.6, to: 'center', count: 1, area: 2 }]] },
  'Phantom Fire': { elem: 'dark', colors: ['#6a8aff', '#e0a0ff'], charge: 'sky', release: 0.25, impact: 'dark', shake: 0.1,
    fx: [['erupt', { kind: 'dark', to: 'center', area: true, count: 5, height: 2 }], ['swarm', { cell: 'wisp', count: 8, size: 0.35, speed: 6, wobble: 1.4, fan: true, spread: 1 }]] },

  // ---- Veemon line ----
  'Vee Headbutt': { elem: 'beast', colors: ['#4aa0ff', '#ffffff'], charge: 'none', release: 0.15, impact: 'hit', shake: 0.06, fx: [['streak', { width: 0.6, dur: 0.22, trail: 'wind' }]] },
  'Vee Laser': { elem: 'holy', colors: ['#4ab0ff', '#e8f6ff'], charge: 'aura', release: 0.25, impact: 'zap', shake: 0.1,
    fx: [['beam', { width: 0.22, dur: 0.4, from: 'chest', glyph: 'x', charge: 0.1 }]] },
  'Desperado Blaster': { elem: 'metal', colors: ['#ffb04a', '#ffffff'], charge: 'hands', release: 0.22, impact: 'metal', shake: 0.08,
    fx: [['volley', { count: 14, rate: 0.03, size: 0.5, guns: 2, from: 'hands' }]] },

  // ---- Dorumon line ----
  'Metal Cannon': { elem: 'metal', colors: ['#b8c4d8', '#ffffff'], charge: 'mouth', release: 0.22, impact: 'metal', fx: [['orb', { size: 0.22, speed: 14, arc: 0.1, trail: 'smoke', color: '#9aa4b8' }]] },
  'Power Metal': { elem: 'metal', colors: ['#c0ccdd', '#ffffff'], charge: 'mouth', release: 0.25, impact: 'metal', shake: 0.08,
    fx: [['orb', { size: 0.36, speed: 13, arc: 0.1, trail: 'smoke', color: '#a8b4c8', impactSize: 1.3 }]] },
  'Brave Metal': { elem: 'fire', colors: ['#ff6a2a', '#ffe8a0'], charge: 'aura', release: 0.25, impact: 'blast', shake: 0.18, fx: [['streak', { width: 1.1, dur: 0.3, trail: 'flame', impactSize: 1.6 }]] },

  // ---- Gaomon line ----
  'Rolling Upper': { elem: 'beast', colors: ['#5aa8ff', '#ffffff'], charge: 'none', release: 0.15, impact: 'hit',
    fx: [['streak', { width: 0.5, dur: 0.18, trail: 'wind' }], ['slash', { at: 0.06, count: 1, vertical: true, size: 1.2 }]] },
  'Dash Double Claw': { elem: 'beast', colors: ['#4a8aff', '#e0f0ff'], charge: 'none', release: 0.15, impact: 'hit',
    fx: [['streak', { width: 0.6, dur: 0.2, trail: 'wind' }], ['slash', { at: 0.06, count: 2, claw: true, size: 1.3, spread: 0.7 }]] },
  'Winning Knuckle': { elem: 'beast', colors: ['#3a8aff', '#ffffff'], charge: 'hands', release: 0.25, impact: 'blast', shake: 0.18,
    fx: [['orb', { size: 0.34, speed: 11, arc: 0.05, from: 'hands', trail: 'flame', color: '#4a9aff', impactSize: 1.5 }]] },

  // ---- Lalamon line ----
  'Seed Blast': { elem: 'plant', colors: ['#8aff5a', '#fff6a0'], charge: 'mouth', release: 0.2, impact: 'none', fx: [['heal', { cell: 'leaf', color: '#6aff6a', color2: '#d8ff9a' }]] },
  'Sunshine Beam': { elem: 'holy', colors: ['#ffe04a', '#fffbe0'], charge: 'sky', release: 0.25, impact: 'none',
    fx: [['pillar', { size: 0.5, height: 8, impact: 'none' }], ['heal', { at: 0.1, cell: 'star', color: '#ffe04a', color2: '#ffffff', motes: false }]] },
  'Lila Shower': { elem: 'plant', colors: ['#ff9ae0', '#ffffff'], charge: 'sky', release: 0.25, impact: 'none',
    fx: [['rain', { to: 'targets', cell: 'petal', count: 70, size: 0.3, impact: 'none' }], ['heal', { at: 0.2, cell: 'petal', color: '#ffa0e8', color2: '#ffffff', motes: false }]] },

  // ---- Commandramon line ----
  'M16 Assassin': { elem: 'metal', colors: ['#ffd27a', '#ffffff'], charge: 'none', release: 0.15, impact: 'metal', fx: [['volley', { count: 3, rate: 0.05, size: 0.4, guns: 1 }]] },
  'Death Behind': { elem: 'metal', colors: ['#d8e0ec', '#9ab8ff'], charge: 'none', release: 0.15, impact: 'hit',
    fx: [['swarm', { cell: 'blade', count: 2, size: 0.3, speed: 16, wobble: 0.2, arc: 0.1, glow: false, color: '#e8eef8' }]] },
  'Hyper Cannon': { elem: 'metal', colors: ['#ffa02a', '#ffffff'], charge: 'none', release: 0.22, impact: 'blast', shake: 0.08,
    fx: [['orb', { size: 0.26, speed: 16, arc: 0.4, trail: 'smoke', color: '#ffb04a' }]] },

  // ---- Myotismon line ----
  'Crimson Lightning': { elem: 'dark', colors: ['#ff1a3a', '#ffb0c0'], charge: 'hands', release: 0.25, impact: 'dark', shake: 0.1,
    fx: [['whip', { lightning: true, dur: 0.45, width: 0.16, sway: 1.3 }]] },
  'Venom Infuse': { elem: 'dark', colors: ['#9a2aff', '#7aff4a'], charge: 'mouth', release: 0.25, impact: 'poison', shake: 0.15,
    fx: [['breath', { dur: 0.6, width: 0.6, cell: 'wisp', mix: 'smoke', size: 0.55, speed: 8, rate: 260 }], ['vortex', { at: 0.3, dur: 0.7, cell: 'wisp' }]] },
  'Pandemonium Lost': { elem: 'dark', colors: ['#6a1aff', '#ff2a5a'], charge: 'sky', release: 0.3, impact: 'dark', shake: 0.25,
    fx: [['vortex', { dur: 1.1, up: true, cell: 'wisp' }], ['swarm', { cell: 'bat', count: 18, size: 0.38, speed: 8, wobble: 1.6, fan: true, spread: 1.2, color: '#3a0a6a', color2: '#ff2a5a' }]] },

  // ---- Imperialdramon line ----
  'Positron Laser': { elem: 'holy', colors: ['#ff5ad8', '#ffffff'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.3, scale: [1.4, 1.6, 1.8],
    fx: [['beam', { width: 0.3, dur: 0.6, charge: 0.25, from: 'hands', core: '#ffe0f6', overshoot: 3, impactSize: 1.6 }], ['erupt', { at: 0.3, kind: 'flame', to: 'center', area: true, count: 4, height: 1.5, color: '#ff8ae0' }]] },
  'Giga Crusher': { elem: 'fire', colors: ['#ff7a1a', '#ffc04a'], charge: 'sky', release: 0.25, impact: 'blast', shake: 0.3, scale: [1.6, 1.7, 1.8],
    fx: [['sphere', { size: 0.8, grow: 0.4, trail: 'flame', impactSize: 2.2 }]] },
  'Omni Sword': { elem: 'holy', colors: ['#7ad8ff', '#ffffff'], charge: 'sky', release: 0.3, impact: 'hit', shake: 0.3, scale: [1.6, 1.7, 1.8],
    fx: [['wave', { width: 1.6, count: 2, dur: 0.35 }], ['slash', { at: 0.3, count: 2, size: 2.2, to: 'center' }], ['pillar', { at: 0.35, to: 'center', size: 0.8 }]] },

  // ---- Lucemon line ----
  'Grand Cross': { elem: 'holy', colors: ['#ffe04a', '#ffffff'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.15, fx: [['cross', { to: 'center', size: 1.6, orbs: true, dur: 0.9 }]] },
  'Paradise Lost': { elem: 'dark', colors: ['#ffd84a', '#a04aff'], charge: 'aura', release: 0.2, impact: 'hit', shake: 0.25,
    fx: [['streak', { width: 0.9, dur: 0.22, trail: 'holy' }], ['slash', { at: 0.08, count: 5, size: 1.4, gap: 0.05, spread: 1.3, to: 'center' }], ['shock', { at: 0.4, to: 'center', hammer: true }]] },
  'Divine Atonement': { elem: 'dark', colors: ['#2a0a4a', '#ffe04a'], charge: 'sky', release: 0.3, impact: 'dark', shake: 0.35, scale: [1.6, 1.7, 1.9],
    fx: [['sphere', { size: 0.9, grow: 0.45, trail: 'dark', color: '#6a2aaa', core: '#ffe9a0', impactSize: 2.2 }], ['vortex', { at: 0.75, dur: 0.8 }], ['cross', { at: 0.8, to: 'center', size: 1.8 }]] },

  // ---- Beelzemon line ----
  'Bada Boom': { elem: 'fire', colors: ['#ff4a2a', '#ffc06a'], charge: 'hands', release: 0.18, impact: 'blast', fx: [['orb', { size: 0.2, speed: 14, arc: 0.3, from: 'hands', trail: 'flame' }]] },
  'Double Impact': { elem: 'dark', colors: ['#ff3a3a', '#ffe0a0'], charge: 'hands', release: 0.18, impact: 'metal', fx: [['volley', { count: 2, rate: 0.06, size: 0.6, guns: 2, from: 'hands' }]] },
  'Corona Destroyer': { elem: 'dark', colors: ['#ff2a4a', '#ffd0a0'], charge: 'hands', release: 0.2, impact: 'metal', shake: 0.05,
    fx: [['volley', { count: 6, rate: 0.025, size: 0.6, guns: 1, from: 'hands', speed: 32 }]] },

  // ---- Machinedramon line ----
  'Dark Side Attack': { elem: 'metal', colors: ['#ff6a2a', '#ffd0a0'], charge: 'hands', release: 0.25, impact: 'blast', shake: 0.2,
    fx: [['missiles', { count: 6, size: 0.14, from: 'hands', organic: true, curve: 1.6, arc: 1.6, dur: 0.6, gap: 0.05 }], ['shock', { at: 0.6, debris: true }]] },
  'Giga Cannon': { elem: 'metal', colors: ['#ff8a2a', '#fff4d0'], charge: 'back', release: 0.3, impact: 'blast', shake: 0.28,
    fx: [['beam', { twin: true, from: 'back', width: 0.3, dur: 0.5, charge: 0.15, core: '#ffffff' }], ['shock', { at: 0.2, size: 2.2 }]] },
  'Hyper Mugen Cannon': { elem: 'metal', colors: ['#ff3a2a', '#ffe8c0'], charge: 'back', release: 0.3, impact: 'blast', shake: 0.35, scale: [1.6, 1.7, 1.9],
    fx: [['beam', { twin: true, from: 'back', width: 0.38, dur: 0.6, charge: 0.2, core: '#ffffff', overshoot: 2 }], ['missiles', { at: 0.1, count: 4, size: 0.12, from: 'chest', curve: 1.5 }], ['shock', { at: 0.25, size: 2.6 }]] },

  // ---- Numemon line ----
  'Nume-Sludge': { elem: 'plant', colors: ['#8a6a2a', '#c8e05a'], charge: 'hands', release: 0.2, impact: 'poison',
    fx: [['orb', { head: 'drop', size: 0.26, speed: 9, arc: 0.7, from: 'hands', trail: 'smoke', headColor: '#7a5a20' }]] },
  'Kingly Sludge': { elem: 'dark', colors: ['#3a2a3a', '#ffd84a'], charge: 'hands', release: 0.22, impact: 'poison', shake: 0.06,
    fx: [['swarm', { cell: 'drop', count: 7, size: 0.32, speed: 8, arc: 0.6, fan: true, spread: 0.5, color: '#3a2a20', color2: '#ffd84a', tint: true }]] },
  'Platinum Sludge': { elem: 'metal', colors: ['#d8e4f4', '#ffffff'], charge: 'hands', release: 0.22, impact: 'poison', shake: 0.08,
    fx: [['swarm', { cell: 'drop', count: 10, size: 0.36, speed: 9, arc: 0.7, fan: true, spread: 0.6, glow: true }], ['erupt', { at: 0.35, kind: 'metal', count: 3, height: 0.8 }]] },

  // ---- Otamamon line ----
  'Lullaby Bubble': { elem: 'water', colors: ['#7ad0ff', '#ffffff'], charge: 'mouth', release: 0.2, impact: 'splash',
    fx: [['swarm', { cell: 'bubble', count: 6, size: 0.3, speed: 6, wobble: 1.0 }], ['swarm', { at: 0.1, cell: 'note', count: 4, size: 0.26, speed: 6, wobble: 1.2 }]] },
  'Symphony Crusher': { elem: 'water', colors: ['#ff6a4a', '#ffe0a0'], charge: 'mouth', release: 0.25, impact: 'splash', shake: 0.08,
    fx: [['rings', { count: 4, size: 0.5, gap: 0.08 }], ['swarm', { at: 0.05, cell: 'note', count: 10, size: 0.3, speed: 8, fan: true, spread: 0.6 }]] },
  'Musical Fist': { elem: 'water', colors: ['#ff4a4a', '#ffd84a'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.15,
    fx: [['rings', { count: 5, size: 0.8, gap: 0.07, from: 'chest' }], ['shock', { at: 0.3, to: 'center', cell: 'note', debris: false, size: 1.6 }]] },

  // ---- Gotsumon line ----
  'Rock Fist': { elem: 'metal', colors: ['#a89a88', '#e8e0d0'], charge: 'aura', release: 0.2, impact: 'metal',
    fx: [['dome', { orbit: 'shard', count: 5 }], ['erupt', { at: 0.1, kind: 'rock', to: 'self', count: 3, height: 0.8 }]] },
  'Meteor Squall': { elem: 'holy', colors: ['#ffd84a', '#fff6c0'], charge: 'sky', release: 0.22, impact: 'holy',
    fx: [['dome', { orbit: 'star', count: 6 }], ['meteor', { at: 0.1, size: 0.2, count: 3, height: 6, trail: 'holy' }]] },
  'Cosmic Ray': { elem: 'holy', colors: ['#ffe04a', '#ffffff'], charge: 'sky', release: 0.25, impact: 'holy', shake: 0.08,
    fx: [['dome', { size: 1.3, orbit: 'star', count: 8 }], ['pillar', { to: 'self', size: 1.2, height: 6 }]] },

  // ---- Falcomon line ----
  'Ninja Shuriken': { elem: 'wind', colors: ['#c8d8ff', '#ffffff'], charge: 'none', release: 0.1, impact: 'hit',
    fx: [['dash', { ghosts: 4, slashes: 0, dur: 0.16 }], ['wheel', { at: 0.1, size: 0.3, cell: 'blade', dur: 0.3 }]] },
  'Kunai Wing': { elem: 'wind', colors: ['#8aa0d8', '#ffffff'], charge: 'none', release: 0.12, impact: 'wind', shake: 0.06,
    fx: [['dash', { ghosts: 5, slashes: 1, dur: 0.18 }], ['swarm', { at: 0.12, cell: 'feather', count: 8, size: 0.3, speed: 14, fan: true, spread: 0.4 }]] },
  'Ether Burst': { elem: 'dark', colors: ['#3a2a6a', '#ffd84a'], charge: 'none', release: 0.12, impact: 'dark', shake: 0.15,
    fx: [['dash', { ghosts: 6, slashes: 2, dur: 0.2, size: 1.4 }], ['swarm', { at: 0.15, cell: 'feather', count: 12, size: 0.34, speed: 10, spread: 0.9, color: '#2a2040' }], ['sphere', { at: 0.2, size: 0.6, grow: 0.2, from: 'above' }]] },

  // ---- Lunamon line ----
  'Lunar Claw': { elem: 'water', colors: ['#8ab8ff', '#ffffff'], charge: 'hands', release: 0.2, impact: 'splash',
    fx: [['beam', { width: 0.14, dur: 0.3, from: 'hands', core: '#ffffff' }], ['slash', { at: 0.1, count: 2, claw: true, size: 1.1 }]] },
  'Tear Arrow': { elem: 'water', colors: ['#5ab0ff', '#e8f8ff'], charge: 'hands', release: 0.22, impact: 'splash', shake: 0.06,
    fx: [['beam', { width: 0.12, dur: 0.3, from: 'hands', core: '#ffffff', overshoot: 2 }], ['arrows', { count: 3, size: 0.4, cell: 'drop', gap: 0.05 }]] },
  'Lunatic Dance': { elem: 'dark', colors: ['#6a5aff', '#e0d8ff'], charge: 'sky', release: 0.28, impact: 'dark', shake: 0.12,
    fx: [['glyph', { glyph: 'circle', size: 1.6, dur: 0.7 }], ['beam', { at: 0.1, width: 0.2, dur: 0.4, charge: 0.1, core: '#ffffff', overshoot: 3 }], ['arrows', { at: 0.1, count: 5, size: 0.45, cell: 'shard', gap: 0.04 }]] },

  // ---- Armadillomon line ----
  'Diamond Shell': { elem: 'metal', colors: ['#d8a85a', '#fff6d0'], charge: 'aura', release: 0.2, impact: 'metal',
    fx: [['dome', { orbit: 'shard', count: 5 }], ['wheel', { at: 0.1, size: 0.4, cell: 'shard', dur: 0.4 }]] },
  'Tail Hammer': { elem: 'metal', colors: ['#b88a4a', '#fff0d0'], charge: 'ground', release: 0.25, impact: 'metal', shake: 0.12,
    fx: [['dome', { size: 1.1 }], ['shock', { at: 0.1, hammer: true, cell: 'shard' }]] },
  'Justice Beam': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'aura', release: 0.28, impact: 'holy', shake: 0.12,
    fx: [['dome', { size: 1.3, orbit: 'star', count: 6 }], ['beam', { at: 0.05, width: 0.26, dur: 0.45, from: 'chest', core: '#ffffff' }]] },

  // ---- Kudamon line ----
  'Holy Shot': { elem: 'holy', colors: ['#fff0a0', '#ffffff'], charge: 'aura', release: 0.2, impact: 'none',
    fx: [['glyph', { glyph: 'circle', size: 1.3, dur: 0.5, to: 'self' }], ['dome', { orbit: 'star', count: 4 }]] },
  'Kamaitachi': { elem: 'wind', colors: ['#b8fff0', '#ffffff'], charge: 'aura', release: 0.22, impact: 'none',
    fx: [['tornado', { to: 'self', cell: 'streak', height: 2, size: 1.0 }], ['dome', { at: 0.15, orbit: 'blade', count: 6 }]] },
  'Holy Breath': { elem: 'holy', colors: ['#ffe08a', '#ffffff'], charge: 'sky', release: 0.28, impact: 'none', shake: 0.05,
    fx: [['pillar', { to: 'self', size: 1.3, height: 6 }], ['glyph', { glyph: 'cross', size: 2.0, dur: 0.8, place: 'ground', to: 'self' }], ['dome', { at: 0.2, size: 1.3, orbit: 'cross', count: 6 }]] },

  // ---- FanBeemon line ----
  'Bee Shot': { elem: 'thunder', colors: ['#ffd23f', '#3a3a2a'], charge: 'hands', release: 0.18, impact: 'zap',
    fx: [['orb', { head: 'needle', size: 0.2, speed: 16, arc: 0.1, trail: 'zap' }]] },
  'Turbo Stinger': { elem: 'thunder', colors: ['#ffd23f', '#ffffff'], charge: 'hands', release: 0.2, impact: 'zap',
    fx: [['missiles', { count: 1, size: 0.14, from: 'chest', curve: 0.6, arc: 0.5, dur: 0.4 }]] },
  'Royal Smasher': { elem: 'thunder', colors: ['#ffb02a', '#ffffff'], charge: 'back', release: 0.25, impact: 'blast', shake: 0.1,
    fx: [['missiles', { count: 2, size: 0.16, from: 'back', curve: 1.0, arc: 1.0, dur: 0.5, impactSize: 1.2 }]] },

  // ---- Mamemon line ----
  'Smiley Bomb': { elem: 'fire', colors: ['#ff8a2a', '#ffe08a'], charge: 'hands', release: 0.22, impact: 'blast', shake: 0.1,
    fx: [['orb', { head: 'heart', size: 0.3, speed: 10, arc: 0.8, from: 'hands', trail: 'smoke' }]] },
  'Energy Bomb': { elem: 'metal', colors: ['#ffb04a', '#d8e4f4'], charge: 'hands', release: 0.22, impact: 'blast', shake: 0.12,
    fx: [['sphere', { size: 0.45, grow: 0.25, from: 'hands', trail: 'spark', cell: 'spark', impactSize: 1.5 }]] },
  'Smiley Warhead': { elem: 'fire', colors: ['#ff5a1a', '#fff0a0'], charge: 'sky', release: 0.28, impact: 'blast', shake: 0.25,
    fx: [['meteor', { size: 0.45, count: 1, height: 8, trail: 'smoke' }], ['shock', { at: 0.45, to: 'center', size: 2.2, cell: 'ember' }]] },

  // ---- Dracmon line ----
  'Zombie Claw': { elem: 'dark', colors: ['#6a2a6a', '#ff5a8a'], charge: 'none', release: 0.15, impact: 'dark',
    fx: [['slash', { count: 2, claw: true, size: 1.2, spread: 0.5 }]] },
  'Snipe Steal': { elem: 'dark', colors: ['#3a1a4a', '#ff3a5a'], charge: 'none', release: 0.15, impact: 'dark', shake: 0.06,
    fx: [['dash', { ghosts: 4, slashes: 1, dur: 0.18 }], ['swarm', { at: 0.1, cell: 'bat', count: 6, size: 0.3, speed: 10, spread: 0.6 }]] },
  'Fuerza Mortal': { elem: 'dark', colors: ['#c01a3a', '#ffd0d8'], charge: 'none', release: 0.15, impact: 'dark', shake: 0.15,
    fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.2 }], ['slash', { at: 0.12, count: 4, size: 1.5, spread: 0.8, gap: 0.04 }], ['swarm', { at: 0.2, cell: 'petal', count: 10, size: 0.28, speed: 6, spread: 1.0, color: '#ff2a4a' }]] },

  // ---- Agumon (Savers) line ----
  'Mega Burst': { elem: 'fire', colors: ['#ff6a1a', '#ffe08a'], charge: 'mouth', release: 0.28, impact: 'blast', shake: 0.12,
    fx: [['breath', { dur: 0.4, width: 0.4, cell: 'flame', size: 0.42, speed: 11, rate: 300 }]] },
  'Trident Revolver': { elem: 'metal', colors: ['#ff8a2a', '#e8f0ff'], charge: 'hands', release: 0.25, impact: 'blast', shake: 0.14,
    fx: [['volley', { count: 3, rate: 0.1, size: 0.7, guns: 1, cell: 'streak', from: 'hands' }]] },
  'Glorious Burst': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'back', release: 0.3, impact: 'holy', shake: 0.22, scale: [1.3, 1.5, 1.7],
    fx: [['sphere', { size: 0.9, grow: 0.3, from: 'above', trail: 'holy', cell: 'flame', core: '#ffffff', impactSize: 2 }], ['pillar', { at: 0.55, size: 1.2 }]] },

  // ---- Dracomon line ----
  'Baby Breath': { elem: 'fire', colors: ['#5ab0ff', '#e0f0ff'], charge: 'mouth', release: 0.22, impact: 'blast',
    fx: [['breath', { dur: 0.32, width: 0.3, cell: 'flame', size: 0.32, speed: 9 }]] },
  'Green Flare Breath': { elem: 'fire', colors: ['#5aff6a', '#e0ffe0'], charge: 'mouth', release: 0.28, impact: 'blast', shake: 0.1,
    fx: [['breath', { dur: 0.5, width: 0.5, cell: 'flame', size: 0.45, speed: 10, rate: 320 }]] },
  'Sonic Blast': { elem: 'wind', colors: ['#5ab0ff', '#ffffff'], charge: 'mouth', release: 0.28, impact: 'wind', shake: 0.15,
    fx: [['rings', { count: 4, size: 0.9, gap: 0.06 }], ['shock', { at: 0.3, to: 'center', size: 1.8, debris: false }]] },

  // ---- Woodmon line ----
  'Branch Drill': { elem: 'plant', colors: ['#8a5a2a', '#c8e05a'], charge: 'hands', release: 0.2, impact: 'leaf',
    fx: [['whip', { dur: 0.35, width: 0.12, count: 1, sway: 0.6, leafy: true }]] },
  'Pit Pelter': { elem: 'plant', colors: ['#ff3a5a', '#ffe0e8'], charge: 'mouth', release: 0.2, impact: 'poison',
    fx: [['volley', { count: 8, rate: 0.03, cell: 'seed', size: 0.5, spread: 0.3, speed: 16 }]] },
  'Puppet Pummel': { elem: 'dark', colors: ['#8a5a3a', '#ffd84a'], charge: 'hands', release: 0.25, impact: 'dark', shake: 0.1,
    fx: [['whip', { dur: 0.35, width: 0.06, count: 3, sway: 0.4, leafy: false }], ['orb', { at: 0.1, size: 0.3, speed: 10, arc: 0.6, trail: 'dark', head: 'star' }]] },

  // ---- Monodramon line ----
  'Beat Knuckle': { elem: 'beast', colors: ['#9a6aff', '#ffffff'], charge: 'none', release: 0.15, impact: 'hit',
    fx: [['slash', { count: 1, size: 1.1 }]] },
  'Strike Fang': { elem: 'fire', colors: ['#ff4a2a', '#ffe0a0'], charge: 'none', release: 0.15, impact: 'blast', shake: 0.08,
    fx: [['streak', { width: 0.5, dur: 0.2, trail: 'flame' }], ['slash', { at: 0.1, count: 2, claw: true, size: 1.3 }]] },
  'Desolation Claw': { elem: 'dark', colors: ['#6a1aff', '#ff3a6a'], charge: 'none', release: 0.15, impact: 'dark', shake: 0.18,
    fx: [['dash', { ghosts: 6, slashes: 0, dur: 0.2, claw: true }], ['slash', { at: 0.12, count: 4, claw: true, size: 1.6, spread: 0.8, gap: 0.03 }]] },

  // ---- Jazamon line ----
  'Jaza Claw': { elem: 'beast', colors: ['#ffb02a', '#fff0c0'], charge: 'none', release: 0.12, impact: 'hit',
    fx: [['dash', { ghosts: 4, slashes: 1, dur: 0.16, claw: true }]] },
  'Jazard Fang': { elem: 'beast', colors: ['#ff8a2a', '#ffffff'], charge: 'none', release: 0.12, impact: 'hit', shake: 0.08,
    fx: [['dash', { ghosts: 5, slashes: 2, dur: 0.18, claw: true, size: 1.3 }]] },
  'Jazarich Strike': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'none', release: 0.12, impact: 'holy', shake: 0.14,
    fx: [['dash', { ghosts: 6, slashes: 2, dur: 0.2, claw: true, size: 1.5 }], ['pillar', { at: 0.2, size: 0.9, height: 4 }]] },

  // ---- Zubamon line ----
  'Zuba Saber': { elem: 'metal', colors: ['#ffb04a', '#ffffff'], charge: 'hands', release: 0.2, impact: 'metal',
    fx: [['wave', { width: 0.6, count: 1, dur: 0.35 }]] },
  'Eager Slash': { elem: 'wind', colors: ['#4ab0ff', '#ffffff'], charge: 'hands', release: 0.22, impact: 'wind', shake: 0.08,
    fx: [['wave', { width: 0.8, count: 2, dur: 0.35 }]] },
  'Durandal': { elem: 'holy', colors: ['#ffe08a', '#ffffff'], charge: 'sky', release: 0.28, impact: 'holy', shake: 0.18,
    fx: [['beam', { width: 0.3, dur: 0.45, charge: 0.12, core: '#ffffff', overshoot: 3 }], ['wave', { at: 0.05, width: 1.1, count: 1, dur: 0.4 }]] },

  // ---- Agunimon line ----
  'Pyro Tornado': { elem: 'fire', colors: ['#ff5a1a', '#ffe08a'], charge: 'ground', release: 0.22, impact: 'blast', shake: 0.12,
    fx: [['tornado', { to: 'self', cell: 'flame', height: 2.2, size: 1.0 }], ['shock', { at: 0.15, cell: 'ember' }]] },
  'Wildfire Tsunami': { elem: 'fire', colors: ['#ff3a1a', '#ffd84a'], charge: 'ground', release: 0.28, impact: 'blast', shake: 0.2,
    fx: [['shock', { size: 2.0, cell: 'flame' }], ['erupt', { at: 0.1, kind: 'flame', area: true, count: 5, height: 1.6 }]] },
  'Pyro Dragons': { elem: 'fire', colors: ['#ff4a1a', '#fff0a0'], charge: 'back', release: 0.3, impact: 'blast', shake: 0.28, scale: [1.3, 1.5, 1.7],
    fx: [['missiles', { count: 4, size: 0.2, from: 'back', organic: true, curve: 1.2, arc: 1.2, dur: 0.6, smoke: false }], ['shock', { at: 0.3, size: 2.4, cell: 'ember' }]] },

  // ---- Lobomon line ----
  'Lobo Kendo': { elem: 'holy', colors: ['#7ad8ff', '#ffffff'], charge: 'hands', release: 0.2, impact: 'holy',
    fx: [['wave', { width: 0.6, count: 1, dur: 0.3 }], ['slash', { at: 0.05, count: 1, size: 1.2 }]] },
  'Solar Laser': { elem: 'holy', colors: ['#ffe94a', '#ffffff'], charge: 'back', release: 0.25, impact: 'holy', shake: 0.1,
    fx: [['beam', { width: 0.22, dur: 0.4, from: 'back', twin: true, core: '#ffffff', overshoot: 3 }]] },
  'Magna Missile': { elem: 'metal', colors: ['#5ab0ff', '#e8f0ff'], charge: 'back', release: 0.28, impact: 'blast', shake: 0.2,
    fx: [['beam', { width: 0.18, dur: 0.35, core: '#ffffff', overshoot: 3 }], ['missiles', { at: 0.05, count: 6, size: 0.12, from: 'back', curve: 1.2, arc: 1.0, dur: 0.5, gap: 0.04 }]] },

  // ---- Hackmon line ----
  'Fifth Crow': { elem: 'fire', colors: ['#ff4a3a', '#ffffff'], charge: 'none', release: 0.12, impact: 'hit',
    fx: [['dash', { ghosts: 4, slashes: 1, dur: 0.16, claw: true }]] },
  'Cyclone Hunter': { elem: 'wind', colors: ['#ff6a4a', '#ffffff'], charge: 'none', release: 0.12, impact: 'wind', shake: 0.1,
    fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.18 }], ['tornado', { at: 0.12, height: 2, size: 0.8, cell: 'streak' }]] },
  'Schwertgeist': { elem: 'holy', colors: ['#ff3a3a', '#ffffff'], charge: 'none', release: 0.12, impact: 'holy', shake: 0.2, scale: [1.2, 1.4, 1.6],
    fx: [['dash', { ghosts: 6, slashes: 2, dur: 0.2, size: 1.5 }], ['wave', { at: 0.1, width: 1.0, count: 3, dur: 0.35 }], ['slash', { at: 0.2, count: 4, size: 1.6, spread: 0.9, gap: 0.03 }]] },

  // ---- BlackGatomon line ----
  "Cat's Eye": { elem: 'dark', colors: ['#c05aff', '#ffd84a'], charge: 'aura', release: 0.2, impact: 'dark',
    fx: [['beam', { width: 0.12, dur: 0.3, core: '#ffd84a' }]] },
  'Darkness Wave': { elem: 'dark', colors: ['#8a2aff', '#ff6ab8'], charge: 'hands', release: 0.25, impact: 'dark', shake: 0.08,
    fx: [['swarm', { cell: 'bat', count: 10, size: 0.32, speed: 9, fan: true, spread: 0.7 }]] },
  'Phantom Pain': { elem: 'dark', colors: ['#d01a6a', '#ffd0e8'], charge: 'hands', release: 0.3, impact: 'dark', shake: 0.16,
    fx: [['glyph', { glyph: 'circle', size: 1.6, dur: 0.7 }], ['portal', { at: 0.1, size: 1.2, dur: 0.9 }], ['slash', { at: 0.3, count: 3, claw: true, size: 1.4 }]] },

  // ---- Meicoomon line ----
  'Meicoo Crack': { elem: 'dark', colors: ['#ffd84a', '#8a5aff'], charge: 'hands', release: 0.2, impact: 'dark',
    fx: [['beam', { width: 0.12, dur: 0.3, core: '#ffffff' }], ['slash', { at: 0.1, count: 2, claw: true, size: 1.0 }]] },
  'Berserk Thinking': { elem: 'dark', colors: ['#c01a3a', '#ffd84a'], charge: 'aura', release: 0.22, impact: 'dark', shake: 0.12,
    fx: [['streak', { width: 0.5, dur: 0.2, trail: 'dark' }], ['slash', { at: 0.1, count: 3, claw: true, size: 1.4, spread: 0.7 }]] },
  'Darkness Zone': { elem: 'dark', colors: ['#2a0a3a', '#ff2a5a'], charge: 'sky', release: 0.3, impact: 'dark', shake: 0.2,
    fx: [['vortex', { to: 'center', size: 1.6, dur: 0.8 }], ['erupt', { at: 0.2, kind: 'dark', height: 2.2 }]] },

  // ---- Vorvomon line ----
  'Vorvo Flame': { elem: 'fire', colors: ['#ff6a1a', '#ffd84a'], charge: 'ground', release: 0.2, impact: 'blast', shake: 0.08,
    fx: [['shock', { cell: 'flame' }]] },
  'Lava Burst': { elem: 'fire', colors: ['#ff3a0a', '#ffb04a'], charge: 'ground', release: 0.25, impact: 'blast', shake: 0.14,
    fx: [['shock', { size: 1.6, cell: 'ember' }], ['erupt', { at: 0.1, kind: 'flame', area: true, count: 3, height: 1.2 }]] },
  'Volcanic Howl': { elem: 'fire', colors: ['#ff2a0a', '#fff0a0'], charge: 'ground', release: 0.3, impact: 'blast', shake: 0.25, scale: [1.3, 1.5, 1.7],
    fx: [['rings', { count: 3, size: 1.0, gap: 0.08, from: 'mouth' }], ['erupt', { at: 0.1, kind: 'rock', area: true, count: 4, height: 1.4 }], ['erupt', { at: 0.2, kind: 'flame', area: true, count: 5, height: 2 }]] },

  // ---- Ryudamon line ----
  'Battojutsu': { elem: 'metal', colors: ['#d8e4f4', '#ffffff'], charge: 'none', release: 0.12, impact: 'metal',
    fx: [['streak', { width: 0.4, dur: 0.15, trail: 'wind' }], ['slash', { at: 0.08, count: 1, size: 1.3 }]] },
  'Tetsuzan': { elem: 'metal', colors: ['#a0b8d8', '#ffffff'], charge: 'none', release: 0.12, impact: 'metal', shake: 0.1,
    fx: [['streak', { width: 0.5, dur: 0.18, trail: 'wind' }], ['slash', { at: 0.08, count: 2, size: 1.4, spread: 0.6 }]] },
  'Hisho Ryuuha': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'none', release: 0.15, impact: 'holy', shake: 0.18,
    fx: [['streak', { width: 0.7, dur: 0.2, trail: 'holy' }], ['slash', { at: 0.1, count: 3, size: 1.6, spread: 0.8, vertical: true }], ['pillar', { at: 0.2, size: 0.9, height: 4 }]] },

  // ---- Veedramon line ----
  'V-Breath Arrow': { elem: 'fire', colors: ['#4a8aff', '#ffffff'], charge: 'mouth', release: 0.22, impact: 'blast',
    fx: [['beam', { width: 0.2, dur: 0.35, glyph: 'x', core: '#ffffff', overshoot: 2 }]] },
  'Magnum Crusher': { elem: 'wind', colors: ['#5ab0ff', '#ffffff'], charge: 'back', release: 0.25, impact: 'wind', shake: 0.15,
    fx: [['streak', { width: 0.8, dur: 0.25, trail: 'wind' }], ['beam', { at: 0.05, width: 0.24, dur: 0.35, core: '#ffffff', overshoot: 3 }]] },
  'Shining V Force': { elem: 'holy', colors: ['#3a7aff', '#ffffff'], charge: 'back', release: 0.3, impact: 'holy', shake: 0.28, scale: [1.3, 1.5, 1.8],
    fx: [['glyph', { glyph: 'x', size: 1.8, dur: 0.6 }], ['beam', { at: 0.05, width: 0.4, dur: 0.55, charge: 0.15, glyph: 'x', core: '#ffffff', overshoot: 3 }]] },

  // ---- Omnimon line ----
  'Great Tornado': { elem: 'fire', colors: ['#ffb02a', '#fff0c0'], charge: 'none', release: 0.15, impact: 'blast', shake: 0.18, scale: [1.5, 1.6, 1.8],
    fx: [['streak', { width: 0.8, dur: 0.25, trail: 'flame' }], ['tornado', { at: 0.15, height: 3, size: 1.2, cell: 'flame' }]] },
  'Transcendent Sword': { elem: 'holy', colors: ['#ffffff', '#ffd84a'], charge: 'sky', release: 0.2, impact: 'holy', shake: 0.25, scale: [1.6, 1.7, 1.9],
    fx: [['streak', { width: 0.7, dur: 0.22, trail: 'holy' }], ['slash', { at: 0.1, count: 2, size: 2.0, vertical: true }], ['pillar', { at: 0.2, size: 1.2, height: 6 }]] },
  'Supreme Cannon': { elem: 'ice', colors: ['#5ab0ff', '#ffffff'], charge: 'hands', release: 0.25, impact: 'frost', shake: 0.3, scale: [1.6, 1.7, 1.9],
    fx: [['sphere', { size: 0.9, grow: 0.3, from: 'hands', trail: 'ice', cell: 'snow', core: '#ffffff', impactSize: 2.2 }], ['erupt', { at: 0.5, kind: 'ice', height: 2 }]] },

  // ---- Alphamon line ----
  'Digitalize of Soul': { elem: 'holy', colors: ['#3a3a6a', '#ffd84a'], charge: 'sky', release: 0.28, impact: 'holy', shake: 0.25, scale: [1.6, 1.7, 1.9],
    fx: [['glyph', { glyph: 'circle', size: 2.4, dur: 0.9, place: 'ground', to: 'self' }], ['shock', { at: 0.15, size: 2.6, cell: 'hex' }]] },
  'Alpha Inforce': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'ground', release: 0.25, impact: 'holy', shake: 0.28, scale: [1.6, 1.7, 1.9],
    fx: [['glyph', { glyph: 'tri', size: 2.2, dur: 0.6, place: 'ground', to: 'self' }], ['shock', { at: 0.1, size: 2.8, cell: 'star' }], ['erupt', { at: 0.2, kind: 'metal', area: true, count: 5, height: 1.6 }]] },
  'Ouryuken': { elem: 'holy', colors: ['#2a5a3a', '#ffe08a'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.35, scale: [1.7, 1.8, 2.0],
    fx: [['pillar', { to: 'self', size: 1.6, height: 8 }], ['shock', { at: 0.15, hammer: true, size: 3.0, cell: 'blade' }], ['erupt', { at: 0.25, kind: 'metal', area: true, count: 6, height: 2.2 }]] },

  // ---- Daemon line ----
  'Evil Inferno': { elem: 'dark', colors: ['#c01a1a', '#ff8a2a'], charge: 'hands', release: 0.28, impact: 'blast', shake: 0.22, scale: [1.6, 1.7, 1.9],
    fx: [['glyph', { glyph: 'circle', size: 1.8, dur: 0.6 }], ['erupt', { at: 0.15, kind: 'flame', to: 'center', area: true, count: 5, height: 2 }]] },
  'Flame Inferno': { elem: 'fire', colors: ['#8a0a2a', '#ff6a1a'], charge: 'sky', release: 0.3, impact: 'blast', shake: 0.28, scale: [1.6, 1.7, 1.9],
    fx: [['meteor', { size: 0.5, count: 3, height: 9, trail: 'flame' }], ['erupt', { at: 0.4, kind: 'flame', to: 'center', area: true, count: 6, height: 2.2 }]] },
  'Chaos Flare': { elem: 'dark', colors: ['#3a0a2a', '#ff3a1a'], charge: 'back', release: 0.3, impact: 'dark', shake: 0.35, scale: [1.7, 1.8, 2.0],
    fx: [['sphere', { size: 1.1, grow: 0.35, from: 'above', trail: 'dark', cell: 'ember', impactSize: 2.4 }], ['erupt', { at: 0.55, kind: 'dark', to: 'center', area: true, count: 6, height: 2.6 }]] },

  // ---- Okuwamon line ----
  'Double Scissor Claw': { elem: 'beast', colors: ['#3a5aff', '#ffffff'], charge: 'none', release: 0.15, impact: 'hit', shake: 0.12,
    fx: [['streak', { width: 0.6, dur: 0.2, trail: 'spark' }], ['slash', { at: 0.08, count: 4, size: 1.5, spread: 1.0, gap: 0.03 }]] },
  'Dimension Scissor': { elem: 'dark', colors: ['#8a1a2a', '#ffd84a'], charge: 'none', release: 0.15, impact: 'dark', shake: 0.25, scale: [1.4, 1.6, 1.8],
    fx: [['portal', { size: 1.2, dur: 0.6 }], ['slash', { at: 0.15, count: 2, size: 2.2, spread: 1.2, vertical: true }]] },

  // ---- Phoenixmon line ----
  'Phoenix Wing': { elem: 'fire', colors: ['#ff6a1a', '#fff0a0'], charge: 'back', release: 0.28, impact: 'blast', shake: 0.2, scale: [1.5, 1.7, 1.9],
    fx: [['orb', { size: 0.5, speed: 10, arc: 0.4, trail: 'flame', wings: true, impactSize: 1.8 }], ['swarm', { at: 0.05, cell: 'feather', count: 10, size: 0.35, speed: 9, fan: true, spread: 0.7 }]] },
  'Starlight Explosion': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.3, scale: [1.6, 1.8, 2.0],
    fx: [['glyph', { glyph: 'circle', size: 2.0, dur: 0.7 }], ['meteor', { at: 0.1, size: 0.3, count: 6, height: 9, trail: 'holy' }], ['swarm', { at: 0.3, cell: 'feather', count: 14, size: 0.4, speed: 8, spread: 1.0, color: '#ff8a2a' }]] },

  // ---- wild Digimon ----
  'Goblin Strike': { elem: 'fire', colors: ['#ff6a2a', '#ffd06a'], charge: 'hands', release: 0.2, impact: 'blast', fx: [['orb', { size: 0.2, speed: 11, arc: 0.6, from: 'hands', trail: 'flame' }]] },
  'Scissor Claw': { elem: 'beast', colors: ['#ff4a4a', '#ffffff'], charge: 'none', release: 0.18, impact: 'hit', fx: [['slash', { count: 2, size: 1.3, spread: 1.1, gap: 0.02 }]] },
  'Pummel Whack': { elem: 'beast', colors: ['#d8c8a8', '#ffffff'], charge: 'sky', release: 0.25, impact: 'hit', shake: 0.15, fx: [['shock', { hammer: true }]] },
  'Blaze Blast': { elem: 'fire', charge: 'mouth', release: 0.25, impact: 'blast', shake: 0.08, fx: [['breath', { dur: 0.5, width: 0.5, cell: 'flame', size: 0.45, speed: 9, rate: 300 }]] },
  'Dark Network': { elem: 'dark', colors: ['#8a3aff', '#ffd84a'], charge: 'aura', release: 0.25, impact: 'dark', shake: 0.08,
    fx: [['swarm', { cell: 'note', count: 12, size: 0.35, speed: 7, wobble: 1.2, fan: true, spread: 0.8 }], ['vortex', { at: 0.35, dur: 0.6 }]] },
  'Ground Zero': { elem: 'fire', colors: ['#ff5a1a', '#ffe0a0'], charge: 'back', release: 0.3, impact: 'blast', shake: 0.3,
    fx: [['missiles', { count: 1, size: 0.28, from: 'back', organic: true, curve: 0.3, arc: 2.6, dur: 0.7, impactSize: 2.2 }]] },
  // ---- Frigimon, Bakemon, Monzaemon (wild helpers) ----
  'Sub Zero Ice Punch': { elem: 'ice', colors: ['#9ad8ff', '#ffffff'], charge: 'hands', release: 0.22, impact: 'frost', shake: 0.08,
    fx: [['orb', { head: 'snow', size: 0.32, speed: 11, arc: 0.2, from: 'hands', trail: 'ice' }]] },
  'Dark Claw': { elem: 'dark', colors: ['#5a3a8a', '#d8c8ff'], charge: 'none', release: 0.12, impact: 'dark',
    fx: [['dash', { ghosts: 4, slashes: 1, dur: 0.18, claw: true }]] },
  'Hearts Attack': { elem: 'holy', colors: ['#ff6ab8', '#ffe0f0'], charge: 'hands', release: 0.22, impact: 'holy',
    fx: [['swarm', { cell: 'heart', count: 8, size: 0.34, speed: 7, wobble: 0.8, fan: true, spread: 0.5 }]] },

  // ---- Toilet Town round ----
  'Poop Toss': { elem: 'plant', colors: ['#8a5a2a', '#e8c890'], charge: 'hands', release: 0.2, impact: 'poison',
    fx: [['orb', { head: 'drop', size: 0.3, speed: 9, arc: 0.9, from: 'hands', trail: 'smoke', headColor: '#7a4a1a' }]] },
  'Party Time': { elem: 'beast', colors: ['#ffd84a', '#ff6ab8'], charge: 'hands', release: 0.22, impact: 'hit', shake: 0.08,
    fx: [['swarm', { cell: 'note', count: 10, size: 0.32, speed: 8, wobble: 1.0, fan: true, spread: 0.6 }], ['orb', { at: 0.1, head: 'star', size: 0.3, speed: 11, arc: 0.3, from: 'hands' }]] },
  // ---- Celestial round ----
  'Seven Heavens': { elem: 'holy', colors: ['#ffe08a', '#ffffff'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.3, scale: [1.6, 1.7, 1.8],
    fx: [['glyph', { glyph: 'circle', size: 2.2, dur: 0.8 }], ['orb', { at: 0.1, head: 'star', size: 0.35, speed: 10, count: 7, gap: 0.05, arc: 0.6, trail: 'holy' }]] },
  "Eden's Javelin": { elem: 'holy', colors: ['#9ad8ff', '#ffffff'], charge: 'hands', release: 0.28, impact: 'holy', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['beam', { width: 0.32, dur: 0.5, charge: 0.12, core: '#ffffff', overshoot: 3 }], ['cross', { at: 0.2, size: 1.4 }]] },
  'Fire Tornado': { elem: 'fire', colors: ['#ff5a1a', '#ffe08a'], charge: 'mouth', release: 0.28, impact: 'blast', shake: 0.28, scale: [1.6, 1.7, 1.8],
    fx: [['tornado', { height: 3.0, size: 1.2, cell: 'flame' }], ['erupt', { at: 0.2, kind: 'flame', to: 'center', area: true, count: 5, height: 2.2 }]] },

  // ---- bosses ----
  'Heat Viper': { elem: 'fire', colors: ['#ff4a1a', '#ffe08a'], charge: 'mouth', release: 0.3, impact: 'blast', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['beam', { width: 0.36, dur: 0.5, charge: 0.15, to: 'center', core: '#ffffff' }], ['erupt', { at: 0.2, kind: 'flame', to: 'center', area: true, count: 4, height: 1.6 }]] },
  'Trump Sword': { elem: 'dark', colors: ['#c01a3a', '#ffffff'], charge: 'none', release: 0.15, impact: 'dark', shake: 0.18, scale: [1.5, 1.6, 1.7],
    fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.2 }], ['swarm', { at: 0.1, cell: 'blade', count: 4, size: 0.5, speed: 14, fan: true, spread: 0.3 }], ['swarm', { at: 0.15, cell: 'card', count: 8, size: 0.3, speed: 6, spread: 1.0, spin: 6 }]] },
  'River of Power': { elem: 'water', colors: ['#2a6aff', '#e0f4ff'], charge: 'mouth', release: 0.28, impact: 'splash', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['beam', { width: 0.4, dur: 0.6, charge: 0.15, core: '#ffffff', overshoot: 4 }]] },
  'Spiral Masquerade': { elem: 'holy', colors: ['#ff8ad8', '#ffffff'], charge: 'none', release: 0.12, impact: 'holy', shake: 0.15, scale: [1.5, 1.6, 1.7],
    fx: [['dash', { ghosts: 6, slashes: 0, dur: 0.2 }], ['tornado', { at: 0.1, height: 2.4, size: 1.0, cell: 'petal' }], ['slash', { at: 0.2, count: 4, size: 1.4, spread: 0.9, gap: 0.03 }]] },
  'Kaiser Nail': { elem: 'beast', colors: ['#ffd84a', '#ffffff'], charge: 'none', release: 0.12, impact: 'hit', shake: 0.2, scale: [1.5, 1.6, 1.7],
    fx: [['streak', { width: 0.7, dur: 0.2, trail: 'spark' }], ['slash', { at: 0.1, count: 3, claw: true, size: 1.8, spread: 0.7 }]] },
  'End Waltz': { elem: 'metal', colors: ['#6a6a8a', '#ffd84a'], charge: 'ground', release: 0.28, impact: 'metal', shake: 0.3, scale: [1.6, 1.7, 1.8],
    fx: [['wheel', { size: 1.2, cell: 'blade', dur: 0.5, saw: true }], ['shock', { at: 0.2, size: 2.6, cell: 'spark' }]] },
  'Breath of Wyvern': { elem: 'fire', colors: ['#c8a02a', '#fff0c0'], charge: 'hands', release: 0.28, impact: 'blast', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['sphere', { size: 0.8, grow: 0.3, from: 'hands', trail: 'flame', cell: 'ember', impactSize: 2 }]] },
  'Extreme Jihad': { elem: 'holy', colors: ['#ffd84a', '#ffffff'], charge: 'aura', release: 0.25, impact: 'holy', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['dome', { size: 1.5, orbit: 'star', count: 8 }], ['shock', { at: 0.15, to: 'self', size: 2.6, cell: 'star', debris: false }]] },
  'Catastrophe Cannon': { elem: 'dark', colors: ['#3a0a3a', '#ff3a8a'], charge: 'mouth', release: 0.28, impact: 'dark', shake: 0.28, scale: [1.6, 1.7, 1.8],
    fx: [['sphere', { size: 0.7, grow: 0.3, from: 'mouth', trail: 'dark', cell: 'wisp', impactSize: 2 }]] },
  'Ultimate Flare': { elem: 'dark', colors: ['#5a0a2a', '#ff8a2a'], charge: 'sky', release: 0.3, impact: 'blast', shake: 0.35, scale: [1.8, 1.9, 2.0],
    fx: [['meteor', { size: 0.6, count: 4, height: 10, trail: 'flame' }], ['erupt', { at: 0.4, kind: 'dark', to: 'center', area: true, count: 6, height: 2.4 }]] },
  'Crimson Flame': { elem: 'fire', colors: ['#ff2a0a', '#ffe08a'], charge: 'sky', release: 0.3, impact: 'blast', shake: 0.3, scale: [1.7, 1.8, 1.9],
    fx: [['swarm', { cell: 'feather', count: 12, size: 0.4, speed: 10, fan: true, spread: 0.8, color: '#ff4a1a' }], ['erupt', { at: 0.3, kind: 'flame', to: 'center', area: true, count: 6, height: 2.4 }]] },
  'Rostrum': { elem: 'water', colors: ['#8a1a2a', '#ffb0a0'], charge: 'mouth', release: 0.25, impact: 'splash', shake: 0.25, scale: [1.6, 1.7, 1.8],
    fx: [['streak', { width: 0.9, dur: 0.25, trail: 'bubble' }], ['slash', { at: 0.1, count: 2, size: 1.8, claw: true }]] },
  'Lampranthus': { elem: 'dark', colors: ['#3a0a1a', '#ff5a1a'], charge: 'ground', release: 0.3, impact: 'dark', shake: 0.35, scale: [1.7, 1.8, 2.0],
    fx: [['shock', { hammer: true, size: 3.0, cell: 'ember' }], ['erupt', { at: 0.15, kind: 'dark', area: true, count: 6, height: 2.4 }]] },
};

// ---------- basic attacks per line ----------
export const BASIC = {
  agumon: { kind: 'claw', elem: 'fire' },
  gabumon: { kind: 'bite', elem: 'ice', colors: ['#7ab8ff', '#ffffff'] },
  tentomon: { kind: 'shot', elem: 'thunder', head: 'spark', trail: 'zap', size: 0.22, arc: 0.1, speed: 18 },
  palmon: { kind: 'shot', elem: 'plant', head: 'seed', size: 0.18, arc: 0.4 },
  gomamon: { kind: 'tail', elem: 'water', colors: ['#7ad0ff', '#ffffff'] },
  biyomon: { kind: 'shot', elem: 'fire', head: 'feather', trail: 'flame', size: 0.26, arc: 0.3, colors: ['#ff7aa0', '#ffd0e0'] },
  hagurumon: { kind: 'punch', elem: 'metal' },
  demidevimon: { kind: 'bite', elem: 'dark' },
  guilmon: { kind: 'claw', elem: 'fire', colors: ['#ff3a2a', '#ffd06a'] },
  renamon: { kind: 'kick', elem: 'holy', colors: ['#ffd27a', '#ffffff'] },
  terriermon: { kind: 'shot', elem: 'wind', head: 'ring', size: 0.24, arc: 0.1, speed: 18, colors: ['#9affc8', '#ffffff'] },
  patamon: { kind: 'shot', elem: 'wind', head: 'bubble', size: 0.22, arc: 0.25, colors: ['#bfe8ff', '#ffffff'] },
  salamon: { kind: 'shot', elem: 'holy', head: 'paw', size: 0.22, arc: 0.3, spin: 0 },
  betamon: { kind: 'shot', elem: 'thunder', head: 'bolt', size: 0.26, arc: 0.15, colors: ['#5ab8ff', '#ffffff'] },
  wormmon: { kind: 'sting', elem: 'plant' },
  elecmon: { kind: 'punch', elem: 'thunder', colors: ['#ffe94a', '#ff8a4a'] },
  hawkmon: { kind: 'peck', elem: 'wind', colors: ['#ffffff', '#ff6a4a'] },
  keramon: { kind: 'shot', elem: 'dark', head: 'square', trail: 'data', size: 0.2, spin: 6, colors: ['#ff3a5a', '#ffd0e0'] },
  lopmon: { kind: 'kick', elem: 'beast', colors: ['#d8b08a', '#ffffff'] },
  gizamon: { kind: 'bite', elem: 'water', colors: ['#7ab0ff', '#ffffff'] },
  candlemon: { kind: 'shot', elem: 'fire', head: 'flame', trail: 'flame', size: 0.24, arc: 0.35, colors: ['#ffa02a', '#fff4b0'] },
  veemon: { kind: 'punch', elem: 'beast', colors: ['#4aa0ff', '#ffffff'] },
  dorumon: { kind: 'shot', elem: 'metal', head: 'dot', trail: 'smoke', size: 0.2, arc: 0.1, speed: 20, colors: ['#b8c4d8', '#ffffff'] },
  gaomon: { kind: 'punch', elem: 'beast', colors: ['#4a8aff', '#ffffff'] },
  lalamon: { kind: 'shot', elem: 'plant', head: 'seed', size: 0.18, arc: 0.35, colors: ['#8aff5a', '#fff6a0'] },
  commandramon: { kind: 'shot', elem: 'metal', head: 'streak', size: 0.3, arc: 0, speed: 30, colors: ['#ffd27a', '#ffffff'] },
  myotismon: { kind: 'shot', elem: 'dark', head: 'bat', size: 0.26, arc: 0.3, colors: ['#ff1a3a', '#ffb0c0'] },
  imperialdramon: { kind: 'shot', elem: 'holy', head: 'glow', size: 0.26, arc: 0.05, speed: 22, colors: ['#ff5ad8', '#ffffff'] },
  lucemon: { kind: 'shot', elem: 'holy', head: 'star', size: 0.24, arc: 0.2, spin: 6, colors: ['#ffe04a', '#ffffff'] },
  beelzemon: { kind: 'shot', elem: 'dark', head: 'streak', size: 0.3, arc: 0, speed: 30, colors: ['#ff3a3a', '#ffe0a0'] },
  machinedramon: { kind: 'claw', elem: 'metal', colors: ['#ff8a2a', '#ffffff'] },
  goblimon: { kind: 'punch', elem: 'beast', colors: ['#d8c8a8', '#ffffff'] },
  kuwagamon: { kind: 'slash', elem: 'beast', colors: ['#ff4a4a', '#ffffff'] },
  ogremon: { kind: 'punch', elem: 'beast', colors: ['#d8c8a8', '#ffffff'] },
  tyrannomon: { kind: 'bite', elem: 'fire' },
  etemon: { kind: 'punch', elem: 'dark', colors: ['#8a3aff', '#ffd84a'] },
  skullgreymon: { kind: 'claw', elem: 'dark', colors: ['#d8d0c0', '#a04aff'] },
  numemon: { kind: 'punch', elem: 'plant', colors: ['#8a6a2a', '#c8e05a'] },
  otamamon: { kind: 'shot', elem: 'water', head: 'note', size: 0.22, arc: 0.3, colors: ['#7ad0ff', '#ffffff'] },
  gotsumon: { kind: 'punch', elem: 'metal', colors: ['#a89a88', '#e8e0d0'] },
  falcomon: { kind: 'slash', elem: 'wind', colors: ['#c8d8ff', '#ffffff'] },
  lunamon: { kind: 'shot', elem: 'water', head: 'drop', trail: 'bubble', size: 0.2, arc: 0.1, speed: 20, colors: ['#8ab8ff', '#ffffff'] },
  armadillomon: { kind: 'tail', elem: 'metal', colors: ['#d8a85a', '#fff6d0'] },
  kudamon: { kind: 'shot', elem: 'holy', head: 'star', size: 0.2, arc: 0.2, colors: ['#fff0a0', '#ffffff'] },
  fanbeemon: { kind: 'shot', elem: 'thunder', head: 'needle', size: 0.22, arc: 0.05, speed: 22, colors: ['#ffd23f', '#ffffff'] },
  mamemon: { kind: 'punch', elem: 'metal', colors: ['#ffb04a', '#ffffff'] },
  dracmon: { kind: 'claw', elem: 'dark', colors: ['#6a2a6a', '#ff5a8a'] },
  agumonsv: { kind: 'claw', elem: 'fire', colors: ['#ff6a1a', '#ffe08a'] },
  dracomon: { kind: 'bite', elem: 'fire', colors: ['#5aff6a', '#e0ffe0'] },
  woodmon: { kind: 'shot', elem: 'plant', head: 'seed', size: 0.2, arc: 0.35, colors: ['#8a5a2a', '#c8e05a'] },
  monodramon: { kind: 'claw', elem: 'beast', colors: ['#9a6aff', '#ffffff'] },
  jazamon: { kind: 'claw', elem: 'beast', colors: ['#ffb02a', '#fff0c0'] },
  zubamon: { kind: 'slash', elem: 'metal', colors: ['#ffb04a', '#ffffff'] },
  agunimon: { kind: 'punch', elem: 'fire', colors: ['#ff5a1a', '#ffe08a'] },
  lobomon: { kind: 'slash', elem: 'holy', colors: ['#7ad8ff', '#ffffff'] },
  hackmon: { kind: 'claw', elem: 'fire', colors: ['#ff4a3a', '#ffffff'] },
  blackgatomon: { kind: 'shot', elem: 'dark', head: 'paw', size: 0.22, arc: 0.25, colors: ['#c05aff', '#ffd84a'] },
  meicoomon: { kind: 'claw', elem: 'dark', colors: ['#ffd84a', '#8a5aff'] },
  vorvomon: { kind: 'bite', elem: 'fire', colors: ['#ff6a1a', '#ffd84a'] },
  ryudamon: { kind: 'slash', elem: 'metal', colors: ['#d8e4f4', '#ffffff'] },
  veedramon: { kind: 'punch', elem: 'beast', colors: ['#4a8aff', '#ffffff'] },
  omnimon: { kind: 'slash', elem: 'holy', colors: ['#ffffff', '#ffd84a'] },
  alphamon: { kind: 'slash', elem: 'holy', colors: ['#3a3a6a', '#ffd84a'] },
  daemon: { kind: 'claw', elem: 'dark', colors: ['#c01a1a', '#ff8a2a'] },
  kimeramon: { kind: 'claw', elem: 'fire', colors: ['#ff4a1a', '#ffe08a'] },
  piedmon: { kind: 'slash', elem: 'dark', colors: ['#c01a3a', '#ffffff'] },
  metalseadramon: { kind: 'shot', elem: 'water', head: 'bubble', trail: 'bubble', size: 0.3, arc: 0.1, speed: 18, colors: ['#2a6aff', '#e0f4ff'] },
  crusadermon: { kind: 'slash', elem: 'holy', colors: ['#ff8ad8', '#ffffff'] },
  leopardmon: { kind: 'claw', elem: 'beast', colors: ['#ffd84a', '#ffffff'] },
  craniamon: { kind: 'slash', elem: 'metal', colors: ['#6a6a8a', '#ffd84a'] },
  dynasmon: { kind: 'punch', elem: 'fire', colors: ['#c8a02a', '#fff0c0'] },
  magnamon: { kind: 'punch', elem: 'holy', colors: ['#ffd84a', '#ffffff'] },
  diaboromon: { kind: 'claw', elem: 'dark', colors: ['#3a0a3a', '#ff3a8a'] },
  armageddemon: { kind: 'shot', elem: 'dark', head: 'wisp', trail: 'dark', size: 0.34, arc: 0.3, colors: ['#5a0a2a', '#ff8a2a'] },
  leviamon: { kind: 'bite', elem: 'water', colors: ['#8a1a2a', '#ffb0a0'] },
  barbamon: { kind: 'shot', elem: 'dark', head: 'skull', trail: 'dark', size: 0.28, arc: 0.3, colors: ['#5a2a8a', '#ffd84a'] },
  belphemon: { kind: 'claw', elem: 'dark', colors: ['#3a0a1a', '#ff5a1a'] },
  okuwamon: { kind: 'slash', elem: 'beast', colors: ['#3a5aff', '#ffffff'] },
  phoenixmon: { kind: 'shot', elem: 'fire', head: 'feather', trail: 'flame', size: 0.3, arc: 0.3, colors: ['#ff6a1a', '#fff0a0'] },
  sukamon: { kind: 'shot', elem: 'plant', head: 'drop', trail: 'smoke', size: 0.24, arc: 0.8, colors: ['#8a5a2a', '#e8c890'] },
  nanimon: { kind: 'punch', elem: 'beast' },
  monzaemon: { kind: 'punch', elem: 'holy', colors: ['#ff6ab8', '#ffe0f0'] },
  frigimon: { kind: 'punch', elem: 'ice' },
  bakemon: { kind: 'claw', elem: 'dark' },
  seraphimon: { kind: 'shot', elem: 'holy', head: 'star', trail: 'holy', size: 0.26, arc: 0.2 },
  ophanimon: { kind: 'shot', elem: 'holy', head: 'cross', trail: 'holy', size: 0.26, arc: 0.1 },
  magnadramon: { kind: 'shot', elem: 'fire', head: 'flame', trail: 'flame', size: 0.28, arc: 0.2 },
};

// ---------- fallbacks for lines without specs ----------
const ELEMENTS = [
  ['dark', /death|dark|demi|inferno|evil|venom|pandemonium|crimson|hell|nightmare|lost|masquerade|shadow|curse|ghost|phantom|bat/i],
  ['holy', /heaven|talisman|cross|divine|gate|fate|deva|angel|celestial|light|charm|sword|holy|saint|seven/i],
  ['thunder', /shock|thunder|lightning|electric|static|spark|electro|javelin|plasma|volt|bolt/i],
  ['ice', /blue blaster|howling|ice|frost|snow|freez|blizzard|glacier/i],
  ['fire', /pepper|nova|pyro|atomic|flame|fire|blaze|inferno|wax|heat|burn|meteor|destroyer|mugen|ground zero|magma|solar|sun/i],
  ['water', /fish|torpedo|harpoon|bubble|coral|aqua|tidal|wave|water|ocean|rain/i],
  ['plant', /ivy|needle|flower|seed|shower|leaf|net|spiking|poison|vine|thorn|petal|root/i],
  ['wind', /twister|wing|rings|storm|wheel|tornado|cyclone|wind|gale|air|feather/i],
  ['metal', /gear|gatling|metal|cannon|laser|missile|m16|crusher|barrage|positron|attack|shooter|beam|gun|blaster|rocket/i],
];
export function elementOf(name) {
  const hit = ELEMENTS.find(([, re]) => re.test(name || ''));
  return hit ? hit[0] : 'beast';
}
const IMPACT_OF = { fire: 'blast', ice: 'frost', thunder: 'zap', water: 'splash', plant: 'poison', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'metal', beast: 'hit' };
const SINGLE = {
  fire: [['orb', { size: 0.3, trail: 'flame' }]], ice: [['spikes', { kind: 'ice' }]], thunder: [['bolt', { count: 2 }]], water: [['orb', { head: 'bubble', trail: 'bubble' }]],
  plant: [['whip', {}]], wind: [['tornado', {}]], holy: [['cross', {}]], dark: [['orb', { trail: 'dark' }]], metal: [['beam', { width: 0.16 }]], beast: [['slash', { count: 2 }]],
};
const AREA = {
  fire: [['meteor', {}]], ice: [['erupt', { kind: 'ice', to: 'center', area: true }]], thunder: [['bolt', { to: 'center', area: 4 }]], water: [['erupt', { kind: 'water', to: 'center', area: true }]],
  plant: [['erupt', { kind: 'thorn', to: 'center', area: true }]], wind: [['tornado', { to: 'center' }]], holy: [['cross', { to: 'center' }]], dark: [['vortex', {}]],
  metal: [['missiles', { count: 4 }]], beast: [['shock', { to: 'center' }]],
};
const BARRAGE_HEAD = { fire: ['flame', 'flame'], ice: ['shard', 'ice'], thunder: ['spark', 'zap'], water: ['bubble', 'bubble'], plant: ['leaf', 'leaf'], wind: ['ring', 'wind'], holy: ['star', 'holy'], dark: ['wisp', 'dark'], metal: ['streak', 'smoke'], beast: ['spark', 'spark'] };

export function fallbackSpec(name, type) {
  const elem = elementOf(name);
  const base = { elem, impact: IMPACT_OF[elem], release: 0.28, fallback: true };
  switch (type) {
    case 'aoe': return { ...base, charge: 'sky', shake: 0.12, fx: AREA[elem] };
    case 'slam': return { ...base, charge: 'ground', shake: 0.15, fx: [['shock', {}]] };
    case 'barrage': { const [head, trail] = BARRAGE_HEAD[elem]; return { ...base, charge: 'hands', fx: [['orb', { head, trail, size: 0.2, speed: 14 }]] }; }
    case 'dash': return { ...base, charge: 'none', release: 0.12, fx: [['dash', { claw: elem === 'beast' }]] };
    case 'shield': return { ...base, charge: 'aura', fx: [['dome', {}]] };
    case 'heal': return { ...base, charge: 'sky', impact: 'none', fx: [['heal', {}]] };
    case 'drain': return { ...base, charge: 'hands', fx: SINGLE[elem] };
    default: return { ...base, charge: elem === 'fire' || elem === 'ice' ? 'mouth' : 'aura', shake: 0.06, fx: SINGLE[elem] };
  }
}

const fbCache = new Map();
// the spec for an ability (line gives the ability type for the fallback)
// A set's own move specs (DT.Sets.extend(id, { art: { moves } })) win over the shared table.
export function specFor(name, line) {
  const S = DT.SET;
  const own = S && S.art && S.art.moves && S.art.moves[name];
  if (own) return own;
  const s = MOVES[name];
  if (s) return s;
  const key = `${name}|${line ? line.type : ''}`;
  let f = fbCache.get(key);
  if (!f) { f = fallbackSpec(name, line ? line.type : 'nuke'); fbCache.set(key, f); }
  return f;
}

export function basicFor(line, ranged) {
  const S = DT.SET;
  if (line && S && S.art && S.art.basic && S.art.basic[line.id]) return S.art.basic[line.id];
  // the shared table is keyed by Faction Wars line ids
  if (line && BASIC[line.id] && (!line.set || line.set === (DT.Sets && DT.Sets.DEFAULT))) return BASIC[line.id];
  const elem = line ? elementOf(line.abil[0]) : 'beast';
  const key = `${line ? line.id : ''}|${ranged ? 1 : 0}`;
  let b = fbCache.get(key);
  if (!b) {
    const [head, trail] = BARRAGE_HEAD[elem];
    b = ranged ? { kind: 'shot', elem, head, trail: elem === 'metal' ? null : trail, size: 0.22 } : { kind: elem === 'beast' ? 'claw' : 'slash', elem };
    fbCache.set(key, b);
  }
  return b;
}

// ---------- validation (tests/moves.cjs) ----------
const ELEMS = Object.keys(PALETTE);
const HEX = /^#[0-9a-f]{6}$/i;
// prims / impacts / charges / cells come from js/moveprims.js at runtime (DT.MoveFxInfo)
export function validateSpec(name, s, info) {
  const errs = [];
  if (!s || typeof s !== 'object') return [`${name}: no spec`];
  if (!ELEMS.includes(s.elem)) errs.push(`${name}: bad elem ${s.elem}`);
  if (s.colors && !(Array.isArray(s.colors) && s.colors.length === 2 && s.colors.every((c) => HEX.test(c)))) errs.push(`${name}: bad colors`);
  if (s.release != null && !(s.release >= 0 && s.release <= 0.8)) errs.push(`${name}: release out of range`);
  if (s.shake != null && !(s.shake >= 0 && s.shake <= 0.5)) errs.push(`${name}: shake out of range`);
  if (s.scale && !(Array.isArray(s.scale) && s.scale.every((v) => v > 0 && v < 4))) errs.push(`${name}: bad scale`);
  if (info) {
    if (s.charge && !info.charges.includes(s.charge)) errs.push(`${name}: bad charge ${s.charge}`);
    if (s.impact && !info.impacts.includes(s.impact)) errs.push(`${name}: bad impact ${s.impact}`);
  }
  if (!Array.isArray(s.fx) || !s.fx.length) errs.push(`${name}: fx must be a non-empty list`);
  else for (const step of s.fx) {
    if (!Array.isArray(step) || typeof step[0] !== 'string') { errs.push(`${name}: bad step`); continue; }
    const [p, o = {}] = step;
    if (info && !info.prims.includes(p)) errs.push(`${name}: unknown primitive ${p}`);
    if (o.impact && info && !info.impacts.includes(o.impact)) errs.push(`${name}: bad step impact ${o.impact}`);
    for (const k of ['color', 'core', 'color2', 'headColor']) if (o[k] && !['a', 'b', 'w'].includes(o[k]) && !HEX.test(o[k])) errs.push(`${name}: bad ${k} ${o[k]}`);
    for (const k of ['head', 'cell', 'mix', 'orbit']) if (o[k] && info && !info.cells.includes(o[k])) errs.push(`${name}: unknown cell ${o[k]}`);
    if (o.at != null && !(o.at >= 0 && o.at < 2)) errs.push(`${name}: step at out of range`);
  }
  return errs;
}

export function validateBasic(id, b, info) {
  const errs = [];
  const KINDS = ['claw', 'bite', 'punch', 'horn', 'slash', 'peck', 'kick', 'tail', 'sting', 'shot'];
  if (!KINDS.includes(b.kind)) errs.push(`${id}: bad basic kind ${b.kind}`);
  if (b.elem && !ELEMS.includes(b.elem)) errs.push(`${id}: bad basic elem`);
  if (b.colors && !(b.colors.length === 2 && b.colors.every((c) => HEX.test(c)))) errs.push(`${id}: bad basic colors`);
  if (b.head && info && !info.cells.includes(b.head)) errs.push(`${id}: unknown head ${b.head}`);
  return errs;
}

DT.MOVES = MOVES;
DT.MOVE_BASIC = BASIC;
DT.Moves = { specFor, basicFor, fallbackSpec, validateSpec, validateBasic, elementOf };
