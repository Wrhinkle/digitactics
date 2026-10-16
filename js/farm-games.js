// Digi-Farm training mini-games, 10-20 seconds each. Each one scores 0..1; js/farm.js turns the score into stats.
//   strength  Power Swing    stop the swinging marker in the gold zone, 5 swings
//   speed     Beat Dash      hit the notes as they reach the ring, two lanes
//   wisdom    Data Memory    repeat the pad sequence, 3 rounds (3, 4, 5 pads)
//   stamina   Endurance Run  tap to keep the meter in the green band for 12 seconds
// DT.FarmGames.run(name, host, { auto, speed, ownKeys }) -> { done: Promise<score, -1 if cancelled>, cancel(), input(x), key(e) }
//   auto: plays itself through the same input path (tests); speed: time scale (tests)
const DT = window.DT;

const sfx = (n) => { try { if (DT.Sfx) DT.Sfx.play(n); } catch (e) { /* audio blocked */ } };
const clamp = (x, a, b) => Math.max(a, Math.min(b, x));

const GAMES = {
  strength: { title: 'Power Swing', stat: 'Strength', help: 'Press Space or click when the marker is in the gold zone. 5 swings.', make: swing },
  speed: { title: 'Beat Dash', stat: 'Speed', help: 'Hit F / Up for the top lane and J / Down for the bottom lane as notes reach the ring. Click a lane works too.', make: beats },
  wisdom: { title: 'Data Memory', stat: 'Wisdom', help: 'Watch the pads light up, then repeat them. Click or press 1-4.', make: memory },
  stamina: { title: 'Endurance Run', stat: 'Stamina', help: 'Tap Space or click to keep the meter inside the green band.', make: endurance },
};

function run(name, host, opts = {}) {
  const G = GAMES[name];
  if (!G) throw new Error(`unknown game ${name}`);
  const speed = opts.speed || 1;
  host.innerHTML = `<div class="dg" data-game="${name}">
    <div class="dg-top"><h3>${G.title}</h3><span>${G.stat} training</span></div>
    <div class="dg-help">${G.help}</div>
    <div class="dg-time"><i style="width:100%"></i></div>
    <div class="dg-stage"><div class="dg-big"></div></div>
    <div class="dg-score"><span class="dg-l"></span><span class="dg-r"></span></div></div>`;
  const stage = host.querySelector('.dg-stage');
  const big = host.querySelector('.dg-big');
  const timeBar = host.querySelector('.dg-time i');
  const ui = {
    stage, big, left: host.querySelector('.dg-l'), right: host.querySelector('.dg-r'),
    say(t, ms = 600) { big.textContent = t; clearTimeout(ui.st); if (ms) ui.st = setTimeout(() => { if (big.textContent === t) big.textContent = ''; }, ms / speed); },
    time(f) { timeBar.style.width = `${clamp(f, 0, 1) * 100}%`; },
  };
  let resolve;
  const done = new Promise((r) => { resolve = r; });
  const game = G.make(ui, opts);
  let raf = 0, last = 0, t = -(opts.auto || speed > 1 ? 0.3 : 1.5), over = false;
  const finish = (score) => {
    if (over) return;
    over = true;
    cancelAnimationFrame(raf);
    cleanup();
    resolve(clamp(score, 0, 1));
  };
  const frame = (ms) => {
    raf = requestAnimationFrame(frame);
    const dt = Math.min(0.05, (ms - (last || ms)) / 1000) * speed;
    last = ms;
    if (t < 0) { t += dt; ui.say(t < -1 ? 'Ready' : 'Go!', 0); if (t >= 0) ui.say('Go!', 500); return; }
    t += dt;
    const r = game.update(dt, t);
    if (opts.auto && game.auto) game.auto(t);
    if (r != null) finish(r);
  };
  raf = requestAnimationFrame(frame);
  const onKey = (e) => {
    if (over) return;
    if (e.key === 'Escape') return; // the farm closes the game
    const x = game.key && game.key(e.key);
    if (x !== undefined && t >= 0) { e.preventDefault(); e.stopPropagation(); game.input(x); }
  };
  const onDown = (e) => { if (over || t < 0) return; const x = game.click ? game.click(e) : 0; if (x !== undefined) game.input(x); };
  // opts.ownKeys: false when the host forwards key events through handle.key(e) (the farm holds all keys while open)
  if (opts.ownKeys !== false) window.addEventListener('keydown', onKey, true);
  stage.addEventListener('pointerdown', onDown);
  const cleanup = () => { window.removeEventListener('keydown', onKey, true); stage.removeEventListener('pointerdown', onDown); };
  return { done, cancel: () => finish(-1), input: (x) => game.input(x), key: onKey, get over() { return over; }, get time() { return t; } };
}

