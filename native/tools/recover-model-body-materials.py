"""Create a pinned, candidate-only MagnaGarurumon body-material recovery.

Uses the archive's corresponding chr678 DAE body surfaces, verified triangle by
triangle against imported chr675. Sword/effect surfaces are outside this repair.
Requires numpy/scipy, Node, and the repository meshopt decoder. Never applies.
"""
import argparse
import copy
import importlib.util
import io
import json
from pathlib import Path
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
    "magnagarurumon": {
        "asset": "models/files/rip_magnagarurumon.glb",
        "originalSha256": "b9ff4ebdbf1333bf1991997c282e11edf5e176b99b1a16269bbe889002e0feb5",
        "archiveSha256": "b3dc1ffd6880c6981d076e442a1def1c9372b315d1e201a3d4344044725fb99d",
        "repairedSha256": "4d58856d87b3c385779bcc7b407bc7af0f18fa2d867b5b83d6e8fc3f874722b8",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/331597/",
        "prefix": "MagnaGarurumon MagnaGarurumon (SV)/",
        "fbx": "chr675.fbx", "fbxSha256": "704512baa298369e64ec53f96ce631d6e8b1ba45b0c9837add0d77cb9fca8a94",
        "dae": "chr678.dae", "daeSha256": "373d813bbeedf9e5990dc4548a5e5e38da85e4dc4124f63ac09682e6e7c89a41",
        "atlases": {
            "chr675a01.png": "7a81a7d00bb28b88f7e999939ce261cb0b9e9701127396aa2c840b8f3e0ad403",
            "chr675a02.png": "79314126f19988d33a94eca27739d5dc77fe10c8b769729fb295debf18b60c1d"
        },
        "meshes": {
            "chr675_4": {"source": "chr678_4", "atlas": "chr675a02.png", "triangles": 2812, "material": 2, "restoreUv": False, "repairBinding": True},
            "chr675_5": {"source": "chr678_5", "atlas": "chr675a01.png", "triangles": 3072, "material": 3, "restoreUv": True, "repairBinding": True},
            "chr675_6": {"source": "chr678_6", "atlas": "chr675a02.png", "triangles": 990, "material": 3, "restoreUv": True, "repairBinding": True},
            "chr675_7": {"source": "chr678_7", "atlas": "chr675a02.png", "triangles": 902, "material": 4, "restoreUv": False, "repairBinding": True},
            "chr675_8": {"source": "chr678_8", "atlas": "chr675a01.png", "triangles": 1332, "material": 4, "restoreUv": False, "repairBinding": False},
            "chr675_9": {"source": "chr678_9", "atlas": "chr675a01.png", "triangles": 2192, "material": 2, "restoreUv": False, "repairBinding": True}
        }
    }
}


