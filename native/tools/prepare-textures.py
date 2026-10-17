"""Canonical RGBA8 PNG textures for the native loader; Pillow is build-time only."""
import gzip
import hashlib
import io
import json
from pathlib import Path
import re
import struct
from PIL import Image

root = Path(__file__).resolve().parents[1]
content = root / 'artifacts/model-content'
manifest_path = root / 'content/models.json'
catalog = json.loads(manifest_path.read_text(encoding='utf-8'))
probes = []
for asset, entry in catalog['assets'].items():
    source = gzip.decompress((content / entry['file']).read_bytes())
    length = struct.unpack_from('<I', source, 12)[0]
    doc = json.loads(source[20:20+length])
    binary = source[28+length:]
    replacements = {}
    images = []
    for image in doc.get('images', []):
        view_index = image['bufferView']
        view = doc['bufferViews'][view_index]
        encoded = binary[view.get('byteOffset', 0):view.get('byteOffset', 0)+view['byteLength']]
        with Image.open(io.BytesIO(encoded)) as decoded:
            rgba = decoded.convert('RGBA')
            output = io.BytesIO()
            rgba.save(output, format='PNG', compress_level=6)
            replacements[view_index] = output.getvalue()
            pixels = rgba.tobytes()
            images.append({'width': rgba.width, 'height': rgba.height, 'sha256': hashlib.sha256(pixels).hexdigest()})
        image['mimeType'] = 'image/png'
    chunks = bytearray()
    for i, view in enumerate(doc.get('bufferViews', [])):
        data = replacements.get(i)
        if data is None:
            data = binary[view.get('byteOffset', 0):view.get('byteOffset', 0)+view['byteLength']]
        chunks.extend(bytes((-len(chunks)) % 4))
        view['byteOffset'] = len(chunks)
        view['byteLength'] = len(data)
        chunks.extend(data)
    doc['buffers'] = [{'byteLength': len(chunks)}]
    for texture in doc.get('textures', []):
        extensions = texture.get('extensions', {})
        webp = extensions.pop('EXT_texture_webp', None)
        if webp is not None:
            texture['source'] = webp['source']
        if 'extensions' in texture and not extensions:
            del texture['extensions']
    for name in ['extensionsUsed', 'extensionsRequired']:
        if name in doc:
            doc[name] = [x for x in doc[name] if x != 'EXT_texture_webp']
    metadata = json.dumps(doc, separators=(',', ':'), ensure_ascii=False).encode()
    metadata += b' ' * ((-len(metadata)) % 4)
    chunks.extend(bytes((-len(chunks)) % 4))
    glb = struct.pack('<III', 0x46546c67, 2, 28+len(metadata)+len(chunks))
    glb += struct.pack('<II', len(metadata), 0x4e4f534a) + metadata
    glb += struct.pack('<II', len(chunks), 0x004e4942) + chunks
    packed = gzip.compress(glb, compresslevel=9, mtime=0)
    digest = hashlib.sha256(packed).hexdigest()
    relative = 'models/' + digest + '.glb.gz'
    destination = content / relative
    if not destination.exists() or hashlib.sha256(destination.read_bytes()).hexdigest() != digest:
        temporary = destination.with_suffix('.tmp')
        temporary.write_bytes(packed)
        temporary.replace(destination)
    entry.update(file=relative, sha256=digest, bytes=len(packed))
    probes.append({'asset': asset, 'images': images})
manifest_path.write_text(json.dumps(catalog, separators=(',', ':'), ensure_ascii=False)+'\n', encoding='utf-8')
(root / 'tests/fixtures/textures.json').write_text(json.dumps(probes, separators=(',', ':'))+'\n', encoding='utf-8')
# Only remove obsolete content-addressed outputs inside this exporter-owned directory.
keep = {Path(a['file']).name for a in catalog['assets'].values()}
directory = (content / 'models').resolve()
for file in directory.iterdir():
    if re.fullmatch(r'[0-9a-f]{64}\.glb\.gz', file.name) and file.name not in keep and file.resolve().parent == directory:
        file.unlink()
print(f"Prepared {sum(len(p['images']) for p in probes)} RGBA8 PNG textures.")
