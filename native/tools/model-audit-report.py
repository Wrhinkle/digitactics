"""Join exhaustive model evidence without treating numerical or raster checks as visual approval."""
from pathlib import Path
import argparse
import csv
import hashlib
import importlib.util
import json
from collections import Counter

ROOT = Path(__file__).resolve().parents[2]
NATIVE = ROOT / "native"
ARTIFACTS = NATIVE / "artifacts"
STATUS = {"VERIFIED", "FAIL", "UNVERIFIED"}
ROLES = {"idle", "movement", "attack", "special", "win"}
ASSEMBLIES = {"DigiTactics.Desktop.dll", "DigiTactics.Assets.dll", "DigiTactics.Presentation.dll", "DigiTactics.Rules.dll"}


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def capture_source_paths():
    spec = importlib.util.spec_from_file_location("capture_provenance", Path(__file__).with_name("freeze-model-audit.py"))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    module.ROOT = ROOT
    return module.source_paths()


def companion_contexts(companions):
    contexts = {}
    for visual in [*companions.get("bases", {}).values(), *companions.get("forms", {}).values()]:
        contexts.setdefault(visual["source"], visual)
    return contexts


def source_identity(catalog, companions, key):
    source = catalog.get("sources", {}).get(key)
    if source is None or source["asset"] not in catalog.get("assets", {}):
        return None
    context = companion_contexts(companions).get(key)
    return {"asset": source["asset"], "assetSha256": catalog["assets"][source["asset"]]["sha256"],
            "motionSha256": source.get("motion", {}).get("sha256"), "metadata": source["metadata"],
            "companion": context, "fallbacks": companions.get("fallbacks") if context else None}


def capture_provenance(directory):
    """A capture is current only for its measured data and applicable code groups."""
    result = {"valid": False, "reason": "Capture has no measured build/source manifest.", "hashes": {}, "groups": {}}
    path = directory / "capture-manifest.json"
    if not path.is_file():
        return result
    manifest = read(path)
    result["manifest"] = str(path.relative_to(ROOT))
    if manifest.get("schemaVersion") != 1 or manifest.get("sourceStableBeforeAfterBuild") is not True or not manifest.get("policy"):
        result["reason"] = "Capture source stability/policy was not established before and after its build."
        return result
    hashes = manifest.get("runtimeHashes", {})
    if not ASSEMBLIES.issubset(hashes) or not all(hashes[name] for name in ASSEMBLIES):
        result["reason"] = "Capture does not identify every rendering/runtime assembly."
        return result
    result["hashes"] = {name: hashes[name] for name in ASSEMBLIES}
    for name in ["models.json", "companion-models.json"]:
        entry = manifest.get("frozenCatalogs", {}).get(name, {})
        frozen = (directory / entry.get("file", "")).resolve()
        if not frozen.is_relative_to(directory.resolve()) or not frozen.is_file() or sha(frozen) != entry.get("sha256"):
            result["reason"] = "Frozen catalog is missing or its measured hash changed: " + name
            return result
        result[name] = read(frozen)
        result["hashes"][name] = entry["sha256"]
    declared = manifest.get("sourceGroups", {})
    if not {"renderer", "combat", "companion"}.issubset(declared):
        result["reason"] = "Capture omits a required runtime source group."
        return result
    expected_groups = capture_source_paths()
    legacy_pose = "pose" not in declared
    for group, paths in expected_groups.items():
        recorded = dict(declared.get(group, {}))
        # Earlier measured manifests recorded ModelAnimator inside combat. This
        # reuses that exact measured entry, never a guessed current hash.
        if legacy_pose and group == "pose":
            recorded = {name: declared["combat"][name] for name in paths if name in declared["combat"]}
        elif legacy_pose and group == "combat":
            for name in expected_groups.get("pose", []):
                recorded.pop(name, None)
        complete = bool(paths) and set(recorded) == set(paths)
        result["groups"][group] = complete and all((ROOT / name).is_file() and sha(ROOT / name) == expected for name, expected in recorded.items())
    result.update(valid=True, reason="", policy=manifest["policy"])
    return result


