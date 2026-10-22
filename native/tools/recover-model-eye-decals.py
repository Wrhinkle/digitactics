"""Restore Digimon World: Next Order eyes from the pinned public source archives.

Next Order draws the eyes as a decal: the body atlas (<code>_c00) carries only the
painted eye whites, and a separate RGBA sheet (<code>_e00) is sampled through the
mesh's second UV set on a few eye-quad triangles. The FBX conversion kept only the
first UV set and never embedded the decal sheet, so every rip lost its pupils.

This tool composites the source decal into the embedded body atlas, using the
source DAE's own triangles: each texel of an eye-quad triangle (located through
UV0) takes the decal color at the same barycentric point of UV1, scaled into the
sheet's first (open-eye) cell as the archive notes describe (size 0.5). The atlas is
upsampled 2x first so the small eye region keeps the decal's detail. Geometry,
UVs, rig and animation bytes are untouched; only the embedded image changes.

Needs numpy, Pillow and Node (for the vendored meshopt decoder used to verify UVs).
  python native/tools/recover-model-eye-decals.py --rips <dir with NO_*.zip> [--model shamanmon] [--apply]
"""
import argparse
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import zipfile
import xml.etree.ElementTree as ET

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
NS = {"c": "http://www.collada.org/2005/11/COLLADASchema"}
URL = "https://models.spriters-resource.com/pc_computer/digimonworldnextorder/asset/"
# Source archive, DAE and decal members, body atlas, Models Resource asset id, pinned input and output hashes.
RECOVERIES = {
    "aruraumon": {
        "asset": "models/files/rip_no_aruraumon.glb", "archive": "NO_Aruraumon.zip", "assetId": "342143",
        "archiveSha256": "8b3566996f1fafcd1617ce379905f905df9c9320e4aa524074c8791c1e3828bf",
        "dae": "Aruraumon/c035.dae", "decal": "Aruraumon/c035_e00.png", "image": "c035_c00.png",
        "originalSha256": "940686beddc7fbee0c38be329ca26ad9794d0427f76db614965102627055a89d",
        "repairedSha256": "fb02d491e9562a48ec6b88ff6a4999fdb28e113bb9e30b9dfe5977c0c39bfdc1"
    },
    "boltboutamon": {
        "asset": "models/files/rip_no_boltboutamon.glb", "archive": "NO_Boltboutamon.zip", "assetId": "342147",
        "archiveSha256": "b2edab3b36f846e6d293c9fbf3b4902df9d3b66db2aa9b5af9d8f5810c5eba3e",
        "dae": "Boltboutamon/g018.dae", "decal": "Boltboutamon/g018_e00.png", "image": "g018_c00.png",
        "originalSha256": "7c10bd0a014fddbb29e15016294e0ab45e349e4a7e26d56a386af4e4b582aab6",
        "repairedSha256": "8cd08fee79a4f0dc87666c173b18dec6cc96ec8e327d4c370d3180051793d593"
    },
    "growlmonyellow": {
        "asset": "models/files/rip_no_growlmon_yellow.glb", "archive": "NO_Growlmon_Yellow.zip", "assetId": "342133",
        "archiveSha256": "11c7672ce9fca0df02331f4e270b7746a8e4a5837d225b04a99293a9714f9ee7",
        "dae": "Growlmon (Yellow)/d052.dae", "decal": "Growlmon (Yellow)/d052_e00.png", "image": "d052_c00.png",
        "originalSha256": "ea0136041e48af1e29743a3f739fe86a60bc9a4f4133ee0a3015111878369508",
        "repairedSha256": "2f05e393cc4aadf9ff9f8400caea07f1053b1aa56737acb2f6488f5d5aea0e14"
    },
    "gururumon": {
        "asset": "models/files/rip_no_gururumon.glb", "archive": "NO_Gururumon.zip", "assetId": "342134",
        "archiveSha256": "d4965dc09cb3564c165ca23f89086b709cfa0a14982cbab74b3f958d6ebc728e",
        "dae": "Gururumon/d043.dae", "decal": "Gururumon/d043_e00.png", "image": "d043_c00.png",
        "originalSha256": "30b260275f69534b4570b67230c97584da5731842153868856d9c7778c899e8d",
        "repairedSha256": "63244aeb28c2b313d860a2da1dd9903515a551ed051d234d073913a812c2a0fc"
    },
    "hyogamon": {
        "asset": "models/files/rip_no_hyogamon.glb", "archive": "NO_Hyogamon.zip", "assetId": "342135",
        "archiveSha256": "d4d7ae46ea79e9936fe29f6e5f60b52b42f3c449e8aa7f234e5d1ef3e02b45b9",
        "dae": "Hyogamon/d040.dae", "decal": "Hyogamon/d040_e00.png", "image": "d040_c00.png",
        "originalSha256": "2f5c1711ac406e16fdb4a9059b92250250f16fbec342e50f6a95aaf97e3afd97",
        "repairedSha256": "8db1f073ffe19b5220da7c087d07d0d4dc4fe004ddd28217d4bf4c47e9cb6cf0"
    },
    "kingsukamon": {
        "asset": "models/files/rip_no_kingsukamon.glb", "archive": "NO_KingSukamon.zip", "assetId": "342141",
        "archiveSha256": "3c1e6ad5ffa7c899ae4c8b91337a58c5aa8195e26475175793ae95eb108e00c3",
        "dae": "KingSukamon/f062.dae", "decal": "KingSukamon/f062_e00.png", "image": "f062_c00.png",
        "originalSha256": "17f708f5e79e3187c22a5bcc9482517f171d1c02726a940ecad977ff13334fbe",
        "repairedSha256": "020e234500fb7664dfc8c53cc76c5396c38e026212dacc2428b3cf6ccb625a63"
    },
    "kyubimonsilver": {
        "asset": "models/files/rip_no_kyubimon_silver.glb", "archive": "NO_Kyubimon_Silver.zip", "assetId": "342140",
        "archiveSha256": "608db6a6b6663aadc7cfd45628bc227cc6ffa1a2ac99a7a555d98ed5079270be",
        "dae": "Youkomon Kyubimon (Silver)/d042.dae", "decal": "Youkomon Kyubimon (Silver)/d042_e00.png", "image": "d042_c00.png",
        "originalSha256": "4910664b0646b0d6e56595c87466ba1c7dccd193045d2ffa75c907cd188e2655",
        "repairedSha256": "37df49bbd26d5eafabc9f79ba24b315fd46cc83474e32f527e6def1990b2e3e4"
    },
    "psychemon": {
        "asset": "models/files/rip_no_psychemon.glb", "archive": "NO_Psychemon.zip", "assetId": "342144",
        "archiveSha256": "822a6be1794178d001d4810955ce154de4c4ad248b8d0839d9030933e1043867",
        "dae": "Psychemon/c033.dae", "decal": "Psychemon/c033_e00.png", "image": "c033_c00.png",
        "originalSha256": "f8d37f175152bff9c36d38a618b74c6f9353ceaf171709873c45af2ee88ab64c",
        "repairedSha256": "a062498223ff43b94233385f2f92d0d80ee4273bfe1e1b317ffb5d664a7e7268"
    },
    "redveedramon": {
        "asset": "models/files/rip_no_redveedramon.glb", "archive": "NO_RedVeedramon.zip", "assetId": "342137",
        "archiveSha256": "c311567919bbb1f2f96b1d61da6ba1a063e9c85ed65431e52afb4bce2a7db77d",
        "dae": "RedVeedramon/d050.dae", "decal": "RedVeedramon/d050_e00.png", "image": "d050_c00.png",
        "originalSha256": "7c675c070d7d9819f16b5a583d247f24479e3aac6cc09a2cfedf4f7140326d6f",
        "repairedSha256": "19e9a7a3711e441a16452b38bef9f7fb070ae37a3d20d770e49983d28d730b8a"
    },
    "redvegiemon": {
        "asset": "models/files/rip_no_redvegiemon.glb", "archive": "NO_RedVegiemon.zip", "assetId": "342138",
        "archiveSha256": "aae97d6a44afee5f4bcca711ba9597cf62d4dca865584e4c3242b3450c37f790",
        "dae": "RedVegiemon/d055.dae", "decal": "RedVegiemon/d055_e00.png", "image": "d055_c00.png",
        "originalSha256": "86453755588c23e09d42de5dbe0a45ec213cb2c0f67b97649ca92be47c682875",
        "repairedSha256": "99261b97e727d7d44eaa371a6c7056ea4b528ed7f38a0eeba12a3be2b13d02da"
    },
    "saberdramon": {
        "asset": "models/files/rip_no_saberdramon.glb", "archive": "NO_Saberdramon.zip", "assetId": "342139",
        "archiveSha256": "a34ec7109d0ef7cc757f7964e45264d36222166c57c3139bdf23d09a4113835e",
        "dae": "Saberdramon/d044.dae", "decal": "Saberdramon/d044_e00.png", "image": "d044_c00.png",
        "originalSha256": "0635c97161b4bf9ce7bfa74e6eadad4e5dd2948cf8d82ba5b5d585216e62cb04",
        "repairedSha256": "0e68c1e44841be7b026dd4291c418701c663dff5f3d4178995f2a0812e08b94d"
    },
    "shamanmon": {
        "asset": "models/files/rip_no_shamanmon.glb", "archive": "NO_Shamanmon.zip", "assetId": "342145",
        "archiveSha256": "7a2d585a6d8acdc64eb98216c72ad8fa8f834d24450ddc0f38a9bc0c33eb32d3",
        "dae": "Shamanmon/c042.dae", "decal": "Shamanmon/c042_e00.png", "image": "c042_c00.png",
        "originalSha256": "d777eb3b22c217b62800bc5211ba819f8bdec797269a39f75642b407219a029a",
        "repairedSha256": "603e5bd9b1aa4c49336657f66536e9449184dcc3e6cbcd890f5dec5fa07b6e3a"
    },
    "snowgoblimon": {
        "asset": "models/files/rip_no_snowgoblimon.glb", "archive": "NO_SnowGoblimon.zip", "assetId": "342146",
        "archiveSha256": "946eb24166065188e65d09834efe8c19410ac548528a650c146fbbd0e0b842fc",
        "dae": "SnowGoblimon/c041.dae", "decal": "SnowGoblimon/c041_e00.png", "image": "c041_c00.png",
        "originalSha256": "9c8c3715942f80b18f4e1d74f5de7087ee15253ebdabc60f3ad7795b9d6758df",
        "repairedSha256": "0e381a879e9af2faaf5e6003723c81565bfaf8916e4213c6cc98b9bc179b6281"
    },
    "wargrowlmonyellow": {
        "asset": "models/files/rip_no_wargrowlmon_yellow.glb", "archive": "NO_WarGrowlmon_Yellow.zip", "assetId": "342142",
        "archiveSha256": "4284ac4eb075b4d225f019d4d810e6130edf31b03dc08fe5eaef09f5b9d8fe79",
        "dae": "WarGrowlmon (Yellow)/e048.dae", "decal": "WarGrowlmon (Yellow)/e048_e00.png", "image": "e048_c00.png",
        "originalSha256": "2d7e593cf1dab1fe34842cdaf2b089efc0b68d7e0a568a3cfabf6921a9a8882a",
        "repairedSha256": "07723e6265e58989c1f765234f6afd64454ea029e92e35e7598773a677a1f023"
    },
    "youkomon": {
        "asset": "models/files/rip_no_youkomon.glb", "archive": "NO_Kyubimon_Silver.zip", "assetId": "342140",
        "archiveSha256": "608db6a6b6663aadc7cfd45628bc227cc6ffa1a2ac99a7a555d98ed5079270be",
        "dae": "Youkomon Kyubimon (Silver)/d042.dae", "decal": "Youkomon Kyubimon (Silver)/d042_e00.png", "image": "d042_c00.png",
        "originalSha256": "bcc441bd4a69acc4b478148ced270341b59ef5a904edc641456744c2d4c9442b",
        "repairedSha256": "531cf27ed1f3478101d07661a8a04d6af600149538eca7a5769eb54de9a60123"
    }
}
SIZE_FACTOR = 2
CELL = (0.0, 0.5)  # open-eye cell of the 2x2 sheet, in UV (v-up) space
DECAL_SCALE = 0.5
WEBP_QUALITY = 92

