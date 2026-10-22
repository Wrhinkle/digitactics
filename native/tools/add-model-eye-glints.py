"""Bake an eye glint into Digimon Story Time Stranger eye textures.

Time Stranger draws the eye glint (`<code>e01h.png`) as a separate projected layer.
Its placement comes from shader constants that are not stored in the rip, so the
converted models show pupils without any shine. This tool does not guess that
projection. It paints the highlight that Cyber Sleuth models of the same family
carry in their baked eye textures (Motimon, Palmon, Lopmon, Veemon, Agumon and
Betamon, viewed from the front in rest pose): one round white glint in the upper
quadrant of the iris toward the light, and a small dim one diagonally opposite.
Both eyes take the glint on the same side, as one light would place it.

The light side and offsets are measured in screen space. The tool skins each eye
mesh in rest pose and uses its UV-to-position Jacobian to convert the screen
offsets and the round glint shape into the eye texture, so a rotated or mirrored
eye layout still receives an upper-left glint on screen. The iris extent is read
from the texture's painted disc around each eye mesh's UV center, and the glint is
clipped to that disc. Only the embedded eye image changes; geometry, UVs, rig and
animation bytes are verified unchanged.

Needs numpy, Pillow and Node (for the vendored meshopt decoder).
  python native/tools/add-model-eye-glints.py [--model ts_palmon] [--apply] [--preview DIR]
"""
import argparse
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
# Screen-space placement in iris radii (x to the viewer's right, y up), from the
# Cyber Sleuth references listed above. Radii are fractions of the mean iris radius.
GLINTS = [
    {"offset": [-0.36, 0.38], "radius": 0.25, "softness": 0.35, "opacity": 0.95},
    {"offset": [0.38, -0.40], "radius": 0.09, "softness": 0.45, "opacity": 0.75},
]
GLINT_COLOR = [255, 255, 252]
IRIS_THRESHOLD = 40  # max channel difference from the texture background
SUPERSAMPLE = 4
RECIPES = {
    "ts_mochimon": {
        "asset": "models/files/rip_cat_ts_mochimon_chr388.glb", "image": "chr388p01.png",
        "originalSha256": "9a62a5ac40109ac3085ac49fadec82cd230fca0ce5cfe27a92eb4dc5003cef9c",
        "repairedSha256": "8fcf1534c591bc931eaa8618fa52c1dfddafdb936b093c60d7ce86c262c6c184"
    },
    "ts_palmon": {
        "asset": "models/files/rip_cat_ts_palmon_chr348.glb", "image": "chr348p01.png",
        "originalSha256": "868a034c3ec6dca14733fe5d45ec8b648cbc37d5612b5ae79eb8f37319398e9b",
        "repairedSha256": "5b1eeaca3c8927d89fc7e31290fcd073d940b98387d955faa26ac6d6fd468b55"
    },
    "ts_terriermon": {
        "asset": "models/files/rip_cat_ts_terriermon_lopmon_and_assistant_chr701.glb", "image": "chr701p01.png",
        "originalSha256": "9b96a3fb83d9557c31f8a2ef55e3ef2757fb25be9c2ac6875259412e8f0cc01b",
        "repairedSha256": "f376bb74e1d1f4ffb3f9b456e79562065d5f6c7d76827e78f6ae0a64d3a15ddc"
    },
    "ts_lopmon": {
        "asset": "models/files/rip_cat_ts_terriermon_lopmon_and_assistant_chr750.glb", "image": "chr750p01.png",
        "originalSha256": "1b7abb42bf8ae17db5e02ced7f638258d98c7c2b7ab446531a5af1cf86b14648",
        "repairedSha256": "8d30f25cfd4523dee24fad5bced54d14b8c1f5245fd1ba69fbf717ca49f474b0"
    },
    "ts_veemon": {
        "asset": "models/files/rip_cat_ts_v_mon_chr114.glb", "image": "chr114p01.png",
        "originalSha256": "21b58f9493f16d5475fe498a669b88e171e564f66897640d15c3c863f1bfcd2b",
        "repairedSha256": "79e8ebef1d8d24190bbcb01603102671b415af117e8ac0ea29a5d594e4d9e1be"
    }
}


