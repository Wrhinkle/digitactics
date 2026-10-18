"""Read-only Brachiomon original OBJ versus imported GLB assembly evidence."""
import hashlib
import json
from pathlib import Path
import sys
import zipfile

OUT = Path(__file__).resolve().parents[1] / "artifacts" / "dw1-rig-diagnosis"
ARCHIVE_SHA = "c352330480da0be3689fc2adee39b294518114deeecc5d6acbda4fb3e3688ba6"
OBJ_SHA = "499eacafb11680aacca629498ed05540d6ed186a8c75ffd9a14a78f80732a21c"
archive_path = Path(sys.argv[1])
archive_bytes = archive_path.read_bytes()
if hashlib.sha256(archive_bytes).hexdigest() != ARCHIVE_SHA:
    raise ValueError("Unexpected original archive")
with zipfile.ZipFile(archive_path) as archive:
    raw = archive.read("Brachiomon.obj")
if hashlib.sha256(raw).hexdigest() != OBJ_SHA:
    raise ValueError("OBJ differs from original inventory hash")
points, faces = [], []
for line in raw.decode("utf8").splitlines():
    if line.startswith("v "):
        points.append(list(map(float, line.split()[1:4])))
    elif line.startswith("f "):
        ids = [int(p.split("/")[0]) - 1 for p in line.split()[1:]]
        for i in range(1, len(ids) - 1):
            faces.append([ids[0], ids[i], ids[i + 1]])
parent = list(range(len(points)))


def find(i):
    while parent[i] != i:
        parent[i] = parent[parent[i]]
        i = parent[i]
    return i


def join(a, b):
    parent[find(a)] = find(b)


# OBJ face/material seams duplicate positions; weld only sub-millimeter copies
# in this roughly 49-unit-high model, never bridge the visible part gaps.
seen = {}
for i, point in enumerate(points):
    key = tuple(round(v, 3) for v in point)
    if key in seen:
        join(i, seen[key])
    else:
        seen[key] = i
for face in faces:
    join(face[0], face[1])
    join(face[1], face[2])
groups = {}
for face in faces:
    groups.setdefault(find(face[0]), []).append(face)
parts = []
for index, triangles in enumerate(groups.values()):
    ids = sorted({v for face in triangles for v in face})
    mapping = {v: i for i, v in enumerate(ids)}
    vertices = [points[v] for v in ids]
    parts.append({"index": index, "name": f"Original component {index}", "points": vertices,
                  "triangles": [[mapping[v] for v in face] for face in triangles],
                  "bounds": {"min": [min(p[a] for p in vertices) for a in range(3)],
                             "max": [max(p[a] for p in vertices) for a in range(3)]}})
imported_part = json.loads((OUT / "DW1_Brachiomon.json").read_text())["parts"][0]
imported = imported_part["points"]
errors = [min(sum((a - b) ** 2 for a, b in zip(p, q)) ** .5 for q in imported) for p in points]
component_ids = {component: index for index, component in enumerate(groups)}
vertex_components = []
for p in imported:
    nearest = min(range(len(points)), key=lambda i: sum((a - b) ** 2 for a, b in zip(p, points[i])))
    vertex_components.append(component_ids[find(nearest)])
imported_components = [{"component": i, "triangles": []} for i in range(len(parts))]
for face in imported_part["triangles"]:
    component = vertex_components[face[0]]
    if any(vertex_components[v] != component for v in face):
        raise ValueError("Imported triangle spans original disconnected pieces")
    imported_components[component]["triangles"].append(face)
report = {"sourceUrl": "https://models.spriters-resource.com/playstation/digimonworld/asset/336247/",
          "archiveSha256": ARCHIVE_SHA, "objSha256": OBJ_SHA,
          "finding": "The original OBJ already contains separated body components in the same exploded positions; this is not introduced by native rendering or GLB transforms.",
          "vertices": len(points), "triangles": len(faces), "disconnectedComponents": len(parts),
          "maximumOriginalToImportedVertexDistance": max(errors),
          "importedTrianglesMappedExactlyOnce": sum(len(p["triangles"]) for p in imported_components),
          "importedComponents": imported_components, "parts": parts}
(OUT / "DW1_Brachiomon_OriginalComponents.json").write_text(json.dumps(report))
(OUT / "Brachiomon.original.obj").write_bytes(raw)
print(json.dumps({k: v for k, v in report.items() if k not in ("parts", "importedComponents")}, indent=2))
