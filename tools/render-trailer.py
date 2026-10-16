"""Edit fixed-cadence gameplay frames into a 720p H.264 trailer.
Usage: python tools/render-trailer.py <ffmpeg.exe> <footage-dir> <music.wav>
Input is produced by capture-trailer.cjs. Music is supplied explicitly by the creator.
"""
from pathlib import Path
import json, subprocess, sys

ffmpeg, footage = sys.argv[1], Path(sys.argv[2]).resolve()
music = Path(sys.argv[3]).resolve()
root = Path(__file__).resolve().parent.parent
work = footage / 'edit'
work.mkdir(exist_ok=True)
font = 'C\\:/Windows/Fonts/arialbd.ttf'

def run(args):
    p = subprocess.run([ffmpeg, '-hide_banner', '-y', '-loglevel', 'error', *map(str, args)], capture_output=True, creationflags=subprocess.CREATE_NO_WINDOW)
    if p.returncode:
        raise RuntimeError(p.stderr.decode(errors='replace'))

# Each image is a distinct 1/30-second simulation sample, regardless of capture time.
shots = [
    ('title', None, 1, ''),
    ('solo-buy', 'solo-buy', 4, 'SOLO: BUY YOUR TEAM.'),
    ('solo-evolve', 'solo-evolve', 3, 'THREE COPIES. DIGIVOLVE.'),
    ('solo', 'solo-combat', 5, 'SOLO: LET THEM BATTLE.'),
    ('duos', 'duos-combat', 5, 'DOUBLE UP: BATTLE TOGETHER.'),
    ('final', 'final-attack', 2, 'MAKE THE LAST HIT COUNT.'),
    ('dance', 'victory-dance', 2, 'TAKE A VICTORY LAP.'),
    ('get-on-github', None, 3, 'GET ON GITHUB NOW'),
]
parts = []
captions = []
elapsed = 0
for name, start, duration, title in shots:
    target = work / (name + '.mp4')
    if name in ('title', 'outro'):
        inp = ['-loop', '1', '-framerate', '30', '-i', root / 'media/github-cover.png']
        vf = 'scale=1280:640,pad=1280:720:0:40:color=0x061323,setsar=1'
    elif name == 'get-on-github':
        inp = ['-f', 'lavfi', '-i', 'color=c=0x061323:s=1280x720:r=30']
        tf = work / 'get-on-github.txt'; tf.write_text(title, encoding='utf-8')
        textfile = tf.as_posix().replace(':', '\\:')
        vf = f"drawtext=fontfile='{font}':textfile='{textfile}':fontsize=70:fontcolor=0xe5f4ff:x=(w-tw)/2:y=(h-th)/2"
        captions.append((elapsed, elapsed + duration, title))
    else:
        inp = ['-framerate', '30', '-start_number', '0', '-i', footage / start / '%05d.png']
        vf = 'scale=1280:720,setsar=1'
        tf = work / (name + '.txt'); tf.write_text(title, encoding='utf-8')
        textfile = tf.as_posix().replace(':', '\\:')
        if name in ('solo-buy', 'solo-evolve'):
            # Keep the live shop, gold counter and bench visible during purchases.
            vf += f",drawbox=x=310:y=108:w=660:h=54:color=0x061323@0.94:t=fill,drawtext=fontfile='{font}':textfile='{textfile}':fontsize=30:fontcolor=white:x=(w-tw)/2:y=120"
        else:
            vf += f",drawbox=x=0:y=620:w=iw:h=100:color=0x061323@0.94:t=fill,drawbox=x=48:y=644:w=6:h=46:color=0x51e6ff:t=fill,drawtext=fontfile='{font}':textfile='{textfile}':fontsize=36:fontcolor=white:x=76:y=649"
        captions.append((elapsed, elapsed + duration, title))
    vf += f',fps=30,fade=t=in:st=0:d=0.12,fade=t=out:st={duration-0.15}:d=0.15,format=yuv420p'
    run([*inp, '-t', duration, '-vf', vf, '-an', '-c:v', 'libx264', '-preset', 'fast', '-crf', '19', '-threads', '4', target])
    parts.append(target); elapsed += duration
    print('Edited', name, flush=True)

concat=work/'concat.txt';concat.write_text(''.join("file '"+p.as_posix()+"'\n" for p in parts),encoding='utf-8')
run(['-f','concat','-safe','0','-i',concat,'-i',music,'-map','0:v','-map','1:a','-c:v','copy','-c:a','aac','-b:a','192k','-af',f'afade=t=in:st=0:d=0.1,afade=t=out:st={elapsed-1}:d=1','-ar','48000','-movflags','+faststart','-shortest',root/'media/digi-tactics-trailer.mp4'])
def stamp(t):
    ms=round(t*1000);h,ms=divmod(ms,3600000);m,ms=divmod(ms,60000);s,ms=divmod(ms,1000)
    return f'{h:02}:{m:02}:{s:02},{ms:03}'
(root/'media/digi-tactics-trailer.srt').write_text('\n\n'.join(f'{i}\n{stamp(a)} --> {stamp(b)}\n{t}' for i,(a,b,t) in enumerate(captions,1))+'\n',encoding='utf-8')
(work/'edit.json').write_text(json.dumps({'duration':elapsed,'shots':shots,'music':music.name,'musicCredit':"Tamer's Morning WIP - mikl_pls; supplied by William",'capture':'30 fps fixed-cadence PNG sequences'},indent=2),encoding='utf-8')
print('Trailer complete:',elapsed,'seconds',flush=True)
