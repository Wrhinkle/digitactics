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
        "archiveSha256": "dfe5d70676668f9e1f96ddfdf5ab406daf7edcf61398edc7602fef5a550ed3e0",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324353/",
        "prefix": "PileVolcamon/chr060", "base": "chr060"
    },
    "triceramon": {
        "asset": "models/files/rip_cs_triceramon.glb",
        "originalSha256": "f5f6675ec24c58a1e9d1c6331ea8fcf056fa675a1f26f6a8e3e14f68845f933d",
        "archiveSha256": "92b53af362f5d024fc2d2903092c7e354a4f7a949c858294eb16fcdc554f30b8",
        "sourceUrl": "https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324322/",
        "prefix": "Triceramon/chr033", "base": "chr033"
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


def recover(archive_data, original, recipe):
    if digest(archive_data) != recipe["archiveSha256"] or digest(original) != recipe["originalSha256"]:
        raise ValueError("Archive or original asset differs from reviewed bytes")
    archive = zipfile.ZipFile(io.BytesIO(archive_data))
    dae_data = archive.read(recipe["prefix"] + ".dae")
    atlas = archive.read(recipe["prefix"] + "a01.png")
    source = ET.fromstring(dae_data)
    # Verify the source assigns this atlas to every surviving base mesh.
    image_paths = [e.text for e in source.findall(".//c:library_images/c:image/c:init_from", NS)]
    if not any(value.endswith(recipe["base"] + "a01.png") for value in image_paths):
        raise ValueError("Expected source albedo is absent")
    document, chunks = glb.read_glb(decode_original(original))
    if len(chunks) != 1 or chunks[0][0] != 0x004E4942:
        raise ValueError("Expected one embedded binary chunk")
    before = copy.deepcopy(document)
    old_data = chunks[0][1]
    data = bytearray(old_data)
    rows = []

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
        if diffuse is None or diffuse.get("texture") != recipe["base"] + "a01_png-sampler":
            raise ValueError("Source mesh does not use the reviewed color atlas")
        inputs = {i.get("semantic"): (int(i.get("offset")), i.get("source")[1:])
                  for i in poly.findall("c:input", NS)}
        stride = 1 + max(value[0] for value in inputs.values())
        if any(int(x) != 3 for x in poly.find("c:vcount", NS).text.split()):
            raise ValueError("Source contains non-triangles")
        faces = np.fromstring(poly.find("c:p", NS).text, sep=" ", dtype=int).reshape(-1, 3, stride)
        positions = arrays[geometry.find("c:mesh/c:vertices/c:input", NS).get("source")[1:]]
        center = (positions.min(0) + positions.max(0)) / 2
        scale = np.ptp(positions, 0).max() / 2
        primitive = mesh["primitives"][0]
        points = accessor(document, old_data, primitive["attributes"]["POSITION"], True)
        indices = accessor(document, old_data, primitive["indices"]).reshape(-1).astype(int)
        distances, nearest = cKDTree(points).query((positions - center) / scale)
        if distances.max() > 0.00015 or len(faces) * 3 != len(indices):
            raise ValueError("Source topology or quantized positions differ")
        keys = [tuple(x) for x in np.round(points * 32767).astype(int)]
        triangles = {}
        for face in faces:
            corners = [keys[nearest[x[inputs["VERTEX"][0]]]] for x in face]
            key = tuple(sorted(corners))
            if key in triangles or len(set(corners)) != 3:
                raise ValueError("Ambiguous source triangle")
            triangles[key] = (corners, arrays[inputs["TEXCOORD"][1]][face[:, inputs["TEXCOORD"][0]]])
        restored = []
        used = set()
        for face in indices.reshape(-1, 3):
            corners = [keys[index] for index in face]
            key = tuple(sorted(corners))
            if key in used or key not in triangles:
                raise ValueError("Source triangle does not map one-to-one")
            used.add(key)
            source_corners, source_uv = triangles[key]
            restored.extend(source_uv[source_corners.index(corner)] for corner in corners)
        restored = np.asarray(restored, dtype="<f4")
        restored[:, 1] = 1 - restored[:, 1]  # Collada vs glTF texture origin.
        row = {"mesh": mesh["name"], "matchedTriangles": len(faces),
               "maximumNormalizedPositionError": float(distances.max()), "ambiguousTriangles": 0}
        if "TEXCOORD_0" in primitive["attributes"]:
            existing = accessor(document, old_data, primitive["attributes"]["TEXCOORD_0"], True)[indices]
            error = float(np.max(np.abs(existing - restored)))
            if error > 0.00015:
                raise ValueError("Recovered UV orientation does not match surviving source UVs")
            row["existingUvMaximumError"] = error
        else:
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
    image = len(document["images"])
    document["images"].append({"name": recipe["base"] + "a01.png", "mimeType": "image/png", "bufferView": append_view(atlas)})
    texture = len(document["textures"])
    document["textures"].append({"source": image, "sampler": document["textures"][0]["sampler"]})
    for material in document["materials"]:
        material["pbrMetallicRoughness"]["baseColorTexture"] = {"index": texture}
        material["pbrMetallicRoughness"]["baseColorFactor"] = [1, 1, 1, 1]
    document["buffers"] = [{"byteLength": len(data)}]
    for key in ("nodes", "skins", "animations", "scenes"):
        if document.get(key) != before.get(key):
            raise ValueError("Rig or animation metadata changed")
    result = glb.write_glb(document, [(0x004E4942, bytes(data) + b"\0" * (-len(data) % 4))])
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
    return result, {"schemaVersion": 1, "asset": recipe["asset"],
                    "originalSha256": recipe["originalSha256"], "repairedSha256": digest(result),
                    "sourceUrl": recipe["sourceUrl"], "archiveSha256": recipe["archiveSha256"],
                    "daeSha256": digest(dae_data), "albedoSha256": digest(atlas),
                    "preservedOriginalTriangleAttributes": True, "preservedRigAndAnimation": True,
                    "meshes": rows}


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
