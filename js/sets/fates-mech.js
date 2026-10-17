// Fates behavior: trait, item, artifact, augment and consumable hooks, the set's ability types, and the Partner
// mechanic (TFT Chosen). Contexts are documented in docs/sets/FORMAT.md. All randomness uses c.rng() / c.pick().
(function (G) {
  const DT = G.DT;
  const ID = 'fates';
  const def = () => DT.Sets.def(ID);
  const P = () => def().partner;

  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const real = (list) => list.filter((u) => !u.summoned);
  const mechOf = (c, team) => (c.side(team) && c.side(team).mech) || {};
  // remove up to amt of a shield later (timed shields; the engine's shields last the whole combat)
  const timedShield = (c, u, amt, dur) => { c.shield(u, amt); c.after(dur, () => { const take = Math.min(u.shield, amt); u.shield -= take; }); };
  // digivolve a combat unit one stage for this combat (Lunar, Lunar Crown)
  const moonGrow = (c, u) => {
    if (u.stage >= 2) return;
    u.stage += 1;
    u.maxHp = Math.round(u.maxHp * 1.5);
    u.ad *= 1.5;
    u.baseAd *= 1.5;
    c.text(u, 'Full Moon');
  };
  const byLowStage = (a, b) => a.stage - b.stage || b.items.length - a.items.length || a.id - b.id;

  // ---------- traits ----------
  function oceanRise(c) {
    const s = c.state;
    s.done = true;
    const foes = c.survivors(1 - c.team);
    if (!foes.length) return;
    // the enemy with the most enemies around it
    let best = foes[0], bn = -1;
    for (const e of foes) { const n = c.enemiesNear(c.team, e, 1).length; if (n > bn) { bn = n; best = e; } }
    let cell = c.freeCellNear(best, best);
    for (let k = 0; !cell && k < foes.length; k++) cell = c.freeCellNear(foes[k], foes[k]);
    for (let k = 0; !cell && k < c.mine.length; k++) if (c.mine[k].alive) cell = c.freeCellNear(c.mine[k], c.mine[k]);
    if (!cell) return;
    const boost = 1 + 0.08 * s.stars;
    const sh = c.summon(c.team, 'fat_shadow', c.v, cell.c, cell.r, { hpMul: c.val('hpMul') * boost });
    if (!sh) return;
    sh.ad *= boost;
    sh.baseAd *= boost;
    c.text(sh, 'Soulmon');
    c.event({ type: 'burst', c: sh.c, r: sh.r, radius: 1, team: c.team });
    const slam = [90, 180, 360][c.lv - 1] * boost;
    for (const e of c.enemiesNear(c.team, sh, 1)) { c.stun(e, 1.5); c.damage(sh, e, slam, 'magic'); }
  }

  function ascend(c, u) {
    const s = u.state.cel;
    s.used = true;
    s.until = c.t + s.dur;
    u.stun = 0;
    c.text(u, 'Ascend');
    c.event({ type: 'shield', at: u.id });
  }
  const ascended = (c, u) => u.state.cel && u.state.cel.until > c.t;

  function bancho(c, u) {
    const s = u.state.bancho;
    s.used = true;
    u.invulnUntil = c.t + 3;
    u.stun = Math.max(u.stun, 3);
    c.text(u, 'Push-ups');
    const back = () => { if (s.done) return; s.done = true; s.pumped = true; u.stun = 0; u.invulnUntil = Math.min(u.invulnUntil, c.t); c.text(u, 'Pumped Up'); };
    for (let k = 1; k <= 6; k++) {
      c.after(0.5 * k, () => {
        if (s.done) return;
        c.heal(u, u.maxHp * 0.08);
        c.buff(u, 'as', 0.08);
        if (u.hp >= u.maxHp - 1 || k === 6) back();
      }, u);
    }
  }

  const traits = {
    OCEAN: {
      hooks: {
        onCombatReady(c) {
          const s = c.state;
          s.total = real(c.mine).reduce((n, u) => n + u.maxHp, 0);
          s.stars = c.members.reduce((n, u) => n + u.stage, 0);
          s.done = false;
        },
        onTick: { order: 3, fn(c) {
          const s = c.state;
          if (s.done || !s.total) return;
          const hp = real(c.mine).reduce((n, u) => n + (u.alive ? Math.max(0, u.hp) : 0), 0);
          if (hp <= s.total * 0.5) oceanRise(c);
        } },
      },
    },
    CELES: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.cel = { dur: c.v, amp: c.val('amp'), n: 0, until: -1, used: false }; },
        onAttack(c, e) {
          const s = e.unit.state.cel;
          if (!s || s.used || !c.isMember(e.unit)) return;
          if (++s.n >= 6) ascend(c, e.unit);
        },
        onDamageTaken(c, e) {
          const u = e.unit, s = u.state.cel;
          if (s && !s.used && c.isMember(u) && u.hp < u.maxHp * 0.5) ascend(c, u);
        },
        onBeforeDamage(c, e) { if (ascended(c, e.unit) && c.isMember(e.unit)) e.mult *= 1 + e.unit.state.cel.amp; },
        onBeforeDamageTaken(c, e) { if (ascended(c, e.unit) && c.isMember(e.unit)) e.dr += e.unit.state.cel.amp; },
      },
    },
    NIGHT: { hooks: all((u, c) => { u.ap += c.v + (c.isMember(u) ? c.val('own') : 0); }) },
    FOREST: {
      hooks: {
        onCombatStart(c) { c.state.next = 2; c.state.k = 0; },
        onTick(c) {
          const s = c.state;
          if (s.k >= 5 || c.t < s.next) return;
          s.k++;
          s.next += 2;
          const g = c.val('grow');
          for (const u of c.members) if (u.alive) { u.armor += c.v; u.mr += c.v; u.ad += g; u.ap += g; }
        },
      },
    },
    PILGRIM: {
      hooks: {
        onAttack(c, e) { const u = e.unit; if (c.isMember(u) && u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + 10 * c.v); },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || e.isBurn || !(u.maxMana > 0)) return;
          u.mana = Math.min(u.maxMana, u.mana + Math.min(15, e.amount * 0.06) * c.v);
        },
      },
    },
    RONIN: {
      hooks: {
        onCombatReady(c) {
          for (const u of c.members) {
            if (c.adjacent(u).some((a) => a.team === u.team)) continue;
            u.shield += u.maxHp * c.v;
            if (c.lv >= 2) u.omnivamp += c.val('vamp');
            c.text(u, 'Lone Blade');
          }
        },
      },
    },
    TREASURE: {
      hooks: {
        ...each((u, c) => { u.dodge += c.v; }),
        // game hook: lucky tokens on PvP losses, paid out on the next PvP win
        onRoundEnd(c, e) {
          if (e.pve) return;
          const m = c.player.mech;
          if (!e.won) { m.luck = Math.min(8, (m.luck || 0) + 1); return; }
          const n = m.luck || 0;
          e.gold += 1 + n + (c.lv >= 2 ? 3 : 0);
          if (n >= 3) c.give(c.randomComponent());
          if (n >= 5) c.give(c.randomCompleted());
          m.luck = 0;
        },
      },
      ai: { bonus: (a) => (a.stage <= 4 ? 1 : 0) },
    },
    LUNAR: {
      hooks: {
        onCombatStart: { order: -5, fn(c) {
          const list = c.members.filter((u) => u.stage < 2).sort(byLowStage);
          for (const u of list.slice(0, c.v)) moonGrow(c, u);
        } },
      },
    },
    NINJA: {
      hooks: {
        onCombatStart(c) {
          if (c.n !== 1 && c.n !== 4) return;
          for (const u of c.members) { u.ad += u.baseAd * c.v; u.ap += Math.round(c.v * 100); }
        },
      },
    },
    SPIRIT: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || u.state.spirit) return;
          u.state.spirit = true;
          const as = (c.v * u.maxMana) / 100;
          for (const a of c.survivors(c.team)) c.buff(a, 'as', as);
          c.text(u, 'Spirit Rush');
        },
      },
    },
    DEVA: {
      hooks: {
        onCombatStart(c) {
          const mult = 1 + 0.1 * Math.min(5, mechOf(c, c.team).deva || 0);
          for (const u of c.members) { u.maxHp += Math.round(c.v * mult); u.ap += Math.round(c.val('ap') * mult); }
        },
        onRoundEnd(c, e) { if (!e.pve && e.won) c.player.mech.deva = Math.min(5, (c.player.mech.deva || 0) + 1); },
      },
    },
    SOVEREIGN: {
      hooks: {
        onCombatReady: { order: 2, fn(c) {
          for (const u of c.members) {
            if (u.summoned) continue;
            const foe = c.target(u, 'closest');
            u.state.guards = [];
            for (let k = 0; k < 2; k++) {
              const cell = c.freeCellNear(u, foe || u);
              if (!cell) break;
              const g = c.summon(c.team, 'fat_statue', u.stage, cell.c, cell.r, { noHooks: true });
              if (!g) break;
              g.maxHp = Math.round(u.maxHp * c.v);
              g.hp = g.maxHp;
              g.armor = u.armor;
              g.mr = u.mr;
              u.state.guards.push(g);
            }
          }
        } },
        onDeath(c, e) {
          const gs = e.unit.state.guards;
          if (!gs) return;
          e.unit.state.guards = null;
          for (const g of gs) if (g.alive) { g.invulnUntil = 0; c.damage(e.unit, g, 1e9, 'true'); }
        },
      },
    },
    BANCHO: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.bancho = { used: false }; },
        onDamageTaken(c, e) {
          const u = e.unit, s = u.state.bancho;
          if (s && !s.used && c.isMember(u) && u.hp < u.maxHp * 0.4) bancho(c, u);
        },
        onBeforeDamage(c, e) { const s = e.unit.state.bancho; if (s && s.pumped && c.isMember(e.unit)) e.mult *= 1.2; },
      },
    },
    FALLEN: {
      hooks: {
        onCombatStart(c) { const f = mechOf(c, c.team).fallen; for (const u of c.members) u.state.fallen = f || null; },
        onBeforeDamage(c, e) { if (e.unit.state.fallen === 'dark' && c.t < 10 && c.isMember(e.unit)) e.mult *= 1.4; },
        onDamageDealt(c, e) { if (e.unit.state.fallen === 'holy' && c.isMember(e.unit) && e.unit.alive) c.heal(e.unit, e.amount * 0.4, true); },
        // game hook: after 3 combats with the line, hand out both path items once
        onRoundEnd(c) {
          const m = c.player.mech;
          m.fallenFights = (m.fallenFights || 0) + 1;
          if (m.fallenFights >= 3 && !m.fallen && !m.fallenGiven && c.player.items.length <= c.R.itemCap - 2) {
            m.fallenGiven = true;
            c.give('fat_holy');
            c.give('fat_dark');
          }
        },
      },
    },
    FORGE: {
      hooks: {
        onRoundEnd(c) {
          const p = c.player, m = p.mech;
          const smith = p.boardUnits().filter((u) => u.line === 'fat_volcamon').reduce((s, u) => Math.max(s, u.stage), -1);
          if (smith < 0) return;
          m.forge = (m.forge || 0) + smith + 1;
          while (m.forge >= c.v && (m.forged || 0) < 4 && p.items.length < c.R.itemCap) {
            m.forge -= c.v;
            m.forged = (m.forged || 0) + 1;
            const have = new Set(p.items);
            const ids = def().artifacts.filter((a) => !have.has(a));
            c.give(c.pick(ids.length ? ids : def().artifacts));
          }
        },
      },
    },
    STYLE: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.style = { n: 0, s: 0 }; },
        onAttack(c, e) {
          const u = e.unit, st = u.state.style;
          if (!st || !c.isMember(u)) return;
          if (++st.n % 2 !== 0) return;
          c.shield(u, u.maxHp * c.v);
          if (e.target.alive) { c.event({ type: 'shot', from: u.id, to: e.target.id }); c.damage(u, e.target, e.dmg, 'phys', e.crit); }
          if (++st.s >= 2) { st.s = 0; u.mana = u.maxMana; c.text(u, 'Showtime'); }
        },
      },
    },
    ADEPT: { hooks: { onCombatStart(c) { for (const e of c.units) if (e.team !== c.team) c.buff(e, 'as', -0.5, c.v); } } },
    ASSASSIN: { hooks: each((u, c) => { u.crit += c.v; u.critDmg += c.val('critDmg'); u.abilCrit = true; }) },
    BRAWLER: { hooks: each((u, c) => { u.maxHp += c.v; }) },
    CHARMER: {
      hooks: {
        onDamageDealt(c, e) {
          const tg = e.target;
          if (e.kind !== 'magic' || e.isBurn || !tg.alive || !c.isMember(e.unit) || tg.state.charmUntil > c.t) return;
          tg.state.charmUntil = c.t + 8;
          c.buff(tg, 'ad', -tg.baseAd * c.v, 8);
        },
      },
    },
    DUELIST: { hooks: each((u, c) => { u.atkAs = c.v; u.atkAsMax = 8; }) },
    HUNTER: {
      hooks: {
        onCombatStart(c) { c.state.next = c.val('every'); },
        onTick(c) {
          const s = c.state;
          if (c.t < s.next) return;
          s.next += c.val('every');
          for (const h of c.members) {
            if (!h.alive) continue;
            const foes = c.enemiesOf(h);
            if (!foes.length) return;
            const tg = foes.reduce((b, x) => (x.hp / x.maxHp < b.hp / b.maxHp ? x : b));
            c.event({ type: 'shot', from: h.id, to: tg.id, big: true });
            c.damage(h, tg, h.ad * c.v, 'phys');
          }
        },
      },
    },
    KEEPER: {
      hooks: {
        onCombatStart: { order: 2, fn(c) {
          const dur = c.val('dur');
          for (const k of c.members) {
            timedShield(c, k, c.v * 1.5, dur);
            for (const a of c.adjacent(k)) if (a.team === k.team) timedShield(c, a, c.v, dur);
          }
        } },
      },
    },
    MAGE: {
      hooks: {
        onCombatReady: { order: 1, fn(c) { for (const u of c.members) u.ap *= c.v; } },
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          if (!u.state.recast) { u.state.recast = true; u.mana = u.maxMana; } else u.state.recast = false;
        },
      },
    },
    WARDER: { hooks: all((u, c) => { u.mr += c.v; }) },
    SHADE: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || u.nAtk % 3 !== 0 || !e.target.alive) return;
          c.damage(u, e.target, c.v, 'magic');
          u.invulnUntil = Math.max(u.invulnUntil, c.t + 0.5);
        },
      },
    },
    SHARP: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          const near = c.enemiesNear(u.team, e.target, 2).filter((x) => x !== e.target).sort((a, b) => c.dist(e.target, a) - c.dist(e.target, b) || a.id - b.id).slice(0, c.v);
          for (const x of near) { c.event({ type: 'shot', from: e.target.id, to: x.id }); c.damage(u, x, e.dmg * c.val('frac'), 'phys'); }
        },
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || !e.target || ['heal', 'shield', 'rally', 'grow', 'rewind', 'frenzy'].includes(u.line.type)) return;
          const near = c.enemiesNear(u.team, e.target, 2).filter((x) => x !== e.target).sort((a, b) => c.dist(e.target, a) - c.dist(e.target, b) || a.id - b.id).slice(0, c.v);
          for (const x of near) { c.event({ type: 'beam', from: e.target.id, to: x.id }); c.abilityHit(u, x, e.value * c.val('frac') * 0.7); }
        },
      },
    },
    VANGUARD: { hooks: each((u, c) => { u.armor += c.v; }) },
  };

  // ---------- ability types ----------
  const abilities = {
    grow: {
      base: 300,
      cast(c, u, tg, v) {
        const allies = c.units.filter((a) => a.alive && a.team === u.team && !a.summoned);
        if (!allies.length) return;
        const a = allies.reduce((b, x) => (x.hp / x.maxHp < b.hp / b.maxHp ? x : b));
        a.maxHp += v;
        c.heal(a, v);
        c.event({ type: 'shield', at: a.id });
        for (const e of c.enemiesNear(u.team, a, 1)) c.stun(e, 1);
      },
      text: (line, stage, v) => `Makes the lowest-health ally grow: +${v} max HP and a heal for as much; enemies next to it are stunned for 1s.`,
    },
    blossom: {
      base: 270,
      cast(c, u, tg, v) {
        const t = c.target(u, 'lowestHp') || tg;
        if (!t) return;
        c.event({ type: 'beam', from: u.id, to: t.id });
        c.abilityHit(u, t, v);
        if (!t.alive) { u.state.blossom = (u.state.blossom || 0) + 1; c.text(u, '+2 AP'); }
      },
      text: (line, stage, v) => `Blasts the lowest-health enemy for ${v} magic damage. Kills give 2 ability power for the rest of the game (up to 60).`,
    },
    burst: {
      base: 125,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
        for (const e of c.enemiesNear(u.team, tg, 1)) { c.stun(e, 1.5); c.abilityHit(u, e, v); }
      },
      text: (line, stage, v) => `Deals ${v} magic damage to the target and enemies next to it, stunning them for 1.5s.`,
    },
    frenzy: {
      base: 220,
      cast(c, u, tg, v) {
        if (!u.state.frenzy) {
          u.state.frenzy = true;
          c.buff(u, 'as', 0.6);
          u.omnivamp += 0.25;
          c.text(u, 'Frenzy');
        } else c.heal(u, v);
        c.event({ type: 'shield', at: u.id });
      },
      text: (line, stage, v) => `First cast: +60% attack speed and 25% omnivamp for the rest of combat. Later casts heal it for ${v}.`,
    },
    lullaby: {
      base: 160,
      cast(c, u, tg, v) {
        const foes = c.enemiesOf(u).sort((a, b) => b.hp - a.hp || a.id - b.id).slice(0, u.stage + 2);
        for (const e of foes) { c.event({ type: 'beam', from: u.id, to: e.id }); c.stun(e, [1.5, 2, 2.5][u.stage]); c.abilityHit(u, e, v); }
      },
      text: (line, stage, v) => `Puts the ${stage + 2} enemies with the most HP to sleep for ${[1.5, 2, 2.5][stage]}s and deals ${v} magic damage to each.`,
    },
    showstopper: {
      base: 130,
      cast(c, u, tg, v) {
        const big = v + tg.maxHp * [0.06, 0.1, 0.2][u.stage];
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
        const splash = c.enemiesNear(u.team, tg, 1).filter((e) => e !== tg);
        c.abilityHit(u, tg, big);
        for (const e of splash) c.abilityHit(u, e, big * 0.5);
      },
      text: (line, stage, v) => `Slams the target for ${v} magic damage plus ${[6, 10, 20][stage]}% of its max HP, and half of that to enemies next to it.`,
    },
    rewind: {
      base: 110,
      cast(c, u, tg, v) {
        const allies = c.units.filter((a) => a.alive && a.team === u.team && a !== u && !a.summoned && !a.state.rune)
          .sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id).slice(0, u.stage + 1);
        for (const a of allies) { a.state.rune = v; c.event({ type: 'shield', at: a.id }); }
      },
      text: (line, stage, v) => `Places a time rune on the ${stage + 1} lowest-health allies. When one would be knocked out, it comes back after 2s with ${v} HP and 30% attack speed.`,
    },
  };

  // ---------- items ----------
  const sameRowBeside = (c, u) => c.units.filter((a) => a.alive && a.team === u.team && a.r === u.r && c.dist(a, u) <= 1);
  const items = {
    fat_deathblade: { hooks: { onKill(c, e) { if (e.unit.alive) c.buff(e.unit, 'ad', e.unit.baseAd * 0.15); } } },
    fat_angel: {
      hooks: {
        onLethal(c, e) {
          const u = e.unit;
          if (u.state.angel || u.hp > 0) return;
          u.state.angel = true;
          u.hp = 1;
          u.invulnUntil = c.t + 2;
          c.text(u, 'Guardian Wing');
          c.after(2, () => c.heal(u, 400), u);
        },
      },
    },
    fat_zeke: { hooks: { onCombatStart(c) { for (const a of sameRowBeside(c, c.unit)) c.buff(a, 'as', 0.3); } } },
    fat_rfc: { hooks: { onCombatStart(c) { c.unit.range += 1; } } },
    fat_zzrot: {
      hooks: {
        onCombatStart(c) { for (const e of c.enemiesNear(c.unit.team, c.unit, 2)) e.target = c.unit; },
        onDeath(c, e) {
          const u = e.unit;
          const g = c.summon(u.team, 'fat_construct', u.stage, u.c, u.r, { noHooks: true });
          if (g) { c.text(g, 'Tankmon'); for (const x of c.enemiesNear(u.team, g, 2)) x.target = g; }
        },
      },
    },
    fat_luden: {
      hooks: {
        onCast(c, e) {
          const u = e.unit, tg = e.target;
          if (!tg) return;
          const hits = [tg].concat(c.enemiesNear(u.team, tg, 2).filter((x) => x !== tg).sort((a, b) => c.dist(tg, a) - c.dist(tg, b) || a.id - b.id).slice(0, 3));
          for (const x of hits) if (x.alive) { c.event({ type: 'beam', from: u.id, to: x.id }); c.damage(u, x, 150, 'magic'); }
        },
      },
    },
    fat_locket: {
      hooks: {
        onCombatStart(c) {
          const u = c.unit, amt = [250, 325, 450][u.stage];
          for (const a of c.units) if (a.alive && a.team === u.team && a.r === u.r && c.dist(a, u) <= 2) timedShield(c, a, amt, 8);
        },
      },
    },
    fat_frozenheart: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (!c.every(u, 'fheart', 1)) return;
          for (const x of c.enemiesNear(u.team, u, 2)) c.buff(x, 'as', -0.3, 1.05);
        },
      },
    },
    fat_chalice: { hooks: { onCombatStart(c) { for (const a of sameRowBeside(c, c.unit)) a.ap += 30; } } },
    fat_shroud: { hooks: { onCombatStart(c) { for (const e of c.units) if (e.team !== c.unit.team && e.c === c.unit.c && e.maxMana > 0) e.mana -= e.maxMana / 3; } } },
    fat_dclaw: { hooks: { onBeforeDamageTaken(c, e) { if (e.kind === 'magic') e.mult *= 0.6; } } },
    fat_zephyr: {
      hooks: {
        onCombatStart(c) {
          const u = c.unit, spot = { c: c.cols - 1 - u.c, r: c.rows - 1 - u.r };
          const foes = c.units.filter((e) => e.alive && e.team !== u.team);
          if (!foes.length) return;
          const tg = foes.reduce((b, x) => (c.dist(spot, x) < c.dist(spot, b) ? x : b));
          c.stun(tg, 3);
          c.text(tg, 'Zephyr');
        },
      },
    },
    fat_trapclaw: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          const u = e.unit;
          if (u.state.trap || e.kind !== 'magic' || e.isBurn || !e.source || e.source.team === u.team) return;
          u.state.trap = true;
          e.mult = 0;
          c.stun(e.source, 3);
          c.text(u, 'Trap Claw');
        },
      },
    },
    // artifacts
    fat_art_defiance: {
      hooks: {
        onBeforeDamageTaken(c, e) { const s = e.unit.state; s.defer = (s.defer || 0) + e.amount * e.mult * 0.6; e.mult *= 0.4; },
        onUnitTick(c, e) {
          const u = e.unit, s = u.state;
          if (!(s.defer > 0)) return;
          const take = Math.min(s.defer, s.defer * e.dt / 2.5 + 10 * e.dt);
          s.defer -= take;
          u.hp = Math.max(1, u.hp - take * (1 - Math.min(0.6, u.dr)));
        },
      },
    },
    fat_art_winter: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit, x = e.source;
          if (!x || !x.alive || x.team === u.team || x.state.winterUntil > c.t) return;
          x.state.winterUntil = c.t + 2;
          c.buff(x, 'as', -0.35, 2);
          x.state.winterN = (x.state.winterN || 0) + 1;
          if (x.state.winterN >= 7 && !(x.state.frozeAt > c.t - 10)) { x.state.winterN = 0; x.state.frozeAt = c.t; c.freeze(x, 2); }
        },
      },
    },
    fat_art_collector: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit, tg = e.target;
          if (!tg.alive || tg.hp >= tg.maxHp * 0.1) return;
          tg.invulnUntil = 0;
          c.damage(u, tg, 1e7, 'true');
          if (!tg.alive) { u.state.gold = (u.state.gold || 0) + 1; c.text(u, '+1 gold'); }
        },
      },
    },
    fat_art_manazane: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (u.state.mz) return;
          u.state.mz = true;
          for (let k = 1; k <= 4; k++) c.after(k, () => { u.mana = Math.min(u.maxMana, u.mana + 25); }, u);
        },
      },
    },
    fat_art_cleaver: {
      hooks: {
        onDamageDealt(c, e) {
          const tg = e.target;
          if (e.kind !== 'magic' || !tg.alive || tg.state.cleaveUntil > c.t) return;
          tg.state.cleaveUntil = c.t + 3;
          c.buff(tg, 'mr', -20, 3);
        },
      },
    },
    fat_art_randuin: { hooks: { onCombatStart(c) { for (const a of c.adjacent(c.unit)) if (a.team === c.unit.team) { a.armor += 40; a.mr += 40; } } } },
    fat_art_fist: {
      hooks: {
        onCombatStart(c) {
          const tg = c.target(c.unit, 'farthest');
          if (!tg) return;
          c.event({ type: 'beam', from: c.unit.id, to: tg.id });
          c.stun(tg, 2);
          c.text(tg, 'Rocket Fist');
        },
      },
    },
    fat_art_zhonya: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          const u = e.unit;
          if (u.state.zhonya) return;
          if (u.hp + u.shield - e.amount * e.mult >= u.maxHp * 0.4) return;
          u.state.zhonya = true;
          e.mult = 0;
          u.invulnUntil = c.t + 2.5;
          u.stun = Math.max(u.stun, 2.5);
          c.text(u, 'Stasis');
        },
      },
    },
  };

  // ---------- consumables ----------
  function giveBack(c, u, idx, reforge) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    let back = u.items;
    if (reforge) {
      const arts = def().artifacts;
      back = back.map((x) => {
        const it = DT.ITEMS[x];
        if (DT.isComponent(x)) return x === 'fat_egg' ? x : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (it.emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (it.cat === 'artifact') return c.pick(arts.filter((k) => k !== x));
        if (x === 'fat_golden_egg') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  // Loaded Dice: a free shop of Digimon that share a trait with the unit (no Partner offers)
  function loadedDice(c, u, idx) {
    const p = c.player, g = c.game;
    const tr = DT.LINE[u.line].traits;
    const share = (l) => g.shopEligible(p, l.id) && l.traits.some((t) => tr.includes(t));
    if (!DT.SHOP_LINES.some((l) => share(l) && g.pool[l.id] > 0)) return 'none';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    for (const x of p.shop) if (x) g.pool[x] += 1;
    p.shopMeta = null;
    const odds = DT.SHOP_ODDS[Math.min(DT.SHOP_ODDS.length - 1, p.level)];
    for (let i = 0; i < p.shop.length; i++) {
      let roll = c.rng() * 100, tier = 1;
      for (let k = 0; k < odds.length; k++) { if (roll < odds[k]) { tier = k + 1; break; } roll -= odds[k]; }
      let lines = DT.SHOP_LINES.filter((l) => l.tier === tier && share(l) && g.pool[l.id] > 0);
      if (!lines.length) lines = DT.SHOP_LINES.filter((l) => l.tier <= tier && share(l) && g.pool[l.id] > 0);
      if (!lines.length) lines = DT.SHOP_LINES.filter((l) => share(l) && g.pool[l.id] > 0);
      if (!lines.length) { p.shop[i] = null; continue; }
      const pick = c.pick(lines);
      g.takeFromPool(pick.id);
      p.shop[i] = pick.id;
    }
    return id;
  }
  function copyData(c, u, idx) {
    const p = c.player, g = c.game;
    if (!DT.LINE[u.line].shop || g.pool[u.line] <= 0) return 'none';
    const free = p.freeBenchSlot();
    if (free < 0) return 'none';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    g.takeFromPool(u.line);
    p.bench[free] = g.newUnit(u.line, 0);
    g.combine(p);
    return id;
  }
  function choosePath(c, u, idx, path) {
    const p = c.player;
    if (u.line !== 'fat_lopmon' || p.mech.fallen) return 'none';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    const other = path === 'holy' ? 'fat_dark' : 'fat_holy';
    const j = p.items.indexOf(other);
    if (j >= 0) p.items.splice(j, 1);
    p.mech.fallen = path;
    return id;
  }
  Object.assign(items, {
    fat_remover: { use: (c, u, idx) => giveBack(c, u, idx, false) },
    fat_reforger: { use: (c, u, idx) => giveBack(c, u, idx, true) },
    fat_dice: { use: loadedDice },
    fat_copy: { use: copyData },
    fat_holy: { use: (c, u, idx) => choosePath(c, u, idx, 'holy') },
    fat_dark: { use: (c, u, idx) => choosePath(c, u, idx, 'dark') },
  });

  // ---------- Partner (TFT Chosen) ----------
  const partnerOf = (p) => p.allUnits().find((u) => u.mods && u.mods.partner) || null;
  const BONUSES = ['hp', 'ap', 'ad', 'mana'];
  // a Partner unit spec for a line: Champion form, one marked trait (counts twice), one bonus
  function partnerSpec(c, lineId) {
    const L = DT.LINE[lineId];
    const marks = L.traits.filter((t) => DT.TRAITS[t] && DT.TRAITS[t].kind !== 'unique');
    const trait = c.pick(marks.length ? marks : L.traits);
    const forced = c.game.mech.forcePartnerBonus; // test hook (tools only); never set in play
    const bonus = forced && BONUSES.includes(forced) ? forced : c.pick(BONUSES);
    return { stage: 1, bonusTraits: { [trait]: 1 }, mods: { partner: true, trait, bonus } };
  }
  // try to turn one shop slot into a Partner offer
  function offerPartner(c, force) {
    const p = c.player, g = c.game, cfg = P();
    if (p.level < cfg.fromLevel || partnerOf(p) || !p.shop.length) return false;
    const chance = p.augs.includes('fat_seeker') ? cfg.seekerChance : cfg.chance;
    if (!force && c.rng() >= chance) return false;
    const odds = cfg.odds[Math.min(cfg.odds.length - 1, p.level)];
    let roll = c.rng() * 100, tier = 1;
    for (let k = 0; k < odds.length; k++) { if (roll < odds[k]) { tier = k + 1; break; } roll -= odds[k]; }
    let lines = [];
    for (let t = tier; t >= 1 && !lines.length; t--) lines = DT.SHOP_LINES.filter((l) => l.tier === t && g.pool[l.id] >= 3 && g.shopEligible(p, l.id));
    if (!lines.length) return false;
    const L = c.pick(lines);
    const slot = Math.floor(c.rng() * p.shop.length);
    if (p.shop[slot]) g.pool[p.shop[slot]] += 1;
    g.takeFromPool(L.id);
    p.shop[slot] = L.id;
    p.shopMeta = p.shop.map(() => null);
    p.shopMeta[slot] = { cost: L.tier * 3, unit: partnerSpec(c, L.id), tag: 'Partner' };
    p.mech.partnerSeen = true;
    return true;
  }

  // apply Partner stats in combat (both teams)
  function partnerCombat(c) {
    const cfg = P();
    for (const u of c.units) {
      const m = u.mods;
      if (!m || !m.partner || u.summoned) continue;
      u.maxHp += cfg.hp;
      if (m.bonus === 'hp') u.maxHp += cfg.bonusHp;
      else if (m.bonus === 'ap') u.ap += cfg.bonusAp;
      else if (m.bonus === 'ad') { u.ad += u.baseAd * cfg.bonusAd; }
      else if (m.bonus === 'mana') u.maxMana = Math.max(10, Math.round(u.maxMana * (1 - cfg.bonusMana)));
    }
  }

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of real(c.mine)) fn(u, c); } });
  const randomArtifact = (c) => c.pick(def().artifacts);
  const augments = {
    fat_envelope: { hooks: { onPick(c) { c.player.gold += 8; } } },
    fat_stall: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    fat_dice_aug: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    fat_charm: { hooks: { onPick(c) { c.player.hp += 5; } } },
    fat_dojo: { hooks: team((u) => { u.ad += u.baseAd * 0.12; u.ap += 12; }) },
    fat_lanterns: { hooks: team((u) => { u.mana += 20; }) },
    fat_wall: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 30; u.mr += 30; u.maxHp += 200; } }) },
    fat_nest: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.25; }) },
    fat_bond: { hooks: team((u) => { if (u.mods && u.mods.partner) { u.maxHp += 300; u.ap += 25; } }),
      ai: { bonus: (c) => (partnerOf(c.player) ? 3 : -1) } },
    fat_seeker: { hooks: { onPick(c) { c.player.gold += 3; } } },
    fat_pouch: { hooks: { onPick(c) { c.player.gold += 18; } } },
    fat_chest: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomComponent()); } } },
    fat_egghunt: { hooks: { onPick(c) { c.give('fat_egg'); } } },
    fat_offerings: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    fat_shrine: { hooks: team((u) => { u.omnivamp += 0.12; }) },
    fat_united: { hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of real(c.mine)) { u.ad += u.baseAd * 0.03 * n; u.ap += 3 * n; } } } },
    fat_tome: { hooks: { onPick(c) { c.give(c.randomEmblem(true)); } } },
    fat_apprentice: { hooks: { onPick(c) { c.give(randomArtifact(c)); } } },
    fat_digivolve: { hooks: { onPick(c) { c.giveUnit([2, 3], 1); } } },
    fat_grace: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit;
          if (u.team !== c.team || u.state.grace || u.hp >= u.maxHp * 0.5) return;
          u.state.grace = true;
          c.buff(u, 'dr', 0.3, 4);
        },
      },
    },
    fat_twin: {
      hooks: { onCountTraits(counts, units) { const u = units.find((x) => x.mods && x.mods.partner); if (u && counts[u.mods.trait]) counts[u.mods.trait] += 1; } },
      ai: { bonus: (c) => (partnerOf(c.player) ? 3 : -1) },
    },
    fat_fortune: { hooks: { onPick(c) { c.player.gold += 35; } } },
    fat_grandarmory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    fat_golden: { hooks: { onPick(c) { c.give('fat_golden_egg'); } } },
    fat_master: { hooks: { onPick(c) { c.give(randomArtifact(c)); c.give(randomArtifact(c)); } } },
    fat_giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    fat_soulmates: {
      hooks: {
        onCountTraits(counts, units) { const u = units.find((x) => x.mods && x.mods.partner); if (u && counts[u.mods.trait]) counts[u.mods.trait] += 1; },
        onCombatStart(c) { for (const u of real(c.mine)) if (u.mods && u.mods.partner) { u.maxHp = Math.round(u.maxHp * 1.5); u.ap += 50; } },
      },
      ai: { bonus: (c) => (partnerOf(c.player) ? 3 : -1) },
    },
    fat_mega: { hooks: { onPick(c) { c.giveUnit([4], 1); } } },
    fat_festival: {
      hooks: {
        onPick(c) {
          const p = c.player, g = c.game;
          p.gold += 4;
          if (partnerOf(p)) { p.gold += 4; return; }
          const lines = DT.SHOP_LINES.filter((l) => l.tier === 4 && g.pool[l.id] >= 3);
          const free = p.freeBenchSlot();
          if (!lines.length || free < 0) { p.gold += 4; return; }
          const L = c.pick(lines);
          g.takeFromPool(L.id, 3);
          const u = g.newUnit(L.id, 0);
          Object.assign(u, partnerSpec(c, L.id));
          p.bench[free] = u;
          g.combine(p);
        },
      },
    },
    fat_crowns: {
      hooks: {
        onCombatStart: { order: -4, fn(c) {
          const list = real(c.mine).filter((u) => u.stage < 2).sort(byLowStage);
          if (list[0]) moonGrow(c, list[0]);
        } },
      },
    },
  };

  // ---------- the set mechanic: Partners, plus set-wide bookkeeping ----------
  const mechanic = {
    id: 'partners',
    name: 'Partners',
    desc: 'From level 4 a shop slot can offer a Partner: a Champion-form Digimon (like 3 copies) for 3 times its cost. Its marked trait counts twice, and it gains 200 HP plus one bonus (300 HP, 40 ability power, 20% attack damage or 20% lower max mana). You can own one Partner at a time; sell it to see more. You see at least one by round 2-2.',
    hooks: {
      onShopRolled(c) {
        const force = !c.player.mech.partnerSeen && (c.stage > 2 || (c.stage === 2 && c.round >= 2));
        offerPartner(c, force);
      },
      // a Partner is 3 copies: the shop slot held one, take the other two (or sell it as a Rookie-form Partner if they are gone)
      onBuy(c, e) {
        const u = e.unit;
        if (!u.mods || !u.mods.partner) return;
        const g = c.game;
        // with a full bench the engine only lets a buy through when it digivolves Rookie-form copies, so the Partner
        // joins them as a Rookie-form copy (refunding the difference); same when the other two copies are gone
        if (g.pool[u.line] >= 2 && c.player.freeBenchSlot() >= 0) g.takeFromPool(u.line, 2);
        else { u.stage = 0; c.player.gold += 2 * DT.LINE[u.line].tier; }
      },
      // keep the Partner mark when it digivolves with plain copies
      onCombine(c, e) {
        const src = e.from.find((x) => x.mods && x.mods.partner);
        if (src && !(e.unit.mods && e.unit.mods.partner)) { e.unit.mods = src.mods; e.unit.bonusTraits = src.bonusTraits; }
      },
      // Shinobi only works with exactly 1 or 4
      onCountTraits(counts) { const n = counts.NINJA; if (n && n !== 1 && n !== 4) counts.NINJA = 0; },
      onCombatStart: { order: 5, fn(c) {
        partnerCombat(c);
        // Veigar (Aruraumon) keeps the ability power from its kills
        for (const u of c.units) if (u.line.id === 'fat_aruraumon' && !u.summoned) u.ap += (mechOf(c, u.team).blossom || 0);
      } },
      // time runes from Millenniummon's ability
      onLethal(c, e) {
        const u = e.unit;
        if (!u.state.rune || u.hp > 0) return;
        const v = u.state.rune;
        u.state.rune = 0;
        u.hp = 1;
        u.invulnUntil = c.t + 2;
        u.stun = Math.max(u.stun, 2);
        c.text(u, 'Rewind');
        c.after(2, () => { c.heal(u, v); c.buff(u, 'as', 0.3); }, u);
      },
      // after combat: Veigar kills and Golden Fang knockouts are paid out to the player
      onRoundEnd(c, e) {
        const cb = e.match && e.match.combat;
        if (!cb) return;
        const m = c.player.mech;
        for (const u of cb.units) {
          if (u.team !== e.team || u.summoned) continue;
          if (u.state.blossom) m.blossom = Math.min(60, (m.blossom || 0) + 2 * u.state.blossom);
          if (u.state.gold) e.gold += u.state.gold;
        }
      },
      onAiTurn(c) {
        const p = c.player;
        // pick a Fallen path at once
        const path = p.items.indexOf('fat_holy') >= 0 ? (c.rng() < 0.5 ? 'fat_holy' : 'fat_dark') : p.items.indexOf('fat_dark') >= 0 ? 'fat_dark' : null;
        if (path && !p.mech.fallen) {
          const u = p.allUnits().find((x) => x.line === 'fat_lopmon');
          if (u) c.game.useConsumable(p, p.items.indexOf(path), u);
        }
        // Copy Data on the unit it owns the most copies of; Loaded Dice on the main carry
        const counts = {};
        for (const u of p.allUnits()) if (u.stage < 2) counts[u.line] = (counts[u.line] || 0) + 1;
        let i = p.items.indexOf('fat_copy');
        if (i >= 0) {
          const u = p.allUnits().filter((x) => x.stage < 2).sort((a, b) => (counts[b.line] || 0) - (counts[a.line] || 0) || DT.LINE[b.line].tier - DT.LINE[a.line].tier)[0];
          if (u) c.game.useConsumable(p, i, u);
        }
        i = p.items.indexOf('fat_dice');
        if (i >= 0) {
          const u = p.boardUnits().sort((a, b) => DT.unitValue(b) - DT.unitValue(a))[0];
          if (u) c.game.useConsumable(p, i, u);
        }
      },
    },
  };

  // tooltip and shop card line for a Partner
  function unitNote(u) {
    const m = u.mods;
    if (!m || !m.partner) return null;
    const cfg = P();
    const T = DT.TRAITS[m.trait];
    const bonus = { hp: `${cfg.bonusHp} more HP`, ap: `${cfg.bonusAp} ability power`, ad: `${Math.round(cfg.bonusAd * 100)}% attack damage`, mana: `${Math.round(cfg.bonusMana * 100)}% lower max mana` }[m.bonus];
    return `Partner: ${T ? T.name : m.trait} counts twice. +${cfg.hp} HP${bonus ? ` and ${bonus}` : ''}.`;
  }

  DT.Sets.extend(ID, { traits, items, augments, mechanic, abilities, unitNote });
})(typeof window !== 'undefined' ? window : globalThis);
