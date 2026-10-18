"""Recover a reviewed OBJ diffuse image and missing UVs as a candidate only.

Requires numpy/scipy, Node, and the local meshopt decoder. Source OBJ triangles
must map uniquely and their UVs must agree at every surviving vertex; this tool
never changes topology or existing attributes to invent a correspondence.
"""
import argparse
import copy
import importlib.util
import io
import json
from pathlib import Path
import zipfile

import numpy as np
from scipy.spatial import cKDTree

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("material_recovery", Path(__file__).with_name("recover-model-materials.py"))
support = importlib.util.module_from_spec(spec)
spec.loader.exec_module(support)
RECOVERIES = {
    "dw1_tsunomon": {
        "asset": "models/files/rip_dw1_tsunomon.glb",
        "originalSha256": "0b33e8f6145211a6634148cdf72c0a317c4829c191aa9d05993f9b121b673f4a",
        "archiveSha256": "d13c6a4a9eb27c75c6814613cfa2ffeedd17ffc05a47437b4b1c7db64173ebae",
        "repairedSha256": "e7b4d0913c480cccd6578e3aaf37d3b6c614788bb13c2fdfa66beca3674771ce",
        "sourceUrl": "https://models.spriters-resource.com/playstation/digimonworld/asset/334142/",
        "obj": "Tsunomon.obj", "mtl": "Tsunomon.mtl", "missingMtllib": "untitled.mtl",
        "objSha256": "7e19c7e6ae797dcef74326b5d6d30fb9fc2b7af0ade9847ab5a6ce64f4fd1165",
        "mtlSha256": "f73f1130717155a96fdcc0023d49f89aee977e78d310591c60e6c96de0101cc6",
        "atlas": "Tsunomon.png",
        "atlasSha256": "6d2003398dad3fa8a9d2fa02d61dcd987bfd08b87c2ad08e7a4fa574688051b1",
        "mesh": "Tsunomon-Mesh", "material": "VRAM_page_#5", "triangles": 150
    }
}


