"""Recover pinned original FBX vertex RGBA into an isolated GLB candidate.

Requires numpy/scipy, Node, and the repository meshopt decoder. This tool never
applies candidates. Only the four reviewed MachGaogamon sonic COLOR_0 streams
and their shared material's alphaMode are added; glow cards remain untouched.
"""
import argparse
import copy
import importlib.util
import io
import itertools
import json
from pathlib import Path
import struct
import zipfile
import zlib

import numpy as np
from scipy.spatial import cKDTree

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("material_recovery", Path(__file__).with_name("recover-model-materials.py"))
support = importlib.util.module_from_spec(spec)
spec.loader.exec_module(support)
RECOVERIES = {
    "machgaogamon": {
        "asset": "models/files/rip_machgaogamon.glb",
        "originalSha256": "de6be2d027173db22c670a7c03f69218991f56f079c7be644fa5340079ee9933",
        "archiveSha256": "eb881926c655c791b72ff330212b2411b41908833a0ae852343a67ebab6f169c",
        "repairedSha256": "a787dd3b90f7df49b3d18608618ea4368e3dfd43806ee845174f4ebfc6fe9e07",
        "sourceUrl": "https://models.spriters-resource.com/mobile/digimonlinks/asset/304020/",
        "fbx": "MachGaogamon/chr074 With Animations.fbx",
        "fbxSha256": "78c3ae259885048e99428a5d331019e3fb422fcb40b6067094dc40ca11352992",
        "material": 2, "sourceMaterialId": 715300048,
        "sourceMaterialName": "EFF_jet_sonic_mat", "sourceDiffuse": "eff_fir_01a.png",
        "meshes": {
            "EFF_jet_R_sonic_0": {"geometryId": 716507792, "modelId": 717933776},
            "EFF_jet_R_sonic1_0": {"geometryId": 716163264, "modelId": 717935776},
            "EFF_jet_L_sonic_0": {"geometryId": 716164800, "modelId": 721366832},
            "EFF_jet_L_sonic1_0": {"geometryId": 716165312, "modelId": 721368832}
        }
    }
}


def parse_fbx(data):
    """Read the pinned binary FBX 7400 node/property format, without conversion."""
    if data[:23] != b"Kaydara FBX Binary  \0\x1a\0" or struct.unpack_from("<I", data, 23)[0] != 7400:
        raise ValueError("Expected reviewed binary FBX 7400")

    def property_at(offset):
        kind = chr(data[offset])
        offset += 1
        scalar_types = {"Y": "h", "C": "?", "I": "i", "F": "f", "D": "d", "L": "q"}
        if kind in scalar_types:
            fmt = "<" + scalar_types[kind]
            return struct.unpack_from(fmt, data, offset)[0], offset + struct.calcsize(fmt)
        if kind in "SR":
            size = struct.unpack_from("<I", data, offset)[0]
            raw = data[offset + 4:offset + 4 + size]
            return (raw.decode("utf-8") if kind == "S" else raw), offset + 4 + size
        if kind in "fdlibc":
            count, encoding, size = struct.unpack_from("<III", data, offset)
            if encoding not in (0, 1):
                raise ValueError("Unsupported FBX array encoding")
            raw = data[offset + 12:offset + 12 + size]
            raw = zlib.decompress(raw) if encoding else raw
            array = np.frombuffer(raw, dtype={"f": "<f4", "d": "<f8", "l": "<i8", "i": "<i4", "b": "u1", "c": "u1"}[kind])
            if len(array) != count:
                raise ValueError("FBX array length differs")
            return array, offset + 12 + size
        raise ValueError("Unsupported FBX property type " + kind)

    def node_at(offset):
        end, count, size, name_length = struct.unpack_from("<IIIB", data, offset)
        if not end:
            return None, offset + 13
        if end <= offset or end > len(data):
            raise ValueError("Invalid FBX node extent")
        name = data[offset + 13:offset + 13 + name_length].decode("utf-8")
        cursor = start = offset + 13 + name_length
        properties, children = [], []
        for _ in range(count):
            value, cursor = property_at(cursor)
            properties.append(value)
        if cursor - start != size:
            raise ValueError("FBX property extent differs")
        while cursor < end - 13:
            child, cursor = node_at(cursor)
            if child is not None:
                children.append(child)
        if cursor not in (end, end - 13):
            raise ValueError("FBX child extent differs")
        return {"name": name, "properties": properties, "children": children}, end

    nodes, offset = [], 27
    while struct.unpack_from("<I", data, offset)[0]:
        node, offset = node_at(offset)
        nodes.append(node)
    return {"children": nodes}