def record_matches_capture(capture, hashes):
    return capture["valid"] and all(hashes.get(name) == expected for name, expected in capture["hashes"].items())


def source_matches_capture(capture, catalog, companions, key):
    if not capture["valid"]:
        return False
    old = source_identity(capture["models.json"], capture["companion-models.json"], key)
    return old is not None and old == source_identity(catalog, companions, key)


def role_coverage(data, required):
    frames = data.get("Frames", 0)
    roles = data.get("Roles", [])
    samples = data.get("Samples", [])
    if frames < 2 or len(set(roles)) != len(roles) or not required.issubset(roles):
        return False
    actual = [(sample.get("Role"), sample.get("Frame")) for sample in samples]
    return len(actual) == len(set(actual)) and set(actual) == {(role, frame) for role in roles for frame in range(frames)}


def visual_groups(catalog):
    contexts = {}
    companion_path = NATIVE / "content/companion-models.json"
    if companion_path.is_file():
        companions = read(companion_path)
        for visual in [*companions.get("bases", {}).values(), *companions.get("forms", {}).values()]:
            contexts.setdefault(visual["source"], {name: value for name, value in visual.items() if name != "source"})
    groups = {}
    for key, source in catalog["sources"].items():
        identity = {"assetSha256": catalog["assets"][source["asset"]]["sha256"],
                    "motionSha256": source.get("motion", {}).get("sha256"),
                    "metadata": source["metadata"], "companion": contexts.get(key),
                    "policy": "ModelAnimator+CombatChoreography+CompanionAnimator/v1"}
        signature = hashlib.sha256(json.dumps(identity, sort_keys=True, separators=(",", ":")).encode()).hexdigest()
        groups.setdefault(signature, []).append(key)
    return {"schemaVersion": 1, "catalogSha256": sha(NATIVE / "content/models.json"),
            "signatureDefinition": "SHA256 of canonical sorted-key JSON {assetSha256,motionSha256,metadata,companion,policy}; informational inventory grouping only. Render reuse requires the renderer's explicit current identity-index and AliasOf record.",
            "sourceCount": len(catalog["sources"]),
            "groups": [{"signature": signature, "representative": keys[0], "sourceKeys": keys}
                       for signature, keys in groups.items()]}


def indexed_sources(document):
    rows = document.get("sources", document.get("perSource", []))
    if isinstance(rows, dict):
        return rows
    return {row.get("sourceKey", row.get("SourceKey")): row for row in rows}


def verdict(value):
    return value if value in STATUS else "UNVERIFIED"


