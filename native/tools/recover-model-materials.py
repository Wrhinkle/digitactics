"""Recover reviewed original model atlases/UVs from the pinned public source archive.

This optional repair tool needs numpy/scipy and Node; ordinary builds do not.
It preserves triangles, every existing vertex attribute, rig and animation.
UV seams lost during the original FBX conversion are restored by duplicating
the existing vertices, never by moving them or inventing texture coordinates.
"""
import argparse
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import zipfile
import xml.etree.ElementTree as ET

import numpy as np
from scipy.spatial import cKDTree

ROOT = Path(__file__).resolve().parents[2]
RECOVERIES = {
    "pilevolcamon": {
        "asset": "models/files/rip_cs_pilevolcamon.glb",
        "originalSha256": "5a2663ea3ba36c8888f7b16576a46bbd44f7bb36a6164a280f85c8b5903e67d9",
        "repairedSha256": "77eb06d0ca779fd6d67ba5b22a40387e094d93a9da906c3868b81e46fde52c8b",
        "archiveSha256": "dfe5d70676668f9e1f96ddfdf5ab406daf7edcf61398edc7602fef5a550ed3e0",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324353/",
        "prefix": "PileVolcamon/chr060", "base": "chr060"
    },
    "triceramon": {
        "asset": "models/files/rip_cs_triceramon.glb",
        "originalSha256": "f5f6675ec24c58a1e9d1c6331ea8fcf056fa675a1f26f6a8e3e14f68845f933d",
        "repairedSha256": "8b16c5e3941670124416e16a985897be317f22050ff81275c3d6b630585ea15e",
        "archiveSha256": "92b53af362f5d024fc2d2903092c7e354a4f7a949c858294eb16fcdc554f30b8",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324322/",
        "prefix": "Triceramon/chr033", "base": "chr033"
    },
    "goldramon": {
        "asset": "models/files/rip_cs_goldramon.glb",
        "originalSha256": "6495c846af57f93e1b744291a595eb1da388edbe575750c4d949ceb3d43b62c7",
        "repairedSha256": "b44b4d518080d3583d645c3bb4b72ca65ad0e9c1575ef4549d27463077c45557",
        "archiveSha256": "6a3fcd45470b94457e1f45a539c0201346291495d7e7e623e5f2e80d57c8acf4",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324339/",
        "prefix": "Goldramon/chr048", "base": "chr048",
        "meshAtlases": {"chr048_0": "chr048a01.png", "chr048_1": "chr048a01.png",
                        "chr048_2": "chr048a01.png", "chr048_3": "chr048a01.png",
                        "chr048_4": "chr048a01.png"}
    },
    "grademon": {
        "asset": "models/files/rip_cs_grademon.glb",
        "originalSha256": "e26a01e0192116da3a7f1459e751ed5eca696a91feeab4ba2ed3fa681e8e2906",
        "repairedSha256": "f976193691868f0e4e68111ed926c71052d4e17b9f7ea58b5d1669509abc3d83",
        "archiveSha256": "f1b6043a9b007705a3e261b33207d7b8c92c05a60f4822861423c44f1551b8fa",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324410/",
        "prefix": "Grademon/chr026", "base": "chr026",
        "meshAtlases": {"chr026_0": "chr026a01.png", "chr026_1": "chr026a02.png",
                        "chr026_2": "chr026a01.png", "chr026_3": "chr026a02.png",
                        "chr026_4": "chr026a01.png"}
    },
    "megidramon": {
        "asset": "models/files/rip_cs_megidramon.glb",
        "originalSha256": "7e1d0c7744cb40972d8416afe11ce150b32b3f4d97e8f3e9d5f5955314211770",
        "repairedSha256": "848fd8f3ccb50a7f302fb87ef254b7dbfbd137300b555d40ef67f0de5eecbf23",
        "archiveSha256": "dfff441ade1ef3a16310f2f44d05a5d098cba039bbbfa4d7c4f26c779d63dc68",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324340/",
        "prefix": "Megidramon/chr047", "base": "chr047",
        "meshAtlases": {"chr047_0": "chr047a02.png", "chr047_1": "chr047a02.png",
                        "chr047_2": "chr047a02.png", "chr047_3": "chr047a01.png",
                        "chr047_4": "chr047a01.png", "chr047_5": "chr047b01.png"}
    },
    "neptunemon": {
        "asset": "models/files/rip_cs_neptunemon.glb",
        "originalSha256": "9899ea80811bd6994dff2473fdbf10a01eeaae2d1047bcd9e736d8d113827a99",
        "repairedSha256": "2a11348ec65ded3dc7459204a3cc6b4256e75c7ce3ccf124b18dcc26e847f1bf",
        "archiveSha256": "4669ace9bb52a309a4c5185a0ba867cdda8a711948332b255de074d7e7f73f6b",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324351/",
        "prefix": "Neptunemon/chr075", "base": "chr075",
        "meshAtlases": {"chr075_0": "chr075a01.png", "chr075_1": "chr075a01.png",
                        "chr075_2": "chr075a01.png", "chr075_3": "chr075a01.png"}
    }
}
spec = importlib.util.spec_from_file_location("material_repair", Path(__file__).with_name("repair-model-materials.py"))
glb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(glb)
NS = {"c": "http://www.collada.org/2005/11/COLLADASchema"}
TYPES = {5120: "i1", 5121: "u1", 5122: "<i2", 5123: "<u2", 5125: "<u4", 5126: "<f4"}
DIMS = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4, "MAT4": 16}


