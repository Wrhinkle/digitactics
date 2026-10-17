import { rewards, sounds } from './mystery-content.js';
const DT=window.DT,C=DT.Cosmetics;
const companions = ['Agumon','Gabumon','Patamon','Gatomon','Gomamon','Palmon','Tentomon','Biyomon','Veemon','Wormmon','Hawkmon','Armadillomon','Guilmon','Renamon','Terriermon','Lopmon','Impmon','Dorumon','Ryudamon','Dracmon',
  'Kudamon','Gaomon','Falcomon','Lalamon','Goblimon','Gazimon','Elecmon','Bearmon','Monodramon','Kotemon','Hyokomon','Coronamon','Lunamon','FanBeemon','Keramon','Gotsumon','Otamamon','Syakomon','Crabmon','Penguinmon'];
C.register('legend', rewards('legend', s => {
  const name=companions[s.p*20+s.i];
  const found=Object.entries(DT.MODEL_SOURCES).find(([key,src])=>src.form?.toLowerCase()===name.toLowerCase());
  if(!found)throw Error(`Missing companion model: ${name}`);
  const [model,src]=found;
  return {name:`${name} Companion`,desc:`${name==='Gatomon'?'Champion':'Rookie'} Digimon. A ${s.label} companion for your board and Digi-Farm.`,
    data:{model,evoRoot:name.toLowerCase(),level:name==='Gatomon'?'Champion':'Rookie',scale:1,rotY:0,clips:{},credit:src},
    preview(el){el.innerHTML=DT.Legends.html(s.id,'lgprev','width:100%;height:100%;min-height:96px');} };
}));
for(const kind of ['sound','elimsound']) C.register(kind,sounds(kind).map(s=>{
  DT.Sfx.defineClip(s.id,(t,out)=>s.synth(t,out,DT.Sfx.engine),s.data.dur);
  return {...s,preview(el){const b=document.createElement('button');b.className='sb-prev';b.textContent='▶ Play';el.appendChild(b);b.onclick=e=>{e.stopPropagation();DT.Sfx.engine.ensure();DT.Sfx.clip(s.id,true);};}};
}));
