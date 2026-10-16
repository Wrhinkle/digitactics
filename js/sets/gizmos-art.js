// Gizmos & Gadgets art: item, Chip, consumable and Digivice upgrade portraits (SVG painters on the js/itemart.js
// kit), a move look for every ability and a basic attack look for every line (js/moves.js spec format).
(function (G) {
  const DT = G.DT;
  const def = DT.Sets.def('gizmos');
  if (!def) return;

  // ---------- move looks ----------
  const IMP = { fire: 'blast', ice: 'frost', thunder: 'zap', water: 'splash', plant: 'poison', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'metal', beast: 'hit' };
  const S = (elem, fx, o = {}) => ({ elem, impact: IMP[elem], release: 0.26, charge: 'aura', ...o, fx });
  const orb = (elem, o = {}, s = {}) => S(elem, [['orb', { size: 0.26, speed: 12, arc: 0.25, ...o }]], { charge: 'mouth', ...s });
  const beam = (elem, o = {}, s = {}) => S(elem, [['beam', { width: 0.18, dur: 0.35, charge: 0.1, ...o }]], { charge: 'mouth', ...s });
  const dash = (elem, extra = [], o = {}, s = {}) => S(elem, [['dash', { ghosts: 5, slashes: 1, dur: 0.2, ...o }], ...extra], { charge: 'none', release: 0.12, ...s });
  const dome = (elem, extra = [], o = {}, s = {}) => S(elem, [['dome', { ...o }], ...extra], { charge: 'aura', ...s });
  const heal = (elem, o = {}, extra = [], s = {}) => S(elem, [['heal', { ...o }], ...extra], { charge: 'sky', impact: 'none', ...s });
  const shock = (elem, o = {}, extra = [], s = {}) => S(elem, [['shock', { ...o }], ...extra], { charge: 'ground', shake: 0.14, ...s });

  const moves = {
    // Meramon line
    Fireball: orb('fire', { size: 0.22, trail: 'flame' }),
    'Burning Fist': S('fire', [['streak', { width: 0.5, dur: 0.2, trail: 'flame' }], ['slash', { at: 0.1, count: 1, size: 1.2 }]], { charge: 'hands', shake: 0.08 }),
    'Heavy Metal Fire': S('fire', [['sphere', { size: 0.5, grow: 0.35, from: 'hands', trail: 'flame', cell: 'flame', impactSize: 1.5 }]], { charge: 'hands', shake: 0.14, colors: ['#ff4a1a', '#ffe08a'] }),
    // Commandramon line
    'M16 Assassin': S('metal', [['volley', { count: 6, rate: 0.04, cell: 'streak', size: 0.4, from: 'hands', speed: 24 }]], { charge: 'hands' }),
    'Death Behind': beam('metal', { width: 0.07, dur: 0.22, charge: 0.12 }, { colors: ['#ff3a3a', '#ffffff'], charge: 'hands' }),
    'Striver Cannon': S('metal', [['beam', { width: 0.32, dur: 0.45, charge: 0.15, impactSize: 1.6 }], ['shock', { at: 0.4, to: 'targets', debris: true }]], { charge: 'hands', shake: 0.2 }),
    // Zubamon line
    'Twenty Dive': dash('metal', [['slash', { at: 0.15, count: 2, size: 1.1 }]]),
    'Road T(w)o Decade': S('metal', [['wave', { width: 0.6, count: 1, dur: 0.4 }]], { charge: 'hands', shake: 0.1 }),
    'Glass Slash': S('metal', [['slash', { count: 3, size: 1.4, spread: 0.6 }], ['shock', { at: 0.2, cell: 'shard' }]], { charge: 'hands', shake: 0.12, colors: ['#bfe8ff', '#ffffff'] }),
    // Goblimon line
    'Goblin Strike': shock('beast', { hammer: true, cell: 'spark' }),
    'Pummel Whack': shock('beast', { hammer: true, debris: true }),
    'Skull Hammer': shock('dark', { hammer: true, cell: 'skull' }, [['erupt', { at: 0.2, kind: 'dark', height: 1.4 }]], { shake: 0.2 }),
    // ToyAgumon line
    'Plastic Blaze': orb('fire', { size: 0.24, trail: 'flame', head: 'square' }),
    'Big Bang Voice': S('wind', [['rings', { count: 3, size: 0.6, gap: 0.08 }]], { charge: 'mouth', shake: 0.08, colors: ['#ffb04a', '#ffffff'] }),
    'Volcanic Driver': S('fire', [['erupt', { kind: 'flame', height: 1.8 }]], { charge: 'ground', shake: 0.18 }),
    // Syakomon line
    'Black Pearl Blast': orb('water', { head: 'bubble', trail: 'bubble', size: 0.28 }, { colors: ['#3a3a6a', '#e8f0ff'] }),
    'Octopus Trap': S('water', [['whip', { dur: 0.45, width: 0.16, count: 3, sway: 1.1 }]], { charge: 'hands', colors: ['#c05aff', '#ffd0f0'] }),
    'Forbidden Trident': S('dark', [['spikes', { count: 3, kind: 'dark', size: 1.2 }]], { charge: 'hands', shake: 0.16 }),
    // Kotemon line
    'Hot Head': shock('fire', { cell: 'ember' }),
    'Wheel Rush': S('metal', [['wheel', { size: 0.8, cell: 'gear', dur: 0.4, to: 'self' }], ['shock', { at: 0.2, to: 'self', cell: 'spark' }]], { charge: 'ground', shake: 0.1 }),
    'Grand Smash': shock('holy', { hammer: true, debris: true }, [['pillar', { at: 0.1, to: 'self', size: 0.6, height: 4 }]], { shake: 0.18 }),
    // Tsukaimon line
    'Bad Message': S('dark', [['swarm', { cell: 'bat', count: 6, size: 0.3, speed: 9, wobble: 0.8 }]], { charge: 'mouth' }),
    'Death Claw': dash('dark', [['slash', { at: 0.2, count: 2, claw: true, size: 1.3 }]]),
    'Zero Freeze': S('ice', [['breath', { dur: 0.5, width: 0.4, cell: 'snow', size: 0.4, speed: 9 }]], { charge: 'mouth', shake: 0.08 }),
    // Bakemon line
    "Hell's Hand": dash('dark', [['whip', { at: 0.12, dur: 0.3, width: 0.16, sway: 0.4 }]]),
    'Soul Chopper': dash('dark', [['slash', { at: 0.15, count: 1, size: 1.6 }]]),
    'Shadow Scythe': dash('dark', [['slash', { at: 0.15, count: 2, size: 1.8 }], ['vortex', { at: 0.25, size: 1 }]], { ghosts: 7 }, { shake: 0.15 }),
    // Kamemon line
    'Shell Guard': dome('water', [], { orbit: 'hex', count: 6 }),
    'Gawappa Punch': dome('water', [['shock', { at: 0.15, cell: 'drop', debris: false }]], {}),
    'Hydro Tornado': dome('water', [['tornado', { at: 0.1, cell: 'drop', height: 2.4 }]], {}, { shake: 0.1 }),
    // Mushroomon line
    'Fungus Crusher': S('plant', [['swarm', { cell: 'seed', count: 8, size: 0.25, speed: 8, wobble: 0.6 }]], { charge: 'hands' }),
    'Hazard Breath': S('plant', [['breath', { dur: 0.55, width: 0.45, cell: 'smoke', size: 0.45, speed: 8 }]], { charge: 'mouth', colors: ['#8aff3a', '#e8ffb0'] }),
    'Fire Fist of Shiva': S('fire', [['streak', { width: 0.6, dur: 0.22, trail: 'flame' }], ['slash', { at: 0.1, count: 4, size: 1.2, spread: 0.7 }]], { charge: 'hands', shake: 0.14 }),
    // Chuumon line
    'Cheese Bomb': orb('fire', { head: 'square', arc: 0.8, size: 0.26 }, { colors: ['#ffd23f', '#fff6c0'] }),
    'Poop Blast': orb('beast', { arc: 0.9, size: 0.3, trail: 'smoke' }, { colors: ['#8a5a2a', '#d0a070'] }),
    'King Stamp': S('beast', [['wave', { width: 0.8, count: 1, dur: 0.4, flat: true }]], { charge: 'ground', shake: 0.16, colors: ['#8a5a2a', '#ffd23f'] }),
    // Mamemon line
    'Smiley Bomb': orb('fire', { arc: 0.7, size: 0.32, impactSize: 1.4 }, { shake: 0.1 }),
    'Energy Bomb': S('thunder', [['sphere', { size: 0.45, grow: 0.3, from: 'hands', trail: 'zap', cell: 'bolt', impactSize: 1.4 }]], { charge: 'hands', shake: 0.12 }),
    'Big Smiley Bomber': S('fire', [['meteor', { size: 0.5, count: 3, height: 7 }]], { charge: 'sky', shake: 0.22 }),
    // Seadramon line
    'Ice Blast': S('ice', [['breath', { dur: 0.45, width: 0.35, cell: 'snow', size: 0.38, speed: 10 }]], { charge: 'mouth' }),
    'Mega Ice Blast': beam('ice', { width: 0.32, dur: 0.45 }, { shake: 0.1 }),
    'River of Power': S('water', [['beam', { width: 0.4, dur: 0.55, charge: 0.15, spiral: true }]], { charge: 'mouth', shake: 0.18 }),
    // Kokuwamon line
    'Electro Strike': S('thunder', [['bolt', { from: 'caster', width: 0.07 }]], { charge: 'hands' }),
    'Power Laser': beam('metal', { width: 0.22 }, { charge: 'hands' }),
    'Spiral Sword': S('thunder', [['slash', { count: 2, size: 1.4 }], ['bolt', { at: 0.1, from: 'caster', width: 0.08 }]], { charge: 'hands', shake: 0.12 }),
    // Terriermon line
    'Bunny Blast': orb('wind', { head: 'ring', trail: 'wind', size: 0.22, speed: 14 }),
    'White Statue': S('metal', [['volley', { count: 8, rate: 0.03, cell: 'streak', size: 0.35, guns: 2, from: 'hands' }]], { charge: 'hands' }),
    'Rapid Fire': S('metal', [['missiles', { count: 3, size: 0.12, from: 'back', curve: 1.2, arc: 1, dur: 0.5 }]], { charge: 'back' }),
    // Tinkermon line
    'Fairy Powder': heal('holy', { cell: 'star' }),
    'Tempest Twist': heal('wind', { cell: 'feather' }, [['tornado', { at: 0.05, to: 'self', height: 2 }]]),
    'Pit Bomb': heal('holy', { cell: 'heart', motes: true }),
    // Kuwagamon line
    'Scissor Claw': S('beast', [['slash', { count: 2, claw: true, size: 1.2 }]], { charge: 'hands' }),
    'Double Scissor Claw': S('beast', [['slash', { count: 4, claw: true, size: 1.3, spread: 0.5 }]], { charge: 'hands', shake: 0.1 }),
    'Dimension Scissor': S('dark', [['portal', { size: 1, dur: 0.5 }], ['slash', { at: 0.2, count: 4, claw: true, size: 1.6 }]], { charge: 'hands', shake: 0.18 }),
    // Kumamon line
    'Snow Bomber': orb('ice', { head: 'snow', trail: 'ice', arc: 0.6 }),
    'Glacier Torpedo': S('ice', [['missiles', { count: 2, size: 0.18, curve: 0.6, arc: 0.6, dur: 0.45, impactSize: 1.3 }]], { charge: 'mouth', shake: 0.1 }),
    'Ice Age': S('ice', [['erupt', { kind: 'ice', to: 'center', area: true, count: 5, height: 1.6 }]], { charge: 'ground', shake: 0.2 }),
    // Falcomon line
    'Scratch Smash': S('wind', [['slash', { count: 2, claw: true, size: 1.1 }]], { charge: 'hands' }),
    'Savage Emperor': S('dark', [['swarm', { cell: 'feather', count: 10, size: 0.3, speed: 11, fan: true, spread: 0.5 }]], { charge: 'back' }),
    'Blast Wing': S('wind', [['swarm', { cell: 'feather', count: 14, size: 0.32, speed: 13, fan: true, spread: 0.7 }], ['vortex', { at: 0.3, size: 1 }]], { charge: 'back', shake: 0.1 }),
    // Candlemon line
    Bonfire: orb('fire', { size: 0.2, trail: 'flame', speed: 14 }),
    'Thunder Cloud': S('thunder', [['bolt', { from: 'sky', count: 1 }]], { charge: 'sky' }),
    'Pandora Dialogue': orb('dark', { head: 'star', trail: 'dark', size: 0.24, speed: 13 }),
    // Dracmon line
    'Undead Fang': dash('dark', [['slash', { at: 0.15, count: 1, claw: true, size: 1.1 }]]),
    'Sticker Blade': dash('dark', [['slash', { at: 0.15, count: 2, size: 1.3 }]]),
    'Thousand Arrows': dash('dark', [['arrows', { at: 0.15, count: 6, size: 0.8 }]], {}, { colors: ['#ff3a5a', '#ffd0d8'] }),
    // BlackGabumon line
    'Little Horn': S('beast', [['slash', { count: 1, size: 1 }]], { charge: 'none', release: 0.15 }),
    'Freeze Fang': S('ice', [['slash', { count: 2, claw: true, size: 1.2 }]], { charge: 'mouth' }),
    'Wolf Claw': S('dark', [['streak', { width: 0.6, dur: 0.22, trail: 'dark' }], ['slash', { at: 0.08, count: 3, claw: true, size: 1.4, spread: 0.5 }]], { charge: 'hands', shake: 0.12 }),
    // Solarmon line
    'Soul Shocker': S('thunder', [['bolt', { from: 'caster', width: 0.08 }]], { charge: 'hands', colors: ['#ffe94a', '#ffffff'] }),
    'Time Buster': S('holy', [['glyph', { glyph: 'circle', size: 1.2, dur: 0.6 }], ['beam', { at: 0.15, width: 0.2, dur: 0.35 }]], { charge: 'hands', colors: ['#ffd06a', '#fff6d0'] }),
    'Chrono Breaker': S('holy', [['portal', { size: 1.1, dur: 0.6 }], ['glyph', { at: 0.05, glyph: 'circle', size: 1.4, dur: 0.7, place: 'ground' }]], { charge: 'sky', shake: 0.12 }),
    // Woodmon line
    'Drain Buster': S('plant', [['whip', { dur: 0.4, width: 0.12, count: 2, leafy: true }]], { charge: 'hands' }),
    'Cherry Bomb': orb('plant', { head: 'petal', arc: 0.7, size: 0.3 }, { colors: ['#ff5a8a', '#ffd0e0'] }),
    'Flying Cross Cutter': S('plant', [['wheel', { size: 0.9, saw: true, dur: 0.45 }]], { charge: 'hands', shake: 0.1 }),
    // Hawkmon line
    'Feather Slash': S('wind', [['swarm', { cell: 'feather', count: 6, size: 0.3, speed: 12, fan: true }]], { charge: 'back' }),
    'Blast Laser': beam('wind', { width: 0.22 }, { colors: ['#ff7a3a', '#fff0c0'] }),
    'Supersonic Voice': S('wind', [['rings', { count: 4, size: 0.8, gap: 0.07 }]], { charge: 'mouth', shake: 0.12 }),
    // Airdramon line
    'Spinning Needle': S('wind', [['tornado', { height: 2.4, cell: 'needle' }]], { charge: 'back' }),
    'Heat Viper': S('fire', [['breath', { dur: 0.55, width: 0.5, cell: 'flame', size: 0.5, speed: 10 }]], { charge: 'mouth', shake: 0.14 }),
    'Dimension Destroyer': S('dark', [['portal', { size: 1.3, dur: 0.6 }], ['vortex', { at: 0.2, size: 1.4 }]], { charge: 'sky', shake: 0.25 }),
    // Gumdramon line
    'Ran-gum Break': shock('fire', { hammer: true, cell: 'ember' }),
    'Spin Caliber': S('metal', [['wheel', { size: 1, dur: 0.45 }]], { charge: 'hands', shake: 0.1 }),
    'Spiral Shredder': S('wind', [['tornado', { to: 'center', height: 2.6 }], ['slash', { at: 0.2, count: 3, size: 1.4 }]], { charge: 'sky', shake: 0.16 }),
    // Gizamon line
    'Spiral Edge': S('water', [['wheel', { size: 0.7, saw: true, dur: 0.4 }]], { charge: 'back' }),
    'Devil Bashing': S('dark', [['slash', { count: 2, size: 1.4 }]], { charge: 'hands', shake: 0.1 }),
    'Dark Deluge': S('dark', [['erupt', { kind: 'water', height: 2 }]], { charge: 'ground', shake: 0.16, colors: ['#3a2a8a', '#9ad0ff'] }),
    // Bearmon line
    'Bear Fist': shock('beast', { cell: 'paw' }),
    'Maul Attack': shock('beast', { cell: 'paw' }, [['slash', { at: 0.1, count: 3, claw: true, size: 1.2 }]]),
    'The King of Fists': shock('beast', { hammer: true, debris: true }, [['streak', { width: 0.6, dur: 0.2 }]], { shake: 0.22, colors: ['#ffd06a', '#ffffff'] }),
    // Coronamon line
    'Corona Flame': dome('fire', [], { size: 1 }),
    'Flame Dive': dome('fire', [['erupt', { at: 0.1, kind: 'flame', to: 'self', height: 1.2 }]], {}),
    'Sol Blaster': dome('fire', [['pillar', { at: 0.1, to: 'self', size: 0.8, height: 5 }]], { size: 1.3 }, { shake: 0.12 }),
    // Starmon line
    'Pepper Breath': orb('fire', { size: 0.2, speed: 13, trail: 'flame' }),
    'Meteor Shower': S('fire', [['meteor', { size: 0.22, count: 1, height: 5 }]], { charge: 'sky' }),
    'Super Star Seeker': orb('holy', { head: 'star', trail: 'holy', size: 0.26, speed: 14 }),
    // Vegiemon line
    'Sweet Breath': S('plant', [['breath', { dur: 0.45, width: 0.35, cell: 'petal', size: 0.35, speed: 8 }]], { charge: 'mouth' }),
    'Unidentified Flying Kiss': S('dark', [['swarm', { cell: 'heart', count: 5, size: 0.3, speed: 8, wobble: 0.8 }]], { charge: 'mouth', colors: ['#ff4aa0', '#ffd0f0'] }),
    'Crusher Bone': orb('dark', { head: 'skull', trail: 'dark', size: 0.3 }),
    // Ranamon line
    'Jealousy Rain': S('water', [['rain', { cell: 'drop', count: 6, height: 4 }]], { charge: 'sky' }),
    'Acid Ink': orb('water', { head: 'drop', trail: 'bubble' }, { colors: ['#4a2a6a', '#c0a0ff'] }),
    'Titanic Tempest': orb('water', { head: 'bubble', trail: 'bubble', size: 0.3 }, { shake: 0.06 }),
    // BlackGatomon line
    'Lightning Paw': dome('thunder', [], { size: 0.9 }),
    'Darkness Wave': dome('dark', [['swarm', { at: 0.05, cell: 'bat', count: 6, size: 0.28, speed: 8 }]], {}),
    'Dark Despair': dome('dark', [['glyph', { at: 0.05, glyph: 'x', size: 1.4, dur: 0.6, place: 'ground' }]], { size: 1.2 }),
    // Salamon line
    'Petty Punch': heal('holy', { cell: 'paw' }),
    "Cat's Eye": heal('holy', { cell: 'star' }, [['glyph', { at: 0, glyph: 'circle', size: 1, dur: 0.5 }]]),
    'Celestial Arrow': heal('holy', { cell: 'cross', motes: true }, [['arrows', { at: 0.1, count: 3, rain: true, size: 0.7 }]]),
    // BlackAgumon line
    Spitfire: dash('fire', [['orb', { at: 0.12, size: 0.22, trail: 'flame' }]]),
    'Great Antler': dash('beast', [['slash', { at: 0.15, count: 2, size: 1.4 }]]),
    'Terra Destroyer': dash('dark', [['sphere', { at: 0.15, size: 0.7, grow: 0.3, from: 'above', trail: 'dark', impactSize: 1.8 }]], {}, { shake: 0.22 }),
    // Impmon line
    'Night of Fire': S('fire', [['orb', { count: 3, gap: 0.08, size: 0.2, trail: 'flame', arc: 0.4 }]], { charge: 'hands' }),
    "Jack O' Lantern": orb('fire', { head: 'flame', arc: 0.6, size: 0.32, impactSize: 1.3 }, { colors: ['#ff8a1a', '#ffe0a0'] }),
    'Trick or Treat Wallace': S('dark', [['erupt', { kind: 'dark', to: 'center', area: true, count: 4, height: 1.6 }], ['swarm', { at: 0.1, cell: 'bat', count: 8, size: 0.3 }]], { charge: 'sky', shake: 0.18 }),
    // Geremon line
    'Dirty Puddle': shock('plant', { cell: 'drop', debris: false }, [], { colors: ['#7a6a3a', '#c0b080'] }),
    'Rotten Tackle': shock('plant', { cell: 'smoke' }, [['streak', { width: 0.6, dur: 0.2, trail: 'smoke' }]]),
    'Stinking Gas': shock('plant', { cell: 'smoke', debris: false }, [['vortex', { at: 0.15, size: 1.2 }]], { shake: 0.12, colors: ['#9aff3a', '#e0ffa0'] }),
    // Youkomon line
    'Blazing Spheres': S('fire', [['orb', { count: 3, gap: 0.07, size: 0.24, trail: 'flame', side: 0.4, wobble: 0.4 }]], { charge: 'hands', colors: ['#3a8aff', '#d0e8ff'] }),
    'Exorcism Scrolls': S('holy', [['swarm', { cell: 'card', count: 8, size: 0.35, speed: 11, spin: 6 }]], { charge: 'hands' }),
    'Taizoukai Mandala': S('holy', [['glyph', { glyph: 'circle', size: 1.6, dur: 0.7 }], ['beam', { at: 0.2, width: 0.3, dur: 0.45 }]], { charge: 'hands', shake: 0.14 }),
    // Tyrannomon line
    'Wild Buster': shock('fire', { debris: true }, [['breath', { at: 0, dur: 0.35, width: 0.4, cell: 'flame', size: 0.4 }]]),
    'Nuclear Laser': S('metal', [['beam', { width: 0.34, dur: 0.4, charge: 0.12, to: 'center' }], ['shock', { at: 0.35, to: 'self' }]], { charge: 'mouth', shake: 0.2 }),
    'Rust Breath': S('dark', [['breath', { dur: 0.6, width: 0.6, cell: 'smoke', size: 0.5, speed: 8 }], ['shock', { at: 0.3, to: 'self' }]], { charge: 'mouth', shake: 0.2, colors: ['#a0602a', '#e0b080'] }),
    // Psychemon line
    'Colored Spark': orb('thunder', { head: 'spark', trail: 'zap', size: 0.22, speed: 14 }, { colors: ['#ff6ad8', '#a0f0ff'] }),
    'Gururu Thrust': orb('metal', { head: 'blade', size: 0.28, speed: 15, arc: 0.1 }),
    'Mad Dog Fire': S('metal', [['volley', { count: 10, rate: 0.03, cell: 'streak', size: 0.35, guns: 2, from: 'hands' }]], { charge: 'hands', shake: 0.06 }),
    // Shoutmon line
    'Bellow Blaster': dash('fire', [['rings', { at: 0.1, count: 2, size: 0.6 }]]),
    'Hard Rock Soul': dash('fire', [['swarm', { at: 0.12, cell: 'note', count: 6, size: 0.3, speed: 8 }]]),
    'Victorize Banking': dash('fire', [['streak', { at: 0.05, width: 0.7, dur: 0.22, trail: 'flame' }], ['slash', { at: 0.2, count: 2, size: 1.5 }]], {}, { shake: 0.16 }),
    // Lekismon line
    'Tear Arrow': S('ice', [['arrows', { count: 1, size: 0.8 }]], { charge: 'hands' }),
    'Ice Archery': S('ice', [['arrows', { count: 3, size: 0.9, gap: 0.08 }]], { charge: 'hands' }),
    'Arrow of Artemis': S('holy', [['beam', { width: 0.2, dur: 0.35, charge: 0.12 }], ['arrows', { at: 0.1, count: 3, size: 1 }]], { charge: 'hands', shake: 0.12 }),
    // Kunemon line
    'Electric Thread': S('thunder', [['whip', { dur: 0.35, width: 0.08, lightning: true }]], { charge: 'mouth' }),
    'Poison Sting': S('plant', [['slash', { count: 2, size: 1.2 }]], { charge: 'none', release: 0.15, colors: ['#9a3aff', '#d0ff7a'] }),
    'Insect Cannon': S('metal', [['beam', { width: 0.28, dur: 0.4, charge: 0.12 }], ['missiles', { at: 0.1, count: 3, size: 0.12, from: 'back', curve: 1 }]], { charge: 'mouth', shake: 0.16 }),
    // Hagurumon line
    'Cog Crusher': dome('metal', [], { orbit: 'gear', count: 4 }),
    'Twinkle Beam': dome('metal', [['beam', { at: 0.1, width: 0.14, dur: 0.3, to: 'self' }]], {}),
    'Atomic Ray': dome('holy', [['pillar', { at: 0.1, size: 0.7, height: 4.5 }]], { orbit: 'hex', count: 6 }, { shake: 0.1 }),
    // Floramon line
    'Rain of Pollen': S('plant', [['rain', { cell: 'petal', count: 8, height: 4 }]], { charge: 'sky' }),
    'Blast Shot': orb('plant', { head: 'seed', size: 0.24, speed: 14 }),
    'Serpent Ruin': S('plant', [['whip', { dur: 0.45, width: 0.16, count: 2, sway: 1 }], ['vortex', { at: 0.2, size: 1 }]], { charge: 'hands', shake: 0.12, colors: ['#ff6ad8', '#d0ffb0'] }),
    // Lalamon line
    'Sing a Song': heal('plant', { cell: 'note' }),
    'Lila Shower': heal('plant', { cell: 'petal' }, [['rain', { at: 0.05, cell: 'petal', count: 6, to: 'self' }]]),
    'Forbidden Temptation': heal('plant', { cell: 'petal', motes: true }, [['rain', { at: 0.05, cell: 'petal', count: 10, to: 'self' }]], { colors: ['#ff4a8a', '#ffd0e8'] }),
    // Dorumon line
    'Dash Metal': orb('metal', { head: 'streak', size: 0.24, speed: 15, arc: 0.05 }),
    'Ambush Crunch': S('beast', [['streak', { width: 0.5, dur: 0.2 }], ['slash', { at: 0.1, count: 1, claw: true, size: 1.2 }]], { charge: 'none', release: 0.15 }),
    'Cross Blade': S('metal', [['wheel', { size: 0.8, cell: 'blade', dur: 0.4 }]], { charge: 'hands' }),
    // Sistermon Blanc line
    'Divine Pierce': beam('holy', { width: 0.12, dur: 0.25 }, { charge: 'hands' }),
    'Protect Wave': S('holy', [['wave', { width: 0.7, count: 1, dur: 0.4 }]], { charge: 'hands', shake: 0.1 }),
    'Divine Pierce (Awake)': beam('holy', { width: 0.26, dur: 0.35, glyph: 'x' }, { charge: 'hands', shake: 0.14 }),
    // Gotsumon line
    'Earth Tremor': shock('beast', { debris: true, cell: 'shard' }),
    'Guardian Bomb': shock('beast', { debris: true }, [['erupt', { at: 0.15, kind: 'rock', to: 'center', area: true, count: 3, height: 1.2 }]], { shake: 0.2 }),
    Tenbagaeshi: shock('beast', { hammer: true, debris: true }, [['erupt', { at: 0.15, kind: 'rock', to: 'center', area: true, count: 6, height: 1.8 }]], { shake: 0.3 }),
    // Justimon line
    'Voltage Blitz': S('thunder', [['bolt', { from: 'sky', count: 2, area: 2 }]], { charge: 'hands', shake: 0.1 }),
    'Thunder Blade': S('thunder', [['slash', { count: 2, size: 1.5 }], ['bolt', { at: 0.1, to: 'center', count: 2, area: 3 }]], { charge: 'hands', shake: 0.16 }),
    'Trinity Arm': S('thunder', [['beam', { width: 0.3, dur: 0.4, charge: 0.12, to: 'center' }], ['bolt', { at: 0.2, to: 'center', count: 3, area: 4 }]], { charge: 'hands', shake: 0.24 }),
    // Sistermon Noir line
    'Mickey Bullet': S('metal', [['volley', { count: 8, rate: 0.03, cell: 'streak', size: 0.35, guns: 2, from: 'hands' }]], { charge: 'hands' }),
    'Bless Fire': S('fire', [['missiles', { count: 4, size: 0.14, from: 'hands', curve: 1.2, arc: 1.2, dur: 0.55, impactSize: 1.2 }]], { charge: 'hands', shake: 0.14 }),
    'Grand Sister Cruz': S('holy', [['cross', { to: 'center', size: 1.5 }], ['meteor', { at: 0.15, size: 0.5, count: 2 }]], { charge: 'sky', shake: 0.24 }),
    // Keramon line
    'Crazy Giggle': orb('dark', { head: 'wisp', trail: 'dark', size: 0.22, speed: 14 }),
    "Hell's Grenade": orb('fire', { arc: 0.7, size: 0.26, trail: 'smoke' }),
    'Catastrophe Cannon': orb('dark', { size: 0.3, trail: 'dark', speed: 15, impactSize: 1.3 }, { colors: ['#ff3a7a', '#ffd0e0'] }),
    // Myotismon line
    'Crimson Lightning': S('dark', [['whip', { dur: 0.4, width: 0.12, lightning: true, sway: 0.6 }]], { charge: 'hands', colors: ['#ff1a3a', '#ffb0c0'] }),
    'Nightmare Leeches': S('dark', [['swarm', { cell: 'bat', count: 12, size: 0.3, speed: 10, wobble: 0.8 }]], { charge: 'back', shake: 0.08 }),
    'Pandemonium Flame': S('dark', [['vortex', { size: 1.4 }], ['erupt', { at: 0.2, kind: 'dark', height: 2.4 }]], { charge: 'sky', shake: 0.22, colors: ['#a020ff', '#ff3a7a'] }),
    // Gekomon line
    'Tongue Attack': S('beast', [['whip', { dur: 0.3, width: 0.16, count: 1, sway: 0.2 }]], { charge: 'mouth', colors: ['#ff6aa0', '#ffd0e0'] }),
    'Musical Fist': S('beast', [['swarm', { cell: 'note', count: 6, size: 0.3, speed: 8 }], ['shock', { at: 0.25, cell: 'note' }]], { charge: 'hands', shake: 0.12 }),
    'Tonosama Jump': shock('water', { hammer: true, debris: true, cell: 'drop' }, [], { shake: 0.26 }),
    // Datamon line
    'Digital Bomb': orb('metal', { head: 'square', trail: 'data', arc: 0.5, size: 0.28 }, { colors: ['#5aff8a', '#e0ffe8'] }),
    'Infinity Cannon': beam('metal', { width: 0.3, dur: 0.45, twin: true }, { charge: 'back', shake: 0.16 }),
    'Hyper Infinity Cannon': S('metal', [['beam', { width: 0.42, dur: 0.55, charge: 0.15, twin: true }], ['missiles', { at: 0.15, count: 4, size: 0.14, from: 'back', curve: 1.2 }]], { charge: 'back', shake: 0.26 }),
    // Renamon line
    Kohenkyo: orb('ice', { head: 'shard', trail: 'ice', size: 0.22, speed: 15 }),
    'Talisman Spell': orb('holy', { head: 'card', trail: 'holy', size: 0.26, speed: 13 }),
    Izuna: S('thunder', [['bolt', { from: 'sky', count: 1, width: 0.1 }]], { charge: 'sky' }),
    // Lucemon line
    'Galaxy Crash': orb('holy', { head: 'star', trail: 'holy', size: 0.32, impactSize: 1.4 }, { charge: 'hands' }),
    'Paradise Lost': S('dark', [['pillar', { size: 0.8, height: 5 }], ['cross', { at: 0.1, size: 1.2 }]], { charge: 'sky', shake: 0.18 }),
    'Purgatorial Flame': S('dark', [['sphere', { size: 0.65, grow: 0.4, from: 'above', trail: 'dark', impactSize: 2 }]], { charge: 'sky', shake: 0.26, colors: ['#ff3a1a', '#3a0a4a'] }),
    // Arukenimon line
    'Spider Thread': dash('plant', [['net', { at: 0.12, size: 1 }]]),
    'Trump Sword': dash('dark', [['swarm', { at: 0.1, cell: 'blade', count: 4, size: 0.5, speed: 14 }]]),
    'Pernicious Waltz': dash('dark', [['slash', { at: 0.12, count: 5, size: 1.4, spread: 0.8 }]], { ghosts: 8 }, { shake: 0.18 }),
    // summons and wild Digimon
    'Sub Machine Gun': S('metal', [['volley', { count: 8, rate: 0.03, cell: 'streak', size: 0.3, from: 'chest' }]], { charge: 'hands' }),
    'Ultimate Slicer': S('metal', [['slash', { count: 2, size: 1.6, claw: true }]], { charge: 'hands', shake: 0.14 }),
    'Laser Cannon': beam('metal', { width: 0.36, dur: 0.45 }, { shake: 0.18 }),
    'Data Crusher': orb('dark', { head: 'square', trail: 'data', size: 0.26 }),
    'Gizumon Saw': S('metal', [['wheel', { size: 0.6, saw: true, dur: 0.35 }]], { charge: 'hands' }),
    'Assault Laser': beam('metal', { width: 0.16, dur: 0.3 }, { colors: ['#ff3a3a', '#ffe0e0'] }),
    'Hyper Cannon': beam('metal', { width: 0.3, dur: 0.4 }, { shake: 0.14 }),
    'Junk Chucker': orb('metal', { head: 'gear', arc: 0.8, size: 0.3, spin: true }, { colors: ['#9a8a6a', '#e0d0b0'] }),
    'Giga Heat': S('fire', [['breath', { dur: 0.6, width: 0.6, cell: 'flame', size: 0.55, speed: 10 }]], { charge: 'mouth', shake: 0.2 }),
    Lampranthus: shock('dark', { hammer: true, debris: true }, [['erupt', { at: 0.15, kind: 'dark', to: 'center', area: true, count: 5, height: 2 }]], { shake: 0.3 }),
    'Darkness Zone': S('dark', [['vortex', { size: 1.8, dur: 0.8 }], ['portal', { at: 0.1, size: 1.2 }]], { charge: 'sky', shake: 0.26 }),
    'Huge Crystal': orb('ice', { head: 'shard', trail: 'ice', size: 0.4, from: 'sky', arc: 0.8 }, { charge: 'sky', colors: ['#a0e8ff', '#ffffff'] }),
    'Volcanic Flare': S('fire', [['erupt', { kind: 'flame', height: 2.2 }], ['meteor', { at: 0.1, size: 0.3, count: 2 }]], { charge: 'sky', shake: 0.18 }),
    'Fossil Bite': S('water', [['slash', { count: 2, claw: true, size: 1.3 }]], { charge: 'mouth', colors: ['#8ab0c0', '#e0f0ff'] }),
    'Darkness Claw': dash('dark', [['slash', { at: 0.15, count: 3, claw: true, size: 1.6 }], ['vortex', { at: 0.25, size: 1 }]], { ghosts: 7 }, { shake: 0.15 }),
    'Harpoon Torpedo': dome('water', [['missiles', { at: 0.05, count: 1, size: 0.2, curve: 0.3, arc: 0.3, dur: 0.4, impact: 'splash' }]], {}),
    "Vulcan's Hammer": dome('metal', [['shock', { at: 0.15, hammer: true, cell: 'spark' }]], {}, { shake: 0.2 }),
    'Smile Warhead': S('fire', [['meteor', { size: 0.5, count: 3, height: 7 }]], { charge: 'sky', shake: 0.22 }),
    'Hurricane Gale': heal('wind', { cell: 'feather' }, [['tornado', { at: 0.05, to: 'self', height: 2 }]]),
    'Snow Punch': orb('ice', { head: 'snow', trail: 'ice', arc: 0.6 }, { charge: 'hands' }),
    'Tusk Crusher': S('ice', [['streak', { width: 0.7, dur: 0.25, trail: 'ice' }], ['erupt', { at: 0.15, kind: 'ice', height: 1.4 }]], { charge: 'ground', shake: 0.2 }),
    'Crystal Cloud': orb('ice', { head: 'shard', trail: 'ice', size: 0.22, speed: 14 }),
    'Ice Wolf Claw': S('ice', [['streak', { width: 0.6, dur: 0.22, trail: 'ice' }], ['slash', { at: 0.08, count: 3, claw: true, size: 1.4, spread: 0.5 }]], { charge: 'hands', shake: 0.12 }),
    'Death Hang': S('holy', [['portal', { size: 1.1, dur: 0.6 }], ['glyph', { at: 0.05, glyph: 'circle', size: 1.4, dur: 0.7, place: 'ground' }]], { charge: 'sky', shake: 0.12 }),
    'Plasma Shoot': S('holy', [['sphere', { size: 0.5, grow: 0.3, from: 'hands', trail: 'holy', impactSize: 1.5 }]], { charge: 'hands', shake: 0.16, colors: ['#ffd84a', '#ffffff'] }),
    'Ice Prism': orb('ice', { head: 'shard', trail: 'ice', size: 0.24 }),
    'Tidal Wave': orb('water', { head: 'bubble', trail: 'bubble', size: 0.3 }, { shake: 0.06 }),
    'Gate of Atlantis': orb('water', { head: 'drop', trail: 'bubble', size: 0.32, speed: 14 }, { colors: ['#2a6aff', '#c0f0ff'] }),
    'Demi Darts': dome('dark', [['orb', { at: 0.05, head: 'needle', size: 0.2, speed: 16 }]], {}),
    'Terra Force Zero': dash('fire', [['sphere', { at: 0.15, size: 0.7, grow: 0.3, from: 'above', trail: 'flame', impactSize: 1.8 }]], {}, { shake: 0.22 }),
    'Phantom Pain': S('dark', [['vortex', { to: 'center', size: 1.4 }], ['swarm', { at: 0.1, cell: 'bat', count: 8, size: 0.3 }]], { charge: 'sky', shake: 0.18 }),
    'Party Time': shock('beast', { cell: 'note', debris: false }, [], { colors: ['#b05ad0', '#ffd0ff'] }),
    'Platinum Burp': shock('metal', { cell: 'smoke', debris: false }, [['vortex', { at: 0.15, size: 1.2 }]], { shake: 0.12, colors: ['#c0c8d8', '#ffffff'] }),
    'Dragon Wheel': S('fire', [['wheel', { size: 0.9, cell: 'flame', dur: 0.45 }]], { charge: 'hands', colors: ['#3a8aff', '#d0e8ff'] }),
    'Flashy Boss Punch': shock('beast', { hammer: true, debris: true }, [['streak', { width: 0.7, dur: 0.22 }]], { shake: 0.28, colors: ['#ffd06a', '#ffffff'] }),
    'Venom Infusion': S('dark', [['breath', { dur: 0.55, width: 0.5, cell: 'smoke', size: 0.5, speed: 8 }]], { charge: 'mouth', shake: 0.14, colors: ['#a020ff', '#ff3a7a'] }),
    'Crystal Revolution': S('ice', [['erupt', { kind: 'ice', height: 2 }], ['vortex', { at: 0.15, size: 1.2 }]], { charge: 'sky', shake: 0.2, colors: ['#ff3a5a', '#d0e8ff'] }),
    'Warning Beam': beam('metal', { width: 0.14, dur: 0.28 }, { colors: ['#ff3a3a', '#ffe0e0'] }),
    'Homing Laser': S('metal', [['missiles', { count: 2, size: 0.12, curve: 1, arc: 0.8, dur: 0.45 }]], { charge: 'hands' }),
    'Destruction Grenade': orb('metal', { arc: 0.7, size: 0.28, trail: 'smoke' }, { shake: 0.1 }),
    'Power Drive': S('beast', [['streak', { width: 0.6, dur: 0.2 }], ['shock', { at: 0.15 }]], { charge: 'none', shake: 0.12 }),
  };

  // ---------- basic attacks ----------
  const shot = (elem, head, trail, size = 0.22, o = {}) => ({ kind: 'shot', elem, head, trail, size, ...o });
  const basic = {
    giz_meramon: shot('fire', 'flame', 'flame'),
    giz_commandramon: shot('metal', 'streak', null, 0.2, { speed: 22 }),
    giz_zubamon: { kind: 'slash', elem: 'metal' },
    giz_goblimon: { kind: 'punch', elem: 'beast' },
    giz_toyagumon: shot('fire', 'square', 'flame'),
    giz_syakomon: { kind: 'bite', elem: 'water' },
    giz_kotemon: { kind: 'slash', elem: 'fire' },
    giz_tsukaimon: { kind: 'claw', elem: 'dark' },
    giz_bakemon: { kind: 'claw', elem: 'dark' },
    giz_kamemon: { kind: 'punch', elem: 'water' },
    giz_mushroomon: { kind: 'punch', elem: 'plant' },
    giz_chuumon: shot('beast', 'square', 'smoke', 0.2, { arc: 0.4 }),
    giz_mamemon: shot('fire', 'dot', 'smoke', 0.22, { arc: 0.5 }),
    giz_seadramon: shot('ice', 'shard', 'ice'),
    giz_kokuwamon: { kind: 'claw', elem: 'thunder' },
    giz_terriermon: shot('wind', 'ring', 'wind', 0.2),
    giz_kazemon: shot('holy', 'star', 'holy', 0.2),
    giz_kuwagamon: { kind: 'claw', elem: 'beast' },
    giz_kumamon: { kind: 'punch', elem: 'ice' },
    giz_falcomon: shot('wind', 'feather', 'wind', 0.24),
    giz_sorcermon: shot('fire', 'flame', 'flame', 0.2),
    giz_dracmon: { kind: 'claw', elem: 'dark' },
    giz_blackgabumon: { kind: 'bite', elem: 'dark' },
    giz_solarmon: shot('holy', 'gear', 'spark', 0.22, { spin: true }),
    giz_woodmon: shot('plant', 'leaf', 'leaf'),
    giz_hawkmon: shot('wind', 'feather', 'wind'),
    giz_airdramon: { kind: 'bite', elem: 'wind' },
    giz_gumdramon: { kind: 'tail', elem: 'fire' },
    giz_gizamon: { kind: 'slash', elem: 'water' },
    giz_bearmon: { kind: 'punch', elem: 'beast' },
    giz_coronamon: { kind: 'punch', elem: 'fire' },
    giz_starmon: shot('holy', 'star', 'spark', 0.2, { speed: 20 }),
    giz_vegiemon: shot('plant', 'seed', 'leaf', 0.2),
    giz_penguinmon: shot('water', 'drop', 'bubble'),
    giz_demidevimon: shot('dark', 'wisp', 'dark', 0.2),
    giz_salamon: shot('holy', 'cross', 'holy', 0.22),
    giz_blackagumon: { kind: 'claw', elem: 'fire' },
    giz_impmon: shot('fire', 'flame', 'flame', 0.24),
    giz_numemon: { kind: 'punch', elem: 'plant' },
    giz_youkomon: shot('fire', 'wisp', 'flame', 0.24),
    giz_tyrannomon: { kind: 'bite', elem: 'fire' },
    giz_psychemon: shot('metal', 'streak', 'spark', 0.2, { speed: 22 }),
    giz_shoutmon: { kind: 'slash', elem: 'fire' },
    giz_lekismon: shot('ice', 'needle', 'ice', 0.26, { speed: 22 }),
    giz_kunemon: { kind: 'sting', elem: 'plant' },
    giz_hagurumon: shot('metal', 'gear', 'spark', 0.24, { spin: true }),
    giz_floramon: shot('plant', 'petal', 'petal'),
    giz_lalamon: shot('plant', 'seed', 'leaf'),
    giz_dorumon: shot('metal', 'blade', 'spark', 0.24, { spin: true }),
    giz_sistermonb: { kind: 'slash', elem: 'holy' },
    giz_gotsumon: { kind: 'punch', elem: 'beast' },
    giz_justimon: { kind: 'punch', elem: 'thunder' },
    giz_sistermonn: shot('metal', 'streak', 'smoke', 0.22, { speed: 22 }),
    giz_keramon: shot('dark', 'wisp', 'dark', 0.22),
    giz_myotismon: shot('dark', 'bat', 'dark', 0.26),
    giz_gekomon: { kind: 'punch', elem: 'water' },
    giz_datamon: shot('metal', 'square', 'data', 0.22),
    giz_renamon: shot('thunder', 'shard', 'zap', 0.22, { speed: 22 }),
    giz_lucemon: shot('holy', 'star', 'holy', 0.24),
    giz_arukenimon: { kind: 'claw', elem: 'dark' },
    giz_inno: { kind: 'punch', elem: 'metal' },
    giz_decoy: { kind: 'bite', elem: 'dark' },
    giz_w_guardromon: { kind: 'claw', elem: 'metal' },
    giz_w_nanimon: { kind: 'punch', elem: 'beast' },
    giz_w_gigadramon: { kind: 'claw', elem: 'metal' },
    giz_w_belphemon: { kind: 'claw', elem: 'dark' },
    giz_w_apocalymon: shot('dark', 'wisp', 'dark', 0.3),
    giz_w_ygg: shot('ice', 'shard', 'ice', 0.3),
  };

  // ---------- item portraits ----------
  const C = {};
  for (const [id, x] of Object.entries(def.items.components)) C[id] = x.color;
  const motif = {
    giz_sword: (p, k, col) => { // power cell
      p.part(k.rr(-15, -30, 30, 56, 8), [[0, k.lt(col, 0.5)], [0.5, col], [1, k.dk(col, 0.5)]]);
      p.part(k.rr(-7, -38, 14, 9, 3), 'chrome', { ow: 4 });
      p.at(0, -2, 1, 0, () => p.part(k.BOLT, '#fff3b0', { ow: 3.5 }));
    },
    giz_bow: (p, k, col) => { // turbo gear
      p.part(k.star(0, 0, 34, 25, 10), col);
      p.part(k.circ(0, 0, 14), k.dk(col, 0.35), { ow: 4 });
      p.part(k.circ(0, 0, 6), 'chrome', { ow: 3 });
    },
    giz_rod: (p, k, col) => { // code rod
      p.at(0, 0, 1, 30, () => { p.tube('M0 34V-16', 9, k.dk(col, 0.2)); p.gem(0, -24, 13, col, 'hex', 4); });
    },
    giz_tear: (p, k, col) => { // mana capacitor
      p.part(k.rr(-18, -30, 36, 60, 14), 'glass', { ow: 5 });
      p.at(0, 4, 1.25, 0, () => p.part(k.DROP, col, { ow: 3.5 }));
      for (const y of [-34, 34]) p.part(k.rr(-20, y - 4, 40, 8, 3), 'chrome', { ow: 4 });
    },
    giz_vest: (p, k) => k.M.plate(p),
    giz_cloak: (p, k, col) => k.M.hood(p, [[0, k.lt(col, 0.3)], [1, k.dk(col, 0.5)]], '#a0ffff'),
    giz_belt: (p, k, col) => { // battery pack
      p.part(k.rr(-32, -22, 64, 44, 8), [[0, k.lt(col, 0.4)], [0.5, col], [1, k.dk(col, 0.5)]]);
      p.part(k.rr(32, -9, 7, 18, 3), 'chrome', { ow: 4 });
      p.part('M-6 -16H6V-6H16V6H6V16H-6V6H-16V-6H-6Z', '#ffffff', { ow: 3.5 });
    },
    giz_glove: (p, k, col) => k.M.glove(p, col),
    giz_chip: (p, k, col) => k.M.chip(p, col),
  };
  const compPainter = (id) => (p, k) => {
    k.frame(p, 'circle', 'steel', [C[id]], { w: 7 });
    p.obj(() => p.at(64, 64, 1, 0, () => motif[id](p, k, C[id])));
  };
  // completed items: the two component motifs and a center symbol for the item
  const SYM_OF = {
    doom_blade: 'sword', titan_breaker: 'chevrons', gun_blade: 'blood', mana_spear: 'drop', backup_drive: 'up', blood_fang: 'blood', war_banner: 'chevrons',
    crit_edge: 'dagger', rail_cannon: 'crosshair', rage_engine: 'refresh', static_shiv: 'star5', titan_core: 'plus', twin_turbine: 'copy', decoy_portal: 'bug',
    armor_piercer: 'dagger', mega_rod: 'star6', seraph_staff: 'cross', guard_locket: 'shield', ion_spark: 'star5', burn_codex: 'flame', jewel_gauntlet: 'star6',
    mana_overflow: 'drop', frost_heart: 'snow', power_chalice: 'pearl', redemption_core: 'plus', justice_glove: 'badge', thorn_mail: 'shieldIron',
    gargoyle_plate: 'skull', solar_cape: 'sun', static_shroud: 'moon', dragon_scale: 'shield', wind_turbine: 'refresh', quick_silver: 'up', giga_battery: 'plus',
    spell_shield: 'shieldIron', thief_glove: 'coin',
  };
  const itemPainter = (r, radiant) => (p, k) => {
    const [a, b] = [r[0], r[1]];
    const name = r[6].id.replace('giz_', '');
    k.frame(p, 'square', radiant ? 'prism' : 'chrome', [C[a], C[b]], { w: radiant ? 8 : 7 });
    p.obj(() => {
      p.at(38, 40, 0.48, -12, () => motif[a](p, k, C[a]));
      p.at(90, 88, 0.48, 12, () => motif[b](p, k, C[b]));
      p.at(64, 64, 1.25, 0, () => { p.glow(0, 0, 26, radiant ? '#ff9ae8' : '#ffffff', 0.45); k.SYM[SYM_OF[name] || 'star5'](p, k.mix(C[a], C[b], 0.5)); });
    });
    if (radiant) { p.sparkle(102, 26, 7, '#fff2ff'); p.sparkle(24, 100, 6, '#c0f8ff'); p.sparkle(96, 106, 4); }
  };

  const items = {};
  for (const id of Object.keys(def.items.components)) items[id] = compPainter(id);
  for (const r of def.items.recipes) {
    items[r[6].id] = itemPainter(r, false);
    items[r[6].id.replace('giz_', 'giz_oc_')] = itemPainter(r, true);
  }
  // Chips (emblems): a crest in the trait color with the trait symbol
  const emblemTraits = Object.values(def.items.emblems).concat(def.items.dropEmblems);
  for (const t of emblemTraits) {
    const T = def.traits[t];
    items[`giz_emblem_${t.toLowerCase()}`] = (p, k) => k.crest(p, T.color, T.crestSymbol || 'star6');
  }
  items.giz_golden_chip = (p, k) => {
    k.frame(p, 'circle', 'gold', ['#8a6420'], { w: 8 });
    p.obj(() => p.at(64, 64, 0.9, 0, () => { p.glow(0, 0, 40, '#ffd84a', 0.6); k.M.chip(p, '#ffd84a'); p.at(0, 0, 0.9, 0, () => k.SYM.plus(p, '#fff3b0')); }));
    p.sparkle(100, 28, 7); p.sparkle(28, 98, 5);
  };
  items.giz_remover = (p, k) => {
    k.frame(p, 'square', 'steel', ['#ff8fa3', '#8a8f9c'], { w: 7 });
    p.obj(() => p.at(64, 66, 1, 0, () => {
      p.tube('M-22 26V-8A22 22 0 0 1 22 -8V26', 16, '#e8455a', { cap: 'butt' });
      for (const x of [-22, 22]) p.part(k.rr(x - 9, 22, 18, 14, 2), 'chrome', { ow: 5 });
    }));
  };
  items.giz_reforger = (p, k) => {
    k.frame(p, 'square', 'steel', ['#ffb347', '#5a6170'], { w: 7 });
    p.obj(() => p.at(64, 64, 1, -30, () => { p.tube('M0 34V-8', 8, '#a8652e'); p.part(k.rr(-22, -30, 44, 22, 4), 'chrome', { ow: 5 }); p.glow(0, -18, 18, '#ffb347', 0.8); }));
    p.sparkle(96, 34, 6, '#ffd27a');
  };
  items.giz_copy = (p, k) => {
    k.frame(p, 'square', 'chrome', ['#7dffb0', '#2a6a4a'], { w: 7 });
    p.obj(() => p.at(64, 64, 1, 0, () => { p.at(-10, -6, 0.8, -10, () => k.M.chip(p, '#3fbf7a', 44)); p.at(12, 8, 0.8, 8, () => k.M.chip(p, '#7dffb0', 44)); }));
  };
  items.giz_ockit = (p, k) => {
    k.frame(p, 'square', 'prism', ['#e58bff', '#4fd0ff'], { w: 8 });
    p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 40, '#e58bff', 0.6); k.M.chip(p, '#c07aff', 52); p.at(0, 0, 0.9, 0, () => k.SYM.up(p, '#fff2ff')); }));
    p.sparkle(100, 26, 7); p.sparkle(26, 100, 5, '#c0f8ff');
  };

  // Digivice upgrades: hexagon frame in the tier metal, a Digivice with a symbol for the upgrade
  const AUG_FRAME = ['', 'silver', 'gold', 'prism'];
  const AUG_BG = ['', '#5a6b84', '#8a6420', '#6a3a9a'];
  const TAG_SCREEN = { econ: '#ffe27a', items: '#7dd0ff', combat: '#ff8a7a' };
  const AUG_SYM = {
    spare: 'coin', parts: 'chip', feather1: 'chevrons', bless1: 'blood', implant1: 'chip', thrill1: 'blood', knife1: 'sword', makeshift1: 'shieldIron', exiles1: 'shield',
    united1: 'plus', ascend: 'up', calc: 'refresh', hyper: 'coin', weak: 'crosshair', firstaid: 'plus', scoped: 'crosshair', underdog: 'up', booster: 'star6',
    runic: 'star6', overload: 'drop', sosmall: 'egg', cutthroat: 'dagger', bank: 'coin', chip: 'chip', crestkit: 'crestTag', airdrop: 'copy', bless2: 'blood',
    implant2: 'chip', feather2: 'chevrons', knife2: 'sword', makeshift2: 'shieldIron', united2: 'plus', thrill2: 'blood', trade: 'refresh', salvage: 'chip',
    shrug: 'plus', spellblade: 'star5', stopwatch: 'refresh', smoke: 'moon', injection: 'drop', reserves: 'coin', junkyard: 'sludge', titanic: 'chevrons',
    censer: 'pearl', jackpot: 'coin', grabbag: 'copy', recruit: 'up', bless3: 'blood', implant3: 'chip', feather3: 'chevrons', highend: 'star5', ticket: 'die',
    woodland: 'copy', windfall: 'coin', forge: 'up', burst: 'star6', united3: 'plus', band: 'dagger',
  };
  for (const a of def.augments) {
    const key = a.id.replace('giz_a_', '');
    const screen = TAG_SCREEN[(a.tags || [])[0]] || '#7dffb0';
    items[`aug:${a.id}`] = (p, k) => {
      k.frame(p, 'hex', AUG_FRAME[a.tier], [AUG_BG[a.tier]], { w: 7 });
      p.obj(() => p.at(60, 66, 0.82, -8, () => k.M.digivice(p, '#eef1f6', a.tier === 3 ? '#c07aff' : a.tier === 2 ? '#ffb02e' : '#8aa0c0', screen)));
      p.obj(() => p.at(88, 88, 1.05, 0, () => { p.glow(0, 0, 20, screen, 0.55); k.SYM[AUG_SYM[key] || 'star5'](p, screen); }));
    };
  }

  DT.Sets.extend('gizmos', { art: { items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
