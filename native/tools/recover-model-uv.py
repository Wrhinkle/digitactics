"""Reproduce a pinned source-verified UV repair as a candidate, never apply it.

Optional dependencies: numpy/scipy, Node, and the repository meshopt decoder.
Only the three reviewed UV views are replaced. Every original binary byte is
retained; other compressed views, materials, images, rig, and animation remain
unchanged. This tool does not participate in ordinary builds.
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
spec = importlib.util.spec_from_file_location(
    "material_recovery", Path(__file__).with_name("recover-model-materials.py"))
support = importlib.util.module_from_spec(spec)
spec.loader.exec_module(support)
NS = support.NS
RECOVERIES = {
    "leopardmonx": {
        "asset": "models/files/rip_l_leopardmon_x.glb",
        "originalSha256": "2666b78200939c69c50aede1092ba615ef80e0e7d852ab3a94533ed3919acdff",
        "archiveSha256": "64424def37d5c9dd4895425c02c5ef3cbc707c525390400e1b3fbddff068294e",
        "repairedSha256": "5f0ebcbe529b5bb10f7364dabd0d4d9698d69712b478b42228ced1ea8d92c5c4",
        "sourceUrl": "https://models.spriters-resource.com/mobile/digimonlinks/asset/310676/",
        "dae": "Leopardmon X/chr717.dae",
        "daeSha256": "cef97c7dbc9be61f58d90ba226eccb37e780b3a684ad2285e1217391a67d071a",
        "meshes": {
            "chr717_0": {"geometry": "_001_chr717", "atlas": "chr717_m2.png",
                         "triangles": 4852, "axes": [0, 2, 1], "signs": [1, 1, -1]},
            "chr717_1": {"geometry": "_002_chr717", "atlas": "chr717_m1.png",
                         "triangles": 7052, "axes": [0, 2, 1], "signs": [1, 1, -1]},
            "sw_ef_lig_0": {"geometry": "_000_sw_ef_lig", "atlas": "chr717_m3.png",
                            "triangles": 78, "axes": [0, 2, 1], "signs": [-1, -1, -1]}
        }
    }
}


def source_mesh(source, reviewed):
    geometry = source.find(".//c:library_geometries/c:geometry[@name='" + reviewed["geometry"] + "']", NS)
    mesh = geometry.find("c:mesh", NS)
    poly = mesh.find("c:polylist", NS)
    if poly is None or any(int(x) != 3 for x in poly.find("c:vcount", NS).text.split()):
        raise ValueError("Expected reviewed source triangle polylist")
    arrays = {value.get("id"): np.fromstring(value.find("c:float_array", NS).text, sep=" ").reshape(
        -1, int(value.find("c:technique_common/c:accessor", NS).get("stride")))
        for value in mesh.findall("c:source", NS)}
    material_id = poly.get("material")
    material = source.find(".//c:library_materials/c:material[@id='" + material_id + "']", NS)
    effect_id = material.find("c:instance_effect", NS).get("url")[1:]
    effect = source.find(".//c:library_effects/c:effect[@id='" + effect_id + "']", NS)
    diffuse = effect.find(".//c:diffuse/c:texture", NS)
    if diffuse is None:
        raise ValueError("Source diffuse texture is missing")
    sampler = effect.find(".//c:newparam[@sid='" + diffuse.get("texture") + "']/c:sampler2D/c:source", NS).text
    surface = effect.find(".//c:newparam[@sid='" + sampler + "']/c:surface/c:init_from", NS).text
    image_path = source.find(".//c:library_images/c:image[@id='" + surface + "']/c:init_from", NS).text
    if image_path != reviewed["atlas"]:
        raise ValueError("Source diffuse differs from reviewed atlas binding")
    uv_input = support.diffuse_uv_input(source, poly, material_id, diffuse)
    inputs = poly.findall("c:input", NS)
    vertex_input = next(value for value in inputs if value.get("semantic") == "VERTEX")
    vertices = mesh.find("c:vertices[@id='" + vertex_input.get("source")[1:] + "']", NS)
    position_input = next(value for value in vertices.findall("c:input", NS)
                          if value.get("semantic") == "POSITION")
    positions = arrays[position_input.get("source")[1:]]
    stride = 1 + max(int(value.get("offset")) for value in inputs)
    faces = np.fromstring(poly.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3, stride)
    indices = faces[:, :, int(vertex_input.get("offset"))]
    uv = arrays[uv_input.get("source")[1:]][faces[:, :, int(uv_input.get("offset"))]]
    return positions, indices, uv, {
        "sourceGeometry": geometry.get("name"), "sourceMaterial": material_id,
        "sourceDiffuse": image_path, "sourceDiffuseSemantic": diffuse.get("texcoord"),
        "sourceUvSet": int(uv_input.get("set", "0")),
        "sourceUvInput": uv_input.get("source")[1:],
        "sourceUvBinding": "explicit diffuse bind_vertex_input"}


def verify_triangles(source, reviewed, document, data, mesh):
    positions, source_indices, source_uv, row = source_mesh(source, reviewed)
    if len(mesh["primitives"]) != 1:
        raise ValueError("Expected one reviewed primitive")
    primitive = mesh["primitives"][0]
    if primitive.get("mode", 4) != 4:
        raise ValueError("Expected triangles")
    indices = support.accessor(document, data, primitive["indices"]).ravel().astype(int)
    if len(indices) != reviewed["triangles"] * 3 or len(source_indices) != reviewed["triangles"]:
        raise ValueError("Source triangle counts differ")
    points = support.accessor(document, data, primitive["attributes"]["POSITION"], True)
    existing = support.accessor(document, data, primitive["attributes"]["TEXCOORD_0"])
    if not np.isfinite(existing).all():
        raise ValueError("Nonfinite UV coordinates")
    repaired = existing.copy()
    repaired[:, 1] = -repaired[:, 1]
    center = (positions.min(0) + positions.max(0)) / 2
    scale = np.ptp(positions, 0).max() / 2
    transformed = ((positions - center) / scale)[:, reviewed["axes"]] * reviewed["signs"]
    distances, nearest = cKDTree(points).query(transformed)
    if distances.max() > 0.00015:
        raise ValueError("Source positions differ")
    keys = [tuple(point) for point in np.round(points * 32767).astype(int)]
    triangles = {}
    for face, uv in zip(source_indices, source_uv):
        corners = [keys[nearest[index]] for index in face]
        if len(set(corners)) != 3:
            raise ValueError("Degenerate source triangle")
        triangles.setdefault(tuple(sorted(corners)), []).append((corners, uv))
    duplicates = sum(len(value) - 1 for value in triangles.values())
    before_error = after_error = relation_error = 0.0
    for face in indices.reshape(-1, 3):
        corners = [keys[index] for index in face]
        candidates = triangles.get(tuple(sorted(corners)), [])
        options = []
        for candidate_index, (source_corners, uv) in enumerate(candidates):
            ordered = np.asarray([uv[source_corners.index(corner)] for corner in corners])
            canonical = ordered.copy()
            canonical[:, 1] = 1 - canonical[:, 1]
            error = float(np.max(np.abs(repaired[face] - canonical)))
            options.append((error, candidate_index, ordered, canonical))
        if not options:
            raise ValueError("Source triangle does not map one-to-one")
        error, candidate_index, ordered, canonical = min(options, key=lambda value: value[0])
        if error > 0.000002:
            raise ValueError("Negated V does not match canonical source UVs")
        candidates.pop(candidate_index)
        reviewed_before = ordered.copy()
        reviewed_before[:, 1] -= 1
        relation_error = max(relation_error, float(np.max(np.abs(existing[face] - reviewed_before))))
        before_error = max(before_error, float(np.max(np.abs(existing[face] - canonical))))
        after_error = max(after_error, error)
    if any(triangles.values()) or relation_error > 0.000002:
        raise ValueError("Source correspondence or reviewed UV relation differs")
    row.update(mesh=mesh["name"], matchedTriangles=len(source_indices),
               maximumNormalizedPositionError=float(distances.max()),
               sourceToCurrentAxes=reviewed["axes"], sourceToCurrentSigns=reviewed["signs"],
               coincidentTrianglesMatchedByUvs=duplicates,
               originalRelation="U=sourceU, V=sourceV-1",
               originalRelationMaximumError=relation_error,
               beforeCanonicalUvMaximumError=before_error,
               afterCanonicalUvMaximumError=after_error)
    return primitive["attributes"]["TEXCOORD_0"], repaired, row


def recover(archive_data, original, recipe):
    if support.digest(archive_data) != recipe["archiveSha256"] or support.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    dae = archive.read(recipe["dae"])
    if support.digest(dae) != recipe["daeSha256"]:
        raise ValueError("Source DAE differs from reviewed bytes")
    source = ET.fromstring(dae)
    original_document, original_chunks = support.glb.read_glb(original)
    if len(original_chunks) != 1 or original_chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one original binary chunk")
    decoded, decoded_chunks = support.glb.read_glb(support.decode_original(original))
    decoded_data = decoded_chunks[0][1]
    document = copy.deepcopy(original_document)
    data = bytearray(original_chunks[0][1])
    if len(data) != document["buffers"][0]["byteLength"]:
        raise ValueError("Unexpected original binary padding")
    if set(recipe["meshes"]) != {mesh["name"] for mesh in document["meshes"]}:
        raise ValueError("Reviewed mesh set differs")
    repaired_uvs, rows, changed_views = {}, [], []
    for mesh in decoded["meshes"]:
        reviewed = recipe["meshes"][mesh["name"]]
        index, values, row = verify_triangles(source, reviewed, decoded, decoded_data, mesh)
        attribute = document["accessors"][index]
        view_index = attribute["bufferView"]
        view = document["bufferViews"][view_index]
        if (attribute["componentType"] != 5126 or attribute["type"] != "VEC2"
                or attribute.get("normalized") or attribute.get("byteOffset", 0)
                or "min" in attribute or "max" in attribute
                or view.get("byteStride", 8) != 8 or view["byteLength"] != values.nbytes
                or sum(a.get("bufferView") == view_index for a in document["accessors"]) != 1
                or index in repaired_uvs or view_index in changed_views):
            raise ValueError("UV accessor differs from reviewed isolated float layout")
        # Append three uncompressed UV replacements; retain all compressed data.
        data.extend(b"\0" * (-len(data) % 4))
        view.update(buffer=0, byteOffset=len(data))
        del view["extensions"]["EXT_meshopt_compression"]
        if not view["extensions"]:
            del view["extensions"]
        data.extend(values.tobytes())
        repaired_uvs[index] = values
        changed_views.append(view_index)
        rows.append(row)
    document["buffers"][0]["byteLength"] = len(data)
    result = support.glb.write_glb(document, [(0x004E4942, bytes(data))])
    if support.digest(result) != recipe["repairedSha256"]:
        raise ValueError("Repair output differs from the reviewed bytes")
    after_document, after_chunks = support.glb.read_glb(result)
    if after_document != document or after_chunks[0][1][:len(original_chunks[0][1])] != original_chunks[0][1]:
        raise ValueError("Serialized metadata or original binary content changed")
    # Metadata may change only the selected UV views and embedded buffer length.
    restored_metadata = copy.deepcopy(after_document)
    restored_metadata["buffers"][0]["byteLength"] = original_document["buffers"][0]["byteLength"]
    for index in changed_views:
        restored_metadata["bufferViews"][index] = original_document["bufferViews"][index]
    if restored_metadata != original_document:
        raise ValueError("Non-UV metadata changed")
    after, chunks = support.glb.read_glb(support.decode_original(result))
    for index in range(len(decoded["accessors"])):
        old_values = support.accessor(decoded, decoded_data, index)
        new_values = support.accessor(after, chunks[0][1], index)
        expected = repaired_uvs.get(index, old_values)
        if new_values.tobytes() != expected.tobytes():
            raise ValueError("Serialized accessor changed unexpectedly: " + str(index))
        if index in repaired_uvs and (new_values[:, 0].tobytes() != old_values[:, 0].tobytes()
                                     or new_values[:, 1].tobytes() != (-old_values[:, 1]).tobytes()):
            raise ValueError("UV change was not exactly U unchanged, V negated")
    report = {"schemaVersion": 1, "asset": recipe["asset"],
              "originalSha256": recipe["originalSha256"], "repairedSha256": support.digest(result),
              "archiveSha256": recipe["archiveSha256"], "sourceUrl": recipe["sourceUrl"],
              "daeSha256": support.digest(dae), "operation": "U unchanged; V = -old V",
              "preservedOriginalBinaryBytes": True, "preservedOtherAccessorsBitForBit": True,
              "preservedNonUvMetadata": True, "preservedRigAnimationMaterialsImages": True,
              "changedUvAccessors": list(repaired_uvs), "changedUvBufferViews": changed_views,
              "verifiedAccessors": len(decoded["accessors"]), "meshes": rows}
    return result, report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path, help="Pinned original if the tracked asset is already repaired")
    parser.add_argument("--output-dir", type=Path,
                        default=ROOT / "native/artifacts/material-candidates-next")
    options = parser.parse_args()
    output = options.output_dir.resolve()
    if not output.is_relative_to((ROOT / "native/artifacts").resolve()):
        raise ValueError("Candidate output must stay under native/artifacts")
    recipe = RECOVERIES[options.model]
    original = (options.original or ROOT / recipe["asset"]).read_bytes()
    result, report = recover(options.archive.read_bytes(), original, recipe)
    output.mkdir(parents=True, exist_ok=True)
    (output / (options.model + ".candidate.glb")).write_bytes(result)
    (output / (options.model + ".candidate.json")).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
