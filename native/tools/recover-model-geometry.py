"""Restore reviewed source geometry accidentally removed with parked attack effects.

Optional reproduction tool: needs NumPy/SciPy and Node, like the other source
recovery tool. Normal builds consume the checked-in GLB and need none of these.
The separate attack-vine geometry remains a documented action-quality task.
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

spec = importlib.util.spec_from_file_location("material_recovery", Path(__file__).with_name("recover-model-materials.py"))
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)
ROOT, NS = recovery.ROOT, recovery.NS
RECIPES = {
    "palmon": {
        "base": "chr348", "part": "chr348.001", "outputPart": "chr348.001.flower",
        "asset": "models/files/rip_cat_ts_palmon_chr348.glb",
        "originalSha256": "04ff698ebdf33811344a94d6fb728eb406b63884d6b33716ae8531d62f4e2a53",
        "repairedSha256": "868a034c3ec6dca14733fe5d45ec8b648cbc37d5612b5ae79eb8f37319398e9b",
        "archiveSha256": "bd783d1eb19c108f52e0631423c22ca0e681d5d05c0350534da84c5329bf9834",
        "sourceId": "606444", "material": "chr348_b02_flower"
    },
    "toyagumon": {
        "base": "chr708", "part": "chr708.003", "outputPart": "chr708.003",
        "asset": "models/files/rip_cat_ts_toyagumon_chr708.glb",
        "originalSha256": "206552528f385504558b1b10f4f221f4a8a246e4d3a41887971b00db42d11900",
        "repairedSha256": "f50ed4cf8760e37ceb967fcad53076ecc5bf0fd75be0f82be681ac7300cce18a",
        "archiveSha256": "5d273e2d43409adb6ee51f0f9e5de71b8c21b5e0809c810395f4b7fe23a03d64",
        "sourceId": "603392", "material": "chr708_b02"
    }
}
HEAD = {"J_atama", "J_kami", "J_hana"}


def geometry(source, name):
    item = source.find(".//c:geometry[@name='" + name + "']", NS)
    arrays = {s.get("id"): np.fromstring(s.find("c:float_array", NS).text, sep=" ").reshape(
        -1, int(s.find("c:technique_common/c:accessor", NS).get("stride")))
        for s in item.findall("c:mesh/c:source", NS)}
    faces = item.find("c:mesh/c:triangles", NS)
    if faces is None:
        raise ValueError("Expected the reviewed triangulated source")
    inputs = {(i.get("semantic"), i.get("set", "0")): (int(i.get("offset")), i.get("source")[1:])
              for i in faces.findall("c:input", NS)}
    stride = max(a[0] for a in inputs.values()) + 1
    indices = np.fromstring(faces.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3, stride)
    positions = arrays[item.find("c:mesh/c:vertices/c:input", NS).get("source")[1:]]
    return positions, arrays, indices, inputs, faces.get("material")


def global_matrices(document):
    matrices = [None] * len(document["nodes"])
    def visit(index, parent):
        node = document["nodes"][index]
        if "matrix" in node:
            local = np.asarray(node["matrix"]).reshape(4, 4).T
        else:
            x, y, z, w = node.get("rotation", [0, 0, 0, 1])
            local = np.eye(4)
            local[:3, :3] = [[1-2*(y*y+z*z), 2*(x*y-z*w), 2*(x*z+y*w)],
                             [2*(x*y+z*w), 1-2*(x*x+z*z), 2*(y*z-x*w)],
                             [2*(x*z-y*w), 2*(y*z+x*w), 1-2*(x*x+y*y)]]
            local[:3, :3] *= np.asarray(node.get("scale", [1, 1, 1]))
            local[:3, 3] = node.get("translation", [0, 0, 0])
        matrices[index] = parent @ local
        for child in node.get("children", []):
            visit(child, matrices[index])
    for index in document["scenes"][document.get("scene", 0)]["nodes"]:
        visit(index, np.eye(4))
    return matrices


def source_weights(source, name, joint_indices):
    controller = source.find(".//c:controller[@id='Geom_" + name.replace(".", "_") + "-skin']/c:skin", NS)
    sources = {s.get("id"): s for s in controller.findall("c:source", NS)}
    weights_node = controller.find("c:vertex_weights", NS)
    inputs = {i.get("semantic"): (int(i.get("offset")), i.get("source")[1:])
              for i in weights_node.findall("c:input", NS)}
    if inputs["JOINT"][0] != 0 or inputs["WEIGHT"][0] != 1:
        raise ValueError("Unexpected source weight layout")
    names = sources[inputs["JOINT"][1]].find("c:Name_array", NS).text.split()
    values = np.fromstring(sources[inputs["WEIGHT"][1]].find("c:float_array", NS).text, sep=" ")
    counts = np.fromstring(weights_node.find("c:vcount", NS).text, sep=" ", dtype=int)
    links = np.fromstring(weights_node.find("c:v", NS).text, sep=" ", dtype=int).reshape(-1, 2)
    links = np.split(links, np.cumsum(counts)[:-1])
    joints, weights = np.zeros((len(counts), 4), dtype="<u2"), np.zeros((len(counts), 4), dtype="<f4")
    for index, influences in enumerate(links):
        if len(influences) > 4:
            raise ValueError("Source exceeds supported four influences; do not discard weights")
        for slot, (joint, weight) in enumerate(influences):
            joints[index, slot] = joint_indices[names[joint]]
            weights[index, slot] = values[weight]
    if np.max(np.abs(weights.sum(1) - 1)) > .00001:
        raise ValueError("Source weights are not normalized")
    return joints, weights


def recover(archive_bytes, original, model):
    recipe = RECIPES[model]
    base, part = recipe["base"], recipe["part"]
    if recovery.digest(archive_bytes) != recipe["archiveSha256"] or recovery.digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original GLB differs from the reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_bytes))
    dae = archive.read(base + ".dae")
    atlas = archive.read("Textures/" + base + "a01.png")
    source = ET.fromstring(dae)
    document, chunks = recovery.glb.read_glb(recovery.decode_original(original))
    before = copy.deepcopy(document)
    data = bytearray(chunks[0][1])
    body_mesh = next(i for i, m in enumerate(document["meshes"]) if m["name"] == base)
    body_node = next(n for n in document["nodes"] if n.get("mesh") == body_mesh)
    skin = document["skins"][body_node["skin"]]
    joint_indices = {document["nodes"][node]["name"]: i for i, node in enumerate(skin["joints"])}
    body, _, body_faces, body_inputs, _ = geometry(source, base)
    removed_fire = 0
    if model == "toyagumon":
        body_joints, body_weights = source_weights(source, base, joint_indices)
        fire_weight = np.where(body_joints == joint_indices["J_fire"], body_weights, 0).sum(1)
        face_vertices = body_faces[:, :, body_inputs[("VERTEX", "0")][0]]
        fire = np.all(fire_weight[face_vertices] > .5, axis=1)
        removed_fire = int(fire.sum())
        body = body[np.unique(face_vertices[~fire])]
    center, scale = (body.min(0) + body.max(0)) / 2, np.ptp(body, 0).max() / 2
    original_points = recovery.accessor(document, data, document["meshes"][body_mesh]["primitives"][0]["attributes"]["POSITION"], True)
    distance, _ = cKDTree(original_points).query((body - center) / scale)
    if distance.max() > .00015:
        raise ValueError("Source body no longer matches the existing quantized coordinate frame")
    positions, arrays, faces, inputs, material = geometry(source, part)
    if material != base + "_b02-material":
        raise ValueError("Unexpected restored mesh material")
    effect = source.find(".//c:effect[@id='" + base + "_b02-effect']//c:diffuse/c:texture", NS)
    if effect.get("texture") != base + "a01_png-sampler":
        raise ValueError("Source mesh no longer uses the reviewed color atlas")
    joints, weights = source_weights(source, part, joint_indices)
    position_indices = faces[:, :, inputs[("VERTEX", "0")][0]]
    if model == "palmon":
        head = np.where(np.isin(joints, [joint_indices[n] for n in HEAD]), weights, 0).sum(1)
        head_weight = head[position_indices]
        flower = np.all(head_weight > .99999, axis=1)
        vines = np.all(head_weight < .00001, axis=1)
        if int(flower.sum()) != 1822 or int(vines.sum()) != 1780 or not np.all(flower | vines):
            raise ValueError("The reviewed flower/vine partition changed")
        selected = faces[flower].reshape(-1, faces.shape[-1])
    else:
        if len(faces) != 5660 or np.any(weights[joints == joint_indices["J_fire"]] > 0):
            raise ValueError("Reviewed body mesh changed or contains fire effect weights")
        selected = faces.reshape(-1, faces.shape[-1])
    vertex = selected[:, inputs[("VERTEX", "0")][0]]
    recovered_positions = ((positions[vertex] - center) / scale).astype("<f4")
    recovered_normals = arrays[inputs[("NORMAL", "0")][1]][selected[:, inputs[("NORMAL", "0")][0]]].astype("<f4")
    uv = arrays[inputs[("TEXCOORD", "0")][1]][selected[:, inputs[("TEXCOORD", "0")][0]]].astype("<f4")
    uv[:, 1] = 1 - uv[:, 1]
    # Check the existing skin frame independently against every recovered source
    # vertex. Source DAE is Z-up; this GLB is Y-up, with the same original units.
    globals_ = global_matrices(document)
    ibm = recovery.accessor(document, data, skin["inverseBindMatrices"]).reshape(-1, 4, 4).transpose(0, 2, 1)
    matrices = np.asarray([globals_[node] @ ibm[i] for i, node in enumerate(skin["joints"])])
    homogeneous = np.column_stack([recovered_positions, np.ones(len(vertex))])
    transformed = np.einsum("vwij,vj->vwi", matrices[joints[vertex]], homogeneous)
    world = (transformed * weights[vertex, :, None]).sum(1)[:, :3]
    expected = positions[vertex][:, [0, 2, 1]] * [1, 1, -1]
    error = float(np.max(np.abs(world - expected)))
    # ToyAgumon's existing spine rest transform differs from its inverse bind
    # by 0.645 mm. Preserve that existing rig instead of shifting source vertices
    # or silently replacing bind matrices. Palmon has no equivalent offset.
    skin_tolerance = .0007 if model == "toyagumon" else .00001
    if error > skin_tolerance:
        raise ValueError("Recovered geometry does not bind to the original skeleton: " + str(error))

    def view(blob):
        data.extend(b"\0" * (-len(data) % 4))
        index = len(document["bufferViews"])
        document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": len(blob)})
        data.extend(blob)
        return index
    def accessor(values, component, kind):
        item = {"bufferView": view(values.tobytes()), "componentType": component, "type": kind, "count": len(values)}
        if kind == "VEC3":
            item.update(min=values.min(0).tolist(), max=values.max(0).tolist())
        index = len(document["accessors"])
        document["accessors"].append(item)
        return index
    image = len(document["images"])
    document["images"].append({"name": base + "a01.original.png", "mimeType": "image/png", "bufferView": view(atlas)})
    texture = len(document["textures"])
    document["textures"].append({"source": image})
    material_index = len(document["materials"])
    original_material = document["meshes"][body_mesh]["primitives"][0]["material"]
    new_material = copy.deepcopy(document["materials"][original_material])
    new_material["name"] = recipe["material"]
    new_material["pbrMetallicRoughness"]["baseColorTexture"] = {"index": texture}
    document["materials"].append(new_material)
    attributes = {"POSITION": accessor(recovered_positions, 5126, "VEC3"),
                  "NORMAL": accessor(recovered_normals, 5126, "VEC3"),
                  "TEXCOORD_0": accessor(uv, 5126, "VEC2"),
                  "JOINTS_0": accessor(joints[vertex], 5123, "VEC4"),
                  "WEIGHTS_0": accessor(weights[vertex], 5126, "VEC4")}
    mesh = len(document["meshes"])
    document["meshes"].append({"name": recipe["outputPart"], "primitives": [{"attributes": attributes, "material": material_index}]})
    node = copy.deepcopy(body_node)
    node.update(name=recipe["outputPart"], mesh=mesh)
    node_index = len(document["nodes"])
    document["nodes"].append(node)
    original_node_index = before["nodes"].index(body_node)
    parent = next(n for n in document["nodes"] if original_node_index in n.get("children", []))
    parent["children"].append(node_index)
    document["buffers"] = [{"byteLength": len(data)}]
    data.extend(b"\0" * (-len(data) % 4))
    result = recovery.glb.write_glb(document, [(0x004E4942, bytes(data))])
    after, after_chunks = recovery.glb.read_glb(result)
    if after_chunks[0][1][:len(chunks[0][1])] != chunks[0][1]:
        raise ValueError("Existing decoded binary attributes changed")
    for key in ["skins", "animations", "scenes"]:
        if before.get(key) != after.get(key):
            raise ValueError("Original rig or animation changed")
    if before["meshes"] != after["meshes"][:len(before["meshes"])]:
        raise ValueError("Existing mesh metadata changed")
    if recovery.digest(result) != recipe["repairedSha256"]:
        raise ValueError("Recovery output differs from the pinned reviewed bytes")
    report = {"schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
              "repairedSha256": recovery.digest(result), "archiveSha256": recipe["archiveSha256"],
              "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorytimestranger/asset/" + recipe["sourceId"] + "/",
              "sourceDaeSha256": recovery.digest(dae), "sourceAlbedoSha256": recovery.digest(atlas),
              "restoredTriangles": len(selected) // 3, "sourcePositionFrameError": float(distance.max()),
              "restoredSkinPositionError": error, "existingRigAndAnimationsPreserved": True,
              "restoredSkinPositionTolerance": skin_tolerance,
              "existingDecodedAttributesPreserved": True,
              "remainingRequirements": ["Inspect restored geometry in fitted front/back and all action roles.",
                                        "Eye material composition remains unverified."]}
    if model == "palmon":
        report.update(restoredFlowerTriangles=1822, excludedParkedVineTriangles=1780, mixedTriangles=0)
        report["remainingRequirements"].append("Parked vine geometry requires separate attack visibility/choreography review.")
    else:
        report.update(restoredBodyTriangles=5660, restoredFireWeight=0, excludedExistingFireTriangles=removed_fire)
    return result, report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECIPES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path)
    parser.add_argument("--apply", action="store_true")
    options = parser.parse_args()
    recipe = RECIPES[options.model]
    original = (options.original or ROOT / recipe["asset"]).read_bytes()
    result, report = recover(options.archive.read_bytes(), original, options.model)
    output = ROOT / "native/artifacts" / (options.model + "-source")
    output.mkdir(parents=True, exist_ok=True)
    (output / "geometry-recovery.json").write_text(json.dumps(report, indent=2) + "\n")
    (output / "recovered-geometry.glb").write_bytes(result)
    if options.apply:
        path = ROOT / recipe["asset"]
        if recovery.digest(path.read_bytes()) not in [recipe["originalSha256"], report["repairedSha256"]]:
            raise ValueError("Current production GLB has unexpected changes")
        backup = ROOT / "native/artifacts/model-material-originals" / path.name
        backup.parent.mkdir(parents=True, exist_ok=True)
        if backup.exists() and backup.read_bytes() != original:
            raise ValueError("Existing original backup differs")
        backup.write_bytes(original)
        path.write_bytes(result)
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