// ---------- strength: Power Swing ----------
function swing(ui) {
  const ZONES = [0.2, 0.17, 0.14, 0.12, 0.1];
  const SPEEDS = [0.9, 1.05, 1.2, 1.35, 1.55]; // track widths per second
  const LIMIT = 3; // seconds per swing before it counts as a miss
  ui.stage.insertAdjacentHTML('beforeend', '<div class="dg-sw-log"></div><div class="dg-sw-track"><div class="dg-sw-zone"></div><div class="dg-sw-mark"></div></div>');
  const zoneEl = ui.stage.querySelector('.dg-sw-zone'), mark = ui.stage.querySelector('.dg-sw-mark'), log = ui.stage.querySelector('.dg-sw-log');
  let i = 0, pos = 0, dir = 1, swingT = 0, pause = 0, center = 0.5, lastDt = 0.016;
  const scores = [];
  const setZone = () => { center = 0.3 + ((i * 37) % 5) * 0.1; zoneEl.style.left = `${(center - ZONES[i] / 2) * 100}%`; zoneEl.style.width = `${ZONES[i] * 100}%`; };
  setZone();
  const total = () => scores.reduce((a, b) => a + b, 0);
  const show = () => { ui.left.textContent = `Swing ${Math.min(i + 1, 5)} / 5`; ui.right.textContent = `Power ${Math.round(total() * 20)}`; };
  show();
  const judge = (miss) => {
    const half = ZONES[i] / 2, d = Math.abs(pos - center);
    const s = miss ? 0 : d <= half ? 1 - (0.3 * d) / half : Math.max(0, 0.5 - (d - half) * 3);
    scores.push(s);
    ui.say(s >= 0.9 ? 'POW!' : s >= 0.7 ? 'Great' : s > 0.3 ? 'OK' : 'Miss', 700);
    if (s > 0.3) { log.classList.remove('hit'); void log.offsetWidth; log.classList.add('hit'); sfx(s >= 0.9 ? 'hit' : 'place'); } else sfx('error');
    i++;
    pause = 0.55;
    if (i < 5) setZone();
    show();
  };
  return {
    update(dt, t) {
      ui.time(1 - (i + Math.min(1, swingT / LIMIT)) / 5);
      if (i >= 5) return pause > 0 ? ((pause -= dt), null) : total() / 5;
      if (pause > 0) { pause -= dt; return null; }
      swingT += dt;
      lastDt = dt;
      pos += dir * SPEEDS[i] * dt;
      if (pos > 1) { pos = 2 - pos; dir = -1; } else if (pos < 0) { pos = -pos; dir = 1; }
      mark.style.left = `${pos * 100}%`;
      if (swingT >= LIMIT) { swingT = 0; judge(true); }
      return null;
    },
    input() { if (i >= 5 || pause > 0) return; swingT = 0; judge(false); },
    key: (k) => (k === ' ' || k === 'Enter' ? 0 : undefined),
    click: () => 0,
    auto() { if (i < 5 && pause <= 0 && Math.abs(pos - center) < Math.max(0.012, SPEEDS[i] * lastDt * 0.55)) this.input(); },
  };
}

