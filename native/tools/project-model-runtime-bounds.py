"""CPU-only projection of existing normalized runtime AABBs. Requires NumPy.

No model loading, skinning, exporter, renderer, or visual approval is involved.
Run with --self-test for focused projection/placement checks only.
"""
import argparse
import gzip
import hashlib
import json
import math
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
LABELS = ["contained AABB", "intersects edges", "completely outside camera"]
PLANES = ["left", "right", "bottom", "top", "near", "far"]
SIGNS = np.array([[i & 1, (i >> 1) & 1, (i >> 2) & 1] for i in range(8)])
SOURCE_FILES = [
    "native/src/DigiTactics.Desktop/BattleView.cs",
    "native/src/DigiTactics.Desktop/ArenaCamera.cs",
    "native/src/DigiTactics.Desktop/GpuModel.cs",
    "native/src/DigiTactics.Desktop/ModelRenderer.cs",
    "native/src/DigiTactics.Desktop/ModelVisualAudit.cs",
    "native/src/DigiTactics.Desktop/MatchScreen.cs",
    "native/src/DigiTactics.Desktop/NativeUiPlatform.cs",
    "native/src/DigiTactics.Presentation/CombatScene.cs",
]


def digest(data):
    return hashlib.sha256(data).hexdigest()


def cell(column, row, columns=7):
    return np.array([(column + .5 * (row & 1) - (columns - .5) / 2) * math.sqrt(3), 0, (row - 3.5) * 1.5])


def outside_hex_footprint(points):
    """Center-only test against the union of 56 radius-one pointy-top hexes."""
    centers = np.array([cell(c, r)[[0, 2]] for r in range(8) for c in range(7)])
    delta = np.abs(np.asarray(points)[:, None, [0, 2]] - centers[None, :, :])
    inside = (delta[:, :, 0] <= math.sqrt(3) / 2 + 1e-9) & (delta[:, :, 1] <= 1 + 1e-9)
    inside &= delta[:, :, 0] + math.sqrt(3) * delta[:, :, 1] <= math.sqrt(3) + 1e-9
    return ~np.any(inside, axis=1)


def camera(profile, width, height, columns=7):
    """System.Numerics row-vector LookAt * Perspective, including float inputs."""
    f32 = np.float32
    if profile == "audit-board":
        distance = f32(max(24, (columns * math.sqrt(3) + 4) / (2 * math.tan(.36) * width / height)))
        eye = np.array([0, distance * f32(.6644), distance * f32(.7474)], dtype=np.float32)
        target = np.zeros(3, dtype=np.float32)
        fov, far = f32(.72), f32(100)
    elif profile == "live-default":
        distance = math.hypot(16.5, 15.5) * max(1, 1.35 * height / width) * (1.65 if columns > 7 else 1)
        pitch = math.atan2(16.5, 15.5)
        target = np.array([0, 0, 1.8], dtype=np.float32)
        eye = target + np.array([0, math.sin(pitch) * distance, math.cos(pitch) * distance], dtype=np.float32)
        fov, far = f32(f32(38) * f32(math.pi) / f32(180)), f32(200)
    else:
        raise ValueError(profile)
    z = eye - target
    z /= f32(np.sqrt(np.dot(z, z)))
    x = np.cross(np.array([0, 1, 0], dtype=np.float32), z)
    x /= f32(np.sqrt(np.dot(x, x)))
    y = np.cross(z, x)
    view = np.eye(4, dtype=np.float32)
    view[:3, :3] = np.stack([x, y, z], axis=1)
    view[3, :3] = [-np.dot(x, eye), -np.dot(y, eye), -np.dot(z, eye)]
    near, aspect = f32(.1), f32(width) / f32(height)
    y_scale = f32(1) / f32(np.tan(f32(fov * f32(.5))))
    depth = far / (near - far)
    projection = np.zeros((4, 4), dtype=np.float32)
    projection[0, 0], projection[1, 1] = y_scale / aspect, y_scale
    projection[2, 2], projection[2, 3], projection[3, 2] = depth, -1, near * depth
    return (view @ projection).astype(np.float64), eye.tolist(), target.tolist()