def load(name, file):
    spec = importlib.util.spec_from_file_location(name, Path(__file__).with_name(file))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


glb = load("material_repair", "repair-model-materials.py")
recovery = load("material_recovery", "recover-model-materials.py")
decals = load("eye_decals", "recover-model-eye-decals.py")


def digest(data):
    return hashlib.sha256(data).hexdigest()


def node_matrix(node):
    if "matrix" in node:
        return np.array(node["matrix"], float).reshape(4, 4).T
    x, y, z, w = node.get("rotation", [0, 0, 0, 1])
    rotation = np.array([[1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)],
                         [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)],
                         [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)]])
    matrix = np.eye(4)
    matrix[:3, :3] = rotation * np.array(node.get("scale", [1, 1, 1]))
    matrix[:3, 3] = node.get("translation", [0, 0, 0])
    return matrix


def eye_meshes(document, data, material_ids):
    """Per eye mesh: rest-pose triangle positions and UV0 of its eye-material primitives."""
    world = {}

    def walk(index, parent):
        world[index] = parent @ node_matrix(document["nodes"][index])
        for child in document["nodes"][index].get("children", []):
            walk(child, world[index])
    for root in document["scenes"][document.get("scene", 0)]["nodes"]:
        walk(root, np.eye(4))
    meshes = []
    for node_index, node in enumerate(document["nodes"]):
        if "mesh" not in node or node_index not in world:
            continue
        positions, uvs = [], []
        for primitive in document["meshes"][node["mesh"]]["primitives"]:
            if primitive.get("material") not in material_ids:
                continue
            attributes = primitive["attributes"]
            points = recovery.accessor(document, data, attributes["POSITION"], True).astype(float)
            homogeneous = np.c_[points, np.ones(len(points))]
            if "skin" in node:
                skin = document["skins"][node["skin"]]
                inverse = recovery.accessor(document, data, skin["inverseBindMatrices"]).reshape(-1, 4, 4).transpose(0, 2, 1)
                joints = np.array([world[j] @ inverse[k] for k, j in enumerate(skin["joints"])])
                weights = recovery.accessor(document, data, attributes["WEIGHTS_0"], True).astype(float)
                weights /= weights.sum(1, keepdims=True)
                skinning = np.einsum("vk,vkij->vij", weights, joints[recovery.accessor(document, data, attributes["JOINTS_0"]).astype(int)])
                points = np.einsum("vij,vj->vi", skinning, homogeneous)[:, :3]
            else:
                points = (homogeneous @ world[node_index].T)[:, :3]
            indices = recovery.accessor(document, data, primitive["indices"]).reshape(-1).astype(int)
            positions.append(points[indices].reshape(-1, 3, 3))
            uvs.append(recovery.accessor(document, data, attributes["TEXCOORD_0"], True).astype(float)[indices].reshape(-1, 3, 2))
        if positions:
            meshes.append({"name": node.get("name"), "positions": np.concatenate(positions), "uvs": np.concatenate(uvs)})
    return meshes


def screen_to_image(positions, uvs):
    """Area-weighted 2x2 map from front-view screen offsets (x right, y up) to UV offsets."""
    total, weight = np.zeros((2, 2)), 0.0
    for p, t in zip(positions, uvs):
        a = np.array([t[1] - t[0], t[2] - t[0]]).T
        b = np.array([p[1, :2] - p[0, :2], p[2, :2] - p[0, :2]]).T
        area = abs(np.linalg.det(a))
        if area < 1e-12:
            continue
        total += area * (b @ np.linalg.inv(a))
        weight += area
    screen_per_uv = total / weight
    return np.linalg.inv(screen_per_uv)  # glTF UVs already address the image (v down)


