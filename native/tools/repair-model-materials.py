"""Reproduce reviewed, exact GLB material repairs without changing geometry or motion.

Normal builds consume the repaired tracked GLBs. --check validates those bindings;
--apply is only for reproducing a repair from the pinned original source asset.
Unreviewed gradient textures are deliberately outside this manifest.
"""
import argparse
import copy
import hashlib
import json
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = Path(__file__).with_name("model-material-repairs.json")


def read_glb(data):
    if len(data) < 20 or struct.unpack_from("<III", data) != (0x46546C67, 2, len(data)):
        raise ValueError("Invalid GLB header")
    chunks = []
    offset = 12
    while offset < len(data):
        size, kind = struct.unpack_from("<II", data, offset)
        end = offset + 8 + size
        if end > len(data) or size % 4:
            raise ValueError("Invalid GLB chunk")
        chunks.append((kind, data[offset + 8:end]))
        offset = end
    if not chunks or chunks[0][0] != 0x4E4F534A:
        raise ValueError("Missing first JSON chunk")
    return json.loads(chunks[0][1]), chunks[1:]


def write_glb(document, chunks):
    encoded = json.dumps(document, separators=(",", ":"), ensure_ascii=False).encode("utf-8")
    encoded += b" " * (-len(encoded) % 4)
    body = b"".join(struct.pack("<II", len(value), kind) + value
                    for kind, value in [(0x4E4F534A, encoded), *chunks])
    return struct.pack("<III", 0x46546C67, 2, len(body) + 12) + body


def image_name(document, texture_index):
    texture = document["textures"][texture_index]
    image_index = texture.get("source", texture.get("extensions", {}).get("EXT_texture_webp", {}).get("source"))
    return document["images"][image_index]["name"]


def repair(entry, apply):
    path = (ROOT / entry["asset"]).resolve()
    if not path.is_relative_to(ROOT / "models"):
        raise ValueError("Repair asset must stay inside models/")
    original = path.read_bytes()
    document, chunks = read_glb(original)
    matches = [i for i, material in enumerate(document["materials"]) if material.get("name") == entry["material"]]
    if len(matches) != 1:
        raise ValueError("Expected exactly one reviewed material")
    material_index = matches[0]
    material = document["materials"][material_index]
    texture = material["pbrMetallicRoughness"].get("baseColorTexture")
    targets = [i for i in range(len(document["textures"])) if image_name(document, i) == entry["albedoImage"]]
    if len(targets) != 1:
        raise ValueError("Expected one exact embedded albedo texture")
    primitives = [p for mesh in document["meshes"] for p in mesh["primitives"] if p.get("material") == material_index]
    if not primitives or any("TEXCOORD_0" not in p["attributes"] for p in primitives):
        raise ValueError("Reviewed surfaces must retain their original UVs")
    current = image_name(document, texture["index"]) if texture else None
    if current == entry["albedoImage"]:
        if hashlib.sha256(original).hexdigest() != entry["repairedSha256"]:
            raise ValueError("Corrected asset differs from the verified repair")
        return {"asset": entry["asset"], "state": "correct", "sha256": hashlib.sha256(original).hexdigest()}
    if current != entry["incorrectImage"] or hashlib.sha256(original).hexdigest() != entry["originalSha256"]:
        raise ValueError("Source differs from the reviewed original; inspect before changing it")
    if not apply:
        raise ValueError("Reviewed correction is not applied; use --apply")
    before = copy.deepcopy(document)
    if texture is None:
        material["pbrMetallicRoughness"]["baseColorTexture"] = {"index": targets[0]}
    else:
        texture["index"] = targets[0]
    if entry.get("resetBaseColorFactor"):
        material["pbrMetallicRoughness"]["baseColorFactor"] = [1, 1, 1, 1]
    # Independently verify only the reviewed binding and optional tint changed.
    expected = copy.deepcopy(before)
    expected_pbr = expected["materials"][material_index]["pbrMetallicRoughness"]
    expected_pbr.setdefault("baseColorTexture", {})["index"] = targets[0]
    if entry.get("resetBaseColorFactor"):
        expected_pbr["baseColorFactor"] = [1, 1, 1, 1]
    assert document == expected
    result = write_glb(document, chunks)
    actual, actual_chunks = read_glb(result)
    assert actual == expected and actual_chunks == chunks
    if hashlib.sha256(result).hexdigest() != entry["repairedSha256"]:
        raise ValueError("Repair output differs from the verified bytes")
    backup = ROOT / "native/artifacts/model-material-originals" / path.name
    backup.parent.mkdir(parents=True, exist_ok=True)
    if backup.exists() and backup.read_bytes() != original:
        raise ValueError("An existing original backup has different bytes")
    backup.write_bytes(original)
    path.write_bytes(result)
    return {"asset": entry["asset"], "state": "repaired", "originalSha256": entry["originalSha256"],
            "sha256": hashlib.sha256(result).hexdigest(), "preservedBinaryChunks": True,
            "material": entry["material"], "albedoImage": entry["albedoImage"]}


