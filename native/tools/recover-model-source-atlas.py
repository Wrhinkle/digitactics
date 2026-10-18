"""Append a pinned source diffuse atlas after verifying all surviving UVs.

Candidate-only tool: retains original binary bytes, all accessors, geometry,
rig, animations, image bytes, and material settings except the reviewed diffuse
texture index. Requires numpy/scipy, Node, and the repository meshopt decoder.
"""
import argparse
import copy
import importlib.util
import io
import json
from pathlib import Path
import struct
import zipfile
import xml.etree.ElementTree as ET

import numpy as np
from scipy.spatial import cKDTree

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("material_recovery", Path(__file__).with_name("recover-model-materials.py"))
support = importlib.util.module_from_spec(spec)
spec.loader.exec_module(support)
NS = support.NS
RECOVERIES = {
    "sukamon": {
        "asset": "models/files/rip_sukamon.glb",
        "originalSha256": "3f3b60ad528f4770ed3c8e507362d6ece9266f6277f7f324bb7738468eb96991",
        "archiveSha256": "e3494921b9789f33dddb7b0a8f58c273563048adddb19c995e00fa3fb3dc3ac6",
        "repairedSha256": "89e9f909f47e913ec86f83e8e508264eebdaf483f51da9ff346a5f11b033ede4",
        "sourceUrl": "https://models.spriters-resource.com/mobile/digimonlinks/asset/304086/",
        "prefix": "Sukamon PlatinumSukamon/",
        "dae": "chr313.dae", "daeSha256": "ff028645a3ddcfef1e8021b75e1159dc2cd50ea2431c3bce967e33ef3b718c66",
        "atlas": "chr313a01.png", "atlasSha256": "794a9e5a9e82200c71fcdea3953c28265fcb4a9482374019c008b36a4e352706",
        "oldAtlas": "chr752a01.png", "oldAtlasSha256": "67a3c264bfc83d554177870aa2fde5d242427ee0c0d8a066c74f8f6ae698b446",
        "mesh": "chr313_0", "sourceGeometry": "chr313", "sourceUvInput": "GeometrySource2",
        "triangles": 2604, "material": 0, "oldTexture": 0, "oldImage": 0
    }
}