def bake(image, meshes):
    """Paint the glints on every eye. Returns the image and one row per eye."""
    height, width = image.shape[:2]
    corners = np.concatenate([image[0], image[-1], image[:, 0], image[:, -1]])
    background = np.median(corners, axis=0)
    differs = np.abs(image - background).max(-1) > IRIS_THRESHOLD
    paint = np.zeros((height, width))
    rows = []
    sub = (np.arange(SUPERSAMPLE) + .5) / SUPERSAMPLE
    for mesh in meshes:
        uv = mesh["uvs"].reshape(-1, 2)
        lo, hi = uv.min(0), uv.max(0)
        x0, y0 = np.floor(lo * [width, height]).astype(int)
        x1, y1 = np.ceil(hi * [width, height]).astype(int)
        window = np.zeros_like(differs)
        window[max(y0, 0):y1, max(x0, 0):x1] = True
        iris = differs & window
        ys, xs = np.nonzero(iris)
        if len(xs) < 50:
            raise ValueError(mesh["name"] + ": no painted iris inside the eye UVs")
        center = np.array([(xs.min() + xs.max() + 1) / 2 / width, (ys.min() + ys.max() + 1) / 2 / height])
        uv_center = (lo + hi) / 2
        if np.abs(center - uv_center).max() > 0.25 * (hi - lo).max():
            raise ValueError(mesh["name"] + ": painted iris is not centered on the eye mesh")
        mapping = screen_to_image(mesh["positions"], mesh["uvs"])
        # Iris radius in screen units, measured along the screen axes.
        half = np.array([(xs.max() - xs.min() + 1) / 2 / width, (ys.max() - ys.min() + 1) / 2 / height])
        screen_half = np.abs(np.linalg.inv(mapping) @ np.diag(half)).max(0)
        radius = float(screen_half.mean())
        inverse = np.linalg.inv(mapping)
        bx0, by0 = max(x0, 0), max(y0, 0)
        gx, gy = np.meshgrid(np.arange(bx0, min(x1, width)), np.arange(by0, min(y1, height)))
        shape = gx.shape + (SUPERSAMPLE, SUPERSAMPLE)
        px = np.broadcast_to((gx[..., None, None] + sub[:, None]) / width, shape)
        py = np.broadcast_to((gy[..., None, None] + sub[None, :]) / height, shape)
        offset = np.stack([px - center[0], py - center[1]], -1)
        screen = offset @ inverse.T  # screen units relative to the iris center
        layer = np.zeros(gx.shape)
        for glint in GLINTS:
            spot = np.array(glint["offset"]) * screen_half
            distance = np.linalg.norm(screen - spot, axis=-1)
            r = glint["radius"] * radius
            edge = np.clip((r - distance) / (glint["softness"] * r), 0, 1)
            alpha = (edge * edge * (3 - 2 * edge)).mean((-1, -2)) * glint["opacity"]
            layer = np.maximum(layer, alpha)
        layer *= iris[by0:min(y1, height), bx0:min(x1, width)]
        region = paint[by0:min(y1, height), bx0:min(x1, width)]
        np.maximum(region, layer, out=region)
        rows.append({"mesh": mesh["name"], "irisCenterUv": [round(float(v), 5) for v in center],
                     "irisRadiusScreen": round(radius, 6), "glintTexels": int((layer > 0).sum())})
    alpha = paint[..., None]
    result = image * (1 - alpha) + np.array(GLINT_COLOR, float) * alpha
    return np.clip(result + .5, 0, 255).astype(np.uint8), rows