def child(node, name):
    matches = [item for item in node["children"] if item["name"] == name]
    if len(matches) != 1:
        raise ValueError("Missing or ambiguous FBX node " + name)
    return matches[0]


def value(node, name):
    return child(node, name)["properties"][0]


def source_meshes(fbx, recipe):
    root = parse_fbx(fbx)
    objects = {item["properties"][0]: item for item in child(root, "Objects")["children"]}
    connections = [item["properties"] for item in child(root, "Connections")["children"] if item["name"] == "C"]
    material = objects[recipe["sourceMaterialId"]]
    if material["name"] != "Material" or material["properties"][1].split("\0")[0] != recipe["sourceMaterialName"]:
        raise ValueError("Source material differs")
    texture_ids = {link[1] for link in connections if link[0] == "OP" and link[2:] == [recipe["sourceMaterialId"], "DiffuseColor"]}
    if len(texture_ids) != 1 or value(objects[next(iter(texture_ids))], "RelativeFilename") != recipe["sourceDiffuse"]:
        raise ValueError("Source diffuse texture binding differs")
    sources = {}
    for name, expected in recipe["meshes"].items():
        geometry, model = objects[expected["geometryId"]], objects[expected["modelId"]]
        if (geometry["name"] != "Geometry" or model["name"] != "Model"
                or model["properties"][1].split("\0")[0] != name
                or ["OO", expected["geometryId"], expected["modelId"]] not in connections
                or ["OO", recipe["sourceMaterialId"], expected["modelId"]] not in connections):
            raise ValueError("Reviewed source geometry/model/material connection differs")
        positions = value(geometry, "Vertices").reshape(-1, 3)
        indices = value(geometry, "PolygonVertexIndex").copy()
        if (positions.shape != (54, 3) or len(indices) != 240 or not np.all(indices[2::3] < 0)
                or not np.all(indices.reshape(-1, 3)[:, :2] >= 0)):
            raise ValueError("Source triangle layout differs")
        indices[indices < 0] = -indices[indices < 0] - 1
        if indices.min() < 0 or indices.max() >= len(positions):
            raise ValueError("Invalid source position index")
        arrays = []
        for kind, field, width in (("LayerElementUV", "UV", 2), ("LayerElementColor", "Colors", 4)):
            layer = child(geometry, kind)
            if (layer["properties"] != [0] or value(layer, "MappingInformationType") != "ByVertice"
                    or value(layer, "ReferenceInformationType") != "Direct"):
                raise ValueError("Source UV/color mapping differs or has competing sets")
            array = value(layer, field).reshape(-1, width)
            if array.shape != (54, width) or not np.isfinite(array).all():
                raise ValueError("Source UV/color stream differs")
            arrays.append(array)
        uv, colors = arrays
        expected_alpha = np.array([0, .250980406999588, .501960813999176, .7490196228027344, 1])
        alpha, counts = np.unique(colors[:, 3], return_counts=True)
        if (not np.all(colors[:, :3] == 1) or not np.array_equal(alpha, expected_alpha)
                or counts.tolist() != [18, 9, 9, 9, 9]):
            raise ValueError("Source RGBA values differ from reviewed white/fade stream")
        sources[name] = {"positions": positions, "faces": indices.reshape(-1, 3), "uv": uv, "colors": colors}
    return sources


