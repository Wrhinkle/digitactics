// Original cosmetic content. Pool metadata stays separate from ordinary shop/egg items.
export const COLLECTIONS = [
  { pool: 'daily', label: 'Daybreak', names: ['Sunbeam','Dewdrop','Orchard','Firefly','Petal','Honeycomb','Raincloud','Seashell','Kite','Sprout','Lantern','Rainbow','Pebble','Snowflake','Feather','Waterlily','Starlight','Campfire','Windmill','Moonrise'], color: ['#ffc76b','#7de0cf','#ed91b9','#a9c7ff','#b8e986'], words: ['HELLO!','NICE!','GROW!','SHINE!','BLOOM!','SWEET!','OOPS!','WAVE!','WHEE!','GO!','CHEERS!','WOW!','SOLID!','CHILL!','FLY!','ZEN!','WISH!','COZY!','SPIN!','NIGHT!'] },
  { pool: 'vault', label: 'Cipher', names: ['Prism','Eclipse','Comet','Circuit','Obsidian','Nebula','Voltage','Portal','Meteor','Mirage','Quartz','Gravity','Signal','Nova','Chrome','Vortex','Pulsar','Phantom','Aurora','Singularity'], color: ['#cb9aff','#5ce5ff','#ff719e','#a3ffcb','#ffcc6e'], words: ['RARE!','DARK!','ZOOM!','LINK!','TOUGH!','DREAM!','ZAP!','WARP!','BOOM!','HUH?!','GLOW!','DROP!','BEEP!','BURST!','SLICK!','SWIRL!','PULSE!','BOO!','DAZZLE!','GG!'] },
];
export function rewards(kind, make) {
  return COLLECTIONS.flatMap((set, p) => Array.from({ length: kind === 'board' ? 5 : 20 }, (_, i) => {
    const id = `mystery_${set.pool}_${kind}_${i + 1}`;
    return { ...make({ ...set, p, i, id, color: set.color[i % 5], name: set.names[i], word: set.words[i] }), id,
      secret: true, lootPool: set.pool, egg: false, price: 0, rarity: i >= 16 ? 'legendary' : i >= 8 ? 'epic' : 'rare', tag: set.label };
  }));
}
// Individually drawn silhouettes remain readable in a small match speech bubble.
// No SVG ids: several copies of one sticker can safely share the same document.
function mysteryStickerArt(s) {
  const ink='#17233b', cream='#fff5d9', mint='#81ead0', pink='#ff93b9';
  const path=(d,fill=s.color,stroke=ink,width=3)=>`<path d="${d}" fill="${fill}" stroke="${stroke}" stroke-width="${width}" stroke-linecap="round" stroke-linejoin="round"/>`;
  const circle=(x,y,r,fill,stroke=ink)=>`<circle cx="${x}" cy="${y}" r="${r}" fill="${fill}" stroke="${stroke}" stroke-width="3"/>`;
  const line=d=>path(d,'none');
  const sparkle=(x,y,r=7)=>path(`M${x} ${y-r} Q${x+1} ${y-1} ${x+r} ${y} Q${x+1} ${y+1} ${x} ${y+r} Q${x-1} ${y+1} ${x-r} ${y} Q${x-1} ${y-1} ${x} ${y-r}`,cream,ink,2);
  const eyes=(x=60,y=53)=>circle(x-9,y,2.5,ink,'none')+circle(x+9,y,2.5,ink,'none')+path(`M${x-5} ${y+7} Q${x} ${y+12} ${x+5} ${y+7}`,'none',ink,2.5);
  let art='',motion='a-bob';
  if(!s.p) switch(s.i) {
    case 0: art=path('M60 13 L66 24 L79 19 L80 32 L94 35 L87 46 L96 57 L83 63 L84 77 L70 76 L60 87 L50 76 L36 77 L37 63 L24 57 L33 46 L26 35 L40 32 L41 19 L54 24', '#ffcf61')+circle(60,50,24,'#ffe699')+eyes();motion='a-pulse';break;
    case 1: art=path('M60 15 C50 31 34 43 34 58 A26 26 0 0 0 86 58 C86 43 70 31 60 15','#80dff0')+path('M45 48 Q39 59 46 66','none',cream,5)+eyes(61,55);break;
    case 2: art=path('M56 34 Q32 21 29 49 Q25 78 48 82 Q60 78 72 82 Q94 77 90 49 Q87 21 64 34','#ff8b8e')+line('M60 35 Q57 23 64 17')+path('M64 28 Q74 12 86 23 Q76 35 64 28',mint)+eyes(60,53);motion='a-swing';break;
    case 3: art=path('M56 44 Q28 22 26 46 Q29 62 52 59 M65 44 Q91 20 94 46 Q92 64 68 59',mint)+path('M48 51 Q60 40 72 51 L72 66 Q60 85 48 66 Z','#ffdc75')+path('M49 62 Q60 70 71 62','none',cream,5)+circle(60,39,13,ink)+circle(55,38,2,cream,'none')+circle(65,38,2,cream,'none')+line('M54 27 L48 20 M66 27 L72 20');motion='a-float';break;
    case 4: art=Array.from({length:5},(_,i)=>`<g transform="rotate(${i*72} 60 50)">${path('M52 39 C31 20 52 8 60 25 C68 8 89 20 68 39 Z',pink)}</g>`).join('')+circle(60,50,18,'#ffdf80')+eyes(60,48)+path('M59 70 L59 83 M59 78 Q37 63 36 77 Q43 89 59 83','none',mint,5);motion='a-swing';break;
    case 5: art=path('M60 19 L87 34 L87 65 L60 81 L33 65 L33 34 Z','#ffc85d')+path('M60 28 L78 39 L78 60 L60 71 L42 60 L42 39 Z','#ffe7a1')+eyes()+path('M80 67 L80 79 Q85 91 91 79 L91 63','#efaa36');break;
    case 6: art=path('M30 59 C13 54 23 33 38 37 C40 15 73 15 79 36 C100 29 107 59 89 62 Z','#b7d1f8')+eyes(60,43)+path('M35 72 L31 81 M58 73 L54 84 M81 72 L77 81','none','#67c9f5',5);motion='a-float';break;
    case 7: art=path('M32 71 C5 43 32 17 42 28 C49 10 64 11 68 24 C84 10 108 40 88 64 L72 79 L48 79 Z','#ffb4c3')+line('M60 73 L42 29 M60 73 L63 24 M60 73 L85 33 M60 73 L29 47 M60 73 L94 49')+circle(60,76,10,cream);motion='a-swing';break;
    case 8: art=path('M59 15 L86 43 L61 71 L33 43 Z','#ffbb64')+path('M59 15 L60 44 L33 43 Z',mint)+path('M60 44 L86 43 L61 71 Z',pink)+line('M61 71 Q42 80 65 85')+path('M59 77 L48 73 L49 84 Z',cream)+line('M59 15 L61 71 M33 43 L86 43');motion='a-swing';break;
    case 9: art=path('M60 67 Q31 64 30 34 Q60 28 60 67',mint)+path('M60 57 Q61 24 89 25 Q91 54 60 57','#b7ed82')+line('M60 80 L60 56 M60 65 L43 46 M60 57 L78 37')+path('M35 79 Q60 68 85 79 L81 87 L39 87 Z','#c58e6d');motion='a-breathe';break;
    case 10: art=line('M47 29 Q44 11 60 11 Q76 11 73 29')+path('M39 30 L81 30 L86 71 L73 82 L47 82 L34 71 Z','#ffc65e')+path('M47 36 L73 36 L77 69 L43 69 Z','#fff0b0')+path('M60 40 C45 55 56 66 62 64 C74 61 68 50 60 40','#ff9958')+line('M39 30 L81 30 M35 72 L85 72');motion='a-swing';break;
    case 11: art=path('M23 65 A37 37 0 0 1 97 65','none',pink,13)+path('M35 65 A25 25 0 0 1 85 65','none','#ffdc7d',11)+path('M45 65 A15 15 0 0 1 75 65','none',mint,9)+path('M17 73 Q8 59 24 55 Q30 44 39 58 Q54 57 48 73 Z',cream)+path('M73 73 Q65 59 79 55 Q87 46 94 59 Q109 60 103 73 Z',cream);break;
    case 12: art=path('M29 69 L34 43 L48 29 L74 31 L91 50 L87 74 L69 83 L43 80 Z','#abc3cf')+path('M35 45 L51 39 L72 41','none',cream,4)+eyes(60,55)+path('M45 77 Q58 72 75 77','none','#718d9e',3);motion='a-breathe';break;
    case 13: art=Array.from({length:6},(_,i)=>`<g transform="rotate(${i*60} 60 50)">${path('M60 50 L60 17 M60 29 L50 22 M60 29 L70 22 M60 40 L48 32 M60 40 L72 32','none','#9feaff',5)}</g>`).join('')+circle(60,50,9,cream);motion='a-spin-slow';break;
    case 14: art=path('M35 80 Q20 36 76 19 Q103 44 45 75 Z',cream)+path('M34 87 Q50 55 77 27','none',ink,3)+line('M49 62 L40 41 M58 51 L54 30 M67 40 L85 42 M43 72 L68 67');motion='a-float';break;
    case 15: art=path('M22 74 Q61 47 99 75 Q59 92 22 74',mint)+path('M60 70 Q31 55 31 39 Q55 38 60 70',pink)+path('M60 70 Q66 38 89 39 Q90 57 60 70',pink)+path('M60 71 Q40 50 60 24 Q80 50 60 71','#ffd1e6');motion='a-breathe';break;
    case 16: art=path('M60 17 L70 38 L93 41 L76 58 L80 82 L60 71 L39 82 L43 58 L26 41 L49 38','#ffdf82')+eyes(60,49)+sparkle(26,22,6)+sparkle(94,73,6);motion='a-pulse';break;
    case 17: art=path('M31 78 L85 65 M35 65 L88 79','none','#aa745c',10)+path('M37 65 Q24 49 48 32 Q49 47 59 42 Q70 30 65 15 Q100 52 79 67 Q58 83 37 65','#ff9758')+path('M49 65 Q42 54 60 42 Q58 54 70 59 Q71 74 49 65','#ffe496');motion='a-breathe';break;
    case 18: art=path('M51 50 L70 50 L78 85 L43 85 Z',cream)+`<g class="a a-spin-slow">${Array.from({length:4},(_,i)=>`<g transform="rotate(${i*90} 60 44)">${path('M60 44 L65 14 L79 17 L69 44 Z',i%2?mint:pink)}</g>`).join('')}</g>`+circle(60,44,6,'#ffc975')+path('M56 85 L56 72 Q62 66 67 72 L67 85',ink);motion='a-breathe';break;
    case 19: art=path('M77 19 A34 34 0 1 0 92 67 A30 30 0 0 1 77 19','#ffe5a1')+path('M40 53 Q44 59 49 53 M40 65 Q45 69 51 65','none',ink,3)+sparkle(83,36,9)+sparkle(95,18,5);motion='a-float';break;
  }
  else switch(s.i) {
    case 0: art=path('M60 15 L88 43 L60 82 L32 43 Z','#b495ef')+path('M60 15 L60 82 L32 43 Z','#81ddf4')+path('M32 43 L88 43 M60 15 L46 43 L60 82 L74 43 Z','none',cream,2);motion='a-pulse';break;
    case 1: art=circle(60,49,32,'#ffd978')+circle(70,42,28,'#292443')+path('M28 56 Q38 83 67 80','none',cream,4)+sparkle(88,23,5);motion='a-float';break;
    case 2: art=path('M30 73 L78 19 L74 43 L100 31 L74 66 Z','#a58aee')+path('M40 68 L76 36 L66 61 Z',mint)+circle(39,68,17,'#ffcc7f')+sparkle(38,67,9);motion='a-float';break;
    case 3: art=path('M38 28 L82 28 L82 72 L38 72 Z','#7ae1ce')+path('M48 38 L72 38 L72 62 L48 62 Z',ink)+line('M26 18 L26 41 L38 41 M16 61 L38 61 M60 13 L60 28 M82 40 L96 40 L96 21 M82 60 L101 60 M60 72 L60 87')+circle(26,18,4,cream)+circle(96,21,4,cream)+circle(60,87,4,cream)+sparkle(60,50,7);motion='a-pulse';break;
    case 4: art=path('M34 73 L29 47 L51 19 L75 26 L91 55 L81 79 L55 84 Z','#494360')+path('M51 19 L57 57 L29 47 M57 57 L81 79 M57 57 L75 26','none','#a78cdc',3)+path('M43 40 L47 33','none',cream,4);motion='a-breathe';break;
    case 5: art=path('M26 60 Q15 36 40 36 Q44 11 66 28 Q91 18 90 45 Q110 66 83 75 Q61 92 47 76 Q24 86 26 60','#b496ea')+path('M37 58 Q56 32 82 51 Q70 70 48 64 Q42 52 64 47','none',pink,7)+sparkle(68,49,9)+sparkle(31,28,5);motion='a-breathe';break;
    case 6: art=path('M64 14 L31 55 L53 55 L44 85 L88 39 L65 39 L75 14 Z','#ffe083')+path('M21 39 L15 33 M89 73 L100 77 M91 20 L96 13','none',mint,4);motion='a-beat';break;
    case 7: art='<ellipse cx="60" cy="51" rx="30" ry="37" fill="#262444" stroke="#b496ff" stroke-width="7"/>'+path('M48 25 Q31 49 47 74 M75 28 Q89 51 74 73','none',mint,3)+path('M47 51 L65 51 L65 42 L79 56 L65 70 L65 61 L47 61 Z',cream);motion='a-pulse';break;
    case 8: art=path('M34 63 L47 20 L59 36 L77 14 L78 43 L99 35 L81 74 Z','#ff9a89')+path('M39 64 L56 40 L60 56 L79 44 L73 70 Z','#ffdd84')+path('M29 66 L38 51 L60 54 L76 70 L67 84 L44 87 Z','#776487')+circle(46,67,5,'#ba9bc0')+line('M59 78 L66 75');motion='a-swing';break;
    case 9: art=path('M24 45 Q60 13 96 45 Q60 78 24 45','#8be3e1')+circle(60,45,17,'#b49bea')+circle(60,45,7,ink)+path('M28 72 Q43 64 60 73 Q78 81 94 71 M38 83 Q60 76 81 84','none',s.color,4);motion='a-float';break;
    case 10: art=path('M47 76 L39 34 L55 16 L70 31 L70 78 Z','#c1a2f1')+path('M47 76 L25 61 L25 42 L42 47 L55 77 Z',mint)+path('M64 78 L75 42 L91 35 L96 60 L77 80 Z',pink)+line('M55 16 L55 77 M75 42 L78 70')+sparkle(81,20,6);motion='a-glow';break;
    case 11: art=circle(60,51,21,'#9886de')+'<ellipse cx="60" cy="51" rx="43" ry="15" transform="rotate(-25 60 51)" fill="none" stroke="#8be3e1" stroke-width="5"/>'+circle(94,33,6,cream)+path('M51 40 Q58 35 65 39','none',cream,3);motion='a-swing';break;
    case 12: art=path('M34 36 L86 36 L91 76 L29 76 Z','#a0e6db')+path('M42 44 L78 44 L78 64 L42 64 Z',ink)+path('M48 55 L53 50 L59 57 L65 48 L72 54','none',cream,3)+line('M60 36 L60 26')+circle(60,22,5,pink)+path('M43 18 Q32 23 39 31 M77 18 Q88 23 81 31','none',s.color,3);motion='a-pulse';break;
    case 13: art=path('M60 12 L68 37 L88 24 L79 45 L104 50 L80 59 L90 80 L68 69 L60 91 L52 69 L29 80 L40 58 L17 50 L41 44 L31 24 L52 37 Z','#ffb3ce')+circle(60,50,17,cream)+sparkle(60,50,10);motion='a-beat';break;
    case 14: art=path('M60 17 L87 33 L87 66 L60 82 L33 66 L33 33 Z','#c9dde9')+path('M33 33 L60 48 L87 33 M60 48 L60 82','none',ink,3)+path('M41 38 L41 60 L52 67 M67 30 L78 36','none',cream,4)+path('M68 57 L79 51 L79 62 L68 68 Z','#8196bb');motion='a-swing';break;
    case 15: art=path('M86 30 C44 -1 12 48 38 75 C64 99 103 68 85 42 C67 17 35 43 49 62 C61 78 82 57 67 47 C57 41 52 53 60 55','none','#b796ed',12)+path('M85 30 C54 8 24 39 35 62','none',mint,3);motion='a-spin-slow';break;
    case 16: art=path('M17 53 L33 53 L42 37 L53 72 L65 23 L77 61 L86 48 L104 48','none',s.color,7)+circle(65,23,5,cream)+path('M35 19 L41 24 M85 78 L92 82','none',mint,3);motion='a-pulse';break;
    case 17: art=path('M31 79 L31 44 C31 10 87 10 87 44 L91 81 L76 73 L65 84 L51 74 L39 84 Z','#d6c6fa')+path('M44 44 Q50 36 55 44 M66 44 Q72 36 77 44','none',ink,4)+path('M54 55 Q61 67 68 55',pink)+path('M37 38 Q38 27 48 25','none',cream,4);motion='a-float';break;
    case 18: art=path('M26 70 Q30 21 49 35 Q65 58 73 22 Q88 18 95 54 L85 76 Q76 45 67 69 Q47 88 41 53 Q35 51 34 77 Z','#a7eacb')+path('M33 68 Q37 33 49 46 Q63 65 76 35 Q83 28 88 57','none','#b79cef',6)+sparkle(28,23,5)+sparkle(94,82,5);motion='a-breathe';break;
    case 19: art='<ellipse cx="60" cy="51" rx="42" ry="17" transform="rotate(-20 60 51)" fill="none" stroke="#c6a3fa" stroke-width="8"/>'+circle(60,51,24,ink,cream)+path('M39 33 Q60 13 81 34 M40 71 Q61 87 80 69','none',s.color,4)+sparkle(18,27,5)+sparkle(99,78,5);motion='a-pulse';break;
  }
  return `<g><ellipse cx="60" cy="88" rx="31" ry="5" fill="${ink}" opacity=".18"/><g class="a ${motion}">${art}</g><path d="M20 91 Q60 85 100 91 L96 111 Q60 117 24 111 Z" fill="${ink}" stroke="${cream}" stroke-width="2"/><text x="60" y="106" text-anchor="middle" fill="${cream}" font-family="system-ui,sans-serif" font-weight="900" font-size="14" letter-spacing=".5">${s.word}</text></g>`;
}
export const stickers = rewards('emote', s => ({
  name: `${s.name} ${s.p ? 'Signal' : 'Greeting'}`, desc: `${s.word} An animated ${s.label.toLowerCase()} sticker.`,
  data: { mood: 'any', role: ['happy','wave','taunt','dance'][s.i % 4] },
  art: () => {
    return mysteryStickerArt(s);
  },
}));
// Distinct eight-beat phrases. The native client embeds this same authored data;
// only interpolation and the bounded travel primitives are implemented twice.
const danceRest = {
  aL:[.25,-.95,.12],aR:[-.25,-.95,.12],fL:[.12,-.9,.2],fR:[-.12,-.9,.2],
  hp:[0,0,0],sp:[0,0,0],hd:[0,0,0],tl:0,w:0,
};
const gesture = p => ({...danceRest,...p});
const mirrorGesture = p => {
  const q={...p};
  for(const [l,r] of [['aL','aR'],['fL','fR']]){q[l]=[-p[r][0],p[r][1],p[r][2]];q[r]=[-p[l][0],p[l][1],p[l][2]];}
  for(const k of ['hp','sp','hd'])q[k]=[p[k][0],-p[k][1],-p[k][2]];
  q.tl=-p.tl;return q;
};
const danceGestures = {
  rest:gesture({}),
  gather:gesture({aL:[-.2,-.45,.8],aR:[.2,-.45,.8],fL:[-.4,.25,.8],fR:[.4,.25,.8],sp:[7,0,0],hd:[12,0,0],w:-.3}),
  offer:gesture({aL:[.2,-.2,.95],aR:[-.2,-.2,.95],fL:[.2,.15,.95],fR:[-.2,.15,.95],sp:[-3,0,0],hd:[-5,0,0],w:.1}),
  open:gesture({aL:[.9,.2,.15],aR:[-.9,.2,.15],fL:[.8,.4,.1],fR:[-.8,.4,.1],sp:[-4,0,0],hd:[-8,0,0],w:.65}),
  crown:gesture({aL:[.5,.8,.15],aR:[-.5,.8,.15],fL:[.1,.95,.15],fR:[-.1,.95,.15],sp:[-6,0,0],hd:[-14,0,0],w:.8}),
  cross:gesture({aL:[-.4,.1,.8],aR:[.4,.1,.8],fL:[-.7,.15,.5],fR:[.7,.15,.5],hp:[0,-8,0],sp:[3,8,0],hd:[3,0,0],w:-.5}),
  pointL:gesture({aL:[.65,.65,.35],fL:[.65,.65,.35],aR:[-.6,-.65,.2],fR:[.3,-.55,.5],hp:[0,4,7],sp:[-3,-12,-5],hd:[-12,-18,-5],tl:18,w:.25}),
  leanL:gesture({aL:[.9,.25,.15],fL:[.85,.45,.1],aR:[-.35,-.65,.55],fR:[.2,.1,.9],hp:[0,0,10],sp:[2,6,-6],hd:[0,-12,-8],tl:20,w:.45}),
  pullL:gesture({aL:[.65,-.4,.4],fL:[-.5,.3,.7],aR:[.05,-.7,.6],fR:[.3,.2,.8],hp:[0,10,4],sp:[6,-8,0],hd:[5,-15,0],tl:14,w:-.25}),
  cupL:gesture({aL:[.4,-.4,.7],fL:[.1,.85,.4],aR:[-.3,-.5,.65],fR:[-.4,.3,.7],sp:[3,-10,-5],hd:[8,-20,-9],tl:12,w:-.1}),
  sweepL:gesture({aL:[.85,-.1,.45],fL:[.9,.15,.35],aR:[.4,-.25,.8],fR:[.65,.2,.7],hp:[0,-10,4],sp:[0,-12,-4],hd:[-3,-18,0],tl:22,w:.2}),
  cradle:gesture({aL:[-.1,-.4,.85],aR:[.1,-.4,.85],fL:[-.85,0,.4],fR:[.85,0,.4],sp:[4,0,0],hd:[12,0,0],w:-.35}),
  clap:gesture({aL:[-.3,.15,.85],aR:[.3,.15,.85],fL:[-.5,.15,.8],fR:[.5,.15,.8],hp:[0,0,0],sp:[-2,0,0],hd:[-6,0,0],w:-.1}),
  low:gesture({aL:[.6,-.7,.35],aR:[-.6,-.7,.35],fL:[.7,-.55,.35],fR:[-.7,-.55,.35],hp:[6,0,0],sp:[9,0,0],hd:[12,0,0],w:-.65}),
  guard:gesture({aL:[.5,-.45,.65],aR:[-.5,-.45,.65],fL:[.05,.9,.4],fR:[-.05,.9,.4],sp:[3,0,0],hd:[3,0,0],w:-.2}),
  angleL:gesture({aL:[.95,.05,.1],fL:[.05,.95,.1],aR:[-.5,-.65,.3],fR:[-.85,0,.35],hp:[0,-8,0],sp:[0,10,-4],hd:[0,-20,0],tl:12,w:.15}),
  traceL:gesture({aL:[.4,.05,.9],fL:[.85,.1,.5],aR:[-.45,-.6,.55],fR:[.05,.85,.45],sp:[2,-8,0],hd:[4,-16,0],tl:8,w:-.1}),
  box:gesture({aL:[.8,.05,.5],aR:[-.8,.05,.5],fL:[.05,.95,.1],fR:[-.05,.95,.1],hp:[0,0,0],sp:[0,0,0],hd:[0,0,0],w:.3}),
  lock:gesture({aL:[.4,-.65,.5],aR:[-.4,-.65,.5],fL:[-.65,.25,.6],fR:[.65,.25,.6],hp:[3,0,0],sp:[4,0,0],hd:[7,0,0],w:-.45}),
  shadeL:gesture({aL:[.65,.65,.35],fL:[-.75,.1,.5],aR:[-.8,-.35,.2],fR:[-.75,-.1,.4],hp:[0,10,0],sp:[0,-12,4],hd:[0,15,0],tl:-14,w:.1}),
};
for(const k of ['point','lean','pull','cup','sweep','angle','trace','shade'])danceGestures[k+'R']=mirrorGesture(danceGestures[k+'L']);
const phrase=(title,story,keys,route='side',accent='flow',hold=.12)=>({title,story,keys:keys.split(' '),route,accent,hold});
export const danceLibrary = {version:1,beats:8,gestures:danceGestures,phrases:[
  phrase('Salute','Gather the light, reach overhead, then greet both sides.','gather offer crown open pointL pointR open rest','side'),
  phrase('Ripple','Catch a drop in each palm and send a ripple outward.','rest cupL sweepL cupR sweepR offer gather rest','sway'),
  phrase('Harvest','Reach, pick, cradle the harvest, and offer it forward.','rest pointL pullL pointR pullR cradle offer rest','step'),
  phrase('Flicker','Answer tiny lights with alternating bright hand accents.','rest pointL rest pointR crown pointL pointR rest','zigzag','spark',.38),
  phrase('Bloom','A closed bud opens, rocks in the breeze, and closes softly.','gather cradle offer crown open leanL leanR gather','still'),
  phrase('Honey Step','Tap each side, stack the rhythm, and share the honey.','guard angleL pullL angleR pullR offer clap rest','step','bounce'),
  phrase('Raindance','Make a canopy, brush away the rain, and open to the sky.','rest crown sweepL sweepR crown open gather rest','sway'),
  phrase('Shell Sway','Listen to one shell, then the other, with a gentle sway.','cupL leanL open cupR leanR open offer rest','sway'),
  phrase('Kite Flight','Guide a kite up, across the wind, and safely home.','rest pointL leanL crown pointR leanR open rest','arc'),
  phrase('New Growth','Rise from a seed, stretch new leaves, and settle.','gather low offer crown open pointL pointR gather','still','rise'),
  phrase('Lantern Lift','Lift a lantern and carry its glow from side to side.','gather offer crown cupL cupR crown offer gather','side'),
  phrase('Rainbow Arc','Paint a broad arc overhead and open the colors.','pointL sweepL crown sweepR pointR open offer rest','arc'),
  phrase('Skipping Stone','Wind up and skim a stone from each side.','gather pullL sweepL rest pullR sweepR offer rest','step','bounce'),
  phrase('Crystal Waltz','Open, close, and turn like a drifting snowflake.','rest open crown open cross open leanR rest','orbit'),
  phrase('Feather Float','Float each arm on the breeze before a soft landing.','rest leanL open leanR open crown gather rest','arc','rise'),
  phrase('Waterlily','Unfurl across the water with slow mirrored reaches.','gather offer open leanL offer leanR open gather','sway','flow',.04),
  phrase('Star Wish','Hold a wish, find two stars, and release it upward.','gather cross crown pointL pointR crown offer gather','still','rise'),
  phrase('Fireside Clap','Warm the hands, clap, and welcome both sides.','rest cradle clap cradle pointL pointR clap rest','step','bounce',.28),
  phrase('Breeze Turn','Trade broad arm sails while making a bounded pivot.','rest pointL open pointR cross pointL pointR rest','orbit'),
  phrase('Moon Lullaby','Cradle and sway before lifting a quiet moon.','gather cradle leanL cradle leanR offer crown gather','sway','flow',.04),
  phrase('Facet Lock','Present alternating facets and lock a bright diamond.','guard angleL angleR crown box angleR angleL lock','zigzag','pulse',.42),
  phrase('Eclipse','Cover, cross, and uncover the light in a measured orbit.','open shadeL cross shadeR open cross crown guard','orbit','flow',.25),
  phrase('Comet Trail','Wind up, launch across the body, and follow each trail.','guard pullL sweepL leanL pullR sweepR leanR guard','arc','spark',.3),
  phrase('Circuit Trace','Trace two circuit branches and close the connection.','guard traceL angleL traceR angleR box lock guard','step','pulse',.48),
  phrase('Obsidian Guard','Set two strong blocks, rise, and hold a firm finish.','guard angleL lock angleR lock crown lock guard','still','pulse',.55),
  phrase('Nebula Drift','Gather a cloud, unfurl it, and let it orbit the body.','gather sweepL open sweepR cradle crown open gather','orbit','flow',.04),
  phrase('Voltage Pop','Send sharp alternating pulses through a held frame.','guard box angleL lock angleR box crown lock','zigzag','spark',.5),
  phrase('Portal Step','Frame a doorway, pass a hand through, and close it.','guard box traceL open traceR box cross lock','forward','pulse',.34),
  phrase('Meteor Strike','Lift, compress, and release two grounded impacts.','guard crown low lock crown low open guard','still','impact',.32),
  phrase('Mirage Echo','Repeat a feint on the opposite side, then reveal it.','rest shadeL leanR shadeR leanL cross open rest','side','flow',.3),
  phrase('Quartz Chime','Touch four bright angles before a centered chime.','guard pointL angleR pointR angleL clap crown guard','step','spark',.36),
  phrase('Gravity Well','Gather inward, rise, settle low, and release the pull.','open gather low offer crown low gather open','still','rise',.18),
  phrase('Signal Relay','Receive a signal, pass it across, and confirm the lock.','guard cupL traceL box cupR traceR clap lock','zigzag','pulse',.52),
  phrase('Nova Burst','Compress a small core, then open into a broad burst.','low gather lock crown open sweepL sweepR guard','still','impact',.2),
  phrase('Chrome Glide','Slide a polished frame left and right with clean stops.','guard box leanL angleL box leanR angleR guard','side','pulse',.4),
  phrase('Vortex Weave','Weave crossing arms through a controlled circular pivot.','guard sweepL cross sweepR crown sweepL sweepR guard','orbit','flow',.08),
  phrase('Pulsar','Alternate a tight core and a wide synchronized pulse.','gather open lock crown gather box lock open','still','pulse',.5),
  phrase('Phantom Veil','Draw a veil, peek from both sides, and vanish into rest.','rest shadeL cross cupR shadeR cross open rest','sway','flow',.34),
  phrase('Aurora Ribbon','Send a ribbon across three heights with a soft sweep.','gather sweepL pointL crown pointR sweepR open gather','arc','flow',.04),
  phrase('Singularity','Gather orbiting energy, suspend it, and resolve the core.','open sweepL gather sweepR low crown lock open','orbit','rise',.28),
]};
const danceEase=t=>t*t*t*(t*(t*6-15)+10);
export function mysteryDancePose(b,index) {
  const p=danceLibrary.phrases[index],cycle=((b%8)+8)%8,k=Math.floor(cycle),f=cycle-k;
  const t=danceEase(Math.max(0,Math.min(1,(f-p.hold)/(1-p.hold))));
  const a=danceGestures[p.keys[k]],z=danceGestures[p.keys[(k+1)%8]],pose={};
  for(const key of Object.keys(danceRest))pose[key]=Array.isArray(a[key])?a[key].map((v,i)=>v+(z[key][i]-v)*t):a[key]+(z[key]-a[key])*t;
  const step=Math.sin(Math.PI*b),lift=Math.sin(Math.PI*b)**2,turn=Math.sin(Math.PI*b/4),circle=Math.PI*b/4;
  pose.lL=[.12,-1,.18*step];pose.lR=[-.12,-1,-.18*step];
  pose.kL=[.05,-1,-.12*Math.max(0,step)];pose.kR=[-.05,-1,-.12*Math.max(0,-step)];
  let x=0,zp=0,yaw=0;
  if(p.route==='side'){x=.055*turn;yaw=.16*turn;}
  if(p.route==='sway'){x=.035*turn;yaw=.12*Math.sin(circle*2);}
  if(p.route==='step'){x=.045*Math.sin(Math.PI*b/2);zp=.018*turn;}
  if(p.route==='zigzag'){x=.04*Math.sin(Math.PI*b/2);yaw=.22*turn;}
  if(p.route==='forward'){zp=.045*(1-Math.cos(circle));yaw=.1*turn;}
  if(p.route==='arc'){x=.05*turn;zp=.025*(1-Math.cos(circle));yaw=.3*turn;}
  if(p.route==='orbit'){x=.035*turn;zp=.03*(1-Math.cos(circle));yaw=.5*turn;}
  const bob=p.accent==='spark'?lift**3*.035:p.accent==='pulse'?lift**2*.028:p.accent==='impact'?lift*.045:p.accent==='rise'?.035*(1-Math.cos(circle)):.018*lift;
  pose.rt={x,y:bob,z:zp,ry:yaw};pose.preserveScale=1;pose.familyAdapt=1;return pose;
}
export const dances = rewards('dance', s => ({
  name: `${s.name} ${danceLibrary.phrases[s.p*20+s.i].title}`,
  desc: danceLibrary.phrases[s.p*20+s.i].story,
  bpm: 84 + s.i * 3 + s.p * 7, icon: '♫',
  pose(b) {
    return mysteryDancePose(b,s.p*20+s.i);
  },
}));
export const killThemes = (() => {
 // This small Canvas vocabulary also generates the native implementation.
 // Scenes stay deterministic in t, bounded, and behind the lineup/caption layers.
 // BEGIN NATIVE MYSTERY FINALES
 function finaleOval(c,x,y,rx,ry,rot,col,alpha) {
  c.fillStyle=col;c.globalAlpha=alpha;c.beginPath();c.ellipse(x,y,rx,ry,rot,0,Math.PI*2);c.fill();
 }
 function finaleLine(c,points,col,alpha,width) {
  c.strokeStyle=col;c.globalAlpha=alpha;c.lineWidth=width;c.beginPath();c.moveTo(points[0],points[1]);
  for(let j=2;j<points.length;j+=2)c.lineTo(points[j],points[j+1]);c.stroke();
 }
 function finalePoly(c,points,col,alpha) {
  c.fillStyle=col;c.globalAlpha=alpha;c.beginPath();c.moveTo(points[0],points[1]);
  for(let j=2;j<points.length;j+=2)c.lineTo(points[j],points[j+1]);c.closePath();c.fill();
 }
 function finaleStar(c,x,y,r,rot,col,alpha) {
  c.fillStyle=col;c.globalAlpha=alpha;c.beginPath();
  for(let j=0;j<10;j++){
   const a=rot+j*Math.PI/5;const d=r*(j%2===0?1:.36);
   if(j===0)c.moveTo(x+Math.cos(a)*d,y+Math.sin(a)*d);else c.lineTo(x+Math.cos(a)*d,y+Math.sin(a)*d);
  }c.closePath();c.fill();
 }
 function finaleHex(c,x,y,r,col,alpha) {
  c.strokeStyle=col;c.globalAlpha=alpha;c.lineWidth=Math.max(1,r*.06);c.beginPath();
  for(let j=0;j<6;j++){const a=j*Math.PI/3;if(j===0)c.moveTo(x+Math.cos(a)*r,y+Math.sin(a)*r);else c.lineTo(x+Math.cos(a)*r,y+Math.sin(a)*r);}
  c.closePath();c.stroke();
 }
 function finaleWave(c,w,h,t,base,amp,col,alpha) {
  c.fillStyle=col;c.globalAlpha=alpha;c.beginPath();c.moveTo(0,h);
  for(let j=0;j<=24;j++){const x=j*w/24;c.lineTo(x,h*(base+Math.sin(j*.5+t)*amp));}
  c.lineTo(w,h);c.closePath();c.fill();
 }
 function finaleDraw(c,w,h,t,pool,index,color) {
  const m=Math.min(w,h);const q=pool*20+index;const bg=pool===0?'#081a24':'#0e081f';
  c.save();c.globalAlpha=1;c.fillStyle=bg;c.fillRect(0,0,w,h);
  switch(q){
   case 0:{ // Sunbeam: a rising sun and long, slow fan rays.
    const x=w*.19;const y=h*.29;
    for(let j=0;j<12;j++){const a=j*Math.PI/6+t*.06;finalePoly(c,[x+Math.cos(a-.035)*m*.12,y+Math.sin(a-.035)*m*.12,x+Math.cos(a)*m*.5,y+Math.sin(a)*m*.5,x+Math.cos(a+.035)*m*.12,y+Math.sin(a+.035)*m*.12],color,.17);}
    finaleOval(c,x,y,m*.105,m*.105,0,'#ffdb87',.85);finaleWave(c,w,h,t*.3,.84,.025,'#b98042',.23);break;
   }
   case 1:{ // Dewdrop: large dew beads suspended above reed silhouettes.
    for(let j=0;j<8;j++){const x=w*(j<4?.04+j*.047:.79+(j-4)*.047);const y=h*(.45+.08*Math.sin(j));
     finaleLine(c,[x,h,x+m*.025*Math.sin(t+j),y],color,.4,m*.007);
     finalePoly(c,[x,y-m*.08,x-m*.035,y,x,y+m*.035,x+m*.035,y],color,.65);
     finaleOval(c,x-m*.009,y,m*.007,m*.014,-.3,'#ffffff',.7);
    }break;
   }
   case 2:{ // Orchard: fruit-bearing boughs frame an open clearing.
    for(let j=0;j<2;j++){const x=j===0?w*.09:w*.91;const d=j===0?1:-1;
     finaleLine(c,[x-d*m*.09,h*.9,x,h*.38,x+d*m*.16,h*.12],'#bb9272',.7,m*.025);
     for(let k=0;k<5;k++){const y=h*(.18+k*.105);const xx=x+d*m*(.1+.045*Math.sin(k+t*.25));
      finaleLine(c,[x,y+h*.09,xx,y],'#bb9272',.6,m*.012);finaleOval(c,xx,y,m*.075,m*.025,-d*.55,'#83b76b',.65);finaleOval(c,xx,y+m*.027,m*.022,m*.025,0,color,.9);}
    }break;
   }
   case 3:{ // Firefly: drifting lights above a jagged meadow.
    for(let j=0;j<15;j++){const x=w*(.04+j*.066);const y=h*(.25+.45*((j*7%15)/15.0))+Math.sin(t*.8+j)*m*.025;
     finaleOval(c,x,y,m*.018,m*.018,0,color,.1);finaleOval(c,x,y,m*.004,m*.004,0,'#e6ffaf',.45+.35*Math.sin(t*2+j)*Math.sin(t*2+j));
     finalePoly(c,[x-m*.025,h,x-m*.014,h*(.78+.06*Math.sin(j)),x,h*.96,x+m*.02,h*.76,x+m*.022,h],'#447a64',.55);
    }break;
   }
   case 4:{ // Petal: two gently turning flower rosettes, loose petals below.
    for(let j=0;j<2;j++){const x=w*(j===0?.12:.88);const y=h*.34;
     for(let k=0;k<7;k++){const a=k*Math.PI*2/7+t*.12;finaleOval(c,x+Math.cos(a)*m*.06,y+Math.sin(a)*m*.06,m*.063,m*.026,a,'#efa9c7',.65);}
     finaleOval(c,x,y,m*.025,m*.025,0,'#ffe4ad',.9);
    }
    for(let j=0;j<6;j++)finaleOval(c,w*(.09+j*.165),h*(.79+.045*Math.sin(t+j)),m*.025,m*.009,t*.2+j,'#efa9c7',.45);break;
   }
   case 5:{ // Honeycomb: interlocking cells on both sides, honey at the foot.
    for(let j=0;j<2;j++)for(let k=0;k<3;k++)for(let n=0;n<4;n++){const x=j===0?w*.02+k*m*.09:w*.98-k*m*.09;const y=h*(.12+n*.17)+(k%2)*m*.085;finaleHex(c,x,y,m*.058,'#ffd06d',.3+.15*Math.sin(t+k+n)*Math.sin(t+k+n));}
    finaleWave(c,w,h,t*.25,.91,.012,'#bd7f31',.5);break;
   }
   case 6:{ // Raincloud: a scalloped cloud canopy with slanted rain at the edges.
    for(let j=0;j<9;j++)finaleOval(c,w*j/8,h*.035,m*.16,m*.08,0,'#7c9cba',.42);
    for(let j=0;j<20;j++){const x=w*(j<10?.02+(j%10)*.023:.77+(j%10)*.023);const y=h*((j*.139+t*.22)%1);finaleLine(c,[x,y,x-m*.012,y+m*.055],'#a8daef',.55,m*.004);}
    finaleWave(c,w,h,t,.92,.012,'#6190a5',.3);break;
   }
   case 7:{ // Seashell: scalloped fans sit in layered shoreline surf.
    finaleWave(c,w,h,t*.5,.82,.025,'#639fad',.25);finaleWave(c,w,h,t*.5+1,.9,.018,color,.24);
    for(let j=0;j<2;j++){const x=w*(j===0?.13:.87);const y=h*.72;
     for(let k=0;k<9;k++){const a=Math.PI*1.1+k*Math.PI*.1;finaleOval(c,x+Math.cos(a)*m*.065,y+Math.sin(a)*m*.065,m*.09,m*.018,a,'#eabfc9',.5);}
     finaleOval(c,x,y,m*.035,m*.02,0,'#f4d6bf',.8);
    }break;
   }
   case 8:{ // Kite: paper diamonds tethered by visibly bending strings.
    for(let j=0;j<3;j++){const x=w*(j===1?.86:.07+j*.06)+Math.sin(t*.6+j)*m*.025;const y=h*(.19+j*.09);
     finalePoly(c,[x,y-m*.07,x+m*.045,y,x,y+m*.09,x-m*.045,y],j===1?'#f9b175':color,.8);
     finaleLine(c,[x,y-m*.07,x,y+m*.09,x-m*.03,y+m*.18,x+m*.01,y+m*.29,x-m*.04,h*.9],'#d0dbe2',.5,m*.004);
     finaleLine(c,[x-m*.045,y,x+m*.045,y],'#ffffff',.5,m*.003);
    }break;
   }
   case 9:{ // Sprout: paired leaves climb from earth along the lower corners.
    for(let j=0;j<6;j++){const x=w*(j<3?.04+j*.065:.83+(j-3)*.065);const y=h*(.57+.07*(j%3));const sway=Math.sin(t*.5+j)*m*.015;
     finaleLine(c,[x,h,x+sway,y],'#84b969',.7,m*.008);
     finaleOval(c,x+sway-m*.032,y+m*.025,m*.05,m*.023,.55,'#b8e986',.7);finaleOval(c,x+sway+m*.035,y,m*.05,m*.022,-.55,'#68b787',.8);
    }finaleWave(c,w,h,t*.2,.96,.012,'#5e5940',.7);break;
   }
   case 10:{ // Lantern: hanging cages, warm panels, and tassels.
    for(let j=0;j<5;j++){const x=w*(.08+j*.21);const y=h*(.13+(j%2)*.04)+Math.sin(t+j)*m*.008;
     finaleLine(c,[x,0,x,y],'#cc9d6a',.65,m*.004);finaleOval(c,x,y+m*.04,m*.035,m*.047,0,'#ffbe68',.65);
     for(let k=-1;k<=1;k++)finaleLine(c,[x+k*m*.018,y,x+k*m*.018,y+m*.08],'#5f3441',.65,m*.004);
     finaleLine(c,[x-m*.025,y,x+m*.025,y],'#efb371',.9,m*.008);finaleLine(c,[x-m*.025,y+m*.08,x+m*.025,y+m*.08,x,y+m*.08,x,y+m*.115],'#efb371',.9,m*.006);
    }break;
   }
   case 11:{ // Rainbow: two multicolour arches hug the side edges.
    for(let j=0;j<2;j++)for(let k=0;k<5;k++){
     c.globalAlpha=.44+.04*Math.sin(t*.4+k);c.strokeStyle=k===0?'#ef91a5':k===1?'#f2c27c':k===2?'#b8db88':k===3?'#83c6df':'#b49bdf';c.lineWidth=m*.019;c.beginPath();c.ellipse(j===0?-m*.04:w+m*.04,h*.57,m*(.3+k*.026),m*(.42+k*.026),0,0,Math.PI*2);c.stroke();
    }break;
   }
   case 12:{ // Pebble: asymmetrical cairns and soft pool ripples.
    for(let j=0;j<2;j++){const x=w*(j===0?.13:.87);
     for(let k=0;k<4;k++)finaleOval(c,x+Math.sin(k)*m*.012,h*(.86-k*.065),m*(.085-k*.015),m*.036,-.12+k*.08,k%2===0?'#a7b6bd':'#778b9a',.85);
     c.globalAlpha=.23;c.strokeStyle=color;c.lineWidth=m*.004;c.beginPath();c.ellipse(x,h*.91,m*(.12+.01*Math.sin(t)),m*.025,0,0,Math.PI*2);c.stroke();
    }break;
   }
   case 13:{ // Snowflake: branching sixfold crystals, with sparse falling snow.
    for(let j=0;j<2;j++){const x=w*(j===0?.13:.87);const y=h*.29;
     for(let k=0;k<6;k++){const a=k*Math.PI/3+t*.05;const dx=Math.cos(a);const dy=Math.sin(a);
      finaleLine(c,[x,y,x+dx*m*.13,y+dy*m*.13],'#d3efff',.7,m*.005);
      for(let n=-1;n<=1;n+=2)finaleLine(c,[x+dx*m*.08+Math.cos(a+n*.8)*m*.035,y+dy*m*.08+Math.sin(a+n*.8)*m*.035,x+dx*m*.08,y+dy*m*.08],'#a4cfe8',.7,m*.004);
     }
    }for(let j=0;j<12;j++)finaleOval(c,w*(j+.5)/12,h*((j*.173+t*.06)%1),m*.003,m*.003,0,'#d3efff',.4);break;
   }
   case 14:{ // Feather: long quills with separated barbs, drifting in the breeze.
    for(let j=0;j<4;j++){const x=w*(j<2?.08+j*.09:.84+(j-2)*.09);const y=h*(.25+(j%2)*.2)+Math.sin(t*.5+j)*m*.03;
     finaleLine(c,[x-m*.03,y+m*.15,x+m*.04,y-m*.12],'#c6ddcc',.7,m*.004);
     for(let k=0;k<9;k++){const yy=y-m*.09+k*m*.025;const xx=x+.008*m-k*m*.004;const span=Math.sin((k+1)*Math.PI/11)*m*.045;
      finaleLine(c,[xx-span,yy+m*.026,xx,yy,xx+span,yy+m*.015],'#c6ddcc',.65,m*.007);}
    }break;
   }
   case 15:{ // Waterlily: floating pads and pointed blossoms on a quiet pond.
    finaleWave(c,w,h,t*.25,.77,.015,'#376d76',.35);
    for(let j=0;j<4;j++){const x=w*(j<2?.08+j*.1:.82+(j-2)*.1);const y=h*(.78+(j%2)*.08)+Math.sin(t*.4+j)*m*.006;
     finaleOval(c,x,y,m*.075,m*.024,0,'#74ab82',.7);
     for(let k=0;k<5;k++){const dx=(k-2)*m*.018;finalePoly(c,[x,y,x+dx-m*.02,y-m*.028,x+dx,y-m*(.065-.009*Math.abs(k-2)),x+dx+m*.02,y-m*.028],'#edbdde',.8);}
    }break;
   }
   case 16:{ // Starlight: two linked constellations with distinct bright stars.
    for(let j=0;j<2;j++){const x=w*(j===0?.05:.77);
     finaleLine(c,[x,h*.32,x+w*.06,h*.16,x+w*.12,h*.25,x+w*.17,h*.1,x+w*.18,h*.42],color,.35,m*.003);
     for(let k=0;k<5;k++){const xx=x+w*(k===0?0:k===1?.06:k===2?.12:k===3?.17:.18);const yy=h*(k===0?.32:k===1?.16:k===2?.25:k===3?.1:.42);finaleStar(c,xx,yy,m*(.014+.004*Math.sin(t+k)*Math.sin(t+k)),-Math.PI/2,'#f1e4ff',.85);}
    }break;
   }
   case 17:{ // Campfire: crossed logs, layered flames and rising embers.
    for(let j=0;j<2;j++){const x=w*(j===0?.13:.87);const y=h*.84;
     finaleLine(c,[x-m*.07,y+m*.025,x+m*.07,y-m*.005],'#ad7751',.9,m*.025);finaleLine(c,[x-m*.07,y-m*.005,x+m*.07,y+m*.025],'#7b5745',.9,m*.025);
     finalePoly(c,[x-m*.055,y,x-m*.035,y-m*.07,x-m*.01,y-m*.035,x+Math.sin(t*3)*m*.015,y-m*.16,x+m*.045,y-m*.07,x+m*.055,y],'#ed864d',.85);
     finalePoly(c,[x-m*.025,y,x,y-m*(.085+.01*Math.sin(t*4)),x+m*.028,y],'#ffcf78',.9);
     for(let k=0;k<4;k++)finaleOval(c,x+Math.sin(k+t)*m*.035,y-m*(.16+((k*.13+t*.08)%.25)),m*.003,m*.006,0,'#ffcc7d',.6);
    }break;
   }
   case 18:{ // Windmill: turning four-sail silhouettes on a rolling skyline.
    finaleWave(c,w,h,t*.2,.88,.035,'#4c7773',.4);
    for(let j=0;j<2;j++){const x=w*(j===0?.12:.88);const y=h*.45;
     finalePoly(c,[x-m*.055,h*.9,x-m*.025,y,x+m*.025,y,x+m*.055,h*.9],'#b1b9b2',.55);
     for(let k=0;k<4;k++){const a=t*.45+k*Math.PI/2;finalePoly(c,[x,y,x+Math.cos(a)*m*.17,y+Math.sin(a)*m*.17,x+Math.cos(a+.2)*m*.17,y+Math.sin(a+.2)*m*.17,x+Math.cos(a+.5)*m*.03,y+Math.sin(a+.5)*m*.03],'#ded7bb',.8);}
     finaleOval(c,x,y,m*.016,m*.016,0,color,1);
    }break;
   }
   case 19:{ // Moonrise: a crescent over broken silver water reflections.
    finaleOval(c,w*.83,h*.24,m*.105,m*.105,0,'#d6e0c8',.8);finaleOval(c,w*.86,h*.21,m*.095,m*.095,0,bg,1);
    for(let j=0;j<9;j++){const y=h*(.58+j*.04);const x=w*.83+Math.sin(j+t*.4)*m*.018;finaleLine(c,[x-m*(.025+j*.009),y,x+m*(.025+j*.009),y],'#a9cbc9',.18,m*.006);}
    finaleWave(c,w,h,t*.3,.91,.02,'#334e68',.6);break;
   }
   case 20:{ // Prism: translucent triangular facets split into a spectral fan.
    for(let j=0;j<5;j++)finalePoly(c,[w*.12,h*.32,w*.02,h*(.62+j*.06),w*.48,h*(.62+j*.06)],j===0?'#ef8ea9':j===1?'#f8cf87':j===2?'#a3dfae':j===3?'#8fcce9':'#bfa1ed',.14);
    finalePoly(c,[w*.12,h*.13,w*.04,h*.48,w*.23,h*.48],color,.4);finaleLine(c,[w*.12,h*.13,w*.04,h*.48,w*.23,h*.48,w*.12,h*.13,w*.14,h*.4,w*.04,h*.48],'#eadcff',.7,m*.004);
    finalePoly(c,[w*.83,h*.2,w*.74,h*.55,w*.96,h*.43],'#80c7e2',.27+.04*Math.sin(t*.7));break;
   }
   case 21:{ // Eclipse: an opaque disk eclipses a gently breathing corona.
    const x=w*.83;const y=h*.3;
    finaleOval(c,x,y,m*.165,m*.165,0,'#8acfe4',.08);finaleOval(c,x,y,m*(.133+.004*Math.sin(t*2)),m*(.133+.004*Math.sin(t*2)),0,'#b9e6f4',.65);finaleOval(c,x-m*.012,y-m*.008,m*.123,m*.123,0,bg,1);
    finaleLine(c,[w*.04,h*.8,w*.25,h*.73,w*.42,h*.8],'#6797ba',.24,m*.004);break;
   }
   case 22:{ // Comet: segmented tails and glowing comet heads.
    for(let j=0;j<3;j++){const x=w*(.08+j*.35)+Math.sin(t*.3+j)*m*.025;const y=h*(.16+(j%2)*.13);
     finalePoly(c,[x-m*.12,y-m*.12,x+m*.012,y-m*.005,x,y+m*.015],color,.22);finaleLine(c,[x-m*.16,y-m*.09,x-m*.075,y-m*.02,x,y],'#f4c0df',.55,m*.007);finaleOval(c,x,y,m*.014,m*.014,0,'#fff0f4',.9);
    }break;
   }
   case 23:{ // Circuit: right-angle traces terminate in lit square contact pads.
    for(let j=0;j<2;j++)for(let k=0;k<6;k++){const x=j===0?w*.025:w*.975;const d=j===0?1:-1;const y=h*(.12+k*.13);
     finaleLine(c,[x,y,x+d*w*.09,y,x+d*w*.09,y+h*.05,x+d*w*(.16+(k%2)*.035),y+h*.05],'#75d6c0',.5,m*.005);
     c.fillStyle=color;c.globalAlpha=.4+.4*Math.sin(t*2+k)*Math.sin(t*2+k);c.fillRect(x+d*w*(.16+(k%2)*.035)-m*.009,y+h*.05-m*.009,m*.018,m*.018);
    }break;
   }
   case 24:{ // Obsidian: angular black glass cliffs with fine hot seams.
    for(let j=0;j<2;j++)for(let k=0;k<4;k++){const d=j===0?1:-1;const x=j===0?w*.01+k*w*.045:w*.99-k*w*.045;const y=h*(.25+(k%3)*.12);
     finalePoly(c,[x-d*m*.06,h,x-d*m*.035,y+m*.14,x,y,x+d*m*.055,y+m*.1,x+d*m*.07,h],'#292234',.95);finaleLine(c,[x,y,x+d*m*.025,y+m*.17,x+d*m*.015,h],color,.32+.12*Math.sin(t*.6+k)*Math.sin(t*.6+k),m*.004);
    }break;
   }
   case 25:{ // Nebula: layered drifting ribbons above a small star field.
    for(let j=0;j<5;j++){
     c.fillStyle=j%2===0?'#aa7cce':'#688bc7';c.globalAlpha=.09;c.beginPath();
     for(let k=0;k<=24;k++){const x=k*w/24;const y=h*(.15+Math.sin(k*.23+t*.18+j*.4)*.075+j*.015);if(k===0)c.moveTo(x,y);else c.lineTo(x,y);}
     for(let k=24;k>=0;k--){const x=k*w/24;const y=h*(.21+Math.sin(k*.23+t*.18+j*.4)*.075+j*.015);c.lineTo(x,y);}c.closePath();c.fill();
    }
    for(let j=0;j<10;j++)finaleStar(c,w*(j+.5)/10,h*(.14+(j*7%10)*.027),m*.005,t*.08,color,.6);break;
   }
   case 26:{ // Voltage: paired zigzag lightning bolts with conductor bars.
    for(let j=0;j<2;j++){const x=w*(j===0?.12:.88);const d=j===0?1:-1;
     // Both mirrored concave outlines use clockwise winding for the native tessellator.
     if(j===0)finalePoly(c,[x+m*.075,h*.12,x+m*.015,h*.32,x+m*.085,h*.32,x-m*.025,h*.68,x+m*.01,h*.39,x-m*.055,h*.39,x,h*.12],'#e0f7ff',.55+.2*Math.sin(t*3)*Math.sin(t*3));
     else finalePoly(c,[x,h*.12,x-d*m*.055,h*.39,x+d*m*.01,h*.39,x-d*m*.025,h*.68,x+d*m*.085,h*.32,x+d*m*.015,h*.32,x+d*m*.075,h*.12],'#e0f7ff',.55+.2*Math.sin(t*3)*Math.sin(t*3));
     finaleLine(c,[x-m*.07,h*.78,x+m*.07,h*.78],color,.4,m*.008);
    }break;
   }
   case 27:{ // Portal: nested doorframes and rising side runes.
    for(let j=0;j<3;j++){const gap=w*(.27+j*.026);const top=h*(.1+j*.035);finaleLine(c,[w/2-gap,h*.82,w/2-gap,top,w/2+gap,top,w/2+gap,h*.82],color,.22+j*.13,m*.006);}
    for(let j=0;j<12;j++){const x=j<6?w*.19:w*.81;const y=h*(.2+((j%6)*.105+t*.08)%.61);finaleHex(c,x,y,m*.012,'#d5ffed',.55);}break;
   }
   case 28:{ // Meteor: scorched impact craters beneath fractured falling rocks.
    for(let j=0;j<3;j++){const x=w*(.12+j*.38);const y=h*(.17+(j%2)*.08)+Math.sin(t*.5+j)*m*.015;finaleLine(c,[x-m*.07,y-m*.1,x,y],color,.4,m*.014);finalePoly(c,[x-m*.02,y-m*.01,x+m*.01,y-m*.024,x+m*.03,y+m*.01,x,y+m*.028,x-m*.025,y+m*.015],'#c58767',.85);}
    for(let j=0;j<5;j++){const x=w*(.06+j*.22);finaleOval(c,x,h*.94,m*.07,m*.024,0,'#a35b57',.55);finaleOval(c,x,h*.937,m*.052,m*.015,0,bg,.9);}break;
   }
   case 29:{ // Mirage: stacked desert ridges and wavering horizon bands.
    for(let j=0;j<4;j++)finaleWave(c,w,h,t*.25+j,.65+j*.075,.018+j*.008,j%2===0?'#665589':'#b28b89',.2);
    for(let j=0;j<5;j++)finaleLine(c,[w*.04,h*(.28+j*.018),w*(.22+.02*Math.sin(t+j)),h*(.28+j*.018)],color,.18,m*.004);
    finaleOval(c,w*.83,h*.24,m*.068,m*.05,0,'#d8ae8e',.45);break;
   }
   case 30:{ // Quartz: two clusters of faceted pointed crystals.
    for(let j=0;j<2;j++)for(let k=0;k<4;k++){const x=w*(j===0?.06+k*.035:.83+k*.035);const y=h*(.42+(k%3)*.11);const base=h*.91;
     finalePoly(c,[x-m*.022,base,x-m*.022,y+m*.06,x,y,x+m*.028,y+m*.06,x+m*.028,base],color,.25+.09*(k%2)+.04*Math.sin(t+k));finaleLine(c,[x,y,x+m*.004,y+m*.07,x+m*.004,base,x-m*.022,y+m*.06],'#e7d9ff',.55,m*.003);
    }break;
   }
   case 31:{ // Gravity: a flattened gravity well with orbiting bodies at its edge.
    for(let j=0;j<7;j++){c.globalAlpha=.16+j*.025;c.strokeStyle=color;c.lineWidth=m*.003;c.beginPath();c.ellipse(w*.5,h*.84,w*(.12+j*.055),h*(.025+j*.014),0,0,Math.PI*2);c.stroke();}
    for(let j=0;j<3;j++){const a=t*.25+j*Math.PI*2/3;finaleOval(c,w*.5+Math.cos(a)*w*.4,h*.84+Math.sin(a)*h*.09,m*.018,m*.018,0,'#b9d4f0',.6);}break;
   }
   case 32:{ // Signal: animated bars and a broadcast tower silhouette.
    for(let j=0;j<20;j++){const x=w*(j<10?.015+j*.019:.81+(j-10)*.019);const bh=h*(.06+.2*Math.abs(Math.sin(t*1.5+j*.8)));c.fillStyle=color;c.globalAlpha=.6;c.fillRect(x,h*.78-bh,w*.009,bh);}
    finaleLine(c,[w*.47,h*.19,w*.5,h*.1,w*.53,h*.19,w*.47,h*.19],color,.4,m*.005);finaleOval(c,w*.5,h*.09,m*.009,m*.009,0,'#ffcedd',.85);break;
   }
   case 33:{ // Nova: a many-point starburst blooms in the upper left.
    const x=w*.15;const y=h*.26;
    for(let j=0;j<16;j++){const a=j*Math.PI/8+t*.08;const r=m*(j%2===0?.22:.14)*(1+.025*Math.sin(t*2));finalePoly(c,[x+Math.cos(a-.045)*m*.04,y+Math.sin(a-.045)*m*.04,x+Math.cos(a)*r,y+Math.sin(a)*r,x+Math.cos(a+.045)*m*.04,y+Math.sin(a+.045)*m*.04],color,.6);}
    finaleStar(c,x,y,m*.055,-Math.PI/2,'#f3fff4',.85);break;
   }
   case 34:{ // Chrome: brushed metal chevrons with narrow moving highlights.
    for(let j=0;j<2;j++)for(let k=0;k<5;k++){const x=j===0?w*.015:w*.985;const d=j===0?1:-1;const y=h*(.1+k*.15);
     finalePoly(c,[x,y,x+d*w*.12,y+h*.07,x+d*w*.12,y+h*.11,x,y+h*.04],'#b5c3d3',.28+.12*Math.sin(t*.7+k)*Math.sin(t*.7+k));finaleLine(c,[x,y,x+d*w*.12,y+h*.07],'#f1e6cd',.65,m*.003);
    }break;
   }
   case 35:{ // Vortex: offset curling spirals made of continuous strokes.
    for(let j=0;j<2;j++){c.strokeStyle=j===0?color:'#759ed4';c.globalAlpha=.55;c.lineWidth=m*.009;c.beginPath();
     for(let k=0;k<=64;k++){const a=k*.16+t*.3;const r=m*(.015+k*.0025);const x=w*(j===0?.12:.88)+Math.cos(a)*r;const y=h*.36+Math.sin(a)*r;if(k===0)c.moveTo(x,y);else c.lineTo(x,y);}c.stroke();
    }break;
   }
   case 36:{ // Pulsar: a neutron star throws two long thin beams.
    const x=w*.84;const y=h*.3;const a=-.8+Math.sin(t*.7)*.12;
    for(let j=0;j<2;j++){const d=j===0?1:-1;finalePoly(c,[x-Math.sin(a)*m*.012,y+Math.cos(a)*m*.012,x+d*Math.cos(a)*m*.45,y+d*Math.sin(a)*m*.45,x+Math.sin(a)*m*.012,y-Math.cos(a)*m*.012],'#98e2ff',.3);}
    finaleOval(c,x,y,m*.025,m*.025,0,'#e8faff',.95);c.strokeStyle=color;c.globalAlpha=.55;c.lineWidth=m*.004;c.beginPath();c.ellipse(x,y,m*.11,m*.045,a+Math.PI/2,0,Math.PI*2);c.stroke();break;
   }
   case 37:{ // Phantom: floating spectral shrouds with cutout eyes.
    for(let j=0;j<2;j++){const x=w*(j===0?.12:.88);const y=h*.32+Math.sin(t*.8+j)*m*.025;
     finaleOval(c,x,y,m*.075,m*.085,0,'#b7a6df',.42);finalePoly(c,[x-m*.075,y,x+m*.075,y,x+m*.085,y+m*.15,x+m*.04,y+m*.12,x,y+m*.16,x-m*.04,y+m*.12,x-m*.085,y+m*.15],'#b7a6df',.42);
     finaleOval(c,x-m*.025,y,m*.009,m*.017,0,bg,1);finaleOval(c,x+m*.025,y,m*.009,m*.017,0,bg,1);
    }break;
   }
   case 38:{ // Aurora: tall coloured curtains curl across the upper sky.
    for(let j=0;j<28;j++){const x=w*j/27;const y=h*(.13+Math.sin(j*.19+t*.3)*.065);const len=h*(.12+.05*Math.sin(j*.4+t*.5));finaleLine(c,[x,y,x+w*.012,y+len],j%3===0?'#85e4c1':j%3===1?'#9ba9ef':'#dc9ce5',.2,m*.025);}
    finalePoly(c,[0,h,w*.08,h*.82,w*.18,h*.93,w*.3,h*.86,w*.45,h,w*.69,h*.9,w*.85,h*.78,w,h*.95,w,h],'#20283d',.9);break;
   }
   case 39:{ // Singularity: a black center distorts a bright accretion disk.
    const x=w*.83;const y=h*.34;const r=m*(.09+.002*Math.sin(t));
    finaleOval(c,x,y,m*.19,m*.046,-.22,'#e3b775',.55);finaleOval(c,x,y,r*1.13,r*1.13,0,'#dba0d2',.45);finaleOval(c,x,y,r,r,0,'#030208',1);
    c.strokeStyle='#f4d1a0';c.globalAlpha=.75;c.lineWidth=m*.005;c.beginPath();c.ellipse(x,y,m*.19,m*.046,-.22,0,Math.PI);c.stroke();
    for(let j=0;j<4;j++)finaleStar(c,w*(.05+j*.055),h*(.18+j*.07),m*.005,t*.1,color,.55);break;
   }
  }
  // Preserve white caption contrast without changing layout or lifetime.
  c.globalAlpha=1;const shade=c.createLinearGradient(0,0,0,h);shade.addColorStop(0,'rgba(0,0,0,0.08)');shade.addColorStop(.55,'rgba(0,0,0,0)');shade.addColorStop(1,'rgba(0,0,0,0.68)');c.fillStyle=shade;c.fillRect(0,0,w,h);
  c.restore();
 }
 // END NATIVE MYSTERY FINALES
 const scenes=[
  'A rising sun fans warm rays over a quiet horizon.','Dew beads sparkle above tall reeds.','Fruit and leafy boughs frame an orchard clearing.','Fireflies drift above a jagged meadow.','Turning flower rosettes scatter loose petals.',
  'Interlocking honeycomb cells catch a soft golden pulse.','A cloud canopy sends slanted rain into rippling water.','Scalloped seashells rest in layered shoreline surf.','Paper kites sway above their winding strings.','Paired leaves rise from the earth along the corners.',
  'Warm hanging lanterns sway above their tassels.','Five-colour rainbow arches frame the celebration.','Balanced stone cairns rest beside quiet pool ripples.','Branching snow crystals turn in sparse falling snow.','Feather quills drift with delicate separated barbs.',
  'Pointed lily blossoms float on green pond pads.','Linked constellations shimmer around bright stars.','Layered campfire flames lift embers above crossed logs.','Four-sail windmills turn over a rolling skyline.','A crescent moon shines on broken silver reflections.',
  'Triangular crystal facets split a spectral fan.','A dark eclipse breathes inside a pale corona.','Bright comet heads trail across the upper sky.','Right-angle circuit traces feed pulsing contact pads.','Black glass cliffs reveal glowing angular seams.',
  'Layered nebula ribbons drift through a sparse star field.','Paired lightning bolts glow above conductor bars.','Nested portal doors frame rising geometric runes.','Fractured meteors descend above scorched impact craters.','Desert ridges shimmer beneath a wavering horizon.',
  'Pointed quartz clusters reveal their faceted edges.','Orbiting bodies circle a flattened gravity well.','Signal bars animate beneath a broadcast beacon.','A many-point nova blooms around a brilliant star.','Brushed chrome chevrons carry narrow moving highlights.',
  'Two curling vortex spirals turn at opposite edges.','A neutron star sweeps two narrow beams through space.','Floating phantom shrouds peer through dark eyes.','Colourful aurora curtains curl above a mountain skyline.','A black singularity bends its bright accretion disk.'
 ];
 return rewards('killscreen',s=>({name:`${s.name} Finale`,desc:scenes[s.p*20+s.i],icon:'✦',theme:{rim:s.color,text:'#ffffff',draw(c,w,h,t){finaleDraw(c,w,h,t,s.p,s.i,s.color);}}}));
})();
// Forty authored musical gestures, each with its own contour and rhythm. Calls
// are compact; cadences answer the gesture in a lower register and resolve it.
// Durations come from the scheduled envelopes, including the engine's release.
const mysterySoundPhrases = [
  ['bell',72,[0,4,7,12],[0,.7,1.8,3.2],'Bright bells climb into a warm sunlit answer.'],
  ['drop',76,[12,7,9,4,0],[0,.55,1.7,2.4,4.1],'Cool droplets tumble through a gently falling phrase.'],
  ['wood',67,[0,7,4,2,0],[0,1.1,1.65,3,4.25],'Hollow wooden plucks skip with an orchard swing.'],
  ['spark',79,[0,12,7,12,4],[0,.45,1.9,2.35,4],'Tiny bright flickers blink in a playful call and answer.'],
  ['flute',74,[7,5,4,0],[0,1.2,2.1,3.7],'A soft flute phrase floats down like falling petals.'],
  ['felt',64,[0,4,7,4,9,7],[0,.8,1.6,2.9,3.5,4.4],'Rounded mallets weave a sweet, gently syncopated pattern.'],
  ['drop',70,[7,3,0,5,2],[0,.65,1.2,2.9,4.5],'Low raindrops patter, pause, and answer with a softer drop.'],
  ['glass',77,[0,7,12,9],[0,1.3,2.2,4.2],'Clear shell-like chimes leave space for a ringing answer.'],
  ['pluck',69,[0,2,7,12,9],[0,.9,1.8,2.45,4.3],'A light plucked melody lifts and dips like a kite.'],
  ['wood',72,[0,2,4,7],[0,1.4,2.2,3.9],'Four tender wooden notes grow from a small beginning.'],
  ['felt',65,[0,7,4,7,12],[0,1.2,2.5,3.1,4.6],'A warm lantern motif rocks between low and high notes.'],
  ['chime',71,[0,2,4,7,9,12],[0,.6,1.35,2.2,3.1,4.2],'Six colored chimes arch upward with a bright finish.'],
  ['wood',60,[0,0,7,4,0],[0,.5,1.7,2.8,4.4],'Small rounded knocks skip twice before settling.'],
  ['glass',81,[12,7,0,4,9],[0,.9,1.9,3.4,4.1],'Fine icy chimes sparkle across a spacious pattern.'],
  ['flute',76,[12,9,7,5,7],[0,1,2.2,3.1,4.8],'An airy feather-light phrase drifts before turning upward.'],
  ['pad',68,[0,7,4,9],[0,1.5,2.8,4.4],'Soft layered tones open slowly over still water.'],
  ['chime',79,[0,12,7,16,12],[0,1.1,2,3.2,4.6],'Distant star chimes jump between luminous registers.'],
  ['felt',62,[0,3,7,5,3,0],[0,.7,1.8,2.6,3.3,4.7],'A low, cozy mallet phrase curls back toward home.'],
  ['pluck',67,[0,7,12,7,2,9],[0,.8,1.6,2.4,3.6,4.4],'A turning plucked figure gathers a small gust of momentum.'],
  ['pad',66,[7,12,9,4,0],[0,1.3,2.5,3.6,5],'A mellow moonlit line rises, hangs, and settles softly.'],
  ['glass',75,[0,6,12,7,14],[0,.7,1.6,3,4.3],'Faceted glass tones refract into a bright angular answer.'],
  ['bass',53,[0,7,3,0],[0,1.4,2.6,4.2],'A shadowed low pulse opens into a quiet eclipse chord.'],
  ['warp',72,[0,7,12,19],[0,.55,1.45,3.3],'Smooth rising glides leave a quick comet trail.'],
  ['signal',74,[0,0,7,4,12,7],[0,.45,1.4,2.2,3.55,4.3],'A clipped circuit handshake answers in two bright bursts.'],
  ['metal',58,[0,3,7,0,10],[0,1,2.2,3.6,4.5],'Dark metallic strikes ring with a restrained stone-like weight.'],
  ['pad',70,[0,7,10,14],[0,1.6,2.9,4.7],'A hazy layered chord drifts through a distant nebula.'],
  ['spark',77,[0,12,1,13,7],[0,.35,1.1,1.65,3.9],'Short electric chirps jump across a crisp syncopated rhythm.'],
  ['warp',65,[0,5,12,5,0],[0,.8,2.1,3.3,4.6],'An opening pitch sweep returns through a calm portal answer.'],
  ['bass',57,[12,7,3,0],[0,.6,1.7,3.8],'A descending meteor figure lands in a rounded low tone.'],
  ['shimmer',73,[0,1,7,8,12],[0,1.2,2.3,3.1,4.8],'Close shimmering notes shift into a clear mirage response.'],
  ['chime',78,[0,7,14,12,7],[0,.85,1.6,3.2,4.5],'Pure quartz chimes make a clean, crystalline arch.'],
  ['bass',50,[12,10,7,3,0],[0,.75,1.7,2.9,4.7],'A measured falling bass phrase gathers gravitational weight.'],
  ['signal',81,[0,0,7,0,12],[0,.4,1.7,2.6,4],'Two short beeps introduce a clear digital reply.'],
  ['spark',71,[0,3,7,10,15,19],[0,.5,1.15,1.9,2.8,4.2],'A quick rising burst opens into a bright nova flourish.'],
  ['metal',70,[0,7,6,12,7],[0,1.05,1.65,3.1,4.6],'Polished metallic taps catch a sharp chromatic glint.'],
  ['warp',68,[0,4,7,4,0,-5],[0,.7,1.4,2.2,3.3,4.8],'A curling glide pattern spirals down into a soft landing.'],
  ['pulse',62,[0,7,0,12,7,0],[0,.6,1.9,2.5,3.8,4.4],'A steady low pulse alternates with quick orbital answers.'],
  ['flute',69,[0,3,2,7,0],[0,1.45,2.25,3.6,5.1],'A breathy phantom phrase appears, bends, and fades away.'],
  ['shimmer',76,[0,4,9,7,12],[0,1.1,2.4,3.5,4.9],'Layered bright tones ripple upward like an aurora curtain.'],
  ['bass',55,[12,7,1,0,12],[0,1.25,2.2,3.8,4.8],'A low orbit contracts into a quiet singularity and releases.'],
];
const mysterySoundVoices = {
  bell:{type:'sine',a:.006,dec:.30,vol:.115,ratio:2.76,overtone:.18},
  drop:{type:'sine',a:.006,dec:.16,vol:.13,fall:.82,ratio:2,overtone:.12},
  wood:{type:'sine',a:.005,dec:.105,vol:.14,ratio:3.01,overtone:.24,noise:1500},
  spark:{type:'triangle',a:.006,dec:.11,vol:.085,fall:.93,ratio:2,overtone:.16,noise:4600},
  flute:{type:'sine',a:.035,dec:.28,vol:.11,ratio:2,overtone:.1,vib:[5,8,.1]},
  felt:{type:'sine',a:.012,dec:.25,vol:.13,ratio:2,overtone:.22},
  glass:{type:'sine',a:.007,dec:.33,vol:.105,ratio:3.76,overtone:.19},
  pluck:{type:'triangle',a:.006,dec:.20,vol:.095,ratio:2,overtone:.13},
  chime:{type:'sine',a:.008,dec:.38,vol:.105,ratio:4.01,overtone:.12},
  pad:{type:'triangle',a:.075,dec:.34,vol:.065,ratio:2.005,overtone:.28,vib:[3,5,.15]},
  bass:{type:'triangle',a:.012,dec:.25,vol:.105,fall:.97,ratio:2,overtone:.18},
  warp:{type:'sine',a:.018,dec:.26,vol:.105,fall:1.16,ratio:2.01,overtone:.17},
  signal:{type:'square',a:.006,dec:.10,vol:.045,ratio:1,overtone:1},
  metal:{type:'sine',a:.006,dec:.19,vol:.115,ratio:3.83,overtone:.21,noise:2400},
  shimmer:{type:'sine',a:.03,dec:.32,vol:.095,ratio:1.006,overtone:.48,vib:[4,6,.12]},
  pulse:{type:'square',a:.008,dec:.12,vol:.042,ratio:1,overtone:1.2},
};
function mysterySoundEvents(s,kind) {
  const [voice,root,notes,beats]=mysterySoundPhrases[s.p*20+s.i],v=mysterySoundVoices[voice];
  const cadence=kind==='elimsound',step=cadence?.18:.16,base=root-(cadence?5:0),gain=cadence?.88:1,events=[];
  for(let j=0;j<notes.length;j++){
    const at=beats[j]*step,a=v.a,dec=v.dec*(j===notes.length-1?1.24:1),f=440*Math.pow(2,(base+notes[j]-69)/12),vol=v.vol*gain*(j===0?1:.87);
    events.push({at,f,type:v.type,a,dec,vol,f2:v.fall?f*v.fall:null,vib:v.vib||null});
    events.push({at:at+.003,f:f*v.ratio,type:'sine',a:a*.9,dec:dec*.65,vol:vol*v.overtone});
    if(v.noise)events.push({at,noise:true,type:'bandpass',f:v.noise,a:.006,dec:.035,vol:voice==='wood'?.009:.006,q:.7,rate:1});
  }
  if(cadence){
    const at=(beats[beats.length-1]+.85)*step;
    for(const [offset,vol] of [[-12,.045],[0,.04],[7,.026],[12,.018]])events.push({at,f:440*Math.pow(2,(base+offset-69)/12),type:'sine',a:.025,dec:.55,vol});
  }
  return events;
}
export const sounds = kind => rewards(kind, s => {
  const events=mysterySoundEvents(s,kind),duration=Math.ceil((Math.max(...events.map(e=>e.at+e.a+e.dec+.05))+.08)*100)/100;
  return {name:`${s.name} ${kind==='sound'?'Call':'Cadence'}`,icon:'♪',desc:mysterySoundPhrases[s.p*20+s.i][4]+(kind==='elimsound'?' A lower answering phrase resolves into a soft victory chord.':''),
    data:{clip:s.id,dur:duration,stinger:kind==='elimsound'},
    synth(t,out,E){for(const event of events){const {at,noise,...options}=event;(noise?E.noise:E.tone)(t+at,{...options,out});}},
  };
});
