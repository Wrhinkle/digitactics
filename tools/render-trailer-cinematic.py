"""Music-led in-engine trailer. Inputs: ffmpeg.exe, job directory, supplied WAV.
Requires Pillow and numpy. Captures come from capture-trailer-cinematic.cjs.
Camera choreography is captured in-engine; this editor adds focused UI framing,
transparent kinetic type, restrained sound cues, and normalized audio.
"""
from pathlib import Path
import json, subprocess, sys, wave
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ffmpeg, job, music = sys.argv[1], Path(sys.argv[2]).resolve(), Path(sys.argv[3]).resolve()
root = Path(__file__).resolve().parent.parent
footage, edit = job / 'footage', job / 'edit'
edit.mkdir(exist_ok=True)
brief = json.loads((root / 'media/trailer-storyboard.json').read_text(encoding='utf-8'))
W, H, FPS = 1920, 1080, 30
CYAN, WHITE, INK = '#51e6ff', '#f0f8ff', '#061323'
FONT = 'C:/Windows/Fonts/arialbd.ttf'

def run(args):
    p = subprocess.run([ffmpeg, '-hide_banner', '-y', *map(str, args)], capture_output=True, creationflags=subprocess.CREATE_NO_WINDOW)
    if p.returncode:
        raise RuntimeError(p.stderr.decode(errors='replace'))
    return p.stderr.decode(errors='replace')

def overlay(shot):
    name = shot['id']
    im = Image.new('RGBA', (W, H))
    d = ImageDraw.Draw(im)
    def text(s, xy, size, fill=WHITE, center=False):
        f = ImageFont.truetype(FONT, size)
        x,y = xy
        if center: x -= d.textlength(s, font=f)/2
        d.text((x,y), s, font=f, fill=fill, stroke_width=2, stroke_fill=(6,19,35,140))
    if name in ('rookie','evolve','duos-orbit'):
        # A soft transparent scrim supports type without masking the game with bars.
        for y in range(660,H):
            d.line((0,y,W,y), fill=(6,19,35,int(180*((y-660)/(H-660))**1.5)))
        if name == 'rookie':
            text('DIGI-TACTICS', (96,76), 30, CYAN)
            text('BUILD YOUR', (96,775), 76)
            text('TEAM', (96,855), 112, CYAN)
        elif name == 'evolve':
            text('DIGIVOLVE', (96,820), 108, CYAN)
            text('AGUMON  >  GREYMON', (102,950), 30)
        else:
            text('FIGHT', (96,790), 78)
            text('TOGETHER', (96,873), 100, CYAN)
    elif name == 'buy':
        d.rounded_rectangle((88,88,355,190), radius=16, fill=(6,19,35,225))
        d.rounded_rectangle((88,88,96,190), radius=4, fill=CYAN)
        text('BUY', (120,99), 66)
    elif name in ('solo-ui','duos-ui'):
        label = 'SOLO' if name=='solo-ui' else 'DOUBLE UP'
        f=ImageFont.truetype(FONT,44);tw=d.textlength(label,font=f)
        x=(W-tw)/2
        d.rounded_rectangle((x-32,135,x+tw+32,217),radius=14,fill=(6,19,35,220))
        text(label,(W/2,148),44,CYAN,True)
    elif name == 'farm':
        d.rounded_rectangle((80,85,650,205), radius=18, fill=(6,19,35,225))
        text('CARE FOR YOUR DIGIMON', (108,100), 38, WHITE)
        text('DIGIFARM', (108,150), 29, CYAN)
    elif name == 'cta':
        text('GET ON GITHUB NOW',(W/2,975),60,WHITE,True)
        d.rounded_rectangle((540,1060,1380,1065),radius=2,fill=CYAN)
    target=edit/(name+'-type.png');im.save(target)
    return target

parts, captions, timeline = [], [], []
elapsed = 0.0
for shot in brief['shots'][:3] if '--preview' in sys.argv else brief['shots']:
    name,duration=shot['id'],shot['duration']
    target=edit/(name+'.mp4')
    if name=='cta':
        inp=['-loop','1','-framerate',FPS,'-i',root/'media/github-cover.png']
        # Preserve every letter of the 2:1 cover and reserve the lower band for the CTA.
        base='scale=1920:960,pad=1920:1080:0:0:color=0x061323,setsar=1'
    else:
        inp=['-framerate',FPS,'-i',footage/name/'%05d.png']
        base='setsar=1'
        if name=='buy':
            # Travel toward the shop/bench, retaining the gold counter at the left.
            z='1+0.32*(3*pow(min(on/27,1),2)-2*pow(min(on/27,1),3))'
            base+=f",zoompan=z='{z}':x=0:y='ih-ih/zoom':d=1:s=1920x1080:fps=30"
    if shot['text']:
        typ=overlay(shot)
        inp += ['-loop','1','-framerate',FPS,'-i',typ]
        graph=f"[0:v]{base}[bg];[1:v]format=rgba,fade=t=in:st=0.06:d=0.18:alpha=1[fg];[bg][fg]overlay=x=0:y='24*pow(max(0,1-t/0.30),3)':shortest=1"
        captions.append((elapsed,elapsed+duration,shot['text']))
    else:
        graph=f'[0:v]{base}'
    if name=='cta': graph+=f',fade=t=out:st={duration-.20}:d=0.20'
    graph+=',format=yuv420p[v]'
    if '--audio-only' not in sys.argv:
        run(['-loglevel','error',*inp,'-t',duration,'-filter_complex',graph,'-map','[v]','-an','-c:v','libx264','-preset','fast','-crf','18','-threads','4',target])
    elif not target.exists():
        raise FileNotFoundError(f'Audio-only mix requires rendered shot: {target}')
    timeline.append({**shot,'start':round(elapsed,3),'end':round(elapsed+duration,3),'path':str(target)})
    parts.append(target);elapsed+=duration
    print('Edited',name,flush=True)