// ---------- speed: Beat Dash ----------
function beats(ui) {
  const BEAT = 60 / 128;
  // [beat, lane] pairs; 18 notes over about 13 seconds
  const PATTERN = [[2, 0], [3, 0], [4, 1], [5, 1], [6, 0], [6.5, 0], [7, 1], [8, 0], [9, 1], [10, 1], [10.5, 0], [11, 0], [12, 1], [13, 0], [13.5, 1], [14, 0], [15, 1], [16, 0]];
  const TRAVEL = 1.6; // seconds from the right edge to the ring
  const END = 16 * BEAT + 0.8;
  ui.stage.insertAdjacentHTML('beforeend', '<div class="dg-lane" style="top:40px"><span class="k">F / Up</span><div class="dg-ring"></div></div><div class="dg-lane" style="top:150px"><span class="k">J / Down</span><div class="dg-ring"></div></div>');
  const lanes = [...ui.stage.querySelectorAll('.dg-lane')];
  const rings = lanes.map((l) => l.querySelector('.dg-ring'));
  const notes = PATTERN.map(([b, lane]) => ({ at: b * BEAT, lane, el: null, done: false, s: 0 }));
  let now = 0, sum = 0, hits = 0, combo = 0, lastDt = 0.016;
  const w = () => ui.stage.clientWidth || 600;
  const show = () => { ui.left.textContent = `Hits ${hits} / ${notes.length}`; ui.right.textContent = combo > 2 ? `Combo ${combo}` : ''; };
  show();
  const flash = (lane) => { rings[lane].classList.add('flash'); setTimeout(() => rings[lane].classList.remove('flash'), 90); };
  return {
    update(dt, t) {
      now = t;
      lastDt = dt;
      ui.time(1 - t / END);
      for (const n of notes) {
        const left = n.at - t; // seconds until it reaches the ring
        if (!n.done && left < -0.2) { n.done = true; combo = 0; if (n.el) n.el.remove(); ui.say('Miss', 400); show(); }
        if (n.done) continue;
        if (left <= TRAVEL && !n.el) { n.el = document.createElement('div'); n.el.className = `dg-note${n.lane ? ' b' : ''}`; lanes[n.lane].appendChild(n.el); }
        if (n.el) n.el.style.left = `${90 + (w() - 110) * (left / TRAVEL)}px`;
      }
      return t >= END ? sum / notes.length : null;
    },
    input(lane) {
      flash(lane);
      let best = null;
      for (const n of notes) if (!n.done && n.lane === lane && Math.abs(n.at - now) < 0.25 && (!best || Math.abs(n.at - now) < Math.abs(best.at - now))) best = n;
      if (!best) return;
      const d = Math.abs(best.at - now);
      best.done = true;
      if (best.el) best.el.remove();
      const s = d < 0.07 ? 1 : d < 0.14 ? 0.6 : 0.25;
      sum += s; hits++; combo++;
      ui.say(s === 1 ? 'Perfect' : s > 0.5 ? 'Good' : 'Late', 350);
      sfx('tick');
      show();
    },
    key: (k) => (k === 'f' || k === 'F' || k === 'ArrowUp' || k === 'w' ? 0 : k === 'j' || k === 'J' || k === 'ArrowDown' || k === 's' ? 1 : undefined),
    click(e) { const r = ui.stage.getBoundingClientRect(); return e.clientY - r.top < 140 ? 0 : 1; },
    auto(t) { for (const n of notes) if (!n.done && Math.abs(n.at - t) < Math.max(0.03, lastDt * 0.55)) this.input(n.lane); },
  };
}

