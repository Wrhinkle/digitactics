// Faction Wars item art and most move looks are the shared base in js/itemart.js and js/moves.js.
// Other sets register theirs with DT.Sets.extend('<id>', { art: { items, moves, basic } }) (docs/sets/FORMAT.md).
// Below: looks for the abilities of the forms swapped in for Cyber Sleuth-family models (October 2026).
(function (G) {
  const DT = G.DT;
  DT.Sets.extend('faction-wars', {
    art: {
      moves: {
        // SkullSatamon (was NeoDevimon)
        'Nail Bone': { elem: 'dark', colors: ['#9a2aff', '#e8e0d0'], charge: 'none', release: 0.12, impact: 'dark', shake: 0.15,
          fx: [['dash', { ghosts: 6, slashes: 1, dur: 0.2, size: 1.5 }], ['erupt', { at: 0.22, kind: 'dark', height: 2.4 }]] },
        // Coelamon and Dragomon (were Gesomon and MarineDevimon)
        'Ancient Fin': { elem: 'water', colors: ['#6ab0ff', '#e0f4ff'], charge: 'none', release: 0.22, impact: 'splash', fx: [['whip', { count: 2, dur: 0.4, width: 0.18, sway: 1.2, leafy: false }]] },
        'Forbidden Trident': { elem: 'dark', colors: ['#3a8a7a', '#c0ffe8'], charge: 'hands', release: 0.22, impact: 'dark',
          fx: [['swarm', { cell: 'blade', count: 3, size: 0.4, speed: 14, wobble: 0.2, arc: 0.1, color: '#2a6a5a', color2: '#c0ffe8' }]] },
        // Meramon and Wisemon (were Candlemon and Mistymon)
        'Burning Fist': { elem: 'fire', colors: ['#ff6a1a', '#fff4b0'], charge: 'hands', release: 0.2, impact: 'blast', fx: [['orb', { size: 0.3, speed: 11, arc: 0.3, trail: 'flame' }]] },
        'Pandora Dialogue': { elem: 'dark', colors: ['#6a8aff', '#e0a0ff'], charge: 'sky', release: 0.25, impact: 'dark', shake: 0.1,
          fx: [['erupt', { kind: 'dark', to: 'center', area: true, count: 5, height: 2 }], ['swarm', { cell: 'wisp', count: 8, size: 0.35, speed: 6, wobble: 1.4, fan: true, spread: 1 }]] },
        // ToyAgumon and MetalTyrannomon (were Commandramon and Tankdramon; Tankmon keeps Hyper Cannon)
        'Toy Flame': { elem: 'fire', colors: ['#ffd23f', '#ffffff'], charge: 'mouth', release: 0.15, impact: 'blast', fx: [['volley', { count: 3, rate: 0.05, size: 0.35, guns: 1 }]] },
        'Nuclear Laser': { elem: 'metal', colors: ['#ff5a2a', '#fff0c0'], charge: 'mouth', release: 0.25, impact: 'blast', shake: 0.15,
          fx: [['beam', { width: 0.24, dur: 0.45, charge: 0.15, impactSize: 1.3 }]] },
        // GranDracmon (was MaloMyotismon)
        'Eyes of the Gorgon': { elem: 'dark', colors: ['#c01a3a', '#ffb0c0'], charge: 'sky', release: 0.3, impact: 'dark', shake: 0.25,
          fx: [['vortex', { dur: 1.1, up: true, cell: 'wisp' }], ['swarm', { cell: 'bat', count: 18, size: 0.38, speed: 8, wobble: 1.6, fan: true, spread: 1.2, color: '#3a0a1a', color2: '#ff2a5a' }]] },
        // Flamedramon (was Imperialdramon DM)
        'Fire Rocket': { elem: 'fire', colors: ['#ff6a1a', '#ffe08a'], charge: 'none', release: 0.2, impact: 'blast', shake: 0.2, scale: [1.3, 1.5, 1.7],
          fx: [['dash', { ghosts: 5, slashes: 0, dur: 0.22, size: 1.4 }], ['erupt', { at: 0.22, kind: 'flame', to: 'center', area: true, count: 4, height: 1.5 }]] },
      },
    },
  });
})(typeof window !== 'undefined' ? window : globalThis);
