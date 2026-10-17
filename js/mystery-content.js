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
export const stickers = rewards('emote', s => ({
  name: `${s.name} ${s.p ? 'Signal' : 'Greeting'}`, desc: `${s.word} An animated ${s.label.toLowerCase()} sticker.`,
  data: { mood: 'any', role: ['happy','wave','taunt','dance'][s.i % 4] },
  art: () => {
    const n = 4 + s.i % 7, points = Array.from({length:n * 2}, (_, k) => { const a=k*Math.PI/n-Math.PI/2,r=k%2?38:53;return `${60+Math.cos(a)*r},${59+Math.sin(a)*r}`; }).join(' ');
    return `<g class="${['a-bob','a-pulse','a-wiggle','a-twinkle'][s.i%4]}"><polygon points="${points}" fill="${s.color}" stroke="#1b1424" stroke-width="4"/><circle cx="60" cy="50" r="${20+s.i%5}" fill="#17233b"/><path d="M43 49 l6 ${s.i%2?'-5':'5'} M71 49 l6 ${s.i%2?'5':'-5'}" stroke="white" stroke-width="4" fill="none"/><path d="M51 59 Q60 ${s.i%3?70:50} 69 59" stroke="white" stroke-width="3" fill="none"/><text x="60" y="91" text-anchor="middle" fill="#17233b" font-family="system-ui" font-weight="900" font-size="15">${s.word}</text></g>`;
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
