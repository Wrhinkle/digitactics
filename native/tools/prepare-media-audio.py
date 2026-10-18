"""Package the qualified, locally built FFmpeg runtime and its complete source inputs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import zipfile

TOOLS = Path(__file__).resolve().parent
ARTIFACTS = TOOLS.parent / 'artifacts'
FILES = ['ffmpeg.exe', 'avcodec-62.dll', 'avfilter-11.dll', 'avformat-62.dll', 'avutil-60.dll', 'swresample-6.dll']
LICENSES = ['COPYING.LGPLv2.1', 'COPYING.GPLv2', 'COPYING.LGPLv3', 'COPYING.GPLv3', 'LICENSE.md']


def digest(data):
    return hashlib.sha256(data).hexdigest()


def read_json(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def encoded(value):
    return (json.dumps(value, indent=2, ensure_ascii=True) + '\n').encode()


def checked(path, pin):
    data = path.read_bytes()
    if len(data) != pin['size'] or digest(data) != pin['sha256']:
        raise ValueError(f'Hash/size mismatch, preserved: {path}')
    return data


def archive(path, entries):
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix('.tmp')
    with zipfile.ZipFile(temporary, 'w', compression=zipfile.ZIP_STORED) as output:
        for name, data in sorted(entries.items()):
            entry = zipfile.ZipInfo(name, (2000, 1, 1, 0, 0, 0))
            entry.create_system = 0
            entry.external_attr = 0x20
            output.writestr(entry, data)
    temporary.replace(path)
    return digest(path.read_bytes())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--stage', type=Path, default=Path(os.environ['LOCALAPPDATA']) / 'DigiTacticsBuild/ffmpeg-audio/stage/digitactics-media-runtime')
    parser.add_argument('--write-pin', action='store_true', help='Explicitly qualify a new build after decoder and mixer tests.')
    parser.add_argument('--runtime-only', action='store_true')
    args = parser.parse_args()
    stage = args.stage.resolve()
    toolchain = read_json(TOOLS / 'media-audio/toolchain.json')
    config = (stage / 'config.h').read_text()
    for name in ('GPL', 'NONFREE', 'VERSION3', 'NETWORK'):
        if not re.search(rf'^#define CONFIG_{name} 0$', config, re.M):
            raise ValueError(f'Unexpected enabled {name} configuration')
    if re.search(r'^#define CONFIG_LIB\w+ 1$', config, re.M):
        raise ValueError('Unexpected external codec library')
    version = subprocess.check_output([str(stage / 'bin/ffmpeg.exe'), '-version']).decode()
    expected_version = '8.1.3-digitactics-audio-29e619e767'
    if not version.startswith(f'ffmpeg version {expected_version} '):
        raise ValueError('Unexpected decoder version')
    binaries = {name: (stage / 'bin' / name).read_bytes() for name in FILES}
    temporary = ARTIFACTS / 'media-downloads/audio-candidate.zip'
    runtime_hash = archive(temporary, binaries)
    runtime_archive = temporary.with_name(runtime_hash + '.zip')
    if runtime_archive.exists() and digest(runtime_archive.read_bytes()) != runtime_hash:
        raise ValueError(f'Corrupt runtime cache was preserved: {runtime_archive}')
    temporary.replace(runtime_archive)
    pin_path = TOOLS / 'media-runtime.json'
    if args.write_pin:
        pin = {'version': 2, 'name': 'FFmpeg', 'provider': 'local-source-build', 'build': expected_version,
               'license': 'LGPL-2.1-or-later', 'sourceCommit': toolchain['sourceCommit'],
               'sourceUrl': toolchain['sourceUrl'], 'sha256': runtime_hash, 'size': runtime_archive.stat().st_size,
               'files': FILES, 'fileHashes': {name: digest(data) for name, data in binaries.items()}}
        pin_path.write_bytes(encoded(pin))
    else:
        pin = read_json(pin_path)
        if pin['sha256'] != runtime_hash:
            raise ValueError(f'Build differs from qualified runtime. Candidate preserved at {runtime_archive}')
    print(f'Runtime: {runtime_archive} ({runtime_archive.stat().st_size} bytes)')
    if args.runtime_only:
        return
    entries = {}
    source_pin = {'size': toolchain['sourceSize'], 'sha256': toolchain['sourceSha256']}
    source_data = checked(ARTIFACTS / 'media-source-downloads' / (toolchain['sourceSha256'] + '.zip'), source_pin)
    entries['sources/FFmpeg-' + toolchain['sourceCommit'] + '.zip'] = source_data
    with zipfile.ZipFile(ARTIFACTS / 'media-source-downloads' / (toolchain['sourceSha256'] + '.zip')) as source:
        source_root = stage.parent.parent / ('FFmpeg-' + toolchain['sourceCommit'])
        for entry in source.infolist():
            if not entry.is_dir():
                relative = entry.filename.split('/', 1)[1]
                if (source_root / relative).read_bytes() != source.read(entry):
                    raise ValueError(f'Upstream source was modified: {relative}')
        for name in LICENSES:
            license_data = source.read('FFmpeg-' + toolchain['sourceCommit'] + '/' + name)
            if license_data != (TOOLS / 'media-licenses' / name).read_bytes():
                raise ValueError(f'License differs from source: {name}')
            entries['licenses/' + name] = license_data
    support = read_json(TOOLS / 'media-build-tools.json')
    for name in ('make', 'nasm'):
        for kind in ('source', 'binary'):
            support_pin = support[name][kind]
            entries[('sources/' if kind == 'source' else 'build-tools/') + support_pin['file']] = checked(ARTIFACTS / 'media-build-tools' / support_pin['file'], support_pin)
    entries['licenses/GNU-Make-COPYING'] = (ARTIFACTS / 'media-build-tools/licenses/make-4.4.1/COPYING').read_bytes()
    entries['licenses/NASM-LICENSE.txt'] = (ARTIFACTS / 'media-build-tools/licenses/NASM-LICENSE.txt').read_bytes()
    for name in ('build-media-audio.ps1', 'setup-media-build-tools.ps1', 'media-build-tools.json', 'MEDIA_BUILD_TOOLS.md',
                 'prepare-media-audio.py', 'media-audio/build.sh', 'media-audio/toolchain.json', 'media-audio/licenses.json'):
        entries['build/native/tools/' + name] = (TOOLS / name).read_bytes()
    for name, source_path in [('build.sh', TOOLS / 'media-audio/build.sh'), ('toolchain.json', TOOLS / 'media-audio/toolchain.json')]:
        if (stage / name).read_bytes() != source_path.read_bytes():
            raise ValueError(f'Build evidence differs from packaged recipe: {name}')
    for name in ('config.h', 'config_components.h', 'ffbuild/config.mak', 'ffbuild/config.log'):
        entries['configuration/' + name] = (stage / name).read_bytes()
    entries['configuration/ffmpeg-version.txt'] = version.encode()
    entries['changes.diff'] = b''  # Upstream source is not patched.
    entries['runtime-provenance.json'] = encoded(pin)
    entries['MEDIA_RUNTIME_NOTICES.txt'] = (TOOLS / 'MEDIA_RUNTIME_NOTICES.txt').read_bytes()
    entries['README.md'] = (TOOLS / 'MEDIA_AUDIO_RUNTIME.md').read_bytes()
    manifest = {'schemaVersion': 1, 'runtimeSha256': runtime_hash, 'distributionReady': True,
                'scope': 'Corresponding FFmpeg source and build materials; distribute this archive beside the matching game binaries.',
                'remainingRequirements': [], 'files': [
                    {'path': name, 'size': len(data), 'sha256': digest(data)} for name, data in sorted(entries.items())]}
    entries['materials-manifest.json'] = encoded(manifest)
    destination = ARTIFACTS / 'media-audio-materials' / runtime_hash / ('digitactics-media-audio-sources-' + expected_version + '.zip')
    source_hash = archive(destination, entries)
    destination.with_suffix('.zip.sha256').write_text(f'{source_hash}  {destination.name}\n', encoding='ascii')
    print(f'Sources: {destination} ({destination.stat().st_size} bytes; sha256 {source_hash})')


if __name__ == '__main__':
    main()