def verify_source_uvs(source, document, data, recipe):
    geometry = source.find(".//c:library_geometries/c:geometry[@name='" + recipe["sourceGeometry"] + "']", NS)
    mesh = geometry.find("c:mesh", NS)
    triangles = mesh.find("c:triangles", NS)
    inputs = triangles.findall("c:input", NS)
    if len(inputs) != 1 or inputs[0].get("semantic") != "VERTEX" or inputs[0].get("offset") != "0":
        raise ValueError("Source no longer has the reviewed interleaved vertex layout")
    vertices = mesh.find("c:vertices[@id='" + inputs[0].get("source")[1:] + "']", NS)
    uv_inputs = [value for value in vertices.findall("c:input", NS) if value.get("semantic") == "TEXCOORD"]
    if len(uv_inputs) != 1 or uv_inputs[0].get("source") != "#" + recipe["sourceUvInput"]:
        raise ValueError("Source UV input is missing or ambiguous")
    symbol = triangles.get("material")
    bindings = source.findall(".//c:instance_material[@symbol='" + symbol + "']", NS)
    targets = {binding.get("target") for binding in bindings}
    if len(targets) != 1:
        raise ValueError("Source material symbol is ambiguous")
    material_id = next(iter(targets))[1:]
    material = source.find(".//c:library_materials/c:material[@id='" + material_id + "']", NS)
    effect_id = material.find("c:instance_effect", NS).get("url")[1:]
    effect = source.find(".//c:library_effects/c:effect[@id='" + effect_id + "']", NS)
    diffuse = effect.find(".//c:diffuse/c:texture", NS)
    sampler = effect.find(".//c:newparam[@sid='" + diffuse.get("texture") + "']/c:sampler2D/c:source", NS).text
    surface = effect.find(".//c:newparam[@sid='" + sampler + "']/c:surface/c:init_from", NS).text
    image = source.find(".//c:library_images/c:image[@id='" + surface + "']/c:init_from", NS).text
    if image != recipe["atlas"] or diffuse.get("texcoord") not in (None, ""):
        raise ValueError("Source diffuse atlas or reviewed UV declaration differs")
    arrays = {value.get("id"): np.fromstring(value.find("c:float_array", NS).text, sep=" ").reshape(
        -1, int(value.find("c:technique_common/c:accessor", NS).get("stride")))
        for value in mesh.findall("c:source", NS)}
    position_input = next(value for value in vertices.findall("c:input", NS) if value.get("semantic") == "POSITION")
    positions = arrays[position_input.get("source")[1:]]
    uv = arrays[recipe["sourceUvInput"]]
    faces = np.fromstring(triangles.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3)
    if len(document["meshes"]) != 1 or document["meshes"][0]["name"] != recipe["mesh"]:
        raise ValueError("Reviewed mesh differs")
    primitives = document["meshes"][0]["primitives"]
    if len(primitives) != 1 or primitives[0].get("mode", 4) != 4:
        raise ValueError("Expected one triangle primitive")
    primitive = primitives[0]
    points = support.accessor(document, data, primitive["attributes"]["POSITION"], True)
    existing_uv = support.accessor(document, data, primitive["attributes"]["TEXCOORD_0"], True)
    indices = support.accessor(document, data, primitive["indices"]).ravel().astype(int)
    if len(indices) != recipe["triangles"] * 3 or len(faces) != recipe["triangles"]:
        raise ValueError("Source triangle count differs")
    center = (positions.min(0) + positions.max(0)) / 2
    scale = np.ptp(positions, 0).max() / 2
    distances, nearest = cKDTree(points).query((positions - center) / scale)
    if distances.max() > 0.00015:
        raise ValueError("Source positions differ")
    keys = [tuple(point) for point in np.round(points * 32767).astype(int)]
    source_triangles = {}
    for face in faces:
        corners = [keys[nearest[index]] for index in face]
        key = tuple(sorted(corners))
        if key in source_triangles or len(set(corners)) != 3:
            raise ValueError("Source triangles are ambiguous")
        source_triangles[key] = (corners, uv[face])
    max_uv_error = 0.0
    for face in indices.reshape(-1, 3):
        corners = [keys[index] for index in face]
        match = source_triangles.pop(tuple(sorted(corners)), None)
        if match is None:
            raise ValueError("Source triangle does not map one-to-one")
        source_corners, source_uv = match
        canonical = np.asarray([source_uv[source_corners.index(corner)] for corner in corners])
        canonical[:, 1] = 1 - canonical[:, 1]
        max_uv_error = max(max_uv_error, float(np.max(np.abs(existing_uv[face] - canonical))))
    if source_triangles or max_uv_error > 0.00015:
        raise ValueError("Surviving UVs differ from the source diffuse layout")
    return {"matchedTriangles": len(faces), "maximumNormalizedPositionError": float(distances.max()),
            "maximumUvError": max_uv_error, "sourceMaterialId": material_id,
            "sourceMaterialName": material.get("name"), "sourceDiffuse": image,
            "sourceUvBinding": "sole interleaved vertex TEXCOORD; no competing UV sets",
            "sourceUvInput": recipe["sourceUvInput"], "sourceToGltfUv": "U=sourceU, V=1-sourceV"}