if '--preview' in sys.argv:
    print('RESULT: '+json.dumps({'status':'succeeded','stage':'preview','artifacts':[str(p) for p in parts]}),flush=True)
    sys.exit(0)

# Quiet original editorial cues: purchase ticks, evolution chime, and short camera swishes.
# The supplied music remains the continuous soundtrack, without looping or reordering.
sr=48000
sfx=np.zeros((int(elapsed*sr),2),dtype=np.float64)
rng=np.random.default_rng(1701)
def cue(at,kind):
    dur=.24 if kind=='coin' else .55 if kind=='evolve' else .16
    t=np.arange(int(sr*dur))/sr
    if kind=='coin':
        v=(np.sin(2*np.pi*1200*t)+.3*np.sin(2*np.pi*1800*t))*np.exp(-t*25)*.055
    elif kind=='evolve':
        v=sum(np.sin(2*np.pi*f*t) for f in (523.25,659.25,783.99))*np.exp(-t*7)*np.minimum(1,t/.015)*.04
    else:
        noise=rng.normal(0,1,len(t));v=np.convolve(noise,np.ones(15)/15,mode='same')*np.sin(np.pi*t/dur)**2*.07
    start=int(sr*at);n=min(len(v),len(sfx)-start)
    if n>0:sfx[start:start+n]+=v[:n,None]
for at in (1.8+.5,1.8+50/30,1.8+91/30):cue(at,'coin')
cue(5.4,'evolve')
for shot in timeline[1:]:cue(shot['start'],'swish')
sound=edit/'editorial-cues.wav'
with wave.open(str(sound),'wb') as f:
    f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes((np.clip(sfx,-1,1)*32767).astype('<i2').tobytes())
concat=edit/'concat.txt';concat.write_text(''.join("file '"+p.as_posix()+"'\n" for p in parts),encoding='utf-8')
mix=edit/'mix.wav'
run(['-loglevel','error','-i',music,'-i',sound,'-filter_complex',f'[0:a]atrim=0:{elapsed},afade=t=out:st={elapsed-.8}:d=0.8[m];[m][1:a]amix=inputs=2:normalize=0[a]','-map','[a]','-ar',sr,'-c:a','pcm_f32le',mix])
report=run(['-i',mix,'-af','loudnorm=I=-16:TP=-1.5:LRA=11:print_format=json','-f','null','-'])
loud=json.loads(report[report.rfind('{'):report.rfind('}')+1])
normal=f"loudnorm=I=-16:TP=-1.5:LRA=11:measured_I={loud['input_i']}:measured_TP={loud['input_tp']}:measured_LRA={loud['input_lra']}:measured_thresh={loud['input_thresh']}:offset={loud['target_offset']}:linear=true"
master=job/'digi-tactics-trailer.mp4'
run(['-loglevel','error','-f','concat','-safe','0','-i',concat,'-i',mix,'-map','0:v','-map','1:a','-c:v','copy','-af',normal,'-c:a','aac','-b:a','256k','-ar',sr,'-movflags','+faststart','-t',elapsed,master])
def stamp(t):
    ms=round(t*1000);h,ms=divmod(ms,3600000);m,ms=divmod(ms,60000);s,ms=divmod(ms,1000)
    return f'{h:02}:{m:02}:{s:02},{ms:03}'
(job/'digi-tactics-trailer.srt').write_text('\n\n'.join(f'{i}\n{stamp(a)} --> {stamp(b)}\n{t}' for i,(a,b,t) in enumerate(captions,1))+'\n',encoding='utf-8')
(job/'edit.json').write_text(json.dumps({'duration':elapsed,'timeline':timeline,'music':music.name,'loudnessFirstPass':loud,'editorialCues':'Original locally synthesized ticks, chime and swishes','source':'Actual game pixels; camera/HUD capture changes only'},indent=2),encoding='utf-8')
print('RESULT: '+json.dumps({'status':'succeeded','stage':'assembly','artifacts':[str(master)],'metrics':{'duration':elapsed,'width':W,'height':H,'fps':FPS}}),flush=True)
