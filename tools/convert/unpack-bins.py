# Unzips every rips/<Name>.zip that has no rips/x/<Name>/ folder yet (run after unbundle.cjs).
import os, zipfile
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'rips')
n = 0
for f in sorted(os.listdir(base)):
    if not f.endswith('.zip'): continue
    out = os.path.join(base, 'x', f[:-4])
    if os.path.isdir(out): continue
    try:
        with zipfile.ZipFile(os.path.join(base, f)) as z: z.extractall(out)
        n += 1
    except Exception as e:
        print('fail', f, e)
print('unzipped', n)