def recover(archive_data, original, recipe):
    if support.digest(archive_data) != recipe["archiveSha256"] or support.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    obj, mtl, atlas = (archive.read(recipe[key]) for key in ("obj", "mtl", "atlas"))
    if any(support.digest(value) != recipe[key + "Sha256"]
           for key, value in (("obj", obj), ("mtl", mtl), ("atlas", atlas))):
        raise ValueError("Source member differs from reviewed bytes")
    # A '#' within this source's material name is literal, not a comment.
    records = [line.strip().split() for line in obj.decode("utf-8").splitlines()
               if line.strip() and not line.lstrip().startswith("#")]
    mtllibs = [" ".join(row[1:]) for row in records if row[0] == "mtllib"]
    materials = {" ".join(row[1:]) for row in records if row[0] == "usemtl"}
    if (mtllibs != [recipe["missingMtllib"]] or recipe["missingMtllib"] in archive.namelist()
            or materials != {recipe["material"]}):
        raise ValueError("Source OBJ material reference differs")
    mtl_records = [line.strip().split() for line in mtl.decode("utf-8").splitlines()
                   if line.strip() and not line.lstrip().startswith("#")]
    if ([" ".join(row[1:]) for row in mtl_records if row[0] == "newmtl"] != [recipe["material"]]
            or [" ".join(row[1:]) for row in mtl_records if row[0] == "map_Kd"] != [recipe["atlas"]]):
        raise ValueError("Source MTL diffuse binding differs")
    diffuse = [list(map(float, row[1:])) for row in mtl_records if row[0] == "Kd"]
    opacity = [float(row[1]) for row in mtl_records if row[0] == "d"]
    if diffuse != [[1.0, 1.0, 1.0]] or opacity != [1.0]:
        raise ValueError("Source diffuse factor differs")
    positions = np.asarray([list(map(float, row[1:])) for row in records if row[0] == "v"])
    texcoords = np.asarray([list(map(float, row[1:])) for row in records if row[0] == "vt"])
    face_rows = [row[1:] for row in records if row[0] == "f"]
    if len(face_rows) != recipe["triangles"] or any(len(row) != 3 for row in face_rows):
        raise ValueError("Source triangle count differs")
    faces = np.asarray([[list(map(int, corner.split("/"))) for corner in row] for row in face_rows]) - 1
    if faces.shape != (recipe["triangles"], 3, 3) or np.any(faces < 0) or texcoords.shape[1] != 2:
        raise ValueError("Expected positive OBJ position/UV/normal indices")
    document, chunks = support.glb.read_glb(original)
    decoded, decoded_chunks = support.glb.read_glb(support.decode_original(original))
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942 or len(document["meshes"]) != 1:
        raise ValueError("Unexpected source GLB layout")
    before = copy.deepcopy(document)
    decoded_data = decoded_chunks[0][1]
    mesh = document["meshes"][0]
    if mesh["name"] != recipe["mesh"] or len(mesh["primitives"]) != 1:
        raise ValueError("Reviewed mesh differs")
    primitive = mesh["primitives"][0]
    if (primitive.get("mode", 4) != 4 or "TEXCOORD_0" in primitive["attributes"]
            or document.get("images") or document.get("textures") or document.get("samplers")):
        raise ValueError("Expected missing original texture and UVs")
    points = support.accessor(decoded, decoded_data, primitive["attributes"]["POSITION"], True)
    indices = support.accessor(decoded, decoded_data, primitive["indices"]).ravel().astype(int)
    if len(indices) != recipe["triangles"] * 3:
        raise ValueError("Current triangle count differs")
    center = (positions.min(0) + positions.max(0)) / 2
    scale = np.ptp(positions, 0).max() / 2
    distances, nearest = cKDTree(points).query((positions - center) / scale)
    if distances.max() > 0.00015:
        raise ValueError("Source positions differ")
    keys = [tuple(point) for point in np.round(points * 32767).astype(int)]
    triangles = {}
    for face in faces:
        corners = [keys[nearest[index]] for index in face[:, 0]]
        key = tuple(sorted(corners))
        if key in triangles or len(set(corners)) != 3:
            raise ValueError("Source triangle is ambiguous")
        triangles[key] = (corners, texcoords[face[:, 1]])
    restored = np.zeros((len(points), 2), dtype="<f4")
    assigned = set()
    for face in indices.reshape(-1, 3):
        corners = [keys[index] for index in face]
        source_triangle = triangles.pop(tuple(sorted(corners)), None)
        if source_triangle is None:
            raise ValueError("Source triangle does not map one-to-one")
        source_corners, source_uvs = source_triangle
        for index, corner in zip(face, corners):
            uv = source_uvs[source_corners.index(corner)].copy()
            uv[1] = 1 - uv[1]
            uv = uv.astype("<f4")
            if index in assigned and not np.array_equal(restored[index], uv):
                raise ValueError("Source UV seam requires a topology change")
            restored[index] = uv
            assigned.add(index)
    if triangles or len(assigned) != len(points):
        raise ValueError("Source correspondence is incomplete")
    data = bytearray(chunks[0][1])

    def append_view(blob):
        data.extend(b"\0" * (-len(data) % 4))
        index = len(document["bufferViews"])
        document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": len(blob)})
        data.extend(blob)
        return index

    uv_index = len(document["accessors"])
    document["accessors"].append({"bufferView": append_view(restored.tobytes()), "componentType": 5126,
                                  "count": len(points), "type": "VEC2"})
    primitive["attributes"]["TEXCOORD_0"] = uv_index
    document["images"] = [{"name": recipe["atlas"], "mimeType": "image/png", "bufferView": append_view(atlas)}]
    document["textures"] = [{"source": 0, "sampler": 0}]
    document["samplers"] = [{"wrapS": 10497, "wrapT": 10497}]
    material = document["materials"][primitive["material"]]
    if len(document["materials"]) != 1 or material.get("name") != recipe["material"]:
        raise ValueError("Current material differs")
    material["pbrMetallicRoughness"].update(baseColorTexture={"index": 0}, baseColorFactor=diffuse[0] + opacity)
    document["buffers"][0]["byteLength"] = len(data)
    data.extend(b"\0" * (-len(data) % 4))
    result = support.glb.write_glb(document, [(0x004E4942, bytes(data))])
    if support.digest(result) != recipe["repairedSha256"]:
        raise ValueError("Repair output differs from the reviewed bytes")
    after_document, after_chunks = support.glb.read_glb(result)
    if after_document != document or after_chunks[0][1][:len(chunks[0][1])] != chunks[0][1]:
        raise ValueError("Serialized data differs")
    after, decoded_chunks = support.glb.read_glb(support.decode_original(result))
    for index in range(len(decoded["accessors"])):
        if (support.accessor(decoded, decoded_data, index).tobytes()
                != support.accessor(after, decoded_chunks[0][1], index).tobytes()):
            raise ValueError("Existing accessor changed")
    if support.accessor(after, decoded_chunks[0][1], uv_index).tobytes() != restored.tobytes():
        raise ValueError("Recovered UV serialization differs")
    expected_metadata = copy.deepcopy(after_document)
    expected_metadata["buffers"][0]["byteLength"] = before["buffers"][0]["byteLength"]
    del expected_metadata["bufferViews"][len(before["bufferViews"]):]
    del expected_metadata["accessors"][len(before["accessors"]):]
    del expected_metadata["meshes"][0]["primitives"][0]["attributes"]["TEXCOORD_0"]
    pbr = expected_metadata["materials"][primitive["material"]]["pbrMetallicRoughness"]
    del pbr["baseColorTexture"]
    pbr["baseColorFactor"] = before["materials"][primitive["material"]]["pbrMetallicRoughness"]["baseColorFactor"]
    for key in ("images", "textures", "samplers"):
        expected_metadata.pop(key)
    if expected_metadata != before:
        raise ValueError("Unrelated metadata changed")
    return result, {
        "schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
        "repairedSha256": support.digest(result), "archiveSha256": recipe["archiveSha256"],
        "sourceUrl": recipe["sourceUrl"], "objSha256": support.digest(obj),
        "mtlSha256": support.digest(mtl), "atlasSha256": support.digest(atlas),
        "missingSourceMaterialLibrary": recipe["missingMtllib"], "recoveredMaterialLibrary": recipe["mtl"],
        "sourceDiffuseBinding": recipe["material"] + " -> " + recipe["atlas"],
        "matchedTriangles": len(indices) // 3, "maximumNormalizedPositionError": float(distances.max()),
        "restoredUvVertices": len(restored), "uvSeamConflicts": 0,
        "uvTransform": "U=sourceU, V=1-sourceV", "preservedTopology": True,
        "preservedOriginalBinaryBytes": True, "preservedExistingAccessorsBitForBit": True,
        "preservedRigAnimationAndOtherMetadata": True
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path)
    parser.add_argument("--output-dir", type=Path, default=ROOT / "native/artifacts/material-candidates-tsuno-rave")
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
