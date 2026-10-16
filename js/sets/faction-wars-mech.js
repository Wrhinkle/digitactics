// Faction Wars behavior: trait, augment and consumable hooks plus the Data Wisps mechanic. Ported from the old
// hardcoded logic in combat.js and game.js with the same order of operations, so results are identical.
// Hook contexts are documented in docs/sets/FORMAT.md. Combat hooks: c.members, c.mine, c.v, c.lv, c.val(key),
// c.state (per source, per combat), u.state (per unit). Game hooks: c.game, c.player, c.give(), c.pick() ...
(function (G) {
  const DT = G.DT;

  // ---------- traits ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });

  // DATS: heal and power up once, on the first ally knockout or at 10 seconds
  function digiSoul(c) {
    const d = c.state;
    d.done = true;
    for (const a of c.survivors(c.team)) {
      if (!a.traits.includes('DATS')) continue;
      c.heal(a, a.maxHp * d.heal);
      a.ad += a.baseAd * d.ad;
      a.ap += Math.round(d.ad * 100);
      c.text(a, 'DigiSoul');
    }
  }

  // Insectoid: bites from the living Insectoids; a bitten enemy misses its next basic attack
  function swarm(c) {
    const ins = c.state;
    const srcs = c.survivors(c.team).filter((u) => u.traits.includes('INS'));
    if (!srcs.length) return;
    for (let k = 0; k < ins.n; k++) {
      const pool = c.survivors(1 - c.team);
      if (!pool.length) break;
      const e = pool[Math.floor(c.rng() * pool.length)];
      const s = srcs[k % srcs.length];
      c.event({ type: 'shot', from: s.id, to: e.id });
      c.damage(s, e, ins.dmg, 'magic');
      e.blind = true;
    }
  }

  function besideKnight(c, u) {
    for (const a of c.adjacent(u)) if (a !== u && a.alive && a.team === u.team && a.state.knight) return true;
    return false;
  }

  const traits = {
    DR: { hooks: each((u, c) => { u.maxHp += c.v; }) },
    NSp: { hooks: each((u, c) => { u.as *= 1 + c.v; }) },
    JT: { hooks: each((u, c) => { u.regen += c.v; }) },
    WG: { hooks: all((u, c) => { u.dodge += c.v; }) },
    DS: { hooks: each((u, c) => { u.shield += u.maxHp * c.v; }) },
    ME: { hooks: each((u, c) => { u.dr += c.v; }) },
    NSo: { hooks: each((u, c) => { u.omnivamp += c.v; }) },
    VB: { hooks: all((u, c) => { u.mana = Math.min(u.maxMana - 1, u.mana + c.v); }) },
    DL: { hooks: each((u, c) => { u.ap += c.v; if (c.lv >= 2) u.amp += c.lv >= 3 ? 0.3 : 0.15; if (c.lv >= 3) u.abilBurn = true; }) },
    RK: {
      // reads the finished stats of the team, after Xros Fusion
      hooks: {
        onCombatReady: {
          order: 1,
          fn(c) {
            const v = c.v * c.activeTraits(c.team);
            for (const u of c.mine) if (u.traits.includes('RK')) { u.maxHp = Math.round(u.maxHp * (1 + v)); u.amp += v; }
          },
        },
      },
      ai: { bonus: (a) => a.active }, // likes many active traits
    },
    DATS: {
      hooks: {
        onCombatStart(c) { c.state.ad = c.v; c.state.heal = c.val('heal'); c.state.done = false; },
        onTick: { order: 1, fn(c) { if (!c.state.done && c.t >= 10) digiSoul(c); } },
        onDeath(c) { if (!c.state.done) digiSoul(c); },
      },
    },
    LW: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.lw = c.v; u.state.lwDr = c.val('dr'); } },
        // Spirit Evolution: once, below 50% HP
        onDamageTaken(c, e) {
          const u = e.unit;
          const s = u.state;
          if (!s.lw || s.lwUsed || u.hp >= u.maxHp * 0.5) return;
          s.lwUsed = true;
          c.heal(u, u.maxHp * s.lw);
          u.buffs.push({ stat: 'as', v: s.lw, until: Infinity });
          u.dr += s.lwDr;
          c.text(u, 'Spirit Evolution');
          c.event({ type: 'shield', at: u.id });
        },
      },
    },
    XF: {
      // DigiXros: shares max HP and AD; reads finished stats, before Royal Knights
      hooks: {
        onCombatReady: {
          order: 0,
          fn(c) {
            const mem = c.mine.filter((u) => u.traits.includes('XF'));
            const hp = mem.reduce((s, u) => s + u.maxHp, 0);
            const ad = mem.reduce((s, u) => s + u.ad, 0);
            for (const u of mem) {
              const addHp = (hp - u.maxHp) * c.v;
              const addAd = (ad - u.ad) * c.v;
              u.maxHp = Math.round(u.maxHp + addHp);
              u.ad += addAd;
            }
          },
        },
      },
    },
    INS: {
      hooks: {
        onCombatStart(c) { c.state.n = c.lv; c.state.dmg = c.v; c.state.next = 2; },
        onTick: { order: 2, fn(c) { if (c.t >= c.state.next) { c.state.next += 2; swarm(c); } } },
      },
    },
    FUN: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.muck = c.v; },
        // every second basic attack throws muck: the target deals less damage for 3 seconds
        onAttack(c, e) {
          const u = e.unit, tg = e.target;
          if (!u.state.muck || u.nAtk % 2 !== 0 || !tg.alive) return;
          tg.weak = Math.max(tg.weakUntil > c.t ? tg.weak : 0, u.state.muck);
          tg.weakUntil = c.t + 3;
          c.text(tg, 'Muck');
        },
        // game hook: gold after every combat
        onRoundEnd(c, e) { e.gold += c.val('gold'); },
      },
      ai: { bonus: (a) => (a.stage <= 3 ? 2 : 0) }, // pays gold early
    },
    KN: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.knight = c.v; },
        onBeforeDamageTaken(c, e) { const k = e.target.state.knight; if (k) e.dr += k * (besideKnight(c, e.target) ? 2 : 1); },
      },
    },
    FL: { hooks: each((u, c) => { u.abilBurn = true; u.vsBurningAmp = c.v; }) },
    FR: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.frost = c.v; },
        onAttack(c, e) {
          const u = e.unit, tg = e.target;
          if (u.state.frost && tg.alive && c.rng() < u.state.frost) { c.stun(tg, 1); c.text(tg, 'Frozen'); }
        },
      },
    },
    UND: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.undead = c.v; },
        // cheat death once: 1 HP, untouchable for 1 second, then heal
        onLethal(c, e) {
          const u = e.unit;
          if (!u.state.undead || u.state.undeadUsed) return;
          u.state.undeadUsed = true;
          u.hp = 1;
          u.invulnUntil = c.t + 1;
          c.after(1, () => c.heal(u, u.maxHp * u.state.undead), u);
          c.text(u, 'Undying');
        },
      },
    },
    HB: { hooks: all((u, c) => { const k = u.traits.includes('HB') ? 2 : 1; u.armor += c.v * k; u.mr += c.v * k; u.ap += c.v * k; }) },
    FA: { hooks: { onCombatStart(c) { for (const e of c.units) if (e.team !== c.team) { e.armor -= c.v; e.mr -= c.v; } } } },
    Guardian: { hooks: each((u, c) => { u.armor += c.v; u.mr += c.v; }) },
    Striker: { hooks: each((u, c) => { u.ad *= 1 + c.v; }) },
    Sniper: { hooks: each((u, c) => { u.perHexAmp = c.v; }) },
    Mystic: { hooks: each((u, c) => { u.ap += c.v; }) },
    Assassin: { hooks: each((u, c) => { u.crit += c.v; }) },
    Bruiser: { hooks: each((u, c) => { u.maxHp = Math.round(u.maxHp * (1 + c.v)); }) },
    Duelist: { hooks: each((u, c) => { u.atkAs = c.v; }) },
    Support: {
      hooks: {
        onCombatStart(c) { c.teamState(c.team).healAmp = c.v; for (const u of c.members) u.state.support = c.val('heal'); },
        // every 4 seconds each Support heals the lowest-health ally
        onUnitTick(c, e) {
          const u = e.unit;
          if (!u.state.support || !c.every(u, 'support', 4)) return;
          let best = null;
          for (const a of c.units) if (a.alive && a.team === u.team && (!best || a.hp / a.maxHp < best.hp / best.maxHp)) best = a;
          if (best) c.heal(best, best.maxHp * u.state.support);
        },
      },
    },
  };

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const augments = {
    // instant effects (game hooks)
    pocket: { hooks: { onPick(c) { c.player.gold += 8; } } },
    bank: { hooks: { onPick(c) { c.player.gold += 18; } } },
    jackpot: { hooks: { onPick(c) { c.player.gold += 35; } } },
    cache: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    trove: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomComponent()); } } },
    armory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    egg: { hooks: { onPick(c) { c.give('egg'); } } },
    goldenEgg: { hooks: { onPick(c) { c.give('golden_egg'); } } },
    heart: { hooks: { onPick(c) { c.player.hp += 5; } } },
    rolls: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    rolls2: { hooks: { onPick(c) { c.player.freeRolls += 2; }, onRoundStart(c) { c.player.freeRolls += 2; } } },
    income: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    evolve: { hooks: { onPick(c) { c.giveUnit([2, 3], 1); } } },
    megaEvolve: { hooks: { onPick(c) { c.giveUnit([4], 1); } } },
    // combat effects
    training: { hooks: team((u) => { u.ad += u.baseAd * 0.12; u.ap += 12; }) },
    blessing: { hooks: team((u) => { u.omnivamp += 0.12; }) },
    lotus: { hooks: team((u) => { u.abilCrit = true; u.crit += 0.15; }) },
    implants: { hooks: team((u) => { if (u.items.length) { u.maxHp += 250; u.ad += u.baseAd * 0.15; } }) },
    unity: { hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of c.mine) { u.ad += u.baseAd * 0.03 * n; u.ap += 3 * n; } } } },
    featherweights: { hooks: team((u) => { if (u.line.tier <= 2) u.as *= 1.25; }) },
    frontline: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 30; u.mr += 30; u.maxHp += 200; } }) },
    backline: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.25; }) },
    overcharge: { hooks: team((u) => { u.mana += 25; }) },
    giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    thrill: { hooks: { onKill(c, e) { if (e.unit.alive) c.heal(e.unit, 350); } } },
    secondWind: {
      hooks: {
        onTick: {
          order: -1, // before the DigiSoul and Insectoid timers, as before the port
          fn(c) {
            if (c.state.done || c.t < 10) return;
            c.state.done = true;
            for (const u of c.survivors(c.team)) c.heal(u, (u.maxHp - u.hp) * 0.35);
          },
        },
      },
    },
    // Crest of Courage: every active family trait counts one extra Digimon
    crest: {
      hooks: {
        onCountTraits(counts) { for (const t in counts) if (DT.TRAITS[t].kind === 'family' && counts[t] >= DT.TRAITS[t].bp[0]) counts[t] += 1; },
      },
      ai: { bonus: (c) => (Object.entries(DT.countTraits(c.player.boardUnits())).some(([t, n]) => DT.TRAITS[t].kind === 'family' && n >= 2) ? 3 : 0) },
    },
  };

  // ---------- consumables ----------
  // Magnetic Remover / Reforger: the unit's items go back to the item bench (Reforger rerolls them first).
  function giveBack(c, u, idx, reforge) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.game.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    let back = u.items;
    if (reforge) {
      back = back.map((x) => {
        if (DT.isComponent(x)) return x === 'egg' ? 'egg' : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (DT.ITEMS[x].emblem) return c.pick(Object.keys(DT.ITEMS).filter((k) => DT.ITEMS[k].emblem && k !== x));
        if (x === 'golden_egg') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  const items = {
    remover: { use: (c, u, idx) => giveBack(c, u, idx, false) },
    reforger: { use: (c, u, idx) => giveBack(c, u, idx, true) },
  };

  // ---------- Data Wisps ----------
  // Every other round from stage 2 a one-use Wisp sits in the sixth shop slot (p.wisp, drawn by the UI).
  function buyWisp(c) {
    const p = c.player;
    const g = c.game;
    const w = p.wisp;
    if (!w) return 'none';
    if (p.gold < w.cost) return 'gold';
    p.gold -= w.cost;
    p.wisp = null;
    const buffs = () => (p.nextBuffs = p.nextBuffs || {});
    switch (w.id) {
      case 'gold': p.gold += w.cost * 2; break;
      case 'comp': c.give(c.randomComponent()); break;
      case 'egg': c.give('egg'); break;
      case 'copy': {
        const rookies = p.allUnits().filter((u) => u.stage === 0 && g.pool[u.line] > 0);
        const free = p.freeBenchSlot();
        if (rookies.length && free >= 0) {
          const src = c.pick(rookies);
          g.takeFromPool(src.line);
          p.bench[free] = g.newUnit(src.line, 0);
          g.combine(p);
        } else p.gold += w.cost;
        break;
      }
      case 'evolve': {
        const rookies = p.boardUnits().filter((u) => u.stage === 0 && g.pool[u.line] >= 2).sort((a, b) => DT.LINE[a.line].tier - DT.LINE[b.line].tier);
        if (rookies.length) {
          rookies[0].stage = 1;
          g.takeFromPool(rookies[0].line, 2);
          g.combine(p);
        } else p.gold += w.cost;
        break;
      }
      case 'level': p.teamBonus += 1; break;
      case 'battle': buffs().as = (buffs().as || 0) + 0.3; break;
      case 'iron': buffs().armor = (buffs().armor || 0) + 35; break;
      case 'mana': buffs().mana = (buffs().mana || 0) + 40; break;
      case 'heal': p.hp += 3; break;
      case 'gamble': if (c.rng() < 0.5) p.gold += w.cost * 3; break;
      case 'pact': p.hp -= 3; c.give(c.randomCompleted()); break;
      case 'rolls': p.freeRolls += 4; break;
      case 'fortune': g.refreshShop(p, 2); break;
      case 'crest': c.give(g.randomCrest(p)); break;
    }
    return w.id;
  }

  const mechanic = {
    id: 'wisps',
    name: 'Data Wisps',
    desc: 'Every other round from stage 2, a one-use Data Wisp sits in the sixth shop slot. It costs its base price plus the stage number and leaves at the end of the round.',
    hooks: {
      onRoundStart(c) {
        const p = c.player;
        p.wisp = null;
        if (c.stage < 2 || c.round % 2 !== 0) return;
        const w = c.pick(DT.WISPS.filter((x) => x.minStage <= c.stage));
        p.wisp = { id: w.id, cost: DT.wispCost(w, c.stage) };
      },
      onAiTurn(c) {
        const p = c.player;
        if (p.wisp && p.gold >= p.wisp.cost + 2 && c.rng() < 0.5) buyWisp(c);
      },
    },
    actions: {
      wisp: (c) => (c.game.phase === 'planning' ? buyWisp(c) : 'phase'),
    },
  };

  DT.Sets.extend('faction-wars', { traits, augments, items, mechanic });
})(typeof window !== 'undefined' ? window : globalThis);
