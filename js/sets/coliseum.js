// K.O. Coliseum (TFT Set 15 "K.O. Coliseum", docs/inspo/tft/set-15-ko-coliseum.md) adapted to Digimon.
// 60 shop lines (13/13/13/11/10), 26 traits, a TFT component item system with two special components (Digi-Egg
// for origin Crests, Crest Tag for class Crests), 36 recipes, 18 Crests, 3 team-size items, 8 Artifacts,
// 4 consumables and 30 augments, on Hyper Roll rules.
// Set mechanic: Digi-Cores (TFT Power Ups as branching digivolution). At 1-3 and 3-3 every Tamer gets a Digi-Core;
// feed it to a Digimon and pick 1 of 3 digivolution paths for it. Selling the Digimon returns the Digi-Core.
// Stance Master (Justimon) and Monster Tamer (Labramon) pick a form when bought. Jewel Gambit (Crystal Gambit) banks
// Gem Power and offers a cash-out or Double Down every 3 player combats.
// All behavior (traits, items, augments, mechanic, ability types) is in coliseum-mech.js; art in coliseum-art.js.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;
  const L = DT.line;

  // ---------- traits ----------
  // lv[i] is the main value at breakpoint i; extra arrays are read with c.val(key) in coliseum-mech.js.
  const TRAITS = {
    // ---- origins ----
    BA: { name: 'Digi Academy', kind: 'origin', color: '#ff8fb8', bp: [3, 5, 7], lv: [3, 5, 7], crestSymbol: 'star5',
      desc: 'Academy students train their Potential. Each point of Potential makes their abilities deal 9% more damage and gives 50 max HP.',
      fmt: (v) => `${v} Potential` },
    CG: { name: 'Jewel Gambit', kind: 'origin', color: '#5ff0e0', bp: [3, 5, 7], lv: [1, 1, 1.3], hp: [80, 150, 280], amp: [0.04, 0.09, 0.18], crestSymbol: 'pearl',
      desc: 'Jewel Gambit Digimon gain max HP and deal more damage. Kills and losses in player combats earn Gem Power. Every 3 player combats, cash the gems out for gold and items, or Double Down: losses then earn twice as much, but a win halves your Gem Power and cashes out at once.',
      notes: 'Gem Power per kill: 1 in stages 1-2, 1.5 in stage 3, 2 from stage 4. Per loss: 4, 6, 8. Cash-out: 50% of Gem Power as gold, a component at 25, a completed item at 45, two at 80.' },
    MEN: { name: 'Sensei', kind: 'origin', color: '#e8c070', bp: [1, 4], lv: [1, 2], crestSymbol: 'chess',
      desc: 'Active only with exactly 1 or 4 different Sensei. With 1, your whole team gains that Sensei\'s lesson. With 4, the Sensei gain every lesson and 10 ability power. Lessons: Jijimon line 6% less damage taken, Kentaurosmon line 8% attack damage and ability power, Grademon line 10% attack speed, Piximon line 2 extra mana per attack.',
      notes: '2 or 3 Sensei do nothing. No Crest exists for this trait.' },
    MECH: { name: 'Mega Mech', kind: 'origin', color: '#9ab0c8', bp: [3, 5, 7], lv: [1, 2, 3], crestSymbol: 'chip',
      desc: 'Machinedramon joins the fight in your front row. Its HP and attack grow with the star levels of your Mega Mech Digimon, and they heal it for 12% of the damage they deal. With 5 and 7 it gets Blade Protocol upgrades.',
      notes: 'No Crest exists for this trait.' },
    MT: { name: 'Monster Tamer', kind: 'origin', color: '#ffb347', bp: [1], lv: [1], crestSymbol: 'egg',
      desc: 'When you get Labramon, pick which partner it fights alongside as: Ballistamon (tank), Dorulumon (diver) or Sparrowmon (ranged attacker).' },
    RC: { name: 'Pirate Captain', kind: 'origin', color: '#e05a5a', bp: [1], lv: [1], crestSymbol: 'skull',
      desc: 'While Piedmon is on your board, you draw a Bounty card at the start of each round: gold, rerolls, components, ability power for Piedmon or a shield for your team. Bounties get better the longer Piedmon stays fielded. Street Crew allies deal 12% of their damage again as true damage.' },
    RM: { name: 'Rose Queen', kind: 'origin', color: '#ff5f9e', bp: [1], lv: [1], crestSymbol: 'flame',
      desc: 'Lotosmon brings two plants: a Woodmon root in front of her and a Togemon spine behind her (four at three stars). When Lotosmon casts, her plants heal 35% HP and gain 35% attack speed; dead plants grow back.' },
    SF: { name: 'Spirit Fighters', kind: 'origin', color: '#ffcf4a', bp: [2, 4, 6], lv: [150, 300, 500], sps: [0.025, 0.04, 0.06], td: [0.12, 0.18, 0.25], crestSymbol: 'sun',
      desc: 'Spirit Fighters gain max HP, and every second gain attack damage and ability power, up to 8 times. At 8 stacks they deal extra true damage.',
      fmt: (v, i) => `+${v} HP, +${pct(TRAITS.SF.sps[i])} AD/AP per second, ${pct(TRAITS.SF.td[i])} true damage at 8 stacks` },
    SM: { name: 'Stance Master', kind: 'origin', color: '#ff7a45', bp: [1], lv: [1], crestSymbol: 'dagger',
      desc: 'When you get Justimon, pick an arm: Blitz Arm (Duelist), Critical Arm (Executioner) or Trinity Arm (Juggernaut). The arm changes its ability and gives it that trait.' },
    SG: { name: 'Star Wards', kind: 'origin', color: '#b88cff', bp: [2, 3, 4, 5, 6, 7, 8], lv: [1, 1.05, 1.1, 1.12, 1.14, 1.16, 1.2], crestSymbol: 'star6',
      desc: 'Every Star Ward line on your board adds its own bonus for all Star Wards, and more Star Wards make every bonus stronger. Starmon line: 150 shield. Sorcermon line: 4 AP every 3 seconds. Aquilamon line: every 3rd attack deals bonus magic damage. Taomon line: 15 mana after casting. Sunflowmon line: 10% stronger healing and shields for your team. Monzaemon line: heal 25% HP once at 40% HP. Rapidmon line: 12% attack speed. Ophanimon line: 25 armor and MR, 100 HP and 8% AD, AS and crit.',
      fmt: (v) => `bonuses x${v}` },
    SC: { name: 'Supreme Virus', kind: 'origin', color: '#7aff5a', bp: [2, 3, 4], lv: [0.1, 0.24, 0.3], sup: [0.12, 0.32, 0.5], crestSymbol: 'bug',
      desc: 'The Supreme Virus Digimon that dealt the most damage in your last combat is Supreme. Supreme Virus Digimon deal more damage; the Supreme one deals even more and finishes off enemies below 10% HP. When it falls, the one with the most damage so far takes over. With 4, two are Supreme.',
      fmt: (v, i) => `+${pct(v)} damage, Supreme +${pct(TRAITS.SC.sup[i])}` },
    CH: { name: 'The Champ', kind: 'origin', color: '#ffd84a', bp: [1], lv: [1], crestSymbol: 'crestTag',
      desc: 'Each player combat Craniamon wins brings fans equal to its star level. When you lose a player combat, each fan blocks 1 damage to you, then all fans leave.' },
    CREW: { name: 'Street Crew', kind: 'origin', color: '#6ac8ff', bp: [2, 3, 4, 5], lv: [0.04, 0.08, 0.1, 0.13], crestSymbol: 'coin',
      desc: 'Street Crew Digimon gain max HP and attack speed. Every 400 damage they deal launches a rocket at a random enemy. Each three-star Street Crew Digimon gives you a free reroll every round.',
      fmt: (v) => `+${pct(v)} HP and AS`, notes: 'Rocket damage: 50 + 25 per stage. No Crest exists for this trait.' },
    WR: { name: 'Phantoms', kind: 'origin', color: '#8a6aff', bp: [2, 4, 6], lv: [0.27, 0.33, 0.48], crestSymbol: 'moon',
      desc: 'Every 4 seconds the Shadow Realm strikes the 3 enemies nearest your Phantoms for a share of the damage your Phantoms dealt since the last strike. Your lowest-health Phantom heals for 18% of that damage.',
      fmt: (v) => `${pct(v)} of Phantom damage` },
    LU: { name: 'Luchadors', kind: 'origin', color: '#ff6a3d', bp: [2, 4], lv: [0.1, 0.35], heal: [0.15, 0.35], crestSymbol: 'flame',
      desc: 'Luchadors gain attack damage. The first time one drops below 50% HP, it shakes off stuns, heals and slams down, stunning adjacent enemies for 1 second.',
      fmt: (v, i) => `+${pct(v)} AD, heal ${pct(TRAITS.LU.heal[i])}` },
    // ---- classes ----
    BAS: { name: 'Bastion', kind: 'class', color: '#c9a66b', bp: [2, 4, 6], lv: [10, 22, 40], crestSymbol: 'shield',
      desc: 'Your team gains 6 armor and MR. Bastions gain more, doubled for the first 10 seconds of combat. With 6, the rest of your team gains 25 more.',
      fmt: (v) => `Bastions +${v} armor/MR` },
    DUE: { name: 'Duelist', kind: 'class', color: '#f4d35e', bp: [2, 4, 6], lv: [0.08, 0.14, 0.18], crestSymbol: 'sword',
      desc: 'Each basic attack gives a Duelist attack speed, stacking up to 12 times. With 6, Duelists also take 12% less damage.',
      fmt: (v) => `+${pct(v)} AS per attack` },
    EDG: { name: 'Swordmaster', kind: 'class', color: '#ff5a7a', bp: [2, 4, 6], lv: [0.22, 0.35, 0.6], vamp: [0.1, 0.12, 0.15], crestSymbol: 'dagger',
      desc: 'Swordmasters gain attack damage and omnivamp. While attacking an enemy below 50% HP they gain 40% attack speed.',
      fmt: (v, i) => `+${pct(v)} AD, ${pct(TRAITS.EDG.vamp[i])} omnivamp` },
    EXE: { name: 'Executioner', kind: 'class', color: '#d84a4a', bp: [2, 3, 4, 5], lv: [0.35, 0.5, 0.55, 0.6], cdmg: [0.15, 0.22, 0.25, 0.3], crestSymbol: 'crosshair',
      desc: 'Executioners gain critical strike chance and critical damage, and their abilities can critically strike.',
      fmt: (v, i) => `+${pct(v)} crit, +${pct(TRAITS.EXE.cdmg[i])} crit damage` },
    HEV: { name: 'Heavyweight', kind: 'class', color: '#d9773a', bp: [2, 4, 6], lv: [0.12, 0.25, 0.4], k: [0.012, 0.02, 0.03], crestSymbol: 'shieldIron',
      desc: 'Your team gains 60 HP. Heavyweights gain more max HP and attack damage equal to a share of their max HP.',
      fmt: (v, i) => `+${pct(v)} max HP, AD +${pct(TRAITS.HEV.k[i])} of max HP` },
    JUG: { name: 'Juggernaut', kind: 'class', color: '#a07a5a', bp: [2, 4, 6], lv: [0.08, 0.1, 0.13], crestSymbol: 'shield',
      desc: 'Juggernauts take less damage, 8% less again while above 50% HP. When a Juggernaut falls, the others heal 10% of their max HP.',
      fmt: (v) => `-${pct(v)} damage taken (-${pct(v + 0.08)} above 50% HP)` },
    PRO: { name: 'Prodigy', kind: 'class', color: '#6ab4ff', bp: [2, 3, 4, 5], lv: [5, 7, 9, 11], crestSymbol: 'drop',
      desc: 'Your team gains 1 mana every second. Prodigies gain more. With 5, damage from Prodigy abilities heals your lowest-health ally for 12% of it.',
      fmt: (v) => `Prodigies +${v} mana per second` },
    PRT: { name: 'Protector', kind: 'class', color: '#7fd0c0', bp: [2, 4, 6], lv: [0.11, 0.19, 0.3], crestSymbol: 'shield',
      desc: 'Protectors take 5% less damage while shielded. The first time one drops below 50% HP, it shields itself and its nearest ally for a share of its max HP.',
      fmt: (v) => `${pct(v)} max HP shield` },
    SNI: { name: 'Sniper', kind: 'class', color: '#5ad1ff', bp: [2, 3, 4, 5], lv: [0.15, 0.2, 0.26, 0.24], hex: [0.03, 0.06, 0.07, 0.08], crestSymbol: 'crosshair',
      desc: 'Snipers deal more damage, and more again for each hex between them and their target.',
      fmt: (v, i) => `+${pct(v)} damage, +${pct(TRAITS.SNI.hex[i])} per hex` },
    SOR: { name: 'Sorcerer', kind: 'class', color: '#d48bff', bp: [2, 4, 6], lv: [18, 38, 65], burst: [0.08, 0.1, 0.12], crestSymbol: 'star6',
      desc: 'Sorcerers gain ability power. When an enemy a Sorcerer damaged falls, another enemy takes magic damage equal to a share of its max HP (two enemies with 6).',
      fmt: (v, i) => `+${v} AP, ${pct(TRAITS.SOR.burst[i])} max HP burst` },
    STR: { name: 'Strategist', kind: 'class', color: '#9cf6c0', bp: [2, 3, 4, 5], lv: [120, 160, 250, 340], amp: [0.03, 0.05, 0.08, 0.12], crestSymbol: 'chess',
      desc: 'At the start of combat, allies in your front two rows gain a shield and allies in your back two rows deal more damage. Strategists gain triple.',
      fmt: (v, i) => `front ${v} shield, back +${pct(TRAITS.STR.amp[i])} damage` },
  };
  {
    const each = (id, f) => { TRAITS[id].bpDesc = TRAITS[id].lv.map((v, i) => f(v, i, TRAITS[id])); };
    each('BA', (v) => `${v} Potential: abilities deal ${pct(v * 0.09)} more damage, +${v * 50} max HP`);
    each('CG', (v, i, T) => `+${T.hp[i]} HP, +${pct(T.amp[i])} damage. ` + ['Earn Gem Power from kills and losses', 'Also: every 8 kills, gain a component (the count then rolls 6-10)', 'Also: every 8 kills a component; cash-outs are worth 30% more'][i]);
    each('MEN', (v, i) => ['Exactly 1 Sensei: your whole team gains its lesson', 'Exactly 4 Sensei: the Sensei gain every lesson and 10 ability power'][i]);
    each('MECH', (v, i) => ['Machinedramon joins the fight', 'Blade Protocol 1: Machinedramon gains 15% attack damage and its attacks also hit a second enemy for 30%', 'Blade Protocol 2: also 25% omnivamp and 20% less damage taken'][i]);
    each('MT', () => 'Pick a partner for Labramon');
    each('RC', () => 'Draw a Bounty card every round; Street Crew deal 12% extra true damage');
    each('RM', () => 'Lotosmon brings 2 plants (4 at three stars)');
    each('SM', () => 'Pick an arm for Justimon');
    each('CH', () => 'Wins bring fans; fans block damage on a loss');
    each('SG', (v) => `Each Star Ward bonus x${v}`);
    each('SC', (v, i, T) => `+${pct(v)} damage; the Supreme deals +${pct(T.sup[i])} and finishes enemies below 10% HP${i === 2 ? '; two Supremes' : ''}`);
    each('CREW', (v) => `+${pct(v)} max HP and attack speed; rockets every 400 damage`);
    each('WR', (v) => `The Shadow Realm deals ${pct(v)} of Phantom damage every 4 seconds`);
    each('BAS', (v, i) => `Bastions +${v} armor and MR (double for 10 seconds)${i === 2 ? '; others +25 more' : ''}`);
    each('DUE', (v, i) => `+${pct(v)} attack speed per attack (12 stacks)${i === 2 ? '; 12% less damage taken' : ''}`);
    each('PRO', (v, i) => `Prodigies +${v} mana per second${i === 3 ? '; their ability damage heals an ally for 12%' : ''}`);
  }

  // ---------- stat templates ----------
  // Engine templates (js/data.js) with a sturdier back line and a slightly softer front line: this set's traits give
  // the front line more armor, HP and damage reduction than Faction Wars does.
  const ROLES = {
    Guardian: { hp: 690, ad: 45, as: 0.6, range: 1, armor: 40, mr: 40, mana: 90, startMana: 30 },
    Bruiser: { hp: 720, ad: 50, as: 0.6, range: 1, armor: 30, mr: 30, mana: 80, startMana: 20 },
    Duelist: { hp: 680, ad: 60, as: 0.78, range: 1, armor: 35, mr: 35, mana: 70, startMana: 0 },
    Sniper: { hp: 530, ad: 58, as: 0.75, range: 4, armor: 20, mr: 20, mana: 70, startMana: 0 },
    Mystic: { hp: 540, ad: 40, as: 0.65, range: 3, armor: 20, mr: 25, mana: 55, startMana: 20 },
    Support: { hp: 560, ad: 35, as: 0.65, range: 3, armor: 20, mr: 25, mana: 70, startMana: 30 },
    Assassin: { hp: 540, ad: 70, as: 0.8, range: 1, armor: 25, mr: 25, mana: 60, startMana: 0, leap: true },
  };

  // ---------- lines ----------
  // src: the TFT Set 15 champion each line adapts. unassigned: placeholder models until models are picked.
  const U = { unassigned: true };
  const o = (src, extra = {}) => ({ ...U, src: `TFT15 ${src}`, ...extra });
  const RCU = ['Rookie', 'Champion', 'Ultimate'];
  const CUM = ['Champion', 'Ultimate', 'Mega'];
  const LINES = [
    // ---- 1-cost (13) ----
    L('col_hagurumon', 1, ['MECH', 'JUG', 'HEV'], 'Virus', ['Hagurumon', 'Guardromon', 'Andromon'], ['Cog Crusher', 'Destruction Grenade', 'Spiral Sword'], 'execute', o('Aatrox', { role: 'Bruiser' })),
    L('col_lunamon', 1, ['BA', 'PRO'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Lop-Ear Ripple', 'Tear Arrow', 'Ice Archery'], 'nuke', o('Ezreal', { role: 'Mystic' })),
    L('col_zubamon', 1, ['BA', 'BAS'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Duramon'], ['Twenty Dive', 'Road Two Decade', 'Glass Slash'], 'drain', o('Garen', { role: 'Bruiser' })),
    L('col_gabumon', 1, ['LU', 'SNI'], 'Data', ['Gabumon', 'Garurumon', 'WereGarurumon'], ['Little Horn', 'Fox Fire', 'Garuru Kick'], 'col_frenzy', o('Gnar', { role: 'Sniper' })),
    L('col_lobomon', 1, ['SF', 'EXE'], 'Vaccine', ['Lobomon', 'KendoGarurumon', 'MagnaGarurumon'], ['Licht Sieger', 'Solar Laser', 'Machinegun Destroyer'], 'line', o('Kalista', { role: 'Sniper', hpMul: 1.1, adMul: 1.15, levels: ['Human Spirit', 'Beast Spirit', 'Fused Spirit'] })),
    L('col_tsukaimon', 1, ['WR', 'DUE'], 'Virus', ['Tsukaimon', 'Devimon', 'SkullSatamon'], ['Bad Message', 'Death Claw', 'Nail Bone'], 'barrage', o('Kayle', { role: 'Sniper', hpMul: 1.1, adMul: 1.1 })),
    L('col_elecmon', 1, ['SC', 'PRT', 'SOR'], 'Data', ['Elecmon', 'Aegiomon', 'IceLeomon'], ['Sparkling Thunder', 'Stun Beat Blow', 'Fist of Ice'], 'slam', o('Kennen', { role: 'Bruiser', hpMul: 1.55, adMul: 1.35 })),
    L('col_kokuwamon', 1, ['MECH', 'SOR'], 'Data', ['Kokuwamon', 'Tankmon', 'Datamon'], ['Electro Strike', 'Hyper Cannon', 'Digital Bomb'], 'barrage', o('Lucian', { role: 'Mystic' })),
    L('col_armadillomon', 1, ['CREW', 'PRT'], 'Free', ['Armadillomon', 'Ankylomon', 'Shakkoumon'], ['Diamond Shell', 'Tail Hammer', 'Disaster Blaster'], 'slam', o('Malphite', { role: 'Guardian', hpMul: 1.15 })),
    L('col_gotsumon', 1, ['SG', 'BAS'], 'Data', ['Gotsumon', 'Starmon', 'SuperStarmon'], ['Rock Fist', 'Meteor Shower', 'Haley Storm'], 'col_aegis', o('Rell', { role: 'Guardian', hpMul: 1.1 })),
    L('col_betamon', 1, ['CREW', 'SNI'], 'Virus', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Blast', 'Lightning Javelin'], 'chain', o('Sivir', { role: 'Sniper' })),
    L('col_syakomon', 1, ['CG', 'SG', 'PRO'], 'Virus', ['Syakomon', 'Sorcermon', 'Meteormon'], ['Black Pearl Blast', 'Crystal Barrage', 'Galactic Flare'], 'nuke', o('Syndra', { role: 'Mystic', hpMul: 1.5, adMul: 1.35 })),
    L('col_numemon', 1, ['WR', 'HEV'], 'Virus', ['Numemon', 'Raremon', 'BlackKingNumemon'], ['Party Time', 'Sludge', 'Dark Pupil'], 'drain', o('Zac', { role: 'Bruiser', levels: ['Champion', 'Champion', 'Ultimate'] })),
    // ---- 2-cost (13) ----
    L('col_goblimon', 2, ['LU', 'JUG'], 'Virus', ['Goblimon', 'Ogremon', 'Fugamon'], ['Goblin Strike', 'Pummel Whack', 'Evil Hurricane'], 'col_bulk', o('Dr. Mundo', { role: 'Bruiser', hpMul: 1.1 })),
    L('col_monodramon', 2, ['MECH', 'DUE'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Knuckle Beater', 'Fang Strike', 'Desolation Claw'], 'execute', o('Gangplank', { role: 'Duelist', hpMul: 1.15, adMul: 1.2 })),
    L('col_wormmon', 2, ['CG', 'PRT', 'STR'], 'Free', ['Wormmon', 'Hudiemon', 'Dinobeemon'], ['Sticky Net', 'Infinity Dream', 'Hell Masquerade'], 'rally', o('Janna', { role: 'Support', hpMul: 1.2, ai: { carry: false } })),
    L('col_demidevimon', 2, ['WR', 'SNI'], 'Virus', ['DemiDevimon', 'IceDevimon', 'LadyDevimon'], ['Demi Darts', 'Frost Claw', 'Darkness Wave'], 'nuke', o('Jhin', { role: 'Sniper', hpMul: 1.1, adMul: 1.15 })),
    L('col_fanbeemon', 2, ['SC', 'DUE'], 'Virus', ['FanBeemon', 'Waspmon', 'CannonBeemon'], ['Gear Stinger', 'Turbo Stinger', 'Sky Rocket'], 'barrage', o("Kai'Sa", { role: 'Sniper' })),
    L('col_falcomon', 2, ['BA', 'EXE'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Scratch Smash', 'Kunai Wing', 'Savage Emperor'], 'dash', o('Katarina', { role: 'Assassin', hpMul: 1.25, adMul: 1.2 })),
    L('col_kamemon', 2, ['MEN', 'HEV'], 'Data', ['Kamemon', 'Golemon', 'Jijimon'], ['Shell Guard', 'Golem Punch', 'Guard Stick'], 'shield', o('Kobuko', { role: 'Guardian', levels: ['Rookie', 'Champion', 'Mega'] })),
    L('col_patamon', 2, ['SF', 'SOR'], 'Data', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Air Shot', "Heaven's Knuckle", "Heaven's Gate"], 'aoe', o('Lux', { role: 'Mystic', hpMul: 1.2 })),
    L('col_biyomon', 2, ['BA', 'PRT'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'heal', o('Rakan', { role: 'Bruiser', hpMul: 1.2 })),
    L('col_lopmon', 2, ['CREW', 'BAS', 'EDG'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Ninja Fist', 'Asipatravana'], 'shield', o('Shen', { role: 'Guardian' })),
    L('col_gaomon', 2, ['CG', 'JUG'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Gaoga Hound', 'Winning Knuckle'], 'drain', o('Vi', { role: 'Bruiser', radius: 1, hpMul: 1.1, adMul: 1.1 })),
    L('col_hawkmon', 2, ['SG', 'EDG'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Top Gun'], 'barrage', o('Xayah', { role: 'Sniper', hpMul: 1.1, adMul: 1.1 })),
    L('col_ryudamon', 2, ['SF', 'BAS'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Kabutogaeshi', 'Metal Armor Blade', 'Legendary Dragon Blade'], 'aoe', o('Xin Zhao', { role: 'Duelist', hpMul: 1.1 })),
    // ---- 3-cost (13) ----
    L('col_kyubimon', 3, ['SG', 'SOR'], 'Data', ['Kyubimon', 'Taomon', 'Sakuyamon'], ['Fox Tail Inferno', 'Talisman of Light', 'Amethyst Mandala'], 'execute', o('Ahri', { role: 'Mystic', levels: CUM })),
    L('col_sistermonnoir', 3, ['BA', 'SNI'], 'Virus', ['Sistermon Noir', 'Sistermon Noir', 'SaviorHuckmon'], ['Mickey Bullet', 'Bless Fire', 'Meteor Flame'], 'nuke', o('Caitlyn', { role: 'Sniper', levels: ['Champion', 'Champion', 'Ultimate'] })),
    L('col_kuwagamon', 3, ['SC', 'HEV'], 'Virus', ['Kuwagamon', 'Okuwamon', 'GranKuwagamon'], ['Scissor Claw', 'Double Scissor Claw', 'Dimension Scissor'], 'execute', o('Darius', { role: 'Bruiser', levels: CUM })),
    L('col_ikkakumon', 3, ['BA', 'HEV'], 'Vaccine', ['Ikkakumon', 'Zudomon', 'Vikemon'], ['Harpoon Torpedo', "Vulcan's Hammer", 'Mjolnir'], 'aoe', o('Jayce', { role: 'Striker', levels: CUM })),
    L('col_labramon', 3, ['MT'], 'Data', ['Labramon', 'Dobermon', 'Dobermon'], ['Retriever Bark', 'Black Cannon', 'Black Cannon Burst'], 'col_monster', o('Lulu (Monster Trainer)', { role: 'Bruiser', ai: { value: 22, carry: true }, levels: ['Rookie', 'Champion', 'Champion'] })),
    L('col_vegiemon', 3, ['WR', 'PRO'], 'Virus', ['Vegiemon', 'Vademon', 'Arukenimon'], ['Sweet Breath', 'Unidentified Flying Kiss', 'Spider Thread'], 'col_infect', o('Malzahar', { role: 'Mystic', levels: ['Champion', 'Ultimate', 'Ultimate'] })),
    L('col_lalamon', 3, ['SG', 'PRT'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Seed Blast', 'Sunshine Beam', 'Lila Shower'], 'slam', o('Neeko', { role: 'Bruiser', radius: 2 })),
    L('col_tyrannomon', 3, ['MECH', 'EXE'], 'Data', ['Tyrannomon', 'MetalTyrannomon', 'Chaosdramon'], ['Fire Breath', 'Nuclear Laser', 'Hyper Mugen Cannon'], 'line', o('Senna', { role: 'Sniper', hpMul: 1.1, adMul: 1.15, levels: CUM })),
    L('col_saberdramon', 3, ['CG', 'BAS', 'SOR'], 'Vaccine', ['Saberdramon', 'Megadramon', 'Gigadramon'], ['Night Roar', 'Ultimate Slicer', 'Giga Heat'], 'drain', o('Swain', { role: 'Bruiser', radius: 1, hpMul: 0.9, levels: ['Champion', 'Ultimate', 'Ultimate'] })),
    L('col_reppamon', 3, ['MEN', 'JUG', 'DUE'], 'Vaccine', ['Reppamon', 'Chirinmon', 'Kentaurosmon'], ['Lion Claw', 'Wind Cutter Sword', 'Bifrost'], 'drain', o('Udyr', { role: 'Duelist', hpMul: 0.93, levels: CUM })),
    L('col_sangloupmon', 3, ['SF', 'DUE', 'EDG'], 'Virus', ['Sangloupmon', 'Matadormon', 'GranDracmon'], ['Sticker Blade', 'Thousand Arrows', 'Crystal Revolution'], 'stun', o('Viego', { role: 'Duelist', levels: CUM })),
    L('col_kotemon', 3, ['MEN', 'EDG'], 'Data', ['Kotemon', 'Raptordramon', 'Grademon'], ['Meteor Slash', 'Crash Charge', 'Grade Slash'], 'aoe', o('Yasuo', { role: 'Striker' })),
    L('col_mamemon', 3, ['CREW', 'STR'], 'Data', ['Mamemon', 'MetalMamemon', 'PrinceMamemon'], ['Smiley Bomb', 'Energy Bomb', 'Smile Warhead'], 'aoe', o('Ziggs', { role: 'Mystic', levels: ['Ultimate', 'Ultimate', 'Mega'] })),
    // ---- 4-cost (11) ----
    L('col_sistermonciel', 4, ['CG', 'DUE'], 'Data', ['Sistermon Ciel (Awakened)', 'Sistermon Ciel (Awakened)', 'Dianamon'], ['Silent Bullet', 'Piercing Wasp', 'Arrow of Artemis'], 'barrage', o('Ashe', { role: 'Sniper', levels: ['Champion', 'Champion', 'Mega'] })),
    L('col_coredramon', 4, ['MECH', 'STR'], 'Virus', ['Coredramon (Green)', 'Groundramon', 'Breakdramon'], ['Strike Bomber', 'Megaton Hammer Crush', 'Destroyed Rush'], 'slam', o('Jarvan IV', { role: 'Guardian', radius: 1, adMul: 0.9, levels: CUM })),
    L('col_gargomon', 4, ['SG', 'SNI'], 'Vaccine', ['Gargomon', 'Rapidmon', 'MegaGargomon'], ['Gargo Laser', 'Miracle Missile', 'Giant Missile'], 'aoe', o('Jinx', { role: 'Sniper', levels: CUM })),
    L('col_bakemon', 4, ['WR', 'PRT'], 'Virus', ['Bakemon', 'Phantomon', 'Titamon'], ["Hell's Hand", 'Soul Chopper', 'Phantom Blade'], 'stun', o("K'Sante", { role: 'Guardian', hpMul: 0.92, levels: CUM })),
    L('col_keramon', 4, ['MECH', 'SOR'], 'Virus', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', "Hell's Grenade"], 'aoe', o('Karma', { role: 'Mystic' })),
    L('col_geogreymon', 4, ['BA', 'BAS'], 'Vaccine', ['GeoGreymon', 'RizeGreymon', 'ShineGreymon'], ['Mega Burst', 'Trident Revolver', 'Glorious Burst'], 'col_aegis', o('Leona', { role: 'Guardian', levels: CUM })),
    L('col_bearmon', 4, ['SG', 'HEV'], 'Vaccine', ['Bearmon', 'Grizzlymon', 'Monzaemon'], ['Bear Fist', 'Crescent Moon Kick', 'Hearts Attack'], 'col_aegis', o('Poppy', { role: 'Bruiser', hpMul: 0.85, levels: ['Rookie', 'Champion', 'Ultimate'] })),
    L('col_wizardmon', 4, ['MEN', 'EXE', 'STR'], 'Data', ['Wizardmon', 'Wisemon', 'Piximon'], ['Thunder Cloud', 'Eternal Nirvana', 'Pit Bomb'], 'aoe', o('Ryze', { role: 'Mystic', hpMul: 1.1, levels: ['Champion', 'Ultimate', 'Ultimate'] })),
    L('col_impmon', 4, ['SF', 'EDG'], 'Virus', ['Impmon', 'Beelzemon', 'Beelzemon Blast Mode'], ['Bada Boom', 'Double Impact', 'Corona Destroyer'], 'dash', o('Samira', { role: 'Sniper', levels: ['Rookie', 'Mega', 'Mega'] })),
    L('col_leomon', 4, ['SF', 'JUG'], 'Vaccine', ['Leomon', 'Panjyamon', 'BanchoLeomon'], ['Fist of the Beast King', 'Ice Fist', 'Flashy Boss Punch'], 'drain', o('Sett', { role: 'Bruiser', hpMul: 0.9, adMul: 0.9, levels: CUM })),
    L('col_salamon', 4, ['BA', 'PRO'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'barrage', o('Yuumi', { role: 'Mystic', levels: RCU })),
    // ---- 5-cost (10) ----
    L('col_ravemon', 5, ['SC', 'EXE'], 'Vaccine', ['Ravemon', 'Ravemon', 'Ravemon'], ['Blast Wing', 'Raven King Sword', 'Crimson Formation'], 'dash', o('Akali', { role: 'Assassin', levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_craniamon', 5, ['CH', 'LU', 'BAS'], 'Vaccine', ['Craniamon', 'Craniamon', 'Craniamon X'], ['End Waltz', 'Breath of the Gods', 'Grand Gridr'], 'slam', o('Braum', { role: 'Guardian', ai: { value: 4 }, radius: 2, hpMul: 1.15, adMul: 1.1, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_puppetmon', 5, ['SF', 'SOR'], 'Virus', ['Puppetmon', 'Puppetmon', 'Puppetmon'], ['Puppet Pummel', 'Flying Cross Cutter', 'Wood Cross'], 'line', o('Gwen', { role: 'Duelist', range: 2, ai: { dmg: 'ap' }, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_justimon', 5, ['SM'], 'Vaccine', ['Justimon', 'Justimon', 'Justimon'], ['Voltage Fist', 'Justice Kick', 'Trinity Arm'], 'col_stance', o('Lee Sin', { role: 'Duelist', ai: { value: 14 }, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_ophanimon', 5, ['SG', 'PRO'], 'Vaccine', ['Ophanimon', 'Ophanimon', 'Ophanimon'], ['Sefirot Crystal', "Eden's Javelin", 'Flame Hellscythe'], 'aoe', o('Seraphine', { role: 'Mystic', radius: 2, mana: 80, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_piedmon', 5, ['RC', 'CREW'], 'Virus', ['Piedmon', 'Piedmon', 'Piedmon'], ['Trump Sword', "Clown's Trick", 'Ending Illusion'], 'barrage', o('Twisted Fate', { role: 'Mystic', hpMul: 1.1, ai: { value: 4 }, shots: 6, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_myotismon', 5, ['WR', 'SNI'], 'Virus', ['Myotismon', 'VenomMyotismon', 'VenomMyotismon'], ['Crimson Lightning', 'Venom Infusion', 'Chaos Flame'], 'line', o('Varus', { role: 'Sniper', levels: ['Ultimate', 'Mega', 'Mega'] })),
    L('col_hiandromon', 5, ['MECH', 'EDG'], 'Vaccine', ['HiAndromon', 'HiAndromon', 'HiAndromon'], ['Lightning Blade', 'Atomic Ray', 'Atomic Ray'], 'line', o('Yone', { role: 'Duelist', hpMul: 1.1, adMul: 1.15, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_lotosmon', 5, ['CG', 'RM'], 'Data', ['Lotosmon', 'Lotosmon', 'Lotosmon'], ['Thorn Whip', 'Serpent Cure', 'Serpent Ruin'], 'aoe', o('Zyra', { role: 'Mystic', ai: { value: 4 }, levels: ['Mega', 'Mega', 'Mega'] })),
    L('col_merukimon', 5, ['LU', 'EDG'], 'Data', ['Merukimon', 'Merukimon', 'Merukimon'], ['Thousand Fists', 'Spiritual Enchantment', 'Mirage Image'], 'col_frenzy', o('Volibear', { role: 'Bruiser', levels: ['Mega', 'Mega', 'Mega'] })),

    // ---- summons (never in the shop) ----
    L('col_mech', 3, [], 'Free', ['Machinedramon', 'Machinedramon', 'Machinedramon'], ['Giga Cannon', 'Giga Cannon', 'Infinity Cannon'], 'slam', { ...U, role: 'Bruiser', shop: false, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_root', 3, [], 'Data', ['Woodmon', 'Woodmon', 'Woodmon'], ['Branch Drain', 'Branch Drain', 'Branch Drain'], 'shield', { ...U, role: 'Guardian', shop: false, mana: 120, levels: ['Champion', 'Champion', 'Champion'] }),
    L('col_spine', 3, [], 'Data', ['Togemon', 'Togemon', 'Togemon'], ['Needle Spray', 'Needle Spray', 'Needle Spray'], 'barrage', { ...U, role: 'Sniper', shop: false, shots: 2, mana: 120, levels: ['Champion', 'Champion', 'Champion'] }),

    // ---- wild Digimon for PvE: the K.O. tournament brackets, headlined by Kimeramon and then Darkdramon ----
    L('col_w_gazimon', 1, [], 'Virus', ['Gazimon', 'Gazimon', 'Gazimon'], ['Electric Stun Blast', 'Electric Stun Blast', 'Electric Stun Blast'], 'nuke', { ...U, role: 'Striker', shop: false, hpMul: 0.8 }),
    L('col_w_mushroomon', 1, [], 'Virus', ['Mushroomon', 'Mushroomon', 'Mushroomon'], ['Fungus Crusher', 'Fungus Crusher', 'Fungus Crusher'], 'nuke', { ...U, role: 'Mystic', shop: false, hpMul: 0.8 }),
    L('col_w_crabmon', 2, [], 'Data', ['Crabmon', 'Crabmon', 'Crabmon'], ['Scissors Execution', 'Scissors Execution', 'Scissors Execution'], 'nuke', { ...U, role: 'Guardian', shop: false }),
    L('col_w_snowgoblimon', 2, [], 'Virus', ['SnowGoblimon', 'SnowGoblimon', 'SnowGoblimon'], ['Snow Goblin Volt', 'Snow Goblin Volt', 'Snow Goblin Volt'], 'nuke', { ...U, role: 'Sniper', shop: false }),
    L('col_w_monochromon', 3, [], 'Data', ['Monochromon', 'Monochromon', 'Monochromon'], ['Volcanic Strike', 'Volcanic Strike', 'Volcanic Strike'], 'slam', { ...U, role: 'Guardian', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('col_w_meramon', 3, [], 'Data', ['Meramon', 'Meramon', 'Meramon'], ['Burning Fist', 'Burning Fist', 'Burning Fist'], 'aoe', { ...U, role: 'Striker', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('col_w_hyogamon', 3, [], 'Virus', ['Hyogamon', 'Hyogamon', 'Hyogamon'], ['Snow Punch', 'Snow Punch', 'Snow Punch'], 'stun', { ...U, role: 'Bruiser', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('col_w_unimon', 3, [], 'Vaccine', ['Unimon', 'Unimon', 'Unimon'], ['Horn Blaster', 'Horn Blaster', 'Horn Blaster'], 'line', { ...U, role: 'Sniper', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('col_w_knightmon', 4, [], 'Data', ['Knightmon', 'Knightmon', 'Knightmon'], ['Berserk Sword', 'Berserk Sword', 'Berserk Sword'], 'slam', { ...U, role: 'Guardian', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('col_w_triceramon', 4, [], 'Data', ['Triceramon', 'Triceramon', 'Triceramon'], ['Tri-Horn Attack', 'Tri-Horn Attack', 'Tri-Horn Attack'], 'stun', { ...U, role: 'Bruiser', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('col_w_etemon', 4, [], 'Virus', ['Etemon', 'Etemon', 'Etemon'], ['Dark Spirits', 'Dark Spirits', 'Dark Spirits'], 'aoe', { ...U, role: 'Mystic', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('col_w_skullgreymon', 4, [], 'Virus', ['SkullGreymon', 'SkullGreymon', 'SkullGreymon'], ['Dark Shot', 'Dark Shot', 'Dark Shot'], 'nuke', { ...U, role: 'Striker', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('col_w_kimeramon', 3, [], 'Data', ['Kimeramon', 'Kimeramon', 'Kimeramon'], ['Heat Viper', 'Heat Viper', 'Heat Viper'], 'drain', { ...U, role: 'Bruiser', shop: false, boss: true, hpMul: 1.25, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('col_w_darkdramon', 5, [], 'Virus', ['Darkdramon', 'Darkdramon', 'Darkdramon'], ['Giga Sea Destroyer', 'Giga Sea Destroyer', 'Giga Sea Destroyer'], 'aoe', { ...U, role: 'Bruiser', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_w_alphamon', 5, [], 'Vaccine', ['Alphamon', 'Alphamon', 'Alphamon'], ['Digitalize of Soul', 'Digitalize of Soul', 'Digitalize of Soul'], 'slam', { ...U, role: 'Guardian', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_w_omnimon', 5, [], 'Vaccine', ['Omnimon', 'Omnimon', 'Omnimon'], ['Transcendent Sword', 'Transcendent Sword', 'Transcendent Sword'], 'execute', { ...U, role: 'Striker', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_w_imperialdramon', 5, [], 'Vaccine', ['Imperialdramon Paladin Mode', 'Imperialdramon Paladin Mode', 'Imperialdramon Paladin Mode'], ['Omni Blade', 'Omni Blade', 'Omni Blade'], 'line', { ...U, role: 'Duelist', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_w_millenniummon', 5, [], 'Virus', ['Millenniummon', 'Millenniummon', 'Millenniummon'], ['Time Unlimited', 'Time Unlimited', 'Time Unlimited'], 'aoe', { ...U, role: 'Mystic', shop: false, boss: true, hpMul: 1.5, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('col_w_apocalymon', 5, [], 'Virus', ['Apocalymon', 'Apocalymon', 'Apocalymon'], ['Darkness Zone', 'Darkness Zone', 'Darkness Zone'], 'drain', { ...U, role: 'Bruiser', shop: false, boss: true, hpMul: 1.6, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
  ];

  // Wild rounds: [lineId, stage, col, row] (rows 4-7, 4 = front). Hyper Roll: 1-2, 1-3, then x-4. Normal pace: 1-2 to
  // 1-4, then x-7, at the end of a longer stage, so some of the bracket's fighters are one star higher.
  const pveWave = (stage, round, game) => {
    if (game && game.pace === 'normal') {
      if (stage === 1) {
        if (round <= 2) return [['col_w_gazimon', 0, 2, 4], ['col_w_gazimon', 0, 4, 4]];
        if (round === 3) return [['col_w_gazimon', 0, 2, 4], ['col_w_gazimon', 0, 4, 4], ['col_w_mushroomon', 0, 3, 6]];
        return [['col_w_gazimon', 0, 2, 4], ['col_w_gazimon', 0, 3, 4], ['col_w_gazimon', 0, 4, 4], ['col_w_mushroomon', 0, 3, 6]];
      }
      // the same bracket as Hyper Roll with some fighters one star higher: one helper in stages 2-3, three helpers from stage 4, the lead too in stage 5
      const up = stage <= 3 ? [1] : stage === 5 ? [0, 1, 2, 3] : [1, 2, 3];
      return hyperWave(stage, 4).map((w, i) => (up.includes(i) ? [w[0], Math.min(2, w[1] + 1), w[2], w[3]] : w));
    }
    return hyperWave(stage, round);
  };
  const hyperWave = (stage, round) => {
    if (stage === 1) {
      if (round === 2) return [['col_w_gazimon', 0, 2, 4], ['col_w_gazimon', 0, 4, 4], ['col_w_mushroomon', 0, 3, 6]];
      return [['col_w_gazimon', 0, 2, 4], ['col_w_gazimon', 0, 3, 4], ['col_w_gazimon', 0, 4, 4], ['col_w_mushroomon', 0, 3, 6]];
    }
    if (stage === 2) return [['col_w_kimeramon', 1, 3, 4], ['col_w_crabmon', 1, 1, 4], ['col_w_crabmon', 0, 5, 4], ['col_w_snowgoblimon', 0, 3, 6]];
    if (stage === 3) return [['col_w_kimeramon', 1, 3, 4], ['col_w_monochromon', 1, 2, 4], ['col_w_monochromon', 1, 4, 4], ['col_w_meramon', 0, 1, 5], ['col_w_hyogamon', 1, 5, 5], ['col_w_unimon', 0, 3, 6]];
    if (stage === 4) return [['col_w_kimeramon', 2, 3, 4], ['col_w_knightmon', 1, 2, 4], ['col_w_triceramon', 1, 4, 4], ['col_w_monochromon', 1, 1, 4], ['col_w_etemon', 1, 2, 6], ['col_w_unimon', 1, 4, 7]];
    if (stage === 5) return [['col_w_darkdramon', 0, 3, 4], ['col_w_knightmon', 2, 2, 4], ['col_w_triceramon', 2, 4, 4], ['col_w_skullgreymon', 2, 1, 5], ['col_w_etemon', 2, 2, 6], ['col_w_unimon', 1, 4, 7]];
    if (stage === 6) return [['col_w_darkdramon', 1, 3, 4], ['col_w_alphamon', 1, 2, 4], ['col_w_omnimon', 1, 4, 4], ['col_w_knightmon', 2, 1, 4], ['col_w_skullgreymon', 2, 1, 6], ['col_w_skullgreymon', 2, 5, 6], ['col_w_etemon', 2, 3, 7]];
    if (stage === 7) return [['col_w_alphamon', 2, 2, 4], ['col_w_omnimon', 2, 4, 4], ['col_w_imperialdramon', 1, 3, 5], ['col_w_darkdramon', 2, 3, 4], ['col_w_skullgreymon', 2, 1, 6], ['col_w_etemon', 2, 2, 7], ['col_w_etemon', 2, 4, 7]];
    return [['col_w_apocalymon', 2, 3, 4], ['col_w_millenniummon', 2, 3, 7], ['col_w_omnimon', 2, 2, 4], ['col_w_alphamon', 2, 4, 4], ['col_w_imperialdramon', 2, 1, 6], ['col_w_darkdramon', 2, 5, 6], ['col_w_skullgreymon', 2, 1, 4], ['col_w_skullgreymon', 2, 5, 4]];
  };

  // ---------- items ----------
  const components = {
    col_sword: { name: 'Power Chip', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    col_bow: { name: 'Speed Chip', short: 'SC', color: '#ffd23f', stats: { as: 0.12 } },
    col_rod: { name: 'Data Rod', short: 'DR', color: '#b26bff', stats: { ap: 15 } },
    col_tear: { name: 'Mana Card', short: 'MC', color: '#4fb3ff', stats: { mana: 15 } },
    col_vest: { name: 'Chrome Plate', short: 'CP', color: '#c0c6d0', stats: { armor: 20 } },
    col_cloak: { name: 'Data Cloak', short: 'DC', color: '#5fe0c0', stats: { mr: 20 } },
    col_belt: { name: 'HP Disk', short: 'HD', color: '#ff5d73', stats: { hp: 150 } },
    col_glove: { name: 'Lucky Glove', short: 'LG', color: '#9be15d', stats: { crit: 0.15 } },
    col_egg: { name: 'Digi-Egg', short: 'EG', color: '#fff3b0', stats: {}, special: true,
      desc: 'Combine with a component to make an origin Crest. Two Digi-Eggs make a Golden Digi-Egg; with a Crest Tag it makes a Digivice.' },
    col_tag: { name: 'Crest Tag', short: 'CT', color: '#ffb84a', stats: {}, special: true,
      desc: 'Combine with a component to make a class Crest. Two Crest Tags make a Golden Tag.' },
  };

  // [a, b, name, desc, passives, extraStats, { id, tags, aiRank }]. Hooks for the items that need them are in coliseum-mech.js.
  const R = (a, b, name, desc, p, extra, opt) => [a, b, name, desc, p, extra, opt];
  const recipes = [
    R('col_sword', 'col_sword', 'Giga Blade', 'Deals 10% more damage.', { amp: 0.1 }, { ad: 0.25 }, { id: 'col_giga_blade', tags: ['ad'], aiRank: 3 }),
    R('col_sword', 'col_bow', 'Titan Slayer', 'Deals 8% more damage, and 25% more to enemies with over 1600 max HP.', { amp: 0.08, slayer: 0.25 }, { ap: 10 }, { id: 'col_titan_slayer', tags: ['ad'], aiRank: 3 }),
    R('col_sword', 'col_rod', 'Data Gunblade', 'Heals for 18% of damage dealt. Also heals the lowest-health ally for 15% of damage dealt.', { vamp: 0.18 }, {}, { id: 'col_data_gunblade', tags: ['ad', 'ap'] }),
    R('col_sword', 'col_tear', 'Shojin Spear', 'Attacks restore 5 extra mana.', { shojin: 5 }, {}, { id: 'col_shojin_spear', tags: ['ap', 'ad'] }),
    R('col_sword', 'col_vest', 'Night Cloak', 'The first time HP drops below 60%, become untouchable for 1 second, shake off stuns and gain 25% attack speed.', { lifeline: { at: 0.6, shield: 0, as: 0.25 } }, {}, { id: 'col_night_cloak', tags: ['ad'] }),
    R('col_sword', 'col_cloak', 'Blood Fang', 'Heals for 20% of damage dealt. Below 40% HP, gain a 25% max HP shield once.', { vamp: 0.2, lifeline: { at: 0.4, shield: 0.25 } }, {}, { id: 'col_blood_fang', tags: ['ad'], aiRank: 2 }),
    R('col_sword', 'col_belt', 'Sterak Gauntlet', 'The first time HP drops below 60%, gain a 40% max HP shield and 30% attack damage.', { lifeline: { at: 0.6, shield: 0.4, ad: 0.3 } }, { hp: 100 }, { id: 'col_sterak_gauntlet', tags: ['ad', 'tank'] }),
    R('col_sword', 'col_glove', 'Crit Edge', 'Abilities can critically strike. +10% crit damage.', { abilCrit: true, critDmg: 0.1 }, { crit: 0.15, ad: 0.1 }, { id: 'col_crit_edge', tags: ['ad'], aiRank: 3 }),
    R('col_bow', 'col_bow', 'Red Fang', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, { as: 0.25 }, { id: 'col_red_fang', tags: ['ad'] }),
    R('col_bow', 'col_rod', 'Rage Rod', 'Each attack grants 5% attack speed, stacking.', { rage: 0.05 }, {}, { id: 'col_rage_rod', tags: ['ad', 'ap'], aiRank: 2 }),
    R('col_bow', 'col_tear', 'Void Staff', 'Damage cuts the target\'s MR by 30% for 5 seconds.', {}, { ap: 10 }, { id: 'col_void_staff', tags: ['ap'] }),
    R('col_bow', 'col_vest', 'Titan Will', 'Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks.', { titan: true }, {}, { id: 'col_titan_will', tags: ['tank', 'ad'] }),
    R('col_bow', 'col_cloak', 'Kraken Fang', 'Each attack grants 3.5% AD, stacking.', { kraken: 0.035 }, {}, { id: 'col_kraken_fang', tags: ['ad'] }),
    R('col_bow', 'col_belt', 'Nashor Fang', 'Attacks restore 3 extra mana.', { shojin: 3 }, { ap: 10 }, { id: 'col_nashor_fang', tags: ['ap'] }),
    R('col_bow', 'col_glove', 'Armor Breaker', 'Attacks cut the target\'s armor by 30% for 3 seconds.', { shred: 0.3 }, {}, { id: 'col_armor_breaker', tags: ['ad'] }),
    R('col_rod', 'col_rod', 'Ultima Rod', 'Deals 8% more damage.', { amp: 0.08 }, { ap: 30 }, { id: 'col_ultima_rod', tags: ['ap'], aiRank: 3 }),
    R('col_rod', 'col_tear', 'Seraph Staff', 'Gain 15 AP every 5 seconds.', { archangel: 15 }, {}, { id: 'col_seraph_staff', tags: ['ap'], aiRank: 2 }),
    R('col_rod', 'col_vest', 'Crown Guard', 'Start combat with a 25% max HP shield. After 8 seconds, gain 30 AP.', { crown: true }, {}, { id: 'col_crown_guard', tags: ['ap', 'tank'] }),
    R('col_rod', 'col_cloak', 'Ion Spark', 'When an enemy within 2 hexes casts, deal 150 magic damage to it. Enemies within 2 hexes lose 15 armor and MR.', { zap: 150, veil: 15 }, {}, { id: 'col_ion_spark', tags: ['tank'] }),
    R('col_rod', 'col_belt', 'Flame Tome', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, {}, { id: 'col_flame_tome', tags: ['ap'] }),
    R('col_rod', 'col_glove', 'Jewel Gauntlet', 'Abilities can critically strike. +15% crit damage.', { abilCrit: true, critDmg: 0.15 }, { crit: 0.1 }, { id: 'col_jewel_gauntlet', tags: ['ap'], aiRank: 3 }),
    R('col_tear', 'col_tear', 'Blue Card', 'Max mana -10. After casting, keep 20 mana.', { bluebuff: true }, { ap: 10, ad: 0.1 }, { id: 'col_blue_card', tags: ['ap'], aiRank: 2 }),
    R('col_tear', 'col_vest', 'Guardian Vow', 'Below 40% HP, gain a 25% max HP shield and 15 mana once.', { lifeline: { at: 0.4, shield: 0.25 } }, { mana: 5 }, { id: 'col_guardian_vow', tags: ['tank'] }),
    R('col_tear', 'col_cloak', 'Adaptive Helm', 'Gain 10 mana every 3 seconds. In the front two rows, gain 25 armor and MR; in the back two rows, gain 15% AD and 15 AP.', { manaRegen: 10 }, {}, { id: 'col_adaptive_helm', tags: ['tank', 'ap'] }),
    R('col_tear', 'col_belt', 'Spirit Visage', 'Regenerates 1.5% max HP per second.', { regen: 0.015 }, { hp: 150 }, { id: 'col_spirit_visage', tags: ['tank'] }),
    R('col_tear', 'col_glove', 'Justice Hand', 'Deals 12% more damage and heals for 12% of damage dealt.', { amp: 0.12, vamp: 0.12 }, {}, { id: 'col_justice_hand', tags: ['ad', 'ap'] }),
    R('col_vest', 'col_vest', 'Bramble Plate', 'Takes 6% less damage. When attacked, deals 80 magic damage to adjacent enemies (1.5s cooldown).', { thorns: 80, dr: 0.06 }, {}, { id: 'col_bramble_plate', tags: ['tank'], aiRank: 2 }),
    R('col_vest', 'col_cloak', 'Gargoyle Stone', 'Gain 10 armor and MR for each enemy within 2 hexes.', { gargoyle: 10 }, { hp: 100 }, { id: 'col_gargoyle_stone', tags: ['tank'], aiRank: 2 }),
    R('col_vest', 'col_belt', 'Sunfire Core', 'Every 2 seconds, burns the nearest enemy within 2 hexes.', { sunfire: true }, { hp: 100 }, { id: 'col_sunfire_core', tags: ['tank'] }),
    R('col_vest', 'col_glove', 'Steadfast Heart', 'Takes 10% less damage, 18% less while above 50% HP.', { dr: 0.1 }, {}, { id: 'col_steadfast_heart', tags: ['tank'] }),
    R('col_cloak', 'col_cloak', 'Dragon Scale', 'Regenerates 1.2% max HP per second.', { regen: 0.012 }, { hp: 100 }, { id: 'col_dragon_scale', tags: ['tank'] }),
    R('col_cloak', 'col_belt', 'Even Shroud', 'Enemies within 2 hexes lose 25 armor and MR. Gain 25 armor and MR for the first 15 seconds.', { veil: 25 }, {}, { id: 'col_even_shroud', tags: ['tank'] }),
    R('col_cloak', 'col_glove', 'Quick Silver', 'Immune to stuns for the first 18 seconds. Each attack grants 2% attack speed, stacking.', { ccImmune: 18, rage: 0.02 }, {}, { id: 'col_quick_silver', tags: ['ad'] }),
    R('col_belt', 'col_belt', 'Warmog Disk', 'Gain 10% more max HP.', {}, { hp: 400 }, { id: 'col_warmog_disk', tags: ['tank'] }),
    R('col_belt', 'col_glove', 'Striker Flail', 'Critical strikes make the holder deal 5% more damage for 5 seconds, stacking up to 4 times.', {}, { as: 0.1 }, { id: 'col_striker_flail', tags: ['ad'] }),
    R('col_glove', 'col_glove', 'Thief Glove', 'Each combat, gains two random completed items.', { thief: true }, {}, { id: 'col_thief_glove', tags: ['ad'] }),
  ];

  // Crests: Digi-Egg + component = origin Crest, Crest Tag + component = class Crest (both like TFT's Spatula and
  // Frying Pan). Protector and Sniper Crests have no recipe; they come from the Armory, loot and augments.
  const crest = (trait, from, stats) => ({ id: `col_cr_${trait.toLowerCase()}`, name: `${TRAITS[trait].name} Crest`, cat: 'emblem', emblem: trait,
    p: { emblem: trait }, stats: stats || {}, from, color: TRAITS[trait].color, color2: '#fff3b0', short: 'C' + TRAITS[trait].name[0],
    desc: `The holder gains the ${TRAITS[trait].name} trait.` });
  const st = (c) => ({ ...components[c].stats });
  const crests = [
    crest('BA', ['col_egg', 'col_tear'], st('col_tear')),
    crest('CG', ['col_egg', 'col_belt'], st('col_belt')),
    crest('LU', ['col_egg', 'col_glove'], st('col_glove')),
    crest('SF', ['col_egg', 'col_sword'], st('col_sword')),
    crest('SG', ['col_egg', 'col_rod'], st('col_rod')),
    crest('STR', ['col_egg', 'col_cloak'], st('col_cloak')),
    crest('SC', ['col_egg', 'col_bow'], st('col_bow')),
    crest('WR', ['col_egg', 'col_vest'], st('col_vest')),
    crest('BAS', ['col_tag', 'col_vest'], st('col_vest')),
    crest('DUE', ['col_tag', 'col_bow'], st('col_bow')),
    crest('EDG', ['col_tag', 'col_sword'], st('col_sword')),
    crest('EXE', ['col_tag', 'col_glove'], st('col_glove')),
    crest('HEV', ['col_tag', 'col_belt'], st('col_belt')),
    crest('JUG', ['col_tag', 'col_cloak'], st('col_cloak')),
    crest('PRO', ['col_tag', 'col_tear'], st('col_tear')),
    crest('SOR', ['col_tag', 'col_rod'], st('col_rod')),
    crest('PRT', null, { hp: 150 }),
    crest('SNI', null, { hp: 150 }),
  ];

  // Artifacts: no recipe. Gold orbs from the boss rounds (stage 5+) and the Armory from stage 5.
  const art = (id, name, stats, desc, p, tags, color, color2) => ({ id, name, cat: 'artifact', noDrop: true, stats, desc, p: p || {}, tags, color, color2 });
  const artifacts = [
    art('col_ar_doom', 'Doom Grasp', { ap: 30 }, 'At the start of combat, and every 13 seconds, blasts the nearest enemy for 25% of its max HP as magic damage.', null, ['ap'], '#7a3aff', '#2a1040'),
    art('col_ar_collector', 'Gold Collector', { ad: 0.25, crit: 0.25 }, 'Damage finishes off enemies below 12% HP.', null, ['ad'], '#ffd84a', '#7a4a10'),
    art('col_ar_lichbane', 'Lich Bane', { ap: 30, as: 0.2 }, 'The first attack after each cast deals 100 + 60 per stage bonus magic damage.', null, ['ap'], '#5ad1ff', '#1a3a6a'),
    art('col_ar_hull', 'Hull Crusher', { armor: 35, mr: 35 }, 'If no ally is adjacent at the start of combat, gain 600 HP, 20% AD and 20 AP.', null, ['tank', 'ad'], '#9ab0c8', '#3a4050'),
    art('col_ar_static', 'Static Shiv', { ap: 30, as: 0.3 }, 'Every 3rd attack sends lightning to 3 enemies for 120 magic damage.', { volt: 120 }, ['ap', 'ad'], '#ffe94a', '#3a3a10'),
    art('col_ar_hydra', 'Titanic Hydra', { ad: 0.2, as: 0.2, hp: 300 }, 'Attacks also hit a second nearby enemy for 40% damage.', { split: 0.4 }, ['ad', 'tank'], '#5fe0a0', '#104030'),
    art('col_ar_zhonya', 'Zhonya Paradox', { ap: 40, armor: 30, mr: 30 }, 'Once per combat at 40% HP, become untouchable for 2.5 seconds.', null, ['ap', 'tank'], '#ffcf6a', '#5a3a10'),
    art('col_ar_infinity', 'Infinity Force', { ad: 0.3, ap: 30, as: 0.3, armor: 30, mr: 30, hp: 300 }, 'A little of everything.', null, ['ad', 'ap', 'tank'], '#ff8fd8', '#4a2a6a'),
  ];

  const consumables = [
    { id: 'col_core', consumable: true, noDrop: true, name: 'Digi-Core', short: 'DC', color: '#7dffb0', color2: '#2a6a50', stats: {}, p: {},
      desc: 'Drag onto a Digimon to pick 1 of 3 digivolution paths for it. A new path replaces the old one. Selling that Digimon gives the Digi-Core back.' },
    { id: 'col_extractor', consumable: true, noDrop: true, name: 'Data Extractor', short: 'DX', color: '#9fd8ff', color2: '#3a4a6a', stats: {}, p: {},
      desc: 'Drag onto a Digimon to remove its digivolution path and get its Digi-Core back.' },
    { id: 'col_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'col_reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
  ];

  const aiRoles = {
    ap: ['col_jewel_gauntlet', 'col_ultima_rod', 'col_seraph_staff', 'col_blue_card', 'col_rage_rod', 'col_shojin_spear', 'col_justice_hand', 'col_data_gunblade', 'col_void_staff', 'col_flame_tome', 'col_nashor_fang', 'col_adaptive_helm', 'col_crown_guard'],
    ad: ['col_crit_edge', 'col_giga_blade', 'col_titan_slayer', 'col_blood_fang', 'col_rage_rod', 'col_kraken_fang', 'col_red_fang', 'col_armor_breaker', 'col_justice_hand', 'col_sterak_gauntlet', 'col_night_cloak', 'col_striker_flail', 'col_quick_silver', 'col_thief_glove'],
    tank: ['col_bramble_plate', 'col_gargoyle_stone', 'col_warmog_disk', 'col_steadfast_heart', 'col_dragon_scale', 'col_sunfire_core', 'col_guardian_vow', 'col_even_shroud', 'col_spirit_visage', 'col_ion_spark', 'col_titan_will', 'col_crown_guard', 'col_sterak_gauntlet'],
  };

  // ---------- Digi-Core paths (TFT Power Ups as digivolution branches) ----------
  // tags pick which three are offered for a Digimon: 'ad' attack carries, 'ap' casters, 'tank' front liners, 'any' everyone.
  // Effects are in coliseum-mech.js (POWER_UP_FX). src: the TFT Set 15 Power Up each path adapts.
  const P = (id, name, tags, desc, src) => ({ id, name, tags, desc, src });
  const POWER_UPS = [
    P('burst', 'Burst Mode', ['ad', 'ap'], 'Deals 12% more damage, 25% more after its first takedown each combat.', 'Annihilation'),
    P('xanti', 'X-Antibody', ['tank'], 'When hit, gains 4 armor (physical) or 8 MR (magic), up to 90 in total.', 'Adaptive Skin'),
    P('dark', 'Dark Digivolution', ['ad'], 'Gains 22% attack damage and 10% omnivamp, but loses 1% max HP every second.', 'All Out'),
    P('warp', 'Warp Digivolution', ['any'], 'Gains 8% attack damage, 8 AP, 10 armor and MR, and deals 5% more damage.', '404 Power Up'),
    P('armor', 'Armor Digivolution', ['tank'], 'Starts combat with a 25% max HP shield. While shielded, gains 30% attack speed.', 'Living Wall'),
    P('mode', 'Mode Change', ['tank', 'ap'], 'At the start of combat, it and its nearest ally gain an 18% max HP shield.', 'Body Change'),
    P('slide', 'Slide Evolution', ['ad'], 'When it switches targets, its next attack deals 120% bonus damage as magic damage.', 'Blink Attack'),
    P('spirit', 'Spirit Evolution', ['any'], 'The first time it drops below 40% HP, heals 25% max HP and gains 25% attack speed.', 'Not Done Yet'),
    P('deathx', 'Death-X Evolution', ['ad'], 'Leaps to the enemy backline at the start of combat and gains 30% crit chance and 10% crit damage.', 'Assassinate'),
    P('biomerge', 'Biomerge', ['ad', 'ap'], 'Gains 3% attack damage and 3 AP every second, up to 30 times.', "Hero's Arc"),
    P('mega', 'Mega Digivolution', ['ad'], 'Gains 40% attack damage.', 'Max Attack'),
    P('sage', 'Sage Evolution', ['ap'], 'Gains 25 AP.', 'Max Arcana'),
    P('golem', 'Golem Form', ['tank'], 'Gains 30% max HP.', 'Max Vitality'),
    P('speed', 'Speed Form', ['ad'], 'Gains 30% attack speed.', 'Max Speed'),
    P('overwrite', 'Overwrite', ['ad', 'ap'], 'Gains 30% crit chance and 15% crit damage; its abilities can critically strike.', 'Critical Threat'),
    P('matrix', 'Matrix Evolution', ['ap'], 'Its max mana is 25% lower and it starts combat with 10 extra mana.', 'Efficient'),
    P('overdrive', 'Overdrive', ['any'], 'Deals 12% more damage. After its first takedown or 10 seconds of combat, stuns enemies within 2 hexes for 1.5 seconds and deals 20% more damage for the rest of combat.', '2XKO'),
    P('atomic', 'Atomic Core', ['tank'], 'Stores 10% max HP at the start of combat and 2% more every second. When it falls, deals the stored amount as magic damage to enemies within 2 hexes.', 'Atomic'),
    P('thorn', 'Thorn Armor', ['tank'], 'When attacked, deals 50 + 5% of its max HP as magic damage to adjacent enemies (1 second cooldown).', 'Spiky Shell'),
    P('glacier', 'Glacier Mode', ['ad'], 'Gains 15% attack speed. Attacks slow the target\'s attack speed by 30% for 2 seconds.', 'Frost Touch'),
    P('inferno', 'Inferno Mode', ['ap'], 'Its abilities burn their targets, and it deals 15% more damage to burning enemies.', 'Inner Fire'),
    P('hunter', 'Hunter Mode', ['ad', 'ap'], 'Deals 8% more damage, and 30% more to enemies below 50% HP.', 'Killer Instinct'),
    P('guardian', 'Guardian Mode', ['tank'], 'Gains 30 armor and MR. Adjacent allies gain 10 armor and MR.', 'Resistant'),
    P('ascend', 'Ascension', ['ad', 'ap'], 'After 6 seconds of combat, deals 40% more damage.', 'Final Ascent'),
  ];

  // Forms picked when the Digimon is bought (Stance Master, Monster Tamer). Effects in coliseum-mech.js.
  const FORMS = {
    col_justimon: [
      { id: 'blitz', name: 'Blitz Arm', trait: 'DUE', desc: 'Duelist stance. Gains the Duelist trait and 30% attack speed. Its ability strikes the 2 nearest enemies several times and cuts their armor.' },
      { id: 'critical', name: 'Critical Arm', trait: 'EXE', desc: 'Executioner stance. Gains the Executioner trait and 20% crit chance. Its ability hits the lowest-health enemy within 3 hexes, stuns it for 1.5 seconds and splashes adjacent enemies.' },
      { id: 'trinity', name: 'Trinity Arm', trait: 'JUG', desc: 'Juggernaut stance. Gains the Juggernaut trait and 400 HP. Its ability takes 50% less damage for 2 seconds, then smashes its target and everything around it.' },
    ],
    col_labramon: [
      { id: 'ballista', name: 'Ballistamon', desc: 'Tank partner. 20% more HP and 15 armor and MR. Its ability gains a shield, taunts nearby enemies and stuns them briefly.' },
      { id: 'dorulu', name: 'Dorulumon', desc: 'Diver partner. 30% more attack damage and 20% omnivamp. Its ability drills through to the lowest-health enemy, double damage below 50% HP.' },
      { id: 'sparrow', name: 'Sparrowmon', desc: 'Ranged partner. Attacks from 4 hexes with 25% more attack speed. Its ability gives it 50% attack speed for 5 seconds and fires at 3 enemies.' },
    ],
  };

  // ---------- augments ----------
  const A = (id, tier, name, desc, tags) => ({ id, tier, name, desc, tags });
  const augments = [
    // Silver
    A('col_a_coins', 1, 'Pocket Change', 'Gain 8 gold.', ['econ']),
    A('col_a_band', 1, 'Band of Thieves', 'Gain a Thief Glove.', ['items']),
    A('col_a_cache', 1, 'Component Cache', 'Gain 2 random components.', ['items']),
    A('col_a_rolls', 1, 'Rolling Start', 'Gain 1 free reroll every round.', ['econ']),
    A('col_a_heart', 1, 'Cheering Crowd', 'Gain 5 Tamer HP.', ['econ']),
    A('col_a_warmup', 1, 'Warm-Up', 'Your team gains 12% AD and 12 AP.', ['combat']),
    A('col_a_front', 1, 'Front Row Seats', 'Digimon in your front row gain 30 armor and MR and 200 HP.', ['combat']),
    A('col_a_ringside', 1, 'Ringside Snipers', 'Digimon in your back two rows gain 25% attack speed.', ['combat']),
    A('col_a_snack', 1, 'Extra Snack', 'Gain a Digi-Core and a random component.', ['items']),
    A('col_a_second', 1, 'Second Wind', 'After 10 seconds of combat, your team heals 35% of its missing HP.', ['combat']),
    // Gold
    A('col_a_prize', 2, 'Prize Money', 'Gain 18 gold.', ['econ']),
    A('col_a_trove', 2, 'Treasure Trove', 'Gain a random completed item and a component.', ['items']),
    A('col_a_ascension', 2, 'Ascension', 'After 15 seconds of combat, your team deals 40% more damage.', ['combat']),
    A('col_a_sponsor', 2, 'Sponsorship', 'Gain 2 extra gold every round.', ['econ']),
    A('col_a_crest', 2, 'Gym Crest', 'Gain a random Crest for a trait on your board.', ['items']),
    A('col_a_tags', 2, 'Tag Team', 'Gain a Crest Tag and a Digi-Egg.', ['items']),
    A('col_a_snacktime', 2, 'Snack Time', 'Gain a Digi-Core and 6 gold.', ['items']),
    A('col_a_lotus', 2, 'Jeweled Lotus', 'Your abilities can critically strike, and your team gains 15% crit.', ['combat']),
    A('col_a_unity', 2, 'Stand United', 'Your team gains 3% AD and 3 AP for every active trait.', ['combat']),
    A('col_a_titan', 2, 'Heavy Training', 'Your team gains 15% max HP.', ['combat']),
    A('col_a_evolve', 2, 'Instant Digivolve', 'Gain a random two-star Digimon of cost 2 or 3.', ['econ']),
    A('col_a_artifact', 2, 'Relic Hunter', 'Gain a random Artifact.', ['items']),
    // Prismatic
    A('col_a_jackpot', 3, 'Grand Prize', 'Gain 35 gold.', ['econ']),
    A('col_a_armory', 3, 'Big Armory', 'Gain 2 random completed items.', ['items']),
    A('col_a_golden', 3, 'Golden Egg', 'Gain a Golden Digi-Egg (+1 team size).', ['items']),
    A('col_a_mega', 3, 'Mega Digivolve', 'Gain a random two-star 4-cost Digimon.', ['econ']),
    A('col_a_giant', 3, 'Giant Grab', 'Your team gains 25% max HP.', ['combat']),
    A('col_a_feast', 3, 'Digi-Core Feast', 'Gain 2 Digi-Cores and a random completed item.', ['items']),
    A('col_a_arena', 3, 'Spirit of the Arena', 'Every active origin trait counts one extra Digimon.', ['combat']),
    A('col_a_finals', 3, 'Finals Night', 'Your team deals 12% more damage and takes 10% less damage.', ['combat']),
  ];

  // ---------- codex rules ----------
  const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;');
  const codexRules = (rules, pace) => [
    { id: 'col_core', title: 'Digi-Cores', html: `<p>At the start of rounds ${((rules && rules.coreRounds) || ['1-3', '3-3']).join(' and ')} every Tamer gets a Digi-Core. Drag it onto a Digimon to pick 1 of 3 digivolution paths for it. Attack carries, casters and front liners get different choices. The path stays when the Digimon digivolves. A new Digi-Core replaces the old path. Selling the Digimon, or a Data Extractor, gives the Digi-Core back.</p><ul>${POWER_UPS.map((x) => `<li><b>${esc(x.name)}</b>: ${esc(x.desc)}</li>`).join('')}</ul>` },
    { id: 'col_forms', title: 'Stances and partners', html: `<p>When you get Justimon or Labramon you pick its form. Until you pick, it fights with the first one.</p><ul>${Object.values(FORMS).flat().map((f) => `<li><b>${esc(f.name)}</b>: ${esc(f.desc)}</li>`).join('')}</ul>` },
    { id: 'col_gambit', title: 'Jewel Gambit', html: `<p>${esc(TRAITS.CG.desc)}</p><p>${esc(TRAITS.CG.notes)}</p>` },
    { id: 'col_rounds', title: 'Rounds', html: pace === 'normal' ? '<p>Normal pace: standard TFT rules (100 Tamer HP, XP, interest and streaks). Wild rounds at 1-2 to 1-4 and x-7 use the same tournament brackets as Hyper Roll.</p>' : '<p>Hyper Roll rules: 20 Tamer HP, levels set by the stage, fixed income (3/5/6/7/8 by stage), 2 gold per player win, rerolls cost 1. Stage 1: opening carousel, then two wild rounds. Later stages: three player rounds and a wild round at x-4, where the tournament bracket sends a stronger champion each time (Kimeramon, then Darkdramon, then the Royal Knights). Armory at x-1 of stages 2-6, augments at 2-2, 3-2 and 4-2.</p>' },
  ];

  DT.Sets.register({
    id: 'coliseum',
    name: 'K.O. Coliseum',
    mode: 'tft',
    nativePace: 'hyper',
    order: 8,
    desc: 'A Digimon fighting tournament. Feed Digi-Cores to pick digivolution paths, choose Justimon\'s arm and Labramon\'s partner, and bank gems with Jewel Gambit. Plays at Hyper Roll or Normal pace.',
    colors: { main: '#ff5a3d', accent: '#ffd84a' },
    source: 'TFT Set 15 K.O. Coliseum (61 shop champions as 60 lines, 26 traits, Power Ups). docs/inspo/tft/set-15-ko-coliseum.md, docs/sets/coliseum.md',
    roles: ROLES,
    traits: TRAITS,
    lines: LINES,
    pve: pveWave,
    items: {
      system: 'tft',
      components,
      recipes,
      special: [
        { id: 'col_golden_egg', name: 'Golden Digi-Egg', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {}, from: ['col_egg', 'col_egg'], color: '#ffd84a', color2: '#fff3b0', short: 'GE' },
        { id: 'col_golden_tag', name: 'Golden Tag', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {}, from: ['col_tag', 'col_tag'], color: '#ffb84a', color2: '#ffe9a0', short: 'GT' },
        { id: 'col_digivice', name: 'Digivice', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {}, from: ['col_egg', 'col_tag'], color: '#7dffb0', color2: '#eef1f6', short: 'DV' },
      ],
      list: crests.concat(artifacts, consumables),
      aiRoles,
    },
    augments,
    powerUps: POWER_UPS,
    forms: FORMS,
    rules: {
      startHp: 20,
      startGold: 3,
      rerollCost: 1,
      winGold: 2,
      shopSize: 5,
      poolSize: [0, 30, 25, 18, 10, 9],
      levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
      incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
      lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
      pveLossDamage: (stage) => (stage === 1 ? 1 : null),
      armoryStages: [2, 3, 4, 5, 6],
      augmentStages: [2, 3, 4],
      coreRounds: ['1-3', '3-3'],
      // standard TFT pace: the corpus timing for Power Snax (1-3 and 3-6); everything else uses the engine's Normal rules
      normal: { coreRounds: ['1-3', '3-6'] },
    },
    codexRules,
    tamers: ['Mikey', 'Nene', 'Taiki', 'Kiriha', 'Ryouma', 'Tagiru', 'Yuu'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