def digest(data):
    return hashlib.sha256(data).hexdigest()


def decode_original(original):
    # Same vendor decoder as export-models.cjs, run independently of the catalog.
    code = r"""
import fs from 'node:fs';
import {MeshoptDecoder} from './vendor/three/addons/libs/meshopt_decoder.module.js';
await MeshoptDecoder.ready;
const b=fs.readFileSync(0), jn=b.readUInt32LE(12);
const j=JSON.parse(b.subarray(20,20+jn)), bin=b.subarray(28+jn);
const parts=[];let size=0;
for(const v of j.bufferViews){
 const e=v.extensions?.EXT_meshopt_compression;let part;
 if(e){part=Buffer.alloc(e.count*e.byteStride);MeshoptDecoder.decodeGltfBuffer(part,e.count,e.byteStride,bin.subarray(e.byteOffset||0,(e.byteOffset||0)+e.byteLength),e.mode,e.filter);delete v.extensions.EXT_meshopt_compression;if(!Object.keys(v.extensions).length)delete v.extensions;}
 else{if(v.buffer!==0)throw Error('Unexpected external buffer');part=bin.subarray(v.byteOffset||0,(v.byteOffset||0)+v.byteLength);}
 if(part.length!==v.byteLength)throw Error('Truncated view');
 const pad=(-size)&3;if(pad){parts.push(Buffer.alloc(pad));size+=pad;}
 v.buffer=0;v.byteOffset=size;parts.push(part);size+=part.length;
}
j.buffers=[{byteLength:size}];
for(const k of ['extensionsUsed','extensionsRequired'])if(j[k])j[k]=j[k].filter(x=>x!=='EXT_meshopt_compression');
const jb=Buffer.from(JSON.stringify(j)), jp=Buffer.alloc((jb.length+3)&~3,32);jb.copy(jp);
const bb=Buffer.concat(parts),bp=Buffer.alloc((bb.length+3)&~3);bb.copy(bp);
const out=Buffer.alloc(28+jp.length+bp.length);out.writeUInt32LE(0x46546c67);out.writeUInt32LE(2,4);out.writeUInt32LE(out.length,8);out.writeUInt32LE(jp.length,12);out.writeUInt32LE(0x4e4f534a,16);jp.copy(out,20);out.writeUInt32LE(bp.length,20+jp.length);out.writeUInt32LE(0x004e4942,24+jp.length);bp.copy(out,28+jp.length);process.stdout.write(out);
"""
    return subprocess.run(["node", "--input-type=module", "-e", code], cwd=ROOT,
                          input=original, check=True, capture_output=True).stdout


def accessor(document, data, index, normalized=False):
    a = document["accessors"][index]
    view = document["bufferViews"][a["bufferView"]]
    dtype = np.dtype(TYPES[a["componentType"]])
    width = DIMS[a["type"]]
    result = np.ndarray((a["count"], width), dtype=dtype, buffer=data,
                        offset=view.get("byteOffset", 0) + a.get("byteOffset", 0),
                        strides=(view.get("byteStride", width * dtype.itemsize), dtype.itemsize)).copy()
    if normalized and a.get("normalized"):
        result = result.astype(float) / np.iinfo(dtype).max
    return result


