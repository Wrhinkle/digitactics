// Auto Chess behavior: race and class hooks, the count rules (Wizard, Demon, Olympos), item and relic hooks, the
// Digi-Egg wildcard, creep loot and relic offers, the round-based income and three ability types (summon, transform,
// storm). Contexts are documented in docs/sets/FORMAT.md. Everything random goes through c.rng() / c.pick().
(function (G) {
  const DT = G.DT;
  const SET = 'autochess';
  // rounds played (pace-aware: Hyper Roll has 4-round stages); loot tiers use the Normal-pace equivalent
  const roundNo = (g) => (g.roundNo ? g.roundNo() : (g.stage - 1) * 5 + g.round);
  const lootRound = (g) => (g.pace === 'hyper' ? Math.round(roundNo(g) * 1.25) : roundNo(g));
  const isRace = (t) => DT.TRAITS[t] && DT.TRAITS[t].kind === 'race';

  // ---------- count rules ----------
  // Wizard (2): every other trait that needs 4+ units needs one fewer. Demon: only one kind of Demon (or 2 Demon
  // Slayers). Olympos: only while no other race is active. Counts are changed in place (onCountTraits).
  function countRules(counts) {
    if ((counts.WIZARD || 0) >= 2) {
      for (const t in counts) {
        if (t === 'WIZARD') continue;
        const T = DT.TRAITS[t];
        const n = counts[t];
        if (T && T.bp.some((b) => b >= 4 && n === b - 1)) counts[t] = n + 1;
      }
    }
    if ((counts.DEMON || 0) >= 2 && (counts.DH || 0) < 2) delete counts.DEMON;
    if (counts.OLYMPOS) {
      const other = Object.keys(counts).some((t) => t !== 'OLYMPOS' && isRace(t) && DT.traitLevel(t, counts[t]) > 0);
      if (other) delete counts.OLYMPOS;
    }
  }

  // ---------- races and classes ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });

  const traits = {
    BEAST: { hooks: all((u, c) => { u.ad += u.baseAd * c.v; }) },
    DRAGON: { hooks: each((u, c) => { u.mana = Math.max(u.mana, Math.round(u.maxMana * c.v)); }) },
    UNDEAD: { hooks: { onCombatStart(c) { for (const e of c.units) if (e.team !== c.team) e.armor -= c.v; } } },
    DEMON: {
      hooks: {
        onAttack(c, e) {
          if (!c.isMember(e.unit) || !e.target.alive) return;
          if (c.traitLevel(1 - c.team, 'DH') >= 1) return; // an enemy Demon Slayer cancels Fel Power
          c.damage(e.unit, e.target, e.dmg * c.v, 'true');
        },
      },
      ai: { bonus: (a) => ((a.n >= 1 && (a.counts.DH || 0) < 2) ? -8 : 2) },
    },
    FAIRY: { hooks: each((u, c) => { u.dodge += c.v; }) },
    HUMAN: {
      hooks: {
        onAttack(c, e) {
          if (!c.isMember(e.unit) || !e.target.alive) return;
          if (!(e.target.silenceUntil > c.t) && c.rng() < c.v) c.silence(e.target, 3);
        },
      },
    },
    MACHINE: {
      hooks: {
        onCombatStart(c) {
          const list = c.lv >= 2 ? c.mine : c.mine.length ? [c.mine[Math.floor(c.rng() * c.mine.length)]] : [];
          for (const u of list) { u.armor += c.v; u.regen += c.val('regen'); }
        },
      },
    },
    DINO: { hooks: each((u, c) => { u.maxHp += c.v; }) },
    INSECT: {
      hooks: {
        onCombatStart(c) {
          const allAs = c.val('all') || 0;
          for (const u of c.mine) {
            let m = 1 + allAs;
            if (c.isMember(u)) m += c.v;
            u.as *= m;
          }
        },
      },
    },
    ELEMENTAL: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.armor += c.val('armor'); u.mr += c.val('armor'); } },
        onDamageTaken(c, e) {
          const u = e.unit, s = e.source;
          if (!(c.lv >= 2 || c.isMember(u))) return;
          if (!s || !s.alive || s.team === u.team || s.range > 1 || e.kind !== 'phys' || e.isBurn) return;
          if ((s.state.acStoneCd || 0) > c.t) return;
          if (c.rng() < c.v) { s.state.acStoneCd = c.t + 2.5; c.stun(s, 1.5); c.text(s, 'Stone'); }
        },
      },
    },
    AQUAN: { hooks: all((u, c) => { u.mr += c.v; }) },
    CYBORG: { hooks: each((u, c) => { u.range += 1; u.ad += u.baseAd * c.v; }) },
    OLYMPOS: {
      hooks: all((u, c) => { u.P.shojin = (u.P.shojin || 0) + c.v; u.mana += c.val('mana'); }),
      ai: { bonus: (a) => (Object.keys(a.counts).some((t) => t !== 'OLYMPOS' && isRace(t) && DT.traitLevel(t, a.counts[t]) > 0) ? -6 : 1) },
    },
    WARRIOR: { hooks: each((u, c) => { u.armor += c.v; }) },
    MAGE: { hooks: { onBeforeDamage(c, e) { if (e.kind === 'magic') e.mult *= 1 + c.v; } } },
    HUNTER: { hooks: each((u, c) => { u.ad += u.baseAd * c.v; }) },
    ASSASSIN: { hooks: each((u, c) => { u.crit += c.v; u.critDmg = Math.max(u.critDmg, c.val('cd')); }) },
    KNIGHT: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || (u.state.acKnCd || 0) > c.t) return;
          let n = 0;
          for (let k = 0; k < c.v; k++) if (c.rng() < 0.3) n++;
          if (!n) { u.state.acKnCd = c.t + 0.5; return; }
          u.state.acKnCd = c.t + 4;
          c.buff(u, 'dr', 0.18 * n, 3);
          c.text(u, n > 1 ? `Divine Shield x${n}` : 'Divine Shield');
          c.event({ type: 'shield', at: u.id });
        },
      },
    },
    DRUID: {
      ai: { bonus: (a) => (a.n >= 1 && a.n < 4 ? 4 : 0) },
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.acDruid = c.v; },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!u.state.acDruid || !c.every(u, 'acDruid', 4)) return;
          let best = null;
          for (const a of c.units) if (a.alive && a.team === u.team && (!best || a.hp / a.maxHp < best.hp / best.maxHp)) best = a;
          if (best) c.heal(best, best.maxHp * u.state.acDruid);
        },
      },
    },
    MECH: { hooks: each((u, c) => { u.regen += c.v; }) },
    SHAMAN: {
      hooks: {
        onCombatStart(c) {
          const foes = c.units.filter((e) => e.team !== c.team && e.alive);
          if (!foes.length) return;
          const e = foes[Math.floor(c.rng() * foes.length)];
          c.stun(e, c.v);
          c.text(e, 'Hexed');
        },
      },
    },
    WARLOCK: { hooks: all((u, c) => { u.omnivamp += c.v; }) },
    DH: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.as *= 1 + c.val('as'); },
        onBeforeDamage(c, e) { if (c.isMember(e.unit) && e.target.traits.includes('DEMON')) e.mult *= 1 + c.v; },
      },
    },
    PRIEST: {
      hooks: {
        onCombatStart(c) { const ts = c.teamState(c.team); ts.healAmp = (ts.healAmp || 0) + c.val('heal'); },
        // game hook: after a lost round, give back part of the damage before it is taken
        onRoundEnd(c, e) {
          if (e.won || !e.match || !e.match.combat) return;
          const d = c.game.lossFor(e.match.combat, 1 - e.team, e.pve);
          c.player.hp += Math.floor(d * c.v);
        },
      },
    },
    WIZARD: {
      hooks: each((u, c) => { u.ap += c.v; }),
      ai: { bonus: (a) => (Object.keys(a.counts).some((t) => t !== 'WIZARD' && DT.TRAITS[t] && DT.TRAITS[t].bp.some((b) => b >= 4 && a.counts[t] === b - 2)) ? 2 : 0) },
    },
  };

  // ---------- items ----------
  const items = {
    ac_recovery_disk: { p: { regen: 0.012 } },
    ac_digizoid_armor: {
      hooks: {
        onCombatStart(c) {
          const u = c.unit;
          for (const a of c.alliesNear(u.team, u, 2)) { a.armor += 20; a.as *= 1.15; }
          for (const e of c.enemiesNear(u.team, u, 3)) e.armor -= 20;
        },
      },
    },
    ac_reflect_mail: {
      hooks: {
        onDamageTaken(c, e) {
          const s = e.source;
          if (e.kind !== 'phys' || e.isBurn || !s || !s.alive || s === c.unit || s.team === c.unit.team) return;
          c.damage(c.unit, s, e.amount * 0.3, 'magic');
        },
      },
    },
    ac_refresh_disk: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (u !== c.unit || u.state.acRefresh) return;
          u.state.acRefresh = true;
          u.mana = u.maxMana;
          c.text(u, 'Refresh');
        },
      },
    },
    ac_numemon_wand: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'acWand', 7)) return;
          const foes = c.enemiesOf(u);
          if (!foes.length) return;
          const t = foes.reduce((b, x) => (x.line.tier * 10 + x.stage * 4 > b.line.tier * 10 + b.stage * 4 ? x : b));
          c.event({ type: 'beam', from: u.id, to: t.id });
          c.stun(t, 2.5);
          c.text(t, 'Numemon!');
        },
      },
    },
    ac_true_lance: { hooks: { onAttack(c, e) { if (e.unit === c.unit && e.target.alive) c.damage(c.unit, e.target, 60, 'magic'); } } },
    ac_hellfire_rod: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'acRod', 6)) return;
          const t = c.target(u, 'random');
          if (!t) return;
          c.event({ type: 'beam', from: u.id, to: t.id });
          c.damage(u, t, 250, 'magic');
        },
      },
    },
    ac_stun_hammer: { hooks: { onAttack(c, e) { if (e.unit === c.unit && e.target.alive && c.rng() < 0.15) { c.stun(e.target, 1.2); c.text(e.target, 'Bash'); } } } },
    ac_frost_ring: { hooks: { onAttack(c, e) { if (e.unit === c.unit && e.target.alive) c.buff(e.target, 'as', -0.25, 2); } } },
    ac_dragon_lance: { hooks: { onCombatStart(c) { if (c.unit.range > 1) c.unit.range += 1; } } },
    ac_ward_cape: { hooks: { onCombatStart(c) { for (const a of c.alliesNear(c.unit.team, c.unit, 2)) a.mr += 25; } } },

    // Digi-Egg: one more copy of the target's form, taken from the pool, then digivolve
    ac_digiegg: {
      use(c, unit, idx) {
        const g = c.game, p = c.player;
        const L = DT.LINE[unit.line];
        if (!L || !L.shop) return 'none';
        const max = g.R.maxStage != null ? g.R.maxStage : 2;
        if (unit.stage >= max) return 'maxed';
        const need = Math.pow(g.copiesOf(unit.line), unit.stage);
        if ((g.pool[unit.line] || 0) < need) return 'poolEmpty';
        const free = p.freeBenchSlot();
        if (free < 0) {
          const same = p.allUnits().filter((u) => u.line === unit.line && u.stage === unit.stage).length;
          if (same < g.copiesOf(unit.line) - 1) return 'benchFull';
        }
        g.takeFromPool(unit.line, need);
        p.items.splice(idx, 1);
        const u = g.newUnit(unit.line, unit.stage);
        if (free >= 0) { p.bench[free] = u; g.combine(p); } else g.combine(p, u);
        return 'ac_digiegg';
      },
    },

    // relics (global items): game hooks for the owner, combat hooks for the owner's team
    ac_r_treasure: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    ac_r_forge: { onGet(c) { c.give(c.randomCompleted()); } },
    ac_r_vault: { hooks: { onIncome(c, e) { e.interest = Math.min(Math.floor(Math.max(0, c.player.gold) / 10), 8); } } },
    ac_r_charm: { hooks: { onIncome(c, e) { e.streak *= 2; } } },
    ac_r_recruiter: { hooks: { onShop(c, e) { e.level = Math.min(10, e.level + 1); } } },
    ac_r_grace: { hooks: { onCombatStart(c) { for (const u of c.mine) u.regen += 0.012; } } },
    ac_r_reaper: { hooks: { onCombatStart(c) { for (const u of c.mine) u.amp += 0.1; } } },
    ac_r_eggnest: { onGet(c) { c.give('ac_digiegg'); c.give('ac_digiegg'); } },
    ac_r_card: { onGet(c) { c.player.freeRolls += 1; }, hooks: { onRoundStart(c) { c.player.freeRolls += 1; } } },
    ac_r_key: { hooks: { onTeamSize(c, e) { e.size += 1; } } },
    ac_r_discount: { hooks: { onSell(c) { c.player.gold += 1; } } },
  };

  // ---------- creep loot ----------
  // Tier odds by creep round (Dota Auto Chess 2019 drop table, compressed to one item per round). Tier 4 = a finished item.
  function tierWeights(n) {
    if (n <= 3) return [100, 0, 0, 0];
    if (n <= 10) return [60, 40, 0, 0];
    if (n <= 15) return [30, 50, 20, 0];
    if (n <= 20) return [20, 40, 35, 5];
    if (n <= 25) return [5, 35, 40, 20];
    if (n <= 30) return [0, 25, 45, 30];
    return [0, 10, 45, 45];
  }
  function rollLoot(c, n) {
    if (n >= 15 && c.rng() < 0.05) return 'ac_digiegg';
    const w = tierWeights(n);
    let x = c.rng() * 100;
    let tier = 1;
    for (let i = 0; i < 4; i++) { if (x < w[i]) { tier = i + 1; break; } x -= w[i]; }
    if (tier === 4) return c.randomCompleted();
    const parts = DT.COMPONENT_IDS.filter((id) => DT.ITEMS[id].tier === tier);
    return c.pick(parts.length ? parts : DT.COMPONENT_IDS);
  }
  function lootOptions(c, n) {
    const out = [];
    for (let guard = 0; out.length < 3 && guard < 40; guard++) {
      const id = rollLoot(c, n);
      if (id && !out.includes(id)) out.push(id);
    }
    return out;
  }
  const reward = (c, won) => {
    const n = lootRound(c.game);
    if (won) { c.player.mech.loot = lootOptions(c, n); return []; }
    c.player.mech.loot = null;
    const id = rollLoot(c, n);
    return id ? [{ kind: 'blue', items: [id] }] : [];
  };

  // AI value of an item for this player (finished items its carries want, parts that finish a wanted recipe)
  function itemScore(c, id) {
    const it = DT.ITEMS[id];
    if (!it) return 0;
    const p = c.player, g = c.game;
    if (id === 'ac_digiegg') return eggTarget(g, p) ? 9 : 3;
    let sc = it.component ? it.tier || 1 : 4 + (it.aiRank || 0) * 0.5;
    for (const h of g.aiHolders(p)) {
      const want = DT.ITEM_ROLE[h.kind] || [];
      if (want.includes(id)) sc += 6 - want.indexOf(id) * 0.2;
      if (it.component) sc += want.filter((w) => DT.ITEMS[w].from && DT.ITEMS[w].from.includes(id) && DT.ITEMS[w].from.some((x) => x !== id && p.items.includes(x))).length * 2;
    }
    return sc;
  }

  // the unit a Digi-Egg should go on: a form one copy short of digivolving (best value first)
  function eggTarget(g, p) {
    const max = g.R.maxStage != null ? g.R.maxStage : 2;
    const groups = {};
    for (const u of p.allUnits()) {
      if (u.stage >= max || !DT.LINE[u.line].shop) continue;
      (groups[`${u.line}|${u.stage}`] = groups[`${u.line}|${u.stage}`] || []).push(u);
    }
    let best = null, bestScore = -1;
    for (const list of Object.values(groups)) {
      const u = list[0];
      if (list.length !== g.copiesOf(u.line) - 1) continue;
      if ((g.pool[u.line] || 0) < Math.pow(g.copiesOf(u.line), u.stage)) continue;
      const sc = DT.LINE[u.line].tier * 10 + u.stage * 25;
      if (sc > bestScore) { bestScore = sc; best = u; }
    }
    return best;
  }

  const RELICS = () => Object.keys(DT.ITEMS).filter((id) => DT.ITEMS[id].relic);
  const RELIC_AI = { ac_r_treasure: 3, ac_r_forge: 3.5, ac_r_vault: 2, ac_r_charm: 1.5, ac_r_recruiter: 2.5, ac_r_grace: 2.5, ac_r_reaper: 3.5, ac_r_eggnest: 3, ac_r_card: 2, ac_r_key: 4, ac_r_discount: 0.5 };
  const card = (id) => {
    const it = DT.ITEMS[id] || { name: String(id), desc: '' };
    const img = DT.ItemArt && DT.ItemArt.img ? DT.ItemArt.img(id, 64, 'augart') : '';
    const recipe = it.from && !it.component ? ` From ${it.from.map((x) => DT.ITEMS[x].name).join(' + ')}.` : '';
    return { name: it.name, desc: (DT.itemText ? DT.itemText(id) : it.desc) + recipe, img };
  };

  const offerKinds = {
    ac_loot: {
      name: 'Creep Loot',
      sub: 'You beat the creeps. Pick one item for your item bench.',
      options: (c) => (c.player.mech.loot || []).slice(),
      choose(c, id) { c.give(id); c.player.mech.loot = null; },
      aiScore: (c, id) => itemScore(c, id),
      card,
    },
    ac_relic: {
      name: 'Relic',
      sub: 'Pick one relic. It replaces the relic you hold.',
      options: (c, exclude = []) => c.shuffle(RELICS().filter((id) => !c.player.globals.includes(id) && !exclude.includes(id))).slice(0, 3),
      choose(c, id) {
        const p = c.player;
        const rel = new Set(RELICS());
        p.globals = p.globals.filter((x) => !rel.has(x));
        p.globals.push(id);
        const it = DT.ITEMS[id];
        if (it && it.onGet) it.onGet(c);
        c.combine();
      },
      aiScore: (c, id) => (RELIC_AI[id] || 1) + (id === 'ac_r_vault' && c.player.gold >= 40 ? 2 : 0) + (id === 'ac_r_key' && c.stage >= 5 ? 1 : 0),
      card,
    },
  };

  // ---------- abilities ----------
  const abilities = {
    // summons N units of line.summon next to the caster (at most 4 alive per caster); their HP follows ability power
    summon: {
      base: 100,
      cast(c, u, tg, v) {
        const L = u.line;
        if (!L.summon || !DT.LINE[L.summon]) return;
        const n = (L.summonN || [1, 2, 2])[u.stage] || 1;
        const k = v / Math.max(1, DT.abilityValue(L, u.stage));
        let alive = c.units.filter((x) => x.alive && x.summoned && x.state.acOwner === u.id).length;
        for (let i = 0; i < n && alive < 4; i++) {
          const cell = c.freeCellNear(u, tg || u);
          if (!cell) break;
          const s = c.summon(u.team, L.summon, u.stage, cell.c, cell.r, { hpMul: k, adMul: k });
          if (!s) break;
          s.state.acOwner = u.id;
          alive++;
        }
      },
      text: (line, stage) => {
        const S = DT.LINE[line.summon];
        const n = (line.summonN || [1, 2, 2])[stage] || 1;
        return `Summons ${n} ${S ? S.forms[0] : 'helper'}${n > 1 ? 's' : ''} beside itself (up to 4 at once).`;
      },
    },
    // transforms for the rest of combat (once), every cast heals
    transform: {
      base: 150,
      cast(c, u, tg, v) {
        c.heal(u, v);
        if (u.state.acTf) return;
        u.state.acTf = true;
        const T = u.line.tf || {};
        u.ad += u.baseAd * (T.ad != null ? T.ad : 0.3);
        if (T.as) u.buffs.push({ stat: 'as', v: T.as, until: Infinity });
        if (T.armor) { u.armor += T.armor; u.mr += T.armor; }
        if (T.range) u.range += T.range;
        c.text(u, 'Transform');
        c.event({ type: 'shield', at: u.id });
      },
      text: (line, stage, v) => {
        const T = line.tf || {};
        const parts = [`+${Math.round((T.ad != null ? T.ad : 0.3) * 100)}% AD`];
        if (T.as) parts.push(`+${Math.round(T.as * 100)}% attack speed`);
        if (T.armor) parts.push(`+${T.armor} armor and MR`);
        if (T.range) parts.push(`+${T.range} range`);
        return `Heals ${v}. The first cast transforms it for the rest of combat: ${parts.join(', ')}.`;
      },
    },
    // hits every enemy
    storm: {
      base: 90,
      cast(c, u, tg, v) {
        for (const e of c.enemiesOf(u)) { c.event({ type: 'beam', from: u.id, to: e.id }); c.abilityHit(u, e, v); }
      },
      text: (line, stage, v) => `Lightning strikes every enemy for ${v} magic damage.`,
    },
  };

  // ---------- mechanic ----------
  const mechanic = {
    id: 'creeps',
    name: 'Creep Loot',
    desc: 'Creep rounds (1, 2, 3, then every fifth round from 10) drop items: win and pick 1 of 3 next round, lose and get one random item. Relics at 3-2, 5-2 and 7-2.',
    hooks: {
      onCountTraits: countRules,
      // Druid gate (Dota Auto Chess): a Druid digivolved with 2 copies fights at full strength only with Druid (2) for
      // Champions and Druid (4) for finals; otherwise at the strength of the form the trait allows.
      onCombatStart(c) {
        const SM = [1, 1.8, 3.24], AM = [1, 1.7, 3];
        for (const u of c.units) {
          if (u.summoned || u.stage < 1 || !u.traits.includes('DRUID')) continue;
          const eff = Math.min(u.stage, c.traitLevel(u.team, 'DRUID'));
          if (eff >= u.stage) continue;
          const k = SM[eff] / SM[u.stage];
          u.maxHp = Math.round(u.maxHp * k);
          u.ad *= k;
          u.baseAd *= k;
          u.ap *= AM[eff] / AM[u.stage];
          u.state.acDruidCap = eff;
        }
      },
      // Dota Auto Chess base income min(round, 5)
      onIncome(c, e) { if (!c.R.incomeByStage) e.base = Math.min(roundNo(c.game), 5); },
      // AI: use Digi-Eggs on a form one copy short of digivolving
      onAiTurn(c) {
        const p = c.player, g = c.game;
        for (let guard = 0; guard < 4; guard++) {
          const idx = p.items.indexOf('ac_digiegg');
          if (idx < 0) break;
          const u = eggTarget(g, p);
          if (!u) break;
          if (g.equipItem(p, idx, u.uid) !== 'ac_digiegg') break;
        }
      },
    },
    offerKinds,
  };

  DT.Sets.extend(SET, { traits, items, abilities, mechanic });
  // creep loot (pve.reward in autochess.js calls this)
  DT.AutoChess = { reward, rollLoot, lootOptions, countRules, eggTarget };
})(typeof window !== 'undefined' ? window : globalThis);