def source_mesh(source, recipe):
    geometries = source.findall(".//c:library_geometries/c:geometry[@name='" + recipe["source"] + "']", NS)
    if len(geometries) != 1:
        raise ValueError("Source geometry is missing or ambiguous")
    mesh = geometries[0].find("c:mesh", NS)
    polys = mesh.findall("c:polylist", NS)
    if len(polys) != 1 or any(value != "3" for value in polys[0].find("c:vcount", NS).text.split()):
        raise ValueError("Expected one source triangle list")
    poly = polys[0]
    material_id = poly.get("material")
    material = source.find(".//c:material[@id='" + material_id + "']", NS)
    effect = source.find(".//c:effect[@id='" + material.find("c:instance_effect", NS).get("url")[1:] + "']", NS)
    diffuse = effect.find(".//c:diffuse/c:texture", NS)
    sampler = effect.find(".//c:newparam[@sid='" + diffuse.get("texture") + "']/c:sampler2D/c:source", NS).text
    surface = effect.find(".//c:newparam[@sid='" + sampler + "']/c:surface/c:init_from", NS).text
    atlas = source.find(".//c:library_images/c:image[@id='" + surface + "']/c:init_from", NS).text
    if atlas != recipe["atlas"]:
        raise ValueError("Source diffuse atlas differs from reviewed binding")
    uv_input = support.diffuse_uv_input(source, poly, material_id, diffuse)
    if (uv_input.get("set", "0") != "0" or uv_input.get("source") != "#" + recipe["source"] + "-mesh-map-0"):
        raise ValueError("Source diffuse UV set differs")
    arrays = {item.get("id"): np.fromstring(item.find("c:float_array", NS).text, sep=" ").reshape(
        -1, int(item.find("c:technique_common/c:accessor", NS).get("stride"))) for item in mesh.findall("c:source", NS)}
    inputs = poly.findall("c:input", NS)
    faces = np.fromstring(poly.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3, 1 + max(int(item.get("offset")) for item in inputs))
    vertex = next(item for item in inputs if item.get("semantic") == "VERTEX")
    vertices = mesh.find("c:vertices[@id='" + vertex.get("source")[1:] + "']", NS)
    position = next(item for item in vertices.findall("c:input", NS) if item.get("semantic") == "POSITION")
    positions = arrays[position.get("source")[1:]]
    center, scale = (positions.min(0) + positions.max(0)) / 2, np.ptp(positions, 0).max() / 2
    positions = ((positions - center) / scale)[faces[:, :, int(vertex.get("offset"))]]
    uv = arrays[uv_input.get("source")[1:]][faces[:, :, int(uv_input.get("offset"))]].copy()
    uv[:, :, 1] = 1 - uv[:, :, 1]
    if len(faces) != recipe["triangles"] or not np.isfinite(positions).all() or not np.isfinite(uv).all():
        raise ValueError("Source body triangle stream differs")
    return positions, uv, {"sourceGeometry": recipe["source"], "sourceDiffuse": atlas,
                           "sourceMaterial": material_id, "sourceUvInput": uv_input.get("source")[1:],
                           "sourceUvSet": 0, "sourceToGltfUv": "U=sourceU, V=1-sourceV"}


def match_triangles(source_positions, source_uv, points, indices, existing_uv):
    """Match complete triangles directly, retaining winding at coincident faces."""
    if len(indices) != len(source_positions) * 3:
        raise ValueError("Source/current triangle count differs")
    tree = cKDTree(source_positions.mean(1))
    used, restored, position_error, uv_error, positional_choices = set(), [], 0.0, 0.0, 0
    for triangle, face in enumerate(indices.reshape(-1, 3)):
        current = points[face]
        candidates, spatial_count = [], 0
        for source_index in tree.query_ball_point(current.mean(0), .00015):
            # Source/current orientation is identity. Only cyclic corner order
            # preserves winding; reversed coincident faces are separate surfaces.
            for order in ((0, 1, 2), (1, 2, 0), (2, 0, 1)):
                order = list(order)
                pe = float(np.linalg.norm(source_positions[source_index, order] - current, axis=1).max())
                if pe > .00015:
                    continue
                spatial_count += 1
                canonical = source_uv[source_index, order]
                ue = float(np.abs(canonical - existing_uv[face]).max()) if existing_uv is not None else 0.0
                if ue <= .00015:
                    candidates.append((source_index, canonical, pe, ue))
        if len(candidates) != 1:
            raise ValueError("Source triangle position/winding/UV match is missing or ambiguous")
        source_index, canonical, pe, ue = candidates[0]
        if source_index in used:
            raise ValueError("Source triangle matched more than once")
        used.add(source_index)
        restored.extend(canonical)
        position_error, uv_error = max(position_error, pe), max(uv_error, ue)
        positional_choices += int(spatial_count > 1)
    if len(used) != len(source_positions):
        raise ValueError("Unmatched source triangles remain")
    return np.asarray(restored, dtype="<f4"), {"matchedTriangles": len(used), "ambiguousTriangles": 0,
        "maximumNormalizedPositionError": position_error, "existingUvMaximumError": uv_error if existing_uv is not None else None,
        "trianglesDisambiguatedByExistingUvs": positional_choices,
        "matching": "direct corner positions, identity axes, cyclic winding, surviving diffuse UVs"}


