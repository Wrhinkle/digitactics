// Template set art: item portraits (SVG painters with the js/itemart.js kit) and move looks (js/moves.js spec format).
// Items without art get a generated badge; abilities without a spec get an element look from their name.
(function (G) {
  const DT = G.DT;

  DT.Sets.extend('_template', {
    art: {
      // painter(p, kit): p is the picture builder, kit has frame, part helpers, shapes and colors (see FORMAT.md)
      items: {
        t_trophy: (p, k) => {
          k.frame(p, 'square', 'gold', ['#3a2a10'], { w: 8 });
          p.obj(() => p.at(64, 66, 1, 0, () => {
            p.part(k.star(0, 0, 34, 15), '#ffd84a');
            p.part(k.circ(0, 0, 9), '#fff3b0', { ow: 3 });
          }));
        },
      },
      moves: {
        'Wyrm Breath': { elem: 'fire', charge: 'mouth', release: 0.3, impact: 'blast', shake: 0.12, fx: [['breath', { dur: 0.6, width: 0.7 }]] },
      },
      basic: {
        t_ember3: { kind: 'shot', elem: 'fire', head: 'flame', trail: 'flame', size: 0.22 },
      },
    },
  });
})(typeof window !== 'undefined' ? window : globalThis);
