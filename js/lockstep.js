// Multiplayer lockstep. Every client builds the same Game from one seed and applies the same ordered log,
// so every client holds identical state. The host only orders entries and decides when phases change.
// No DOM or network access, so tests can run it in Node.
//
// Log entries:
//   { k: 'a', s, a, g, at }  player action: slot s runs Game.applyAction(a, g)
//   { k: 'vote', s, at }     slot s is ready to fight
//   { k: 'start', at }       planning ends, combat starts
//   { k: 'end', at }         combat ends, damage applied
//   { k: 'next', at, d }     next round (also ends the opening carousel); d = Lock.check of the host's state before it
// `at` is the host's server-clock time in ms; timers count from it.
(function (G) {
  const DT = G.DT;

  const Lock = {
    CAROUSEL_TURN_MS: 12000,
    CAROUSEL_END_MS: 1500,
    RESULT_MS: 3200,
    COMBAT_GRACE_MS: 4000,
    OFFLINE_RELEASE_MS: 20000,

    // init: { seed, slots: [{ name, human }], planTime, at, setId, pace, format, teams }. setId and pace are resolved (the
    // host resolves 'chaos' before writing init); a missing setId (lobbies from older builds) plays the default set, a
    // missing pace plays the set's native pace (Hyper Roll for TFT-mode sets) and a missing format plays solo.
    // format 'duos': teams is 4 pairs of seats ([[0, 1], [2, 3], [4, 5], [6, 7]] when missing), js/duos.js.
    create(init) {
      const game = new DT.Game({ seed: init.seed, slots: init.slots, setId: init.setId || DT.Sets.DEFAULT, pace: init.pace, chaosPace: init.chaosPace,
        format: init.format, teams: init.teams });
      game.planMs = (init.planTime || game.R.planTime) * 1000;
      game.phaseAt = init.at || 0;
      game.turnAt = init.at || 0;
      game.votes = [];
      game.seq = 0;
      return game;
    },

    // Applies one entry. Returns the action result for 'a' entries, true/false for the rest.
    apply(game, e) {
      game.ensureSet();
      game.seq++;
      const at = e.at || 0;
      switch (e.k) {
        case 'a': {
          const p = game.players[e.s];
          let res = null;
          try { res = game.applyAction(p, e.a, e.g || {}); } catch (err) { res = null; }
          if (game.duo) game.syncTeams(); // Duos: a pick or Wisp that changed one Tamer's HP changes the team's
          if (e.a === 'carousel' && res) game.turnAt = at;
          return res;
        }
        case 'vote':
          if (game.phase === 'planning' && !game.votes.includes(e.s)) game.votes.push(e.s);
          return true;
        case 'start':
          if (game.over || game.phase !== 'planning' || game.roundType === 'carousel') return false;
          game.startCombat();
          game.phaseAt = at;
          game.votes = [];
          game.maxCombatMs = null;
          return true;
        case 'end':
          if (game.phase !== 'combat') return false;
          game.finishCombat();
          game.phaseAt = at;
          return true;
        case 'next': {
          const fromCarousel = game.roundType === 'carousel' && game.phase === 'planning';
          if (game.over || !(fromCarousel || game.phase === 'result')) return false;
          if (fromCarousel) game.finishCarousel();
          game.nextRound();
          game.phaseAt = at;
          game.turnAt = at;
          game.votes = [];
          return true;
        }
      }
      return false;
    },

    // Longest watched combat this round, in ms of combat time (replayed on a copy, so state is untouched).
    maxCombatMs(game) {
      if (game.maxCombatMs == null) {
        let t = 0;
        for (const m of game.matches || []) {
          if (!game.matchPlayers(m).some((p) => p.isHuman)) continue;
          t = Math.max(t, DT.simulate(m.sideA, m.sideB, m.seed, m.info).t);
        }
        game.maxCombatMs = t * 1000;
      }
      return game.maxCombatMs;
    },

    // Host only. ctx: { now, online: Set of slots with a connected human, done: Set of slots whose combat replay finished }.
    // Returns the next entry to append (without `at`), or null.
    decide(game, ctx) {
      if (game.over) return null;
      game.ensureSet();
      const { now, online, done } = ctx;
      const humans = game.players.filter((p) => p.alive && p.isHuman && online.has(p.id));
      if (game.roundType === 'carousel' && game.phase === 'planning') {
        if (!game.carousel || game.carouselDone()) return now - game.turnAt >= Lock.CAROUSEL_END_MS ? { k: 'next' } : null;
        const turn = game.carouselTurn();
        if (!online.has(turn.id) || now - game.turnAt >= Lock.CAROUSEL_TURN_MS) return { k: 'a', s: turn.id, a: 'carousel', g: {} };
        return null;
      }
      if (game.phase === 'planning') {
        const allReady = humans.length > 0 && humans.every((p) => game.votes.includes(p.id));
        return now >= game.phaseAt + game.planMs || allReady ? { k: 'start' } : null;
      }
      if (game.phase === 'combat') {
        const watching = humans.filter((p) => game.matchOf(p));
        const allDone = watching.every((p) => done.has(p.id));
        return allDone || now >= game.phaseAt + Lock.maxCombatMs(game) + Lock.COMBAT_GRACE_MS ? { k: 'end' } : null;
      }
      if (game.phase === 'result') return now - game.phaseAt >= Lock.RESULT_MS ? { k: 'next' } : null;
      return null;
    },

    // 8-hex FNV-1a hash of the digest. The host stamps it on 'next' entries so clients can detect a desync.
    check(game) {
      const s = Lock.digest(game);
      let h = 0x811c9dc5;
      for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 0x01000193) >>> 0; }
      return h.toString(16).padStart(8, '0');
    },

    // Compact digest of rule state, to compare clients in tests.
    digest(game) {
      const ps = game.players.map((p) => [p.hp, p.gold, p.level, p.alive, p.isHuman, p.shop.join(','), p.items.join(','),
        Object.entries(p.board).map(([k, u]) => `${k}:${u.line}${u.stage}${u.items.join('+')}`).join(';'),
        p.bench.map((u) => (u ? u.line + u.stage : '-')).join(','), p.augs.join(','), p.orbs.length]
        .concat(game.R && game.R.xp ? [p.xp] : [], game.R && game.R.streak ? [`s${p.streak}`] : [], p.power ? [`${p.power.id}${p.power.rank}`] : [], p.globals && p.globals.length ? [p.globals.join(',')] : [],
          game.duo ? [`t${p.team}p${p.portal}`] : []).join('|'));
      return `${game.duo ? 'duos|' : ''}${game.pace || ''}|${game.roundLabel}|${game.phase}|${JSON.stringify(game.pool)}|${ps.join('#')}`;
    },
  };

  DT.Lock = Lock;
})(typeof window !== 'undefined' ? window : globalThis);
