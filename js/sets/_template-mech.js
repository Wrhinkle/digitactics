// Template set hooks. Every hook is a function (c, e) or { order, fn }; see docs/sets/FORMAT.md for the contexts.
(function (G) {
  const DT = G.DT;

  DT.Sets.extend('_template', {
    traits: {
      // hook 1: basic attacks burn; more damage to burning enemies (engine stats: vsBurningAmp, burn helper)
      EMBER: {
        hooks: {
          onCombatStart(c) { for (const u of c.members) u.vsBurningAmp = c.v; },
          onAttack(c, e) { if (c.isMember(e.unit) && e.target.alive) c.burn(e.unit, e.target); },
        },
      },
      // hook 2: a starting shield
      TIDE: { hooks: { onCombatStart(c) { for (const u of c.members) u.shield += u.maxHp * c.v; } } },
      VAN: { hooks: { onCombatStart(c) { for (const u of c.members) { u.armor += c.v; u.mr += c.v; } } } },
      ARC: { hooks: { onCombatStart(c) { for (const u of c.mine) u.ap += c.v * (c.isMember(u) ? 2 : 1); } } },
    },
    items: {
      t_trophy: { hooks: { onKill(c, e) { c.buff(e.unit, 'ap', 20); } } },
    },
    augments: {
      t_cash: { hooks: { onPick(c) { c.player.gold += 10; } } },
      t_rally: { hooks: { onCombatStart(c) { for (const u of c.mine) u.as *= 1.15; } } },
      t_fortune: { hooks: { onIncome(c, e) { e.augment += 3; } } },
    },
    mechanic: {
      id: 'tribute',
      name: 'Tribute',
      desc: 'Every PvE round you win pays 1 extra gold.',
      hooks: {
        onRoundEnd(c, e) { if (e.pve && e.won) e.gold += 1; },
      },
    },
  });
})(typeof window !== 'undefined' ? window : globalThis);