def merge(args):
    for name in ["inventory", "runtime", "visual", "output"]:
        if getattr(args, name, None) is not None:
            setattr(args, name, Path(getattr(args, name)).resolve())
    catalog = read(NATIVE / "content/models.json")
    catalog_hash = sha(NATIVE / "content/models.json")
    companion_path = NATIVE / "content/companion-models.json"
    companions = read(companion_path) if companion_path.is_file() else {}
    contexts = companion_contexts(companions)
    capture = capture_provenance(args.visual)
    grouping = visual_groups(catalog)
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    group_path = ARTIFACTS / "model-audit-visual-groups.json"
    group_path.write_text(json.dumps(grouping, indent=2) + "\n", encoding="utf-8")
    if args.groups_only:
        print(f"{grouping['sourceCount']} source mappings / {len(grouping['groups'])} exact visual identities: {group_path}")
        return
    inventory = read(args.inventory)
    if inventory["catalogSha256"] != catalog_hash:
        raise ValueError("Inventory was measured against a different model catalog. Rerun it before merging.")
    for name, expected in inventory.get("codeIdentity", {}).items():
        path = NATIVE / name
        if not path.is_file() or sha(path) != expected:
            raise ValueError("Inventory source code changed since measurement: " + name)
    rows = indexed_sources(inventory)
    if set(rows) != set(catalog["sources"]):
        raise ValueError("Inventory does not enumerate every shipped source mapping exactly.")
    runtime, runtime_valid, runtime_reason = {}, False, "Runtime audit has not been supplied."
    if args.runtime and args.runtime.exists():
        document = read(args.runtime)
        runtime = indexed_sources(document)
        runtime_valid = document.get("catalogSha256") == catalog_hash
        runtime_code = document.get("codeHashes", {})
        runtime_valid = runtime_valid and all((NATIVE / name).is_file() and sha(NATIVE / name) == expected for name, expected in runtime_code.items())
        runtime_reason = "" if runtime_valid else "Runtime evidence has no matching catalog SHA; it cannot establish a current verification."
    visual, reviews, render_identities = {}, {}, {}
    if args.visual.is_dir():
        for path in args.visual.glob("[0-9][0-9][0-9][0-9].json"):
            data = read(path)
            key = data.get("Source", data.get("sourceKey"))
            if key in visual:
                raise ValueError(f"Duplicate rendered evidence for {key}")
            visual[key] = (data, str(path.relative_to(ROOT)))
        review_path = args.visual / "reviews.json"
        if review_path.exists():
            reviews = indexed_sources(read(review_path))
        index_path = args.visual / "identity-index.json"
        if index_path.exists():
            index = read(index_path)
            if capture["valid"] and index.get("CatalogSha256") == capture["hashes"]["models.json"] and index.get("CompanionCatalogSha256") == capture["hashes"]["companion-models.json"]:
                identities = index.get("Sources", [])
                render_identities = {row["Source"]: row for row in identities}
                if set(render_identities) != set(capture["models.json"]["sources"]) or len(identities) != len(render_identities):
                    raise ValueError("Renderer identity index does not enumerate every shipped source exactly.")
    identity_by_source = {key: group for group in grouping["groups"] for key in group["sourceKeys"]}
    asset_evidence = {row["asset"]: row for row in inventory["assets"]}
    matrix = []
    for key, source in sorted(catalog["sources"].items()):
        row = rows[key]
        model = asset_evidence[source["asset"]]
        group = identity_by_source[key]
        runtime_row = runtime.get(key)
        runtime_status = "UNVERIFIED"
        runtime_notes = []
        if runtime_row:
            runtime_status = verdict(runtime_row.get("status", runtime_row.get("Status"))) if runtime_valid else "UNVERIFIED"
            if runtime_valid and runtime_row.get("Error", runtime_row.get("error")):
                runtime_status = "FAIL"
            runtime_notes = runtime_row.get("review", runtime_row.get("ReviewFlags", runtime_row.get("reviewFlags", [])))
        if not runtime_valid:
            runtime_notes = [*runtime_notes, runtime_reason]
        evidence = visual.get(key)
        reused_from = None
        alias_file = None
        if evidence and evidence[0].get("Status") == "alias-unreviewed":
            alias, alias_file = evidence
            target = alias.get("AliasOf")
            identity, representative = render_identities.get(key), render_identities.get(target)
            valid = identity and representative and record_matches_capture(capture, alias.get("RuntimeHashes", {}))
            valid = valid and source_matches_capture(capture, catalog, companions, key) and source_matches_capture(capture, catalog, companions, target)
            valid = valid and identity.get("Signature") == representative.get("Signature") == alias.get("Signature")
            valid = valid and identity.get("Representative") == representative.get("Index") == alias.get("RepresentativeIndex")
            valid = valid and alias.get("Index") == identity.get("Index") and alias.get("Asset") == source["asset"]
            target_evidence = visual.get(target)
            valid = valid and target_evidence and target_evidence[0].get("Status") != "alias-unreviewed"
            if valid:
                evidence, reused_from = target_evidence, target
        render_status = "UNVERIFIED"
        render_flags = []
        render_file = None
        render_current = False
        texture_current = False
        coverage = False
        image = texture_image = None
        required_roles = ROLES | ({"companion-walk"} if key in contexts else set())
        required_groups = {"renderer", "pose", "combat"} | ({"companion"} if key in contexts else set())
        if evidence:
            data, render_file = evidence
            expected = catalog["sources"][reused_from or key]
            # An asset + prepared-motion identity is required even for a technical render.
            motion = data.get("Motion")
            motion_matches = data.get("Status") == "render-error" or motion in (expected.get("motion", {}).get("sha256"), expected.get("motion", {}).get("file"))
            asset_matches = data.get("Asset") == expected["asset"]
            capture_matches = record_matches_capture(capture, data.get("RuntimeHashes", {}))
            data_matches = source_matches_capture(capture, catalog, companions, key)
            if reused_from:
                data_matches = data_matches and source_matches_capture(capture, catalog, companions, reused_from)
            runtime_matches = all(capture["groups"].get(group, False) for group in required_groups)
            render_current = capture_matches and data_matches and runtime_matches and asset_matches and motion_matches
            texture_current = capture_matches and data_matches and asset_matches and motion_matches and all(capture["groups"].get(group, False) for group in ["renderer", "pose"])
            image_name = data.get("Image")
            image = (ROOT / image_name) if image_name else None
            if image and not image.is_file():
                image = args.visual / image_name
            if data.get("TextureImage"):
                texture_image = args.visual / data["TextureImage"]
                if not texture_image.is_file():
                    texture_image = ROOT / data["TextureImage"]
            render_flags = data.get("Flags", [])
            coverage = role_coverage(data, required_roles)
            if data.get("Status") == "render-error" and render_current:
                render_status = "FAIL"
                render_flags = [*render_flags, data.get("Error", "Native render failed.")]
            elif render_current and image and image.is_file() and coverage:
                samples = data["Samples"]
                render_status = "VERIFIED" if all(sample.get("Draws", 0) > 0 and sample.get("VisiblePixels", 0) > 0 for sample in samples) else "FAIL"
            else:
                render_flags = [*render_flags, capture["reason"] or "Rendered data/runtime identity, image, or complete role/frame coverage is not current."]
        review = reviews.get(key)
        if review is None and reused_from:
            review = reviews.get(reused_from)
        visual_status = verdict(review.get("visualFidelityStatus")) if review else "UNVERIFIED"
        fine_texture_status = verdict(review.get("fineTextureStatus")) if review else "UNVERIFIED"
        motion_review_status = verdict(review.get("motionReviewStatus", review.get("motionStatus"))) if review else "UNVERIFIED"
        if review:
            review_current = record_matches_capture(capture, review.get("runtimeHashes", {}))
            review_paths = [ROOT / path for path in review.get("evidence", [])]
            recorded_hashes = {name.replace("\\", "/"): value for name, value in review.get("evidenceSha256", {}).items()}
            if recorded_hashes:
                review_current = review_current and all(path.is_file() and sha(path) == recorded_hashes.get(str(path.relative_to(ROOT)).replace("\\", "/")) for path in review_paths)
            if not review_current or not review_paths or not all(path.is_file() for path in review_paths):
                visual_status = fine_texture_status = motion_review_status = "UNVERIFIED"
            else:
                scope = review.get("reviewScope", {})
                reviewed_paths = {path.resolve() for path in review_paths}
                motion_complete = render_current and coverage and image and image.is_file() and image.resolve() in reviewed_paths and set(data["Roles"]).issubset(scope.get("roles", [])) and scope.get("framesPerRole", 0) >= data.get("Frames", 0) and scope.get("animationFramesInspected", 0) >= len(data.get("Samples", []))
                texture_complete = texture_current and texture_image and texture_image.is_file() and texture_image.resolve() in reviewed_paths and scope.get("textureViewsInspected", 0) >= 2
                if not render_current or (motion_review_status == "VERIFIED" and not motion_complete):
                    motion_review_status = "UNVERIFIED"
                if not texture_current or (fine_texture_status == "VERIFIED" and not texture_complete):
                    fine_texture_status = "UNVERIFIED"
                if not render_current or (visual_status == "VERIFIED" and (not motion_complete or not texture_complete or motion_review_status != "VERIFIED" or fine_texture_status != "VERIFIED")):
                    visual_status = "UNVERIFIED"
        checks = [row["structureStatus"], model["status"], runtime_status, render_status, visual_status, fine_texture_status, motion_review_status]
        overall = "FAIL" if "FAIL" in checks else "VERIFIED" if all(status == "VERIFIED" for status in checks) else "UNVERIFIED"
        matrix.append({"sourceKey": key, "asset": source["asset"], "assetSha256": catalog["assets"][source["asset"]]["sha256"],
                       "motionSha256": source.get("motion", {}).get("sha256"), "visualIdentity": render_identities.get(key, {}).get("Signature"),
                       "inventoryContextIdentity": group["signature"],
                       "metadata": source["metadata"], "companionBindings": row.get("companionBindings", []),
                       "aliases": row["aliases"], "companionContexts": row["companionContexts"], "status": overall,
                       "assetStatus": model["status"], "structureStatus": row["structureStatus"],
                       "runtimeStatus": runtime_status, "finiteSamplingStatus": "VERIFIED" if runtime_valid and runtime_row and runtime_row.get("runtimeSampling")=="FINITE" else "UNVERIFIED", "renderedTechnicalStatus": render_status,
                       "visualFidelityStatus": visual_status, "fineTextureStatus": fine_texture_status, "motionReviewStatus": motion_review_status,
                       "roles": row["roles"], "clips": row["clips"],
                       "failures": [*model["failures"], *row["failures"]],
                       "review": [*model["review"], *row["review"]], "runtimeReview": runtime_notes,
                       "renderReview": render_flags, "visualReview": review,
                       "evidence": {"inventory": str(args.inventory.relative_to(ROOT)),
                                    "runtime": str(args.runtime.relative_to(ROOT)) if runtime_row and args.runtime else None,
                                    "render": render_file, "renderAlias": alias_file, "renderReusedFrom": reused_from,
                                    "renderCatalogSha256": evidence[0].get("RuntimeHashes", {}).get("models.json") if evidence else None,
                                    "captureManifest": capture.get("manifest"), "captureCatalogSha256": capture["hashes"].get("models.json"),
                                    "captureRuntimeGroupsCurrent": capture["groups"], "renderDataAndRuntimeCurrent": render_current,
                                    "completeRoleFrameCoverage": coverage}})
    counts = dict(Counter(row["status"] for row in matrix))
    result = {"schemaVersion": 1, "catalogSha256": catalog_hash,
              "scope": "Every shipped source; VERIFIED requires current asset, structure, runtime, native raster and reviewed visual fidelity evidence. Missing evidence remains UNVERIFIED. Numerical finiteness and nonblank pictures do not establish visual correctness.",
              "summary": {"assets": len(catalog["assets"]), "sources": len(matrix), "visualIdentities": len(grouping["groups"]),
                          "status": counts, "renderedTechnical": dict(Counter(row["renderedTechnicalStatus"] for row in matrix)),
                          "visualFidelity": dict(Counter(row["visualFidelityStatus"] for row in matrix)),
                          "fineTexture": dict(Counter(row["fineTextureStatus"] for row in matrix)),
                          "motionReview": dict(Counter(row["motionReviewStatus"] for row in matrix))}, "sources": matrix}
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    with args.output.with_suffix(".csv").open("w", newline="", encoding="utf-8") as file:
        fields = ["sourceKey", "asset", "status", "assetStatus", "structureStatus", "runtimeStatus", "renderedTechnicalStatus", "visualFidelityStatus", "fineTextureStatus", "motionReviewStatus"]
        writer = csv.DictWriter(file, fieldnames=fields, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(matrix)
    print(json.dumps(result["summary"]))
    print(args.output)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--groups-only", action="store_true")
    parser.add_argument("--inventory", type=Path, default=ARTIFACTS / "model-audit-inventory.json")
    parser.add_argument("--runtime", type=Path, default=ARTIFACTS / "model-runtime-summary.json")
    parser.add_argument("--visual", type=Path, default=ARTIFACTS / "model-visual-audit-v2")
    parser.add_argument("--output", type=Path, default=ARTIFACTS / "model-audit-matrix.json")
    merge(parser.parse_args())
