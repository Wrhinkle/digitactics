// Duos format (Double Up / Battlegrounds Duos): 8 Tamers in 4 teams of 2. Rules data and helpers; the match flow is in
// js/game.js (teams, portal, team fights) and the fight in js/combat.js (wide grid, multi-owner sides). No DOM access.
//
// - Teams: seats (0,1) (2,3) (4,5) (6,7) unless the init gives teams. Each team shares one HP pool and is knocked out
//   together; placements go 1-4. Each player keeps their own board, bench, shop, gold, level, items and augments.
// - Portal: during planning a player can send a bench Digimon or an item to their partner, free, `sends` times a round.
//   A sent Digimon counts toward the partner's copies at once, so partners can finish each other's digivolutions.
// - Team fights: every PvP round the alive teams pair up. Both partners' boards stand side by side on one 14 x 8 field
//   (lower seat on the left), the other pair mirrored on the far side. Trait counts are summed across both partners'
//   boards (teamCounts) and trait effects reach every unit on that side; augments, Tamer powers and global items still act
//   for their own partner's units. Wild rounds and carousels stay individual.
// - The losing team loses the pace's stage damage plus unitScale x the unit damage of every surviving enemy unit.
// - Duo augments (below) join every set's augment rounds: one of the three choices is always a duo augment.
(function (G) {
  const DT = G.DT;
  const FORMATS = ['solo', 'duos'];
  const TEAMS = [[0, 1], [2, 3], [4, 5], [6, 7]];

  // Duo rules for a pace's rules R. A set can override any key in rules.duo (or rules.hyper.duo / rules.normal.duo).
  // teamHp: shared HP (the pace's start HP: 20 Hyper Roll, 100 Normal, 4 lives in Underlords Knockout)
  // sends: portal sends per round; unitScale: share of the per-unit loss damage (Normal) a team takes
  function rules(R) {
    const normal = R.pace === 'normal';
    return {
      teamHp: R.startHp,
      sends: normal ? 2 : 1,
      unitScale: 0.5,
      ...(R.duo || {}),
    };
  }

  // valid teams: 4 disjoint pairs of seats 0-7, else the default pairs
  function validTeams(teams) {
    if (!Array.isArray(teams) || teams.length !== 4) return TEAMS.map((t) => t.slice());
    const seen = new Set();
    for (const t of teams) {
      if (!Array.isArray(t) || t.length !== 2) return TEAMS.map((x) => x.slice());
      for (const s of t) { if (!Number.isInteger(s) || s < 0 || s > 7 || seen.has(s)) return TEAMS.map((x) => x.slice()); seen.add(s); }
    }
    return teams.map((t) => t.slice().sort((a, b) => a - b));
  }

  // Team trait counts for a team fight: each partner's own counts (with their augments and the set's onCountTraits),
  // summed. A summed count above the trait's top breakpoint does nothing more, so it is capped there (a board that
  // reaches past it alone, with Crests, keeps its own count). A trait can set `duo` to change how partners combine:
  //   duo: 'sum' (default) | 'max' (the higher partner's count only) | a number k: the lower partner adds k x their count.
  // parts: [{ owner, units (board side entries), augs }]
  function teamCounts(parts) {
    const per = parts.map((p) => DT.countTraits(p.units, p.augs));
    const out = {};
    const keys = [];
    for (const c of per) for (const t in c) if (!(t in out)) { out[t] = 0; keys.push(t); }
    for (const t of keys) {
      const T = DT.TRAITS[t];
      const vals = per.map((c) => c[t] || 0);
      const hi = Math.max(...vals);
      const rest = vals.reduce((a, b) => a + b, 0) - hi;
      const mode = T && T.duo != null ? T.duo : 'sum';
      out[t] = mode === 'max' ? hi : typeof mode === 'number' ? hi + Math.floor(rest * mode) : hi + rest;
    }
    // Crest Link: traits a partner's Crests add count once more
    parts.forEach((p) => {
      if (!(p.augs || []).includes('duo_crest_link')) return;
      for (const u of p.units) {
        const L = DT.LINE[u.line];
        if (!L || !L.shop) continue;
        for (const id of DT.fitItems(u.items || [])) {
          const it = DT.ITEMS[id];
          if (it && it.emblem && !L.traits.includes(it.emblem)) out[it.emblem] = (out[it.emblem] || 0) + 1;
        }
      }
    });
    // Sync Bond: traits active on both boards count once more
    if (per.length === 2 && parts.some((p) => (p.augs || []).includes('duo_sync'))) {
      for (const t in per[0]) if (per[1][t] && DT.traitLevel(t, per[0][t]) && DT.traitLevel(t, per[1][t])) out[t] += 1;
    }
    for (const t in out) {
      const T = DT.TRAITS[t];
      if (!T || !Array.isArray(T.bp) || !T.bp.length) continue;
      const top = T.bp[T.bp.length - 1];
      const own = Math.max(...per.map((c) => c[t] || 0));
      out[t] = Math.min(out[t], Math.max(top, own));
    }
    return out;
  }

  // merged per-player mechanic state for a team side (set helpers that read c.side(team).mech in trait hooks):
  // numbers take the higher partner's value, lists join, anything else comes from the first partner that has it
  function mergeMech(list) {
    const out = {};
    let any = false;
    for (const m of list) {
      if (!m) continue;
      for (const k of Object.keys(m)) {
        any = true;
        const v = m[k];
        if (!(k in out)) { out[k] = Array.isArray(v) ? v.slice() : v; continue; }
        if (typeof v === 'number' && typeof out[k] === 'number') out[k] = Math.max(out[k], v);
        else if (Array.isArray(v) && Array.isArray(out[k])) out[k] = out[k].concat(v);
      }
    }
    return any ? out : null;
  }

  // ---------- duo augments (every set) ----------
  // Game hooks see c.partner (the partner Player, or null); combat hooks c.owner (this player's id), c.partnerUnits()
  // and c.side(c.team).partnerTop (the value of the partner's best board Digimon: cost x copies).
  const once = (p) => (p.duo = p.duo || {});
  const AUGS = [
    { id: 'duo_wide_portal', tier: 1, name: 'Wide Portal', tags: ['econ'], portalSends: 1,
      desc: '+1 portal send each round. Each Digimon or item you send also gives your partner 1 gold.',
      hooks: { onPortal(c, e) { if (e.from === c.player && e.to && e.to.alive) e.to.gold += 1; } } },
    { id: 'duo_proud', tier: 1, name: 'Proud Partner', tags: ['econ'],
      desc: 'Gain 2 gold whenever your partner digivolves a Digimon (up to 3 times a round).',
      hooks: {
        onPartnerCombine(c) {
          const d = once(c.player);
          const r = c.game.roundNo();
          if (d.proudRound !== r) { d.proudRound = r; d.proudN = 0; }
          if (d.proudN >= 3) return;
          d.proudN++;
          c.player.gold += 2;
        },
      } },
    { id: 'duo_gear', tier: 1, name: 'Pass the Gear', tags: ['items'],
      desc: 'Gain 2 random components. Your partner gains 1.',
      hooks: {
        onPick(c) {
          c.give(c.randomComponent());
          c.give(c.randomComponent());
          const q = c.partner;
          if (q && q.alive) c.game.giveItem(q, c.randomComponent());
        },
      } },
    { id: 'duo_interest', tier: 2, name: 'Shared Interest', tags: ['econ'],
      desc: 'Each round, gain 1 gold for every 10 gold your partner is holding (up to 3).',
      hooks: { onIncome(c, e) { const q = c.partner; if (q && q.alive) e.augment += Math.min(3, Math.floor(Math.max(0, q.gold) / 10)); } } },
    { id: 'duo_care', tier: 2, name: 'Care Package', tags: ['econ'],
      desc: 'At the start of every round, a random Digimon goes through the portal to your partner for free (1-2 cost early, up to 4 cost late).',
      hooks: {
        onRoundStart(c) {
          const q = c.partner;
          if (!q || !q.alive || c.game.roundType === 'carousel') return;
          const s = c.stage;
          c.game.portalGift(c.player, q, s <= 2 ? [1, 2] : s <= 4 ? [1, 2, 3] : [2, 3, 4]);
        },
      } },
    { id: 'duo_ace', tier: 2, name: "Partner's Ace", tags: ['combat'],
      desc: "Your Digimon gain 3% attack damage and 3 ability power for each gold your partner's most valuable Digimon is worth (cost x copies, up to 10).",
      hooks: {
        onCombatStart(c) {
          const s = c.side(c.team) || {};
          const v = Math.min(10, s.partnerTop || 0);
          if (!v) return;
          for (const u of c.mine) { u.ad += u.baseAd * 0.03 * v; u.ap += 3 * v; }
        },
      } },
    { id: 'duo_tag', tier: 2, name: 'Tag Team', tags: ['combat'],
      desc: "In team fights, whenever one of your partner's Digimon is knocked out, your Digimon heal 10% of their max HP and gain 15% attack speed for 4 seconds.",
      hooks: {
        onAnyDeath(c, e) {
          const d = e.unit;
          if (!d || d.team !== c.team || d.owner == null || d.owner === c.owner || c.owner == null) return;
          for (const u of c.mine) if (u.alive) { c.heal(u, u.maxHp * 0.1); c.buff(u, 'as', 0.15, 4); }
        },
      } },
    { id: 'duo_crest_link', tier: 2, name: 'Crest Link', tags: ['items'],
      desc: 'Your Crests also count for your partner: in team fights, every trait your Crests add counts one more time for your team.' },
    { id: 'duo_guard', tier: 2, name: 'Team Guard', tags: ['econ'],
      desc: 'Your team loses 25% less HP from lost fights (rounded up). Two Team Guards do not stack.' },
    { id: 'duo_sync', tier: 3, name: 'Sync Bond', tags: ['combat'],
      desc: "In team fights, every trait active on both your board and your partner's counts one more for your team. Your Digimon deal 8% more damage.",
      hooks: { onCombatStart(c) { for (const u of c.mine) u.amp += 0.08; } } },
    { id: 'duo_trove', tier: 3, name: 'Shared Treasure', tags: ['items'],
      desc: 'You and your partner each gain a random completed item and 5 gold.',
      hooks: {
        onPick(c) {
          c.give(c.randomCompleted());
          c.player.gold += 5;
          const q = c.partner;
          if (q && q.alive) { c.game.giveItem(q, c.randomCompleted()); q.gold += 5; }
        },
      } },
  ];
  for (const a of AUGS) { a.kind = 'augment'; a.duo = true; DT.SHARED_AUGMENT[a.id] = a; }

  DT.Duos = {
    FORMATS,
    TEAMS,
    AUGMENTS: AUGS,
    rules,
    validTeams,
    teamCounts,
    mergeMech,
    validFormat: (f) => (f === 'duos' ? 'duos' : 'solo'),
    label: (f) => (f === 'duos' ? 'Duos' : 'Solo'),
  };
})(typeof window !== 'undefined' ? window : globalThis);