def placement(metadata, footprint, stage, tier, location):
    height = [1.3, 1.65, 2.0][stage] + (.15 if tier == 5 else 0)
    scale = min(height, metadata.get("maxFootprint", 1.9) / max(footprint, .01))
    yaw = metadata.get("rotY", 0) + math.pi
    cosine, sine = math.cos(yaw), math.sin(yaw)
    transform = np.array([[cosine, 0, -sine, 0], [0, 1, 0, 0], [sine, 0, cosine, 0], [0, 0, 0, 1]], dtype=float)
    transform[:3, :3] *= scale
    transform[3, :3] = location + [0, metadata.get("yOffset", 0) * scale, 0]
    return transform, scale


def project(minimum, maximum, transform, projection):
    """Project all eight corners; a common separating plane proves outside.

    A remaining intersection is conservative: OBB/frustum edge separating axes
    and actual triangles are not tested. ModelRenderer maps z[0,w] to GL[-w,w].
    """
    minimum, maximum = np.asarray(minimum), np.asarray(maximum)
    corners = minimum[:, None, :] + (maximum - minimum)[:, None, :] * SIGNS
    points = np.concatenate([corners, np.ones((*corners.shape[:2], 1))], axis=2)
    clip = points @ (transform @ projection)
    x, y, z, w = (clip[:, :, i] for i in range(4))
    planes = np.stack([w + x, w - x, w + y, w - y, z, w - z], axis=2)
    outside = np.any(np.all(planes < -1e-7, axis=1), axis=1)
    contained = np.all(planes >= 1e-7, axis=(1, 2))
    labels = np.where(outside, 2, np.where(contained, 0, 1))
    front = np.all(w > 1e-7, axis=1)
    safe_w = np.where(np.abs(w) < 1e-9, 1e-9, w)
    ndc = clip[:, :, :3] / safe_w[:, :, None]
    rects = np.stack([ndc[:, :, 0].min(axis=1), ndc[:, :, 1].min(axis=1), ndc[:, :, 0].max(axis=1), ndc[:, :, 1].max(axis=1)], axis=1)
    # Among equally classified samples, prefer the greatest viewport overrun.
    scores = np.maximum.reduce([-rects[:, 0] - 1, -rects[:, 1] - 1, rects[:, 2] - 1, rects[:, 3] - 1])
    scores = np.where(front, scores, 1e6 + np.max(-w, axis=1))
    return labels, scores, rects, front, planes


def self_test():
    matrix, _, _ = camera("audit-board", 960, 640)
    # Independent System.Numerics Matrix4x4 output, obtained without a GPU.
    expected = np.array([[1.771152, 0, 0, 0], [0, 1.98560441, -.6650537, -.664388657],
                         [0, -1.7651, -.7481353, -.7473872], [0, .00000253365329, 23.9243317, 24.0004063]])
    assert np.max(np.abs(matrix - expected)) < 4e-6, (matrix, expected)
    live_matrix, _, _ = camera("live-default", 568, 254)
    live_expected = np.array([[1.29871416, 0, 0, 0], [0, 1.98844206, -.729212761, -.7288481],
                              [0, -2.11672878, -.685018, -.684675455], [0, 3.81011248, 23.7827721, 23.8708782]])
    assert np.max(np.abs(live_matrix - live_expected)) < 4e-6, (live_matrix, live_expected)
    assert np.allclose(cell(3, 4), [-math.sqrt(3) / 4, 0, .75])
    assert np.allclose(cell(3, 7), [math.sqrt(3) / 4, 0, 5.25])
    assert outside_hex_footprint([cell(3, 4), cell(3, 7), [100, 0, 0]]).tolist() == [False, False, True]
    transform, scale = placement({"maxFootprint": 1.9, "rotY": math.pi / 2, "yOffset": .25}, 2, 2, 5, cell(3, 4))
    assert scale == .95
    assert np.allclose(np.array([1, 2, 3, 1]) @ transform, [cell(3, 4)[0] - 2.85, 2.1375, 1.7, 1])
    identity = np.eye(4)
    lo = [[-.2, -.2, .2], [2, -.2, .2], [.8, -.2, .2], [-.2, -.2, -.3], [-.2, -.2, 1.2], [-.2, -.2, -.1]]
    hi = [[.2, .2, .8], [3, .2, .8], [1.2, .2, .8], [.2, .2, -.1], [.2, .2, 1.3], [.2, .2, .1]]
    labels, *_ = project(lo, hi, identity, identity)
    assert labels.tolist() == [0, 2, 1, 2, 2, 1], labels
    print("Projection math checks passed: both native cameras, placement, hex coordinates/footprint, and six frustum cases.")


