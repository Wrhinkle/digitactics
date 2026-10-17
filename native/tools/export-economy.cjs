const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const traces = [];
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const R = DT.HYPER;
  const game = Object.create(DT.Game.prototype);
  Object.assign(game, { R, fireGame() {} });
  const player = { gold: R.startGold ?? 3, level: R.xp ? R.xp.startLevel || 1 : R.levelByStage[1], xp: 0, streak: 0 };
  const snapshot = () => ({ ...player });
  const initial = snapshot(), steps = [], rounds = [];
  for (let stage = 1; stage <= 200; stage++) for (let round = 1; round <= R.roundsInStage(stage); round++)
    rounds.push({ stage, round, type: game.typeOf(stage, round) });
  for (let i = 0; i < 300; i++) {
    const stage = 1 + Math.floor(i / 7), round = 1 + i % 7;
    game.stage = stage; game.round = round;
    if (R.xp) game.addXp(player, R.xp.perRound ?? 2);
    else if (round === 1) player.level = R.levelByStage[Math.min(stage, R.levelByStage.length - 1)];
    const income = game.incomeBreakdown(player);
    player.gold += income.total;
    const bought = i % 3 === 0 ? game.buyXp(player) : null;
    const won = i % 17 < 8, pve = i % 7 === 6;
    if (won) player.gold += pve ? R.winGoldPvE || 0 : R.winGold;
    if (!pve) game.applyStreak(player, won);
    // Exercise interest thresholds, insufficient funds, and negative balances caused by hooks.
    const debit = i % 11 === 0 ? player.gold + 2 : i % 5 === 0 ? Math.min(20, player.gold) : 0;
    player.gold -= debit;
    steps.push({ stage, round, income, bought, won, pve, debit, expected: snapshot() });
  }
  traces.push({ variant: `${set.id}/${pace}`, initial, steps, rounds });
}
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/economy.json'), JSON.stringify(traces) + '\n');
console.log(`Exported ${traces.length} economy traces.`);
