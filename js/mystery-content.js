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
// Distinct eight-beat phrases: steps, arm patterns, accents, tempo and turns vary per unlock.
export const dances = rewards('dance', s => ({
  name: `${s.name} ${['Two-Step','Waltz','Shuffle','Bounce','Wave'][s.i%5]}`,
  desc: `${s.label} choreography with ${s.i%2 ? 'alternating arm sweeps' : 'overhead accents'} and a ${s.i%3 ? 'side step' : 'quarter turn'}.`,
  bpm: 84 + s.i * 3 + s.p * 7, icon: '♫',
  pose(b) {
    const phase = b * Math.PI * (s.i%3===0 ? 2 : 1) + s.p*Math.PI/2, wave=Math.sin(phase), beat=Math.max(0, Math.sin(b*Math.PI)), bar=Math.floor(b)%8;
    const reach=.35+(s.i%5)*.1, swing=Math.sin(phase+s.i*.31), left=bar%2===0;
    return { aL:[reach,.25+swing*.5,.45],aR:[-reach,.25-swing*.5,.45],fL:[.2,left?.9:.1,.55],fR:[-.2,left?.1:.9,.55],
      lL:[.13+wave*.14,-1,.12*beat],lR:[-.13+wave*.14,-1,-.12*beat],
      hp:[0,wave*(4+s.i%4),wave*7],sp:[beat*5,0,-wave*5],hd:[-beat*8,wave*9,0],tl:wave*24,w:wave*.45,
      rt:{x:wave*(.025+s.i%4*.015),y:beat*(.025+s.i%5*.008),ry:s.i%3===0?Math.sin(b*Math.PI/4)*.6:wave*.12,z:Math.sin(b*Math.PI/2)*.025} };
  },
}));
export const killThemes = rewards('killscreen', s => ({
  name: `${s.name} Finale`, desc: `${s.label} rings, ${s.i%2 ? 'rising stars' : 'orbiting shards'} and a sweeping victory halo.`, icon: '✦',
  theme: { rim:s.color,text:'#ffffff',draw(c,w,h,t) {
    c.fillStyle=s.p?'#0e081f':'#081a24';c.fillRect(0,0,w,h); const m=Math.min(w,h),cx=w/2,cy=h*.42;
    c.save();c.translate(cx,cy);c.rotate(Math.sin(t*.3)*.2);
    for(let j=0;j<4+s.i%5;j++){c.strokeStyle=s.color;c.globalAlpha=.5-j*.045;c.lineWidth=2+j%3;
      c.beginPath();c.ellipse(0,0,m*(.12+j*.065),m*(.12+j*.065)*(s.i%2?.55:1),t*(j%2?-.2:.2),0,Math.PI*2);c.stroke();}
    for(let j=0;j<36;j++){const a=j*2.399+t*(.18+s.i*.015),r=m*(.1+((j/36+t*.07)%1)*.6),sz=2+(j+s.i)%6;
      c.globalAlpha=.3+(j%4)*.16;c.fillStyle=j%3?s.color:'#fff';c.save();c.translate(Math.cos(a)*r,Math.sin(a)*r);c.rotate(a);
      if(s.i%2){c.beginPath();c.moveTo(0,-sz*2);c.lineTo(sz,0);c.lineTo(0,sz*2);c.lineTo(-sz,0);c.closePath();c.fill();}else c.fillRect(-sz,-sz,sz*2,sz*2);c.restore();}
    c.restore();
  } },
}));
export const sounds = kind => rewards(kind, s => ({
  name:`${s.name} ${kind==='sound'?'Call':'Cadence'}`,icon:'♪',desc:`An original ${s.p?'electronic':'bell-like'} ${kind==='sound'?'call':'victory cadence'}.`,
  data:{clip:s.id,dur:kind==='sound'?1.5:2.2,stinger:kind==='elimsound'},
  synth(t,out,E){const notes=kind==='sound'?5:8,step=kind==='sound'?.19:.22;
    for(let j=0;j<notes;j++){const scale=[0,2,4,7,9],degree=(j*(s.i%4+1)+s.i)%5,base=(kind==='sound'?66:48)+s.p*5+(s.i%7);
      E.tone(t+j*step,{f:E.note(base+scale[degree]+(j===notes-1?12:0)),type:s.p?'triangle':'sine',dur:.28+(j===notes-1?.35:0),a:.012,dec:.3,vol:.09,out});}
  },
}));