spec = importlib.util.spec_from_file_location("material_repair", Path(__file__).with_name("repair-model-materials.py"))
glb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(glb)
spec = importlib.util.spec_from_file_location("material_recovery", Path(__file__).with_name("recover-model-materials.py"))
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def dae_triangles(dae):
    """Per material image: (uv0, uv1) corner arrays of every source triangle."""
    root = ET.fromstring(dae)
    out = {}
    for geometry in root.findall(".//c:library_geometries/c:geometry", NS):
        mesh = geometry.find("c:mesh", NS)
        arrays = {s.get("id"): np.array(s.find("c:float_array", NS).text.split(), float).reshape(
            -1, int(s.find("c:technique_common/c:accessor", NS).get("stride")))
            for s in mesh.findall("c:source", NS) if s.find("c:float_array", NS) is not None}
        for poly in mesh.findall("c:polylist", NS) + mesh.findall("c:triangles", NS):
            if poly.find("c:vcount", NS) is not None and any(int(x) != 3 for x in poly.find("c:vcount", NS).text.split()):
                raise ValueError("Source contains non-triangles")
            inputs = poly.findall("c:input", NS)
            stride = 1 + max(int(i.get("offset")) for i in inputs)
            p = np.array(poly.find("c:p", NS).text.split(), int).reshape(-1, stride)
            uv = {i.get("set", "0"): arrays[i.get("source")[1:]][p[:, int(i.get("offset"))], :2]
                  for i in inputs if i.get("semantic") == "TEXCOORD"}
            if set(uv) != {"0", "1"}:
                raise ValueError("Expected the body and eye UV sets")
            material = root.find(".//c:library_materials/c:material[@id='" + poly.get("material") + "']", NS)
            effect = root.find(".//c:library_effects/c:effect[@id='" + material.find("c:instance_effect", NS).get("url")[1:] + "']", NS)
            texture = effect.find(".//c:diffuse/c:texture", NS).get("texture")
            sampler = effect.find(".//c:newparam[@sid='" + texture + "']/c:sampler2D/c:source", NS)
            surface = effect.find(".//c:newparam[@sid='" + sampler.text + "']/c:surface/c:init_from", NS) if sampler is not None else None
            image = root.find(".//c:library_images/c:image[@id='" + (surface.text if surface is not None else texture) + "']/c:init_from", NS)
            name = image.text.replace("\\", "/").rsplit("/", 1)[-1]
            entry = out.setdefault(name, [[], []])
            entry[0].append(uv["0"]); entry[1].append(uv["1"])
    return {k: (np.concatenate(v[0]), np.concatenate(v[1])) for k, v in out.items()}