def map_corners(source, points, indices, uv, axes=(0, 1, 2), signs=(1, 1, 1)):
    """Require unique source triangles and source UV agreement for every corner."""
    positions = source["positions"]
    normalized = (positions - (positions.min(0) + positions.max(0)) / 2) / (np.ptp(positions, 0).max() / 2)
    distances, nearest = cKDTree(points).query(normalized[:, axes] * signs)
    if distances.max() > .00015:
        raise ValueError("Source positions differ")
    keys = [tuple(point) for point in np.round(points * 32767).astype(int)]
    triangles = {}
    for face in source["faces"]:
        corners = [keys[nearest[index]] for index in face]
        if len(set(corners)) != 3:
            raise ValueError("Degenerate source triangle")
        triangles.setdefault(tuple(sorted(corners)), []).append((corners, face))
    assigned, max_uv_error = {}, 0.0
    for face in indices.reshape(-1, 3):
        corners = [keys[index] for index in face]
        key, matches = tuple(sorted(corners)), []
        for choice, (source_corners, source_face) in enumerate(triangles.get(key, [])):
            order = [source_corners.index(corner) for corner in corners]
            source_indices = source_face[order]
            canonical = source["uv"][source_indices].copy()
            canonical[:, 1] = 1 - canonical[:, 1]
            error = float(np.abs(canonical - uv[face]).max())
            if error <= .00015:
                matches.append((choice, source_indices, error))
        if len(matches) != 1:
            raise ValueError("Source position/UV triangle does not map uniquely")
        choice, source_indices, error = matches[0]
        triangles[key].pop(choice)
        max_uv_error = max(max_uv_error, error)
        for index, color in zip(face, source["colors"][source_indices].astype("<f4")):
            if index in assigned and not np.array_equal(assigned[index], color):
                raise ValueError("Unexpected color seam requires separate review")
            assigned[index] = color
    if any(triangles.values()) or set(assigned) != set(range(len(points))):
        raise ValueError("Source/current mesh coverage is incomplete")
    return np.array([assigned[index] for index in range(len(points))], dtype="<f4"), {
        "matchedTriangles": len(indices) // 3, "vertices": len(points), "duplicatedVertices": 0,
        "maximumNormalizedPositionError": float(distances.max()), "maximumUvError": max_uv_error}