def recover(archive_data, original, recipe):
    if support.digest(archive_data) != recipe["archiveSha256"] or support.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    dae, atlas, old_atlas = (archive.read(recipe["prefix"] + recipe[key]) for key in ("dae", "atlas", "oldAtlas"))
    if any(support.digest(value) != recipe[key + "Sha256"]
           for key, value in (("dae", dae), ("atlas", atlas), ("oldAtlas", old_atlas))):
        raise ValueError("Source member differs from reviewed bytes")
    if struct.unpack_from(">II", atlas, 16) != (512, 512) or struct.unpack_from(">II", old_atlas, 16) != (512, 512):
        raise ValueError("Reviewed atlas dimensions differ")
    document, chunks = support.glb.read_glb(original)
    decoded, decoded_chunks = support.glb.read_glb(support.decode_original(original))
    proof = verify_source_uvs(ET.fromstring(dae), decoded, decoded_chunks[0][1], recipe)
    before = copy.deepcopy(document)
    primitive = document["meshes"][0]["primitives"][0]
    pbr = document["materials"][recipe["material"]]["pbrMetallicRoughness"]
    if (primitive["material"] != recipe["material"] or pbr["baseColorTexture"] != {"index": recipe["oldTexture"]}
            or document["images"][recipe["oldImage"]]["name"] != recipe["oldAtlas"]
            or document["textures"][recipe["oldTexture"]]["extensions"]["EXT_texture_webp"]["source"] != recipe["oldImage"]):
        raise ValueError("Original diffuse binding differs")
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one original binary chunk")
    data = bytearray(chunks[0][1])
    data.extend(b"\0" * (-len(data) % 4))
    view = len(document["bufferViews"])
    document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": len(atlas)})
    data.extend(atlas)
    new_image = len(document["images"])
    document["images"].append({"name": recipe["atlas"], "mimeType": "image/png", "bufferView": view})
    new_texture = len(document["textures"])
    document["textures"].append({"source": new_image, "sampler": document["textures"][recipe["oldTexture"]]["sampler"]})
    pbr["baseColorTexture"] = {"index": new_texture}
    document["buffers"][0]["byteLength"] = len(data)
    data.extend(b"\0" * (-len(data) % 4))
    result = support.glb.write_glb(document, [(0x004E4942, bytes(data))])
    if support.digest(result) != recipe["repairedSha256"]:
        raise ValueError("Repair output differs from the reviewed bytes")
    after_document, after_chunks = support.glb.read_glb(result)
    if after_document != document or after_chunks[0][1][:len(chunks[0][1])] != chunks[0][1]:
        raise ValueError("Serialized original metadata or binary content changed")
    check = copy.deepcopy(after_document)
    check["buffers"][0]["byteLength"] = before["buffers"][0]["byteLength"]
    for key in ("bufferViews", "images", "textures"):
        del check[key][len(before[key]):]
    check["materials"][recipe["material"]]["pbrMetallicRoughness"]["baseColorTexture"] = {"index": recipe["oldTexture"]}
    if check != before:
        raise ValueError("Unrelated metadata changed")
    after, after_decoded_chunks = support.glb.read_glb(support.decode_original(result))
    for index in range(len(decoded["accessors"])):
        if (support.accessor(decoded, decoded_chunks[0][1], index).tobytes()
                != support.accessor(after, after_decoded_chunks[0][1], index).tobytes()):
            raise ValueError("Existing accessor changed")
    return result, {"schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
                    "repairedSha256": support.digest(result), "archiveSha256": recipe["archiveSha256"],
                    "sourceUrl": recipe["sourceUrl"], "daeSha256": support.digest(dae),
                    "albedoSha256": support.digest(atlas), "replacedBinding": recipe["oldAtlas"] + " -> " + recipe["atlas"],
                    "preservedOriginalBinaryBytes": True, "preservedAllAccessorsBitForBit": True,
                    "preservedRigAnimationsMaterialsExceptDiffuseIndex": True,
                    "verifiedAccessors": len(decoded["accessors"]), "sourceProof": proof}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path)
    parser.add_argument("--output-dir", type=Path, default=ROOT / "native/artifacts/material-candidates-sera-suka")
    options = parser.parse_args()
    output = options.output_dir.resolve()
    if not output.is_relative_to((ROOT / "native/artifacts").resolve()):
        raise ValueError("Candidate output must stay under native/artifacts")
    recipe = RECOVERIES[options.model]
    result, report = recover(options.archive.read_bytes(), (options.original or ROOT / recipe["asset"]).read_bytes(), recipe)
    output.mkdir(parents=True, exist_ok=True)
    (output / (options.model + ".candidate.glb")).write_bytes(result)
    (output / (options.model + ".candidate.json")).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