def sample(image, u, v):
    """Bilinear RGBA sample at image-space (u right, v down)."""
    h, w = image.shape[:2]
    x, y = u * w - .5, v * h - .5
    x0, y0 = np.floor(x).astype(int), np.floor(y).astype(int)
    fx, fy = (x - x0)[:, None], (y - y0)[:, None]
    at = lambda yy, xx: image[np.clip(yy, 0, h - 1), np.clip(xx, 0, w - 1)]
    return ((at(y0, x0) * (1 - fx) + at(y0, x0 + 1) * fx) * (1 - fy) +
            (at(y0 + 1, x0) * (1 - fx) + at(y0 + 1, x0 + 1) * fx) * fy)


def bake(base, decal, uv0, uv1, supersample=4):
    """Composite the decal over base on every eye-quad triangle. Returns (image, triangles, texels)."""
    size = base.shape[0]
    premultiplied = np.zeros(base.shape[:2] + (4,))
    seen, used = set(), 0
    sub = (np.arange(supersample) + .5) / supersample
    for t in range(len(uv0) // 3):
        q1 = uv1[3 * t:3 * t + 3]
        if np.ptp(q1, axis=0).sum() < 0.05:  # body triangles collapse to one decal point
            continue
        key = tuple(np.round(np.concatenate([uv0[3 * t:3 * t + 3], q1]).ravel(), 5))
        if key in seen:  # mirrored eyes share both UV sets; paint each texel once
            continue
        seen.add(key)
        used += 1
        q0 = uv0[3 * t:3 * t + 3].copy()
        q0[:, 1] = 1 - q0[:, 1]
        q0 *= size
        lo, hi = np.floor(q0.min(0)).astype(int), np.ceil(q0.max(0)).astype(int)
        xs, ys = np.meshgrid(np.arange(lo[0], hi[0]), np.arange(lo[1], hi[1]))
        shape = xs.shape + (supersample, supersample)
        px = np.broadcast_to(xs[..., None, None] + sub[:, None], shape).reshape(-1)
        py = np.broadcast_to(ys[..., None, None] + sub[None, :], shape).reshape(-1)
        tx, ty = np.repeat(xs.reshape(-1), supersample ** 2), np.repeat(ys.reshape(-1), supersample ** 2)
        a, b, c = q0
        m = np.array([[b[0] - a[0], c[0] - a[0]], [b[1] - a[1], c[1] - a[1]]])
        if abs(np.linalg.det(m)) < 1e-12:
            continue
        w1, w2 = np.linalg.solve(m, np.stack([px - a[0], py - a[1]]))
        w0 = 1 - w1 - w2
        inside = (w0 >= 0) & (w1 >= 0) & (w2 >= 0)
        point = w0[:, None] * q1[0] + w1[:, None] * q1[1] + w2[:, None] * q1[2]
        du = point[:, 0] * DECAL_SCALE + CELL[0]
        dv = 1 - (point[:, 1] * DECAL_SCALE + CELL[1])
        color = sample(decal, du % 1, dv % 1)[inside]
        alpha = color[:, 3:] / 255
        np.add.at(premultiplied, (ty[inside] % size, tx[inside] % size),
                  np.concatenate([color[:, :3] * alpha, alpha], 1) / supersample ** 2)
    coverage = np.clip(premultiplied[..., 3:], 0, 1)
    result = base * (1 - coverage) + premultiplied[..., :3]
    return np.clip(result + .5, 0, 255).astype(np.uint8), used, int((coverage[..., 0] > 0).sum())


def replace_view(document, binary, view_index, blob):
    """Replace one plain buffer-0 view, shifting later data so meshopt streams stay byte-identical."""
    view = document["bufferViews"][view_index]
    if view.get("buffer") != 0 or "extensions" in view:
        raise ValueError("Image view must be plain data in buffer 0")
    start = view.get("byteOffset", 0)
    regions = []
    for v in document["bufferViews"]:
        e = v.get("extensions", {}).get("EXT_meshopt_compression")
        if e and e.get("buffer", 0) == 0:
            regions.append(e.get("byteOffset", 0))
        elif v.get("buffer") == 0:
            regions.append(v.get("byteOffset", 0))
    following = [o for o in regions if o > start]
    end = min(following) if following else len(binary)
    room = end - start
    padded = len(blob) + ((room - len(blob)) % 4)
    shift = padded - room
    data = binary[:start] + blob + b"\0" * (padded - len(blob)) + binary[end:]
    for v in document["bufferViews"]:
        e = v.get("extensions", {}).get("EXT_meshopt_compression")
        if e and e.get("buffer", 0) == 0 and e.get("byteOffset", 0) >= end:
            e["byteOffset"] = e.get("byteOffset", 0) + shift
        if v.get("buffer") == 0 and v is not view and v.get("byteOffset", 0) >= end:
            v["byteOffset"] = v.get("byteOffset", 0) + shift
    view["byteLength"] = len(blob)
    document["buffers"][0]["byteLength"] = len(data)
    return data


def view_bytes(document, binary, v):
    e = v.get("extensions", {}).get("EXT_meshopt_compression")
    if e:
        return binary[e.get("byteOffset", 0):e.get("byteOffset", 0) + e["byteLength"]]
    return binary[v.get("byteOffset", 0):v.get("byteOffset", 0) + v["byteLength"]]


def recover(name, entry, archive_data, original):
    if digest(archive_data) != entry["archiveSha256"] or digest(original) != entry["originalSha256"]:
        raise ValueError(name + ": archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    dae = archive.read(entry["dae"])
    decal_bytes = archive.read(entry["decal"])
    decal = np.asarray(Image.open(io.BytesIO(decal_bytes)).convert("RGBA")).astype(float)
    document, chunks = glb.read_glb(original)
    if len(chunks) != 1:
        raise ValueError("Expected one binary chunk")
    before = copy.deepcopy(document)
    binary = chunks[0][1]
    triangles = dae_triangles(dae)
    if entry["image"] not in triangles:
        raise ValueError("Source DAE does not bind the reviewed body atlas")
    uv0, uv1 = triangles[entry["image"]]
    # The tracked GLB must carry exactly the source triangles and UV0 of that atlas.
    decoded, decoded_chunks = glb.read_glb(recovery.decode_original(original))
    data = decoded_chunks[0][1]
    images = [i for i, im in enumerate(document["images"]) if im.get("name") == entry["image"]]
    if len(images) != 1:
        raise ValueError("Expected one embedded body atlas")
    textures = {i for i in range(len(document["textures"])) if glb.image_name(document, i) == entry["image"]}
    materials = {i for i, m in enumerate(document["materials"])
                 if m.get("pbrMetallicRoughness", {}).get("baseColorTexture", {}).get("index") in textures}
    corners = []
    for mesh in decoded["meshes"]:
        for p in mesh["primitives"]:
            if p.get("material") in materials:
                indices = recovery.accessor(decoded, data, p["indices"]).reshape(-1).astype(int)
                corners.append(recovery.accessor(decoded, data, p["attributes"]["TEXCOORD_0"], True)[indices])
    corners = np.concatenate(corners)
    # Every source triangle of this atlas must survive (a rip whose second atlas was
    # never embedded binds those extra triangles to it too; they are left as they are).
    if len(corners) < len(uv0):
        raise ValueError(name + ": source and tracked triangle counts differ: %d vs %d" % (len(uv0) // 3, len(corners) // 3))
    source = uv0.copy()
    source[:, 1] = 1 - source[:, 1]
    from scipy.spatial import cKDTree
    uv_error = float(cKDTree(corners).query(source)[0].max())
    if uv_error > 0.0005:  # quantized UV0; below a quarter texel of the 2x atlas
        raise ValueError("Source UV0 does not match the tracked atlas UVs: %g" % uv_error)
    view_index = document["images"][images[0]]["bufferView"]
    shipped = Image.open(io.BytesIO(view_bytes(document, binary, document["bufferViews"][view_index]))).convert("RGB")
    size = shipped.width * SIZE_FACTOR
    base = np.asarray(shipped.resize((size, size), Image.LANCZOS)).astype(float)
    baked, used, texels = bake(base, decal, uv0, uv1)
    out = io.BytesIO()
    Image.fromarray(baked).save(out, "WEBP", quality=WEBP_QUALITY, method=6)
    new_binary = replace_view(document, binary, view_index, out.getvalue())
    result = glb.write_glb(document, [(chunks[0][0], new_binary)])
    # Only the image bytes, view offsets/lengths and buffer length may change.
    after, after_chunks = glb.read_glb(result)
    for i, (old_v, new_v) in enumerate(zip(before["bufferViews"], after["bufferViews"])):
        if i != view_index and view_bytes(before, binary, old_v) != view_bytes(after, after_chunks[0][1], new_v):
            raise ValueError("Non-image buffer data changed")
    strip = lambda d: {k: v for k, v in d.items() if k not in ("bufferViews", "buffers")}
    if strip(after) != strip(before):
        raise ValueError("GLB metadata other than buffer layout changed")
    report = {"schemaVersion": 1, "asset": entry["asset"], "originalSha256": entry["originalSha256"],
              "repairedSha256": digest(result), "sourceUrl": URL + entry["assetId"] + "/",
              "archiveSha256": entry["archiveSha256"], "daeSha256": digest(dae), "decalSha256": digest(decal_bytes),
              "decal": entry["decal"].rsplit("/", 1)[-1], "bodyAtlas": entry["image"],
              "sourceTriangles": len(uv0) // 3, "eyeQuadTriangles": used, "decalTexels": texels,
              "maximumUv0Error": uv_error, "atlasSize": [size, size], "decalCell": list(CELL), "decalScale": DECAL_SCALE,
              "preservedGeometryRigAndAnimation": True}
    return result, report, baked


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--rips", type=Path, required=True, help="Directory holding the NO_*.zip source archives")
    parser.add_argument("--model", choices=sorted(RECOVERIES), action="append")
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--preview", type=Path, help="Write each baked atlas here as PNG")
    options = parser.parse_args()
    reports = []
    for name in options.model or sorted(RECOVERIES):
        entry = RECOVERIES[name]
        asset = ROOT / entry["asset"]
        current = asset.read_bytes()
        backup = ROOT / "native/artifacts/model-material-originals" / asset.name
        # A later reviewed step (recover-model-materials.py boltboutamon) may start from
        # this repair; the tracked asset then carries that step's output instead.
        later = digest(current) not in (entry["originalSha256"], entry.get("repairedSha256"))
        if (digest(current) == entry.get("repairedSha256") or later) and not backup.exists():
            reports.append({"asset": entry["asset"], "state": "superseded" if later else "correct",
                            "sha256": entry["repairedSha256"]})
            continue
        if later and options.apply:
            raise ValueError(name + ": the asset carries a later reviewed repair; reproduce with --preview only")
        original = backup.read_bytes() if digest(current) == entry.get("repairedSha256") or later else current
        result, report, baked = recover(name, entry, (options.rips / entry["archive"]).read_bytes(), original)
        if options.preview:
            options.preview.mkdir(parents=True, exist_ok=True)
            Image.fromarray(baked).save(options.preview / (name + ".png"))
        if entry.get("repairedSha256") and report["repairedSha256"] != entry["repairedSha256"]:
            raise ValueError(name + ": output differs from the reviewed bytes")
        if options.apply:
            if not entry.get("repairedSha256"):
                raise ValueError(name + ": pin repairedSha256 after reviewing the preview before applying")
            backup.parent.mkdir(parents=True, exist_ok=True)
            if backup.exists() and backup.read_bytes() != original:
                raise ValueError("Original backup already exists with different bytes")
            backup.write_bytes(original)
            asset.write_bytes(result)
        reports.append(report)
    print(json.dumps(reports, indent=2))


if __name__ == "__main__":
    main()
