// Dragonlands behavior: trait, item, augment and consumable hooks, the Copycat ability, the Dragon / Treasure Dragon /
// Golden Armor mechanic and the wild-round rewards. Hook contexts are documented in docs/sets/FORMAT.md.
// Everything is deterministic: randomness only from c.rng() / c.pick().
(function (G) {
  const DT = G.DT;
  const SET = 'dragonlands';

  // ---------- helpers ----------
  const team = (c) => c.mine.filter((u) => !u.summoned); // real units (not statues or larvae)
  const isDragonLine = (L) => !!(L && L.slots === 2);
  const isDragon = (u) => isDragonLine(u.line);
  const sideMech = (c, t) => ((c.side(t == null ? c.team : t) || {}).mech) || {};
  const bankOf = (c) => Math.max(0, Math.min(15, sideMech(c).bank || 0));
  const fxAdd = (c, key, n = 1) => { const fx = c.teamState(c.team); fx[key] = (fx[key] || 0) + n; };
  // the per-combat team scratch object of the combat a player just finished (game hook onRoundEnd)
  const fxOf = (e) => (e && e.match && e.match.combat && e.match.combat.teamFx && e.match.combat.teamFx[e.team]) || {};
  const sameRow = (a, b, w) => a.r === b.r && Math.abs(a.c - b.c) <= w;
  const execute = (c, src, tg) => { if (tg.alive) { c.text(tg, 'Finished'); c.damage(src, tg, tg.hp + tg.shield + 1, 'true'); } };
  const COLS = DT.COLS, ROWS = DT.ROWS;

  // puts a Rookie-form unit of a line on the bench (gold for its cost when the pool or bench is out)
  function giveLine(c, lineId) {
    const g = c.game, p = c.player, L = DT.LINE[lineId];
    if (!L) return null;
    const free = p.freeBenchSlot();
    if (g.pool[lineId] > 0 && free >= 0) {
      g.takeFromPool(lineId);
      p.bench[free] = g.newUnit(lineId, 0);
      g.combine(p);
      return lineId;
    }
    p.gold += L.price || L.tier;
    return null;
  }
  const shopLines = (f) => DT.SHOP_LINES.filter(f);
  const goldItemIds = () => Object.keys(DT.ITEMS).filter((id) => DT.ITEMS[id].cat === 'gold');
  const radiantIds = () => Object.keys(DT.ITEMS).filter((id) => DT.ITEMS[id].cat === 'radiant');
  const isGoldItem = (id) => !!(DT.ITEMS[id] && DT.ITEMS[id].cat === 'gold');

  // ---------- traits ----------
  const traits = {
    STAR: {
      hooks: {
        onCombatStart(c) { for (const u of team(c)) u.ap += c.v; },
        // every third combat with Stargazers active: a Star Orb of gold and a Stargazer in the next shop
        onRoundEnd(c, e) {
          const m = c.player.mech;
          m.starN = (m.starN || 0) + 1;
          if (m.starN % 3 === 0) { e.gold += c.val('gold'); m.starShop = 1; }
        },
      },
    },
    XAB: {
      hooks: {
        onCombatStart(c) {
          const lines = new Set(c.members.map((u) => u.line.id));
          const m = c.v;
          const b = { hp: 0, as: 0, arm: 0, ad: 0, mana: 0, vamp: 0 };
          if (lines.has('drg_elecmon')) b.hp += 100;
          if (lines.has('drg_mushroomon')) b.as += 0.08;
          if (lines.has('drg_dorumon')) b.arm += 8;
          if (lines.has('drg_hackmon')) b.ad += 0.08;
          if (lines.has('drg_baohuckmon')) b.mana += 2;
          for (const u of c.members) if (!u.line.traits.includes('XAB')) b.vamp += 0.03; // Crest holders
          for (const u of team(c)) {
            const k = m * (c.isMember(u) ? 2 : 1);
            u.maxHp += Math.round(b.hp * k);
            u.as *= 1 + b.as * k;
            u.armor += b.arm * k; u.mr += b.arm * k;
            u.ad += u.baseAd * b.ad * k;
            u.omnivamp += b.vamp * k;
            if (b.mana) u.state.xabMana = b.mana * k;
          }
        },
        onAttack(c, e) { const u = e.unit; if (u.state.xabMana) u.mana = Math.min(u.maxMana, u.mana + u.state.xabMana); },
      },
    },
    STONE: {
      hooks: {
        onCombatStart(c) {
          const allies = team(c);
          if (!allies.length) return;
          const stage = (c.info && c.info.stage) || 3;
          const hp = 150 + 120 * stage;
          const mine = (r) => (c.team === 0 ? r >= 4 : r <= 3);
          const statues = [];
          for (let k = 0; k < c.v; k++) {
            let best = null, bestScore = 0;
            for (const a of allies) {
              for (const n of c.neighbors(a.c, a.r)) {
                if (!mine(n.r) || c.unitAt(n.c, n.r)) continue;
                let sc = 0;
                for (const m of c.neighbors(n.c, n.r)) { const x = c.unitAt(m.c, m.r); if (x && x.team === c.team && !x.summoned) sc++; }
                if (sc > bestScore) { bestScore = sc; best = n; }
              }
            }
            if (!best) break;
            const st = c.summon(c.team, 'drg_stone', 0, best.c, best.r, { noHooks: true });
            if (!st) break;
            st.maxHp = hp; st.hp = hp; st.ad = 0; st.baseAd = 0; st.armor = 30; st.mr = 30;
            st.moveCd = Infinity; st.atkCd = Infinity; st.maxMana = 0; st.mana = 0;
            st.state.statue = true;
            statues.push(st);
          }
          const as = c.val('as');
          for (const a of allies) {
            if (!statues.some((s) => c.dist(s, a) <= 1)) continue;
            a.state.jade = c.val('heal');
            a.as *= 1 + as;
          }
        },
        onUnitTick(c, e) { const u = e.unit; if (u.state.jade && c.every(u, 'jade', 2)) c.heal(u, u.maxHp * u.state.jade); },
        onDeath(c, e) {
          const s = e.unit;
          if (!s.state.statue) return;
          c.event({ type: 'burst', c: s.c, r: s.r, radius: 1, team: s.team });
          for (const en of c.enemiesNear(s.team, s, 1)) c.damage(s, en, s.maxHp * 0.33, 'magic');
        },
        // statues alone never hold a fight: they crumble when the last real ally falls
        onTick(c) {
          if (c.state.cleared) return;
          const alive = c.survivors(c.team);
          if (!alive.length || !alive.every((u) => u.state.statue)) return;
          c.state.cleared = true;
          for (const s of alive) c.damage(s, s, s.hp + s.shield + 1, 'true');
        },
      },
    },
    MIRAGE: {
      hooks: {
        onCombatStart(c) {
          const mech = c.info && c.info.mech;
          const v = (mech && mech.mirage) || ['dancer', 'warlord', 'overload', 'executioner'][Math.floor(c.rng() * 4)];
          c.state.v = v;
          for (const u of c.members) {
            if (v === 'dancer') { u.dodge += c.val('dodge'); u.as *= 1 + c.val('das'); }
            else if (v === 'warlord') { const w = c.val('war'); u.ad += u.baseAd * w; u.maxHp = Math.round(u.maxHp * (1 + w)); }
            else if (v === 'overload') u.state.mirZap = c.val('zap');
            else { u.crit += c.val('crit'); u.abilCrit = true; u.state.mirExec = true; }
          }
        },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!u.state.mirZap || !c.every(u, 'mirZap', 2)) return;
          const near = c.enemiesNear(u.team, u, 1);
          if (near.length) c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
          for (const en of near) c.damage(u, en, u.state.mirZap, 'magic');
        },
        onDamageDealt(c, e) {
          const u = e.unit, tg = e.target;
          if (u.state.mirExec && tg.alive && !e.isBurn && tg.hp < tg.maxHp * 0.1) execute(c, u, tg);
        },
      },
    },
    RAGE: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.rage = true; },
        onAttack(c, e) { const u = e.unit; if (u.state.rage && !(u.state.ragingUntil > c.t)) u.mana = Math.min(u.maxMana, u.mana + 3); },
        onCast(c, e) {
          const u = e.unit;
          if (!u.state.rage) return;
          u.state.ragingUntil = c.t + 4;
          c.buff(u, 'as', 0.25 + c.v, 4);
          c.buff(u, 'omnivamp', c.val('vamp'), 4);
          c.text(u, 'Rage');
        },
      },
    },
    SLAYER: {
      hooks: {
        onCombatStart(c) { c.state.noDragon = !team(c).some(isDragon); for (const u of c.members) u.state.slayer = true; },
        onBeforeDamageTaken(c, e) { if (e.unit.state.slayer && e.source && e.source.maxHp >= 1800) e.dr += 0.2; },
        onBeforeDamage(c, e) { if (c.state.noDragon && e.unit.state.slayer && !e.isBurn) e.mult *= 1 + c.v; },
      },
    },
    GOLD: { hooks: { onCombatStart(c) { const a = c.val('armor'); for (const u of c.members) { u.armor += a; u.mr += a; } } } }, // golden items: the mechanic, at round start
    THUNDER: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 8) return;
          c.state.done = true;
          const src = c.survivors(c.team).find((u) => !u.summoned);
          if (!src) return;
          for (const en of c.enemiesOf(src)) {
            c.event({ type: 'beam', from: src.id, to: en.id });
            c.stun(en, 1);
            c.damage(src, en, en.maxHp * c.v, 'true');
          }
          for (const u of c.members) if (u.alive) c.buff(u, 'as', c.val('as'));
        },
      },
    },
    WHISPER: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.whisper = c.v; },
        onDamageDealt(c, e) {
          const u = e.unit, tg = e.target;
          if (!u.state.whisper || e.isBurn || !tg.alive) return;
          if (!(tg.state.shrunkUntil > c.t)) {
            tg.state.shrunkUntil = c.t + 6;
            if (tg.armor > 0) c.buff(tg, 'armor', -tg.armor * 0.4, 6);
            if (tg.mr > 0) c.buff(tg, 'mr', -tg.mr * 0.4, 6);
            c.text(tg, 'Shrunk');
          } else if ((u.state.whN || 0) < 25) {
            u.state.whN = (u.state.whN || 0) + 1;
            u.ad += u.baseAd * u.state.whisper;
            u.ap += Math.round(u.state.whisper * 100);
          }
        },
      },
    },
    LAGOON: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.ap += c.v; u.as *= 1 + c.val('as'); u.state.lagoon = true; } },
        onCast(c, e) { if (e.unit.state.lagoon) fxAdd(c, 'lagoon'); },
        onRoundEnd(c, e) {
          const m = c.player.mech;
          m.lagoon = (m.lagoon || 0) + (fxOf(e).lagoon || 0);
          while (m.lagoon >= 20) {
            m.lagoon -= 20;
            m.lagoonPaid = (m.lagoonPaid || 0) + 1;
            e.gold += c.val('gold');
            if (m.lagoonPaid % 3 === 0) c.give(c.randomComponent());
          }
        },
      },
    },
    ASSN: { hooks: { onCombatStart(c) { for (const u of c.members) { u.crit += c.v; u.critDmg += c.val('cd'); u.amp += c.val('amp'); u.abilCrit = true; } } } },
    BRUISER: { hooks: { onCombatStart(c) { for (const u of team(c)) u.maxHp += c.v * (c.isMember(u) ? 2 : 1); } } },
    CANNON: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.cannon = c.v; u.state.cannonFlat = c.val('flat'); } },
        onAttack(c, e) {
          const u = e.unit, tg = e.target;
          if (!u.state.cannon) return;
          u.state.cnN = (u.state.cnN || 0) + 1;
          if (u.state.cnN % 4 !== 0) return;
          c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
          for (const en of c.enemiesNear(u.team, tg, 1)) c.damage(u, en, u.ad * u.state.cannon + u.state.cannonFlat, 'phys');
        },
      },
    },
    RIDER: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) {
            u.armor += 2 * c.v; u.mr += 2 * c.v;
            const v = c.v;
            c.after(4, () => { u.armor -= v; u.mr -= v; }, u);
          }
        },
      },
    },
    TAMER: {
      hooks: {
        onCombatStart(c) {
          let hero = null, best = -1, stars = 0;
          for (const u of c.members) {
            stars += u.stage;
            const sc = u.items.length * 100 + u.line.tier * 10 + u.stage;
            if (sc > best) { best = sc; hero = u; }
          }
          if (!hero) return;
          const k = 1 + 0.1 * stars;
          const ap = c.val('ap') * k;
          hero.maxHp += Math.round(c.v * k);
          hero.ap += ap;
          hero.ad += hero.baseAd * ap / 100;
          hero.state.hero = true;
          c.text(hero, 'Hero');
        },
      },
    },
    EVOKER: {
      hooks: {
        onAnyCast(c) { for (const u of c.members) if (u.alive) u.mana = Math.min(u.maxMana, u.mana + c.v); },
      },
    },
    GUARD: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.guard = c.v; },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!u.state.guard || u.state.guardUsed || u.hp >= u.maxHp * 0.5) return;
          u.state.guardUsed = true;
          const amt = u.maxHp * u.state.guard;
          c.shield(u, amt);
          let ally = null, bd = Infinity;
          for (const a of c.alliesOf(u)) { if (a.summoned) continue; const d = c.dist(u, a); if (d < bd) { bd = d; ally = a; } }
          if (ally) c.shield(ally, amt);
        },
      },
    },
    MAGE: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.ap *= c.v; u.state.mage = true; } },
        // cast twice: the second cast runs right after the first
        onCast(c, e) {
          const u = e.unit;
          if (!u.state.mage || u.state.mageAgain || !u.alive) return;
          const tg = e.target && e.target.alive ? e.target : c.target(u, 'closest');
          if (!tg) return;
          u.state.mageAgain = true;
          try { c.castAgain(u, tg); } finally { u.state.mageAgain = false; }
        },
      },
    },
    MYSTIC: { hooks: { onCombatStart(c) { for (const u of team(c)) u.mr += c.v; } } },
    MODE: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.mode = c.v; },
        onCast(c, e) {
          const u = e.unit;
          if (!u.state.mode || u.state.modeDone) return;
          u.state.modeDone = true;
          const add = Math.round(u.maxHp * u.state.mode);
          u.maxHp += add;
          c.heal(u, add);
          c.text(u, 'Mode Change');
        },
      },
    },
    SWIFT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.range += 1; u.state.swift = c.v; } },
        onAttack(c, e) { const u = e.unit; if (u.state.swift) u.atkCd /= 1 + u.state.swift * c.dist(u, e.target); },
      },
    },
    WARRIOR: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.warrior = c.v; },
        onAttack(c, e) { const u = e.unit; if (u.state.warrior && c.rng() < 0.5) u.state.wEmp = true; },
        onBeforeDamage(c, e) {
          const u = e.unit;
          if (!u.state.wEmp || e.kind !== 'phys' || e.isBurn) return;
          u.state.wEmp = false;
          e.mult *= 1 + u.state.warrior;
        },
      },
    },
    DRAGON: {
      hooks: {
        onCombatStart(c) {
          // rules.drgTraitMax (Normal): the bonus turns off with more Dragons than this
          const R = DT.Sets.rulesFor(SET, c.info && c.info.pace);
          if (R && R.drgTraitMax && c.n > R.drgTraitMax) return;
          for (const u of c.members) {
            u.maxHp += c.v;
            if (c.lv >= 2) { u.armor += c.val('armor'); u.mr += c.val('armor'); }
            if (c.lv >= 3) u.amp += c.val('amp');
          }
        },
      },
    },
    STARCALL: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || c.state.done || !c.info || c.info.type !== 'pvp') return;
          c.state.done = true;
          if (sideMech(c).hpFull) {
            const foes = c.enemiesOf(u).filter((x) => !x.summoned);
            if (foes.length) { const f = foes[Math.floor(c.rng() * foes.length)]; c.text(f, 'Wished away'); c.damage(u, f, f.hp + f.shield + 1, 'true'); }
          } else c.teamState(c.team).wish = u.stage === 2 ? 2 : 1;
          c.text(u, 'Wish');
        },
        onRoundEnd(c, e) { const w = fxOf(e).wish; if (w) c.player.hp = Math.min(c.R.startHp, c.player.hp + w); },
      },
    },
    COPYCAT: { hooks: {} }, // the 'copy' ability type does the work
    CORE: {
      hooks: {
        onCast(c, e) { if (c.isMember(e.unit)) fxAdd(c, 'doots'); },
        onCombatEnd(c) {
          if (!c.info || c.info.type !== 'pvp') return;
          for (const u of c.survivors(c.team)) if (!u.summoned && c.rng() < 0.02) fxAdd(c, 'doots');
        },
        onRoundEnd(c, e) { const m = c.player.mech; m.doots = Math.min(20, (m.doots || 0) + (fxOf(e).doots || 0)); },
      },
    },
    MONO: {
      hooks: {
        onCombatStart(c) {
          const spots = ['2,4', '4,4', '3,6'];
          for (const u of team(c)) {
            const col = (c.team === 0 ? u.c : c.cols - 1 - u.c) % COLS; // board column (Duos: either half)
            if (spots.includes(`${col},${u.homeR}`)) { u.dr += c.v; c.text(u, 'Monolith'); }
          }
        },
      },
    },
  };

  // ---------- items ----------
  const items = {
    drg_night_veil: {
      hooks: {
        onDamageTaken(c, e) {
          const u = c.unit;
          if (u.state.veil || u.hp >= u.maxHp * 0.6) return;
          u.state.veil = true;
          u.invulnUntil = c.t + 1;
          u.stun = 0; u.burn = null;
          c.buff(u, 'as', 0.4);
          c.text(u, 'Night Veil');
        },
      },
    },
    drg_herald_banner: { hooks: { onCombatStart(c) { const h = c.unit; for (const a of team(c)) if (sameRow(a, h, 1)) a.as *= 1.3; } } },
    drg_chalice: { hooks: { onCombatStart(c) { const h = c.unit; for (const a of team(c)) if (sameRow(a, h, 1)) a.ap += 30; } } },
    drg_solar_locket: { hooks: { onCombatStart(c) { const h = c.unit, amt = [250, 350, 450][h.stage]; for (const a of team(c)) if (sameRow(a, h, 2)) a.shield += amt; } } },
    drg_banshee: { hooks: { onCombatStart(c) { const h = c.unit, amt = [200, 300, 450][h.stage]; for (const a of team(c)) if (sameRow(a, h, 1)) a.shield += amt; } } },
    drg_rapid_cannon: { hooks: { onCombatStart(c) { c.unit.range += 1; } } },
    drg_justice_hand: { hooks: { onCombatStart(c) { const u = c.unit; if (c.rng() < 0.5) u.amp += 0.12; else u.omnivamp += 0.12; } } },
    drg_lure_portal: {
      hooks: {
        onCombatStart(c) { const h = c.unit; for (const en of c.enemiesNear(h.team, h, 4)) en.target = h; },
        onDeath(c) {
          const h = c.unit;
          const v = c.summon(h.team, 'drg_voidling', h.stage, h.c, h.r);
          if (v) { v.target = null; for (const en of c.enemiesNear(v.team, v, 2)) en.target = v; }
        },
      },
    },
    drg_frost_heart: {
      hooks: {
        onUnitTick(c) {
          const h = c.unit;
          if (!c.every(h, 'frostHeart', 1)) return;
          for (const en of c.enemiesNear(h.team, h, 2)) c.buff(en, 'as', -0.25, 1);
        },
      },
    },
    drg_shroud: {
      hooks: {
        onCombatStart(c) {
          const h = c.unit;
          for (const en of c.units) {
            if (en.team === h.team || !en.alive || Math.abs(en.c - h.c) > 1 || en.state.shroud) continue;
            en.state.shroud = en.maxMana;
            en.maxMana = Math.round(en.maxMana * 1.35);
          }
        },
        onAnyCast(c, e) { const u = e.unit; if (u.state.shroud) { u.maxMana = u.state.shroud; u.state.shroud = 0; } },
      },
    },
    drg_dragon_claw: {
      hooks: {
        onUnitTick(c) {
          const h = c.unit;
          if (!c.every(h, 'dClaw', 2)) return;
          const n = c.enemiesOf(h).filter((x) => x.target === h).length;
          if (n) c.heal(h, h.maxHp * 0.012 * n * (isDragon(h) ? 1.2 : 1), true);
        },
      },
    },
    drg_zephyr: {
      hooks: {
        onCombatStart(c) {
          const h = c.unit;
          const spot = { c: c.cols - 1 - h.c, r: c.rows - 1 - h.r };
          let best = null, bd = Infinity;
          for (const en of c.enemiesOf(h)) { const d = c.dist(spot, en); if (d < bd) { bd = d; best = en; } }
          if (best) { c.stun(best, 4); c.text(best, 'Blown away'); }
        },
      },
    },
    // ---- Golden Armor items ----
    drg_g_axe: {
      hooks: {
        onCombatStart(c) { const u = c.unit; u.ad += u.baseAd * 0.03 * bankOf(c); },
        onAttack(c) { fxAdd(c, 'axe'); },
      },
    },
    drg_g_blade: {
      hooks: {
        onCombatStart(c) { c.unit.as *= 1 + 0.03 * bankOf(c); },
        onAttack(c) {
          if ((c.state.g || 0) >= 3 || c.rng() >= 0.1) return;
          c.state.g = (c.state.g || 0) + 1;
          fxAdd(c, 'gold');
          c.text(c.unit, '+1 gold');
        },
      },
    },
    drg_g_staff: {
      hooks: {
        onCombatStart(c) { c.unit.ap += 4 * bankOf(c); },
        onKill(c) {
          if ((c.state.g || 0) >= 6 || c.rng() >= 0.8) return;
          c.state.g = (c.state.g || 0) + 2;
          fxAdd(c, 'gold', 2);
          c.text(c.unit, '+2 gold');
        },
      },
    },
    drg_g_mail: {
      hooks: {
        onDamageTaken(c) {
          const u = c.unit;
          if ((c.state.n || 0) >= 40) return;
          c.state.n = (c.state.n || 0) + 1;
          u.armor += 1; u.mr += 1; u.maxHp += 8; u.hp += 8;
          if (c.state.n === 40) { fxAdd(c, 'gold', 2); c.text(u, '+2 gold'); }
        },
      },
    },
    drg_g_gem: {
      hooks: {
        onTick(c) {
          const h = c.unit;
          if (c.state.done || c.t < 15 || !h.alive) return;
          c.state.done = true;
          const alive = c.survivors(h.team).filter((u) => !u.summoned);
          for (const u of alive) u.amp += 0.15 + 0.02 * bankOf(c);
          const g = Math.floor(alive.length / 2);
          if (g) { fxAdd(c, 'gold', g); c.text(h, `+${g} gold`); }
        },
      },
    },
    drg_g_investor: { hooks: { onDeath(c) { fxAdd(c, 'investor'); } } },
    drg_g_diamond: {
      hooks: {
        onDamageTaken(c) {
          const u = c.unit;
          for (const [k, at] of [['a', 0.66], ['b', 0.33]]) {
            if (c.state[k] || u.hp >= u.maxHp * at) continue;
            c.state[k] = true;
            u.invulnUntil = c.t + 1;
            fxAdd(c, 'gold');
            c.text(u, '+1 gold');
          }
        },
      },
    },
    drg_g_crown: {
      hooks: {
        onAttack(c, e) {
          if (c.t < (c.state.next || 0) || !e.target.alive) return;
          c.state.next = c.t + 4;
          const b = bankOf(c);
          if (b > 0) c.damage(c.unit, e.target, 20 * b, 'true');
        },
      },
    },
  };

  // consumables
  function giveBack(c, u, idx, reforge) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.game.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    let back = u.items;
    if (reforge) {
      back = back.map((x) => {
        const it = DT.ITEMS[x];
        if (DT.isComponent(x)) return x === 'egg' ? 'egg' : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (it.emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (it.cat === 'radiant') return c.pick(radiantIds().filter((k) => k !== x));
        if (it.cat !== 'completed' || it.noDrop) return x; // golden items, the crown
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  function duplicate(c, u, idx, maxTier) {
    const p = c.player, g = c.game, L = DT.LINE[u.line];
    if (!L || !L.shop || L.tier > maxTier) return 'none';
    if (!(g.pool[L.id] > 0)) return 'none';
    const free = p.freeBenchSlot();
    if (free < 0) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    g.takeFromPool(L.id);
    p.bench[free] = g.newUnit(L.id, 0);
    g.combine(p);
    return id;
  }
  items.drg_remover = { use: (c, u, idx) => giveBack(c, u, idx, false) };
  items.drg_reforger = { use: (c, u, idx) => giveBack(c, u, idx, true) };
  items.drg_dup_lesser = { use: (c, u, idx) => duplicate(c, u, idx, 3) };
  items.drg_dup = { use: (c, u, idx) => duplicate(c, u, idx, 5) };

  // ---------- augments ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of team(c)) fn(u, c); } });
  const adjAllies = (c, u) => c.adjacent(u).filter((a) => a.team === u.team && !a.summoned);
  const augments = {
    drg_hoard: { hooks: { onPick(c) { c.player.gold += 8; } } },
    drg_feather: { hooks: each((u) => { if (u.line.tier <= 2) u.as *= 1.25; }) },
    drg_exiles: { hooks: each((u, c) => { if (!adjAllies(c, u).length) u.shield += u.maxHp * 0.3; }) },
    drg_battlemage: { hooks: each((u) => { if (u.homeR <= 5) { u.armor += 20; u.ap += 20; } }) },
    drg_friends: { hooks: each((u, c) => { if (adjAllies(c, u).length === 1) { u.as *= 1.15; u.armor += 20; } }) },
    drg_calcloss: {
      hooks: {
        onRoundEnd(c, e) { if (!e.won && !e.pve) { e.gold += 2; c.player.mech.calcRoll = 1; } },
        onRoundStart(c) { const m = c.player.mech; if (m.calcRoll) { c.player.freeRolls += 1; m.calcRoll = 0; } },
      },
    },
    drg_firstaid: { hooks: { onCombatStart(c) { const t = c.teamState(c.team); t.healAmp = (t.healAmp || 0) + 0.3; } } },
    drg_training: {
      hooks: {
        onCombatStart(c) { const k = Math.min(40, sideMech(c).ctKills || 0); for (const u of team(c)) u.ad += u.baseAd * (0.08 + 0.005 * k); },
        onKill(c) { fxAdd(c, 'ctKills'); },
        onRoundEnd(c, e) { const m = c.player.mech; m.ctKills = Math.min(40, (m.ctKills || 0) + (fxOf(e).ctKills || 0)); },
      },
    },
    drg_duplicates: { hooks: { onPick(c) { c.give('drg_dup_lesser'); c.give('drg_dup_lesser'); } } },
    drg_scraps: { hooks: { onPick(c) { c.give(c.randomComponent()); c.player.gold += 3; } } },
    drg_ascension: { hooks: { onTick(c) { if (c.state.done || c.t < 15) return; c.state.done = true; for (const u of c.survivors(c.team)) u.amp += 0.4; } } },
    drg_celestial: { hooks: each((u) => { u.omnivamp += 0.18; }) },
    drg_implants: { hooks: each((u) => { if (u.items.length) { u.maxHp += 200; u.ad += u.baseAd * 0.15; } }) },
    drg_shell: { hooks: each((u) => { if (u.items.length) { u.maxHp += 200; u.armor += 30; } }) },
    drg_uplink: {
      hooks: {
        onCombatStart(c) { for (const u of team(c)) if (u.items.length) { u.maxHp += 200; u.state.uplink = true; } },
        onUnitTick(c, e) { const u = e.unit; if (u.state.uplink && c.every(u, 'uplink', 1)) u.mana = Math.min(u.maxMana, u.mana + 3); },
      },
    },
    drg_grabbag: { hooks: { onPick(c) { for (let k = 0; k < 3; k++) c.give(c.randomComponent()); } } },
    drg_archives: { hooks: { onPick(c) { c.give(c.randomEmblem(true)); } } },
    drg_imperialist: {
      hooks: {
        onPick(c) { giveLine(c, 'drg_dorumon'); },
        onCombatStart(c) { c.state.on = new Set(team(c).filter(isDragon).map((u) => u.line.id)).size === 1; },
        onDamageDealt(c, e) { if (c.state.on && isDragon(e.unit) && e.target.alive && e.target.hp < e.target.maxHp * 0.18) execute(c, e.unit, e.target); },
      },
    },
    drg_axiom: { hooks: { onKill(c, e) { const u = e.unit; if (u.alive) u.mana = Math.min(u.maxMana, u.mana + 30); } } },
    drg_bigfriend: {
      hooks: {
        onCombatStart(c) { for (const u of team(c)) if (adjAllies(c, u).some((a) => a.maxHp >= 1800)) u.state.bigFriend = true; },
        onBeforeDamageTaken(c, e) { if (e.unit.state.bigFriend) e.dr += 0.15; },
      },
    },
    drg_double: {
      hooks: {
        onCombatStart(c) {
          const n = {};
          for (const u of team(c)) n[u.line.id] = (n[u.line.id] || 0) + 1;
          for (const u of team(c)) if (n[u.line.id] === 2) { u.ad += u.baseAd * 0.25; u.ap += 25; u.armor += 25; u.mr += 25; }
        },
      },
    },
    drg_electro: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!e.crit || u.summoned || u.state.elCd > c.t) return;
          u.state.elCd = c.t + 1;
          const dmg = 80 + 25 * ((c.info && c.info.stage) || 3);
          for (const en of c.enemiesNear(u.team, u, 1)) c.damage(u, en, dmg, 'magic');
        },
      },
    },
    drg_treasure_hoard: { hooks: { onPick(c) { c.player.gold += 4; } } },
    drg_built: {
      hooks: each((u, c) => {
        if (u.traits.some((t) => c.traitLevel(c.team, t) > 0)) return;
        u.maxHp += 250 + 60 * ((c.info && c.info.stage) || 3);
        u.as *= 1.4;
      }),
    },
    drg_crest_gold: { hooks: { onPick(c) { c.give(DT.emblemOf('GOLD')); giveLine(c, 'drg_kamemon'); } } },
    drg_crest_rage: { hooks: { onPick(c) { c.give(DT.emblemOf('RAGE')); giveLine(c, 'drg_impmon'); } } },
    drg_crest_tamer: { hooks: { onPick(c) { c.give(DT.emblemOf('TAMER')); giveLine(c, 'drg_gotsumon'); } } },
    drg_crest_mage: { hooks: { onPick(c) { c.give(DT.emblemOf('MAGE')); giveLine(c, 'drg_syakomon'); giveLine(c, 'drg_tsukaimon'); } } },
    drg_jackpot: { hooks: { onPick(c) { c.player.gold += 30; } } },
    drg_relics: { hooks: { onPick(c) { c.give(c.pick(radiantIds())); } } },
    drg_crown: { hooks: { onPick(c) { c.give('drg_crown'); } } },
    drg_wings: { hooks: each((u) => { if (isDragon(u)) { u.maxHp = Math.round(u.maxHp * 1.4); u.amp += 0.3; } }) },
    drg_pact: {
      hooks: {
        onPick(c) {
          const list = shopLines((l) => l.tier === 4 && isDragonLine(l) && c.game.pool[l.id] > 0);
          if (list.length) giveLine(c, c.pick(list).id);
          c.player.teamBonus += 1;
        },
      },
    },
    drg_final: {
      hooks: {
        onCombatStart(c) { for (const u of team(c)) u.amp += 0.15; },
        onTick(c) { if (c.state.done || c.t < 15) return; c.state.done = true; for (const u of c.survivors(c.team)) u.amp += 0.45; },
      },
    },
  };

  // ---------- Copycat ability (Millenniummon's line) ----------
  const COPY_NAMES = ['Blast', 'Explosion', 'Time Stop', 'Barrage', 'Restore'];
  const abilities = {
    copy: {
      base: 280,
      cast(c, u, tg, v) {
        const k = Math.floor(c.rng() * 5);
        c.text(u, COPY_NAMES[k]);
        if (k === 0) { c.event({ type: 'beam', from: u.id, to: tg.id }); c.abilityHit(u, tg, v * 1.4); }
        else if (k === 1) { c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team }); for (const en of c.enemiesNear(u.team, tg, 1)) c.abilityHit(u, en, v); }
        else if (k === 2) { c.event({ type: 'beam', from: u.id, to: tg.id }); c.stun(tg, 1.5); c.abilityHit(u, tg, v); }
        else if (k === 3) {
          for (let i = 0; i < 5; i++) {
            const pool = c.enemiesOf(u);
            if (!pool.length) break;
            const en = pool[Math.floor(c.rng() * pool.length)];
            c.event({ type: 'shot', from: u.id, to: en.id, big: true });
            c.abilityHit(u, en, v * 0.4);
          }
        } else {
          const allies = c.units.filter((a) => a.alive && a.team === u.team && !a.summoned).sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id).slice(0, 2);
          for (const a of allies) c.heal(a, v * 0.9);
        }
      },
      text: (line, stage, v) => `Casts a random ability: a ${Math.round(v * 1.4)} damage blast, a ${v} damage explosion, a ${v} damage stun, 5 shots of ${Math.round(v * 0.4)}, or heals 2 allies for ${Math.round(v * 0.9)}.`,
    },
  };

  // ---------- Treasure Dragon (offer kind drg_treasure at 5-1) ----------
  // Bundle ids describe their own content, so they replay identically: gold:N, comp:a.b.c, item:x.comp,
  // emblem:id, dup:N, unit:lineId, rad:id, chaos:N.
  function bundle(c, kind) {
    const g = c.game;
    switch (kind) {
      case 'gold': return `gold:${9 + Math.floor(c.rng() * 4)}`;
      case 'comp': return `comp:${[c.randomComponent(), c.randomComponent(), c.randomComponent()].join('.')}`;
      case 'item': return `item:${c.randomCompleted()}.${c.randomComponent()}`;
      case 'emblem': return `emblem:${c.randomEmblem(true)}`;
      case 'dup': return 'dup:5';
      case 'unit': {
        const list = shopLines((l) => l.tier === 5 && !isDragonLine(l) && g.pool[l.id] > 0);
        return list.length ? `unit:${c.pick(list).id}` : 'gold:10';
      }
      case 'rad': return `rad:${c.pick(radiantIds())}`;
      default: return `chaos:${1 + Math.floor(c.rng() * 9999)}`;
    }
  }
  const BUNDLE_KINDS = ['gold', 'comp', 'item', 'item', 'emblem', 'dup', 'unit', 'rad'];
  function treasureOptions(c, exclude = []) {
    const p = c.player, ver = (c.game.mech && c.game.mech.treasure) || 'classic';
    const won = p.mech.treasureWon !== false;
    const n = (won ? 4 : 3) + (p.augs.includes('drg_treasure_hoard') ? 2 : 0);
    const out = [];
    const add = (id) => { if (id && !out.includes(id) && !exclude.includes(id)) { out.push(id); return true; } return false; };
    if (ver === 'order') for (let k = 0; k < 6 && !add(bundle(c, 'rad')); k++);
    for (let guard = 0; out.length < n && guard < 60; guard++) {
      const kind = ver === 'chaos' && c.rng() < 0.5 ? 'chaos' : BUNDLE_KINDS[Math.floor(c.rng() * (won ? BUNDLE_KINDS.length : BUNDLE_KINDS.length - 1))];
      add(bundle(c, kind === 'rad' && ver === 'classic' && c.rng() < 0.6 ? 'item' : kind));
    }
    return out;
  }
  function chooseTreasure(c, id) {
    const p = c.player;
    const [kind, rest = ''] = String(id).split(':');
    const parts = rest.split('.').filter(Boolean);
    switch (kind) {
      case 'gold': p.gold += Number(parts[0]) || 0; break;
      case 'comp': case 'item': case 'emblem': case 'rad': for (const x of parts) if (DT.ITEMS[x]) c.give(x); break;
      case 'dup': c.give('drg_dup'); p.gold += Number(parts[0]) || 0; break;
      case 'unit': if (DT.LINE[parts[0]]) giveLine(c, parts[0]); p.gold += 2; break;
      case 'chaos':
        if (c.rng() < 0.5) { c.give(c.pick(radiantIds())); c.give(c.randomCompleted()); }
        else p.gold += 4;
        break;
    }
  }
  const itemName = (x) => (DT.ITEMS[x] ? DT.ITEMS[x].name : x);
  function treasureCard(id) {
    const [kind, rest = ''] = String(id).split(':');
    const parts = rest.split('.').filter(Boolean);
    const img = (x) => (G.DT && DT.ItemArt && DT.ItemArt.img && DT.ITEMS[x] ? DT.ItemArt.img(x, 64) : '');
    switch (kind) {
      case 'gold': return { name: `${parts[0]} Gold`, desc: `A pile of ${parts[0]} gold.`, img: '' };
      case 'comp': return { name: 'Component Hoard', desc: parts.map(itemName).join(', '), img: img(parts[0]) };
      case 'item': return { name: itemName(parts[0]), desc: `${itemName(parts[0])} and a ${itemName(parts[1])}.`, img: img(parts[0]) };
      case 'emblem': return { name: itemName(parts[0]), desc: 'A Crest for your board.', img: img(parts[0]) };
      case 'dup': return { name: 'Duplicator Stash', desc: `A Champion Duplicator and ${parts[0]} gold.`, img: img('drg_dup') };
      case 'unit': return { name: DT.LINE[parts[0]] ? DT.LINE[parts[0]].forms[0] : 'Digimon', desc: 'A 5-cost Digimon for your bench and 2 gold.', img: '' };
      case 'rad': return { name: itemName(parts[0]), desc: 'A Shining item.', img: img(parts[0]) };
      default: return { name: 'Mystery Hoard', desc: 'Half the time a Shining item and a completed item. Otherwise 4 gold.', img: '' };
    }
  }
  function treasureScore(c, id) {
    const [kind, rest = ''] = String(id).split(':');
    const parts = rest.split('.').filter(Boolean);
    const counts = DT.countTraits(c.player.boardUnits(), c.player.augs);
    switch (kind) {
      case 'gold': return (Number(parts[0]) || 0) * 0.5 + (c.stage <= 5 ? 1 : 0);
      case 'comp': return 6;
      case 'item': return 7;
      case 'emblem': { const t = DT.ITEMS[parts[0]] && DT.ITEMS[parts[0]].emblem; return counts[t] ? 7.5 : 3; }
      case 'dup': return 5;
      case 'unit': return 6;
      case 'rad': return 9;
      default: return 6.5;
    }
  }

  // ---------- wild-round rewards ----------
  function pveReward(c, won) {
    const g = c.game, s = c.stage, out = [];
    if (s === 1) {
      out.push({ kind: 'blue', items: [c.randomComponent()] });
      if (won) out.push({ kind: 'blue', gold: c.round === 3 ? 2 : 1 });
      if (won && c.round === 3) {
        const list = shopLines((l) => l.tier <= 2 && g.pool[l.id] > 0);
        if (list.length) out.push({ kind: 'gold', unit: c.pick(list).id });
      }
      return out;
    }
    if (s === 4) { // the Treasure Dragon: its loot is the pick at 5-1
      c.player.mech.treasureWon = !!won;
      out.push({ kind: 'blue', gold: 3 });
      return out;
    }
    if (s <= 3) {
      if (won) out.push({ kind: 'blue', gold: 2 });
      out.push({ kind: 'blue', items: [c.randomComponent(s >= 3)] });
      if (won && s === 3) out.push({ kind: 'blue', items: [c.randomComponent()] });
      return out;
    }
    out.push({ kind: 'blue', gold: won ? 3 : 2 });
    if (won) out.push({ kind: 'gold', items: [c.randomCompleted()] });
    else out.push({ kind: 'blue', items: [c.randomComponent()] });
    if (won && c.rng() < 0.25) out.push({ kind: 'gold', items: [c.randomEmblem(true)] });
    if (won && c.rng() < 0.2 && DT.CONSUMABLE_IDS.length) out.push({ kind: 'gold', items: [c.pick(DT.CONSUMABLE_IDS)] });
    return out;
  }
  const def = DT.Sets.def(SET);
  if (def && def.pve && typeof def.pve === 'object') def.pve.reward = pveReward;

  // ---------- the set mechanic ----------
  // Golden Armor: strip golden items from the item bench and benched Digimon, then top the board up to the trait's count.
  function goldenArmor(c) {
    const p = c.player;
    p.items = p.items.filter((id) => !isGoldItem(id));
    for (const u of p.bench) if (u && u.items.some(isGoldItem)) u.items = u.items.filter((id) => !isGoldItem(id));
    const counts = DT.countTraits(p.boardUnits(), p.augs);
    const lv = DT.traitLevel('GOLD', counts.GOLD || 0);
    if (!lv) return;
    const want = DT.TRAITS.GOLD.lv[lv - 1];
    const owned = p.boardUnits().flatMap((u) => u.items.filter(isGoldItem));
    let pool = goldItemIds().filter((id) => id !== 'drg_g_diamond' && (id !== 'drg_g_crown' || lv >= 3) && !owned.includes(id));
    for (let k = owned.length; k < want && pool.length; k++) {
      const id = c.pick(pool);
      pool = pool.filter((x) => x !== id);
      c.give(id);
    }
  }
  // Digi-Cores: move up to 20% of the shop odds from the cheapest costs to the top costs the level allows
  function dootOdds(odds, doots) {
    const o = odds.slice();
    let d = doots;
    for (let i = 0; i < 2 && d > 0; i++) { const t = Math.min(d, o[i]); o[i] -= t; d -= t; }
    const moved = doots - d;
    if (!moved) return odds;
    const top = o[4] > 0 ? [3, 4] : o[3] > 0 ? [2, 3] : [1, 2];
    o[top[0]] += moved * 0.6;
    o[top[1]] += moved * 0.4;
    return o;
  }
  const mechanic = {
    id: 'dragons',
    name: 'Dragons and Treasure',
    desc: 'Dragons take 2 team slots, cost more and count as 3 for their marked trait. Round 4-4 is a Treasure Dragon whose loot you pick at 5-1. Augments come at 2-1, 3-2 and 4-2 with one reroll for the game.',
    hooks: {
      onGameStart(c) {
        const g = c.game;
        g.mech.mirage = c.pick(['dancer', 'warlord', 'overload', 'executioner']);
        const x = c.rng();
        g.mech.treasure = x < 0.4 ? 'classic' : x < 0.7 ? 'order' : 'chaos';
      },
      // Dragons count 3 for their marked trait (a line counts once however many copies)
      onCountTraits(counts, units) {
        const seen = new Set();
        for (const u of units) {
          const L = DT.LINE[u.line];
          if (!L || !L.shop || !L.mark || seen.has(L.id)) continue;
          seen.add(L.id);
          counts[L.mark] = (counts[L.mark] || 0) + 2;
        }
      },
      onShop(c, e) {
        const d = c.player.mech.doots;
        if (!d) return;
        const table = DT.SHOP_ODDS;
        e.odds = dootOdds(e.odds || table[Math.min(table.length - 1, e.level)], d);
      },
      onShopRolled(c, e) {
        const p = c.player, g = c.game, shop = e.shop;
        // Stargazers: the shop after a Star Orb shows a Stargazer in its first slot
        if (p.mech.starShop) {
          p.mech.starShop = 0;
          const odds = DT.SHOP_ODDS[Math.min(DT.SHOP_ODDS.length - 1, p.level)];
          const list = shopLines((l) => l.traits.includes('STAR') && odds[l.tier - 1] > 0 && g.pool[l.id] > 0 && l.id !== shop[0]);
          if (list.length) {
            const id = c.pick(list).id;
            if (shop[0]) g.pool[shop[0]] += 1;
            g.takeFromPool(id);
            shop[0] = id;
          }
        }
        // rules.drgShopCut (Normal): from that level, each Dragon a shop rolls is swapped for a non-Dragon of the same
        // cost with that chance, so level 9-10 boards do not fill up with Dragons
        const cut = c.R.drgShopCut;
        if (cut && p.level >= cut.level) {
          for (let i = 0; i < shop.length; i++) {
            const L = shop[i] && DT.LINE[shop[i]];
            if (!isDragonLine(L) || c.rng() >= cut.chance) continue;
            const list = shopLines((l) => l.tier === L.tier && !isDragonLine(l) && g.pool[l.id] > 0);
            const total = list.reduce((n, l) => n + g.pool[l.id], 0);
            if (!total) continue;
            let x = c.rng() * total, id = list[list.length - 1].id;
            for (const l of list) { x -= g.pool[l.id]; if (x < 0) { id = l.id; break; } }
            g.pool[shop[i]] += 1;
            g.takeFromPool(id);
            shop[i] = id;
          }
        }
        if (shop.some((id) => id && isDragonLine(DT.LINE[id]))) p.shopMeta = shop.map((id) => (id && isDragonLine(DT.LINE[id]) ? { cost: DT.LINE[id].price, tag: 'Dragon' } : null));
      },
      onRoundStart(c) {
        const p = c.player;
        p.mech.bank = Math.min(15, Math.max(0, p.gold));
        p.mech.hpFull = p.hp >= c.R.startHp;
        goldenArmor(c);
        // one augment reroll per game: a pick outside the first options shown means the reroll was spent
        if (p.offer && p.offer.kind === 'augment') {
          if (p.mech.augOpts && !p.mech.augRolled) {
            const last = p.augs[p.augs.length - 1];
            if (last && !p.mech.augOpts.includes(last)) p.mech.augRolled = 1;
          }
          p.offer.rerolls = p.mech.augRolled ? 0 : 1;
          p.mech.augOpts = p.offer.options.slice();
        }
        if (p.offer && p.offer.kind === 'drg_treasure' && p.augs.includes('drg_treasure_hoard')) p.offer.rerolls += 1;
      },
      // gold and stacks earned by golden items during the combat
      onRoundEnd(c, e) {
        const fx = fxOf(e), p = c.player, m = p.mech;
        if (fx.gold) e.gold += fx.gold;
        if (fx.axe) {
          m.axe = (m.axe || 0) + fx.axe;
          while (m.axe >= 50) { m.axe -= 50; e.gold += 4; c.give(c.randomComponent()); }
        }
        if (fx.investor) {
          m.investor = (m.investor || 0) + fx.investor;
          if (m.investor >= 4) {
            m.investor = 0;
            const holder = p.allUnits().find((u) => u.items.includes('drg_g_investor'));
            if (holder) holder.items[holder.items.indexOf('drg_g_investor')] = 'drg_g_diamond';
            c.give('drg_dup');
            e.gold += 8;
          }
        }
      },
    },
    offerKinds: {
      drg_treasure: {
        name: 'Treasure Dragon',
        sub: 'Pick one loot bundle from the Treasure Dragon.',
        options: (c, exclude) => treasureOptions(c, exclude),
        choose: (c, id) => chooseTreasure(c, id),
        aiScore: (c, id) => treasureScore(c, id),
        card: (id) => treasureCard(id),
      },
    },
  };

  // selling a Dragon refunds its price, not its tier (shown in the UI too)
  function sellValue(unit, base) {
    const L = DT.LINE[unit.line];
    return isDragonLine(L) && L.price > L.tier ? base + (L.price - L.tier) * DT.unitCopies(unit) : base;
  }

  DT.Sets.extend(SET, { traits, items, augments, abilities, mechanic, sellValue });
})(typeof window !== 'undefined' ? window : globalThis);
