"""Verify qualified runtime/source packaging and isolated cache failure behavior."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import zipfile

tools = Path(__file__).resolve().parent
artifacts = tools.parent / 'artifacts'
pin = json.loads((tools / 'media-runtime.json').read_text(encoding='utf-8-sig'))
checks = 0


def check(value, message):
    global checks
    if not value:
        raise AssertionError(message)
    checks += 1


def sha(data):
    return hashlib.sha256(data).hexdigest()


def package():
    subprocess.run([sys.executable, str(tools / 'prepare-media-audio.py')], check=True, capture_output=True)
    return next((artifacts / 'media-audio-materials' / pin['sha256']).glob('*.zip'))


bundle = package()
first = bundle.read_bytes()
check(package().read_bytes() == first, 'Repeated source packaging differs')
with zipfile.ZipFile(bundle) as archive:
    manifest = json.loads(archive.read('materials-manifest.json'))
    check(manifest['runtimeSha256'] == pin['sha256'], 'Source/runtime binding')
    check(manifest['distributionReady'] and not manifest['remainingRequirements'], 'Native source closure')
    check(len(archive.namelist()) == len(set(archive.namelist())), 'Duplicate entries')
    check(set(archive.namelist()) == {x['path'] for x in manifest['files']} | {'materials-manifest.json'}, 'Unlisted source entries')
    for entry in manifest['files']:
        data = archive.read(entry['path'])
        check(len(data) == entry['size'] and sha(data) == entry['sha256'], 'Source integrity: ' + entry['path'])
    check(archive.read('changes.diff') == b'', 'Unexpected upstream patch')

test_parent = artifacts / 'media-audio-tests'
test_parent.mkdir(exist_ok=True)
root = Path(tempfile.mkdtemp(prefix='cache-', dir=test_parent))
isolated_tools = root / 'native/tools'
isolated_tools.mkdir(parents=True)
for name in ['setup-media.ps1', 'media-runtime.json', 'MEDIA_RUNTIME_NOTICES.txt',
             'media-audio/toolchain.json', 'media-audio/licenses.json']:
    target = isolated_tools / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(tools / name, target)
license_pins = json.loads((tools / 'media-audio/licenses.json').read_text())
(isolated_tools / 'media-licenses').mkdir()
for entry in license_pins['licenses']:
    shutil.copyfile(tools / 'media-licenses' / entry['name'], isolated_tools / 'media-licenses' / entry['name'])
cache = root / 'native/artifacts/media-downloads' / (pin['sha256'] + '.zip')
cache.parent.mkdir(parents=True)
shutil.copyfile(artifacts / 'media-downloads' / cache.name, cache)
original = cache.read_bytes()


def setup():
    return subprocess.run(['powershell.exe', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(isolated_tools / 'setup-media.ps1')], capture_output=True, text=True)


result = setup()
check(result.returncode == 0, 'Fresh isolated setup: ' + result.stderr)
installed = root / 'native/artifacts/media-runtime' / pin['sha256']
for name, expected in pin['fileHashes'].items():
    check(sha((installed / name).read_bytes()) == expected, 'Installed runtime: ' + name)
check(set(x.name for x in (installed / 'licenses').iterdir()) == {x['name'] for x in license_pins['licenses']}, 'Installed license set')
mutated = bytearray(original)
mutated[len(mutated) // 2] ^= 1
cache.write_bytes(mutated)
result = setup()
check(result.returncode != 0 and 'Cached media runtime failed verification' in result.stderr, 'Corrupt cache must fail')
check(cache.read_bytes() == mutated, 'Corrupt cache evidence must be preserved')
for name, expected in pin['fileHashes'].items():
    check(sha((installed / name).read_bytes()) == expected, 'Corruption must preserve installed runtime: ' + name)
cache.write_bytes(original)
license_file = isolated_tools / 'media-licenses/COPYING.LGPLv2.1'
license_file.write_bytes(license_file.read_bytes() + b'changed')
result = setup()
check(result.returncode != 0 and 'Modified media license' in result.stderr, 'Changed license must fail')
report = {'checks': checks, 'runtimeSha256': pin['sha256'], 'sourceSha256': sha(first), 'sourceArchive': str(bundle), 'isolatedEvidence': str(root)}
(root / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