def diffuse_uv_input(source, poly, material_id, diffuse):
    """Resolve the Collada material semantic to its explicitly bound UV set."""
    semantic = diffuse.get("texcoord")
    if not semantic:
        raise ValueError("Source diffuse texture does not identify UV coordinates")
    bindings = [binding for material in source.findall(".//c:instance_material", NS)
                if material.get("target") == "#" + material_id
                for binding in material.findall("c:bind_vertex_input", NS)
                if binding.get("semantic") == semantic and binding.get("input_semantic") == "TEXCOORD"]
    sets = {binding.get("input_set") for binding in bindings}
    if len(sets) != 1 or None in sets:
        raise ValueError("Source diffuse UV binding is missing or ambiguous")
    selected = [value for value in poly.findall("c:input", NS)
                if value.get("semantic") == "TEXCOORD" and value.get("set", "0") in sets]
    if len(selected) != 1:
        raise ValueError("Source diffuse UV set does not resolve to one input")
    return selected[0]


def recover(archive_data, original, recipe):
    if digest(archive_data) != recipe["archiveSha256"] or digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    dae_data = archive.read(recipe["prefix"] + ".dae")
    source = ET.fromstring(dae_data)
    document, chunks = glb.read_glb(decode_original(original))
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one embedded binary chunk")
    before = copy.deepcopy(document)
    old_data = chunks[0][1]
    data = bytearray(old_data)
    rows = []
    mesh_atlases = recipe.get("meshAtlases", {mesh["name"]: recipe["base"] + "a01.png"
                                               for mesh in document["meshes"]})
    if set(mesh_atlases) != {mesh["name"] for mesh in document["meshes"]}:
        raise ValueError("Reviewed atlas assignments differ from surviving meshes")
    atlas_files = dict.fromkeys(mesh_atlases.values())
    atlases = {name: archive.read(recipe["prefix"].rsplit("/", 1)[0] + "/" + name)
               for name in atlas_files}

    def append_view(blob):
        data.extend(b"\0" * (-len(data) % 4))
        index = len(document["bufferViews"])
        document["bufferViews"].append({"buffer": 0, "byteOffset": len(data), "byteLength": len(blob)})
        data.extend(blob)
        return index

    def append_accessor(values, template):
        item = copy.deepcopy(template)
        item.update(bufferView=append_view(values.tobytes()), byteOffset=0, count=len(values))
        index = len(document["accessors"])
        document["accessors"].append(item)
        return index

    geometries = source.findall(".//c:library_geometries/c:geometry", NS)
    for mesh in document["meshes"]:
        geometry = next(g for g in geometries if g.get("name") == mesh["name"])
        arrays = {s.get("id"): np.fromstring(s.find("c:float_array", NS).text, sep=" ").reshape(
            -1, int(s.find("c:technique_common/c:accessor", NS).get("stride")))
            for s in geometry.findall("c:mesh/c:source", NS)}
        poly = geometry.find("c:mesh/c:polylist", NS)
        material_id = poly.get("material")
        material = source.find(".//c:library_materials/c:material[@id='" + material_id + "']", NS)
        effect_id = material.find("c:instance_effect", NS).get("url")[1:]
        effect = source.find(".//c:library_effects/c:effect[@id='" + effect_id + "']", NS)
        diffuse = effect.find(".//c:diffuse/c:texture", NS)
        if diffuse is None:
            raise ValueError("Source mesh has no diffuse texture")
        sampler = effect.find(".//c:newparam[@sid='" + diffuse.get("texture") + "']/c:sampler2D/c:source", NS)
        surface = (effect.find(".//c:newparam[@sid='" + sampler.text + "']/c:surface/c:init_from", NS)
                   if sampler is not None else None)
        image_path = (source.find(".//c:library_images/c:image[@id='" + surface.text + "']/c:init_from", NS)
                      if surface is not None else None)
        if image_path is None or image_path.text != mesh_atlases[mesh["name"]]:
            raise ValueError("Source mesh does not use the reviewed color atlas")
        poly_inputs = poly.findall("c:input", NS)
        uv_input = diffuse_uv_input(source, poly, material_id, diffuse)
        inputs = {i.get("semantic"): (int(i.get("offset")), i.get("source")[1:])
                  for i in poly_inputs if i.get("semantic") != "TEXCOORD"}
        inputs["TEXCOORD"] = (int(uv_input.get("offset")), uv_input.get("source")[1:])
        stride = 1 + max(int(value.get("offset")) for value in poly_inputs)
        if any(int(x) != 3 for x in poly.find("c:vcount", NS).text.split()):
            raise ValueError("Source contains non-triangles")
        faces = np.fromstring(poly.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3, stride)
        positions = arrays[geometry.find("c:mesh/c:vertices/c:input", NS).get("source")[1:]]
        center = (positions.min(0) + positions.max(0)) / 2
        scale = np.ptp(positions, 0).max() / 2
        primitive = mesh["primitives"][0]
        if len(mesh["primitives"]) != 1 or primitive.get("mode", 4) != 4:
            raise ValueError("Expected one triangle primitive per reviewed mesh")
        points = accessor(document, old_data, primitive["attributes"]["POSITION"], True)
        indices = accessor(document, old_data, primitive["indices"]).reshape(-1).astype(int)
        distances, nearest = cKDTree(points).query((positions - center) / scale)
        if distances.max() > 0.00015 or len(faces) * 3 != len(indices):
            raise ValueError("Source topology or quantized positions differ")
        keys = [tuple(x) for x in np.round(points * 32767).astype(int)]
        existing = (accessor(document, old_data, primitive["attributes"]["TEXCOORD_0"], True)[indices]
                    if "TEXCOORD_0" in primitive["attributes"] else None)
        triangles = {}
        for face in faces:
            corners = [keys[nearest[x[inputs["VERTEX"][0]]]] for x in face]
            key = tuple(sorted(corners))
            if len(set(corners)) != 3 or (key in triangles and existing is None):
                raise ValueError("Ambiguous source triangle")
            triangles.setdefault(key, []).append(
                (corners, arrays[inputs["TEXCOORD"][1]][face[:, inputs["TEXCOORD"][0]]]))
        repeated_positions = sum(len(candidates) - 1 for candidates in triangles.values())
        restored = []
        for face_index, face in enumerate(indices.reshape(-1, 3)):
            corners = [keys[index] for index in face]
            key = tuple(sorted(corners))
            candidates = triangles.get(key, [])
            if not candidates:
                raise ValueError("Source triangle does not map one-to-one")
            chosen = 0
            if existing is not None and len(candidates) > 1:
                # Coincident triangles with different UVs are distinguishable without
                # changing their surviving coordinates or choosing by source ordering.
                errors = []
                for source_corners, source_uv in candidates:
                    candidate_uv = np.asarray([source_uv[source_corners.index(corner)] for corner in corners])
                    candidate_uv[:, 1] = 1 - candidate_uv[:, 1]
                    errors.append(float(np.max(np.abs(existing[face_index * 3:face_index * 3 + 3] - candidate_uv))))
                chosen = int(np.argmin(errors))
                if errors[chosen] > 0.00015:
                    raise ValueError("Coincident source triangles do not match surviving UVs")
            source_corners, source_uv = candidates.pop(chosen)
            restored.extend(source_uv[source_corners.index(corner)] for corner in corners)
        if any(triangles.values()):
            raise ValueError("Unmatched source triangles remain")
        restored = np.asarray(restored, dtype="<f4")
        restored[:, 1] = 1 - restored[:, 1]  # Collada vs glTF texture origin.
        row = {"mesh": mesh["name"], "matchedTriangles": len(faces),
               "maximumNormalizedPositionError": float(distances.max()), "ambiguousTriangles": 0}
        if "meshAtlases" in recipe:
            row.update(albedoImage=mesh_atlases[mesh["name"]],
                       sourceUvSet=int(uv_input.get("set", "0")),
                       sourceUvInput=uv_input.get("source")[1:],
                       coincidentTrianglesMatchedByExistingUvs=repeated_positions)
        if existing is not None:
            error = float(np.max(np.abs(existing - restored)))
            if error > 0.00015:
                raise ValueError("Recovered UV orientation does not match surviving source UVs")
            row["existingUvMaximumError"] = error
        else:
            if len(indices) > 65536:
                raise ValueError("UV seam restoration exceeds reviewed 16-bit indices")
            # Lost UV seams require separate vertices. Copy original attributes bit-for-bit.
            for name, index in list(primitive["attributes"].items()):
                values = accessor(document, old_data, index)[indices]
                new_index = append_accessor(values, before["accessors"][index])
                primitive["attributes"][name] = new_index
                if not np.array_equal(accessor(document, data, new_index), values):
                    raise ValueError("Vertex attribute changed during seam restoration")
            primitive["attributes"]["TEXCOORD_0"] = append_accessor(restored, {"componentType": 5126, "type": "VEC2"})
            primitive["indices"] = append_accessor(np.arange(len(indices), dtype="<u2").reshape(-1, 1),
                                                     {"componentType": 5123, "type": "SCALAR"})
            row["restoredUvVertices"] = len(indices)
        rows.append(row)
    textures = {}
    for name, atlas in atlases.items():
        image = len(document["images"])
        document["images"].append({"name": name, "mimeType": "image/png", "bufferView": append_view(atlas)})
        textures[name] = len(document["textures"])
        document["textures"].append({"source": image, "sampler": document["textures"][0]["sampler"]})
    # A material shared by differently textured source meshes must be split.
    assignments = {}
    variants = {}
    for mesh in document["meshes"]:
        primitive = mesh["primitives"][0]
        original_material = primitive["material"]
        texture = textures[mesh_atlases[mesh["name"]]]
        variant = (original_material, texture)
        if variant not in variants:
            index = original_material
            if index in assignments and assignments[index] != texture:
                index = len(document["materials"])
                document["materials"].append(copy.deepcopy(before["materials"][original_material]))
            assignments[index] = texture
            variants[variant] = index
            material = document["materials"][index]
            material["pbrMetallicRoughness"]["baseColorTexture"] = {"index": texture}
            material["pbrMetallicRoughness"]["baseColorFactor"] = [1, 1, 1, 1]
        primitive["material"] = variants[variant]
    document["buffers"] = [{"byteLength": len(data)}]
    for key in ("nodes", "skins", "animations", "scenes"):
        if document.get(key) != before.get(key):
            raise ValueError("Rig or animation metadata changed")
    result = glb.write_glb(document, [(0x004E4942, bytes(data) + b"\0" * (-len(data) % 4))])
    if digest(result) != recipe["repairedSha256"]:
        raise ValueError("Repair output differs from the reviewed bytes")
    after, after_chunks = glb.read_glb(result)
    after_data = after_chunks[0][1]
    for old_mesh, new_mesh in zip(before["meshes"], after["meshes"]):
        old_p, new_p = old_mesh["primitives"][0], new_mesh["primitives"][0]
        old_i = accessor(before, old_data, old_p["indices"]).reshape(-1).astype(int)
        new_i = accessor(after, after_data, new_p["indices"]).reshape(-1).astype(int)
        for name, old_a in old_p["attributes"].items():
            if not np.array_equal(accessor(before, old_data, old_a)[old_i],
                                  accessor(after, after_data, new_p["attributes"][name])[new_i]):
                raise ValueError("Serialized triangle attributes differ: " + name)
    report = {"schemaVersion": 1, "asset": recipe["asset"],
                    "originalSha256": recipe["originalSha256"], "repairedSha256": digest(result),
                    "sourceUrl": recipe["sourceUrl"], "archiveSha256": recipe["archiveSha256"],
                    "daeSha256": digest(dae_data),
                    "preservedOriginalTriangleAttributes": True, "preservedRigAndAnimation": True,
                    "meshes": rows}
    if len(atlases) == 1:
        report["albedoSha256"] = digest(next(iter(atlases.values())))
    else:
        report["albedos"] = [{"name": name, "sha256": digest(atlas)} for name, atlas in atlases.items()]
    return result, report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model", choices=RECOVERIES, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--original", type=Path,
                        help="Pinned original GLB, if the tracked asset is already repaired")
    parser.add_argument("--apply", action="store_true")
    options = parser.parse_args()
    recipe = RECOVERIES[options.model]
    asset = ROOT / recipe["asset"]
    original = (options.original or asset).read_bytes()
    result, report = recover(options.archive.read_bytes(), original, recipe)
    evidence = ROOT / ("native/artifacts/" + options.model + "-material-recovery.json")
    evidence.write_text(json.dumps(report, indent=2) + "\n")
    if options.apply:
        if asset.read_bytes() not in (original, result):
            raise ValueError("Current asset is neither the pinned original nor this exact repair")
        backup = ROOT / "native/artifacts/model-material-originals" / asset.name
        backup.parent.mkdir(parents=True, exist_ok=True)
        if backup.exists() and backup.read_bytes() != original:
            raise ValueError("Original backup already exists with different bytes")
        backup.write_bytes(original)
        asset.write_bytes(result)
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