def alpha_repair(entry, apply):
    """Restore only reviewed embedded cutouts; never infer alpha from a color."""
    path = (ROOT / entry["asset"]).resolve()
    if not path.is_relative_to(ROOT / "models"):
        raise ValueError("Repair asset must stay inside models/")
    original = path.read_bytes()
    actual_hash = hashlib.sha256(original).hexdigest()
    if actual_hash == entry["repairedSha256"]:
        return {"asset": entry["asset"], "state": "correct", "sha256": actual_hash}
    if actual_hash != entry["originalSha256"]:
        raise ValueError("Cutout asset differs from its reviewed original")
    document, chunks = read_glb(original)
    matches = [material for material in document["materials"] if material.get("name") == entry["material"]]
    if len(matches) != 1:
        raise ValueError("Expected exactly one reviewed cutout material")
    material = matches[0]
    if material.get("alphaMode") != entry.get("originalAlphaMode") or "alphaCutoff" in material:
        raise ValueError("Original cutout mode differs from reviewed mode")
    texture = material.get("pbrMetallicRoughness", {}).get("baseColorTexture")
    if not texture or image_name(document, texture["index"]) != entry["albedoImage"]:
        raise ValueError("Cutout diffuse binding differs from reviewed image")
    before = copy.deepcopy(document)
    if "originalBaseColorAlpha" in entry:
        factor = material["pbrMetallicRoughness"].get("baseColorFactor")
        if not factor or factor[3] != entry["originalBaseColorAlpha"]:
            raise ValueError("Original opacity differs from reviewed factor")
        factor[3] = 1
    material["alphaMode"] = "MASK"
    material["alphaCutoff"] = .5
    expected = copy.deepcopy(before)
    expected_material = next(m for m in expected["materials"] if m.get("name") == entry["material"])
    expected_material.update(alphaMode="MASK", alphaCutoff=.5)
    if "originalBaseColorAlpha" in entry:
        expected_material["pbrMetallicRoughness"]["baseColorFactor"][3] = 1
    result = write_glb(document, chunks)
    actual, actual_chunks = read_glb(result)
    if actual != expected or actual_chunks != chunks:
        raise ValueError("Cutout repair changed unrelated GLB data")
    if hashlib.sha256(result).hexdigest() != entry["repairedSha256"]:
        raise ValueError("Cutout output differs from verified repair")
    if not apply:
        raise ValueError("Reviewed cutout correction is not applied; use --apply")
    backup = ROOT / "native/artifacts/model-material-originals" / path.name
    backup.parent.mkdir(parents=True, exist_ok=True)
    if backup.exists() and backup.read_bytes() != original:
        raise ValueError("Existing original backup has different bytes")
    backup.write_bytes(original)
    path.write_bytes(result)
    return {"asset": entry["asset"], "state": "repaired", "sha256": entry["repairedSha256"],
            "preservedBinaryChunks": True, "alphaMode": "MASK", "alphaCutoff": .5}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    operation = parser.add_mutually_exclusive_group()
    operation.add_argument("--apply", action="store_true")
    operation.add_argument("--check", action="store_true")
    options = parser.parse_args()
    manifest = json.loads(MANIFEST.read_text())
    rows = [repair(entry, options.apply) for entry in manifest["repairs"]]
    rows += [alpha_repair(entry, options.apply) for entry in manifest.get("alphaRepairs", [])]
    for entry in (manifest.get("sourceRecoveries", []) + manifest.get("geometryRecoveries", []) +
                  manifest.get("uvRecoveries", []) + manifest.get("eyeDecalRecoveries", [])):
        path = (ROOT / entry["asset"]).resolve()
        if not path.is_relative_to(ROOT / "models") or hashlib.sha256(path.read_bytes()).hexdigest() != entry["repairedSha256"]:
            raise ValueError("Recovered source asset differs from its verified repair: " + entry["asset"])
        rows.append({"asset": entry["asset"], "state": "correct", "sha256": entry["repairedSha256"]})
    mappings = json.loads((ROOT / "models/index.json").read_text())
    portraits = json.loads((ROOT / "models/portraits/index.json").read_text())
    for entry in manifest.get("mappingCorrections", []):
        path = (ROOT / entry["asset"]).resolve()
        portrait = portraits.get(entry["sourceKey"], {})
        portrait_path = (ROOT / "models/portraits" / entry["portrait"]).resolve()
        if (not path.is_relative_to(ROOT / "models") or mappings.get(entry["sourceKey"]) != entry["asset"] or
                hashlib.sha256(path.read_bytes()).hexdigest() != entry["repairedSha256"] or
                portrait.get("src") != entry["asset"] or portrait.get("file") != entry["portrait"] or
                not portrait_path.is_relative_to(ROOT / "models/portraits") or
                hashlib.sha256(portrait_path.read_bytes()).hexdigest() != entry["portraitSha256"]):
            raise ValueError("Reviewed model/portrait mapping differs: " + entry["sourceKey"])
        rows.append({"source": entry["sourceKey"], "asset": entry["asset"], "state": "correct"})
    print(json.dumps({"schemaVersion": 1, "repairs": rows}, indent=2))


if __name__ == "__main__":
    main()
