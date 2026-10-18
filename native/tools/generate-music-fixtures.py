"""Explicitly regenerate synthetic test audio using the development-only encoder."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

tools = Path(__file__).resolve().parent
native = tools.parent
pin = json.loads((tools / 'media-fixture-runtime.json').read_text(encoding='utf-8-sig'))
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--encoder', type=Path, default=native / 'artifacts/media-runtime' / pin['sha256'] / 'ffmpeg.exe')
args = parser.parse_args()
encoder = args.encoder.resolve()
version = subprocess.check_output([str(encoder), '-version']).decode()
if not version.startswith('ffmpeg version ' + pin['build'] + ' '):
    raise ValueError('Regeneration requires the pinned development encoder')
destination = native / 'tests/DigiTactics.Tests/Fixtures/Music'
destination.mkdir(parents=True, exist_ok=True)
files = []
for extension, codec, duration, name in [
    ('wav', 'pcm_s16le', '0.3', 'short'), ('mp3', 'libmp3lame', '0.3', 'short'),
    ('ogg', 'libvorbis', '0.3', 'short'), ('oga', 'libvorbis', '0.3', 'short'),
    ('m4a', 'aac', '0.3', 'short'), ('aac', 'aac', '0.3', 'short'),
    ('flac', 'flac', '0.3', 'short'), ('opus', 'libopus', '0.3', 'short'),
    ('webm', 'libopus', '0.3', 'short'), ('flac', 'flac', '60', 'long')]:
    filename = name + '.' + extension
    options = ['-hide_banner', '-loglevel', 'error', '-nostdin', '-y', '-f', 'lavfi', '-i',
               'sine=frequency=441:sample_rate=32000:duration=' + duration, '-ac', '1', '-c:a', codec]
    subprocess.run([str(encoder), *options, str(destination / filename)], check=True)
    data = (destination / filename).read_bytes()
    files.append({'path': filename, 'size': len(data), 'sha256': hashlib.sha256(data).hexdigest(), 'arguments': options})
manifest = {'schemaVersion': 1, 'description': 'Original synthetic 441 Hz sine wave; 32 kHz mono; no sampled recordings.',
            'encoderArchiveSha256': pin['sha256'], 'encoderBuild': pin['build'], 'files': files}
(destination / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
print(f'Generated {len(files)} fixtures, {sum(f["size"] for f in files):,} bytes.')