def split_uv_seams(indices, vertex_count, corner_uv):
    """Keep original vertex indices first; duplicate only conflicting UV seams."""
    vertex_map = list(range(vertex_count))
    uvs, variants, remapped = [None] * vertex_count, [{} for _ in range(vertex_count)], []
    for original_index, uv in zip(indices, corner_uv):
        options, key = variants[original_index], uv.tobytes()
        if key not in options:
            index = original_index if not options else len(vertex_map)
            options[key] = index
            if index == original_index:
                uvs[index] = uv
            else:
                vertex_map.append(original_index)
                uvs.append(uv)
        remapped.append(options[key])
    if any(uv is None for uv in uvs):
        raise ValueError("Unreferenced original vertex has no reviewed source UV")
    return np.asarray(vertex_map, dtype=int), np.asarray(uvs, dtype="<f4"), np.asarray(remapped, dtype="<u2").reshape(-1, 1)


def recover(archive_data, original, recipe):
    if support.digest(archive_data) != recipe["archiveSha256"] or support.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    with zipfile.ZipFile(io.BytesIO(archive_data)) as archive:
        fbx, dae = (archive.read(recipe["prefix"] + recipe[key]) for key in ("fbx", "dae"))
        atlases = {name: archive.read(recipe["prefix"] + name) for name in recipe["atlases"]}
    if support.digest(fbx) != recipe["fbxSha256"] or support.digest(dae) != recipe["daeSha256"]:
        raise ValueError("Exact source FBX/DAE member differs")
    if any(support.digest(atlas) != recipe["atlases"][name] for name, atlas in atlases.items()):
        raise ValueError("Exact source atlas member differs")
    source = ET.fromstring(dae)
    document, chunks = support.glb.read_glb(original)
    decoded, decoded_chunks = support.glb.read_glb(support.decode_original(original))
    old_data, before = decoded_chunks[0][1], copy.deepcopy(document)
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one original binary chunk")
    if {mesh["name"] for mesh in document["meshes"]} != {"chr675_" + str(index) for index in range(10)}:
        raise ValueError("Original reviewed mesh set differs")
    data, rows, changed_primitives, restored_corners = bytearray(chunks[0][1]), [], set(), {}

    def append_view(blob):
        data.extend(b"\0" * (-len(data) % 4))
        view = len(document["bufferViews"])
        document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": len(blob)})
        data.extend(blob)
        return view

    def append_accessor(values, template):
        item = copy.deepcopy(template)
        item.update(bufferView=append_view(values.tobytes()), byteOffset=0, count=len(values))
        index = len(document["accessors"])
        document["accessors"].append(item)
        return index

    for mesh_index, mesh in enumerate(document["meshes"]):
        if mesh["name"] not in recipe["meshes"]:
            continue
        selected = recipe["meshes"][mesh["name"]]
        if len(mesh["primitives"]) != 1:
            raise ValueError("Expected one primitive per body mesh")
        primitive = mesh["primitives"][0]
        if (primitive.get("mode", 4) != 4 or primitive["material"] != selected["material"]
                or ("TEXCOORD_0" not in primitive["attributes"]) != selected["restoreUv"]):
            raise ValueError("Original reviewed primitive/material/UV state differs")
        positions, source_uv, proof = source_mesh(source, selected)
        points = support.accessor(decoded, old_data, primitive["attributes"]["POSITION"], True)
        indices = support.accessor(decoded, old_data, primitive["indices"]).ravel().astype(int)
        existing_uv = (support.accessor(decoded, old_data, primitive["attributes"]["TEXCOORD_0"], True)
                       if not selected["restoreUv"] else None)
        corners, matched = match_triangles(positions, source_uv, points, indices, existing_uv)
        proof.update(mesh=mesh["name"], **matched, originalVertices=len(points), duplicatedUvSeamVertices=0,
                     restoredUv=selected["restoreUv"], repairedBinding=selected["repairBinding"])
        if selected["restoreUv"]:
            vertex_map, uv, new_indices = split_uv_seams(indices, len(points), corners)
            if len(vertex_map) > 65536:
                raise ValueError("UV recovery exceeds reviewed 16-bit indices")
            if len(vertex_map) != len(points):
                for name, index in list(primitive["attributes"].items()):
                    primitive["attributes"][name] = append_accessor(support.accessor(decoded, old_data, index)[vertex_map], before["accessors"][index])
                primitive["indices"] = append_accessor(new_indices, {"componentType": 5123, "type": "SCALAR"})
            elif not np.array_equal(new_indices.ravel(), indices):
                raise ValueError("Unexpected no-seam index reordering")
            primitive["attributes"]["TEXCOORD_0"] = append_accessor(uv, {"componentType": 5126, "type": "VEC2"})
            proof["duplicatedUvSeamVertices"] = len(vertex_map) - len(points)
            restored_corners[mesh_index] = corners
            changed_primitives.add(mesh_index)
        rows.append(proof)
    textures = {}
    for name, atlas in atlases.items():
        image = len(document["images"])
        document["images"].append({"name": name, "mimeType": "image/png", "bufferView": append_view(atlas)})
        textures[name] = len(document["textures"])
        document["textures"].append({"source": image, "sampler": document["textures"][0]["sampler"]})
    variants = {}
    for mesh_index, mesh in enumerate(document["meshes"]):
        selected = recipe["meshes"].get(mesh["name"])
        if selected is None or not selected["repairBinding"]:
            continue
        primitive = mesh["primitives"][0]
        key = (primitive["material"], selected["atlas"])
        if key not in variants:
            material = copy.deepcopy(before["materials"][primitive["material"]])
            material["pbrMetallicRoughness"].update(baseColorTexture={"index": textures[selected["atlas"]]}, baseColorFactor=[1, 1, 1, 1])
            variants[key] = len(document["materials"])
            document["materials"].append(material)
        primitive["material"] = variants[key]
        changed_primitives.add(mesh_index)
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
    for key in ("bufferViews", "accessors", "materials", "images", "textures"):
        del check[key][len(before[key]):]
    for mesh_index in changed_primitives:
        check["meshes"][mesh_index]["primitives"] = before["meshes"][mesh_index]["primitives"]
    if check != before:
        raise ValueError("Unrelated metadata changed")
    after, after_chunks = support.glb.read_glb(support.decode_original(result))
    for index in range(len(decoded["accessors"])):
        if support.accessor(decoded, old_data, index).tobytes() != support.accessor(after, after_chunks[0][1], index).tobytes():
            raise ValueError("Existing stored accessor changed")
    for mesh_index, (old_mesh, new_mesh) in enumerate(zip(decoded["meshes"], after["meshes"])):
        old_p, new_p = old_mesh["primitives"][0], new_mesh["primitives"][0]
        old_i = support.accessor(decoded, old_data, old_p["indices"]).ravel().astype(int)
        new_i = support.accessor(after, after_chunks[0][1], new_p["indices"]).ravel().astype(int)
        for name, index in old_p["attributes"].items():
            if (support.accessor(decoded, old_data, index)[old_i].tobytes()
                    != support.accessor(after, after_chunks[0][1], new_p["attributes"][name])[new_i].tobytes()):
                raise ValueError("Serialized triangle attributes changed: " + name)
        if mesh_index in restored_corners:
            if support.accessor(after, after_chunks[0][1], new_p["attributes"]["TEXCOORD_0"])[new_i].tobytes() != restored_corners[mesh_index].tobytes():
                raise ValueError("Serialized recovered source UVs differ")
    return result, {"schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
                    "repairedSha256": support.digest(result), "archiveSha256": recipe["archiveSha256"],
                    "sourceUrl": recipe["sourceUrl"], "sourceDae": recipe["prefix"] + recipe["dae"],
                    "daeSha256": support.digest(dae), "originalInputFbxSha256": support.digest(fbx),
                    "albedos": recipe["atlases"], "verifiedExistingAccessors": len(decoded["accessors"]),
                    "preservedOriginalBinaryBytesAndStoredAccessors": True,
                    "preservedEveryTriangleAttributeAndWinding": True, "preservedRigAnimationsAndSwordEffects": True,
                    "sourceProof": rows}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path)
    parser.add_argument("--output-dir", type=Path, default=ROOT / "native/artifacts/material-candidate-beowolfmon")
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