// ---------- wisdom: Data Memory ----------
function memory(ui) {
  const PADS = [['★', '#ffcf4a'], ['♥', '#ff6f9a'], ['◆', '#4fb3ff'], ['●', '#9fe07a']];
  const ROUNDS = [3, 4, 5];
  const LIMIT = 24;
  const SHOW = 0.55, GAP = 0.18;
  ui.stage.insertAdjacentHTML('beforeend', `<div class="dg-pads">${PADS.map(([s, c], i) => `<button class="dg-pad" data-pad="${i}" style="color:${c};background:${c}33"><i>${i + 1}</i>${s}</button>`).join('')}</div>`);
  const pads = [...ui.stage.querySelectorAll('.dg-pad')];
  let round = 0, seq = [], phase = 'show', pt = 0, idx = 0, got = 0, litT = 0, litPad = -1, seed = 7;
  const totalPads = ROUNDS.reduce((a, b) => a + b, 0);
  const rnd = () => { seed = (seed * 16807) % 2147483647; return seed / 2147483647; };
  const newRound = () => { seq = Array.from({ length: ROUNDS[round] }, () => Math.floor(rnd() * 4)); phase = 'show'; pt = -0.4; idx = 0; ui.say(`Round ${round + 1}`, 700); };
  newRound();
  const light = (i, dur) => { litPad = i; litT = dur; pads.forEach((p, k) => p.classList.toggle('lit', k === i)); };
  const show = () => { ui.left.textContent = `Round ${Math.min(round + 1, ROUNDS.length)} / ${ROUNDS.length}`; ui.right.textContent = `Correct ${got} / ${totalPads}`; };
  show();
  let elapsed = 0;
  const next = () => { round++; if (round < ROUNDS.length) newRound(); else phase = 'end'; show(); };
  return {
    update(dt) {
      elapsed += dt;
      ui.time(1 - elapsed / LIMIT);
      if (litT > 0) { litT -= dt; if (litT <= 0) { litPad = -1; pads.forEach((p) => p.classList.remove('lit')); } }
      if (phase === 'end') return got / totalPads;
      if (elapsed >= LIMIT) { ui.say('Time!', 800); return got / totalPads; }
      if (phase === 'show') {
        pt += dt;
        const step = SHOW + GAP;
        const k = Math.floor(pt / step);
        if (pt >= 0 && k < seq.length && pt - k * step < SHOW && litPad !== seq[k]) { light(seq[k], SHOW - (pt - k * step)); sfx('tick'); }
        if (pt >= seq.length * step) { phase = 'input'; idx = 0; ui.say('Your turn', 600); }
      }
      return null;
    },
    input(i) {
      if (phase !== 'input') return;
      light(i, 0.2);
      if (seq[idx] === i) {
        got++; idx++; sfx('pick');
        if (idx >= seq.length) { ui.say('Correct!', 600); phase = 'wait'; setTimeout(next, 350); }
      } else { sfx('error'); ui.say('Oops', 600); phase = 'wait'; setTimeout(next, 450); }
      show();
    },
    key: (k) => (k >= '1' && k <= '4' ? Number(k) - 1 : undefined),
    click(e) { const p = e.target.closest('[data-pad]'); return p ? Number(p.dataset.pad) : undefined; },
    auto() { if (phase === 'input' && litT <= 0.05 && !this.cool) { this.cool = true; this.input(seq[idx]); setTimeout(() => { this.cool = false; }, 60); } },
  };
}

// ---------- stamina: Endurance Run ----------
function endurance(ui) {
  const LEN = 12, LO = 0.38, HI = 0.82;
  ui.stage.insertAdjacentHTML('beforeend', `<div class="dg-meter"><div class="z" style="bottom:${LO * 100}%;top:${(1 - HI) * 100}%"></div><div class="f"></div></div><div class="dg-runner">🏃</div>`);
  const fill = ui.stage.querySelector('.dg-meter .f'), runner = ui.stage.querySelector('.dg-runner');
  let v = 0.6, inZone = 0, t0 = 0;
  return {
    update(dt, t) {
      t0 = t;
      ui.time(1 - t / LEN);
      v -= dt * (0.28 + 0.03 * t); // drains faster as the run goes on
      v = clamp(v, 0, 1);
      const ok = v >= LO && v <= HI;
      if (ok) inZone += dt;
      fill.style.height = `${v * 100}%`;
      runner.style.transform = `translateY(${Math.sin(t * 14) * 6}px)`;
      runner.style.opacity = ok ? 1 : 0.5;
      ui.left.textContent = `In the band ${Math.round((100 * inZone) / Math.max(t, 0.01))}%`;
      ui.right.textContent = `${Math.max(0, LEN - t).toFixed(1)} s`;
      return t >= LEN ? inZone / LEN : null;
    },
    input() { v = clamp(v + 0.075, 0, 1); },
    key: (k) => (k === ' ' || k === 'Enter' ? 0 : undefined),
    click: () => 0,
    auto() { if (v < 0.58) this.input(); },
  };
}

DT.FarmGames = { run, GAMES, list: Object.keys(GAMES) };