def first_aliases(models, rules):
    tiers = {v["id"]: {line["id"]: line["tier"] for line in v["lines"] + v["extraLines"]} for v in rules["variants"]}
    aliases = {}
    for variant, lines in models["variants"].items():
        for line, sources in lines.items():
            for stage, source in enumerate(sources):
                if source not in aliases:
                    aliases[source] = {"variant": variant, "line": line, "stage": stage, "tier": tiers[variant][line]}
    return aliases


def candidate(sample, context, label, score, rect, front, planes):
    width, height = context["viewport"]
    return {"label": LABELS[int(label)], "phase": sample["Phase"], "time": sample["Time"], "context": context["id"],
            "viewport": [width, height], "cell": context["cell"], "allCornersInFront": bool(front),
            "outsidePlanes": [PLANES[i] for i in range(6) if np.all(planes[:, i] < -1e-7)],
            "projectedRectanglePixels": [round((rect[0] + 1) * width / 2, 3), round((1 - rect[3]) * height / 2, 3),
                                          round((rect[2] + 1) * width / 2, 3), round((1 - rect[1]) * height / 2, 3)] if front else None,
            "normalizedAabb": {"min": sample["Min"], "max": sample["Max"]}, "_rank": (int(label), float(score))}


def run(args):
    input_path, summary_path, models_path, rules_path = map(Path, [args.input, args.summary, args.models, args.catalog])
    packed = input_path.read_bytes()
    raw = gzip.decompress(packed) if input_path.suffix == ".gz" else packed
    audit, summary = json.loads(raw), json.loads(summary_path.read_bytes())
    model_bytes, rule_bytes = models_path.read_bytes(), rules_path.read_bytes()
    assert digest(packed) == summary["compressedSha256"], "Runtime gzip does not match its summary"
    assert digest(raw) == summary["evidenceSha256"], "Runtime JSON does not match its summary"
    assert digest(model_bytes) == summary["catalogSha256"], "Model catalog differs from sampled runtime; supply its matching frozen catalog"
    models, rules = json.loads(model_bytes), json.loads(rule_bytes)
    aliases = first_aliases(models, rules)
    code_hashes = {name: digest((ROOT / name).read_bytes()) for name in SOURCE_FILES}
    for name in ["native/src/DigiTactics.Desktop/GpuModel.cs", "native/src/DigiTactics.Desktop/BattleView.cs"]:
        assert code_hashes[name] == summary["codeHashes"][name.removeprefix("native/")], "Projection source differs from runtime audit: " + name
    contexts = []
    for profile in ["audit-board", "live-default"]:
        for width, height in [(960, 640), (568, 254)]:
            projection, eye, target = camera(profile, width, height)
            for row in [4, 7]:
                contexts.append({"id": f"{profile}/{width}x{height}/cell-3-{row}", "camera": profile, "viewport": [width, height],
                                 "cell": [3, row], "eye": eye, "target": target, "matrix": projection.tolist()})
    results, counts, counts_by_placement, errors = [], {}, {}, []
    for key, evidence in audit["perSource"].items():
        if evidence.get("Error"):
            errors.append({"source": key, "error": evidence["Error"]}); continue
        source = models["sources"][key]
        alias = aliases.get(key, {"variant": None, "line": None, "stage": 2, "tier": 1})
        applicability = "roster-unit" if key in aliases else "hypothetical-unit-preview"
        row = {"source": key, "uid": source["metadata"].get("uid"), "assetSha256": models["assets"][source["asset"]]["sha256"],
               "motionSha256": source.get("motion", {}).get("sha256"), "alias": alias, "footprint": evidence["BaseFootprint"],
               "placementApplicability": applicability, "hasRosterAlias": key in aliases,
               "metadata": {name: source["metadata"].get(name, fallback) for name, fallback in [("rotY", 0), ("yOffset", 0), ("maxFootprint", 1.9)]}, "phases": []}
        for phase in evidence["Phases"]:
            samples = phase["Samples"]
            if not samples: continue
            minimum, maximum = [s["Min"] for s in samples], [s["Max"] for s in samples]
            for profile in ["audit-board", "live-default"]:
                phase_counts = [0, 0, 0]
                worst, farthest_center, center_outside_count = None, None, 0
                for context in [c for c in contexts if c["camera"] == profile]:
                    transform, scale = placement(source["metadata"], evidence["BaseFootprint"], alias["stage"], alias["tier"], cell(*context["cell"]))
                    row["scale"] = scale
                    labels, scores, rects, front, planes = project(minimum, maximum, transform, np.array(context["matrix"]))
                    centers = (np.asarray(minimum) + np.asarray(maximum)) / 2
                    centers = np.concatenate([centers, np.ones((len(centers), 1))], axis=1) @ transform
                    center_outside = outside_hex_footprint(centers[:, :3])
                    distances = np.linalg.norm(centers[:, [0, 2]] - cell(*context["cell"])[[0, 2]], axis=1)
                    center_outside_count += int(center_outside.sum())
                    center_index = max(range(len(samples)), key=lambda i: (center_outside[i], distances[i]))
                    center_record = {"time": samples[center_index]["Time"], "context": context["id"],
                                     "worldCenter": centers[center_index, :3].tolist(), "distanceFromCell": float(distances[center_index]),
                                     "outsidePlayableHexFootprint": bool(center_outside[center_index])}
                    if farthest_center is None or (center_record["outsidePlayableHexFootprint"], center_record["distanceFromCell"]) > (farthest_center["outsidePlayableHexFootprint"], farthest_center["distanceFromCell"]):
                        farthest_center = center_record
                    for label in range(3): phase_counts[label] += int(np.count_nonzero(labels == label))
                    index = max(range(len(samples)), key=lambda i: (labels[i], scores[i]))
                    current = candidate(samples[index], context, labels[index], scores[index], rects[index], front[index], planes[index])
                    if worst is None or current["_rank"] > worst["_rank"]: worst = current
                worst.pop("_rank")
                counts.setdefault(profile, [0, 0, 0])
                counts[profile] = [a + b for a, b in zip(counts[profile], phase_counts)]
                counts_by_placement.setdefault(profile, {}).setdefault(applicability, [0, 0, 0])
                counts_by_placement[profile][applicability] = [a + b for a, b in zip(counts_by_placement[profile][applicability], phase_counts)]
                row["phases"].append({"phase": phase["Name"], "clip": phase["Clip"], "runtimeSupported": phase["RuntimeSupported"],
                                      "camera": profile, "sampleCountPerContext": len(samples), "counts": dict(zip(LABELS, phase_counts)), "worst": worst,
                                      "aabbCenterOutsideBoardSamples": center_outside_count, "farthestCenter": farthest_center})
        results.append(row)
    snapshots = []
    for path in sorted((ROOT / "native/artifacts").glob("model-visual-audit*/*.board-*.json")):
        capture = json.loads(path.read_bytes())
        hashes = capture.get("RuntimeHashes", {})
        catalog_match = hashes.get("models.json") == summary["catalogSha256"]
        assembly_match = hashes.get("DigiTactics.Assets.dll") == summary.get("assemblyHashes", {}).get("DigiTactics.Assets.dll")
        snapshots.append({"file": str(path.relative_to(ROOT)), "source": capture.get("Source"), "role": capture.get("Role"),
                          "times": capture.get("Times"), "catalogMatches": catalog_match, "assetsAssemblyMatches": assembly_match,
                          "status": "matching catalog and assets assembly; sampling times and initial idle phase still differ" if catalog_match and assembly_match else "not an exact matching-runtime comparison"})
    report = {"schemaVersion": 1, "status": "TRIAGE_ONLY_UNVERIFIED", "policy": "Conservative projection of eight corners of existing normalized AABBs; no visual approval.",
              "limitations": ["Intersects edges means a possible intersection; AABB overcoverage can flag empty space.",
                             "All model geometry contributes to input AABBs; attached effects and weapons cannot be separated from the body.",
                             "AABB-center membership in the union of playable hexes is a separate geometry diagnostic, not body travel or camera clipping proof.",
                             "Occlusion, transparency, textures, anatomy, and screen-space silhouette readability are not evaluated.",
                             "Only sampled instants are evaluated. Raw cycle, runtime blend, movement, and transition phases remain separate.",
                             "Movement samples contain pose changes only; this projection fixes actor placement, yaw PI, body offset zero and scale one.",
                             "Live camera covers its reset state only; user orbit, pan, zoom and camera shake are excluded.",
                             "Only roster-unit rows have an actual unit alias. Other rows use hypothetical stage2/tier1 unit placement; PlaceCompanion and companion clipping are not covered.",
                             "AABB normalization and Footprint come from the runtime report's same coarse idle bounds as GpuModel; no renormalization is performed."],
              "input": {"runtime": str(input_path), "runtimeSha256": digest(packed), "runtimeJsonSha256": digest(raw), "summarySha256": digest(summary_path.read_bytes()),
                        "runtimeGeneratedUtc": audit["generatedUtc"], "modelsSha256": digest(model_bytes), "rulesSha256": digest(rule_bytes), "runtimeCodeHashes": summary["codeHashes"],
                        "runtimeAssemblyHashes": summary.get("assemblyHashes", {}), "projectionSourceHashes": code_hashes, "toolSha256": digest(Path(__file__).read_bytes())},
              "minimumViewport": {"window": [1100, 760], "board": [568, 254], "formula": "MatchScreen: max(300,width-532),max(120,height-506), at 100% UI scale"},
              "boardFootprint": "Union of 56 radius-one pointy-top hexagons centered by CombatLayout.Cell; center-only test, not model containment.",
              "contexts": contexts, "counts": {profile: dict(zip(LABELS, values)) for profile, values in counts.items()},
              "countsByPlacement": {profile: {applicability: dict(zip(LABELS, values)) for applicability, values in groups.items()} for profile, groups in counts_by_placement.items()},
              "sources": results, "errors": errors, "snapshotProvenance": snapshots}
    # Keep the result coherent if another agent starts exporting while this runs.
    assert models_path.read_bytes() == model_bytes and rules_path.read_bytes() == rule_bytes, "Placement inputs changed during projection"
    assert all(digest((ROOT / name).read_bytes()) == value for name, value in code_hashes.items()), "Projection sources changed during projection"
    output = Path(args.output); output.parent.mkdir(parents=True, exist_ok=True)
    encoded = json.dumps(report, separators=(",", ":"), allow_nan=False).encode()
    output.write_bytes(gzip.compress(encoded, mtime=0))
    flagged = [{"source": row["source"], "alias": row["alias"], "placementApplicability": row["placementApplicability"], "hasRosterAlias": row["hasRosterAlias"], "worst": p["worst"], "camera": p["camera"]}
               for row in results for p in row["phases"] if p["worst"]["label"] != LABELS[0]]
    brief = {key: value for key, value in report.items() if key not in ["sources", "snapshotProvenance"]}
    outside = [r for r in flagged if r["camera"] == "live-default" and r["worst"]["phase"].endswith("-runtime") and r["worst"]["label"] == LABELS[2]]
    brief.update({"sourceCount": len(results), "flaggedSourceCount": len({r["source"] for r in flagged}), "flaggedPhaseContexts": flagged,
                  "sourceCountByPlacement": {app: sum(r["placementApplicability"] == app for r in results) for app in ["roster-unit", "hypothetical-unit-preview"]},
                  "flaggedSourceCountByPlacement": {app: len({r["source"] for r in flagged if r["placementApplicability"] == app}) for app in ["roster-unit", "hypothetical-unit-preview"]},
                  "outsideLiveActionRuntime": outside,
                  "outsideLiveActionRuntimeByPlacement": {app: [r for r in outside if r["placementApplicability"] == app] for app in ["roster-unit", "hypothetical-unit-preview"]},
                  "snapshotCount": len(snapshots), "matchingRuntimeSnapshotCount": sum(s["catalogMatches"] and s["assetsAssemblyMatches"] for s in snapshots),
                  "reportSha256": digest(output.read_bytes())})
    brief_path = output.with_name(output.name.removesuffix(".json.gz") + "-summary.json")
    brief_path.write_text(json.dumps(brief, indent=2, allow_nan=False) + "\n")
    print(json.dumps({"sources": len(results), "flaggedSources": brief["flaggedSourceCount"], "counts": report["counts"], "report": str(output), "bytes": output.stat().st_size,
                      "matchingRuntimeSnapshots": brief["matchingRuntimeSnapshotCount"]}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--self-test", action="store_true")
    parser.add_argument("--input", default=ROOT / "native/artifacts/model-runtime-audit.json.gz")
    parser.add_argument("--summary", default=ROOT / "native/artifacts/model-runtime-summary.json")
    parser.add_argument("--models", default=ROOT / "native/content/models.json")
    parser.add_argument("--catalog", default=ROOT / "native/content/catalog.json")
    parser.add_argument("--output", default=ROOT / "native/artifacts/model-runtime-projection.json.gz")
    options = parser.parse_args()
    self_test()
    if not options.self_test:
        run(options)