def recover(archive_data, original, recipe):
    if support.digest(archive_data) != recipe["archiveSha256"] or support.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    with zipfile.ZipFile(io.BytesIO(archive_data)) as archive:
        fbx = archive.read(recipe["fbx"])
    if support.digest(fbx) != recipe["fbxSha256"]:
        raise ValueError("Source FBX differs from reviewed bytes")
    sources = source_meshes(fbx, recipe)
    document, chunks = support.glb.read_glb(original)
    decoded, decoded_chunks = support.glb.read_glb(support.decode_original(original))
    before = copy.deepcopy(document)
    material = document["materials"][recipe["material"]]
    if material["name"] != recipe["sourceMaterialName"] or "alphaMode" in material:
        raise ValueError("Original reviewed sonic material differs")
    users = [mesh["name"] for mesh in document["meshes"] for primitive in mesh["primitives"] if primitive.get("material") == recipe["material"]]
    if sorted(users) != sorted(sources):
        raise ValueError("Sonic material has unreviewed users")
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one original binary chunk")
    data, proofs, recovered = bytearray(chunks[0][1]), [], {}
    for mesh_index, mesh in enumerate(document["meshes"]):
        if mesh["name"] not in sources:
            continue
        if len(mesh["primitives"]) != 1:
            raise ValueError("Expected one reviewed triangle primitive")
        primitive = mesh["primitives"][0]
        if primitive.get("mode", 4) != 4 or "COLOR_0" in primitive["attributes"]:
            raise ValueError("Original triangle/color layout differs")
        points = support.accessor(decoded, decoded_chunks[0][1], primitive["attributes"]["POSITION"], True)
        indices = support.accessor(decoded, decoded_chunks[0][1], primitive["indices"]).ravel().astype(int)
        uv = support.accessor(decoded, decoded_chunks[0][1], primitive["attributes"]["TEXCOORD_0"], True)
        if len(points) != 54 or len(indices) != 240:
            raise ValueError("Original sonic vertex/triangle count differs")
        colors, proof = map_corners(sources[mesh["name"]], points, indices, uv)
        # Geometry is rotationally symmetric. UVs must select only identity out
        # of all signed axis permutations, rather than trusting nearest points.
        for axes in itertools.permutations(range(3)):
            for signs in itertools.product((-1, 1), repeat=3):
                if axes == (0, 1, 2) and signs == (1, 1, 1):
                    continue
                try:
                    map_corners(sources[mesh["name"]], points, indices, uv, axes, signs)
                except ValueError:
                    continue
                raise ValueError("Source transform remains ambiguous after UV comparison")
        data.extend(b"\0" * (-len(data) % 4))
        view, accessor = len(document["bufferViews"]), len(document["accessors"])
        document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": colors.nbytes, "target": 34962})
        document["accessors"].append({"bufferView": view, "componentType": 5126, "count": len(colors), "type": "VEC4"})
        data.extend(colors.tobytes())
        primitive["attributes"]["COLOR_0"] = accessor
        recovered[mesh_index] = colors
        alpha, counts = np.unique(colors[:, 3], return_counts=True)
        proof.update(mesh=mesh["name"], **recipe["meshes"][mesh["name"]],
                     alphaHistogram=dict(zip(map(str, alpha.tolist()), map(int, counts))), rgb=[1, 1, 1],
                     sourceColorBinding="LayerElementColor 0 / ByVertice / Direct",
                     sourceUvBinding="sole LayerElementUV 0 / ByVertice / Direct",
                     sourceToGltfUv="U=sourceU, V=1-sourceV", uniqueSignedAxisPermutation="identity")
        proofs.append(proof)
    material["alphaMode"] = "BLEND"
    document["buffers"][0]["byteLength"] = len(data)
    data.extend(b"\0" * (-len(data) % 4))
    result = support.glb.write_glb(document, [(0x004E4942, bytes(data))])
    if support.digest(result) != recipe["repairedSha256"]:
        raise ValueError("Repair output differs from the reviewed bytes")
    serialized, serialized_chunks = support.glb.read_glb(result)
    if serialized != document or serialized_chunks[0][1][:len(chunks[0][1])] != chunks[0][1]:
        raise ValueError("Serialized original metadata or binary content changed")
    check = copy.deepcopy(serialized)
    check["buffers"][0]["byteLength"] = before["buffers"][0]["byteLength"]
    for key in ("bufferViews", "accessors"):
        del check[key][len(before[key]):]
    for mesh_index in recovered:
        del check["meshes"][mesh_index]["primitives"][0]["attributes"]["COLOR_0"]
    del check["materials"][recipe["material"]]["alphaMode"]
    if check != before:
        raise ValueError("Unrelated metadata changed")
    after, after_chunks = support.glb.read_glb(support.decode_original(result))
    for index in range(len(decoded["accessors"])):
        if (support.accessor(decoded, decoded_chunks[0][1], index).tobytes()
                != support.accessor(after, after_chunks[0][1], index).tobytes()):
            raise ValueError("Existing accessor changed")
    for mesh_index, colors in recovered.items():
        color_index = after["meshes"][mesh_index]["primitives"][0]["attributes"]["COLOR_0"]
        if support.accessor(after, after_chunks[0][1], color_index).tobytes() != colors.tobytes():
            raise ValueError("Serialized source RGBA differs")
    return result, {"schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
                    "repairedSha256": support.digest(result), "archiveSha256": recipe["archiveSha256"],
                    "sourceUrl": recipe["sourceUrl"], "sourceFbx": recipe["fbx"], "fbxSha256": support.digest(fbx),
                    "changedMaterial": {"index": recipe["material"], "name": material["name"], "alphaMode": "BLEND"},
                    "preservedOriginalBinaryBytes": True, "preservedAllExistingAccessorsBitForBit": True,
                    "preservedGeometryUvsRigAnimationsImagesAndGlowCards": True,
                    "verifiedExistingAccessors": len(decoded["accessors"]), "sourceProof": proofs}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path)
    parser.add_argument("--output-dir", type=Path, default=ROOT / "native/artifacts/material-candidate-machgaogamon")
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
