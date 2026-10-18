// Validate authored source schedules, then optionally audit the exact native PCM export.
// node native/tools/test-mystery-sounds.cjs [native-audio-directory] [--previews]
const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert/strict'),crypto=require('crypto');
const root=path.resolve(__dirname,'../..'),context={};vm.createContext(context);
vm.runInContext(fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8').replace(/export /g,''),context);
const definitions=vm.runInContext('[...sounds("sound"),...sounds("elimsound")]',context),signatures=new Set(),rhythms=new Set(),timbres=new Set();let checks=0;
function check(pass,why){checks++;assert.ok(pass,why);}
for(const clip of definitions){
 const trace=[],out={},E={tone:(at,p)=>trace.push({at,...p,noise:false}),noise:(at,p)=>trace.push({at,...p,noise:true})};clip.synth(0,out,E);
 check(clip.secret&&['daily','vault'].includes(clip.lootPool)&&clip.price===0&&!clip.egg,clip.id+' remains an exclusive discovery');
 check(trace.length>=8&&trace.length<=32,clip.id+' has a bounded authored voice schedule');
 check(clip.data.dur>0&&clip.data.dur<=(clip.data.stinger?2.4:1.9),clip.id+' has a compact complete duration');
 check(trace.every(e=>Object.values(e).filter(v=>typeof v==='number').every(Number.isFinite)),clip.id+' finite audio parameters');
 check(trace.every(e=>e.out===out&&!Object.hasOwn(e,'dur')),clip.id+' uses the requested board bus and supported envelope parameters');
 check(trace.every(e=>e.at>=0&&e.a>=.004&&e.dec>=.025&&e.vol>0&&e.vol<=.15&&e.f>=35&&e.f<=10000),clip.id+' restrained attacks, decays, gain and pitch');
 const end=Math.max(...trace.map(e=>e.at+e.a+e.dec+.05));
 check(clip.data.dur>=end+.075,clip.id+' metadata contains every release plus silence');
 const plain=trace.map(({out,...e})=>e),signature=JSON.stringify(plain);check(!signatures.has(signature),clip.id+' is not a duplicated clip');signatures.add(signature);
 rhythms.add(JSON.stringify([...new Set(trace.map(e=>+e.at.toFixed(4)))]));timbres.add(JSON.stringify(plain.slice(0,2).map(({type,a,dec,vol,f,f2,noise})=>({type,a,dec,vol,noise,ratio:f2?f2/f:1}))));
 let peakBound=0;
 for(let t=0;t<clip.data.dur;t+=.002){let amplitude=0;for(const e of trace){const age=t-e.at;if(age<0||age>e.a+e.dec)continue;amplitude+=age<e.a?.0001+(e.vol-.0001)*age/e.a:e.vol*Math.pow(.0001/e.vol,(age-e.a)/e.dec);}peakBound=Math.max(peakBound,amplitude);}
 check(peakBound<.55,clip.id+' summed envelope has headroom before bus gain');
}
check(definitions.length===80,'all40 Calls and40 Cadences present');check(rhythms.size>=50,'rhythmic variety across collectibles');check(timbres.size>=16,'timbre/envelope variety across collectibles');
// Exercise the production Collection preview adapter with a minimal DOM/audio
// boundary. PCM synthesis itself is rendered by export-audio, not mocked here.
const registered=[],clips=new Map();let unlocked=0,previewRequest=null,scheduled=0,stopped=0;
context.window={DT:{
 Cosmetics:{register:(_,items)=>registered.push(...items)},
 Sfx:{engine:{ensure:()=>unlocked++,tone:()=>scheduled++,noise:()=>scheduled++},defineClip:(id,fn,dur)=>clips.set(id,{fn,dur}),clip:(id,preview)=>{previewRequest={id,preview};clips.get(id).fn(0,{});}}
}};
context.document={createElement:tag=>({tag})};
const adapter=fs.readFileSync(path.join(root,'js/mystery-register.js'),'utf8');
vm.runInContext('const DT=window.DT,C=DT.Cosmetics;\n'+adapter.slice(adapter.indexOf("for(const kind of ['sound','elimsound'])")),context);
for(const item of registered){
 let button;item.preview({appendChild:b=>button=b});const beforeUnlock=unlocked,beforeScheduled=scheduled;
 check(button?.tag==='button'&&button.textContent==='▶ Play',item.id+' exposes the source Play preview control');
 button.onclick({stopPropagation:()=>stopped++});
 check(unlocked===beforeUnlock+1&&previewRequest.id===item.id&&previewRequest.preview===true&&scheduled>beforeScheduled,item.id+' Play unlocks audio and schedules its own preview');
 check(clips.get(item.id).dur===item.data.dur,item.id+' preview registers its complete authored duration');
}
check(registered.length===80&&stopped===80,'all80 preview controls stop card selection propagation');
if(process.argv[2]){
 const directory=path.resolve(process.argv[2]),manifest=JSON.parse(fs.readFileSync(path.join(directory,'manifest.json'),'utf8')),hashes=new Set(),report=[];
 for(const clip of definitions){
  const row=manifest.clips['clip_'+clip.id];check(!!row,clip.id+' native export exists');check(row.duration===clip.data.dur,clip.id+' native preview/duck duration matches source');
  const wav=fs.readFileSync(path.join(directory,row.file));check(wav.toString('ascii',0,4)==='RIFF'&&wav.readUInt32LE(24)===44100&&wav.readUInt16LE(22)===2,clip.id+' stereo native PCM format');
  const hash=crypto.createHash('sha256').update(wav).digest('hex');check(hash===row.sha256&&!hashes.has(hash),clip.id+' verified distinct PCM');hashes.add(hash);
  let peak=0,energy=0,tail=0,dc=0,frames=(wav.length-44)/4;
  for(let i=0;i<frames;i++)for(let channel=0;channel<2;channel++){const s=wav.readInt16LE(44+i*4+channel*2)/32768;peak=Math.max(peak,Math.abs(s));energy+=s*s;dc+=s;if(i>=frames-128)tail=Math.max(tail,Math.abs(s));}
  const rms=Math.sqrt(energy/(2*frames));check(peak>.008&&peak<.3&&rms>.001&&rms<.08&&row.clipped===0,clip.id+' audible restrained PCM without clipping');
  check(tail<.0001&&Math.abs(dc/(2*frames))<.0005,clip.id+' silent complete tail and no DC offset');
  check(frames/44100<=clip.data.dur,clip.id+' exported tail fits declared playback/duck duration');
  report.push({id:clip.id,name:clip.name,description:clip.desc,kind:clip.data.stinger?'Cadence':'Call',pool:clip.lootPool,file:row.file,seconds:frames/44100,duration:clip.data.dur,peak,rms,tail,sha256:hash});
 }
 if(process.argv.includes('--previews')){
 fs.writeFileSync(path.join(directory,'mystery-sound-quality.json'),JSON.stringify({checks,scope:'Automated schedule and PCM quality checks. No human listening review is recorded by this audit.',clips:report},null,2)+'\n');
 const html='<!doctype html><meta charset="utf-8"><title>Mystery sound previews</title><style>body{background:#081322;color:#edf3fa;font:17px system-ui;margin:28px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(310px,1fr));gap:14px}article{padding:16px;background:#14283b;border:1px solid #49627b}h2{font-size:19px}p{color:#c0d1e2}audio{width:100%}</style><h1>Mystery sound previews</h1><p>Development audit: the same PCM files used by the native client. This gallery is separate from the player collection and its discovery rules.</p><main>'+report.map(r=>`<article><h2>${r.name}</h2><p>${r.pool} · ${r.kind} · ${r.seconds.toFixed(2)}s · peak ${r.peak.toFixed(3)}</p><audio controls preload="none" src="${r.file}"></audio></article>`).join('')+'</main>';
 fs.writeFileSync(path.join(directory,'mystery-sound-previews.html'),html);
 }
}
console.log(`Mystery sounds passed ${checks} checks:80 exclusive clips, distinct authored schedules, rhythm/timbre variety, complete tails and bounded envelope headroom${process.argv[2]?', exact native PCM identity/duration/level/tails':''}${process.argv.includes('--previews')?' and isolated development preview gallery':''}.`);
