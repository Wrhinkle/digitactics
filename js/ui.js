// DOM panels, input handling, and the main loop.
(function () {
  const DT = window.DT;
  const H = DT.HYPER;
  const P = DT.Portraits;
  const $ = (id) => document.getElementById(id);
  // Breakpoint effect text: a set may give bpDesc strings instead of (or as well as) fmt().
  const bpText = (T, i) => {
    if (T.bpDesc && T.bpDesc[i] != null) return T.bpDesc[i];
    try { return typeof T.fmt === 'function' ? T.fmt(T.lv ? T.lv[i] : undefined, i) : ''; } catch (e) { return ''; }
  };

  const canvas = $('board');
  const renderer = new DT.Renderer(canvas);
  const ui = {
    drag: null, hoverCell: null, hoverBench: -1, hoverUid: null, hoverCombatId: null,
    hoverShop: -1, hoverItem: -1, hoverWisp: false, hoverTrait: null, hoverPlayer: -1, previewSide: null, speed: 1, mouse: null,
    itemDrag: null, timerOn: true, planLeft: H.planTime, peek: false, cx: 0, cy: 0, tipAnchor: null, chartMode: 'dealt',
    viewing: null, // id of the player being scouted (js/scout.js), null for your own board
  };
  let game = null;
  let combat = null;
  let combatEndAt = 0;
  let acc = 0;
  let last = performance.now();
  let resultTimer = null;
  let resultPending = null;
  let paused = false, pausedAt = 0;
  // Keep the remaining result delay when solo is paused, including spectator retries.
  function clearResultTimer() {
    clearTimeout(resultTimer);
    resultTimer = resultPending = null;
  }
  function armResultTimer() {
    if (!resultPending || paused) return;
    resultPending.due = performance.now() + resultPending.remaining;
    resultTimer = setTimeout(() => {
      const fn = resultPending.fn;
      resultTimer = resultPending = null;
      fn();
    }, resultPending.remaining);
  }
  function scheduleResult(fn, delay) {
    clearResultTimer();
    resultPending = { fn, remaining: delay, due: 0 };
    armResultTimer();
  }
  function pauseSolo() {
    if (!game || session || paused) return;
    paused = true;
    pausedAt = performance.now();
    if (resultPending) {
      resultPending.remaining = Math.max(0, resultPending.due - pausedAt);
      clearTimeout(resultTimer);
      resultTimer = null;
    }
  }
  function resumeSolo() {
    if (!paused) return;
    const now = performance.now();
    if (combatEndAt && Number.isFinite(combatEndAt)) combatEndAt += now - pausedAt;
    last = renderer.clock = now;
    paused = false;
    armResultTimer();
  }
  let chartAt = 0;
  let hist = new Map(); // player id -> [{ round, res, dmg, opp, pve }], local view history only

  // ---------- small helpers ----------
  const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
  const itemName = (id) => (DT.ITEMS[id] ? DT.ITEMS[id].name : id);
  const itemImg = (id, extra = '') => (DT.ITEMS[id] ? `<img class="${DT.ITEMS[id].component ? 'c' : 'f'}" src="${DT.itemIcon(id)}" alt="${esc(itemName(id))}" ${extra}>` : '');
  const coin = '<i class="coin s"></i>';
  const store = {
    get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
    set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
  };

  // ---------- Little Legend (equipped 'legend' cosmetic, js/legends.js; not game state) ----------
  const legendId = () => DT.Cosmetics.equipped('legend');
  // 'id@form' once the legend has digivolved (js/legendevo.js)
  const legendKey = () => (DT.LegendEvo ? DT.LegendEvo.key(legendId()) : legendId());
  renderer.setLegend(legendKey());
  DT.Cosmetics.onChange((kind) => {
    if (kind !== 'legend') return;
    renderer.setLegend(legendKey());
    fillTitleLegend();
    if (!$('picker').classList.contains('hidden') && $('picker').querySelector('.lgown')) openPicker();
    if (game) refresh();
  });

  // A Tamer's picture: your Little Legend, or the strongest Digimon an AI or other player has.
  function avatarHTML(q) {
    if (game && q === game.human) return DT.Legends.html(legendKey());
    const units = q.boardUnits().length ? q.boardUnits() : q.allUnits();
    let best = null;
    for (const u of units) if (!best || DT.unitValue(u) + u.items.length * 0.5 > DT.unitValue(best) + best.items.length * 0.5) best = u;
    if (best) return P.html(best.line, best.stage);
    return `<span class="pt" style="--fc:#556"><b>${esc(q.name.slice(0, 2))}</b></span>`;
  }

  // ---------- game flow ----------
  // Solo and multiplayer share one path: every change is a DT.Lock log entry. Solo commits entries locally;
  // multiplayer gets them from the host through `session` (js/net.js) and sends player actions as intents.
  let session = null; // multiplayer session, null in solo
  let myTeam = 0; // combat team of the viewer this round
  let combatReported = false;
  // Spectating (js/scout.js): the fight on screen may be another Tamer's local replay instead of `combat`.
  let drawn = null; // the Combat the renderer is showing
  let drawnTeam = 0; // the viewed Tamer's team in it
  let clock = 0; // combat time of this round on this client: your own fight's time, else seconds since combat began times speed
  let outShown = false; // the eliminated screen has been shown for this game
  const shownCombat = () => drawn || combat;

  function resetView() {
    paused = false;
    P.cancelPrefetch();
    if (DT.Sfx) { DT.Sfx.combat(false); DT.Sfx.music(true); }
    clearResultTimer();
    combat = null;
    drawn = null;
    clock = 0;
    outShown = false;
    ui.peek = false;
    ui.orbPending = new Set();
    hist = new Map();
    renderer.mirror = false;
    renderer.resetFx();
    clearCasts();
    $('overlay').classList.add('hidden');
    $('banner').classList.add('hidden');
    $('dmgChart').classList.add('hidden');
  }

  // setId: a set id, 'chaos' (picked from the seed) or nothing (the set chosen on the Play page, else the default)
  function newGame(setId) {
    session = null;
    const pick = typeof setId === 'string' ? setId : (DT.SetPicker && DT.SetPicker.selected()) || DT.Sets.DEFAULT;
    // pace: the one chosen on the Play page (Hyper Roll or Normal); DT.UI.newGame(id, pace) for tests and the console
    const pace = typeof arguments[1] === 'string' ? arguments[1] : DT.SetPicker && DT.SetPicker.pace ? DT.SetPicker.pace() : undefined;
    game = DT.Lock.create({ seed: (Math.random() * 1e9) | 0, at: performance.now(), setId: pick, pace, format: typeof arguments[2] === 'string' ? arguments[2] : DT.SetPicker.format() });
    document.body.classList.remove('mp');
    resetView();
    startPlanning();
  }

  // Multiplayer: net.js has built the game from the room's seed and replayed the log so far.
  function startNetGame(s) {
    session = s;
    game = s.game;
    game.human = game.players[s.slot];
    document.body.classList.add('mp');
    resetView();
    s.onEntry = onEntry;
    syncPhaseView();
  }

  // After joining mid-game, show whatever phase the log has reached.
  function syncPhaseView() {
    if (game.phase === 'combat') beginCombatView();
    else if (game.phase === 'result') { const m = game.matchOf(game.human); combat = m ? m.combat : null; endCombatView(); }
    else startPlanning();
    if (game.over) showGameOver();
  }

  function commit(e) {
    if (paused) return;
    e.at = performance.now();
    onEntry(e, DT.Lock.apply(game, e), true);
  }

  // A player action from this client.
  function act(a, g = {}) {
    if (session) return session.send(a, g);
    commit({ k: 'a', s: game.human.id, a, g });
  }

  // Runs after each log entry is applied. live is false while replaying history on join.
  function onEntry(e, res, live) {
    if (!live) return;
    if (e.k === 'a') {
      if (e.s === game.human.id) feedback(e.a, e.g || {}, res);
      if (e.a === 'claim' || e.a === 'release') toast(`${game.players[e.s].name} ${e.a === 'claim' ? 'joined' : 'left (now AI)'}`, 'info');
      refresh();
    } else if (e.k === 'vote') refresh();
    else if (e.k === 'start' && res) beginCombatView();
    else if (e.k === 'end' && res) { endCombatView(); if (DT.KillScreens) DT.KillScreens.onRoundEnd(game); DT.MysteryLoot?.roundEnd(game); }
    else if (e.k === 'next' && res) nextRoundView();
  }

  function startPlanning() {
    ui.planLeft = game.planMs / 1000;
    ui.outPlan = 0;
    ui.peek = false;
    refresh();
  }

  function ready() {
    if (!game || game.phase !== 'planning' || game.over) return;
    if (game.roundType === 'carousel') return;
    if (game.human.offer) { ui.peek = false; renderModal(); return toast('Make your pick first'); }
    if (session) {
      if (!game.human.alive) return;
      session.vote();
      return toast('Ready. Waiting for the other players', 'info');
    }
    commit({ k: 'start' });
  }

  function beginCombatView() {
    // spectating replays every fight from copies of this round's inputs, taken before anything can change them
    if (DT.Scout && DT.Scout.snapshot) DT.Scout.snapshot(game);
    ui.drag = null;
    endItemDrag();
    const m = game.matchOf(game.human);
    combat = m ? m.combat : null;
    myTeam = m ? m.team : 0;
    renderer.mirror = myTeam === 1;
    combatEndAt = 0;
    combatReported = false;
    acc = 0;
    chartAt = 0;
    // joining a multiplayer game mid-combat: the round's clock counts from the host's combat start
    clock = session && !combat ? Math.max(0, (session.now() - game.phaseAt) / 1000) : 0;
    drawn = null;
    combatFresh = true;
    renderer.resetFx();
    clearCasts();
    $('banner').classList.add('hidden');
    refresh();
    if (DT.Sfx) { DT.Sfx.play('fight'); DT.Sfx.combat(true); }
    // No fight of your own (eliminated, or a carousel): multiplayer reports at once (the host times the round);
    // solo waits until the longest fight has played out so a spectator can watch it (see frame()).
    if (!combat && (session || !(game.matches && game.matches.length))) combatWatched();
  }

  // solo, no fight of your own: the round ends once the longest fight would have finished
  function longestFight() {
    let t = 0;
    for (const m of game.matches || []) if (m.combat) t = Math.max(t, m.combat.t);
    return t;
  }

  // This client has finished watching its fight.
  function combatWatched() {
    if (combatReported) return;
    combatReported = true;
    if (session) session.done();
    else commit({ k: 'end' });
  }

  function skip() {
    if (!game || game.phase !== 'combat') return;
    if (!combat) return combatWatched();
    while (!combat.done) combat.step(DT.TICK);
    if (drawn === combat) { // a spectated fight on screen keeps playing
      renderer.resetFx();
      clearCasts();
      renderer.evIdx = combat.events.length;
    }
    combatWatched();
  }

  // Remember every Tamer's result this round for the history strip and the W/L pips.
  function recordResults() {
    const label = game.roundLabel;
    const add = (pl, res, dmg, opp, pve) => {
      const h = hist.get(pl.id) || [];
      if (h.length && h[h.length - 1].round === label) return;
      h.push({ round: label, res, dmg, opp, pve });
      hist.set(pl.id, h);
    };
    for (const r of game.results) {
      const m = r.match;
      if (m.a2) add(m.a2, r.aWon ? 'w' : r.bWon ? 'l' : 'd', r.dmgA, m.b.name, false);
      if (m.b2 && !m.ghost) add(m.b2, r.bWon ? 'w' : r.aWon ? 'l' : 'd', r.dmgB, m.a.name, false);
      add(m.a, r.aWon ? 'w' : r.bWon ? 'l' : 'd', r.dmgA, m.pve ? 'Wild Digimon' : m.b.name, !!m.pve);
      if (m.b && !m.ghost) add(m.b, r.bWon ? 'w' : r.aWon ? 'l' : 'd', r.dmgB, m.a.name, false);
    }
  }

  function endCombatView() {
    const finishedGame = game;
    const me = game.human;
    recordResults();
    const r = game.results.find((x) => (x.match.a === me || x.match.a2 === me) || ((x.match.b === me || x.match.b2 === me) && !x.match.ghost));
    if (r) showResult(r);
    renderChart(true);
    refresh();
    clearResultTimer();
    scheduleResult(() => {
      if (game !== finishedGame) return;
      $('banner').classList.add('hidden');
      if (session) {
        if (game.over) return showGameOver();
        if (!me.alive) { if (!outShown) { outShown = true; showEliminated(); } return; }
        return;
      }
      // solo: knocked out with AI Tamers still standing, offer to spectate the rest (Game.playOn)
      if (!me.alive && !outShown && game.alivePlayers().length > 1) { outShown = true; return showEliminated(); }
      if (game.over) return showGameOver();
      // spectating a fight that is still going: let it finish before the next round (solo only, nobody waits on you)
      const g = game;
      const next = (n) => {
        if (game !== g || g.phase !== 'result') return;
        if (ui.viewing != null && drawn && !drawn.done && n < 200) { scheduleResult(() => next(n + 1), 250); return; }
        commit({ k: 'next' });
      };
      next(0);
    }, 2200);
  }

  function nextRoundView() {
    clearResultTimer();
    $('banner').classList.add('hidden');
    combat = null;
    drawn = null;
    renderer.mirror = false;
    ui.orbPending = new Set();
    renderer.resetFx();
    clearCasts();
    startPlanning();
    announceRound();
  }

  const roundName = (t) => ({ carousel: 'Carousel', pve: 'Wild Digimon', pvp: 'PvP' })[t] || t;
  const offerName = (k) => ({ armory: 'Armory', augment: 'Augment', power: 'Tamer Power', itemTier: 'Item Pick' })[k] || ((game && game.set.mechanic.offerKinds[k]) || {}).name || k;

  // Buy XP button, shown only in sets with rules.xp (F key)
  function xpButton(p) {
    const X = game.R.xp;
    let b = $('xpBtn');
    if (!X) { if (b) b.classList.add('hidden'); return; }
    if (!b) {
      b = document.createElement('button');
      b.id = 'xpBtn';
      b.title = 'F';
      b.onclick = buyXp;
      $('shopBtns').prepend(b);
    }
    b.classList.remove('hidden');
    b.innerHTML = `Buy XP <span class="cost">${coin} ${X.buyCost || 4}</span>`;
    b.disabled = p.gold < (X.buyCost || 4) || p.level >= (X.maxLevel || 9);
  }
  // Normal pace (rules.xp): an XP bar under the level; hidden in Hyper Roll
  function xpBar(p, X, need) {
    let bar = $('xpBar');
    if (!X) { if (bar) bar.classList.add('hidden'); return; }
    if (!bar) {
      bar = document.createElement('div');
      bar.id = 'xpBar';
      bar.innerHTML = '<i></i>';
      $('levelBox').appendChild(bar);
    }
    bar.classList.remove('hidden');
    const max = p.level >= (X.maxLevel || 9);
    bar.firstChild.style.width = `${max ? 100 : Math.max(0, Math.min(100, (100 * p.xp) / (need || 1)))}%`;
    bar.title = max ? 'Max level' : `${p.xp}/${need} XP to level ${p.level + 1}. Buy ${X.buyXp || 4} XP for ${X.buyCost || 4} gold (F); +${X.perRound != null ? X.perRound : 2} XP each round.`;
  }
  // Interest and streak gold (sets or paces with rules.interest / rules.streak), under the gold
  function econLine(p, inc) {
    const R = game.R;
    let el = $('econLine');
    if (!R.interest && !R.streak) { if (el) el.classList.add('hidden'); return; }
    if (!el) {
      el = document.createElement('div');
      el.id = 'econLine';
      $('goldBox').insertAdjacentElement('afterend', el);
    }
    el.classList.remove('hidden');
    const parts = [];
    if (R.interest) {
      const per = R.interest.per || 10, cap = R.interest.max || 5;
      const next = inc.interest < cap ? ` (${per * (inc.interest + 1) - Math.max(0, p.gold)} to +${inc.interest + 1})` : ' (max)';
      parts.push(`<span class="ec-int" title="1 gold per ${per} saved, up to ${cap}">Interest <b>+${inc.interest}</b>${next}</span>`);
    }
    if (R.streak) {
      const s = p.streak;
      const kind = s > 1 ? 'w' : s < -1 ? 'l' : '';
      parts.push(`<span class="ec-stk ${kind}" title="Streak gold: ${R.streak.map(([n, g]) => `${g} at ${n}`).join(', ')} wins or losses in a row">${kind ? `${kind === 'w' ? 'Win' : 'Loss'} streak ${Math.abs(s)}` : 'No streak'} <b>+${inc.streak}</b></span>`);
    }
    const html = parts.join('');
    if (el.dataset.h !== html) { el.dataset.h = html; el.innerHTML = html; }
  }
  const econCss = document.createElement('style');
  econCss.textContent = `#xpBar { height: 4px; margin-top: 3px; border-radius: 2px; background: rgba(255,255,255,0.12); overflow: hidden; }
#xpBar i { display: block; height: 100%; width: 0; background: linear-gradient(90deg, #3fd6ff, #7fb8ff); transition: width 0.3s; }
#econLine { grid-column: 1 / 3; display: flex; gap: 8px; flex-wrap: wrap; font-size: 11px; color: var(--muted); line-height: 1.2; }
#econLine b { color: var(--gold); }
#econLine .ec-stk.w b, #econLine .ec-stk.w { color: #6fe08a; }
#econLine .ec-stk.l b, #econLine .ec-stk.l { color: #ff8a7a; }
#xpBar.hidden, #econLine.hidden { display: none; }`;
  document.head.appendChild(econCss);

  function buyXp() {
    if (!game || game.over || !game.human.alive || !game.R.xp) return;
    if (game.human.gold < (game.R.xp.buyCost || 4)) return toast('Not enough gold');
    act('buyXp');
  }

  function announceRound() {
    const p = game.human;
    if (!p.alive) return;
    const opp = game.opponentOf(p);
    const bits = [];
    if (game.round === 1 && game.stage > 1) bits.push(`Stage ${game.stage}: you are now level ${p.level}`);
    if (p.offer) bits.push(`${offerName(p.offer.kind)} pick`);
    if (p.wisp) bits.push('Data Wisp in the shop');
    const who = game.isPvE ? 'Wild Digimon' : opp ? `vs ${esc(opp.player.name)}${opp.ghost ? ' (ghost)' : ''}` : roundName(game.roundType);
    if (DT.Sfx) { DT.Sfx.combat(false); DT.Sfx.play(game.round === 1 && game.stage > 1 ? 'levelup' : 'round'); }
    const b = $('banner');
    b.className = 'round';
    b.innerHTML = `<div>Round ${game.roundLabel} · ${who}</div>${bits.length ? `<div class="small">${bits.join(' · ')}</div>` : ''}`;
    clearResultTimer();
    scheduleResult(() => b.classList.add('hidden'), 1600);
  }

  function showResult(r) {
    const me = game.human;
    const isA = r.match.a === me || r.match.a2 === me;
    const won = isA ? r.aWon : r.bWon;
    const lost = isA ? r.bWon : r.aWon;
    const dmg = isA ? r.dmgA : r.dmgB;
    const oppP = r.match.pve ? null : isA ? r.match.b : r.match.a;
    const opp = oppP ? oppP.name : 'Wild Digimon';
    const title = won ? 'Victory' : lost ? 'Defeat' : 'Draw';
    if (DT.Sfx) DT.Sfx.play(won ? 'win' : lost ? 'loss' : 'round');
    // your Little Legend reacts to the result
    if (DT.Legends && (won || lost)) DT.Legends.play(me.id, won ? 'happy' : 'sad');
    const bits = [];
    if (dmg) bits.push(`<b style="color:var(--bad)">-${dmg} HP</b>`);
    if (won && !r.match.pve) bits.push(`<b style="color:var(--gold)">+${H.winGold} gold</b>`);
    if (r.reward && r.reward.orbs) bits.push(`${r.reward.orbs} loot orb${r.reward.orbs > 1 ? 's' : ''} dropped: right-click to walk your Little Legend over them`);
    const b = $('banner');
    b.className = won ? 'win' : lost ? 'loss' : '';
    b.innerHTML = `<div class="bh">${oppP ? avatarHTML(oppP) : ''}<div><span style="color:${won ? 'var(--good)' : lost ? 'var(--bad)' : 'var(--text)'}">${title}</span> <span style="font-weight:500;color:var(--muted);font-size:18px">vs ${esc(opp)}</span></div></div>
      ${bits.length ? `<div class="small">${bits.join(' · ')}</div>` : ''}`;
  }

  function setPanel(html, title = false) {
    const panel = $('overlay').querySelector('.panel');
    panel.classList.toggle('title', title);
    panel.innerHTML = html;
    return panel;
  }

  function showGameOver() {
    const place = game.human.placement || 1;
    const o = $('overlay');
    const winner = game.players.find((p) => p.placement === 1);
    if (DT.Sfx) { DT.Sfx.music(false); DT.Sfx.play(place === 1 ? 'champion' : 'gameover'); }
    // the client shell (js/client.js) shows the results on its Home page and records the match for the Profile
    if (DT.Client) return DT.Client.gameOver({ game, session, again: () => (session ? leaveNet() : newGame()) });
    setPanel(`
      <h1>${place === 1 ? 'Champion Tamer!' : 'Game Over'}</h1>
      <div class="place">#${place}</div>
      <p class="sub">Reached round ${game.roundLabel} at level ${game.human.level}.${winner && winner !== game.human ? ` Winner: ${esc(winner.name)}.` : ''}</p>
      <button id="againBtn" class="primary big">${session ? 'Back to lobbies' : 'Play again'}</button>`);
    o.classList.remove('hidden');
    $('againBtn').onclick = () => (session ? leaveNet() : newGame());
    if (DT.Shop) DT.Shop.showAward(o.querySelector('.panel'), game, session); // Bits breakdown (js/shop.js)
  }

  // Out of the game while others keep playing: spectate the rest (js/scout.js) or leave.
  // Solo: spectating lets the AI Tamers play on (Game.playOn); leaving ends the game as before.
  function showEliminated() {
    const o = $('overlay');
    const g = game;
    const watch = () => {
      if (game !== g) return;
      o.classList.add('hidden');
      if (!session) {
        if (!game.playOn()) return showGameOver();
        commit({ k: 'next' });
      }
      if (DT.Scout && DT.Scout.follow) DT.Scout.follow();
    };
    const leave = () => leaveSpectate();
    if (DT.Client) return DT.Client.eliminated({ game, session, watch, leave });
    setPanel(`
      <h1>Eliminated</h1>
      <div class="place">#${game.human.placement}</div>
      <p class="sub">The others are still playing. Spectate their fights or leave.</p>
      <button id="watchBtn" class="primary">Spectate</button> <button id="leaveBtn">Leave</button>`);
    o.classList.remove('hidden');
    $('watchBtn').onclick = watch;
    $('leaveBtn').onclick = leave;
    if (DT.Shop) DT.Shop.showAward(o.querySelector('.panel'), game, session); // Bits breakdown (js/shop.js)
  }

  // Leave while spectating after elimination. Solo: the game ends for you here (results as at game over).
  function leaveSpectate() {
    if (!game) return;
    if (session) return leaveNet();
    clearResultTimer();
    game.over = true;
    showGameOver();
  }

  function leaveNet() {
    paused = false;
    const s = session;
    if (s) s.onEntry = null;
    clearResultTimer();
    clearTimeout(toastTimer);
    P.cancelPrefetch();
    DT.KillScreens?.clear();
    closePicker();
    endItemDrag();
    ui.drag = null;
    ui.viewing = null;
    ui.hoverUid = ui.hoverCombatId = ui.hoverTrait = null;
    ui.hoverShop = ui.hoverItem = ui.hoverPlayer = -1;
    ui.tipAnchor = null;
    session = null;
    game = null;
    combat = null;
    drawn = null;
    clock = acc = combatEndAt = 0;
    renderer.mirror = false;
    renderer.resetFx();
    clearCasts();
    if (DT.Sfx) { DT.Sfx.combat(false); DT.Sfx.music(false); }
    document.body.classList.remove('mp');
    for (const id of ['banner', 'modal', 'tooltip', 'sellZone', 'dmgChart', 'duoPanel']) $(id)?.classList.add('hidden');
    if (s) return Promise.resolve().then(() => s.leave()).catch(() => {
      showTitle();
      DT.Client?.toast('Left the match. The lobby connection could not be closed cleanly.', 'info');
    }); // normal multiplayer exits still show the lobby list
    showTitle();
  }

  // Native dialog keeps keyboard focus in the menu and blocks accidental board actions.
  // Solo can explicitly pause; a local menu must never pause a multiplayer host.
  const gameMenu = document.createElement('dialog');
  gameMenu.id = 'gameMenu';
  gameMenu.setAttribute('aria-labelledby', 'gameMenuTitle');
  gameMenu.setAttribute('aria-describedby', 'gameMenuDescription');
  document.body.appendChild(gameMenu);
  let leavingGame = false;
  function renderGameMenu(confirmLeave = false) {
    gameMenu.innerHTML = `<h2 id="gameMenuTitle">${confirmLeave ? 'Leave this match?' : paused ? 'Game paused' : 'Game menu'}</h2>
      <p id="gameMenuDescription">${confirmLeave
        ? (session ? 'You will leave the lobby. Remaining players can continue the match.' : 'This ends your current match. You cannot resume it, and no match result will be saved.')
        : paused ? 'Your solo match is paused. Resume whenever you are ready.' : 'The match continues while this menu is open.'}</p>
      <div class="game-menu-actions"><button type="button" data-game-return autofocus>${paused ? 'Resume game' : confirmLeave ? 'Keep playing' : 'Return to game'}</button>
      ${!session && !paused && !confirmLeave ? '<button type="button" data-game-pause>Pause game</button>' : ''}
      <button type="button" class="game-menu-leave" ${confirmLeave ? 'data-game-confirm' : 'data-game-leave'}>Leave match${confirmLeave ? '' : '…'}</button></div>`;
  }
  function openGameMenu() {
    if (!game || !$('overlay').classList.contains('hidden') || document.body.classList.contains('client-on') || leavingGame) return;
    endItemDrag();
    ui.drag = null;
    $('sellZone').classList.add('hidden');
    $('tooltip').classList.add('hidden');
    renderGameMenu();
    gameMenu.showModal();
  }
  $('gameMenuBtn').onclick = openGameMenu;
  gameMenu.addEventListener('cancel', e => { if (leavingGame) e.preventDefault(); });
  gameMenu.addEventListener('close', resumeSolo);
  gameMenu.addEventListener('click', async e => {
    if (leavingGame) return;
    if (e.target.closest('[data-game-pause]')) {
      pauseSolo();
      renderGameMenu();
      gameMenu.querySelector('[data-game-return]').focus();
      return;
    }
    if (e.target.closest('[data-game-return]')) return gameMenu.close();
    if (e.target.closest('[data-game-leave]')) {
      renderGameMenu(true);
      gameMenu.querySelector('[data-game-return]').focus();
      return;
    }
    if (!e.target.closest('[data-game-confirm]')) return;
    leavingGame = true;
    gameMenu.querySelectorAll('button').forEach(b => { b.disabled = true; });
    $('gameMenuDescription').textContent = 'Leaving match…';
    try {
      await leaveNet();
      DT.Client?.lobbyBack();
      showTitle();
    } finally { leavingGame = false; gameMenu.close(); }
  });
  window.addEventListener('keydown', e => {
    if (gameMenu.open) {
      e.stopImmediatePropagation(); // native Tab/Enter still work, game hotkeys do not
      if (e.key === 'Escape') { e.preventDefault(); if (!leavingGame) gameMenu.close(); }
      return;
    }
    if (e.key !== 'Escape' || e.repeat || e.defaultPrevented || !game) return;
    // Let existing popovers, scouting and cinematic controls consume Escape first.
    if (document.querySelector('dialog[open], #picker:not(.hidden), #emoteWheel.open') || ui.viewing != null
      || DT.KillScreens?.active || DT.SoundUI?.isOpen() || DT.Shop?.isOpen() || DT.Codex?.fullOpen() || DT.LegendEvo?.isOpen()) return;
    if (!$('overlay').classList.contains('hidden') || document.body.classList.contains('client-on')) return;
    e.preventDefault(); e.stopImmediatePropagation(); openGameMenu();
  }, true);

  // ---------- actions ----------
  let toastTimer = null;
  function toast(msg, kind) {
    const t = $('toast');
    t.textContent = msg;
    t.style.color = kind === 'info' ? 'var(--gold)' : 'var(--bad)';
    t.style.opacity = 1;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => { t.style.opacity = 0; }, 1800);
  }

  // Checks that need no state change run here first so the message is instant; the action itself goes through act().
  // Board and shop inputs act on your own board, so they only work while you are looking at it (js/scout.js).
  function ownView() {
    if (ui.viewing == null) return true;
    toast('Return to your board first (Space)');
    return false;
  }

  function buy(slot) {
    if (!ownView()) return;
    const p = game.human;
    const why = game.canBuy(p, slot);
    if (why === 'gold') return toast('Not enough gold');
    if (why === 'bench') return toast('Bench is full');
    if (why !== 'ok') return;
    act('buy', { slot });
  }

  function buyWisp() {
    if (!ownView()) return;
    const p = game.human;
    if (game.phase !== 'planning') return toast('Wisps can only be bought during planning');
    if (p.wisp && p.gold < p.wisp.cost) return toast('Not enough gold');
    if (p.wisp) { ui.lastWisp = p.wisp.id; act('wisp'); }
  }

  function reroll() {
    if (!game || game.over || !game.human.alive) return;
    const p = game.human;
    if (p.freeRolls <= 0 && p.gold < H.rerollCost) return toast('Not enough gold');
    act('reroll');
  }

  function sellUid(uid) {
    if (!ownView()) return;
    const f = game.findUnit(game.human, uid);
    if (f && !DT.canSell(f.u)) return toast("That Digimon can't be sold");
    ui.hoverUid = null;
    act('sell', { uid });
  }

  function equip(idx, uid) {
    if (!ownView()) return;
    ui.lastEquip = game.human.items[idx] || null;
    act('equip', { idx, uid });
  }

  // Messages for this player's own actions, once the action has been applied.
  function feedback(a, g, res) {
    sound(a, res);
    if (a === 'portal') toast(res && res.kind ? 'Sent to your partner' : ({ limit: 'No portal sends left this round', full: 'Your partner’s bench is full', itemsFull: 'Your partner’s item bench is full', bench: 'Send a Digimon from your bench', phase: 'The portal opens during planning', noSend: 'This unit cannot be sent' }[res] || 'Cannot send that right now'), res && res.kind ? 'info' : undefined);
    if (a === 'buy' && Array.isArray(res)) for (const u of res) toast(`Digivolved into ${DT.LINE[u.line].forms[u.stage]}!`, 'info');
    else if (a === 'sell' && !res) { const f = game.findUnit(game.human, g.uid); toast(f && !DT.canSell(f.u) ? "That Digimon can't be sold" : 'Cannot sell board units during combat'); }
    else if (a === 'reroll' && !res) toast('Not enough gold');
    else if (a === 'move') {
      if (res === 'cap') toast(`Board is full (${game.human.cap()}). ${game.R.xp ? 'Buy XP to level up.' : 'You level up at the start of each stage.'}`);
      else if (res === false && game.phase === 'combat') toast('The board is locked during combat');
    } else if (a === 'wisp') {
      if (res === 'gold') toast('Not enough gold');
      else if (res && DT.WISP[res]) toast(`${DT.WISP[res].name} used`, 'info');
    } else if (a === 'equip') {
      const used = ui.lastEquip && DT.ITEMS[ui.lastEquip];
      ui.lastEquip = null;
      const msg = (used && used.errors && used.errors[res]) || EQUIP_MSG[res];
      if (res === 'none') { /* nothing happened */ }
      else if (msg) toast(msg);
      else if (used && used.consumable && typeof res === 'string') toast(`${used.name} used`, 'info');
      else if (res && DT.ITEMS[res] && !DT.ITEMS[res].component && used && used.component) toast(`Made ${itemName(res)}`, 'info');
    } else if (a === 'offer' && res) {
      toast(DT.AUGMENT[res] ? `Augment: ${DT.AUGMENT[res].name}` : DT.POWER[res] ? `Tamer power: ${DT.POWER[res].name}` : DT.ITEMS[res] ? `Got ${itemName(res)}` : 'Picked', 'info');
    } else if (a === 'orb' && res) {
      const bits = [];
      if (res.gold) bits.push(`+${res.gold}g`);
      for (const id of res.items) bits.push(itemName(id));
      if (res.unit) bits.push(DT.LINE[res.unit].forms[0]);
      toast(bits.join(', '), 'info');
    }
  }

  // equip / consumable results that mean nothing happened (a consumable's own item.errors can add more)
  const EQUIP_MSG = { locked: 'The board is locked during combat', full: "That Digimon can't hold another item", hasTrait: 'That Digimon already has this trait',
    noItems: 'That Digimon has no items', benchFull: 'Not enough room on the item bench', cantHold: "That Digimon can't hold items",
    maxed: 'That Digimon is already in its last form', poolEmpty: 'No copies left in the pool', phase: 'Only during planning', busy: 'Make your pick first' };

  // sound for this client's own actions
  function sound(a, res) {
    const S = DT.Sfx;
    if (!S) return;
    const ok = res !== null && res !== false && typeof res !== 'string';
    if (a === 'buy') S.play(res == null ? 'error' : res.length ? 'digivolve' : 'buy');
    else if (a === 'sell') S.play(res ? 'sell' : 'error');
    else if (a === 'reroll') S.play(res ? 'reroll' : 'error');
    else if (a === 'move') S.play(res === true ? 'place' : res === 'cap' ? 'error' : null);
    else if (a === 'equip') S.play(DT.ITEMS[res] ? (DT.ITEMS[res].component ? 'equip' : 'forge') : typeof res === 'string' && res !== 'none' && !EQUIP_MSG[res] ? 'forge' : 'error');
    else if (a === 'wisp') S.play(res && DT.WISP[res] ? 'pick' : 'error');
    else if (a === 'offer' || a === 'carousel' || a === 'offerReroll') S.play(ok ? 'pick' : null);
    else if (a === 'orb' && res) S.play('orb');
    else if (a === 'lock') S.play('place');
    else if (a === 'swapItems' && res) S.play('place');
  }

  // ---------- panels ----------
  function refresh() {
    if (!game) return;
    const p = game.human;
    const opp = game.opponentOf(p);
    renderDuos(p);
    renderTop(p, opp);
    ui.previewSide = game.phase === 'planning' && game.roundType !== 'carousel' ? (game.isPvE ? game.pveSide() : opp ? { units: opp.player.boardSide() } : null) : null;
    renderEcon(p);
    renderShop(p);
    renderTraits(scouted() || p);
    renderAugs(p);
    renderPlayers(p, opp);
    renderHistory(p);
    renderModal();
    renderTooltip();
    if (DT.Scout) DT.Scout.render();
  }
  const scouted = () => (game && ui.viewing != null ? game.players[ui.viewing] || null : null);

  function renderDuos(p) {
    let panel = $('duoPanel');
    if (!panel) {
      panel = document.createElement('div'); panel.id = 'duoPanel'; $('arena').appendChild(panel);
      panel.innerHTML = '<button id="duoPartner"></button><button id="duoPortal" title="Drag a bench Digimon or item here to send it to your partner"><span class="portal-ring">◎</span><b>Partner Portal</b><small></small></button><span id="duoIncoming"></span>';
      $('duoPartner').onclick = () => { const q = game.partnerOf(game.human); if (q && DT.Scout) DT.Scout.view(ui.viewing === q.id ? game.human.id : q.id); };
      $('duoPortal').onclick = () => toast('Drag a bench Digimon or item onto the portal to send it', 'info');
    }
    panel.classList.toggle('hidden', !game.duo);
    if (!game.duo) return;
    const q = game.partnerOf(p);
    $('duoPartner').innerHTML = `<b>${esc(q.name)}</b> · Lv ${q.level} · ${q.gold}g<br><small>Team ${p.team + 1} · ${Math.max(0,p.hp)} shared HP · View partner</small>`;
    const open = p.alive && game.phase === 'planning' && game.roundType !== 'carousel' && p.portal < game.portalLimit(p);
    $('duoPortal').classList.toggle('closed', !open);
    $('duoPortal').querySelector('small').textContent = open ? `${game.portalLimit(p) - p.portal} sends left · drag here` : 'Opens next planning round';
    const arrived = (p.incoming || []).map(e => e.kind === 'item' ? itemName(e.item) : DT.LINE[e.line]?.forms[e.stage] || 'Digimon');
    const txt = arrived.length ? `Received: ${arrived.join(', ')}` : '';
    if ($('duoIncoming').textContent !== txt) {
      $('duoIncoming').textContent = txt;
      if (txt) { panel.classList.remove('arrived'); void panel.offsetWidth; panel.classList.add('arrived'); if (DT.Sfx) DT.Sfx.play('orb'); }
    }
  }

  function renderTop(p, opp) {
    $('stageLabel').textContent = game.roundLabel;
    const n = H.roundsInStage(game.stage);
    const mine = hist.get(p.id) || [];
    const track = [];
    for (let r = 1; r <= n; r++) {
      const t = game.typeOf(game.stage, r);
      const label = `${game.stage}-${r}`;
      const h = mine.find((x) => x.round === label);
      const ok = game.offerKindAt(game.stage, r);
      const aug = ok === 'augment' || ok === 'power';
      const arm = !!ok && !aug;
      const tip = `${label} ${roundName(t)}${ok ? ` · ${offerName(ok)}` : ''}${h ? ` · ${{ w: 'Won', l: 'Lost', d: 'Draw' }[h.res]}` : ''}`;
      const cls = ['rt', r === game.round ? 'cur' : '', h && r !== game.round ? h.res : ''].join(' ');
      track.push(`<div class="${cls}" title="${tip}">${{ carousel: 'C', pve: 'PvE', pvp: 'PvP' }[t]}${aug ? `<i class="ex" style="background:${DT.AUG_TIER_COLOR[3]}"></i>` : arm ? '<i class="ex" style="background:#4fb3ff"></i>' : ''}</div>`);
    }
    $('stageTrack').innerHTML = track.join('');
    $('phaseLabel').textContent = { planning: 'Planning', combat: 'Combat', result: 'Result' }[game.phase];
    $('phaseSub').textContent = roundName(game.roundType);
    const ol = $('opponentLabel');
    ol.className = '';
    if (game.isPvE) { ol.className = 'pve'; ol.innerHTML = `<div><div class="ol">Opponent</div><div class="on">Wild Digimon</div></div>`; }
    else if (opp) {
      ol.className = 'pvp';
      const q = opp.player;
      ol.innerHTML = `${avatarHTML(q)}<div><div class="ol">${game.phase === 'planning' ? 'Next opponent' : 'Fighting'}${opp.ghost ? ' (ghost copy)' : ''}</div>
        <div class="on">${esc(q.name)}${game.duo ? ` + ${esc(game.partnerOf(q).name)}` : ''}<small>${Math.max(0, q.hp)} HP</small></div></div>`;
    } else ol.innerHTML = '';
    $('readyBtn').disabled = game.phase !== 'planning' || game.roundType === 'carousel' || !p.alive;
    if (session) {
      const humans = game.players.filter((q) => q.alive && q.isHuman);
      const voted = humans.filter((q) => game.votes.includes(q.id)).length;
      $('readyBtn').textContent = game.votes.includes(p.id) ? `Ready ${voted}/${humans.length}` : `Ready (${voted}/${humans.length})`;
    } else $('readyBtn').textContent = 'Fight!';
    $('skipBtn').disabled = game.phase !== 'combat';
  }

  function renderEcon(p) {
    $('levelLabel').textContent = `Level ${p.level}`;
    const X = game.R.xp;
    const nx = game.nextLevelAt();
    const need = X ? DT.XP_NEXT[p.level] : 0;
    $('xpLabel').textContent = X ? (p.level >= (X.maxLevel || 9) ? 'Max level' : `${p.xp}/${need} XP`) : nx ? `Lv ${nx.level} at stage ${nx.stage}` : 'Max level';
    xpBar(p, X, need);
    xpButton(p);
    const bc = p.boardCount(), cap = p.cap();
    $('boardCount').textContent = `${bc}/${cap}`;
    $('boardBox').classList.toggle('under', bc < cap && p.benchCount() > 0 && game.phase === 'planning');
    $('boardBox').title = bc < cap ? `You can field ${cap - bc} more Digimon` : 'Board is full';
    $('gold').textContent = p.gold;
    const inc = game.incomeBreakdown(p);
    $('income').textContent = `+${inc.total} next round`;
    $('income').title = [`Income: ${inc.base} per round`, inc.interest ? `${inc.interest} interest` : '', inc.streak ? `${inc.streak} streak` : '',
      inc.augment ? `${inc.augment} from augments` : '', inc.bonus ? `${inc.bonus} bonus` : ''].filter(Boolean).join(' + ') +
      `.${game.R.interest ? '' : ' No interest.'} Wins pay ${H.winGold}.`;
    econLine(p, inc);
    $('rollBtn').innerHTML = p.freeRolls > 0 ? `Reroll <span class="cost">free x${p.freeRolls}</span>` : `Reroll <span class="cost">${coin} ${H.rerollCost}</span>`;
    $('rollBtn').disabled = p.freeRolls <= 0 && p.gold < H.rerollCost;
    $('lockBtn').textContent = p.locked ? 'Shop locked' : 'Lock shop';
    $('lockBtn').classList.toggle('on', p.locked);
    const full = (a, b) => `<b class="${a >= b ? 'full' : ''}">${a}/${b}</b>`;
    $('capLabel').innerHTML = `Bench ${full(p.benchCount(), DT.BENCH)} · Items ${full(p.items.length, H.itemCap)}`;
  }

  // the unit a shop slot sells: the line's base form, plus what a mechanic put in shopMeta[slot].unit (form, traits, mods)
  function shopUnit(p, i) {
    const id = p.shop[i];
    const meta = p.shopMeta && p.shopMeta[i];
    const mu = (meta && meta.unit) || {};
    return { line: id, stage: mu.stage || 0, items: [], bonusTraits: mu.bonusTraits || null, mods: mu.mods || null, meta: meta || null };
  }

  function renderShop(p) {
    const owned = {};
    for (const u of p.allUnits()) owned[`${u.line}|${u.stage}`] = (owned[`${u.line}|${u.stage}`] || 0) + 1;
    const maxStage = game.R && game.R.maxStage != null ? game.R.maxStage : 2;
    const cards = p.shop.map((id, i) => {
      if (!id) return `<div class="card empty"></div>`;
      const L = DT.LINE[id];
      const su = shopUnit(p, i);
      const cls = ['card'];
      const cost = game.shopCost(p, i);
      const need = game.copiesOf(id);
      if (p.gold < cost) cls.push('poor');
      if (su.meta && su.meta.tag) cls.push('meta');
      const own = su.stage < maxStage ? owned[`${id}|${su.stage}`] || 0 : 0;
      if (own && own >= need - 1) cls.push('evo');
      // traits, with the times a mechanic's bonusTraits make one count (Fates Partner: x2)
      const bonus = su.bonusTraits || {};
      const times = (t) => (L.traits.includes(t) ? 1 : 0) + (bonus[t] || 0);
      const tlist = L.traits.concat(Object.keys(bonus).filter((t) => !L.traits.includes(t)));
      const traits = tlist.filter((t) => DT.TRAITS[t]).map((t) => `<span${bonus[t] ? ' class="bt"' : ''}><i style="background:${DT.TRAITS[t].color}"></i>${DT.TRAITS[t].name}${bonus[t] && times(t) > 1 ? ` <b title="Counts ${times(t)} times">x${times(t)}</b>` : ''}</span>`).join('');
      const stars = su.stage ? ` <small style="color:${DT.STAGE_COLORS[su.stage]}">${'★'.repeat(su.stage + 1)}</small>` : '';
      return `<div class="${cls.join(' ')}" data-slot="${i}" style="--tier:${DT.TIER_COLORS[L.tier]}">
        <div class="art">${P.html(id, su.stage)}</div>
        <div class="ct">${traits}</div>
        ${su.meta && su.meta.tag ? `<div class="mtag">${esc(su.meta.tag)}</div>` : ''}
        <div class="attr" style="color:${DT.ATTR[L.attr].color}">${L.attr}</div>
        ${own ? `<div class="own" title="${own >= need - 1 ? 'Buying this one digivolves it' : 'Copies you own'}">${own >= need - 1 ? 'Digivolve' : `${own}/${need}`}</div>` : ''}
        <div class="nb"><div class="cn">${L.forms[su.stage]}${stars}</div><div class="price">${coin}${cost}</div></div>
      </div>`;
    });
    if (p.wisp) {
      const w = DT.WISP[p.wisp.id];
      cards.push(`<div class="card wisp ${p.gold < p.wisp.cost ? 'poor' : ''}" data-wisp="1">
        ${DT.ItemArt.img(`wisp:${w.id}`, 30, 'wispart')}<div class="kind">${w.kind} Wisp</div><div class="cn">${w.name}</div><div class="wd">${esc(w.desc)}</div>
        <div class="price">${coin}${p.wisp.cost}</div></div>`);
    } else if (DT.WISPS.length) cards.push(`<div class="card empty" title="A Data Wisp appears here every other round from stage 2"></div>`);
    $('shop').innerHTML = cards.join('');
  }

  // Trait rows: hex badge in bronze/silver/gold by breakpoint reached, breakpoints, and the Digimon that count.
  function traitRows(p) {
    const units = p.boardUnits();
    const counts = DT.countTraits(units, p.augs);
    return Object.keys(counts).map((id) => {
      const byLine = new Map();
      for (const u of units) if (DT.LINE[u.line].shop && DT.unitTraits(u).includes(id) && (!byLine.has(u.line) || byLine.get(u.line).stage < u.stage)) byLine.set(u.line, u);
      return { id, n: counts[id], lv: DT.traitLevel(id, counts[id]), units: [...byLine.values()] };
    }).sort((a, b) => b.lv - a.lv || b.n - a.n || a.id.localeCompare(b.id));
  }
  const traitTier = (T, lv) => (!lv ? 'l0' : lv >= T.bp.length ? 'gold' : lv >= 2 ? 'silver' : 'bronze');

  function renderTraits(p) {
    let rows = traitRows(p);
    if (game.duo && game.roundType === 'pvp') {
      const team = game.teamOf(p), side = game.teamSide(team), counts = side.counts;
      rows = Object.keys(counts).filter(id => DT.TRAITS[id]).map(id => ({id,n:counts[id],lv:DT.traitLevel(id,counts[id]),units:side.units.filter(u=>DT.LINE[u.line].traits.includes(id))})).sort((a,b)=>b.lv-a.lv || b.n-a.n);
    }
    const active = rows.filter((r) => r.lv).length;
    const head = game.duo && game.roundType === 'pvp' ? 'Combined team traits' : p === game.human ? 'Traits' : `${esc(p.name)}'s traits`;
    $('traitList').innerHTML = `<div class="ph${p === game.human ? '' : ' scout'}">${head}<span>${active} active</span></div>` + (rows.length ? rows.map(({ id, n, lv, units }) => {
      const T = DT.TRAITS[id];
      const next = T.bp.find((b) => b > n);
      const bps = T.bp.map((b, i) => `<b class="${i < lv ? 'on' : ''}">${b}</b>`).join('');
      return `<div class="trait ${traitTier(T, lv)}" data-trait="${id}" style="--tc:${T.color}">
        <div class="tb"><span>${n}</span></div>
        <div class="tn"><span>${T.name}</span><em>${next ? `${n}/${next}` : n}</em></div>
        <div class="tbp">${bps}${lv ? `<span class="fx">${bpText(T, lv - 1)}</span>` : ''}</div>
        <div class="tu">${units.map((u) => P.html(u.line, u.stage, `t${DT.LINE[u.line].tier}`)).join('')}</div>
      </div>`;
    }).join('') : '<div class="emptyhint">Place Digimon on the board to activate traits.</div>');
  }

  function renderAugs(p) {
    $('augs').innerHTML = p.augs.length ? `<div class="ph">Augments<span>${p.augs.length}/3</span></div>` + p.augs.map((id) => {
      const a = DT.AUGMENT[id];
      const c = DT.AUG_TIER_COLOR[a.tier];
      return `<div class="aug" style="border-color:${c}55">${DT.ItemArt.img(`aug:${id}`, 30, 'augart')}<div><b style="color:${c}">${a.name}</b><div class="ad">${esc(a.desc)}</div></div></div>`;
    }).join('') : '';
  }

  function renderPlayers(p, opp) {
    const list = game.players.slice().sort((a, b) => (b.alive - a.alive) || (a.alive ? b.hp - a.hp : a.placement - b.placement) || (game.duo ? a.team - b.team : 0) || a.id-b.id);
    const lastDmg = {};
    if (game.phase !== 'combat') for (const r of game.results || []) { lastDmg[r.match.a.id] = r.dmgA; if (r.match.b && !r.match.ghost) lastDmg[r.match.b.id] = r.dmgB; }
    const alive = game.players.filter((q) => q.alive).length;
    $('players').innerHTML = `<div class="ph">${game.duo ? 'Teams' : 'Tamers'}${session ? '<button id="mpPlayerControls" title="Mute or remove players">Players</button>' : ''}<span>${game.duo ? game.aliveTeams().length : alive} left</span></div>` + list.map((q) => {
      const cls = ['pl'];
      if (q === p) cls.push('me');
      const isOpp = opp && opp.player === q;
      if (isOpp) cls.push('opp');
      if (!q.alive) cls.push('dead');
      const tags = [];
      if (game.duo) tags.push(`<span class="tag">T${q.team + 1}${game.partnerOf(p) === q ? ' · PARTNER' : ''}</span>`);
      if (q === p) tags.push('<span class="tag you">YOU</span>');
      else if (session) tags.push(q.isHuman ? '<span class="tag p" title="Player">PLAYER</span>' : '<span class="tag ai" title="Played by the AI">AI</span>');
      if (isOpp) tags.push(`<span class="tag vs">${game.phase === 'planning' ? 'NEXT' : 'VS'}</span>`);
      if (session && game.votes.includes(q.id) && game.phase === 'planning') tags.push('<span class="tag rd">READY</span>');
      const streak = q.streak > 1 ? `<span class="stk w" title="Win streak">W${q.streak}</span>` : q.streak < -1 ? `<span class="stk l" title="Loss streak">L${-q.streak}</span>` : '';
      const hp = Math.max(0, q.hp);
      const pct = Math.min(100, (100 * hp) / game.R.startHp);
      const pips = (hist.get(q.id) || []).filter((h) => !h.pve).slice(-5).map((h) => `<i class="${h.res}" title="${h.round}: ${{ w: 'won', l: 'lost', d: 'draw' }[h.res]} vs ${esc(h.opp)}"></i>`).join('');
      const d = lastDmg[q.id];
      return `<div class="${cls.join(' ')}" data-pid="${q.id}">
        ${avatarHTML(q)}
        <div class="nm"><span class="n">${esc(q.name)}</span>${tags.join('')}</div>
        <div class="hpv">${q.alive ? hp : `#${q.placement}`}${q.alive && d ? `<small>-${d}</small>` : ''}</div>
        <div class="pm">${q.alive ? `<div class="hpbar"><div class="${pct <= 25 ? 'low' : pct <= 50 ? 'mid' : ''}" style="width:${pct}%"></div></div>` : '<span>out</span>'}<span>Lv ${q.level}</span>${streak}${pips ? `<span class="pips">${pips}</span>` : ''}</div>
      </div>`;
    }).join('');
  }

  function renderHistory(p) {
    const h = hist.get(p.id) || [];
    if (!h.length) { $('history').innerHTML = ''; return; }
    const pvp = h.filter((x) => !x.pve);
    const w = pvp.filter((x) => x.res === 'w').length, l = pvp.filter((x) => x.res === 'l').length;
    $('history').innerHTML = `<div class="ph">Your rounds<span>PvP ${w}-${l}</span></div><div class="hrow">` + h.slice(-14).map((x) =>
      `<div class="hc ${x.res} ${x.pve ? 'pve' : ''}" title="${x.round} ${x.pve ? 'Wild Digimon' : `vs ${esc(x.opp)}`}: ${{ w: 'won', l: 'lost', d: 'draw' }[x.res]}${x.dmg ? `, -${x.dmg} HP` : ''}">${x.round}<span>${x.res.toUpperCase()}${x.dmg ? ` -${x.dmg}` : ''}</span></div>`).join('') + '</div>';
  }

  // Carousel, Armory and augment pickers share one modal.
  // The set mechanic's game-start announcement (mechanic.intro, e.g. this game's Galaxy): shown in stage 1 until
  // dismissed, above the opening carousel or on its own.
  function introHtml() {
    if (!game || game.stage > 1) return '';
    if (!ui.intro || ui.intro.game !== game) ui.intro = { game, data: DT.mechanicIntro(game), seen: false };
    const d = ui.intro.data;
    if (!d || ui.intro.seen) return '';
    return `<div class="mintro" style="--ic:${esc(d.color || 'var(--gold)')}">${d.img || ''}<div class="mi-b">${d.title ? `<div class="mi-t">${esc(d.title)}</div>` : ''}<h3>${esc(d.name || '')}</h3>
      <div class="mi-d">${esc(d.desc || '')}</div></div><button id="introOk">Got it</button></div>`;
  }

  function renderModal() {
    const m = $('modal');
    const p = game && game.human;
    let html = '';
    const intro = introHtml();
    if (game && game.roundType === 'carousel' && game.carousel) {
      const c = game.carousel;
      const myTurn = game.carouselTurn() === p;
      const choices = c.opts.map((o, i) => {
        const L = DT.LINE[o.line];
        return `<div class="choice small ${o.by ? 'taken' : ''}" data-carousel="${i}" style="border-color:${DT.TIER_COLORS[L.tier]}">
          <div class="cimg">${P.html(o.line, 0)}${itemImg(o.item)}</div>
          <h4>${L.forms[0]}</h4>
          <div class="d">${L.tier}-cost · ${L.traits.map((t) => DT.TRAITS[t].name).join(', ')}</div>
          <div class="d" style="color:var(--text)">${itemName(o.item)}</div>
          ${o.by ? `<div class="d" style="color:var(--gold)">Taken by ${esc(o.by)}</div>` : ''}</div>`;
      }).join('');
      html = `<div class="mpanel">${intro}<div class="mhead"><h2>${game.stage === 1 ? 'Opening Carousel' : 'Carousel'}</h2></div>
        <div class="msub">${myTurn ? '<b style="color:var(--gold)">Your pick:</b> one Digimon and its item.' : game.carouselDone() ? 'Everyone has picked.' : `Waiting for ${esc(game.carouselTurn().name)}...`} ${game.stage === 1 ? 'Pick order is random.' : 'Lowest HP picks first.'}</div>
        <div class="choices">${choices}</div>
        ${game.carouselDone() ? (session ? '<div class="mfoot">The next round is starting...</div>' : `<div class="mfoot"><button id="carouselGo" class="primary">Start round ${game.stage}-${game.round + 1}</button></div>`) : ''}</div>`;
    } else if (p && p.offer && game.phase === 'planning') {
      const o = p.offer;
      const aug = o.kind === 'augment' || o.kind === 'power';
      const custom = !aug && o.kind !== 'armory' && o.kind !== 'itemTier';
      const defOf = (id) => (o.kind === 'power' ? DT.POWER[id] : DT.AUGMENT[id]) || { name: id, desc: '', tier: 1 };
      const tier = aug ? defOf(o.options[0]).tier || 1 : 0;
      const choices = o.options.map((id, i) => {
        if (aug) {
          const a = defOf(id);
          const c = DT.AUG_TIER_COLOR[a.tier || 1];
          return `<div class="choice aug" data-offer="${i}" style="border-color:${c}">${DT.ItemArt.img(`aug:${id}`, 64, 'augart')}<h4 style="color:${c}">${esc(a.name)}</h4><div class="d">${esc(a.desc)}</div></div>`;
        }
        if (custom) {
          const K = game.set.mechanic.offerKinds[o.kind] || {};
          const card = K.card ? K.card(id) : { name: String(id) };
          return `<div class="choice" data-offer="${i}">${card.img || ''}<h4>${esc(card.name)}</h4><div class="d">${esc(card.desc || '')}</div></div>`;
        }
        const it = DT.ITEMS[id];
        return `<div class="choice" data-offer="${i}">${itemImg(id, 'class="bigicon"').replace(/class="(c|f)" /, '')}<h4>${it.name}</h4><div class="d">${esc(DT.itemText(id))}</div>
          ${it.from && !it.component ? `<div class="d">From ${it.from.map(itemName).join(' + ')}</div>` : ''}</div>`;
      }).join('');
      const title = o.kind === 'augment' ? `${DT.AUG_TIER_NAME[tier]} Augment` : offerName(o.kind);
      const sub = o.kind === 'augment' ? 'Pick one augment. It lasts the whole game.' : o.kind === 'power' ? 'Pick your Tamer power. It ranks up later in the game.'
        : custom ? ((game.set.mechanic.offerKinds[o.kind] || {}).sub || 'Pick one.') : 'Pick one item. It goes to your item bench.';
      html = `<div class="mpanel">${intro}<div class="mhead"><h2 style="${aug ? `color:${DT.AUG_TIER_COLOR[tier]}` : ''}">${esc(title)}</h2>
          <button id="peekBtn">${ui.peek ? 'Show choices' : 'View board'}</button></div>
        <div class="msub">${sub}</div>
        <div class="choices">${choices}</div>
        ${o.rerolls > 0 ? `<div class="mfoot"><button id="offerReroll">Reroll choices (${o.rerolls})</button></div>` : ''}</div>`;
    } else if (intro && game.phase === 'planning') html = `<div class="mpanel">${intro}</div>`;
    m.innerHTML = html;
    m.classList.toggle('hidden', !html);
    m.classList.toggle('peek', !!html && ui.peek);
  }

  function renderChart(force) {
    const el = $('dmgChart');
    const cb = shownCombat();
    if (!cb) { el.classList.add('hidden'); return; }
    const now = performance.now();
    if (!force && now - chartAt < 300) return;
    chartAt = now;
    const key = ui.chartMode;
    const team = drawn ? drawnTeam : myTeam;
    const mine = cb.units.filter((u) => u.team === team).sort((a, b) => b[key] - a[key]);
    const max = Math.max(1, ...mine.map((u) => u[key]));
    const color = { dealt: '#ffb35a', taken: '#8a97b4', healed: '#5adf8a' }[key];
    const tabs = [['dealt', 'Dealt'], ['taken', 'Taken'], ['healed', 'Healed']].map(([k, n]) => `<button data-chart="${k}" class="${k === key ? 'on' : ''}">${n}</button>`).join('');
    el.innerHTML = `<div class="ph">Damage<span>this fight</span></div><div class="tabs">${tabs}</div>` + mine.map((u) =>
      `<div class="row ${u.alive ? '' : 'dead'}">${P.html(u.line.id, u.stage)}
        <div><div class="n">${u.name}</div><div class="bar"><i style="width:${(100 * u[key]) / max}%;background:${color}"></i></div></div>
        <div class="v">${Math.round(u[key])}</div></div>`).join('');
    el.classList.remove('hidden');
  }

  function statsTable(s, live) {
    const hp = live ? `${Math.round(s.hp)}/${Math.round(s.maxHp)}` : s.maxHp;
    const mana = live ? `${Math.floor(s.mana)}/${s.maxMana}` : `${s.mana}/${s.maxMana}`;
    return `<table>
        <tr><td>HP</td><td>${hp}</td><td>Mana</td><td>${mana}</td></tr>
        <tr><td>AD</td><td>${Math.round(s.ad)}</td><td>AS</td><td>${s.as.toFixed(2)}</td></tr>
        <tr><td>Armor</td><td>${Math.round(s.armor)}</td><td>MR</td><td>${Math.round(s.mr)}</td></tr>
        <tr><td>Range</td><td>${s.range}</td><td>${live ? 'AP' : ''}</td><td>${live ? Math.round(s.ap) : ''}</td></tr>
      </table>`;
  }

  function itemsBlock(items) {
    return items.map((id) => `<div class="itm">${itemImg(id)}<div><b>${itemName(id)}</b><br>${esc(DT.itemText(id))}</div></div>`).join('');
  }

  // the set's notes for a unit (def.unitNote): digivolution path, form, Partner, Anomaly...
  const unitNotes = (u) => (u ? DT.unitNote(u).map((t) => `<div class="foot unote">${esc(t)}</div>`).join('') : '');

  function tooltipFor(line, stage, live, items = [], unit = null) {
    const s = live || DT.unitStats(line, stage);
    let traits = live ? live.traits : DT.unitTraits({ line: line.id, items });
    const bt = !live && unit && unit.bonusTraits ? Object.keys(unit.bonusTraits).filter((t) => DT.TRAITS[t] && !traits.includes(t)) : [];
    if (bt.length) traits = traits.concat(bt);
    const tags = traits.map((t) => `<span class="tag" style="color:${DT.TRAITS[t].color};border-color:${DT.TRAITS[t].color}55">${DT.TRAITS[t].name}</span>`).join('') +
      `<span class="tag" style="color:${DT.ATTR[line.attr].color};border-color:${DT.ATTR[line.attr].color}55">${line.attr}</span>`;
    const evo = line.forms.map((f, i) => (i === stage ? `<b>${f}</b>` : f)).join(' → ');
    return `<div class="th" style="--tier:${DT.TIER_COLORS[line.tier]}">${P.html(line.id, stage)}<div><h3>${line.forms[stage]}</h3>
      <div class="line">${DT.stageName(line, stage)} · ${line.tier}-cost · <span style="color:${DT.STAGE_COLORS[stage]}">${'★'.repeat(stage + 1)}</span></div></div></div>
      <div class="line">${evo}</div>
      <div class="tags">${tags}</div>
      ${statsTable(s, live)}
      <div class="ab"><b>${line.abil[stage]}</b>: ${DT.abilityText(line, stage, live ? live.ap : 100)}</div>
      ${unitNotes(live || unit)}
      ${itemsBlock(live ? live.items : items)}`;
  }

  // What dropping item id on unit u will do, for the drag tooltip.
  function dropPreview(id, u) {
    const it = DT.ITEMS[id];
    if (it.consumable) return `${it.name}: ${DT.consumablePreview(id, u, game.human, game).text}`;
    if (it.emblem && DT.unitTraits(u).includes(it.emblem)) return 'Already has this trait';
    if (it.component) {
      const c = u.items.find((x) => DT.isComponent(x) && DT.combineItems(x, id));
      if (c) return `Combines into ${itemName(DT.combineItems(c, id))}`;
    }
    if (!DT.holdsItems(u)) return "Can't hold items";
    return u.items.length >= 3 || game.equipVeto(game.human, u, id, null) ? 'Item slots full' : `Equip ${it.name}`;
  }

  function itemTip(id) {
    const it = DT.ITEMS[id];
    let html = `<div class="itip">${DT.ItemArt.img(id, 64)}<div><h3>${itemName(id)}</h3><div class="line">${it.component ? 'Component' : it.consumable ? 'Consumable' : it.emblem ? 'Crest' : 'Completed item'}</div></div></div><div class="ab">${esc(DT.itemText(id))}</div>`;
    if (it.component) {
      const combos = DT.COMPONENT_IDS.map((c) => [c, DT.combineItems(id, c)]).filter((x) => x[1]);
      html += `<div class="foot">Combines with:</div><div class="recipes">` +
        combos.map(([c, r]) => `<div class="itm">${itemImg(c)}<div><b>${itemName(r)}</b></div></div>`).join('') + '</div>';
    } else if (it.from) html += `<div class="foot">Made from ${DT.recipesOf(id).map((r) => r.map(itemName).join(' + ')).join(' or ')}</div>`;
    return html;
  }

  function traitTip(id) {
    const p = scouted() || game.human;
    const T = DT.TRAITS[id];
    const row = traitRows(p).find((r) => r.id === id);
    const n = row ? row.n : 0;
    const lv = DT.traitLevel(id, n);
    const counted = new Set(row ? row.units.map((u) => u.line) : []);
    const bench = [...new Set(p.bench.filter((u) => u && DT.unitTraits(u).includes(id) && !counted.has(u.line)).map((u) => DT.LINE[u.line].forms[u.stage]))];
    return `<h3 style="color:${T.color}">${T.name}</h3><div class="line">${T.kind ? T.kind[0].toUpperCase() + T.kind.slice(1) : 'Trait'} · ${n} unique Digimon on the board</div>
      <div class="ab">${esc(T.desc)}</div>
      ${T.bp.map((b, i) => `<div class="bpl ${i < lv ? 'on' : ''}"><b style="color:${i < lv ? T.color : 'inherit'}">(${b})</b> ${bpText(T, i)}</div>`).join('')}
      ${row && row.units.length ? `<div class="foot">Counting: ${row.units.map((u) => DT.LINE[u.line].forms[u.stage]).join(', ')}</div>` : ''}
      ${bench.length ? `<div class="foot">${p === game.human ? 'On your bench' : 'On their bench'}: ${bench.join(', ')}</div>` : ''}
      <div class="foot">Each different Digimon counts once.</div>`;
  }

  function playerTip(q) {
    const units = q.boardUnits().slice().sort((a, b) => DT.unitValue(b) - DT.unitValue(a));
    const counts = DT.countTraits(q.boardUnits(), q.augs);
    const act = Object.keys(counts).filter((t) => DT.traitLevel(t, counts[t])).map((t) => `<span class="tag" style="color:${DT.TRAITS[t].color};border-color:${DT.TRAITS[t].color}55">${counts[t]} ${DT.TRAITS[t].name}</span>`).join('');
    const h = (hist.get(q.id) || []).slice(-6);
    return `<div class="th">${avatarHTML(q)}<div><h3>${esc(q.name)}</h3><div class="line">${q.alive ? `${Math.max(0, q.hp)} HP · Level ${q.level}` : `Placed #${q.placement}`}${session ? (q.isHuman ? ' · Player' : ' · AI') : q === game.human ? '' : ' · AI'}</div></div></div>
      ${act ? `<div class="tags">${act}</div>` : ''}
      ${units.length ? `<div class="foot">Board: ${units.map((u) => DT.LINE[u.line].forms[u.stage] + (u.items.length ? ` (${u.items.length} item${u.items.length > 1 ? 's' : ''})` : '')).join(', ')}</div>` : ''}
      <div class="foot">Augments: ${q.augs.map((a) => DT.AUGMENT[a].name).join(', ') || 'none yet'}</div>
      ${h.length ? `<div class="foot">Recent: ${h.map((x) => `${x.round} ${x.res.toUpperCase()}`).join(' · ')}</div>` : ''}`;
  }

  function renderTooltip() {
    const el = $('tooltip');
    let html = '';
    ui.tipAnchor = null;
    const p = game && game.human;
    if (!p) { el.classList.add('hidden'); return; }
    const IC = DT.ItemCards; // js/itemcards.js: rich item cards
    if (ui.itemDrag && ui.hoverUid != null) {
      const f = game.findUnit(p, ui.hoverUid);
      const id = p.items[ui.itemDrag.idx];
      if (f && id) html = IC ? IC.unitCard(id, f.u, game, p) : `<div class="drop">${dropPreview(id, f.u)}</div>` + tooltipFor(DT.LINE[f.u.line], f.u.stage, null, f.u.items, f.u);
    } else if (ui.itemDrag && IC) {
      // over another bench item: what the two make; over empty ground: the held item's card
      const id = p.items[ui.itemDrag.idx];
      const over = ui.itemDrag.over;
      if (id && over >= 0 && p.items[over]) html = IC.pairCard(id, p.items[over]);
      else if (id && ui.itemDrag.x != null) html = IC.itemCard(id, p);
    } else if (ui.itemDrag) {
      // no tooltip while dragging over empty ground
    } else if (ui.hoverItem >= 0 && p.items[ui.hoverItem]) {
      html = IC ? IC.itemCard(p.items[ui.hoverItem], p) : itemTip(p.items[ui.hoverItem]);
    } else if (ui.hoverWisp && p.wisp) {
      const w = DT.WISP[p.wisp.id];
      html = `<div class="itip">${DT.ItemArt.img(`wisp:${w.id}`, 64)}<div><h3>${w.name}</h3><div class="line">${w.kind} Wisp · ${p.wisp.cost} gold · one use, this round only</div></div></div><div class="ab">${esc(w.desc)}</div>`;
      ui.tipAnchor = { el: document.querySelector('.card.wisp'), side: 'top' };
    } else if (ui.hoverShop >= 0 && p.shop[ui.hoverShop]) {
      const id = p.shop[ui.hoverShop];
      const su = shopUnit(p, ui.hoverShop);
      html = tooltipFor(DT.LINE[id], su.stage, null, [], su) +
        (su.meta && su.meta.tag ? `<div class="foot mtagline">${esc(su.meta.tag)} offer${su.meta.note ? `: ${esc(su.meta.note)}` : ''}</div>` : '') +
        `<div class="foot">${game.pool[id]} more in the shared pool (of ${DT.POOL_SIZE[DT.LINE[id].tier]})</div>`;
      ui.tipAnchor = { el: document.querySelector(`.card[data-slot="${ui.hoverShop}"]`), side: 'top' };
    } else if (ui.hoverTrait && DT.TRAITS[ui.hoverTrait]) {
      html = traitTip(ui.hoverTrait);
      ui.tipAnchor = { el: document.querySelector(`.trait[data-trait="${ui.hoverTrait}"]`), side: 'right' };
    } else if (ui.hoverPlayer >= 0 && game.players[ui.hoverPlayer]) {
      html = playerTip(game.players[ui.hoverPlayer]);
      ui.tipAnchor = { el: document.querySelector(`.pl[data-pid="${ui.hoverPlayer}"]`), side: 'left' };
    } else if (ui.hoverCombatId != null && shownCombat()) {
      const u = shownCombat().units[ui.hoverCombatId];
      if (u && u.alive) html = tooltipFor(u.line, u.stage, u, [], u);
    } else if (ui.hoverUid != null) {
      const f = game.findUnit(p, ui.hoverUid);
      if (f) html = tooltipFor(DT.LINE[f.u.line], f.u.stage, null, f.u.items, f.u) + (DT.canSell(f.u) ? `<div class="foot">Sell for ${DT.sellValue(f.u)} gold (E)${f.u.items.length ? ', items return to your bench' : ''}</div>` : `<div class="foot">Can't be sold</div>`);
    }
    if (html !== el._html) { el.innerHTML = html; el._html = html; }
    el.classList.toggle('hidden', !html);
    el.classList.toggle('icard', html.startsWith('<div class="ic'));
    if (html) placeTip();
  }

  // Next to the hovered panel element, or beside the cursor over the board; always kept on screen.
  function placeTip() {
    const el = $('tooltip');
    const w = el.offsetWidth, h = el.offsetHeight;
    const W = window.innerWidth, Hh = window.innerHeight;
    let x, y;
    const a = ui.tipAnchor && ui.tipAnchor.el ? ui.tipAnchor.el.getBoundingClientRect() : null;
    if (a && ui.tipAnchor.side === 'top') { x = a.left + a.width / 2 - w / 2; y = a.top - h - 8; }
    else if (a && ui.tipAnchor.side === 'right') { x = a.right + 8; y = a.top; }
    else if (a && ui.tipAnchor.side === 'left') { x = a.left - w - 8; y = a.top; }
    else { x = ui.cx + 22; y = ui.cy + 16; if (x + w > W - 8) x = ui.cx - w - 22; }
    x = Math.max(8, Math.min(W - w - 8, x));
    y = Math.max(8, Math.min(Hh - h - 8, y));
    el.style.left = `${x}px`;
    el.style.top = `${y}px`;
  }

  // ---------- cast feed: who cast what, newest at the bottom ----------
  function clearCasts() { $('castFeed').innerHTML = ''; }
  renderer.onCast = (c) => {
    const feed = $('castFeed');
    const u = c.unit;
    const row = document.createElement('div');
    row.className = `cf ${c.team === 0 ? 'ally' : 'enemy'}${u.stage >= 2 ? ' big' : ''}`;
    row.style.setProperty('--ec', c.color);
    row.innerHTML = `${P.html(u.line.id, u.stage)}<div style="min-width:0"><div class="cn">${c.team === 0 ? '' : 'Enemy '}${u.name}</div><div class="ca">${esc(c.name)}</div></div>`;
    feed.appendChild(row);
    while (feed.children.length > 5) feed.firstChild.remove();
    const life = Math.max(900, 2600 / ui.speed);
    setTimeout(() => row.classList.add('out'), life);
    setTimeout(() => row.remove(), life + 400);
  };

  // ---------- Little Legend: the store's Locker, or a list of owned legends when there is no Locker ----------
  function openPicker() {
    if (DT.Shop && DT.Shop.openLocker) return DT.Shop.openLocker('legend');
    const cur = legendId();
    const tiles = DT.Cosmetics.list('legend').filter((x) => DT.Cosmetics.owns('legend', x.id)).map((x) =>
      `<div class="lg ${x.id === cur ? 'on' : ''}" data-lid="${esc(x.id)}" title="${esc(x.desc || x.name)}">${DT.Legends.html(x.id)}<span>${esc(x.name)}</span></div>`).join('');
    const el = $('picker');
    el.innerHTML = `<div class="pk"><div class="pkh"><h2>Little Legend</h2><div class="muted">Your tactician on the board. Other players see it in combat.</div>
      <button id="pickClose">Done</button></div><div class="pkgrid lgown">${tiles}</div></div>`;
    el.classList.remove('hidden');
  }
  function closePicker() { $('picker').classList.add('hidden'); }
  $('picker').addEventListener('click', (ev) => {
    const t = ev.target;
    if (t.id === 'picker' || t.id === 'pickClose') return closePicker();
    const tile = t.closest('.lg[data-lid]');
    if (tile) DT.Cosmetics.equip('legend', tile.dataset.lid);
  });
  $('legendBtn').onclick = openPicker;

  function fillTitleLegend() {
    const el = $('titleLegend');
    if (!el) return;
    const it = DT.Cosmetics.get('legend', legendId());
    const evo = DT.LegendEvo && DT.LegendEvo.info(legendId());
    const name = evo ? `${evo.formInfo.name} <span class="muted">Lv ${evo.level}</span>` : esc(it ? it.name : '');
    el.innerHTML = `${DT.Legends.html(legendKey())}<div><div class="lt">Little Legend</div><div class="ln">${name}</div><button id="pickLegendBtn">Choose</button>${evo ? ` <button data-evo-open="${esc(legendId())}">Digivolve</button>` : ''}</div>`;
    $('pickLegendBtn').onclick = openPicker;
  }

  // ---------- input ----------
  function unitAt(pt) {
    if (ui.viewing != null) return null; // another Tamer's board is read-only
    const p = game.human;
    const bi = renderer.benchAt(pt.x, pt.y);
    if (bi >= 0 && p.bench[bi]) return { unit: p.bench[bi], from: { type: 'bench', i: bi } };
    if (game.phase === 'combat' && combat) return null;
    const cell = renderer.cellAt(pt.x, pt.y);
    if (cell && cell.r >= 4) {
      const u = p.board[DT.cellKey(cell.c, cell.r)];
      if (u) return { unit: u, from: { type: 'board', c: cell.c, r: cell.r } };
    }
    return null;
  }

  function combatUnitAt(pt) {
    const cb = shownCombat();
    return cb ? renderer.pickCombat(pt, cb) : null;
  }

  // Item drag: from the item bench onto a Digimon.
  function endItemDrag() {
    ui.itemDrag = null;
    canvas.classList.remove('grabbing');
  }
  // Camera: wheel zooms at the cursor, right-drag rotates, left-drag on empty ground (or middle-drag) pans, R resets.
  let cam = null;
  canvas.addEventListener('wheel', (ev) => {
    ev.preventDefault();
    const pt = renderer.toLocal(ev);
    renderer.zoomAt(pt.x, pt.y, Math.exp(ev.deltaY * 0.0015));
  }, { passive: false });
  canvas.addEventListener('contextmenu', (ev) => ev.preventDefault());
  window.addEventListener('mousemove', (ev) => {
    ui.cx = ev.clientX;
    ui.cy = ev.clientY;
    if (!cam) return;
    const pt = renderer.toLocal(ev);
    if (!cam.moved && Math.hypot(pt.x - cam.sx, pt.y - cam.sy) < 5) return;
    cam.moved = true;
    if (cam.mode === 'orbit') renderer.orbitBy(pt.x - cam.x, pt.y - cam.y);
    else renderer.panBy(cam.x, cam.y, pt.x, pt.y);
    cam.x = pt.x;
    cam.y = pt.y;
  });
  window.addEventListener('mouseup', (ev) => {
    if (cam && cam.mode === 'orbit' && !cam.moved && game && ui.viewing == null) renderer.moveLegend(cam.sx, cam.sy);
    cam = null;
    canvas.classList.remove('grabbing');
  });

  canvas.addEventListener('mousedown', (ev) => {
    if (!game) return;
    const pt = renderer.toLocal(ev);
    const startCam = (mode) => { cam = { mode, x: pt.x, y: pt.y, sx: pt.x, sy: pt.y, moved: false }; canvas.classList.add('grabbing'); ev.preventDefault(); };
    if (ev.button === 2) return startCam('orbit');
    if (ev.button === 1) return startCam('pan');
    if (ev.button !== 0) return;
    const slot = game.over || ui.viewing != null ? -1 : renderer.itemSlotAt(pt.x, pt.y, game.human.items.length);
    if (slot >= 0) {
      ui.itemDrag = { idx: slot, x: pt.x, y: pt.y };
      canvas.classList.add('grabbing');
      ev.preventDefault();
      return;
    }
    const hit = game.over ? null : unitAt(pt);
    if (!hit) return startCam('pan');
    ui.drag = { ...hit, x: pt.x, y: pt.y, sx: pt.x, sy: pt.y, moved: false };
    ev.preventDefault();
  });

  window.addEventListener('mousemove', (ev) => {
    if (!game || cam) return;
    const pt = renderer.toLocal(ev);
    ui.mouse = pt;
    if (ui.itemDrag) {
      ui.itemDrag.x = pt.x;
      ui.itemDrag.y = pt.y;
      const over = ev.target === canvas ? renderer.itemSlotAt(pt.x, pt.y, game.human.items.length) : -1;
      ui.itemDrag.over = over === ui.itemDrag.idx ? -1 : over;
      const hit = ev.target === canvas && over < 0 ? unitAt(pt) : null;
      ui.hoverUid = hit ? hit.unit.uid : null;
      renderTooltip();
      return;
    }
    if (ui.drag) {
      ui.drag.x = pt.x;
      ui.drag.y = pt.y;
      if (Math.hypot(pt.x - ui.drag.sx, pt.y - ui.drag.sy) > 4) ui.drag.moved = true;
      const cell = renderer.cellAt(pt.x, pt.y);
      ui.hoverCell = cell && cell.r >= 4 && game.phase !== 'combat' ? cell : null;
      ui.hoverBench = renderer.benchAt(pt.x, pt.y);
      const overShop = !!(ev.target.closest && ev.target.closest('#shopbar'));
      $('sellZone').classList.toggle('hidden', !(ui.drag.moved && overShop));
      if (ui.drag.moved) $('sellZone').textContent = DT.canSell(ui.drag.unit) ? `Sell for ${DT.sellValue(ui.drag.unit)} gold` : "Can't be sold";
      return;
    }
    const prev = [ui.hoverUid, ui.hoverCombatId, ui.hoverItem, ui.hoverTrait, ui.hoverPlayer];
    ui.hoverUid = null;
    ui.hoverCombatId = null;
    ui.hoverItem = -1;
    const tr = ev.target.closest && ev.target.closest('.trait[data-trait]');
    ui.hoverTrait = tr ? tr.dataset.trait : null;
    const pl = ev.target.closest && ev.target.closest('.pl[data-pid]');
    ui.hoverPlayer = pl ? Number(pl.dataset.pid) : -1;
    if (ev.target === canvas && ui.viewing == null) ui.hoverItem = renderer.itemSlotAt(pt.x, pt.y, game.human.items.length);
    if (ev.target === canvas && ui.hoverItem < 0) {
      const hit = unitAt(pt);
      if (hit) ui.hoverUid = hit.unit.uid;
      else if (shownCombat() && game.phase !== 'planning') {
        const cu = combatUnitAt(pt);
        if (cu) ui.hoverCombatId = cu.id;
      }
    }
    const changed = prev[0] !== ui.hoverUid || prev[1] !== ui.hoverCombatId || prev[2] !== ui.hoverItem || prev[3] !== ui.hoverTrait || prev[4] !== ui.hoverPlayer;
    if (changed) renderTooltip();
    else if (!$('tooltip').classList.contains('hidden') && !ui.tipAnchor) placeTip();
  });

  window.addEventListener('mouseup', (ev) => {
    if (ui.itemDrag) {
      const idx = ui.itemDrag.idx;
      const portal = ev.target.closest && ev.target.closest('#duoPortal');
      endItemDrag();
      if (portal && ownView()) { act('portal', { item: idx }); return; }
      if (ev.target === canvas) {
        const pt = renderer.toLocal(ev);
        // dropped on another bench item: swap pedestals (items only combine on a Digimon, as in TFT)
        const slot = renderer.itemSlotAt(pt.x, pt.y, game.human.items.length);
        const hit = slot < 0 ? unitAt(pt) : null;
        if (slot >= 0 && slot !== idx) act('swapItems', { a: idx, b: slot });
        else if (hit) equip(idx, hit.unit.uid);
      }
      ui.hoverUid = null;
      renderTooltip();
      return;
    }
    if (!ui.drag) return;
    const d = ui.drag;
    ui.drag = null;
    ui.hoverCell = null;
    ui.hoverBench = -1;
    $('sellZone').classList.add('hidden');
    if (!d.moved) return;
    if (ev.target.closest && ev.target.closest('#duoPortal') && ownView()) { act('portal', { uid: d.unit.uid }); return; }
    if (ev.target.closest && ev.target.closest('#shopbar')) return sellUid(d.unit.uid);
    const pt = renderer.toLocal(ev);
    const bi = renderer.benchAt(pt.x, pt.y);
    let to = null;
    if (bi >= 0) to = { type: 'bench', i: bi };
    else {
      const cell = renderer.cellAt(pt.x, pt.y);
      if (cell && cell.r >= 4) to = { type: 'board', c: cell.c, r: cell.r };
    }
    if (!to || !ownView()) return;
    act('move', { from: d.from, to });
  });

  $('shop').addEventListener('click', (ev) => {
    const card = ev.target.closest('.card');
    if (!card) return;
    if (card.dataset.wisp) return buyWisp();
    if (card.dataset.slot != null) buy(Number(card.dataset.slot));
  });
  $('shop').addEventListener('mouseover', (ev) => {
    const card = ev.target.closest('.card');
    const slot = card && card.dataset.slot != null ? Number(card.dataset.slot) : -1;
    const wisp = !!(card && card.dataset.wisp);
    if (slot !== ui.hoverShop || wisp !== ui.hoverWisp) { ui.hoverShop = slot; ui.hoverWisp = wisp; renderTooltip(); }
  });
  $('shop').addEventListener('mouseleave', () => { ui.hoverShop = -1; ui.hoverWisp = false; renderTooltip(); });
  $('dmgChart').addEventListener('click', (ev) => {
    const b = ev.target.closest('[data-chart]');
    if (b) { ui.chartMode = b.dataset.chart; renderChart(true); }
  });

  $('modal').addEventListener('click', (ev) => {
    if (!game) return;
    const p = game.human;
    const t = ev.target;
    if (t.id === 'peekBtn') { ui.peek = !ui.peek; return renderModal(); }
    if (t.id === 'introOk') { if (ui.intro) ui.intro.seen = true; return renderModal(); }
    if (t.id === 'offerReroll') return act('offerReroll');
    if (t.id === 'carouselGo') return session ? null : commit({ k: 'next' });
    const c = t.closest('[data-carousel]');
    if (c && game.carousel && game.carouselTurn() === p) return act('carousel', { i: Number(c.dataset.carousel) });
    const o = t.closest('[data-offer]');
    // The host already owns the offer kind. Only the choice index belongs on the wire.
    if (o && p.offer) act('offer', { i: Number(o.dataset.offer) });
  });

  $('readyBtn').onclick = ready;
  $('players').addEventListener('click', (ev) => { if (ev.target.closest('#mpPlayerControls')) { ev.stopPropagation(); DT.Net?.players(); } });
  $('skipBtn').onclick = skip;
  $('rollBtn').onclick = reroll;
  $('lockBtn').onclick = () => act('lock');
  $('timerBtn').onclick = () => {
    ui.timerOn = !ui.timerOn;
    ui.planLeft = H.planTime;
    $('timerBtn').textContent = ui.timerOn ? 'Timer on' : 'Timer off';
    $('timerBtn').classList.toggle('off', !ui.timerOn);
  };
  const soundLabel = () => { $('soundBtn').textContent = { all: 'Sound on', effects: 'Music off', off: 'Muted' }[DT.Sfx.mode]; $('soundBtn').classList.toggle('off', DT.Sfx.mode === 'off'); };
  soundLabel();
  // the settings popover (soundui.js) when loaded, else cycle on / music off / muted
  $('soundBtn').onclick = () => { if (DT.SoundUI) return DT.SoundUI.toggle(); DT.Sfx.cycle(); soundLabel(); if (DT.Sfx.mode !== 'off') DT.Sfx.play('buy'); };
  $('speedBtn').onclick = () => {
    ui.speed = ui.speed === 1 ? 2 : ui.speed === 2 ? 4 : 1;
    renderer.speed = ui.speed;
    $('speedBtn').textContent = `${ui.speed}x`;
  };
  const titleHTML = $('overlay').querySelector('.panel').innerHTML;
  function bindTitle() {
    $('startBtn').onclick = newGame;
    $('creditsBtn').onclick = (ev) => { ev.preventDefault(); showCredits(); };
    const mp = $('mpBtn');
    if (mp) mp.onclick = () => (DT.Net ? DT.Net.open() : toast('Multiplayer is not available here'));
    fillTitleLegend();
    // warm the portrait cache with every shop Digimon's base form while the title screen is up
    setTimeout(() => { if (!game && !session) P.prefetch(DT.SHOP_LINES.map((l) => P.key(l.id, 0))); }, 1500);
  }
  function showTitle() {
    if (DT.Sfx) DT.Sfx.music(false);
    const o = $('overlay');
    if (DT.Client) { o.classList.remove('hidden'); bindTitle(); return DT.Client.home(); }
    setPanel(titleHTML, true);
    o.classList.remove('hidden');
    bindTitle();
  }
  function showCredits() {
    if (DT.Client) return DT.Client.credits();
    const rows = (DT.AssetCredits || []).map(s => `<div><b>${esc(s.name)}</b>: ${esc(s.contributors.join(', ') || 'Contributor not yet verified')} (${esc(s.license)})${s.url ? ` <a href="${esc(s.url)}" target="_blank" rel="noopener">Source</a>` : ''}</div>`).join('');
    setPanel(`<h1 style="font-size:26px">Credits &amp; acknowledgements</h1>
      <p class="sub">An unofficial, non-commercial fan project. Model sources and recorded contributors are listed below. Full acknowledgements are in CREDITS.md.</p>
      <div class="credits">${rows || 'No models configured.'}</div>
      <button id="backBtn" class="primary">Back</button>`);
    $('backBtn').onclick = () => { setPanel(titleHTML, true); bindTitle(); };
  }
  bindTitle();

  window.addEventListener('keydown', (ev) => {
    if (ev.target && /input|textarea|select/i.test(ev.target.tagName)) return;
    const k = ev.key.toLowerCase();
    if (k === 'escape') return closePicker();
    const mid = () => ({ x: canvas.clientWidth / 2, y: canvas.clientHeight / 2 });
    if (k === 'r') return renderer.resetView();
    if (k === '=' || k === '+') return renderer.zoomAt(mid().x, mid().y, 0.8);
    if (k === '-' || k === '_') return renderer.zoomAt(mid().x, mid().y, 1.25);
    if (!game || game.over || ev.repeat) return;
    if (k === 'd') reroll();
    else if (k === 'f') buyXp();
    else if (k === 'e' && ui.hoverUid != null) sellUid(ui.hoverUid);
    else if (k === ' ') { ev.preventDefault(); if (game.phase === 'planning') ready(); }
  });

  function fit() {
    const arena = $('arena');
    renderer.fit(arena.clientWidth - 16, arena.clientHeight - 16);
  }
  window.addEventListener('resize', fit);

  const OUT_PLAN_S = 4; // planning length while a solo player spectates after elimination
  // Planning timer: auto-starts combat at zero, picking a random option if a pick is still open.
  function tickTimer(dt) {
    const el = $('timerLabel');
    const box = $('timerBox');
    const p = game.human;
    // solo spectator (out, AI Tamers playing on): a short look at the boards, then the next fight
    if (!session && !p.alive && game.playOut && !game.over && game.phase === 'planning' && game.roundType !== 'carousel') {
      ui.outPlan = (ui.outPlan || 0) + dt;
      if (ui.outPlan >= OUT_PLAN_S) { ui.outPlan = 0; commit({ k: 'start' }); }
    }
    const running = (ui.timerOn || session) && game.phase === 'planning' && game.roundType !== 'carousel' && !game.over && p.alive;
    box.classList.toggle('off', !running);
    if (!running) { el.textContent = ''; return; }
    // multiplayer: the host's clock decides; at zero the host starts combat and open picks are chosen for you
    if (session) ui.planLeft = (game.phaseAt + game.planMs - session.now()) / 1000;
    else ui.planLeft -= dt;
    const left = Math.max(0, Math.ceil(ui.planLeft));
    if (left !== ui.lastLeft) { if (left > 0 && left <= 5 && DT.Sfx) DT.Sfx.play('tick', { last: left === 1 }); ui.lastLeft = left; }
    el.textContent = `${left}s`;
    el.classList.toggle('low', left <= 5);
    box.classList.toggle('low', left <= 5);
    $('timerBar').firstChild.style.width = `${Math.max(0, Math.min(100, (100 * ui.planLeft * 1000) / game.planMs))}%`;
    if (!session && ui.planLeft <= 0) commit({ k: 'start' });
  }

  // The Little Legend opens any orb it walks into.
  function checkOrbs() {
    const p = game.human;
    if (!p.orbs.length) return;
    const L = renderer.legendPos();
    for (const o of p.orbs.slice()) {
      const w = renderer.orbWorld(o);
      if (ui.orbPending.has(o.id) || Math.hypot(w.x - L.x, w.z - L.z) > 0.85) continue;
      ui.orbPending.add(o.id);
      act('orb', { id: o.id });
    }
  }

  // ---------- loop ----------
  // Put a fight on screen: your own `combat`, or a spectated replay (js/scout.js). Switching clears the old fight's
  // effects and unit views (resetFx bumps the view keys, so sync() disposes them); a fight joined midway starts from
  // its current state without replaying the effects it already had. fresh: show its opening events too.
  let combatFresh = false;
  function show(cb, team, fresh) {
    if (cb === drawn && (!cb || team === drawnTeam)) return;
    drawn = cb;
    drawnTeam = team;
    renderer.resetFx();
    clearCasts();
    renderer.mirror = !!cb && team === 1;
    if (cb) renderer.evIdx = fresh ? 0 : cb.events.length;
    combatFresh = false;
    ui.hoverCombatId = null;
    renderChart(true);
  }

  function frame(now) {
    const dt = Math.min(0.1, (now - last) / 1000);
    last = now;
    if (paused) { requestAnimationFrame(frame); return; }
    if (game) {
      tickTimer(dt);
      checkOrbs();
      const fighting = game.phase === 'combat' || game.phase === 'result';
      if (game.phase === 'combat' && combat) {
        if (!combat.done) {
          acc += dt * ui.speed;
          while (acc >= DT.TICK && !combat.done) {
            combat.step(DT.TICK);
            acc -= DT.TICK;
          }
        } else if (!combatEndAt) {
          combatEndAt = now;
        } else if (now - combatEndAt > 700) {
          combatEndAt = Infinity;
          combatWatched();
        }
      }
      // the round's clock: spectated fights are stepped to it, so they stay at the same moment as yours
      if (fighting) {
        if (game.phase === 'combat' && combat && !combat.done) clock = combat.t;
        else clock += dt * ui.speed;
      }
      // solo without a fight of your own (spectating after elimination): the round ends when the longest fight has
      if (game.phase === 'combat' && !combat && !session && !combatReported && clock >= longestFight() + 0.7) combatWatched();
      let cb = null;
      let team = 0;
      let fresh = false;
      if (fighting && ui.viewing != null) {
        const f = DT.Scout && DT.Scout.fight ? DT.Scout.fight(clock) : null;
        if (f) { cb = f.cb; team = f.team; fresh = f.fresh; }
      } else if (fighting && combat) { cb = combat; team = myTeam; fresh = combatFresh; }
      show(cb, team, fresh);
      if (cb) {
        renderer.consume(cb);
        if (!cb.done || game.phase === 'combat') {
          if (ui.hoverCombatId != null) renderTooltip();
          renderChart(false);
        }
        renderer.drawCombat(game, ui, cb);
      } else {
        if (game.phase === 'planning') $('dmgChart').classList.add('hidden');
        renderer.drawPlanning(game, ui);
      }
    }
    requestAnimationFrame(frame);
  }

  // net.js uses these to show lobby screens and hand over a multiplayer game
  // spectating (js/scout.js) reads: own() your fight this round, clock() the round's combat time, leaveSpectate()
  DT.UI = { startNetGame, leaveNet, showTitle, newGame, toast, panel: () => $('overlay').querySelector('.panel'), overlay: () => $('overlay'), inGame: () => !!session, game: () => game, session: () => session, renderer: () => renderer, ui: () => ui, refresh,
    paused: () => paused, own: () => ({ combat, team: myTeam }), clock: () => clock, shown: () => ({ combat: drawn, team: drawnTeam }), leaveSpectate };
  window.__digi = { get game() { return game; }, get session() { return session; }, renderer, ui, get clock() { return clock; }, get shown() { return drawn; }, get own() { return combat; } };
  fit();
  requestAnimationFrame(frame);
})();