def add_glints(name, recipe, original):
    if digest(original) != recipe["originalSha256"]:
        raise ValueError(name + ": asset differs from the reviewed original")
    document, chunks = glb.read_glb(original)
    if len(chunks) != 1:
        raise ValueError("Expected one binary chunk")
    before = copy.deepcopy(document)
    binary = chunks[0][1]
    images = [i for i, image in enumerate(document["images"]) if image.get("name") == recipe["image"]]
    if len(images) != 1:
        raise ValueError("Expected one embedded eye image")
    textures = {i for i in range(len(document["textures"])) if glb.image_name(document, i) == recipe["image"]}
    materials = {i for i, m in enumerate(document["materials"])
                 if m.get("pbrMetallicRoughness", {}).get("baseColorTexture", {}).get("index") in textures}
    if not materials or any("_eye" not in document["materials"][i].get("name", "") for i in materials):
        raise ValueError("The eye image must be bound only to eye materials")
    decoded, decoded_chunks = glb.read_glb(recovery.decode_original(original))
    meshes = eye_meshes(decoded, decoded_chunks[0][1], materials)
    if sorted(m["name"] for m in meshes) != ["eye_L", "eye_R"]:
        raise ValueError("Expected the eye_L and eye_R meshes")
    view_index = document["images"][images[0]]["bufferView"]
    shipped = Image.open(io.BytesIO(decals.view_bytes(document, binary, document["bufferViews"][view_index])))
    if shipped.mode != "RGB":
        raise ValueError("Expected an opaque RGB eye image")
    baked, rows = bake(np.asarray(shipped).astype(float), meshes)
    out = io.BytesIO()
    Image.fromarray(baked).save(out, "WEBP", lossless=True, quality=100, method=6)
    new_binary = decals.replace_view(document, binary, view_index, out.getvalue())
    result = glb.write_glb(document, [(chunks[0][0], new_binary)])
    after, after_chunks = glb.read_glb(result)
    for i, (old_v, new_v) in enumerate(zip(before["bufferViews"], after["bufferViews"])):
        if i != view_index and decals.view_bytes(before, binary, old_v) != decals.view_bytes(after, after_chunks[0][1], new_v):
            raise ValueError("Non-image buffer data changed")
    strip = lambda d: {k: v for k, v in d.items() if k not in ("bufferViews", "buffers")}
    if strip(after) != strip(before):
        raise ValueError("GLB metadata other than buffer layout changed")
    report = {"schemaVersion": 1, "asset": recipe["asset"], "originalSha256": recipe["originalSha256"],
              "repairedSha256": digest(result), "eyeImage": recipe["image"], "glints": GLINTS,
              "glintColor": GLINT_COLOR, "eyes": rows, "imageSize": list(shipped.size),
              "preservedGeometryRigAndAnimation": True}
    return result, report, baked


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--model", choices=sorted(RECIPES), action="append")
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--preview", type=Path, help="Write each baked eye image (PNG) and candidate GLB here")
    options = parser.parse_args()
    reports = []
    for name in options.model or sorted(RECIPES):
        recipe = RECIPES[name]
        asset = ROOT / recipe["asset"]
        current = asset.read_bytes()
        backup = ROOT / "native/artifacts/model-material-originals" / (asset.stem + "." + recipe["originalSha256"][:12] + ".glb")
        if digest(current) == recipe["repairedSha256"]:
            if not backup.exists():
                reports.append({"asset": recipe["asset"], "state": "correct", "sha256": recipe["repairedSha256"]})
                continue
            current = backup.read_bytes()
        result, report, baked = add_glints(name, recipe, current)
        if options.preview:
            options.preview.mkdir(parents=True, exist_ok=True)
            Image.fromarray(baked).save(options.preview / (name + ".png"))
            (options.preview / asset.name).write_bytes(result)
        if recipe["repairedSha256"] and report["repairedSha256"] != recipe["repairedSha256"]:
            raise ValueError(name + ": output differs from the reviewed bytes")
        if options.apply:
            if not recipe["repairedSha256"]:
                raise ValueError(name + ": pin repairedSha256 after reviewing the preview before applying")
            backup.parent.mkdir(parents=True, exist_ok=True)
            if backup.exists() and backup.read_bytes() != current:
                raise ValueError("Original backup already exists with different bytes")
            backup.write_bytes(current)
            asset.write_bytes(result)
        reports.append(report)
    print(json.dumps(reports, indent=2))


if __name__ == "__main__":
    main()
